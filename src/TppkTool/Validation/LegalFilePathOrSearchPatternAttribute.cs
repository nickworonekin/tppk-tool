using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using TppkTool.Resources;

namespace TppkTool.Validation
{
    /// <summary>
    /// Specifies that a value must be a legal file path or search pattern.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class LegalFilePathOrSearchPatternAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes an instance of <see cref="LegalFilePathOrSearchPatternAttribute"/>.
        /// </summary>
        public LegalFilePathOrSearchPatternAttribute()
            : base(ErrorMessages.InvalidFilePathOrSearchPattern)
        {
        }

        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string path)
            {
                // Check if path is a legal file path.
                try
                {
                    new FileInfo(path);
                    return ValidationResult.Success;
                }
                catch
                {
                }

                // Check if path is a search pattern.
                try
                {
                    var directory = Path.GetDirectoryName(path);
                    if (directory == string.Empty)
                    {
                        directory = Environment.CurrentDirectory;
                    }
                    Directory.EnumerateFileSystemEntries(directory, Path.GetFileName(path));
                    return ValidationResult.Success;
                }
                catch
                {
                }
            }

            return new ValidationResult(FormatErrorMessage(value as string));
        }
    }
}
