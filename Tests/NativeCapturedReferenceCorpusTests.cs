// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ContinuousLearning;
using Xunit.Abstractions;

public sealed class NativeCapturedReferenceCorpusTests(ITestOutputHelper output)
{
    [Fact]
    public void NativeCapture_PrivateOwnerSealAndReopenPreserveRawAssessmentInputsAndRejectOriginTampering()
    {
        using var fixture = new Fixture();
        string? unrelatedLinuxFile = null;
        var unrelatedLinuxMode = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.GroupRead;
        if (OperatingSystem.IsLinux()) // This branch is source-only until this same Fact is actually run on Linux.
        {
            var unrelated = Path.Combine(fixture.Repository, "training-data", "dpo", "unrelated-native-probe");
            Directory.CreateDirectory(unrelated);
            unrelatedLinuxFile = Path.Combine(unrelated, "unrelated.txt"); File.WriteAllText(unrelatedLinuxFile, "synthetic unrelated artifact");
            File.SetUnixFileMode(unrelatedLinuxFile, unrelatedLinuxMode); // Deliberately broad fixture mode must remain unchanged by native operations.
        }
        var time = new DateTimeOffset(2026, 10, 15, 1, 0, 0, TimeSpan.Zero);
        var capture = new NativeReferenceCaptureMetadata("codex-cli", "0.154.0", "exec-json-last-message-files", "gpt-6-astra", "ultra",
            null, null, null, null, "synthetic-native-thread", Ref("request"), Ref("response"), Ref("stdout"), Ref("stderr"), Ref("capture"), time, time.AddMinutes(1), 42, 23, 7);
        var rows = Enumerable.Range(0, 12).Select(i => new
        {
            caseId = "synthetic-case-" + i.ToString(CultureInfo.InvariantCulture),
            ownerInput = $"Synthetic source {i}: e\u0301\r\nreturn null;\r\n",
            referenceAnswer = $"Synthetic closed reference {i}: e\u0301\r\nnull\r\n",
            partition = "held-out", sourceEntityFingerprint = Ref("synthetic-family-" + i),
        }).Select(r => new { r.caseId, r.ownerInput, r.referenceAnswer, r.partition, r.sourceEntityFingerprint,
            ownerInputRawUtf8Sha256 = Ref(r.ownerInput), referenceAnswerRawUtf8Sha256 = Ref(r.referenceAnswer) }).ToArray();
        var captureJson = new
        {
            runtimeId = capture.RuntimeId, runtimeVersion = capture.RuntimeVersion, captureBoundary = capture.CaptureBoundary,
            requestedModelAlias = capture.RequestedModelAlias, requestedReasoningEffort = capture.RequestedReasoningEffort,
            observedProviderId = capture.ObservedProviderId, observedModelId = capture.ObservedModelId,
            observedModelVersion = capture.ObservedModelVersion, observedServerRequestId = capture.ObservedServerRequestId, threadId = capture.ThreadId,
            requestRawUtf8Sha256 = capture.RequestRawUtf8Sha256, responseRawUtf8Sha256 = capture.ResponseRawUtf8Sha256,
            stdoutSha256 = capture.StdoutSha256, stderrSha256 = capture.StderrSha256, captureRecordSha256 = capture.CaptureRecordSha256,
            startedAtUtc = time.ToString("O", CultureInfo.InvariantCulture), capturedAtUtc = capture.CapturedAtUtc.ToString("O", CultureInfo.InvariantCulture),
            inputTokens = capture.InputTokens, outputTokens = capture.OutputTokens, reasoningTokens = capture.ReasoningTokens,
        };
        var intakeBytes = JsonSerializer.SerializeToUtf8Bytes(new { schema = RepositoryOwnedNativeReferenceCorpusIntake.Schema,
            corpusName = "synthetic-native-sealed", corpusVersion = "native-test-v1", frozenAtUtc = time.AddMinutes(2).ToString("O", CultureInfo.InvariantCulture),
            subject = "synthetic-native", capture = captureJson, entries = rows });
        var intakePath = Path.Combine(fixture.Private, "intake.json");
        using (var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(intakePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, FileOptions.WriteThrough))
        { stream.Write(intakeBytes); stream.Flush(true); }
        Assert.False(RepositoryOwnedNativeReferenceCorpusIntake.SealFromPrivateFile(fixture.Repository, intakePath, new string('0', 64)).IsSuccess);
        var sealedResult = Require(RepositoryOwnedNativeReferenceCorpusIntake.SealFromPrivateFile(fixture.Repository, intakePath, Hex(intakeBytes)));
        var address = Require(RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(fixture.Repository, "csharp-code-documentation", "synthetic-native", "native-test-v1"));
        var opened = Require(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(address));
        Assert.Equal(sealedResult.Opened.Manifest, opened.Manifest);
        var seal = opened.Manifest;
        var requiredNative = Require(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedNativeCapture(address, seal.CorpusSha256, seal.PayloadSha256, seal.ManifestSha256));
        Assert.Equal(seal, requiredNative.Manifest); Assert.Equal(opened.Corpus.CanonicalRepresentation, requiredNative.Corpus.CanonicalRepresentation);
        Assert.False(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedNativeCapture(address, "sha256:" + new string('0', 64), seal.PayloadSha256, seal.ManifestSha256).IsSuccess);
        Assert.False(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedNativeCapture(address, seal.CorpusSha256, new string('0', 64), seal.ManifestSha256).IsSuccess);
        Assert.False(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedNativeCapture(address, seal.CorpusSha256, seal.PayloadSha256, new string('0', 64)).IsSuccess);
        Assert.Equal(RepositoryOwnedReferenceCorpusManifestSealer.NativeCaptureSchema, opened.Manifest.Schema);
        Assert.Equal(sealedResult.Opened.Corpus.CanonicalRepresentation, opened.Corpus.CanonicalRepresentation);
        Assert.Equal(12, opened.Corpus.Count);
        Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(address.CorpusRoot));
        Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(address.ManifestPath));
        Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(Path.Combine(address.CorpusRoot, "corpus.json")));
        foreach (var binding in sealedResult.CaseBindings)
        {
            var expected = rows.Single(r => r.caseId == binding.SourceCaseId);
            var actual = opened.Corpus.FindEntry(binding.EntryId)!;
            Assert.NotNull(actual); Assert.Null(actual.PaidReferenceProvenance);
            Assert.Equal(Encoding.UTF8.GetBytes(expected.ownerInput), Encoding.UTF8.GetBytes(actual.OwnerInput));
            Assert.Equal(Encoding.UTF8.GetBytes(expected.referenceAnswer), Encoding.UTF8.GetBytes(actual.PaidModelReferenceAnswer));
            var p = actual.NativeCaptureProvenance!;
            Assert.Equal(capture, p.Capture); Assert.Null(p.Capture.ObservedModelId); Assert.Null(p.Capture.ObservedModelVersion);
            Assert.Null(p.Capture.ObservedProviderId); Assert.Null(p.Capture.ObservedServerRequestId);
            Assert.Equal("gpt-6-astra", p.Capture.RequestedModelAlias); Assert.Equal("0.154.0", p.Capture.RuntimeVersion);
            Assert.True(p.ClosedReference && p.ReferenceOutputNeverTrainable && p.ReferenceOutputNeverDpoPairSide);
            Assert.Equal(expected.referenceAnswerRawUtf8Sha256, p.ReferenceAnswerRawUtf8Sha256);
            var prompt = CodeDocumentationSavedModelAssessmentService.BuildCheckerPrompt(actual, "synthetic candidate");
            using var json = JsonDocument.Parse(prompt[prompt.IndexOf('{')..]);
            Assert.Equal(expected.ownerInput, json.RootElement.GetProperty("ownerTask").GetString());
            Assert.Equal(expected.referenceAnswer, json.RootElement.GetProperty("closedReferenceAnswer").GetString());
        }

        var settings = new CodeDocumentationSavedModelAssessmentGenerationSettings(16, 0, 0, 1, 7, 12, 4096);
        var config = Require(ContinuousLearningTp1DpoConfiguration.Create("qwen3.5:9b", new string('a', 64), new string('b', 64), 120,
            Ref("synthetic-retention"), Ref("synthetic-output"), new byte[32])); // Synthetic configuration creates no client, grant or model invocation.
        var plan = Require(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.PlanQualityGate(opened.Corpus, settings, config, MeasurementNoisePolicyConfig.Canonical));
        Assert.Equal(5, plan.BaselinePlan.RepeatCount); Assert.Equal(12, plan.BaselinePlan.HeldOutCaseCount);
        Assert.Equal(60, plan.BaselinePlan.ExpectedEvaluationCallCount); Assert.Equal(120, plan.ExpectedTotalEvaluationCallCount);
        var descriptor = new CodeDocumentationOneLessonCorpusV2("synthetic-native", "ClosedEvaluation", opened.Manifest.Domain, opened.Manifest.Subject,
            opened.Manifest.CorpusVersion, Hex(File.ReadAllBytes(address.ManifestPath)), opened.Corpus.CorpusSha256[7..],
            new FileInfo(address.ManifestPath).Length, opened.Manifest.PayloadSha256, opened.Manifest.PayloadByteLength);
        var selection = Require(CodeDocumentationOneLessonCorpusSelection.Create(descriptor, opened.Corpus.EntryNames));
        var fresh = Require(selection.OpenFresh(fixture.Repository, settings)); // Actual closed-evaluation owner reopens the native corpus and all selected cases.
        Assert.Equal(opened.Corpus.CorpusSha256, fresh.Corpus.CorpusSha256); Assert.Equal(12, fresh.Selection.CaseIds.Count);
        if (OperatingSystem.IsLinux()) // Native seal, generic reopen and actual corpus selection must not repair unrelated artifact modes.
        {
            Assert.Equal(unrelatedLinuxMode, File.GetUnixFileMode(unrelatedLinuxFile!));
            foreach (var file in new[] { address.ManifestPath, Path.Combine(address.CorpusRoot, "corpus.json") })
            {
                var safe = File.GetUnixFileMode(file); var unsafeMode = safe | UnixFileMode.GroupRead;
                try
                {
                    File.SetUnixFileMode(file, unsafeMode); // Change only this synthetic native file to test refusal without mode repair.
                    Assert.False(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(address).IsSuccess);
                    Assert.False(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedNativeCapture(address, seal.CorpusSha256, seal.PayloadSha256, seal.ManifestSha256).IsSuccess);
                    Assert.Equal(unsafeMode, File.GetUnixFileMode(file));
                    Assert.False(RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(address, opened.Corpus).IsSuccess);
                    Assert.Equal(unsafeMode, File.GetUnixFileMode(file));
                    Assert.Equal(unrelatedLinuxMode, File.GetUnixFileMode(unrelatedLinuxFile!));
                }
                finally { File.SetUnixFileMode(file, safe); } // Restore only the owned synthetic fixture for subsequent assertions.
            }
            var safeRoot = File.GetUnixFileMode(address.CorpusRoot); var unsafeRoot = safeRoot | UnixFileMode.GroupRead;
            try
            {
                File.SetUnixFileMode(address.CorpusRoot, unsafeRoot);
                Assert.False(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(address).IsSuccess);
                Assert.False(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedNativeCapture(address, seal.CorpusSha256, seal.PayloadSha256, seal.ManifestSha256).IsSuccess);
                Assert.False(RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(address, opened.Corpus).IsSuccess);
                Assert.Equal(unsafeRoot, File.GetUnixFileMode(address.CorpusRoot));
                Assert.Equal(unrelatedLinuxMode, File.GetUnixFileMode(unrelatedLinuxFile!));
            }
            finally { File.SetUnixFileMode(address.CorpusRoot, safeRoot); }
            Assert.True(selection.OpenFresh(fixture.Repository, settings).IsSuccess);
        }

        var first = opened.Corpus.Entries[0];
        var known = Require(PaidReferenceProvenance.CreateRawUtf8("synthetic-provider", "synthetic-model", "v1", "synthetic-request", time, first.PaidModelReferenceAnswer));
        var knownEntry = Require(FrozenReferenceEntry.CreateRawCodeDocumentation(first.OwnerInput, first.PaidModelReferenceAnswer, known,
            first.SubjectId, FrozenReferencePartition.HeldOut, Ref("another-synthetic-family")));
        Assert.Equal("FROZEN_REFERENCE_CORPUS_ORIGIN_MISMATCH", FrozenReferenceCorpus.Build("mixed-origin", "v1", time, [first, knownEntry]).ErrorCode);
        Assert.False(PaidReferenceProvenance.CreateRawUtf8("synthetic-provider", "synthetic-model", null!, "request", time, first.PaidModelReferenceAnswer).IsSuccess);
        Assert.False(FrozenReferenceEntry.CreateNativeCapturedCodeDocumentation(first.OwnerInput, first.PaidModelReferenceAnswer + "changed",
            first.NativeCaptureProvenance!, first.SubjectId, first.Partition, first.SourceEntityFingerprint).IsSuccess);

        var payloadPath = Path.Combine(address.CorpusRoot, "corpus.json");
        var payloadBytes = File.ReadAllBytes(payloadPath); var manifestBytes = File.ReadAllBytes(address.ManifestPath);
        using (var legacyFixture = new Fixture())
        {
            var legacyCorpus = Require(FrozenReferenceCorpus.Build("synthetic-known", seal.CorpusVersion, time.AddMinutes(2), [knownEntry]));
            var legacyAddress = Require(RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(legacyFixture.Repository, seal.Domain, seal.Subject, seal.CorpusVersion));
            var legacySeal = Require(RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(legacyAddress, legacyCorpus));
            Assert.Equal(legacySeal, Require(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(legacyAddress)).Manifest); // Valid generic legacy readback still works.
            Assert.False(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedNativeCapture(legacyAddress, legacySeal.CorpusSha256, legacySeal.PayloadSha256, legacySeal.ManifestSha256).IsSuccess); // Even correct legacy pins cannot enter native-required readback.
            try
            {
                File.WriteAllBytes(payloadPath, File.ReadAllBytes(Path.Combine(legacyAddress.CorpusRoot, "corpus.json")));
                File.WriteAllBytes(address.ManifestPath, File.ReadAllBytes(legacyAddress.ManifestPath)); // Actual valid legacy seal replaces the native address; no race or fabricated header is needed.
                Assert.False(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedNativeCapture(address, seal.CorpusSha256, seal.PayloadSha256, seal.ManifestSha256).IsSuccess);
                Assert.False(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedNativeCapture(address, legacySeal.CorpusSha256, legacySeal.PayloadSha256, legacySeal.ManifestSha256).IsSuccess);
                if (OperatingSystem.IsLinux()) Assert.Equal(unrelatedLinuxMode, File.GetUnixFileMode(unrelatedLinuxFile!)); // Legacy-origin refusal must precede any global mode repair.
            }
            finally { File.WriteAllBytes(payloadPath, payloadBytes); File.WriteAllBytes(address.ManifestPath, manifestBytes); }
        }
        Assert.True(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedNativeCapture(address, seal.CorpusSha256, seal.PayloadSha256, seal.ManifestSha256).IsSuccess);
        foreach (var name in RepositoryOwnedReferenceCorpusManifestSealer.CaptureFieldNames)
        {
            var payload = JsonNode.Parse(payloadBytes)!.AsObject();
            var metadata = payload["entries"]![0]!["nativeCaptureProvenance"]!["capture"]!.AsObject();
            metadata[name] = name switch
            {
                "inputTokens" or "outputTokens" or "reasoningTokens" => JsonValue.Create(99L),
                "startedAtUtc" => JsonValue.Create(time.AddSeconds(1).ToString("O", CultureInfo.InvariantCulture)),
                "capturedAtUtc" => JsonValue.Create(time.AddMinutes(1).AddSeconds(1).ToString("O", CultureInfo.InvariantCulture)),
                var n when n.EndsWith("Sha256", StringComparison.Ordinal) => JsonValue.Create(Ref("tampered")),
                _ => JsonValue.Create("synthetic-changed-identity"),
            };
            ResealOuter(address.ManifestPath, payloadPath, payload, manifestBytes); // Refresh outer payload/self hashes so the actual inner canonical metadata check must refuse.
            Assert.False(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(address).IsSuccess, name);
        }
        var mixed = JsonNode.Parse(payloadBytes)!.AsObject();
        mixed["entries"]![0]!["paidReferenceProvenance"] = new JsonObject();
        ResealOuter(address.ManifestPath, payloadPath, mixed, manifestBytes);
        Assert.False(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(address).IsSuccess);
        File.WriteAllBytes(payloadPath, payloadBytes); File.WriteAllBytes(address.ManifestPath, manifestBytes);
        Assert.True(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(address).IsSuccess);

        // Reuse the existing independently pinned v1/raw-v2 owner Fact directly within this one bounded new check.
        new RepositoryOwnedReferenceCorpusManifestTests(output).Sealer_WritesPayloadAndManifestThenFreshOpenReconstructsEquivalentCorpus();
        output.WriteLine(JsonSerializer.Serialize(new { fixture = "SYNTHETIC_ONLY", opened.Corpus.CorpusSha256, opened.Manifest.PayloadSha256,
            opened.Manifest.ManifestSha256, entries = 12, captureMetadataTamperFields = RepositoryOwnedReferenceCorpusManifestSealer.CaptureFieldNames.Length,
            actualPlanBaselineCalls = 60, actualPlanTotalCalls = 120, linuxModeAssertions = OperatingSystem.IsLinux() ? "EXECUTED" : "NOT_RUN_WINDOWS_BRANCH",
            closedScientificCorpusExecuted = false, modelCalls = 0, teacherCalls = 0, optimizerUpdates = 0 }));
    }

    private static void ResealOuter(string manifestPath, string payloadPath, JsonObject payload, byte[] originalManifest)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload); File.WriteAllBytes(payloadPath, bytes);
        var m = JsonNode.Parse(originalManifest)!.AsObject(); m["payloadSha256"] = Hex(bytes); m["payloadByteLength"] = bytes.LongLength; m["manifestSha256"] = "";
        m["manifestSha256"] = Hex(JsonSerializer.SerializeToUtf8Bytes(m)); File.WriteAllBytes(manifestPath, JsonSerializer.SerializeToUtf8Bytes(m));
    }
    private static string Ref(string value) => "sha256:" + Hex(new UTF8Encoding(false, true).GetBytes(value));
    private static string Hex(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static T Require<T>(AgentInfrastructure.OperationResult<T> value) { Assert.True(value.IsSuccess, value.ErrorCode + ": " + value.ErrorMessage); return Assert.IsAssignableFrom<T>(value.Value); }
    private sealed class Fixture : IDisposable
    {
        internal string Repository { get; } = Path.Combine(Path.GetTempPath(), "rp01-native-reference-synthetic-" + Guid.NewGuid().ToString("N"));
        internal string Private => Path.Combine(Repository, "private-intake");
        internal Fixture()
        {
            Directory.CreateDirectory(Repository); File.WriteAllText(Path.Combine(Repository, "LlmFromScratch.sln"), "synthetic sentinel");
            Directory.CreateDirectory(Path.Combine(Repository, "14.ContinuousLearning")); Directory.CreateDirectory(Path.Combine(Repository, "11.DPO"));
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(Private);
        }
        public void Dispose() { if (Directory.Exists(Repository)) Directory.Delete(Repository, true); } // Delete only this uniquely created synthetic fixture root.
    }
}
