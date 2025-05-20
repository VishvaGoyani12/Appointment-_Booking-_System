using Appointment__Booking__System.Models;
using Microsoft.EntityFrameworkCore;

namespace Appointment__Booking__System.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly DbContextOptions options;

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            this.options = options;
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

    }
}
