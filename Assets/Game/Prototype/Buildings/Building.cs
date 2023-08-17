using System.Collections;
using UnityEngine;

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
    //public MeshRenderer[] radiusDisplays;

    public virtual void Deactivate() => Active = false;
    public virtual void Reactivate() => Active = true;

    public virtual int GetTowerEXP() { return 0; }
  
    /*public IEnumerator FadeRadiusDisplay(float fromAlpha, float toAlpha)
    {
        float radiusFadeDuration = 0.5f;
        float durationElapsed = 0.0f;
        float durationProgress = 0;

        while (durationElapsed < radiusFadeDuration)
        {
            durationProgress = durationElapsed / radiusFadeDuration;

            foreach(MeshRenderer radius in radiusDisplays)
            {
                Color radiusColor = radius.material.color;

                float newAlpha = Mathf.Lerp(fromAlpha, toAlpha, durationProgress);

                radius.material.color = new Color(radiusColor.r, radiusColor.g, radiusColor.b, newAlpha);
            }

            yield return new WaitForSeconds(Time.deltaTime);
        }
    }*/

    public virtual void Sell()
    {
        Debug.Log("Sold",this);
    }
}