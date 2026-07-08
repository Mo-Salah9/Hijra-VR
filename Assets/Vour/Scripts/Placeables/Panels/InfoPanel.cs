using TMPro;
using UnityEngine;

namespace CrizGames.Vour
{
    public enum InfoPanelImageType
    {
        LeftImage,
        RightImage
    }
    
    public class InfoPanel : Panel
    {
        public string title = "Awesome Title";

        public Sprite image;
        public InfoPanelImageType panelType;
        public PanelDisplayType displayType = PanelDisplayType._2D;
        public Layout3D layout3D;

        [TextArea(5, 10)]
        public string text = "Interesting text.";

        public override Transform panelContainer => transform;
        
        /// <summary>
        /// InitPanel
        /// </summary>
        public override void InitPanel()
        {
            name = $"Info Panel ({title})";
            
            var panelVariant = panel.GetComponent<InfoPanelVariant>();
            
            panelVariant.title.text = title;
            panelVariant.text.text = text;
            
            if (panelVariant.image != null)
            {
                panelVariant.image.displayType = displayType;
                panelVariant.image.layout3D = layout3D;
                panelVariant.image.sprite = image;
            }
        }

        /// <summary>
        /// FindPanel
        /// </summary>
        /// <returns>Transform of panel</returns>
        protected override Transform GetPanel()
        {
            if (transform.childCount > 0)
                return transform.GetChild(0);
            return null;
        }
    }
}
