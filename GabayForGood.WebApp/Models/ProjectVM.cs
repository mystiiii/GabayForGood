using System.ComponentModel.DataAnnotations;

namespace GabayForGood.WebApp.Models
{
    public class ProjectVM
    {
        public int ProjectId { get; set; }
        public int OrganizationId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title must be less than 100 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description must be less than 1000 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200, ErrorMessage = "Location must be less than 200 characters.")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(50, ErrorMessage = "Category must be less than 50 characters.")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Cause is required.")]
        [StringLength(50, ErrorMessage = "Cause must be less than 50 characters.")]
        public string Cause { get; set; }

        [Required(ErrorMessage = "Goal Amount is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Goal Amount must be greater than 0.")]
        public decimal GoalAmount { get; set; }

        [Required(ErrorMessage = "Start Date is required.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required.")]
        [DataType(DataType.Date)]
        [DateGreaterThan("StartDate", ErrorMessage = "End Date must be after Start Date.")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(50, ErrorMessage = "Status must be less than 50 characters.")]
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _startDatePropertyName;

        public DateGreaterThanAttribute(string startDatePropertyName)
        {
            _startDatePropertyName = startDatePropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var startDate = (DateTime)validationContext.ObjectType
                .GetProperty(_startDatePropertyName)
                .GetValue(validationContext.ObjectInstance);

            var endDate = (DateTime)value;

            if (endDate <= startDate)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}