using System.ComponentModel.DataAnnotations;

namespace Appointment__Booking__System.Models.ViewModels
{
    public class DoctorViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public string SpecialistIn { get; set; } = string.Empty;

        [Required]
        public bool Status { get; set; }
    }
}
