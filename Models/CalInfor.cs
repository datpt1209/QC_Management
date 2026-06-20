using System;
using System.Collections.Generic;
using QC_Management.Models;

namespace QC_Management;

public partial class CalInfor
{
    public int Id { get; set; }

    public string CalLot { get; set; } = null!;

    public DateTime ExpirationDate { get; set; }

    public int IdCalType { get; set; }

    // New status flag for CalInfor. When changed it should propagate to related CalDetails.
    public bool Status { get; set; } = true;

    public virtual ICollection<CalDetail> CalDetails { get; set; } = new List<CalDetail>();

    public virtual CalType IdCalTypeNavigation { get; set; } = null!;
}
