using UnityEngine;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance;

    Vector2 targetPos;
    PathFinder pathFinder;

    LinkedList<Vector2> path = null;

    private void Awake()
    {
        instance = this;
        targetPos = transform.position;
    }

    void Start()
    {
        pathFinder = GetComponent<PathFinder>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        }
        move();
    }


    void move()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 start = transform.position;

            Vector2 goal = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            long time;

            path = pathFinder.getShortestPath(start, goal, out time);

            Debug.Log(time);


        }

        if (path != null && path.Count > 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, path.First.Value, 3 * Time.deltaTime);


            if ((Vector2)transform.position == path.First.Value)
            {
                path.RemoveFirst();
            }
        }
        
        

    }
}
