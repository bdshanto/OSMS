using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineShopping.Areas.Admin.Models.ViewModels.Shop;
using OnlineShopping.Models.Data;
using OnlineShopping.Models.ViewModels.Shop;
 using Rotativa;
using Rotativa.MVC;

namespace OnlineShopping.Areas.Admin.Controllers
{
    public class DashboardController : Controller
    {
        
        // GET: Admin/Dashboard 
       
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            List<OrderForAdminVM> _orderForAdmin;
            //init list of ordersfor admin
            _orderForAdmin = new List<OrderForAdminVM>();

            using (Db db = new Db())
            {
                //init list order vm
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();
                //loop through list of ordervm
                foreach (var order in orders)
                {
                    //init product
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();
                    //declare total
                    decimal total = 0m;
                    //init list of orderdetailsdto
                    List<OrderDetailsDto> orderDetailsList =
                        db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();
                    //get username 
                    UserDTO user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();
                    string userName = user.Username;
                    //loop through list of orderforadmin list 
                    foreach (var orderDetails in orderDetailsList)
                    {
                        // Get product
                        var product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        // Get product price
                        var price = product.Price;

                        // Get product name
                        var productName = product.Name;

                        // Add to product dict
                        productAndQty.Add(productName, orderDetails.Quantity);

                        // Get total
                        total += orderDetails.Quantity * price;
                    }
                    //add to order admin list 
                    _orderForAdmin.Add(new OrderForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        UserName = userName,
                        Total = total,
                        ProductAndQty = productAndQty,
                        CratedAt = order.CreatedAt

                    });
                }
            }
            //return view with the ordersforadminVM List
            return View(_orderForAdmin);
        }
       
        //print 
        public ActionResult PrintOrder()
        {
            var q = new ActionAsPdf("Index");
            return q;
        }

        // GET: Admin/Dashboard/DeleteOrder/id
        public ActionResult DeleteOrder(int id)
        {
            using (Db db = new Db())
            {
                // Get the order
                OrderDetailsDto order = db.OrderDetails.Find(id);
                // Remove the order
                db.OrderDetails.Remove(order);

                // Save
                db.SaveChanges();

                OrderDto dto = db.Orders.Find(id);
                db.Orders.Remove(dto);
                db.SaveChanges();
            }
            // Redirect
            return RedirectToAction("Index");
        }
    }
}