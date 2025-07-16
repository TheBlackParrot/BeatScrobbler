using System;
using System.Collections.Generic;
using BeatScrobbler.Config;
using BeatScrobbler.Utils;
using SiraUtil.Services;
using SiraUtil.Logging;
using Zenject;

namespace BeatScrobbler.Managers;

public class SongManager : IInitializable, IDisposable
{
    [Inject] private readonly SiraLog _log = null!;
    [Inject] private readonly MainConfig _config = null!;
    [Inject] private readonly LastFmClient _client = null!;
    [Inject] private readonly ILevelFinisher _levelFinisher = null!;
    [Inject] private readonly GameScenesManager _gameScenesManager = null!;

    private IPreviewBeatmapLevel? _lastBeatmap;
    private CurrentSongData? _songData;

    public void Initialize()
    {
        _levelFinisher.MissionLevelFinished += MissionFinished;
        _levelFinisher.StandardLevelFinished += StandardFinished;
        _gameScenesManager.transitionDidFinishEvent += TransitionFinished;
    }

    public void Dispose()
    {
        _levelFinisher.MissionLevelFinished -= MissionFinished;
        _levelFinisher.StandardLevelFinished -= StandardFinished;
        _gameScenesManager.transitionDidFinishEvent -= TransitionFinished;
    }

    private void TransitionFinished(ScenesTransitionSetupDataSO _, DiContainer container)
    {
        if (container.HasBinding<IDifficultyBeatmap>())
        {
            var beatmap = container.Resolve<IDifficultyBeatmap>();
            PlayerDataModel? player = container.Resolve<PlayerDataModel>();
            OnLevelStarted(beatmap.level, player.playerData?.practiceSettings?.startSongTime ?? 0);
        }
    }

    private void StandardFinished(LevelCompletionResults results)
    {
        OnLevelFinished(results);
    }

    private void MissionFinished(MissionCompletionResults missionResults)
    {
        OnLevelFinished(missionResults.levelCompletionResults);
    }

    // For 2 methods below check https://www.last.fm/api/scrobbling for more info
    private async void OnLevelStarted(IPreviewBeatmapLevel currentBeatmap, float offset)
    {
        _lastBeatmap = currentBeatmap;
        bool shouldBeScrobbled = _lastBeatmap.songDuration.TotalSeconds() > 30;
        var time = DateTime.Now.ToUnixTime();

        if (string.IsNullOrEmpty(_lastBeatmap.songAuthorName))
        {
            shouldBeScrobbled = false;
            _log.Debug("Skipping song with empty author name");
        }
        else
        {
            try
            {
                if (_config.NowPlayingEnabled)
                {
                    await _client.SendNowPlaying(
                        _lastBeatmap.songAuthorName,
                        _lastBeatmap.songName
                    );
                }
            }
            catch (Exception e)
            {
                _log.Warn(
                    $"Failed to send now playing: {_lastBeatmap.songAuthorName} - {_lastBeatmap.songAuthorName}");
                _log.Warn(e);
            }
        }

        _songData = new CurrentSongData(offset, shouldBeScrobbled, time);
    }

    private async void OnLevelFinished(LevelCompletionResults results)
    {
        CurrentSongData? toScrobble = _songData;

        if (toScrobble is null || _lastBeatmap is null)
        {
            _log.Warn("Unexpected null in song data");
            return;
        }

        _songData = null;

        bool notEnoughPlayed = (results.endSongTime - toScrobble.Offset) / _lastBeatmap.songDuration <
                               _config.SongScrobbleLength / 100d;

        if (!toScrobble.ShouldBeScrobbled || notEnoughPlayed) return;

        try
        {
            ScrobbleResponse? res = await _client.SendScrobble(
                _lastBeatmap.songAuthorName,
                _lastBeatmap.songName,
                toScrobble.StartTimestamp
            );

            if (res.Scrobbles.Attribute.Accepted != 1)
            {
                IgnoredMessage? ignoredMessage = res.Scrobbles.Data.IgnoredMessage;
                _log.Warn($"Scrobble was rejected with code: {ignoredMessage.Code}, message: {ignoredMessage.Text}");
                // TODO: cache failing scrobbles and re-submit them later 
            }
        }
        catch (Exception e)
        {
            _log.Warn($"Failed to scrobble: {_lastBeatmap.songAuthorName} - {_lastBeatmap.songAuthorName}");
            _log.Warn(e);
        }
    }

    private class CurrentSongData
    {
        internal readonly float Offset;
        internal readonly bool ShouldBeScrobbled;
        internal readonly long StartTimestamp;

        internal CurrentSongData(float offset, bool shouldBeScrobbled, long startTimestamp)
        {
            ShouldBeScrobbled = shouldBeScrobbled;
            StartTimestamp = startTimestamp;
            Offset = offset;
        }
    }
}