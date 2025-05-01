using System.Collections;
using System.Collections.Generic;
using UnityEngine.Splines;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using System.Numerics;

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

    private UnityEngine.Vector3 position;
    private UnityEngine.Vector3 direction;
    private SplinePath path;
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






    private UnityEngine.Vector3[] gizmoPositions;
    // CheckSpheres check obstacles for the cars.
    // Every Vector2 contains : x = distance, y = radius.
    // The first one checks Fix Car Obstacles. 
    // The second one checks MovingObjectCollider. 
    // The third one checks all obstacles to slow.
    UnityEngine.Vector2[] checkSpheres = {
        new UnityEngine.Vector2(5f, 0.5f),
        new UnityEngine.Vector2(5f, 1.5f),
        new UnityEngine.Vector2(7f, 2f)
    };

    private void ObstacleOnPath()
    {

        isShortRangeObstacle = false;
        isLongRangeObstacle = false;

        gizmoPositions = new UnityEngine.Vector3[checkSpheres.Length];

        for (int i = 0; i < checkSpheres.Length; i++)
        {
            float distance = checkSpheres[i].x;
            float radius = checkSpheres[i].y;

            float checkT = positionOnCurve + distance / pathLenght;
            if (checkT > 1f) checkT -= 1f;

            UnityEngine.Vector3 checkPoint = (UnityEngine.Vector3)splinePaths[0].EvaluatePosition(checkT) + new UnityEngine.Vector3(0, 1, 0);
            gizmoPositions[i] = checkPoint;

            Collider[] hitColliders = Physics.OverlapSphere(checkPoint, radius);

            foreach (Collider collider in hitColliders)
            {
                bool tagMatch = false;

                if (i == 0)
                    tagMatch = collider.CompareTag("CarObstacle");
                else if (i == 1)
                    tagMatch = collider.CompareTag("MovingObjectCollider");
                else if (i == 2)
                    tagMatch = collider.CompareTag("MovingObjectCollider") || collider.CompareTag("CarObstacle");

                if (tagMatch && collider.gameObject != gameObject)
                {
                    if (i == 0 || i == 1)
                    {
                        isShortRangeObstacle = true;
                        elapsedTimeSinceMovingObstacleEnter = 0f;
                    }
                    else if (i == 2)
                    {
                        isLongRangeObstacle = true;
                    }
                }
            }
        }

        // Détection de virage (à partir du dernier point)
        UnityEngine.Vector3 lastTangent = splinePaths[0].EvaluateTangent(positionOnCurve + checkSpheres.Last().x / pathLenght);
        float angle = UnityEngine.Vector3.Angle(lastTangent.normalized, direction.normalized);
        bool turning = angle > 10f;

        targetSpeed = turning ? Mathf.Min(maxSpeed, maxSpeed / 2f) : maxSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (gizmoPositions == null) return;

        for (int i = 0; i < gizmoPositions.Length; i++)
        {
            Gizmos.DrawWireSphere(gizmoPositions[i], checkSpheres[i].y);
        }
    }


    
    // Collider logic. Only for pedestrians.
    public void OnFrontTriggerEnter(Collider collision)
    {
        if (isCar)return;
        if (collision.tag == "MovingObjectCollider" || collision.tag == "PedestrianObstacle")
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
        if (isCar)return;
        if (collision.tag == "MovingObjectCollider" || collision.tag == "PedestrianObstacle")
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
        if (paths != null)  // A path is assigned to it, calculated the trajectory.
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
            path = splinePaths[0];
            position = path.EvaluatePosition(positionOnCurve);
            direction = path.EvaluateTangent(positionOnCurve);
        }

        if (isHuman == true)
        {
            animator = transform.GetChild(0).GetComponent<Animator>();
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
        if (isCar) ObstacleOnPath();
        
        if (paths == null)
        {
            return;  // Nothing to do, we don't have a path to follow.
        }

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

        if (isLongRangeObstacle)
        {  // Go at half speed if there's something far.
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
        path = splinePaths[0];
        position = path.EvaluatePosition(positionOnCurve);
        direction = path.EvaluateTangent(positionOnCurve);
        transform.position = position;
        transform.LookAt(position + direction);
    }
}
