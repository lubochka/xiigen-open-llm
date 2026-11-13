// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32.SafeHandles;
using ModelServer;
using ModelServer.DesignConversion;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

[Collection(ModelServerContinuousLearningEnvironmentCollection.Name)]
public sealed partial class ContinuousLearningModelServerReadinessEndpointTests
{
    [Fact]
    public void Builder_RegistersExactStartupOwnedAuthorityAsOnlyTranslationLeaseBroker()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyClosedEnvironment(invalid: false);

        var builder = Program.CreateLocalLoopbackBuilder([]);
        var startup = Assert.IsType<ContinuousLearningModelServerStartup>(
            Assert.Single(
                    builder.Services,
                    descriptor => descriptor.ServiceType ==
                                  typeof(ContinuousLearningModelServerStartup))
                .ImplementationInstance);
        var authority = Assert.IsType<
            ServerOwnedLegalTrainingTeacherAdmissionAuthority>(
                Assert.Single(
                    builder.Services,
                    descriptor => descriptor.ServiceType == typeof(
                        ServerOwnedLegalTrainingTeacherAdmissionAuthority))
                    .ImplementationInstance);
        var broker = Assert.IsAssignableFrom<
            IServerOwnedDpoTrainingLeaseBroker>(
                Assert.Single(
                    builder.Services,
                    descriptor => descriptor.ServiceType == typeof(
                        IServerOwnedDpoTrainingLeaseBroker))
                    .ImplementationInstance);

        Assert.Same(startup.LegalTeacherAdmissionAuthority, authority);
        Assert.Same(authority, broker);
        using var app = builder.Build();
        Assert.Same(
            authority,
            app.Services.GetRequiredService<
                IServerOwnedDpoTrainingLeaseBroker>());
        Assert.Single(app.Services.GetServices<
            IServerOwnedDpoTrainingLeaseBroker>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DisabledOrInvalidBuilder_RegistersNoLeaseActivatorAndTouchesNoPath(
        bool invalid)
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyClosedEnvironment(invalid);
        var before = fixture.Snapshot();

        var builder = Program.CreateLocalLoopbackBuilder([]);
        var startup = Assert.Single(
            builder.Services,
            descriptor => descriptor.ServiceType ==
                typeof(ContinuousLearningModelServerStartup))
            .ImplementationInstance as ContinuousLearningModelServerStartup;

        Assert.NotNull(startup);
        Assert.False(startup!.Readiness.ConfigurationAdmitted);
        Assert.DoesNotContain(
            builder.Services,
            descriptor => descriptor.ServiceType ==
                              typeof(ServerOwnedContinuousLearningFileAccess)
                          || descriptor.ImplementationType ==
                              typeof(ContinuousLearningRetainedObjectHostedActivator)
                          || descriptor.ServiceType == typeof(
                              DesignConversionRetainedDpoPolicyBaseProviderGateway));
        Assert.Equal(before, fixture.Snapshot());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DisabledOrInvalidProductionHost_InjectsNoDefaultPolicyBaseProvider(
        bool invalid)
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyClosedEnvironment(invalid);
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("closed"));
        var artifactDb = ReadTrainingIntakeArtifactDbFromUnstartedHost(app);

        Assert.Null(ReadDpoPolicyBaseProvider(artifactDb));
        Assert.Empty(app.Services.GetServices<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>());
    }

    [Fact]
    public async Task OverBudgetTokenizer_NeverPublishesPositiveContinuousLearningReadiness()
    {
        using var fixture = new ModelServerEndpointFixture();
        using (var tokenizer = new FileStream(
                   fixture.Tokenizer,
                   FileMode.Open,
                   FileAccess.Write,
                   FileShare.Read))
        {
            tokenizer.SetLength(BpeTokenizer.MaximumCanonicalArtifactBytes + 1);
        }
        var expectedTokenizerLength = new FileInfo(fixture.Tokenizer).Length;
        using var environment = fixture.ApplyEnvironment();
        var builder = Program.CreateLocalLoopbackBuilder([]);
        var startup = Assert.IsType<ContinuousLearningModelServerStartup>(
            Assert.Single(
                    builder.Services,
                    descriptor => descriptor.ServiceType ==
                                  typeof(ContinuousLearningModelServerStartup))
                .ImplementationInstance);

        Assert.False(startup.HasDeferredCompositionPlan);
        Assert.False(startup.Readiness.ConfigurationAdmitted);
        Assert.DoesNotContain(
            builder.Services,
            descriptor => descriptor.ServiceType ==
                              typeof(ServerOwnedContinuousLearningFileAccess)
                          || descriptor.ImplementationType ==
                              typeof(ContinuousLearningRetainedObjectHostedActivator)
                          || descriptor.ServiceType == typeof(
                              DesignConversionRetainedDpoPolicyBaseProviderGateway));

        var app = builder.Build();
        Program.ConfigureLocalLoopbackRoutes(app);
        try
        {
            await app.StartAsync();
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            using var response = await client.GetAsync(
                ContinuousLearningReadinessEndpoint.EndpointPath);
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var readiness = document.RootElement;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(readiness.GetProperty("configuration_admitted").GetBoolean());
            Assert.False(readiness.GetProperty("composition_materialized").GetBoolean());
            Assert.False(readiness.GetProperty("path_identity_verified").GetBoolean());
            Assert.False(readiness.GetProperty("retained_object_identity_bound").GetBoolean());
            Assert.False(readiness.GetProperty("stable_dpo_executor_configured").GetBoolean());
            Assert.False(readiness.GetProperty("product_ready").GetBoolean());
            Assert.Equal("invalid", readiness.GetProperty("configuration_status").GetString());
            Assert.Equal(expectedTokenizerLength, new FileInfo(fixture.Tokenizer).Length);
        }
        finally
        {
            await app.DisposeAsync();
        }

        fixture.AssertLeasedPathsRenameable();
    }

    [Fact]
    public async Task MissingRequiredRoot_PreventsHostReadinessWithStableRedactedFailure()
    {
        using var fixture = new ModelServerEndpointFixture();
        Directory.Delete(fixture.PairRoot);
        using var environment = fixture.ApplyEnvironment();
        var builder = Program.CreateLocalLoopbackBuilder([]);
        var startup = Assert.IsType<ContinuousLearningModelServerStartup>(
            Assert.Single(
                    builder.Services,
                    descriptor => descriptor.ServiceType ==
                                  typeof(ContinuousLearningModelServerStartup))
                .ImplementationInstance);
        var accessFactoryTarget = GetAccessFactoryTarget(builder.Services, startup);
        var app = builder.Build();

        try
        {
            var exception = await Assert.ThrowsAsync<
                ContinuousLearningPathIdentityException>(
                () => app.StartAsync());
            var json = JsonSerializer.Serialize(startup.Readiness);

            Assert.Equal(
                ContinuousLearningPathIdentityException.StableErrorCode,
                exception.Message);
            Assert.False(startup.Readiness.PathIdentityVerified);
            Assert.Equal(
                ContinuousLearningPathIdentityException.StableErrorCode,
                startup.Readiness.ConfigurationErrorCode);
            Assert.DoesNotContain(
                fixture.Root,
                json,
                StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(fixture.ModelId, json, StringComparison.Ordinal);
            Assert.DoesNotContain(fixture.OllamaManifestDigest, json, StringComparison.Ordinal);
            Assert.DoesNotContain(fixture.AuditSalt, json, StringComparison.Ordinal);
            Assert.False(startup.Readiness.RetainedObjectIdentityBound);
            AssertExactConfiguredPathsNotReachable(
                [startup, accessFactoryTarget],
                fixture.ConfiguredPaths);
        }
        finally
        {
            await app.DisposeAsync();
        }

        fixture.AssertLeasedPathsRenameable();
    }

    [Fact]
    public async Task RealModelServerBuilder_RegistersOneDeferredStartupAndExposesGetOnlyRedactedReadiness()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment();
        var before = fixture.Snapshot();
        var builder = Program.CreateLocalLoopbackBuilder([]);
        var registeredStartup = Assert.IsType<ContinuousLearningModelServerStartup>(
            Assert.Single(
                    builder.Services,
                    descriptor => descriptor.ServiceType ==
                                  typeof(ContinuousLearningModelServerStartup))
                .ImplementationInstance);
        var accessFactoryTarget = GetAccessFactoryTarget(
            builder.Services,
            registeredStartup);
        var registeredGateway = Assert.IsType<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>(
                Assert.Single(
                    builder.Services,
                    descriptor => descriptor.ServiceType == typeof(
                        DesignConversionRetainedDpoPolicyBaseProviderGateway))
                    .ImplementationInstance);
        Assert.False(registeredGateway.IsBound);
        var app = builder.Build();
        Program.ConfigureLocalLoopbackRoutes(app);

        try
        {
            var startup = app.Services
                .GetRequiredService<ContinuousLearningModelServerStartup>();
            Assert.Same(registeredStartup, startup);
            Assert.Same(
                startup,
                app.Services.GetRequiredService<ContinuousLearningModelServerStartup>());
            Assert.Single(app.Services.GetServices<ContinuousLearningModelServerStartup>());
            Assert.True(startup.HasDeferredCompositionPlan);
            Assert.False(startup.Readiness.PathIdentityVerified);
            Assert.Equal(before, fixture.Snapshot());

            await app.StartAsync();
            var access = Assert.Single(
                app.Services.GetServices<ServerOwnedContinuousLearningFileAccess>());
            var activator = Assert.Single(
                app.Services.GetServices<IHostedService>(),
                service => service is ContinuousLearningRetainedObjectHostedActivator);
            AssertExactConfiguredPathsNotReachable(
                [startup, access, activator, accessFactoryTarget],
                fixture.ConfiguredPaths);
            AssertOnlyOwnerLockLeavesAdded(before, fixture.Snapshot());
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            using var response = await client.GetAsync(
                ContinuousLearningReadinessEndpoint.EndpointPath);
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            Assert.True(root.GetProperty("configuration_admitted").GetBoolean());
            Assert.True(root
                .GetProperty("ollama_manifest_digest_configured")
                .GetBoolean());
            Assert.True(root
                .GetProperty("ollama_model_layer_digest_configured")
                .GetBoolean());
            Assert.True(root.GetProperty("composition_materialized").GetBoolean());
            Assert.True(root.GetProperty("path_identity_verified").GetBoolean());
            Assert.Equal(
                "materialized_inactive",
                root.GetProperty("configuration_status").GetString());
            Assert.Equal("none", root.GetProperty("configuration_error_code").GetString());
            Assert.True(root
                .GetProperty("retained_object_identity_bound")
                .GetBoolean());
            Assert.True(root.GetProperty("stable_dpo_executor_configured").GetBoolean());
            Assert.False(root.GetProperty("training_enabled").GetBoolean());
            Assert.False(root.GetProperty("cycle_execution_available").GetBoolean());
            Assert.False(root.GetProperty("performs_training").GetBoolean());
            Assert.False(root.GetProperty("product_ready").GetBoolean());
            Assert.DoesNotContain(fixture.Root, json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(fixture.AuditSalt, json, StringComparison.Ordinal);
            Assert.DoesNotContain(fixture.ModelId, json, StringComparison.Ordinal);
            Assert.DoesNotContain(fixture.OllamaManifestDigest, json, StringComparison.Ordinal);
            Assert.DoesNotContain(new string('c', 64), json, StringComparison.Ordinal);
            Assert.DoesNotContain("request_id", json, StringComparison.OrdinalIgnoreCase);

            using var post = await client.PostAsync(
                ContinuousLearningReadinessEndpoint.EndpointPath,
                new StringContent("{\"model_id\":\"caller-controlled\"}"));
            Assert.Equal(HttpStatusCode.MethodNotAllowed, post.StatusCode);
        }
        finally
        {
            await app.DisposeAsync();
        }

        fixture.AssertLeasedPathsRenameable();
    }

    [Fact]
    public void OwnerLockSnapshotAssertion_RejectsMissingModifiedOrUnexpectedLeaves()
    {
        string[] before =
        [
            "directory:adapter",
            "directory:registry",
            "file:base.bin:ABCDEF",
        ];
        var expectedOwnerLocks = ModelServerEndpointFixture
            .ExpectedOwnerLockSnapshotEntries();

        AssertOnlyOwnerLockLeavesAdded(
            before,
            before.Concat(expectedOwnerLocks).ToArray());
        Assert.IsAssignableFrom<Xunit.Sdk.XunitException>(
            Record.Exception(() => AssertOnlyOwnerLockLeavesAdded(
                before,
                before.Concat(expectedOwnerLocks.Skip(1)).ToArray())));
        Assert.IsAssignableFrom<Xunit.Sdk.XunitException>(
            Record.Exception(() => AssertOnlyOwnerLockLeavesAdded(
                before,
                before.Concat(expectedOwnerLocks)
                    .Append("file:unexpected.bin:012345")
                    .ToArray())));
        Assert.IsAssignableFrom<Xunit.Sdk.XunitException>(
            Record.Exception(() => AssertOnlyOwnerLockLeavesAdded(
                before,
                before.Where(static entry =>
                        !entry.StartsWith("file:base.bin:", StringComparison.Ordinal))
                    .Append("file:base.bin:CHANGED")
                    .Concat(expectedOwnerLocks)
                    .ToArray())));
        Assert.IsAssignableFrom<Xunit.Sdk.XunitException>(
            Record.Exception(() => AssertOnlyOwnerLockLeavesAdded(
                before,
                before.Where(static entry => entry != "directory:registry")
                    .Concat(expectedOwnerLocks)
                    .ToArray())));
        Assert.IsAssignableFrom<Xunit.Sdk.XunitException>(
            Record.Exception(() => AssertOnlyOwnerLockLeavesAdded(
                before,
                before.Concat(expectedOwnerLocks.Skip(1))
                    .Append($"owner-lock:{Path.Combine(
                        "adapter",
                        ".unexpected-owner.lock")}")
                    .ToArray())));
    }

    [Fact]
    public async Task ProductionHost_InjectsUnboundGatewayThenBindsExactRetainedProviderWithoutRunningItAtStartup()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment();
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("retained-default"));
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var artifactDb = ReadTrainingIntakeArtifactDbFromUnstartedHost(app);
        var authority = app.Services.GetRequiredService<
            ServerOwnedLegalTrainingTeacherAdmissionAuthority>();

        Assert.Same(gateway, ReadDpoPolicyBaseProvider(artifactDb));
        Assert.Same(authority, ReadLegalTeacherAdmissionAuthority(artifactDb));
        Assert.False(gateway.IsBound);
        Assert.True(authority.IsAvailable);
        Assert.False(startup.IsLegalTrainingTeacherAdmissionActive());
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            authority.ValidateBatch(
                [TrainingTeacherProvenance.Deterministic("pre-host-denied")])
                .ErrorCode);
        Assert.Null(
            DesignConversionDpoTeacherAttestationAssociation.Resolve(
                artifactDb));
        AssertStableReadinessRequiresBoundGateway(startup, gateway);

        await app.StartAsync();

        Assert.True(gateway.IsBound);
        var exactTeacherRuntime = Assert.IsType<
            DesignConversionDpoTeacherAttestationRuntime>(
                DesignConversionDpoTeacherAttestationAssociation.Resolve(
                    artifactDb));
        Assert.Same(
            exactTeacherRuntime,
            DesignConversionDpoTeacherAttestationAssociation.Resolve(
                artifactDb));
        Assert.False(startup.IsLegalTrainingTeacherAdmissionActive());
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            authority.ValidateBatch(
                [TrainingTeacherProvenance.Deterministic("default-off-denied")])
                .ErrorCode);
        AssertStableReadinessRequiresBoundGateway(startup, gateway);
        var modelSetResult = await gateway.CreateModelSetAsync(
            CancellationToken.None);
        Assert.True(modelSetResult.IsSuccess, modelSetResult.ErrorMessage);
        var modelSet = Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(
            modelSetResult.Value);
        Assert.Equal(fixture.BaseModelId, modelSet.Identity.BaseModelId);
        Assert.Equal(fixture.BaseModelRevision, modelSet.Identity.BaseRevision);
        Assert.Equal(fixture.ModelConfiguration, modelSet.Policy.Config);

        var continuation = await gateway.CreateContinuedModelSetAsync(
            continuationRequest: null!,
            CancellationToken.None);
        Assert.False(continuation.IsSuccess);
        Assert.Equal(
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .ContinuationContractInvalidErrorCode,
            continuation.ErrorCode);

        await app.StopAsync();
        Assert.False(gateway.IsBound);
        Assert.Null(
            DesignConversionDpoTeacherAttestationAssociation.Resolve(
                artifactDb));
        await app.StopAsync();
        Assert.Null(
            DesignConversionDpoTeacherAttestationAssociation.Resolve(
                artifactDb));
        Assert.Equal(
            ContinuousLearningModelServerStartup.DpoPolicyBaseStoppedErrorCode,
            startup.Readiness.ConfigurationErrorCode);
        Assert.Equal(
            "dpo_policy_base_stopped_inactive",
            startup.Readiness.ConfigurationStatus);
        Assert.False(startup.Readiness.StableDpoExecutorConfigured);
        Assert.False(startup.Readiness.CompositionMaterialized);
        AssertStableReadinessRequiresBoundGateway(startup, gateway);
        var afterStop = await gateway.CreateModelSetAsync(
            CancellationToken.None);
        Assert.False(afterStop.IsSuccess);
        Assert.Equal(
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ProviderUnavailableErrorCode,
            afterStop.ErrorCode);
        await app.DisposeAsync();
        await app.DisposeAsync();
        Assert.Null(
            DesignConversionDpoTeacherAttestationAssociation.Resolve(
                artifactDb));
    }

    [Fact]
    public async Task ProductionHost_CleanStopReleasesRetainedRootsAfterRetirementAndPreservesStoppedReadiness()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment();
        var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("clean-stop-releases-retained-roots"));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        try
        {
            await app.StartAsync();
            Assert.True(gateway.IsBound);

            await app.StopAsync();
            await startup.AwaitRetirementAsync()
                .WaitAsync(TimeSpan.FromSeconds(5));
            await gateway.AwaitRetirementAsync()
                .WaitAsync(TimeSpan.FromSeconds(5));

            Assert.False(gateway.IsBound);
            Assert.Equal(
                ContinuousLearningModelServerStartup
                    .DpoPolicyBaseStoppedErrorCode,
                startup.Readiness.ConfigurationErrorCode);
            Assert.Equal(
                "dpo_policy_base_stopped_inactive",
                startup.Readiness.ConfigurationStatus);
            Assert.NotEqual(
                ContinuousLearningModelServerStartup
                    .RetainedObjectIdentityLostErrorCode,
                startup.Readiness.ConfigurationErrorCode);

            await app.StopAsync();
            fixture.AssertLeasedPathsRenameable();
            await app.DisposeAsync();
            await app.DisposeAsync();
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    [Fact]
    public async Task ProductionHost_ExplicitTrainingActivationPublishesExactDesignConversionIntakeCycleOnlyAfterStart()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment(
            trainingActivationRequested: true);
        var arguments = fixture.ReadyFileArguments(
            "retained-training-opt-in");
        Directory.CreateDirectory(Path.Combine(fixture.RepositoryRoot, "training-artifacts", "models"));
        await using var app = Program.CreateLocalLoopbackHostCore(
            arguments, Environment.GetEnvironmentVariable, fixture.RepositoryRoot);
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        Assert.True(startup.Readiness.ConfigurationAdmitted);
        Assert.True(startup.HasDeferredCompositionPlan);
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        var authority = app.Services.GetRequiredService<
            ServerOwnedLegalTrainingTeacherAdmissionAuthority>();
        var artifactDb = ReadTrainingIntakeArtifactDbFromUnstartedHost(app);

        Assert.Same(gateway, ReadDpoPolicyBaseProvider(artifactDb));
        Assert.Same(authority, ReadLegalTeacherAdmissionAuthority(artifactDb));
        Assert.False(startup.Readiness.TrainingEnabled);
        Assert.False(startup.Readiness.CycleExecutionAvailable);
        Assert.False(startup.Readiness.PerformsTraining);
        Assert.False(startup.IsLegalTrainingTeacherAdmissionActive());

        await app.StartAsync();

        Assert.True(startup.Readiness.TrainingEnabled);
        Assert.True(startup.Readiness.CycleExecutionAvailable);
        Assert.True(startup.Readiness.PerformsTraining);
        Assert.False(startup.Readiness.ProductReady);
        Assert.True(startup.IsLegalTrainingTeacherAdmissionActive());
        Assert.True(gateway.IsBound);
        Assert.NotNull(
            DesignConversionDpoTeacherAttestationAssociation.Resolve(
                artifactDb));
        await WaitUntilAsync(
            () => File.Exists(arguments[1]),
            TimeSpan.FromSeconds(10));
        using (var readyDocument = JsonDocument.Parse(
                   await File.ReadAllTextAsync(arguments[1])))
        {
            var ready = readyDocument.RootElement;
            var readiness = ready.GetProperty(
                "continuousLearningReadiness");
            Assert.True(readiness
                .GetProperty("performs_training")
                .GetBoolean());
            Assert.True(ready.GetProperty("performsTraining").GetBoolean());
            Assert.Equal(
                readiness.GetProperty("performs_training").GetBoolean(),
                ready.GetProperty("performsTraining").GetBoolean());
            Assert.DoesNotContain(
                "no_training",
                ready.GetProperty("readyFileContractBoundary").GetString(),
                StringComparison.Ordinal);
            Assert.False(ready.GetProperty("productReady").GetBoolean());
        }
        using (var client = new HttpClient
               {
                   BaseAddress = Program.ResolveStartedLoopbackAddress(app),
               })
        using (var routeRequest = new HttpRequestMessage(
                   HttpMethod.Post,
                   DesignConversionModelServerEndpointRegistry
                       .TrainingIntakeEndpointPath)
               {
                   Content = new StringContent(
                       JsonSerializer.Serialize(
                           new DesignConversionModelServerTrainingIntakeRequest(
                               DesignConversionModelServerTrainingIntakeHandler
                                   .ActionList)),
                       System.Text.Encoding.UTF8,
                       "application/json"),
               })
        using (var routeResponse = await client.SendAsync(routeRequest))
        {
            routeResponse.EnsureSuccessStatusCode();
            var routeBody = JsonSerializer.Deserialize<
                DesignConversionModelServerTrainingIntakeResponse>(
                    await routeResponse.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions(JsonSerializerDefaults.Web));
            Assert.NotNull(routeBody);
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry
                    .TrainingIntakeEndpointPath,
                routeBody!.Endpoint);
            Assert.Equal(
                bool.TrueString,
                routeBody.BoundaryMetadata["runtime_invoked"]);
            Assert.Equal(
                DesignConversionModelServerTrainingIntakeHandler
                    .LocalTrainingIntakeRouteBoundary,
                routeBody.BoundaryMetadata[
                    "local_training_intake_route_boundary"]);
        }

        var legalLeaseResult = authority.AcquireTrainingLease(
            [CreateLegalTeacherProvenance(
                fixture.ModelId,
                fixture.OllamaManifestDigest,
                "positive-production-lease")]);
        Assert.True(legalLeaseResult.IsSuccess, legalLeaseResult.ErrorMessage);
        using (var legalLease = Assert.IsType<
                   ServerOwnedLegalTrainingTeacherAdmissionLease>(
                       legalLeaseResult.Value))
        {
            var mutationPermit = legalLease.AcquireMutationPermit();
            Assert.True(mutationPermit.IsSuccess, mutationPermit.ErrorMessage);
            Assert.NotNull(mutationPermit.Value);
            mutationPermit.Value!.Dispose();
        }

        await app.StopAsync();
        Assert.False(startup.Readiness.TrainingEnabled);
        Assert.False(startup.Readiness.CycleExecutionAvailable);
        Assert.False(startup.Readiness.PerformsTraining);
        Assert.False(startup.IsLegalTrainingTeacherAdmissionActive());
        Assert.False(gateway.IsBound);
        Assert.Null(
            DesignConversionDpoTeacherAttestationAssociation.Resolve(
                artifactDb));
        Assert.False(File.Exists(arguments[1]));
    }

    [Fact]
    public async Task ProductionHost_ExplicitTrainingActivationRequiresTrustedOwnedRootAndModels()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment(trainingActivationRequested: true);
        var arguments = fixture.ReadyFileArguments("retained-training-root-controls");
        Assert.False(Directory.Exists(Path.Combine(fixture.RepositoryRoot, "training-artifacts", "models")));
        var incomplete = Assert.Throws<InvalidOperationException>(() => Program.CreateLocalLoopbackHostCore(
            arguments, Environment.GetEnvironmentVariable, fixture.RepositoryRoot));
        Assert.Contains("ROOT_INVALID", incomplete.Message);
        var wrongRoot = Path.Combine(fixture.Root, "absent-trusted-root");
        var wrong = Assert.Throws<InvalidOperationException>(() => Program.CreateLocalLoopbackHostCore(
            arguments, Environment.GetEnvironmentVariable, wrongRoot));
        Assert.Contains("ROOT_INVALID", wrong.Message);
        Directory.CreateDirectory(Path.Combine(fixture.RepositoryRoot, "training-artifacts", "models"));
        await using var valid = Program.CreateLocalLoopbackHostCore(
            arguments, Environment.GetEnvironmentVariable, fixture.RepositoryRoot);
        var startup = valid.Services.GetRequiredService<ContinuousLearningModelServerStartup>();
        Assert.True(startup.Readiness.ConfigurationAdmitted);
        Assert.True(startup.HasDeferredCompositionPlan);
        Assert.False(startup.Readiness.TrainingEnabled);
        Assert.False(File.Exists(arguments[1]));
    }

    [Fact]
    public async Task ProductionHost_StopRevokesBeforeHeldPermitDrainAndExactReplacementReacquires()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment(
            trainingActivationRequested: true);
        var firstArguments = fixture.ReadyFileArguments(
            "retained-training-drain-first");
        await using var first = Program.CreateLocalLoopbackHost(firstArguments);
        await first.StartAsync();
        var firstStartup = first.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var firstAuthority = first.Services.GetRequiredService<
            ServerOwnedLegalTrainingTeacherAdmissionAuthority>();
        var firstBroker = first.Services.GetRequiredService<
            IServerOwnedDpoTrainingLeaseBroker>();
        Assert.Same(firstAuthority, firstBroker);
        var trainingContent = BoundedDpoTrainingContentProjector.Project(
        [
            new DpoTrainingContentRowInput(
                "chosen_ids;f32;shape=1",
                "chosen_mask;f32;shape=1",
                "rejected_ids;f32;shape=1\nrejected_mask;f32;shape=1",
                [0.25],
                "translation-dpo-final-token-tensors-v2")
        ]);
        Assert.True(trainingContent.IsSuccess, trainingContent.ErrorMessage);
        var firstLeaseResult = firstBroker.AcquireTranslationTrainingLease(
            trainingContent.Value!,
            "production-lifecycle-held");
        Assert.True(firstLeaseResult.IsSuccess, firstLeaseResult.ErrorMessage);
        var firstLease = Assert.IsType<
            ServerOwnedLegalTrainingTeacherAdmissionLease>(
                firstLeaseResult.Value);
        var heldPermitResult = firstLease.AcquireMutationPermit();
        var heldPermit = Assert.IsAssignableFrom<IDisposable>(
            heldPermitResult.Value);
        Task? stop = null;
        try
        {
            stop = first.StopAsync();
            await WaitUntilAsync(
                () => !firstStartup.Readiness.TrainingEnabled,
                TimeSpan.FromSeconds(10));
            Assert.False(stop.IsCompleted);
            Assert.False(firstStartup.Readiness.TrainingEnabled);
            Assert.False(firstStartup.Readiness.CycleExecutionAvailable);
            Assert.False(firstStartup.Readiness.PerformsTraining);
            Assert.False(firstStartup.IsLegalTrainingTeacherAdmissionActive());
            Assert.False(firstLease.AcquireMutationPermit().IsSuccess);
            Assert.False(firstBroker.AcquireTranslationTrainingLease(
                trainingContent.Value!,
                "production-lifecycle-post-stop-denied").IsSuccess);

            await using (var duplicate = Program.CreateLocalLoopbackHost(
                             fixture.ReadyFileArguments(
                                 "retained-training-drain-duplicate")))
            {
                var duplicateFailure = await Assert.ThrowsAsync<
                    ContinuousLearningMutationAdmissionDomainAlreadyOwnedException>(
                    () => duplicate.StartAsync());
                Assert.Equal(
                    ContinuousLearningModelServerStartup
                        .MutationAdmissionDomainAlreadyOwnedErrorCode,
                    duplicateFailure.Message);
            }

            heldPermit.Dispose();
            heldPermit = null!;
            firstLease.Dispose();
            await stop.WaitAsync(TimeSpan.FromSeconds(10));
            await first.DisposeAsync();

            await using var replacement = Program.CreateLocalLoopbackHost(
                fixture.ReadyFileArguments(
                    "retained-training-drain-replacement"));
            await replacement.StartAsync();
            var replacementStartup = replacement.Services.GetRequiredService<
                ContinuousLearningModelServerStartup>();
            var replacementAuthority = replacement.Services.GetRequiredService<
                ServerOwnedLegalTrainingTeacherAdmissionAuthority>();
            var replacementBroker = replacement.Services.GetRequiredService<
                IServerOwnedDpoTrainingLeaseBroker>();
            Assert.Same(replacementAuthority, replacementBroker);
            Assert.True(replacementStartup.IsLegalTrainingTeacherAdmissionActive());
            var replacementLeaseResult = replacementBroker
                .AcquireTranslationTrainingLease(
                    trainingContent.Value!,
                    "production-lifecycle-replacement");
            Assert.True(
                replacementLeaseResult.IsSuccess,
                replacementLeaseResult.ErrorMessage);
            using var replacementLease = Assert.IsType<
                ServerOwnedLegalTrainingTeacherAdmissionLease>(
                    replacementLeaseResult.Value);
            var replacementPermitResult = replacementLease
                .AcquireMutationPermit();
            Assert.True(
                replacementPermitResult.IsSuccess,
                replacementPermitResult.ErrorMessage);
            replacementPermitResult.Value!.Dispose();
            await replacement.StopAsync();
        }
        finally
        {
            heldPermit?.Dispose();
            firstLease.Dispose();
            if (stop is not null)
                await stop.WaitAsync(TimeSpan.FromSeconds(10));
        }
    }

    [Fact]
    public void ProductionHost_TrainingOptInRejectsCallerProviderOverrideWithoutExactServerAuthority()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment(
            trainingActivationRequested: true);
        var callerProvider = new DesignConversionDpoPolicyBaseTestProvider();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            Program.CreateLocalLoopbackHost(
                fixture.ReadyFileArguments(
                    "training-opt-in-caller-override"),
                callerProvider));

        Assert.Contains(
            "exact server-owned policy-base provider",
            exception.Message,
            StringComparison.Ordinal);
        Assert.Equal(0, callerProvider.InvocationCount);
    }

    [Fact]
    public async Task ProductionHost_ExplicitCallerProviderWinsOverConfiguredRetainedGateway()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment();
        var callerProvider = new DesignConversionDpoPolicyBaseTestProvider();
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("caller-override"),
            callerProvider);
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var artifactDb = ReadTrainingIntakeArtifactDbFromUnstartedHost(app);

        Assert.Same(callerProvider, ReadDpoPolicyBaseProvider(artifactDb));
        Assert.Null(ReadLegalTeacherAdmissionAuthority(artifactDb));
        Assert.False(gateway.IsBound);

        await app.StartAsync();

        Assert.True(gateway.IsBound);
        AssertStableReadinessRequiresBoundGateway(startup, gateway);
        Assert.Same(callerProvider, ReadDpoPolicyBaseProvider(artifactDb));
        Assert.Null(ReadLegalTeacherAdmissionAuthority(artifactDb));
        Assert.Equal(0, callerProvider.InvocationCount);
    }

    [Fact]
    public async Task ProductionHost_AdmittedButInvalidCheckpointLeavesInjectedGatewayUnboundAndFailClosed()
    {
        using var fixture = new ModelServerEndpointFixture();
        File.WriteAllBytes(fixture.BaseCheckpoint, [0x01, 0x02, 0x03]);
        using var environment = fixture.ApplyEnvironment();
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("invalid-checkpoint"));
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        var artifactDb = ReadTrainingIntakeArtifactDbFromUnstartedHost(app);

        Assert.Same(gateway, ReadDpoPolicyBaseProvider(artifactDb));
        await app.StartAsync();

        Assert.False(gateway.IsBound);
        Assert.Null(
            DesignConversionDpoTeacherAttestationAssociation.Resolve(
                artifactDb));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        Assert.Equal(
            ContinuousLearningModelServerStartup.MaterializationFailedErrorCode,
            startup.Readiness.ConfigurationErrorCode);
        AssertStableReadinessRequiresBoundGateway(startup, gateway);
        var result = await gateway.CreateModelSetAsync(CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ProviderUnavailableErrorCode,
            result.ErrorCode);
        Assert.DoesNotContain(
            fixture.Root,
            result.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProductionHost_EligibleProviderTeacherRuntimeCreationFailureRemainsFailFast()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment();
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("teacher-runtime-failure"));
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var artifactDb = ReadTrainingIntakeArtifactDbFromUnstartedHost(app);
        var digestField = Assert.IsAssignableFrom<FieldInfo>(
            typeof(ContinuousLearningTp1DpoConfiguration).GetField(
                "<ExpectedOllamaManifestDigest>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic));
        const string malformedDigest =
            "not-a-canonical-ollama-manifest-digest";
        digestField.SetValue(
            startup.TeacherConfiguration,
            malformedDigest);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => app.StartAsync());

        Assert.Equal(
            DesignConversionRetainedDpoPolicyBaseActivation
                .ActivationFailedErrorCode,
            exception.Message);
        Assert.DoesNotContain(
            malformedDigest,
            exception.Message,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "expectedOllamaManifestDigest",
            exception.Message,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            fixture.Root,
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            fixture.ModelId,
            exception.Message,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            fixture.OllamaManifestDigest,
            exception.Message,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            fixture.AuditSalt,
            exception.Message,
            StringComparison.Ordinal);
        Assert.True(gateway.IsBound);
        Assert.True(startup.Readiness.StableDpoExecutorConfigured);
        Assert.Null(
            DesignConversionDpoTeacherAttestationAssociation.Resolve(
                artifactDb));
    }

    [Fact]
    public void EndpointMapper_HasNoRequestPolicyOrExecutionParameter()
    {
        var mapper = typeof(ContinuousLearningReadinessEndpoint).GetMethod(
            "MapContinuousLearningReadiness",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var parameter = Assert.Single(mapper.GetParameters());
        Assert.Equal("IEndpointRouteBuilder", parameter.ParameterType.Name);
        Assert.DoesNotContain(
            typeof(ContinuousLearningReadinessEndpoint).GetMethods(
                BindingFlags.Static | BindingFlags.NonPublic),
            method => method.Name.Contains("RunCycle", StringComparison.Ordinal)
                      || method.Name.Contains("Train", StringComparison.Ordinal));
    }

    [Fact]
    public void ProgramAndStartup_HaveNoInternalPathIdentitySuccessTransition()
    {
        Assert.Null(typeof(ContinuousLearningModelServerStartup).GetMethod(
            "MarkPathIdentityVerified",
            BindingFlags.Instance | BindingFlags.NonPublic));
        Assert.DoesNotContain(
            typeof(ContinuousLearningModelServerStartup).GetMethods(
                BindingFlags.Instance | BindingFlags.NonPublic),
            method => method.Name.Contains(
                "PathIdentityVerified",
                StringComparison.Ordinal));
        Assert.DoesNotContain(
            typeof(Program).Assembly.GetTypes()
                .SelectMany(type => type.GetMethods(
                    BindingFlags.Instance |
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic)),
            method => method.Name.Contains(
                "MarkPathIdentityVerified",
                StringComparison.Ordinal));

        var incomplete = new ContinuousLearningProductionReadinessStatus(
            ConfigurationAdmitted: true,
            ConfigurationStatus:
                "retained_object_identity_bound_path_unverified_inactive",
            ConfigurationErrorCode: ContinuousLearningModelServerStartup
                .NamespacePathUnverifiedErrorCode,
            TeacherLane: ContinuousLearningProductionReadinessStatus.Tp1Lane,
            TeacherPolicyConfiguration:
                ContinuousLearningProductionReadinessStatus.Tp1PolicyConfiguration,
            CompositionMaterialized: false,
            PathIdentityVerified: false,
            RetainedObjectIdentityBound: false,
            StableDpoExecutorConfigured: false,
            TrainingEnabled: false,
            CycleExecutionAvailable: false,
            PerformsTraining: false,
            ProductReady: false);
        var (
            configurationAdmitted,
            _,
            _,
            _,
            _,
            _,
            _,
            _,
            _,
            _,
            _,
            _,
            productReady) = incomplete;
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(incomplete));
        Assert.True(configurationAdmitted);
        Assert.False(productReady);
        Assert.False(incomplete.LegalEvidenceConfigured);
        Assert.False(incomplete.LegalEvidenceVerified);
        Assert.Null(incomplete.LocalRetentionPolicyRef);
        Assert.Null(incomplete.OutputLicenseRef);
        Assert.False(incomplete.OllamaManifestDigestConfigured);
        Assert.False(incomplete.OllamaModelLayerDigestConfigured);
        Assert.Contains(
            typeof(ContinuousLearningProductionReadinessStatus).GetConstructors(),
            constructor => constructor.GetParameters().Length == 13);
        var legacyReadinessDeconstruct = Assert.Single(
            typeof(ContinuousLearningProductionReadinessStatus).GetMethods(),
            method => method.Name == "Deconstruct" && method.GetParameters().Length == 13);
        Assert.Equal(
            [
                "ConfigurationAdmitted",
                "ConfigurationStatus",
                "ConfigurationErrorCode",
                "TeacherLane",
                "TeacherPolicyConfiguration",
                "CompositionMaterialized",
                "PathIdentityVerified",
                "RetainedObjectIdentityBound",
                "StableDpoExecutorConfigured",
                "TrainingEnabled",
                "CycleExecutionAvailable",
                "PerformsTraining",
                "ProductReady",
            ],
            legacyReadinessDeconstruct.GetParameters().Select(parameter => parameter.Name!));
        Assert.False(document.RootElement
            .GetProperty("path_identity_verified")
            .GetBoolean());
    }

    private static object GetAccessFactoryTarget(
        IServiceCollection services,
        ContinuousLearningModelServerStartup startup)
    {
        var descriptor = Assert.Single(
            services,
            candidate => candidate.ServiceType ==
                         typeof(ServerOwnedContinuousLearningFileAccess));
        var factory = descriptor.ImplementationFactory;
        Assert.NotNull(factory);
        var target = factory!.Target!;
        Assert.NotNull(target);
        var captures = target.GetType()
            .GetFields(BindingFlags.Instance |
                       BindingFlags.Public |
                       BindingFlags.NonPublic)
            .Select(field => field.GetValue(target))
            .ToArray();
        Assert.Single(captures);
        Assert.Same(startup, captures[0]);
        return target;
    }

    private static JsonlDesignConversionLocalArtifactDb
        ReadTrainingIntakeArtifactDbFromUnstartedHost(WebApplication app)
    {
        var endpoint = Assert.Single(
            ((IEndpointRouteBuilder)app).DataSources
                .SelectMany(dataSource => dataSource.Endpoints)
                .OfType<RouteEndpoint>(),
            candidate =>
                string.Equals(
                    candidate.RoutePattern.RawText,
                    DesignConversionModelServerEndpointRegistry
                        .TrainingIntakeEndpointPath,
                    StringComparison.Ordinal)
                && candidate.Metadata
                    .GetMetadata<HttpMethodMetadata>()?
                    .HttpMethods
                    .Contains(
                        DesignConversionModelServerEndpointRegistry
                            .TrainingIntakeEndpointMethod,
                        StringComparer.OrdinalIgnoreCase) == true);
        var requestDelegate = Assert.IsType<RequestDelegate>(
            endpoint.RequestDelegate);
        var registry = ReadSingleReachableInstance<
            DesignConversionModelServerEndpointRegistry>(
                requestDelegate,
                maxDepth: 8,
                maxNodeCount: 128);
        var registration = registry.Resolve(
            DesignConversionModelServerEndpointRegistry
                .TrainingIntakeEndpointMethod,
            DesignConversionModelServerEndpointRegistry
                .TrainingIntakeEndpointPath);
        Assert.True(registration.IsSuccess, registration.ErrorMessage);
        var handler = Assert.IsType<
            DesignConversionModelServerTrainingIntakeHandler>(
                registration.Value!.Handler.Target);
        var api = Assert.IsType<DesignConversionDpoUsefulnessApi>(
            typeof(DesignConversionModelServerTrainingIntakeHandler)
                .GetField(
                    "_api",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(handler));
        return Assert.IsType<JsonlDesignConversionLocalArtifactDb>(
            typeof(DesignConversionDpoUsefulnessApi)
                .GetField(
                    "_artifactDb",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(api));
    }

    private static IDesignConversionDpoPolicyBaseProvider?
        ReadDpoPolicyBaseProvider(
            JsonlDesignConversionLocalArtifactDb artifactDb)
        => (IDesignConversionDpoPolicyBaseProvider?)
            typeof(JsonlDesignConversionLocalArtifactDb)
                .GetField(
                    "_dpoPolicyBaseProvider",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(artifactDb);

    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority?
        ReadLegalTeacherAdmissionAuthority(
            JsonlDesignConversionLocalArtifactDb artifactDb)
        => (ServerOwnedLegalTrainingTeacherAdmissionAuthority?)
            typeof(JsonlDesignConversionLocalArtifactDb)
                .GetField(
                    "_legalTeacherAdmissionAuthority",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(artifactDb);

    private static void AssertStableReadinessRequiresBoundGateway(
        ContinuousLearningModelServerStartup startup,
        DesignConversionRetainedDpoPolicyBaseProviderGateway gateway)
    {
        if (startup.Readiness.StableDpoExecutorConfigured)
            Assert.True(gateway.IsBound);
    }

    private static TrainingTeacherProvenance CreateLegalTeacherProvenance(
        string modelId,
        string ollamaManifestDigest,
        string requestId)
        => new(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            modelId,
            ollamaManifestDigest,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            requestId,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind = LegalTrainingTeacherAdmissionAuthority
                .OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = new string('c', 64),
        };

    private static async Task WaitUntilAsync(
        Func<bool> predicate,
        TimeSpan timeout)
    {
        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        while (!predicate())
        {
            if (elapsed.Elapsed >= timeout)
                throw new TimeoutException("The expected host lifecycle state was not observed.");
            await Task.Delay(10);
        }
    }

    private static void AssertOnlyOwnerLockLeavesAdded(
        IReadOnlyCollection<string> before,
        IReadOnlyCollection<string> after)
    {
        var expectedOwnerLocks = ModelServerEndpointFixture
            .ExpectedOwnerLockSnapshotEntries();
        Assert.DoesNotContain(
            expectedOwnerLocks,
            before.Contains);
        Assert.Empty(before.Except(after, StringComparer.Ordinal));
        var added = after
            .Except(before, StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            expectedOwnerLocks.Order(StringComparer.Ordinal),
            added);
        Assert.Equal(before.Count + expectedOwnerLocks.Length, after.Count);
    }

    private static T ReadSingleReachableInstance<T>(
        object root,
        int maxDepth,
        int maxNodeCount)
        where T : class
    {
        var pending = new Queue<(object Value, int Depth)>();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var matches = new List<T>();
        pending.Enqueue((root, 0));
        while (pending.Count > 0)
        {
            var (value, depth) = pending.Dequeue();
            if (!visited.Add(value))
                continue;
            Assert.True(visited.Count <= maxNodeCount);
            if (value is T match)
            {
                matches.Add(match);
                continue;
            }
            if (depth >= maxDepth || IsTraversalLeaf(value.GetType()))
                continue;
            if (value is Delegate callback)
            {
                foreach (var invocation in callback.GetInvocationList())
                {
                    if (invocation.Target is not null)
                        pending.Enqueue((invocation.Target, depth + 1));
                }
                continue;
            }

            for (var currentType = value.GetType();
                 currentType is not null;
                 currentType = currentType.BaseType)
            {
                foreach (var field in currentType.GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly))
                {
                    if (field.FieldType.IsPointer || field.FieldType.IsByRef)
                        continue;
                    var child = field.GetValue(value);
                    if (child is not null)
                        pending.Enqueue((child, depth + 1));
                }
            }
        }
        return Assert.Single(matches);
    }

    private static bool IsTraversalLeaf(Type type)
        => type.IsPrimitive
           || type.IsEnum
           || type.IsValueType
           || type == typeof(string)
           || type == typeof(Type)
           || typeof(MemberInfo).IsAssignableFrom(type)
           || typeof(System.Reflection.Assembly).IsAssignableFrom(type)
           || typeof(System.Reflection.Module).IsAssignableFrom(type)
           || typeof(System.Collections.IEnumerable).IsAssignableFrom(type);

    private static void AssertExactConfiguredPathsNotReachable(
        IEnumerable<object?> roots,
        IReadOnlyCollection<string> exactPaths)
    {
        var pending = new Stack<object?>(roots);
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var continuousLearningAssembly = typeof(ContinuousLearningModelServerStartup).Assembly;
        var modelServerAssembly = typeof(Program).Assembly;
        while (pending.TryPop(out var candidate))
        {
            if (candidate is null)
                continue;
            if (candidate is string text)
            {
                Assert.DoesNotContain(
                    exactPaths,
                    path => string.Equals(path, text, StringComparison.OrdinalIgnoreCase));
                continue;
            }

            var type = candidate.GetType();
            if (!type.IsValueType && !visited.Add(candidate))
                continue;
            Assert.True(visited.Count <= 10_000, "Reachability walk exceeded its bound.");
            if (candidate is Array array)
            {
                foreach (var element in array)
                    pending.Push(element);
                continue;
            }
            if (candidate is Delegate callback)
            {
                foreach (var invocation in callback.GetInvocationList())
                    pending.Push(invocation.Target);
                continue;
            }
            if (type.Assembly != continuousLearningAssembly &&
                type.Assembly != modelServerAssembly)
            {
                continue;
            }

            foreach (var field in type.GetFields(
                         BindingFlags.Instance |
                         BindingFlags.Public |
                         BindingFlags.NonPublic))
            {
                pending.Push(field.GetValue(candidate));
            }
        }
    }
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ModelServerContinuousLearningEnvironmentCollection
{
    public const string Name = "ModelServer continuous-learning environment";
}

file sealed class ModelServerEndpointFixture : IDisposable
{
    private const string OwnerLockSnapshotPrefix = "owner-lock:";
    private static readonly string[] ExpectedOwnerLockRelativePaths =
    [
        Path.Combine(
            "adapter",
            ".continuous-learning.adapter-owner.lock"),
        Path.Combine(
            "registry",
            ".continuous-learning.registry-owner.lock"),
    ];
    private readonly Dictionary<string, string> _environment;
    private string _cleanupRoot;

    public ModelServerEndpointFixture()
    {
        Root = Path.Combine(
            Path.GetTempPath(),
            $"modelserver-continuous-learning-endpoint-tests-{Guid.NewGuid():N}");
        _cleanupRoot = Root;
        Directory.CreateDirectory(Root);
        BaseCheckpoint = Path.Combine(Root, "base.bin");
        Tokenizer = Path.Combine(Root, "tokenizer.bin");
        AdapterRoot = Path.Combine(Root, "adapter");
        CacheRoot = Path.Combine(Root, "cache");
        RegistryRoot = Path.Combine(Root, "registry");
        RepositoryRoot = Path.Combine(Root, "repository-root");
        CreateRepositorySentinels(RepositoryRoot);
        PairRoot = Path.Combine(
            RepositoryRoot,
            "training-data",
            "dpo",
            "html-css",
            "learning-unit",
            "v0001",
            "pairs");
        ModelConfiguration = CreateTestModelConfig();
        new LlmModel(ModelConfiguration).SaveCheckpoint(BaseCheckpoint);
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(
            "modelserver continuous learning endpoint fixture",
            ModelConfiguration.VocabSize);
        tokenizer.Save(Tokenizer);
        Directory.CreateDirectory(AdapterRoot);
        Directory.CreateDirectory(CacheRoot);
        Directory.CreateDirectory(RegistryRoot);
        Directory.CreateDirectory(PairRoot);
        ModelId = "qwen-endpoint-secret-marker:latest";
        OllamaManifestDigest = new string('d', 64);
        BaseModelId = "registry/endpoint-base-model";
        BaseModelRevision = "endpoint-base-revision-r3";
        AuditSalt = Convert.ToBase64String(Enumerable.Range(40, 32)
            .Select(static value => checked((byte)value))
            .ToArray());
        _environment = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ContinuousLearningModelServerConfigurationLoader.EnabledEnvironmentName]
                = bool.TrueString,
            [ContinuousLearningModelServerConfigurationLoader.ModelIdEnvironmentName]
                = ModelId,
            [ContinuousLearningModelServerConfigurationLoader.OllamaManifestDigestEnvironmentName]
                = OllamaManifestDigest,
            [ContinuousLearningModelServerConfigurationLoader.OllamaModelLayerDigestEnvironmentName]
                = new string('c', 64),
            [ContinuousLearningModelServerConfigurationLoader.BaseModelIdEnvironmentName]
                = BaseModelId,
            [ContinuousLearningModelServerConfigurationLoader.BaseModelRevisionEnvironmentName]
                = BaseModelRevision,
            [ContinuousLearningModelServerConfigurationLoader.MaxTeacherCallsEnvironmentName]
                = "25",
            [ContinuousLearningModelServerConfigurationLoader.AuditSaltEnvironmentName]
                = AuditSalt,
            [ContinuousLearningModelServerConfigurationLoader.LocalRetentionPolicyRefEnvironmentName]
                = $"sha256:{new string('a', 64)}",
            [ContinuousLearningModelServerConfigurationLoader.OutputLicenseRefEnvironmentName]
                = $"sha256:{new string('b', 64)}",
            [ContinuousLearningModelServerConfigurationLoader.BaseCheckpointEnvironmentName]
                = BaseCheckpoint,
            [ContinuousLearningModelServerConfigurationLoader.TokenizerEnvironmentName]
                = Tokenizer,
            [ContinuousLearningModelServerConfigurationLoader.AdapterRootEnvironmentName]
                = AdapterRoot,
            [ContinuousLearningModelServerConfigurationLoader.CacheRootEnvironmentName]
                = CacheRoot,
            [ContinuousLearningModelServerConfigurationLoader.RegistryRootEnvironmentName]
                = RegistryRoot,
            [ContinuousLearningModelServerConfigurationLoader.RepositoryRootEnvironmentName]
                = RepositoryRoot,
            [ContinuousLearningModelServerConfigurationLoader
                .RepositoryModelIdEnvironmentName] = "qwen3-5-9b-dpo",
            [ContinuousLearningModelServerConfigurationLoader
                .RepositoryModelVersionEnvironmentName] = "v0001",
            [ContinuousLearningModelServerConfigurationLoader
                .RuntimeConfigSha256EnvironmentName] = new string('1', 64),
            [ContinuousLearningModelServerConfigurationLoader
                .RuntimeTopologySha256EnvironmentName] = new string('2', 64),
            [ContinuousLearningModelServerConfigurationLoader
                .RuntimeTokenizerSha256EnvironmentName] = new string('3', 64),
            [ContinuousLearningModelServerConfigurationLoader
                .RuntimeTrainerSha256EnvironmentName] = new string('4', 64),
            [ContinuousLearningModelServerConfigurationLoader
                .RuntimeSchemaSha256EnvironmentName] = new string('5', 64),
            [ContinuousLearningModelServerConfigurationLoader
                .RuntimeCompatibilityKeyEnvironmentName] =
                "tp1-dpo-runtime-v0001",
            [ContinuousLearningModelServerConfigurationLoader.DpoPairRootEnvironmentName]
                = PairRoot,
        };
        AddExplicitModelAndTrainingConfiguration(
            _environment,
            ModelConfiguration);
    }

    public string Root { get; }
    public string ModelId { get; }
    public string OllamaManifestDigest { get; }
    public string BaseModelId { get; }
    public string BaseModelRevision { get; }
    public string AuditSalt { get; }
    public string BaseCheckpoint { get; }
    public string Tokenizer { get; }
    public string AdapterRoot { get; }
    public string CacheRoot { get; }
    public string RegistryRoot { get; }
    public string RepositoryRoot { get; }
    public string PairRoot { get; }
    public ModelConfig ModelConfiguration { get; }
    public string[] ConfiguredPaths =>
    [
        BaseCheckpoint,
        Tokenizer,
        AdapterRoot,
        CacheRoot,
        RegistryRoot,
        PairRoot,
    ];

    private static void CreateRepositorySentinels(string repositoryRoot)
    {
        Directory.CreateDirectory(repositoryRoot);
        File.WriteAllText(
            Path.Combine(repositoryRoot, "LlmFromScratch.sln"),
            "fixture solution sentinel");
        Directory.CreateDirectory(Path.Combine(repositoryRoot, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(repositoryRoot, "11.DPO"));
    }

    public string[] ReadyFileArguments(string scenario)
        =>
        [
            Program.ReadyFileArgumentName,
            Path.Combine(Root, scenario, "modelserver-ready.json"),
        ];

    private static ModelConfig CreateTestModelConfig()
        => new()
        {
            VocabSize = 256,
            ContextLen = 8,
            HiddenDim = 8,
            NumLayers = 1,
            NumHeads = 1,
            NumKvHeads = 1,
            FfnHiddenDim = 16,
            RopeTheta = 10000f,
            RmsNormEps = 1e-5f,
        };

    private static void AddExplicitModelAndTrainingConfiguration(
        IDictionary<string, string> environment,
        ModelConfig model)
    {
        environment[ContinuousLearningModelServerConfigurationLoader.ModelVocabSizeEnvironmentName] = model.VocabSize.ToString();
        environment[ContinuousLearningModelServerConfigurationLoader.ModelContextLenEnvironmentName] = model.ContextLen.ToString();
        environment[ContinuousLearningModelServerConfigurationLoader.ModelHiddenDimEnvironmentName] = model.HiddenDim.ToString();
        environment[ContinuousLearningModelServerConfigurationLoader.ModelNumLayersEnvironmentName] = model.NumLayers.ToString();
        environment[ContinuousLearningModelServerConfigurationLoader.ModelNumHeadsEnvironmentName] = model.NumHeads.ToString();
        environment[ContinuousLearningModelServerConfigurationLoader.ModelNumKvHeadsEnvironmentName] = model.NumKvHeads.ToString();
        environment[ContinuousLearningModelServerConfigurationLoader.ModelFfnHiddenDimEnvironmentName] = model.FfnHiddenDim.ToString();
        environment[ContinuousLearningModelServerConfigurationLoader.ModelRopeThetaEnvironmentName] = "10000";
        environment[ContinuousLearningModelServerConfigurationLoader.ModelRmsNormEpsEnvironmentName] = "1E-05";
        environment[ContinuousLearningModelServerConfigurationLoader.TrainingBatchSizeEnvironmentName] = "1";
        environment[ContinuousLearningModelServerConfigurationLoader.TrainingSeqLenEnvironmentName] = "8";
        environment[ContinuousLearningModelServerConfigurationLoader.TrainingMaxLrEnvironmentName] = "0.01";
        environment[ContinuousLearningModelServerConfigurationLoader.TrainingMinLrEnvironmentName] = "0.001";
        environment[ContinuousLearningModelServerConfigurationLoader.TrainingWarmupStepsEnvironmentName] = "0";
        environment[ContinuousLearningModelServerConfigurationLoader.TrainingTotalStepsEnvironmentName] = "10";
        environment[ContinuousLearningModelServerConfigurationLoader.TrainingGradAccumStepsEnvironmentName] = "1";
        environment[ContinuousLearningModelServerConfigurationLoader.TrainingGradClipNormEnvironmentName] = "1";
        environment[ContinuousLearningModelServerConfigurationLoader.TrainingWeightDecayEnvironmentName] = "0";
        environment[ContinuousLearningModelServerConfigurationLoader.TrainingCheckpointEveryEnvironmentName] = "1";
        environment[ContinuousLearningModelServerConfigurationLoader.TrainingLogEveryEnvironmentName] = "1";
    }

    public IDisposable ApplyEnvironment(
        bool trainingActivationRequested = false)
    {
        var values = new Dictionary<string, string>(
            _environment,
            StringComparer.Ordinal)
        {
            [ContinuousLearningModelServerConfigurationLoader
                .TrainingActivationEnabledEnvironmentName] =
                trainingActivationRequested.ToString(),
        };
        if (trainingActivationRequested)
            values[ContinuousLearningModelServerConfigurationLoader
                .TrainingRootInitializationSeedEnvironmentName] = "0";
        return new EnvironmentScope(values);
    }

    public IDisposable ApplyClosedEnvironment(bool invalid)
    {
        var values = new Dictionary<string, string>(
            _environment,
            StringComparer.Ordinal);
        if (invalid)
            values[ContinuousLearningModelServerConfigurationLoader
                    .AuditSaltEnvironmentName] = "not-canonical-base64";
        else
            values[ContinuousLearningModelServerConfigurationLoader
                    .EnabledEnvironmentName] = bool.FalseString;
        return new EnvironmentScope(values);
    }

    public void Dispose()
    {
        if (Directory.Exists(_cleanupRoot))
            Directory.Delete(_cleanupRoot, recursive: true);
    }

    public string[] Snapshot()
        => Directory.EnumerateFileSystemEntries(
                Root,
                "*",
                SearchOption.AllDirectories)
            .Select(path =>
            {
                var relative = Path.GetRelativePath(Root, path);
                if (!File.Exists(path))
                    return $"directory:{relative}";
                if (ExpectedOwnerLockRelativePaths.Contains(
                        relative,
                        StringComparer.Ordinal))
                {
                    return OwnerLockSnapshotPrefix + relative;
                }
                return $"file:{relative}:" + Convert.ToHexString(
                    SHA256.HashData(File.ReadAllBytes(path)));
            })
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();

    public static string[] ExpectedOwnerLockSnapshotEntries()
        => ExpectedOwnerLockRelativePaths
            .Select(static relative => OwnerLockSnapshotPrefix + relative)
            .ToArray();

    public void AssertLeasedPathsRenameable()
    {
        if (OperatingSystem.IsWindows())
        {
            // Windows сохраняет прежний строгий oracle: после Dispose внешний
            // процесс снова обязан получить DELETE access к удержанным handles.
            AssertDeleteAccessAvailable(BaseCheckpoint, requireDirectory: false);
            AssertDeleteAccessAvailable(AdapterRoot, requireDirectory: true);
            AssertDeleteAccessAvailable(Root, requireDirectory: true);
        }
        var movedFile = BaseCheckpoint + ".post-dispose";
        var movedDirectory = AdapterRoot + ".post-dispose";
        var movedRoot = Root + ".post-dispose";
        MoveOneWay(BaseCheckpoint, movedFile);
        MoveOneWay(AdapterRoot, movedDirectory);
        MoveOneWay(Root, movedRoot);
        _cleanupRoot = movedRoot;
        Assert.True(File.Exists(Path.Combine(
            movedRoot,
            Path.GetFileName(movedFile))));
        Assert.True(Directory.Exists(Path.Combine(
            movedRoot,
            Path.GetFileName(movedDirectory))));
    }

    private static void AssertDeleteAccessAvailable(
        string path,
        bool requireDirectory)
    {
        var handle = OpenDeleteAccess(path, requireDirectory);
        var nativeHandle = handle.DangerousGetHandle();
        try
        {
            Assert.False(
                handle.IsInvalid,
                $"DELETE access failed after disposal with Win32 error {Marshal.GetLastPInvokeError()}.");
        }
        finally
        {
            handle.Dispose();
        }
        var open = GetHandleInformationWindows(nativeHandle, out _);
        var error = open ? 0 : Marshal.GetLastPInvokeError();
        Assert.False(open, "DELETE-access probe handle unexpectedly remained open.");
        Assert.Equal(6, error);
    }

    private static SafeFileHandle OpenDeleteAccess(
        string path,
        bool requireDirectory)
    {
        const uint deleteAccess = 0x00010000;
        const uint openExisting = 3;
        const uint backupSemantics = 0x02000000;
        const uint openReparsePoint = 0x00200000;
        return CreateFileForDeleteAccess(
            path,
            deleteAccess,
            (uint)(FileShare.Read | FileShare.Write | FileShare.Delete),
            IntPtr.Zero,
            openExisting,
            openReparsePoint | (requireDirectory ? backupSemantics : 0),
            IntPtr.Zero);
    }

    private static void MoveOneWay(string path, string moved)
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.True(
                MoveFileWindows(path, moved),
                $"One-way rename failed with Win32 error {Marshal.GetLastPInvokeError()}.");
            return;
        }

        // POSIX не доказывает Windows delete-share семантику. Для WSL здесь
        // нужен только post-dispose smoke: retained handles отпущены, а root
        // остаётся управляемым обычным rename без kernel32 P/Invoke.
        if (File.Exists(path))
            File.Move(path, moved);
        else
            Directory.Move(path, moved);
    }

    [DllImport(
        "kernel32.dll",
        EntryPoint = "MoveFileW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool MoveFileWindows(
        string existingFileName,
        string newFileName);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "CreateFileW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern SafeFileHandle CreateFileForDeleteAccess(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", EntryPoint = "GetHandleInformation", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetHandleInformationWindows(
        IntPtr @object,
        out uint flags);

    private sealed class EnvironmentScope : IDisposable
    {
        private readonly Dictionary<string, string?> _prior = new(StringComparer.Ordinal);

        public EnvironmentScope(IReadOnlyDictionary<string, string> values)
        {
            foreach (var (name, value) in values)
            {
                _prior[name] = Environment.GetEnvironmentVariable(name);
                Environment.SetEnvironmentVariable(name, value);
            }
        }

        public void Dispose()
        {
            foreach (var (name, value) in _prior)
                Environment.SetEnvironmentVariable(name, value);
        }
    }
}


public sealed partial class ContinuousLearningModelServerReadinessEndpointTests
{
    [Fact]
    public async Task RouteCapabilityRevoke_ReadinessAllocationFailurePreservesActiveLifecycleForRetry()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment(
            trainingActivationRequested: true);
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("route-revoke-readiness-retry"));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        await app.StartAsync();
        var capability = Assert.IsAssignableFrom<
            ContinuousLearningDesignConversionRouteMappingCapability>(
                typeof(ContinuousLearningModelServerStartup)
                    .GetField(
                        "_designConversionRouteMappingCapability",
                        BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(startup));
        var lifecycleStateField = typeof(ContinuousLearningModelServerStartup)
            .GetField(
                "_dpoPolicyBaseLifecycleState",
                BindingFlags.Instance | BindingFlags.NonPublic)!;
        var readinessBefore = startup.Readiness;
        var lifecycleStateBefore = lifecycleStateField.GetValue(startup);
        var retirementBefore = startup.AwaitRetirementAsync();
        var fatal = new OutOfMemoryException(
            "route-revoke-readiness-allocation");
        var prior = ContinuousLearningModelServerStartup
            .BeforeReadinessSnapshotAllocationObserverForTesting;
        try
        {
            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = () =>
                throw fatal;

            var thrown = Assert.Throws<OutOfMemoryException>(
                capability.Revoke);

            Assert.Same(fatal, thrown);
            Assert.True(capability.IsActive);
            Assert.Same(readinessBefore, startup.Readiness);
            Assert.Equal(
                lifecycleStateBefore,
                lifecycleStateField.GetValue(startup));
            Assert.Same(retirementBefore, startup.AwaitRetirementAsync());
            Assert.True(retirementBefore.IsCompletedSuccessfully);
            Assert.True(gateway.IsBound);

            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = null;
            capability.Revoke();

            Assert.False(capability.IsActive);
            var retirementAfter = startup.AwaitRetirementAsync();
            Assert.NotSame(retirementBefore, retirementAfter);
            Assert.False(startup.TryRegisterRetirementDependency(
                static () => Task.CompletedTask));
            await Task.WhenAll(
                    retirementAfter,
                    gateway.AwaitRetirementAsync())
                .WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = prior;
        }
    }


    [Fact]
    public async Task TeacherCapabilityLeaseDispose_ReadinessAllocationFailurePreservesRegistrationForRetry()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment(
            trainingActivationRequested: true);
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("teacher-revoke-readiness-retry"));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        await app.StartAsync();
        var hosted = Assert.Single(
            app.Services.GetServices<IHostedService>()
                .OfType<
                    DesignConversionDpoTeacherAttestationHostedAssociation>());
        var association = Assert.IsType<
            DesignConversionDpoTeacherAttestationAssociation
                .TrainingCapabilityLease>(
                    typeof(
                            DesignConversionDpoTeacherAttestationHostedAssociation)
                        .GetField(
                            "_association",
                            BindingFlags.Instance | BindingFlags.NonPublic)!
                        .GetValue(hosted));
        var runtime = Assert.IsType<
            DesignConversionDpoTeacherAttestationRuntime>(
                DesignConversionDpoTeacherAttestationAssociation
                    .ResolveExactProvider(gateway));
        var lifecycleStateField = typeof(ContinuousLearningModelServerStartup)
            .GetField(
                "_dpoPolicyBaseLifecycleState",
                BindingFlags.Instance | BindingFlags.NonPublic)!;
        var readinessBefore = startup.Readiness;
        var lifecycleStateBefore = lifecycleStateField.GetValue(startup);
        var retirementBefore = startup.AwaitRetirementAsync();
        var fatal = new OutOfMemoryException(
            "teacher-revoke-readiness-allocation");
        var prior = ContinuousLearningModelServerStartup
            .BeforeReadinessSnapshotAllocationObserverForTesting;
        try
        {
            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = () =>
                throw fatal;

            var thrown = Assert.Throws<OutOfMemoryException>(
                association.Dispose);

            Assert.Same(fatal, thrown);
            Assert.True(association.Capability.IsActive);
            Assert.Same(readinessBefore, startup.Readiness);
            Assert.Equal(
                lifecycleStateBefore,
                lifecycleStateField.GetValue(startup));
            Assert.Same(retirementBefore, startup.AwaitRetirementAsync());
            Assert.True(retirementBefore.IsCompletedSuccessfully);
            Assert.Same(
                runtime,
                DesignConversionDpoTeacherAttestationAssociation
                    .ResolveExactProvider(gateway));
            Assert.True(gateway.IsBound);

            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = null;
            association.Dispose();

            Assert.False(association.Capability.IsActive);
            Assert.Null(DesignConversionDpoTeacherAttestationAssociation
                .ResolveExactProvider(gateway));
            var retirementAfter = startup.AwaitRetirementAsync();
            Assert.NotSame(retirementBefore, retirementAfter);
            Assert.False(startup.TryRegisterRetirementDependency(
                static () => Task.CompletedTask));
            await Task.WhenAll(
                    retirementAfter,
                    gateway.AwaitRetirementAsync())
                .WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = prior;
        }
    }


    [Fact]
    public async Task HostedTeacherDisposal_PrecommitFailureRetainsResourcesAndRetryDoesNotHang()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment(
            trainingActivationRequested: true);
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("hosted-disposal-readiness-retry"));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        await app.StartAsync();
        var hosted = Assert.Single(
            app.Services.GetServices<IHostedService>()
                .OfType<
                    DesignConversionDpoTeacherAttestationHostedAssociation>());
        var associationField = typeof(
                DesignConversionDpoTeacherAttestationHostedAssociation)
            .GetField(
                "_association",
                BindingFlags.Instance | BindingFlags.NonPublic)!;
        var runtimeField = typeof(
                DesignConversionDpoTeacherAttestationHostedAssociation)
            .GetField(
                "_runtime",
                BindingFlags.Instance | BindingFlags.NonPublic)!;
        var association = Assert.IsType<
            DesignConversionDpoTeacherAttestationAssociation
                .TrainingCapabilityLease>(associationField.GetValue(hosted));
        var runtime = Assert.IsType<
            DesignConversionDpoTeacherAttestationRuntime>(
                runtimeField.GetValue(hosted));
        var readinessBefore = startup.Readiness;
        var retirementBefore = startup.AwaitRetirementAsync();
        var fatal = new OutOfMemoryException(
            "hosted-disposal-readiness-allocation");
        var prior = ContinuousLearningModelServerStartup
            .BeforeReadinessSnapshotAllocationObserverForTesting;
        try
        {
            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = () =>
                throw fatal;

            var failedAttempt = hosted.StopAsync(CancellationToken.None);
            var thrown = await Assert.ThrowsAsync<OutOfMemoryException>(
                () => failedAttempt.WaitAsync(TimeSpan.FromSeconds(5)));

            Assert.Same(fatal, thrown);
            Assert.True(failedAttempt.IsCompleted);
            Assert.Same(association, associationField.GetValue(hosted));
            Assert.Same(runtime, runtimeField.GetValue(hosted));
            Assert.True(association.Capability.IsActive);
            Assert.Same(readinessBefore, startup.Readiness);
            Assert.Same(retirementBefore, startup.AwaitRetirementAsync());
            Assert.True(retirementBefore.IsCompletedSuccessfully);
            Assert.Same(
                runtime,
                DesignConversionDpoTeacherAttestationAssociation
                    .ResolveExactProvider(gateway));
            Assert.True(gateway.IsBound);

            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = null;
            var retry = hosted.StopAsync(CancellationToken.None);

            Assert.NotSame(failedAttempt, retry);
            await retry.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Null(associationField.GetValue(hosted));
            Assert.Null(runtimeField.GetValue(hosted));
            Assert.Null(DesignConversionDpoTeacherAttestationAssociation
                .ResolveExactProvider(gateway));
            Assert.False(gateway.IsBound);
        }
        finally
        {
            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = prior;
        }
    }


    [Fact]
    public async Task ProductionHost_ConsumedTeacherRequestFailureRemovesPartialRegistryAssociation()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment(
            trainingActivationRequested: true);
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("teacher-partial-association-cleanup"));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        await app.StartAsync();
        var hosted = Assert.Single(
            app.Services.GetServices<IHostedService>()
                .OfType<
                    DesignConversionDpoTeacherAttestationHostedAssociation>());
        var association = Assert.IsType<
            DesignConversionDpoTeacherAttestationAssociation
                .TrainingCapabilityLease>(
                    typeof(
                            DesignConversionDpoTeacherAttestationHostedAssociation)
                        .GetField(
                            "_association",
                            BindingFlags.Instance | BindingFlags.NonPublic)!
                        .GetValue(hosted));
        var runtime = Assert.IsType<
            DesignConversionDpoTeacherAttestationRuntime>(
                typeof(
                        DesignConversionDpoTeacherAttestationHostedAssociation)
                    .GetField(
                        "_runtime",
                        BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(hosted));
        var consumedIssuer = association.Capability.AssociationIssuer;

        association.Dispose();

        Assert.Null(DesignConversionDpoTeacherAttestationAssociation
            .ResolveExactProvider(gateway));
        Assert.Throws<InvalidOperationException>(() =>
            DesignConversionDpoTeacherAttestationAssociation.Associate(
                gateway,
                runtime,
                consumedIssuer));
        Assert.Null(DesignConversionDpoTeacherAttestationAssociation
            .ResolveExactProvider(gateway));
        Assert.False(startup.Readiness.TrainingEnabled);
        Assert.False(startup.Readiness.CycleExecutionAvailable);
        Assert.False(startup.Readiness.PerformsTraining);
        Assert.False(startup.Readiness.ProductReady);
        await startup.AwaitRetirementAsync()
            .WaitAsync(TimeSpan.FromSeconds(5));
        await gateway.AwaitRetirementAsync()
            .WaitAsync(TimeSpan.FromSeconds(5));
    }


    [Fact]
    public async Task ProductionHost_TeacherAssociationFailureRemovesBindingBeforeRetirementStartsAndPreservesFailureStatus()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment(
            trainingActivationRequested: true);
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("teacher-association-phase-order"));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        var retainedActivator = Assert.Single(
            app.Services.GetServices<IHostedService>()
                .OfType<ContinuousLearningRetainedObjectHostedActivator>());
        var teacherAssociation = Assert.Single(
            app.Services.GetServices<IHostedService>()
                .OfType<
                    DesignConversionDpoTeacherAttestationHostedAssociation>());
        var original = new InvalidOperationException(
            "teacher-association-failure-for-phase-order");
        var withdrawalPhaseObserved = false;
        var phaseObservedBinding = false;
        var phaseObservedRetirementStarted = false;
        var retirementStartObservedBinding = false;
        Task? startupRetirementDuringPhaseOne = null;
        Task? publishedStartupRetirement = null;
        var priorAssociationObserver =
            DesignConversionDpoTeacherAttestationHostedAssociation
                .AfterRuntimeCreatedBeforeAssociationForTesting;
        var priorWithdrawalObserver =
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ReadinessWithdrawnBeforeBindingRemovedObserverForTesting;
        var priorRetirementObserver = ContinuousLearningModelServerStartup
            .RetirementTaskPublishedObserverForTesting;
        var retirementStarted = false;
        try
        {
            DesignConversionDpoTeacherAttestationHostedAssociation
                .AfterRuntimeCreatedBeforeAssociationForTesting = () =>
                throw original;
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ReadinessWithdrawnBeforeBindingRemovedObserverForTesting =
                () =>
                {
                    withdrawalPhaseObserved = true;
                    phaseObservedBinding = gateway.IsBound;
                    phaseObservedRetirementStarted = retirementStarted;
                    startupRetirementDuringPhaseOne =
                        startup.AwaitRetirementAsync();
                };
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = retirement =>
                {
                    retirementStarted = true;
                    retirementStartObservedBinding = gateway.IsBound;
                    publishedStartupRetirement = retirement;
                };

            await retainedActivator.StartAsync(CancellationToken.None);
            Assert.True(gateway.IsBound);
            var thrown = await Assert.ThrowsAsync<InvalidOperationException>(
                () => teacherAssociation.StartAsync(CancellationToken.None));

            Assert.Same(original, thrown);
            Assert.True(withdrawalPhaseObserved);
            Assert.True(phaseObservedBinding);
            Assert.False(phaseObservedRetirementStarted);
            Assert.False(retirementStartObservedBinding);
            Assert.False(gateway.IsBound);
            Assert.NotNull(startupRetirementDuringPhaseOne);
            Assert.Same(
                startupRetirementDuringPhaseOne,
                publishedStartupRetirement);
            Assert.Equal(
                ContinuousLearningModelServerStartup
                    .DesignConversionTrainingIntakeTeacherAssociationFailedErrorCode,
                startup.Readiness.ConfigurationErrorCode);
            Assert.Equal(
                "design_conversion_training_intake_teacher_association_failed_inactive",
                startup.Readiness.ConfigurationStatus);
            Assert.False(startup.Readiness.StableDpoExecutorConfigured);
            Assert.False(startup.Readiness.TrainingEnabled);
            Assert.False(startup.Readiness.CycleExecutionAvailable);
            Assert.False(startup.Readiness.PerformsTraining);
            Assert.False(startup.Readiness.ProductReady);
            await Task.WhenAll(
                    startup.AwaitRetirementAsync(),
                    gateway.AwaitRetirementAsync())
                .WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            DesignConversionDpoTeacherAttestationHostedAssociation
                .AfterRuntimeCreatedBeforeAssociationForTesting =
                priorAssociationObserver;
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ReadinessWithdrawnBeforeBindingRemovedObserverForTesting =
                priorWithdrawalObserver;
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting =
                priorRetirementObserver;
        }
    }


    [Fact]
    public async Task ProgramMappedRouteAndBoundGateway_RejectCallerAssertedTeacherCapabilitySubclass()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment(
            trainingActivationRequested: true);
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("caller-asserted-teacher-capability"));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        var retainedActivator = Assert.Single(
            app.Services.GetServices<IHostedService>()
                .OfType<ContinuousLearningRetainedObjectHostedActivator>());
        await retainedActivator.StartAsync(CancellationToken.None);
        Assert.True(gateway.IsBound);
        Assert.True(startup.Readiness.CompositionMaterialized);
        Assert.True(startup.Readiness.StableDpoExecutorConfigured);
        var teacherIssuer = startup
            .PrepareDesignConversionTeacherAssociationIssuer(gateway);
        Assert.NotNull(teacherIssuer);
        var callerAssertedCapability =
            new CallerAssertedTeacherAssociationCapability(
                teacherIssuer!,
                gateway,
                new object(),
                new object());

        Assert.False(startup
            .TryMarkDesignConversionTrainingIntakeTeacherRuntimeAssociated(
                callerAssertedCapability));
        Assert.True(gateway.IsBound);
        Assert.True(startup.Readiness.StableDpoExecutorConfigured);
        Assert.False(startup.Readiness.TrainingEnabled);
        Assert.False(startup.Readiness.CycleExecutionAvailable);
        Assert.False(startup.Readiness.PerformsTraining);
        Assert.False(startup.Readiness.ProductReady);
        Assert.False(startup.IsLegalTrainingTeacherAdmissionActive());
        gateway.Close();
        await gateway.AwaitRetirementAsync()
            .WaitAsync(TimeSpan.FromSeconds(5));
    }


    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task GatewayWithdrawal_WithdrawsReadinessBeforeUnbindAndDrainsProviderAndLegalPermits(
        bool close,
        bool upgradeToClose)
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment(
            trainingActivationRequested: true);
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments(close
                ? "withdraw-close-interleaving"
                : upgradeToClose
                    ? "withdraw-unbind-close-upgrade-interleaving"
                    : "withdraw-unbind-interleaving"));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        var authority = app.Services.GetRequiredService<
            ServerOwnedLegalTrainingTeacherAdmissionAuthority>();
        await app.StartAsync();

        var legalLeaseResult = authority.AcquireTrainingLease(
            [CreateLegalTeacherProvenance(
                fixture.ModelId,
                fixture.OllamaManifestDigest,
                "withdraw-held-lease")]);
        Assert.True(legalLeaseResult.IsSuccess, legalLeaseResult.ErrorMessage);
        var legalLease = Assert.IsType<
            ServerOwnedLegalTrainingTeacherAdmissionLease>(
                legalLeaseResult.Value);
        var legalPermitResult = legalLease.AcquireMutationPermit();
        Assert.True(legalPermitResult.IsSuccess, legalPermitResult.ErrorMessage);
        var legalPermit = Assert.IsAssignableFrom<IDisposable>(
            legalPermitResult.Value);
        var providerLeaseHeld = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseProviderLease = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var withdrawalPhaseObserved = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseWithdrawalPhase = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>?
            providerInvocation = null;
        Task? withdrawal = null;
        try
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .InvocationLeaseAcquiredObserverForTesting = () =>
                {
                    providerLeaseHeld.TrySetResult(null);
                    return releaseProviderLease.Task;
                };
            providerInvocation = gateway.CreateModelSetAsync(
                CancellationToken.None);
            await providerLeaseHeld.Task.WaitAsync(TimeSpan.FromSeconds(5));

            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ReadinessWithdrawnBeforeBindingRemovedObserverForTesting =
                () =>
                {
                    withdrawalPhaseObserved.TrySetResult(null);
                    releaseWithdrawalPhase.Task.GetAwaiter().GetResult();
                };
            withdrawal = Task.Run(() =>
            {
                if (close)
                    gateway.Close();
                else
                    gateway.Unbind();
            });
            await withdrawalPhaseObserved.Task.WaitAsync(
                TimeSpan.FromSeconds(5));

            Assert.False(startup.Readiness.TrainingEnabled);
            Assert.False(startup.Readiness.CycleExecutionAvailable);
            Assert.False(startup.Readiness.PerformsTraining);
            Assert.False(startup.Readiness.ProductReady);
            Assert.False(startup.IsLegalTrainingTeacherAdmissionActive());
            Assert.True(gateway.IsBound);
            var retirementPublishedDuringPhaseOne =
                gateway.AwaitRetirementAsync();
            Assert.False(retirementPublishedDuringPhaseOne.IsCompleted);
            if (upgradeToClose)
                gateway.Close();
            Assert.Same(
                retirementPublishedDuringPhaseOne,
                gateway.AwaitRetirementAsync());
            var admissionAfterWithdrawal = authority.AcquireTrainingLease(
                [CreateLegalTeacherProvenance(
                    fixture.ModelId,
                    fixture.OllamaManifestDigest,
                    "withdraw-new-admission-denied")]);
            Assert.False(admissionAfterWithdrawal.IsSuccess);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority
                    .ConfigurationRequiredErrorCode,
                admissionAfterWithdrawal.ErrorCode);

            releaseWithdrawalPhase.TrySetResult(null);
            await withdrawal.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(gateway.IsBound);
            var startupRetirement = startup.AwaitRetirementAsync();
            var gatewayRetirement = gateway.AwaitRetirementAsync();
            Assert.Same(
                retirementPublishedDuringPhaseOne,
                gatewayRetirement);
            Assert.False(startupRetirement.IsCompleted);
            Assert.False(gatewayRetirement.IsCompleted);

            releaseProviderLease.TrySetResult(null);
            var invocationResult = await providerInvocation.WaitAsync(
                TimeSpan.FromSeconds(5));
            Assert.False(invocationResult.IsSuccess);
            Assert.Equal(
                DesignConversionRetainedDpoPolicyBaseProviderGateway
                    .ProviderUnavailableErrorCode,
                invocationResult.ErrorCode);
            Assert.False(startupRetirement.IsCompleted);
            Assert.False(gatewayRetirement.IsCompleted);

            legalPermit.Dispose();
            await Task.WhenAll(startupRetirement, gatewayRetirement)
                .WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(startupRetirement.IsCompletedSuccessfully);
            Assert.True(gatewayRetirement.IsCompletedSuccessfully);
        }
        finally
        {
            releaseWithdrawalPhase.TrySetResult(null);
            releaseProviderLease.TrySetResult(null);
            legalPermit.Dispose();
            legalLease.Dispose();
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .InvocationLeaseAcquiredObserverForTesting = null;
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ReadinessWithdrawnBeforeBindingRemovedObserverForTesting =
                null;
            if (withdrawal is not null)
                await withdrawal.WaitAsync(TimeSpan.FromSeconds(5));
            if (providerInvocation is not null)
                await providerInvocation.WaitAsync(TimeSpan.FromSeconds(5));
        }
    }


    [Fact]
    public async Task HostedStopDuringGatewayPreTicketPhaseCannotStartRetirementBeforeBindingRemoval()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment(
            trainingActivationRequested: true);
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("hosted-stop-pre-ticket-order"));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        var activator = Assert.Single(
            app.Services.GetServices<IHostedService>()
                .OfType<ContinuousLearningRetainedObjectHostedActivator>());
        await app.StartAsync();

        var preTicketPhaseEntered = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releasePreTicketPhase = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var readinessPhaseEntered = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseReadinessPhase = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var hostedStopStarted = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var retirementStartObserved = false;
        var retirementStartObservedBinding = false;
        Task? withdrawal = null;
        Task? hostedStop = null;
        var priorPreTicketObserver =
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeStartupReadinessWithdrawalObserverForTesting;
        var priorRetirementObserver = ContinuousLearningModelServerStartup
            .RetirementTaskPublishedObserverForTesting;
        var priorReadinessObserver =
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ReadinessWithdrawnBeforeBindingRemovedObserverForTesting;
        try
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeStartupReadinessWithdrawalObserverForTesting = () =>
                {
                    preTicketPhaseEntered.TrySetResult(null);
                    releasePreTicketPhase.Task.GetAwaiter().GetResult();
                };
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = _ =>
                {
                    retirementStartObserved = true;
                    retirementStartObservedBinding = gateway.IsBound;
                };
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ReadinessWithdrawnBeforeBindingRemovedObserverForTesting =
                () =>
                {
                    readinessPhaseEntered.TrySetResult(null);
                    releaseReadinessPhase.Task.GetAwaiter().GetResult();
                };

            withdrawal = Task.Run(gateway.Unbind);
            await preTicketPhaseEntered.Task.WaitAsync(
                TimeSpan.FromSeconds(5));
            hostedStop = Task.Run(async () =>
            {
                hostedStopStarted.TrySetResult(null);
                await activator.StopAsync(CancellationToken.None);
            });
            await hostedStopStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await Task.Yield();

            Assert.True(gateway.IsBound);
            Assert.False(retirementStartObserved);
            Assert.False(hostedStop.IsCompleted);

            releasePreTicketPhase.TrySetResult(null);
            await readinessPhaseEntered.Task.WaitAsync(
                TimeSpan.FromSeconds(5));
            Assert.True(SpinWait.SpinUntil(
                () => startup.Readiness.ConfigurationErrorCode
                      == ContinuousLearningModelServerStartup
                          .DpoPolicyBaseStoppedErrorCode,
                TimeSpan.FromSeconds(5)));
            Assert.True(gateway.IsBound);
            Assert.False(retirementStartObserved);
            Assert.False(hostedStop.IsCompleted);

            releaseReadinessPhase.TrySetResult(null);
            await Task.WhenAll(withdrawal, hostedStop)
                .WaitAsync(TimeSpan.FromSeconds(5));

            Assert.True(retirementStartObserved);
            Assert.False(retirementStartObservedBinding);
            Assert.False(gateway.IsBound);
            Assert.Equal(
                ContinuousLearningModelServerStartup
                    .DpoPolicyBaseStoppedErrorCode,
                startup.Readiness.ConfigurationErrorCode);
        }
        finally
        {
            releasePreTicketPhase.TrySetResult(null);
            releaseReadinessPhase.TrySetResult(null);
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeStartupReadinessWithdrawalObserverForTesting =
                priorPreTicketObserver;
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting =
                priorRetirementObserver;
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ReadinessWithdrawnBeforeBindingRemovedObserverForTesting =
                priorReadinessObserver;
            if (withdrawal is not null)
                await withdrawal.WaitAsync(TimeSpan.FromSeconds(5));
            if (hostedStop is not null)
                await hostedStop.WaitAsync(TimeSpan.FromSeconds(5));
        }
    }


    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HostedStopDuringGatewayPreTicketFailureNeverHangsOrStartsRetirementWithoutWithdrawal(
        bool retrySucceeds)
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment(
            trainingActivationRequested: true);
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("hosted-stop-pre-ticket-failure"));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        var activator = Assert.Single(
            app.Services.GetServices<IHostedService>()
                .OfType<ContinuousLearningRetainedObjectHostedActivator>());
        await app.StartAsync();

        var firstFatal = new OutOfMemoryException(
            "hosted-stop-pre-ticket-first-failure");
        var secondFatal = new OutOfMemoryException(
            "hosted-stop-pre-ticket-second-failure");
        var withdrawalAttempts = 0;
        var retirementPublishCount = 0;
        var stoppedRetirementPublishedBeforeBindingRemoved = false;
        Task<bool>? firstAttemptOutcome = null;
        var priorPreTicketObserver =
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeStartupReadinessWithdrawalObserverForTesting;
        var priorRetirementObserver = ContinuousLearningModelServerStartup
            .RetirementTaskPublishedObserverForTesting;
        try
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeStartupReadinessWithdrawalObserverForTesting = () =>
                {
                    var attempt = Interlocked.Increment(
                        ref withdrawalAttempts);
                    if (attempt == 1)
                    {
                        firstAttemptOutcome =
                            gateway.AwaitStartupWithdrawalBegunAsync();
                    }
                    if (attempt == 1 || !retrySucceeds)
                        throw attempt == 1 ? firstFatal : secondFatal;
                };
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = _ =>
                {
                    Interlocked.Increment(ref retirementPublishCount);
                    stoppedRetirementPublishedBeforeBindingRemoved |=
                        gateway.IsBound
                        && startup.Readiness.ConfigurationErrorCode
                            == ContinuousLearningModelServerStartup
                                .DpoPolicyBaseStoppedErrorCode;
                };

            var stop = activator.StopAsync(CancellationToken.None);
            if (retrySucceeds)
            {
                await stop.WaitAsync(TimeSpan.FromSeconds(5));
            }
            else
            {
                var thrown = await Assert.ThrowsAsync<OutOfMemoryException>(
                    () => stop.WaitAsync(TimeSpan.FromSeconds(5)));
                Assert.Same(firstFatal, thrown);
            }

            Assert.Equal(2, withdrawalAttempts);
            Assert.NotNull(firstAttemptOutcome);
            Assert.False(await firstAttemptOutcome!
                .WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.False(stoppedRetirementPublishedBeforeBindingRemoved);
            if (retrySucceeds)
            {
                Assert.False(gateway.IsBound);
                Assert.True(retirementPublishCount > 0);
                Assert.Equal(
                    ContinuousLearningModelServerStartup
                        .DpoPolicyBaseStoppedErrorCode,
                    startup.Readiness.ConfigurationErrorCode);
                await Task.WhenAll(
                        startup.AwaitRetirementAsync(),
                        gateway.AwaitRetirementAsync())
                    .WaitAsync(TimeSpan.FromSeconds(5));
            }
            else
            {
                Assert.False(gateway.IsBound);
                Assert.NotEqual(
                    ContinuousLearningModelServerStartup
                        .DpoPolicyBaseStoppedErrorCode,
                    startup.Readiness.ConfigurationErrorCode);
            }
        }
        finally
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeStartupReadinessWithdrawalObserverForTesting =
                priorPreTicketObserver;
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting =
                priorRetirementObserver;
            if (gateway.IsBound)
            {
                gateway.Close();
                await gateway.AwaitRetirementAsync()
                    .WaitAsync(TimeSpan.FromSeconds(5));
            }
        }
    }


    [Fact]
    public async Task GatewayInvocation_LeaseAllocationFailureDoesNotLeakFutureRetirementDrain()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment();
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("gateway-invocation-lease-allocation-failure"));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        await app.StartAsync();

        var fatal = new OutOfMemoryException(
            "fatal-during-gateway-invocation-lease-allocation");
        var priorObserver =
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeInvocationLeaseAllocationObserverForTesting;
        try
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeInvocationLeaseAllocationObserverForTesting = () =>
                throw fatal;

            var thrown = await Assert.ThrowsAsync<OutOfMemoryException>(() =>
                gateway.CreateModelSetAsync(CancellationToken.None));

            Assert.Same(fatal, thrown);
            Assert.True(gateway.IsBound);
            Assert.True(startup.Readiness.StableDpoExecutorConfigured);
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeInvocationLeaseAllocationObserverForTesting = null;
            gateway.Close();
            await Task.WhenAll(
                    startup.AwaitRetirementAsync(),
                    gateway.AwaitRetirementAsync())
                .WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(gateway.IsBound);
        }
        finally
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeInvocationLeaseAllocationObserverForTesting = null;
            if (gateway.IsBound)
            {
                gateway.Close();
                await gateway.AwaitRetirementAsync()
                    .WaitAsync(TimeSpan.FromSeconds(5));
            }
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeInvocationLeaseAllocationObserverForTesting =
                priorObserver;
        }
    }


    [Fact]
    public async Task GatewayWithdrawal_ActiveInvocationDrainAllocationFailureLeavesBindingRecoverable()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment();
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("gateway-invocation-drain-allocation-failure"));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        await app.StartAsync();

        var invocationLeaseHeld = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseInvocation = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var fatal = new OutOfMemoryException(
            "fatal-during-active-invocation-drain-allocation");
        var initialGatewayRetirement = gateway.AwaitRetirementAsync();
        var priorInvocationObserver =
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .InvocationLeaseAcquiredObserverForTesting;
        var priorDrainObserver =
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeInvocationDrainAllocationObserverForTesting;
        Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>?
            invocation = null;
        try
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .InvocationLeaseAcquiredObserverForTesting = async () =>
                {
                    invocationLeaseHeld.TrySetResult(null);
                    await releaseInvocation.Task;
                };
            invocation = gateway.CreateModelSetAsync(CancellationToken.None);
            await invocationLeaseHeld.Task.WaitAsync(TimeSpan.FromSeconds(5));
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeInvocationDrainAllocationObserverForTesting = () =>
                throw fatal;

            var thrown = Assert.Throws<OutOfMemoryException>(gateway.Unbind);

            Assert.Same(fatal, thrown);
            Assert.True(gateway.IsBound);
            Assert.True(startup.Readiness.StableDpoExecutorConfigured);
            Assert.True(startup.AwaitRetirementAsync().IsCompletedSuccessfully);
            Assert.Same(initialGatewayRetirement, gateway.AwaitRetirementAsync());
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeInvocationDrainAllocationObserverForTesting = null;
            gateway.Close();
            Assert.False(gateway.IsBound);
            Assert.False(gateway.AwaitRetirementAsync().IsCompleted);

            releaseInvocation.TrySetResult(null);
            await invocation.WaitAsync(TimeSpan.FromSeconds(5));
            await Task.WhenAll(
                    startup.AwaitRetirementAsync(),
                    gateway.AwaitRetirementAsync())
                .WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            releaseInvocation.TrySetResult(null);
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .InvocationLeaseAcquiredObserverForTesting =
                priorInvocationObserver;
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeInvocationDrainAllocationObserverForTesting =
                priorDrainObserver;
            if (invocation is not null)
                await invocation.WaitAsync(TimeSpan.FromSeconds(5));
            if (gateway.IsBound)
            {
                gateway.Close();
                await gateway.AwaitRetirementAsync()
                    .WaitAsync(TimeSpan.FromSeconds(5));
            }
        }
    }


    [Fact]
    public async Task GatewayWithdrawal_RetirementWorkAllocationFailureCannotRemoveBinding()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment();
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("gateway-retirement-work-allocation-failure"));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        await app.StartAsync();

        var fatal = new OutOfMemoryException(
            "fatal-before-gateway-retirement-work-allocation");
        var initialGatewayRetirement = gateway.AwaitRetirementAsync();
        var priorObserver =
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeRetirementWorkAllocationObserverForTesting;
        try
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeRetirementWorkAllocationObserverForTesting = () =>
                throw fatal;

            var thrown = Assert.Throws<OutOfMemoryException>(gateway.Unbind);

            Assert.Same(fatal, thrown);
            Assert.True(gateway.IsBound);
            Assert.True(startup.Readiness.StableDpoExecutorConfigured);
            Assert.True(startup.AwaitRetirementAsync().IsCompletedSuccessfully);
            Assert.Same(initialGatewayRetirement, gateway.AwaitRetirementAsync());
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeRetirementWorkAllocationObserverForTesting = null;
            gateway.Close();
            await Task.WhenAll(
                    startup.AwaitRetirementAsync(),
                    gateway.AwaitRetirementAsync())
                .WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(gateway.IsBound);
        }
        finally
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeRetirementWorkAllocationObserverForTesting = null;
            if (gateway.IsBound)
            {
                gateway.Close();
                await gateway.AwaitRetirementAsync()
                    .WaitAsync(TimeSpan.FromSeconds(5));
            }
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeRetirementWorkAllocationObserverForTesting =
                priorObserver;
        }
    }


    [Fact]
    public async Task GatewayWithdrawal_RetirementPublicationFailureLeavesBindingRecoverableByLaterClose()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment();
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("gateway-retirement-publication-failure"));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        await app.StartAsync();

        var fatal = new OutOfMemoryException(
            "fatal-during-gateway-retirement-publication");
        var initialGatewayRetirement = gateway.AwaitRetirementAsync();
        var priorObserver =
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforePendingRetirementPublicationObserverForTesting;
        try
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforePendingRetirementPublicationObserverForTesting = () =>
                throw fatal;

            var thrown = Assert.Throws<OutOfMemoryException>(gateway.Unbind);

            Assert.Same(fatal, thrown);
            Assert.True(gateway.IsBound);
            Assert.True(startup.Readiness.StableDpoExecutorConfigured);
            Assert.True(startup.AwaitRetirementAsync().IsCompletedSuccessfully);
            Assert.Same(initialGatewayRetirement, gateway.AwaitRetirementAsync());
            Assert.True(initialGatewayRetirement.IsCompletedSuccessfully);

            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforePendingRetirementPublicationObserverForTesting = null;
            gateway.Close();

            Assert.False(gateway.IsBound);
            var recoveryRetirement = gateway.AwaitRetirementAsync();
            Assert.NotSame(initialGatewayRetirement, recoveryRetirement);
            await Task.WhenAll(
                    startup.AwaitRetirementAsync(),
                    recoveryRetirement)
                .WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(recoveryRetirement.IsCompletedSuccessfully);
        }
        finally
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforePendingRetirementPublicationObserverForTesting = null;
            if (gateway.IsBound)
            {
                gateway.Close();
                await gateway.AwaitRetirementAsync()
                    .WaitAsync(TimeSpan.FromSeconds(5));
            }
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforePendingRetirementPublicationObserverForTesting =
                priorObserver;
        }
    }


    [Fact]
    public async Task GatewayWithdrawal_FatalBeforeStartupTicketLeavesBindingRecoverableByLaterClose()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment();
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("gateway-fatal-before-startup-ticket"));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        await app.StartAsync();

        var fatal = new OutOfMemoryException(
            "fatal-after-gateway-retirement-publication-before-startup-ticket");
        var priorObserver =
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeStartupReadinessWithdrawalObserverForTesting;
        try
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeStartupReadinessWithdrawalObserverForTesting = () =>
                throw fatal;

            var thrown = Assert.Throws<OutOfMemoryException>(gateway.Unbind);

            Assert.Same(fatal, thrown);
            Assert.True(gateway.IsBound);
            Assert.True(startup.Readiness.StableDpoExecutorConfigured);
            Assert.True(startup.AwaitRetirementAsync().IsCompletedSuccessfully);
            var publishedGatewayRetirement = gateway.AwaitRetirementAsync();
            Assert.False(publishedGatewayRetirement.IsCompleted);

            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeStartupReadinessWithdrawalObserverForTesting = null;
            gateway.Close();

            Assert.False(gateway.IsBound);
            Assert.Same(
                publishedGatewayRetirement,
                gateway.AwaitRetirementAsync());
            await Task.WhenAll(
                    startup.AwaitRetirementAsync(),
                    publishedGatewayRetirement)
                .WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(publishedGatewayRetirement.IsCompletedSuccessfully);
        }
        finally
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeStartupReadinessWithdrawalObserverForTesting = null;
            if (gateway.IsBound)
            {
                gateway.Close();
                await gateway.AwaitRetirementAsync()
                    .WaitAsync(TimeSpan.FromSeconds(5));
            }
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeStartupReadinessWithdrawalObserverForTesting =
                priorObserver;
        }
    }


    [Fact]
    public async Task StartupRetirement_IsPublishedAndNonBlockingUntilActivePermitDrains()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment(
            trainingActivationRequested: true);
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("startup-retirement-active-permit"));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var authority = app.Services.GetRequiredService<
            ServerOwnedLegalTrainingTeacherAdmissionAuthority>();
        await app.StartAsync();

        var leaseResult = authority.AcquireTrainingLease(
            [CreateLegalTeacherProvenance(
                fixture.ModelId,
                fixture.OllamaManifestDigest,
                "startup-retirement-active-permit")]);
        Assert.True(leaseResult.IsSuccess, leaseResult.ErrorMessage);
        using var lease = Assert.IsType<
            ServerOwnedLegalTrainingTeacherAdmissionLease>(leaseResult.Value);
        var permitResult = lease.AcquireMutationPermit();
        Assert.True(permitResult.IsSuccess, permitResult.ErrorMessage);
        using var permit = Assert.IsAssignableFrom<IDisposable>(
            permitResult.Value);
        Task? published = null;
        var prior = ContinuousLearningModelServerStartup
            .RetirementTaskPublishedObserverForTesting;
        try
        {
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = task =>
                    published = task;
            var callback = Task.Run(startup.MarkDpoPolicyBaseStopped);
            await callback.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.NotNull(published);
            Assert.False(published!.IsCompleted);
            Assert.Equal(
                ServerOwnedLegalTrainingTeacherAdmissionAuthority
                    .AdmissionRevokedErrorCode,
                lease.AcquireMutationPermit().ErrorCode);

            permit.Dispose();
            await published.WaitAsync(TimeSpan.FromSeconds(5));
            await startup.AwaitRetirementAsync()
                .WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = prior;
        }
    }


    [Fact]
    public async Task StartupRetirement_ThrowingPublicationObserverStillRevokesAndDrains()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment(
            trainingActivationRequested: true);
        var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("startup-retirement-observer-failure"));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        var authority = app.Services.GetRequiredService<
            ServerOwnedLegalTrainingTeacherAdmissionAuthority>();
        var observerFailure = new InvalidOperationException(
            "retirement-observer-failed");
        var prior = ContinuousLearningModelServerStartup
            .RetirementTaskPublishedObserverForTesting;
        try
        {
            await app.StartAsync();
            var leaseResult = authority.AcquireTrainingLease(
                [CreateLegalTeacherProvenance(
                    fixture.ModelId,
                    fixture.OllamaManifestDigest,
                    "startup-retirement-observer-failure")]);
            Assert.True(leaseResult.IsSuccess, leaseResult.ErrorMessage);
            using var lease = Assert.IsType<
                ServerOwnedLegalTrainingTeacherAdmissionLease>(
                    leaseResult.Value);
            var permitResult = lease.AcquireMutationPermit();
            Assert.True(permitResult.IsSuccess, permitResult.ErrorMessage);
            using var permit = Assert.IsAssignableFrom<IDisposable>(
                permitResult.Value);
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = _ =>
                    throw observerFailure;

            startup.MarkDpoPolicyBaseStopped();
            var retirement = startup.AwaitRetirementAsync();
            Assert.False(retirement.IsCompleted);
            Assert.False(gateway.IsBound);
            Assert.Equal(
                ServerOwnedLegalTrainingTeacherAdmissionAuthority
                    .AdmissionRevokedErrorCode,
                lease.AcquireMutationPermit().ErrorCode);

            permit.Dispose();
            var thrown = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await retirement.WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.Same(observerFailure, thrown);
            var gatewayThrown = await Assert.ThrowsAsync<
                InvalidOperationException>(async () =>
                await gateway.AwaitRetirementAsync()
                    .WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.Same(observerFailure, gatewayThrown);

            var stopThrown = await Record.ExceptionAsync(
                () => app.StopAsync());
            Assert.NotNull(stopThrown);
            if (stopThrown is AggregateException aggregate)
            {
                Assert.Contains(
                    aggregate.Flatten().InnerExceptions,
                    exception => ReferenceEquals(observerFailure, exception));
            }
            else
            {
                Assert.Same(observerFailure, stopThrown);
            }
            Assert.Equal(
                ContinuousLearningModelServerStartup
                    .DpoPolicyBaseStoppedErrorCode,
                startup.Readiness.ConfigurationErrorCode);
            Assert.NotEqual(
                ContinuousLearningModelServerStartup
                    .RetainedObjectIdentityLostErrorCode,
                startup.Readiness.ConfigurationErrorCode);
            fixture.AssertLeasedPathsRenameable();
        }
        finally
        {
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = prior;
            try
            {
                await app.DisposeAsync();
            }
            catch (Exception exception)
            {
                if (exception is AggregateException aggregate)
                {
                    Assert.Contains(
                        aggregate.Flatten().InnerExceptions,
                        inner => ReferenceEquals(observerFailure, inner));
                }
                else
                {
                    Assert.Same(observerFailure, exception);
                }
            }
        }
    }


    [Fact]
    public async Task GatewayWithdrawal_CorruptedActiveTokenUsesInvariantTicketAfterBindingRemovalAndDrainsCapturedInvocation()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment();
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("withdraw-rejected-ticket"));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        await app.StartAsync();

        var providerLeaseHeld = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseProviderLease = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var withdrawalPhaseObserved = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseWithdrawalPhase = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>?
            providerInvocation = null;
        Task? withdrawal = null;
        Task? startupRetirementDuringPhaseOne = null;
        var retirementStartObserved = false;
        try
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .InvocationLeaseAcquiredObserverForTesting = () =>
                {
                    providerLeaseHeld.TrySetResult(null);
                    return releaseProviderLease.Task;
                };
            providerInvocation = gateway.CreateModelSetAsync(
                CancellationToken.None);
            await providerLeaseHeld.Task.WaitAsync(TimeSpan.FromSeconds(5));
            typeof(ContinuousLearningModelServerStartup)
                .GetField(
                    "_activeDpoPolicyBaseBindingToken",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(startup, null);

            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = retirement =>
                {
                    retirementStartObserved = true;
                    Assert.Same(startupRetirementDuringPhaseOne, retirement);
                };
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ReadinessWithdrawnBeforeBindingRemovedObserverForTesting =
                () =>
                {
                    startupRetirementDuringPhaseOne =
                        startup.AwaitRetirementAsync();
                    withdrawalPhaseObserved.TrySetResult(null);
                    releaseWithdrawalPhase.Task.GetAwaiter().GetResult();
                };
            withdrawal = Task.Run(gateway.Unbind);
            await withdrawalPhaseObserved.Task.WaitAsync(
                TimeSpan.FromSeconds(5));

            Assert.True(gateway.IsBound);
            Assert.False(startupRetirementDuringPhaseOne!.IsCompleted);
            Assert.False(retirementStartObserved);
            Assert.False(startup.Readiness.StableDpoExecutorConfigured);
            Assert.False(startup.Readiness.TrainingEnabled);
            Assert.False(startup.Readiness.CycleExecutionAvailable);
            Assert.False(startup.Readiness.PerformsTraining);
            Assert.False(startup.Readiness.ProductReady);

            releaseWithdrawalPhase.TrySetResult(null);
            await withdrawal.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.False(gateway.IsBound);
            Assert.True(retirementStartObserved);
            var startupRetirement = startup.AwaitRetirementAsync();
            Assert.Same(
                startupRetirementDuringPhaseOne,
                startupRetirement);
            Assert.False(startupRetirement.IsCompleted);
            Assert.False(startup.Readiness.StableDpoExecutorConfigured);
            Assert.False(startup.Readiness.TrainingEnabled);
            Assert.False(startup.Readiness.CycleExecutionAvailable);
            Assert.False(startup.Readiness.PerformsTraining);
            Assert.False(startup.Readiness.ProductReady);
            var gatewayRetirement = gateway.AwaitRetirementAsync();
            Assert.False(gatewayRetirement.IsCompleted);
            releaseProviderLease.TrySetResult(null);
            var result = await providerInvocation.WaitAsync(
                TimeSpan.FromSeconds(5));
            Assert.False(result.IsSuccess);
            Assert.Equal(
                DesignConversionRetainedDpoPolicyBaseProviderGateway
                    .ProviderUnavailableErrorCode,
                result.ErrorCode);
            await Task.WhenAll(startupRetirement, gatewayRetirement)
                .WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(startupRetirement.IsCompletedSuccessfully);
            Assert.True(gatewayRetirement.IsCompletedSuccessfully);
        }
        finally
        {
            releaseWithdrawalPhase.TrySetResult(null);
            releaseProviderLease.TrySetResult(null);
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = null;
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .InvocationLeaseAcquiredObserverForTesting = null;
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ReadinessWithdrawnBeforeBindingRemovedObserverForTesting =
                null;
            if (withdrawal is not null)
                await withdrawal.WaitAsync(TimeSpan.FromSeconds(5));
            if (providerInvocation is not null)
                await providerInvocation.WaitAsync(TimeSpan.FromSeconds(5));
        }
    }


    [Fact]
    public async Task IndependentStartupWithdrawal_PublishesRetirementBeforeGatewayObservesAlreadyInactive()
    {
        using var fixture = new ModelServerEndpointFixture();
        using var environment = fixture.ApplyEnvironment();
        await using var app = Program.CreateLocalLoopbackHost(
            fixture.ReadyFileArguments("startup-withdrawal-publication"));
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var gateway = app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>();
        await app.StartAsync();
        var retirementObserverEntered = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseRetirementObserver = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        Task? publishedStartupRetirement = null;
        Task? independentWithdrawal = null;
        try
        {
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = retirement =>
                {
                    publishedStartupRetirement = retirement;
                    retirementObserverEntered.TrySetResult(null);
                    releaseRetirementObserver.Task.GetAwaiter().GetResult();
                };
            independentWithdrawal = Task.Run(
                startup.MarkDpoPolicyBaseStopped);
            await retirementObserverEntered.Task.WaitAsync(
                TimeSpan.FromSeconds(5));

            Assert.False(startup.Readiness.StableDpoExecutorConfigured);
            Assert.False(startup.Readiness.TrainingEnabled);
            Assert.NotNull(publishedStartupRetirement);
            Assert.Same(
                publishedStartupRetirement,
                startup.AwaitRetirementAsync());
            Assert.False(publishedStartupRetirement!.IsCompleted);

            gateway.Close();

            Assert.False(gateway.IsBound);
            var gatewayRetirement = gateway.AwaitRetirementAsync();
            Assert.False(gatewayRetirement.IsCompleted);
            releaseRetirementObserver.TrySetResult(null);
            await independentWithdrawal.WaitAsync(TimeSpan.FromSeconds(5));
            await Task.WhenAll(
                publishedStartupRetirement,
                gatewayRetirement).WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(
                publishedStartupRetirement.IsCompletedSuccessfully);
            Assert.True(gatewayRetirement.IsCompletedSuccessfully);
        }
        finally
        {
            releaseRetirementObserver.TrySetResult(null);
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = null;
            if (independentWithdrawal is not null)
                await independentWithdrawal.WaitAsync(
                    TimeSpan.FromSeconds(5));
        }
    }


    private sealed class CallerAssertedTeacherAssociationCapability
        : ContinuousLearningDesignConversionTeacherAssociationCapability
    {
        internal CallerAssertedTeacherAssociationCapability(
            ContinuousLearningDesignConversionTeacherAssociationCapability
                .Issuer issuer,
            object providerIdentity,
            object runtimeIdentity,
            object registrationIdentity)
            : base(
                issuer,
                providerIdentity,
                runtimeIdentity,
                registrationIdentity)
        {
        }
    }

}
