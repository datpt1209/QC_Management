using System;
using System.Collections.Generic;

namespace QC_Management.Models;

public partial class DeviceTest : BaseViewModel
{
    public int Id { get; set; }

    public int IdTest { get; set; }

    public int IdDevice { get; set; }

    public virtual Device IdDeviceNavigation { get; set; } = null!;

    public virtual Test IdTestNavigation { get; set; } = null!;

    // Optional TEa override for specific device-method
    public double? TEaPercentOverride { get; set; }

    // Optional persisted last computed bias% (from external control) and sigma
    public double? LastBiasPercent { get; set; }
    public double? LastSigma { get; set; }
    public DateTime? LastBiasUpdatedAt { get; set; }

    // New: JSON-serialized selected Westgard rule keys for this device-test mapping.
    // Example: '["1_2S","1_3S","R-4s"]'
    public string? WestgardRulesJson { get; set; }
}
