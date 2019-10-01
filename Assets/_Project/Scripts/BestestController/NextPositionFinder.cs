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
    private GroundedChecker groundChecker;

    //State
    private Vector3 playerVelocity = Vector3.zero;
    
    //Exposed
    [Tooltip("Ground move speed")]
    [SerializeField] private float moveSpeed = 7.0f;
    [Tooltip("Ground accel")]
    [SerializeField] private float runAcceleration = 14.0f;
    [Tooltip("Deacceleration that occurs when running on the ground")]
    [SerializeField] private float runDeacceleration = 10.0f;
    [Tooltip("Air accel")]
    [SerializeField] private float airAcceleration = 2.0f;
    [Tooltip("Deacceleration experienced when ooposite strafing")]
    [SerializeField] private float airDecceleration = 2.0f;
    [Tooltip("How precise air control is")]
    public float airControl = 0.3f;
    [Tooltip("Ground friction")]
    [SerializeField] private float friction = 6;
    [SerializeField] private float gravity = 20.0f;
    [Tooltip("The speed at which the character's up axis gains when hitting jump")]
    [SerializeField] private float jumpSpeed = 8.0f;
    [Tooltip("How fast acceleration occurs to get up to sideStrafeSpeed when")]
    [SerializeField] private float sideStrafeAcceleration = 50.0f;
    [Tooltip("What the max speed to generate when side strafing")]
    [SerializeField] private float sideStrafeSpeed = 1.0f;

    //More Temp
    bool wishJump = false;

    bool jump = false;

    private void Awake()
    {
        BPhysicsWorld.Get().AddAction(this);
        characterMover = GetComponent<CharacterMover>();
        inputHandler = GetComponent<InputHandler>();
        groundChecker = GetComponent<GroundedChecker>();
    }

    //Bullets version of FixedUpdate. 
    public void UpdateAction(CollisionWorld collisionWorld, float deltaTimeStep)
    {
        //Super Jump
        jumpSpeed = Input.GetKey(KeyCode.LeftShift) ? 18 : 8;

        //Movement Logic
        Vector3 localWishDir = inputHandler.GetWishDir();
        Vector3 worldWishDir = transform.TransformDirection(inputHandler.GetWishDir()).normalized;

        QueueJump();

        if (GroundCheck())
        {
            GroundMove(worldWishDir, deltaTimeStep);
        }
        else
        {
            AirMove(worldWishDir, localWishDir, deltaTimeStep);
        }

        Vector3 currentPos = transform.position;
        Vector3 targetPos = currentPos + (playerVelocity * deltaTimeStep); //Should I be multiplying by deltatime here?
        characterMover.MoveCharacter(collisionWorld, currentPos.ToBullet(), targetPos.ToBullet());
    }

    private void GroundMove(Vector3 wishDir, float deltaTimeStep)
    {
        ApplyFriction(deltaTimeStep);
        Accelerate(wishDir, runAcceleration, deltaTimeStep);

        // Reset the gravity velocity
        playerVelocity.y = -gravity * deltaTimeStep;

        //Ground Jump
        if (wishJump)
        {
            playerVelocity.y = jumpSpeed;
            wishJump = false;
            jump = true;
            StartCoroutine(JumpFix());
            GetComponent<AudioSource>().Play();
        }
    }

    /// <summary>
    /// Queues the next jump just like in Q3
    /// </summary>
    private void QueueJump()
    {
        if (Input.GetMouseButtonDown(1) && !wishJump)
            wishJump = true;
        if (Input.GetMouseButtonUp(1))
            wishJump = false;

        //if (Input.GetKeyDown(KeyCode.Space) && !wishJump)
        //    wishJump = true;
        //if (Input.GetKeyUp(KeyCode.Space))
        //    wishJump = false;
    }

    private void ApplyFriction(float deltaTimeStep)
    {
        Vector3 vec = playerVelocity;
        vec.y = 0.0f;
        float speed = vec.magnitude;

        float control = Mathf.Max(speed, runDeacceleration);
        float drop = wishJump ? 0 : control * friction * deltaTimeStep;

        float newspeed = speed - drop;

        if (newspeed < 0) newspeed = 0;
        if (speed > 0)
            newspeed /= speed;

        playerVelocity.x *= newspeed;
        playerVelocity.z *= newspeed;
    }

    private void Accelerate(Vector3 wishdir, float accel, float deltaTimeStep)
    {
        float addspeed;
        float accelspeed;
        float currentspeed;

        currentspeed = Vector3.Dot(playerVelocity, wishdir);
        addspeed = moveSpeed - currentspeed;
        if (addspeed <= 0)
            return;
        accelspeed = accel * deltaTimeStep * moveSpeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        playerVelocity.x += accelspeed * wishdir.x;
        playerVelocity.z += accelspeed * wishdir.z;
    }  

    private bool GroundCheck()
    {
        return groundChecker.IsGrounded() && !jump;
    }


    private void AirMove(Vector3 worldWishDir, Vector3 localWishDir, float deltaTimeStep)
    {
        float accel;
        if (Vector3.Dot(playerVelocity, worldWishDir) < 0)
            accel = airDecceleration;
        else
            accel = airAcceleration;

        float wishSpeed = moveSpeed;

        // If the player is ONLY strafing left or right
        if (Mathf.Approximately(localWishDir.z, 0) && !Mathf.Approximately(localWishDir.x, 0))
        {
            if (wishSpeed > sideStrafeSpeed)
                wishSpeed = sideStrafeSpeed;
            accel = sideStrafeAcceleration;
        }

        Accelerate(worldWishDir, wishSpeed, accel);
        if (airControl > 0)
            AirControl(worldWishDir, localWishDir, moveSpeed, deltaTimeStep);

        // Apply gravity
        playerVelocity.y -= gravity * Time.deltaTime;
    }

    /// <summary>
    /// Air control occurs when the player is in the air, it allows
    /// players to move side to side much faster rather than being
    /// 'sluggish' when it comes to cornering.
    /// </summary>
    private void AirControl(Vector3 worldWishDir, Vector3 localWishDir, float wishspeed, float deltaTimeStep)
    {
        float zspeed;
        float speed;
        float dot;
        float k;

        // Can't control movement if not moving forward or backward
        if (Mathf.Abs(localWishDir.z) < 0.001 || Mathf.Abs(wishspeed) < 0.001)
            return;
        zspeed = playerVelocity.y;
        playerVelocity.y = 0;
        /* Next two lines are equivalent to idTech's VectorNormalize() */
        speed = playerVelocity.magnitude;
        playerVelocity.Normalize();

        dot = Vector3.Dot(playerVelocity, worldWishDir);
        k = 32;
        k *= airControl * dot * dot * deltaTimeStep;

        // Change direction while slowing down
        if (dot > 0)
        {
            playerVelocity.x = playerVelocity.x * speed + worldWishDir.x * k;
            playerVelocity.y = playerVelocity.y * speed + worldWishDir.y * k;
            playerVelocity.z = playerVelocity.z * speed + worldWishDir.z * k;

            playerVelocity.Normalize();
        }

        playerVelocity.x *= speed;
        playerVelocity.y = zspeed; // Note this line
        playerVelocity.z *= speed;
    }

    private IEnumerator JumpFix()
    {
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        jump = false;
    }

    public void DebugDraw(IDebugDraw debugDrawer) { }

}
