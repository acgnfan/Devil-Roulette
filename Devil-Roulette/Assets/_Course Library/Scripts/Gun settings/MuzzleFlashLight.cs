using UnityEngine;
using System.Collections;

public class MuzzleFlashLight : MonoBehaviour
{
    public Light muzzleLight;

    [Header("Flash Settings")]
    public float maxIntensity = 6f;
    public float totalDuration = 0.06f;

    [Header("Intensity Curve")]
    public AnimationCurve intensityCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    Coroutine flashRoutine;

    public void Fire()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        float t = 0f;
        muzzleLight.intensity = 0f;

        while (t < totalDuration)
        {
            float normalizedTime = t / totalDuration;
            float curveValue = intensityCurve.Evaluate(normalizedTime);

            muzzleLight.intensity = curveValue * maxIntensity;

            t += Time.deltaTime;
            yield return null;
        }

        muzzleLight.intensity = 0f;
    }
}
