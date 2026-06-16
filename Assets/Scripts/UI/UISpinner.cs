using UnityEngine;
using System.Collections;

public class UISpinner : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 100f; // سرعت چرخش سریع
    [SerializeField] private float rotationAmount = 60f; // چقدر بچرخه (مثلا 60 درجه)
    [SerializeField] private float pauseDuration = 2f;   // چند ثانیه مکث کنه

    void Start()
    {
        StartCoroutine(RotateRoutine());
    }

    IEnumerator RotateRoutine()
    {
        while (true)
        {
            // ۱. چرخش
            Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, 0, -rotationAmount);
            float time = 0;
            float duration = 0.5f; // سرعت حرکت بین هر تیک

            while (time < duration)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, time / duration);
                time += Time.deltaTime;
                yield return null;
            }

            // ۲. مکث
            yield return new WaitForSeconds(pauseDuration);
        }
    }
}