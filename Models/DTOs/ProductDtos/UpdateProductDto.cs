﻿using System.ComponentModel.DataAnnotations;

namespace Ecommerce_APIs.Models.DTOs.ProductDtos
{
    public class UpdateProductDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public int? UpdatedBy { get; set; }
    }
}
