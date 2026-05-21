using UnityEngine;

public class MapAreaTrigger : MonoBehaviour
{
    [SerializeField] private MapChunk mapChunk;

    private void Awake()
    {
        if (mapChunk == null)
            mapChunk = GetComponentInParent<MapChunk>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (mapChunk == null)
            return;

        MapProgressSave.SaveCurrentMapLevel(mapChunk.Level);
    }
}