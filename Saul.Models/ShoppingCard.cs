using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saul.Models
{
    public class ShoppingCard
    {
        [Key]
        public int Id { get; set; }

        [Range(0, 1000)]
        public double Count { get; set; }

        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        [ValidateNever]
        public Product? Product { get; set; }

        public string? UserId { get; set; }


        [ForeignKey(nameof(UserId))]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }

        [NotMapped]
        public double? Price { get; set; }
    }
}
