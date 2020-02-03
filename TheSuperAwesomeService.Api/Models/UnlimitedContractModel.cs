using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheSuperAwesomeService.Api.Models
{
    public class UnlimitedContractModel
    {
        public Guid ServiceId { get; set; }
        public Guid CustomerId { get; set; }
        public double SpecialPrice { get; set; }
        public double Discount { get; set; }
    }
}
