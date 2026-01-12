using UnityEngine;

public abstract class Perception : MonoBehaviour
{
    public string tagName;
    public float maxDistance;
    public float fov;

    public abstract GameObject[] GetGameObjects();
}
