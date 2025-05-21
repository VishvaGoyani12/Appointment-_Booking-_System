using Appointment__Booking__System.Data;
using Appointment__Booking__System.Models;
using Appointment__Booking__System.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace Appointment__Booking__System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DoctorAjaxController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorAjaxController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult GetAll()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumnIndex = Convert.ToInt32(Request.Form["order[0][column]"]);
            var sortColumn = Request.Form[$"columns[{sortColumnIndex}][data]"];
            var sortDir = Request.Form["order[0][dir]"];
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            var gender = Request.Form["gender"].FirstOrDefault();
            var status = Request.Form["status"].FirstOrDefault();
            var specialistIn = Request.Form["specialistIn"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var query = _context.Doctors.AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(d =>
                    d.Name.Contains(searchValue) ||
                    d.Gender.Contains(searchValue) ||
                    d.SpecialistIn.Contains(searchValue) ||
                    (d.status ? "Active" : "Deactive").Contains(searchValue)
                );
            }

            // Filters
            if (!string.IsNullOrEmpty(gender))
                query = query.Where(d => d.Gender == gender);

            if (!string.IsNullOrEmpty(status) && bool.TryParse(status, out var boolStatus))
                query = query.Where(d => d.status == boolStatus);

            if (!string.IsNullOrEmpty(specialistIn)) 
                query = query.Where(d => d.SpecialistIn == specialistIn);

            int recordsTotal = query.Count();

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDir))
                query = query.OrderBy($"{sortColumn} {sortDir}");

            var data = query.Skip(skip).Take(pageSize)
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.Gender,
                    d.SpecialistIn,
                    Status = d.status ? "Active" : "Deactive"
                }).ToList();

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
            var viewModel = new DoctorViewModel();
            return PartialView("_Create", viewModel);
        }


        [HttpGet]
        public IActionResult GetSpecialistList()
        {
            var specialistList = _context.Doctors
                .Where(d => !string.IsNullOrEmpty(d.SpecialistIn))
                .Select(d => d.SpecialistIn)
                .Distinct()
                .ToList();

            return Json(specialistList);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DoctorViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool isDuplicate = _context.Doctors.Any(d =>
                    d.Name == model.Name &&
                    d.Gender == model.Gender &&
                    d.SpecialistIn == model.SpecialistIn);

                if (isDuplicate)
                {
                    ModelState.AddModelError("Name", "A doctor with the same name, gender, and specialty already exists.");
                    return PartialView("_Create", model);
                }

                var doctor = new Doctor
                {
                    Name = model.Name,
                    Gender = model.Gender,
                    SpecialistIn = model.SpecialistIn,
                    status = model.Status
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return PartialView("_Create", model);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            var viewModel = new DoctorViewModel
            {
                Id = doctor.Id,
                Name = doctor.Name,
                Gender = doctor.Gender,
                SpecialistIn = doctor.SpecialistIn,
                Status = doctor.status
            };

            return PartialView("_Create", viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(DoctorViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool isDuplicate = _context.Doctors.Any(d =>
                    d.Id != model.Id &&
                    d.Name == model.Name &&
                    d.Gender == model.Gender &&
                    d.SpecialistIn == model.SpecialistIn);

                if (isDuplicate)
                {
                    ModelState.AddModelError("Name", "A doctor with the same name, gender, and specialty already exists.");
                }
                else
                {
                    var doctor = await _context.Doctors.FindAsync(model.Id);
                    if (doctor == null) return NotFound();

                    doctor.Name = model.Name;
                    doctor.Gender = model.Gender;
                    doctor.SpecialistIn = model.SpecialistIn;
                    doctor.status = model.Status;

                    _context.Doctors.Update(doctor);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
            }

            return PartialView("_Create", model);
        }



        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return Json(new { success = false });
            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
