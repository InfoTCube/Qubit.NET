using System.Numerics;

namespace Qubit.NET.Utilities;

internal class InitialState
{
    public int QubitIndex { get; set; }

    public Complex Alpha { get; set; }

    public Complex Beta { get; set; }

    public State BasicState { get; set; }
}