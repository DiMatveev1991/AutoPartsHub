using System.Reflection;
using AutoPartsHub.Models;

namespace AutoPartsHub.Tests;

/// <summary>Защищает принятое правило: проект Models содержит данные, а не бизнес-поведение.</summary>
public sealed class ModelArchitectureTests
{
    /// <summary>Проверяет отсутствие объявленных пользовательских методов у моделей.</summary>
    [Fact]
    public void Models_ExposeOnlyPropertiesAndObjectInfrastructure()
    {
        var modelTypes = typeof(User).Assembly.GetTypes()
            .Where(type => type.IsClass && type.Namespace == typeof(User).Namespace)
            .ToArray();

        var businessMethods = modelTypes
            .SelectMany(type => type.GetMethods(
                BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly))
            .Where(method => !method.IsSpecialName)
            .ToArray();

        Assert.Empty(businessMethods);
    }

    /// <summary>Проверяет, что EF Core и BLL могут заполнять свойства POCO-моделей.</summary>
    [Fact]
    public void Models_HavePublicSettersForStoredState()
    {
        var propertiesWithoutPublicSetter = typeof(User).Assembly.GetTypes()
            .Where(type => type.IsClass && type.Namespace == typeof(User).Namespace)
            .SelectMany(type => type.GetProperties().Select(property => (type, property)))
            .Where(item => item.property.SetMethod?.IsPublic != true)
            .ToArray();

        Assert.Empty(propertiesWithoutPublicSetter);
    }
}
