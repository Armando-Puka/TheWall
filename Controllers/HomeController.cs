using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using TheWall.Models;

namespace TheWall.Controllers;

public class SessionCheckAttribute : ActionFilterAttribute {
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        int? userId = context.HttpContext.Session.GetInt32("UserId");
        if (userId == null) {
            context.Result = new RedirectToActionResult("Auth", "Home", null);
        }
    }
}

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private MyContext _context; 

    public HomeController(ILogger<HomeController> logger, MyContext context)
    {
        _logger = logger;
        _context = context;
    }

    [SessionCheck]
    public IActionResult Index()
    {
        int? userId =  HttpContext.Session.GetInt32("UserId");
        ViewBag.userId = userId;
        User currentUser = _context.Users.FirstOrDefault(e => e.UserId == userId);
        ViewBag.Name = currentUser;

        ViewBag.AllMessages = _context.Messages.Include(e => e.Commenter).Include(e => e.AllComments).ThenInclude(e => e.UserCommenter).OrderByDescending(e => e.CreatedAt).ToList();
        // ViewBag.AllComments = _context.Comments.Include(e => e.UserCommenter).Include(e => e.Messages).ToList();

        return View();
    }

    [HttpGet("Auth")]
    public IActionResult Auth() {
        return View();
    }

    [HttpPost("Register")]
    public IActionResult Register(User userFromForm) {
        if (ModelState.IsValid) {
            PasswordHasher<User> Hasher = new PasswordHasher<User>();
            userFromForm.Password = Hasher.HashPassword(userFromForm, userFromForm.Password);
            _context.Add(userFromForm);
            _context.SaveChanges();

            HttpContext.Session.SetInt32("UserId", userFromForm.UserId);
            return RedirectToAction("Index");
        }
        return View("Auth");
    }

    [HttpPost("Login")]
    public IActionResult Login(LoginUser registeredUser) {
        if (ModelState.IsValid) {
            User userFromDb = _context.Users.FirstOrDefault(e => e.Email == registeredUser.LoginEmail);

            if (userFromDb == null) {
                ModelState.AddModelError("LoginEmail", "Invalid email address.");
                return View("Auth");
            }

            PasswordHasher<LoginUser> Hasher = new PasswordHasher<LoginUser>();
            
            var result = Hasher.VerifyHashedPassword(registeredUser, userFromDb.Password, registeredUser.LoginPassword);

            if (result == 0) {
                ModelState.AddModelError("LoginPassword", "Invalid password.");
                return View("Auth");
            }

            HttpContext.Session.SetInt32("UserId", userFromDb.UserId);
            return RedirectToAction("Index");
        }

        return View("Auth");
    }

    [HttpGet("Logout")]
    public IActionResult Logout() {
        HttpContext.Session.Clear();
        return RedirectToAction("Auth");
    }

    [SessionCheck]
    [HttpPost]
    public IActionResult PostMessage(Message messageFromForm) {
        if (ModelState.IsValid) {
            messageFromForm.UserId = HttpContext.Session.GetInt32("UserId");
            _context.Add(messageFromForm);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        int? userId =  HttpContext.Session.GetInt32("UserId");
        ViewBag.userId = userId;
        User currentUser = _context.Users.FirstOrDefault(e => e.UserId == userId);
        ViewBag.Name = currentUser;

        ViewBag.AllMessages = _context.Messages.Include(e => e.Commenter).Include(e => e.AllComments).ThenInclude(e => e.UserCommenter).ToList();

        return View("Index");
    }

    [SessionCheck]
    [HttpGet]
    public IActionResult DeletePost(int id) {
        Message DeletePost = _context.Messages.Include(e => e.Commenter).Include(e => e.AllComments).ThenInclude(e => e.UserCommenter).FirstOrDefault(e => e.MessageId == id);
        _context.Remove(DeletePost);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    [SessionCheck]
    [HttpPost]
    public IActionResult PostComment(Comment commentFromForm, int id) {
        if (ModelState.IsValid) {
            commentFromForm.UserId = HttpContext.Session.GetInt32("UserId");
            commentFromForm.MessageId = id;
            _context.Add(commentFromForm);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        int? userId =  HttpContext.Session.GetInt32("UserId");
        ViewBag.userId = userId;
        User currentUser = _context.Users.FirstOrDefault(e => e.UserId == userId);
        ViewBag.Name = currentUser;

        ViewBag.AllMessages = _context.Messages.Include(e => e.Commenter).Include(e => e.AllComments).ThenInclude(e => e.UserCommenter).ToList();

        return View("Index");
    }

    [SessionCheck]
    [HttpGet]
    public IActionResult DeleteComment(int id) {
        Comment DeleteComment = _context.Comments.FirstOrDefault(e => e.CommentId == id);
        _context.Remove(DeleteComment);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
