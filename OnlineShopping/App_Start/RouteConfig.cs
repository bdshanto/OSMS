﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OnlineShopping
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Account",
                "Account/{action}/{id}",
                new { controller = "Account", action = "Index", id = UrlParameter.Optional },
                new[] { "OnlineShopping.Controllers" }
            );

            routes.MapRoute(
                "Cart",
                "Cart/{action}/{id}",
                new { controller = "Cart", action = "Index", id = UrlParameter.Optional },
                new[] { "OnlineShopping.Controllers" }
            );

            routes.MapRoute(
                "Shop",
                "Shop/{action}/{name}",
                new { controller = "Shop", action = "Index", name = UrlParameter.Optional },
                new[] { "OnlineShopping.Controllers" }
            );

            routes.MapRoute(
                "SidebarPartial",
                "Pages/SidebarPartial",
                new { controller = "Pages", action = "SidebarPartial" },
                new[] { "OnlineShopping.Controllers" }
            );

            routes.MapRoute(
                "PagesMenuPartial",
                "Pages/PagesMenuPartial",
                new { controller = "Pages", action = "PagesMenuPartial" },
                new[] { "OnlineShopping.Controllers" }
            );

            routes.MapRoute(
                "Default",
                "",
                new { controller = "Shop", action = "Dashboard"},
                new[] { "OnlineShopping.Controllers" }
            );

            routes.MapRoute(
                "Pages",
                "{page}",
                new { controller = "Pages", action = "Index" },
                new[] { "OnlineShopping.Controllers" }
            );
            routes.MapRoute(
                "Dashboard",
                "{Dashboard}",
                new { controller = "Dashboard", action = "Shop" },
                new[] { "OnlineShopping.Controllers" }
            );

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);
        }
    }
}
