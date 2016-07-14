using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hsl.Xrm.Sdk
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns a default value if the string is null.
        /// </summary>
        /// <param name="target">The target string.</param>
        /// <param name="defaultValue">The default value or empty if not specified.</param>
        /// <returns></returns>
        public static string DefaultIfNull(this string target, string defaultValue = "")
        {
            return (target != null) ? target : defaultValue;
        }

        /// <summary>
        /// Returns a default value if the string is null or empty.
        /// </summary>
        /// <param name="target">The target string.</param>
        /// <param name="defaultValue">The default value or empty if not specified.</param>
        /// <returns></returns>
        public static string DefaultIfNullOrEmpty(this string target, string defaultValue = "")
        {
            return !string.IsNullOrEmpty(target) ? target : defaultValue;
        }

        /// <summary>
        /// Returns a default value if the string is null, empty or whitespace.
        /// </summary>
        /// <param name="target">The target string.</param>
        /// <param name="defaultValue">The default value or empty if not specified.</param>
        /// <returns></returns>
        public static string DefaultIfNullOrWhiteSpace(this string target, string defaultValue = "")
        {
            return !string.IsNullOrWhiteSpace(target) ? target : defaultValue;
        }

        /// <summary>
        /// Parses a string into an enum value or returns the default value specified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T ParseEnumOrDefault<T>(this string value, T defaultValue) where T : struct
        {
            T parsedValue;
            if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse<T>(value, true, out parsedValue))
            {
                return parsedValue;
            }

            return defaultValue;
        }

        /// <summary>
        /// Uses string.Format to substitute placeholders in the string.
        /// </summary>
        /// <returns></returns>
        public static string Substitute(this string target, params object[] args)
        {
            return (target != null) ? string.Format(target, args) : string.Empty;
        }

        /// <summary>
        /// Trims the string if it is not null.
        /// </summary>
        /// <param name="target">The target string.</param>
        /// <returns></returns>
        public static string TrimIfNotNull(this string target)
        {
            return (target != null) ? target.Trim() : target;
        }

        /// <summary>
        /// Truncates a string to the maximum length specified.
        /// </summary>
        public static string Truncate(this string target, int maxLength)
        {
            return (!string.IsNullOrEmpty(target) && (target.Length > maxLength)) ? target.Substring(0, maxLength) : target;
        }
    }
}
