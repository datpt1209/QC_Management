using System.Collections.Generic;

namespace QC_Management.Models;

public partial class UserRole : BaseViewModel
{

    public int Id { get; set; }

    public string DisplayName { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
