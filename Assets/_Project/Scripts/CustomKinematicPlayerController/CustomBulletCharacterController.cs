using BulletSharp;
using BulletSharp.Math;
using BulletUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomBulletCharacterController : BPairCachingGhostObject, ICharacterController
{
    [SerializeField] private BCollisionShape collisionShape;

    public bool Init { get; private set; } = false;

    /// <summary>
    /// Add Init Pattern since using MonoBehaviour. If unsure if this Character Controller's Start has been
    /// called yet, call it manually at no risk, or check Init.
    /// </summary>
    internal override void Start()
    {
        if (Init) return;

        base.Start();
        _BuildCollisionObject();

        Init = true;
    }

    //Slightly modified BPairCachingGhostObject's _BuildCollisionObject method. 
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

        return true;
    }

    #region ICharacterController Implementation
    public bool CanJump => throw new System.NotImplementedException();

    public bool OnGround => throw new System.NotImplementedException();

    public void Jump()
    {
        throw new System.NotImplementedException();
    }

    public void PlayerStep(CollisionWorld collisionWorld, float dt)
    {
        throw new System.NotImplementedException();
    }

    public void PreStep(CollisionWorld collisionWorld)
    {
        throw new System.NotImplementedException();
    }

    public void Reset(CollisionWorld collisionWorld)
    {
        throw new System.NotImplementedException();
    }

    public void SetUpInterpolate(bool value)
    {
        throw new System.NotImplementedException();
    }

    public void SetVelocityForTimeInterval(ref BulletSharp.Math.Vector3 velocity, float timeInterval)
    {
        throw new System.NotImplementedException();
    }

    public void SetWalkDirection(ref BulletSharp.Math.Vector3 walkDirection)
    {
        throw new System.NotImplementedException();
    }

    public void UpdateAction(CollisionWorld collisionWorld, float deltaTimeStep)
    {
        throw new System.NotImplementedException();
    }

    #region IAction Implementation
    public void Warp(ref BulletSharp.Math.Vector3 origin)
    {
        throw new System.NotImplementedException();
    }

    public void DebugDraw(IDebugDraw debugDrawer)
    {
        throw new System.NotImplementedException();
    }
    #endregion

    #endregion
}
