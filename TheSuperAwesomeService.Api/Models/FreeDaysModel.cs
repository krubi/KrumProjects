using System;

namespace TheSuperAwesomeService.Api.Models
{
    public class FreeDaysModel
    {
        public Guid CustomerId { get; set; }
        public long FreeDays { get; set; }
    }
}