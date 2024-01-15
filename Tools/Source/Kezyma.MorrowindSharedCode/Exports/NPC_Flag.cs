namespace Kezyma.MorrowindSharedCode.Exports
{
    [Flags]
    public enum NPC_Flag : long
    {
        Female = 0x0001,
        Essential = 0x0002,
        Respawn = 0x0004,
        None = 0x0008,
        Autocalc = 0x0010,
        BloodSkel = 0x0400,
        BloodMetal = 0x0800
    }
}