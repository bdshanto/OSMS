using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web.Mvc;
using OnlineShopping.Models.Data;
using OnlineShopping.Models.ViewModels.Shop;

namespace OnlineShopping.Controllers
{
    public class ShopController : Controller
    {
        //Default
        public ActionResult Dashboard()
        {
            List<ProductDto> proDto; 
            using (Db db = new Db())
            {
                proDto = db.Products.ToList();

            }

            // Return view with list
            return View(proDto);
        }
        //New arrival partial
    
        public ActionResult NewArrivalPartial()
        {
            IOrderedQueryable<ProductDto> proDto;
            using (Db db = new Db())
            {
                proDto = db.Products.OrderByDescending(t => t.Id);
            }

            // Return view with list
            return View(proDto);
        }
        
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            // Declare list of CategoryVM
            List<CategoryVM> categoryVmList;

            // Init the list
            using (Db db = new Db())
            {
                categoryVmList = db.Categories
                    .ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new CategoryVM(x))
                    .ToList();

            }

            // Return partial with list
            return PartialView(categoryVmList);
        } 

        // GET: /shop/category/name
        public ActionResult Category(string name)
        {
            // Declare a list of ProductVM
            List<ProductVM> ProductVMList;

            using (Db db = new Db())
            {
                // Get category id
                CategoryDTO categoryDTO = db.Categories
                    .Where(x => x.Slug == name)
                    .FirstOrDefault();

                int catId = categoryDTO.Id;
                // Init the list
                ProductVMList = db.Products
                    .ToArray()
                    .Where(x => x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();

                // Get category name
                var productCat = db.Products
                    .Where(x => x.CategoryId == catId)
                    .FirstOrDefault();

                ViewBag.CategoryName = productCat.CategoryName;
            }

            // Return view with list

            return View(ProductVMList);
        }

        // GET: /shop/product-details/name
        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            // Declare the VM and DTO
            ProductVM model;
            ProductDto dto;

            // Init product id
            int id = 0;

            using (Db db = new Db())
            {
                // Check if product exists
                if (! db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }

                // Init productDTO
                dto = db.Products.FirstOrDefault(x => x.Slug == name);

                // Get  id
                id = dto.Id;

                // Init model
                model = new ProductVM(dto);
            }

            // Get gallery images
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                .Select(fn => Path.GetFileName(fn));

            // Return view with model
            return View("ProductDetails", model);
        }
    }
}