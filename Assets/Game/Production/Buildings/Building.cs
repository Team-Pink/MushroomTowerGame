using System.Collections;
using UnityEngine;

public abstract class Building : MonoBehaviour
{
    private bool active = true;
    public bool Active
    {
        get => active;

        private set => active = value;
    }

    public GameObject radiusDisplay;
    public Material[] radiusMaterials;
    public GameObject bud;

    public virtual void Deactivate() => Active = false;
    public virtual void Reactivate() => Active = true;

    public virtual int GetTowerEXP() { return 0; }

    public virtual void Sell()
    {
        //Debug.Log("Sold", this);
    }

    public IEnumerator ExpandRadiusDisplay()
    {
        float radiusFadeDuration = 0.05f;
        float durationElapsed = 0.0f;

        while (durationElapsed < radiusFadeDuration)
        {
            float durationProgress = durationElapsed / radiusFadeDuration;

            foreach (Material radiusMaterial in radiusMaterials)
            {
                radiusMaterial.SetFloat("_Display_Amount", durationProgress);
            }

            durationElapsed += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        foreach (Material radiusMaterial in radiusMaterials)
        {
            radiusMaterial.SetFloat("_Display_Amount", 1);
        }
    }
}