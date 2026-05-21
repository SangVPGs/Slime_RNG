using UnityEngine;

public abstract class UnitData : ScriptableObject
{
    [SerializeField] protected string id;

    public string Id => id;

    public string unitName;

    public GameObject model;

    public int hp = 100;
    public int atk = 10;

    public float atkRange = 1.5f;
    public float atkSpeed = 1f;
    public float speed = 3.5f;
}