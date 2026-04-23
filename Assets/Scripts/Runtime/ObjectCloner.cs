using UnityEngine;
using UnityEngine.Assertions;

namespace WebXRTheater
{
    public sealed class ObjectCloner : MonoBehaviour
    {
        [SerializeField]
        GameObject prefab;

        [SerializeField]
        Transform[] positions;

        [SerializeField]
        bool cloneOnStart = true;

        void Start()
        {
            Assert.IsNotNull(prefab, "Prefab is not assigned.");
            Assert.IsNotNull(positions, "Positions array is not assigned.");

            if (cloneOnStart)
            {
                CloneObject();
            }
        }

        public void CloneObject()
        {
            var parent = transform;
            foreach (var position in positions)
            {
                Instantiate(prefab, position.position, position.rotation, parent);
            }
        }
    }
}
