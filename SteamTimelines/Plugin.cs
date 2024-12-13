using Dalamud.Interface.Windowing;
using Dalamud.Plugin;

namespace SteamTimelines;

public unsafe class Plugin : IDalamudPlugin {
    private readonly Configuration configuration;
    private readonly EventDispatcher eventDispatcher;
    private readonly WindowSystem windowSystem;
    private readonly MainWindow mainWindow;

    public Plugin(IDalamudPluginInterface pluginInterface) {
        pluginInterface.Create<Services>();

        this.configuration = Services.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        this.configuration.Save();

        // Register the Steam API instance if we're a non-Steam service account
        // This needs to be done before DX11 gets set up for the overlay
        // Doesn't matter for actual Steam service accounts, Framework will have the handle
        if (this.configuration.NonSteamAppId is not null) SteamTimeline.Get(this.configuration.NonSteamAppId);

        this.eventDispatcher = new EventDispatcher();
        this.windowSystem = new WindowSystem(pluginInterface.InternalName);
        this.windowSystem.AddWindow(this.mainWindow = new MainWindow(this.configuration));

        Services.PluginInterface.UiBuilder.Draw += this.Draw;
        Services.PluginInterface.UiBuilder.OpenConfigUi += this.OpenConfigUi;
    }

    private void Draw() {
        this.windowSystem.Draw();
    }

    private void OpenConfigUi() {
        this.mainWindow.IsOpen = true;
    }

    public void Dispose() {
        Services.PluginInterface.UiBuilder.OpenConfigUi -= this.OpenConfigUi;
        Services.PluginInterface.UiBuilder.Draw -= this.Draw;

        this.mainWindow.Dispose();
        this.windowSystem.RemoveAllWindows();
        this.eventDispatcher.Dispose();
        this.configuration.Save();
        SteamTimeline.Dispose();
    }
}
