namespace AutomotiveDMS.Web.ViewModels.Users
{
    public class UserListItemViewModel
    {
        public string Id { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public bool InvitationPending { get; init; }
        public DateTime? LastLoginLocal { get; init; }
        public bool IsCurrentUser { get; init; }
        public bool IsProtected { get; init; }
    }
}
