using BulletSharp;
using BulletUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextPositionFinder : MonoBehaviour, IAction
{
    //Refs
    private CharacterMover characterMover;
    private InputHandler inputHandler;

    //State
    private Vector3 playerVelocity = Vector3.zero;
    
    //Exposed
    [Tooltip("Ground move speed")]
    [SerializeField] private float moveSpeed = 7.0f;
    [Tooltip("Ground accel")]
    [SerializeField] private float runAcceleration = 14.0f;
    [Tooltip("Deacceleration that occurs when running on the ground")]
    [SerializeField] private float runDeacceleration = 10.0f;
    [Tooltip("Ground friction")]
    [SerializeField] private float friction = 6;
    [SerializeField] private float gravity = 20.0f;

    private void Awake()
    {
        BPhysicsWorld.Get().AddAction(this);
        characterMover = GetComponent<CharacterMover>();
        inputHandler = GetComponent<InputHandler>();
    }

    //Bullets version of FixedUpdate. 
    public void UpdateAction(CollisionWorld collisionWorld, float deltaTimeStep)
    {
        //Movement Logic
        Vector3 wishDir = transform.TransformDirection(inputHandler.GetWishDir()).normalized;
        Vector3 currentPos = transform.position;

        ApplyFriction();
        Accelerate(wishDir, deltaTimeStep);

        // air
        //playerVelocity.y -= gravity*deltaTimeStep;
        // ground
        playerVelocity.y = -gravity * deltaTimeStep;

        Vector3 targetPos = currentPos + (playerVelocity * deltaTimeStep); //Should I be multiplying by deltatime here?

        characterMover.MoveCharacter(collisionWorld, currentPos.ToBullet(), targetPos.ToBullet());
    }

    private void ApplyFriction()
    {
        Vector3 vec = playerVelocity; // Equivalent to: VectorCopy();
        float speed;
        float newspeed;
        float control;
        float drop;

        vec.y = 0.0f;
        speed = vec.magnitude;
        drop = 0.0f;

        /* Only if the player is on the ground then apply friction */
        control = speed < runDeacceleration ? runDeacceleration : speed;
        drop = control * friction * Time.deltaTime;

        newspeed = speed - drop;
        if (newspeed < 0)
            newspeed = 0;
        if (speed > 0)
            newspeed /= speed;

        playerVelocity.x *= newspeed;
        playerVelocity.z *= newspeed;
    }

    private void Accelerate(Vector3 wishdir, float deltaTimeStep)
    {
        float addspeed;
        float accelspeed;
        float currentspeed;

        currentspeed = Vector3.Dot(playerVelocity, wishdir);
        addspeed = moveSpeed - currentspeed;
        if (addspeed <= 0)
            return;
        accelspeed = runAcceleration * deltaTimeStep * moveSpeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        playerVelocity.x += accelspeed * wishdir.x;
        playerVelocity.z += accelspeed * wishdir.z;
    }

    public void DebugDraw(IDebugDraw debugDrawer) { }

}
