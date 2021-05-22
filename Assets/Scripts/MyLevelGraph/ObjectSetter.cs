using System;
using System.Collections;
using UnityEngine;

namespace DiceyAdventuresAR.MyLevelGraph
{
    public class ObjectSetter : MonoBehaviour
    {
        [NonSerialized] public bool ended = false;

        public IEnumerator Set()
        {
            var rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

            yield return new WaitForSeconds(1.5f);
            Destroy(rigidbody);
            ended = true;

            yield break;
        }
    }
}
