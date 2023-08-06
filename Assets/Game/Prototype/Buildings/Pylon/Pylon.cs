using UnityEngine;
using BuildingList = System.Collections.Generic.List<Building>;

public class Pylon : Building
{
    //[HideInInspector]
    [SerializeField] int instance = 0;
    public static int cost = 10;

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

    public int GetPylonCost()
    {
        return cost * (instance + 1);
    }
    public int GetPylonCost(int instance)
    {
        return cost * (instance + 1);
    }
    public int GetInstanceNumber()
    {
        return instance;
    }
    public void SetInstanceNumber(int number)
    {
        instance = number;
    }
    public static int GetPylonBaseCurrency()
    {
        return cost;
    }

}