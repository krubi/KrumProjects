using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheSuperAwesomeService.Domain.Models;
using TheSuperAwesomeService.Api.Models;

namespace TheSuperAwesomeService.Api.Services.Interfaces
{
    public interface IContractService
    {
        Task<List<Customer>> GetCustomersList();
        Task<List<Service>> GetServicesList();
        Task<IEnumerable<UseServiceEvent>> GetEventsPerCustomer(Guid customerId);
        Task<Customer> AddCustomer(CustomerModel customer);
        Task<Contract> AddNewContract(ContractModel contract);
        Task<Contract> AddNewContract(UnlimitedContractModel contract);
        Task AddUseServiceEvent(UseServiceModel useServiceRequest);
        Task<Customer> UpdateCustomer(FreeDaysModel freeDays);
        Task<TotalPriceModel> CalculatePrice(CustomerCostsPeriodModel timeframe);
    }
}
