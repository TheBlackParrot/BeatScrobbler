using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using JetBrains.Annotations;
using Zenject;

namespace BeatScrobbler.UI;

[UsedImplicitly]
public class MenuButtonHandler : IInitializable, IDisposable
{
    private readonly ScrobblerFlowCoordinator _flowCoordinator;
    private readonly MenuButton _menuButton;

    public MenuButtonHandler(ScrobblerFlowCoordinator flowCoordinator)
    {
        _flowCoordinator = flowCoordinator;
        _menuButton = new MenuButton("BeatScrobbler", OnClick);
    }

    public void Dispose()
    {
        MenuButtons.Instance.UnregisterButton(_menuButton);
    }

    public void Initialize()
    {
        MenuButtons.Instance.RegisterButton(_menuButton);
    }

    private void OnClick()
    {
        BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(_flowCoordinator);
    }
}
