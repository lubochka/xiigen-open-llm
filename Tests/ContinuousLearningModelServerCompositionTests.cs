// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using DPO;
using LoRA;
using ModelServer;
using ModelServer.DesignConversion;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

public sealed partial class ContinuousLearningModelServerCompositionTests
{
    private const string RetainedPolicyModelId = "registry/qwen3.5-base-9b";
    private const string RetainedPolicyRevision = "base-checkpoint-r17";

    [Fact]
    public void LocalIndependentPreferenceEvaluator_DefaultsOffDespiteValidTp1Configuration()
    {
        using var fixture = new ModelServerCompositionFixture();

        var startup = CreateStartup(fixture.ValidEnvironment());

        Assert.True(startup.Readiness.ConfigurationAdmitted);
        Assert.False(startup.LocalIndependentPreferenceEvaluatorRequested);
    }

    [Fact]
    public void LocalIndependentPreferenceEvaluator_ExplicitTrueCapturesAdmittedTp1Identity()
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        environment[
            ContinuousLearningModelServerConfigurationLoader
                .LocalIndependentPreferenceEvaluatorEnabledEnvironmentName] =
            bool.TrueString;

        var startup = CreateStartup(environment);

        Assert.True(startup.Readiness.ConfigurationAdmitted);
        Assert.True(startup.LocalIndependentPreferenceEvaluatorRequested);
        Assert.Equal(
            environment[
                ContinuousLearningModelServerConfigurationLoader
                    .ModelIdEnvironmentName],
            startup.TeacherConfiguration.ModelId);
        Assert.Equal(
            environment[
                ContinuousLearningModelServerConfigurationLoader
                    .OllamaManifestDigestEnvironmentName],
            startup.TeacherConfiguration.ExpectedOllamaManifestDigest);
        Assert.Equal(
            environment[
                ContinuousLearningModelServerConfigurationLoader
                    .OllamaModelLayerDigestEnvironmentName],
            startup.TeacherConfiguration.ExpectedOllamaModelLayerDigest);
    }

    [Fact]
    public void LocalIndependentPreferenceEvaluator_ExplicitTrueWithoutCompositionFailsClosed()
    {
        var environment = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ContinuousLearningModelServerConfigurationLoader
                .EnabledEnvironmentName] = bool.FalseString,
            [ContinuousLearningModelServerConfigurationLoader
                .LocalIndependentPreferenceEvaluatorEnabledEnvironmentName] =
                bool.TrueString
        };

        var startup = CreateStartup(environment);

        Assert.False(startup.Readiness.ConfigurationAdmitted);
        Assert.False(startup.LocalIndependentPreferenceEvaluatorRequested);
        Assert.Equal(
            ContinuousLearningModelServerConfigurationLoader
                .LocalIndependentPreferenceEvaluatorRequiresCompositionErrorCode,
            startup.Readiness.ConfigurationErrorCode);
    }

    [Fact]
    public void LocalIndependentPreferenceEvaluator_InvalidBooleanFailsClosed()
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        environment[
            ContinuousLearningModelServerConfigurationLoader
                .LocalIndependentPreferenceEvaluatorEnabledEnvironmentName] =
            "enabled";

        var startup = CreateStartup(environment);

        Assert.False(startup.Readiness.ConfigurationAdmitted);
        Assert.False(startup.LocalIndependentPreferenceEvaluatorRequested);
        Assert.Equal(
            "CONTINUOUS_LEARNING_CONFIGURATION_INVALID",
            startup.Readiness.ConfigurationErrorCode);
    }

    [Fact]
    public void LegalTeacherAdmissionCapability_IsOpaqueAndUnavailableStartupFailsClosed()
    {
        var startup = ContinuousLearningModelServerStartup.Create(name =>
            name == ContinuousLearningModelServerConfigurationLoader
                .EnabledEnvironmentName
                ? bool.FalseString
                : throw new InvalidOperationException());

        var first = startup.LegalTeacherAdmissionAuthority;
        var second = startup.LegalTeacherAdmissionAuthority;
        var result = first.ValidateBatch(
            [TrainingTeacherProvenance.Deterministic("disabled-capability")]);
        var leaseResult = first.AcquireTrainingLease(
            [TrainingTeacherProvenance.Deterministic("disabled-lease")]);

        Assert.Same(first, second);
        Assert.False(first.IsAvailable);
        Assert.False(result.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            result.ErrorCode);
        Assert.Equal(
            "Server-owned legal teacher admission configuration is required.",
            result.ErrorMessage);
        Assert.False(leaseResult.IsSuccess);
        Assert.Null(leaseResult.Value);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            leaseResult.ErrorCode);

        var capabilityType =
            typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority);
        Assert.Empty(capabilityType.GetConstructors(BindingFlags.Public |
                                                    BindingFlags.Instance));
        Assert.DoesNotContain(
            capabilityType.GetMethods(BindingFlags.Public |
                                      BindingFlags.Static |
                                      BindingFlags.DeclaredOnly),
            method => method.ReturnType == capabilityType);
        Assert.DoesNotContain(
            capabilityType.GetMembers(BindingFlags.Public |
                                      BindingFlags.Instance |
                                      BindingFlags.Static),
            member => member switch
            {
                FieldInfo field => field.FieldType ==
                    typeof(LegalTrainingTeacherAdmissionAuthority),
                PropertyInfo property => property.PropertyType ==
                    typeof(LegalTrainingTeacherAdmissionAuthority),
                MethodInfo method => method.ReturnType ==
                    typeof(LegalTrainingTeacherAdmissionAuthority) ||
                    method.GetParameters().Any(parameter =>
                        parameter.ParameterType ==
                        typeof(LegalTrainingTeacherAdmissionAuthority)),
                _ => false,
            });
        Assert.All(
            capabilityType.GetFields(
                BindingFlags.Instance | BindingFlags.NonPublic),
            field => Assert.True(field.IsInitOnly));
    }

    [Fact]
    public void AdmittedStartup_CapturesOneStableButPreHostDeniedCapability()
    {
        using var fixture = new ModelServerCompositionFixture();
        var startup = CreateStartup(fixture.ValidEnvironment());

        var first = startup.LegalTeacherAdmissionAuthority;
        var second = startup.LegalTeacherAdmissionAuthority;
        var admission = first.ValidateBatch(
            [TrainingTeacherProvenance.Deterministic("admitted-capability")]);

        Assert.True(startup.Readiness.ConfigurationAdmitted);
        Assert.True(first.IsAvailable);
        Assert.Same(first, second);
        Assert.False(startup.IsLegalTrainingTeacherAdmissionActive());
        Assert.False(admission.IsSuccess);
        Assert.Null(admission.Value);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            admission.ErrorCode);
    }

    [Fact]
    public void LegalTeacherAdmissionCapability_SamplesInjectedLivePredicateOnceBeforeEnumeration()
    {
        var live = false;
        var predicateCallCount = 0;
        var validatorCallCount = 0;
        var enumerationCount = 0;
        IEnumerable<TrainingTeacherProvenance?> Provenance()
        {
            enumerationCount++;
            yield return TrainingTeacherProvenance.Deterministic(
                "authority-live-predicate-order");
        }

        var authority = ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
            provenance =>
            {
                validatorCallCount++;
                _ = provenance?.ToArray();
                return OperationResult<LegalTrainingTeacherAdmission>.Fail(
                    "The structural validator was invoked.",
                    "STRUCTURAL_VALIDATOR_INVOKED");
            },
            () =>
            {
                predicateCallCount++;
                return live;
            });

        var first = authority.ValidateBatch(Provenance());
        live = true;
        var second = authority.ValidateBatch(Provenance());
        live = false;
        var third = authority.ValidateBatch(Provenance());

        Assert.True(authority.IsAvailable);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            first.ErrorCode);
        Assert.Equal("STRUCTURAL_VALIDATOR_INVOKED", second.ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            third.ErrorCode);
        Assert.Equal(3, predicateCallCount);
        Assert.Equal(1, validatorCallCount);
        Assert.Equal(1, enumerationCount);
    }

    [Fact]
    public void LegalTeacherAdmissionCapability_SanitizesOnlyNonFatalPredicateAndValidatorFailures()
    {
        const string privateMarker = "C:/private/legal-authority-failure";
        var nonFatalPredicate =
            ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
                static _ => throw new InvalidOperationException(),
                () => throw new IOException(privateMarker));
        var fatalPredicate =
            ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
                static _ => throw new InvalidOperationException(),
                () => throw new OutOfMemoryException(privateMarker));
        var nonFatalValidator =
            ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
                _ => throw new IOException(privateMarker),
                static () => true);
        var fatalValidator =
            ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
                _ => throw new OutOfMemoryException(privateMarker),
                static () => true);

        var predicateFailure = nonFatalPredicate.ValidateBatch([]);
        var validatorFailure = nonFatalValidator.ValidateBatch([]);

        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            predicateFailure.ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            validatorFailure.ErrorCode);
        Assert.DoesNotContain(privateMarker, predicateFailure.ErrorMessage);
        Assert.DoesNotContain(privateMarker, validatorFailure.ErrorMessage);
        Assert.Throws<OutOfMemoryException>(
            () => fatalPredicate.ValidateBatch([]));
        Assert.Throws<OutOfMemoryException>(
            () => fatalValidator.ValidateBatch([]));
    }

    [Fact]
    public async Task LegalTeacherAdmissionLease_RevocationDrainsActivePermitAndRejectsLaterMutation()
    {
        var timeout = TimeSpan.FromSeconds(5);
        var authority = CreateRevocableAuthority(static () => true);
        using var lease = AssertSuccessfulLease(authority);
        var permit = lease.AcquireMutationPermit();
        Assert.True(permit.IsSuccess);
        Assert.NotNull(permit.Value);

        Task? drain = null;
        try
        {
            var beginInvocation = Task.Factory.StartNew(
                () => authority.BeginRevocation(),
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                TaskScheduler.Default);
            drain = await beginInvocation.WaitAsync(timeout);

            Assert.False(drain.IsCompleted);
            Assert.Equal(
                ServerOwnedLegalTrainingTeacherAdmissionAuthority
                    .AdmissionRevokedErrorCode,
                lease.AcquireMutationPermit().ErrorCode);
        }
        finally
        {
            permit.Value!.Dispose();
        }

        permit.Value.Dispose();
        Assert.NotNull(drain);
        await drain.WaitAsync(timeout);
        var repeatedInvocation = Task.Factory.StartNew(
            () => authority.BeginRevocation(),
            CancellationToken.None,
            TaskCreationOptions.DenyChildAttach,
            TaskScheduler.Default);
        var repeatedDrain = await repeatedInvocation.WaitAsync(timeout);
        Assert.Same(drain, repeatedDrain);
        lease.Dispose();
        lease.Dispose();
        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            lease.AcquireMutationPermit().ErrorCode);
    }

    [Fact]
    public async Task LegalTeacherAdmissionLease_IssuesAdmissionBundleAndRevokesBundleUse()
    {
        var timeout = TimeSpan.FromSeconds(5);
        var authority = CreateRevocableAuthority(static () => true);
        using var lease = AssertSuccessfulLease(authority);

        var bundleResult = lease.IssueAdmissionCapabilities();
        Assert.True(bundleResult.IsSuccess, bundleResult.ErrorMessage);
        var bundle = bundleResult.Value!;
        Assert.NotNull(bundle.DpoTrainerMutationAdmissionCapability);
        var permit = bundle.AcquireMutationPermit();
        Assert.True(permit.IsSuccess, permit.ErrorMessage);
        Assert.NotNull(permit.Value);
        permit.Value!.Dispose();

        lease.Dispose();
        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            lease.IssueAdmissionCapabilities().ErrorCode);
        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            bundle.AcquireMutationPermit().ErrorCode);

        var revokeAuthority = CreateRevocableAuthority(static () => true);
        using var revokedLease = AssertSuccessfulLease(revokeAuthority);
        var revokedBundle = revokedLease.IssueAdmissionCapabilities();
        Assert.True(revokedBundle.IsSuccess, revokedBundle.ErrorMessage);
        await revokeAuthority.BeginRevocation().WaitAsync(timeout);

        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            revokedBundle.Value!.AcquireMutationPermit().ErrorCode);
        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            revokedLease.IssueAdmissionCapabilities().ErrorCode);
    }

    [Fact]
    public async Task LegalTeacherAdmissionLease_IssueRaceWithRevocationFailsClosed()
    {
        var timeout = TimeSpan.FromSeconds(5);
        using var validatorEntered = new ManualResetEventSlim();
        using var releaseValidator = new ManualResetEventSlim();
        var authority = ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
            _ =>
            {
                validatorEntered.Set();
                if (!releaseValidator.Wait(timeout))
                    throw new TimeoutException("Validator release was not signaled.");
                return OperationResult<LegalTrainingTeacherAdmission>.Ok(
                    CreateLegalTeacherAdmission());
            },
            static () => true);

        var issueTask = Task.Run(() => authority.AcquireTrainingLease(
            [TrainingTeacherProvenance.Deterministic("issue-revoke-race")]));
        Task? drain = null;
        try
        {
            Assert.True(validatorEntered.Wait(timeout));
            var beginInvocation = Task.Factory.StartNew(
                () => authority.BeginRevocation(),
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                TaskScheduler.Default);
            drain = await beginInvocation.WaitAsync(timeout);
        }
        finally
        {
            releaseValidator.Set();
        }

        var result = await issueTask.WaitAsync(timeout);
        Assert.NotNull(drain);
        await drain.WaitAsync(timeout);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public async Task LegalTeacherAdmissionLease_DisposeLinearizesBeforeFinalPermitGrant()
    {
        var timeout = TimeSpan.FromSeconds(5);
        var nonCompletionWindow = TimeSpan.FromMilliseconds(150);
        using var observer = new LeaseConcurrencyObserverForTesting(timeout);
        var authority = ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
            _ => OperationResult<LegalTrainingTeacherAdmission>.Ok(
                CreateLegalTeacherAdmission()),
            static () => true,
            observer);
        using var lease = AssertSuccessfulLease(authority);
        var permitTask = Task.Factory.StartNew(
            lease.AcquireMutationPermit,
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        Task? disposeTask = null;

        try
        {
            Assert.True(observer.GrantPaused.Wait(timeout));
            disposeTask = Task.Factory.StartNew(
                lease.Dispose,
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            Assert.True(observer.DisposeAttempted.Wait(timeout));
            var firstCompletion = await Task.WhenAny(
                disposeTask,
                Task.Delay(nonCompletionWindow));
            Assert.NotSame(disposeTask, firstCompletion);
        }
        finally
        {
            observer.ReleaseGrant.Set();
        }

        var permit = await permitTask.WaitAsync(timeout);
        Assert.NotNull(disposeTask);
        await disposeTask.WaitAsync(timeout);
        Assert.True(permit.IsSuccess, permit.ErrorMessage);
        Assert.NotNull(permit.Value);
        permit.Value.Dispose();
        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            lease.AcquireMutationPermit().ErrorCode);

        var beginInvocation = Task.Factory.StartNew(
            () => authority.BeginRevocation(),
            CancellationToken.None,
            TaskCreationOptions.DenyChildAttach,
            TaskScheduler.Default);
        var drain = await beginInvocation.WaitAsync(timeout);
        await drain.WaitAsync(timeout);
    }

    [Fact]
    public void LegalTeacherAdmissionLease_SanitizesNonFatalAndPropagatesFatalFailures()
    {
        const string privateMarker = "C:/private/revocable-admission";
        var liveFailure = ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
            _ => OperationResult<LegalTrainingTeacherAdmission>.Ok(
                CreateLegalTeacherAdmission()),
            () => throw new IOException(privateMarker));
        var validatorFailure = ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
            _ => throw new IOException(privateMarker),
            static () => true);
        var fatalValidator = ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
            _ => throw new OutOfMemoryException(privateMarker),
            static () => true);
        var live = true;
        var permitAuthority = CreateRevocableAuthority(() => live
            ? true
            : throw new IOException(privateMarker));
        using var lease = AssertSuccessfulLease(permitAuthority);
        live = false;
        var fatalPermitIsInitialCheck = true;
        var fatalPermitAuthority = CreateRevocableAuthority(() =>
        {
            if (fatalPermitIsInitialCheck)
            {
                fatalPermitIsInitialCheck = false;
                return true;
            }

            throw new OutOfMemoryException(privateMarker);
        });
        using var fatalPermitLease = AssertSuccessfulLease(fatalPermitAuthority);

        var liveResult = liveFailure.AcquireTrainingLease([]);
        var validatorResult = validatorFailure.AcquireTrainingLease([]);
        var permitResult = lease.AcquireMutationPermit();

        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            liveResult.ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            validatorResult.ErrorCode);
        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            permitResult.ErrorCode);
        Assert.DoesNotContain(privateMarker, liveResult.ErrorMessage);
        Assert.DoesNotContain(privateMarker, validatorResult.ErrorMessage);
        Assert.DoesNotContain(privateMarker, permitResult.ErrorMessage);
        Assert.Throws<OutOfMemoryException>(
            () => fatalValidator.AcquireTrainingLease([]));
        Assert.Throws<OutOfMemoryException>(
            () => fatalPermitLease.AcquireMutationPermit());
    }

    [Fact]
    public void LegalTeacherAdmissionLease_IsOpaqueAndHasNoPublicFactoryOrEpochSurface()
    {
        var leaseType = typeof(ServerOwnedLegalTrainingTeacherAdmissionLease);

        Assert.Empty(leaseType.GetConstructors(
            BindingFlags.Public | BindingFlags.Instance));
        Assert.Equal(
            [nameof(ServerOwnedLegalTrainingTeacherAdmissionLease.Admission)],
            leaseType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(static property => property.Name)
                .ToArray());
        Assert.DoesNotContain(
            leaseType.GetMembers(BindingFlags.Public | BindingFlags.Instance),
            member => member switch
            {
                FieldInfo field => field.FieldType == typeof(long)
                                         || field.FieldType == typeof(
                                             ServerOwnedLegalTrainingTeacherAdmissionAuthority),
                PropertyInfo property => property.PropertyType == typeof(long)
                                               || property.PropertyType == typeof(
                                                   ServerOwnedLegalTrainingTeacherAdmissionAuthority),
                _ => false,
            });
    }

    [Fact]
    public void MaterializedComposition_RejectsMissingCapabilityBeforeTouchingArtifacts()
    {
        ServerOwnedLegalTrainingTeacherAdmissionAuthority?[] capabilities =
        [
            null,
            ServerOwnedLegalTrainingTeacherAdmissionAuthority.Unavailable(),
        ];

        foreach (var capability in capabilities)
        {
            var result = ContinuousLearningMaterializedTp1Composition.Create(
                capability!,
                fileAccess: null!,
                modelConfig: null!,
                trainingConfig: null!,
                teacherConfiguration: null!,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
                result.ErrorCode);
        }
    }

    [Fact]
    public async Task DirectArtifactDbPromotion_FailsBeforeCreatingStorageWithoutCapability()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"legal-capability-direct-db-{Guid.NewGuid():N}");
        var promotionRoot = Path.Combine(root, "promotion");
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);

        var result = await artifactDb.RunTrainingIntakeAndPromoteLocalAsync(
            "not-read-without-capability",
            promotionRoot);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            result.ErrorCode);
        Assert.Equal(0, provider.InvocationCount);
        Assert.Null(provider.LastModelSet);
        Assert.False(Directory.Exists(root));
    }

    [Fact]
    public void ArtifactDb_RetainsExactStartupCapabilityInstance()
    {
        using var fixture = new ModelServerCompositionFixture();
        var startup = CreateStartup(fixture.ValidEnvironment());
        var capability = startup.LegalTeacherAdmissionAuthority;
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            Path.Combine(fixture.Root, "capability-db"),
            capability);

        var retained = typeof(JsonlDesignConversionLocalArtifactDb)
            .GetField(
                "_legalTeacherAdmissionAuthority",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(artifactDb);

        Assert.Same(capability, retained);
    }

    [Fact]
    public void ProgramProviderSelectionPropagatesExactServerPairAndRejectsCustomProvider()
    {
        using var fixture = new ModelServerCompositionFixture();
        var startup = CreateStartup(fixture.ValidEnvironment());
        var capability = startup.LegalTeacherAdmissionAuthority;
        var serverProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var customProvider = new DesignConversionDpoPolicyBaseTestProvider();

        Assert.Same(
            capability,
            ModelServer.Program.ResolveServerOwnedLegalTeacherAdmissionAuthority(
                null,
                serverProvider,
                capability));
        Assert.Null(
            ModelServer.Program.ResolveServerOwnedLegalTeacherAdmissionAuthority(
                customProvider,
                serverProvider,
                capability));
        Assert.Null(
            ModelServer.Program.ResolveServerOwnedLegalTeacherAdmissionAuthority(
                null,
                null,
                capability));

        var pairedDb = ModelServer.Program.CreateLocalLoopbackArtifactDb(
            Path.Combine(fixture.Root, "program-paired-ready.json"),
            serverProvider,
            capability);
        var customDb = ModelServer.Program.CreateLocalLoopbackArtifactDb(
            Path.Combine(fixture.Root, "program-custom-ready.json"),
            customProvider);
        var providerField = typeof(JsonlDesignConversionLocalArtifactDb)
            .GetField(
                "_dpoPolicyBaseProvider",
                BindingFlags.Instance | BindingFlags.NonPublic)!;
        var authorityField = typeof(JsonlDesignConversionLocalArtifactDb)
            .GetField(
                "_legalTeacherAdmissionAuthority",
                BindingFlags.Instance | BindingFlags.NonPublic)!;

        Assert.Same(serverProvider, providerField.GetValue(pairedDb));
        Assert.Same(capability, authorityField.GetValue(pairedDb));
        Assert.Same(customProvider, providerField.GetValue(customDb));
        Assert.Null(authorityField.GetValue(customDb));
    }

    [Fact]
    public void LegalTeacherAssociationLeaseIsReferenceExactExclusiveAndIdempotentlyOwned()
    {
        using var firstFixture = new ModelServerCompositionFixture();
        using var secondFixture = new ModelServerCompositionFixture();
        var firstAuthority = CreateStartup(firstFixture.ValidEnvironment())
            .LegalTeacherAdmissionAuthority;
        var secondAuthority = CreateStartup(secondFixture.ValidEnvironment())
            .LegalTeacherAdmissionAuthority;
        var firstProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var secondProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var firstAssociation = DesignConversionRetainedDpoPolicyBaseActivation
            .AssociateLegalTeacherAdmissionAuthority(
                firstProvider,
                firstAuthority);
        var secondAssociation = DesignConversionRetainedDpoPolicyBaseActivation
            .AssociateLegalTeacherAdmissionAuthority(
                secondProvider,
                secondAuthority);

        try
        {
            Assert.NotSame(firstAuthority, secondAuthority);
            Assert.Same(
                firstAuthority,
                DesignConversionRetainedDpoPolicyBaseActivation
                    .ResolveLegalTeacherAdmissionAuthority(firstProvider));
            Assert.Same(
                secondAuthority,
                DesignConversionRetainedDpoPolicyBaseActivation
                    .ResolveLegalTeacherAdmissionAuthority(secondProvider));
            Assert.Throws<ArgumentException>(() =>
                DesignConversionRetainedDpoPolicyBaseActivation
                    .AssociateLegalTeacherAdmissionAuthority(
                        firstProvider,
                        secondAuthority));

            firstAssociation.Dispose();
            firstAssociation.Dispose();
            Assert.Null(
                DesignConversionRetainedDpoPolicyBaseActivation
                    .ResolveLegalTeacherAdmissionAuthority(firstProvider));
            Assert.Same(
                secondAuthority,
                DesignConversionRetainedDpoPolicyBaseActivation
                    .ResolveLegalTeacherAdmissionAuthority(secondProvider));

            using var replacement =
                DesignConversionRetainedDpoPolicyBaseActivation
                    .AssociateLegalTeacherAdmissionAuthority(
                        firstProvider,
                        secondAuthority);
            Assert.Same(
                secondAuthority,
                DesignConversionRetainedDpoPolicyBaseActivation
                    .ResolveLegalTeacherAdmissionAuthority(firstProvider));
        }
        finally
        {
            firstAssociation.Dispose();
            secondAssociation.Dispose();
        }

        Assert.Null(
            DesignConversionRetainedDpoPolicyBaseActivation
                .ResolveLegalTeacherAdmissionAuthority(firstProvider));
        Assert.Null(
            DesignConversionRetainedDpoPolicyBaseActivation
                .ResolveLegalTeacherAdmissionAuthority(secondProvider));
    }

    [Fact]
    public void DisabledConfiguration_HasNoPlanAndDoesNotTouchConfiguredPaths()
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        environment[ContinuousLearningModelServerConfigurationLoader.EnabledEnvironmentName]
            = bool.FalseString;
        environment[ContinuousLearningModelServerConfigurationLoader.BaseCheckpointEnvironmentName]
            = "not-an-absolute-path";
        var before = fixture.Snapshot();

        var startup = CreateStartup(environment);

        Assert.False(startup.HasDeferredCompositionPlan);
        Assert.False(startup.Readiness.ConfigurationAdmitted);
        Assert.Equal("disabled", startup.Readiness.ConfigurationStatus);
        AssertClosedAndUnmaterialized(startup.Readiness);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoOutputDirectories();
    }

    [Fact]
    public void DisabledConfiguration_DoesNotReadLegalEvidenceVariables()
    {
        var reads = new List<string>();

        var startup = ContinuousLearningModelServerStartup.Create(name =>
        {
            reads.Add(name);
            if (name == ContinuousLearningModelServerConfigurationLoader.EnabledEnvironmentName)
                return bool.FalseString;
            if (name == ContinuousLearningModelServerConfigurationLoader
                    .TrainingActivationEnabledEnvironmentName)
            {
                return null;
            }

            if (name == ContinuousLearningModelServerConfigurationLoader
                    .LocalIndependentPreferenceEvaluatorEnabledEnvironmentName)
            {
                return null;
            }
            throw new InvalidOperationException(
                "Disabled composition read beyond its control switches.");
        });

        Assert.Equal(
            [
                ContinuousLearningModelServerConfigurationLoader.EnabledEnvironmentName,
                ContinuousLearningModelServerConfigurationLoader
                    .TrainingActivationEnabledEnvironmentName,
                ContinuousLearningModelServerConfigurationLoader
                    .LocalIndependentPreferenceEvaluatorEnabledEnvironmentName
            ],
            reads);
        Assert.Equal("disabled", startup.Readiness.ConfigurationStatus);
        Assert.False(startup.Readiness.LegalEvidenceConfigured);
        Assert.Null(startup.Readiness.LocalRetentionPolicyRef);
        Assert.Null(startup.Readiness.OutputLicenseRef);
    }

    [Theory]
    [InlineData("")]
    [InlineData("pending")]
    [InlineData("mock")]
    [InlineData("C:/legal/retention.txt")]
    [InlineData("https://example.invalid/legal")]
    [InlineData("sha256:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("sha256:gggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggg")]
    public void InvalidLegalEvidenceRef_FailsBeforeAnyPathConfigurationIsRead(
        string invalidRef)
    {
        using var fixture = new ModelServerCompositionFixture();
        var before = fixture.Snapshot();
        foreach (var environmentName in new[]
        {
            ContinuousLearningModelServerConfigurationLoader.LocalRetentionPolicyRefEnvironmentName,
            ContinuousLearningModelServerConfigurationLoader.OutputLicenseRefEnvironmentName,
        })
        {
            var reads = new List<string>();
            var environment = fixture.ValidEnvironment();
            environment[environmentName] = invalidRef;

            var startup = ContinuousLearningModelServerStartup.Create(name =>
            {
                reads.Add(name);
                return environment.TryGetValue(name, out var value) ? value : null;
            });

            Assert.Equal("invalid", startup.Readiness.ConfigurationStatus);
            Assert.Equal(
                "CONTINUOUS_LEARNING_CONFIGURATION_INVALID",
                startup.Readiness.ConfigurationErrorCode);
            Assert.DoesNotContain(
                reads,
                name => name == ContinuousLearningModelServerConfigurationLoader.BaseCheckpointEnvironmentName
                        || name == ContinuousLearningModelServerConfigurationLoader.TokenizerEnvironmentName
                        || name == ContinuousLearningModelServerConfigurationLoader.AdapterRootEnvironmentName
                        || name == ContinuousLearningModelServerConfigurationLoader.CacheRootEnvironmentName
                        || name == ContinuousLearningModelServerConfigurationLoader.RegistryRootEnvironmentName
                        || name == ContinuousLearningModelServerConfigurationLoader.DpoPairRootEnvironmentName);
            AssertClosedAndUnmaterialized(startup.Readiness);
            Assert.Equal(before, fixture.Snapshot());
            fixture.AssertNoOutputDirectories();
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MissingLegalEvidenceRef_FailsClosedBeforePathInspection(bool removeRetention)
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        environment.Remove(removeRetention
            ? ContinuousLearningModelServerConfigurationLoader.LocalRetentionPolicyRefEnvironmentName
            : ContinuousLearningModelServerConfigurationLoader.OutputLicenseRefEnvironmentName);
        var reads = new List<string>();
        var before = fixture.Snapshot();

        var startup = ContinuousLearningModelServerStartup.Create(name =>
        {
            reads.Add(name);
            return environment.TryGetValue(name, out var value) ? value : null;
        });

        Assert.Equal("invalid", startup.Readiness.ConfigurationStatus);
        Assert.Equal(
            "CONTINUOUS_LEARNING_CONFIGURATION_INVALID",
            startup.Readiness.ConfigurationErrorCode);
        Assert.DoesNotContain(
            reads,
            name => name == ContinuousLearningModelServerConfigurationLoader.BaseCheckpointEnvironmentName
                    || name == ContinuousLearningModelServerConfigurationLoader.TokenizerEnvironmentName);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoOutputDirectories();
    }

    [Fact]
    public void EnabledIncompleteConfiguration_FailsClosedWithoutFilesystemMutation()
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        environment.Remove(
            ContinuousLearningModelServerConfigurationLoader.AuditSaltEnvironmentName);
        var before = fixture.Snapshot();

        var startup = CreateStartup(environment);

        Assert.False(startup.HasDeferredCompositionPlan);
        Assert.Equal("invalid", startup.Readiness.ConfigurationStatus);
        Assert.Equal(
            "CONTINUOUS_LEARNING_CONFIGURATION_INVALID",
            startup.Readiness.ConfigurationErrorCode);
        AssertClosedAndUnmaterialized(startup.Readiness);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoOutputDirectories();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData(" aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public void MissingOrMalformedOllamaManifestDigest_FailsClosedWithoutLegacyFallback(
        string? digest)
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        if (digest is null)
        {
            environment.Remove(
                ContinuousLearningModelServerConfigurationLoader
                    .OllamaManifestDigestEnvironmentName);
        }
        else
        {
            environment[
                ContinuousLearningModelServerConfigurationLoader
                    .OllamaManifestDigestEnvironmentName] = digest;
        }
        environment["XIIGEN_CONTINUOUS_LEARNING_TP1_MODEL_VERSION"] =
            new string('a', 64);
        var before = fixture.Snapshot();

        var startup = CreateStartup(environment);

        Assert.False(startup.HasDeferredCompositionPlan);
        Assert.False(startup.Readiness.ConfigurationAdmitted);
        Assert.Equal("invalid", startup.Readiness.ConfigurationStatus);
        Assert.Equal(
            "CONTINUOUS_LEARNING_CONFIGURATION_INVALID",
            startup.Readiness.ConfigurationErrorCode);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoOutputDirectories();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public void MissingOrMalformedOllamaModelLayerDigest_FailsClosed(
        string? digest)
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        if (digest is null)
        {
            environment.Remove(
                ContinuousLearningModelServerConfigurationLoader
                    .OllamaModelLayerDigestEnvironmentName);
        }
        else
        {
            environment[
                ContinuousLearningModelServerConfigurationLoader
                    .OllamaModelLayerDigestEnvironmentName] = digest;
        }
        var before = fixture.Snapshot();

        var startup = CreateStartup(environment);

        Assert.False(startup.HasDeferredCompositionPlan);
        Assert.False(startup.Readiness.ConfigurationAdmitted);
        Assert.Equal(
            "CONTINUOUS_LEARNING_CONFIGURATION_INVALID",
            startup.Readiness.ConfigurationErrorCode);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoOutputDirectories();
    }

    [Theory]
    [InlineData(ContinuousLearningModelServerConfigurationLoader.BaseModelIdEnvironmentName)]
    [InlineData(ContinuousLearningModelServerConfigurationLoader.BaseModelRevisionEnvironmentName)]
    public void MissingBaseCheckpointIdentity_FailsConfigurationAdmission(
        string missingName)
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        environment.Remove(missingName);

        var startup = CreateStartup(environment);

        Assert.False(startup.HasDeferredCompositionPlan);
        Assert.Equal(
            "CONTINUOUS_LEARNING_CONFIGURATION_INVALID",
            startup.Readiness.ConfigurationErrorCode);
        AssertClosedAndUnmaterialized(startup.Readiness);
    }

    [Theory]
    [InlineData(
        ContinuousLearningModelServerConfigurationLoader.BaseModelIdEnvironmentName,
        "../private-model")]
    [InlineData(
        ContinuousLearningModelServerConfigurationLoader.BaseModelRevisionEnvironmentName,
        "owner/revision")]
    public void PathLikeBaseCheckpointIdentity_FailsConfigurationAdmission(
        string name,
        string value)
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        environment[name] = value;

        var startup = CreateStartup(environment);

        Assert.False(startup.HasDeferredCompositionPlan);
        Assert.Equal(
            "CONTINUOUS_LEARNING_CONFIGURATION_INVALID",
            startup.Readiness.ConfigurationErrorCode);
        AssertClosedAndUnmaterialized(startup.Readiness);
    }

    [Theory]
    [InlineData(31)]
    [InlineData(33)]
    public void AuditSalt_MustDecodeFromCanonicalBase64ToExactly32Bytes(
        int byteCount)
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        environment[ContinuousLearningModelServerConfigurationLoader.AuditSaltEnvironmentName]
            = Convert.ToBase64String(Enumerable.Range(0, byteCount)
                .Select(static value => checked((byte)value))
                .ToArray());
        var before = fixture.Snapshot();

        var startup = CreateStartup(environment);

        Assert.False(startup.HasDeferredCompositionPlan);
        AssertClosedAndUnmaterialized(startup.Readiness);
        Assert.Equal(before, fixture.Snapshot());
    }

    [Theory]
    [InlineData("10001")]
    [InlineData("-1")]
    [InlineData("1.0")]
    public void TeacherCallBudget_IsServerParsedAndHardCapped(string value)
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        environment[ContinuousLearningModelServerConfigurationLoader.MaxTeacherCallsEnvironmentName]
            = value;

        var startup = CreateStartup(environment);

        Assert.False(startup.HasDeferredCompositionPlan);
        AssertClosedAndUnmaterialized(startup.Readiness);
        fixture.AssertNoOutputDirectories();
    }

    [Fact]
    public void NonQwenTeacherIdentity_CannotAdmitTp1Plan()
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        environment[ContinuousLearningModelServerConfigurationLoader.ModelIdEnvironmentName]
            = "mistral:7b";

        var startup = CreateStartup(environment);

        Assert.False(startup.HasDeferredCompositionPlan);
        AssertClosedAndUnmaterialized(startup.Readiness);
        fixture.AssertNoOutputDirectories();
    }

    [Fact]
    public void NonCanonicalAliasAndLexicalCollision_FailWithoutPhysicalIdentityClaim()
    {
        using var fixture = new ModelServerCompositionFixture();
        var aliasEnvironment = fixture.ValidEnvironment();
        aliasEnvironment[
            ContinuousLearningModelServerConfigurationLoader.AdapterRootEnvironmentName]
            = Path.Combine(fixture.Root, "alias", "..", "adapter-root");
        var before = fixture.Snapshot();

        var alias = CreateStartup(aliasEnvironment);

        Assert.False(alias.HasDeferredCompositionPlan);
        Assert.False(alias.Readiness.PathIdentityVerified);
        Assert.Equal(before, fixture.Snapshot());

        var collisionEnvironment = fixture.ValidEnvironment();
        collisionEnvironment[
            ContinuousLearningModelServerConfigurationLoader.RegistryRootEnvironmentName]
            = fixture.CacheRoot;

        var collision = CreateStartup(collisionEnvironment);

        Assert.False(collision.HasDeferredCompositionPlan);
        Assert.Equal(
            "CONTINUOUS_LEARNING_LEXICAL_PATH_COLLISION",
            collision.Readiness.ConfigurationErrorCode);
        Assert.False(collision.Readiness.PathIdentityVerified);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoOutputDirectories();
    }

    [Fact]
    public void ExistingReparseRoot_FailsClosedWhenPlatformAllowsCreatingIt()
    {
        using var fixture = new ModelServerCompositionFixture();
        var target = Path.Combine(fixture.Root, "reparse-target");
        var link = Path.Combine(fixture.Root, "reparse-adapter-root");
        Directory.CreateDirectory(target);
        try
        {
            Directory.CreateSymbolicLink(link, target);
        }
        catch (Exception exception)
            when (exception is UnauthorizedAccessException
                  or IOException
                  or PlatformNotSupportedException
                  or NotSupportedException)
        {
            return;
        }

        Assert.True(
            (File.GetAttributes(link) & FileAttributes.ReparsePoint) != 0);
        var environment = fixture.ValidEnvironment();
        environment[
            ContinuousLearningModelServerConfigurationLoader.AdapterRootEnvironmentName]
            = link;
        var before = fixture.Snapshot();

        var startup = CreateStartup(environment);

        Assert.False(startup.HasDeferredCompositionPlan);
        AssertClosedAndUnmaterialized(startup.Readiness);
        Assert.Equal(before, fixture.Snapshot());
    }

    [Fact]
    public void ValidConfiguration_AdmitsDeferredPlanAndCreatesNoFilesystemEntry()
    {
        using var fixture = new ModelServerCompositionFixture();
        var before = fixture.Snapshot();

        var startup = CreateStartup(fixture.ValidEnvironment());

        Assert.True(startup.HasDeferredCompositionPlan);
        Assert.True(startup.Readiness.ConfigurationAdmitted);
        Assert.Equal(
            "configured_unverified_inactive",
            startup.Readiness.ConfigurationStatus);
        Assert.True(startup.Readiness.LegalEvidenceConfigured);
        Assert.False(startup.Readiness.LegalEvidenceVerified);
        Assert.Equal(
            fixture.ValidEnvironment()[ContinuousLearningModelServerConfigurationLoader.LocalRetentionPolicyRefEnvironmentName],
            startup.Readiness.LocalRetentionPolicyRef);
        Assert.Equal(
            fixture.ValidEnvironment()[ContinuousLearningModelServerConfigurationLoader.OutputLicenseRefEnvironmentName],
            startup.Readiness.OutputLicenseRef);
        AssertClosedAndUnmaterialized(startup.Readiness);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoOutputDirectories();
    }

    [Fact]
    public void ValidConfiguration_CapturesRepositoryOwnedResumeStartWithoutFilesystemWrites()
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        var before = fixture.Snapshot();

        var startup = CreateStartup(environment);
        var plan = GetDeferredCompositionPlan(startup);
        var resolved = plan.ResolveRepositoryStartRequest("html-css");
        var wrongDomain = plan.ResolveRepositoryStartRequest("translation");

        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        var request = Assert.IsType<
            RepositoryOwnedIncrementalTrainingSessionStartRequest>(
            resolved.Value);
        Assert.Equal(Path.GetFullPath(fixture.RepositoryRoot), request.RepositoryRoot);
        Assert.Equal("html-css", request.DomainSlug);
        Assert.Equal("learning-unit", request.DesignPatternSlug);
        Assert.Equal(
            environment[
                ContinuousLearningModelServerConfigurationLoader
                    .RepositoryModelIdEnvironmentName],
            request.ModelIdSlug);
        Assert.Equal(
            environment[
                ContinuousLearningModelServerConfigurationLoader
                    .RepositoryModelVersionEnvironmentName],
            request.ModelVersion);
        Assert.Equal(
            environment[
                ContinuousLearningModelServerConfigurationLoader
                    .RuntimeConfigSha256EnvironmentName],
            request.RuntimeCompatibility.ConfigSha256);
        Assert.Equal(
            environment[
                ContinuousLearningModelServerConfigurationLoader
                    .RuntimeTopologySha256EnvironmentName],
            request.RuntimeCompatibility.TopologySha256);
        Assert.Equal(
            environment[
                ContinuousLearningModelServerConfigurationLoader
                    .RuntimeTokenizerSha256EnvironmentName],
            request.RuntimeCompatibility.TokenizerSha256);
        Assert.Equal(
            environment[
                ContinuousLearningModelServerConfigurationLoader
                    .RuntimeTrainerSha256EnvironmentName],
            request.RuntimeCompatibility.TrainerSha256);
        Assert.Equal(
            environment[
                ContinuousLearningModelServerConfigurationLoader
                    .RuntimeSchemaSha256EnvironmentName],
            request.RuntimeCompatibility.SchemaSha256);
        Assert.Equal(
            environment[
                ContinuousLearningModelServerConfigurationLoader
                    .RuntimeCompatibilityKeyEnvironmentName],
            request.RuntimeCompatibility.CompatibilityKey);
        Assert.False(request.ColdStartPolicy.IsAuthorized);
        Assert.Null(request.ColdStartPolicy.HumanReason);
        var checkpointAddress =
            RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                request.RepositoryRoot,
                request.DomainSlug,
                request.DesignPatternSlug,
                request.ModelIdSlug,
                request.ModelVersion,
                step: 1);
        Assert.True(checkpointAddress.IsSuccess, checkpointAddress.ErrorMessage);
        Assert.Equal(
            "training-artifacts/models/html-css/learning-unit/" +
            "qwen3-5-9b-dpo/v0001/checkpoints/000001/weights.bin",
            checkpointAddress.Value!.WeightsRelativePath);
        Assert.False(wrongDomain.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointLoader.InvalidRequestErrorCode,
            wrongDomain.ErrorCode);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoOutputDirectories();
    }

    [Fact]
    public void RepositoryOwnedResumeStartConfiguration_RequiredAndMalformedFieldsAreFailClosed()
    {
        using var fixture = new ModelServerCompositionFixture();
        var cases = new (string Name, string? Value)[]
        {
            (ContinuousLearningModelServerConfigurationLoader
                .RepositoryModelIdEnvironmentName, null),
            (ContinuousLearningModelServerConfigurationLoader
                .RepositoryModelIdEnvironmentName, "qwen3.5-9b-dpo"),
            (ContinuousLearningModelServerConfigurationLoader
                .RepositoryModelVersionEnvironmentName, null),
            (ContinuousLearningModelServerConfigurationLoader
                .RepositoryModelVersionEnvironmentName, "v.0001"),
            (ContinuousLearningModelServerConfigurationLoader
                .RuntimeConfigSha256EnvironmentName, null),
            (ContinuousLearningModelServerConfigurationLoader
                .RuntimeConfigSha256EnvironmentName, new string('A', 64)),
            (ContinuousLearningModelServerConfigurationLoader
                .RuntimeTopologySha256EnvironmentName, null),
            (ContinuousLearningModelServerConfigurationLoader
                .RuntimeTopologySha256EnvironmentName, new string('2', 63)),
            (ContinuousLearningModelServerConfigurationLoader
                .RuntimeTokenizerSha256EnvironmentName, null),
            (ContinuousLearningModelServerConfigurationLoader
                .RuntimeTokenizerSha256EnvironmentName, new string('6', 63)),
            (ContinuousLearningModelServerConfigurationLoader
                .RuntimeTrainerSha256EnvironmentName, null),
            (ContinuousLearningModelServerConfigurationLoader
                .RuntimeTrainerSha256EnvironmentName, new string('g', 64)),
            (ContinuousLearningModelServerConfigurationLoader
                .RuntimeSchemaSha256EnvironmentName, null),
            (ContinuousLearningModelServerConfigurationLoader
                .RuntimeSchemaSha256EnvironmentName, ""),
            (ContinuousLearningModelServerConfigurationLoader
                .RuntimeCompatibilityKeyEnvironmentName, null),
            (ContinuousLearningModelServerConfigurationLoader
                .RuntimeCompatibilityKeyEnvironmentName, "human readable key"),
        };

        foreach (var (name, value) in cases)
        {
            var environment = fixture.ValidEnvironment();
            if (value is null)
                environment.Remove(name);
            else
                environment[name] = value;

            var startup = CreateStartup(environment);

            Assert.False(startup.HasDeferredCompositionPlan);
            Assert.False(startup.Readiness.ConfigurationAdmitted);
            Assert.Equal(
                "CONTINUOUS_LEARNING_CONFIGURATION_INVALID",
                startup.Readiness.ConfigurationErrorCode);
            AssertClosedAndUnmaterialized(startup.Readiness);
        }
    }

    [Fact]
    public void ValidConfiguration_AdmitsInstalledMixedCaseOllamaTagFromEnvironment()
    {
        const string installedModelId = "qwen3.5:9b-q4_K_M";
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        environment[ContinuousLearningModelServerConfigurationLoader.ModelIdEnvironmentName]
            = installedModelId;
        var before = fixture.Snapshot();

        var startup = CreateStartup(environment);
        var admission = startup.LegalTeacherAdmissionAuthority.ValidateBatch(
        [
            new TrainingTeacherProvenance(
                TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                installedModelId,
                environment[
                    ContinuousLearningModelServerConfigurationLoader
                        .OllamaManifestDigestEnvironmentName],
                TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false,
                ProviderTermsAllowTraining: false,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: true,
                RequestId: "installed-mixed-case-startup",
                InvocationCount: 1)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = environment[
                    ContinuousLearningModelServerConfigurationLoader
                        .OllamaModelLayerDigestEnvironmentName]
            }
        ]);

        Assert.True(startup.HasDeferredCompositionPlan);
        Assert.True(startup.Readiness.ConfigurationAdmitted);
        Assert.Equal(
            "configured_unverified_inactive",
            startup.Readiness.ConfigurationStatus);
        Assert.False(admission.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            admission.ErrorCode);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoOutputDirectories();
    }

    [Fact]
    public void OptionalDpoMaxStepCountEnvironment_AdmitsIntoDeferredPlanWithoutFilesystemEntry()
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        environment[ContinuousLearningModelServerConfigurationLoader.DpoMaxStepCountEnvironmentName]
            = "2";
        var before = fixture.Snapshot();

        var resolution = ContinuousLearningModelServerConfigurationLoader.Resolve(
            name => environment.GetValueOrDefault(name));

        Assert.Equal(ContinuousLearningConfigurationState.Enabled, resolution.State);
        Assert.NotNull(resolution.DeferredPlan);
        Assert.Equal(2, resolution.DeferredPlan!.TeacherConfiguration.DpoMaxStepCount);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoOutputDirectories();
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("not-a-number")]
    [InlineData(" 2")]
    public void OptionalDpoMaxStepCountEnvironment_RejectsInvalidBeforePlan(
        string invalidStepCount)
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        environment[ContinuousLearningModelServerConfigurationLoader.DpoMaxStepCountEnvironmentName]
            = invalidStepCount;
        var before = fixture.Snapshot();

        var resolution = ContinuousLearningModelServerConfigurationLoader.Resolve(
            name => environment.GetValueOrDefault(name));

        Assert.Equal(ContinuousLearningConfigurationState.Invalid, resolution.State);
        Assert.Null(resolution.DeferredPlan);
        Assert.Equal("CONTINUOUS_LEARNING_CONFIGURATION_INVALID", resolution.ErrorCode);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoOutputDirectories();
    }

    [Fact]
    public void OptionalDpoMaxStepCountEnvironment_RejectsAboveSupportedMaximumBeforePlan()
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        environment[ContinuousLearningModelServerConfigurationLoader.DpoMaxStepCountEnvironmentName]
            = (AgentInfrastructure.Logical.LogicalDpoTrainingUnit.MaximumSupportedMaxStepCount + 1)
                .ToString(System.Globalization.CultureInfo.InvariantCulture);
        var before = fixture.Snapshot();

        var resolution = ContinuousLearningModelServerConfigurationLoader.Resolve(
            name => environment.GetValueOrDefault(name));

        Assert.Equal(ContinuousLearningConfigurationState.Invalid, resolution.State);
        Assert.Null(resolution.DeferredPlan);
        Assert.Equal("CONTINUOUS_LEARNING_CONFIGURATION_INVALID", resolution.ErrorCode);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoOutputDirectories();
    }

    [Fact]
    public void CorruptRegistryAndExclusivelyLockedAdapter_AreNotInspectedAtInactiveStartup()
    {
        using var fixture = new ModelServerCompositionFixture();
        Directory.CreateDirectory(fixture.RegistryRoot);
        Directory.CreateDirectory(fixture.AdapterRoot);
        var registry = Path.Combine(fixture.RegistryRoot, "registry.json");
        var adapter = Path.Combine(fixture.AdapterRoot, "BUG.lora");
        File.WriteAllText(registry, "{ definitely-not-a-registry }");
        File.WriteAllBytes(adapter, [0xDE, 0xAD, 0xBE, 0xEF]);
        using var lockedAdapter = new FileStream(
            adapter,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.None);
        var before = fixture.Snapshot();

        var startup = CreateStartup(fixture.ValidEnvironment());

        Assert.True(startup.HasDeferredCompositionPlan);
        Assert.True(startup.Readiness.ConfigurationAdmitted);
        AssertClosedAndUnmaterialized(startup.Readiness);
        Assert.Equal(before, fixture.Snapshot());
        Assert.Equal("{ definitely-not-a-registry }", File.ReadAllText(registry));
    }

    [Fact]
    public void ExclusivelyLockedCheckpointAndTokenizer_ContentIsNotOpened()
    {
        using var fixture = new ModelServerCompositionFixture();
        using var lockedBase = new FileStream(
            fixture.BaseCheckpointPath,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.None);
        using var lockedTokenizer = new FileStream(
            fixture.TokenizerPath,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.None);
        var before = fixture.Snapshot();

        var startup = CreateStartup(fixture.ValidEnvironment());

        Assert.True(startup.HasDeferredCompositionPlan);
        Assert.True(startup.Readiness.ConfigurationAdmitted);
        AssertClosedAndUnmaterialized(startup.Readiness);
        Assert.Equal(before, fixture.Snapshot());
    }

    [Fact]
    public void LexicallyDistinctArtifactPaths_NeverClaimPhysicalIdentityVerification()
    {
        using var fixture = new ModelServerCompositionFixture();
        var startup = CreateStartup(fixture.ValidEnvironment());

        Assert.True(startup.HasDeferredCompositionPlan);
        Assert.False(startup.Readiness.PathIdentityVerified);
        Assert.False(startup.Readiness.CompositionMaterialized);
    }

    [Fact]
    public void BindRetainedFileAccess_RepeatIsIdempotentAndDifferentAccessFailsBeforeMaterialization()
    {
        using var fixture = new ModelServerCompositionFixture();
        using var foreignFixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        foreignFixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        var foreignStartup = CreateStartup(foreignFixture.ValidEnvironment());
        using var foreignAccess = foreignStartup.AcquireRetainedFileAccess();
        var configuredReadiness = startup.Readiness;

        Assert.Throws<ContinuousLearningPathIdentityException>(() =>
            startup.BindRetainedFileAccess(foreignAccess));
        Assert.Equal(configuredReadiness, startup.Readiness);

        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        var boundReadiness = startup.Readiness;
        startup.BindRetainedFileAccess(access);

        Assert.Equal(boundReadiness, startup.Readiness);
        Assert.Throws<ContinuousLearningPathIdentityException>(() =>
            startup.BindRetainedFileAccess(foreignAccess));
        Assert.Equal(boundReadiness, startup.Readiness);
        Assert.False(startup.Readiness.CompositionMaterialized);
        Assert.False(startup.Readiness.StableDpoExecutorConfigured);
    }

    [Fact]
    public void ValidExplicitConfiguration_MaterializesExactlyOnceWithOnlyOwnerLockLeaves()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        var before = fixture.Snapshot();
        using var access = startup.AcquireRetainedFileAccess();

        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var first = startup.Readiness;
        var firstComposition = GetMaterializedComposition(startup);
        var firstFactory = GetRetainedSessionFactory(firstComposition);
        startup.MaterializeInactiveTp1Composition(access);
        var second = startup.Readiness;
        var secondComposition = GetMaterializedComposition(startup);
        var secondFactory = GetRetainedSessionFactory(secondComposition);

        Assert.Equal(first, second);
        Assert.True(first.ConfigurationAdmitted);
        Assert.True(first.LegalEvidenceConfigured);
        Assert.True(first.LegalEvidenceVerified);
        Assert.True(first.CompositionMaterialized);
        Assert.True(first.PathIdentityVerified);
        Assert.True(first.RetainedObjectIdentityBound);
        Assert.False(first.StableDpoExecutorConfigured);
        Assert.Equal(
            "materialized_policy_base_binding_pending_inactive",
            first.ConfigurationStatus);
        Assert.False(first.TrainingEnabled);
        Assert.False(first.CycleExecutionAvailable);
        Assert.False(first.PerformsTraining);
        Assert.False(first.ProductReady);
        Assert.Same(firstComposition, secondComposition);
        Assert.Same(firstFactory, secondFactory);
        Assert.ThrowsAny<IOException>(() =>
            CreateStartup(fixture.ValidEnvironment())
                .AcquireRetainedFileAccess());
        AssertOnlyOwnerLockLeavesAdded(before, fixture.Snapshot());
    }

    [Theory]
    [InlineData("adapter")]
    [InlineData("cache")]
    [InlineData("registry")]
    [InlineData("dpo-pairs")]
    public void MutableCustodyRoot_ConcurrentOwnerFailsWithSanitizedStableCode(
        string sharedRole)
    {
        using var firstFixture = new ModelServerCompositionFixture();
        using var secondFixture = new ModelServerCompositionFixture();
        firstFixture.CreateOutputDirectories();
        secondFixture.CreateOutputDirectories();
        var firstEnvironment = firstFixture.ValidEnvironment();
        var secondEnvironment = secondFixture.ValidEnvironment();
        var environmentName = sharedRole switch
        {
            "adapter" => ContinuousLearningModelServerConfigurationLoader
                .AdapterRootEnvironmentName,
            "cache" => ContinuousLearningModelServerConfigurationLoader
                .CacheRootEnvironmentName,
            "registry" => ContinuousLearningModelServerConfigurationLoader
                .RegistryRootEnvironmentName,
            "dpo-pairs" => ContinuousLearningModelServerConfigurationLoader
                .DpoPairRootEnvironmentName,
            _ => throw new ArgumentOutOfRangeException(nameof(sharedRole)),
        };
        secondEnvironment[environmentName] = firstEnvironment[environmentName];
        if (sharedRole == "dpo-pairs")
        {
            secondEnvironment[
                ContinuousLearningModelServerConfigurationLoader
                    .RepositoryRootEnvironmentName] = firstEnvironment[
                        ContinuousLearningModelServerConfigurationLoader
                            .RepositoryRootEnvironmentName];
        }
        using var firstAccess = CreateStartup(firstEnvironment)
            .AcquireRetainedFileAccess();
        var secondStartup = CreateStartup(secondEnvironment);

        var collision = Assert.Throws<
            ContinuousLearningMutationAdmissionDomainAlreadyOwnedException>(
            () => secondStartup.AcquireRetainedFileAccess());
        secondStartup.MarkRetainedFileAccessAcquisitionFailed(collision);

        Assert.Equal(
            ContinuousLearningModelServerStartup
                .MutationAdmissionDomainAlreadyOwnedErrorCode,
            secondStartup.Readiness.ConfigurationErrorCode);
        Assert.DoesNotContain(firstFixture.Root, collision.Message);
        Assert.DoesNotContain(secondFixture.Root, collision.Message);
    }

    [Fact]
    public void ImmutableArtifacts_AreShareableAcrossDistinctMutableCustodyDomains()
    {
        using var firstFixture = new ModelServerCompositionFixture();
        using var secondFixture = new ModelServerCompositionFixture();
        firstFixture.CreateOutputDirectories();
        secondFixture.CreateOutputDirectories();
        var firstEnvironment = firstFixture.ValidEnvironment();
        var secondEnvironment = secondFixture.ValidEnvironment();
        secondEnvironment[ContinuousLearningModelServerConfigurationLoader
            .BaseCheckpointEnvironmentName] = firstEnvironment[
                ContinuousLearningModelServerConfigurationLoader
                    .BaseCheckpointEnvironmentName];
        secondEnvironment[ContinuousLearningModelServerConfigurationLoader
            .TokenizerEnvironmentName] = firstEnvironment[
                ContinuousLearningModelServerConfigurationLoader
                    .TokenizerEnvironmentName];

        using var firstAccess = CreateStartup(firstEnvironment)
            .AcquireRetainedFileAccess();
        using var secondAccess = CreateStartup(secondEnvironment)
            .AcquireRetainedFileAccess();

        firstAccess.EnsureRetained();
        secondAccess.EnsureRetained();
    }

    [Fact]
    public async Task RetainedDpoPolicyActivation_CollisionIsSanitizedAndReleasesAfterOwnerStops()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var environment = fixture.ValidEnvironment();
        var owner = await DesignConversionRetainedDpoPolicyBaseActivation
            .ActivateAsync(name => environment.TryGetValue(name, out var value)
                ? value
                : null);
        var ownerLease = Assert.IsType<
            DesignConversionRetainedDpoPolicyBaseActivationLease>(owner.Lease);

        var collision = await DesignConversionRetainedDpoPolicyBaseActivation
            .ActivateAsync(name => environment.TryGetValue(name, out var value)
                ? value
                : null);

        AssertSanitizedActivationFailure(
            collision,
            ContinuousLearningModelServerStartup
                .MutationAdmissionDomainAlreadyOwnedErrorCode);
        Assert.DoesNotContain(fixture.Root, collision.ErrorMessage);

        await ownerLease.DisposeAsync();
        var replacement = await DesignConversionRetainedDpoPolicyBaseActivation
            .ActivateAsync(name => environment.TryGetValue(name, out var value)
                ? value
                : null);
        var replacementLease = Assert.IsType<
            DesignConversionRetainedDpoPolicyBaseActivationLease>(replacement.Lease);
        await replacementLease.DisposeAsync();
    }

    [Fact]
    public void OddHeadDimension_ExplicitConfigurationAndMatchingArtifactMaterialize()
    {
        var oddHeadConfig = new ModelConfig
        {
            VocabSize = 256,
            ContextLen = 8,
            HiddenDim = 3,
            NumLayers = 1,
            NumHeads = 1,
            NumKvHeads = 1,
            FfnHiddenDim = 6,
            RopeTheta = 10000f,
            RmsNormEps = 1e-5f,
        };
        using var fixture = new ModelServerCompositionFixture(oddHeadConfig);
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        var before = fixture.Snapshot();
        using var access = startup.AcquireRetainedFileAccess();

        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);

        Assert.True(startup.Readiness.ConfigurationAdmitted);
        Assert.True(startup.Readiness.CompositionMaterialized);
        Assert.Equal(
            "materialized_policy_base_binding_pending_inactive",
            startup.Readiness.ConfigurationStatus);
        Assert.False(startup.Readiness.StableDpoExecutorConfigured);
        AssertOnlyOwnerLockLeavesAdded(before, fixture.Snapshot());
    }

    [Theory]
    [InlineData(ContinuousLearningModelServerConfigurationLoader.ModelVocabSizeEnvironmentName)]
    [InlineData(ContinuousLearningModelServerConfigurationLoader.TrainingMaxLrEnvironmentName)]
    public void MissingExplicitModelOrTrainingConfiguration_NeverAdmitsOrMaterializes(
        string missingName)
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        environment.Remove(missingName);
        var before = fixture.Snapshot();

        var startup = CreateStartup(environment);

        Assert.False(startup.HasDeferredCompositionPlan);
        Assert.Equal("invalid", startup.Readiness.ConfigurationStatus);
        Assert.Equal("CONTINUOUS_LEARNING_CONFIGURATION_INVALID", startup.Readiness.ConfigurationErrorCode);
        AssertClosedAndUnmaterialized(startup.Readiness);
        Assert.Equal(before, fixture.Snapshot());
    }

    [Fact]
    public void CheckpointModelConfigurationMismatch_FailsClosedWithoutMaterializing()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var environment = fixture.ValidEnvironment();
        environment[ContinuousLearningModelServerConfigurationLoader.ModelVocabSizeEnvironmentName] = "512";
        var startup = CreateStartup(environment);
        var before = fixture.Snapshot();
        using var access = startup.AcquireRetainedFileAccess();

        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);

        Assert.True(startup.Readiness.ConfigurationAdmitted);
        Assert.Equal("materialization_failed_inactive", startup.Readiness.ConfigurationStatus);
        Assert.Equal(
            ContinuousLearningModelServerStartup.MaterializationFailedErrorCode,
            startup.Readiness.ConfigurationErrorCode);
        AssertClosedAndUnmaterialized(startup.Readiness);
        Assert.Equal(before, fixture.Snapshot());
    }

    [Fact]
    public void DeserializableButInvalidTokenizer_FailsClosedWithoutMaterializing()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var original = File.ReadAllText(fixture.TokenizerPath);
        var malformed = original.Replace(
            "\"1\":\"AQ==\"",
            "\"1\":\"AA==\"",
            StringComparison.Ordinal);
        Assert.NotEqual(original, malformed);
        File.WriteAllText(fixture.TokenizerPath, malformed);
        Assert.Equal(256, BpeTokenizer.Load(fixture.TokenizerPath).VocabSize);

        var startup = CreateStartup(fixture.ValidEnvironment());
        var before = fixture.Snapshot();
        using var access = startup.AcquireRetainedFileAccess();

        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);

        Assert.Equal("materialization_failed_inactive", startup.Readiness.ConfigurationStatus);
        Assert.Equal(
            ContinuousLearningModelServerStartup.MaterializationFailedErrorCode,
            startup.Readiness.ConfigurationErrorCode);
        AssertClosedAndUnmaterialized(startup.Readiness);
        Assert.Equal(before, fixture.Snapshot());
    }

    [Fact]
    public void MissingRetainedAdapterArtifact_IsNotResolvedDuringMaterialization()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var missingArtifactPath = SeedMissingRetainedAdapterReference(
            fixture.ValidEnvironment(),
            fixture.ModelConfiguration,
            fixture.AdapterRoot,
            fixture.BaseCheckpointPath,
            fixture.TokenizerPath);
        Assert.False(File.Exists(missingArtifactPath));
        var before = fixture.Snapshot();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();

        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);

        Assert.True(startup.Readiness.CompositionMaterialized);
        Assert.True(startup.Readiness.PathIdentityVerified);
        Assert.True(startup.Readiness.RetainedObjectIdentityBound);
        Assert.False(startup.Readiness.TrainingEnabled);
        Assert.False(startup.Readiness.CycleExecutionAvailable);
        access.EnsureRetained();
        Assert.False(File.Exists(missingArtifactPath));
        Assert.Equal(before, fixture.Snapshot());
    }

    [Fact]
    public void OverBudgetTokenizer_FailsConfigurationAdmissionBeforeCompositionCanMaterialize()
    {
        using var fixture = new ModelServerCompositionFixture();
        using (var tokenizer = new FileStream(
                   fixture.TokenizerPath,
                   FileMode.Open,
                   FileAccess.Write,
                   FileShare.Read))
        {
            tokenizer.SetLength(BpeTokenizer.MaximumCanonicalArtifactBytes + 1);
        }
        var before = fixture.Snapshot();

        var startup = CreateStartup(fixture.ValidEnvironment());

        Assert.False(startup.HasDeferredCompositionPlan);
        Assert.False(startup.Readiness.ConfigurationAdmitted);
        Assert.Equal("invalid", startup.Readiness.ConfigurationStatus);
        Assert.Equal(
            "CONTINUOUS_LEARNING_CONFIGURATION_INVALID",
            startup.Readiness.ConfigurationErrorCode);
        AssertClosedAndUnmaterialized(startup.Readiness);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoOutputDirectories();
    }

    [Fact]
    public void MissingTokenizer_FailsConfigurationAdmissionBeforeRetainedBinding()
    {
        using var fixture = new ModelServerCompositionFixture();
        File.Delete(fixture.TokenizerPath);
        var before = fixture.Snapshot();

        var startup = CreateStartup(fixture.ValidEnvironment());

        Assert.False(startup.HasDeferredCompositionPlan);
        Assert.False(startup.Readiness.ConfigurationAdmitted);
        Assert.Equal("invalid", startup.Readiness.ConfigurationStatus);
        Assert.Equal(
            "CONTINUOUS_LEARNING_CONFIGURATION_INVALID",
            startup.Readiness.ConfigurationErrorCode);
        AssertClosedAndUnmaterialized(startup.Readiness);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoOutputDirectories();
    }

    [Fact]
    public async Task RetainedObjectLoss_ClearsAllPositiveMaterializationReadinessFlags()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        var dependencyRelease = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var dependencyInvocations = 0;
        try
        {
            startup.BindRetainedFileAccess(access);
            startup.MaterializeInactiveTp1Composition(access);
            Assert.True(startup.TryRegisterRetirementDependency(
                () =>
                {
                    Interlocked.Increment(ref dependencyInvocations);
                    return dependencyRelease.Task;
                }));
            Assert.True(startup.Readiness.CompositionMaterialized);
            var composition = GetMaterializedComposition(startup);
            var retainedFactory = GetRetainedSessionFactory(composition);
            Assert.False(GetPrivateBoolean(retainedFactory, "_disposed"));

            access.Dispose();

            var status = startup.Readiness;
            Assert.Equal("retained_object_identity_lost_inactive", status.ConfigurationStatus);
            Assert.Equal(
                ContinuousLearningModelServerStartup.RetainedObjectIdentityLostErrorCode,
                status.ConfigurationErrorCode);
            Assert.False(status.LegalEvidenceVerified);
            AssertClosedAndUnmaterialized(status);
            Assert.Null(GetMaterializedCompositionOrNull(startup));
            var retirement = startup.AwaitRetirementAsync();
            Assert.False(retirement.IsCompleted);
            Assert.Equal(1, Volatile.Read(ref dependencyInvocations));
            Assert.False(GetPrivateBoolean(retainedFactory, "_disposed"));

            dependencyRelease.TrySetResult(null);
            await retirement.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.True(GetPrivateBoolean(retainedFactory, "_disposed"));
            AssertRootOwnerLocksAreAvailable(fixture.ValidEnvironment());
        }
        finally
        {
            dependencyRelease.TrySetResult(null);
            access.Dispose();
        }
    }

    [Fact]
    public void RetainedFileAccessAcquisitionFailure_ClearsEveryActivationFlagAndPreservesFailureIdentity()
    {
        using var fixture = new ModelServerCompositionFixture();
        var startup = CreateStartup(fixture.ValidEnvironment());

        startup.MarkRetainedFileAccessAcquisitionFailed();

        Assert.Equal(
            "retained_object_acquisition_failed_inactive",
            startup.Readiness.ConfigurationStatus);
        Assert.Equal(
            ContinuousLearningPathIdentityException.StableErrorCode,
            startup.Readiness.ConfigurationErrorCode);
        Assert.False(startup.Readiness.LegalEvidenceVerified);
        AssertClosedAndUnmaterialized(startup.Readiness);
        Assert.False(startup.IsLegalTrainingTeacherAdmissionActive());
    }

    [Fact]
    public void CancelledMaterialization_PropagatesCancellationWithoutPublishingReadiness()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(
            () => startup.MaterializeInactiveTp1Composition(access, cancellation.Token));

        Assert.True(startup.Readiness.ConfigurationAdmitted);
        Assert.False(startup.Readiness.CompositionMaterialized);
        Assert.False(startup.Readiness.PathIdentityVerified);
        Assert.False(startup.Readiness.LegalEvidenceVerified);
    }

    [Fact]
    public void CancellationAfterSuccessfulBind_DisposesOwnerCapabilitiesAndClearsReadiness()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var environment = fixture.ValidEnvironment();
        var startup = CreateStartup(environment);
        var before = fixture.Snapshot();
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        using var cancellation = new CancellationTokenSource();

        Assert.Throws<OperationCanceledException>(() =>
            startup.MaterializeInactiveTp1Composition(
                access,
                cancellation.Token,
                cancellation.Cancel));

        Assert.Equal(
            ContinuousLearningModelServerStartup.MaterializationFailedErrorCode,
            startup.Readiness.ConfigurationErrorCode);
        AssertClosedAndUnmaterialized(startup.Readiness);
        access.EnsureRetained();
        access.Dispose();
        AssertRootOwnerLocksAreAvailable(environment);
        AssertOnlyOwnerLockLeavesAdded(before, fixture.Snapshot());
    }

    [Fact]
    public void FailureAfterSuccessfulBind_DisposesOwnerCapabilitiesAndFailsClosed()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var environment = fixture.ValidEnvironment();
        var startup = CreateStartup(environment);
        var before = fixture.Snapshot();
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);

        startup.MaterializeInactiveTp1Composition(
            access,
            afterRetainedSessionFactoryBindForTesting: static () =>
                throw new InvalidDataException("post-bind test failure"));

        Assert.Equal("materialization_failed_inactive", startup.Readiness.ConfigurationStatus);
        Assert.Equal(
            ContinuousLearningModelServerStartup.MaterializationFailedErrorCode,
            startup.Readiness.ConfigurationErrorCode);
        AssertClosedAndUnmaterialized(startup.Readiness);
        access.EnsureRetained();
        access.Dispose();
        AssertRootOwnerLocksAreAvailable(environment);
        AssertOnlyOwnerLockLeavesAdded(before, fixture.Snapshot());
    }

    [Fact]
    public void InactiveSessionFactory_BothOverloadsDenyIndependentlyWithoutArtifactAccess()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        // The retained lease admits identity and metadata only. These deliberately
        // invalid payloads prove that the inactive factory does not read/model-load
        // them before returning its stable execution-denied result.
        File.WriteAllBytes(fixture.BaseCheckpointPath, [0xFF, 0x00, 0xAA]);
        File.WriteAllBytes(fixture.TokenizerPath, [0xC0, 0xAF, 0xED]);
        var before = fixture.Snapshot();
        var factory = new InactiveIncrementalTrainingSessionFactoryGuard();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = factory.Create("domain");
        var cancelledOverloadResult = factory.Create(
            "domain",
            cancellation.Token);
        var singleArgumentCreate = typeof(InactiveIncrementalTrainingSessionFactoryGuard)
            .GetMethod(nameof(IIncrementalTrainingSessionFactory.Create), [typeof(string)]);
        var cancellationCreate = typeof(InactiveIncrementalTrainingSessionFactoryGuard)
            .GetMethod(
                nameof(IIncrementalTrainingSessionFactory.Create),
                [typeof(string), typeof(CancellationToken)]);
        var guardConstructor = typeof(InactiveIncrementalTrainingSessionFactoryGuard)
            .GetConstructor(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic,
                binder: null,
                Type.EmptyTypes,
                modifiers: null);

        Assert.False(result.IsSuccess);
        Assert.Equal("CONTINUOUS_LEARNING_TRAINING_INACTIVE", result.ErrorCode);
        Assert.False(cancelledOverloadResult.IsSuccess);
        Assert.Equal(
            "CONTINUOUS_LEARNING_TRAINING_INACTIVE",
            cancelledOverloadResult.ErrorCode);
        Assert.NotNull(singleArgumentCreate);
        Assert.NotNull(cancellationCreate);
        Assert.NotNull(guardConstructor);
        var selectedSingleArgumentCreate = singleArgumentCreate!;
        var selectedCancellationCreate = cancellationCreate!;
        var selectedGuardConstructor = guardConstructor!;
        Assert.False(ContainsCallTo(
            selectedCancellationCreate,
            selectedSingleArgumentCreate));
        Assert.True(ContainsCallTo(
            EncodeSyntheticMethodCall(OpCodes.Call, selectedSingleArgumentCreate),
            selectedCancellationCreate,
            selectedSingleArgumentCreate));
        Assert.True(ContainsCallTo(
            EncodeSyntheticMethodCall(OpCodes.Callvirt, selectedSingleArgumentCreate),
            selectedCancellationCreate,
            selectedSingleArgumentCreate));
        Assert.False(ContainsCallTo(
            EncodeSyntheticMethodCall(OpCodes.Call, selectedCancellationCreate),
            selectedCancellationCreate,
            selectedSingleArgumentCreate));
        Assert.True(ContainsCallTo(
            EncodeSyntheticMethodCall(OpCodes.Newobj, selectedGuardConstructor),
            selectedCancellationCreate,
            selectedGuardConstructor));
        Assert.Throws<InvalidDataException>(() => ContainsCallTo(
            [unchecked((byte)OpCodes.Call.Value), 0x00, 0x00],
            selectedCancellationCreate,
            selectedSingleArgumentCreate));
        Assert.Equal(before, fixture.Snapshot());
    }

    [Fact]
    public async Task MaterializedComposition_RetainedTrainerDeniesBeforeModelOrArtifactMutation()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var before = fixture.Snapshot();

        var composition = GetMaterializedComposition(startup);
        var retainedFactory = GetRetainedSessionFactory(composition);
        var trainer = Assert.IsType<IncrementalTrainer>(composition.GetType()
            .GetField("_trainer", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(composition));
        var trainerAuthority = trainer.GetType()
            .GetField(
                "_legalTeacherAdmissionAuthority",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(trainer);
        var trainerFactory = trainer.GetType()
            .GetField("_sessionFactory", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(trainer);

        Assert.IsType<InactiveIncrementalTrainingSessionFactoryGuard>(trainerFactory);
        Assert.Same(startup.LegalTeacherAdmissionAuthority, trainerAuthority);
        Assert.NotSame(retainedFactory, trainerFactory);
        Assert.False(GetPrivateBoolean(retainedFactory, "_disposed"));

        var result = await trainer.TrainAsync(
            "BUG",
            [new TrainingExample(
                "prompt",
                "response",
                "BUG",
                1d,
                DateTimeOffset.UtcNow,
                SourceId: "test-source")],
            []);

        Assert.False(result.IsSuccess);
        Assert.Equal("CONTINUOUS_LEARNING_TRAINING_INACTIVE", result.ErrorCode);
        Assert.Equal(before, fixture.Snapshot());
    }

    [Fact]
    public void MaterializedComposition_PassesRepositoryResumeResolverToRetainedFactoryWithoutActivatingTrainer()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var before = fixture.Snapshot();

        var composition = GetMaterializedComposition(startup);
        var retainedFactory = GetRetainedSessionFactory(composition);
        var resolver = Assert.IsType<Func<
            string,
            OperationResult<RepositoryOwnedIncrementalTrainingSessionStartRequest>>>(
                retainedFactory.GetType()
                    .GetField(
                        "_resolveRepositoryStartRequest",
                        BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(retainedFactory));
        var trainer = Assert.IsType<IncrementalTrainer>(composition.GetType()
            .GetField("_trainer", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(composition));
        var trainerFactory = trainer.GetType()
            .GetField("_sessionFactory", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(trainer);

        var resolved = resolver("html-css");
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        Assert.Equal("html-css", resolved.Value!.DomainSlug);
        Assert.False(resolved.Value.ColdStartPolicy.IsAuthorized);
        Assert.IsType<InactiveIncrementalTrainingSessionFactoryGuard>(trainerFactory);
        Assert.NotSame(retainedFactory, trainerFactory);
        Assert.False(startup.Readiness.TrainingEnabled);
        Assert.False(startup.Readiness.CycleExecutionAvailable);
        Assert.False(startup.Readiness.PerformsTraining);
        Assert.Equal(before, fixture.Snapshot());
    }

    [Fact]
    public async Task RetainedDpoPolicyBinding_UsesExactCheckpointAndTokenizerWithoutActivatingComposition()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        var before = fixture.Snapshot();
        using var access = startup.AcquireRetainedFileAccess();

        var result = await DesignConversionRetainedDpoPolicyBaseComposition
            .CreateInactiveProviderAsync(
                access,
                fixture.ModelConfiguration,
                RetainedPolicyModelId,
                RetainedPolicyRevision);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var provider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(result.Value);
        var identity = GetProviderIdentity(provider);
        var modelSetResult = await provider.CreateModelSetAsync(
            CancellationToken.None);
        Assert.True(modelSetResult.IsSuccess, modelSetResult.ErrorMessage);
        var modelSet = Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(
            modelSetResult.Value);
        Assert.Equal(HashFile(fixture.BaseCheckpointPath), identity.BaseCheckpointSha256);
        Assert.Equal(identity, modelSet.Identity);
        Assert.Equal(RetainedPolicyModelId, identity.BaseModelId);
        Assert.Equal(RetainedPolicyRevision, identity.BaseRevision);
        await AssertRetainedCanonicalBpeTokenizerReferenceAsync(
            modelSet,
            fixture.TokenizerPath,
            fixture.ModelConfiguration,
            fixture.Root);
        AssertClosedAndUnmaterialized(startup.Readiness);
        Assert.Equal(before, fixture.Snapshot());
        Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.CacheRoot));
    }

    [Fact]
    public void RetainedDpoPolicyBindingConfiguration_IsCapturedOnlyAfterValidationAndReturnsDefensiveConfigCopies()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();

        Assert.Throws<ContinuousLearningPathIdentityException>(() =>
            startup.CaptureDpoPolicyBaseBindingConfiguration(access));

        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var first = startup.CaptureDpoPolicyBaseBindingConfiguration(access);
        var second = startup.CaptureDpoPolicyBaseBindingConfiguration(access);
        var firstConfig = first.ModelConfig;
        var repeatedFirstConfig = first.ModelConfig;

        Assert.NotSame(first, second);
        Assert.NotSame(firstConfig, repeatedFirstConfig);
        Assert.NotSame(firstConfig, second.ModelConfig);
        Assert.Equal(fixture.ModelConfiguration, firstConfig);
        Assert.Equal(fixture.ModelConfiguration, repeatedFirstConfig);
        Assert.Equal(RetainedPolicyModelId, first.ModelId);
        Assert.Equal(RetainedPolicyRevision, first.ModelVersion);
        Assert.Equal(first.ModelId, second.ModelId);
        Assert.Equal(first.ModelVersion, second.ModelVersion);
    }

    [Fact]
    public async Task RetainedDpoPolicyPromotion_RequiresRegisteredAndGatewayCommittedToken()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var directToken = startup.CaptureDpoPolicyBaseBindingToken(access);

        Assert.False(directToken.IsTrustLossRevocationRegistered);
        Assert.False(directToken.IsGatewayCommitted);
        Assert.False(startup.TryMarkDpoPolicyBaseProviderBound(
            access,
            directToken));
        Assert.True(directToken.TryRegisterTrustLossRevocation(static _ => { }));
        Assert.True(directToken.IsTrustLossRevocationRegistered);
        Assert.False(directToken.IsGatewayCommitted);
        Assert.False(startup.TryMarkDpoPolicyBaseProviderBound(
            access,
            directToken));
        Assert.Equal(
            "materialized_policy_base_binding_pending_inactive",
            startup.Readiness.ConfigurationStatus);
        Assert.False(startup.Readiness.StableDpoExecutorConfigured);

        var configuration =
            startup.CaptureDpoPolicyBaseBindingConfiguration(access);
        var providerResult = await DesignConversionRetainedDpoPolicyBaseComposition
            .CreateInactiveProviderAsync(
                access,
                configuration.ModelConfig,
                configuration.ModelId,
                configuration.ModelVersion);
        var provider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                providerResult.Value);
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var bound = gateway.BindExactProvider(
            provider,
            startup,
            access,
            CancellationToken.None);
        Assert.True(bound.IsSuccess, bound.ErrorMessage);
        var committedToken = Assert.IsType<
            ContinuousLearningDpoPolicyBaseBindingToken>(bound.Value);

        Assert.NotSame(directToken, committedToken);
        Assert.True(committedToken.IsTrustLossRevocationRegistered);
        Assert.True(committedToken.IsGatewayCommitted);
        Assert.True(startup.TryMarkDpoPolicyBaseProviderBound(
            access,
            committedToken));
        Assert.True(startup.Readiness.StableDpoExecutorConfigured);
        AssertStableReadinessRequiresBoundGateway(startup, gateway);
        startup.MarkDpoPolicyBaseStopped();
        gateway.Close();
    }

    [Fact]
    public async Task RetainedDpoPolicyGateway_ConcurrentFreshBindWaitsForOlderAttemptBeforeCommit()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var configuration =
            startup.CaptureDpoPolicyBaseBindingConfiguration(access);
        var firstProviderResult = await
            DesignConversionRetainedDpoPolicyBaseComposition
                .CreateInactiveProviderAsync(
                    access,
                    configuration.ModelConfig,
                    configuration.ModelId,
                    configuration.ModelVersion,
                    configuration.RootInitializationSeed,
                    configuration.TrainingConfigurationFingerprint);
        Assert.True(
            firstProviderResult.IsSuccess,
            firstProviderResult.ErrorMessage);
        var secondProviderResult = await
            DesignConversionRetainedDpoPolicyBaseComposition
                .CreateInactiveProviderAsync(
                    access,
                    configuration.ModelConfig,
                    configuration.ModelId,
                    configuration.ModelVersion,
                    configuration.RootInitializationSeed,
                    configuration.TrainingConfigurationFingerprint);
        Assert.True(
            secondProviderResult.IsSuccess,
            secondProviderResult.ErrorMessage);
        var firstProvider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                firstProviderResult.Value);
        var secondProvider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                secondProviderResult.Value);
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var firstCaptured = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var secondStarted = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var secondCaptured = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var firstBind = Task.Run(() => gateway.BindExactProvider(
            firstProvider,
            startup,
            access,
            CancellationToken.None,
            afterValidationBeforeCommitForTesting: () =>
            {
                firstCaptured.TrySetResult(null);
                releaseFirst.Task.GetAwaiter().GetResult();
            }));

        await firstCaptured.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var secondBind = Task.Run(() =>
        {
            secondStarted.TrySetResult(null);
            return gateway.BindExactProvider(
                secondProvider,
                startup,
                access,
                CancellationToken.None,
                afterValidationBeforeCommitForTesting: () =>
                {
                    secondCaptured.TrySetResult(null);
                });
        });

        try
        {
            await secondStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var prematureSecondCapture = await Task.WhenAny(
                secondCaptured.Task,
                Task.Delay(TimeSpan.FromMilliseconds(250)));

            Assert.NotSame(secondCaptured.Task, prematureSecondCapture);
            Assert.False(secondBind.IsCompleted);

            releaseFirst.TrySetResult(null);
            var firstResult = await firstBind.WaitAsync(
                TimeSpan.FromSeconds(5));
            var secondResult = await secondBind.WaitAsync(
                TimeSpan.FromSeconds(5));

            Assert.True(firstResult.IsSuccess, firstResult.ErrorMessage);
            Assert.NotNull(firstResult.Value);
            Assert.False(secondResult.IsSuccess);
            Assert.Equal(
                DesignConversionRetainedDpoPolicyBaseProviderGateway
                    .ProviderUnavailableErrorCode,
                secondResult.ErrorCode);
            Assert.True(startup.TryMarkDpoPolicyBaseProviderBound(
                access,
                firstResult.Value));
            Assert.True(startup.Readiness.StableDpoExecutorConfigured);
            AssertStableReadinessRequiresBoundGateway(startup, gateway);

            var repeatWinner = gateway.BindExactProvider(
                firstProvider,
                startup,
                access,
                CancellationToken.None);
            Assert.True(repeatWinner.IsSuccess, repeatWinner.ErrorMessage);
            Assert.Same(firstResult.Value, repeatWinner.Value);

            var competingRebind = gateway.BindExactProvider(
                secondProvider,
                startup,
                access,
                CancellationToken.None);
            Assert.False(competingRebind.IsSuccess);
            Assert.Equal(
                DesignConversionRetainedDpoPolicyBaseProviderGateway
                    .ProviderUnavailableErrorCode,
                competingRebind.ErrorCode);
        }
        finally
        {
            releaseFirst.TrySetResult(null);
            gateway.Close();
            if (startup.Readiness.CompositionMaterialized)
            {
                startup.MarkDpoPolicyBaseStopped();
                await startup.AwaitRetirementAsync()
                    .WaitAsync(TimeSpan.FromSeconds(5));
            }
            await gateway.AwaitRetirementAsync()
                .WaitAsync(TimeSpan.FromSeconds(5));
        }
    }

    [Fact]
    public async Task RetainedDpoPolicyGateway_ConcurrentFreshBindCannotCaptureWhileAttemptReserved()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var configuration =
            startup.CaptureDpoPolicyBaseBindingConfiguration(access);
        var firstProviderResult = await
            DesignConversionRetainedDpoPolicyBaseComposition
                .CreateInactiveProviderAsync(
                    access,
                    configuration.ModelConfig,
                    configuration.ModelId,
                    configuration.ModelVersion,
                    configuration.RootInitializationSeed,
                    configuration.TrainingConfigurationFingerprint);
        Assert.True(
            firstProviderResult.IsSuccess,
            firstProviderResult.ErrorMessage);
        var secondProviderResult = await
            DesignConversionRetainedDpoPolicyBaseComposition
                .CreateInactiveProviderAsync(
                    access,
                    configuration.ModelConfig,
                    configuration.ModelId,
                    configuration.ModelVersion,
                    configuration.RootInitializationSeed,
                    configuration.TrainingConfigurationFingerprint);
        Assert.True(
            secondProviderResult.IsSuccess,
            secondProviderResult.ErrorMessage);
        var firstProvider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                firstProviderResult.Value);
        var secondProvider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                secondProviderResult.Value);
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var firstReserved = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var secondStarted = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var secondReserved = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var reservations = 0;
        var prior = DesignConversionRetainedDpoPolicyBaseProviderGateway
            .FreshBindingReservedBeforeStartupCaptureObserverForTesting;

        try
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .FreshBindingReservedBeforeStartupCaptureObserverForTesting =
                () =>
                {
                    if (Interlocked.Increment(ref reservations) == 1)
                    {
                        firstReserved.TrySetResult(null);
                        releaseFirst.Task.GetAwaiter().GetResult();
                        return;
                    }
                    secondReserved.TrySetResult(null);
                };

            var firstBind = Task.Run(() => gateway.BindExactProvider(
                firstProvider,
                startup,
                access,
                CancellationToken.None));

            await firstReserved.Task.WaitAsync(TimeSpan.FromSeconds(5));

            var secondBind = Task.Run(() =>
            {
                secondStarted.TrySetResult(null);
                return gateway.BindExactProvider(
                    secondProvider,
                    startup,
                    access,
                    CancellationToken.None);
            });

            await secondStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var prematureSecondReservation = await Task.WhenAny(
                secondReserved.Task,
                Task.Delay(TimeSpan.FromMilliseconds(250)));

            Assert.NotSame(secondReserved.Task, prematureSecondReservation);
            Assert.False(secondBind.IsCompleted);

            releaseFirst.TrySetResult(null);
            var firstResult = await firstBind.WaitAsync(
                TimeSpan.FromSeconds(5));
            var secondResult = await secondBind.WaitAsync(
                TimeSpan.FromSeconds(5));

            Assert.True(firstResult.IsSuccess, firstResult.ErrorMessage);
            Assert.NotNull(firstResult.Value);
            Assert.False(secondResult.IsSuccess);
            Assert.Equal(
                DesignConversionRetainedDpoPolicyBaseProviderGateway
                    .ProviderUnavailableErrorCode,
                secondResult.ErrorCode);
            Assert.True(startup.TryMarkDpoPolicyBaseProviderBound(
                access,
                firstResult.Value));
            Assert.True(startup.Readiness.StableDpoExecutorConfigured);
            AssertStableReadinessRequiresBoundGateway(startup, gateway);

            var repeatFirst = gateway.BindExactProvider(
                firstProvider,
                startup,
                access,
                CancellationToken.None);
            Assert.True(repeatFirst.IsSuccess, repeatFirst.ErrorMessage);
            Assert.Same(firstResult.Value, repeatFirst.Value);

            var competingSecond = gateway.BindExactProvider(
                secondProvider,
                startup,
                access,
                CancellationToken.None);
            Assert.False(competingSecond.IsSuccess);
            Assert.Equal(
                DesignConversionRetainedDpoPolicyBaseProviderGateway
                    .ProviderUnavailableErrorCode,
                competingSecond.ErrorCode);
        }
        finally
        {
            releaseFirst.TrySetResult(null);
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .FreshBindingReservedBeforeStartupCaptureObserverForTesting =
                prior;
            gateway.Close();
            if (startup.Readiness.CompositionMaterialized)
            {
                startup.MarkDpoPolicyBaseStopped();
                await startup.AwaitRetirementAsync()
                    .WaitAsync(TimeSpan.FromSeconds(5));
            }
            await gateway.AwaitRetirementAsync()
                .WaitAsync(TimeSpan.FromSeconds(5));
        }
    }

    [Fact]
    public async Task RetainedDpoPolicyGateway_UnbindWhileFreshBindReservedInvalidatesAttemptAndAllowsRetry()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var configuration =
            startup.CaptureDpoPolicyBaseBindingConfiguration(access);
        var firstProviderResult = await
            DesignConversionRetainedDpoPolicyBaseComposition
                .CreateInactiveProviderAsync(
                    access,
                    configuration.ModelConfig,
                    configuration.ModelId,
                    configuration.ModelVersion,
                    configuration.RootInitializationSeed,
                    configuration.TrainingConfigurationFingerprint);
        Assert.True(
            firstProviderResult.IsSuccess,
            firstProviderResult.ErrorMessage);
        var secondProviderResult = await
            DesignConversionRetainedDpoPolicyBaseComposition
                .CreateInactiveProviderAsync(
                    access,
                    configuration.ModelConfig,
                    configuration.ModelId,
                    configuration.ModelVersion,
                    configuration.RootInitializationSeed,
                    configuration.TrainingConfigurationFingerprint);
        Assert.True(
            secondProviderResult.IsSuccess,
            secondProviderResult.ErrorMessage);
        var firstProvider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                firstProviderResult.Value);
        var secondProvider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                secondProviderResult.Value);
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var firstReserved = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var prior = DesignConversionRetainedDpoPolicyBaseProviderGateway
            .FreshBindingReservedBeforeStartupCaptureObserverForTesting;

        try
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .FreshBindingReservedBeforeStartupCaptureObserverForTesting =
                () =>
                {
                    firstReserved.TrySetResult(null);
                    releaseFirst.Task.GetAwaiter().GetResult();
                };

            var firstBind = Task.Run(() => gateway.BindExactProvider(
                firstProvider,
                startup,
                access,
                CancellationToken.None));

            await firstReserved.Task.WaitAsync(TimeSpan.FromSeconds(5));
            gateway.Unbind();
            releaseFirst.TrySetResult(null);
            var firstResult = await firstBind.WaitAsync(
                TimeSpan.FromSeconds(5));

            Assert.False(firstResult.IsSuccess);
            Assert.Equal(
                DesignConversionRetainedDpoPolicyBaseProviderGateway
                    .ProviderUnavailableErrorCode,
                firstResult.ErrorCode);
            Assert.False(gateway.IsBound);
            Assert.False(startup.Readiness.StableDpoExecutorConfigured);

            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .FreshBindingReservedBeforeStartupCaptureObserverForTesting =
                null;
            var retry = gateway.BindExactProvider(
                secondProvider,
                startup,
                access,
                CancellationToken.None);

            Assert.True(retry.IsSuccess, retry.ErrorMessage);
            Assert.NotNull(retry.Value);
            Assert.True(startup.TryMarkDpoPolicyBaseProviderBound(
                access,
                retry.Value));
            Assert.True(startup.Readiness.StableDpoExecutorConfigured);
            AssertStableReadinessRequiresBoundGateway(startup, gateway);
        }
        finally
        {
            releaseFirst.TrySetResult(null);
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .FreshBindingReservedBeforeStartupCaptureObserverForTesting =
                prior;
            gateway.Close();
            if (startup.Readiness.CompositionMaterialized)
            {
                startup.MarkDpoPolicyBaseStopped();
                await startup.AwaitRetirementAsync()
                    .WaitAsync(TimeSpan.FromSeconds(5));
            }
            await gateway.AwaitRetirementAsync()
                .WaitAsync(TimeSpan.FromSeconds(5));
        }
    }

    [Fact]
    public async Task RetainedDpoPolicyGateway_UnboundAndCancelledCallsFailClosedWithoutDelegation()
    {
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();

        var unbound = await gateway.CreateModelSetAsync(CancellationToken.None);
        var unboundContinuation = await gateway.CreateContinuedModelSetAsync(
            continuationRequest: null!,
            CancellationToken.None);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelled = await gateway.CreateModelSetAsync(cancellation.Token);

        AssertFailure(
            unbound,
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ProviderUnavailableErrorCode,
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ProviderUnavailableErrorMessage);
        AssertFailure(
            unboundContinuation,
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ProviderUnavailableErrorCode,
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ProviderUnavailableErrorMessage);
        AssertFailure(
            cancelled,
            ContinuousLearningRetainedDpoPolicyCheckpointSource.CancelledErrorCode,
            ContinuousLearningRetainedDpoPolicyCheckpointSource.CancelledErrorMessage);
        Assert.False(gateway.IsBound);
    }

    [Fact]
    public async Task RetainedDpoPolicyGateway_BindsExactProviderDelegatesContinuationAndRevokesAfterTrustLoss()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        var access = startup.AcquireRetainedFileAccess();
        try
        {
            startup.BindRetainedFileAccess(access);
            startup.MaterializeInactiveTp1Composition(access);
            var bindingConfiguration =
                startup.CaptureDpoPolicyBaseBindingConfiguration(access);
            var providerResult = await DesignConversionRetainedDpoPolicyBaseComposition
                .CreateInactiveProviderAsync(
                    access,
                    bindingConfiguration.ModelConfig,
                    bindingConfiguration.ModelId,
                    bindingConfiguration.ModelVersion);
            var provider = Assert.IsType<
                DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                    providerResult.Value);
            var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
            var cancelledGateway =
                new DesignConversionRetainedDpoPolicyBaseProviderGateway();
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var cancelledBind = cancelledGateway.BindExactProvider(
                provider,
                startup,
                access,
                cancellation.Token);

            var bound = gateway.BindExactProvider(
                provider,
                startup,
                access,
                CancellationToken.None);
            Assert.True(bound.IsSuccess, bound.ErrorMessage);
            Assert.NotNull(bound.Value);
            Assert.True(startup.TryMarkDpoPolicyBaseProviderBound(
                access,
                bound.Value));
            var modelSetResult = await gateway.CreateModelSetAsync(
                CancellationToken.None);
            var continuationResult = await gateway.CreateContinuedModelSetAsync(
                continuationRequest: null!,
                CancellationToken.None);

            AssertFailure(
                cancelledBind,
                ContinuousLearningRetainedDpoPolicyCheckpointSource.CancelledErrorCode,
                ContinuousLearningRetainedDpoPolicyCheckpointSource.CancelledErrorMessage);
            Assert.False(cancelledGateway.IsBound);
            Assert.True(gateway.IsBound);
            AssertStableReadinessRequiresBoundGateway(startup, gateway);
            Assert.True(modelSetResult.IsSuccess, modelSetResult.ErrorMessage);
            var modelSet = Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(
                modelSetResult.Value);
            Assert.Equal(RetainedPolicyModelId, modelSet.Identity.BaseModelId);
            Assert.Equal(RetainedPolicyRevision, modelSet.Identity.BaseRevision);
            Assert.Equal(fixture.ModelConfiguration, modelSet.Policy.Config);
            await AssertRetainedCanonicalBpeTokenizerReferenceAsync(
                modelSet,
                fixture.TokenizerPath,
                fixture.ModelConfiguration,
                fixture.Root);
            AssertFailure(
                continuationResult,
                DesignConversionCheckpointBackedDpoPolicyBaseProvider
                    .ContinuationContractInvalidErrorCode,
                "The DesignConversion DPO continuation contract is invalid.");

            access.Dispose();
            Assert.False(gateway.IsBound);
            Assert.Equal(
                ContinuousLearningModelServerStartup
                    .RetainedObjectIdentityLostErrorCode,
                startup.Readiness.ConfigurationErrorCode);
            AssertStableReadinessRequiresBoundGateway(startup, gateway);
            var afterTrustLoss = await gateway.CreateContinuedModelSetAsync(
                continuationRequest: null!,
                CancellationToken.None);
            AssertFailure(
                afterTrustLoss,
                DesignConversionRetainedDpoPolicyBaseProviderGateway
                    .ProviderUnavailableErrorCode,
                DesignConversionRetainedDpoPolicyBaseProviderGateway
                    .ProviderUnavailableErrorMessage);
            Assert.False(gateway.IsBound);
            AssertStableReadinessRequiresBoundGateway(startup, gateway);

            gateway.Unbind();
            var afterUnbind = await gateway.CreateModelSetAsync(
                CancellationToken.None);
            AssertFailure(
                afterUnbind,
                DesignConversionRetainedDpoPolicyBaseProviderGateway
                    .ProviderUnavailableErrorCode,
                DesignConversionRetainedDpoPolicyBaseProviderGateway
                    .ProviderUnavailableErrorMessage);
        }
        finally
        {
            access.Dispose();
        }
    }

    [Fact]
    public async Task BindRetainedFileAccess_RebindAfterProviderPromotionFailsWithoutCorruptingHealth()
    {
        using var fixture = new ModelServerCompositionFixture();
        using var foreignFixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        foreignFixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        var foreignStartup = CreateStartup(foreignFixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        using var foreignAccess = foreignStartup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var configuration =
            startup.CaptureDpoPolicyBaseBindingConfiguration(access);
        var providerResult = await DesignConversionRetainedDpoPolicyBaseComposition
            .CreateInactiveProviderAsync(
                access,
                configuration.ModelConfig,
                configuration.ModelId,
                configuration.ModelVersion);
        var provider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                providerResult.Value);
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var bound = gateway.BindExactProvider(
            provider,
            startup,
            access,
            CancellationToken.None);
        Assert.True(bound.IsSuccess, bound.ErrorMessage);
        Assert.NotNull(bound.Value);
        Assert.True(startup.TryMarkDpoPolicyBaseProviderBound(
            access,
            bound.Value));
        var healthyReadiness = startup.Readiness;

        startup.BindRetainedFileAccess(access);
        Assert.Equal(healthyReadiness, startup.Readiness);
        Assert.Throws<ContinuousLearningPathIdentityException>(() =>
            startup.BindRetainedFileAccess(foreignAccess));

        Assert.Equal(healthyReadiness, startup.Readiness);
        Assert.True(gateway.IsBound);
        Assert.True(startup.Readiness.StableDpoExecutorConfigured);
        AssertStableReadinessRequiresBoundGateway(startup, gateway);
        startup.MarkDpoPolicyBaseStopped();
        gateway.Close();
    }

    [Fact]
    public async Task RetainedDpoPolicyGateway_SanitizesNonFatalDelegateFailureAndLetsFatalFailureEscape()
    {
        const string privateFailure =
            "C:/private/checkpoints/gateway-delegate-failure";
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);

        var nonFatalProvider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
                    new ThrowingDpoPolicyCheckpointSource(
                        new IOException(privateFailure)),
                    fixture.ModelConfiguration,
                    RetainedPolicyModelId,
                    RetainedPolicyRevision,
                    new string('a', 64)).Value);
        var nonFatalGateway =
            new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var nonFatalBinding = nonFatalGateway.BindExactProvider(
            nonFatalProvider,
            startup,
            access,
            CancellationToken.None);
        Assert.True(nonFatalBinding.IsSuccess, nonFatalBinding.ErrorMessage);
        Assert.NotNull(nonFatalBinding.Value);
        Assert.True(startup.TryMarkDpoPolicyBaseProviderBound(
            access,
            nonFatalBinding.Value));
        AssertStableReadinessRequiresBoundGateway(startup, nonFatalGateway);

        var nonFatal = await nonFatalGateway.CreateModelSetAsync(
            CancellationToken.None);

        AssertFailure(
            nonFatal,
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .CheckpointSourceFailedErrorCode,
            "The DesignConversion DPO policy base checkpoint source failed.");
        Assert.DoesNotContain(privateFailure, nonFatal.ErrorMessage, StringComparison.Ordinal);
        Assert.False(nonFatalGateway.IsBound);
        Assert.Equal(
            ContinuousLearningModelServerStartup
                .DpoPolicyBaseBindingFailedErrorCode,
            startup.Readiness.ConfigurationErrorCode);
        AssertStableReadinessRequiresBoundGateway(startup, nonFatalGateway);

        access.Dispose();
        var fatalStartup = CreateStartup(fixture.ValidEnvironment());
        using var fatalAccess = fatalStartup.AcquireRetainedFileAccess();
        fatalStartup.BindRetainedFileAccess(fatalAccess);
        fatalStartup.MaterializeInactiveTp1Composition(fatalAccess);

        var fatalProvider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
                    new ThrowingDpoPolicyCheckpointSource(
                        new OutOfMemoryException("fatal-private-marker")),
                    fixture.ModelConfiguration,
                    RetainedPolicyModelId,
                    RetainedPolicyRevision,
                    new string('b', 64)).Value);
        var fatalGateway =
            new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var fatalBinding = fatalGateway.BindExactProvider(
            fatalProvider,
            fatalStartup,
            fatalAccess,
            CancellationToken.None);
        Assert.True(fatalBinding.IsSuccess, fatalBinding.ErrorMessage);
        Assert.NotNull(fatalBinding.Value);
        Assert.True(fatalStartup.TryMarkDpoPolicyBaseProviderBound(
            fatalAccess,
            fatalBinding.Value));

        await Assert.ThrowsAsync<OutOfMemoryException>(() =>
            fatalGateway.CreateModelSetAsync(CancellationToken.None));
        AssertStableReadinessRequiresBoundGateway(fatalStartup, fatalGateway);
        fatalStartup.MarkDpoPolicyBaseStopped();
        fatalGateway.Close();
    }

    [Fact]
    public async Task RetainedDpoPolicyGateway_DescriptorFromPriorEquivalentBindingFailsBeforeInvocation()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        OperationResult<DesignConversionDpoPolicyBasePreparationDescriptor>
            staleDescriptor;

        var firstStartup = CreateStartup(fixture.ValidEnvironment());
        using (var firstAccess = firstStartup.AcquireRetainedFileAccess())
        {
            firstStartup.BindRetainedFileAccess(firstAccess);
            firstStartup.MaterializeInactiveTp1Composition(firstAccess);
            var firstConfiguration =
                firstStartup.CaptureDpoPolicyBaseBindingConfiguration(firstAccess);
            var firstProvider = Assert.IsType<
                DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                    (await DesignConversionRetainedDpoPolicyBaseComposition
                        .CreateInactiveProviderAsync(
                            firstAccess,
                            firstConfiguration.ModelConfig,
                            firstConfiguration.ModelId,
                            firstConfiguration.ModelVersion)).Value);
            var firstBinding = gateway.BindExactProvider(
                firstProvider,
                firstStartup,
                firstAccess,
                CancellationToken.None);
            Assert.True(firstBinding.IsSuccess, firstBinding.ErrorMessage);
            Assert.True(firstStartup.TryMarkDpoPolicyBaseProviderBound(
                firstAccess,
                firstBinding.Value!));
            staleDescriptor = gateway.DescribePreparation();
            Assert.True(staleDescriptor.IsSuccess, staleDescriptor.ErrorMessage);
            Assert.NotNull(staleDescriptor.Value);
            gateway.Unbind();
            await gateway.AwaitRetirementAsync();
        }

        var secondStartup = CreateStartup(fixture.ValidEnvironment());
        using var secondAccess = secondStartup.AcquireRetainedFileAccess();
        secondStartup.BindRetainedFileAccess(secondAccess);
        secondStartup.MaterializeInactiveTp1Composition(secondAccess);
        var secondConfiguration =
            secondStartup.CaptureDpoPolicyBaseBindingConfiguration(secondAccess);
        var secondProvider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                (await DesignConversionRetainedDpoPolicyBaseComposition
                    .CreateInactiveProviderAsync(
                        secondAccess,
                        secondConfiguration.ModelConfig,
                        secondConfiguration.ModelId,
                        secondConfiguration.ModelVersion)).Value);
        var secondBinding = gateway.BindExactProvider(
            secondProvider,
            secondStartup,
            secondAccess,
            CancellationToken.None);
        Assert.True(secondBinding.IsSuccess, secondBinding.ErrorMessage);
        Assert.True(secondStartup.TryMarkDpoPolicyBaseProviderBound(
            secondAccess,
            secondBinding.Value!));

        var invocationBoundaryCount = 0;
        var priorObserver =
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .InvocationLeaseAcquiredObserverForTesting;
        try
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .InvocationLeaseAcquiredObserverForTesting = () =>
                {
                    Interlocked.Increment(ref invocationBoundaryCount);
                    return Task.CompletedTask;
                };
            var result = await gateway.CreateModelSetAsync(
                staleDescriptor.Value!,
                CancellationToken.None);

            AssertFailure(
                result,
                DesignConversionRetainedDpoPolicyBaseProviderGateway
                    .ProviderUnavailableErrorCode,
                DesignConversionRetainedDpoPolicyBaseProviderGateway
                    .ProviderUnavailableErrorMessage);
            Assert.Equal(0, invocationBoundaryCount);
        }
        finally
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .InvocationLeaseAcquiredObserverForTesting = priorObserver;
            secondStartup.MarkDpoPolicyBaseStopped();
            gateway.Close();
        }
    }

    [Fact]
    public async Task RetainedDpoPolicyGateway_GenerationBarriersRejectCancelledStaleAndRemovedBindings()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var bindingConfiguration =
            startup.CaptureDpoPolicyBaseBindingConfiguration(access);
        var providerResult = await DesignConversionRetainedDpoPolicyBaseComposition
            .CreateInactiveProviderAsync(
                access,
                bindingConfiguration.ModelConfig,
                bindingConfiguration.ModelId,
                bindingConfiguration.ModelVersion);
        var provider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                providerResult.Value);

        var cancellationGateway =
            new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        using var cancellation = new CancellationTokenSource();
        var cancelledBind = cancellationGateway.BindExactProvider(
            provider,
            startup,
            access,
            cancellation.Token,
            cancellation.Cancel);
        AssertFailure(
            cancelledBind,
            ContinuousLearningRetainedDpoPolicyCheckpointSource.CancelledErrorCode,
            ContinuousLearningRetainedDpoPolicyCheckpointSource.CancelledErrorMessage);
        Assert.False(cancellationGateway.IsBound);

        var unboundDuringCommitGateway =
            new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var unboundDuringCommit = unboundDuringCommitGateway.BindExactProvider(
            provider,
            startup,
            access,
            CancellationToken.None,
            unboundDuringCommitGateway.Unbind);
        AssertFailure(
            unboundDuringCommit,
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ProviderUnavailableErrorCode,
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ProviderUnavailableErrorMessage);
        Assert.False(unboundDuringCommitGateway.IsBound);

        var identicalBindingGateway =
            new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        Assert.True(identicalBindingGateway.BindExactProvider(
            provider,
            startup,
            access,
            CancellationToken.None).IsSuccess);
        var identicalAfterUnbind = identicalBindingGateway.BindExactProvider(
            provider,
            startup,
            access,
            CancellationToken.None,
            identicalBindingGateway.Unbind);
        AssertFailure(
            identicalAfterUnbind,
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ProviderUnavailableErrorCode,
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ProviderUnavailableErrorMessage);
        Assert.False(identicalBindingGateway.IsBound);

        access.Dispose();
        var staleStartup = CreateStartup(fixture.ValidEnvironment());
        using var staleAccess = staleStartup.AcquireRetainedFileAccess();
        staleStartup.BindRetainedFileAccess(staleAccess);
        staleStartup.MaterializeInactiveTp1Composition(staleAccess);
        var staleBindingConfiguration =
            staleStartup.CaptureDpoPolicyBaseBindingConfiguration(staleAccess);
        var staleProviderResult = await DesignConversionRetainedDpoPolicyBaseComposition
            .CreateInactiveProviderAsync(
                staleAccess,
                staleBindingConfiguration.ModelConfig,
                staleBindingConfiguration.ModelId,
                staleBindingConfiguration.ModelVersion);
        var staleProvider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                staleProviderResult.Value);
        var staleInvocationGateway =
            new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var staleBinding = staleInvocationGateway.BindExactProvider(
            staleProvider,
            staleStartup,
            staleAccess,
            CancellationToken.None);
        Assert.True(staleBinding.IsSuccess, staleBinding.ErrorMessage);
        Assert.NotNull(staleBinding.Value);
        Assert.True(staleStartup.TryMarkDpoPolicyBaseProviderBound(
            staleAccess,
            staleBinding.Value));
        AssertStableReadinessRequiresBoundGateway(
            staleStartup,
            staleInvocationGateway);
        var staleInvocation = await staleInvocationGateway
            .CreateModelSetAsyncForTesting(
                staleInvocationGateway.Unbind,
                CancellationToken.None);
        AssertFailure(
            staleInvocation,
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ProviderUnavailableErrorCode,
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ProviderUnavailableErrorMessage);
        Assert.False(staleInvocationGateway.IsBound);
        AssertStableReadinessRequiresBoundGateway(
            staleStartup,
            staleInvocationGateway);
    }

    [Fact]
    public async Task RetainedDpoPolicyGateway_NullProviderResultWithdrawsReadinessBeforeRevocation()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var bindingConfiguration =
            startup.CaptureDpoPolicyBaseBindingConfiguration(access);
        var providerResult = await DesignConversionRetainedDpoPolicyBaseComposition
            .CreateInactiveProviderAsync(
                access,
                bindingConfiguration.ModelConfig,
                bindingConfiguration.ModelId,
                bindingConfiguration.ModelVersion);
        var provider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                providerResult.Value);
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var bound = gateway.BindExactProvider(
            provider,
            startup,
            access,
            CancellationToken.None);
        Assert.True(bound.IsSuccess, bound.ErrorMessage);
        Assert.NotNull(bound.Value);
        Assert.True(startup.TryMarkDpoPolicyBaseProviderBound(
            access,
            bound.Value));
        AssertStableReadinessRequiresBoundGateway(startup, gateway);

        var result = await gateway.CreateNullModelSetResultAsyncForTesting(
            CancellationToken.None);

        AssertFailure(
            result,
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ProviderInvocationFailedErrorCode,
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ProviderInvocationFailedErrorMessage);
        Assert.False(gateway.IsBound);
        Assert.Equal(
            ContinuousLearningModelServerStartup
                .DpoPolicyBaseBindingFailedErrorCode,
            startup.Readiness.ConfigurationErrorCode);
        AssertStableReadinessRequiresBoundGateway(startup, gateway);
    }

    [Fact]
    public async Task HostedProviderBindingFailure_SanitizesReadinessAfterMaterialization()
    {
        const string privateFailure = "C:/private/provider-binding-failure";
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var activator = new ContinuousLearningRetainedObjectHostedActivator(
            access,
            gateway,
            startup);

        await activator.ActivatePolicyBaseProviderAsync(
            CancellationToken.None,
            () => throw new IOException(privateFailure));

        Assert.False(gateway.IsBound);
        Assert.Equal(
            ContinuousLearningModelServerStartup
                .DpoPolicyBaseBindingFailedErrorCode,
            startup.Readiness.ConfigurationErrorCode);
        Assert.Equal(
            "dpo_policy_base_binding_failed_inactive",
            startup.Readiness.ConfigurationStatus);
        AssertClosedAndUnmaterialized(startup.Readiness);
        Assert.DoesNotContain(
            privateFailure,
            JsonSerializer.Serialize(startup.Readiness),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task HostedProviderBindingFailure_DoesNotOverwriteStickyTrustLoss()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var activator = new ContinuousLearningRetainedObjectHostedActivator(
            access,
            gateway,
            startup);

        await activator.ActivatePolicyBaseProviderAsync(
            CancellationToken.None,
            access.Dispose);

        Assert.False(gateway.IsBound);
        Assert.Equal(
            ContinuousLearningModelServerStartup.RetainedObjectIdentityLostErrorCode,
            startup.Readiness.ConfigurationErrorCode);
        Assert.Equal(
            "retained_object_identity_lost_inactive",
            startup.Readiness.ConfigurationStatus);
        AssertClosedAndUnmaterialized(startup.Readiness);
    }

    [Fact]
    public async Task HostedStopBetweenGatewayBindAndReadinessPromotionCannotRepublishHealth()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var activator = new ContinuousLearningRetainedObjectHostedActivator(
            access,
            gateway,
            startup);

        await activator.ActivatePolicyBaseProviderAsync(
            CancellationToken.None,
            afterProviderCreationBeforeBindingForTesting: null,
            afterGatewayBindingBeforeReadinessPromotionForTesting: () => activator
                .StopAsync(CancellationToken.None)
                .GetAwaiter()
                .GetResult());

        Assert.False(gateway.IsBound);
        Assert.Equal(
            ContinuousLearningModelServerStartup.DpoPolicyBaseStoppedErrorCode,
            startup.Readiness.ConfigurationErrorCode);
        Assert.Equal(
            "dpo_policy_base_stopped_inactive",
            startup.Readiness.ConfigurationStatus);
        AssertClosedAndUnmaterialized(startup.Readiness);
        AssertStableReadinessRequiresBoundGateway(startup, gateway);
    }

    [Fact]
    public async Task StartupRetirement_IsPublishedAndNonBlockingUntilControlledDependencyDrains()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var activator = new ContinuousLearningRetainedObjectHostedActivator(
            access, gateway, startup);
        await activator.ActivatePolicyBaseProviderAsync(
            CancellationToken.None, null, null);
        var dependencyRelease = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var dependencyInvocations = 0;
        Assert.True(startup.TryRegisterRetirementDependency(() =>
        {
            Interlocked.Increment(ref dependencyInvocations);
            return dependencyRelease.Task;
        }));
        Assert.False(startup.Readiness.TrainingEnabled);
        Assert.False(startup.Readiness.CycleExecutionAvailable);
        Assert.False(startup.Readiness.PerformsTraining);
        Assert.False(startup.IsLegalTrainingTeacherAdmissionActive());
        Task? published = null;
        var prior = ContinuousLearningModelServerStartup
            .RetirementTaskPublishedObserverForTesting;
        try
        {
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = task => published = task;
            var callback = Task.Run(startup.MarkDpoPolicyBaseStopped);
            await callback.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.NotNull(published);
            Assert.False(published!.IsCompleted);
            Assert.Same(published, startup.AwaitRetirementAsync());
            Assert.False(gateway.IsBound);
            AssertClosedAndUnmaterialized(startup.Readiness);
            Assert.Equal(1, Volatile.Read(ref dependencyInvocations));
            dependencyRelease.TrySetResult(null);
            await published.WaitAsync(TimeSpan.FromSeconds(5));
            await startup.AwaitRetirementAsync().WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(1, Volatile.Read(ref dependencyInvocations));
        }
        finally
        {
            dependencyRelease.TrySetResult(null);
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = prior;
        }
    }

    [Fact]
    public async Task StartupRetirement_ThrowingPublicationObserverStillRevokesAndDrains()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var activator = new ContinuousLearningRetainedObjectHostedActivator(
            access, gateway, startup);
        await activator.ActivatePolicyBaseProviderAsync(
            CancellationToken.None, null, null);
        var dependencyRelease = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var dependencyInvocations = 0;
        Assert.True(startup.TryRegisterRetirementDependency(() =>
        {
            Interlocked.Increment(ref dependencyInvocations);
            return dependencyRelease.Task;
        }));
        Assert.False(startup.Readiness.TrainingEnabled);
        Assert.False(startup.Readiness.CycleExecutionAvailable);
        Assert.False(startup.Readiness.PerformsTraining);
        Assert.False(startup.IsLegalTrainingTeacherAdmissionActive());
        var observerFailure = new InvalidOperationException(
            "retirement-observer-failed");
        var prior = ContinuousLearningModelServerStartup
            .RetirementTaskPublishedObserverForTesting;
        try
        {
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = _ =>
                    throw observerFailure;

            startup.MarkDpoPolicyBaseStopped();
            var retirement = startup.AwaitRetirementAsync();
            Assert.False(retirement.IsCompleted);
            Assert.False(gateway.IsBound);
            AssertClosedAndUnmaterialized(startup.Readiness);
            Assert.Equal(1, Volatile.Read(ref dependencyInvocations));

            dependencyRelease.TrySetResult(null);
            var thrown = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await retirement.WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.Same(observerFailure, thrown);
            var gatewayThrown = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await gateway.AwaitRetirementAsync()
                    .WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.Same(observerFailure, gatewayThrown);
            Assert.Equal(1, Volatile.Read(ref dependencyInvocations));
        }
        finally
        {
            dependencyRelease.TrySetResult(null);
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = prior;
        }
    }

    [Fact]
    public async Task StartupRetirement_InvokesAndObservesEveryDependencyInRegistrationOrder()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        var observerFailure = new IOException("observer-nonfatal");
        var firstFatal = new OutOfMemoryException("first-fatal-dependency");
        var secondFatal = new StackOverflowException("second-fatal-dependency");
        var laterFailure = new IOException("later-dependency");
        var invoked = 0;
        Assert.True(startup.TryRegisterRetirementDependency(() =>
        {
            invoked++;
            return Task.FromException(firstFatal);
        }));
        Assert.True(startup.TryRegisterRetirementDependency(() =>
        {
            invoked++;
            throw secondFatal;
        }));
        Assert.True(startup.TryRegisterRetirementDependency(() =>
        {
            invoked++;
            return Task.FromException(laterFailure);
        }));
        var prior = ContinuousLearningModelServerStartup
            .RetirementTaskPublishedObserverForTesting;
        try
        {
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = _ =>
                    throw observerFailure;

            startup.MarkDpoPolicyBaseStopped();

            Assert.Equal(3, invoked);
            var thrown = await Assert.ThrowsAsync<OutOfMemoryException>(
                async () => await startup.AwaitRetirementAsync()
                    .WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.Same(firstFatal, thrown);
            var attached = Assert.IsType<AggregateException>(
                thrown.Data["CONTINUOUS_LEARNING_RETIREMENT_FAILURE"]);
            Assert.Equal(
                [observerFailure, secondFatal, laterFailure],
                attached.InnerExceptions);
        }
        finally
        {
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = prior;
        }
    }

    [Fact]
    public async Task HostedTeacherAssociation_StopWaitsForDrainBeforeExactOnceRemoval()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var activator = new ContinuousLearningRetainedObjectHostedActivator(
            access, gateway, startup);
        await activator.ActivatePolicyBaseProviderAsync(
            CancellationToken.None, null, null);
        var hosted = new DesignConversionDpoTeacherAttestationHostedAssociation(
            startup, gateway);
        await hosted.StartAsync(CancellationToken.None);
        Assert.NotNull(DesignConversionDpoTeacherAttestationAssociation
            .ResolveExactProvider(gateway));
        var dependencyEntered = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var dependencyRelease = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var dependencyInvocations = 0;
        Assert.True(startup.TryRegisterRetirementDependency(() =>
        {
            Interlocked.Increment(ref dependencyInvocations);
            dependencyEntered.TrySetResult(null);
            return dependencyRelease.Task;
        }));
        Assert.False(startup.Readiness.TrainingEnabled);
        Assert.False(startup.Readiness.CycleExecutionAvailable);
        Assert.False(startup.Readiness.PerformsTraining);
        Assert.False(startup.IsLegalTrainingTeacherAdmissionActive());

        Task? stop = null;
        try
        {
            stop = hosted.StopAsync(CancellationToken.None);
            var repeatedStop = hosted.StopAsync(CancellationToken.None);
            Assert.Same(stop, repeatedStop);
            await dependencyEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(stop.IsCompleted);
            Assert.False(gateway.IsBound);
            AssertClosedAndUnmaterialized(startup.Readiness);
            Assert.Equal(1, Volatile.Read(ref dependencyInvocations));
            Assert.NotNull(DesignConversionDpoTeacherAttestationAssociation
                .ResolveExactProvider(gateway));
            dependencyRelease.TrySetResult(null);
            await stop.WaitAsync(TimeSpan.FromSeconds(5));
            await repeatedStop.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Null(DesignConversionDpoTeacherAttestationAssociation
                .ResolveExactProvider(gateway));
            var completedStop = hosted.StopAsync(CancellationToken.None);
            Assert.Same(stop, completedStop);
            await completedStop;
            Assert.Equal(1, Volatile.Read(ref dependencyInvocations));
            Assert.Null(DesignConversionDpoTeacherAttestationAssociation
                .ResolveExactProvider(gateway));
        }
        finally
        {
            dependencyRelease.TrySetResult(null);
            if (stop is not null)
                await stop.WaitAsync(TimeSpan.FromSeconds(5));
        }
    }

    [Fact]
    public async Task GatewayClose_PublishesPendingRetirementUntilHeldInvocationReleases()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var activator = new ContinuousLearningRetainedObjectHostedActivator(
            access, gateway, startup);
        await activator.ActivatePolicyBaseProviderAsync(
            CancellationToken.None, null, null);
        var entered = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var prior = DesignConversionRetainedDpoPolicyBaseProviderGateway
            .InvocationLeaseAcquiredObserverForTesting;
        try
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .InvocationLeaseAcquiredObserverForTesting = async () =>
                {
                    entered.TrySetResult(null);
                    await release.Task.ConfigureAwait(false);
                };
            var invocation = gateway.CreateModelSetAsync(CancellationToken.None);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));

            gateway.Close();
            var retirement = gateway.AwaitRetirementAsync();
            Assert.False(gateway.IsBound);
            Assert.False(retirement.IsCompleted);
            Assert.Same(retirement, gateway.AwaitRetirementAsync());
            var rejected = await gateway.CreateModelSetAsync(
                CancellationToken.None);
            Assert.False(rejected.IsSuccess);
            Assert.Equal(
                DesignConversionRetainedDpoPolicyBaseProviderGateway
                    .ProviderUnavailableErrorCode,
                rejected.ErrorCode);

            release.TrySetResult(null);
            await invocation.WaitAsync(TimeSpan.FromSeconds(5));
            await retirement.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            release.TrySetResult(null);
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .InvocationLeaseAcquiredObserverForTesting = prior;
        }
    }

    [Fact]
    public void LifecycleCleanupArbiter_NonFatalOriginalYieldsExactFatalCleanup()
    {
        var original = new IOException("nonfatal-original");
        var fatal = new OutOfMemoryException("fatal-cleanup");
        var later = new IOException("later-cleanup");
        var arbiter = new LifecycleCleanupArbiter();
        arbiter.CaptureOriginal(original);
        arbiter.CaptureCleanup(fatal);
        arbiter.CaptureCleanup(later);

        var thrown = Assert.Throws<OutOfMemoryException>(arbiter.ThrowIfAny);

        Assert.Same(fatal, thrown);
        var aggregate = Assert.IsType<AggregateException>(
            thrown.Data["CONTINUOUS_LEARNING_RETIREMENT_FAILURE"]);
        Assert.Equal([original, later], aggregate.InnerExceptions);
    }

    [Fact]
    public void LifecycleCleanupArbiter_PreservesFlattensAndDeduplicatesExistingDiagnostics()
    {
        const string key = "CONTINUOUS_LEARNING_RETIREMENT_FAILURE";
        var original = new IOException("original");
        var fatal = new OutOfMemoryException("fatal");
        var existing = new IOException("existing");
        var later = new IOException("later");
        existing.Data[key] = fatal;
        var nested = new AggregateException(existing, later, existing);
        nested.Data[key] = new AggregateException(existing, original);
        fatal.Data[key] = nested;
        var arbiter = new LifecycleCleanupArbiter();
        arbiter.CaptureOriginal(original);
        arbiter.CaptureCleanup(fatal);
        arbiter.CaptureCleanup(later);

        var thrown = Assert.Throws<OutOfMemoryException>(arbiter.ThrowIfAny);

        Assert.Same(fatal, thrown);
        var attached = Assert.IsType<AggregateException>(thrown.Data[key]);
        Assert.Equal([existing, later, original], attached.InnerExceptions);
    }

    [Fact]
    public async Task HostedCancellationAfterGatewayBindWithdrawsExactBindingBeforeReadinessPromotion()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var activator = new ContinuousLearningRetainedObjectHostedActivator(
            access,
            gateway,
            startup);
        using var cancellation = new CancellationTokenSource();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            activator.ActivatePolicyBaseProviderAsync(
                cancellation.Token,
                afterProviderCreationBeforeBindingForTesting: null,
                afterGatewayBindingBeforeReadinessPromotionForTesting:
                    () =>
                    {
                        Assert.True(gateway.IsBound);
                        Assert.False(
                            startup.Readiness.StableDpoExecutorConfigured);
                        cancellation.Cancel();
                    }));

        Assert.False(gateway.IsBound);
        Assert.Equal(
            ContinuousLearningModelServerStartup
                .DpoPolicyBaseBindingFailedErrorCode,
            startup.Readiness.ConfigurationErrorCode);
        Assert.Equal(
            "dpo_policy_base_binding_failed_inactive",
            startup.Readiness.ConfigurationStatus);
        AssertClosedAndUnmaterialized(startup.Readiness);
        Assert.Null(
            DesignConversionRetainedDpoPolicyBaseActivation
                .ResolveLegalTeacherAdmissionAuthority(gateway));
        Assert.Null(
            DesignConversionDpoTeacherAttestationAssociation
                .ResolveExactProvider(gateway));
    }

    [Fact]
    public async Task RetainedDpoPolicyBinding_NullCapabilityReturnsSanitizedBindFailure()
    {
        using var fixture = new ModelServerCompositionFixture();
        var before = fixture.Snapshot();

        var result = await DesignConversionRetainedDpoPolicyBaseComposition
            .CreateInactiveProviderAsync(
                fileAccess: null,
                fixture.ModelConfiguration,
                RetainedPolicyModelId,
                RetainedPolicyRevision);

        AssertFailure(
            result,
            ContinuousLearningRetainedDpoPolicyCheckpointSource.BindFailedErrorCode,
            ContinuousLearningRetainedDpoPolicyCheckpointSource.BindFailedErrorMessage);
        Assert.DoesNotContain(fixture.Root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoOutputDirectories();
    }

    [Fact]
    public async Task RetainedDpoPolicyBinding_ConfigIncompatibleTokenizerFailsBeforeProviderReturn()
    {
        using var fixture = new ModelServerCompositionFixture();
        var incompatibleTokenizer = new BpeTokenizer();
        incompatibleTokenizer.Train(
            "retained tokenizer config mismatch fixture",
            fixture.ModelConfiguration.VocabSize + 1);
        incompatibleTokenizer.Save(fixture.TokenizerPath);
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();

        var result = await DesignConversionRetainedDpoPolicyBaseComposition
            .CreateInactiveProviderAsync(
                access,
                fixture.ModelConfiguration,
                RetainedPolicyModelId,
                RetainedPolicyRevision);

        AssertFailure(
            result,
            DesignConversionRetainedDpoPolicyBaseComposition
                .RetainedTokenizerInvalidErrorCode,
            DesignConversionRetainedDpoPolicyBaseComposition
                .RetainedTokenizerInvalidErrorMessage);
        Assert.DoesNotContain(
            fixture.Root,
            result.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RetainedDpoPolicyProvider_LaterCreatesThreeIndependentEquivalentModels()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        var bound = await DesignConversionRetainedDpoPolicyBaseComposition
            .CreateInactiveProviderAsync(
                access,
                fixture.ModelConfiguration,
                RetainedPolicyModelId,
                RetainedPolicyRevision);
        var provider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(bound.Value);

        var created = await provider.CreateModelSetAsync(CancellationToken.None);

        Assert.True(created.IsSuccess, created.ErrorMessage);
        var models = Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(created.Value);
        Assert.Equal(HashFile(fixture.BaseCheckpointPath), models.Identity.BaseCheckpointSha256);
        AssertModelsPhysicallyIndependentAndBitwiseEqual(models.Policy, models.Reference);
        AssertModelsPhysicallyIndependentAndBitwiseEqual(
            models.Policy,
            models.FreshLoadTarget);
        AssertModelsPhysicallyIndependentAndBitwiseEqual(
            models.Reference,
            models.FreshLoadTarget);
        await AssertRetainedCanonicalBpeTokenizerReferenceAsync(
            models,
            fixture.TokenizerPath,
            fixture.ModelConfiguration,
            fixture.Root);
        AssertClosedAndUnmaterialized(startup.Readiness);
    }

    [Fact]
    public async Task RetainedDpoPolicyTokenizerReference_ReplacementAfterCaptureIsNotOpenedAsFallback()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        var access = startup.AcquireRetainedFileAccess();
        var bound = await DesignConversionRetainedDpoPolicyBaseComposition
            .CreateInactiveProviderAsync(
                access,
                fixture.ModelConfiguration,
                RetainedPolicyModelId,
                RetainedPolicyRevision);
        var provider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(bound.Value);
        var created = await provider.CreateModelSetAsync(CancellationToken.None);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        var modelSet = Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(
            created.Value);
        var reference = Assert.IsType<
            DesignConversionDpoCanonicalBpeTokenizerSnapshotReference>(
                modelSet.CanonicalBpeTokenizerSnapshot);

        access.Dispose();
        var replacementTokenizer = new BpeTokenizer();
        replacementTokenizer.Train(
            "replacement tokenizer must not be opened by retained reference",
            fixture.ModelConfiguration.VocabSize);
        replacementTokenizer.Save(fixture.TokenizerPath);

        var reopened = await reference.OpenSnapshotAsync(CancellationToken.None);

        AssertFailure(
            reopened,
            DesignConversionDpoCanonicalBpeTokenizerSnapshotReference
                .SourceFailedErrorCode,
            "The DesignConversion DPO canonical BPE tokenizer snapshot source failed.");
        Assert.DoesNotContain(
            fixture.Root,
            reopened.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RetainedDpoPolicySource_SequentialAndConcurrentOpensReturnDistinctZeroPositionStreams()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        var sequentialSource =
            new ContinuousLearningRetainedDpoPolicyCheckpointSource(access);
        using var bothHooksEntered = new ManualResetEventSlim(false);
        using var releaseHooks = new ManualResetEventSlim(false);
        var hookInvocationCount = 0;
        var activeHookCount = 0;
        var maximumHookCount = 0;
        var concurrentSource =
            new ContinuousLearningRetainedDpoPolicyCheckpointSource(
                access,
                _ =>
                {
                    var invocation = Interlocked.Increment(
                        ref hookInvocationCount);
                    var active = Interlocked.Increment(ref activeHookCount);
                    RecordMaximum(ref maximumHookCount, active);
                    try
                    {
                        if (invocation == 2)
                            bothHooksEntered.Set();
                        if (!releaseHooks.Wait(TimeSpan.FromSeconds(10)))
                        {
                            throw new TimeoutException(
                                "Concurrent retained checkpoint hooks did not receive release.");
                        }
                    }
                    finally
                    {
                        Interlocked.Decrement(ref activeHookCount);
                    }
                });
        var ownedStreams = new List<Stream>();
        try
        {
            var first = await sequentialSource.OpenCheckpointSnapshotAsync(
                CancellationToken.None);
            if (first.Value is not null)
                ownedStreams.Add(first.Value);
            var second = await sequentialSource.OpenCheckpointSnapshotAsync(
                CancellationToken.None);
            if (second.Value is not null)
                ownedStreams.Add(second.Value);
            var firstConcurrent = Task.Run(() =>
                concurrentSource.OpenCheckpointSnapshotAsync(
                    CancellationToken.None));
            var secondConcurrent = Task.Run(() =>
                concurrentSource.OpenCheckpointSnapshotAsync(
                    CancellationToken.None));
            var overlapped = bothHooksEntered.Wait(TimeSpan.FromSeconds(5));
            var activeAtRelease = Volatile.Read(ref activeHookCount);
            releaseHooks.Set();
            var concurrent = await Task.WhenAll(
                firstConcurrent,
                secondConcurrent);
            var results = new[] { first, second, concurrent[0], concurrent[1] };
            ownedStreams.AddRange(concurrent
                .Where(result => result.Value is not null)
                .Select(result => result.Value!));
            Assert.True(overlapped, "Both retained checkpoint hooks must overlap.");
            Assert.Equal(2, hookInvocationCount);
            Assert.Equal(2, maximumHookCount);
            Assert.Equal(2, activeAtRelease);
            Assert.Equal(0, activeHookCount);
            Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));

            Assert.Equal(
                ownedStreams.Count,
                ownedStreams.Distinct(ReferenceEqualityComparer.Instance).Count());
            Assert.All(ownedStreams, stream =>
            {
                Assert.True(stream.CanRead);
                Assert.True(stream.CanSeek);
                Assert.Equal(0, stream.Position);
            });
        }
        finally
        {
            releaseHooks.Set();
            foreach (var stream in ownedStreams)
                stream.Dispose();
        }
    }

    [Fact]
    public async Task RetainedDpoPolicyBinding_PreCancelledTokenWinsOverDisposedCapability()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        var access = startup.AcquireRetainedFileAccess();
        access.Dispose();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await DesignConversionRetainedDpoPolicyBaseComposition
            .CreateInactiveProviderAsync(
                access,
                fixture.ModelConfiguration,
                RetainedPolicyModelId,
                RetainedPolicyRevision,
                cancellation.Token);

        AssertFailure(
            result,
            ContinuousLearningRetainedDpoPolicyCheckpointSource.CancelledErrorCode,
            ContinuousLearningRetainedDpoPolicyCheckpointSource.CancelledErrorMessage);
        Assert.DoesNotContain(fixture.Root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RetainedDpoPolicyBinding_DisposedCapabilityReturnsExactTrustFailure()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        var access = startup.AcquireRetainedFileAccess();
        access.Dispose();

        var result = await DesignConversionRetainedDpoPolicyBaseComposition
            .CreateInactiveProviderAsync(
                access,
                fixture.ModelConfiguration,
                RetainedPolicyModelId,
                RetainedPolicyRevision);

        AssertFailure(
            result,
            ContinuousLearningPathIdentityException.StableErrorCode,
            ContinuousLearningRetainedDpoPolicyCheckpointSource.PathIdentityErrorMessage);
        Assert.DoesNotContain(fixture.Root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RetainedDpoPolicySource_DisposedCapabilityReturnsExactTrustFailure()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        var access = startup.AcquireRetainedFileAccess();
        var source = new ContinuousLearningRetainedDpoPolicyCheckpointSource(access);
        access.Dispose();

        var result = await source.OpenCheckpointSnapshotAsync(CancellationToken.None);

        AssertFailure(
            result,
            ContinuousLearningPathIdentityException.StableErrorCode,
            ContinuousLearningRetainedDpoPolicyCheckpointSource.PathIdentityErrorMessage);
        Assert.DoesNotContain(fixture.Root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RetainedDpoPolicySource_TrustLossAfterOpenDisposesCapturedStream()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        var access = startup.AcquireRetainedFileAccess();
        Stream? captured = null;
        var source = new ContinuousLearningRetainedDpoPolicyCheckpointSource(
            access,
            stream =>
            {
                captured = stream;
                access.Dispose();
            });

        var result = await source.OpenCheckpointSnapshotAsync(CancellationToken.None);

        AssertFailure(
            result,
            ContinuousLearningPathIdentityException.StableErrorCode,
            ContinuousLearningRetainedDpoPolicyCheckpointSource.PathIdentityErrorMessage);
        Assert.NotNull(captured);
        Assert.Throws<ObjectDisposedException>(() => captured!.ReadByte());
    }

    [Fact]
    public async Task RetainedDpoPolicySource_NonFatalHookFailureIsSanitizedAndDisposesStream()
    {
        const string privateFailure = "C:/private/checkpoints/RETAINED-HOOK-FAILURE";
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        Stream? captured = null;
        var source = new ContinuousLearningRetainedDpoPolicyCheckpointSource(
            access,
            stream =>
            {
                captured = stream;
                throw new IOException(privateFailure);
            });

        var result = await source.OpenCheckpointSnapshotAsync(CancellationToken.None);

        AssertFailure(
            result,
            ContinuousLearningRetainedDpoPolicyCheckpointSource.BindFailedErrorCode,
            ContinuousLearningRetainedDpoPolicyCheckpointSource.BindFailedErrorMessage);
        Assert.DoesNotContain(privateFailure, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(fixture.Root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(captured);
        Assert.Throws<ObjectDisposedException>(() => captured!.ReadByte());
    }

    [Fact]
    public async Task RetainedDpoPolicySource_HookCancellationReturnsCancelledAndDisposesStream()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        using var cancellation = new CancellationTokenSource();
        Stream? captured = null;
        var hookInvocationCount = 0;
        var source = new ContinuousLearningRetainedDpoPolicyCheckpointSource(
            access,
            stream =>
            {
                captured = stream;
                Interlocked.Increment(ref hookInvocationCount);
                cancellation.Cancel();
            });

        var result = await source.OpenCheckpointSnapshotAsync(
            cancellation.Token);

        AssertFailure(
            result,
            ContinuousLearningRetainedDpoPolicyCheckpointSource.CancelledErrorCode,
            ContinuousLearningRetainedDpoPolicyCheckpointSource.CancelledErrorMessage);
        Assert.Equal(1, hookInvocationCount);
        Assert.NotNull(captured);
        Assert.Throws<ObjectDisposedException>(() => captured!.ReadByte());
        Assert.DoesNotContain(fixture.Root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RetainedDpoPolicyTokenizerSource_NonFatalHookFailureIsSanitizedAndDisposesStream()
    {
        const string privateFailure = "C:/private/tokenizer/RETAINED-HOOK-FAILURE";
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        Stream? captured = null;
        var source =
            new ContinuousLearningRetainedDpoPolicyCanonicalBpeTokenizerSnapshotSource(
                access,
                stream =>
                {
                    captured = stream;
                    throw new IOException(privateFailure);
                });

        var result = await source.OpenCanonicalBpeTokenizerSnapshotAsync(
            CancellationToken.None);

        AssertFailure(
            result,
            ContinuousLearningRetainedDpoPolicyCanonicalBpeTokenizerSnapshotSource
                .BindFailedErrorCode,
            ContinuousLearningRetainedDpoPolicyCanonicalBpeTokenizerSnapshotSource
                .BindFailedErrorMessage);
        Assert.DoesNotContain(privateFailure, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(fixture.Root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(captured);
        Assert.Throws<ObjectDisposedException>(() => captured!.ReadByte());
    }

    [Fact]
    public async Task RetainedDpoPolicyTokenizerSource_HookCancellationReturnsCancelledAndDisposesStream()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        using var cancellation = new CancellationTokenSource();
        Stream? captured = null;
        var hookInvocationCount = 0;
        var source =
            new ContinuousLearningRetainedDpoPolicyCanonicalBpeTokenizerSnapshotSource(
                access,
                stream =>
                {
                    captured = stream;
                    Interlocked.Increment(ref hookInvocationCount);
                    cancellation.Cancel();
                });

        var result = await source.OpenCanonicalBpeTokenizerSnapshotAsync(
            cancellation.Token);

        AssertFailure(
            result,
            ContinuousLearningRetainedDpoPolicyCheckpointSource.CancelledErrorCode,
            ContinuousLearningRetainedDpoPolicyCheckpointSource.CancelledErrorMessage);
        Assert.Equal(1, hookInvocationCount);
        Assert.NotNull(captured);
        Assert.Throws<ObjectDisposedException>(() => captured!.ReadByte());
        Assert.DoesNotContain(fixture.Root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task RetainedDpoPolicyBinding_InvalidMetadataWinsBeforeDisposedCapability(
        int invalidField)
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        var access = startup.AcquireRetainedFileAccess();
        access.Dispose();
        var config = invalidField == 0
            ? fixture.ModelConfiguration with { VocabSize = 1 }
            : fixture.ModelConfiguration;
        var modelId = invalidField == 1
            ? "C:/private/checkpoints/model"
            : RetainedPolicyModelId;
        var revision = invalidField == 2
            ? "invalid/revision"
            : RetainedPolicyRevision;

        var result = await DesignConversionRetainedDpoPolicyBaseComposition
            .CreateInactiveProviderAsync(
                access,
                config,
                modelId,
                revision);

        AssertFailure(
            result,
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .ConfigurationInvalidErrorCode,
            "DesignConversion DPO checkpoint-backed policy base configuration is invalid.");
        Assert.DoesNotContain(fixture.Root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RetainedDpoPolicyProvider_AfterCapabilityDisposalReturnsGenericSourceFailureWithoutCacheUse()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        var access = startup.AcquireRetainedFileAccess();
        var bound = await DesignConversionRetainedDpoPolicyBaseComposition
            .CreateInactiveProviderAsync(
                access,
                fixture.ModelConfiguration,
                RetainedPolicyModelId,
                RetainedPolicyRevision);
        var provider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(bound.Value);
        access.Dispose();

        var result = await provider.CreateModelSetAsync(CancellationToken.None);

        AssertFailure(
            result,
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .CanonicalBpeTokenizerSourceFailedErrorCode,
            "The DesignConversion DPO canonical BPE tokenizer snapshot source failed.");
        Assert.NotEqual(
            ContinuousLearningPathIdentityException.StableErrorCode,
            result.ErrorCode);
        Assert.DoesNotContain(fixture.Root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.CacheRoot));
    }

    [Fact]
    public async Task RetainedDpoPolicySource_TransferredStreamRemainsCallerOwnedAfterCapabilityDisposal()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        var access = startup.AcquireRetainedFileAccess();
        var source = new ContinuousLearningRetainedDpoPolicyCheckpointSource(access);
        var opened = await source.OpenCheckpointSnapshotAsync(CancellationToken.None);
        using var stream = Assert.IsAssignableFrom<Stream>(opened.Value);

        access.Dispose();

        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        Assert.True(stream.CanRead);
        Assert.Equal(0, stream.Position);
        Assert.NotEqual(-1, stream.ReadByte());
    }

    [Fact]
    public async Task RetainedDpoPolicyBinding_NonemptyMalformedCheckpointBindsButLaterModelLoadRejectsIt()
    {
        using var fixture = new ModelServerCompositionFixture();
        File.WriteAllBytes(fixture.BaseCheckpointPath, [0x01, 0x02, 0x03]);
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();

        var bound = await DesignConversionRetainedDpoPolicyBaseComposition
            .CreateInactiveProviderAsync(
                access,
                fixture.ModelConfiguration,
                RetainedPolicyModelId,
                RetainedPolicyRevision);
        Assert.True(bound.IsSuccess, bound.ErrorMessage);
        var provider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(bound.Value);

        var loaded = await provider.CreateModelSetAsync(CancellationToken.None);

        AssertFailure(
            loaded,
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .CheckpointInvalidErrorCode,
            "The DesignConversion DPO policy base checkpoint is invalid.");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RetainedDpoPolicyCheckpoint_EmptyOrOversizedArtifactIsRejectedAtCapabilityAcquisition(
        bool oversized)
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        using (var checkpoint = new FileStream(
                   fixture.BaseCheckpointPath,
                   FileMode.Open,
                   FileAccess.Write,
                   FileShare.Read))
        {
            checkpoint.SetLength(oversized
                ? ContinuousLearningModelServerConfigurationLoader
                    .MaximumBaseCheckpointBytes + 1
                : 0);
        }

        var exception = Assert.Throws<ContinuousLearningPathIdentityException>(() =>
            ServerOwnedContinuousLearningFileAccess.Acquire(
                fixture.BaseCheckpointPath,
                fixture.TokenizerPath,
                fixture.AdapterRoot,
                fixture.CacheRoot,
                fixture.RegistryRoot,
                fixture.PairRoot,
                options: null,
                onRetainedObjectTrustLost: static () => { }));

        Assert.Equal(
            ContinuousLearningPathIdentityException.StableErrorCode,
            exception.Message);
    }

    [Fact]
    public void RetainedDpoPolicyComposition_IsInternalAndProgramActivationUsesOnlyTheHostedGateway()
    {
        var sourceType = typeof(ContinuousLearningRetainedDpoPolicyCheckpointSource);
        var tokenizerSourceType = typeof(
            ContinuousLearningRetainedDpoPolicyCanonicalBpeTokenizerSnapshotSource);
        var compositionType = typeof(DesignConversionRetainedDpoPolicyBaseComposition);
        Assert.True(sourceType.IsNotPublic);
        Assert.True(sourceType.IsSealed);
        Assert.True(tokenizerSourceType.IsNotPublic);
        Assert.True(tokenizerSourceType.IsSealed);
        Assert.True(compositionType.IsNotPublic);
        Assert.True(compositionType.IsAbstract);
        Assert.True(compositionType.IsSealed);
        var create = RequireExactMethod(
            compositionType,
            nameof(DesignConversionRetainedDpoPolicyBaseComposition
                .CreateInactiveProviderAsync),
            BindingFlags.Static | BindingFlags.NonPublic,
            typeof(ServerOwnedContinuousLearningFileAccess),
            typeof(ModelConfig),
            typeof(string),
            typeof(string),
            typeof(CancellationToken));
        var seededCreate = RequireExactMethod(
            compositionType,
            nameof(DesignConversionRetainedDpoPolicyBaseComposition
                .CreateInactiveProviderAsync),
            BindingFlags.Static | BindingFlags.NonPublic,
            typeof(ServerOwnedContinuousLearningFileAccess),
            typeof(ModelConfig),
            typeof(string),
            typeof(string),
            typeof(ulong?),
            typeof(string),
            typeof(CancellationToken));
        Assert.False(create.IsPublic);
        Assert.False(seededCreate.IsPublic);

        var programMethods = typeof(ModelServer.Program)
            .GetMethods(
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly)
            .Where(method => method.GetMethodBody() is not null);
        Assert.DoesNotContain(
            programMethods,
            method => ContainsCallTo(method, create) ||
                      ContainsCallTo(method, seededCreate));
        Assert.DoesNotContain(
            typeof(ModelServer.Program).GetFields(
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic),
            field => field.FieldType == sourceType || field.FieldType == compositionType);

        var gatewayType = typeof(
            DesignConversionRetainedDpoPolicyBaseProviderGateway);
        Assert.True(gatewayType.IsNotPublic);
        Assert.True(gatewayType.IsSealed);
        Assert.True(typeof(IDesignConversionDpoPolicyContinuationProvider)
            .IsAssignableFrom(gatewayType));
        var source = File.ReadAllText(
            Path.Combine(
                FindRepositoryRoot(),
                "25.ModelServer",
                "Program.cs"));
        Assert.Contains(
            "DesignConversionRetainedDpoPolicyBaseComposition",
            source,
            StringComparison.Ordinal);
        Assert.Contains("BindExactProvider", source, StringComparison.Ordinal);
        Assert.DoesNotContain(".CreateModelSetAsync(", source, StringComparison.Ordinal);
        Assert.DoesNotContain(".CreateContinuedModelSetAsync(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("StepVector", source, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "dpoPolicyBaseProvider: null).Run()",
            source,
            StringComparison.Ordinal);
    }

    [Fact]
    public void LegalTeacherCapability_ProgramAspNetAndLiveSourcesPropagateOnlyTheExactProviderAssociation()
    {
        var repositoryRoot = FindRepositoryRoot();
        var programSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "25.ModelServer",
            "Program.cs"));
        var aspNetSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "25.ModelServer",
            "DesignConversion",
            "DesignConversionModelServerAspNetHostStartup.cs"));
        var activationSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "25.ModelServer",
            "DesignConversion",
            "DesignConversionRetainedDpoPolicyBaseActivation.cs"));
        var liveSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "Program.cs"));

        Assert.Contains(
            "builder.Services.AddSingleton(\n            continuousLearning.LegalTeacherAdmissionAuthority);",
            programSource.Replace("\r\n", "\n", StringComparison.Ordinal),
            StringComparison.Ordinal);
        Assert.Contains(
            "ResolveServerOwnedLegalTeacherAdmissionAuthority(",
            programSource,
            StringComparison.Ordinal);
        Assert.Contains(
            "ResolveLegalTeacherAdmissionAuthority(\n                    promotionHostOptions.DpoPolicyBaseProvider)",
            aspNetSource.Replace("\r\n", "\n", StringComparison.Ordinal),
            StringComparison.Ordinal);
        Assert.Contains(
            "ownedRetainedDpoPolicyBaseLease?.Provider",
            liveSource,
            StringComparison.Ordinal);
        Assert.Contains(
            "ownedRetainedDpoPolicyBaseLease?.LegalTeacherAdmissionAuthority",
            liveSource,
            StringComparison.Ordinal);
        Assert.Contains(
            "ConditionalWeakTable<",
            activationSource,
            StringComparison.Ordinal);
        var teacherRuntimeFactorySource = ExtractMethodSource(
            activationSource,
            "internal static DesignConversionDpoTeacherAttestationRuntime");
        Assert.Contains(
            NormalizeSource(
                "() => startup.IsLegalTrainingTeacherAdmissionActive() && gateway.IsBound"),
            NormalizeSource(teacherRuntimeFactorySource),
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            NormalizeSource(
                "startup.Readiness.StableDpoExecutorConfigured"),
            NormalizeSource(teacherRuntimeFactorySource),
            StringComparison.Ordinal);
        Assert.Contains(
            "_legalTeacherAdmissionAssociation.Dispose()",
            activationSource,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "IssueFromAdmittedConfiguration",
            string.Join("\n", programSource, aspNetSource, activationSource, liveSource),
            StringComparison.Ordinal);

        var registryField = typeof(DesignConversionRetainedDpoPolicyBaseActivation)
            .GetField(
                "LegalTeacherAdmissionAuthorities",
                BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(registryField);
        Assert.True(registryField!.IsInitOnly);
        Assert.Equal(
            typeof(System.Runtime.CompilerServices.ConditionalWeakTable<,>),
            registryField.FieldType.GetGenericTypeDefinition());
        var registrationType = registryField.FieldType.GenericTypeArguments[1];
        Assert.DoesNotContain(
            registrationType.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
            field => typeof(IDesignConversionDpoPolicyBaseProvider)
                .IsAssignableFrom(field.FieldType));
    }

    [Fact]
    public async Task RetainedDpoPolicyActivation_DisabledIsSuccessfulWithoutLease()
    {
        var reads = new List<string>();

        var resolution = await DesignConversionRetainedDpoPolicyBaseActivation
            .ActivateAsync(name =>
            {
                reads.Add(name);
                if (name == ContinuousLearningModelServerConfigurationLoader
                        .EnabledEnvironmentName)
                {
                    return bool.FalseString;
                }
                if (name == ContinuousLearningModelServerConfigurationLoader
                        .TrainingActivationEnabledEnvironmentName)
                {
                    return bool.FalseString;
                }
                if (name == ContinuousLearningModelServerConfigurationLoader
                        .LocalIndependentPreferenceEvaluatorEnabledEnvironmentName)
                {
                    return bool.FalseString;
                }

                throw new InvalidOperationException(
                    "Disabled activation read beyond its control switches.");
            });

        Assert.True(resolution.IsSuccess);
        Assert.False(resolution.IsActivated);
        Assert.Equal(
            DesignConversionRetainedDpoPolicyBaseActivation.NotConfiguredStatus,
            resolution.Status);
        Assert.Equal(
            DesignConversionRetainedDpoPolicyBaseActivation.NoErrorCode,
            resolution.ErrorCode);
        Assert.Null(resolution.Lease);
        Assert.Null(resolution.LegalTeacherAdmissionAuthority);
        Assert.Equal(
            [
                ContinuousLearningModelServerConfigurationLoader.EnabledEnvironmentName,
                ContinuousLearningModelServerConfigurationLoader
                    .TrainingActivationEnabledEnvironmentName,
                ContinuousLearningModelServerConfigurationLoader
                    .LocalIndependentPreferenceEvaluatorEnabledEnvironmentName
            ],
            reads);
    }

    [Fact]
    public async Task RetainedDpoPolicyActivation_ValidReturnsExactContinuationGatewayWithoutModelExecution()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var environment = fixture.ValidEnvironment();
        Assert.Null(
            DesignConversionRetainedDpoPolicyBaseActivation
                .ResolveLegalTeacherAdmissionAuthority(
                    new DesignConversionDpoPolicyBaseTestProvider()));

        var resolution = await DesignConversionRetainedDpoPolicyBaseActivation
            .ActivateAsync(name =>
                environment.TryGetValue(name, out var value) ? value : null);

        Assert.True(resolution.IsSuccess);
        Assert.True(resolution.IsActivated);
        Assert.Equal(
            DesignConversionRetainedDpoPolicyBaseActivation.ActivatedStatus,
            resolution.Status);
        var lease = Assert.IsType<
            DesignConversionRetainedDpoPolicyBaseActivationLease>(
                resolution.Lease);
        await using (lease)
        {
            var gateway = Assert.IsType<
                DesignConversionRetainedDpoPolicyBaseProviderGateway>(
                    lease.Provider);
            Assert.IsAssignableFrom<IDesignConversionDpoPolicyContinuationProvider>(
                lease.Provider);
            Assert.Same(
                lease.LegalTeacherAdmissionAuthority,
                resolution.LegalTeacherAdmissionAuthority);
            Assert.Same(
                lease.LegalTeacherAdmissionAuthority,
                DesignConversionRetainedDpoPolicyBaseActivation
                    .ResolveLegalTeacherAdmissionAuthority(lease.Provider));
            Assert.NotNull(
                DesignConversionDpoTeacherAttestationAssociation
                    .ResolveExactProvider(lease.Provider));
            Assert.True(gateway.IsBound);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority
                    .ConfigurationRequiredErrorCode,
                lease.LegalTeacherAdmissionAuthority.ValidateBatch(
                    [TrainingTeacherProvenance.Deterministic(
                        "standalone-default-off")]).ErrorCode);
        }
        Assert.Null(
            DesignConversionRetainedDpoPolicyBaseActivation
                .ResolveLegalTeacherAdmissionAuthority(lease.Provider));
        Assert.Null(
            DesignConversionDpoTeacherAttestationAssociation
                .ResolveExactProvider(lease.Provider));
        lease.Dispose();
        Assert.Null(
            DesignConversionDpoTeacherAttestationAssociation
                .ResolveExactProvider(lease.Provider));

        var publicActivation = RequireExactMethod(
            typeof(DesignConversionRetainedDpoPolicyBaseActivation),
            nameof(DesignConversionRetainedDpoPolicyBaseActivation.ActivateAsync),
            BindingFlags.Static | BindingFlags.Public,
            typeof(CancellationToken));
        Assert.Equal(
            typeof(Task<DesignConversionRetainedDpoPolicyBaseActivationResolution>),
            publicActivation.ReturnType);
        var activationOverload = RequireExactMethod(
            typeof(DesignConversionRetainedDpoPolicyBaseActivation),
            nameof(DesignConversionRetainedDpoPolicyBaseActivation.ActivateAsync),
            BindingFlags.Static | BindingFlags.NonPublic,
            typeof(Func<string, string?>),
            typeof(CancellationToken),
            typeof(DesignConversionRetainedDpoPolicyBaseActivationTestHooks));
        Assert.True(ContainsCallTo(publicActivation, activationOverload));
        AssertRetainedActivationHasExactInactiveCallChainWithoutExecution(
            publicActivation,
            activationOverload);
    }

    [Fact]
    public async Task RetainedDpoPolicyActivation_TwoLeasesKeepExactIndependentAssociationsThroughRepeatedDisposal()
    {
        using var firstFixture = new ModelServerCompositionFixture();
        using var secondFixture = new ModelServerCompositionFixture();
        firstFixture.CreateOutputDirectories();
        secondFixture.CreateOutputDirectories();
        var firstEnvironment = firstFixture.ValidEnvironment();
        var secondEnvironment = secondFixture.ValidEnvironment();

        var resolutions = await Task.WhenAll(
            DesignConversionRetainedDpoPolicyBaseActivation.ActivateAsync(
                name => firstEnvironment.TryGetValue(name, out var value)
                    ? value
                    : null),
            DesignConversionRetainedDpoPolicyBaseActivation.ActivateAsync(
                name => secondEnvironment.TryGetValue(name, out var value)
                    ? value
                    : null));
        var firstLease = Assert.IsType<
            DesignConversionRetainedDpoPolicyBaseActivationLease>(
                resolutions[0].Lease);
        var secondLease = Assert.IsType<
            DesignConversionRetainedDpoPolicyBaseActivationLease>(
                resolutions[1].Lease);

        try
        {
            Assert.NotSame(firstLease.Provider, secondLease.Provider);
            Assert.NotSame(
                firstLease.LegalTeacherAdmissionAuthority,
                secondLease.LegalTeacherAdmissionAuthority);
            Assert.Same(
                firstLease.LegalTeacherAdmissionAuthority,
                DesignConversionRetainedDpoPolicyBaseActivation
                    .ResolveLegalTeacherAdmissionAuthority(firstLease.Provider));
            Assert.Same(
                secondLease.LegalTeacherAdmissionAuthority,
                DesignConversionRetainedDpoPolicyBaseActivation
                    .ResolveLegalTeacherAdmissionAuthority(secondLease.Provider));
            Assert.NotNull(
                DesignConversionDpoTeacherAttestationAssociation
                    .ResolveExactProvider(firstLease.Provider));
            Assert.NotNull(
                DesignConversionDpoTeacherAttestationAssociation
                    .ResolveExactProvider(secondLease.Provider));

            await firstLease.DisposeAsync();
            firstLease.Dispose();
            Assert.Null(
                DesignConversionRetainedDpoPolicyBaseActivation
                    .ResolveLegalTeacherAdmissionAuthority(firstLease.Provider));
            Assert.Null(
                DesignConversionDpoTeacherAttestationAssociation
                    .ResolveExactProvider(firstLease.Provider));
            Assert.Same(
                secondLease.LegalTeacherAdmissionAuthority,
                DesignConversionRetainedDpoPolicyBaseActivation
                    .ResolveLegalTeacherAdmissionAuthority(secondLease.Provider));
            Assert.NotNull(
                DesignConversionDpoTeacherAttestationAssociation
                    .ResolveExactProvider(secondLease.Provider));

            await secondLease.DisposeAsync();
            secondLease.Dispose();
            Assert.Null(
                DesignConversionRetainedDpoPolicyBaseActivation
                    .ResolveLegalTeacherAdmissionAuthority(secondLease.Provider));
            Assert.Null(
                DesignConversionDpoTeacherAttestationAssociation
                    .ResolveExactProvider(secondLease.Provider));
        }
        finally
        {
            await firstLease.DisposeAsync();
            await secondLease.DisposeAsync();
        }

        AssertRetainedPathsReleasedAndDeleteRoot(firstFixture.Root);
        AssertRetainedPathsReleasedAndDeleteRoot(secondFixture.Root);
    }

    [Fact]
    public async Task RetainedDpoPolicyActivation_InvalidReturnsSanitizedStableFailure()
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        environment.Remove(
            ContinuousLearningModelServerConfigurationLoader
                .AuditSaltEnvironmentName);

        var resolution = await DesignConversionRetainedDpoPolicyBaseActivation
            .ActivateAsync(name =>
                environment.TryGetValue(name, out var value) ? value : null);

        Assert.False(resolution.IsSuccess);
        Assert.False(resolution.IsActivated);
        Assert.Equal(
            DesignConversionRetainedDpoPolicyBaseActivation.FailedStatus,
            resolution.Status);
        Assert.Equal(
            "CONTINUOUS_LEARNING_CONFIGURATION_INVALID",
            resolution.ErrorCode);
        Assert.Equal(
            DesignConversionRetainedDpoPolicyBaseActivation
                .ActivationFailedErrorMessage,
            resolution.ErrorMessage);
        Assert.Null(resolution.Lease);
        Assert.DoesNotContain(
            fixture.Root,
            resolution.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RetainedDpoPolicyActivation_CorruptCheckpointFailsSanitizedAndReleasesPaths()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        File.WriteAllBytes(fixture.BaseCheckpointPath, [1, 2, 3, 4]);
        var environment = fixture.ValidEnvironment();

        var resolution = await DesignConversionRetainedDpoPolicyBaseActivation
            .ActivateAsync(name =>
                environment.TryGetValue(name, out var value) ? value : null);

        AssertSanitizedActivationFailure(
            resolution,
            ContinuousLearningModelServerStartup.MaterializationFailedErrorCode);
        Assert.DoesNotContain(
            fixture.Root,
            resolution.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
        AssertRetainedPathsReleasedAndDeleteRoot(fixture.Root);
    }

    [Fact]
    public async Task RetainedDpoPolicyActivation_UnsupportedAcquisitionFailsSanitizedAndReleasesPaths()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var environment = fixture.ValidEnvironment();
        DesignConversionRetainedDpoPolicyBaseProviderGateway? gateway = null;

        var resolution = await DesignConversionRetainedDpoPolicyBaseActivation
            .ActivateAsync(
                name => environment.TryGetValue(name, out var value)
                    ? value
                    : null,
                testHooks: new()
                {
                    FileAccessOptions = new ServerOwnedContinuousLearningFileAccessOptions
                    {
                        PlatformForTesting =
                            ServerOwnedContinuousLearningFileAccessPlatform.Unsupported,
                    },
                    GatewayCreated = createdGateway => gateway = createdGateway,
                });

        AssertSanitizedActivationFailure(resolution);
        Assert.Null(gateway);
        Assert.DoesNotContain(
            fixture.Root,
            resolution.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
        AssertRetainedPathsReleasedAndDeleteRoot(fixture.Root);
    }

    [Fact]
    public async Task RetainedDpoPolicyActivation_AcquisitionFailureAwaitsRetirementAndFatalRetirementDominates()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var environment = fixture.ValidEnvironment();
        var fatalRetirement = new OutOfMemoryException(
            "fatal-acquisition-retirement");
        Task? publishedRetirement = null;
        var prior = ContinuousLearningModelServerStartup
            .RetirementTaskPublishedObserverForTesting;
        try
        {
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = retirement =>
                {
                    publishedRetirement = retirement;
                    throw fatalRetirement;
                };

            var thrown = await Assert.ThrowsAsync<OutOfMemoryException>(() =>
                DesignConversionRetainedDpoPolicyBaseActivation.ActivateAsync(
                    name => environment.TryGetValue(name, out var value)
                        ? value
                        : null,
                    testHooks: new()
                    {
                        FileAccessOptions =
                            new ServerOwnedContinuousLearningFileAccessOptions
                            {
                                PlatformForTesting =
                                    ServerOwnedContinuousLearningFileAccessPlatform
                                        .Unsupported,
                            },
                    }));

            Assert.Same(fatalRetirement, thrown);
            Assert.NotNull(publishedRetirement);
            Assert.True(publishedRetirement!.IsCompleted);
            Assert.True(publishedRetirement.IsFaulted);
            Assert.NotNull(
                thrown.Data["CONTINUOUS_LEARNING_RETIREMENT_FAILURE"]);
            AssertRetainedPathsReleasedAndDeleteRoot(fixture.Root);
        }
        finally
        {
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = prior;
        }
    }

    [Fact]
    public async Task RetainedDpoPolicyActivation_CancellationFailsSanitizedAndReleasesPaths()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var environment = fixture.ValidEnvironment();
        DesignConversionRetainedDpoPolicyBaseProviderGateway? gateway = null;
        using var cancellation = new CancellationTokenSource();

        var resolution = await DesignConversionRetainedDpoPolicyBaseActivation
            .ActivateAsync(
                name => environment.TryGetValue(name, out var value)
                    ? value
                    : null,
                cancellation.Token,
                new()
                {
                    GatewayCreated = createdGateway => gateway = createdGateway,
                    AfterStartBeforeResolution = createdGateway =>
                    {
                        Assert.True(createdGateway.IsBound);
                        cancellation.Cancel();
                    },
                });

        AssertSanitizedActivationFailure(
            resolution,
            DesignConversionRetainedDpoPolicyBaseActivation
                .ActivationCancelledErrorCode);
        Assert.NotNull(gateway);
        Assert.False(gateway!.IsBound);
        Assert.Null(
            DesignConversionRetainedDpoPolicyBaseActivation
                .ResolveLegalTeacherAdmissionAuthority(gateway));
        Assert.Null(
            DesignConversionDpoTeacherAttestationAssociation
                .ResolveExactProvider(gateway));
        Assert.DoesNotContain(
            fixture.Root,
            resolution.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
        AssertRetainedPathsReleasedAndDeleteRoot(fixture.Root);
    }

    [Fact]
    public async Task RetainedDpoPolicyActivation_PostBindLossFailsSanitizedAndReleasesPaths()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var environment = fixture.ValidEnvironment();
        DesignConversionRetainedDpoPolicyBaseProviderGateway? gateway = null;

        var resolution = await DesignConversionRetainedDpoPolicyBaseActivation
            .ActivateAsync(
                name => environment.TryGetValue(name, out var value)
                    ? value
                    : null,
                testHooks: new()
                {
                    GatewayCreated = createdGateway => gateway = createdGateway,
                    AfterStartBeforeResolution = createdGateway =>
                        createdGateway.Unbind(),
                });

        AssertSanitizedActivationFailure(
            resolution,
            ContinuousLearningModelServerStartup.DpoPolicyBaseBindingFailedErrorCode);
        Assert.NotNull(gateway);
        Assert.False(gateway!.IsBound);
        Assert.DoesNotContain(
            fixture.Root,
            resolution.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
        AssertRetainedPathsReleasedAndDeleteRoot(fixture.Root);
    }

    [Fact]
    public async Task RetainedDpoPolicyActivation_ConcurrentMixedDisposalJoinsUntilProviderIsUnavailableAndPathsAreReleased()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var environment = fixture.ValidEnvironment();
        var stopEntered = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseStop = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var resolution = await DesignConversionRetainedDpoPolicyBaseActivation
            .ActivateAsync(
                name => environment.TryGetValue(name, out var value)
                    ? value
                    : null,
                testHooks: new()
                {
                    BeforeLeaseStopAsync = () =>
                    {
                        stopEntered.TrySetResult(true);
                        return releaseStop.Task;
                    },
                });
        var lease = Assert.IsType<
            DesignConversionRetainedDpoPolicyBaseActivationLease>(
                resolution.Lease);
        var provider = lease.Provider;
        var gateway = Assert.IsType<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>(provider);

        var disposalTimeout = TimeSpan.FromSeconds(10);
        var syncDispose = Task.Run(lease.Dispose);
        Task? asyncDispose = null;
        try
        {
            await stopEntered.Task.WaitAsync(disposalTimeout);
            asyncDispose = lease.DisposeAsync().AsTask();

            Assert.False(syncDispose.IsCompleted);
            Assert.False(asyncDispose.IsCompleted);
            Assert.True(gateway.IsBound);
        }
        finally
        {
            releaseStop.TrySetResult(true);
            asyncDispose ??= lease.DisposeAsync().AsTask();
            await Task.WhenAll(syncDispose, asyncDispose)
                .WaitAsync(disposalTimeout);
        }

        var unavailable = await provider.CreateModelSetAsync(CancellationToken.None);

        AssertFailure(
            unavailable,
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ProviderUnavailableErrorCode,
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ProviderUnavailableErrorMessage);
        Assert.False(gateway.IsBound);
        lease.Dispose();
        AssertRetainedPathsReleasedAndDeleteRoot(fixture.Root);
    }

    [Fact]
    public void RestartReadiness_IsStableAndRedacted()
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        const string modelMarker = "qwen-secret-model-marker:latest";
        var digestMarker = new string('c', 64);
        environment[ContinuousLearningModelServerConfigurationLoader.ModelIdEnvironmentName]
            = modelMarker;
        environment[ContinuousLearningModelServerConfigurationLoader.OllamaManifestDigestEnvironmentName]
            = digestMarker;
        var rawSalt = environment[
            ContinuousLearningModelServerConfigurationLoader.AuditSaltEnvironmentName];
        var before = fixture.Snapshot();

        var first = CreateStartup(environment);
        var second = CreateStartup(environment);
        var firstJson = JsonSerializer.Serialize(first.Readiness);
        var secondJson = JsonSerializer.Serialize(second.Readiness);

        Assert.Equal(firstJson, secondJson);
        Assert.Contains("\"configuration_admitted\":true", firstJson, StringComparison.Ordinal);
        Assert.Contains("\"composition_materialized\":false", firstJson, StringComparison.Ordinal);
        Assert.Contains("\"path_identity_verified\":false", firstJson, StringComparison.Ordinal);
        Assert.Contains(
            "\"retained_object_identity_bound\":false",
            firstJson,
            StringComparison.Ordinal);
        Assert.Contains("\"training_enabled\":false", firstJson, StringComparison.Ordinal);
        Assert.Contains("\"cycle_execution_available\":false", firstJson, StringComparison.Ordinal);
        Assert.Contains("\"legal_evidence_configured\":true", firstJson, StringComparison.Ordinal);
        Assert.Contains("\"legal_evidence_verified\":false", firstJson, StringComparison.Ordinal);
        Assert.Contains(
            environment[ContinuousLearningModelServerConfigurationLoader.LocalRetentionPolicyRefEnvironmentName],
            firstJson,
            StringComparison.Ordinal);
        Assert.Contains(
            environment[ContinuousLearningModelServerConfigurationLoader.OutputLicenseRefEnvironmentName],
            firstJson,
            StringComparison.Ordinal);
        Assert.DoesNotContain(modelMarker, firstJson, StringComparison.Ordinal);
        Assert.DoesNotContain(digestMarker, firstJson, StringComparison.Ordinal);
        Assert.DoesNotContain(rawSalt, firstJson, StringComparison.Ordinal);
        Assert.DoesNotContain(fixture.Root, firstJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("request_id", firstJson, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(before, fixture.Snapshot());
    }

    [Fact]
    public void PublicStartupApi_HasNoBuildRunTrainCyclePathOrPolicySurface()
    {
        var publicMethods = typeof(ContinuousLearningModelServerStartup)
            .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
            .Where(method => method.DeclaringType
                             == typeof(ContinuousLearningModelServerStartup));
        Assert.DoesNotContain(publicMethods, HasForbiddenExecutionOrPolicySurface);
        Assert.DoesNotContain(
            typeof(ContinuousLearningModelServerStartup)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public),
            property => HasForbiddenName(property.Name));

        var assembly = typeof(ContinuousLearningModelServerStartup).Assembly;
        Assert.Null(assembly.GetType("ContinuousLearning.ContinuousLearningProductionGraph"));
        Assert.Null(assembly.GetType("ContinuousLearning.BoundedCompilerStorage"));
        Assert.Null(assembly.GetType("ContinuousLearning.BoundedCompilerEventLogger"));
        var fileAccess = assembly.GetType(
            "ContinuousLearning.ServerOwnedContinuousLearningFileAccess");
        Assert.NotNull(fileAccess);
        Assert.False(fileAccess!.IsPublic);
    }

    [Fact]
    public void LegalTeacherLivePredicate_IsInternalExactConjunctionWithoutProductReadyOrTrustReads()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "14.ContinuousLearning",
            "Training",
            "ContinuousLearningModelServerComposition.cs"));
        const string signature =
            "internal bool IsLegalTrainingTeacherAdmissionActive()";
        var predicateSource = ExtractMethodSource(source, signature);
        const string expectedPredicateSource = """
            internal bool IsLegalTrainingTeacherAdmissionActive()
            {
                lock (_readinessGate)
                {
                    return _readiness.ConfigurationAdmitted
                           && _materializedComposition is not null
                           && _readiness.CompositionMaterialized
                           && _readiness.PathIdentityVerified
                           && _readiness.RetainedObjectIdentityBound
                           && _readiness.LegalEvidenceVerified
                           && _dpoPolicyBaseLifecycleState
                               == ContinuousLearningDpoPolicyBaseLifecycleState.ProviderBound
                           && _activeDpoPolicyBaseBindingToken is not null
                           && _designConversionRouteMappingCapability is
                               { IsActive: true }
                           && _designConversionTeacherAssociationCapability is
                               { IsActive: true }
                           && _readiness.StableDpoExecutorConfigured
                           && _readiness.TrainingEnabled
                           && _readiness.CycleExecutionAvailable
                           && _readiness.PerformsTraining;
                }
            }
            """;

        Assert.Equal(
            NormalizeSource(expectedPredicateSource),
            NormalizeSource(predicateSource));
    }

    [Fact]
    public void ProductionTeacherClientAndModelServerOptions_HaveNoPublicCallerSurface()
    {
        var clientType = typeof(LocalQwenOllamaTeacherInferenceClient);
        Assert.False(clientType.IsPublic);
        Assert.False(clientType.IsNestedPublic);
        Assert.False(clientType.IsVisible);
        Assert.DoesNotContain(
            clientType,
            typeof(ContinuousLearningModelServerStartup).Assembly.ExportedTypes);

        var create = clientType.GetMethod(
            nameof(LocalQwenOllamaTeacherInferenceClient.Create),
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        var infer = clientType.GetMethod(
            nameof(LocalQwenOllamaTeacherInferenceClient.InferAsync),
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        Assert.NotNull(create);
        Assert.NotNull(infer);
        Assert.False(create!.IsPublic);
        Assert.False(infer!.IsPublic);
        Assert.Empty(clientType.GetMembers(
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.DeclaredOnly));

        var forbiddenTypes = new HashSet<Type>
        {
            clientType,
            typeof(ITrainingTeacherInferenceClient),
            typeof(ContinuousLearningTp1DpoConfiguration)
        };
        bool ContainsForbiddenTeacherSurface(Type type)
        {
            if (forbiddenTypes.Contains(type))
                return true;
            if (type.HasElementType && type.GetElementType() is { } elementType)
                return ContainsForbiddenTeacherSurface(elementType);
            return type.IsGenericType &&
                   type.GetGenericArguments().Any(ContainsForbiddenTeacherSurface);
        }

        var modelServerPublicTypes = typeof(ModelServer.Program)
            .Assembly
            .ExportedTypes
            .ToArray();
        var publicModelServerMethods = modelServerPublicTypes.SelectMany(type =>
            type.GetMethods(
                BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.DeclaredOnly));
        Assert.DoesNotContain(
            publicModelServerMethods,
            method => ContainsForbiddenTeacherSurface(method.ReturnType) ||
                      method.GetParameters().Any(parameter =>
                          ContainsForbiddenTeacherSurface(parameter.ParameterType)));

        var publicModelServerOptions = modelServerPublicTypes
            .Where(type => type.Name.EndsWith("Options", StringComparison.Ordinal));
        Assert.All(publicModelServerOptions, optionsType =>
        {
            Assert.DoesNotContain(
                optionsType.GetConstructors(BindingFlags.Instance | BindingFlags.Public),
                constructor => constructor.GetParameters().Any(parameter =>
                    ContainsForbiddenTeacherSurface(parameter.ParameterType)));
            Assert.DoesNotContain(
                optionsType.GetProperties(BindingFlags.Instance | BindingFlags.Public),
                property => ContainsForbiddenTeacherSurface(property.PropertyType));
            Assert.DoesNotContain(
                optionsType.GetFields(BindingFlags.Instance | BindingFlags.Public),
                field => ContainsForbiddenTeacherSurface(field.FieldType));
        });
    }

    private static DesignConversionDpoPolicyBaseIdentity GetProviderIdentity(
        DesignConversionCheckpointBackedDpoPolicyBaseProvider provider)
        => Assert.IsType<DesignConversionDpoPolicyBaseIdentity>(
            typeof(DesignConversionCheckpointBackedDpoPolicyBaseProvider)
                .GetField(
                    "_identity",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(provider));

    private static async Task AssertRetainedCanonicalBpeTokenizerReferenceAsync(
        DesignConversionDpoPolicyBaseModelSet modelSet,
        string tokenizerPath,
        ModelConfig modelConfiguration,
        string privateRoot)
    {
        var reference = Assert.IsType<
            DesignConversionDpoCanonicalBpeTokenizerSnapshotReference>(
                modelSet.CanonicalBpeTokenizerSnapshot);
        var expected = BuildCanonicalBpeTokenizerDescriptor(
            tokenizerPath,
            modelConfiguration);

        Assert.Equal(expected, reference.Descriptor);
        Assert.False(reference.Descriptor.RawPayloadPersisted);
        Assert.False(reference.Descriptor.AbsolutePathPersisted);
        Assert.Equal(HashFile(tokenizerPath), reference.Descriptor.Sha256);
        Assert.Equal(
            modelConfiguration.VocabSize,
            reference.Descriptor.VocabularySize);
        AssertPublicTokenizerReferenceIsPathAndPayloadFree(
            reference,
            privateRoot);

        var reopened = await reference.OpenSnapshotAsync(CancellationToken.None);
        Assert.True(reopened.IsSuccess, reopened.ErrorMessage);
        using var stream = Assert.IsAssignableFrom<Stream>(reopened.Value);
        Assert.True(stream.CanRead);
        Assert.True(stream.CanSeek);
        Assert.Equal(0, stream.Position);
        var revalidated = DesignConversionDpoPromotionArtifactContractCodec
            .RevalidateCanonicalBpeTokenizerArtifactDescriptor(
                stream,
                modelConfiguration,
                expected);
        Assert.True(revalidated.IsSuccess, revalidated.ErrorMessage);
        Assert.Equal(expected, revalidated.Value);
        Assert.Equal(0, stream.Position);
    }

    private static DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor
        BuildCanonicalBpeTokenizerDescriptor(string path, ModelConfig config)
    {
        using var stream = File.OpenRead(path);
        var result = DesignConversionDpoPromotionArtifactContractCodec
            .BuildCanonicalBpeTokenizerArtifactDescriptor(stream, config);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return Assert.IsType<
            DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>(
                result.Value);
    }

    private static void AssertPublicTokenizerReferenceIsPathAndPayloadFree(
        DesignConversionDpoCanonicalBpeTokenizerSnapshotReference reference,
        string privateRoot)
    {
        var properties = reference.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);
        Assert.Equal(
            new[]
            {
                nameof(DesignConversionDpoCanonicalBpeTokenizerSnapshotReference
                    .Descriptor)
            },
            properties.Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.Empty(reference.GetType().GetFields(
            BindingFlags.Public | BindingFlags.Instance));
        var publicData = JsonSerializer.Serialize(reference.Descriptor);
        Assert.DoesNotContain(privateRoot, publicData, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"C:\", publicData, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/", publicData, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AA==", publicData, StringComparison.Ordinal);
    }

    private static void AssertFailure<T>(
        OperationResult<T> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    private static void AssertSanitizedActivationFailure(
        DesignConversionRetainedDpoPolicyBaseActivationResolution resolution,
        string? expectedCode = null)
    {
        Assert.False(resolution.IsSuccess);
        Assert.False(resolution.IsActivated);
        Assert.Equal(
            DesignConversionRetainedDpoPolicyBaseActivation.FailedStatus,
            resolution.Status);
        Assert.Null(resolution.Lease);
        Assert.Matches("^[A-Z0-9_]+$", resolution.ErrorCode);
        Assert.NotEqual(
            DesignConversionRetainedDpoPolicyBaseActivation.NoErrorCode,
            resolution.ErrorCode);
        if (expectedCode is not null)
            Assert.Equal(expectedCode, resolution.ErrorCode);
        Assert.Equal(
            DesignConversionRetainedDpoPolicyBaseActivation
                .ActivationFailedErrorMessage,
            resolution.ErrorMessage);
    }

    private static void AssertRetainedPathsReleasedAndDeleteRoot(string root)
    {
        var movedRoot = root + "-released-" + Guid.NewGuid().ToString("N");
        Directory.Move(root, movedRoot);
        Directory.Delete(movedRoot, recursive: true);
        Assert.False(Directory.Exists(root));
        Assert.False(Directory.Exists(movedRoot));
    }

    [Fact]
    public void TrainingActivationSwitch_IsStrictImmutableAndDefaultOff()
    {
        using var fixture = new ModelServerCompositionFixture();
        var defaultStartup = CreateStartup(fixture.ValidEnvironment());
        var enabledEnvironment = fixture.ValidEnvironment();
        RequestTrainingActivation(enabledEnvironment);
        var enabledStartup = CreateStartup(enabledEnvironment);
        enabledEnvironment[
            ContinuousLearningModelServerConfigurationLoader
                .TrainingActivationEnabledEnvironmentName] = bool.FalseString;
        enabledEnvironment.Remove(
            ContinuousLearningModelServerConfigurationLoader
                .TrainingRootInitializationSeedEnvironmentName);
        var explicitlyFalseStartup = CreateStartup(enabledEnvironment);
        var malformedEnvironment = fixture.ValidEnvironment();
        malformedEnvironment[
            ContinuousLearningModelServerConfigurationLoader
                .TrainingActivationEnabledEnvironmentName] = " true ";
        var malformedStartup = CreateStartup(malformedEnvironment);

        Assert.False(defaultStartup.TrainingActivationRequested);
        Assert.True(enabledStartup.TrainingActivationRequested);
        Assert.False(explicitlyFalseStartup.TrainingActivationRequested);
        Assert.False(malformedStartup.Readiness.ConfigurationAdmitted);
        Assert.Equal(
            "CONTINUOUS_LEARNING_CONFIGURATION_INVALID",
            malformedStartup.Readiness.ConfigurationErrorCode);
        AssertClosedAndUnmaterialized(malformedStartup.Readiness);
    }

    [Fact]
    public async Task StandaloneRetainedActivation_ExplicitTrainingOptInRemainsNonTrainingWithoutProgramRouteSurface()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var environment = fixture.ValidEnvironment();
        RequestTrainingActivation(environment);

        var resolution = await DesignConversionRetainedDpoPolicyBaseActivation
            .ActivateAsync(
                name => environment.TryGetValue(name, out var value)
                    ? value
                    : null,
                CancellationToken.None);
        var lease = Assert.IsType<
            DesignConversionRetainedDpoPolicyBaseActivationLease>(
                resolution.Lease);
        try
        {
            Assert.True(resolution.IsSuccess);
            Assert.True(resolution.IsActivated);
            Assert.NotNull(resolution.LegalTeacherAdmissionAuthority);
            var admission = resolution.LegalTeacherAdmissionAuthority!
                .ValidateBatch(
                    [TrainingTeacherProvenance.Deterministic(
                        "standalone-opt-in-no-route-surface")]);
            Assert.False(admission.IsSuccess);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority
                    .ConfigurationRequiredErrorCode,
                admission.ErrorCode);
        }
        finally
        {
            await lease.DisposeAsync();
        }
    }

    [Fact]
    public void TrainingActivation_MismatchedRouteCapabilityFailsClosed()
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        RequestTrainingActivation(environment);
        var startup = CreateStartup(environment);
        var surface = new object();
        var provider = new object();
        var issuer = startup
            .PrepareDesignConversionTrainingIntakeRouteMappingIssuer(
                surface,
                provider,
                startup.LegalTeacherAdmissionAuthority);
        Assert.NotNull(issuer);
        var callerAsserted = new CallerAssertedRouteMappingCapability(
            issuer!,
            new object(),
            surface,
            provider,
            startup.LegalTeacherAdmissionAuthority);

        Assert.False(startup
            .TryBindDesignConversionTrainingIntakeRouteMapping(callerAsserted));
        Assert.False(startup.Readiness.TrainingEnabled);
        Assert.False(startup.Readiness.CycleExecutionAvailable);
        Assert.False(startup.Readiness.PerformsTraining);
        Assert.False(startup.IsLegalTrainingTeacherAdmissionActive());
    }

    private sealed class CallerAssertedRouteMappingCapability
        : ContinuousLearningDesignConversionRouteMappingCapability
    {
        internal CallerAssertedRouteMappingCapability(
            ContinuousLearningDesignConversionRouteMappingCapability.Issuer issuer,
            object registryIdentity,
            object surfaceIdentity,
            object providerIdentity,
            ServerOwnedLegalTrainingTeacherAdmissionAuthority legalAuthority)
            : base(
                issuer,
                registryIdentity,
                surfaceIdentity,
                providerIdentity,
                legalAuthority)
        {
        }
    }

    private static void RecordMaximum(ref int maximum, int candidate)
    {
        var observed = Volatile.Read(ref maximum);
        while (candidate > observed)
        {
            var prior = Interlocked.CompareExchange(
                ref maximum,
                candidate,
                observed);
            if (prior == observed)
                return;
            observed = prior;
        }
    }

    private static void AssertModelsPhysicallyIndependentAndBitwiseEqual(
        LlmModel left,
        LlmModel right)
    {
        Assert.NotSame(left, right);
        Assert.NotSame(left.Config, right.Config);
        Assert.Equal(left.Config, right.Config);
        var leftEntries = left.GetCheckpointParameterEntries();
        var rightEntries = right.GetCheckpointParameterEntries();
        Assert.Equal(leftEntries.Count, rightEntries.Count);
        for (var index = 0; index < leftEntries.Count; index++)
        {
            Assert.Equal(leftEntries[index].Key, rightEntries[index].Key);
            Assert.Equal(
                leftEntries[index].Parameter.Value.Shape.Dimensions,
                rightEntries[index].Parameter.Value.Shape.Dimensions);
            Assert.NotSame(
                leftEntries[index].Parameter,
                rightEntries[index].Parameter);
            Assert.NotSame(
                leftEntries[index].Parameter.Value,
                rightEntries[index].Parameter.Value);
            Assert.NotSame(
                leftEntries[index].Parameter.Value.Storage,
                rightEntries[index].Parameter.Value.Storage);
            Assert.NotSame(
                leftEntries[index].Parameter.Value.Data,
                rightEntries[index].Parameter.Value.Data);
            Assert.Equal(
                leftEntries[index].Parameter.Value.Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray(),
                rightEntries[index].Parameter.Value.Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray());
        }
    }

    private static bool HasForbiddenExecutionOrPolicySurface(MethodInfo method)
        => HasForbiddenName(method.Name)
           || method.GetParameters().Any(parameter =>
               HasForbiddenName(parameter.Name ?? string.Empty)
               || parameter.ParameterType == typeof(LegalTrainingTeacherAdmissionAuthority));

    private static bool HasForbiddenName(string value)
        => value.Contains("build", StringComparison.OrdinalIgnoreCase)
           || value.Contains("run", StringComparison.OrdinalIgnoreCase)
           || value.Contains("train", StringComparison.OrdinalIgnoreCase)
           || value.Contains("cycle", StringComparison.OrdinalIgnoreCase)
           || value.Contains("path", StringComparison.OrdinalIgnoreCase)
           || value.Contains("salt", StringComparison.OrdinalIgnoreCase)
           || value.Contains("model", StringComparison.OrdinalIgnoreCase)
           || value.Contains("teacher", StringComparison.OrdinalIgnoreCase)
           || value.Contains("policy", StringComparison.OrdinalIgnoreCase);

    private static ContinuousLearningModelServerStartup CreateStartup(
        IReadOnlyDictionary<string, string> environment)
        => ContinuousLearningModelServerStartup.Create(name =>
            environment.TryGetValue(name, out var value) ? value : null);

    private static object GetMaterializedComposition(
        ContinuousLearningModelServerStartup startup)
    {
        var composition = GetMaterializedCompositionOrNull(startup);
        Assert.NotNull(composition);
        return composition;
    }

    private static ContinuousLearningDeferredCompositionPlan
        GetDeferredCompositionPlan(ContinuousLearningModelServerStartup startup)
        => Assert.IsType<ContinuousLearningDeferredCompositionPlan>(
            typeof(ContinuousLearningModelServerStartup)
                .GetField(
                    "_deferredPlan",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(startup));

    private static object? GetMaterializedCompositionOrNull(
        ContinuousLearningModelServerStartup startup)
        => typeof(ContinuousLearningModelServerStartup)
            .GetField(
                "_materializedComposition",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(startup);

    private static RetainedCheckpointBackedIncrementalTrainingSessionFactory
        GetRetainedSessionFactory(object composition)
        => Assert.IsType<
            RetainedCheckpointBackedIncrementalTrainingSessionFactory>(
                composition.GetType()
                    .GetField(
                        "_retainedSessionFactory",
                        BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(composition));

    private static bool GetPrivateBoolean(object instance, string fieldName)
        => Assert.IsType<bool>(instance.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(instance));

    private static void
        AssertRetainedActivationHasExactInactiveCallChainWithoutExecution(
            MethodInfo publicActivation,
            MethodInfo activationOverload)
    {
        const BindingFlags allDeclared =
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;

        var hostedStart = RequireExactMethod(
            typeof(ContinuousLearningRetainedObjectHostedActivator),
            nameof(ContinuousLearningRetainedObjectHostedActivator.StartAsync),
            allDeclared,
            typeof(CancellationToken),
            typeof(Action));
        var materialize = typeof(ContinuousLearningModelServerStartup)
            .GetMethod(
                nameof(ContinuousLearningModelServerStartup
                    .MaterializeInactiveTp1Composition),
                allDeclared)!;
        var activateProvider = RequireExactMethod(
            typeof(ContinuousLearningRetainedObjectHostedActivator),
            nameof(ContinuousLearningRetainedObjectHostedActivator
                .ActivatePolicyBaseProviderAsync),
            allDeclared,
            typeof(CancellationToken),
            typeof(Action<DesignConversionCheckpointBackedDpoPolicyBaseProvider>),
            typeof(Action));
        var materializedCreate = typeof(
                ContinuousLearningMaterializedTp1Composition)
            .GetMethod(
                nameof(ContinuousLearningMaterializedTp1Composition.Create),
                allDeclared)!;
        var validateBoundArtifacts = typeof(
                ContinuousLearningMaterializedTp1Composition)
            .GetMethod("ValidateBoundArtifacts", allDeclared)!;
        var createInactiveTrainer = typeof(ContinuousLearningProductionComposition)
            .GetMethods(allDeclared)
            .Single(method =>
                method.Name == nameof(
                    ContinuousLearningProductionComposition.CreateTp1Trainer)
                && method.GetParameters().FirstOrDefault()?.ParameterType ==
                    typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority));
        var createInactiveProvider = RequireExactMethod(
            typeof(DesignConversionRetainedDpoPolicyBaseComposition),
            nameof(DesignConversionRetainedDpoPolicyBaseComposition
                .CreateInactiveProviderAsync),
            allDeclared,
            typeof(ServerOwnedContinuousLearningFileAccess),
            typeof(ModelConfig),
            typeof(string),
            typeof(string),
            typeof(ulong?),
            typeof(string),
            typeof(CancellationToken));
        var captureTokenizerDescriptor = typeof(
                DesignConversionRetainedDpoPolicyBaseComposition)
            .GetMethod("CaptureCanonicalBpeTokenizerDescriptorAsync", allDeclared)!;
        var openCheckpointSnapshot = typeof(
                ContinuousLearningRetainedDpoPolicyCheckpointSource)
            .GetMethod(
                nameof(ContinuousLearningRetainedDpoPolicyCheckpointSource
                    .OpenCheckpointSnapshotAsync),
                allDeclared)!;
        var openTokenizerSnapshot = typeof(
                ContinuousLearningRetainedDpoPolicyCanonicalBpeTokenizerSnapshotSource)
            .GetMethod(
                nameof(
                    ContinuousLearningRetainedDpoPolicyCanonicalBpeTokenizerSnapshotSource
                        .OpenCanonicalBpeTokenizerSnapshotAsync),
                allDeclared)!;
        var createCheckpointBackedProvider = RequireExactMethod(
            typeof(DesignConversionCheckpointBackedDpoPolicyBaseProvider),
            nameof(DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .CreateWithOptions),
            allDeclared,
            typeof(IDesignConversionDpoPolicyCheckpointSource),
            typeof(ModelConfig),
            typeof(string),
            typeof(string),
            typeof(string),
            typeof(DesignConversionDpoPolicyBaseProviderOptions),
            typeof(DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor),
            typeof(IDesignConversionDpoCanonicalBpeTokenizerSnapshotSource));
        var bindExactProvider = typeof(
                DesignConversionRetainedDpoPolicyBaseProviderGateway)
            .GetMethod(
                nameof(DesignConversionRetainedDpoPolicyBaseProviderGateway
                    .BindExactProvider),
                allDeclared)!;

        var activationMoveNext = GetAsyncStateMachineMoveNext(activationOverload);
        var hostedStartMoveNext = GetAsyncStateMachineMoveNext(hostedStart);
        var activateProviderMoveNext =
            GetAsyncStateMachineMoveNext(activateProvider);
        var createInactiveProviderMoveNext =
            GetAsyncStateMachineMoveNext(createInactiveProvider);
        var captureTokenizerDescriptorMoveNext =
            GetAsyncStateMachineMoveNext(captureTokenizerDescriptor);

        Assert.True(ContainsCallTo(activationMoveNext, hostedStart));
        Assert.True(ContainsCallTo(hostedStartMoveNext, materialize));
        Assert.True(ContainsCallTo(hostedStartMoveNext, activateProvider));
        Assert.True(ContainsCallTo(materialize, materializedCreate));
        Assert.True(ContainsCallTo(materializedCreate, validateBoundArtifacts));
        Assert.True(ContainsCallTo(materializedCreate, createInactiveTrainer));
        Assert.True(ContainsCallTo(
            activateProviderMoveNext,
            createInactiveProvider));
        Assert.True(ContainsCallTo(activateProviderMoveNext, bindExactProvider));
        Assert.True(ContainsCallTo(
            createInactiveProviderMoveNext,
            openCheckpointSnapshot));
        Assert.True(ContainsCallTo(
            createInactiveProviderMoveNext,
            captureTokenizerDescriptor));
        Assert.True(ContainsCallTo(
            captureTokenizerDescriptorMoveNext,
            openTokenizerSnapshot));
        Assert.True(ContainsCallTo(
            createInactiveProviderMoveNext,
            createCheckpointBackedProvider));

        var reachable = CollectRepositoryCallGraph(
            publicActivation,
            typeof(DesignConversionRetainedDpoPolicyBaseActivation).Assembly,
            typeof(ContinuousLearningModelServerStartup).Assembly,
            typeof(OperationResult<>).Assembly,
            typeof(AgentInfrastructure.BaseService).Assembly,
            typeof(TensorCore.Core.Tensor).Assembly,
            typeof(DatasetPipeline.DataLoader).Assembly,
            typeof(TrainingLoop.Trainer).Assembly,
            typeof(LlmModel).Assembly,
            typeof(BpeTokenizer).Assembly,
            typeof(LoraApplicator).Assembly,
            typeof(DesignConversionCheckpointBackedDpoPolicyBaseProvider)
                .Assembly,
            typeof(DPO.DpoTrainer).Assembly,
            typeof(AgentInfrastructure.Logical.LogicalDpoTrainingUnit).Assembly);
        Assert.Contains(publicActivation, reachable);
        Assert.Contains(activationOverload, reachable);
        Assert.Contains(activationMoveNext, reachable);

        var forbidden = new List<MethodBase>();
        void Forbid(Type type, string methodName, params Type[] parameterTypes)
            => AddForbiddenBoundary(
                forbidden,
                RequireExactMethod(
                    type,
                    methodName,
                    allDeclared,
                    parameterTypes));
        void ForbidConstructor(Type type, params Type[] parameterTypes)
            => forbidden.Add(RequireExactConstructor(type, parameterTypes));

        ForbidConstructor(typeof(LlmModel), typeof(ModelConfig));
        Forbid(typeof(LlmModel), nameof(LlmModel.LoadCheckpoint), typeof(string));
        Forbid(typeof(LlmModel), nameof(LlmModel.LoadCheckpoint), typeof(Stream));
        Forbid(typeof(LlmModel), nameof(LlmModel.LoadAdapterCheckpoint), typeof(string));
        Forbid(typeof(LlmModel), nameof(LlmModel.LoadAdapterCheckpoint), typeof(Stream));
        Forbid(typeof(LlmModel), nameof(LlmModel.Forward), typeof(TensorCore.Core.Tensor));
        Forbid(
            typeof(LlmModel),
            nameof(LlmModel.Forward),
            typeof(TensorCore.Core.Tensor),
            typeof(bool));
        Forbid(
            typeof(LlmModel),
            nameof(LlmModel.Forward),
            typeof(TensorCore.Core.Tensor),
            typeof(bool),
            typeof(CancellationToken));
        Forbid(
            typeof(LlmModel),
            nameof(LlmModel.Forward),
            typeof(TensorCore.Core.Tensor),
            typeof(bool),
            typeof(CancellationToken),
            typeof(Action<LlmForwardPhase>));
        Forbid(
            typeof(LlmModel),
            nameof(LlmModel.ForwardWithCache),
            typeof(TensorCore.Core.Tensor),
            typeof(TransformerModel.Caching.ITransformerKvCache));
        Forbid(
            typeof(LlmModel),
            nameof(LlmModel.ForwardWithCache),
            typeof(TensorCore.Core.Tensor),
            typeof(TransformerModel.Caching.ITransformerKvCache),
            typeof(CancellationToken));
        Forbid(typeof(LlmModel), nameof(LlmModel.ForwardHidden), typeof(TensorCore.Core.Tensor));
        Forbid(
            typeof(LlmModel),
            nameof(LlmModel.ForwardHidden),
            typeof(TensorCore.Core.Tensor),
            typeof(bool));
        Forbid(
            typeof(LlmModel),
            nameof(LlmModel.ForwardHidden),
            typeof(TensorCore.Core.Tensor),
            typeof(bool),
            typeof(CancellationToken));

        Forbid(
            typeof(IDesignConversionDpoPolicyBaseProvider),
            nameof(IDesignConversionDpoPolicyBaseProvider.CreateModelSetAsync),
            typeof(CancellationToken));
        Forbid(
            typeof(IDesignConversionDpoPolicyContinuationProvider),
            nameof(IDesignConversionDpoPolicyContinuationProvider
                .CreateContinuedModelSetAsync),
            typeof(DesignConversionDpoPolicyContinuationRequest),
            typeof(CancellationToken));
        Forbid(
            typeof(DesignConversionCheckpointBackedDpoPolicyBaseProvider),
            nameof(DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .CreateModelSetAsync),
            typeof(CancellationToken));
        Forbid(
            typeof(DesignConversionCheckpointBackedDpoPolicyBaseProvider),
            nameof(DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .CreateContinuedModelSetAsync),
            typeof(DesignConversionDpoPolicyContinuationRequest),
            typeof(CancellationToken));
        foreach (var loaderType in new[]
                 {
                     typeof(IDesignConversionActivatedModelCheckpointLoader),
                     typeof(DesignConversionLocalActivatedModelCheckpointLoader),
                     typeof(DesignConversionRepositoryActivatedModelCheckpointLoader),
                 })
        {
            Forbid(
                loaderType,
                nameof(IDesignConversionActivatedModelCheckpointLoader.LoadAsync),
                typeof(DesignConversionActivatedModelVersion),
                typeof(CancellationToken));
        }
        Forbid(
            typeof(DesignConversionMachine),
            nameof(DesignConversionMachine.LoadCheckpointAsync),
            typeof(string),
            typeof(CancellationToken));
        Forbid(
            typeof(DesignConversionMachine),
            nameof(DesignConversionMachine.LoadCheckpointAsync),
            typeof(string),
            typeof(Func<string, CancellationToken, Task<byte[]>>),
            typeof(CancellationToken));

        Forbid(typeof(IIncrementalTrainingSessionFactory), nameof(IIncrementalTrainingSessionFactory.Create), typeof(string));
        Forbid(
            typeof(IIncrementalTrainingSessionFactory),
            nameof(IIncrementalTrainingSessionFactory.Create),
            typeof(string),
            typeof(CancellationToken));
        Forbid(typeof(InactiveIncrementalTrainingSessionFactoryGuard), nameof(InactiveIncrementalTrainingSessionFactoryGuard.Create), typeof(string));
        Forbid(
            typeof(InactiveIncrementalTrainingSessionFactoryGuard),
            nameof(InactiveIncrementalTrainingSessionFactoryGuard.Create),
            typeof(string),
            typeof(CancellationToken));
        Forbid(typeof(RetainedCheckpointBackedIncrementalTrainingSessionFactory), nameof(RetainedCheckpointBackedIncrementalTrainingSessionFactory.Create), typeof(string));
        Forbid(
            typeof(RetainedCheckpointBackedIncrementalTrainingSessionFactory),
            nameof(RetainedCheckpointBackedIncrementalTrainingSessionFactory.Create),
            typeof(string),
            typeof(CancellationToken));
        Forbid(typeof(CheckpointBackedIncrementalTrainingSessionFactory), nameof(CheckpointBackedIncrementalTrainingSessionFactory.Create), typeof(string));

        var incrementalTrainingParameters = new[]
        {
            typeof(string),
            typeof(IReadOnlyList<TrainingExample>),
            typeof(IReadOnlyList<DpoPair>),
            typeof(CancellationToken),
        };
        Forbid(typeof(IIncrementalTrainer), nameof(IIncrementalTrainer.TrainAsync), incrementalTrainingParameters);
        Forbid(typeof(IncrementalTrainer), nameof(IncrementalTrainer.TrainAsync), incrementalTrainingParameters);
        Forbid(
            typeof(IDpoTrainingExecutor),
            nameof(IDpoTrainingExecutor.ExecuteAsync),
            typeof(DpoTrainingExecutionRequest),
            typeof(CancellationToken));
        Forbid(
            typeof(StableDpoTrainingExecutor),
            nameof(StableDpoTrainingExecutor.ExecuteAsync),
            typeof(DpoTrainingExecutionRequest),
            typeof(CancellationToken));
        ForbidConstructor(
            typeof(DPO.DpoTrainer),
            typeof(LlmModel),
            typeof(LlmModel),
            typeof(string),
            typeof(float));
        ForbidConstructor(
            typeof(DPO.DpoTrainer),
            typeof(LlmModel),
            typeof(LlmModel),
            typeof(string),
            typeof(float),
            typeof(DPO.DpoTrainerPublicationObserver));
        Forbid(typeof(DPO.DpoTrainer), nameof(DPO.DpoTrainer.Step), typeof(DPO.Models.DpoBatch), typeof(int));
        Forbid(typeof(DPO.DpoTrainer), nameof(DPO.DpoTrainer.StepMany), typeof(IReadOnlyList<DPO.Models.DpoBatch>), typeof(int));
        Forbid(typeof(DPO.DpoTrainer), nameof(DPO.DpoTrainer.StepVector), typeof(IReadOnlyList<DPO.DpoVectorBatch>), typeof(int));
        Forbid(
            typeof(AgentInfrastructure.Logical.LogicalDpoTrainingUnit),
            nameof(AgentInfrastructure.Logical.LogicalDpoTrainingUnit.ExecuteAsync),
            typeof(AgentInfrastructure.Logical.LogicalUnitRequest<
                AgentInfrastructure.Logical.LogicalDpoTrainingRequest>),
            typeof(CancellationToken));

        var handoffParameters = new[]
        {
            typeof(IEnumerable<DesignConversionDpoTrainingRecord>),
            typeof(IDesignConversionDpoPolicyBaseProvider),
            typeof(BpeTokenizer),
            typeof(string),
            typeof(LoraConfig),
            typeof(int),
            typeof(CancellationToken),
        };
        Forbid(typeof(DesignConversionDpoTrainingHandoff), nameof(DesignConversionDpoTrainingHandoff.ExecuteAsync), handoffParameters);
        Forbid(
            typeof(DesignConversionDpoTrainingHandoff),
            nameof(DesignConversionDpoTrainingHandoff.ExecuteAsync),
            [typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority), .. handoffParameters]);
        Forbid(
            typeof(DesignConversionDpoTrainingHandoff),
            "ExecuteWithAuthorityAsync",
            [typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority), .. handoffParameters]);

        var handoffConversionParameters = new[]
        {
            typeof(DesignConversionDpoTrainingRecord),
            typeof(string),
            typeof(IDesignConversionDpoPolicyBaseProvider),
            typeof(DesignConversionLoraCheckpointConfiguration),
            typeof(string),
            typeof(CancellationToken),
        };
        Forbid(
            typeof(DesignConversionDpoTrainingHandoff),
            nameof(DesignConversionDpoTrainingHandoff.ConvertPolicyCheckpointToRuntimeCheckpointAsync),
            handoffConversionParameters);
        Forbid(
            typeof(DesignConversionDpoTrainingHandoff),
            nameof(DesignConversionDpoTrainingHandoff.ConvertPolicyCheckpointToRuntimeCheckpointAsync),
            [typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority), .. handoffConversionParameters]);
        Forbid(
            typeof(DesignConversionDpoTrainingHandoff),
            "ConvertWithAuthorityAsync",
            [typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority), .. handoffConversionParameters]);

        Forbid(
            typeof(LegalTrainingTeacherAdmissionAuthority),
            nameof(LegalTrainingTeacherAdmissionAuthority.ValidateBatch),
            typeof(IEnumerable<TrainingTeacherProvenance>));
        Forbid(
            typeof(IDesignConversionImprovementCandidateStrategy),
            nameof(IDesignConversionImprovementCandidateStrategy
                .CreateNextCandidateAsync),
            typeof(DesignConversionCandidateUpdateRequest),
            typeof(CancellationToken));
        Forbid(
            typeof(DesignConversionImprovementLoopCoordinator),
            nameof(DesignConversionImprovementLoopCoordinator.RunAsync),
            typeof(DesignConversionImprovementLoopRequest),
            typeof(CancellationToken));
        Forbid(
            typeof(IContinuousLearningPipeline),
            nameof(IContinuousLearningPipeline.RunCycleAsync),
            typeof(string),
            typeof(CancellationToken));
        Forbid(
            typeof(ContinuousLearningPipeline),
            nameof(ContinuousLearningPipeline.RunCycleAsync),
            typeof(string),
            typeof(CancellationToken));

        Forbid(
            typeof(TrainingLoop.Trainer),
            nameof(TrainingLoop.Trainer.TrainSupervisedNextTokenStep),
            typeof(LlmModel),
            typeof(TrainingLoop.Optimizers.AdamW),
            typeof(IReadOnlyList<int>),
            typeof(float));
        Forbid(
            typeof(TrainingLoop.Trainer),
            nameof(TrainingLoop.Trainer.TrainAsync),
            typeof(LlmModel),
            typeof(TrainingLoop.Optimizers.AdamW),
            typeof(DatasetPipeline.DataLoader),
            typeof(CancellationToken));
        Forbid(
            typeof(TrainingLoop.Trainer),
            "RunPretrainingCoreAsync",
            typeof(LlmModel),
            typeof(TrainingLoop.Optimizers.AdamW),
            typeof(Func<(long[,], long[,])>),
            typeof(Action<string>),
            typeof(CancellationToken));
        var supervisedStepParameters = new[]
        {
            typeof(IncrementalTrainingSession),
            typeof(IReadOnlyList<int>),
            typeof(float),
            typeof(IReadOnlyList<TensorCore.Layers.Parameter>),
        };
        Forbid(typeof(ISupervisedTrainingStepExecutor), nameof(ISupervisedTrainingStepExecutor.Execute), supervisedStepParameters);
        Forbid(typeof(ProductionSupervisedTrainingStepExecutor), nameof(ProductionSupervisedTrainingStepExecutor.Execute), supervisedStepParameters);

        Assert.NotEmpty(forbidden);

        var reachedForbidden = forbidden
            .Distinct()
            .Where(target => reachable.Any(
                candidate => HasSameMethodIdentity(candidate, target)))
            .Select(DescribeMethod)
            .OrderBy(static description => description, StringComparer.Ordinal)
            .ToArray();
        Assert.True(
            reachedForbidden.Length == 0,
            $"Retained activation reached forbidden execution boundaries: {string.Join(", ", reachedForbidden)}");
    }

    private static MethodInfo RequireExactMethod(
        Type type,
        string methodName,
        BindingFlags bindingFlags,
        params Type[] parameterTypes)
    {
        var method = type.GetMethod(
            methodName,
            bindingFlags,
            binder: null,
            parameterTypes,
            modifiers: null);
        Assert.True(
            method is not null,
            $"Expected exact method boundary was not resolved: {type.FullName}.{methodName}({string.Join(",", parameterTypes.Select(static parameter => parameter.FullName))}).");
        return method!;
    }

    private static ConstructorInfo RequireExactConstructor(
        Type type,
        params Type[] parameterTypes)
    {
        var constructor = type.GetConstructor(
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly,
            binder: null,
            parameterTypes,
            modifiers: null);
        Assert.True(
            constructor is not null,
            $"Expected exact constructor boundary was not resolved: {type.FullName}({string.Join(",", parameterTypes.Select(static parameter => parameter.FullName))}).");
        return constructor!;
    }

    private static void AddForbiddenBoundary(
        ICollection<MethodBase> forbidden,
        MethodInfo method)
    {
        forbidden.Add(method);
        var stateMachine = method.GetCustomAttribute<
            System.Runtime.CompilerServices.AsyncStateMachineAttribute>();
        if (stateMachine is not null)
            forbidden.Add(GetAsyncStateMachineMoveNext(method));
    }

    private static MethodInfo GetAsyncStateMachineMoveNext(MethodInfo asyncMethod)
    {
        var stateMachine = asyncMethod.GetCustomAttribute<
            System.Runtime.CompilerServices.AsyncStateMachineAttribute>();
        Assert.NotNull(stateMachine);
        return stateMachine!.StateMachineType.GetMethod(
                   nameof(System.Runtime.CompilerServices.IAsyncStateMachine.MoveNext),
                   BindingFlags.Instance |
                   BindingFlags.Public |
                   BindingFlags.NonPublic)
               ?? throw new InvalidOperationException(
                   $"Async state machine for {DescribeMethod(asyncMethod)} has no MoveNext body.");
    }

    private static IReadOnlySet<MethodBase> CollectRepositoryCallGraph(
        MethodInfo root,
        params Assembly[] traversableAssemblies)
    {
        var repositoryOwnedAssemblyNames =
            ReadModelServerProjectClosureAssemblyNames();
        var allowedAssemblyNames = traversableAssemblies
            .Select(static assembly => assembly.GetName().Name)
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .Select(static name => name!)
            .ToHashSet(StringComparer.Ordinal);
        var rootAssemblyName = root.Module.Assembly.GetName().Name;
        Assert.NotNull(rootAssemblyName);
        Assert.Contains(rootAssemblyName!, allowedAssemblyNames);
        Assert.Contains(rootAssemblyName, repositoryOwnedAssemblyNames);
        Assert.All(
            allowedAssemblyNames,
            assemblyName => Assert.Contains(
                assemblyName,
                repositoryOwnedAssemblyNames));
        var reachable = new HashSet<MethodBase>();
        var inspectedBodies = new HashSet<MethodBase>();
        var pending = new Stack<MethodBase>();
        pending.Push(root);

        while (pending.Count > 0)
        {
            var method = pending.Pop();
            if (!inspectedBodies.Add(method))
                continue;
            reachable.Add(method);

            if (method is MethodInfo methodInfo)
            {
                var stateMachine = methodInfo.GetCustomAttribute<
                    System.Runtime.CompilerServices.AsyncStateMachineAttribute>();
                if (stateMachine is not null)
                {
                    var moveNext = stateMachine.StateMachineType.GetMethod(
                        nameof(System.Runtime.CompilerServices.IAsyncStateMachine
                            .MoveNext),
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic);
                    if (moveNext is not null)
                        pending.Push(moveNext);
                }
            }

            var il = method.GetMethodBody()?.GetILAsByteArray();
            if (il is null)
                continue;

            foreach (var called in EnumerateCalledMethods(il, method))
            {
                reachable.Add(called);
                var calledAssemblyName = called.Module.Assembly.GetName().Name;
                if (calledAssemblyName is null
                    || !repositoryOwnedAssemblyNames.Contains(calledAssemblyName))
                {
                    continue;
                }

                Assert.True(
                    allowedAssemblyNames.Contains(calledAssemblyName),
                    $"Reachable repository-owned call escaped the approved activation graph: {DescribeMethod(method)} -> {DescribeMethod(called)} in {calledAssemblyName}.");
                pending.Push(called);
            }
        }

        return reachable;
    }

    private static IReadOnlySet<string>
        ReadModelServerProjectClosureAssemblyNames()
    {
        var repositoryRoot = FindRepositoryRoot();
        var rootProjectPath = Path.Combine(
            repositoryRoot,
            "25.ModelServer",
            "ModelServer.csproj");
        Assert.True(
            File.Exists(rootProjectPath),
            $"ModelServer project was not found: {rootProjectPath}.");

        var assemblyNames = new HashSet<string>(StringComparer.Ordinal);
        var inspectedProjects = new HashSet<string>(
            OperatingSystem.IsWindows()
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal);
        var pendingProjects = new Stack<string>();
        pendingProjects.Push(rootProjectPath);
        while (pendingProjects.Count > 0)
        {
            var projectPath = Path.GetFullPath(pendingProjects.Pop());
            if (!inspectedProjects.Add(projectPath))
                continue;
            Assert.True(
                File.Exists(projectPath),
                $"Repository project reference was not found: {projectPath}.");
            var project = System.Xml.Linq.XDocument.Load(projectPath);
            var configuredAssemblyName = project
                .Descendants()
                .FirstOrDefault(static element =>
                    element.Name.LocalName == "AssemblyName")
                ?.Value;
            var assemblyName = string.IsNullOrWhiteSpace(configuredAssemblyName)
                ? Path.GetFileNameWithoutExtension(projectPath)
                : configuredAssemblyName.Trim();
            Assert.False(string.IsNullOrWhiteSpace(assemblyName));
            assemblyNames.Add(assemblyName);

            foreach (var projectReference in project
                         .Descendants()
                         .Where(static element =>
                             element.Name.LocalName == "ProjectReference"))
            {
                var include = projectReference.Attribute("Include")?.Value;
                Assert.False(string.IsNullOrWhiteSpace(include));
                var referencedPath = Path.GetFullPath(Path.Combine(
                    Path.GetDirectoryName(projectPath)!,
                    include!
                        .Replace('\\', Path.DirectorySeparatorChar)
                        .Replace('/', Path.DirectorySeparatorChar)));
                Assert.True(
                    referencedPath.StartsWith(
                        Path.TrimEndingDirectorySeparator(repositoryRoot)
                        + Path.DirectorySeparatorChar,
                        OperatingSystem.IsWindows()
                            ? StringComparison.OrdinalIgnoreCase
                            : StringComparison.Ordinal),
                    $"Project reference escaped the repository root: {referencedPath}.");
                pendingProjects.Push(referencedPath);
            }
        }

        Assert.NotEmpty(assemblyNames);
        return assemblyNames;
    }

    private static string DescribeMethod(MethodBase method)
        => $"{method.DeclaringType?.FullName}.{method.Name}({string.Join(",", method.GetParameters().Select(static parameter => parameter.ParameterType.Name))})";

    private static bool ContainsCallTo(MethodInfo caller, MethodBase target)
    {
        var il = caller.GetMethodBody()?.GetILAsByteArray()
                 ?? throw new InvalidOperationException(
                     "The tested guard overload must have a managed IL body.");
        return ContainsCallTo(il, caller, target);
    }

    private static bool ContainsCallTo(
        byte[] il,
        MethodInfo caller,
        MethodBase target)
        => EnumerateCalledMethods(il, caller)
            .Any(candidate => HasSameMethodIdentity(candidate, target));

    private static IReadOnlyList<MethodBase> EnumerateCalledMethods(
        byte[] il,
        MethodBase caller)
    {
        var calledMethods = new List<MethodBase>();
        var offset = 0;
        while (offset < il.Length)
        {
            var instructionOffset = offset;
            var firstByte = il[offset++];
            ushort opcodeValue = firstByte;
            if (firstByte == 0xFE)
            {
                EnsureIlBytesAvailable(il, offset, 1, instructionOffset);
                opcodeValue = checked((ushort)(0xFE00 | il[offset++]));
            }

            if (!IlOpcodes.TryGetValue(opcodeValue, out var opcode))
            {
                throw new InvalidDataException(
                    $"Unknown IL opcode 0x{opcodeValue:X4} at offset {instructionOffset}.");
            }

            var operandOffset = offset;
            var operandSize = GetIlOperandSize(
                il,
                operandOffset,
                opcode,
                instructionOffset);
            EnsureIlBytesAvailable(
                il,
                operandOffset,
                operandSize,
                instructionOffset);

            if (opcode.OperandType == OperandType.InlineMethod
                && (opcode.Value == OpCodes.Call.Value
                    || opcode.Value == OpCodes.Callvirt.Value
                    || opcode.Value == OpCodes.Newobj.Value))
            {
                var metadataToken = BinaryPrimitives.ReadInt32LittleEndian(
                    il.AsSpan(operandOffset, sizeof(int)));
                MethodBase resolved;
                try
                {
                    resolved = caller.Module.ResolveMethod(
                        metadataToken,
                        GetDeclaringTypeGenericArguments(caller),
                        GetMethodGenericArguments(caller))
                        ?? throw new InvalidDataException(
                            $"IL method token 0x{metadataToken:X8} could not be resolved.");
                }
                catch (InvalidDataException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    throw new InvalidDataException(
                        $"IL method token 0x{metadataToken:X8} is invalid.",
                        exception);
                }

                calledMethods.Add(resolved);
            }

            offset = checked(operandOffset + operandSize);
        }
        return calledMethods;
    }

    private static readonly IReadOnlyDictionary<ushort, OpCode> IlOpcodes =
        BuildIlOpcodeLookup();

    private static IReadOnlyDictionary<ushort, OpCode> BuildIlOpcodeLookup()
    {
        var opcodes = new Dictionary<ushort, OpCode>();
        foreach (var field in typeof(OpCodes).GetFields(
                     BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType != typeof(OpCode))
                continue;
            var opcode = (OpCode)field.GetValue(null)!;
            var key = unchecked((ushort)opcode.Value);
            if (!opcodes.TryAdd(key, opcode))
            {
                throw new InvalidOperationException(
                    $"Duplicate IL opcode 0x{key:X4} in the runtime opcode table.");
            }
        }
        return opcodes;
    }

    private static int GetIlOperandSize(
        byte[] il,
        int operandOffset,
        OpCode opcode,
        int instructionOffset)
    {
        if (opcode.OperandType == OperandType.InlineSwitch)
        {
            EnsureIlBytesAvailable(
                il,
                operandOffset,
                sizeof(int),
                instructionOffset);
            var targetCount = BinaryPrimitives.ReadInt32LittleEndian(
                il.AsSpan(operandOffset, sizeof(int)));
            if (targetCount < 0)
            {
                throw new InvalidDataException(
                    $"Negative IL switch target count at offset {instructionOffset}.");
            }
            try
            {
                return checked(sizeof(int) + checked(targetCount * sizeof(int)));
            }
            catch (OverflowException exception)
            {
                throw new InvalidDataException(
                    $"IL switch operand overflows at offset {instructionOffset}.",
                    exception);
            }
        }

        return opcode.OperandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget or
                OperandType.ShortInlineI or
                OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineBrTarget or
                OperandType.InlineField or
                OperandType.InlineI or
                OperandType.InlineMethod or
                OperandType.InlineSig or
                OperandType.InlineString or
                OperandType.InlineTok or
            OperandType.InlineType or
                OperandType.ShortInlineR => 4,
            OperandType.InlineI8 or OperandType.InlineR => 8,
            _ => throw new InvalidDataException(
                $"Unsupported IL operand type at offset {instructionOffset}."),
        };
    }

    private static void EnsureIlBytesAvailable(
        byte[] il,
        int offset,
        int byteCount,
        int instructionOffset)
    {
        if (offset < 0 || byteCount < 0 || offset > il.Length - byteCount)
        {
            throw new InvalidDataException(
                $"Truncated IL operand at offset {instructionOffset}.");
        }
    }

    private static Type[]? GetDeclaringTypeGenericArguments(MethodBase method)
    {
        try
        {
            return method.DeclaringType?.GetGenericArguments();
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }

    private static Type[] GetMethodGenericArguments(MethodBase method)
    {
        try
        {
            return method.GetGenericArguments();
        }
        catch (NotSupportedException)
        {
            return Type.EmptyTypes;
        }
    }

    private static bool HasSameMethodIdentity(
        MethodBase candidate,
        MethodBase target)
    {
        if (!candidate.Equals(target) ||
            candidate.DeclaringType != target.DeclaringType)
        {
            return false;
        }

        return GetMethodGenericArguments(candidate)
            .SequenceEqual(GetMethodGenericArguments(target));
    }

    private static byte[] EncodeSyntheticMethodCall(
        OpCode opcode,
        MethodBase method)
    {
        if (opcode.Value != OpCodes.Call.Value
            && opcode.Value != OpCodes.Callvirt.Value
            && opcode.Value != OpCodes.Newobj.Value)
        {
            throw new ArgumentException(
                "Synthetic method calls require call, callvirt, or newobj.",
                nameof(opcode));
        }

        var il = new byte[6];
        il[0] = unchecked((byte)opcode.Value);
        BinaryPrimitives.WriteInt32LittleEndian(
            il.AsSpan(1, sizeof(int)),
            method.MetadataToken);
        il[5] = unchecked((byte)OpCodes.Ret.Value);
        return il;
    }

    private static void AssertRootOwnerLocksAreAvailable(
        IReadOnlyDictionary<string, string> environment)
    {
        var startup = CreateStartup(environment);
        using var access = startup.AcquireRetainedFileAccess();
        using var registryRoot = access.AcquireRegistryRootCapability();
        using var adapterRoot = access.AcquireAdapterRootCapability();
        Assert.NotNull(registryRoot);
        Assert.NotNull(adapterRoot);
    }

    private static void AssertOnlyOwnerLockLeavesAdded(
        IReadOnlyCollection<string> before,
        IReadOnlyCollection<string> after)
    {
        Assert.Empty(before.Except(after, StringComparer.Ordinal));
        var added = after
            .Except(before, StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] expected =
        [
            $"F|{Path.Combine("adapter-root", ".continuous-learning.adapter-owner.lock")}|0",
            $"F|{Path.Combine("registry-root", ".continuous-learning.registry-owner.lock")}|0",
        ];
        Assert.Equal(expected.Order(StringComparer.Ordinal), added);
    }

    private static string SeedMissingRetainedAdapterReference(
        IReadOnlyDictionary<string, string> environment,
        ModelConfig modelConfiguration,
        string adapterRootPath,
        string baseCheckpointPath,
        string tokenizerPath)
    {
        var startup = CreateStartup(environment);
        using var access = startup.AcquireRetainedFileAccess();
        using var registryRoot = access.AcquireRegistryRootCapability();
        using var adapterRoot = access.AcquireAdapterRootCapability();
        var model = new LlmModel(modelConfiguration);
        _ = LoraApplicator.Apply(model, IncrementalTrainer.DefaultConfig);
        var publication = adapterRoot.Publish(
            model,
            ServerOwnedContinuousLearningAdapterPublicationRequest.Create(
                "bug",
                "startup-resolution-test"));
        var reference = AdapterArtifactReference.FromPublication(publication);
        var sourceId = TrainingSourceProvenance.CreateSourceId(
            "model-server-composition/BUG",
            "missing retained adapter must remain deferred");
        var manifest = Assert.IsType<TrainingSourceManifest>(
            TrainingSourceProvenance.BuildManifest([sourceId]).Value);
        var provenance = new ModelAdapterProvenance(
            manifest,
            publication.Sha256Hex,
            HashFile(baseCheckpointPath),
            HashFile(tokenizerPath),
            TrainingSourceProvenance.Identify(modelConfiguration),
            TrainingSourceProvenance.Identify(IncrementalTrainer.DefaultConfig));
        var registryBytes = JsonSerializer.SerializeToUtf8Bytes(new
        {
            SchemaVersion = 2,
            Adapters = new[]
            {
                new
                {
                    Domain = "BUG",
                    TrainingExamples = 1,
                    TrainedAt = DateTimeOffset.UtcNow,
                    Provenance = provenance,
                    StorageKind = 2,
                    RetainedReference = reference,
                },
            },
        });
        PublishInitialRegistry(registryRoot, registryBytes);
        var artifactPath = Path.Combine(
            adapterRootPath,
            reference.CanonicalLeafName);
        File.Delete(artifactPath);
        return artifactPath;
    }

    private static void PublishInitialRegistry(
        ServerOwnedContinuousLearningRegistryRootCapability registryRoot,
        byte[] bytes)
    {
        if (OperatingSystem.IsLinux())
        {
            // POSIX seed использует только initial create-only registry route:
            // test не должен зелёнить temp→canonical rename, который теперь
            // честно считается unsafe exact namespace mutation.
            var created = registryRoot.CreateCanonicalIfAbsent(bytes);
            var canonical = registryRoot.ReadCanonical();
            Assert.Equal(
                ServerOwnedContinuousLearningRegistryCanonicalState.Present,
                canonical.State);
            Assert.NotNull(canonical.Snapshot);
            Assert.True(canonical.Snapshot!.Matches(created));
            Assert.Equal(bytes, canonical.Snapshot.CopyBytes());
            return;
        }

        var prior =
            ServerOwnedContinuousLearningRegistryPriorStateEvidence.Absent();
        var written = registryRoot.CreateWriteTemp(bytes);
        var candidate = new ServerOwnedContinuousLearningRegistrySnapshot(
            written.Identity,
            bytes);
        Assert.True(registryRoot.ReadExact(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            written.Identity).Matches(candidate));
        var rollback = registryRoot.PreparePublication(candidate, prior);
        registryRoot.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            candidate.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
        registryRoot.FlushFile(
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
            candidate.Identity);
        registryRoot.FlushRootDirectory();
        var accepted = registryRoot.CommitPublication(candidate, prior, rollback);
        _ = registryRoot.ResumeAcceptedCleanup(accepted);
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return TrainingSourceProvenance.Sha256(stream);
    }

    private static void AssertClosedAndUnmaterialized(
        ContinuousLearningProductionReadinessStatus status)
    {
        Assert.False(status.CompositionMaterialized);
        Assert.False(status.PathIdentityVerified);
        Assert.False(status.RetainedObjectIdentityBound);
        Assert.False(status.StableDpoExecutorConfigured);
        Assert.False(status.TrainingEnabled);
        Assert.False(status.CycleExecutionAvailable);
        Assert.False(status.PerformsTraining);
        Assert.False(status.ProductReady);
    }

    private static void AssertStableReadinessRequiresBoundGateway(
        ContinuousLearningModelServerStartup startup,
        DesignConversionRetainedDpoPolicyBaseProviderGateway gateway)
    {
        if (startup.Readiness.StableDpoExecutorConfigured)
            Assert.True(gateway.IsBound);
        Assert.False(startup.IsLegalTrainingTeacherAdmissionActive());
    }

    private sealed class ThrowingDpoPolicyCheckpointSource(Exception exception)
        : IDesignConversionDpoPolicyCheckpointSource
    {
        public Task<OperationResult<Stream>> OpenCheckpointSnapshotAsync(
            CancellationToken cancellationToken)
        {
            _ = cancellationToken;
            return Task.FromException<OperationResult<Stream>>(exception);
        }
    }

    private static string ExtractMethodSource(string source, string signature)
    {
        var start = source.IndexOf(signature, StringComparison.Ordinal);
        if (start < 0 || source.IndexOf(
                signature,
                start + signature.Length,
                StringComparison.Ordinal) >= 0)
            throw new InvalidOperationException(
                $"Expected exactly one source method matching '{signature}'.");

        var bodyStart = source.IndexOf('{', start);
        if (bodyStart < 0)
            throw new InvalidOperationException(
                $"Could not locate the body for source method '{signature}'.");

        var depth = 0;
        for (var index = bodyStart; index < source.Length; index++)
        {
            if (source[index] == '{')
                depth++;
            else if (source[index] == '}' && --depth == 0)
                return source[start..(index + 1)];
        }

        throw new InvalidOperationException(
            $"Could not locate the end of source method '{signature}'.");
    }

    private static string NormalizeSource(string source)
        => string.Concat(source.Where(static character =>
            !char.IsWhiteSpace(character)));

    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority
        CreateRevocableAuthority(Func<bool> isAdmissionLive)
        => ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
            _ => OperationResult<LegalTrainingTeacherAdmission>.Ok(
                CreateLegalTeacherAdmission()),
            isAdmissionLive);

    private static ServerOwnedLegalTrainingTeacherAdmissionLease
        AssertSuccessfulLease(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority authority)
    {
        var result = authority.AcquireTrainingLease(
            [TrainingTeacherProvenance.Deterministic("revocable-authority")]);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return Assert.IsType<ServerOwnedLegalTrainingTeacherAdmissionLease>(
            result.Value);
    }

    private static LegalTrainingTeacherAdmission CreateLegalTeacherAdmission()
        => new(
            [],
            ExternalTeacherInvocationCount: 0,
            PolicyIdentityRef: "policy/revocable-authority",
            EvidenceRefs: []);

    private sealed class LeaseConcurrencyObserverForTesting(
        TimeSpan timeout)
        : IServerOwnedLegalTrainingTeacherAdmissionLeaseObserverForTesting,
          IDisposable
    {
        internal ManualResetEventSlim GrantPaused { get; } = new();
        internal ManualResetEventSlim DisposeAttempted { get; } = new();
        internal ManualResetEventSlim ReleaseGrant { get; } = new();

        public void OnFinalMutationPermitGrantUnderGateForTesting()
        {
            GrantPaused.Set();
            if (!ReleaseGrant.Wait(timeout))
            {
                throw new TimeoutException(
                    "Final permit grant release was not signaled.");
            }
        }

        public void OnMutationPermitActiveCountChangedForTesting(
            int activePermitCount)
        {
        }

        public void OnLeaseDisposeAttemptedBeforeGateForTesting()
            => DisposeAttempted.Set();

        public void Dispose()
        {
            GrantPaused.Dispose();
            DisposeAttempted.Dispose();
            ReleaseGrant.Dispose();
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException(
            "Could not locate the repository root from the test output directory.");
    }
}

namespace LlmFromScratch.Tests
{
    public sealed class ContinuousLearningModelServerCompositionTests
    {
        private const string RawTeacherPolicyBudgetAuthorityRole =
            "teacher-policy-call-budget-provenance-admission-never-dpo-mutation-or-lease-issuance";
        private const string ServerOwnedDpoTrainingLeaseAuthorityRole =
            "server-owned-dpo-training-lease-revocable-mutation-admission";

        [Fact]
        public void W154_ServerOwnedAuthorityRole_IsExplicitlyDistinctFromTeacherPolicyBudgetAuthority()
        {
            var rawType = typeof(LegalTrainingTeacherAdmissionAuthority);
            var serverOwnedType = typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority);

            var rawRole = ReadPublicConstRoleMarker(rawType);
            var serverOwnedRole = ReadPublicConstRoleMarker(serverOwnedType);

            Assert.Equal(RawTeacherPolicyBudgetAuthorityRole, rawRole);
            Assert.Equal(ServerOwnedDpoTrainingLeaseAuthorityRole, serverOwnedRole);
            Assert.NotEqual(rawRole, serverOwnedRole);
            Assert.Contains("teacher-policy", rawRole, StringComparison.Ordinal);
            Assert.Contains("call-budget", rawRole, StringComparison.Ordinal);
            Assert.Contains("provenance-admission", rawRole, StringComparison.Ordinal);
            Assert.Contains("never-dpo-mutation", rawRole, StringComparison.Ordinal);
            Assert.Contains("server-owned", serverOwnedRole, StringComparison.Ordinal);
            Assert.Contains("dpo-training-lease", serverOwnedRole, StringComparison.Ordinal);
            Assert.Contains(
                "revocable-mutation-admission",
                serverOwnedRole,
                StringComparison.Ordinal);

            Assert.False(typeof(ILogicalCapabilityTrainingAdmissionAuthority)
                .IsAssignableFrom(rawType));
            Assert.False(typeof(IServerOwnedDpoTrainingLeaseBroker)
                .IsAssignableFrom(rawType));
            Assert.DoesNotContain(
                rawType.GetInterfaces(),
                IsDpoMutationOrLeaseSurfaceType);
            Assert.DoesNotContain(
                PublicDeclaredMemberSurfaceTypes(rawType),
                IsDpoMutationOrLeaseSurfaceType);

            Assert.True(typeof(ILogicalCapabilityTrainingAdmissionAuthority)
                .IsAssignableFrom(serverOwnedType));
            Assert.True(typeof(IServerOwnedDpoTrainingLeaseBroker)
                .IsAssignableFrom(serverOwnedType));
            Assert.DoesNotContain(
                PublicDeclaredMemberSurfaceTypes(serverOwnedType),
                ContainsRawTeacherPolicyBudgetAuthority);
        }

        private static string ReadPublicConstRoleMarker(Type type)
        {
            var marker = type.GetField(
                "AuthorityRole",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            Assert.NotNull(marker);
            Assert.True(marker!.IsLiteral);
            Assert.False(marker.IsInitOnly);
            Assert.Equal(typeof(string), marker.FieldType);
            return Assert.IsType<string>(marker.GetRawConstantValue());
        }

        private static IEnumerable<Type> PublicDeclaredMemberSurfaceTypes(Type type)
        {
            const BindingFlags flags =
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.DeclaredOnly;

            foreach (var constructor in type.GetConstructors(flags))
            {
                foreach (var parameter in constructor.GetParameters())
                    yield return parameter.ParameterType;
            }

            foreach (var field in type.GetFields(flags))
                yield return field.FieldType;

            foreach (var property in type.GetProperties(flags))
                yield return property.PropertyType;

            foreach (var method in type.GetMethods(flags))
            {
                yield return method.ReturnType;
                foreach (var parameter in method.GetParameters())
                    yield return parameter.ParameterType;
            }
        }

        private static bool IsDpoMutationOrLeaseSurfaceType(Type type)
            => ContainsType(
                type,
                static candidate =>
                    candidate == typeof(ServerOwnedLegalTrainingTeacherAdmissionLease) ||
                    candidate == typeof(IServerOwnedDpoTrainingLeaseBroker) ||
                    candidate == typeof(ILogicalCapabilityTrainingAdmissionAuthority) ||
                    candidate == typeof(ILogicalCapabilityTrainingAdmissionLease) ||
                    candidate == typeof(ILogicalDpoMutationAdmissionIssuer) ||
                    candidate == typeof(IDpoMutationAdmissionIssuer) ||
                    typeof(ServerOwnedLegalTrainingTeacherAdmissionLease)
                        .IsAssignableFrom(candidate) ||
                    typeof(IServerOwnedDpoTrainingLeaseBroker)
                        .IsAssignableFrom(candidate) ||
                    typeof(ILogicalCapabilityTrainingAdmissionAuthority)
                        .IsAssignableFrom(candidate) ||
                    typeof(ILogicalCapabilityTrainingAdmissionLease)
                        .IsAssignableFrom(candidate) ||
                    typeof(ILogicalDpoMutationAdmissionIssuer)
                        .IsAssignableFrom(candidate) ||
                    typeof(IDpoMutationAdmissionIssuer).IsAssignableFrom(candidate));

        private static bool ContainsRawTeacherPolicyBudgetAuthority(Type type)
            => ContainsType(
                type,
                static candidate =>
                    candidate == typeof(LegalTrainingTeacherAdmissionAuthority) ||
                    typeof(LegalTrainingTeacherAdmissionAuthority)
                        .IsAssignableFrom(candidate));

        private static bool ContainsType(Type type, Func<Type, bool> predicate)
        {
            if (type.IsByRef || type.IsPointer || type.IsArray)
                return ContainsType(type.GetElementType()!, predicate);
            if (predicate(type))
                return true;
            return type.IsGenericType &&
                   type.GetGenericArguments().Any(argument =>
                       ContainsType(argument, predicate));
        }
    }
}

file sealed class ModelServerCompositionFixture : IDisposable
{
    private readonly ModelConfig _modelConfig;

    public ModelServerCompositionFixture(ModelConfig? modelConfig = null)
    {
        Root = Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-modelserver-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Root);
        BaseCheckpointPath = Path.Combine(Root, "base-checkpoint.bin");
        TokenizerPath = Path.Combine(Root, "tokenizer.bin");
        _modelConfig = modelConfig ?? CreateTestModelConfig();
        new LlmModel(_modelConfig).SaveCheckpoint(BaseCheckpointPath);
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("continuous learning modelserver fixture", _modelConfig.VocabSize);
        tokenizer.Save(TokenizerPath);
        AdapterRoot = Path.Combine(Root, "adapter-root");
        CacheRoot = Path.Combine(Root, "cache-root");
        RegistryRoot = Path.Combine(Root, "registry-root");
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
    }

    public string Root { get; }
    public string BaseCheckpointPath { get; }
    public string TokenizerPath { get; }
    public string AdapterRoot { get; }
    public string CacheRoot { get; }
    public string RegistryRoot { get; }
    public string RepositoryRoot { get; }
    public string PairRoot { get; }
    public ModelConfig ModelConfiguration => _modelConfig;

    public Dictionary<string, string> ValidEnvironment()
    {
        var environment = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ContinuousLearningModelServerConfigurationLoader.EnabledEnvironmentName]
                = bool.TrueString,
            [ContinuousLearningModelServerConfigurationLoader.ModelIdEnvironmentName]
                = "qwen3.5:9b",
            [ContinuousLearningModelServerConfigurationLoader.OllamaManifestDigestEnvironmentName]
                = new string('d', 64),
            [ContinuousLearningModelServerConfigurationLoader.OllamaModelLayerDigestEnvironmentName]
                = new string('c', 64),
            [ContinuousLearningModelServerConfigurationLoader.BaseModelIdEnvironmentName]
                = "registry/qwen3.5-base-9b",
            [ContinuousLearningModelServerConfigurationLoader.BaseModelRevisionEnvironmentName]
                = "base-checkpoint-r17",
            [ContinuousLearningModelServerConfigurationLoader.MaxTeacherCallsEnvironmentName]
                = "100",
            [ContinuousLearningModelServerConfigurationLoader.AuditSaltEnvironmentName]
                = Convert.ToBase64String(Enumerable.Range(1, 32)
                    .Select(static value => checked((byte)value))
                    .ToArray()),
            [ContinuousLearningModelServerConfigurationLoader.LocalRetentionPolicyRefEnvironmentName]
                = $"sha256:{new string('a', 64)}",
            [ContinuousLearningModelServerConfigurationLoader.OutputLicenseRefEnvironmentName]
                = $"sha256:{new string('b', 64)}",
            [ContinuousLearningModelServerConfigurationLoader.BaseCheckpointEnvironmentName]
                = BaseCheckpointPath,
            [ContinuousLearningModelServerConfigurationLoader.TokenizerEnvironmentName]
                = TokenizerPath,
            [ContinuousLearningModelServerConfigurationLoader.AdapterRootEnvironmentName]
                = AdapterRoot,
            [ContinuousLearningModelServerConfigurationLoader.CacheRootEnvironmentName]
                = CacheRoot,
            [ContinuousLearningModelServerConfigurationLoader.RegistryRootEnvironmentName]
                = RegistryRoot,
            [ContinuousLearningModelServerConfigurationLoader.RepositoryRootEnvironmentName]
                = RepositoryRoot,
            [ContinuousLearningModelServerConfigurationLoader.DpoPairRootEnvironmentName]
                = PairRoot,
            [ContinuousLearningModelServerConfigurationLoader.RepositoryModelIdEnvironmentName]
                = "qwen3-5-9b-dpo",
            [ContinuousLearningModelServerConfigurationLoader.RepositoryModelVersionEnvironmentName]
                = "v0001",
            [ContinuousLearningModelServerConfigurationLoader.RuntimeConfigSha256EnvironmentName]
                = new string('1', 64),
            [ContinuousLearningModelServerConfigurationLoader.RuntimeTopologySha256EnvironmentName]
                = new string('2', 64),
            [ContinuousLearningModelServerConfigurationLoader.RuntimeTokenizerSha256EnvironmentName]
                = new string('3', 64),
            [ContinuousLearningModelServerConfigurationLoader.RuntimeTrainerSha256EnvironmentName]
                = new string('4', 64),
            [ContinuousLearningModelServerConfigurationLoader.RuntimeSchemaSha256EnvironmentName]
                = new string('5', 64),
            [ContinuousLearningModelServerConfigurationLoader.RuntimeCompatibilityKeyEnvironmentName]
                = "tp1-dpo-runtime-v0001",
        };
        AddExplicitModelAndTrainingConfiguration(environment, _modelConfig);
        return environment;
    }

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

    public string[] Snapshot()
        => Directory
            .EnumerateFileSystemEntries(Root, "*", SearchOption.AllDirectories)
            .Select(path =>
            {
                var relative = Path.GetRelativePath(Root, path);
                return File.Exists(path)
                    ? $"F|{relative}|{new FileInfo(path).Length}"
                    : $"D|{relative}";
            })
            .Order(StringComparer.Ordinal)
            .ToArray();

    public void AssertNoOutputDirectories()
    {
        Assert.False(Directory.Exists(AdapterRoot));
        Assert.False(Directory.Exists(CacheRoot));
        Assert.False(Directory.Exists(RegistryRoot));
        Assert.False(Directory.Exists(PairRoot));
    }

    public void CreateOutputDirectories()
    {
        Directory.CreateDirectory(AdapterRoot);
        Directory.CreateDirectory(CacheRoot);
        Directory.CreateDirectory(RegistryRoot);
        Directory.CreateDirectory(PairRoot);
    }

    private static void CreateRepositorySentinels(string repositoryRoot)
    {
        Directory.CreateDirectory(repositoryRoot);
        File.WriteAllText(
            Path.Combine(repositoryRoot, "LlmFromScratch.sln"),
            "fixture solution sentinel");
        Directory.CreateDirectory(Path.Combine(repositoryRoot, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(repositoryRoot, "11.DPO"));
    }

    public void Dispose()
    {
        if (Directory.Exists(Root))
            Directory.Delete(Root, recursive: true);
    }
}


public sealed partial class ContinuousLearningModelServerCompositionTests
{
    private const string TestTrainingConfigurationFingerprint =
        "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";


    [Theory]
    [InlineData("True")]
    [InlineData(" true ")]
    [InlineData("pending")]
    public void DisabledComposition_TrueOrMalformedTrainingActivationIsInvalid(
        string trainingActivation)
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        environment[ContinuousLearningModelServerConfigurationLoader
            .EnabledEnvironmentName] = bool.FalseString;
        environment[ContinuousLearningModelServerConfigurationLoader
            .TrainingActivationEnabledEnvironmentName] = trainingActivation;

        var startup = CreateStartup(environment);

        Assert.False(startup.HasDeferredCompositionPlan);
        Assert.False(startup.Readiness.ConfigurationAdmitted);
        Assert.Equal("invalid", startup.Readiness.ConfigurationStatus);
        Assert.Equal(
            "CONTINUOUS_LEARNING_CONFIGURATION_INVALID",
            startup.Readiness.ConfigurationErrorCode);
        AssertClosedAndUnmaterialized(startup.Readiness);
    }


    [Fact]
    public async Task HostedProviderActivation_TransportsOnlyAdmittedRootSeedAndFingerprint()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var environment = fixture.ValidEnvironment();
        RequestTrainingActivation(environment);
        var startup = CreateStartup(environment);
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var admitted = startup.CaptureDpoPolicyBaseBindingConfiguration(access);
        environment[ContinuousLearningModelServerConfigurationLoader
            .TrainingRootInitializationSeedEnvironmentName] = ulong.MaxValue.ToString();
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var activator = new ContinuousLearningRetainedObjectHostedActivator(
            access,
            gateway,
            startup);
        DesignConversionCheckpointBackedDpoPolicyBaseProvider? observed = null;

        await activator.ActivatePolicyBaseProviderAsync(
            CancellationToken.None,
            provider => observed = provider);

        var provider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(observed);
        Assert.Equal(
            admitted.RootInitializationSeed,
            provider.ProviderOptions!.RootInitializationSeed);
        Assert.Equal(
            admitted.TrainingConfigurationFingerprint,
            provider.ProviderOptions.TrainingConfigurationFingerprint);
        Assert.Equal(0UL, provider.ProviderOptions.RootInitializationSeed);
        Assert.True(
            DesignConversionDpoPolicyRandomContextContract.MatchesCurrentAuthority(
                provider.CurrentRandomContext,
                admitted.RootInitializationSeed!.Value,
                admitted.TrainingConfigurationFingerprint));
        Assert.True(gateway.IsBound);
    }


    [Fact]
    public async Task RetainedDpoPolicyBinding_RootSeedZeroRetainsExactAuthority()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();

        var result = await DesignConversionRetainedDpoPolicyBaseComposition
            .CreateInactiveProviderAsync(
                access,
                fixture.ModelConfiguration,
                RetainedPolicyModelId,
                RetainedPolicyRevision,
                rootInitializationSeed: 0,
                TestTrainingConfigurationFingerprint);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var provider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(result.Value);
        Assert.Equal(
            new DesignConversionDpoPolicyBaseProviderOptions(
                0,
                TestTrainingConfigurationFingerprint),
            provider.ProviderOptions);
        Assert.True(
            DesignConversionDpoPolicyRandomContextContract.MatchesCurrentAuthority(
                provider.CurrentRandomContext,
                0,
                TestTrainingConfigurationFingerprint));
        Assert.Equal(0UL, provider.CurrentRandomContext!.RootInitializationSeed);
    }


    [Fact]
    public async Task GatewayUnbind_CompletesThenAcceptsFreshExactStartupBinding()
    {
        using var firstFixture = new ModelServerCompositionFixture();
        using var secondFixture = new ModelServerCompositionFixture();
        firstFixture.CreateOutputDirectories();
        secondFixture.CreateOutputDirectories();
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var firstStartup = CreateStartup(firstFixture.ValidEnvironment());
        using var firstAccess = firstStartup.AcquireRetainedFileAccess();
        firstStartup.BindRetainedFileAccess(firstAccess);
        firstStartup.MaterializeInactiveTp1Composition(firstAccess);
        await BindExactDpoPolicyBaseProviderAsync(
            gateway,
            firstStartup,
            firstAccess);

        gateway.Unbind();
        var firstRetirement = gateway.AwaitRetirementAsync();
        await firstRetirement.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(firstRetirement.IsCompletedSuccessfully);
        Assert.False(gateway.IsBound);

        var secondStartup = CreateStartup(secondFixture.ValidEnvironment());
        using var secondAccess = secondStartup.AcquireRetainedFileAccess();
        secondStartup.BindRetainedFileAccess(secondAccess);
        secondStartup.MaterializeInactiveTp1Composition(secondAccess);
        await BindExactDpoPolicyBaseProviderAsync(
            gateway,
            secondStartup,
            secondAccess);

        Assert.True(gateway.IsBound);
        Assert.True(secondStartup.Readiness.StableDpoExecutorConfigured);
        gateway.Close();
        await gateway.AwaitRetirementAsync()
            .WaitAsync(TimeSpan.FromSeconds(5));
        Assert.False(gateway.IsBound);
    }


    [Fact]
    public async Task GatewayConcurrentCloseUpgradeMakesUnbindPermanentlyRejectFreshBinding()
    {
        using var firstFixture = new ModelServerCompositionFixture();
        using var secondFixture = new ModelServerCompositionFixture();
        firstFixture.CreateOutputDirectories();
        secondFixture.CreateOutputDirectories();
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var firstStartup = CreateStartup(firstFixture.ValidEnvironment());
        using var firstAccess = firstStartup.AcquireRetainedFileAccess();
        firstStartup.BindRetainedFileAccess(firstAccess);
        firstStartup.MaterializeInactiveTp1Composition(firstAccess);
        await BindExactDpoPolicyBaseProviderAsync(
            gateway,
            firstStartup,
            firstAccess);
        var phaseOneEntered = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releasePhaseOne = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        Task? unbind = null;
        try
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ReadinessWithdrawnBeforeBindingRemovedObserverForTesting =
                () =>
                {
                    phaseOneEntered.TrySetResult(null);
                    releasePhaseOne.Task.GetAwaiter().GetResult();
                };
            unbind = Task.Run(gateway.Unbind);
            await phaseOneEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            gateway.Close();
            releasePhaseOne.TrySetResult(null);
            await unbind.WaitAsync(TimeSpan.FromSeconds(5));
            await gateway.AwaitRetirementAsync()
                .WaitAsync(TimeSpan.FromSeconds(5));

            var secondStartup = CreateStartup(
                secondFixture.ValidEnvironment());
            using var secondAccess =
                secondStartup.AcquireRetainedFileAccess();
            secondStartup.BindRetainedFileAccess(secondAccess);
            secondStartup.MaterializeInactiveTp1Composition(secondAccess);
            var configuration = secondStartup
                .CaptureDpoPolicyBaseBindingConfiguration(secondAccess);
            var providerResult = await
                DesignConversionRetainedDpoPolicyBaseComposition
                    .CreateInactiveProviderAsync(
                        secondAccess,
                        configuration.ModelConfig,
                        configuration.ModelId,
                        configuration.ModelVersion,
                        rootInitializationSeed: null,
                        TestTrainingConfigurationFingerprint);
            Assert.True(providerResult.IsSuccess, providerResult.ErrorMessage);
            var provider = Assert.IsType<
                DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                    providerResult.Value);

            var rebound = gateway.BindExactProvider(
                provider,
                secondStartup,
                secondAccess,
                CancellationToken.None);

            Assert.False(rebound.IsSuccess);
            Assert.Equal(
                DesignConversionRetainedDpoPolicyBaseProviderGateway
                    .ProviderUnavailableErrorCode,
                rebound.ErrorCode);
            Assert.False(gateway.IsBound);
            Assert.False(
                secondStartup.Readiness.StableDpoExecutorConfigured);
        }
        finally
        {
            releasePhaseOne.TrySetResult(null);
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ReadinessWithdrawnBeforeBindingRemovedObserverForTesting =
                null;
            if (unbind is not null)
                await unbind.WaitAsync(TimeSpan.FromSeconds(5));
        }
    }


    [Fact]
    public async Task GatewayWithdrawal_OrdinaryUnbindAllowsRebindAfterRetirement()
    {
        using var firstFixture = new ModelServerCompositionFixture();
        using var secondFixture = new ModelServerCompositionFixture();
        firstFixture.CreateOutputDirectories();
        secondFixture.CreateOutputDirectories();
        var firstStartup = CreateStartup(firstFixture.ValidEnvironment());
        using var firstAccess = firstStartup.AcquireRetainedFileAccess();
        firstStartup.BindRetainedFileAccess(firstAccess);
        firstStartup.MaterializeInactiveTp1Composition(firstAccess);
        var firstConfiguration =
            firstStartup.CaptureDpoPolicyBaseBindingConfiguration(firstAccess);
        var firstProvider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
            (await DesignConversionRetainedDpoPolicyBaseComposition
                .CreateInactiveProviderAsync(
                    firstAccess,
                    firstConfiguration.ModelConfig,
                    firstConfiguration.ModelId,
                    firstConfiguration.ModelVersion,
                    rootInitializationSeed: null,
                    TestTrainingConfigurationFingerprint)).Value);
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var firstBinding = gateway.BindExactProvider(
            firstProvider,
            firstStartup,
            firstAccess,
            CancellationToken.None);
        Assert.True(firstBinding.IsSuccess, firstBinding.ErrorMessage);
        Assert.True(firstStartup.TryMarkDpoPolicyBaseProviderBound(
            firstAccess,
            firstBinding.Value!));

        gateway.Unbind();
        await Task.WhenAll(
                firstStartup.AwaitRetirementAsync(),
                gateway.AwaitRetirementAsync())
            .WaitAsync(TimeSpan.FromSeconds(5));
        Assert.False(gateway.IsBound);

        var secondStartup = CreateStartup(secondFixture.ValidEnvironment());
        using var secondAccess = secondStartup.AcquireRetainedFileAccess();
        secondStartup.BindRetainedFileAccess(secondAccess);
        secondStartup.MaterializeInactiveTp1Composition(secondAccess);
        var secondConfiguration =
            secondStartup.CaptureDpoPolicyBaseBindingConfiguration(secondAccess);
        var secondProvider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
            (await DesignConversionRetainedDpoPolicyBaseComposition
                .CreateInactiveProviderAsync(
                    secondAccess,
                    secondConfiguration.ModelConfig,
                    secondConfiguration.ModelId,
                    secondConfiguration.ModelVersion,
                    rootInitializationSeed: null,
                    TestTrainingConfigurationFingerprint)).Value);
        var rebound = gateway.BindExactProvider(
            secondProvider,
            secondStartup,
            secondAccess,
            CancellationToken.None);

        Assert.True(rebound.IsSuccess, rebound.ErrorMessage);
        Assert.True(gateway.IsBound);
        gateway.Close();
        await Task.WhenAll(
                secondStartup.AwaitRetirementAsync(),
                gateway.AwaitRetirementAsync())
            .WaitAsync(TimeSpan.FromSeconds(5));
    }


    [Fact]
    public async Task RetainedDpoPolicyBinding_InvalidAuthorityFailsBeforeDisposedSources()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        var access = startup.AcquireRetainedFileAccess();
        access.Dispose();
        var invalidFingerprints = new string?[]
        {
            TestTrainingConfigurationFingerprint.ToUpperInvariant(),
            TestTrainingConfigurationFingerprint[..^1],
            TestTrainingConfigurationFingerprint[..^1] + "g",
            " " + TestTrainingConfigurationFingerprint[..^1],
            null
        };

        foreach (var fingerprint in invalidFingerprints)
        {
            var result = await DesignConversionRetainedDpoPolicyBaseComposition
                .CreateInactiveProviderAsync(
                    access,
                    fixture.ModelConfiguration,
                    RetainedPolicyModelId,
                    RetainedPolicyRevision,
                    rootInitializationSeed: 0,
                    fingerprint!);

            AssertFailure(
                result,
                DesignConversionCheckpointBackedDpoPolicyBaseProvider
                    .ConfigurationInvalidErrorCode,
                "DesignConversion DPO checkpoint-backed policy base configuration is invalid.");
            Assert.DoesNotContain(
                fixture.Root,
                result.ErrorMessage,
                StringComparison.OrdinalIgnoreCase);
        }
    }


    [Fact]
    public async Task ProviderPromotion_TrustLossRevocationCannotInvertGatewayAndReadinessLocks()
    {
        using var fixture = new ModelServerCompositionFixture();
        using var rebindFixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        rebindFixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var configuration =
            startup.CaptureDpoPolicyBaseBindingConfiguration(access);
        var providerResult = await DesignConversionRetainedDpoPolicyBaseComposition
            .CreateInactiveProviderAsync(
                access,
                configuration.ModelConfig,
                configuration.ModelId,
                configuration.ModelVersion,
                rootInitializationSeed: null,
                TestTrainingConfigurationFingerprint);
        var provider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                providerResult.Value);
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var bound = gateway.BindExactProvider(
            provider,
            startup,
            access,
            CancellationToken.None);
        Assert.True(bound.IsSuccess, bound.ErrorMessage);
        var bindingToken = Assert.IsType<
            ContinuousLearningDpoPolicyBaseBindingToken>(bound.Value);

        var gatewayPhaseEntered = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseGatewayPhase = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var trustLossRevocationEntered = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        Task? withdrawal = null;
        Task<bool>? repeatedPromotion = null;
        var priorObserver =
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeStartupReadinessWithdrawalObserverForTesting;
        try
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeStartupReadinessWithdrawalObserverForTesting = () =>
                {
                    gatewayPhaseEntered.TrySetResult(null);
                    releaseGatewayPhase.Task.GetAwaiter().GetResult();
                };
            withdrawal = Task.Factory.StartNew(
                gateway.Unbind,
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            await gatewayPhaseEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));

            repeatedPromotion = Task.Factory.StartNew(
                () => startup.TryMarkDpoPolicyBaseProviderBound(
                    access,
                    bindingToken,
                    afterReadinessPublicationBeforeRetainedRevalidationForTesting:
                    () =>
                    {
                        trustLossRevocationEntered.TrySetResult(null);
                        bindingToken.RevokeAfterTrustLoss();
                    }),
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            await trustLossRevocationEntered.Task.WaitAsync(
                TimeSpan.FromSeconds(5));
            Assert.False(repeatedPromotion.IsCompleted);
            Assert.True(gateway.IsBound);

            releaseGatewayPhase.TrySetResult(null);
            await withdrawal.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(await repeatedPromotion.WaitAsync(
                TimeSpan.FromSeconds(5)));
            Assert.False(gateway.IsBound);
            await Task.WhenAll(
                    startup.AwaitRetirementAsync(),
                    gateway.AwaitRetirementAsync())
                .WaitAsync(TimeSpan.FromSeconds(5));

            var rebindStartup = CreateStartup(rebindFixture.ValidEnvironment());
            using var rebindAccess = rebindStartup.AcquireRetainedFileAccess();
            rebindStartup.BindRetainedFileAccess(rebindAccess);
            rebindStartup.MaterializeInactiveTp1Composition(rebindAccess);
            var rebindConfiguration =
                rebindStartup.CaptureDpoPolicyBaseBindingConfiguration(
                    rebindAccess);
            var rebindProvider = Assert.IsType<
                DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                (await DesignConversionRetainedDpoPolicyBaseComposition
                    .CreateInactiveProviderAsync(
                        rebindAccess,
                        rebindConfiguration.ModelConfig,
                        rebindConfiguration.ModelId,
                        rebindConfiguration.ModelVersion,
                        rootInitializationSeed: null,
                        TestTrainingConfigurationFingerprint)).Value);
            var rejectedRebind = gateway.BindExactProvider(
                rebindProvider,
                rebindStartup,
                rebindAccess,
                CancellationToken.None);
            AssertFailure(
                rejectedRebind,
                DesignConversionRetainedDpoPolicyBaseProviderGateway
                    .ProviderUnavailableErrorCode,
                DesignConversionRetainedDpoPolicyBaseProviderGateway
                    .ProviderUnavailableErrorMessage);
        }
        finally
        {
            releaseGatewayPhase.TrySetResult(null);
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .BeforeStartupReadinessWithdrawalObserverForTesting =
                priorObserver;
            if (withdrawal is not null)
                await withdrawal.WaitAsync(TimeSpan.FromSeconds(5));
            if (repeatedPromotion is not null)
                await repeatedPromotion.WaitAsync(TimeSpan.FromSeconds(5));
        }
    }


    [Fact]
    public async Task HostedUnbindBetweenGatewayBindAndReadinessPromotionFailsExactPendingStartup()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var activator = new ContinuousLearningRetainedObjectHostedActivator(
            access,
            gateway,
            startup);
        var withdrawalPhaseObserved = false;
        var retirementStartObserved = false;
        Task? startupRetirementDuringPhaseOne = null;
        try
        {
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
                    withdrawalPhaseObserved = true;
                    Assert.True(gateway.IsBound);
                    Assert.False(startup.Readiness.StableDpoExecutorConfigured);
                    Assert.False(startup.Readiness.TrainingEnabled);
                    startupRetirementDuringPhaseOne =
                        startup.AwaitRetirementAsync();
                    Assert.False(startupRetirementDuringPhaseOne.IsCompleted);
                    Assert.False(retirementStartObserved);
                };
            await activator.ActivatePolicyBaseProviderAsync(
                CancellationToken.None,
                afterProviderCreationBeforeBindingForTesting: null,
                afterGatewayBindingBeforeReadinessPromotionForTesting: () =>
                {
                    Assert.True(gateway.IsBound);
                    Assert.False(startup.Readiness.StableDpoExecutorConfigured);
                    gateway.Unbind();
                    Assert.False(gateway.IsBound);
                    Assert.False(startup.Readiness.StableDpoExecutorConfigured);
                });
        }
        finally
        {
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = null;
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ReadinessWithdrawnBeforeBindingRemovedObserverForTesting =
                null;
        }

        Assert.True(withdrawalPhaseObserved);
        Assert.True(retirementStartObserved);
        Assert.NotNull(startupRetirementDuringPhaseOne);
        Assert.Same(
            startupRetirementDuringPhaseOne,
            startup.AwaitRetirementAsync());
        Assert.False(gateway.IsBound);
        Assert.Equal(
            ContinuousLearningModelServerStartup
                .DpoPolicyBaseBindingFailedErrorCode,
            startup.Readiness.ConfigurationErrorCode);
        Assert.Equal(
            "dpo_policy_base_binding_failed_inactive",
            startup.Readiness.ConfigurationStatus);
        AssertClosedAndUnmaterialized(startup.Readiness);
        await startup.AwaitRetirementAsync()
            .WaitAsync(TimeSpan.FromSeconds(5));
        await gateway.AwaitRetirementAsync()
            .WaitAsync(TimeSpan.FromSeconds(5));
    }


    [Fact]
    public async Task ReadinessPublication_NegativeAllocationFailureLeavesLifecycleAndRetirementRetryable()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var activeReadiness = startup.Readiness;
        var initialRetirement = startup.AwaitRetirementAsync();
        var fatal = new OutOfMemoryException(
            "fatal-before-negative-readiness-allocation");
        var prior = ContinuousLearningModelServerStartup
            .BeforeReadinessSnapshotAllocationObserverForTesting;
        try
        {
            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = () =>
                throw fatal;

            var thrown = Assert.Throws<OutOfMemoryException>(
                startup.MarkDpoPolicyBaseStopped);

            Assert.Same(fatal, thrown);
            Assert.Equal(activeReadiness, startup.Readiness);
            Assert.Same(initialRetirement, startup.AwaitRetirementAsync());
            Assert.True(initialRetirement.IsCompletedSuccessfully);
            Assert.True(startup.TryRegisterRetirementDependency(
                static () => Task.CompletedTask));
            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = null;
            startup.MarkDpoPolicyBaseStopped();
            await startup.AwaitRetirementAsync()
                .WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(startup.Readiness.CompositionMaterialized);
        }
        finally
        {
            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = null;
            if (startup.Readiness.CompositionMaterialized)
            {
                startup.MarkDpoPolicyBaseStopped();
                await startup.AwaitRetirementAsync()
                    .WaitAsync(TimeSpan.FromSeconds(5));
            }
            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = prior;
        }
    }


    [Fact]
    public async Task ReadinessPublication_PositiveAllocationFailureLeavesBindAndProviderPromotionRetryable()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        var fatalBind = new OutOfMemoryException(
            "fatal-before-bound-readiness-allocation");
        var fatalPromotion = new OutOfMemoryException(
            "fatal-before-provider-bound-readiness-allocation");
        var prior = ContinuousLearningModelServerStartup
            .BeforeReadinessSnapshotAllocationObserverForTesting;
        DesignConversionRetainedDpoPolicyBaseProviderGateway? gateway = null;
        try
        {
            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = () =>
                throw fatalBind;
            var bindThrown = Assert.Throws<OutOfMemoryException>(() =>
                startup.BindRetainedFileAccess(access));
            Assert.Same(fatalBind, bindThrown);
            Assert.False(startup.Readiness.RetainedObjectIdentityBound);

            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = null;
            startup.BindRetainedFileAccess(access);
            startup.MaterializeInactiveTp1Composition(access);
            var configuration =
                startup.CaptureDpoPolicyBaseBindingConfiguration(access);
            var provider = Assert.IsType<
                DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                (await DesignConversionRetainedDpoPolicyBaseComposition
                    .CreateInactiveProviderAsync(
                        access,
                        configuration.ModelConfig,
                        configuration.ModelId,
                        configuration.ModelVersion,
                        rootInitializationSeed: null,
                        TestTrainingConfigurationFingerprint)).Value);
            gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
            var bound = gateway.BindExactProvider(
                provider,
                startup,
                access,
                CancellationToken.None);
            Assert.True(bound.IsSuccess, bound.ErrorMessage);
            var bindingToken = Assert.IsType<
                ContinuousLearningDpoPolicyBaseBindingToken>(bound.Value);

            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = () =>
                throw fatalPromotion;
            var promotionThrown = Assert.Throws<OutOfMemoryException>(() =>
                startup.TryMarkDpoPolicyBaseProviderBound(
                    access,
                    bindingToken));
            Assert.Same(fatalPromotion, promotionThrown);
            Assert.False(startup.Readiness.StableDpoExecutorConfigured);
            Assert.True(gateway.IsBound);

            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = null;
            Assert.True(startup.TryMarkDpoPolicyBaseProviderBound(
                access,
                bindingToken));
            Assert.True(startup.Readiness.StableDpoExecutorConfigured);
        }
        finally
        {
            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = null;
            gateway?.Close();
            startup.MarkDpoPolicyBaseStopped();
            await startup.AwaitRetirementAsync()
                .WaitAsync(TimeSpan.FromSeconds(5));
            if (gateway is not null)
            {
                await gateway.AwaitRetirementAsync()
                    .WaitAsync(TimeSpan.FromSeconds(5));
            }
            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = prior;
        }
    }


    [Fact]
    public async Task StartupRetirement_PendingCompletionAllocationFailureLeavesCompositionRetryable()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var initialRetirement = startup.AwaitRetirementAsync();
        var fatal = new OutOfMemoryException(
            "fatal-before-startup-retirement-pending-completion-allocation");
        var prior = ContinuousLearningModelServerStartup
            .BeforeRetirementPendingCompletionAllocationObserverForTesting;
        try
        {
            ContinuousLearningModelServerStartup
                .BeforeRetirementPendingCompletionAllocationObserverForTesting =
                () => throw fatal;

            var thrown = Assert.Throws<OutOfMemoryException>(
                startup.MarkDpoPolicyBaseStopped);

            Assert.Same(fatal, thrown);
            Assert.True(startup.Readiness.CompositionMaterialized);
            Assert.Same(initialRetirement, startup.AwaitRetirementAsync());
            Assert.True(initialRetirement.IsCompletedSuccessfully);
            ContinuousLearningModelServerStartup
                .BeforeRetirementPendingCompletionAllocationObserverForTesting =
                null;
            startup.MarkDpoPolicyBaseStopped();
            await startup.AwaitRetirementAsync()
                .WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(startup.Readiness.CompositionMaterialized);
        }
        finally
        {
            ContinuousLearningModelServerStartup
                .BeforeRetirementPendingCompletionAllocationObserverForTesting =
                null;
            if (startup.Readiness.CompositionMaterialized)
            {
                startup.MarkDpoPolicyBaseStopped();
                await startup.AwaitRetirementAsync()
                    .WaitAsync(TimeSpan.FromSeconds(5));
            }
            ContinuousLearningModelServerStartup
                .BeforeRetirementPendingCompletionAllocationObserverForTesting =
                prior;
        }
    }


    [Theory]
    [InlineData("dependency-snapshot")]
    [InlineData("failure-state")]
    [InlineData("completion-launch")]
    public async Task StartupRetirement_StartPreparationFailureCannotCommitStartFlag(
        string boundary)
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var startup = CreateStartup(fixture.ValidEnvironment());
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var dependencyInvocations = 0;
        Assert.True(startup.TryRegisterRetirementDependency(() =>
        {
            dependencyInvocations++;
            return Task.CompletedTask;
        }));
        var fatal = new OutOfMemoryException(
            $"fatal-during-startup-retirement-{boundary}");
        var priorSnapshotObserver = ContinuousLearningModelServerStartup
            .BeforeRetirementDependencySnapshotObserverForTesting;
        var priorFailureObserver = ContinuousLearningModelServerStartup
            .BeforeRetirementFailureStateAllocationObserverForTesting;
        var priorLaunchObserver = ContinuousLearningModelServerStartup
            .BeforeRetirementCompletionLaunchObserverForTesting;
        try
        {
            if (boundary == "dependency-snapshot")
            {
                ContinuousLearningModelServerStartup
                    .BeforeRetirementDependencySnapshotObserverForTesting = () =>
                    throw fatal;
            }
            else if (boundary == "failure-state")
            {
                ContinuousLearningModelServerStartup
                    .BeforeRetirementFailureStateAllocationObserverForTesting =
                    () => throw fatal;
            }
            else
            {
                ContinuousLearningModelServerStartup
                    .BeforeRetirementCompletionLaunchObserverForTesting = () =>
                    throw fatal;
            }

            var thrown = Assert.Throws<OutOfMemoryException>(
                startup.MarkDpoPolicyBaseStopped);

            Assert.Same(fatal, thrown);
            Assert.True(startup.Readiness.CompositionMaterialized);
            Assert.True(startup.AwaitRetirementAsync().IsCompletedSuccessfully);
            Assert.True(startup.TryRegisterRetirementDependency(() =>
            {
                dependencyInvocations++;
                return Task.CompletedTask;
            }));
            ClearStartupRetirementPreparationObservers();
            startup.MarkDpoPolicyBaseStopped();
            await startup.AwaitRetirementAsync()
                .WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(2, dependencyInvocations);
        }
        finally
        {
            ContinuousLearningModelServerStartup
                .BeforeRetirementDependencySnapshotObserverForTesting = null;
            ContinuousLearningModelServerStartup
                .BeforeRetirementFailureStateAllocationObserverForTesting = null;
            ContinuousLearningModelServerStartup
                .BeforeRetirementCompletionLaunchObserverForTesting = null;
            if (startup.Readiness.CompositionMaterialized)
            {
                startup.MarkDpoPolicyBaseStopped();
                await startup.AwaitRetirementAsync()
                    .WaitAsync(TimeSpan.FromSeconds(5));
            }
            ContinuousLearningModelServerStartup
                .BeforeRetirementDependencySnapshotObserverForTesting =
                priorSnapshotObserver;
            ContinuousLearningModelServerStartup
                .BeforeRetirementFailureStateAllocationObserverForTesting =
                priorFailureObserver;
            ContinuousLearningModelServerStartup
                .BeforeRetirementCompletionLaunchObserverForTesting =
                priorLaunchObserver;
        }
    }


    [Fact]
    public async Task RetainedDpoPolicyActivation_DisposalPrecommitFailureDoesNotHangAndRetryStartsFreshAttempt()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var environment = fixture.ValidEnvironment();
        var resolution = await DesignConversionRetainedDpoPolicyBaseActivation
            .ActivateAsync(
                name => environment.TryGetValue(name, out var value)
                    ? value
                    : null);
        var lease = Assert.IsType<
            DesignConversionRetainedDpoPolicyBaseActivationLease>(
                resolution.Lease);
        var gateway = Assert.IsType<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>(
                lease.Provider);
        var fatal = new OutOfMemoryException(
            "activation-lease-disposal-readiness-allocation");
        var prior = ContinuousLearningModelServerStartup
            .BeforeReadinessSnapshotAllocationObserverForTesting;
        try
        {
            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = () =>
                throw fatal;

            var failedAttempt = lease.DisposeAsync().AsTask();
            var thrown = await Assert.ThrowsAsync<OutOfMemoryException>(
                () => failedAttempt.WaitAsync(TimeSpan.FromSeconds(5)));

            Assert.Same(fatal, thrown);
            Assert.True(failedAttempt.IsCompleted);
            Assert.True(gateway.IsBound);

            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = null;
            var retry = lease.DisposeAsync().AsTask();

            Assert.NotSame(failedAttempt, retry);
            await retry.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(gateway.IsBound);
            Assert.Same(retry, lease.DisposeAsync().AsTask());
        }
        finally
        {
            ContinuousLearningModelServerStartup
                .BeforeReadinessSnapshotAllocationObserverForTesting = null;
            try { await lease.DisposeAsync(); }
            finally
            {
                ContinuousLearningModelServerStartup
                    .BeforeReadinessSnapshotAllocationObserverForTesting = prior;
            }
        }

        AssertRetainedPathsReleasedAndDeleteRoot(fixture.Root);
    }


    [Fact]
    public async Task RetainedDpoPolicyActivation_FatalAfterGatewayBindingRemovesBindingBeforeRetirementStarts()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var environment = fixture.ValidEnvironment();
        var fatal = new OutOfMemoryException(
            "fatal-after-gateway-binding-before-readiness-promotion");
        DesignConversionRetainedDpoPolicyBaseProviderGateway? gateway = null;
        Task? publishedRetirement = null;
        var withdrawalPhaseObserved = false;
        var retirementStartObserved = false;
        var phaseObservedRetirementAlreadyStarted = false;
        var retirementStartObservedBindingPresent = false;
        var priorRetirementObserver = ContinuousLearningModelServerStartup
            .RetirementTaskPublishedObserverForTesting;
        var priorWithdrawalObserver =
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ReadinessWithdrawnBeforeBindingRemovedObserverForTesting;
        try
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ReadinessWithdrawnBeforeBindingRemovedObserverForTesting =
                () =>
                {
                    withdrawalPhaseObserved = true;
                    phaseObservedRetirementAlreadyStarted =
                        retirementStartObserved;
                };
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting = retirement =>
                {
                    retirementStartObserved = true;
                    retirementStartObservedBindingPresent =
                        gateway?.IsBound == true;
                    publishedRetirement = retirement;
                };

            var thrown = await Assert.ThrowsAsync<OutOfMemoryException>(() =>
                DesignConversionRetainedDpoPolicyBaseActivation.ActivateAsync(
                    name => environment.TryGetValue(name, out var value)
                        ? value
                        : null,
                    testHooks: new()
                    {
                        GatewayCreated = createdGateway => gateway =
                            createdGateway,
                        AfterGatewayBindingBeforeReadinessPromotion = () =>
                            throw fatal,
                    }));

            Assert.Same(fatal, thrown);
            Assert.NotNull(gateway);
            Assert.True(withdrawalPhaseObserved);
            Assert.True(retirementStartObserved);
            Assert.False(phaseObservedRetirementAlreadyStarted);
            Assert.False(retirementStartObservedBindingPresent);
            Assert.False(gateway!.IsBound);
            Assert.NotNull(publishedRetirement);
            Assert.True(publishedRetirement!.IsCompleted);
            AssertRetainedPathsReleasedAndDeleteRoot(fixture.Root);
        }
        finally
        {
            ContinuousLearningModelServerStartup
                .RetirementTaskPublishedObserverForTesting =
                priorRetirementObserver;
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .ReadinessWithdrawnBeforeBindingRemovedObserverForTesting =
                priorWithdrawalObserver;
        }
    }


    [Fact]
    public async Task TrainingActivation_ProviderOnlyAndWrongSurfaceIdentityRemainInactive()
    {
        using var fixture = new ModelServerCompositionFixture();
        fixture.CreateOutputDirectories();
        var environment = fixture.ValidEnvironment();
        RequestTrainingActivation(environment);
        var startup = CreateStartup(environment);
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        startup.MaterializeInactiveTp1Composition(access);
        var gateway = await BindExactDpoPolicyBaseProviderAsync(startup, access);
        var wrongSurface = Program.CreateLocalLoopbackArtifactDb(
            readyFilePath: null,
            dpoPolicyBaseProvider: gateway);

        var uncommittedRouteIssuer = startup
            .PrepareDesignConversionTrainingIntakeRouteMappingIssuer(
                wrongSurface,
                gateway,
                startup.LegalTeacherAdmissionAuthority);
        Assert.NotNull(uncommittedRouteIssuer);
        var finalizer = Assert.IsAssignableFrom<MethodInfo>(
            typeof(ContinuousLearningModelServerStartup).GetMethod(
                "TryMarkDesignConversionTrainingIntakeTeacherRuntimeAssociated",
                BindingFlags.Instance | BindingFlags.NonPublic));
        Assert.Equal(
            typeof(ContinuousLearningDesignConversionTeacherAssociationCapability),
            Assert.Single(finalizer.GetParameters()).ParameterType);
        Assert.Throws<TargetInvocationException>(() =>
            finalizer.Invoke(startup, [null]));
        var teacherAssociation =
            new DesignConversionDpoTeacherAttestationHostedAssociation(
                startup,
                gateway);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            teacherAssociation.StartAsync(CancellationToken.None));

        Assert.False(startup.Readiness.StableDpoExecutorConfigured);
        Assert.False(startup.Readiness.TrainingEnabled);
        Assert.False(startup.Readiness.CycleExecutionAvailable);
        Assert.False(startup.Readiness.PerformsTraining);
        Assert.False(startup.Readiness.ProductReady);
        Assert.False(startup.IsLegalTrainingTeacherAdmissionActive());
        Assert.False(gateway.IsBound);
        await gateway.AwaitRetirementAsync();
    }


    [Fact]
    public void TrainingActivation_CallerAssertedFriendRouteCapabilitySubclassIsRejected()
    {
        using var fixture = new ModelServerCompositionFixture();
        var environment = fixture.ValidEnvironment();
        RequestTrainingActivation(environment);
        var startup = CreateStartup(environment);
        var surface = new object();
        var provider = new object();
        var routeIssuer = startup
            .PrepareDesignConversionTrainingIntakeRouteMappingIssuer(
                surface,
                provider,
                startup.LegalTeacherAdmissionAuthority);
        Assert.NotNull(routeIssuer);
        var callerAssertedRouteCapability =
            new CallerAssertedRouteMappingCapability(
                routeIssuer!,
                new object(),
                surface,
                provider,
                startup.LegalTeacherAdmissionAuthority);

        Assert.False(startup
            .TryBindDesignConversionTrainingIntakeRouteMapping(
                callerAssertedRouteCapability));

        Assert.False(startup.Readiness.TrainingEnabled);
        Assert.False(startup.Readiness.CycleExecutionAvailable);
        Assert.False(startup.Readiness.PerformsTraining);
        Assert.False(startup.Readiness.ProductReady);
        Assert.False(startup.IsLegalTrainingTeacherAdmissionActive());
    }


    private static async Task BindExactDpoPolicyBaseProviderAsync(
        DesignConversionRetainedDpoPolicyBaseProviderGateway gateway,
        ContinuousLearningModelServerStartup startup,
        ServerOwnedContinuousLearningFileAccess access)
    {
        var configuration =
            startup.CaptureDpoPolicyBaseBindingConfiguration(access);
        var providerResult = await DesignConversionRetainedDpoPolicyBaseComposition
            .CreateInactiveProviderAsync(
                access,
                configuration.ModelConfig,
                configuration.ModelId,
                configuration.ModelVersion,
                configuration.RootInitializationSeed,
                configuration.TrainingConfigurationFingerprint);
        Assert.True(providerResult.IsSuccess, providerResult.ErrorMessage);
        var provider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(
                providerResult.Value);
        var bound = gateway.BindExactProvider(
            provider,
            startup,
            access,
            CancellationToken.None);
        Assert.True(bound.IsSuccess, bound.ErrorMessage);
        Assert.NotNull(bound.Value);
        Assert.True(startup.TryMarkDpoPolicyBaseProviderBound(
            access,
            bound.Value));
    }

    private static async Task<DesignConversionRetainedDpoPolicyBaseProviderGateway>
        BindExactDpoPolicyBaseProviderAsync(
            ContinuousLearningModelServerStartup startup,
            ServerOwnedContinuousLearningFileAccess access)
    {
        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        await BindExactDpoPolicyBaseProviderAsync(gateway, startup, access);
        return gateway;
    }


    private static void ClearStartupRetirementPreparationObservers()
    {
        ContinuousLearningModelServerStartup
            .BeforeRetirementDependencySnapshotObserverForTesting = null;
        ContinuousLearningModelServerStartup
            .BeforeRetirementFailureStateAllocationObserverForTesting = null;
        ContinuousLearningModelServerStartup
            .BeforeRetirementCompletionLaunchObserverForTesting = null;
    }


    private static void RequestTrainingActivation(
        IDictionary<string, string> environment)
    {
        environment[ContinuousLearningModelServerConfigurationLoader
            .TrainingActivationEnabledEnvironmentName] = bool.TrueString;
        environment[ContinuousLearningModelServerConfigurationLoader
            .TrainingRootInitializationSeedEnvironmentName] = "0";
    }

}
