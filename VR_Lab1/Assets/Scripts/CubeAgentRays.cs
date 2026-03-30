using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents;
using UnityEngine;

public class CubeAgentRays : Agent
{
    public Transform Target;
    public Transform Platform;

    public float speedMultiplier = 0.1f;
    public float rotationMultiplier = 5f;

    bool hasBlock = false;

    public override void OnEpisodeBegin()
    {
        // reset de positie en orientatie als de agent gevallen is
        if (this.transform.localPosition.y < 0)
        {

            this.transform.localPosition = new Vector3(0, 0.5f, 0);
            this.transform.localRotation = Quaternion.identity;
        }

        // verplaats de target naar een nieuwe willekeurige locatie 
        Target.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);

        Target.gameObject.SetActive(true);

        hasBlock = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target en Agent posities
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(hasBlock ? 1 : 0);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Acties, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.z = actionBuffers.ContinuousActions[0];
        transform.Translate(controlSignal * speedMultiplier);

        transform.Rotate(0.0f, rotationMultiplier * actionBuffers.ContinuousActions[1], 0.0f);

        if (transform.localPosition.y < 0)
        {
            AddReward(-0.5f);
            EndEpisode();

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasBlock && other.transform == Target)
        {
            hasBlock = true;
            Target.gameObject.SetActive(false);
            AddReward(0.5f);
        }

        if (hasBlock && other.transform == Platform)
        {
            AddReward(1.0f);
            EndEpisode();

        }
    }

    //public override void Heuristic(in ActionBuffers actionsOut)
    //{
    //    var c = actionsOut.ContinuousActions;

    //    // W/S (Vertical) = forward/back
    //    c[0] = Input.GetAxis("Vertical");

    //    // A/D (Horizontal) = turn left/right
    //    c[1] = Input.GetAxis("Horizontal");
    //}
}
