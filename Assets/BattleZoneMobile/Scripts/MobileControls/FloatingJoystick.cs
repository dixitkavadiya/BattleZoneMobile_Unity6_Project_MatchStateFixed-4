using UnityEngine;
using UnityEngine.EventSystems;

namespace BattleZoneMobile
{
    public class FloatingJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float maxRadius = 90f;
        [SerializeField, Range(0f, 0.4f)] private float deadZone = 0.1f;
        [SerializeField, Range(0.6f, 1.8f)] private float outputCurve = 1.12f;
        [SerializeField] private bool floating;

        private Canvas parentCanvas;
        private Vector2 originalBackgroundPosition;

        public Vector2 Value { get; private set; }
        public bool IsPressed { get; private set; }
        public bool HasVisualReferences => background != null && handle != null;

        public void Configure(RectTransform backgroundRect, RectTransform handleRect, float radius, bool useFloatingMode)
        {
            background = backgroundRect;
            handle = handleRect;
            maxRadius = Mathf.Max(1f, radius);
            floating = useFloatingMode;
            parentCanvas = GetComponentInParent<Canvas>();

            if (background != null)
            {
                originalBackgroundPosition = background.anchoredPosition;
            }

            ResetJoystick();
        }

        public void ConfigureAdvanced(float runtimeDeadZone, float runtimeOutputCurve)
        {
            deadZone = Mathf.Clamp(runtimeDeadZone, 0f, 0.4f);
            outputCurve = Mathf.Clamp(runtimeOutputCurve, 0.6f, 1.8f);
        }

        private void Awake()
        {
            parentCanvas = GetComponentInParent<Canvas>();

            if (background == null)
            {
                background = transform as RectTransform;
            }

            if (background != null)
            {
                originalBackgroundPosition = background.anchoredPosition;
            }
        }

        private void OnEnable()
        {
            ResetJoystick();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsPressed = true;

            if (floating && background != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    background.parent as RectTransform,
                    eventData.position,
                    parentCanvas != null ? parentCanvas.worldCamera : null,
                    out Vector2 localPoint);

                background.anchoredPosition = localPoint;
            }

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null || handle == null)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background,
                eventData.position,
                parentCanvas != null ? parentCanvas.worldCamera : null,
                out Vector2 localPoint);

            Vector2 clamped = Vector2.ClampMagnitude(localPoint, maxRadius);
            handle.anchoredPosition = clamped;
            Value = ProcessInput(clamped / maxRadius);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsPressed = false;
            ResetJoystick();
        }

        private void ResetJoystick()
        {
            Value = Vector2.zero;

            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }

            if (floating && background != null)
            {
                background.anchoredPosition = originalBackgroundPosition;
            }
        }

        private Vector2 ProcessInput(Vector2 raw)
        {
            float magnitude = raw.magnitude;
            if (magnitude <= deadZone)
            {
                return Vector2.zero;
            }

            float normalized = Mathf.InverseLerp(deadZone, 1f, Mathf.Clamp01(magnitude));
            float curved = Mathf.Pow(normalized, outputCurve);
            return raw.normalized * curved;
        }

#if UNITY_EDITOR
        public void SetValueForRuntimeTest(Vector2 rawInput)
        {
            Value = ProcessInput(Vector2.ClampMagnitude(rawInput, 1f));
            IsPressed = Value.sqrMagnitude > 0.0001f;
        }
#endif
    }
}
