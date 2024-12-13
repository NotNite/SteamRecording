using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace SteamRecording;

public unsafe class Plugin : IDalamudPlugin {
    private uint? lastHealth;

    public Plugin(IDalamudPluginInterface pluginInterface) {
        pluginInterface.Create<Services>();

        Services.DutyState.DutyStarted += this.DutyStarted;
        Services.DutyState.DutyWiped += this.DutyWiped;
        Services.DutyState.DutyCompleted += this.DutyCompleted;
        Services.Condition.ConditionChange += this.ConditionChange;
        Services.ClientState.Login += this.Login;
        Services.ClientState.Logout += this.Logout;
        Services.Framework.Update += this.Update;
    }

    private string GetZoneString() {
        try {
            return Services.DataManager.GetExcelSheet<TerritoryType>()
                .GetRow(Services.ClientState.TerritoryType).PlaceName.Value.Name.ExtractText();
        } catch {
            return string.Empty;
        }
    }

    private void DutyStarted(object? sender, ushort e) {
        Services.Framework.RunOnTick(() => {
            var tl = SteamTimeline.Get();
            if (tl != null) {
                tl->AddInstantaneousTimelineEvent("Duty Started", this.GetZoneString(), "steam_combat", 0, 0, ETimelineEventClipPriority.Featured);
            }
        });
    }

    private void DutyWiped(object? _, ushort e) {
        Services.Framework.RunOnTick(() => {
            var tl = SteamTimeline.Get();
            if (tl != null) {
                tl->AddInstantaneousTimelineEvent("Duty Wipe", this.GetZoneString(), "steam_x", 0, 0);
            }
        });
    }

    private void DutyCompleted(object? _, ushort e) {
        Services.Framework.RunOnTick(() => {
            var tl = SteamTimeline.Get();
            if (tl != null) {
                tl->AddInstantaneousTimelineEvent("Duty Completed", this.GetZoneString(), "steam_crown", 0, 0, ETimelineEventClipPriority.Featured);
            }
        });
    }

    private void ConditionChange(ConditionFlag flag, bool value) {
        if (flag is not ConditionFlag.BetweenAreas) return;
        Services.Framework.RunOnTick(() => {
            var tl = SteamTimeline.Get();
            if (tl != null) {
                tl->SetTimelineGameMode(value ? ETimelineGameMode.LoadingScreen : ETimelineGameMode.Playing);
            }
        });
    }

    private void Login() {
        Services.Framework.RunOnTick(() => {
            var tl = SteamTimeline.Get();
            if (tl != null) {
                tl->SetTimelineGameMode(ETimelineGameMode.Playing);
            }
        });
    }

    private void Logout(int type, int code) {
        Services.Framework.RunOnTick(() => {
            var tl = SteamTimeline.Get();
            if (tl != null) {
                tl->SetTimelineGameMode(ETimelineGameMode.LoadingScreen);
            }
        });
    }

    private void Update(IFramework framework) {
        var health = Services.ClientState.LocalPlayer?.CurrentHp;
        if (health is not null) {
            if (this.lastHealth is null) {
                this.lastHealth = health;
            } else if (this.lastHealth != health) {
                var capturedHealth = this.lastHealth.Value;

                Services.Framework.RunOnTick(() => {
                    var tl = SteamTimeline.Get();
                    if (tl != null) {
                        var zone = this.GetZoneString();
                        if (health == 0) {
                            tl->AddInstantaneousTimelineEvent("Death", zone, "steam_death", 0, 0);
                        } /*else if (capturedHealth == 0) {
                            tl->AddInstantaneousTimelineEvent("Raise", zone, "steam_heart", 0, 0);
                        }*/
                    }
                });

                this.lastHealth = health.Value;
            }
        }
    }

    public void Dispose() {
        Services.DutyState.DutyStarted -= this.DutyStarted;
        Services.DutyState.DutyWiped -= this.DutyWiped;
        Services.DutyState.DutyCompleted -= this.DutyCompleted;
        Services.Condition.ConditionChange -= this.ConditionChange;
        Services.ClientState.Login -= this.Login;
        Services.ClientState.Logout -= this.Logout;
        Services.Framework.Update -= this.Update;

        SteamTimeline.Dispose();
    }
}
