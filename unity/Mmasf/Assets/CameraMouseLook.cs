using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class CameraMouseLook : MonoBehaviour
{
    public float Sensitivity = 5;
    public float Smoothing = 2;
    GameObject Character;
    Vector2 SmoothVector;
    Vector2 Direction;

    void Start() { Character = transform.parent.gameObject; }

    void Update()
    {
        SmoothVector = GetNewSmoothVector();
        Direction += SmoothVector;
        Apply();
    }

    void Apply()
    {
        transform.localRotation
            = Quaternion.AngleAxis(Direction.y, Vector3.left);
        Character.transform.localRotation
            = Quaternion.AngleAxis(Direction.x, Character.transform.up);
    }

    Vector2 GetNewSmoothVector()
    {
        return SmoothVector
            + (Command * SensitivitySmoothing - SmoothVector) * InversSmoothing;
    }

    float InversSmoothing { get { return Mathf.Clamp01(1f / Smoothing); } }
    float SensitivitySmoothing { get { return Sensitivity * Smoothing; } }

    static Vector2 Command
    {
        get
        {
            return new Vector2
            (
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Mouse Y")
            );
        }
    }
}