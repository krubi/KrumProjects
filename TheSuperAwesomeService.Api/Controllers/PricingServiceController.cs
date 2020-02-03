using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TheSuperAwesomeService.Domain.Models;
using TheSuperAwesomeService.Domain.Persistance;
using TheSuperAwesomeService.Api.Models;
using TheSuperAwesomeService.Api.Services.Interfaces;

namespace TheSuperAwesomeService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PricingServiceController : ControllerBase
    {
        private readonly IContractService _contractService;

        public PricingServiceController(IContractService contractService)
        {
            _contractService = contractService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Service>>> GetService()
        {
            var result = await _contractService.GetServicesList();
            return Ok(result);
        }

        [HttpGet("GetCustomerData/{customerId}")]
        public async Task<ActionResult<IEnumerable<UseServiceEvent>>> GetServiceEventsPerCustomer(Guid customerId)
        {
            var result = await _contractService.GetEventsPerCustomer(customerId);
            return Ok(result);
        }


        [HttpPost("UseService")]
        public async Task<IActionResult> UseServiceA([FromBody] UseServiceModel useServiceRequest)
        {

            try
            {
                await _contractService.AddUseServiceEvent(useServiceRequest);
                return Ok();
            }
            catch (DataException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost("GetCosts")]
        public async Task<ActionResult<TotalPriceModel>> GetCustomerCosts([FromBody] CustomerCostsPeriodModel customerModel)
        {
            var result = await _contractService.CalculatePrice(customerModel);
            return Ok(result);
        }
    }
}
