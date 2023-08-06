using BuildingList = System.Collections.Generic.List<Building>;
using ContextMenu = UnityEngine.ContextMenu;

using Debug = UnityEngine.Debug;

public class Pylon : Building
{
    public bool Enhanced { get; private set; }
    private readonly BuildingList connectedBuildings = new();
    public bool IsBuildingInList(Building building)
    {
        return connectedBuildings.Contains(building);
    }

    private void Update()
    {
        for (int buildingIndex = 0; buildingIndex < connectedBuildings.Count; buildingIndex++)
        {
            Building building = connectedBuildings[buildingIndex];
            if (building is Tower)
            {
                if ((building as Tower).sellFlag)
                    SellTower(building as Tower);
            }
        }
    }


    public void Enhance() => Enhanced = true;
    public void AddBuilding(Building building) => connectedBuildings.Add(building);

    [ContextMenu("Deactivate")] public override void Deactivate()
    {
        DeactivateConnectedBuildings();

        base.Deactivate();
    }
    public void DeactivateConnectedBuildings()
    {
        foreach (Building building in connectedBuildings)
        {
            building.Deactivate();
        }
    }
    public override void Reactivate()
    {
        base.Reactivate();

        foreach (Building building in connectedBuildings)
        {
            building.Reactivate();
        }
    }

    public override void Sell()
    {
        DeactivateConnectedBuildings();

        base.Sell();
    }
    public void SellTower(Tower tower)
    {
        Debug.Log("Sold Tower", tower);
        connectedBuildings.Remove(tower);
        Destroy(tower.gameObject);
    }
    public void SellAll()
    {
        BuildingList sellList = new();

        for (int buildingIndex = 0; buildingIndex < connectedBuildings.Count; buildingIndex++)
        {
            Building building = connectedBuildings[buildingIndex];
            if (building is Pylon)
                (building as Pylon).SellAll();
            else
            {
                sellList.Add(building);
            }
        }

        for (int buildingIndex = 0; buildingIndex < sellList.Count; buildingIndex++)
        {
            SellTower(sellList[buildingIndex] as Tower);
        }

        base.Sell();
    }
}