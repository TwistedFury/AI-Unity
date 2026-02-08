using System.Linq;
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

    [Header("Flock")]
    [SerializeField] Perception flockPerception;
    [SerializeField, Range(0, 5)] float cohesionWeight = 1;
    [SerializeField, Range(0, 5)] float separationWeight = 1;
    [SerializeField, Range(0, 5)] float alignmentWeight = 1;
    [SerializeField, Range(0, 5)] float separationRadius = 1;

    [Header("Obstacle")]
    [SerializeField] Perception obstaclePerception;
    [SerializeField, Range(0, 5)] float obstacleWeight = 1; 

    void Start()
    {
        wanderAngle = Random.Range(0, 360);
    }

    void Update()
    {
        // Early Out Obstacle Perception
        Vector3 facing = (movement.Velocity.sqrMagnitude > 0.0001f)
            ? movement.Velocity.normalized
            : transform.forward;
        if (obstaclePerception != null &&
            obstaclePerception.GetGameObjectInDirection(facing) != null)
        {
            Vector3 openDirection = Vector3.zero;
            if (obstaclePerception.GetOpenDirection(ref openDirection) && openDirection.sqrMagnitude > 0.0001f)
            {
                movement.ApplyForce(GetSteeringForce(openDirection) * obstacleWeight);
                if (movement.Velocity.sqrMagnitude > 0)
                {
                    transform.rotation = Quaternion.LookRotation(movement.Velocity, Vector3.up);
                }
                return;
            }
        }

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
        if (flockPerception != null)
        {
            var gameObjects = flockPerception.GetGameObjects();
            if (gameObjects.Length > 0)
            {
                hasTarget = true;
                movement.ApplyForce(Cohesion(gameObjects) * cohesionWeight);
                movement.ApplyForce(Separation(gameObjects, separationRadius) * separationWeight);
                movement.ApplyForce(Alignment(gameObjects) * alignmentWeight);
            }
        }
        if (!hasTarget)
        {
            movement.ApplyForce(Wander());
        }

        if (movement.Velocity.sqrMagnitude > 0)
        {
            transform.rotation = Quaternion.LookRotation(movement.Velocity, Vector3.up);
        }
    }

    private void LateUpdate()
    {
        transform.position = Utilities.WrapXZ(transform.position, new(-10, 0.5f, -10), new(10, 1.5f, 10));
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

    Vector3 Cohesion(GameObject[] neighbors)
    {
        if (neighbors == null || neighbors.Length == 0) return Vector3.zero;

        Vector3 positions = Vector3.zero;
        // accumulate the position vectors of the neighbors
        foreach (var obj in neighbors)
	    {
            // add neighbor position to positions
            positions += obj.transform.position;
        }

        // average the positions to get the center of the neighbors
        Vector3 center = positions / neighbors.Count();
        // create direction vector to point towards the center of the neighbors from agent position
        Vector3 direction = center - transform.position;

        // steer towards the center point
        Vector3 force = GetSteeringForce(direction); // ✅ direction is a VECTOR

        return force;
    }

    Vector3 Separation(GameObject[] neighbors, float radius)
    {
        if (neighbors == null || neighbors.Length == 0) return Vector3.zero;

        Vector3 separation = Vector3.zero;
        // accumulate the separation vectors of the neighbors
        foreach (var neighbor in neighbors)
	    {
            // get direction vector away from neighbor
            Vector3 direction = transform.position - neighbor.transform.position;
            float distance = direction.magnitude;
            // check if within separation radius
            if (distance > 0 && distance < radius)
		    {
                // scale separation vector inversely proportional to the direction distance
                // closer the distance the stronger the separation
                separation += direction * (1 / distance);
            }
        }

        // steer towards the separation point
        Vector3 force = (separation.magnitude > 0) ? GetSteeringForce(separation) : Vector3.zero;

        return force;
    }

    private Vector3 Alignment(GameObject[] neighbors)
    {
        if (neighbors == null || neighbors.Length == 0) return Vector3.zero;

        Vector3 velocities = Vector3.zero;
        int neighborsWithAgent = 0;
        // accumulate the velocity vectors of the neighbors
        foreach (var neighbor in neighbors)
	    {
            // get the velocity from the agent movement
            if (neighbor.TryGetComponent<AutonomousAgent>(out AutonomousAgent agent))
		    {
                // add agent movement velocity to velocities
                velocities += agent.movement.Velocity;
                neighborsWithAgent++;
            }
        }
        if (neighborsWithAgent == 0) return Vector3.zero;
        // get the average velocity of the neighbors
        Vector3 averageVelocity = velocities / neighborsWithAgent;

        // steer towards the average velocity
        Vector3 force = GetSteeringForce(averageVelocity);

        return force;
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