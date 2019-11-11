using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuiGUI.Utils
{
    public static class LuiUtil
    {
        /// <summary>
        /// Calculates the Left Offset
        /// </summary>
        /// <param name="LeftAnchor"></param>
        /// <param name="RightAnchor"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double CalculateLeftOffsetValue(bool LeftAnchor, bool RightAnchor, double x)
        {
            if(!LeftAnchor && RightAnchor)
                return (Constants.ScreenWidth - x) * -1;

            if(!LeftAnchor && !RightAnchor)
                return x - (Constants.ScreenWidth / 2);

            return x;
        }

        /// <summary>
        /// Calculates the Right Offset
        /// </summary>
        /// <param name="LeftAnchor"></param>
        /// <param name="RightAnchor"></param>
        /// <param name="x"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static double CalculateRightOffsetValue(bool LeftAnchor, bool RightAnchor, double x, double width)
        {
            if (LeftAnchor && !RightAnchor)
                return x + width;

            if(!LeftAnchor && !RightAnchor)
                return x - (Constants.ScreenWidth / 2) + width;

            return (Constants.ScreenWidth - x - width) * -1;
        }

        /// <summary>
        /// Calculate the Top Offset
        /// </summary>
        /// <param name="TopAnchor"></param>
        /// <param name="BottomAnchor"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double CalculateTopOffsetValue(bool TopAnchor, bool BottomAnchor, double y)
        {
            if (!TopAnchor && BottomAnchor)
                return (Constants.ScreenHeight - y) * -1;

            if (!TopAnchor && !BottomAnchor)
                return y - (Constants.ScreenHeight / 2);

            return y;
        }

        /// <summary>
        /// Calculate the Bottom Offset
        /// </summary>
        /// <param name="TopAnchor"></param>
        /// <param name="BottomAnchor"></param>
        /// <param name="y"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static double CalculateBottomOffsetValue(bool TopAnchor, bool BottomAnchor, double y, double height)
        {
            if (TopAnchor && !BottomAnchor)
                return y + height;

            if (!TopAnchor && !BottomAnchor)
                return y - (Constants.ScreenHeight / 2) + height;

            return (Constants.ScreenHeight - y - height) * -1;
        }
    }
}
