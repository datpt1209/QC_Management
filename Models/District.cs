namespace QC_Management.Models;

public partial class District
{
    public int DistrictId { get; set; }

    public int ProvinceId { get; set; }

    public string Name { get; set; } = null!;

    public virtual Province Province { get; set; } = null!;
}
