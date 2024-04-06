using GameX.WbB.Formats.Props;

namespace GameX.WbB.Formats.Entity
{
    public struct GenericPropertyId
    {
        public GenericPropertyId(uint propertyId, PropertyType propertyType)
        {
            PropertyId = propertyId;
            PropertyType = propertyType;
        }

        public uint PropertyId { get; set; }
        public PropertyType PropertyType { get; set; }
    }
}
