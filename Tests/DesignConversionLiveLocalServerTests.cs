// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
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
            AssertPublicHealthReadyStringsDoNotLeakLocalPaths(readyRoot, repoRoot, tempRoot);
            Assert.True(readyRoot.GetProperty("localhostOnly").GetBoolean());
            Assert.False(readyRoot.GetProperty("publicDeploymentVerified").GetBoolean());
            Assert.False(readyRoot.GetProperty("externalProviderCalled").GetBoolean());
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
            AssertPublicHealthReadyStringsDoNotLeakLocalPaths(healthRoot, repoRoot, tempRoot);
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

            using var dpoResponse = await client.GetAsync(
                DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath + "?includeBlocked=true&limit=1");
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

            process1 = StartServerProcess(
                repoRoot,
                projectPath,
                readyFile1,
                artifactDbRoot,
                machineBackedLocal: false);
            using var readyDocument1 = await WaitForReadyFileAsync(process1, readyFile1, TimeSpan.FromSeconds(75));
            var readyRoot1 = readyDocument1.RootElement;
            var readyBaseUrl1 = new Uri(readyRoot1.GetProperty("baseUrl").GetString()!);
            Assert.True(readyRoot1.GetProperty("localArtifactDbConfigured").GetBoolean());
            Assert.False(readyRoot1.GetProperty("productReady").GetBoolean());
            AssertPublicRenderArtifactRootRef(readyRoot1);
            AssertPublicHealthReadyStringsDoNotLeakLocalPaths(readyRoot1, repoRoot, tempRoot);

            using var serverOwnedArtifactRewriteHandler1 = new ServerOwnedArtifactRefRewriteHandler(new HttpClientHandler());
            using var httpClient1 = new HttpClient(serverOwnedArtifactRewriteHandler1)
            {
                BaseAddress = readyBaseUrl1,
                Timeout = LiveBrowserBackedSubmitTimeout
            };
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
        var blockedParent = Path.Combine(tempRoot, "blocked-render-parent");
        var artifactDbRoot = Path.Combine(blockedParent, "artifact-db");
        Directory.CreateDirectory(tempRoot);
        File.WriteAllText(blockedParent, "This file blocks render-artifacts directory creation.", Encoding.UTF8);

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

        using var process = StartServerProcess(repoRoot, projectPath, readyFile, artifactDbRoot);
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

            var packageFixture = await CreateConfiguredReadyJsonSubmitPackageAsync(privateMarker, strictArtifactRoot);
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
            Assert.Equal(bool.TrueString, metadata["flow47_verified"]);
            Assert.Equal(
                DesignConversionModelServerReadyJsonSubmitClient.ConfiguredFlow47SubmitStatus,
                metadata["flow47_submit_status"]);
            Assert.Equal(string.Empty, metadata["flow47_missing_setting_names"]);
            Assert.Equal("ready.json", metadata["flow47_base_url_source"]);
            Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointPath, metadata["flow47_endpoint_path"]);
            Assert.Equal(submitted.RequestUri.ToString(), metadata["flow47_submit_request_uri"]);
            Assert.Equal(bool.TrueString, metadata["flow47_live_local_loopback_verified"]);
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
                Assert.False(responseRoot.GetProperty("modelTrainingVerified").GetBoolean());
                Assert.False(responseRoot.GetProperty("modelUploadReady").GetBoolean());
                Assert.False(responseRoot.GetProperty("productReady").GetBoolean());
                Assert.False(responseRoot.GetProperty("deploymentVerified").GetBoolean());
                Assert.False(responseRoot.GetProperty("publicHttpEndpointVerified").GetBoolean());
                Assert.False(responseRoot.GetProperty("platformIntegrationVerified").GetBoolean());
                Assert.False(responseRoot.GetProperty("trainingReadiness").GetProperty("modelTrainingVerified").GetBoolean());
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
                Assert.Equal(bool.FalseString, responseMetadata.GetProperty("trained_model_authority").GetString());
                Assert.Equal(bool.TrueString, responseMetadata.GetProperty("not_trained_model_authority").GetString());
                Assert.Equal(bool.FalseString, responseMetadata.GetProperty("product_ready").GetString());

                var responseEvidenceRefs = ReadStringArray(responseRoot, "evidenceRefs");
                Assert.Contains(
                    "tier_policy:stop_reason=" + DesignConversionVisualQaBoundaryPolicy.BudgetCapReached,
                    responseEvidenceRefs);
                Assert.DoesNotContain(
                    "tier_policy:stop_reason=" + DesignConversionVisualQaBoundaryPolicy.QualityReached,
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
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, storedRecord.Metadata["tier_policy_stop_reason"]);
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, storedRecord.VisualJudgeStatus.TierDecision);
            Assert.False(storedRecord.VisualJudgeStatus.ExternalCallExecuted);
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker, storedRecord.VisualJudgeStatus.ProviderBoundary);
            Assert.Contains(
                "tier_policy:stop_reason=" + DesignConversionVisualQaBoundaryPolicy.BudgetCapReached,
                storedRecord.VisualJudgeStatus.EvidenceRefs);
            Assert.DoesNotContain(
                "tier_policy:stop_reason=" + DesignConversionVisualQaBoundaryPolicy.QualityReached,
                storedRecord.VisualJudgeStatus.EvidenceRefs);
            Assert.DoesNotContain(
                storedRecord.VisualJudgeStatus.EvidenceRefs,
                reference => reference.Contains(
                    DeterministicDesignConversionComparisonProvider.VisualComparisonNotProvenMarker,
                    StringComparison.Ordinal));
            Assert.Equal(bool.TrueString, storedRecord.Metadata["runtime_created_dpo_candidate"]);
            Assert.Equal("1", storedRecord.Metadata["runtime_created_dpo_candidate_count"]);
            Assert.Equal(bool.TrueString, storedRecord.Metadata["review_required"]);
            Assert.Equal("pending", storedRecord.Metadata["approval_status"]);
            Assert.Equal(bool.FalseString, storedRecord.Metadata["approved_for_training"]);
            Assert.Equal(bool.FalseString, storedRecord.Metadata["production_dpo"]);
            Assert.Equal(bool.FalseString, storedRecord.Metadata["model_readiness_claimed"]);
            var storedCandidate = Assert.Single(storedRecord.DpoCandidates);
            Assert.Equal(DesignConversionDpoUnits.ImproverUnit, storedCandidate.UnitId);
            Assert.True(storedCandidate.TrainingCandidateOnly);
            Assert.True(storedCandidate.ProductNotReady);
            Assert.False(storedCandidate.IsQuarantined);
            Assert.True(storedCandidate.UsefulnessScore > 0);
            Assert.Equal("pending", storedCandidate.TrainingRecord.Provenance.Metadata["approval_status"]);
            Assert.Equal(bool.FalseString, storedCandidate.TrainingRecord.Provenance.Metadata["approved_for_training"]);
            Assert.Equal(bool.FalseString, storedCandidate.TrainingRecord.Provenance.Metadata["production_dpo"]);
            Assert.Equal(bool.FalseString, storedCandidate.TrainingRecord.Provenance.Metadata["model_readiness_claimed"]);
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

            process1 = StartServerProcess(
                repoRoot,
                projectPath,
                readyFile,
                artifactDbRoot,
                routeAdmissionEnabled: true,
                routeAdmissionCurrentToken: RouteAdmissionCurrentToken,
                routeAdmissionPreviousToken: RouteAdmissionPreviousToken,
                routeAdmissionPreviousTokenExpiresAtUtc: RouteAdmissionPreviousTokenExpiresAtUtc,
                routeAdmissionTenantId: RouteAdmissionTenantId);

            using var readyDocument1 = await WaitForReadyFileAsync(process1, readyFile, TimeSpan.FromSeconds(75));
            var readyRoot1 = readyDocument1.RootElement;
            var readyBaseUrl1 = new Uri(readyRoot1.GetProperty("baseUrl").GetString()!);
            AssertReadyJsonBaseUrlForSubmit(readyRoot1, readyBaseUrl1);
            AssertDurableReplayPublicMetadata(readyRoot1, expectedCompletedRecordCount: 0);
            AssertPublicHealthReadyStringsDoNotLeakLocalPaths(readyRoot1, repoRoot, tempRoot);

            using var serverOwnedArtifactRewriteHandler1 = new ServerOwnedArtifactRefRewriteHandler(new HttpClientHandler());
            serverOwnedArtifactRewriteHandler1.AddMappings(packageFixture.ServerOwnedArtifactUrisBySha256);
            using var httpClient1 = new HttpClient(serverOwnedArtifactRewriteHandler1)
            {
                Timeout = LiveBrowserBackedSubmitTimeout
            };

            string healthJson1;
            using (var healthResponse1 = await httpClient1.GetAsync(new Uri(readyBaseUrl1, "/__pass143/health")))
            {
                healthJson1 = await healthResponse1.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, healthResponse1.StatusCode);
                using var healthDocument1 = JsonDocument.Parse(healthJson1);
                AssertDurableReplayPublicMetadata(healthDocument1.RootElement, expectedCompletedRecordCount: 0);
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
                routeAdmissionTenantId: RouteAdmissionTenantId);

            using var readyDocument2 = await WaitForReadyFileAsync(process2, readyFile, TimeSpan.FromSeconds(75));
            var readyRoot2 = readyDocument2.RootElement;
            var readyBaseUrl2 = new Uri(readyRoot2.GetProperty("baseUrl").GetString()!);
            AssertReadyJsonBaseUrlForSubmit(readyRoot2, readyBaseUrl2);
            AssertDurableReplayPublicMetadata(readyRoot2, expectedCompletedRecordCount: 1);
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
                AssertPublicHealthReadyStringsDoNotLeakLocalPaths(healthDocument2.RootElement, repoRoot, tempRoot);
            }

            using (var preReplayStatusResponse = await GetAuthorizedAsync(
                       httpClient2,
                       readyBaseUrl2,
                       DesignConversionModelServerStatusHandler.EndpointPathPrefix + firstEnvelope.ParsedJobId))
            {
                var preReplayStatusJson = await preReplayStatusResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.NotFound, preReplayStatusResponse.StatusCode);
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
    public async Task ExternalVisualProviderFromEnv_WritesFactoryReadyRuntimeBoundaryWithoutSecrets()
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
        var trustedRenderArtifactRoot = Path.Combine(tempRoot, "render-artifacts");
        const string invalidEnvRootMustNotOverrideTrustedRoot = "relative-env-root-must-not-be-used";
        var secret = "pass-runtime-secret-" + Guid.NewGuid().ToString("N");
        var rawPrice = "0.03791";
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
                    "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] =
                    "safe-runtime-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] =
                    "safe-runtime-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret,
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName] = "X-Test-Key",
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostPerRequestUsdSettingName] = rawPrice,
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    invalidEnvRootMustNotOverrideTrustedRoot
            });
        try
        {
            using var readyDocument = await WaitForReadyFileAsync(process, readyFile, TimeSpan.FromSeconds(75));
            var readyRoot = readyDocument.RootElement;
            var readyJson = readyRoot.GetRawText();
            var readyProvider = readyRoot.GetProperty("externalVisualProvider");
            Assert.True(readyRoot.GetProperty("externalVisualProviderConfigComplete").GetBoolean());
            Assert.True(readyRoot.GetProperty("externalVisualProviderFactoryReady").GetBoolean());
            Assert.False(readyRoot.GetProperty("externalVisualProviderNetworkCallExecuted").GetBoolean());
            Assert.False(readyRoot.GetProperty("externalVisualProviderRealProviderExecuted").GetBoolean());
            AssertExternalVisualProviderCompleteBoundary(readyProvider, secret, rawPrice, trustedRenderArtifactRoot);
            Assert.DoesNotContain(secret, readyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(rawPrice, readyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(trustedRenderArtifactRoot, readyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(invalidEnvRootMustNotOverrideTrustedRoot, readyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("/v1/judge", readyJson, StringComparison.Ordinal);

            using var client = new HttpClient
            {
                BaseAddress = new Uri(readyRoot.GetProperty("baseUrl").GetString()!),
                Timeout = TimeSpan.FromSeconds(10)
            };
            using var healthResponse = await client.GetAsync("/__pass143/health");
            var healthJson = await healthResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
            using var healthDocument = JsonDocument.Parse(healthJson);
            var healthProvider = healthDocument.RootElement.GetProperty("externalVisualProvider");
            Assert.True(healthDocument.RootElement.GetProperty("externalVisualProviderConfigComplete").GetBoolean());
            Assert.True(healthDocument.RootElement.GetProperty("externalVisualProviderFactoryReady").GetBoolean());
            Assert.False(healthDocument.RootElement.GetProperty("externalVisualProviderNetworkCallExecuted").GetBoolean());
            Assert.False(healthDocument.RootElement.GetProperty("externalVisualProviderRealProviderExecuted").GetBoolean());
            AssertExternalVisualProviderCompleteBoundary(healthProvider, secret, rawPrice, trustedRenderArtifactRoot);
            Assert.DoesNotContain(secret, healthJson, StringComparison.Ordinal);
            Assert.DoesNotContain(rawPrice, healthJson, StringComparison.Ordinal);
            Assert.DoesNotContain(trustedRenderArtifactRoot, healthJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(invalidEnvRootMustNotOverrideTrustedRoot, healthJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("/v1/judge", healthJson, StringComparison.Ordinal);
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task RequestScopedExternalVisualProvider_InvalidRequestConfigFailsClosedThroughActualLiveLocalRoute()
    {
        const string privateMarker = "PRIVATE_LIVE_LOCAL_REQUEST_SCOPED_INVALID_CONFIG_SECRET";
        const string invalidEndpoint = "http://10.253.0.19:43187/v1/private-judge";
        const string providerName = "live-local-invalid-request-provider";
        const string modelId = "live-local-invalid-request-model";
        const string apiKeyHeaderName = "X-Live-Local-Invalid-Key";
        const string idempotencyKey = "live-local-request-scoped-invalid-provider-config-key";
        var providerSecret = "live-local-invalid-provider-secret-" + Guid.NewGuid().ToString("N");
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
        var providerArtifactRoot = Path.Combine(tempRoot, "request-scoped-provider-artifacts");
        Directory.CreateDirectory(tempRoot);
        Directory.CreateDirectory(providerArtifactRoot);

        using var process = StartServerProcess(
            repoRoot,
            projectPath,
            readyFile,
            artifactDbRoot,
            machineBackedLocal: false);
        try
        {
            using var readyDocument = await WaitForReadyFileAsync(process, readyFile, TimeSpan.FromSeconds(75));
            var readyRoot = readyDocument.RootElement;
            var readyJson = readyRoot.GetRawText();
            AssertRequestScopedExternalVisualProviderLiveLocalPublicFields(readyRoot);
            AssertNoRouteAdmissionSecretLeakage(readyJson, privateMarker, providerSecret, invalidEndpoint, providerArtifactRoot);

            using var client = new HttpClient
            {
                BaseAddress = new Uri(readyRoot.GetProperty("baseUrl").GetString()!),
                Timeout = TimeSpan.FromSeconds(10)
            };

            using var healthResponse = await client.GetAsync("/__pass143/health");
            var healthJson = await healthResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
            using (var healthDocument = JsonDocument.Parse(healthJson))
            {
                AssertRequestScopedExternalVisualProviderLiveLocalPublicFields(healthDocument.RootElement);
                AssertNoRouteAdmissionSecretLeakage(healthJson, privateMarker, providerSecret, invalidEndpoint, providerArtifactRoot);
            }

            var package = CreateConfiguredReadyJsonSubmitPackage(
                privateMarker,
                includeMobileViewport: false,
                requestId: "live-local-request-scoped-invalid-config-package");
            Assert.NotNull(package.Budget);
            var requestScopedMetadata = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var pair in package.Metadata ?? new Dictionary<string, string>())
                requestScopedMetadata[pair.Key] = pair.Value;
            requestScopedMetadata["tenant_id"] = RouteAdmissionTenantId;
            requestScopedMetadata[DesignConversionModelServerApi.ServerAssignedDesignTierMetadataKey] = "free";
            requestScopedMetadata[DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey] = "25";
            requestScopedMetadata[DesignConversionModelServerApi.DesignTierAuthorityMetadataKey] =
                DesignConversionModelServerApi.PlatformTenantConfigurationAuthority;
            requestScopedMetadata[DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey] =
                DesignConversionModelServerApi.PlatformTenantConfigurationAuthority;
            requestScopedMetadata[DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey] =
                DesignConversionModelServerApi.VerifiedInboundPrincipalAuthority;
            var requestScopedPackage = package with
            {
                Budget = package.Budget with
                {
                    UserId = "live-local-request-scoped-user",
                    Tier = "free"
                },
                Metadata = requestScopedMetadata
            };
            var loopRequest = new DesignConversionModelServerImprovementLoopRequest(
                LoopId: "live-local-request-scoped-invalid-config-loop",
                InitialPackage: requestScopedPackage with
                {
                    Budget = requestScopedPackage.Budget with
                    {
                        MaxImprovementCandidates = 1,
                        MaxRenderQaRuns = 1,
                        BudgetUsd = 1.00m
                    }
                },
                MaxAttempts: 1,
                Metadata: new Dictionary<string, string>
                {
                    ["caller_private_marker"] = privateMarker
                },
                ExternalVisualProviderConfig: new DesignConversionExternalVisualProviderConfigInput(
                    Endpoint: invalidEndpoint,
                    ProviderName: providerName,
                    ModelId: modelId,
                    ApiKey: providerSecret,
                    ApiKeyHeaderName: apiKeyHeaderName,
                    ApiKeyScheme: "Bearer",
                    ServerOwnedCostPerRequestUsd: "0.0125",
                    ServerOwnedImageArtifactRoot: providerArtifactRoot));
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

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            AssertNoRouteAdmissionSecretLeakage(
                bodyJson,
                privateMarker,
                requestScopedPackage.Html!,
                requestScopedPackage.Css!,
                requestScopedPackage.DesignTree!.Payload!,
                requestScopedPackage.OriginalTree!.Payload!,
                invalidEndpoint,
                "10.253.0.19",
                "43187",
                "/v1/private-judge",
                providerSecret,
                providerArtifactRoot,
                providerName,
                modelId,
                apiKeyHeaderName,
                idempotencyKey);

            using var document = JsonDocument.Parse(bodyJson);
            var root = document.RootElement;
            Assert.Equal(
                DesignConversionModelServerImprovementLoopHandler.ExternalVisualProviderConfigInvalidErrorCode,
                root.GetProperty("errorCode").GetString());
            Assert.Equal(400, root.GetProperty("statusCode").GetInt32());
            var boundaryMetadata = root.GetProperty("boundaryMetadata");
            AssertRequestScopedExternalVisualProviderRouteBoundaryMetadata(boundaryMetadata);
            Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("runtime_invoked").GetString());
            Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("external_provider_called").GetString());
            Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("external_ai_visual_judge_executed").GetString());
            Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("network_call_executed").GetString());
            Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("provider_payload_included").GetString());
            Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("raw_html_included").GetString());
            Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("raw_css_included").GetString());
            Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("raw_design_tree_included").GetString());
            Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("response_contains_raw_private_data").GetString());
            Assert.False(boundaryMetadata.TryGetProperty("external_visual_provider_endpoint_host", out _));
            Assert.False(boundaryMetadata.TryGetProperty("external_visual_provider_name", out _));
            Assert.False(boundaryMetadata.TryGetProperty("external_visual_provider_model", out _));
            Assert.False(boundaryMetadata.TryGetProperty("external_visual_provider_api_key_header_name", out _));
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ExternalVisualProviderFromEnv_MissingConfigReportsMissingNamesWithoutSecrets()
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
        var secret = "pass-runtime-missing-secret-" + Guid.NewGuid().ToString("N");
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
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret
            });
        try
        {
            using var readyDocument = await WaitForReadyFileAsync(process, readyFile, TimeSpan.FromSeconds(75));
            var readyRoot = readyDocument.RootElement;
            var provider = readyRoot.GetProperty("externalVisualProvider");
            var missing = ReadStringArray(provider, "missing_setting_names");
            var metadata = provider.GetProperty("boundary_metadata");

            Assert.False(readyRoot.GetProperty("externalVisualProviderConfigComplete").GetBoolean());
            Assert.False(readyRoot.GetProperty("externalVisualProviderFactoryReady").GetBoolean());
            Assert.False(provider.GetProperty("config_complete").GetBoolean());
            Assert.False(provider.GetProperty("factory_ready").GetBoolean());
            Assert.False(provider.GetProperty("factory_construction_succeeded").GetBoolean());
            Assert.False(provider.GetProperty("network_call_executed").GetBoolean());
            Assert.False(provider.GetProperty("real_external_provider_executed").GetBoolean());
            Assert.False(provider.GetProperty("api_key_value_included").GetBoolean());
            Assert.False(provider.GetProperty("secret_value_included").GetBoolean());
            Assert.False(provider.GetProperty("provider_payload_included").GetBoolean());
            Assert.Equal("none", provider.GetProperty("selected_provider").GetString());
            Assert.Equal("not_configured", provider.GetProperty("provider_mode").GetString());
            Assert.Equal("fallback_unavailable", provider.GetProperty("provider_fallback_status").GetString());
            Assert.Equal("provider_config_incomplete", provider.GetProperty("provider_fallback_reason").GetString());
            Assert.Equal(
                ["claude", "openai", "gemini"],
                ReadStringArray(provider, "unavailable_provider_names"));
            Assert.Contains(DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName, missing);
            Assert.Contains(DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName, missing);
            Assert.Contains(DesignConversionExternalVisualProviderConfigIntake.ModelSettingName, missing);
            Assert.DoesNotContain(DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName, missing);
            Assert.Equal(bool.TrueString, metadata.GetProperty("external_visual_provider_api_key_configured").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("external_visual_provider_api_key_value_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("secret_value_included").GetString());
            Assert.Equal("none", metadata.GetProperty(DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey).GetString());
            Assert.Equal("not_configured", metadata.GetProperty(DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey).GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty(DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey).GetString());
            Assert.DoesNotContain(secret, readyRoot.GetRawText(), StringComparison.Ordinal);
        }
        finally
        {
            await StopProcessAndDeleteTempRootAsync(process, tempRoot);
        }
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
        IReadOnlyList<string>? additionalArguments = null)
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
            "artifact://pass143/live-local/render-artifacts/root",
            root.GetProperty("renderArtifactRoot").GetString());
        Assert.Equal(
            "artifact://pass143/live-local/render-artifacts/root",
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
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
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

    private static string BuildConfiguredArtifactDbAutowriteEnvelopeDiagnostic(
        DesignConversionModelServerLocalCallerAdapterResponse envelope)
    {
        const int maxSummaryLength = 768;
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

        return summary.Length <= maxSummaryLength
            ? summary.ToString()
            : summary.ToString(0, maxSummaryLength);

        void AppendMetadataField(string key)
        {
            envelope.BoundaryMetadata.TryGetValue(key, out var value);
            AppendField($"metadata.{key}", value);
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
           ContainsWindowsDriveRoot(value);

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
            process.StartInfo.ArgumentList.Add("rev-parse");
            process.StartInfo.ArgumentList.Add("HEAD");
            process.Start();
            if (!process.WaitForExit(5000))
            {
                process.Kill(entireProcessTree: true);
                return null;
            }

            return process.ExitCode == 0
                ? process.StandardOutput.ReadToEnd().Trim().ToLowerInvariant()
                : null;
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException)
        {
            return null;
        }
    }

    private static bool CommitsMatch(string sourceCommit, string expectedCommit)
    {
        if (string.IsNullOrWhiteSpace(sourceCommit) || string.IsNullOrWhiteSpace(expectedCommit))
            return false;

        return sourceCommit.Length == expectedCommit.Length
            ? string.Equals(sourceCommit, expectedCommit, StringComparison.OrdinalIgnoreCase)
            : sourceCommit.Length < expectedCommit.Length
                ? expectedCommit.StartsWith(sourceCommit, StringComparison.OrdinalIgnoreCase)
                : sourceCommit.StartsWith(expectedCommit, StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveServerAssemblyPath(string projectPath)
    {
        var projectDirectory = Path.GetDirectoryName(projectPath)
            ?? throw new DirectoryNotFoundException($"Could not resolve project directory for {projectPath}.");
        var assemblyPath = Path.Combine(
            projectDirectory,
            "bin",
            "Debug",
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

    private static void AssertExternalVisualProviderCompleteBoundary(
        JsonElement provider,
        string secret,
        string rawPrice,
        string trustedRenderArtifactRoot)
    {
        var metadata = provider.GetProperty("boundary_metadata");

        Assert.True(provider.GetProperty("requested").GetBoolean());
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.DescriptorStatusComplete,
            provider.GetProperty("status").GetString());
        Assert.True(provider.GetProperty("config_complete").GetBoolean());
        Assert.True(provider.GetProperty("factory_ready").GetBoolean());
        Assert.True(provider.GetProperty("factory_construction_succeeded").GetBoolean());
        Assert.True(provider.GetProperty("real_external_provider_executable_when_network_authorized").GetBoolean());
        Assert.False(provider.GetProperty("network_call_executed").GetBoolean());
        Assert.False(provider.GetProperty("real_external_provider_executed").GetBoolean());
        Assert.False(provider.GetProperty("api_key_value_included").GetBoolean());
        Assert.False(provider.GetProperty("secret_value_included").GetBoolean());
        Assert.False(provider.GetProperty("provider_payload_included").GetBoolean());
        Assert.Empty(ReadStringArray(provider, "missing_setting_names"));
        Assert.Empty(ReadStringArray(provider, "invalid_setting_names"));
        Assert.Equal("visual-judge.example", provider.GetProperty("endpoint_host").GetString());
        Assert.Equal("safe-runtime-vision-provider", provider.GetProperty("provider_name").GetString());
        Assert.Equal("safe-runtime-vision-model", provider.GetProperty("provider_model").GetString());
        Assert.Equal(DesignConversionVisualAiProviderSelectionPolicy.PolicyName, provider.GetProperty("provider_selection_policy").GetString());
        Assert.Equal("custom", provider.GetProperty("requested_provider").GetString());
        Assert.Equal("custom", provider.GetProperty("selected_provider").GetString());
        Assert.Equal("configured_not_called", provider.GetProperty("selected_provider_status").GetString());
        Assert.Equal("external_configured_not_called", provider.GetProperty("provider_mode").GetString());
        Assert.Equal("not_needed", provider.GetProperty("provider_fallback_status").GetString());
        Assert.Equal("none", provider.GetProperty("provider_fallback_reason").GetString());
        Assert.Equal(
            ["claude", "openai", "gemini"],
            ReadStringArray(provider, "unavailable_provider_names"));
        Assert.Equal("X-Test-Key", provider.GetProperty("api_key_header_name").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("external_visual_provider_config_complete").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("external_visual_provider_factory_ready").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("external_visual_provider_factory_construction_succeeded").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("external_visual_provider_image_content_resolver_configured").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("external_visual_provider_image_artifact_root_value_included").GetString());
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName,
            metadata.GetProperty("external_visual_provider_image_artifact_root_setting_name").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("external_visual_provider_api_key_value_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("secret_value_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("provider_payload_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("network_call_executed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("real_external_provider_executed").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("server_owned_pricing_configured").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("server_owned_pricing_valid").GetString());
        Assert.Equal("configured", metadata.GetProperty("server_owned_pricing_status").GetString());
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            metadata.GetProperty("server_owned_pricing_cost_source").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("server_owned_pricing_raw_value_included").GetString());
        Assert.Equal("custom", metadata.GetProperty(DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey).GetString());
        Assert.Equal("external_configured_not_called", metadata.GetProperty(DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey).GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty(DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey).GetString());

        var serialized = provider.GetRawText();
        Assert.DoesNotContain(secret, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(rawPrice, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(trustedRenderArtifactRoot, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/v1/judge", serialized, StringComparison.Ordinal);
    }

    private static void AssertRequestScopedExternalVisualProviderLiveLocalPublicFields(JsonElement root)
    {
        Assert.True(root.GetProperty("externalVisualProviderRequestScopedResolverConfigured").GetBoolean());
        Assert.True(root.GetProperty("externalVisualProviderSharedHttpClientInjected").GetBoolean());
        Assert.True(root.GetProperty("externalVisualProviderSharedHttpClientOwnedByServerProcess").GetBoolean());
        Assert.False(root.GetProperty("externalVisualProviderSharedHttpClientDisposedByRegistry").GetBoolean());
    }

    private static void AssertRequestScopedExternalVisualProviderRouteBoundaryMetadata(JsonElement metadata)
    {
        Assert.Equal(
            bool.TrueString,
            metadata.GetProperty("external_visual_provider_request_config_supported").GetString());
        Assert.Equal(
            bool.TrueString,
            metadata.GetProperty("external_visual_provider_request_scoped_resolver_configured").GetString());
        Assert.Equal(
            bool.TrueString,
            metadata.GetProperty("external_visual_provider_shared_http_client_injected").GetString());
        Assert.Equal(
            bool.TrueString,
            metadata.GetProperty("external_visual_provider_http_client_owned_by_caller").GetString());
        Assert.Equal(
            bool.FalseString,
            metadata.GetProperty("external_visual_provider_http_client_disposed_by_registry").GetString());
        Assert.False(metadata.TryGetProperty("raw_external_visual_provider_config_in_route_metadata", out _));
    }

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
            Html: $"<main data-secret=\"{privateMarker}\">Flow47 configured ready.json submit</main>",
            Css: $".flow47{{background-image:url('{privateMarker}');}}",
            Screenshots: screenshots,
            DesignRenders: designRenders,
            Budget: new DesignConversionModelServerBudget(
                UserId: canonicalPlatformUserId,
                Tier: canonicalTier,
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: includeMobileViewport ? 2 : 1,
                BudgetUsd: 0m,
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker
                }),
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
        string? requestId = null)
    {
        var package = CreateConfiguredReadyJsonSubmitPackage(
            privateMarker,
            includeMobileViewport,
            requestId);
        var renderer = new DeterministicLocalDesignConversionPngRenderProvider(strictArtifactRoot);
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
            package,
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
                package,
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
        DeterministicLocalDesignConversionPngRenderProvider renderer,
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
