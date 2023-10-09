using UnityEngine;

public class Bullet : MonoBehaviour
{
    private bool initialised;

    public float timeToTarget;
    private float timeElapsed;
    private Vector3 startPos;
    public Target target;
    private Transform targetTransform;
    private Vector3 targetPos;
    
    // parobola variables
    public bool parabola;
    public float trackingSpeed = 0.001f; // the distance the targetPos will move towards the enemyPos in a frame
    private float progress = 0;
    private float arcHeight = 40;
    Vector3 currentPos;
    

    public void Initialise()
    {
        startPos = transform.position;
        targetTransform = target.enemy.transform;
        targetPos = targetTransform.position;
        initialised = true;
    }

    void Update()
    {
        if (!initialised) return;

        if (parabola)MoveParabola(); 
        else MoveStraightToTarget();
        
        if (timeElapsed >= timeToTarget)
        {
            Destroy(gameObject);
        }
        timeElapsed += Time.deltaTime;
    }

    void MoveStraightToTarget()
    {
        targetPos = targetTransform.position;
        transform.position = Vector3.Lerp(startPos, targetPos, Mathf.Min(timeElapsed / timeToTarget, 1));
    }

    void MoveParabola()
    {
        LooseTargetTracking();

        // update progress to match time elapsed
        progress = timeElapsed / timeToTarget;
        

        currentPos = Vector3.Lerp(startPos, targetPos, progress); // update xz position
        currentPos.y = -progress * progress + progress; // update y position
        currentPos.y *= arcHeight; 

        transform.rotation = Quaternion.LookRotation(currentPos); // rotate towards the direction of movement

        transform.position = currentPos; // Update position
       

    }

    void LooseTargetTracking()
    {
        targetPos = Vector3.MoveTowards( targetPos, targetTransform.position, trackingSpeed);
        Debug.DrawLine(transform.position, targetPos, Color.red, 0.02f);
    }
}