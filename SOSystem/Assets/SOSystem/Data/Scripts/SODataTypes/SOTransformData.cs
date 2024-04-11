using System;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "data_Transform", menuName = "ScriptableObjects/Data/Transform"), SOData(typeof(Transform))]
    public class SOTransformData : SOBaseData<Transform> { }
}
