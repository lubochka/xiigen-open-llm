// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace LlmFromScratch.Tests;

[Collection("ProcessSpawningTests")]
public sealed class ExtensionTokenQualityRunnerHarnessTests
{
    private static readonly TimeSpan ProcessTimeout = TimeSpan.FromSeconds(60);

    [Fact]
    public void RunnerRejectsMalformedJsonlRows()
    {
        using var workspace = TempWorkspace.Create();
        var packPath = workspace.Write("bad.jsonl", "{not-json}\n");

        var result = RunRunner(packPath, workspace.OutputDir);

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("Malformed JSONL row", result.CombinedOutput, StringComparison.Ordinal);
    }

    [Fact]
    public void RunnerRejectsDuplicateScenarioIds()
    {
        using var workspace = TempWorkspace.Create();
        var row = BuildScenario("TQ-DUPLICATE");
        var packPath = workspace.WriteJsonl("duplicate.jsonl", row, row);

        var result = RunRunner(packPath, workspace.OutputDir);

        Assert.Equal(2, result.ExitCode);
        using var report = JsonDocument.Parse(File.ReadAllText(Path.Combine(workspace.OutputDir, "token-runner-report.json")));
        var errors = ReadStringArray(report.RootElement.GetProperty("validation").GetProperty("errors"));
        Assert.Contains("duplicate_scenario_id_TQ-DUPLICATE", errors);
        Assert.False(report.RootElement.GetProperty("token_savings_claim_allowed").GetBoolean());
    }

    [Fact]
    public void RunnerRejectsRowsWithMissingHostOrMode()
    {
        using var workspace = TempWorkspace.Create();
        var badRow = new
        {
            schema = "xiigen.token_quality.scenario.v1",
            scenario_id = "TQ-MISSING-HOST-MODE",
            prompt_text = "Measure the prompt safely.",
            methods_to_run = new[] { "prompt-to-task" }
        };
        var packPath = workspace.WriteJsonl("missing-host-mode.jsonl", badRow);

        var result = RunRunner(packPath, workspace.OutputDir);

        Assert.Equal(2, result.ExitCode);
        using var report = JsonDocument.Parse(File.ReadAllText(Path.Combine(workspace.OutputDir, "token-runner-report.json")));
        var errors = ReadStringArray(report.RootElement.GetProperty("validation").GetProperty("errors"));
        Assert.Contains("row_1_missing_required_field_hosts", errors);
        Assert.Contains("row_1_missing_required_field_modes", errors);
        Assert.Contains("row_1_missing_host", errors);
        Assert.Contains("row_1_missing_mode", errors);
    }

    [Fact]
    public void RunnerBlocksTokenSavingsWhenAdaptiveIsLowerButQualityFailed()
    {
        using var workspace = TempWorkspace.Create();
        var row = BuildScenario(
            "TQ-LOWER-FAILED-QUALITY",
            qualityNonRegressionPassed: false,
            noDeterministicShortcutPassed: true,
            baselineOutputText: "Baseline answer keeps every required gate and records all evidence fields.",
            adaptiveOutputText: "Short answer.");
        var packPath = workspace.WriteJson("failed-quality.json", new[] { row });

        var result = RunRunner(packPath, workspace.OutputDir);

        Assert.Equal(0, result.ExitCode);
        using var pair = ReadFirstJsonlDocument(Path.Combine(workspace.OutputDir, "token-runner-pair-results.jsonl"));
        var root = pair.RootElement;
        Assert.True(root.GetProperty("token_reduction_observed").GetBoolean());
        Assert.False(root.GetProperty("quality_non_regression_passed").GetBoolean());
        Assert.False(root.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.Contains("quality_non_regression_not_proven", ReadStringArray(root.GetProperty("failure_reasons")));
    }

    [Fact]
    public void RunnerAllowsTokenSavingsOnlyForValidQualityAndShortcutPass()
    {
        using var workspace = TempWorkspace.Create();
        var row = BuildScenario(
            "TQ-VALID-PASS",
            qualityNonRegressionPassed: true,
            noDeterministicShortcutPassed: true,
            baselineOutputText: "Baseline answer keeps every required gate and records all evidence fields.",
            adaptiveOutputText: "Short answer.");
        var packPath = workspace.WriteJson("valid-pass.json", new { scenarios = new[] { row } });

        var result = RunRunner(packPath, workspace.OutputDir);

        Assert.Equal(0, result.ExitCode);
        using var report = JsonDocument.Parse(File.ReadAllText(Path.Combine(workspace.OutputDir, "token-runner-report.json")));
        using var pair = ReadFirstJsonlDocument(Path.Combine(workspace.OutputDir, "token-runner-pair-results.jsonl"));
        Assert.True(pair.RootElement.GetProperty("token_reduction_observed").GetBoolean());
        Assert.True(pair.RootElement.GetProperty("quality_non_regression_passed").GetBoolean());
        Assert.True(pair.RootElement.GetProperty("no_deterministic_shortcut_gate").GetProperty("passed").GetBoolean());
        Assert.True(pair.RootElement.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.Equal(1, report.RootElement.GetProperty("token_savings_claim_allowed_count").GetInt32());
    }

    private static object BuildScenario(
        string scenarioId,
        bool qualityNonRegressionPassed = false,
        bool noDeterministicShortcutPassed = false,
        string baselineOutputText = "Baseline placeholder output.",
        string adaptiveOutputText = "Adaptive placeholder output.")
        => new
        {
            schema = "xiigen.token_quality.scenario.v1",
            pack_id = "test-pack",
            scenario_id = scenarioId,
            prompt_text = "Plan token measurement without overclaiming quality.",
            hosts = new[] { "Claude" },
            modes = new[]
            {
                "baseline_common_model_only",
                "adaptive_common_plus_user_model"
            },
            methods_to_run = new[] { "prompt-to-task" },
            scoring_rubric = new
            {
                task_quality = "score >= 4",
                no_deterministic_shortcut = "score >= 4"
            },
            quality_rubric_scores = new
            {
                task_quality = 4,
                no_deterministic_shortcut = noDeterministicShortcutPassed ? 4 : 2
            },
            quality_non_regression_passed = qualityNonRegressionPassed,
            no_deterministic_shortcut_passed = noDeterministicShortcutPassed,
            baseline_output_text = baselineOutputText,
            adaptive_output_text = adaptiveOutputText
        };

    private static RunnerResult RunRunner(string scenarioPackPath, string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        var scriptPath = Path.Combine(FindRepoRoot(), "tools", "token-quality-runner.ps1");
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = FindRepoRoot()
        };
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(scriptPath);
        startInfo.ArgumentList.Add("-ScenarioPackPath");
        startInfo.ArgumentList.Add(scenarioPackPath);
        startInfo.ArgumentList.Add("-OutputDir");
        startInfo.ArgumentList.Add(outputDir);
        startInfo.ArgumentList.Add("-RunId");
        startInfo.ArgumentList.Add("test-run");
        startInfo.ArgumentList.Add("-WriteArtifacts");

        return RunProcessWithTimeoutAsync(startInfo, ProcessTimeout)
            .GetAwaiter()
            .GetResult();
    }

    private static async Task<RunnerResult> RunProcessWithTimeoutAsync(
        ProcessStartInfo startInfo,
        TimeSpan timeout)
    {
        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Process could not start: {DescribeCommand(startInfo)}");
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        using var timeoutCts = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            await KillProcessTreeAsync(process);
            var stdoutTail = await ReadOutputTailAsync(stdoutTask);
            var stderrTail = await ReadOutputTailAsync(stderrTask);
            throw new TimeoutException(
                $"Process timed out after {timeout.TotalSeconds:0} seconds.{Environment.NewLine}" +
                $"Command: {DescribeCommand(startInfo)}{Environment.NewLine}" +
                $"Stdout tail:{Environment.NewLine}{stdoutTail}{Environment.NewLine}" +
                $"Stderr tail:{Environment.NewLine}{stderrTail}");
        }

        var stdout = await stdoutTask;
        var stderr = await stderrTask;
        return new RunnerResult(process.ExitCode, stdout, stderr);
    }

    private static async Task KillProcessTreeAsync(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (NotSupportedException)
                {
                    if (!process.HasExited)
                        process.Kill();
                }
            }
        }
        catch (InvalidOperationException)
        {
            return;
        }

        try
        {
            await Task.WhenAny(process.WaitForExitAsync(), Task.Delay(TimeSpan.FromSeconds(5)));
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static async Task<string> ReadOutputTailAsync(Task<string> outputTask)
    {
        var completedTask = await Task.WhenAny(outputTask, Task.Delay(TimeSpan.FromSeconds(5)));
        if (completedTask != outputTask)
            return "<output was not fully drained after process kill>";

        try
        {
            return Tail(await outputTask);
        }
        catch (Exception ex)
        {
            return $"<output read failed: {ex.Message}>";
        }
    }

    private static string DescribeCommand(ProcessStartInfo startInfo)
    {
        var arguments = startInfo.ArgumentList.Count > 0
            ? string.Join(" ", startInfo.ArgumentList.Select(QuoteArgument))
            : startInfo.Arguments;
        return $"{startInfo.FileName} {arguments}".Trim();
    }

    private static string QuoteArgument(string argument)
    {
        if (argument.Length == 0)
            return "\"\"";

        if (!argument.Any(char.IsWhiteSpace) && !argument.Contains('"'))
            return argument;

        return $"\"{argument.Replace("\"", "\\\"")}\"";
    }

    private static string Tail(string value, int maxChars = 4000)
        => value.Length <= maxChars ? value : value[^maxChars..];

    private static JsonDocument ReadFirstJsonlDocument(string path)
    {
        var line = File.ReadLines(path, Encoding.UTF8).First(line => !string.IsNullOrWhiteSpace(line));
        return JsonDocument.Parse(line);
    }

    private static string[] ReadStringArray(JsonElement array)
        => array.EnumerateArray().Select(item => item.GetString()!).ToArray();

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }

    private sealed record RunnerResult(int ExitCode, string Stdout, string Stderr)
    {
        public string CombinedOutput => Stdout + "\n" + Stderr;
    }

    private sealed class TempWorkspace : IDisposable
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = false
        };

        private readonly string _root;

        private TempWorkspace(string root)
        {
            _root = root;
            OutputDir = Path.Combine(root, "out");
        }

        public string OutputDir { get; }

        public static TempWorkspace Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "xiigen-token-runner-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return new TempWorkspace(root);
        }

        public string Write(string fileName, string text)
        {
            var path = Path.Combine(_root, fileName);
            File.WriteAllText(path, text, new UTF8Encoding(false));
            return path;
        }

        public string WriteJson(string fileName, object value)
            => Write(fileName, JsonSerializer.Serialize(value, JsonOptions) + "\n");

        public string WriteJsonl(string fileName, params object[] rows)
            => Write(fileName, string.Join("\n", rows.Select(row => JsonSerializer.Serialize(row, JsonOptions))) + "\n");

        public void Dispose()
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }
    }
}
