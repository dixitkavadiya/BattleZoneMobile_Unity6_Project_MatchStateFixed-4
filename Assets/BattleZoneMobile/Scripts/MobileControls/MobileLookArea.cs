using UnityEngine;
using UnityEngine.EventSystems;

namespace BattleZoneMobile
{
    public class MobileLookArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, ICanvasRaycastFilter
    {
        [SerializeField] private bool rightSideOnly = true;
        [SerializeField, Range(0.1f, 2f)] private float sensitivity = 0.35f;

        private int activePointerId = int.MinValue;
        private Vector2 accumulatedDelta;

        public bool IsDragging { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (rightSideOnly && eventData.position.x < Screen.width * 0.42f)
            {
                return;
            }

            activePointerId = eventData.pointerId;
            IsDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsDragging || eventData.pointerId != activePointerId)
            {
                return;
            }

            accumulatedDelta += eventData.delta * sensitivity;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
            {
                return;
            }

            activePointerId = int.MinValue;
            IsDragging = false;
            accumulatedDelta = Vector2.zero;
        }

        public Vector2 ConsumeLookDelta()
        {
            Vector2 value = accumulatedDelta;
            accumulatedDelta = Vector2.zero;
            return value;
        }

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            return !rightSideOnly || screenPoint.x >= Screen.width * 0.42f;
        }
    }
}
