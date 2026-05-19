using ASPNETCore_DB.Data;
using ASPNETCore_DB.Interfaces;
using ASPNETCore_DB.Models;

namespace ASPNETCore_DB.Repositories
{
    public class StudentRepo : IStudent
    {
        private readonly SQLiteDBContext _context;

        public StudentRepo(SQLiteDBContext context)
        {
            _context = context;
        }

        public Student Create(Student student)
        {
            _context.Add(student);
            _context.SaveChanges();
            return student;
        }

        public bool Delete(Student student)
        {
            _context.Remove(student);
            _context.SaveChanges();
            return IsExist(student.StudentNumber);
        }

        public Student Details(string id)
        {
            return _context.Students?.FirstOrDefault(x => x.StudentNumber == id);
        }

        public Student ByEmail(string id)
        {
            return _context.Students?.FirstOrDefault(x => x.Email == id);
        }

        public Student Edit(Student student)
        {
            _context.Update(student);
            _context.SaveChanges();
            return student;
        }

        // Fix: return IQueryable directly from EF — no ToList() here
        public IQueryable<Student> GetStudents(string searchString, string sortOrder)
        {
            if (_context.Students == null)
                return Enumerable.Empty<Student>().AsQueryable();

            IQueryable<Student> students = _context.Students;

            if (!String.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.StudentNumber.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "number_desc":
                    students = students.OrderByDescending(s => s.StudentNumber);
                    break;
                case "name_desc":
                    students = students.OrderByDescending(s => s.Surname);
                    break;
                case "Date":
                    students = students.OrderBy(s => s.EnrollmentDate);
                    break;
                case "date_desc":
                    students = students.OrderByDescending(s => s.EnrollmentDate);
                    break;
                default:
                    students = students.OrderBy(s => s.Surname);
                    break;
            }

            return students;
        }

        public bool IsExist(string id)
        {
            return Details(id) == null;
        }
    }
}