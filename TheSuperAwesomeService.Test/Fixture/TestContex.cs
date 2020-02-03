using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheSuperAwesomeService.Domain.Persistance;

namespace TheSuperAwesomeService.Test.Fixture
{
    public class TestContex : IDisposable
    {
        public IConfiguration Configuration { get; set; }
        public ServiceProvider ServiceProvider { get; }

        public TestContex()
        {
            var builder = new ConfigurationBuilder().
                SetBasePath(Environment.CurrentDirectory).
                AddJsonFile("local.settings.json", optional: true, reloadOnChange: true).
                AddEnvironmentVariables();

            Configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<ServiceContext>(opt => opt.UseInMemoryDatabase("PricingService"));

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public void Dispose()
        {
            ServiceProvider.Dispose();
        }
    }
}
