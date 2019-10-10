using BulletSharp;
using BulletSharp.Math;
using BulletUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = BulletSharp.Math.Vector3;

public class CharacterMover : MonoBehaviour
{
    #region Attributes
    //Exposed
    [SerializeField] private int maximumIterations = 10;
    [SerializeField] private float smallestFraction = 0.01f;

    Vector3 currentPosition;
    Vector3 targetPosition;

    Vector3 normalizedDirection;

    // Keep track of the contact manifolds
    AlignedManifoldArray manifoldArray = new AlignedManifoldArray();

    ConvexShape collisionShape;
    CollisionObject collisionObject; 
    PairCachingGhostObject pairCacheCollObjCast; //Same thing as above just cast.
    #endregion

    void Awake()
    {
        //Not to be confused with collision object.
        collisionShape = (ConvexShape)GetComponent<BCollisionShape>().GetCollisionShape();
        //We don't need the ghost obj, just the collision obj so let's grab that.
        collisionObject = GetComponent<BPairCachingGhostObject>().GetCollisionObject();
        //Keeps us from doing this cast every two lines.
        pairCacheCollObjCast = (PairCachingGhostObject) collisionObject;
    }

    public void MoveCharacter(CollisionWorld collisionWorld, Vector3 currentPosition, Vector3 requestedTargetPos)
    {
        this.currentPosition = currentPosition;
        this.targetPosition = requestedTargetPos;

        PlayerStep(collisionWorld);
        PenetrationLoop(collisionWorld);

        SetGameObjPosFromCollisionObj();
    }

    void PlayerStep(CollisionWorld collisionWorld)
    {
        Matrix worldTransformSave = collisionObject.WorldTransform;

        StepForwardAndStrafe(collisionWorld);

        //Set only position changes (keep old rot, scale values)
        worldTransformSave.Origin = currentPosition;
        //Update collisionObject.
        collisionObject.WorldTransform = worldTransformSave;
    }

    void StepForwardAndStrafe(CollisionWorld collisionWorld)
    {
        Matrix start = Matrix.Identity, end = Matrix.Identity;

        float fraction = 1.0f;

        int maxIter = maximumIterations;

        while (fraction > smallestFraction && maxIter-- > 0)
        {
            start.Origin = currentPosition;
            end.Origin = targetPosition;

            Vector3 sweepDirNegative = currentPosition - targetPosition;
            KinematicClosestNotMeConvexResultCallback callback = new KinematicClosestNotMeConvexResultCallback(collisionObject, sweepDirNegative, 0f);
            callback.CollisionFilterGroup = pairCacheCollObjCast.BroadphaseHandle.CollisionFilterGroup;
            callback.CollisionFilterMask = pairCacheCollObjCast.BroadphaseHandle.CollisionFilterMask;

            float margin = collisionShape.Margin;

            //Collision Test
            pairCacheCollObjCast.ConvexSweepTestRef(collisionShape, ref start, ref end, callback, collisionWorld.DispatchInfo.AllowedCcdPenetration);
            collisionShape.Margin = margin;

            //The decimal percent of we can move along targetPosition - currentPosition before collision.
            fraction -= callback.ClosestHitFraction;

            if (callback.HasHit)
            {
                //We moved only a fraction of target-current.

                Vector3 hitNormalWorld = callback.HitNormalWorld;
                UpdateTargetPositionBasedOnCollision(ref hitNormalWorld);
                Vector3 currentDir = targetPosition - currentPosition;

                if (Mathf.Approximately(currentDir.LengthSquared, 0)) break;

                //TODO: Break if velocity is against original velocity, stop early to avoid tiny oscilations in sloping corners.
            }
            else
            {
                //We can move the requested to the requested position. Therefore fraction > smallestFraction. 
                currentPosition = targetPosition;
                //But I don't trust that so imma break.
                break;
            }
        }

    }

    void UpdateTargetPositionBasedOnCollision(ref Vector3 hitNormal)
    {
        Vector3 movementDirection = targetPosition - currentPosition;
        float movementLength = movementDirection.Length;
        if (movementLength > MathUtil.SIMD_EPSILON)
        {
            movementDirection.Normalize();

            Vector3 reflectDir = ComputeReflectionDirection(ref movementDirection, ref hitNormal);
            reflectDir.Normalize();

            Vector3 perpindicularDir = PerpindicularComponent(ref reflectDir, ref hitNormal);

            targetPosition = currentPosition;

            Vector3 perpComponent = perpindicularDir * movementLength;
            targetPosition += perpComponent;
        }
    }

    void PenetrationLoop(CollisionWorld collisionWorld)
    {
        int numPenetrationLoops = 0;
        while (RecoverFromPenetration(collisionWorld))
        {
            numPenetrationLoops++;
            if (numPenetrationLoops > 4)
            {
                break;
            }
        }
    }

    bool RecoverFromPenetration(CollisionWorld collisionWorld)
    {
        Vector3 minAabb, maxAabb;
        collisionShape.GetAabb(collisionObject.WorldTransform, out minAabb, out maxAabb);
        collisionWorld.Broadphase.SetAabbRef(collisionObject.BroadphaseHandle,
                     ref minAabb,
                     ref maxAabb,
                     collisionWorld.Dispatcher);

        bool penetration = false;

        collisionWorld.Dispatcher.DispatchAllCollisionPairs(pairCacheCollObjCast.OverlappingPairCache, collisionWorld.DispatchInfo, collisionWorld.Dispatcher);

        currentPosition = collisionObject.WorldTransform.Origin;

        float maxPen = 0f;
        for (int i = 0; i < pairCacheCollObjCast.OverlappingPairCache.NumOverlappingPairs; i++)
        {
            manifoldArray.Clear();

            BroadphasePair collisionPair = pairCacheCollObjCast.OverlappingPairCache.OverlappingPairArray[i];

            CollisionObject obj0 = collisionPair.Proxy0.ClientObject as CollisionObject;
            CollisionObject obj1 = collisionPair.Proxy1.ClientObject as CollisionObject;

            if ((obj0 != null && !obj0.HasContactResponse) || (obj1 != null && !obj1.HasContactResponse))
                continue;

            if (collisionPair.Algorithm != null)
            {
                collisionPair.Algorithm.GetAllContactManifolds(manifoldArray);
            }

            for (int j = 0; j < manifoldArray.Count; j++)
            {
                PersistentManifold manifold = manifoldArray[j];
                float directionSign = manifold.Body0 == collisionObject ? -1f : 1f;
                for (int p = 0; p < manifold.NumContacts; p++)
                {
                    ManifoldPoint pt = manifold.GetContactPoint(p);

                    float dist = pt.Distance;

                    if (dist < 0.0f)
                    {
                        if (dist < maxPen)
                        {
                            maxPen = dist;
                        }
                        currentPosition += pt.NormalWorldOnB * directionSign * dist * 0.2f;
                        penetration = true;
                    }
                }
            }
        }
        Matrix newTrans = collisionObject.WorldTransform;
        newTrans.Origin = currentPosition;
        collisionObject.WorldTransform = newTrans;

        return penetration;
    }

    void SetGameObjPosFromCollisionObj()
    {
        collisionObject.GetWorldTransform(out Matrix trans);

        transform.position = BSExtensionMethods2.ExtractTranslationFromMatrix(ref trans);
    }

    #region Helper
    static Vector3 ComputeReflectionDirection(ref Vector3 direction, ref Vector3 normal)
    {
        float dot;
        Vector3.Dot(ref direction, ref normal, out dot);
        return direction - (2.0f * dot) * normal;
    }

    static Vector3 ParallelComponent(ref Vector3 direction, ref Vector3 normal)
    {
        float magnitude;
        Vector3.Dot(ref direction, ref normal, out magnitude);
        return normal * magnitude;
    }

    static Vector3 PerpindicularComponent(ref Vector3 direction, ref Vector3 normal)
    {
        return direction - ParallelComponent(ref direction, ref normal);
    }
    #endregion
}
