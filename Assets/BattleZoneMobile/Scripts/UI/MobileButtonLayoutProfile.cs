using UnityEngine;

namespace BattleZoneMobile
{
    public class MobileButtonLayoutProfile : MonoBehaviour
    {
        [SerializeField] private RectTransform[] adjustableControls;
        [SerializeField] private float minScale = 0.82f;
        [SerializeField] private float maxScale = 1.18f;

        public void Configure(params RectTransform[] controls)
        {
            adjustableControls = controls;
        }

        public void SetButtonScale(float normalizedValue)
        {
            float scale = Mathf.Lerp(minScale, maxScale, Mathf.Clamp01(normalizedValue));
            if (adjustableControls == null)
            {
                return;
            }

            foreach (RectTransform control in adjustableControls)
            {
                if (control != null)
                {
                    control.localScale = Vector3.one * scale;
                }
            }
        }
    }
}
