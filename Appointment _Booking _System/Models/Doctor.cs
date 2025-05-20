namespace Appointment__Booking__System.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string SpecialistIn { get; set; }
        public bool status { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }

    }
}
