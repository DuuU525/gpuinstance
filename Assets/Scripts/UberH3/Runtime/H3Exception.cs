using System;
using UnityEngine.Scripting;

namespace H3Unity
{
    [Preserve]
    public class H3Exception : Exception
    {
        public int ErrorCode { get; }

        public H3Exception(string message, int errorCode = -1)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public H3Exception(string message, Exception inner, int errorCode = -1)
            : base(message, inner)
        {
            ErrorCode = errorCode;
        }

        public override string ToString()
        {
            return $"H3Exception (Code {ErrorCode}): {Message}";
        }
    }
}
