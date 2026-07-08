using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

namespace CrizGames.Vour
{
    /// <summary>
    /// InfoPoint
    /// </summary>
    public class InfoPoint : PopupPoint
    {
        [FormerlySerializedAs("CustomPanel")] 
        public bool useCustomPanel = false;
        [FormerlySerializedAs("CustomPanelObject")] 
        public GameObject customPanelObject;

        [FormerlySerializedAs("Title")] 
        public string title = "Awesome Title";

        [FormerlySerializedAs("Image")] 
        public Sprite image;
        public PanelDisplayType displayType = PanelDisplayType._2D;
        public Layout3D layout3D;
        public InfoPanelImageType panelType;

        [FormerlySerializedAs("Text")] 
        [TextArea(5, 10)]
        public string text = "Interesting text.";

        public override Transform panelContainer => transform.FindChildByTag("PopupPanel");

        protected override void Start()
        {
            if (useCustomPanel)
            {
                if (customPanelObject == null)
                    Debug.LogError($"Info Point \"{gameObject.name}\" has no Custom Panel Object set!", this);
                
                for (int i = 0; i < panelContainer.childCount; i++)
                    panelContainer.GetChild(i).gameObject.SetActive(false);
            }

            panel.localScale = new Vector3(0, 0, 1);
            panel.gameObject.SetActive(false);

            base.Start();

            RotateTowardsPlayer();
        }

        public override void InitPanel()
        {
            if (useCustomPanel)
                return;

            name = $"Info Point ({title})";
            
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

        protected override Transform GetPanel()
        {
            if (useCustomPanel)
                return customPanelObject.transform;
            
            return panelContainer.GetChild(0);
        }
        
        public override void RotateTowardsPlayer()
        {
            if (!rotateTowardsPlayer)
                return;

            var playerPos = Vector3.zero;

            if (Application.isPlaying && PlayerBase.Instance != null)
                playerPos = PlayerBase.Instance.cam.transform.position;
            
            transform.LookAt(playerPos);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + 180, 0);

            if (!useCustomPanel)
            {
                panelContainer.LookAt(playerPos);
                panelContainer.eulerAngles = new Vector3(0, panelContainer.eulerAngles.y + 180, 0);
            }
            else if (customPanelObject != null)
            {
                customPanelObject.transform.LookAt(playerPos);
                customPanelObject.transform.eulerAngles = new Vector3(0, customPanelObject.transform.eulerAngles.y + 180, 0);
            }
        }
    }
}