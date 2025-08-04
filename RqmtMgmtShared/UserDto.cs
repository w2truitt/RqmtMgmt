namespace RqmtMgmtShared
{
    public class UserDto
    {
        public int Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
