// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;

namespace DesignConversionDomain;

public sealed class DesignConversionDomainMarker;

public sealed record DesignConversionCriticUnitInput(
    SubmittedDesignConversionRequest Request,
    IReadOnlyList<RenderQaResultRecord> QaResults,
    IReadOnlyList<DesignConversionQaComparisonArtifact> ComparisonArtifacts,
    IReadOnlyList<string> AiTeacherEvidenceRefs,
    IReadOnlyDictionary<string, string> DeterministicEvidenceRefs);

public sealed record DesignConversionCriticUnitOutput(
    string OutputId,
    VisualVerdict Verdict,
    IReadOnlyList<string> QaEvidenceRefs,
    IReadOnlyList<string> AiTeacherEvidenceRefs,
    bool DeterministicLogicClaimedAsLearning);

public sealed record DesignConversionImproverUnitInput(
    SubmittedDesignConversionRequest Request,
    VisualVerdict CriticVerdict,
    DesignConversionImprovementCandidate Candidate,
    IReadOnlyList<string> QaEvidenceRefs,
    IReadOnlyList<string> AiTeacherEvidenceRefs);

public sealed record DesignConversionImproverUnitOutput(
    string OutputId,
    DesignConversionImprovementCandidate Candidate,
    DesignConversionImprovementOutput Output,
    IReadOnlyList<string> QaEvidenceRefs,
    IReadOnlyList<string> AiTeacherEvidenceRefs,
    bool DeterministicLogicClaimedAsLearning);

public interface IDesignConversionCriticUnit
    : ILogicalUnit<DesignConversionCriticUnitInput, DesignConversionCriticUnitOutput>
{
}

public interface IDesignConversionImproverUnit
    : ILogicalUnit<DesignConversionImproverUnitInput, DesignConversionImproverUnitOutput>
{
}
