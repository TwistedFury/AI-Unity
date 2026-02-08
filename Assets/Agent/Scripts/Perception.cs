using UnityEngine;

public abstract class Perception : MonoBehaviour
{
    [SerializeField] string info;

    [SerializeField] protected string tagName;
    [SerializeField] protected LayerMask layerMask = Physics.AllLayers;
    [SerializeField, Range(0, 10)] protected float maxDistance = 5;
    [SerializeField, Range(0, 180)] protected float maxHalfAngle = 180;

    [Header("Debug")]
    [SerializeField] protected bool debug = false;
    [SerializeField] protected Color debugColor = Color.white;

    public abstract GameObject[] GetGameObjects();

    public virtual GameObject GetGameObjectInDirection(Vector3 dir) { return null; }
    public virtual bool GetOpenDirection(ref Vector3 dir) { return false; }
}
