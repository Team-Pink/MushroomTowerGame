using System.Linq;
using UnityEngine;

public enum InteractionState
{
    None,
    DraggingBud,
    PositionSelected
}

public class TowerCreation : MonoBehaviour
{
    [SerializeField] private bool debugMode;
    [SerializeField] private KeyCode interactKey = KeyCode.Mouse0;

    private Vector3 mouseScreenPosition;
    private Vector3 mouseWorldPosition;

    [SerializeField] private GameObject targetPlane;
    [SerializeField] private GameObject towerPrefab;

    [SerializeField] LayerMask layersToCheck;
    [SerializeField] private float placementExclusionSize = 5;
    private const float capsuleCheckBound = 5;

    private InteractionState currentInteraction = InteractionState.None;
    private RaycastHit currentHit;

    private void Update()
    {
        mouseScreenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.1f);
        mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        switch(currentInteraction)
        {
            case InteractionState.None:
                DraggingBudState();
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
            SpawnTower();

        currentInteraction = InteractionState.None;
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

    private void SpawnTower()
    {
        Instantiate(towerPrefab, currentHit.point, Quaternion.identity);
    }
}