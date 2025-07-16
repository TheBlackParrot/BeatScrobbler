using System;
using System.IO;
using System.Reflection;
using BeatScrobbler.Config;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace BeatScrobbler.Managers;

public interface ICredentialsLoader
{
    public LastFmCredentials LoadCredentials();
}

[UsedImplicitly]
public class CredentialsLoader : ICredentialsLoader
{
    private const string CREDENTIALS_LOCATION = "BeatScrobbler.credentials.json";

    public LastFmCredentials LoadCredentials()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        using Stream stream = assembly.GetManifestResourceStream(CREDENTIALS_LOCATION) ??
                              throw new Exception("Failed to load last fm credentials");
        using StreamReader reader = new(stream);
        LastFmCredentials? credentials = JsonConvert.DeserializeObject<LastFmCredentials>(reader.ReadToEnd());
        Plugin.Log.Info("Credentials loaded");
        return credentials!;
    }
}