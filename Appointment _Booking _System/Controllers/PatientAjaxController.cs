using Appointment__Booking__System.Data;
using Appointment__Booking__System.Models;
using Appointment__Booking__System.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq.Dynamic.Core;


namespace Appointment__Booking__System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PatientAjaxController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientAjaxController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult GetAll()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            int start = int.Parse(Request.Form["start"]);
            int length = int.Parse(Request.Form["length"]);
            var sortColumnIndex = int.Parse(Request.Form["order[0][column]"]);
            var sortColumn = Request.Form[$"columns[{sortColumnIndex}][data]"];
            var sortDir = Request.Form["order[0][dir]"];
            var searchValue = Request.Form["search[value]"];

            var gender = Request.Form["gender"].ToString();
            var status = Request.Form["status"].ToString();
            var joinDate = Request.Form["joinDate"].ToString();

            var query = _context.Patients.AsQueryable();

            // search 
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(p =>
                    p.Name.Contains(searchValue) ||
                    p.Gender.Contains(searchValue) ||
                    p.JoinDate.ToString().Contains(searchValue) ||
                    (p.Status ? "Active" : "Deactive").Contains(searchValue)
                );
            }

            // filters
            if (!string.IsNullOrEmpty(gender))
                query = query.Where(p => p.Gender == gender);

            if (!string.IsNullOrEmpty(status) && bool.TryParse(status, out var boolStatus))
                query = query.Where(p => p.Status == boolStatus);

            if (!string.IsNullOrEmpty(joinDate) && DateTime.TryParse(joinDate, out var parsedDate))
                query = query.Where(p => p.JoinDate.Date == parsedDate.Date);

            int recordsTotal = query.Count();

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDir))
                query = query.OrderBy($"{sortColumn} {sortDir}");

            // Paging 
            var data = query.Skip(start).Take(length)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Gender,
                    JoinDate = p.JoinDate.ToString("yyyy-MM-dd"),
                    Status = p.Status ? "Active" : "Deactive"
                })
                .ToList();

            return Json(new
            {
                draw,
                recordsFiltered = recordsTotal,
                recordsTotal,
                data
            });
        }




        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new PatientViewModel
            {
                JoinDate = DateTime.Now
            };
            return PartialView("_Create", viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Create(PatientViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool isDuplicate = _context.Patients
                    .Any(p => p.Name == model.Name && p.Gender == model.Gender);

                if (isDuplicate)
                {
                    ModelState.AddModelError("Name", "A patient with the same name and gender already exists.");
                    return PartialView("_Create", model);
                }

                var patient = new Patient
                {
                    Name = model.Name,
                    Gender = model.Gender,
                    JoinDate = model.JoinDate,
                    Status = model.Status
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return PartialView("_Create", model);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            var viewModel = new PatientViewModel
            {
                Id = patient.Id,
                Name = patient.Name,
                Gender = patient.Gender,
                JoinDate = patient.JoinDate,
                Status = patient.Status
            };

            return PartialView("_Create", viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(PatientViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool isDuplicate = _context.Patients.Any(p =>
                    p.Id != model.Id &&
                    p.Name == model.Name &&
                    p.Gender == model.Gender);

                if (isDuplicate)
                {
                    ModelState.AddModelError("Name", "A patient with the same name and gender already exists.");
                }
                else
                {
                    var patient = await _context.Patients.FindAsync(model.Id);
                    if (patient == null) return NotFound();

                    patient.Name = model.Name;
                    patient.Gender = model.Gender;
                    patient.JoinDate = model.JoinDate;
                    patient.Status = model.Status;

                    _context.Patients.Update(patient);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
            }

            return PartialView("_Create", model);
        }



        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return Json(new { success = false });
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }

}
