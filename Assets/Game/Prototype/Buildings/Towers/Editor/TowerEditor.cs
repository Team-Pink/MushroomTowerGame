using UnityEngine;

namespace Editor
{
    using UnityEditor;
    using static GUILayout;
    using GUI = UnityEditor.EditorGUILayout;

    [CustomEditor(typeof(Tower))]
    public class TowerEditor : Editor
    {
        Tower tower;

        float attackRadius;
        float turnRate;
        float attackCooldown;
        float damageDelay;

        bool upgradable;

        public enum TargeterType
        {
            Close,
            Cluster,
            Fast,
            Strong,
            Track
        } TargeterType targeterType = TargeterType.Close;
        public enum AttackerType
        {
            Area,
            Single,
            Trap
        } AttackerType attackerType = AttackerType.Area;

        public override void OnInspectorGUI()
        {
            tower = target as Tower;
            SetComponents();

            BeginHorizontal("box", ExpandWidth(true), Height(35));
                GUI.Space(); GUI.TextField("Tower Name", Stylesheet.TitleTextField, MaxWidth(120), Height(25)); GUI.Space();
            EndHorizontal();

            GUI.Space();

            BeginVertical("box", ExpandWidth(true));
                GUI.LabelField("Tower Values", Stylesheet.TitleLabel);
                BeginHorizontal();
                    GUI.Space(); GUI.LabelField("Attack Radius", Stylesheet.RightLabel, MaxWidth(100));
                    attackRadius = GUI.FloatField(tower.TargeterComponent.range, Stylesheet.CentreText, MaxWidth(35));
                    GUI.LabelField("Meters", MaxWidth(100)); GUI.Space();
                EndHorizontal();
                GUI.Space();
                BeginHorizontal();
                    GUI.Space(); GUI.LabelField("Turn Rate", Stylesheet.RightLabel, MaxWidth(100));
                    turnRate = GUI.FloatField(tower.TargeterComponent.turnRate, Stylesheet.CentreText, MaxWidth(35));
                    GUI.LabelField("° per Second", MaxWidth(100)); GUI.Space();
                EndHorizontal();
                GUI.Space();
                BeginHorizontal();
                    GUI.Space(); GUI.LabelField("Attack Cooldown", Stylesheet.RightLabel, MaxWidth(100));
                    attackCooldown = GUI.FloatField(tower.AttackerComponent.attackCooldown, Stylesheet.CentreText, MaxWidth(35));
                    GUI.LabelField("Seconds", MaxWidth(100)); GUI.Space();
                EndHorizontal();
                GUI.Space();
                BeginHorizontal();
                    GUI.Space(); GUI.LabelField("Damage Delay", Stylesheet.RightLabel, MaxWidth(100));
                    damageDelay = GUI.FloatField(tower.AttackerComponent.attackDelay, Stylesheet.CentreText, MaxWidth(35));
                    GUI.LabelField("Seconds", MaxWidth(100)); GUI.Space();
                EndHorizontal();
                GUI.Space();
            EndVertical();

            GUI.Space();

            BeginVertical("box", ExpandWidth(true));
                GUI.LabelField("Upgrades", Stylesheet.TitleLabel);
                BeginHorizontal();
                    GUI.Space(); GUI.LabelField("Upgradable", Stylesheet.RightLabel, MaxWidth(80));
                    upgradable = GUI.Toggle(upgradable, Width(20), Height(20)); GUI.Space();
                EndHorizontal();
                if (upgradable)
                {
                    GUI.Space();
                    BeginHorizontal();
                        GUI.Space(); GUI.LabelField("Path 1 Prefab", Stylesheet.RightLabel, MaxWidth(100));
                        GUI.TextField("", MaxWidth(80)); GUI.Space();
                    EndHorizontal();
                    GUI.Space();
                    BeginHorizontal();
                        GUI.Space(); GUI.LabelField("Path 2 Prefab", Stylesheet.RightLabel, MaxWidth(100));
                        GUI.TextField("", MaxWidth(80)); ; GUI.Space();
                    EndHorizontal();
                    GUI.Space();
                }
            EndVertical();

            GUI.Space();

            BeginVertical("box", ExpandWidth(true));
                BeginHorizontal();
                    GUI.Space(); GUI.LabelField("Targeting Type", Stylesheet.RightLabel, MaxWidth(100));
                    targeterType = (TargeterType)GUI.EnumPopup(targeterType, MaxWidth(80)); GUI.Space();
                EndHorizontal();
                GUI.Space();
                BeginHorizontal();
                    GUI.Space(); GUI.LabelField("Attacking Type", Stylesheet.RightLabel, MaxWidth(100));
                    attackerType = (AttackerType)GUI.EnumPopup(attackerType, MaxWidth(80)); ; GUI.Space();
                EndHorizontal();
            EndVertical();

            GUI.Space();

            BeginVertical("box");
                GUI.LabelField("Tags", Stylesheet.TitleLabel);
                //Tags go here
                BeginHorizontal();
                    BeginVertical("box", ExpandWidth(true));
                        BeginHorizontal(); GUI.Space(); GUI.LabelField("Targeting", Stylesheet.HeadingLabel, MaxWidth(80)); GUI.Space(); EndHorizontal();
                        for (int tagIndex = 0; tagIndex < 2; tagIndex++)
                        {
                            if (tagIndex % 2 == 0) BeginHorizontal();
                            Button("Target " + (tagIndex+1));
                            if (tagIndex % 2 == 1) EndHorizontal();
                        }
                    EndVertical();
                    BeginVertical("box", ExpandWidth(true));
                        BeginHorizontal(); GUI.Space(); GUI.LabelField("Attacking", Stylesheet.HeadingLabel, MaxWidth(80)); GUI.Space(); EndHorizontal();
                        for (int tagIndex = 0; tagIndex < 4; tagIndex++)
                        {
                            if (tagIndex % 2 == 0) BeginHorizontal();
                            Button("Attack " + (tagIndex + 1));
                            if (tagIndex % 2 == 1) EndHorizontal();
                        }
                    EndVertical();
                EndHorizontal();
                BeginHorizontal();
                    GUI.Space(); BeginVertical("box", MaxWidth(160));
                        BeginHorizontal();  GUI.Space(); GUI.LabelField("On-Kill", Stylesheet.HeadingLabel, MaxWidth(80)); GUI.Space(); EndHorizontal();
                        for (int tagIndex = 0; tagIndex < 1; tagIndex++)
                        {
                            BeginHorizontal();
                                GUI.Space();
                                Button("On-Kill " + (tagIndex + 1), MaxWidth(80));
                                GUI.Space();
                            EndHorizontal();
                        }

                    EndVertical(); GUI.Space();
                EndHorizontal();
                GUI.Space();
            EndVertical();

            SaveChanges();

            //base.OnInspectorGUI();
        }

        private void SaveChanges()
        {
            SetComponents();

            tower.TargeterComponent.range = attackRadius;
            tower.TargeterComponent.turnRate = turnRate;
            tower.AttackerComponent.attackCooldown = attackCooldown;
            tower.AttackerComponent.attackDelay = damageDelay;
        }

        private void SetComponents()
        {
            if (tower.TargeterComponent is null)
                switch (targeterType)
                {
                    case TargeterType.Close:
                        if (tower.TargeterComponent is not CloseTargeter)
                            tower.TargeterComponent = new CloseTargeter();
                        break;
                    case TargeterType.Cluster:
                        if (tower.TargeterComponent is not ClusterTargeter)
                            tower.TargeterComponent = new ClusterTargeter();
                        break;
                    case TargeterType.Fast:
                        if (tower.TargeterComponent is not FastTargeter)
                            tower.TargeterComponent = new FastTargeter();
                        break;
                    case TargeterType.Strong:
                        if (tower.TargeterComponent is not StrongTargeter)
                            tower.TargeterComponent = new StrongTargeter();
                        break;
                    case TargeterType.Track:
                        if (tower.TargeterComponent is not TrackTargeter)
                            tower.TargeterComponent = new TrackTargeter();
                        break;
                }
            if (tower.AttackerComponent is null)
                switch (attackerType)
                {
                    case AttackerType.Area:
                        if (tower.AttackerComponent is not AreaAttacker)
                            tower.AttackerComponent = new AreaAttacker();
                        break;
                    case AttackerType.Single:
                        if (tower.AttackerComponent is not SingleAttacker)
                            tower.AttackerComponent = new SingleAttacker();
                        break;
                    case AttackerType.Trap:
                        if (tower.AttackerComponent is not TrapAttacker)
                            tower.AttackerComponent = new TrapAttacker();
                        break;
                }
        }
    }
}