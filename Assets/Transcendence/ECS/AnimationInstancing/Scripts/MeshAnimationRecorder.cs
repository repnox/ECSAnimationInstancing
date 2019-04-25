using System.Collections.Generic;
using System.Runtime.InteropServices;
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

        public int maxTextureDimension = 4096;

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
                ExportAssets();

                _exported = true;
            }
        }

        private void ExportAssets()
        {
            if (_animation.Count > 0)
            {
                int numVertices = _animation[0].Length;
                int numAnimationFrames = _animation.Count + 1;
                
                int textureWidth;
                int textureHeight;
                if (numVertices > maxTextureDimension)
                {
                    textureHeight = maxTextureDimension;
                    textureWidth = numAnimationFrames * (numVertices / maxTextureDimension + (numVertices % maxTextureDimension > 0 ? 1 : 0));
                }
                else
                {
                    textureHeight = numVertices;
                    textureWidth = numAnimationFrames;
                }
                
                Texture2D texture2D =
                    new Texture2D(textureWidth, textureHeight, TextureFormat.RGBAFloat, false);

                int frameIndex = 0;
                int vertexIndex;
                int track;
                foreach (Vector3[] vertices in _animation)
                {
                    vertexIndex = 0;
                    track = 0;
                    foreach (Vector3 vertex in vertices)
                    {
                        var xPos = frameIndex + track*numAnimationFrames;
                        var color = new Color(vertex.x, vertex.y, vertex.z);
                        texture2D.SetPixel(xPos, vertexIndex, color);
                        if (frameIndex == 0)
                        {
                            texture2D.SetPixel(xPos + numAnimationFrames - 1, vertexIndex, color);
                        }
                        vertexIndex++;
                        if (vertexIndex >= textureHeight)
                        {
                            vertexIndex = 0;
                            track++;
                        }
                    }

                    frameIndex++;
                }

                AssetDatabase.CreateAsset(texture2D,
                    "Assets/Generated/AnimationTextures/" + animationName + "-animtex.asset");

                if (generateNewMesh)
                {
                    Mesh mesh = copyMesh(_skinnedMeshRenderer.sharedMesh);

                    Dictionary<Vector3, Color> vertexPositions = new Dictionary<Vector3, Color>();

                    double xOffset = 1.0 / textureWidth / 2;
                    double yOffset = 1.0 / textureHeight / 2;
                    int column = 0;
                    int yPos = 0;
                    for (int i = 0; i < mesh.vertices.Length; i++)
                    {
                        if (yPos >= textureHeight)
                        {
                            column++;
                            yPos = 0;
                        }
                        var meshVertex = mesh.vertices[i];
                        if (!vertexPositions.ContainsKey(meshVertex))
                        {
                            double red = xOffset + (double)(column * numAnimationFrames) / textureWidth;
                            double green = yOffset + (double)yPos/ textureHeight;
                            
                            vertexPositions.Add(meshVertex, new Color((float)red, (float)green, 0));
                        }

                        yPos++;
                    }

                    Color[] newColors = new Color[mesh.vertices.Length];
                    for (int i = 0; i < mesh.vertices.Length; i++)
                    {
                        newColors[i] = vertexPositions[mesh.vertices[i]];
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