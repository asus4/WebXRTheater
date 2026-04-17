using System;
using UnityEngine;
using UnityEngine.Events;
using WebXR;

namespace HailMaryXR
{
    public sealed class MainController : MonoBehaviour
    {
        [Serializable]
        public class StateEvent : UnityEvent
        {
        }

        public StateEvent OnEnterXR;
        public StateEvent OnEnterTheater;

        // [SerializeField]
        // GameObject

        public void StartVR()
        {

        }

        public void StartAR()
        {

        }
        
        public void BackToNormal()
        {

        }
    }
}
