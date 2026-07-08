using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static CrizGames.Vour.Location;

namespace CrizGames.Vour
{
    public abstract class MediaLocation : LocationView
    {
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int Layout = Shader.PropertyToID("_Layout");
        private static readonly int ImageType = Shader.PropertyToID("_ImageType");
        
        [SerializeField] private protected Material material;

        public int size = 4;

        [Tooltip("Adjust width of this location based on the size of the provided texture.")]
        public bool resizeWidth = false;

        public MeshRenderer rend;
        
        private Camera _cam;

        private bool Is3D => location.displayType.Is3D();
        private bool Is180 => location.displayType.Is180();
        private bool Is360 => location.displayType.Is360();

        public override void Init()
        {
            base.Init();
            
            rend.sharedMaterial = material;

            _cam = PlayerBase.Instance ? PlayerBase.Instance.cam : Camera.main;
        }

        public void SetTexture(Texture texture)
        {
            int layout = 0;
            if (Is3D)
                layout = location.layout3D == Layout3D.SideBySide ? 1 : 2;
            
            material.SetTexture(MainTex, texture);
            material.SetInt(Layout, layout);

            if (Is180 || Is360)
            {
                material.SetInt(ImageType, Is360 ? 0 : 1);
                // Rotate 360 sphere
                rend.transform.eulerAngles = rotOffset;
            }
        }

        protected void UpdateSize(Vector2 sourceSize)
        {
            var scale = Vector3.one;

            // Adjust X scale
            if (resizeWidth)
            {
                scale.x = sourceSize.x / sourceSize.y;

                if (Is3D)
                {
                    switch (location.layout3D)
                    {
                        case Layout3D.OverUnder:
                            scale.y /= 2;
                            break;

                        case Layout3D.SideBySide:
                            scale.x /= 2;
                            break;
                    }
                }
            }

            // Scale to fullscreen
            if (!Is360 && !Is180 && location.scaleToFullscreen)
            {
                // Calculate the height of the viewport in world units
                var distance = Mathf.Abs(rend.transform.position.z);
                var viewportHeight = 2f * Mathf.Tan(_cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * distance;
                scale *= viewportHeight / size;

                // Half the scale in VR, because for some reason it is double the size in VR.
                if (Utils.InVR())
                    scale /= 2f;
                
                // In Over/Under mode Y scale is halved, so we need to double it
                if (Is3D && location.layout3D == Layout3D.OverUnder)
                    scale *= 2;
            }

            // Set scale
            rend.transform.localScale = size * scale;
        }
    }
}