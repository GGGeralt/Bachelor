using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
[RequireComponent(typeof(SplineRoad))]
public class City : MonoBehaviour
{
    public static City Instance;
    public SplineContainer splineContainer;
    public SplineRoad splineRoad;
    public CityData cityData;

    public List<Waypoint> waypoints;
    [SerializeField] private List<Waypoint> startPoints;

    public GameObject carPrefab;
    [SerializeField] private GameObject carHolder;
    [SerializeField] private bool isPaused = false;
    [SerializeField] private GameObject pausedPanel;


    private void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();

        startPoints = waypoints.FindAll(w => w.startPoint == true);

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        UpdateRoadData();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            System.Random random = new();
            Waypoint startPoint = startPoints[random.Next(startPoints.Count)];
            Car car = Instantiate(carPrefab, startPoint.transform.position, Quaternion.identity, carHolder.transform).GetComponent<Car>();

            car.currentSpline = splineContainer.Splines[startPoint.GetRandomRoad().GetIndex()];
            car.nextWaypoint = startPoint.GetRandomRoad().GetWaypoint();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPaused = !isPaused;
            pausedPanel.SetActive(isPaused);

            if (isPaused)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }
        }
    }

    public void LoadWaypoints()
    {
        waypoints = GetComponentsInChildren<Waypoint>().ToList<Waypoint>();
    }

    public void GenerateRoads()
    {
        foreach (Waypoint waypoint in waypoints)
        {
            foreach (Road road in waypoint.GetRoads())
            {
                Spline spline = splineContainer.AddSpline();

                BezierKnot[] knots = new BezierKnot[2];
                knots[0] = new BezierKnot(waypoint.transform.position - transform.position);
                knots[1] = new BezierKnot(road.to.transform.position - transform.position);
                spline.Knots = knots;
                spline.SetTangentMode(TangentMode.AutoSmooth);

                road.SetLength(spline.GetLength());
                road.SetIndex(splineContainer.Splines.Count - 1);
            }
        }
    }

    public void UpdateRoadData()
    {
        int splineIndex = 0;

        foreach (Waypoint waypoint in waypoints)
        {
            foreach (Road road in waypoint.GetRoads())
            {
                road.SetLength(splineContainer.Splines[splineIndex].GetLength());
                road.SetIndex(splineIndex);
                splineIndex++;
            }
        }
    }

    public void DeleteRoads()
    {
        foreach (Spline road in splineContainer.Splines)
        {
            splineContainer.RemoveSpline(road);
        }
        splineRoad.DeleteMesh();
    }
}
