using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class CapsuleController : MonoBehaviour
{
    public float Speed = 10.0f;

    void Start() { Cursor.lockState = CursorLockMode.Locked; }

    void Update()
    {
        var commandVector = new Vector3
        (
            Input.GetAxis("Horizontal"),
            0,
            Input.GetAxis("Vertical")
        );

        transform.Translate(commandVector * Speed * Time.deltaTime);

        if(Input.GetKeyDown("escape"))
            Cursor.lockState = CursorLockMode.None;
    }
}