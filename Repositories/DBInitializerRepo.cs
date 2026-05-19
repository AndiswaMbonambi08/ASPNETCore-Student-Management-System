using ASPNETCore_DB.Data;
using ASPNETCore_DB.Interfaces;
using ASPNETCore_DB.Models;

namespace ASPNETCore_DB.Repositories
{
    public class DBInitializerRepo : IDBInitializer
    {
        public async Task Initialize(SQLiteDBContext context)
        {
            if (context.Students.Any())
                return;

            var students = new Student[]
            {
                new Student {
                    StudentNumber  = "2021000001",
                    FirstName      = "Alexander",
                    Surname        = "May",
                    EnrollmentDate = DateTime.Parse("2021-02-03"),
                    Photo          = "DefaultPic.png",
                    Email          = "DefaultEmail@gmail.com"
                },
                new Student {
                    StudentNumber  = "2012000002",
                    FirstName      = "Meredith",
                    Surname        = "Alonso",
                    EnrollmentDate = DateTime.Parse("2021-02-01"),
                    Photo          = "DefaultPic.png",
                    Email          = "DefaultEmail@gmail.com"
                },
                new Student {
                    StudentNumber  = "2021000003",
                    FirstName      = "Arturo",
                    Surname        = "Anand",
                    EnrollmentDate = DateTime.Parse("2021-02-04"),
                    Photo          = "DefaultPic.png",
                    Email          = "DefaultEmail@gmail.com"
                }
            };

            foreach (Student s in students)
                context.Students.Add(s);

            context.SaveChanges();
        }
    }
}