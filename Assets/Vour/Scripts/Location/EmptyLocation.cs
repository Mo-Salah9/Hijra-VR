using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrizGames.Vour
{
    /// <summary>
    /// EmptyLocation
    /// </summary>
    public class EmptyLocation : LocationView
    {
        public override void UpdateLocation() => IsReady = true;
    }
}