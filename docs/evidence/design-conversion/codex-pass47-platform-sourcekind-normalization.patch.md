# codex Pass47 Platform SourceKind Normalization Patch

claim_scope: Patch artifact only for platform DesignConversion sourceKind normalization before FLOW-47 submit.
evidence_scope: Read-only inspection of `C:\Projects\xiigen-platform-dot-net-angular` source files plus sandbox writable-root boundary for this session.
count_claimed: 1 patch artifact.
independent_evidence_count: 1 artifact file.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT.
downgrade_needed: yes. This patch is not applied to the external platform repo in this sandbox.

write_mode: patch_artifact_only

Reason direct write was not used: the active sandbox write roots include `C:\Projects\llm_mvp_core` and temp locations, not `C:\Projects\xiigen-platform-dot-net-angular`. This artifact is the fallback requested by the work order.

## Files To Patch

- `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\DesignConversion\Flow47DesignConversion.cs`
- `C:\Projects\xiigen-platform-dot-net-angular\tests\Xiigen.Platform.Api.Tests\DesignConversionEndpointTests.cs`

## Patch

```diff
diff --git a/src/Xiigen.Platform.Api/DesignConversion/Flow47DesignConversion.cs b/src/Xiigen.Platform.Api/DesignConversion/Flow47DesignConversion.cs
--- a/src/Xiigen.Platform.Api/DesignConversion/Flow47DesignConversion.cs
+++ b/src/Xiigen.Platform.Api/DesignConversion/Flow47DesignConversion.cs
@@
 public sealed record Flow47DesignConversionJobRequest(
     string TenantId,
     string? UserId,
     string IdempotencyKey,
     string? CorrelationId,
@@
     IReadOnlyList<DesignConversionSourceFile> SourceTree,
     IReadOnlyList<DesignConversionScreenshot> Screenshots,
     IReadOnlyDictionary<string, string> Metadata);
+
+internal static class Flow47DesignConversionSourceKind
+{
+    public static string Normalize(string? sourceKind) =>
+        sourceKind switch
+        {
+            null => "html",
+            "figma" => "figma-file",
+            "figma-file" => "figma-file",
+            "canva" => "canva-design",
+            "canva-design" => "canva-design",
+            "image" => "image",
+            "html" => "html",
+            _ => "unknown"
+        };
+}
+
 public sealed record Flow47DesignConversionSubmitRequest(
     string? TenantId,
     string SourceType,
     string SourceUri,
     IReadOnlyList<string>? OutputTargets,
@@
                 "DESIGN_SOURCE_REQUIRED",
                 "At least one of html, css, sourceTree, or screenshots is required."));
         }
+
+        var sourceKind = Flow47DesignConversionSourceKind.Normalize(command.Request.SourceKind);
         var request = new Flow47DesignConversionJobRequest(
             command.TenantId,
             command.UserId,
             command.IdempotencyKey,
             command.CorrelationId,
             command.IfMatch,
-            command.Request.SourceKind ?? "html",
+            sourceKind,
             command.Tier.DesignTier,
             command.Tier.LoopBudget,
             command.Request.Html,
             command.Request.Css,
             command.Request.SourceTree ?? Array.Empty<DesignConversionSourceFile>(),
@@
-    private static string NormalizeSourceType(string sourceKind)
-    {
-        return sourceKind switch
-        {
-            "figma" => "figma-file",
-            "figma-file" => "figma-file",
-            "canva" => "canva-design",
-            "canva-design" => "canva-design",
-            "image" => "image",
-            "html" => "html",
-            _ => "unknown"
-        };
-    }
+    private static string NormalizeSourceType(string sourceKind) =>
+        Flow47DesignConversionSourceKind.Normalize(sourceKind);
+
     private static string ResolveSourceUri(Flow47DesignConversionJobRequest request)
     {
         if (request.Metadata.TryGetValue("sourceUri", out var sourceUri) && !string.IsNullOrWhiteSpace(sourceUri))
diff --git a/tests/Xiigen.Platform.Api.Tests/DesignConversionEndpointTests.cs b/tests/Xiigen.Platform.Api.Tests/DesignConversionEndpointTests.cs
--- a/tests/Xiigen.Platform.Api.Tests/DesignConversionEndpointTests.cs
+++ b/tests/Xiigen.Platform.Api.Tests/DesignConversionEndpointTests.cs
@@
         Assert.Equal(1, fakeFlowClient.SubmitCount);
         Assert.Equal(1, fakeFlowClient.StatusCount);
     }
+
+    [Theory]
+    [InlineData("figma", "figma-file")]
+    [InlineData("canva", "canva-design")]
+    [InlineData("figma-file", "figma-file")]
+    [InlineData("canva-design", "canva-design")]
+    [InlineData("sketch", "unknown")]
+    public async Task SubmitNormalizesPublicSourceKindBeforeFlow47Dispatch(
+        string sourceKind,
+        string expectedSourceKind)
+    {
+        var fakeFlowClient = new TrackingFakeFlow47DesignConversionClient();
+        using var factory = CreateFactoryWithFakeFlowClient(fakeFlowClient);
+        var client = factory.CreateClient();
+        AddAuthorizedHeaders(client, $"source-kind-{sourceKind}");
+
+        var submitResponse = await client.PostAsJsonAsync("/conversion/submit", CreateSubmitRequest(sourceKind));
+
+        Assert.Equal(HttpStatusCode.Accepted, submitResponse.StatusCode);
+        Assert.Equal(expectedSourceKind, fakeFlowClient.LastSubmitRequest?.SourceKind);
+        var submitted = await ReadObjectAsync(submitResponse);
+        Assert.Equal(expectedSourceKind, submitted.RootElement.GetProperty("sourceKind").GetString());
+    }
+
     [Fact]
     public async Task SubmitRequiresStudioRoleHeader()
     {
         var client = _factory.CreateClient();
         client.DefaultRequestHeaders.Add("X-Tenant-Id", "tenant-a");
@@
         Assert.Contains(result.Data.Verdict!.Findings, finding => finding.Contains("fake-core-dcj-contract", StringComparison.Ordinal));
     }
+
+    [Theory]
+    [InlineData("figma", "figma-file", "figma://platform/design-conversion/source")]
+    [InlineData("canva", "canva-design", "canva://platform/design-conversion/source")]
+    public async Task HttpFlowClientPostsCanonicalFlow47SourceTypeForFigmaAndCanvaPayloads(
+        string sourceKind,
+        string expectedSourceType,
+        string expectedSourceUri)
+    {
+        var handler = new CapturingFlow47Handler(request =>
+        {
+            var now = DateTimeOffset.UtcNow;
+            return new
+            {
+                jobId = $"dcj-{sourceKind}",
+                tenantId = "tenant-a",
+                status = "queued",
+                idempotencyKey = $"flow-{sourceKind}",
+                sourceType = expectedSourceType,
+                sourceUri = expectedSourceUri,
+                outputTargets = new[] { "angular" },
+                coreProviderJobId = $"fake-core-{sourceKind}",
+                error = (object?)null,
+                createdAt = now,
+                updatedAt = now
+            };
+        });
+        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://flow47.local") };
+        var flowClient = new HttpFlow47DesignConversionClient(httpClient, new Flow47DesignConversionOptions());
+
+        var result = await flowClient.SubmitAsync(CreateFlowJobRequest($"flow-{sourceKind}", sourceKind));
+
+        Assert.False(result.IsFailure);
+        var body = JsonDocument.Parse(handler.LastRequestBody ?? "{}").RootElement;
+        Assert.Equal(expectedSourceType, body.GetProperty("sourceType").GetString());
+        Assert.Equal(expectedSourceUri, body.GetProperty("sourceUri").GetString());
+        Assert.False(body.TryGetProperty("sourceKind", out _));
+    }
+
     [Fact]
     public async Task Flow47DispatchPreservesSubmitContextWhenHttpStatusOmitsIt()
     {
         var handler = new CapturingFlow47Handler(request =>
         {
@@
-    private static object CreateSubmitRequest() => new
+    private static object CreateSubmitRequest(string sourceKind = "html") => new
     {
         html = "<main>hello</main>",
         css = "main { color: #123456; }",
-        sourceKind = "html",
+        sourceKind,
         sourceTree = new[]
         {
             new { path = "index.html", kind = "html", content = "<main>hello</main>" }
         },
@@
-    private static Flow47DesignConversionJobRequest CreateFlowJobRequest(string idempotencyKey) =>
+    private static Flow47DesignConversionJobRequest CreateFlowJobRequest(
+        string idempotencyKey,
+        string sourceKind = "html") =>
         new(
             "tenant-a",
             "user-a",
             idempotencyKey,
             "corr-contract",
             "etag-1",
-            "html",
+            sourceKind,
             "pro",
             3,
             "<main>hello</main>",
             "main { color: #123456; }",
```

## Tests To Run After Applying

Run from `C:\Projects\xiigen-platform-dot-net-angular`:

```powershell
dotnet test tests\Xiigen.Platform.Api.Tests\Xiigen.Platform.Api.Tests.csproj --filter "FullyQualifiedName~DesignConversionEndpointTests"
git diff --check -- src\Xiigen.Platform.Api\DesignConversion\Flow47DesignConversion.cs tests\Xiigen.Platform.Api.Tests\DesignConversionEndpointTests.cs
```

## Current-Turn Verification

- Direct platform repo write: not run. The external repo is outside this sandbox's writable roots.
- Targeted tests: not run. The patch is not applied to the platform repo in this sandbox, and running the platform test project would write under the external repo.
- Diff check against platform changed files: not run for the same reason.
- Artifact location: `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass47-platform-sourcekind-normalization.patch.md`

## Expected Behavior After Patch

- Public `sourceKind = "figma"` is converted to `figma-file` before `IFlow47DesignConversionClient.SubmitAsync`.
- Public `sourceKind = "canva"` is converted to `canva-design` before `IFlow47DesignConversionClient.SubmitAsync`.
- Canonical `figma-file` and `canva-design` remain unchanged.
- Unknown values follow the existing FLOW-47 submit style and become `unknown` instead of being forwarded as caller-specific values.
