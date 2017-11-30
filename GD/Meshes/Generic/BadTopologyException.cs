namespace GD.Meshes.Generic
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class BadTopologyException : MeshException
    {
        public BadTopologyException()
        {
        }

        public BadTopologyException(string message) : base(message)
        {
        }

        protected BadTopologyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public BadTopologyException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

