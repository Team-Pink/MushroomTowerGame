using MonoBehaviour = UnityEngine.MonoBehaviour;
using GameObject = UnityEngine.GameObject;
using Debug = UnityEngine.Debug;

public abstract class Building : MonoBehaviour
{
    private bool active = true;
    public bool Active
    {
        get => active;

        private set
        {
            active = value;

            if (value)
                Debug.Log("Reactivated", this);
            else
                Debug.Log("Deactivated", this);
        }
    }

    public GameObject radiusDisplay;

    public virtual void Deactivate() => Active = false;
    public virtual void Reactivate() => Active = true;

    public virtual int GetTowerEXP() { return 0; }
  

    public virtual void Sell()
    {
        Debug.Log("Sold",this);
    }

    public virtual void SellAll()
    {
        Debug.Log("Sold", this);
        Destroy(gameObject);
    }
}