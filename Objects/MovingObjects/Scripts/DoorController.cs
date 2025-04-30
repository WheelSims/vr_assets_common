using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private Transform LDoor; 
    [SerializeField] private Transform RDoor;
    [SerializeField] private Transform LDoorOpenedPositionTransform;
    [SerializeField] private Transform RDoorOpenedPositionTransform;
    [SerializeField] private float doorsSpeed;

    private Vector3 LDoorClosedPosition;
    private Vector3 RDoorClosedPosition;
    private int counter;
    private bool openedDoors;
    private bool closedDoors;
    void Start()
    {
        counter = 0;
        openedDoors = false;
        closedDoors = true;
        LDoorClosedPosition = LDoor.position;
        RDoorClosedPosition = RDoor.position;
    }

    void Update(){
        if (counter > 0 && !openedDoors){
            OpenTheDoor();
        }else if (counter == 0 && !closedDoors){
            CloseTheDoor();
        }
    }

    private void CloseTheDoor(){
        LDoor.transform.position = Vector3.MoveTowards(LDoor.transform.position, LDoorClosedPosition, doorsSpeed * Time.deltaTime);
        RDoor.transform.position = Vector3.MoveTowards(RDoor.transform.position, RDoorClosedPosition, doorsSpeed * Time.deltaTime);
        if (openedDoors) openedDoors = false;
        if (LDoor.transform.position == LDoorClosedPosition && RDoor.transform.position == RDoorClosedPosition){
            closedDoors = true;
        }
    }

    private void OpenTheDoor(){
        LDoor.transform.position = Vector3.MoveTowards(LDoor.transform.position, LDoorOpenedPositionTransform.position, doorsSpeed * Time.deltaTime);
        RDoor.transform.position = Vector3.MoveTowards(RDoor.transform.position, RDoorOpenedPositionTransform.position, doorsSpeed * Time.deltaTime);
        if (closedDoors) closedDoors = false;
        if (LDoor.transform.position == LDoorOpenedPositionTransform.position && RDoor.transform.position == RDoorOpenedPositionTransform.position){
            openedDoors = true;
        }
    }

    private void OnTriggerEnter(Collider collider){
        counter += 1;
    }

    void OnTriggerExit(Collider collider){
        counter -= 1;
        if (counter < 0){
            Debug.LogError("Counter should be positive: Counter = " + counter);
        }
    }
}
