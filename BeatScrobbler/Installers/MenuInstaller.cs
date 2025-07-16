using BeatScrobbler.Config;
using BeatScrobbler.Managers;
using BeatScrobbler.UI;
using SiraUtil;
using SiraUtil.Logging;
using Zenject;

namespace BeatScrobbler.Installers;

public class MenuInstaller : Installer
{
    [Inject] private readonly SiraLog _log = null!;

    public override void InstallBindings()
    {
        InstallUI();
        InstallScrobbler();
    }

    private void InstallUI()
    {
        Container.Bind<ScrobblerConfigView>().FromNewComponentAsViewController().AsSingle();
        Container.Bind<NotAuthorizedView>().FromNewComponentAsViewController().AsSingle();
        Container.Bind<AuthorizedView>().FromNewComponentAsViewController().AsSingle();
        Container.Bind<ScrobblerFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
        Container.BindInterfacesAndSelfTo<MenuButtonHandler>().AsSingle();

        _log.Debug("Finished setting up UI");
    }

    private void InstallScrobbler()
    {
        MainConfig? cfg = Container.Resolve<MainConfig>();

        Container.BindInterfacesAndSelfTo<CredentialsLoader>().AsSingle();
        Container.BindInterfacesAndSelfTo<LinksOpener>().AsSingle();
        Container.BindInterfacesAndSelfTo<LastFmClient>().AsSingle();

        if (!cfg.IsAuthorized())
        {
            _log.Warn("Client is not authorized, scrobbler is disabled.");
            return;
        }
        Container.BindInterfacesAndSelfTo<SongManager>().AsSingle();

        _log.Info("Setup if finished.");
    }
}