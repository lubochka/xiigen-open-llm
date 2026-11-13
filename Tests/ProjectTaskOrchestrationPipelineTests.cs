// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using ProjectWikiDomain;

public sealed class ProjectTaskOrchestrationPipelineTests
{
    [Fact]
    public async Task SubmitUserTaskApi_ContentPipelinePrompt_ReturnsHumanRouteFromCore()
    {
        using var active = CreateClientProject("GenieDynamicApp");
        using var linked = CreateServerProject("GenieDynamicServer");
        var api = new SubmitUserTaskApi();

        var result = await api.SubmitAsync(new SubmitUserTaskRequest(
            ContentPipelinePrompt,
            active.Root,
            [linked.Root],
            "xiigen.orchestratePrompt"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var response = result.Value!;
        Assert.Equal("ORCHESTRATED", response.Status);
        Assert.Contains("Конечный ответ XIIGen:", response.HumanRoute.UserAnswer);
        Assert.Contains("Я понял задачу так:", response.HumanRoute.UserAnswer);
        Assert.Contains("Что я вижу в проекте:", response.HumanRoute.UserAnswer);
        Assert.Contains("Как вести работу:", response.HumanRoute.UserAnswer);
        Assert.Contains("Для части \"intake text or audio\"", response.HumanRoute.UserAnswer);
        Assert.Contains("Для части \"convert audio to text\"", response.HumanRoute.UserAnswer);
        Assert.Contains("Какие вопросы XIIGen должен задать project wiki/RAG:", response.HumanRoute.UserAnswer);
        Assert.Contains("Что нельзя замазывать:", response.HumanRoute.UserAnswer);
        Assert.Contains("TaskOrchestrationUnit", response.HumanRoute.UserAnswer);
        Assert.Contains("ProjectUnderstandingUnit", response.HumanRoute.UserAnswer);
        Assert.Contains("HumanRouteComposer", response.HumanRoute.UserAnswer);
        Assert.Contains("JavaScript/TypeScript package evidence found", response.HumanRoute.UserAnswer);
        Assert.Contains("wiki evidence says UI surface: test client can host orchestration review screens.", response.HumanRoute.UserAnswer);
        Assert.Contains(".NET solution/project evidence found", response.HumanRoute.UserAnswer);
        Assert.Contains("backend controller evidence found", response.HumanRoute.UserAnswer);
        Assert.Contains("service layer evidence found", response.HumanRoute.UserAnswer);
    }

    [Fact]
    public async Task PluginOrchestrationAdapter_ContentPipelinePrompt_ExposesHumanRouteAsPluginResult()
    {
        using var active = CreateClientProject("GenieDynamicApp");
        using var linked = CreateServerProject("GenieDynamicServer");
        var adapter = new ProjectWikiPluginOrchestrationAdapter();

        var result = await adapter.OrchestrateAsync(new ProjectWikiPluginOrchestrationRequest(
            ContentPipelinePrompt,
            active.Root,
            [linked.Root],
            "linked_repository_group",
            [],
            "test-request",
            "test-session",
            false,
            false,
            "vs_code",
            "xiigen.orchestratePrompt"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var response = result.Value!;
        Assert.Equal("ORCHESTRATED", response.Status);
        Assert.Equal("linked_repository_group", response.RepoScopeClass);
        Assert.Contains("Конечный ответ XIIGen:", response.AnswerPreviewOrPlanSummary);
        Assert.Contains("Для части \"intake text or audio\"", response.AnswerPreviewOrPlanSummary);
        Assert.Contains("TaskOrchestrationUnit", response.AnswerPreviewOrPlanSummary);
        Assert.Contains("What project evidence is needed before planning: intake text or audio?", response.EvidenceRequirements);
        Assert.Contains("What project evidence is needed before planning: convert audio to text?", response.EvidenceRequirements);
    }

    private const string ContentPipelinePrompt =
        "User wants XIIGen to orchestrate a content generation pipeline: intake text or audio, convert audio to text, improve text, create image plan, video plan, music plan, voice/avatar generation, package results for multiple platforms, create descriptions/captions, schedule publishing, store prompts/assets/feedback, and support user revisions.";

    private static TempProject CreateClientProject(string name)
    {
        var project = new TempProject(name);
        Directory.CreateDirectory(Path.Combine(project.Root, "src", "app"));
        Directory.CreateDirectory(Path.Combine(project.Root, ".github", "workflows"));
        Directory.CreateDirectory(Path.Combine(project.Root, ".xiigen", "wiki-domain", "evidence"));
        File.WriteAllText(Path.Combine(project.Root, "package.json"), "{}");
        File.WriteAllText(Path.Combine(project.Root, ".github", "workflows", "build.yml"), "name: build");
        File.WriteAllText(Path.Combine(project.Root, "src", "app", "app.component.ts"), "export class AppComponent {}");
        File.WriteAllText(Path.Combine(project.Root, ".xiigen", "wiki-domain", "evidence", "client-wiki.md"), "UI surface: test client can host orchestration review screens.");
        return project;
    }

    private static TempProject CreateServerProject(string name)
    {
        var project = new TempProject(name);
        Directory.CreateDirectory(Path.Combine(project.Root, "Controllers"));
        Directory.CreateDirectory(Path.Combine(project.Root, "Services"));
        Directory.CreateDirectory(Path.Combine(project.Root, ".xiigen", "wiki-domain", "evidence"));
        File.WriteAllText(Path.Combine(project.Root, "Server.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\" />");
        File.WriteAllText(Path.Combine(project.Root, "Controllers", "JobsController.cs"), "public sealed class JobsController {}");
        File.WriteAllText(Path.Combine(project.Root, "Services", "JobService.cs"), "public sealed class JobService {}");
        return project;
    }

    private sealed class TempProject : IDisposable
    {
        public TempProject(string name)
        {
            Root = Path.Combine(Path.GetTempPath(), $"{name}-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Root);
        }

        public string Root { get; }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}

public sealed class PluginBoundaryTests
{
    [Fact]
    public void VsCodePlugin_DoesNotBuildClosedOrchestrationRequest()
    {
        var pluginRoot = @"C:\Projects\xiigen_visual_code\src";
        if (!Directory.Exists(pluginRoot))
            return;

        var sourceText = string.Join(
            Environment.NewLine,
            Directory.EnumerateFiles(pluginRoot, "*.ts", SearchOption.AllDirectories)
                .Select(File.ReadAllText));

        Assert.DoesNotContain("XIIGEN_ORCHESTRATION_REQUEST", sourceText);
        Assert.DoesNotContain("WikiOrchestrationRequest", sourceText);
        Assert.DoesNotContain("pluginHost: 'vs_code'", sourceText);
    }
}
