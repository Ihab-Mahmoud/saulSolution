﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Saul.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public  string Title { get; set; }
        [Required]
        public  string Description { get; set; }
        [Required]
        public  string Author { get; set; }
        [Required]
        public  string ISBN { get; set; }
        [Required]
        [DisplayName("List Price")]
        [Range(1,1000)]
        public int ListPrice { get; set; }
        [Required]
        [DisplayName("Price for 1-50")]
        [Range(1, 1000)]
        public int Price { get; set; }
        [Required]
        [DisplayName("Price for 50+")]
        [Range(1, 1000)]
        public int Price50 { get; set; }
        [Required]
        [DisplayName("Price for 100+")]
        [Range(1, 1000)]
        public int Price100 { get; set; }


        public int CategoryId { get; set; }

        [ValidateNever]
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [DisplayName("Product Images")]
        [ValidateNever]
        public List<ProductImage> ProductImages { get; set; }
    }
}
