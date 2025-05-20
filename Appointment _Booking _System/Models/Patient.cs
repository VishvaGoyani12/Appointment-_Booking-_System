namespace Appointment__Booking__System.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime JoinDate { get; set; }
        public bool Status { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
