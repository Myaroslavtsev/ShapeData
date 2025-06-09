using System.Collections.Generic;
using System.Linq;

namespace ShapeData
{
    public enum PartReplicationMethod
    {
        NoReplication,
        AtFixedPos,
        AtTheEnd,
        ByFixedIntervals,
        ByEvenIntervals,
        ByDeflection
    }

    public enum PartScalingMethod
    {
        FixLengthOnly,
        FixLengthAndCut,
        Scale
    }

    public enum PartStretchInWidthMethod
    {
        ReplicateAlongAllTracks,
        ReplicateAlongLeftTrack,
        ReplicateAlongRightTrack,
        StretchInWidth
    }

    public class PartReplication
    {
        public PartReplicationMethod ReplicationMethod { get; set; }

        public PartStretchInWidthMethod StretchInWidthMethod { get; set; }

        public PartScalingMethod ScalingMethod { get; set; }        

        public bool ScaleTexture { get; set; }

        public bool BendPart { get; set; }

        public bool LeaveAtLeastOne { get; set; }

        public Dictionary<string, float> ReplicationParams { get; private set; }

        public IEnumerable<(string Name, float Value)> GetReplicationParams()
        {            
            foreach(var paramName in GetReplicationParamNames())
                yield return (paramName, ReplicationParams[paramName]);            
        }

        private PartReplication (PartReplicationMethod replicationMethod)
        {
            ReplicationMethod = replicationMethod;
        }

        public PartReplication(PartReplicationMethod replicationMethod,
            PartScalingMethod scalingMethod,
            PartStretchInWidthMethod stretchInWidthMethod,
            bool scaleTexture,
            bool bendPart,
            bool leaveAtLeastOne,
            Dictionary<string, float> replicationParams = null)
        {
            ReplicationMethod = replicationMethod;
            ScalingMethod = scalingMethod;
            StretchInWidthMethod = stretchInWidthMethod;
            ScaleTexture = scaleTexture;
            BendPart = bendPart;
            LeaveAtLeastOne = leaveAtLeastOne;

            if (replicationParams == null)
                ReplicationParams = new Dictionary<string, float>();
            else
                ReplicationParams = replicationParams;
        }

        public static PartReplication NoReplication()
        {
            return new PartReplication(PartReplicationMethod.NoReplication);
        }

        public void SetReplicationParams(Dictionary<string, float> replicationParams)
        {
            ReplicationParams = replicationParams;
        }

        public int ReplicationParamCount()
        {
            int count = 0;
            _ = GetReplicationParamNames().Select(rpn => count++);
            return count;
        }

        private IEnumerable<string> GetReplicationParamNames()
        {
            if (ReplicationMethod == PartReplicationMethod.ByEvenIntervals ||
                ReplicationMethod == PartReplicationMethod.ByEvenIntervals)
                yield return "MinLength";

            if (ReplicationMethod == PartReplicationMethod.ByDeflection)
                yield return "MaxDeflection";

            if (ReplicationMethod == PartReplicationMethod.ByEvenIntervals ||
                ReplicationMethod == PartReplicationMethod.ByEvenIntervals ||
                ReplicationMethod == PartReplicationMethod.ByDeflection)
                yield return "OriginalLength";
            //yield return "InitialShift";
        }
    }
}
