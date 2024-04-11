using System;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "data_Color", menuName = "ScriptableObjects/Data/Color"), SOData(typeof(Color))]
    public class SOColorData : SOBaseData<Color> { }
}
