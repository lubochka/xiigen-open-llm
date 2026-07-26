// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Reflection;
using System.Text.RegularExpressions;
using AgentInfrastructure;

/// <summary>
/// Projects already-produced runtime evidence and a physical reflection scan of the
/// caller-specified boundary types into the locked F5.1.3 predicate carrier. The meter
/// does not execute a target, infer checkpoint absence, assess trained features, accuracy,
/// or aggregate quality, and does not decide lifecycle or promotion state.
/// </summary>
public static class DynamicGraphUntrainedHonestyMeter
{
    private const string InvalidMeasurementCode = "INVALID_UNTRAINED_HONESTY_MEASUREMENT";
    private const string InvalidMeasurementMessage = "Dynamic graph untrained honesty measurement is invalid.";
    private const string NoTrainedStateToken = "NO_TRAINED_STATE_LOADED";
    private const string NoSemanticClassificationToken = "NO_SEMANTIC_CLASSIFICATION_PERFORMED";

    /// <summary>
    /// Reflects only the supplied concrete types and classifies the supplied runtime result.
    /// No inspected target is instantiated or invoked.
    /// </summary>
    public static OperationResult<DynamicGraphUntrainedHonestyMeasurement> Measure(
        DynamicGraphUntrainedHonestyObservation? observation)
    {
        if (observation is null)
            return InvalidMeasurement();

        string[]? runtimeEvidence;
        Type[]? inspectedTypes;
        try
        {
            // Snapshot in contract order before validating or reflecting either carrier.
            runtimeEvidence = observation.RuntimeEvidence?.ToArray();
            inspectedTypes = observation.InspectedTypes?.ToArray();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidMeasurement();
        }

        if (!IsCanonicalValue(observation.EvaluationId)
            || runtimeEvidence is null
            || inspectedTypes is null
            || !IsCanonicalUniqueEvidence(runtimeEvidence)
            || inspectedTypes.Length == 0
            || inspectedTypes.Any(type => type is null)
            || inspectedTypes.Distinct(ReferenceEqualityComparer.Instance).Count() != inspectedTypes.Length)
        {
            return InvalidMeasurement();
        }

        string[] inspectedTypeNames;
        string[] forbiddenShortcutMatches;
        try
        {
            var typeNames = new HashSet<string>(StringComparer.Ordinal);
            var matches = new HashSet<string>(StringComparer.Ordinal);
            const BindingFlags flags = BindingFlags.Instance
                                       | BindingFlags.Static
                                       | BindingFlags.Public
                                       | BindingFlags.NonPublic
                                       | BindingFlags.FlattenHierarchy;

            foreach (var inspectedType in inspectedTypes)
            {
                var typeName = inspectedType.FullName;
                if (!IsCanonicalValue(typeName) || !typeNames.Add(typeName!))
                    return InvalidMeasurement();

                foreach (var field in inspectedType.GetFields(flags))
                {
                    var fieldType = field.FieldType;
                    if (typeof(Regex).IsAssignableFrom(fieldType)
                        || fieldType.Name.Contains("Dictionary", StringComparison.OrdinalIgnoreCase))
                    {
                        var fieldTypeName = fieldType.FullName ?? fieldType.Name;
                        matches.Add($"{typeName}|field-type:{field.Name}:{fieldTypeName}");
                    }
                }

                foreach (var member in inspectedType.GetMembers(flags))
                {
                    if (HasForbiddenMemberName(member.Name))
                        matches.Add($"{typeName}|member:{member.MemberType}:{member.Name}");
                }
            }

            inspectedTypeNames = typeNames.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            forbiddenShortcutMatches = matches.OrderBy(value => value, StringComparer.Ordinal).ToArray();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidMeasurement();
        }

        var sortedRuntimeEvidence = runtimeEvidence
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        var hasNoTrainedStateEvidence = sortedRuntimeEvidence.Contains(
            NoTrainedStateToken,
            StringComparer.Ordinal);
        var hasNoSemanticClassificationEvidence = sortedRuntimeEvidence.Contains(
            NoSemanticClassificationToken,
            StringComparer.Ordinal);
        var meetsUntrainedHonestyGate = observation.RuntimeReportedNotReady
                                        && hasNoTrainedStateEvidence
                                        && hasNoSemanticClassificationEvidence
                                        && forbiddenShortcutMatches.Length == 0;

        return OperationResult<DynamicGraphUntrainedHonestyMeasurement>.Ok(
            new DynamicGraphUntrainedHonestyMeasurement(
                observation.EvaluationId,
                observation.RuntimeReportedNotReady,
                hasNoTrainedStateEvidence,
                hasNoSemanticClassificationEvidence,
                Array.AsReadOnly(sortedRuntimeEvidence),
                Array.AsReadOnly(inspectedTypeNames),
                Array.AsReadOnly(forbiddenShortcutMatches),
                meetsUntrainedHonestyGate));
    }

    private static bool IsCanonicalUniqueEvidence(IReadOnlyList<string> values)
    {
        var unique = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in values)
        {
            if (!IsCanonicalValue(value) || !unique.Add(value))
                return false;
        }

        return true;
    }

    private static bool HasForbiddenMemberName(string name)
        => name.Contains("Classifier", StringComparison.OrdinalIgnoreCase)
           || name.Contains("Trainer", StringComparison.OrdinalIgnoreCase)
           || name.Contains("Lookup", StringComparison.OrdinalIgnoreCase)
           || name.Contains("Bm25", StringComparison.OrdinalIgnoreCase)
           || name.Contains("ExactPrompt", StringComparison.OrdinalIgnoreCase);

    private static bool IsCanonicalValue(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && value.All(character => !char.IsControl(character))
           && value.IndexOfAny(['\r', '\n', '\u0085', '\u2028', '\u2029']) < 0;

    private static OperationResult<DynamicGraphUntrainedHonestyMeasurement> InvalidMeasurement()
        => OperationResult<DynamicGraphUntrainedHonestyMeasurement>.Fail(
            InvalidMeasurementMessage,
            InvalidMeasurementCode);
}
