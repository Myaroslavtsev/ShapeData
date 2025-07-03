/// describes all part replication parameters for ShapeReplicator class

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
        FixLength,
        Stretch
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

            FillReplicationParams();
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

        public bool GetReplicationParam(string paramName, out float parameter)
        {
            if (!ReplicationParams.ContainsKey(paramName))
            {
                parameter = 0;
                return false;
            }

            parameter = (float)ReplicationParams[paramName];
            return true;
        }

        private void FillReplicationParams()
        {
            foreach (var paramName in GetReplicationParamNames())
            {
                if (!ReplicationParams.ContainsKey(paramName))
                    ReplicationParams.Add(paramName, 0);
            }
        }

        private IEnumerable<string> GetReplicationParamNames()
        {
            if (ReplicationMethod == PartReplicationMethod.ByFixedIntervals ||
                ReplicationMethod == PartReplicationMethod.ByEvenIntervals)
                yield return "IntervalLength";

            if (ReplicationMethod == PartReplicationMethod.ByDeflection)
                yield return "MaxDeflection";

            if (ReplicationMethod == PartReplicationMethod.ByFixedIntervals ||
                ReplicationMethod == PartReplicationMethod.ByEvenIntervals ||
                ReplicationMethod == PartReplicationMethod.ByDeflection)
            {
                yield return "OriginalLength";
                yield return "InitialShift";
                yield return "SubdivisionCount";
            }
        }
    }
}
