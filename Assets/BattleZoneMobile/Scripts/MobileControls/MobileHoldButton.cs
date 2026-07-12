using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace BattleZoneMobile
{
    [Serializable]
    public class BoolEvent : UnityEvent<bool>
    {
    }

    public class MobileHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public UnityEvent onPressed = new UnityEvent();
        public UnityEvent onReleased = new UnityEvent();
        public BoolEvent onStateChanged = new BoolEvent();

        public bool IsPressed { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            SetPressed(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            SetPressed(false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (IsPressed)
            {
                SetPressed(false);
            }
        }

        private void OnDisable()
        {
            SetPressed(false);
        }

        private void SetPressed(bool value)
        {
            if (IsPressed == value)
            {
                return;
            }

            IsPressed = value;
            onStateChanged.Invoke(IsPressed);

            if (IsPressed)
            {
                onPressed.Invoke();
            }
            else
            {
                onReleased.Invoke();
            }
        }
    }
}
