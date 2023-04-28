using Assignment.IServices;
using Assignment.Models;
using Assignment.Models.Data;
using Assignment.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Assignment.Controllers
{
    public class DanhMucController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		private Shopping_Dbcontext db = new Shopping_Dbcontext();
		private IProductsService productsService;
		private ICapacityService capacityService;
		private ICategoryService categoryService;
		private ISupplierService supplierService;
		private IUserService _iuser;

		private ICartService _icartService;
		private ICartDetialsService _icartdetal;
		public DanhMucController(ILogger<HomeController> logger)
		{
			_logger = logger;
			productsService = new ProductsService();
			capacityService = new CapacityService();
			categoryService = new CategoryService();
			supplierService = new SupplierService();
			_iuser = new UserService();
			_icartService = new CartService();

			_icartdetal = new CartDetailsService();
	}
		public IActionResult DienThoai(int? pageNumber)
		{
			int pageSize = 8;
			var list = db.Products.Include(c => c.Category).Include(c => c.Capacity).Include(c => c.Supplier).Where(c => c.Category.Description == "Điện thoại" && c.AvailableQuantity > 0);
			return View(PaginatedList<Product>.Create(list.ToList(), pageNumber ?? 1, pageSize));
		}
		public IActionResult LapTop(int? pageNumber)
		{
			int pageSize = 8;
			var list = db.Products.Include(c => c.Category).Include(c => c.Capacity).Include(c => c.Supplier).Where(c => c.Category.Description == "LapTop" && c.AvailableQuantity > 0);
			return View(PaginatedList<Product>.Create(list.ToList(), pageNumber ?? 1, pageSize));
		}
		public IActionResult MayTinhBang(int? pageNumber)
		{
			int pageSize = 8;
			var list = db.Products.Include(c => c.Category).Include(c => c.Capacity).Include(c => c.Supplier).Where(c => c.Category.Description == "Máy Tinh Bảng" && c.AvailableQuantity > 0);
			return View(PaginatedList<Product>.Create(list.ToList(), pageNumber ?? 1, pageSize));
		}
		public IActionResult TV(int? pageNumber)
		{
			int pageSize = 8;
			var list = db.Products.Include(c => c.Category).Include(c => c.Capacity).Include(c => c.Supplier).Where(c => c.Category.Description == "TiVi" && c.AvailableQuantity > 0);
			return View(PaginatedList<Product>.Create(list.ToList(), pageNumber ?? 1, pageSize));
		}
		public IActionResult AmThanh(int? pageNumber)
		{
			int pageSize = 8;
			var list = db.Products.Include(c => c.Category).Include(c => c.Capacity).Include(c => c.Supplier).Where(c => c.Category.Description == "Âm Thanh" && c.AvailableQuantity > 0);
			return View(PaginatedList<Product>.Create(list.ToList(), pageNumber ?? 1, pageSize));
		}
		public IActionResult DongHo(int? pageNumber)
		{
			int pageSize = 8;
			var list = db.Products.Include(c => c.Category).Include(c => c.Capacity).Include(c => c.Supplier).Where(c => c.Category.Description == "Đồng hồ" && c.AvailableQuantity > 0);
			return View(PaginatedList<Product>.Create(list.ToList(), pageNumber ?? 1, pageSize));
		}
		public IActionResult HangCu(int? pageNumber)
		{
			int pageSize = 8;
			var list = db.Products.Include(c => c.Category).Include(c => c.Capacity).Include(c => c.Supplier).Where(c => c.Category.Description == "Hàng cũ" && c.AvailableQuantity > 0);
			return View(PaginatedList<Product>.Create(list.ToList(), pageNumber ?? 1, pageSize));
		}
		public IActionResult ProductDetail(Guid id)
		{
			var product = productsService.GetProductById(id);
			ViewBag.CapacityID = new SelectList(db.Capacities, "ID", "Capacitys", product.CapacityID);
			ViewBag.CapacityID = new SelectList(db.Capacities, "ID", "Capacitys", product.CategoryID);
			ViewBag.CapacityID = new SelectList(db.Capacities, "ID", "Capacitys", product.SupplierID);
			return View(product);
		}
		[HttpGet]
		public IActionResult Update(Guid id)
		{
			
			var p = productsService.GetProductById(id);
			ViewBag.CategoryID = new SelectList(db.Categorys, "ID", "Name", p.CategoryID);
			ViewBag.CapacityID = new SelectList(db.Capacities, "ID", "Capacitys", p.CapacityID);
			ViewBag.SupplierID = new SelectList(db.Suppliers, "ID", "NameSupplier", p.SupplierID);
			return View(p);
		}
		public IActionResult Update(Product p)
		{
			try
			{
				if (productsService.UpdateProduct(p))
				{
					return RedirectToAction("Index", "Home");
				}
				else
				{
					return BadRequest();
				}
			} catch
			{
				return RedirectToAction("Index", "Home");
			}
			

		}
		public IActionResult Delete(Guid id)
		{
			productsService.DeleteProduct(id);
			return RedirectToAction("Index");
		}
		public IActionResult AddToCart(Guid id)
		{
			try
			{
				ClaimsPrincipal claimsPrincipal = HttpContext.User;
				if (claimsPrincipal.Identity.IsAuthenticated) // check xem đã đăng nhập chưa 
				{
					List<Cart> cart = new List<Cart>();
					List<CartDetails> cartDetails1 = new List<CartDetails>();
					List<Product> product = new List<Product>();
					var user = HttpContext.User; // người dùng đăng nhập
					var email = user.FindFirstValue(ClaimTypes.Email); // lấy email của người dùng khi đăng nhập
					var IdUser = _iuser.GetAllUsers().Where(c => c.Email == email).Select(c => c.UserID).FirstOrDefault();
					var idproduct = productsService.GetProductById(id);
					if (_icartService.GetAllCarts().Any(c => c.UserID == IdUser) == false)
					{
						Cart newcart = new Cart()
						{
							UserID = IdUser,
							Description = "Newcart"
						};
					}
					var idgh = _icartService.GetCartById(IdUser);
					if (_icartdetal.GetCartDetail().Any(c => c.IDSp == id) == false)
					{
						CartDetails newcartdetail = new CartDetails()
						{
							ID = Guid.NewGuid(),
							IDSp = idproduct.ID,
							UserID = idgh.UserID,
							Quantity = 1,

						};
						_icartdetal.AddCartDetail(newcartdetail);
					}
					else
					{
						var soluong = cartDetails1.Where(c => c.IDSp == id).Select(c => c.Quantity).FirstOrDefault();
						CartDetails cartupdate = _icartdetal.GetCartDetail().FirstOrDefault(c => c.IDSp == id);
						cartupdate.Quantity = cartupdate.Quantity + 1;
						_icartdetal.UpdateCartDetail(cartupdate);
					}
					return RedirectToAction("ShowCart", "Home");
				}
				else
				{
					return RedirectToAction("Login", "Account");
				}
			} catch
			{
				return RedirectToAction("Index", "Home");
			}
			
		}

	}
}

