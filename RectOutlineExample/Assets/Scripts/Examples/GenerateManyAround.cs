using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateManyAround : MonoBehaviour
{
    public uint total = 50;
    public float interval = 0.5f;
    public GameObject target;
    void Awake()
    {
        int currentStep = 1;
        int current = 1;
        Vector3 currentPointer = target.transform.position;
        Vector2 currentDirection = Vector2.up;
        while (true)
        {
            if (current >= total) break;
            for (int i = 0; i < currentStep; i++)
            {
                var offset = currentDirection * interval;
                currentPointer += new Vector3(offset.x, 0f, offset.y);
                PutNewObject(currentPointer);
                current += 1;
            }
            if (current >= total) break;
            currentDirection = Rotate90(currentDirection);
            for (int i = 0; i < currentStep; i++)
            {
                var offset = currentDirection * interval;
                currentPointer += new Vector3(offset.x, 0f, offset.y);
                PutNewObject(currentPointer);
                current += 1;
            }
            currentDirection = Rotate90(currentDirection);
            currentStep += 1;
        }
    }
    void PutNewObject(Vector3 pos)
    {
        var newItem = Instantiate(target);
        if (newItem.GetComponent<GenerateManyAround>())
        {
            newItem.GetComponent<GenerateManyAround>().enabled = false;
        }
        newItem.transform.position = pos;
    }
    Vector2 Rotate90(Vector2 v2)
    {
        return new(v2.y, -v2.x);
    }
    // Update is called once per frame
    void Update()
    {

    }
}
