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

    /* Comment out to suppress issues with unfinished code
    Vector3 Coherence(Collider[] colliders)
    {
        Vector3 coherence;
        foreach(Collider boid in colliders)
        {

        }

        return coherence;
    }

    Vector3 Seperation(Collider[] colliders)
    {
        Vector3 seperation;
        foreach (Collider boid in colliders)
        {

        }

        return seperation;
    }

    Vector3 Alignment(Collider[] colliders)
    {
        Vector3 alignment;
        foreach (Collider boid in colliders)
        {

        }

        return alignment;
    }

    Vector3 CalculateBoidMovement()
    {
        Collider[] colliders = Physics.OverlapSphere(this.transform.position, 0.2f, this.gameObject.layer);
        Vector3 Move = Vector3.zero;
        Move += Coherence(colliders);
        Move += Seperation(colliders);
        Move += Alignment(colliders);
        return Move;
    }*/

}
