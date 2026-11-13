// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using ContinuousLearning;
using DesignConversionDomain;
using DPO.Models;
using TransformerModel.Config;

public sealed class DesignConversionDpoPolicyRepresentationTests
{
    [Fact]
    public void CanonicalSideText_ContainsExactlyHtmlCssAndNormalizesOnlyLineEndings()
    {
        var text = DesignConversionDpoPolicyCheckpointBridge.SerializePolicySideText(
            "<main>alpha\r\nbeta\rgamma</main>",
            ".card{display:grid}\r\n.note{color:blue}");

        using var json = JsonDocument.Parse(text);
        Assert.Equal(
            ["html", "css"],
            json.RootElement.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal("<main>alpha\nbeta\ngamma</main>", json.RootElement.GetProperty("html").GetString());
        Assert.Equal(".card{display:grid}\n.note{color:blue}", json.RootElement.GetProperty("css").GetString());
    }

    [Fact]
    public void CanonicalSideText_LabelOnlyMutationsDoNotChangeTrainableText()
    {
        var baseline = BuildRecord();
        var mutatedChosen = baseline.Chosen with
        {
            OutputId = "changed-output-id",
            CandidateId = "changed-candidate-id",
            Accepted = !baseline.Chosen.Accepted,
            Verdict = baseline.Chosen.Verdict with
            {
                VerdictId = "changed-verdict-id",
                Score = 0.01,
                Passed = !baseline.Chosen.Verdict.Passed,
                Defects =
                [
                    new VisualDefect(
                        "changed-defect-id",
                        "outcome-only-defect",
                        "high",
                        "outcome-only description",
                        1,
                        2,
                        3,
                        4,
                        ["outcome-only-image"])
                ]
            },
            EvidenceRefs = ["outcome-only-evidence"]
        };
        var mutated = baseline with { Chosen = mutatedChosen };

        var originalText = DesignConversionDpoPolicyCheckpointBridge
            .BuildPolicyPreferenceTextPreviewForTests(baseline).Chosen;
        var mutatedText = DesignConversionDpoPolicyCheckpointBridge
            .BuildPolicyPreferenceTextPreviewForTests(mutated).Chosen;

        Assert.Equal(originalText, mutatedText);
        foreach (var forbidden in new[] { "changed-output", "changed-candidate", "changed-verdict", "outcome-only", "score", "passed", "accepted", "defect" })
            Assert.DoesNotContain(forbidden, mutatedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CanonicalSideText_DifferentHtmlOrCssChangesTrainableText()
    {
        var baseline = DesignConversionDpoPolicyCheckpointBridge.SerializePolicySideText(
            "<main>alpha</main>",
            ".card{display:grid}");
        var htmlChanged = DesignConversionDpoPolicyCheckpointBridge.SerializePolicySideText(
            "<main>beta</main>",
            ".card{display:grid}");
        var cssChanged = DesignConversionDpoPolicyCheckpointBridge.SerializePolicySideText(
            "<main>alpha</main>",
            ".card{display:block}");

        Assert.NotEqual(baseline, htmlChanged);
        Assert.NotEqual(baseline, cssChanged);
        Assert.NotEqual(htmlChanged, cssChanged);
    }

    [Fact]
    public void CanonicalSideText_StandaloneEvaluatedRuntimeAndMachineUseByteIdenticalRepresentation()
    {
        var record = BuildRecord();
        var standalone = DesignConversionDpoPolicyCheckpointBridge
            .BuildPolicyPreferenceTextPreviewForTests(record);
        var evaluated = DesignConversionDpoPolicyCheckpointBridge
            .BuildEvaluatedPackedBatchPreviewForTests(record, BuildConfig());
        var runtime = DesignConversionDpoPolicyCheckpointBridge
            .BuildPolicyRuntimeBridgeItemPreviewForTests(
                record,
                "policy-checkpoint.bin",
                new string('b', 64));
        var machine = DesignConversionMachine.BuildDpoPolicyRuntimeCandidateSidePreviewForTests(
            record.Chosen.Html,
            record.Chosen.Css);

        Assert.True(evaluated.IsSuccess, evaluated.ErrorMessage);
        Assert.Equal(standalone.Chosen, evaluated.Value!.Chosen);
        Assert.Equal(standalone.Rejected, evaluated.Value.Rejected);
        Assert.Equal(standalone.Chosen, runtime.ChosenResponse);
        Assert.Equal(standalone.Rejected, runtime.RejectedResponse);
        Assert.Equal(standalone.Chosen, machine);
    }

    [Fact]
    public void CanonicalSideText_IdentityPairIsRejectedWhenOnlyOutcomeLabelsDiffer()
    {
        var baseline = BuildRecord();
        var identityOnly = baseline with
        {
            Rejected = baseline.Chosen with
            {
                OutputId = "identity-label-rejected-output",
                CandidateId = "identity-label-rejected-candidate",
                Accepted = false,
                Verdict = baseline.Chosen.Verdict with
                {
                    VerdictId = "identity-label-rejected-verdict",
                    Score = 0.0,
                    Passed = false,
                    EvidenceRefs = ["identity-label-rejected-evidence"]
                },
                EvidenceRefs = ["identity-label-rejected-evidence"]
            }
        };

        var packed = DesignConversionDpoPolicyCheckpointBridge
            .BuildEvaluatedPackedBatchPreviewForTests(identityOnly, BuildConfig());

        Assert.False(packed.IsSuccess);
        Assert.Equal(
            DesignConversionDpoPolicyCheckpointBridge.EvaluatedPackingInvalidErrorCode,
            packed.ErrorCode);
    }

    private static DesignConversionDpoTrainingRecord BuildRecord()
    {
        var chosen = BuildOutput(
            "chosen-output",
            "chosen-candidate",
            "<main data-side=\"chosen\">chosen</main>",
            "[data-side=chosen]{display:grid;gap:12px}",
            score: 0.96,
            passed: true,
            accepted: true);
        var rejected = BuildOutput(
            "rejected-output",
            "rejected-candidate",
            "<main data-side=\"rejected\">rejected</main>",
            "[data-side=rejected]{display:block;gap:0}",
            score: 0.21,
            passed: false,
            accepted: false);
        return new DesignConversionDpoTrainingRecord(
            RecordId: "policy-representation-record",
            UnitId: DesignConversionDpoUnits.CriticUnit,
            InternalUnitStatePayload: "{\"state\":\"representation\"}",
            InternalVectorPayload: [0.25, -0.5, 0.75],
            SourceStructurePayload: "{\"type\":\"FRAME\"}",
            Chosen: chosen,
            Rejected: rejected,
            TieBreakerPayload: null,
            UsefulnessScore: 0.8,
            NegativeQualityReason: "measured difference",
            NegativeReasonTaxonomy: "layout",
            Provenance: new DesignConversionDpoProvenance(
                "policy-representation-request",
                "Figma",
                "artifact://policy-representation/manifest.json",
                "reviewer:redacted",
                DateTimeOffset.UnixEpoch,
                new Dictionary<string, string>()),
            License: new DesignConversionLicenseScope(
                "policy-representation-license",
                "local-test",
                AllowsTraining: true,
                ContainsUserPrivateData: false,
                new Dictionary<string, string>()),
            Quarantine: new DesignConversionQuarantineState(
                IsQuarantined: false,
                Reason: string.Empty,
                EvidenceRefs: []),
            TeacherProvenance: TrainingTeacherProvenance.Deterministic(
                "policy-representation-deterministic"))
        {
            // Контракт representation не доказывает внешнего teacher: это
            // deterministic/no-external fixture с явным паспортом сторон.
            ChosenAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator,
            RejectedAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator
        };
    }

    private static DesignConversionImprovementOutput BuildOutput(
        string outputId,
        string candidateId,
        string html,
        string css,
        double score,
        bool passed,
        bool accepted)
        => new(
            outputId,
            candidateId,
            html,
            css,
            new VisualVerdict(
                $"{outputId}-verdict",
                score,
                [],
                [$"{outputId}-evidence"],
                passed),
            QaResults: [],
            EvidenceRefs: [$"{outputId}-evidence"],
            Accepted: accepted);

    private static ModelConfig BuildConfig()
        => new()
        {
            VocabSize = 256,
            ContextLen = 128,
            HiddenDim = 16,
            NumLayers = 1,
            NumHeads = 4,
            NumKvHeads = 2,
            FfnHiddenDim = 32
        };
}
