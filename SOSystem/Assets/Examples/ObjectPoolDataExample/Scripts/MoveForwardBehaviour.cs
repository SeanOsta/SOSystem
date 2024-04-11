using Data;
using UnityEngine;

namespace Examples
{
    public class MoveForwardBehaviour : MonoBehaviour
    {
        [SerializeField] private FloatRef m_MetersPerSecond = null;

        private void Update()
        {
            if (m_MetersPerSecond == null)
                return;

            Vector3 dirVector = transform.forward;
            dirVector *= (m_MetersPerSecond.data * Time.deltaTime);

            transform.position = transform.position + dirVector;
        }
    }
}