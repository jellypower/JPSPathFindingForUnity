using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace DSNavigation
{
    public struct PathFinderNode
    {
        public float m_gCost;
        public float m_hCost;
        public Vector2Int m_paretnNode;
        public bool m_onCloseList;
        public bool m_onOpenList;

        public float getFCost() { return m_gCost + m_hCost; }
    }

    public struct JPSGridInfoToFindPath
    {
        public ulong[] m_horizontalBitmap; // for grid scannning
        public uint m_horizontalBitmapSize;

        public ulong[] m_verticalBitmap; // for grid scanning
        public uint m_verticalBitmalSize;

        public PathFinderNode[] m_pathFinderGridmap;
        public uint m_gridMapHorizontalSize;
        public uint m_gridMapVerticalSize;
    }

    public class JPSGridInfoFaster : MonoBehaviour
    {
        public JPSGridInfoToFindPath m_gridMapPathfinderInfo;

        [Header("Grid Info")]
        [SerializeField] Vector2 m_gridStartPoint;
        [SerializeField] Vector2 m_gridEndPoint;
        [SerializeField] uint m_gridHorizontalSize;
        [SerializeField] uint m_gridVerticalSize;


        [Header("Bake Info")]
        [SerializeField] Vector2 m_collisionCheckSensorSize = new Vector2(1, 1);
        [SerializeField] LayerMask m_layerTocheckCollide;

        
        void Awake()
        {
            m_gridMapPathfinderInfo = new JPSGridInfoToFindPath();

            m_gridMapPathfinderInfo.m_gridMapHorizontalSize
                = ((m_gridHorizontalSize + 2) / 64 +
                (m_gridHorizontalSize + 2) % 64 == 0 ? 0u : 1u) * 64;
            m_gridMapPathfinderInfo.m_gridMapVerticalSize
                = ((m_gridVerticalSize + 2) / 64 +
                (m_gridVerticalSize + 2) % 64 == 0 ? 0u : 1u) * 64;
            // "+ 2" is for avoiding check edge during grid scanning

            m_gridMapPathfinderInfo.m_horizontalBitmapSize =
               (m_gridMapPathfinderInfo.m_gridMapHorizontalSize / 64) * 
                   m_gridMapPathfinderInfo.m_gridMapVerticalSize;

            m_gridMapPathfinderInfo.m_verticalBitmalSize =
                m_gridMapPathfinderInfo.m_gridMapHorizontalSize *
                (m_gridMapPathfinderInfo.m_gridMapVerticalSize / 64);

            m_gridMapPathfinderInfo.m_horizontalBitmap
                = new ulong[m_gridMapPathfinderInfo.m_horizontalBitmapSize];
            m_gridMapPathfinderInfo.m_verticalBitmap
                = new ulong[m_gridMapPathfinderInfo.m_verticalBitmalSize];

            m_gridMapPathfinderInfo.m_pathFinderGridmap
                 = new PathFinderNode[m_gridMapPathfinderInfo.m_gridMapHorizontalSize
                                     * m_gridMapPathfinderInfo.m_gridMapVerticalSize];
        }
    

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlockAt(uint x, uint y)
        {
            Assert.IsTrue(x < m_gridMapPathfinderInfo.m_gridMapHorizontalSize);
            Assert.IsTrue(y < m_gridMapPathfinderInfo.m_gridMapVerticalSize);

            const ulong BIT_BASE = 1ul << 63;

            uint arrayXIdx = x / 64u;
            byte bitmapXIdx = (byte)(x % 64u);
            ulong horizontalBitFlag = BIT_BASE >> bitmapXIdx;
            m_gridMapPathfinderInfo.m_horizontalBitmap
                [m_gridMapPathfinderInfo.m_gridMapHorizontalSize/64 * y + arrayXIdx]
                |= horizontalBitFlag;

            uint arrayYIdx = y / 64u;
            byte bitmapYIdx = (byte)(y % 64u);
            ulong verticalBitFlag = BIT_BASE >> bitmapYIdx;
            m_gridMapPathfinderInfo.m_verticalBitmap
                [m_gridMapPathfinderInfo.m_gridMapHorizontalSize * arrayYIdx + x]
                |= verticalBitFlag;
        } // TODO: SetNonBlockAt ÀÌ¶û GetBlockAt ¸¸µé±â


    }
}