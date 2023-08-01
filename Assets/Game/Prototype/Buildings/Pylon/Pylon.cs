using UnityEngine;
using BuildingList = System.Collections.Generic.List<Building>;

public class Pylon : Building
{
    private BuildingList connectedBuildings = new();

    public void AddBuilding(Building building) => connectedBuildings.Add(building);

    [ContextMenu("Deactivate")]
    public override void Deactivate()
    {
        base.Deactivate();

        foreach (Building building in connectedBuildings)
        {
            building.Deactivate();
        }

        Debug.Log("Deactivated");
    }
    public override void Reactivate()
    {
        base.Reactivate();

        foreach (Building building in connectedBuildings)
        {
            building.Reactivate();
        }
    }
}