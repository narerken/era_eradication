using UnityEngine;

namespace Emberfall.CameraCtrl
{

    public class CameraFollow2D : MonoBehaviour
    {
        public Transform target;
        public float smoothTime = 0.18f;
        public Vector2 offset = new Vector2(0f, 1.2f);
        public bool useBounds;
        public Vector2 minBounds;
        public Vector2 maxBounds;

        private Vector3 _velocity;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = target.position + (Vector3)offset;
            desired.z = transform.position.z;

            if (useBounds)
            {
                desired.x = Mathf.Clamp(desired.x, minBounds.x, maxBounds.x);
                desired.y = Mathf.Clamp(desired.y, minBounds.y, maxBounds.y);
            }

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
        }
    }
}
