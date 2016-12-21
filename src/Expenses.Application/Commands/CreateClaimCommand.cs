using Expenses.Application.Model;
using System;
using System.Collections.Generic;

namespace Expenses.Application.Commands
{
	public class CreateClaimCommand
    {
		public string Description { get; set; }
		public Guid ClaimantId { get; set; }
		public IList<Expense> Expenses { get; set; }
	}
}
