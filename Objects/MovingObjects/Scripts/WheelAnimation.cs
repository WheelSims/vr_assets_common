using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WheelAnimation : MonoBehaviour
{
    [SerializeField] private GameObject FLWheel, FRWheel, BLWheel, BRWheel;
    private List<GameObject> Wheels;
    [SerializeField] private MovingObject movingObjectScript;
    private float currentSpeed;
    [SerializeField] private float wheelRadium;
    private float wheelsAngularSpeed;
    private Vector3 FWheelDirection;
    private int counter;

    void Start()
    {
        StartCoroutine(BackWheelSecurisation());
        counter = 0;
        Wheels = new List<GameObject> {FLWheel, FRWheel, BLWheel, BRWheel};
    }


    void Update()
    {
        //Turn on Y axis every 10 frames.
        FWheelDirection = movingObjectScript.frontWheelDirection;
        Vector3 FLcurrentEuler = FLWheel.transform.eulerAngles;
        Vector3 FRcurrentEuler = FRWheel.transform.eulerAngles;
        float angleY = Mathf.Atan2(FWheelDirection.x, FWheelDirection.z) * Mathf.Rad2Deg;        
        if (counter == 0)
        {
        FLWheel.transform.rotation = Quaternion.Euler(FLcurrentEuler.x, angleY, 0);
        FRWheel.transform.rotation = Quaternion.Euler(FRcurrentEuler.x, angleY, 0);
        }
        

        //Here we rotate the wheels on the X axis proportionnally to the car speed.
        //It doesn't work to turn on both axis. I don't know the reason. 
        //So when it turns on Y for the front wheels, it doesn't turn on X.
        currentSpeed = movingObjectScript.currentSpeed;
        wheelsAngularSpeed = (180/Mathf.PI) * currentSpeed / wheelRadium;
        for(int i = 0; i < Wheels.Count; i++)
        {
            //Stop X turning for Front wheels because they turn on Y at this frame.
            if (i<= 2 && counter == 0){
                continue;
            }
            Wheels[i].transform.Rotate(Wheels[i].transform.right * wheelsAngularSpeed* Time.deltaTime , Space.World);
        }

        counter += 1;
        if (counter > 10) counter = 0;
    }


    // It prevents some no-sense behaviour of the wheels
    private IEnumerator BackWheelSecurisation(){
        while(true){
            BLWheel.transform.LookAt(FLWheel.transform.position);
            BRWheel.transform.LookAt(FRWheel.transform.position);
            yield return new WaitForSeconds(10);
        }
    }
}
