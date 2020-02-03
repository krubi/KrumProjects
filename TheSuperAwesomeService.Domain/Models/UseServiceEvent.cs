using System;
using System.Collections.Generic;
using System.Text;

namespace TheSuperAwesomeService.Domain.Models
{
    public class UseServiceEvent
    {
        public Guid Id { get; set; }
        public Contract Contract { get; set; }
        public Customer Customer { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
