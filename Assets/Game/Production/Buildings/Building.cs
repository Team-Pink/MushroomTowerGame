using System.Collections;
using UnityEngine;

public abstract class Building : MonoBehaviour
{
    protected enum LineMode
    {
        Default,
        Highlighted,
        Sell
    }

    private bool active = true;
    public bool Active
    {
        get => active;

        private set => active = value;
    }

    public GameObject radiusDisplay;
    public Material[] radiusMaterials;
    public GameObject bud;

    [HideInInspector] public bool recurseHighlight;
    [HideInInspector] public bool showSelling;

    public virtual void Deactivate() => Active = false;
    public virtual void Reactivate() => Active = true;

    public virtual void Sell()
    {
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



    public virtual void AddLine(Building target) { Debug.Log("AddLine() is not defined for this class", this); }
    public virtual void RemoveLine(Building target) { Debug.Log("RemoveLine() is not defined for this class", this); }

    public virtual void SetLineDefault(Building target) { Debug.Log("SetLineDefault() is not defined for this class", this); }
    public virtual void SetLinesDefault() { Debug.Log("SetLinesDefault() is not defined for this class", this); }

    public virtual void SetLineHighlighted(Building target) { Debug.Log("SetLineHighlighted() is not defined for this class", this); }
    public virtual void SetLinesHighlighted() { Debug.Log("SetLinesHighlighted() is not defined for this class", this); }

    public virtual void SetLineSell(Building target) { Debug.Log("SetLineSell() is not defined for this class", this); }
    public virtual void SetLinesSell() { Debug.Log("SetLinesSell() is not defined for this class", this); }
    
}