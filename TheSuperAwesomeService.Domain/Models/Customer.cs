using System;
using System.Collections.Generic;
using System.Text;

namespace TheSuperAwesomeService.Domain.Models
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NId { get; set; }
        public DateTime StartDate { get; set; }
        public long FreeDays { get; set; }
    }
}
