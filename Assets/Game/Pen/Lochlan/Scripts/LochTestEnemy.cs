using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LochTestEnemy : MonoBehaviour
{
    Vector3 startPos;
    public Vector3 targetHeart;
    public LayerMask heartMask;
    Vector3 velocity;
    float health = 100;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        targetHeart = GameObject.Find("Heart").transform.position;
        velocity = targetHeart - transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(velocity * Time.deltaTime * 0.5f);
        if (health <= 0)
            OnDeath();
    }

    private void OnTriggerEnter(Collider other)
    {
            if (other.gameObject.name == "Heart")
            transform.position = startPos;
    }

    void OnDeath()
    {
        //increase currency in closest Pylon

        // do a sphere cast for towers in range and remove this enemy from them
        Collider[] towers = Physics.OverlapSphere(this.transform.position, 0.2f, LayerMask.NameToLayer("Range"));
        foreach(Collider tower in towers)
        {
            tower.gameObject.GetComponent<TurretController>().inRangeEnemies.Remove(this.gameObject); //The Remove method returns false if item is not found in the Hashset.
            
        }
    }
}
