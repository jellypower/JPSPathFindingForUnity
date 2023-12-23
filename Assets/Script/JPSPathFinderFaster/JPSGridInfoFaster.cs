//#define GET_HORIZONTAL_BLOCK_INFO
#define GET_VERTICAL_BLOCK_INFO

using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;


namespace DSNavigation
{
    public struct PathFinderNode
    {
        public float m_gCost;
        public bool m_onCloseList;
        public Vector2Int m_paretnNode;
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
        [SerializeField] Vector2 m_InGridStartPoint;
        [SerializeField] Vector2 m_InGridEndPoint;
        [SerializeField] uint m_InGridHorizontalSize;
        [SerializeField] uint m_InGridVerticalSize;


        [Header("Bake Info")]
        [SerializeField] Vector2 m_collisionCheckSensorSize = new Vector2(1, 1);
        [SerializeField] LayerMask m_InLayerToCheckCollide;

        private Vector2 m_gridStartCalculated;
        private Vector2 m_gridEndCalculated; 
        private Vector2 m_eachNodesize;

        public Vector2 CollisionCheckSensorSize { get { return m_collisionCheckSensorSize; } }
        public LayerMask LayerToCheckCollide { get { return m_InLayerToCheckCollide; } }
        
        void Awake()
        {

            Assert.IsTrue(m_InGridHorizontalSize > 0 );
            Assert.IsTrue(m_InGridVerticalSize > 0 );
            Assert.IsTrue(m_InGridEndPoint.x > m_InGridStartPoint.x);
            Assert.IsTrue(m_InGridEndPoint.y > m_InGridStartPoint.y);


            m_gridMapPathfinderInfo = new JPSGridInfoToFindPath();

            m_gridMapPathfinderInfo.m_gridMapHorizontalSize
                = (m_InGridHorizontalSize / 64 +
                (m_InGridHorizontalSize % 64 == 0 ? 0u : 1u)) * 64;

            m_gridMapPathfinderInfo.m_gridMapVerticalSize
                = (m_InGridVerticalSize / 64 +
                (m_InGridVerticalSize % 64 == 0 ? 0u : 1u)) * 64;
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

            for(uint y = 0; y < m_gridMapPathfinderInfo.m_gridMapVerticalSize; y++)
            {
                for(uint x=0; x<m_gridMapPathfinderInfo.m_gridMapHorizontalSize; x++)
                {
                    m_gridMapPathfinderInfo.m_pathFinderGridmap
                        [m_gridMapPathfinderInfo.m_gridMapHorizontalSize * y + x].m_paretnNode = new Vector2Int(-1, -1);

                    m_gridMapPathfinderInfo.m_pathFinderGridmap
                        [m_gridMapPathfinderInfo.m_gridMapHorizontalSize * y + x].m_onCloseList = false;

                    m_gridMapPathfinderInfo.m_pathFinderGridmap
                        [m_gridMapPathfinderInfo.m_gridMapHorizontalSize * y + x].m_gCost = float.MaxValue;
                }
            }


            Vector2 girdMapWidthHeight = m_InGridEndPoint - m_InGridStartPoint;
            m_eachNodesize =
                new Vector2(girdMapWidthHeight.x / m_InGridHorizontalSize, girdMapWidthHeight.y / m_InGridVerticalSize);

            m_gridStartCalculated = m_InGridStartPoint;
            m_gridEndCalculated = m_InGridEndPoint;


            for (uint x = 0; x < m_gridMapPathfinderInfo.m_gridMapHorizontalSize; x++)
            {
                SetBlockAt(x, 0);
                SetBlockAt(x, m_gridMapPathfinderInfo.m_gridMapVerticalSize - 1);
            }

            for (uint y = 1; y < m_gridMapPathfinderInfo.m_gridMapVerticalSize - 1; y++)
            {
                SetBlockAt(0, y);
                SetBlockAt(m_gridMapPathfinderInfo.m_gridMapHorizontalSize - 1, y);
            }
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
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetNonBlockAt(uint x, uint y)
        {
            Assert.IsTrue(x < m_gridMapPathfinderInfo.m_gridMapHorizontalSize);
            Assert.IsTrue(y < m_gridMapPathfinderInfo.m_gridMapVerticalSize);

            const ulong BIT_BASE = 1ul << 63;

            uint arrayXIdx = x / 64u;
            byte bitmapXIdx = (byte)(x % 64u);
            ulong horizontalBitFlag = BIT_BASE >> bitmapXIdx;
            m_gridMapPathfinderInfo.m_horizontalBitmap
                [m_gridMapPathfinderInfo.m_gridMapHorizontalSize / 64 * y + arrayXIdx]
                &= ~horizontalBitFlag;

            uint arrayYIdx = y / 64u;
            byte bitmapYIdx = (byte)(y % 64u);
            ulong verticalBitFlag = BIT_BASE >> bitmapYIdx;
            m_gridMapPathfinderInfo.m_verticalBitmap
                [m_gridMapPathfinderInfo.m_gridMapHorizontalSize * arrayYIdx + x]
                &= ~verticalBitFlag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetBlockAt(uint x, uint y)
        {
            Assert.IsTrue(x < m_gridMapPathfinderInfo.m_gridMapHorizontalSize);
            Assert.IsTrue(y < m_gridMapPathfinderInfo.m_gridMapVerticalSize);

            const ulong BIT_BASE = 1ul << 63;

#if GET_HORIZONTAL_BLOCK_INFO
            uint arrayXIdx = x / 64u;
            byte bitmapXIdx = (byte)(x % 64u);
            ulong horizontalBitFlag = BIT_BASE >> bitmapXIdx;

            return 
            (
            m_gridMapPathfinderInfo.m_horizontalBitmap[m_gridMapPathfinderInfo.m_gridMapHorizontalSize / 64 * y + arrayXIdx]
               & horizontalBitFlag
               ) != 0;
#elif GET_VERTICAL_BLOCK_INFO
            uint arrayYIdx = y / 64u;
            byte bitmapYIdx = (byte)(y % 64u);
            ulong verticalBitFlag = BIT_BASE >> bitmapYIdx;

            return
            (
                m_gridMapPathfinderInfo.m_verticalBitmap[m_gridMapPathfinderInfo.m_gridMapHorizontalSize * arrayYIdx + x]
                & verticalBitFlag
                ) != 0;
#endif
        }

        /// <returns> Vector2.negativeInfinity if invalid location input is passed </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 GetNodeCenter(uint x, uint y)
        {
            if (x<0 || x>=m_gridMapPathfinderInfo.m_gridMapHorizontalSize ||
                y<0 || y>=m_gridMapPathfinderInfo.m_gridMapVerticalSize)
            {
                Debug.LogError($"In location is Invalid. Location: (x: {x}, y: {y})");
                return Vector2.negativeInfinity;
            }
            Assert.IsTrue(0<=x && x<m_gridMapPathfinderInfo.m_gridMapHorizontalSize);
            Assert.IsTrue(0<=y && y<m_gridMapPathfinderInfo.m_gridMapVerticalSize);

            return new Vector2(m_gridStartCalculated.x + m_eachNodesize.x / 2 + x * m_eachNodesize.x,
                                m_gridStartCalculated.y + m_eachNodesize.y / 2 + y * m_eachNodesize.y);
        }

        /// <returns> (-1, -1) if invalid location input is passed </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2Int GetNodeIdx(in Vector2 location)
        {
            if( location.x < m_gridStartCalculated.x || location.x >= m_gridEndCalculated.x ||
                location.y < m_gridStartCalculated.y || location.y >= m_gridEndCalculated.y)
            {
                Debug.LogError($"In location is Invalid. (vector: {location})");
                return new Vector2Int(-1, -1);
            }

            Vector2 relativeLocation = location - m_gridStartCalculated;

            return new Vector2Int(
                (int)(relativeLocation.x / m_eachNodesize.x),
                (int)(relativeLocation.y / m_eachNodesize.y));
        }

        public void BakeGridInfo()
        {
            for (uint y = 0; y < m_gridMapPathfinderInfo.m_gridMapVerticalSize; y++)
            {
                for (uint x = 0; x < m_gridMapPathfinderInfo.m_gridMapHorizontalSize; x++)
                {
                    Vector2 center = GetNodeCenter(x, y);
                    bool bBlock = Physics2D.OverlapArea
                        (center-m_collisionCheckSensorSize/2, center+m_collisionCheckSensorSize/2, m_InLayerToCheckCollide);

                    if (bBlock)
                        SetBlockAt(x,y);
                }
            }
        }

#if DEBUG
        [Header("Debug Info")]
        [SerializeField]bool DrawGizmo = false;
        private void OnDrawGizmos()
        {
            if (EditorApplication.isPlaying && DrawGizmo)
            {
                DrawGrid();
                DrawBlockNodes(new Color(1, 0, 0, 0.5f));
            }   
        }

        void DrawGrid()
        {
            Gizmos.color = Color.gray;
            Vector2 gridMapSize = m_InGridEndPoint - m_InGridStartPoint;
            Vector2 eachNodeSize = new Vector2(gridMapSize.x / m_InGridHorizontalSize, gridMapSize.y / m_InGridVerticalSize);

            for(uint x = 0;x <= m_gridMapPathfinderInfo.m_gridMapHorizontalSize; x++)
            {
                Vector2 from = new Vector3(m_gridStartCalculated.x + eachNodeSize.x * x, m_gridStartCalculated.y);
                Vector2 to = new Vector3(m_gridStartCalculated.x + eachNodeSize.x * x, m_gridEndCalculated.y);
                Gizmos.DrawLine(from, to);
            }

            for(uint y = 0; y <= m_gridMapPathfinderInfo.m_gridMapVerticalSize; y++)
            {
                Vector2 from = new Vector3(m_gridStartCalculated.x, m_gridStartCalculated.y + eachNodeSize.y * y);
                Vector2 to = new Vector3(m_gridEndCalculated.x, m_gridStartCalculated.y + eachNodeSize.y * y);
                Gizmos.DrawLine(from, to);
            }
        }

        void DrawBlockNodes(Color InColor)
        {
            Color originalColor = Gizmos.color;
            Gizmos.color = InColor;

            for(uint y = 0;y<m_gridMapPathfinderInfo.m_gridMapVerticalSize;y++)
            {
                for(uint x = 0; x < m_gridMapPathfinderInfo.m_gridMapHorizontalSize; x++)
                {
                    if(GetBlockAt(x, y))
                        Gizmos.DrawCube(GetNodeCenter(x, y), m_eachNodesize);
                }
            }

            Gizmos.color = originalColor;
        }
#endif
    }
}