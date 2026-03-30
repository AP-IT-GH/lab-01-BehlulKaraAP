using UnityEngine;

public class FollowWaypoint : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] GameObject[] waypoints;
    int currentWaypointIndex = 0;

    [SerializeField] float rotSpeed = 5f;
    [SerializeField] float speed = 5f;
    float distanceCheck = 2f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Transform target = waypoints[currentWaypointIndex].transform;

        Vector3 relativePos = target.position - transform.position;

        Quaternion lookRotation = Quaternion.LookRotation(relativePos);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotSpeed);

        transform.Translate(0, 0, speed * Time.deltaTime);

        if (relativePos.magnitude < distanceCheck)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
        }
    }
}
