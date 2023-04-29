using System;
using System.Collections.Generic;

namespace DemoApp.Models;

public partial class UnitType
{
    public int UnitTypeId { get; set; }

    public string? UnitTypeName { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
