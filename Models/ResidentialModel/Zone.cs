namespace PalmHilsSemanticKernelBot.Models.ResidentialModel
{
    public class Zone
    {
        public Zone(int zoneId, string zoneName, double? startingPrice, string description, List<Property> properties)
        {
            ZoneId = zoneId;
            ZoneName = zoneName;
            StartingPrice = startingPrice;
            Description = description;
            Properties = properties;
        }

        public int ZoneId { get; set; }
        public string ZoneName { get; set; }
        public double? StartingPrice { get; set; }
        public string Description { get; set; }
        public List<Property> Properties { get; set; }
    }
}
