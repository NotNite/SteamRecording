using System.Runtime.InteropServices;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace SteamRecording;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public unsafe partial struct SteamTimeline {
    [FieldOffset(0x0)] public SteamTimelineVTable* VTable;

    public void SetTimelineTooltip(string description, float timeDelta = 0f) {
        fixed (byte* descriptionPtr = Encoding.UTF8.GetBytes(description + "\0"))
        fixed (SteamTimeline* self = &this)
            this.VTable->SetTimelineTooltip(self, (char*) descriptionPtr, timeDelta);
    }

    public void ClearTimelineTooltip(float timeDelta = 0f) {
        fixed (SteamTimeline* self = &this) this.VTable->ClearTimelineTooltip(self, timeDelta);
    }

    public void SetTimelineGameMode(ETimelineGameMode mode) {
        fixed (SteamTimeline* self = &this) this.VTable->SetTimelineGameMode(self, mode);
    }

    public nint AddInstantaneousTimelineEvent(
        string title, string description, string icon, uint iconPriority,
        float startOffsetSeconds, ETimelineEventClipPriority possibleClip = ETimelineEventClipPriority.None
    ) {
        fixed (byte* titlePtr = Encoding.UTF8.GetBytes(title + "\0"))
        fixed (byte* descriptionPtr = Encoding.UTF8.GetBytes(description + "\0"))
        fixed (byte* iconPtr = Encoding.UTF8.GetBytes(icon + "\0"))
        fixed (SteamTimeline* self = &this)
            return this.VTable->AddInstantaneousTimelineEvent(self, (char*) titlePtr,
                (char*) descriptionPtr,
                (char*) iconPtr, iconPriority, startOffsetSeconds, possibleClip);
    }

    public nint AddRangeTimelineEvent(
        string title, string description, string icon, uint iconPriority,
        float startOffsetSeconds, float duration,
        ETimelineEventClipPriority possibleClip = ETimelineEventClipPriority.None
    ) {
        fixed (byte* titlePtr = Encoding.UTF8.GetBytes(title + "\0"))
        fixed (byte* descriptionPtr = Encoding.UTF8.GetBytes(description + "\0"))
        fixed (byte* iconPtr = Encoding.UTF8.GetBytes(icon + "\0"))
        fixed (SteamTimeline* self = &this)
            return this.VTable->AddRangeTimelineEvent(self, (char*) titlePtr,
                (char*) descriptionPtr,
                (char*) iconPtr, iconPriority, startOffsetSeconds, duration, possibleClip);
    }

    public nint StartRangeTimelineEvent(
        string title, string description, string icon, uint priority,
        float startOffsetSeconds, ETimelineEventClipPriority possibleClip = ETimelineEventClipPriority.None
    ) {
        fixed (byte* titlePtr = Encoding.UTF8.GetBytes(title + "\0"))
        fixed (byte* descriptionPtr = Encoding.UTF8.GetBytes(description + "\0"))
        fixed (byte* iconPtr = Encoding.UTF8.GetBytes(icon + "\0"))
        fixed (SteamTimeline* self = &this)
            return this.VTable->StartRangeTimelineEvent(self, (char*) titlePtr,
                (char*) descriptionPtr,
                (char*) iconPtr, priority, startOffsetSeconds, possibleClip);
    }

    public void UpdateRangeTimelineEvent(
        nint @event,
        string title, string description, string icon, uint priority,
        ETimelineEventClipPriority possibleClip = ETimelineEventClipPriority.None
    ) {
        fixed (byte* titlePtr = Encoding.UTF8.GetBytes(title + "\0"))
        fixed (byte* descriptionPtr = Encoding.UTF8.GetBytes(description + "\0"))
        fixed (byte* iconPtr = Encoding.UTF8.GetBytes(icon + "\0"))
        fixed (SteamTimeline* self = &this)
            this.VTable->UpdateRangeTimelineEvent(self, @event, (char*) titlePtr,
                (char*) descriptionPtr,
                (char*) iconPtr, priority, possibleClip);
    }

    public void EndRangeTimelineEvent(nint @event, float endOffsetSeconds) {
        fixed (SteamTimeline* self = &this) this.VTable->EndRangeTimelineEvent(self, @event, endOffsetSeconds);
    }

    public void RemoveTimelineEvent(nint @event) {
        fixed (SteamTimeline* self = &this) this.VTable->RemoveTimelineEvent(self, @event);
    }

    public void StartGamePhase() {
        fixed (SteamTimeline* self = &this) this.VTable->StartGamePhase(self);
    }

    public void EndGamePhase() {
        fixed (SteamTimeline* self = &this) this.VTable->EndGamePhase(self);
    }

    public void SetGamePhaseId(string phaseId) {
        fixed (byte* phaseIdPtr = Encoding.UTF8.GetBytes(phaseId + "\0"))
        fixed (SteamTimeline* self = &this)
            this.VTable->SetGamePhaseID(self, (char*) phaseIdPtr);
    }

    public void AddGamePhaseTag(
        string tagName, string tagIcon, string tagGroup, uint priority
    ) {
        fixed (byte* tagNamePtr = Encoding.UTF8.GetBytes(tagName + "\0"))
        fixed (byte* tagIconPtr = Encoding.UTF8.GetBytes(tagIcon + "\0"))
        fixed (byte* tagGroupPtr = Encoding.UTF8.GetBytes(tagGroup + "\0"))
        fixed (SteamTimeline* self = &this)
            this.VTable->AddGamePhaseTag(self, (char*) tagNamePtr, (char*) tagIconPtr,
                (char*) tagGroupPtr, priority);
    }

    public void SetGamePhaseAttribute(
        string attributeGroup, string attributeValue, uint priority
    ) {
        fixed (byte* attributeGroupPtr = Encoding.UTF8.GetBytes(attributeGroup + "\0"))
        fixed (byte* attributeValuePtr = Encoding.UTF8.GetBytes(attributeValue + "\0"))
        fixed (SteamTimeline* self = &this)
            this.VTable->SetGamePhaseAttribute(self, (char*) attributeGroupPtr, (char*) attributeValuePtr,
                priority);
    }

    public void OpenOverlayToGamePhase(string phaseId) {
        fixed (byte* phaseIdPtr = Encoding.UTF8.GetBytes(phaseId + "\0"))
        fixed (SteamTimeline* self = &this)
            this.VTable->OpenOverlayToGamePhase(self, (char*) phaseIdPtr);
    }

    public void OpenOverlayToTimelineEvent(nint @event) {
        fixed (SteamTimeline* self = &this) this.VTable->OpenOverlayToTimelineEvent(self, @event);
    }

    private static SteamTimeline* Instance;
    private delegate uint GetHSteamUserDelegate();
    private delegate nint FindOrCreateUserInterfaceDelegate(uint steamUser, char* version);

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint GetProcAddress(nint module, string procName);

    public static SteamTimeline* Get() {
        if (Instance != null) return Instance;

        var framework = Framework.Instance();
        if (framework == null) {
            Services.PluginLog.Debug("Framework was null");
            return null;
        }

        if (!framework->IsSteamApiInitialized()) {
            Services.PluginLog.Debug("Steam API not initialized");
            return null;
        }

        var handle = framework->SteamApiLibraryHandle;
        if (handle == nint.Zero) {
            Services.PluginLog.Debug("Steam API library handle was null");
            return null;
        }

        var getHSteamUserAddr = GetProcAddress(handle, "SteamAPI_GetHSteamUser");
        if (getHSteamUserAddr == nint.Zero) {
            Services.PluginLog.Debug("GetHSteamUser addr was null");
            return null;
        }

        var getHSteamUser = Marshal.GetDelegateForFunctionPointer<GetHSteamUserDelegate>(getHSteamUserAddr);
        var hSteamUser = getHSteamUser();
        Services.PluginLog.Debug("Got HSteamUser: {0}", hSteamUser);

        var findOrCreateUserInterfaceAddr = GetProcAddress(handle, "SteamInternal_FindOrCreateUserInterface");
        if (findOrCreateUserInterfaceAddr == nint.Zero) {
            Services.PluginLog.Debug("FindOrCreateUserInterface addr was null");
            return null;
        }

        var findOrCreateUserInterface = Marshal.GetDelegateForFunctionPointer<FindOrCreateUserInterfaceDelegate>(
            findOrCreateUserInterfaceAddr
        );

        fixed (byte* name = "STEAMTIMELINE_INTERFACE_V004\0"u8) {
            var @interface = findOrCreateUserInterface(hSteamUser, (char*) name);
            if (@interface == nint.Zero) {
                Services.PluginLog.Debug("Interface was null");
                return null;
            }

            var @struct = (SteamTimeline*) @interface;
            if (@struct->VTable == null) {
                Services.PluginLog.Debug("Interface VTable was null");
                return null;
            }

            Services.PluginLog.Debug("Got interface: {0}", @interface.ToString("X8"));
            Instance = (SteamTimeline*) @interface;

            return Instance;
        }
    }

    public static void Dispose() {
        Instance = null;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct SteamTimelineVTable {
    public delegate* unmanaged<SteamTimeline*, char*, float, void> SetTimelineTooltip;
    public delegate* unmanaged <SteamTimeline*, float, void> ClearTimelineTooltip;
    public delegate* unmanaged <SteamTimeline*, ETimelineGameMode, void> SetTimelineGameMode;
    public delegate* unmanaged <SteamTimeline*, char*, char*, char*, uint, float, ETimelineEventClipPriority, nint>
        AddInstantaneousTimelineEvent;
    public delegate* unmanaged <SteamTimeline*, char*, char*, char*, uint, float, float, ETimelineEventClipPriority,
        nint>
        AddRangeTimelineEvent;
    public delegate* unmanaged <SteamTimeline*, char*, char*, char*, uint, float, ETimelineEventClipPriority, nint>
        StartRangeTimelineEvent;
    public delegate* unmanaged <SteamTimeline*, nint, char*, char*, char*, uint, ETimelineEventClipPriority, void
        > UpdateRangeTimelineEvent;
    public delegate* unmanaged <SteamTimeline*, nint, float, void> EndRangeTimelineEvent;
    public delegate* unmanaged <SteamTimeline*, nint, void> RemoveTimelineEvent;
    public delegate* unmanaged <SteamTimeline*, nint, nint> DoesEventRecordingExist;
    public delegate* unmanaged <SteamTimeline*, void> StartGamePhase;
    public delegate* unmanaged <SteamTimeline*, void> EndGamePhase;
    public delegate* unmanaged <SteamTimeline*, char*, void> SetGamePhaseID;
    public delegate* unmanaged <SteamTimeline*, char*, nint> DoesGamePhaseRecordingExist;
    public delegate* unmanaged <SteamTimeline*, char*, char*, char*, uint, void> AddGamePhaseTag;
    public delegate* unmanaged <SteamTimeline*, char*, char*, uint, void> SetGamePhaseAttribute;
    public delegate* unmanaged <SteamTimeline*, char*, void> OpenOverlayToGamePhase;
    public delegate* unmanaged <SteamTimeline*, nint, void> OpenOverlayToTimelineEvent;
}

public enum ETimelineGameMode {
    Invalid = 0,
    Playing = 1,
    Staging = 2,
    Menus = 3,
    LoadingScreen = 4,
    Max
}

public enum ETimelineEventClipPriority {
    Invalid = 0,
    None = 1,
    Standard = 2,
    Featured = 3
}
