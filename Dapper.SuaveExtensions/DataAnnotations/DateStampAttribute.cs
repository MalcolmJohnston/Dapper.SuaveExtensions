using System;
using System.ComponentModel.DataAnnotations;

namespace Dapper.SuaveExtensions.DataAnnotations
{
    /// <summary>
    /// The Date Stamp attribute is intended to be used to ensure that Properties that are Date Stamps
    /// are automatically set to DateTime.Now on Insert, Update or Soft Delete.
    /// </summary>
    /// <seealso cref="ValidationAttribute" />
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DateStampAttribute : ValidationAttribute
    {
        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <param name="value">The value of the object to validate.</param>
        /// <returns>
        /// true if the specified value is valid; otherwise, false.
        /// </returns>
        public override bool IsValid(object value)
        {
            return value is DateTime;
        }
    }
}
