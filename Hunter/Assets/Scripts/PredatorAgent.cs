using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PredatorAgent : Agent
{
    public GameObject Hunter;

    public float speedMultiplier = 0.1f;
    public float rotationMultiplier = 5f;

    float minX = -10;
    float maxX = 10;
    float minZ = -10;
    float maxZ = 10;

    float previousDistanceToHunter;
    public override void OnEpisodeBegin()
    {
        //Random teleport
        transform.localPosition = new Vector3(
            Random.Range(minX, maxX),
            0.5f,
            Random.Range(minZ, maxZ)
        );

        previousDistanceToHunter = Vector3.Distance(
        transform.localPosition,
        Hunter.transform.localPosition
        );
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // PredatorAgent positie
        sensor.AddObservation(this.transform.localPosition - transform.localPosition);

        // HunterAgent positie
        sensor.AddObservation(Hunter.transform.localPosition - transform.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Acties, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.z = actionBuffers.ContinuousActions[0];
        transform.Translate(controlSignal * speedMultiplier);

        transform.Rotate(0.0f, rotationMultiplier * actionBuffers.ContinuousActions[1], 0.0f);

        //Reward voor dichtbij komen
        float currentDistance = Vector3.Distance(
            transform.localPosition,
            Hunter.transform.localPosition
        );

        float distanceDelta = previousDistanceToHunter - currentDistance;
        AddReward(distanceDelta * 0.1f);

        previousDistanceToHunter = currentDistance;

        //Hunter richting kijken
        Vector3 directionToHunter = (Hunter.transform.localPosition - transform.localPosition).normalized;
        float alignment = Vector3.Dot(transform.forward, directionToHunter);

        AddReward(alignment * 0.01f);

        // Inactief bestraffen om sneller te leren stimuleren
        AddReward(-0.0005f);

    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Predator raakt muur");
            AddReward(-0.5f);
            TeleportToRandomPosition();
        }
        if (collision.gameObject.CompareTag("Hunter"))
        {
            Debug.Log("Predator heeft hunter gevangen");
            AddReward(2f);
            EndEpisode();
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RedBlock"))
        {
            Debug.Log("Predator raakt blokje");
            AddReward(-1f);
        }
    }

    void TeleportToRandomPosition()
    {
        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);

        Vector3 newPosition = new Vector3(randomX, 0.5f, randomZ);

        transform.localPosition = newPosition;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    //public override void Heuristic(in ActionBuffers actionsOut)
    //{
    //    var c = actionsOut.ContinuousActions;

    //    // Vooruit achteruit
    //    c[0] = Input.GetAxis("Vertical");

    //    // Links rechts
    //    c[1] = Input.GetAxis("Horizontal");
    //}
}
