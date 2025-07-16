using System;
using System.Threading.Tasks;
using BeatScrobbler.Config;
using BeatScrobbler.Utils;
using SiraUtil.Services;
using Zenject;

namespace BeatScrobbler.Managers;

public class SongManager : IInitializable, IDisposable
{
    [Inject] private readonly MainConfig _config = null!;
    [Inject] private readonly LastFmClient _client = null!;
    [Inject] private readonly ILevelFinisher _levelFinisher = null!;

    public void Initialize()
    {
        _levelFinisher.StandardLevelDidFinish += StandardFinished;
        _levelFinisher.MultiplayerLevelDidFinish += MultiplayerFinished;
    }

    public void Dispose()
    {
        _levelFinisher.StandardLevelDidFinish -= StandardFinished;
        _levelFinisher.MultiplayerLevelDidFinish -= MultiplayerFinished;
    }

    private void StandardFinished(StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupDataSo, LevelCompletionResults results)
    {
        _ = OnLevelFinished(standardLevelScenesTransitionSetupDataSo.beatmapLevel, results);
    }
    
    private void MultiplayerFinished(MultiplayerLevelScenesTransitionSetupDataSO multiplayerLevelScenesTransitionSetupDataSo, MultiplayerResultsData results)
    {
        _ = OnLevelFinished(multiplayerLevelScenesTransitionSetupDataSo.beatmapLevel, results.localPlayerResultData.multiplayerLevelCompletionResults.levelCompletionResults);
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
            Plugin.Log.Warn($"Failed to send now playing: {beatmapLevel.songAuthorName} - {trackName}");
            Plugin.Log.Warn(e);
        }
    }
}