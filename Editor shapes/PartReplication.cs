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
        FixLengthAndTrim,
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

        public bool PreserveTextureDimension { get; set; }

        public bool BendPart { get; set; }

        public bool LeaveAtLeastOne { get; set; }

        private Dictionary<string, float> ReplicationParams;      

        public IEnumerable<(string Name, float Value)> GetReplicationParams()
        {
            if (ReplicationParams is not null)
                foreach(var paramName in GetReplicationParamNames())
                    yield return (paramName.ToLower(), ReplicationParams[paramName.ToLower()]);            
        }

        private PartReplication (PartReplicationMethod replicationMethod)
        {
            ReplicationMethod = replicationMethod;
        }

        public PartReplication(PartReplicationMethod replicationMethod,
            PartScalingMethod scalingMethod,
            PartStretchInWidthMethod stretchInWidthMethod,
            bool preserveTextureDimension,
            bool bendPart,
            bool leaveAtLeastOne,
            Dictionary<string, float> replicationParams = null)
        {
            ReplicationMethod = replicationMethod;
            ScalingMethod = scalingMethod;
            StretchInWidthMethod = stretchInWidthMethod;
            PreserveTextureDimension = preserveTextureDimension;
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

        public void ReplaceReplicationParams(Dictionary<string, float> replicationParams)
        {
            ReplicationParams = replicationParams;
        }

        public bool GetReplicationParam(string paramName, out float parameter)
        {
            if (!ReplicationParams.ContainsKey(paramName.ToLower()))
            {
                parameter = 0;
                return false;
            }

            parameter = (float)ReplicationParams[paramName.ToLower()];
            return true;
        }

        public void SetReplicationParam(string paramName, float parameter)
        {
            if (ReplicationParams is null)
            {
                ReplicationParams = new Dictionary<string, float>();
                FillReplicationParams();
            }

            var realParamName = paramName.ToLower();

            if (ReplicationParams.ContainsKey(realParamName))
                ReplicationParams[realParamName] = parameter;
            else
                ReplicationParams.Add(realParamName, parameter);
        }

        private void FillReplicationParams()
        {
            foreach (var paramName in GetReplicationParamNames())
            {
                if (!ReplicationParams.ContainsKey(paramName.ToLower()))
                    ReplicationParams.Add(paramName.ToLower(), 0);
            }
        }

        private IEnumerable<string> GetReplicationParamNames()
        {
            if (ReplicationMethod == PartReplicationMethod.ByFixedIntervals)
                yield return "IntervalLength";

            if (ReplicationMethod == PartReplicationMethod.ByDeflection)
                yield return "MaxDeflection";

            if (ReplicationMethod == PartReplicationMethod.ByFixedIntervals ||
                ReplicationMethod == PartReplicationMethod.ByEvenIntervals ||
                ReplicationMethod == PartReplicationMethod.ByDeflection)
            {                
                yield return "InitialShift";
                yield return "SubdivisionCount";
            }

            yield return "OriginalLength"; // mandatory param
        }
    }
}
