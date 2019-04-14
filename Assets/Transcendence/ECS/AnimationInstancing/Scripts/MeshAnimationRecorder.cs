using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Transcendence.AnimationInstancing
{
    public class MeshAnimationRecorder : MonoBehaviour
    {

        public string animationName;

        public int numberOfFrames;

        public float duration;

        public bool generateNewMesh;

        public Animator animator;

        private SkinnedMeshRenderer _skinnedMeshRenderer;

        private List<Vector3[]> _animation = new List<Vector3[]>();

        private int _currentFrame = 0;

        private float _nextFrameTime = 0;

        private float _nextFrameDelay = 0;

        private bool _exported = false;

        private void Awake()
        {
            _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            _nextFrameDelay = duration / numberOfFrames;
            _nextFrameTime = Time.time + _nextFrameDelay;
        }

        void LateUpdate()
        {
            if (animator.isInitialized && _currentFrame < numberOfFrames)
            {
                if (Time.time >= _nextFrameTime)
                {
                    Mesh mesh = new Mesh();
                    _skinnedMeshRenderer.BakeMesh(mesh);
                    _animation.Add(mesh.vertices);

                    _nextFrameTime = Time.time + _nextFrameDelay;
                    _currentFrame++;
                }
            }
            else if (!_exported)
            {
                ExportAsPng();

                _exported = true;
            }
        }

        private void ExportAsPng()
        {
            if (_animation.Count > 0)
            {
                Texture2D texture2D =
                    new Texture2D(_animation.Count, _animation[0].Length, TextureFormat.RGBAFloat, false);

                int frameIndex = 0;
                int vertexIndex = 0;
                foreach (Vector3[] vertices in _animation)
                {
                    vertexIndex = 0;
                    foreach (Vector3 vertex in vertices)
                    {
                        texture2D.SetPixel(frameIndex, vertexIndex, new Color(vertex.x, vertex.y, vertex.z));

                        vertexIndex++;
                    }

                    frameIndex++;
                }

                AssetDatabase.CreateAsset(texture2D,
                    "Assets/Generated/AnimationTextures/" + animationName + "-animtex.asset");

                if (generateNewMesh)
                {
                    Mesh mesh = copyMesh(_skinnedMeshRenderer.sharedMesh);

                    Dictionary<Vector3, int> vertexPositions = new Dictionary<Vector3, int>();

                    for (int i = 0; i < mesh.vertices.Length; i++)
                    {
                        var meshVertex = mesh.vertices[i];
                        if (!vertexPositions.ContainsKey(meshVertex))
                        {
                            vertexPositions.Add(meshVertex, i);
                        }
                    }

                    Color[] newColors = new Color[mesh.vertices.Length];
                    //Vector2[] newUv2 = new Vector2[mesh.vertices.Length];
                    float offset = 1f / mesh.vertices.Length / 2;
                    for (int i = 0; i < mesh.vertices.Length; i++)
                    {
                        float r = vertexPositions[mesh.vertices[i]];
                        var value = offset + r / mesh.vertices.Length;
                        //var value = Random.Range(0f,1f);
                        var vertexColor = new Color(value, 0, 0, 0);
                        //newUv2[i] = new Vector2(value, 0);
                        newColors[i] = vertexColor;
                    }

                    //mesh.uv2 = newUv2;
                    mesh.colors = newColors;

                    AssetDatabase.CreateAsset(mesh,
                        "Assets/Generated/AnimationTextures/" + animationName + "-mesh.asset");
                }
            }
        }

        private Mesh copyMesh(Mesh mesh)
        {
            Mesh newMesh = new Mesh();
            newMesh.vertices = mesh.vertices;
            newMesh.triangles = mesh.triangles;
            newMesh.uv = mesh.uv;
            newMesh.uv2 = mesh.uv2;
            newMesh.uv3 = mesh.uv3;
            newMesh.uv4 = mesh.uv4;
            newMesh.uv5 = mesh.uv5;
            newMesh.uv6 = mesh.uv6;
            newMesh.uv7 = mesh.uv7;
            newMesh.uv8 = mesh.uv8;
            newMesh.normals = mesh.normals;
            newMesh.tangents = mesh.tangents;
            newMesh.colors = mesh.colors;
            return newMesh;
        }
    }
}