using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ObelixAgent : Agent
{
    public Transform[] Menhirs;
    public Transform[] Destinations;

    public float speedMultiplier = 0.1f;
    public float rotationMultiplier = 5f;

    private Transform carriedMenhir = null;
    private bool hasMenhir = false;
    public override void OnEpisodeBegin()
    {
        // reset de positie en orientatie als de agent gevallen is
        if (this.transform.localPosition.y < 0)
        {

            this.transform.localPosition = new Vector3(0, 0.5f, 0);
            this.transform.localRotation = Quaternion.identity;
        }

        //verplaats de menhir naar een nieuwe willekeurige locatie
        foreach (var menhir in Menhirs)
        {
            menhir.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
        }

        carriedMenhir = null;
        hasMenhir = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent posities
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(transform.forward);
        // Menhir posities
        foreach (var menhir in Menhirs)
        {
            sensor.AddObservation(menhir.localPosition - transform.localPosition);
        }
        // Destination posities
        foreach (var destination in Destinations)
        {
            sensor.AddObservation(destination.localPosition - transform.localPosition);
        }

        sensor.AddObservation(hasMenhir ? 1.0f : 0.0f);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Acties, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.z = actionBuffers.ContinuousActions[0];
        transform.Translate(controlSignal * speedMultiplier);

        transform.Rotate(0.0f, rotationMultiplier * actionBuffers.ContinuousActions[1], 0.0f);

        AddReward(-0.001f);

        if (transform.localPosition.y < 0)
        {
            AddReward(-1f);
            EndEpisode();
        }
        if (carriedMenhir != null)
        {
            //carriedMenhir.position = transform.position + transform.forward * 1f + Vector3.up * 0.5f;
            carriedMenhir.position = transform.position + Vector3.up * 1.5f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Menhir") && carriedMenhir == null)
        {
            carriedMenhir = other.transform;
            hasMenhir = true;
            AddReward(0.5f);
            Debug.Log("Menhir aangeraakt");
        }

        if (other.CompareTag("Menhir") && carriedMenhir != null && other.transform != carriedMenhir)
        {
            AddReward(-0.2f);
            Debug.Log("Menhir aangeraakt terwijl je al een hebt");
        }

        if (other.CompareTag("Destination") && carriedMenhir == null)
        {
            AddReward(-0.1f);
            Debug.Log("Bestemming bereikt zonder menhir");
        }

        if (other.CompareTag("Destination") && carriedMenhir != null)
        {
            hasMenhir = false;
            carriedMenhir = null;

            AddReward(1f);
            Debug.Log("Menhir naar bestemming gebracht");
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var c = actionsOut.ContinuousActions;

        // W/S (Vertical) = forward/back
        c[0] = Input.GetAxis("Vertical");

        // A/D (Horizontal) = turn left/right
        c[1] = Input.GetAxis("Horizontal");
    }

}
