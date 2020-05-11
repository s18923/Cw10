using Cw10.DTOs.Requests;
using Cw10.DTOs.Responses;
using Cw10.DTOs.Results;
using Cw10.ModelsFromDb;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Cw10.Services
{
    public class StudentDbService : IStudentDbService
    {
        private readonly s18923Context context;

        public StudentDbService(s18923Context context)
        {
            this.context = context;
        }
        

        public EnrollStudentResponse PromoteStudents(PromoteStudentRequest request)
        {

            var semestr = new SqlParameter("@Semester", SqlDbType.Int);
            semestr.Value = request.Semester;

            var name = new SqlParameter("@Name", SqlDbType.NVarChar);
            name.Value = request.Studies;

            var idEnrollment = new SqlParameter("@IdEnrollment", SqlDbType.Int); 
            idEnrollment.Direction = ParameterDirection.Output;

            var idStudies = new SqlParameter("@IdStudies", SqlDbType.Int);
            idStudies.Direction = ParameterDirection.Output;

            var startDate = new SqlParameter("@StartDate", SqlDbType.DateTime);
            startDate.Direction = ParameterDirection.Output;

            var x = context.Database.ExecuteSqlRaw("PromoteStudents @Name, @Semester, @IdEnrollment OUT, @IdStudies OUT, @StartDate OUT", parameters: new[] { name, semestr, idEnrollment, idStudies, startDate });

                var response = new EnrollStudentResponse
                {
                    IdEnrollment = (int)idEnrollment.Value,
                    IdStudy = (int)idStudies.Value,
                    Semester = request.Semester + 1,
                    StartDate = (DateTime)startDate.Value
                };

                return response;
        }
        
        public EnrollStudentResult EnrollStudent(EnrollStudentRequest request)
        {

            EnrollStudentResult result = new EnrollStudentResult();

            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.IndexNumber) ||
                string.IsNullOrWhiteSpace(request.BirthDate) ||
                string.IsNullOrWhiteSpace(request.Studies) ||
                !DateTime.TryParse(request.BirthDate, out DateTime birthDate))
            {
                result.ResultCode = ResultCodes.NieWpisanoWszystkichDanychStudenta;

                return result;
            }

            var studies = context.Studies.FirstOrDefault(e => e.Name == request.Studies);

            context.SaveChanges();

                if (studies == null)
                {                   
                    result.ResultCode = ResultCodes.NieIstniejaStudia;
                    return result;
                }

            int idStudy = studies.IdStudy;       

            var bigSelect = context.Student.Any(x => x.IndexNumber == "" && x.IdEnrollmentNavigation.Semester == 1);

                if (bigSelect)
                {
                    result.ResultCode = ResultCodes.StudentJestJuzZapisanyNaSemest;
                    return result;
                }

            var maxSelect = context.Enrollment.Max(e => e.IdEnrollment);
            int maxId = maxSelect + 1;     
            DateTime startDate = DateTime.Now;


            context.Enrollment.Add(new Enrollment
            {
                IdEnrollment = maxId,
                Semester = 1,
                IdStudy = idStudy,
                StartDate = startDate,

            });

            var nameSelect = context.Student.Any(e => e.IndexNumber == request.IndexNumber);

                if (nameSelect)
                {
                    result.ResultCode = ResultCodes.StudentJuzIstnieje;
                    return result;
                }

            DateTime date = DateTime.Parse(request.BirthDate);
            context.Student.Add(new Student
            {
                IndexNumber = request.IndexNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = date,
                IdEnrollment = maxId
            });             

            var response = new EnrollStudentResponse
            {
                IdEnrollment = maxId,
                IdStudy = idStudy,
                Semester = 1,
                StartDate = startDate
            };

            result.ResultCode = ResultCodes.StudentDodany;
            result.Response = response;
            return result;
            
        }
    }
}
