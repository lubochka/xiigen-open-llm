// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
using System.Diagnostics;
using System.Runtime.InteropServices; // Читаем фактические float bytes параметров без изменения значений.
using System.Security.Cryptography; // Identity контроля выводится из реального полного состояния.
using System.Text.Json; // Канонический локальный паспорт групп сохраняет owner keys.
using AgentInfrastructure; // Новая offline boundary возвращает существующий OperationResult.
using AgentInfrastructure.Logical; // Некритические исключения не превращаются в успешную генерацию.
using ContinuousLearning;
using LoRA;
using Tokenizer;
using TransformerModel; // Только owner-created модель остаётся внутри generation service.

namespace TrainableRouter.Training;

// Explicit trusted host configuration, never an HTTP request or assessment corpus profile.
public sealed record CodeDocumentationGenerationOptions(
    CodeDocumentationRepositoryOwnedModelCheckpointStartRequest StartRequest,
    BpeTokenizer Tokenizer, CodeDocumentationStudentGenerationSettings Settings);

public sealed record CodeDocumentationControlParameterIdentity(string Key, int[] Shape, int ValueCount, string Sha256); // Проверяем каждую owner-keyed группу на точных bytes.
public sealed record CodeDocumentationControlStateIdentity(string FullCheckpointSha256, long ByteLength, string BaseSha256, string LoraASha256, string LoraBSha256, string RandomStateSha256, IReadOnlyList<CodeDocumentationControlParameterIdentity> Parameters); // Отделяем веса базы, обе LoRA стороны и фактические RNG cursors.
public sealed record CodeDocumentationInitialControlReceipt(CodeDocumentationGenerationResult Generation, CodeDocumentationControlStateIdentity? Before, CodeDocumentationControlStateIdentity? After, string PrivateSnapshotRelativePath); // Receipt не возвращает модель или trusted handle caller.

public sealed class CodeDocumentationGenerationService
{
    private readonly CodeDocumentationRepositoryOwnedModelCheckpointStartRequest _start;
    private readonly CodeDocumentationStudentGenerationSettings _settings;
    private readonly byte[] _tokenizerBytes;
    public string RepositoryRoot => _start.RepositoryRoot;
    public string DomainSlug => _start.DomainSlug;
    public CodeDocumentationGenerationService(CodeDocumentationGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _start = options.StartRequest with
        {
            ModelConfig = options.StartRequest.ModelConfig with { },
            LoraConfig = options.StartRequest.LoraConfig is null ? null : LoraApplicator.ValidateAndCanonicalize(options.StartRequest.LoraConfig)
        };
        _settings = options.Settings with { };
        using var stream = new MemoryStream(); options.Tokenizer.Save(stream);
        _tokenizerBytes = stream.ToArray();
    }

    public CodeDocumentationGenerationResult Generate(CodeDocumentationGenerationInput input,
        CancellationToken cancellationToken = default)
        => GenerateCore(input, cancellationToken, null, null); // Обычный consumer сохраняет прежний START и serving-only путь.

    public OperationResult<CodeDocumentationInitialControlReceipt> GenerateInitialControl(CodeDocumentationGenerationInput input, CancellationToken cancellationToken = default) // Отдельный offline контроль не публикует accepted state и не обучает.
        => GenerateControlCore(input, cancellationToken, retained: false); // Первое initialization сохраняет прежний new-only контракт.

    public OperationResult<CodeDocumentationInitialControlReceipt> GenerateRetainedInitialControl(CodeDocumentationGenerationInput input, CancellationToken cancellationToken = default) // Отдельно разрешённый diagnostic использует прежний immutable snapshot без перезаписи.
        => GenerateControlCore(input, cancellationToken, retained: true); // Модель остаётся owner-created; caller не передаёт weights либо handle.

    private OperationResult<CodeDocumentationInitialControlReceipt> GenerateControlCore(CodeDocumentationGenerationInput input, CancellationToken cancellationToken, bool retained) // Обе control стадии делят прежнюю START→shared Generate цепочку.
    { // Эффект ограничен единственным новым частным snapshot текущего RP01 контроля.
        const string relative = ".kiro/xiigen-slices/work/results/RP01.03A-initial-control-binding-20261014/private/initial.weights.bin"; // Нельзя перенаправить export в exam либо accepted tree.
        CodeDocumentationControlStateIdentity? before = null, after = null; // До фактического owner START состояния не считаются наблюдёнными.
        var snapshotMismatch = false; // Несовпадение recreated state со snapshot не допускает prediction.
        string? snapshotError = null; // Проверки destination/length сохраняют прежние typed refusal codes после deferred open.
        try // Сохраняем typed refusal при custody либо model ошибке.
        { // Явный seed и cold authorization обязательны только для этого offline control.
            if (!_start.ColdStartPolicy.IsAuthorized || _start.BaseInitializationSeed is null || _start.LoraConfig is null || _start.AdapterSeedAuthority is null) // Не открываем новый путь скрытого cold start.
                return OperationResult<CodeDocumentationInitialControlReceipt>.Fail("Initial control requires explicit clean-start and independent base/adapter seed authority.", "INITIAL_CONTROL_AUTHORITY_INVALID"); // Нет подмены human reason либо caller Model.
            var path = Path.GetFullPath(Path.Combine(RepositoryRoot, relative)); // Адрес вычисляет owner из уже проверенного canonical root.
            var directory = Path.GetDirectoryName(path)!; // Новый snapshot остаётся в фиксированном private каталоге.
            if (!CodeDocumentationRepositoryOwnedModelCheckpointStartService.ValidateRequest(_start).IsSuccess || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path) || !SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(directory) || Directory.Exists(path) || (retained ? !File.Exists(path) || !SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path) : File.Exists(path))) // Reuse допускает только прежний protected файл; новый режим не перезаписывает existing state.
                return OperationResult<CodeDocumentationInitialControlReceipt>.Fail("Initial control custody or new-only destination is invalid.", "INITIAL_CONTROL_DESTINATION_INVALID"); // Отказ предшествует owner START и созданию файла.
            FileStream? snapshot = null; // Не открываем и не создаём snapshot до initial-only допуска настоящего START.
            try // Lifetime handle ограничен единственным shared control вызовом.
            { // Присутствие accepted candidate должно отказать до любого snapshot эффекта.
                var result = GenerateCore(input, cancellationToken, model => // Публичный caller не получает этот закрытый observer либо модель.
                { // Freeze actual state до первого prediction, используя существующий serializer.
                    snapshot = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, retained ? FileMode.Open : FileMode.CreateNew, retained ? FileAccess.Read : FileAccess.ReadWrite, FileShare.None, FileOptions.SequentialScan, 81920); // Snapshot открывается только после discovery-only START admission.
                    if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(snapshot.SafeFileHandle, path)) throw new IOException(snapshotError = "INITIAL_CONTROL_DESTINATION_INVALID"); // Не сериализуем в неизвестную поверхность.
                    if (retained && (snapshot.Length <= 0 || snapshot.Length > _start.MaximumCheckpointBytes)) throw new IOException(snapshotError = "INITIAL_CONTROL_RETAINED_SNAPSHOT_INVALID"); // Empty/corrupt boundary не заменяется fresh state.
                    var retainedHash = retained ? TrainingSourceProvenance.Sha256(snapshot) : null; // Actual protected snapshot identity фиксируется до prediction.
                    if (retained) // Воссозданная owner модель обязана совпасть с прежними полными bytes.
                    { // Не загружаем caller weights и не переписываем immutable snapshot.
                        using var actual = new MemoryStream(); model.SaveCheckpointToStream(actual); // Existing serializer читает recreated production state.
                        var actualHash = TrainingSourceProvenance.Sha256(actual); // Проверяем все веса, включая actual project LoRA.
                        snapshotMismatch = actualHash != retainedHash || actual.Length != snapshot.Length; // Любое изменение init/config/source выявляется до prediction.
                        if (snapshotMismatch) throw new IOException("INITIAL_CONTROL_RETAINED_SNAPSHOT_MISMATCH"); // Закрытый observer прерывает общий Generate до model inference.
                        before = CaptureControlState(model, actualHash, actual.Length); // Только byte-identical state допускается как retained no-update control.
                    } // Завершаем retained hash guard без write effects.
                    else // Initial режим сохраняет полный новый snapshot прежним serializer.
                    { // New-only файл открыт под проверенной custody после clean initial admission.
                        model.SaveCheckpointToStream(snapshot); // Записываем полный owner-created LlmModel, включая реальную project LoRA.
                        snapshot.Flush(flushToDisk: true); // Snapshot должен быть durable до генерации.
                        before = CaptureControlState(model, TrainingSourceProvenance.Sha256(snapshot), snapshot.Length); // Хешируем те же открытые полные checkpoint bytes.
                    } // Не смешиваем new snapshot с retained runtime receipt.
                }, model => // Readback исполняется также после cooperative cancellation генерации.
                { // Не превращаем timeout в потерю before/after доказательства.
                    using var state = new MemoryStream(); // Это сравнение сериализации, не новый trainer/accepted artifact.
                    model.SaveCheckpointToStream(state); // Прежний serializer читает реальное после-инференсное состояние.
                    after = CaptureControlState(model, TrainingSourceProvenance.Sha256(state), state.Length); // Сохраняем фактическую state identity без повторного prediction.
                }, initialOnly: true); // BOTH initial controls use the same owner refusal before accepted payload reads/import.
                if (snapshotError is not null) return OperationResult<CodeDocumentationInitialControlReceipt>.Fail("Initial control snapshot validation failed before inference.", snapshotError); // Deferred open не превращает прежние typed guards в generic generation error.
                if (snapshotMismatch) return OperationResult<CodeDocumentationInitialControlReceipt>.Fail("Recreated model differs from the retained snapshot; inference was not started.", "INITIAL_CONTROL_RETAINED_SNAPSHOT_MISMATCH"); // Не превращаем несовпадение в valid control либо fallback.
                if (before is not null && after is not null && (before.FullCheckpointSha256 != after.FullCheckpointSha256 || before.RandomStateSha256 != after.RandomStateSha256)) // Инференс не вправе менять weights или adapter RNG.
                    return OperationResult<CodeDocumentationInitialControlReceipt>.Fail("Read-only inference changed model state.", "INITIAL_CONTROL_STATE_CHANGED"); // Изменённое состояние никогда не становится valid control.
                return OperationResult<CodeDocumentationInitialControlReceipt>.Ok(new(result, before, after, relative)); // Generation error сохраняется внутри настоящего receipt; quality не присваивается.
            } // Все result branches оставляют disposal владельцу handle.
            finally { snapshot?.Dispose(); } // Отказ START не создаёт handle либо файл; остальные branches закрывают snapshot.
        } // Завершаем ограниченный offline эффект.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Error остаётся ошибкой, snapshot/history сохраняются.
        { // Не запускаем повторный START, fallback либо optimizer.
            return OperationResult<CodeDocumentationInitialControlReceipt>.Fail("Initial control failed; inspect the retained diagnostic state before any retry.", "INITIAL_CONTROL_FAILED"); // Отказ не выдаёт fake pass либо accepted state.
        } // Закрываем nonfatal boundary.
    } // Закрываем реальный частный no-update control.

    private CodeDocumentationControlStateIdentity CaptureControlState(LlmModel model, string fullSha256, long byteLength) // Identity всегда выводится из actual owner model.
    { // Единственный checkpoint key catalogue задаётся LlmModel, а не новым parser.
        var parameters = model.GetCheckpointParameterEntries().OrderBy(entry => entry.Key, StringComparer.Ordinal).Select(entry => new CodeDocumentationControlParameterIdentity(entry.Key, entry.Parameter.Value.Shape.Dimensions.ToArray(), entry.Parameter.Value.Data.Length, Convert.ToHexString(SHA256.HashData(MemoryMarshal.AsBytes(entry.Parameter.Value.Data.AsSpan()))).ToLowerInvariant())).ToArray(); // Все float bytes и формы читаются без записи.
        string Group(string prefix, string? suffix = null) => Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(parameters.Where(entry => entry.Key.StartsWith(prefix, StringComparison.Ordinal) && (suffix is null || entry.Key.EndsWith(suffix, StringComparison.Ordinal))).ToArray()))).ToLowerInvariant(); // Diagnostic digest содержит exact keys, shapes, lengths и raw-value hashes.
        var random = model.CaptureRandomState(_start.RuntimeCompatibility.TopologySha256, LoraRandomStateIdentity.ComputeLoraConfigurationSha256(_start.LoraConfig!, _start.AdapterSeedAuthority!), 0); // Настоящий owner фиксирует каждый initialization/dropout cursor.
        return new(fullSha256, byteLength, Group("base."), Group("lora.", ".lora_a"), Group("lora.", ".lora_b"), Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(random))).ToLowerInvariant(), parameters); // Фактические suffix задаёт LoraLinear, не новые placeholder labels.
    } // Закрываем observation-only fingerprint.

    private CodeDocumentationGenerationResult GenerateCore(CodeDocumentationGenerationInput input, CancellationToken cancellationToken, Action<LlmModel>? beforeGeneration, Action<LlmModel>? afterGeneration, bool initialOnly = false) // Offline control и обычный consumer делят ровно одну цепочку START→Generate; default оставляет обычную загрузку.
    {
        var elapsed = Stopwatch.StartNew(); double loadMilliseconds = 0;
        Stopwatch? generationElapsed = null;
        CodeDocumentationStudentSourceIdentity? identity = null;
        string? tokenizerHash = null;
        string digest = string.Empty;
        try
        {
            CodeDocumentationStudentGeneration.ValidateInput(input, DomainSlug);
            digest = CodeDocumentationStudentGeneration.InputSha256(input.OwnerInput);
            cancellationToken.ThrowIfCancellationRequested();
            using var tokenizerStream = new MemoryStream(_tokenizerBytes, writable: false);
            var tokenizer = BpeTokenizer.Load(tokenizerStream);
            tokenizerHash = tokenizer.ComputeStateSha256();
            if (tokenizerHash != _start.RuntimeCompatibility.TokenizerSha256)
                return Failure("CODE_DOCUMENTATION_GENERATION_TOKENIZER_MISMATCH", "ERROR");
            var loaded = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(_start, cancellationToken, initialOnly); // Initial-only отказ происходит внутри discovery owner до accepted payload, не поздним manifest check.
            loadMilliseconds = elapsed.Elapsed.TotalMilliseconds;
            if (!loaded.IsSuccess || loaded.Value is null)
                return Failure(loaded.ErrorCode ?? "CODE_DOCUMENTATION_GENERATION_SOURCE_UNAVAILABLE",
                    loaded.ErrorCode == CodeDocumentationRepositoryOwnedModelCheckpointStartService.CancelledErrorCode
                        ? "CANCELLED" : "ERROR");
            var manifest = loaded.Value.ParentManifest;
            identity = manifest is null
                ? new("explicitly_authorized_clean_initial_state", DomainSlug, _start.ModelIdSlug, _start.ModelVersion, null, null, null)
                : new("repository_published_checkpoint", manifest.Domain, manifest.ModelId, manifest.Version,
                    manifest.Step, manifest.ManifestSha256, manifest.WeightsSha256);
            // This isolated loaded instance is serving-only; do not mutate flags, weights or RNG.
            var handle = CodeDocumentationStudentHandle.BindVerified(loaded.Value.Model, tokenizer, identity);
            beforeGeneration?.Invoke(loaded.Value.Model); // Закрытый control observer фиксирует ровно serving instance до prediction.
            generationElapsed = Stopwatch.StartNew();
            Inference.TextGenerationReceipt receipt; // Сохраняем тот же реальный receipt генерации.
            try { receipt = CodeDocumentationStudentGeneration.Generate(handle, input, _settings, cancellationToken); } // Единственный prediction использует прежний framing и decoder.
            finally { afterGeneration?.Invoke(loaded.Value.Model); } // Даже timeout сохраняет фактический после-инференсный state hash.
            if (receipt.Text.Length > _settings.MaxAnswerChars)
                return Failure("CODE_DOCUMENTATION_GENERATION_OUTPUT_BOUND_EXCEEDED", "ERROR");
            return new(input.RequestId, input.DomainSlug, input.ContractVersion,
                CodeDocumentationStudentGeneration.FramingVersion, digest, receipt.Text, identity, tokenizerHash,
                CodeDocumentationStudentGeneration.SettingsSha256(_settings), receipt.PromptTokenCount,
                receipt.GeneratedTokenCount, receipt.StopReason, loadMilliseconds, receipt.ElapsedMilliseconds,
                elapsed.Elapsed.TotalMilliseconds, null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        { return Failure("CODE_DOCUMENTATION_GENERATION_CANCELLED", "CANCELLED"); }
        catch (ArgumentException exception)
        { return Failure(exception.Message.StartsWith("TEXT_GENERATOR_", StringComparison.Ordinal) ||
            exception.Message.StartsWith("CODE_DOCUMENTATION_GENERATION_", StringComparison.Ordinal)
                ? exception.Message : CodeDocumentationStudentGeneration.InvalidErrorCode, "ERROR"); }
        catch (Exception exception) when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        { return Failure("CODE_DOCUMENTATION_GENERATION_FAILED", "ERROR"); }

        CodeDocumentationGenerationResult Failure(string code, string stop)
            => new(input?.RequestId ?? "", input?.DomainSlug ?? "", CodeDocumentationStudentGeneration.ContractVersion,
                CodeDocumentationStudentGeneration.FramingVersion, digest, null, identity, tokenizerHash,
                CodeDocumentationStudentGeneration.SettingsSha256(_settings), null, null, stop,
                loadMilliseconds, generationElapsed?.Elapsed.TotalMilliseconds ?? 0, elapsed.Elapsed.TotalMilliseconds, code);
    }
}
