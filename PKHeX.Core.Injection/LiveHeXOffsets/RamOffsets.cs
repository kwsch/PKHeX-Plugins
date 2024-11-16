using static PKHeX.Core.Injection.LiveHeXVersion;

namespace PKHeX.Core.Injection;

public static class RamOffsets
{
    public static LiveHeXVersion[] GetValidVersions(SaveFile sf) => sf switch
    {
        SAV9SV =>
        [
            SV_v101, SV_v110, SV_v120, SV_v130, SV_v131, SV_v132,
            SV_v201, SV_v202,
            SV_v300, SV_v301,
        ],
        SAV8LA => [LA_v100, LA_v101, LA_v102, LA_v111],
        SAV8BS =>
        [
            BD_v100, SP_v100,
            BD_v110, SP_v110, BD_v111, SP_v111, BDSP_v112, BDSP_v113,
            BDSP_v120,
            BD_v130, SP_v130,
        ],
        SAV8SWSH => [SWSH_v111, SWSH_v121, SWSH_v132],
        SAV7b => [LGPE_v102],
        SAV7USUM => [UM_v120, US_v120],
        SAV7SM => [SM_v120],
        SAV6AO => [ORAS_v140],
        SAV6XY => [XY_v150],
        _ => [SWSH_v132],
    };

    public static bool IsLiveHeXSupported(SaveFile sav) => sav switch
    {
        SAV9SV => true,
        SAV8LA => true,
        SAV8BS => true,
        SAV8SWSH => true,
        SAV7b => true,
        SAV7USUM => true,
        SAV7SM => true,
        SAV6AO => true,
        SAV6XY => true,
        _ => false,
    };

    public static bool IsSwitchTitle(SaveFile sav) => sav switch
    {
        SAV7USUM => false,
        SAV7SM => false,
        SAV6AO => false,
        SAV6XY => false,
        _ => true,
    };

    public static ICommunicator GetCommunicator(SaveFile sav, InjectorCommunicationType ict)
    {
        bool isSwitch = IsSwitchTitle(sav);
        return isSwitch ? GetSwitchInterface(ict) : new NTRClient();
    }

    public static int GetB1S1Offset(LiveHeXVersion lv) => lv switch
    {
        LGPE_v102 => 0x533675B0,
        SWSH_v111 => 0x4293D8B0,
        SWSH_v121 => 0x4506D890,
        SWSH_v132 => 0x45075880,
        UM_v120 => 0x33015AB0,
        US_v120 => 0x33015AB0,
        SM_v120 => 0x330D9838,
        ORAS_v140 => 0x8C9E134,
        XY_v150 => 0x8C861C8,
        _ => 0x0,
    };

    public static int GetSlotSize(LiveHeXVersion lv) => lv switch
    {
        LA_v111 => 360,
        LA_v102 => 360,
        LA_v101 => 360,
        LA_v100 => 360,
        LGPE_v102 => 260,
        UM_v120 => 232,
        US_v120 => 232,
        SM_v120 => 232,
        ORAS_v140 => 232,
        XY_v150 => 232,
        _ => 344,
    };

    public static int GetGapSize(LiveHeXVersion lv) => lv switch
    {
        LGPE_v102 => 380,
        _ => 0,
    };

    public static int GetSlotCount(LiveHeXVersion lv) => lv switch
    {
        LGPE_v102 => 25,
        _ => 30,
    };

    public static int GetTrainerBlockSize(LiveHeXVersion lv) => lv switch
    {
        BD_v130 => 0x50,
        SP_v130 => 0x50,
        BDSP_v120 => 0x50,
        BDSP_v113 => 0x50,
        BDSP_v112 => 0x50,
        BD_v111 => 0x50,
        BD_v110 => 0x50,
        BD_v100 => 0x50,
        SP_v111 => 0x50,
        SP_v110 => 0x50,
        SP_v100 => 0x50,
        LGPE_v102 => 0x168,
        SWSH_v111 => 0x110,
        SWSH_v121 => 0x110,
        SWSH_v132 => 0x110,
        UM_v120 => 0xC0,
        US_v120 => 0xC0,
        SM_v120 => 0xC0,
        ORAS_v140 => 0x170,
        XY_v150 => 0x170,
        _ => 0x110,
    };

    public static uint GetTrainerBlockOffset(LiveHeXVersion lv) => lv switch
    {
        LGPE_v102 => 0x53582030,
        SWSH_v111 => 0x42935E48,
        SWSH_v121 => 0x45061108,
        SWSH_v132 => 0x45068F18,
        UM_v120 => 0x33012818,
        US_v120 => 0x33012818,
        SM_v120 => 0x330D67D0,
        ORAS_v140 => 0x8C81340,
        XY_v150 => 0x8C79C3C,
        _ => 0x0,
    };

    public static bool WriteBoxData(LiveHeXVersion lv) => lv switch
    {
        LA_v111 => true,
        LA_v102 => true,
        LA_v101 => true,
        LA_v100 => true,
        UM_v120 => true,
        US_v120 => true,
        SM_v120 => true,
        ORAS_v140 => true,
        XY_v150 => true,
        _ => false,
    };

    // relative to: PlayerWork.SaveData_TypeInfo
    public static (string, int) BoxOffsets(LiveHeXVersion lv) => lv switch
    {
        BD_v130 => ("[[[[main+4C64DC0]+B8]+10]+A0]+20", 40),
        SP_v130 => ("[[[[main+4E7BE98]+B8]+10]+A0]+20", 40),
        BDSP_v120 => ("[[[[main+4E36C58]+B8]+10]+A0]+20", 40),
        BDSP_v113 => ("[[[[main+4E59E60]+B8]+10]+A0]+20", 40),
        BDSP_v112 => ("[[[[main+4E34DD0]+B8]+10]+A0]+20", 40),
        BD_v111 => ("[[[[main+4C1DCF8]+B8]+10]+A0]+20", 40),
        BD_v110 => ("[[[main+4E27C50]+B8]+170]+20", 40),
        BD_v100 => ("[[[main+4C0ABD8]+520]+C0]+5E0", 40),
        SP_v111 => ("[[[[main+4E34DD0]+B8]+10]+A0]+20", 40),
        SP_v110 => ("[[[main+4E27C50]+B8]+170]+20", 40), // untested
        SP_v100 => ("[[[main+4C0ABD8]+520]+C0]+5E0", 40), // untested
        _ => (string.Empty, 0),
    };

    public static object? GetOffsets(LiveHeXVersion lv) => lv switch
    {
        SWSH_v132 => Offsets8.Rigel2,
        _ => null,
    };

    private static ICommunicator GetSwitchInterface(InjectorCommunicationType ict) => ict switch
    {
        // No conditional expression possible
        InjectorCommunicationType.SocketNetwork => new SysBotMini(),
        InjectorCommunicationType.USB => new UsbBotMini(),
        _ => new SysBotMini(),
    };
}