using Assignment.Models.Data;
using Assignment.Models;
using Assignment.Service;
using Microsoft.AspNetCore.Mvc;
using Assignment.IServices;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private Shopping_Dbcontext db = new Shopping_Dbcontext();
		private IProductsService productsService;
		private ICapacityService capacityService;
		private ICategoryService categoryService;
		private ISupplierService supplierService;
		private IUserService _iuser;
		private ICapacityService _icap;

		private ICartService _icartService;
		private ICartDetialsService _icartdetal;
		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
			productsService = new ProductsService();
			capacityService = new CapacityService();
			categoryService = new CategoryService();
			supplierService = new SupplierService();
			_iuser = new UserService();
			_icartService = new CartService();
			_icap = new CapacityService();
			_icartdetal = new CartDetailsService();
		}
		[HttpGet]
		public IActionResult Index(int? pageNumber)
		{
			int pageSize = 8;


			var list = db.Products.Include(c => c.Category).Include(c => c.Capacity).Include(c => c.Supplier).Where(c => c.AvailableQuantity > 0);


			return View(PaginatedList<Product>.Create(list.ToList(), pageNumber ?? 1, pageSize));
			//return View(list.ToList());
		}
		[HttpGet]
		public IActionResult Search(string name)
		{
			var show = productsService.GetProductByName(name);
			return View(show.ToList());
		}
		public IActionResult About()
		{
			return View();
		}
		public IActionResult Contact()
		{
			return View();
		}
		public IActionResult ProductDetail(Guid id)
		{
			var product = productsService.GetProductById(id);
			ViewBag.CapacityID = _icap.GetCapacity().Where(c => c.ID == product.CapacityID).Select(c => c.Capacitys).FirstOrDefault();
			return View(product);
		}
	}
}
