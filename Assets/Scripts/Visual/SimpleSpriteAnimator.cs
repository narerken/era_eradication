using UnityEngine;

namespace Emberfall.Visual
{

    [RequireComponent(typeof(SpriteRenderer))]
    public class SimpleSpriteAnimator : MonoBehaviour
    {
        [System.Serializable]
        public class Clip
        {
            public string name;
            public Sprite[] frames;
            public float frameRate = 12f;
            public bool loop = true;
        }

        [SerializeField] private Clip[] clips;
        [Tooltip("If set, this clip starts playing automatically on Awake (e.g. a pickup's idle spin).")]
        [SerializeField] private string autoPlayClip;

        private SpriteRenderer _sr;
        private Clip _current;
        private int _frameIndex;
        private float _timer;

        private SpriteRenderer Sr => _sr != null ? _sr : (_sr = GetComponent<SpriteRenderer>());

        private void Awake()
        {
            if (!string.IsNullOrEmpty(autoPlayClip)) Play(autoPlayClip);
        }

        public void Configure(Clip[] newClips, string autoPlay = null)
        {
            clips = newClips;
            if (!string.IsNullOrEmpty(autoPlay)) Play(autoPlay);
        }

        public void Play(string clipName)
        {
            if (_current != null && _current.name == clipName) return;
            if (clips == null) return;
            foreach (var c in clips)
            {
                if (c.name != clipName) continue;
                _current = c;
                _frameIndex = 0;
                _timer = 0f;
                if (_current.frames != null && _current.frames.Length > 0) Sr.sprite = _current.frames[0];
                return;
            }
        }

        private void Update()
        {
            if (_current == null || _current.frames == null || _current.frames.Length == 0) return;
            _timer += Time.deltaTime;
            float frameDuration = 1f / Mathf.Max(1f, _current.frameRate);
            while (_timer >= frameDuration)
            {
                _timer -= frameDuration;
                _frameIndex++;
                if (_frameIndex >= _current.frames.Length)
                    _frameIndex = _current.loop ? 0 : _current.frames.Length - 1;
                Sr.sprite = _current.frames[_frameIndex];
            }
        }
    }
}
