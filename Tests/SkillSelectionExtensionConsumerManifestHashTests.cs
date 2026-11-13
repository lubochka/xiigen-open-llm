using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SmartSkillsDomain;
using Xunit;

namespace LlmFromScratch.Tests;

public sealed class SkillSelectionExtensionConsumerManifestHashTests
{
    [Fact]
    public async Task ExtensionCommonSkillSelectionConsumerRejectsTamperedCheckpointBeforeAssetUse()
    {
        var root = await CreateCommonSkillSelectionAssetRootAsync();
        try
        {
            var checkpointPath = SkillSelectionCheckpointPath(root);
            var manifestPath = SkillSelectionManifestPath(root);
            var manifestHashBeforeTamper = Sha256File(manifestPath);
            File.AppendAllText(checkpointPath, "\n ", Encoding.UTF8);

            var service = new XiigenExtensionApiService(root);
            var response = ProcessSkillSelectionRequest(service);

            Assert.Equal(manifestHashBeforeTamper, Sha256File(manifestPath));
            Assert.Equal("false", response.CheckSetResults["skill_selection_asset_loaded"]);
            Assert.Equal("MANIFEST_CHECKPOINT_HASH_MISMATCH", response.CheckSetResults["skill_selection_load_error"]);
            Assert.DoesNotContain("skill_selection_checkpoint_sha256", response.CheckSetResults.Keys);
            Assert.Contains(
                response.Warnings,
                warning => warning.Contains(
                    "SkillSelection common asset load skipped/failed: MANIFEST_CHECKPOINT_HASH_MISMATCH",
                    StringComparison.Ordinal));
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task ExtensionCommonSkillSelectionConsumerAcceptsManifestMatchedCheckpoint()
    {
        var root = await CreateCommonSkillSelectionAssetRootAsync();
        try
        {
            var checkpointPath = SkillSelectionCheckpointPath(root);
            var expectedHash = Sha256File(checkpointPath);
            var service = new XiigenExtensionApiService(root);
            var response = ProcessSkillSelectionRequest(service);

            Assert.Equal("true", response.CheckSetResults["skill_selection_asset_loaded"]);
            Assert.Equal(expectedHash, response.CheckSetResults["skill_selection_checkpoint_sha256"]);
            Assert.False(response.CheckSetResults.ContainsKey("skill_selection_load_error"));
            Assert.DoesNotContain(
                response.Warnings,
                warning => warning.Contains("SkillSelection common asset load skipped/failed", StringComparison.Ordinal));
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task ExtensionCommonSkillSelectionConsumerAcceptsSnakeCaseManifestHash()
    {
        var root = await CreateCommonSkillSelectionAssetRootAsync();
        try
        {
            var checkpointPath = SkillSelectionCheckpointPath(root);
            var manifestPath = SkillSelectionManifestPath(root);
            var expectedHash = Sha256File(checkpointPath);
            MutateManifest(
                manifestPath,
                manifest =>
                {
                    manifest.Remove("checkpointSha256");
                    manifest["checkpoint_sha256"] = expectedHash;
                });

            var service = new XiigenExtensionApiService(root);
            var response = ProcessSkillSelectionRequest(service);

            Assert.Equal("true", response.CheckSetResults["skill_selection_asset_loaded"]);
            Assert.Equal(expectedHash, response.CheckSetResults["skill_selection_checkpoint_sha256"]);
            Assert.False(response.CheckSetResults.ContainsKey("skill_selection_load_error"));
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Theory]
    [InlineData("missing_hash", "MANIFEST_CHECKPOINT_SHA256_REQUIRED")]
    [InlineData("bad_checkpoint_ref", "CHECKPOINT_NOT_FOUND")]
    [InlineData("bad_manifest_ref", "MANIFEST_REF_MISMATCH")]
    [InlineData("semantic_claim_true", "SEMANTIC_QUALITY_CLAIM_UNSUPPORTED")]
    public async Task ExtensionCommonSkillSelectionConsumerRejectsUnsafeManifestVariants(
        string variant,
        string expectedErrorCode)
    {
        var root = await CreateCommonSkillSelectionAssetRootAsync();
        try
        {
            var manifestPath = SkillSelectionManifestPath(root);
            MutateManifest(
                manifestPath,
                manifest =>
                {
                    switch (variant)
                    {
                        case "missing_hash":
                            manifest.Remove("checkpointSha256");
                            manifest.Remove("checkpoint_sha256");
                            break;
                        case "bad_checkpoint_ref":
                            manifest["checkpointRef"] = "wrong-checkpoint.json";
                            break;
                        case "bad_manifest_ref":
                            manifest["manifestRef"] = "wrong-manifest.json";
                            break;
                        case "semantic_claim_true":
                            manifest["semanticQualityClaimed"] = true;
                            break;
                    }
                });

            var service = new XiigenExtensionApiService(root);
            var response = ProcessSkillSelectionRequest(service);

            Assert.Equal("false", response.CheckSetResults["skill_selection_asset_loaded"]);
            Assert.Equal(expectedErrorCode, response.CheckSetResults["skill_selection_load_error"]);
            Assert.DoesNotContain("skill_selection_checkpoint_sha256", response.CheckSetResults.Keys);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    private static async Task<string> CreateCommonSkillSelectionAssetRootAsync()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "xiigen-skill-selection-consumer-" + Guid.NewGuid().ToString("N"));
        var modelDirectory = Path.Combine(root, "models", "skill-selection", "skill-selection-v1");
        Directory.CreateDirectory(modelDirectory);

        var machine = new SkillSelectionMachine();
        var export = await machine.SaveCheckpointAsync(
            SkillSelectionCheckpointPath(root),
            SkillSelectionManifestPath(root));
        Assert.True(export.IsSuccess, export.ErrorMessage);
        Assert.True(File.Exists(export.Value!.CheckpointRef));
        Assert.True(File.Exists(export.Value.ManifestRef));

        return root;
    }

    private static XiigenExtensionResponse ProcessSkillSelectionRequest(XiigenExtensionApiService service)
    {
        var result = service.ProcessHostExtensionRequest(new XiigenExtensionRequest(
            HostName: "codex",
            HostType: "cli-agent",
            Method: XiigenExtensionApiContract.SkillSelectionMethod,
            UserPromptOrTaskText: "Select the most relevant planning skill for a bounded implementation task.",
            HostFacts: new Dictionary<string, string>
            {
                ["workspace"] = "C:\\Projects\\llm_mvp_core-DpoImplementation",
                ["host_client"] = "thin"
            },
            DebugEnabled: false,
            RequestedSkills: [],
            AvailableSkills: ["plan-review", "code-review"],
            CheckSetInputs: new Dictionary<string, string>
            {
                ["scope"] = "bounded",
                ["external_ai"] = "not_used"
            },
            Provenance: new Dictionary<string, string>
            {
                ["source"] = "skill-selection-consumer-manifest-hash-test"
            },
            CorrelationId: "skill-selection-consumer-manifest-hash-test"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    private static string SkillSelectionCheckpointPath(string root)
        => Path.Combine(root, "models", "skill-selection", "skill-selection-v1", "skill-selection-v1-checkpoint.json");

    private static string SkillSelectionManifestPath(string root)
        => Path.Combine(root, "models", "skill-selection", "skill-selection-v1", "manifest.json");

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static void MutateManifest(string manifestPath, Action<JsonObject> mutate)
    {
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath, Encoding.UTF8))!.AsObject();
        mutate(manifest);
        File.WriteAllText(
            manifestPath,
            manifest.ToJsonString(new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true }),
            Encoding.UTF8);
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }
}
