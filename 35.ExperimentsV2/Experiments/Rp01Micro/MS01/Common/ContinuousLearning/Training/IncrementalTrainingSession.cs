// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using LoRA;
using Microsoft.Win32.SafeHandles;
using Tokenizer;
using TrainingLoop;
using TrainingLoop.Optimizers;
using TransformerModel;
using TransformerModel.Config;

/// <summary>
/// A single-use training graph. Every factory call returns a fresh model,
/// tokenizer, trainer, and optimizer so optimizer moments and gradients cannot
/// leak between domains or training runs.
/// </summary>
public sealed record IncrementalTrainingSession(
    LlmModel Model,
    BpeTokenizer Tokenizer,
    Trainer Trainer,
    AdamW Optimizer,
    string BaseCheckpointSha256,
    string TokenizerSha256,
    ModelConfigurationIdentity ModelIdentity)
{
    internal IncrementalTrainingParentAdapterCheckpoint? ParentAdapterCheckpoint
    {
        get;
    }

    public IncrementalTrainingCheckpointStartMetadata? CheckpointStart
    {
        get;
    }

    public RepositoryOwnedModelCheckpointManifest? ResumeParentCheckpoint
        => CheckpointStart?.ResumeParentCheckpoint;

    public IncrementalTrainingColdStartMetadata? ColdStartCheckpoint
        => CheckpointStart?.ColdStart;

    internal IncrementalTrainingSession(
        LlmModel model,
        BpeTokenizer tokenizer,
        Trainer trainer,
        AdamW optimizer,
        IncrementalTrainingParentAdapterCheckpoint? parentAdapterCheckpoint,
        string baseCheckpointSha256,
        string tokenizerSha256,
        ModelConfigurationIdentity modelIdentity,
        IncrementalTrainingCheckpointStartMetadata? checkpointStart)
        : this(
            model,
            tokenizer,
            trainer,
            optimizer,
            baseCheckpointSha256,
            tokenizerSha256,
            modelIdentity)
    {
        ParentAdapterCheckpoint = parentAdapterCheckpoint;
        CheckpointStart = checkpointStart;
    }
}

public enum IncrementalTrainingCheckpointStartSource
{
    RepositoryOwnedModelCheckpoint = 1,
    RetainedBaseCheckpointColdStart = 2,
}

public sealed record IncrementalTrainingColdStartMetadata(
    string HumanReason,
    RepositoryOwnedIncrementalTrainingSessionStartRequest?
        RepositoryStartRequest = null);

public sealed class IncrementalTrainingCheckpointStartMetadata
{
    private IncrementalTrainingCheckpointStartMetadata(
        IncrementalTrainingCheckpointStartSource source,
        RepositoryOwnedModelCheckpointManifest? resumeParentCheckpoint,
        RepositoryOwnedIncrementalTrainingSessionStartRequest?
            repositoryStartRequest,
        IncrementalTrainingColdStartMetadata? coldStart)
    {
        Source = source;
        ResumeParentCheckpoint = resumeParentCheckpoint;
        RepositoryStartRequest = repositoryStartRequest;
        ColdStart = coldStart;
    }

    public IncrementalTrainingCheckpointStartSource Source { get; }
    public RepositoryOwnedModelCheckpointManifest? ResumeParentCheckpoint { get; }
    public RepositoryOwnedIncrementalTrainingSessionStartRequest?
        RepositoryStartRequest { get; }
    public IncrementalTrainingColdStartMetadata? ColdStart { get; }

    internal static IncrementalTrainingCheckpointStartMetadata FromRepository(
        RepositoryOwnedModelCheckpointManifest resumeParentCheckpoint,
        RepositoryOwnedIncrementalTrainingSessionStartRequest startRequest)
    {
        ArgumentNullException.ThrowIfNull(resumeParentCheckpoint);
        ArgumentNullException.ThrowIfNull(startRequest);
        return new IncrementalTrainingCheckpointStartMetadata(
            IncrementalTrainingCheckpointStartSource.RepositoryOwnedModelCheckpoint,
            resumeParentCheckpoint,
            startRequest,
            coldStart: null);
    }

    internal static IncrementalTrainingCheckpointStartMetadata FromColdStart(
        string humanReason,
        RepositoryOwnedIncrementalTrainingSessionStartRequest startRequest)
    {
        ArgumentNullException.ThrowIfNull(startRequest);
        return new(
            IncrementalTrainingCheckpointStartSource
                .RetainedBaseCheckpointColdStart,
            resumeParentCheckpoint: null,
            startRequest,
            new IncrementalTrainingColdStartMetadata(
                humanReason,
                startRequest));
    }

    internal static IncrementalTrainingCheckpointStartMetadata FromColdStart(
        string humanReason)
        => new(
            IncrementalTrainingCheckpointStartSource
                .RetainedBaseCheckpointColdStart,
            resumeParentCheckpoint: null,
            repositoryStartRequest: null,
            new IncrementalTrainingColdStartMetadata(humanReason));
}

public sealed record RepositoryOwnedIncrementalTrainingColdStartPolicy(
    bool IsAuthorized,
    string? HumanReason)
{
    public static RepositoryOwnedIncrementalTrainingColdStartPolicy Denied { get; }
        = new(IsAuthorized: false, HumanReason: null);

    public static RepositoryOwnedIncrementalTrainingColdStartPolicy Authorize(
        string humanReason)
        => new(IsAuthorized: true, HumanReason: humanReason);
}

public sealed record RepositoryOwnedIncrementalTrainingSessionStartRequest(
    string RepositoryRoot,
    string DomainSlug,
    string DesignPatternSlug,
    string ModelIdSlug,
    string ModelVersion,
    RepositoryOwnedModelRuntimeCompatibility RuntimeCompatibility,
    RepositoryOwnedIncrementalTrainingColdStartPolicy ColdStartPolicy,
    RepositoryOwnedDpoDatasetManifestIdentity? CurrentDpoDatasetManifest = null)
{
    // WHY: CurrentDpoDatasetManifest was added as an optional tail so source
    // callers kept compiling, but older binaries bind the exact seven-argument
    // constructor and seven-value deconstruction shape. Keep those shapes
    // explicit while the new manifest identity remains available to new code.
    public RepositoryOwnedIncrementalTrainingSessionStartRequest(
        string repositoryRoot,
        string domainSlug,
        string designPatternSlug,
        string modelIdSlug,
        string modelVersion,
        RepositoryOwnedModelRuntimeCompatibility runtimeCompatibility,
        RepositoryOwnedIncrementalTrainingColdStartPolicy coldStartPolicy)
        : this(
            repositoryRoot,
            domainSlug,
            designPatternSlug,
            modelIdSlug,
            modelVersion,
            runtimeCompatibility,
            coldStartPolicy,
            CurrentDpoDatasetManifest: null)
    {
    }

    public void Deconstruct(
        out string repositoryRoot,
        out string domainSlug,
        out string designPatternSlug,
        out string modelIdSlug,
        out string modelVersion,
        out RepositoryOwnedModelRuntimeCompatibility runtimeCompatibility,
        out RepositoryOwnedIncrementalTrainingColdStartPolicy coldStartPolicy)
    {
        repositoryRoot = RepositoryRoot;
        domainSlug = DomainSlug;
        designPatternSlug = DesignPatternSlug;
        modelIdSlug = ModelIdSlug;
        modelVersion = ModelVersion;
        runtimeCompatibility = RuntimeCompatibility;
        coldStartPolicy = ColdStartPolicy;
    }
}

/// <summary>
/// An immutable, path-free snapshot of a parent adapter checkpoint captured at
/// session creation. Every reader receives an independent non-writable stream.
/// </summary>
internal sealed class IncrementalTrainingParentAdapterCheckpoint
{
    internal const long MaximumArtifactBytes =
        ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes;

    private readonly byte[] _artifactBytes;

    internal ModelAdapterProvenance Provenance { get; }

    internal IncrementalTrainingParentAdapterCheckpoint(
        Stream callerOwnedStream,
        ModelAdapterProvenance provenance)
    {
        ArgumentNullException.ThrowIfNull(callerOwnedStream);
        ArgumentNullException.ThrowIfNull(provenance);
        if (!callerOwnedStream.CanRead)
        {
            throw new ArgumentException(
                "Adapter checkpoint stream must be readable.",
                nameof(callerOwnedStream));
        }

        Provenance = SnapshotProvenance(provenance);
        _artifactBytes = CaptureBounded(callerOwnedStream);

        using (var validationStream = CreateReadStream(_artifactBytes))
            LlmModel.ValidateAdapterCheckpointArtifact(validationStream);

        using var hashStream = CreateReadStream(_artifactBytes);
        var capturedSha256 = TrainingSourceProvenance.Sha256(hashStream);
        if (!StringComparer.Ordinal.Equals(
                capturedSha256,
                Provenance.AdapterSha256))
        {
            throw new IncrementalTrainingCheckpointHashMismatchException();
        }
    }

    internal Stream OpenRead()
        => CreateReadStream(_artifactBytes);

    private static byte[] CaptureBounded(Stream callerOwnedStream)
    {
        if (callerOwnedStream.CanSeek)
            callerOwnedStream.Position = 0;

        using var captured = new MemoryStream();
        var buffer = new byte[64 * 1024];
        long total = 0;
        while (true)
        {
            var remaining = MaximumArtifactBytes - total;
            var maximumRead = (int)Math.Min(buffer.Length, remaining + 1);
            var read = callerOwnedStream.Read(buffer, 0, maximumRead);
            if (read == 0)
                break;
            if (read > remaining)
                throw new InvalidDataException("ADAPTER_CHECKPOINT_SIZE_INVALID");

            captured.Write(buffer, 0, read);
            total += read;
        }

        if (total == 0)
            throw new InvalidDataException("ADAPTER_CHECKPOINT_SIZE_INVALID");
        return captured.ToArray();
    }

    private static MemoryStream CreateReadStream(byte[] artifactBytes)
        => new(
            artifactBytes,
            index: 0,
            count: artifactBytes.Length,
            writable: false,
            publiclyVisible: false);

    private static ModelAdapterProvenance SnapshotProvenance(
        ModelAdapterProvenance provenance)
    {
        if (provenance.TrainingSources is null
            || provenance.TrainingSources.SourceIds is null
            || provenance.Model is null
            || provenance.Lora is null
            || provenance.Lora.OrderedTargetModules is null)
        {
            throw new InvalidDataException("ADAPTER_PROVENANCE_INVALID");
        }

        return provenance with
        {
            TrainingSources = provenance.TrainingSources with
            {
                SourceIds = Array.AsReadOnly(
                    provenance.TrainingSources.SourceIds.ToArray()),
            },
            Model = provenance.Model with { },
            Lora = provenance.Lora with
            {
                OrderedTargetModules = Array.AsReadOnly(
                    provenance.Lora.OrderedTargetModules.ToArray()),
            },
        };
    }
}

internal sealed class IncrementalTrainingCheckpointHashMismatchException
    : Exception
{
}

/// <summary>
/// Builds an isolated incremental-training graph from freshly opened streams.
/// Each stream factory is invoked only when its artifact is needed, and every
/// returned stream is disposed before this method returns.
/// </summary>
internal static class IncrementalTrainingSessionBuilder
{
    internal const long MaximumBaseCheckpointBytes =
        16L * 1024 * 1024 * 1024;
    internal const long MaximumTokenizerBytes = 512L * 1024 * 1024;

    internal static OperationResult<IncrementalTrainingSession> Build(
        ModelConfig modelConfig,
        TrainingConfig trainingConfig,
        Func<Stream> openBaseCheckpoint,
        Func<Stream> openTokenizer,
        IncrementalTrainingParentAdapterCheckpoint? parentAdapterCheckpoint,
        CancellationToken cancellationToken = default,
        IncrementalTrainingCheckpointStartMetadata? checkpointStart = null,
        LoraConfig? checkpointAdapterConfig = null)
    {
        ArgumentNullException.ThrowIfNull(modelConfig);
        ArgumentNullException.ThrowIfNull(trainingConfig);
        ArgumentNullException.ThrowIfNull(openBaseCheckpoint);
        ArgumentNullException.ThrowIfNull(openTokenizer);

        cancellationToken.ThrowIfCancellationRequested();
        using var baseCheckpointStream = openBaseCheckpoint();
        RequireReadableSeekableBounded(
            baseCheckpointStream,
            MaximumBaseCheckpointBytes,
            "BASE_CHECKPOINT_INVALID");

        cancellationToken.ThrowIfCancellationRequested();
        baseCheckpointStream.Position = 0;
        var baseCheckpointSha256 =
            TrainingSourceProvenance.Sha256(baseCheckpointStream);
        cancellationToken.ThrowIfCancellationRequested();
        baseCheckpointStream.Position = 0;
        var model = new LlmModel(modelConfig);
        try
        {
            model.LoadCheckpoint(baseCheckpointStream);
        }
        catch (Exception exception)
            when (CanRetryRepositoryCheckpointWithAdapterTopology(
                exception,
                checkpointStart,
                checkpointAdapterConfig))
        {
            baseCheckpointStream.Position = 0;
            var adapterAwareModel = new LlmModel(modelConfig);
            _ = LoraApplicator.Apply(adapterAwareModel, checkpointAdapterConfig!);
            adapterAwareModel.LoadCheckpoint(baseCheckpointStream);
            model = adapterAwareModel;
        }

        cancellationToken.ThrowIfCancellationRequested();
        using var tokenizerStream = openTokenizer();
        RequireReadableSeekableBounded(
            tokenizerStream,
            MaximumTokenizerBytes,
            "TOKENIZER_CHECKPOINT_INVALID");

        cancellationToken.ThrowIfCancellationRequested();
        tokenizerStream.Position = 0;
        var tokenizerSha256 =
            TrainingSourceProvenance.Sha256(tokenizerStream);
        cancellationToken.ThrowIfCancellationRequested();
        tokenizerStream.Position = 0;
        var tokenizer = BpeTokenizer.Load(tokenizerStream);
        if (tokenizer.VocabSize < 256
            || tokenizer.VocabSize > modelConfig.VocabSize)
        {
            return OperationResult<IncrementalTrainingSession>.Fail(
                "The tokenizer vocabulary exceeds the model vocabulary.",
                "TRAINING_SESSION_CREATION_FAILED");
        }

        cancellationToken.ThrowIfCancellationRequested();
        // A one-example incremental call must have a non-zero first step.
        var perCallConfig = trainingConfig with { WarmupSteps = 0 };
        var trainer = new Trainer(perCallConfig);
        var optimizer = new AdamW(weightDecay: perCallConfig.WeightDecay);

        return OperationResult<IncrementalTrainingSession>.Ok(
            new IncrementalTrainingSession(
                model,
                tokenizer,
                trainer,
                optimizer,
                parentAdapterCheckpoint,
                baseCheckpointSha256,
                tokenizerSha256,
                TrainingSourceProvenance.Identify(modelConfig),
                checkpointStart));
    }

    private static void RequireReadableSeekableBounded(
        Stream stream,
        long maximumBytes,
        string errorCode)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead
            || !stream.CanSeek
            || stream.Length <= 0
            || stream.Length > maximumBytes)
        {
            throw new InvalidDataException(errorCode);
        }
    }

    private static bool CanRetryRepositoryCheckpointWithAdapterTopology(
        Exception exception,
        IncrementalTrainingCheckpointStartMetadata? checkpointStart,
        LoraConfig? checkpointAdapterConfig)
        // WHY: repository-owned child checkpoints are full model artifacts.
        // After an incremental LoRA update, those bytes legitimately contain
        // adapter records; a fresh model must attach the same topology before
        // parsing them. Cold starts and legacy base checkpoints stay on the
        // plain base-model load path.
        => checkpointStart?.Source
            == IncrementalTrainingCheckpointStartSource
                .RepositoryOwnedModelCheckpoint
           && checkpointAdapterConfig is not null
           && LlmModel.IsCheckpointParameterCountMismatch(exception);
}

/// <summary>
/// Creates isolated sessions from immutable on-disk base artifacts and the
/// registry's optional current adapter for the requested domain.
/// </summary>
public sealed class CheckpointBackedIncrementalTrainingSessionFactory
    : IIncrementalTrainingSessionFactory
{
    private readonly ModelConfig _modelConfig;
    private readonly string _baseCheckpointPath;
    private readonly string _tokenizerPath;
    private readonly IDomainModelRegistry _registry;
    private readonly TrainingConfig _trainingConfig;
    private readonly Action<FileStream, string>? _afterParentAdapterOpenForTesting;
    private readonly Action<FileStream, string>? _afterParentAdapterCaptureForTesting;

    public CheckpointBackedIncrementalTrainingSessionFactory(
        ModelConfig modelConfig,
        string baseCheckpointPath,
        string tokenizerPath,
        IDomainModelRegistry registry,
        TrainingConfig trainingConfig)
        : this(
            modelConfig,
            baseCheckpointPath,
            tokenizerPath,
            registry,
            trainingConfig,
            afterParentAdapterOpenForTesting: null,
            afterParentAdapterCaptureForTesting: null)
    {
    }

    internal CheckpointBackedIncrementalTrainingSessionFactory(
        ModelConfig modelConfig,
        string baseCheckpointPath,
        string tokenizerPath,
        IDomainModelRegistry registry,
        TrainingConfig trainingConfig,
        Action<FileStream, string>? afterParentAdapterOpenForTesting,
        Action<FileStream, string>? afterParentAdapterCaptureForTesting = null)
    {
        _modelConfig = modelConfig
            ?? throw new ArgumentNullException(nameof(modelConfig));
        _baseCheckpointPath = baseCheckpointPath
            ?? throw new ArgumentNullException(nameof(baseCheckpointPath));
        _tokenizerPath = tokenizerPath
            ?? throw new ArgumentNullException(nameof(tokenizerPath));
        _registry = registry
            ?? throw new ArgumentNullException(nameof(registry));
        _trainingConfig = trainingConfig
            ?? throw new ArgumentNullException(nameof(trainingConfig));
        _afterParentAdapterOpenForTesting = afterParentAdapterOpenForTesting;
        _afterParentAdapterCaptureForTesting =
            afterParentAdapterCaptureForTesting;
    }

    public OperationResult<IncrementalTrainingSession> Create(string domain)
    {
        if (!File.Exists(_baseCheckpointPath))
        {
            return OperationResult<IncrementalTrainingSession>.Fail(
                "The configured base checkpoint was not found.",
                "BASE_CHECKPOINT_NOT_FOUND");
        }

        if (!File.Exists(_tokenizerPath))
        {
            return OperationResult<IncrementalTrainingSession>.Fail(
                "The configured tokenizer checkpoint was not found.",
                "TOKENIZER_CHECKPOINT_NOT_FOUND");
        }

        try
        {
            var adapterResult = _registry.GetAdapter(domain);
            if (!adapterResult.IsSuccess)
            {
                return OperationResult<IncrementalTrainingSession>.Fail(
                    adapterResult.ErrorMessage,
                    adapterResult.ErrorCode);
            }

            IncrementalTrainingParentAdapterCheckpoint? parentAdapterCheckpoint = null;
            if (adapterResult.Value is { } parentAdapter)
            {
                if (!StringComparer.Ordinal.Equals(parentAdapter.Domain, domain))
                {
                    return OperationResult<IncrementalTrainingSession>.Fail(
                        "Registered adapter domain does not match the requested domain.",
                        "ADAPTER_DOMAIN_MISMATCH");
                }

                if (string.IsNullOrWhiteSpace(parentAdapter.AdapterPath)
                    || !Path.IsPathFullyQualified(parentAdapter.AdapterPath))
                {
                    return OperationResult<IncrementalTrainingSession>.Fail(
                        "Adapter path must be fully qualified.",
                        "ADAPTER_PATH_NOT_ABSOLUTE");
                }

                string parentAdapterPath;
                try
                {
                    parentAdapterPath = RequireCanonicalPathSpelling(
                        parentAdapter.AdapterPath);
                }
                catch (Exception exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                    return OperationResult<IncrementalTrainingSession>.Fail(
                        "Adapter path must be a valid fully-qualified path.",
                        "ADAPTER_PATH_NOT_ABSOLUTE");
                }

                if (parentAdapter.Provenance is null)
                {
                    return OperationResult<IncrementalTrainingSession>.Fail(
                        "The parent adapter has no durable training provenance.",
                        "ADAPTER_PROVENANCE_REQUIRED");
                }

                FileStream parentStream;
                try
                {
                    ValidateNonReparseParentAncestry(parentAdapterPath);
                    parentStream = OpenParentAdapterRead(parentAdapterPath);
                }
                catch (Exception exception)
                    when ((exception is FileNotFoundException or DirectoryNotFoundException)
                          && SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                    return OperationResult<IncrementalTrainingSession>.Fail(
                        "Adapter checkpoint was not found.",
                        "ADAPTER_CHECKPOINT_NOT_FOUND");
                }
                catch (Exception exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                    return OperationResult<IncrementalTrainingSession>.Fail(
                        "Adapter checkpoint is invalid.",
                        "ADAPTER_CHECKPOINT_INVALID");
                }

                try
                {
                    using (parentStream)
                    {
                        _afterParentAdapterOpenForTesting?.Invoke(
                            parentStream,
                            parentAdapterPath);
                        var openedIdentity = ValidateOpenedParentAdapter(
                            parentStream,
                            parentAdapterPath);
                        IncrementalTrainingParentAdapterCheckpoint capturedCheckpoint;
                        try
                        {
                            capturedCheckpoint =
                                new IncrementalTrainingParentAdapterCheckpoint(
                                    parentStream,
                                    parentAdapter.Provenance);
                        }
                        catch (Exception captureException)
                        {
                            if (!SnapshotExceptionGraphClassifier.IsNonFatal(
                                    captureException))
                            {
                                throw;
                            }

                            _ = ValidateOpenedParentAdapter(
                                parentStream,
                                parentAdapterPath,
                                openedIdentity);
                            throw;
                        }

                        _afterParentAdapterCaptureForTesting?.Invoke(
                            parentStream,
                            parentAdapterPath);
                        _ = ValidateOpenedParentAdapter(
                            parentStream,
                            parentAdapterPath,
                            openedIdentity);
                        parentAdapterCheckpoint = capturedCheckpoint;
                    }
                }
                catch (IncrementalTrainingCheckpointHashMismatchException)
                {
                    return OperationResult<IncrementalTrainingSession>.Fail(
                        "The parent adapter bytes do not match registered provenance.",
                        "ADAPTER_PROVENANCE_MISMATCH");
                }
                catch (Exception exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                    return OperationResult<IncrementalTrainingSession>.Fail(
                        "Adapter checkpoint is invalid.",
                        "ADAPTER_CHECKPOINT_INVALID");
                }
            }

            return IncrementalTrainingSessionBuilder.Build(
                _modelConfig,
                _trainingConfig,
                () => OpenRead(_baseCheckpointPath),
                () => OpenRead(_tokenizerPath),
                parentAdapterCheckpoint);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<IncrementalTrainingSession>.Fail(
                "The incremental training session could not be created.",
                "TRAINING_SESSION_CREATION_FAILED");
        }
    }

    private static FileStream OpenRead(string path)
        => new(
            TrainingSourceProvenance.RequireCanonicalRegularFilePath(path),
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 64 * 1024,
            FileOptions.SequentialScan);

    private static string RequireCanonicalPathSpelling(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (!string.Equals(
                path,
                fullPath,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException("Artifact path must be canonical.");
        }

        return fullPath;
    }

    private static FileStream OpenParentAdapterRead(string path)
    {
        SafeFileHandle ownedHandle;
        if (OperatingSystem.IsWindows())
        {
            ownedHandle = OpenParentAdapterWindows(path);
        }
        else if (OperatingSystem.IsLinux())
        {
            ownedHandle = OpenParentAdapterLinux(path);
        }
        else
        {
            throw new PlatformNotSupportedException(
                "Adapter checkpoint native open is unsupported.");
        }

        try
        {
            return new FileStream(
                ownedHandle,
                FileAccess.Read,
                bufferSize: 64 * 1024,
                isAsync: false);
        }
        catch
        {
            ownedHandle.Dispose();
            throw;
        }
    }

    private static SafeFileHandle OpenParentAdapterWindows(string path)
    {
        const uint genericRead = 0x8000_0000;
        const uint shareRead = 0x0000_0001;
        const uint shareDelete = 0x0000_0004;
        const uint openExisting = 3;
        const uint fileFlagOpenReparsePoint = 0x0020_0000;
        _ = NormalizeWindowsIdentityPath(path);
        var handle = CreateFileWindows(
            path,
            genericRead,
            shareRead | shareDelete,
            IntPtr.Zero,
            openExisting,
            fileFlagOpenReparsePoint,
            IntPtr.Zero);
        if (!handle.IsInvalid)
            return handle;

        var error = Marshal.GetLastPInvokeError();
        handle.Dispose();
        if (error is 2 or 3)
        {
            throw new FileNotFoundException(
                "The parent adapter checkpoint was not found.",
                path);
        }

        throw new InvalidDataException(
            $"The parent adapter native open failed with Win32 error {error}.");
    }

    private static SafeFileHandle OpenParentAdapterLinux(string path)
    {
        const int openReadOnly = 0;
        const int openNonBlocking = 0x0000_0800;
        const int openNoFollow = 0x0002_0000;
        const int openCloseOnExec = 0x0008_0000;
        var descriptor = OpenLinux(
            path,
            openReadOnly | openNonBlocking | openNoFollow | openCloseOnExec);
        if (descriptor >= 0)
        {
            return new SafeFileHandle(
                new IntPtr(descriptor),
                ownsHandle: true);
        }

        var error = Marshal.GetLastPInvokeError();
        if (error == 2)
        {
            throw new FileNotFoundException(
                "The parent adapter checkpoint was not found.",
                path);
        }

        throw new InvalidDataException(
            $"The parent adapter native open failed with errno {error}.");
    }

    private static OpenedParentAdapterIdentity ValidateOpenedParentAdapter(
        FileStream stream,
        string expectedPath,
        OpenedParentAdapterIdentity? expectedIdentity = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (stream.SafeFileHandle.IsInvalid || stream.SafeFileHandle.IsClosed)
            throw new InvalidDataException("Adapter checkpoint handle is invalid.");

        ValidateNonReparseParentAncestry(expectedPath);
        var identity = OperatingSystem.IsWindows()
            ? ValidateOpenedParentAdapterWindows(stream.SafeFileHandle, expectedPath)
            : OperatingSystem.IsLinux()
                ? ValidateOpenedParentAdapterLinux(stream.SafeFileHandle, expectedPath)
                : throw new PlatformNotSupportedException(
                    "Adapter checkpoint identity validation is unsupported.");
        if (expectedIdentity is { } requiredIdentity
            && identity != requiredIdentity)
        {
            throw new InvalidDataException(
                "Adapter checkpoint identity changed during capture.");
        }

        return identity;
    }

    private static OpenedParentAdapterIdentity ValidateOpenedParentAdapterWindows(
        SafeFileHandle handle,
        string expectedPath)
    {
        const uint fileTypeDisk = 0x0001;
        const uint fileAttributeDirectory = 0x0010;
        const uint fileAttributeReparsePoint = 0x0400;
        if (GetFileTypeWindows(handle) != fileTypeDisk
            || !GetFileInformationByHandleExWindows(
                handle,
                FileInfoByHandleClass.FileAttributeTagInfo,
                out FileAttributeTagInformation attributes,
                (uint)Marshal.SizeOf<FileAttributeTagInformation>())
            || (attributes.FileAttributes
                & (fileAttributeDirectory | fileAttributeReparsePoint)) != 0
            || !GetFileInformationByHandleExWindows(
                handle,
                FileInfoByHandleClass.FileStandardInfo,
                out FileStandardInformation standard,
                (uint)Marshal.SizeOf<FileStandardInformation>())
            || standard.EndOfFile <= 0
            || standard.EndOfFile
                > IncrementalTrainingParentAdapterCheckpoint.MaximumArtifactBytes
            || standard.DeletePending != 0
            || standard.Directory != 0
            || standard.NumberOfLinks != 1
            || !GetFileInformationByHandleExWindows(
                handle,
                FileInfoByHandleClass.FileIdInfo,
                out FileIdInformation identity,
                (uint)Marshal.SizeOf<FileIdInformation>())
            || identity.VolumeSerialNumber == 0
            || identity.FileId.Low == 0 && identity.FileId.High == 0)
        {
            throw new InvalidDataException(
                "Adapter checkpoint is not a stable regular file.");
        }

        var openedPath = ResolveOpenedAdapterArtifactPathWindows(
            handle,
            expectedPath);
        if (!PathsEqual(openedPath, expectedPath))
        {
            throw new InvalidDataException(
                "Adapter checkpoint name changed during capture.");
        }

        return new OpenedParentAdapterIdentity(
            identity.VolumeSerialNumber,
            identity.FileId.Low,
            identity.FileId.High,
            standard.NumberOfLinks,
            checked((ulong)standard.EndOfFile));
    }

    private static OpenedParentAdapterIdentity ValidateOpenedParentAdapterLinux(
        SafeFileHandle handle,
        string expectedPath)
    {
        const int atFileDescriptorWorkingDirectory = -100;
        const int atSymlinkNoFollow = 0x100;
        const int atEmptyPath = 0x1000;
        const uint statxBasicStats = 0x07ff;
        const uint requiredMask = 0x0307;
        const ushort fileTypeMask = 0xf000;
        const ushort regularFile = 0x8000;
        ValidateLinuxStatxAbi();
        var descriptorValue = handle.DangerousGetHandle().ToInt64();
        if (descriptorValue < 0 || descriptorValue > int.MaxValue
            || StatxLinux(
                checked((int)descriptorValue),
                string.Empty,
                atEmptyPath,
                statxBasicStats,
                out var opened) != 0
            || StatxLinux(
                atFileDescriptorWorkingDirectory,
                expectedPath,
                atSymlinkNoFollow,
                statxBasicStats,
                out var current) != 0
            || (opened.Mask & requiredMask) != requiredMask
            || (current.Mask & requiredMask) != requiredMask
            || (opened.Mode & fileTypeMask) != regularFile
            || (current.Mode & fileTypeMask) != regularFile
            || opened.LinkCount != 1
            || current.LinkCount != 1
            || opened.Size == 0
            || opened.Size
                > (ulong)IncrementalTrainingParentAdapterCheckpoint.MaximumArtifactBytes
            || current.Size != opened.Size
            || opened.DeviceMajor != current.DeviceMajor
            || opened.DeviceMinor != current.DeviceMinor
            || opened.Inode != current.Inode)
        {
            throw new InvalidDataException(
                "Adapter checkpoint is not the stable named regular file.");
        }

        var openedPath = ResolveOpenedAdapterArtifactPathLinux(
            checked((int)descriptorValue));
        if (!PathsEqual(openedPath, expectedPath))
        {
            throw new InvalidDataException(
                "Adapter checkpoint name changed during capture.");
        }

        return new OpenedParentAdapterIdentity(
            ((ulong)opened.DeviceMajor << 32) | opened.DeviceMinor,
            opened.Inode,
            0,
            opened.LinkCount,
            opened.Size);
    }

    private static string ResolveOpenedAdapterArtifactPathWindows(
        SafeFileHandle handle,
        string expectedPath)
    {
        const int maximumPathCharacters = 32 * 1024;
        const uint volumeNameDos = 0;
        const uint volumeNameGuid = 1;
        var flags = IsExtendedVolumeGuidPath(expectedPath)
            ? volumeNameGuid
            : volumeNameDos;
        var capacity = 512;
        while (capacity <= maximumPathCharacters)
        {
            var buffer = new StringBuilder(capacity);
            var length = GetFinalPathNameByHandleWindows(
                handle,
                buffer,
                (uint)buffer.Capacity,
                flags);
            if (length == 0)
                break;
            if (length < buffer.Capacity)
                return buffer.ToString();

            if (length >= maximumPathCharacters)
                break;
            capacity = checked((int)length + 1);
        }

        throw new InvalidDataException(
            "Adapter checkpoint opened path is unavailable.");
    }

    private static string ResolveOpenedAdapterArtifactPathLinux(int descriptor)
    {
        const int maximumPathBytes = 64 * 1024;
        var descriptorPath = "/proc/self/fd/"
            + descriptor.ToString(CultureInfo.InvariantCulture);
        var capacity = 256;
        while (capacity <= maximumPathBytes)
        {
            var buffer = new byte[capacity];
            var read = ReadLinkLinux(
                descriptorPath,
                buffer,
                (nuint)buffer.Length);
            if (read < 0)
                break;
            if (read < buffer.Length)
            {
                var path = Encoding.UTF8.GetString(buffer, 0, checked((int)read));
                if (!Path.IsPathFullyQualified(path)
                    || path.EndsWith(" (deleted)", StringComparison.Ordinal))
                {
                    break;
                }

                return Path.GetFullPath(path);
            }

            capacity = checked(capacity * 2);
        }

        throw new InvalidDataException(
            "Adapter checkpoint opened path is unavailable.");
    }

    private static void ValidateNonReparseParentAncestry(string path)
    {
        for (var directory = new FileInfo(path).Directory;
             directory is not null;
             directory = directory.Parent)
        {
            var attributes = File.GetAttributes(directory.FullName);
            if ((attributes & FileAttributes.Directory) == 0
                || (attributes & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidDataException(
                    "Adapter checkpoint ancestry is not stable.");
            }
        }
    }

    private static bool PathsEqual(string first, string second)
        => OperatingSystem.IsWindows()
            ? string.Equals(
                NormalizeWindowsIdentityPath(first),
                NormalizeWindowsIdentityPath(second),
                StringComparison.Ordinal)
            : string.Equals(
                Path.TrimEndingDirectorySeparator(Path.GetFullPath(first)),
                Path.TrimEndingDirectorySeparator(Path.GetFullPath(second)),
                StringComparison.Ordinal);

    // The comparison boundary intentionally preserves component casing. Windows
    // namespace aliases are canonicalized, but a case-only name exchange fails
    // closed under the resulting Ordinal comparison.
    private static string NormalizeWindowsIdentityPath(string path)
    {
        const string extendedPrefix = @"\\?\";
        const string extendedUncPrefix = @"\\?\UNC\";
        const string devicePrefix = @"\\.\";
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string normalized;
        if (path.StartsWith(extendedUncPrefix, StringComparison.OrdinalIgnoreCase))
        {
            normalized = @"\\" + path[extendedUncPrefix.Length..];
        }
        else if (IsExtendedVolumeGuidPath(path))
        {
            var closeBrace = path.IndexOf('}', extendedPrefix.Length);
            var guidText = path.Substring(
                extendedPrefix.Length + "Volume{".Length,
                closeBrace - extendedPrefix.Length - "Volume{".Length);
            if (!Guid.TryParse(guidText, out var volumeGuid))
                throw new InvalidDataException("Windows volume GUID path is invalid.");
            normalized = string.Concat(
                @"\\?\Volume{",
                volumeGuid.ToString("D").ToUpperInvariant(),
                "}",
                path[(closeBrace + 1)..]);
        }
        else if (path.StartsWith(extendedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var withoutPrefix = path[extendedPrefix.Length..];
            if (!IsWindowsDriveAbsolutePath(withoutPrefix))
            {
                throw new InvalidDataException(
                    "Windows extended path namespace is unsupported.");
            }

            normalized = withoutPrefix;
        }
        else
        {
            if (path.StartsWith(devicePrefix, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Windows device paths are unsupported.");
            normalized = path;
        }

        var fullPath = Path.GetFullPath(normalized);
        if (!IsExtendedVolumeGuidPath(fullPath)
            && !IsWindowsDriveAbsolutePath(fullPath)
            && !IsWindowsUncAbsolutePath(fullPath))
        {
            throw new InvalidDataException("Windows artifact path namespace is invalid.");
        }

        if (IsWindowsDriveAbsolutePath(fullPath))
        {
            fullPath = char.ToUpperInvariant(fullPath[0]) + fullPath[1..];
        }

        var root = Path.GetPathRoot(fullPath);
        return string.Equals(fullPath, root, StringComparison.Ordinal)
            ? fullPath
            : Path.TrimEndingDirectorySeparator(fullPath);
    }

    private static bool IsExtendedVolumeGuidPath(string path)
    {
        const string prefix = @"\\?\Volume{";
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;
        var closeBrace = path.IndexOf('}', prefix.Length);
        return closeBrace > prefix.Length
               && closeBrace + 1 < path.Length
               && path[closeBrace + 1] == '\\'
               && Guid.TryParse(
                   path.Substring(prefix.Length, closeBrace - prefix.Length),
                   out _);
    }

    private static bool IsWindowsDriveAbsolutePath(string path)
        => path.Length >= 3
           && char.IsAsciiLetter(path[0])
           && path[1] == ':'
           && path[2] == '\\';

    private static bool IsWindowsUncAbsolutePath(string path)
    {
        if (!path.StartsWith(@"\\", StringComparison.Ordinal)
            || path.StartsWith(@"\\?\", StringComparison.Ordinal)
            || path.StartsWith(@"\\.\", StringComparison.Ordinal))
        {
            return false;
        }

        var serverEnd = path.IndexOf('\\', 2);
        return serverEnd > 2
               && serverEnd + 1 < path.Length
               && path.IndexOf('\\', serverEnd + 1) > serverEnd + 1;
    }

    private static void ValidateLinuxStatxAbi()
    {
        if (Marshal.SizeOf<LinuxStatx>() != 256
            || Marshal.OffsetOf<LinuxStatx>(nameof(LinuxStatx.Mask)).ToInt32() != 0
            || Marshal.OffsetOf<LinuxStatx>(nameof(LinuxStatx.LinkCount)).ToInt32() != 16
            || Marshal.OffsetOf<LinuxStatx>(nameof(LinuxStatx.Mode)).ToInt32() != 28
            || Marshal.OffsetOf<LinuxStatx>(nameof(LinuxStatx.Inode)).ToInt32() != 32
            || Marshal.OffsetOf<LinuxStatx>(nameof(LinuxStatx.Size)).ToInt32() != 40
            || Marshal.OffsetOf<LinuxStatx>(nameof(LinuxStatx.DeviceMajor)).ToInt32() != 136
            || Marshal.OffsetOf<LinuxStatx>(nameof(LinuxStatx.DeviceMinor)).ToInt32() != 140)
        {
            throw new InvalidDataException("Linux statx ABI layout is unavailable.");
        }
    }

    private readonly record struct OpenedParentAdapterIdentity(
        ulong DeviceOrVolume,
        ulong FileIdLow,
        ulong FileIdHigh,
        uint LinkCount,
        ulong Length);

    private enum FileInfoByHandleClass
    {
        FileStandardInfo = 1,
        FileAttributeTagInfo = 9,
        FileIdInfo = 18,
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileAttributeTagInformation
    {
        internal uint FileAttributes;
        internal uint ReparseTag;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileStandardInformation
    {
        internal long AllocationSize;
        internal long EndOfFile;
        internal uint NumberOfLinks;
        internal byte DeletePending;
        internal byte Directory;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileId128
    {
        internal ulong Low;
        internal ulong High;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileIdInformation
    {
        internal ulong VolumeSerialNumber;
        internal FileId128 FileId;
    }

    [StructLayout(LayoutKind.Explicit, Size = 256)]
    private struct LinuxStatx
    {
        [FieldOffset(0)] internal uint Mask;
        [FieldOffset(16)] internal uint LinkCount;
        [FieldOffset(28)] internal ushort Mode;
        [FieldOffset(32)] internal ulong Inode;
        [FieldOffset(40)] internal ulong Size;
        [FieldOffset(136)] internal uint DeviceMajor;
        [FieldOffset(140)] internal uint DeviceMinor;
    }

    [DllImport(
        "kernel32.dll",
        EntryPoint = "CreateFileW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern SafeFileHandle CreateFileWindows(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", EntryPoint = "GetFileType", SetLastError = true)]
    private static extern uint GetFileTypeWindows(SafeFileHandle file);

    [DllImport("libc", EntryPoint = "open", SetLastError = true)]
    private static extern int OpenLinux(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        int flags);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetFileInformationByHandleEx",
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleExWindows(
        SafeFileHandle file,
        FileInfoByHandleClass fileInformationClass,
        out FileAttributeTagInformation fileInformation,
        uint bufferSize);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetFileInformationByHandleEx",
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleExWindows(
        SafeFileHandle file,
        FileInfoByHandleClass fileInformationClass,
        out FileStandardInformation fileInformation,
        uint bufferSize);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetFileInformationByHandleEx",
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleExWindows(
        SafeFileHandle file,
        FileInfoByHandleClass fileInformationClass,
        out FileIdInformation fileInformation,
        uint bufferSize);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetFinalPathNameByHandleW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern uint GetFinalPathNameByHandleWindows(
        SafeFileHandle file,
        [Out] StringBuilder path,
        uint pathCapacity,
        uint flags);

    [DllImport("libc", EntryPoint = "statx", SetLastError = true)]
    private static extern int StatxLinux(
        int directoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        int flags,
        uint mask,
        out LinuxStatx buffer);

    [DllImport("libc", EntryPoint = "readlink", SetLastError = true)]
    private static extern nint ReadLinkLinux(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        byte[] buffer,
        nuint bufferSize);
}
