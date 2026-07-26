// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

/// <summary>
/// Admits only canonical learning-subject identities for DynamicGraph measurement.
/// The guard classifies caller-provided evidence; it does not inspect or execute a target.
/// </summary>
public static class DynamicGraphMeasurementTargetGuard
{
    private const string InvalidTargetCode = "INVALID_DYNAMIC_GRAPH_MEASUREMENT_TARGET";
    private const string InvalidTargetMessage = "Dynamic graph measurement target is invalid.";
    private const string ForbiddenShortcutCode = "FORBIDDEN_RUNTIME_SHORTCUT";
    private const string ForbiddenShortcutMessage = "Dynamic graph measurement target is a forbidden runtime shortcut.";

    /// <summary>
    /// Validates target shape before shortcut policy and returns an independently owned snapshot.
    /// </summary>
    public static OperationResult<DynamicGraphMeasurementTarget> Validate(
        DynamicGraphMeasurementTarget? target)
    {
        if (target is null
            || !IsCanonicalTargetId(target.TargetId)
            || !Enum.IsDefined(target.Level)
            || !Enum.IsDefined(target.MechanismKind))
        {
            return OperationResult<DynamicGraphMeasurementTarget>.Fail(
                InvalidTargetMessage,
                InvalidTargetCode);
        }

        if (target.MechanismKind != DynamicGraphMeasurementMechanismKind.LearningSubject
            || target.DeterministicFinalAuthority)
        {
            return OperationResult<DynamicGraphMeasurementTarget>.Fail(
                ForbiddenShortcutMessage,
                ForbiddenShortcutCode);
        }

        return OperationResult<DynamicGraphMeasurementTarget>.Ok(
            new DynamicGraphMeasurementTarget(
                target.TargetId,
                target.Level,
                target.MechanismKind,
                target.DeterministicFinalAuthority));
    }

    private static bool IsCanonicalTargetId(string? targetId)
        => !string.IsNullOrWhiteSpace(targetId)
           && string.Equals(targetId, targetId.Trim(), StringComparison.Ordinal)
           && targetId.All(character =>
               !char.IsControl(character)
               && character is not '\r'
               && character is not '\n'
               && character is not '\u0085'
               && character is not '\u2028'
               && character is not '\u2029');
}
