// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using PretrainingRunner;
using TrainingLoop;
using TrainingLoop.Optimizers;
using TransformerModel;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

public sealed class PretrainingRunnerTests
{
    [Fact]
    public async Task DryRun_ValidatesWithoutCreatingClaimOrRunDirectory()
    {
        using var layout = new RunnerLayout();
        var result = await InvokeAsync(layout, train: false);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(Program.ValidatedToken + Environment.NewLine, result.Stdout);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.False(File.Exists(layout.ClaimPath));
        Assert.False(Directory.Exists(layout.RunDirectory));
    }

    [Theory]
    [InlineData("empty")]
    [InlineData("unknown")]
    [InlineData("positional")]
    [InlineData("missing-value")]
    [InlineData("missing-required")]
    [InlineData("duplicate-value")]
    [InlineData("duplicate-train")]
    public async Task Arguments_StrictlyRejectMalformedForms(string scenario)
    {
        using var layout = new RunnerLayout();
        var valid = layout.Arguments(train: false).ToList();
        string[] arguments = scenario switch
        {
            "empty" => [],
            "unknown" => [.. valid, "--unknown"],
            "positional" => [.. valid, "stray"],
            "missing-value" => ["--config"],
            "missing-required" => valid
                .Where(value => value != "--run-id" && value != layout.RunId)
                .ToArray(),
            "duplicate-value" => [.. valid, "--config", layout.ConfigPath],
            "duplicate-train" => [.. valid, "--train", "--train"],
            _ => throw new InvalidOperationException(),
        };

        var result = await InvokeRawAsync(arguments);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal(Program.InvalidArgumentsToken + Environment.NewLine, result.Stderr);
        Assert.False(File.Exists(layout.ClaimPath));
        Assert.False(Directory.Exists(layout.RunDirectory));
    }

    [Fact]
    public async Task Arguments_AcceptValuedOptionsInAnyOrder()
    {
        using var layout = new RunnerLayout();
        string[] arguments =
        [
            "--run-id", layout.RunId,
            "--output-root", layout.OutputRoot,
            "--config", layout.ConfigPath,
            "--data-dir", layout.DataDirectory,
        ];

        var result = await InvokeRawAsync(arguments);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(Program.ValidatedToken + Environment.NewLine, result.Stdout);
    }

    [Theory]
    [InlineData("comment")]
    [InlineData("trailing-comma")]
    [InlineData("duplicate")]
    [InlineData("unknown")]
    [InlineData("alternate-case")]
    [InlineData("null")]
    [InlineData("missing")]
    [InlineData("checkpoint-dir")]
    [InlineData("wrong-version")]
    public async Task JsonContract_RejectsNonExactDocuments(string scenario)
    {
        using var layout = new RunnerLayout();
        var json = layout.ValidJson;
        json = scenario switch
        {
            "comment" => json + "// comment",
            "trailing-comma" => json[..^1] + ",}",
            "duplicate" => json.Replace(
                "\"schemaVersion\":1",
                "\"schemaVersion\":1,\"schemaVersion\":1",
                StringComparison.Ordinal),
            "unknown" => json.Replace(
                "\"schemaVersion\":1",
                "\"schemaVersion\":1,\"extra\":0",
                StringComparison.Ordinal),
            "alternate-case" => json.Replace(
                "\"VocabSize\":16",
                "\"vocabSize\":16",
                StringComparison.Ordinal),
            "null" => json.Replace(
                "\"VocabSize\":16",
                "\"VocabSize\":null",
                StringComparison.Ordinal),
            "missing" => json.Replace(
                "\"NumLayers\":1,",
                string.Empty,
                StringComparison.Ordinal),
            "checkpoint-dir" => json.Replace(
                "\"LogEvery\":100",
                "\"LogEvery\":100,\"CheckpointDir\":\"caller\"",
                StringComparison.Ordinal),
            "wrong-version" => json.Replace(
                "\"schemaVersion\":1",
                "\"schemaVersion\":2",
                StringComparison.Ordinal),
            _ => throw new InvalidOperationException(),
        };
        File.WriteAllText(layout.ConfigPath, json);

        var result = await InvokeAsync(layout, train: false);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal(Program.InvalidConfigToken + Environment.NewLine, result.Stderr);
        Assert.False(File.Exists(layout.ClaimPath));
        Assert.False(Directory.Exists(layout.RunDirectory));
    }

    [Theory]
    [InlineData("\"HiddenDim\":4", "\"HiddenDim\":0")]
    [InlineData("\"NumHeads\":1", "\"NumHeads\":3")]
    [InlineData("\"SeqLen\":2", "\"SeqLen\":5")]
    [InlineData("\"MaxLr\":0.001", "\"MaxLr\":0")]
    [InlineData("\"MinLr\":0.0001", "\"MinLr\":0.002")]
    [InlineData("\"WarmupSteps\":0", "\"WarmupSteps\":2")]
    [InlineData("\"GradAccumSteps\":1", "\"GradAccumSteps\":0")]
    [InlineData("\"WeightDecay\":0.125", "\"WeightDecay\":-1")]
    public async Task Configuration_RejectsInvalidCrossFieldAndNumericValues(
        string current,
        string replacement)
    {
        using var layout = new RunnerLayout();
        File.WriteAllText(
            layout.ConfigPath,
            layout.ValidJson.Replace(current, replacement, StringComparison.Ordinal));

        var result = await InvokeAsync(layout, train: false);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(Program.InvalidConfigToken + Environment.NewLine, result.Stderr);
        Assert.False(File.Exists(layout.ClaimPath));
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("CON")]
    [InlineData("com1.txt")]
    [InlineData("run.")]
    [InlineData("run ")]
    [InlineData("a/b")]
    [InlineData("a\\b")]
    [InlineData("a:b")]
    public async Task Paths_RejectUnsafeRunIdentifiers(string runId)
    {
        using var layout = new RunnerLayout(runId);
        var result = await InvokeAsync(layout, train: false);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(Program.InvalidPathToken + Environment.NewLine, result.Stderr);
        Assert.Empty(Directory.EnumerateFileSystemEntries(layout.OutputRoot));
    }

    [Fact]
    public async Task Paths_RejectOverlappingDataAndOutput()
    {
        using var layout = new RunnerLayout();
        var arguments = layout.Arguments(train: false);
        arguments[Array.IndexOf(arguments, layout.DataDirectory)] = layout.OutputRoot;

        var result = await InvokeRawAsync(arguments);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(Program.InvalidPathToken + Environment.NewLine, result.Stderr);
        Assert.Empty(Directory.EnumerateFileSystemEntries(layout.OutputRoot));
    }

    [Fact]
    public async Task Paths_RejectFileSystemRootAsOutput()
    {
        using var layout = new RunnerLayout();
        var arguments = layout.Arguments(train: false);
        arguments[Array.IndexOf(arguments, layout.OutputRoot)] =
            Path.GetPathRoot(layout.OutputRoot)!;

        var result = await InvokeRawAsync(arguments);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(Program.InvalidPathToken + Environment.NewLine, result.Stderr);
    }

    [Theory]
    [InlineData("unc")]
    [InlineData("device")]
    [InlineData("ads")]
    public async Task Paths_RejectUncDeviceNamespaceAndAds(string scenario)
    {
        using var layout = new RunnerLayout();
        var arguments = layout.Arguments(train: true);
        arguments[Array.IndexOf(arguments, layout.ConfigPath)] = scenario switch
        {
            "unc" => @"\\server\share\runner.json",
            "device" => @"\\?\C:\runner.json",
            "ads" => layout.ConfigPath + ":stream",
            _ => throw new InvalidOperationException(),
        };

        var result = await InvokeRawAsync(arguments);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal(Program.InvalidPathToken + Environment.NewLine, result.Stderr);
        Assert.False(File.Exists(layout.ClaimPath));
        Assert.False(Directory.Exists(layout.RunDirectory));
    }

    [ReparsePointFactPR]
    public async Task Paths_RejectReparsePointDataDirectory()
    {
        using var layout = new RunnerLayout();
        var realData = Path.Combine(layout.Root, "real-data");
        Directory.CreateDirectory(realData);
        File.Move(
            Path.Combine(layout.DataDirectory, "tokens.npy"),
            Path.Combine(realData, "tokens.npy"));
        Directory.Delete(layout.DataDirectory);
        Directory.CreateSymbolicLink(layout.DataDirectory, realData);

        try
        {
            var result = await InvokeAsync(layout, train: false);

            Assert.Equal(2, result.ExitCode);
            Assert.Equal(Program.InvalidPathToken + Environment.NewLine, result.Stderr);
            Assert.False(File.Exists(layout.ClaimPath));
        }
        finally
        {
            Directory.Delete(layout.DataDirectory);
            Directory.CreateDirectory(layout.DataDirectory);
        }
    }

    [Fact]
    public async Task DataValidation_RequiresUsableLowercaseNpyShard()
    {
        using var layout = new RunnerLayout();
        File.Move(
            Path.Combine(layout.DataDirectory, "tokens.npy"),
            Path.Combine(layout.DataDirectory, "tokens.NPY"));

        var result = await InvokeAsync(layout, train: false);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(Program.InvalidDataToken + Environment.NewLine, result.Stderr);
        Assert.False(File.Exists(layout.ClaimPath));
    }

    [Fact]
    public async Task DataValidation_RejectsOutOfVocabularyTokensBeforeClaim()
    {
        using var layout = new RunnerLayout();
        NpyTestFile.Write(
            Path.Combine(layout.DataDirectory, "tokens.npy"),
            1, 2, 99, 4);

        var result = await InvokeAsync(layout, train: true);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(Program.InvalidDataToken + Environment.NewLine, result.Stderr);
        Assert.False(File.Exists(layout.ClaimPath));
        Assert.False(Directory.Exists(layout.RunDirectory));
    }

    [Fact]
    public async Task ExistingRunLeaf_IsCollisionAndIsNeverDeleted()
    {
        using var layout = new RunnerLayout();
        Directory.CreateDirectory(layout.RunDirectory);
        var sentinel = Path.Combine(layout.RunDirectory, "sentinel.txt");
        File.WriteAllText(sentinel, "keep");

        var result = await InvokeAsync(layout, train: true);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(Program.CollisionToken + Environment.NewLine, result.Stderr);
        Assert.Equal("keep", File.ReadAllText(sentinel));
        Assert.False(File.Exists(layout.ClaimPath));
    }

    [Fact]
    public async Task ExistingClaimMarker_IsCollisionAndIsNeverOverwritten()
    {
        using var layout = new RunnerLayout();
        File.WriteAllText(layout.ClaimPath, "existing owner");

        var result = await InvokeAsync(layout, train: true);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(Program.CollisionToken + Environment.NewLine, result.Stderr);
        Assert.Equal("existing owner", File.ReadAllText(layout.ClaimPath));
        Assert.False(Directory.Exists(layout.RunDirectory));
    }

    [Fact]
    public async Task RuntimeFailure_RetainsClaimAndRunDirectory()
    {
        using var layout = new RunnerLayout();
        var dependencies = RunnerDependencies.Production with
        {
            TrainAsync = (_, _, _, _, _) =>
                throw new InvalidOperationException("sensitive runtime detail"),
        };

        var result = await InvokeAsync(layout, train: true, dependencies);

        Assert.Equal(3, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal(Program.RuntimeFailedToken + Environment.NewLine, result.Stderr);
        Assert.True(File.Exists(layout.ClaimPath));
        Assert.True(Directory.Exists(layout.CheckpointDirectory));
        Assert.DoesNotContain("sensitive", result.Stderr, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PostClaimSetupFailure_IsRuntimeFailureAndRetainsClaim()
    {
        using var layout = new RunnerLayout();
        var dependencies = RunnerDependencies.Production with
        {
            AfterClaimCreated = paths =>
                File.WriteAllText(paths.RunDirectory, "collision after claim"),
        };

        var result = await InvokeAsync(layout, train: true, dependencies);

        Assert.Equal(3, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal(Program.RuntimeFailedToken + Environment.NewLine, result.Stderr);
        Assert.True(File.Exists(layout.ClaimPath));
        Assert.True(File.Exists(layout.RunDirectory));
    }

    [Fact]
    public async Task ClaimMarkerPersistenceFailure_IsRuntimeAndRetainsMarker()
    {
        using var layout = new RunnerLayout();
        var dependencies = RunnerDependencies.Production with
        {
            PersistClaimMarker = claim =>
            {
                claim.WriteByte((byte)'P');
                throw new IOException("injected marker persistence failure");
            },
        };

        var result = await InvokeAsync(layout, train: true, dependencies);

        Assert.Equal(3, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal(Program.RuntimeFailedToken + Environment.NewLine, result.Stderr);
        Assert.True(File.Exists(layout.ClaimPath));
        Assert.True(new FileInfo(layout.ClaimPath).Length > 0);
        Assert.False(Directory.Exists(layout.RunDirectory));
    }

    [Fact]
    public async Task ObservedCancellationAfterClaim_Returns130AndRetainsArtifacts()
    {
        using var layout = new RunnerLayout();
        using var cancellation = new CancellationTokenSource();
        var dependencies = RunnerDependencies.Production with
        {
            TrainAsync = (_, _, _, _, token) =>
            {
                cancellation.Cancel();
                token.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            },
        };

        var result = await InvokeAsync(
            layout,
            train: true,
            dependencies,
            cancellation.Token);

        Assert.Equal(130, result.ExitCode);
        Assert.Equal(Program.CancelledToken + Environment.NewLine, result.Stderr);
        Assert.True(File.Exists(layout.ClaimPath));
        Assert.True(Directory.Exists(layout.RunDirectory));
    }

    [Fact]
    public async Task CancellationObservedAfterFreshLoad_Returns130WithoutSuccess()
    {
        using var layout = new RunnerLayout();
        using var cancellation = new CancellationTokenSource();
        var dependencies = RunnerDependencies.Production with
        {
            TrainAsync = (_, model, _, _, _) =>
            {
                model.SaveCheckpoint(layout.FinalCheckpoint);
                return Task.CompletedTask;
            },
            GetRunState = _ => TrainerRunState.Completed,
            GetCompletedSteps = _ => 1,
            LoadCheckpoint = (model, path) =>
            {
                model.LoadCheckpoint(path);
                cancellation.Cancel();
            },
        };

        var result = await InvokeAsync(
            layout,
            train: true,
            dependencies,
            cancellation.Token);

        Assert.Equal(130, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal(Program.CancelledToken + Environment.NewLine, result.Stderr);
        Assert.True(File.Exists(layout.ClaimPath));
        Assert.True(Directory.Exists(layout.RunDirectory));
    }

    [Fact]
    public async Task FatalClrException_IsRethrownWithoutTokenization()
    {
        using var layout = new RunnerLayout();
        var dependencies = RunnerDependencies.Production with
        {
            CreateDataLoader = (_, _, _) => throw new BadImageFormatException(),
        };

        await Assert.ThrowsAsync<BadImageFormatException>(
            () => InvokeAsync(layout, train: false, dependencies));

        Assert.False(File.Exists(layout.ClaimPath));
        Assert.False(Directory.Exists(layout.RunDirectory));
    }

    [Fact]
    public async Task NestedInnerFatalException_IsRethrownWithoutTokenization()
    {
        using var layout = new RunnerLayout();
        var expected = new InvalidOperationException(
            "outer",
            new BadImageFormatException("fatal inner"));
        var dependencies = RunnerDependencies.Production with
        {
            CreateDataLoader = (_, _, _) => throw expected,
        };

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(
            () => InvokeAsync(layout, train: false, dependencies));

        Assert.Same(expected, thrown);
        Assert.False(File.Exists(layout.ClaimPath));
    }

    [Fact]
    public async Task AggregateContainingFatalException_IsRethrownWithoutTokenization()
    {
        using var layout = new RunnerLayout();
        var expected = new AggregateException(
            new InvalidOperationException("ordinary"),
            new BadImageFormatException("fatal aggregate member"));
        var dependencies = RunnerDependencies.Production with
        {
            CreateDataLoader = (_, _, _) => throw expected,
        };

        var thrown = await Assert.ThrowsAsync<AggregateException>(
            () => InvokeAsync(layout, train: false, dependencies));

        Assert.Same(expected, thrown);
        Assert.False(File.Exists(layout.ClaimPath));
    }

    [Fact]
    public async Task TrainingConfiguration_PropagatesEveryFieldAndWeightDecay()
    {
        using var layout = new RunnerLayout();
        ModelConfig? observedModel = null;
        TrainingConfig? observedTraining = null;
        float? observedWeightDecay = null;
        var dependencies = RunnerDependencies.Production with
        {
            CreateModel = configuration =>
            {
                observedModel = configuration;
                return new LlmModel(configuration);
            },
            CreateOptimizer = weightDecay =>
            {
                observedWeightDecay = weightDecay;
                return new AdamW(weightDecay: weightDecay);
            },
            CreateTrainer = configuration =>
            {
                observedTraining = configuration;
                return new Trainer(configuration);
            },
            TrainAsync = (_, model, _, _, _) =>
            {
                model.SaveCheckpoint(layout.FinalCheckpoint);
                return Task.CompletedTask;
            },
            GetRunState = _ => TrainerRunState.Completed,
            GetCompletedSteps = _ => 1,
        };

        var result = await InvokeAsync(layout, train: true, dependencies);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(Program.CompletedToken + Environment.NewLine, result.Stdout);
        Assert.NotNull(observedModel);
        Assert.Equal(16, observedModel.VocabSize);
        Assert.Equal(4, observedModel.ContextLen);
        Assert.Equal(4, observedModel.HiddenDim);
        Assert.Equal(1, observedModel.NumLayers);
        Assert.Equal(1, observedModel.NumHeads);
        Assert.Equal(1, observedModel.NumKvHeads);
        Assert.Equal(8, observedModel.FfnHiddenDim);
        Assert.Equal(10_000f, observedModel.RopeTheta);
        Assert.Equal(1e-5f, observedModel.RmsNormEps);
        Assert.NotNull(observedTraining);
        Assert.Equal(1, observedTraining.BatchSize);
        Assert.Equal(2, observedTraining.SeqLen);
        Assert.Equal(0.001f, observedTraining.MaxLr);
        Assert.Equal(0.0001f, observedTraining.MinLr);
        Assert.Equal(0, observedTraining.WarmupSteps);
        Assert.Equal(1, observedTraining.TotalSteps);
        Assert.Equal(1, observedTraining.GradAccumSteps);
        Assert.Equal(1f, observedTraining.GradClipNorm);
        Assert.Equal(0.125f, observedTraining.WeightDecay);
        Assert.Equal(1, observedTraining.CheckpointEvery);
        Assert.Equal(100, observedTraining.LogEvery);
        Assert.Equal(layout.CheckpointDirectory, observedTraining.CheckpointDir);
        Assert.Equal(0.125f, observedWeightDecay);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("empty")]
    [InlineData("outside")]
    [InlineData("unloadable")]
    [InlineData("not-completed")]
    [InlineData("wrong-step-count")]
    public async Task ArtifactGate_RejectsInvalidCompletionEvidence(string scenario)
    {
        using var layout = new RunnerLayout();
        var outside = Path.Combine(layout.Root, "outside.bin");
        var dependencies = RunnerDependencies.Production with
        {
            TrainAsync = (_, model, _, _, _) =>
            {
                switch (scenario)
                {
                    case "empty":
                        File.WriteAllBytes(layout.FinalCheckpoint, []);
                        break;
                    case "outside":
                        model.SaveCheckpoint(outside);
                        break;
                    case "unloadable":
                        File.WriteAllText(layout.FinalCheckpoint, "not a checkpoint");
                        break;
                    case "missing":
                    case "not-completed":
                    case "wrong-step-count":
                        break;
                }

                return Task.CompletedTask;
            },
            GetRunState = _ => scenario == "not-completed"
                ? TrainerRunState.Faulted
                : TrainerRunState.Completed,
            GetCompletedSteps = _ => scenario == "wrong-step-count" ? 0 : 1,
            ResolveFinalCheckpoint = _ =>
                scenario == "outside" ? outside : layout.FinalCheckpoint,
        };

        var result = await InvokeAsync(layout, train: true, dependencies);

        Assert.Equal(3, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal(Program.ArtifactFailedToken + Environment.NewLine, result.Stderr);
        Assert.True(File.Exists(layout.ClaimPath));
        Assert.True(Directory.Exists(layout.RunDirectory));
    }

    [ReparsePointFactPR]
    public async Task ArtifactGate_RejectsReparsePointFinalCheckpoint()
    {
        using var layout = new RunnerLayout();
        var realCheckpoint = Path.Combine(layout.Root, "real-final.bin");
        var dependencies = RunnerDependencies.Production with
        {
            TrainAsync = (_, model, _, _, _) =>
            {
                model.SaveCheckpoint(realCheckpoint);
                File.CreateSymbolicLink(
                    layout.FinalCheckpoint,
                    realCheckpoint);
                return Task.CompletedTask;
            },
            GetRunState = _ => TrainerRunState.Completed,
            GetCompletedSteps = _ => 1,
        };

        var result = await InvokeAsync(layout, train: true, dependencies);

        Assert.Equal(3, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal(Program.ArtifactFailedToken + Environment.NewLine, result.Stderr);
        Assert.True(File.Exists(layout.ClaimPath));
        Assert.True(File.Exists(layout.FinalCheckpoint));
        Assert.True(
            (File.GetAttributes(layout.FinalCheckpoint)
                & FileAttributes.ReparsePoint) != 0);
    }

    [Fact]
    public async Task Process_TrainSmoke_UsesPublicProductionChainAndLoadsFinalCheckpoint()
    {
        using var layout = new RunnerLayout();

        var result = await InvokeProcessAsync(layout.Arguments(train: true));

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(Program.CompletedToken + Environment.NewLine, result.Stdout);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.True(File.Exists(layout.ClaimPath));
        Assert.True(new FileInfo(layout.FinalCheckpoint).Length > 0);
        var loaded = new LlmModel(layout.ModelConfiguration);
        loaded.LoadCheckpoint(layout.FinalCheckpoint);
    }

    [Fact]
    public async Task Process_TrainingUsesFreshLoaderAfterValidationRead()
    {
        using var layout = new RunnerLayout();
        NpyTestFile.Write(
            Path.Combine(layout.DataDirectory, "tokens.npy"),
            1, 2, 3, 99);

        var result = await InvokeProcessAsync(layout.Arguments(train: true));

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(Program.CompletedToken + Environment.NewLine, result.Stdout);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.True(new FileInfo(layout.FinalCheckpoint).Length > 0);
        var loaded = new LlmModel(layout.ModelConfiguration);
        loaded.LoadCheckpoint(layout.FinalCheckpoint);
    }

    [Fact]
    public async Task Process_InvalidArguments_Exit2WithoutCreatingRun()
    {
        using var layout = new RunnerLayout();

        var result = await InvokeProcessAsync(
            [.. layout.Arguments(train: true), "--bogus"]);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal(Program.InvalidArgumentsToken + Environment.NewLine, result.Stderr);
        Assert.False(File.Exists(layout.ClaimPath));
        Assert.False(Directory.Exists(layout.RunDirectory));
    }

    private static async Task<InvocationResult> InvokeAsync(
        RunnerLayout layout,
        bool train,
        RunnerDependencies? dependencies = null,
        CancellationToken cancellationToken = default)
        => await InvokeRawAsync(
            layout.Arguments(train),
            dependencies,
            cancellationToken);

    private static async Task<InvocationResult> InvokeRawAsync(
        string[] arguments,
        RunnerDependencies? dependencies = null,
        CancellationToken cancellationToken = default)
    {
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var exitCode = await Program.RunCoreAsync(
            arguments,
            stdout,
            stderr,
            cancellationToken,
            dependencies ?? RunnerDependencies.Production);
        return new InvocationResult(exitCode, stdout.ToString(), stderr.ToString());
    }

    private static async Task<InvocationResult> InvokeProcessAsync(
        IReadOnlyList<string> arguments)
    {
        var dotnetHost = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH")
            ?? Environment.ProcessPath
            ?? throw new InvalidOperationException("The dotnet host was not found.");
        var runnerAssembly = typeof(Program).Assembly.Location;
        var startInfo = new ProcessStartInfo(dotnetHost)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        startInfo.ArgumentList.Add(runnerAssembly);
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("The runner process did not start.");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            throw;
        }

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        return new InvocationResult(process.ExitCode, stdout, stderr);
    }

    private sealed record InvocationResult(
        int ExitCode,
        string Stdout,
        string Stderr);

    private sealed class RunnerLayout : IDisposable
    {
        internal RunnerLayout(string runId = "run-001")
        {
            Root = Path.Combine(
                Path.GetTempPath(),
                $"pretraining-runner-tests-{Guid.NewGuid():N}");
            var configDirectory = Path.Combine(Root, "config");
            DataDirectory = Path.Combine(Root, "data");
            OutputRoot = Path.Combine(Root, "output");
            Directory.CreateDirectory(configDirectory);
            Directory.CreateDirectory(DataDirectory);
            Directory.CreateDirectory(OutputRoot);
            ConfigPath = Path.Combine(configDirectory, "runner.json");
            RunId = runId;
            File.WriteAllText(ConfigPath, ValidJson);
            NpyTestFile.Write(
                Path.Combine(DataDirectory, "tokens.npy"),
                1, 2, 3, 4);
        }

        internal string Root { get; }
        internal string ConfigPath { get; }
        internal string DataDirectory { get; }
        internal string OutputRoot { get; }
        internal string RunId { get; }
        internal string RunDirectory => Path.Combine(OutputRoot, RunId);
        internal string CheckpointDirectory =>
            Path.Combine(RunDirectory, "checkpoints");
        internal string FinalCheckpoint =>
            Path.Combine(CheckpointDirectory, "model-final.bin");
        internal string ClaimPath =>
            Path.Combine(OutputRoot, $".pretraining-{RunId}.claim");

        internal ModelConfig ModelConfiguration { get; } = new()
        {
            VocabSize = 16,
            ContextLen = 4,
            HiddenDim = 4,
            NumLayers = 1,
            NumHeads = 1,
            NumKvHeads = 1,
            FfnHiddenDim = 8,
            RopeTheta = 10_000f,
            RmsNormEps = 1e-5f,
        };

        internal string ValidJson =>
            """
            {"schemaVersion":1,"model":{"VocabSize":16,"ContextLen":4,"HiddenDim":4,"NumLayers":1,"NumHeads":1,"NumKvHeads":1,"FfnHiddenDim":8,"RopeTheta":10000,"RmsNormEps":0.00001},"training":{"BatchSize":1,"SeqLen":2,"MaxLr":0.001,"MinLr":0.0001,"WarmupSteps":0,"TotalSteps":1,"GradAccumSteps":1,"GradClipNorm":1,"WeightDecay":0.125,"CheckpointEvery":1,"LogEvery":100}}
            """;

        internal string[] Arguments(bool train)
        {
            var arguments = new List<string>
            {
                "--config", ConfigPath,
                "--data-dir", DataDirectory,
                "--output-root", OutputRoot,
                "--run-id", RunId,
            };
            if (train)
                arguments.Add("--train");
            return arguments.ToArray();
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}

file sealed class ReparsePointFactPRAttribute : FactAttribute
{
    public ReparsePointFactPRAttribute()
        => Skip = ReparsePointTestSupportPR.GetCapabilitySkipReason();
}

file static class ReparsePointTestSupportPR
{
    private const int HResultPrivilegeNotHeld = unchecked((int)0x80070522);
    private const int HResultOperationNotSupported = unchecked((int)0x80070032);
    private const int HResultInvalidFunction = unchecked((int)0x80070001);

    internal static string? GetCapabilitySkipReason()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"pretraining-runner-reparse-capability-{Guid.NewGuid():N}");
        var targetDirectory = Path.Combine(root, "target-directory");
        var directoryLink = Path.Combine(root, "directory-link");
        var targetFile = Path.Combine(root, "target.bin");
        var fileLink = Path.Combine(root, "file-link.bin");
        try
        {
            Directory.CreateDirectory(targetDirectory);
            File.WriteAllBytes(targetFile, [1]);
            try
            {
                Directory.CreateSymbolicLink(directoryLink, targetDirectory);
                File.CreateSymbolicLink(fileLink, targetFile);
                return null;
            }
            catch (Exception exception)
                when (IsRecognizedCapabilityLimitation(
                    exception,
                    OperatingSystem.IsWindows()))
            {
                return "REPARSE_TEST_PLATFORM_LIMITATION: "
                    + $"{exception.GetType().FullName}, "
                    + $"HResult=0x{exception.HResult:X8}: "
                    + exception.Message;
            }
        }
        finally
        {
            if (File.Exists(fileLink))
                File.Delete(fileLink);
            if (Directory.Exists(directoryLink))
                Directory.Delete(directoryLink);
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static bool IsRecognizedCapabilityLimitation(
        Exception exception,
        bool isWindows)
    {
        if (exception is NotSupportedException)
            return true;
        if (!isWindows
            || exception is not (IOException or UnauthorizedAccessException))
        {
            return false;
        }

        return exception.HResult is
            HResultPrivilegeNotHeld
            or HResultOperationNotSupported
            or HResultInvalidFunction;
    }
}
