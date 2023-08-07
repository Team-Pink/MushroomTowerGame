<<<<<<< HEAD
using System.Collections.Generic;
using UnityEngine;
<<<<<<< HEAD
using static UnityEngine.Rendering.DebugUI;
=======
=======
>>>>>>> bd1fdbe35e0e26cdee5d69b7a12991881e13b72c
>>>>>>> Prototype
using BuildingList = System.Collections.Generic.List<Building>;
using ContextMenu = UnityEngine.ContextMenu;

using Debug = UnityEngine.Debug;

public class Pylon : Building
{
<<<<<<< HEAD
    private BuildingList connectedBuildings = new();
    private List<Tower> connectedTowerList = new();
    private int buildingCount;
    private int EXPforLVLup = 2;
    [SerializeField] private bool enhanced;
    public bool Enhanced { get { return enhanced; }}

<<<<<<< HEAD
    [SerializeField] private int pylonEXP;
    public int EXP
    {
        get
        {
            return pylonEXP;
        }
        set
        {
            pylonEXP = value;
            CheckIfEnhanced();
        }
    }





    private void CheckIfEnhanced()
    {
        if (pylonEXP >= EXPforLVLup)
        {
            enhanced = true;
        }
    }
=======
    public int PylonEXP;
=======
    public bool Enhanced { get; private set; }
    private readonly BuildingList connectedBuildings = new();
    public bool IsBuildingInList(Building building)
    {
        return connectedBuildings.Contains(building);
    }
>>>>>>> bd1fdbe35e0e26cdee5d69b7a12991881e13b72c
>>>>>>> Prototype

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

<<<<<<< HEAD
    private void Update()
    {
        // Move this to on wave end in the wave manager when it exists.
        GetTowerEXP();
    }

    /// <summary>
    /// generate a list of Tower from the building list 
    /// </summary>
    private void GenerateTowerList()
    {
        buildingCount = connectedBuildings.Count;// set buildingcount.

        connectedTowerList.Clear(); // clobber the existing list
        // generate a list of towers
        foreach (Building building in connectedBuildings)
        {
            if (building.gameObject.GetComponent<Tower>())
            {
                connectedTowerList.Add(building.gameObject.GetComponent<Tower>());
            }
        }
    }
    public void GetTowerEXP()
    {
        if (buildingCount < connectedBuildings.Count)
            GenerateTowerList();
        foreach (Tower tower in connectedTowerList)
        {
            if (tower.TowerController.storedExperience > 0)
            { 
            EXP += tower.TowerController.storedExperience;
            tower.TowerController.storedExperience = 0;
            }
        }
    }
}

// sorry if my lack of knowledge regarding Context menus, regions and serialize field throw you off.
// I'll make sure to comment on what is my code. _Lochlan_
=======
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
>>>>>>> bd1fdbe35e0e26cdee5d69b7a12991881e13b72c
