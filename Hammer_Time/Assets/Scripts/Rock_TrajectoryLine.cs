using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct RegisteredRock
{
    public Rock real;
    public Rock hidden;
}
public class Rock_TrajectoryLine : MonoBehaviour
{
    public static bool charging;

    public GameObject rock;
    public Transform referenceRock;

    private Scene mainScene;
    private Scene physicsScene;

    public GameObject marker;
    private List<GameObject> markers = new List<GameObject>();
    private Dictionary<string, RegisteredRock> allRocks = new Dictionary<string, RegisteredRock>();

    public GameObject objectsToSpawn;


    void Start()
    {
        Physics2D.autoSimulation = false;

        mainScene = SceneManager.GetActiveScene();
        physicsScene = SceneManager.CreateScene("physics-scene", new CreateSceneParameters(LocalPhysicsMode.Physics2D));

        PreparePhysicsScene();
    }

    void FixedUpdate()
    { 
        if (Input.GetMouseButton(0))
        {
            ShowTrajectory();
        }

        mainScene.GetPhysicsScene2D().Simulate(Time.fixedDeltaTime);
    }

    public void RegisterRock(Rock rock)
    {
        if (!allRocks.ContainsKey(rock.gameObject.name))
        {
            allRocks[rock.gameObject.name] = new RegisteredRock();
        }

        var rocks = allRocks[rock.gameObject.name];
        if(string.Compare(rock.gameObject.name, physicsScene.name) == 0)
        {
            rocks.hidden = rock;
        }
        else
        {
            rocks.real = rock;
        }

        allRocks[rock.gameObject.name] = rocks;
    }

    public void PreparePhysicsScene()
    {
        SceneManager.SetActiveScene(physicsScene);

        GameObject g = GameObject.Instantiate(objectsToSpawn);
        g.transform.name = "ReferenceRock";
        g.GetComponent<Rock>().isReference = true;
        Destroy(g.GetComponent<SpriteRenderer>());

        SceneManager.SetActiveScene(mainScene);
    }

    public void CreateMovementMarkers()
    {
        foreach (var rockType in allRocks)
        {
            var rocks = rockType.Value;
            Rock hidden = rocks.hidden;

            GameObject g = GameObject.Instantiate(marker, hidden.transform.position, Quaternion.identity);
            g.transform.localScale = new Vector2(0.3f, 0.3f);
            markers.Add(g);
        }
    }

    public void ShowTrajectory()
    {
        SyncRocks();

        allRocks["ReferenceRock"].hidden.transform.rotation = referenceRock.transform.rotation;
        allRocks["ReferenceRock"].hidden.GetComponent<Rigidbody>().velocity = referenceRock.transform.TransformDirection(Vector2.up * 15f);
        allRocks["ReferenceRock"].hidden.GetComponent<Rigidbody>().useGravity = false;

        int steps = (int)(2f / Time.fixedDeltaTime);
        for (int i = 0; i < steps; i++)
        {
            physicsScene.GetPhysicsScene2D().Simulate(Time.fixedDeltaTime);
            CreateMovementMarkers();
        }
    }

    public void SyncRocks()
    {
        foreach (var rockType in allRocks)
        {
            var rocks = rockType.Value;

            Rock visual = rocks.real;
            Rock hidden = rocks.hidden;
            var rb = hidden.GetComponent<Rigidbody2D>();

            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            hidden.transform.position = visual.transform.position;
            hidden.transform.rotation = hidden.transform.rotation;
        }
    }
}
