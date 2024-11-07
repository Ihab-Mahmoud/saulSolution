using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saul.Models.ViewModels
{
    public class ShoppingVM
    {
        public IEnumerable<ShoppingCard> ShoppingCards { get; set; }
        public OrderHeader? OrderHeader { get; set; }

    }
}
