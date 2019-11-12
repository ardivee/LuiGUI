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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace LuiGUI.Models
{
    public class HudElem
    {
        /// <summary>
        /// The Image we preview from the PSD Layer
        /// </summary>
        [JsonIgnore]
        public Rectangle HudPreviewImage { get; set; }
        /// <summary>
        /// The Image Retrieve from the PSD Layer
        /// </summary>
        [JsonIgnore]
        public System.Drawing.Bitmap Image { get; set; }
        /// <summary>
        /// The name of the Photoshop Layer
        /// </summary>
        public string PSDLayerName { get; set; }
        /// <summary>
        /// The name of the Image Element in Lui
        /// </summary>
        public string ElemName { get; set; }
        /// <summary>
        /// The name of the Image used in the Element
        /// </summary>
        public string ImageName { get; set; }
        /// <summary>
        /// The Left Anchor
        /// </summary>
        public bool LeftAnchor { get; set; }
        /// <summary>
        /// The Right Anchor
        /// </summary>
        public bool RightAnchor { get; set; }
        /// <summary>
        /// The Top Anchor
        /// </summary>
        public bool TopAnchor { get; set; }
        /// <summary>
        /// The Bottom Anchor
        /// </summary>
        public bool BottomAnchor { get; set; }
        /// <summary>
        /// The Opacity of the Image
        /// </summary>
        public double Alpha { get; set; }
        /// <summary>
        /// If it's Text
        /// </summary>
        public bool IsText { get; set; }
        /// <summary>
        /// The Text to display
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// The FontName used for the Text
        /// </summary>
        public string FontName { get; set; }
        /// <summary>
        /// The FontFile used for the Text
        /// </summary>
        public string FontFile { get; set; }

    }
}
