using System;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "data_Int", menuName = "ScriptableObjects/Data/Int"), SOData(typeof(int))]
    public class SOIntData : SOBaseData<int> { }
}
