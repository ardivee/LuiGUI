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
