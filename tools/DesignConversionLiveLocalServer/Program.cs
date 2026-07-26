// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using ModelServer.DesignConversion;

Pass143Options options;
try
{
    options = Pass143Options.Parse(args);
}
catch (Pass143ArtifactDbRootUnavailableException ex)
{
    Console.Error.WriteLine(ex.Message);
    Environment.ExitCode = 64;
    return;
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine(ex.Message);
    Environment.ExitCode = 64;
    return;
}

var sourceIdentity = Pass143SourceIdentitySnapshot.Create(options.ExpectedSourceCommit);
using var externalVisualProviderSharedHttpClient =
    DesignConversionExternalVisualJudgeNetwork.CreateSecureHttpClient();
Pass143LiveLocalRuntime runtime;
try
{
    runtime = await Pass143LiveLocalRuntime.CreateAsync(
        options,
        externalVisualProviderSharedHttpClient);
}
catch (Pass143ArtifactDbRootUnavailableException ex)
{
    Console.Error.WriteLine(ex.Message);
    Environment.ExitCode = 64;
    return;
}

if (runtime is null)
    throw new InvalidOperationException("Pass143 live-local runtime creation returned null.");

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = []
});
builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

await using var app = builder.Build();
switch (runtime.RouteProfile)
{
    case Pass143LiveLocalRouteProfile.NonTraining:
        app.ConfigureNonTrainingDesignConversionModelServer(runtime.Registry);
        break;
    case Pass143LiveLocalRouteProfile.NonDurable:
        app.ConfigureNonDurableDesignConversionModelServer(runtime.Registry);
        break;
    case Pass143LiveLocalRouteProfile.Full:
        app.ConfigureDesignConversionModelServer(runtime.Registry);
        break;
    default:
        throw new InvalidOperationException($"Unsupported Pass143 live-local route profile: {runtime.RouteProfile}.");
}
if (runtime.ArtifactUploadHandler is { } artifactUploadHandler)
    app.MapDesignConversionModelServerArtifactUpload(artifactUploadHandler);
app.MapGet("/__pass143/health", () => Results.Json(CreateHealthBody(runtime, sourceIdentity)));

await app.StartAsync();
var baseAddress = ResolveStartedAddress(app);
WriteReadyFile(options.ReadyFile, baseAddress, runtime, sourceIdentity);
Console.WriteLine($"PASS143_SERVER_READY {baseAddress}");
Console.Out.Flush();
await app.WaitForShutdownAsync();

static Uri ResolveStartedAddress(WebApplication app)
{
    var addresses = app.Services
        .GetRequiredService<IServer>()
        .Features
        .Get<IServerAddressesFeature>()?
        .Addresses
        .ToArray() ?? [];
    var address = addresses.Single();
    var uri = new Uri(address);
    if (!IPAddress.IsLoopback(IPAddress.Parse(uri.Host)))
        throw new InvalidOperationException($"Pass143 server must bind loopback only, got {uri.Host}.");

    return uri;
}

static void WriteReadyFile(
    string readyFile,
    Uri baseAddress,
    Pass143LiveLocalRuntime runtime,
    Pass143SourceIdentitySnapshot sourceIdentity)
{
    var directory = Path.GetDirectoryName(readyFile);
    if (!string.IsNullOrWhiteSpace(directory))
        Directory.CreateDirectory(directory);

    var endpointPath = runtime.RoutePaths.Contains(DesignConversionModelServerEndpointRegistry.EndpointPath, StringComparer.Ordinal)
        ? DesignConversionModelServerEndpointRegistry.EndpointPath
        : DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath;
    var body = CreateReadyBody(baseAddress, endpointPath, runtime, sourceIdentity);
    var readyJson = JsonSerializer.Serialize(body, DesignConversionModelServerHttpHandler.JsonOptions);
    var tempReadyFile = readyFile + "." + Guid.NewGuid().ToString("N") + ".tmp";
    try
    {
        File.WriteAllText(
            tempReadyFile,
            readyJson,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        File.Move(tempReadyFile, readyFile, overwrite: true);
    }
    finally
    {
        if (File.Exists(tempReadyFile))
            File.Delete(tempReadyFile);
    }
}

static Dictionary<string, object?> CreateHealthBody(
    Pass143LiveLocalRuntime runtime,
    Pass143SourceIdentitySnapshot sourceIdentity)
{
    var body = new Dictionary<string, object?>
    {
        ["status"] = "ok"
    };
    AddPublicRuntimeFields(body, runtime, sourceIdentity);
    return body;
}

static Dictionary<string, object?> CreateReadyBody(
    Uri baseAddress,
    string endpointPath,
    Pass143LiveLocalRuntime runtime,
    Pass143SourceIdentitySnapshot sourceIdentity)
{
    var body = new Dictionary<string, object?>
    {
        ["baseUrl"] = baseAddress.ToString().TrimEnd('/'),
        ["endpointPath"] = endpointPath
    };
    AddPublicRuntimeFields(body, runtime, sourceIdentity);
    return body;
}

static void AddPublicRuntimeFields(
    IDictionary<string, object?> body,
    Pass143LiveLocalRuntime runtime,
    Pass143SourceIdentitySnapshot sourceIdentity)
{
    var renderArtifactRootRef = PublicRenderArtifactRootRef(runtime);
    body["sourceIdentity"] = sourceIdentity;
    body["machineBackedLocal"] = runtime.MachineBackedLocal;
    body["fullLocalRouteSet"] = runtime.FullLocalRouteSet;
    body["routePaths"] = runtime.RoutePaths;
    foreach (var pair in runtime.Flow38PublicBoundary.ToPublicFields())
        body[pair.Key] = pair.Value;
    body["artifactUploadRouteMapped"] = runtime.ArtifactUploadRouteMapped;
    body["artifactUploadConfigured"] = runtime.ArtifactUploadConfigured;
    body["artifactUploadEndpointPath"] = runtime.ArtifactUploadEndpointPath;
    body["artifactUploadFailClosedAdmissionRequired"] = runtime.ArtifactUploadFailClosedAdmissionRequired;
    body["failClosedAdmissionRequired"] = runtime.ArtifactUploadFailClosedAdmissionRequired;
    body["artifactUploadMaxBytes"] = runtime.ArtifactUploadMaxBytes;
    body["artifactUploadStartupStatus"] = runtime.ArtifactUploadStartupStatus;
    body["artifactUploadNonReadyCode"] = runtime.ArtifactUploadNonReadyCode;
    body["artifactUploadNonReadyReason"] = runtime.ArtifactUploadNonReadyReason;
    body["renderArtifactRootConfigured"] = !string.IsNullOrWhiteSpace(runtime.RenderArtifactRoot);
    body["renderArtifactRoot"] = renderArtifactRootRef;
    body["renderArtifactRootRef"] = renderArtifactRootRef;
    body["renderProviderKind"] = runtime.RenderProviderKind;
    body["browserRendererConfigured"] = runtime.BrowserRendererConfigured;
    body["browserRendererRuntimeAvailable"] = runtime.BrowserRendererRuntimeAvailable;
    body["browserExecutablePath"] = null;
    body["browserExecutable"] = Pass143BrowserExecutablePublicMetadata.Create(
        runtime.BrowserExecutablePath,
        runtime.BrowserRendererConfigured,
        runtime.BrowserRendererRuntimeAvailable);
    body["scaffoldRendererMode"] = runtime.ScaffoldRendererMode;
    body["renderProviderStartupStatus"] = runtime.RenderProviderStartupStatus;
    body["renderProviderNonReadyCode"] = runtime.RenderProviderNonReadyCode;
    body["renderProviderNonReadyReason"] = runtime.RenderProviderNonReadyReason;
    body["visualJudgeProviderKind"] = runtime.ExternalVisualProvider.ProviderKind;
    body["visualJudgeProviderConfigured"] = runtime.ExternalVisualProvider.Configured;
    body["visualJudgeProviderAvailable"] = runtime.ExternalVisualProvider.Available;
    body["machineTrainingRows"] = runtime.MachineTrainingRows;
    body["learnedWeightArtifactCount"] = runtime.LearnedWeightArtifactCount;
    body["machineBackedApiFactoryCaller"] = runtime.MachineBackedApiFactoryCaller;
    body["machineBackedApiRuntimeProbeAttempted"] = runtime.MachineBackedApiRuntimeProbeAttempted;
    body["machineBackedApiRuntimeProbeExecuted"] = runtime.MachineBackedApiRuntimeProbeExecuted;
    body["machineBackedApiLearnedInferenceApplied"] = runtime.MachineBackedApiLearnedInferenceApplied;
    body["machineBackedApiBrowserE2eVerified"] = runtime.MachineBackedApiBrowserE2eVerified;
    body["machineBackedApiBrowserE2eVerificationState"] = runtime.MachineBackedApiBrowserE2eVerificationState;
    body["machineBackedApiBrowserE2eVerificationAuthority"] = runtime.MachineBackedApiBrowserE2eVerificationAuthority;
    body["machineBackedApiBrowserE2eVerifiedRenderArtifactCount"] = runtime.MachineBackedApiBrowserE2eVerifiedRenderArtifactCount;
    body["machineBackedApiBrowserE2eRequiredViewportCount"] = runtime.MachineBackedApiBrowserE2eRequiredViewportCount;
    body["machineBackedApiBrowserE2eObservedViewportCount"] = runtime.MachineBackedApiBrowserE2eObservedViewportCount;
    body["machineBackedApiBrowserE2eFailureReason"] = runtime.MachineBackedApiBrowserE2eFailureReason;
    body["machineBackedApiProductReady"] = runtime.MachineBackedApiProductReady;
    body["machineBackedApiPublicHttpEndpointVerified"] = runtime.MachineBackedApiPublicHttpEndpointVerified;
    body["machineBackedApiStartupStatus"] = runtime.MachineBackedApiStartupStatus;
    body["machineBackedApiNonReadyCode"] = runtime.MachineBackedApiNonReadyCode;
    body["machineBackedApiNonReadyReason"] = runtime.MachineBackedApiNonReadyReason;
    body["machineBackedApiPngProofValid"] = runtime.MachineBackedApiPngProofValid;
    body["machineBackedApiProbeArtifactEvidence"] = runtime.MachineBackedApiProbeArtifactEvidence
        .Select(evidence => evidence.ToPublicJsonObject())
        .ToArray();
    body["localArtifactDbConfigured"] = runtime.LocalArtifactDbConfigured;
    body["externalVisualProvider"] = runtime.ExternalVisualProvider.ToPublicJsonObject();
    body["externalVisualProviderKind"] = runtime.ExternalVisualProvider.ProviderKind;
    body["externalVisualProviderConfigured"] = runtime.ExternalVisualProvider.Configured;
    body["externalVisualProviderAvailable"] = runtime.ExternalVisualProvider.Available;
    body["externalVisualProviderConfigComplete"] = runtime.ExternalVisualProvider.ConfigComplete;
    body["externalVisualProviderFactoryReady"] = runtime.ExternalVisualProvider.FactoryReady;
    body["externalVisualProviderNetworkCallExecuted"] = runtime.ExternalVisualProvider.NetworkCallExecuted;
    body["externalVisualProviderRealProviderExecuted"] = runtime.ExternalVisualProvider.RealExternalProviderExecuted;
    body["externalVisualProviderRequestScopedResolverConfigured"] =
        runtime.ExternalVisualProviderRequestScopedResolverConfigured;
    body["externalVisualProviderSharedHttpClientInjected"] =
        runtime.ExternalVisualProviderSharedHttpClientInjected;
    body["externalVisualProviderSharedHttpClientOwnedByServerProcess"] =
        runtime.ExternalVisualProviderSharedHttpClientOwnedByServerProcess;
    body["externalVisualProviderSharedHttpClientDisposedByRegistry"] =
        runtime.ExternalVisualProviderSharedHttpClientDisposedByRegistry;
    foreach (var pair in runtime.RouteAdmissionPublicMetadata)
        body[pair.Key] = pair.Value;

    body["localhostOnly"] = true;
    body["publicDeploymentVerified"] = false;
    body["externalProviderCalled"] = runtime.ExternalVisualProvider.NetworkCallExecuted;
    body["trainedModelAuthority"] = false;
    body["productReady"] = false;
}

static string PublicRenderArtifactRootRef(Pass143LiveLocalRuntime runtime)
    => string.IsNullOrWhiteSpace(runtime.RenderArtifactRoot)
        ? string.Empty
        : "artifact://pass143/live-local/render-artifacts/root";

sealed record Pass143ArtifactUploadStartup(
    DesignConversionModelServerArtifactUploadHttpHandler? Handler,
    string Status,
    string NonReadyCode,
    string NonReadyReason)
{
    public static Pass143ArtifactUploadStartup NotConfigured { get; } = new(
        Handler: null,
        Status: "not_configured",
        NonReadyCode: "ARTIFACT_UPLOAD_NOT_CONFIGURED",
        NonReadyReason: "DesignConversion artifact upload is not configured for this live-local mode.");

    public static Pass143ArtifactUploadStartup Mapped(
        DesignConversionModelServerArtifactUploadHttpHandler handler)
        => new(
            Handler: handler,
            Status: "mapped",
            NonReadyCode: "none",
            NonReadyReason: "none");

    public static Pass143ArtifactUploadStartup BoundedStorageUnavailable { get; } = new(
        Handler: null,
        Status: "bounded_non_ready",
        NonReadyCode: "DESIGN_CONVERSION_ARTIFACT_UPLOAD_STARTUP_FAILED",
        NonReadyReason: "DesignConversion artifact upload startup could not prepare safe local storage; upload route was not mapped.");
}

sealed class Pass143ArtifactDbRootUnavailableException : Exception
{
    public const string Code = "DESIGN_CONVERSION_LIVE_LOCAL_ARTIFACT_DB_ROOT_UNAVAILABLE";
    public const string SafeMessage =
        Code +
        ": DesignConversion durable local artifact DB startup could not prove local persistence before live-local ready.";

    public Pass143ArtifactDbRootUnavailableException()
        : base(SafeMessage)
    {
    }
}

static class Pass143ArtifactDbRootStartup
{
    public static string NormalizeRootOrThrowUnavailable(string artifactDbRoot)
    {
        try
        {
            if (ContainsUnavailablePathCharacter(artifactDbRoot))
                throw new Pass143ArtifactDbRootUnavailableException();

            return Path.GetFullPath(artifactDbRoot);
        }
        catch (Pass143ArtifactDbRootUnavailableException)
        {
            throw;
        }
        catch (Exception ex) when (IsExpectedUnavailableException(ex))
        {
            throw new Pass143ArtifactDbRootUnavailableException();
        }
    }

    public static bool IsExpectedUnavailableException(Exception ex)
        => ex is IOException or UnauthorizedAccessException or SecurityException or NotSupportedException or
               PathTooLongException or ArgumentException;

    private static bool ContainsUnavailablePathCharacter(string path)
        => path.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ||
           path.Any(character => char.IsControl(character) || character is '"' or '<' or '>' or '|' or '?' or '*');
}

sealed record Pass143Options(
    string ReadyFile,
    bool MachineBackedLocal,
    bool ScaffoldRenderer,
    string? ArtifactDbRoot,
    bool ExternalVisualProviderFromEnv,
    string? ExpectedSourceCommit,
    DesignConversionModelServerRouteAdmissionOptions? RouteAdmissionOptions,
    string? ModelActivationStateRootDirectory,
    string? HumanApprovalStateRootDirectory,
    string VisualCostLedgerRootDirectory,
    string VisualCostBudgetStoreRootDirectory,
    DesignConversionModelServerModelLifecycleTokenKeyRingOptions? ModelLifecycleTokenKeyRingOptions,
    string Flow38QuarantineStateRootDirectory,
    string? Flow38QuarantineBearerToken)
{
    private const string ExpectedSourceCommitEnvironmentName = "DESIGN_CONVERSION_EXPECTED_SOURCE_COMMIT";
    private const string RouteAdmissionEnabledEnvironmentName = "DESIGN_CONVERSION_LIVE_ROUTE_ADMISSION_ENABLED";
    private const string RouteAdmissionCurrentTokenEnvironmentName = "DESIGN_CONVERSION_LIVE_ROUTE_CURRENT_TOKEN";
    private const string RouteAdmissionPreviousTokenEnvironmentName = "DESIGN_CONVERSION_LIVE_ROUTE_PREVIOUS_TOKEN";
    private const string RouteAdmissionPreviousTokenExpiresEnvironmentName = "DESIGN_CONVERSION_LIVE_ROUTE_PREVIOUS_TOKEN_EXPIRES_AT_UTC";
    private const string RouteAdmissionTenantIdEnvironmentName = "DESIGN_CONVERSION_LIVE_ROUTE_TENANT_ID";
    private const string ModelLifecycleCurrentTokenEnvironmentName = "DESIGN_CONVERSION_LIVE_MODEL_LIFECYCLE_CURRENT_TOKEN";
    private const string ModelLifecyclePreviousTokenEnvironmentName = "DESIGN_CONVERSION_LIVE_MODEL_LIFECYCLE_PREVIOUS_TOKEN";
    private const string ModelLifecyclePreviousTokenExpiresEnvironmentName =
        "DESIGN_CONVERSION_LIVE_MODEL_LIFECYCLE_PREVIOUS_TOKEN_EXPIRES_AT_UTC";
    private const string Flow38QuarantineBearerTokenEnvironmentName =
        DesignConversionModelServerFlow38QuarantineHandler.BearerTokenEnvironmentName;
    private const string ArtifactDbRootRequiredCode = "DESIGN_CONVERSION_LIVE_LOCAL_ARTIFACT_DB_ROOT_REQUIRED";

    public static Pass143Options Parse(string[] args)
    {
        var readyFile = ReadValue(args, "--ready-file");
        if (string.IsNullOrWhiteSpace(readyFile))
            throw new ArgumentException("Pass143 live-local server requires --ready-file <path>.");
        var artifactDbRoot = NormalizeArtifactDbRoot(ReadValue(args, "--artifact-db-root"));
        var privateRuntimeDataRoot = ResolvePrivateRuntimeDataRoot(readyFile, artifactDbRoot);
        var modelLifecycleTokenKeyRingOptions = ParseModelLifecycleTokenKeyRingOptions(
            privateRuntimeDataRoot,
            out var modelActivationStateRootDirectory);
        var humanApprovalStateRootDirectory = Path.Combine(privateRuntimeDataRoot, "human-approval");
        var visualCostLedgerRootDirectory = Path.Combine(privateRuntimeDataRoot, "visual-cost-ledger");
        var visualCostBudgetStoreRootDirectory = Path.Combine(privateRuntimeDataRoot, "visual-cost-budget-store");
        if (ReadValue(args, "--browser-executable") is not null)
        {
            throw new ArgumentException(
                "Pass143 live-local server does not accept --browser-executable; Chromium is resolved by server-owned discovery only.");
        }

        var explicitMachineBackedLocal = ReadFlag(args, "--machine-backed-local");
        var noMachineBackedLocal = ReadFlag(args, "--no-machine-backed-local");
        var scaffoldRenderer = ReadFlag(args, "--scaffold-renderer") ||
                               ReadFlag(args, "--deterministic-scaffold-renderer");
        if (explicitMachineBackedLocal && noMachineBackedLocal)
            throw new ArgumentException("Pass143 live-local server cannot combine --machine-backed-local with --no-machine-backed-local.");
        if (explicitMachineBackedLocal && scaffoldRenderer)
            throw new ArgumentException("Pass143 live-local server cannot combine --machine-backed-local with --scaffold-renderer.");

        var machineBackedLocal = explicitMachineBackedLocal ||
                                 (!noMachineBackedLocal && !scaffoldRenderer);
        if (!scaffoldRenderer && string.IsNullOrWhiteSpace(artifactDbRoot))
        {
            throw new ArgumentException(
                ArtifactDbRootRequiredCode +
                ": Pass143 live-local server requires --artifact-db-root <path> for browser-backed or machine-backed live modes; only explicit scaffold renderer mode may run without durable local artifact DB.");
        }

        return new Pass143Options(
            readyFile,
            machineBackedLocal,
            scaffoldRenderer,
            artifactDbRoot,
            ReadFlag(args, "--external-visual-provider-from-env"),
            ReadValue(args, "--expected-source-commit") ??
                Environment.GetEnvironmentVariable(ExpectedSourceCommitEnvironmentName),
            ParseRouteAdmissionOptions(args, privateRuntimeDataRoot),
            modelActivationStateRootDirectory,
            humanApprovalStateRootDirectory,
            visualCostLedgerRootDirectory,
            visualCostBudgetStoreRootDirectory,
            modelLifecycleTokenKeyRingOptions,
            Path.Combine(privateRuntimeDataRoot, "flow38-quarantine"),
            Environment.GetEnvironmentVariable(Flow38QuarantineBearerTokenEnvironmentName));
    }

    private static string? NormalizeArtifactDbRoot(string? artifactDbRoot)
        => string.IsNullOrWhiteSpace(artifactDbRoot)
            ? artifactDbRoot
            : Pass143ArtifactDbRootStartup.NormalizeRootOrThrowUnavailable(artifactDbRoot);

    private static DesignConversionModelServerModelLifecycleTokenKeyRingOptions? ParseModelLifecycleTokenKeyRingOptions(
        string privateRuntimeDataRoot,
        out string? modelActivationStateRootDirectory)
    {
        var currentToken = Environment.GetEnvironmentVariable(ModelLifecycleCurrentTokenEnvironmentName);
        var previousToken = Environment.GetEnvironmentVariable(ModelLifecyclePreviousTokenEnvironmentName);
        var previousExpiresRaw = Environment.GetEnvironmentVariable(ModelLifecyclePreviousTokenExpiresEnvironmentName);
        var hasCurrentToken = !string.IsNullOrWhiteSpace(currentToken);
        var hasPreviousToken = !string.IsNullOrWhiteSpace(previousToken);
        var hasPreviousExpires = !string.IsNullOrWhiteSpace(previousExpiresRaw);

        if (!hasCurrentToken && !hasPreviousToken && !hasPreviousExpires)
        {
            modelActivationStateRootDirectory = null;
            return null;
        }

        DateTimeOffset? previousExpiresAtUtc = null;
        if (hasPreviousExpires)
        {
            if (!DateTimeOffset.TryParse(
                    previousExpiresRaw,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var parsed))
            {
                throw new ArgumentException(
                    $"{ModelLifecyclePreviousTokenExpiresEnvironmentName} must be a valid UTC DateTimeOffset value.");
            }

            previousExpiresAtUtc = parsed.ToUniversalTime();
        }

        modelActivationStateRootDirectory = Path.Combine(privateRuntimeDataRoot, "model-activation");
        return new DesignConversionModelServerModelLifecycleTokenKeyRingOptions(
            currentToken,
            previousToken,
            previousExpiresAtUtc);
    }

    private static DesignConversionModelServerRouteAdmissionOptions? ParseRouteAdmissionOptions(
        string[] args,
        string privateRuntimeDataRoot)
    {
        var enabled = ReadFlag(args, "--route-admission-enabled") ||
                      IsTruthy(Environment.GetEnvironmentVariable(RouteAdmissionEnabledEnvironmentName));
        if (!enabled)
            return null;

        var previousExpiresRaw = ReadValue(args, "--route-admission-previous-token-expires-at-utc") ??
                                 Environment.GetEnvironmentVariable(RouteAdmissionPreviousTokenExpiresEnvironmentName);
        DateTimeOffset? previousExpiresAt = null;
        if (!string.IsNullOrWhiteSpace(previousExpiresRaw) &&
            DateTimeOffset.TryParse(
                previousExpiresRaw,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed))
        {
            previousExpiresAt = parsed;
        }

        return new DesignConversionModelServerRouteAdmissionOptions(
            enabled: true,
            currentToken: ReadValue(args, "--route-admission-current-token") ??
                Environment.GetEnvironmentVariable(RouteAdmissionCurrentTokenEnvironmentName),
            previousToken: ReadValue(args, "--route-admission-previous-token") ??
                Environment.GetEnvironmentVariable(RouteAdmissionPreviousTokenEnvironmentName),
            previousTokenExpiresAtUtc: previousExpiresAt,
            tenantId: ReadValue(args, "--route-admission-tenant-id") ??
                Environment.GetEnvironmentVariable(RouteAdmissionTenantIdEnvironmentName),
            durableIdempotencyStoreRoot: Path.Combine(
                privateRuntimeDataRoot,
                "route-admission-idempotency"));
    }

    private static string ResolvePrivateRuntimeDataRoot(string readyFile, string? artifactDbRoot)
    {
        if (!string.IsNullOrWhiteSpace(artifactDbRoot))
        {
            return Path.Combine(
                Path.GetDirectoryName(artifactDbRoot) ?? artifactDbRoot,
                "runtime-data");
        }

        var fullReadyFile = Path.GetFullPath(readyFile);
        return Path.Combine(
            Path.GetDirectoryName(fullReadyFile) ?? Path.GetTempPath(),
            "runtime-data");
    }

    private static string? ReadValue(string[] args, string name)
    {
        for (var index = 0; index < args.Length - 1; index++)
        {
            if (string.Equals(args[index], name, StringComparison.Ordinal))
                return args[index + 1];
        }

        return null;
    }

    private static bool ReadFlag(string[] args, string name)
        => args.Any(arg => string.Equals(arg, name, StringComparison.Ordinal));

    private static bool IsTruthy(string? value)
        => string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
           string.Equals(value, "1", StringComparison.Ordinal) ||
           string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);
}

sealed record Pass143SourceIdentitySnapshot(
    Pass143AssemblySourceIdentity EntryAssembly,
    Pass143AssemblySourceIdentity ModelServerAssembly,
    Pass143AssemblySourceIdentity DesignConversionDomainAssembly,
    bool IdentityConsistency,
    bool AllSourceCommitsAvailable,
    bool SourceCommitsAligned,
    int AvailableSourceCommitCount,
    bool ExpectedSourceCommitConfigured,
    bool ExpectedSourceCommitValid,
    string ExpectedSourceCommit,
    bool? ExpectedSourceCommitMatches,
    string ExpectedSourceCommitStatus)
{
    public static Pass143SourceIdentitySnapshot Create(string? expectedSourceCommit)
    {
        var entryAssembly = Assembly.GetEntryAssembly() ?? typeof(Pass143SourceIdentitySnapshot).Assembly;
        var assemblies = new[]
        {
            Pass143AssemblySourceIdentity.Create(entryAssembly),
            Pass143AssemblySourceIdentity.Create(typeof(DesignConversionModelServerAspNetHostStartup).Assembly),
            Pass143AssemblySourceIdentity.Create(typeof(DesignConversionLocalServiceComposition).Assembly)
        };
        var commits = assemblies
            .Select(assembly => assembly.SourceCommit)
            .Where(commit => !string.IsNullOrWhiteSpace(commit))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var allSourceCommitsAvailable = assemblies.All(assembly => !string.IsNullOrWhiteSpace(assembly.SourceCommit));
        var sourceCommitsAligned = allSourceCommitsAvailable && commits.Length == 1;
        var expected = Pass143SourceCommit.NormalizeExpected(expectedSourceCommit);
        var expectedMatch = expected.Configured && expected.Valid
            ? Pass143SourceCommit.CommitsMatch(assemblies[0].SourceCommit, expected.Value)
            : (bool?)null;

        return new Pass143SourceIdentitySnapshot(
            EntryAssembly: assemblies[0],
            ModelServerAssembly: assemblies[1],
            DesignConversionDomainAssembly: assemblies[2],
            IdentityConsistency:
                sourceCommitsAligned &&
                (!expected.Configured || (expected.Valid && expectedMatch == true)),
            AllSourceCommitsAvailable: allSourceCommitsAvailable,
            SourceCommitsAligned: sourceCommitsAligned,
            AvailableSourceCommitCount: assemblies.Count(assembly => !string.IsNullOrWhiteSpace(assembly.SourceCommit)),
            ExpectedSourceCommitConfigured: expected.Configured,
            ExpectedSourceCommitValid: expected.Valid,
            ExpectedSourceCommit: expected.Valid ? expected.Value : string.Empty,
            ExpectedSourceCommitMatches: expectedMatch,
            ExpectedSourceCommitStatus: expected.Status);
    }
}

sealed record Pass143AssemblySourceIdentity(
    string Name,
    string Version,
    string InformationalVersion,
    string SourceCommit,
    string Sha256,
    bool Sha256Available)
{
    public static Pass143AssemblySourceIdentity Create(Assembly assembly)
    {
        var name = assembly.GetName();
        var informationalVersion =
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ??
            string.Empty;
        var sourceCommit =
            Pass143SourceCommit.TryReadFromAssemblyMetadata(assembly) ??
            Pass143SourceCommit.TryParseFromInformationalVersion(informationalVersion) ??
            string.Empty;
        var sha256 = TryHashAssemblyFile(assembly, out var shaAvailable);

        return new Pass143AssemblySourceIdentity(
            Name: name.Name ?? string.Empty,
            Version: name.Version?.ToString() ?? string.Empty,
            InformationalVersion: informationalVersion,
            SourceCommit: sourceCommit,
            Sha256: sha256,
            Sha256Available: shaAvailable);
    }

    private static string TryHashAssemblyFile(Assembly assembly, out bool available)
    {
        available = false;
        if (string.IsNullOrWhiteSpace(assembly.Location) || !File.Exists(assembly.Location))
            return string.Empty;

        try
        {
            using var stream = File.OpenRead(assembly.Location);
            available = true;
            return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return string.Empty;
        }
    }
}

static class Pass143SourceCommit
{
    private static readonly string[] MetadataKeys =
    [
        "RepositoryCommit",
        "SourceRevisionId",
        "GitCommit",
        "CommitHash",
        "Revision"
    ];

    public static string? TryReadFromAssemblyMetadata(Assembly assembly)
    {
        foreach (var metadata in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
        {
            if (!MetadataKeys.Contains(metadata.Key, StringComparer.OrdinalIgnoreCase))
                continue;

            var normalized = TryNormalizeCommit(metadata.Value);
            if (normalized is not null)
                return normalized;
        }

        return null;
    }

    public static string? TryParseFromInformationalVersion(string informationalVersion)
    {
        if (string.IsNullOrWhiteSpace(informationalVersion))
            return null;

        var plusIndex = informationalVersion.LastIndexOf('+');
        if (plusIndex < 0 || plusIndex == informationalVersion.Length - 1)
            return null;

        foreach (var candidate in informationalVersion[(plusIndex + 1)..]
            .Split(['.', '-', '_'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var normalized = TryNormalizeCommit(candidate);
            if (normalized is not null)
                return normalized;
        }

        return null;
    }

    public static ExpectedSourceCommit NormalizeExpected(string? expectedSourceCommit)
    {
        if (string.IsNullOrWhiteSpace(expectedSourceCommit))
        {
            return new ExpectedSourceCommit(
                Configured: false,
                Valid: false,
                Value: string.Empty,
                Status: "not_configured");
        }

        var normalized = TryNormalizeCommit(expectedSourceCommit);
        if (normalized is null)
        {
            return new ExpectedSourceCommit(
                Configured: true,
                Valid: false,
                Value: string.Empty,
                Status: "invalid");
        }

        return new ExpectedSourceCommit(
            Configured: true,
            Valid: true,
            Value: normalized,
            Status: "configured_valid");
    }

    public static bool CommitsMatch(string sourceCommit, string expectedCommit)
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

    public sealed record ExpectedSourceCommit(
        bool Configured,
        bool Valid,
        string Value,
        string Status);
}

enum Pass143LiveLocalRouteProfile
{
    NonTraining,
    NonDurable,
    Full
}

sealed record Pass143LiveLocalRuntime(
    DesignConversionModelServerEndpointRegistry Registry,
    Pass143LiveLocalRouteProfile RouteProfile,
    bool MachineBackedLocal,
    bool FullLocalRouteSet,
    IReadOnlyList<string> RoutePaths,
    string RenderArtifactRoot,
    string RenderProviderKind,
    bool BrowserRendererConfigured,
    bool BrowserRendererRuntimeAvailable,
    string? BrowserExecutablePath,
    bool ScaffoldRendererMode,
    string RenderProviderStartupStatus,
    string RenderProviderNonReadyCode,
    string RenderProviderNonReadyReason,
    int MachineTrainingRows,
    int LearnedWeightArtifactCount,
    bool MachineBackedApiFactoryCaller,
    bool MachineBackedApiRuntimeProbeAttempted,
    bool MachineBackedApiRuntimeProbeExecuted,
    bool MachineBackedApiLearnedInferenceApplied,
    bool MachineBackedApiBrowserE2eVerified,
    string MachineBackedApiBrowserE2eVerificationState,
    string MachineBackedApiBrowserE2eVerificationAuthority,
    int MachineBackedApiBrowserE2eVerifiedRenderArtifactCount,
    int MachineBackedApiBrowserE2eRequiredViewportCount,
    int MachineBackedApiBrowserE2eObservedViewportCount,
    string MachineBackedApiBrowserE2eFailureReason,
    bool MachineBackedApiProductReady,
    bool MachineBackedApiPublicHttpEndpointVerified,
    string MachineBackedApiStartupStatus,
    string MachineBackedApiNonReadyCode,
    string MachineBackedApiNonReadyReason,
    IReadOnlyList<Pass143MachineBackedApiProbeArtifactEvidence> MachineBackedApiProbeArtifactEvidence,
    bool LocalArtifactDbConfigured,
    IReadOnlyDictionary<string, object?> RouteAdmissionPublicMetadata,
    Pass143ExternalVisualProviderRuntimeBoundary ExternalVisualProvider,
    Pass143ArtifactUploadStartup ArtifactUploadStartup)
{
    // Allows bounded cold/concurrent Chromium starts without changing retry policy.
    private static readonly TimeSpan ServerOwnedBrowserCaptureTimeout = TimeSpan.FromSeconds(30);

    public DesignConversionFlow38PublicBoundarySnapshot Flow38PublicBoundary
        => DesignConversionFlow38PublicBoundarySnapshot.FromRegistry(Registry);

    public DesignConversionModelServerArtifactUploadHttpHandler? ArtifactUploadHandler => ArtifactUploadStartup.Handler;

    public bool ArtifactUploadRouteMapped => ArtifactUploadHandler is not null;

    public bool ArtifactUploadConfigured => ArtifactUploadHandler is not null;

    public string? ArtifactUploadEndpointPath => ArtifactUploadHandler is null
        ? null
        : DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath;

    public bool ArtifactUploadFailClosedAdmissionRequired => ArtifactUploadHandler is not null;

    public long? ArtifactUploadMaxBytes => ArtifactUploadHandler?.UploadStore.MaxUploadBytes;

    public string ArtifactUploadStartupStatus => ArtifactUploadStartup.Status;

    public string ArtifactUploadNonReadyCode => ArtifactUploadStartup.NonReadyCode;

    public string ArtifactUploadNonReadyReason => ArtifactUploadStartup.NonReadyReason;

    private const string MachineBackedApiProbeRequestId = "pass143-live-local-machine-backed-api-probe";
    private const string MachineBackedApiProbeHtml = "<main class=\"pass143\"><h1>Machine backed local</h1></main>";
    private const string MachineBackedApiProbeCss = ".pass143{display:grid;gap:16px;color:#111827;}";
    private const int MachineBackedApiProbeExpectedArtifactCount = 4;
    private static readonly IReadOnlyList<RenderQaResolution> MachineBackedApiProbeResolutions =
    [
        new("pass143-live-local-desktop", 1440, 900, 1d),
        new("pass143-live-local-mobile", 390, 844, 1d)
    ];
    private static readonly IReadOnlyDictionary<string, string> EmptyBoundaryMetadata =
        new Dictionary<string, string>(StringComparer.Ordinal);

    public bool ExternalVisualProviderRequestScopedResolverConfigured
        => ReadBoolMetadata(
            ImprovementLoopRouteBoundaryMetadata,
            "external_visual_provider_request_scoped_resolver_configured");

    public bool ExternalVisualProviderSharedHttpClientInjected
        => ReadBoolMetadata(
            ImprovementLoopRouteBoundaryMetadata,
            "external_visual_provider_shared_http_client_injected");

    public bool ExternalVisualProviderSharedHttpClientOwnedByServerProcess
        => ExternalVisualProviderSharedHttpClientInjected &&
           ReadBoolMetadata(
               ImprovementLoopRouteBoundaryMetadata,
               "external_visual_provider_http_client_owned_by_caller");

    public bool ExternalVisualProviderSharedHttpClientDisposedByRegistry
        => ReadBoolMetadata(
            ImprovementLoopRouteBoundaryMetadata,
            "external_visual_provider_http_client_disposed_by_registry");

    private IReadOnlyDictionary<string, string> ImprovementLoopRouteBoundaryMetadata
    {
        get
        {
            var resolved = Registry.Resolve(
                DesignConversionModelServerEndpointRegistry.EndpointMethod,
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath);
            return resolved.IsSuccess
                ? resolved.Value!.BoundaryMetadata
                : EmptyBoundaryMetadata;
        }
    }

    public bool MachineBackedApiPngProofValid
        => MachineBackedApiProbeArtifactEvidence.Count == MachineBackedApiProbeExpectedArtifactCount &&
           MachineBackedApiProbeArtifactEvidence.All(evidence =>
               evidence.PngSignatureVerified &&
               evidence.Sha256Verified &&
               evidence.HasNonBlankPixels &&
               evidence.BrowserOrPlaywrightUsed &&
               evidence.VisualFidelityClaimed &&
               string.Equals(
                   evidence.ProviderKind,
                   BrowserBackedDesignConversionRenderProvider.ProviderKind,
                   StringComparison.Ordinal) &&
               string.Equals(
                   evidence.HiddenDeterminismBoundary,
                   "browser_backed_render_evidence",
                   StringComparison.Ordinal) &&
               IsExpectedProbeViewport(evidence));

    public static async Task<Pass143LiveLocalRuntime> CreateAsync(
        Pass143Options options,
        HttpClient externalVisualProviderSharedHttpClient)
    {
        if (externalVisualProviderSharedHttpClient is null)
            throw new ArgumentNullException(nameof(externalVisualProviderSharedHttpClient));

        var routeAdmissionPublicMetadata = DesignConversionModelServerRouteAdmission
            .Create(options.RouteAdmissionOptions, () => DateTimeOffset.UtcNow)
            .PublicMetadata();
        var renderArtifactRoot = ResolveRenderArtifactRoot(options);
        var visualProviderSelection = Pass143VisualProviderSelection.Create(
            options.ExternalVisualProviderFromEnv,
            renderArtifactRoot,
            externalVisualProviderSharedHttpClient);
        var externalVisualProvider = visualProviderSelection.RuntimeBoundary;
        var browserExecutablePath = ResolveBrowserExecutablePath();
        var browserRendererRuntimeAvailable =
            !string.IsNullOrWhiteSpace(browserExecutablePath) &&
            File.Exists(browserExecutablePath);
        var comparisonProvider = visualProviderSelection.ComparisonProvider;
        var visualJudge = visualProviderSelection.VisualJudge;
        var candidateStrategy = new DesignConversionRuntimeImprovementCandidateStrategy();
        var visualCostLedger = new JsonlDesignConversionVisualCostLedger(options.VisualCostLedgerRootDirectory);
        var visualCostBudgetStore =
            new JsonlDesignConversionVisualCostBudgetStore(options.VisualCostBudgetStoreRootDirectory);

        if (options.ScaffoldRenderer)
        {
            var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalPng(
                renderArtifactRoot,
                comparisonProvider,
                visualJudge);
            var registry = DesignConversionModelServerAspNetHostStartup
                .CreateNonTrainingLocalRouteRegistryWithRequestScopedExternalVisualProvider(
                composition,
                candidateStrategy,
                externalVisualProviderSharedHttpClient,
                new JsonlDesignConversionFlow38QuarantineStore(
                    options.Flow38QuarantineStateRootDirectory),
                options.Flow38QuarantineBearerToken);

            return new Pass143LiveLocalRuntime(
                registry,
                RouteProfile: Pass143LiveLocalRouteProfile.NonTraining,
                MachineBackedLocal: false,
                FullLocalRouteSet: false,
                RoutePaths: DesignConversionModelServerAspNetHostStartup.NonTrainingLocalRouteSet(),
                RenderArtifactRoot: renderArtifactRoot,
                RenderProviderKind: DeterministicLocalDesignConversionPngRenderProvider.ProviderKind,
                BrowserRendererConfigured: false,
                BrowserRendererRuntimeAvailable: false,
                BrowserExecutablePath: null,
                ScaffoldRendererMode: true,
                RenderProviderStartupStatus: "explicit_scaffold_mode",
                RenderProviderNonReadyCode: "SCAFFOLD_RENDERER_EXPLICITLY_REQUESTED",
                RenderProviderNonReadyReason: "Explicit scaffold renderer mode was requested; deterministic local PNG output is non-trusted and not approval evidence.",
                MachineTrainingRows: 0,
                LearnedWeightArtifactCount: 0,
                MachineBackedApiFactoryCaller: false,
                MachineBackedApiRuntimeProbeAttempted: false,
                MachineBackedApiRuntimeProbeExecuted: false,
                MachineBackedApiLearnedInferenceApplied: false,
                MachineBackedApiBrowserE2eVerified: false,
                MachineBackedApiBrowserE2eVerificationState: "not_evaluated_machine_backed_local_not_requested",
                MachineBackedApiBrowserE2eVerificationAuthority: "server_owned_model_server_browser_render_receipts",
                MachineBackedApiBrowserE2eVerifiedRenderArtifactCount: 0,
                MachineBackedApiBrowserE2eRequiredViewportCount: 0,
                MachineBackedApiBrowserE2eObservedViewportCount: 0,
                MachineBackedApiBrowserE2eFailureReason: "machine_backed_local_not_requested",
                MachineBackedApiProductReady: false,
                MachineBackedApiPublicHttpEndpointVerified: false,
                MachineBackedApiStartupStatus: "not_requested",
                MachineBackedApiNonReadyCode: "MACHINE_BACKED_LOCAL_NOT_REQUESTED",
                MachineBackedApiNonReadyReason: "Machine-backed local startup probe was not requested.",
                MachineBackedApiProbeArtifactEvidence: [],
                LocalArtifactDbConfigured: false,
                RouteAdmissionPublicMetadata: routeAdmissionPublicMetadata,
                ExternalVisualProvider: externalVisualProvider,
                ArtifactUploadStartup: Pass143ArtifactUploadStartup.NotConfigured);
        }

        if (!options.MachineBackedLocal)
        {
            var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalDefault(
                CreateServerOwnedBrowserAdapter(browserExecutablePath, renderArtifactRoot),
                comparisonProvider,
                visualJudge: visualJudge);
            var defaultArtifactRefResolver = new DesignConversionServerOwnedLocalArtifactRefResolver(renderArtifactRoot);
            var defaultArtifactDb = CreateConfiguredArtifactDb(options);
            var defaultRouteRegistry = DesignConversionModelServerAspNetHostStartup
                .CreateLocalRouteRegistryWithRequestScopedExternalVisualProvider(
                composition,
                candidateStrategy,
                defaultArtifactDb,
                externalVisualProviderSharedHttpClient,
                () => DateTimeOffset.UtcNow,
                activationStateRootDirectory: options.ModelActivationStateRootDirectory,
                modelLifecycleTokenKeyRing: options.ModelLifecycleTokenKeyRingOptions,
                humanApprovalStateRootDirectory: options.HumanApprovalStateRootDirectory,
                artifactRefResolver: defaultArtifactRefResolver,
                routeAdmissionOptions: options.RouteAdmissionOptions,
                visualCostLedger: visualCostLedger,
                visualCostBudgetStore: visualCostBudgetStore,
                flow38QuarantineStore: new JsonlDesignConversionFlow38QuarantineStore(
                    options.Flow38QuarantineStateRootDirectory),
                flow38QuarantineBearerToken: options.Flow38QuarantineBearerToken);
            var defaultArtifactUploadStartup = CreateArtifactUploadStartup(
                defaultRouteRegistry,
                renderArtifactRoot);
            var renderProviderNonReady = browserRendererRuntimeAvailable
                ? Pass143RenderProviderPublicNonReady.None
                : Pass143RenderProviderPublicNonReady.BrowserRuntimeUnavailable;

            return new Pass143LiveLocalRuntime(
                defaultRouteRegistry,
                RouteProfile: defaultArtifactDb is null
                    ? Pass143LiveLocalRouteProfile.NonDurable
                    : Pass143LiveLocalRouteProfile.Full,
                MachineBackedLocal: false,
                FullLocalRouteSet: defaultArtifactDb is not null,
                RoutePaths: BuildLocalRouteSet(
                    defaultArtifactDb is not null,
                    defaultArtifactUploadStartup.Handler is not null),
                RenderArtifactRoot: renderArtifactRoot,
                RenderProviderKind: BrowserBackedDesignConversionRenderProvider.ProviderKind,
                BrowserRendererConfigured: true,
                BrowserRendererRuntimeAvailable: browserRendererRuntimeAvailable,
                BrowserExecutablePath: browserExecutablePath,
                ScaffoldRendererMode: false,
                RenderProviderStartupStatus: browserRendererRuntimeAvailable
                    ? "browser_runtime_available"
                    : "bounded_non_ready",
                RenderProviderNonReadyCode: renderProviderNonReady.Code,
                RenderProviderNonReadyReason: renderProviderNonReady.Reason,
                MachineTrainingRows: 0,
                LearnedWeightArtifactCount: 0,
                MachineBackedApiFactoryCaller: false,
                MachineBackedApiRuntimeProbeAttempted: false,
                MachineBackedApiRuntimeProbeExecuted: false,
                MachineBackedApiLearnedInferenceApplied: false,
                MachineBackedApiBrowserE2eVerified: false,
                MachineBackedApiBrowserE2eVerificationState: "not_evaluated_machine_backed_local_not_requested",
                MachineBackedApiBrowserE2eVerificationAuthority: "server_owned_model_server_browser_render_receipts",
                MachineBackedApiBrowserE2eVerifiedRenderArtifactCount: 0,
                MachineBackedApiBrowserE2eRequiredViewportCount: 0,
                MachineBackedApiBrowserE2eObservedViewportCount: 0,
                MachineBackedApiBrowserE2eFailureReason: "machine_backed_local_not_requested",
                MachineBackedApiProductReady: false,
                MachineBackedApiPublicHttpEndpointVerified: false,
                MachineBackedApiStartupStatus: "not_requested",
                MachineBackedApiNonReadyCode: "MACHINE_BACKED_LOCAL_NOT_REQUESTED",
                MachineBackedApiNonReadyReason: "Machine-backed local startup probe was not requested.",
                MachineBackedApiProbeArtifactEvidence: [],
                LocalArtifactDbConfigured: defaultArtifactDb is not null,
                RouteAdmissionPublicMetadata: routeAdmissionPublicMetadata,
                ExternalVisualProvider: externalVisualProvider,
                ArtifactUploadStartup: defaultArtifactUploadStartup);
        }

        var trainedMachine = await CreateTinyMachineBackedLocalMachineAsync();
        var checkpoint = trainedMachine.ExportCheckpoint();
        if (!checkpoint.IsSuccess)
            throw new InvalidOperationException(checkpoint.ErrorMessage ?? "DesignConversion machine checkpoint export failed.");

        var renderProvider = CreateBrowserBackedRenderProvider(browserExecutablePath, renderArtifactRoot);
        var artifactRefResolver = new DesignConversionServerOwnedLocalArtifactRefResolver(renderArtifactRoot);
        var machineBackedApi = DesignConversionModelServerApi.CreateMachineBackedLocal(
            trainedMachine,
            renderProvider,
            comparisonProvider,
            utcNow: () => DateTimeOffset.UtcNow,
            visualJudge: visualJudge,
            artifactRefResolver: artifactRefResolver,
            visualCostLedger: visualCostLedger,
            visualCostBudgetStore: visualCostBudgetStore);

        var probeArtifacts = browserRendererRuntimeAvailable
            ? await TryMaterializeMachineBackedApiProbeArtifactsAsync(renderProvider)
            : OperationResult<Pass143MachineBackedApiProbeArtifacts>.Fail(
                Pass143MachineBackedApiPublicNonReady.BrowserRuntimeUnavailable.Reason,
                Pass143MachineBackedApiPublicNonReady.BrowserRuntimeUnavailable.Code);
        DesignConversionModelServerApiResult? machineBackedApiProbeValue = null;
        var machineBackedApiProbeAttempted = false;
        var machineBackedApiStartupStatus = "bounded_non_ready";
        var machineBackedApiNonReadyCode = Pass143MachineBackedApiPublicNonReady.PngProofNotReady.Code;
        var machineBackedApiNonReadyReason = Pass143MachineBackedApiPublicNonReady.PngProofNotReady.Reason;

        if (browserRendererRuntimeAvailable && probeArtifacts.IsSuccess)
        {
            OperationResult<DesignConversionModelServerApiResult> machineBackedApiProbe;
            try
            {
                machineBackedApiProbeAttempted = true;
                machineBackedApiProbe = await machineBackedApi.SubmitAndEvaluateAsync(
                    CreateMachineBackedApiProbeRequest(probeArtifacts.Value!));
            }
            catch (Exception ex) when (Pass143MachineBackedApiPublicNonReady.IsBoundedStartupException(ex))
            {
                var nonReady = Pass143MachineBackedApiPublicNonReady.FromProbeException(ex);
                machineBackedApiProbe = OperationResult<DesignConversionModelServerApiResult>.Fail(
                    nonReady.Reason,
                    nonReady.Code);
            }

            if (machineBackedApiProbe.IsSuccess)
            {
                machineBackedApiProbeValue = machineBackedApiProbe.Value!;
                machineBackedApiStartupStatus = machineBackedApiProbeValue.ProductReady
                    ? "probe_completed_product_ready"
                    : "probe_completed_product_not_ready";
                machineBackedApiNonReadyCode = machineBackedApiProbeValue.ProductReady
                    ? "none"
                    : "MACHINE_BACKED_API_PROBE_COMPLETED_PRODUCT_NOT_READY";
                machineBackedApiNonReadyReason = machineBackedApiProbeValue.ProductReady
                    ? "none"
                    : "Machine-backed local startup probe completed, but product readiness remains false.";
            }
            else
            {
                var nonReady = Pass143MachineBackedApiPublicNonReady.FromProbeFailure(
                    machineBackedApiProbe.ErrorCode,
                    machineBackedApiProbe.ErrorMessage);
                machineBackedApiStartupStatus = "bounded_non_ready";
                machineBackedApiNonReadyCode = nonReady.Code;
                machineBackedApiNonReadyReason = nonReady.Reason;
            }
        }
        else
        {
            var nonReady = Pass143MachineBackedApiPublicNonReady.FromPngProofFailure(probeArtifacts.ErrorCode);
            machineBackedApiNonReadyCode = nonReady.Code;
            machineBackedApiNonReadyReason = nonReady.Reason;
        }

        var compositionWithRuntimeFlow = DesignConversionLocalServiceComposition.CreateBrowserBackedMachineLocalDefault(
            CreateServerOwnedBrowserAdapter(browserExecutablePath, renderArtifactRoot),
            trainedMachine,
            comparisonProvider,
            visualJudge: visualJudge);
        var artifactDb = CreateConfiguredArtifactDb(options);
        var fullRouteRegistry = DesignConversionModelServerAspNetHostStartup
            .CreateLocalRouteRegistryWithRequestScopedExternalVisualProvider(
            compositionWithRuntimeFlow,
            candidateStrategy,
            artifactDb,
            externalVisualProviderSharedHttpClient,
            () => DateTimeOffset.UtcNow,
            activationStateRootDirectory: options.ModelActivationStateRootDirectory,
            modelLifecycleTokenKeyRing: options.ModelLifecycleTokenKeyRingOptions,
            humanApprovalStateRootDirectory: options.HumanApprovalStateRootDirectory,
            artifactRefResolver: artifactRefResolver,
            routeAdmissionOptions: options.RouteAdmissionOptions,
            visualCostLedger: visualCostLedger,
            visualCostBudgetStore: visualCostBudgetStore,
            flow38QuarantineStore: new JsonlDesignConversionFlow38QuarantineStore(
                options.Flow38QuarantineStateRootDirectory),
            flow38QuarantineBearerToken: options.Flow38QuarantineBearerToken);
        var artifactUploadStartup = CreateArtifactUploadStartup(
            fullRouteRegistry,
            renderArtifactRoot);

        var observedExternalVisualProvider = externalVisualProvider.WithExecutionEvidence(
            machineBackedApiProbeValue?.BoundaryMetadata);

        return new Pass143LiveLocalRuntime(
            fullRouteRegistry,
            RouteProfile: artifactDb is null
                ? Pass143LiveLocalRouteProfile.NonDurable
                : Pass143LiveLocalRouteProfile.Full,
            MachineBackedLocal: true,
            FullLocalRouteSet: artifactDb is not null,
            RoutePaths: BuildLocalRouteSet(
                artifactDb is not null,
                artifactUploadStartup.Handler is not null),
            RenderArtifactRoot: renderArtifactRoot,
            RenderProviderKind: BrowserBackedDesignConversionRenderProvider.ProviderKind,
            BrowserRendererConfigured: true,
            BrowserRendererRuntimeAvailable: browserRendererRuntimeAvailable,
            BrowserExecutablePath: browserExecutablePath,
            ScaffoldRendererMode: false,
            RenderProviderStartupStatus: browserRendererRuntimeAvailable
                ? "browser_runtime_available"
                : "bounded_non_ready",
            RenderProviderNonReadyCode: browserRendererRuntimeAvailable
                ? Pass143RenderProviderPublicNonReady.None.Code
                : Pass143RenderProviderPublicNonReady.BrowserRuntimeUnavailable.Code,
            RenderProviderNonReadyReason: browserRendererRuntimeAvailable
                ? Pass143RenderProviderPublicNonReady.None.Reason
                : Pass143RenderProviderPublicNonReady.BrowserRuntimeUnavailable.Reason,
            MachineTrainingRows: 1,
            LearnedWeightArtifactCount: DesignConversionMachine.CountRealLearnedWeightArtifacts(checkpoint.Value),
            MachineBackedApiFactoryCaller: true,
            MachineBackedApiRuntimeProbeAttempted: machineBackedApiProbeAttempted,
            MachineBackedApiRuntimeProbeExecuted: machineBackedApiProbeValue is not null,
            MachineBackedApiLearnedInferenceApplied: machineBackedApiProbeValue?.LearnedInferenceApplied ?? false,
            MachineBackedApiBrowserE2eVerified: ReadBoolMetadata(
                machineBackedApiProbeValue?.BoundaryMetadata,
                "browser_e2e_verified"),
            MachineBackedApiBrowserE2eVerificationState: ReadMetadata(
                machineBackedApiProbeValue?.BoundaryMetadata,
                "browser_e2e_verification_state",
                machineBackedApiProbeValue is null
                    ? "not_evaluated_machine_backed_api_probe_not_completed"
                    : "not_verified_by_server_owned_browser_render_receipts"),
            MachineBackedApiBrowserE2eVerificationAuthority: ReadMetadata(
                machineBackedApiProbeValue?.BoundaryMetadata,
                "browser_e2e_verification_authority",
                "server_owned_model_server_browser_render_receipts"),
            MachineBackedApiBrowserE2eVerifiedRenderArtifactCount: ReadIntMetadata(
                machineBackedApiProbeValue?.BoundaryMetadata,
                "browser_e2e_verified_render_artifact_count"),
            MachineBackedApiBrowserE2eRequiredViewportCount: ReadIntMetadata(
                machineBackedApiProbeValue?.BoundaryMetadata,
                "browser_e2e_required_viewport_count"),
            MachineBackedApiBrowserE2eObservedViewportCount: ReadIntMetadata(
                machineBackedApiProbeValue?.BoundaryMetadata,
                "browser_e2e_observed_viewport_count"),
            MachineBackedApiBrowserE2eFailureReason: ReadMetadata(
                machineBackedApiProbeValue?.BoundaryMetadata,
                "browser_e2e_failure_reason",
                machineBackedApiProbeValue is null
                    ? "machine_backed_api_probe_not_completed"
                    : "unknown"),
            MachineBackedApiProductReady: machineBackedApiProbeValue?.ProductReady ?? false,
            MachineBackedApiPublicHttpEndpointVerified: machineBackedApiProbeValue?.PublicHttpEndpointVerified ?? false,
            MachineBackedApiStartupStatus: machineBackedApiStartupStatus,
            MachineBackedApiNonReadyCode: machineBackedApiNonReadyCode,
            MachineBackedApiNonReadyReason: machineBackedApiNonReadyReason,
            MachineBackedApiProbeArtifactEvidence: probeArtifacts.Value?.AllEvidence ?? [],
            LocalArtifactDbConfigured: artifactDb is not null,
            RouteAdmissionPublicMetadata: routeAdmissionPublicMetadata,
            ExternalVisualProvider: observedExternalVisualProvider,
            ArtifactUploadStartup: artifactUploadStartup);
    }

    private static Pass143ArtifactUploadStartup CreateArtifactUploadStartup(
        DesignConversionModelServerEndpointRegistry fullRouteRegistry,
        string renderArtifactRoot)
    {
        var evaluateRoute = fullRouteRegistry.Resolve(
            DesignConversionModelServerEndpointRegistry.EndpointMethod,
            DesignConversionModelServerEndpointRegistry.EndpointPath);
        if (!evaluateRoute.IsSuccess)
        {
            throw new InvalidOperationException(
                "DesignConversion live-local artifact upload startup requires the registered evaluate route before upload can be mapped.");
        }

        var routeAdmission = evaluateRoute.Value!.RouteAdmission;
        if (routeAdmission is null)
        {
            throw new InvalidOperationException(
                "DesignConversion live-local artifact upload startup requires the exact evaluate route admission instance.");
        }

        try
        {
            return Pass143ArtifactUploadStartup.Mapped(
                new DesignConversionModelServerArtifactUploadHttpHandler(
                    new DesignConversionModelServerArtifactUploadStore(renderArtifactRoot),
                    routeAdmission));
        }
        catch (Exception ex) when (IsExpectedArtifactUploadStartupException(ex))
        {
            return Pass143ArtifactUploadStartup.BoundedStorageUnavailable;
        }
    }

    private static IReadOnlyList<string> BuildFullLocalRouteSet(bool includeArtifactUpload)
        => BuildLocalRouteSet(localArtifactDbConfigured: true, includeArtifactUpload);

    private static IReadOnlyList<string> BuildLocalRouteSet(
        bool localArtifactDbConfigured,
        bool includeArtifactUpload)
    {
        var routePaths = localArtifactDbConfigured
            ? DesignConversionModelServerAspNetHostStartup.FullLocalRouteSet().ToList()
            : DesignConversionModelServerAspNetHostStartup.NonDurableLocalRouteSet().ToList();
        if (includeArtifactUpload &&
            !routePaths.Contains(DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, StringComparer.Ordinal))
        {
            routePaths.Add(DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath);
        }

        return routePaths;
    }

    private static JsonlDesignConversionLocalArtifactDb? CreateConfiguredArtifactDb(Pass143Options options)
    {
        if (string.IsNullOrWhiteSpace(options.ArtifactDbRoot))
            return null;

        var durableRoot = EnsureConfiguredArtifactDbRootDurable(options.ArtifactDbRoot);
        return new JsonlDesignConversionLocalArtifactDb(durableRoot);
    }

    private static string EnsureConfiguredArtifactDbRootDurable(string artifactDbRoot)
    {
        string? probePath = null;
        try
        {
            var durableRoot = Pass143ArtifactDbRootStartup.NormalizeRootOrThrowUnavailable(artifactDbRoot);
            Directory.CreateDirectory(durableRoot);
            probePath = Path.Combine(
                durableRoot,
                "design-conversion-artifact-db-startup-probe-" + Guid.NewGuid().ToString("N") + ".tmp");

            using (var stream = new FileStream(
                       probePath,
                       FileMode.CreateNew,
                       FileAccess.ReadWrite,
                       FileShare.None,
                       bufferSize: 64,
                       FileOptions.WriteThrough))
            {
                var probeBytes = Encoding.UTF8.GetBytes("design-conversion-artifact-db-startup-probe\n");
                stream.Write(probeBytes, 0, probeBytes.Length);
                stream.Flush(flushToDisk: true);
            }

            File.Delete(probePath);
            if (File.Exists(probePath))
                throw new Pass143ArtifactDbRootUnavailableException();

            return durableRoot;
        }
        catch (Pass143ArtifactDbRootUnavailableException)
        {
            TryDeleteArtifactDbStartupProbe(probePath);
            throw;
        }
        catch (Exception ex) when (Pass143ArtifactDbRootStartup.IsExpectedUnavailableException(ex))
        {
            TryDeleteArtifactDbStartupProbe(probePath);
            throw new Pass143ArtifactDbRootUnavailableException();
        }
    }

    private static void TryDeleteArtifactDbStartupProbe(string? probePath)
    {
        if (string.IsNullOrWhiteSpace(probePath))
            return;

        try
        {
            if (File.Exists(probePath))
                File.Delete(probePath);
        }
        catch (Exception ex) when (Pass143ArtifactDbRootStartup.IsExpectedUnavailableException(ex))
        {
        }
    }

    private static bool IsExpectedArtifactUploadStartupException(Exception ex)
        => ex is IOException or UnauthorizedAccessException or NotSupportedException or PathTooLongException ||
           ex is ArgumentException argumentException &&
           string.Equals(argumentException.ParamName, "artifactRoot", StringComparison.Ordinal);

    private static BrowserBackedDesignConversionRenderProvider CreateBrowserBackedRenderProvider(
        string? browserExecutablePath,
        string renderArtifactRoot)
        => new(CreateServerOwnedBrowserAdapter(browserExecutablePath, renderArtifactRoot));

    private static LocalChromiumBrowserProcessAdapter CreateServerOwnedBrowserAdapter(
        string? browserExecutablePath,
        string renderArtifactRoot)
        => string.IsNullOrWhiteSpace(browserExecutablePath)
            ? LocalChromiumBrowserProcessAdapter.CreateServerOwnedDefault(
                ServerOwnedBrowserCaptureTimeout,
                renderArtifactRoot)
            : new LocalChromiumBrowserProcessAdapter(
                browserExecutablePath,
                ServerOwnedBrowserCaptureTimeout,
                renderArtifactRoot);

    private static int ReadIntMetadata(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null &&
           metadata.TryGetValue(key, out var value) &&
           int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;

    private static string ReadMetadata(
        IReadOnlyDictionary<string, string>? metadata,
        string key,
        string fallback)
        => metadata is not null &&
           metadata.TryGetValue(key, out var value) &&
           !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;

    private static string? ResolveBrowserExecutablePath()
        => LocalChromiumBrowserProcessAdapter.TryResolveServerOwnedDefaultExecutablePath();

    private static string ResolveRenderArtifactRoot(Pass143Options options)
    {
        if (!string.IsNullOrWhiteSpace(options.ArtifactDbRoot))
        {
            return Path.Combine(
                Path.GetDirectoryName(options.ArtifactDbRoot) ?? options.ArtifactDbRoot,
                "render-artifacts");
        }

        return Path.Combine(
            Path.GetTempPath(),
            "design-conversion-live-local-server",
            $"render-artifacts-{Guid.NewGuid():N}");
    }

    private static async Task<OperationResult<Pass143MachineBackedApiProbeArtifacts>> TryMaterializeMachineBackedApiProbeArtifactsAsync(
        IDesignConversionRenderProvider renderProvider)
    {
        try
        {
            var screenshots = new List<Pass143MachineBackedApiProbeArtifactEvidence>();
            var designRenders = new List<Pass143MachineBackedApiProbeArtifactEvidence>();
            foreach (var resolution in MachineBackedApiProbeResolutions)
            {
                var screenshot = await MaterializeMachineBackedApiProbeArtifactAsync(
                    renderProvider,
                    artifactId: $"pass143-live-local-screenshot-{resolution.ResolutionId}",
                    kind: "screenshot",
                    renderRequestId: $"pass143-live-local-screenshot-proof-{resolution.ResolutionId}",
                    resolution);
                if (!screenshot.IsSuccess)
                    return OperationResult<Pass143MachineBackedApiProbeArtifacts>.Fail(
                        screenshot.ErrorMessage,
                        screenshot.ErrorCode);

                screenshots.Add(screenshot.Value!);

                var designRender = await MaterializeMachineBackedApiProbeArtifactAsync(
                    renderProvider,
                    artifactId: $"pass143-live-local-design-render-{resolution.ResolutionId}",
                    kind: "design-render",
                    renderRequestId: $"pass143-live-local-design-render-proof-{resolution.ResolutionId}",
                    resolution);
                if (!designRender.IsSuccess)
                    return OperationResult<Pass143MachineBackedApiProbeArtifacts>.Fail(
                        designRender.ErrorMessage,
                        designRender.ErrorCode);

                designRenders.Add(designRender.Value!);
            }

            return OperationResult<Pass143MachineBackedApiProbeArtifacts>.Ok(
                new Pass143MachineBackedApiProbeArtifacts(
                    screenshots,
                    designRenders));
        }
        catch (Exception ex) when (Pass143MachineBackedApiPublicNonReady.IsBoundedStartupException(ex))
        {
            return OperationResult<Pass143MachineBackedApiProbeArtifacts>.Fail(
                Pass143MachineBackedApiPublicNonReady.PngMaterializationFailed.Reason,
                Pass143MachineBackedApiPublicNonReady.PngMaterializationFailed.Code);
        }
    }

    private static async Task<OperationResult<Pass143MachineBackedApiProbeArtifactEvidence>> MaterializeMachineBackedApiProbeArtifactAsync(
        IDesignConversionRenderProvider renderProvider,
        string artifactId,
        string kind,
        string renderRequestId,
        RenderQaResolution resolution)
    {
        var render = await renderProvider.RenderAsync(new DesignConversionRenderRequest(
            RenderRequestId: renderRequestId,
            RequestId: MachineBackedApiProbeRequestId,
            Html: MachineBackedApiProbeHtml,
            Css: MachineBackedApiProbeCss,
            Resolution: resolution,
            Metadata: new Dictionary<string, string>
            {
                ["runtime_caller"] = "DesignConversionLiveLocalServer",
                ["machine_backed_local_probe"] = bool.TrueString,
                ["png_proof_materialized_before_probe"] = bool.TrueString,
                ["product_ready"] = bool.FalseString
            }));
        if (!render.IsSuccess)
            return OperationResult<Pass143MachineBackedApiProbeArtifactEvidence>.Fail(
                "DesignConversion machine-backed local PNG proof materialization failed without crashing startup.",
                string.IsNullOrWhiteSpace(render.ErrorCode)
                    ? "MACHINE_BACKED_API_PNG_MATERIALIZATION_FAILED"
                    : render.ErrorCode);

        var image = render.Value!.Image;
        artifactId = RequireSafeLogicalToken(artifactId, nameof(artifactId));
        kind = RequireSafeLogicalToken(kind, nameof(kind));
        var viewportId = RequireSafeLogicalToken(resolution.ResolutionId, nameof(resolution.ResolutionId));
        var pngEvidence = DesignConversionBrowserPngEvidenceInspector.Inspect(image.Uri);
        if (!pngEvidence.IsSuccess)
            return OperationResult<Pass143MachineBackedApiProbeArtifactEvidence>.Fail(
                "DesignConversion machine-backed local PNG proof inspection failed without exposing local paths.",
                string.IsNullOrWhiteSpace(pngEvidence.ErrorCode)
                    ? "MACHINE_BACKED_API_PNG_INSPECTION_FAILED"
                    : pngEvidence.ErrorCode);

        if (pngEvidence.Value!.PngWidth != resolution.Width || pngEvidence.Value.PngHeight != resolution.Height)
            return OperationResult<Pass143MachineBackedApiProbeArtifactEvidence>.Fail(
                "DesignConversion machine-backed local PNG proof dimensions did not match the probe viewport.",
                "MACHINE_BACKED_API_PNG_DIMENSIONS_MISMATCH");

        if (!pngEvidence.Value.HasNonBlankPixels)
            return OperationResult<Pass143MachineBackedApiProbeArtifactEvidence>.Fail(
                "DesignConversion machine-backed local PNG proof was blank.",
                "MACHINE_BACKED_API_PNG_BLANK");

        var declaredSha = NormalizeSha256(image.Sha256);
        var actualSha = await Sha256FileAsync(pngEvidence.Value.LocalPath);
        if (!string.Equals(declaredSha, actualSha, StringComparison.OrdinalIgnoreCase))
            return OperationResult<Pass143MachineBackedApiProbeArtifactEvidence>.Fail(
                "DesignConversion machine-backed local PNG proof sha256 did not match the local file bytes.",
                "MACHINE_BACKED_API_PNG_SHA256_MISMATCH");

        return OperationResult<Pass143MachineBackedApiProbeArtifactEvidence>.Ok(
            new Pass143MachineBackedApiProbeArtifactEvidence(
                ArtifactId: artifactId,
                Kind: kind,
                ViewportId: viewportId,
                Width: pngEvidence.Value.PngWidth,
                Height: pngEvidence.Value.PngHeight,
                Uri: image.Uri,
                Sha256: $"sha256:{actualSha}",
                LocalPath: pngEvidence.Value.LocalPath,
                ProviderKind: ReadMetadata(image.Metadata, "provider_kind"),
                BrowserOrPlaywrightUsed: ReadBoolMetadata(image.Metadata, "browser_or_playwright_used"),
                VisualFidelityClaimed: ReadBoolMetadata(image.Metadata, "visual_fidelity_claimed"),
                HiddenDeterminismBoundary: ReadMetadata(image.Metadata, "hidden_determinism_boundary"),
                BrowserScreenshotFileVerified: ReadBoolMetadata(image.Metadata, "browser_screenshot_file_verified"),
                BrowserScreenshotSha256Verified: ReadBoolMetadata(image.Metadata, "browser_screenshot_sha256_verified"),
                BrowserScreenshotNonBlankPixelsVerified: ReadBoolMetadata(image.Metadata, "browser_screenshot_nonblank_pixels_verified"),
                ByteLength: pngEvidence.Value.ByteLength,
                PngSignatureVerified: true,
                Sha256Verified: true,
                HasNonBlankPixels: pngEvidence.Value.HasNonBlankPixels,
                NonWhitePixelCount: pngEvidence.Value.NonWhitePixelCount,
                DistinctColorCount: pngEvidence.Value.DistinctColorCount));
    }

    private static string RequireSafeLogicalToken(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > 96 ||
            value.Any(character => !IsSafeLogicalTokenCharacter(character)))
        {
            throw new InvalidOperationException(
                $"Pass143 machine-backed probe {name} must be a server-derived safe logical id.");
        }

        return value;
    }

    private static bool IsSafeLogicalTokenCharacter(char character)
        => character is >= 'a' and <= 'z' ||
           character is >= 'A' and <= 'Z' ||
           character is >= '0' and <= '9' ||
           character is '-' or '_';

    private static async Task<string> Sha256FileAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        return Convert.ToHexString(await SHA256.HashDataAsync(stream)).ToLowerInvariant();
    }

    private static string NormalizeSha256(string value)
    {
        var trimmed = value.Trim();
        return trimmed.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)
            ? trimmed["sha256:".Length..]
            : trimmed;
    }

    private static string ReadMetadata(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) ? value : string.Empty;

    private static bool ReadBoolMetadata(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null &&
           metadata.TryGetValue(key, out var value) &&
           (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "1", StringComparison.Ordinal) ||
            string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase));

    private static DesignConversionModelServerApiRequest CreateMachineBackedApiProbeRequest(
        Pass143MachineBackedApiProbeArtifacts artifacts)
        => new(
            RequestId: MachineBackedApiProbeRequestId,
            SourceKind: DesignConversionSourceKind.Figma,
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: "pass143-live-local-design-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:pass143-live-local-design-tree",
                Payload: "{\"type\":\"FRAME\",\"name\":\"pass143\"}",
                Format: "figma-json"),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: "pass143-live-local-original-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:pass143-live-local-original-tree",
                Payload: "{\"type\":\"FRAME\",\"name\":\"pass143-original\"}",
                Format: "figma-json"),
            Html: MachineBackedApiProbeHtml,
            Css: MachineBackedApiProbeCss,
            Screenshots: artifacts.Screenshots
                .Select(artifact => artifact.ToArtifactRef())
                .ToArray(),
            DesignRenders: artifacts.DesignRenders
                .Select(artifact => artifact.ToArtifactRef())
                .ToArray(),
            Budget: new DesignConversionModelServerBudget(
                UserId: "local-user-redacted",
                Tier: "local-runtime-probe",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: MachineBackedApiProbeResolutions.Count,
                BudgetUsd: 0m),
            Metadata: new Dictionary<string, string>
            {
                ["runtime_caller"] = "DesignConversionLiveLocalServer",
                ["machine_backed_local_probe"] = bool.TrueString,
                ["external_provider_called"] = bool.FalseString,
                ["product_ready"] = bool.FalseString
            });

    private static async Task<DesignConversionMachine> CreateTinyMachineBackedLocalMachineAsync()
    {
        var machine = new DesignConversionMachine();
        var training = await machine.TrainBatchAsync(new LearningBatch<DesignConversionDomain.DesignConversionDomain>(
            BatchId: "pass143-live-local-machine-backed-batch",
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: "pass143-live-local-machine-backed-critic",
                    Prompt: JsonSerializer.Serialize(new Dictionary<string, object>
                    {
                        ["unit_id"] = DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                        ["source_kind"] = nameof(DesignConversionSourceKind.Figma),
                        ["source_manifest_hash"] = "pass143-live-local-source-manifest",
                        ["source_identity_hash"] = "pass143-live-local-source-identity",
                        ["source_structure_hash"] = "pass143-live-local-source-structure",
                        ["pair_family"] = "visual"
                    }),
                    ChosenResponse: JsonSerializer.Serialize(new Dictionary<string, object>
                    {
                        ["visual_score"] = 0.97,
                        ["accepted"] = true
                    }),
                    RejectedResponse: JsonSerializer.Serialize(new Dictionary<string, object>
                    {
                        ["visual_score"] = 0.11,
                        ["accepted"] = false
                    }))
            ],
            SourceIds: ["pass143-live-local-source-disjoint"]));
        if (!training.IsSuccess)
            throw new InvalidOperationException(training.ErrorMessage ?? "DesignConversion tiny local machine training failed.");

        return machine;
    }

    private static bool IsExpectedProbeViewport(Pass143MachineBackedApiProbeArtifactEvidence evidence)
        => MachineBackedApiProbeResolutions.Any(resolution =>
            string.Equals(evidence.ViewportId, resolution.ResolutionId, StringComparison.Ordinal) &&
            evidence.Width == resolution.Width &&
            evidence.Height == resolution.Height);
}

sealed record Pass143RenderProviderPublicNonReady(string Code, string Reason)
{
    public static readonly Pass143RenderProviderPublicNonReady None = new(
        "none",
        "none");

    public static readonly Pass143RenderProviderPublicNonReady BrowserRuntimeUnavailable = new(
        "BROWSER_RUNTIME_UNAVAILABLE",
        "No Chromium-family browser executable is available through server-owned discovery.");
}

sealed record Pass143MachineBackedApiPublicNonReady(string Code, string Reason)
{
    public static readonly Pass143MachineBackedApiPublicNonReady BrowserRuntimeUnavailable = new(
        "BROWSER_RUNTIME_UNAVAILABLE",
        "No Chromium-family browser executable is available for the machine-backed local startup probe.");

    public static readonly Pass143MachineBackedApiPublicNonReady PngProofNotReady = new(
        "MACHINE_BACKED_API_PNG_PROOF_NOT_READY",
        "DesignConversion machine-backed local startup could not materialize verified PNG proof.");

    public static readonly Pass143MachineBackedApiPublicNonReady PngMaterializationFailed = new(
        "MACHINE_BACKED_API_PNG_MATERIALIZATION_FAILED",
        "DesignConversion machine-backed local PNG proof materialization failed without crashing startup.");

    private static readonly Pass143MachineBackedApiPublicNonReady PngInspectionFailed = new(
        "MACHINE_BACKED_API_PNG_INSPECTION_FAILED",
        "DesignConversion machine-backed local PNG proof inspection failed without exposing local paths.");

    private static readonly Pass143MachineBackedApiPublicNonReady PngDimensionsMismatch = new(
        "MACHINE_BACKED_API_PNG_DIMENSIONS_MISMATCH",
        "DesignConversion machine-backed local PNG proof dimensions did not match the probe viewport.");

    private static readonly Pass143MachineBackedApiPublicNonReady PngBlank = new(
        "MACHINE_BACKED_API_PNG_BLANK",
        "DesignConversion machine-backed local PNG proof was blank.");

    private static readonly Pass143MachineBackedApiPublicNonReady PngSha256Mismatch = new(
        "MACHINE_BACKED_API_PNG_SHA256_MISMATCH",
        "DesignConversion machine-backed local PNG proof sha256 did not match the local file bytes.");

    private static readonly Pass143MachineBackedApiPublicNonReady ProbeFailed = new(
        "MACHINE_BACKED_API_PROBE_FAILED",
        "DesignConversion machine-backed local model-server API probe failed without crashing startup.");

    public static bool IsBoundedStartupException(Exception exception)
        => exception is not OperationCanceledException;

    public static Pass143MachineBackedApiPublicNonReady FromPngProofFailure(string? errorCode)
    {
        var code = SafeCodeOrDefault(errorCode, PngMaterializationFailed.Code);
        return code switch
        {
            "BROWSER_RUNTIME_UNAVAILABLE" => BrowserRuntimeUnavailable,
            "MACHINE_BACKED_API_PNG_INSPECTION_FAILED" => PngInspectionFailed,
            "MACHINE_BACKED_API_PNG_DIMENSIONS_MISMATCH" => PngDimensionsMismatch,
            "MACHINE_BACKED_API_PNG_BLANK" => PngBlank,
            "MACHINE_BACKED_API_PNG_SHA256_MISMATCH" => PngSha256Mismatch,
            _ => PngMaterializationFailed
        };
    }

    public static Pass143MachineBackedApiPublicNonReady FromProbeException(Exception exception)
    {
        var safeType = SafeExceptionType(exception);
        var reason = string.IsNullOrWhiteSpace(safeType)
            ? ProbeFailed.Reason
            : $"DesignConversion machine-backed local model-server API probe failed without crashing startup ({safeType}).";
        return new Pass143MachineBackedApiPublicNonReady("MACHINE_BACKED_API_PROBE_EXCEPTION", reason);
    }

    public static Pass143MachineBackedApiPublicNonReady FromProbeFailure(string? errorCode, string? errorMessage)
    {
        var code = SafeCodeOrDefault(errorCode, ProbeFailed.Code);
        if (string.Equals(code, "MACHINE_BACKED_API_PROBE_EXCEPTION", StringComparison.Ordinal) &&
            IsSafeBoundedProbeExceptionMessage(errorMessage))
        {
            return new Pass143MachineBackedApiPublicNonReady(code, errorMessage!);
        }

        return new Pass143MachineBackedApiPublicNonReady(code, ProbeFailed.Reason);
    }

    private static string SafeCodeOrDefault(string? errorCode, string fallback)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
            return fallback;

        var trimmed = errorCode.Trim();
        return trimmed.Length <= 80 &&
               trimmed.All(character => character is >= 'A' and <= 'Z' || character is >= '0' and <= '9' || character is '_')
            ? trimmed
            : fallback;
    }

    private static bool IsSafeBoundedProbeExceptionMessage(string? message)
        => !string.IsNullOrWhiteSpace(message) &&
           message.Length <= 160 &&
           message.StartsWith(
               "DesignConversion machine-backed local model-server API probe failed without crashing startup",
               StringComparison.Ordinal) &&
           !LooksLikeLocalPath(message);

    private static string SafeExceptionType(Exception exception)
        => exception.GetType().Name switch
        {
            nameof(IOException) => nameof(IOException),
            nameof(UnauthorizedAccessException) => nameof(UnauthorizedAccessException),
            nameof(InvalidOperationException) => nameof(InvalidOperationException),
            nameof(NotSupportedException) => nameof(NotSupportedException),
            nameof(FileNotFoundException) => nameof(FileNotFoundException),
            nameof(DirectoryNotFoundException) => nameof(DirectoryNotFoundException),
            nameof(ArgumentException) => nameof(ArgumentException),
            nameof(JsonException) => nameof(JsonException),
            _ => string.Empty
        };

    private static bool LooksLikeLocalPath(string value)
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
}

sealed record Pass143BrowserExecutablePublicMetadata(
    bool Configured,
    bool RuntimeAvailable,
    bool Trusted,
    string Capability,
    string Name,
    string Version,
    string Sha256,
    bool Sha256Available)
{
    public static Pass143BrowserExecutablePublicMetadata Create(
        string? executablePath,
        bool configured,
        bool runtimeAvailable)
    {
        var sha256Available = false;
        var sha256 = runtimeAvailable
            ? TryHashExecutable(executablePath, out sha256Available)
            : string.Empty;
        var name = string.Empty;
        var version = runtimeAvailable
            ? TryReadVersionInfo(executablePath, out name)
            : string.Empty;

        return new Pass143BrowserExecutablePublicMetadata(
            Configured: configured,
            RuntimeAvailable: runtimeAvailable,
            Trusted: runtimeAvailable && sha256Available,
            Capability: configured ? "chromium-family-browser" : string.Empty,
            Name: runtimeAvailable ? name : string.Empty,
            Version: version,
            Sha256: sha256Available ? $"sha256:{sha256}" : string.Empty,
            Sha256Available: sha256Available);
    }

    private static string TryHashExecutable(string? executablePath, out bool available)
    {
        available = false;
        if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
            return string.Empty;

        try
        {
            using var stream = File.OpenRead(executablePath);
            available = true;
            return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return string.Empty;
        }
    }

    private static string TryReadVersionInfo(string? executablePath, out string name)
    {
        name = "chromium-family-browser";
        if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
            return string.Empty;

        try
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(executablePath);
            name = SafeBrowserMetadataText(versionInfo.ProductName) ??
                   SafeBrowserMetadataText(versionInfo.FileDescription) ??
                   "chromium-family-browser";
            return SafeBrowserMetadataText(versionInfo.ProductVersion) ??
                   SafeBrowserMetadataText(versionInfo.FileVersion) ??
                   string.Empty;
        }
        catch (Exception ex) when (ex is ArgumentException or FileNotFoundException)
        {
            return string.Empty;
        }
    }

    private static string? SafeBrowserMetadataText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        if (LooksLikeLocalPath(trimmed))
            return null;

        var safe = new string(trimmed
            .Where(character =>
                character is >= 'a' and <= 'z' ||
                character is >= 'A' and <= 'Z' ||
                character is >= '0' and <= '9' ||
                character is ' ' or '.' or '-' or '_' or '(' or ')')
            .Take(96)
            .ToArray())
            .Trim();
        return string.IsNullOrWhiteSpace(safe) ? null : safe;
    }

    private static bool LooksLikeLocalPath(string value)
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
}

sealed record Pass143MachineBackedApiProbeArtifacts(
    IReadOnlyList<Pass143MachineBackedApiProbeArtifactEvidence> Screenshots,
    IReadOnlyList<Pass143MachineBackedApiProbeArtifactEvidence> DesignRenders)
{
    public IReadOnlyList<Pass143MachineBackedApiProbeArtifactEvidence> AllEvidence
        => Screenshots.Concat(DesignRenders).ToArray();
}

sealed record Pass143MachineBackedApiProbeArtifactEvidence(
    string ArtifactId,
    string Kind,
    string ViewportId,
    int Width,
    int Height,
    string Uri,
    string Sha256,
    string LocalPath,
    string ProviderKind,
    bool BrowserOrPlaywrightUsed,
    bool VisualFidelityClaimed,
    string HiddenDeterminismBoundary,
    bool BrowserScreenshotFileVerified,
    bool BrowserScreenshotSha256Verified,
    bool BrowserScreenshotNonBlankPixelsVerified,
    long ByteLength,
    bool PngSignatureVerified,
    bool Sha256Verified,
    bool HasNonBlankPixels,
    int NonWhitePixelCount,
    int DistinctColorCount)
{
    public object ToPublicJsonObject()
    {
        var logicalFilename = $"{Kind}-{ViewportId}.png";
        var artifactUri = $"artifact://pass143/live-local/machine-backed-api-probe/{logicalFilename}";
        return new
        {
            artifactId = ArtifactId,
            kind = Kind,
            logicalFilename,
            viewportId = ViewportId,
            width = Width,
            height = Height,
            uri = artifactUri,
            artifactRef = artifactUri,
            localPath = (string?)null,
            mediaType = "image/png",
            byteLength = ByteLength,
            sha256 = Sha256,
            providerKind = ProviderKind,
            browserOrPlaywrightUsed = BrowserOrPlaywrightUsed,
            visualFidelityClaimed = VisualFidelityClaimed,
            hiddenDeterminismBoundary = HiddenDeterminismBoundary,
            browserScreenshotFileVerified = BrowserScreenshotFileVerified,
            browserScreenshotSha256Verified = BrowserScreenshotSha256Verified,
            browserScreenshotNonBlankPixelsVerified = BrowserScreenshotNonBlankPixelsVerified,
            pngSignatureVerified = PngSignatureVerified,
            sha256Verified = Sha256Verified,
            hasNonBlankPixels = HasNonBlankPixels,
            nonWhitePixelCount = NonWhitePixelCount,
            distinctColorCount = DistinctColorCount
        };
    }

    public DesignConversionModelServerArtifactRef ToArtifactRef()
        => new(
            ArtifactId,
            Kind,
            Width,
            Height,
            Uri,
            Sha256,
            new Dictionary<string, string>
            {
                ["local_png_proof_materialized"] = bool.TrueString,
                ["viewport_id"] = ViewportId,
                ["provider_kind"] = ProviderKind,
                ["browser_or_playwright_used"] = BrowserOrPlaywrightUsed.ToString(),
                ["visual_fidelity_claimed"] = VisualFidelityClaimed.ToString(),
                ["hidden_determinism_boundary"] = HiddenDeterminismBoundary,
                ["browser_screenshot_file_verified"] = BrowserScreenshotFileVerified.ToString(),
                ["browser_screenshot_sha256_verified"] = BrowserScreenshotSha256Verified.ToString(),
                ["browser_screenshot_nonblank_pixels_verified"] = BrowserScreenshotNonBlankPixelsVerified.ToString(),
                ["png_signature_verified"] = PngSignatureVerified.ToString(),
                ["png_sha256_verified"] = Sha256Verified.ToString(),
                ["png_width"] = Width.ToString(CultureInfo.InvariantCulture),
                ["png_height"] = Height.ToString(CultureInfo.InvariantCulture),
                ["png_byte_length"] = ByteLength.ToString(CultureInfo.InvariantCulture),
                ["png_nonblank_pixels_verified"] = HasNonBlankPixels.ToString(),
                ["png_non_white_pixel_count"] = NonWhitePixelCount.ToString(CultureInfo.InvariantCulture),
                ["png_distinct_color_count"] = DistinctColorCount.ToString(CultureInfo.InvariantCulture)
            });
}

sealed record Pass143VisualProviderSelection(
    Pass143ExternalVisualProviderRuntimeBoundary RuntimeBoundary,
    IDesignConversionComparisonProvider ComparisonProvider,
    IDesignConversionVisualJudge? VisualJudge)
{
    public static Pass143VisualProviderSelection Create(
        bool externalVisualProviderFromEnv,
        string renderArtifactRoot,
        HttpClient externalVisualProviderSharedHttpClient)
    {
        if (externalVisualProviderSharedHttpClient is null)
            throw new ArgumentNullException(nameof(externalVisualProviderSharedHttpClient));

        var boundary = Pass143ExternalVisualProviderRuntimeBoundary.Create(
            externalVisualProviderFromEnv,
            renderArtifactRoot,
            externalVisualProviderSharedHttpClient);
        if (!externalVisualProviderFromEnv)
        {
            return new Pass143VisualProviderSelection(
                boundary,
                new ServerOwnedLocalPngComparisonProvider(renderArtifactRoot),
                VisualJudge: null);
        }

        if (!boundary.ConfigComplete || !boundary.FactoryReady)
        {
            var noConfigured = new NoConfiguredVisualJudge();
            return new Pass143VisualProviderSelection(
                boundary,
                new DesignConversionExternalVisualAiComparisonProvider(noConfigured),
                noConfigured);
        }

        var input = Pass143ExternalVisualProviderRuntimeBoundary.CreateConfigInput(renderArtifactRoot);
        var judge = DesignConversionExternalVisualProviderConfigIntake.TryCreateVisualJudge(
            input,
            externalVisualProviderSharedHttpClient);
        if (!judge.IsSuccess)
        {
            var noConfigured = new NoConfiguredVisualJudge();
            return new Pass143VisualProviderSelection(
                boundary,
                new DesignConversionExternalVisualAiComparisonProvider(noConfigured),
                noConfigured);
        }

        return new Pass143VisualProviderSelection(
            boundary,
            new DesignConversionExternalVisualAiComparisonProvider(judge.Value!),
            judge.Value!);
    }
}

sealed record Pass143ExternalVisualProviderRuntimeBoundary(
    bool Requested,
    string Status,
    bool ConfigComplete,
    bool FactoryReady,
    bool FactoryConstructionSucceeded,
    bool RealExternalProviderExecutableWhenNetworkAuthorized,
    bool RealExternalProviderExecuted,
    bool NetworkCallExecuted,
    string EndpointHost,
    string ProviderName,
    string ModelId,
    string ApiKeyHeaderName,
    string ApiKeyScheme,
    IReadOnlyList<string> MissingSettingNames,
    IReadOnlyList<string> InvalidSettingNames,
    IReadOnlyDictionary<string, string> BoundaryMetadata,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings)
{
    public string ProviderKind
        => ConfigComplete
            ? DesignConversionExternalVisualAiComparisonProvider.ProviderKind
            : "none";

    public bool Configured => ConfigComplete;

    public bool Available => FactoryReady;

    public static Pass143ExternalVisualProviderRuntimeBoundary Create(
        bool requested,
        string renderArtifactRoot,
        HttpClient externalVisualProviderSharedHttpClient)
    {
        if (externalVisualProviderSharedHttpClient is null)
            throw new ArgumentNullException(nameof(externalVisualProviderSharedHttpClient));

        if (!requested)
            return FromDescriptor(
                requested: false,
                DesignConversionExternalVisualProviderConfigIntake.Describe(null),
                factoryConstructionSucceeded: false,
                factoryConstructionErrorCode: "not_requested");

        var input = CreateConfigInput(renderArtifactRoot);
        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var provider = descriptor.IsComplete
            ? DesignConversionExternalVisualProviderConfigIntake.TryCreateVisualJudge(
                input,
                externalVisualProviderSharedHttpClient)
            : null;

        return FromDescriptor(
            requested: true,
            descriptor,
            factoryConstructionSucceeded: provider?.IsSuccess == true,
            factoryConstructionErrorCode: provider is null ? "config_incomplete" : provider.ErrorCode);
    }

    public static DesignConversionExternalVisualProviderConfigInput CreateConfigInput(string renderArtifactRoot)
        => DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            ReadExternalVisualProviderEnvironmentSettings()) with
        {
            ServerOwnedImageArtifactRoot = renderArtifactRoot
        };

    public object ToPublicJsonObject()
        => new
        {
            requested = Requested,
            status = Status,
            config_complete = ConfigComplete,
            factory_ready = FactoryReady,
            factory_construction_succeeded = FactoryConstructionSucceeded,
            real_external_provider_executable_when_network_authorized =
                RealExternalProviderExecutableWhenNetworkAuthorized,
            real_external_provider_executed = RealExternalProviderExecuted,
            network_call_executed = NetworkCallExecuted,
            endpoint_host = EndpointHost,
            provider_name = ProviderName,
            provider_model = ModelId,
            provider_selection_policy = ReadBoundary(
                DesignConversionVisualAiProviderSelectionPolicy.ProviderSelectionPolicyKey,
                DesignConversionVisualAiProviderSelectionPolicy.PolicyName),
            requested_provider = ReadBoundary(
                DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderKey,
                "auto"),
            selected_provider = ReadBoundary(
                DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey,
                "none"),
            selected_provider_status = ReadBoundary(
                DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey,
                "not_selected"),
            provider_mode = ReadBoundary(
                DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey,
                "not_configured"),
            provider_fallback_status = ReadBoundary(
                DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey,
                "fallback_unavailable"),
            provider_fallback_reason = ReadBoundary(
                DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackReasonKey,
                "not_configured"),
            unavailable_provider_names = SplitCsv(ReadBoundary(
                DesignConversionVisualAiProviderSelectionPolicy.UnavailableProvidersKey,
                DesignConversionVisualAiProviderSelectionPolicy.CandidateProviders)),
            api_key_header_name = ApiKeyHeaderName,
            api_key_scheme = ApiKeyScheme,
            missing_setting_names = MissingSettingNames,
            invalid_setting_names = InvalidSettingNames,
            boundary_metadata = BoundaryMetadata,
            evidence_refs = EvidenceRefs,
            warnings = Warnings,
            api_key_value_included = false,
            secret_value_included = false,
            provider_payload_included = false
        };

    public Pass143ExternalVisualProviderRuntimeBoundary WithExecutionEvidence(
        IReadOnlyDictionary<string, string>? probeMetadata)
    {
        if (probeMetadata is null ||
            !IsTruthy(probeMetadata, "network_call_executed"))
        {
            return this;
        }

        var realExternalProviderExecuted =
            IsTruthy(probeMetadata, "real_external_provider_executed") ||
            IsTruthy(probeMetadata, "external_visual_ai_executed") ||
            IsTruthy(probeMetadata, "external_ai_visual_judge_executed");
        var metadata = new Dictionary<string, string>(BoundaryMetadata, StringComparer.Ordinal)
        {
            ["real_external_provider_executed"] = realExternalProviderExecuted.ToString(),
            ["network_call_executed"] = bool.TrueString,
            ["external_visual_provider_runtime_execution_observed"] = bool.TrueString,
            ["external_visual_provider_api_key_value_included"] = bool.FalseString,
            ["secret_value_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString
        };

        return this with
        {
            Status = realExternalProviderExecuted ? "executed" : Status,
            RealExternalProviderExecuted = realExternalProviderExecuted,
            NetworkCallExecuted = true,
            BoundaryMetadata = metadata,
            EvidenceRefs = EvidenceRefs
                .Append("external_visual_provider_runtime_execution_observed:true")
                .Append("network_call_executed:true")
                .Append($"real_external_provider_executed:{realExternalProviderExecuted.ToString()}")
                .Distinct(StringComparer.Ordinal)
                .OrderBy(reference => reference, StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static Pass143ExternalVisualProviderRuntimeBoundary FromDescriptor(
        bool requested,
        DesignConversionExternalVisualProviderConfigDescriptor descriptor,
        bool factoryConstructionSucceeded,
        string factoryConstructionErrorCode)
    {
        var metadata = new Dictionary<string, string>(descriptor.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["external_visual_provider_runtime_requested"] = requested.ToString(),
            ["external_visual_provider_factory_construction_succeeded"] = factoryConstructionSucceeded.ToString(),
            ["external_visual_provider_factory_construction_error_code"] =
                string.IsNullOrWhiteSpace(factoryConstructionErrorCode) ? "none" : factoryConstructionErrorCode,
            ["external_visual_provider_api_key_value_included"] = bool.FalseString,
            ["secret_value_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["real_external_provider_executed"] = bool.FalseString,
            ["network_call_executed"] = bool.FalseString
        };
        if (!requested)
        {
            DesignConversionVisualAiProviderSelectionPolicy.CopyPublicMetadata(
                metadata,
                DesignConversionVisualAiProviderSelectionPolicy.DescribeNotConfigured(
                    fallbackReason: "not_requested"));
        }

        var factoryReady = descriptor.FactoryReady && factoryConstructionSucceeded;

        return new Pass143ExternalVisualProviderRuntimeBoundary(
            Requested: requested,
            Status: requested ? descriptor.Status : "not_requested_network_not_executed",
            ConfigComplete: descriptor.IsComplete,
            FactoryReady: factoryReady,
            FactoryConstructionSucceeded: factoryConstructionSucceeded,
            RealExternalProviderExecutableWhenNetworkAuthorized:
                descriptor.RealExternalProviderExecutableWhenNetworkAuthorized && factoryReady,
            RealExternalProviderExecuted: false,
            NetworkCallExecuted: false,
            EndpointHost: descriptor.EndpointHost,
            ProviderName: descriptor.ProviderName,
            ModelId: descriptor.ModelId,
            ApiKeyHeaderName: descriptor.ApiKeyHeaderName,
            ApiKeyScheme: descriptor.ApiKeyScheme,
            MissingSettingNames: descriptor.MissingSettingNames,
            InvalidSettingNames: descriptor.InvalidSettingNames,
            BoundaryMetadata: metadata,
            EvidenceRefs: descriptor.EvidenceRefs
                .Append("external_visual_provider_runtime_env_boundary")
                .Append("factory_ready:" + factoryReady.ToString())
                .Append("network_call_executed:false")
                .Append("real_external_provider_executed:false")
                .Distinct(StringComparer.Ordinal)
                .OrderBy(reference => reference, StringComparer.Ordinal)
                .ToArray(),
            Warnings: descriptor.Warnings);
    }

    private string ReadBoundary(string key, string fallback)
        => BoundaryMetadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;

    private static IReadOnlyList<string> SplitCsv(string value)
        => string.IsNullOrWhiteSpace(value) || string.Equals(value, "none", StringComparison.Ordinal)
            ? []
            : value
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

    public static IReadOnlyDictionary<string, string> ReadExternalVisualProviderEnvironmentSettings()
    {
        var settings = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var name in ExternalVisualProviderSettingNames())
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrWhiteSpace(value))
                settings[name] = value;
        }

        return settings;
    }

    public static IReadOnlyList<string> ExternalVisualProviderSettingNames()
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

    private static bool IsTruthy(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "1", StringComparison.Ordinal) ||
            string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase));
}

sealed class Pass143RecordingBrowserAdapter : IDesignConversionBrowserProcessAdapter
{
    public Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
        DesignConversionBrowserCaptureRequest request,
        CancellationToken cancellationToken = default)
    {
        var sha = Pass143Hash.Sha256Text(string.Join(
            "|",
            request.CaptureRequestId,
            request.RequestId,
            request.Resolution.ResolutionId,
            request.Resolution.Width,
            request.Resolution.Height,
            request.DocumentHtml));

        return Task.FromResult(OperationResult<DesignConversionBrowserCaptureResult>.Ok(
            new DesignConversionBrowserCaptureResult(
                ScreenshotUri: $"artifact://pass143/browser/{sha}.png",
                ScreenshotSha256: sha,
                ElapsedMs: 1,
                Metadata: new Dictionary<string, string>
                {
                    ["browser_adapter_kind"] = "pass143-live-local-recording-browser-adapter",
                    ["browser_process_boundary"] = "local_recording_adapter_no_external_browser_process",
                    ["external_provider_called"] = bool.FalseString,
                    ["network_call_executed"] = bool.FalseString,
                    ["public_deployment_verified"] = bool.FalseString,
                    ["product_ready"] = bool.FalseString
                },
                Warnings: [])));
    }
}

static class Pass143Hash
{
    public static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
