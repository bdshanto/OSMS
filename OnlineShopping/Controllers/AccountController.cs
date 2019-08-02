using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using OnlineShopping.Models.Data;
using OnlineShopping.Models.ViewModels.Account;
using OnlineShopping.Models.ViewModels.Shop;

namespace OnlineShopping.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account

        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

        // GET: account/login

        public ActionResult Login()
        {
            // Confirm user is not logged in
            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
                return RedirectToAction("user-profile");

            // Return view
            return View();
        }

        // POST: /account/login

        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Check if the user is valid
            bool isValid = false;

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }
            if (!isValid)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            } else
            {
                FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
            }
        }

        // GET: /account/create-account

        [ActionName("create-account")]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        // POST: /account/create-account

        [HttpPost]
        [ActionName("create-account")]
        public ActionResult CreateAccount(UserVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
                return View("CreateAccount", model);
            
            // Check if passwords match
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                // Make sure username is unique
                if (db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", "Username" + model.Username + " is taken.");
                    model.Username = "";
                    return View("CreateAccount", model);
                }
                // Create userDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    Username = model.Username,
                    Password = model.Password
                };

                // Add the DTO
                db.Users.Add(userDTO);

                // Save
                db.SaveChanges();

                // Add to userRolesDTO
                int id = userDTO.Id;

                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
            }

            // Create a TempData message
            TempData["SM"] = "You are now registered and can login.";

            // Redirect
            return Redirect("~/account/login");
        }

        // GET: /account/logout

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }
        [Authorize]
        public ActionResult UserNavPartial()
        {
            // Get username
            string username = User.Identity.Name;

            // Declare model
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                // Get the user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);
                // Build the model
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }

            // Return partial view with model
            return PartialView(model);
        }

        // GET: /acount/user-profile

        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            // Get username
            string username = User.Identity.Name;

            // Declare model
            UserProfileVM model;

            using (Db db = new Db())
            {
                // Get user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                // Build model
                model = new UserProfileVM(dto);
            }
            // Return view with model
            return View("UserProfile", model);
        }

        // GET: /acount/user-profile

        [HttpPost]
        [Authorize]
        [ActionName("user-profile")]
        public ActionResult UserProfile(UserProfileVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            // Check if passwords match if need be
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Passwords do not match.");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                // Get username
                string username = User.Identity.Name;

                // Make sure username is unique
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == username))
                {
                    ModelState.AddModelError("", "Username " + model.Username + " already exists.");
                    model.Username = "";
                    return View("UserProfile", model);
                }

                // Edit DTO
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.Username = model.Username;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                // Save
                db.SaveChanges();
            }

            // Set TempData message
            TempData["SM"] = "You have edited your profile!";

            // Redirect
            return Redirect("~/account/user-profile");
        }

        // GET: /account/Orders

        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            //init list of orderuserVM
            List<OrderForUserVM> orderForUser = new List<OrderForUserVM>();

            using (Db db = new Db())
            {
                //get id
                UserDTO userDto = db.Users.FirstOrDefault(x => x.Username == User.Identity.Name);
                int userId = userDto.Id;
                //init list order VM 
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x))
                    .ToList();
                //loop  through list of orderVM
                foreach (var order in orders)
                {
                    //init products 
                    Dictionary<string,int> produtAndQty= new Dictionary<string, int>();
                    //declar total
                    decimal total = 0m;

                    //init list of orderdetailsDTO
                    List<OrderDetailsDTO> orderDetailsDtos =
                        db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();
                    //loop though list of orderdetailsDTO
                    foreach (var orderDetails in orderDetailsDtos)
                    {
                        //get products
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);
                        //get product price
                        decimal price = product.Price;
                        //get product name
                        string productName = product.Name;
                        //add to product dict
                        produtAndQty.Add(productName,orderDetails.Quantity);
                     
                        //get total
                        total += orderDetails.Quantity * price;
                    } 
                    //add to orderForUserVM List
                        orderForUser.Add(new OrderForUserVM()
                        {
                            OrderNumber = order.OrderId,
                            Total = total,
                            ProductAndQty = produtAndQty,
                            CratedAt = order.CreatedAt
                        }); 
                }
            }
            return View(orderForUser);
        }
    }
}