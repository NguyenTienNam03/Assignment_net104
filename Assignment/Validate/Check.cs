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
			if (password.Length > 6 && Regex.IsMatch(password, "^[a-zA-Z0-9]+$"))
			{
				return true;
			}
			return false;
		}
	}
}
