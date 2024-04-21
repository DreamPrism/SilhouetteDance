using Lagrange.Core;

namespace SilhouetteDance.Function;

/// <summary>
/// The base class for all executable functions, inspired by ControllerBase from ASP.NET Core.
/// </summary>
public abstract class FunctionBase
{
    protected readonly ResContext ResContext;
    public FunctionBase(ResContext _resContext) => ResContext = _resContext;
}