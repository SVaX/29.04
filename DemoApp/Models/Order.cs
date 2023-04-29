using System;
using System.Collections.Generic;

namespace DemoApp.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int OrderStatusId { get; set; }

    public int PickupPointId { get; set; }

    public DateTime OrderCreateDate { get; set; }

    public DateTime OrderDeliveryDate { get; set; }

    public int? UserId { get; set; }

    public int OrderGetCode { get; set; }

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual OrderStatus OrderStatus { get; set; } = null!;

    public virtual PickupPoint PickupPoint { get; set; } = null!;

    public virtual User? User { get; set; }
}
