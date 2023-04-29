using System;
using System.Collections.Generic;

namespace DemoApp.Models;

public partial class ProductManufacturer
{
    public int ProductManufacturerId { get; set; }

    public string ProductManufacturerName { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
