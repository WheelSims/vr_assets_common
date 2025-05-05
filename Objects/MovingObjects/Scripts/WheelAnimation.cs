using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelAnimation : MonoBehaviour
{
    [SerializeField] private GameObject FLWheel, FRWheel, BLWheel, BRWheel;
    private List<GameObject> Wheels;
    [SerializeField] private MovingObject movingObjectScript;
    private float currentSpeed;
    [SerializeField] private float wheelRadium;
    private float wheelsAngularSpeed;
    //private float 
    void Start()
    {
        Wheels = new List<GameObject> {FLWheel, FRWheel, BLWheel, BRWheel};
    }

    void Update()
    {
        currentSpeed = movingObjectScript.currentSpeed;

        wheelsAngularSpeed = (180/Mathf.PI) * currentSpeed / wheelRadium;

        foreach(GameObject Wheel in Wheels)
        {
            Wheel.transform.Rotate(Wheel.transform.right * wheelsAngularSpeed* Time.deltaTime, Space.World);
        }
    }
}
