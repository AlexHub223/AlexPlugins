using UnityEngine;

namespace BlueCrystalCooking
{
    public class FreezingTrayObject
    {
        public uint instanceID;
        public ulong owner;
        public ulong group;
        public float angle_x;
        public float angle_y;
        public float angle_z;
        public Vector3 pos;
        public int freezingSeconds;

        public FreezingTrayObject(uint instanceID, Vector3 pos, ulong owner, ulong group, float angle_x, float angle_y, float angle_z, int freezingSeconds)
        {
            this.instanceID = instanceID;
            this.pos = pos;
            this.owner = owner;
            this.group = group;
            this.angle_x = angle_x;
            this.angle_y = angle_y;
            this.angle_z = angle_z;
            this.freezingSeconds = freezingSeconds;
        }
    }
}
