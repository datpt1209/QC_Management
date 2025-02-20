using System;
using System.Collections.Generic;

namespace QC_Management;

public partial class CalType
{
    public int Id { get; set; }

    public string CalTypeName { get; set; } = null!;

    public virtual ICollection<CalInfor> CalInfors { get; set; } = new List<CalInfor>();
}
