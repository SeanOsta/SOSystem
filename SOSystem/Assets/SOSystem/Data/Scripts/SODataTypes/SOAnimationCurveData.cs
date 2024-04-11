using System;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "data_Curve", menuName = "ScriptableObjects/Data/Curve"), SOData(typeof(AnimationCurve))]
    public class SOAnimationCurveData : SOBaseData<AnimationCurve> { }
}
