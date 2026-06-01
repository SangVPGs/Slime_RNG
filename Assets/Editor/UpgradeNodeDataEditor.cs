#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(UpgradeNodeData))]
public class UpgradeNodeDataEditor : Editor
{
    private SerializedProperty id;
    private SerializedProperty displayName;
    private SerializedProperty icon;
    private SerializedProperty parent;
    private SerializedProperty cost;

    private SerializedProperty effectType;
    private SerializedProperty targetId;

    private SerializedProperty statType;
    private SerializedProperty statModifierType;
    private SerializedProperty value;

    private void OnEnable()
    {
        id = serializedObject.FindProperty("id");
        displayName = serializedObject.FindProperty("displayName");
        icon = serializedObject.FindProperty("icon");
        parent = serializedObject.FindProperty("parent");
        cost = serializedObject.FindProperty("cost");

        effectType = serializedObject.FindProperty("effectType");
        targetId = serializedObject.FindProperty("targetId");

        statType = serializedObject.FindProperty("statType");
        statModifierType = serializedObject.FindProperty("statModifierType");
        value = serializedObject.FindProperty("value");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawIdentity();
        DrawTree();
        DrawCost();
        DrawEffect();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawIdentity()
    {
        EditorGUILayout.LabelField("Identity", EditorStyles.boldLabel);

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(id);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.PropertyField(displayName);
        EditorGUILayout.PropertyField(icon);

        EditorGUILayout.Space(8);
    }

    private void DrawTree()
    {
        EditorGUILayout.LabelField("Tree", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(parent);

        EditorGUILayout.Space(8);
    }

    private void DrawCost()
    {
        EditorGUILayout.LabelField("Cost", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(cost);

        EditorGUILayout.Space(8);
    }

    private void DrawEffect()
    {
        EditorGUILayout.LabelField("Effect", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(effectType);

        UpgradeEffectType selectedEffect =
            (UpgradeEffectType)effectType.enumValueIndex;

        switch (selectedEffect)
        {
            case UpgradeEffectType.UnlockItem:
                EditorGUILayout.PropertyField(targetId);
                break;

            case UpgradeEffectType.ChangeStat:
                EditorGUILayout.PropertyField(statType);
                EditorGUILayout.PropertyField(statModifierType);
                EditorGUILayout.PropertyField(value);
                break;
        }
    }
}
#endif