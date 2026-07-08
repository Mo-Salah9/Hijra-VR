using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using static CrizGames.Vour.Location;

namespace CrizGames.Vour
{
    /// <summary>
    /// Base for all location views
    /// </summary>
    public abstract class LocationView : MonoBehaviour
    {
        [HideInInspector] public Location location;
        
        public LocationType locationType => location.locationType;

        public Texture2D texture => location.texture;

        public VideoClip video => location.video;

        public SceneReference scene => location.scene;

        public Vector3 rotOffset => location.rotOffset;

        
        public bool IsReady { get; protected set; }

        /// <summary>
        /// Init
        /// </summary>
        public virtual void Init()
        {
            IsReady = false;
        }

        /// <summary>
        /// UpdateLocation
        /// </summary>
        public abstract void UpdateLocation();
        
        /// <summary>
        /// Set location's data
        /// </summary>
        public void SetData(Location l)
        {
            location = l;

            Init();
            UpdateLocation();
        }
    }
}