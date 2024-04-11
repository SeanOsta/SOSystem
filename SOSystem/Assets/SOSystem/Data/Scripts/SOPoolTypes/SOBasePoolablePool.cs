using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Object pool for any base poolable behaviour
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [CreateAssetMenu(fileName = "pool_BasePoolablePool", menuName = "ScriptableObjects/Data/Pools/BasePoolable")]
    public class SOBasePoolablePool : SOObjectPool<BasePoolableBehaviour> { }
}
