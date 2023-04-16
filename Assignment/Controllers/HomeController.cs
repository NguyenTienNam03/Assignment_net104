using Assignment.IServices;
using Assignment.Models;
using Assignment.Models.Data;
using Assignment.Models.ViewModel;
using Assignment.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Security.Claims;

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
		public IActionResult ProductList()
		{
			var list = db.Products.Include(c => c.Category).Include(c => c.Capacity).Include(c => c.Supplier);
			return View(list.ToList());
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
		//[Authorize(Roles = "Admin")]
		public IActionResult CreateProduct()
		{
			try
			{
				ClaimsPrincipal claimsPrincipal = HttpContext.User;
				if (claimsPrincipal.Identity.IsAuthenticated) // check xem đã đăng nhập chưa 
				{
					ViewBag.CategoryID = new SelectList(db.Categorys, "ID", "Name");
					ViewBag.CapacityID = new SelectList(db.Capacities, "ID", "Capacitys");
					ViewBag.SupplierID = new SelectList(db.Suppliers, "ID", "NameSupplier");

				}
				else
				{
					return RedirectToAction("Login", "Account");
				}
				return View();
			} catch
			{
				return RedirectToAction("Index", "Home");
			}
			
		}
		[HttpPost]
		//[Authorize(Roles = "Admin")]
		public IActionResult CreateProduct(Product products)
		{
			try
			{
				ViewBag.CategoryID = new SelectList(db.Categorys, "ID", "Name", products.CategoryID);
				ViewBag.CapacityID = new SelectList(db.Capacities, "ID", "Capacitys", products.CapacityID);
				ViewBag.SupplierID = new SelectList(db.Suppliers, "ID", "NameSupplier", products.SupplierID);
				products.Status = 1;
				productsService.CreateProduct(products);
				return RedirectToAction("Index", "Home");
			} catch
			{
				return RedirectToAction("Index", "Home");
			}
			
		}

		[HttpGet]
		//[Authorize(Roles = "Admin")]
		public IActionResult Update(Guid id)
		{
			try
			{
				ClaimsPrincipal claimsPrincipal = HttpContext.User;
				if (claimsPrincipal.Identity.IsAuthenticated) // check xem đã đăng nhập chưa 
				{
					var p = productsService.GetProductById(id);
					ViewBag.CategoryID = new SelectList(db.Categorys, "ID", "Name", p.CategoryID);
					ViewBag.CapacityID = new SelectList(db.Capacities, "ID", "Capacitys", p.CapacityID);
					ViewBag.SupplierID = new SelectList(db.Suppliers, "ID", "NameSupplier", p.SupplierID);
					return View(p);
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
		//[Authorize(Roles = "Admin")]
		[HttpPost]	
		public IActionResult Update(Product p)
		{
			try
			{
				if (p.AvailableQuantity > 0)
				{
					if (productsService.UpdateProduct(p))
					{
						return RedirectToAction("Index");
					}
					else
					{
						return BadRequest();
					}
				}
				else
				{
					p.Status = 0;
					productsService.UpdateProduct(p);
					return RedirectToAction("Index");
				}
			} catch
			{
				return RedirectToAction("Index", "Home");
			}
		}
		//[Authorize(Roles = "Admin")]
		public IActionResult Delete(Guid id)
		{
			try
			{
				ClaimsPrincipal claimsPrincipal = HttpContext.User;
				if (claimsPrincipal.Identity.IsAuthenticated) // check xem đã đăng nhập chưa 
				{
					productsService.DeleteProduct(id);
					return RedirectToAction("Index");
				}
				else
				{
					return RedirectToAction("Login", "Account");
				}
			}
			catch
			{
				return RedirectToAction("Index", "Home");
			}
			
				
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
						_icartService.AddCart(newcart);
					}
					var idgh = _icartService.GetCartById(IdUser);
					if (_icartdetal.GetCartDetail().Any(c => c.IDSp == id) == false)
					{
						CartDetails newcartdetail = new CartDetails()
						{
							ID = Guid.NewGuid(),
							IDSp = idproduct.ID,
							UserID = idgh.UserID,
							Price = idproduct.Price,
							Quantity = 1,

						};
						
						_icartdetal.AddCartDetail(newcartdetail);
						
					}
					else
					{
						var soluong = cartDetails1.Where(c => c.IDSp == id).Select(c => c.Quantity).FirstOrDefault();
						CartDetails cartupdate = _icartdetal.GetCartDetail().FirstOrDefault(c => c.IDSp == id);
						cartupdate.Quantity = cartupdate.Quantity + 1;
                        //if (cartupdate.Quantity > soluong)
                        //{
                        //    ViewBag.Soluonghon = "Số lượng tồn không đủ với số lượng bạn yêu cầu.";
                        //}
                        //else
                        //{
                            _icartdetal.UpdateCartDetail(cartupdate);
                        //}
                        
					}
					return RedirectToAction("ShowCart", "Account");
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
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}