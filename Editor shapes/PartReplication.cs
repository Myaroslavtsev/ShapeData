using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeData
{
    public enum PartReplicationMethod
    {
        AtFixedPos,
        AtTheEnd,
        ByFixedIntervals,
        ByEvenIntervals,
        StretchedByArc,
        StretchedByDeflection
    }

    public interface IPartReplication
    {
        PartReplicationMethod ReplicationMethod {get; }

        int GetParamCount();
        IEnumerable<(string Name, float Value)> GetParams();

        bool LeaveAtLeastOnePart { get; set; }
    }

    public interface IDistancingParams
    {
        IEnumerable<(string Name, float Value)> GetAll();
        int Count { get; }
    }

    public sealed class NoParams : IDistancingParams
    {
        public static readonly NoParams Instance = new();
        private NoParams() { }

        public int Count => 0;

        public IEnumerable<(string Name, float Value)> GetAll()
        {
            yield break;
        }
    }

    public class FixedInterval : IDistancingParams
    {
        public float Interval { get; }

        public FixedInterval(float interval)
        {
            Interval = interval;
        }

        public IEnumerable<(string Name, float Value)> GetAll()
        {
            yield return ("Interval", Interval);
        }
        public int Count => 1;
    }

    public class EvenInterval : IDistancingParams
    {
        public float Interval { get; }

        public EvenInterval(float interval)
        {
            Interval = interval;
        }

        public IEnumerable<(string Name, float Value)> GetAll()
        {
            yield return ("MinInterval", Interval);
        }
        public int Count => 1;
    }

    public class StretchedByArc : IDistancingParams
    {
        public float OriginalLength { get; }
        public float MinLength { get; }

        public StretchedByArc(float originalLength, float minLength)
        {
            OriginalLength = originalLength;
            MinLength = minLength;
        }

        public IEnumerable<(string Name, float Value)> GetAll()
        {
            yield return ("OriginalLength", OriginalLength);
            yield return ("MinLength", MinLength);
        }
        public int Count => 2;
    }

    public class StretchedByDeflection : IDistancingParams
    {
        public float OriginalLength { get; }
        public float MaxDeflection { get; }

        public StretchedByDeflection(float originalLength, float maxDeflection)
        {
            OriginalLength = originalLength;
            MaxDeflection = maxDeflection;
        }

        public IEnumerable<(string Name, float Value)> GetAll()
        {
            yield return ("OriginalLength", OriginalLength);
            yield return ("MaxDeflection", MaxDeflection);
        }
        public int Count => 1;
    }

    public class ReplicationAtFixedPos : IPartReplication
    {
        public PartReplicationMethod ReplicationMethod { get => PartReplicationMethod.AtFixedPos; }        

        public readonly IDistancingParams _distancingParams;

        public ReplicationAtFixedPos()
        {
            _distancingParams = NoParams.Instance;
        }

        public IEnumerable<(string Name, float Value)> GetParams() => _distancingParams.GetAll();
        public int GetParamCount() => _distancingParams.Count;
        public bool LeaveAtLeastOnePart { get; set; }
    }

    public class ReplicationAtTheEnd : IPartReplication
    {
        public PartReplicationMethod ReplicationMethod { get => PartReplicationMethod.AtTheEnd; }

        public readonly IDistancingParams _distancingParams;

        public ReplicationAtTheEnd()
        {
            _distancingParams = NoParams.Instance;
        }

        public IEnumerable<(string Name, float Value)> GetParams() => _distancingParams.GetAll();
        public int GetParamCount() => _distancingParams.Count;
        public bool LeaveAtLeastOnePart { get; set; }
    }

    public class ReplicationByFixedIntervals : IPartReplication
    {
        public PartReplicationMethod ReplicationMethod { get => PartReplicationMethod.ByFixedIntervals; }

        public readonly IDistancingParams _distancingParams;

        public ReplicationByFixedIntervals(float interval, bool leaveAtLeastOne = false)
        {
            _distancingParams = new FixedInterval(interval);
            LeaveAtLeastOnePart = leaveAtLeastOne;
        }
        public IEnumerable<(string Name, float Value)> GetParams() => _distancingParams.GetAll();
        public int GetParamCount() => _distancingParams.Count;
        public bool LeaveAtLeastOnePart { get; set; }
    }

    public class ReplicationByEvenIntervals : IPartReplication
    {
        public PartReplicationMethod ReplicationMethod { get => PartReplicationMethod.ByEvenIntervals; }

        public readonly IDistancingParams _distancingParams;

        public ReplicationByEvenIntervals(float interval, bool leaveAtLeastOne = false)
        {
            _distancingParams = new EvenInterval(interval);
            LeaveAtLeastOnePart = leaveAtLeastOne;
        }
        public IEnumerable<(string Name, float Value)> GetParams() => _distancingParams.GetAll();
        public int GetParamCount() => _distancingParams.Count;
        public bool LeaveAtLeastOnePart { get; set; }
    }

    public class ReplicationStretchedByArc : IPartReplication
    {
        public PartReplicationMethod ReplicationMethod { get => PartReplicationMethod.StretchedByArc; }

        public readonly IDistancingParams _distancingParams;

        public ReplicationStretchedByArc(float originalLength, float minLength, bool leaveAtLeastOne = false)
        {
            _distancingParams = new StretchedByArc(originalLength, minLength);
            LeaveAtLeastOnePart = leaveAtLeastOne;
        }
        public IEnumerable<(string Name, float Value)> GetParams() => _distancingParams.GetAll();
        public int GetParamCount() => _distancingParams.Count;
        public bool LeaveAtLeastOnePart { get; set; }
    }

    public class ReplicationStretchedByDeflection : IPartReplication
    {
        public PartReplicationMethod ReplicationMethod { get => PartReplicationMethod.StretchedByDeflection; }

        public readonly IDistancingParams _distancingParams;

        public ReplicationStretchedByDeflection(float originalLength, float maxDeflection, bool leaveAtLeastOne = false)
        {
            _distancingParams = new StretchedByDeflection(originalLength, maxDeflection);
            LeaveAtLeastOnePart = leaveAtLeastOne;
        }
        public IEnumerable<(string Name, float Value)> GetParams() => _distancingParams.GetAll();
        public int GetParamCount() => _distancingParams.Count;
        public bool LeaveAtLeastOnePart { get; set; }
    }
}
