using System;
using System.IO;
using System.Reflection;
using BeatScrobbler.Config;
using Newtonsoft.Json;
using SiraUtil.Logging;
using Zenject;

namespace BeatScrobbler.Managers;

public interface ICredentialsLoader
{
    public LastFmCredentials LoadCredentials();
}

public class CredentialsLoader : ICredentialsLoader
{
    private const string CREDENTIALS_LOCATION = "BeatScrobbler.credentials.json";

    [Inject] private readonly SiraLog _log = null!;

    public LastFmCredentials LoadCredentials()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        using Stream stream = assembly.GetManifestResourceStream(CREDENTIALS_LOCATION) ??
                              throw new Exception("Failed to load last fm credentials");
        using StreamReader reader = new(stream);
        LastFmCredentials? credentials = JsonConvert.DeserializeObject<LastFmCredentials>(reader.ReadToEnd());
        _log.Debug("Credentials loaded");
        return credentials!;
    }
}