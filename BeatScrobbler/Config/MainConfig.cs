﻿using System;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using SiraUtil.Converters;
using Version = Hive.Versioning.Version;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace BeatScrobbler.Config;

public class MainConfig
{
    public Action? OnChanged;

    public virtual string? SessionName { get; set; } = null;

    public virtual string? SessionKey { get; set; } = null;

    public virtual int SongScrobbleLength { get; set; } = 50;

    public virtual bool ScrobbleEnabled { get; set; } = true;
    
    public virtual bool NowPlayingEnabled { get; set; } = false;

    public bool IsAuthorized()
    {
        return SessionName is not null && SessionKey is not null;
    }

    public virtual void Changed()
    {
        OnChanged?.Invoke();
    }
}