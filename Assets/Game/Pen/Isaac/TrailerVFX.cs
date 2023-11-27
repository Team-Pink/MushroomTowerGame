using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class TrailerVFX : MonoBehaviour
{
    public VisualEffect detonateVFX;
    public VisualEffect launchVFX;

    float count = 0;
    [SerializeField] float timer = 2.5f;
    
    
    // Start is called before the first frame update
    void Start()
    {
        if (detonateVFX == null || launchVFX == null) return;

        detonateVFX.Play();
        launchVFX.Play();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (count >= timer)
        {
            detonateVFX.Stop();
            launchVFX.Stop();
        }
        else count += Time.deltaTime;
    }
}
