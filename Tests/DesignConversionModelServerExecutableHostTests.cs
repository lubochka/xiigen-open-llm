// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.IO.Compression;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModelServer;
using ModelServer.AutonomousPromotion;
using ModelServer.DesignConversion;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

[Collection(DesignConversionLocalHostTestCollectionNames.CollectionName)]
public sealed class DesignConversionModelServerExecutableHostTests
{
    [Fact(Timeout = 120000)]
    public async Task ProgramExecutableHost_WritesReadyJsonBaseUrlForExternalCallerAndPreservesNoPublicBoundary()
    {
        const string privateMarker = "PRIVATE_EXECUTABLE_HOST_ROUTE_SECRET";
        var root = CreateTemporaryDirectory();
        var readyFile = Path.Combine(root, "ready", "modelserver-ready.json");
        Directory.CreateDirectory(Path.GetDirectoryName(readyFile)!);
        await File.WriteAllTextAsync(readyFile, "stale-ready-file");

        await using var app = Program.CreateLocalLoopbackHost(
            [
                Program.ReadyFileArgumentName, readyFile,
                "--urls", "http://0.0.0.0:54321"
            ]);

        try
        {
            await app.StartAsync();
            using var readyDocument = await WaitForReadyJsonAsync(readyFile);
            var ready = readyDocument.RootElement;
            var baseUrl = new Uri(ready.GetProperty("baseUrl").GetString()!);
            var routePaths = ready
                .GetProperty("routePaths")
                .EnumerateArray()
                .Select(route => route.GetString())
                .ToArray();
            var routes = ready
                .GetProperty("routes")
                .EnumerateArray()
                .Select(route => route.GetString())
                .ToArray();

            Assert.Equal(Uri.UriSchemeHttp, baseUrl.Scheme);
            Assert.Equal("127.0.0.1", baseUrl.Host);
            Assert.NotEqual(0, baseUrl.Port);
            Assert.NotEqual(54321, baseUrl.Port);
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                ready.GetProperty("endpointPath").GetString());
            Assert.Equal(
                Program.LocalExecutableHostBoundary,
                ready.GetProperty("boundary").GetString());
            Assert.Equal(
                Program.ReadyFileContractBoundary,
                ready.GetProperty("readyFileContractBoundary").GetString());
            Assert.DoesNotContain(
                "no_training",
                ready.GetProperty("readyFileContractBoundary").GetString(),
                StringComparison.Ordinal);
            Assert.True(ready.GetProperty("loopbackOnly").GetBoolean());
            Assert.False(ready.GetProperty("publicDeploymentVerified").GetBoolean());
            Assert.False(ready.GetProperty("publicProductionDefaultReady").GetBoolean());
            Assert.False(ready.GetProperty("productReady").GetBoolean());
            AssertExecutableReadyMetadataIsBrowserBackedAndNonDeterministic(ready);
            Assert.False(ready.GetProperty("performsTraining").GetBoolean());
            var continuousLearningReadiness = ready.GetProperty(
                "continuousLearningReadiness");
            Assert.False(continuousLearningReadiness
                .GetProperty("performs_training")
                .GetBoolean());
            Assert.Equal(
                continuousLearningReadiness
                    .GetProperty("performs_training")
                    .GetBoolean(),
                ready.GetProperty("performsTraining").GetBoolean());
            Assert.True(ready.GetProperty("dpoRoutesMapped").GetBoolean());
            Assert.True(ready.GetProperty("trainingRoutesMapped").GetBoolean());
            Assert.True(ready.GetProperty("artifactUploadConfigured").GetBoolean());
            Assert.True(ready.GetProperty("artifactUploadRouteMapped").GetBoolean());
            Assert.True(ready.GetProperty("artifactUploadFailClosedAdmissionRequired").GetBoolean());
            Assert.Equal(
                DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
                ready.GetProperty("artifactUploadEndpointPath").GetString());
            Assert.Equal(
                DesignConversionModelServerArtifactUploadStore.DefaultMaxUploadBytes,
                ready.GetProperty("artifactUploadMaxBytes").GetInt64());
            Assert.Equal("mapped", ready.GetProperty("artifactUploadStartupStatus").GetString());
            Assert.Equal("none", ready.GetProperty("artifactUploadNonReadyCode").GetString());
            Assert.Equal("none", ready.GetProperty("artifactUploadNonReadyReason").GetString());
            Assert.True(ready.GetProperty("autonomousPromotionEvaluationRouteMapped").GetBoolean());
            Assert.Equal(
                AutonomousPromotionEvaluationEndpoint.EndpointPath,
                ready.GetProperty("autonomousPromotionEvaluationEndpointPath").GetString());
            Assert.False(ready.GetProperty("autonomousPromotionEvaluationConfigured").GetBoolean());
            Assert.True(ready.GetProperty("autonomousPromotionEvaluationOptInRequired").GetBoolean());
            Assert.False(ready.GetProperty("autonomousPromotionEvaluationPerformsTraining").GetBoolean());
            Assert.False(ready.GetProperty("autonomousPromotionEvaluationProductReady").GetBoolean());
            Assert.False(ready.GetProperty("containsRawPrivateData").GetBoolean());
            Assert.Contains(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                routePaths);
            Assert.Contains(
                DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
                routePaths);
            Assert.Contains(
                DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
                routePaths);
            Assert.Contains(
                DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                routePaths);
            Assert.Contains(
                AutonomousPromotionEvaluationEndpoint.EndpointPath,
                routePaths);
            Assert.Contains(
                "POST " + DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
                routes);
            Assert.Contains(
                DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointRoute,
                routes);
            Assert.Contains(
                DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointRoute,
                routes);
            Assert.Contains(
                "POST " + AutonomousPromotionEvaluationEndpoint.EndpointPath,
                routes);
            var readyJson = ready.GetRawText();
            Assert.DoesNotContain(
                Program.LocalLoopbackArtifactUploadAdmissionToken,
                readyJson,
                StringComparison.Ordinal);
            Assert.DoesNotContain(".artifact-db", readyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("executable-artifact-db", readyJson, StringComparison.OrdinalIgnoreCase);

            AssertFullLocalStartupMetadata(app);

            using var client = new HttpClient
            {
                BaseAddress = baseUrl,
                Timeout = TimeSpan.FromSeconds(5)
            };
            var artifactUploadRoot = Program.CreateLocalLoopbackArtifactUploadRoot(readyFile);
            Assert.DoesNotContain(
                Path.GetFullPath(artifactUploadRoot),
                ready.GetRawText(),
                StringComparison.OrdinalIgnoreCase);
            var png = BuildRgbaPng(width: 6, height: 4, blank: false);
            var artifactId = "executable-upload-" + Guid.NewGuid().ToString("N");
            using var uploadRequest = BuildArtifactUploadRequest(
                png,
                artifactId,
                width: 6,
                height: 4,
                idempotencyKey: "executable-upload-" + Guid.NewGuid().ToString("N"));
            using var uploadResponse = await client.SendAsync(uploadRequest);
            var uploadBodyJson = await uploadResponse.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
            Assert.Equal(
                Program.LocalExecutableHostBoundary,
                uploadResponse.Headers.GetValues(
                    "x-design-conversion-modelserver-executable-host-boundary").Single());
            Assert.Equal(
                bool.FalseString,
                uploadResponse.Headers.GetValues(
                    "x-design-conversion-product-ready").Single());
            Assert.DoesNotContain(
                Program.LocalLoopbackArtifactUploadAdmissionToken,
                uploadBodyJson,
                StringComparison.Ordinal);

            using (var uploadDocument = JsonDocument.Parse(uploadBodyJson))
            {
                var uploadBody = uploadDocument.RootElement;
                Assert.Equal(
                    DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
                    uploadBody.GetProperty("endpoint").GetString());
                Assert.True(uploadBody.GetProperty("liveHttpRouteRegistered").GetBoolean());
                Assert.True(uploadBody.GetProperty("aspNetEndpointMapped").GetBoolean());
                Assert.False(uploadBody.GetProperty("publicHttpEndpointVerified").GetBoolean());
                Assert.False(uploadBody.GetProperty("productReady").GetBoolean());
                Assert.False(uploadBody.GetProperty("rawBytesIncluded").GetBoolean());
                Assert.Equal(png.Length, uploadBody.GetProperty("byteCount").GetInt64());
                Assert.Equal(Sha256(png), uploadBody.GetProperty("sha256").GetString());
                Assert.Equal(artifactId, uploadBody.GetProperty("artifactId").GetString());
                Assert.Equal("screenshot", uploadBody.GetProperty("kind").GetString());

                var artifactRef = JsonSerializer.Deserialize<DesignConversionModelServerArtifactRef>(
                    uploadBody.GetProperty("artifactRef").GetRawText(),
                    DesignConversionModelServerHttpHandler.JsonOptions);
                Assert.NotNull(artifactRef);
                var resolved = new DesignConversionServerOwnedLocalArtifactRefResolver(
                        artifactUploadRoot)
                    .Validate(artifactRef!, "executable host artifact upload");
                Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
                Assert.Equal(png, await File.ReadAllBytesAsync(new Uri(artifactRef!.Uri).LocalPath));
            }

            using var evaluateContent = new StringContent("{}", Encoding.UTF8, "application/json");
            using var evaluateResponse = await client.PostAsync(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                evaluateContent);
            var bodyJson = await evaluateResponse.Content.ReadAsStringAsync();
            Assert.DoesNotContain(
                Path.GetFullPath(artifactUploadRoot),
                bodyJson,
                StringComparison.OrdinalIgnoreCase);

            Assert.Equal(HttpStatusCode.BadRequest, evaluateResponse.StatusCode);
            Assert.Equal(
                Program.LocalExecutableHostBoundary,
                evaluateResponse.Headers.GetValues(
                    "x-design-conversion-modelserver-executable-host-boundary").Single());
            Assert.Equal(
                bool.TrueString,
                evaluateResponse.Headers.GetValues(
                    "x-design-conversion-modelserver-localhost-loopback-only").Single());
            Assert.Equal(
                bool.FalseString,
                evaluateResponse.Headers.GetValues(
                    "x-design-conversion-public-deployment-verified").Single());
            Assert.Equal(
                bool.FalseString,
                evaluateResponse.Headers.GetValues(
                    "x-design-conversion-product-ready").Single());

            using var document = JsonDocument.Parse(bodyJson);
            var metadata = document.RootElement.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_host_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("localhost_loopback_only").GetString());
            Assert.Equal("127.0.0.1", metadata.GetProperty("loopback_host").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("public_production_default_ready").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("refs_only_public_response").GetString());

            using var dpoResponse = await client.GetAsync(
                DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath + "?includeBlocked=true&limit=10");
            var dpoBodyJson = await dpoResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, dpoResponse.StatusCode);
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
                dpoResponse.Headers.GetValues("x-design-conversion-asp-net-route").Single());
            Assert.Equal(
                bool.FalseString,
                dpoResponse.Headers.GetValues("x-design-conversion-product-ready").Single());
            Assert.DoesNotContain(privateMarker, dpoBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(".artifact-db", dpoBodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(
                Path.GetFullPath(artifactUploadRoot),
                dpoBodyJson,
                StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Program.LocalLoopbackArtifactUploadAdmissionToken, dpoBodyJson, StringComparison.Ordinal);

            using (var dpoDocument = JsonDocument.Parse(dpoBodyJson))
            {
                var dpoRoot = dpoDocument.RootElement;
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
                    dpoRoot.GetProperty("endpoint").GetString());
                Assert.Equal("GET", dpoRoot.GetProperty("method").GetString());
                Assert.Equal(200, dpoRoot.GetProperty("statusCode").GetInt32());
                Assert.Equal("dpo-usefulness-readback-completed", dpoRoot.GetProperty("contractStatus").GetString());
                Assert.Equal(0, dpoRoot.GetProperty("rows").GetArrayLength());
                Assert.Equal(0, dpoRoot.GetProperty("rowCount").GetInt32());
                Assert.Equal(0, dpoRoot.GetProperty("trainReadyCount").GetInt32());
                Assert.False(dpoRoot.GetProperty("productReady").GetBoolean());

                var dpoMetadata = dpoRoot.GetProperty("boundaryMetadata");
                Assert.Equal(bool.TrueString, dpoMetadata.GetProperty("runtime_invoked").GetString());
                Assert.Equal(bool.TrueString, dpoMetadata.GetProperty("dpo_usefulness_response_refs_only").GetString());
                Assert.Equal(bool.TrueString, dpoMetadata.GetProperty("refs_only_public_response").GetString());
                Assert.Equal(bool.FalseString, dpoMetadata.GetProperty("raw_html_included").GetString());
                Assert.Equal(bool.FalseString, dpoMetadata.GetProperty("raw_css_included").GetString());
                Assert.Equal(bool.FalseString, dpoMetadata.GetProperty("raw_design_tree_included").GetString());
                Assert.Equal(bool.FalseString, dpoMetadata.GetProperty("response_contains_raw_private_data").GetString());
                Assert.Equal(bool.FalseString, dpoMetadata.GetProperty("public_http_endpoint_verified").GetString());
                Assert.Equal(bool.FalseString, dpoMetadata.GetProperty("deployment_verified").GetString());
                Assert.Equal(bool.FalseString, dpoMetadata.GetProperty("product_ready").GetString());
            }

            using var trainingContent = new StringContent(
                JsonSerializer.Serialize(
                    new
                    {
                        action = "list",
                        metadata = new Dictionary<string, string>
                        {
                            ["private_marker"] = privateMarker
                        }
                    },
                    DesignConversionModelServerHttpHandler.JsonOptions),
                Encoding.UTF8,
                "application/json");
            using var trainingResponse = await client.PostAsync(
                DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                trainingContent);
            var trainingBodyJson = await trainingResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, trainingResponse.StatusCode);
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                trainingResponse.Headers.GetValues("x-design-conversion-asp-net-route").Single());
            Assert.Equal(
                bool.FalseString,
                trainingResponse.Headers.GetValues("x-design-conversion-product-ready").Single());
            Assert.DoesNotContain(privateMarker, trainingBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(".artifact-db", trainingBodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Program.LocalLoopbackArtifactUploadAdmissionToken, trainingBodyJson, StringComparison.Ordinal);

            using var trainingDocument = JsonDocument.Parse(trainingBodyJson);
            var trainingRoot = trainingDocument.RootElement;
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                trainingRoot.GetProperty("endpoint").GetString());
            Assert.Equal("POST", trainingRoot.GetProperty("method").GetString());
            Assert.Equal(200, trainingRoot.GetProperty("statusCode").GetInt32());
            Assert.Equal("training-intake-action-completed", trainingRoot.GetProperty("contractStatus").GetString());
            Assert.Equal("list", trainingRoot.GetProperty("action").GetString());
            Assert.Equal(0, trainingRoot.GetProperty("intakeCount").GetInt32());
            Assert.Equal(0, trainingRoot.GetProperty("intakes").GetArrayLength());
            Assert.Equal(JsonValueKind.Null, trainingRoot.GetProperty("trainingRun").ValueKind);
            Assert.False(trainingRoot.GetProperty("productReady").GetBoolean());
            Assert.False(trainingRoot.GetProperty("deploymentVerified").GetBoolean());
            Assert.False(trainingRoot.GetProperty("publicHttpEndpointVerified").GetBoolean());

            var trainingMetadata = trainingRoot.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, trainingMetadata.GetProperty("runtime_invoked").GetString());
            Assert.Equal("list", trainingMetadata.GetProperty("training_intake_action").GetString());
            Assert.Equal(bool.TrueString, trainingMetadata.GetProperty("training_intake_response_refs_only").GetString());
            Assert.Equal(bool.TrueString, trainingMetadata.GetProperty("refs_only_public_response").GetString());
            Assert.Equal(bool.FalseString, trainingMetadata.GetProperty("raw_html_included").GetString());
            Assert.Equal(bool.FalseString, trainingMetadata.GetProperty("raw_css_included").GetString());
            Assert.Equal(bool.FalseString, trainingMetadata.GetProperty("raw_design_tree_included").GetString());
            Assert.Equal(bool.FalseString, trainingMetadata.GetProperty("response_contains_raw_private_data").GetString());
            Assert.Equal(bool.FalseString, trainingMetadata.GetProperty("network_call_executed").GetString());
            Assert.Equal(bool.FalseString, trainingMetadata.GetProperty("product_ready").GetString());
        }
        finally
        {
            await app.StopAsync();
            Assert.False(File.Exists(readyFile));
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ProgramExecutableHost_RejectsBlankAndDirectoryReadyFilePaths()
    {
        Assert.Throws<ArgumentException>(() => Program.CreateLocalLoopbackHost(
            [Program.ReadyFileArgumentName, " "]));

        var root = CreateTemporaryDirectory();
        try
        {
            Assert.Throws<ArgumentException>(() => Program.CreateLocalLoopbackHost(
                [Program.ReadyFileArgumentName, root]));
            Assert.Throws<ArgumentException>(() => Program.CreateLocalLoopbackHost(
                [Program.ReadyFileArgumentName + "=" + root + Path.DirectorySeparatorChar]));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ModelServerAssembly_DeclaresExecutableEntryPointAndLoopbackDefault()
    {
        var entryPoint = typeof(Program).Assembly.EntryPoint;

        Assert.NotNull(entryPoint);
        Assert.Equal(typeof(Program), entryPoint!.DeclaringType);
        Assert.Equal(nameof(Program.Main), entryPoint.Name);
        Assert.Equal(typeof(void), entryPoint.ReturnType);
        Assert.Equal("http://127.0.0.1:0", Program.LocalLoopbackUrl);
        Assert.Contains("loopback_host", Program.LocalExecutableHostBoundary, StringComparison.Ordinal);
        Assert.Contains("no_public_deployment", Program.LocalExecutableHostBoundary, StringComparison.Ordinal);
        Assert.Contains("no_product_ready", Program.LocalExecutableHostBoundary, StringComparison.Ordinal);

        var projectXml = File.ReadAllText(FindRepoFile("25.ModelServer", "ModelServer.csproj"));
        Assert.Contains("<OutputType>Exe</OutputType>", projectXml, StringComparison.Ordinal);

        AssertExecutableDefaultCompositionIsBrowserBackedAndNonDeterministic(
            Program.CreateLocalLoopbackServiceComposition());

        var registry = Program.CreateLocalLoopbackRouteRegistry();
        Assert.True(registry.Resolve(
            DesignConversionModelServerEndpointRegistry.EndpointMethod,
            DesignConversionModelServerEndpointRegistry.EndpointPath).IsSuccess);
        var improvementLoopRegistration = registry.Resolve(
            DesignConversionModelServerEndpointRegistry.EndpointMethod,
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath);
        Assert.True(improvementLoopRegistration.IsSuccess, improvementLoopRegistration.ErrorMessage);
        AssertExecutableRouteMetadataIsBrowserBackedAndNonDeterministic(
            improvementLoopRegistration.Value!.BoundaryMetadata);
        Assert.Contains(
            "executable-artifact-db",
            Program.CreateLocalLoopbackArtifactDbRoot(),
            StringComparison.Ordinal);
        var dpoUsefulnessRegistration = registry.Resolve(
            DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointMethod,
            DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath);
        Assert.True(dpoUsefulnessRegistration.IsSuccess, dpoUsefulnessRegistration.ErrorMessage);
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointRoute,
            dpoUsefulnessRegistration.Value!.BoundaryMetadata["endpoint_route"]);
        Assert.Equal(bool.FalseString, dpoUsefulnessRegistration.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.TrueString, dpoUsefulnessRegistration.Value.BoundaryMetadata["dpo_usefulness_response_refs_only"]);
        var trainingIntakeRegistration = registry.Resolve(
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointMethod,
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath);
        Assert.True(trainingIntakeRegistration.IsSuccess, trainingIntakeRegistration.ErrorMessage);
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointRoute,
            trainingIntakeRegistration.Value!.BoundaryMetadata["endpoint_route"]);
        Assert.Equal(bool.FalseString, trainingIntakeRegistration.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.TrueString, trainingIntakeRegistration.Value.BoundaryMetadata["training_intake_response_refs_only"]);
    }

    [Fact]
    public async Task ProgramLoopbackRegistry_SharedPngRootComparesUploadedArtifactsAndRejectsSiblingEscape()
    {
        var root = CreateTemporaryDirectory();
        var sharedPngRoot = Path.Combine(root, "server-owned-png-root");
        var escapedRoot = Path.Combine(root, "escaped-sibling");
        Directory.CreateDirectory(escapedRoot);

        try
        {
            using var registry = Program.CreateLocalLoopbackRouteRegistryWithLocalPngArtifactRoot(
                sharedPngRoot);
            Assert.True(
                Program.TryGetLocalLoopbackRouteRegistryServiceComposition(
                    registry,
                    out var resolvedComposition));
            var composition = Assert.IsType<DesignConversionLocalServiceComposition>(
                resolvedComposition);
            var comparisonProvider = Assert.IsType<ServerOwnedLocalPngComparisonProvider>(
                composition.ComparisonProvider);
            Assert.IsType<DefaultLocalDesignConversionVisualJudge>(composition.VisualJudge);
            Assert.IsType<DesignConversionServerOwnedLocalArtifactRefResolver>(
                ReadProgramRegistryArtifactRefResolver(registry));

            var png = BuildRgbaPng(width: 6, height: 4, blank: false);
            var uploadStore = new DesignConversionModelServerArtifactUploadStore(sharedPngRoot);
            var expectedUpload = await uploadStore.StoreAsync(
                new MemoryStream(png, writable: false),
                new DesignConversionModelServerArtifactUploadRequest(
                    ArtifactId: "w047-uploaded-expected",
                    Kind: "design-render",
                    MediaType: "image/png",
                    ExpectedSha256: Sha256(png),
                    Width: 6,
                    Height: 4));
            var renderedUpload = await uploadStore.StoreAsync(
                new MemoryStream(png, writable: false),
                new DesignConversionModelServerArtifactUploadRequest(
                    ArtifactId: "w047-browser-rendered",
                    Kind: "browser-render",
                    MediaType: "image/png",
                    ExpectedSha256: Sha256(png),
                    Width: 6,
                    Height: 4));
            Assert.True(expectedUpload.IsSuccess, expectedUpload.ErrorMessage);
            Assert.True(renderedUpload.IsSuccess, renderedUpload.ErrorMessage);

            var resolution = new RenderQaResolution("w047-6x4", 6, 4, 1d);
            var expected = ToRenderQaImage(
                expectedUpload.Value!.ArtifactRef,
                resolution,
                "expected");
            var rendered = ToRenderQaImage(
                renderedUpload.Value!.ArtifactRef,
                resolution,
                "rendered");
            var compared = await comparisonProvider.CompareAsync(
                new DesignConversionQaComparisonRequest(
                    ComparisonRequestId: "w047-shared-root-comparison",
                    RequestId: "w047-shared-root-request",
                    Expected: expected,
                    Actual: rendered,
                    Metadata: new Dictionary<string, string>()));
            Assert.True(compared.IsSuccess, compared.ErrorMessage);
            Assert.Equal(
                ServerOwnedLocalPngComparisonProvider.ProviderKind,
                compared.Value!.Metadata["comparison_provider_kind"]);

            var escapedPath = Path.Combine(escapedRoot, "escaped.png");
            await File.WriteAllBytesAsync(escapedPath, png);
            var escaped = await comparisonProvider.CompareAsync(
                new DesignConversionQaComparisonRequest(
                    ComparisonRequestId: "w047-sibling-escape-comparison",
                    RequestId: "w047-sibling-escape-request",
                    Expected: expected with { Uri = new Uri(escapedPath).AbsoluteUri },
                    Actual: rendered,
                    Metadata: new Dictionary<string, string>()));
            Assert.False(escaped.IsSuccess);
            Assert.Equal("LOCAL_PNG_COMPARISON_PATH_ESCAPED_ROOT", escaped.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ProgramLoopbackRegistry_LocalIndependentEvaluatorIsExplicitFailClosedAndDisposedWithRegistry()
    {
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            Path.Combine(
                Path.GetTempPath(),
                "modelserver-local-evaluator-" + Guid.NewGuid().ToString("N")));
        using var defaultRegistry = Program.CreateLocalLoopbackRouteRegistry(
            artifactDb);

        Assert.False(defaultRegistry.LocalIndependentPreferenceEvaluatorRequested);
        Assert.False(defaultRegistry.LocalIndependentPreferenceEvaluatorConfigured);
        Assert.Equal("disabled", defaultRegistry.LocalIndependentPreferenceEvaluatorStatus);
        Assert.Null(
            DesignConversionIndependentPreferenceEvidenceAssociation.Resolve(
                artifactDb));

        var configuration = CreateLocalIndependentEvaluatorConfiguration();
        var configuredRegistry = Program
            .CreateLocalLoopbackRouteRegistryWithLocalIndependentPreferenceEvaluatorAndLocalPngArtifactRoot(
                artifactDb,
                configuration,
                Path.Combine(
                    Path.GetTempPath(),
                    "modelserver-local-evaluator-png-" + Guid.NewGuid().ToString("N")));
        Assert.True(configuredRegistry.LocalIndependentPreferenceEvaluatorRequested);
        Assert.True(configuredRegistry.LocalIndependentPreferenceEvaluatorConfigured);
        Assert.Equal("configured", configuredRegistry.LocalIndependentPreferenceEvaluatorStatus);
        Assert.Equal("none", configuredRegistry.LocalIndependentPreferenceEvaluatorErrorCode);
        Assert.NotNull(
            DesignConversionIndependentPreferenceEvidenceAssociation.Resolve(
                artifactDb));
        Assert.IsType<DesignConversionServerOwnedLocalArtifactRefResolver>(
            ReadProgramRegistryArtifactRefResolver(configuredRegistry));

        using var duplicateRegistry = Program
            .CreateLocalLoopbackRouteRegistryWithLocalIndependentPreferenceEvaluator(
            artifactDb,
            configuration);
        Assert.True(duplicateRegistry.LocalIndependentPreferenceEvaluatorRequested);
        Assert.False(duplicateRegistry.LocalIndependentPreferenceEvaluatorConfigured);
        Assert.Equal("unavailable", duplicateRegistry.LocalIndependentPreferenceEvaluatorStatus);
        Assert.Equal(
            DesignConversionLocalIndependentPreferenceEvaluatorRuntime
                .ArtifactRootAlreadyOwnedErrorCode,
            duplicateRegistry.LocalIndependentPreferenceEvaluatorErrorCode);

        configuredRegistry.Dispose();
        Assert.Null(
            DesignConversionIndependentPreferenceEvidenceAssociation.Resolve(
                artifactDb));
    }

    [Fact(Timeout = 120000)]
    public async Task AspNetApplicationStop_DisposesExplicitLocalEvaluatorRegistryAuthority()
    {
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            Path.Combine(
                Path.GetTempPath(),
                "modelserver-local-evaluator-host-" +
                Guid.NewGuid().ToString("N")));
        using var registry = Program
            .CreateLocalLoopbackRouteRegistryWithLocalIndependentPreferenceEvaluator(
            artifactDb,
            CreateLocalIndependentEvaluatorConfiguration());
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);
        await app.StartAsync();
        Assert.NotNull(
            DesignConversionIndependentPreferenceEvidenceAssociation.Resolve(
                artifactDb));

        await app.StopAsync();

        Assert.Null(
            DesignConversionIndependentPreferenceEvidenceAssociation.Resolve(
                artifactDb));
        Assert.True(registry.IsDisposed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("False")]
    public async Task ProgramCreateLocalLoopbackHost_LocalEvaluatorAbsentOrFalseKeepsDefaultZeroAuthority(
        string? localEvaluatorSetting)
    {
        using var fixture = new ProgramLocalEvaluatorEnvironmentFixture();
        using var environment = fixture.Apply(localEvaluatorSetting);
        var readyFile = Path.Combine(
            fixture.Root,
            localEvaluatorSetting is null ? "absent" : "false",
            "ready.json");
        await using var app = Program.CreateLocalLoopbackHost(
            [Program.ReadyFileArgumentName, readyFile]);
        var registry = ReadProgramRegistryFromUnstartedHost(app);
        var artifactDb = ReadTrainingIntakeArtifactDb(registry);

        Assert.False(registry.LocalIndependentPreferenceEvaluatorRequested);
        Assert.False(registry.LocalIndependentPreferenceEvaluatorConfigured);
        Assert.Equal("disabled", registry.LocalIndependentPreferenceEvaluatorStatus);
        Assert.Null(
            DesignConversionIndependentPreferenceEvidenceAssociation.Resolve(
                artifactDb));
        Assert.Equal(
            Path.GetFullPath(Program.CreateLocalLoopbackArtifactDbRoot(readyFile)),
            Path.GetFullPath(artifactDb.AllowedArtifactRootDirectory));

        var dpo = await registry.InvokeAsync(
            new DesignConversionModelServerHttpRequest(
                Method: DesignConversionModelServerEndpointRegistry
                    .DpoUsefulnessEndpointMethod,
                Path: DesignConversionModelServerEndpointRegistry
                    .DpoUsefulnessEndpointPath,
                BodyJson: null,
                ContentType: null,
                QueryString: "?includeBlocked=true&limit=10"));
        Assert.True(dpo.IsSuccess, dpo.ErrorMessage);
        using var document = JsonDocument.Parse(dpo.Value!.BodyJson);
        Assert.Equal(0, document.RootElement.GetProperty("rowCount").GetInt32());
        Assert.Empty(document.RootElement.GetProperty("rows").EnumerateArray());
    }

    [Fact(Timeout = 120000)]
    public async Task ProgramCreateLocalLoopbackHost_LocalEvaluatorTrueOwnsExactRootUntilApplicationStopping()
    {
        using var fixture = new ProgramLocalEvaluatorEnvironmentFixture();
        using var environment = fixture.Apply(bool.TrueString);
        var readyFile = Path.Combine(fixture.Root, "true", "ready.json");
        Directory.CreateDirectory(Path.GetDirectoryName(readyFile)!);
        await using var app = Program.CreateLocalLoopbackHost(
            [Program.ReadyFileArgumentName, readyFile]);
        var registry = ReadProgramRegistryFromUnstartedHost(app);
        var artifactDb = ReadTrainingIntakeArtifactDb(registry);
        var expectedRootBinding =
            DesignConversionIndependentPreferenceEvidenceAssociation
                .BuildArtifactRootBinding(artifactDb);

        Assert.True(registry.LocalIndependentPreferenceEvaluatorRequested);
        Assert.True(registry.LocalIndependentPreferenceEvaluatorConfigured);
        Assert.Equal("configured", registry.LocalIndependentPreferenceEvaluatorStatus);
        var registration =
            DesignConversionIndependentPreferenceEvidenceAssociation.Resolve(
                artifactDb);
        Assert.NotNull(registration);
        Assert.Equal(expectedRootBinding, registration!.ArtifactRootBinding);
        Assert.Equal(
            Path.GetFullPath(Program.CreateLocalLoopbackArtifactDbRoot(readyFile)),
            Path.GetFullPath(artifactDb.AllowedArtifactRootDirectory));

        await app.StartAsync();
        await app.StopAsync();

        Assert.True(registry.IsDisposed);
        Assert.False(registry.LocalIndependentPreferenceEvaluatorConfigured);
        Assert.Equal("disposed", registry.LocalIndependentPreferenceEvaluatorStatus);
        Assert.Null(
            DesignConversionIndependentPreferenceEvidenceAssociation.Resolve(
                artifactDb));
    }

    [Fact]
    public async Task ProgramLoopbackRegistry_ConfiguredMeasurementGateThresholdReachesMachineHeldOutEvaluation()
    {
        var defaultRegistry = Program.CreateLocalLoopbackRouteRegistry();
        var defaultMachine = ReadProgramRegistryMachine(defaultRegistry);
        await TrainProgramThresholdMachineAsync(defaultMachine, "program-threshold-default-training");

        var defaultEvaluation = await defaultMachine.EvaluateAsync(
            BuildProgramThreeOfFourHeldOutEvaluationSet("program-threshold-default-eval"));

        Assert.True(defaultEvaluation.IsSuccess, defaultEvaluation.ErrorMessage);
        Assert.Equal(4, defaultEvaluation.Value!.RowsEvaluated);
        Assert.Equal(0.75, defaultEvaluation.Value.Accuracy);
        Assert.True(defaultEvaluation.Value.PassesGate, defaultEvaluation.Value.GateNote);
        Assert.Contains("accuracy=0.7500", defaultEvaluation.Value.GateNote, StringComparison.Ordinal);
        Assert.Contains("threshold=0.7500", defaultEvaluation.Value.GateNote, StringComparison.Ordinal);

        var configuration = new DesignConversionModelServerMeasurementGateThresholdConfiguration(
            RequiredHeldOutQualityScore: 0.90,
            MaxRuntimeMs: 800,
            MaxCostUsd: 1.00m,
            MaxPeakRamMb: 512);
        var configuredRegistry = Program.CreateLocalLoopbackRouteRegistry(
            measurementGateThresholdConfiguration: configuration);

        Assert.Same(
            configuration,
            ReadTrainingIntakeMeasurementGateThresholdConfiguration(configuredRegistry));

        var configuredMachine = ReadProgramRegistryMachine(configuredRegistry);
        await TrainProgramThresholdMachineAsync(configuredMachine, "program-threshold-configured-training");

        var configuredEvaluation = await configuredMachine.EvaluateAsync(
            BuildProgramThreeOfFourHeldOutEvaluationSet("program-threshold-configured-eval"));

        Assert.True(configuredEvaluation.IsSuccess, configuredEvaluation.ErrorMessage);
        Assert.Equal(4, configuredEvaluation.Value!.RowsEvaluated);
        Assert.Equal(0.75, configuredEvaluation.Value.Accuracy);
        Assert.False(configuredEvaluation.Value.PassesGate);
        Assert.Contains("accuracy=0.7500", configuredEvaluation.Value.GateNote, StringComparison.Ordinal);
        Assert.Contains("threshold=0.9000", configuredEvaluation.Value.GateNote, StringComparison.Ordinal);
        Assert.Contains("< threshold=0.9000;", configuredEvaluation.Value.GateNote, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ProgramComposition_ThreadsOnlyExactCallerOwnedNullableDpoPolicyBaseProvider()
    {
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var directDb = Program.CreateLocalLoopbackArtifactDb(
            dpoPolicyBaseProvider: provider);
        Assert.Same(provider, ReadDpoPolicyBaseProvider(directDb));

        var registry = Program.CreateLocalLoopbackRouteRegistry(
            dpoPolicyBaseProvider: provider);
        var routedDb = ReadTrainingIntakeArtifactDb(registry);
        Assert.Same(provider, ReadDpoPolicyBaseProvider(routedDb));

        var externalProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var externalRoot = CreateTemporaryDirectory();
        try
        {
            var externalDb = new JsonlDesignConversionLocalArtifactDb(
                externalRoot,
                dpoPolicyBaseProvider: externalProvider);
            var externalRegistry = Program.CreateLocalLoopbackRouteRegistry(
                externalDb,
                provider);

            Assert.Same(externalDb, ReadTrainingIntakeArtifactDb(externalRegistry));
            Assert.Same(externalProvider, ReadDpoPolicyBaseProvider(externalDb));
            Assert.NotSame(provider, ReadDpoPolicyBaseProvider(externalDb));
        }
        finally
        {
            Directory.Delete(externalRoot, recursive: true);
        }

        var unconfiguredDb = Program.CreateLocalLoopbackArtifactDb(
            dpoPolicyBaseProvider: null);
        var unconfiguredRegistry = Program.CreateLocalLoopbackRouteRegistry(
            dpoPolicyBaseProvider: null);
        Assert.Null(ReadDpoPolicyBaseProvider(unconfiguredDb));
        Assert.Null(ReadDpoPolicyBaseProvider(ReadTrainingIntakeArtifactDb(unconfiguredRegistry)));

        var hostRoot = CreateTemporaryDirectory();
        try
        {
            await using (var configuredHost = Program.CreateLocalLoopbackHost(
                [Program.ReadyFileArgumentName, Path.Combine(hostRoot, "configured", "ready.json")],
                provider))
            {
                var hostDb = ReadTrainingIntakeArtifactDbFromUnstartedHost(configuredHost);
                Assert.Same(provider, ReadDpoPolicyBaseProvider(hostDb));
            }

            await using (var unconfiguredHost = Program.CreateLocalLoopbackHost(
                [Program.ReadyFileArgumentName, Path.Combine(hostRoot, "unconfigured", "ready.json")],
                dpoPolicyBaseProvider: null))
            {
                var hostDb = ReadTrainingIntakeArtifactDbFromUnstartedHost(unconfiguredHost);
                Assert.Null(ReadDpoPolicyBaseProvider(hostDb));
            }
        }
        finally
        {
            Directory.Delete(hostRoot, recursive: true);
        }

        Assert.Equal(0, provider.InvocationCount);
        Assert.Equal(0, externalProvider.InvocationCount);

        AssertOptionalNullableProviderParameter(
            typeof(Program).GetMethod(nameof(Program.CreateLocalLoopbackHost))!);
        AssertOptionalNullableProviderParameter(
            typeof(Program).GetMethod(nameof(Program.CreateLocalLoopbackRouteRegistry))!);
        AssertOptionalNullableProviderParameter(
            typeof(Program).GetMethod(nameof(Program.CreateLocalLoopbackArtifactDb))!);
        AssertLocalLoopbackRouteRegistryOptionalParameterContract();
        AssertTrailingOptionalNullableMeasurementGateThresholdConfigurationParameter(
            typeof(Program).GetMethod(nameof(Program.CreateLocalLoopbackServiceComposition))!);

        var productionProviderTypes = typeof(Program).Assembly.GetTypes()
            .Where(type =>
                typeof(IDesignConversionDpoPolicyBaseProvider)
                    .IsAssignableFrom(type)
                && type is { IsInterface: false, IsAbstract: false })
            .ToArray();
        Assert.Equal(
            [typeof(DesignConversionRetainedDpoPolicyBaseProviderGateway)],
            productionProviderTypes);

        var source = File.ReadAllText(FindRepoFile("25.ModelServer", "Program.cs"));
        Assert.DoesNotContain(
            "dpoPolicyBaseProvider: null).Run()",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "DesignConversionRetainedDpoPolicyBaseProviderGateway",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain("Qwen", source, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertOptionalNullableProviderParameter(
        System.Reflection.MethodInfo method)
        => AssertOptionalNullableNamedParameter(
            method,
            "dpoPolicyBaseProvider",
            typeof(IDesignConversionDpoPolicyBaseProvider));

    private static void AssertLocalLoopbackRouteRegistryOptionalParameterContract()
    {
        var method = typeof(Program).GetMethod(
            nameof(Program.CreateLocalLoopbackRouteRegistry))!;
        var parameters = method.GetParameters();
        Assert.Equal(
            [
                "artifactDb",
                "dpoPolicyBaseProvider",
                "measurementGateThresholdConfiguration",
                "trainingIntakeDpoPairStoreOptions",
            ],
            parameters.Select(parameter => parameter.Name!).ToArray());

        // Опция хранилища обучающих пар добавлена четвертым параметром,
        // после настройки порога измерительной проверки. Поэтому старый
        // третий позиционный аргумент по-прежнему означает именно настройку
        // порога. Этот тест закрепляет полный порядок параметров метода
        // `CreateLocalLoopbackRouteRegistry` по именам; правило "порог является
        // последним параметром" здесь не применимо.
        AssertOptionalNullableNamedParameter(
            method,
            "measurementGateThresholdConfiguration",
            typeof(DesignConversionModelServerMeasurementGateThresholdConfiguration));
        AssertOptionalNullableNamedParameter(
            method,
            "trainingIntakeDpoPairStoreOptions",
            typeof(DesignConversionTrainingIntakeDpoPairStoreOptions));
    }

    private static void AssertTrailingOptionalNullableMeasurementGateThresholdConfigurationParameter(
        System.Reflection.MethodInfo method)
    {
        var parameter = method.GetParameters().Last();
        Assert.Equal("measurementGateThresholdConfiguration", parameter.Name);
        AssertOptionalNullableNamedParameter(
            method,
            "measurementGateThresholdConfiguration",
            typeof(DesignConversionModelServerMeasurementGateThresholdConfiguration));
    }

    private static void AssertOptionalNullableNamedParameter(
        System.Reflection.MethodInfo method,
        string parameterName,
        Type parameterType)
    {
        var parameter = method.GetParameters().Single(candidate =>
            string.Equals(candidate.Name, parameterName, StringComparison.Ordinal));
        Assert.Equal(parameterName, parameter.Name);
        Assert.Equal(parameterType, parameter.ParameterType);
        Assert.True(parameter.HasDefaultValue);
        Assert.Null(parameter.DefaultValue);
        Assert.Equal(
            System.Reflection.NullabilityState.Nullable,
            new System.Reflection.NullabilityInfoContext()
                .Create(parameter)
                .ReadState);
    }

    private static JsonlDesignConversionLocalArtifactDb ReadTrainingIntakeArtifactDb(
        DesignConversionModelServerEndpointRegistry registry)
    {
        var registration = registry.Resolve(
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointMethod,
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath);
        Assert.True(registration.IsSuccess, registration.ErrorMessage);
        var handler = Assert.IsType<DesignConversionModelServerTrainingIntakeHandler>(
            registration.Value!.Handler.Target);
        var api = Assert.IsType<DesignConversionDpoUsefulnessApi>(
            typeof(DesignConversionModelServerTrainingIntakeHandler)
                .GetField(
                    "_api",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)!
                .GetValue(handler));
        return Assert.IsType<JsonlDesignConversionLocalArtifactDb>(
            typeof(DesignConversionDpoUsefulnessApi)
                .GetField(
                    "_artifactDb",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)!
                .GetValue(api));
    }

    private static DesignConversionModelServerMeasurementGateThresholdConfiguration?
        ReadTrainingIntakeMeasurementGateThresholdConfiguration(
            DesignConversionModelServerEndpointRegistry registry)
    {
        var registration = registry.Resolve(
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointMethod,
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath);
        Assert.True(registration.IsSuccess, registration.ErrorMessage);
        var handler = Assert.IsType<DesignConversionModelServerTrainingIntakeHandler>(
            registration.Value!.Handler.Target);
        return (DesignConversionModelServerMeasurementGateThresholdConfiguration?)
            typeof(DesignConversionModelServerTrainingIntakeHandler)
                .GetField(
                    "_measurementGateThresholdConfiguration",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)!
                .GetValue(handler);
    }

    private static DesignConversionMachine ReadProgramCompositionMachine(
        DesignConversionLocalServiceComposition composition)
    {
        var requestScopedFactory =
            typeof(DesignConversionLocalServiceComposition)
                .GetField(
                    "_requestScopedServiceFactory",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)!
                .GetValue(composition);
        Assert.NotNull(requestScopedFactory);
        return ReadSingleReachableInstance<DesignConversionMachine>(
            requestScopedFactory,
            maxDepth: 8,
            maxNodeCount: 128);
    }

    private static DesignConversionMachine ReadProgramRegistryMachine(
        DesignConversionModelServerEndpointRegistry registry)
    {
        Assert.True(
            Program.TryGetLocalLoopbackRouteRegistryServiceComposition(
                registry,
                out var composition));
        var registryComposition =
            Assert.IsType<DesignConversionLocalServiceComposition>(composition);

        var registration = registry.Resolve(
            DesignConversionModelServerEndpointRegistry.EndpointMethod,
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath);
        Assert.True(registration.IsSuccess, registration.ErrorMessage);
        Assert.Same(
            registryComposition.Service,
            ReadSingleReachableInstance<DesignConversionInternalService>(
                registration.Value!.Handler.Target!,
                maxDepth: 8,
                maxNodeCount: 128));

        return ReadProgramCompositionMachine(registryComposition);
    }

    private static IDesignConversionModelServerArtifactRefResolver?
        ReadProgramRegistryArtifactRefResolver(
            DesignConversionModelServerEndpointRegistry registry)
    {
        var registration = registry.Resolve(
            DesignConversionModelServerEndpointRegistry.EndpointMethod,
            DesignConversionModelServerEndpointRegistry.EndpointPath);
        Assert.True(registration.IsSuccess, registration.ErrorMessage);
        var handler = Assert.IsType<DesignConversionModelServerHttpHandler>(
            registration.Value!.Handler.Target);
        var api = Assert.IsType<DesignConversionModelServerApi>(
            typeof(DesignConversionModelServerHttpHandler)
                .GetField(
                    "_api",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)!
                .GetValue(handler));
        return (IDesignConversionModelServerArtifactRefResolver?)
            typeof(DesignConversionModelServerApi)
                .GetField(
                    "_artifactRefResolver",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)!
                .GetValue(api);
    }

    private static async Task TrainProgramThresholdMachineAsync(
        DesignConversionMachine machine,
        string batchId)
    {
        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, BuildProgramThresholdTrainingBatch(batchId));
        Assert.True(train.IsSuccess, train.ErrorMessage);
    }

    private static LearningBatch<DesignConversionDomain.DesignConversionDomain>
        BuildProgramThresholdTrainingBatch(string batchId)
        => new(
            BatchId: batchId,
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: $"{batchId}:critic",
                    Prompt: $"{{\"unit_id\":\"{DesignConversionDpoUnits.CriticUnit}\",\"source_kind\":\"Figma\",\"source_manifest_hash\":\"program-threshold-manifest\",\"source_identity_hash\":\"program-threshold-identity\",\"source_structure_hash\":\"program-threshold-structure\",\"internal_vector\":[0.2,0.4]}}",
                    ChosenResponse: "{\"html\":\"<main class=\\\"layout-a\\\"></main>\",\"css\":\".layout-a{display:grid;gap:16px}\"}",
                    RejectedResponse: "{\"html\":\"<main class=\\\"layout-b\\\"></main>\",\"css\":\".layout-b{display:block;gap:2px}\"}")
            ],
            SourceIds: ["program-threshold-source"]);

    private static LearningEvaluationSet<DesignConversionDomain.DesignConversionDomain>
        BuildProgramThreeOfFourHeldOutEvaluationSet(string evaluationId)
        => new(
            EvaluationId: evaluationId,
            DomainTag: DesignConversionMachine.DomainTag,
            EvaluationItems:
            [
                BuildProgramHeldOutEvaluationItem($"{evaluationId}:match-1", includeInternalVector: true),
                BuildProgramHeldOutEvaluationItem($"{evaluationId}:match-2", includeInternalVector: true),
                BuildProgramHeldOutEvaluationItem($"{evaluationId}:match-3", includeInternalVector: true),
                BuildProgramHeldOutEvaluationItem($"{evaluationId}:miss-1", includeInternalVector: false)
            ]);

    private static LearningEvaluationItem BuildProgramHeldOutEvaluationItem(
        string itemId,
        bool includeInternalVector)
        => new(
            ItemId: itemId,
            Prompt: BuildProgramHeldOutEvaluationPrompt(includeInternalVector),
            ExpectedResponse: "{\"visual_score\":0.95,\"passed\":true}");

    private static string BuildProgramHeldOutEvaluationPrompt(bool includeInternalVector)
    {
        const string heldOutSourceHash = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var vectorJson = includeInternalVector
            ? ",\"internal_vector\":[0.2,0.4]"
            : string.Empty;

        return $"{{\"unit_id\":\"{DesignConversionDpoUnits.CriticUnit}\",\"source_manifest_hash\":\"sha256:{heldOutSourceHash}\"{vectorJson}}}";
    }

    private static JsonlDesignConversionLocalArtifactDb ReadTrainingIntakeArtifactDbFromUnstartedHost(
        WebApplication app)
        => ReadTrainingIntakeArtifactDb(
            ReadProgramRegistryFromUnstartedHost(app));

    private static DesignConversionModelServerEndpointRegistry
        ReadProgramRegistryFromUnstartedHost(WebApplication app)
    {
        var endpoint = Assert.Single(
            ((IEndpointRouteBuilder)app).DataSources
                .SelectMany(dataSource => dataSource.Endpoints)
                .OfType<RouteEndpoint>(),
            candidate =>
                string.Equals(
                    candidate.RoutePattern.RawText,
                    DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                    StringComparison.Ordinal)
                && candidate.Metadata
                    .GetMetadata<HttpMethodMetadata>()?
                    .HttpMethods
                    .Contains(
                        DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointMethod,
                        StringComparer.OrdinalIgnoreCase) == true);
        var requestDelegate = Assert.IsType<RequestDelegate>(endpoint.RequestDelegate);
        var registry = ReadSingleReachableInstance<DesignConversionModelServerEndpointRegistry>(
            requestDelegate,
            maxDepth: 8,
            maxNodeCount: 128);
        return registry;
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
            Assert.True(
                visited.Count <= maxNodeCount,
                $"Bounded host object-graph traversal exceeded {maxNodeCount} nodes.");

            if (value is T match)
            {
                matches.Add(match);
                continue;
            }

            if (depth >= maxDepth || IsHostGraphTraversalLeaf(value.GetType()))
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

            for (var currentType = value.GetType(); currentType is not null; currentType = currentType.BaseType)
            {
                foreach (var field in currentType.GetFields(
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.DeclaredOnly))
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

    private static bool IsHostGraphTraversalLeaf(Type type)
        => type.IsPrimitive
           || type.IsEnum
           || type.IsValueType
           || type == typeof(string)
           || type == typeof(Type)
           || typeof(System.Reflection.MemberInfo).IsAssignableFrom(type)
           || typeof(System.Reflection.Assembly).IsAssignableFrom(type)
           || typeof(System.Reflection.Module).IsAssignableFrom(type)
           || typeof(System.Collections.IEnumerable).IsAssignableFrom(type);

    private static IDesignConversionDpoPolicyBaseProvider? ReadDpoPolicyBaseProvider(
        JsonlDesignConversionLocalArtifactDb artifactDb)
        => (IDesignConversionDpoPolicyBaseProvider?)
            typeof(JsonlDesignConversionLocalArtifactDb)
                .GetField(
                    "_dpoPolicyBaseProvider",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)!
                .GetValue(artifactDb);

    private static void AssertExecutableDefaultCompositionIsBrowserBackedAndNonDeterministic(
        DesignConversionLocalServiceComposition composition)
    {
        Assert.Equal(
            DesignConversionLocalServiceComposition.BrowserBackedLocalDefaultCompositionKind,
            composition.Descriptor.CompositionKind);
        Assert.IsType<BrowserBackedDesignConversionRenderProvider>(composition.RenderProvider);
        Assert.IsType<ServerOwnedLocalPngComparisonProvider>(composition.ComparisonProvider);
        Assert.IsType<DefaultLocalDesignConversionVisualJudge>(composition.VisualJudge);
        Assert.Equal(
            bool.TrueString,
            composition.Descriptor.BoundaryMetadata["server_owned_local_live_host_authority"]);
        Assert.Contains(
            DesignConversionLocalServiceComposition.ServerOwnedLocalLiveHostAuthorityMarker,
            composition.Descriptor.EvidenceRefs);
        Assert.DoesNotContain(
            DesignConversionLocalServiceComposition.DeterministicCompositionMarker,
            composition.Descriptor.EvidenceRefs);
        Assert.DoesNotContain(
            DeterministicDesignConversionRenderProvider.ProviderKind,
            string.Join("|", composition.Descriptor.EvidenceRefs),
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            DeterministicDesignConversionComparisonProvider.ProviderKind,
            string.Join("|", composition.Descriptor.EvidenceRefs),
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            DeterministicDesignConversionComparisonProvider.BoundaryMarker,
            string.Join("|", composition.Descriptor.EvidenceRefs),
            StringComparison.Ordinal);
    }

    private static void AssertExecutableRouteMetadataIsBrowserBackedAndNonDeterministic(
        IReadOnlyDictionary<string, string> metadata)
    {
        Assert.Equal(
            DesignConversionLocalServiceComposition.BrowserBackedLocalDefaultCompositionKind,
            metadata["composition_kind"]);
        Assert.Equal(bool.TrueString, metadata["browser_renderer_default_wired"]);
        Assert.Equal(bool.TrueString, metadata["browser_renderer_explicit_local_injection_proven"]);
        Assert.Equal(bool.FalseString, metadata["browser_renderer_default_production_wired"]);
        Assert.Equal(
            DesignConversionLocalServiceComposition.BrowserBackedLocalDefaultScope,
            metadata["browser_renderer_default_scope"]);
        Assert.Equal(BrowserBackedDesignConversionRenderProvider.ProviderKind, metadata["render_provider_kind"]);
        Assert.Equal(bool.FalseString, metadata["product_ready"]);
        Assert.Equal(
            bool.TrueString,
            metadata["server_owned_local_live_host_authority"]);

        var refs = metadata["local_service_composition_evidence_refs"];
        Assert.Contains(
            DesignConversionLocalServiceComposition.BrowserBackedCompositionMarker,
            refs,
            StringComparison.Ordinal);
        Assert.Contains(
            DesignConversionLocalServiceComposition.BrowserBackedLocalDefaultCompositionMarker,
            refs,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            DesignConversionLocalServiceComposition.DeterministicCompositionMarker,
            refs,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            DeterministicDesignConversionRenderProvider.ProviderKind,
            refs,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            DeterministicDesignConversionComparisonProvider.ProviderKind,
            refs,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            DeterministicDesignConversionComparisonProvider.BoundaryMarker,
            refs,
            StringComparison.Ordinal);
    }

    private static void AssertExecutableReadyMetadataIsBrowserBackedAndNonDeterministic(
        JsonElement ready)
    {
        Assert.Equal(
            DesignConversionLocalServiceComposition.BrowserBackedLocalDefaultCompositionKind,
            ready.GetProperty("compositionKind").GetString());
        Assert.True(ready.GetProperty("browserBackedLocalDefaultComposition").GetBoolean());
        Assert.True(ready.GetProperty("browserRendererDefaultWired").GetBoolean());
        Assert.Equal(
            DesignConversionLocalServiceComposition.BrowserBackedLocalDefaultScope,
            ready.GetProperty("browserRendererDefaultScope").GetString());
        Assert.False(ready.GetProperty("browserRendererDefaultProductionWired").GetBoolean());
        Assert.Equal(
            BrowserBackedDesignConversionRenderProvider.ProviderKind,
            ready.GetProperty("renderProviderKind").GetString());
        Assert.Equal(
            ServerOwnedLocalPngComparisonProvider.ProviderKind,
            ready.GetProperty("comparisonProviderKind").GetString());
        Assert.True(ready.GetProperty("serverOwnedLocalCompositionAuthority").GetBoolean());
        Assert.True(ready.GetProperty("serverOwnedLocalComparisonProvider").GetBoolean());
        Assert.False(ready.GetProperty("externalJudgeCarrierPresent").GetBoolean());
        Assert.False(ready.TryGetProperty("localPngArtifactRoot", out _));
        Assert.False(ready.TryGetProperty("artifactUploadRoot", out _));
        Assert.False(ready.TryGetProperty("browserArtifactRoot", out _));
        Assert.Equal(
            DesignConversionLocalServiceComposition.LocalVisualJudgeMarker,
            ready.GetProperty("visualJudgeBoundary").GetString());
        Assert.False(ready.GetProperty("deterministicLocalCompositionDefault").GetBoolean());
        Assert.False(ready.GetProperty("deterministicRenderProviderDefault").GetBoolean());
        Assert.False(ready.GetProperty("deterministicComparisonProviderDefault").GetBoolean());
        Assert.Equal("none", ready.GetProperty("deterministicBoundary").GetString());
        Assert.Equal("server_owned_local_chromium_default", ready.GetProperty("browserRuntimeSetup").GetString());
        Assert.Equal(
            "fail_closed_no_deterministic_fallback",
            ready.GetProperty("browserRuntimeUnavailableBehavior").GetString());
        Assert.False(ready.GetProperty("externalVisualAiConfiguredByDefault").GetBoolean());
        Assert.Equal(
            "fail_closed_no_deterministic_fallback",
            ready.GetProperty("comparisonProviderUnavailableBehavior").GetString());

        var json = ready.GetRawText();
        Assert.DoesNotContain(
            DesignConversionLocalServiceComposition.DeterministicLocalCompositionKind,
            json,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            DeterministicDesignConversionRenderProvider.ProviderKind,
            json,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            DeterministicDesignConversionComparisonProvider.ProviderKind,
            json,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            DeterministicDesignConversionComparisonProvider.BoundaryMarker,
            json,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            nameof(DesignConversionExternalVisualAiComparisonProvider),
            json,
            StringComparison.Ordinal);
    }

    private static void AssertFullLocalStartupMetadata(WebApplication app)
    {
        var expectedRoutes = DesignConversionModelServerAspNetHostStartup.FullLocalRouteSet()
            .Concat([DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath])
            .ToArray();
        var mappedEndpoints = app.Services
            .GetRequiredService<EndpointDataSource>()
            .Endpoints
            .OfType<RouteEndpoint>()
            .Select(endpoint => endpoint.Metadata
                .GetMetadata<DesignConversionModelServerAspNetHostStartupMetadata>())
            .Where(metadata => metadata is not null)
            .Select(metadata => metadata!)
            .ToArray();

        Assert.Equal(expectedRoutes.Length, mappedEndpoints.Length);
        Assert.Equal(
            expectedRoutes.Order(StringComparer.Ordinal),
            mappedEndpoints.Select(metadata => metadata.MappedRoute).Order(StringComparer.Ordinal));
        Assert.Contains(
            DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
            mappedEndpoints.Select(metadata => metadata.MappedRoute));
        Assert.Contains(
            DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
            mappedEndpoints.Select(metadata => metadata.MappedRoute));
        Assert.Contains(
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
            mappedEndpoints.Select(metadata => metadata.MappedRoute));
        Assert.All(mappedEndpoints, metadata =>
        {
            Assert.Equal(
                DesignConversionModelServerAspNetHostStartup.HostStartupSurface,
                metadata.StartupSurface);
            Assert.Equal(
                DesignConversionModelServerAspNetHostStartup.HostStartupBoundary,
                metadata.Boundary);
            Assert.Equal(expectedRoutes, metadata.MappedRoutes);
            Assert.True(metadata.LocalOnly);
            Assert.True(metadata.RefsOnly);
            Assert.False(metadata.PublicDeploymentVerified);
            Assert.False(metadata.CrossRepoCallerVerified);
            Assert.False(metadata.ExternalProviderConfigured);
            Assert.False(metadata.ProductReady);
        });
    }

    private static HttpRequestMessage BuildArtifactUploadRequest(
        byte[] png,
        string artifactId,
        int width,
        int height,
        string idempotencyKey)
    {
        var content = new ByteArrayContent(png);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath)
        {
            Content = content
        };

        request.Headers.TryAddWithoutValidation(
            DesignConversionModelServerRouteAdmission.AuthorizationHeaderName,
            "Bearer " + Program.LocalLoopbackArtifactUploadAdmissionToken);
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
            Sha256(png));
        request.Headers.TryAddWithoutValidation(
            DesignConversionModelServerArtifactUploadHttpHandler.WidthHeaderName,
            width.ToString(System.Globalization.CultureInfo.InvariantCulture));
        request.Headers.TryAddWithoutValidation(
            DesignConversionModelServerArtifactUploadHttpHandler.HeightHeaderName,
            height.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return request;
    }

    private static RenderQaImageRecord ToRenderQaImage(
        DesignConversionModelServerArtifactRef artifact,
        RenderQaResolution resolution,
        string label)
        => new(
            ImageId: artifact.ArtifactId,
            Label: label,
            Resolution: resolution,
            Uri: artifact.Uri,
            Sha256: artifact.Sha256,
            Metadata: artifact.Metadata ?? new Dictionary<string, string>());

    private static byte[] BuildRgbaPng(int width, int height, bool blank)
    {
        var raw = new byte[checked(height * ((width * 4) + 1))];
        for (var y = 0; y < height; y++)
        {
            var row = y * ((width * 4) + 1);
            raw[row] = 0;
            for (var x = 0; x < width; x++)
            {
                var offset = row + 1 + (x * 4);
                if (blank)
                {
                    raw[offset] = 255;
                    raw[offset + 1] = 255;
                    raw[offset + 2] = 255;
                    raw[offset + 3] = 255;
                    continue;
                }

                raw[offset] = (byte)(20 + ((x * 31) % 180));
                raw[offset + 1] = (byte)(40 + ((y * 47) % 160));
                raw[offset + 2] = (byte)(90 + (((x + y) * 29) % 150));
                raw[offset + 3] = 255;
            }
        }

        using var png = new MemoryStream();
        png.Write([137, 80, 78, 71, 13, 10, 26, 10]);
        WritePngChunk(png, "IHDR", BuildPngIhdr(width, height));
        WritePngChunk(png, "IDAT", CompressPngScanlines(raw));
        WritePngChunk(png, "IEND", []);
        return png.ToArray();
    }

    private static byte[] BuildPngIhdr(int width, int height)
    {
        var data = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(0, 4), width);
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(4, 4), height);
        data[8] = 8;
        data[9] = 6;
        return data;
    }

    private static byte[] CompressPngScanlines(byte[] raw)
    {
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
            zlib.Write(raw);
        return compressed.ToArray();
    }

    private static void WritePngChunk(Stream stream, string type, byte[] data)
    {
        var typeBytes = Encoding.ASCII.GetBytes(type);
        Span<byte> lengthBytes = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(lengthBytes, data.Length);
        stream.Write(lengthBytes);
        stream.Write(typeBytes);
        stream.Write(data);

        var crcInput = new byte[typeBytes.Length + data.Length];
        Array.Copy(typeBytes, 0, crcInput, 0, typeBytes.Length);
        Array.Copy(data, 0, crcInput, typeBytes.Length, data.Length);
        Span<byte> crcBytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crcBytes, Crc32(crcInput));
        stream.Write(crcBytes);
    }

    private static uint Crc32(ReadOnlySpan<byte> bytes)
    {
        var crc = 0xffffffffu;
        foreach (var b in bytes)
        {
            crc ^= b;
            for (var bit = 0; bit < 8; bit++)
                crc = (crc & 1) == 1 ? (crc >> 1) ^ 0xedb88320u : crc >> 1;
        }

        return ~crc;
    }

    private static string Sha256(byte[] bytes)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string FindRepoFile(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(segments).ToArray());
            if (File.Exists(candidate))
                return candidate;
            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            "Could not find repo file from the test output directory.",
            Path.Combine(segments));
    }

    private sealed class ProgramLocalEvaluatorEnvironmentFixture : IDisposable
    {
        private readonly ModelConfig _modelConfig;

        internal ProgramLocalEvaluatorEnvironmentFixture()
        {
            Root = Path.Combine(
                Path.GetTempPath(),
                "program-local-evaluator-env-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
            BaseCheckpointPath = Path.Combine(Root, "base-checkpoint.bin");
            TokenizerPath = Path.Combine(Root, "tokenizer.bin");
            _modelConfig = new ModelConfig
            {
                VocabSize = 256,
                ContextLen = 8,
                HiddenDim = 8,
                NumLayers = 1,
                NumHeads = 1,
                NumKvHeads = 1,
                FfnHiddenDim = 16,
                RopeTheta = 10000f,
                RmsNormEps = 1e-5f
            };
            new LlmModel(_modelConfig).SaveCheckpoint(BaseCheckpointPath);
            var tokenizer = new BpeTokenizer();
            tokenizer.Train(
                "program local evaluator environment fixture",
                _modelConfig.VocabSize);
            tokenizer.Save(TokenizerPath);
            Directory.CreateDirectory(Path.Combine(Root, "adapter-root"));
            Directory.CreateDirectory(Path.Combine(Root, "cache-root"));
            Directory.CreateDirectory(Path.Combine(Root, "registry-root"));
            RepositoryRoot = Path.Combine(Root, "repository-root");
            CreateRepositorySentinels(RepositoryRoot);
            Directory.CreateDirectory(PairRoot);
        }

        internal string Root { get; }
        private string BaseCheckpointPath { get; }
        private string TokenizerPath { get; }
        private string RepositoryRoot { get; }
        private string PairRoot => Path.Combine(
            RepositoryRoot,
            "training-data",
            "dpo",
            "html-css",
            "learning-unit",
            "v0001",
            "pairs");

        internal IDisposable Apply(string? localEvaluatorSetting)
        {
            var values = new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                [ContinuousLearningModelServerConfigurationLoader.EnabledEnvironmentName] =
                    bool.TrueString,
                [ContinuousLearningModelServerConfigurationLoader.ModelIdEnvironmentName] =
                    "qwen3.5:9b",
                [ContinuousLearningModelServerConfigurationLoader.OllamaManifestDigestEnvironmentName] =
                    new string('d', 64),
                [ContinuousLearningModelServerConfigurationLoader.OllamaModelLayerDigestEnvironmentName] =
                    new string('c', 64),
                [ContinuousLearningModelServerConfigurationLoader.BaseModelIdEnvironmentName] =
                    "registry/qwen3.5-base-9b",
                [ContinuousLearningModelServerConfigurationLoader.BaseModelRevisionEnvironmentName] =
                    "base-checkpoint-program-e2e",
                [ContinuousLearningModelServerConfigurationLoader.MaxTeacherCallsEnvironmentName] =
                    "8",
                [ContinuousLearningModelServerConfigurationLoader.AuditSaltEnvironmentName] =
                    Convert.ToBase64String(Enumerable.Range(1, 32)
                        .Select(static value => checked((byte)value))
                        .ToArray()),
                [ContinuousLearningModelServerConfigurationLoader.LocalRetentionPolicyRefEnvironmentName] =
                    "sha256:" + new string('a', 64),
                [ContinuousLearningModelServerConfigurationLoader.OutputLicenseRefEnvironmentName] =
                    "sha256:" + new string('b', 64),
                [ContinuousLearningModelServerConfigurationLoader.BaseCheckpointEnvironmentName] =
                    BaseCheckpointPath,
                [ContinuousLearningModelServerConfigurationLoader.TokenizerEnvironmentName] =
                    TokenizerPath,
                [ContinuousLearningModelServerConfigurationLoader.AdapterRootEnvironmentName] =
                    Path.Combine(Root, "adapter-root"),
                [ContinuousLearningModelServerConfigurationLoader.CacheRootEnvironmentName] =
                    Path.Combine(Root, "cache-root"),
                [ContinuousLearningModelServerConfigurationLoader.RegistryRootEnvironmentName] =
                    Path.Combine(Root, "registry-root"),
                [ContinuousLearningModelServerConfigurationLoader.RepositoryRootEnvironmentName] =
                    RepositoryRoot,
                [ContinuousLearningModelServerConfigurationLoader.DpoPairRootEnvironmentName] =
                    PairRoot,
                [ContinuousLearningModelServerConfigurationLoader.ModelVocabSizeEnvironmentName] =
                    _modelConfig.VocabSize.ToString(CultureInfo.InvariantCulture),
                [ContinuousLearningModelServerConfigurationLoader.ModelContextLenEnvironmentName] =
                    _modelConfig.ContextLen.ToString(CultureInfo.InvariantCulture),
                [ContinuousLearningModelServerConfigurationLoader.ModelHiddenDimEnvironmentName] =
                    _modelConfig.HiddenDim.ToString(CultureInfo.InvariantCulture),
                [ContinuousLearningModelServerConfigurationLoader.ModelNumLayersEnvironmentName] =
                    _modelConfig.NumLayers.ToString(CultureInfo.InvariantCulture),
                [ContinuousLearningModelServerConfigurationLoader.ModelNumHeadsEnvironmentName] =
                    _modelConfig.NumHeads.ToString(CultureInfo.InvariantCulture),
                [ContinuousLearningModelServerConfigurationLoader.ModelNumKvHeadsEnvironmentName] =
                    _modelConfig.NumKvHeads.ToString(CultureInfo.InvariantCulture),
                [ContinuousLearningModelServerConfigurationLoader.ModelFfnHiddenDimEnvironmentName] =
                    _modelConfig.FfnHiddenDim.ToString(CultureInfo.InvariantCulture),
                [ContinuousLearningModelServerConfigurationLoader.ModelRopeThetaEnvironmentName] =
                    "10000",
                [ContinuousLearningModelServerConfigurationLoader.ModelRmsNormEpsEnvironmentName] =
                    "1E-05",
                [ContinuousLearningModelServerConfigurationLoader.TrainingBatchSizeEnvironmentName] =
                    "1",
                [ContinuousLearningModelServerConfigurationLoader.TrainingSeqLenEnvironmentName] =
                    "8",
                [ContinuousLearningModelServerConfigurationLoader.TrainingMaxLrEnvironmentName] =
                    "0.01",
                [ContinuousLearningModelServerConfigurationLoader.TrainingMinLrEnvironmentName] =
                    "0.001",
                [ContinuousLearningModelServerConfigurationLoader.TrainingWarmupStepsEnvironmentName] =
                    "0",
                [ContinuousLearningModelServerConfigurationLoader.TrainingTotalStepsEnvironmentName] =
                    "10",
                [ContinuousLearningModelServerConfigurationLoader.TrainingGradAccumStepsEnvironmentName] =
                    "1",
                [ContinuousLearningModelServerConfigurationLoader.TrainingGradClipNormEnvironmentName] =
                    "1",
                [ContinuousLearningModelServerConfigurationLoader.TrainingWeightDecayEnvironmentName] =
                    "0",
                [ContinuousLearningModelServerConfigurationLoader.TrainingCheckpointEveryEnvironmentName] =
                    "1",
                [ContinuousLearningModelServerConfigurationLoader.TrainingLogEveryEnvironmentName] =
                    "1"
            };
            if (localEvaluatorSetting is not null)
            {
                values[ContinuousLearningModelServerConfigurationLoader
                    .LocalIndependentPreferenceEvaluatorEnabledEnvironmentName] =
                    localEvaluatorSetting;
            }

            return new ContinuousLearningEnvironmentVariableScope(values);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
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
    }

    private sealed class ContinuousLearningEnvironmentVariableScope : IDisposable
    {
        private readonly IReadOnlyDictionary<string, string?> _previous;
        private int _disposed;

        internal ContinuousLearningEnvironmentVariableScope(
            IReadOnlyDictionary<string, string?> values)
        {
            var variableNames = typeof(ContinuousLearningModelServerConfigurationLoader)
                .GetFields(
                    System.Reflection.BindingFlags.Static |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic)
                .Where(field =>
                    field.FieldType == typeof(string) &&
                    field.Name.EndsWith("EnvironmentName", StringComparison.Ordinal))
                .Select(field => (string?)field.GetValue(null))
                .Where(static value => !string.IsNullOrWhiteSpace(value))
                .Cast<string>()
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            _previous = variableNames.ToDictionary(
                static name => name,
                Environment.GetEnvironmentVariable,
                StringComparer.Ordinal);
            foreach (var name in variableNames)
                Environment.SetEnvironmentVariable(name, null);
            foreach (var pair in values)
                Environment.SetEnvironmentVariable(pair.Key, pair.Value);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;
            foreach (var pair in _previous)
                Environment.SetEnvironmentVariable(pair.Key, pair.Value);
        }
    }

    private static ContinuousLearningTp1DpoConfiguration
        CreateLocalIndependentEvaluatorConfiguration()
    {
        var created = ContinuousLearningTp1DpoConfiguration.Create(
            "qwen-modelserver-local-evaluator-test:9b",
            new string('a', 64),
            new string('b', 64),
            maxTeacherCalls: 8,
            localRetentionPolicyRef:
                "sha256:" + new string('c', 64),
            outputLicenseRef:
                "sha256:" + new string('d', 64),
            serverAuditHashSalt: new byte[32]);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        return created.Value!;
    }

    private static string CreateTemporaryDirectory()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            nameof(DesignConversionModelServerExecutableHostTests),
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static async Task<JsonDocument> WaitForReadyJsonAsync(string readyFile)
    {
        Exception? lastFailure = null;
        for (var attempt = 0; attempt < 100; attempt++)
        {
            if (File.Exists(readyFile))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(readyFile);
                    Assert.DoesNotContain("stale-ready-file", json, StringComparison.Ordinal);
                    return JsonDocument.Parse(json);
                }
                catch (IOException ex)
                {
                    lastFailure = ex;
                }
                catch (JsonException ex)
                {
                    lastFailure = ex;
                }
            }

            await Task.Delay(TimeSpan.FromMilliseconds(50));
        }

        throw new TimeoutException(
            "DesignConversion ModelServer ready file was not written with valid JSON.",
            lastFailure);
    }
}
