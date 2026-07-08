using UnityEngine;
using UnityEngine.UI;

namespace CrizGames.Vour
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class Image3D : Image
    {
        private static readonly int LayoutProperty = Shader.PropertyToID("_Layout");
        
        [SerializeField] 
        private PanelDisplayType _displayType = PanelDisplayType._2D;
        public PanelDisplayType displayType
        {
            get => _displayType;
            set
            {
                if (_displayType == value)
                    return;

                _displayType = value;
                UpdateSprite();
            }
        }

        [SerializeField] 
        private Layout3D _layout3D;
        public Layout3D layout3D
        {
            get => _layout3D;
            set
            {
                if (_layout3D == value)
                    return;
                
                _layout3D = value;
                UpdateSprite();
            }
        }

        [SerializeField] 
        private Sprite originalSprite;
        public new Sprite sprite
        {
            get => originalSprite;
            set
            {
                if (originalSprite == value)
                    return;
                
                originalSprite = value;
                UpdateSprite();
            }
        }

        [SerializeField] 
        private Material mat3D;
        public override Material material
        {
            get
            {
                if (mat3D == null)
                    mat3D = new Material(Shader.Find("Vour/UI 3D"));

                return mat3D;
            }
        }

        private void UpdateSprite()
        {
            if (originalSprite == null)
                return;
            
            if (displayType == PanelDisplayType._3D)
                base.sprite = CreateSpriteFor3D(originalSprite, layout3D);
            else
                base.sprite = originalSprite;
            
            UpdateMaterialLayout();
        }
        
        private void UpdateMaterialLayout()
        {
            int layout = 0;
            if (displayType == PanelDisplayType._3D)
                layout = layout3D == Layout3D.SideBySide ? 1 : 2;

            material.SetInt(LayoutProperty, layout);
            base.material = material;
        }

        /// <summary>
        /// Modify rect and pivot in a copy of the sprite for use in the 3D UI shader.
        /// </summary>
        private static Sprite CreateSpriteFor3D(Sprite sprite, Layout3D layout3D)
        {
            Rect imgRect;
            Vector2 imgPivot;
            switch (layout3D)
            {
                case Layout3D.SideBySide:
                    imgRect = new Rect(sprite.rect.x / 2f, sprite.rect.y, sprite.rect.width / 2f, sprite.rect.height);
                    imgPivot = new Vector2(sprite.pivot.x / 2f, sprite.pivot.y);
                    break;
                case Layout3D.OverUnder:
                default:
                    imgRect = new Rect(sprite.rect.x, sprite.rect.y / 2f, sprite.rect.width, sprite.rect.height / 2f);
                    imgPivot = new Vector2(sprite.pivot.x, sprite.pivot.y / 2f);
                    break;
            }
            return Sprite.Create(sprite.texture, imgRect, imgPivot);
        }
    }
}