using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WireFrameObject : MonoBehaviour
{
    public float size = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, size);
    }
}
