namespace PalmHilsSemanticKernelBot.Models.ResidentialModel
{
    public class ProjectDto
    {
        public ProjectDto(int projectId, string projectName, string description, List<ZoneDto> zones)
        {
            ProjectId = projectId;
            ProjectName = projectName;
            Description = description;
            Zones = zones;
        }

        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public List<ZoneDto> Zones { get; set; } = new List<ZoneDto>();
    }
}
