using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrizGames.Vour
{
    public class ImageLocation : MediaLocation
    {
        public override void UpdateLocation()
        {
            SetTexture(texture);

            UpdateSize(texture ? new Vector2(texture.width, texture.height) : Vector2.one * size);
            IsReady = true;
        }
    }
}