using EventSourcing.Abstractions;
using Expenses.Application.Commands;
using Expenses.Domain;
using System.Linq;

namespace Expenses.Application
{
    public class ExpenseClaimService : IExpenseClaimService
	{
		private readonly IEventStore EventStore;

		public ExpenseClaimService(IEventStore eventStore)
		{
			EventStore = eventStore;
		}

		/// <summary>
		/// Create new claim
		/// </summary>
		/// <param name="command">Command that describes the claim</param>
		public void CreateClaim(CreateClaimCommand command)
		{
			var resultingEvents = Claim.CreateClaim(new Claim.CreateClaimCommand
			{
				Claimant = new Domain.Claimant
				{
					Name = "Test Claimant",
				},
				Description = command.Description,
				Expenses = command.Expenses.Select(e => new Domain.Expense
				{
					Amount = e.Amount
				}).ToList()
			});

			EventStore.StreamEvents(resultingEvents);
		}

		public void SubmitClaim(SubmitClaimCommand command)
		{
			// Take the command and apply it to the correct claim object.

			// This retrieves current state of the claim and calls Submit on it.
			// Claim object reflects the state claim had at the time this is called.
			// Before submit is executed, actual state could have changed.
			// Submit needs to validate 

			//Submit can generate events and/or return new state of the entity

			// Store both event and entity at that point in time?
			var resultingEvents = EventStore.GetEvents<Claim>(command.ClaimId)
											.Replay() // Gets current entity object
											.Submit(); // New command/mutation on the object

			// Store resulting events into the stream
			EventStore.StreamEvents(resultingEvents);

			// If this give as a correct result, we send changed claim to the event store
			
		}

    }
}
