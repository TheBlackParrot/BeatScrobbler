using System;
using System.Threading.Tasks;
using BeatScrobbler.Config;
using BeatScrobbler.Utils;
using JetBrains.Annotations;
using SiraUtil.Services;
using Zenject;

namespace BeatScrobbler.Managers;

[UsedImplicitly]
public class SongManager : IInitializable, IDisposable
{
    [Inject] private readonly MainConfig _config = null!;
    [Inject] private readonly LastFmClient _client = null!;
    [Inject] private readonly ILevelFinisher _levelFinisher = null!;
    [Inject] private readonly StandardLevelScenesTransitionSetupDataSO _standardLevelScenesTransitionSetupDataSo = null!;
    [Inject] private readonly MultiplayerLevelScenesTransitionSetupDataSO _multiplayerLevelScenesTransitionSetupDataSo = null!;
    
    private static BeatmapLevel? _startedBeatmapLevel;

    public void Initialize()
    {
        _levelFinisher.StandardLevelDidFinish += StandardFinished;
        _levelFinisher.MultiplayerLevelDidFinish += MultiplayerFinished;
        
        if (_standardLevelScenesTransitionSetupDataSo != null)
        {
            _standardLevelScenesTransitionSetupDataSo.beforeScenesWillBeActivatedEvent += BeforeScenesWillBeActivated;
        }

        if (_multiplayerLevelScenesTransitionSetupDataSo != null)
        {
            _multiplayerLevelScenesTransitionSetupDataSo.beforeScenesWillBeActivatedEvent += BeforeScenesWillBeActivated;
        }
    }

    public void Dispose()
    {
        _levelFinisher.StandardLevelDidFinish -= StandardFinished;
        _levelFinisher.MultiplayerLevelDidFinish -= MultiplayerFinished;
        
        if (_standardLevelScenesTransitionSetupDataSo != null)
        {
            _standardLevelScenesTransitionSetupDataSo.beforeScenesWillBeActivatedEvent -= BeforeScenesWillBeActivated;
        }

        if (_multiplayerLevelScenesTransitionSetupDataSo != null)
        {
            _multiplayerLevelScenesTransitionSetupDataSo.beforeScenesWillBeActivatedEvent -= BeforeScenesWillBeActivated;
        }
    }
    
    private void BeforeScenesWillBeActivated()
    {
        _ = OnLevelStarted();
    }
    
    private void StandardFinished(StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupDataSo, LevelCompletionResults results)
    {
        _ = OnLevelFinished(standardLevelScenesTransitionSetupDataSo.beatmapLevel, results);
    }
    
    private void MultiplayerFinished(MultiplayerLevelScenesTransitionSetupDataSO multiplayerLevelScenesTransitionSetupDataSo, MultiplayerResultsData results)
    {
        _ = OnLevelFinished(multiplayerLevelScenesTransitionSetupDataSo.beatmapLevel, results.localPlayerResultData.multiplayerLevelCompletionResults.levelCompletionResults);
    }

    private async Task OnLevelStarted()
    {
        _startedBeatmapLevel = null;
        if (_standardLevelScenesTransitionSetupDataSo != null)
        {
            _startedBeatmapLevel = _standardLevelScenesTransitionSetupDataSo.beatmapLevel;
        } else if (_multiplayerLevelScenesTransitionSetupDataSo != null)
        {
            _startedBeatmapLevel = _multiplayerLevelScenesTransitionSetupDataSo.beatmapLevel;
        }

        if (_startedBeatmapLevel == null)
        {
            Plugin.DebugMessage("startedBeatmapLevel is null");
            return;
        }

        if (string.IsNullOrEmpty(_startedBeatmapLevel.songAuthorName))
        {
            Plugin.Log.Info("Skipping song with empty author name");
            return;
        }

        string trackName = string.IsNullOrEmpty(_startedBeatmapLevel.songSubName)
            ? _startedBeatmapLevel.songName
            : $"{_startedBeatmapLevel.songName} - {_startedBeatmapLevel.songSubName}";

        try
        {
            if (_config.NowPlayingEnabled)
            {
                await _client.SendNowPlaying(
                    _startedBeatmapLevel.songAuthorName,
                    trackName
                );
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Warn($"Failed to send now playing: {_startedBeatmapLevel.songAuthorName} - {trackName}");
            Plugin.Log.Warn(e);
        }
    }

    private async Task OnLevelFinished(BeatmapLevel beatmapLevel, LevelCompletionResults results)
    {
        if (beatmapLevel.songDuration < 30 ||
            results.endSongTime < beatmapLevel.songDuration * (_config.SongScrobbleLength / 100f) ||
            string.IsNullOrEmpty(beatmapLevel.songAuthorName))
        {
            Plugin.Log.Info("Not scrobbling");
            return;
        }

        string trackName = string.IsNullOrEmpty(beatmapLevel.songSubName)
            ? beatmapLevel.songName
            : $"{beatmapLevel.songName} - {beatmapLevel.songSubName}";
        
        try
        {
            if (_config.ScrobbleEnabled)
            {
                ScrobbleResponse res = await _client.SendScrobble(
                    beatmapLevel.songAuthorName,
                    trackName,
                    DateTimeOffset.Now.ToUnixTimeSeconds()
                );
                
                if (res.Scrobbles.Attribute.Accepted != 1)
                {
                    IgnoredMessage ignoredMessage = res.Scrobbles.Data.IgnoredMessage;
                    Plugin.Log.Warn($"Scrobble was rejected with code: {ignoredMessage.Code}, message: {ignoredMessage.Text}");
                }
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Warn($"Failed to send scrobble: {beatmapLevel.songAuthorName} - {trackName}");
            Plugin.Log.Warn(e);
        }
    }
}