using BulletSharp;
using BulletUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = BulletSharp.Math.Vector3;

public class NextPositionFinder : MonoBehaviour, IAction
{
    private CharacterMover characterMover;
    private InputHandler inputHandler;

    [Range(1,5)]
    [SerializeField] private float speed;

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
        Vector3 wishDir = inputHandler.GetWishDir().ToBullet();
        Vector3 currentPos = transform.position.ToBullet();
        Vector3 targetPos = currentPos + (wishDir * speed * deltaTimeStep);

        characterMover.MoveCharacter(collisionWorld, currentPos, targetPos);
    }

    public void DebugDraw(IDebugDraw debugDrawer) { }

}
