using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RaycastPerception : Perception
{

    [SerializeField, Tooltip("The number of rays casted.")] int numRays = 1;

    public override GameObject[] GetGameObjects()
    {
        // create result list
        List<GameObject> result = new List<GameObject>();

        // get array of directions in circle
        Vector3[] directions = Utilities.GetDirectionsInCircle(numRays, maxHalfAngle);

        // iterate through directions
        foreach (var direction in directions)
        {
            GameObject go = GetGameObjectInDirection(transform.rotation * direction);
            if (go != null) { result.Add(go); }
        }

        // convert list to array
        return result.ToArray();
    }

    public override GameObject GetGameObjectInDirection(Vector3 dir)
    {
        // create ray from transform postion in the direction
        Ray ray = new Ray(transform.position, dir);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, maxDistance, layerMask))
        {
            // do not include ourselves
            if (raycastHit.collider.gameObject == gameObject) return null;
            // check for matching tag
            if (tagName == "" || raycastHit.collider.CompareTag(tagName))
            {
                // add game object to results
                Debug.DrawRay(ray.origin, ray.direction * raycastHit.distance, Color.red);
                return raycastHit.collider.gameObject;
            }
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green);
        }
        return null;
    }
    public override bool GetOpenDirection(ref Vector3 openDirection)
    {
        Vector3[] directions = Utilities.GetDirectionsInCircle(numRays, maxHalfAngle);
        foreach (var direction in directions)
        {
            Vector3 worldDir = transform.rotation * direction;
            GameObject go = GetGameObjectInDirection(worldDir);
            if (go == null) { openDirection = worldDir; return true; }
        }
        return false;
    }


    private void OnDrawGizmos()
    {
        if (!debug) return;
        Vector3[] directions = Utilities.GetDirectionsInCircle(numRays, maxHalfAngle);
        foreach (var dir in directions)
        {
            Gizmos.color = debugColor;
            Vector3 worldDir = transform.rotation * dir;
            Gizmos.DrawRay(transform.position, worldDir * maxDistance);
        }
    }

}
