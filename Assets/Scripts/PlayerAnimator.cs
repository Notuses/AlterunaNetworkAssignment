using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alteruna.Trinity;
using Alteruna;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Alteruna.Avatar avatar;

    [SerializeField] float MovementSmoothing;

    [SerializeField] private Animator anim;
    private RigidbodySynchronizable rbSync;
    [SerializeField] private PlayerActions playerActions;
    [SerializeField] private VelocityRemoteSync velSync;
    private Rigidbody rb;


    private Vector3 lastPosition;
    private float velocityDelta;
    private float prevDelta;
    private float smoothVelocityDelta;

    private float prevAngle;
    private float turnAngle;
    private float smoothedTurnAngle;

    [SerializeField] float Coefficent = 1;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        //rbSync = GetComponent<RigidbodySynchronizable>();
    }

    private void Start()
    {
        //Bind event to play spell cast animation here
        if (playerActions)
        {
            playerActions.OnShoot += PlayShootAnimation;
            playerActions.OnTryDeflect += PlayDeflectAnimation;
            playerActions.OnTaunt += PlayTauntAnimation;

        }
        else
        {
            Debug.Log("PlayerActions is not set for the Animator script");
        }
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 positionDelta = transform.position - lastPosition;
        velocityDelta = positionDelta.magnitude;
        lastPosition = transform.position;

        

        //prevDelta = velocityDelta;
        prevDelta = prevDelta * 0.2f + velocityDelta * 0.8f;

        
        prevAngle = turnAngle;

        turnAngle = CalculateDirection(positionDelta);

        //anim.SetFloat("VelocityMagnitude", velocityDelta * Coefficent);


        //Use doubled values if we arent the avatar as the animator value flashes to 0 every other frame for some reason. Awful workaround
        
        if (!avatar.IsMe) //TODO: This changes sometimes for seemingly no reason. Perhaps cache this value to ensure it isnt changing!
        {
            anim.SetFloat("StrafeX", turnAngle * 1, 0.2f, Time.deltaTime);
            anim.SetFloat("VelocityMagnitude", velSync.VelocityMagnitude * 20, 0.2f, Time.deltaTime);
        }
        else
        {
            anim.SetFloat("StrafeX", turnAngle * 2, 0.2f, Time.deltaTime);
            anim.SetFloat("VelocityMagnitude", rb.velocity.magnitude * 10, 0.2f, Time.deltaTime);
        }

        
       

    }

    float CalculateDirection(Vector3 velocity)
    {
        if (velocity.magnitude > 0.01)
        {
            float ForwardCosAngle = Vector3.Dot(transform.forward, velocity.normalized);
            float ForwardDeltaDegree = Mathf.Rad2Deg * Mathf.Acos(ForwardCosAngle);

            float RightCosangle = Vector3.Dot(transform.right, velocity.normalized);
            if (RightCosangle < 0)
            {
                ForwardDeltaDegree *= -1;
            }

            return ForwardDeltaDegree;
        }
        return 0.0f;
    }

    private void LateUpdate()
    {
        //anim.SetFloat("VelocityMagnitude", velSync.VelocityMagnitude * 2);
        
        //This is jittery
        float SmoothedVelocity = Mathf.SmoothDamp(prevDelta, velocityDelta, ref smoothVelocityDelta, MovementSmoothing, 0.01f, Time.deltaTime);

        float SmoothedTurn = Mathf.SmoothDampAngle(prevAngle, turnAngle, ref smoothedTurnAngle, 0.01f);
        
        
    }

    //Plays animation once on action layer. Like an Unreal Engine montage
    private void PlayAction(string ActionName, float time) 
    {
        anim.Play(ActionName, 1, time);
    }

    private void PlayShootAnimation()
    {
        //PlayAction("Shoot", 0.15f);
        anim.CrossFade("Shoot", 0.1f, 1, 0);
    }

    private void PlayTauntAnimation()
    {
        anim.CrossFade("Taunt", 0.1f, 1, 0);
    }

    private void PlayDeflectAnimation()
    {
        anim.CrossFade("Deflect", 0.1f, 1, 0.0f);
    }
}
