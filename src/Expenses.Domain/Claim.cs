using EventSourcing.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Expenses.Domain
{
	public class Claim : IEventSourced<Claim>
	{
		public Guid Id { get; set; }
		public DateTime DateCreatedUtc { get; set; }
		public DateTime DateModifiedUtc { get; private set; }
		public Claimant Claimant { get; private set; }
		public string Description { get; set; }
		public ClaimStatus Status { get; private set; }
		public IList<Expense> Expenses { get; set; }
		public decimal TotalAmount { get; private set; }
		public long Version { get; private set; }

		#region Constructors

		public Claim()
		{
			Id = Guid.NewGuid();
			DateCreatedUtc = DateTime.UtcNow;
			DateModifiedUtc = DateTime.UtcNow;
			Expenses = new List<Expense>();
			Status = ClaimStatus.Open;
		}

		private Claim(Claimant claimant)
			:this()
		{
			if (claimant == null)
				throw new ArgumentNullException(nameof(claimant));

			Claimant = claimant;
		}

		private Claim(Claimant claimant, IList<Expense> expenses)
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

		/*
		 * Mutation only mutate state of the object. They should not generate any commands or
		 * any additional external side effects. That can be done in the Methods themselves.
		 */

		private static Func<Claim, CreateClaimCommand, ClaimCreatedEvent> CreateClaimMutation = (c, cmd) =>
		{
			var claim = new Claim(cmd.Claimant, cmd.Expenses);
			claim.Description = cmd.Description;
			claim.TotalAmount = claim.Expenses.Sum(e => e.Amount);
			claim.Version++;

			// TOOD: On replay, original DateCreated and DateModified should remain

			return new ClaimCreatedEvent
			{
				Id = Guid.NewGuid(),
				Entity = claim,
				EntityId = claim.Id,
				Command = cmd,
				Mutation = "CreateClaim",
				Timestamp = DateTime.UtcNow.Ticks
			};
		};

		private static Func<Claim, ChangeClaimantCommand, ClaimantChangedEvent> ChangeClaimantMutation = (claim, cmd) =>
		{
			var command = cmd as ChangeClaimantCommand;
			claim.Claimant = command.Claimant;
			claim.DateModifiedUtc = DateTime.UtcNow;
			claim.Version++;
			return new ClaimantChangedEvent
			{
				Id = Guid.NewGuid(),
				Entity = claim,
				EntityId = claim.Id,
				Command = command,
				Mutation = "ChangeClaimant",
				Timestamp = DateTime.UtcNow.Ticks
			};
		};

		private static Func<Claim, SubmitCommand, ClaimSubmittedEvent> SubmitMutation = (claim, cmd) =>
		{
			claim.Status = ClaimStatus.Submitted;
			claim.DateModifiedUtc = DateTime.UtcNow;
			claim.Version++;
			return new ClaimSubmittedEvent
			{
				Id = Guid.NewGuid(),
				Entity = claim,
				EntityId = claim.Id,
				Command = cmd,
				Mutation = "Submit",
				Timestamp = DateTime.UtcNow.Ticks
			};
		};

		// Build up Claim object by applying mutations from all events
		// If we



		#endregion

		// State changes result in events getting raised.
		// We need to raise an event for this particular entity (Id bound)

		#region Commands and Events

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

		public class CreateClaimCommand: Command
		{
			public string Description { get; set; }
			public IList<Expense> Expenses { get; set; }
			public Claimant Claimant { get; set; }
		}

		public class ClaimCreatedEvent : Event<Claim, CreateClaimCommand>
		{
		}

		public class ChangeClaimantCommand: Command
		{
			public Claimant Claimant { get; set; }
		}

		public class ClaimantChangedEvent: Event<Claim, ChangeClaimantCommand>
		{
		}

		public class SubmitCommand : Command
		{
		}

		public class ClaimSubmittedEvent: Event<Claim, SubmitCommand>
		{
		}



		// Should it be possible to return more that one??
		public ClaimantChangedEvent ChangeClaimant(ChangeClaimantCommand command)
		{
			var result = ChangeClaimantMutation(this, command);
			return result as ClaimantChangedEvent;
		}

		public ClaimSubmittedEvent Submit()
		{
			return SubmitMutation(this, new SubmitCommand());
		}

		public static ClaimCreatedEvent CreateClaim(CreateClaimCommand command)
		{
			return CreateClaimMutation(null, command);
		}

        public Claim Replay(IEnumerable<(Type type,string data)> events)
		{
			var claim = new Claim();
			// Starting form beginning, there is either create or snapshot event that gives us initial object
			foreach (var @event in events)
			{
				switch (@event.type.Name)
				{
					case "ClaimCreatedEvent":
						{
							var evt = JsonConvert.DeserializeObject<ClaimCreatedEvent>(@event.data);
							var result = CreateClaimMutation(claim, evt.Command);
							claim = result.Entity;
							claim.Id = evt.EntityId;
							break;
						}
					case "ClaimantChangedEvent":
						{
							var evt = JsonConvert.DeserializeObject<ClaimantChangedEvent>(@event.data);
							var result = ChangeClaimantMutation(claim, evt.Command);
							claim = result.Entity;
							break;
						}
					case "ClaimSubmittedEvent":
						{
							var evt = JsonConvert.DeserializeObject<ClaimSubmittedEvent>(@event.data);
							var result = SubmitMutation(claim, evt.Command);
							claim = result.Entity;
							break;
						}
					default:
						break;
				}
			}

			return claim;
		}
		#endregion
	}
}
