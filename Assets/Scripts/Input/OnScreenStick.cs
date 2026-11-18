using UnityEngine;
using UnityEngine.EventSystems;

namespace Emberfall.Input
{

    [RequireComponent(typeof(RectTransform))]
    public class OnScreenStick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform knob;
        [SerializeField] private float radius = 70f;

        private Vector2 _origin;

        private void Awake()
        {
            if (knob == null) knob = GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData e) => OnDrag(e);

        public void OnDrag(PointerEventData e)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(background, e.position, e.pressEventCamera, out var local);
            Vector2 clamped = Vector2.ClampMagnitude(local, radius);
            knob.anchoredPosition = clamped;
            MobileInput.SetMove(clamped.x / radius);
        }

        public void OnPointerUp(PointerEventData e)
        {
            knob.anchoredPosition = Vector2.zero;
            MobileInput.SetMove(0f);
        }
    }
}
