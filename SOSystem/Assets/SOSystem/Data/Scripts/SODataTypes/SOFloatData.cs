using System;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "data_Float", menuName = "ScriptableObjects/Data/Float"), SOData(typeof(float))]
    public class SOFloatData : SOBaseData<float> { }
}
