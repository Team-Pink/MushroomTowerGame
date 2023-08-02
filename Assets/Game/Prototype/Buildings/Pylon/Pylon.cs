using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using BuildingList = System.Collections.Generic.List<Building>;

public class Pylon : Building
{
    private BuildingList connectedBuildings = new();
    private List<Tower> connectedTowerList = new();
    private int buildingCount;
    private int EXPforLVLup = 2;
    public int pylonLevel= 0;

    private int pylonEXP;
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
            EXP -= EXPforLVLup;
            pylonLevel++;
            EXPforLVLup += 1;
        }
    }

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