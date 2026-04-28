using NUnit.Framework;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;

public class HunterAgent : Agent
{
    public GameObject Predator;

    public float speedMultiplier = 0.1f;
    public float rotationMultiplier = 5f;

    public GameObject redBlock;
    private GameObject[] blocksArray;

    int maxBlock = 10;

    float minX = -10;
    float maxX = 10;
    float minZ = -10;
    float maxZ = 10;
    public override void OnEpisodeBegin()
    {
        // Verwijder oude blokken
        if (blocksArray != null)
        {
            for (int i = 0; i < blocksArray.Length; i++)
            {
                if (blocksArray[i] != null)
                {
                    Destroy(blocksArray[i]);
                }
            }
        }

        blocksArray = new GameObject[maxBlock];

        for (int i = 0; i < maxBlock; i++)
        {
            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);

            Vector3 randomPosition = new Vector3(randomX, 0.5f, randomZ);

            blocksArray[i] = Instantiate(redBlock, randomPosition, Quaternion.identity);
        }

        //Random teleport
        transform.localPosition = new Vector3(
            Random.Range(minX, maxX),
            0.5f,
            Random.Range(minZ, maxZ)
        );
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // HunterAgent positie
        sensor.AddObservation(this.transform.localPosition - transform.localPosition);

        // RedBlock posities
        //foreach (var block in blocksArray)
        //{
        //    sensor.AddObservation(block.transform.localPosition - transform.localPosition);
        //}

        //Dichtste blok positie
        GameObject closestBlock = null;
        float closestDist = float.MaxValue;

        foreach (var block in blocksArray)
        {
            if (block.activeSelf)
            {
                float dist = Vector3.Distance(transform.localPosition, block.transform.localPosition);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestBlock = block;
                }
            }
        }

        if (closestBlock != null)
        {
            sensor.AddObservation(closestBlock.transform.localPosition - transform.localPosition);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
        }

        // PredatorAgent positie
        sensor.AddObservation(Predator.transform.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Acties, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.z = actionBuffers.ContinuousActions[0];
        transform.Translate(controlSignal * speedMultiplier);

        transform.Rotate(0.0f, rotationMultiplier * actionBuffers.ContinuousActions[1], 0.0f);

        // Reward voor dichtbij komen
        //float closestDist = float.MaxValue;

        //foreach (var block in blocksArray)
        //{
        //    if (block.activeSelf)
        //    {
        //        float dist = Vector3.Distance(transform.localPosition, block.transform.localPosition);
        //        if (dist < closestDist)
        //        {
        //            closestDist = dist;
        //        }
        //    }
        //}

        //AddReward(-closestDist * 0.001f);

        float closestDist = float.MaxValue;
        bool foundBlock = false;

        foreach (var block in blocksArray)
        {
            if (block.activeSelf)
            {
                float dist = Vector3.Distance(transform.localPosition, block.transform.localPosition);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    foundBlock = true;
                }
            }
        }

        if (foundBlock)
        {
            AddReward(-closestDist * 0.001f);
        }

        // Inactief bestraffen om sneller te leren stimuleren
        AddReward(-0.001f);

        // Alle blokken zijn gepakt
        bool allCollected = true;

        foreach (var block in blocksArray)
        {
            if (block.activeSelf)
            {
                allCollected = false;
                break;
            }
        }

        if (allCollected)
        {
            AddReward(2f);
            EndEpisode();
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Hunter raakt muur");
            AddReward(-0.5f);
            TeleportToRandomPosition();
        }
        if (collision.gameObject.CompareTag("Predator"))
        {
            Debug.Log("Hunter heeft Predator geraakt");
            AddReward(-1f);
            EndEpisode();
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RedBlock"))
        {
            Debug.Log("Hunter raakt blokje");
            other.gameObject.SetActive(false);
            AddReward(1f);
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
