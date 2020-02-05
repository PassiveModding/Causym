using System;

namespace Causym.Services
{
    // Interface used for reflection to auto-generate the service provider.
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ServiceAttribute : Attribute
    {
    }
}