using BeatScrobbler.Config;
using BeatScrobbler.Managers;
using BeatScrobbler.UI;
using JetBrains.Annotations;
using Zenject;

namespace BeatScrobbler.Installers;

[UsedImplicitly]
public class MenuInstaller : Installer
{
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

        Plugin.Log.Debug("Finished setting up UI");
    }

    private void InstallScrobbler()
    {
        MainConfig? cfg = Container.Resolve<MainConfig>();

        Container.BindInterfacesAndSelfTo<CredentialsLoader>().AsSingle();
        Container.BindInterfacesAndSelfTo<LastFmClient>().AsSingle();

        if (!cfg.IsAuthorized())
        {
            Plugin.Log.Warn("Client is not authorized, scrobbler is disabled");
            return;
        }
        
        Container.BindInterfacesAndSelfTo<SongManager>().AsSingle();
        Plugin.Log.Info("Setup finished");
    }
}