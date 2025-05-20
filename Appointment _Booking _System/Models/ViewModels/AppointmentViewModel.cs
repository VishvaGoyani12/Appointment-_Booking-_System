using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Appointment__Booking__System.Models.ViewModels
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Status { get; set; }
    }
}
