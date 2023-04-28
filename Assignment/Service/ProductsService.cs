
using Assignment.IServices;
using Assignment.Models.Data;

namespace Assignment.Service
{
    public class ProductsService : IProductsService
	{
		public Shopping_Dbcontext _context;
		public ProductsService()
		{
			_context = new Shopping_Dbcontext();
		}
		public bool CreateProduct(Product p)
		{
			try
			{
				_context.Products.Add(p); // add vao Dbset
				_context.SaveChanges();
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		public bool DeleteProduct(Guid id)
		{
			try
			{
				var product = _context.Products.Find(id);
				_context.Products.Remove(product); // xoa khoi Db theo id
				_context.SaveChanges();
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		public List<Product> GetAllProducts()
		{
			return _context.Products.ToList();
		}

		public Product GetProductById(Guid id)
		{
			return _context.Products.FirstOrDefault(c => c.ID == id);
		}

		public List<Product> GetProductByName(string name)
		{
			return _context.Products.Where(c => c.NameProduct.Contains(name)).ToList();
		}

		public bool UpdateProduct(Product p)
		{
			try
			{
				var product = _context.Products.Find(p.ID);
				product.NameProduct = p.NameProduct;
				product.Description = p.Description;

				product.Price = p.Price;

				product.AvailableQuantity = p.AvailableQuantity;
				product.SupplierID = p.SupplierID;
				product.CapacityID = p.CapacityID;
				product.CategoryID = p.CategoryID;
				product.Description = p.Description;
				product.Status = p.Status;
				product.Supplier = p.Supplier;
				product.Image = p.Image;
				_context.Products.Update(product);
				_context.SaveChanges();
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
	}
}
