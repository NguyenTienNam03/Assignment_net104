using System.Text.RegularExpressions;

namespace Assignment.Validate
{
	public class Check
	{
		public bool CheckSDT(string sdt)
		{
			if (Regex.IsMatch(sdt, @"^[0][0-9]{9}$"))
			{
				return true;
			}

			return false;
		}
		public bool CheckPassWord(string password)
		{
			if (password.Length > 6 && password.All(c => char.IsLetterOrDigit(c)))
			{
				return true;
			}
			return false;
		}
	}
}
