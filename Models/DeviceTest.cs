namespace QC_Management.Models;

public partial class DeviceTest : BaseViewModel

{
    public int Id { get; set; }

    public int IdTest { get; set; }

    public int IdDevice { get; set; }

    public virtual Device IdDeviceNavigation { get; set; } = null!;

    public virtual Test IdTestNavigation { get; set; } = null!;
}
