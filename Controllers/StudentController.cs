using ASPNETCore_DB.Interfaces;
using ASPNETCore_DB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ASPNETCore_DB.Controllers
{
    [TypeFilter(typeof(CustomExceptionFilter))]
    public class StudentController : Controller
    {
        private readonly IStudent _studentRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<IdentityUser> _userManager; 

        public StudentController(IStudent studentRepo,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment webHostEnvironment,
            UserManager<IdentityUser> userManager) 
        {
            _studentRepo = studentRepo;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager; 
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index(string sortOrder, string currentFilter,
            string searchString, int? pageNumber)
        {
            pageNumber = pageNumber ?? 1;
            int pageSize = 3;

            ViewData["CurrentSort"] = sortOrder;
            ViewData["StudentNumberSortParm"] = String.IsNullOrEmpty(sortOrder) ? "number_desc" : "";
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
                pageNumber = 1;
            else
                searchString = currentFilter;

            ViewData["CurrentFilter"] = searchString;

            try
            {
                var students = _studentRepo.GetStudents(searchString, sortOrder);
                return View(PaginatedList<Student>.Create(students, pageNumber ?? 1, pageSize));
            }
            catch (Exception ex)
            {
                throw new Exception("No student records detected");
            }
        }

        //  handles null id by finding student via logged-in user's email
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser == null)
                        return RedirectToAction("Index", "Home");

                    var studentByEmail = _studentRepo.ByEmail(currentUser.Email);
                    if (studentByEmail == null)
                    {
                        ViewBag.Message = "You have not enrolled yet. Please enroll first.";
                        return View("NoEnrollment");
                    }
                    return View(studentByEmail);
                }

                return View(_studentRepo.Details(id));
            }
            catch (Exception ex)
            {
                throw new Exception("Student detail not found");
            }
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        public IActionResult Create()
        {
            Student student = new Student();
            student.Photo = "DefaultPic.png";
            return View(student);
        }
        [Authorize(Roles = "User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Student student)
        {
            string webRootPath = _webHostEnvironment.WebRootPath;

            // ✅ Use Path.Combine instead of string concatenation with backslashes
            string upload = Path.Combine(webRootPath, "images");

            // ✅ Make sure the images folder exists
            if (!Directory.Exists(upload))
                Directory.CreateDirectory(upload);

            var files = HttpContext.Request.Form.Files;

            if (files != null && files.Count > 0)
            {
                string fileName = Guid.NewGuid().ToString();
                string extension = Path.GetExtension(files[0].FileName);

                using (var fileStream = new FileStream(
                    Path.Combine(upload, fileName + extension), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }
                student.Photo = fileName + extension;
            }
            else
            {
                student.Photo = "DefaultPic.png";
            }

            try
            {
                if (ModelState.IsValid)
                    _studentRepo.Create(student);
            }
            catch (Exception ex)
            {
                throw new Exception("Student record not saved.");
            }

            return RedirectToAction("Details", new { id = student.StudentNumber });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Edit(string id)
        {
            try
            {
                return View(_studentRepo.Details(id));
            }
            catch (Exception ex)
            {
                throw new Exception("Student detail not found");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string photoName, Student student)
        {
            if (HttpContext.Request.Form.Files.Count > 0)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;
                string upload = webRootPath + WebConstants.ImagePath;
                string fileName = Guid.NewGuid().ToString();
                string extension = Path.GetExtension(files[0].FileName);

                var oldFile = Path.Combine(upload, photoName);
                if (System.IO.File.Exists(oldFile))
                    System.IO.File.Delete(oldFile);

                using (var fileStream = new FileStream(
                    Path.Combine(upload, fileName + extension), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }
                student.Photo = fileName + extension;
            }
            else
            {
                student.Photo = photoName;
            }

            try
            {
                _studentRepo.Edit(student);
            }
            catch (Exception ex)
            {
                throw new Exception("Student detail could not be edited");
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Delete(string id)
        {
            try
            {
                return View(_studentRepo.Details(id));
            }
            catch (Exception ex)
            {
                throw new Exception("Student detail not found");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(
            [Bind("StudentNumber, FirstName, Surname, EnrollmentDate, Photo, Email")]
            Student student)
        {
            try
            {
                _studentRepo.Delete(student);
            }
            catch (Exception ex)
            {
                throw new Exception("Student could not be deleted");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}