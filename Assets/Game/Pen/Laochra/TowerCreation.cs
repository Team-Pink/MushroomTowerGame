using GameObjectList = System.Collections.Generic.List<UnityEngine.GameObject>;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum InteractionState
{
    None,
    DraggingBud,
    PositionSelected
}

public class TowerCreation : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] GameObject targetPlane;
    [SerializeField, NonReorderable] private GameObjectList towerPrefabs = new(4);
    private const int towerPrefabAmount = 4;

    [Header("Placement")]
    [SerializeField] LayerMask layersToCheck;
    [SerializeField] float placementExclusionSize = 5;
    private const float capsuleCheckBound = 5;

    [Header("UI")]
    [SerializeField] GameObject radialMenu;
    [SerializeField] Image selectionIndicator;

    [Header("Testing")]
    [SerializeField] bool debugMode;
    [SerializeField] KeyCode interactKey = KeyCode.Mouse0;

    private Vector3 mouseScreenPosition;
    private Vector3 mouseWorldPosition;

    private InteractionState currentInteraction = InteractionState.None;
    private RaycastHit currentHit;

    private void Awake()
    {
        radialMenu.SetActive(false);
        selectionIndicator.enabled = false;
    }

    private void OnValidate()
    {
        if (towerPrefabs.Count == towerPrefabAmount)
            return;

        Debug.LogWarning("Stop that, the list tpwer prefabs should be exactly " + towerPrefabAmount + " elements!", this);

        while (towerPrefabs.Count < towerPrefabAmount)
        {
            towerPrefabs.Add(null);
        }
        while (towerPrefabs.Count > towerPrefabAmount)
        {
            towerPrefabs.RemoveAt(towerPrefabs.Count - 1);
        }
    }

    private void Update()
    {
        mouseScreenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.1f);
        mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        switch(currentInteraction)
        {
            case InteractionState.None:
                DefaultState();
                break;
            case InteractionState.DraggingBud:
                DraggingBudState();
                break;
            case InteractionState.PositionSelected:
                PositionSelectedState();
                break;
        }

        if(debugMode)
        {
            if (GetRayHit().collider is null)
                Debug.DrawRay(Camera.main.transform.position, (mouseWorldPosition - Camera.main.transform.position) * 1000, Color.red);
            else
                Debug.DrawRay(Camera.main.transform.position, (mouseWorldPosition - Camera.main.transform.position).normalized * GetRayHit().distance, Color.green);
            Debug.Log(currentInteraction);
        }
    }


    private void DefaultState()
    {
        if (Input.GetKeyDown(interactKey))
        {
            currentInteraction = InteractionState.DraggingBud;
        }
    }


    private void DraggingBudState()
    {
        if (Input.GetKeyUp(interactKey))
        {
            currentHit = GetRayHit();

            if (currentHit.collider is not null)
                currentInteraction = InteractionState.PositionSelected;
            else
                currentInteraction = InteractionState.None;
        }
    }

    private void PositionSelectedState()
    {
        if (!TargetIsPlane())
        {
            currentInteraction = InteractionState.None;
            return;
        }

        if (SpaceForTower())
        {
            if (selectionIndicator.enabled == false)
            {
                radialMenu.SetActive(true);

                selectionIndicator.rectTransform.position = mouseScreenPosition;
                selectionIndicator.enabled = true;
            }
        }
        else currentInteraction = InteractionState.None;
    }



    private RaycastHit GetRayHit()
    {
        Physics.Raycast(Camera.main.transform.position, mouseWorldPosition - Camera.main.transform.position, out RaycastHit hit, Mathf.Infinity);
        return hit;
    }

    private bool TargetIsPlane()
    {
        return currentHit.collider.gameObject == targetPlane;
    }

    private bool SpaceForTower()
    {
        Vector3 capsuleTop = new(currentHit.point.x, currentHit.point.y + capsuleCheckBound, currentHit.point.z);
        Vector3 capsuleBottom = new(currentHit.point.x, currentHit.point.y - capsuleCheckBound, currentHit.point.z);

        var targets = Physics.CapsuleCastAll(capsuleTop, capsuleBottom, placementExclusionSize, Vector3.up, Mathf.Infinity, layersToCheck).ToList();
        
        Debug.Log(targets.Count);
        return targets.Count == 0; 
    }

    public void SpawnTower(int towerIndex)
    {
        Instantiate(towerPrefabs[towerIndex], currentHit.point, Quaternion.identity);

        radialMenu.SetActive(false);
        selectionIndicator.enabled = false;

        currentInteraction = InteractionState.None;
    }
}