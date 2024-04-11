using Data;
using UnityEngine;

public class SetTransformDataOnAwake : MonoBehaviour
{
    [SerializeField] private SOTransformData m_TransformDataToSet = null;

    private void Awake()
    {
        m_TransformDataToSet.data = transform;
    }
}
