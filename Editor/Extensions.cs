
using System.Linq;
using UnityEngine.Rendering;

namespace YMToonURP.Scripts
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
        /// 
        /// </summary>
        /// <param name="vp"></param>
        public static T GetValue<T>(this VolumeParameter vp)
        {
            return vp.GetValue<T>();
        }

    }
}