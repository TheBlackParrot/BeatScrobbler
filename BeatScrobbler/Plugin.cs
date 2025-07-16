using BeatScrobbler.Config;
using IPA;
using IPA.Config.Stores;
using JetBrains.Annotations;
using SiraUtil.Zenject;
using IPAConfig = IPA.Config.Config;
using IPALogger = IPA.Logging.Logger;

namespace BeatScrobbler;

[Plugin(RuntimeOptions.DynamicInit)]
[NoEnableDisable]
[UsedImplicitly]
public class Plugin
{
    // ReSharper disable once MemberCanBePrivate.Global
    internal static IPALogger Log { get; private set; } = null!;
    
    [Init]
    public Plugin(IPAConfig cfg, IPALogger log, Zenjector zenjector)
    {
        Log = log;
        zenjector.UseLogger(Log);
        
        MainConfig config = cfg.Generated<MainConfig>();
        
        zenjector.Install(Location.App, container => { container.BindInstance(config).AsSingle(); });
        zenjector.Install<Installers.MenuInstaller>(Location.Menu);
        zenjector.UseAutoBinder();

        log.Info("Plugin loaded");
    }

    public static void DebugMessage(string message)
    {
#if DEBUG
        Log.Info(message);
#endif
    }
}
