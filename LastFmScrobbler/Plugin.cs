using IPA;
using IPA.Config.Stores;
using IPA.Loader;
using IPA.Logging;
using LastFmScrobbler.Config;
using SiraUtil;
using SiraUtil.Zenject;
using IPAConfig = IPA.Config.Config;

namespace LastFmScrobbler
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        private readonly Logger _log;

        [Init]
        public Plugin(IPAConfig cfg, Logger log, Zenjector injector, PluginMetadata metadata)
        {
            _log = log;

            var config = cfg.Generated<MainConfig>();
            config.Version = metadata.HVersion;

            injector.UseLogger(log);
            injector.Install(Location.App, Container => { Container.BindInstance(config).AsSingle(); });
            injector.Install<Installers.MenuInstaller>(Location.Menu);
            injector.UseAutoBinder();

            _log.Debug("Finished plugin initialization");
        }

        [OnEnable]
        [OnDisable]
        public void Nop()
        {
        }
    }
}