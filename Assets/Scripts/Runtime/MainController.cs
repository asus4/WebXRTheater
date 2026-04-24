using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WebXR;

namespace WebXRTheater
{
    /// <summary>
    /// Entrypoint of the project.
    /// </summary>
    public sealed class MainController : MonoBehaviour
    {
        [Header("State Events")]
        public UnityEvent OnEnterXR;
        public UnityEvent OnEnterTheater;

        [Header("Scene references")]
        [SerializeField]
        Button startXrButton;

        void OnEnable()
        {
            WebXRManager.OnXRChange += OnXRChange;
            // Sent initial state event
            var manager = WebXRManager.Instance;
            OnXRChange(manager.XRState, manager.ViewsCount, manager.ViewsLeftRect, manager.ViewsRightRect);

            if (startXrButton != null)
            {
                startXrButton.onClick.AddListener(StartXR);
            }
        }

        void OnDisable()
        {
            WebXRManager.OnXRChange -= OnXRChange;
            if (startXrButton != null)
            {
                startXrButton.onClick.RemoveListener(StartXR);
            }
        }

        void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            switch (state)
            {
                case WebXRState.VR:
                case WebXRState.AR:
                    OnEnterXR.Invoke();
                    break;
                case WebXRState.NORMAL:
                    OnEnterTheater.Invoke();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public void StartXR()
        {
            OnEnterXR.Invoke();
        }

        public void StartTheater()
        {
            OnEnterTheater.Invoke();
        }
    }
}
