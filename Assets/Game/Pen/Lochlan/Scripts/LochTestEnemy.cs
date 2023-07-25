using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LochTestEnemy : MonoBehaviour
{
    Vector3 startPos;
    public Vector3 targetHeart;
    public LayerMask heartMask;
    public LayerMask range;
    Vector3 velocity;
    public float health = 100;
    bool dead; // indicates availability to object pool
    // Start is called before the first frame update
    public int priorityScoreModifier = 0;
    public float speed = 1;
    void Start()
    {
        startPos = transform.position;
        targetHeart = GameObject.Find("Heart").transform.position;
        velocity = (targetHeart - transform.position).normalized;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(velocity * Time.deltaTime * speed);
        if (health <= 0 && !dead) OnDeath();
    }

    private void OnTriggerEnter(Collider other)
    {
            if (other.gameObject.name == "Heart")
            transform.position = startPos;
    }

    public void OnDeath()
    {        
        
        // do a sphere cast for towers in range and remove this enemy from them
        Collider[] towers = Physics.OverlapSphere(this.transform.position, 0.2f, range);
        foreach(Collider tower in towers)
        {
            if(tower.gameObject.GetComponent<TurretController>().inRangeEnemies.Contains(this.gameObject))
            { 
            tower.gameObject.GetComponent<TurretController>().inRangeEnemies.Remove(this.gameObject); //The Remove method returns false if item is not found in the Hashset.
            }
        }  
        // play death animation

        // increase global currency
  
        //increase exp in closest Pylon


        //unrender enamy and reset values

        // flag the enemy data as available in the object pool
        dead = true;
        // get rid of this when alternative solution is implemented. // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        Debug.Log(this.gameObject.name + " is dead");
        velocity = velocity * 0;
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;
        // get rid of this when alternative solution is implemented. //

        
    }


}
