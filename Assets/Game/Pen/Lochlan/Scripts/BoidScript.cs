using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidScript : MonoBehaviour
{
    public float speed = 10;
    Vector3 velocity;
    // Start is called before the first frame update
    void Start()
    {
        // Start moving in a random direction at a random speed.
        velocity = generateRandomMoveVec();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(velocity * Time.deltaTime);
    }

    Vector3 generateRandomMoveVec()
    {
        float x;
        x = Random.value;
        float z;
        z = Random.value;
        return new Vector3(x,0,z);
    }

}
