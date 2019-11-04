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
    public Vector3 PlayerVelocity { get { return playerVelocity; } private set { playerVelocity = value; } }
    public float BaseSpeed { get { return moveSpeed; } private set { moveSpeed = value; } }
    public float MaxSpeed { get { return maxMoveSpeed; } private set { maxMoveSpeed = value; } }

    //Exposed
    [Tooltip("Ground move speed")]
    [SerializeField] private float moveSpeed = 7.0f;
    [Tooltip("Max player intended movement.")]
    [SerializeField] private float maxMoveSpeed = 17.5f;
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

    //Bullet Physic's version of FixedUpdate. 
    public void UpdateAction(CollisionWorld collisionWorld, float deltaTimeStep)
    {
        //Jank Super Jump
        jumpSpeed = Input.GetKey(KeyCode.LeftShift) ? 18 : 8;

        //Movement Logic
        Vector3 localWishDir = inputHandler.GetWishDir();
        Vector3 worldWishDir = transform.TransformDirection(inputHandler.GetWishDir()).normalized;

        Debug.DrawRay(transform.position, playerVelocity, Color.red);
        Debug.DrawRay(transform.position, worldWishDir * moveSpeed / 2f, Color.blue);

        QueueJump();

        if (GroundCheck())
        {
            GroundMove(worldWishDir, deltaTimeStep);
            GroundJump();
        }
        else
        {
            AirMove(worldWishDir, localWishDir, deltaTimeStep);
        }

        UserInputVelocityClamp();
        Vector3 currentPos = transform.position;
        Vector3 targetPos = currentPos + (playerVelocity * deltaTimeStep); //Should I be multiplying by deltatime here?
        characterMover.MoveCharacter(collisionWorld, currentPos.ToBullet(), targetPos.ToBullet());
    }

    private void UserInputVelocityClamp()
    {
        var velCop = playerVelocity;
        velCop.y = 0;
        velCop = Vector3.ClampMagnitude(velCop, maxMoveSpeed);
        playerVelocity.x = velCop.x;
        playerVelocity.z = velCop.z;
    }

    private void GroundMove(Vector3 wishDir, float deltaTimeStep)
    {
        ApplyFriction(deltaTimeStep);
        Accelerate(wishDir, moveSpeed, runAcceleration, deltaTimeStep);

        // Reset the gravity velocity. Seems unnecesary...
        playerVelocity.y = -gravity * deltaTimeStep;
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

    //Subtracts friction (runDeacceleration * friction) from current speed.
    private void ApplyFriction(float deltaTimeStep)
    {
        Vector3 vec = playerVelocity;
        vec.y = 0.0f;
        float speed = vec.magnitude;

        // Not 100% why friction increases when speed > runDeacceleration, seems fine though.
        float control = Mathf.Max(speed, runDeacceleration);
        // Friction is runDeacceleration * friction unless a jump is queued
        float drop = wishJump ? 0 : control * friction * deltaTimeStep;

        float newspeed = speed - drop;

        //Obviously friction won't cause you to go backward, just stop.
        if (newspeed < 0) newspeed = 0;
        if (speed > 0)
            //Find the value needed to scale playerVelocity by
            newspeed /= speed; 

        playerVelocity.x *= newspeed;
        playerVelocity.z *= newspeed;
    }

    //Adds scaled wishDir to playerVelocity.
    //https://www.desmos.com/calculator/vef9eqpcdt
    private void Accelerate(Vector3 wishdir, float wishspeed, float accel, float deltaTimeStep)
    {
        //Amount wishDir is in dir of vel between playerVelocity (same dir) and -playerVelocity.
        float currentspeed = Vector3.Dot(playerVelocity, wishdir);
        //Relative to wishspeed
        float addspeed = wishspeed - currentspeed;
        //No acceleration if you're faster than wishspeed. tsk tsk.
        if (addspeed <= 0)
            return;

        float accelspeed = wishspeed * accel * deltaTimeStep;
        //When wishdir and playerVelocity near same dir, use the smaller addspeed val.
        //Note when (and as approach) wishdir=playerVelocity, addspeed = (and approaches) wishspeed.
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        //However, speed will still increase above wishspeed when wishDir is acute and not equal to playerVelocity
        //This is where b-hopping mouse wiggle comes from.
        //It also means turning such that each update tick holds wish dir acute to velocity
        //helps gain speed each frame. i.e. Turning neither too slow or too fast will not lose speed, as one might expect.
        playerVelocity.x += accelspeed * wishdir.x;
        playerVelocity.z += accelspeed * wishdir.z;
    }  

    private bool GroundCheck()
    {
        return groundChecker.IsGrounded() && !jump;
    }

    private void GroundJump()
    {
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

        //Note no friction in air.
        Accelerate(worldWishDir, wishSpeed, accel, deltaTimeStep);
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
