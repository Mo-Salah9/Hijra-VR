using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrizGames.Vour
{
    public class FollowPlayer : MonoBehaviour
    {
        [SerializeField] private float distance = 2;
        [SerializeField] private float yOffset = 0;
        [Space]
        [SerializeField] private float maxRotationDiff = 30f;
        [SerializeField] private float rotationSmoothTime = 0.1f;
        private float currentSurroundRot, targetSurroundRot, rotVel;

        private Transform PlayerT => PlayerBase.Instance != null ? PlayerBase.Instance.cam.transform : null;
        private float lastPlayerYRot = 0f;

        private IEnumerator Start()
        {
            yield return new WaitWhile(() => PlayerT == null);
            ResetRotation();
        }

        private void OnEnable()
        {
            if (PlayerT != null)
                ResetRotation();
        }

        private void LateUpdate()
        {
            Rotate();
        }

        private void Rotate()
        {
            float playerYRot = PlayerT.eulerAngles.y;

            // Adjust currentSurroundRot if the 0 to 360 flip happened
            if (lastPlayerYRot < 90f && playerYRot > 270f)
                currentSurroundRot += 360f;
            else if (lastPlayerYRot > 270f && playerYRot < 90f)
                currentSurroundRot -= 360f;

            // Normalize currentSurroundRot to be within ±180 degrees of playerYRot
            while (currentSurroundRot - playerYRot > 180f)
                currentSurroundRot -= 360f;
            while (currentSurroundRot - playerYRot < -180f)
                currentSurroundRot += 360f;

            // Rotate around player
            //targetSurroundRot = Mathf.Clamp(currentSurroundRot, playerYRot - maxRotationDiff, playerYRot + maxRotationDiff);
            if (currentSurroundRot < playerYRot - maxRotationDiff || currentSurroundRot > playerYRot + maxRotationDiff)
                targetSurroundRot = playerYRot;
            
            currentSurroundRot = Mathf.SmoothDampAngle(currentSurroundRot, targetSurroundRot, ref rotVel, rotationSmoothTime);
    
            var dir = Quaternion.Euler(0, currentSurroundRot, 0) * Vector3.forward;
            transform.position = PlayerT.position + dir * distance + Vector3.up * yOffset;
    
            // Look at player
            var lookDir = transform.position - PlayerT.position;
            transform.rotation = Quaternion.LookRotation(lookDir);

            lastPlayerYRot = playerYRot;
        }

        private void ResetRotation()
        {
            currentSurroundRot = targetSurroundRot = PlayerT.eulerAngles.y;
            rotVel = 0f;
        }
    }
}