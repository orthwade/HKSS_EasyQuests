using System;

public static class TypeChecker
{
    public static bool IsOfType(object obj, string typeName)
    {
        if (obj == null || string.IsNullOrWhiteSpace(typeName))
            return false;

        Type type = Type.GetType(typeName);
        if (type == null)
            return false;

        return type.IsInstanceOfType(obj);
    }
}
