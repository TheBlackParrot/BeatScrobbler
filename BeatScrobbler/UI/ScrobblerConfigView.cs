﻿using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatScrobbler.Config;
using UnityEngine;
using Zenject;

namespace BeatScrobbler.UI;

[ViewDefinition("BeatScrobbler.UI.Views.config-view.bsml")]
[HotReload(RelativePathToLayout = @"\Views\config-view.bsml")]
public class ScrobblerConfigView : AbstractView
{
    public event Action<bool>? AuthClicked;

    [Inject] private readonly MainConfig _config = null!;
    
    public string PercentageFormatter(int x) => $"{x:N0}%";

    private bool _authorized;

    [UIValue("authorized")]
    public bool Authorized
    {
        get => _authorized;
        set
        {
            _authorized = value;
            NotifyPropertyChanged();
            NotifyPropertyChanged(nameof(AuthText));
            NotifyPropertyChanged(nameof(AuthColor));
        }
    }

    [UIValue("scrobble-enable")]
    public bool ScrobbleEnabled
    {
        get => _config.ScrobbleEnabled;
        set
        {
            // Without it BSML initialization would trigger _restartRequired and I don't want it.
            if (_config.ScrobbleEnabled != value)
            {
                _config.ScrobbleEnabled = value;
            }
        }
    }
    
    [UIValue("now-playing-enable")]
    public bool NowPlayingEnabled
    {
        get => _config.NowPlayingEnabled;
        set
        {
            if (_config.NowPlayingEnabled != value)
            {
                _config.NowPlayingEnabled = value;
            }
        }
    }

    [UIValue("scrobble-percentage")]
    public int ScrobblePercentage
    {
        get => _config.SongScrobbleLength;
        set
        {
            if (_config.SongScrobbleLength != value)
            {
                _config.SongScrobbleLength = value;
            }
        }
    }

    [UIValue("auth-text")]
    public string AuthText => Authorized ? $"Logged in as {_config.SessionName}" : "Not authorized";

    [UIValue("auth-color")] public string AuthColor => Authorized ? "#00ff00" : "#ff0000";

    public void Initialize()
    {
        Authorized = _config.IsAuthorized();
    }

    [UIAction("clicked-show-auth-button")]
    protected void ClickedShow()
    {
        AuthClicked?.Invoke(Authorized);
    }

    [UIAction("clicked-github")]
    protected void ClickedGithub()
    {
        ShowInfoModal(() => Application.OpenURL("https://github.com/TheBlackParrot/BeatScrobbler"));
    }
}
