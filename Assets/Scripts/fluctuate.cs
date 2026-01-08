using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fluctuate : MonoBehaviour
{
    public Vector3 from = new Vector3(0f, 0f, 0f);
    public Vector3 to = new Vector3(0f, 0.1f, 0f);

    public float speed = 0.1f;
    // Update is called once per frame
    void Update()
    {
        float t = (Mathf.Sin(Time.time * speed * Mathf.PI * 2.0f) + 1.0f) / 2.0f;
        transform.position = Vector3.Lerp(from, to, t);
    }
}
