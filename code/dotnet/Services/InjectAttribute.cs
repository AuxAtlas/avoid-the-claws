using System;

namespace AvoidClaws.code.dotnet.Services;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class InjectAttribute : Attribute
{
}