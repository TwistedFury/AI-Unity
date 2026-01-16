using UnityEngine;

public abstract class Movement : MonoBehaviour
{
    public float max_speed = 1.0f;
    public float max_force = 1.0f;

    public virtual Vector3 Velocity { get; set; }
    public virtual Vector3 Acceleration { get; set; }
    public virtual Vector3 Direction { get { return transform.forward.normalized; } }

    public abstract void ApplyForce(Vector3 force);
}
