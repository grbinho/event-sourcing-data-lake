using EventSourcing.Abstractions;
using Expenses.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Expenses.Application
{
	/// <summary>
	/// Implements all mutations with event handlers. 
	/// Extends initial class to have natural API
	/// </summary>
    public static class ClaimEventExtensions
    {
		private mutations //functions that transform claim into claim. Also generate events??

		//SubmitMutator function that

		public static Claim Apply()
		{
			// Takes all events and applies them in order
		}	

		public static Claim Submit(this Claim claim)
		{
			SubmitHandler(claim);
		}
    }
}
