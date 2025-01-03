namespace AutoMapper.OData.EFCore.Tests.AirVinylModel
{
    public class DynamicPropertyModel
    {
        public string Key { get; set; }
        public string SerializedValue { get; set; }

        public object Value { get; set; }

        public int VinylRecordId { get; set; }
        public virtual VinylRecordModel VinylRecord { get; set; }

    }
}
