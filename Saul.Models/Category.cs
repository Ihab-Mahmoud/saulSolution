using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Saul.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [DisplayName("Category Name")]
        [MaxLength(100)]
        public required string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1,99)]
        public int DisplayOrder { get; set; }
    }
}
        