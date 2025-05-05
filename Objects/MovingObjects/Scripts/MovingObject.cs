using System.Collections.Generic;
using UnityEngine.Splines;
using UnityEngine;
using System.Runtime.Serialization.Formatters;

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

    private Vector3 position;
    private Vector3 direction;
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
    public float positionOnCurve = 0f;


    // Parameter used to modify animation of an avatar 
    private Animator animator;
    private float targetSpeed;
    public float currentSpeed = 0f;
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


    [Header("Trigger Colliders")]
    [SerializeField] private GameObject[] Triggers;
    private List<float> triggerDistanceFromEntity;
    private int currentObstacleCounter;
    private int currentFarObstacleCounter;

    private void TriggersMovement()
    {
        for (int i = 0; i < Triggers.Length; i++){
            if (pathLenght == 0){
                break;
            }
            float checkT;
            if (currentSpeed < 1 || isHuman){
                checkT = positionOnCurve + triggerDistanceFromEntity[i] / pathLenght;
            }else
            {
                checkT = positionOnCurve + (triggerDistanceFromEntity[i] + currentSpeed / 5)/ pathLenght;
            }
            
            if (checkT > 1f) checkT -= 1f;
            Vector3 checkPoint = (Vector3)splinePaths[0].EvaluatePosition(checkT);
            Triggers[i].transform.position = checkPoint;
        }
    }



    //Collider Logic
    public void OnFrontTriggerEnter(Collider collision)
    {
        if (collision.tag == "MovingObjectCollider" && collision.gameObject != gameObject )
        {
            currentObstacleCounter += 1;
            elapsedTimeSinceMovingObstacleEnter = 0f;
        }
    }

    public void OnFrontTriggerExit(Collider collision)
    {
        if (collision.tag == "MovingObjectCollider" && collision.gameObject != gameObject )
        {
            currentObstacleCounter -= 1;
            elapsedTimeSinceMovingObstacleExit = 0f;
        }
    }

    // These Triggers are only for cars.
    public void OnCloseTriggerEnter(Collider collision)
    {
        if (collision.tag == "StaticObjectCollider")
        {
            elapsedTimeSinceMovingObstacleEnter = 0f;
            currentObstacleCounter += 1;
        }
    }

    public void OnCloseTriggerExit(Collider collision)
    {
        if (collision.tag == "StaticObjectCollider")
        {
            currentObstacleCounter -= 1;
            elapsedTimeSinceMovingObstacleExit = 0f;
        }
    }
    public void OnFarTriggerEnter(Collider collision)
    {
        if (collision.tag == "MovingObjectCollider" || collision.tag == "StaticObjectCollider")
        {
            currentFarObstacleCounter += 1;
        }
    }
    public void OnFarTriggerExit(Collider collision)
    {
        if (collision.tag == "MovingObjectCollider" || collision.tag == "StaticObjectCollider")
        {
            currentFarObstacleCounter -= 1;
        }
    }

    void Start()
    {
        currentFarObstacleCounter = 0;
        currentObstacleCounter = 0;
        triggerDistanceFromEntity = new List<float>();
        float distance;
        foreach(GameObject trigger in Triggers){
            distance = trigger.transform.localPosition.z;
            triggerDistanceFromEntity.Add(distance);
        }
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
        else if (currentSpeed <= 0.5f)  // Idle
        {
            animator.SetInteger("Anim", 0);
        }
        else if (currentSpeed < 2f)  // Walk
        {
            animator.SetInteger("Anim", 1);
            animator.SetFloat("AnimMult", currentSpeed * 2/3);
        }
        else  // Run
        {
            animator.SetInteger("Anim", 2);
        }
    }


    private void Update()
    {
        TriggersMovement();

        if (currentObstacleCounter > 0){
            isShortRangeObstacle = true;
        }else{
            isShortRangeObstacle = false;
        }

        if (currentFarObstacleCounter > 0){
            isLongRangeObstacle = true;
        }else{
            isLongRangeObstacle = false;
        }
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


        // Détection de virage (à partir du dernier point)
        if (isCar){    
            Vector3 lastTangent = splinePaths[0].EvaluateTangent(positionOnCurve +  7 / pathLenght);
            float angle = Vector3.Angle(lastTangent.normalized, direction.normalized);
            bool turning = angle > 10f;

            targetSpeed = turning ? Mathf.Min(maxSpeed, maxSpeed / 2f) : maxSpeed;
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
