using UnityEngine;

public abstract class Building : MonoBehaviour
{
    public bool Active{ get; private set; } = true;

    public GameObject radiusDisplay;

    public virtual void Deactivate() => Active = false;
    public virtual void Reactivate() => Active = true;
}