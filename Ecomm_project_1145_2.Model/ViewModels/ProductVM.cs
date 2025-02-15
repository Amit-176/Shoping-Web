﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecomm_project_1145_2.Model.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }
        public IEnumerable<SelectListItem>CategoryList { get; set; }
        public IEnumerable<SelectListItem>CoverTypeList { get; set; }
    }
}
