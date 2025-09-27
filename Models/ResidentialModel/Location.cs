namespace PalmHilsSemanticKernelBot.Models.ResidentialModel
{
    public class Location
    {
        public Location(int locationId, string locationName, List<Project> projects)
        {
            LocationId = locationId;
            LocationName = locationName;
            Projects = projects;
        }

        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public List<Project> Projects { get; set; } = new List<Project>();
    }
}
