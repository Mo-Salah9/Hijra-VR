using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace CrizGames.Vour
{
    public abstract class PopupPoint : Panel, IInteractable
    {
        protected bool open = false;

        [Tooltip("Should the point and its panel be rotated towards the player automatically?")]
        public bool rotateTowardsPlayer = true;

        protected SpriteRenderer icon;

        protected override void Start()
        {
            icon = GetComponentInChildren<SpriteRenderer>();

            base.Start();
        }

        protected virtual void OnEnable()
        {
            RotateTowardsPlayer();
        }

        public virtual void Interact()
        {
            open = !open;
            StartCoroutine(OpenCloseAnim(open));
        }

        public virtual IEnumerator OpenCloseAnim(bool open)
        {
            if (open)
                panel.gameObject.SetActive(true);

            float currentScale = panel.localScale.x;
            float targetSize = open ? 1f : 0f;
            float scaleVel = 0;
            float animTime = 0.15f;

            // Scale panel faster when closing
            if (!open)
                animTime /= 1.5f;

            // Scale panel while target scale is not reached
            while (System.Math.Round(currentScale, 2) != targetSize)
            {
                currentScale = Mathf.SmoothDamp(currentScale, targetSize, ref scaleVel, animTime);
                panel.localScale = new Vector3(currentScale, currentScale, 1f);

                // Stop coroutine when another has started
                if (open != this.open)
                    yield break;

                yield return null;
            }

            if (!open)
                panel.gameObject.SetActive(false);
        }

        public virtual void RotateTowardsPlayer()
        {
            if (!rotateTowardsPlayer)
                return;

            var playerPos = Vector3.zero;

            if (Application.isPlaying && PlayerBase.Instance != null)
                playerPos = PlayerBase.Instance.cam.transform.position;
            
            transform.LookAt(playerPos);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + 180, 0);

            panel.LookAt(playerPos);
            panel.eulerAngles = new Vector3(0, panel.eulerAngles.y + 180, 0);
        }

        public virtual void OnPointerHoverEnter()
        {
            icon.color = new Color(0.6f, 0.6f, 0.6f);
        }

        public virtual void OnPointerHoverExit()
        {
            icon.color = Color.white;
        }
    }
}