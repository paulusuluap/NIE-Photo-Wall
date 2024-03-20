using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace sunny
{
    public class PhotoWallMosaic : MonoBehaviour
    {
        private struct Index2D
        {
            public int x;
            public int y;
        }
        public enum SpawnOrder
        {
            UpToDown,
            DownToUp,
            LeftToRight,
            RightToLeft,
            Random
        }
        public SpawnOrder spawnOrder;
        private class SortItem
        {
            public Color color;
            public PhotoWallDataset.TextureData data;
            public float distance;
            public Vector2[] uv;
            public Material material;
            public int useCount;
        }
        public Material material;
        public float spawnPerSecond = 1000;
        public Vector2 gridSize = Vector2.one;
        [Range(8, 80)]
        public int columns = 10;
        [Range(8, 80)]
        public int rows = 10;
        private GameObject[] children;
        [Range(0, 1)]
        public float colorCheat = .5f;
        [Range(0, 1)]
        public float photoCheat = .5f;
        [Range(0, 1)]
        public float tryAvoidRepeat = .1f;
        public PhotoWallDataset defaultDataSet;

        List<SortItem> sortList = new List<SortItem>();
        public bool usePool;
        Stack<GameObject> pool = new Stack<GameObject>();
        Coroutine currentWork;

        public delegate void SpawnEventHandler(GameObject piece, PhotoWallDataset.TextureData data);
        public event SpawnEventHandler onSpawn;

        public void generate(Texture2D target, PhotoWallDataset dataSet = null)
        {
            if (dataSet == null) dataSet = defaultDataSet;
            if (currentWork != null) StopCoroutine(currentWork);
            currentWork = StartCoroutine(doGenerate(target, dataSet));
        }
        public IEnumerator doGenerate(Texture2D target, PhotoWallDataset dataSet)
        {
            float timer = Time.realtimeSinceStartup;
            sortList.Clear();
            float texRatio = target.width / (float)target.height;
            int minSize;
            Vector2 gridUVSize = new Vector2();
            float resultRatio = (gridSize.x * columns) / (float)(gridSize.y * rows);
            if (texRatio > resultRatio)
            {
                minSize = target.height;
                gridUVSize.x = resultRatio / texRatio;
                gridUVSize.y = 1;

            }
            else
            {
                minSize = target.width;
                gridUVSize.x = 1;
                gridUVSize.y = texRatio / resultRatio;
            }
            Rect uvRect = new Rect();
            
            uvRect.xMin = .5f - gridUVSize.x / 2;
            uvRect.xMax = .5f + gridUVSize.x / 2;
            uvRect.yMin = .5f - gridUVSize.y / 2;
            uvRect.yMax = .5f + gridUVSize.y / 2;

            foreach (PhotoWallDataset.TextureData texData in dataSet.textures)
            {
                Material mat = Instantiate(material) as Material;
                mat.SetTexture("_MainTex", texData.texture);
                mat.SetTexture("_MainTex2", target);
                mat.SetFloat("_ColorCheat", colorCheat);
                mat.SetFloat("_PhotoCheat", photoCheat);

                Vector2[] uv = new Vector2[]{ //flipped
					new Vector2(texData.uvRect.xMax,texData.uvRect.yMin),
                    new Vector2(texData.uvRect.xMin,texData.uvRect.yMin),
                    new Vector2(texData.uvRect.xMax,texData.uvRect.yMax),
                    new Vector2(texData.uvRect.xMin,texData.uvRect.yMax)
                };
                sortList.Add(new SortItem()
                {
                    color = texData.color,
                    data = texData,
                    material = mat,
                    uv = uv
                });
            }
            if (children != null)
            {
                if (usePool)
                {
                    foreach (GameObject gObj in children)
                    {
                        if (gObj)
                        {
                            gObj.SetActive(false);
                            pool.Push(gObj);
                        }
                    }
                }
                else
                {
                    destroyPieces();
                }

            }
            children = new GameObject[columns * rows];
            //Vector2 gridDistance = new Vector2();
            Vector3[] vertices = new Vector3[]{
                new Vector3(-gridSize.x/2,-gridSize.y/2,0),
                new Vector3(gridSize.x/2,-gridSize.y/2,0),
                new Vector3(-gridSize.x/2,gridSize.y/2,0),
                new Vector3(gridSize.x/2,gridSize.y/2,0)
            };
            int[] triangles = new int[]{
                0,1,2,
                1,3,2
            };
            yield return null;


            Index2D[] orderList = new Index2D[columns * rows];
            int index = 0;
            if (spawnOrder == SpawnOrder.RightToLeft || spawnOrder == SpawnOrder.Random)
            {
                for (int i = 0; i < columns; i++)
                {
                    for (int j = 0; j < rows; j++)
                    {
                        orderList[index].x = i;
                        orderList[index].y = j;
                        index++;
                    }
                }
                if (spawnOrder == SpawnOrder.Random)
                {
                    for (index = 0; index < orderList.Length; index++)
                    {
                        int rnd = Random.Range(0, orderList.Length);
                        Index2D m = orderList[rnd];
                        orderList[rnd] = orderList[index];
                        orderList[index] = m;
                    }
                }
            }
            else if (spawnOrder == SpawnOrder.LeftToRight)
            {

                for (int i = columns - 1; i >= 0; i--)
                {
                    for (int j = 0; j < rows; j++)
                    {
                        orderList[index].x = i;
                        orderList[index].y = j;
                        index++;
                    }
                }
            }
            else if (spawnOrder == SpawnOrder.DownToUp)
            {
                for (int j = 0; j < rows; j++)
                {
                    for (int i = 0; i < columns; i++)
                    {
                        orderList[index].x = i;
                        orderList[index].y = j;
                        index++;
                    }
                }
            }
            else if (spawnOrder == SpawnOrder.UpToDown)
            {
                for (int j = rows - 1; j >= 0; j--)
                {
                    for (int i = 0; i < columns; i++)
                    {
                        orderList[index].x = i;
                        orderList[index].y = j;
                        index++;
                    }
                }
            }


            float spawnTimer = spawnPerSecond * Time.deltaTime;

            for (index = 0; index < orderList.Length; index++)
            {


                int i = orderList[index].x;
                int j = orderList[index].y;

                Color[] colors = new Color[]{//flipped
						target.GetPixelBilinear(Mathf.Lerp(uvRect.xMin,uvRect.xMax,1-(i+1-.3f)/columns),
                            Mathf.Lerp(uvRect.yMin,uvRect.yMax,(j-.3f)/rows)),
                        target.GetPixelBilinear(Mathf.Lerp(uvRect.xMin,uvRect.xMax,1-(i+1+.3f)/columns),
                            Mathf.Lerp(uvRect.yMin,uvRect.yMax,(j-.3f)/rows)),
                        target.GetPixelBilinear(Mathf.Lerp(uvRect.xMin,uvRect.xMax,1-(i+1+.3f)/columns),
                            Mathf.Lerp(uvRect.yMin,uvRect.yMax,(j+.3f)/rows)),
                        target.GetPixelBilinear(Mathf.Lerp(uvRect.xMin,uvRect.xMax,1-(i+1-.3f)/columns),
                            Mathf.Lerp(uvRect.yMin,uvRect.yMax,(j+.3f)/rows))
                    };
                colors[0].a = colors[1].a = colors[2].a = colors[3].a = .5f;
                Color color = (colors[0] + colors[1] + colors[2] + colors[3]) / 4;
                SortItem selected = null;
                float disanceCmp = 999;
                for (int k = 0; k < sortList.Count; k++)
                {
                    SortItem item = sortList[k];
                    Color c = item.color;
                    item.distance = Vector3.Distance(
                        new Vector3(c.r, c.g, c.b),
                        new Vector3(color.r, color.g, color.b)
                    ) + item.useCount * tryAvoidRepeat;
                    if (item.distance < disanceCmp)
                    {
                        selected = item;
                        disanceCmp = item.distance;
                    }
                }


                GameObject gObj = null;
                MeshRenderer mRenderer = null;
                MeshFilter mFilter = null;

                Mesh mesh = null;
                if (usePool && pool.Count > 0)
                {
                    gObj = pool.Pop();
                    gObj.SetActive(true);
                    mRenderer = gObj.GetComponent<MeshRenderer>();
                    mFilter = gObj.GetComponent<MeshFilter>();
                    mesh = mFilter.sharedMesh;
                }
                if (gObj == null)
                {
                    gObj = new GameObject();
                    mRenderer = gObj.AddComponent<MeshRenderer>();
                    mFilter = gObj.AddComponent<MeshFilter>();
                    mesh = new Mesh();
                    mesh.vertices = vertices;
                    mesh.triangles = triangles;
                    mesh.uv = selected.uv;
                }
                gObj.name = i + "_" + j;
                children[i * rows + j] = gObj;
                gObj.transform.parent = transform;
                gObj.transform.localPosition = new Vector3(
                    gridSize.x * (i + .5f - columns / 2f),
                    gridSize.y * (j + .5f - rows / 2f)
                );

                selected.useCount++;

                mRenderer.sharedMaterial = selected.material;
                mesh.uv2 = new Vector2[]{
                        new Vector2(
                            Mathf.Lerp(uvRect.xMin,uvRect.xMax,1-(i+0)/(float)columns),
                            Mathf.Lerp(uvRect.yMin,uvRect.yMax,(j+0)/(float)rows)),
                        new Vector2(
                            Mathf.Lerp(uvRect.xMin,uvRect.xMax,1-(i+1)/(float)columns),
                            Mathf.Lerp(uvRect.yMin,uvRect.yMax,(j+0)/(float)rows)),
                        new Vector2(
                            Mathf.Lerp(uvRect.xMin,uvRect.xMax,1-(i+0)/(float)columns),
                            Mathf.Lerp(uvRect.yMin,uvRect.yMax,(j+1)/(float)rows)),
                        new Vector2(
                            Mathf.Lerp(uvRect.xMin,uvRect.xMax,1-(i+1)/(float)columns),
                            Mathf.Lerp(uvRect.yMin,uvRect.yMax,(j+1)/(float)rows))
                    };
                mesh.colors = colors;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                mFilter.sharedMesh = mesh;
                if (onSpawn != null) onSpawn(gObj, selected.data);
                spawnTimer--;
                while (spawnTimer < 1)
                {
                    yield return null;
                    spawnTimer += spawnPerSecond * Time.deltaTime;
                }
            }

        }


        void Update()
        {
            foreach (SortItem item in sortList)
            {
                item.material.SetFloat("_ColorCheat", colorCheat);
                item.material.SetFloat("_PhotoCheat", photoCheat);
            }
        }
        void OnDrawGizmos()
        {
            Vector3 cornerTL = transform.TransformPoint(new Vector3(-gridSize.x * columns / 2, gridSize.y * rows / 2));
            Vector3 cornerTR = transform.TransformPoint(new Vector3(gridSize.x * columns / 2, gridSize.y * rows / 2));
            Vector3 cornerBL = transform.TransformPoint(new Vector3(-gridSize.x * columns / 2, -gridSize.y * rows / 2));
            Vector3 cornerBR = transform.TransformPoint(new Vector3(gridSize.x * columns / 2, -gridSize.y * rows / 2));

            Vector3 cornerSTL = transform.TransformPoint(new Vector3(-gridSize.x * columns / 2, gridSize.y * rows / 2) +
                new Vector3());
            Vector3 cornerSTR = transform.TransformPoint(new Vector3(-gridSize.x * columns / 2, gridSize.y * rows / 2) +
                new Vector3(0, -gridSize.y));
            Vector3 cornerSBL = transform.TransformPoint(new Vector3(-gridSize.x * columns / 2, gridSize.y * rows / 2) +
                new Vector3(gridSize.x, 0));
            Vector3 cornerSBR = transform.TransformPoint(new Vector3(-gridSize.x * columns / 2, gridSize.y * rows / 2) +
                new Vector3(gridSize.x, -gridSize.y));
            Gizmos.DrawLine(cornerTL, cornerTR);
            Gizmos.DrawLine(cornerTL, cornerBL);
            Gizmos.DrawLine(cornerBR, cornerTR);
            Gizmos.DrawLine(cornerBR, cornerBL);
            Gizmos.DrawLine(cornerSTL, cornerSTR);
            Gizmos.DrawLine(cornerSTL, cornerSBL);
            Gizmos.DrawLine(cornerSBR, cornerSTR);
            Gizmos.DrawLine(cornerSBR, cornerSBL);
        }
        void destroyPieces()
        {
            if(children.Length > 0)
                foreach (GameObject gObj in children)
                {
                    if (gObj)
                    {
                        Destroy(gObj.GetComponent<MeshFilter>().sharedMesh);
                        Destroy(gObj);
                    }
                }
        }
        void OnDestroy()
        {
            destroyPieces();
            for (int k = 0; k < sortList.Count; k++)
            {
                SortItem item = sortList[k];
                Destroy(item.material);
                item.data = null;
                item.uv = null;
            }
            sortList = null;
        }
    }
}
