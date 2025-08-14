using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GabayForGood.WebApp.Models
{
    public class ProjectVM : IValidatableObject
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
        [StringLength(150, ErrorMessage = "Location must be less than 150 characters.")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(100, ErrorMessage = "Category must be less than 100 characters.")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Cause is required.")]
        [StringLength(100, ErrorMessage = "Cause must be less than 100 characters.")]
        public string Cause { get; set; }

        [Required(ErrorMessage = "Goal amount is required")]
        [Range(1000, double.MaxValue, ErrorMessage = "Goal amount must be at least ₱1,000")]
        [Display(Name = "Goal Amount")]
        public decimal GoalAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Initial amount cannot be negative")]
        [Display(Name = "Current Amount")]
        public decimal CurrentAmount { get; set; }

        [StringLength(500, ErrorMessage = "Image URL must be less than 500 characters.")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Project Image")]
        public IFormFile? ImageFile { get; set; }

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

        // Add Organization Name property for display
        [Display(Name = "Organization")]
        public string OrganizationName { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        // Computed properties for display - Changed to double for consistency
        [Display(Name = "Progress Percentage")]
        public double ProgressPercentage => GoalAmount > 0 ? (double)(CurrentAmount / GoalAmount) * 100 : 0;

        [Display(Name = "Remaining Amount")]
        public decimal RemainingAmount => GoalAmount - CurrentAmount;

        [Display(Name = "Is Goal Reached")]
        public bool IsGoalReached => CurrentAmount >= GoalAmount;

        // Custom validation method - moved to ProjectVM class and implements IValidatableObject
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validate that current amount doesn't exceed goal amount
            if (CurrentAmount > GoalAmount)
            {
                yield return new ValidationResult(
                    "Initial amount cannot exceed the funding goal",
                    new[] { nameof(CurrentAmount) });
            }

            // Validate that start date is not in the past (optional - you can remove this if not needed)
            if (StartDate < DateTime.Today)
            {
                yield return new ValidationResult(
                    "Start date cannot be in the past",
                    new[] { nameof(StartDate) });
            }

            // Additional validation: End date should be at least one day after start date
            if (EndDate <= StartDate)
            {
                yield return new ValidationResult(
                    "End date must be after start date",
                    new[] { nameof(EndDate) });
            }
        }
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
            var startDateProperty = validationContext.ObjectType.GetProperty(_startDatePropertyName);
            if (startDateProperty == null)
            {
                return new ValidationResult($"Unknown property: {_startDatePropertyName}");
            }

            var startDate = (DateTime)startDateProperty.GetValue(validationContext.ObjectInstance);
            var endDate = (DateTime)value;

            if (endDate <= startDate)
            {
                return new ValidationResult(ErrorMessage ?? "End date must be after start date");
            }

            return ValidationResult.Success;
        }
    }
}