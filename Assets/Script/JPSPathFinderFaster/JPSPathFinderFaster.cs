using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace DSNavigation
{
    enum PathfindResult : byte
    {
        Found = 0,
        NotFound = 1,
        StartOrEndPointOutOfBound = 2,
        PriorityQueuePoolOverflow = 3,
        PathResultPoolOverflow = 4,
        CloseListPoolOverflow = 5,
    }

    internal struct PriorityQueuePair
    {
        Vector2Int m_nodeIdx;
        float m_fCost;
    }

    internal struct PathfinderPriorityQueuePool
    {
        public PriorityQueuePair[] m_priorityQueueData;
        public uint m_priorityQueueCapacity;
        public uint m_priorityQueueSize;

        public PathfinderPriorityQueuePool(uint capacity)
        {
            m_priorityQueueCapacity = capacity;
            m_priorityQueueData = new PriorityQueuePair[capacity];
            m_priorityQueueSize = 0;
        }
    };

    internal struct PathfinderCloseListPool
    {
        public Vector2Int[] m_closeListData;
        public uint m_closeListCapacity;
        public uint m_closeListSize;

        public PathfinderCloseListPool(uint capacity)
        {
            m_closeListCapacity = capacity;
            m_closeListData = new Vector2Int[capacity];
            m_closeListSize = 0;
        }
    }

    internal unsafe struct PathfinderPriorityQueuePoolUnsafe
    {
        public PriorityQueuePair* m_AStarPriorityQueue;
        public uint m_priorityQueueCapacity;
        public uint m_priorityQueueSize;
    };

    internal unsafe struct PathfinderCloseListPoolUnsafe
    {
        public Vector2Int* m_closeListData;
        public uint m_closeListCapacity;
        public uint m_closeListSize;
    }

    internal unsafe struct OutPathListPoolUnsafe
    {
        public Vector2Int* m_outPathListData;
        public uint m_outPathListCapacity;
        public uint m_outPathListSize;
    }

    internal unsafe struct JPSGridInfoToFindPathUnsafe
    {
        public ulong* m_gridScanningHorizontalBitmap; // for grid scannning
        public uint m_gridScanningHorizontalBitmapCapacity;

        public ulong* m_gridScanningVerticalBitmal; // for grid scanning
        public uint m_gridScanningVerticalBitmalCapacity;

        public PathFinderNode* m_gridMapPathfinderInfo;
        public uint m_gridMapHorizontalSize;
        public uint m_gridMapVerticalSize;
    };


    public class JPSPathFinderFaster : MonoBehaviour
    {
        [SerializeField] uint m_priorityQueueMaxCapacity = 200;
        [SerializeField] uint m_pathResultPoolMaxCapacity = 100;
        [SerializeField] uint m_closeListCapacity = 100;
        [SerializeField] bool m_optimizePath = true;
        [SerializeField] bool m_findWalkableOnBlockedGoal = true;

        PathfinderPriorityQueuePool m_priorityQueuePool;
        PathfinderCloseListPool m_closeListPool;
        Vector2Int[] m_pathResultPool;

        void Awake()
        {
            m_priorityQueuePool = new PathfinderPriorityQueuePool(m_priorityQueueMaxCapacity);
            m_closeListPool = new PathfinderCloseListPool(m_closeListCapacity);
            m_pathResultPool = new Vector2Int[m_pathResultPoolMaxCapacity];
        }

        public bool FindPath(in JPSGridInfoFaster jpsGrid, in Vector2 start, in Vector2 end, ref LinkedList<Vector2> outShortestPath)
        {
            Assert.IsTrue(outShortestPath.Count == 0, "outShortestPath must be cleared");

            uint tempOutPathSize = 0;

            bool bEndPointBlocked = false;
            Vector2Int startPointIdx = jpsGrid.GetNodeIdx(start);
            Vector2Int endPointIdx = jpsGrid.GetNodeIdx(end);
            if(m_findWalkableOnBlockedGoal && jpsGrid.GetBlockAt((uint)endPointIdx.x, (uint)endPointIdx.y))
            {
                jpsGrid.TryFindClosestPointIdx(endPointIdx, out endPointIdx);
                bEndPointBlocked = true;
            }


            bool isPathFound = FindPath_UnsafeInternal(
                jpsGrid.m_gridMapPathfinderInfo,
                m_priorityQueuePool,
                m_closeListPool,
                 startPointIdx, endPointIdx, 
                ref m_pathResultPool, ref tempOutPathSize);


            if (isPathFound)
            {
                if (m_optimizePath)
                {
                    uint sensingStartIdx = 0;
                    uint sensingEndIdx = 1;

                    Vector2 sensingStartPos = jpsGrid.GetNodeCenter(
                            (uint)m_pathResultPool[sensingStartIdx].x, (uint)m_pathResultPool[sensingStartIdx].y);

                    Vector2 sensingEndPos = jpsGrid.GetNodeCenter(
                        (uint)m_pathResultPool[sensingEndIdx].x, (uint)m_pathResultPool[sensingEndIdx].y);

                    if(bEndPointBlocked)
                    {
                        outShortestPath.AddFirst(jpsGrid.GetNodeCenter((uint)endPointIdx.x, (uint)endPointIdx.y));
                    }
                    else
                    {
                        outShortestPath.AddFirst(end);
                    }

                    while(sensingEndIdx < tempOutPathSize)
                    {
                        sensingStartPos = jpsGrid.GetNodeCenter(
                            (uint)m_pathResultPool[sensingStartIdx].x, (uint)m_pathResultPool[sensingStartIdx].y);

                        sensingEndPos = jpsGrid.GetNodeCenter(
                            (uint)m_pathResultPool[sensingEndIdx].x, (uint)m_pathResultPool[sensingEndIdx].y);

                        if (Physics2D.BoxCast(sensingStartPos, jpsGrid.CollisionCheckSensorSize, 0,
                        sensingEndPos - sensingStartPos, Vector2.Distance(sensingEndPos, sensingStartPos),
                        jpsGrid.LayerToCheckCollide))
                        {
                            sensingStartIdx = sensingEndIdx - 1;
                            ++sensingEndIdx;

                            Vector2 posToAdd = jpsGrid.GetNodeCenter(
                            (uint)m_pathResultPool[sensingStartIdx].x, (uint)m_pathResultPool[sensingStartIdx].y);

                            outShortestPath.AddFirst(posToAdd);
                        }
                        else
                        {
                            ++sensingEndIdx;
                        }
                    }

                    outShortestPath.AddFirst(start);
                }
                else
                {
                    for (int i = 0; i < tempOutPathSize; ++i)
                        outShortestPath.AddFirst(jpsGrid.GetNodeCenter((uint)m_pathResultPool[i].x, (uint)m_pathResultPool[i].y));
                }
            }
            return isPathFound;
        }

        bool FindPath_UnsafeInternal
            (in JPSGridInfoToFindPath GridInfo, 
            in PathfinderPriorityQueuePool priorityQueueSafe,
            in PathfinderCloseListPool closeListSafe,
            in Vector2Int Start, in Vector2Int End,
            ref Vector2Int[] OutPath, ref uint OutPathSize)
        {
            unsafe
            {
                fixed (ulong* horizontalBitmapUnsafe = GridInfo.m_horizontalBitmap)
                fixed (ulong* verticalBitmapUnsafe = GridInfo.m_verticalBitmap)
                fixed (PathFinderNode* gridmapPathfinderInfo = GridInfo.m_pathFinderGridmap)
                fixed (PriorityQueuePair* priorityQueueDataUnsafe = priorityQueueSafe.m_priorityQueueData)
                fixed (Vector2Int* closeListDataUnsafe = closeListSafe.m_closeListData)
                fixed (Vector2Int* outPathUnsafe = OutPath)
                {

                    JPSGridInfoToFindPathUnsafe unsafeGridInfo = new JPSGridInfoToFindPathUnsafe()
                    {
                        m_gridScanningHorizontalBitmap = horizontalBitmapUnsafe,
                        m_gridScanningHorizontalBitmapCapacity = GridInfo.m_horizontalBitmapSize,
                        m_gridScanningVerticalBitmal = verticalBitmapUnsafe,
                        m_gridScanningVerticalBitmalCapacity = GridInfo.m_verticalBitmalSize,
                        m_gridMapPathfinderInfo = gridmapPathfinderInfo,
                        m_gridMapHorizontalSize = GridInfo.m_gridMapHorizontalSize,
                        m_gridMapVerticalSize = GridInfo.m_gridMapVerticalSize
                    };

                    PathfinderPriorityQueuePoolUnsafe unsafePriorityQueuePool = new PathfinderPriorityQueuePoolUnsafe()
                    {
                        m_AStarPriorityQueue = priorityQueueDataUnsafe,
                        m_priorityQueueCapacity = priorityQueueSafe.m_priorityQueueCapacity,
                        m_priorityQueueSize = priorityQueueSafe.m_priorityQueueSize
                    };

                    PathfinderCloseListPoolUnsafe unsafeCloseListPool = new PathfinderCloseListPoolUnsafe()
                    {
                        m_closeListData = closeListDataUnsafe,
                        m_closeListCapacity = closeListSafe.m_closeListCapacity,
                        m_closeListSize = closeListSafe.m_closeListSize
                    };

                    OutPathListPoolUnsafe unsafeOutPath = new OutPathListPoolUnsafe()
                    {
                        m_outPathListData = outPathUnsafe,
                        m_outPathListCapacity = m_pathResultPoolMaxCapacity,
                        m_outPathListSize = 0
                    };

                    PathfindResult result = FindPathJPSFaster
                        (ref unsafeGridInfo,
                        ref unsafePriorityQueuePool,
                        ref unsafeCloseListPool,
                        Start, End,
                        ref unsafeOutPath);

                    OutPathSize = unsafeOutPath.m_outPathListSize;

                    switch (result)
                    {
                        case PathfindResult.Found:
                            return true;
                        case PathfindResult.NotFound:
                            Debug.LogWarning("Destination is not reachable");
                            return false;
                        case PathfindResult.StartOrEndPointOutOfBound:
                            Debug.LogError("start or end point is out of gridmap");
                            return false;
                        case PathfindResult.PriorityQueuePoolOverflow:
                            Debug.LogError("Priority queue capacity is not enough");
                            return false;
                        case PathfindResult.PathResultPoolOverflow:
                            Debug.LogError("Path result pool capacity is not enough");
                            return false;
                        case PathfindResult.CloseListPoolOverflow:
                            Debug.LogError("Close list pool capacity is not enough");
                            return false;

                        default:
                            Assert.IsTrue(false);
                            return false;
                    }

                }
            }
        }


        [DllImport("JPSPathfinderFaster")]
        unsafe extern static private PathfindResult FindPathJPSFaster
            (ref JPSGridInfoToFindPathUnsafe InGridInfo,
            ref PathfinderPriorityQueuePoolUnsafe InPathFinderPriorityQueuePool,
            ref PathfinderCloseListPoolUnsafe InPathFinderCloseListPool,
            Vector2Int Start, Vector2Int End,
            ref OutPathListPoolUnsafe OutPath);
    }

}