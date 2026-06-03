#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    private SerializedProperty id;
    private SerializedProperty itemName;
    private SerializedProperty description;

    private SerializedProperty icon;
    private SerializedProperty prefab;

    private SerializedProperty stackable;
    private SerializedProperty maxStack;

    private SerializedProperty itemType;
    private SerializedProperty statType;
    private SerializedProperty modifierType;
    private SerializedProperty value;
    private SerializedProperty duration;

    private void OnEnable()
    {
        id = serializedObject.FindProperty("id");
        itemName = serializedObject.FindProperty("itemName");
        description = serializedObject.FindProperty("description");

        icon = serializedObject.FindProperty("icon");
        prefab = serializedObject.FindProperty("prefab");

        stackable = serializedObject.FindProperty("stackable");
        maxStack = serializedObject.FindProperty("maxStack");

        itemType = serializedObject.FindProperty("itemType");
        statType = serializedObject.FindProperty("statType");
        modifierType = serializedObject.FindProperty("modifierType");
        value = serializedObject.FindProperty("value");
        duration = serializedObject.FindProperty("duration");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawIdentity();
        DrawVisual();
        DrawStack();
        DrawEffect();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawIdentity()
    {
        EditorGUILayout.LabelField("Identity", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(id);
        EditorGUILayout.PropertyField(itemName);
        EditorGUILayout.PropertyField(description);

        EditorGUILayout.Space();
    }

    private void DrawVisual()
    {
        EditorGUILayout.LabelField("Visual", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(icon);
        EditorGUILayout.PropertyField(prefab);

        EditorGUILayout.Space();
    }

    private void DrawStack()
    {
        EditorGUILayout.LabelField("Stack", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(stackable);

        if (stackable.boolValue)
        {
            EditorGUILayout.PropertyField(maxStack);

            if (maxStack.intValue < 1)
                maxStack.intValue = 1;
        }

        EditorGUILayout.Space();
    }

    private void DrawEffect()
    {
        EditorGUILayout.LabelField("Effect", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(itemType);

        ItemType type = (ItemType)itemType.enumValueIndex;

        switch (type)
        {
            case ItemType.Food:
                DrawFoodEffect();
                break;

            case ItemType.BuffStat:
                DrawBuffStatEffect();
                break;
        }
    }

    private void DrawFoodEffect()
    {
        EditorGUILayout.PropertyField(
            value,
            new GUIContent("Exp Value")
        );

        if (value.floatValue < 0f)
            value.floatValue = 0f;
    }

    private void DrawBuffStatEffect()
    {
        EditorGUILayout.PropertyField(statType);
        EditorGUILayout.PropertyField(modifierType);

        EditorGUILayout.PropertyField(
            value,
            new GUIContent("Stat Value")
        );

        EditorGUILayout.PropertyField(duration);

        if (duration.floatValue < 0f)
            duration.floatValue = 0f;
    }
}
#endif