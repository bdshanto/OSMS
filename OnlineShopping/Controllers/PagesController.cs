using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineShopping.Models.Data;
using OnlineShopping.Models.ViewModels.Pages;

namespace OnlineShopping.Controllers
{
    public class PagesController : Controller
    {
        // GET: Index/{page}
        public ActionResult Index(string page = "")
        {
            // Get/Set page slug
            if (page == "")
                page = "home";

            // Declare model and DTO
            PageVM model;
            PageDTO dto;

            // Check if page exists
            using (Db db = new Db())
            {
                if (! db.Pages.Any(x => x.Slug.Equals(page)))
                {
                    return RedirectToAction("Index", new { page = "" });
                }
            }

            // Get page DTO
            using (Db db = new Db())
            {
                dto = db.Pages.FirstOrDefault(x => x.Slug == page);
            }

            // Set page title
            ViewBag.PageTitle = dto.Title;

            // Check for sidebar
            ViewBag.Sidebar = dto.HasSidebar == true ? "Yes" : "No";

            // Init model
            model = new PageVM(dto);

            // Return view with model
            return View(model);
        }

        public ActionResult PagesMenuPartial()
        {
            // Declare list of PageVM
            List<PageVM> pageVmList;

            // Get all pages except home
            using (Db db = new Db())
            {
                pageVmList = db.Pages
                    .ToArray()
                    .OrderBy(x => x.Sorting)
                    .Where(x => x.Slug != "home")
                    .Select(x => new PageVM(x))
                    .ToList();
            }

            // Return partial view with list
            return PartialView(pageVmList);
        }
        

        public ActionResult SidebarPartial()
        {
            // Declare model
            SidebarVM model;

            // Init model
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebar.Find(1);

                model = new SidebarVM(dto);
            }

            // Return Partial view with model
            return PartialView(model);
        }
    }
}