using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuiGUI.Utils
{
    public static class PSDUtil
    {
        /// <summary>
        /// Calculates the new Opacity used in UIElement
        /// </summary>
        /// <param name="opacity"></param>
        /// <returns></returns>
        public static double CalculateOpacity( double opacity )
        {
            return Math.Round(opacity / 255, 2);
        }
    }
}
