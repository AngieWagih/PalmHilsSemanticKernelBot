namespace PalmHilsSemanticKernelBot.Models.ResidentialModel
{
    public class LocationDto
    {
        public LocationDto(int locationId, string locationName, List<ProjectDto> projects)
        {
            LocationId = locationId;
            LocationName = locationName;
            Projects = projects;
        }

        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public List<ProjectDto> Projects { get; set; } = new List<ProjectDto>();
    }
}
