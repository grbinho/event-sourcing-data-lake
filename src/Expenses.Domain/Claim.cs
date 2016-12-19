using EventSourcing.Abstractions;
using System;
using System.Collections.Generic;
using ClaimMutation = System.Func<Expenses.Domain.Claim, EventSourcing.Abstractions.ICommand, EventSourcing.Abstractions.IEvent<Expenses.Domain.Claim>>;

namespace Expenses.Domain
{
	public class Claim: IEventSourced<Claim>
	{
		public Guid Id { get; set; }
		public DateTime DateCreatedUtc { get; set; }
		public DateTime DateModifiedUtc { get; private set; }
		public Claimant Claimant { get; private set; }
		public string Description { get; set; }
		public ClaimStatus Status { get; private set; }
		public IList<Expense> Expenses { get; set; }
		public decimal TotalAmount { get; }

		#region Constructors

		public Claim()
		{
			Id = Guid.NewGuid();
			DateCreatedUtc = DateTime.UtcNow;
			DateModifiedUtc = DateTime.UtcNow;
			Expenses = new List<Expense>();
			Status = ClaimStatus.Open;
		}

		public Claim(Claimant claimant)
			:this()
		{
			if (claimant == null)
				throw new ArgumentNullException(nameof(claimant));

			Claimant = claimant;
		}

		public Claim(Claimant claimant, IList<Expense> expenses)
			:this(claimant)
		{
			if(expenses == null)			
				throw new ArgumentNullException(nameof(expenses));

			if (expenses.Count == 0)
				throw new ArgumentException("Expected at least one expense", nameof(expenses));

			Expenses = expenses;
		}

		#endregion

		#region Commands/Mutations

		private static ClaimMutation ChangeClaimantMutation = (claim, cmd) =>
		{
			var command = cmd as ChangeClaimantCommand;
			claim.Claimant = command.Claimant;
			return new ClaimantChangedEvent
			{
				Entity = claim,
				Command = command,
				Mutation = "ChangeClaimant"
			};
		};

		private static ClaimMutation SubmitMutation = (claim, cmd) =>
		{
			claim.Status = ClaimStatus.Submitted;
			return new ClaimSubmittedEvent
			{
				Entity = claim,
				Command = cmd,
				Mutation = "Submit"
			};
		};

		private static Dictionary<string, ClaimMutation> _mutations = new Dictionary<string, ClaimMutation>
		{
			["ChangeClaimant"] = ChangeClaimantMutation
		};

		// Build up Claim object by applying mutations from all events
		// If we 
		
		

		#endregion

		// State changes result in events getting raised.
		// We need to raise an event for this particular entity (Id bound)

		#region Commands

		/* 
		 * Each method that modifies state is an event handler.
		 * Some commands have implicit state changes (Submit)
		 * Others have input parameters (ChangeClaimant) and can have additional
		 * 
		 * If we only store command name, 
		 * we can't have current state without the code in domain model.
		 * There could be multiple transformations related to that event happening.
		 * This also allows us to change some of the logic and replay the events.
		*/
		public class ChangeClaimantCommand: ICommand
		{
			public Claimant Claimant { get; set; }
		}

		public class ClaimantChangedEvent: IEvent<Claim>
		{
			public string Mutation { get; set; }
			public ICommand Command { get; set; }
			public Claim Entity { get; set; }
		}


		public class SubmitCommand : ICommand
		{
		}

		public class ClaimSubmittedEvent: IEvent<Claim>
		{
			public string Mutation { get; set; }
			public ICommand Command { get; set; }
			public Claim Entity { get; set; }
		}

		// Should it be possible to return more that one??
		public ClaimantChangedEvent ChangeClaimant(ChangeClaimantCommand command)
		{
			var result = ChangeClaimantMutation(this, command);
			return result as ClaimantChangedEvent;
		}

		public ClaimSubmittedEvent Submit()
		{
			var result = SubmitMutation(this, new SubmitCommand());
			return result as ClaimSubmittedEvent;
		}

		public Claim Apply(IEnumerable<IEvent<Claim>> events)
		{
			var claim = new Claim();
			// Starting form beginning, there is either create or snapshot event that gives us initial object
			foreach (var @event in events)
			{
				_mutations[@event.Mutation](claim, @event.Command);
			}

			return claim;
		}

		#endregion
	}
}
