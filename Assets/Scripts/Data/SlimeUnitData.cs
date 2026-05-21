using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Unit/Slime")]
public class SlimeUnitData : UnitData
{
    [Header("Slime Info")]
    public int goldDrop;


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = GenerateSlimeId();
            EditorUtility.SetDirty(this);
        }
    }

    private string GenerateSlimeId()
    {
        string[] guids = AssetDatabase.FindAssets("t:SlimeUnitData");

        int count = guids.Length;

        return $"S{count:000}";
    }

#endif
}