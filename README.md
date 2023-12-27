# PathFindingForUnity
**JPS(B) + A* algorithm for Unity**

## Update
* This is highly optimized Grid base Pathfinding System.
* In 256x256 gridmap, this algorithm run 15 times faster than previous version of JPS algorithm.

## How to Use
1. Copy and Paste Asset>Scripts>Pathfinder folder to your project
2. Add Pathfinder Script to your gameobject need pathfinding.
3. Do Basic Setting on the unity editor's Inspector.
    * **Grid Start/End Point:** This is Grid base system. So, You have to limit domain of Grid.
    * **Cell Size:** This is size of cells of the grid system. As the size smaller, The path will be smoother and It will takes more time to process. (0.5 is preferred)
    * **Collision Check Sensor Size:** Actual collider size of gameobejct and collider size used on the pathfinding can be different. You can set collider size used on the pathfinding with this parameter.
    * **Priority Queue Max Size:** Maximum size of Priority Queue for A* algorithm's tasks.
    * **Layer To Check Collide:** You can set collider info to find path limited to a specific layer.
    * **Optimizing Path:** On the grid base path finding system, the ways object can move are straight and diagonal(=45 degrees). But sometime we have to move diagonal angles other than 45 degrees to get fastest path. If you check this, you can optimize the path.
4. GetComponent<PathFinder>getShortestPath(Vector2 start, Vector2 goal) will return shortest path as LinkedList<Vector2>
5. You can find out how to use Script>Player>PlayerMovement

## Performance
// 이미지 넣어주기

## caution
* If you have any question or feedback for this code. Contact to **Email: dongcheold147@gmail.com**
* I'm still student. So, The code is a bit messy. That's why feedback is welcome.

## Rreference
https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp - PriorityQueue.dll's source

