using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saul.Models.ViewModels
{
        public class UserRoleVM
        {
            [ValidateNever]
            public IEnumerable<SelectListItem> RoleList { get; set; }


            [ValidateNever]
            public IEnumerable<SelectListItem> CompanyList { get; set; }

            public ApplicationUser ApplicationUser { get; set; }
        }

}
