using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject target;
    public float bulletSpeed = 1;
    Vector3 directionToTarget;

    void Update()
    {
        directionToTarget = (target.transform.position - transform.position).normalized;
        transform.Translate(directionToTarget * (Time.deltaTime * bulletSpeed));

        if (Vector3.Distance(target.transform.position, transform.position) < 0.2f)
        {
            Destroy(gameObject);
        }
    }
}