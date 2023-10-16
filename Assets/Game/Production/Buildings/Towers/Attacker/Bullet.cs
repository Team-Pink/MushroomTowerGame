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
    private float progress = 0;
    private float arcHeight = 40;
    Vector3 currentPos;

    public void InitialiseForNonEnemies(Transform _transform)
    {
        startPos = transform.position;
        targetTransform = _transform;
        initialised = true;
    }

    public void Initialise()
    {
        startPos = transform.position;
        targetTransform = target.enemy.transform;
        initialised = true;
    }

    void Update()
    {
        if (!initialised) return;

        targetPos = targetTransform.position;


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
        transform.position = Vector3.Lerp(startPos, targetPos, Mathf.Min(timeElapsed / timeToTarget, 1));
    }

    void MoveParabola()
    {

        // update progress to match time elapsed
        progress = timeElapsed / timeToTarget;
        

        currentPos = Vector3.Lerp(startPos, targetPos, progress); // update xz position
        currentPos.y = -progress * progress + progress; // update y position
        currentPos.y *= arcHeight; 

        transform.rotation = Quaternion.LookRotation(currentPos); // rotate towards the direction of movement

        transform.position = currentPos; // Update position
       

    }
}