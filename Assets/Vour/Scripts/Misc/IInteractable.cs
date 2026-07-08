using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrizGames.Vour
{
    public interface IInteractable
    {
        void Interact();

        void OnPointerHoverEnter();

        void OnPointerHoverExit();
    }
}