using UnityEngine;

namespace BeatScrobbler.Managers;

public interface ILinksOpener
{
    public void OpenLink(string url);
}

public class LinksOpener : ILinksOpener
{
    public void OpenLink(string url)
    {
        Application.OpenURL(url);
    }
}