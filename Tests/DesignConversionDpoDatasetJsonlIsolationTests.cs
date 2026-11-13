// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using DesignConversionDomain;

public sealed class DesignConversionDpoDatasetJsonlIsolationTests
{
    [Fact]
    public void OptimizerJsonl_IsTrainOnlyExactFiveFields_WhileAuditCoversBothSplitsWithoutModelText()
    {
        var dataset = BuildDataset(
        [
            WithSource(
                DesignConversionDpoGeneratorTests.CreateExample(
                    "jsonl-isolation-a",
                    DesignConversionDpoUnits.CriticUnit,
                    usefulness: 0.91),
                "jsonl-isolation-source-a"),
            WithSource(
                DesignConversionDpoGeneratorTests.CreateExample(
                    "jsonl-isolation-b",
                    DesignConversionDpoUnits.ImproverUnit,
                    usefulness: 0.82),
                "jsonl-isolation-source-b")
        ]);

        Assert.Single(dataset.TrainRows);
        Assert.Single(dataset.HeldOutRows);

        var optimizerLine = Assert.Single(dataset.ToJsonlLines());
        Assert.Equal(dataset.Rows.Count, dataset.ToAuditJsonlLines().Count);
        Assert.DoesNotContain(dataset.HeldOutRows[0].Prompt, dataset.ToJsonlLines());

        using var optimizerDocument = JsonDocument.Parse(optimizerLine);
        var optimizer = optimizerDocument.RootElement;
        Assert.Equal(
            ["domainTag", "unitId", "prompt", "chosen", "rejected"],
            optimizer.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal(dataset.TrainRows[0].Prompt, optimizer.GetProperty("prompt").GetString());
        Assert.Equal(dataset.TrainRows[0].Chosen, optimizer.GetProperty("chosen").GetString());
        Assert.Equal(dataset.TrainRows[0].Rejected, optimizer.GetProperty("rejected").GetString());

        foreach (var auditLine in dataset.ToAuditJsonlLines())
        {
            using var auditDocument = JsonDocument.Parse(auditLine);
            Assert.Equal(
                ["split", "identity", "custody", "result", "labels", "review", "validation", "trainingGrade"],
                auditDocument.RootElement.EnumerateObject().Select(property => property.Name).ToArray());

            foreach (var row in dataset.Rows)
            {
                Assert.DoesNotContain(row.Prompt, auditLine, StringComparison.Ordinal);
                Assert.DoesNotContain(row.Chosen, auditLine, StringComparison.Ordinal);
                Assert.DoesNotContain(row.Rejected, auditLine, StringComparison.Ordinal);
            }

            Assert.DoesNotContain("\"prompt\":", auditLine, StringComparison.Ordinal);
            Assert.DoesNotContain("\"chosen\":", auditLine, StringComparison.Ordinal);
            Assert.DoesNotContain("\"rejected\":", auditLine, StringComparison.Ordinal);
            Assert.DoesNotContain("\"html\":", auditLine, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\"css\":", auditLine, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void OptimizerJsonl_IsByteStableAcrossAuditOnlyIdentityOutcomeReviewerAndTrainingGradeMutations()
    {
        var original = BuildDataset(
        [
            WithSource(
                DesignConversionDpoGeneratorTests.CreateExample(
                    "jsonl-audit-mutation",
                    DesignConversionDpoUnits.ImproverUnit,
                    usefulness: 0.87),
                "jsonl-audit-mutation-source")
        ]);
        var originalRow = Assert.Single(original.TrainRows);
        var originalMetadata = originalRow.Metadata.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        originalMetadata["reviewer_identity_hash"] = new string('a', 64);
        originalMetadata["approval_status"] = "approved";
        originalMetadata["review_status"] = "review-complete";
        originalMetadata["approved_for_training"] = bool.TrueString;
        var baseline = WithTrainRow(original, originalRow with { Metadata = originalMetadata });

        var identityMutation = WithTrainRow(
            baseline,
            baseline.TrainRows[0] with { RecordIdentityHash = new string('b', 64) });
        var outcomeMutation = WithTrainRow(
            baseline,
            baseline.TrainRows[0] with
            {
                Result = baseline.TrainRows[0].Result with
                {
                    ChosenScore = baseline.TrainRows[0].Result.ChosenScore + 0.01
                }
            });
        var reviewerMetadata = baseline.TrainRows[0].Metadata.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        reviewerMetadata["reviewer_identity_hash"] = new string('c', 64);
        reviewerMetadata["review_status"] = "review-reopened";
        var reviewerMutation = WithTrainRow(
            baseline,
            baseline.TrainRows[0] with { Metadata = reviewerMetadata });
        var trainingGradeMutation = WithTrainRow(
            baseline,
            baseline.TrainRows[0] with
            {
                ValidationOutcome = baseline.TrainRows[0].ValidationOutcome with
                {
                    TrainingGrade = true,
                    NotTrainingGrade = false,
                    TrainingGradeBoundary = "real_training_quality_contract_passed"
                }
            });

        var optimizerBytes = baseline.ToJsonlLines();
        var auditBytes = baseline.ToAuditJsonlLines();
        foreach (var mutation in new[] { identityMutation, outcomeMutation, reviewerMutation, trainingGradeMutation })
        {
            Assert.Equal(optimizerBytes, mutation.ToJsonlLines());
            Assert.NotEqual(auditBytes, mutation.ToAuditJsonlLines());
        }
    }

    public static IEnumerable<object?[]> ExactSha256ReferenceCases()
    {
        var lower = new string('a', 64);
        var upper = new string('A', 64);
        var mixed = string.Concat(Enumerable.Repeat("aB", 32));
        yield return [lower, $"sha256:{lower}"];
        yield return [upper, $"sha256:{lower}"];
        yield return [mixed, $"sha256:{mixed.ToLowerInvariant()}"];
        yield return [$"sha256:{lower}", $"sha256:{lower}"];
        yield return [$"sha256:{upper}", $"sha256:{lower}"];
        yield return [null, string.Empty];
        yield return [string.Empty, string.Empty];
        yield return [$" {lower}", string.Empty];
        yield return [$"{lower} ", string.Empty];
        yield return [$"sha256:{lower} ", string.Empty];
        yield return [$"SHA256:{lower}", string.Empty];
        yield return [$"Sha256:{lower}", string.Empty];
        yield return [$"sha256::{lower}", string.Empty];
        yield return [new string('a', 63), string.Empty];
        yield return [new string('a', 65), string.Empty];
        yield return [$"sha256:{new string('a', 63)}", string.Empty];
        yield return [$"sha256:{new string('a', 65)}", string.Empty];
        yield return [new string('g', 64), string.Empty];
        yield return [new string('\uFF21', 64), string.Empty];
        yield return [$"{new string('a', 31)} {new string('a', 32)}", string.Empty];
    }

    [Theory]
    [MemberData(nameof(ExactSha256ReferenceCases))]
    public void AuditSha256Reference_ExactContractTable(string? candidate, string expected)
        => Assert.Equal(
            expected,
            DesignConversionDpoDatasetAuditProjection.NormalizeSha256Reference(candidate));

    [Fact]
    public void AuditJsonl_RetainsCanonicalProductionCustodyReferencesAndRejectsInvalidValues()
    {
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords(
        [
            WithSource(
                DesignConversionDpoGeneratorTests.CreateExample(
                    "jsonl-canonical-custody",
                    DesignConversionDpoUnits.CriticUnit,
                    usefulness: 0.9),
                "jsonl-canonical-custody-source")
        ]);
        Assert.True(build.IsSuccess, build.ErrorMessage);
        var record = Assert.Single(build.Value!.AcceptedRecords);
        var expected = new[]
        {
            DesignConversionDpoGenerator.Sha256Ref("optimizer-prompt"),
            DesignConversionDpoGenerator.Sha256Ref("preferred-response"),
            DesignConversionDpoGenerator.Sha256Ref("non-preferred-response"),
            DesignConversionDpoGenerator.Sha256Ref("governance"),
            DesignConversionDpoGenerator.Sha256Ref("training-record-binding")
        };
        var metadata = record.Provenance.Metadata.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        metadata["payload_custody_required"] = bool.FalseString;
        metadata["payload_custody_version"] = "design-conversion-dpo-payload-custody-v1";
        metadata["payload_custody_prompt_sha256"] = expected[0];
        metadata["payload_custody_chosen_response_sha256"] = expected[1];
        metadata["payload_custody_rejected_response_sha256"] = expected[2];
        metadata["payload_custody_governance_sha256"] = expected[3];
        metadata["payload_custody_training_record_binding_sha256"] = expected[4];
        metadata["payload_custody_raw_in_public_record"] = bool.FalseString;
        var dataset = BuildDataset(record with { Provenance = record.Provenance with { Metadata = metadata } });

        var auditRow = Assert.Single(dataset.ToAuditJsonlRows());
        var custody = auditRow.Custody;
        Assert.Equal(expected[0], custody.OptimizerPromptSha256);
        Assert.Equal(expected[1], custody.PreferredResponseSha256);
        Assert.Equal(expected[2], custody.NonPreferredResponseSha256);
        Assert.Equal(expected[3], custody.GovernanceSha256);
        Assert.Equal(expected[4], custody.TrainingRecordBindingSha256);
        Assert.Equal(
            DesignConversionDpoGenerator.Sha256Ref(record.Provenance.ReviewerRef),
            auditRow.Review.ReviewerIdentityHash);
        var auditLine = Assert.Single(dataset.ToAuditJsonlLines());
        Assert.All(expected, value => Assert.Contains(value, auditLine, StringComparison.Ordinal));

        var invalidMetadata = metadata.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        invalidMetadata["payload_custody_prompt_sha256"] = "C:/private/prompt.html";
        invalidMetadata["payload_custody_chosen_response_sha256"] = "sha256:not-a-digest";
        invalidMetadata["payload_custody_rejected_response_sha256"] = "<style>private-model-text</style>";
        invalidMetadata["payload_custody_governance_sha256"] = new string('f', 63);
        invalidMetadata["payload_custody_training_record_binding_sha256"] = $"sha256:{new string('g', 64)}";
        var invalidDataset = BuildDataset(record with
        {
            Provenance = record.Provenance with { Metadata = invalidMetadata }
        });
        var invalidCustody = Assert.Single(invalidDataset.ToAuditJsonlRows()).Custody;
        Assert.Empty(invalidCustody.OptimizerPromptSha256);
        Assert.Empty(invalidCustody.PreferredResponseSha256);
        Assert.Empty(invalidCustody.NonPreferredResponseSha256);
        Assert.Empty(invalidCustody.GovernanceSha256);
        Assert.Empty(invalidCustody.TrainingRecordBindingSha256);
        var invalidAuditLine = Assert.Single(invalidDataset.ToAuditJsonlLines());
        Assert.DoesNotContain("C:/private", invalidAuditLine, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private-model-text", invalidAuditLine, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AuditJsonl_ProjectsAdversarialDefectCategoriesWithoutChangingOptimizerBytes()
    {
        var baseline = BuildDataset(
        [
            WithSource(
                DesignConversionDpoGeneratorTests.CreateExample(
                    "jsonl-adversarial-category",
                    DesignConversionDpoUnits.ImproverUnit,
                    usefulness: 0.86),
                "jsonl-adversarial-category-source")
        ]);
        var row = Assert.Single(baseline.TrainRows);
        var adversarial = new[]
        {
            DesignConversionNegativeReasonTaxonomy.Spacing,
            "<style>body{display:none}</style>",
            "model says the preferred answer contains private prose",
            "C:/Users/lubao/private/design.fig",
            "<style>body{display:none}</style>"
        };
        var mutated = WithTrainRow(
            baseline,
            row with
            {
                Result = row.Result with { DefectCategories = adversarial },
                TeacherLabels = row.TeacherLabels with { DefectCategories = adversarial.Reverse().ToArray() }
            });

        Assert.Equal(baseline.ToJsonlLines(), mutated.ToJsonlLines());
        var firstAuditLine = Assert.Single(mutated.ToAuditJsonlLines());
        var secondAuditLine = Assert.Single(mutated.ToAuditJsonlLines());
        Assert.Equal(firstAuditLine, secondAuditLine);
        foreach (var raw in adversarial.Where(value => value != DesignConversionNegativeReasonTaxonomy.Spacing))
            Assert.DoesNotContain(raw, firstAuditLine, StringComparison.Ordinal);

        var audit = Assert.Single(mutated.ToAuditJsonlRows());
        Assert.Equal(audit.Result.DefectCategories, audit.Labels.DefectCategories);
        Assert.Contains(DesignConversionNegativeReasonTaxonomy.Spacing, audit.Result.DefectCategories);
        foreach (var raw in adversarial
                     .Where(value => value != DesignConversionNegativeReasonTaxonomy.Spacing)
                     .Distinct(StringComparer.Ordinal))
        {
            Assert.Contains(
                $"unknown:{DesignConversionDpoGenerator.Sha256Ref(raw)}",
                audit.Result.DefectCategories);
        }
        Assert.Equal(
            audit.Result.DefectCategories.Order(StringComparer.Ordinal),
            audit.Result.DefectCategories);
        Assert.Equal(audit.Result.DefectCategories.Distinct(StringComparer.Ordinal), audit.Result.DefectCategories);
        Assert.True(audit.Result.DefectCategories.Count <= 16);
    }

    [Fact]
    public void AuditDefectCategoryProjection_BoundsEnumerationAndOversizedValuesBeforeHashing()
    {
        var overflowForward = Enumerable.Range(
                0,
                DesignConversionDpoDatasetAuditProjection.MaximumInputDefectCategoryCount + 1)
            .Select(index => $"bounded-category-{index}")
            .ToArray();
        var expectedOverflow = new[]
        {
            DesignConversionDpoDatasetAuditProjection.DefectCategoryInputOverflowMarker
        };
        Assert.Equal(
            expectedOverflow,
            DesignConversionDpoDatasetAuditProjection.ProjectDefectCategories(overflowForward));
        Assert.Equal(
            expectedOverflow,
            DesignConversionDpoDatasetAuditProjection.ProjectDefectCategories(overflowForward.Reverse()));
        Assert.Equal(
            expectedOverflow,
            DesignConversionDpoDatasetAuditProjection.ProjectDefectCategories(
                new ThrowBeyondSnapshotLimitCategories()));

        var oversizedCharacters = "private-model-prose-" + new string(
            'x',
            DesignConversionDpoDatasetAuditProjection.MaximumDefectCategoryLength);
        var oversizedUtf8 = new string(
            '\u754C',
            DesignConversionDpoDatasetAuditProjection.MaximumDefectCategoryUtf8ByteLength / 3 + 1);
        Assert.True(oversizedUtf8.Length <= DesignConversionDpoDatasetAuditProjection.MaximumDefectCategoryLength);
        var projected = DesignConversionDpoDatasetAuditProjection.ProjectDefectCategories(
        [
            DesignConversionNegativeReasonTaxonomy.Spacing,
            string.Empty,
            " \t ",
            oversizedCharacters,
            oversizedUtf8
        ]);
        Assert.Equal(
            [
                DesignConversionDpoDatasetAuditProjection.DefectCategoryInvalidMarker,
                DesignConversionNegativeReasonTaxonomy.Spacing
            ],
            projected);

        var baseline = BuildDataset(
        [
            WithSource(
                DesignConversionDpoGeneratorTests.CreateExample(
                    "jsonl-bounded-category",
                    DesignConversionDpoUnits.CriticUnit,
                    usefulness: 0.84),
                "jsonl-bounded-category-source")
        ]);
        var row = Assert.Single(baseline.TrainRows);
        var mutated = WithTrainRow(
            baseline,
            row with
            {
                Result = row.Result with
                {
                    DefectCategories = [oversizedCharacters, oversizedUtf8, " \t "]
                },
                TeacherLabels = row.TeacherLabels with
                {
                    DefectCategories = [oversizedUtf8, oversizedCharacters]
                }
            });
        Assert.Equal(baseline.ToJsonlLines(), mutated.ToJsonlLines());
        var auditLine = Assert.Single(mutated.ToAuditJsonlLines());
        Assert.DoesNotContain(oversizedCharacters, auditLine, StringComparison.Ordinal);
        Assert.DoesNotContain(oversizedUtf8, auditLine, StringComparison.Ordinal);
        var audit = Assert.Single(mutated.ToAuditJsonlRows());
        Assert.Same(audit.Result.DefectCategories, audit.Labels.DefectCategories);
        Assert.Equal(
            [DesignConversionDpoDatasetAuditProjection.DefectCategoryInvalidMarker],
            audit.Result.DefectCategories);
    }

    private static DesignConversionDpoDataset WithTrainRow(
        DesignConversionDpoDataset dataset,
        DesignConversionDpoDatasetRow row)
        => dataset with { TrainRows = [row] };

    private static DesignConversionDpoDataset BuildDataset(IReadOnlyList<DesignConversionStoredDpoExample> examples)
    {
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords(examples);
        Assert.True(build.IsSuccess, build.ErrorMessage);
        var dataset = new DesignConversionDpoDatasetBuilder().BuildDataset(
            build.Value!,
            new DesignConversionDpoDatasetOptions(
                DatasetId: "dcm-dpo-dataset-jsonl-isolation",
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1,
                Preflight: DesignConversionDpoDatasetPreflight.LocalFixtureTrainingCandidateOnly()));
        Assert.True(dataset.IsSuccess, dataset.ErrorMessage);
        return dataset.Value!;
    }

    private static DesignConversionDpoDataset BuildDataset(DesignConversionDpoTrainingRecord record)
    {
        var dataset = new DesignConversionDpoDatasetBuilder().BuildDataset(
            new DesignConversionDpoBuildResult([record], [], []),
            new DesignConversionDpoDatasetOptions(
                DatasetId: "dcm-dpo-dataset-jsonl-isolation-record",
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1,
                Preflight: DesignConversionDpoDatasetPreflight.LocalFixtureTrainingCandidateOnly()));
        Assert.True(dataset.IsSuccess, dataset.ErrorMessage);
        return dataset.Value!;
    }

    private static DesignConversionStoredDpoExample WithSource(
        DesignConversionStoredDpoExample example,
        string sourceId)
        => example with
        {
            Provenance = example.Provenance with
            {
                SourceRequestId = $"request:{sourceId}",
                SourceManifestRef = $"manifest://dcm-source/{sourceId}",
                Metadata = new Dictionary<string, string>
                {
                    ["source_design_id"] = sourceId
                }
            }
        };

    private sealed class ThrowBeyondSnapshotLimitCategories : IEnumerable<string>
    {
        public IEnumerator<string> GetEnumerator()
            => new GuardedEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();

        private sealed class GuardedEnumerator : IEnumerator<string>
        {
            private int _moveNextCount;

            public string Current => $"guarded-category-{_moveNextCount}";
            object System.Collections.IEnumerator.Current => Current;

            public bool MoveNext()
            {
                _moveNextCount++;
                if (_moveNextCount > DesignConversionDpoDatasetAuditProjection.MaximumInputDefectCategoryCount + 1)
                    throw new InvalidOperationException("Category projection enumerated beyond its bounded snapshot.");
                return true;
            }

            public void Reset() => _moveNextCount = 0;
            public void Dispose()
            {
            }
        }
    }
}
