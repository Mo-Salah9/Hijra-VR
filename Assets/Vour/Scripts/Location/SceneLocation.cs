using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrizGames.Vour
{
    public class SceneLocation : LocationView
    {
        public override void UpdateLocation()
        {
            if (Application.isPlaying)
                SceneManager.LoadScene(scene.SceneName);
        }
    }
}