using UnityEngine;
using System;
using System.Collections;

public static class CoroutineHelpers
{
    private static IEnumerator DelayedAction(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

    public static Coroutine DelayedAction(Action action, float delay, MonoBehaviour monoBehaviour)
    {
        return monoBehaviour.StartCoroutine(DelayedAction(action, delay));
    }

    public static Coroutine LerpAction(
        this MonoBehaviour monoBehaviour,
        Action<float> action,
        float duration,
        Action onComplete
    )
    {
        return monoBehaviour.StartCoroutine(LerpAction(action, duration, onComplete));
    }

    private static IEnumerator LerpAction(
        Action<float> action,
        float duration,
        Action onComplete = null
    )
    {
        float startTime = Time.time;
        float endTime = startTime + duration;
        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / duration;
            action(t);
            yield return null;
        }
        onComplete?.Invoke();
    }

    private static IEnumerator ChangeColor(
        object obj,
        Color startColor,
        Color endColor,
        float duration
    )
    {
        float startTime = Time.time;
        float endTime = startTime + duration;
        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / duration;
            Color newColor = Color.Lerp(startColor, endColor, t);
            if (obj == null)
            {
                yield break;
            }
            obj.GetType().GetProperty("color").SetValue(obj, newColor, null);
            yield return null;
        }
    }

    public static void ChangeColor(
        object obj,
        Color startColor,
        Color endColor,
        float duration,
        MonoBehaviour monoBehaviour
    )
    {
        monoBehaviour.StartCoroutine(ChangeColor(obj, startColor, endColor, duration));
    }
}