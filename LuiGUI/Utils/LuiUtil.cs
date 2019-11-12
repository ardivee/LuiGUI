#region copyright
// LuiGUI - PSD to Lui made easier
// Copyright (c) 2019 Ardivee
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion
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
