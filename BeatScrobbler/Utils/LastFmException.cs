using System;

namespace BeatScrobbler.Utils;

public class LastFmException : Exception
{
    private const int NOT_AUTH_TOKEN = 14;
    private const int SERVICE_OFFLINE = 11;
    private const int SERVER_ERROR = 16;
    private readonly int? _errorCode;

    // ReSharper disable once ConvertToPrimaryConstructor
    public LastFmException(string message, int? errorCode = null) : base(message)
    {
        _errorCode = errorCode;
    }

    public bool ShouldBeReported()
    {
        return _errorCode == null ||
               _errorCode != NOT_AUTH_TOKEN && _errorCode != SERVER_ERROR && _errorCode != SERVICE_OFFLINE;
    }

    public bool TokenNotAuthorized()
    {
        return _errorCode == NOT_AUTH_TOKEN;
    }
}