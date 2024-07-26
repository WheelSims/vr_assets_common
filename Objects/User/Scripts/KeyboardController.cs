using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // React to keyboard
        float linearSpeed = 0f;  // default
        float angularSpeed = 0f;  // default
        if (Input.GetKey(KeyCode.UpArrow)) linearSpeed = 1f;  // m/s
        if (Input.GetKey(KeyCode.DownArrow)) linearSpeed = -0.5f;  // m/s
        if (Input.GetKey(KeyCode.LeftArrow)) angularSpeed = -60f;  // deg/s
        if (Input.GetKey(KeyCode.RightArrow)) angularSpeed = 60f;  // deg/s

        // Rotate around y - axis
        transform.Rotate(0, angularSpeed * Time.deltaTime, 0);

        // Move forward / backward
        transform.Translate(0, 0, linearSpeed * Time.deltaTime);
    }
}
