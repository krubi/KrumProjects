using System;
using System.Collections.Generic;
using System.Text;

namespace TheSuperAwesomeService.Domain.Models
{
    public class Service
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double BasePrice { get; set; }
        public DayOfWeek FirstBillableDay { get; set; }
        public DayOfWeek LastBillableDay { get; set; }

    }
}
