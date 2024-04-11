using System;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "data_String", menuName = "ScriptableObjects/Data/String"), SOData(typeof(string))]
    public class SOStringData : SOBaseData<string> { }
}
