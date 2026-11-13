// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;

public sealed class CodeDocumentationCertificationAdmissionTests
{
    private const string Domain = "code-documentation";
    private const string Pattern = "builder";
    private const string Subject = "api-reference";
    private const string Owner = "synthetic-source-owner";
    private static readonly DateTimeOffset Cut = new(2026, 9, 15, 12, 0, 0, TimeSpan.Zero);
    private static readonly CertificationHistoryCoverage Complete = new(
        CertificationHistoryCompleteness.Complete, CertificationHistoryCompleteness.Complete,
        CertificationHistoryCompleteness.Complete);

    [Fact]
    public void Admission_CompleteServerOwnedScopeBindsCheckpointCorpusAndConditions()
    {
        using var fixture = new Fixture();
        var input = fixture.Create();
        var before = fixture.SnapshotFiles();
        Assert.NotEmpty(before);
        var authority = Issue(input);
        var admitted = Admit(input, authority);
        Assert.Equal(CodeDocumentationCertificationAdmission.CurrentSchema, admitted.Schema);
        Assert.Equal("independent-certification-v1", admitted.Purpose);
        Assert.Equal(input.Scope.ScopeId, admitted.ScopeId);
        Assert.Equal(input.Scope.ScopeVersion, admitted.ScopeVersion);
        Assert.Equal(Cut, admitted.CoverageCutUtc);
        Assert.Equal(authority.ScopeIdentity, admitted.ScopeIdentitySha256);
        Assert.Equal(authority.IndexIdentity, admitted.ExposureIndexIdentitySha256);
        Assert.Equal(authority.ConditionsIdentity, admitted.EvaluationConditionsIdentitySha256);
        Assert.Equal(input.Index.Version, admitted.ExposureIndexVersion);
        Assert.Equal(input.Candidate, admitted.Candidate);
        Assert.Equal(input.Opened.Manifest, admitted.CorpusManifest);
        Assert.Equal(input.Opened.Corpus.EntryNames, admitted.CaseIds);
        Assert.Equal(input.Generation, admitted.GenerationSettings);
        Assert.Equal(CodeDocumentationSavedModelAssessmentService.ComputeGenerationSettingsSha256(
            input.Generation, input.Opened.Corpus.Entries), admitted.GenerationSettingsSha256);
        Assert.Equal(input.Evaluator.ModelId, admitted.Evaluator.ExpectedModelId);
        Assert.Equal(input.Evaluator.ExpectedOllamaManifestDigest, admitted.Evaluator.ExpectedOllamaManifestDigest);
        Assert.Equal(input.Evaluator.ExpectedOllamaModelLayerDigest, admitted.Evaluator.ExpectedOllamaModelLayerDigest);
        Assert.Empty(admitted.Evaluator.ObservedEvaluationRequestIds);
        Assert.Equal(MeasurementNoisePolicyConfig.Canonical.IdentitySha256, admitted.NoisePolicyIdentitySha256);
        Assert.Equal(5, admitted.RepeatCount);
        Assert.Equal(2d, admitted.StandardDeviationMultiplier);
        Assert.Equal(10, admitted.ExpectedEvaluationCallCount);
        Assert.Equal("code-documentation-case-pass-fraction-v1", admitted.MetricProfileId);
        Assert.Equal("code-documentation-strict-target-noise-v1", admitted.TargetPolicyId);
        Assert.Equal(.95d, admitted.Target);
        Assert.All(input.Opened.Corpus.Entries, entry =>
        {
            var binding = Assert.Single(input.Scope.CertificationSourceBindings, b => b.EntryId == entry.EntryId);
            var source = Assert.Single(input.Index.Sources, node => node.SourceEntityFingerprint == entry.SourceEntityFingerprint);
            Assert.Equal(entry.OwnerInput, source.SourceInput);
            Assert.Equal(binding.OwnerSnapshotId, source.OwnerSnapshotId);
            Assert.Equal(binding.OwnerSnapshotVersion, source.OwnerSnapshotVersion);
            Assert.Equal(CertificationLineageKind.KnownRoot, source.LineageKind);
        });
        var serialized = JsonSerializer.Serialize(admitted);
        Assert.DoesNotContain("CLOSED_CERT", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("CLOSED_REFERENCE", serialized, StringComparison.Ordinal);
        Assert.Equal(before, fixture.SnapshotFiles());
    }

    [Theory]
    [InlineData("training")]
    [InlineData("diagnostic")]
    [InlineData("teaching")]
    public void Admission_RelabeledDiagnosticSourceIsRejected(string exposure)
    {
        using var fixture = new Fixture();
        var original = fixture.Create();
        AssertClean(original);
        var disclosed = Entry("teaching", "same-topic separate teaching input", "old-reference");
        var relabeled = Entry("teaching", disclosed.OwnerInput, "renamed-reference");
        Assert.NotEqual(disclosed.EntryId, relabeled.EntryId);
        Assert.Equal(disclosed.SourceEntityFingerprint, relabeled.SourceEntityFingerprint);
        var changed = fixture.Create("renamed-v2", [relabeled, original.Opened.Corpus.Entries[1]], "renamed-exam");
        Assert.NotEqual(original.Opened.Corpus.CorpusName, changed.Opened.Corpus.CorpusName);
        Assert.NotEqual(original.Opened.Corpus.CorpusVersion, changed.Opened.Corpus.CorpusVersion);
        var sources = changed.Opened.Corpus.Entries.Select(entry => Node(entry,
            entry.SourceEntityFingerprint == relabeled.SourceEntityFingerprint ? Role(exposure) : CertificationExposureRole.ReferenceAcquisitionOnly));
        RejectFresh(WithSources(changed, sources), "SOURCE_OVERLAP");
    }

    [Theory]
    [InlineData("crlf")]
    [InlineData("nfc")]
    public void Admission_NormalizedInputAliasIsRejected(string normalization)
    {
        using var fixture = new Fixture();
        var input = fixture.Create();
        AssertClean(input);
        var cert = input.Opened.Corpus.Entries.Single(entry => entry.SourceEntityFingerprint == Fingerprint("cert-a"));
        var alias = normalization == "crlf" ? cert.OwnerInput.Replace("\n", "\r\n", StringComparison.Ordinal)
            : cert.OwnerInput.Normalize(NormalizationForm.FormD);
        Assert.NotEqual(cert.OwnerInput, alias);
        Assert.Equal(FrozenReferenceEntry.ComputeNormalizedTextSha256(cert.OwnerInput), FrozenReferenceEntry.ComputeNormalizedTextSha256(alias));
        var changed = Replace(input, "teaching", node => Copy(node, sourceInput: alias));
        Assert.NotEqual(cert.SourceEntityFingerprint, Find(changed, "teaching").SourceEntityFingerprint);
        RejectFresh(changed, "CONTENT_OVERLAP");
    }

    [Theory]
    [InlineData("direct")]
    [InlineData("transitive")]
    public void Admission_DeclaredDerivedSourceIsRejected(string lineage)
    {
        using var fixture = new Fixture();
        var input = fixture.Create();
        AssertClean(input);
        var ancestor = lineage == "direct" ? "teaching" : "middle";
        var changed = Replace(input, "cert-a", node => Copy(node, lineage: CertificationLineageKind.Derived, ancestors: [Fingerprint(ancestor)]));
        if (lineage == "transitive")
            changed = WithSources(changed, changed.Index.Sources.Append(Source("middle", ancestors: [Fingerprint("teaching")])));
        AssertNoDirectOverlap(changed);
        RejectFresh(changed, "LINEAGE_OVERLAP");
    }

    [Fact]
    public void Admission_CertificationAncestorOfExposedDerivedSourceIsRejected()
    {
        using var fixture = new Fixture();
        var input = fixture.Create();
        AssertClean(input);
        var changed = Replace(input, "teaching", node => Copy(node, lineage: CertificationLineageKind.Derived, ancestors: [Fingerprint("cert-a")]));
        Assert.Equal(CertificationLineageKind.KnownRoot, Find(changed, "cert-a").LineageKind);
        Assert.False(CertificationValidation.Forbidden(Find(changed, "cert-a").Roles));
        AssertNoDirectOverlap(changed);
        RejectFresh(changed, "LINEAGE_OVERLAP");
    }

    [Fact]
    public void Admission_SiblingSourcesWithCommonUnexposedRootAreRejected()
    {
        using var fixture = new Fixture();
        var input = fixture.Create();
        AssertClean(input);
        var changed = WithSources(input, input.Index.Sources.Append(Source("common-root")));
        changed = Replace(changed, "cert-a", node => Copy(node, lineage: CertificationLineageKind.Derived, ancestors: [Fingerprint("common-root")]));
        changed = Replace(changed, "teaching", node => Copy(node, lineage: CertificationLineageKind.Derived, ancestors: [Fingerprint("common-root")]));
        Assert.False(CertificationValidation.Forbidden(Find(changed, "common-root").Roles));
        AssertNoDirectOverlap(changed);
        RejectFresh(changed, "LINEAGE_OVERLAP");
    }

    [Theory]
    [InlineData("missing-category")]
    [InlineData("unknown-exposure")]
    [InlineData("omitted-owner-version")]
    [InlineData("candidate-lineage-gap")]
    [InlineData("covered-checkpoint-gap")]
    [InlineData("missing-ancestor")]
    [InlineData("unknown-ancestry")]
    [InlineData("cycle")]
    [InlineData("derived-empty")]
    [InlineData("root-with-ancestor")]
    [InlineData("duplicate-source")]
    [InlineData("conflicting-source")]
    [InlineData("missing-binding")]
    [InlineData("duplicate-binding")]
    [InlineData("attested-empty-exposed")]
    public void Admission_UnknownOrIncompleteHistoryFailsClosed(string defect)
    {
        using var fixture = new Fixture();
        var input = fixture.Create();
        AssertClean(input);
        var changed = input;
        switch (defect)
        {
            case "missing-category":
                changed = WithScope(input, Scope(input, history: new(CertificationHistoryCompleteness.Unknown,
                    CertificationHistoryCompleteness.Complete, CertificationHistoryCompleteness.Complete)));
                break;
            case "unknown-exposure": changed = Replace(input, "cert-a", n => Copy(n, roles: CertificationExposureRole.Unknown)); break;
            case "omitted-owner-version": changed = WithScope(input, Scope(input, owners: [new(Owner, "")])); break;
            case "candidate-lineage-gap": changed = WithScope(input, Scope(input, lineage: [Hex('e')])); break;
            case "covered-checkpoint-gap": changed = input with { Index = Index(input.Scope, input.Index.Sources, checkpoint: Hex('e')) }; break;
            case "missing-ancestor": changed = Replace(input, "cert-a", n => Copy(n, lineage: CertificationLineageKind.Derived, ancestors: [Fingerprint("missing")])); break;
            case "unknown-ancestry": changed = Replace(input, "cert-a", n => Copy(n, lineage: CertificationLineageKind.Unknown)); break;
            case "cycle": changed = Replace(input, "cert-a", n => Copy(n, lineage: CertificationLineageKind.Derived, ancestors: [n.SourceEntityFingerprint])); break;
            case "derived-empty": changed = Replace(input, "cert-a", n => Copy(n, lineage: CertificationLineageKind.Derived)); break;
            case "root-with-ancestor": changed = Replace(input, "cert-a", n => Copy(n, ancestors: [Fingerprint("teaching")])); break;
            case "duplicate-source": changed = WithSources(input, input.Index.Sources.Append(Find(input, "cert-a"))); break;
            case "conflicting-source": changed = WithSources(input, input.Index.Sources.Append(Copy(Find(input, "cert-a"), sourceInput: "conflicting source bytes"))); break;
            case "missing-binding": changed = WithScope(input, Scope(input, bindings: input.Scope.CertificationSourceBindings.Skip(1))); break;
            case "duplicate-binding": changed = WithScope(input, Scope(input, bindings: input.Scope.CertificationSourceBindings.Append(input.Scope.CertificationSourceBindings[0]))); break;
            case "attested-empty-exposed": changed = WithScope(input, Scope(input, history: new(CertificationHistoryCompleteness.Complete,
                CertificationHistoryCompleteness.Complete, CertificationHistoryCompleteness.AttestedEmpty))); break;
            default: throw new ArgumentOutOfRangeException(nameof(defect));
        }
        RejectFresh(changed, "PROVENANCE_INCOMPLETE");
    }

    [Theory]
    [InlineData("missing-cert-node")]
    [InlineData("input-mismatch")]
    [InlineData("undeclared-owner-version")]
    public void Admission_EveryCertificationRowRequiresMatchingCoveredIndexNode(string defect)
    {
        using var fixture = new Fixture();
        var input = fixture.Create();
        AssertClean(input);
        var changed = defect switch
        {
            "missing-cert-node" => WithSources(input, input.Index.Sources.Where(n => n.SourceEntityFingerprint != Fingerprint("cert-a"))),
            "input-mismatch" => Replace(input, "cert-a", n => Copy(n, sourceInput: "different actual input")),
            "undeclared-owner-version" => Replace(input, "cert-a", n => Copy(n, ownerVersion: "v2")),
            _ => throw new ArgumentOutOfRangeException(nameof(defect))
        };
        Assert.Equal(CertificationHistoryCompleteness.Complete, changed.Scope.History!.Training);
        Assert.Equal(CertificationHistoryCompleteness.Complete, changed.Scope.History.Diagnostic);
        Assert.Equal(CertificationHistoryCompleteness.Complete, changed.Scope.History.Teaching);
        RejectFresh(changed, "PROVENANCE_INCOMPLETE");
    }

    [Fact]
    public void Admission_CallerClaimsCannotMintOrReplaceAuthority()
    {
        using var fixture = new Fixture();
        var input = fixture.Create();
        var authority = Issue(input);
        Admit(input, authority);
        AssertFailure(CodeDocumentationCertificationAdmission.TryCreate(null, input.Candidate, input.Opened, input.Index), "AUTHORITY_REQUIRED");
        foreach (var type in new[] { typeof(ServerOwnedCodeDocumentationCertificationScope), typeof(CodeDocumentationCertificationAdmission) })
        {
            Assert.True(type.IsSealed);
            Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
            Assert.DoesNotContain(type.GetMethods(BindingFlags.Public | BindingFlags.Static), m => m.ReturnType == type);
            Assert.All(type.GetProperties(BindingFlags.Public | BindingFlags.Instance), p => Assert.Null(p.SetMethod));
            Assert.Null(type.GetMethod("<Clone>$"));
        }
        Assert.False(typeof(ServerOwnedCodeDocumentationCertificationScope).IsPublic);
        Assert.Throws<NotSupportedException>(() => JsonSerializer.Deserialize<ServerOwnedCodeDocumentationCertificationScope>("{\"clean\":true}"));
        Assert.Throws<NotSupportedException>(() => JsonSerializer.Deserialize<CodeDocumentationCertificationAdmission>("{\"clean\":true}"));
        var changedIndex = Replace(input, "teaching", n => Copy(n, sourceInput: "independently changed teaching input"));
        var otherAuthority = Issue(changedIndex);
        Admit(changedIndex, otherAuthority);
        Assert.NotEqual(authority.IndexIdentity, otherAuthority.IndexIdentity);
        AssertFailure(CodeDocumentationCertificationAdmission.TryCreate(authority, input.Candidate, input.Opened, changedIndex.Index), "SCOPE_MISMATCH");
        // This is a genuinely re-sealed new corpus, not a fabricated sealer success.
        var other = fixture.Create("other-v2", [Entry("cert-a", "changed closed input"), Entry("cert-b", "second closed input")]);
        AssertClean(other);
        Assert.NotEqual(input.Opened.Manifest.ManifestSha256, other.Opened.Manifest.ManifestSha256);
        AssertFailure(CodeDocumentationCertificationAdmission.TryCreate(authority, input.Candidate, other.Opened, input.Index), "CORPUS_MISMATCH");
    }

    [Theory]
    [InlineData("domain")]
    [InlineData("pattern")]
    [InlineData("subject")]
    [InlineData("scope-version")]
    [InlineData("source-owner-version")]
    [InlineData("coverage-revision")]
    public void Admission_ScopeOrVersionDriftIsRejected(string field)
    {
        using var fixture = new Fixture();
        var input = fixture.Create();
        var authority = Issue(input);
        Admit(input, authority);
        var scope = field switch
        {
            "domain" => Scope(input, domain: "other-domain"),
            "pattern" => Scope(input, pattern: "other-pattern"),
            "subject" => Scope(input, subject: "other-subject"),
            "scope-version" => Scope(input, version: "v2"),
            "source-owner-version" => Scope(input, owners: [new(Owner, "v2")]),
            "coverage-revision" => Scope(input, revision: "coverage-v2"),
            _ => throw new ArgumentOutOfRangeException(nameof(field))
        };
        var changed = WithScope(input, scope);
        AssertFailure(CodeDocumentationCertificationAdmission.TryCreate(authority, input.Candidate, input.Opened, changed.Index), "SCOPE_MISMATCH");
    }

    [Theory]
    [InlineData("step")]
    [InlineData("manifest")]
    [InlineData("weights")]
    [InlineData("source-kind")]
    [InlineData("model-id")]
    [InlineData("model-version")]
    [InlineData("weight-length")]
    [InlineData("manifest-path")]
    [InlineData("weights-path")]
    [InlineData("corpus")]
    [InlineData("payload")]
    [InlineData("corpus-version")]
    [InlineData("index-content")]
    [InlineData("index-version")]
    public void Admission_CandidateCorpusOrIndexDriftCannotReuseAuthority(string field)
    {
        using var fixture = new Fixture();
        var input = fixture.Create();
        var authority = Issue(input);
        Admit(input, authority);
        var candidate = input.Candidate;
        var opened = input.Opened;
        var index = input.Index;
        var expected = "CANDIDATE_MISMATCH";
        switch (field)
        {
            case "step": candidate = candidate with { CheckpointStep = 2 }; break;
            case "manifest": candidate = candidate with { ManifestSha256 = Hex('e') }; break;
            case "weights": candidate = candidate with { WeightsSha256 = Hex('e') }; break;
            case "source-kind": candidate = candidate with { ModelSourceKind = CodeDocumentationSavedModelAssessmentModelPassport.TrialTrainingAttemptCheckpointSourceKind }; break;
            case "model-id": candidate = candidate with { ModelIdSlug = "another-model" }; break;
            case "model-version": candidate = candidate with { ModelVersion = "v2" }; break;
            case "weight-length": candidate = candidate with { WeightsByteLength = 65 }; break;
            case "manifest-path": candidate = candidate with { RepositoryRelativeManifestPath = "models/other/manifest.json" }; break;
            case "weights-path": candidate = candidate with { RepositoryRelativeWeightsPath = "models/other/weights.bin" }; break;
            case "corpus": opened = opened with { Manifest = opened.Manifest with { CorpusSha256 = Fingerprint("forged-corpus") } }; expected = "CORPUS_MISMATCH"; break;
            case "payload": opened = opened with { Manifest = opened.Manifest with { PayloadSha256 = Hex('e') } }; expected = "CORPUS_MISMATCH"; break;
            case "corpus-version": opened = opened with { Manifest = opened.Manifest with { CorpusVersion = "v2" } }; expected = "CORPUS_MISMATCH"; break;
            case "index-content": index = Replace(input, "teaching", n => Copy(n, sourceInput: "new input")).Index; expected = "SCOPE_MISMATCH"; break;
            case "index-version": index = Index(input.Scope, input.Index.Sources, version: "v2"); expected = "SCOPE_MISMATCH"; break;
            default: throw new ArgumentOutOfRangeException(nameof(field));
        }
        AssertFailure(CodeDocumentationCertificationAdmission.TryCreate(authority, candidate, opened, index), expected);
    }

    [Fact]
    public void Admission_FixedEvaluationProfileIsBoundWithoutCallerOverride()
    {
        using var fixture = new Fixture();
        var admitted = AssertClean(fixture.Create());
        Assert.Equal(LocalQwenOllamaInferenceProfileAuthority.Evaluation.ProfileId, admitted.Evaluator.EvaluationProfileId);
        Assert.Equal(LocalQwenOllamaInferenceProfileAuthority.Evaluation.ConfigurationSha256, admitted.Evaluator.EvaluationProfileConfigurationSha256);
        var issue = typeof(ServerOwnedCodeDocumentationCertificationScope).GetMethod("CreateFromServerOwnedSnapshot", BindingFlags.NonPublic | BindingFlags.Static)!;
        var admit = typeof(CodeDocumentationCertificationAdmission).GetMethod("TryCreate", BindingFlags.NonPublic | BindingFlags.Static)!;
        Assert.DoesNotContain(issue.GetParameters().Concat(admit.GetParameters()), p => p.Name!.Contains("profile", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("evaluator-model")]
    [InlineData("evaluator-manifest")]
    [InlineData("evaluator-layer")]
    [InlineData("max-new-tokens")]
    [InlineData("temperature")]
    [InlineData("top-k")]
    [InlineData("top-p")]
    [InlineData("seed")]
    [InlineData("max-held-out-cases")]
    [InlineData("max-answer-chars")]
    [InlineData("budget")]
    public void Admission_ValidEvaluatorOrGenerationChangeCreatesNewIdentity(string condition)
    {
        using var fixture = new Fixture();
        var input = fixture.Create();
        var original = AssertClean(input);
        var changed = condition switch
        {
            "evaluator-model" => input with { Evaluator = Evaluator(model: "qwen-assessment-other:9b") },
            "evaluator-manifest" => input with { Evaluator = Evaluator(manifest: Hex('e')) },
            "evaluator-layer" => input with { Evaluator = Evaluator(layer: Hex('e')) },
            "max-new-tokens" => input with { Generation = input.Generation with { MaxNewTokens = 33 } },
            "temperature" => input with { Generation = input.Generation with { Temperature = .2f } },
            "top-k" => input with { Generation = input.Generation with { TopK = 1 } },
            "top-p" => input with { Generation = input.Generation with { TopP = .8f } },
            "seed" => input with { Generation = input.Generation with { Seed = 8 } },
            "max-held-out-cases" => input with { Generation = input.Generation with { MaxHeldOutCases = 3 } },
            "max-answer-chars" => input with { Generation = input.Generation with { MaxAnswerChars = 1024 } },
            "budget" => input with { Evaluator = Evaluator(budget: 101) },
            _ => throw new ArgumentOutOfRangeException(nameof(condition))
        };
        var admitted = AssertClean(changed);
        Assert.NotEqual(original.EvaluationConditionsIdentitySha256, admitted.EvaluationConditionsIdentitySha256);
        Assert.NotEqual(original.AdmissionIdentitySha256, admitted.AdmissionIdentitySha256);
        Assert.Equal(original.Evaluator.EvaluationProfileId, admitted.Evaluator.EvaluationProfileId);
        Assert.Equal(original.Evaluator.EvaluationProfileConfigurationSha256, admitted.Evaluator.EvaluationProfileConfigurationSha256);
    }

    [Theory]
    [InlineData("repeat-count")]
    [InlineData("multiplier")]
    public void Admission_NoncanonicalNoisePolicyIsRejected(string condition)
    {
        using var fixture = new Fixture();
        var input = fixture.Create();
        AssertClean(input);
        var noise = condition == "repeat-count" ? new MeasurementNoisePolicyConfig(6, 2) : new MeasurementNoisePolicyConfig(5, 3);
        AssertFailure(CreateAuthority(input with { Noise = noise }), "CONDITIONS_MISMATCH");
    }

    [Fact]
    public void Admission_DistinctTeachingTaskAndPaidEvaluationReferenceRemainEligible()
    {
        using var fixture = new Fixture();
        var input = fixture.Create();
        var teaching = Entry("teaching", "same-topic separate teaching input");
        Assert.All(input.Opened.Corpus.Entries, entry =>
        {
            Assert.Equal(teaching.PaidModelReferenceAnswer, entry.PaidModelReferenceAnswer);
            Assert.Equal(teaching.DomainTag, entry.DomainTag);
            Assert.Equal(teaching.SubjectId, entry.SubjectId);
            Assert.NotEqual(teaching.SourceEntityFingerprint, entry.SourceEntityFingerprint);
            Assert.True(entry.PaidReferenceProvenance.ClosedPaidReference);
            Assert.True(entry.PaidReferenceProvenance.PaidReferenceOutputNeverTrainable);
            Assert.True(entry.PaidReferenceProvenance.PaidReferenceOutputNeverDpoPairSide);
        });
        AssertClean(input);
        var distinctRoots = WithSources(input, input.Index.Sources.Concat([Source("cert-root"), Source("teaching-root")]));
        distinctRoots = Replace(distinctRoots, "cert-a", n => Copy(n, lineage: CertificationLineageKind.Derived, ancestors: [Fingerprint("cert-root")]));
        distinctRoots = Replace(distinctRoots, "teaching", n => Copy(n, lineage: CertificationLineageKind.Derived, ancestors: [Fingerprint("teaching-root")]));
        AssertClean(distinctRoots);
    }

    [Theory]
    [InlineData("training")]
    [InlineData("diagnostic")]
    [InlineData("teaching")]
    public void Admission_ReferenceAcquisitionRoleCannotCancelForbiddenExposure(string exposure)
    {
        using var fixture = new Fixture();
        var input = fixture.Create();
        Assert.Equal(CertificationExposureRole.ReferenceAcquisitionOnly, Find(input, "cert-a").Roles);
        AssertClean(input);
        RejectFresh(Replace(input, "cert-a", n => Copy(n, roles: n.Roles | Role(exposure))), "SOURCE_OVERLAP");
    }

    [Fact]
    public void Admission_MutableInputsCannotChangeIssuedIdentity()
    {
        using var fixture = new Fixture();
        var input = fixture.Create();
        var ancestors = new List<string> { Fingerprint("private-root") };
        var sources = input.Index.Sources.Select(n => n.SourceEntityFingerprint == Fingerprint("cert-a")
            ? Copy(n, lineage: CertificationLineageKind.Derived, ancestors: ancestors) : n).Append(Source("private-root")).ToArray();
        var owners = new[] { new CertificationOwnerSnapshot(Owner, "v1") };
        var bindings = input.Scope.CertificationSourceBindings.ToArray();
        var lineage = new[] { input.Candidate.ManifestSha256 };
        var changed = WithScope(input, Scope(input, owners: owners, bindings: bindings, lineage: lineage));
        changed = WithSources(changed, sources);
        var authority = Issue(changed);
        var before = Admit(changed, authority);
        ancestors.Add(Fingerprint("teaching"));
        owners[0] = new(Owner, "v2");
        bindings[0] = new(bindings[0].EntryId, Fingerprint("wrong"), Owner, "v2");
        lineage[0] = Hex('e');
        sources[0] = Copy(sources[0], sourceInput: "caller mutation");
        var after = Admit(changed, authority);
        Assert.Equal(before.AdmissionIdentitySha256, after.AdmissionIdentitySha256);
        Assert.Single(Find(changed, "cert-a").Ancestors);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)after.CaseIds)[0] = "mutated");
        Assert.Throws<NotSupportedException>(() => ((IList<string>)Find(changed, "cert-a").Ancestors).Add("mutated"));
        Assert.Throws<NotSupportedException>(() => ((IList<CertificationSourceBinding>)changed.Scope.CertificationSourceBindings)[0] = bindings[0]);
        AssertFailure(CodeDocumentationCertificationAdmission.TryCreate(authority, changed.Candidate, changed.Opened,
            Index(changed.Scope, sources)), "SCOPE_MISMATCH");
        var alternateCandidate = after.Candidate with { CheckpointStep = 2 };
        Assert.Equal(1, after.Candidate.CheckpointStep);
        AssertFailure(CodeDocumentationCertificationAdmission.TryCreate(authority, alternateCandidate, changed.Opened, changed.Index), "CANDIDATE_MISMATCH");
    }

    [Fact]
    public void Admission_CanonicalOrderingIsStableAndChangesAreBound()
    {
        using var fixture = new Fixture();
        var input = fixture.Create();
        input = WithSources(input, input.Index.Sources.Concat([Source("root-a"), Source("root-b")]));
        input = Replace(input, "cert-a", n => Copy(n, lineage: CertificationLineageKind.Derived, ancestors: [Fingerprint("root-a"), Fingerprint("root-b")]));
        input = WithScope(input, Scope(input, owners: [new(Owner, "v1"), new("other-owner", "v1")], lineage: [Hex('a'), Hex('e')]));
        var first = AssertClean(input);
        var reordered = WithScope(input, Scope(input, owners: input.Scope.OwnerSnapshots.Reverse(),
            bindings: input.Scope.CertificationSourceBindings.Reverse(), lineage: input.Scope.CheckpointLineageManifestIdentities.Reverse()));
        reordered = WithSources(reordered, input.Index.Sources.Reverse().Select(n => Copy(n, ancestors: n.Ancestors.Reverse())));
        reordered = reordered with { Opened = fixture.Open("v1", input.Opened.Corpus.Entries.Reverse(), "synthetic-certification") };
        var second = AssertClean(reordered);
        Assert.Equal(first.ScopeIdentitySha256, second.ScopeIdentitySha256);
        Assert.Equal(first.ExposureIndexIdentitySha256, second.ExposureIndexIdentitySha256);
        Assert.Equal(first.EvaluationConditionsIdentitySha256, second.EvaluationConditionsIdentitySha256);
        Assert.Equal(first.AdmissionIdentitySha256, second.AdmissionIdentitySha256);
        foreach (var changed in new[]
        {
            Replace(input, "teaching", n => Copy(n, sourceInput: "different separate teaching input")),
            Replace(input, "teaching", n => Copy(n, roles: CertificationExposureRole.Training)),
            Replace(input, "cert-a", n => Copy(n, ancestors: [Fingerprint("root-a")])),
            WithScope(input, Scope(input, revision: "coverage-v2")),
            input with { Candidate = input.Candidate with { CheckpointStep = 2 } }
        })
            Assert.NotEqual(first.AdmissionIdentitySha256, AssertClean(changed).AdmissionIdentitySha256);
        var changedCorpus = fixture.Create("v2", [Entry("cert-a", "new closed input"), Entry("cert-b", "second closed input")]);
        Assert.NotEqual(first.AdmissionIdentitySha256, AssertClean(changedCorpus).AdmissionIdentitySha256);
        RejectFresh(WithScope(input, Scope(input, purpose: "diagnostic-only")), "PURPOSE_INVALID");
    }

    [Theory]
    [InlineData("empty-factory")]
    [InlineData("multi-scope-factory")]
    [InlineData("train-only")]
    [InlineData("mixed-partitions")]
    [InlineData("duplicate-cert-source")]
    [InlineData("duplicate-cert-content")]
    [InlineData("source-limit")]
    [InlineData("edge-limit")]
    [InlineData("invalid-source-hash")]
    [InlineData("invalid-unicode")]
    [InlineData("invalid-origin")]
    [InlineData("index-schema")]
    [InlineData("null-index")]
    public void Admission_InvalidClosedSetAndBoundsAreRejected(string defect)
    {
        using var fixture = new Fixture();
        var input = fixture.Create();
        AssertClean(input);
        if (defect is "empty-factory" or "multi-scope-factory")
        {
            var rejected = FrozenReferenceCorpus.Build("invalid", "v1", Cut,
                defect == "empty-factory" ? Array.Empty<FrozenReferenceEntry>() :
                [Entry("one", "one"), Entry("two", "two", subject: "other-subject")]);
            Assert.False(rejected.IsSuccess);
            Assert.Null(rejected.Value);
            Assert.Equal(defect == "empty-factory" ? "FROZEN_REFERENCE_CORPUS_EMPTY" : "FROZEN_REFERENCE_CORPUS_SUBJECT_MISMATCH", rejected.ErrorCode);
            return; // These are explicitly F factory tests, not admission-path evidence.
        }
        if (defect == "null-index")
        {
            AssertFailure(CodeDocumentationCertificationAdmission.TryCreate(Issue(input), input.Candidate, input.Opened, null), "INPUT_INVALID");
            return;
        }
        if (defect is "train-only" or "mixed-partitions" or "duplicate-cert-source" or "duplicate-cert-content")
        {
            FrozenReferenceEntry[] entries = defect switch
            {
                "train-only" => new[] { Entry("one", "one", partition: FrozenReferencePartition.Train) },
                "mixed-partitions" => [Entry("one", "one", partition: FrozenReferencePartition.Train), Entry("two", "two")],
                "duplicate-cert-source" => [Entry("one", "one"), Entry("one", "two")],
                _ => [Entry("one", "same"), Entry("two", "same")]
            };
            var changed = fixture.Create("invalid-set", entries);
            if (defect == "train-only")
                AssertFailure(CreateAuthority(changed), "CONDITIONS_MISMATCH");
            else
                RejectFresh(changed, "CORPUS_MISMATCH");
            return;
        }
        var malformed = defect switch
        {
            "source-limit" => WithSources(input, Enumerable.Range(0, 10_001).Select(i => Source("source-" + i))),
            "edge-limit" => Replace(input, "cert-a", n => Copy(n, lineage: CertificationLineageKind.Derived,
                ancestors: Enumerable.Repeat(Fingerprint("teaching"), 100_001))),
            "invalid-source-hash" => Replace(input, "cert-a", n => Copy(n, fingerprint: "not-a-hash")),
            "invalid-unicode" => Replace(input, "cert-a", n => Copy(n, sourceInput: "\ud800")),
            "invalid-origin" => Replace(input, "cert-a", n => Copy(n, evidence: "untrusted-origin")),
            "index-schema" => input with { Index = new("wrong-schema", "v1", input.Scope, Hex('a'), Complete, input.Index.Sources) },
            _ => throw new ArgumentOutOfRangeException(nameof(defect))
        };
        AssertFailure(CreateAuthority(malformed), "INPUT_INVALID");
        if (defect == "source-limit")
            Assert.Empty(malformed.Index.Sources); // No admitted partial prefix.
        if (defect == "edge-limit")
        {
            Assert.Empty(Find(malformed, "cert-a").Ancestors);
            // The aggregate bound also applies when every individual node is within its bound.
            var sharedEdges = Enumerable.Repeat(Fingerprint("teaching"), 10_000).ToArray();
            var aggregateOverLimit = WithSources(input, Enumerable.Range(0, 11)
                .Select(i => Source("bounded-node-" + i, sharedEdges)));
            Assert.All(aggregateOverLimit.Index.Sources, n => Assert.Equal(10_000, n.Ancestors.Count));
            AssertFailure(CreateAuthority(aggregateOverLimit), "INPUT_INVALID");
        }
    }

    [Theory]
    [InlineData("tokens-zero")]
    [InlineData("tokens-over")]
    [InlineData("temperature-nan")]
    [InlineData("temperature-over")]
    [InlineData("top-k-negative")]
    [InlineData("top-k-over")]
    [InlineData("top-p-zero")]
    [InlineData("top-p-infinity")]
    [InlineData("held-out-zero")]
    [InlineData("held-out-over")]
    [InlineData("answer-zero")]
    [InlineData("answer-over")]
    [InlineData("call-budget")]
    public void Admission_InvalidGenerationBoundsAreRejected(string boundary)
    {
        using var fixture = new Fixture();
        var input = fixture.Create();
        AssertClean(input);
        var settings = input.Generation;
        var changed = boundary switch
        {
            "tokens-zero" => settings with { MaxNewTokens = 0 },
            "tokens-over" => settings with { MaxNewTokens = 2049 },
            "temperature-nan" => settings with { Temperature = float.NaN },
            "temperature-over" => settings with { Temperature = 2.1f },
            "top-k-negative" => settings with { TopK = -1 },
            "top-k-over" => settings with { TopK = 10001 },
            "top-p-zero" => settings with { TopP = 0f },
            "top-p-infinity" => settings with { TopP = float.PositiveInfinity },
            "held-out-zero" => settings with { MaxHeldOutCases = 0 },
            "held-out-over" => settings with { MaxHeldOutCases = 201 },
            "answer-zero" => settings with { MaxAnswerChars = 0 },
            "answer-over" => settings with { MaxAnswerChars = 65537 },
            "call-budget" => settings,
            _ => throw new ArgumentOutOfRangeException(nameof(boundary))
        };
        AssertFailure(CreateAuthority(input with { Generation = changed,
            Evaluator = boundary == "call-budget" ? Evaluator(budget: 9) : input.Evaluator }), "CONDITIONS_MISMATCH");
    }

    [Fact]
    public void Admission_SnapshotFailureDoesNotSwallowCancellationOrFatalExceptions()
    {
        using var fixture = new Fixture();
        var input = fixture.Create();
        AssertClean(input);
        var cancellation = new OperationCanceledException("synthetic cancellation");
        var aggregate = new AggregateException(cancellation);
        Assert.Same(aggregate, Assert.Throws<AggregateException>(() => WithSources(input, ThrowingSources(aggregate))));
        var disposalCount = 0;
        Assert.Same(cancellation, Assert.Throws<OperationCanceledException>(() => WithSources(input,
            new CancellationAndThrowingDisposal(cancellation, () => disposalCount++))));
        Assert.Equal(1, disposalCount);
        var fatal = new OutOfMemoryException("synthetic fatal; no allocation pressure");
        Assert.Same(fatal, Assert.Throws<OutOfMemoryException>(() => WithSources(input, ThrowingSources(fatal))));
        var invalid = WithSources(input, ThrowingSources(new InvalidOperationException("synthetic enumeration failure")));
        Assert.Empty(invalid.Index.Sources);
        AssertFailure(CreateAuthority(invalid), "INPUT_INVALID");
    }

    private static IEnumerable<CodeDocumentationCertificationSource> ThrowingSources(Exception exception)
    {
        yield return Source("first");
        throw exception;
    }

    private sealed class CancellationAndThrowingDisposal(OperationCanceledException cancellation, Action disposed)
        : IEnumerable<CodeDocumentationCertificationSource>, IEnumerator<CodeDocumentationCertificationSource>
    {
        public CodeDocumentationCertificationSource Current => throw new InvalidOperationException("No current item.");
        object System.Collections.IEnumerator.Current => Current;
        public IEnumerator<CodeDocumentationCertificationSource> GetEnumerator() => this;
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        public bool MoveNext() => throw cancellation;
        public void Reset() => throw new NotSupportedException();
        public void Dispose()
        {
            disposed();
            throw new InvalidOperationException("Secondary synthetic disposal failure.");
        }
    }

    private static void AssertNoDirectOverlap(Scenario input)
    {
        var cert = input.Opened.Corpus.Entries;
        var exposed = input.Index.Sources.Where(n => CertificationValidation.Forbidden(n.Roles)).ToArray();
        Assert.NotEmpty(exposed);
        Assert.All(cert, entry => Assert.All(exposed, node =>
        {
            Assert.NotEqual(entry.SourceEntityFingerprint, node.SourceEntityFingerprint);
            Assert.NotEqual(FrozenReferenceEntry.ComputeNormalizedTextSha256(entry.OwnerInput), FrozenReferenceEntry.ComputeNormalizedTextSha256(node.SourceInput));
        }));
    }

    private static CodeDocumentationCertificationAdmission AssertClean(Scenario input) => Admit(input, Issue(input));

    private static ServerOwnedCodeDocumentationCertificationScope Issue(Scenario input) => Require(CreateAuthority(input));

    private static OperationResult<ServerOwnedCodeDocumentationCertificationScope> CreateAuthority(Scenario input)
        => ServerOwnedCodeDocumentationCertificationScope.CreateFromServerOwnedSnapshot(
            input.Scope, input.Candidate, input.Opened, input.Generation, input.Evaluator, input.Noise, input.Index);

    private static CodeDocumentationCertificationAdmission Admit(Scenario input, ServerOwnedCodeDocumentationCertificationScope authority)
        => Require(CodeDocumentationCertificationAdmission.TryCreate(authority, input.Candidate, input.Opened, input.Index));

    private static void RejectFresh(Scenario input, string suffix)
    {
        var authority = Issue(input);
        Assert.Equal(input.Candidate, authority.Candidate);
        Assert.Equal(input.Opened.Manifest, authority.Corpus.Manifest);
        Assert.Equal(CertificationIdentity.Index(input.Index), authority.IndexIdentity);
        AssertFailure(CodeDocumentationCertificationAdmission.TryCreate(authority, input.Candidate, input.Opened, input.Index), suffix);
    }

    private static void AssertFailure<T>(OperationResult<T> result, string suffix)
    {
        Assert.False(result.IsSuccess, $"Unexpected admission: {result.ErrorCode}; {result.ErrorMessage}");
        Assert.Null(result.Value);
        Assert.Equal(CodeDocumentationCertificationAdmission.ErrorPrefix + suffix, result.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.DoesNotContain("CLOSED_CERT", result.ErrorMessage, StringComparison.Ordinal);
    }

    private static T Require<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.NotNull(result.Value);
        return result.Value!;
    }

    private static Scenario WithSources(Scenario input, IEnumerable<CodeDocumentationCertificationSource> sources)
        => input with { Index = Index(input.Scope, sources, history: input.Index.History) };

    private static Scenario WithScope(Scenario input, CodeDocumentationCertificationScopeDeclaration scope)
        => input with { Scope = scope, Index = Index(scope, input.Index.Sources, history: scope.History) };

    private static Scenario Replace(Scenario input, string seed, Func<CodeDocumentationCertificationSource, CodeDocumentationCertificationSource> replace)
        => WithSources(input, input.Index.Sources.Select(n => n.SourceEntityFingerprint == Fingerprint(seed) ? replace(n) : n));

    private static CodeDocumentationCertificationSource Find(Scenario input, string seed)
        => Assert.Single(input.Index.Sources, node => node.SourceEntityFingerprint == Fingerprint(seed));

    private static CodeDocumentationCertificationSource Copy(CodeDocumentationCertificationSource node,
        string? sourceInput = null, string? ownerVersion = null, CertificationExposureRole? roles = null,
        CertificationLineageKind? lineage = null, IEnumerable<string>? ancestors = null,
        string? fingerprint = null, string? evidence = null)
        => new(fingerprint ?? node.SourceEntityFingerprint, node.Domain, node.Subject, node.OwnerSnapshotId,
            ownerVersion ?? node.OwnerSnapshotVersion, sourceInput ?? node.SourceInput, evidence ?? node.OriginEvidenceRef,
            roles ?? node.Roles, lineage ?? node.LineageKind, ancestors ?? node.Ancestors);

    private static CodeDocumentationCertificationSource Node(FrozenReferenceEntry entry,
        CertificationExposureRole roles = CertificationExposureRole.ReferenceAcquisitionOnly)
        => new(entry.SourceEntityFingerprint, entry.DomainTag, entry.SubjectId, Owner, "v1", entry.OwnerInput,
            Fingerprint("origin"), roles, CertificationLineageKind.KnownRoot, []);

    private static CodeDocumentationCertificationSource Source(string seed, IEnumerable<string>? ancestors = null)
        => new(Fingerprint(seed), Domain, Subject, Owner, "v1", "separate source input " + seed, Fingerprint("origin"),
            CertificationExposureRole.None, ancestors is null ? CertificationLineageKind.KnownRoot : CertificationLineageKind.Derived,
            ancestors ?? []);

    private static CertificationExposureRole Role(string name) => name switch
    {
        "training" => CertificationExposureRole.Training,
        "diagnostic" => CertificationExposureRole.TeacherVisibleDiagnostic,
        "teaching" => CertificationExposureRole.AuthoringOrTeaching,
        _ => throw new ArgumentOutOfRangeException(nameof(name))
    };

    private static CodeDocumentationCertificationScopeDeclaration Scope(Scenario input,
        string domain = Domain, string pattern = Pattern, string subject = Subject, string version = "v1",
        string revision = "coverage-v1", string purpose = "independent-certification-v1",
        IEnumerable<CertificationOwnerSnapshot>? owners = null, IEnumerable<CertificationSourceBinding>? bindings = null,
        IEnumerable<string>? lineage = null, CertificationHistoryCoverage? history = null)
        => new(CodeDocumentationCertificationScopeDeclaration.CurrentSchema, domain, pattern, subject,
            "synthetic-certification-scope", version, purpose, Cut, revision,
            lineage ?? input.Scope.CheckpointLineageManifestIdentities,
            owners ?? input.Scope.OwnerSnapshots, history ?? input.Scope.History,
            bindings ?? input.Scope.CertificationSourceBindings);

    private static CodeDocumentationCertificationExposureIndex Index(CodeDocumentationCertificationScopeDeclaration scope,
        IEnumerable<CodeDocumentationCertificationSource> sources, string version = "v1", string? checkpoint = null,
        CertificationHistoryCoverage? history = null)
        => new(CodeDocumentationCertificationExposureIndex.CurrentSchema, version, scope,
            checkpoint ?? Hex('a'), history ?? Complete, sources);

    private static ContinuousLearningTp1DpoConfiguration Evaluator(string model = "qwen-assessment-test:9b",
        string? manifest = null, string? layer = null, int budget = 100)
        => Require(ContinuousLearningTp1DpoConfiguration.Create(model, manifest ?? Hex('c'), layer ?? Hex('d'), budget,
            "sha256:" + Hex('e'), "sha256:" + Hex('f'), Enumerable.Range(1, 32).Select(static i => (byte)i).ToArray()));

    private static FrozenReferenceEntry Entry(string seed, string input, string reference = "CLOSED_REFERENCE: shared short answer",
        string subject = Subject, FrozenReferencePartition partition = FrozenReferencePartition.HeldOut)
    {
        var provenance = Require(PaidReferenceProvenance.Create("synthetic-paid-provider", "synthetic-paid-model", "v1",
            "request-" + seed, Cut.AddHours(-1), FrozenReferenceEntry.ComputeNormalizedTextSha256(reference)));
        return Require(FrozenReferenceEntry.Create(input, reference, provenance, Domain, subject, partition, Fingerprint(seed)));
    }

    private static string Fingerprint(string seed) => FrozenReferenceEntry.ComputeNormalizedTextSha256("source:" + seed);
    private static string Hex(char digit) => new(digit, 64);

    private sealed record Scenario(CodeDocumentationCertificationScopeDeclaration Scope,
        CodeDocumentationSavedModelAssessmentModelPassport Candidate, RepositoryOwnedReferenceCorpusOpenResult Opened,
        CodeDocumentationSavedModelAssessmentGenerationSettings Generation, ContinuousLearningTp1DpoConfiguration Evaluator,
        MeasurementNoisePolicyConfig Noise, CodeDocumentationCertificationExposureIndex Index);

    private sealed class Fixture : IDisposable
    {
        private readonly string _scratchBase;
        private readonly string _root;
        private readonly string _repository;

        internal Fixture()
        {
            _scratchBase = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "code-documentation-certification-fixtures"));
            _root = Path.GetFullPath(Path.Combine(_scratchBase, Guid.NewGuid().ToString("N")));
            _repository = Path.Combine(_root, "repo");
            Directory.CreateDirectory(_repository);
            // Synthetic CF storage fixture, not another checkout or production host authority.
            File.WriteAllText(Path.Combine(_repository, "LlmFromScratch.sln"), "synthetic solution sentinel");
            Directory.CreateDirectory(Path.Combine(_repository, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(_repository, "11.DPO"));
        }

        internal Scenario Create(string version = "v1", IEnumerable<FrozenReferenceEntry>? entries = null,
            string name = "synthetic-certification")
        {
            var opened = Open(version, entries ?? [Entry("cert-a", "CLOSED_CERT: café\nline"), Entry("cert-b", "CLOSED_CERT: second input")], name);
            var candidate = new CodeDocumentationSavedModelAssessmentModelPassport(Domain, Pattern, "synthetic-model", "v1", 1,
                "models/synthetic/manifest.json", Hex('a'), "models/synthetic/weights.bin", Hex('b'), 64, 1024);
            var scope = new CodeDocumentationCertificationScopeDeclaration(
                CodeDocumentationCertificationScopeDeclaration.CurrentSchema, Domain, Pattern, Subject,
                "synthetic-certification-scope", "v1", "independent-certification-v1", Cut, "coverage-v1",
                [candidate.ManifestSha256], [new(Owner, "v1")], Complete,
                opened.Corpus.Entries.Select(entry => new CertificationSourceBinding(entry.EntryId, entry.SourceEntityFingerprint, Owner, "v1")));
            var nodes = opened.Corpus.Entries.Select(entry => Node(entry)).Append(
                Node(Entry("teaching", "same-topic separate teaching input"), CertificationExposureRole.AuthoringOrTeaching));
            return new(scope, candidate, opened, new(32, 0f, 0, 1f, 7, 4, 512), Evaluator(), MeasurementNoisePolicyConfig.Canonical, Index(scope, nodes));
        }

        internal RepositoryOwnedReferenceCorpusOpenResult Open(string version, IEnumerable<FrozenReferenceEntry> entries, string name)
        {
            var corpus = Require(FrozenReferenceCorpus.Build(name, version, Cut, entries));
            var address = Require(RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(_repository, Domain, Subject, version));
            var sealedManifest = Require(RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(address, corpus));
            var opened = Require(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(address));
            Assert.Equal(sealedManifest, opened.Manifest);
            Assert.Equal(corpus.CanonicalRepresentation, opened.Corpus.CanonicalRepresentation);
            return opened;
        }

        internal string[] SnapshotFiles() => Directory.GetFiles(_root, "*", SearchOption.AllDirectories)
            .Order(StringComparer.Ordinal).Select(path => Path.GetRelativePath(_root, path) + ":" +
                new FileInfo(path).Length + ":" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))).ToArray();

        public void Dispose()
        {
            var resolved = Path.GetFullPath(_root);
            var prefix = _scratchBase.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!resolved.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
                !Guid.TryParseExact(Path.GetFileName(resolved), "N", out _) || Path.GetDirectoryName(resolved) != _scratchBase)
                throw new InvalidOperationException("Fixture cleanup escaped its owned GUID child.");
            if (Directory.Exists(resolved))
                Directory.Delete(resolved, recursive: true);
        }
    }
}
