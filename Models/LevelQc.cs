using System.Collections.Generic;

namespace QC_Management.Models;

public partial class LevelQc : BaseViewModel
{

    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<ControlInfoDetail> ControlInfoDetails { get; set; } = new List<ControlInfoDetail>();

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();
}
