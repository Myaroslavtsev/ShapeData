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
        StretchedByHorde,
        StretchedByDeflection
    }

    public interface IPartReplication
    {
        PartReplicationMethod ReplicationMetod {get; }

        int GetParamCount();
        IEnumerable<(string Name, float Value)> GetParams();
    }

    public interface IDistancingParams
    {
        IEnumerable<(string Name, float Value)> GetAll();
        int Count { get; }
    }

    public sealed class NoParams : IDistancingParams
    {
        public static readonly NoParams Instance = new NoParams();
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

    public class StretchedByHorde : IDistancingParams
    {
        public float OriginalLength { get; }
        public float MinLength { get; }

        public StretchedByHorde(float originalLength, float minLength)
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
        public float MaxDeflection { get; }

        public StretchedByDeflection(float maxDeflection)
        {
            MaxDeflection = maxDeflection;
        }

        public IEnumerable<(string Name, float Value)> GetAll()
        {
            yield return ("MaxDeflection", MaxDeflection);
        }
        public int Count => 1;
    }

    public class ReplicationAtFixedPos : IPartReplication
    {
        public PartReplicationMethod ReplicationMetod { get => PartReplicationMethod.AtFixedPos; }

        public readonly IDistancingParams _distancingParams;

        public ReplicationAtFixedPos()
        {
            _distancingParams = NoParams.Instance;
        }

        public IEnumerable<(string Name, float Value)> GetParams() => _distancingParams.GetAll();

        public int GetParamCount() => _distancingParams.Count;
    }

    public class ReplicationAtTheEnd : IPartReplication
    {
        public PartReplicationMethod ReplicationMetod { get => PartReplicationMethod.AtTheEnd; }

        public readonly IDistancingParams _distancingParams;

        public ReplicationAtTheEnd()
        {
            _distancingParams = NoParams.Instance;
        }

        public IEnumerable<(string Name, float Value)> GetParams() => _distancingParams.GetAll();

        public int GetParamCount() => _distancingParams.Count;
    }

    public class ReplicationByFixedIntervals : IPartReplication
    {
        public PartReplicationMethod ReplicationMetod { get => PartReplicationMethod.ByFixedIntervals; }

        public readonly IDistancingParams _distancingParams;

        public ReplicationByFixedIntervals(float interval)
        {
            _distancingParams = new FixedInterval(interval);
        }
        public IEnumerable<(string Name, float Value)> GetParams() => _distancingParams.GetAll();

        public int GetParamCount() => _distancingParams.Count;
    }

    public class ReplicationStretchedByHorde : IPartReplication
    {
        public PartReplicationMethod ReplicationMetod { get => PartReplicationMethod.ByFixedIntervals; }

        public readonly IDistancingParams _distancingParams;

        public ReplicationStretchedByHorde(float originalLength, float minLength)
        {
            _distancingParams = new StretchedByHorde(originalLength, minLength);
        }
        public IEnumerable<(string Name, float Value)> GetParams() => _distancingParams.GetAll();

        public int GetParamCount() => _distancingParams.Count;
    }

    public class ReplicationStretchedByDeflection : IPartReplication
    {
        public PartReplicationMethod ReplicationMetod { get => PartReplicationMethod.StretchedByDeflection; }

        public readonly IDistancingParams _distancingParams;

        public ReplicationStretchedByDeflection(float maxDeflection)
        {
            _distancingParams = new StretchedByDeflection(maxDeflection);
        }
        public IEnumerable<(string Name, float Value)> GetParams() => _distancingParams.GetAll();

        public int GetParamCount() => _distancingParams.Count;
    }
}
