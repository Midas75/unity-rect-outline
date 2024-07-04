using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAroundOrigin : MonoBehaviour
{
    public float speed = 10f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(Vector3.zero,Vector3.up, speed * Time.deltaTime);
    }
}
