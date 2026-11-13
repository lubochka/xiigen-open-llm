// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

public sealed class ExtensionRuntimeTokenQualityArtifactPathsTests
{
    private const string ArtifactRef = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-04-row-scores.jsonl";

    // Матрица защищает ровно переносимость artifact refs: текущий repo маппится,
    // а stale sibling, внешний repo и traversal остаются красной fail-closed границей.
    [Fact]
    public void ResolveArtifactRef_MapsCurrentCanonicalWindowsRepoRefToRuntimeRepoRoot()
    {
        var resolved = ExtensionRuntimeTokenQualityArtifactPaths.ResolveArtifactRef(
            "C:\\Projects\\llm_mvp_core\\artifacts\\extension-runtime-token-quality-learnability\\quality-semantic-gates-04-row-scores.jsonl",
            "unit current canonical Windows ref");

        Assert.Equal(ExpectedArtifactPath(), resolved);
    }

    [Fact]
    public void ResolveArtifactRef_MapsRelativeArtifactRefUnderRuntimeRepoRoot()
    {
        var resolved = ExtensionRuntimeTokenQualityArtifactPaths.ResolveArtifactRef(
            ArtifactRef,
            "unit relative artifact ref");

        Assert.Equal(ExpectedArtifactPath(), resolved);
    }

    [Fact]
    public void ResolveArtifactRef_KeepsRuntimeNativeAbsoluteRefNormalized()
    {
        var nativeRef = Path.Combine(
            ExtensionRuntimeTokenQualityArtifactPaths.RepoRoot,
            "artifacts",
            "extension-runtime-token-quality-learnability",
            "quality-semantic-gates-04-row-scores.jsonl");

        var resolved = ExtensionRuntimeTokenQualityArtifactPaths.ResolveArtifactRef(
            nativeRef,
            "unit native absolute artifact ref");

        Assert.Equal(ExpectedArtifactPath(), resolved);
    }

    [Fact]
    public void ToCanonicalWindowsRepoRef_WritesStableCurrentRepoArtifactRefFromRuntimePath()
    {
        var runtimePath = Path.Combine(
            ExtensionRuntimeTokenQualityArtifactPaths.RepoRoot,
            "artifacts",
            "extension-runtime-token-quality-learnability",
            "quality-semantic-gates-09-fresh-api-capture",
            "response.json");

        var portableRef = ExtensionRuntimeTokenQualityArtifactPaths.ToCanonicalWindowsRepoRef(
            runtimePath,
            "unit artifact writer ref");

        Assert.Equal(
            "C:\\Projects\\llm_mvp_core\\artifacts\\extension-runtime-token-quality-learnability\\quality-semantic-gates-09-fresh-api-capture\\response.json",
            portableRef);
        Assert.Equal(runtimePath, ExtensionRuntimeTokenQualityArtifactPaths.ResolveArtifactRef(portableRef, "unit roundtrip"));
    }

    [Fact]
    public void ToCanonicalWindowsRepoRef_WritesStableCurrentRepoDocPlanRefFromRuntimePath()
    {
        var runtimePath = Path.Combine(
            ExtensionRuntimeTokenQualityArtifactPaths.RepoRoot,
            "docs",
            "plans",
            "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");

        var portableRef = ExtensionRuntimeTokenQualityArtifactPaths.ToCanonicalWindowsRepoRef(
            runtimePath,
            "unit docs plan writer ref");

        // Проверка нужна отдельно от artifact roundtrip: документы плана тоже должны
        // ссылаться на текущий repo, но не являются artifact-ссылками для ResolveArtifactRef.
        Assert.Equal(
            "C:\\Projects\\llm_mvp_core\\docs\\plans\\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json",
            portableRef);
    }

    [Fact]
    public void ToCanonicalWindowsRepoRef_AcceptsAlreadyCanonicalWindowsRepoRef()
    {
        var canonicalRef = "C:\\Projects\\llm_mvp_core\\docs\\plans\\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json";

        var portableRef = ExtensionRuntimeTokenQualityArtifactPaths.ToCanonicalWindowsRepoRef(
            canonicalRef,
            "unit already canonical Windows ref");

        // Этот случай нужен именно для WSL: уже готовую Windows-ссылку нельзя
        // повторно превращать в относительный путь внутри Linux filesystem.
        Assert.Equal(canonicalRef, portableRef);
    }

    [Fact]
    public void ToCanonicalWindowsRepoRefIfOwned_LeavesExternalWindowsRefUntouched()
    {
        var externalRef = "C:\\Projects\\outside-core\\evidence.json";

        var portableRef = ExtensionRuntimeTokenQualityArtifactPaths.ToCanonicalWindowsRepoRefIfOwned(
            externalRef,
            "unit external Windows ref");

        Assert.Equal(externalRef, portableRef);
    }

    [Fact]
    public void ResolveArtifactRef_RejectsStaleDpoImplementationSiblingRepoRef()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ExtensionRuntimeTokenQualityArtifactPaths.ResolveArtifactRef(
                "C:\\Projects\\llm_mvp_core-DpoImplementation\\artifacts\\extension-runtime-token-quality-learnability\\quality-semantic-gates-04-row-scores.jsonl",
                "unit stale sibling artifact ref"));

        Assert.Contains("старый sibling repo", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("llm_mvp_core-DpoImplementation", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ResolveArtifactRef_RejectsExternalWindowsRepoRef()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ExtensionRuntimeTokenQualityArtifactPaths.ResolveArtifactRef(
                "C:\\Projects\\claude-xiigen-smart-skills\\artifacts\\extension-runtime-token-quality-learnability\\response.json",
                "unit external artifact ref"));

        Assert.Contains("внешний repo", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ResolveArtifactRef_RejectsTraversalOutsideRuntimeRepoRoot()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ExtensionRuntimeTokenQualityArtifactPaths.ResolveArtifactRef(
                "artifacts/extension-runtime-token-quality-learnability/../../../../outside-core.json",
                "unit traversal artifact ref"));

        Assert.Contains("выходит за текущий canonical repo", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ResolveSiblingArtifactRef_DerivesSiblingAfterResponseRefResolution()
    {
        var responsePath = ExtensionRuntimeTokenQualityArtifactPaths.ResolveArtifactRef(
            "C:\\Projects\\llm_mvp_core\\artifacts\\extension-runtime-token-quality-learnability\\qsg09\\response.json",
            "unit response ref for sibling derivation");

        var requestPath = ExtensionRuntimeTokenQualityArtifactPaths.ResolveSiblingArtifactRef(
            responsePath,
            "request.json",
            "unit request sibling");

        Assert.Equal(Path.Combine(Path.GetDirectoryName(responsePath)!, "request.json"), requestPath);
    }

    private static string ExpectedArtifactPath()
        => Path.Combine(
            ExtensionRuntimeTokenQualityArtifactPaths.RepoRoot,
            "artifacts",
            "extension-runtime-token-quality-learnability",
            "quality-semantic-gates-04-row-scores.jsonl");
}
