using System.Collections.Generic;

namespace QC_Management.Models;

public partial class User : BaseViewModel
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public int Role { get; set; }

    public string DisplayName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();

    public virtual UserRole RoleNavigation { get; set; } = null!;
    public User(string userName)
    {
        this.UserName = userName;
    }
    public User() { }
}