using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;  // Required for [NotMapped]

namespace FoodY.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string Name { get; set; } // Save random userid or custom name if needed
        public string PaymentType { get; set; } = "Not Paid"; // Default
        public DateTimeOffset? CreatedDate { get; set; } = DateTimeOffset.Now;
        public ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();
        public string? Status { get; set; } = "active"; // Default status
        public string? UserId { get; set; }
    }

    public class CartDetail
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public Cart Cart { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; } = 1; // Default quantity 1
        public decimal Price { get; set; } // Get price from the product
        public DateTimeOffset? CreatedDate { get; set; } = DateTimeOffset.Now;
        public string? UserId { get; set; } // UserId from SignInManager
    }
    public class CartItemRequest
    {
        public string? UserId { get; set; }   // User who is adding the item
        public int ProductId { get; set; }   // Product being added
        public int Quantity { get; set; }    // Quantity of the product (default 1)
    }
    public class KhaltiPaymentRequest
    {
        public string return_url { get; set; }
        public string website_url { get; set; }
        public int amount { get; set; }
        public string purchase_order_id { get; set; }
        public string purchase_order_name { get; set; }
        public CustomerInfo customer_info { get; set; }
    }

    public class CustomerInfo
    {
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
    }

}
