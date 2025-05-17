using homework2.Data;
using homework2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;

namespace homework2.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // ======================
        // Display Login Page
        // ======================
        public IActionResult Login()
        {
            return View();
        }

        // ======================
        // Handle Login (POST)
        // ======================
        /*
         [HttpPost]
            public async Task<IActionResult> Login(string username, string password)
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("", "All fields are required.");
                    return View();
                }

                string hashedPassword = HashPassword(password);

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hashedPassword);

                if (user != null)
                {
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Role", user.Role);

                    if (user.Role == "Admin")
                        return RedirectToAction("AdminDashboard");
                    else
                        return RedirectToAction("UserDashboard");
                }

                ModelState.AddModelError("", "Invalid login attempt.");
                return View();
            }*/
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View();
            }

            string hashedPassword = HashPassword(password);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hashedPassword);

            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);

                return user.Role == "Admin"
                    ? RedirectToAction("AdminDashboard")
                    : RedirectToAction("UserDashboard");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View("Login");
        }



        // ======================
        // Display Register Page
        // ======================
        public IActionResult Register()
        {
            return View();
        }

        // ======================
        // Handle Register (POST)
        // ======================
        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View();
            }

            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                ModelState.AddModelError("", "Username already exists. Please choose another.");
                return View();
            }

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ModelState.AddModelError("", "Email already exists. Please choose another.");
                return View();
            }

            string hashedPassword = HashPassword(password);
            int usersCount = await _context.Users.CountAsync();
            string role = usersCount == 0 ? "Admin" : "StandardUser";

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = hashedPassword,
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            if (role == "Admin")
                return RedirectToAction("AdminDashboard");
            else
                return RedirectToAction("UserDashboard");
        }

        // ======================
        // Handle Logout
        // ======================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ======================
        // Display Forgot Password Page
        // ======================
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // ======================
        // Handle Forgot Password (POST)
        // ======================
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Please enter your email.");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                ModelState.AddModelError("", "You are not registered in the system.");
                return View();
            }

            // ✅ بدل ما نعرض رابط ➔ نحوله مباشر إلى صفحة ResetPassword
            return RedirectToAction("ResetPassword", new { username = user.Username });
        }

        // ======================
        // Display Reset Password Page
        // ======================
        public IActionResult ResetPassword(string username)
        {
            ViewBag.Username = username;
            return View();
        }

        // ======================
        // Handle Reset Password (POST)
        // ======================
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string username, string newPassword)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(newPassword))
            {
                ModelState.AddModelError("", "All fields are required.");
                ViewBag.Username = username;
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                ModelState.AddModelError("", "Username not found.");
                return View();
            }

            user.PasswordHash = HashPassword(newPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password updated successfully! Please login.";
            return RedirectToAction("Login");
        }

        // ======================
        // Admin Dashboard
        // ======================
        public IActionResult AdminDashboard()
        {
            if (HttpContext.Session.GetString("Username") == null ||
                HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login");

            var users = _context.Users.ToList();
            return View(users);  // 👈 نرسل القائمة إلى الـ View
        }


        public IActionResult UserDashboard()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login");

            return View(user); // هذا هو المهم
        }
        [HttpPost]
        public IActionResult UpdateProfile(User updatedUser)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("Login");

           

            if (!ModelState.IsValid)
            {
               
                foreach (var e in ModelState.Values.SelectMany(v => v.Errors))
                    Console.WriteLine(e.ErrorMessage);

                return View("UserDashboard", updatedUser);
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
             
                return View("UserDashboard", updatedUser);
            }

            user.FirstName = updatedUser.FirstName?.Trim();
            user.LastName = updatedUser.LastName?.Trim();
            user.NationalId = updatedUser.NationalId?.Trim();
            user.CreditCardNumber = updatedUser.CreditCardNumber?.Trim();
            user.ValidDate = updatedUser.ValidDate?.Trim();
            user.CVC = updatedUser.CVC?.Trim();

            _context.Entry(user).State = EntityState.Modified;
            int result = _context.SaveChanges();
           

            ViewBag.Success = "sucsees!";
            return View("UserDashboard", user);
        }



        // ======================
        // Helper: Hash Password using SHA-256
        // ======================
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

    }
}
