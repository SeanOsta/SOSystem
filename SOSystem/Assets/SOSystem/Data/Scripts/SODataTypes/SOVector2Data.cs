using System;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "data_Vector2", menuName = "ScriptableObjects/Data/Vector2"), SOData(typeof(Vector2))]
    public class SOVector2Data : SOBaseData<Vector2> { }
}
