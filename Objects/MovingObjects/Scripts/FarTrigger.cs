using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarColliderTrigger : MonoBehaviour
{
    MovingObject parentScript;

    // Start is called before the first frame update
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
