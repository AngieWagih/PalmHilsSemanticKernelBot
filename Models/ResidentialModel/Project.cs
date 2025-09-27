namespace PalmHilsSemanticKernelBot.Models.ResidentialModel
{
    public class Project
    {
        public Project(int projectId, string projectName, string description, List<Zone> zones)
        {
            ProjectId = projectId;
            ProjectName = projectName;
            Description = description;
            Zones = zones;
        }

        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public List<Zone> Zones { get; set; } = new List<Zone>();
    }
}
