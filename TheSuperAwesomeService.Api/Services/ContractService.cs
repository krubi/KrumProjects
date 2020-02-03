using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TheSuperAwesomeService.Api.Models;
using TheSuperAwesomeService.Api.Services.Interfaces;
using TheSuperAwesomeService.Domain.Models;
using TheSuperAwesomeService.Domain.Persistance;
using System.Data;

namespace TheSuperAwesomeService.Api.Services
{
    public class ContractService : IContractService
    {
        private readonly ServiceContext _context;
        public ContractService(ServiceContext context)
        {
            _context = context;

            var serviceAID = Guid.Parse("d3981aef-bb8e-4cdd-9637-76f0f6b19029");
            var serviceBID = Guid.Parse("b59f7664-8c4a-4352-8e3d-cae0dad23120");
            var serviceCID = Guid.Parse("3111bc87-8964-4548-8353-a7b3dd90af27");
            if (!ServiceExists(serviceAID)) _context.Services.Add(new Service
            {
                Id = serviceAID,
                Name = "Service A",
                BasePrice = 0.2,
                FirstBillableDay = DayOfWeek.Monday,
                LastBillableDay = DayOfWeek.Friday
            });
            if (!ServiceExists(serviceBID)) _context.Services.Add(new Service
            {
                Id = serviceBID,
                Name = "Service B",
                BasePrice = 0.24,
                FirstBillableDay = DayOfWeek.Monday,
                LastBillableDay = DayOfWeek.Friday
            });
            if (!ServiceExists(serviceCID)) _context.Services.Add(new Service
            {
                Id = serviceCID,
                Name = "Service C",
                BasePrice = 0.4,
                FirstBillableDay = DayOfWeek.Monday,
                LastBillableDay = DayOfWeek.Sunday
            });

            _context.SaveChanges();
        }

        public async Task<List<Customer>> GetCustomersList()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task<List<Service>> GetServicesList()
        {
            return await _context.Services.ToListAsync();
        }
        public async Task<IEnumerable<UseServiceEvent>> GetEventsPerCustomer(Guid customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                throw new DataException($"Customer with ID={customerId} not found!");
            }
            return await _context.UseServiceEvents.Where(u => u.Customer == customer).ToListAsync();
        }
        public async Task<Customer> AddCustomer(CustomerModel customer)
        {
            customer.StartDate = DateInDays(customer.StartDate);

            var id = Guid.NewGuid();
            // TODO: check if customer with the same NID exists

            var newCustomer = new Customer { Id = id, Name = customer.Name, NId = customer.NId, StartDate = customer.StartDate, FreeDays = customer.FreeDays };
            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync();
            return newCustomer;
        }

        public async Task AddUseServiceEvent(UseServiceModel useServiceRequest)
        {
            useServiceRequest.StartDate = DateInDays(useServiceRequest.StartDate);
            useServiceRequest.EndDate = DateInDays(useServiceRequest.EndDate);
            var customer = await _context.Customers.FindAsync(useServiceRequest.CustomerId);
            var service = await _context.Services.FindAsync(useServiceRequest.ServiceId);
            var contracts = await _context.Contracts.Where(c => c.Service == service && c.Customer == customer).OrderBy(c => c.StartDate).ToListAsync();

            //var currentContracts = new List<Contract>();
            var usageCovered = false;
            foreach (Contract contract in contracts)
            {
                if (contract.EndDate >= useServiceRequest.EndDate
                    && contract.StartDate <= useServiceRequest.StartDate)
                {
                    // Case 1 CSSC
                    // We have a contract that covers the whole service usage
                    await AddServiceUse(customer, contract, useServiceRequest.StartDate, useServiceRequest.EndDate);
                    usageCovered = true;
                    break;
                }
                if (contract.StartDate <= useServiceRequest.StartDate
                    && contract.EndDate >= useServiceRequest.StartDate
                    && contract.EndDate <= useServiceRequest.EndDate)
                {
                    // Case 2 CSCS
                    await AddServiceUse(customer, contract, useServiceRequest.StartDate, contract.EndDate);
                    useServiceRequest.StartDate = contract.EndDate;
                    continue;
                }
                if (contract.EndDate <= useServiceRequest.EndDate
                    && contract.StartDate > useServiceRequest.StartDate)
                {
                    // Case 3 SCCS
                    var newContract = await AddDefaultContract(customer, service, useServiceRequest.StartDate, contract.StartDate);
                    // add first contract 
                    await AddServiceUse(customer, newContract, newContract.StartDate, newContract.EndDate);
                    // use current contract
                    await AddServiceUse(customer, contract, contract.StartDate, contract.EndDate);
                    useServiceRequest.StartDate = contract.EndDate;
                    continue;
                }
                if (contract.EndDate >= useServiceRequest.EndDate
                    && contract.StartDate > useServiceRequest.StartDate
                    && contract.StartDate < useServiceRequest.EndDate)
                {
                    // Case 4 SCSC
                    var newContract = await AddDefaultContract(customer, service, useServiceRequest.StartDate, contract.StartDate);
                    // add new contract 
                    await AddServiceUse(customer, newContract, useServiceRequest.StartDate, newContract.EndDate);
                    // use current contract
                    await AddServiceUse(customer, contract, contract.StartDate, useServiceRequest.EndDate);
                    usageCovered = true;
                    break;
                }
                else if (contract.StartDate >= useServiceRequest.EndDate)
                {
                    // Case 5 SSCC
                    Contract newContract = await AddDefaultContract(customer, service, useServiceRequest.StartDate, useServiceRequest.EndDate);
                    await AddServiceUse(customer, newContract, useServiceRequest.StartDate, newContract.EndDate);
                    usageCovered = true;
                    break;
                }
            }

            if (!usageCovered)
            {
                // Case 6 CCSS
                var newContract = await AddDefaultContract(customer, service, useServiceRequest.StartDate, useServiceRequest.EndDate);
                await _context.Contracts.AddAsync(newContract);
                await AddServiceUse(customer, newContract, newContract.StartDate, newContract.EndDate);
            }
            await _context.SaveChangesAsync();

        }

        private async Task<Contract> AddDefaultContract(Customer customer, Service service, DateTime startDate, DateTime endDate, double price = -1, double discount = 0)
        {
            price = price < 0 ? service.BasePrice : price;
            var newContract = new Contract
            {
                Id = Guid.NewGuid(),
                Service = service,
                StartDate = startDate,
                EndDate = endDate,
                SpecialPrice = price,
                Customer = customer,
                Discount = discount
            };
            await _context.Contracts.AddAsync(newContract);
            return newContract;
        }

        private async Task<UseServiceEvent> AddServiceUse(Customer customer, Contract contract, DateTime startDate, DateTime endDate)
        {
            var serviceUse = new UseServiceEvent
            {
                Id = Guid.NewGuid(),
                Customer = customer,
                Contract = contract,
                StartDate = startDate,
                EndDate = endDate
            };
            await _context.UseServiceEvents.AddAsync(serviceUse);
            return serviceUse;
        }

        public async Task<Contract> AddNewContract(ContractModel contract)
        {
            contract.StartDate = DateInDays(contract.StartDate);
            contract.EndDate = DateInDays(contract.EndDate);
            var service = await _context.Services.FindAsync(contract.ServiceId);
            if (service == null)
            {
                throw new DataException($"Service with ID={contract.ServiceId} not found!");
            }

            var customer = await _context.Customers.FindAsync(contract.CustomerId);
            if (customer == null)
            {
                throw new DataException($"Customer with ID={contract.CustomerId} not found!");
            }


            var newContract = new Contract
            {
                Id = Guid.NewGuid(),
                Customer = customer,
                Service = service,
                Discount = contract.Discount,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                SpecialPrice = contract.SpecialPrice
            };
            await CheckOverlapingContracts(newContract);
            await _context.AddAsync<Contract>(newContract);
            await _context.SaveChangesAsync();

            return newContract;
        }

        public async Task<Contract> AddNewContract(UnlimitedContractModel contract)
        {
            var newContract = new ContractModel
            {
                CustomerId = contract.CustomerId,
                ServiceId = contract.ServiceId,
                Discount = contract.Discount,
                SpecialPrice = contract.SpecialPrice,
                StartDate = DateTime.MinValue,
                EndDate = DateTime.MaxValue
            };

            return await AddNewContract(newContract);
        }

        public async Task<Customer> UpdateCustomer(FreeDaysModel freeDays)
        {
            var customer = await GetCustomer(freeDays.CustomerId);
            customer.FreeDays = freeDays.FreeDays;
            await _context.SaveChangesAsync();

            return customer;
        }

        public async Task<TotalPriceModel> CalculatePrice(CustomerCostsPeriodModel timeframe)
        {
            timeframe.StartDate = DateInDays(timeframe.StartDate);
            timeframe.EndDate = DateInDays(timeframe.EndDate);

            _context.Services.Load<Service>();
            _context.Contracts.Load<Contract>();
            var customer = await GetCustomer(timeframe.CustomerId);

            var serviceEvents = await _context.UseServiceEvents.
                Where(e => e.Customer == customer && e.Contract != null).
                OrderBy(e => e.StartDate).
                ToListAsync();
            var freeDict = new Dictionary<Service, long>();
            double price = 0;
            foreach (UseServiceEvent evnt in serviceEvents)
            {
                var service = evnt.Contract.Service;
                if (freeDict.ContainsKey(service))
                {
                    if (freeDict[service] - CalcDays(evnt, timeframe, service) >= 0)
                    {
                        freeDict[service] -= CalcDays(evnt, timeframe, service);
                    }
                    else
                    {
                        price += evnt.Contract.SpecialPrice * (CalcDays(evnt, timeframe, service) - freeDict[service]) * (1 + evnt.Contract.Discount / 100);
                        freeDict[service] = 0;
                    }
                }
                else
                {
                    var pricedDays = CalcDays(evnt, timeframe, service);
                    if (customer.FreeDays - pricedDays >= 0)
                    {
                        freeDict[service] = customer.FreeDays - CalcDays(evnt, timeframe, service);
                    }
                    else
                    {
                        price += evnt.Contract.SpecialPrice * (pricedDays - customer.FreeDays) * (1 - evnt.Contract.Discount / 100);
                    }
                }
            }
            return new TotalPriceModel { Price = price };
        }

        private long CalcDays(UseServiceEvent use, CustomerCostsPeriodModel timeframe, Service service)
        {

            var endStartDiff = DaysDiff(use.EndDate, timeframe.StartDate);
            var startEndDiff = DaysDiff(use.StartDate, timeframe.EndDate);

            // No overlap
            if (endStartDiff > 0 || startEndDiff < 0) return 0;

            var startDiff = DaysDiff(timeframe.StartDate, use.StartDate);
            var endDiff = DaysDiff(use.EndDate, timeframe.EndDate);
            var useDiff = DaysDiff(use.StartDate, use.EndDate);

            var sDate = DateTime.MinValue;
            var eDate = DateTime.MinValue;

            if (startDiff >= 0 && endDiff >= 0)
            {
                sDate = use.StartDate;
                eDate = use.EndDate;
            }
            else if (startDiff < 0 && endDiff < 0)
            {
                sDate = timeframe.StartDate;
                eDate = timeframe.EndDate;
            }
            else if (startDiff < 0)
            {
                sDate = timeframe.StartDate;
                eDate = use.EndDate;
            }
            else
            {
                sDate = use.StartDate;
                eDate = timeframe.EndDate;
            }
            if (service.LastBillableDay > service.FirstBillableDay)
            {
                var daysStart = 0;
                if (sDate.DayOfWeek != service.FirstBillableDay)
                    daysStart = service.LastBillableDay - sDate.DayOfWeek + 1;
                var daysEnd = 0;
                if (eDate.DayOfWeek != service.LastBillableDay)
                    daysEnd = eDate.DayOfWeek - service.FirstBillableDay;

                var totalDays = DaysDiff(sDate, eDate);

                var weeks = totalDays / 7 * (service.LastBillableDay - service.FirstBillableDay + 1);

                return (weeks + daysStart + daysEnd);
            }
            else
            {
                return DaysDiff(sDate, eDate);
            }
        }

        private async Task<Customer> GetCustomer(Guid customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                throw new DataException($"Customer with ID={customerId} not found!");
            }
            return customer;
        }

        private async Task CheckOverlapingContracts(Contract newContract)
        {
            var contracts = await _context.Contracts.Where(c => c.Service == newContract.Service && c.Customer == newContract.Customer).OrderBy(c => c.StartDate).ToListAsync();
            foreach (Contract contract in contracts)
            {
                var eventList = await _context.UseServiceEvents.Where(e => e.Contract == contract).ToListAsync();
                switch (OverlapCase(contract.StartDate, contract.EndDate, newContract.StartDate, newContract.EndDate))
                {
                    /*0: No overlap 1122 e1S2 > 0 or 2211 s1E2 < 0
                    * 1: Start overlat 1212
                    * 2: Middle overlap 2112
                    * 3: End overlap 2121
                    * 4: Complete overlap 1221
                    * */
                    case 0:
                        // good stuff no overlap
                        break;
                    case 1: //1212 => 1122
                        contract.EndDate = newContract.StartDate;
                        foreach (UseServiceEvent s in eventList)
                        {
                            await CheckServiceUseOverlap(s, newContract);
                        }
                        break;
                    case 2: // 2112 => 22 remove contract
                        foreach (UseServiceEvent ev in eventList)
                        {
                            await CheckServiceUseOverlap(ev, newContract);
                        }
                        _context.Contracts.Remove(contract);
                        break;
                    case 3: //2121 => 2211
                        contract.StartDate = (newContract.EndDate);
                        foreach (UseServiceEvent s in eventList)
                        {
                            await CheckServiceUseOverlap(s, newContract);
                        }
                        break;
                    case 4: //1221 => 112233

                        var thirdContract = await AddDefaultContract(
                            contract.Customer,
                            contract.Service,
                            (newContract.EndDate),
                            contract.EndDate,
                            contract.SpecialPrice,
                            contract.Discount);
                        contract.EndDate = (newContract.StartDate);
                        foreach (UseServiceEvent s in eventList)
                        {
                            await CheckServiceUseOverlap(s, newContract, thirdContract);
                        }
                        break;
                }
            }
        }

        private async Task CheckServiceUseOverlap(UseServiceEvent serviceEvent, Contract contract, Contract secondContract = null)
        {
            var overlapCase = OverlapCase(serviceEvent.StartDate, serviceEvent.EndDate, contract.StartDate, contract.EndDate);
            /*0: No overlap 1122 e1S2 > 0 or 2211 s1E2 < 0
            * 1: Start overlat 1212
            * 2: Middle overlap 2112
            * 3: End overlap 2121
            * 4: Complete overlap 1221
            * */
            switch (overlapCase)
            {
                case 0: break;
                case 1:
                    serviceEvent.EndDate = (contract.StartDate);
                    await AddServiceUse(
                        serviceEvent.Customer,
                        contract,
                        contract.StartDate,
                        serviceEvent.EndDate);
                    break;
                case 2:
                    serviceEvent.Contract = contract;
                    break;
                case 3:
                    serviceEvent.StartDate = (contract.EndDate);
                    await AddServiceUse(serviceEvent.Customer,
                        contract,
                        serviceEvent.StartDate,
                        contract.EndDate
                        );
                    break;
                case 4:
                    serviceEvent.EndDate = contract.StartDate;
                    await AddServiceUse(serviceEvent.Customer,
                        contract,
                        contract.StartDate,
                        contract.EndDate
                        );
                    if (secondContract != null)
                    {
                        await AddServiceUse(serviceEvent.Customer,
                                                secondContract,
                                                secondContract.StartDate,
                                                secondContract.EndDate
                                                );
                    }

                    break;
            }
        }

        private DateTime DateInDays(DateTime date)
        {
            var diff = DaysDiff(DateTime.MinValue, date);
            if (date == DateTime.MaxValue)
            {
                return DateTime.MaxValue;
            }
            else
            {
                return DateTime.MinValue.AddDays(diff);
            }
        }

        private long DaysDiff(DateTime startDate, DateTime endDate)
        {
            return Convert.ToInt64(Math.Floor((endDate - startDate).TotalDays));
        }

        private bool ServiceExists(Guid id)
        {
            return _context.Services.Any(e => e.Id == id);
        }

        /**
        * 0: No overlap 1122 e1S2 > 0 or 2211 s1E2 < 0 
        * 1: Start overlat 1212
        * 2: Middle overlap 2112
        * 3: End overlap 2121
        * 4: Complete overlap 1221 
        **/
        private int OverlapCase(DateTime i1Start, DateTime i1End, DateTime i2Start, DateTime i2End)
        {
            var s1S2 = DaysDiff(i1Start, i2Start);
            var s1E2 = DaysDiff(i1Start, i2End);
            var e1S2 = DaysDiff(i1End, i2Start);
            var e1E2 = DaysDiff(i1End, i2End);

            if (e1S2 >= 0 || s1E2 <= 0) return 0;
            if (s1S2 >= 0 && e1S2 >= 0 && e1E2 >= 0) return 1;
            if (s1S2 <= 0 && e1E2 >= 0) return 2;
            if (s1S2 <= 0 && s1E2 >= 0 && e1E2 <= 0) return 3;
            if (s1S2 >= 0 && e1E2 <= 0) return 4;

            return -1;
        }
    }
}
