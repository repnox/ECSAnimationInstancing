using Unity.Mathematics;
using UnityEngine;

namespace Transcendence.AnimationInstancing
{
    [ExecuteAlways]
    public class AnimatedObjectPreview : MonoBehaviour
    {
    

        private void Update()
        {
            var proxy = GetComponent<AnimationInstanceProxy>();
            var materialPropertyBlock = new MaterialPropertyBlock();
            materialPropertyBlock.SetVector("_Anim", new Vector4(0, 0, proxy.animationSpeed, 1));
            materialPropertyBlock.SetFloat("_AnimPhase", proxy.animationPhase);
            Matrix4x4 transformation = transform.localToWorldMatrix * Matrix4x4.Rotate(
                                           quaternion.Euler(proxy.meshRotation*Mathf.Deg2Rad, math.RotationOrder.XYZ)
                                        );
            Graphics.DrawMesh(proxy.mesh, transformation, proxy.material, 0, Camera.current, 0, materialPropertyBlock);

        }
    }
}