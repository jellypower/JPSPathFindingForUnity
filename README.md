# PathFindingForUnity
**JPS + A* algorithm for Unity**

## Info
1. This is Grid base Pathfinding System.
2. The main source code of this system is located in "Script>Pathfinder"
3. class Script>Pathfinder>Node is component of each cell object on the grid system.

## How to Use
1. Add Pathfinder Script to your gameobject need pathfinding.
2. Do Basic Setting on the unity editor's Inspector.
    * **Grid Start/End Point:** This is Grid base system. So, You have to limit domain of Grid.
    * **Cell Size:** This is size of cells of the grid system. As the size smaller, The path will be smoother and It will takes more time to process. (0.5 is preferred)
    * **Collision Check Sensor Size:** Actual collider size of gameobejct and collider size used on the pathfinding can be different. You can set collider size used on the pathfinding with this parameter.
    * **Priority Queue Max Size:** Maximum size of Priority Queue for A* algorithm's tasks.
    * **Layer To Check Collide:** You can set collider info to find path limited to a specific layer.
    * **Optimizing Path:** On the grid base path finding system, the ways object can move are straight and diagonal(=45 degrees). But sometime we have to move diagonal angles other than 45 degrees to get fastest path. If you check this, you can optimize the path.
3. GetComponent<PathFinder>getShortestPath(Vector2 start, Vector2 goal) will return shortest path as LinkedList<Vector2>
4. You can find out how to use Script>Player>PlayerMovement

## Improvement
* Now, Gird Generator is on PathFinder class. However, It will be changed as seperated class(GridGenerator) as if you're using Nav Mesh on Unity 3D.
* The Grid's collision info is not updated on the realtime yet.(Grid is updated only on the Start but Update) If GridGenerator class is made, that function also will be updated soon.
* If you want to use Debug Tools, You can use some functions under the OnDrawGizmos() function. (Showing closed and open nodes, drawing lines between nodes etc.)

## caution
* If you have any question or feedback for this code. Contact to **Email: dongcheold147@gmail.com**
* I'm still student. So, The code is a bit messy. That's why feedback is welcome.

## Rreference
https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp - PriorityQueue.dll's source

