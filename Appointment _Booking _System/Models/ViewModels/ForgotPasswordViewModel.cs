using System.ComponentModel.DataAnnotations;

namespace Appointment__Booking__System.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
