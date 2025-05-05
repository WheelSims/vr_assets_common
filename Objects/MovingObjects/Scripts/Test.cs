using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(transform.right *50 * Time.deltaTime, Space.World);
    }
}
