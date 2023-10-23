using Instagram.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Instagram.Controllers
{
	[Authorize]
	public class PublicationController : Controller
	{
		private readonly InstagramContext _context;
		private readonly UserManager<User> _userManager;
		IWebHostEnvironment _appEnvironment;


		public PublicationController(InstagramContext context, IWebHostEnvironment appEnvironment)
		{
			_context = context;
			_appEnvironment = appEnvironment;
		}

		public async Task<IActionResult> Index()
		{
			var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			List<Publication>? posts = new List<Publication>();
			List<Publication> publications = new List<Publication>();
			var sAS = _context.SubscribesAndSubscriptions
				.Where(s => s.SubscriberId == Convert.ToInt32(currentUserId)).ToList();
			foreach (var s in sAS)
			{
				posts = _context.Publications.Include(p => p.User).Where(p => p.UserId == s.AuthorId).ToList();
				publications.AddRange(posts);
			}

			List<Publication>? myPosts = _context.Publications.Include(p => p.User)
				.Where(p => p.UserId == Convert.ToInt32(currentUserId)).ToList();
			publications.AddRange(myPosts);
			return View(publications.OrderByDescending(p => p.DateOfAdd));
		}

		public async Task<IActionResult> Search(string? search)
		{
			var result = _context.Users
				.Where(u => !string.IsNullOrEmpty(search) &&
				            (u.UserName.Contains(search) ||
				             u.Email.Contains(search) ||
				             u.Name.Contains(search) ||
				             u.UserInfo.Contains(search)))
				.Select(u => new
				{
					UserName = u.UserName,
					Email = u.Email,
					Name = u.Name,
					UserInfo = u.UserInfo
				})
				.ToList();

			return Json(result);
		}


		public async Task<IActionResult> Details(int? id)
		{
			if (id == null || _context.Publications == null)
			{
				return NotFound();
			}

			var publication = await _context.Publications
				.Include(p => p.User)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (publication == null)
			{
				return NotFound();
			}

			return View(publication);
		}

		public IActionResult Create()
		{
			ViewData["UserId"] = new SelectList(_context.Users, "Id", "Login");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(
			[Bind("Id,Description,LikeCount,ComentsCount,DateOfAdd,Publication,image")] Publication publication,
			[FromForm(Name = "image")] IFormFile image)
		{
			string path = "/Post/" + image.FileName;
			using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
			{
				await image.CopyToAsync(fileStream);
			}

			if (ModelState.IsValid)
			{
				if (User.Identity.IsAuthenticated)
				{
					var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
					var curentUser = _context.Users.FirstOrDefault(u => u.Id == Convert.ToInt32(currentUserId));
					curentUser.Publications++;
					_context.Users.Update(curentUser);
					publication.UserId = Convert.ToInt32(currentUserId);
					publication.Pictures = path;
					_context.Add(publication);
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
				else
				{
					return RedirectToAction("Login", "Account");
				}
			}

			ViewData["UserId"] = new SelectList(_context.Users, "Id", "Login", publication.UserId);
			return View(publication);
		}

		public async Task<IActionResult> Delete(int? id)
		{
			if (_context.Publications == null)
			{
				return Problem("Entity set 'InstagramContext.Publications'  is null.");
			}

			var publication = await _context.Publications.FindAsync(id);
			if (publication != null)
			{
				var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				var curentUser = _context.Users.FirstOrDefault(u => u.Id == Convert.ToInt32(currentUserId));
				curentUser.Publications--;
				_context.Users.Update(curentUser);
				_context.Publications.Remove(publication);
			}

			await _context.SaveChangesAsync();
			return Json(new { success = true });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit([FromForm] Publication editedPublication, IFormFile uploadedFile)
		{
			if (editedPublication.Id == 0 || !ModelState.IsValid)
			{
				return Json(new { success = false, message = "Недопустимые данные" });
			}

			string path = "/Post/" + uploadedFile.FileName;
			string fullPath = Path.Combine(_appEnvironment.WebRootPath, path);

			if (!Directory.Exists(fullPath))
			{
				Directory.CreateDirectory(fullPath);
			}

			

			var existingPublication = await _context.Publications.FindAsync(editedPublication.Id);


			if (existingPublication != null)
			{
				existingPublication.Pictures = path;
				existingPublication.Description = editedPublication.Description;
				existingPublication.LikeCount = editedPublication.LikeCount;
				existingPublication.ComentsCount = editedPublication.ComentsCount;
				existingPublication.DateOfAdd = DateTime.Now;
				_context.Update(existingPublication);
				await _context.SaveChangesAsync();

				// Вернуться на страницу Profile с указанием ID пользователя
				return RedirectToAction("Profile", "User", new { id = existingPublication.UserId });
			}
			return Json(new { success = false, message = "Недопустимые данные" });
		}
	

	





		private bool PublicationExists(int id)
		{
			return (_context.Publications?.Any(e => e.Id == id)).GetValueOrDefault();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Like(int publicationId)
		{
			if (User.Identity.IsAuthenticated)
			{
				var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				int.TryParse(currentUserId, out int currentUserIdInt);
				var existingLike = _context.Likes.FirstOrDefault(l => l.PublicationId == publicationId && l.UserId == currentUserIdInt);
				if (existingLike == null)
				{
					var like = new Like
					{
						PublicationId = publicationId,
						UserId = currentUserIdInt
					};

					_context.Likes.Add(like);
					await _context.SaveChangesAsync();

					var publication = _context.Publications.FirstOrDefault(p => p.Id == publicationId);
					if (publication != null)
					{
						publication.LikeCount++;
						await _context.SaveChangesAsync();
					}
				}
				else
				{
					_context.Likes.Remove(existingLike);
					await _context.SaveChangesAsync();

					var publication = _context.Publications.FirstOrDefault(p => p.Id == publicationId);
					if (publication != null)
					{
						publication.LikeCount--;
						await _context.SaveChangesAsync();
					}
				}
			}

			return RedirectToAction("Index", "Publication");
		}
		[HttpGet]
		public IActionResult CheckLikeStatus(int publicationId)
		{
			if (User.Identity.IsAuthenticated)
			{
				var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				int.TryParse(currentUserId, out int currentUserIdInt);
				var existingLike = _context.Likes.FirstOrDefault(l => l.PublicationId == publicationId && l.UserId == currentUserIdInt);
				return Json(new { isLiked = existingLike != null });
			}

			return Json(new { isLiked = false });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Comment(int publicationId, string comentText)
		{
			if (comentText != null)
			{
				

				if (User.Identity.IsAuthenticated)
				{
					var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
					int.TryParse(currentUserId, out int currentUserIdInt);

					var coment = new Coment
					{
						PublicationId = publicationId,
						UserId = currentUserIdInt,
						Text = comentText,
						DateOfAdd = DateTime.Now
					};

					_context.Coments.Add(coment);
					await _context.SaveChangesAsync();

					var publication = _context.Publications.FirstOrDefault(p => p.Id == publicationId);
					if (publication != null)
					{
						publication.ComentsCount++;
						await _context.SaveChangesAsync();
					}

				}
			}

			return RedirectToAction("Index");

		}
		public IActionResult GetComments(int publicationId)
		{
			var comments = _context.Coments
				.Where(c => c.PublicationId == publicationId)
				.OrderBy(comments => comments.DateOfAdd)
				.Select(comment => $"{comment.User.Avatar},{comment.User.Login}:{comment.Text},{comment.DateOfAdd.ToString()}")
				.ToList<string>(); 
			return Content(string.Join("\n", comments));
		}
	}
}
