// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using DatasetPipeline;
using TrainingLoop;
using TrainingLoop.Optimizers;
using TransformerModel;
using TransformerModel.Config;

[assembly: InternalsVisibleTo("LlmFromScratch.Tests")]

namespace PretrainingRunner;

internal static class Program
{
    internal const string ValidatedToken = "PRETRAINING_VALIDATED_NOT_STARTED";
    internal const string CompletedToken = "PRETRAINING_COMPLETED";
    internal const string CancelledToken = "PRETRAINING_CANCELLED";
    internal const string InvalidArgumentsToken = "PRETRAINING_INVALID_ARGUMENTS";
    internal const string InvalidConfigToken = "PRETRAINING_INVALID_CONFIG";
    internal const string InvalidPathToken = "PRETRAINING_INVALID_PATH";
    internal const string InvalidDataToken = "PRETRAINING_INVALID_DATA";
    internal const string CollisionToken = "PRETRAINING_COLLISION";
    internal const string RuntimeFailedToken = "PRETRAINING_RUNTIME_FAILED";
    internal const string ArtifactFailedToken = "PRETRAINING_ARTIFACT_FAILED";

    private const int ValidationExitCode = 2;
    private const int RuntimeExitCode = 3;
    private const int CancellationExitCode = 130;

    public static async Task<int> Main(string[] args)
    {
        using var cancellation = new CancellationTokenSource();
        ConsoleCancelEventHandler handler = (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellation.Cancel();
        };

        Console.CancelKeyPress += handler;
        try
        {
            return await RunCoreAsync(
                args,
                Console.Out,
                Console.Error,
                cancellation.Token,
                RunnerDependencies.Production);
        }
        finally
        {
            Console.CancelKeyPress -= handler;
        }
    }

    internal static async Task<int> RunCoreAsync(
        string[] args,
        TextWriter stdout,
        TextWriter stderr,
        CancellationToken cancellationToken,
        RunnerDependencies dependencies)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(stdout);
        ArgumentNullException.ThrowIfNull(stderr);
        ArgumentNullException.ThrowIfNull(dependencies);

        RunnerOptions options;
        try
        {
            options = ParseArguments(args);
        }
        catch (RunnerValidationException exception)
        {
            await stderr.WriteLineAsync(exception.Token);
            return ValidationExitCode;
        }

        ValidatedInputs inputs;
        try
        {
            inputs = ValidateInputs(options, dependencies);
        }
        catch (RunnerValidationException exception)
        {
            await stderr.WriteLineAsync(exception.Token);
            return ValidationExitCode;
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            await stderr.WriteLineAsync(InvalidConfigToken);
            return ValidationExitCode;
        }

        if (!options.Train)
        {
            await stdout.WriteLineAsync(ValidatedToken);
            return 0;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            ClaimRun(inputs.Paths, dependencies);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await stderr.WriteLineAsync(CancelledToken);
            return CancellationExitCode;
        }
        catch (RunnerValidationException exception)
        {
            await stderr.WriteLineAsync(exception.Token);
            return ValidationExitCode;
        }
        catch (RunnerClaimedException)
        {
            await stderr.WriteLineAsync(RuntimeFailedToken);
            return RuntimeExitCode;
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            await stderr.WriteLineAsync(InvalidPathToken);
            return ValidationExitCode;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var loader = dependencies.CreateDataLoader(
                inputs.Paths.DataDirectory,
                inputs.Configuration.Training.BatchSize,
                inputs.Configuration.Training.SeqLen);
            var model = dependencies.CreateModel(inputs.Configuration.Model);
            var optimizer = dependencies.CreateOptimizer(
                inputs.Configuration.Training.WeightDecay);
            var trainer = dependencies.CreateTrainer(inputs.Configuration.Training);

            await dependencies.TrainAsync(
                trainer,
                model,
                optimizer,
                loader,
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            var finalCheckpoint = VerifyCompletedTraining(
                trainer,
                inputs.Configuration.Training,
                inputs.Paths,
                dependencies);

            var freshModel = dependencies.CreateModel(inputs.Configuration.Model);
            try
            {
                dependencies.LoadCheckpoint(freshModel, finalCheckpoint);
            }
            catch (Exception exception) when (!IsFatal(exception))
            {
                throw new RunnerArtifactException();
            }

            cancellationToken.ThrowIfCancellationRequested();
            await stdout.WriteLineAsync(CompletedToken);
            return 0;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await stderr.WriteLineAsync(CancelledToken);
            return CancellationExitCode;
        }
        catch (RunnerArtifactException)
        {
            await stderr.WriteLineAsync(ArtifactFailedToken);
            return RuntimeExitCode;
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            await stderr.WriteLineAsync(RuntimeFailedToken);
            return RuntimeExitCode;
        }
    }

    private static RunnerOptions ParseArguments(string[] args)
    {
        string? config = null;
        string? dataDirectory = null;
        string? outputRoot = null;
        string? runId = null;
        var train = false;

        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            switch (argument)
            {
                case "--train":
                    if (train)
                        throw new RunnerValidationException(InvalidArgumentsToken);
                    train = true;
                    break;
                case "--config":
                    config = ReadSingleValue(args, ref index, config);
                    break;
                case "--data-dir":
                    dataDirectory = ReadSingleValue(args, ref index, dataDirectory);
                    break;
                case "--output-root":
                    outputRoot = ReadSingleValue(args, ref index, outputRoot);
                    break;
                case "--run-id":
                    runId = ReadSingleValue(args, ref index, runId);
                    break;
                default:
                    throw new RunnerValidationException(InvalidArgumentsToken);
            }
        }

        if (config is null
            || dataDirectory is null
            || outputRoot is null
            || runId is null)
        {
            throw new RunnerValidationException(InvalidArgumentsToken);
        }

        return new RunnerOptions(config, dataDirectory, outputRoot, runId, train);
    }

    private static string ReadSingleValue(
        string[] args,
        ref int index,
        string? existing)
    {
        if (existing is not null
            || index + 1 >= args.Length
            || args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            throw new RunnerValidationException(InvalidArgumentsToken);
        }

        index++;
        if (string.IsNullOrWhiteSpace(args[index]))
            throw new RunnerValidationException(InvalidArgumentsToken);
        return args[index];
    }

    private static ValidatedInputs ValidateInputs(
        RunnerOptions options,
        RunnerDependencies dependencies)
    {
        var configPath = CanonicalizeLocalAbsolutePath(options.ConfigPath);
        var dataDirectory = CanonicalizeLocalAbsolutePath(options.DataDirectory);
        var outputRoot = CanonicalizeLocalAbsolutePath(options.OutputRoot);

        if (!IsSafeRunId(options.RunId))
            throw new RunnerValidationException(InvalidPathToken);
        if (!IsExistingRegularFile(configPath)
            || !Directory.Exists(dataDirectory)
            || !Directory.Exists(outputRoot)
            || IsFileSystemRoot(outputRoot))
        {
            throw new RunnerValidationException(InvalidPathToken);
        }

        EnsureNoReparsePoint(configPath);
        EnsureNoReparsePoint(dataDirectory);
        EnsureNoReparsePoint(outputRoot);
        EnsureMutuallyDisjoint(configPath, dataDirectory, outputRoot);

        var runDirectory = Path.Combine(outputRoot, options.RunId);
        var checkpointDirectory = Path.Combine(runDirectory, "checkpoints");
        var claimPath = Path.Combine(outputRoot, $".pretraining-{options.RunId}.claim");
        var paths = new ValidatedPaths(
            configPath,
            dataDirectory,
            outputRoot,
            runDirectory,
            checkpointDirectory,
            claimPath);

        ValidateProspectivePaths(paths);
        var configuration = ParseConfiguration(configPath, checkpointDirectory);
        ValidateConfiguration(configuration);
        ValidateData(dataDirectory, configuration, dependencies);
        return new ValidatedInputs(configuration, paths);
    }

    private static string CanonicalizeLocalAbsolutePath(string value)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value)
                || !Path.IsPathFullyQualified(value)
                || value.StartsWith(@"\\", StringComparison.Ordinal)
                || value.StartsWith(@"\\?\", StringComparison.Ordinal)
                || value.StartsWith(@"\\.\", StringComparison.Ordinal)
                || HasAdditionalColon(value))
            {
                throw new RunnerValidationException(InvalidPathToken);
            }

            var canonical = Path.GetFullPath(value);
            if (canonical.StartsWith(@"\\", StringComparison.Ordinal)
                || HasAdditionalColon(canonical))
            {
                throw new RunnerValidationException(InvalidPathToken);
            }

            return TrimEndingSeparators(canonical);
        }
        catch (RunnerValidationException)
        {
            throw;
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            throw new RunnerValidationException(InvalidPathToken);
        }
    }

    private static bool HasAdditionalColon(string path)
        => path.IndexOf(':', 2) >= 0;

    private static string TrimEndingSeparators(string path)
    {
        var root = Path.GetPathRoot(path);
        if (root is not null
            && string.Equals(path, root, StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        return Path.TrimEndingDirectorySeparator(path);
    }

    private static bool IsFileSystemRoot(string path)
    {
        var root = Path.GetPathRoot(path);
        return root is not null
            && string.Equals(
                TrimEndingSeparators(path),
                TrimEndingSeparators(root),
                StringComparison.OrdinalIgnoreCase);
    }

    private static void ValidateProspectivePaths(ValidatedPaths paths)
    {
        if (!IsDescendant(paths.RunDirectory, paths.OutputRoot)
            || !IsDescendant(paths.CheckpointDirectory, paths.RunDirectory)
            || Intersects(paths.RunDirectory, paths.ConfigPath)
            || Intersects(paths.RunDirectory, paths.DataDirectory)
            || Intersects(paths.CheckpointDirectory, paths.ConfigPath)
            || Intersects(paths.CheckpointDirectory, paths.DataDirectory))
        {
            throw new RunnerValidationException(InvalidPathToken);
        }

        EnsureNoReparsePoint(paths.RunDirectory);
        EnsureNoReparsePoint(paths.CheckpointDirectory);
        EnsureNoReparsePoint(paths.ClaimPath);
        if (PathEntryExists(paths.RunDirectory)
            || PathEntryExists(paths.CheckpointDirectory)
            || PathEntryExists(paths.ClaimPath))
        {
            throw new RunnerValidationException(CollisionToken);
        }
    }

    private static void EnsureMutuallyDisjoint(
        string configPath,
        string dataDirectory,
        string outputRoot)
    {
        if (Intersects(configPath, dataDirectory)
            || Intersects(configPath, outputRoot)
            || Intersects(dataDirectory, outputRoot))
        {
            throw new RunnerValidationException(InvalidPathToken);
        }
    }

    private static bool Intersects(string left, string right)
        => IsSameOrDescendant(left, right) || IsSameOrDescendant(right, left);

    private static bool IsDescendant(string path, string parent)
        => !string.Equals(path, parent, StringComparison.OrdinalIgnoreCase)
            && IsSameOrDescendant(path, parent);

    private static bool IsSameOrDescendant(string path, string parent)
    {
        var normalizedPath = TrimEndingSeparators(Path.GetFullPath(path));
        var normalizedParent = TrimEndingSeparators(Path.GetFullPath(parent));
        if (string.Equals(
                normalizedPath,
                normalizedParent,
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return normalizedPath.StartsWith(
            normalizedParent + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSafeRunId(string runId)
    {
        if (string.IsNullOrWhiteSpace(runId)
            || runId.Length > 64
            || runId is "." or ".."
            || runId.EndsWith(' ')
            || runId.EndsWith('.')
            || runId.Any(character =>
                !(character is >= 'a' and <= 'z')
                && !(character is >= 'A' and <= 'Z')
                && !(character is >= '0' and <= '9')
                && character is not '-'
                && character is not '_'
                && character is not '.'))
        {
            return false;
        }

        var deviceBase = runId.Split('.', 2)[0];
        if (deviceBase.Equals("CON", StringComparison.OrdinalIgnoreCase)
            || deviceBase.Equals("PRN", StringComparison.OrdinalIgnoreCase)
            || deviceBase.Equals("AUX", StringComparison.OrdinalIgnoreCase)
            || deviceBase.Equals("NUL", StringComparison.OrdinalIgnoreCase)
            || deviceBase.Equals("CLOCK$", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (deviceBase.Length == 4
            && (deviceBase.StartsWith("COM", StringComparison.OrdinalIgnoreCase)
                || deviceBase.StartsWith("LPT", StringComparison.OrdinalIgnoreCase))
            && deviceBase[3] is >= '1' and <= '9')
        {
            return false;
        }

        return true;
    }

    private static void EnsureNoReparsePoint(string path)
    {
        var current = path;
        while (!string.IsNullOrEmpty(current))
        {
            try
            {
                if (PathEntryExists(current))
                {
                    var attributes = File.GetAttributes(current);
                    if ((attributes & FileAttributes.ReparsePoint) != 0)
                        throw new RunnerValidationException(InvalidPathToken);
                }
            }
            catch (RunnerValidationException)
            {
                throw;
            }
            catch (Exception exception) when (!IsFatal(exception))
            {
                throw new RunnerValidationException(InvalidPathToken);
            }

            var parent = Directory.GetParent(current);
            if (parent is null)
                break;
            current = parent.FullName;
        }
    }

    private static bool PathEntryExists(string path)
    {
        try
        {
            _ = File.GetAttributes(path);
            return true;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            return false;
        }
    }

    private static bool IsExistingRegularFile(string path)
    {
        if (!PathEntryExists(path))
            return false;
        var attributes = File.GetAttributes(path);
        return (attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) == 0;
    }

    private static RunnerConfiguration ParseConfiguration(
        string configPath,
        string checkpointDirectory)
    {
        try
        {
            using var document = JsonDocument.Parse(
                File.ReadAllText(configPath),
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = 8,
                });

            var root = document.RootElement;
            EnsureExactObject(root, ["schemaVersion", "model", "training"]);
            if (ReadInt(root, "schemaVersion") != 1)
                throw new RunnerValidationException(InvalidConfigToken);

            var modelElement = root.GetProperty("model");
            EnsureExactObject(
                modelElement,
                [
                    "VocabSize",
                    "ContextLen",
                    "HiddenDim",
                    "NumLayers",
                    "NumHeads",
                    "NumKvHeads",
                    "FfnHiddenDim",
                    "RopeTheta",
                    "RmsNormEps",
                ]);
            var model = new ModelConfig
            {
                VocabSize = ReadInt(modelElement, "VocabSize"),
                ContextLen = ReadInt(modelElement, "ContextLen"),
                HiddenDim = ReadInt(modelElement, "HiddenDim"),
                NumLayers = ReadInt(modelElement, "NumLayers"),
                NumHeads = ReadInt(modelElement, "NumHeads"),
                NumKvHeads = ReadInt(modelElement, "NumKvHeads"),
                FfnHiddenDim = ReadInt(modelElement, "FfnHiddenDim"),
                RopeTheta = ReadSingle(modelElement, "RopeTheta"),
                RmsNormEps = ReadSingle(modelElement, "RmsNormEps"),
            };

            var trainingElement = root.GetProperty("training");
            EnsureExactObject(
                trainingElement,
                [
                    "BatchSize",
                    "SeqLen",
                    "MaxLr",
                    "MinLr",
                    "WarmupSteps",
                    "TotalSteps",
                    "GradAccumSteps",
                    "GradClipNorm",
                    "WeightDecay",
                    "CheckpointEvery",
                    "LogEvery",
                ]);
            var training = new TrainingConfig
            {
                BatchSize = ReadInt(trainingElement, "BatchSize"),
                SeqLen = ReadInt(trainingElement, "SeqLen"),
                MaxLr = ReadSingle(trainingElement, "MaxLr"),
                MinLr = ReadSingle(trainingElement, "MinLr"),
                WarmupSteps = ReadInt(trainingElement, "WarmupSteps"),
                TotalSteps = ReadInt(trainingElement, "TotalSteps"),
                GradAccumSteps = ReadInt(trainingElement, "GradAccumSteps"),
                GradClipNorm = ReadSingle(trainingElement, "GradClipNorm"),
                WeightDecay = ReadSingle(trainingElement, "WeightDecay"),
                CheckpointEvery = ReadInt(trainingElement, "CheckpointEvery"),
                LogEvery = ReadInt(trainingElement, "LogEvery"),
                CheckpointDir = checkpointDirectory,
            };

            return new RunnerConfiguration(model, training);
        }
        catch (RunnerValidationException)
        {
            throw;
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            throw new RunnerValidationException(InvalidConfigToken);
        }
    }

    private static void EnsureExactObject(
        JsonElement element,
        IReadOnlyCollection<string> expectedNames)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw new RunnerValidationException(InvalidConfigToken);

        var expected = new HashSet<string>(expectedNames, StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!expected.Contains(property.Name)
                || !seen.Add(property.Name)
                || property.Value.ValueKind is JsonValueKind.Null
                    or JsonValueKind.Undefined)
            {
                throw new RunnerValidationException(InvalidConfigToken);
            }
        }

        if (!expected.SetEquals(seen))
            throw new RunnerValidationException(InvalidConfigToken);
    }

    private static int ReadInt(JsonElement element, string propertyName)
    {
        var value = element.GetProperty(propertyName);
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var result))
            throw new RunnerValidationException(InvalidConfigToken);
        return result;
    }

    private static float ReadSingle(JsonElement element, string propertyName)
    {
        var value = element.GetProperty(propertyName);
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetSingle(out var result))
            throw new RunnerValidationException(InvalidConfigToken);
        return result;
    }

    private static void ValidateConfiguration(RunnerConfiguration configuration)
    {
        var model = configuration.Model;
        var training = configuration.Training;
        if (model.VocabSize <= 1
            || model.ContextLen <= 0
            || model.HiddenDim <= 0
            || model.NumLayers <= 0
            || model.NumHeads <= 0
            || model.NumKvHeads <= 0
            || model.FfnHiddenDim <= 0
            || model.HiddenDim % model.NumHeads != 0
            || model.NumHeads % model.NumKvHeads != 0
            || !float.IsFinite(model.RopeTheta)
            || model.RopeTheta <= 0f
            || !float.IsFinite(model.RmsNormEps)
            || model.RmsNormEps <= 0f
            || training.BatchSize <= 0
            || training.SeqLen <= 0
            || training.SeqLen > model.ContextLen
            || !float.IsFinite(training.MaxLr)
            || training.MaxLr <= 0f
            || !float.IsFinite(training.MinLr)
            || training.MinLr < 0f
            || training.MinLr > training.MaxLr
            || training.WarmupSteps < 0
            || training.TotalSteps <= 0
            || training.WarmupSteps > training.TotalSteps
            || training.GradAccumSteps <= 0
            || !float.IsFinite(training.GradClipNorm)
            || training.GradClipNorm <= 0f
            || !float.IsFinite(training.WeightDecay)
            || training.WeightDecay < 0f
            || training.CheckpointEvery <= 0
            || training.LogEvery <= 0)
        {
            throw new RunnerValidationException(InvalidConfigToken);
        }
    }

    private static void ValidateData(
        string dataDirectory,
        RunnerConfiguration configuration,
        RunnerDependencies dependencies)
    {
        try
        {
            var shards = Directory
                .EnumerateFiles(dataDirectory, "*", SearchOption.TopDirectoryOnly)
                .Where(path => string.Equals(
                    Path.GetExtension(path),
                    ".npy",
                    StringComparison.Ordinal))
                .ToArray();
            if (shards.Length == 0 || shards.Any(path => !IsExistingRegularFile(path)))
                throw new RunnerValidationException(InvalidDataToken);

            var loader = dependencies.CreateDataLoader(
                dataDirectory,
                configuration.Training.BatchSize,
                configuration.Training.SeqLen);
            var (inputs, targets) = loader.NextBatch();
            ValidateBatchTokens(inputs, configuration.Model.VocabSize);
            ValidateBatchTokens(targets, configuration.Model.VocabSize);
        }
        catch (RunnerValidationException)
        {
            throw;
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            throw new RunnerValidationException(InvalidDataToken);
        }
    }

    private static void ValidateBatchTokens(long[,] batch, int vocabularySize)
    {
        foreach (var token in batch)
        {
            if (token < 0 || token >= vocabularySize)
                throw new RunnerValidationException(InvalidDataToken);
        }
    }

    private static void ClaimRun(
        ValidatedPaths paths,
        RunnerDependencies dependencies)
    {
        // Ordinary local Windows paths are supported. Hostile same-user
        // reparse/rename TOCTOU attacks are outside this runner's guarantee.
        ValidateProspectivePaths(paths);
        FileStream claim;
        try
        {
            claim = new FileStream(
                paths.ClaimPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);
        }
        catch (IOException)
        {
            throw new RunnerValidationException(CollisionToken);
        }
        catch (UnauthorizedAccessException)
        {
            throw new RunnerValidationException(CollisionToken);
        }

        PersistClaimMarker(claim, dependencies);

        try
        {
            dependencies.AfterClaimCreated(paths);
            Directory.CreateDirectory(paths.RunDirectory);
            Directory.CreateDirectory(paths.CheckpointDirectory);
            EnsureNoReparsePoint(paths.RunDirectory);
            EnsureNoReparsePoint(paths.CheckpointDirectory);

            if (!IsDescendant(paths.RunDirectory, paths.OutputRoot)
                || !IsDescendant(paths.CheckpointDirectory, paths.RunDirectory))
            {
                throw new IOException("Claimed paths changed containment.");
            }
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            throw new RunnerClaimedException();
        }
    }

    private static void PersistClaimMarker(
        FileStream claim,
        RunnerDependencies dependencies)
    {
        Exception? failure = null;
        try
        {
            dependencies.PersistClaimMarker(claim);
        }
        catch (Exception exception)
        {
            failure = exception;
        }

        try
        {
            claim.Dispose();
        }
        catch (Exception exception)
        {
            failure = failure is null
                ? exception
                : new AggregateException(failure, exception);
        }

        if (failure is null)
            return;
        if (IsFatal(failure))
            System.Runtime.ExceptionServices.ExceptionDispatchInfo
                .Capture(failure)
                .Throw();

        throw new RunnerClaimedException();
    }

    private static string VerifyCompletedTraining(
        Trainer trainer,
        TrainingConfig configuration,
        ValidatedPaths paths,
        RunnerDependencies dependencies)
    {
        try
        {
            if (dependencies.GetRunState(trainer) != TrainerRunState.Completed
                || dependencies.GetCompletedSteps(trainer) != configuration.TotalSteps)
            {
                throw new RunnerArtifactException();
            }

            var finalCheckpoint = dependencies.ResolveFinalCheckpoint(
                paths.CheckpointDirectory);
            var canonicalFinal = TrimEndingSeparators(Path.GetFullPath(finalCheckpoint));

            if (!IsDescendant(canonicalFinal, paths.CheckpointDirectory)
                || !IsExistingRegularFile(canonicalFinal)
                || new FileInfo(canonicalFinal).Length <= 0)
            {
                throw new RunnerArtifactException();
            }

            EnsureNoReparsePoint(canonicalFinal);
            return canonicalFinal;
        }
        catch (RunnerArtifactException)
        {
            throw;
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            throw new RunnerArtifactException();
        }
    }

    private static bool IsFatal(Exception exception)
    {
        var pending = new Stack<Exception>();
        var seen = new HashSet<Exception>(ReferenceEqualityComparer.Instance);
        pending.Push(exception);
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!seen.Add(current))
                continue;

            if (current is OutOfMemoryException
                or StackOverflowException
                or AccessViolationException
                or AppDomainUnloadedException
                or BadImageFormatException
                || current.GetType().FullName == "System.Threading.ThreadAbortException")
            {
                return true;
            }

            if (current is AggregateException aggregate)
            {
                foreach (var inner in aggregate.InnerExceptions)
                    pending.Push(inner);
            }
            else if (current.InnerException is not null)
            {
                pending.Push(current.InnerException);
            }
        }

        return false;
    }

    private sealed record RunnerOptions(
        string ConfigPath,
        string DataDirectory,
        string OutputRoot,
        string RunId,
        bool Train);

    internal sealed record RunnerConfiguration(
        ModelConfig Model,
        TrainingConfig Training);

    internal sealed record ValidatedPaths(
        string ConfigPath,
        string DataDirectory,
        string OutputRoot,
        string RunDirectory,
        string CheckpointDirectory,
        string ClaimPath);

    private sealed record ValidatedInputs(
        RunnerConfiguration Configuration,
        ValidatedPaths Paths);

    private sealed class RunnerValidationException(string token) : Exception
    {
        internal string Token { get; } = token;
    }

    private sealed class RunnerArtifactException : Exception;

    private sealed class RunnerClaimedException : Exception;
}

internal sealed record RunnerDependencies(
    Func<string, int, int, DataLoader> CreateDataLoader,
    Func<ModelConfig, LlmModel> CreateModel,
    Func<float, AdamW> CreateOptimizer,
    Func<TrainingConfig, Trainer> CreateTrainer,
    Func<Trainer, LlmModel, AdamW, DataLoader, CancellationToken, Task> TrainAsync,
    Func<Trainer, TrainerRunState> GetRunState,
    Func<Trainer, int> GetCompletedSteps,
    Func<string, string> ResolveFinalCheckpoint,
    Action<LlmModel, string> LoadCheckpoint,
    Action<FileStream> PersistClaimMarker,
    Action<Program.ValidatedPaths> AfterClaimCreated)
{
    internal static RunnerDependencies Production { get; } = new(
        (dataDirectory, batchSize, sequenceLength) =>
            new DataLoader(dataDirectory, batchSize, sequenceLength),
        configuration => new LlmModel(configuration),
        weightDecay => new AdamW(weightDecay: weightDecay),
        configuration => new Trainer(configuration),
        RunTrainingWithoutConsoleNoiseAsync,
        trainer => trainer.RunState,
        trainer => trainer.CompletedSteps,
        checkpointDirectory => Path.Combine(
            checkpointDirectory,
            "model-final.bin"),
        (model, path) => model.LoadCheckpoint(path),
        claim =>
        {
            var marker = Encoding.ASCII.GetBytes("PRETRAINING_CLAIM_V1\n");
            claim.Write(marker);
            claim.Flush(flushToDisk: true);
        },
        _ => { });

    private static async Task RunTrainingWithoutConsoleNoiseAsync(
        Trainer trainer,
        LlmModel model,
        AdamW optimizer,
        DataLoader loader,
        CancellationToken cancellationToken)
    {
        var original = Console.Out;
        try
        {
            Console.SetOut(TextWriter.Null);
            await trainer.TrainAsync(
                model,
                optimizer,
                loader,
                cancellationToken);
        }
        finally
        {
            Console.SetOut(original);
        }
    }
}
