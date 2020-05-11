using Cw10.DTOs.Requests;
using Cw10.DTOs.Results;
using Cw10.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw10.Controllers
{
    [Route("api/enrollment")]
    [ApiController]
    public class EnrollmentController : ControllerBase
    {

        private IStudentDbService service;

        public EnrollmentController(IStudentDbService service)
        {
            this.service = service;
        }

        [HttpPost]
        public IActionResult EntrollStudent(EnrollStudentRequest request)
        {
            var result = service.EnrollStudent(request);

            switch (result.ResultCode)
            {
                case ResultCodes.NieWpisanoWszystkichDanychStudenta:
                    return BadRequest("Nie wpisano poprawnie wszystkich danych studenta");

                case ResultCodes.NieIstniejaStudia:
                    return BadRequest("Studia nie istnieją");

                case ResultCodes.StudentJestJuzZapisanyNaSemest:
                    return BadRequest("Student już jest zapisany na semestr 1!");

                case ResultCodes.StudentJuzIstnieje:
                    return BadRequest("Student już istnieje");
            }

            return Created("", result.Response);

        }
    }
}
