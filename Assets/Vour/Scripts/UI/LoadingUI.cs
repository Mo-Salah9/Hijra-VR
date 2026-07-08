using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace CrizGames.Vour
{
    public class LoadingUI : MonoBehaviour
    {
        public static string LoadingTextOverride;
        
        [SerializeField] private TextMeshProUGUI loadingText;
        
        private Coroutine _currentCoroutine;

        private void OnEnable()
        {
            _currentCoroutine = StartCoroutine(LoadingAnimation());
        }

        private void OnDisable()
        {
            StopCoroutine(_currentCoroutine);
        }

        private IEnumerator LoadingAnimation()
        {
            var baseText = string.IsNullOrWhiteSpace(LoadingTextOverride) ? VourSettings.Instance.defaultLoadingText : LoadingTextOverride;
            while (true)
            {
                for (int i = 0; i <= 3; i++)
                {
                    loadingText.text = baseText + new string('.', i);
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
    }
}
