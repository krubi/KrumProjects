using System;

namespace TheSuperAwesomeService.Api.Models
{
    public class CustomerCostsPeriodModel
    {
        public Guid CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}