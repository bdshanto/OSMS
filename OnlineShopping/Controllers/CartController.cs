﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using OnlineShopping.Models.Data;
using OnlineShopping.Models.ViewModels.Cart;

namespace OnlineShopping.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            // Init the cart list
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // Check if cart is empty
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty";
                return View();
            }

            // Calculate total and save to ViewBag
            decimal total = 0m;

            foreach(var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            // Return view with list
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            // Init CartVM
            CartVM model = new CartVM();

            // Init quantity
            int qty = 0;

            // Init price
            decimal price = 0m;

            // Check for cart session
            if (Session["cart"] != null)
            {
                // Get total qty and price
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;
               
            } else
            {
                // Or set qty and price to 0
                model.Quantity = 0;
                model.Price = 0m;
            }
            
            // Return partial view with model
            return PartialView(model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            // Init CartVM list
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // Init CartVM
            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                // Get the product
                ProductDto product = db.Products.Find(id);

                // Check if the product is already in cart
                var productInCart = cart
                    .FirstOrDefault(x => x.ProductId == id);

                // If not, add new
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                } else
                {
                    // If it is, increment
                    productInCart.Quantity++;
                }

            }
            // Get total qty and price and add to model
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            // Save cart back to session
            Session["cart"] = cart;

            // Return partial view with model
            return PartialView(model);
        }

        // GET: /Cart/IncrementProduct 
        public JsonResult IncrementProduct(int productId)
        {
            // Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using(Db db = new Db())
            {
                // Get cartVM from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Increment qty
                model.Quantity++;

                // Store needed data
                var result = new { qty = model.Quantity, price = model.Price };

                // Return json with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Cart/DecrementProduct              
        public JsonResult DecrementProduct(int productId)
        {
            // Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Get cartVM from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Decrement qty
                if (model.Quantity > 1)
                {
                    model.Quantity--;
                } else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                // Store needed data
                var result = new { qty = model.Quantity, price = model.Price };

                // Return json with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Cart/RemoveProduct
        public void RemoveProduct(int productId)
        {
            // Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Get model from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Remove model from list
                cart.Remove(model);
            }
        }

        public ActionResult PaypalPartial()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            return PartialView(cart);
        }
        // post: /Cart/PlaceOrder
        [HttpPost]
        public void PlaceOrder()
        {
            //get the list 
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            // get username
            string username = User.Identity.Name;
            //declare order
            int orderId = 0;

            using (Db db = new Db())
            {
                //init OrderDto 
                OrderDto orderDto= new OrderDto();
                //get user id 
                var q = db.Users.FirstOrDefault(x => x.Username == username);
                int userId = q.Id;

                //add to orderDTO and save 
                //orderDto.OrderId = orderId;
                orderDto.UserId = userId;
                orderDto.CreatedAt=DateTime.Now;

                db.Orders.Add(orderDto);
                db.SaveChanges();
                //get inserted id
                orderId = orderDto.OrderId;
                //init orderDetailsDto
                OrderDetailsDto orderDetails = new OrderDetailsDto(); 
                //add to orderDetailsDto
                foreach (var item in cart)
                {
                    orderDetails.OrderId = orderId;
                    orderDetails.UserId = userId;
                    orderDetails.ProductId = item.ProductId;
                    orderDetails.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetails);
                    db.SaveChanges();
                     
                }
            }
            //email admin
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("fad7a9d4597328", "d2265eb88adc2f"),
                EnableSsl = true
            };
            client.Send("admin@example.com", "admin@example.com", "New Order", "you have a new order & order number is:-" + orderId);

            //reset session
            Session["cart"] = null;

        }
    }
}