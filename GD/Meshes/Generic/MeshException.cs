namespace GD.Meshes.Generic
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class MeshException : Exception
    {
        public MeshException()
        {
        }

        public MeshException(string message) : base(message)
        {
        }

        protected MeshException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MeshException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

