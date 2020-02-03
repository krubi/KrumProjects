using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheSuperAwesomeService.Domain.Persistance;
using TheSuperAwesomeService.Api.Services;
using TheSuperAwesomeService.Api.Models;
using TheSuperAwesomeService.Domain.Models;
using TheSuperAwesomeService.Test.Fixture;
using System.Globalization;

namespace TheSuperAwesomeService.Test.Controllers
{
    [Collection("BaseCollection")]
    public class TestCustomers
    {

        private readonly IConfiguration _configuration;
        private readonly ServiceContext _context;

        public TestCustomers(TestContex testContex)
        {
            _context = testContex.ServiceProvider.GetService<ServiceContext>();
            _configuration = testContex.Configuration;
        }

        [Fact]
        public async Task TestCustomerX()
        {

            var service = new ContractService(_context);
            string pattern = "yyyy-MM-dd";
            var startDate = DateTime.ParseExact("2019-09-20", pattern, null);
            var customer = await service.AddCustomer(new CustomerModel { Name = "Customer X", FreeDays = 0, NId = "893423409", StartDate = startDate });

            Assert.IsType<Guid>(customer.Id);

            var services = await service.GetServicesList();

            Service serviceA = null;
            Service serviceC = null;

            foreach (Service s in services)
            {
                if (s.Name.Equals("Service A")) serviceA = s;
                if (s.Name.Equals("Service C")) serviceC = s;
            }

            Assert.NotNull(serviceA);
            Assert.NotNull(serviceC);

            await service.AddUseServiceEvent(new UseServiceModel { CustomerId = customer.Id, ServiceId = serviceA.Id, StartDate = startDate, EndDate = DateTime.Now });

            await service.AddUseServiceEvent(new UseServiceModel { CustomerId = customer.Id, ServiceId = serviceC.Id, StartDate = startDate, EndDate = DateTime.Now });

            var contractStart = DateTime.ParseExact("2019-09-22", pattern, null);
            var contractEnd = DateTime.ParseExact("2019-09-24", pattern, null);
            var contract = await service.AddNewContract(new ContractModel
            {
                CustomerId = customer.Id,
                ServiceId = serviceC.Id,
                Discount = 20,
                StartDate = contractStart,
                EndDate = contractEnd,
                SpecialPrice = serviceC.BasePrice
            });

            var periodEnd = DateTime.ParseExact("2019-10-01", pattern, null);

            var totalPrice = await service.CalculatePrice(new CustomerCostsPeriodModel { CustomerId = contract.Customer.Id, StartDate = startDate, EndDate = periodEnd });

            var specifier = "F2";
            var culture = CultureInfo.CreateSpecificCulture("en-US");
            var priceInEuro = totalPrice.Price.ToString(specifier, culture);

            Assert.True(priceInEuro.Equals("5.64"));

            Console.WriteLine(startDate.ToString());

            Assert.True(startDate < DateTime.Now);
        }
        [Fact]
        public async Task TestCustomerY()
        {
            var service = new ContractService(_context);
            string pattern = "yyyy-MM-dd";
            var startDate = DateTime.ParseExact("2018-01-01", pattern, null);
            var customer = await service.AddCustomer(new CustomerModel
            {
                Name = "Customer Y",
                FreeDays = 200,
                NId = "893423409Y",
                StartDate = startDate
            });

            Assert.IsType<Guid>(customer.Id);

            var services = await service.GetServicesList();

            Service serviceB = null;
            Service serviceC = null;

            foreach (Service s in services)
            {
                if (s.Name.Equals("Service B")) serviceB = s;
                if (s.Name.Equals("Service C")) serviceC = s;
            }

            Assert.NotNull(serviceB);
            Assert.NotNull(serviceC);

            await service.AddUseServiceEvent(new UseServiceModel { CustomerId = customer.Id, ServiceId = serviceB.Id, StartDate = startDate, EndDate = DateTime.Now });

            await service.AddUseServiceEvent(new UseServiceModel { CustomerId = customer.Id, ServiceId = serviceC.Id, StartDate = startDate, EndDate = DateTime.Now });

            //var contractStart = DateTime.ParseExact("2019-09-22", pattern, null);
            //var contractEnd = DateTime.ParseExact("2019-09-24", pattern, null);
            var contractB = await service.AddNewContract(new ContractModel
            {
                CustomerId = customer.Id,
                ServiceId = serviceB.Id,
                Discount = 30,
                StartDate = startDate,
                EndDate = DateTime.MaxValue,
                SpecialPrice = serviceB.BasePrice
            });

            var contractC = await service.AddNewContract(new ContractModel
            {
                CustomerId = customer.Id,
                ServiceId = serviceC.Id,
                Discount = 30,
                StartDate = startDate,
                EndDate = DateTime.MaxValue,
                SpecialPrice = serviceC.BasePrice
            });

            var periodEnd = DateTime.ParseExact("2019-10-01", pattern, null);

            var totalPrice = await service.CalculatePrice(new CustomerCostsPeriodModel { CustomerId = customer.Id, StartDate = startDate, EndDate = periodEnd });

            var specifier = "F3";
            var culture = CultureInfo.CreateSpecificCulture("en-US");
            var priceInEuro = totalPrice.Price.ToString(specifier, culture);

            //price = 165.648

            Assert.True(priceInEuro.Equals("165.648"));

            Console.WriteLine(startDate.ToString());

            Assert.True(startDate < DateTime.Now);
        }
    }
}
