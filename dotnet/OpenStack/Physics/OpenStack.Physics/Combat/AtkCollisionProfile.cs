using ACE.Entity.Enum;
using ACE.Server.Physics.Collision;

namespace OpenStack.Physics.Combat
{
    public class AtkCollisionProfile : ObjCollisionProfile
    {
        public int Part;
        public Quadrant Location;

        public AtkCollisionProfile() { }

        public AtkCollisionProfile(uint id, int part, Quadrant location)
        {
            ID = id;
            Part = part;
            Location = location;
        }
    }
}
