using UnityEngine;
using System.Collections;

// Lifted from : https://gist.github.com/mstevenson/5103365
public class Fps : MonoBehaviour
{
    private float currentFps;
    private float smoothedFps;
    private float smoothingFactor = 0.1f; // Adjust this to control how much weight is given to recent FPS

    private IEnumerator Start()
    {
        GUI.depth = 2;
        while (true)
        {
            currentFps = 1f / Time.unscaledDeltaTime;

            // Apply exponential smoothing to calculate the weighted average
            smoothedFps = (smoothingFactor * currentFps) + (1f - smoothingFactor) * smoothedFps;

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnGUI()
    {
        // Display the smoothed FPS
        GUI.Label(new Rect(0, 100, 100, 25), "FPS: " + Mathf.Round(smoothedFps));
    }
}