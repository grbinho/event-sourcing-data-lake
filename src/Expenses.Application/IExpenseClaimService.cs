using Expenses.Application.Commands;

namespace Expenses.Application
{
	public interface IExpenseClaimService
	{
		void CreateClaim(CreateClaimCommand command);
		void SubmitClaim(SubmitClaimCommand command);
	}
}