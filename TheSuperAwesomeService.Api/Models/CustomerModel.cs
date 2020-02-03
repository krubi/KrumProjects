using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheSuperAwesomeService.Api.Models
{
    public class CustomerModel
    {
        public string Name { get; set; }
        public string NId { get; set; }
        public DateTime StartDate { get; set; }
        public long FreeDays { get; set; }
    }
}
