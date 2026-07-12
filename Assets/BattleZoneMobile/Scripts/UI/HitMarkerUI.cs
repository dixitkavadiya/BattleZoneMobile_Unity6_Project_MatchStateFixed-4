using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BattleZoneMobile
{
    public class HitMarkerUI : MonoBehaviour
    {
        [SerializeField] private Graphic hitMarkerGraphic;
        [SerializeField] private float visibleTime = 0.12f;
        [SerializeField] private float fadeTime = 0.16f;

        private Coroutine routine;
        private Color normalColor = new Color(1f, 0.86f, 0.25f, 1f);
        private Color headshotColor = new Color(1f, 0.18f, 0.08f, 1f);

        public void Configure(Graphic graphic)
        {
            hitMarkerGraphic = graphic;
            SetAlpha(0f);
        }

        public void ShowHit()
        {
            Show(false);
        }

        public void ShowHeadshot()
        {
            Show(true);
        }

        private void Show(bool headshot)
        {
            if (hitMarkerGraphic == null)
            {
                return;
            }

            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(ShowRoutine(headshot));
        }

        private IEnumerator ShowRoutine(bool headshot)
        {
            Color color = headshot ? headshotColor : normalColor;
            color.a = 1f;
            hitMarkerGraphic.color = color;
            RectTransform rect = hitMarkerGraphic.rectTransform;
            if (rect != null)
            {
                rect.localScale = Vector3.one * (headshot ? 1.45f : 1f);
            }

            SetAlpha(1f);
            yield return new WaitForSeconds(headshot ? visibleTime * 1.45f : visibleTime);

            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                if (rect != null)
                {
                    rect.localScale = Vector3.Lerp(rect.localScale, Vector3.one, elapsed / fadeTime);
                }

                SetAlpha(1f - elapsed / fadeTime);
                yield return null;
            }

            SetAlpha(0f);
            routine = null;
        }

        private void SetAlpha(float alpha)
        {
            if (hitMarkerGraphic == null)
            {
                return;
            }

            Color color = hitMarkerGraphic.color;
            color.a = Mathf.Clamp01(alpha);
            hitMarkerGraphic.color = color;
        }
    }
}
