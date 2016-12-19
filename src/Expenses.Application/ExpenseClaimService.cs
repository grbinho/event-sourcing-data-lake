using EventSourcing.Abstractions;
using Expenses.Application.Commands;
using Expenses.Domain;

namespace Expenses.Application
{
    public class ExpenseClaimService
    {
		private readonly IEventStore EventStore;

		public ExpenseClaimService(IEventStore eventStore)
		{
			EventStore = eventStore;
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
											.Apply() // Gets current entity object
											.Submit(); // New command/mutation on the object

			// Store resulting events into the stream
			EventStore.StreamEvents(resultingEvents);

			// If this give as a correct result, we send changed claim to the event store
			
		}

    }
}
