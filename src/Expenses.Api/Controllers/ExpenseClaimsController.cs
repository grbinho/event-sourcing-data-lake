using Expenses.Application;
using Expenses.Application.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Expenses.Api.Controllers
{
	[Route("api/expense-claims")]
    public class ExpenseClaimsController: Controller
    {
		private readonly IExpenseClaimService _service;

		public ExpenseClaimsController(IExpenseClaimService service)
		{
			_service = service;
		}

		[HttpPost("create")]
		public IActionResult CreateClaim([FromBody] CreateClaimCommand command)
		{		
			_service.CreateClaim(command);
			return Ok();
		}

		[HttpPost("submit")]
		public IActionResult SubmitClaim([FromBody] SubmitClaimCommand command)
		{
			_service.SubmitClaim(command);
			return Ok();
		}
    }
}
