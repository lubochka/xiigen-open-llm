// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using ContinuousLearning;
using LoRA;
using TensorCore.Core;
using TensorCore.Layers;
using TransformerModel;
using TransformerModel.Config;

public sealed class ServerOwnedContinuousLearningAdapterRootCapabilityTests
{
    private const string AdapterOwnerLockName =
        ".continuous-learning.adapter-owner.lock";

    [Fact]
    public void CapabilitySurface_ExposesTypedPublicationOnlyAndNoPathOrNativeHandle()
    {
        var type = typeof(ServerOwnedContinuousLearningAdapterRootCapability);
        Assert.False(type.IsPublic);
        Assert.True(type.IsSealed);
        var fields = type.GetFields(BindingFlags.Instance |
                                    BindingFlags.Public |
                                    BindingFlags.NonPublic);
        Assert.DoesNotContain(fields, field => field.FieldType == typeof(string));
        Assert.DoesNotContain(
            fields,
            field => field.FieldType == typeof(Microsoft.Win32.SafeHandles.SafeFileHandle) &&
                     !field.IsPrivate);
        var methods = type.GetMethods(BindingFlags.Instance |
                                      BindingFlags.Static |
                                      BindingFlags.Public |
                                      BindingFlags.NonPublic |
                                      BindingFlags.DeclaredOnly)
            .Where(method => method.IsPublic || method.IsAssembly || method.IsFamilyOrAssembly)
            .ToArray();
        Assert.DoesNotContain(
            methods,
            method => method.ReturnType == typeof(string) ||
                      method.ReturnType == typeof(Microsoft.Win32.SafeHandles.SafeFileHandle) ||
                      method.GetParameters().Any(parameter =>
                          parameter.ParameterType == typeof(string) ||
                          parameter.ParameterType == typeof(Microsoft.Win32.SafeHandles.SafeFileHandle)));
        var publish = Assert.Single(methods, method => method.Name == "Publish");
        Assert.Equal(
            typeof(ServerOwnedContinuousLearningAdapterPublication),
            publish.ReturnType);
        Assert.Equal(
            typeof(ServerOwnedContinuousLearningAdapterPublicationRequest),
            publish.GetParameters()[1].ParameterType);
        var readExact = methods.Where(method => method.Name == "ReadExact").ToArray();
        Assert.Equal(2, readExact.Length);
        Assert.All(
            readExact,
            method =>
            {
                Assert.Equal(
                    typeof(ServerOwnedContinuousLearningAdapterSnapshot),
                    method.ReturnType);
                Assert.DoesNotContain(
                    method.GetParameters(),
                    parameter => parameter.ParameterType == typeof(string) ||
                                 parameter.ParameterType == typeof(byte[]) ||
                                 parameter.ParameterType == typeof(Stream) ||
                                 parameter.ParameterType == typeof(FileStream) ||
                                 typeof(System.Runtime.InteropServices.SafeHandle)
                                     .IsAssignableFrom(parameter.ParameterType));
            });
        Assert.Contains(
            readExact,
            method => method.GetParameters().Select(parameter => parameter.ParameterType)
                .SequenceEqual(
                    [
                        typeof(ServerOwnedContinuousLearningAdapterPublication),
                        typeof(CancellationToken),
                    ]));
        Assert.Contains(
            readExact,
            method => method.GetParameters().Select(parameter => parameter.ParameterType)
                .SequenceEqual(
                    [
                        typeof(AdapterArtifactReference),
                        typeof(CancellationToken),
                    ]));

        var publicationProperties = typeof(ServerOwnedContinuousLearningAdapterPublication)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public);
        Assert.Equal(
            typeof(string),
            publicationProperties.Single(property => property.Name == "Sha256Hex").PropertyType);
        Assert.DoesNotContain(
            publicationProperties,
            property => property.PropertyType == typeof(byte[]));
        Assert.Equal(
            typeof(ServerOwnedContinuousLearningAdapterArtifactIdentity),
            publicationProperties.Single(property => property.Name == "FinalArtifactIdentity").PropertyType);

        var referenceType = typeof(AdapterArtifactReference);
        Assert.False(referenceType.IsPublic);
        Assert.True(referenceType.IsSealed);
        Assert.Equal(
            typeof(AdapterArtifactReferenceJsonConverter),
            Assert.Single(referenceType.GetCustomAttributes<JsonConverterAttribute>())
                .ConverterType);
        var referenceProperties = referenceType.GetProperties(
            BindingFlags.Instance | BindingFlags.Public);
        Assert.Equal(
            [
                "CanonicalLeafName",
                "FinalArtifactIdentity",
                "Length",
                "RetainedRootIdentity",
                "SchemaVersion",
                "Sha256Hex",
                "StorageKind",
            ],
            referenceProperties.Select(property => property.Name).Order());
        Assert.Equal(
            ["CanonicalLeafName", "Sha256Hex"],
            referenceProperties
                .Where(property => property.PropertyType == typeof(string))
                .Select(property => property.Name)
                .Order());
        Assert.All(
            referenceProperties,
            property => Assert.Contains(
                typeof(System.Runtime.CompilerServices.IsExternalInit),
                property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers()));
        Assert.DoesNotContain(
            referenceProperties,
            property => property.PropertyType == typeof(byte[]) ||
                        property.PropertyType == typeof(Stream) ||
                        property.PropertyType == typeof(FileStream) ||
                        typeof(System.Runtime.InteropServices.SafeHandle)
                            .IsAssignableFrom(property.PropertyType));

        var snapshotType = typeof(ServerOwnedContinuousLearningAdapterSnapshot);
        Assert.DoesNotContain(
            snapshotType.GetFields(BindingFlags.Instance |
                                   BindingFlags.Public |
                                   BindingFlags.NonPublic),
            field => field.FieldType == typeof(byte[]) && !field.IsPrivate);
        Assert.DoesNotContain(
            snapshotType.GetProperties(BindingFlags.Instance |
                                       BindingFlags.Public |
                                       BindingFlags.NonPublic),
            property => property.PropertyType == typeof(byte[]) ||
                        property.PropertyType == typeof(Stream) ||
                        property.PropertyType == typeof(FileStream) ||
                        typeof(System.Runtime.InteropServices.SafeHandle)
                            .IsAssignableFrom(property.PropertyType));
    }

    [ContinuousLearningAdapterCapabilityLinuxX64Fact]
    public void PosixPublishRejectsFinalSymlinkCollisionWithoutFollowingTarget()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        var request = CreateRequest();
        var outsideTarget = Path.Combine(fixture.Root, "outside-adapter-target.lora");
        File.WriteAllBytes(outsideTarget, [0xAA, 0xBB, 0xCC]);
        SetLinuxOwnerWritableFile(outsideTarget);
        var finalPath = Path.Combine(
            fixture.AdapterRoot,
            request.GetCanonicalFinalLeafName());
        File.CreateSymbolicLink(finalPath, outsideTarget);

        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability();
        var model = CreateAttachedModel();
        FillSequential(model.Parameters());

        // Угроза: строковая публикация могла бы пройти по symlink и записать
        // чужой target. POSIX ветка обязана упереться в no-replace leaf.
        Assert.Throws<IOException>(() => capability.Publish(model, request));
        Assert.NotNull(File.ResolveLinkTarget(finalPath, returnFinalTarget: false));
        Assert.Equal(new byte[] { 0xAA, 0xBB, 0xCC }, File.ReadAllBytes(outsideTarget));
        access.EnsureRetained();
    }

    [ContinuousLearningAdapterCapabilityLinuxX64Fact]
    public void PosixPublishUsesCreateOnlyFinalLeafAndSelfValidatingReceipt()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        var stages = new List<ServerOwnedContinuousLearningAdapterIoObservation>();
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                ObserveIoStageForTesting = stages.Add,
            });
        var model = CreateAttachedModel();
        FillSequential(model.Parameters());
        var request = CreateRequest();

        var publication = capability.Publish(model, request);

        // POSIX не публикует source-name rename: final leaf создаётся сразу
        // через no-replace/no-follow open, а receipt доверяется только после
        // readback hash и root fsync.
        Assert.Equal(request.GetCanonicalFinalLeafName(), publication.CanonicalLeafName);
        Assert.All(stages, observation => Assert.Null(observation.CanonicalTemporaryLeafName));
        Assert.DoesNotContain(
            stages,
            observation => observation.Stage ==
                ServerOwnedContinuousLearningAdapterIoStage.AfterTemporaryLeafCreated);
        Assert.Empty(Directory.EnumerateFiles(fixture.AdapterRoot, "*.tmp"));
        Assert.True(File.Exists(Path.Combine(fixture.AdapterRoot, publication.CanonicalLeafName)));
        Assert.Equal(
            publication.Sha256Hex,
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(
                fixture.AdapterRoot,
                publication.CanonicalLeafName)))).ToLowerInvariant());
        var snapshot = capability.ReadExact(publication);
        Assert.Equal(publication.Length, snapshot.Length);
        access.EnsureRetained();
    }

    [ContinuousLearningAdapterCapabilityLinuxX64Fact]
    public void PosixPublishedArtifactHardLink_IsRejectedByExactRead()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability();
        var model = CreateAttachedModel();
        FillSequential(model.Parameters());
        var publication = capability.Publish(model, CreateRequest());
        var finalPath = Path.Combine(fixture.AdapterRoot, publication.CanonicalLeafName);
        CreateHardLinkPosixOrFail(
            Path.Combine(fixture.AdapterRoot, "unconfigured-adapter-alias.lora"),
            finalPath);

        // Угроза: nlink>1 даёт второй writable name на те же adapter bytes.
        // Exact read должен отказать через public receipt и сделать trust
        // sticky-lost, потому что опубликованный artifact уже не одиночный.
        Assert.ThrowsAny<Exception>(() => capability.ReadExact(publication));
        Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
    }

    [ContinuousLearningAdapterCapabilityLinuxX64Fact]
    public void PosixGroupWritableOwnerLock_IsRejectedBeforeCapabilityBinds()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        var lockPath = Path.Combine(fixture.AdapterRoot, AdapterOwnerLockName);
        File.WriteAllBytes(lockPath, [0xA1]);
        SetLinuxGroupWritableFile(lockPath);

        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();

        // Owner-lock хранит lease-домен. Если он group/other writable,
        // capability не имеет server-owned границы и должна fail closed.
        Assert.ThrowsAny<Exception>(() => access.AcquireAdapterRootCapability());
        access.EnsureRetained();
    }

    [ContinuousLearningAdapterCapabilityLinuxX64Fact]
    public void PosixRootReplacementDuringPublishFlush_RevokesRetainedTrust()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        var request = CreateRequest();
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var replaced = false;
        using var capability = access.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                ObserveIoStageForTesting = observation =>
                {
                    if (observation.Stage !=
                            ServerOwnedContinuousLearningAdapterIoStage
                                .BeforeRootDirectoryFlush ||
                        replaced)
                    {
                        return;
                    }

                    replaced = true;
                    ReplaceDirectoryWithFreshOwnerBoundary(fixture.AdapterRoot);
                },
            });

        // Rename уже мог состояться, поэтому метод честно возвращает
        // indeterminate commit, но retained trust всё равно sticky-lost.
        var exception = Assert.Throws<AdapterPublicationCommitIndeterminateException>(
            () => capability.Publish(CreateAttachedModel(), request));
        Assert.Equal(
            request.GetCanonicalFinalLeafName(),
            exception.CommittedArtifact.CanonicalLeafName);
        Assert.True(replaced);
        Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
    }

    [ContinuousLearningAdapterWindowsFact]
    public void Publish_RoundTripsExactAdapterArtifactAndReportsVerifiedHash()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability();
        var model = CreateAttachedModel();
        FillSequential(model.Parameters());
        var request = CreateRequest();

        var publication = capability.Publish(model, request);

        var expectedLeaf = request.GetCanonicalFinalLeafName();
        Assert.Equal(expectedLeaf, publication.CanonicalLeafName);
        Assert.True(publication.Length > 0);
        Assert.NotEqual(default, publication.FinalArtifactIdentity);
        Assert.Matches("^[0-9a-f]{64}$", publication.Sha256Hex);
        var finalPath = Path.Combine(fixture.AdapterRoot, expectedLeaf);
        Assert.True(File.Exists(finalPath));
        Assert.Equal(publication.Length, new FileInfo(finalPath).Length);
        Assert.Equal(
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(finalPath))).ToLowerInvariant(),
            publication.Sha256Hex);
        using var stream = File.OpenRead(finalPath);
        model.ValidateCurrentAdapterCheckpointArtifact(stream);
        Assert.Equal(stream.Length, stream.Position);
        Assert.Empty(Directory.EnumerateFiles(fixture.AdapterRoot, "*.tmp"));
    }

    [ContinuousLearningAdapterWindowsFact]
    public void ReadExact_PublishRoundTripReturnsExactDefensiveImmutableSnapshot()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability();
        var model = CreateAttachedModel();
        FillSequential(model.Parameters());
        var publication = capability.Publish(model, CreateRequest());
        var expected = File.ReadAllBytes(Path.Combine(
            fixture.AdapterRoot,
            publication.CanonicalLeafName));

        var snapshot = capability.ReadExact(publication);

        Assert.Equal(publication, snapshot.Publication);
        Assert.Equal(publication.CanonicalLeafName, snapshot.CanonicalLeafName);
        Assert.Equal(publication.RetainedRootIdentity, snapshot.RetainedRootIdentity);
        Assert.Equal(publication.FinalArtifactIdentity, snapshot.FinalArtifactIdentity);
        Assert.Equal(publication.Length, snapshot.Length);
        Assert.Equal(publication.Sha256Hex, snapshot.Sha256Hex);
        Assert.Equal(expected, snapshot.CopyBytes());
        var mutableCopy = snapshot.CopyBytes();
        mutableCopy[0] ^= 0xff;
        Assert.Equal(expected, snapshot.CopyBytes());
    }

    [ContinuousLearningAdapterWindowsFact]
    public void ReadExactReference_JsonRoundTripAndReacquiredCapabilityReturnExactBytesAndEvidence()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        ServerOwnedContinuousLearningAdapterPublication publication;
        AdapterArtifactReference reference;
        byte[] expected;
        using (var publishingAccess = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var publishingCapability = publishingAccess.AcquireAdapterRootCapability())
        {
            publication = publishingCapability.Publish(CreateAttachedModel(), CreateRequest());
            reference = AdapterArtifactReference.FromPublication(publication);
            expected = File.ReadAllBytes(Path.Combine(
                fixture.AdapterRoot,
                publication.CanonicalLeafName));
        }

        var json = JsonSerializer.Serialize(reference);
        var roundTripped = Assert.IsType<AdapterArtifactReference>(
            JsonSerializer.Deserialize<AdapterArtifactReference>(json));

        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability();
        var snapshot = capability.ReadExact(roundTripped);

        Assert.Equal(reference, roundTripped);
        Assert.Equal(publication, snapshot.Publication);
        Assert.Equal(publication.CanonicalLeafName, snapshot.CanonicalLeafName);
        Assert.Equal(publication.RetainedRootIdentity, snapshot.RetainedRootIdentity);
        Assert.Equal(publication.FinalArtifactIdentity, snapshot.FinalArtifactIdentity);
        Assert.Equal(publication.Length, snapshot.Length);
        Assert.Equal(publication.Sha256Hex, snapshot.Sha256Hex);
        Assert.Equal(expected, snapshot.CopyBytes());
    }

    [Fact]
    public void AdapterArtifactReference_ConversionsAndJsonContractRoundTripExactly()
    {
        var publication = CreateSyntheticPublication();

        var reference = AdapterArtifactReference.FromPublication(publication);
        var convertedPublication = reference.ToPublication();
        var convertedReference = AdapterArtifactReference.FromPublication(convertedPublication);
        var json = JsonSerializer.Serialize(reference);

        Assert.Equal(publication, convertedPublication);
        Assert.Equal(reference, convertedReference);
        Assert.Equal(
            "{\"schema_version\":1,\"storage_kind\":1," +
            "\"canonical_leaf_name\":\"adapter-v2--d1-a--p1-b.lora\"," +
            "\"retained_root_identity\":{\"volume_serial_number\":11,\"file_id_low\":12,\"file_id_high\":13}," +
            "\"final_artifact_identity\":{\"volume_serial_number\":21,\"file_id_low\":22,\"file_id_high\":23}," +
            "\"length\":42,\"sha256_hex\":\"" + new string('a', 64) + "\"}",
            json);
        Assert.Equal(
            reference,
            JsonSerializer.Deserialize<AdapterArtifactReference>(json));

        var hostileOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        hostileOptions.Converters.Add(new JsonStringEnumConverter());
        Assert.Equal(json, JsonSerializer.Serialize(reference, hostileOptions));
        Assert.Equal(
            reference,
            JsonSerializer.Deserialize<AdapterArtifactReference>(json, hostileOptions));
    }

    [Fact]
    public void AdapterArtifactReference_UnknownVersionAndKindFailClosedWithExactCodes()
    {
        AssertReferenceWireCode(
            "future version",
            "ADAPTER_REFERENCE_VERSION_UNSUPPORTED",
            CreateReferenceWire(schemaVersion: "\"schema_version\":2"));
        AssertReferenceWireCode(
            "future storage kind",
            "ADAPTER_REFERENCE_KIND_UNSUPPORTED",
            CreateReferenceWire(storageKind: "\"storage_kind\":2"));
        AssertReferenceWireCode(
            "version precedes kind",
            "ADAPTER_REFERENCE_VERSION_UNSUPPORTED",
            CreateReferenceWire(
                schemaVersion: "\"schema_version\":2",
                storageKind: "\"storage_kind\":2"));
        AssertReferenceWireCode(
            "version precedes malformed evidence",
            "ADAPTER_REFERENCE_VERSION_UNSUPPORTED",
            CreateReferenceWire(
                schemaVersion: "\"schema_version\":2",
                canonicalLeafName: "\"canonical_leaf_name\":\"bad\""));
        AssertReferenceWireCode(
            "kind precedes malformed evidence",
            "ADAPTER_REFERENCE_KIND_UNSUPPORTED",
            CreateReferenceWire(
                storageKind: "\"storage_kind\":2",
                canonicalLeafName: "\"canonical_leaf_name\":\"bad\""));
        AssertReferenceWireCode(
            "structural invalidity precedes unsupported semantics",
            "ADAPTER_REFERENCE_INVALID",
            CreateReferenceWire(
                schemaVersion: "\"schema_version\":2",
                sha256Hex: null));
    }

    [Fact]
    public void AdapterArtifactReference_JsonBoundaryRejectsEveryNonCanonicalWireForm()
    {
        var malformed = new (string Name, string Json)[]
        {
            ("top-level array", "[]"),
            ("top-level string", "\"reference\""),
            ("missing schema", CreateReferenceWire(schemaVersion: null)),
            ("missing kind", CreateReferenceWire(storageKind: null)),
            ("missing leaf", CreateReferenceWire(canonicalLeafName: null)),
            ("missing root", CreateReferenceWire(retainedRootIdentity: null)),
            ("missing artifact", CreateReferenceWire(finalArtifactIdentity: null)),
            ("missing length", CreateReferenceWire(length: null)),
            ("missing hash", CreateReferenceWire(sha256Hex: null)),
            ("duplicate schema", CreateReferenceWire(
                schemaVersion: "\"schema_version\":1,\"schema_version\":1")),
            ("duplicate kind", CreateReferenceWire(
                storageKind: "\"storage_kind\":1,\"storage_kind\":1")),
            ("duplicate leaf", CreateReferenceWire(
                canonicalLeafName: "\"canonical_leaf_name\":\"adapter-v2--d1-a--p1-b.lora\"," +
                                   "\"canonical_leaf_name\":\"adapter-v2--d1-a--p1-b.lora\"")),
            ("duplicate root", CreateReferenceWire(
                retainedRootIdentity: "\"retained_root_identity\":" + CreateIdentityWire() + "," +
                                      "\"retained_root_identity\":" + CreateIdentityWire())),
            ("duplicate artifact", CreateReferenceWire(
                finalArtifactIdentity: "\"final_artifact_identity\":" + CreateIdentityWire(21, 22, 23) + "," +
                                       "\"final_artifact_identity\":" + CreateIdentityWire(21, 22, 23))),
            ("duplicate length", CreateReferenceWire(length: "\"length\":42,\"length\":42")),
            ("duplicate hash", CreateReferenceWire(
                sha256Hex: "\"sha256_hex\":\"" + new string('a', 64) + "\"," +
                           "\"sha256_hex\":\"" + new string('a', 64) + "\"")),
            ("unknown top-level", CreateReferenceWire()![..^1] + ",\"unknown\":1}"),
            ("case-variant top-level", CreateReferenceWire(
                schemaVersion: "\"Schema_Version\":1")),
            ("string enum", CreateReferenceWire(
                storageKind: "\"storage_kind\":\"RetainedAdapterRoot\"")),
            ("null schema", CreateReferenceWire(schemaVersion: "\"schema_version\":null")),
            ("null kind", CreateReferenceWire(storageKind: "\"storage_kind\":null")),
            ("null leaf", CreateReferenceWire(canonicalLeafName: "\"canonical_leaf_name\":null")),
            ("null root", CreateReferenceWire(retainedRootIdentity: "\"retained_root_identity\":null")),
            ("null artifact", CreateReferenceWire(finalArtifactIdentity: "\"final_artifact_identity\":null")),
            ("null length", CreateReferenceWire(length: "\"length\":null")),
            ("null hash", CreateReferenceWire(sha256Hex: "\"sha256_hex\":null")),
            ("wrong schema type", CreateReferenceWire(schemaVersion: "\"schema_version\":\"1\"")),
            ("wrong kind type", CreateReferenceWire(storageKind: "\"storage_kind\":true")),
            ("wrong leaf type", CreateReferenceWire(canonicalLeafName: "\"canonical_leaf_name\":1")),
            ("wrong root type", CreateReferenceWire(retainedRootIdentity: "\"retained_root_identity\":[]")),
            ("wrong artifact type", CreateReferenceWire(finalArtifactIdentity: "\"final_artifact_identity\":\"id\"")),
            ("wrong length type", CreateReferenceWire(length: "\"length\":\"42\"")),
            ("wrong hash type", CreateReferenceWire(sha256Hex: "\"sha256_hex\":64")),
            ("schema overflow", CreateReferenceWire(schemaVersion: "\"schema_version\":2147483648")),
            ("kind overflow", CreateReferenceWire(storageKind: "\"storage_kind\":2147483648")),
            ("root identity overflow", CreateReferenceWire(
                retainedRootIdentity: "\"retained_root_identity\":" +
                                      CreateIdentityWire(
                                          volumeProperty: "\"volume_serial_number\":18446744073709551616"))),
            ("artifact identity overflow", CreateReferenceWire(
                finalArtifactIdentity: "\"final_artifact_identity\":" +
                                       CreateIdentityWire(
                                           21,
                                           22,
                                           23,
                                           lowProperty: "\"file_id_low\":18446744073709551616"))),
            ("length overflow", CreateReferenceWire(length: "\"length\":9223372036854775808")),
            ("missing nested root volume", CreateReferenceWire(
                retainedRootIdentity: "\"retained_root_identity\":" +
                                      CreateIdentityWire(volumeProperty: null))),
            ("missing nested root low", CreateReferenceWire(
                retainedRootIdentity: "\"retained_root_identity\":" +
                                      CreateIdentityWire(lowProperty: null))),
            ("missing nested root high", CreateReferenceWire(
                retainedRootIdentity: "\"retained_root_identity\":" +
                                      CreateIdentityWire(highProperty: null))),
            ("missing nested artifact volume", CreateReferenceWire(
                finalArtifactIdentity: "\"final_artifact_identity\":" +
                                       CreateIdentityWire(21, 22, 23, volumeProperty: null))),
            ("missing nested artifact low", CreateReferenceWire(
                finalArtifactIdentity: "\"final_artifact_identity\":" +
                                       CreateIdentityWire(21, 22, 23, lowProperty: null))),
            ("missing nested artifact high", CreateReferenceWire(
                finalArtifactIdentity: "\"final_artifact_identity\":" +
                                       CreateIdentityWire(21, 22, 23, highProperty: null))),
            ("duplicate nested root volume", CreateReferenceWire(
                retainedRootIdentity: "\"retained_root_identity\":" + CreateIdentityWire(
                    volumeProperty: "\"volume_serial_number\":11,\"volume_serial_number\":11"))),
            ("duplicate nested root low", CreateReferenceWire(
                retainedRootIdentity: "\"retained_root_identity\":" + CreateIdentityWire(
                    lowProperty: "\"file_id_low\":12,\"file_id_low\":12"))),
            ("duplicate nested root high", CreateReferenceWire(
                retainedRootIdentity: "\"retained_root_identity\":" + CreateIdentityWire(
                    highProperty: "\"file_id_high\":13,\"file_id_high\":13"))),
            ("duplicate nested artifact volume", CreateReferenceWire(
                finalArtifactIdentity: "\"final_artifact_identity\":" + CreateIdentityWire(
                    21, 22, 23,
                    volumeProperty: "\"volume_serial_number\":21,\"volume_serial_number\":21"))),
            ("duplicate nested artifact low", CreateReferenceWire(
                finalArtifactIdentity: "\"final_artifact_identity\":" + CreateIdentityWire(
                    21, 22, 23,
                    lowProperty: "\"file_id_low\":22,\"file_id_low\":22"))),
            ("duplicate nested artifact high", CreateReferenceWire(
                finalArtifactIdentity: "\"final_artifact_identity\":" + CreateIdentityWire(
                    21, 22, 23,
                    highProperty: "\"file_id_high\":23,\"file_id_high\":23"))),
            ("unknown nested root", CreateReferenceWire(
                retainedRootIdentity: "\"retained_root_identity\":" +
                                      CreateIdentityWire()[..^1] + ",\"unknown\":1}")),
            ("unknown nested artifact", CreateReferenceWire(
                finalArtifactIdentity: "\"final_artifact_identity\":" +
                                       CreateIdentityWire(21, 22, 23)[..^1] + ",\"unknown\":1}")),
            ("case-variant nested root", CreateReferenceWire(
                retainedRootIdentity: "\"retained_root_identity\":" + CreateIdentityWire(
                    volumeProperty: "\"Volume_Serial_Number\":11"))),
            ("case-variant nested artifact", CreateReferenceWire(
                finalArtifactIdentity: "\"final_artifact_identity\":" + CreateIdentityWire(
                    21, 22, 23,
                    highProperty: "\"File_Id_High\":23"))),
            ("null nested identity number", CreateReferenceWire(
                retainedRootIdentity: "\"retained_root_identity\":" + CreateIdentityWire(
                    lowProperty: "\"file_id_low\":null"))),
            ("wrong nested identity type", CreateReferenceWire(
                finalArtifactIdentity: "\"final_artifact_identity\":" + CreateIdentityWire(
                    21, 22, 23,
                    highProperty: "\"file_id_high\":\"23\""))),
            ("truncated JSON", CreateReferenceWire()[..^1]),
            ("trailing JSON value", CreateReferenceWire() + "{}"),
        };

        foreach (var malformedWire in malformed)
        {
            AssertReferenceWireCode(
                malformedWire.Name,
                "ADAPTER_REFERENCE_INVALID",
                malformedWire.Json);
        }
    }

    [Fact]
    public void AdapterArtifactReference_JsonBoundaryCannotBeBroadenedByCallerOptions()
    {
        var hostileOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        hostileOptions.Converters.Add(new JsonStringEnumConverter());

        AssertReferenceWireCode(
            "case-insensitive caller option",
            "ADAPTER_REFERENCE_INVALID",
            CreateReferenceWire(schemaVersion: "\"Schema_Version\":1"),
            hostileOptions);
        AssertReferenceWireCode(
            "string enum caller converter",
            "ADAPTER_REFERENCE_INVALID",
            CreateReferenceWire(storageKind: "\"storage_kind\":\"RetainedAdapterRoot\""),
            hostileOptions);
    }

    [ContinuousLearningAdapterWindowsFact]
    public void AdapterArtifactReference_MalformedJsonDoesNotRevokeRetainedAuthority()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability();
        var publication = capability.Publish(CreateAttachedModel(), CreateRequest());
        var valid = AdapterArtifactReference.FromPublication(publication);

        AssertReferenceWireCode(
            "whole null",
            "ADAPTER_REFERENCE_REQUIRED",
            "null");
        AssertReferenceWireCode(
            "unknown property before retained read",
            "ADAPTER_REFERENCE_INVALID",
            CreateReferenceWire()[..^1] + ",\"unknown\":1}");
        AssertReferenceWireCode(
            "malformed JSON before retained read",
            "ADAPTER_REFERENCE_INVALID",
            CreateReferenceWire()[..^1]);
        AssertReferenceWireCode(
            "trailing value before retained read",
            "ADAPTER_REFERENCE_INVALID",
            CreateReferenceWire() + "{}");

        Assert.Equal(publication, capability.ReadExact(valid).Publication);
        access.EnsureRetained();
    }

    [ContinuousLearningAdapterWindowsFact]
    public void ReadExactReference_MalformedCallerDataDoesNotRevokeThenValidReadSucceeds()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability();
        var publication = capability.Publish(CreateAttachedModel(), CreateRequest());
        var valid = AdapterArtifactReference.FromPublication(publication);
        var malformed = new[]
        {
            valid with { CanonicalLeafName = "../adapter.lora" },
            valid with { RetainedRootIdentity = default },
            valid with { FinalArtifactIdentity = default },
            valid with { Length = 0 },
            valid with
            {
                Length = ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes + 1L,
            },
            valid with { Sha256Hex = new string('A', 64) },
            valid with { Sha256Hex = "abc" },
        };

        AssertReferenceCode(
            "ADAPTER_REFERENCE_REQUIRED",
            () => capability.ReadExact(null!));
        AssertReferenceCode(
            "ADAPTER_REFERENCE_VERSION_UNSUPPORTED",
            () => capability.ReadExact(valid with { SchemaVersion = 2 }));
        AssertReferenceCode(
            "ADAPTER_REFERENCE_KIND_UNSUPPORTED",
            () => capability.ReadExact(valid with
            {
                StorageKind = (AdapterArtifactStorageKind)2,
            }));
        foreach (var invalid in malformed)
        {
            AssertReferenceCode(
                "ADAPTER_REFERENCE_INVALID",
                () => capability.ReadExact(invalid));
        }

        Assert.Equal(publication, capability.ReadExact(valid).Publication);
        access.EnsureRetained();
    }

    [ContinuousLearningAdapterWindowsFact]
    public void ReadExactReference_ForgedEvidencePreservesExistingFailClosedBehavior()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var foreignFixture = new FileAccessFixture(provisionAllRoots: true);
        ServerOwnedContinuousLearningAdapterPublication publication;
        AdapterArtifactReference foreignReference;
        using (var publishingAccess = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var publishingCapability = publishingAccess.AcquireAdapterRootCapability())
        {
            publication = publishingCapability.Publish(CreateAttachedModel(), CreateRequest());
        }
        using (var foreignAccess = foreignFixture.CreateStartup().AcquireRetainedFileAccess())
        using (var foreignCapability = foreignAccess.AcquireAdapterRootCapability())
        {
            foreignReference = AdapterArtifactReference.FromPublication(
                foreignCapability.Publish(CreateAttachedModel(), CreateRequest()));
        }
        var reference = AdapterArtifactReference.FromPublication(publication);

        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireAdapterRootCapability())
        {
            Assert.Throws<ArgumentException>(() => capability.ReadExact(foreignReference));
            Assert.Equal(publication, capability.ReadExact(reference).Publication);
            access.EnsureRetained();
        }

        AssertTrustFailure(reference with
        {
            FinalArtifactIdentity = new ServerOwnedContinuousLearningAdapterArtifactIdentity(
                reference.FinalArtifactIdentity.VolumeSerialNumber,
                reference.FinalArtifactIdentity.FileIdLow == ulong.MaxValue
                    ? reference.FinalArtifactIdentity.FileIdLow - 1
                    : reference.FinalArtifactIdentity.FileIdLow + 1,
                reference.FinalArtifactIdentity.FileIdHigh),
        });
        AssertTrustFailure(reference with { Length = reference.Length - 1 });
        AssertTrustFailure(reference with
        {
            Sha256Hex = new string(reference.Sha256Hex[0] == '0' ? '1' : '0', 64),
        });

        void AssertTrustFailure(AdapterArtifactReference forged)
        {
            using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
            using var capability = access.AcquireAdapterRootCapability();
            Assert.Throws<ContinuousLearningPathIdentityException>(() =>
                capability.ReadExact(forged));
            Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
        }
    }

    [ContinuousLearningAdapterWindowsFact]
    public void ReadExactReference_HonoursCancellationWithoutRevokingTrust()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var cancellation = new CancellationTokenSource();
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                ObserveIoStageForTesting = observation =>
                {
                    if (observation.Stage ==
                        ServerOwnedContinuousLearningAdapterIoStage.DuringExactRead)
                    {
                        cancellation.Cancel();
                    }
                },
            });
        var publication = capability.Publish(CreateAttachedModel(), CreateRequest());
        var reference = AdapterArtifactReference.FromPublication(publication);
        using var alreadyCancelled = new CancellationTokenSource();
        alreadyCancelled.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            capability.ReadExact(reference, alreadyCancelled.Token));
        Assert.Throws<OperationCanceledException>(() =>
            capability.ReadExact(reference, cancellation.Token));
        Assert.Equal(publication, capability.ReadExact(reference).Publication);
        access.EnsureRetained();
    }

    [ContinuousLearningAdapterWindowsFact]
    public void ReadExact_RejectsMalformedForeignAndWrongRootReceiptsWithoutConsumingValidAuthority()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var foreignFixture = new FileAccessFixture(provisionAllRoots: true);
        using var foreignAccess = foreignFixture.CreateStartup().AcquireRetainedFileAccess();
        using var foreignCapability = foreignAccess.AcquireAdapterRootCapability();
        var foreignPublication = foreignCapability.Publish(
            CreateAttachedModel(),
            CreateRequest());
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability();
        var publication = capability.Publish(CreateAttachedModel(), CreateRequest());

        Assert.Throws<ArgumentException>(() => capability.ReadExact(publication with
        {
            CanonicalLeafName = "../adapter.lora",
        }));
        Assert.Throws<ArgumentException>(() => capability.ReadExact(foreignPublication));
        var root = publication.RetainedRootIdentity;
        Assert.Throws<ArgumentException>(() => capability.ReadExact(publication with
        {
            RetainedRootIdentity = new ServerOwnedContinuousLearningAdapterRootIdentity(
                root.VolumeSerialNumber,
                root.FileIdLow == ulong.MaxValue ? root.FileIdLow - 1 : root.FileIdLow + 1,
                root.FileIdHigh),
        }));

        Assert.Equal(publication, capability.ReadExact(publication).Publication);
    }

    [ContinuousLearningAdapterWindowsFact]
    public void ReadExact_RejectsWrongArtifactIdentityLengthAndHashAndRevokesTrust()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        ServerOwnedContinuousLearningAdapterPublication publication;
        using (var publishingAccess = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var publishingCapability = publishingAccess.AcquireAdapterRootCapability())
        {
            publication = publishingCapability.Publish(CreateAttachedModel(), CreateRequest());
        }

        AssertTrustFailure(publication with
        {
            FinalArtifactIdentity = new ServerOwnedContinuousLearningAdapterArtifactIdentity(
                publication.FinalArtifactIdentity.VolumeSerialNumber,
                publication.FinalArtifactIdentity.FileIdLow == ulong.MaxValue
                    ? publication.FinalArtifactIdentity.FileIdLow - 1
                    : publication.FinalArtifactIdentity.FileIdLow + 1,
                publication.FinalArtifactIdentity.FileIdHigh),
        });
        AssertTrustFailure(publication with { Length = publication.Length - 1 });
        AssertTrustFailure(publication with
        {
            Sha256Hex = new string(publication.Sha256Hex[0] == '0' ? '1' : '0', 64),
        });

        void AssertTrustFailure(ServerOwnedContinuousLearningAdapterPublication forged)
        {
            using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
            using var capability = access.AcquireAdapterRootCapability();
            Assert.Throws<ContinuousLearningPathIdentityException>(() =>
                capability.ReadExact(forged));
            Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
        }
    }

    [ContinuousLearningAdapterWindowsFact]
    public void ReadExact_RejectsTruncationTamperMalformedAndFullCheckpointStructure()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        ServerOwnedContinuousLearningAdapterPublication publication;
        using (var publishingAccess = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var publishingCapability = publishingAccess.AcquireAdapterRootCapability())
        {
            publication = publishingCapability.Publish(CreateAttachedModel(), CreateRequest());
        }
        var finalPath = Path.Combine(fixture.AdapterRoot, publication.CanonicalLeafName);
        var original = File.ReadAllBytes(finalPath);

        File.WriteAllBytes(finalPath, original[..^1]);
        AssertTrustFailure(publication);

        File.WriteAllBytes(finalPath, original);
        var tampered = original.ToArray();
        tampered[^1] ^= 0x01;
        File.WriteAllBytes(finalPath, tampered);
        AssertTrustFailure(publication);

        var malformed = original.ToArray();
        malformed[0] ^= 0xff;
        File.WriteAllBytes(finalPath, malformed);
        AssertTrustFailure(publication with
        {
            Sha256Hex = Convert.ToHexString(SHA256.HashData(malformed)).ToLowerInvariant(),
        });

        var fullCheckpointSource = Path.Combine(
            fixture.AdapterRoot,
            ".full-checkpoint-structure-source.bin");
        byte[] fullCheckpoint;
        try
        {
            CreateAttachedModel().SaveCheckpoint(fullCheckpointSource);
            fullCheckpoint = File.ReadAllBytes(fullCheckpointSource);
        }
        finally
        {
            File.Delete(fullCheckpointSource);
        }
        Assert.False(File.Exists(fullCheckpointSource));

        using var structuralAccess = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var structuralCapability = structuralAccess.AcquireAdapterRootCapability();
        var identityBeforeOverwrite = DescribePublishedAdapterIdentity(
            structuralAccess,
            publication,
            out _);
        Assert.Equal(publication.FinalArtifactIdentity, identityBeforeOverwrite);
        using (var inPlace = new FileStream(
                   finalPath,
                   FileMode.Open,
                   FileAccess.Write,
                   FileShare.Read))
        {
            inPlace.Position = 0;
            inPlace.Write(fullCheckpoint);
            inPlace.SetLength(fullCheckpoint.LongLength);
            inPlace.Flush(flushToDisk: true);
        }

        var identityAfterOverwrite = DescribePublishedAdapterIdentity(
            structuralAccess,
            publication,
            out var fullCheckpointLength);
        Assert.Equal(identityBeforeOverwrite, identityAfterOverwrite);
        Assert.Equal(publication.FinalArtifactIdentity, identityAfterOverwrite);
        Assert.Equal(fullCheckpoint.LongLength, fullCheckpointLength);
        var fullCheckpointReceipt = publication with
        {
            Length = fullCheckpoint.LongLength,
            Sha256Hex = Convert.ToHexString(SHA256.HashData(fullCheckpoint)).ToLowerInvariant(),
        };

        var structuralException = Assert.Throws<AdapterPublicationTrustException>(() =>
            ServerOwnedContinuousLearningAdapterRootNative.ReadExact(
                GetAdapterRootHandle(structuralAccess),
                fullCheckpointReceipt,
                structuralCapability,
                () =>
                {
                    structuralAccess.EnsureRetained();
                    return true;
                },
                CancellationToken.None));
        Assert.Equal(
            "The published adapter content cannot be trusted for exact read.",
            structuralException.Message);
        Assert.NotNull(structuralException.InnerException);
        Assert.StartsWith(
            "LLM_CHECKPOINT_ARTIFACT_KIND_MISMATCH:",
            structuralException.InnerException!.Message);
        structuralAccess.EnsureRetained();

        Assert.Throws<ContinuousLearningPathIdentityException>(() =>
            structuralCapability.ReadExact(fullCheckpointReceipt));
        Assert.Throws<ContinuousLearningPathIdentityException>(structuralAccess.EnsureRetained);

        void AssertTrustFailure(ServerOwnedContinuousLearningAdapterPublication receipt)
        {
            using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
            using var capability = access.AcquireAdapterRootCapability();
            Assert.Throws<ContinuousLearningPathIdentityException>(() =>
                capability.ReadExact(receipt));
            Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
        }
    }

    [ContinuousLearningAdapterWindowsFact]
    public void ReadExact_RejectsDirectoryOrReparseAndValidFileSubstitution()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        ServerOwnedContinuousLearningAdapterPublication publication;
        ServerOwnedContinuousLearningAdapterPublication substitute;
        using (var publishingAccess = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var publishingCapability = publishingAccess.AcquireAdapterRootCapability())
        {
            publication = publishingCapability.Publish(CreateAttachedModel(), CreateRequest());
            substitute = publishingCapability.Publish(
                CreateAttachedModel(),
                ServerOwnedContinuousLearningAdapterPublicationRequest.Create(
                    "figma-html",
                    "release-002"));
        }
        var finalPath = Path.Combine(fixture.AdapterRoot, publication.CanonicalLeafName);
        var savedPath = finalPath + ".saved";
        File.Move(finalPath, savedPath);
        try
        {
            try
            {
                File.CreateSymbolicLink(finalPath, savedPath);
            }
            catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
            {
                Directory.CreateDirectory(finalPath);
            }
            AssertTrustFailure(publication);
        }
        finally
        {
            if (Directory.Exists(finalPath))
                Directory.Delete(finalPath);
            else if (File.Exists(finalPath))
                File.Delete(finalPath);
            File.Move(savedPath, finalPath);
        }

        File.Move(finalPath, savedPath);
        File.Copy(
            Path.Combine(fixture.AdapterRoot, substitute.CanonicalLeafName),
            finalPath);
        AssertTrustFailure(publication);

        void AssertTrustFailure(ServerOwnedContinuousLearningAdapterPublication receipt)
        {
            using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
            using var capability = access.AcquireAdapterRootCapability();
            Assert.Throws<ContinuousLearningPathIdentityException>(() =>
                capability.ReadExact(receipt));
            Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
        }
    }

    [ContinuousLearningAdapterWindowsFact]
    public void ReadExact_RootTrustLossRevokesCapabilityAndRetainedAccessSticky()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var rootHandle = GetAdapterRootHandle(access);
        using var capability = access.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                ObserveIoStageForTesting = observation =>
                {
                    if (observation.Stage ==
                        ServerOwnedContinuousLearningAdapterIoStage.AfterExactReadOpen)
                    {
                        rootHandle.Dispose();
                    }
                },
            });
        var publication = capability.Publish(CreateAttachedModel(), CreateRequest());

        Assert.Throws<ContinuousLearningPathIdentityException>(() =>
            capability.ReadExact(publication));
        Assert.Throws<ContinuousLearningPathIdentityException>(() =>
            capability.ReadExact(publication));
        Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
    }

    [ContinuousLearningAdapterWindowsFact]
    public void ReadExact_HonoursCancellationBeforeAndDuringReadWithoutRevokingTrust()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var cancellation = new CancellationTokenSource();
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                ObserveIoStageForTesting = observation =>
                {
                    if (observation.Stage ==
                        ServerOwnedContinuousLearningAdapterIoStage.DuringExactRead)
                    {
                        cancellation.Cancel();
                    }
                },
            });
        var publication = capability.Publish(CreateAttachedModel(), CreateRequest());
        using var alreadyCancelled = new CancellationTokenSource();
        alreadyCancelled.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            capability.ReadExact(publication, alreadyCancelled.Token));
        Assert.Throws<OperationCanceledException>(() =>
            capability.ReadExact(publication, cancellation.Token));
        Assert.Equal(publication, capability.ReadExact(publication).Publication);
        access.EnsureRetained();
    }

    [ContinuousLearningAdapterWindowsFact]
    public void ReadExact_EnforcesExactMaximumAndRejectsOneByteOverWithoutMutation()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        ServerOwnedContinuousLearningAdapterPublication publication;
        using (var publishingAccess = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var publishingCapability = publishingAccess.AcquireAdapterRootCapability())
        {
            publication = publishingCapability.Publish(CreateAttachedModel(), CreateRequest());
        }
        var finalPath = Path.Combine(fixture.AdapterRoot, publication.CanonicalLeafName);
        var original = File.ReadAllBytes(finalPath);

        using (var exactAccess = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var exactCapability = exactAccess.AcquireAdapterRootCapability(
                   new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
                   {
                       MaximumArtifactBytes = publication.Length,
                   }))
        {
            Assert.Equal(original, exactCapability.ReadExact(publication).CopyBytes());
        }

        using var boundedAccess = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var boundedCapability = boundedAccess.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                MaximumArtifactBytes = publication.Length - 1,
            });
        Assert.Throws<ArgumentException>(() => boundedCapability.ReadExact(publication));
        Assert.Equal(original, File.ReadAllBytes(finalPath));
        Assert.Empty(Directory.EnumerateFiles(fixture.AdapterRoot, "*.tmp"));
        boundedAccess.EnsureRetained();
    }

    [ContinuousLearningAdapterWindowsFact]
    public void Publish_DoesNotOverwriteExistingCanonicalLeaf()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability();
        var request = CreateRequest();
        var finalPath = Path.Combine(fixture.AdapterRoot, request.GetCanonicalFinalLeafName());
        var original = new byte[] { 0xA1, 0xB2, 0xC3, 0xD4 };
        File.WriteAllBytes(finalPath, original);

        Assert.Throws<IOException>(() => capability.Publish(CreateAttachedModel(), request));

        Assert.Equal(original, File.ReadAllBytes(finalPath));
        Assert.Empty(Directory.EnumerateFiles(fixture.AdapterRoot, "*.tmp"));
    }

    [ContinuousLearningAdapterWindowsFact]
    public void Capability_IsOneShotAndRetainedRootOwnershipExcludesSecondAccess()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var firstAccess = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var first = firstAccess.AcquireAdapterRootCapability();

        Assert.Throws<InvalidOperationException>(() => firstAccess.AcquireAdapterRootCapability());
        Assert.Throws<ContinuousLearningMutationAdmissionDomainAlreadyOwnedException>(
            () => fixture.CreateStartup().AcquireRetainedFileAccess());

        first.Dispose();
        firstAccess.Dispose();
        using var secondAccess = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var second = secondAccess.AcquireAdapterRootCapability();
        Assert.NotNull(second);
    }

    [ContinuousLearningAdapterWindowsFact]
    public void Capability_OwnerLockRejectsASeparateProcessUntilDisposed()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var capability = access.AcquireAdapterRootCapability();
        try
        {
            Assert.Equal(10, RunCrossProcessOwnerLockProbe(fixture.AdapterRoot));
        }
        finally
        {
            capability.Dispose();
        }

        Assert.Equal(0, RunCrossProcessOwnerLockProbe(fixture.AdapterRoot));
    }

    [ContinuousLearningAdapterWindowsFact]
    public void Publish_RejectsTemporaryPathSubstitutionWithoutDeletingForeignBytes()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        string? temporaryLeaf = null;
        var foreignBytes = new byte[] { 0xF0, 0x0D, 0xBA, 0xBE };
        using var capability = access.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                ObserveIoStageForTesting = observation =>
                {
                    var stage = observation.Stage;
                    var leaf = observation.CanonicalTemporaryLeafName;
                    if (stage != ServerOwnedContinuousLearningAdapterIoStage.AfterSerializationFlush ||
                        leaf is null ||
                        temporaryLeaf is not null)
                    {
                        return;
                    }

                    temporaryLeaf = leaf;
                    var temporaryPath = Path.Combine(fixture.AdapterRoot, leaf);
                    File.Move(temporaryPath, temporaryPath + ".foreign");
                    File.WriteAllBytes(temporaryPath, foreignBytes);
                },
            });

        Assert.Throws<ContinuousLearningPathIdentityException>(
            () => capability.Publish(CreateAttachedModel(), CreateRequest()));

        Assert.NotNull(temporaryLeaf);
        Assert.Equal(
            foreignBytes,
            File.ReadAllBytes(Path.Combine(fixture.AdapterRoot, temporaryLeaf!)));
        Assert.False(File.Exists(Path.Combine(
            fixture.AdapterRoot,
            CreateRequest().GetCanonicalFinalLeafName())));
        Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
    }

    [ContinuousLearningAdapterWindowsFact]
    public void Publish_RootHandleLossBeforeCommitIsStickyAndDoesNotCreateFinalLeaf()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var rootHandle = GetAdapterRootHandle(access);
        using var capability = access.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                ObserveIoStageForTesting = observation =>
                {
                    if (observation.Stage == ServerOwnedContinuousLearningAdapterIoStage.BeforeCommit)
                        rootHandle.Dispose();
                },
            });
        var request = CreateRequest();

        Assert.Throws<ContinuousLearningPathIdentityException>(
            () => capability.Publish(CreateAttachedModel(), request));

        Assert.False(File.Exists(Path.Combine(fixture.AdapterRoot, request.GetCanonicalFinalLeafName())));
        Assert.Throws<ContinuousLearningPathIdentityException>(() => capability.Publish(
            CreateAttachedModel(), request));
        Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
    }

    [ContinuousLearningAdapterWindowsFact]
    public void Publish_CancellationSizeCapAndSerializerFailureLeaveNoFinalOrTemporaryLeaf()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var request = CreateRequest();
        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireAdapterRootCapability())
        using (var cancelled = new CancellationTokenSource())
        {
            cancelled.Cancel();
            Assert.Throws<OperationCanceledException>(() => capability.Publish(
                CreateAttachedModel(), request, cancelled.Token));
        }
        Assert.Empty(Directory.EnumerateFiles(fixture.AdapterRoot, "*.tmp"));

        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireAdapterRootCapability(
                   new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
                   {
                       MaximumArtifactBytes = 1,
                   }))
        {
            Assert.ThrowsAny<Exception>(() => capability.Publish(CreateAttachedModel(), request));
        }
        Assert.Empty(Directory.EnumerateFiles(fixture.AdapterRoot, "*.tmp"));
        Assert.False(File.Exists(Path.Combine(fixture.AdapterRoot, request.GetCanonicalFinalLeafName())));

        using var failureAccess = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var failureCapability = failureAccess.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                SerializeAdapterForTesting = static (_, _) =>
                    throw new IOException("TEST_SERIALIZER_FAILURE"),
            });
        Assert.Throws<IOException>(() => failureCapability.Publish(CreateAttachedModel(), request));
        Assert.Empty(Directory.EnumerateFiles(fixture.AdapterRoot, "*.tmp"));
        Assert.False(File.Exists(Path.Combine(fixture.AdapterRoot, request.GetCanonicalFinalLeafName())));
    }

    [ContinuousLearningAdapterWindowsFact]
    public void Publish_HardWriteCapRejectsOverflowBeforeTheTemporaryLeafCanGrow()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var request = CreateRequest();
        var lengthImmediatelyBeforeOverflow = -1L;
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                MaximumArtifactBytes = 16,
                SerializeAdapterForTesting = (_, stream) =>
                {
                    stream.Write(new byte[16]);
                    Assert.Equal(16, stream.Length);
                    lengthImmediatelyBeforeOverflow = stream.Length;
                    Assert.Throws<InvalidDataException>(() => stream.WriteByte(0xFF));
                    throw new InvalidDataException("TEST_HARD_WRITE_CAP_CONFIRMED");
                },
            });

        Assert.Throws<InvalidDataException>(() =>
            capability.Publish(CreateAttachedModel(), request));

        Assert.Equal(16, lengthImmediatelyBeforeOverflow);
        Assert.Empty(Directory.EnumerateFiles(fixture.AdapterRoot, "*.tmp"));
        Assert.False(File.Exists(Path.Combine(fixture.AdapterRoot, request.GetCanonicalFinalLeafName())));
    }

    [ContinuousLearningAdapterWindowsFact]
    public void Publish_CancellationDuringSerializerWriteAbortsAndCleansOnlyItsTemporaryLeaf()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var cancellation = new CancellationTokenSource();
        var request = CreateRequest();
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                SerializeAdapterForTesting = (_, stream) =>
                {
                    stream.WriteByte(0x11);
                    cancellation.Cancel();
                    Assert.Throws<OperationCanceledException>(() => stream.WriteByte(0x22));
                    throw new OperationCanceledException("TEST_MID_WRITE_CANCELLATION_CONFIRMED");
                },
            });

        Assert.Throws<OperationCanceledException>(() =>
            capability.Publish(CreateAttachedModel(), request, cancellation.Token));

        Assert.Empty(Directory.EnumerateFiles(fixture.AdapterRoot, "*.tmp"));
        Assert.False(File.Exists(Path.Combine(fixture.AdapterRoot, request.GetCanonicalFinalLeafName())));
    }

    [ContinuousLearningAdapterWindowsFact]
    public void Publish_RejectsSameFilePayloadTamperAfterSerialization()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var model = CreateAttachedModel();
        FillSequential(model.Parameters());
        var request = CreateRequest();
        var changedFinitePayload = false;
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                ObserveIoStageForTesting = observation =>
                {
                    if (observation.Stage != ServerOwnedContinuousLearningAdapterIoStage.AfterSerializationFlush ||
                        observation.CanonicalTemporaryLeafName is null ||
                        changedFinitePayload)
                    {
                        return;
                    }

                    var temporaryPath = Path.Combine(
                        fixture.AdapterRoot,
                        observation.CanonicalTemporaryLeafName);
                    var bytes = File.ReadAllBytes(temporaryPath);
                    Assert.True(bytes.Length >= sizeof(float));
                    bytes[^1] ^= 0x01;
                    File.WriteAllBytes(temporaryPath, bytes);
                    using var altered = File.OpenRead(temporaryPath);
                    model.ValidateCurrentAdapterCheckpointArtifact(altered);
                    changedFinitePayload = true;
                },
            });

        Assert.Throws<ContinuousLearningPathIdentityException>(() =>
            capability.Publish(model, request));

        Assert.True(changedFinitePayload);
        Assert.Empty(Directory.EnumerateFiles(fixture.AdapterRoot, "*.tmp"));
        Assert.False(File.Exists(Path.Combine(fixture.AdapterRoot, request.GetCanonicalFinalLeafName())));
    }

    [ContinuousLearningAdapterWindowsFact]
    public void Publish_IgnoresCancellationAfterIrreversibleCommitAndReturnsDurableResult()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var cancellation = new CancellationTokenSource();
        var request = CreateRequest();
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                ObserveIoStageForTesting = observation =>
                {
                    if (observation.Stage == ServerOwnedContinuousLearningAdapterIoStage.AfterCommit)
                        cancellation.Cancel();
                },
            });

        var publication = capability.Publish(CreateAttachedModel(), request, cancellation.Token);

        Assert.True(cancellation.IsCancellationRequested);
        Assert.True(File.Exists(Path.Combine(fixture.AdapterRoot, publication.CanonicalLeafName)));
        Assert.Matches("^[0-9a-f]{64}$", publication.Sha256Hex);
    }

    [ContinuousLearningAdapterWindowsFact]
    public void Publish_DirectoryFlushFailureReportsTheCommittedArtifactWithoutClaimingSuccess()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var request = CreateRequest();
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                FailRootDirectoryFlushForTesting = true,
            });

        var exception = Assert.Throws<AdapterPublicationCommitIndeterminateException>(() =>
            capability.Publish(CreateAttachedModel(), request));

        Assert.Equal(request.GetCanonicalFinalLeafName(), exception.CommittedArtifact.CanonicalLeafName);
        Assert.NotEqual(default, exception.CommittedArtifact.FinalArtifactIdentity);
        Assert.Matches("^[0-9a-f]{64}$", exception.CommittedArtifact.Sha256Hex);
        Assert.True(File.Exists(Path.Combine(
            fixture.AdapterRoot,
            exception.CommittedArtifact.CanonicalLeafName)));
    }

    [ContinuousLearningAdapterWindowsFact]
    public void Publish_HoldsTheFinalArtifactWithoutWriteShareThroughDirectoryFlush()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var request = CreateRequest();
        var writeBlocked = false;
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                ObserveIoStageForTesting = observation =>
                {
                    if (observation.Stage != ServerOwnedContinuousLearningAdapterIoStage.BeforeRootDirectoryFlush)
                        return;
                    var finalPath = Path.Combine(fixture.AdapterRoot, request.GetCanonicalFinalLeafName());
                    Assert.Throws<IOException>(() => File.Open(
                        finalPath,
                        FileMode.Open,
                        FileAccess.Write,
                        FileShare.ReadWrite | FileShare.Delete));
                    writeBlocked = true;
                },
            });

        _ = capability.Publish(CreateAttachedModel(), request);

        Assert.True(writeBlocked);
    }

    [Theory]
    [InlineData("Domain", "release-1")]
    [InlineData("domain", "release/1")]
    [InlineData("domain", "../release")]
    [InlineData("domain", "release.")]
    public void Request_RejectsNonCanonicalOrPathLikeIdentifiers(
        string domain,
        string publicationId)
    {
        Assert.Throws<ArgumentException>(() =>
            ServerOwnedContinuousLearningAdapterPublicationRequest.Create(
                domain,
                publicationId));
    }

    [Fact]
    public void Request_LengthPrefixesMakePreviouslyCollidingPairsDistinctAndWindowsBounded()
    {
        var first = ServerOwnedContinuousLearningAdapterPublicationRequest.Create("a", "b--c");
        var second = ServerOwnedContinuousLearningAdapterPublicationRequest.Create("a--b", "c");

        Assert.NotEqual(first.GetCanonicalFinalLeafName(), second.GetCanonicalFinalLeafName());
        Assert.True(first.GetCanonicalFinalLeafName().Length <= 255);
        Assert.True(second.GetCanonicalFinalLeafName().Length <= 255);

        var longest = ServerOwnedContinuousLearningAdapterPublicationRequest.Create(
            new string('a', 48),
            new string('b', 48));
        Assert.True(longest.GetCanonicalFinalLeafName().Length <= 255);
        Assert.False(ServerOwnedContinuousLearningAdapterPublicationRequest
            .IsCanonicalFinalLeafName("adapter-v2--d01-a--p1-b.lora"));
        Assert.False(ServerOwnedContinuousLearningAdapterPublicationRequest
            .IsCanonicalFinalLeafName("adapter-v2--d1-a--p001-b.lora"));
    }

    [ContinuousLearningAdapterWindowsFact]
    public void Publish_OnlySerializesExistingAdaptersAndDoesNotMutateParameters()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability();
        var model = CreateAttachedModel();
        FillSequential(model.Parameters());
        var before = model.Parameters()
            .Select(parameter => parameter.Value.Data.ToArray())
            .ToArray();

        _ = capability.Publish(model, CreateRequest());

        var after = model.Parameters()
            .Select(parameter => parameter.Value.Data.ToArray())
            .ToArray();
        Assert.Equal(before.Length, after.Length);
        for (var index = 0; index < before.Length; index++)
            Assert.Equal(before[index], after[index]);
    }

    [ContinuousLearningAdapterWindowsFact]
    public void ReconcileCommitted_RetriesPostRenameFlushThroughTheSameCapability()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var model = CreateAttachedModel();
        FillSequential(model.Parameters());
        var reconciliationWriteBlocked = false;
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                FailRootDirectoryFlushForTesting = true,
                ObserveIoStageForTesting = observation =>
                {
                    if (observation.Stage !=
                        ServerOwnedContinuousLearningAdapterIoStage
                            .BeforeReconciliationRootDirectoryFlush)
                    {
                        return;
                    }

                    Assert.Throws<IOException>(() => File.Open(
                        Path.Combine(fixture.AdapterRoot, CreateRequest().GetCanonicalFinalLeafName()),
                        FileMode.Open,
                        FileAccess.Write,
                        FileShare.ReadWrite | FileShare.Delete));
                    reconciliationWriteBlocked = true;
                },
            });

        var indeterminate = Assert.Throws<AdapterPublicationCommitIndeterminateException>(() =>
            capability.Publish(model, CreateRequest()));
        var reconciled = capability.ReconcileCommitted(model, indeterminate.CommittedArtifact);

        Assert.Equal(indeterminate.CommittedArtifact, reconciled);
        Assert.True(reconciliationWriteBlocked);
        Assert.True(File.Exists(Path.Combine(fixture.AdapterRoot, reconciled.CanonicalLeafName)));
        Assert.Equal(
            reconciled.Sha256Hex,
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(
                fixture.AdapterRoot,
                reconciled.CanonicalLeafName)))).ToLowerInvariant());
    }

    [ContinuousLearningAdapterWindowsFact]
    public void ReconcileCommitted_RejectsTamperedFinalContentAndRevokesTheCapability()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var model = CreateAttachedModel();
        FillSequential(model.Parameters());
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability();
        var publication = capability.Publish(model, CreateRequest());
        var finalPath = Path.Combine(fixture.AdapterRoot, publication.CanonicalLeafName);
        var bytes = File.ReadAllBytes(finalPath);
        bytes[^1] ^= 0x01;
        File.WriteAllBytes(finalPath, bytes);
        using (var altered = File.OpenRead(finalPath))
            model.ValidateCurrentAdapterCheckpointArtifact(altered);

        Assert.Throws<ContinuousLearningPathIdentityException>(() =>
            capability.ReconcileCommitted(model, publication));
        Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
    }

    [ContinuousLearningAdapterWindowsFact]
    public void ReconcileCommitted_RejectsForeignArtifactIdentityAndRevokesTheCapability()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var model = CreateAttachedModel();
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability();
        var publication = capability.Publish(model, CreateRequest());
        var expected = publication.FinalArtifactIdentity;
        var foreignLow = expected.FileIdLow == ulong.MaxValue
            ? expected.FileIdLow - 1
            : expected.FileIdLow + 1;
        var foreignReceipt = publication with
        {
            FinalArtifactIdentity = new ServerOwnedContinuousLearningAdapterArtifactIdentity(
                expected.VolumeSerialNumber,
                foreignLow,
                expected.FileIdHigh),
        };

        Assert.Throws<ContinuousLearningPathIdentityException>(() =>
            capability.ReconcileCommitted(model, foreignReceipt));
        Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
    }

    [ContinuousLearningAdapterWindowsFact]
    public void ReconcileCommitted_RejectsWrongRootReceiptBeforeAnyRootLookup()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var model = CreateAttachedModel();
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability();
        var publication = capability.Publish(model, CreateRequest());
        var root = publication.RetainedRootIdentity;
        var wrongLow = root.FileIdLow == ulong.MaxValue ? root.FileIdLow - 1 : root.FileIdLow + 1;
        var wrongRootReceipt = publication with
        {
            RetainedRootIdentity = new ServerOwnedContinuousLearningAdapterRootIdentity(
                root.VolumeSerialNumber,
                wrongLow,
                root.FileIdHigh),
        };

        Assert.Throws<ArgumentException>(() => capability.ReconcileCommitted(model, wrongRootReceipt));
        Assert.Equal(publication, capability.ReconcileCommitted(model, publication));
    }

    [ContinuousLearningAdapterWindowsFact]
    public void ReconcileCommitted_RejectsNonCanonicalLeadingZeroReceiptBeforeAnyRootLookup()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var model = CreateAttachedModel();
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability();
        var publication = capability.Publish(model, CreateRequest());
        var nonCanonicalReceipt = publication with
        {
            CanonicalLeafName = "adapter-v2--d01-a--p001-b.lora",
        };

        Assert.Throws<ArgumentException>(() =>
            capability.ReconcileCommitted(model, nonCanonicalReceipt));
        Assert.Equal(publication, capability.ReconcileCommitted(model, publication));
    }

    [ContinuousLearningAdapterWindowsFact]
    public void ReconcileCommitted_HonoursCancellationBeforeOpeningTheFinalLeaf()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var model = CreateAttachedModel();
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability();
        var publication = capability.Publish(model, CreateRequest());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            capability.ReconcileCommitted(model, publication, cancellation.Token));
        Assert.Equal(publication, capability.ReconcileCommitted(model, publication));
    }

    [ContinuousLearningAdapterWindowsFact]
    public void Publish_DisposesTheHashingWriterOnSuccessAndSerializerFailure()
    {
        RequireWindows();
        var successDisposals = 0;
        using (var successFixture = new FileAccessFixture(provisionAllRoots: true))
        using (var successAccess = successFixture.CreateStartup().AcquireRetainedFileAccess())
        using (var successCapability = successAccess.AcquireAdapterRootCapability(
                   new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
                   {
                       BoundedWriterDisposedForTesting = () => successDisposals++,
                   }))
        {
            _ = successCapability.Publish(CreateAttachedModel(), CreateRequest());
        }
        Assert.Equal(1, successDisposals);

        var failureDisposals = 0;
        using (var failureFixture = new FileAccessFixture(provisionAllRoots: true))
        using (var failureAccess = failureFixture.CreateStartup().AcquireRetainedFileAccess())
        using (var failureCapability = failureAccess.AcquireAdapterRootCapability(
                   new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
                   {
                       BoundedWriterDisposedForTesting = () => failureDisposals++,
                       SerializeAdapterForTesting = static (_, _) =>
                           throw new IOException("TEST_SERIALIZER_FAILURE"),
                   }))
        {
            Assert.Throws<IOException>(() =>
                failureCapability.Publish(CreateAttachedModel(), CreateRequest()));
        }
        Assert.Equal(1, failureDisposals);
    }

    [ContinuousLearningAdapterWindowsFact]
    public void Publish_PublicationCancellationCancelsAnInFlightSerializerWriteAsync()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var cancellation = new CancellationTokenSource();
        CancellationAwareDelayedWriteStream? delayed = null;
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                WrapSerializerStreamForTesting = stream => delayed =
                    new CancellationAwareDelayedWriteStream(stream),
                SerializeAdapterForTesting = (_, stream) =>
                {
                    var write = stream.WriteAsync(
                        new ReadOnlyMemory<byte>([0x42]),
                        CancellationToken.None).AsTask();
                    Assert.NotNull(delayed);
                    Assert.True(delayed!.WriteStarted.Task.Wait(TimeSpan.FromSeconds(5)));
                    cancellation.Cancel();
                    Assert.True(SpinWait.SpinUntil(() => write.IsCompleted, TimeSpan.FromSeconds(5)),
                        "The outer publication cancellation did not reach the in-flight inner write.");
                    Assert.True(write.IsCanceled);
                    Assert.ThrowsAny<OperationCanceledException>(() => write.GetAwaiter().GetResult());
                    throw new OperationCanceledException(cancellation.Token);
                },
            });

        Assert.Throws<OperationCanceledException>(() =>
            capability.Publish(CreateAttachedModel(), CreateRequest(), cancellation.Token));
        Assert.NotNull(delayed);
        Assert.True(delayed!.ObservedCancellation.CanBeCanceled);
        Assert.True(delayed.WasCancelled);
        Assert.Empty(Directory.EnumerateFiles(fixture.AdapterRoot, "*.tmp"));
    }

    private static void RequireWindows()
    {
        if (OperatingSystem.IsWindows())
            return;

        // Это legacy Windows-oracle полоса: handle share-mode и delete-pending
        // проверяются на Windows, а WSL получает отдельные POSIX assertions.
        throw Xunit.Sdk.SkipException.ForSkip(
            "WINDOWS_ONLY_CAPABILITY_ORACLE: R1.3a AdapterRoot capability tests require Windows execution.");
    }

    private static void PrepareLinuxOwnerWriteBoundary(FileAccessFixture fixture)
    {
        if (!OperatingSystem.IsLinux())
            return;

        SetLinuxOwnerWritableDirectory(fixture.Root);
        SetLinuxOwnerWritableDirectory(fixture.AdapterRoot);
        SetLinuxOwnerWritableDirectory(fixture.CacheRoot);
        SetLinuxOwnerWritableDirectory(fixture.RegistryRoot);
        if (Directory.Exists(fixture.PairRoot))
            SetLinuxOwnerWritableDirectory(fixture.PairRoot);
        SetLinuxOwnerWritableFile(fixture.BaseCheckpointPath);
        SetLinuxOwnerWritableFile(fixture.TokenizerPath);
    }

    private static void ReplaceDirectoryWithFreshOwnerBoundary(string directory)
    {
        var moved = directory + ".replaced";
        Directory.Move(directory, moved);
        Directory.CreateDirectory(directory);
        SetLinuxOwnerWritableDirectory(directory);
    }

    private static void SetLinuxOwnerWritableDirectory(string path)
    {
        if (!OperatingSystem.IsLinux())
            return;

        File.SetUnixFileMode(
            path,
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.UserExecute);
    }

    private static void SetLinuxOwnerWritableFile(string path)
    {
        if (!OperatingSystem.IsLinux())
            return;

        File.SetUnixFileMode(
            path,
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite);
    }

    private static void SetLinuxGroupWritableFile(string path)
    {
        if (!OperatingSystem.IsLinux())
            return;

        // Тест ослабляет только mode-биты owner-lock, чтобы capability отвергла границу.
        File.SetUnixFileMode(
            path,
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.GroupWrite);
    }

    private static void CreateHardLinkPosixOrFail(
        string linkPath,
        string existingPath)
    {
        if (LinkPosix(existingPath, linkPath) == 0)
            return;

        throw new Xunit.Sdk.XunitException(
            $"POSIX hard-link setup failed with errno {Marshal.GetLastPInvokeError()}.");
    }

    private static ServerOwnedContinuousLearningAdapterPublicationRequest CreateRequest()
        => ServerOwnedContinuousLearningAdapterPublicationRequest.Create(
            "figma-html",
            "release-001");

    private static ServerOwnedContinuousLearningAdapterPublication CreateSyntheticPublication()
        => new(
            "adapter-v2--d1-a--p1-b.lora",
            new ServerOwnedContinuousLearningAdapterRootIdentity(11, 12, 13),
            new ServerOwnedContinuousLearningAdapterArtifactIdentity(21, 22, 23),
            42,
            new string('a', 64));

    private const string DefaultWireMember = "\0";

    private static string CreateReferenceWire(
        string? schemaVersion = DefaultWireMember,
        string? storageKind = DefaultWireMember,
        string? canonicalLeafName = DefaultWireMember,
        string? retainedRootIdentity = DefaultWireMember,
        string? finalArtifactIdentity = DefaultWireMember,
        string? length = DefaultWireMember,
        string? sha256Hex = DefaultWireMember)
    {
        schemaVersion = schemaVersion == DefaultWireMember
            ? "\"schema_version\":1"
            : schemaVersion;
        storageKind = storageKind == DefaultWireMember
            ? "\"storage_kind\":1"
            : storageKind;
        canonicalLeafName = canonicalLeafName == DefaultWireMember
            ? "\"canonical_leaf_name\":\"adapter-v2--d1-a--p1-b.lora\""
            : canonicalLeafName;
        retainedRootIdentity = retainedRootIdentity == DefaultWireMember
            ? "\"retained_root_identity\":" + CreateIdentityWire()
            : retainedRootIdentity;
        finalArtifactIdentity = finalArtifactIdentity == DefaultWireMember
            ? "\"final_artifact_identity\":" + CreateIdentityWire(21, 22, 23)
            : finalArtifactIdentity;
        length = length == DefaultWireMember
            ? "\"length\":42"
            : length;
        sha256Hex = sha256Hex == DefaultWireMember
            ? "\"sha256_hex\":\"" + new string('a', 64) + "\""
            : sha256Hex;

        return "{" + string.Join(
            ",",
            new[]
            {
                schemaVersion,
                storageKind,
                canonicalLeafName,
                retainedRootIdentity,
                finalArtifactIdentity,
                length,
                sha256Hex,
            }.Where(member => member is not null)) + "}";
    }

    private static string CreateIdentityWire(
        ulong volumeSerialNumber = 11,
        ulong fileIdLow = 12,
        ulong fileIdHigh = 13,
        string? volumeProperty = DefaultWireMember,
        string? lowProperty = DefaultWireMember,
        string? highProperty = DefaultWireMember)
    {
        volumeProperty = volumeProperty == DefaultWireMember
            ? $"\"volume_serial_number\":{volumeSerialNumber}"
            : volumeProperty;
        lowProperty = lowProperty == DefaultWireMember
            ? $"\"file_id_low\":{fileIdLow}"
            : lowProperty;
        highProperty = highProperty == DefaultWireMember
            ? $"\"file_id_high\":{fileIdHigh}"
            : highProperty;

        return "{" + string.Join(
            ",",
            new[]
            {
                volumeProperty,
                lowProperty,
                highProperty,
            }.Where(member => member is not null)) + "}";
    }

    private static void AssertReferenceWireCode(
        string caseName,
        string expectedCode,
        string json,
        JsonSerializerOptions? options = null)
    {
        var exception = Record.Exception(() =>
            JsonSerializer.Deserialize<AdapterArtifactReference>(json, options));
        if (exception is null)
        {
            throw new Xunit.Sdk.XunitException(
                $"{caseName}: deserialization unexpectedly succeeded.");
        }
        if (exception.GetType() != typeof(AdapterArtifactReferenceValidationException))
        {
            throw new Xunit.Sdk.XunitException(
                $"{caseName}: leaked {exception.GetType().FullName} instead of the owned validation taxonomy.");
        }

        var validation = (AdapterArtifactReferenceValidationException)exception;
        Assert.Equal(expectedCode, validation.Code);
        Assert.Equal(expectedCode, validation.Message);
    }

    private static void AssertReferenceCode(string expectedCode, Action action)
    {
        var exception = Assert.Throws<AdapterArtifactReferenceValidationException>(action);
        Assert.Equal(expectedCode, exception.Code);
        Assert.Equal(expectedCode, exception.Message);
    }

    private static LlmModel CreateAttachedModel()
    {
        var model = new LlmModel(new ModelConfig
        {
            VocabSize = 16,
            ContextLen = 4,
            HiddenDim = 4,
            NumLayers = 1,
            NumHeads = 1,
            NumKvHeads = 1,
            FfnHiddenDim = 8,
            RopeTheta = 10_000f,
            RmsNormEps = 1e-5f,
        });
        _ = LoraApplicator.Apply(
            model,
            new LoraConfig
            {
                Rank = 2,
                Alpha = 4,
                Dropout = 0,
                TargetModules = ["q_proj", "v_proj"],
            });
        return model;
    }

    private static void FillSequential(IEnumerable<Parameter> parameters)
    {
        var value = 1f;
        foreach (var parameter in parameters)
        {
            for (var index = 0; index < parameter.Value.Data.Length; index++)
                parameter.Value.Data[index] = value++;
        }
    }

    private static Microsoft.Win32.SafeHandles.SafeFileHandle GetAdapterRootHandle(
        ServerOwnedContinuousLearningFileAccess access)
    {
        var entries = Assert.IsAssignableFrom<Array>(
            typeof(ServerOwnedContinuousLearningFileAccess)
                .GetField("_entries", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(access));
        foreach (var entry in entries)
        {
            var role = Assert.IsType<ContinuousLearningRetainedObjectRole>(
                entry!.GetType().GetProperty("Role")!.GetValue(entry));
            if (role == ContinuousLearningRetainedObjectRole.AdapterRoot)
            {
                return Assert.IsType<Microsoft.Win32.SafeHandles.SafeFileHandle>(
                    entry.GetType().GetProperty("Handle")!.GetValue(entry));
            }
        }
        throw new Xunit.Sdk.XunitException("AdapterRoot handle was not retained.");
    }

    private static ServerOwnedContinuousLearningAdapterArtifactIdentity
        DescribePublishedAdapterIdentity(
            ServerOwnedContinuousLearningFileAccess access,
            ServerOwnedContinuousLearningAdapterPublication publication,
            out long length)
    {
        using var handle = ServerOwnedContinuousLearningCacheRootNative.OpenNamedRegularLeaf(
            GetAdapterRootHandle(access),
            publication.CanonicalLeafName,
            mutationAccess: false,
            ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes);
        var identity = ServerOwnedContinuousLearningCacheRootNative.DescribeNamedRegularLeaf(
            handle,
            ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes,
            out length);
        return new ServerOwnedContinuousLearningAdapterArtifactIdentity(
            identity.VolumeSerialNumber,
            identity.FileIdLow,
            identity.FileIdHigh);
    }

    private static int RunCrossProcessOwnerLockProbe(string adapterRoot)
    {
        var helperPath = ResolveRegistryLockHolderPath();
        var dotnetHost = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH")
                         ?? Environment.ProcessPath;
        Assert.False(string.IsNullOrWhiteSpace(dotnetHost));
        Assert.True(File.Exists(dotnetHost), "The dotnet host is unavailable.");
        Assert.True(File.Exists(helperPath), "The owner-lock probe helper is unavailable.");

        using var child = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = dotnetHost,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
            },
        };
        child.StartInfo.ArgumentList.Add(helperPath);
        child.StartInfo.ArgumentList.Add("--probe-exclusive-owner");
        child.StartInfo.ArgumentList.Add(adapterRoot);
        Assert.True(child.Start());
        Assert.True(child.WaitForExit(10_000), "Owner-lock probe timed out.");
        var stderr = child.StandardError.ReadToEnd();
        Assert.True(
            string.IsNullOrWhiteSpace(stderr),
            $"Owner-lock probe reported stderr: {stderr}");
        return child.ExitCode;
    }

    private static string ResolveRegistryLockHolderPath()
    {
        var frameworkDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        var configurationDirectory = frameworkDirectory.Parent
            ?? throw new InvalidOperationException("Test configuration directory is unavailable.");
        var repositoryRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            ".."));
        return Path.GetFullPath(Path.Combine(
            repositoryRoot,
            "Tests.Tools",
            "RegistryLockHolder",
            "bin",
            configurationDirectory.Name,
            frameworkDirectory.Name,
            "RegistryLockHolder.dll"));
    }

    [DllImport("libc", EntryPoint = "link", SetLastError = true)]
    private static extern int LinkPosix(
        string existingPath,
        string linkPath);

    private sealed class CancellationAwareDelayedWriteStream : Stream
    {
        private readonly Stream _inner;

        internal CancellationAwareDelayedWriteStream(Stream inner)
        {
            _inner = inner;
        }

        internal TaskCompletionSource<bool> WriteStarted { get; } = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        internal CancellationToken ObservedCancellation { get; private set; }

        internal bool WasCancelled { get; private set; }

        public override bool CanRead => false;

        public override bool CanSeek => _inner.CanSeek;

        public override bool CanWrite => _inner.CanWrite;

        public override long Length => _inner.Length;

        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public override void Flush()
            => _inner.Flush();

        public override Task FlushAsync(CancellationToken cancellationToken)
            => DelayUntilCancellationAsync(cancellationToken);

        public override int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin)
            => _inner.Seek(offset, origin);

        public override void SetLength(long value)
            => _inner.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
            => _inner.Write(buffer, offset, count);

        public override void Write(ReadOnlySpan<byte> buffer)
            => _inner.Write(buffer);

        public override void WriteByte(byte value)
            => _inner.WriteByte(value);

        public override Task WriteAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
            => DelayUntilCancellationAsync(cancellationToken);

        public override ValueTask WriteAsync(
            ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default)
            => new(DelayUntilCancellationAsync(cancellationToken));

        private async Task DelayUntilCancellationAsync(CancellationToken cancellationToken)
        {
            ObservedCancellation = cancellationToken;
            WriteStarted.TrySetResult(true);
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                WasCancelled = true;
                throw;
            }
        }
    }
}

file sealed class ContinuousLearningAdapterCapabilityLinuxX64FactAttribute : Xunit.FactAttribute
{
    public ContinuousLearningAdapterCapabilityLinuxX64FactAttribute()
    {
        if (!OperatingSystem.IsLinux())
        {
            Skip = "LINUX_ONLY_CAPABILITY_ORACLE: test requires Linux.";
            return;
        }

        if (RuntimeInformation.ProcessArchitecture != Architecture.X64)
            Skip = "LINUX_X64_ONLY_CAPABILITY_ORACLE: test requires Linux x64.";
    }
}

file sealed class ContinuousLearningAdapterWindowsFactAttribute : Xunit.FactAttribute
{
    public ContinuousLearningAdapterWindowsFactAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            // Discovery-time skip сохраняет Windows oracle в WSL как
            // NotExecuted; dynamic SkipException этот runner считает failure.
            Skip = "WINDOWS_ONLY_CAPABILITY_ORACLE: R1.3a AdapterRoot capability tests require Windows execution.";
        }
    }
}

file sealed class ContinuousLearningAdapterWindowsTheoryAttribute : Xunit.TheoryAttribute
{
    public ContinuousLearningAdapterWindowsTheoryAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            // Теории старой полосы остаются Windows-only; Linux ветка имеет
            // отдельные public API checks без ослабления Windows assertions.
            Skip = "WINDOWS_ONLY_CAPABILITY_ORACLE: R1.3a AdapterRoot capability tests require Windows execution.";
        }
    }
}
