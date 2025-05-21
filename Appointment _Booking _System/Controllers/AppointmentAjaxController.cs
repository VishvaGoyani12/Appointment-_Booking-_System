using Appointment__Booking__System.Data;
using Appointment__Booking__System.Models;
using Appointment__Booking__System.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;

namespace Appointment__Booking__System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AppointmentAjaxController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentAjaxController(ApplicationDbContext context)
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
            var status = Request.Form["status"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var dataQuery = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Select(a => new
                {
                    a.Id,
                    PatientName = a.Patient.Name,
                    DoctorName = a.Doctor.Name,
                    a.AppointmentDate,
                    a.Description,
                    a.Status
                });

            if (!string.IsNullOrEmpty(status))
                dataQuery = dataQuery.Where(a => a.Status == status);

            if (!string.IsNullOrEmpty(searchValue))
            {
                dataQuery = dataQuery.Where(a =>
                    a.PatientName.Contains(searchValue) ||
                    a.DoctorName.Contains(searchValue) ||
                    a.Description.Contains(searchValue) ||
                    a.Status.Contains(searchValue));
            }

            int recordsTotal = dataQuery.Count();

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDir))
            {
                try
                {
                    dataQuery = dataQuery.OrderBy($"{sortColumn} {sortDir}");
                }
                catch (ParseException)
                {
                    dataQuery = dataQuery.OrderBy("AppointmentDate desc");
                }
            }

            var data = dataQuery
                .Skip(skip)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id,
                    a.PatientName,
                    a.DoctorName,
                    AppointmentDate = a.AppointmentDate.ToString("yyyy-MM-dd"),
                    a.Description,
                    a.Status
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
            var viewModel = new AppointmentViewModel
            {
                AppointmentDate = DateTime.Now
            };

            PopulateDropdowns();
            return PartialView("_Create", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AppointmentViewModel vm)
        {
            if (ModelState.IsValid)
            {
                bool isDuplicate = _context.Appointments.Any(a =>
                    a.DoctorId == vm.DoctorId &&
                    a.PatientId == vm.PatientId &&
                    a.AppointmentDate.Date == vm.AppointmentDate.Date);

                if (isDuplicate)
                {
                    ModelState.AddModelError("AppointmentDate", "This appointment already exists for the selected doctor and patient on the same date.");
                }
                else
                {
                    var appointment = new Appointment
                    {
                        PatientId = vm.PatientId,
                        DoctorId = vm.DoctorId,
                        AppointmentDate = vm.AppointmentDate,
                        Description = vm.Description,
                        Status = vm.Status
                    };

                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
            }

            PopulateDropdowns();
            return PartialView("_Create", vm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            var vm = new AppointmentViewModel
            {
                Id = appointment.Id,
                PatientId = appointment.PatientId,
                DoctorId = appointment.DoctorId,
                AppointmentDate = appointment.AppointmentDate,
                Description = appointment.Description,
                Status = appointment.Status
            };

            PopulateDropdowns();
            return PartialView("_Create", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AppointmentViewModel vm)
        {
            if (ModelState.IsValid)
            {
                bool isDuplicate = _context.Appointments.Any(a =>
                    a.Id != vm.Id &&
                    a.DoctorId == vm.DoctorId &&
                    a.PatientId == vm.PatientId &&
                    a.AppointmentDate.Date == vm.AppointmentDate.Date);

                if (isDuplicate)
                {
                    ModelState.AddModelError("AppointmentDate", "This appointment already exists for the selected doctor and patient on the same date.");
                }
                else
                {
                    var appointment = await _context.Appointments.FindAsync(vm.Id);
                    if (appointment == null) return NotFound();

                    appointment.PatientId = vm.PatientId;
                    appointment.DoctorId = vm.DoctorId;
                    appointment.AppointmentDate = vm.AppointmentDate;
                    appointment.Description = vm.Description;
                    appointment.Status = vm.Status;

                    _context.Appointments.Update(appointment);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
            }

            PopulateDropdowns();
            return PartialView("_Create", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return Json(new { success = false });

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        private void PopulateDropdowns()
        {
            ViewBag.Patients = _context.Patients
                .Where(p => p.Status == true)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            ViewBag.Doctors = _context.Doctors
                .Where(d => d.status == true)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
                .ToList();
        }
    }
}
