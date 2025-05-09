using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowDown : MonoBehaviour
{
    MovingObject parentScript;

    void Start()
    {
        parentScript = transform.parent.parent.GetComponent<MovingObject>();
    }

    void OnTriggerEnter(Collider collision)
    {
        parentScript.OnFarTriggerEnter(collision);
    }

    void OnTriggerExit(Collider collision)
    {
        parentScript.OnFarTriggerExit(collision);
    }

}
