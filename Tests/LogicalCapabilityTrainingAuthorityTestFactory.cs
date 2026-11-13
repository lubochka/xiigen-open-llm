// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;

internal static class LogicalCapabilityTrainingAuthorityTestFactory
{
    internal static LogicalTrainingContentProvenance IssueSingle(
        LogicalFlowResult source,
        LogicalTrainingContentDeterministicInputs inputs)
        => IssueGroup((source, inputs))[0];

    internal static LogicalTrainingContentProvenance[] IssueGroup(
        params (LogicalFlowResult Source,
            LogicalTrainingContentDeterministicInputs Inputs)[] candidates)
    {
        if (candidates.Length == 0)
            throw new InvalidOperationException();
        var prepared = candidates.Select(candidate =>
        {
            var subPhase = new LogicalSubPhase(
                candidate.Inputs.SubPhaseId,
                candidate.Inputs.PhaseId,
                candidate.Inputs.SubPhaseName,
                candidate.Inputs.SubPhasePurpose,
                candidate.Inputs.DomainIds.ToArray(),
                candidate.Inputs.PredicateIds.ToArray(),
                []);
            var current = LogicalCapabilityTrainingPreferenceProjector
                .ProjectCurrent(
                    candidate.Inputs.OriginalUserRequest,
                    candidate.Source,
                    subPhase);
            if (!current.IsSuccess || current.Value is null ||
                !LogicalTrainingContentProvenance
                    .TryComputeDeterministicTrainingIdentities(
                        candidate.Source,
                        candidate.Inputs,
                        out var bindingSha256,
                        out var replaySealGroupSha256))
            {
                throw new InvalidOperationException();
            }
            return (
                candidate.Source,
                candidate.Inputs,
                SubPhase: subPhase,
                Current: current.Value,
                BindingSha256: bindingSha256,
                ReplaySealGroupSha256: replaySealGroupSha256);
        }).ToArray();
        var group = prepared[0].ReplaySealGroupSha256;
        if (prepared.Any(candidate => !StringComparer.Ordinal.Equals(
                candidate.ReplaySealGroupSha256,
                group)))
        {
            throw new InvalidOperationException();
        }
        var bindings = prepared
            .Select(static candidate => candidate.BindingSha256)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
        var replay = LogicalCapabilityTrainingPreferenceProjector
            .ProjectAuthorizedReplay(
                prepared.Select(static candidate => candidate.Current)
                    .ToArray(),
                bindings,
                group,
                prepared[0].Inputs.DomainAdapterTag,
                prepared[0].SubPhase);
        if (!replay.IsSuccess || replay.Value is null)
            throw new InvalidOperationException();

        return prepared.Select(candidate =>
            LogicalTrainingContentProvenance.IssueDeterministicSynthetic(
                candidate.Source,
                candidate.Inputs,
                candidate.Current.DpoPreferenceSha256,
                bindings,
                replay.Value.Descriptor.ReplayPayloadSha256,
                replay.Value.Descriptor.DpoPreferenceSha256)).ToArray();
    }
}
