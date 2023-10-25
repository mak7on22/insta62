using Instagram.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Claims;

namespace Instagram.Controllers
{
    public class UserController : Controller
    {
        private readonly InstagramContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _appEnvironment;

        public UserController(InstagramContext context, UserManager<User> userManager, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _appEnvironment = appEnvironment;
        }

        public IActionResult Index()
        {
            List<User> users = _context.Users.ToList();
            return View(users);
        }

        public async Task<IActionResult> Profile(int? id)
        {
	        var currentId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
	        if (id == currentId)
	        {
		        var user = await _userManager.GetUserAsync(User);
		        ViewData["I"] = "DushnilaDima";
		        ViewData["Publications"] = _context.Publications.Where(p => p.UserId == id).ToList();
		        return View(user);
	        }            
	        else
	        {
		        User anotherUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
		        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		        int.TryParse(currentUserId, out int currentUserIdInt);
		        var isAlreadySubscribed = await _context.SubscribesAndSubscriptions
			        .AnyAsync(s => s.SubscriberId == currentUserIdInt && s.AuthorId == anotherUser.Id);
		        ViewData["Desub"] = isAlreadySubscribed.ToString();
		        ViewData["Publications"]= _context.Publications.Where(p=>p.UserId == id).ToList();
		        return View(anotherUser);
	        }
        }

        [HttpPost]
		[ValidateAntiForgeryToken]
        [Route("Subscribe")]
		public async Task<IActionResult> Subscribe(int id)
		{
			var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			int.TryParse(currentUserId, out int currentUserIdInt);
			User currentUser = await _context.Users.FirstOrDefaultAsync(i => i.Id == currentUserIdInt);
			User userToSubscribe = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

			if (currentUser != null && userToSubscribe != null)
			{
				var isAlreadySubscribed = await _context.SubscribesAndSubscriptions
					.AnyAsync(s => s.SubscriberId == currentUserIdInt && s.AuthorId == userToSubscribe.Id);

				if (!isAlreadySubscribed)
				{
					var subscription = new SubscribesAndSubscriptions
					{
						SubscriberId = currentUserIdInt,
						AuthorId = userToSubscribe.Id
					};
					var user = _context.Users.FirstOrDefault(p => p.Id == id);
					if (user != null)
					{
						user.Subscribers++;
						currentUser.Subscribes++;
						_context.Update(user);
						_context.Update(currentUser);
						await _context.SaveChangesAsync();
					}
					_context.SubscribesAndSubscriptions.Add(subscription);
					await _context.SaveChangesAsync();
				}
			}
			return Json(new { success = true, subscribed = true });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("DeSubscribe")]
		public async Task<IActionResult> DeSubscribe(int id)
		{
			var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			int.TryParse(currentUserId, out int currentUserIdInt);
			User currentUser = await _context.Users.FirstOrDefaultAsync(i => i.Id == currentUserIdInt);
			User userToDeSubscribe = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

			if (currentUser != null && userToDeSubscribe != null)
			{
				var isAlreadySubscribed = await _context.SubscribesAndSubscriptions
					.AnyAsync(s => s.SubscriberId == currentUserIdInt && s.AuthorId == userToDeSubscribe.Id);

				if (isAlreadySubscribed == true)
				{
					var subscription = await _context.SubscribesAndSubscriptions
				   .FirstOrDefaultAsync(s => s.SubscriberId == currentUserIdInt && s.AuthorId == userToDeSubscribe.Id);
					_context.SubscribesAndSubscriptions.Remove(subscription);
					var user = _context.Users.FirstOrDefault(p => p.Id == id);
					if (user != null)
					{
						user.Subscribers--;
						currentUser.Subscribes--;
						_context.Update(user);
						_context.Update(currentUser);
						await _context.SaveChangesAsync();
					}
					await _context.SaveChangesAsync();
				}
			}

			return Json(new { success = true, subscribed = true });
		}

		public async Task<ActionResult> Edit(int id)
        {
            User user = _context.Users.Find(id);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(User user, [FromForm(Name = "uploadedFile")] IFormFile uploadedFile)
        {
                
                if (await _context.Users.FindAsync(user.Id) is User found)
                {
                    _context.Entry(found).State = EntityState.Detached;
                    found.Name = user.Name;
                    found.PhoneNumber = user.PhoneNumber;
                    found.UserInfo = user.UserInfo;
                    if (uploadedFile != null)
                    {
                        string path = "/Files/" + uploadedFile.FileName;
                        using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                            await uploadedFile.CopyToAsync(fileStream);
                        user.Avatar = path;
                        _context.Entry(user).State = EntityState.Detached;
                        found.Avatar = user.Avatar;
                    }
                    found.Email = user.Email;
                    _context.Update(found);
                    await _context.SaveChangesAsync();
                }
                //await  _context.SaveChangesAsync();
                
                return RedirectToAction("Profile", "User", new {id = user.Id});
        }

        public IActionResult SearchResult(string searchResult)
        {
            if (string.IsNullOrEmpty(searchResult))
            {
                return View(new List<User>());
            }

            var filteredUsers = _context.Users
                .Where(u => u.Email.Contains(searchResult) ||
                            u.Login.Contains(searchResult) ||
                            u.Name.Contains(searchResult) ||
                            u.UserName.Contains(searchResult))
                .ToList();

            return View(filteredUsers);
        }
        [HttpGet]
        public IActionResult Search(string search)
        {
	        try
	        {
		        var results = _context.Users
			        .Where(p => p.Login.Contains(search) ||
			                    p.Email.Contains(search) ||
			                    p.Name.Contains(search) ||
			                    p.UserInfo.Contains(search))
			        .Select(p => $"{p.Id},{p.Login},{p.Name},{p.Avatar},{p.Email}")
			        .ToList();
		        return Content(string.Join(";", results));
	        }
	        catch (Exception ex)
	        {
		        // Запись ошибки в лог
		        Console.WriteLine(ex.Message);
		        return Content("Error");
	        }
        }

        public IActionResult Subscribe()
        {
            
            return RedirectToAction("Index", "User");
        }
        public IActionResult DeSubscribe()
        {

            return RedirectToAction("Index", "User");
        }
    }
}

