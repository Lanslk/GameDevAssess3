using System;
using System.Collections.Generic;
using UnityEngine;

public class Tweener : MonoBehaviour
{
    // Private member for active tween
    //private Tween activeTween;
    private List<Tween> activeTweens = new List<Tween>();

    private float speed = 0.1f;

    // Example method to add a tween
    public Boolean AddTween(Transform targetObject, Vector3 startPos, Vector3 endPos, float duration)
    {
        
        if (TweenExists(targetObject) == true)
        {
            return false;
        }
        else
        {
            activeTweens.Add(new Tween(targetObject, startPos, endPos, Time.time, duration));
            return true;
        }
    }

    // Call this in Update to move the target
    private void Update()
    {
        List<int> removeIndex = new List<int>();
        for (int i = 0; i < activeTweens.Count; i++)
        {
            Tween activeTween = activeTweens[i];

            if (activeTween != null)
            {
                // Calculate the fraction of interpolation based on time
                float elapsedTime = Time.time - activeTween.StartTime;
            
                // Calculate the fraction of the time passed
                float t = Mathf.Clamp(elapsedTime / activeTween.Duration, 0f, 1f);

                // Update the position based on the time fraction
                activeTween.Target.position = Vector3.Lerp(activeTween.StartPos, activeTween.EndPos, t);
                //print(activeTween.StartPos + " - " + activeTween.EndPos + " - " + t);

                // If the target has reached its destination, mark for removal
                if (t >= 1f)
                {
                    activeTween.Target.position = activeTween.EndPos;
                    removeIndex.Add(i);
                }
            }
        }

        // Remove completed tweens
        for (int i = removeIndex.Count - 1; i >= 0; i--)
        {
            activeTweens.RemoveAt(removeIndex[i]);
        }
    }

    public Boolean TweenExists(Transform target)
    {
        foreach (Tween tween in activeTweens)
        {
            if (tween.Target == target)
            {
                return true;
            }
        }

        return false;

    }
}