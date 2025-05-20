using System.ComponentModel.DataAnnotations;

namespace Appointment__Booking__System.Models.ViewModels
{
    public class PatientViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime JoinDate { get; set; } = DateTime.Now;

        [Required]
        public bool Status { get; set; }
    }
}
