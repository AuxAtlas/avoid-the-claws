using System;

namespace AvoidClaws.code.dotnet.Util.Exceptions;

public class InvalidPrefabException(string message = "") : ArgumentException(message)
{
    
}