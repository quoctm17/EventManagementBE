using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class SystemConfig
{
    public Guid ConfigId { get; set; }

    public string ConfigKey { get; set; } = null!;

    public string ConfigValue { get; set; } = null!;

    public string? Additional { get; set; }
}
