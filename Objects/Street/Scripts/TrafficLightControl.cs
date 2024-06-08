using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Directives
{
    NorthGreen, NorthYellow, NorthRed,
    SouthGreen, SouthYellow, SouthRed,
    EastGreen, EastYellow, EastRed,
    WestGreen, WestYellow, WestRed,
    Delay1s, Delay3s, Delay5s, Delay10s, Delay30s, Delay45s, Delay60s
}

public class TrafficLightControl : MonoBehaviour
{

    public List<Directives> sequence;

    public List<GameObject> northGreenActivated, northYellowActivated, southGreenActivated, southYellowActivated, eastGreenActivated, eastYellowActivated, westGreenActivated, westYellowActivated;

    [SerializeField]
    private Material matNorthGreen, matNorthYellow, matNorthRed, matSouthGreen, matSouthYellow, matSouthRed, matEastGreen, matEastYellow, matEastRed, matWestGreen, matWestYellow, matWestRed;
    private float elapsedTime = 0f;  // Elapsed time since last directive
    private float delay = 0f;  // Time to stay on this directive
    private int currentDirectiveIndex;


    void MoveUp(GameObject objectToMove)
    {
        //Move object to same level as the traffic light object
        Vector3 objectPosition = objectToMove.transform.position;
        objectPosition.y = transform.position.y;
        objectToMove.transform.position = objectPosition;
    }

    void MoveDown(GameObject objectToMove)
    {
        //Move object 5 meters below the traffic light object
        Vector3 objectPosition = objectToMove.transform.position;
        objectPosition.y = transform.position.y - 5;
        objectToMove.transform.position = objectPosition;
    }


    void NextDirective()
    {
        currentDirectiveIndex += 1;
        if (currentDirectiveIndex >= sequence.Count) currentDirectiveIndex = 0;


        var currentDirective = sequence[currentDirectiveIndex];
        switch (currentDirective)
        {
            case Directives.NorthRed:
                matNorthGreen.SetFloat("_OnOff", 0);
                matNorthYellow.SetFloat("_OnOff", 0);
                matNorthRed.SetFloat("_OnOff", 1);
                delay = 0f;
                break;
            case Directives.NorthYellow:
                matNorthGreen.SetFloat("_OnOff", 0);
                matNorthYellow.SetFloat("_OnOff", 1);
                matNorthRed.SetFloat("_OnOff", 0);
                foreach (GameObject controlledObject in northYellowActivated) { MoveUp(controlledObject); }
                foreach (GameObject controlledObject in northGreenActivated) { MoveDown(controlledObject); }
                delay = 0f;
                break;
            case Directives.NorthGreen:
                matNorthGreen.SetFloat("_OnOff", 1);
                matNorthYellow.SetFloat("_OnOff", 0);
                matNorthRed.SetFloat("_OnOff", 0);
                foreach (GameObject controlledObject in northYellowActivated) { MoveDown(controlledObject); }
                foreach (GameObject controlledObject in northGreenActivated) { MoveUp(controlledObject); }
                delay = 0f;
                break;
            case Directives.SouthRed:
                matSouthGreen.SetFloat("_OnOff", 0);
                matSouthYellow.SetFloat("_OnOff", 0);
                matSouthRed.SetFloat("_OnOff", 1);
                delay = 0f;
                break;
            case Directives.SouthYellow:
                matSouthGreen.SetFloat("_OnOff", 0);
                matSouthYellow.SetFloat("_OnOff", 1);
                matSouthRed.SetFloat("_OnOff", 0);
                foreach (GameObject controlledObject in southYellowActivated) { MoveUp(controlledObject); }
                foreach (GameObject controlledObject in southGreenActivated) { MoveDown(controlledObject); }
                delay = 0f;
                break;
            case Directives.SouthGreen:
                matSouthGreen.SetFloat("_OnOff", 1);
                matSouthYellow.SetFloat("_OnOff", 0);
                matSouthRed.SetFloat("_OnOff", 0);
                foreach (GameObject controlledObject in southYellowActivated) { MoveDown(controlledObject); }
                foreach (GameObject controlledObject in southGreenActivated) { MoveUp(controlledObject); }
                delay = 0f;
                break;
            case Directives.EastRed:
                matEastGreen.SetFloat("_OnOff", 0);
                matEastYellow.SetFloat("_OnOff", 0);
                matEastRed.SetFloat("_OnOff", 1);
                delay = 0f;
                break;
            case Directives.EastYellow:
                matEastGreen.SetFloat("_OnOff", 0);
                matEastYellow.SetFloat("_OnOff", 1);
                matEastRed.SetFloat("_OnOff", 0);
                foreach (GameObject controlledObject in eastYellowActivated) { MoveUp(controlledObject); }
                foreach (GameObject controlledObject in eastGreenActivated) { MoveDown(controlledObject); }
                delay = 0f;
                break;
            case Directives.EastGreen:
                matEastGreen.SetFloat("_OnOff", 1);
                matEastYellow.SetFloat("_OnOff", 0);
                matEastRed.SetFloat("_OnOff", 0);
                foreach (GameObject controlledObject in eastYellowActivated) { MoveDown(controlledObject); }
                foreach (GameObject controlledObject in eastGreenActivated) { MoveUp(controlledObject); }
                delay = 0f;
                break;
            case Directives.WestRed:
                matWestGreen.SetFloat("_OnOff", 0);
                matWestYellow.SetFloat("_OnOff", 0);
                matWestRed.SetFloat("_OnOff", 1);
                delay = 0f;
                break;
            case Directives.WestYellow:
                matWestGreen.SetFloat("_OnOff", 0);
                matWestYellow.SetFloat("_OnOff", 1);
                matWestRed.SetFloat("_OnOff", 0);
                foreach (GameObject controlledObject in westYellowActivated) { MoveUp(controlledObject); }
                foreach (GameObject controlledObject in westGreenActivated) { MoveDown(controlledObject); }
                delay = 0f;
                break;
            case Directives.WestGreen:
                matWestGreen.SetFloat("_OnOff", 1);
                matWestYellow.SetFloat("_OnOff", 0);
                matWestRed.SetFloat("_OnOff", 0);
                foreach (GameObject controlledObject in westYellowActivated) { MoveDown(controlledObject); }
                foreach (GameObject controlledObject in westGreenActivated) { MoveUp(controlledObject); }
                delay = 0f;
                break;
            case Directives.Delay1s:
                elapsedTime = 0f;
                delay = 1f;
                break;
            case Directives.Delay3s:
                elapsedTime = 0f;
                delay = 3f;
                break;
            case Directives.Delay5s:
                elapsedTime = 0f;
                delay = 5f;
                break;
            case Directives.Delay10s:
                elapsedTime = 0f;
                delay = 10f;
                break;
            case Directives.Delay30s:
                elapsedTime = 0f;
                delay = 30f;
                break;
            case Directives.Delay45s:
                elapsedTime = 0f;
                delay = 45f;
                break;
            case Directives.Delay60s:
                elapsedTime = 0f;
                delay = 60f;
                break;
        }



    }


    void Start()
    {
        currentDirectiveIndex = -1;
        NextDirective();
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime > delay)
        {
            NextDirective();
        }
    }
}
