// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace LlmFromScratch.Tests;

public sealed class DesignConversionDpoCandidateOnlyAuthoringArchitectureTests
{
    private static readonly string[] CandidateAuthoringProductionFiles =
    [
        "44.DesignConversionDomain/DesignConversionDpoTeacherAuthoring.cs",
        "44.DesignConversionDomain/DesignConversionImprovementLoopCoordinator.cs",
        "44.DesignConversionDomain/DesignConversionThinClientJobRuntime.cs",
        "44.DesignConversionDomain/DesignConversionDpoGenerator.cs"
    ];

    [Fact]
    public void ProductionCandidateAuthoring_HasNoTeacherPreferenceAuthoritySurface()
    {
        var source = ReadProductionSource();

        Assert.DoesNotContain("preferred_side", source, StringComparison.Ordinal);
        Assert.DoesNotContain("PreferredSide", source, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "runtime_teacher_selected_preference",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "DesignConversionDpoTeacherAuthoredPreference",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain("AuthorPreferenceAsync", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ProductionCandidateAuthoring_HasNoLatentPositiveProjectionBody()
    {
        var generator = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "44.DesignConversionDomain",
            "DesignConversionDpoGenerator.cs"));

        Assert.DoesNotContain(
            "BuildRuntimeTeacherAuthoredStoredExample",
            generator,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "AttachTeacherAuthoredPayloadCustody",
            generator,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "HasEligibleTeacherAuthoredMeasurement",
            generator,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "high_quality_negative_pair_proven",
            CandidateOnlyGeneratorRegion(generator),
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "training_grade=true",
            CandidateOnlyGeneratorRegion(generator),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IndependentProjection_IsReachableOnlyThroughBoundReceiptValidator()
    {
        var generator = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "44.DesignConversionDomain",
            "DesignConversionDpoGenerator.cs"));
        var receiptRegionStart = generator.IndexOf(
            "IReadOnlyList<DesignConversionIndependentPreferenceEvidence>",
            StringComparison.Ordinal);
        var receiptRegionEnd = generator.IndexOf(
            "NormalizeMeasuredVisualScoreForDpo(",
            receiptRegionStart,
            StringComparison.Ordinal);
        Assert.True(receiptRegionStart >= 0 && receiptRegionEnd > receiptRegionStart);
        var receiptRegion = generator[receiptRegionStart..receiptRegionEnd];

        Assert.Contains(
            "TryValidateIndependentPreferenceEvidence(",
            receiptRegion,
            StringComparison.Ordinal);
        Assert.Contains(
            "BuildIndependentPreferenceStoredExample(",
            receiptRegion,
            StringComparison.Ordinal);
        Assert.Contains("artifactRootBinding", receiptRegion,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "runtime_teacher_selected_preference",
            receiptRegion,
            StringComparison.Ordinal);
        Assert.DoesNotContain(".Append(\"training_grade=true\")", receiptRegion,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ProductionCandidateAuthoring_PreservesGeneratedAndIncumbentIdentities()
    {
        var source = ReadProductionSource();

        Assert.Contains(
            "DesignConversionInternalEvaluationResult GeneratedEvaluation",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "DesignConversionInternalEvaluationResult IncumbentEvaluation",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "teacherSelectedGenerated ?",
            source,
            StringComparison.Ordinal);
    }

    private static string CandidateOnlyGeneratorRegion(string generator)
    {
        var start = generator.IndexOf(
            "BuildRuntimeTeacherAuthoredTrainingCandidates(",
            StringComparison.Ordinal);
        var evidenceParameter = generator.IndexOf(
            "IReadOnlyList<DesignConversionIndependentPreferenceEvidence>",
            start,
            StringComparison.Ordinal);
        var end = generator.LastIndexOf(
            "internal OperationResult<DesignConversionDpoBuildResult>",
            evidenceParameter,
            StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start);
        return generator[start..end];
    }

    private static string ReadProductionSource()
    {
        var root = FindRepositoryRoot();
        return string.Join(
            "\n",
            CandidateAuthoringProductionFiles.Select(relative =>
                File.ReadAllText(Path.Combine(root, relative))));
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                return current.FullName;
            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
