using System;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "data_Byte", menuName = "ScriptableObjects/Data/Byte"), SOData(typeof(byte))]
    public class SOByteData : SOBaseData<byte> { }
}
