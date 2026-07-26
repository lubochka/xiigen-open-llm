// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Reflection;
using System.Runtime.CompilerServices;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class DynamicGraphMeasurementTargetGuardTests
{
    private const string InvalidCode = "INVALID_DYNAMIC_GRAPH_MEASUREMENT_TARGET";
    private const string InvalidMessage = "Dynamic graph measurement target is invalid.";
    private const string ForbiddenCode = "FORBIDDEN_RUNTIME_SHORTCUT";
    private const string ForbiddenMessage = "Dynamic graph measurement target is a forbidden runtime shortcut.";

    [Fact]
    public void Contracts_ExposeExactEnumNamesValuesAndRecordShape()
    {
        Assert.Equal(
            ["Pair", "L2LogicalUnit", "L3LogicalFlow", "L4LogicalOrchestrator", "L5LearningMachine"],
            Enum.GetNames<DynamicGraphMeasurementTargetLevel>());
        Assert.Equal([0, 1, 2, 3, 4], Enum.GetValues<DynamicGraphMeasurementTargetLevel>().Select(value => (int)value));

        Assert.Equal(
            ["LearningSubject", "Knn", "Scorer", "Counter", "Regex"],
            Enum.GetNames<DynamicGraphMeasurementMechanismKind>());
        Assert.Equal([0, 1, 2, 3, 4], Enum.GetValues<DynamicGraphMeasurementMechanismKind>().Select(value => (int)value));

        var constructor = Assert.Single(typeof(DynamicGraphMeasurementTarget).GetConstructors());
        Assert.Equal(
            [
                ("TargetId", typeof(string)),
                ("Level", typeof(DynamicGraphMeasurementTargetLevel)),
                ("MechanismKind", typeof(DynamicGraphMeasurementMechanismKind)),
                ("DeterministicFinalAuthority", typeof(bool))
            ],
            constructor.GetParameters().Select(parameter => (parameter.Name!, parameter.ParameterType)).ToArray());
        Assert.Equal(
            ["TargetId", "Level", "MechanismKind", "DeterministicFinalAuthority"],
            typeof(DynamicGraphMeasurementTarget)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)
                .ToArray());
        Assert.Null(typeof(DynamicGraphMeasurementTarget).GetProperty(
            "Mechanism",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        var mechanismKindProperty = typeof(DynamicGraphMeasurementTarget).GetProperty(
            "MechanismKind",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.NotNull(mechanismKindProperty);
        Assert.Equal(typeof(DynamicGraphMeasurementMechanismKind), mechanismKindProperty!.PropertyType);
    }

    [Fact]
    public void Contracts_HaveNoPublicMutableFieldsAndRecordPropertiesAreInitOnly()
    {
        Assert.Empty(typeof(DynamicGraphMeasurementTarget).GetFields(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));

        foreach (var property in typeof(DynamicGraphMeasurementTarget).GetProperties(
                     BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Assert.NotNull(property.SetMethod);
            var setter = property.SetMethod!;
            Assert.Contains(
                typeof(IsExternalInit),
                setter.ReturnParameter.GetRequiredCustomModifiers());
        }
    }

    [Fact]
    public void Guard_ExposesExactlyOnePublicDeclaredMethodWithThePlannedEnvelope()
    {
        var method = Assert.Single(typeof(DynamicGraphMeasurementTargetGuard).GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));

        Assert.Equal("Validate", method.Name);
        Assert.Equal(typeof(OperationResult<DynamicGraphMeasurementTarget>), method.ReturnType);
        var parameter = Assert.Single(method.GetParameters());
        Assert.Equal("target", parameter.Name);
        Assert.Equal(typeof(DynamicGraphMeasurementTarget), parameter.ParameterType);
    }

    [Fact]
    public void Guard_PublicSurfaceContainsNoMetricsRuntimeHeuristicsTrainingDpoOrLifecycleVerdict()
    {
        var surface = string.Join(
            " ",
            typeof(DynamicGraphMeasurementTargetGuard)
                .GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(member => member.ToString()));
        var forbiddenTerms = new[]
        {
            "metric", "runtime", "reflection", "heuristic", "train", "dpo",
            "checkpoint", "promotion", "lifecycle", "verdict"
        };

        foreach (var forbiddenTerm in forbiddenTerms)
            Assert.DoesNotContain(forbiddenTerm, surface, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementTargetLevel.Pair)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L3LogicalFlow)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L4LogicalOrchestrator)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L5LearningMachine)]
    public void Validate_AllFiveLearningSubjectLevelsPass(DynamicGraphMeasurementTargetLevel level)
    {
        var result = DynamicGraphMeasurementTargetGuard.Validate(Target(level: level));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(level, result.Value!.Level);
    }

    [Fact]
    public void Validate_ReturnsDistinctEqualImmutableSnapshot()
    {
        var target = Target(targetId: "target-a", level: DynamicGraphMeasurementTargetLevel.L4LogicalOrchestrator);

        var result = DynamicGraphMeasurementTargetGuard.Validate(target);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value);
        Assert.NotSame(target, result.Value);
        Assert.Equal(target, result.Value);
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementMechanismKind.Knn)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Scorer)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Counter)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Regex)]
    public void Validate_EachRuntimeShortcutFailsClosed(DynamicGraphMeasurementMechanismKind mechanismKind)
    {
        AssertForbidden(DynamicGraphMeasurementTargetGuard.Validate(Target(mechanismKind: mechanismKind)));
    }

    [Fact]
    public void Validate_DeterministicFinalAuthorityFailsClosed()
    {
        AssertForbidden(DynamicGraphMeasurementTargetGuard.Validate(Target(deterministicFinalAuthority: true)));
    }

    [Fact]
    public void Validate_NullTargetIsInvalid()
    {
        AssertInvalid(DynamicGraphMeasurementTargetGuard.Validate(null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(" leading")]
    [InlineData("trailing ")]
    [InlineData("tab\tinside")]
    [InlineData("control\u0001inside")]
    [InlineData("carriage\rreturn")]
    [InlineData("line\nfeed")]
    [InlineData("next\u0085line")]
    [InlineData("line\u2028separator")]
    [InlineData("paragraph\u2029separator")]
    public void Validate_MalformedTargetIdsAreInvalid(string? targetId)
    {
        AssertInvalid(DynamicGraphMeasurementTargetGuard.Validate(Target(targetId: targetId!)));
    }

    [Fact]
    public void Validate_UndefinedLevelIsInvalid()
    {
        AssertInvalid(DynamicGraphMeasurementTargetGuard.Validate(Target(
            level: (DynamicGraphMeasurementTargetLevel)99)));
    }

    [Fact]
    public void Validate_UndefinedMechanismIsInvalid()
    {
        AssertInvalid(DynamicGraphMeasurementTargetGuard.Validate(Target(
            mechanismKind: (DynamicGraphMeasurementMechanismKind)99)));
    }

    [Fact]
    public void Validate_ShapeFailurePrecedesShortcutPolicy()
    {
        AssertInvalid(DynamicGraphMeasurementTargetGuard.Validate(Target(
            targetId: " malformed",
            mechanismKind: DynamicGraphMeasurementMechanismKind.Regex,
            deterministicFinalAuthority: true)));
        AssertInvalid(DynamicGraphMeasurementTargetGuard.Validate(Target(
            level: (DynamicGraphMeasurementTargetLevel)99,
            mechanismKind: DynamicGraphMeasurementMechanismKind.Knn)));
        AssertInvalid(DynamicGraphMeasurementTargetGuard.Validate(Target(
            mechanismKind: (DynamicGraphMeasurementMechanismKind)99,
            deterministicFinalAuthority: true)));
    }

    private static DynamicGraphMeasurementTarget Target(
        string targetId = "target-a",
        DynamicGraphMeasurementTargetLevel level = DynamicGraphMeasurementTargetLevel.Pair,
        DynamicGraphMeasurementMechanismKind mechanismKind = DynamicGraphMeasurementMechanismKind.LearningSubject,
        bool deterministicFinalAuthority = false)
        => new(targetId, level, mechanismKind, deterministicFinalAuthority);

    private static void AssertInvalid(OperationResult<DynamicGraphMeasurementTarget> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(InvalidCode, result.ErrorCode);
        Assert.Equal(InvalidMessage, result.ErrorMessage);
    }

    private static void AssertForbidden(OperationResult<DynamicGraphMeasurementTarget> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(ForbiddenCode, result.ErrorCode);
        Assert.Equal(ForbiddenMessage, result.ErrorMessage);
    }
}
