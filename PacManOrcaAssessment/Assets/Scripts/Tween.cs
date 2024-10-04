using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tween
{
    // Public properties with private setters
    public Transform Target { get; private set; }
    public Vector3 StartPos { get; private set; }
    public Vector3 EndPos { get; private set; }
    public float StartTime { get; private set; }
    public float Duration { get; private set; }

    // Constructor to initialize the properties
    public Tween(Transform target, Vector3 startPos, Vector3 endPos, float startTime, float duration)
    {
        Target = target;
        StartPos = startPos;
        EndPos = endPos;
        StartTime = startTime;
        Duration = duration;
    }

    // Method to update the target's position based on time passed
    // public void UpdatePosition(float currentTime)
    // {
    //     if (currentTime >= StartTime && currentTime <= StartTime + Duration)
    //     {
    //         float t = (currentTime - StartTime) / Duration;
    //         Target.position = Vector3.Lerp(StartPos, EndPos, t);
    //     }
    // }
}
