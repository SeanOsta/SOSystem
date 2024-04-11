using System;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "data_Bool", menuName = "ScriptableObjects/Data/Bool"), SOData(typeof(bool))]
    public class SOBoolData : SOBaseData<bool> { }
}
