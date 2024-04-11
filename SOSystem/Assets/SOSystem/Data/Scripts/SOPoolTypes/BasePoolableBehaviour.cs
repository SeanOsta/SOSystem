using System;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Base behaviour for a poolable Monobehaviour object. Make an abstract base class to help with dependency injection
    /// in editor
    /// </summary>
    public abstract class BasePoolableBehaviour : MonoBehaviour, IPoolable<BasePoolableBehaviour>
    {
        public Action<BasePoolableBehaviour> onReturnToPool { get; set; }

        public abstract void Disable();

        public abstract void Enable();

        public virtual void ReturnToPool()
        {
            if (onReturnToPool == null)
            {
                Debug.LogError("Returned to pool listener is null when returning to pool! Objects should at the very least have one subscriber to properly go back into the object pool");
                return;
            }

            onReturnToPool.Invoke(this);

            //Clear the returned to pool events
            onReturnToPool = null;
        }
    }
}
