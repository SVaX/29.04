using System;
using System.Collections.Generic;

namespace DemoApp.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductArticleNumber { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public int UnitTypeId { get; set; }

    public decimal ProductCost { get; set; }

    public byte? ProductMaxDiscountAmount { get; set; }

    public int ProductManufacturerId { get; set; }

    public int ProductSupplierId { get; set; }

    public int ProductCategoryId { get; set; }

    public byte? ProductDiscountAmount { get; set; }

    public int ProductQuantityInStock { get; set; }

    public string ProductDescription { get; set; } = null!;

    public string? ProductPhoto { get; set; }

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual ProductCategory ProductCategory { get; set; } = null!;

    public virtual ProductManufacturer ProductManufacturer { get; set; } = null!;

    public virtual ProductSupplier ProductSupplier { get; set; } = null!;

    public virtual UnitType UnitType { get; set; } = null!;
}
