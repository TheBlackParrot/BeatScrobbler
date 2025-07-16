using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BeatScrobbler.Config;
using BeatScrobbler.Utils;
using Newtonsoft.Json.Linq;
using SiraUtil.Logging;
using Zenject;

namespace BeatScrobbler.Managers;

public class LastFmClient : IInitializable
{
    private const string SCROBBLER_BASE_URL = "https://ws.audioscrobbler.com/2.0/";
    private const string LAST_FM_BASE_URL = "https://www.last.fm/api";
    
    [Inject] private readonly MainConfig _config = null!;
    [Inject] private readonly ICredentialsLoader _credentialsLoader = null!;
    [Inject] private readonly ILinksOpener _linksOpener = null!;
    [Inject] private readonly SiraLog _log = null!;

    private HttpClient? _client;
    private LastFmCredentials _credentials = null!;

    public void Initialize()
    {
        _client ??= new HttpClient();

        _credentials = _credentialsLoader.LoadCredentials();
    }

    public async Task<string> GetToken()
    {
        _log.Debug("Sending token request");

        string url = $"{SCROBBLER_BASE_URL}?method=auth.gettoken&api_key={_credentials.Key}&format=json";

        HttpResponseMessage? httpResponse = await _client!.GetAsync(url);

        string? resp = await httpResponse.Content.ReadAsStringAsync();

        _log.Debug("Got response for token request");

        return CheckError<AuthToken>(resp).Token;
    }

    public void Authorize(string authToken)
    {
        _log.Debug("Sending auth request");

        string url = $"{LAST_FM_BASE_URL}/auth/?api_key={_credentials.Key}&token={authToken!}";

        _linksOpener.OpenLink(url);
    }

    public async Task<AuthSession> GetSession(string token)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>
        {
            {"method", "auth.getSession"},
            {"api_key", _credentials.Key},
            {"token", token}
        };

        string url = $"{SCROBBLER_BASE_URL}?{SignatureUtils.SignedParams(parameters, _credentials.Secret)}";

        HttpResponseMessage? httpResponse = await _client!.GetAsync(url);

        string? resp = await httpResponse.Content.ReadAsStringAsync();

        _log.Debug($"Got response for auth request {resp}");

        return CheckError<AuthSessionResponse>(resp).Session;
    }

    // Return object only for testing purpose 
    public async Task<object> SendNowPlaying(string artist, string track)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>
        {
            {"method", "track.updateNowPlaying"},
            {"artist", artist},
            {"track", track},
            {"api_key", _credentials.Key},
            {"sk", _config.SessionKey!}
        };

        string resp = await PostAsync(parameters);

        _log.Debug($"Got response for update now request {resp}");

        return CheckError<object>(resp);
    }

    // Return object only for testing purpose 
    public async Task<ScrobbleResponse> SendScrobble(string artist, string track, long timestamp)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>
        {
            {"method", "track.scrobble"},
            {"artist", artist},
            {"track", track},
            {"timestamp", timestamp.ToString()},
            {"api_key", _credentials.Key},
            {"sk", _config.SessionKey!}
        };

        string resp = await PostAsync(parameters);

        _log.Debug($"Got response for scrobble request {resp}");

        return CheckError<ScrobbleResponse>(resp);
    }

    private async Task<string> PostAsync(Dictionary<string, string> parameters)
    {
        string body = SignatureUtils.SignedParams(parameters, _credentials.Secret);

        StringContent data = new StringContent(body, Encoding.UTF8, null);

        HttpResponseMessage? httpResponse = await _client!.PostAsync(SCROBBLER_BASE_URL, data);

        return await httpResponse.Content.ReadAsStringAsync();
    }

    private static T CheckError<T>(string resp)
    {
        JObject json = JObject.Parse(resp);

        JToken? err = json.GetValue("error");

        if (err is null) return json.ToObject<T>() ?? throw new LastFmException($"Failed to deserialize {json}");

        string? msg = json.GetValue("message")?.ToString();

        throw new LastFmException(msg ?? "<Unknown http error>", err.ToObject<int>());
    }
}