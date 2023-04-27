using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Assignment.Models.Data;
using System.Security.Claims;
using Assignment.IServices;
using Assignment.Service;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Collections.Immutable;
using System.Collections.Generic;
using NuGet.Packaging.Signing;
using Assignment.Validate;
using Assignment.Areas.Admin.Data.Data;

namespace Assignment.Controllers
{
    public class AccountController : Controller
	{
		private readonly ILogger<AccountController> _logger;

		private Shopping_Dbcontext db = new Shopping_Dbcontext();
		private IProductsService productsService;
		private ICapacityService capacityService;
		private ICategoryService categoryService;
		private ISupplierService supplierService;
		private IUserService _iuser;
		private IBillSerivce _ibillservice;
		private IBillDetialsService _ibilldetailservice;
		private ICartService _icartService;
		private ICartDetialsService _icartdetal;
		private Check _check;
		public AccountController(ILogger<AccountController> logger)
		{
			_logger = logger;
			productsService = new ProductsService();
			capacityService = new CapacityService();
			categoryService = new CategoryService();
			supplierService = new SupplierService();
			_iuser = new UserService();
			_icartService = new CartService();
			_ibillservice = new BillService();
			_check = new Check();
			_ibilldetailservice = new BillDetailService();
			_icartdetal = new CartDetailsService();
		}

		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return RedirectToAction("Login", "Account");
		}
		public IActionResult RemoveItem(Guid id)
		{
			try
			{
				var layid = _icartdetal.GetCartDetail().FirstOrDefault(c => c.ID == id);
				if (layid.ID == id)
				{
					_icartdetal.DeleteCartDetail(id);
				}
				else
				{
					ViewBag.Erorr = "Mời bạn thao tác lại";
				}
				return RedirectToAction("ShowCart", "Account");
			}
			catch
			{
				return BadRequest();
			}
		}

		public IActionResult ShowCart()
		{
			ClaimsPrincipal claimsPrincipal = HttpContext.User;
			if (claimsPrincipal.Identity.IsAuthenticated) // check xem đã đăng nhập chưa 
			{
				var user = HttpContext.User; // người dùng đăng nhập
				var email = user.FindFirstValue(ClaimTypes.Email); // lấy email của người dùng khi đăng nhập
				var IdUser = _iuser.GetAllUsers().Where(c => c.Email == email).Select(c => c.UserID).FirstOrDefault();
				var idgh = _icartService.GetCartById(IdUser);
				if(idgh == null)
				{
					return RedirectToAction("ShowCart", "Account");
				} else
				{
					var ShoppingCart = db.CartDetails.Include(c => c.Product).Include(c => c.Cart).Where(c => c.UserID == idgh.UserID);
					//List<CartDetails> ShoppingCart = _icartdetal.GetCartDetail().Where(c => c.UserID == idgh.UserID).ToList();
					if (ShoppingCart != null)
					{
						ViewBag.id = IdUser;
						return View(ShoppingCart.ToList());
					}
					else
					{
						//return Content("Giỏ hàng không có sản phẩm");
						return RedirectToAction("ShowCart", "Account");
					}
				}
				//return RedirectToAction("ShowCart", "Account");
			}
			else
			{
				return RedirectToAction("Login", "Account");
			}
		}

		public IActionResult UpdateAmount(Guid ID, int Quantity)
		{
			var idcartdeatil = _icartdetal.GetCartDetailById(ID);
			var spluongsanpham = productsService.GetAllProducts().Where(c => c.ID == idcartdeatil.IDSp).Select(c => c.AvailableQuantity).FirstOrDefault();
			if(spluongsanpham >= Quantity)
			{
				var cartdetail = new CartDetails()
				{
					ID = ID,
					IDSp = idcartdeatil.IDSp,
					UserID = idcartdeatil.UserID,
					Quantity = Quantity,
				};
				_icartdetal.UpdateCartDetail(cartdetail);
				return RedirectToAction("ShowCart", "Account");
			}else
			{
				ViewBag.soluong = "Số lượng tồn của sản phẩm không đủ với số lượng bạn yêu cầu.";
				return RedirectToAction("ShowCart", "Account");
			}
		
		}
		public IActionResult Register()
		{
			return View();
		}
		[HttpPost]
		public IActionResult Register(User user)
		{
			try
			{
				var CheckEmail = db.Users.FirstOrDefault(c => c.Email == user.Email);

				if (db.Users.Any(c => c.Email == user.Email) == true)
				{
					ViewBag.error = "Email da ton tai.";
					return View();
					//return RedirectToAction("Index");
				}
				else if (_check.CheckSDT(user.PhoneNumber) == false)
				{
					ViewBag.ValidatePhone = "Số điện thoại gồm có 10 số và có số 0 ở đầu.";
					return View();
				}
				else if (_iuser.GetAllUsers().Any(c => c.PhoneNumber == user.PhoneNumber) == true)
				{
					ViewBag.phonenumber = "SĐT này đã được đăng kí.";
					return View();
				}
				else
				{

					user.Status = 1;
					user.IDRole = db.Roles.Where(c => c.RoleName == "Customer").Select(c => c.IdRole).FirstOrDefault();
					_iuser.AddUser(user);
					return RedirectToAction("Login", "Account");

				}
			} catch
			{
				ViewBag.Resgister = "Bạn nhập đầy đủ thông tin";
				return View();
			}
			
		}

		[HttpGet]
		public IActionResult Login()
		{
			ClaimsPrincipal claimsPrincipal = HttpContext.User;
			if (claimsPrincipal.Identity.IsAuthenticated)
			{
				return RedirectToAction("Index", "Home");
			}
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Login(User user)
		{
			var laymail = _iuser.GetAllUsers().FirstOrDefault(c => c.Email == user.Email);
			var laypass = _iuser.GetAllUsers().Where(c => c.Email == user.Email).Select(c => c.Password).FirstOrDefault();
			if (_iuser.GetAllUsers().Any(c => c.Email == user.Email) == true)
			{
				if (laymail.Email == user.Email && laypass == user.Password)
				{
					List<Claim> claims = new List<Claim>()
					{
						new Claim(ClaimTypes.Email, user.Email),
						new Claim("OtherProperties", "Example Role")
					};
					ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims,
						CookieAuthenticationDefaults.AuthenticationScheme);
					AuthenticationProperties properties = new AuthenticationProperties()
					{
						AllowRefresh = true,

					};
					await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
					new ClaimsPrincipal(claimsIdentity), properties);

					return RedirectToAction("Index", "Home");
				}
				else
				{
					ViewBag.ErrorMessage = "Sai email hoặc password";
					return View();
				}
			}
			else if (user.Email == null || user.Password == null)
			{
				ViewBag.Error = "Mời nhập Email và Password";
				return View();
			}
			else
			{
				ViewBag.ErrorMail = "Email chưa được đăng ký";
				return View();
			}
		}
		public IActionResult Profile()
		{
			ClaimsPrincipal claimsPrincipal = HttpContext.User;
			if (claimsPrincipal.Identity.IsAuthenticated) // check xem đã đăng nhập chưa 
			{
				var user = HttpContext.User; // người dùng đăng nhập
				var email = user.FindFirstValue(ClaimTypes.Email); // lấy email của người dùng khi đăng nhập
				var IdUser = _iuser.GetAllUsers().Where(c => c.Email == email).Select(c => c.UserID).FirstOrDefault();
				var show = _iuser.GetUserById(IdUser);
				return View(show);

			}
			else
			{
				return RedirectToAction("Login", "Account");
			}
		}
		[HttpGet]
		public IActionResult UpdateProfile(Guid id)
		{
			var show = _iuser.GetUserById(id);
			return View(show);
		}
		[HttpPost]
		public IActionResult UpdateProfile(User user)
		{
			try
			{
				if (_iuser.UpdateUser(user))
				{
					return RedirectToAction("Profile", "Account");
				}
				else
				{
					return BadRequest();
				}
			}
			catch 
			{
				ViewBag.Error = "Update thong tin that bai.";
			}
			return RedirectToAction("Profile", "Account");
		}

		public IActionResult Bill()
		{
			ClaimsPrincipal claimsPrincipal = HttpContext.User;
			if (claimsPrincipal.Identity.IsAuthenticated) // check xem đã đăng nhập chưa 
			{
				var user = HttpContext.User; // người dùng đăng nhập
				var email = user.FindFirstValue(ClaimTypes.Email); // lấy email của người dùng khi đăng nhập
				var IdUser = _iuser.GetAllUsers().Where(c => c.Email == email).Select(c => c.UserID).FirstOrDefault();
				var bill = _ibillservice.GetBillList().Where(c => c.UserId == IdUser).OrderByDescending(c => c.MaHD).ToList();

				return View(bill);

			}
			else
			{
				return RedirectToAction("Login", "Account");
			}
		}

		public IActionResult CreateBill(Guid id)
		{
			
			var soluongsp = _icartdetal.GetCartDetail().Where(c => c.UserID == id);
			ViewBag.soluong = soluongsp.Count();
			if(soluongsp.Count() > 0)
			{
				//ClaimsPrincipal claimsPrincipal = HttpContext.User;
				//if (claimsPrincipal.Identity.IsAuthenticated) // check xem đã đăng nhập chưa 
				//{
				List<Product> products = new List<Product>();
				//var user = HttpContext.User; // người dùng đăng nhập
				//var email = user.FindFirstValue(ClaimTypes.Email); // lấy email của người dùng khi đăng nhập
				//var IdUser = _iuser.GetAllUsers().Where(c => c.Email == email).Select(c => c.UserID).FirstOrDefault();
				var checkid = _iuser.GetUserById(id);
				var IdUser = _iuser.GetAllUsers().FirstOrDefault(c => c.UserID == checkid.UserID);
				var cartdetails = _icartdetal.GetCartDetail().Where(c => c.UserID == id).ToList();

				// Tạo hoá đơn
				Bill bill1 = new Bill()
				{
					ID = Guid.NewGuid(),
                    UserId = IdUser.UserID,
					MaHD = "HD" + Convert.ToString(_ibillservice.GetBillList().Count() + 1),
					CreateDate = DateTime.Now,
					Receiveddate = DateTime.Now.AddDays(3),
					Status = 0,
				};
				_ibillservice.AddBill(bill1);

				// thêm sản phẩm vào hoá đơn vừa tạo
				foreach (var cartdetail in cartdetails)
				{
					var gia = productsService.GetAllProducts().Where(c => c.ID == cartdetail.IDSp).Select(c => c.Price).FirstOrDefault();
					var billDetail = new BillDetail()
					{
						Id = Guid.NewGuid(),
						IdHD = bill1.ID,
						IdSp = cartdetail.IDSp,
						Price = cartdetail.Price,
						Quantity = cartdetail.Quantity,
					};
					_ibilldetailservice.AddBillDetails(billDetail);

					// Cap nhat lai so luong san pham
					var Sanphamgoc = productsService.GetAllProducts().FirstOrDefault(c => c.ID == cartdetail.IDSp);
					int soluongcon = Convert.ToInt32(Sanphamgoc.AvailableQuantity) - cartdetail.Quantity;
					if(soluongcon > 0)
					{
						var pro = new Product()
						{
							ID = Sanphamgoc.ID ,
							CapacityID = Sanphamgoc .CapacityID,
							CategoryID = Sanphamgoc.CategoryID,
							SupplierID = Sanphamgoc.SupplierID,
							NameProduct = Sanphamgoc.NameProduct,
							Image = Sanphamgoc.Image,
							Color = Sanphamgoc.Color,
							Status = Sanphamgoc.Status,
							Description = Sanphamgoc.Description,
							Features = Sanphamgoc.Features,
							Price = Sanphamgoc.Price,
							AvailableQuantity = soluongcon
						};
						productsService.UpdateProduct( pro);
					} 
					else
					{
						var pro = new Product()
						{
							ID = Sanphamgoc.ID,
							CapacityID = Sanphamgoc.CapacityID,
							CategoryID = Sanphamgoc.CategoryID,
							SupplierID = Sanphamgoc.SupplierID,
							NameProduct = Sanphamgoc.NameProduct,
							Image = Sanphamgoc.Image,
							Color = Sanphamgoc.Color,
							Status = 0,
							Description = Sanphamgoc.Description,
							Features = Sanphamgoc.Features,
							Price = Sanphamgoc.Price,
							AvailableQuantity = soluongcon
						};
						productsService.UpdateProduct(pro);
					}
					_icartdetal.DeleteCartDetail(cartdetail.ID);
				}
				
				return RedirectToAction("Bill", "Account");
			} else
			{
				ViewBag.Thongbao = "Gio hang khong the thanh toan khi chua co san pham? ";
				return View();
			}
			
		}
		public IActionResult BillDetail(Guid id)
		{
			ClaimsPrincipal claimsPrincipal = HttpContext.User;
			if (claimsPrincipal.Identity.IsAuthenticated) // check xem đã đăng nhập chưa 
			{
				var user = HttpContext.User; // người dùng đăng nhập
				var email = user.FindFirstValue(ClaimTypes.Email); // lấy email của người dùng khi đăng nhập
				var IdUser = _iuser.GetAllUsers().Where(c => c.Email == email).Select(c => c.UserID).FirstOrDefault();

				var idhd = db.BillDetails.Include(c => c.Bills).Include(c => c.Product).Where(c => c.IdHD == id).ToList();

				return View(idhd);

			}
			else
			{
				return RedirectToAction("Login", "Account");
			}
		}
		public IActionResult HuyHang(Guid id)
		{
			var idhd = _ibillservice.GetBillByID(id);
			var bill = new Bill()
			{
				ID = id,
				UserId = idhd.UserId,
				MaHD = idhd.MaHD,
				CreateDate = idhd.CreateDate,
				Receiveddate = idhd.Receiveddate,
				Status = 1,
			};

			var billdetails = _ibilldetailservice.GetBillDetails().Where(c => c.IdHD == idhd.ID);

			foreach(var billdetail in billdetails.ToList())
			{
				var idsp = productsService.GetProductById(billdetail.IdSp);
				var soluonginbilldetail = _ibilldetailservice.GetBillDetails().Where(c => c.IdSp == idsp.ID).Select(c => c.Quantity).FirstOrDefault();
				var tongsoluong = idsp.AvailableQuantity + soluonginbilldetail;
				var product = new Product()
				{
					ID = idsp.ID,
					CapacityID = idsp.CapacityID,
					CategoryID = idsp.CategoryID,
					SupplierID = idsp.SupplierID,
					AvailableQuantity = tongsoluong,
					NameProduct = idsp.NameProduct,
					Status = idsp.Status,
					Description = idsp.Description,
					Features = idsp.Features,
					Price = idsp.Price,
					Image = idsp.Image,
					Color = idsp.Color,
				};
				productsService.UpdateProduct(product);
			}
			_ibillservice.UpdateBill(bill);
			return RedirectToAction("Bill", "Account");
		}
		public IActionResult NhanHang(Guid id)
		{
			var idhd = _ibillservice.GetBillByID(id);

			var bill = new Bill()
			{
				ID = id,
				UserId = idhd.UserId,
				MaHD = idhd.MaHD,
				CreateDate = idhd.CreateDate,
				Receiveddate = idhd.Receiveddate,
				Status = 2,
			};
			_ibillservice.UpdateBill(bill);
			return RedirectToAction("Bill", "Account");
		}
	}
}
