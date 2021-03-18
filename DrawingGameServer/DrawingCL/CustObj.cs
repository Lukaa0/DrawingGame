using System;

namespace DrawingCL
{
    [Serializable]
    public class CustObj
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public bool IsJoining { get; set; }
        public byte[] MotionEventBytes{ get; set; }

    }
}
