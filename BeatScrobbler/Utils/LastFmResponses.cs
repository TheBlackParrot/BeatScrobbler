﻿using Newtonsoft.Json;

namespace BeatScrobbler.Utils;

public class AuthToken
{
    [JsonProperty(PropertyName = "token")] public string Token { get; set; } = null!;
}

public class AuthSessionResponse
{
    [JsonProperty(PropertyName = "session")]
    public AuthSession Session { get; set; } = null!;
}

public class AuthSession
{
    [JsonProperty(PropertyName = "name")] public string Name { get; set; } = null!;

    [JsonProperty(PropertyName = "key")] public string Key { get; set; } = null!;
}

public class ScrobbleResponse
{
    [JsonProperty(PropertyName = "scrobbles")]
    public Scrobbles Scrobbles { get; set; } = null!;
}

public class Scrobbles
{
    [JsonProperty(PropertyName = "@attr")] public ScrobbleAttribute Attribute { get; set; } = null!;

    [JsonProperty(PropertyName = "scrobble")]
    public ScrobbleData Data { get; set; } = null!;
}

public class ScrobbleAttribute
{
    [JsonProperty(PropertyName = "accepted")]
    public int Accepted { get; set; }

    [JsonProperty(PropertyName = "ignored")]
    public int Ignored { get; set; }
}

public class ScrobbleData
{
    [JsonProperty(PropertyName = "ignoredMessage")]
    public IgnoredMessage IgnoredMessage { get; set; } = null!;
}

public class IgnoredMessage {
    [JsonProperty(PropertyName = "code")]
    public int Code { get; set; }
    
    [JsonProperty(PropertyName = "#text")]
    public string Text { get; set; } = null!;
}
