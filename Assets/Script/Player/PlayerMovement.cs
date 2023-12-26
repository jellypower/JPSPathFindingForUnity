using UnityEngine;
using System.Collections.Generic;
using DSNavigation;
using System.Linq;
using UnityEditor;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float m_movementSpeed = 10;

    public static PlayerMovement instance;

    PathFinder m_pathFinder;
    JPSGridInfoFaster m_jpsGridInfoFaster;
    JPSPathFinderFaster m_jpsPathFinderFaster;

    LinkedList<Vector2> path = null;

    Vector2 m_start;
    Vector2 m_goal;

    LinkedList<Vector2> m_fasterPath = new();

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        m_pathFinder = GetComponent<PathFinder>();
        m_jpsGridInfoFaster = GetComponent<JPSGridInfoFaster>();
        m_jpsPathFinderFaster = GetComponent<JPSPathFinderFaster>();
        m_jpsGridInfoFaster.BakeGridInfo();
    }

    // Update is called once per frame
    void Update()
    {
        move();
    }


    void move()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_start = transform.position;
            m_goal = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            long jpsBElapsedMS;
            long jpsElapsedMS;
            long jpsBElapsedTick;
            long jpsElapsedTick;
            m_fasterPath.Clear();
            {
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                {
                    bool isPathFound = m_jpsPathFinderFaster.FindPath
                        (m_jpsGridInfoFaster,
                        m_jpsGridInfoFaster.GetNodeIdx(m_start),
                        m_jpsGridInfoFaster.GetNodeIdx(m_goal),
                        ref m_fasterPath);
                }
                watch.Stop();
                jpsBElapsedMS = watch.ElapsedMilliseconds;
                jpsBElapsedTick = watch.ElapsedTicks;

                watch.Start();
                {
                    m_pathFinder.getShortestPath(m_start, m_goal);
                }
                watch.Stop();
                jpsElapsedMS = watch.ElapsedMilliseconds;
                jpsElapsedTick = watch.ElapsedTicks;

                Debug.Log(
                    "JPS(B): " + jpsBElapsedMS + "ms" + "(" + jpsBElapsedTick + "ticks)\t\t" +
                    "JPS: " + jpsElapsedMS + "ms" + "(" + jpsElapsedTick + "ticks)\t\t" +
                    "Performance improvement: " + ((float)jpsElapsedTick/jpsBElapsedTick)
                    );
            }
        }

        if (m_fasterPath != null && m_fasterPath.Count > 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, m_fasterPath.First.Value, m_movementSpeed * Time.deltaTime);
            if ((Vector2)transform.position == m_fasterPath.First.Value)
            {
                m_fasterPath.RemoveFirst();
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (EditorApplication.isPlaying)
        {
            Color originalColor = Gizmos.color;

            Gizmos.color = Color.red;
            Gizmos.DrawCube(new Vector3(m_start.x, m_start.y, 0), new Vector3(0.5f, 0.5f, 0.5f));


            Gizmos.color = Color.red;
            Gizmos.DrawCube(new Vector3(m_goal.x, m_goal.y, 0), new Vector3(0.5f, 0.5f, 0.5f));


            if (m_fasterPath.Count > 0)
            {
                Gizmos.color = Color.green;
                foreach (var loc in m_fasterPath)
                    Gizmos.DrawCube(new Vector3(loc.x, loc.y, 0), new Vector3(0.5f, 0.5f, 0.5f));

                Gizmos.DrawLine(transform.position, m_fasterPath.First.Value);

                for (LinkedListNode<Vector2> iter = m_fasterPath.First; iter.Next != null; iter = iter.Next)
                {
                    Vector3 from = iter.Value;
                    Vector3 to = iter.Next.Value;

                    Gizmos.DrawLine(from, to);
                }
            }

            Gizmos.color = originalColor;
        }
    }
}
