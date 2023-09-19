using UnityEngine;

public class Bullet : MonoBehaviour
{
    
    public float timeToTarget;
    private float timeElapsed;
    private Vector3 startPosition;
    public Target target;
    private Vector3 targetPosition;
    
    // parobola variables
    public bool parabola;
    private float progress;
    private float launchAngle;
    private float distance;

    private void Awake()
    {
        startPosition = transform.position;        
        if (target.enemy != null)
            targetPosition = target.enemy.transform.position;
    }

    void Update()
    {

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
        transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / timeToTarget);
    }

    void MoveParabola()
    {
        // Ideas                                                                                           problems
        // 1. predict where the enemy will be and shoot the bullet in a perfect parabola to that position. impossible to get accurate predictions without complex simulation.
        // 2. fire it along a line and modify the y value based on the distance to the target.             distance will be changing as the enemy moves.
        // 3. fire it along a parabola to the current changeing location of the target.                    requires a new parabola to be created every update.
        // 4. get the midpoint of the tower and target and rotate the bullet around that point.            unexplored maths required, would need the midpoint to be moveable.

        // Generate a parobola between the tower and enemy pos every movement generate a vector3 position
        // based on the progress of a float between 0 and 1 then rotate + lerp the bullet towards it.


    }
}