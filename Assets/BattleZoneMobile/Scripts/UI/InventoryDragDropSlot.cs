using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BattleZoneMobile
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class InventoryDragDropSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField] private Text label;
        [SerializeField] private Image background;
        [SerializeField] private int slotIndex;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Vector2 startPosition;
        private Transform startParent;

        public string Label
        {
            get => label != null ? label.text : string.Empty;
            set
            {
                if (label != null)
                {
                    label.text = value;
                }
            }
        }

        public void Configure(int index, Text slotLabel, Image slotBackground)
        {
            slotIndex = index;
            label = slotLabel;
            background = slotBackground;
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (background == null)
            {
                background = GetComponent<Image>();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            startPosition = rectTransform.anchoredPosition;
            startParent = transform.parent;
            canvasGroup.alpha = 0.72f;
            canvasGroup.blocksRaycasts = false;
            transform.SetAsLastSibling();
            Tint(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (rectTransform == null || startParent == null)
            {
                return;
            }

            RectTransform parentRect = startParent as RectTransform;
            if (parentRect != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                rectTransform.anchoredPosition = localPoint;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            rectTransform.anchoredPosition = startPosition;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            Tint(false);
        }

        public void OnDrop(PointerEventData eventData)
        {
            InventoryDragDropSlot other = eventData.pointerDrag != null
                ? eventData.pointerDrag.GetComponent<InventoryDragDropSlot>()
                : null;

            if (other == null || other == this)
            {
                return;
            }

            string currentLabel = Label;
            Label = other.Label;
            other.Label = currentLabel;
        }

        private void Tint(bool dragging)
        {
            if (background != null)
            {
                background.color = dragging
                    ? new Color(0.18f, 0.38f, 0.44f, 0.96f)
                    : new Color(0.07f, 0.11f, 0.13f, 0.9f);
            }
        }
    }
}
