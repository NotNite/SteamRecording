using System;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace SteamTimelines;

public class MainWindow : Window, IDisposable {
    private readonly Configuration configuration;

    public MainWindow(Configuration configuration) : base("Steam Timelines") {
        this.configuration = configuration;
        this.Size = new Vector2(500, 350);
        this.SizeCondition = ImGuiCond.Always;
    }

    public const uint TrialAppId = 312060;

    public override void Draw() {
        ImGui.TextWrapped("Your Steam Game Recording sessions will now have extra labels. Yay! :steamhappy:");
        ImGui.TextWrapped("If you don't own the game through Steam, you'll need to do some extra configuration.");

        if (ImGui.CollapsingHeader("For Non-Steam users")) {
            using (ImRaii.PushIndent()) {
                ImGui.TextWrapped(
                    "The Steam overlay needs to be active with a real Steam game playing for the Steam Timelines API to work. The plugin will connect to the Steam API pretending to be the FFXIV free trial.");
                ImGui.TextWrapped(
                    "You will need to own the FFXIV free trial on Steam - you don't need to start it or play through it, it just needs to be in your Steam library.");
                ImGui.TextWrapped(
                    "While XIVLauncher can be added as a non-Steam game, it will not work, because the API only works for actual Steam games.");
                ImGui.TextWrapped("Restart the game after changing this.");

                var notNull = this.configuration.NonSteamAppId is not null;
                if (ImGui.Checkbox("Enable Non-Steam compatibility", ref notNull)) {
                    if (this.configuration.NonSteamAppId is not null) {
                        this.configuration.NonSteamAppId = null;
                    } else {
                        this.configuration.NonSteamAppId = TrialAppId;
                    }
                    this.configuration.Save();
                }

                if (this.configuration.NonSteamAppId is { } val) {
                    var appId = (int) val;
                    if (ImGui.InputInt("Steam App ID", ref appId)) {
                        this.configuration.NonSteamAppId = (uint) appId;
                        this.configuration.Save();
                    }
                }
            }
        }
    }

    public void Dispose() { }
}
