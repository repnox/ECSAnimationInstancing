using UnityEngine;

namespace Transcendence.AnimationInstancing
{
    public class MeshAndMaterial
    {
        public Mesh mesh;
        public Material material;

        protected bool Equals(MeshAndMaterial other)
        {
            return Equals(mesh, other.mesh) && Equals(material, other.material);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MeshAndMaterial) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((mesh != null ? mesh.GetHashCode() : 0) * 397) ^ (material != null ? material.GetHashCode() : 0);
            }
        }
    }
}