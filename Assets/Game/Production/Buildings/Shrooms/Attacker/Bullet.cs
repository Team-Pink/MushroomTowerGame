using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public bool spin;
    public float spinSpeed;

    private bool initialised;

    public Attacker attacker;

    public float timeToTarget;
    private float timeElapsed;
    private Vector3 startPos;
    public Target target;
    private Transform targetTransform;
    private Vector3 targetPos;

    // parobola variables
    public bool parabola;
    private float progress = 0;
    [SerializeField] private float arcHeight = 40;
    Vector3 currentPos;

    private bool attackFinished = false;



    /// <summary>
    /// Do not use this on moving targets this does not track.
    /// </summary>
    public void InitializeNoTrackParabolaBullet(Vector3 pos)
    {

        targetPos = pos;
        parabola = true;
        CommonVariablesToInitialize();
    }

    /// <summary>
    /// This does not track moving targets but a singular position.
    /// </summary>
    public void InitialiseForTargetPosition(Vector3 tPosition)
    {
        startPos = transform.position;
        targetPos = tPosition;
        CommonVariablesToInitialize();

    }

    public void Initialise()
    {
        if (!target.enemy) Destroy(gameObject);
        targetTransform = target.enemy.transform; // this will fail if it is called on an enemy that no longer exists.
        targetPos = targetTransform.position;
        CommonVariablesToInitialize();
    }

    public void CommonVariablesToInitialize()
    {
        startPos = transform.position;
        initialised = true;        
        //StartCoroutine(SelfDestruct());
    }

    void Update()
    {
        if (attackFinished) return;

        if (spin)
        {
            transform.Rotate(new Vector3(0, -spinSpeed * Time.deltaTime, 0));
        }

        if (!initialised) return;

        if (attacker == null) Destroy(gameObject);

        if (timeElapsed >= timeToTarget)
        {
            attackFinished = true;
            attacker.AttackHit();

            //Debug.Log("Bullet hit, " + timeToTarget);

            if (spin)
            {
                Destroy(gameObject);
                return;
            }
            if (!TryGetComponent(out MeshRenderer renderer))
            {
                renderer = GetComponentInChildren<MeshRenderer>();
            }
            if (renderer != null) renderer.enabled = false;
            
            Destroy(gameObject, 3);
        }
        else
        {
            timeElapsed += Time.deltaTime;

            if (parabola) MoveParabola();
            else MoveStraightToTarget();
        }
    }

    void MoveStraightToTarget()
    {
        if (target.enemy || targetTransform != null)
            targetPos = targetTransform.position; // update target position for moving targets if the targettransform is an Object not null.

        transform.position = Vector3.Lerp(startPos, targetPos, Mathf.Min(timeElapsed / timeToTarget, 1));
    }

    void MoveParabola()
    {
        //LooseTargetTracking();
        Debug.DrawLine(transform.position, targetPos, Color.red, 0.02f);
        // update progress to match time elapsed
        progress = timeElapsed / timeToTarget;

        Vector3 newPos = Vector3.Lerp(startPos, targetPos, progress); // update xz position
        newPos.y = -progress * progress + progress; // update y position
        newPos.y *= arcHeight;

        transform.rotation = Quaternion.LookRotation((newPos - currentPos).normalized);

        currentPos = newPos;
        transform.position = newPos;
    }


    //void LooseTargetTracking()
    //{
    //    targetPos = Vector3.MoveTowards( targetPos, targetTransform.position, trackingSpeed);

    //}
}



// Personally as the creator of this script I must say, using a physical bullet is the worst possible way we could have done this. While it has
// yet to cause any game breaking errors yet the ones it does cause have refused to be put to rest despite many efforts to do so.
// it doesnn't help that this bullet script was made all the way back in pre production in place of a proper firing animation as a demonstration
// to show how we wanted our game to work as well as to test out putting an animation trigger into a turret. Unfortunately it's too late now to change.