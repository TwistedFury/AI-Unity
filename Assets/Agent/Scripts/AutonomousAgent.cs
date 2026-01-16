using UnityEngine;

public class AutonomousAgent : AIAgent
{
    [SerializeField] Movement movement;
    [SerializeField] Perception seekPerception;
    [SerializeField] Perception fleePerception;

    [Header("Wander")]
    [SerializeField] float wanderRadius = 1;
    [SerializeField] float wanderDistance = 1;
    [SerializeField] float wanderDisplacement = 1;
    float wanderAngle = 0.0f;

    void Start()
    {
        wanderAngle = Random.Range(0, 360);
    }

    void Update()
    {
        bool hasTarget = false;
        if (seekPerception != null)
        {
            var seenObjects = seekPerception.GetGameObjects();
            if (seenObjects != null && seenObjects.Length > 0)
            {
                hasTarget = true;
                movement.ApplyForce(Seek(seenObjects[0]));
                foreach (var obj in seenObjects)
                {
                    Debug.DrawLine(transform.position, obj.transform.position, Color.darkKhaki);
                }
            }
        }
        if (fleePerception != null)
        {
            var fleeObjects = fleePerception.GetGameObjects();
            if (fleeObjects != null && fleeObjects.Length > 0)
            {
                hasTarget = true;
                movement.ApplyForce(Flee(fleeObjects[0]));
            }
        }
        if (!hasTarget)
        {
            movement.ApplyForce(Wander());
        }

        transform.position = Utilities.Wrap(transform.position, new(-15, -15, -15), new(15, 15, 15));
        if (movement.Velocity.sqrMagnitude > 0)
        {
            transform.rotation = Quaternion.LookRotation(movement.Velocity, transform.up);
        }
    }

    Vector3 Seek(GameObject target)
    {
        Vector3 direction = target.transform.position - transform.position;
        return GetSteeringForce(direction);
    }

    Vector3 Flee(GameObject target)
    {
        Vector3 direction = transform.position - target.transform.position;
        return GetSteeringForce(direction);
    }

    Vector3 Wander()
    {
        // randomly adjust the wander angle within (+/-) displacement range
        wanderAngle += Random.Range(-wanderDisplacement, wanderDisplacement);
        // calculate a point on the wander circle using the wander angle
        Quaternion rotation = Quaternion.AngleAxis(wanderAngle, Vector3.up);
        Vector3 pointOnCircle = rotation * (Vector3.forward * wanderRadius);
        // project the wander circle in front of the agent
        Vector3 circleCenter = movement.Direction * wanderDistance;
        // steer toward the target point (circle center + point on circle)
        Vector3 force = GetSteeringForce(circleCenter + pointOnCircle);

        Debug.DrawLine(transform.position, transform.position + circleCenter, Color.blue);
        Debug.DrawLine(transform.position, transform.position + circleCenter + pointOnCircle, Color.red);
        return force;
    }

    Vector3 GetSteeringForce(Vector3 direction)
    {
        Vector3 desired = movement.max_speed * direction.normalized;
        Vector3 steer = desired - movement.Velocity;
        return Vector3.ClampMagnitude(steer, movement.max_force);
    }
}