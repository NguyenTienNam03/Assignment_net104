using Assignment.Areas.Admin.Data.Data;
using Assignment.Models.Data;

namespace Assignment.IServices
{
    public interface IBillSerivce
	{
		public bool AddBill(Bill bill);
		public bool UpdateBill(Bill bill);
		public List<Bill> GetBillList();
		public Bill GetBillByID(Guid id);
	}
}
