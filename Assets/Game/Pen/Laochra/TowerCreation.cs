using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TowerCreation : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.Mouse0;

    private Vector3 mouseScreenPosition;
    private Vector3 mouseWorldPosition;

    [SerializeField] private GameObject targetPlane;
    [SerializeField] private GameObject towerPrefab;

    [SerializeField] LayerMask layersToCheck;
    [SerializeField] private float placementExclusionSize = 5;
    private const float capsuleCheckBound = 5;

    private void Update()
    {
        mouseScreenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.1f);
        mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        RaycastHit hit = GetRayHit();

        if (hit.collider is null)
        {
            Debug.DrawRay(Camera.main.transform.position, (mouseWorldPosition - Camera.main.transform.position) * 1000, Color.red);
            return;
        }
        Debug.DrawRay(Camera.main.transform.position, (mouseWorldPosition - Camera.main.transform.position).normalized * hit.distance, Color.green);

        bool targetIsPlane = CheckForPlane(hit);

        if (targetIsPlane && Input.GetKeyDown(interactKey))
            AttemptToSpawnTower(hit);
    }

    private RaycastHit GetRayHit()
    {
        Physics.Raycast(Camera.main.transform.position, mouseWorldPosition - Camera.main.transform.position, out RaycastHit hit, Mathf.Infinity);
        return hit;
    }

    private bool CheckForPlane(RaycastHit hit)
    {
        return hit.collider.gameObject == targetPlane;
    }

    private void AttemptToSpawnTower(RaycastHit hit)
    {
        Vector3 capsuleTop = new(hit.point.x, hit.point.y + capsuleCheckBound, hit.point.z);
        Vector3 capsuleBottom = new(hit.point.x, hit.point.y - capsuleCheckBound, hit.point.z);

        var targets = Physics.CapsuleCastAll(capsuleTop, capsuleBottom, placementExclusionSize, Vector3.up, Mathf.Infinity, layersToCheck).ToList();
        Debug.Log(targets.Count);
        if (targets.Count == 0)
            Instantiate(towerPrefab, hit.point, Quaternion.identity);
    }
}