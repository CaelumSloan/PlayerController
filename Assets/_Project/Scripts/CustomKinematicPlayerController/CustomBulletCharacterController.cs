using BulletSharp;
using BulletSharp.Math;
using BulletUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = BulletSharp.Math.Vector3;

public class CustomBulletCharacterController : BPairCachingGhostObject, ICharacterController
{
    #region Attributes
    //Inspector Exposed
    [Header("Setup Values")]
    [SerializeField] private BCollisionShape collisionShape;
    [SerializeField] private float groundCheckerLength = 0.1f;

    [Header("Movement")]
    [Tooltip("Gravity is set in BPhysicsWorld. This local gravity is added to that.")]
    [SerializeField] private float gravityModifier = 10.19f;
    [Tooltip("The speed at which the character's up axis gains when hitting jump")]
    [SerializeField] private float jumpSpeed = 8.0f;

    //Important Attributes
    protected BulletSharp.Math.Vector3 currentPosition;
    protected BulletSharp.Math.Vector3 targetPosition;

    protected float verticalVelocity = 0;

    protected PairCachingGhostObject ghostObject;
    protected ConvexShape convexShape; //is also in ghostObject, but it needs to be convex, so we store it here to avoid upcast.

    #region Things I want to hold onto for longer than a function. 
    protected bool touchingContact;
    protected BulletSharp.Math.Vector3 m_touchingNormal;

    protected bool wasOnGround = true;

    protected float verticalOffset = 0;

    protected BulletSharp.Math.Vector3 normalizedDirection;
    #endregion

    //Keep track of the contact manifolds.
    protected AlignedManifoldArray localManifoldArray = new AlignedManifoldArray();
    #endregion

    /// <summary>
    /// Slightly modified BPairCachingGhostObject's _BuildCollisionObject method, sets local variables. 
    /// Done on Initialization.
    /// </summary>
    internal override bool _BuildCollisionObject()
    {
        #region Remove old collision object.
        BPhysicsWorld world = BPhysicsWorld.Get();
        if (m_collisionObject != null)
        {
            if (isInWorld && world != null)
            {
                isInWorld = false;
                world.RemoveCollisionObject(this);
            }
        }
        #endregion

        #region Warnings and Errors
        //Friendly note
        if (collisionShape.transform.localScale != UnityEngine.Vector3.one)
        {
            Debug.LogWarning("The local scale on this collision shape is not one. Bullet physics does not support scaling on a rigid body world transform. Instead alter the dimensions of the CollisionShape. " + name, this);
        }

        if (collisionShape == null)
        {
            Debug.LogError("There was no collision shape set. " + name, this);
            return false;
        }
        if (!(collisionShape.GetCollisionShape() is ConvexShape))
        {
            Debug.LogError("The CollisionShape on this character controller is not a convex shape. " + name, this);
            return false;
        }
        #endregion

        #region Init PairCachingGhostObject and ConvexShape
        m_collisionObject = new PairCachingGhostObject();

        m_collisionShape = collisionShape;
        m_collisionObject.CollisionShape = m_collisionShape.GetCollisionShape();
        m_collisionObject.CollisionFlags = m_collisionFlags;

        BulletSharp.Math.Matrix worldTrans;
        BulletSharp.Math.Quaternion q = transform.rotation.ToBullet();
        BulletSharp.Math.Matrix.RotationQuaternion(ref q, out worldTrans);
        worldTrans.Origin = transform.position.ToBullet();
        m_collisionObject.WorldTransform = worldTrans;
        m_collisionObject.UserObject = this;
        #endregion

        ghostObject = (PairCachingGhostObject) m_collisionObject;
        convexShape = (ConvexShape) m_collisionShape.GetCollisionShape();

        world.AddAction(this);

        return true;
    }

    public void FixedUpdate()
    {
        BulletSharp.Math.Matrix trans;
        ghostObject.GetWorldTransform(out trans);
        transform.position = BSExtensionMethods2.ExtractTranslationFromMatrix(ref trans);
        transform.rotation = BSExtensionMethods2.ExtractRotationFromMatrix(ref trans);
        transform.localScale = BSExtensionMethods2.ExtractScaleFromMatrix(ref trans);
    }

    #region Custom Methods
    protected bool RecoverFromPenetration(CollisionWorld collisionWorld)
    {
        Vector3 minAabb, maxAabb;
        convexShape.GetAabb(ghostObject.WorldTransform, out minAabb, out maxAabb);
        collisionWorld.Broadphase.SetAabbRef(ghostObject.BroadphaseHandle,
                     ref minAabb,
                     ref maxAabb,
                     collisionWorld.Dispatcher);

        bool penetration = false;

        collisionWorld.Dispatcher.DispatchAllCollisionPairs(ghostObject.OverlappingPairCache, collisionWorld.DispatchInfo, collisionWorld.Dispatcher);

        currentPosition = ghostObject.WorldTransform.Origin;

        float maxPen = 0f;
        for (int i = 0; i < ghostObject.OverlappingPairCache.NumOverlappingPairs; i++)
        {
            localManifoldArray.Clear();

            BroadphasePair collisionPair = ghostObject.OverlappingPairCache.OverlappingPairArray[i];

            CollisionObject obj0 = collisionPair.Proxy0.ClientObject as CollisionObject;
            CollisionObject obj1 = collisionPair.Proxy1.ClientObject as CollisionObject;

            if ((obj0 != null && !obj0.HasContactResponse) || (obj1 != null && !obj1.HasContactResponse))
                continue;

            if (collisionPair.Algorithm != null)
            {
                collisionPair.Algorithm.GetAllContactManifolds(localManifoldArray);
            }

            for (int j = 0; j < localManifoldArray.Count; j++)
            {
                PersistentManifold manifold = localManifoldArray[j];
                float directionSign = manifold.Body0 == ghostObject ? -1f : 1f;
                for (int p = 0; p < manifold.NumContacts; p++)
                {
                    ManifoldPoint pt = manifold.GetContactPoint(p);

                    float dist = pt.Distance;

                    if (dist < 0.0f)
                    {
                        if (dist < maxPen)
                        {
                            maxPen = dist;
                            m_touchingNormal = pt.NormalWorldOnB * directionSign;//??
                        }
                        currentPosition += pt.NormalWorldOnB * directionSign * dist * 0.2f;
                        penetration = true;
                    }
                }
            }
        }
        Matrix newTrans = ghostObject.WorldTransform;
        newTrans.Origin = currentPosition;
        ghostObject.WorldTransform = newTrans;
        return penetration;
    }

    protected void StepForwardAndStrafe(CollisionWorld collisionWorld, ref Vector3 walkMove)
    {
        Matrix start = Matrix.Identity, end = Matrix.Identity;
        targetPosition = currentPosition + walkMove;

        float fraction = 1.0f;
        float distance2 = (currentPosition - targetPosition).LengthSquared;
        //	printf("distance2=%f\n",distance2);

        if (touchingContact)
        {
            float dot;
            Vector3.Dot(ref normalizedDirection, ref m_touchingNormal, out dot);
            if (dot > 0.0f)
            {
                //interferes with step movement
                //UpdateTargetPositionBasedOnCollision(ref m_touchingNormal, 0.0f, 1.0f);
            }
        }

        int maxIter = 10;

        while (fraction > 0.01f && maxIter-- > 0)
        {
            start.Origin = (currentPosition);
            end.Origin = (targetPosition);

            Vector3 sweepDirNegative = currentPosition - targetPosition;

            KinematicClosestNotMeConvexResultCallback callback = new KinematicClosestNotMeConvexResultCallback(ghostObject, sweepDirNegative, 0f);
            callback.CollisionFilterGroup = ghostObject.BroadphaseHandle.CollisionFilterGroup;
            callback.CollisionFilterMask = ghostObject.BroadphaseHandle.CollisionFilterMask;


            float margin = convexShape.Margin;
            convexShape.Margin = margin + .02f;

            ghostObject.ConvexSweepTestRef(convexShape, ref start, ref end, callback, collisionWorld.DispatchInfo.AllowedCcdPenetration);

            convexShape.Margin = margin;


            fraction -= callback.ClosestHitFraction;

            if (callback.HasHit)
            {
                // We moved only a fraction
                float hitDistance = (callback.HitPointWorld - currentPosition).Length;

                Vector3 hitNormalWorld = callback.HitNormalWorld;
                UpdateTargetPositionBasedOnCollision(ref hitNormalWorld, 0f, 1f);
                Vector3 currentDir = targetPosition - currentPosition;
                distance2 = currentDir.LengthSquared;
                if (distance2 > MathUtil.SIMD_EPSILON)
                {
                    currentDir.Normalize();
                    /* See Quake2: "If velocity is against original velocity, stop ead to avoid tiny oscilations in sloping corners." */
                    float dot;
                    Vector3.Dot(ref currentDir, ref normalizedDirection, out dot);
                    if (dot <= 0.0f)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            else
            {
                // we moved whole way
                currentPosition = targetPosition;
            }
        }
    }

    //Think this is supposed doing the move and slide Riley reffered to.
    protected void UpdateTargetPositionBasedOnCollision(ref Vector3 hitNormal, float tangentMag, float normalMag)
    {
        Vector3 movementDirection = targetPosition - currentPosition;
        float movementLength = movementDirection.Length;
        if (movementLength > MathUtil.SIMD_EPSILON)
        {
            movementDirection.Normalize();

            Vector3 reflectDir = ComputeReflectionDirection(ref movementDirection, ref hitNormal);
            reflectDir.Normalize();

            Vector3 perpindicularDir;

            perpindicularDir = PerpindicularComponent(ref reflectDir, ref hitNormal);

            targetPosition = currentPosition;

            if (normalMag != 0.0f)
            {
                Vector3 perpComponent = perpindicularDir * (normalMag * movementLength);
                targetPosition += perpComponent;
            }
        }
    }

    protected Vector3 PerpindicularComponent(ref Vector3 direction, ref Vector3 normal)
    {
        return direction - ParallelComponent(ref direction, ref normal);
    }

    protected Vector3 ParallelComponent(ref Vector3 direction, ref Vector3 normal)
    {
        float magnitude;
        Vector3.Dot(ref direction, ref normal, out magnitude);
        return normal * magnitude;
    }

    protected Vector3 ComputeReflectionDirection(ref Vector3 direction, ref Vector3 normal)
    {
        float dot;
        Vector3.Dot(ref direction, ref normal, out dot);
        return direction - (2.0f * dot) * normal;
    }
    #endregion

    #region ICharacterController Implementation

    #region IAction Implementation
    //The Bullet equivalent to Unity's Update.
    public void UpdateAction(CollisionWorld collisionWorld, float deltaTimeStep)
    {
        PreStep(collisionWorld);
        PlayerStep(collisionWorld, deltaTimeStep);
    }

    //Draw whatever you want with the debugDrawer. Blank for now.
    public void DebugDraw(IDebugDraw debugDrawer) { }
    #endregion

    //Collision solving.
    public void PreStep(CollisionWorld collisionWorld)
    {
        int numPenetrationLoops = 0;
        touchingContact = false;
        while (RecoverFromPenetration(collisionWorld))
        {
            numPenetrationLoops++;
            touchingContact = true;
            if (numPenetrationLoops > 4)
            {
                break;
            }
        }

        currentPosition = ghostObject.WorldTransform.Origin;
        targetPosition = currentPosition;
    }

    //Where the magic happens
    public void PlayerStep(CollisionWorld collisionWorld, float dt)
    {
        wasOnGround = OnGround;

        //Doesn't support sideways gravity, also flipping gravity upside down might not work great.
        verticalVelocity -= (-BPhysicsWorld.Get().gravity.y + gravityModifier) * dt;

        Matrix xform = ghostObject.WorldTransform;

        var movementDir = BulletSharp.Math.Vector3.Zero;

        //TODO: Implement more movement.
        StepForwardAndStrafe(collisionWorld, ref movementDir);

        xform.Origin = currentPosition;
        ghostObject.WorldTransform = xform;
    }

    //?
    public void SetWalkDirection(ref BulletSharp.Math.Vector3 walkDirection)
    {
        //this.walkDirection = walkDirection;
        //normalizedDirection = walkDirection.normalized();
    }

    public virtual void SetWalkDirection(BulletSharp.Math.Vector3 walkDirection)
    {
        SetWalkDirection(ref walkDirection);
    }

    public void Jump() {}

    public bool CanJump { get { return OnGround; } }

    public bool OnGround
    {
        get
        {
            #region Raycast Down Check
            BulletSharp.Math.Vector3 from = transform.position.ToBullet();
            BulletSharp.Math.Vector3 to = (transform.position + (UnityEngine.Vector3.down * groundCheckerLength)).ToBullet();

            ClosestRayResultCallback callback = new ClosestRayResultCallback(ref from, ref to);
            BPhysicsWorld.Get().world.RayTest(from, to, callback);
            #endregion
            //Additionally one might check if any of the objects we're currently touching is tagged ground.

            return verticalVelocity == 0.0f && verticalOffset == 0.0f && callback.HasHit;
        }
    }

    public void Warp(ref BulletSharp.Math.Vector3 origin)
    {
        ghostObject.WorldTransform = Matrix.Translation(origin);
    }

    public void Reset(CollisionWorld collisionWorld)
    {
        verticalVelocity = 0.0f;
        verticalOffset = 0.0f;
        wasOnGround = false;
        //?
        //walkDirection = Vector3.Zero;

        //clear pair cache
        HashedOverlappingPairCache cache = ghostObject.OverlappingPairCache;
        while (cache.OverlappingPairArray.Count > 0)
        {
            cache.RemoveOverlappingPair(cache.OverlappingPairArray[0].Proxy0, cache.OverlappingPairArray[0].Proxy1, collisionWorld.Dispatcher);
        }
    }

    public void SetUpInterpolate(bool v) { }

    public void SetVelocityForTimeInterval(ref BulletSharp.Math.Vector3 velocity, float timeInterval) { }
    #endregion
}
