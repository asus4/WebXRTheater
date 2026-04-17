using UnityEngine;
using UnityEngine.Assertions;

namespace HailMaryXR
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
            if (cloneOnStart)
            {
                CloneObject();
            }
        }

        public void CloneObject()
        {
            Assert.IsNotNull(prefab, "Prefab is not assigned.");
            Assert.IsNotNull(positions, "Positions array is not assigned.");

            var parent = transform;
            foreach (var position in positions)
            {
                Instantiate(prefab, position.position, position.rotation, parent);
            }
        }
    }
}
