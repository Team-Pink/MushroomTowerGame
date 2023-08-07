using UnityEngine;

public class Bullet : MonoBehaviour
{
    GameObject target;
    public float bulletSpeed = 1;
    Vector3 directionToTarget;
    private void Start()
    {
        target = Physics.OverlapSphere(transform.position, 0.2f, LayerMask.GetMask("Range"))[0].gameObject.GetComponent<TurretController>().targetGameObject;
        if (target == null) Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        //transform.LookAt(turretController.targetGameObject.transform.position);
        
        directionToTarget = (target.transform.position - transform.position).normalized;
        transform.Translate(directionToTarget * (Time.deltaTime * bulletSpeed));

        if (Vector3.Distance(target.transform.position, transform.position) < 0.2f)
        {
            Destroy(gameObject);
        }
    }

}
