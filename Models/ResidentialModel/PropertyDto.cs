namespace PalmHilsSemanticKernelBot.Models.ResidentialModel
{
    public class PropertyDto
    {
        public PropertyDto(int propertyId, string propertyCode, string zoneName, string projectName, string locationName, PropertyTypeName propertyTypeName, double? priceMin, double? priceMax, PropertyStatus propertyStatus, int? bedrooms, int? bathrooms, double? areaSqft, int? floorNumber, PaymentPlanDto paymentPlan)
        {
            PropertyId = propertyId;
            PropertyCode = propertyCode;
            ZoneName = zoneName;
            ProjectName = projectName;
            LocationName = locationName;
            PropertyTypeName = propertyTypeName;
            PriceMin = priceMin;
            PriceMax = priceMax;
            PropertyStatus = propertyStatus;
            Bedrooms = bedrooms;
            Bathrooms = bathrooms;
            AreaSqft = areaSqft;
            FloorNumber = floorNumber;
            PaymentPlan = paymentPlan;
        }

        public int PropertyId { get; set; }
        public string PropertyCode { get; set; }
        public string ZoneName { get; set; }
        public string ProjectName { get; set; }
        public string LocationName { get; set; }
        public PropertyTypeName PropertyTypeName { get; set; }
        public double? PriceMin { get; set; }
        public double? PriceMax { get; set; }
        public PropertyStatus PropertyStatus { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public double? AreaSqft { get; set; }
        public int? FloorNumber { get; set; }
        public PaymentPlanDto PaymentPlan { get; set; }
    }
    public enum PropertyTypeName
    {
        Apartment = 1,
        Villa = 2,
        PentaHouse = 3,
        TownHome = 4
    }
    public enum PropertyStatus
    {
        UnderConstruction = 1,
        Operating = 2
    }


}
