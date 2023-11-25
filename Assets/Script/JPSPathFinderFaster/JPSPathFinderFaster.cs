using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


namespace DSNavigation
{
    public struct PathfinderPriorityQueuePool
    {
        public PathFinderNode[] m_AStarPriorityQueue;
        public uint m_priorityQueueCapacity;
        public uint m_priorityQueueSize;
    };

    internal unsafe struct PathfinderPriorityQueuePoolUnsafe
    {
        public PathFinderNode* m_AStarPriorityQueue;
        public uint m_priorityQueueCapacity;
        public uint m_priorityQueueSize;
    };

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
        [SerializeField] int priorityQueueMaxSize = 200;

        public void TempCall()
        {

            JPSGridInfoToFindPath tempGridInfo = new JPSGridInfoToFindPath();
            PathfinderPriorityQueuePool tempPriorityQ = new PathfinderPriorityQueuePool();
            Vector2Int tempStart = new Vector2Int();
            Vector2Int tempEnd = new Vector2Int();
            Vector2Int[] tempOutPath = new Vector2Int[10];
            for (int i = 0; i < 10; i++) { tempOutPath[i] = new Vector2Int(); }
            int tempOutPathSize = 0;


            FindPath(tempGridInfo, tempPriorityQ, tempStart, tempEnd, ref tempOutPath, ref tempOutPathSize);
        }

        // public함수는 Safe하게 넘겨주기
        bool FindPath
            (in JPSGridInfoToFindPath GridInfo, 
            in PathfinderPriorityQueuePool aStarPriorityQueue
            ,in Vector2Int Start, in Vector2Int End,
            ref Vector2Int[] OutPath, ref int OutPathSize)
        {
            unsafe {
                fixed (ulong* _horizontalBitmap = GridInfo.m_horizontalBitmap)
                fixed (ulong* _verticalBitmap = GridInfo.m_verticalBitmap)
                fixed (PathFinderNode* _gridmapPathfinderInfo = GridInfo.m_pathFinderGridmap)
                fixed (PathFinderNode* _AStarPriorityQueue = aStarPriorityQueue.m_AStarPriorityQueue)
                fixed (Vector2Int* _outPathUnsafe = OutPath)
                {

                    JPSGridInfoToFindPathUnsafe unsafeGridInfo = new JPSGridInfoToFindPathUnsafe()
                    {
                        m_gridScanningHorizontalBitmap = _horizontalBitmap,
                        m_gridScanningHorizontalBitmapCapacity = GridInfo.m_horizontalBitmapSize,
                        m_gridScanningVerticalBitmal = _verticalBitmap,
                        m_gridScanningVerticalBitmalCapacity = GridInfo.m_verticalBitmalSize,
                        m_gridMapPathfinderInfo = _gridmapPathfinderInfo,
                        m_gridMapHorizontalSize = GridInfo.m_gridMapHorizontalSize,
                        m_gridMapVerticalSize = GridInfo.m_gridMapVerticalSize
                    };

                    PathfinderPriorityQueuePoolUnsafe unsafePriorityQueuePool = new PathfinderPriorityQueuePoolUnsafe()
                    {
                        m_AStarPriorityQueue = _AStarPriorityQueue,
                        m_priorityQueueCapacity = aStarPriorityQueue.m_priorityQueueCapacity,
                        m_priorityQueueSize = aStarPriorityQueue.m_priorityQueueSize
                    };

                    print(FindPathJPSFaster
                        (ref unsafeGridInfo, ref unsafePriorityQueuePool,
                        Start, End,
                        _outPathUnsafe, ref OutPathSize));
                }

            }
            return true;
        }


        [DllImport("D:\\PrivateLibrary\\JPSPathfinderFaster\\x64\\Debug\\JPSPathfinderFaster")]
        unsafe extern static private bool FindPathJPSFaster
            (ref JPSGridInfoToFindPathUnsafe InGridInfo,
            ref PathfinderPriorityQueuePoolUnsafe InAStarPriorityQueue,
            Vector2Int Start, Vector2Int End,
            Vector2Int* OutPath, ref int OutPathSize);
    }

}