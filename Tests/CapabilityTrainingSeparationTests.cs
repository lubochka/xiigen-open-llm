// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Buffers.Binary;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DPO;

public sealed class CapabilityTrainingSeparationTests
{
    [Fact]
    public void CapabilityTrainingExecutionTypesLiveInCapabilityTrainingAssembly()
    {
        var assemblyName = typeof(DefaultLogicalCapabilityTrainer).Assembly.GetName().Name;

        Assert.Equal("CapabilityTraining", assemblyName);
        Assert.Equal(assemblyName, typeof(LogicalDpoTrainingUnit).Assembly.GetName().Name);
        Assert.Equal(assemblyName, typeof(ReplayFusionRanker).Assembly.GetName().Name);
        Assert.Equal(assemblyName, typeof(TeachingRegistryPersistence).Assembly.GetName().Name);
        Assert.Equal(assemblyName, typeof(LogicalTeachingTextCompressor).Assembly.GetName().Name);
        Assert.Equal(assemblyName, typeof(AutonomousLearningReplayMixer).Assembly.GetName().Name);
    }

    [Fact]
    public void AgentInfrastructureAssemblyDoesNotContainCapabilityTrainingImplementations()
    {
        var agentAssembly = typeof(AdaptiveLogicalOrchestrator).Assembly;

        Assert.Null(agentAssembly.GetType("AgentInfrastructure.Logical.DefaultLogicalCapabilityTrainer"));
        Assert.Null(agentAssembly.GetType("AgentInfrastructure.Logical.LogicalDpoTrainingUnit"));
        Assert.Null(agentAssembly.GetType("AgentInfrastructure.Logical.ReplayFusionRanker"));
        Assert.Null(agentAssembly.GetType("AgentInfrastructure.Logical.TeachingRegistryPersistence"));
        Assert.Null(agentAssembly.GetType("AgentInfrastructure.Logical.LogicalTeachingTextCompressor"));
        Assert.Null(agentAssembly.GetType("AgentInfrastructure.Logical.AutonomousLearningReplayMixer"));
    }

    [Fact]
    public void NeutralAdmissionContractsRemainInAgentInfrastructureAndOwnCanonicalRows()
    {
        var contractAssembly = typeof(ILogicalCapabilityTrainingAdmissionAuthority).Assembly;
        Assert.Equal("AgentInfrastructure", contractAssembly.GetName().Name);
        Assert.DoesNotContain(
            contractAssembly.GetReferencedAssemblies(),
            reference => reference.Name == "ContinuousLearning");

        var canonical = LogicalCapabilityTrainingRowIdentity.Create(
            new string('a', LogicalCapabilityTrainingRowIdentity.Sha256HexLength));
        Assert.True(canonical.IsSuccess, canonical.ErrorMessage);
        Assert.False(LogicalCapabilityTrainingRowIdentity.Create(
            new string('A', LogicalCapabilityTrainingRowIdentity.Sha256HexLength)).IsSuccess);
        var source = new[] { canonical.Value };
        var batch = LogicalCapabilityTrainingAdmissionBatch.Create(source);
        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        source[0] = default;
        Assert.True(batch.Value!.Rows[0].IsCanonical);
        Assert.False(batch.Value.Rows[0]
            .HasCanonicalTrainingContentProvenance);
    }

    [Fact]
    public void AgentInfrastructureNoLongerGrantsCapabilityTrainingFriendship()
    {
        var root = FindRepositoryRoot();
        var carriers = new[]
        {
            Path.Combine(
                root,
                "21.AgentInfrastructure",
                "AgentInfrastructure.csproj"),
            Path.Combine(
                root,
                "21.AgentInfrastructure",
                "Logical",
                "BoundedDpoTrainingContentProjector.cs")
        };

        Assert.All(carriers, carrier =>
        {
            var source = File.ReadAllText(carrier);
            Assert.DoesNotContain(
                "InternalsVisibleTo(\"CapabilityTraining\")",
                source,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                "<_Parameter1>CapabilityTraining</_Parameter1>",
                source,
                StringComparison.Ordinal);
        });
    }

    [Fact]
    public void DpoAssemblyNoLongerGrantsAgentInfrastructureFriendship()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "11.DPO",
            "DPO.csproj"));

        Assert.DoesNotContain(
            "<_Parameter1>AgentInfrastructure</_Parameter1>",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "<_Parameter1>ContinuousLearning</_Parameter1>",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "<_Parameter1>LlmFromScratch.Tests</_Parameter1>",
            source,
            StringComparison.Ordinal);
    }

    [Fact]
    public void LogicalCapabilityTrainingAdmissionCapabilities_IsOpaquePublicBundle()
    {
        var bundleType = typeof(ILogicalCapabilityTrainingAdmissionLease)
            .Assembly
            .GetType(
                "AgentInfrastructure.Logical.LogicalCapabilityTrainingAdmissionCapabilities");

        Assert.NotNull(bundleType);
        Assert.True(bundleType!.IsPublic);
        Assert.True(bundleType.IsSealed);
        Assert.Empty(bundleType.GetConstructors(
            BindingFlags.Public | BindingFlags.Instance));
        Assert.Empty(bundleType.GetMethods(
            BindingFlags.Public |
            BindingFlags.Static |
            BindingFlags.DeclaredOnly));

        var declaredMethods = bundleType.GetMethods(
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly);
        Assert.Contains(declaredMethods,
            method => method.Name == "AcquireMutationPermit");
        Assert.Contains(declaredMethods,
            method => method.Name == "SealExecutableManifest");
        Assert.DoesNotContain(declaredMethods,
            method => method.Name is "Issue" or "AcquireDpoMutationPermit" or
                "AcquireDpoTrainerMutationPermit" ||
                method.ReturnType.Name.Contains(
                    "Issuer",
                    StringComparison.Ordinal));

        var declaredProperties = bundleType.GetProperties(
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly);
        var trainerToken = Assert.Single(
            declaredProperties,
            property => property.PropertyType ==
                typeof(DpoMutationAdmissionCapability));
        Assert.Equal(
            "DpoTrainerMutationAdmissionCapability",
            trainerToken.Name);
        Assert.DoesNotContain(declaredProperties,
            property => property.PropertyType.Name.Contains(
                    "Issuer",
                    StringComparison.Ordinal) ||
                property.Name.Contains("Issuer", StringComparison.Ordinal));
    }

    [Fact]
    public void DpoTrainerMutationAdmissionCapability_HasNoPublicForgeOrPermitSurface()
    {
        var capabilityType = typeof(DpoMutationAdmissionCapability);
        var issuerType = typeof(IDpoMutationAdmissionIssuer);

        Assert.True(capabilityType.IsPublic);
        Assert.True(capabilityType.IsSealed);
        Assert.False(issuerType.IsPublic);
        Assert.Empty(capabilityType.GetConstructors(
            BindingFlags.Public | BindingFlags.Instance));
        Assert.Empty(capabilityType.GetMethods(
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.DeclaredOnly));
        Assert.Contains(
            capabilityType.GetMethods(
                BindingFlags.NonPublic |
                BindingFlags.Static |
                BindingFlags.DeclaredOnly),
            method => method.Name == "Issue" &&
                method.ReturnType == typeof(DpoMutationAdmissionCapability));
        Assert.Contains(
            capabilityType.GetMethods(
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly),
            method => method.Name == "AcquireMutationPermit");
    }

    [Fact]
    public void LogicalCapabilityReplayVerificationCapability_IsOpaquePublicVerificationBundle()
    {
        var verifierType = typeof(LogicalCapabilityReplayVerificationCapability);

        Assert.True(verifierType.IsPublic);
        Assert.True(verifierType.IsSealed);
        Assert.Empty(verifierType.GetConstructors(
            BindingFlags.Public | BindingFlags.Instance));
        Assert.Empty(verifierType.GetMethods(
            BindingFlags.Public |
            BindingFlags.Static |
            BindingFlags.DeclaredOnly));
        Assert.Contains(
            verifierType.GetMethods(
                BindingFlags.NonPublic |
                BindingFlags.Static |
                BindingFlags.DeclaredOnly),
            method => method.Name == "Issue");

        var declaredMethods = verifierType.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly)
            .Where(static method => !method.IsSpecialName)
            .OrderBy(static method => method.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            ["VerifyExecutableManifest", "VerifyReplay"],
            declaredMethods.Select(static method => method.Name).ToArray());
        Assert.All(declaredMethods, method =>
        {
            Assert.DoesNotContain(
                "Issuer",
                method.ReturnType.Name,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                method.GetParameters(),
                parameter => parameter.ParameterType.Name.Contains(
                    "Issuer",
                    StringComparison.Ordinal));
        });
        Assert.Empty(verifierType.GetProperties(
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.DeclaredOnly));
    }

    [Fact]
    public void NeutralPublicAdmissionLeaseCannotMintAdmissionCapabilities()
    {
        var method = typeof(ILogicalCapabilityTrainingAdmissionLease)
            .GetMethod(
                "IssueAdmissionCapabilities",
                BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(method);
        var result = method!.Invoke(new NeutralPublicAdmissionLease(), []);
        Assert.NotNull(result);
        var isSuccess = (bool)result.GetType()
            .GetProperty("IsSuccess")!
            .GetValue(result)!;
        var errorCode = (string?)result.GetType()
            .GetProperty("ErrorCode")!
            .GetValue(result);

        Assert.False(isSuccess);
        Assert.Equal(
            "LOGICAL_CAPABILITY_TRAINING_ADMISSION_CAPABILITIES_REQUIRED",
            errorCode);
    }

    [Fact]
    public void CapabilityTrainingAdmissionBatch_UsesExactCanonicalCurrentReplayOrderAndDigests()
    {
        const string runId = "fixed-admission-run";
        const string domain = "PLAN";
        var provenance = CreateOpaqueSyntheticProvenance();
        var projection = LogicalCapabilityTrainingPreferenceProjector
            .ProjectCurrent(
                "synthetic request",
                CreateSyntheticBranch(),
                new LogicalSubPhase(
                    "synthetic-subphase",
                    "synthetic-phase",
                    "synthetic name",
                    "synthetic purpose",
                    ["synthetic-domain"],
                    ["SYNTHETIC"],
                    []));
        Assert.True(projection.IsSuccess, projection.ErrorMessage);
        var current = new LogicalDpoPreferenceExample(
            projection.Value!.Prompt,
            projection.Value.Chosen,
            projection.Value.Rejected,
            projection.Value.TeachingVector)
        {
            TrainingContentProvenance = provenance.Snapshot()
        };
        var replayVector = new[] { -2.25f, 3.5f };
        var replayDigest = LogicalCapabilityTrainingPreferenceProjector
            .ComputeDpoPreferenceSha256(
                "replay-prompt",
                "chosen-replay",
                "rejected-replay-end",
                replayVector).Value!;
        var replay = new LogicalDpoPreferenceExample(
            "replay-prompt",
            "chosen-replay",
            "rejected-replay-end",
            replayVector)
        {
            ReplayDescriptor = new(
                LogicalCapabilityTrainingReplayDescriptor.CurrentSchemaVersion,
                LogicalTrainingContentSourceKind
                    .DeterministicSyntheticNoExternalTeacher,
                provenance.ReplaySealGroupSha256!,
                new LogicalCapabilityTrainingSourceBindingComposite(
                    [provenance.BindingSha256]),
                new string('d', 64),
                replayDigest),
            ReplayAttestation = new(
                LogicalCapabilityTrainingReplayDescriptor.CurrentSchemaVersion,
                LogicalTrainingContentSourceKind
                    .DeterministicSyntheticNoExternalTeacher,
                provenance.ReplaySealGroupSha256!,
                new LogicalCapabilityTrainingSourceBindingComposite(
                    [provenance.BindingSha256]),
                new string('d', 64),
                replayDigest,
                $"sha256:{new string('e', 64)}",
                new string('f', 64))
        };
        LogicalDpoPreferenceExample[] finalOrderedRows =
            [current, current with { TeachingVector = current.TeachingVector!.ToArray() }, replay];

        var batch = DefaultLogicalCapabilityTrainer.BuildAdmissionBatch(
            runId,
            domain,
            finalOrderedRows);

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        var expected = finalOrderedRows
            .Select((row, ordinal) => ComputeCanonicalRowDigestForTest(
                runId,
                domain,
                ordinal,
                row))
            .ToArray();
        Assert.Equal(expected,
            batch.Value!.Rows.Select(static row => row.Sha256).ToArray());
        Assert.All(batch.Value.Rows, static row =>
            Assert.True(row.HasCanonicalTrainingContentProvenance));
        Assert.Equal(
            [
                LogicalCapabilityTrainingAdmissionRowKind.Current,
                LogicalCapabilityTrainingAdmissionRowKind.Current,
                LogicalCapabilityTrainingAdmissionRowKind.Replay
            ],
            batch.Value.Rows.Select(static row => row.RowKind).ToArray());
    }

    [Fact]
    public void CapabilityTrainingAdmissionBatch_RejectsUnattestedPreferenceRows()
    {
        var batch = DefaultLogicalCapabilityTrainer.BuildAdmissionBatch(
            "unattested-run",
            "PLAN",
            [new LogicalDpoPreferenceExample(
                "unattested-prompt",
                "chosen",
                "rejected",
                [1f])]);

        Assert.False(batch.IsSuccess);
        Assert.Equal(
            DefaultLogicalCapabilityTrainer
                .TrainingContentProvenanceRequiredErrorCode,
            batch.ErrorCode);
    }

    [Fact]
    public void SyntheticTrainingAttestation_IsOpaqueBoundedAndDeepSnapshotBound()
    {
        Assert.Empty(typeof(LogicalTrainingContentProvenance)
            .GetConstructors());
        var branch = CreateSyntheticBranch();
        var provenance = LogicalTrainingContentProvenance
            .IssueDeterministicSynthetic(branch);
        Assert.True(provenance.Attests(branch));

        var changedMetrics = new Dictionary<string, string>(
            branch.Metrics,
            StringComparer.Ordinal)
        {
            ["changed"] = "true"
        };
        Assert.False(provenance.Attests(branch with
        {
            Metrics = changedMetrics
        }));
        Assert.False(provenance.Attests(branch with
        {
            Metrics = Enumerable.Range(0, 257).ToDictionary(
                static index => $"metric-{index}",
                static index => index.ToString(),
                StringComparer.Ordinal)
        }));
    }

    internal static LogicalTrainingContentProvenance
        CreateOpaqueSyntheticProvenance()
        => LogicalCapabilityTrainingAuthorityTestFactory.IssueSingle(
                CreateSyntheticBranch(),
                new LogicalTrainingContentDeterministicInputs(
                    "synthetic request",
                    "PLAN",
                    "synthetic-phase",
                    "synthetic-subphase",
                    "synthetic name",
                    "synthetic purpose",
                    ["SYNTHETIC"],
                    ["synthetic-domain"]));

    internal static LogicalCapabilityTrainingCurrentProjection
        CreateOpaqueSyntheticCurrentProjection()
        => LogicalCapabilityTrainingPreferenceProjector.ProjectCurrent(
                "synthetic request",
                CreateSyntheticBranch(),
                new LogicalSubPhase(
                    "synthetic-subphase",
                    "synthetic-phase",
                    "synthetic name",
                    "synthetic purpose",
                    ["synthetic-domain"],
                    ["SYNTHETIC"],
                    []))
            .Value!;

    private static LogicalFlowResult CreateSyntheticBranch()
    {
        return new LogicalFlowResult(
            "synthetic-flow",
            "synthetic-scenario",
            "KNOWLEDGE_EDGE",
            "synthetic result",
            new LogicalFlowSummary(
                "synthetic-summary",
                1,
                0,
                0,
                "knowledge_edge",
                false,
                true,
                false,
                "SCHEDULE_CAPABILITY_LEARNING",
                1,
                "synthetic",
                "synthetic",
                0,
                0),
            new Dictionary<string, string>(StringComparer.Ordinal),
            [],
            []);
    }

    private static string ComputeCanonicalRowDigestForTest(
        string runId,
        string domain,
        int ordinal,
        LogicalDpoPreferenceExample row)
    {
        using var payload = new MemoryStream();
        WriteString("logical-capability-training-row-v1");
        WriteString(runId);
        WriteString(domain);
        WriteInt32(ordinal);
        WriteString(row.Prompt);
        WriteString(row.Chosen);
        WriteString(row.Rejected);
        WriteInt32(row.TeachingVector!.Length);
        foreach (var value in row.TeachingVector)
            WriteInt32(BitConverter.SingleToInt32Bits(value));
        return Convert.ToHexString(SHA256.HashData(payload.ToArray()))
            .ToLowerInvariant();

        void WriteString(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            WriteInt32(bytes.Length);
            payload.Write(bytes, 0, bytes.Length);
        }

        void WriteInt32(int value)
        {
            Span<byte> bytes = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(bytes, value);
            payload.Write(bytes);
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(
                    directory.FullName,
                    "LlmFromScratch.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate the repository root.");
    }

    private sealed class NeutralPublicAdmissionLease
        : ILogicalCapabilityTrainingAdmissionLease
    {
        public OperationResult<IDisposable> AcquireMutationPermit()
            => OperationResult<IDisposable>.Ok(new Permit());

        public void Dispose()
        {
        }

        private sealed class Permit : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}

internal sealed class RecordingLogicalCapabilityTrainingAdmissionAuthority(
    Func<int, OperationResult<IDisposable>>? permitFactory = null,
    Action<LogicalCapabilityTrainingAdmissionBatch>? leaseObserver = null,
    Func<Exception?>? leaseDisposeFailureFactory = null)
    : ILogicalCapabilityTrainingAdmissionAuthority,
      ILogicalCapabilityReplayVerificationIssuer
{
    private int _leaseCount;
    private int _permitCount;
    private int _bundleCount;
    private int _leaseDisposeCount;

    internal int LeaseCount => Volatile.Read(ref _leaseCount);
    internal int PermitCount => Volatile.Read(ref _permitCount);
    internal int BundleCount => Volatile.Read(ref _bundleCount);
    internal int LeaseDisposeCount => Volatile.Read(ref _leaseDisposeCount);
    internal LogicalCapabilityTrainingAdmissionBatch? LastBatch { get; private set; }
    internal LogicalCapabilityTrainingAdmissionCapabilities? LastBundle
    {
        get;
        private set;
    }

    public OperationResult<ILogicalCapabilityTrainingAdmissionLease> AcquireTrainingLease(
        LogicalCapabilityTrainingAdmissionBatch batch)
    {
        Interlocked.Increment(ref _leaseCount);
        LastBatch = batch;
        leaseObserver?.Invoke(batch);
        return OperationResult<ILogicalCapabilityTrainingAdmissionLease>.Ok(
            new Lease(this, permitFactory, leaseDisposeFailureFactory));
    }

    public OperationResult<bool> VerifyReplay(
        LogicalCapabilityTrainingReplayAttestation attestation)
        => attestation is not null && attestation.IsCanonical
            ? OperationResult<bool>.Ok(true)
            : OperationResult<bool>.Fail(
                "Replay verification failed.",
                LogicalCapabilityTrainingAdmissionBatch
                    .ProvenanceInvalidErrorCode);

    OperationResult<bool>
        ILogicalCapabilityReplayVerificationIssuer.VerifyReplayAttestation(
            LogicalCapabilityTrainingReplayAttestation attestation)
        => VerifyReplay(attestation);

    OperationResult<bool> ILogicalCapabilityReplayVerificationIssuer
        .VerifyExecutableManifestAttestation(
            string executableManifestSha256,
            LogicalCapabilityTrainingReplayAttestation replayAttestation,
            LogicalCapabilityExecutableManifestAttestation
                executableAttestation)
    {
        var replay = VerifyReplay(replayAttestation);
        return replay.IsSuccess && replay.Value &&
               executableAttestation is not null &&
               executableAttestation.Matches(
                   executableManifestSha256,
                   replayAttestation)
            ? OperationResult<bool>.Ok(true)
            : OperationResult<bool>.Fail(
                "Executable manifest verification failed.",
                LogicalCapabilityTrainingAdmissionBatch
                    .ProvenanceInvalidErrorCode);
    }

    private sealed class Lease(
        RecordingLogicalCapabilityTrainingAdmissionAuthority owner,
        Func<int, OperationResult<IDisposable>>? permitFactory,
        Func<Exception?>? leaseDisposeFailureFactory)
        : ILogicalCapabilityTrainingAdmissionLease,
          ILogicalDpoMutationAdmissionIssuer,
          IDpoMutationAdmissionIssuer,
          ILogicalCapabilityExecutableManifestAttestationIssuer
    {
        private int _disposed;

        public OperationResult<IDisposable> AcquireMutationPermit()
        {
            var ordinal = Interlocked.Increment(ref owner._permitCount);
            return permitFactory?.Invoke(ordinal)
                   ?? OperationResult<IDisposable>.Ok(new Permit());
        }

        OperationResult<IDisposable>
            ILogicalDpoMutationAdmissionIssuer.AcquireDpoMutationPermit()
            => AcquireMutationPermit();

        IDisposable IDpoMutationAdmissionIssuer
            .AcquireDpoTrainerMutationPermit()
        {
            var permit = AcquireMutationPermit();
            if (!permit.IsSuccess || permit.Value is null)
            {
                throw new DpoMutationAdmissionException(
                    permit.ErrorMessage,
                    permit.ErrorCode);
            }

            return permit.Value;
        }

        public OperationResult<LogicalCapabilityTrainingAdmissionCapabilities>
            IssueAdmissionCapabilities()
        {
            if (Volatile.Read(ref _disposed) != 0)
            {
                return OperationResult<
                    LogicalCapabilityTrainingAdmissionCapabilities>.Fail(
                    "The test admission lease was revoked.",
                    "TEST_ADMISSION_REVOKED");
            }

            var logicalMutationAdmission =
                LogicalDpoMutationAdmissionCapability.Issue(this);
            if (!logicalMutationAdmission.IsSuccess ||
                logicalMutationAdmission.Value is null)
            {
                return logicalMutationAdmission.MapError<
                    LogicalCapabilityTrainingAdmissionCapabilities>();
            }

            var executableManifestAttestation =
                LogicalCapabilityExecutableManifestAttestationCapability.Issue(
                    this);
            if (!executableManifestAttestation.IsSuccess ||
                executableManifestAttestation.Value is null)
            {
                return executableManifestAttestation.MapError<
                    LogicalCapabilityTrainingAdmissionCapabilities>();
            }

            var bundle = new LogicalCapabilityTrainingAdmissionCapabilities(
                logicalMutationAdmission.Value,
                DpoMutationAdmissionCapability.Issue(this),
                executableManifestAttestation.Value);
            Interlocked.Increment(ref owner._bundleCount);
            owner.LastBundle = bundle;
            return OperationResult<
                LogicalCapabilityTrainingAdmissionCapabilities>.Ok(bundle);
        }

        public OperationResult<LogicalCapabilityTrainingReplayAttestation>
            SealReplay(LogicalCapabilityTrainingReplayDescriptor descriptor)
            => descriptor is not null && descriptor.IsCanonical &&
               Volatile.Read(ref _disposed) == 0
                ? OperationResult<
                    LogicalCapabilityTrainingReplayAttestation>.Ok(
                    new(
                        descriptor.SchemaVersion,
                        descriptor.OriginalSourceKind,
                        descriptor.ReplaySealGroupSha256,
                        descriptor.SourceBindings.Snapshot(),
                        descriptor.ReplayPayloadSha256,
                        descriptor.DpoPreferenceSha256,
                        $"sha256:{new string('a', 64)}",
                        new string('b', 64)))
                : OperationResult<
                    LogicalCapabilityTrainingReplayAttestation>.Fail(
                    "Replay sealing is unavailable.",
                    LogicalCapabilityTrainingAdmissionBatch
                        .ReplaySealUnavailableErrorCode);

        OperationResult<LogicalCapabilityExecutableManifestAttestation>
            ILogicalCapabilityExecutableManifestAttestationIssuer
                .SealExecutableManifest(
                    string executableManifestSha256,
                    LogicalCapabilityTrainingReplayAttestation
                        replayAttestation)
            => Volatile.Read(ref _disposed) == 0 &&
               LogicalCapabilityTrainingReplayDescriptor.IsCanonicalSha256(
                   executableManifestSha256) &&
               replayAttestation is { IsCanonical: true }
                ? OperationResult<
                    LogicalCapabilityExecutableManifestAttestation>.Ok(
                    new(
                        LogicalCapabilityExecutableManifestAttestation
                            .CurrentSchemaVersion,
                        executableManifestSha256,
                        replayAttestation.AuthorityKeyRef,
                        replayAttestation.MacSha256,
                        $"sha256:{new string('e', 64)}",
                        new string('f', 64)))
                : OperationResult<
                    LogicalCapabilityExecutableManifestAttestation>.Fail(
                    "Executable manifest sealing is unavailable.",
                    LogicalCapabilityExecutableManifestAttestationCapability
                        .RequiredErrorCode);

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                Interlocked.Increment(ref owner._leaseDisposeCount);
                var failure = leaseDisposeFailureFactory?.Invoke();
                if (failure is not null)
                    throw failure;
            }
        }
    }

    private sealed class Permit : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
