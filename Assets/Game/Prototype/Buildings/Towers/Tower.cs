using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Tower : Building
{
    public TurretController TowerController; // this  seems like a god place to leave a reference to the individual tower functionality.
    // should strip the tower controller for parts for this later.




#if UNITY_EDITOR
    private void Update()
    {
       EditorApplication.Beep(); // Why Connor?
    }  
#endif
}