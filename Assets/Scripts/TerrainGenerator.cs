using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public Terrain terrain;

    [Header("Perlin Noise")]
    public float perlinScale = 0.05f;
    public float perlinHeight = 0.1f;

    [Header("Voronoi")]
    public int voronoiPoints = 4;
    public float voronoiHeight = 0.4f;
    public float falloff = 1f;

    [Header("Texture Settings")]
    public float snowStart = 0.3f;
    public float snowEnd = 0.6f;

    private TerrainData terrainData;

    // 🔥 QUAN TRỌNG: lưu height thật
    private float[,] generatedHeights;

    [Header("Tree Settings")]
    public GameObject[] treePrefabs;
    public int treeCount = 500;
    public float minHeight = 0.1f;
    public float maxHeight = 0.4f;
    public float maxSlope = 30f;

    void Start()
    {
        terrainData = terrain.terrainData;
        GenerateTerrain();
        ApplyTexture();
        GenerateTrees();
    }

    void GenerateTerrain()
    {
        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;

        float[,] heights = new float[width, height];

        // 🌊 PERLIN
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                float xCoord = x * perlinScale;
                float zCoord = z * perlinScale;

                float perlin = Mathf.PerlinNoise(xCoord, zCoord);
                heights[x, z] = perlin * perlinHeight;
            }
        }

        // ⛰️ VORONOI
        for (int i = 0; i < voronoiPoints; i++)
        {
            int peakX = Random.Range(0, width);
            int peakZ = Random.Range(0, height);
            float peakHeight = Random.Range(0.3f, 1f) * voronoiHeight;

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    float dist = Vector2.Distance(
                        new Vector2(x, z),
                        new Vector2(peakX, peakZ)
                    );

                    float h = peakHeight - Mathf.Pow(dist / width, falloff);

                    if (h > heights[x, z])
                    {
                        heights[x, z] = h;
                    }
                }
            }
        }

        NormalizeHeights(heights);

        // 🔥 LƯU LẠI HEIGHT CHUẨN
        generatedHeights = heights;

        terrainData.SetHeights(0, 0, heights);
    }

    void NormalizeHeights(float[,] heights)
    {
        int w = heights.GetLength(0);
        int h = heights.GetLength(1);

        for (int x = 0; x < w; x++)
        {
            for (int z = 0; z < h; z++)
            {
                heights[x, z] = Mathf.Clamp01(heights[x, z]);
            }
        }
    }

    void ApplyTexture()
    {
        TerrainData data = terrain.terrainData;

        int width = data.alphamapWidth;
        int height = data.alphamapHeight;

        float[,,] alphamaps = new float[width, height, 2];

        int hmWidth = generatedHeights.GetLength(0);
        int hmHeight = generatedHeights.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                int hx = Mathf.Clamp((int)((float)x / width * hmWidth), 0, hmWidth - 1);
                int hz = Mathf.Clamp((int)((float)z / height * hmHeight), 0, hmHeight - 1);

                float heightValue = generatedHeights[hx, hz];

                // ❄️ snow đúng theo đỉnh
                float snow = Mathf.InverseLerp(snowStart, snowEnd, heightValue);
                float grass = 1f - snow;

                alphamaps[x, z, 0] = grass;
                alphamaps[x, z, 1] = snow;
            }
        }

        data.SetAlphamaps(0, 0, alphamaps);
    }
    void GenerateTrees()
    {
        TerrainData data = terrain.terrainData;

        int gridSize = 25; // càng lớn → càng nhiều cây

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                // 🎯 random nhẹ trong mỗi ô (để không bị cứng)
                float offsetX = Random.value;
                float offsetZ = Random.value;

                float normX = (x + offsetX) / gridSize;
                float normZ = (z + offsetZ) / gridSize;

                // 🌍 chuyển sang world
                float worldX = normX * data.size.x;
                float worldZ = normZ * data.size.z;
                float worldY = terrain.SampleHeight(new Vector3(worldX, 0, worldZ));

                GameObject tree = Instantiate(
                    treePrefabs[Random.Range(0, treePrefabs.Length)],
                    new Vector3(worldX, worldY, worldZ),
                    Quaternion.identity
                );

                tree.transform.localScale *= Random.Range(0.8f, 1.2f);
            }
        }
    }
}