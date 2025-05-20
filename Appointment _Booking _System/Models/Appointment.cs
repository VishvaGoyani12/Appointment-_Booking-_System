using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment__Booking__System.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        [ForeignKey("PatientId")]
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
        public DateTime AppointmentDate { get; set; }
        [ForeignKey("DoctorId")]
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

    }
}
