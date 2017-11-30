namespace GD.Meshes.Generic
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class MismatchedMeshException : MeshException
    {
        public MismatchedMeshException()
        {
        }

        public MismatchedMeshException(string message) : base(message)
        {
        }

        protected MismatchedMeshException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MismatchedMeshException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

