using System.Collections;
using System.Collections.Generic;
using UnityEngine.Splines;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

/*
Script Name: MovingObject
Description: Script that manages collisions between cars and humans
Author     : Félix Chénier, Laurent Gosselin
*/


/* Used as a selection box in the editor. */
public enum ObjTypeA
{
    Human,
    Car
}


/* Main behaviour class. */
public class MovingObject : MonoBehaviour
{
    public ObjTypeA objType;
    private bool isHuman = false;
    private bool isCar = false;

    [Range(0, 20)][SerializeField] float maxSpeed;


    // object for animation on curve 
    [SerializeField] SplineContainer paths;
    [SerializeField] int splineIndex;
    [Range(0, 1)][SerializeField] float startingPoint;




    // basic curve parameter 
    private float pathLenght;
    private int nbKnotsInSpline;
    private SplinePath[] splinePaths;

    // Parameter used to procedurally animate the progression 
    private float positionOnCurve = 0f;


    // Parameter used to modify animation of an avatar 
    private Animator animator;
    private float targetSpeed;
    private float currentSpeed = 0f;
    public bool isShortRangeObstacle = false;
    public bool isLongRangeObstacle = false;
    private float finalTargetSpeed;


    // Parameters relative to collisions
    [Header("Advanced settings")]
    public float waitTimeAfterIdle = 0.5f;  // Seconds before resetting isShortRangeObstacle and targetSpeed to default
    public float maxIdleTimeBeforeRespawn = 60f;  // Seconds before resetting the position of the object to its starting point (in case of mutual stuck)

    [Header("Variables shown for debugging only")]
    public float elapsedTimeSinceMovingObstacleEnter = 0f;
    public float elapsedTimeSinceMovingObstacleExit = 0f;


    // Collider logic
    public void OnFrontTriggerEnter(Collider collision)
    {
        if (collision.tag == "MovingObjectCollider")
        {
            isShortRangeObstacle = true;
            elapsedTimeSinceMovingObstacleEnter = 0f;
        }
        else if (collision.tag == "SpeedZoneCollider")
        {
            targetSpeed = Mathf.Min(maxSpeed, collision.GetComponent<SpeedZone>().speedLimit);
        }
    }

    public void OnFrontTriggerExit(Collider collision)
    {
        if (collision.tag == "MovingObjectCollider")
        {
            isShortRangeObstacle = false;
            elapsedTimeSinceMovingObstacleExit = 0f;
        }
        else if (collision.tag == "SpeedZoneCollider")
        {
            targetSpeed = maxSpeed;
        }

    }

    public void OnFarTriggerEnter(Collider collision)
    {
        if (collision.tag == "MovingObjectCollider")
        {
            isLongRangeObstacle = true;
        }
    }
    public void OnFarTriggerExit(Collider collision)
    {
        if (collision.tag == "MovingObjectCollider")
        {
            isLongRangeObstacle = false;
        }
    }


    void Start()
    {
        targetSpeed = maxSpeed;
        if (paths == null)
        {
            Debug.Log("Error: No path has been assigned to the script MovingObject of " + this.name);
            gameObject.SetActive(false);
        }
        else
        {

            switch (objType)
            {
                case ObjTypeA.Human:
                    isHuman = true;
                    isCar = false;
                    break;

                case ObjTypeA.Car:
                    isHuman = false;
                    isCar = true;
                    break;
            }


            //preparing for animation
            positionOnCurve = startingPoint;
            nbKnotsInSpline = paths.Splines[splineIndex].Count + 1;

            //Defining the path used in animation
            splinePaths = new SplinePath[1];
            var localToWorldMatrix = paths.transform.localToWorldMatrix;

            splinePaths[0] = new SplinePath(new[]
            {
                new SplineSlice<Spline>(paths.Splines[splineIndex], new SplineRange(0, nbKnotsInSpline), localToWorldMatrix),
            });

            //calculating the ratio of the curve for making sure speed is constant 
            pathLenght = splinePaths[0].GetLength();

            if (isHuman == true)
            {
                animator = transform.GetChild(0).GetComponent<Animator>();
            }
        }
    }


    private void UpdateAnimation()
    {
        if (currentSpeed < -0.001f)  // Walk backwards
        {
            animator.SetInteger("Anim", -1);
        }
        else if (currentSpeed <= 0f)  // Idle
        {
            animator.SetInteger("Anim", 0);
        }
        else if (currentSpeed < 2f)  // Walk
        {
            animator.SetInteger("Anim", 1);
        }
        else  // Run
        {
            animator.SetInteger("Anim", 2);
        }
    }


    private void Update()
    {
        // Slowly adapt the current speed to match the target.
        elapsedTimeSinceMovingObstacleEnter += Time.deltaTime;
        elapsedTimeSinceMovingObstacleExit += Time.deltaTime;

        // Check if we are stuck and reset our position if we are stuck for a long time
        if (isShortRangeObstacle && (elapsedTimeSinceMovingObstacleEnter > maxIdleTimeBeforeRespawn))
        {
            elapsedTimeSinceMovingObstacleEnter = 0f;
            positionOnCurve = startingPoint;
        }

        // Determine our speed
        // TODO Use acceleration sliders in the inspector.
        if ((isShortRangeObstacle == true) || (elapsedTimeSinceMovingObstacleExit < waitTimeAfterIdle))
        {
            finalTargetSpeed = 0f;
        }
        else
        {
            finalTargetSpeed = targetSpeed;
        }

        if (isLongRangeObstacle) {  // Go at half speed if there's something far.
            finalTargetSpeed /= 2f;
        }

        if (currentSpeed < finalTargetSpeed)
        {
            currentSpeed += 2 * Time.deltaTime;  // Slowly accelerate back to normal speed
            if (currentSpeed > finalTargetSpeed)
            {
                currentSpeed = finalTargetSpeed;
            }
            if (isHuman) UpdateAnimation();
        }
        else if (currentSpeed > finalTargetSpeed)
        {
            currentSpeed -= 20 * Time.deltaTime;  // Gradual brake
            if (currentSpeed < 0)
            {
                currentSpeed = 0;
            }
            if (isHuman) UpdateAnimation();
        }

        // Update position on curve
        positionOnCurve += currentSpeed * Time.deltaTime / pathLenght;
        if (positionOnCurve > 1f)
        {
            positionOnCurve = 0f;
        }
        var path = splinePaths[0];
        var pos = path.EvaluatePosition(positionOnCurve);
        var direction = path.EvaluateTangent(positionOnCurve);
        transform.position = pos;
        transform.LookAt(pos + direction);
    }
}
