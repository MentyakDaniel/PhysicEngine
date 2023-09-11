using System.Runtime.InteropServices;

namespace PhysicEngine.Collision.ContactSystem
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ContactId
    {
        [FieldOffset(0)]
        public ContactFeature ContactFeature;

        [FieldOffset(0)]
        public uint Key;
    }
}
