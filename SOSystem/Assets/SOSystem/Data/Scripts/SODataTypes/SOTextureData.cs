using System;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "data_Texture", menuName = "ScriptableObjects/Data/Texture"), SOData(typeof(Texture))]
    public class SOTextureData : SOBaseData<Texture> { }
}
