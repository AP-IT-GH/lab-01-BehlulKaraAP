using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FindPathAStar : MonoBehaviour
{
    public Maze maze;
    public Material closedMaterial;
    public Material openMaterial;
    public GameObject start;
    public GameObject end;
    public GameObject pathP;

    PathMarker startNode;
    PathMarker goalNode;
    PathMarker lastPos;
    bool done = false;
    bool hasStarted = false;

    List<PathMarker> open = new List<PathMarker>();
    List<PathMarker> closed = new List<PathMarker>();

    void RemoveAllMarkers()
    {

        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");

        foreach (GameObject m in markers) Destroy(m);

        GameObject goal = GameObject.FindGameObjectWithTag("Goal");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        // Destroy(goal);
        Destroy(player);
    }

    void BeginSearch()
    {

        done = false;

        open.Clear();
        closed.Clear();
        path.Clear();
        RemoveAllMarkers();

        List<MapLocation> locations = new List<MapLocation>();

        for (int z = 1; z < maze.depth - 1; ++z)
        {
            for (int x = 1; x < maze.width - 1; ++x)
            {

                if (maze.map[x, z] != 1)
                {
                    locations.Add(new MapLocation(x, z));
                }
            }
        }
        //locations.Shuffle();

        Vector3 startLocation = new Vector3(1, 0.5f, 1);
        startNode = new PathMarker(new MapLocation(1, 1),
            0.0f, 0.0f, 0.0f, Instantiate(start, startLocation, Quaternion.identity), null);

        Vector3 endLocation = new Vector3(Random.Range(5, 8), 0.5f, Random.Range(5, 8));
        goalNode = new PathMarker(new MapLocation((int)endLocation.x, (int)endLocation.z),
            0.0f, 0.0f, 0.0f, Instantiate(end, endLocation, Quaternion.identity), null);

        open.Add(startNode);
        lastPos = startNode;
    }

    void Search(PathMarker thisNode)
    {

        if (thisNode.Equals(goalNode))
        {

            done = true;

            return;
        }

        foreach (MapLocation dir in maze.directions)
        {

            MapLocation neighbour = dir + thisNode.location;

            if (neighbour.x < 1 || neighbour.x > maze.width || neighbour.z < 1 || neighbour.z > maze.depth) continue;

            if (maze.map[neighbour.x, neighbour.z] == 1) continue;
            if (IsClosed(neighbour)) continue;

            float g = Vector2.Distance(thisNode.location.ToVector(), neighbour.ToVector()) + thisNode.G;
            float h = Vector2.Distance(neighbour.ToVector(), goalNode.location.ToVector());
            float f = g + h;

            GameObject pathBlock = Instantiate(pathP, new Vector3(neighbour.x * maze.scale, 0.0f, neighbour.z * maze.scale), Quaternion.identity);

            if (!UpdateMarker(neighbour, g, h, f, thisNode))
            {

                open.Add(new PathMarker(neighbour, g, h, f, pathBlock, thisNode));
            }
        }
        open = open.OrderBy(p => p.F).ToList<PathMarker>();
        PathMarker pm = (PathMarker)open.ElementAt(0);
        closed.Add(pm);

        open.RemoveAt(0);

        if(pm.marker != null)
        {
            pm.marker.GetComponent<Renderer>().material = closedMaterial;
        }
        

        lastPos = pm;
    }

    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt)
    {

        foreach (PathMarker p in open)
        {

            if (p.location.Equals(pos))
            {

                p.G = g;
                p.H = h;
                p.F = f;
                p.parent = prt;
                return true;
            }
        }
        return false;
    }

    bool IsClosed(MapLocation marker)
    {

        foreach (PathMarker p in closed)
        {

            if (p.location.Equals(marker)) return true;
        }
        return false;
    }

    void Start()
    {

    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.P))
        {

            BeginSearch();
            hasStarted = true;
        }


        if (hasStarted)
            if (Input.GetKeyDown(KeyCode.C)) Search(lastPos);
    }

    // The coroutine function
    bool searchingHasFinished = false;
    IEnumerator Searching()
    {
        Debug.Log("searching started!");

        while (!done)
        {
            // Perform some task
            Debug.Log("Coroutine is running...");
            Search(lastPos);
            // Wait for the next frame
            //            yield return true;
        }

        searchingHasFinished = true;
        yield return null;

        Debug.Log("Coroutine finished!");
    }

    bool PathHasConstructed = false;
    List<PathMarker> path = new List<PathMarker>();
    void ReconstructPath()
    {

        path.Add(closed[closed.Count - 1]);
        var p = closed[closed.Count - 1].parent;
        while (p != startNode)
        {
            path.Insert(0, p);
            p = p.parent;
        }
        path.Insert(0, startNode);
        PathHasConstructed = true;


    }
}
