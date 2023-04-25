using System.ComponentModel.DataAnnotations;

namespace OnlineVotingWebApp.Validations
{
    public class AgeValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            DateTime date;
            if (value is DateTime)
            {
                date = (DateTime)value;
            }
            else if (value is string)
            {
                if (!DateTime.TryParse((string)value, out date))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            // Compute age
            int age = DateTime.Now.Year - date.Year;
            if (DateTime.Now < date.AddYears(age))
            {
                age--;
            }

            // Age must be at least 18
            return age >= 18;
        }
    }
}

