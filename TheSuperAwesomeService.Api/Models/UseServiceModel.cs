using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheSuperAwesomeService.Api.Models
{
    public class UseServiceModel
    {
        public Guid ServiceId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
