using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantThing : MonoBehaviour
{
    Animator animator;

    [SerializeField]
    Transform leg1;
    [SerializeField]
    Transform leg2;
    [SerializeField]
    float minStepInterval;
    [SerializeField]
    float maxStepInterval;
    [SerializeField]
    float patrolRange;
    [SerializeField]
    float moveFootRange;
    [SerializeField]
    float moveFootSpeed;

    Transform prevLeg;
    Transform curLeg;

    float startTime;
    Vector3 startPos;
    Vector3 targetPos;
    float journeyLength;
    bool isMoving;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isMoving)
        {
            // Distance moved equals elapsed time times speed..
            float distCovered = (Time.time - startTime) * moveFootSpeed;

            // Fraction of journey completed equals current distance divided by total distance.
            float fractionOfJourney = distCovered / journeyLength;

            // Set our position as a fraction of the distance between the markers.
            curLeg.localPosition = Vector3.Lerp(startPos, targetPos, fractionOfJourney);
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

        startTime = Time.time;
        startPos = curLeg.localPosition;
        CapsuleCollider curFootCollider = curLeg.GetComponentInChildren<CapsuleCollider>();
        bool canLand;
        Vector2 rand;

        do
        {
            canLand = true;
            rand = UnityEngine.Random.insideUnitCircle * moveFootRange;
            targetPos = new Vector3(rand.x, 0, rand.y) + prevLeg.localPosition;
            journeyLength = Vector3.Distance(targetPos, startPos);

            // Do not step too close to the other foot or the original position
            // Foot may not have enough time to move to target position
            if (rand.magnitude < curFootCollider.radius * 2 | journeyLength < curFootCollider.radius * 2)
                canLand = false;

            // Do not step outside of patrol range
            if (Vector3.Distance(targetPos, transform.localPosition) > patrolRange)
                canLand = false;

            // Do not step on walls
            // Non-convex wall mesh collider cannot be detected
            Collider[] allOverlappingColliders = Physics.OverlapSphere(targetPos, curFootCollider.radius + 0.5f);
            foreach (Collider collider in allOverlappingColliders)
            {
                if(collider.tag == "Obstacle")
                    canLand = false;
            }
        } while (!canLand);
        Debug.Log(transform.localPosition + ", " + rand);
        isMoving = true;
    }

    void StopFoot()
    {
        isMoving = false;
    }

    void SwitchFoot()
    {
        animator.SetBool("Is Foot 1", !animator.GetBool("Is Foot 1"));
    }
}
