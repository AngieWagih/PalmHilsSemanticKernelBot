namespace PalmHilsSemanticKernelBot.Models
{
    public class CustomerDto
    {
        public CustomerDto(int id, string? name, string? email, string? phone, bool ownProperty, bool isBlocked)
        {
            Id = id;
            Name = name;
            Email = email;
            Phone = phone;
            OwnProperty = ownProperty;
            IsBlocked = isBlocked;
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public bool OwnProperty { get; set; }
        public bool IsBlocked { get; set; }
    }
}
