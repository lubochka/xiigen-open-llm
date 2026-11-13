// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using LoRA;
using Microsoft.Win32.SafeHandles;
using OrchestratorModel.Logical;
using TensorCore.Layers;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;
using Domain = SemanticCompiler29.Domain;

public sealed class TeachingRegistryPersistenceTests
{
    [Theory]
    [InlineData("schema")]
    [InlineData("tokenizer-hash")]
    [InlineData("policy-hash")]
    [InlineData("runtime-hash")]
    [InlineData("replay-attestation")]
    [InlineData("executable-attestation")]
    [InlineData("unit-registration-order")]
    [InlineData("flow-registration-order")]
    [InlineData("knn-examples")]
    [InlineData("knn-corpus-hash")]
    [InlineData("schema-v1")]
    public void ExecutableRestore_StrippedCurrentManifestFailsClosed(
        string strippedField)
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var baseline = BuildValidTrustStateSnapshot(artifacts);
        var manifest = Assert.Single(baseline.GeneratedCapabilities);
        var stripped = strippedField switch
        {
            "schema" => manifest with { SchemaVersion = null },
            "tokenizer-hash" => manifest with { TokenizerSha256 = null },
            "policy-hash" => manifest with { PolicyCheckpointSha256 = null },
            "runtime-hash" => manifest with
            {
                RuntimeProvenanceSha256 = null
            },
            "replay-attestation" => manifest with
            {
                ReplayAttestation = null
            },
            "executable-attestation" => manifest with
            {
                ExecutableManifestAttestation = null
            },
            "unit-registration-order" => manifest with
            {
                UnitRegistrationOrder = null
            },
            "flow-registration-order" => manifest with
            {
                FlowRegistrationOrder = null
            },
            "knn-examples" => manifest with { KnnExamples = null },
            "knn-corpus-hash" => manifest with
            {
                KnnCorpusSha256 = null
            },
            "schema-v1" => manifest with { SchemaVersion = 1 },
            _ => throw new InvalidOperationException()
        };
        var snapshot = baseline with
        {
            GeneratedCapabilities = [stripped]
        };
        var units = new InMemoryLogicalUnitRegistry();
        var flows = new InMemoryLogicalFlowRegistry();

        var result = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            units,
            flows);

        Assert.False(result.IsSuccess);
        var expectedErrorCode = strippedField switch
        {
            "unit-registration-order" => "GENERATED_CAPABILITY_RESTORE_INVALID",
            "flow-registration-order" =>
                "GENERATED_CAPABILITY_FLOW_MANIFEST_NOT_FOUND",
            "knn-examples" or "knn-corpus-hash" =>
                "GENERATED_CAPABILITY_MANIFEST_INVALID",
            _ => TeachingRegistryPersistence
                .GeneratedCapabilityExecutableManifestInvalidErrorCode
        };
        Assert.Equal(
            expectedErrorCode,
            result.ErrorCode);
        Assert.Empty(units.ListDescriptors());
        Assert.Empty(flows.ListDescriptors());
    }

    [Fact]
    public void KnnCorpusProjector_DuplicateExampleIdsFailClosed()
    {
        var projection = GeneratedCapabilityKnnCorpusProjector.Project(
            [
                new GeneratedCapabilityKnnExample(
                    "duplicate-example",
                    "first training row",
                    "FIRST",
                    ["IMPLEMENT"],
                    ["software-development"]),
                new GeneratedCapabilityKnnExample(
                    "duplicate-example",
                    "second training row",
                    "SECOND",
                    ["VERIFY"],
                    ["software-development"])
            ]);

        Assert.False(projection.IsSuccess);
        Assert.Equal(
            GeneratedCapabilityKnnCorpusProjector.InvalidErrorCode,
            projection.ErrorCode);
    }

    [Fact]
    public void ExecutableV2_EmptyPredicateIdsCannotBeProjectedOrRestored()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var baseline = BuildValidTrustStateSnapshot(artifacts);
        var manifest = Assert.Single(baseline.GeneratedCapabilities) with
        {
            PredicateIds = []
        };

        var projection = GeneratedCapabilityExecutableManifestProjector
            .ProjectSha256(
                manifest,
                manifest.TokenizerSha256,
                manifest.PolicyCheckpointSha256);
        Assert.False(projection.IsSuccess);

        var snapshot = baseline with
        {
            Flows = [BuildFlowEntry(manifest.FlowId, manifest, 3)],
            GeneratedCapabilities = [manifest]
        };
        var units = new InMemoryLogicalUnitRegistry();
        var flows = new InMemoryLogicalFlowRegistry();

        var restore = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            units,
            flows);

        Assert.False(restore.IsSuccess);
        Assert.Equal(
            TeachingRegistryPersistence
                .GeneratedCapabilityExecutableManifestInvalidErrorCode,
            restore.ErrorCode);
        Assert.Empty(units.ListDescriptors());
        Assert.Empty(flows.ListDescriptors());
    }

    [Fact]
    public void ExecutableRestore_FlowDomainTagsRequireExactOrdinalSequence()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var baseline = BuildValidTrustStateSnapshot(artifacts);
        var manifest = WithExecutableManifestAttestation(
            WithReplayAttestation(
                Assert.Single(baseline.GeneratedCapabilities) with
                {
                    DomainTags = ["software-development", "integration"]
                }));
        var nonExactBindings = new[]
        {
            new[] { "integration", "software-development" },
            new[] { " software-development", "integration" },
            new[] { "software-development", "integration", "integration" }
        };

        foreach (var domainTags in nonExactBindings)
        {
            var flowEntry = BuildFlowEntry(manifest.FlowId, manifest, 3) with
            {
                DomainTags = domainTags
            };
            var snapshot = baseline with
            {
                Flows = [flowEntry],
                GeneratedCapabilities = [manifest]
            };
            var units = new InMemoryLogicalUnitRegistry();
            var flows = new InMemoryLogicalFlowRegistry();

            var restore = LoadExecutableIntoRegistries(
                snapshot,
                BuildTopologyWithSubPhase41(),
                units,
                flows);

            Assert.False(restore.IsSuccess);
            Assert.Empty(units.ListDescriptors());
            Assert.Empty(flows.ListDescriptors());
        }
    }

    [Fact]
    public void ExecutableRestore_FactoryUsesManifestOwnedSignedDomainTags()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var snapshot = BuildValidTrustStateSnapshot(artifacts);
        var manifest = Assert.Single(snapshot.GeneratedCapabilities);
        var units = new InMemoryLogicalUnitRegistry();
        var flows = new InMemoryLogicalFlowRegistry();
        var restore = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            units,
            flows);
        Assert.True(restore.IsSuccess, restore.ErrorMessage);

        snapshot.Flows[0].DomainTags[0] = "tampered-after-restore";
        Assert.Equal(
            manifest.DomainTags,
            Assert.Single(flows.ListDescriptors()).DomainTags);

        var binding = flows.ResolveBindingBySubPhase(
            manifest.SubPhaseId,
            new LogicalFlowFactoryRequest(
                "manifest-owned-domain-tags",
                new LogicalExecutionFlags
                {
                    UseLearnedUnitWhenAvailable = false
                },
                units,
                new Dictionary<string, object>()));
        Assert.True(binding.IsSuccess, binding.ErrorMessage);
        var domainTagsField = binding.Value!.Flow.GetType().GetField(
            "_domainTags",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(domainTagsField);
        Assert.Equal(
            manifest.DomainTags,
            Assert.IsType<string[]>(
                domainTagsField.GetValue(binding.Value.Flow)));
    }

    [Theory]
    [InlineData("unit-id")]
    [InlineData("flow-id")]
    [InlineData("trust-state")]
    [InlineData("version")]
    [InlineData("training-text")]
    [InlineData("output-text")]
    [InlineData("predicate-ids")]
    [InlineData("domain-tags")]
    [InlineData("original-prompt")]
    [InlineData("created-at")]
    [InlineData("unit-family")]
    [InlineData("model-config")]
    [InlineData("lora-config")]
    [InlineData("runtime-hash")]
    [InlineData("replay-binding")]
    [InlineData("knn-corpus")]
    public void ExecutableRestore_TamperedSignedManifestFailsBeforeMutation(
        string tamperedField)
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var baseline = BuildValidTrustStateSnapshot(artifacts);
        var manifest = Assert.Single(baseline.GeneratedCapabilities);
        var replay = Assert.IsType<
            LogicalCapabilityTrainingReplayAttestation>(
            manifest.ReplayAttestation);
        var tampered = tamperedField switch
        {
            "unit-id" => manifest with { UnitId = manifest.UnitId + "-tampered" },
            "flow-id" => manifest with { FlowId = manifest.FlowId + "-tampered" },
            "trust-state" => manifest with
            {
                TrustState = nameof(LogicalUnitTrustState.Candidate)
            },
            "version" => manifest with { Version = "9.9.9" },
            "training-text" => manifest with
            {
                TrainingText = manifest.TrainingText + " tampered"
            },
            "output-text" => manifest with
            {
                OutputText = manifest.OutputText + " tampered"
            },
            "predicate-ids" => manifest with
            {
                PredicateIds = [.. manifest.PredicateIds, "TAMPERED"]
            },
            "domain-tags" => manifest with
            {
                DomainTags = [.. manifest.DomainTags, "tampered"]
            },
            "original-prompt" => manifest with
            {
                OriginalPrompt = manifest.OriginalPrompt + " tampered"
            },
            "created-at" => manifest with
            {
                CreatedAtUtc = manifest.CreatedAtUtc.AddTicks(1)
            },
            "unit-family" => manifest with
            {
                UnitFamilyId = manifest.UnitFamilyId + "-tampered"
            },
            "model-config" => manifest with
            {
                ContextLen = manifest.ContextLen + 1
            },
            "lora-config" => manifest with
            {
                LoraAlpha = manifest.LoraAlpha + 1f
            },
            "runtime-hash" => manifest with
            {
                RuntimeProvenanceSha256 = new string('0', 64)
            },
            "replay-binding" => manifest with
            {
                ReplayAttestation = replay with
                {
                    SourceBindings =
                        new LogicalCapabilityTrainingSourceBindingComposite(
                            [new string('9', 64)])
                }
            },
            "knn-corpus" => manifest with
            {
                KnnExamples = manifest.KnnExamples!
                    .Select((example, index) => index == 0
                        ? new GeneratedCapabilityKnnExample(
                            example.ExampleId,
                            example.TrainingText,
                            example.OutputText + " tampered",
                            example.PredicateIds,
                            example.DomainTags)
                        : example)
                    .ToArray()
            },
            _ => throw new InvalidOperationException()
        };
        var snapshot = baseline with
        {
            GeneratedCapabilities = [tampered]
        };
        var registries = BuildRegistriesWithSentinels();

        var result = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            registries.Units,
            registries.Flows);

        Assert.False(result.IsSuccess);
        Assert.Equal(default, result.Value);
        Assert.Equal(
            registries.UnitDescriptors,
            registries.Units.ListDescriptors());
        Assert.Equal(
            registries.FlowDescriptors,
            registries.Flows.ListDescriptors());
    }

    [Fact]
    public void ExecutableRestore_ValidHmacCannotAuthorizeDifferentKnnCorpus()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var baseline = BuildValidTrustStateSnapshot(artifacts);
        var hmacKey = Enumerable.Range(1, 32)
            .Select(static value => checked((byte)value))
            .ToArray();
        var signed = WithServerOwnedHmac(
            Assert.Single(baseline.GeneratedCapabilities),
            hmacKey);
        var changedExamples = signed.KnnExamples!
            .Select((example, index) => index == 0
                ? new GeneratedCapabilityKnnExample(
                    example.ExampleId,
                    example.TrainingText,
                    example.OutputText + " different corpus",
                    example.PredicateIds,
                    example.DomainTags)
                : example)
            .ToArray();
        var changedCorpus = GeneratedCapabilityKnnCorpusProjector.Project(
            changedExamples);
        Assert.True(changedCorpus.IsSuccess, changedCorpus.ErrorMessage);
        var differentCorpus = signed with
        {
            KnnExamples = changedCorpus.Value!.Examples,
            KnnCorpusSha256 = changedCorpus.Value.Sha256
        };
        var snapshot = baseline with
        {
            GeneratedCapabilities = [differentCorpus]
        };
        var verification =
            ServerOwnedLogicalCapabilityReplayVerificationComposition
                .CreateForTesting(_ => Convert.ToBase64String(hmacKey));
        Assert.True(verification.IsSuccess, verification.ErrorMessage);
        var signedDigest =
            GeneratedCapabilityExecutableManifestProjector.ProjectSha256(
                signed,
                signed.TokenizerSha256,
                signed.PolicyCheckpointSha256);
        Assert.True(signedDigest.IsSuccess, signedDigest.ErrorMessage);
        var validHmac = verification.Value!.VerifyExecutableManifest(
            signedDigest.Value!,
            signed.ReplayAttestation!,
            signed.ExecutableManifestAttestation!);
        Assert.True(validHmac.IsSuccess && validHmac.Value);
        var registries = BuildRegistriesWithSentinels();

        var result = TeachingRegistryPersistence.LoadIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            registries.Units,
            registries.Flows,
            verification.Value!);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            TeachingRegistryPersistence
                .GeneratedCapabilityReplayAttestationInvalidErrorCode,
            result.ErrorCode);
        Assert.Equal(
            registries.UnitDescriptors,
            registries.Units.ListDescriptors());
        Assert.Equal(
            registries.FlowDescriptors,
            registries.Flows.ListDescriptors());
    }

    [Fact]
    public void ManifestWrite_EscapeHeavyAggregateFailsBeforePayloadAllocation()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var manifest = Assert.Single(
            BuildValidTrustStateSnapshot(artifacts).GeneratedCapabilities)
            with
            {
                TrainingText = new string('"', 220_000),
                OutputText = new string('"', 220_000),
                OriginalPrompt = new string('"', 220_000)
            };
        using var destination = new MemoryStream();
        var payloadAllocated = false;
        TeachingRegistryPersistence
            .ManifestPayloadAllocationObserverForTesting =
                () => payloadAllocated = true;
        try
        {
            Assert.Throws<InvalidOperationException>(() =>
                TeachingRegistryPersistence.WriteGeneratedCapabilityManifest(
                    manifest,
                    destination));
        }
        finally
        {
            TeachingRegistryPersistence
                .ManifestPayloadAllocationObserverForTesting = null;
        }

        Assert.False(payloadAllocated);
        Assert.Equal(0, destination.Length);
    }

    [Theory]
    [InlineData("token-count")]
    [InlineData("depth")]
    public void ManifestInventory_ComplexityPreflightRunsBeforeFullPayloadAllocation(
        string complexityKind)
    {
        if (!OperatingSystem.IsWindows())
            return;

        var root = Path.Combine(
            Path.GetTempPath(),
            $"manifest-complexity-preflight-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var manifestPath = Path.Combine(
            root,
            TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);
        var payload = complexityKind switch
        {
            "token-count" => string.Concat(
                "[",
                string.Join(",", Enumerable.Repeat(
                    "0",
                    TeachingRegistryPersistence
                        .MaximumManifestJsonTokenCount)),
                "]"),
            "depth" => string.Concat(
                new string('[', TeachingRegistryPersistence
                    .MaximumManifestJsonDepth + 1),
                "0",
                new string(']', TeachingRegistryPersistence
                    .MaximumManifestJsonDepth + 1)),
            _ => throw new InvalidOperationException()
        };
        File.WriteAllText(manifestPath, payload);
        var payloadAllocated = false;
        var previousObserver = TeachingRegistryPersistence
            .ManifestReadPayloadAllocationObserverForTesting;
        TeachingRegistryPersistence
            .ManifestReadPayloadAllocationObserverForTesting =
                () => payloadAllocated = true;
        try
        {
            var inventory = TeachingRegistryPersistence
                .ReadGeneratedCapabilityManifestInventory(root);

            Assert.False(inventory.IsSuccess);
            Assert.Equal(
                "GENERATED_CAPABILITY_MANIFEST_TOO_LARGE",
                inventory.ErrorCode);
            Assert.False(payloadAllocated);
        }
        finally
        {
            TeachingRegistryPersistence
                .ManifestReadPayloadAllocationObserverForTesting =
                    previousObserver;
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LegacySchemaV1ManifestIsInventoryOnly()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var baseline = BuildValidTrustStateSnapshot(artifacts);
        var legacy = Assert.Single(baseline.GeneratedCapabilities) with
        {
            SchemaVersion = 1,
            UnitRegistrationOrder = null,
            FlowRegistrationOrder = null,
            ExecutableManifestAttestation = null
        };
        var save = TeachingRegistryPersistence.SaveGeneratedCapabilityManifest(
            legacy,
            Path.Combine(
                artifacts.Root,
                TeachingRegistryPersistence
                    .GeneratedCapabilityManifestFileName));
        Assert.True(save.IsSuccess, save.ErrorMessage);

        var inventory =
            TeachingRegistryPersistence.ReadGeneratedCapabilityManifestInventory(
                artifacts.Root);
        Assert.True(inventory.IsSuccess, inventory.ErrorMessage);
        Assert.Equal(1, Assert.Single(inventory.Value!.Sources).Manifest.SchemaVersion);

        var units = new InMemoryLogicalUnitRegistry();
        var flows = new InMemoryLogicalFlowRegistry();
        var load = LoadExecutableIntoRegistries(
            baseline with { GeneratedCapabilities = [legacy] },
            BuildTopologyWithSubPhase41(),
            units,
            flows);
        Assert.False(load.IsSuccess);
        Assert.Empty(units.ListDescriptors());
        Assert.Empty(flows.ListDescriptors());
    }

    [Fact]
    public void ExecutableRestore_PublicDummyVerifierCannotMintOrEnter()
    {
        var dummy = new PublicDummyReplayAuthority();
        Assert.False(
            typeof(ILogicalCapabilityReplayVerificationIssuer)
                .IsAssignableFrom(dummy.GetType()));
        Assert.DoesNotContain(
            typeof(LogicalCapabilityReplayVerificationCapability)
                .GetMethods(System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Static),
            method => string.Equals(
                method.Name,
                "Issue",
                StringComparison.Ordinal));

        var executableOverloads = typeof(TeachingRegistryPersistence)
            .GetMethods(System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static)
            .Where(static method =>
                method.Name is "LoadIntoRegistries" or
                    "LoadIntoRegistriesFromHeldArtifacts")
            .Where(static method => method.GetParameters().Any(parameter =>
                parameter.ParameterType ==
                    typeof(LogicalCapabilityReplayVerificationCapability)))
            .ToArray();
        Assert.NotEmpty(executableOverloads);
        Assert.All(executableOverloads, static method =>
            Assert.Equal(
                typeof(LogicalCapabilityReplayVerificationCapability),
                method.GetParameters()[^1].ParameterType));
        Assert.DoesNotContain(
            typeof(TeachingRegistryPersistence)
                .GetMethods(System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Static)
                .Where(static method =>
                    method.Name is "LoadIntoRegistries" or
                        "LoadIntoRegistriesFromHeldArtifacts")
                .SelectMany(static method => method.GetParameters()),
            parameter =>
                parameter.ParameterType == dummy.GetType() ||
                parameter.ParameterType ==
                    typeof(ILogicalCapabilityReplayVerificationIssuer));
    }

    [Fact]
    public void ExecutableRestore_CurrentManifestWithoutCapabilityFailsClosed()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var snapshot = BuildValidTrustStateSnapshot(artifacts);
        var units = new InMemoryLogicalUnitRegistry();
        var flows = new InMemoryLogicalFlowRegistry();

        var result = TeachingRegistryPersistence.LoadIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            units,
            flows);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            TeachingRegistryPersistence
                .GeneratedCapabilityReplayVerifierRequiredErrorCode,
            result.ErrorCode);
        Assert.Empty(units.ListDescriptors());
        Assert.Empty(flows.ListDescriptors());
    }

    [Fact]
    public void ExecutableManifestProjector_AggregatePayloadBeyondBudgetFailsClosed()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var baseline = BuildValidTrustStateSnapshot(artifacts);
        var manifest = Assert.Single(baseline.GeneratedCapabilities) with
        {
            TrainingText = new string(
                'a',
                GeneratedCapabilityExecutableManifestProjector
                    .MaximumCanonicalPayloadBytes / 2),
            OutputText = new string(
                'b',
                GeneratedCapabilityExecutableManifestProjector
                    .MaximumCanonicalPayloadBytes / 2)
        };

        var result = GeneratedCapabilityExecutableManifestProjector
            .ProjectSha256(
                manifest,
                manifest.TokenizerSha256,
                manifest.PolicyCheckpointSha256);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            GeneratedCapabilityExecutableManifestProjector.InvalidErrorCode,
            result.ErrorCode);
    }

    private sealed class PublicDummyReplayAuthority
        : ILogicalCapabilityTrainingAdmissionAuthority
    {
        public OperationResult<bool> VerifyReplay(
            LogicalCapabilityTrainingReplayAttestation attestation)
            => OperationResult<bool>.Ok(true);

        public OperationResult<ILogicalCapabilityTrainingAdmissionLease>
            AcquireTrainingLease(
                LogicalCapabilityTrainingAdmissionBatch batch)
            => OperationResult<ILogicalCapabilityTrainingAdmissionLease>.Fail(
                "Unavailable.",
                "UNAVAILABLE");
    }

    [Fact]
    public void ManifestFileDispositionInfo_UsesOneByteNativeBoolean()
    {
        var dispositionType = typeof(TeachingRegistryPersistence)
            .GetNestedType(
                "FileDispositionInformation",
                System.Reflection.BindingFlags.NonPublic);

        Assert.NotNull(dispositionType);
        Assert.Equal(1, Marshal.SizeOf(dispositionType!));
    }

    [Fact]
    public void ManifestIdentityCustody_CapabilityContractPublishesOrFailsBeforeMutation()
    {
        const string expectedCode =
            "GENERATED_CAPABILITY_MANIFEST_WINDOWS_IDENTITY_CUSTODY_REQUIRED";
        // Код ошибки оставлен историческим, чтобы старые журналы и правила
        // разбора не сломались. Сейчас он означает не "обязателен Windows",
        // а "эта платформа не умеет безопасно удерживать личность manifest".
        Assert.Equal(
            expectedCode,
            TeachingRegistryPersistence
                .GeneratedCapabilityManifestIdentityCustodyRequiredErrorCode);

        var root = Path.Combine(
            Path.GetTempPath(),
            $"manifest-platform-custody-{Guid.NewGuid():N}");
        var saveRoot = Path.Combine(root, "save-must-not-exist");
        var savePath = Path.Combine(
            saveRoot,
            TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);
        var inventoryRoot = Path.Combine(root, "inventory");
        try
        {
            var expectedManifest =
                BuildInventoryManifest("platform-save-unit", "PLAN", "4.1");
            var save = TeachingRegistryPersistence.SaveGeneratedCapabilityManifest(
                expectedManifest,
                savePath);
            // Ветвимся по реальной способности платформы, а не по имени ОС:
            // Windows и Linux x64 обязаны доказать безопасную публикацию,
            // остальные платформы должны отказать до создания каталога.
            if (LogicalCapabilityFileCustody.IsCurrentPlatformSupported)
            {
                Assert.True(save.IsSuccess, save.ErrorMessage);
                Assert.Equal(Path.GetFullPath(savePath), save.Value);
                Assert.True(File.Exists(savePath));

                var savedInventory = TeachingRegistryPersistence
                    .ReadGeneratedCapabilityManifestInventory(saveRoot);
                Assert.True(savedInventory.IsSuccess, savedInventory.ErrorMessage);
                var savedSource = Assert.Single(savedInventory.Value!.Sources);
                // Сравнение всего manifest защищает все смысловые поля паспорта
                // способности, а не только имя учебного блока.
                Assert.Equivalent(expectedManifest, savedSource.Manifest, strict: true);
                Assert.Equal(Path.GetFullPath(savePath), savedSource.CanonicalPath);
                return;
            }

            Assert.False(save.IsSuccess);
            Assert.Equal(expectedCode, save.ErrorCode);
            Assert.False(Directory.Exists(saveRoot));

            Directory.CreateDirectory(inventoryRoot);
            var inventoryPath = Path.Combine(
                inventoryRoot,
                TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);
            using (var destination = File.Create(inventoryPath))
            {
                TeachingRegistryPersistence.WriteGeneratedCapabilityManifest(
                    BuildInventoryManifest(
                        "platform-inventory-unit",
                        "PLAN",
                        "4.1"),
                    destination);
            }

            var inventory = TeachingRegistryPersistence
                .ReadGeneratedCapabilityManifestInventory(inventoryRoot);
            Assert.False(inventory.IsSuccess);
            Assert.Equal(expectedCode, inventory.ErrorCode);

            var capture = TeachingRegistryPersistence.Capture(
                "platform-custody-bundle",
                1,
                new InMemoryLogicalUnitRegistry(),
                new InMemoryLogicalFlowRegistry(),
                inventoryRoot);
            Assert.False(capture.IsSuccess);
            Assert.Equal(expectedCode, capture.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ManifestWriteBounds_FailBeforeDestinationOrFilesystemMutation()
    {
        var root = Path.Combine(Path.GetTempPath(), $"manifest-write-bound-{Guid.NewGuid():N}");
        var invalidPath = Path.Combine(root, "invalid",
            TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);
        var oversizedPath = Path.Combine(root, "oversized",
            TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);
        var valid = BuildInventoryManifest("bounded-unit", "PLAN", "4.1");
        var invalid = valid with { UnitId = "" };
        var oversized = valid with { TrainingText = new string('"', 600_000) };
        using var destination = new MemoryStream([1, 2, 3, 4]);
        var originalLength = destination.Length;
        try
        {
            var invalidSave = TeachingRegistryPersistence
                .SaveGeneratedCapabilityManifest(invalid, invalidPath);
            var oversizedSave = TeachingRegistryPersistence
                .SaveGeneratedCapabilityManifest(oversized, oversizedPath);
            Assert.False(invalidSave.IsSuccess);
            Assert.False(oversizedSave.IsSuccess);
            Assert.Equal("GENERATED_CAPABILITY_MANIFEST_WRITE_FAILED",
                invalidSave.ErrorCode);
            Assert.Equal(invalidSave.ErrorCode, oversizedSave.ErrorCode);
            Assert.Throws<InvalidOperationException>(() =>
                TeachingRegistryPersistence.WriteGeneratedCapabilityManifest(
                    oversized, destination));
            Assert.Equal(originalLength, destination.Length);
            Assert.False(Directory.Exists(root));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void ManifestInventory_RejectsOversizeAndHeldInPlaceWriter()
    {
        if (!OperatingSystem.IsWindows()) return;
        var root = Path.Combine(Path.GetTempPath(), $"manifest-read-bound-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var path = Path.Combine(root,
            TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);
        try
        {
            File.WriteAllBytes(path,
                new byte[checked((int)TeachingRegistryPersistence.MaximumManifestBytes + 1)]);
            var oversized = TeachingRegistryPersistence
                .ReadGeneratedCapabilityManifestInventory(root);
            Assert.False(oversized.IsSuccess);

            File.Delete(path);
            using (var output = File.Create(path))
                TeachingRegistryPersistence.WriteGeneratedCapabilityManifest(
                    BuildInventoryManifest("writer-unit", "PLAN", "4.1"), output);
            using var writer = new FileStream(path, FileMode.Open,
                FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
            var heldWriter = TeachingRegistryPersistence
                .ReadGeneratedCapabilityManifestInventory(root);
            Assert.False(heldWriter.IsSuccess);
            Assert.Equal("GENERATED_CAPABILITY_MANIFEST_READ_FAILED",
                heldWriter.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }
    [Fact]
    public void ManifestInventory_PathReplacementAfterReadFailsClosed()
    {
        if (!OperatingSystem.IsWindows()) return;
        var root = Path.Combine(Path.GetTempPath(), $"manifest-swap-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var path = Path.Combine(root,
            TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);
        var prior = TeachingRegistryPersistence
            .ManifestAfterReadBeforeIdentityReopenObserverForTesting;
        try
        {
            var manifest = BuildInventoryManifest("swap-unit", "PLAN", "4.1");
            using (var destination = File.Create(path))
                TeachingRegistryPersistence.WriteGeneratedCapabilityManifest(
                    manifest, destination);
            var replacement = File.ReadAllBytes(path);
            var invoked = 0;
            TeachingRegistryPersistence
                .ManifestAfterReadBeforeIdentityReopenObserverForTesting = observed =>
                {
                    Assert.Equal(Path.GetFullPath(path), Path.GetFullPath(observed));
                    Interlocked.Increment(ref invoked);
                    File.Delete(observed);
                    File.WriteAllBytes(observed, replacement);
                };

            var inventory = TeachingRegistryPersistence
                .ReadGeneratedCapabilityManifestInventory(root);

            Assert.False(inventory.IsSuccess);
            Assert.Equal("GENERATED_CAPABILITY_MANIFEST_CHANGED_DURING_READ",
                inventory.ErrorCode);
            Assert.Equal(1, invoked);
        }
        finally
        {
            TeachingRegistryPersistence
                .ManifestAfterReadBeforeIdentityReopenObserverForTesting = prior;
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }
    [Fact]
    public void SaveGeneratedCapabilityManifest_ExistingFinalPreservesBytes()
    {
        var root = Path.Combine(Path.GetTempPath(), $"f42-manifest-existing-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);
        var sentinel = Encoding.UTF8.GetBytes("sentinel-existing-manifest");
        File.WriteAllBytes(path, sentinel);
        try
        {
            var result = TeachingRegistryPersistence.SaveGeneratedCapabilityManifest(
                BuildInventoryManifest("new-unit", "BUG", "4.1"),
                path);

            Assert.False(result.IsSuccess);
            Assert.Equal("GENERATED_CAPABILITY_MANIFEST_ALREADY_EXISTS", result.ErrorCode);
            Assert.Equal(sentinel, File.ReadAllBytes(path));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task SaveGeneratedCapabilityManifest_ConcurrentSamePathExactlyOnePublisher()
    {
        var root = Path.Combine(Path.GetTempPath(), $"f42-manifest-concurrent-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);
        using var gate = new ManualResetEventSlim(false);
        try
        {
            Task<OperationResult<string>> Publish(string unitId) => Task.Run(() =>
            {
                gate.Wait();
                return TeachingRegistryPersistence.SaveGeneratedCapabilityManifest(
                    BuildInventoryManifest(unitId, "BUG", "4.1"),
                    path);
            });

            var first = Publish("publisher-one");
            var second = Publish("publisher-two");
            gate.Set();
            var results = await Task.WhenAll(first, second);

            Assert.Single(results, result => result.IsSuccess);
            Assert.Single(results, result =>
                !result.IsSuccess &&
                result.ErrorCode == "GENERATED_CAPABILITY_MANIFEST_ALREADY_EXISTS");
            using var stream = File.OpenRead(path);
            var manifest = JsonSerializer.Deserialize<GeneratedCapabilityCheckpointManifest>(stream);
            Assert.NotNull(manifest);
            Assert.Contains(manifest.UnitId, new[] { "publisher-one", "publisher-two" });
            Assert.Single(Directory.GetFiles(
                root,
                TeachingRegistryPersistence.GeneratedCapabilityManifestFileName,
                SearchOption.TopDirectoryOnly));
            Assert.Empty(Directory.GetFiles(root,
                $"{TeachingRegistryPersistence.GeneratedCapabilityManifestFileName}.tmp-*",
                SearchOption.TopDirectoryOnly));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void SaveGeneratedCapabilityManifest_DestinationRaceDoesNotOverwriteOrLeakTemp()
    {
        if (!OperatingSystem.IsWindows())
            return;
        var root = Path.Combine(Path.GetTempPath(),
            $"f42-manifest-handle-rename-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var path = Path.Combine(root,
            TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);
        var sentinel = Encoding.UTF8.GetBytes("foreign-winner");
        var prior = TeachingRegistryPersistence
            .ManifestBeforeHandleRenameObserverForTesting;
        try
        {
            TeachingRegistryPersistence
                .ManifestBeforeHandleRenameObserverForTesting =
                (_, canonicalPath) => File.WriteAllBytes(
                    canonicalPath, sentinel);

            var result = TeachingRegistryPersistence
                .SaveGeneratedCapabilityManifest(
                    BuildInventoryManifest("losing-publisher", "BUG", "4.1"),
                    path);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "GENERATED_CAPABILITY_MANIFEST_ALREADY_EXISTS",
                result.ErrorCode);
            Assert.Equal(sentinel, File.ReadAllBytes(path));
            Assert.Empty(Directory.GetFiles(root,
                $"{TeachingRegistryPersistence.GeneratedCapabilityManifestFileName}.tmp-*",
                SearchOption.TopDirectoryOnly));
        }
        finally
        {
            TeachingRegistryPersistence
                .ManifestBeforeHandleRenameObserverForTesting = prior;
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void SaveGeneratedCapabilityManifest_RejectsAncestorReparseBeforeExternalMutation()
    {
        if (!OperatingSystem.IsWindows())
            return;
        var fixtureRoot = Path.Combine(
            Path.GetTempPath(),
            $"manifest-publication-ancestor-reparse-{Guid.NewGuid():N}");
        var root = Path.Combine(fixtureRoot, "root");
        var outside = Path.Combine(fixtureRoot, "outside");
        var link = Path.Combine(root, "linked-ancestor");
        var outsideChild = Path.Combine(outside, "must-not-be-created");
        var path = Path.Combine(
            link,
            "must-not-be-created",
            TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(outside);
        var linkMechanism = DirectoryLinkMechanism.None;
        try
        {
            linkMechanism = CreateDirectoryLinkOrFail(link, outside);

            var result = TeachingRegistryPersistence
                .SaveGeneratedCapabilityManifest(
                    BuildInventoryManifest("ancestor-reparse", "BUG", "4.1"),
                    path);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "GENERATED_CAPABILITY_MANIFEST_WRITE_FAILED",
                result.ErrorCode);
            Assert.False(Directory.Exists(outsideChild));
            Assert.Empty(Directory.GetFiles(outside, "*", SearchOption.AllDirectories));
        }
        finally
        {
            RemoveDirectoryLinkIfPresent(link, linkMechanism);
            if (Directory.Exists(fixtureRoot))
                Directory.Delete(fixtureRoot, recursive: true);
        }
    }

    [Fact]
    public void SaveGeneratedCapabilityManifest_HoldsDirectoryIdentityAcrossRenameRace()
    {
        if (!OperatingSystem.IsWindows())
            return;
        var fixtureRoot = Path.Combine(
            Path.GetTempPath(),
            $"manifest-publication-directory-race-{Guid.NewGuid():N}");
        var intended = Path.Combine(fixtureRoot, "intended");
        var moved = Path.Combine(fixtureRoot, "moved-intended");
        var replacement = Path.Combine(fixtureRoot, "replacement");
        var path = Path.Combine(
            intended,
            TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);
        Directory.CreateDirectory(intended);
        Directory.CreateDirectory(replacement);
        var prior = TeachingRegistryPersistence
            .ManifestBeforeHandleRenameObserverForTesting;
        var originalMoved = false;
        Exception? replacementFailure = null;
        try
        {
            TeachingRegistryPersistence
                .ManifestBeforeHandleRenameObserverForTesting = (_, _) =>
                {
                    try
                    {
                        Directory.Move(intended, moved);
                        originalMoved = true;
                        Directory.Move(replacement, intended);
                    }
                    catch (Exception exception)
                        when (exception is IOException or UnauthorizedAccessException)
                    {
                        replacementFailure = exception;
                    }
                };

            var result = TeachingRegistryPersistence
                .SaveGeneratedCapabilityManifest(
                    BuildInventoryManifest("directory-race", "BUG", "4.1"),
                    path);

            if (originalMoved)
            {
                Assert.False(result.IsSuccess);
                Assert.Equal(
                    "GENERATED_CAPABILITY_MANIFEST_WRITE_FAILED",
                    result.ErrorCode);
                Assert.False(File.Exists(path));
                Assert.Empty(Directory.GetFiles(
                    moved,
                    $"{TeachingRegistryPersistence.GeneratedCapabilityManifestFileName}.tmp-*",
                    SearchOption.TopDirectoryOnly));
            }
            else
            {
                Assert.NotNull(replacementFailure);
                Assert.True(result.IsSuccess, result.ErrorMessage);
                Assert.True(File.Exists(path));
            }
        }
        finally
        {
            TeachingRegistryPersistence
                .ManifestBeforeHandleRenameObserverForTesting = prior;
            if (Directory.Exists(fixtureRoot))
                Directory.Delete(fixtureRoot, recursive: true);
        }
    }

    [Fact]
    public void Capture_DuplicateUnitIdFailsDeterministicallyWithoutPathLeak()
    {
        static (string Code, string Message) RunFixture(bool reverse)
        {
            var root = Path.Combine(Path.GetTempPath(), $"f42-inventory-duplicate-{Guid.NewGuid():N}");
            try
            {
                var children = reverse ? new[] { "z", "a" } : new[] { "a", "z" };
                foreach (var child in children)
                {
                    var directory = Path.Combine(root, child);
                    Directory.CreateDirectory(directory);
                    File.WriteAllText(
                        Path.Combine(directory, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName),
                        JsonSerializer.Serialize(BuildInventoryManifest("duplicate-unit", "BUG", "4.1")));
                }

                var result = TeachingRegistryPersistence.Capture(
                    "duplicate-bundle",
                    1,
                    new InMemoryLogicalUnitRegistry(),
                    new InMemoryLogicalFlowRegistry(),
                    root);
                Assert.False(result.IsSuccess);
                Assert.DoesNotContain(root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
                return (result.ErrorCode, result.ErrorMessage);
            }
            finally
            {
                if (Directory.Exists(root))
                    Directory.Delete(root, recursive: true);
            }
        }

        var first = RunFixture(reverse: false);
        var second = RunFixture(reverse: true);
        Assert.Equal("GENERATED_CAPABILITY_MANIFEST_DUPLICATE_UNIT_ID", first.Code);
        Assert.Equal(first, second);
        Assert.Contains("source #0002", first.Message, StringComparison.Ordinal);
        Assert.Contains("source #0001", first.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Capture_MalformedManifestReturnsFixedCodeWithoutExceptionText()
    {
        var root = Path.Combine(Path.GetTempPath(), $"f42-inventory-malformed-{Guid.NewGuid():N}");
        var child = Path.Combine(root, "secret-child");
        Directory.CreateDirectory(child);
        var content = "{ definitely-not-json SECRET_PAYLOAD";
        File.WriteAllText(
            Path.Combine(child, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName),
            content);
        try
        {
            var result = TeachingRegistryPersistence.Capture(
                "malformed-bundle",
                1,
                new InMemoryLogicalUnitRegistry(),
                new InMemoryLogicalFlowRegistry(),
                root);

            Assert.False(result.IsSuccess);
            Assert.Equal("GENERATED_CAPABILITY_MANIFEST_INVALID", result.ErrorCode);
            Assert.DoesNotContain(root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("SECRET_PAYLOAD", result.ErrorMessage, StringComparison.Ordinal);
            Assert.Null(result.Value);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Capture_RejectsChildDirectoryReparsePointBeforeDescent()
    {
        var fixtureRoot = Path.Combine(Path.GetTempPath(), $"f42-inventory-reparse-{Guid.NewGuid():N}");
        var root = Path.Combine(fixtureRoot, "root");
        var outside = Path.Combine(fixtureRoot, "outside");
        var link = Path.Combine(root, "linked-child");
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(outside);
        File.WriteAllText(
            Path.Combine(outside, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName),
            JsonSerializer.Serialize(BuildInventoryManifest("outside-unit", "BUG", "4.1")));
        var linkMechanism = DirectoryLinkMechanism.None;
        try
        {
            linkMechanism = CreateDirectoryLinkOrFail(link, outside);

            var result = TeachingRegistryPersistence.Capture(
                "reparse-bundle",
                1,
                new InMemoryLogicalUnitRegistry(),
                new InMemoryLogicalFlowRegistry(),
                root);

            Assert.False(result.IsSuccess);
            Assert.Equal("GENERATED_CAPABILITY_MANIFEST_PATH_INVALID", result.ErrorCode);
            Assert.DoesNotContain(fixtureRoot, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            RemoveDirectoryLinkIfPresent(link, linkMechanism);
            if (Directory.Exists(fixtureRoot))
                Directory.Delete(fixtureRoot, recursive: true);
        }
    }

    [Fact]
    public void LoadIntoRegistries_DuplicateManifestUnitIdFailsBeforeMutation()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var valid = BuildValidTrustStateSnapshot(artifacts);
        var manifest = Assert.Single(valid.GeneratedCapabilities);
        var snapshot = valid with
        {
            GeneratedCapabilities = [manifest, manifest with { CreatedAtUtc = manifest.CreatedAtUtc.AddSeconds(1) }]
        };
        var registries = BuildRegistriesWithSentinels();

        var result = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            registries.Units,
            registries.Flows);

        AssertFailedLoadWithoutRegistryMutation(
            result,
            "GENERATED_CAPABILITY_MANIFEST_DUPLICATE_UNIT_ID",
            "Generated capability manifest UnitId is duplicated.",
            registries);
    }

    [Theory]
    [InlineData("unit")]
    [InlineData("flow")]
    public void LoadIntoRegistries_EveryManifestRequiresOneExactExecutableBinding(
        string omittedBinding)
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var valid = BuildValidTrustStateSnapshot(artifacts);
        var snapshot = omittedBinding switch
        {
            "unit" => valid with { Units = [] },
            "flow" => valid with { Flows = [] },
            _ => throw new InvalidOperationException("Unknown test mutation.")
        };
        var registries = BuildRegistriesWithSentinels();

        var result = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            registries.Units,
            registries.Flows);

        AssertFailedLoadWithoutRegistryMutation(
            result,
            "GENERATED_CAPABILITY_RESTORE_INVALID",
            "Generated capability snapshot binding is invalid.",
            registries);
    }

    [Fact]
    public void LoadIntoRegistries_CandidateCapabilityRemainsRestorableForTrainingLifecycle()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var valid = BuildValidTrustStateSnapshot(artifacts);
        var candidate = WithExecutableManifestAttestation(
            Assert.Single(valid.GeneratedCapabilities) with
            {
                TrustState = nameof(LogicalUnitTrustState.Candidate)
            });
        var snapshot = valid with
        {
            Units =
            [
                valid.Units[0] with
                {
                    TrustState = nameof(LogicalUnitTrustState.Candidate)
                }
            ],
            GeneratedCapabilities = [candidate]
        };
        var units = new InMemoryLogicalUnitRegistry();
        var flows = new InMemoryLogicalFlowRegistry();

        var result = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            units,
            flows);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var descriptor = Assert.Single(units.ListDescriptors());
        Assert.Equal(LogicalUnitTrustState.Candidate, descriptor.TrustState);
        Assert.Single(flows.ListDescriptors());
    }

    [Fact]
    public void LoadIntoRegistries_LegacyFlowMultipleAdapterDomainGroupsFailsBeforeMutation()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var valid = BuildValidTrustStateSnapshot(artifacts);
        var baseline = Assert.Single(valid.GeneratedCapabilities);
        const string flowId = "generated-capability-flow-4-1";
        var bug = baseline with
        {
            UnitId = "legacy-bug",
            DomainTag = "BUG",
            FlowId = flowId,
            UnitRegistrationOrder = 2,
            FlowRegistrationOrder = 4
        };
        var plan = baseline with
        {
            UnitId = "legacy-plan",
            DomainTag = "PLAN",
            FlowId = flowId,
            UnitRegistrationOrder = 3,
            FlowRegistrationOrder = 4
        };
        var snapshot = valid with
        {
            Units = [BuildUnitEntry(bug, 2), BuildUnitEntry(plan, 3)],
            Flows = [BuildFlowEntry(flowId, baseline, 4)],
            GeneratedCapabilities = [bug, plan]
        };
        var registries = BuildRegistriesWithSentinels();

        var result = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            registries.Units,
            registries.Flows);

        AssertFailedLoadWithoutRegistryMutation(
            result,
            "GENERATED_CAPABILITY_FLOW_MANIFEST_AMBIGUOUS",
            "Generated capability flow manifest identity is ambiguous.",
            registries);
    }

    [Fact]
    public void LoadIntoRegistries_TaxonomyDomainTagsMismatchFailsBeforeMutation()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var valid = BuildValidTrustStateSnapshot(artifacts);
        var manifest = Assert.Single(valid.GeneratedCapabilities) with
        {
            FlowId = "generated-capability-flow-4-1",
            DomainTags = ["software-development"]
        };
        var snapshot = valid with
        {
            Flows = [BuildFlowEntry(manifest.FlowId, manifest, 3) with { DomainTags = ["quality-assurance"] }],
            GeneratedCapabilities = [manifest]
        };
        var registries = BuildRegistriesWithSentinels();

        var result = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            registries.Units,
            registries.Flows);

        AssertFailedLoadWithoutRegistryMutation(
            result,
            "GENERATED_CAPABILITY_FLOW_MANIFEST_NOT_FOUND",
            "Generated capability flow has no matching manifest.",
            registries);
    }

    [Fact]
    public void LoadIntoRegistries_FlowOrderConflictFailsBeforeUnitOrFlowMutation()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var valid = BuildValidTrustStateSnapshot(artifacts);
        var registries = BuildRegistriesWithSentinels();
        var collidingFlowOrder = Assert.Single(registries.FlowDescriptors).RegistrationOrder;
        var manifest = WithExecutableManifestAttestation(
            Assert.Single(valid.GeneratedCapabilities) with
            {
                FlowRegistrationOrder = collidingFlowOrder
            });
        var snapshot = valid with
        {
            Flows = [BuildFlowEntry(manifest.FlowId, manifest, collidingFlowOrder)],
            GeneratedCapabilities = [manifest]
        };
        var before = CaptureRegistryDescriptorBytes(registries.Units, registries.Flows);

        var result = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            registries.Units,
            registries.Flows);

        Assert.False(result.IsSuccess);
        Assert.Equal(default, result.Value);
        Assert.Equal("LOADED_FLOW_REGISTRATION_ORDER_CONFLICT", result.ErrorCode);
        Assert.Equal(
            "Loaded flow registration order conflicts with another registry tuple.",
            result.ErrorMessage);
        Assert.Equal(before, CaptureRegistryDescriptorBytes(registries.Units, registries.Flows));
        Assert.Equal(registries.UnitDescriptors, registries.Units.ListDescriptors());
        Assert.Equal(registries.FlowDescriptors, registries.Flows.ListDescriptors());
    }

    [Fact]
    public async Task LoadIntoRegistries_CompositeLeaseBlocksConcurrentFlowPublisherUntilBothAppendsComplete()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var snapshot = BuildValidTrustStateSnapshot(artifacts);
        var registries = BuildRegistriesWithSentinels();
        using var publisherAttempted = new ManualResetEventSlim();
        Task<OperationResult<LogicalFlowDescriptor>>? publisher = null;
        var priorObserver = TeachingRegistryPersistence
            .RestorePublicationLeaseAcquiredObserverForTesting;
        try
        {
            TeachingRegistryPersistence
                .RestorePublicationLeaseAcquiredObserverForTesting = () =>
                {
                    publisher = Task.Run(() =>
                    {
                        publisherAttempted.Set();
                        return registries.Flows.Register(
                            "concurrent-flow",
                            "concurrent-scenario",
                            "1.0.0",
                            new UniversalNineNodeDevelopmentFlow(
                                "4",
                                "4.1",
                                "backend-services",
                                ["software-development"],
                                ["IMPLEMENT"]));
                    });
                    Assert.True(
                        publisherAttempted.Wait(TimeSpan.FromSeconds(5)));
                    Assert.False(
                        SpinWait.SpinUntil(
                            () => publisher!.IsCompleted,
                            TimeSpan.FromMilliseconds(100)));
                };

            var result = LoadExecutableIntoRegistries(
                snapshot,
                BuildTopologyWithSubPhase41(),
                registries.Units,
                registries.Flows);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(2, result.Value);
        }
        finally
        {
            TeachingRegistryPersistence
                .RestorePublicationLeaseAcquiredObserverForTesting =
                priorObserver;
        }

        Assert.NotNull(publisher);
        var publicationResult = await publisher!;
        Assert.True(
            publicationResult.IsSuccess,
            publicationResult.ErrorMessage);
        Assert.Equal(2, registries.Units.ListDescriptors().Count);
        Assert.Equal(3, registries.Flows.ListDescriptors().Count);
        Assert.Contains(
            registries.Flows.ListDescriptors(),
            descriptor => descriptor.FlowId == "concurrent-flow" &&
                descriptor.RegistrationOrder == 4);
    }

    [Fact]
    public void LoadIntoRegistries_LegacyOneFlowForMultipleManifestsFailsAsAmbiguous()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var valid = BuildValidTrustStateSnapshot(artifacts);
        var baseline = Assert.Single(valid.GeneratedCapabilities);
        const string flowId = "generated-capability-flow-4-1";
        var old = baseline with
        {
            UnitId = "legacy-a",
            DomainTag = "BUG",
            FlowId = flowId,
            PredicateIds = ["OLD"],
            CreatedAtUtc = DateTimeOffset.UnixEpoch,
            TrainingStepCount = 100,
            UnitRegistrationOrder = 2,
            FlowRegistrationOrder = 5
        };
        var tiedLow = baseline with
        {
            UnitId = "legacy-b",
            DomainTag = "BUG",
            FlowId = flowId,
            PredicateIds = ["TIED_LOW"],
            CreatedAtUtc = DateTimeOffset.UnixEpoch.AddSeconds(1),
            TrainingStepCount = 2,
            UnitRegistrationOrder = 3,
            FlowRegistrationOrder = 5
        };
        var selected = baseline with
        {
            UnitId = "legacy-z",
            DomainTag = "BUG",
            FlowId = flowId,
            PredicateIds = ["SELECTED"],
            CreatedAtUtc = tiedLow.CreatedAtUtc,
            TrainingStepCount = tiedLow.TrainingStepCount,
            UnitRegistrationOrder = 4,
            FlowRegistrationOrder = 5
        };
        var snapshot = valid with
        {
            Units =
            [
                BuildUnitEntry(old, 2),
                BuildUnitEntry(tiedLow, 3),
                BuildUnitEntry(selected, 4)
            ],
            Flows = [BuildFlowEntry(flowId, baseline, 5)],
            GeneratedCapabilities = [selected, old, tiedLow]
        };
        var registries = BuildRegistriesWithSentinels();

        var result = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            registries.Units,
            registries.Flows);

        AssertFailedLoadWithoutRegistryMutation(
            result,
            "GENERATED_CAPABILITY_FLOW_MANIFEST_AMBIGUOUS",
            "Generated capability flow manifest identity is ambiguous.",
            registries);
    }
    [Fact]
    public void UnitRegistry_LoadEntries_PreservesRegistrationOrderAndResolveLatest()
    {
        var registry = new InMemoryLogicalUnitRegistry();
        var loadedUnit = new StaticStringUnit("loaded-unit", "loaded output");
        var descriptor = new LogicalUnitDescriptor(
            "loaded-unit",
            "generated-capability",
            "4.1",
            "1.0.0",
            LogicalUnitTrustState.Probationary,
            true,
            typeof(string),
            typeof(string),
            7);

        var load = registry.LoadEntries([new LogicalUnitRegistryLoadedEntry(descriptor, loadedUnit)]);

        Assert.True(load.IsSuccess, load.ErrorMessage);
        Assert.Equal(7, Assert.Single(registry.ListDescriptors()).RegistrationOrder);
        Assert.True(registry.ResolveLatest<string, string>("generated-capability", "4.1").IsSuccess);

        var next = registry.Register<string, string>(
            "next-unit",
            "generated-capability",
            "4.1",
            "1.0.1",
            LogicalUnitTrustState.Probationary,
            true,
            new StaticStringUnit("next-unit", "next output"));

        Assert.True(next.IsSuccess, next.ErrorMessage);
        Assert.True(next.Value!.RegistrationOrder > 7);
    }

    [Fact]
    public void FlowRegistry_LoadEntries_PreservesRegistrationOrderAndResolveBinding()
    {
        var registry = new InMemoryLogicalFlowRegistry();
        var descriptor = new LogicalFlowDescriptor(
            "loaded-flow-4-1",
            "loaded-scenario-4-1",
            "1.0.0",
            typeof(string),
            "4",
            "4.1",
            "backend-services",
            ["software-development"],
            9);

        var load = registry.LoadEntries([
            new LogicalFlowRegistryLoadedEntry(
                descriptor,
                new UniversalNineNodeDevelopmentFlow("4", "4.1", "backend-services", ["software-development"], ["IMPLEMENT"]))
        ]);

        Assert.True(load.IsSuccess, load.ErrorMessage);
        Assert.Equal(9, Assert.Single(registry.ListDescriptors()).RegistrationOrder);
        Assert.True(registry.ResolveBindingBySubPhase("4.1").IsSuccess);

        var next = registry.Register(
            "next-flow",
            "next-scenario",
            "1.0.1",
            new UniversalNineNodeDevelopmentFlow("4", "4.2", "frontend", ["software-development"], ["IMPLEMENT"]));

        Assert.True(next.IsSuccess, next.ErrorMessage);
        Assert.True(next.Value!.RegistrationOrder > 9);
    }

    [Fact]
    public void LoadRegistry_WithLegacyLearnedFlag_FailsBeforeMutation()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var baseline = BuildValidTrustStateSnapshot(artifacts);
        var snapshot = baseline with
        {
            Units =
            [
                Assert.Single(baseline.Units) with { IsLearned = true }
            ]
        };
        var manifest = Assert.Single(snapshot.GeneratedCapabilities);
        var manifestSave = TeachingRegistryPersistence.SaveGeneratedCapabilityManifest(
            manifest,
            Path.Combine(artifacts.Root, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName));
        Assert.True(manifestSave.IsSuccess, manifestSave.ErrorMessage);
        var units = new InMemoryLogicalUnitRegistry();
        var flows = new InMemoryLogicalFlowRegistry();

        var result = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            units,
            flows);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "GENERATED_CAPABILITY_RESTORE_INVALID",
            result.ErrorCode);
        Assert.Empty(units.ListDescriptors());
        Assert.Empty(flows.ListDescriptors());
    }

    [Fact]
    public void LoadRegistry_WithExplicitProvenance_ReconstructsCompleteModelConfig()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var legacySnapshot = BuildValidTrustStateSnapshot(artifacts);
        var manifest = WithExplicitArtifactProvenance(
            Assert.Single(legacySnapshot.GeneratedCapabilities),
            artifacts);
        var snapshot = legacySnapshot with { GeneratedCapabilities = [manifest] };
        var manifestSave = TeachingRegistryPersistence.SaveGeneratedCapabilityManifest(
            manifest,
            Path.Combine(artifacts.Root, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName));
        Assert.True(manifestSave.IsSuccess, manifestSave.ErrorMessage);
        var units = new InMemoryLogicalUnitRegistry();

        var result = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            units,
            new InMemoryLogicalFlowRegistry());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var resolved = units.ResolveLatestById<string, string>(manifest.UnitId);
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        var retrievalUnit = Assert.IsType<TransformerEmbeddingKnnLogicalUnit<string>>(resolved.Value!.Unit);
        var embeddingField = typeof(TransformerEmbeddingKnnLogicalUnit<string>).GetField(
            "_embeddingModel",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(embeddingField);
        var embeddingModel = embeddingField.GetValue(retrievalUnit);
        Assert.NotNull(embeddingModel);
        var encoderField = embeddingModel.GetType().GetField(
            "_encoder",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(encoderField);
        var restoredModel = Assert.IsType<LlmModel>(encoderField.GetValue(embeddingModel));

        Assert.Equal(artifacts.ModelConfig.VocabSize, restoredModel.Config.VocabSize);
        Assert.Equal(artifacts.ModelConfig.ContextLen, restoredModel.Config.ContextLen);
        Assert.Equal(artifacts.ModelConfig.HiddenDim, restoredModel.Config.HiddenDim);
        Assert.Equal(artifacts.ModelConfig.NumLayers, restoredModel.Config.NumLayers);
        Assert.Equal(artifacts.ModelConfig.NumHeads, restoredModel.Config.NumHeads);
        Assert.Equal(artifacts.ModelConfig.NumKvHeads, restoredModel.Config.NumKvHeads);
        Assert.Equal(artifacts.ModelConfig.FfnHiddenDim, restoredModel.Config.FfnHiddenDim);
        Assert.Equal(artifacts.ModelConfig.RopeTheta, restoredModel.Config.RopeTheta);
        Assert.Equal(artifacts.ModelConfig.RmsNormEps, restoredModel.Config.RmsNormEps);
    }

    [Theory]
    [InlineData("tokenizer")]
    [InlineData("policy")]
    public void LoadRegistry_WithExplicitProvenanceHashMismatch_FailsBeforeMutation(string artifact)
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var legacySnapshot = BuildValidTrustStateSnapshot(artifacts);
        var validManifest = WithExplicitArtifactProvenance(
            Assert.Single(legacySnapshot.GeneratedCapabilities),
            artifacts);
        var mismatchedHash = new string('0', 64);
        var manifest = artifact switch
        {
            "tokenizer" => validManifest with { TokenizerSha256 = mismatchedHash },
            "policy" => validManifest with { PolicyCheckpointSha256 = mismatchedHash },
            _ => throw new InvalidOperationException($"Unknown artifact '{artifact}'.")
        };
        var snapshot = legacySnapshot with { GeneratedCapabilities = [manifest] };
        var manifestSave = TeachingRegistryPersistence.SaveGeneratedCapabilityManifest(
            manifest,
            Path.Combine(artifacts.Root, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName));
        Assert.True(manifestSave.IsSuccess, manifestSave.ErrorMessage);
        var registries = BuildRegistriesWithSentinels();

        var result = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            registries.Units,
            registries.Flows);

        AssertFailedLoadWithoutRegistryMutation(
            result,
            TeachingRegistryPersistence
                .GeneratedCapabilityExecutableManifestInvalidErrorCode,
            "Generated capability executable manifest schema or provenance is invalid.",
            registries);
    }

    [Theory]
    [InlineData("rope-theta")]
    [InlineData("rms-norm-eps")]
    public void LoadRegistry_WithRuntimeConfigEnvelopeTampering_FailsBeforeMutation(string field)
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var legacySnapshot = BuildValidTrustStateSnapshot(artifacts);
        var validManifest = WithExplicitArtifactProvenance(
            Assert.Single(legacySnapshot.GeneratedCapabilities),
            artifacts);
        var manifest = field switch
        {
            "rope-theta" => validManifest with
            {
                RopeTheta = validManifest.RopeTheta!.Value + 1f
            },
            "rms-norm-eps" => validManifest with
            {
                RmsNormEps = validManifest.RmsNormEps!.Value * 2f
            },
            _ => throw new InvalidOperationException($"Unknown field '{field}'.")
        };
        var snapshot = legacySnapshot with { GeneratedCapabilities = [manifest] };
        var manifestSave = TeachingRegistryPersistence.SaveGeneratedCapabilityManifest(
            manifest,
            Path.Combine(artifacts.Root, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName));
        Assert.True(manifestSave.IsSuccess, manifestSave.ErrorMessage);
        var registries = BuildRegistriesWithSentinels();

        var result = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            registries.Units,
            registries.Flows);

        AssertFailedLoadWithoutRegistryMutation(
            result,
            TeachingRegistryPersistence
                .GeneratedCapabilityExecutableManifestInvalidErrorCode,
            "Generated capability executable manifest schema or provenance is invalid.",
            registries);
    }

    [Theory]
    [InlineData("alpha")]
    [InlineData("dropout")]
    [InlineData("target-set")]
    [InlineData("target-order")]
    public void LoadRegistry_WithRuntimeLoraEnvelopeTampering_FailsBeforeMutation(string field)
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var legacySnapshot = BuildValidTrustStateSnapshot(artifacts);
        var validManifest = WithExplicitArtifactProvenance(
            Assert.Single(legacySnapshot.GeneratedCapabilities),
            artifacts);
        var manifest = field switch
        {
            "alpha" => validManifest with { LoraAlpha = validManifest.LoraAlpha + 1f },
            "dropout" => validManifest with { LoraDropout = 0.25f },
            "target-set" => validManifest with { LoraTargetModules = ["q_proj"] },
            "target-order" => validManifest with
            {
                LoraTargetModules = validManifest.LoraTargetModules.Reverse().ToArray()
            },
            _ => throw new InvalidOperationException($"Unknown field '{field}'.")
        };
        var snapshot = legacySnapshot with { GeneratedCapabilities = [manifest] };
        var manifestSave = TeachingRegistryPersistence.SaveGeneratedCapabilityManifest(
            manifest,
            Path.Combine(artifacts.Root, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName));
        Assert.True(manifestSave.IsSuccess, manifestSave.ErrorMessage);
        var registries = BuildRegistriesWithSentinels();

        var result = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            registries.Units,
            registries.Flows);

        AssertFailedLoadWithoutRegistryMutation(
            result,
            TeachingRegistryPersistence
                .GeneratedCapabilityExecutableManifestInvalidErrorCode,
            "Generated capability executable manifest schema or provenance is invalid.",
            registries);
    }

    [Fact]
    public void LoadRegistry_LoadsVerifiedHeldCheckpointWhenPathIsReplacedAfterHash()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var legacySnapshot = BuildValidTrustStateSnapshot(artifacts);
        var manifest = WithExplicitArtifactProvenance(
            Assert.Single(legacySnapshot.GeneratedCapabilities),
            artifacts);
        var snapshot = legacySnapshot with { GeneratedCapabilities = [manifest] };
        var manifestSave = TeachingRegistryPersistence.SaveGeneratedCapabilityManifest(
            manifest,
            Path.Combine(artifacts.Root, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName));
        Assert.True(manifestSave.IsSuccess, manifestSave.ErrorMessage);

        var originalModel = new LlmModel(artifacts.ModelConfig);
        _ = LoraApplicator.Apply(originalModel, artifacts.LoraConfig);
        originalModel.LoadCheckpoint(artifacts.PolicyCheckpointPath);
        var originalState = SnapshotCheckpointState(originalModel);

        var replacementPath = Path.Combine(artifacts.Root, "replacement-policy.bin");
        var replacementModel = new LlmModel(artifacts.ModelConfig);
        _ = LoraApplicator.Apply(replacementModel, artifacts.LoraConfig);
        FillParametersDeterministically(replacementModel.Parameters(), 0.875f);
        replacementModel.SaveCheckpoint(replacementPath);
        Assert.False(CheckpointStatesEqual(
            originalState,
            SnapshotCheckpointState(replacementModel)));

        var observer = new PersistenceCheckpointSwapObserver(
            artifacts.PolicyCheckpointPath,
            replacementPath);
        var units = new InMemoryLogicalUnitRegistry();
        var result = TeachingRegistryPersistence.LoadIntoRegistriesForTesting(
            snapshot,
            BuildTopologyWithSubPhase41(),
            units,
            new InMemoryLogicalFlowRegistry(),
            observer,
            IssueReplayVerificationCapability());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(observer.WasInvoked);
        if (OperatingSystem.IsWindows())
            Assert.False(observer.ReplacementSucceeded);
        var restoredModel = ResolveRestoredModel(units, manifest.UnitId);
        AssertCheckpointStateEqual(originalState, restoredModel);
    }

    [Fact]
    public void Capture_SameRoleStringStaticUnitWithoutManifest_RemainsDeterministic()
    {
        var units = new InMemoryLogicalUnitRegistry();
        var registration = units.Register<string, string>(
            "static-generated-role-lookalike",
            DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole,
            "PLAN",
            DefaultLogicalCapabilityTrainer.GeneratedCapabilityVersion,
            LogicalUnitTrustState.Probationary,
            false,
            new StaticStringUnit("static-generated-role-lookalike", "stored static output"));
        Assert.True(registration.IsSuccess, registration.ErrorMessage);

        var result = TeachingRegistryPersistence.Capture(
            "static-lookalike",
            1,
            units,
            new InMemoryLogicalFlowRegistry(),
            checkpointRoot: string.Empty);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var entry = Assert.Single(result.Value!.Units);
        Assert.Equal(TeachingRegistrySnapshotKinds.DeterministicUnit, entry.UnitKind);
        Assert.False(entry.IsLearned);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Capture_GeneratedRetrievalWithoutManifest_FailsWithFixedCodeAndNoSnapshot(
        bool legacyLearnedFlag)
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var sourceSnapshot = BuildValidTrustStateSnapshot(artifacts);
        var manifest = Assert.Single(sourceSnapshot.GeneratedCapabilities);
        var retrievalUnit = BuildGeneratedRetrievalUnit(artifacts, manifest);
        var units = new InMemoryLogicalUnitRegistry();
        var registration = units.Register<string, string>(
            manifest.UnitId,
            manifest.UnitRole,
            manifest.DomainTag,
            manifest.Version,
            LogicalUnitTrustState.Probationary,
            legacyLearnedFlag,
            retrievalUnit);
        Assert.True(registration.IsSuccess, registration.ErrorMessage);

        var result = TeachingRegistryPersistence.Capture(
            "missing-generated-manifest",
            1,
            units,
            new InMemoryLogicalFlowRegistry(),
            checkpointRoot: string.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal("GENERATED_CAPABILITY_CAPTURE_INVALID", result.ErrorCode);
        Assert.Equal("Generated capability registry tuple is invalid.", result.ErrorMessage);
        Assert.Null(result.Value);
    }

    [Theory]
    [InlineData("unit")]
    [InlineData("flow")]
    public void Capture_ManifestRequiresExactLiveUnitAndFlow(
        string omittedBinding)
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var sourceSnapshot = BuildValidTrustStateSnapshot(artifacts);
        var manifest = Assert.Single(sourceSnapshot.GeneratedCapabilities);
        var manifestSave = TeachingRegistryPersistence.SaveGeneratedCapabilityManifest(
            manifest,
            Path.Combine(
                artifacts.Root,
                TeachingRegistryPersistence.GeneratedCapabilityManifestFileName));
        Assert.True(manifestSave.IsSuccess, manifestSave.ErrorMessage);

        var units = new InMemoryLogicalUnitRegistry();
        if (!string.Equals(omittedBinding, "unit", StringComparison.Ordinal))
        {
            var unitRegistration = units.Register<string, string>(
                manifest.UnitId,
                manifest.UnitRole,
                manifest.DomainTag,
                manifest.Version,
                LogicalUnitTrustState.Probationary,
                false,
                BuildGeneratedRetrievalUnit(artifacts, manifest));
            Assert.True(unitRegistration.IsSuccess, unitRegistration.ErrorMessage);
        }

        var flows = new InMemoryLogicalFlowRegistry();
        if (!string.Equals(omittedBinding, "flow", StringComparison.Ordinal))
        {
            var flowRegistration = flows.RegisterFactory<string>(
                manifest.FlowId,
                manifest.ScenarioId,
                manifest.Version,
                manifest.PhaseId,
                manifest.SubPhaseId,
                manifest.TaskType,
                manifest.DomainTags,
                new GeneratedCapabilityFlowFactory(
                    manifest.PhaseId,
                    manifest.SubPhaseId,
                    manifest.TaskType,
                    manifest.DomainTags,
                    manifest.PredicateIds,
                    manifest.DomainTag,
                    manifest.OriginalPrompt));
            Assert.True(flowRegistration.IsSuccess, flowRegistration.ErrorMessage);
        }

        var result = TeachingRegistryPersistence.Capture(
            "incomplete-live-binding",
            1,
            units,
            flows,
            artifacts.Root);

        Assert.False(result.IsSuccess);
        Assert.Equal("GENERATED_CAPABILITY_CAPTURE_INVALID", result.ErrorCode);
        Assert.Equal(
            "Generated capability registry tuple is invalid.",
            result.ErrorMessage);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Capture_ManifestBackedStaticExecutable_FailsWithFixedCodeAndNoSnapshot()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var snapshot = BuildValidTrustStateSnapshot(artifacts);
        var manifest = Assert.Single(snapshot.GeneratedCapabilities);
        var manifestSave = TeachingRegistryPersistence.SaveGeneratedCapabilityManifest(
            manifest,
            Path.Combine(artifacts.Root, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName));
        Assert.True(manifestSave.IsSuccess, manifestSave.ErrorMessage);

        var units = new InMemoryLogicalUnitRegistry();
        var registration = units.Register<string, string>(
            manifest.UnitId,
            manifest.UnitRole,
            manifest.DomainTag,
            manifest.Version,
            LogicalUnitTrustState.Probationary,
            false,
            new StaticStringUnit(manifest.UnitId, "not a transformer KNN retrieval unit"));
        Assert.True(registration.IsSuccess, registration.ErrorMessage);

        var result = TeachingRegistryPersistence.Capture(
            "invalid-executable",
            1,
            units,
            new InMemoryLogicalFlowRegistry(),
            artifacts.Root);

        Assert.False(result.IsSuccess);
        Assert.Equal("GENERATED_CAPABILITY_CAPTURE_INVALID", result.ErrorCode);
        Assert.Equal("Generated capability registry tuple is invalid.", result.ErrorMessage);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Capture_ManifestBackedLegacyLearnedFlagFailsClosed()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var sourceSnapshot = BuildValidTrustStateSnapshot(artifacts);
        var manifest = Assert.Single(sourceSnapshot.GeneratedCapabilities);
        var manifestSave = TeachingRegistryPersistence.SaveGeneratedCapabilityManifest(
            manifest,
            Path.Combine(artifacts.Root, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName));
        Assert.True(manifestSave.IsSuccess, manifestSave.ErrorMessage);

        var retrievalUnit = BuildGeneratedRetrievalUnit(artifacts, manifest);
        var units = new InMemoryLogicalUnitRegistry();
        var registration = units.Register<string, string>(
            manifest.UnitId,
            manifest.UnitRole,
            manifest.DomainTag,
            manifest.Version,
            LogicalUnitTrustState.Probationary,
            isLearned: true,
            retrievalUnit);
        Assert.True(registration.IsSuccess, registration.ErrorMessage);
        Assert.True(registration.Value!.IsLearned);
        var flows = new InMemoryLogicalFlowRegistry();
        var flowRegistration = flows.RegisterFactory<string>(
            manifest.FlowId,
            manifest.ScenarioId,
            manifest.Version,
            manifest.PhaseId,
            manifest.SubPhaseId,
            manifest.TaskType,
            manifest.DomainTags,
            new GeneratedCapabilityFlowFactory(
                manifest.PhaseId,
                manifest.SubPhaseId,
                manifest.TaskType,
                manifest.DomainTags,
                manifest.PredicateIds,
                manifest.DomainTag,
                manifest.OriginalPrompt));
        Assert.True(flowRegistration.IsSuccess, flowRegistration.ErrorMessage);

        var result = TeachingRegistryPersistence.Capture(
            "legacy-retrieval",
            1,
            units,
            flows,
            artifacts.Root);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "GENERATED_CAPABILITY_CAPTURE_INVALID",
            result.ErrorCode);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Capture_ManifestBackedRetrievalWithMismatchedTuple_FailsWithFixedCode()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var sourceSnapshot = BuildValidTrustStateSnapshot(artifacts);
        var manifest = Assert.Single(sourceSnapshot.GeneratedCapabilities);
        var manifestSave = TeachingRegistryPersistence.SaveGeneratedCapabilityManifest(
            manifest,
            Path.Combine(artifacts.Root, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName));
        Assert.True(manifestSave.IsSuccess, manifestSave.ErrorMessage);

        var retrievalUnit = BuildGeneratedRetrievalUnit(artifacts, manifest);
        var units = new InMemoryLogicalUnitRegistry();
        var registration = units.Register<string, string>(
            manifest.UnitId,
            manifest.UnitRole,
            manifest.DomainTag,
            "9.9.9",
            LogicalUnitTrustState.Probationary,
            isLearned: false,
            retrievalUnit);
        Assert.True(registration.IsSuccess, registration.ErrorMessage);

        var result = TeachingRegistryPersistence.Capture(
            "mismatched-retrieval",
            1,
            units,
            new InMemoryLogicalFlowRegistry(),
            artifacts.Root);

        Assert.False(result.IsSuccess);
        Assert.Equal("GENERATED_CAPABILITY_CAPTURE_INVALID", result.ErrorCode);
        Assert.Equal("Generated capability registry tuple is invalid.", result.ErrorMessage);
        Assert.Null(result.Value);
    }

    // Контроль 1 ремонта A27-K2: модуль подбора ответа с НЕПУСТЫМ каталогом правил ранжирования
    // нельзя молча записать как восстановимый, потому что опись снимка каталог не хранит.
    [Fact]
    public void Capture_RetrievalUnitWithConfiguredPrincipleCatalogue_FailsWithUnsupportedModeAndNoSnapshot()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var manifest = BuildFirstOrderCapabilityManifest(artifacts);
        SaveCapabilityManifestBesideCheckpoints(artifacts, manifest);

        // Обучаем модуль обычным путём, а затем включаем необязательную надстройку ранжирования.
        var trained = BuildGeneratedRetrievalUnit(artifacts, manifest);
        var catalog = KnnCandidatePrincipleCatalog.Create(
            manifest.DomainTag,
            [KnnDomainCandidatePrincipleProfiles.RussianTimeFrame(manifest.DomainTag)]);
        Assert.True(catalog.IsSuccess, catalog.ErrorMessage);
        var configured = trained.WithCandidatePrinciples(catalog.Value!);
        Assert.True(configured.IsSuccess, configured.ErrorMessage);
        // Режим действительно настроен, иначе контроль проверял бы не тот модуль.
        Assert.Equal(1, configured.Value!.CandidatePrincipleCount);

        var result = CaptureSingleCapability(
            artifacts,
            manifest,
            configured.Value!,
            "configured-principle-catalogue");

        // Отказ обязан быть явным и названным, а не общим сбоем связки.
        Assert.False(result.IsSuccess);
        Assert.Equal("UNSUPPORTED_PRINCIPLE_CATALOGUE_MODE", result.ErrorCode);
        Assert.Equal(
            "Generated capability candidate principle catalogue cannot be captured durably yet.",
            result.ErrorMessage);
        // Снимка нет вообще: частичный или «почти» сохранённый снимок не создаётся.
        Assert.Null(result.Value);
    }

    // Контроли 2 и 3 ремонта A27-K2: пустой каталог и отсутствующий каталог обязаны
    // сохраняться ровно как раньше, а восстановление — давать прежний ответ.
    [Fact]
    public async Task Capture_RetrievalUnitWithEmptyOrAbsentCatalogue_StaysIdenticalAndRestorable()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var manifest = BuildFirstOrderCapabilityManifest(artifacts);
        SaveCapabilityManifestBesideCheckpoints(artifacts, manifest);

        // Контроль 3: модуль вообще без каталога — это прежний рабочий путь продукта.
        var withoutCatalogue = BuildGeneratedRetrievalUnit(artifacts, manifest);
        Assert.Equal(0, withoutCatalogue.CandidatePrincipleCount);
        var absent = CaptureSingleCapability(
            artifacts,
            manifest,
            withoutCatalogue,
            "absent-principle-catalogue");
        Assert.True(absent.IsSuccess, absent.ErrorMessage);

        // Контроль 2: пустой каталог — это тоже прежнее поведение, а не настроенный режим.
        var emptyCatalog = KnnCandidatePrincipleCatalog.CreateEmpty(manifest.DomainTag);
        Assert.True(emptyCatalog.IsSuccess, emptyCatalog.ErrorMessage);
        var configuredEmpty = BuildGeneratedRetrievalUnit(artifacts, manifest)
            .WithCandidatePrinciples(emptyCatalog.Value!);
        Assert.True(configuredEmpty.IsSuccess, configuredEmpty.ErrorMessage);
        Assert.Equal(0, configuredEmpty.Value!.CandidatePrincipleCount);
        var empty = CaptureSingleCapability(
            artifacts,
            manifest,
            configuredEmpty.Value!,
            "empty-principle-catalogue");
        Assert.True(empty.IsSuccess, empty.ErrorMessage);

        // Оба снимка обязаны описывать одно и то же состояние: пустой каталог ничего не меняет.
        Assert.Equivalent(absent.Value!.Units, empty.Value!.Units, strict: true);
        Assert.Equivalent(absent.Value!.Flows, empty.Value!.Flows, strict: true);
        Assert.Equivalent(
            absent.Value!.GeneratedCapabilities,
            empty.Value!.GeneratedCapabilities,
            strict: true);

        // Восстановление из сохранённого снимка обязано вернуть прежний пустой режим.
        var restoredUnits = new InMemoryLogicalUnitRegistry();
        var restore = LoadExecutableIntoRegistries(
            empty.Value!,
            BuildTopologyWithSubPhase41(),
            restoredUnits,
            new InMemoryLogicalFlowRegistry());
        Assert.True(restore.IsSuccess, restore.ErrorMessage);
        var resolved = restoredUnits.ResolveLatestById<string, string>(manifest.UnitId);
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        var restored = Assert.IsType<TransformerEmbeddingKnnLogicalUnit<string>>(resolved.Value!.Unit);
        Assert.Equal(0, restored.CandidatePrincipleCount);

        // Ответ после восстановления обязан совпасть с ответом живого модуля.
        var live = await withoutCatalogue.ExecuteAsync(BuildRetrievalRequest(manifest));
        var afterRestore = await restored.ExecuteAsync(BuildRetrievalRequest(manifest));
        Assert.True(live.IsSuccess, live.ErrorMessage);
        Assert.True(afterRestore.IsSuccess, afterRestore.ErrorMessage);
        Assert.Equal(manifest.OutputText, afterRestore.Value!.Output);
        Assert.Equal(live.Value!.Output, afterRestore.Value!.Output);
    }

    // Контроль 4 ремонта A27-K2: снимок, записанный до этой починки, читается как прежде
    // и означает ровно прежний пустой режим ранжирования.
    [Fact]
    public void LoadIntoRegistries_PreRepairSnapshotWithoutCatalogue_RestoresTheEmptyMode()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        // Эта опись построена в форме до ремонта: поля каталога правил в ней нет вовсе.
        var snapshot = BuildValidTrustStateSnapshot(artifacts);
        var manifest = Assert.Single(snapshot.GeneratedCapabilities);
        var units = new InMemoryLogicalUnitRegistry();

        var restore = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            units,
            new InMemoryLogicalFlowRegistry());

        // Старый снимок обязан загрузиться без единого нового требования.
        Assert.True(restore.IsSuccess, restore.ErrorMessage);
        var resolved = units.ResolveLatestById<string, string>(manifest.UnitId);
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        var restored = Assert.IsType<TransformerEmbeddingKnnLogicalUnit<string>>(resolved.Value!.Unit);
        // Прежний пустой режим: ни одного правила ранжирования.
        Assert.Equal(0, restored.CandidatePrincipleCount);
        // И список составных частей не рекламирует ранжирование, которое не выполнялось.
        Assert.DoesNotContain("KDP01:CANDIDATE_PRINCIPLE_RANKING", restored.Manifest.IngredientIds);
    }

    // Положить паспорт способности рядом с контрольными точками, иначе опись снимка пуста.
    private static void SaveCapabilityManifestBesideCheckpoints(
        LoadableTrustStateArtifacts artifacts,
        GeneratedCapabilityCheckpointManifest manifest)
    {
        var manifestSave = TeachingRegistryPersistence.SaveGeneratedCapabilityManifest(
            manifest,
            Path.Combine(
                artifacts.Root,
                TeachingRegistryPersistence.GeneratedCapabilityManifestFileName));
        Assert.True(manifestSave.IsSuccess, manifestSave.ErrorMessage);
    }

    // Взять проверенный паспорт способности и сдвинуть оба порядка регистрации на первый,
    // чтобы живые реестры этих контролей состояли ровно из одной способности.
    private static GeneratedCapabilityCheckpointManifest BuildFirstOrderCapabilityManifest(
        LoadableTrustStateArtifacts artifacts)
    {
        var manifest = Assert.Single(BuildValidTrustStateSnapshot(artifacts).GeneratedCapabilities);
        // После смены полей исполняемая опись подписывается заново, как и в остальных контролях.
        return WithExecutableManifestAttestation(manifest with
        {
            UnitRegistrationOrder = 1,
            FlowRegistrationOrder = 1
        });
    }

    // Зарегистрировать ровно одну способность в живых реестрах и попробовать сохранить снимок.
    private static OperationResult<TeachingRegistrySnapshot> CaptureSingleCapability(
        LoadableTrustStateArtifacts artifacts,
        GeneratedCapabilityCheckpointManifest manifest,
        TransformerEmbeddingKnnLogicalUnit<string> retrievalUnit,
        string bundleId)
    {
        var units = new InMemoryLogicalUnitRegistry();
        var unitRegistration = units.Register<string, string>(
            manifest.UnitId,
            manifest.UnitRole,
            manifest.DomainTag,
            manifest.Version,
            LogicalUnitTrustState.Probationary,
            isLearned: false,
            retrievalUnit);
        Assert.True(unitRegistration.IsSuccess, unitRegistration.ErrorMessage);

        var flows = new InMemoryLogicalFlowRegistry();
        var flowRegistration = flows.RegisterFactory<string>(
            manifest.FlowId,
            manifest.ScenarioId,
            manifest.Version,
            manifest.PhaseId,
            manifest.SubPhaseId,
            manifest.TaskType,
            manifest.DomainTags,
            new GeneratedCapabilityFlowFactory(
                manifest.PhaseId,
                manifest.SubPhaseId,
                manifest.TaskType,
                manifest.DomainTags,
                manifest.PredicateIds,
                manifest.DomainTag,
                manifest.OriginalPrompt));
        Assert.True(flowRegistration.IsSuccess, flowRegistration.ErrorMessage);

        return TeachingRegistryPersistence.Capture(bundleId, 1, units, flows, artifacts.Root);
    }

    // Один и тот же запрос для живого и для восстановленного модуля, чтобы ответы сравнивались честно.
    private static LogicalUnitRequest<string> BuildRetrievalRequest(
        GeneratedCapabilityCheckpointManifest manifest)
        => new(
            "capture-principle-mode-run",
            manifest.UnitId,
            manifest.UnitRole,
            manifest.DomainTag,
            manifest.TrainingText,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());

    [Theory]
    [InlineData("wrong-kind")]
    [InlineData("wrong-role")]
    [InlineData("wrong-input-type")]
    [InlineData("wrong-version")]
    [InlineData("learned-flag")]
    public void LoadRegistry_WithInvalidGeneratedBinding_FailsBeforeEitherRegistryMutation(string mutation)
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var validSnapshot = BuildValidTrustStateSnapshot(artifacts);
        var validEntry = Assert.Single(validSnapshot.Units);
        var invalidEntry = mutation switch
        {
            "wrong-kind" => validEntry with { UnitKind = TeachingRegistrySnapshotKinds.DeterministicUnit },
            "wrong-role" => validEntry with { UnitRole = "different-role" },
            "wrong-input-type" => validEntry with { InputTypeName = typeof(int).AssemblyQualifiedName! },
            "wrong-version" => validEntry with { Version = "9.9.9" },
            "learned-flag" => validEntry with { IsLearned = true },
            _ => throw new InvalidOperationException($"Unknown mutation '{mutation}'.")
        };
        var snapshot = validSnapshot with { Units = [invalidEntry] };
        var registries = BuildRegistriesWithSentinels();

        var result = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            registries.Units,
            registries.Flows);

        AssertFailedLoadWithoutRegistryMutation(
            result,
            "GENERATED_CAPABILITY_RESTORE_INVALID",
            "Generated capability snapshot binding is invalid.",
            registries);
    }

    [Fact]
    public void LoadRegistry_WithBlankUnitTrustState_FailsWithTrustStateInvalid()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var validSnapshot = BuildValidTrustStateSnapshot(artifacts);
        var snapshot = validSnapshot with
        {
            Units = [Assert.Single(validSnapshot.Units) with { TrustState = "" }]
        };
        var registries = BuildRegistriesWithSentinels();

        Assert.Same(validSnapshot.GeneratedCapabilities, snapshot.GeneratedCapabilities);

        var result = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            registries.Units,
            registries.Flows);

        AssertFailedLoadWithoutRegistryMutation(
            result,
            "TEACHING_UNIT_TRUST_STATE_INVALID",
            "Teaching unit trust state is invalid.",
            registries);
    }

    [Fact]
    public void LoadRegistry_WithUnknownManifestTrustState_FailsWithCapabilityTrustStateInvalid()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var validSnapshot = BuildValidTrustStateSnapshot(artifacts);
        var snapshot = validSnapshot with
        {
            GeneratedCapabilities =
            [
                Assert.Single(validSnapshot.GeneratedCapabilities) with { TrustState = "Unknown" }
            ]
        };
        var registries = BuildRegistriesWithSentinels();

        Assert.Same(validSnapshot.Units, snapshot.Units);

        var result = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            registries.Units,
            registries.Flows);

        AssertFailedLoadWithoutRegistryMutation(
            result,
            "GENERATED_CAPABILITY_TRUST_STATE_INVALID",
            "Generated capability trust state is invalid.",
            registries);
    }

    [Fact]
    public void LoadRegistry_WithUnitAndManifestTrustMismatch_FailsWithMismatchCode()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var validSnapshot = BuildValidTrustStateSnapshot(artifacts);
        var snapshot = validSnapshot with
        {
            GeneratedCapabilities =
            [
                Assert.Single(validSnapshot.GeneratedCapabilities) with
                {
                    TrustState = nameof(LogicalUnitTrustState.Trusted)
                }
            ]
        };
        var registries = BuildRegistriesWithSentinels();

        Assert.Same(validSnapshot.Units, snapshot.Units);

        var result = LoadExecutableIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            registries.Units,
            registries.Flows);

        AssertFailedLoadWithoutRegistryMutation(
            result,
            "GENERATED_CAPABILITY_TRUST_STATE_MISMATCH",
            "Generated capability trust state does not match its teaching unit.",
            registries);
    }

    [Fact]
    public async Task CapabilityTrainer_WritesLoadableGeneratedCapabilityArtifacts()
    {
        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"capability-artifacts-{Guid.NewGuid():N}");
        try
        {
            var topology = BuildTopologyWithSubPhase41();
            var flows = new InMemoryLogicalFlowRegistry();
            var units = new InMemoryLogicalUnitRegistry();
            var branch = BuildKnowledgeEdgeBranch("4.1");
            var trainer = new DefaultLogicalCapabilityTrainer(
                checkpointRoot,
                admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority());

            var result = await trainer.TrainAndRegisterAsync(new LogicalCapabilityTrainingRequest(
                "artifact-run",
                BuildRequest("artifact-run"),
                [branch],
                topology,
                flows,
                units)
            {
                Domain = Domain.PLAN
            });

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.True(result.Value!.CapabilityRegistered);

            var manifestPath = Assert.Single(Directory.GetFiles(
                checkpointRoot,
                TeachingRegistryPersistence.GeneratedCapabilityManifestFileName,
                SearchOption.AllDirectories));
            var snapshot = TeachingRegistryPersistence.Capture("bundle-artifact", 1, units, flows, checkpointRoot);

            Assert.True(snapshot.IsSuccess, snapshot.ErrorMessage);
            var manifest = Assert.Single(snapshot.Value!.GeneratedCapabilities);
            var generatedUnit = Assert.Single(units.ListDescriptors(), descriptor =>
                descriptor.UnitRole == DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole);
            Assert.Equal(generatedUnit.UnitId, manifest.UnitId);
            Assert.Equal(Assert.Single(flows.ListDescriptors()).FlowId, manifest.FlowId);
            Assert.True(File.Exists(manifest.TokenizerPath));
            Assert.True(File.Exists(manifest.PolicyCheckpointPath));
            Assert.Equal(Path.GetFullPath(manifestPath), Path.GetFullPath(Directory.GetFiles(
                checkpointRoot,
                TeachingRegistryPersistence.GeneratedCapabilityManifestFileName,
                SearchOption.AllDirectories).Single()));
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task SnapshotLoad_MakesPriorRowVisibleWithoutCapabilityGeneration()
    {
        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"snapshot-load-{Guid.NewGuid():N}");
        try
        {
            var topology = BuildTopologyWithSubPhase41();
            var rowOneFlows = new InMemoryLogicalFlowRegistry();
            var rowOneUnits = new InMemoryLogicalUnitRegistry();
            var trainer = new DefaultLogicalCapabilityTrainer(
                checkpointRoot,
                admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority());
            var training = await trainer.TrainAndRegisterAsync(new LogicalCapabilityTrainingRequest(
                "row-1-run",
                BuildRequest("row-1-run"),
                [BuildKnowledgeEdgeBranch("4.1")],
                topology,
                rowOneFlows,
                rowOneUnits)
            {
                Domain = Domain.PLAN
            });
            Assert.True(training.IsSuccess, training.ErrorMessage);

            var snapshot = TeachingRegistryPersistence.Capture("bundle-snapshot", 1, rowOneUnits, rowOneFlows, checkpointRoot);
            Assert.True(snapshot.IsSuccess, snapshot.ErrorMessage);

            var loadedFlows = new InMemoryLogicalFlowRegistry();
            var loadedUnits = new InMemoryLogicalUnitRegistry();
            var load = LoadExecutableIntoRegistries(
                snapshot.Value!,
                topology,
                loadedUnits,
                loadedFlows);
            Assert.True(load.IsSuccess, load.ErrorMessage);

            var binding = loadedFlows.ResolveBindingBySubPhase(
                "4.1",
                new LogicalFlowFactoryRequest(
                    "row-2-run",
                    new LogicalExecutionFlags(),
                    loadedUnits,
                    new Dictionary<string, object>()));
            Assert.True(binding.IsSuccess, binding.ErrorMessage);

            var result = await binding.Value!.Flow.ExecuteAsync(new LogicalFlowRequest<string>(
                "row-2-run",
                binding.Value.FlowId,
                binding.Value.ScenarioId,
                "Implement the backend service and provider connector.",
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal("PASS", result.Value!.OverallVerdict);
            Assert.Equal("True", result.Value.Metrics["retrieval_unit_selected"]);
            Assert.Equal("False", result.Value.Metrics["learned_unit_selected"]);
            Assert.False(result.Value.Metrics.ContainsKey("learned_unit_id"));
            Assert.False(result.Value.Metrics.ContainsKey("learned_unit_confidence"));
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task GeneratedCapabilityManifest_OriginalPromptRoundTripsIntoRestoredFlow()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var baseline = BuildValidTrustStateSnapshot(artifacts);
        var manifest = Assert.Single(baseline.GeneratedCapabilities) with
        {
            OriginalPrompt = "implement the backend service",
            OutputText = "RESTORED_OUTPUT: use the provider connector."
        };

        var roundTrip = await RoundTripGeneratedFlowAsync(
            artifacts,
            baseline,
            manifest,
            manifest.OriginalPrompt,
            includeRuntimeTopology: true);

        Assert.Contains(
            "For your request: \"implement the backend service\"",
            roundTrip.Before.FinalHumanResult,
            StringComparison.Ordinal);
        Assert.Equal(roundTrip.Before.FinalHumanResult, roundTrip.After.FinalHumanResult);
        Assert.Equal(manifest.OriginalPrompt, roundTrip.PersistedManifest.OriginalPrompt);
    }

    [Fact]
    public async Task GeneratedCapabilityManifest_MultilinePromptSuppressionSurvivesRestoreExactly()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var baseline = BuildValidTrustStateSnapshot(artifacts);
        const string multilinePromptAndOutput = "implement the\r\nbackend service";
        var manifest = Assert.Single(baseline.GeneratedCapabilities) with
        {
            OriginalPrompt = multilinePromptAndOutput,
            OutputText = multilinePromptAndOutput
        };

        var roundTrip = await RoundTripGeneratedFlowAsync(
            artifacts,
            baseline,
            manifest,
            multilinePromptAndOutput,
            includeRuntimeTopology: true);

        Assert.Equal(roundTrip.Before.FinalHumanResult, roundTrip.After.FinalHumanResult);
        Assert.StartsWith(
            multilinePromptAndOutput,
            roundTrip.After.FinalHumanResult,
            StringComparison.Ordinal);
        Assert.Equal(
            roundTrip.After.FinalHumanResult.IndexOf(multilinePromptAndOutput, StringComparison.Ordinal),
            roundTrip.After.FinalHumanResult.LastIndexOf(multilinePromptAndOutput, StringComparison.Ordinal));
        Assert.DoesNotContain(
            "For your request:",
            roundTrip.After.FinalHumanResult,
            StringComparison.Ordinal);
    }

    [Fact]
    public void SnapshotLoad_MissingOptionalOriginalPromptRestoresAsEmpty()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var baseline = BuildValidTrustStateSnapshot(artifacts);
        var manifest = Assert.Single(baseline.GeneratedCapabilities);
        var snapshot = baseline with
        {
            Flows = [BuildFlowEntry(manifest.FlowId, manifest, 3)]
        };
        var snapshotPath = Path.Combine(artifacts.Root, "legacy-snapshot-without-original-prompt.json");
        var legacyJson = JsonNode.Parse(JsonSerializer.Serialize(snapshot))!.AsObject();
        var legacyManifest = legacyJson["GeneratedCapabilities"]!.AsArray()[0]!.AsObject();
        Assert.True(legacyManifest.Remove("OriginalPrompt"));
        File.WriteAllText(snapshotPath, legacyJson.ToJsonString());

        var loadedSnapshot = TeachingRegistryPersistence.Load(snapshotPath);

        Assert.True(loadedSnapshot.IsSuccess, loadedSnapshot.ErrorMessage);
        var restoredManifest = Assert.Single(loadedSnapshot.Value!.GeneratedCapabilities);
        Assert.Equal(string.Empty, restoredManifest.OriginalPrompt);
        var units = new InMemoryLogicalUnitRegistry();
        var flows = new InMemoryLogicalFlowRegistry();
        var load = LoadExecutableIntoRegistries(
            loadedSnapshot.Value,
            BuildTopologyWithSubPhase41(),
            units,
            flows);
        Assert.True(load.IsSuccess, load.ErrorMessage);
        Assert.Single(units.ListDescriptors());
        Assert.Single(flows.ListDescriptors());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task LoadIntoRegistries_OptionalOriginalPromptRestoresWithoutPromptAnnotation(
        string optionalPrompt)
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var baseline = BuildValidTrustStateSnapshot(artifacts);
        var manifest = WithExecutableManifestAttestation(
            Assert.Single(baseline.GeneratedCapabilities) with
            {
                OriginalPrompt = optionalPrompt
            });
        var snapshot = baseline with
        {
            Flows = [BuildFlowEntry(manifest.FlowId, manifest, 3)],
            GeneratedCapabilities = [manifest]
        };

        var execution = await LoadAndExecuteGeneratedFlowAsync(
            snapshot,
            BuildTopologyWithSubPhase41(),
            "runtime request with no persisted prompt",
            includeRuntimeTopology: true);

        Assert.True(execution.IsSuccess, execution.ErrorMessage);
        Assert.StartsWith(manifest.OutputText, execution.Value!.FinalHumanResult, StringComparison.Ordinal);
        Assert.DoesNotContain("For your request:", execution.Value.FinalHumanResult, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecutableRestore_LiveTopologyDriftCannotInfluenceLearnedOutput()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var snapshot = BuildValidTrustStateSnapshot(artifacts);
        var manifest = Assert.Single(snapshot.GeneratedCapabilities);
        var liveTopology = new InMemoryLogicalTopologyRegistry();
        var liveSubPhase = liveTopology.AddOrUpdateSubPhase(
            new LogicalSubPhase(
                manifest.SubPhaseId,
                manifest.PhaseId,
                "UNSIGNED_LIVE_NAME",
                "UNSIGNED_LIVE_PURPOSE",
                manifest.DomainTags,
                ["UNSIGNED_LIVE_PREDICATE"],
                ["unsigned-live-topology"]));
        Assert.True(liveSubPhase.IsSuccess, liveSubPhase.ErrorMessage);

        var execution = await LoadAndExecuteGeneratedFlowAsync(
            snapshot,
            liveTopology,
            "execute the restored signed capability",
            includeRuntimeTopology: true);

        Assert.True(execution.IsSuccess, execution.ErrorMessage);
        Assert.Contains(
            $"Sub-phase {manifest.SubPhaseId} ({manifest.TaskType})",
            execution.Value!.FinalHumanResult,
            StringComparison.Ordinal);
        Assert.Contains(
            $"Focus areas: {string.Join(", ", manifest.PredicateIds)}",
            execution.Value.FinalHumanResult,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "UNSIGNED_LIVE_NAME",
            execution.Value.FinalHumanResult,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "UNSIGNED_LIVE_PURPOSE",
            execution.Value.FinalHumanResult,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "UNSIGNED_LIVE_PREDICATE",
            execution.Value.FinalHumanResult,
            StringComparison.Ordinal);
    }

    [Fact]
    public void ManifestInventory_WrongTypeOriginalPromptFailsClosedWithoutPromptLeak()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var manifest = Assert.Single(BuildValidTrustStateSnapshot(artifacts).GeneratedCapabilities);
        var manifestJson = JsonNode.Parse(JsonSerializer.Serialize(manifest))!.AsObject();
        manifestJson["OriginalPrompt"] = new JsonObject
        {
            ["secret"] = "DO_NOT_LEAK_THIS_PROMPT"
        };
        File.WriteAllText(
            Path.Combine(artifacts.Root, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName),
            manifestJson.ToJsonString());

        var result = TeachingRegistryPersistence.ReadGeneratedCapabilityManifestInventory(artifacts.Root);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("GENERATED_CAPABILITY_MANIFEST_INVALID", result.ErrorCode);
        Assert.Equal(
            "Generated capability manifest source #0001 is invalid.",
            result.ErrorMessage);
        Assert.DoesNotContain("DO_NOT_LEAK_THIS_PROMPT", result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TeachingSessionBuilder_LoadsPriorSnapshotBeforeNextRow()
    {
        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"teaching-session-{Guid.NewGuid():N}");
        var snapshotPath = Path.Combine(checkpointRoot, "bundle-registry-snapshot.json");
        var admissionAuthority =
            new RecordingLogicalCapabilityTrainingAdmissionAuthority();
        var replayVerification =
            LogicalCapabilityReplayVerificationCapability.Issue(
                admissionAuthority);
        Assert.True(
            replayVerification.IsSuccess,
            replayVerification.ErrorMessage);
        try
        {
            var rowOneSession = TeachingSessionBuilder.Open(
                new TeachingNullSessionStorage(),
                "bundle-teaching-session",
                checkpointRoot,
                snapshotPath,
                admissionAuthority,
                replayVerification.Value!);
            Assert.True(rowOneSession.IsSuccess, rowOneSession.ErrorMessage);
            var rowOneSubPhase = rowOneSession.Value!.Topology
                .ResolveSubPhase("4.1");
            Assert.True(rowOneSubPhase.IsSuccess, rowOneSubPhase.ErrorMessage);

            var rowOne = await new DefaultLogicalCapabilityTrainer(
                checkpointRoot,
                admissionAuthority: admissionAuthority).TrainAndRegisterAsync(
                new LogicalCapabilityTrainingRequest(
                    "row-one-teaching-session",
                    BuildRequest("row-one-teaching-session"),
                    [BuildKnowledgeEdgeBranch(
                        "4.1",
                        rowOneSubPhase.Value!.SubPhase)],
                    rowOneSession.Value.Topology,
                    rowOneSession.Value.Flows,
                    rowOneSession.Value.Units)
                {
                    Domain = Domain.PLAN
                });
            Assert.True(rowOne.IsSuccess, rowOne.ErrorMessage);
            Assert.True(rowOne.Value!.CapabilityRegistered);

            var saved = rowOneSession.Value.SaveSnapshot(1);
            Assert.True(saved.IsSuccess, saved.ErrorMessage);
            Assert.True(File.Exists(snapshotPath));

            var rowTwoSession = TeachingSessionBuilder.Open(
                new TeachingNullSessionStorage(),
                "bundle-teaching-session",
                checkpointRoot,
                snapshotPath,
                admissionAuthority,
                replayVerification.Value!);
            Assert.True(rowTwoSession.IsSuccess, rowTwoSession.ErrorMessage);

            Assert.Contains(rowTwoSession.Value!.Units.ListDescriptors(), descriptor =>
                descriptor.UnitRole == DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole &&
                descriptor.DomainTag == Domain.PLAN.ToString());
            Assert.Contains(rowTwoSession.Value.Flows.ListDescriptors(), descriptor =>
                descriptor.FlowId.StartsWith(DefaultLogicalCapabilityTrainer.GeneratedCapabilityFlowIdPrefix, StringComparison.Ordinal) &&
                descriptor.SubPhaseId == "4.1");

            var rowTwo = await rowTwoSession.Value.Assistant.RunAsync(
                "Implement the backend service and provider connector for the integration layer.");

            Assert.True(rowTwo.IsSuccess, rowTwo.ErrorMessage);
            Assert.Equal("PASS", rowTwo.Value!.OverallVerdict);
            Assert.Equal("True", rowTwo.Value.Metrics["retrieval_unit_selected"]);
            Assert.Equal("False", rowTwo.Value.Metrics["learned_unit_selected"]);
            Assert.False(rowTwo.Value.Metrics.ContainsKey("learned_unit_id"));
            Assert.False(rowTwo.Value.Metrics.ContainsKey("learned_unit_confidence"));
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    private static GeneratedCapabilityCheckpointManifest BuildInventoryManifest(
        string unitId,
        string domainTag,
        string subPhaseId)
    {
        return new GeneratedCapabilityCheckpointManifest(
            unitId,
            DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole,
            domainTag,
            DefaultLogicalCapabilityTrainer.GeneratedCapabilityVersion,
            nameof(LogicalUnitTrustState.Probationary),
            $"generated-capability-flow-{subPhaseId.Replace('.', '-')}",
            $"generated-capability-{subPhaseId.Replace('.', '-')}",
            subPhaseId.Split('.')[0],
            subPhaseId,
            "test-task",
            "training text",
            "PASS",
            $"example-{unitId}",
            ["IMPLEMENT"],
            ["software-development"],
            "policy.bin",
            "tokenizer.json",
            512,
            64,
            32,
            4,
            4,
            2,
            64,
            2,
            4,
            0,
            ["q_proj", "v_proj"],
            DateTimeOffset.UnixEpoch,
            UnitFamilyId: $"generated-capability-{domainTag.ToLowerInvariant()}");
    }

    private static TeachingUnitRegistryEntry BuildUnitEntry(
        GeneratedCapabilityCheckpointManifest manifest,
        long registrationOrder)
    {
        return new TeachingUnitRegistryEntry(
            manifest.UnitId,
            manifest.UnitRole,
            manifest.DomainTag,
            manifest.Version,
            manifest.TrustState,
            false,
            typeof(string).AssemblyQualifiedName!,
            typeof(string).AssemblyQualifiedName!,
            registrationOrder,
            TeachingRegistrySnapshotKinds.GeneratedCapabilityUnit);
    }

    private static TeachingFlowRegistryEntry BuildFlowEntry(
        string flowId,
        GeneratedCapabilityCheckpointManifest manifest,
        long registrationOrder)
    {
        return new TeachingFlowRegistryEntry(
            flowId,
            manifest.ScenarioId,
            manifest.Version,
            typeof(string).AssemblyQualifiedName!,
            manifest.PhaseId,
            manifest.SubPhaseId,
            manifest.TaskType,
            manifest.DomainTags,
            registrationOrder,
            TeachingRegistrySnapshotKinds.GeneratedCapabilityFlow);
    }

    private static InMemoryLogicalTopologyRegistry BuildTopologyWithSubPhase41()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        var subPhase = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "4.1",
            "4",
            "Backend Services",
            "Owns backend service implementation, provider connectors, integration code, and application service changes.",
            ["backend-services"],
            ["IMPLEMENT"],
            ["teaching-registry-persistence-test"]));
        Assert.True(subPhase.IsSuccess, subPhase.ErrorMessage);
        return topology;
    }

    private static TeachingRegistrySnapshot BuildValidTrustStateSnapshot(
        LoadableTrustStateArtifacts artifacts)
    {
        const string unitId = "trust-boundary-generated-unit";
        const string unitRole = DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole;
        const string domainTag = "PLAN";
        const string version = "1.0.0";
        var createdAtUtc = DateTimeOffset.UnixEpoch;

        var snapshot = new TeachingRegistrySnapshot(
            SnapshotId: "trust-boundary-snapshot",
            BundleId: "trust-boundary-bundle",
            RowIndex: 1,
            CreatedAtUtc: createdAtUtc,
            Units:
            [
                new TeachingUnitRegistryEntry(
                    UnitId: unitId,
                    UnitRole: unitRole,
                    DomainTag: domainTag,
                    Version: version,
                    TrustState: nameof(LogicalUnitTrustState.Probationary),
                    IsLearned: false,
                    InputTypeName: typeof(string).AssemblyQualifiedName!,
                    OutputTypeName: typeof(string).AssemblyQualifiedName!,
                    RegistrationOrder: 2,
                    UnitKind: TeachingRegistrySnapshotKinds.GeneratedCapabilityUnit)
            ],
            Flows:
            [
                new TeachingFlowRegistryEntry(
                    FlowId: "generated-capability-flow-trust-boundary",
                    ScenarioId: "trust-boundary-scenario",
                    Version: version,
                    InputTypeName: typeof(string).AssemblyQualifiedName!,
                    PhaseId: "4",
                    SubPhaseId: "4.1",
                    TaskType: "backend-services",
                    DomainTags: ["software-development"],
                    RegistrationOrder: 3,
                    FlowKind: TeachingRegistrySnapshotKinds.GeneratedCapabilityFlow)
            ],
            GeneratedCapabilities:
            [
                new GeneratedCapabilityCheckpointManifest(
                    UnitId: unitId,
                    UnitRole: unitRole,
                    DomainTag: domainTag,
                    Version: version,
                    TrustState: nameof(LogicalUnitTrustState.Probationary),
                    FlowId: "generated-capability-flow-trust-boundary",
                    ScenarioId: "trust-boundary-scenario",
                    PhaseId: "4",
                    SubPhaseId: "4.1",
                    TaskType: "backend-services",
                    TrainingText: "Implement the backend service.",
                    OutputText: "PASS",
                    ExampleId: "trust-boundary-example",
                    PredicateIds: ["IMPLEMENT"],
                    DomainTags: ["software-development"],
                    PolicyCheckpointPath: artifacts.PolicyCheckpointPath,
                    TokenizerPath: artifacts.TokenizerPath,
                    VocabSize: artifacts.ModelConfig.VocabSize,
                    ContextLen: artifacts.ModelConfig.ContextLen,
                    HiddenDim: artifacts.ModelConfig.HiddenDim,
                    NumLayers: artifacts.ModelConfig.NumLayers,
                    NumHeads: artifacts.ModelConfig.NumHeads,
                    NumKvHeads: artifacts.ModelConfig.NumKvHeads,
                    FfnHiddenDim: artifacts.ModelConfig.FfnHiddenDim,
                    LoraRank: artifacts.LoraConfig.Rank,
                    LoraAlpha: artifacts.LoraConfig.Alpha,
                    LoraDropout: artifacts.LoraConfig.Dropout,
                    LoraTargetModules: artifacts.LoraConfig.TargetModules,
                    CreatedAtUtc: createdAtUtc)
            ],
            CheckpointPaths: []);
        var manifest = WithReplayAttestation(
            WithExplicitArtifactProvenance(
                Assert.Single(snapshot.GeneratedCapabilities) with
                {
                    SchemaVersion =
                        GeneratedCapabilityCheckpointManifest
                            .CurrentSchemaVersion,
                    UnitRegistrationOrder = 2,
                    FlowRegistrationOrder = 3
                },
                artifacts));
        manifest = WithExecutableManifestAttestation(manifest);
        return snapshot with
        {
            GeneratedCapabilities = [manifest]
        };
    }

    private static async Task<(
        LogicalFlowResult Before,
        LogicalFlowResult After,
        GeneratedCapabilityCheckpointManifest PersistedManifest)> RoundTripGeneratedFlowAsync(
        LoadableTrustStateArtifacts artifacts,
        TeachingRegistrySnapshot baseline,
        GeneratedCapabilityCheckpointManifest manifest,
        string runtimeInput,
        bool includeRuntimeTopology)
    {
        manifest = WithExecutableManifestAttestation(
            WithReplayAttestation(manifest));
        var topology = BuildTopologyWithSubPhase41();
        var beforeFlow = new GeneratedCapabilityFlow(
            manifest.PhaseId,
            manifest.SubPhaseId,
            manifest.TaskType,
            manifest.DomainTags,
            manifest.PredicateIds,
            manifest.DomainTag,
            new StaticStringUnit(manifest.UnitId, manifest.OutputText),
            includeRuntimeTopology ? topology : null,
            manifest.OriginalPrompt);
        var before = await beforeFlow.ExecuteAsync(BuildGeneratedFlowRequest(manifest, runtimeInput));
        Assert.True(before.IsSuccess, before.ErrorMessage);

        var manifestSave = TeachingRegistryPersistence.SaveGeneratedCapabilityManifest(
            manifest,
            Path.Combine(artifacts.Root, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName));
        Assert.True(manifestSave.IsSuccess, manifestSave.ErrorMessage);
        var inventory = TeachingRegistryPersistence.ReadGeneratedCapabilityManifestInventory(artifacts.Root);
        Assert.True(inventory.IsSuccess, inventory.ErrorMessage);
        var persistedManifest = Assert.Single(inventory.Value!.Sources).Manifest;

        var snapshot = baseline with
        {
            Flows = [BuildFlowEntry(persistedManifest.FlowId, persistedManifest, 3)],
            GeneratedCapabilities = [persistedManifest]
        };
        var snapshotPath = Path.Combine(artifacts.Root, "generated-capability-round-trip.json");
        var snapshotSave = TeachingRegistryPersistence.Save(snapshot, snapshotPath);
        Assert.True(snapshotSave.IsSuccess, snapshotSave.ErrorMessage);
        var snapshotLoad = TeachingRegistryPersistence.Load(snapshotPath);
        Assert.True(snapshotLoad.IsSuccess, snapshotLoad.ErrorMessage);
        Assert.NotNull(snapshotLoad.Value);

        var after = await LoadAndExecuteGeneratedFlowAsync(
            snapshotLoad.Value,
            topology,
            runtimeInput,
            includeRuntimeTopology);
        Assert.True(after.IsSuccess, after.ErrorMessage);
        return (before.Value!, after.Value!, persistedManifest);
    }

    private static async Task<OperationResult<LogicalFlowResult>> LoadAndExecuteGeneratedFlowAsync(
        TeachingRegistrySnapshot snapshot,
        ILogicalTopologyRegistry topology,
        string runtimeInput,
        bool includeRuntimeTopology)
    {
        var units = new InMemoryLogicalUnitRegistry();
        var flows = new InMemoryLogicalFlowRegistry();
        var load = LoadExecutableIntoRegistries(
            snapshot,
            topology,
            units,
            flows);
        Assert.True(load.IsSuccess, load.ErrorMessage);
        var manifest = Assert.Single(snapshot.GeneratedCapabilities);
        var binding = flows.ResolveBindingBySubPhase(
            manifest.SubPhaseId,
            new LogicalFlowFactoryRequest(
                "restored-generated-capability-run",
                new LogicalExecutionFlags(),
                units,
                new Dictionary<string, object>(),
                includeRuntimeTopology ? topology : null));
        Assert.True(binding.IsSuccess, binding.ErrorMessage);
        return await binding.Value!.Flow.ExecuteAsync(
            BuildGeneratedFlowRequest(manifest, runtimeInput));
    }

    private static LogicalFlowRequest<string> BuildGeneratedFlowRequest(
        GeneratedCapabilityCheckpointManifest manifest,
        string input)
    {
        return new LogicalFlowRequest<string>(
            "restored-generated-capability-run",
            manifest.FlowId,
            manifest.ScenarioId,
            input,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());
    }

    private static LoadableTrustStateArtifacts CreateLoadableTrustStateArtifacts()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"teaching-registry-trust-boundary-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            var tokenizer = new BpeTokenizer();
            tokenizer.Train(
                "Implement the backend service and provider connector. PASS",
                vocabSize: 260);
            var tokenizerPath = Path.Combine(root, "tokenizer.json");
            tokenizer.Save(tokenizerPath);

            var modelConfig = new ModelConfig
            {
                VocabSize = tokenizer.VocabSize,
                ContextLen = 32,
                HiddenDim = 8,
                NumLayers = 1,
                NumHeads = 1,
                NumKvHeads = 1,
                FfnHiddenDim = 16,
                RopeTheta = 12_345f,
                RmsNormEps = 2.5e-5f
            };
            var loraConfig = new LoraConfig
            {
                Rank = 2,
                Alpha = 4.0f,
                Dropout = 0.0f,
                TargetModules = ["q_proj", "v_proj"]
            };
            var policyModel = new LlmModel(modelConfig);
            LoraApplicator.Apply(policyModel, loraConfig);
            var policyCheckpointPath = Path.Combine(root, "policy.bin");
            policyModel.SaveCheckpoint(policyCheckpointPath);

            Assert.True(File.Exists(tokenizerPath));
            Assert.True(File.Exists(policyCheckpointPath));
            return new LoadableTrustStateArtifacts(
                root,
                policyCheckpointPath,
                tokenizerPath,
                modelConfig,
                loraConfig);
        }
        catch
        {
            Directory.Delete(root, recursive: true);
            throw;
        }
    }

    private static GeneratedCapabilityCheckpointManifest WithExplicitArtifactProvenance(
        GeneratedCapabilityCheckpointManifest manifest,
        LoadableTrustStateArtifacts artifacts)
    {
        var tokenizerSha256 = BpeTokenizer.ComputeArtifactSha256(artifacts.TokenizerPath);
        var policyCheckpointSha256 = ComputeFileSha256(artifacts.PolicyCheckpointPath);
        var runtimeProvenance = RuntimeProvenance.ComputeSha256(
            artifacts.ModelConfig,
            artifacts.LoraConfig,
            tokenizerSha256,
            policyCheckpointSha256);
        Assert.True(runtimeProvenance.IsSuccess, runtimeProvenance.ErrorMessage);
        return manifest with
        {
            RopeTheta = artifacts.ModelConfig.RopeTheta,
            RmsNormEps = artifacts.ModelConfig.RmsNormEps,
            TokenizerSha256 = tokenizerSha256,
            PolicyCheckpointSha256 = policyCheckpointSha256,
            RuntimeProvenanceSha256 = runtimeProvenance.Value!
        };
    }

    private static string ComputeFileSha256(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static LlmModel ResolveRestoredModel(
        InMemoryLogicalUnitRegistry units,
        string unitId)
    {
        var resolved = units.ResolveLatestById<string, string>(unitId);
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        var retrievalUnit = Assert.IsType<TransformerEmbeddingKnnLogicalUnit<string>>(resolved.Value!.Unit);
        var embeddingField = typeof(TransformerEmbeddingKnnLogicalUnit<string>).GetField(
            "_embeddingModel",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(embeddingField);
        var embeddingModel = embeddingField.GetValue(retrievalUnit);
        Assert.NotNull(embeddingModel);
        var encoderField = embeddingModel.GetType().GetField(
            "_encoder",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(encoderField);
        return Assert.IsType<LlmModel>(encoderField.GetValue(embeddingModel));
    }

    private static CheckpointState SnapshotCheckpointState(LlmModel model)
    {
        var entries = model.GetCheckpointParameterEntries();
        return new CheckpointState(
            entries.Select(entry => entry.Key).ToArray(),
            entries.Select(entry => entry.Parameter.Value.Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray())
                .ToArray());
    }

    private static bool CheckpointStatesEqual(CheckpointState left, CheckpointState right)
        => left.Keys.SequenceEqual(right.Keys, StringComparer.Ordinal)
           && left.ParameterBits.Length == right.ParameterBits.Length
           && left.ParameterBits.Zip(right.ParameterBits)
               .All(pair => pair.First.SequenceEqual(pair.Second));

    private static void AssertCheckpointStateEqual(CheckpointState expected, LlmModel actual)
    {
        var actualState = SnapshotCheckpointState(actual);
        Assert.True(
            CheckpointStatesEqual(expected, actualState),
            "Restored checkpoint state did not match the verified held artifact.");
    }

    private static void FillParametersDeterministically(
        IEnumerable<Parameter> parameters,
        float seed)
    {
        var parameterIndex = 0;
        foreach (var parameter in parameters)
        {
            for (var valueIndex = 0; valueIndex < parameter.Value.Data.Length; valueIndex++)
                parameter.Value.Data[valueIndex] = seed + (parameterIndex * 0.001f) + (valueIndex * 0.00001f);
            parameterIndex++;
        }
    }

    private static TransformerEmbeddingKnnLogicalUnit<string> BuildGeneratedRetrievalUnit(
        LoadableTrustStateArtifacts artifacts,
        GeneratedCapabilityCheckpointManifest manifest)
    {
        var tokenizer = BpeTokenizer.Load(artifacts.TokenizerPath);
        var model = new LlmModel(artifacts.ModelConfig);
        LoraApplicator.Apply(model, artifacts.LoraConfig);
        model.LoadCheckpoint(artifacts.PolicyCheckpointPath);

        var trained = TransformerEmbeddingKnnLogicalUnit<string>.Train(
            manifest.UnitId,
            manifest.UnitRole,
            manifest.DomainTag,
            model,
            tokenizer,
            [
                new TransformerKnnTrainingExample<string>(
                    manifest.ExampleId,
                    manifest.TrainingText,
                    manifest.OutputText,
                    manifest.PredicateIds,
                    manifest.DomainTags)
            ]);
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        return trained.Value!;
    }

    private static (
        InMemoryLogicalUnitRegistry Units,
        InMemoryLogicalFlowRegistry Flows,
        LogicalUnitDescriptor[] UnitDescriptors,
        LogicalFlowDescriptor[] FlowDescriptors) BuildRegistriesWithSentinels()
    {
        var units = new InMemoryLogicalUnitRegistry();
        var unitRegistration = units.Register<string, string>(
            "trust-boundary-sentinel-unit",
            "sentinel-role",
            "sentinel-domain",
            "1.0.0",
            LogicalUnitTrustState.Trusted,
            false,
            new StaticStringUnit("trust-boundary-sentinel-unit", "sentinel output"));
        Assert.True(unitRegistration.IsSuccess, unitRegistration.ErrorMessage);

        var flows = new InMemoryLogicalFlowRegistry();
        var flowRegistration = flows.Register(
            "trust-boundary-sentinel-flow",
            "trust-boundary-sentinel-scenario",
            "1.0.0",
            new UniversalNineNodeDevelopmentFlow(
                "4",
                "4.1",
                "backend-services",
                ["software-development"],
                ["IMPLEMENT"]));
        Assert.True(flowRegistration.IsSuccess, flowRegistration.ErrorMessage);

        return (
            units,
            flows,
            units.ListDescriptors().ToArray(),
            flows.ListDescriptors().ToArray());
    }

    private static void AssertFailedLoadWithoutRegistryMutation(
        OperationResult<int> result,
        string expectedErrorCode,
        string expectedErrorMessage,
        (
            InMemoryLogicalUnitRegistry Units,
            InMemoryLogicalFlowRegistry Flows,
            LogicalUnitDescriptor[] UnitDescriptors,
            LogicalFlowDescriptor[] FlowDescriptors) registries)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(default, result.Value);
        Assert.Equal(expectedErrorCode, result.ErrorCode);
        Assert.Equal(expectedErrorMessage, result.ErrorMessage);
        Assert.Equal(registries.UnitDescriptors, registries.Units.ListDescriptors());
        Assert.Equal(registries.FlowDescriptors, registries.Flows.ListDescriptors());
    }

    private static byte[] CaptureRegistryDescriptorBytes(
        InMemoryLogicalUnitRegistry units,
        InMemoryLogicalFlowRegistry flows)
    {
        return JsonSerializer.SerializeToUtf8Bytes(new
        {
            Units = units.ListDescriptors().Select(descriptor => new
            {
                descriptor.UnitId,
                descriptor.UnitRole,
                descriptor.DomainTag,
                descriptor.Version,
                TrustState = descriptor.TrustState.ToString(),
                descriptor.IsLearned,
                InputType = descriptor.InputType.AssemblyQualifiedName,
                OutputType = descriptor.OutputType.AssemblyQualifiedName,
                descriptor.RegistrationOrder
            }).ToArray(),
            Flows = flows.ListDescriptors().Select(descriptor => new
            {
                descriptor.FlowId,
                descriptor.ScenarioId,
                descriptor.Version,
                InputType = descriptor.InputType.AssemblyQualifiedName,
                descriptor.PhaseId,
                descriptor.SubPhaseId,
                descriptor.TaskType,
                DomainTags = descriptor.DomainTags.ToArray(),
                descriptor.RegistrationOrder
            }).ToArray()
        });
    }

    private static DirectoryLinkMechanism CreateDirectoryLinkOrFail(
        string linkPath,
        string targetPath)
    {
        try
        {
            Directory.CreateSymbolicLink(linkPath, targetPath);
            return DirectoryLinkMechanism.SymbolicLink;
        }
        catch (Exception exception)
            when (exception is IOException or NotSupportedException or
                PlatformNotSupportedException or UnauthorizedAccessException)
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new InvalidOperationException(
                    $"Environment could not exercise the required directory-link safety branch: " +
                    $"{exception.GetType().Name}: {exception.Message}",
                    exception);
            }

            CreateWindowsJunctionOrFail(linkPath, targetPath, exception);
            return DirectoryLinkMechanism.WindowsJunction;
        }
    }

    private static void CreateWindowsJunctionOrFail(
        string linkPath,
        string targetPath,
        Exception symbolicLinkFailure)
    {
        const uint genericWrite = 0x40000000;
        const uint openExisting = 3;
        const uint backupSemantics = 0x02000000;
        const uint openReparsePoint = 0x00200000;
        const uint setReparsePoint = 0x000900A4;
        const uint mountPointTag = 0xA0000003;

        var directoryAlreadyExisted = Directory.Exists(linkPath);
        Directory.CreateDirectory(linkPath);
        using var handle = CreateFileForJunction(
            linkPath,
            genericWrite,
            (uint)(FileShare.Read | FileShare.Write | FileShare.Delete),
            IntPtr.Zero,
            openExisting,
            backupSemantics | openReparsePoint,
            IntPtr.Zero);
        if (handle.IsInvalid)
        {
            var error = Marshal.GetLastPInvokeError();
            if (!directoryAlreadyExisted)
                Directory.Delete(linkPath);
            throw new InvalidOperationException(
                $"Environment denied both symbolic-link and junction creation (CreateFile Win32={error}).",
                symbolicLinkFailure);
        }

        var printName = Path.TrimEndingDirectorySeparator(Path.GetFullPath(targetPath));
        var substituteName = @"\??\" + printName;
        var substituteBytes = Encoding.Unicode.GetBytes(substituteName);
        var printBytes = Encoding.Unicode.GetBytes(printName);
        var pathBytes = substituteBytes.Length + sizeof(ushort) + printBytes.Length + sizeof(ushort);
        var reparseDataLength = checked((ushort)(8 + pathBytes));
        var buffer = new byte[8 + reparseDataLength];
        using (var memory = new MemoryStream(buffer, writable: true))
        using (var writer = new BinaryWriter(memory, Encoding.Unicode, leaveOpen: true))
        {
            writer.Write(mountPointTag);
            writer.Write(reparseDataLength);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write(checked((ushort)substituteBytes.Length));
            writer.Write(checked((ushort)(substituteBytes.Length + sizeof(ushort))));
            writer.Write(checked((ushort)printBytes.Length));
            writer.Write(substituteBytes);
            writer.Write((ushort)0);
            writer.Write(printBytes);
            writer.Write((ushort)0);
        }

        if (!DeviceIoControlForJunction(
                handle,
                setReparsePoint,
                buffer,
                buffer.Length,
                IntPtr.Zero,
                0,
                out _,
                IntPtr.Zero))
        {
            var error = Marshal.GetLastPInvokeError();
            handle.Dispose();
            if (!directoryAlreadyExisted)
                Directory.Delete(linkPath);
            throw new InvalidOperationException(
                $"Environment denied both symbolic-link and junction creation " +
                $"(FSCTL_SET_REPARSE_POINT Win32={error}).",
                symbolicLinkFailure);
        }
    }

    private static void RemoveDirectoryLinkIfPresent(
        string linkPath,
        DirectoryLinkMechanism mechanism)
    {
        if (mechanism == DirectoryLinkMechanism.None)
            return;

        if (mechanism == DirectoryLinkMechanism.WindowsJunction)
        {
            if (!RemoveDirectoryForJunction(linkPath))
            {
                var error = Marshal.GetLastPInvokeError();
                if (error != 3)
                {
                    throw new InvalidOperationException(
                        $"Windows junction cleanup failed (RemoveDirectory Win32={error}).");
                }
            }

            return;
        }

        try
        {
            Directory.Delete(linkPath);
        }
        catch (DirectoryNotFoundException)
        {
        }
    }

    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFileForJunction(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeviceIoControlForJunction(
        SafeFileHandle device,
        uint controlCode,
        byte[] inputBuffer,
        int inputBufferSize,
        IntPtr outputBuffer,
        int outputBufferSize,
        out int bytesReturned,
        IntPtr overlapped);

    [DllImport("kernel32.dll", EntryPoint = "RemoveDirectoryW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveDirectoryForJunction(string pathName);

    private enum DirectoryLinkMechanism
    {
        None,
        SymbolicLink,
        WindowsJunction
    }

    private static LogicalOrchestratorRequest BuildRequest(string runId)
    {
        return new LogicalOrchestratorRequest(
            runId,
            AdaptiveLogicalOrchestrator.ScenarioId,
            "Implement the backend service and provider connector for the integration layer.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());
    }

    private static LogicalFlowResult BuildKnowledgeEdgeBranch(
        string subPhaseId,
        LogicalSubPhase? attestedSubPhase = null)
    {
        var branch = new LogicalFlowResult(
            AdaptiveLogicalOrchestrator.KnowledgeEdgeFlowId,
            $"generated-capability-{subPhaseId}",
            "KNOWLEDGE_EDGE",
            $"Missing capability for {subPhaseId}.",
            new LogicalFlowSummary(
                "knowledge-edge",
                1,
                0,
                0,
                "knowledge_edge",
                false,
                false,
                false,
                "TRAIN_CAPABILITY",
                1,
                subPhaseId,
                "Missing capability.",
                0,
                0),
            new Dictionary<string, string>
            {
                ["missing_sub_phase"] = subPhaseId,
                ["knowledge_edge"] = true.ToString()
            },
            [],
            []);
        var subPhase = attestedSubPhase ?? new LogicalSubPhase(
            "4.1",
            "4",
            "Backend Services",
            "Owns backend service implementation, provider connectors, integration code, and application service changes.",
            ["backend-services"],
            ["IMPLEMENT"],
            ["teaching-registry-persistence-test"]);
        return branch with
        {
            TrainingContentProvenance = LogicalCapabilityTrainingAuthorityTestFactory
                .IssueSingle(branch,
                    new LogicalTrainingContentDeterministicInputs(
                        "Implement the backend service and provider connector for the integration layer.",
                        Domain.PLAN.ToString(), subPhase.PhaseId,
                        subPhase.SubPhaseId, subPhase.Name, subPhase.Purpose,
                        subPhase.PredicateIds, subPhase.DomainIds))
        };
    }

    private static GeneratedCapabilityCheckpointManifest
        WithReplayAttestation(
            GeneratedCapabilityCheckpointManifest manifest)
    {
        var knnCorpus = GeneratedCapabilityKnnCorpusProjector.Project(
            [new GeneratedCapabilityKnnExample(
                manifest.ExampleId,
                manifest.TrainingText,
                manifest.OutputText,
                manifest.PredicateIds,
                manifest.DomainTags)]);
        Assert.True(knnCorpus.IsSuccess, knnCorpus.ErrorMessage);
        manifest = manifest with
        {
            KnnExamples = knnCorpus.Value!.Examples,
            KnnCorpusSha256 = knnCorpus.Value.Sha256
        };

        var payload = LogicalCapabilityTrainingPreferenceProjector
            .ProjectReplayPayload(
                [new string('a', 64)],
                new string('b', 64),
                manifest.DomainTag,
                manifest.SubPhaseId,
                manifest.ExampleId,
                manifest.TrainingText,
                manifest.OutputText,
                manifest.PredicateIds,
                manifest.DomainTags);
        Assert.True(payload.IsSuccess, payload.ErrorMessage);
        var descriptor = payload.Value!.Descriptor;
        return manifest with
        {
            SchemaVersion =
                GeneratedCapabilityCheckpointManifest.CurrentSchemaVersion,
            ReplayAttestation = new(
                descriptor.SchemaVersion,
                descriptor.OriginalSourceKind,
                descriptor.ReplaySealGroupSha256,
                descriptor.SourceBindings.Snapshot(),
                descriptor.ReplayPayloadSha256,
                descriptor.DpoPreferenceSha256,
                $"sha256:{new string('c', 64)}",
                new string('d', 64))
        };
    }

    private static GeneratedCapabilityCheckpointManifest
        WithServerOwnedHmac(
            GeneratedCapabilityCheckpointManifest manifest,
            byte[] hmacKey)
    {
        var replayPayload = LogicalCapabilityTrainingPreferenceProjector
            .ProjectReplayPayload(
                [new string('a', 64)],
                new string('b', 64),
                manifest.DomainTag,
                manifest.SubPhaseId,
                manifest.ExampleId,
                manifest.TrainingText,
                manifest.OutputText,
                manifest.PredicateIds,
                manifest.DomainTags);
        Assert.True(replayPayload.IsSuccess, replayPayload.ErrorMessage);
        var descriptor = replayPayload.Value!.Descriptor;
        var authorityKeyRef =
            LogicalCapabilityReplayHmacCodec.BuildAuthorityKeyRef(hmacKey);
        var replay = new LogicalCapabilityTrainingReplayAttestation(
            descriptor.SchemaVersion,
            descriptor.OriginalSourceKind,
            descriptor.ReplaySealGroupSha256,
            descriptor.SourceBindings.Snapshot(),
            descriptor.ReplayPayloadSha256,
            descriptor.DpoPreferenceSha256,
            authorityKeyRef,
            LogicalCapabilityReplayHmacCodec.ComputeReplayMac(
                hmacKey,
                descriptor));
        manifest = manifest with
        {
            ReplayAttestation = replay,
            ExecutableManifestAttestation = null
        };
        var executableDigest =
            GeneratedCapabilityExecutableManifestProjector.ProjectSha256(
                manifest,
                manifest.TokenizerSha256,
                manifest.PolicyCheckpointSha256);
        Assert.True(executableDigest.IsSuccess, executableDigest.ErrorMessage);
        return manifest with
        {
            ExecutableManifestAttestation = new(
                LogicalCapabilityExecutableManifestAttestation
                    .CurrentSchemaVersion,
                executableDigest.Value!,
                replay.AuthorityKeyRef,
                replay.MacSha256,
                replay.AuthorityKeyRef,
                LogicalCapabilityReplayHmacCodec
                    .ComputeExecutableManifestMac(
                        hmacKey,
                        executableDigest.Value!,
                        replay))
        };
    }

    private static GeneratedCapabilityCheckpointManifest
        WithExecutableManifestAttestation(
            GeneratedCapabilityCheckpointManifest manifest)
    {
        var executableManifestSha256 =
            GeneratedCapabilityExecutableManifestProjector.ProjectSha256(
                manifest,
                manifest.TokenizerSha256,
                manifest.PolicyCheckpointSha256);
        Assert.True(
            executableManifestSha256.IsSuccess,
            executableManifestSha256.ErrorMessage);
        var replayAttestation = Assert.IsType<
            LogicalCapabilityTrainingReplayAttestation>(
            manifest.ReplayAttestation);
        return manifest with
        {
            ExecutableManifestAttestation = new(
                LogicalCapabilityExecutableManifestAttestation
                    .CurrentSchemaVersion,
                executableManifestSha256.Value!,
                replayAttestation.AuthorityKeyRef,
                replayAttestation.MacSha256,
                $"sha256:{new string('e', 64)}",
                new string('f', 64))
        };
    }

    private static LogicalCapabilityReplayVerificationCapability
        IssueReplayVerificationCapability()
    {
        var result = LogicalCapabilityReplayVerificationCapability.Issue(
            new RecordingLogicalCapabilityTrainingAdmissionAuthority());
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    private static OperationResult<int> LoadExecutableIntoRegistries(
        TeachingRegistrySnapshot snapshot,
        ILogicalTopologyRegistry topology,
        InMemoryLogicalUnitRegistry units,
        InMemoryLogicalFlowRegistry flows)
        => TeachingRegistryPersistence.LoadIntoRegistries(
            snapshot,
            topology,
            units,
            flows,
            IssueReplayVerificationCapability());

    private sealed class StaticStringUnit : ILogicalUnit<string, string>
    {
        private readonly string _unitId;
        private readonly string _output;

        public StaticStringUnit(string unitId, string output)
        {
            _unitId = unitId;
            _output = output;
        }

        public Task<OperationResult<LogicalUnitResult<string>>> ExecuteAsync(
            LogicalUnitRequest<string> request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(OperationResult<LogicalUnitResult<string>>.Ok(new LogicalUnitResult<string>(
                _unitId,
                request.UnitRole,
                request.DomainTag,
                _output,
                0.95,
                ["IMPLEMENT"],
                [request.DomainTag],
                [_unitId],
                [])));
        }
    }

    private sealed record LoadableTrustStateArtifacts(
        string Root,
        string PolicyCheckpointPath,
        string TokenizerPath,
        ModelConfig ModelConfig,
        LoraConfig LoraConfig) : IDisposable
    {
        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }

    private sealed record CheckpointState(string[] Keys, int[][] ParameterBits);

    private sealed class PersistenceCheckpointSwapObserver : ITeachingRegistryArtifactLoadObserver
    {
        private readonly string _targetPath;
        private readonly string _replacementPath;

        internal PersistenceCheckpointSwapObserver(string targetPath, string replacementPath)
        {
            _targetPath = targetPath;
            _replacementPath = replacementPath;
        }

        internal bool WasInvoked { get; private set; }
        internal bool ReplacementSucceeded { get; private set; }

        public void OnArtifactsVerified()
        {
            WasInvoked = true;
            try
            {
                File.Move(_replacementPath, _targetPath, overwrite: true);
                ReplacementSucceeded = true;
            }
            catch (Exception exception)
                when (exception is IOException or UnauthorizedAccessException)
            {
                ReplacementSucceeded = false;
            }
        }
    }

    private sealed class TeachingNullSessionStorage : ISessionStorage
    {
        public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
        {
            return Task.FromResult(OperationResult<bool>.Ok(true));
        }

        public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        {
            return Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NULL_STORAGE"));
        }

        public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        {
            return Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(Array.Empty<string>()));
        }
    }

    // ================================================================================
    // Проводка журнала состоявшихся выдач в ШТАТНОЕ восстановление
    // (узкий перенос Q12 карточки 74, раздел «Подключить журнал выдач в штатной публикации и
    // восстановлении»).
    //
    // Что здесь проверяется по-человечески. Берётся готовое сохранённое состояние и загружается
    // ОБЫЧНОЙ загрузкой в реестры, и этой загрузке передаётся журнал выдач. В реестре живут два
    // получателя одной роли и одного домена: первый, восстановленный из сохранённого состояния,
    // отвечает «review»; второй, опубликованный обычной регистрацией после загрузки, отвечает
    // «archive». Поток берётся ШТАТНЫМ получением из реестра потоков, а не прямым вызовом
    // фабрики. Один запрос — отвечает второй получатель. Затем по ЭТОЙ выдаче приходит
    // подтверждённое исправление, и урок обязан дойти ровно до второго получателя, не задев
    // первого.
    //
    // Чем эти проверки НЕ являются, названо прямо:
    //   * это НЕ допуск доменных функций ранжирования: ни один каталог правил не создаётся;
    //   * это НЕ измерение пользы обучения: ни одна точность не улучшается и не заявляется;
    //   * это НЕ доказательство побайтовой сохранности состояния: опознание состояния только
    //     составляется из реестра и сверяется;
    //   * писатель обучения — состав проверки принятого круга, он обучает РОДНЫМ обучением блока
    //     ближайших соседей и держит набор уроков у себя, потому что у этого блока нет родного
    //     сохранения и загрузки.
    // ================================================================================

    // Запрос пользователя. Одинаковый текст запроса у двух получателей — обычное дело, и адресом
    // он быть не может.
    private const string AnsweredDeliveryWiringQuery = "approve report";

    // Опознание запуска. Оно совпадает с опознанием, которое ставит штатный сборщик запроса
    // потока, потому что журнал ищет выдачу по паре «запуск и опознание выдачи».
    private const string AnsweredDeliveryWiringRunId = "restored-generated-capability-run";

    // Опознание второго получателя — того, кто фактически отвечает пользователю.
    private const string AnsweredDeliveryWiringSecondOwnerId = "q12-wiring-second-owner";

    // Опознание урока второго получателя: его же исправляет пользователь.
    private const string AnsweredDeliveryWiringSecondOwnerExampleId = "q12-wiring-second-owner-row-1";

    [Fact]
    public async Task LoadIntoRegistries_DeliveryLedgerLetsOrdinaryAnswerTrainTheSelectedOwner()
    {
        // Сохранённое состояние готовится обычными составами этой проверки.
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var baseline = BuildValidTrustStateSnapshot(artifacts);

        // Первый получатель приходит из сохранённого состояния и на запрос отвечает «review».
        var restoredManifest = WithExecutableManifestAttestation(
            WithReplayAttestation(
                Assert.Single(baseline.GeneratedCapabilities) with
                {
                    OutputText = "review"
                }));

        // Снимок состояния описывает ровно одного восстановленного получателя и его поток.
        var snapshot = baseline with
        {
            Flows = [BuildFlowEntry(restoredManifest.FlowId, restoredManifest, 3)],
            GeneratedCapabilities = [restoredManifest]
        };

        // Продуктовые реестры и устройство подфаз: в них и живёт восстановленная способность.
        var units = new InMemoryLogicalUnitRegistry();
        var flows = new InMemoryLogicalFlowRegistry();
        var topology = BuildTopologyWithSubPhase41();

        // Справочник владельцев — продуктовый переходник к тому же реестру единиц.
        var directory = new LogicalUnitRegistryAddressedCorrectionDirectory<string>(units);

        // Набор уроков второго получателя до исправления: один урок с ответом «archive».
        var lessons = new Dictionary<string, List<LogicalKnnTrainingExample<string, string>>>(StringComparer.Ordinal)
        {
            [AnsweredDeliveryWiringSecondOwnerId] =
            [
                new LogicalKnnTrainingExample<string, string>(
                    AnsweredDeliveryWiringSecondOwnerExampleId,
                    AnsweredDeliveryWiringQuery,
                    "archive",
                    [],
                    [restoredManifest.DomainTag],
                    [$"evidence:{AnsweredDeliveryWiringSecondOwnerExampleId}"])
            ]
        };

        // Писатель обучения и публикации принятого круга переиспользуется как есть.
        var writer = new ExperimentFixes.Q12NativeTrainingWriter(
            units,
            lessons,
            AnsweredDeliveryWiringQuery);

        // Каталог долговечных отметок применения. Свой у этой проверки, чтобы не мешать другим.
        var journalRoot = Path.Combine(
            Path.GetTempPath(),
            "llm-q12-answered-delivery-wiring",
            $"restore-{Guid.NewGuid():N}");

        // Существующий координатор адресного исправления. Он НЕ переписывается этой проверкой.
        var coordinator = new KnnAddressedCorrectionCoordinator<string>(directory, writer, journalRoot);

        // Журнал выдач — ровно тот путь, который проводится в штатное восстановление.
        var ledger = new KnnAnsweredDeliveryLedger<string>(directory, coordinator);

        try
        {
            // ШТАТНОЕ ВОССТАНОВЛЕНИЕ: журнал уходит в обычную загрузку необязательным аргументом.
            var load = TeachingRegistryPersistence.LoadIntoRegistries(
                snapshot,
                topology,
                units,
                flows,
                IssueReplayVerificationCapability(),
                ledger);
            Assert.True(load.IsSuccess, load.ErrorMessage);

            // Второй получатель той же роли и того же домена публикуется обычной регистрацией
            // ПОСЛЕ загрузки, поэтому обычный путь выдачи выберет именно его.
            var secondOwner = KnnLogicalUnit<string, string>.Train(
                AnsweredDeliveryWiringSecondOwnerId,
                restoredManifest.UnitRole,
                restoredManifest.DomainTag,
                lessons[AnsweredDeliveryWiringSecondOwnerId],
                text => text);
            Assert.True(secondOwner.IsSuccess, secondOwner.ErrorMessage);
            Assert.True(units.Register<string, string>(
                AnsweredDeliveryWiringSecondOwnerId,
                restoredManifest.UnitRole,
                restoredManifest.DomainTag,
                "1.0.0",
                LogicalUnitTrustState.Trusted,
                isLearned: true,
                secondOwner.Value!).IsSuccess);

            // ШТАТНОЕ ПОЛУЧЕНИЕ ПОТОКА: поток приходит из реестра потоков по подфазе, а не
            // прямым вызовом фабрики.
            var binding = flows.ResolveBindingBySubPhase(
                restoredManifest.SubPhaseId,
                new LogicalFlowFactoryRequest(
                    AnsweredDeliveryWiringRunId,
                    new LogicalExecutionFlags(),
                    units,
                    new Dictionary<string, object>(StringComparer.Ordinal),
                    topology));
            Assert.True(binding.IsSuccess, binding.ErrorMessage);

            // Собранный поток обязан быть потоком обученной способности, а не запасным путём:
            // иначе ответ пришёл бы не от обученного получателя, и проверка ничего не значила бы.
            var flow = Assert.IsType<GeneratedCapabilityFlow>(binding.Value!.Flow);

            // ОДИН ЗАПРОС обычным путём выдачи: ответ даёт выбранный получатель.
            var served = await flow.ExecuteAsync(
                BuildGeneratedFlowRequest(restoredManifest, AnsweredDeliveryWiringQuery));
            Assert.True(served.IsSuccess, served.ErrorMessage);

            // Точка выдачи запомнила выдачу. Шаг первый: признак обязан ПРИСУТСТВОВАТЬ в
            // показателях прогона. Отдельный шаг нужен затем, чтобы случай «признака нет»
            // отличался от случая «признак есть, но равен False» сверкой значений, а не
            // брошенным исключением индексатора словаря: иначе прибор не различает две разные
            // причины.
            Assert.True(
                served.Value!.Metrics.TryGetValue("answered_delivery_recorded", out var recordedFlag),
                "В показателях прогона нет признака «выдача запомнена».");

            // Шаг второй: сверка самого значения признака.
            Assert.Equal(bool.TrueString, recordedFlag);

            // Записанный выбранный получатель — второй владелец: его ответ и увидел пользователь.
            // Те же два шага: сначала присутствие строки показателей, потом сверка значения.
            Assert.True(
                served.Value.Metrics.TryGetValue("answered_delivery_owner_id", out var recordedOwnerId),
                "В показателях прогона нет опознания выбранного получателя.");
            Assert.Equal(AnsweredDeliveryWiringSecondOwnerId, recordedOwnerId);

            // Тот же выбранный получатель назван и обычным показателем выдачи.
            Assert.Equal(
                AnsweredDeliveryWiringSecondOwnerId,
                served.Value.Metrics["retrieval_unit_id"]);

            // Ответы обоих получателей ДО исправления читаются обычным путём реестра.
            var secondBefore = await AnsweredDeliveryWiringAnswerOf(
                directory,
                AnsweredDeliveryWiringSecondOwnerId,
                restoredManifest.UnitRole,
                restoredManifest.DomainTag);
            var firstBefore = await AnsweredDeliveryWiringAnswerOf(
                directory,
                restoredManifest.UnitId,
                restoredManifest.UnitRole,
                restoredManifest.DomainTag);

            // Объявленные заранее ожидания до исправления.
            Assert.Equal("archive", secondBefore);
            Assert.Equal("review", firstBefore);

            // Подтверждённое исправление приходит ПО ЭТОЙ ВЫДАЧЕ и уходит в существующий
            // координатор: опознание запуска и опознание выдачи те же, что у прогона.
            var receipt = await ledger.ApplyConfirmedCorrectionAsync(
                AnsweredDeliveryWiringRunId,
                restoredManifest.FlowId,
                "fb-q12-wiring-restore",
                AnsweredDeliveryWiringSecondOwnerExampleId,
                "archive then notify");
            Assert.True(receipt.IsSuccess, receipt.ErrorMessage);

            // Квитанция называет выбранного получателя, а не соседа.
            Assert.Equal(AnsweredDeliveryWiringSecondOwnerId, receipt.Value!.UnitId);
            Assert.Equal(restoredManifest.DomainTag, receipt.Value.DomainTag);

            // Обучение случилось ровно один раз и получило ФАКТИЧЕСКИЙ объект выбранного владельца.
            Assert.Equal(1, writer.TrainCallCount);
            Assert.Same(secondOwner.Value, writer.SelectedUnitPassedToTraining);

            // Теперь снова обращаемся к ОБОИМ получателям обычным путём реестра.
            var secondAfter = await AnsweredDeliveryWiringAnswerOf(
                directory,
                AnsweredDeliveryWiringSecondOwnerId,
                restoredManifest.UnitRole,
                restoredManifest.DomainTag);
            var firstAfter = await AnsweredDeliveryWiringAnswerOf(
                directory,
                restoredManifest.UnitId,
                restoredManifest.UnitRole,
                restoredManifest.DomainTag);

            // Первое из двух: урок ФАКТИЧЕСКИ применён — исходное действие дополнено «notify».
            Assert.Equal("archive then notify", secondAfter);
            Assert.NotEqual(secondBefore, secondAfter);

            // Второе из двух: восстановленный СОСЕД не изменился ни на знак.
            Assert.Equal("review", firstAfter);
            Assert.Equal(firstBefore, firstAfter);

            // Наблюдения сохраняются уликой в дереве проекта.
            WriteAnsweredDeliveryWiringEvidence(
                "03-observed-run-values.md",
                [
                    "# Наблюдённые значения прогона штатного восстановления с журналом выдач",
                    string.Empty,
                    $"путь получения потока: реестр потоков по подфазе {restoredManifest.SubPhaseId}",
                    $"тип собранного потока: {flow.GetType().Name}",
                    $"выбранный получатель, записанный точкой выдачи: {recordedOwnerId}",
                    $"признак «выдача запомнена»: {recordedFlag}",
                    string.Empty,
                    $"второй получатель (ответивший), ответ ДО исправления: {secondBefore}",
                    $"второй получатель (ответивший), ответ ПОСЛЕ исправления: {secondAfter}",
                    $"первый получатель (восстановленный сосед), ответ ДО исправления: {firstBefore}",
                    $"первый получатель (восстановленный сосед), ответ ПОСЛЕ исправления: {firstAfter}",
                    string.Empty,
                    $"квитанция координатора, владелец: {receipt.Value.UnitId}",
                    $"квитанция координатора, опубликованная версия: {receipt.Value.PublishedVersion}",
                    $"обучений исправленного кандидата: {writer.TrainCallCount}",
                    string.Empty,
                    "Сосед не изменился: " + (string.Equals(firstBefore, firstAfter, StringComparison.Ordinal) ? "да" : "НЕТ"),
                    "Урок фактически применён: " + (!string.Equals(secondBefore, secondAfter, StringComparison.Ordinal) ? "да" : "НЕТ")
                ]);
        }
        finally
        {
            // Каталог отметок этой проверки убирается за собой.
            if (Directory.Exists(journalRoot))
                Directory.Delete(journalRoot, recursive: true);
        }
    }

    [Fact]
    public async Task LoadIntoRegistries_WithoutDeliveryLedgerKeepsPreviousBehaviourAndRefusesUnknownDelivery()
    {
        // Тот же снимок состояния, что и в успешном случае.
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var baseline = BuildValidTrustStateSnapshot(artifacts);
        var restoredManifest = WithExecutableManifestAttestation(
            WithReplayAttestation(
                Assert.Single(baseline.GeneratedCapabilities) with
                {
                    OutputText = "review"
                }));
        var snapshot = baseline with
        {
            Flows = [BuildFlowEntry(restoredManifest.FlowId, restoredManifest, 3)],
            GeneratedCapabilities = [restoredManifest]
        };

        var units = new InMemoryLogicalUnitRegistry();
        var flows = new InMemoryLogicalFlowRegistry();
        var topology = BuildTopologyWithSubPhase41();

        // ПРЕЖНИЙ ПУТЬ: журнал выдач штатной загрузке НЕ передаётся.
        var load = TeachingRegistryPersistence.LoadIntoRegistries(
            snapshot,
            topology,
            units,
            flows,
            IssueReplayVerificationCapability());
        Assert.True(load.IsSuccess, load.ErrorMessage);

        // Поток берётся тем же штатным получением из реестра потоков.
        var binding = flows.ResolveBindingBySubPhase(
            restoredManifest.SubPhaseId,
            new LogicalFlowFactoryRequest(
                AnsweredDeliveryWiringRunId,
                new LogicalExecutionFlags(),
                units,
                new Dictionary<string, object>(StringComparer.Ordinal),
                topology));
        Assert.True(binding.IsSuccess, binding.ErrorMessage);
        var flow = Assert.IsType<GeneratedCapabilityFlow>(binding.Value!.Flow);

        // Обычная выдача проходит без единого отказа.
        var served = await flow.ExecuteAsync(
            BuildGeneratedFlowRequest(restoredManifest, AnsweredDeliveryWiringQuery));
        Assert.True(served.IsSuccess, served.ErrorMessage);

        // Ни одной новой строки показателей не появляется: поведение в точности прежнее.
        Assert.False(served.Value!.Metrics.ContainsKey("answered_delivery_recorded"));
        Assert.False(served.Value.Metrics.ContainsKey("answered_delivery_owner_id"));
        Assert.False(served.Value.Metrics.ContainsKey("answered_delivery_refusal_code"));

        // Доставка исправления по этой выдаче получает ИМЕНОВАННЫЙ отказ, а не угаданного адресата.
        var directory = new LogicalUnitRegistryAddressedCorrectionDirectory<string>(units);
        var writer = new ExperimentFixes.Q12NativeTrainingWriter(
            units,
            new Dictionary<string, List<LogicalKnnTrainingExample<string, string>>>(StringComparer.Ordinal),
            AnsweredDeliveryWiringQuery);
        var journalRoot = Path.Combine(
            Path.GetTempPath(),
            "llm-q12-answered-delivery-wiring",
            $"previous-{Guid.NewGuid():N}");
        var coordinator = new KnnAddressedCorrectionCoordinator<string>(directory, writer, journalRoot);
        var ledger = new KnnAnsweredDeliveryLedger<string>(directory, coordinator);

        try
        {
            var refused = await ledger.ApplyConfirmedCorrectionAsync(
                AnsweredDeliveryWiringRunId,
                restoredManifest.FlowId,
                "fb-q12-wiring-previous",
                AnsweredDeliveryWiringSecondOwnerExampleId,
                "archive then notify");

            // Отказ назван своим кодом «выдача не запомнена».
            Assert.False(refused.IsSuccess);
            Assert.Equal(
                KnnAnsweredDeliveryLedger<string>.UnknownDeliveryErrorCode,
                refused.ErrorCode);

            // Ни одного обучения при этом не произошло.
            Assert.Equal(0, writer.TrainCallCount);
        }
        finally
        {
            if (Directory.Exists(journalRoot))
                Directory.Delete(journalRoot, recursive: true);
        }
    }

    // Обратиться к одному получателю обычным путём: чтение последней версии владельца из реестра
    // по его опознанию и обычный вызов его ответа.
    private static async Task<string> AnsweredDeliveryWiringAnswerOf(
        LogicalUnitRegistryAddressedCorrectionDirectory<string> directory,
        string unitId,
        string unitRole,
        string domainTag)
    {
        // Владелец читается тем же продуктовым путём, которым его читает координатор.
        var resolved = directory.ResolveLatestOwnerById(unitId);
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);

        // Ответ запрашивается обычным конвертом запроса к единице.
        var answered = await resolved.Value!.Unit.ExecuteAsync(new LogicalUnitRequest<string>(
            AnsweredDeliveryWiringRunId,
            unitId,
            unitRole,
            domainTag,
            AnsweredDeliveryWiringQuery,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>(StringComparer.Ordinal)));
        Assert.True(answered.IsSuccess, answered.ErrorMessage);

        // Наружу уходит фактический ответ получателя.
        return answered.Value!.Output;
    }

    // Сохранить наблюдённые значения уликой рядом с объявленными ожиданиями.
    private static void WriteAnsweredDeliveryWiringEvidence(string fileName, string[] lines)
    {
        // Корень дерева ищется вверх от каталога сборки: жёсткого пути в проверке нет.
        var root = new DirectoryInfo(AppContext.BaseDirectory);

        // Корнем считается каталог, в котором лежат и проект обучения способностей, и улики.
        while (root is not null &&
               !(Directory.Exists(Path.Combine(root.FullName, "18.CapabilityTraining")) &&
                 Directory.Exists(Path.Combine(root.FullName, "artifacts"))))
            root = root.Parent;

        // Без найденного корня улика не пишется, но и проверка от этого не падает.
        if (root is null)
            return;

        // Точный каталог улик этого наряда.
        var evidenceDirectory = Path.Combine(
            root.FullName,
            "artifacts",
            "fbe",
            "Q12-ANSWERED-DELIVERY-WIRING",
            "attempt-0001");

        // Каталог создаётся, если его ещё нет.
        Directory.CreateDirectory(evidenceDirectory);

        // Наблюдения пишутся простыми строками «имя: значение», чтобы их читал человек.
        // Перенос строки задаётся явно одним знаком: улики проекта хранятся с переносом вида
        // LF, а не с переносом Windows, иначе файл улики не проходит гейт формата улик.
        File.WriteAllText(
            Path.Combine(evidenceDirectory, fileName),
            string.Join('\n', lines) + '\n');
    }
}
