using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float timeToTarget;
    float timeElapsed;
    public Target target;
    Vector3 directionToTarget;
    Vector3 previousPos;

    
    void Update()
    {


        transform.position = Vector3.Lerp(transform.position, target.position, timeElapsed / timeToTarget);

        
        previousPos = transform.position;
        if (timeElapsed >= timeToTarget)
        {
            Destroy(gameObject);
        }
        timeElapsed += Time.deltaTime;
    }
}