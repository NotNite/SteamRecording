using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace SteamTimelines;

public unsafe class EventDispatcher {
    private uint? lastHealth;

    public EventDispatcher() {
        Services.DutyState.DutyStarted += this.DutyStarted;
        Services.DutyState.DutyWiped += this.DutyWiped;
        Services.DutyState.DutyCompleted += this.DutyCompleted;
        Services.Condition.ConditionChange += this.ConditionChange;
        Services.ClientState.Login += this.Login;
        Services.ClientState.Logout += this.Logout;
        Services.Framework.Update += this.Update;
        Services.ClientState.TerritoryChanged += this.TerritoryChanged;
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
                tl->AddInstantaneousTimelineEvent("Duty Started", this.GetZoneString(), "steam_combat", 0, 0,
                    ETimelineEventClipPriority.Featured);
            }
        });
    }

    private void DutyWiped(object? _, ushort e) {
        Services.Framework.RunOnTick(() => {
            var tl = SteamTimeline.Get();
            if (tl != null) {
                tl->AddInstantaneousTimelineEvent("Duty Wipe", this.GetZoneString(), "steam_x");
            }
        });
    }

    private void DutyCompleted(object? _, ushort e) {
        Services.Framework.RunOnTick(() => {
            var tl = SteamTimeline.Get();
            if (tl != null) {
                tl->AddInstantaneousTimelineEvent("Duty Completed", this.GetZoneString(), "steam_crown", 0, 0,
                    ETimelineEventClipPriority.Featured);
            }
        });
    }

    private void ConditionChange(ConditionFlag flag, bool value) {
        switch (flag) {
            case ConditionFlag.BetweenAreas: {
                Services.Framework.RunOnTick(() => {
                    var tl = SteamTimeline.Get();
                    if (tl != null) {
                        tl->SetTimelineGameMode(value ? ETimelineGameMode.LoadingScreen : ETimelineGameMode.Playing);
                    }
                });
                break;
            }

            case ConditionFlag.BoundByDuty: {
                Services.Framework.RunOnTick(() => {
                    var tl = SteamTimeline.Get();
                    if (tl != null) {
                        if (value) {
                            tl->StartGamePhase();
                            tl->AddGamePhaseTag(this.GetZoneString(), "steam_attack", "Duty");
                        } else {
                            tl->EndGamePhase();
                        }
                    }
                });
                break;
            }
        }
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
                //var capturedHealth = this.lastHealth.Value;

                Services.Framework.RunOnTick(() => {
                    var tl = SteamTimeline.Get();
                    if (tl != null) {
                        var zone = this.GetZoneString();
                        if (health == 0) {
                            tl->AddInstantaneousTimelineEvent("Death", zone, "steam_death");
                        } /*else if (capturedHealth == 0) {
                            tl->AddInstantaneousTimelineEvent("Raise", zone, "steam_heart", 0, 0);
                        }*/
                    }
                });

                this.lastHealth = health.Value;
            }
        }
    }

    private void TerritoryChanged(ushort obj) {
        Services.Framework.RunOnTick(() => {
            var tl = SteamTimeline.Get();
            if (tl != null) {
                tl->SetTimelineTooltip(this.GetZoneString());
            }
        });
    }

    public void Dispose() {
        Services.DutyState.DutyStarted -= this.DutyStarted;
        Services.DutyState.DutyWiped -= this.DutyWiped;
        Services.DutyState.DutyCompleted -= this.DutyCompleted;
        Services.Condition.ConditionChange -= this.ConditionChange;
        Services.ClientState.Login -= this.Login;
        Services.ClientState.Logout -= this.Logout;
        Services.Framework.Update -= this.Update;
    }
}
