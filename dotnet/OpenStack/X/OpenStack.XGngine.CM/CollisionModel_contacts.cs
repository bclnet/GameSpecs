using CmHandle = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.CM
{
    partial class CM
    {
        partial class CollisionModelManagerLocal
        {
            // stores all contact points of the trm with the model, returns the number of 
            public int Contacts(ContactInfo contacts, int maxContacts, in Vector3 start, in Vector6 dir, in float depth, in TraceModel trm, in Matrix3x3 trmAxis, int contentMask, CmHandle model, in Vector3 modelOrigin, in Matrix3x3 modelAxis)
            {
                // same as Translation but instead of storing the first collision we store all collisions as contacts
                this.getContacts = true;
                this.contacts = contacts;
                this.maxContacts = maxContacts;
                this.numContacts = 0;
                var end = start + dir.SubVec3(0) * depth;
                this.Translation(out var results, start, end, trm, trmAxis, contentMask, model, modelOrigin, modelAxis);
                if (dir.SubVec3(1).LengthSqr != 0f) { } // FIXME: rotational contacts
                this.getContacts = false;
                this.maxContacts = 0;

                return this.numContacts;
            }
        }
    }
}