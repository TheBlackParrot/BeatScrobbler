using System;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using JetBrains.Annotations;

// ReSharper disable RedundantDefaultMemberInitializer

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace BeatScrobbler.Config;

public class MainConfig
{
    public Action? OnChanged;

    public string? SessionName { get; set; }

    public string? SessionKey { get; set; }

    public int SongScrobbleLength { get; set; } = 50;

    public bool ScrobbleEnabled { get; set; } = true;
    
    public bool NowPlayingEnabled { get; set; } = false;

    public bool IsAuthorized()
    {
        return SessionName is not null && SessionKey is not null;
    }

    [UsedImplicitly]
    public void Changed()
    {
        OnChanged?.Invoke();
    }
}