// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using TranslationDomain;

public sealed class TranslationOrdinaryCandidateStoreTests
{
    private const string LoserSource = "Он удалил файл";
    private const string LoserTarget = "He deleted the file";
    private const string WinnerSource = "Он проверил файл";

    [Fact]
    public async Task Candidate_output_is_explicit_and_never_inferred_from_input_corpus()
    {
        var directory = Path.GetFullPath(Path.Combine(Environment.GetEnvironmentVariable("Q01_EVIDENCE_ROOT")
            ?? Path.Combine(RepositoryRoot(), "qa-results"), "q01-output-policy-" + Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(directory);
        var pairs = Path.Combine(directory, "input.jsonl");
        await File.WriteAllTextAsync(pairs, "");
        var options = new TatoebaPilotLoopRunOptions(pairs, TrainSetSize: 1, TestSetSize: 1, MaxCycles: 0);
        Assert.Null(TatoebaPilotLoopRunner.ResolveLostCandidateDirectory(options));
        Assert.Equal(Path.Combine(directory, "lost-candidates"),
            TatoebaPilotLoopRunner.ResolveLostCandidateDirectory(options with { CheckpointPath = Path.Combine(directory, "checkpoint.json") }));
        var explicitOutput = Path.Combine(directory, "chosen-output");
        Assert.Equal(explicitOutput, TatoebaPilotLoopRunner.ResolveLostCandidateDirectory(options with { LostCandidateDirectory = explicitOutput }));
        await TatoebaPilotLoopRunner.RunAsync(options); // Пустые rows: Train/Load/ordinary readout=0 по этому пути.
        Assert.Equal(new[] { pairs }, Directory.GetFiles(directory));
        Assert.Empty(Directory.GetDirectories(directory));
    }

    [Theory]
    [InlineData("fresh")]
    [InlineData("disk")]
    [InlineData("cache")]
    public async Task Public_runner_assigns_a_durable_store_to_each_machine_entry(string entry)
    {
        var root = RepositoryRoot();
        var evidenceRoot = Environment.GetEnvironmentVariable("Q01_EVIDENCE_ROOT")
            ?? Path.Combine(root, "qa-results", "q01-concurrency-" + Guid.NewGuid().ToString("N"));
        var directory = Path.GetFullPath(Path.Combine(evidenceRoot, "composition-" + entry + "-" + Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(directory); // Долговечное свидетельство, не удаляем в finally.
        var pairs = Path.Combine(directory, "pairs.jsonl");
        await File.WriteAllTextAsync(pairs, ""); // Нет скрытого обучения в подготовке RunAsync.
        var checkpoint = Path.Combine(directory, "runner-checkpoint.json");
        var cache = new TranslationCheckpointCache();
        TranslationMachine? seeded = null;
        if (entry != "fresh")
        {
            seeded = new TranslationMachine();
            var loop = new TatoebaPilotLoopCheckpoint(pairs, 1, 1, 0, 0, 0, 0.78, [], []);
            Assert.True((await seeded.SaveCheckpointAsync(checkpoint, loop)).IsSuccess);
            if (entry == "cache")
            {
                // Старое назначение не должно остаться у повторно используемой машины.
                Assert.True(seeded.UseLostCandidateStore(new TranslationFileLostCandidateStore("")).IsSuccess);
                cache.Store(checkpoint, new TranslationMachineLoadResult(seeded, loop));
            }
        }
        var report = await TatoebaPilotLoopRunner.RunAsync(new TatoebaPilotLoopRunOptions(
            pairs, TrainSetSize: 1, TestSetSize: 1, MaxCycles: 0, CheckpointPath: checkpoint, Cache: cache));
        Assert.Equal(entry != "fresh", report.CheckpointLoaded);
        Assert.True(cache.TryGet(checkpoint, out var loaded));
        var machine = loaded!.Machine;
        if (entry == "cache") Assert.Same(seeded, machine);
        Assert.Empty(machine.ExportCheckpoint().Value!.TrainingHistory);
        var baseVersion = machine.CurrentStateVersion;
        machine.BeforeConditionalPublishBarrier = () =>
        {
            machine.BeforeConditionalPublishBarrier = null;
            var winner = machine.TrainStructuredBatchAsync(Batch("ordinary-winner", WinnerSource,
                "He checked the file", "He checks the file [WRONG: tense]")).GetAwaiter().GetResult();
            Assert.True(winner.IsSuccess, winner.ErrorMessage);
        };
        var refused = await machine.TrainStructuredBatchAsync(Batch("ordinary-loser", LoserSource,
            LoserTarget, "He deletes the file [WRONG: tense]"));
        Assert.False(refused.IsSuccess);
        Assert.Equal(TranslationMachine.StateVersionChangedErrorCode, refused.ErrorCode);
        Assert.Equal(baseVersion + 1, machine.CurrentStateVersion);
        var artifactPath = Assert.Single(Directory.GetFiles(Path.Combine(directory, "lost-candidates"), "*.json"));
        var bytes = await File.ReadAllBytesAsync(artifactPath);
        var hash = Convert.ToHexString(SHA256.HashData(bytes));
        Assert.Contains(hash, refused.ErrorMessage);
        var artifact = JsonSerializer.Deserialize<TranslationLostCandidateArtifact>(bytes, TranslationCheckpointJsonOptions.Standard)!;
        Assert.Equal("ordinary-loser", artifact.CandidateBatchId);
        Assert.Contains(artifact.CandidateCheckpoint.KnnStore.Entries, x => x.Source == LoserSource && x.Target == LoserTarget);
        Assert.DoesNotContain(artifact.CandidateCheckpoint.KnnStore.Entries, x => x.Source == WinnerSource);
        var live = machine.ExportCheckpoint().Value!;
        Assert.Single(live.TrainingHistory);
        Assert.Contains(live.KnnStore.Entries, x => x.Source == WinnerSource);
        Assert.DoesNotContain(live.KnnStore.Entries, x => x.Source == LoserSource);

        await File.WriteAllTextAsync(Path.Combine(directory, "composition-receipt.json"), JsonSerializer.Serialize(new {
            entry, parent_pid = Environment.ProcessId, base_version = baseVersion,
            current_version = machine.CurrentStateVersion, artifact_path = artifactPath, artifact_sha256 = hash,
            candidate_state_identity = artifact.CandidateStateIdentity,
            source_derived_structured_train_calls = 2, source_derived_unit_lesson_calls = 6,
            runner_initial_training_rows = 0, runner_cycles = report.Cycles.Count
        }, new JsonSerializerOptions { WriteIndented = true }));
        if (entry == "fresh") await ReadInSeparateProcess(root, artifactPath, hash, directory);
    }

    private static async Task ReadInSeparateProcess(string root, string artifact, string hash, string directory)
    {
        var configuration = typeof(TranslationOrdinaryCandidateStoreTests).Assembly
            .GetCustomAttribute<AssemblyConfigurationAttribute>()!.Configuration;
        var reader = Environment.GetEnvironmentVariable("Q01_CANDIDATE_READER_DLL")
            ?? Path.Combine(root, "Tests.Tools", "TranslationLostCandidateReader", "bin", configuration, "net9.0", "TranslationLostCandidateReader.dll");
        Assert.True(File.Exists(reader), "Reader dependency was not built: " + reader);
        var output = Path.Combine(directory, "fresh-process");
        var start = new ProcessStartInfo("dotnet") { UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true, WorkingDirectory = root };
        foreach (var argument in new[] { reader, artifact, hash, LoserSource, output }) start.ArgumentList.Add(argument);
        using var process = Process.Start(start)!;
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        try { await process.WaitForExitAsync(deadline.Token); }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            await File.WriteAllTextAsync(Path.Combine(directory, "reader-timeout.txt"), "PID=" + process.Id);
            throw;
        }
        var stdout = await stdoutTask;
        var stderr = await stderrTask;
        await File.WriteAllTextAsync(Path.Combine(directory, "reader.stdout.log"), stdout);
        await File.WriteAllTextAsync(Path.Combine(directory, "reader.stderr.log"), stderr);
        await File.WriteAllTextAsync(Path.Combine(directory, "reader-process.json"), JsonSerializer.Serialize(new {
            pid = process.Id, parent_pid = Environment.ProcessId, exit = process.ExitCode,
            start_utc = process.StartTime.ToUniversalTime(), end_utc = process.ExitTime.ToUniversalTime(),
            command = start.FileName, arguments = start.ArgumentList.ToArray(),
            reader_sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(reader)))
        }, new JsonSerializerOptions { WriteIndented = true }));
        Assert.Equal(0, process.ExitCode);
        using var receipt = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(output, "reader-receipt.json")));
        Assert.NotEqual(Environment.ProcessId, receipt.RootElement.GetProperty("pid").GetInt32());
        Assert.Equal(LoserTarget, receipt.RootElement.GetProperty("actual_answer").GetString());
        Assert.Equal(1, receipt.RootElement.GetProperty("load_calls").GetInt32());
        Assert.Equal(1, receipt.RootElement.GetProperty("ordinary_calls").GetInt32());
        Assert.Equal(hash, receipt.RootElement.GetProperty("artifact_sha256").GetString());
    }

    private static TranslationLearningBatch Batch(string id, string source, string target, string rejected)
        => TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(id,
            [new TranslationTrainReadyPair(id + "-p01", "simulation", "RU_EN", "tense", "Переведи",
                source, target, rejected, "train_ready", [], VerifiedRuleTags: ["TENSE_PAST"])],
            signatureMode: TranslationSignatureMode.ExactPair);

    private static string RepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "Tests", "LlmFromScratch.Tests.csproj"))) return directory.FullName;
        throw new DirectoryNotFoundException("Core repository root was not found.");
    }
}
