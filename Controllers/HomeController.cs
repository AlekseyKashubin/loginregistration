using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using loginregistration.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;

namespace loginregistration.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private MyContext db;

    public HomeController(ILogger<HomeController> logger, MyContext context)
    {
        _logger = logger;
        db = context;
    }


    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }

    // ===========================Create Method==================================

    [HttpPost("user/create")]
    public IActionResult Register(User newUser)
    {
        if (!ModelState.IsValid)
        {
            return View("Index");
        }
        PasswordHasher<User> hashBrowns = new PasswordHasher<User>();
        newUser.Password = hashBrowns.HashPassword(newUser, newUser.Password);
        db.Users.Add(newUser);
        db.SaveChanges();
        HttpContext.Session.SetInt32("UUID", newUser.UserId);
        return RedirectToAction("Success");
    }


    // [SessionCheck]
    [HttpGet("success")]
    public IActionResult Success()
    {
        if (HttpContext.Session.GetInt32("UUID") == null)
        {
            return RedirectToAction("Index");
        }
        return View("Success");
    }



    [HttpPost("users/login")]
    public IActionResult Login(LoginUser loginUser)
    {
        if (ModelState.IsValid)
        {
            User? userInDb = db.Users.FirstOrDefault(u => u.Email == loginUser.LoginEmail);
            if (userInDb == null)
            {
                ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                return View("Index");
            }
            PasswordHasher<LoginUser> hashBrown = new PasswordHasher<LoginUser>();
            var result = hashBrown.VerifyHashedPassword(loginUser, userInDb.Password, loginUser.LoginPassword);
            if (result == 0)
            {
                ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                return View("Index");
            }
            else
            {
                HttpContext.Session.SetInt32("UUID", userInDb.UserId);
                return RedirectToAction("Success");
            }
        }
        else
        {
            return View("Index");
        }
    }


    [HttpGet("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

// Name this anything you want with the word "Attribute" at the end
public class SessionCheckAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {

        int? userId = context.HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {

            context.Result = new RedirectToActionResult("Index", "Home", null);
        }
    }
}


