using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mmasf
{
    public sealed class CapsuleController : MonoBehaviour
    {
        public float Speed = 10.0f;
        public bool IsCursorLocked = true;

        void Update()
        {
            ApplyMove();
            UpdateLockState();
        }

        void UpdateLockState()
        {
            if(!IsCursorLocked && Input.GetMouseButtonDown(0))
                IsCursorLocked = true;

            if(IsCursorLocked && Input.GetKeyDown("escape"))
                IsCursorLocked = false;

            Cursor.lockState = IsCursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
        }

        void ApplyMove()
        {
            if(!IsCursorLocked)
                return;

            var commandVector = new Vector3
            (
                Input.GetAxis("Horizontal"),
                0,
                Input.GetAxis("Vertical")
            );

            transform.Translate(commandVector * Speed * Time.deltaTime);
        }
    }
}