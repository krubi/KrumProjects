using Microsoft.EntityFrameworkCore;
using TheSuperAwesomeService.Domain.Models;
using System;

namespace TheSuperAwesomeService.Domain.Persistance
{
    public class ServiceContext : DbContext
    {
        public ServiceContext(DbContextOptions<ServiceContext> options)
            : base(options)
        {
        }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<UseServiceEvent> UseServiceEvents { get; set; }
    }
}
