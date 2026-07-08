using System.Collections;
using UnityEngine;

public class PopupSequence : MonoBehaviour
{
    [System.Serializable]
    public class PopupItem
    {
        public GameObject targetObject;
        public float delayBeforeShow;
        public float scaleDuration = 0.4f;
        public float popScale = 2f; // how big it gets
    }

    public PopupItem[] items;

    private void OnEnable()
    {
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        foreach (PopupItem item in items)
        {
            item.targetObject.SetActive(false);
        }

        foreach (PopupItem item in items)
        {
            yield return new WaitForSeconds(item.delayBeforeShow);

            item.targetObject.SetActive(true);

            StartCoroutine(
                ScalePopup(
                    item.targetObject.transform,
                    item.scaleDuration,
                    item.popScale
                )
            );
        }
    }

    IEnumerator ScalePopup(Transform target, float duration, float popScale)
    {
        Vector3 originalScale = target.localScale;

        // Start from actual size
        target.localScale = originalScale;

        Vector3 biggerScale = originalScale * popScale;

        float time = 0f;

        // Scale bigger
        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;
            t = Mathf.SmoothStep(0, 1, t);

            target.localScale = Vector3.Lerp(originalScale, biggerScale, t);

            yield return null;
        }

        time = 0f;

        // Scale back to normal
        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;
            t = Mathf.SmoothStep(0, 1, t);

            target.localScale = Vector3.Lerp(biggerScale, originalScale, t);

            yield return null;
        }

        target.localScale = originalScale;
    }
}