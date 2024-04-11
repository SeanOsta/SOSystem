using UnityEngine;

namespace Events
{
    [CreateAssetMenu(fileName = "event_Transform", menuName = "ScriptableObjects/Events/Transform")]
    public class SOTransformEvent : SOEventArg1<Transform> { }
}
