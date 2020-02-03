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
    public class CustomersController : ControllerBase
    {
        private readonly IContractService _contractService;

        public CustomersController(ServiceContext context, IContractService contractService)
        {
            _contractService = contractService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetAllCustomers()
        {
            var result = await _contractService.GetCustomersList();
            return Ok(result);
        }

        [HttpPost("AddNewCustomer")]
        public async Task<ActionResult<Customer>> PostCustomer([FromBody] CustomerModel customer)
        {
            Customer newCustomer = await _contractService.AddCustomer(customer);

            return Ok(newCustomer);
        }

        [HttpPost("Contract")]
        public async Task<ActionResult<Contract>> AddContract([FromBody] ContractModel newContract)
        {
            var _newContract = await _contractService.AddNewContract(newContract);

            return Ok(_newContract);
        }

        [HttpPost("UnlimitedContract")]
        public async Task<ActionResult<Contract>> UnlimitedContract([FromBody] UnlimitedContractModel newContract)
        {
            var _newContract = await _contractService.AddNewContract(newContract);

            return Ok(_newContract);
        }

        [HttpPost("SetFreeDays")]
        public async Task<ActionResult<Contract>> UnlimitedContract([FromBody] FreeDaysModel freeDaysUpdate)
        {
            var _newContract = await _contractService.UpdateCustomer(freeDaysUpdate);

            return Ok(_newContract);
        }

    }
}
