namespace Expenses.Domain
{
	public struct Currency
    {
		public string ISOCode { get; }
		public string Name { get; }

		public Currency(string isoCode, string name)
		{
			this.ISOCode = isoCode;
			this.Name = name;
		}

		//TODO: Implement properly
    }
}
