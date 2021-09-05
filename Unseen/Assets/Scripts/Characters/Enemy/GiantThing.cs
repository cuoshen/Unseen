using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GiantThing : MonoBehaviour
{
    Animator animator;

    [SerializeField]
    Transform leg1;
    [SerializeField]
    Transform leg2;
    [SerializeField]
    Transform voice;
    [SerializeField]
    float minStepInterval;
    [SerializeField]
    float maxStepInterval;
    [SerializeField]
    float patrolRange;
    [SerializeField]
    float moveFootRange;

    Transform prevLeg;
    Transform curLeg;

    float moveStartTime;
    Vector3 startPos;
    Vector3 targetPos;
    float journeyLength;
    bool isMoving;

    float idleStartTime;
    float stepInterval;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isMoving)
        {
            // Distance moved equals elapsed time times speed.
            // 2 * moveFootRange allows the foot to traverse diameter within 1 second, which is the duration of the animation.
            float distCovered = (Time.time - moveStartTime) * 2 * moveFootRange;

            // Fraction of journey completed equals current distance divided by total distance.
            float fractionOfJourney = distCovered / journeyLength;

            // Set our position as a fraction of the distance between the markers.
            curLeg.position = Vector3.Lerp(startPos, targetPos, fractionOfJourney);

            voice.position = (prevLeg.position + curLeg.position) / 2;
        }

        if(animator.GetCurrentAnimatorStateInfo(0).IsName("Giant Idle"))
        {
            if(Time.time - idleStartTime < stepInterval)
            {
                animator.speed = 0;
            }
            else
            {
                animator.speed = 1;
            }
        }
    }

    void FindFootLandPosition()
    {
        if (animator.GetBool("Is Foot 1"))
        {
            prevLeg = leg2;
            curLeg = leg1;
        }
        else
        {
            prevLeg = leg1;
            curLeg = leg2;
        }

        moveStartTime = Time.time;
        startPos = curLeg.position;
        CapsuleCollider curFootCollider = curLeg.GetComponentInChildren<CapsuleCollider>();
        LayerMask maze_layer = 1 << LayerMask.NameToLayer("Maze");
        bool canLand;
        Vector2 rand;

        do
        {
            canLand = true;
            rand = UnityEngine.Random.insideUnitCircle * moveFootRange;
            targetPos = new Vector3(rand.x, 0, rand.y) + prevLeg.position;
            journeyLength = Vector3.Distance(targetPos, startPos);

            // Do not step too close to the other foot
            if (rand.magnitude < curFootCollider.radius * 2)
                canLand = false;

            // Do not step outside of patrol range
            if (Vector3.Distance(targetPos, transform.position) > patrolRange)
                canLand = false;

            // Do not step into empty space
            Collider[] allOverlappingColliders = Physics.OverlapSphere(targetPos, 1f);
            if (allOverlappingColliders.Length == 0)
                canLand = false;

            // Do not step on walls
            // Non-convex wall mesh collider cannot be detected
            Collider[] allOverlappingMazeColliders = Physics.OverlapSphere(targetPos, curFootCollider.radius + 0.5f, maze_layer);
            if (allOverlappingMazeColliders.Length != 0)
                canLand = false;
        } while (!canLand);

        isMoving = true;
    }

    void StopFoot()
    {
        isMoving = false;
    }

    void SwitchFoot()
    {
        if (curLeg != null)
        {
            curLeg.GetComponentInChildren<ParticleSystem>().Play();
            curLeg.GetComponentInChildren<CinemachineImpulseSource>().GenerateImpulse();
            curLeg.GetComponent<AudioSource>().Play();
        }

        animator.SetBool("Is Foot 1", !animator.GetBool("Is Foot 1"));

        idleStartTime = Time.time;
        stepInterval = UnityEngine.Random.Range(minStepInterval, maxStepInterval);
    }
}
