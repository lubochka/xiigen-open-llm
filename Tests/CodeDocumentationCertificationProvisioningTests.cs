// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace LlmFromScratch.Tests;

using System.Security.Cryptography;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using ContinuousLearning;
using ModelServer;
using TrainableRouter.Training;

public sealed class CodeDocumentationCertificationProvisioningTests
{
    private const string Domain = "code-documentation";
    private const string Pattern = "builder";
    private const string Subject = "api-reference";
    private const string Owner = "synthetic-owner";
    private const string OwnerVersion = "v1";
    private const string ScopeId = "scope-v1";
    private const string ScopeVersion = "v1";
    private const string ApprovalRef = "approved-synthetic-v1";
    private static readonly DateTimeOffset CoverageCutUtc =
        new(2026, 9, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void LoadProvider_ReadsApprovedSyntheticSnapshotAndBindsActualContext()
    {
        using var fixture = new Fixture();
        var input = fixture.CreatePayload();
        var descriptor = fixture.WriteSnapshot(input.PayloadBytes);
        var loaded = CodeDocumentationCertificationProvisioning.LoadProvider(
            _ => descriptor,
            fixture.RepositoryRoot);

        Assert.True(loaded.IsSuccess, $"{loaded.ErrorCode}: {loaded.ErrorMessage}");
        Assert.NotNull(loaded.Value);

        var provider = loaded.Value!;
        var result = provider.Resolve(
            fixture.RepositoryRoot,
            input.StartRequest,
            input.Parent,
            input.OpenedCorpus,
            input.Profile,
            input.Evaluator);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.NotNull(result.Value);
        Assert.Equal(input.Parent.ManifestSha256, result.Value!.Candidate.ManifestSha256);
        Assert.Equal(input.OpenedCorpus.Manifest.ManifestSha256, result.Value.Corpus.Manifest.ManifestSha256);
        Assert.Equal(CodeDocumentationCertificationScopeDeclaration.CertificationPurpose, result.Value.Declaration.Purpose);
    }

    [Fact]
    public void LoadProvider_PublicCheckoutRootWithProtectedProvisioningLoadsSyntheticSnapshot()
    {
        using var fixture = new Fixture();
        var input = fixture.CreatePayload();
        var descriptor = fixture.WriteSnapshot(input.PayloadBytes);

        Assert.False(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(fixture.RepositoryRoot));
        Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(fixture.ProvisioningRoot));
        Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(fixture.SnapshotPath));
        var loaded = CodeDocumentationCertificationProvisioning.LoadProvider(
            _ => descriptor, fixture.RepositoryRoot);
        Assert.True(loaded.IsSuccess, $"{loaded.ErrorCode}: {loaded.ErrorMessage}");
        Assert.True(loaded.Value!.Resolve(
            fixture.RepositoryRoot, input.StartRequest, input.Parent,
            input.OpenedCorpus, input.Profile, input.Evaluator).IsSuccess);
    }

    [Fact]
    public void LoadProvider_RejectsPublicProvisioningDirectoryUnderPublicCheckoutRoot()
    {
        using var fixture = new Fixture();
        var input = fixture.CreatePayload();
        var descriptor = fixture.WriteSnapshot(input.PayloadBytes);
        fixture.MakeProvisioningPublic();

        Assert.False(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(fixture.ProvisioningRoot));
        var loaded = CodeDocumentationCertificationProvisioning.LoadProvider(
            _ => descriptor, fixture.RepositoryRoot);
        Assert.False(loaded.IsSuccess);
        Assert.Null(loaded.Value);
        Assert.Equal(CodeDocumentationCertificationProvisioning.ErrorPrefix + "CUSTODY_INVALID", loaded.ErrorCode);
    }

    [Fact]
    public void LoadProvider_RejectsDuplicateDescriptorPropertiesBeforeIssuingAuthority()
    {
        using var fixture = new Fixture();
        var input = fixture.CreatePayload();
        var descriptor = fixture.WriteSnapshot(input.PayloadBytes);
        var duplicate = descriptor.Replace(
            "{\"schema\":\"code-documentation-certification-provisioning-descriptor-v1\",",
            "{\"schema\":\"code-documentation-certification-provisioning-descriptor-v1\",\"schema\":\"code-documentation-certification-provisioning-descriptor-v1\",",
            StringComparison.Ordinal);

        var loaded = CodeDocumentationCertificationProvisioning.LoadProvider(
            _ => duplicate,
            fixture.RepositoryRoot);

        Assert.False(loaded.IsSuccess);
        Assert.Null(loaded.Value);
        Assert.Equal(
            CodeDocumentationCertificationProvisioning.ErrorPrefix + "DESCRIPTOR_INVALID",
            loaded.ErrorCode);
    }

    [Fact]
    public void LoadProvider_RejectsOversizedDescriptorBeforeByteArrayEncoding()
    {
        using var fixture = new Fixture();
        var oversizedDescriptor = new string(
            'x',
            CodeDocumentationCertificationProvisioning.MaximumDescriptorBytes + 1);

        var loaded = CodeDocumentationCertificationProvisioning.LoadProvider(
            _ => oversizedDescriptor,
            fixture.RepositoryRoot);

        Assert.False(loaded.IsSuccess);
        Assert.Null(loaded.Value);
        Assert.Equal(
            CodeDocumentationCertificationProvisioning.ErrorPrefix + "DESCRIPTOR_LIMIT_EXCEEDED",
            loaded.ErrorCode);
    }

    [Fact]
    public void LoadProvider_RejectsDuplicateSelectorTuplesBeforeProviderConstruction()
    {
        using var fixture = new Fixture();
        var input = fixture.CreatePayload();
        var descriptor = fixture.WriteSnapshot(input.PayloadBytes);
        var document = JsonNode.Parse(descriptor)!;
        var snapshots = (JsonArray)document["snapshots"]!;
        snapshots.Add(snapshots[0]!.DeepClone());

        var loaded = CodeDocumentationCertificationProvisioning.LoadProvider(
            _ => document.ToJsonString(), fixture.RepositoryRoot);
        Assert.False(loaded.IsSuccess);
        Assert.Null(loaded.Value);
        Assert.Equal(CodeDocumentationCertificationProvisioning.ErrorPrefix + "DESCRIPTOR_INVALID", loaded.ErrorCode);
    }

    [Fact]
    public void LoadProvider_RejectsUnknownPayloadPropertyAndNeverIssuesAuthority()
    {
        using var fixture = new Fixture();
        var input = fixture.CreatePayload(payloadVariant: PayloadVariant.UnknownProperty);
        var descriptor = fixture.WriteSnapshot(input.PayloadBytes);

        var loaded = CodeDocumentationCertificationProvisioning.LoadProvider(
            _ => descriptor,
            fixture.RepositoryRoot);

        Assert.False(loaded.IsSuccess);
        Assert.Null(loaded.Value);
        Assert.Equal(
            CodeDocumentationCertificationProvisioning.ErrorPrefix + "PAYLOAD_INVALID",
            loaded.ErrorCode);
    }

    [Fact]
    public void LoadProvider_RejectsMalformedUtf8SnapshotAsPayloadInvalid()
    {
        using var fixture = new Fixture();
        var descriptor = fixture.WriteSnapshot([0xff, 0xfe, 0x80]);
        var loaded = CodeDocumentationCertificationProvisioning.LoadProvider(
            _ => descriptor, fixture.RepositoryRoot);
        Assert.False(loaded.IsSuccess);
        Assert.Null(loaded.Value);
        Assert.Equal(CodeDocumentationCertificationProvisioning.ErrorPrefix + "PAYLOAD_INVALID", loaded.ErrorCode);
    }

    [Fact]
    public void LoadProvider_RejectsMalformedJsonSnapshotAsPayloadInvalid()
    {
        using var fixture = new Fixture();
        var descriptor = fixture.WriteSnapshot(Encoding.UTF8.GetBytes("{\"schema\":"));
        var loaded = CodeDocumentationCertificationProvisioning.LoadProvider(
            _ => descriptor, fixture.RepositoryRoot);
        Assert.False(loaded.IsSuccess);
        Assert.Null(loaded.Value);
        Assert.Equal(CodeDocumentationCertificationProvisioning.ErrorPrefix + "PAYLOAD_INVALID", loaded.ErrorCode);
    }

    [Fact]
    public void LoadProvider_RejectsAggregateAncestorEdgesBeyondMaximumBeforeAuthority()
    {
        using var fixture = new Fixture();
        var input = fixture.CreatePayload();
        var atLimit = WithAncestorRows(input.PayloadBytes, sourceCount: 10);
        var atLimitDescriptor = fixture.WriteSnapshot(atLimit);
        var loadedAtLimit = CodeDocumentationCertificationProvisioning.LoadProvider(
            _ => atLimitDescriptor, fixture.RepositoryRoot);
        Assert.True(loadedAtLimit.IsSuccess, $"{loadedAtLimit.ErrorCode}: {loadedAtLimit.ErrorMessage}");

        var overLimit = WithAncestorRows(input.PayloadBytes, sourceCount: 11);
        var overLimitDescriptor = fixture.WriteSnapshot(overLimit);
        var loadedOverLimit = CodeDocumentationCertificationProvisioning.LoadProvider(
            _ => overLimitDescriptor, fixture.RepositoryRoot);
        Assert.False(loadedOverLimit.IsSuccess);
        Assert.Null(loadedOverLimit.Value);
        Assert.Equal(CodeDocumentationCertificationProvisioning.ErrorPrefix + "PAYLOAD_INVALID", loadedOverLimit.ErrorCode);
    }

    [Fact]
    public void LoadProvider_RejectsMismatchedPayloadHashAndPathShape()
    {
        using (var hashFixture = new Fixture())
        {
            var input = hashFixture.CreatePayload();
            var descriptor = hashFixture.WriteSnapshot(input.PayloadBytes, overrideHash: Hex('f'));

            var loaded = CodeDocumentationCertificationProvisioning.LoadProvider(
                _ => descriptor,
                hashFixture.RepositoryRoot);

            Assert.False(loaded.IsSuccess);
            Assert.Null(loaded.Value);
            Assert.Equal(
                CodeDocumentationCertificationProvisioning.ErrorPrefix + "PAYLOAD_HASH_INVALID",
                loaded.ErrorCode);
        }

        using (var pathFixture = new Fixture())
        {
            var input = pathFixture.CreatePayload();
            var descriptor = pathFixture.WriteSnapshot(input.PayloadBytes);
            var descriptorDocument = JsonNode.Parse(descriptor)!.AsObject();
            var snapshot = descriptorDocument["snapshots"]!.AsArray()[0]!.AsObject();
            snapshot["snapshotRelativePath"] = "../outside.json";
            File.Delete(pathFixture.SnapshotPath);

            var loaded = CodeDocumentationCertificationProvisioning.LoadProvider(
                _ => descriptorDocument.ToJsonString(),
                pathFixture.RepositoryRoot);

            Assert.False(loaded.IsSuccess);
            Assert.Null(loaded.Value);
            Assert.Equal(
                CodeDocumentationCertificationProvisioning.ErrorPrefix + "DESCRIPTOR_INVALID",
                loaded.ErrorCode);
            Assert.False(File.Exists(pathFixture.SnapshotPath));
        }

        using (var lengthFixture = new Fixture())
        {
            var input = lengthFixture.CreatePayload();
            var descriptor = lengthFixture.WriteSnapshot(input.PayloadBytes);
            var descriptorDocument = JsonNode.Parse(descriptor)!.AsObject();
            var snapshot = descriptorDocument["snapshots"]!.AsArray()[0]!.AsObject();
            snapshot["snapshotByteLength"] = input.PayloadBytes.LongLength + 1;

            var loaded = CodeDocumentationCertificationProvisioning.LoadProvider(
                _ => descriptorDocument.ToJsonString(),
                lengthFixture.RepositoryRoot);

            Assert.False(loaded.IsSuccess);
            Assert.Null(loaded.Value);
            Assert.Equal(
                CodeDocumentationCertificationProvisioning.ErrorPrefix + "PAYLOAD_LENGTH_INVALID",
                loaded.ErrorCode);
        }
    }

    [Fact]
    public void Provider_RejectsIncompleteHistoryAndGraphBeforeCertificationAdmission()
    {
        using var fixture = new Fixture();
        var input = fixture.CreatePayload(PayloadVariant.IncompleteHistory);
        var descriptor = fixture.WriteSnapshot(input.PayloadBytes);
        var loaded = CodeDocumentationCertificationProvisioning.LoadProvider(
            _ => descriptor,
            fixture.RepositoryRoot);
        Assert.True(loaded.IsSuccess, $"{loaded.ErrorCode}: {loaded.ErrorMessage}");

        var result = loaded.Value!.Resolve(
            fixture.RepositoryRoot,
            input.StartRequest,
            input.Parent,
            input.OpenedCorpus,
            input.Profile,
            input.Evaluator);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Contains("PROVENANCE_INCOMPLETE", result.ErrorCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Provider_RejectsInvalidGraphAtAdmissionAfterStructuralReaderValidation()
    {
        using var fixture = new Fixture();
        var input = fixture.CreatePayload(PayloadVariant.InvalidGraph);
        var descriptor = fixture.WriteSnapshot(input.PayloadBytes);

        var loaded = CodeDocumentationCertificationProvisioning.LoadProvider(
            _ => descriptor,
            fixture.RepositoryRoot);

        Assert.True(loaded.IsSuccess, $"{loaded.ErrorCode}: {loaded.ErrorMessage}");
        var resolved = loaded.Value!.Resolve(
            fixture.RepositoryRoot, input.StartRequest, input.Parent,
            input.OpenedCorpus, input.Profile, input.Evaluator);
        Assert.False(resolved.IsSuccess);
        Assert.Null(resolved.Value);
        Assert.Equal(CodeDocumentationCertificationProvisioning.ErrorPrefix + "PROVENANCE_INCOMPLETE", resolved.ErrorCode);
    }

    private static byte[] WithAncestorRows(byte[] payloadBytes, int sourceCount)
    {
        var document = JsonNode.Parse(Encoding.UTF8.GetString(payloadBytes))!;
        var sources = (JsonArray)document["exposureIndex"]!["sources"]!;
        var template = sources[0]!.DeepClone();
        sources.Clear();
        var ancestors = new JsonArray();
        for (var index = 0; index < 10_000; index++)
            ancestors.Add("sha256:" + index.ToString("x64"));
        for (var index = 0; index < sourceCount; index++)
        {
            var source = template.DeepClone();
            source["sourceEntityFingerprint"] = "sha256:" + (index + 10_000).ToString("x64");
            source["ancestors"] = ancestors.DeepClone();
            sources.Add(source);
        }
        return Encoding.UTF8.GetBytes(document.ToJsonString());
    }

    private enum PayloadVariant
    {
        Valid,
        UnknownProperty,
        IncompleteHistory,
        InvalidGraph,
    }

    private sealed class Fixture : IDisposable
    {
        private readonly string _root;
        private readonly string _repository;
        private bool _repositoryIsPublic;

        internal Fixture()
        {
            _root = Path.Combine(Path.GetTempPath(), "c05-provisioning-" + Guid.NewGuid().ToString("N"));
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(_root);
            _repository = Path.Combine(_root, "repo");
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(_repository);
            File.WriteAllText(Path.Combine(_repository, "LlmFromScratch.sln"), "synthetic solution sentinel");
            Directory.CreateDirectory(Path.Combine(_repository, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(_repository, "11.DPO"));
        }

        internal string RepositoryRoot => _repository;
        internal string ProvisioningRoot => Path.Combine(
            _repository, "training-artifacts", "assessments", Domain, Pattern, "provisioning");
        internal string SnapshotPath { get; private set; } = string.Empty;

        internal string WriteSnapshot(byte[] payloadBytes, string? overrideHash = null)
        {
            if (!_repositoryIsPublic)
            {
                MakeDirectoryPublic(_repository);
                _repositoryIsPublic = true;
            }
            var snapshotHash = overrideHash ?? Convert.ToHexString(SHA256.HashData(payloadBytes)).ToLowerInvariant();
            var relative = $"training-artifacts/assessments/{Domain}/{Pattern}/provisioning/{snapshotHash}.json";
            var path = Path.Combine(_repository, relative.Replace('/', Path.DirectorySeparatorChar));
            CreateCustodyDirectory(Path.GetDirectoryName(path)!);
            using (var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
                       path,
                       FileMode.CreateNew,
                       FileAccess.ReadWrite,
                       FileShare.None,
                       FileOptions.WriteThrough,
                       bufferSize: 4096))
            {
                stream.Write(payloadBytes);
                stream.Flush(flushToDisk: true);
            }
            SnapshotPath = path;

            var descriptor = new
            {
                schema = CodeDocumentationCertificationProvisioning.DescriptorSchema,
                snapshots = new[]
                {
                    new
                    {
                        domainSlug = Domain,
                        designPatternSlug = Pattern,
                        subjectSlug = Subject,
                        scopeId = ScopeId,
                        scopeVersion = ScopeVersion,
                        ownerSnapshotId = Owner,
                        ownerSnapshotVersion = OwnerVersion,
                        approvalRef = ApprovalRef,
                        snapshotRelativePath = relative,
                        snapshotSha256 = snapshotHash,
                        snapshotByteLength = payloadBytes.LongLength,
                    }
                }
            };
            return JsonSerializer.Serialize(descriptor);
        }

        internal void MakeProvisioningPublic() => MakeDirectoryPublic(ProvisioningRoot);

        internal PayloadInput CreatePayload(PayloadVariant payloadVariant = PayloadVariant.Valid)
        {
            var opened = CreateCorpus();
            var parent = CreateParent();
            var generation = new CodeDocumentationSavedModelAssessmentGenerationSettings(
                32,
                0f,
                0,
                1f,
                7,
                4,
                512);
            var evaluator = Require(ContinuousLearningTp1DpoConfiguration.Create(
                "qwen-assessment-test:9b",
                Hex('c'),
                Hex('d'),
                100,
                "sha256:" + Hex('e'),
                "sha256:" + Hex('f'),
                Enumerable.Range(1, 32).Select(static value => (byte)value).ToArray()));
            var profile = new CodeDocumentationCertificationProfile(
                Domain,
                Pattern,
                Subject,
                "v1",
                opened.Manifest.ManifestSha256,
                ScopeId,
                ScopeVersion,
                "coverage-v1",
                CoverageCutUtc,
                Owner,
                OwnerVersion,
                generation);
            var startRequest = new CodeDocumentationRepositoryOwnedModelCheckpointStartRequest(
                _repository,
                Domain,
                Pattern,
                parent.ModelId,
                parent.Version,
                null!,
                parent.RuntimeCompatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied,
                MaximumCheckpointBytes: 1024);

            var history = new
            {
                training = payloadVariant == PayloadVariant.IncompleteHistory ? "unknown" : "complete",
                diagnostic = "complete",
                teaching = "complete",
            };
            var entries = opened.Corpus.Entries;
            var bindings = entries.Select(entry => new
            {
                entryId = entry.EntryId,
                sourceEntityFingerprint = entry.SourceEntityFingerprint,
                ownerSnapshotId = Owner,
                ownerSnapshotVersion = OwnerVersion,
            }).ToArray();
            var sources = entries.Select(entry => new
            {
                sourceEntityFingerprint = entry.SourceEntityFingerprint,
                domain = Domain,
                subject = Subject,
                ownerSnapshotId = Owner,
                ownerSnapshotVersion = OwnerVersion,
                sourceInput = entry.OwnerInput,
                originEvidenceRef = FrozenReferenceEntry.ComputeNormalizedTextSha256("synthetic-origin"),
                roles = new[] { "reference-acquisition-only" },
                lineageKind = payloadVariant == PayloadVariant.InvalidGraph ? "derived" : "known-root",
                ancestors = Array.Empty<string>(),
            }).ToArray();
            var payloadObject = new
            {
                schema = CodeDocumentationCertificationProvisioning.SnapshotSchema,
                approvalRef = ApprovalRef,
                declaration = new
                {
                    domain = Domain,
                    designPattern = Pattern,
                    subject = Subject,
                    scopeId = ScopeId,
                    scopeVersion = ScopeVersion,
                    purpose = CodeDocumentationCertificationScopeDeclaration.CertificationPurpose,
                    coverageCutUtc = CoverageCutUtc.UtcDateTime.ToString("O"),
                    coverageRevision = "coverage-v1",
                    checkpointLineageManifestIdentities = new[] { parent.ManifestSha256 },
                    ownerSnapshots = new[] { new { id = Owner, version = OwnerVersion } },
                    history,
                    certificationSourceBindings = bindings,
                },
                exposureIndex = new
                {
                    version = ScopeVersion,
                    coveredCheckpointManifestSha256 = parent.ManifestSha256,
                    history,
                    sources,
                },
            };
            var payloadJson = JsonSerializer.Serialize(payloadObject);
            if (payloadVariant == PayloadVariant.UnknownProperty)
                payloadJson = payloadJson.Insert(payloadJson.Length - 1, ",\"unexpected\":true");

            return new(
                Encoding.UTF8.GetBytes(payloadJson),
                opened,
                parent,
                profile,
                evaluator,
                startRequest);
        }

        private RepositoryOwnedReferenceCorpusOpenResult CreateCorpus()
        {
            var entries = new[]
            {
                Entry("cert-a", "CLOSED_CERT: first synthetic input"),
                Entry("cert-b", "CLOSED_CERT: second synthetic input"),
            };
            var corpus = Require(FrozenReferenceCorpus.Build(
                "synthetic-certification",
                "v1",
                CoverageCutUtc,
                entries));
            var address = Require(RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(
                _repository,
                Domain,
                Subject,
                "v1"));
            Require(RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(address, corpus));
            return Require(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(address));
        }

        private static RepositoryOwnedModelCheckpointManifest CreateParent()
            => new(
                "repository-owned-model-checkpoint-v1",
                "repository-owned-model-checkpoint",
                Domain,
                Pattern,
                "synthetic-model",
                "v1",
                1,
                "training-artifacts/models/code-documentation/v1/step-1",
                "training-artifacts/models/code-documentation/v1/step-1/weights.bin",
                "training-artifacts/models/code-documentation/v1/step-1/manifest.json",
                Hex('b'),
                64,
                null,
                "synthetic-test-fixture",
                new RepositoryOwnedDpoDatasetManifestIdentity(
                    "training-data/dpo/code-documentation/builder/v1/manifest.json",
                    Hex('c')),
                new RepositoryOwnedModelRuntimeCompatibility(
                    Hex('1'), Hex('2'), Hex('3'), Hex('4'), Hex('5'), Hex('6')),
                "2026-09-15T12:00:00.0000000Z",
                new[] { "sha256:" + Hex('7') },
                Hex('a'));

        private static FrozenReferenceEntry Entry(string seed, string input)
        {
            var reference = "CLOSED_REFERENCE: synthetic answer";
            var provenance = Require(PaidReferenceProvenance.Create(
                "synthetic-provider",
                "synthetic-model",
                "v1",
                "request-" + seed,
                CoverageCutUtc.AddHours(-1),
                FrozenReferenceEntry.ComputeNormalizedTextSha256(reference)));
            return Require(FrozenReferenceEntry.Create(
                input,
                reference,
                provenance,
                Domain,
                Subject,
                FrozenReferencePartition.HeldOut,
                FrozenReferenceEntry.ComputeNormalizedTextSha256("source:" + seed)));
        }

        private void CreateCustodyDirectory(string path)
        {
            var relative = Path.GetRelativePath(_repository, path);
            var current = _repository;
            if (relative == ".")
                return;
            foreach (var part in relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            {
                current = Path.Combine(current, part);
                if (!Directory.Exists(current))
                {
                    if (string.Equals(current, ProvisioningRoot, StringComparison.OrdinalIgnoreCase))
                        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(current);
                    else
                        Directory.CreateDirectory(current);
                }
            }
        }

        private static void MakeDirectoryPublic(string path)
        {
            if (OperatingSystem.IsWindows())
            {
                var directory = new DirectoryInfo(path);
                var security = directory.GetAccessControl();
                security.AddAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),
                    FileSystemRights.Modify,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow));
                directory.SetAccessControl(security);
            }
            else if (OperatingSystem.IsLinux())
            {
                File.SetUnixFileMode(path,
                    UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                    UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                    UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
            }
        }

        public void Dispose()
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }
    }

    private sealed record PayloadInput(
        byte[] PayloadBytes,
        RepositoryOwnedReferenceCorpusOpenResult OpenedCorpus,
        RepositoryOwnedModelCheckpointManifest Parent,
        CodeDocumentationCertificationProfile Profile,
        ContinuousLearningTp1DpoConfiguration Evaluator,
        CodeDocumentationRepositoryOwnedModelCheckpointStartRequest StartRequest);

    private static string Hex(char value) => new(value, 64);

    private static T Require<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.NotNull(result.Value);
        return result.Value!;
    }
}
