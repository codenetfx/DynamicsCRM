using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fdr.Crm.Plugin.MCSS
{
    public static class StringExtensions
    {
        /// <summary>
        /// Joins the strings using the specified separator and settings when they are not null.
        /// </summary>
        /// <param name="separator">The separator to use when joining the strings.</param>
        /// <param name="trimBeforeJoin">An indication of whether or not to trim the string before joining.</param>
        /// <param name="valueWhenNoArgs">The value to use when the arguments are null or empty.</param>
        /// <param name="args">The string to join.</param>
        /// <returns></returns>
        public static string JoinIfNotNull(string separator, bool trimBeforeJoin, string valueWhenNoArgs, params string[] args)
        {
            var result = valueWhenNoArgs;

            var itemsToJoin = args.Where(s => s != null).Select(s => trimBeforeJoin ? s.Trim() : s);
            if (itemsToJoin.Count() > 0)
            {
                result = string.Join(separator, itemsToJoin);
            }

            return result;
        }

        /// <summary>
        /// Joins the strings using the specified separator and settings when they are not empty or null.
        /// </summary>
        /// <param name="separator">The separator to use when joining the strings.</param>
        /// <param name="trimBeforeJoin">An indication of whether or not to trim the string before joining.</param>
        /// <param name="valueWhenNoArgs">The value to use when the arguments are null or empty.</param>
        /// <param name="args">The string to join.</param>
        /// <returns></returns>
        public static string JoinIfNotNullOrEmpty(string separator, bool trimBeforeJoin, string valueWhenNoArgs, params string[] args)
        {
            var result = valueWhenNoArgs;

            var itemsToJoin = args.Where(s => !string.IsNullOrEmpty(s)).Select(s => trimBeforeJoin ? s.Trim() : s);
            if (itemsToJoin.Count() > 0)
            {
                result = string.Join(separator, itemsToJoin);
            }

            return result;
        }

        /// <summary>
        /// Joins the strings using the specified separator and settings when they are not whitespace, null, or empty.
        /// </summary>
        /// <param name="separator">The separator to use when joining the strings.</param>
        /// <param name="trimBeforeJoin">An indication of whether or not to trim the string before joining.</param>
        /// <param name="valueWhenNoArgs">The value to use when the arguments are null or empty.</param>
        /// <param name="args">The string to join.</param>
        /// <returns></returns>
        public static string JoinIfNotNullOrWhiteSpace(string separator, bool trimBeforeJoin, string valueWhenNoArgs, params string[] args)
        {
            var result = valueWhenNoArgs;

            var itemsToJoin = args.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => trimBeforeJoin ? s.Trim() : s);
            if (itemsToJoin.Count() > 0)
            {
                result = string.Join(separator, itemsToJoin);
            }

            return result;
        }

    }
}
