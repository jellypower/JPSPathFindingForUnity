# PathFindingForUnity

## Intro
* Highly optimized JPS(B) + A* base pathfinding systme for Unity.
* In 256x256 gridmap, this algorithm run 15 times faster than just JPS algorithm.
* This system is memory pool base. So it is fast because there is no realtime allocation during processing.
* Main algorithm to find path is C++ dll base. So the algorithm is optimized on the native level.
    * **To use C++ dll. Allow unsafe code for project.**

## How to Use

### UnityPackage
https://github.com/jellypower/JPSPathFindingForUnity/releases/tag/1.0.0

### class JPSGridInfoFaster
* **InGridStartPoint/InGridEndPoint:** Grid start and end point.
   * this system atuomatically recalculate start/end point of grid for faster boundary check.
* **InGridHorizontalSize/InGridVerticalSize:** grid horizontal/vertical devide size.
* **Collision Check Sensor Size:** Each cell of grid check whether itself blocked with this sensor size from center.
* **Layer To Check Collide:** On Baking the gridmap, which layers used.

### class JPSPathFinderFaster
* **priorityQueueMaxCapacity:** priority queue size for A* algorithm.
* **pathResultPoolMaxCapacity:** size of memory pool to store path finding results.
* **closeListCapacity:** close list size for A* algorithm.
* **optimizePath:** whether optimize the path for smooth and faster path.
* **findWalkableOnBlockedGoal:** whether automatically find closest non-blocked point when destination node is blocked. 

## Performance
<img src="https://github.com/jellypower/PublicImageDataBase/blob/main/Portfolio/JPSPathfinder/performance.png" alt="drawing" width="600"/>

* 2D project available
* Runtime block/non-blocking is possible

## Note
* Because of Readability, The native(c++) Code of JPSPathFinderFaster is not optimized for Debug build.
* However, the code was designed to benefit from compiler optimization.
* Therefore, Release builds can deliver 7 to 8 times the performance of Debug builds.

## Rreference
https://github.com/jellypower/JPSPathFinderFaster.git - C++/dll base pathfinder algorithm.
https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp - For previous version of JPS pathfinder

