namespace Expenses.Domain
{
	public struct ExpenseCategory
    {
		public string Code { get; set; }
		public string Name { get; set; }

		public ExpenseCategory(string code, string name)
		{
			this.Code = code;
			this.Name = name;
		}

		// TODO: Implement properly
    }
}
