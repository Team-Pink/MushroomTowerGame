using UnityEngine;

public class Bullet : MonoBehaviour
{
    
    public float timeToTarget;
    private float timeElapsed;
    private Vector3 startPos;
    public Target target;
    private Vector3 targetPos;
    
    // parobola variables
    public bool parabola;
    private float progress = 0;
    private float arcHeight = 40;
    Vector3 currentPos;


    private void Awake()
    {
        startPos = transform.position;
        if (target.enemy != null)
            targetPos = target.enemy.transform.position;
    }

    void Update()
    {
        targetPos = target.position;


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
        transform.position = Vector3.Lerp(startPos, targetPos, timeElapsed / timeToTarget);
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