
using System.Linq;
using UnityEngine.Rendering;

namespace MAOTimelineExtension.Scripts
{
    public static class YMExtensions
    {
        /// Return a titlecased version of the string
        /// Examples: "elysia".Title() => "Elysia"
        public static string Title(this string str)
        {
            return str.First().ToString().ToUpper() + str.Substring(1);
        }


        /// <summary>
        /// Repeats the given string a specified number of times, with the option to insert a separator between repetitions.
        /// </summary>
        /// <param name="str">The string to be repeated.</param>
        /// <param name="times">The number of times to repeat. If less than or equal to 0, an empty string is returned.</param>
        /// <param name="separator">The separator to insert between repetitions of the string. Defaults to an empty string, meaning no separator is added.</param>
        /// <returns>The concatenated string resulting from repeating the input string the specified number of times. Returns an empty string if the input string is null or empty, or if the repeat count is less than or equal to 0.</returns>
        public static string Repeat(this string str, int times, string separator = "")
        {
            if (string.IsNullOrEmpty(str) || times <= 0)
            {
                return string.Empty;
            }

            return string.Join(separator , Enumerable.Repeat(str, times));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vp"></param>
        public static T GetValue<T>(this VolumeParameter vp)
        {
            return vp.GetValue<T>();
        }

    }
}