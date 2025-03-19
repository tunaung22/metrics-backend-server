﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Common.Filters
{
    class ApiPaginationFilter
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be present and greater than 0")]
        public int PageNumber { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "PageSize must be present and greater than 0")]
        public int PageSize { get; set; }


        public ApiPaginationFilter() { }

        public ApiPaginationFilter(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
