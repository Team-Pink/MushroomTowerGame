using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float timeToTarget;
    private float timeElapsed;
    private Vector3 startPosition;
    public Target target;
    private Vector3 targetPosition;

    private void Awake()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (target.enemy != null)
            targetPosition = target.enemy.transform.position;

        transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / timeToTarget);
        
        if (timeElapsed >= timeToTarget)
        {
            Destroy(gameObject);
        }
        timeElapsed += Time.deltaTime;
    }
}