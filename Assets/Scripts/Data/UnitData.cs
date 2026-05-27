using UnityEngine;

public abstract class UnitData : ScriptableObject
{
    [SerializeField] protected string id;
    public string Id => id;

    public string unitName;
    public GameObject model;

    [Header("Base Stats")]
    public int baseHp = 100;
    public int baseAtk = 10;

    public float baseAtkRange = 1.5f;
    public float baseAtkSpeed = 1f;
    public float baseSpeed = 3.5f;

    [Header("Level")]
    public int defaultLevel = 1;
    public int maxLevel = 999;
}