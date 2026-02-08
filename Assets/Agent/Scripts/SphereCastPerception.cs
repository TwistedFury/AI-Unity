using System.Collections.Generic;
using UnityEngine;

public class SphereCastPerception : Perception
{
    [SerializeField, Tooltip("The number of rays casted."), Range(1, 20)] int numRays = 1;
    [SerializeField, Tooltip("The radius of the sphere casted."), Range(0, 10)] float sphereRadius = 1;
    public override GameObject[] GetGameObjects()
    {
        // create result list
        List<GameObject> result = new List<GameObject>();
        // get array of directions in circle
        Vector3[] directions = Utilities.GetDirectionsInCircle(numRays, maxHalfAngle);
        // iterate through directions
        foreach (var direction in directions)
        {
            // get game object in direction (in object space)
            GameObject go = GetGameObjectInDirection(transform.rotation * direction);
            if (go != null)
            {
                // add game object to results
                result.Add(go);
            }
        }
        // convert list to array
        return result.ToArray();
    }
    public override GameObject GetGameObjectInDirection(Vector3 direction)
    {
        // create ray from transform postion in the direction
        Ray ray = new Ray(transform.position, direction);
        if (Physics.SphereCast(ray, sphereRadius, out RaycastHit raycastHit, maxDistance, layerMask))
        {
            // do not include ourselves
            if (raycastHit.collider.gameObject == gameObject) return null;
            // check for matching tag
            if (tagName == "" || raycastHit.collider.CompareTag(tagName))
            {
                // add game object to results
                if (debug) Debug.DrawRay(ray.origin, ray.direction *
                raycastHit.distance, Color.red);
                return raycastHit.collider.gameObject;
            }
        }
        else
        {
            if (debug) Debug.DrawRay(ray.origin, ray.direction * maxDistance,
            Color.green);
        }
        return null;
    }
    public override bool GetOpenDirection(ref Vector3 openDirection)
    {
        // get array of directions in circle
        Vector3[] directions = Utilities.GetDirectionsInCircle(numRays, maxHalfAngle);
        // iterate through directions
        foreach (var direction in directions)
        {
            // get game object in direction (in object space), if game object is returned then space is not open
            GameObject go = GetGameObjectInDirection(transform.rotation * direction);
            if (go == null)
            {
                // no game object in this direction, set open direction and return true
                openDirection = transform.rotation * direction;
                return true;
            }
        }
        // no open spaces
        return false;
    }
    private void OnDrawGizmos()
    {
        if (!debug) return; // don't draw gizmos if not debugging
                            // get directions in circle starting from forward
        Vector3[] directions = Utilities.GetDirectionsInCircle(numRays, maxHalfAngle);
        // draw rays using position and directions (transformed into object space using transform.rotation)
        // draw sphere at the end of the rays
        foreach (var direction in directions)
        {
            Gizmos.color = debugColor;
            Gizmos.DrawRay(transform.position, transform.rotation * direction *
            maxDistance);
            Gizmos.DrawWireSphere(transform.position + transform.rotation * direction *
            maxDistance, sphereRadius);
        }
    }
}
