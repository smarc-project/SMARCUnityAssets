using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class UFO : MonoBehaviour {
    public float alpha = 0.25f;
    public Transform AUVTransform ;
    public float desired_height = 7f;
    public float desired_displacement= 5f;

    void Start()
    {
        if (AUVTransform == null)
        {
            Debug.LogWarning("No AUVTransform set for UFO sensor. Disabling.");
            enabled = false;
        }
    }

    void FixedUpdate() {
        float t = Time.time;
        float t_dash = t % (float)Math.Floor(2f * (float)desired_displacement / (float)alpha);
        transform.position = new Vector3(AUVTransform.position.x-desired_displacement + alpha*t_dash, desired_height, AUVTransform.position.z);
    }
}
