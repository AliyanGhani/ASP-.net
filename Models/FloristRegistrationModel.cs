using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Admin.Models
{
    public class FloristRegistrationModel
    {
        // Business Information
        [Required(ErrorMessage = "Business name is required")]
        public string BusinessName { get; set; }

        [Required(ErrorMessage = "Business type is required")]
        public string BusinessType { get; set; }

        [Required(ErrorMessage = "Years in operation is required")]
        public string YearsOperation { get; set; }

        [Required(ErrorMessage = "Business address is required")]
        public string BusinessAddress { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }

        [Required(ErrorMessage = "ZIP code is required")]
        public string ZipCode { get; set; }

        // Personal Details
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        // Documents
        [Required(ErrorMessage = "Business license is required")]
        public IFormFile BusinessLicense { get; set; }

        [Required(ErrorMessage = "Tax ID certificate is required")]
        public IFormFile TaxId { get; set; }

        public IFormFile Portfolio { get; set; }

        [Required(ErrorMessage = "You must agree to the terms")]
        public bool AgreeTerms { get; set; }

        [Required(ErrorMessage = "You must agree to the privacy policy")]
        public bool AgreePrivacy { get; set; }
    }
}