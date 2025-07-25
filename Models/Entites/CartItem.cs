﻿using Ecommerce_APIs.Models.Entities;

namespace Ecommerce_APIs.Models.Entites
{
    public class CartItem
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public Users User { get; set; } // FK relationship

        public int ProductId { get; set; }
        public Product Product { get; set; } // FK relationship

        public int Quantity { get; set; } = 1;

        public bool IsActive { get; set; } = true;
        public DateTime AddedAt { get; set; } = DateTime.Now;
    }

}
