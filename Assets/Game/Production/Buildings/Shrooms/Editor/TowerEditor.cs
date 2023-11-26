namespace EditorScripts
{
    using static UnityEngine.GUILayout;
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using System;

    [Serializable]
    public class Tag
    {
        public string name;
        public bool enabled;
        public Tag requiredTag;
        public TargeterType requiredTargeterType;
        public AttackerType requiredAttackerType;

        public bool RequirementsMet(TowerEditor tower)
        {
            if (requiredTag != null && !requiredTag.enabled) return false;

            if (requiredTargeterType != TargeterType.SelectAType &&
                tower.targeterType != requiredTargeterType) return false;

            if (requiredAttackerType != AttackerType.SelectAType &&
                tower.attackerType != requiredAttackerType) return false;

            return true;
        }

        public Tag(string nameInit)
        {
            name = nameInit;
            enabled = false;
            requiredTag = null;
            requiredTargeterType = TargeterType.SelectAType;
            requiredAttackerType = AttackerType.SelectAType;
        }

        public void ToggleEnabled()
        {
            enabled = !enabled;
        }
    }

    [CustomEditor(typeof(Shroom))]
    public class TowerEditor : Editor
    {
        Shroom tower;

        string towerName;

        float attackRadius;
        float turnRate;

        int damage;
        float attackCooldown;
        float damageDelay;

        bool upgradable;

        public TargeterType targeterType = TargeterType.Close;
        public AttackerType attackerType = AttackerType.Attacker;

        Color selectedColor = new(1.75f, 1, 1.75f);

        readonly List<Tag> targettingTags = new()
        {
            new("Continuous"),
            new("Lock-on"),
            new("Multi-target")
        };
        readonly List<Tag> attackingTags = new()
        {
            new("Bounce"),
            new("Delay"),
            new("Knockback"),
            new("Non-repeating"),
            new("Spray"),
            new("Strikethrough")
        };
        readonly List<Tag> onKillTags = new()
        {
            new("Accelerate")
        };

        readonly List<Condition> conditions = new();
        public void AddCondition() => conditions.Add(new(Condition.ConditionType.None, 0, 0));
        public void RemoveCondition(int index) => conditions.RemoveAt(index);

        public void OnEnable()
        {
            tower = target as Shroom;

            {
                targettingTags[0].requiredTag = targettingTags[1]; // Continuous requires lock-on
                attackingTags[1].requiredAttackerType = AttackerType.Area; // Delay requires area
                attackingTags[3].requiredTag = attackingTags[0]; // Non-repeating requires bounce
            } // Set Tag Requirements

            {
                // Tag Value gets set here
                // Tag Value gets set here
                // Tag Value gets set here
                // Tag Value gets set here
                // Tag Value gets set here
                // Tag Value gets set here
                // Tag Value gets set here
            } // Set Tag Values - WIP !!!!!!!!!!!!!!!!!!!!!!!!!!!!

            {
                if (tower.TargeterComponent is CloseTargeter)
                    targeterType = TargeterType.Close;
                else if (tower.TargeterComponent is ClusterTargeter)
                    targeterType = TargeterType.Cluster;
                else if (tower.TargeterComponent is FastTargeter)
                    targeterType = TargeterType.Fast;
                else if (tower.TargeterComponent is StrongTargeter)
                    targeterType = TargeterType.Strong;
                else if (tower.TargeterComponent is TrackTargeter)
                    targeterType = TargeterType.Track;
                else
                    targeterType = TargeterType.Close;

                if (tower.AttackerComponent is AreaAttacker)
                    attackerType = AttackerType.Area;
                else if (tower.AttackerComponent is SingleAttacker)
                    attackerType = AttackerType.Single;
                else if (tower.AttackerComponent is TrapAttacker)
                    attackerType = AttackerType.Trap;
                else
                    attackerType = AttackerType.Attacker;
            } // Get Existing Components

            SetComponents();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            BeginHorizontal("box", ExpandWidth(true), Height(35));
                EditorGUILayout.Space(); towerName = EditorGUILayout.TextField(tower.details.name, Stylesheet.TitleTextField, MaxWidth(200), Height(25)); EditorGUILayout.Space();
            EndHorizontal();

            EditorGUILayout.Space();

            #region Tower Values
            BeginVertical("box", ExpandWidth(true));
                EditorGUILayout.LabelField("Tower Values", Stylesheet.TitleLabel); 
                BeginHorizontal();
                    EditorGUILayout.Space(); EditorGUILayout.LabelField("Attack Radius", Stylesheet.RightLabel, MaxWidth(100));
                    attackRadius = EditorGUILayout.FloatField(tower.TargeterComponent.range, Stylesheet.CentreText, MaxWidth(35));
                    EditorGUILayout.LabelField("Meters", MaxWidth(100)); EditorGUILayout.Space();
                EndHorizontal();
                EditorGUILayout.Space();
                BeginHorizontal();
                    EditorGUILayout.Space(); EditorGUILayout.LabelField("Turn Rate", Stylesheet.RightLabel, MaxWidth(100));
                    turnRate = EditorGUILayout.FloatField(tower.TargeterComponent.turnRate, Stylesheet.CentreText, MaxWidth(35));
                    EditorGUILayout.LabelField("° per Second", MaxWidth(100)); EditorGUILayout.Space();
                EndHorizontal();
                EditorGUILayout.Space(10);
                BeginHorizontal();
                    EditorGUILayout.Space(); EditorGUILayout.LabelField("Damage", Stylesheet.RightLabel, MaxWidth(100));
                    damage = EditorGUILayout.IntField(tower.AttackerComponent.damage, Stylesheet.CentreText, MaxWidth(35));
                    EditorGUILayout.LabelField("Hitpoints", MaxWidth(100)); EditorGUILayout.Space();
                EndHorizontal();
                EditorGUILayout.Space();
                BeginHorizontal();
                    EditorGUILayout.Space(); EditorGUILayout.LabelField("Attack Cooldown", Stylesheet.RightLabel, MaxWidth(100));
                    attackCooldown = EditorGUILayout.FloatField(tower.AttackerComponent.attackCooldown, Stylesheet.CentreText, MaxWidth(35));
                    EditorGUILayout.LabelField("Seconds", MaxWidth(100)); EditorGUILayout.Space();
                EndHorizontal();
                EditorGUILayout.Space();
                BeginHorizontal();
                    EditorGUILayout.Space(); EditorGUILayout.LabelField("Damage Delay", Stylesheet.RightLabel, MaxWidth(100));
                    damageDelay = EditorGUILayout.FloatField(tower.AttackerComponent.attackDelay, Stylesheet.CentreText, MaxWidth(35));
                    EditorGUILayout.LabelField("Seconds", MaxWidth(100)); EditorGUILayout.Space();
                EndHorizontal();
                EditorGUILayout.Space();
            EndVertical();
            #endregion

            EditorGUILayout.Space();

            #region Upgrades
            BeginVertical("box", ExpandWidth(true));
                EditorGUILayout.LabelField("Upgrades", Stylesheet.TitleLabel);
                BeginHorizontal();
                    EditorGUILayout.Space(); EditorGUILayout.LabelField("Upgradable", Stylesheet.RightLabel, MaxWidth(80));
                    upgradable = EditorGUILayout.Toggle(upgradable, Width(20), Height(20)); EditorGUILayout.Space();
                EndHorizontal();
                if (upgradable)
                {
                    EditorGUILayout.Space();
                    BeginHorizontal();
                        EditorGUILayout.Space(); EditorGUILayout.LabelField("Path 1 Prefab", Stylesheet.RightLabel, MaxWidth(100));
                        EditorGUILayout.TextField("", MaxWidth(80)); EditorGUILayout.Space();
                    EndHorizontal();
                    EditorGUILayout.Space();
                    BeginHorizontal();
                        EditorGUILayout.Space(); EditorGUILayout.LabelField("Path 2 Prefab", Stylesheet.RightLabel, MaxWidth(100));
                        EditorGUILayout.TextField("", MaxWidth(80)); ; EditorGUILayout.Space();
                    EndHorizontal();
                    EditorGUILayout.Space();
                }
            EndVertical();
            #endregion

            EditorGUILayout.Space();

            #region Targeting / Attacking Types
            BeginVertical("box", ExpandWidth(true));
                BeginHorizontal();
                    EditorGUILayout.Space(); EditorGUILayout.LabelField("Targeting Type", Stylesheet.RightLabel, MaxWidth(100));
                    targeterType = (TargeterType)EditorGUILayout.EnumPopup(targeterType, MaxWidth(80)); EditorGUILayout.Space();
                EndHorizontal();
                EditorGUILayout.Space();
                BeginHorizontal();
                    EditorGUILayout.Space(); EditorGUILayout.LabelField("Attacking Type", Stylesheet.RightLabel, MaxWidth(100));
                    attackerType = (AttackerType)EditorGUILayout.EnumPopup(attackerType, MaxWidth(80)); EditorGUILayout.Space();
                EndHorizontal();
            EndVertical();
            #endregion

            EditorGUILayout.Space();

            #region Tags
            BeginVertical("box");
                EditorGUILayout.LabelField("Tags", Stylesheet.TitleLabel);
                //Tags go here
                BeginHorizontal();
                    BeginVertical("box", ExpandWidth(true));
                        BeginHorizontal(); EditorGUILayout.Space(); EditorGUILayout.LabelField("Targeting", Stylesheet.HeadingLabel, MaxWidth(80)); EditorGUILayout.Space(); EndHorizontal();
                        for (int tagIndex = 0; tagIndex < targettingTags.Count; tagIndex++)
                        {
                            Tag currentTag = targettingTags[tagIndex];
                            if (tagIndex % 2 == 0) BeginHorizontal();
                            if (!currentTag.RequirementsMet(this))
                            {
                                currentTag.enabled = false;
                                GUI.backgroundColor = Color.grey;
                            }
                            if (currentTag.enabled) GUI.backgroundColor = selectedColor;
                            if (Button(currentTag.name)) currentTag.ToggleEnabled();
                            GUI.backgroundColor = Color.white;
                            if (tagIndex % 2 == 1 || tagIndex + 1 == targettingTags.Count) EndHorizontal();
                        }
                    EndVertical();
                    BeginVertical("box", ExpandWidth(true));
                        BeginHorizontal(); EditorGUILayout.Space(); EditorGUILayout.LabelField("Attacking", Stylesheet.HeadingLabel, MaxWidth(80)); EditorGUILayout.Space(); EndHorizontal();
                        for (int tagIndex = 0; tagIndex < attackingTags.Count; tagIndex++)
                        {
                            Tag currentTag = attackingTags[tagIndex];
                            if (tagIndex % 2 == 0) BeginHorizontal();
                            if (!currentTag.RequirementsMet(this))
                            {
                                currentTag.enabled = false;
                                GUI.backgroundColor = Color.grey;
                            }
                            if (currentTag.enabled) GUI.backgroundColor = selectedColor;
                            if (Button(currentTag.name)) currentTag.ToggleEnabled();
                            GUI.backgroundColor = Color.white;
                            if (tagIndex % 2 == 1 || tagIndex + 1 == attackingTags.Count) EndHorizontal();
                        }
                    EndVertical();
                EndHorizontal();
                BeginHorizontal();
                    EditorGUILayout.Space(); BeginVertical("box", MaxWidth(160));
                        BeginHorizontal();  EditorGUILayout.Space(); EditorGUILayout.LabelField("On-Kill", Stylesheet.HeadingLabel, MaxWidth(80)); EditorGUILayout.Space(); EndHorizontal();
                        for (int tagIndex = 0; tagIndex < onKillTags.Count; tagIndex++)
                        {
                            Tag currentTag = onKillTags[tagIndex];
                            if (tagIndex % 2 == 0) BeginHorizontal();
                            if (!currentTag.RequirementsMet(this))
                            {
                                currentTag.enabled = false;
                                GUI.backgroundColor = Color.grey;
                            }
                            if (currentTag.enabled) GUI.backgroundColor = selectedColor;
                            if (Button(currentTag.name)) currentTag.ToggleEnabled();
                            GUI.backgroundColor = Color.white;
                            if (tagIndex % 2 == 1 || tagIndex + 1 == onKillTags.Count) EndHorizontal();
                        }

                    EndVertical(); EditorGUILayout.Space();
                EndHorizontal();
                EditorGUILayout.Space();
            EndVertical();
            #endregion

            EditorGUILayout.Space();

            #region Conditions
            BeginVertical("box");
                EditorGUILayout.LabelField("Conditions", Stylesheet.TitleLabel);

                BeginHorizontal(); EditorGUILayout.Space();
                if (Button("Add", MaxWidth(80))) AddCondition();
                EditorGUILayout.Space(); EndHorizontal();

                EditorGUILayout.Space();

                for (int conditionIndex = 0; conditionIndex < conditions.Count; conditionIndex++)
                {     
                    BeginVertical("box");

                        EditorGUILayout.Space();

                        BeginHorizontal(); EditorGUILayout.Space();
                            EditorGUILayout.EnumPopup(conditions[conditionIndex].type, MaxWidth(120));
                            if (Button("Remove", MaxWidth(80))) RemoveCondition(conditionIndex);
                        EditorGUILayout.Space(); EndHorizontal();

                        EditorGUILayout.Space();

                        BeginHorizontal(); EditorGUILayout.Space();
                            EditorGUILayout.LabelField("Value", Stylesheet.RightLabel, MaxWidth(50));
                            EditorGUILayout.FloatField(0, MaxWidth(80));
                        EditorGUILayout.Space(); EndHorizontal();

                        EditorGUILayout.Space();

                        BeginHorizontal(); EditorGUILayout.Space();
                            EditorGUILayout.LabelField("Duration", Stylesheet.RightLabel, MaxWidth(50));
                            EditorGUILayout.FloatField(0, MaxWidth(80));
                        EditorGUILayout.Space(); EndHorizontal();

                        EditorGUILayout.Space();

                    EndVertical();
                }
            EndVertical();
            #endregion

            EditorGUILayout.Space();

            if (EditorGUI.EndChangeCheck())
                SaveChanges();
            //base.OnInspectorGUI(); 
        }

        private void SaveChanges() 
        {
            Undo.RecordObject(tower, "Modified Tower Values");

            SetComponents();

            tower.details = new(towerName, targeterType, attackerType);

            tower.TargeterComponent.range = attackRadius;
            tower.TargeterComponent.turnRate = turnRate;

            tower.AttackerComponent.damage = damage;
            tower.AttackerComponent.attackCooldown = attackCooldown;
            tower.AttackerComponent.attackDelay = damageDelay;

            EditorUtility.SetDirty(tower);
        }

        private void SetComponents(bool checkType = false)
        { 

            if (tower.TargeterComponent is null || tower.TargeterComponent.GetType() == typeof(Targeter) || !checkType)
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
            if (tower.AttackerComponent is null || tower.AttackerComponent.GetType() == typeof(Attacker) || !checkType)
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
                    case AttackerType.Attacker:
                        if (tower.AttackerComponent is TrapAttacker or AreaAttacker or SingleAttacker)
                            tower.AttackerComponent = new Attacker();
                        break;
            }
        }
    }
}