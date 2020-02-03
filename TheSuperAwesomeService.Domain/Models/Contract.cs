using System;
using System.Collections.Generic;
using System.Text;

namespace TheSuperAwesomeService.Domain.Models
{
    public class Contract
    {
        public Guid Id { get; set; }
        public Service Service { get; set; }
        public Customer Customer { get; set; }
        public DateTime StartDate { get; set; }
        public double SpecialPrice { get; set; }
        public double Discount { get; set; }
        public DateTime EndDate { get; set; }
    }
}
