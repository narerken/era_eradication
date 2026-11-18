using UnityEngine;
using UnityEngine.EventSystems;

namespace Emberfall.Input
{

    public class OnScreenButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public void OnPointerDown(PointerEventData e) => MobileInput.SetJumpHeld(true);
        public void OnPointerUp(PointerEventData e) => MobileInput.SetJumpHeld(false);
        private void OnDisable() => MobileInput.SetJumpHeld(false);

        public void OnClickPulse() => MobileInput.PulseJump();
    }
}
