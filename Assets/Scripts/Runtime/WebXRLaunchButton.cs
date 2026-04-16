using UnityEngine;
using UnityEngine.UI;
using WebXR;

namespace HailMaryXR
{
    /// <summary>
    /// A button that toggles the WebXR mode (VR or AR) when clicked. 
    /// The button will be interactable only if the target mode is supported.
    /// </summary>
    public sealed class WebXRLaunchButton : MonoBehaviour
    {
        enum InactiveBehavior
        {
            NonInteractable,
            HideButton
        }


        [SerializeField]
        Button target = null;

        [SerializeField]
        WebXRState targetState = WebXRState.VR;

        [SerializeField]
        InactiveBehavior inactiveBehavior = InactiveBehavior.NonInteractable;

        bool IsSupported
        {
            get
            {
                var instance = WebXRManager.Instance;
                if (instance == null)
                {
                    return false;
                }
                return targetState switch
                {
                    WebXRState.VR => instance.isSupportedVR,
                    WebXRState.AR => instance.isSupportedAR,
                    _ => false,
                };
            }
        }

        void OnEnable()
        {
            if (target == null)
            {
                return;
            }

            if (IsSupported)
            {
                target.onClick.AddListener(OnButtonClicked);
            }
            else
            {
                SetInactive();
            }
        }

        void OnDisable()
        {
            if (target == null)
            {
                return;
            }
            target.onClick.RemoveListener(OnButtonClicked);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (target == null && TryGetComponent<Button>(out var button))
            {
                target = button;
            }
        }
#endif // UNITY_EDITOR

        void OnButtonClicked()
        {
            if(!IsSupported)
            {
                return;
            }
            var manager = WebXRManager.Instance;
            switch (targetState)
            {
                case WebXRState.VR:
                    manager.ToggleVR();
                    break;
                case WebXRState.AR:
                    manager.ToggleAR();
                    break;
            }
        }


        void SetInactive()
        {
            switch (inactiveBehavior)
            {
                case InactiveBehavior.HideButton:
                    target.gameObject.SetActive(false);
                    break;
                case InactiveBehavior.NonInteractable:
                    target.interactable = false;
                    break;
            }
        }
    }
}
