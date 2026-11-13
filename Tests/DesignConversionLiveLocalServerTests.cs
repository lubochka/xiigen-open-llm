// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using ContinuousLearning;
using DesignConversionDomain;
using ModelServer.DesignConversion;

[Collection(DesignConversionLocalHostTestCollectionNames.CollectionName)]
public sealed class DesignConversionLiveLocalServerTests
{
    private static readonly TimeSpan ServerProcessStopTimeout = TimeSpan.FromSeconds(10);
    // Envelopes two bounded 20s server-owned capture attempts plus process cleanup and synchronous post-capture persistence/readback.
    private static readonly TimeSpan LiveBrowserBackedSubmitTimeout = TimeSpan.FromSeconds(90);
    private const string RouteAdmissionCurrentToken = "live-local-current-route-token-0123456789";
    private const string RouteAdmissionPreviousToken = "live-local-previous-route-token-0123456789";
    private const string RouteAdmissionPreviousTokenExpiresAtUtc = "2036-01-01T00:00:00Z";
    private const string RouteAdmissionTenantId = "tenant-live-local-route-admission-test";
    private const string ModelLifecycleCurrentToken = "live-local-current-lifecycle-token-0123456789";
    private const string ModelLifecyclePreviousToken = "live-local-previous-lifecycle-token-0123456789";
    private const string ModelLifecycleUnknownToken = "live-local-unknown-lifecycle-token-0123456789";
    private const string ModelLifecyclePreviousTokenExpiresAtUtc = "2036-01-01T00:00:00Z";
    private const string ModelLifecycleCurrentTokenEnvironmentName = "DESIGN_CONVERSION_LIVE_MODEL_LIFECYCLE_CURRENT_TOKEN";
    private const string ModelLifecyclePreviousTokenEnvironmentName = "DESIGN_CONVERSION_LIVE_MODEL_LIFECYCLE_PREVIOUS_TOKEN";
    private const string ModelLifecyclePreviousTokenExpiresEnvironmentName =
        "DESIGN_CONVERSION_LIVE_MODEL_LIFECYCLE_PREVIOUS_TOKEN_EXPIRES_AT_UTC";
    private static readonly ConcurrentDictionary<int, ServerProcessOutputCapture> ServerProcessOutputs = new();

    [Fact(Timeout = 120000)]
    public async Task FullLiveHost_MeasurementGateConfigurationControlsFailClosedTrainingBoundaryAndPublicBoolean()
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var absentReadyFile = Path.Combine(tempRoot, "absent-ready.json");
        var configuredReadyFile = Path.Combine(tempRoot, "configured-ready.json");
        var configurationFile = Path.Combine(tempRoot, "private-measurement-thresholds.json");
        Directory.CreateDirectory(tempRoot);
        await WriteMeasurementGateThresholdConfigurationAsync(configurationFile);

        Process? absentProcess = null;
        Process? configuredProcess = null;
        try
        {
            absentProcess = StartServerProcess(
                repoRoot,
                projectPath,
                absentReadyFile,
                Path.Combine(tempRoot, "absent-artifact-db"),
                machineBackedLocal: false,
                routeAdmissionEnabled: false);
            using (var readyDocument = await WaitForReadyFileAsync(
                       absentProcess,
                       absentReadyFile,
                       TimeSpan.FromSeconds(75)))
            {
                AssertMeasurementGateThresholdPublicBoolean(
                    readyDocument.RootElement,
                    expectedConfigured: false,
                    configurationFile);
                using var client = new HttpClient
                {
                    BaseAddress = new Uri(readyDocument.RootElement.GetProperty("baseUrl").GetString()!),
                    Timeout = TimeSpan.FromSeconds(10)
                };
                using var healthResponse = await client.GetAsync("/__pass143/health");
                var healthJson = await healthResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
                using var healthDocument = JsonDocument.Parse(healthJson);
                AssertMeasurementGateThresholdPublicBoolean(
                    healthDocument.RootElement,
                    expectedConfigured: false,
                    configurationFile);

                using var trainingResponse = await PostLiveThresholdTrainingRequestAsync(
                    client,
                    includeMeasurementGateRequest: true);
                var trainingJson = await trainingResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.ServiceUnavailable, trainingResponse.StatusCode);
                Assert.Equal(
                    "TRAINING_INTAKE_MEASUREMENT_GATE_THRESHOLDS_NOT_CONFIGURED",
                    JsonNode.Parse(trainingJson)!["errorCode"]!.GetValue<string>());
                Assert.DoesNotContain(configurationFile, trainingJson, StringComparison.OrdinalIgnoreCase);
            }

            await StopProcessAsync(absentProcess);
            absentProcess = null;

            configuredProcess = StartServerProcess(
                repoRoot,
                projectPath,
                configuredReadyFile,
                Path.Combine(tempRoot, "configured-artifact-db"),
                machineBackedLocal: false,
                routeAdmissionEnabled: false,
                measurementGateThresholdsFile: configurationFile);
            using (var readyDocument = await WaitForReadyFileAsync(
                       configuredProcess,
                       configuredReadyFile,
                       TimeSpan.FromSeconds(75)))
            {
                AssertMeasurementGateThresholdPublicBoolean(
                    readyDocument.RootElement,
                    expectedConfigured: true,
                    configurationFile);
                using var client = new HttpClient
                {
                    BaseAddress = new Uri(readyDocument.RootElement.GetProperty("baseUrl").GetString()!),
                    Timeout = TimeSpan.FromSeconds(10)
                };
                using var healthResponse = await client.GetAsync("/__pass143/health");
                var healthJson = await healthResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
                using var healthDocument = JsonDocument.Parse(healthJson);
                AssertMeasurementGateThresholdPublicBoolean(
                    healthDocument.RootElement,
                    expectedConfigured: true,
                    configurationFile);

                using var trainingResponse = await PostLiveThresholdTrainingRequestAsync(
                    client,
                    includeMeasurementGateRequest: false);
                var trainingJson = await trainingResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.BadRequest, trainingResponse.StatusCode);
                Assert.Equal(
                    "TRAINING_INTAKE_MEASUREMENT_GATE_REQUEST_REQUIRED",
                    JsonNode.Parse(trainingJson)!["errorCode"]!.GetValue<string>());
                Assert.DoesNotContain(configurationFile, trainingJson, StringComparison.OrdinalIgnoreCase);
            }
        }
        finally
        {
            if (configuredProcess is not null)
                await StopProcessAsync(configuredProcess);
            if (absentProcess is not null)
                await StopProcessAsync(absentProcess);
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ScaffoldLiveHost_ConfiguredThresholdFileExposesBooleanButDoesNotMapTrainingRoutes()
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var configurationFile = Path.Combine(tempRoot, "private-scaffold-measurement-thresholds.json");
        Directory.CreateDirectory(tempRoot);
        await WriteMeasurementGateThresholdConfigurationAsync(configurationFile);

        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            artifactDbRoot: null,
            machineBackedLocal: false,
            scaffoldRenderer: true,
            routeAdmissionEnabled: false,
            measurementGateThresholdsFile: configurationFile);
        try
        {
            using var readyDocument = await WaitForReadyFileAsync(
                process,
                readyFile,
                TimeSpan.FromSeconds(75));
            var readyRoot = readyDocument.RootElement;
            AssertMeasurementGateThresholdPublicBoolean(
                readyRoot,
                expectedConfigured: true,
                configurationFile);
            Assert.DoesNotContain(
                DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                ReadStringArray(readyRoot, "routePaths"));

            using var client = CreateLoopbackClient(readyRoot);
            await AssertNonTrainingPhysicalRouteSetAsync(client);
            using var healthResponse = await client.GetAsync("/__pass143/health");
            var healthJson = await healthResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
            using var healthDocument = JsonDocument.Parse(healthJson);
            AssertMeasurementGateThresholdPublicBoolean(
                healthDocument.RootElement,
                expectedConfigured: true,
                configurationFile);
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Theory(Timeout = 60000)]
    [InlineData("duplicate")]
    [InlineData("missing")]
    [InlineData("empty")]
    [InlineData("next-flag")]
    public async Task MeasurementGateThresholdOption_MalformedCardinalityFails64BeforeReady(
        string scenario)
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var configurationFile = Path.Combine(tempRoot, "private-cli-cardinality-thresholds.json");
        Directory.CreateDirectory(tempRoot);
        await WriteMeasurementGateThresholdConfigurationAsync(configurationFile);
        IReadOnlyList<string> additionalArguments = scenario switch
        {
            "duplicate" =>
            [
                "--measurement-gate-thresholds-file",
                configurationFile
            ],
            "missing" => ["--measurement-gate-thresholds-file"],
            "empty" => ["--measurement-gate-thresholds-file", ""],
            "next-flag" => ["--measurement-gate-thresholds-file", "--scaffold-renderer"],
            _ => throw new InvalidOperationException(scenario)
        };

        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            Path.Combine(tempRoot, "artifact-db"),
            machineBackedLocal: false,
            routeAdmissionEnabled: false,
            additionalArguments: additionalArguments,
            measurementGateThresholdsFile: scenario == "duplicate" ? configurationFile : null);
        try
        {
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));

            Assert.Equal(64, process.ExitCode);
            Assert.False(File.Exists(readyFile));
            var processOutput = DescribeProcessOutput(process);
            Assert.Contains("--measurement-gate-thresholds-file", processOutput, StringComparison.Ordinal);
            Assert.Contains(
                scenario == "duplicate" ? "at most once" : "one non-empty file path value",
                processOutput,
                StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(configurationFile, processOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("9876543210", processOutput, StringComparison.Ordinal);
            Assert.DoesNotContain("1234.56789", processOutput, StringComparison.Ordinal);
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Fact]
    public void ProgramSource_UsesSingleBalancedReadyFileLifecycleInvocation()
    {
        AssertProgramReadyFileLifecycleWiring(ReadLiveLocalProgramSource());
    }

    [Fact]
    public void ProgramSource_MachineBackedStartupUsesPinnedCheckpointAndNoTrainingEntryPoint()
    {
        var source = ReadLiveLocalProgramSource();
        var codeOnly = MaskNonCodeCSharpTextPreservingStructure(source);

        Assert.DoesNotContain(".TrainBatchAsync(", codeOnly, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "DesignConversionMachineTrainingAuthority.ExecuteExactAsync(",
            codeOnly,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "CreateTinyMachineBackedLocalMachineAsync(",
            codeOnly,
            StringComparison.Ordinal);
        Assert.Contains(
            "Pass143MachineBootstrapCheckpoint.LoadAsync()",
            codeOnly,
            StringComparison.Ordinal);
    }

    [Fact]
    public void ProgramSource_MachineBackedStartupDoesNotEmbedTrainingPayload()
    {
        var codeOnly = MaskNonCodeCSharpTextPreservingStructure(
            ReadLiveLocalProgramSource());

        Assert.DoesNotContain("LearningBatch<", codeOnly, StringComparison.Ordinal);
        Assert.DoesNotContain("LearningTrainingItem(", codeOnly, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "DesignConversionDpoPolicyCheckpointBridge.SerializePolicySideText(",
            codeOnly,
            StringComparison.Ordinal);
        Assert.DoesNotContain("pass143-machine-backed-local", codeOnly, StringComparison.Ordinal);
    }

    [Fact]
    public void ProgramSource_MachineBackedStartupCannotReceiveOrMintTrainingAuthority()
    {
        var source = ReadLiveLocalProgramSource();
        var codeOnly = MaskNonCodeCSharpTextPreservingStructure(source);

        Assert.Contains(
            "Pass143MachineBootstrapCheckpoint.LoadAsync()",
            codeOnly,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "Pass143MachineBootstrapCheckpoint.LoadAsync(legalTeacherAdmissionAuthority",
            codeOnly,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "new ServerOwnedLegalTrainingTeacherAdmissionAuthority",
            codeOnly,
            StringComparison.Ordinal);
    }

    [Fact]
    public void ProgrammaticComposition_ThreadsExactNullableDpoProviderThroughBothInternalDbPaths()
    {
        var assembly = LoadBuiltLiveLocalAssembly();
        var optionsType = assembly.GetType("Pass143Options", throwOnError: true)!;
        var activationDecisionType = assembly.GetType(
            "Pass143RetainedDpoPolicyBaseActivationDecision",
            throwOnError: true)!;
        var runtimeType = assembly.GetType("Pass143LiveLocalRuntime", throwOnError: true)!;
        var createAsync = runtimeType.GetMethod(
            "CreateAsync",
            BindingFlags.Public | BindingFlags.Static)!;
        var createConfiguredArtifactDb = runtimeType.GetMethod(
            "CreateConfiguredArtifactDb",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        var createAsyncProviderParameter = Assert.Single(
            createAsync.GetParameters(),
            parameter =>
                parameter.Name == "dpoPolicyBaseProvider" &&
                parameter.ParameterType ==
                    typeof(IDesignConversionDpoPolicyBaseProvider));
        Assert.Equal("dpoPolicyBaseProvider", createAsyncProviderParameter.Name);
        Assert.Equal(
            typeof(IDesignConversionDpoPolicyBaseProvider),
            createAsyncProviderParameter.ParameterType);
        Assert.True(createAsyncProviderParameter.HasDefaultValue);
        Assert.Null(createAsyncProviderParameter.DefaultValue);
        Assert.Equal(
            NullabilityState.Nullable,
            new NullabilityInfoContext().Create(createAsyncProviderParameter).ReadState);

        var createAsyncAuthorityParameter = Assert.Single(
            createAsync.GetParameters(),
            parameter =>
                parameter.Name == "legalTeacherAdmissionAuthority" &&
                parameter.ParameterType == typeof(
                    ServerOwnedLegalTrainingTeacherAdmissionAuthority));
        Assert.True(createAsyncAuthorityParameter.HasDefaultValue);
        Assert.Null(createAsyncAuthorityParameter.DefaultValue);
        Assert.Equal(
            NullabilityState.Nullable,
            new NullabilityInfoContext().Create(createAsyncAuthorityParameter).ReadState);

        var configuredProviderParameter = Assert.Single(
            createConfiguredArtifactDb.GetParameters(),
            parameter =>
                parameter.Name == "dpoPolicyBaseProvider" &&
                parameter.ParameterType ==
                    typeof(IDesignConversionDpoPolicyBaseProvider));
        Assert.Equal("dpoPolicyBaseProvider", configuredProviderParameter.Name);
        Assert.Equal(
            typeof(IDesignConversionDpoPolicyBaseProvider),
            configuredProviderParameter.ParameterType);
        Assert.False(configuredProviderParameter.HasDefaultValue);
        Assert.Equal(
            NullabilityState.Nullable,
            new NullabilityInfoContext().Create(configuredProviderParameter).ReadState);
        var configuredAuthorityParameter = Assert.Single(
            createConfiguredArtifactDb.GetParameters(),
            parameter =>
                parameter.Name == "legalTeacherAdmissionAuthority" &&
                parameter.ParameterType == typeof(
                    ServerOwnedLegalTrainingTeacherAdmissionAuthority));
        Assert.False(configuredAuthorityParameter.HasDefaultValue);
        Assert.Equal(
            NullabilityState.Nullable,
            new NullabilityInfoContext().Create(configuredAuthorityParameter).ReadState);

        var stateMachine = createAsync.GetCustomAttribute<AsyncStateMachineAttribute>();
        Assert.NotNull(stateMachine);
        var moveNext = stateMachine!.StateMachineType.GetMethod(
            nameof(IAsyncStateMachine.MoveNext),
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;
        var providerStateField = Assert.Single(
            stateMachine.StateMachineType.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
            field =>
                field.FieldType == typeof(IDesignConversionDpoPolicyBaseProvider)
                && field.Name.Contains("dpoPolicyBaseProvider", StringComparison.Ordinal));
        var authorityStateField = Assert.Single(
            stateMachine.StateMachineType.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
            field =>
                field.FieldType == typeof(
                    ServerOwnedLegalTrainingTeacherAdmissionAuthority)
                && field.Name.Contains(
                    "legalTeacherAdmissionAuthority",
                    StringComparison.Ordinal));
        AssertExactArgumentStoredInStateMachineField(
            createAsync,
            createAsyncProviderParameter.Position,
            providerStateField);
        AssertExactArgumentStoredInStateMachineField(
            createAsync,
            createAsyncAuthorityParameter.Position,
            authorityStateField);
        AssertExactInstanceFieldsPassedAsFinalArgumentsAtEveryCall(
            moveNext,
            createConfiguredArtifactDb,
            [providerStateField, authorityStateField],
            expectedCallCount: 2);
        AssertStateMachineFieldIsNeverReassigned(moveNext, providerStateField);
        AssertStateMachineFieldIsNeverReassigned(moveNext, authorityStateField);
        AssertProviderFieldFlowAnalyzerControls();

        var parse = optionsType.GetMethod(
            "Parse",
            BindingFlags.Public | BindingFlags.Static)!;
        var shouldActivate = activationDecisionType.GetMethod(
            "ShouldActivate",
            BindingFlags.Public | BindingFlags.Static)!;
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            nameof(DesignConversionLiveLocalServerTests),
            "dpo-provider-reflection",
            Guid.NewGuid().ToString("N"));
        var configuredRoot = Path.Combine(tempRoot, "configured-artifact-db");
        var unconfiguredRoot = Path.Combine(tempRoot, "unconfigured-artifact-db");
        var provider = new DesignConversionDpoPolicyBaseTestProvider();

        try
        {
            var configuredOptions = parse.Invoke(
                null,
                [new[]
                {
                    "--ready-file", Path.Combine(tempRoot, "configured-ready.json"),
                    "--artifact-db-root", configuredRoot,
                    "--no-machine-backed-local"
                }])!;
            Assert.True(Assert.IsType<bool>(
                shouldActivate.Invoke(null, [configuredOptions])));
            var configuredDb = Assert.IsType<JsonlDesignConversionLocalArtifactDb>(
                createConfiguredArtifactDb.Invoke(
                    null,
                    [configuredOptions, provider, null]));
            Assert.Same(provider, ReadDpoPolicyBaseProvider(configuredDb));

            var scaffoldOptions = parse.Invoke(
                null,
                [new[]
                {
                    "--ready-file", Path.Combine(tempRoot, "scaffold-ready.json"),
                    "--artifact-db-root", Path.Combine(tempRoot, "scaffold-artifact-db"),
                    "--scaffold-renderer"
                }])!;
            Assert.False(Assert.IsType<bool>(
                shouldActivate.Invoke(null, [scaffoldOptions])));

            var unconfiguredOptions = parse.Invoke(
                null,
                [new[]
                {
                    "--ready-file", Path.Combine(tempRoot, "unconfigured-ready.json"),
                    "--artifact-db-root", unconfiguredRoot,
                    "--no-machine-backed-local"
                }])!;
            var unconfiguredDb = Assert.IsType<JsonlDesignConversionLocalArtifactDb>(
                createConfiguredArtifactDb.Invoke(
                    null,
                    [unconfiguredOptions, null, null]));
            Assert.Null(ReadDpoPolicyBaseProvider(unconfiguredDb));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }

        Assert.Equal(0, provider.InvocationCount);
        Assert.DoesNotContain(
            assembly.GetTypes(),
            type =>
                typeof(IDesignConversionDpoPolicyBaseProvider).IsAssignableFrom(type)
                && type is { IsInterface: false, IsAbstract: false });

        var source = ReadLiveLocalProgramSource();
        Assert.DoesNotContain(
            "dpoPolicyBaseProvider: null",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "Pass143RetainedDpoPolicyBaseActivationDecision.ShouldActivate(options)",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "return !options.ScaffoldRenderer",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "&& !string.IsNullOrWhiteSpace(options.ArtifactDbRoot)",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "DesignConversionRetainedDpoPolicyBaseActivation",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "await using var ownedRetainedDpoPolicyBaseLease",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "dpoPolicyBaseProvider: ownedRetainedDpoPolicyBaseLease?.Provider",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "activation.ErrorCode + \": \" + activation.ErrorMessage",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "new DesignConversionRetainedDpoPolicyBaseProviderGateway",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain("Qwen", source, StringComparison.OrdinalIgnoreCase);
        var codeOnlyWithoutWhitespace = string.Concat(
            MaskNonCodeCSharpTextPreservingStructure(source)
                .Where(character => !char.IsWhiteSpace(character)));
        Assert.True(
            codeOnlyWithoutWhitespace.IndexOf(
                "DesignConversionRetainedDpoPolicyBaseActivation.ActivateAsync()",
                StringComparison.Ordinal)
            < codeOnlyWithoutWhitespace.IndexOf(
                "Pass143LiveLocalRuntime.CreateAsync(",
                StringComparison.Ordinal));
        Assert.Equal(
            2,
            CountOccurrences(
                codeOnlyWithoutWhitespace,
                "CreateConfiguredArtifactDb(options,dpoPolicyBaseProvider,legalTeacherAdmissionAuthority)"));
    }

    [Theory]
    [InlineData("block-comment")]
    [InlineData("normal-string")]
    [InlineData("escaped-normal-string")]
    [InlineData("verbatim-string")]
    [InlineData("raw-string")]
    [InlineData("interpolated-string")]
    [InlineData("interpolated-verbatim-string")]
    [InlineData("interpolated-raw-string")]
    public void ProgramSource_LifecycleCleanupTokenOutsideCodeCannotSatisfyWiringAssertion(
        string literalKind)
    {
        var source = ReadLiveLocalProgramSource().ReplaceLineEndings("\n");
        const string executableCleanup =
            """
            cleanupReadyFile: () =>
                        Pass143ReadyFileCleanup.TryDelete(options.ReadyFile)
            """;
        var fakeCleanup = literalKind switch
        {
            "block-comment" =>
                "cleanupReadyFile: () => { /* Pass143ReadyFileCleanup.TryDelete(options.ReadyFile) */ }",
            "normal-string" =>
                "cleanupReadyFile: () => { _ = \"Pass143ReadyFileCleanup.TryDelete(options.ReadyFile)\"; }",
            "escaped-normal-string" =>
                "cleanupReadyFile: () => { _ = \"\\\"Pass143ReadyFileCleanup.TryDelete(options.ReadyFile)\\\"\"; }",
            "verbatim-string" =>
                "cleanupReadyFile: () => { _ = @\"Pass143ReadyFileCleanup.TryDelete(options.ReadyFile)\"; }",
            "raw-string" =>
                "cleanupReadyFile: () => { _ = \"\"\"Pass143ReadyFileCleanup.TryDelete(options.ReadyFile)\"\"\"; }",
            "interpolated-string" =>
                "cleanupReadyFile: () => { _ = $\"Pass143ReadyFileCleanup.TryDelete(options.ReadyFile)\"; }",
            "interpolated-verbatim-string" =>
                "cleanupReadyFile: () => { _ = $@\"Pass143ReadyFileCleanup.TryDelete(options.ReadyFile)\"; }",
            "interpolated-raw-string" =>
                "cleanupReadyFile: () => { _ = $\"\"\"Pass143ReadyFileCleanup.TryDelete(options.ReadyFile)\"\"\"; }",
            _ => throw new InvalidOperationException(literalKind)
        };
        var mutated = source.Replace(
            executableCleanup,
            fakeCleanup,
            StringComparison.Ordinal);
        Assert.NotEqual(source, mutated);

        Assert.ThrowsAny<Xunit.Sdk.XunitException>(
            () => AssertProgramReadyFileLifecycleWiring(mutated));
    }

    [Fact]
    public void ProgramSource_FakeLifecycleInvocationInCommentCannotReplaceProductionInvocation()
    {
        var source = ReadLiveLocalProgramSource();
        var mutated = source.Replace(
            "DesignConversionLiveLocalReadyFileLifecycle.RunAsync",
            "BypassedReadyFileLifecycle.RunAsync",
            StringComparison.Ordinal)
            + """

              /*
              DesignConversionLiveLocalReadyFileLifecycle.RunAsync(
                  app.StartAsync(),
                  WriteReadyFile(options.ReadyFile),
                  app.WaitForShutdownAsync(),
                  Pass143ReadyFileCleanup.TryDelete(options.ReadyFile));
              */
              """;
        Assert.NotEqual(source, mutated);

        Assert.ThrowsAny<Xunit.Sdk.XunitException>(
            () => AssertProgramReadyFileLifecycleWiring(mutated));
    }

    [Fact]
    public async Task ReadyFileLifecycle_NormalCompletionRunsOrderedPhasesAndRemovesMarker()
    {
        var tempRoot = CreateReadyFileLifecycleTempRoot();
        var readyFile = Path.Combine(tempRoot, "normal-ready.json");
        var operations = new List<string>();

        try
        {
            await DesignConversionLiveLocalReadyFileLifecycle.RunAsync(
                startAsync: () =>
                {
                    operations.Add("start");
                    return Task.CompletedTask;
                },
                writeReadyFile: () =>
                {
                    operations.Add("write");
                    File.WriteAllText(readyFile, "{\"ready\":true}", Encoding.UTF8);
                },
                waitForShutdownAsync: () =>
                {
                    operations.Add("wait");
                    return Task.CompletedTask;
                },
                cleanupReadyFile: () =>
                {
                    operations.Add("cleanup");
                    File.Delete(readyFile);
                });

            Assert.Equal(["start", "write", "wait", "cleanup"], operations);
            Assert.False(File.Exists(readyFile));
        }
        finally
        {
            DeleteReadyFileLifecycleTempRoot(tempRoot);
        }
    }

    [Theory]
    [InlineData("start")]
    [InlineData("write")]
    [InlineData("wait")]
    public async Task ReadyFileLifecycle_PrimaryPhaseFailureCleansMarkerAndRethrowsSameException(
        string failingPhase)
    {
        var tempRoot = CreateReadyFileLifecycleTempRoot();
        var readyFile = Path.Combine(tempRoot, $"{failingPhase}-ready.json");
        File.WriteAllText(readyFile, "{\"stale\":true}", Encoding.UTF8);
        var operations = new List<string>();
        var primary = new InvalidOperationException($"primary-{failingPhase}");

        try
        {
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                DesignConversionLiveLocalReadyFileLifecycle.RunAsync(
                    startAsync: () =>
                    {
                        operations.Add("start");
                        if (string.Equals(failingPhase, "start", StringComparison.Ordinal))
                            throw primary;
                        return Task.CompletedTask;
                    },
                    writeReadyFile: () =>
                    {
                        operations.Add("write");
                        File.WriteAllText(readyFile, "{\"ready\":true}", Encoding.UTF8);
                        if (string.Equals(failingPhase, "write", StringComparison.Ordinal))
                            throw primary;
                    },
                    waitForShutdownAsync: () =>
                    {
                        operations.Add("wait");
                        if (string.Equals(failingPhase, "wait", StringComparison.Ordinal))
                            throw primary;
                        return Task.CompletedTask;
                    },
                    cleanupReadyFile: () =>
                    {
                        operations.Add("cleanup");
                        File.Delete(readyFile);
                    }));

            Assert.Same(primary, actual);
            Assert.Equal(
                failingPhase switch
                {
                    "start" => ["start", "cleanup"],
                    "write" => ["start", "write", "cleanup"],
                    "wait" => ["start", "write", "wait", "cleanup"],
                    _ => throw new InvalidOperationException(failingPhase)
                },
                operations);
            Assert.False(File.Exists(readyFile));
        }
        finally
        {
            DeleteReadyFileLifecycleTempRoot(tempRoot);
        }
    }

    [Fact]
    public async Task ReadyFileLifecycle_PrimaryFailureExpectedCleanupIOExceptionCannotReplaceOrLeak()
    {
        var tempRoot = CreateReadyFileLifecycleTempRoot();
        var readyFile = Path.Combine(tempRoot, "private-ready-marker.json");
        File.WriteAllText(readyFile, "{\"ready\":true}", Encoding.UTF8);
        var operations = new List<string>();
        var primary = new InvalidOperationException("primary-wait-failure");

        try
        {
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                DesignConversionLiveLocalReadyFileLifecycle.RunAsync(
                    startAsync: () =>
                    {
                        operations.Add("start");
                        return Task.CompletedTask;
                    },
                    writeReadyFile: () => operations.Add("write"),
                    waitForShutdownAsync: () =>
                    {
                        operations.Add("wait");
                        throw primary;
                    },
                    cleanupReadyFile: () =>
                    {
                        operations.Add("cleanup");
                        throw new IOException($"cannot remove private path {readyFile}");
                    }));

            Assert.Same(primary, actual);
            Assert.Equal(["start", "write", "wait", "cleanup"], operations);
            Assert.DoesNotContain(readyFile, actual.ToString(), StringComparison.OrdinalIgnoreCase);
            Assert.True(File.Exists(readyFile));
        }
        finally
        {
            DeleteReadyFileLifecycleTempRoot(tempRoot);
        }
    }

    [Fact]
    public async Task ReadyFileLifecycle_NormalCompletionExpectedCleanupFailureIsBestEffort()
    {
        var tempRoot = CreateReadyFileLifecycleTempRoot();
        var readyFile = Path.Combine(tempRoot, "best-effort-ready-marker.json");
        var operations = new List<string>();

        try
        {
            await DesignConversionLiveLocalReadyFileLifecycle.RunAsync(
                startAsync: () =>
                {
                    operations.Add("start");
                    return Task.CompletedTask;
                },
                writeReadyFile: () =>
                {
                    operations.Add("write");
                    File.WriteAllText(readyFile, "{\"ready\":true}", Encoding.UTF8);
                },
                waitForShutdownAsync: () =>
                {
                    operations.Add("wait");
                    return Task.CompletedTask;
                },
                cleanupReadyFile: () =>
                {
                    operations.Add("cleanup");
                    throw new UnauthorizedAccessException(
                        $"private cleanup path {readyFile}");
                });

            Assert.Equal(["start", "write", "wait", "cleanup"], operations);
            Assert.True(File.Exists(readyFile));
        }
        finally
        {
            DeleteReadyFileLifecycleTempRoot(tempRoot);
        }
    }

    [Fact]
    public async Task ReadyFileLifecycle_NormalCompletionUnexpectedCleanupFailureEscapesUnchanged()
    {
        var tempRoot = CreateReadyFileLifecycleTempRoot();
        var readyFile = Path.Combine(tempRoot, "unexpected-cleanup-ready-marker.json");
        var operations = new List<string>();
        var unexpected = new InvalidOperationException(
            $"unexpected cleanup failure for {readyFile}");

        try
        {
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                DesignConversionLiveLocalReadyFileLifecycle.RunAsync(
                    startAsync: () =>
                    {
                        operations.Add("start");
                        return Task.CompletedTask;
                    },
                    writeReadyFile: () =>
                    {
                        operations.Add("write");
                        File.WriteAllText(readyFile, "{\"ready\":true}", Encoding.UTF8);
                    },
                    waitForShutdownAsync: () =>
                    {
                        operations.Add("wait");
                        return Task.CompletedTask;
                    },
                    cleanupReadyFile: () =>
                    {
                        operations.Add("cleanup");
                        throw unexpected;
                    }));

            Assert.Same(unexpected, actual);
            Assert.Contains(readyFile, actual.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(["start", "write", "wait", "cleanup"], operations);
            Assert.True(File.Exists(readyFile));
        }
        finally
        {
            DeleteReadyFileLifecycleTempRoot(tempRoot);
        }
    }

    [Fact(Timeout = 60000)]
    public async Task InvalidMeasurementGateThresholdConfiguration_Fails64BeforeReadyWithoutPrivateLeak()
    {
        const string privateMarker = "PRIVATE_INVALID_MEASUREMENT_THRESHOLD_CONFIGURATION";
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var invalidConfigurationFile = Path.Combine(
            tempRoot,
            privateMarker,
            "invalid-thresholds.json");
        Directory.CreateDirectory(Path.GetDirectoryName(invalidConfigurationFile)!);
        await File.WriteAllTextAsync(
            readyFile,
            "{\"stale\":true}",
            new UTF8Encoding(false));
        await File.WriteAllTextAsync(
            invalidConfigurationFile,
            $$"""{"unknown_key":"{{privateMarker}}"}""",
            new UTF8Encoding(false));

        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            Path.Combine(tempRoot, "artifact-db"),
            machineBackedLocal: false,
            routeAdmissionEnabled: false,
            measurementGateThresholdsFile: invalidConfigurationFile);
        try
        {
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));

            Assert.Equal(64, process.ExitCode);
            Assert.False(File.Exists(readyFile));
            var processOutput = DescribeProcessOutput(process);
            Assert.Contains(
                "DESIGN_CONVERSION_MEASUREMENT_GATE_THRESHOLD_CONFIGURATION_SCHEMA_INVALID",
                processOutput,
                StringComparison.Ordinal);
            Assert.DoesNotContain("PASS143_SERVER_READY", processOutput, StringComparison.Ordinal);
            Assert.DoesNotContain(tempRoot, processOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(readyFile, processOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(invalidConfigurationFile, processOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(privateMarker, processOutput, StringComparison.Ordinal);
            Assert.DoesNotContain("unknown_key", processOutput, StringComparison.Ordinal);
        }
        finally
        {
            if (!process.HasExited)
                await StopProcessAsync(process);
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task NoFlagOptions_DefaultToMachineBackedLocalAndWriteReadyFileWithHonestyFlags()
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        Directory.CreateDirectory(tempRoot);

        using var process = StartServerProcess(repoRoot, projectPath, readyFile, artifactDbRoot);
        AssertServerProcessOmittedMachineModeFlags(process);
        try
        {
            using var readyDocument = await WaitForReadyFileAsync(process, readyFile, TimeSpan.FromSeconds(75));
            var readyRoot = readyDocument.RootElement;
            var routePaths = readyRoot.GetProperty("routePaths")
                .EnumerateArray()
                .Select(path => path.GetString())
                .ToArray();
            var expectedMachineBackedRoutePaths = FullLocalRouteSetWithArtifactUpload();

            AssertSourceIdentity(readyRoot, repoRoot);
            Assert.True(readyRoot.GetProperty("machineBackedLocal").GetBoolean());
            Assert.True(readyRoot.GetProperty("fullLocalRouteSet").GetBoolean());
            Assert.Equal(
                BrowserBackedDesignConversionRenderProvider.ProviderKind,
                readyRoot.GetProperty("renderProviderKind").GetString());
            AssertPublicRenderArtifactRootRef(readyRoot);
            Assert.True(readyRoot.GetProperty("browserRendererConfigured").GetBoolean());
            Assert.True(readyRoot.GetProperty("browserRendererRuntimeAvailable").GetBoolean());
            AssertPublicBrowserExecutableMetadata(readyRoot, configured: true, runtimeAvailable: true);
            Assert.True(readyRoot.GetProperty("machineBackedApiFactoryCaller").GetBoolean());
            Assert.True(readyRoot.GetProperty("machineBackedApiRuntimeProbeAttempted").GetBoolean());
            Assert.True(readyRoot.GetProperty("machineBackedApiRuntimeProbeExecuted").GetBoolean());
            Assert.True(readyRoot.GetProperty("machineBackedApiLearnedInferenceApplied").GetBoolean());
            Assert.True(readyRoot.GetProperty("machineBackedApiBrowserE2eVerified").GetBoolean());
            Assert.Equal(
                "verified_by_server_owned_browser_render_receipts",
                readyRoot.GetProperty("machineBackedApiBrowserE2eVerificationState").GetString());
            Assert.Equal(
                "server_owned_model_server_browser_render_receipts",
                readyRoot.GetProperty("machineBackedApiBrowserE2eVerificationAuthority").GetString());
            Assert.Equal(2, readyRoot.GetProperty("machineBackedApiBrowserE2eVerifiedRenderArtifactCount").GetInt32());
            Assert.Equal(2, readyRoot.GetProperty("machineBackedApiBrowserE2eRequiredViewportCount").GetInt32());
            Assert.Equal(2, readyRoot.GetProperty("machineBackedApiBrowserE2eObservedViewportCount").GetInt32());
            Assert.Equal("none", readyRoot.GetProperty("machineBackedApiBrowserE2eFailureReason").GetString());
            Assert.False(readyRoot.GetProperty("machineBackedApiProductReady").GetBoolean());
            Assert.False(readyRoot.GetProperty("machineBackedApiPublicHttpEndpointVerified").GetBoolean());
            Assert.Equal("probe_completed_product_not_ready", readyRoot.GetProperty("machineBackedApiStartupStatus").GetString());
            Assert.Equal(
                "MACHINE_BACKED_API_PROBE_COMPLETED_PRODUCT_NOT_READY",
                readyRoot.GetProperty("machineBackedApiNonReadyCode").GetString());
            Assert.NotEqual(
                "MACHINE_BACKED_LOCAL_NOT_REQUESTED",
                readyRoot.GetProperty("machineBackedApiNonReadyCode").GetString());
            Assert.NotEqual(
                "DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED",
                readyRoot.GetProperty("machineBackedApiNonReadyCode").GetString());
            Assert.Contains(
                "product readiness remains false",
                readyRoot.GetProperty("machineBackedApiNonReadyReason").GetString(),
                StringComparison.OrdinalIgnoreCase);
            var internalRenderArtifactRoot = Path.Combine(tempRoot, "render-artifacts");
            AssertMachineBackedPngProof(readyRoot, internalRenderArtifactRoot);
            AssertBrowserRendererPngProofPublicFields(
                readyRoot,
                expectedRequired: true,
                expectedAttempted: true,
                expectedValid: true,
                expectedStartupStatus: "png_proof_verified",
                expectedNonReadyCode: "none",
                expectedEvidenceCount: 4,
                expectedRequiredArtifactCount: 4,
                expectedRequiredViewportCount: 2,
                expectedObservedViewportCount: 2);
            AssertPublicHealthReadyStringsDoNotLeakLocalPaths(readyRoot, repoRoot, tempRoot);
            Assert.True(readyRoot.GetProperty("localhostOnly").GetBoolean());
            Assert.False(readyRoot.GetProperty("publicDeploymentVerified").GetBoolean());
            AssertServerOwnedLocalEvaluationOnlyWithoutExternalJudgeCarrier(readyRoot);
            Assert.False(readyRoot.GetProperty("trainedModelAuthority").GetBoolean());
            Assert.False(readyRoot.GetProperty("productReady").GetBoolean());
            Assert.Equal(1, readyRoot.GetProperty("machineTrainingRows").GetInt32());
            Assert.True(readyRoot.GetProperty("learnedWeightArtifactCount").GetInt32() > 0);
            Assert.Equal(
                expectedMachineBackedRoutePaths.Order(StringComparer.Ordinal),
                routePaths.Order(StringComparer.Ordinal));
            AssertFlow38PublicFields(readyRoot, expectedMapped: true, expectedConfigured: false);
            AssertArtifactUploadPublicFields(readyRoot, expectedMapped: true);
            AssertNoRouteAdmissionSecretLeakage(readyRoot.GetRawText());

            using var client = new HttpClient
            {
                BaseAddress = new Uri(readyRoot.GetProperty("baseUrl").GetString()!),
                Timeout = TimeSpan.FromSeconds(10)
            };
            using var unconfiguredFlow38Response = await client.PostAsync(
                DesignConversionModelServerFlow38QuarantineHandler.EndpointPath,
                new StringContent("{}", Encoding.UTF8, "application/json"));
            var unconfiguredFlow38Json = await unconfiguredFlow38Response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.ServiceUnavailable, unconfiguredFlow38Response.StatusCode);
            using (var unconfiguredFlow38Document = JsonDocument.Parse(unconfiguredFlow38Json))
            {
                Assert.Equal(
                    "FLOW38_QUARANTINE_INTAKE_NOT_CONFIGURED",
                    unconfiguredFlow38Document.RootElement.GetProperty("errorCode").GetString());
                Assert.False(
                    unconfiguredFlow38Document.RootElement.GetProperty("dpoTrainReady").GetBoolean());
                Assert.False(
                    unconfiguredFlow38Document.RootElement.GetProperty("trainingTriggered").GetBoolean());
            }

            using var healthResponse = await client.GetAsync("/__pass143/health");
            var healthJson = await healthResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
            using var healthDocument = JsonDocument.Parse(healthJson);
            var healthRoot = healthDocument.RootElement;
            var healthRoutePaths = ReadStringArray(healthRoot, "routePaths");
            AssertSourceIdentity(healthRoot, repoRoot);
            Assert.True(healthRoot.GetProperty("machineBackedLocal").GetBoolean());
            Assert.True(healthRoot.GetProperty("fullLocalRouteSet").GetBoolean());
            Assert.Equal(
                expectedMachineBackedRoutePaths.Order(StringComparer.Ordinal),
                healthRoutePaths.Order(StringComparer.Ordinal));
            AssertFlow38PublicFields(healthRoot, expectedMapped: true, expectedConfigured: false);
            AssertArtifactUploadPublicFields(healthRoot, expectedMapped: true);
            Assert.Equal(
                BrowserBackedDesignConversionRenderProvider.ProviderKind,
                healthRoot.GetProperty("renderProviderKind").GetString());
            AssertPublicRenderArtifactRootRef(healthRoot);
            Assert.True(healthRoot.GetProperty("browserRendererConfigured").GetBoolean());
            Assert.True(healthRoot.GetProperty("browserRendererRuntimeAvailable").GetBoolean());
            AssertPublicBrowserExecutableMetadata(healthRoot, configured: true, runtimeAvailable: true);
            Assert.True(healthRoot.GetProperty("machineBackedApiFactoryCaller").GetBoolean());
            Assert.True(healthRoot.GetProperty("machineBackedApiRuntimeProbeAttempted").GetBoolean());
            Assert.True(healthRoot.GetProperty("machineBackedApiRuntimeProbeExecuted").GetBoolean());
            Assert.True(healthRoot.GetProperty("machineBackedApiLearnedInferenceApplied").GetBoolean());
            Assert.True(healthRoot.GetProperty("machineBackedApiBrowserE2eVerified").GetBoolean());
            Assert.Equal(
                "verified_by_server_owned_browser_render_receipts",
                healthRoot.GetProperty("machineBackedApiBrowserE2eVerificationState").GetString());
            Assert.Equal(2, healthRoot.GetProperty("machineBackedApiBrowserE2eVerifiedRenderArtifactCount").GetInt32());
            Assert.Equal(2, healthRoot.GetProperty("machineBackedApiBrowserE2eRequiredViewportCount").GetInt32());
            Assert.Equal(2, healthRoot.GetProperty("machineBackedApiBrowserE2eObservedViewportCount").GetInt32());
            Assert.Equal("none", healthRoot.GetProperty("machineBackedApiBrowserE2eFailureReason").GetString());
            Assert.False(healthRoot.GetProperty("machineBackedApiProductReady").GetBoolean());
            Assert.False(healthRoot.GetProperty("machineBackedApiPublicHttpEndpointVerified").GetBoolean());
            Assert.Equal("probe_completed_product_not_ready", healthRoot.GetProperty("machineBackedApiStartupStatus").GetString());
            Assert.Equal(
                "MACHINE_BACKED_API_PROBE_COMPLETED_PRODUCT_NOT_READY",
                healthRoot.GetProperty("machineBackedApiNonReadyCode").GetString());
            Assert.NotEqual(
                "MACHINE_BACKED_LOCAL_NOT_REQUESTED",
                healthRoot.GetProperty("machineBackedApiNonReadyCode").GetString());
            Assert.NotEqual(
                "DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED",
                healthRoot.GetProperty("machineBackedApiNonReadyCode").GetString());
            AssertMachineBackedPngProof(healthRoot, internalRenderArtifactRoot);
            AssertBrowserRendererPngProofPublicFields(
                healthRoot,
                expectedRequired: true,
                expectedAttempted: true,
                expectedValid: true,
                expectedStartupStatus: "png_proof_verified",
                expectedNonReadyCode: "none",
                expectedEvidenceCount: 4,
                expectedRequiredArtifactCount: 4,
                expectedRequiredViewportCount: 2,
                expectedObservedViewportCount: 2);
            AssertPublicHealthReadyStringsDoNotLeakLocalPaths(healthRoot, repoRoot, tempRoot);
            AssertServerOwnedLocalEvaluationOnlyWithoutExternalJudgeCarrier(healthRoot);
            AssertNoRouteAdmissionSecretLeakage(healthJson);
            Assert.False(healthRoot.GetProperty("productReady").GetBoolean());
            await AssertHumanApprovalChallengeRouteConfiguredAsync(client, tempRoot);

            var uploadPng = SelectUploadPngFixture(internalRenderArtifactRoot);
            var uploadEndpointPath = readyRoot.GetProperty("artifactUploadEndpointPath").GetString()!;
            var ingressBeforeUpload = PublishedIngressPngFiles(internalRenderArtifactRoot).Length;
            Assert.Equal(0, ingressBeforeUpload);
            Assert.Empty(TempFiles(internalRenderArtifactRoot));

            using var uploadResponse = await PostArtifactUploadAsync(
                client,
                uploadEndpointPath,
                uploadPng.Bytes,
                uploadPng,
                artifactId: "live-local-machine-backed-upload",
                idempotencyKey: "live-local-machine-backed-upload-key");
            var uploadJson = await uploadResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
            AssertLiveLocalArtifactUploadSuccess(uploadJson, uploadPng, internalRenderArtifactRoot);
            Assert.Equal(ingressBeforeUpload + 1, PublishedIngressPngFiles(internalRenderArtifactRoot).Length);
            Assert.Empty(TempFiles(internalRenderArtifactRoot));

            using var replayResponse = await PostArtifactUploadAsync(
                client,
                uploadEndpointPath,
                [0x89, 0x50, 0x4e, 0x47, 0x00, 0x01],
                uploadPng,
                artifactId: "live-local-machine-backed-upload",
                idempotencyKey: "live-local-machine-backed-upload-key");
            var replayJson = await replayResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, replayResponse.StatusCode);
            Assert.Equal(uploadJson, replayJson);
            Assert.Equal(ingressBeforeUpload + 1, PublishedIngressPngFiles(internalRenderArtifactRoot).Length);
            Assert.Empty(TempFiles(internalRenderArtifactRoot));

            using var dpoRequest = new HttpRequestMessage(
                HttpMethod.Get,
                DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath +
                "?includeBlocked=true&limit=1");
            AddRouteAdmissionHeaders(
                dpoRequest,
                RouteAdmissionCurrentToken,
                idempotencyKey: null);
            using var dpoResponse = await client.SendAsync(dpoRequest);
            var dpoJson = await dpoResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, dpoResponse.StatusCode);
            using var dpoDocument = JsonDocument.Parse(dpoJson);
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
                dpoDocument.RootElement.GetProperty("endpoint").GetString());
            Assert.False(dpoDocument.RootElement.GetProperty("productReady").GetBoolean());
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ExpectedSourceCommitMismatch_IsVisibleWithoutBlockingReadyOrHealth()
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        var mismatchedCommit = new string('0', 40);
        Directory.CreateDirectory(tempRoot);

        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            artifactDbRoot,
            machineBackedLocal: false,
            expectedSourceCommit: mismatchedCommit);
        try
        {
            using var readyDocument = await WaitForReadyFileAsync(process, readyFile, TimeSpan.FromSeconds(75));
            var readyRoot = readyDocument.RootElement;
            AssertSourceIdentity(
                readyRoot,
                repoRoot,
                expectedSourceCommit: mismatchedCommit,
                expectedSourceCommitMatches: false,
                expectedIdentityConsistency: false);
            Assert.False(readyRoot.GetProperty("productReady").GetBoolean());

            using var client = new HttpClient
            {
                BaseAddress = new Uri(readyRoot.GetProperty("baseUrl").GetString()!),
                Timeout = TimeSpan.FromSeconds(10)
            };
            using var healthResponse = await client.GetAsync("/__pass143/health");
            var healthJson = await healthResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
            using var healthDocument = JsonDocument.Parse(healthJson);
            AssertSourceIdentity(
                healthDocument.RootElement,
                repoRoot,
                expectedSourceCommit: mismatchedCommit,
                expectedSourceCommitMatches: false,
                expectedIdentityConsistency: false);
            Assert.Equal("ok", healthDocument.RootElement.GetProperty("status").GetString());
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ActualSourceCommit_IsReportedWithoutExpectedCommitAndPublicJsonDoesNotLeakGitDiagnostics()
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        Directory.CreateDirectory(tempRoot);

        // Non-training режим оставляет тест в source-identity контракте:
        // browser/PNG семейства не должны решать судьбу этой проверки.
        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            artifactDbRoot,
            machineBackedLocal: false,
            routeAdmissionEnabled: false);
        try
        {
            using var readyDocument = await WaitForReadyFileAsync(process, readyFile, TimeSpan.FromSeconds(75));
            var readyRoot = readyDocument.RootElement;
            AssertSourceIdentity(readyRoot, repoRoot);
            AssertActualSourceCommitPublicSurface(readyRoot, repoRoot, tempRoot);

            using var client = new HttpClient
            {
                BaseAddress = new Uri(readyRoot.GetProperty("baseUrl").GetString()!),
                Timeout = TimeSpan.FromSeconds(10)
            };
            using var healthResponse = await client.GetAsync("/__pass143/health");
            var healthJson = await healthResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
            using var healthDocument = JsonDocument.Parse(healthJson);
            AssertSourceIdentity(healthDocument.RootElement, repoRoot);
            AssertActualSourceCommitPublicSurface(healthDocument.RootElement, repoRoot, tempRoot);
            Assert.Equal("ok", healthDocument.RootElement.GetProperty("status").GetString());
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Fact]
    public void PngEvidenceInspector_RejectsArtifactUriProofForLiveLocalStartup()
    {
        var result = DesignConversionBrowserPngEvidenceInspector.Inspect(
            "artifact://pass143/live-local/screenshot.png");

        Assert.False(result.IsSuccess);
        Assert.Equal("PNG_SCREENSHOT_URI_NOT_LOCAL_FILE", result.ErrorCode);
        Assert.Contains("local file path", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(Timeout = 120000)]
    public async Task BrowserExecutableArgument_FailsClosedBeforeReadyFileWithoutDeterministicFallback()
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        var missingBrowser = Path.Combine(tempRoot, "missing-browser.exe");
        Directory.CreateDirectory(tempRoot);

        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            artifactDbRoot,
            browserExecutable: missingBrowser);
        try
        {
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
            Assert.NotEqual(0, process.ExitCode);
            Assert.False(File.Exists(readyFile));

            var processOutput = DescribeProcessOutput(process);
            Assert.Contains("--browser-executable", processOutput, StringComparison.Ordinal);
            Assert.Contains("server-owned discovery", processOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(
                DeterministicLocalDesignConversionPngRenderProvider.ProviderKind,
                processOutput,
                StringComparison.Ordinal);
            Assert.DoesNotContain(missingBrowser, processOutput, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ConflictingMachineModeOptions_FailClosedBeforeReadyFile()
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        Directory.CreateDirectory(tempRoot);

        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            artifactDbRoot,
            machineBackedLocal: true,
            additionalArguments: ["--no-machine-backed-local"]);
        try
        {
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
            Assert.NotEqual(0, process.ExitCode);
            Assert.False(File.Exists(readyFile));

            var processOutput = DescribeProcessOutput(process);
            Assert.Contains("--machine-backed-local", processOutput, StringComparison.Ordinal);
            Assert.Contains("--no-machine-backed-local", processOutput, StringComparison.Ordinal);
            Assert.Contains("cannot combine", processOutput, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ExplicitMachineBackedLocalWithScaffoldRenderer_FailsClosedBeforeReadyFile()
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        Directory.CreateDirectory(tempRoot);

        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            artifactDbRoot,
            machineBackedLocal: true,
            scaffoldRenderer: true);
        try
        {
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
            Assert.NotEqual(0, process.ExitCode);
            Assert.False(File.Exists(readyFile));

            var processOutput = DescribeProcessOutput(process);
            Assert.Contains("--machine-backed-local", processOutput, StringComparison.Ordinal);
            Assert.Contains("--scaffold-renderer", processOutput, StringComparison.Ordinal);
            Assert.Contains("cannot combine", processOutput, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Theory(Timeout = 120000)]
    [InlineData("--scaffold-renderer", null)]
    [InlineData("--deterministic-scaffold-renderer", false)]
    public async Task ScaffoldRendererOptions_KeepDeterministicPngProviderNonTrustedAndNonReady(
        string scaffoldRendererFlag,
        bool? machineBackedLocal)
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        Directory.CreateDirectory(tempRoot);

        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            artifactDbRoot: null,
            machineBackedLocal: machineBackedLocal,
            scaffoldRenderer: true,
            scaffoldRendererFlag: scaffoldRendererFlag);
        Assert.Contains(scaffoldRendererFlag, process.StartInfo.ArgumentList);
        Assert.DoesNotContain("--artifact-db-root", process.StartInfo.ArgumentList);
        if (machineBackedLocal is null)
            AssertServerProcessOmittedMachineModeFlags(process);
        else
            Assert.Contains("--no-machine-backed-local", process.StartInfo.ArgumentList);
        try
        {
            using var readyDocument = await WaitForReadyFileAsync(process, readyFile, TimeSpan.FromSeconds(75));
            var readyRoot = readyDocument.RootElement;
            var routePaths = ReadStringArray(readyRoot, "routePaths");
            var expectedNonTrainingRoutePaths =
                DesignConversionModelServerAspNetHostStartup.NonTrainingLocalRouteSet();

            Assert.False(readyRoot.GetProperty("machineBackedLocal").GetBoolean());
            Assert.False(readyRoot.GetProperty("fullLocalRouteSet").GetBoolean());
            Assert.Equal(
                expectedNonTrainingRoutePaths,
                routePaths);
            Assert.DoesNotContain(DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, routePaths);
            AssertFlow38PublicFields(readyRoot, expectedMapped: true, expectedConfigured: false);
            AssertArtifactUploadPublicFields(readyRoot, expectedMapped: false);
            Assert.Equal(
                DeterministicLocalDesignConversionPngRenderProvider.ProviderKind,
                readyRoot.GetProperty("renderProviderKind").GetString());
            Assert.True(readyRoot.GetProperty("scaffoldRendererMode").GetBoolean());
            Assert.Equal("explicit_scaffold_mode", readyRoot.GetProperty("renderProviderStartupStatus").GetString());
            Assert.Equal(
                "SCAFFOLD_RENDERER_EXPLICITLY_REQUESTED",
                readyRoot.GetProperty("renderProviderNonReadyCode").GetString());
            Assert.Contains(
                "non-trusted",
                readyRoot.GetProperty("renderProviderNonReadyReason").GetString(),
                StringComparison.OrdinalIgnoreCase);
            AssertPublicRenderArtifactRootRef(readyRoot);
            Assert.False(readyRoot.GetProperty("browserRendererConfigured").GetBoolean());
            Assert.False(readyRoot.GetProperty("browserRendererRuntimeAvailable").GetBoolean());
            AssertPublicBrowserExecutableMetadata(readyRoot, configured: false, runtimeAvailable: false);
            AssertPublicHealthReadyStringsDoNotLeakLocalPaths(readyRoot, repoRoot, tempRoot);
            AssertServerOwnedLocalEvaluationOnlyWithoutExternalJudgeCarrier(readyRoot);
            Assert.False(readyRoot.GetProperty("machineBackedApiPngProofValid").GetBoolean());
            Assert.Equal("not_requested", readyRoot.GetProperty("machineBackedApiStartupStatus").GetString());
            Assert.Equal(
                "MACHINE_BACKED_LOCAL_NOT_REQUESTED",
                readyRoot.GetProperty("machineBackedApiNonReadyCode").GetString());
            Assert.False(readyRoot.GetProperty("productReady").GetBoolean());

            using var client = new HttpClient
            {
                BaseAddress = new Uri(readyRoot.GetProperty("baseUrl").GetString()!),
                Timeout = TimeSpan.FromSeconds(10)
            };
            await AssertNonTrainingPhysicalRouteSetAsync(client);
            using var healthResponse = await client.GetAsync("/__pass143/health");
            var healthJson = await healthResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
            using var healthDocument = JsonDocument.Parse(healthJson);
            var healthRoot = healthDocument.RootElement;
            var healthRoutePaths = ReadStringArray(healthRoot, "routePaths");
            Assert.Equal(
                expectedNonTrainingRoutePaths,
                healthRoutePaths);
            Assert.DoesNotContain(DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, healthRoutePaths);
            AssertFlow38PublicFields(healthRoot, expectedMapped: true, expectedConfigured: false);
            AssertArtifactUploadPublicFields(healthRoot, expectedMapped: false);
            AssertServerOwnedLocalEvaluationOnlyWithoutExternalJudgeCarrier(healthRoot);
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task NonMachineBackedLocal_Flow38QuarantinePersistsAndReplaysAcrossRestartWithoutTraining()
    {
        const string token = "flow38-live-local-quarantine-token-0123456789";
        const string eventId =
            "flow38-feedback:dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd";
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        var storePath = Path.Combine(
            tempRoot,
            "runtime-data",
            "flow38-quarantine",
            JsonlDesignConversionFlow38QuarantineStore.FileName);
        var environment = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DesignConversionModelServerFlow38QuarantineHandler.BearerTokenEnvironmentName] = token
        };
        var row = new DesignConversionFlow38QuarantinedSourceRow(
            eventId,
            "correlation-flow38-live-local",
            "tenant-flow38-live-local",
            "cycle-flow38-live-local",
            "pattern-flow38-live-local",
            "retrieval-flow38-live-local",
            "sha256:" + new string('1', 64),
            "artifact://flow38-live-local/chosen-output-ref",
            "sha256:" + new string('2', 64),
            "artifact://flow38-live-local/rejected-output-ref",
            "sha256:" + new string('3', 64),
            "client_unverified",
            "terms://flow38-live-local/pending",
            "license://flow38-live-local/pending",
            ["provenance://flow38-live-local/client-submission"],
            "redaction://flow38-live-local/refs-only",
            "client_supplied_unverified",
            "client_supplied_unverified",
            "QUARANTINE",
            "PersonalAdaptive",
            "adaptive_or_quarantine",
            SemanticQualityClaimed: false,
            DpoTrainReady: false);
        Directory.CreateDirectory(tempRoot);

        Process? firstProcess = null;
        Process? secondProcess = null;
        try
        {
            firstProcess = StartServerProcess(
                repoRoot,
                projectPath,
                readyFile,
                artifactDbRoot,
                machineBackedLocal: false,
                scaffoldRenderer: true,
                routeAdmissionEnabled: false,
                environment: environment);
            using var firstReady = await WaitForReadyFileAsync(
                firstProcess,
                readyFile,
                TimeSpan.FromSeconds(75));
            AssertNonTrainingFlow38Ready(firstReady.RootElement, token);

            using var firstClient = CreateLoopbackClient(firstReady.RootElement);
            using var firstResponse = await PostFlow38QuarantineAsync(firstClient, row, token);
            var firstJson = await firstResponse.Content.ReadAsStringAsync();
            Assert.DoesNotContain(token, firstJson, StringComparison.Ordinal);
            var firstSnapshot = AssertLiveLocalFlow38Response(
                firstResponse,
                firstJson,
                HttpStatusCode.Accepted,
                created: true);
            Assert.True(File.Exists(storePath));
            var firstStoreBytes = await File.ReadAllBytesAsync(storePath);
            Assert.DoesNotContain(token, Encoding.UTF8.GetString(firstStoreBytes), StringComparison.Ordinal);
            AssertLiveLocalFlow38Store(firstStoreBytes, row, firstSnapshot);

            await StopProcessAsync(firstProcess);
            firstProcess.Dispose();
            firstProcess = null;
            File.Delete(readyFile);

            secondProcess = StartServerProcess(
                repoRoot,
                projectPath,
                readyFile,
                artifactDbRoot,
                machineBackedLocal: false,
                scaffoldRenderer: true,
                routeAdmissionEnabled: false,
                environment: environment);
            using var secondReady = await WaitForReadyFileAsync(
                secondProcess,
                readyFile,
                TimeSpan.FromSeconds(75));
            AssertNonTrainingFlow38Ready(secondReady.RootElement, token);

            using var secondClient = CreateLoopbackClient(secondReady.RootElement);
            using var replayResponse = await PostFlow38QuarantineAsync(secondClient, row, token);
            var replayJson = await replayResponse.Content.ReadAsStringAsync();
            Assert.DoesNotContain(token, replayJson, StringComparison.Ordinal);
            var replaySnapshot = AssertLiveLocalFlow38Response(
                replayResponse,
                replayJson,
                HttpStatusCode.OK,
                created: false);
            Assert.Equal(firstSnapshot, replaySnapshot);
            var replayStoreBytes = await File.ReadAllBytesAsync(storePath);
            Assert.Equal(firstStoreBytes, replayStoreBytes);
            AssertLiveLocalFlow38Store(replayStoreBytes, row, replaySnapshot);

            await StopProcessAsync(secondProcess);
            secondProcess.Dispose();
            secondProcess = null;
        }
        finally
        {
            if (secondProcess is not null)
                await StopProcessAsync(secondProcess);
            if (firstProcess is not null)
                await StopProcessAsync(firstProcess);
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ExplicitNoMachineBackedLocal_UsesBrowserBackedFullRouteSetWithoutMachineBackedTraining()
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        Directory.CreateDirectory(tempRoot);

        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            artifactDbRoot,
            machineBackedLocal: false);
        Assert.Contains("--no-machine-backed-local", process.StartInfo.ArgumentList);
        try
        {
            using var readyDocument = await WaitForReadyFileAsync(process, readyFile, TimeSpan.FromSeconds(75));
            var readyRoot = readyDocument.RootElement;
            var routePaths = ReadStringArray(readyRoot, "routePaths");
            var expectedRoutePaths = FullLocalRouteSetWithArtifactUpload();
            var browserRuntimeAvailable = readyRoot.GetProperty("browserRendererRuntimeAvailable").GetBoolean();

            Assert.False(readyRoot.GetProperty("machineBackedLocal").GetBoolean());
            Assert.True(readyRoot.GetProperty("fullLocalRouteSet").GetBoolean());
            Assert.Equal(
                expectedRoutePaths.Order(StringComparer.Ordinal),
                routePaths.Order(StringComparer.Ordinal));
            AssertArtifactUploadPublicFields(readyRoot, expectedMapped: true);
            Assert.Equal(
                BrowserBackedDesignConversionRenderProvider.ProviderKind,
                readyRoot.GetProperty("renderProviderKind").GetString());
            Assert.False(readyRoot.GetProperty("scaffoldRendererMode").GetBoolean());
            Assert.True(readyRoot.GetProperty("browserRendererConfigured").GetBoolean());
            AssertPublicBrowserExecutableMetadata(readyRoot, configured: true, runtimeAvailable: browserRuntimeAvailable);
            Assert.Equal(
                browserRuntimeAvailable ? "browser_runtime_available" : "bounded_non_ready",
                readyRoot.GetProperty("renderProviderStartupStatus").GetString());
            Assert.Equal(
                browserRuntimeAvailable ? "none" : "BROWSER_RUNTIME_UNAVAILABLE",
                readyRoot.GetProperty("renderProviderNonReadyCode").GetString());
            Assert.Equal(0, readyRoot.GetProperty("machineTrainingRows").GetInt32());
            Assert.Equal(0, readyRoot.GetProperty("learnedWeightArtifactCount").GetInt32());
            Assert.False(readyRoot.GetProperty("machineBackedApiFactoryCaller").GetBoolean());
            Assert.Equal("not_requested", readyRoot.GetProperty("machineBackedApiStartupStatus").GetString());
            Assert.Equal(
                "MACHINE_BACKED_LOCAL_NOT_REQUESTED",
                readyRoot.GetProperty("machineBackedApiNonReadyCode").GetString());
            Assert.True(readyRoot.GetProperty("localArtifactDbConfigured").GetBoolean());
            AssertPublicRenderArtifactRootRef(readyRoot);
            AssertPublicHealthReadyStringsDoNotLeakLocalPaths(readyRoot, repoRoot, tempRoot);
            Assert.DoesNotContain(
                DeterministicLocalDesignConversionPngRenderProvider.ProviderKind,
                readyRoot.GetRawText(),
                StringComparison.Ordinal);
            Assert.False(readyRoot.GetProperty("productReady").GetBoolean());

            using var client = new HttpClient
            {
                BaseAddress = new Uri(readyRoot.GetProperty("baseUrl").GetString()!),
                Timeout = TimeSpan.FromSeconds(10)
            };
            using var healthResponse = await client.GetAsync("/__pass143/health");
            var healthJson = await healthResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
            using var healthDocument = JsonDocument.Parse(healthJson);
            var healthRoot = healthDocument.RootElement;
            var healthRoutePaths = ReadStringArray(healthRoot, "routePaths");
            Assert.Equal(
                expectedRoutePaths.Order(StringComparer.Ordinal),
                healthRoutePaths.Order(StringComparer.Ordinal));
            AssertArtifactUploadPublicFields(healthRoot, expectedMapped: true);
            Assert.Equal(
                BrowserBackedDesignConversionRenderProvider.ProviderKind,
                healthRoot.GetProperty("renderProviderKind").GetString());
            Assert.False(healthRoot.GetProperty("scaffoldRendererMode").GetBoolean());
            Assert.DoesNotContain(
                DeterministicLocalDesignConversionPngRenderProvider.ProviderKind,
                healthJson,
                StringComparison.Ordinal);
            await AssertHumanApprovalChallengeRouteConfiguredAsync(client, tempRoot);
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Theory(Timeout = 60000)]
    [InlineData(null)]
    [InlineData(false)]
    public async Task NonScaffoldLiveLocal_WithoutArtifactDbRoot_FailsClosedBeforeReadyFileAndDoesNotCreateRandomTempArtifactDb(
        bool? machineBackedLocal)
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var isolatedProcessTempRoot = Path.Combine(tempRoot, "child-process-temp");
        var isolatedProcessEnvironment = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["TEMP"] = isolatedProcessTempRoot,
            ["TMP"] = isolatedProcessTempRoot,
            ["TMPDIR"] = isolatedProcessTempRoot
        };
        Directory.CreateDirectory(tempRoot);
        Directory.CreateDirectory(isolatedProcessTempRoot);
        AssertNoLiveLocalRandomArtifactDbRoots(isolatedProcessTempRoot);
        AssertNoLiveLocalRandomArtifactDbFiles(isolatedProcessTempRoot);

        Process? process = null;
        try
        {
            process = StartServerProcess(
                repoRoot,
                projectPath,
                readyFile,
                artifactDbRoot: null,
                machineBackedLocal: machineBackedLocal,
                environment: isolatedProcessEnvironment);
            if (machineBackedLocal is null)
                AssertServerProcessOmittedMachineModeFlags(process);
            else
                Assert.Contains("--no-machine-backed-local", process.StartInfo.ArgumentList);
            Assert.DoesNotContain("--artifact-db-root", process.StartInfo.ArgumentList);

            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
            Assert.Equal(64, process.ExitCode);
            Assert.False(File.Exists(readyFile));

            var processOutput = DescribeProcessOutput(process);
            Assert.Contains("DESIGN_CONVERSION_LIVE_LOCAL_ARTIFACT_DB_ROOT_REQUIRED", processOutput, StringComparison.Ordinal);
            Assert.Contains("--artifact-db-root", processOutput, StringComparison.Ordinal);
            Assert.Contains("durable local artifact DB", processOutput, StringComparison.Ordinal);
            Assert.Contains("scaffold renderer mode", processOutput, StringComparison.Ordinal);
            Assert.DoesNotContain(tempRoot, processOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(isolatedProcessTempRoot, processOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(readyFile, processOutput, StringComparison.OrdinalIgnoreCase);

            AssertNoLiveLocalRandomArtifactDbRoots(isolatedProcessTempRoot);
            AssertNoLiveLocalRandomArtifactDbFiles(isolatedProcessTempRoot);
        }
        finally
        {
            if (process is not null)
                await StopProcessAsync(process);
            process?.Dispose();
            AssertNoLiveLocalRandomArtifactDbRoots(isolatedProcessTempRoot);
            AssertNoLiveLocalRandomArtifactDbFiles(isolatedProcessTempRoot);
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact(Timeout = 60000)]
    public async Task ScaffoldLiveLocal_WithExplicitInvalidArtifactDbRoot_FailsClosedBeforeReadyAndDoesNotLeakPath()
    {
        const string unavailableCode = "DESIGN_CONVERSION_LIVE_LOCAL_ARTIFACT_DB_ROOT_UNAVAILABLE";
        const string safeStartupMessage =
            unavailableCode +
            ": DesignConversion durable local artifact DB startup could not prove local persistence before live-local ready.";
        const string privateMarker = "PRIVATE_INVALID_ARTIFACT_DB_ROOT_SECRET";
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot =
            tempRoot + Path.DirectorySeparatorChar + privateMarker + "\"invalid-artifact-db-root";
        Directory.CreateDirectory(tempRoot);

        Process? process = null;
        try
        {
            process = StartServerProcess(
                repoRoot,
                projectPath,
                readyFile,
                artifactDbRoot,
                machineBackedLocal: false,
                scaffoldRenderer: true);
            Assert.Contains("--artifact-db-root", process.StartInfo.ArgumentList);
            Assert.Contains(artifactDbRoot, process.StartInfo.ArgumentList);
            Assert.Contains("--scaffold-renderer", process.StartInfo.ArgumentList);

            var deadline = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(30);
            while (!process.HasExited && !File.Exists(readyFile) && DateTimeOffset.UtcNow < deadline)
                await Task.Delay(250);

            Assert.True(
                process.HasExited,
                "Expected explicit invalid artifact DB root to fail closed during option parsing; " +
                $"ProcessExited={process.HasExited}; ReadyFileExists={File.Exists(readyFile)}; {DescribeProcessOutput(process)}");
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
            Assert.Equal(64, process.ExitCode);
            Assert.False(File.Exists(readyFile));

            var processOutput = DescribeProcessOutput(process);
            Assert.Contains(unavailableCode, processOutput, StringComparison.Ordinal);
            Assert.Contains(safeStartupMessage, processOutput, StringComparison.Ordinal);
            Assert.DoesNotContain("PASS143_SERVER_READY", processOutput, StringComparison.Ordinal);
            Assert.DoesNotContain(tempRoot, processOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(artifactDbRoot, processOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(readyFile, processOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(privateMarker, processOutput, StringComparison.Ordinal);
            Assert.DoesNotContain(nameof(ArgumentException), processOutput, StringComparison.Ordinal);
            Assert.DoesNotContain("Exception", processOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("StackTrace", processOutput, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (process is not null)
                await StopProcessAsync(process);
            process?.Dispose();
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact(Timeout = 60000)]
    public async Task NonScaffoldLiveLocal_WithArtifactDbRootThatIsAFile_FailsClosedBeforeReadyAndDoesNotLeakPath()
    {
        const string unavailableCode = "DESIGN_CONVERSION_LIVE_LOCAL_ARTIFACT_DB_ROOT_UNAVAILABLE";
        const string safeStartupMessage =
            unavailableCode +
            ": DesignConversion durable local artifact DB startup could not prove local persistence before live-local ready.";
        const string privateMarker = "PRIVATE_ARTIFACT_DB_ROOT_FILE_COLLISION_SECRET";
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        var collisionBytes = Encoding.UTF8.GetBytes(
            privateMarker + Environment.NewLine + "ordinary file blocking artifact DB root");
        var collisionHash = Convert.ToHexString(SHA256.HashData(collisionBytes)).ToLowerInvariant();
        Directory.CreateDirectory(tempRoot);
        await File.WriteAllBytesAsync(artifactDbRoot, collisionBytes);

        Process? process = null;
        try
        {
            process = StartServerProcess(
                repoRoot,
                projectPath,
                readyFile,
                artifactDbRoot,
                machineBackedLocal: false);
            Assert.Contains("--no-machine-backed-local", process.StartInfo.ArgumentList);
            Assert.Contains("--artifact-db-root", process.StartInfo.ArgumentList);

            var deadline = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(30);
            while (!process.HasExited && !File.Exists(readyFile) && DateTimeOffset.UtcNow < deadline)
                await Task.Delay(250);

            Assert.True(
                process.HasExited,
                "Expected artifact DB root file collision to fail closed before ready; " +
                $"ProcessExited={process.HasExited}; ReadyFileExists={File.Exists(readyFile)}; {DescribeProcessOutput(process)}");
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
            Assert.Equal(64, process.ExitCode);
            Assert.False(File.Exists(readyFile));

            var processOutput = DescribeProcessOutput(process);
            Assert.DoesNotContain("PASS143_SERVER_READY", processOutput, StringComparison.Ordinal);
            Assert.Contains(safeStartupMessage, processOutput, StringComparison.Ordinal);
            Assert.DoesNotContain(tempRoot, processOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(artifactDbRoot, processOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(readyFile, processOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(privateMarker, processOutput, StringComparison.Ordinal);
            Assert.DoesNotContain("Exception", processOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("StackTrace", processOutput, StringComparison.OrdinalIgnoreCase);

            var collisionReadback = await File.ReadAllBytesAsync(artifactDbRoot);
            Assert.Equal(collisionBytes, collisionReadback);
            Assert.Equal(collisionHash, Convert.ToHexString(SHA256.HashData(collisionReadback)).ToLowerInvariant());
            Assert.Empty(Directory.EnumerateFiles(
                tempRoot,
                "design-conversion-artifact-db-startup-probe-*",
                SearchOption.TopDirectoryOnly));
        }
        finally
        {
            if (process is not null)
                await StopProcessAsync(process);
            process?.Dispose();
            if (File.Exists(artifactDbRoot))
            {
                var collisionReadback = await File.ReadAllBytesAsync(artifactDbRoot);
                Assert.Equal(collisionBytes, collisionReadback);
                Assert.Equal(collisionHash, Convert.ToHexString(SHA256.HashData(collisionReadback)).ToLowerInvariant());
            }

            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task NonScaffoldLiveLocal_WithWritableArtifactDbRoot_ProvesDurabilityBeforeReadyAndRemovesStartupProbe()
    {
        const string privateMarker = "PRIVATE_WRITABLE_ARTIFACT_DB_ROOT_SECRET";
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, privateMarker, "artifact-db");
        Directory.CreateDirectory(tempRoot);

        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            artifactDbRoot,
            machineBackedLocal: false);
        Assert.Contains("--no-machine-backed-local", process.StartInfo.ArgumentList);
        Assert.Contains("--artifact-db-root", process.StartInfo.ArgumentList);
        try
        {
            using var readyDocument = await WaitForReadyFileAsync(process, readyFile, TimeSpan.FromSeconds(75));
            Assert.False(
                process.HasExited,
                $"Expected writable artifact DB startup to remain alive after ready; {DescribeProcessOutput(process)}");

            var readyRoot = readyDocument.RootElement;
            Assert.True(readyRoot.GetProperty("localArtifactDbConfigured").GetBoolean());
            Assert.False(readyRoot.GetProperty("productReady").GetBoolean());
            AssertPublicHealthReadyStringsDoNotLeakLocalPaths(readyRoot, repoRoot, tempRoot);
            AssertNoRouteAdmissionSecretLeakage(readyRoot.GetRawText(), privateMarker);
            Assert.True(Directory.Exists(artifactDbRoot));
            Assert.Empty(Directory.EnumerateFiles(
                artifactDbRoot,
                "design-conversion-artifact-db-startup-probe-*",
                SearchOption.TopDirectoryOnly));
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Fact]
    public void ConfiguredArtifactDbAutowriteStatusAssertion_On400_FailsWithBoundedSanitizedPublicDiagnostic()
    {
        const string privateMarker = "PRIVATE_CONFIGURED_ARTIFACT_DB_DIAGNOSTIC_SECRET";
        const string privatePath = @"C:\private\artifact-db\ready.json";
        const string rawBodyMarker = "RAW_RESPONSE_BODY_PRIVATE_CONTENT";
        const string unselectedMetadataMarker = "UNSELECTED_METADATA_PRIVATE_CONTENT";
        const string safeErrorCode = LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode;
        var envelope = new DesignConversionModelServerLocalCallerAdapterResponse(
            EnvelopeSchemaVersion: "test-v1",
            EnvelopeBoundary: "test",
            EnvelopeKind: "error",
            AdapterBoundary: "test",
            CallerSourceKind: "test",
            ContractSchemaVersion: "test-v1",
            RequiredRequestFields: Array.Empty<string>(),
            EndpointMethod: "POST",
            EndpointPath: "/test",
            StatusCode: 400,
            RegistryInvokeSucceeded: true,
            RuntimeInvoked: false,
            ParsedResponseJsonSucceeded: true,
            ResponseJsonParseError: $"{privateMarker} parse-error {privatePath}",
            ParsedContractStatus: "rejected",
            ParsedRequestId: "diagnostic-request",
            ParsedJobId: null,
            ParsedErrorCode: safeErrorCode,
            ParsedErrorMessage: $"{privateMarker} parsed-error-message {privatePath}",
            ResponseBodyJson: $"{{\"private\":\"{privateMarker}\",\"raw\":\"{rawBodyMarker}\",\"path\":\"{privatePath}\"}}",
            RawHtmlIncluded: false,
            RawCssIncluded: false,
            RawDesignTreeIncluded: false,
            ResponseContainsRawPrivateData: true,
            RawPrivateResponseLeakageDetected: true,
            ExternalCallerRepoTouched: false,
            CrossRepoCallerVerified: false,
            PublicHttpEndpointVerified: false,
            BoundaryMetadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["http_status_code"] = "400",
                ["runtime_invoked"] = bool.FalseString,
                ["route_admission_error_ref"] = "safe-route-admission-ref",
                ["handler_failure_kind"] = "safe_handler_failure",
                ["multi_viewport_complete"] = bool.FalseString,
                ["browser_render_executed"] = bool.FalseString,
                ["unselected_private_metadata"] = $"{privateMarker} {unselectedMetadataMarker} {privatePath}"
            });

        var failure = Record.Exception(
            () => AssertConfiguredArtifactDbAutowriteEnvelope(envelope, "diagnostic-request"));
        var assertionFailure = Assert.IsAssignableFrom<Xunit.Sdk.XunitException>(failure);
        var message = assertionFailure.Message;

        Assert.Contains("status=400", message, StringComparison.Ordinal);
        Assert.Contains($"parsed_error_code={safeErrorCode}", message, StringComparison.Ordinal);
        Assert.Contains("metadata.http_status_code=400", message, StringComparison.Ordinal);
        Assert.Contains(
            "metadata.route_admission_error_ref=safe-route-admission-ref",
            message,
            StringComparison.Ordinal);
        Assert.Contains(
            "metadata.handler_failure_kind=safe_handler_failure",
            message,
            StringComparison.Ordinal);
        Assert.Contains("parsed_browser_failure_detail=<redacted>", message, StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, message, StringComparison.Ordinal);
        Assert.DoesNotContain(rawBodyMarker, message, StringComparison.Ordinal);
        Assert.DoesNotContain(unselectedMetadataMarker, message, StringComparison.Ordinal);
        Assert.DoesNotContain("unselected_private_metadata", message, StringComparison.Ordinal);
        Assert.DoesNotContain(privatePath, message, StringComparison.OrdinalIgnoreCase);
        Assert.True(message.Length <= 1024, $"Expected bounded diagnostic; length={message.Length}.");

        var pathFreeArbitraryDetail =
            $"Browser screenshot process failed with arbitrary private marker {privateMarker}.";
        var pathFreeArbitraryEnvelope = envelope with { ParsedErrorMessage = pathFreeArbitraryDetail };
        var pathFreeArbitraryFailure = Record.Exception(
            () => AssertConfiguredArtifactDbAutowriteEnvelope(
                pathFreeArbitraryEnvelope,
                "diagnostic-request"));
        var pathFreeArbitraryAssertionFailure =
            Assert.IsAssignableFrom<Xunit.Sdk.XunitException>(pathFreeArbitraryFailure);
        Assert.Contains(
            "parsed_browser_failure_detail=<redacted>",
            pathFreeArbitraryAssertionFailure.Message,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            privateMarker,
            pathFreeArbitraryAssertionFailure.Message,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            pathFreeArbitraryDetail,
            pathFreeArbitraryAssertionFailure.Message,
            StringComparison.Ordinal);

        var stderrHash = new string('a', 64);
        var stdoutHash = new string('b', 64);
        string BuildProcessFailureDetail(string exitCode, string stderrChars, string stdoutChars)
            => $"Browser screenshot process failed with exit code {exitCode}. " +
               $"stderr_sha256={stderrHash} stderr_chars={stderrChars} " +
               $"stdout_sha256={stdoutHash} stdout_chars={stdoutChars}";

        var safeProcessFailureDetail = BuildProcessFailureDetail("-9", "321", "654");
        var safeProcessEnvelope = envelope with { ParsedErrorMessage = safeProcessFailureDetail };
        var safeProcessFailure = Record.Exception(
            () => AssertConfiguredArtifactDbAutowriteEnvelope(safeProcessEnvelope, "diagnostic-request"));
        var safeProcessAssertionFailure =
            Assert.IsAssignableFrom<Xunit.Sdk.XunitException>(safeProcessFailure);
        var safeProcessMessage = safeProcessAssertionFailure.Message;

        Assert.Contains(
            $"parsed_browser_failure_detail={safeProcessFailureDetail}",
            safeProcessMessage,
            StringComparison.Ordinal);
        Assert.Contains("exit code -9", safeProcessMessage, StringComparison.Ordinal);
        Assert.Contains($"stderr_sha256={stderrHash}", safeProcessMessage, StringComparison.Ordinal);
        Assert.Contains("stderr_chars=321", safeProcessMessage, StringComparison.Ordinal);
        Assert.Contains($"stdout_sha256={stdoutHash}", safeProcessMessage, StringComparison.Ordinal);
        Assert.Contains("stdout_chars=654", safeProcessMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, safeProcessMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(rawBodyMarker, safeProcessMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(unselectedMetadataMarker, safeProcessMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(privatePath, safeProcessMessage, StringComparison.OrdinalIgnoreCase);
        Assert.True(
            safeProcessMessage.Length <= 1024,
            $"Expected bounded browser process diagnostic; length={safeProcessMessage.Length}.");

        const string nonScreenshotFailureCode = "SAFE_NON_SCREENSHOT_FAILURE";
        var nonScreenshotFailureEnvelope = envelope with
        {
            ParsedErrorCode = nonScreenshotFailureCode,
            ParsedErrorMessage = safeProcessFailureDetail
        };
        var nonScreenshotFailure = Record.Exception(
            () => AssertConfiguredArtifactDbAutowriteEnvelope(
                nonScreenshotFailureEnvelope,
                "diagnostic-request"));
        var nonScreenshotAssertionFailure =
            Assert.IsAssignableFrom<Xunit.Sdk.XunitException>(nonScreenshotFailure);

        Assert.Contains(
            $"parsed_error_code={nonScreenshotFailureCode}",
            nonScreenshotAssertionFailure.Message,
            StringComparison.Ordinal);
        Assert.Contains(
            "parsed_browser_failure_detail=<redacted>",
            nonScreenshotAssertionFailure.Message,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            safeProcessFailureDetail,
            nonScreenshotAssertionFailure.Message,
            StringComparison.Ordinal);

        var malformedProcessFailureDetails = new[]
        {
            BuildProcessFailureDetail("-0", "321", "654"),
            BuildProcessFailureDetail("-9", "0321", "654"),
            BuildProcessFailureDetail("2147483648", "321", "654"),
            BuildProcessFailureDetail("-9", "321", "2147483648")
        };
        foreach (var malformedProcessFailureDetail in malformedProcessFailureDetails)
        {
            var malformedEnvelope = envelope with { ParsedErrorMessage = malformedProcessFailureDetail };
            var malformedFailure = Record.Exception(
                () => AssertConfiguredArtifactDbAutowriteEnvelope(
                    malformedEnvelope,
                    "diagnostic-request"));
            var malformedAssertionFailure =
                Assert.IsAssignableFrom<Xunit.Sdk.XunitException>(malformedFailure);

            Assert.Contains(
                "parsed_browser_failure_detail=<redacted>",
                malformedAssertionFailure.Message,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                malformedProcessFailureDetail,
                malformedAssertionFailure.Message,
                StringComparison.Ordinal);
        }

        const string safeExceptionFailureDetail =
            "Browser screenshot process failed with sanitized exception type InvalidOperationException.";
        var safeExceptionEnvelope = envelope with { ParsedErrorMessage = safeExceptionFailureDetail };
        var safeExceptionFailure = Record.Exception(
            () => AssertConfiguredArtifactDbAutowriteEnvelope(safeExceptionEnvelope, "diagnostic-request"));
        var safeExceptionAssertionFailure =
            Assert.IsAssignableFrom<Xunit.Sdk.XunitException>(safeExceptionFailure);
        Assert.Contains(
            $"parsed_browser_failure_detail={safeExceptionFailureDetail}",
            safeExceptionAssertionFailure.Message,
            StringComparison.Ordinal);
    }

    [Fact(Timeout = 180000)]
    public async Task DefaultLocal_WithExplicitStableArtifactDbRoot_PersistsOnceAndFreshServerReadsBackWithoutReplayAppend()
    {
        const string privateMarker = "PRIVATE_N8_14_CONFIGURED_ARTIFACT_DB_SECRET";
        const string requestId = "n8-14-configured-persistence-submit";
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile1 = Path.Combine(tempRoot, "ready-1.json");
        var readyFile2 = Path.Combine(tempRoot, "ready-2.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        var stableArtifactRoot = Path.Combine(tempRoot, "render-artifacts");
        var recordsFile = Path.Combine(artifactDbRoot, "conversion-results.jsonl");
        Directory.CreateDirectory(tempRoot);

        Process? process1 = null;
        Process? process2 = null;
        try
        {
            var packageFixture = await CreateConfiguredReadyJsonSubmitPackageAsync(
                privateMarker,
                stableArtifactRoot,
                requestId: requestId);
            var activeCandidate = await CreateLiveActiveModelCandidateAsync(
                tempRoot,
                "configured-artifact-db-active");

            process1 = StartServerProcess(
                repoRoot,
                projectPath,
                readyFile1,
                artifactDbRoot,
                machineBackedLocal: false,
                environment: ModelLifecycleEnvironment());
            using var readyDocument1 = await WaitForReadyFileAsync(process1, readyFile1, TimeSpan.FromSeconds(75));
            var readyRoot1 = readyDocument1.RootElement;
            var readyBaseUrl1 = new Uri(readyRoot1.GetProperty("baseUrl").GetString()!);
            Assert.True(readyRoot1.GetProperty("localArtifactDbConfigured").GetBoolean());
            Assert.False(readyRoot1.GetProperty("productReady").GetBoolean());
            AssertPublicRenderArtifactRootRef(readyRoot1);
            AssertBrowserRendererPngProofPublicFields(
                readyRoot1,
                expectedRequired: false,
                expectedAttempted: false,
                expectedValid: false,
                expectedStartupStatus: "not_required",
                expectedNonReadyCode: "BROWSER_RENDERER_PNG_PROOF_NOT_REQUIRED",
                expectedEvidenceCount: 0,
                expectedRequiredArtifactCount: 0,
                expectedRequiredViewportCount: 0,
                expectedObservedViewportCount: 0);
            AssertPublicHealthReadyStringsDoNotLeakLocalPaths(readyRoot1, repoRoot, tempRoot);

            using var serverOwnedArtifactRewriteHandler1 = new ServerOwnedArtifactRefRewriteHandler(new HttpClientHandler());
            using var httpClient1 = new HttpClient(serverOwnedArtifactRewriteHandler1)
            {
                BaseAddress = readyBaseUrl1,
                Timeout = LiveBrowserBackedSubmitTimeout
            };
            await ActivateLiveModelAsync(
                httpClient1,
                activeCandidate,
                "activate:configured-artifact-db-active");
            var uploadedPackage = await UploadPackageScreenshotsForLiveSubmitAsync(
                httpClient1,
                readyRoot1.GetProperty("artifactUploadEndpointPath").GetString()!,
                packageFixture,
                serverOwnedArtifactRewriteHandler1,
                idempotencyKeyPrefix: "n8-14-configured-upload-1");
            Assert.All(uploadedPackage.Screenshots!, screenshot =>
                Assert.StartsWith("sha256:", screenshot.Uri, StringComparison.OrdinalIgnoreCase));

            var submitClient1 = new DesignConversionModelServerReadyJsonSubmitClient(httpClient1);
            var firstResult = await submitClient1.TrySubmitReadyFileAsync(
                readyFile1,
                uploadedPackage,
                routeAdmissionToken: RouteAdmissionCurrentToken,
                idempotencyKey: "n8-14-configured-submit-1");
            Assert.True(firstResult.IsSuccess, firstResult.ErrorMessage);
            var firstEnvelope = firstResult.Value!.Envelope;
            AssertConfiguredArtifactDbAutowriteEnvelope(firstEnvelope, requestId);
            var jobId = Assert.IsType<string>(firstEnvelope.ParsedJobId);
            Assert.False(string.IsNullOrWhiteSpace(jobId));
            Assert.Equal(1, await CountNonEmptyLinesAsync(recordsFile));
            Assert.Equal(1, await CountPersistedRecordsAsync(recordsFile, requestId));

            await StopProcessAsync(process1);
            process1.Dispose();
            process1 = null;

            process2 = StartServerProcess(
                repoRoot,
                projectPath,
                readyFile2,
                artifactDbRoot,
                machineBackedLocal: false,
                routeAdmissionEnabled: false);
            using var readyDocument2 = await WaitForReadyFileAsync(process2, readyFile2, TimeSpan.FromSeconds(75));
            var readyRoot2 = readyDocument2.RootElement;
            var readyBaseUrl2 = new Uri(readyRoot2.GetProperty("baseUrl").GetString()!);
            Assert.True(readyRoot2.GetProperty("localArtifactDbConfigured").GetBoolean());
            Assert.False(readyRoot2.GetProperty("productReady").GetBoolean());
            AssertPublicRenderArtifactRootRef(readyRoot2);
            AssertBrowserRendererPngProofPublicFields(
                readyRoot2,
                expectedRequired: false,
                expectedAttempted: false,
                expectedValid: false,
                expectedStartupStatus: "not_required",
                expectedNonReadyCode: "BROWSER_RENDERER_PNG_PROOF_NOT_REQUIRED",
                expectedEvidenceCount: 0,
                expectedRequiredArtifactCount: 0,
                expectedRequiredViewportCount: 0,
                expectedObservedViewportCount: 0);
            AssertPublicHealthReadyStringsDoNotLeakLocalPaths(readyRoot2, repoRoot, tempRoot);

            using var httpClient2 = new HttpClient
            {
                BaseAddress = readyBaseUrl2,
                Timeout = LiveBrowserBackedSubmitTimeout
            };
            string statusJson;
            using (var statusResponse = await GetAuthorizedAsync(
                       httpClient2,
                       readyBaseUrl2,
                       DesignConversionModelServerStatusHandler.EndpointPathPrefix + jobId,
                       token: string.Empty))
            {
                statusJson = await statusResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);
            }

            using (var statusDocument = JsonDocument.Parse(statusJson))
            {
                var statusRoot = statusDocument.RootElement;
                Assert.Equal(jobId, statusRoot.GetProperty("jobId").GetString());
                Assert.Equal(requestId, statusRoot.GetProperty("requestId").GetString());
                Assert.Equal(DesignConversionThinClientStatus.Completed, statusRoot.GetProperty("contractStatus").GetString());
                Assert.False(statusRoot.GetProperty("productReady").GetBoolean());
                var statusMetadata = statusRoot.GetProperty("boundaryMetadata");
                Assert.Equal(bool.TrueString, statusMetadata.GetProperty("local_artifact_db_status_restored").GetString());
                Assert.Equal(bool.FalseString, statusMetadata.GetProperty("status_restore_executed_runtime").GetString());
                Assert.Equal(
                    JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
                    statusMetadata.GetProperty("status_restore_source").GetString());

                var artifactKinds = statusRoot.GetProperty("artifacts")
                    .EnumerateArray()
                    .Select(artifact => artifact.GetProperty("kind").GetString())
                    .ToArray();
                Assert.Contains("improved-html", artifactKinds);
                Assert.Contains("improved-css", artifactKinds);
                Assert.Equal(2, artifactKinds.Count(kind => string.Equals(
                    kind,
                    "browser-render-screenshot",
                    StringComparison.Ordinal)));
            }

            var freshArtifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
            var freshReadback = await freshArtifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: jobId, RequestId: requestId));
            Assert.True(freshReadback.IsSuccess, freshReadback.ErrorMessage);
            var persistedRecord = Assert.Single(freshReadback.Value!);
            Assert.Equal(jobId, persistedRecord.JobId);
            Assert.Equal(requestId, persistedRecord.RequestId);
            Assert.StartsWith("dcm-html_result-", persistedRecord.ConversionResult.HtmlRef, StringComparison.Ordinal);
            Assert.StartsWith("dcm-css_result-", persistedRecord.ConversionResult.CssRef, StringComparison.Ordinal);
            Assert.Equal(bool.TrueString, persistedRecord.Metadata["local_artifact_db_persistence_verified"]);
            Assert.Equal(
                nameof(DesignConversionThinClientJobRuntime),
                persistedRecord.Metadata["artifact_db_single_writer_owner"]);

            Assert.Equal(1, await CountNonEmptyLinesAsync(recordsFile));
            Assert.Equal(1, await CountPersistedRecordsAsync(recordsFile, requestId));
            AssertNoConfiguredArtifactDbPublicLeakage(
                [readyRoot1.GetRawText(), firstEnvelope.ResponseBodyJson, readyRoot2.GetRawText(), statusJson],
                tempRoot,
                artifactDbRoot,
                stableArtifactRoot,
                recordsFile,
                privateMarker,
                uploadedPackage.Html!,
                uploadedPackage.Css!,
                uploadedPackage.DesignTree!.Payload!,
                uploadedPackage.OriginalTree!.Payload!);
        }
        finally
        {
            if (process1 is not null)
                await StopProcessAsync(process1);
            if (process2 is not null)
                await StopProcessAsync(process2);
            process1?.Dispose();
            process2?.Dispose();
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task MachineBackedLocal_PngProofMaterializationFailureWritesBoundedNonReadyWithoutProductReady()
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        var blockedRenderArtifactRoot = Path.Combine(tempRoot, "render-artifacts");
        Directory.CreateDirectory(tempRoot);
        File.WriteAllText(
            blockedRenderArtifactRoot,
            "This file blocks only render-artifacts directory creation.",
            Encoding.UTF8);

        using var process = StartServerProcess(repoRoot, projectPath, readyFile, artifactDbRoot);
        try
        {
            using var readyDocument = await WaitForReadyFileAsync(process, readyFile, TimeSpan.FromSeconds(75));
            var readyRoot = readyDocument.RootElement;
            var readyJson = readyRoot.GetRawText();
            var routePaths = ReadStringArray(readyRoot, "routePaths");
            var expectedRoutePaths = FullLocalRouteSetWithoutArtifactUpload();

            Assert.True(readyRoot.GetProperty("machineBackedLocal").GetBoolean());
            Assert.True(readyRoot.GetProperty("fullLocalRouteSet").GetBoolean());
            Assert.Equal(
                expectedRoutePaths.Order(StringComparer.Ordinal),
                routePaths.Order(StringComparer.Ordinal));
            Assert.DoesNotContain(DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, routePaths);
            AssertArtifactUploadPublicFields(
                readyRoot,
                expectedMapped: false,
                expectedStartupStatus: "bounded_non_ready",
                expectedNonReadyCode: "DESIGN_CONVERSION_ARTIFACT_UPLOAD_STARTUP_FAILED",
                expectedNonReadyReason: "DesignConversion artifact upload startup could not prepare safe local storage; upload route was not mapped.");
            AssertPublicRenderArtifactRootRef(readyRoot);
            AssertPublicBrowserExecutableMetadata(readyRoot, configured: true, runtimeAvailable: true);
            AssertPublicHealthReadyStringsDoNotLeakLocalPaths(readyRoot, repoRoot, tempRoot);
            Assert.DoesNotContain("ArgumentException", readyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("This file blocks", readyJson, StringComparison.Ordinal);
            Assert.True(readyRoot.GetProperty("machineBackedApiFactoryCaller").GetBoolean());
            Assert.False(readyRoot.GetProperty("machineBackedApiRuntimeProbeAttempted").GetBoolean());
            Assert.False(readyRoot.GetProperty("machineBackedApiRuntimeProbeExecuted").GetBoolean());
            Assert.False(readyRoot.GetProperty("machineBackedApiLearnedInferenceApplied").GetBoolean());
            Assert.False(readyRoot.GetProperty("machineBackedApiProductReady").GetBoolean());
            Assert.False(readyRoot.GetProperty("machineBackedApiPngProofValid").GetBoolean());
            AssertBrowserRendererPngProofPublicFields(
                readyRoot,
                expectedRequired: true,
                expectedAttempted: true,
                expectedValid: false,
                expectedStartupStatus: "bounded_non_ready",
                expectedNonReadyCode: "MACHINE_BACKED_API_PNG_MATERIALIZATION_FAILED",
                expectedEvidenceCount: 0,
                expectedRequiredArtifactCount: 4,
                expectedRequiredViewportCount: 2,
                expectedObservedViewportCount: 0);
            Assert.Empty(readyRoot.GetProperty("machineBackedApiProbeArtifactEvidence").EnumerateArray());
            Assert.Equal("bounded_non_ready", readyRoot.GetProperty("machineBackedApiStartupStatus").GetString());
            Assert.Equal(
                "MACHINE_BACKED_API_PNG_MATERIALIZATION_FAILED",
                readyRoot.GetProperty("machineBackedApiNonReadyCode").GetString());
            Assert.Equal(
                "DesignConversion machine-backed local PNG proof materialization failed without crashing startup.",
                readyRoot.GetProperty("machineBackedApiNonReadyReason").GetString());
            Assert.DoesNotContain(
                DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
                ReadStringArray(readyRoot, "routePaths"));
            Assert.False(readyRoot.GetProperty("productReady").GetBoolean());

            using var client = new HttpClient
            {
                BaseAddress = new Uri(readyRoot.GetProperty("baseUrl").GetString()!),
                Timeout = TimeSpan.FromSeconds(10)
            };
            using var healthResponse = await client.GetAsync("/__pass143/health");
            var healthJson = await healthResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
            using var healthDocument = JsonDocument.Parse(healthJson);
            var healthRoot = healthDocument.RootElement;
            var healthRoutePaths = ReadStringArray(healthRoot, "routePaths");
            Assert.Equal(
                expectedRoutePaths.Order(StringComparer.Ordinal),
                healthRoutePaths.Order(StringComparer.Ordinal));
            Assert.DoesNotContain(DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, healthRoutePaths);
            AssertArtifactUploadPublicFields(
                healthRoot,
                expectedMapped: false,
                expectedStartupStatus: "bounded_non_ready",
                expectedNonReadyCode: "DESIGN_CONVERSION_ARTIFACT_UPLOAD_STARTUP_FAILED",
                expectedNonReadyReason: "DesignConversion artifact upload startup could not prepare safe local storage; upload route was not mapped.");
            AssertBrowserRendererPngProofPublicFields(
                healthRoot,
                expectedRequired: true,
                expectedAttempted: true,
                expectedValid: false,
                expectedStartupStatus: "bounded_non_ready",
                expectedNonReadyCode: "MACHINE_BACKED_API_PNG_MATERIALIZATION_FAILED",
                expectedEvidenceCount: 0,
                expectedRequiredArtifactCount: 4,
                expectedRequiredViewportCount: 2,
                expectedObservedViewportCount: 0);
            AssertPublicHealthReadyStringsDoNotLeakLocalPaths(healthRoot, repoRoot, tempRoot);
            Assert.DoesNotContain("ArgumentException", healthJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("This file blocks", healthJson, StringComparison.Ordinal);

            var uploadProbeBytes = new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };
            using var uploadProbeRequest = new HttpRequestMessage(
                HttpMethod.Post,
                DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath)
            {
                Content = new ByteArrayContent(uploadProbeBytes)
            };
            uploadProbeRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            AddRouteAdmissionHeaders(
                uploadProbeRequest,
                RouteAdmissionCurrentToken,
                idempotencyKey: "live-local-upload-route-not-mapped-probe");
            uploadProbeRequest.Headers.TryAddWithoutValidation(
                DesignConversionModelServerArtifactUploadHttpHandler.ArtifactIdHeaderName,
                "live-local-upload-route-not-mapped-probe");
            uploadProbeRequest.Headers.TryAddWithoutValidation(
                DesignConversionModelServerArtifactUploadHttpHandler.ArtifactKindHeaderName,
                "screenshot");
            uploadProbeRequest.Headers.TryAddWithoutValidation(
                DesignConversionModelServerArtifactUploadHttpHandler.ExpectedSha256HeaderName,
                "sha256:" + Convert.ToHexString(SHA256.HashData(uploadProbeBytes)).ToLowerInvariant());
            uploadProbeRequest.Headers.TryAddWithoutValidation(
                DesignConversionModelServerArtifactUploadHttpHandler.WidthHeaderName,
                "1");
            uploadProbeRequest.Headers.TryAddWithoutValidation(
                DesignConversionModelServerArtifactUploadHttpHandler.HeightHeaderName,
                "1");
            using var uploadProbeResponse = await client.SendAsync(uploadProbeRequest);
            Assert.Equal(HttpStatusCode.NotFound, uploadProbeResponse.StatusCode);
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task MachineBackedLocal_ValidArtifactUploadRootMapsRouteAndPersistsUpload()
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        Directory.CreateDirectory(tempRoot);

        using var process = StartServerProcess(repoRoot, projectPath, readyFile, artifactDbRoot);
        try
        {
            using var readyDocument = await WaitForReadyFileAsync(process, readyFile, TimeSpan.FromSeconds(75));
            var readyRoot = readyDocument.RootElement;
            var routePaths = ReadStringArray(readyRoot, "routePaths");
            var expectedRoutePaths = FullLocalRouteSetWithArtifactUpload();
            var internalRenderArtifactRoot = Path.Combine(tempRoot, "render-artifacts");

            Assert.Equal(
                expectedRoutePaths.Order(StringComparer.Ordinal),
                routePaths.Order(StringComparer.Ordinal));
            AssertArtifactUploadPublicFields(readyRoot, expectedMapped: true);
            AssertMachineBackedPngProof(readyRoot, internalRenderArtifactRoot);
            AssertBrowserRendererPngProofPublicFields(
                readyRoot,
                expectedRequired: true,
                expectedAttempted: true,
                expectedValid: true,
                expectedStartupStatus: "png_proof_verified",
                expectedNonReadyCode: "none",
                expectedEvidenceCount: 4,
                expectedRequiredArtifactCount: 4,
                expectedRequiredViewportCount: 2,
                expectedObservedViewportCount: 2);
            AssertPublicHealthReadyStringsDoNotLeakLocalPaths(readyRoot, repoRoot, tempRoot);
            Assert.False(readyRoot.GetProperty("productReady").GetBoolean());

            using var client = new HttpClient
            {
                BaseAddress = new Uri(readyRoot.GetProperty("baseUrl").GetString()!),
                Timeout = TimeSpan.FromSeconds(10)
            };
            using var healthResponse = await client.GetAsync("/__pass143/health");
            var healthJson = await healthResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
            using var healthDocument = JsonDocument.Parse(healthJson);
            var healthRoot = healthDocument.RootElement;
            AssertArtifactUploadPublicFields(healthRoot, expectedMapped: true);
            AssertBrowserRendererPngProofPublicFields(
                healthRoot,
                expectedRequired: true,
                expectedAttempted: true,
                expectedValid: true,
                expectedStartupStatus: "png_proof_verified",
                expectedNonReadyCode: "none",
                expectedEvidenceCount: 4,
                expectedRequiredArtifactCount: 4,
                expectedRequiredViewportCount: 2,
                expectedObservedViewportCount: 2);
            AssertPublicHealthReadyStringsDoNotLeakLocalPaths(healthRoot, repoRoot, tempRoot);

            var uploadPng = SelectUploadPngFixture(internalRenderArtifactRoot);
            var ingressBeforeUpload = PublishedIngressPngFiles(internalRenderArtifactRoot).Length;
            Assert.Empty(TempFiles(internalRenderArtifactRoot));

            using var uploadResponse = await PostArtifactUploadAsync(
                client,
                readyRoot.GetProperty("artifactUploadEndpointPath").GetString()!,
                uploadPng.Bytes,
                uploadPng,
                artifactId: "live-local-machine-backed-upload",
                idempotencyKey: "live-local-machine-backed-valid-root-upload-key");
            var uploadJson = await uploadResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
            AssertLiveLocalArtifactUploadSuccess(uploadJson, uploadPng, internalRenderArtifactRoot);
            Assert.Equal(ingressBeforeUpload + 1, PublishedIngressPngFiles(internalRenderArtifactRoot).Length);
            Assert.Empty(TempFiles(internalRenderArtifactRoot));
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Fact(Timeout = 180000)]
    public async Task ReadyJsonSubmitClient_LiveLocalUploadThenEvaluate_UsesRealServerOwnedResolverWithoutRewriteHandler()
    {
        const string privateMarker = "PRIVATE_READY_JSON_LIVE_UPLOAD_RESOLVER_SECRET";
        const string requestId = "ready-json-live-upload-resolver";
        const string idempotencyKey = "ready-json-live-upload-resolver-key";
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var serverRoot = Path.Combine(tempRoot, "server");
        var readyFile = Path.Combine(serverRoot, "ready.json");
        var artifactDbRoot = Path.Combine(serverRoot, "artifact-db");
        var serverArtifactRoot = Path.Combine(serverRoot, "render-artifacts");
        var callerArtifactRoot = Path.Combine(tempRoot, "caller-local-artifacts");
        Directory.CreateDirectory(serverRoot);

        var package = CreateConfiguredReadyJsonSubmitPackage(
            privateMarker,
            includeMobileViewport: true,
            requestId: requestId);
        var renderer = new BrowserBackedDesignConversionRenderProvider(
            LocalChromiumBrowserProcessAdapter.CreateServerOwnedDefault(
                timeout: TimeSpan.FromSeconds(30),
                artifactRoot: callerArtifactRoot));
        var resolution = new RenderQaResolution("source-1440x900", 1440, 900, 1d);
        var mobileResolution = new RenderQaResolution("source-390x844", 390, 844, 3d);
        var screenshotFixture = await CreateConfiguredReadyJsonServerOwnedArtifactRefAsync(
            renderer,
            package,
            "ready-json-live-local-source-screenshot",
            "screenshot",
            resolution);
        var designRenderFixture = await CreateConfiguredReadyJsonServerOwnedArtifactRefAsync(
            renderer,
            package,
            "ready-json-live-local-design-render",
            "design-render",
            resolution);
        var mobileScreenshotFixture = await CreateConfiguredReadyJsonServerOwnedArtifactRefAsync(
            renderer,
            package,
            "ready-json-live-local-source-screenshot-mobile",
            "screenshot",
            mobileResolution);
        var mobileDesignRenderFixture = await CreateConfiguredReadyJsonServerOwnedArtifactRefAsync(
            renderer,
            package,
            "ready-json-live-local-design-render-mobile",
            "design-render",
            mobileResolution);
        var callerSourceUris = new[]
        {
            screenshotFixture.ServerOwnedFileUri,
            designRenderFixture.ServerOwnedFileUri,
            mobileScreenshotFixture.ServerOwnedFileUri,
            mobileDesignRenderFixture.ServerOwnedFileUri
        };
        var callerSourcePaths = callerSourceUris
            .Select(uri => new Uri(uri).LocalPath)
            .ToArray();
        var callerSourceBase64 = await Task.WhenAll(callerSourcePaths.Select(async path =>
            Convert.ToBase64String(await File.ReadAllBytesAsync(path))));
        package = package with
        {
            Screenshots =
            [
                screenshotFixture.PublicRef with { Uri = screenshotFixture.ServerOwnedFileUri },
                mobileScreenshotFixture.PublicRef with { Uri = mobileScreenshotFixture.ServerOwnedFileUri }
            ],
            DesignRenders =
            [
                designRenderFixture.PublicRef with { Uri = designRenderFixture.ServerOwnedFileUri },
                mobileDesignRenderFixture.PublicRef with { Uri = mobileDesignRenderFixture.ServerOwnedFileUri }
            ]
        };

        Assert.All(callerSourcePaths, sourcePath => Assert.False(
            Path.GetFullPath(sourcePath).StartsWith(
                Path.GetFullPath(serverArtifactRoot) + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase)));

        var activeCandidate = await CreateLiveActiveModelCandidateAsync(
            serverRoot,
            "ready-json-upload-resolver-active");
        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            artifactDbRoot,
            machineBackedLocal: false,
            environment: ModelLifecycleEnvironment());
        try
        {
            using var readyDocument = await WaitForReadyFileAsync(process, readyFile, TimeSpan.FromSeconds(75));
            var readyRoot = readyDocument.RootElement;
            var readyBaseUrl = new Uri(readyRoot.GetProperty("baseUrl").GetString()!);
            AssertReadyJsonBaseUrlForSubmit(readyRoot, readyBaseUrl);
            Assert.True(readyRoot.GetProperty("artifactUploadConfigured").GetBoolean());
            Assert.True(readyRoot.GetProperty("artifactUploadRouteMapped").GetBoolean());
            Assert.Contains(
                DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
                ReadStringArray(readyRoot, "routePaths"));
            Assert.Empty(PublishedIngressPngFiles(serverArtifactRoot));

            using (var activationClient = new HttpClient
                   {
                       BaseAddress = readyBaseUrl,
                       Timeout = LiveBrowserBackedSubmitTimeout
                   })
            {
                await ActivateLiveModelAsync(
                    activationClient,
                    activeCandidate,
                    "activate:ready-json-upload-resolver-active");
            }

            using var httpClient = new HttpClient
            {
                Timeout = LiveBrowserBackedSubmitTimeout
            };
            var submitClient = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
            var result = await submitClient.TrySubmitReadyFileAsync(
                readyFile,
                package,
                routeAdmissionToken: RouteAdmissionCurrentToken,
                idempotencyKey: idempotencyKey);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            var submitted = result.Value!;
            AssertConfiguredArtifactDbAutowriteEnvelope(submitted.Envelope, requestId);
            var ingressFiles = PublishedIngressPngFiles(serverArtifactRoot);
            Assert.Equal(4, ingressFiles.Length);
            Assert.Contains(ingressFiles, path =>
                path.Contains(
                    Path.Combine("screenshot", "ready-json-live-local-source-screenshot"),
                    StringComparison.OrdinalIgnoreCase));
            Assert.Contains(ingressFiles, path =>
                path.Contains(
                    Path.Combine("design-render", "ready-json-live-local-design-render"),
                    StringComparison.OrdinalIgnoreCase));
            Assert.Contains(ingressFiles, path =>
                path.Contains(
                    Path.Combine("screenshot", "ready-json-live-local-source-screenshot-mobile"),
                    StringComparison.OrdinalIgnoreCase));
            Assert.Contains(ingressFiles, path =>
                path.Contains(
                    Path.Combine("design-render", "ready-json-live-local-design-render-mobile"),
                    StringComparison.OrdinalIgnoreCase));
            Assert.Empty(TempFiles(serverArtifactRoot));

            using var statusResponse = await GetAuthorizedAsync(
                httpClient,
                readyBaseUrl,
                DesignConversionModelServerStatusHandler.EndpointPathPrefix + submitted.Envelope.ParsedJobId);
            var statusJson = await statusResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);

            var publicSurface = string.Join(
                '\n',
                readyRoot.GetRawText(),
                submitted.Envelope.ResponseBodyJson,
                JsonSerializer.Serialize(
                    submitted.Envelope.BoundaryMetadata,
                    DesignConversionModelServerHttpHandler.JsonOptions),
                statusJson);
            Assert.All(callerSourcePaths, sourcePath =>
                Assert.DoesNotContain(sourcePath, publicSurface, StringComparison.OrdinalIgnoreCase));
            Assert.All(callerSourceUris, sourceUri =>
                Assert.DoesNotContain(sourceUri, publicSurface, StringComparison.OrdinalIgnoreCase));
            Assert.All(callerSourceBase64, sourceBase64 =>
                Assert.DoesNotContain(sourceBase64, publicSurface, StringComparison.Ordinal));
            Assert.DoesNotContain(RouteAdmissionCurrentToken, publicSurface, StringComparison.Ordinal);
            Assert.DoesNotContain(privateMarker, publicSurface, StringComparison.Ordinal);
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ReadyJson_ReturnsConcreteLoopbackBaseUrlForSubmit()
    {
        const string privateMarker = "PRIVATE_FLOW47_CONFIGURED_READY_JSON_SECRET";
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        Directory.CreateDirectory(tempRoot);

        var activeCandidate = await CreateLiveActiveModelCandidateAsync(
            tempRoot,
            "ready-json-loopback-active");
        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            artifactDbRoot,
            environment: ModelLifecycleEnvironment());
        try
        {
            using var readyDocument = await WaitForReadyFileAsync(process, readyFile, TimeSpan.FromSeconds(75));
            var readyRoot = readyDocument.RootElement;
            var readyBaseUrl = new Uri(readyRoot.GetProperty("baseUrl").GetString()!);
            AssertReadyJsonBaseUrlForSubmit(readyRoot, readyBaseUrl);
            AssertRouteAdmissionPublicMetadata(
                readyRoot,
                expectedConfigurationStatus: "enabled",
                expectedAcceptedKeyCount: 2,
                expectedPreviousTokenConfigured: true,
                expectedPreviousTokenActive: true);
            AssertNoRouteAdmissionSecretLeakage(readyRoot.GetRawText());

            using var serverOwnedArtifactRewriteHandler = new ServerOwnedArtifactRefRewriteHandler(new HttpClientHandler());
            using var httpClient = new HttpClient(serverOwnedArtifactRewriteHandler)
            {
                Timeout = LiveBrowserBackedSubmitTimeout
            };
            using (var healthResponse = await httpClient.GetAsync(new Uri(readyBaseUrl, "/__pass143/health")))
            {
                var healthJson = await healthResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
                using var healthDocument = JsonDocument.Parse(healthJson);
                AssertRouteAdmissionPublicMetadata(
                    healthDocument.RootElement,
                    expectedConfigurationStatus: "enabled",
                    expectedAcceptedKeyCount: 2,
                    expectedPreviousTokenConfigured: true,
                    expectedPreviousTokenActive: true);
                AssertNoRouteAdmissionSecretLeakage(healthJson);
            }

            using (var activationClient = new HttpClient
                   {
                       BaseAddress = readyBaseUrl,
                       Timeout = LiveBrowserBackedSubmitTimeout
                   })
            {
                await ActivateLiveModelAsync(
                    activationClient,
                    activeCandidate,
                    "activate:ready-json-loopback-active");
            }

            var submitClient = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
            var strictArtifactRoot = Path.Combine(tempRoot, "render-artifacts");
            const string failClosedRequestId = "flow47-ready-json-route-admission-fail-closed";
            const string missingTokenIdempotencyKey = "flow47-ready-json-missing-token-key";
            const string invalidTokenIdempotencyKey = "flow47-ready-json-invalid-token-key";
            var failClosedHtml = $"<main data-secret=\"{privateMarker}\">Flow47 admission fail closed</main>";
            var failClosedCss = $".flow47-fail-closed{{background-image:url('{privateMarker}');}}";
            var failClosedTree = $"{{\"secret\":\"{privateMarker}\",\"kind\":\"fail-closed\"}}";
            var failClosedBody = JsonSerializer.Serialize(
                new
                {
                    requestId = failClosedRequestId,
                    sourceKind = "Figma",
                    designTree = new
                    {
                        refId = "flow47-fail-closed-design-tree",
                        kind = "figma-json",
                        sha256 = "sha256:flow47-fail-closed-design-tree",
                        payload = failClosedTree,
                        format = "figma-json"
                    },
                    originalTree = new
                    {
                        refId = "flow47-fail-closed-original-tree",
                        kind = "figma-json",
                        sha256 = "sha256:flow47-fail-closed-original-tree",
                        payload = failClosedTree,
                        format = "figma-json"
                    },
                    html = failClosedHtml,
                    css = failClosedCss,
                    screenshots = Array.Empty<object>(),
                    designRenders = Array.Empty<object>()
                },
                DesignConversionModelServerHttpHandler.JsonOptions);
            using (var missingTokenResponse = await PostEvaluateAsync(
                       httpClient,
                       readyBaseUrl,
                       failClosedBody,
                       token: null,
                       idempotencyKey: missingTokenIdempotencyKey))
            {
                var missingTokenJson = await missingTokenResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.Unauthorized, missingTokenResponse.StatusCode);
                AssertAdmissionRejectedBeforeRuntime(
                    missingTokenJson,
                    "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_REQUIRED",
                    failClosedBody,
                    failClosedRequestId,
                    failClosedHtml,
                    failClosedCss,
                    failClosedTree,
                    missingTokenIdempotencyKey);
            }

            using (var invalidTokenResponse = await PostEvaluateAsync(
                       httpClient,
                       readyBaseUrl,
                       failClosedBody,
                       token: "wrong-live-local-route-token",
                       idempotencyKey: invalidTokenIdempotencyKey))
            {
                var invalidTokenJson = await invalidTokenResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.Forbidden, invalidTokenResponse.StatusCode);
                AssertAdmissionRejectedBeforeRuntime(
                    invalidTokenJson,
                    "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_INVALID",
                    failClosedBody,
                    failClosedRequestId,
                    failClosedHtml,
                    failClosedCss,
                    failClosedTree,
                    invalidTokenIdempotencyKey,
                    "wrong-live-local-route-token");
            }

            var recordsFile = Path.Combine(artifactDbRoot, "conversion-results.jsonl");
            Assert.Equal(0, await CountPersistedRecordsAsync(recordsFile, failClosedRequestId));

            var expectedArtifactRoot = Path.Combine(strictArtifactRoot, "expected-input");
            var packageFixture = await CreateConfiguredReadyJsonSubmitPackageAsync(
                privateMarker,
                expectedArtifactRoot,
                includeServerOwnedNegativeDelta: true);
            serverOwnedArtifactRewriteHandler.AddMappings(packageFixture.ServerOwnedArtifactUrisBySha256);
            var package = packageFixture.Package;
            const string replayIdempotencyKey = "flow47-ready-json-replay-key";

            var result = await submitClient.TrySubmitReadyFileAsync(
                readyFile,
                package,
                routeAdmissionToken: RouteAdmissionCurrentToken,
                idempotencyKey: replayIdempotencyKey);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            var submitted = result.Value!;
            Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.ClientBoundary, submitted.ClientBoundary);
            Assert.Equal(readyBaseUrl, submitted.BaseUrl);
            Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointPath, submitted.EndpointPath);
            Assert.Equal(
                new Uri(readyBaseUrl, DesignConversionModelServerEndpointRegistry.EndpointPath),
                submitted.RequestUri);
            Assert.True(submitted.ReadyJsonBaseUrlConfigured);
            Assert.False(submitted.ReadyJsonBaseUrlMissing);
            Assert.True(submitted.ReadyJsonBaseUrlLoopback);
            Assert.True(submitted.ReadyJsonAdvertisedSubmitRoute);
            Assert.False(submitted.ProductReady);

            var envelope = submitted.Envelope;
            Assert.True(
                string.Equals("success", envelope.EnvelopeKind, StringComparison.Ordinal),
                $"Expected success envelope; status={envelope.StatusCode}; error={envelope.ParsedErrorCode}; message={envelope.ParsedErrorMessage}; body={envelope.ResponseBodyJson}");
            Assert.Equal(200, envelope.StatusCode);
            Assert.True(envelope.RegistryInvokeSucceeded);
            Assert.True(envelope.RuntimeInvoked);
            Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
            AssertCompletedQualityThreshold(envelope);
            Assert.Equal(package.RequestId, envelope.ParsedRequestId);
            Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointPath, envelope.EndpointPath);
            Assert.False(envelope.PublicHttpEndpointVerified);
            Assert.False(envelope.CrossRepoCallerVerified);
            Assert.False(envelope.ExternalCallerRepoTouched);
            Assert.False(envelope.RawPrivateResponseLeakageDetected);
            Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.Html!, envelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.Css!, envelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.DesignTree!.Payload!, envelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.OriginalTree!.Payload!, envelope.ResponseBodyJson, StringComparison.Ordinal);
            AssertNoRouteAdmissionSecretLeakage(
                envelope.ResponseBodyJson,
                privateMarker,
                package.Html!,
                package.Css!,
                package.DesignTree!.Payload!,
                package.OriginalTree!.Payload!,
                replayIdempotencyKey);

            await AssertSuccessfulHumanApprovalOverHttpAsync(
                httpClient,
                readyBaseUrl,
                envelope.ParsedJobId!,
                package.RequestId,
                tempRoot,
                privateMarker,
                package.Html!,
                package.Css!,
                package.DesignTree!.Payload!,
                package.OriginalTree!.Payload!);

            var metadata = envelope.BoundaryMetadata;
            Assert.Equal(bool.TrueString, metadata["ready_json_base_url_configured"]);
            Assert.Equal(bool.FalseString, metadata["ready_json_base_url_missing"]);
            Assert.Equal(bool.TrueString, metadata["ready_json_base_url_loopback"]);
            Assert.Equal(bool.TrueString, metadata["ready_json_submit_route_advertised"]);
            Assert.Equal(readyBaseUrl.GetLeftPart(UriPartial.Authority), metadata["ready_json_base_url"]);
            var submittedRequestUri = submitted.RequestUri ?? throw new InvalidOperationException("Submitted request URI was not captured.");
            Assert.Equal(submittedRequestUri.ToString(), metadata["ready_json_submit_request_uri"]);
            Assert.Equal(bool.TrueString, metadata["default_submit_base_url_configured"]);
            Assert.Equal(bool.FalseString, metadata["default_submit_base_url_missing"]);
            Assert.Equal(
                DesignConversionModelServerReadyJsonSubmitClient.ConfiguredSubmitStatus,
                metadata["default_submit_base_url_status"]);
            Assert.Equal(string.Empty, metadata["default_submit_base_url_missing_setting"]);
            Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.ConfiguredSubmitStatus, metadata["ready_json_submit_status"]);
            Assert.Equal(bool.TrueString, metadata["ready_json_submit_http_request_sent"]);
            Assert.Equal(HttpMethod.Post.Method, metadata["ready_json_submit_http_method"]);
            Assert.Equal(bool.FalseString, metadata["flow47_verified"]);
            Assert.Equal(
                DesignConversionModelServerCallerContract.Flow47VerificationAuthority,
                metadata["flow47_verification_authority"]);
            Assert.Equal(
                DesignConversionModelServerReadyJsonSubmitClient.Flow47VerificationStateNotEvaluatedInReadyJson,
                metadata["flow47_verification_state"]);
            Assert.Equal(
                "public_flow47_post_conversion_submit_not_evaluated_at_ready_json_loopback_boundary",
                metadata["flow47_submit_status"]);
            Assert.Equal(string.Empty, metadata["flow47_missing_setting_names"]);
            Assert.Equal("xiigen_platform_post_conversion_submit_live_proof", metadata["flow47_missing_proof_name"]);
            Assert.Equal(
                "verify_xiigen_platform_post_conversion_submit_delegates_to_flow47_and_returns_202_job_status",
                metadata["flow47_next_action"]);
            Assert.Equal(string.Empty, metadata["flow47_base_url_source"]);
            Assert.Equal(string.Empty, metadata["flow47_endpoint_path"]);
            Assert.Equal(string.Empty, metadata["flow47_submit_request_uri"]);
            Assert.Equal(bool.FalseString, metadata["flow47_live_local_loopback_verified"]);
            Assert.Equal(bool.TrueString, metadata["asp_net_host_verified"]);
            Assert.Equal(bool.TrueString, metadata["asp_net_route_matched"]);
            Assert.Equal(bool.TrueString, metadata["asp_net_route_verified"]);
            Assert.Equal(
                DesignConversionModelServerAspNetEndpointMapper.AspNetRouteVerificationAuthority,
                metadata["asp_net_route_verification_authority"]);
            Assert.Equal(
                DesignConversionModelServerAspNetEndpointMapper.AspNetRouteVerifiedState,
                metadata["asp_net_route_verification_state"]);
            Assert.Equal("design_conversion_evaluate", metadata["asp_net_route_id"]);
            Assert.Equal("design_conversion_evaluate", metadata["asp_net_route_verification_route_id"]);
            Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointPath, metadata["asp_net_route_path"]);
            Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointPath, metadata["asp_net_route_verification_route_path"]);
            Assert.Equal(bool.TrueString, metadata["asp_net_route_verification_successful_handler_dispatch"]);
            Assert.Equal(bool.TrueString, metadata["local_loopback_http_call_executed"]);
            Assert.Equal(bool.FalseString, metadata["public_http_endpoint_verified"]);
            Assert.Equal(bool.FalseString, metadata["deployment_verified"]);
            Assert.Equal(bool.FalseString, metadata["product_ready"]);
            Assert.Equal(bool.TrueString, metadata["route_admission_authorized"]);
            Assert.Equal(bool.TrueString, metadata["route_admission_token_supplied"]);
            Assert.Equal(bool.TrueString, metadata["route_admission_token_accepted"]);
            Assert.Equal("2", metadata["route_admission_accepted_key_count"]);
            Assert.Equal("previous_token_active", metadata["route_admission_rotation_state"]);
            Assert.Equal(bool.TrueString, metadata["idempotency_key_accepted"]);
            Assert.Equal(bool.FalseString, metadata["idempotency_payload_digest_exposed"]);
            Assert.Equal(bool.FalseString, metadata["token_value_exposed"]);
            Assert.Equal(bool.FalseString, metadata["token_hash_exposed"]);
            Assert.Equal(bool.TrueString, metadata["browser_render_executed"]);
            Assert.Equal(bool.FalseString, metadata["browser_render_not_executed"]);
            Assert.Equal(
                BrowserBackedDesignConversionRenderProvider.ProviderKind,
                metadata["browser_renderer_injected_provider_kind"]);
            Assert.Equal("2", metadata["render_qa_result_count"]);
            Assert.Equal("2", metadata["multi_viewport_required_count"]);
            Assert.Equal("2", metadata["multi_viewport_observed_count"]);
            Assert.Equal(bool.TrueString, metadata["multi_viewport_complete"]);
            Assert.Equal("critic_local_checkpoint_feature_adjusted_score", metadata["logical_flow_primary_metric_name"]);
            Assert.True(
                int.TryParse(metadata["LocalCheckpointFeatureContributionCount"], out var contributionCount) &&
                contributionCount > 0,
                $"Expected positive LocalCheckpointFeatureContributionCount, got '{metadata["LocalCheckpointFeatureContributionCount"]}'.");
            Assert.Equal(bool.TrueString, metadata["local_checkpoint_weights_configured"]);
            Assert.Equal(bool.FalseString, metadata["trained_model_authority"]);
            Assert.Equal(bool.TrueString, metadata["not_trained_model_authority"]);
            Assert.Equal(bool.FalseString, metadata["product_ready"]);
            Assert.Equal(bool.FalseString, metadata["visual_fidelity_verified"]);
            Assert.Equal(bool.FalseString, metadata["external_provider_called"]);
            Assert.Equal(bool.FalseString, metadata["external_ai_visual_judge_executed"]);
            Assert.Equal(bool.FalseString, metadata["external_visual_ai_executed"]);
            Assert.Equal(bool.FalseString, metadata["real_external_provider_executed"]);
            Assert.Equal(bool.FalseString, metadata["network_call_executed"]);
            Assert.Equal(bool.FalseString, metadata["provider_payload_included"]);
            Assert.Equal(bool.TrueString, metadata["visual_provider_output_observed"]);
            Assert.Equal(ServerOwnedLocalPngComparisonProvider.ProviderKind, metadata["provider_kind"]);

            using (var responseDocument = JsonDocument.Parse(envelope.ResponseBodyJson))
            {
                var responseRoot = responseDocument.RootElement;
                Assert.False(responseRoot.GetProperty("trainedModelAuthority").GetBoolean());
                Assert.True(responseRoot.GetProperty("notTrainedModelAuthority").GetBoolean());
                Assert.True(responseRoot.GetProperty("modelTrainingVerified").GetBoolean());
                Assert.False(responseRoot.GetProperty("modelUploadReady").GetBoolean());
                Assert.False(responseRoot.GetProperty("productReady").GetBoolean());
                Assert.False(responseRoot.GetProperty("deploymentVerified").GetBoolean());
                Assert.False(responseRoot.GetProperty("publicHttpEndpointVerified").GetBoolean());
                Assert.False(responseRoot.GetProperty("platformIntegrationVerified").GetBoolean());
                Assert.True(responseRoot.GetProperty("trainingReadiness").GetProperty("modelTrainingVerified").GetBoolean());
                Assert.False(responseRoot.GetProperty("trainingReadiness").GetProperty("modelUploadReady").GetBoolean());
                Assert.False(responseRoot.GetProperty("trainingReadiness").GetProperty("productReady").GetBoolean());

                var responseMetadata = responseRoot.GetProperty("boundaryMetadata");
                Assert.Equal(bool.FalseString, responseMetadata.GetProperty("product_ready").GetString());
                Assert.Equal(
                    "critic_local_checkpoint_feature_adjusted_score",
                    responseMetadata.GetProperty("logical_flow_primary_metric_name").GetString());
                var responseContributionCountText = responseMetadata
                    .GetProperty("LocalCheckpointFeatureContributionCount")
                    .GetString();
                Assert.True(
                    int.TryParse(responseContributionCountText, out var responseContributionCount) &&
                    responseContributionCount > 0,
                    $"Expected positive response LocalCheckpointFeatureContributionCount, got '{responseContributionCountText}'.");
                Assert.True(bool.Parse(responseMetadata.GetProperty("local_checkpoint_weights_configured").GetString()!));
                Assert.Equal(
                    "production_activated_runtime_registry_checkpoint_load",
                    responseMetadata.GetProperty("local_model_verification_authority").GetString());
                Assert.Equal(bool.FalseString, responseMetadata.GetProperty("trained_model_authority").GetString());
                Assert.Equal(bool.TrueString, responseMetadata.GetProperty("not_trained_model_authority").GetString());
                Assert.Equal(bool.FalseString, responseMetadata.GetProperty("product_ready").GetString());

                var responseEvidenceRefs = ReadStringArray(responseRoot, "evidenceRefs");
                Assert.Contains(
                    "tier_policy:stop_reason=" + DesignConversionVisualQaBoundaryPolicy.QualityReached,
                    responseEvidenceRefs);
                Assert.DoesNotContain(
                    "tier_policy:stop_reason=" + DesignConversionVisualQaBoundaryPolicy.BudgetCapReached,
                    responseEvidenceRefs);
                Assert.Contains(ServerOwnedLocalPngComparisonProvider.ProviderKind, responseEvidenceRefs);
                Assert.DoesNotContain(
                    responseEvidenceRefs,
                    reference => reference.Contains(
                        DeterministicDesignConversionComparisonProvider.VisualComparisonNotProvenMarker,
                        StringComparison.Ordinal));
                Assert.Contains("external_ai_visual_judge_executed:false", responseEvidenceRefs);
                Assert.Contains(DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker, responseEvidenceRefs);
                Assert.Contains(
                    "logical-flow:" + DesignConversionRoutingFlow.FlowId,
                    responseEvidenceRefs);
                Assert.Contains(
                    "logical-orchestrator-selected-flow:" + DesignConversionRoutingFlow.FlowId,
                    responseEvidenceRefs);
                Assert.Contains(DesignConversionMachine.LocalCheckpointFeatureAdjustmentEvidence, responseEvidenceRefs);

                var responseWarnings = ReadStringArray(responseRoot, "warnings");
                Assert.Contains("visual_fidelity_verified=false", responseWarnings);
                Assert.Contains(DesignConversionMachine.RuntimeScaffoldNotTrainedModelWarning, responseWarnings);

                Assert.DoesNotContain("pass143-local-comparison-provider", envelope.ResponseBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain("pass143-comparison:", envelope.ResponseBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain("artifact://pass143/diff", envelope.ResponseBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain("artifact://flow47/ready-json", envelope.ResponseBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain("sha256:flow47-ready-json", envelope.ResponseBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain("client_claimed_browser_marker", envelope.ResponseBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain("private_marker", envelope.ResponseBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(tempRoot, envelope.ResponseBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(strictArtifactRoot, envelope.ResponseBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(expectedArtifactRoot, envelope.ResponseBodyJson, StringComparison.OrdinalIgnoreCase);
            }

            var replayResult = await submitClient.TrySubmitReadyFileAsync(
                readyFile,
                package,
                routeAdmissionToken: RouteAdmissionCurrentToken,
                idempotencyKey: replayIdempotencyKey);
            Assert.True(replayResult.IsSuccess, replayResult.ErrorMessage);
            var replayEnvelope = replayResult.Value!.Envelope;
            Assert.Equal(200, replayEnvelope.StatusCode);
            Assert.Equal(envelope.ParsedJobId, replayEnvelope.ParsedJobId);
            Assert.Equal(envelope.ParsedRequestId, replayEnvelope.ParsedRequestId);
            Assert.Equal(envelope.ResponseBodyJson, replayEnvelope.ResponseBodyJson);
            AssertNoRouteAdmissionSecretLeakage(
                replayEnvelope.ResponseBodyJson,
                privateMarker,
                package.Html!,
                package.Css!,
                package.DesignTree!.Payload!,
                package.OriginalTree!.Payload!,
                replayIdempotencyKey);

            var conflictPackageFixture = await CreateConfiguredReadyJsonSubmitPackageAsync(
                privateMarker,
                strictArtifactRoot,
                requestId: "flow47-configured-ready-json-submit-conflict-request");
            serverOwnedArtifactRewriteHandler.AddMappings(conflictPackageFixture.ServerOwnedArtifactUrisBySha256);
            var conflictPackage = conflictPackageFixture.Package;
            var conflictResult = await submitClient.TrySubmitReadyFileAsync(
                readyFile,
                conflictPackage,
                routeAdmissionToken: RouteAdmissionCurrentToken,
                idempotencyKey: replayIdempotencyKey);
            Assert.True(conflictResult.IsSuccess, conflictResult.ErrorMessage);
            var conflictEnvelope = conflictResult.Value!.Envelope;
            Assert.Equal(409, conflictEnvelope.StatusCode);
            Assert.Equal("DESIGN_CONVERSION_IDEMPOTENCY_KEY_CONFLICT", conflictEnvelope.ParsedErrorCode);
            Assert.False(conflictEnvelope.RuntimeInvoked);
            AssertNoRouteAdmissionSecretLeakage(
                conflictEnvelope.ResponseBodyJson,
                privateMarker,
                conflictPackage.Html!,
                conflictPackage.Css!,
                conflictPackage.DesignTree!.Payload!,
                conflictPackage.OriginalTree!.Payload!,
                replayIdempotencyKey);
            Assert.Equal(1, await CountPersistedRecordsAsync(recordsFile, package.RequestId));
            Assert.Equal(0, await CountPersistedRecordsAsync(recordsFile, conflictPackage.RequestId));

            using (var statusResponse = await GetAuthorizedAsync(
                       httpClient,
                       readyBaseUrl,
                       DesignConversionModelServerStatusHandler.EndpointPathPrefix + envelope.ParsedJobId))
            {
                var statusJson = await statusResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);
                using var statusDocument = JsonDocument.Parse(statusJson);
                var statusMetadata = statusDocument.RootElement.GetProperty("boundaryMetadata");
                Assert.Equal(bool.TrueString, statusMetadata.GetProperty("route_admission_authorized").GetString());
                Assert.Equal(bool.TrueString, statusMetadata.GetProperty("route_admission_token_accepted").GetString());
                Assert.Equal(bool.FalseString, statusMetadata.GetProperty("idempotency_payload_digest_exposed").GetString());
                Assert.Equal(package.RequestId, statusDocument.RootElement.GetProperty("requestId").GetString());
                AssertNoRouteAdmissionSecretLeakage(
                    statusJson,
                    privateMarker,
                    package.Html!,
                    package.Css!,
                    package.DesignTree!.Payload!,
                    package.OriginalTree!.Payload!,
                    replayIdempotencyKey);
            }

            var oneViewportPackageFixture = await CreateConfiguredReadyJsonSubmitPackageAsync(
                privateMarker,
                strictArtifactRoot,
                includeMobileViewport: false,
                requestId: "flow47-configured-ready-json-submit-one-viewport-request");
            serverOwnedArtifactRewriteHandler.AddMappings(oneViewportPackageFixture.ServerOwnedArtifactUrisBySha256);
            var oneViewportPackage = oneViewportPackageFixture.Package;
            var oneViewportResult = await submitClient.TrySubmitReadyFileAsync(
                readyFile,
                oneViewportPackage,
                routeAdmissionToken: RouteAdmissionCurrentToken,
                idempotencyKey: "flow47-ready-json-one-viewport-key");
            Assert.True(oneViewportResult.IsSuccess, oneViewportResult.ErrorMessage);
            var oneViewportEnvelope = oneViewportResult.Value!.Envelope;
            Assert.Equal("success", oneViewportEnvelope.EnvelopeKind);
            Assert.Equal("2", oneViewportEnvelope.BoundaryMetadata["multi_viewport_required_count"]);
            Assert.Equal("1", oneViewportEnvelope.BoundaryMetadata["multi_viewport_observed_count"]);
            Assert.Equal(bool.FalseString, oneViewportEnvelope.BoundaryMetadata["multi_viewport_complete"]);
            Assert.Contains(
                DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete,
                oneViewportEnvelope.ResponseBodyJson,
                StringComparison.Ordinal);
            using (var oneViewportResponseDocument = JsonDocument.Parse(oneViewportEnvelope.ResponseBodyJson))
            {
                var oneViewportResponseRoot = oneViewportResponseDocument.RootElement;
                Assert.False(oneViewportResponseRoot.GetProperty("productReady").GetBoolean());
                var oneViewportResponseWarnings = ReadStringArray(oneViewportResponseRoot, "warnings");
                Assert.Contains(
                    "MULTI_RESOLUTION_RENDER_EVIDENCE_INCOMPLETE",
                    oneViewportResponseWarnings);
                var oneViewportResponseEvidenceRefs = ReadStringArray(oneViewportResponseRoot, "evidenceRefs");
                Assert.Contains(
                    "tier_policy:stop_reason=" + DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete,
                    oneViewportResponseEvidenceRefs);
                var oneViewportResponseMetadata = oneViewportResponseRoot.GetProperty("boundaryMetadata");
                Assert.Equal("2", oneViewportResponseMetadata.GetProperty("multi_viewport_required_count").GetString());
                Assert.Equal("1", oneViewportResponseMetadata.GetProperty("multi_viewport_observed_count").GetString());
                Assert.Equal(bool.FalseString, oneViewportResponseMetadata.GetProperty("multi_viewport_complete").GetString());
                Assert.Equal(bool.FalseString, oneViewportResponseMetadata.GetProperty("product_ready").GetString());
                Assert.Equal(bool.FalseString, oneViewportResponseMetadata.GetProperty("visual_fidelity_verified").GetString());
                Assert.Equal("1", oneViewportResponseMetadata.GetProperty("render_qa_result_count").GetString());
                Assert.Equal("1", oneViewportResponseMetadata.GetProperty("comparison_artifact_count").GetString());
            }

            using var oneViewportStatusResponse = await GetAuthorizedAsync(
                httpClient,
                readyBaseUrl,
                DesignConversionModelServerStatusHandler.EndpointPathPrefix + oneViewportEnvelope.ParsedJobId);
            var oneViewportStatusJson = await oneViewportStatusResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, oneViewportStatusResponse.StatusCode);
            using var oneViewportStatusDocument = JsonDocument.Parse(oneViewportStatusJson);
            var oneViewportStatusRoot = oneViewportStatusDocument.RootElement;
            Assert.False(oneViewportStatusRoot.GetProperty("productReady").GetBoolean());
            var oneViewportStatusWarnings = ReadStringArray(oneViewportStatusRoot, "warnings");
            Assert.Contains(
                "MULTI_RESOLUTION_RENDER_EVIDENCE_INCOMPLETE",
                oneViewportStatusWarnings);
            var oneViewportStatusEvidenceRefs = ReadStringArray(oneViewportStatusRoot, "evidenceRefs");
            Assert.Contains(
                "tier_policy:stop_reason=" + DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete,
                oneViewportStatusEvidenceRefs);
            var oneViewportStatusMetadata = oneViewportStatusRoot.GetProperty("boundaryMetadata");
            Assert.Equal("2", oneViewportStatusMetadata.GetProperty("multi_viewport_required_count").GetString());
            Assert.Equal("1", oneViewportStatusMetadata.GetProperty("multi_viewport_observed_count").GetString());
            Assert.Equal(bool.FalseString, oneViewportStatusMetadata.GetProperty("multi_viewport_complete").GetString());
            Assert.Equal(bool.FalseString, oneViewportStatusMetadata.GetProperty("product_ready").GetString());
            Assert.Equal(bool.FalseString, oneViewportStatusMetadata.GetProperty("visual_fidelity_verified").GetString());
            Assert.Equal("1", oneViewportStatusMetadata.GetProperty("render_qa_result_count").GetString());
            Assert.Equal("1", oneViewportStatusMetadata.GetProperty("comparison_artifact_count").GetString());
            Assert.Contains(
                DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete,
                oneViewportStatusJson,
                StringComparison.Ordinal);

            Assert.True(File.Exists(recordsFile), $"Expected local artifact DB file at {recordsFile}.");
            var persistedLines = await File.ReadAllLinesAsync(recordsFile);
            var persistedJsonLines = persistedLines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();
            Assert.True(persistedJsonLines.Length >= 1, $"Expected at least one local artifact DB record at {recordsFile}.");
            var storedRecords = persistedJsonLines
                .Select(line => JsonSerializer.Deserialize<DesignConversionLocalArtifactDbRecord>(
                    line,
                    DesignConversionCheckpointJsonOptions.Standard))
                .ToArray();
            Assert.All(storedRecords, Assert.NotNull);
            var storedRecord = Assert.Single(storedRecords, record => string.Equals(
                record!.RequestId,
                package.RequestId,
                StringComparison.Ordinal))!;
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind, storedRecord.DatabaseKind);
            Assert.Equal(envelope.ParsedJobId, storedRecord.JobId);
            Assert.Equal(package.RequestId, storedRecord.RequestId);
            Assert.StartsWith("dcm-html_result-", storedRecord.ConversionResult.HtmlRef, StringComparison.Ordinal);
            Assert.StartsWith("sha256:", storedRecord.ConversionResult.HtmlSha256, StringComparison.Ordinal);
            Assert.StartsWith("dcm-css_result-", storedRecord.ConversionResult.CssRef, StringComparison.Ordinal);
            Assert.StartsWith("sha256:", storedRecord.ConversionResult.CssSha256, StringComparison.Ordinal);
            Assert.NotEmpty(storedRecord.RenderArtifactRefs);
            Assert.Equal(2, storedRecord.RenderArtifactRefs.Count);
            Assert.Contains(storedRecord.RenderArtifactRefs, artifact =>
                artifact.ViewportId == "source-1440x900" &&
                artifact.Width == 1440 &&
                artifact.Height == 900);
            Assert.Contains(storedRecord.RenderArtifactRefs, artifact =>
                artifact.ViewportId == "source-390x844@3" &&
                artifact.Width == 390 &&
                artifact.Height == 844);
            Assert.All(storedRecord.RenderArtifactRefs, artifact =>
            {
                Assert.Equal("browser-render-screenshot", artifact.Kind);
                Assert.StartsWith("dcm-artifact-ref-", artifact.UriRef, StringComparison.Ordinal);
                Assert.StartsWith("sha256:", artifact.Sha256, StringComparison.Ordinal);
                Assert.Equal(
                    BrowserBackedDesignConversionRenderProvider.ProviderKind,
                    artifact.Metadata["provider_kind"]);
                Assert.Equal(bool.TrueString, artifact.Metadata["browser_or_playwright_used"]);
                Assert.Equal(bool.TrueString, artifact.Metadata["visual_fidelity_claimed"]);
                Assert.Equal(
                    "browser_backed_render_evidence",
                    artifact.Metadata["hidden_determinism_boundary"]);
                Assert.Equal(
                    "browser_backed_render_evidence",
                    artifact.Metadata["render_evidence_trust_classification"]);
                Assert.Equal(bool.TrueString, artifact.Metadata["render_evidence_production_trusted"]);
                Assert.Equal(bool.FalseString, artifact.Metadata["synthetic_or_scaffold_render_result"]);
                Assert.DoesNotContain("client_claimed_browser_marker", artifact.Metadata.Keys);
                Assert.DoesNotContain(privateMarker, artifact.Metadata.Values, StringComparer.Ordinal);
            });
            Assert.Equal(bool.TrueString, storedRecord.Metadata["local_artifact_db"]);
            Assert.Equal(bool.TrueString, storedRecord.Metadata["storage_training_artifact_db"]);
            Assert.Equal(bool.FalseString, storedRecord.Metadata["external_ai_visual_judge_executed"]);
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, storedRecord.Metadata["tier_policy_stop_reason"]);
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, storedRecord.VisualJudgeStatus.TierDecision);
            Assert.False(storedRecord.VisualJudgeStatus.ExternalCallExecuted);
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker, storedRecord.VisualJudgeStatus.ProviderBoundary);
            Assert.Contains(
                "tier_policy:stop_reason=" + DesignConversionVisualQaBoundaryPolicy.QualityReached,
                storedRecord.VisualJudgeStatus.EvidenceRefs);
            Assert.DoesNotContain(
                "tier_policy:stop_reason=" + DesignConversionVisualQaBoundaryPolicy.BudgetCapReached,
                storedRecord.VisualJudgeStatus.EvidenceRefs);
            Assert.DoesNotContain(
                storedRecord.VisualJudgeStatus.EvidenceRefs,
                reference => reference.Contains(
                    DeterministicDesignConversionComparisonProvider.VisualComparisonNotProvenMarker,
                    StringComparison.Ordinal));
            Assert.True(
                string.Equals(
                    bool.TrueString,
                    storedRecord.Metadata["runtime_created_dpo_candidate"],
                    StringComparison.Ordinal),
                storedRecord.Metadata.TryGetValue(
                    "runtime_created_dpo_candidate_warnings",
                    out var runtimeCreatedDpoCandidateWarnings)
                    ? runtimeCreatedDpoCandidateWarnings
                    : "runtime_created_dpo_candidate_warnings missing");
            Assert.Equal("1", storedRecord.Metadata["runtime_created_dpo_candidate_count"]);
            Assert.Equal(bool.TrueString, storedRecord.Metadata["review_required"]);
            Assert.Equal("pending", storedRecord.Metadata["approval_status"]);
            Assert.Equal(bool.FalseString, storedRecord.Metadata["approved_for_training"]);
            Assert.Equal(bool.FalseString, storedRecord.Metadata["production_dpo"]);
            Assert.Equal(bool.FalseString, storedRecord.Metadata["model_readiness_claimed"]);
            Assert.Contains(
                "runtime_created_dpo_candidate_review_pending",
                storedRecord.Metadata["runtime_created_dpo_candidate_warnings"],
                StringComparison.Ordinal);
            var storedCandidate = Assert.Single(storedRecord.DpoCandidates);
            Assert.Equal(DesignConversionDpoUnits.ImproverUnit, storedCandidate.UnitId);
            Assert.True(storedCandidate.TrainingCandidateOnly);
            Assert.True(storedCandidate.ProductNotReady);
            Assert.False(storedCandidate.IsQuarantined);
            Assert.True(storedCandidate.UsefulnessScore > 0);
            Assert.Equal("pending", storedCandidate.TrainingRecord.Provenance.Metadata["approval_status"]);
            Assert.Equal(
                bool.FalseString,
                storedCandidate.TrainingRecord.Provenance.Metadata["approved_for_training"]);
            Assert.Equal(
                bool.FalseString,
                storedCandidate.TrainingRecord.Provenance.Metadata["production_dpo"]);
            Assert.Equal(
                bool.FalseString,
                storedCandidate.TrainingRecord.Provenance.Metadata["model_readiness_claimed"]);
            var persistedText = string.Join("\n", persistedJsonLines);
            Assert.DoesNotContain(privateMarker, persistedText, StringComparison.Ordinal);
            Assert.DoesNotContain(package.Html!, persistedText, StringComparison.Ordinal);
            Assert.DoesNotContain(package.Css!, persistedText, StringComparison.Ordinal);
            Assert.DoesNotContain(package.DesignTree!.Payload!, persistedText, StringComparison.Ordinal);
            Assert.DoesNotContain(package.OriginalTree!.Payload!, persistedText, StringComparison.Ordinal);
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Fact(Timeout = 240000)]
    public async Task RouteAdmissionDurableReplay_ReplaysAcrossSeparateLiveLocalProcessRestartWithoutSecondPersistence()
    {
        const string privateMarker = "PRIVATE_FLOW47_DURABLE_RESTART_SECRET";
        const string replayIdempotencyKey = "flow47-durable-live-restart-replay-key";
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        var recordsFile = Path.Combine(artifactDbRoot, "conversion-results.jsonl");
        var privateRuntimeRoot = Path.Combine(tempRoot, "runtime-data");
        var durableRecordsRoot = Path.Combine(
            privateRuntimeRoot,
            "route-admission-idempotency",
            "records");
        var strictArtifactRoot = Path.Combine(tempRoot, "render-artifacts");
        Directory.CreateDirectory(tempRoot);

        Process? process1 = null;
        Process? process2 = null;
        try
        {
            var packageFixture = await CreateConfiguredReadyJsonSubmitPackageAsync(
                privateMarker,
                strictArtifactRoot,
                requestId: "flow47-durable-live-restart-submit");
            var package = packageFixture.Package;
            var activeCandidate = await CreateLiveActiveModelCandidateAsync(
                tempRoot,
                "durable-replay-active");

            process1 = StartServerProcess(
                repoRoot,
                projectPath,
                readyFile,
                artifactDbRoot,
                routeAdmissionEnabled: true,
                routeAdmissionCurrentToken: RouteAdmissionCurrentToken,
                routeAdmissionPreviousToken: RouteAdmissionPreviousToken,
                routeAdmissionPreviousTokenExpiresAtUtc: RouteAdmissionPreviousTokenExpiresAtUtc,
                routeAdmissionTenantId: RouteAdmissionTenantId,
                environment: ModelLifecycleEnvironment());

            using var readyDocument1 = await WaitForReadyFileAsync(process1, readyFile, TimeSpan.FromSeconds(75));
            var readyRoot1 = readyDocument1.RootElement;
            var readyBaseUrl1 = new Uri(readyRoot1.GetProperty("baseUrl").GetString()!);
            AssertReadyJsonBaseUrlForSubmit(readyRoot1, readyBaseUrl1);
            AssertDurableReplayPublicMetadata(readyRoot1, expectedCompletedRecordCount: 0);
            AssertBrowserRendererPngProofPublicFields(
                readyRoot1,
                expectedRequired: true,
                expectedAttempted: true,
                expectedValid: true,
                expectedStartupStatus: "png_proof_verified",
                expectedNonReadyCode: "none",
                expectedEvidenceCount: 4,
                expectedRequiredArtifactCount: 4,
                expectedRequiredViewportCount: 2,
                expectedObservedViewportCount: 2);
            AssertPublicHealthReadyStringsDoNotLeakLocalPaths(readyRoot1, repoRoot, tempRoot);

            using var serverOwnedArtifactRewriteHandler1 = new ServerOwnedArtifactRefRewriteHandler(new HttpClientHandler());
            serverOwnedArtifactRewriteHandler1.AddMappings(packageFixture.ServerOwnedArtifactUrisBySha256);
            using var httpClient1 = new HttpClient(serverOwnedArtifactRewriteHandler1)
            {
                Timeout = LiveBrowserBackedSubmitTimeout
            };
            using (var activationClient = new HttpClient
                   {
                       BaseAddress = readyBaseUrl1,
                       Timeout = LiveBrowserBackedSubmitTimeout
                   })
            {
                await ActivateLiveModelAsync(
                    activationClient,
                    activeCandidate,
                    "activate:durable-replay-active");
            }

            string healthJson1;
            using (var healthResponse1 = await httpClient1.GetAsync(new Uri(readyBaseUrl1, "/__pass143/health")))
            {
                healthJson1 = await healthResponse1.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, healthResponse1.StatusCode);
                using var healthDocument1 = JsonDocument.Parse(healthJson1);
                AssertDurableReplayPublicMetadata(healthDocument1.RootElement, expectedCompletedRecordCount: 0);
                AssertBrowserRendererPngProofPublicFields(
                    healthDocument1.RootElement,
                    expectedRequired: true,
                    expectedAttempted: true,
                    expectedValid: true,
                    expectedStartupStatus: "png_proof_verified",
                    expectedNonReadyCode: "none",
                    expectedEvidenceCount: 4,
                    expectedRequiredArtifactCount: 4,
                    expectedRequiredViewportCount: 2,
                    expectedObservedViewportCount: 2);
                AssertPublicHealthReadyStringsDoNotLeakLocalPaths(healthDocument1.RootElement, repoRoot, tempRoot);
            }

            var submitClient1 = new DesignConversionModelServerReadyJsonSubmitClient(httpClient1);
            var firstResult = await submitClient1.TrySubmitReadyFileAsync(
                readyFile,
                package,
                routeAdmissionToken: RouteAdmissionCurrentToken,
                idempotencyKey: replayIdempotencyKey);
            Assert.True(firstResult.IsSuccess, firstResult.ErrorMessage);
            var firstEnvelope = firstResult.Value!.Envelope;
            Assert.Equal(200, firstEnvelope.StatusCode);
            Assert.Equal("success", firstEnvelope.EnvelopeKind);
            Assert.True(firstEnvelope.RuntimeInvoked);
            Assert.Equal(DesignConversionThinClientStatus.Completed, firstEnvelope.ParsedContractStatus);
            Assert.Equal(package.RequestId, firstEnvelope.ParsedRequestId);
            Assert.Equal(bool.TrueString, firstEnvelope.BoundaryMetadata["idempotency_durable_replay_enabled"]);
            Assert.Equal("enabled", firstEnvelope.BoundaryMetadata["idempotency_durable_replay_store_state"]);
            Assert.DoesNotContain("http://127.0.0.1:0", firstEnvelope.ResponseBodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("sha256:" + new string('0', 64), firstEnvelope.ResponseBodyJson, StringComparison.OrdinalIgnoreCase);

            string firstStatusJson;
            using (var firstStatusResponse = await GetAuthorizedAsync(
                       httpClient1,
                       readyBaseUrl1,
                       DesignConversionModelServerStatusHandler.EndpointPathPrefix + firstEnvelope.ParsedJobId))
            {
                firstStatusJson = await firstStatusResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, firstStatusResponse.StatusCode);
                using var firstStatusDocument = JsonDocument.Parse(firstStatusJson);
                Assert.Equal(firstEnvelope.ParsedJobId, firstStatusDocument.RootElement.GetProperty("jobId").GetString());
                Assert.Equal(package.RequestId, firstStatusDocument.RootElement.GetProperty("requestId").GetString());
                var firstStatusMetadata = firstStatusDocument.RootElement.GetProperty("boundaryMetadata");
                Assert.Equal(bool.TrueString, firstStatusMetadata.GetProperty("route_admission_authorized").GetString());
                    Assert.Equal(bool.TrueString, firstStatusMetadata.GetProperty("route_admission_token_accepted").GetString());
            }

            var firstArtifactDbRecordCount = await CountNonEmptyLinesAsync(recordsFile);
            var firstPackageRecordCount = await CountPersistedRecordsAsync(recordsFile, package.RequestId);
            var firstDurableRecordCount = CountDurableCompletedRecords(durableRecordsRoot);
            Assert.Equal(1, firstArtifactDbRecordCount);
            Assert.Equal(1, firstPackageRecordCount);
            Assert.Equal(1, firstDurableRecordCount);
            await AssertNoDurableRestartLeakageAsync(
                tempRoot,
                recordsFile,
                durableRecordsRoot,
                [readyRoot1.GetRawText(), healthJson1, firstEnvelope.ResponseBodyJson, firstStatusJson],
                replayIdempotencyKey,
                privateMarker,
                package.Html!,
                package.Css!,
                package.DesignTree!.Payload!,
                package.OriginalTree!.Payload!);

            await StopProcessAsync(process1);
            process1.Dispose();
            process1 = null;
            File.Delete(readyFile);

            process2 = StartServerProcess(
                repoRoot,
                projectPath,
                readyFile,
                artifactDbRoot,
                routeAdmissionEnabled: true,
                routeAdmissionCurrentToken: RouteAdmissionCurrentToken,
                routeAdmissionPreviousToken: RouteAdmissionPreviousToken,
                routeAdmissionPreviousTokenExpiresAtUtc: RouteAdmissionPreviousTokenExpiresAtUtc,
                routeAdmissionTenantId: RouteAdmissionTenantId,
                environment: ModelLifecycleEnvironment());

            using var readyDocument2 = await WaitForReadyFileAsync(process2, readyFile, TimeSpan.FromSeconds(75));
            var readyRoot2 = readyDocument2.RootElement;
            var readyBaseUrl2 = new Uri(readyRoot2.GetProperty("baseUrl").GetString()!);
            AssertReadyJsonBaseUrlForSubmit(readyRoot2, readyBaseUrl2);
            AssertDurableReplayPublicMetadata(readyRoot2, expectedCompletedRecordCount: 1);
            AssertBrowserRendererPngProofPublicFields(
                readyRoot2,
                expectedRequired: true,
                expectedAttempted: true,
                expectedValid: true,
                expectedStartupStatus: "png_proof_verified",
                expectedNonReadyCode: "none",
                expectedEvidenceCount: 4,
                expectedRequiredArtifactCount: 4,
                expectedRequiredViewportCount: 2,
                expectedObservedViewportCount: 2);
            AssertPublicHealthReadyStringsDoNotLeakLocalPaths(readyRoot2, repoRoot, tempRoot);

            using var serverOwnedArtifactRewriteHandler2 = new ServerOwnedArtifactRefRewriteHandler(new HttpClientHandler());
            serverOwnedArtifactRewriteHandler2.AddMappings(packageFixture.ServerOwnedArtifactUrisBySha256);
            using var httpClient2 = new HttpClient(serverOwnedArtifactRewriteHandler2)
            {
                Timeout = LiveBrowserBackedSubmitTimeout
            };

            string healthJson2;
            using (var healthResponse2 = await httpClient2.GetAsync(new Uri(readyBaseUrl2, "/__pass143/health")))
            {
                healthJson2 = await healthResponse2.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, healthResponse2.StatusCode);
                using var healthDocument2 = JsonDocument.Parse(healthJson2);
                AssertDurableReplayPublicMetadata(healthDocument2.RootElement, expectedCompletedRecordCount: 1);
                AssertBrowserRendererPngProofPublicFields(
                    healthDocument2.RootElement,
                    expectedRequired: true,
                    expectedAttempted: true,
                    expectedValid: true,
                    expectedStartupStatus: "png_proof_verified",
                    expectedNonReadyCode: "none",
                    expectedEvidenceCount: 4,
                    expectedRequiredArtifactCount: 4,
                    expectedRequiredViewportCount: 2,
                    expectedObservedViewportCount: 2);
                AssertPublicHealthReadyStringsDoNotLeakLocalPaths(healthDocument2.RootElement, repoRoot, tempRoot);
            }

            using (var preReplayStatusResponse = await GetAuthorizedAsync(
                       httpClient2,
                       readyBaseUrl2,
                       DesignConversionModelServerStatusHandler.EndpointPathPrefix + firstEnvelope.ParsedJobId))
            {
                var preReplayStatusJson = await preReplayStatusResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, preReplayStatusResponse.StatusCode);
                using var preReplayStatusDocument = JsonDocument.Parse(preReplayStatusJson);
                Assert.Equal(
                    firstEnvelope.ParsedJobId,
                    preReplayStatusDocument.RootElement.GetProperty("jobId").GetString());
                Assert.Equal(
                    package.RequestId,
                    preReplayStatusDocument.RootElement.GetProperty("requestId").GetString());
                var preReplayStatusMetadata =
                    preReplayStatusDocument.RootElement.GetProperty("boundaryMetadata");
                Assert.Equal(
                    bool.TrueString,
                    preReplayStatusMetadata.GetProperty("local_artifact_db_status_restored").GetString());
                Assert.Equal(
                    bool.FalseString,
                    preReplayStatusMetadata.GetProperty("status_restore_executed_runtime").GetString());
                AssertNoRouteAdmissionSecretLeakage(preReplayStatusJson, replayIdempotencyKey, privateMarker);
            }

            var replayEquivalentBody = SerializeCanonicalEquivalentEvaluateBody(package);
            string replayResponseBodyJson;
            using (var replayHttpResponse = await PostEvaluateAsync(
                       httpClient2,
                       readyBaseUrl2,
                       replayEquivalentBody,
                       RouteAdmissionCurrentToken,
                       replayIdempotencyKey))
            {
                replayResponseBodyJson = await replayHttpResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, replayHttpResponse.StatusCode);
                Assert.Equal(firstEnvelope.ResponseBodyJson, replayResponseBodyJson);
                Assert.DoesNotContain("http://127.0.0.1:0", replayResponseBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("sha256:" + new string('0', 64), replayResponseBodyJson, StringComparison.OrdinalIgnoreCase);
                using var replayDocument = JsonDocument.Parse(replayResponseBodyJson);
                Assert.Equal(firstEnvelope.ParsedJobId, replayDocument.RootElement.GetProperty("jobId").GetString());
                Assert.Equal(firstEnvelope.ParsedRequestId, replayDocument.RootElement.GetProperty("requestId").GetString());
                var replayMetadata = replayDocument.RootElement.GetProperty("boundaryMetadata");
                Assert.Equal(bool.TrueString, replayMetadata.GetProperty("runtime_invoked").GetString());
                Assert.Equal(bool.TrueString, replayMetadata.GetProperty("idempotency_durable_replay_response_metadata_stable").GetString());
                Assert.Equal(bool.TrueString, replayMetadata.GetProperty("idempotency_durable_replay_response_volatiles_redacted").GetString());
                Assert.Equal(bool.FalseString, replayMetadata.GetProperty("idempotency_durable_replay_response_fake_endpoint").GetString());
            }
            Assert.Equal(firstArtifactDbRecordCount, await CountNonEmptyLinesAsync(recordsFile));
            Assert.Equal(firstPackageRecordCount, await CountPersistedRecordsAsync(recordsFile, package.RequestId));
            Assert.Equal(firstDurableRecordCount, CountDurableCompletedRecords(durableRecordsRoot));

            string replayStatusJson;
            using (var replayStatusResponse = await GetAuthorizedAsync(
                       httpClient2,
                       readyBaseUrl2,
                       DesignConversionModelServerStatusHandler.EndpointPathPrefix + firstEnvelope.ParsedJobId))
            {
                replayStatusJson = await replayStatusResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, replayStatusResponse.StatusCode);
                using var replayStatusDocument = JsonDocument.Parse(replayStatusJson);
                Assert.Equal(firstEnvelope.ParsedJobId, replayStatusDocument.RootElement.GetProperty("jobId").GetString());
                Assert.Equal(package.RequestId, replayStatusDocument.RootElement.GetProperty("requestId").GetString());
                var replayStatusMetadata = replayStatusDocument.RootElement.GetProperty("boundaryMetadata");
                Assert.Equal(bool.TrueString, replayStatusMetadata.GetProperty("route_admission_authorized").GetString());
                Assert.Equal(bool.TrueString, replayStatusMetadata.GetProperty("route_admission_token_accepted").GetString());
                Assert.Equal(bool.TrueString, replayStatusMetadata.GetProperty("local_artifact_db_status_restored").GetString());
                Assert.Equal(bool.FalseString, replayStatusMetadata.GetProperty("status_restore_executed_runtime").GetString());
            }

            var submitClient2 = new DesignConversionModelServerReadyJsonSubmitClient(httpClient2);
            var conflictFixture = await CreateConfiguredReadyJsonSubmitPackageAsync(
                privateMarker,
                strictArtifactRoot,
                requestId: "flow47-durable-live-restart-conflict");
            serverOwnedArtifactRewriteHandler2.AddMappings(conflictFixture.ServerOwnedArtifactUrisBySha256);
            var conflictResult = await submitClient2.TrySubmitReadyFileAsync(
                readyFile,
                conflictFixture.Package,
                routeAdmissionToken: RouteAdmissionCurrentToken,
                idempotencyKey: replayIdempotencyKey);
            Assert.True(conflictResult.IsSuccess, conflictResult.ErrorMessage);
            var conflictEnvelope = conflictResult.Value!.Envelope;
            Assert.Equal(409, conflictEnvelope.StatusCode);
            Assert.Equal("DESIGN_CONVERSION_IDEMPOTENCY_KEY_CONFLICT", conflictEnvelope.ParsedErrorCode);
            Assert.False(conflictEnvelope.RuntimeInvoked);
            Assert.Equal(firstArtifactDbRecordCount, await CountNonEmptyLinesAsync(recordsFile));
            Assert.Equal(firstDurableRecordCount, CountDurableCompletedRecords(durableRecordsRoot));

            await AssertNoDurableRestartLeakageAsync(
                tempRoot,
                recordsFile,
                durableRecordsRoot,
                [
                    readyRoot2.GetRawText(),
                    healthJson2,
                    replayResponseBodyJson,
                    replayStatusJson,
                    conflictEnvelope.ResponseBodyJson
                ],
                replayIdempotencyKey,
                privateMarker,
                package.Html!,
                package.Css!,
                package.DesignTree!.Payload!,
                package.OriginalTree!.Payload!,
                conflictFixture.Package.Html!,
                conflictFixture.Package.Css!,
                conflictFixture.Package.DesignTree!.Payload!,
                conflictFixture.Package.OriginalTree!.Payload!);

            await StopProcessAsync(process2);
            process2.Dispose();
            process2 = null;
        }
        finally
        {
            if (process2 is not null)
                await StopProcessAsync(process2);
            if (process1 is not null)
                await StopProcessAsync(process1);
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void RouteAdmissionPublicLeakageGuard_HashesWslTmpCheckpointPathFragments()
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var rawPublicWarning =
            "CHECKPOINT_PATH:/tmp/design-conversion-live-local-server-tests/example/runtime-data/model-activation/checkpoint.bin";

        var sanitized = DesignConversionDpoLeakageGuard.SanitizeScalar(
            rawPublicWarning,
            "route_admission_public_warning",
            redactions);

        // Этот тест защищает именно WSL/root ветку: /tmp-путь не секрет модели,
        // но он доказывает локальную файловую топологию и не должен уходить в public JSON.
        Assert.Contains("CHECKPOINT_PATH:redacted-local-path-", sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain("/tmp/design-conversion-live-local-server-tests", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("route_admission_public_warning:local_path", redactions);
        Assert.True(DesignConversionDpoLeakageGuard.ContainsForbiddenLeakage(rawPublicWarning));
        Assert.False(DesignConversionDpoLeakageGuard.ContainsForbiddenLeakage(sanitized));
    }

    [Fact(Timeout = 120000)]
    public async Task RouteAdmissionEnabledWithoutCurrentToken_WritesNonReadyMetadataAndRejectsEvaluate()
    {
        const string privateMarker = "PRIVATE_ROUTE_ADMISSION_MISSING_CURRENT_SECRET";
        const string idempotencyKey = "flow47-missing-current-token-key";
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        Directory.CreateDirectory(tempRoot);

        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            artifactDbRoot,
            routeAdmissionCurrentToken: null,
            routeAdmissionPreviousToken: null,
            routeAdmissionPreviousTokenExpiresAtUtc: null);
        try
        {
            using var readyDocument = await WaitForReadyFileAsync(process, readyFile, TimeSpan.FromSeconds(75));
            var readyRoot = readyDocument.RootElement;
            var readyBaseUrl = new Uri(readyRoot.GetProperty("baseUrl").GetString()!);
            AssertRouteAdmissionPublicMetadata(
                readyRoot,
                expectedConfigurationStatus: "bounded_non_ready",
                expectedAcceptedKeyCount: 0,
                expectedPreviousTokenConfigured: false,
                expectedPreviousTokenActive: false);
            AssertNoRouteAdmissionSecretLeakage(readyRoot.GetRawText(), privateMarker, idempotencyKey);

            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            using (var healthResponse = await httpClient.GetAsync(new Uri(readyBaseUrl, "/__pass143/health")))
            {
                var healthJson = await healthResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
                using var healthDocument = JsonDocument.Parse(healthJson);
                AssertRouteAdmissionPublicMetadata(
                    healthDocument.RootElement,
                    expectedConfigurationStatus: "bounded_non_ready",
                    expectedAcceptedKeyCount: 0,
                    expectedPreviousTokenConfigured: false,
                    expectedPreviousTokenActive: false);
                AssertNoRouteAdmissionSecretLeakage(healthJson, privateMarker, idempotencyKey);
            }

            var bodyJson = $$"""
                           {
                             "requestId": "flow47-missing-current-token",
                             "sourceKind": "Figma",
                             "html": "<main data-secret='{{privateMarker}}'>Missing current token</main>",
                             "css": ".missing-current{content:'{{privateMarker}}';}"
                           }
                           """;
            using var response = await PostEvaluateAsync(
                httpClient,
                readyBaseUrl,
                bodyJson,
                token: null,
                idempotencyKey: idempotencyKey);
            var responseJson = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            AssertAdmissionRejectedBeforeRuntime(
                responseJson,
                "DESIGN_CONVERSION_ROUTE_ADMISSION_CURRENT_TOKEN_REQUIRED",
                bodyJson,
                privateMarker,
                idempotencyKey);
            Assert.Equal(0, await CountPersistedRecordsAsync(
                Path.Combine(artifactDbRoot, "conversion-results.jsonl"),
                "flow47-missing-current-token"));
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ModelLifecycleTokenKeyRingFromEnvironment_ReadActiveAcceptsRotationAndRejectsUnknownWithoutLeakage()
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(repoRoot, "tools", "DesignConversionLiveLocalServer", "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(Path.GetTempPath(), "design-conversion-live-local-server-tests", Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        var runtimeDataRoot = Path.Combine(tempRoot, "runtime-data");
        var activationRoot = Path.Combine(runtimeDataRoot, "model-activation");
        Directory.CreateDirectory(tempRoot);

        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            artifactDbRoot,
            environment: new Dictionary<string, string>
            {
                [ModelLifecycleCurrentTokenEnvironmentName] = ModelLifecycleCurrentToken,
                [ModelLifecyclePreviousTokenEnvironmentName] = ModelLifecyclePreviousToken,
                [ModelLifecyclePreviousTokenExpiresEnvironmentName] = ModelLifecyclePreviousTokenExpiresAtUtc
            });
        try
        {
            using var readyDocument = await WaitForReadyFileAsync(process, readyFile, TimeSpan.FromSeconds(75));
            var readyRoot = readyDocument.RootElement;
            AssertNoModelLifecycleTokenLeakage(readyRoot.GetRawText(), tempRoot);

            using var client = new HttpClient
            {
                BaseAddress = new Uri(readyRoot.GetProperty("baseUrl").GetString()!),
                Timeout = TimeSpan.FromSeconds(30)
            };

            using var healthResponse = await client.GetAsync("/__pass143/health");
            var healthJson = await healthResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
            AssertNoModelLifecycleTokenLeakage(healthJson, tempRoot);

            var readActiveBodyJson = SerializeModelActivationRequest(
                new DesignConversionModelServerModelActivationRequest(
                    Action: DesignConversionModelServerModelActivationHandler.ActionReadActive,
                    TenantId: RouteAdmissionTenantId));

            async Task AssertReadActiveAsync(string token, HttpStatusCode statusCode, string errorCode)
            {
                using var response = await PostModelActivationAsync(client, readActiveBodyJson, token);
                var responseJson = await response.Content.ReadAsStringAsync();
                Assert.Equal(statusCode, response.StatusCode);
                Assert.Equal(errorCode, JsonNode.Parse(responseJson)!["errorCode"]!.GetValue<string>());
                AssertNoModelLifecycleTokenLeakage(responseJson, tempRoot);
            }

            const string notFound = "DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND";
            await AssertReadActiveAsync(ModelLifecycleCurrentToken, HttpStatusCode.NotFound, notFound);
            await AssertReadActiveAsync(ModelLifecyclePreviousToken, HttpStatusCode.NotFound, notFound);
            await AssertReadActiveAsync(
                ModelLifecycleUnknownToken,
                HttpStatusCode.Forbidden,
                "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_INVALID");

            Assert.True(Directory.Exists(activationRoot), $"Expected activation root under runtime-data: {activationRoot}");
            AssertPathUnderRoot(activationRoot, runtimeDataRoot);
            AssertNoModelLifecycleTokenLeakage(DescribeProcessOutput(process), tempRoot);
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Fact(Timeout = 60000)]
    public async Task ModelLifecycleTokenKeyRingFromEnvironment_InvalidPreviousExpiryFailsClosedBeforeReadyFile()
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(repoRoot, "tools", "DesignConversionLiveLocalServer", "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(Path.GetTempPath(), "design-conversion-live-local-server-tests", Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        Directory.CreateDirectory(tempRoot);

        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            artifactDbRoot,
            routeAdmissionEnabled: false,
            environment: new Dictionary<string, string>
            {
                [ModelLifecyclePreviousTokenExpiresEnvironmentName] = "not-a-utc-date"
            });
        try
        {
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
            Assert.NotEqual(0, process.ExitCode);
            Assert.False(File.Exists(readyFile));
            var processOutput = DescribeProcessOutput(process);
            Assert.Contains(ModelLifecyclePreviousTokenExpiresEnvironmentName, processOutput, StringComparison.Ordinal);
            Assert.Contains("valid UTC DateTimeOffset", processOutput, StringComparison.Ordinal);
            AssertNoModelLifecycleTokenLeakage(processOutput, tempRoot);
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task RemovedExternalVisualProviderFlag_FailsClosedBeforeReadyFileWithoutSecretLeakage()
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        var secret = "removed-external-provider-secret-" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(tempRoot);

        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            artifactDbRoot,
            machineBackedLocal: false,
            externalVisualProviderFromEnv: true,
            environment: new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] =
                    "https://removed-visual-judge.example/v1/private",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "removed-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "removed-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret
            });
        try
        {
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
            Assert.Equal(64, process.ExitCode);
            Assert.False(File.Exists(readyFile));
            var processOutput = DescribeProcessOutput(process);
            Assert.Contains("selection was removed", processOutput, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("local evaluation only", processOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(secret, processOutput, StringComparison.Ordinal);
            Assert.DoesNotContain("/v1/private", processOutput, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }



    [Fact(Timeout = 120000)]
    public async Task ProductionHostStartup_ReconcilesExactDurableStoresAndRestartIsIdempotent()
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        var stateRoot = Path.Combine(
            artifactDbRoot,
            JsonDesignConversionPostEvaluationStateStore.DirectoryName);
        var budgetRoot = Path.Combine(
            tempRoot,
            "runtime-data",
            "visual-cost-budget-store");
        Directory.CreateDirectory(tempRoot);
        var stateStore = new JsonDesignConversionPostEvaluationStateStore(stateRoot);
        var budgetStore = new JsonlDesignConversionVisualCostBudgetStore(budgetRoot);
        var pending = await CreateAccountingReconciliationPendingSettlementAsync(
            stateStore,
            budgetStore,
            "live-host-startup-reconciliation");
        var journalPath = budgetStore.RecordsPath;

        using var firstProcess = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            artifactDbRoot,
            machineBackedLocal: false);
        try
        {
            using var firstReady = await WaitForReadyFileAsync(
                firstProcess,
                readyFile,
                TimeSpan.FromSeconds(75));
            var firstStatus = firstReady.RootElement.GetProperty(
                "accountingReconciliation");
            Assert.True(firstStatus.GetProperty("enabled").GetBoolean());
            Assert.Equal("startup_one_shot", firstStatus.GetProperty("schedule").GetString());
            Assert.Equal("completed", firstStatus.GetProperty("status").GetString());
            Assert.Equal(1, firstStatus.GetProperty("examinedCount").GetInt32());
            Assert.Equal(1, firstStatus.GetProperty("reconciledCount").GetInt32());
            Assert.Equal(0, firstStatus.GetProperty("failedCount").GetInt32());
            Assert.Equal(0, firstStatus.GetProperty("remainingPendingCount").GetInt32());
            Assert.False(firstStatus.GetProperty("trainingEnabledByReconciliation").GetBoolean());
            Assert.False(firstStatus.GetProperty("degraded").GetBoolean());
            var afterFirst = await stateStore.ReadAsync(pending.StateRef);
            Assert.True(afterFirst.IsSuccess, afterFirst.ErrorMessage);
            Assert.Equal(
                JsonDesignConversionPostEvaluationStateStore.AccountingSettled,
                afterFirst.Value!.AccountingStatus);
            var journalLinesAfterFirst = File.ReadAllLines(journalPath).Length;
            Assert.Equal(2, journalLinesAfterFirst);

            using var client = new HttpClient
            {
                BaseAddress = new Uri(
                    firstReady.RootElement.GetProperty("baseUrl").GetString()!)
            };
            using var healthResponse = await client.GetAsync("/__pass143/health");
            using var health = JsonDocument.Parse(
                await healthResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
            Assert.Equal("ok", health.RootElement.GetProperty("status").GetString());
            Assert.Equal(
                "completed",
                health.RootElement
                    .GetProperty("accountingReconciliation")
                    .GetProperty("status")
                    .GetString());

            await StopProcessAsync(firstProcess);
            File.Delete(readyFile);

            using var secondProcess = StartServerProcess(
                repoRoot,
                projectPath,
                readyFile,
                artifactDbRoot,
                machineBackedLocal: false);
            try
            {
                using var secondReady = await WaitForReadyFileAsync(
                    secondProcess,
                    readyFile,
                    TimeSpan.FromSeconds(75));
                var secondStatus = secondReady.RootElement.GetProperty(
                    "accountingReconciliation");
                Assert.Equal("completed", secondStatus.GetProperty("status").GetString());
                Assert.Equal(0, secondStatus.GetProperty("examinedCount").GetInt32());
                Assert.Equal(0, secondStatus.GetProperty("reconciledCount").GetInt32());
                Assert.Equal(
                    journalLinesAfterFirst,
                    File.ReadAllLines(journalPath).Length);
                var afterRestart = await stateStore.ReadAsync(pending.StateRef);
                Assert.True(afterRestart.IsSuccess, afterRestart.ErrorMessage);
                Assert.Equal(
                    JsonDesignConversionPostEvaluationStateStore.AccountingSettled,
                    afterRestart.Value!.AccountingStatus);
            }
            finally
            {
                await StopProcessAsync(secondProcess);
            }
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(firstProcess, tempRoot);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ProductionHostStartup_ActualChildProcessCannotEnterWhileExactLeaseIsHeld()
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        var stateRoot = Path.Combine(
            artifactDbRoot,
            JsonDesignConversionPostEvaluationStateStore.DirectoryName);
        var budgetRoot = Path.Combine(
            tempRoot,
            "runtime-data",
            "visual-cost-budget-store");
        Directory.CreateDirectory(tempRoot);

        var acquired = await DesignConversionAccountingReconciliationLease
            .TryAcquireAsync(
                stateRoot,
                budgetRoot,
                DesignConversionAccountingReconciliationHostedService
                    .ProductionHostScope,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(10));
        Assert.True(acquired.IsSuccess, acquired.ErrorMessage);
        var heldLease = acquired.Value!;
        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            artifactDbRoot,
            machineBackedLocal: false);
        try
        {
            using var ready = await WaitForReadyFileAsync(
                process,
                readyFile,
                TimeSpan.FromSeconds(75));
            var readyRoot = ready.RootElement;
            var status = readyRoot.GetProperty("accountingReconciliation");
            Assert.True(status.GetProperty("enabled").GetBoolean());
            Assert.Equal(
                "skipped_cross_process_lease_held",
                status.GetProperty("status").GetString());
            Assert.False(status.GetProperty("completedBatchResult").GetBoolean());
            Assert.Equal(
                DesignConversionAccountingReconciliationLease.LeaseHeldErrorCode,
                status.GetProperty("lastErrorCode").GetString());
            Assert.Equal(
                DesignConversionAccountingReconciliationLease.LeaseHeldErrorCode,
                status.GetProperty("terminalErrorCode").GetString());
            Assert.Equal(0, status.GetProperty("examinedCount").GetInt32());
            Assert.Equal(0, status.GetProperty("reconciledCount").GetInt32());
            Assert.Equal(0, status.GetProperty("failedCount").GetInt32());
            Assert.False(
                status.GetProperty("trainingEnabledByReconciliation").GetBoolean());
            Assert.True(status.GetProperty("degraded").GetBoolean());
            Assert.False(readyRoot.GetProperty("productReady").GetBoolean());

            using var client = new HttpClient
            {
                BaseAddress = new Uri(readyRoot.GetProperty("baseUrl").GetString()!)
            };
            using var healthResponse = await client.GetAsync("/__pass143/health");
            using var health = JsonDocument.Parse(
                await healthResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
            Assert.Equal(
                "degraded",
                health.RootElement.GetProperty("status").GetString());
            var healthStatus = health.RootElement.GetProperty(
                "accountingReconciliation");
            Assert.Equal(
                DesignConversionAccountingReconciliationLease.LeaseHeldErrorCode,
                healthStatus.GetProperty("terminalErrorCode").GetString());
            Assert.False(
                healthStatus
                    .GetProperty("trainingEnabledByReconciliation")
                    .GetBoolean());
        }
        finally
        {
            await StopProcessAsync(process);
            heldLease.Dispose();
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ProductionHostStartup_CorruptStateInventoryIsDegradedNotReadyAndNeverEnablesTraining()
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(
            repoRoot,
            "tools",
            "DesignConversionLiveLocalServer",
            "DesignConversionLiveLocalServer.csproj");
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server-tests",
            Guid.NewGuid().ToString("N"));
        var readyFile = Path.Combine(tempRoot, "ready.json");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        var stateRoot = Path.Combine(
            artifactDbRoot,
            JsonDesignConversionPostEvaluationStateStore.DirectoryName);
        Directory.CreateDirectory(stateRoot);
        await File.WriteAllTextAsync(
            Path.Combine(stateRoot, "unexpected-inventory-entry.txt"),
            "corrupt-inventory",
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            artifactDbRoot,
            machineBackedLocal: false);
        try
        {
            using var ready = await WaitForReadyFileAsync(
                process,
                readyFile,
                TimeSpan.FromSeconds(75));
            var readyRoot = ready.RootElement;
            var status = readyRoot.GetProperty("accountingReconciliation");
            Assert.True(status.GetProperty("enabled").GetBoolean());
            Assert.Equal("failed", status.GetProperty("status").GetString());
            Assert.False(status.GetProperty("completedBatchResult").GetBoolean());
            Assert.Equal(
                JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
                status.GetProperty("lastErrorCode").GetString());
            Assert.Equal(
                JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
                status.GetProperty("terminalErrorCode").GetString());
            Assert.Equal(0, status.GetProperty("examinedCount").GetInt32());
            Assert.Equal(0, status.GetProperty("reconciledCount").GetInt32());
            Assert.Equal(0, status.GetProperty("failedCount").GetInt32());
            Assert.Equal(0, status.GetProperty("remainingPendingCount").GetInt32());
            Assert.False(
                status.GetProperty("trainingEnabledByReconciliation").GetBoolean());
            Assert.True(status.GetProperty("degraded").GetBoolean());
            Assert.False(readyRoot.GetProperty("productReady").GetBoolean());

            using var client = new HttpClient
            {
                BaseAddress = new Uri(readyRoot.GetProperty("baseUrl").GetString()!)
            };
            using var healthResponse = await client.GetAsync("/__pass143/health");
            using var health = JsonDocument.Parse(
                await healthResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
            Assert.Equal(
                "degraded",
                health.RootElement.GetProperty("status").GetString());
            var healthStatus = health.RootElement.GetProperty(
                "accountingReconciliation");
            Assert.Equal(
                JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
                healthStatus.GetProperty("terminalErrorCode").GetString());
            Assert.False(
                healthStatus
                    .GetProperty("trainingEnabledByReconciliation")
                    .GetBoolean());
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    private static async Task<DesignConversionPostEvaluationState>
        CreateAccountingReconciliationPendingSettlementAsync(
            JsonDesignConversionPostEvaluationStateStore states,
            JsonlDesignConversionVisualCostBudgetStore budget,
            string seed)
    {
        var userRef = JsonlDesignConversionVisualCostLedger
            .BuildPseudonymousUserRef("live-host-reconciliation-user")
            .Value!;
        var period = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(
            new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero));
        var reservationRef =
            JsonlDesignConversionVisualCostBudgetStore.BuildCanonicalReservationRef(seed);
        var reserved = await budget.TryReserveAsync(
            new DesignConversionVisualCostReservationRequest(
                reservationRef,
                userRef,
                "paid",
                period.StartUtc,
                period.EndExclusiveUtc,
                1m,
                0.10m,
                new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero)));
        Assert.True(reserved.IsSuccess, reserved.ErrorMessage);
        Assert.True(reserved.Value!.Reserved);

        var initial = new DesignConversionPostEvaluationState(
            JsonDesignConversionPostEvaluationStateStore.BuildStateRef(reservationRef),
            reservationRef,
            userRef,
            period.StartUtc,
            period.EndExclusiveUtc,
            0.08m,
            JsonDesignConversionPostEvaluationStateStore.AccountingPending,
            JsonDesignConversionPostEvaluationStateStore.DesiredSettle,
            SettlementAttempted: false,
            SettlementDefinitivelyNotApplied: false,
            ReleaseAttempted: false,
            JsonDesignConversionPostEvaluationStateStore.PersistenceProvisional,
            LedgerRecordId: string.Empty,
            ArtifactRecordId: string.Empty,
            CompletionBindingKind: string.Empty,
            CompletionBindingRef: string.Empty,
            LastErrorCode: string.Empty,
            new DateTimeOffset(2026, 7, 15, 12, 0, 1, TimeSpan.Zero),
            Version: 1);
        var created = await states.WriteAsync(initial);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        var pending = await states.WriteAsync(
            created.Value! with
            {
                AccountingStatus =
                    JsonDesignConversionPostEvaluationStateStore.AccountingReconciliationRequired,
                SettlementAttempted = true,
                LastErrorCode =
                    DesignConversionThinClientJobRuntime.AccountingReconciliationRequiredErrorCode,
                UpdatedAtUtc = initial.UpdatedAtUtc.AddSeconds(1)
            });
        Assert.True(pending.IsSuccess, pending.ErrorMessage);
        return pending.Value!;
    }

    private static async Task WriteMeasurementGateThresholdConfigurationAsync(string path)
    {
        await File.WriteAllTextAsync(
            path,
            """
            {
              "required_held_out_quality_score": 0.95,
              "max_runtime_ms": 1000,
              "max_cost_usd": 1.5,
              "max_peak_ram_mb": 1024
            }
            """,
            new UTF8Encoding(false));
    }

    private static async Task<HttpResponseMessage> PostLiveThresholdTrainingRequestAsync(
        HttpClient client,
        bool includeMeasurementGateRequest)
    {
        var options = new DesignConversionTrainingPipelineOptions(
            RunId: "live-measurement-threshold-run",
            BatchSize: 1,
            MeasurementGateRequest: includeMeasurementGateRequest
                ? new DesignConversionMeasurementGateRequest(
                    GateId: "live-measurement-threshold-gate",
                    UnitId: DesignConversionDpoUnits.CriticUnit,
                    HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                        HeldOutRecordCount: 1,
                        HeldOutQualityScore: 0.99,
                        RuntimeMs: 1,
                        CostUsd: 0m,
                        PeakRamMb: 1,
                        EvidenceRefs:
                        ["artifact://caller-measurement/live-threshold/heldout.json"]),
                    IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                        SourceDisjoint: true,
                        LeakageDetected: false,
                        HiddenDeterminismDetected: false,
                        AblationPassed: true,
                        EvidenceRefs:
                        ["artifact://caller-measurement/live-threshold/integrity.json"]),
                    Thresholds: new DesignConversionMeasurementGateThresholds(
                        RequiredHeldOutQualityScore: 0.1,
                        MaxRuntimeMs: long.MaxValue,
                        MaxCostUsd: decimal.MaxValue,
                        MaxPeakRamMb: long.MaxValue))
                : null);
        var json = JsonSerializer.Serialize(
            new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionRun,
                intakeId = "live-measurement-threshold-intake",
                tenantId = "tenant:live-measurement-threshold-intake",
                idempotencyKey = "idempotency:live-measurement-threshold-intake:run",
                options
            },
            DesignConversionModelServerHttpHandler.JsonOptions);

        return await client.PostAsync(
            DesignConversionModelServerTrainingIntakeHandler.EndpointPath,
            new StringContent(json, Encoding.UTF8, "application/json"));
    }

    private static void AssertMeasurementGateThresholdPublicBoolean(
        JsonElement root,
        bool expectedConfigured,
        string privateConfigurationPath)
    {
        var fields = root
            .EnumerateObject()
            .Where(property => property.Name.Contains(
                "measurementGateThreshold",
                StringComparison.Ordinal))
            .ToArray();
        var field = Assert.Single(fields);
        Assert.Equal("measurementGateThresholdsConfigured", field.Name);
        Assert.Equal(
            expectedConfigured ? JsonValueKind.True : JsonValueKind.False,
            field.Value.ValueKind);
        Assert.DoesNotContain(
            privateConfigurationPath,
            root.GetRawText(),
            StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadLiveLocalProgramSource()
        => File.ReadAllText(
            Path.Combine(
                FindRepoRoot(),
                "tools",
                "DesignConversionLiveLocalServer",
                "Program.cs"),
            Encoding.UTF8);

    private static Assembly LoadBuiltLiveLocalAssembly()
    {
        var repoRoot = FindRepoRoot();
        var testOutputDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        var currentConfiguration = testOutputDirectory.Parent?.Name;
        var configurations = new[] { currentConfiguration, "Debug", "Release" }
            .Where(configuration => !string.IsNullOrWhiteSpace(configuration))
            .Distinct(StringComparer.OrdinalIgnoreCase);
        var candidates = configurations
            .Select(configuration => Path.Combine(
                repoRoot,
                "tools",
                "DesignConversionLiveLocalServer",
                "bin",
                configuration!,
                "net9.0",
                "DesignConversionLiveLocalServer.dll"))
            .ToArray();
        var assemblyPath = candidates.FirstOrDefault(File.Exists);
        Assert.True(
            assemblyPath is not null,
            "Build DesignConversionLiveLocalServer before running its reflection-only composition test. " +
            "Checked: " + string.Join(", ", candidates));
        return Assembly.LoadFrom(assemblyPath!);
    }

    private static void AssertExactArgumentStoredInStateMachineField(
        MethodInfo asyncStub,
        int parameterPosition,
        FieldInfo expectedField)
    {
        var instructions = DecodeIlFailClosed(asyncStub);
        var matchingStores = instructions
            .Select((instruction, index) => (instruction, index))
            .Where(candidate =>
                candidate.instruction.OpCode == OpCodes.Stfld
                && candidate.instruction.Operand is FieldInfo field
                && IsSameField(field, expectedField))
            .ToArray();
        var store = Assert.Single(matchingStores);
        var sourceIndex = PreviousNonNopInstructionIndex(instructions, store.index);
        var argumentIndex = parameterPosition + (asyncStub.IsStatic ? 0 : 1);
        Assert.True(
            IsLoadArgument(instructions[sourceIndex], argumentIndex),
            $"State-machine field {expectedField.Name} must be initialized directly from " +
            $"argument {argumentIndex}; observed {instructions[sourceIndex].OpCode.Name}.");
    }

    private static void AssertExactInstanceFieldPassedAsFinalArgumentAtEveryCall(
        MethodInfo caller,
        MethodInfo target,
        FieldInfo expectedField,
        int expectedCallCount)
    {
        var instructions = DecodeIlFailClosed(caller);
        var calls = instructions
            .Select((instruction, index) => (instruction, index))
            .Where(candidate =>
                candidate.instruction.OpCode is var opCode
                && (opCode == OpCodes.Call || opCode == OpCodes.Callvirt)
                && candidate.instruction.Operand is MethodBase calledMethod
                && IsSameMethod(calledMethod, target))
            .ToArray();
        Assert.Equal(expectedCallCount, calls.Length);

        foreach (var call in calls)
        {
            var fieldLoadIndex = PreviousNonNopInstructionIndex(instructions, call.index);
            var fieldLoad = instructions[fieldLoadIndex];
            Assert.True(
                fieldLoad.OpCode == OpCodes.Ldfld
                && fieldLoad.Operand is FieldInfo loadedField
                && IsSameField(loadedField, expectedField),
                $"Call to {target.Name} at IL_{call.instruction.Offset:x4} must load its final " +
                $"argument directly from state-machine field {expectedField.Name}; observed " +
                $"{fieldLoad.OpCode.Name}.");
            var receiverLoadIndex = PreviousNonNopInstructionIndex(instructions, fieldLoadIndex);
            Assert.True(
                IsLoadArgument(instructions[receiverLoadIndex], argumentIndex: 0),
                $"Field {expectedField.Name} must be loaded from the current state-machine instance.");
        }
    }

    private static void AssertExactInstanceFieldsPassedAsFinalArgumentsAtEveryCall(
        MethodInfo caller,
        MethodInfo target,
        IReadOnlyList<FieldInfo> expectedFields,
        int expectedCallCount)
    {
        var instructions = DecodeIlFailClosed(caller);
        var calls = instructions
            .Select((instruction, index) => (instruction, index))
            .Where(candidate =>
                candidate.instruction.OpCode is var opCode
                && (opCode == OpCodes.Call || opCode == OpCodes.Callvirt)
                && candidate.instruction.Operand is MethodBase calledMethod
                && IsSameMethod(calledMethod, target))
            .ToArray();
        Assert.Equal(expectedCallCount, calls.Length);

        foreach (var call in calls)
        {
            var cursor = call.index;
            foreach (var expectedField in expectedFields.Reverse())
            {
                var fieldLoadIndex = PreviousNonNopInstructionIndex(
                    instructions,
                    cursor);
                var fieldLoad = instructions[fieldLoadIndex];
                Assert.True(
                    fieldLoad.OpCode == OpCodes.Ldfld
                    && fieldLoad.Operand is FieldInfo loadedField
                    && IsSameField(loadedField, expectedField),
                    $"Call to {target.Name} at IL_{call.instruction.Offset:x4} must load " +
                    $"trailing field {expectedField.Name}; observed {fieldLoad.OpCode.Name}.");
                var receiverLoadIndex = PreviousNonNopInstructionIndex(
                    instructions,
                    fieldLoadIndex);
                Assert.True(
                    IsLoadArgument(instructions[receiverLoadIndex], argumentIndex: 0),
                    $"Field {expectedField.Name} must be loaded from the current " +
                    "state-machine instance.");
                cursor = receiverLoadIndex;
            }
        }
    }

    private static void AssertStateMachineFieldIsNeverReassigned(
        MethodInfo moveNext,
        FieldInfo expectedField)
    {
        var instructions = DecodeIlFailClosed(moveNext);
        Assert.DoesNotContain(
            instructions,
            instruction =>
                (instruction.OpCode == OpCodes.Stfld || instruction.OpCode == OpCodes.Ldflda)
                && instruction.Operand is FieldInfo field
                && IsSameField(field, expectedField));
    }

    private static void AssertProviderFieldFlowAnalyzerControls()
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;
        var fixtureType = typeof(ProviderFieldFlowIlFixture);
        var sink = fixtureType.GetMethod("Consume", staticFlags)!;
        var expectedField = fixtureType.GetField("_expectedProvider", instanceFlags)!;
        var forwardExpected = fixtureType.GetMethod("ForwardExpected", instanceFlags)!;
        AssertExactInstanceFieldPassedAsFinalArgumentAtEveryCall(
            forwardExpected,
            sink,
            expectedField,
            expectedCallCount: 1);
        AssertStateMachineFieldIsNeverReassigned(forwardExpected, expectedField);
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() =>
            AssertExactInstanceFieldPassedAsFinalArgumentAtEveryCall(
                fixtureType.GetMethod("ForwardNull", instanceFlags)!,
                sink,
                expectedField,
                expectedCallCount: 1));
        var overwriteThenForward = fixtureType.GetMethod("OverwriteThenForward", instanceFlags)!;
        AssertExactInstanceFieldPassedAsFinalArgumentAtEveryCall(
            overwriteThenForward,
            sink,
            expectedField,
            expectedCallCount: 1);
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() =>
            AssertStateMachineFieldIsNeverReassigned(overwriteThenForward, expectedField));
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() =>
            AssertExactInstanceFieldPassedAsFinalArgumentAtEveryCall(
                fixtureType.GetMethod("ForwardOther", instanceFlags)!,
                sink,
                expectedField,
                expectedCallCount: 1));
    }

    private static IReadOnlyList<DecodedIlInstruction> DecodeIlFailClosed(MethodInfo method)
    {
        var body = method.GetMethodBody();
        Assert.NotNull(body);
        var il = body!.GetILAsByteArray();
        Assert.NotNull(il);
        var opCodes = typeof(OpCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(OpCode))
            .Select(field => (OpCode)field.GetValue(null)!)
            .ToDictionary(opCode => unchecked((ushort)opCode.Value));
        var instructions = new List<DecodedIlInstruction>();
        var offset = 0;

        while (offset < il!.Length)
        {
            var instructionOffset = offset;
            RequireIlBytes(method, il, offset, 1);
            ushort encoded = il[offset++];
            if (encoded == 0xfe)
            {
                RequireIlBytes(method, il, offset, 1);
                encoded = (ushort)(0xfe00 | il[offset++]);
            }
            Assert.True(
                opCodes.TryGetValue(encoded, out var opCode),
                $"Unknown IL opcode 0x{encoded:x4} in {method.Name} at IL_{instructionOffset:x4}.");

            object? operand;
            try
            {
                operand = ReadIlOperand(method, opCode, il, ref offset);
            }
            catch (Xunit.Sdk.XunitException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Xunit.Sdk.XunitException(
                    $"Failed to resolve {opCode.Name} operand in {method.Name} at " +
                    $"IL_{instructionOffset:x4}: {ex.GetType().Name}: {ex.Message}");
            }

            instructions.Add(new DecodedIlInstruction(instructionOffset, opCode, operand));
        }

        Assert.Equal(il.Length, offset);
        return instructions;
    }

    private static object? ReadIlOperand(
        MethodInfo method,
        OpCode opCode,
        byte[] il,
        ref int offset)
    {
        var typeArguments = method.DeclaringType?.GetGenericArguments();
        var methodArguments = method.GetGenericArguments();
        switch (opCode.OperandType)
        {
            case OperandType.InlineNone:
                return null;
            case OperandType.ShortInlineI:
                RequireIlBytes(method, il, offset, 1);
                return unchecked((sbyte)il[offset++]);
            case OperandType.ShortInlineVar:
                RequireIlBytes(method, il, offset, 1);
                return il[offset++];
            case OperandType.InlineVar:
                RequireIlBytes(method, il, offset, 2);
                var variable = BitConverter.ToUInt16(il, offset);
                offset += 2;
                return variable;
            case OperandType.InlineI:
                return ReadIlInt32(method, il, ref offset);
            case OperandType.InlineI8:
                RequireIlBytes(method, il, offset, 8);
                var int64 = BitConverter.ToInt64(il, offset);
                offset += 8;
                return int64;
            case OperandType.ShortInlineR:
                RequireIlBytes(method, il, offset, 4);
                var single = BitConverter.ToSingle(il, offset);
                offset += 4;
                return single;
            case OperandType.InlineR:
                RequireIlBytes(method, il, offset, 8);
                var @double = BitConverter.ToDouble(il, offset);
                offset += 8;
                return @double;
            case OperandType.ShortInlineBrTarget:
                RequireIlBytes(method, il, offset, 1);
                var shortDelta = unchecked((sbyte)il[offset++]);
                return offset + shortDelta;
            case OperandType.InlineBrTarget:
                var branchDelta = ReadIlInt32(method, il, ref offset);
                return offset + branchDelta;
            case OperandType.InlineSwitch:
                var count = ReadIlInt32(method, il, ref offset);
                Assert.True(count >= 0, $"Negative switch target count in {method.Name}.");
                Assert.True(
                    count <= (il.Length - offset) / 4,
                    $"Truncated switch target table in {method.Name}.");
                var switchBase = offset + (count * 4);
                var targets = new int[count];
                for (var index = 0; index < count; index++)
                    targets[index] = switchBase + ReadIlInt32(method, il, ref offset);
                return targets;
            case OperandType.InlineString:
                return method.Module.ResolveString(ReadIlInt32(method, il, ref offset));
            case OperandType.InlineSig:
                return method.Module.ResolveSignature(ReadIlInt32(method, il, ref offset));
            case OperandType.InlineMethod:
                return method.Module.ResolveMethod(
                    ReadIlInt32(method, il, ref offset),
                    typeArguments,
                    methodArguments)
                    ?? throw new Xunit.Sdk.XunitException(
                        $"Unresolved method token in {method.Name}.");
            case OperandType.InlineField:
                return method.Module.ResolveField(
                    ReadIlInt32(method, il, ref offset),
                    typeArguments,
                    methodArguments)
                    ?? throw new Xunit.Sdk.XunitException(
                        $"Unresolved field token in {method.Name}.");
            case OperandType.InlineType:
                return method.Module.ResolveType(
                    ReadIlInt32(method, il, ref offset),
                    typeArguments,
                    methodArguments);
            case OperandType.InlineTok:
                return method.Module.ResolveMember(
                    ReadIlInt32(method, il, ref offset),
                    typeArguments,
                    methodArguments)
                    ?? throw new Xunit.Sdk.XunitException(
                        $"Unresolved member token in {method.Name}.");
            default:
                throw new Xunit.Sdk.XunitException(
                    $"Unsupported IL operand type {opCode.OperandType} in {method.Name}.");
        }
    }

    private static int ReadIlInt32(MethodInfo method, byte[] il, ref int offset)
    {
        RequireIlBytes(method, il, offset, 4);
        var value = BitConverter.ToInt32(il, offset);
        offset += 4;
        return value;
    }

    private static void RequireIlBytes(
        MethodInfo method,
        byte[] il,
        int offset,
        int count)
        => Assert.True(
            offset >= 0 && count >= 0 && offset <= il.Length - count,
            $"Truncated IL for {method.Name}: need {count} bytes at offset {offset}, " +
            $"length is {il.Length}.");

    private static int PreviousNonNopInstructionIndex(
        IReadOnlyList<DecodedIlInstruction> instructions,
        int exclusiveIndex)
    {
        for (var index = exclusiveIndex - 1; index >= 0; index--)
        {
            if (instructions[index].OpCode != OpCodes.Nop)
                return index;
        }

        throw new Xunit.Sdk.XunitException(
            $"No preceding non-nop IL instruction before instruction index {exclusiveIndex}.");
    }

    private static bool IsLoadArgument(DecodedIlInstruction instruction, int argumentIndex)
        => instruction.OpCode == OpCodes.Ldarg_0 && argumentIndex == 0
           || instruction.OpCode == OpCodes.Ldarg_1 && argumentIndex == 1
           || instruction.OpCode == OpCodes.Ldarg_2 && argumentIndex == 2
           || instruction.OpCode == OpCodes.Ldarg_3 && argumentIndex == 3
           || (instruction.OpCode == OpCodes.Ldarg
               || instruction.OpCode == OpCodes.Ldarg_S)
           && Convert.ToInt32(instruction.Operand, CultureInfo.InvariantCulture) == argumentIndex;

    private static bool IsSameMethod(MethodBase candidate, MethodInfo expected)
        => candidate.Module == expected.Module
           && candidate.MetadataToken == expected.MetadataToken;

    private static bool IsSameField(FieldInfo candidate, FieldInfo expected)
        => candidate.Module == expected.Module
           && candidate.MetadataToken == expected.MetadataToken;

    private sealed record DecodedIlInstruction(
        int Offset,
        OpCode OpCode,
        object? Operand);

    private sealed class ProviderFieldFlowIlFixture
    {
        private object? _expectedProvider = new();
        private readonly object _otherProvider = new();

        private object? ForwardExpected()
            => Consume(_expectedProvider);

        private object? ForwardNull()
            => Consume(null);

        private object? ForwardOther()
            => Consume(_otherProvider);

        private object? OverwriteThenForward()
        {
            _expectedProvider = null;
            return Consume(_expectedProvider);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static object? Consume(object? provider)
            => provider;
    }

    private static IDesignConversionDpoPolicyBaseProvider? ReadDpoPolicyBaseProvider(
        JsonlDesignConversionLocalArtifactDb artifactDb)
        => (IDesignConversionDpoPolicyBaseProvider?)
            typeof(JsonlDesignConversionLocalArtifactDb)
                .GetField(
                    "_dpoPolicyBaseProvider",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(artifactDb);

    private static void AssertProgramReadyFileLifecycleWiring(string source)
    {
        var codeOnly = MaskNonCodeCSharpTextPreservingStructure(source);
        const string lifecycleInvocation =
            "DesignConversionLiveLocalReadyFileLifecycle.RunAsync";
        var invocationArguments = ReadSingleBalancedInvocationArguments(
            codeOnly,
            lifecycleInvocation);

        var startIndex = invocationArguments.IndexOf(
            "app.StartAsync()",
            StringComparison.Ordinal);
        var writeIndex = invocationArguments.IndexOf(
            "WriteReadyFile(",
            StringComparison.Ordinal);
        var waitIndex = invocationArguments.IndexOf(
            "app.WaitForShutdownAsync()",
            StringComparison.Ordinal);
        var cleanupIndex = invocationArguments.IndexOf(
            "Pass143ReadyFileCleanup.TryDelete(options.ReadyFile)",
            StringComparison.Ordinal);

        Assert.True(startIndex >= 0);
        Assert.True(writeIndex > startIndex);
        Assert.True(waitIndex > writeIndex);
        Assert.True(cleanupIndex > waitIndex);
        Assert.Equal(1, CountOccurrences(codeOnly, lifecycleInvocation));
        Assert.Equal(1, CountOccurrences(codeOnly, "app.StartAsync()"));
        Assert.Equal(1, CountOccurrences(codeOnly, "app.WaitForShutdownAsync()"));
        Assert.Equal(2, CountOccurrences(codeOnly, "WriteReadyFile("));
        Assert.Equal(
            2,
            CountOccurrences(
                codeOnly,
                "Pass143ReadyFileCleanup.TryDelete(options.ReadyFile)"));
    }

    private static string MaskNonCodeCSharpTextPreservingStructure(string source)
    {
        var codeOnly = source.ToCharArray();
        var index = 0;
        while (index < source.Length)
        {
            if (source[index] == '/'
                && index + 1 < source.Length
                && source[index + 1] == '/')
            {
                var end = index + 2;
                while (end < source.Length && source[end] is not '\r' and not '\n')
                    end++;
                MaskNonNewlineCharacters(codeOnly, index, end);
                index = end;
                continue;
            }

            if (source[index] == '/'
                && index + 1 < source.Length
                && source[index + 1] == '*')
            {
                var end = source.IndexOf("*/", index + 2, StringComparison.Ordinal);
                end = end < 0 ? source.Length : end + 2;
                MaskNonNewlineCharacters(codeOnly, index, end);
                index = end;
                continue;
            }

            if (TryFindStringLiteralEnd(source, index, out var stringEnd))
            {
                MaskNonNewlineCharacters(codeOnly, index, stringEnd);
                index = stringEnd;
                continue;
            }

            if (source[index] == '\'')
            {
                var characterEnd = FindCharacterLiteralEnd(source, index);
                MaskNonNewlineCharacters(codeOnly, index, characterEnd);
                index = characterEnd;
                continue;
            }

            index++;
        }

        return new string(codeOnly);
    }

    private static bool TryFindStringLiteralEnd(
        string source,
        int start,
        out int endExclusive)
    {
        endExclusive = start;
        var cursor = start;
        var interpolated = false;
        var verbatim = false;

        if (source[cursor] == '@')
        {
            verbatim = true;
            cursor++;
            if (cursor < source.Length && source[cursor] == '$')
            {
                interpolated = true;
                cursor++;
            }
        }
        else
        {
            while (cursor < source.Length && source[cursor] == '$')
            {
                interpolated = true;
                cursor++;
            }

            if (cursor < source.Length && source[cursor] == '@')
            {
                verbatim = true;
                cursor++;
            }
        }

        if (cursor >= source.Length || source[cursor] != '"')
            return false;

        var quoteCount = CountRun(source, cursor, '"');
        if (quoteCount >= 3)
        {
            var rawDelimiterLength = quoteCount;
            cursor += rawDelimiterLength;
            while (cursor < source.Length)
            {
                if (source[cursor] == '"')
                {
                    var closingQuoteCount = CountRun(source, cursor, '"');
                    if (closingQuoteCount >= rawDelimiterLength)
                    {
                        endExclusive = cursor + closingQuoteCount;
                        return true;
                    }
                    cursor += closingQuoteCount;
                    continue;
                }
                cursor++;
            }

            endExclusive = source.Length;
            return true;
        }

        cursor++;
        var interpolationBraceDepth = 0;
        while (cursor < source.Length)
        {
            var current = source[cursor];
            var next = cursor + 1 < source.Length ? source[cursor + 1] : '\0';

            if (interpolated && current == '{')
            {
                if (interpolationBraceDepth == 0 && next == '{')
                {
                    cursor += 2;
                    continue;
                }
                interpolationBraceDepth++;
                cursor++;
                continue;
            }

            if (interpolated && current == '}' && interpolationBraceDepth > 0)
            {
                interpolationBraceDepth--;
                cursor++;
                continue;
            }

            if (interpolationBraceDepth > 0)
            {
                if (current == '/'
                    && next == '/')
                {
                    cursor += 2;
                    while (cursor < source.Length && source[cursor] is not '\r' and not '\n')
                        cursor++;
                    continue;
                }

                if (current == '/'
                    && next == '*')
                {
                    var commentEnd = source.IndexOf(
                        "*/",
                        cursor + 2,
                        StringComparison.Ordinal);
                    cursor = commentEnd < 0 ? source.Length : commentEnd + 2;
                    continue;
                }

                if (TryFindStringLiteralEnd(source, cursor, out var nestedStringEnd))
                {
                    cursor = nestedStringEnd;
                    continue;
                }

                if (current == '\'')
                {
                    cursor = FindCharacterLiteralEnd(source, cursor);
                    continue;
                }

                cursor++;
                continue;
            }

            if (verbatim && current == '"' && next == '"')
            {
                cursor += 2;
                continue;
            }

            if (current == '"' && (verbatim || !IsEscaped(source, cursor)))
            {
                endExclusive = cursor + 1;
                return true;
            }

            if (!verbatim && current == '\\')
            {
                cursor = Math.Min(cursor + 2, source.Length);
                continue;
            }

            cursor++;
        }

        endExclusive = source.Length;
        return true;
    }

    private static int FindCharacterLiteralEnd(string source, int start)
    {
        var cursor = start + 1;
        while (cursor < source.Length)
        {
            if (source[cursor] == '\'' && !IsEscaped(source, cursor))
                return cursor + 1;
            if (source[cursor] == '\\')
                cursor = Math.Min(cursor + 2, source.Length);
            else
                cursor++;
        }
        return source.Length;
    }

    private static int CountRun(string source, int start, char value)
    {
        var end = start;
        while (end < source.Length && source[end] == value)
            end++;
        return end - start;
    }

    private static void MaskNonNewlineCharacters(
        char[] codeOnly,
        int start,
        int endExclusive)
    {
        for (var index = start; index < endExclusive; index++)
        {
            if (codeOnly[index] is not '\r' and not '\n')
                codeOnly[index] = ' ';
        }
    }

    private static string ReadSingleBalancedInvocationArguments(
        string source,
        string qualifiedMethodName)
    {
        var invocationStarts = new List<int>();
        var searchIndex = 0;
        while (searchIndex < source.Length)
        {
            var invocationStart = source.IndexOf(
                qualifiedMethodName,
                searchIndex,
                StringComparison.Ordinal);
            if (invocationStart < 0)
                break;

            invocationStarts.Add(invocationStart);
            searchIndex = invocationStart + qualifiedMethodName.Length;
        }

        var singleStart = Assert.Single(invocationStarts);
        var openParenthesis = source.IndexOf(
            '(',
            singleStart + qualifiedMethodName.Length);
        Assert.True(openParenthesis >= 0);

        var parenthesisDepth = 1;
        var braceDepth = 0;
        var bracketDepth = 0;
        var inLineComment = false;
        var inBlockComment = false;
        var inString = false;
        var inVerbatimString = false;
        var inCharacter = false;

        for (var index = openParenthesis + 1; index < source.Length; index++)
        {
            var current = source[index];
            var next = index + 1 < source.Length ? source[index + 1] : '\0';

            if (inLineComment)
            {
                if (current is '\r' or '\n')
                    inLineComment = false;
                continue;
            }

            if (inBlockComment)
            {
                if (current == '*' && next == '/')
                {
                    inBlockComment = false;
                    index++;
                }
                continue;
            }

            if (inString)
            {
                if (inVerbatimString && current == '"' && next == '"')
                {
                    index++;
                    continue;
                }

                if (current == '"' && (inVerbatimString || !IsEscaped(source, index)))
                {
                    inString = false;
                    inVerbatimString = false;
                }
                continue;
            }

            if (inCharacter)
            {
                if (current == '\'' && !IsEscaped(source, index))
                    inCharacter = false;
                continue;
            }

            if (current == '/' && next == '/')
            {
                inLineComment = true;
                index++;
                continue;
            }

            if (current == '/' && next == '*')
            {
                inBlockComment = true;
                index++;
                continue;
            }

            if (current == '"')
            {
                inString = true;
                inVerbatimString = index > 0 && source[index - 1] == '@';
                continue;
            }

            if (current == '\'')
            {
                inCharacter = true;
                continue;
            }

            switch (current)
            {
                case '(':
                    parenthesisDepth++;
                    break;
                case ')':
                    parenthesisDepth--;
                    if (parenthesisDepth == 0)
                    {
                        Assert.Equal(0, braceDepth);
                        Assert.Equal(0, bracketDepth);
                        return source[(openParenthesis + 1)..index];
                    }
                    break;
                case '{':
                    braceDepth++;
                    break;
                case '}':
                    braceDepth--;
                    Assert.True(braceDepth >= 0);
                    break;
                case '[':
                    bracketDepth++;
                    break;
                case ']':
                    bracketDepth--;
                    Assert.True(bracketDepth >= 0);
                    break;
            }
        }

        throw new Xunit.Sdk.XunitException(
            $"Could not find a balanced invocation for {qualifiedMethodName}.");
    }

    private static bool IsEscaped(string source, int index)
    {
        var slashCount = 0;
        for (var cursor = index - 1; cursor >= 0 && source[cursor] == '\\'; cursor--)
            slashCount++;
        return slashCount % 2 != 0;
    }

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }
        return count;
    }

    private static string CreateReadyFileLifecycleTempRoot()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-ready-file-lifecycle-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteReadyFileLifecycleTempRoot(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }

    private static Process StartServerProcess(
        string repoRoot,
        string projectPath,
        string readyFile,
        string? artifactDbRoot,
        bool? machineBackedLocal = null,
        bool externalVisualProviderFromEnv = false,
        bool scaffoldRenderer = false,
        string scaffoldRendererFlag = "--scaffold-renderer",
        string? browserExecutable = null,
        string? expectedSourceCommit = null,
        bool routeAdmissionEnabled = true,
        string? routeAdmissionCurrentToken = RouteAdmissionCurrentToken,
        string? routeAdmissionPreviousToken = RouteAdmissionPreviousToken,
        string? routeAdmissionPreviousTokenExpiresAtUtc = RouteAdmissionPreviousTokenExpiresAtUtc,
        string? routeAdmissionTenantId = RouteAdmissionTenantId,
        IReadOnlyDictionary<string, string>? environment = null,
        IReadOnlyList<string>? additionalArguments = null,
        string? measurementGateThresholdsFile = null)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ResolveDotnetExecutable(),
                WorkingDirectory = repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };
        var outputCapture = new ServerProcessOutputCapture();
        process.OutputDataReceived += (_, eventArgs) => outputCapture.AppendStdout(eventArgs.Data);
        process.ErrorDataReceived += (_, eventArgs) => outputCapture.AppendStderr(eventArgs.Data);
        foreach (var name in ExternalVisualProviderSettingNames())
            process.StartInfo.Environment.Remove(name);
        foreach (var name in ModelLifecycleSettingNames())
            process.StartInfo.Environment.Remove(name);
        process.StartInfo.Environment.Remove(
            DesignConversionModelServerFlow38QuarantineHandler.BearerTokenEnvironmentName);
        foreach (var pair in environment ?? new Dictionary<string, string>())
            process.StartInfo.Environment[pair.Key] = pair.Value;

        process.StartInfo.Environment["DOTNET_ROLL_FORWARD"] = "Major";
        process.StartInfo.ArgumentList.Add(ResolveServerAssemblyPath(projectPath));
        process.StartInfo.ArgumentList.Add("--ready-file");
        process.StartInfo.ArgumentList.Add(readyFile);
        if (machineBackedLocal is true)
            process.StartInfo.ArgumentList.Add("--machine-backed-local");
        else if (machineBackedLocal is false)
            process.StartInfo.ArgumentList.Add("--no-machine-backed-local");
        if (!string.IsNullOrWhiteSpace(artifactDbRoot))
        {
            process.StartInfo.ArgumentList.Add("--artifact-db-root");
            process.StartInfo.ArgumentList.Add(artifactDbRoot);
        }

        if (scaffoldRenderer)
            process.StartInfo.ArgumentList.Add(scaffoldRendererFlag);
        if (!string.IsNullOrWhiteSpace(browserExecutable))
        {
            process.StartInfo.ArgumentList.Add("--browser-executable");
            process.StartInfo.ArgumentList.Add(browserExecutable);
        }

        if (!string.IsNullOrWhiteSpace(expectedSourceCommit))
        {
            process.StartInfo.ArgumentList.Add("--expected-source-commit");
            process.StartInfo.ArgumentList.Add(expectedSourceCommit);
        }

        if (externalVisualProviderFromEnv)
            process.StartInfo.ArgumentList.Add("--external-visual-provider-from-env");
        if (!string.IsNullOrWhiteSpace(measurementGateThresholdsFile))
        {
            process.StartInfo.ArgumentList.Add("--measurement-gate-thresholds-file");
            process.StartInfo.ArgumentList.Add(measurementGateThresholdsFile);
        }

        foreach (var argument in additionalArguments ?? [])
            process.StartInfo.ArgumentList.Add(argument);
        if (routeAdmissionEnabled)
        {
            process.StartInfo.ArgumentList.Add("--route-admission-enabled");
            if (!string.IsNullOrWhiteSpace(routeAdmissionCurrentToken))
            {
                process.StartInfo.ArgumentList.Add("--route-admission-current-token");
                process.StartInfo.ArgumentList.Add(routeAdmissionCurrentToken);
            }

            if (!string.IsNullOrWhiteSpace(routeAdmissionPreviousToken))
            {
                process.StartInfo.ArgumentList.Add("--route-admission-previous-token");
                process.StartInfo.ArgumentList.Add(routeAdmissionPreviousToken);
            }

            if (!string.IsNullOrWhiteSpace(routeAdmissionPreviousTokenExpiresAtUtc))
            {
                process.StartInfo.ArgumentList.Add("--route-admission-previous-token-expires-at-utc");
                process.StartInfo.ArgumentList.Add(routeAdmissionPreviousTokenExpiresAtUtc);
            }

            if (!string.IsNullOrWhiteSpace(routeAdmissionTenantId))
            {
                process.StartInfo.ArgumentList.Add("--route-admission-tenant-id");
                process.StartInfo.ArgumentList.Add(routeAdmissionTenantId);
            }
        }

        process.Start();
        ServerProcessOutputs[process.Id] = outputCapture;
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return process;
    }

    private static void AssertServerProcessOmittedMachineModeFlags(Process process)
    {
        Assert.DoesNotContain("--machine-backed-local", process.StartInfo.ArgumentList);
        Assert.DoesNotContain("--no-machine-backed-local", process.StartInfo.ArgumentList);
    }

    private static void AssertSourceIdentity(
        JsonElement root,
        string repoRoot,
        string? expectedSourceCommit = null,
        bool? expectedSourceCommitMatches = null,
        bool expectedIdentityConsistency = true)
    {
        var identity = root.GetProperty("sourceIdentity");
        var entry = identity.GetProperty("entryAssembly");
        var modelServer = identity.GetProperty("modelServerAssembly");
        var domain = identity.GetProperty("designConversionDomainAssembly");

        AssertAssemblySourceIdentity(entry, "DesignConversionLiveLocalServer");
        AssertAssemblySourceIdentity(modelServer, "ModelServer");
        AssertAssemblySourceIdentity(domain, "44.DesignConversionDomain");
        Assert.True(identity.GetProperty("allSourceCommitsAvailable").GetBoolean());
        Assert.True(identity.GetProperty("sourceCommitsAligned").GetBoolean());
        Assert.Equal(3, identity.GetProperty("availableSourceCommitCount").GetInt32());
        Assert.Equal(
            expectedIdentityConsistency,
            identity.GetProperty("identityConsistency").GetBoolean());
        Assert.Equal(
            !string.IsNullOrWhiteSpace(expectedSourceCommit),
            identity.GetProperty("expectedSourceCommitConfigured").GetBoolean());

        if (string.IsNullOrWhiteSpace(expectedSourceCommit))
        {
            Assert.False(identity.GetProperty("expectedSourceCommitValid").GetBoolean());
            Assert.Equal(string.Empty, identity.GetProperty("expectedSourceCommit").GetString());
            Assert.Equal(JsonValueKind.Null, identity.GetProperty("expectedSourceCommitMatches").ValueKind);
            Assert.Equal("not_configured", identity.GetProperty("expectedSourceCommitStatus").GetString());
        }
        else
        {
            Assert.True(identity.GetProperty("expectedSourceCommitValid").GetBoolean());
            Assert.Equal(expectedSourceCommit, identity.GetProperty("expectedSourceCommit").GetString());
            Assert.Equal(
                expectedSourceCommitMatches,
                identity.GetProperty("expectedSourceCommitMatches").GetBoolean());
            Assert.Equal("configured_valid", identity.GetProperty("expectedSourceCommitStatus").GetString());
        }

        var entryCommit = entry.GetProperty("sourceCommit").GetString();
        Assert.False(string.IsNullOrWhiteSpace(entryCommit));
        Assert.Equal(entryCommit, modelServer.GetProperty("sourceCommit").GetString());
        Assert.Equal(entryCommit, domain.GetProperty("sourceCommit").GetString());

        var gitHead = TryReadGitHead(repoRoot);
        if (!string.IsNullOrWhiteSpace(gitHead))
            Assert.True(CommitsMatch(entryCommit!, gitHead), $"Expected assembly commit {entryCommit} to match git HEAD {gitHead}.");

        var sourceIdentityJson = identity.GetRawText();
        Assert.DoesNotContain(repoRoot, sourceIdentityJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(":\\\\", sourceIdentityJson, StringComparison.Ordinal);
        Assert.DoesNotContain(".dll", sourceIdentityJson, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertActualSourceCommitPublicSurface(
        JsonElement root,
        string repoRoot,
        string tempRoot)
    {
        var identity = root.GetProperty("sourceIdentity");
        var entryCommit = identity.GetProperty("entryAssembly").GetProperty("sourceCommit").GetString();
        var gitHead = TryReadGitHead(repoRoot);
        Assert.False(
            string.IsNullOrWhiteSpace(gitHead),
            "Focused source-identity test requires readable git HEAD to prove the actual commit without expected fallback.");
        Assert.True(
            CommitsMatch(entryCommit!, gitHead!),
            $"Expected public sourceCommit {entryCommit} to match git HEAD {gitHead} without --expected-source-commit.");
        AssertPublicHealthReadyStringsDoNotLeakLocalPaths(root, repoRoot, tempRoot);

        var sourceIdentityJson = identity.GetRawText();
        Assert.DoesNotContain("fatal:", sourceIdentityJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("rev-parse", sourceIdentityJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not a git repository", sourceIdentityJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stderr", sourceIdentityJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("file://", sourceIdentityJson, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertAssemblySourceIdentity(JsonElement assembly, string expectedName)
    {
        Assert.Equal(expectedName, assembly.GetProperty("name").GetString());
        Assert.False(string.IsNullOrWhiteSpace(assembly.GetProperty("version").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(assembly.GetProperty("informationalVersion").GetString()));
        AssertCommitLike(assembly.GetProperty("sourceCommit").GetString());
        Assert.True(assembly.GetProperty("sha256Available").GetBoolean());
        AssertSha256(assembly.GetProperty("sha256").GetString());
    }

    private static void AssertCommitLike(string? value)
    {
        Assert.False(string.IsNullOrWhiteSpace(value));
        Assert.InRange(value!.Length, 7, 64);
        Assert.All(value, character => Assert.True(Uri.IsHexDigit(character), $"Commit contains non-hex character: {value}"));
    }

    private static void AssertSha256(string? value)
    {
        Assert.False(string.IsNullOrWhiteSpace(value));
        Assert.Equal(64, value!.Length);
        Assert.All(value, character => Assert.True(Uri.IsHexDigit(character), $"SHA-256 contains non-hex character: {value}"));
    }

    private static void AssertSha256Ref(string? value)
    {
        Assert.False(string.IsNullOrWhiteSpace(value));
        Assert.StartsWith("sha256:", value, StringComparison.OrdinalIgnoreCase);
        AssertSha256(value!["sha256:".Length..]);
    }

    private static void AssertPublicRenderArtifactRootRef(JsonElement root)
    {
        Assert.True(root.GetProperty("renderArtifactRootConfigured").GetBoolean());
        Assert.Equal(
            "artifact://pass143/live-local/render-artifacts/store",
            root.GetProperty("renderArtifactRoot").GetString());
        Assert.Equal(
            "artifact://pass143/live-local/render-artifacts/store",
            root.GetProperty("renderArtifactRootRef").GetString());
    }

    private static IReadOnlyList<string> FullLocalRouteSetWithArtifactUpload()
    {
        var routePaths = DesignConversionModelServerAspNetHostStartup.FullLocalRouteSet().ToList();
        if (!routePaths.Contains(DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, StringComparer.Ordinal))
            routePaths.Add(DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath);

        return routePaths;
    }

    private static void AssertNonTrainingFlow38Ready(JsonElement root, string bearerToken)
    {
        Assert.False(root.GetProperty("machineBackedLocal").GetBoolean());
        Assert.False(root.GetProperty("fullLocalRouteSet").GetBoolean());
        Assert.Equal(0, root.GetProperty("machineTrainingRows").GetInt32());
        Assert.Equal(0, root.GetProperty("learnedWeightArtifactCount").GetInt32());
        Assert.False(root.GetProperty("machineBackedApiRuntimeProbeAttempted").GetBoolean());
        Assert.False(root.GetProperty("machineBackedApiRuntimeProbeExecuted").GetBoolean());
        Assert.False(root.GetProperty("trainedModelAuthority").GetBoolean());
        Assert.False(root.GetProperty("productReady").GetBoolean());
        Assert.Equal(
            DesignConversionModelServerAspNetHostStartup.NonTrainingLocalRouteSet(),
            ReadStringArray(root, "routePaths"));
        Assert.Equal(2, DesignConversionModelServerAspNetHostStartup.NonTrainingLocalRouteSet().Count);
        AssertArtifactUploadPublicFields(root, expectedMapped: false);
        AssertFlow38PublicFields(root, expectedMapped: true, expectedConfigured: true);
        Assert.DoesNotContain(bearerToken, root.GetRawText(), StringComparison.Ordinal);
    }

    private static async Task AssertNonTrainingPhysicalRouteSetAsync(HttpClient client)
    {
        var forbiddenRoutes = new (HttpMethod Method, string Path)[]
        {
            (HttpMethod.Post, DesignConversionModelServerEndpointRegistry.EndpointPath),
            (HttpMethod.Get, DesignConversionModelServerStatusHandler.EndpointPathPrefix + "non-training-live-forbidden-job"),
            (HttpMethod.Get, DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath),
            (HttpMethod.Post, DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath),
            (HttpMethod.Post, DesignConversionModelServerEndpointRegistry.ModelActivationEndpointPath),
            (HttpMethod.Post, DesignConversionModelServerEndpointRegistry.HumanApprovalChallengeEndpointPath),
            (HttpMethod.Post, DesignConversionModelServerEndpointRegistry.HumanApprovalApproveEndpointPath)
        };

        foreach (var forbidden in forbiddenRoutes)
        {
            using var request = new HttpRequestMessage(forbidden.Method, forbidden.Path);
            if (forbidden.Method == HttpMethod.Post)
                request.Content = new StringContent("{}", Encoding.UTF8, "application/json");
            using var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(body);
            Assert.False(response.Headers.Contains("x-design-conversion-endpoint"));
            Assert.DoesNotContain("ENDPOINT_ROUTE_NOT_FOUND", body, StringComparison.Ordinal);
        }

        using (var improvementRequest = new HttpRequestMessage(
                   HttpMethod.Post,
                   DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath)
               {
                   Content = new StringContent("{}", Encoding.UTF8, "application/json")
               })
        using (var improvementResponse = await client.SendAsync(improvementRequest))
        {
            Assert.NotEqual(HttpStatusCode.NotFound, improvementResponse.StatusCode);
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                improvementResponse.Headers.GetValues("x-design-conversion-endpoint").Single());
        }

        using (var flow38Request = new HttpRequestMessage(
                   HttpMethod.Post,
                   DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointPath)
               {
                   Content = new StringContent("{}", Encoding.UTF8, "application/json")
               })
        using (var flow38Response = await client.SendAsync(flow38Request))
        {
            Assert.Equal(HttpStatusCode.ServiceUnavailable, flow38Response.StatusCode);
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointPath,
                flow38Response.Headers.GetValues("x-design-conversion-endpoint").Single());
        }
    }

    private static HttpClient CreateLoopbackClient(JsonElement readyRoot) => new(
        new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false,
            UseProxy = false,
            ConnectTimeout = TimeSpan.FromSeconds(5)
        },
        disposeHandler: true)
    {
        BaseAddress = new Uri(readyRoot.GetProperty("baseUrl").GetString()!),
        Timeout = TimeSpan.FromSeconds(10)
    };

    private static async Task<HttpResponseMessage> PostFlow38QuarantineAsync(
        HttpClient client,
        DesignConversionFlow38QuarantinedSourceRow row,
        string bearerToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            DesignConversionModelServerFlow38QuarantineHandler.EndpointPath)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(row, DesignConversionModelServerHttpHandler.JsonOptions),
                Encoding.UTF8,
                "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        request.Headers.TryAddWithoutValidation("X-Tenant-Id", row.TenantScope);
        request.Headers.TryAddWithoutValidation("X-Correlation-Id", row.CorrelationId);
        request.Headers.TryAddWithoutValidation("X-Idempotency-Key", row.EventId);
        return await client.SendAsync(request);
    }

    private static LiveLocalFlow38Snapshot AssertLiveLocalFlow38Response(
        HttpResponseMessage response,
        string bodyJson,
        HttpStatusCode expectedStatus,
        bool created)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("utf-8", response.Content.Headers.ContentType?.CharSet);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Equal(
            DesignConversionModelServerFlow38QuarantineHandler.EndpointPath,
            Assert.Single(response.Headers.GetValues("x-design-conversion-endpoint")));
        Assert.Equal(bool.TrueString, Assert.Single(response.Headers.GetValues("x-flow38-quarantine-only")));
        Assert.Equal(bool.FalseString, Assert.Single(response.Headers.GetValues("x-flow38-dpo-train-ready")));
        Assert.Equal(bool.FalseString, Assert.Single(response.Headers.GetValues("x-flow38-training-triggered")));

        using var document = JsonDocument.Parse(bodyJson);
        var root = document.RootElement;
        Assert.Equal(
            DesignConversionModelServerFlow38QuarantineHandler.SchemaVersion,
            root.GetProperty("schemaVersion").GetString());
        Assert.Equal(
            DesignConversionModelServerFlow38QuarantineHandler.EndpointPath,
            root.GetProperty("endpoint").GetString());
        Assert.Equal(HttpMethod.Post.Method, root.GetProperty("method").GetString());
        Assert.Equal((int)expectedStatus, root.GetProperty("statusCode").GetInt32());
        Assert.Equal("QUARANTINE", root.GetProperty("legalVerdict").GetString());
        Assert.Equal("PersonalAdaptive", root.GetProperty("allowedUse").GetString());
        Assert.Equal("adaptive_or_quarantine", root.GetProperty("route").GetString());
        Assert.False(root.GetProperty("semanticQualityClaimed").GetBoolean());
        Assert.False(root.GetProperty("dpoTrainReady").GetBoolean());
        Assert.False(root.GetProperty("dpoPairCreated").GetBoolean());
        Assert.False(root.GetProperty("trainingTriggered").GetBoolean());
        Assert.True(root.GetProperty("quarantinePersisted").GetBoolean());
        Assert.Equal(created, root.GetProperty("created").GetBoolean());
        Assert.Equal(!created, root.GetProperty("replayed").GetBoolean());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.True(metadata.EnumerateObject().Count() >= 12);
        Assert.Equal(bool.TrueString, metadata.GetProperty("flow38_quarantine_only").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("flow38_quarantine_store_configured").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("flow38_quarantine_persisted").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("semantic_quality_claimed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("dpo_train_ready").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("dpo_pair_created").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("training_triggered").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("common_training_allowed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_output_accepted").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("refs_only_public_response").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_route_verified").GetString());
        Assert.DoesNotContain("chosenOutput", bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("rejectedOutput", bodyJson, StringComparison.Ordinal);

        var eventId = root.GetProperty("eventId").GetString()!;
        var recordRef = root.GetProperty("recordRef").GetString()!;
        var fingerprint = root.GetProperty("fingerprint").GetString()!;
        Assert.Equal(eventId, Assert.Single(response.Headers.GetValues("x-flow38-event-id")));
        Assert.StartsWith("flow38-feedback:", eventId, StringComparison.Ordinal);
        Assert.StartsWith("flow38-quarantine://sha256:", recordRef, StringComparison.Ordinal);
        Assert.EndsWith('/' + eventId, recordRef, StringComparison.Ordinal);
        AssertSha256Ref(fingerprint);
        return new LiveLocalFlow38Snapshot(eventId, recordRef, fingerprint);
    }

    private static void AssertLiveLocalFlow38Store(
        byte[] storeBytes,
        DesignConversionFlow38QuarantinedSourceRow expectedRow,
        LiveLocalFlow38Snapshot expectedIdentity)
    {
        Assert.NotEmpty(storeBytes);
        Assert.Equal((byte)'\n', storeBytes[^1]);
        Assert.DoesNotContain((byte)'\r', storeBytes);
        var storeText = new UTF8Encoding(false, true).GetString(storeBytes);
        var line = Assert.Single(storeText.Split('\n', StringSplitOptions.RemoveEmptyEntries));
        Assert.DoesNotContain("\"chosenOutput\":", line, StringComparison.Ordinal);
        Assert.DoesNotContain("\"rejectedOutput\":", line, StringComparison.Ordinal);
        Assert.DoesNotContain("rawOutput", line, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("dpoPairCreated", line, StringComparison.Ordinal);
        Assert.DoesNotContain("trainingTriggered", line, StringComparison.Ordinal);
        Assert.DoesNotContain("productReady", line, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(line);
        var root = document.RootElement;
        Assert.Equal(12, root.EnumerateObject().Count());
        Assert.Equal(JsonlDesignConversionFlow38QuarantineStore.SchemaVersion, root.GetProperty("schemaVersion").GetString());
        Assert.Equal(expectedIdentity.EventId, root.GetProperty("eventId").GetString());
        Assert.Equal(expectedIdentity.RecordRef, root.GetProperty("recordRef").GetString());
        Assert.Equal(expectedIdentity.Fingerprint, root.GetProperty("fingerprint").GetString());
        Assert.Equal("QUARANTINE", root.GetProperty("legalVerdict").GetString());
        Assert.Equal("PersonalAdaptive", root.GetProperty("allowedUse").GetString());
        Assert.Equal("adaptive_or_quarantine", root.GetProperty("route").GetString());
        Assert.False(root.GetProperty("semanticQualityClaimed").GetBoolean());
        Assert.False(root.GetProperty("dpoTrainReady").GetBoolean());

        var source = root.GetProperty("source");
        Assert.Equal(23, source.EnumerateObject().Count());
        Assert.Equal(expectedRow.EventId, source.GetProperty("eventId").GetString());
        Assert.Equal(expectedRow.CorrelationId, source.GetProperty("correlationId").GetString());
        Assert.Equal(expectedRow.TenantScope, source.GetProperty("tenantScope").GetString());
        Assert.Equal(expectedRow.ChosenOutputRef, source.GetProperty("chosenOutputRef").GetString());
        Assert.Equal(expectedRow.RejectedOutputRef, source.GetProperty("rejectedOutputRef").GetString());
        Assert.Equal(expectedRow.ProvenanceRefs, ReadStringArray(source, "provenanceRefs"));
        Assert.False(source.GetProperty("semanticQualityClaimed").GetBoolean());
        Assert.False(source.GetProperty("dpoTrainReady").GetBoolean());
    }

    private sealed record LiveLocalFlow38Snapshot(
        string EventId,
        string RecordRef,
        string Fingerprint);

    private static void AssertFlow38PublicFields(
        JsonElement root,
        bool expectedMapped,
        bool expectedConfigured)
    {
        Assert.Equal(expectedMapped, root.GetProperty("flow38QuarantineRouteMapped").GetBoolean());
        Assert.Equal(expectedConfigured, root.GetProperty("flow38QuarantineConfigured").GetBoolean());
        Assert.Equal(expectedConfigured, root.GetProperty("flow38QuarantineStoreConfigured").GetBoolean());
        Assert.Equal(expectedConfigured, root.GetProperty("flow38QuarantineStoreConfiguredObserved").GetBoolean());
        Assert.True(root.GetProperty("flow38BoundaryMetadataValid").GetBoolean());
        Assert.Equal(
            DesignConversionFlow38PublicBoundarySnapshot.ValidStatus,
            root.GetProperty("flow38BoundaryMetadataStatus").GetString());
        Assert.Empty(root.GetProperty("flow38BoundaryMetadataIssues").EnumerateArray());
        Assert.Equal(
            DesignConversionModelServerFlow38QuarantineHandler.SchemaVersion,
            root.GetProperty("flow38ContractSchemaVersion").GetString());
        if (expectedMapped)
        {
            Assert.Equal(
                DesignConversionModelServerFlow38QuarantineHandler.EndpointPath,
                root.GetProperty("flow38QuarantineEndpointPath").GetString());
        }
        else
        {
            Assert.Equal(JsonValueKind.Null, root.GetProperty("flow38QuarantineEndpointPath").ValueKind);
        }

        Assert.Equal(expectedMapped, root.GetProperty("flow38QuarantineOnly").GetBoolean());
        Assert.False(root.GetProperty("flow38Persisted").GetBoolean());
        Assert.Equal(
            DesignConversionModelServerFlow38QuarantineHandler.QuarantineVerdict,
            root.GetProperty("flow38LegalVerdict").GetString());
        Assert.Equal(
            DesignConversionModelServerFlow38QuarantineHandler.PersonalAdaptiveUse,
            root.GetProperty("flow38AllowedUse").GetString());
        Assert.Equal(
            DesignConversionModelServerFlow38QuarantineHandler.AdaptiveOrQuarantineRoute,
            root.GetProperty("flow38Route").GetString());
        Assert.False(root.GetProperty("flow38SemanticQualityClaimed").GetBoolean());
        Assert.False(root.GetProperty("flow38DpoTrainReady").GetBoolean());
        Assert.False(root.GetProperty("flow38DpoPairCreated").GetBoolean());
        Assert.False(root.GetProperty("flow38TrainingTriggered").GetBoolean());
        Assert.False(root.GetProperty("flow38CommonTrainingAllowed").GetBoolean());
        Assert.False(root.GetProperty("flow38RawOutputAccepted").GetBoolean());
        Assert.True(root.GetProperty("flow38RefsOnlyPublicResponse").GetBoolean());
        Assert.False(root.GetProperty("flow38ProductReady").GetBoolean());
    }

    private static IReadOnlyList<string> FullLocalRouteSetWithoutArtifactUpload()
        => DesignConversionModelServerAspNetHostStartup.FullLocalRouteSet();

    private static IReadOnlyList<string> NonDurableLocalRouteSetWithArtifactUpload()
    {
        var routePaths = DesignConversionModelServerAspNetHostStartup.NonDurableLocalRouteSet().ToList();
        if (!routePaths.Contains(DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, StringComparer.Ordinal))
            routePaths.Add(DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath);

        return routePaths;
    }

    private static void AssertNoConfigNonDurableReadyRoot(JsonElement root)
    {
        var routePaths = ReadStringArray(root, "routePaths");
        var expectedRoutePaths = NonDurableLocalRouteSetWithArtifactUpload();

        Assert.False(root.GetProperty("localArtifactDbConfigured").GetBoolean());
        Assert.False(root.GetProperty("fullLocalRouteSet").GetBoolean());
        Assert.Equal(
            expectedRoutePaths.Order(StringComparer.Ordinal),
            routePaths.Order(StringComparer.Ordinal));
        Assert.Contains(DesignConversionModelServerEndpointRegistry.EndpointPath, routePaths);
        Assert.Contains(DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointPath, routePaths);
        Assert.DoesNotContain(DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath, routePaths);
        Assert.DoesNotContain(DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath, routePaths);
        AssertArtifactUploadPublicFields(root, expectedMapped: true);
        Assert.False(root.GetProperty("productReady").GetBoolean());
    }

    private static void AssertNoConfigNonDurableEnvelope(
        DesignConversionModelServerLocalCallerAdapterResponse envelope)
    {
        Assert.True(
            envelope.StatusCode == 200,
            $"Expected no-config submit to complete; status={envelope.StatusCode}; " +
            $"error={envelope.ParsedErrorCode}; body={envelope.ResponseBodyJson}");
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.True(envelope.RuntimeInvoked);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);

        var metadata = envelope.BoundaryMetadata;
        Assert.Equal(bool.FalseString, metadata["persistence_verified"]);
        Assert.Equal("local_artifact_db_not_configured", metadata["persistence_failure_reason"]);
        Assert.Equal(bool.FalseString, metadata["persistence_server_append_reported"]);
        Assert.Equal(bool.FalseString, metadata["persistence_fresh_readback_success"]);
        Assert.Equal("0", metadata["persistence_fresh_readback_record_count"]);
        Assert.Equal("0", metadata["persistence_server_owned_record_count"]);
        Assert.Equal(bool.FalseString, metadata["product_ready"]);
    }

    private static void AssertConfiguredArtifactDbAutowriteEnvelope(
        DesignConversionModelServerLocalCallerAdapterResponse envelope,
        string expectedRequestId)
    {
        if (envelope.StatusCode != 200)
        {
            Assert.True(
                envelope.StatusCode == 200,
                BuildConfiguredArtifactDbAutowriteEnvelopeDiagnostic(envelope));
        }

        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.True(envelope.RuntimeInvoked);
        Assert.True(
            string.Equals(
                DesignConversionThinClientStatus.Completed,
                envelope.ParsedContractStatus,
                StringComparison.Ordinal),
            BuildConfiguredArtifactDbAutowriteEnvelopeDiagnostic(envelope));
        AssertCompletedQualityThreshold(envelope);
        Assert.Equal(expectedRequestId, envelope.ParsedRequestId);

        var metadata = envelope.BoundaryMetadata;
        Assert.Equal(bool.TrueString, metadata["persistence_verified"]);
        Assert.Equal("none", metadata["persistence_failure_reason"]);
        Assert.Equal(bool.TrueString, metadata["persistence_server_append_reported"]);
        Assert.Equal(bool.TrueString, metadata["persistence_fresh_readback_success"]);
        Assert.Equal("1", metadata["persistence_fresh_readback_record_count"]);
        Assert.Equal("1", metadata["persistence_server_owned_record_count"]);
        Assert.Equal(bool.TrueString, metadata["persistence_record_job_bound"]);
        Assert.Equal(bool.TrueString, metadata["persistence_record_request_bound"]);
        Assert.Equal(bool.TrueString, metadata["persistence_record_refs_only"]);
        Assert.Equal(bool.TrueString, metadata["persistence_raw_content_free"]);
        Assert.Equal(bool.FalseString, metadata["product_ready"]);
    }

    private static void AssertCompletedQualityThreshold(
        DesignConversionModelServerLocalCallerAdapterResponse envelope)
    {
        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var qualityMetrics = document.RootElement
            .GetProperty("runtimeStatus")
            .GetProperty("qualityMetrics");
        var score = qualityMetrics.GetProperty("score").GetDouble();
        var requiredThreshold = qualityMetrics.GetProperty("requiredThreshold").GetDouble();

        Assert.Equal(ServerOwnedLocalPngComparisonProvider.PassThreshold, requiredThreshold);
        Assert.True(
            score >= requiredThreshold,
            $"Expected completed browser-backed quality score {score:R} to meet unchanged threshold {requiredThreshold:R}.");
        Assert.True(qualityMetrics.GetProperty("passed").GetBoolean());
        Assert.True(qualityMetrics.GetProperty("thresholdMet").GetBoolean());
    }

    private static string BuildConfiguredArtifactDbAutowriteEnvelopeDiagnostic(
        DesignConversionModelServerLocalCallerAdapterResponse envelope)
    {
        const int maxSummaryLength = 2048;
        var summary = new StringBuilder("Configured artifact DB autowrite status assertion failed");

        AppendField("status", envelope.StatusCode.ToString(CultureInfo.InvariantCulture));
        AppendField("envelope_kind", envelope.EnvelopeKind);
        AppendField("runtime_invoked", envelope.RuntimeInvoked ? "true" : "false");
        AppendField(
            "parsed_response_json_succeeded",
            envelope.ParsedResponseJsonSucceeded ? "true" : "false");
        AppendField("parsed_error_code", envelope.ParsedErrorCode);
        summary.Append("; parsed_browser_failure_detail=")
            .Append(SelectKnownSanitizedBrowserProcessFailureDetail(
                envelope.ParsedErrorCode,
                envelope.ParsedErrorMessage));
        AppendField("parsed_contract_status", envelope.ParsedContractStatus);
        AppendMetadataField("http_status_code");
        AppendMetadataField("runtime_invoked");
        AppendMetadataField("route_admission_error_ref");
        AppendMetadataField("handler_failure_kind");
        AppendMetadataField("multi_viewport_complete");
        AppendMetadataField("browser_render_executed");
        AppendMetadataField("final_visual_decision");
        AppendMetadataField("tier_policy_stop_reason");
        AppendMetadataField("thin_client_terminal_status");
        AppendResponseDiagnostics();

        return summary.Length <= maxSummaryLength
            ? summary.ToString()
            : summary.ToString(0, maxSummaryLength);

        void AppendMetadataField(string key)
        {
            envelope.BoundaryMetadata.TryGetValue(key, out var value);
            AppendField($"metadata.{key}", value);
        }

        void AppendResponseDiagnostics()
        {
            try
            {
                using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
                if (!document.RootElement.TryGetProperty("runtimeStatus", out var runtimeStatus))
                    return;

                if (runtimeStatus.TryGetProperty("message", out var message))
                    AppendField("runtime_status.message", message.GetString());
                if (runtimeStatus.TryGetProperty("qualityMetrics", out var qualityMetrics))
                {
                    if (qualityMetrics.TryGetProperty("score", out var score))
                        AppendField("runtime_status.quality_score", score.GetRawText());
                    if (qualityMetrics.TryGetProperty("requiredThreshold", out var threshold))
                        AppendField("runtime_status.required_threshold", threshold.GetRawText());
                    if (qualityMetrics.TryGetProperty("thresholdMet", out var thresholdMet))
                        AppendField("runtime_status.threshold_met", thresholdMet.GetRawText());
                }
                if (runtimeStatus.TryGetProperty("warnings", out var warnings) &&
                    warnings.ValueKind == JsonValueKind.Array)
                {
                    AppendField(
                        "runtime_status.decision_warnings",
                        string.Join(
                            ',',
                            warnings.EnumerateArray()
                                .Select(item => item.GetString())
                                .Where(item =>
                                    item is not null &&
                                    (item.StartsWith("final_visual_decision=", StringComparison.Ordinal) ||
                                     item.StartsWith("tier_policy_stop_reason=", StringComparison.Ordinal) ||
                                     item.StartsWith("thin_client_terminal_status=", StringComparison.Ordinal) ||
                                     item.Contains("submit_path", StringComparison.Ordinal) ||
                                     item.Contains("improvement_loop", StringComparison.Ordinal) ||
                                     item.Contains("max_attempt", StringComparison.Ordinal)))));
                }
            }
            catch (JsonException)
            {
                AppendField("runtime_status.response_parse", "invalid_json");
            }
        }

        void AppendField(string name, string? value)
            => summary.Append("; ")
                .Append(name)
                .Append('=')
                .Append(SanitizePublicDiagnosticValue(value));
    }

    private static string SelectKnownSanitizedBrowserProcessFailureDetail(
        string? parsedErrorCode,
        string? parsedErrorMessage)
    {
        const int maxDetailLength = 384;
        const string exitCodePrefix = "Browser screenshot process failed with exit code ";
        const string exceptionTypePrefix =
            "Browser screenshot process failed with sanitized exception type ";
        const string exitCodePattern =
            @"\ABrowser screenshot process failed with exit code (?<exit_code>-?[0-9]+)\. stderr_sha256=[0-9a-f]{64} stderr_chars=(?<stderr_chars>[0-9]+) stdout_sha256=[0-9a-f]{64} stdout_chars=(?<stdout_chars>[0-9]+)\z";
        const string exceptionTypePattern =
            @"\ABrowser screenshot process failed with sanitized exception type [A-Za-z_][A-Za-z0-9_`]*\.\z";

        if (string.IsNullOrWhiteSpace(parsedErrorMessage))
            return "<missing>";
        if (!string.Equals(
                parsedErrorCode,
                LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode,
                StringComparison.Ordinal))
        {
            return "<redacted>";
        }

        if (parsedErrorMessage.Length > maxDetailLength ||
            parsedErrorMessage.Any(char.IsControl) ||
            parsedErrorMessage.Contains("://", StringComparison.Ordinal) ||
            parsedErrorMessage.Contains('\\') ||
            parsedErrorMessage.Contains('/'))
        {
            return "<redacted>";
        }

        var exitCodeMatch = Regex.Match(
            parsedErrorMessage,
            exitCodePattern,
            RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);
        var matchesExitCodeDetail =
            parsedErrorMessage.StartsWith(exitCodePrefix, StringComparison.Ordinal) &&
            exitCodeMatch.Success &&
            IsCanonicalInvariantInt32(
                exitCodeMatch.Groups["exit_code"].Value,
                requireNonNegative: false) &&
            IsCanonicalInvariantInt32(
                exitCodeMatch.Groups["stderr_chars"].Value,
                requireNonNegative: true) &&
            IsCanonicalInvariantInt32(
                exitCodeMatch.Groups["stdout_chars"].Value,
                requireNonNegative: true);
        var matchesExceptionTypeDetail =
            parsedErrorMessage.StartsWith(exceptionTypePrefix, StringComparison.Ordinal) &&
            Regex.IsMatch(
                parsedErrorMessage,
                exceptionTypePattern,
                RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

        return matchesExitCodeDetail || matchesExceptionTypeDetail
            ? parsedErrorMessage
            : "<redacted>";
    }

    private static bool IsCanonicalInvariantInt32(string value, bool requireNonNegative)
    {
        var styles = requireNonNegative
            ? NumberStyles.None
            : NumberStyles.AllowLeadingSign;
        return int.TryParse(value, styles, CultureInfo.InvariantCulture, out var parsed) &&
               (!requireNonNegative || parsed >= 0) &&
               string.Equals(
                   value,
                   parsed.ToString(CultureInfo.InvariantCulture),
                   StringComparison.Ordinal);
    }

    private static string SanitizePublicDiagnosticValue(string? value)
    {
        const int maxValueLength = 96;
        if (string.IsNullOrWhiteSpace(value))
            return "<missing>";

        var trimmed = value.Trim();
        if (trimmed.Contains("://", StringComparison.Ordinal) ||
            trimmed.Contains('\\') ||
            trimmed.Contains('/'))
        {
            return "<redacted>";
        }

        var sanitized = new StringBuilder(Math.Min(trimmed.Length, maxValueLength));
        foreach (var character in trimmed)
        {
            if (sanitized.Length == maxValueLength)
                break;

            sanitized.Append(char.IsControl(character) || character is ';' or '=' ? '_' : character);
        }

        if (trimmed.Length > maxValueLength)
            sanitized.Append("...");

        return sanitized.ToString();
    }

    private static void AssertNoConfiguredArtifactDbPublicLeakage(
        IReadOnlyList<string> publicJsonTexts,
        params string[] forbidden)
    {
        var forbiddenRepresentations = forbidden
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .SelectMany(value => new[] { value, JsonSerializer.Serialize(value).Trim('"') })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var publicJson in publicJsonTexts)
        {
            AssertNoRouteAdmissionSecretLeakage(publicJson, forbiddenRepresentations);
            Assert.DoesNotContain("file:", publicJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("private_marker", publicJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("caller_private_marker", publicJson, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static async Task AssertNoConfigImprovementLoopRuntimeEnvelopeAsync(
        HttpClient client,
        DesignConversionModelServerLocalCallerPackage package,
        string privateMarker,
        string loopId,
        string idempotencyKey)
    {
        var loopRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: loopId,
            InitialPackage: package,
            MaxAttempts: 1,
            Metadata: new Dictionary<string, string>
            {
                ["n8_14_no_config_runtime_envelope"] = bool.TrueString,
                ["caller_private_marker"] = privateMarker
            });
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(loopRequest, DesignConversionModelServerHttpHandler.JsonOptions),
                Encoding.UTF8,
                "application/json")
        };
        AddRouteAdmissionHeaders(request, RouteAdmissionCurrentToken, idempotencyKey);

        using var response = await client.SendAsync(request);
        var bodyJson = await response.Content.ReadAsStringAsync();
        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected no-config improvement-loop runtime envelope; status={(int)response.StatusCode}; body={bodyJson}");
        AssertNoRouteAdmissionSecretLeakage(bodyJson);
        Assert.DoesNotContain(privateMarker, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.Html!, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.Css!, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.DesignTree!.Payload!, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.OriginalTree!.Payload!, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("file://", bodyJson, StringComparison.OrdinalIgnoreCase);

        using var document = JsonDocument.Parse(bodyJson);
        var root = document.RootElement;
        Assert.Equal("bounded_non_ready", root.GetProperty("contractStatus").GetString());
        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("persistence_invoked").GetString());
        Assert.Equal(
            bool.FalseString,
            metadata.GetProperty(DesignConversionImprovementLoopCoordinator.LocalArtifactDbAutowriteMarker).GetString());
        Assert.Equal("not_configured_non_durable", metadata.GetProperty("local_artifact_db_persistence_status").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
    }

    private static HashSet<string> SnapshotLiveLocalRandomArtifactDbRoots(string isolatedProcessTempRoot)
    {
        var root = Path.Combine(
            Path.GetFullPath(isolatedProcessTempRoot),
            "design-conversion-live-local-server");
        return Directory.Exists(root)
            ? Directory
                .EnumerateDirectories(root, "artifact-db-*", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFullPath)
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    private static HashSet<string> SnapshotLiveLocalRandomArtifactDbFiles(string isolatedProcessTempRoot)
        => SnapshotLiveLocalRandomArtifactDbRoots(isolatedProcessTempRoot)
            .Where(Directory.Exists)
            .SelectMany(path => Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            .Select(Path.GetFullPath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static void AssertNoLiveLocalRandomArtifactDbRoots(string isolatedProcessTempRoot)
        => Assert.Empty(SnapshotLiveLocalRandomArtifactDbRoots(isolatedProcessTempRoot));

    private static void AssertNoLiveLocalRandomArtifactDbFiles(string isolatedProcessTempRoot)
        => Assert.Empty(SnapshotLiveLocalRandomArtifactDbFiles(isolatedProcessTempRoot));

    private static void AssertArtifactUploadPublicFields(
        JsonElement root,
        bool expectedMapped,
        string? expectedStartupStatus = null,
        string? expectedNonReadyCode = null,
        string? expectedNonReadyReason = null)
    {
        expectedStartupStatus ??= expectedMapped ? "mapped" : "not_configured";
        expectedNonReadyCode ??= expectedMapped ? "none" : "ARTIFACT_UPLOAD_NOT_CONFIGURED";
        expectedNonReadyReason ??= expectedMapped
            ? "none"
            : "DesignConversion artifact upload is not configured for this live-local mode.";

        Assert.Equal(expectedMapped, root.GetProperty("artifactUploadRouteMapped").GetBoolean());
        Assert.Equal(expectedMapped, root.GetProperty("artifactUploadConfigured").GetBoolean());
        Assert.Equal(expectedMapped, root.GetProperty("artifactUploadFailClosedAdmissionRequired").GetBoolean());
        Assert.Equal(expectedMapped, root.GetProperty("failClosedAdmissionRequired").GetBoolean());
        Assert.Equal(expectedStartupStatus, root.GetProperty("artifactUploadStartupStatus").GetString());
        Assert.Equal(expectedNonReadyCode, root.GetProperty("artifactUploadNonReadyCode").GetString());
        Assert.Equal(expectedNonReadyReason, root.GetProperty("artifactUploadNonReadyReason").GetString());

        if (expectedMapped)
        {
            Assert.Equal(
                DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
                root.GetProperty("artifactUploadEndpointPath").GetString());
            Assert.Equal(
                DesignConversionModelServerArtifactUploadStore.DefaultMaxUploadBytes,
                root.GetProperty("artifactUploadMaxBytes").GetInt64());
        }
        else
        {
            Assert.Equal(JsonValueKind.Null, root.GetProperty("artifactUploadEndpointPath").ValueKind);
            Assert.Equal(JsonValueKind.Null, root.GetProperty("artifactUploadMaxBytes").ValueKind);
        }
    }

    private static void AssertPublicBrowserExecutableMetadata(
        JsonElement root,
        bool configured,
        bool runtimeAvailable)
    {
        Assert.Equal(JsonValueKind.Null, root.GetProperty("browserExecutablePath").ValueKind);
        var browser = root.GetProperty("browserExecutable");
        Assert.Equal(configured, browser.GetProperty("configured").GetBoolean());
        Assert.Equal(runtimeAvailable, browser.GetProperty("runtimeAvailable").GetBoolean());
        Assert.Equal(runtimeAvailable, browser.GetProperty("trusted").GetBoolean());
        Assert.Equal(
            configured ? "chromium-family-browser" : string.Empty,
            browser.GetProperty("capability").GetString());

        if (runtimeAvailable)
        {
            Assert.False(string.IsNullOrWhiteSpace(browser.GetProperty("name").GetString()));
            Assert.True(browser.GetProperty("sha256Available").GetBoolean());
            AssertSha256Ref(browser.GetProperty("sha256").GetString());
        }
        else
        {
            Assert.Equal(string.Empty, browser.GetProperty("name").GetString());
            Assert.Equal(string.Empty, browser.GetProperty("version").GetString());
            Assert.Equal(string.Empty, browser.GetProperty("sha256").GetString());
            Assert.False(browser.GetProperty("sha256Available").GetBoolean());
        }
    }

    private static void AssertPublicHealthReadyStringsDoNotLeakLocalPaths(
        JsonElement root,
        string repoRoot,
        string tempRoot)
    {
        var sensitiveFragments = new[]
            {
                repoRoot,
                tempRoot,
                Path.GetTempPath(),
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "/tmp/design-conversion-private",
                "/root/design-conversion-private",
                "/home/user/design-conversion-private",
                @"\/tmp\/design-conversion-private",
                @"\/root\/design-conversion-private",
                @"\/home\/user\/design-conversion-private",
                @"C:\Users\design-user\design-conversion-private",
                @"C:\\Users\\design-user\\design-conversion-private",
                @"\\server\share\design-conversion-private",
                "file:///tmp/design-conversion-private",
                Environment.UserName
            }
            .Where(fragment => !string.IsNullOrWhiteSpace(fragment))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var (path, value) in EnumerateJsonStrings(root, "$"))
        {
            Assert.False(
                LooksLikePublicLocalPathLeak(value),
                $"Public health/ready string leaked a local path at {path}: {value}");

            foreach (var fragment in sensitiveFragments)
            {
                Assert.DoesNotContain(fragment, value, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    private static void AssertBrowserRendererPngProofPublicFields(
        JsonElement root,
        bool expectedRequired,
        bool expectedAttempted,
        bool expectedValid,
        string expectedStartupStatus,
        string expectedNonReadyCode,
        int expectedEvidenceCount,
        int expectedRequiredArtifactCount,
        int expectedRequiredViewportCount,
        int expectedObservedViewportCount)
    {
        Assert.Equal(expectedRequired, root.GetProperty("browserRendererPngProofRequired").GetBoolean());
        Assert.Equal(expectedAttempted, root.GetProperty("browserRendererPngProofAttempted").GetBoolean());
        Assert.Equal(expectedValid, root.GetProperty("browserRendererPngProofValid").GetBoolean());
        Assert.Equal(expectedStartupStatus, root.GetProperty("browserRendererPngProofStartupStatus").GetString());
        Assert.Equal(expectedNonReadyCode, root.GetProperty("browserRendererPngProofNonReadyCode").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("browserRendererPngProofNonReadyReason").GetString()));
        Assert.Equal(expectedEvidenceCount, root.GetProperty("browserRendererPngProofEvidenceCount").GetInt32());
        Assert.Equal(
            expectedRequiredArtifactCount,
            root.GetProperty("browserRendererPngProofRequiredArtifactCount").GetInt32());
        Assert.Equal(
            expectedRequiredViewportCount,
            root.GetProperty("browserRendererPngProofRequiredViewportCount").GetInt32());
        Assert.Equal(
            expectedObservedViewportCount,
            root.GetProperty("browserRendererPngProofObservedViewportCount").GetInt32());
    }

    private static void AssertServerOwnedLocalEvaluationOnlyWithoutExternalJudgeCarrier(
        JsonElement root)
    {
        Assert.Equal(
            "server_owned_local_evaluation_only",
            root.GetProperty("visualJudgeProviderKind").GetString());
        Assert.False(root.GetProperty("externalJudgeCarrierPresent").GetBoolean());
        Assert.True(root.GetProperty("externalVisualJudgeRuntimeRemoved").GetBoolean());
        Assert.True(root.GetProperty("serverOwnedLocalEvaluationOnly").GetBoolean());

        var obsoleteExternalCarrierProperties = new[]
        {
            "visualJudgeProviderConfigured",
            "visualJudgeProviderAvailable",
            "externalVisualProvider",
            "externalVisualProviderKind",
            "externalVisualProviderConfigured",
            "externalVisualProviderAvailable",
            "externalVisualProviderConfigComplete",
            "externalVisualProviderFactoryReady",
            "externalVisualProviderNetworkCallExecuted",
            "externalVisualProviderRealProviderExecuted",
            "externalVisualProviderRequestScopedResolverConfigured",
            "externalVisualProviderSharedHttpClientInjected",
            "externalVisualProviderSharedHttpClientOwnedByServerProcess",
            "externalVisualProviderSharedHttpClientDisposedByRegistry",
            "externalProviderCalled"
        };
        foreach (var propertyName in obsoleteExternalCarrierProperties)
        {
            Assert.False(
                root.TryGetProperty(propertyName, out _),
                $"Obsolete external-judge carrier '{propertyName}' must be absent from live public metadata.");
        }
    }

    private static async Task<HttpResponseMessage> GetAuthorizedAsync(
        HttpClient client,
        Uri baseUrl,
        string relativePath,
        string token = RouteAdmissionCurrentToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUrl, relativePath));
        AddRouteAdmissionHeaders(request, token, idempotencyKey: null);
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> PostEvaluateAsync(
        HttpClient client,
        Uri baseUrl,
        string bodyJson,
        string? token,
        string? idempotencyKey)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri(baseUrl, DesignConversionModelServerEndpointRegistry.EndpointPath))
        {
            Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
        };
        AddRouteAdmissionHeaders(request, token, idempotencyKey);
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> PostModelActivationAsync(
        HttpClient client,
        string bodyJson,
        string token)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            DesignConversionModelServerModelActivationHandler.EndpointPath)
        {
            Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation(
            DesignConversionModelServerModelActivationApi.LocalAuthorizationHeaderName,
            token);
        AddRouteAdmissionHeaders(request, RouteAdmissionCurrentToken, idempotencyKey: null);
        return await client.SendAsync(request);
    }

    private static IReadOnlyDictionary<string, string> ModelLifecycleEnvironment()
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ModelLifecycleCurrentTokenEnvironmentName] = ModelLifecycleCurrentToken
        };

    private static Task<DesignConversionPromotedModelActivationRecord> CreateLiveActiveModelCandidateAsync(
        string serverRoot,
        string name)
        => DesignConversionModelActivationRouteRuntimeTests.BuildActivationRecordAsync(
            serverRoot,
            RouteAdmissionTenantId,
            name,
            batchCount: 1);

    private static async Task ActivateLiveModelAsync(
        HttpClient client,
        DesignConversionPromotedModelActivationRecord candidate,
        string idempotencyKey)
    {
        var bodyJson = SerializeModelActivationRequest(
            new DesignConversionModelServerModelActivationRequest(
                Action: DesignConversionModelServerModelActivationHandler.ActionActivate,
                TenantId: RouteAdmissionTenantId,
                RequestedByRef: "architect:live-local-real-activation-fixture",
                Reason: "activate a valid learned checkpoint before admitted live evaluation",
                IdempotencyKey: idempotencyKey,
                ExpectedGeneration: 0,
                Candidate: candidate,
                ExpectedModelVersion: candidate.Model.Version,
                RequestedAtUtc: DateTimeOffset.Parse("2026-07-29T08:00:00Z")));
        using var response = await PostModelActivationAsync(client, bodyJson, ModelLifecycleCurrentToken);
        var responseJson = await response.Content.ReadAsStringAsync();
        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected model activation OK; status={(int)response.StatusCode}; body={responseJson}");
        using var document = JsonDocument.Parse(responseJson);
        Assert.Equal(200, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.True(document.RootElement.GetProperty("applied").GetBoolean());
        AssertNoModelLifecycleTokenLeakage(responseJson);
        AssertNoRouteAdmissionSecretLeakage(responseJson);
    }

    private static async Task AssertHumanApprovalChallengeRouteConfiguredAsync(
        HttpClient client,
        string tempRoot)
    {
        using var malformedResponse = await PostHumanApprovalChallengeAsync(client, "{");
        var malformedJson = await malformedResponse.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.BadRequest, malformedResponse.StatusCode);
        Assert.Equal("REQUEST_JSON_INVALID", JsonNode.Parse(malformedJson)!["errorCode"]!.GetValue<string>());
        AssertHumanApprovalResponseDoesNotLookNotConfigured(malformedJson, tempRoot);

        var minimalBodyJson = JsonSerializer.Serialize(
            new DesignConversionModelServerHumanApprovalChallengeHttpRequest(
                "missing-human-approval-job-" + Guid.NewGuid().ToString("N"),
                new DesignConversionHumanApprovalActor(
                    RouteAdmissionTenantId,
                    "live-local-human-approval-test-actor",
                    [DesignConversionHumanApprovalService.RequiredApproverRole],
                    Authenticated: true)),
            DesignConversionModelServerHttpHandler.JsonOptions);
        using var missingJobResponse = await PostHumanApprovalChallengeAsync(client, minimalBodyJson);
        var missingJobJson = await missingJobResponse.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.NotFound, missingJobResponse.StatusCode);
        Assert.Equal("JOB_NOT_FOUND", JsonNode.Parse(missingJobJson)!["errorCode"]!.GetValue<string>());
        AssertHumanApprovalResponseDoesNotLookNotConfigured(missingJobJson, tempRoot);
    }

    private static async Task AssertSuccessfulHumanApprovalOverHttpAsync(
        HttpClient httpClient,
        Uri readyBaseUrl,
        string jobId,
        string requestId,
        string tempRoot,
        string privateMarker,
        string rawHtml,
        string rawCss,
        string designTreePayload,
        string originalTreePayload)
    {
        const string actorId = "live-local-human-approval-success-actor";
        var actor = new DesignConversionHumanApprovalActor(
            RouteAdmissionTenantId,
            actorId,
            [DesignConversionHumanApprovalService.RequiredApproverRole],
            Authenticated: true);
        var challengeBodyJson = JsonSerializer.Serialize(
            new DesignConversionModelServerHumanApprovalChallengeHttpRequest(jobId, actor),
            DesignConversionModelServerHttpHandler.JsonOptions);

        using var challengeRequest = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri(readyBaseUrl, DesignConversionModelServerEndpointRegistry.HumanApprovalChallengeEndpointPath))
        {
            Content = new StringContent(challengeBodyJson, Encoding.UTF8, "application/json")
        };
        using var challengeResponse = await httpClient.SendAsync(challengeRequest);
        var challengeJson = await challengeResponse.Content.ReadAsStringAsync();
        Assert.True(
            challengeResponse.StatusCode == HttpStatusCode.OK,
            $"Expected successful human-approval challenge; status={(int)challengeResponse.StatusCode}; body={challengeJson}");
        var challenge = JsonSerializer.Deserialize<DesignConversionModelServerHumanApprovalChallengeResponse>(
            challengeJson,
            DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.NotNull(challenge);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.HumanApprovalChallengeEndpointPath, challenge.Endpoint);
        Assert.Equal("POST", challenge.Method);
        Assert.Equal(200, challenge.StatusCode);
        Assert.Equal(jobId, challenge.JobId);
        Assert.Equal(requestId, challenge.RequestId);
        Assert.Equal(RouteAdmissionTenantId, challenge.TenantId);
        Assert.False(challenge.ProductReady);
        Assert.False(string.IsNullOrWhiteSpace(challenge.ChallengeId));
        Assert.False(string.IsNullOrWhiteSpace(challenge.SnapshotDigest));
        Assert.NotEmpty(challenge.RequiredViewportIds);

        IReadOnlyList<DesignConversionHumanApprovalSnapshotRef>[] challengeRefSets =
        [
            challenge.TargetScreenshotRefs,
            challenge.RenderedScreenshotRefs,
            challenge.ComparisonRefs,
            challenge.VisualJudgeRefs
        ];
        var requiredViewportIds = challenge.RequiredViewportIds
            .OrderBy(static viewportId => viewportId, StringComparer.Ordinal)
            .ToArray();
        foreach (var refs in challengeRefSets)
        {
            Assert.NotEmpty(refs);
            Assert.Equal(
                requiredViewportIds,
                refs.Select(static reference => reference.ViewportId)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(static viewportId => viewportId, StringComparer.Ordinal)
                    .ToArray());
            Assert.All(refs, reference =>
            {
                Assert.False(string.IsNullOrWhiteSpace(reference.RefKind));
                Assert.False(string.IsNullOrWhiteSpace(reference.RefId));
                Assert.False(string.IsNullOrWhiteSpace(reference.UriRef));
                Assert.False(string.IsNullOrWhiteSpace(reference.Sha256));
                Assert.True(reference.Width > 0);
                Assert.True(reference.Height > 0);
            });
        }

        Assert.Equal("captured_server_owned_completed_evaluation", challenge.BoundaryMetadata["snapshot_source"]);
        Assert.Equal(bool.TrueString, challenge.BoundaryMetadata["snapshot_digest_computed_server_side"]);
        Assert.Equal(bool.FalseString, challenge.BoundaryMetadata["client_snapshot_digest_accepted"]);
        Assert.Equal(bool.FalseString, challenge.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, challenge.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, challenge.BoundaryMetadata["screenshot_bytes_included"]);
        Assert.Equal(bool.FalseString, challenge.BoundaryMetadata["token_or_secret_included"]);

        var operationKey = "live-local-human-approval-operation-" + Guid.NewGuid().ToString("N");
        var approveBodyJson = JsonSerializer.Serialize(
            new DesignConversionModelServerHumanApprovalApproveHttpRequest(
                challenge.ChallengeId,
                jobId,
                actor,
                challenge.RequiredViewportIds,
                operationKey,
                "Approve the completed refs-only DesignConversion result for this test."),
            DesignConversionModelServerHttpHandler.JsonOptions);
        using var approveRequest = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri(readyBaseUrl, DesignConversionModelServerEndpointRegistry.HumanApprovalApproveEndpointPath))
        {
            Content = new StringContent(approveBodyJson, Encoding.UTF8, "application/json")
        };
        using var approveResponse = await httpClient.SendAsync(approveRequest);
        var approveJson = await approveResponse.Content.ReadAsStringAsync();
        Assert.True(
            approveResponse.StatusCode == HttpStatusCode.OK,
            $"Expected successful human approval; status={(int)approveResponse.StatusCode}; body={approveJson}");
        var approval = JsonSerializer.Deserialize<DesignConversionModelServerHumanApprovalApproveResponse>(
            approveJson,
            DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.NotNull(approval);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.HumanApprovalApproveEndpointPath, approval.Endpoint);
        Assert.Equal("POST", approval.Method);
        Assert.Equal(200, approval.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(approval.ApprovalId));
        Assert.Equal(challenge.ChallengeId, approval.ChallengeId);
        Assert.Equal(jobId, approval.JobId);
        Assert.Equal(requestId, approval.RequestId);
        Assert.Equal(RouteAdmissionTenantId, approval.TenantId);
        Assert.Equal(actorId, approval.ActorId);
        Assert.Equal(challenge.SnapshotDigest, approval.SnapshotDigest);
        Assert.True(approval.AcceptedForUser);
        Assert.False(approval.RuntimeStateChanged);
        Assert.False(approval.ProductReady);
        Assert.False(approval.TrainedOrLearnedProvenance);
        Assert.False(approval.DpoEligible);
        Assert.False(approval.ModelWeightsPublished);
        Assert.Contains(DesignConversionHumanApprovalService.ApprovalBoundary, approval.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_NON_DPO_HUMAN_APPROVAL_ACCEPTED_FOR_USER", approval.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_HUMAN_APPROVAL_PRODUCT_READY_FALSE", approval.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_HUMAN_APPROVAL_DPO_ELIGIBLE_FALSE", approval.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_HUMAN_APPROVAL_MODEL_WEIGHTS_PUBLISHED_FALSE", approval.EvidenceRefs);
        Assert.Equal("accepted-for-user", approval.BoundaryMetadata["human_approval_status"]);
        Assert.Equal(bool.TrueString, approval.BoundaryMetadata["accepted_for_user"]);
        Assert.Equal(bool.FalseString, approval.BoundaryMetadata["runtime_state_changed"]);
        Assert.Equal(bool.FalseString, approval.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, approval.BoundaryMetadata["trained_or_learned_provenance"]);
        Assert.Equal(bool.FalseString, approval.BoundaryMetadata["dpo_eligible"]);
        Assert.Equal(bool.FalseString, approval.BoundaryMetadata["model_weights_published"]);
        Assert.Equal("captured_server_owned_completed_evaluation", approval.BoundaryMetadata["snapshot_source"]);
        Assert.Equal(bool.FalseString, approval.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, approval.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, approval.BoundaryMetadata["screenshot_bytes_included"]);
        Assert.Equal(bool.FalseString, approval.BoundaryMetadata["token_or_secret_included"]);

        foreach (var publicBodyJson in new[] { challengeJson, approveJson })
        {
            Assert.DoesNotContain(tempRoot, publicBodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(privateMarker, publicBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(rawHtml, publicBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(rawCss, publicBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(designTreePayload, publicBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(originalTreePayload, publicBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(operationKey, publicBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain("human-approval-events.jsonl", publicBodyJson, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static async Task<HttpResponseMessage> PostHumanApprovalChallengeAsync(
        HttpClient client,
        string bodyJson)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            DesignConversionModelServerEndpointRegistry.HumanApprovalChallengeEndpointPath)
        {
            Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
        };
        return await client.SendAsync(request);
    }

    private static void AssertHumanApprovalResponseDoesNotLookNotConfigured(
        string responseJson,
        string tempRoot)
    {
        Assert.DoesNotContain(
            "DESIGN_CONVERSION_HUMAN_APPROVAL_STATE_ROOT_NOT_CONFIGURED",
            responseJson,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "DESIGN_CONVERSION_HUMAN_APPROVAL_ROUTE_NOT_READY",
            responseJson,
            StringComparison.Ordinal);
        Assert.DoesNotContain("human_approval_state_root_missing", responseJson, StringComparison.Ordinal);
        Assert.DoesNotContain("human-approval-route-not-ready-reason", responseJson, StringComparison.Ordinal);
        Assert.DoesNotContain("human-approval-events.jsonl", responseJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(tempRoot, responseJson, StringComparison.OrdinalIgnoreCase);
    }

    private static string SerializeModelActivationRequest(
        DesignConversionModelServerModelActivationRequest request)
        => JsonSerializer.Serialize(request, DesignConversionModelServerHttpHandler.JsonOptions);

    private static UploadPngFixture SelectUploadPngFixture(string internalRenderArtifactRoot)
    {
        Assert.True(
            Directory.Exists(internalRenderArtifactRoot),
            $"Expected internal render artifact root to exist at {internalRenderArtifactRoot}.");

        foreach (var path in Directory.EnumerateFiles(internalRenderArtifactRoot, "*.png", SearchOption.AllDirectories)
                     .Order(StringComparer.Ordinal))
        {
            var evidence = DesignConversionBrowserPngEvidenceInspector.Inspect(new Uri(path).AbsoluteUri);
            if (!evidence.IsSuccess || !evidence.Value!.HasNonBlankPixels)
                continue;

            var bytes = File.ReadAllBytes(path);
            var sha256 = "sha256:" + Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            Assert.Equal(evidence.Value.ByteLength, bytes.LongLength);
            return new UploadPngFixture(
                Path: path,
                Bytes: bytes,
                Sha256: sha256,
                Width: evidence.Value.PngWidth,
                Height: evidence.Value.PngHeight);
        }

        throw new InvalidOperationException("Expected at least one nonblank PNG fixture under the internal render artifact root.");
    }

    private static async Task<DesignConversionModelServerLocalCallerPackage> UploadPackageScreenshotsForLiveSubmitAsync(
        HttpClient client,
        string endpointPath,
        ConfiguredReadyJsonSubmitPackageFixture fixture,
        ServerOwnedArtifactRefRewriteHandler serverOwnedArtifactRewriteHandler,
        string idempotencyKeyPrefix)
    {
        var uploadedScreenshots = new List<DesignConversionModelServerArtifactRef>();
        var serverOwnedArtifactUrisBySha256 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var index = 0;
        foreach (var screenshot in fixture.Package.Screenshots ?? [])
        {
            Assert.True(
                fixture.ServerOwnedArtifactUrisBySha256.TryGetValue(screenshot.Sha256, out var sourceFileUri),
                $"Expected a deterministic PNG fixture for {screenshot.Sha256}.");
            Assert.True(Uri.TryCreate(sourceFileUri, UriKind.Absolute, out var sourceUri) && sourceUri.IsFile);

            var bytes = await File.ReadAllBytesAsync(sourceUri.LocalPath);
            var actualSha256 = "sha256:" + Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            Assert.Equal(screenshot.Sha256, actualSha256, ignoreCase: true);
            var pngEvidence = DesignConversionBrowserPngEvidenceInspector.Inspect(sourceUri.AbsoluteUri);
            Assert.True(pngEvidence.IsSuccess, pngEvidence.ErrorMessage);
            Assert.True(pngEvidence.Value!.HasNonBlankPixels);
            var uploadPng = new UploadPngFixture(
                sourceUri.LocalPath,
                bytes,
                actualSha256,
                pngEvidence.Value.PngWidth,
                pngEvidence.Value.PngHeight);

            using var response = await PostArtifactUploadAsync(
                client,
                endpointPath,
                bytes,
                uploadPng,
                screenshot.ArtifactId,
                $"{idempotencyKeyPrefix}-{index++}");
            var bodyJson = await response.Content.ReadAsStringAsync();
            Assert.True(
                response.StatusCode == HttpStatusCode.Created,
                $"Expected artifact upload 201 Created, got {(int)response.StatusCode}: {bodyJson}");
            AssertNoRouteAdmissionSecretLeakage(bodyJson);

            var uploadReceipt = JsonSerializer.Deserialize<DesignConversionModelServerArtifactUploadHttpSuccessResponse>(
                bodyJson,
                DesignConversionModelServerHttpHandler.JsonOptions);
            Assert.NotNull(uploadReceipt);
            Assert.Equal(201, uploadReceipt!.StatusCode);
            Assert.StartsWith("sha256:", uploadReceipt.Sha256, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(screenshot.Sha256, uploadReceipt.Sha256, ignoreCase: true);
            Assert.Equal(uploadReceipt.Sha256, uploadReceipt.ArtifactRef.Sha256, ignoreCase: true);
            Assert.Equal(screenshot.ArtifactId, uploadReceipt.ArtifactId);
            Assert.Equal(screenshot.Kind, uploadReceipt.Kind);
            Assert.Equal(uploadPng.Width, uploadReceipt.Width);
            Assert.Equal(uploadPng.Height, uploadReceipt.Height);

            uploadedScreenshots.Add(uploadReceipt.ArtifactRef with
            {
                Width = screenshot.Width,
                Height = screenshot.Height,
                Uri = uploadReceipt.Sha256,
                Metadata = screenshot.Metadata
            });
            serverOwnedArtifactUrisBySha256[uploadReceipt.Sha256] = uploadReceipt.ArtifactRef.Uri;
        }

        Assert.NotEmpty(uploadedScreenshots);
        serverOwnedArtifactRewriteHandler.AddMappings(serverOwnedArtifactUrisBySha256);
        return fixture.Package with
        {
            Screenshots = uploadedScreenshots,
            DesignRenders = []
        };
    }

    private static async Task<HttpResponseMessage> PostArtifactUploadAsync(
        HttpClient client,
        string endpointPath,
        byte[] bodyBytes,
        UploadPngFixture headerFixture,
        string artifactId,
        string idempotencyKey)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpointPath)
        {
            Content = new ByteArrayContent(bodyBytes)
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        request.Headers.TryAddWithoutValidation(
            DesignConversionModelServerRouteAdmission.AuthorizationHeaderName,
            "Bearer " + RouteAdmissionCurrentToken);
        request.Headers.TryAddWithoutValidation(
            DesignConversionModelServerRouteAdmission.IdempotencyHeaderName,
            idempotencyKey);
        request.Headers.TryAddWithoutValidation(
            DesignConversionModelServerArtifactUploadHttpHandler.ArtifactIdHeaderName,
            artifactId);
        request.Headers.TryAddWithoutValidation(
            DesignConversionModelServerArtifactUploadHttpHandler.ArtifactKindHeaderName,
            "screenshot");
        request.Headers.TryAddWithoutValidation(
            DesignConversionModelServerArtifactUploadHttpHandler.ExpectedSha256HeaderName,
            headerFixture.Sha256);
        request.Headers.TryAddWithoutValidation(
            DesignConversionModelServerArtifactUploadHttpHandler.WidthHeaderName,
            headerFixture.Width.ToString(System.Globalization.CultureInfo.InvariantCulture));
        request.Headers.TryAddWithoutValidation(
            DesignConversionModelServerArtifactUploadHttpHandler.HeightHeaderName,
            headerFixture.Height.ToString(System.Globalization.CultureInfo.InvariantCulture));

        return await client.SendAsync(request);
    }

    private static void AssertLiveLocalArtifactUploadSuccess(
        string bodyJson,
        UploadPngFixture uploadPng,
        string internalRenderArtifactRoot)
    {
        AssertNoRouteAdmissionSecretLeakage(bodyJson);
        Assert.DoesNotContain(Convert.ToBase64String(uploadPng.Bytes), bodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(bodyJson);
        var body = document.RootElement;
        Assert.Equal(DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, body.GetProperty("endpoint").GetString());
        Assert.Equal("POST", body.GetProperty("method").GetString());
        Assert.True(body.GetProperty("liveHttpRouteRegistered").GetBoolean());
        Assert.True(body.GetProperty("aspNetEndpointMapped").GetBoolean());
        Assert.False(body.GetProperty("deploymentVerified").GetBoolean());
        Assert.False(body.GetProperty("publicHttpEndpointVerified").GetBoolean());
        Assert.False(body.GetProperty("productReady").GetBoolean());
        Assert.False(body.GetProperty("rawBytesIncluded").GetBoolean());
        Assert.Equal(uploadPng.Bytes.LongLength, body.GetProperty("byteCount").GetInt64());
        Assert.Equal("image/png", body.GetProperty("mediaType").GetString());
        Assert.Equal(uploadPng.Sha256, body.GetProperty("sha256").GetString());
        Assert.Equal("live-local-machine-backed-upload", body.GetProperty("artifactId").GetString());
        Assert.Equal("screenshot", body.GetProperty("kind").GetString());
        Assert.Equal(uploadPng.Width, body.GetProperty("width").GetInt32());
        Assert.Equal(uploadPng.Height, body.GetProperty("height").GetInt32());

        var artifactRef = JsonSerializer.Deserialize<DesignConversionModelServerArtifactRef>(
            body.GetProperty("artifactRef").GetRawText(),
            DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.NotNull(artifactRef);
        var resolver = new DesignConversionServerOwnedLocalArtifactRefResolver(internalRenderArtifactRoot);
        var resolved = resolver.Validate(artifactRef!, "live-local artifact upload success");
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        Assert.True(Uri.TryCreate(artifactRef!.Uri, UriKind.Absolute, out var artifactUri));
        Assert.True(artifactUri.IsFile, $"Expected uploaded artifact ref to resolve to a file URI, got {artifactRef.Uri}.");
        AssertPathUnderRoot(artifactUri.LocalPath, internalRenderArtifactRoot);

        var metadata = body.GetProperty("boundaryMetadata");
        Assert.Equal("design_conversion_artifact_upload", metadata.GetProperty("asp_net_route_id").GetString());
        Assert.Equal(
            DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
            metadata.GetProperty("asp_net_route_path").GetString());
        Assert.Equal(
            nameof(DesignConversionModelServerArtifactUploadHttpHandler),
            metadata.GetProperty("asp_net_handler_dispatch_target").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("live_http_route_registered").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("aspnet_endpoint_mapped").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_route_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("upload_store_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_bytes_included").GetString());
    }

    private static string[] PublishedIngressPngFiles(string root)
    {
        var ingress = Path.Combine(root, "ingress");
        return Directory.Exists(ingress)
            ? Directory.GetFiles(ingress, "*.png", SearchOption.AllDirectories)
            : [];
    }

    private static string[] TempFiles(string root)
        => Directory.Exists(root)
            ? Directory.GetFiles(root, "*.tmp", SearchOption.AllDirectories)
            : [];

    private static void AssertPathUnderRoot(string path, string root)
    {
        var fullPath = Path.GetFullPath(path);
        var fullRoot = Path.GetFullPath(root);
        var rootWithSeparator = fullRoot.EndsWith(Path.DirectorySeparatorChar)
            ? fullRoot
            : fullRoot + Path.DirectorySeparatorChar;
        Assert.True(
            fullPath.StartsWith(rootWithSeparator, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal),
            $"Expected {fullPath} to be under {fullRoot}.");
    }

    private static string SerializeCanonicalEquivalentEvaluateBody(
        DesignConversionModelServerLocalCallerPackage package)
    {
        if (!Enum.TryParse<DesignConversionSourceKind>(
                package.SourceKind,
                ignoreCase: true,
                out var sourceKind))
        {
            throw new InvalidOperationException("Test package source kind is invalid.");
        }

        var request = new DesignConversionModelServerApiRequest(
            RequestId: package.RequestId,
            SourceKind: sourceKind,
            DesignTree: package.DesignTree!,
            OriginalTree: package.OriginalTree!,
            Html: package.Html!,
            Css: package.Css!,
            Screenshots: package.Screenshots,
            DesignRenders: package.DesignRenders,
            Budget: package.Budget,
            Metadata: package.Metadata);
        var source = JsonNode.Parse(
            JsonSerializer.Serialize(request, DesignConversionModelServerHttpHandler.JsonOptions))!.AsObject();
        var reordered = new JsonObject();
        foreach (var key in new[]
                 {
                     "metadata",
                     "budget",
                     "designRenders",
                     "screenshots",
                     "css",
                     "html",
                     "originalTree",
                     "designTree",
                     "sourceKind",
                     "requestId"
                 })
        {
            if (!source.TryGetPropertyValue(key, out var node))
                continue;

            reordered[key] = node is null
                ? null
                : JsonNode.Parse(node.ToJsonString(DesignConversionModelServerHttpHandler.JsonOptions));
        }

        return reordered.ToJsonString(DesignConversionModelServerHttpHandler.JsonOptions);
    }

    private sealed record UploadPngFixture(
        string Path,
        byte[] Bytes,
        string Sha256,
        int Width,
        int Height);

    private static void AddRouteAdmissionHeaders(
        HttpRequestMessage request,
        string? token,
        string? idempotencyKey)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.TryAddWithoutValidation(
                DesignConversionModelServerRouteAdmission.AuthorizationHeaderName,
                "Bearer " + token.Trim());
        }

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            request.Headers.TryAddWithoutValidation(
                DesignConversionModelServerRouteAdmission.IdempotencyHeaderName,
                idempotencyKey.Trim());
        }
    }

    private static void AssertRouteAdmissionPublicMetadata(
        JsonElement root,
        string expectedConfigurationStatus,
        int expectedAcceptedKeyCount,
        bool expectedPreviousTokenConfigured,
        bool expectedPreviousTokenActive)
    {
        Assert.True(root.GetProperty("routeAdmissionEnabled").GetBoolean());
        Assert.Equal(
            expectedConfigurationStatus,
            root.GetProperty("routeAdmissionConfigurationStatus").GetString());
        Assert.Equal(
            DesignConversionModelServerRouteAdmission.AuthorizationHeaderName,
            root.GetProperty("routeAdmissionHeaderName").GetString());
        Assert.Equal(
            DesignConversionModelServerRouteAdmission.ProjectAuthorizationHeaderName,
            root.GetProperty("routeAdmissionProjectHeaderName").GetString());
        Assert.Equal(expectedAcceptedKeyCount, root.GetProperty("routeAdmissionAcceptedKeyCount").GetInt32());
        Assert.Equal(expectedPreviousTokenConfigured, root.GetProperty("routeAdmissionPreviousTokenConfigured").GetBoolean());
        Assert.Equal(expectedPreviousTokenActive, root.GetProperty("routeAdmissionPreviousTokenActive").GetBoolean());
        Assert.Equal(
            expectedPreviousTokenActive ? "previous_token_active" :
            expectedPreviousTokenConfigured ? "previous_token_retired" :
            expectedConfigurationStatus == "enabled" ? "current_only" : "not_active",
            root.GetProperty("routeAdmissionRotationState").GetString());
        Assert.Equal(
            DesignConversionModelServerRouteAdmission.RouteScopeDefault,
            root.GetProperty("routeAdmissionScope").GetString());
        Assert.Equal(
            expectedConfigurationStatus == "enabled",
            root.GetProperty("routeAdmissionTenantBound").GetBoolean());
        Assert.False(root.GetProperty("routeAdmissionRawTenantExposed").GetBoolean());
        var tenantIdRef = root.GetProperty("routeAdmissionTenantIdRef").GetString();
        if (expectedConfigurationStatus == "enabled")
        {
            Assert.Equal(
                DesignConversionModelActivationRegistry.BuildTenantIdRef(RouteAdmissionTenantId),
                tenantIdRef);
            Assert.DoesNotContain(RouteAdmissionTenantId, root.GetRawText(), StringComparison.Ordinal);
        }
        else
        {
            Assert.Equal(string.Empty, tenantIdRef);
        }
        Assert.Equal(
            DesignConversionModelServerRouteAdmission.IdempotencyHeaderName,
            root.GetProperty("idempotencyHeaderName").GetString());
        Assert.Equal(
            expectedConfigurationStatus == "enabled",
            root.GetProperty("idempotencyRequired").GetBoolean());
    }

    private static void AssertAdmissionRejectedBeforeRuntime(
        string bodyJson,
        string expectedErrorCode,
        params string[] forbidden)
    {
        Assert.Contains(expectedErrorCode, bodyJson, StringComparison.Ordinal);
        AssertNoRouteAdmissionSecretLeakage(bodyJson, forbidden);
        using var document = JsonDocument.Parse(bodyJson);
        var root = document.RootElement;
        Assert.Equal(expectedErrorCode, root.GetProperty("errorCode").GetString());
        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("route_admission_authorized").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("persistence_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("browser_runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("model_provider_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("idempotency_payload_digest_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("token_value_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("token_hash_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("token_fingerprint_exposed").GetString());
    }

    private static void AssertNoRouteAdmissionSecretLeakage(string text, params string[] forbidden)
    {
        var values = forbidden
            .Concat([
                RouteAdmissionCurrentToken,
                RouteAdmissionPreviousToken,
                RouteAdmissionTenantId
            ])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Concat([
                Sha256Hex(RouteAdmissionCurrentToken),
                Sha256Hex(RouteAdmissionPreviousToken)
            ])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var value in values)
            Assert.DoesNotContain(value, text, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertNoModelLifecycleTokenLeakage(string text, params string[] forbidden)
    {
        var values = forbidden.Concat([
                ModelLifecycleCurrentToken,
                ModelLifecyclePreviousToken,
                ModelLifecycleUnknownToken,
                Sha256Hex(ModelLifecycleCurrentToken),
                Sha256Hex(ModelLifecyclePreviousToken),
                Sha256Hex(ModelLifecycleUnknownToken)
            ])
            .SelectMany(value => new[] { value, JsonSerializer.Serialize(value)[1..^1] })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var value in values)
            Assert.DoesNotContain(value, text, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<int> CountPersistedRecordsAsync(string recordsFile, string requestId)
    {
        if (!File.Exists(recordsFile))
            return 0;

        var count = 0;
        foreach (var line in await File.ReadAllLinesAsync(recordsFile))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var record = JsonSerializer.Deserialize<DesignConversionLocalArtifactDbRecord>(
                line,
                DesignConversionCheckpointJsonOptions.Standard);
            if (string.Equals(record?.RequestId, requestId, StringComparison.Ordinal))
                count++;
        }

        return count;
    }

    private static async Task<int> CountNonEmptyLinesAsync(string path)
    {
        if (!File.Exists(path))
            return 0;

        return (await File.ReadAllLinesAsync(path, Encoding.UTF8))
            .Count(line => !string.IsNullOrWhiteSpace(line));
    }

    private static int CountDurableCompletedRecords(string recordsRoot)
        => Directory.Exists(recordsRoot)
            ? Directory.EnumerateFiles(recordsRoot, "*.json", SearchOption.TopDirectoryOnly).Count()
            : 0;

    private static void AssertDurableReplayPublicMetadata(
        JsonElement root,
        int expectedCompletedRecordCount)
    {
        Assert.True(root.GetProperty("idempotencyDurableReplayEnabled").GetBoolean());
        Assert.Equal("enabled", root.GetProperty("idempotencyDurableReplayStoreState").GetString());
        Assert.Equal(
            expectedCompletedRecordCount,
            root.GetProperty("idempotencyDurableReplayCompletedRecordCount").GetInt32());
        Assert.DoesNotContain("route-admission-idempotency", root.GetRawText(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("runtime-data", root.GetRawText(), StringComparison.OrdinalIgnoreCase);
    }

    private static async Task AssertNoDurableRestartLeakageAsync(
        string tempRoot,
        string recordsFile,
        string durableRecordsRoot,
        IReadOnlyList<string> publicTexts,
        params string[] forbidden)
    {
        var privateRuntimeRoot = Path.Combine(tempRoot, "runtime-data");
        var durableStoreRoot = Path.GetDirectoryName(durableRecordsRoot) ?? durableRecordsRoot;
        var forbiddenValues = forbidden
            .Concat([
                RouteAdmissionCurrentToken,
                RouteAdmissionPreviousToken,
                RouteAdmissionTenantId,
                tempRoot,
                recordsFile,
                Path.GetDirectoryName(recordsFile) ?? string.Empty,
                privateRuntimeRoot,
                durableStoreRoot,
                durableRecordsRoot
            ])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var forbiddenValueRepresentations = forbiddenValues
            .SelectMany(value => new[]
            {
                value,
                JsonSerializer.Serialize(value).Trim('"')
            })
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var text in publicTexts.Where(text => !string.IsNullOrWhiteSpace(text)))
        {
            AssertNoRouteAdmissionSecretLeakage(text, forbiddenValueRepresentations);
            Assert.DoesNotContain("http://127.0.0.1:0", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("sha256:" + new string('0', 64), text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("design-conversion-route-admission-request-digest", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("RequestDigest", text, StringComparison.OrdinalIgnoreCase);
        }

        var diskFiles = Directory.Exists(durableRecordsRoot)
            ? Directory.EnumerateFiles(durableRecordsRoot, "*.json", SearchOption.TopDirectoryOnly).ToList()
            : new List<string>();
        if (File.Exists(recordsFile))
            diskFiles.Add(recordsFile);

        foreach (var path in diskFiles)
        {
            var fileName = Path.GetFileName(path);
            foreach (var value in forbiddenValueRepresentations)
                Assert.DoesNotContain(value, fileName, StringComparison.OrdinalIgnoreCase);

            var text = await File.ReadAllTextAsync(path, Encoding.UTF8);
            foreach (var value in forbiddenValueRepresentations)
                Assert.DoesNotContain(value, text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("file:", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("http://127.0.0.1:0", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("sha256:" + new string('0', 64), text, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string Sha256Hex(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static IEnumerable<(string Path, string Value)> EnumerateJsonStrings(JsonElement element, string path)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                yield return (path, element.GetString() ?? string.Empty);
                break;
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    foreach (var item in EnumerateJsonStrings(property.Value, $"{path}.{property.Name}"))
                        yield return item;
                }

                break;
            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    foreach (var nested in EnumerateJsonStrings(item, $"{path}[{index.ToString()}]"))
                        yield return nested;
                    index++;
                }

                break;
        }
    }

    private static bool LooksLikePublicLocalPathLeak(string value)
        => value.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ||
           value.StartsWith(@"\\", StringComparison.Ordinal) ||
           ContainsWindowsDriveRoot(value) ||
           ContainsLocalPosixPath(value);

    // Ready/health JSON читается снаружи, поэтому ловим Linux-home/tmp формы так же строго,
    // как Windows/UNC: public ref не должен случайно содержать приватный сегмент пути.
    private static bool ContainsLocalPosixPath(string value)
    {
        var normalized = value
            .Replace(@"\/", "/", StringComparison.Ordinal)
            .Replace('\\', '/');
        return normalized.Contains("/tmp/", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains("/root/", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains("/home/", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains("/mnt/", StringComparison.OrdinalIgnoreCase) ||
               normalized.StartsWith("/tmp", StringComparison.OrdinalIgnoreCase) ||
               normalized.StartsWith("/root", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsWindowsDriveRoot(string value)
    {
        for (var index = 0; index < value.Length - 2; index++)
        {
            if (((value[index] is >= 'a' and <= 'z') || (value[index] is >= 'A' and <= 'Z')) &&
                (index == 0 || !char.IsLetterOrDigit(value[index - 1])) &&
                value[index + 1] == ':' &&
                value[index + 2] is '\\' or '/')
            {
                return true;
            }
        }

        return false;
    }

    private static string? TryReadGitHead(string repoRoot)
        => TryReadGitHeadWithProcess(repoRoot, gitDirectory: null) ??
           TryReadGitHeadFromResolvedGitDirectory(repoRoot);

    private static string? TryReadGitHeadFromResolvedGitDirectory(string repoRoot)
    {
        var gitDirectory = TryResolveGitDirectory(repoRoot);
        return string.IsNullOrWhiteSpace(gitDirectory)
            ? null
            : TryReadGitHeadWithProcess(repoRoot, gitDirectory);
    }

    private static string? TryReadGitHeadWithProcess(string repoRoot, string? gitDirectory)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    WorkingDirectory = repoRoot,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };
            if (!string.IsNullOrWhiteSpace(gitDirectory))
            {
                process.StartInfo.ArgumentList.Add("--git-dir");
                process.StartInfo.ArgumentList.Add(gitDirectory);
            }

            process.StartInfo.ArgumentList.Add("rev-parse");
            process.StartInfo.ArgumentList.Add("HEAD");
            process.Start();
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderrDrain = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(5000))
            {
                process.Kill(entireProcessTree: true);
                return null;
            }

            _ = stderrDrain.GetAwaiter().GetResult();
            return process.ExitCode == 0
                ? TryNormalizeCommit(stdout.GetAwaiter().GetResult())
                : null;
        }
        catch (Exception ex) when (
            ex is InvalidOperationException or
                IOException or
                UnauthorizedAccessException or
                System.ComponentModel.Win32Exception)
        {
            return null;
        }
    }

    private static string? TryResolveGitDirectory(string repoRoot)
    {
        try
        {
            var dotGit = Path.Combine(repoRoot, ".git");
            if (Directory.Exists(dotGit))
                return dotGit;

            if (!File.Exists(dotGit))
                return null;

            var firstLine = File.ReadLines(dotGit).FirstOrDefault();
            const string GitDirPrefix = "gitdir:";
            if (firstLine?.StartsWith(GitDirPrefix, StringComparison.OrdinalIgnoreCase) != true)
                return null;

            var gitDirectory = firstLine[GitDirPrefix.Length..].Trim();
            if (string.IsNullOrWhiteSpace(gitDirectory))
                return null;

            // Тесты часто идут из WSL поверх Windows worktree: gitdir-файл
            // хранит `C:/...`, а Linux git принимает тот же путь как `/mnt/c/...`.
            var resolved = LooksLikeWindowsDrivePath(gitDirectory)
                ? gitDirectory
                : Path.IsPathRooted(gitDirectory)
                    ? gitDirectory
                    : Path.GetFullPath(Path.Combine(repoRoot, gitDirectory));
            return NormalizeGitDirectoryForCurrentRuntime(resolved);
        }
        catch (Exception ex) when (
            ex is IOException or
                UnauthorizedAccessException or
                ArgumentException or
                NotSupportedException)
        {
            return null;
        }
    }

    private static string NormalizeGitDirectoryForCurrentRuntime(string gitDirectory)
    {
        if (!LooksLikeWindowsDrivePath(gitDirectory) || Directory.Exists(gitDirectory))
            return gitDirectory;

        var drive = char.ToLowerInvariant(gitDirectory[0]);
        var rest = gitDirectory[2..].Replace('\\', '/').TrimStart('/');
        var wslPath = $"/mnt/{drive}/{rest}";
        return Directory.Exists(wslPath) ? wslPath : gitDirectory;
    }

    private static bool LooksLikeWindowsDrivePath(string value)
        => value.Length >= 3 &&
           char.IsLetter(value[0]) &&
           value[1] == ':' &&
           value[2] is '\\' or '/';

    private static bool CommitsMatch(string sourceCommit, string expectedCommit)
    {
        var source = TryNormalizeCommit(sourceCommit);
        var expected = TryNormalizeCommit(expectedCommit);
        if (source is null || expected is null)
            return false;

        return source.Length == expected.Length
            ? string.Equals(source, expected, StringComparison.OrdinalIgnoreCase)
            : source.Length < expected.Length
                ? expected.StartsWith(source, StringComparison.OrdinalIgnoreCase)
                : source.StartsWith(expected, StringComparison.OrdinalIgnoreCase);
    }

    private static string? TryNormalizeCommit(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        if (trimmed.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
            return null;

        if (trimmed.Length is < 7 or > 64)
            return null;

        return trimmed.All(Uri.IsHexDigit)
            ? trimmed.ToLowerInvariant()
            : null;
    }

    private static string ResolveServerAssemblyPath(string projectPath)
    {
        var projectDirectory = Path.GetDirectoryName(projectPath)
            ?? throw new DirectoryNotFoundException($"Could not resolve project directory for {projectPath}.");
        var configuration = AppContext.BaseDirectory.Contains(
            $"{Path.DirectorySeparatorChar}Release{Path.DirectorySeparatorChar}",
            StringComparison.OrdinalIgnoreCase)
            ? "Release"
            : "Debug";
        var assemblyPath = Path.Combine(
            projectDirectory,
            "bin",
            configuration,
            "net9.0",
            "DesignConversionLiveLocalServer.dll");

        if (!File.Exists(assemblyPath))
            throw new FileNotFoundException(
                "DesignConversionLiveLocalServer tests require the tool project to be built before spawning the server.",
                assemblyPath);

        return assemblyPath;
    }

    private static string ResolveDotnetExecutable()
    {
        var configured = Environment.GetEnvironmentVariable("DOTNET_EXE");
        if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
            return configured;

        const string pinnedDotnet = @"C:\tmp\dotnet-10.0.300\dotnet.exe";
        return File.Exists(pinnedDotnet)
            ? pinnedDotnet
            : "dotnet";
    }

    private static void AssertMachineBackedPngProof(JsonElement root, string? internalRenderArtifactRoot = null)
    {
        Assert.True(root.GetProperty("machineBackedApiPngProofValid").GetBoolean());
        var artifacts = root.GetProperty("machineBackedApiProbeArtifactEvidence")
            .EnumerateArray()
            .ToArray();
        var internalPngs = string.IsNullOrWhiteSpace(internalRenderArtifactRoot)
            ? Array.Empty<InternalPngEvidence>()
            : ReadInternalPngEvidence(internalRenderArtifactRoot);

        Assert.Equal(4, artifacts.Length);
        Assert.Contains(artifacts, artifact =>
            artifact.GetProperty("kind").GetString() == "screenshot" &&
            artifact.GetProperty("viewportId").GetString() == "pass143-live-local-desktop");
        Assert.Contains(artifacts, artifact =>
            artifact.GetProperty("kind").GetString() == "screenshot" &&
            artifact.GetProperty("viewportId").GetString() == "pass143-live-local-mobile");
        Assert.Contains(artifacts, artifact =>
            artifact.GetProperty("kind").GetString() == "design-render" &&
            artifact.GetProperty("viewportId").GetString() == "pass143-live-local-desktop");
        Assert.Contains(artifacts, artifact =>
            artifact.GetProperty("kind").GetString() == "design-render" &&
            artifact.GetProperty("viewportId").GetString() == "pass143-live-local-mobile");

        foreach (var artifact in artifacts)
        {
            var viewportId = artifact.GetProperty("viewportId").GetString();
            if (viewportId == "pass143-live-local-desktop")
            {
                Assert.Equal(1440, artifact.GetProperty("width").GetInt32());
                Assert.Equal(900, artifact.GetProperty("height").GetInt32());
            }
            else
            {
                Assert.Equal("pass143-live-local-mobile", viewportId);
                Assert.Equal(390, artifact.GetProperty("width").GetInt32());
                Assert.Equal(844, artifact.GetProperty("height").GetInt32());
            }

            Assert.Equal(
                BrowserBackedDesignConversionRenderProvider.ProviderKind,
                artifact.GetProperty("providerKind").GetString());
            Assert.True(artifact.GetProperty("browserOrPlaywrightUsed").GetBoolean());
            Assert.True(artifact.GetProperty("visualFidelityClaimed").GetBoolean());
            Assert.Equal(
                "browser_backed_render_evidence",
                artifact.GetProperty("hiddenDeterminismBoundary").GetString());
            Assert.True(artifact.GetProperty("browserScreenshotFileVerified").GetBoolean());
            Assert.True(artifact.GetProperty("browserScreenshotSha256Verified").GetBoolean());
            Assert.True(artifact.GetProperty("browserScreenshotNonBlankPixelsVerified").GetBoolean());
            Assert.True(artifact.GetProperty("pngSignatureVerified").GetBoolean());
            Assert.True(artifact.GetProperty("sha256Verified").GetBoolean());
            Assert.True(artifact.GetProperty("hasNonBlankPixels").GetBoolean());
            Assert.True(artifact.GetProperty("nonWhitePixelCount").GetInt32() > 0);
            Assert.True(artifact.GetProperty("distinctColorCount").GetInt32() > 1);

            var uriText = artifact.GetProperty("uri").GetString();
            Assert.True(Uri.TryCreate(uriText, UriKind.Absolute, out var uri), $"Expected absolute file uri, got {uriText}.");
            Assert.Equal("artifact", uri.Scheme);
            Assert.StartsWith("artifact://pass143/live-local/machine-backed-api-probe/", uriText, StringComparison.Ordinal);
            Assert.Equal(uriText, artifact.GetProperty("artifactRef").GetString());
            Assert.Equal("image/png", artifact.GetProperty("mediaType").GetString());

            Assert.Equal(JsonValueKind.Null, artifact.GetProperty("localPath").ValueKind);
            var logicalFilename = artifact.GetProperty("logicalFilename").GetString();
            Assert.False(string.IsNullOrWhiteSpace(logicalFilename));
            Assert.EndsWith(".png", logicalFilename, StringComparison.Ordinal);
            Assert.DoesNotContain("..", logicalFilename, StringComparison.Ordinal);
            Assert.DoesNotContain("\\", logicalFilename, StringComparison.Ordinal);
            Assert.DoesNotContain("/", logicalFilename, StringComparison.Ordinal);

            var byteLength = artifact.GetProperty("byteLength").GetInt64();
            Assert.True(byteLength >= 8, $"PNG proof byte length was too short: {byteLength}.");
            var sha256 = artifact.GetProperty("sha256").GetString();
            AssertSha256Ref(sha256);

            if (internalPngs.Length > 0)
            {
                Assert.Contains(internalPngs, png =>
                    png.ByteLength == byteLength &&
                    string.Equals(png.Sha256Ref, sha256, StringComparison.OrdinalIgnoreCase));
            }
        }
    }

    private static InternalPngEvidence[] ReadInternalPngEvidence(string internalRenderArtifactRoot)
    {
        Assert.True(
            Directory.Exists(internalRenderArtifactRoot),
            $"Expected internal render artifact root to exist at {internalRenderArtifactRoot}.");
        return Directory
            .EnumerateFiles(internalRenderArtifactRoot, "*.png", SearchOption.AllDirectories)
            .Select(path =>
            {
                var bytes = File.ReadAllBytes(path);
                Assert.True(bytes.Length >= 8, $"PNG proof was too short: {path}.");
                Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, bytes.Take(8).ToArray());
                var actualSha = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
                return new InternalPngEvidence(bytes.LongLength, $"sha256:{actualSha}");
            })
            .ToArray();
    }

    private sealed record InternalPngEvidence(long ByteLength, string Sha256Ref);




    private static string[] ReadStringArray(JsonElement root, string propertyName)
        => root.GetProperty(propertyName)
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToArray();

    private static void AssertReadyJsonBaseUrlForSubmit(JsonElement readyRoot, Uri readyBaseUrl)
    {
        var baseUrlText = readyRoot.GetProperty("baseUrl").GetString();
        Assert.False(string.IsNullOrWhiteSpace(baseUrlText));
        Assert.True(readyBaseUrl.IsAbsoluteUri);
        Assert.Equal(Uri.UriSchemeHttp, readyBaseUrl.Scheme);
        Assert.True(IsLoopbackHost(readyBaseUrl.Host), $"Expected loopback ready.json baseUrl, got {readyBaseUrl}.");
        Assert.True(readyBaseUrl.Port > 0, $"Expected ready.json baseUrl to include a concrete port, got {readyBaseUrl}.");
        Assert.Equal("/", readyBaseUrl.AbsolutePath);
        Assert.Equal(string.Empty, readyBaseUrl.Query);
        Assert.Equal(string.Empty, readyBaseUrl.Fragment);
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry.EndpointPath,
            readyRoot.GetProperty("endpointPath").GetString());

        var routePaths = ReadStringArray(readyRoot, "routePaths");
        Assert.Contains(DesignConversionModelServerEndpointRegistry.EndpointPath, routePaths);

        var submitUri = new Uri(
            new Uri(readyBaseUrl.GetLeftPart(UriPartial.Authority) + "/"),
            DesignConversionModelServerEndpointRegistry.EndpointPath.TrimStart('/'));
        Assert.Equal(readyBaseUrl.GetLeftPart(UriPartial.Authority), submitUri.GetLeftPart(UriPartial.Authority));
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointPath, submitUri.AbsolutePath);
        Assert.False(readyRoot.GetProperty("productReady").GetBoolean());
    }

    private static bool IsLoopbackHost(string host)
        => string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
           (IPAddress.TryParse(host, out var address) && IPAddress.IsLoopback(address));

    [Fact]
    public void ConfiguredReadyJsonSubmitPackage_UsesTrustedPlatformIdentityAndKeepsBudgetMetadataClientOnly()
    {
        const string privateMarker = "PRIVATE_FLOW47_FIXTURE_CONTRACT_SECRET";
        const string requestId = "flow47-fixture-contract-request";

        var package = CreateConfiguredReadyJsonSubmitPackage(
            privateMarker,
            includeMobileViewport: false,
            requestId: requestId);

        Assert.Equal(requestId, package.RequestId);
        Assert.NotNull(package.Budget);
        var budget = package.Budget!;
        Assert.Equal("flow47-platform-user", budget.UserId);
        Assert.DoesNotContain(privateMarker, budget.UserId, StringComparison.Ordinal);
        Assert.Equal("paid", budget.Tier);
        Assert.Equal(0.25m, budget.BudgetUsd);
        Assert.Equal(1, budget.MaxImprovementIterations);

        Assert.NotNull(package.Metadata);
        var metadata = package.Metadata!;
        Assert.Equal(RouteAdmissionTenantId, metadata["tenant_id"]);
        Assert.Equal("paid", metadata[DesignConversionModelServerApi.ServerAssignedDesignTierMetadataKey]);
        var monthlyCapText = metadata[DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey];
        Assert.Equal("125.75", monthlyCapText);
        Assert.True(
            decimal.TryParse(
                monthlyCapText,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out var monthlyCapUsd),
            "Expected monthly visual cost cap metadata to be parseable as an invariant decimal.");
        Assert.Equal(125.75m, monthlyCapUsd);
        Assert.Equal(
            DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
            metadata[DesignConversionModelServerApi.DesignTierAuthorityMetadataKey]);
        Assert.Equal(
            DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
            metadata[DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey]);
        Assert.Equal(
            DesignConversionModelServerApi.VerifiedInboundPrincipalAuthority,
            metadata[DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey]);

        Assert.NotNull(budget.Metadata);
        var budgetMetadata = budget.Metadata!;
        Assert.Equal(privateMarker, budgetMetadata["private_marker"]);
        Assert.False(budgetMetadata.ContainsKey("tenant_id"));
        Assert.False(budgetMetadata.ContainsKey(DesignConversionModelServerApi.ServerAssignedDesignTierMetadataKey));
        Assert.False(budgetMetadata.ContainsKey(DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey));
        Assert.False(budgetMetadata.ContainsKey(DesignConversionModelServerApi.DesignTierAuthorityMetadataKey));
        Assert.False(budgetMetadata.ContainsKey(DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey));
        Assert.False(budgetMetadata.ContainsKey(DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey));
    }

    private static DesignConversionModelServerLocalCallerPackage CreateConfiguredReadyJsonSubmitPackage(
        string privateMarker,
        bool includeMobileViewport = true,
        string? requestId = null)
    {
        const string canonicalPlatformUserId = "flow47-platform-user";
        const string canonicalTier = "paid";
        const decimal canonicalPerJobBudgetUsd = 0.25m;
        const string canonicalMonthlyCapUsd = "125.75";

        var screenshots = new List<DesignConversionModelServerArtifactRef>
        {
            new(
                ArtifactId: "flow47-ready-json-source-screenshot",
                Kind: "screenshot",
                Width: 1440,
                Height: 900,
                Uri: "artifact://flow47/ready-json/source-screenshot.png",
                Sha256: "sha256:flow47-ready-json-source-screenshot",
                Metadata: ClientClaimedTrustMetadata(privateMarker))
        };
        var designRenders = new List<DesignConversionModelServerArtifactRef>
        {
            new(
                ArtifactId: "flow47-ready-json-design-render",
                Kind: "design-render",
                Width: 1440,
                Height: 900,
                Uri: "artifact://flow47/ready-json/design-render.png",
                Sha256: "sha256:flow47-ready-json-design-render",
                Metadata: ClientClaimedTrustMetadata(privateMarker))
        };
        if (includeMobileViewport)
        {
            screenshots.Add(new DesignConversionModelServerArtifactRef(
                ArtifactId: "flow47-ready-json-source-screenshot-mobile",
                Kind: "screenshot",
                Width: 390,
                Height: 844,
                Uri: "artifact://flow47/ready-json/source-screenshot-mobile.png",
                Sha256: "sha256:flow47-ready-json-source-screenshot-mobile",
                Metadata: ClientClaimedTrustMetadata(privateMarker)));

            designRenders.Add(new DesignConversionModelServerArtifactRef(
                ArtifactId: "flow47-ready-json-design-render-mobile",
                Kind: "design-render",
                Width: 390,
                Height: 844,
                Uri: "artifact://flow47/ready-json/design-render-mobile.png",
                Sha256: "sha256:flow47-ready-json-design-render-mobile",
                Metadata: ClientClaimedTrustMetadata(privateMarker)));
        }

        return new DesignConversionModelServerLocalCallerPackage(
            RequestId: requestId ?? "flow47-configured-ready-json-submit-request",
            SourceKind: nameof(DesignConversionSourceKind.Figma),
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: "flow47-ready-json-design-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:flow47-ready-json-design-tree",
                Payload: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"generated\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker
                }),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: "flow47-ready-json-original-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:flow47-ready-json-original-tree",
                Payload: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"original\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker
                }),
            Html: $"<main class=\"flow47\" data-secret=\"{privateMarker}\"><strong>Flow47 configured ready.json submit</strong></main>",
            Css: $":root{{--private-marker:'{privateMarker}';}}html,body{{margin:0;min-height:100%;}}.flow47{{min-height:100vh;background:#2463eb;color:#fff;display:grid;place-items:center;font:700 32px/1.2 Arial,sans-serif;}}",
            Screenshots: screenshots,
            DesignRenders: designRenders,
            Budget: new DesignConversionModelServerBudget(
                UserId: canonicalPlatformUserId,
                Tier: canonicalTier,
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: includeMobileViewport ? 2 : 1,
                BudgetUsd: canonicalPerJobBudgetUsd,
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker
                },
                MaxImprovementIterations: 1),
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker,
                ["flow47_ready_json_submit"] = bool.TrueString,
                ["tenant_id"] = RouteAdmissionTenantId,
                [DesignConversionModelServerApi.ServerAssignedDesignTierMetadataKey] = canonicalTier,
                [DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey] =
                    canonicalMonthlyCapUsd,
                [DesignConversionModelServerApi.DesignTierAuthorityMetadataKey] =
                    DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
                [DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey] =
                    DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
                [DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey] =
                    DesignConversionModelServerApi.VerifiedInboundPrincipalAuthority
            });
    }

    private static async Task<ConfiguredReadyJsonSubmitPackageFixture> CreateConfiguredReadyJsonSubmitPackageAsync(
        string privateMarker,
        string strictArtifactRoot,
        bool includeMobileViewport = true,
        string? requestId = null,
        bool includeServerOwnedNegativeDelta = false)
    {
        var package = CreateConfiguredReadyJsonSubmitPackage(
            privateMarker,
            includeMobileViewport,
            requestId);
        var serverOwnedBaselinePackage = includeServerOwnedNegativeDelta
            ? package with
            {
                RequestId = package.RequestId + "-server-owned-baseline",
                Css = package.Css +
                      ".flow47{box-shadow:inset 0 0 0 1px #000}"
            }
            : package;
        var renderer = new BrowserBackedDesignConversionRenderProvider(
            LocalChromiumBrowserProcessAdapter.CreateServerOwnedDefault(
                timeout: TimeSpan.FromSeconds(30),
                artifactRoot: strictArtifactRoot));
        var desktop = new RenderQaResolution("source-1440x900", 1440, 900, 1d);
        var serverOwnedArtifactUrisBySha256 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var desktopScreenshot = await CreateConfiguredReadyJsonServerOwnedArtifactRefAsync(
            renderer,
            package,
            "flow47-ready-json-source-screenshot",
            "screenshot",
            desktop);
        serverOwnedArtifactUrisBySha256[desktopScreenshot.PublicRef.Sha256] = desktopScreenshot.ServerOwnedFileUri;
        var desktopDesignRender = await CreateConfiguredReadyJsonServerOwnedArtifactRefAsync(
            renderer,
            serverOwnedBaselinePackage,
            "flow47-ready-json-design-render",
            "design-render",
            desktop);
        serverOwnedArtifactUrisBySha256[desktopDesignRender.PublicRef.Sha256] = desktopDesignRender.ServerOwnedFileUri;
        var screenshots = new List<DesignConversionModelServerArtifactRef>
        {
            desktopScreenshot.PublicRef
        };
        var designRenders = new List<DesignConversionModelServerArtifactRef>
        {
            desktopDesignRender.PublicRef
        };
        if (includeServerOwnedNegativeDelta)
        {
            Assert.NotEqual(
                desktopScreenshot.PublicRef.Sha256,
                desktopDesignRender.PublicRef.Sha256);
        }

        if (includeMobileViewport)
        {
            var mobile = new RenderQaResolution("source-390x844", 390, 844, 3d);
            var mobileScreenshot = await CreateConfiguredReadyJsonServerOwnedArtifactRefAsync(
                renderer,
                package,
                "flow47-ready-json-source-screenshot-mobile",
                "screenshot",
                mobile);
            serverOwnedArtifactUrisBySha256[mobileScreenshot.PublicRef.Sha256] = mobileScreenshot.ServerOwnedFileUri;
            screenshots.Add(mobileScreenshot.PublicRef);

            var mobileDesignRender = await CreateConfiguredReadyJsonServerOwnedArtifactRefAsync(
                renderer,
                serverOwnedBaselinePackage,
                "flow47-ready-json-design-render-mobile",
                "design-render",
                mobile);
            serverOwnedArtifactUrisBySha256[mobileDesignRender.PublicRef.Sha256] = mobileDesignRender.ServerOwnedFileUri;
            designRenders.Add(mobileDesignRender.PublicRef);
        }

        return new ConfiguredReadyJsonSubmitPackageFixture(
            package with
            {
                Screenshots = screenshots,
                DesignRenders = designRenders
            },
            serverOwnedArtifactUrisBySha256);
    }

    private static async Task<ConfiguredReadyJsonServerOwnedArtifactRefFixture> CreateConfiguredReadyJsonServerOwnedArtifactRefAsync(
        IDesignConversionRenderProvider renderer,
        DesignConversionModelServerLocalCallerPackage package,
        string artifactId,
        string kind,
        RenderQaResolution resolution)
    {
        var physicalResolution = DesignConversionBrowserPhysicalScreenshotResolution.Resolve(
            resolution,
            "FLOW47_READY_JSON_ARTIFACT_RESOLUTION_INVALID",
            "Flow47 ready.json server-owned artifact fixture");
        Assert.True(physicalResolution.IsSuccess, physicalResolution.ErrorMessage);

        var render = await renderer.RenderAsync(new DesignConversionRenderRequest(
            RenderRequestId: $"{artifactId}-render",
            RequestId: $"{package.RequestId}-{artifactId}",
            Html: package.Html!,
            Css: package.Css!,
            Resolution: new RenderQaResolution(
                $"{resolution.ResolutionId}:physical-png",
                physicalResolution.Value!.PhysicalWidth,
                physicalResolution.Value.PhysicalHeight,
                1d),
            Metadata: new Dictionary<string, string>
            {
                ["artifact_ref_fixture"] = "flow47_configured_ready_json_submit",
                ["artifact_kind"] = kind,
                ["proof_boundary"] = DesignConversionServerOwnedLocalArtifactRefResolver.Boundary
            }));
        Assert.True(render.IsSuccess, render.ErrorMessage);

        var image = render.Value!.Image;
        var png = DesignConversionBrowserPngEvidenceInspector.Inspect(image.Uri);
        Assert.True(png.IsSuccess, png.ErrorMessage);
        var pngEvidence = png.Value!;
        Assert.True(pngEvidence.HasNonBlankPixels);

        var metadata = new Dictionary<string, string>(image.Metadata, StringComparer.Ordinal)
        {
            ["proof_boundary"] = DesignConversionServerOwnedLocalArtifactRefResolver.Boundary,
            ["media_type"] = "image/png",
            ["content_type"] = "image/png",
            ["png_width"] = pngEvidence.PngWidth.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["png_height"] = pngEvidence.PngHeight.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["viewport_id"] = resolution.ResolutionId,
            ["viewport_width"] = resolution.Width.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["viewport_height"] = resolution.Height.ToString(System.Globalization.CultureInfo.InvariantCulture),
            [DesignConversionRenderResolutionFactory.DeviceScaleFactorMetadataKey] = resolution.DeviceScaleFactor.ToString("R", System.Globalization.CultureInfo.InvariantCulture)
        };

        return new ConfiguredReadyJsonServerOwnedArtifactRefFixture(
            new DesignConversionModelServerArtifactRef(
                ArtifactId: artifactId,
                Kind: kind,
                Width: resolution.Width,
                Height: resolution.Height,
                Uri: $"artifact://flow47/ready-json/{artifactId}.png",
                Sha256: image.Sha256,
                Metadata: metadata),
            image.Uri);
    }

    private sealed record ConfiguredReadyJsonSubmitPackageFixture(
        DesignConversionModelServerLocalCallerPackage Package,
        IReadOnlyDictionary<string, string> ServerOwnedArtifactUrisBySha256);

    private sealed record ConfiguredReadyJsonServerOwnedArtifactRefFixture(
        DesignConversionModelServerArtifactRef PublicRef,
        string ServerOwnedFileUri);

    private sealed class ServerOwnedArtifactRefRewriteHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
    {
        private readonly ConcurrentDictionary<string, string> _serverOwnedArtifactUrisBySha256 = new(StringComparer.OrdinalIgnoreCase);

        public void AddMappings(IReadOnlyDictionary<string, string> serverOwnedArtifactUrisBySha256)
        {
            foreach (var mapping in serverOwnedArtifactUrisBySha256)
                _serverOwnedArtifactUrisBySha256[mapping.Key] = mapping.Value;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestPath = request.RequestUri?.AbsolutePath;
            if (request.Content is not null &&
                request.Method == HttpMethod.Post &&
                string.Equals(
                    requestPath,
                    DesignConversionModelServerEndpointRegistry.EndpointPath,
                    StringComparison.Ordinal))
            {
                var json = await request.Content.ReadAsStringAsync(cancellationToken);
                var root = JsonNode.Parse(json)?.AsObject();
                if (root is not null)
                {
                    RewriteArtifactRefs(root, "screenshots");
                    RewriteArtifactRefs(root, "designRenders");
                    request.Content = new StringContent(
                        root.ToJsonString(DesignConversionModelServerHttpHandler.JsonOptions),
                        Encoding.UTF8,
                        "application/json");
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private void RewriteArtifactRefs(JsonObject root, string propertyName)
        {
            if (root[propertyName] is not JsonArray artifacts)
                return;

            foreach (var artifactNode in artifacts)
            {
                if (artifactNode is not JsonObject artifact)
                    continue;
                var sha256 = artifact["sha256"]?.GetValue<string>();
                if (string.IsNullOrWhiteSpace(sha256) ||
                    !_serverOwnedArtifactUrisBySha256.TryGetValue(sha256, out var serverOwnedFileUri))
                {
                    continue;
                }

                artifact["uri"] = serverOwnedFileUri;
            }
        }
    }

    private static IReadOnlyDictionary<string, string> ClientClaimedTrustMetadata(string privateMarker)
        => new Dictionary<string, string>
        {
            ["client_claimed_browser_marker"] = privateMarker,
            ["browser_or_playwright_used"] = bool.TrueString,
            ["visual_fidelity_claimed"] = bool.TrueString,
            ["hidden_determinism_boundary"] = "browser_backed_render_evidence",
            ["provider_kind"] = "client_supplied_browser_provider"
        };

    private static IReadOnlyList<string> ExternalVisualProviderSettingNames()
        =>
        [
            DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName,
            DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName,
            DesignConversionExternalVisualProviderConfigIntake.ModelSettingName,
            DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName,
            DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName,
            DesignConversionExternalVisualProviderConfigIntake.ApiKeySchemeSettingName,
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostPerRequestUsdSettingName,
            DesignConversionExternalVisualProviderConfigIntake.LegacyEndpointSettingName,
            DesignConversionExternalVisualProviderConfigIntake.LegacyProviderSettingName,
            DesignConversionExternalVisualProviderConfigIntake.LegacyModelSettingName,
            DesignConversionExternalVisualProviderConfigIntake.LegacyApiKeySettingName
        ];

    private static IReadOnlyList<string> ModelLifecycleSettingNames()
        =>
        [
            ModelLifecycleCurrentTokenEnvironmentName,
            ModelLifecyclePreviousTokenEnvironmentName,
            ModelLifecyclePreviousTokenExpiresEnvironmentName
        ];

    private static async Task StopProcessAndDeleteTempRootAsync(Process process, string tempRoot)
    {
        try
        {
            await StopProcessAsync(process);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    private static async Task StopProcessAsync(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync().WaitAsync(ServerProcessStopTimeout);
            }
        }
        catch (TimeoutException ex)
        {
            throw new TimeoutException(
                $"Timed out after {ServerProcessStopTimeout.TotalSeconds:0}s waiting for DesignConversionLiveLocalServer process {process.Id} to exit during test cleanup.",
                ex);
        }
        finally
        {
            ServerProcessOutputs.TryRemove(process.Id, out _);
        }
    }

    private static async Task<JsonDocument> WaitForReadyFileAsync(
        Process process,
        string readyFile,
        TimeSpan timeout)
    {
        Exception? lastReadyReadError = null;
        var deadline = DateTimeOffset.UtcNow + timeout;
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (File.Exists(readyFile))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(readyFile, Encoding.UTF8);
                    return JsonDocument.Parse(json);
                }
                catch (Exception ex) when (ex is IOException or JsonException)
                {
                    lastReadyReadError = ex;
                }
            }

            if (process.HasExited)
            {
                throw new InvalidOperationException(
                    $"DesignConversionLiveLocalServer exited before ready file. Exit={process.ExitCode}; {DescribeProcessOutput(process)}");
            }

            await Task.Delay(250);
        }

        throw new TimeoutException(
            "DesignConversionLiveLocalServer did not write ready file within " +
            $"{timeout.TotalSeconds:0}s. ProcessExited={process.HasExited}; " +
            $"ReadyFile={readyFile}; LastReadyReadError={lastReadyReadError?.GetType().Name}: {lastReadyReadError?.Message}; " +
            DescribeProcessOutput(process));
    }

    private static string DescribeProcessOutput(Process process)
        => ServerProcessOutputs.TryGetValue(process.Id, out var output)
            ? output.Describe()
            : "stdout=<capture unavailable>; stderr=<capture unavailable>";

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "tools",
                "DesignConversionLiveLocalServer",
                "DesignConversionLiveLocalServer.csproj");
            if (File.Exists(candidate))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root for DesignConversionLiveLocalServer tests.");
    }

    private sealed class ServerProcessOutputCapture
    {
        private const int MaxLines = 80;
        private readonly ConcurrentQueue<string> stdout = new();
        private readonly ConcurrentQueue<string> stderr = new();

        public void AppendStdout(string? line)
            => Append(stdout, line);

        public void AppendStderr(string? line)
            => Append(stderr, line);

        public string Describe()
            => $"stdout={Snapshot(stdout)}; stderr={Snapshot(stderr)}";

        private static void Append(ConcurrentQueue<string> queue, string? line)
        {
            if (line is null)
                return;

            queue.Enqueue(line);
            while (queue.Count > MaxLines)
                queue.TryDequeue(out _);
        }

        private static string Snapshot(ConcurrentQueue<string> queue)
        {
            var lines = queue.ToArray();
            return lines.Length == 0
                ? "<empty>"
                : string.Join(Environment.NewLine, lines);
        }
    }
}
