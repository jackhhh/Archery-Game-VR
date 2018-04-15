using UnityEngine;
using System.Collections;

namespace UltraReal.Utilities
{
    public abstract class UltraRealMonobehaviorBase : MonoBehaviour
    {

        [SerializeField]
        private bool _showDebug = true;

        private Transform _cachedTransform = null;
        private Renderer _cachedRenderer = null;
        private Rigidbody _cachedRigidbody = null;
        private Animator _cachedAnimator = null;
        private AudioSource _cachedAudioSource = null;

        public virtual Transform CachedTransform
        {
            get
            {
                if (_cachedTransform == null)
                    _cachedTransform = GetComponent<Transform>();
                return _cachedTransform;
            }
            set { _cachedTransform = value; }
        }

        public virtual Renderer CachedRenderer
        {
            get
            {
                if (_cachedRenderer == null)
                    _cachedRenderer = GetComponent<Renderer>();
                return _cachedRenderer;
            }
            set { _cachedRenderer = value; }
        }

        public virtual Rigidbody CachedRigidbody
        {
            get
            {
                if (_cachedRigidbody == null)
                    _cachedRigidbody = GetComponent<Rigidbody>();
                return _cachedRigidbody;
            }
            set { _cachedRigidbody = value; }
        }

        public virtual Animator CachedAnimator
        {
            get
            {
                if (_cachedAnimator == null)
                    _cachedAnimator = GetComponent<Animator>();
                return _cachedAnimator;
            }
            set { _cachedAnimator = value; }
        }

        public virtual AudioSource CachedAudioSource
        {
            get
            {
                if (_cachedAudioSource == null)
                    _cachedAudioSource = GetComponent<AudioSource>();
                return _cachedAudioSource;
            }
            set { _cachedAudioSource = value; }
        }

        public virtual void DebugLog(string message)
        {
            if (_showDebug)
                Debug.Log(message);
        }

        public virtual void DebugLog(Vector2 message)
        {
            if (_showDebug)
                Debug.Log(message.ToString());
        }

        public virtual void DebugLog(float message)
        {
            if (_showDebug)
                Debug.Log(message.ToString());
        }

        public virtual void DebugLog(int message)
        {
            if (_showDebug)
                Debug.Log(message.ToString());
        }

        void Start()
        {
            OnStart();
        }

        void Awake()
        {
            OnAwake();
        }

        void Update()
        {
            OnUpdate();
        }

        void FixedUpdate()
        {
            OnFixedUpdate();
        }

        protected virtual void OnStart() { }

        protected virtual void OnAwake() { }

        protected virtual void OnUpdate() { }

        protected virtual void OnFixedUpdate() { }

        protected virtual void OnLateUpdate() { }

        protected virtual void OnEnable() { }

        protected virtual void OnDisable() { }

        protected virtual void OnCollisionEnter(Collision col) { }
    }
}
