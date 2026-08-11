#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Services;

public partial class DependencyManager : Node
{
    private readonly List<object> _injectables = new();
    private readonly List<IService> _services = new();

    private List<Node> _nodeSearchBuffer = new();

    public void ReconstructDependencies()
    {
        _injectables.Clear();
        _services.Clear();
        _nodeSearchBuffer.Clear();
        GetChildrenFromInto(GetTree().Root, ref _nodeSearchBuffer);
        _nodeSearchBuffer.ForEach(OnNodeAdded);
    }
    public void ReinjectDependencies()
    {
        _injectables.ForEach(InjectServicesInto);
    }


    public override void _EnterTree()
    {
        GetTree().NodeAdded += OnNodeAdded;
        GetTree().NodeRemoved += OnNodeRemoved;
    }
    public override void _ExitTree()
    {
        GetTree().NodeAdded -= OnNodeAdded;
        GetTree().NodeRemoved -= OnNodeRemoved;
    }

    private void OnNodeAdded(Node node)
    {
        if (node == this)
            return;

        if (node is IService nodeService && !_services.Contains(nodeService))
            AddServiceUnique(nodeService);

        if (IsInjectable(node) && !_injectables.Contains(node))
        {
            _injectables.Add(node);
            InjectServicesInto(node);
        }
    }

    private void OnNodeRemoved(Node node)
    {
        if (node == this)
            return;

        if (node is IService nodeService)
            RemoveService(nodeService);

        if (_injectables.Contains(node))
            _injectables.Remove(node);
    }

    public void InjectServicesInto(object target)
    {
        if (!IsInjectable(target))
            return;
        
        foreach (var field in target
                     .GetType()
                     .GetFields
                     (
                         BindingFlags.Instance
                         | BindingFlags.NonPublic
                         | BindingFlags.Public
                     )
                )
        {
            if (!Attribute.IsDefined(field, typeof(InjectAttribute)))
                continue;

            var service = GetService(field.FieldType);
            field.SetValue(target, service);
        }

        foreach (var propertyInfo in target
                     .GetType()
                     .GetProperties
                     (
                         BindingFlags.Instance
                         | BindingFlags.NonPublic
                         | BindingFlags.Public
                     )
                )
        {
            if (!Attribute.IsDefined(propertyInfo, typeof(InjectAttribute)))
                continue;

            var service = GetService(propertyInfo.PropertyType);
            if (service == null)
                continue;

            if (!propertyInfo.CanWrite)
            {
                var setter = propertyInfo.GetSetMethod(true);
                if (setter == null)
                {
                    // If there is no setter at all for this PropertyInfo, then the DotNet compiler still
                    // has a hidden 'Backing Field' property that we can find via reflection and write to.
                    var backingFieldName = $"<{propertyInfo.Name}>k__BackingField";
                    var backingField = propertyInfo.DeclaringType?.GetField
                    (
                        backingFieldName,
                        BindingFlags.Instance | BindingFlags.NonPublic
                    );

                    if (backingField != null)
                        backingField.SetValue(target, service);

                    continue;
                }

                setter?.Invoke(target, [service]);
                continue;
            }

            propertyInfo.SetValue(target, service);
        }
    }

    private bool IsInjectable(object target)
    {
        foreach (var field in target
                     .GetType()
                     .GetFields
                     (
                         BindingFlags.Instance
                         | BindingFlags.NonPublic
                         | BindingFlags.Public
                     )
                )
            if (Attribute.IsDefined(field, typeof(InjectAttribute)))
                return true;
        
        foreach (var propertyInfo in target
                     .GetType()
                     .GetProperties
                     (
                         BindingFlags.Instance
                         | BindingFlags.NonPublic
                         | BindingFlags.Public
                     )
                )
            if (Attribute.IsDefined(propertyInfo, typeof(InjectAttribute)))
                return true;

        return false;
    }

    public object? GetService(Type serviceType)
    {
        return _services.FirstOrDefault(serviceType.IsInstanceOfType);
    }

    public void AddServiceUnique(IService value)
    {
        if (_services.Exists(x => value.GetType().IsInstanceOfType(x)))
            return;

        _services.Add(value);

        _injectables.ForEach(InjectServicesInto);

        GD.Print($"DependencyManager registered service type: {value.GetType().FullName}");
    }

    private void RemoveService(IService value)
    {
        _services.Remove(value);
    }

    private void GetChildrenFromInto(Node rootNode, ref List<Node> collectionRef)
    {
        collectionRef.AddRange(rootNode.GetChildren());
        foreach (var child in rootNode.GetChildren())
        {
            GetChildrenFromInto(child, ref collectionRef);
        }
    }
}