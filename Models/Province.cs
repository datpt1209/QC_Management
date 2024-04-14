using System.Collections.Generic;

namespace QC_Management.Models;

public partial class Province
{
    public int ProvinceId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<District> Districts { get; set; } = new List<District>();
}
