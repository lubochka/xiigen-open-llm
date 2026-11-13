// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
using System.Diagnostics; // Записываем фактические затраты текущего control process.
using System.Runtime.InteropServices; // Windows host memory observation не меняет настройки системы.
using System.Security.AccessControl; // Новые owned artifacts получают тот же custody ACL профиль.
using System.Security.Cryptography; // Pins всегда вычисляются из actual bytes.
using System.Security.Principal; // ACL выдаётся только текущему owner, SYSTEM и Administrators.
using System.Text; // Exact UTF-8 owner input не исправляется и не обрезается.
using System.Text.Json; // Конфигурация и receipts сохраняются структурно, без секретного environment dump.
using ContinuousLearning; // Переиспользуем actual provenance и runtime contracts.
using LoRA; // Реальная project LoRA topology остаётся owner-derived.
using ModelServer; // Запускаем настоящий production configuration reader.
using Tokenizer; // Используем canonical production BPE, обученный только на разрешённых TRAIN sources.
using TrainableRouter.Training; // Owner START и существующая генерация исполняются напрямую.
using TransformerModel.Config; // Student использует поддержанный production Small profile.

internal static class Program // Одноразовый локальный initialization/control executor, без Trainer либо Teacher.
{ // Tool никогда не принимает caller weights либо произвольный exam path.
    private const string Root = "C:/Projects/llm_mvp_core"; // Текущий canonical root закреплён рабочим заданием.
    private const string ExamRelative = ".kiro/xiigen-slices/work/results/RP01.02A-independent-csharp-exam-20261014"; // Закрепляем существующий public eligibility owner.
    private const string EvidenceRelative = ".kiro/xiigen-slices/work/results/RP01.03A-initial-control-binding-20261014"; // Запись ограничена новым owned evidence surface.
    private const string PanelHash = "1ea18aab6750bbafe30dda43fe13d6028a2beedffea6e8abc00c3c43e6b2ca06"; // Это actual prespecified public commitment, не fabricated runtime identity.
    private const string ProtocolHash = "2c44a6aa5f39d92e738d2809d671d5d29c5a49ce11a0af4056d006b23f300a21"; // Actual user-amended protocol frozen before Teacher; старый hash хранится в историческом receipt.
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true }; // Читаемый receipt имеет stable declared property policy.
    private static readonly UTF8Encoding Strict = new(false, true); // Некорректный input не заменяется replacement characters.
    private static readonly string[] CaseIds = ["case-241908b0d8333ad8e821", "case-b07da119d4827825c877", "case-7b5f00b28b8b569d6c23", "case-0fb028e3ec00718c449b"]; // Только четыре заранее допущенные TRAIN семьи.

    private static int Main(string[] args) // CLI process exit отражает реальный typed result, не тестовую метку.
    { // Любой отказ сохраняет историю; повторный control не разрешается автоматически.
        try // Native runtime/custody failure остаётся actual error.
        { // Стадия требует конкретной переданной причины, производной от действующего разрешения.
            if (args.Length == 3 && args[0] == "--verify-configuration" && args[1] == "--configuration-sha256") return VerifyConfiguration(args[2]); // Read-only проверка reader/Denied START никогда не вызывает Generate либо snapshot write.
            if (args.Length != 3 || args[0] is not ("--initialize-control" or "--repeat-retained-control") || args[1] != "--cold-start-reason" || string.IsNullOrWhiteSpace(args[2])) // Другие пути/профили и неопределённая причина не поддержаны.
                return Refuse("CONTROL_ARGUMENTS_INVALID"); // Не создаём конфигурацию при неопределённой authority.
            if (args[2].Length > 512 || args[2] != args[2].Trim() || args[2].Any(char.IsControl)) return Refuse("CONTROL_REASON_INVALID"); // Owner reason bound проверяется до source/tokenizer/ACL/file effects.
            if (args[0] == "--repeat-retained-control") return RepeatRetainedControl(args[2]); // Explicit user override разрешил ровно один retained diagnostic, без новой initialization/config записи.
            return Run(args[2]); // Root work order разрешает эту конкретную clean initialization, не fabricated signature.
        } // Завершаем единственный CLI вызов.
        catch (Exception exception) // Не выдаём successful receipt при actual host/file/model exception.
        { // Показываем только type и bounded error, не environment/provider secrets.
            Console.Error.WriteLine($"CONTROL_FAILED:{exception.GetType().Name}:{exception.Message}:{exception.StackTrace}"); // Реальная bounded local stack trace сохраняет место отказа без environment/secret dump.
            return 1; // History и новые files сохраняются, implicit retry отсутствует.
        } // Закрываем процесс после конечного результата.
    } // Никакой HTTP host либо paid provider не запускается.

    private static int VerifyConfiguration(string expectedHash) // Проверяем final reader/ordinary denial без повторного inference и без записи snapshot/config.
    { // Existing configuration не означает accepted model либо разрешение нового cold start.
        using var cancellation = new ManualStop(); // Пользовательская отмена действует и на read-only проверку; нового wall timeout нет.
        var configured = CodeDocumentationGenerationConfiguration.ReadVerified(name => name switch { "XIIGEN_CODE_DOCUMENTATION_GENERATION_ENABLED" => "true", "XIIGEN_CODE_DOCUMENTATION_GENERATION_CONFIG_SHA256" => expectedHash, _ => null }, Root, cancellation.Token); // Actual public wrapper вызывает unchanged production Resolve.
        if (!configured.IsSuccess || configured.Value is null) return Refuse(configured.ErrorCode); // Неверный pinned hash не заменяется actual file hash.
        var options = configured.Value; // Только verified result участвует в read-only owner START.
        var denied = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(options.StartRequest, cancellation.Token); // Clean missing с Denied обязан отказать до создания model/artifacts.
        var expectedDenial = !denied.IsSuccess && denied.ErrorCode == CodeDocumentationRepositoryOwnedModelCheckpointStartService.ColdStartNotAuthorizedErrorCode; // Реальный owner refusal не подменяется строкой конфигурации.
        Console.WriteLine(JsonSerializer.Serialize(new { configurationSha256 = expectedHash, tokenizerSha256 = options.Tokenizer.ComputeStateSha256(), actualConfigSha256 = TrainingSourceProvenance.Identify(options.StartRequest.ModelConfig).Sha256, options.StartRequest.BaseInitializationSeed, ordinaryColdStartDenied = !options.StartRequest.ColdStartPolicy.IsAuthorized, ownerError = denied.ErrorCode, expectedDenial, inferenceCalls = 0, artifactWrites = 0 }, Json)); // Readback содержит реальный reader и owner result, не model output.
        return expectedDenial ? 0 : 1; // Найденное accepted либо corrupt state требует другого owner, без fake baseline success.
    } // Этот verification режим не повторяет initialization/control experiment.

    private static int Run(string reason) // Единственный разрешённый initialization плюс один real TRAIN diagnostic.
    { // Подготовка и prediction имеют разные объявленные deadlines.
        using var preparation = new CancellationTokenSource(TimeSpan.FromSeconds(600)); // Prespecified preparation cap защищает дальнейшие эффекты.
        var started = Stopwatch.StartNew(); // Затраты не начинают считаться только после успешного inference.
        var evidence = Path.GetFullPath(Path.Combine(Root, EvidenceRelative)); // Owned output fixed before reading any source body.
        var configDirectory = Path.Combine(Root, "config", "code-documentation"); // Это live reader target, не supporting-files copy.
        if (!OperatingSystem.IsWindows() || Directory.Exists(configDirectory) || File.Exists(configDirectory) || File.Exists(Path.Combine(evidence, "preregistration.json"))) // New-only initialization не перезаписывает existing/corrupt state.
            return Refuse("CONTROL_NEW_ONLY_OR_PLATFORM_INVALID"); // Existing config требует read-only diagnosis другим owner.
        var exam = Path.Combine(Root, ExamRelative); // Ни одной wildcard discovery закрытых private bodies.
        VerifyFile(Path.Combine(exam, "panel-commitment.json"), PanelHash); // Подтверждаем actual public immutable pins.
        VerifyFile(Path.Combine(exam, "control-protocol.json"), ProtocolHash); // Decoder и thresholds нельзя поменять post-hoc.
        using var panel = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(exam, "panel-commitment.json"))); // Public hashes/partitions содержат no closed body.
        using var eligibility = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(exam, "selector-eligibility.json"))); // Принимаем только actual TRAIN allowlist.
        var permitted = eligibility.RootElement.GetProperty("trainingAllowedOpaqueFamilies").EnumerateArray().ToArray(); // Public metadata не становится Teacher grant.
        if (permitted.Length != 4 || !permitted.Select(value => value.GetProperty("caseId").GetString()).SequenceEqual(CaseIds)) return Refuse("CONTROL_TRAIN_ELIGIBILITY_MISMATCH"); // Fixed input set не выбирается по результату.
        var sources = new List<object>(); // Provenance captures actual paths/hashes only for allowlisted bytes.
        var ownerInputs = new List<string>(); // Исходные exact owner frames сохраняются без префикса.
        foreach (var id in CaseIds) // Никогда не открываем DEV/SEALED либо semantic-reference/rubric.
        { // Metadata и source должны принадлежать одной TRAIN case/family.
            var record = panel.RootElement.GetProperty("records").EnumerateArray().Single(value => value.GetProperty("caseId").GetString() == id); // Полный private manifest не нужен.
            var allowed = permitted.Single(value => value.GetProperty("caseId").GetString() == id); // Excluded families не участвуют в tokenizer.
            if (record.GetProperty("partition").GetString() != "TRAIN" || record.GetProperty("familyId").GetString() != allowed.GetProperty("familyId").GetString()) return Refuse("CONTROL_SOURCE_FAMILY_MISMATCH"); // Source-family role нельзя вывести из имени каталога.
            var directory = Path.Combine(exam, "private", "TRAIN", id); // Directory access ограничен exact allowlisted TRAIN path.
            var specimen = ReadSource(Path.Combine(directory, "specimen.cs"), allowed.GetProperty("sourceRawUtf8Sha256").GetString()!); // Source hash взят из actual eligibility.
            var input = ReadSource(Path.Combine(directory, "owner-input.txt"), record.GetProperty("ownerInputRawUtf8Sha256").GetString()!); // Exact raw framing нельзя реконструировать либо подменять.
            var question = ReadSource(Path.Combine(directory, "question.txt"), null); // Вопрос допустим как teaching input, не target/reference.
            if (!input.Contains(specimen, StringComparison.Ordinal) || !input.Contains(question, StringComparison.Ordinal)) return Refuse("CONTROL_OWNER_FRAME_MISMATCH"); // Подтверждаем вход, не добавляя answer hint.
            sources.Add(new { caseId = id, familyId = allowed.GetProperty("familyId").GetString(), sourceRole = "owner_authorized_original_scientific_teaching_source", externalProvider = "NONE", permissionScope = "tokenizer_input_and_one_diagnostic_only_not_DPO_admission", specimenSha256 = Hash(Strict.GetBytes(specimen)), ownerInputSha256 = Hash(Strict.GetBytes(input)), questionSha256 = Hash(Strict.GetBytes(question)) }); // Не придумываем third-party license либо paid-output provenance.
            ownerInputs.Add(input); // Только эти TRAIN frames формируют tokenizer corpus.
        } // Закрываем source-family и exact byte guard до artifacts.
        var tokenizer = new BpeTokenizer(); // Это actual production factory, не fixture/default256 словарь.
        tokenizer.Train(string.Join("\n", ownerInputs), ModelConfig.Small.VocabSize); // Реальная BPE обучается на allowed teaching inputs; unattainable target не набивается synthetic tokens.
        preparation.Token.ThrowIfCancellationRequested(); // Истёкшая подготовка запрещает дальнейшие live effects.
        if (tokenizer.VocabSize <= 256) return Refuse("CONTROL_NO_LEARNED_BPE_MERGES"); // Empty/tiny default tokenizer не считается Student configuration.
        var config = ModelConfig.Small with { VocabSize = tokenizer.VocabSize, ContextLen = 2048 }; // Hidden/layers/heads/FFN берутся из supported Small; vocab actual, context predeclared до prediction.
        var settings = new CodeDocumentationStudentGenerationSettings(256, 0, 0, 1, 20261014, 4096); // Ровно frozen decoder, no post-hoc knob change.
        var lora = LoraApplicator.ValidateAndCanonicalize(new LoraConfig { Rank = 8, Alpha = 16, Dropout = 0, TargetModules = ["q_proj", "v_proj"] }); // Actual project LoRA, serving control имеет объявленный zero dropout.
        var authority = new LoraPairRandomSeedAuthority(20261014, "rp01-policy", "rp01-reference"); // Независимые реальные policy/reference streams frozen before output.
        const ulong baseSeed = 20261014; // Explicit opt-in базы не меняет legacy global seed behavior.
        var fit = ownerInputs.Select((input, index) => new { caseId = CaseIds[index], promptTokens = tokenizer.Encode(input).Count, exactRoundTrip = tokenizer.Decode(tokenizer.Encode(input)) == input }).ToArray(); // Проверяем только четыре реально прочитанные TRAIN inputs.
        if (fit.Any(value => !value.exactRoundTrip || (long)value.promptTokens + 256 > config.ContextLen)) return Refuse("CONTROL_TRAIN_CONTEXT_OR_ROUNDTRIP_INVALID"); // Нет truncation или изменения exam case.
        using var canonical = new MemoryStream(); tokenizer.Save(canonical); // Existing canonical serializer задаёт tokenizer bytes.
        var tokenizerBytes = canonical.ToArray(); // Это настоящий BPE artifact с реальными merges.
        using var roundTrip = new MemoryStream(tokenizerBytes, false); // Production validator проверяет actual serialized history.
        var reopenedTokenizer = BpeTokenizer.ValidateAndLoadCanonicalArtifact(roundTrip, config.VocabSize, preparation.Token); // Никакой альтернативный parser не допускает artifact.
        if (Hash(tokenizerBytes) != reopenedTokenizer.ComputeStateSha256()) return Refuse("CONTROL_TOKENIZER_CANONICAL_HASH_MISMATCH"); // Raw hash и state hash обязаны совпасть.
        var trainerSources = SourcePins(["11.DPO/DpoTrainer.cs", "11.DPO/DpoTrainingSeededLoraTopology.cs", "14.ContinuousLearning/Training/DpoTrainingExecution.cs"]); // Три actual existing trainer sources определяют declared future trainer closure, не выполненный optimizer.
        var schemaSources = SourcePins(["03.TransformerModel/LlmModel.cs", "14.ContinuousLearning/Training/RepositoryOwnedModelCheckpointManifest.cs", "14.ContinuousLearning/Training/RepositoryOwnedModelCheckpointLoader.cs"]); // Existing full checkpoint/schema/loader source closure фиксируется честно.
        var runtime = new RepositoryOwnedModelRuntimeCompatibility(TrainingSourceProvenance.Identify(config).Sha256, LoraRandomStateIdentity.ComputeTopologySha256(config, lora, authority), Hash(tokenizerBytes), Hash(JsonSerializer.SerializeToUtf8Bytes(trainerSources, Json)), Hash(JsonSerializer.SerializeToUtf8Bytes(schemaSources, Json)), "rp01-small-csharp-bpe-train-only-v1"); // Ни одно поле не placeholder: методы/bytes записаны в provenance.
        var memory = ObserveMemory(); // До admission наблюдаем actual available whole-PC memory.
        if (memory is null) return Refuse("CONTROL_HOST_BUDGET_UNBOUND"); // Нет assumed 8 GiB floor либо fabricated host capacity.
        var processBudget = checked((long)(memory.Available * 3 / 4) + Process.GetCurrentProcess().WorkingSet64); // Process cap оставляет четверть реально свободной памяти concurrent PC.
        var privateDirectory = Path.Combine(evidence, "private"); // Частные unpublished веса не входят в accepted model tree.
        CreateCustodyDirectory(privateDirectory); // ACL только новых owned paths; global policy не меняется.
        var declaration = new { status = "UNTRAINED_PRIVATE_INITIAL_CONTROL_NOT_ADOPTED", diagnosticCase = CaseIds[0], sourceRights = sources, modelProfile = "ModelConfig.Small", modelConfig = config, settings, loraConfig = lora, adapterSeedAuthority = authority, baseInitializationSeed = baseSeed, runtimeCompatibility = runtime, trainerSources, schemaSources, fit, contextCoverage = "TRAIN_ONLY_4; DEV_SEALED_NOT_READ_NOT_PROVEN", framing = CodeDocumentationStudentGeneration.FramingVersion, teacherCalls = 0, evaluatorCalls = 0, optimizerUpdates = 0, dpoPairsConsumed = 0, parentManifest = (string?)null, controls = "feature/arbiter/kNN N/A: direct START->StudentGeneration->TextGenerator closure", actualOS = RuntimeInformation.OSDescription, runtimeVersion = RuntimeInformation.FrameworkDescription, cpuArchitecture = RuntimeInformation.ProcessArchitecture.ToString(), processorCount = Environment.ProcessorCount, precision = "actual System.Single CPU", memoryBefore = memory, historicalMemoryEnvelopeObservationBytes = processBudget, maximumPreparationSeconds = 600, maximumStudentCallSeconds = (int?)null, localStudentCancellation = "manual Ctrl+C/explicit stop/process shutdown only", monetaryCost = "NOT_MEASURED", vram = "NOT_MEASURED", authorizationReason = reason }; // Freeze settings and observed budget before first prediction.
        WriteNew(Path.Combine(evidence, "preregistration.json"), JsonSerializer.SerializeToUtf8Bytes(declaration, Json), false); // Доказательство реально применяется до model call.
        CreateCustodyDirectory(configDirectory); // Live config directory must pass actual production custody check.
        WriteNew(Path.Combine(configDirectory, "tokenizer.json"), tokenizerBytes, true); // New-only canonical BPE artifact; no copied fixture weights.
        var wire = new { version = "code-documentation-generation-server-config-v1", domainSlug = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug, designPatternSlug = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DesignPatternSlug, modelIdSlug = "rp01-csharp-small", modelVersion = "v1", modelConfig = new { config.VocabSize, config.ContextLen, config.HiddenDim, config.NumLayers, config.NumHeads, config.NumKvHeads, config.FfnHiddenDim, config.RopeTheta, config.RmsNormEps }, runtimeCompatibility = runtime, settings, loraConfig = new { lora.Rank, lora.Alpha, lora.Dropout, lora.TargetModules }, adapterSeedAuthority = authority, baseInitializationSeed = baseSeed }; // Exact strict reader schema contains no learned/adopted assertion.
        var configBytes = JsonSerializer.SerializeToUtf8Bytes(wire, Json); // Hash captures actual bytes that server startup will load.
        WriteNew(Path.Combine(configDirectory, "generation-v1.json"), configBytes, true); // Canonical live path is actually consumed below.
        var configured = CodeDocumentationGenerationConfiguration.ReadVerified(name => name switch { "XIIGEN_CODE_DOCUMENTATION_GENERATION_ENABLED" => "true", "XIIGEN_CODE_DOCUMENTATION_GENERATION_CONFIG_SHA256" => Hash(configBytes), _ => null }, Root, preparation.Token); // Invoke exact production reader through its result-only wrapper, no second parser.
        if (configured.Value is null) return Refuse(configured.ErrorCode ?? "CONTROL_CONFIGURATION_UNAVAILABLE"); // No mock or alternate reader fallback.
        if (configured.Value.StartRequest.ColdStartPolicy.IsAuthorized || configured.Value.StartRequest.BaseInitializationSeed != baseSeed || configured.Value.Tokenizer.ComputeStateSha256() != runtime.TokenizerSha256) return Refuse("CONTROL_READER_STATE_MISMATCH"); // Ordinary config must retain denied cold-start authority.
        var request = configured.Value.StartRequest with { ColdStartPolicy = RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize(reason) }; // Только отдельно authorized offline stage получает реальную reason, обычная options не меняется.
        var initial = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(request, preparation.Token); // Реальный owner проверяет clean missing/corrupt existing и создаёт actual Student.
        if (!initial.IsSuccess || initial.Value is null || initial.Value.ParentManifest is not null) return Refuse(initial.ErrorCode ?? "CONTROL_EXPECTED_CLEAN_INITIAL_STATE"); // Configured identity не переименовывает existing trained parent.
        using var reference = new MemoryStream(); initial.Value.Model.SaveCheckpointToStream(reference); // Reproducibility self-check использует full real serialized state.
        var independentInitialHash = TrainingSourceProvenance.Sha256(reference); // Это independently reconstructed before-prediction state, не иной inference run.
        WriteNew(Path.Combine(evidence, "configuration-readback.json"), JsonSerializer.SerializeToUtf8Bytes(new { configurationSha256 = Hash(configBytes), tokenizerSha256 = Hash(tokenizerBytes), ordinaryColdStartDenied = !configured.Value.StartRequest.ColdStartPolicy.IsAuthorized, exactModelConfigSha256 = TrainingSourceProvenance.Identify(configured.Value.StartRequest.ModelConfig).Sha256, sourceEvidence = initial.Value.EvidenceRefs, cleanInitialReason = initial.Value.ColdStartReason, independentInitialHash, independentInitialBytes = reference.Length }, Json), false); // Actual reader/START proof exists before prediction.
        preparation.Token.ThrowIfCancellationRequested(); // Preparation timeout forbids starting prediction.
        using var inference = new ManualStop(); // User override убрал automatic local Student deadline; только явная отмена прекращает Generate.
        var samples = new List<object>(); // Raw 1-second own-process/whole-PC observations stay in this receipt.
        using var timer = new Timer(_ => { var process = Process.GetCurrentProcess(); process.Refresh(); var sample = new { utc = DateTimeOffset.UtcNow, pid = process.Id, workingSet = process.WorkingSet64, peakWorkingSet = process.PeakWorkingSet64, privateBytes = process.PrivateMemorySize64, cpuSeconds = process.TotalProcessorTime.TotalSeconds, wholePcMemory = ObserveMemory() }; lock (samples) samples.Add(sample); }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1)); // Наблюдение ресурсов само по себе больше не отменяет local Student.
        var service = new CodeDocumentationGenerationService(configured.Value with { StartRequest = request }); // Реальный existing generation owner исполняет тот же tokenizer/settings/frame.
        var result = service.GenerateInitialControl(new("rp01-initial-train-diagnostic", request.DomainSlug, ownerInputs[0]), inference.Token); // Единственный детерминированно выбранный TRAIN diagnostic, не CODEX lesson selector.
        timer.Change(Timeout.Infinite, Timeout.Infinite); // Новые resource samples прекращаются после фактического вызова.
        var returned = result.Value; // Typed refusal и real receipt не смешиваются.
        var reproducible = returned?.Before?.FullCheckpointSha256 == independentInitialHash; // Второй owner-created actual state обязан совпасть с первым P.
        object[] observed; lock (samples) observed = samples.ToArray(); // Stable cost record не гоняется с timer callback.
        var receipt = new { status = "UNTRAINED_CONTROL_ONLY_NOT_LEARNING_NOT_ADOPTED", toolSuccess = result.IsSuccess, errorCode = result.ErrorCode, result = returned, reproducibleBeforePrediction = reproducible, configSha256Before = Hash(configBytes), configSha256After = Hash(File.ReadAllBytes(Path.Combine(configDirectory, "generation-v1.json"))), tokenizerSha256Before = Hash(tokenizerBytes), tokenizerSha256After = Hash(File.ReadAllBytes(Path.Combine(configDirectory, "tokenizer.json"))), outputRawUtf8Sha256 = returned?.Generation.Text is { } text ? Hash(Strict.GetBytes(text)) : null, elapsedSeconds = started.Elapsed.TotalSeconds, samples = observed, memoryAfter = ObserveMemory(), provider = "NONE", teacherCalls = 0, evaluatorCalls = 0, optimizerUpdates = 0, independentExam = "NOT_PERFORMED", POldCodeExecution = "NOT_PERFORMED", acceptedPublication = "NOT_PERFORMED", phase503Bypass = false }; // Включаем ошибки/таймауты без fake quality/pass.
        WriteNew(Path.Combine(evidence, "control-receipt.json"), JsonSerializer.SerializeToUtf8Bytes(receipt, Json), false); // Actual output/state/cost data durable независимо от outcome.
        Console.WriteLine(JsonSerializer.Serialize(new { result.IsSuccess, result.ErrorCode, generationError = returned?.Generation.ErrorCode, stopReason = returned?.Generation.StopReason, promptTokens = returned?.Generation.PromptTokenCount, generatedTokens = returned?.Generation.GeneratedTokenCount, reproducible, weightsUnchanged = returned?.Before?.FullCheckpointSha256 == returned?.After?.FullCheckpointSha256, configSha256 = Hash(configBytes), tokenizerSha256 = Hash(tokenizerBytes), elapsedSeconds = started.Elapsed.TotalSeconds }, Json)); // Public summary не раскрывает gold либо секреты.
        return result.IsSuccess && returned?.Generation.ErrorCode is null && reproducible ? 0 : 1; // Настоящая failure не заменяется successful tool exit.
    } // После этого процесса ни optimizer, ни Teacher, ни background work нет.

    private static int RepeatRetainedControl(string reason) // Ровно один отдельно разрешённый diagnostic повторяет тот же immutable control без timer.
    { // Не создаём новую конфигурацию/tokenizer/weights и не вызываем Teacher/optimizer.
        var evidence = Path.Combine(Root, EvidenceRelative); // Новый receipt остаётся в прежнем owned diagnostic surface.
        var preregistrationPath = Path.Combine(evidence, "repeat-preregistration.json"); // Сохранённый marker блокирует автоматический второй retained повтор.
        if (File.Exists(preregistrationPath)) return Refuse("CONTROL_RETAINED_REPEAT_ALREADY_ATTEMPTED"); // User override не превращается в безлимитный retry loop.
        VerifyFile(Path.Combine(Root, ExamRelative, "panel-commitment.json"), PanelHash); // Families/case allocation не менялись.
        VerifyFile(Path.Combine(Root, ExamRelative, "control-protocol.json"), ProtocolHash); // User-amended manual cancellation policy уже frozen до prediction.
        using var pinned = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(evidence, "configuration-readback.json"))); // Existing initial identity receipt содержит no closed answers.
        var configHash = pinned.RootElement.GetProperty("configurationSha256").GetString()!; // Принимаем прежний actual config hash, не arbitrary replacement.
        var tokenizerHash = pinned.RootElement.GetProperty("tokenizerSha256").GetString()!; // Retained diagnostic обязан читать ровно прежний BPE.
        var initialHash = pinned.RootElement.GetProperty("independentInitialHash").GetString()!; // Это сохранённый genuine untrained state, не accepted manifest.
        var configPath = Path.Combine(Root, "config", "code-documentation", "generation-v1.json"); // Путь реального reader фиксирован.
        var tokenizerPath = Path.Combine(Root, "config", "code-documentation", "tokenizer.json"); // Никакой synthetic/default tokenizer не подставляется.
        var snapshotPath = Path.Combine(evidence, "private", "initial.weights.bin"); // Retained owner открывает этот snapshot только read-only.
        VerifyFile(configPath, configHash); VerifyFile(tokenizerPath, tokenizerHash); VerifyFile(snapshotPath, initialHash); // Immutable bytes подтверждаются до нового local model call.
        using var stop = new ManualStop(); // Только Ctrl+C/явный stop/process shutdown, без automatic wall-clock/RAM cancellation.
        var configured = CodeDocumentationGenerationConfiguration.ReadVerified(name => name switch { "XIIGEN_CODE_DOCUMENTATION_GENERATION_ENABLED" => "true", "XIIGEN_CODE_DOCUMENTATION_GENERATION_CONFIG_SHA256" => configHash, _ => null }, Root, stop.Token); // Actual current production reader wrapper проверяет live files.
        if (!configured.IsSuccess || configured.Value is null) return Refuse(configured.ErrorCode); // Error не становится fallback initialization.
        using var panel = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(Root, ExamRelative, "panel-commitment.json"))); // Public metadata только для exact diagnostic case.
        var row = panel.RootElement.GetProperty("records").EnumerateArray().Single(value => value.GetProperty("caseId").GetString() == CaseIds[0]); // Тот же первый TRAIN, без opportunistic selection.
        if (row.GetProperty("partition").GetString() != "TRAIN") return Refuse("CONTROL_RETAINED_CASE_ROLE_INVALID"); // DEV/SEALED contents не читаются.
        var inputHash = row.GetProperty("ownerInputRawUtf8Sha256").GetString()!; // Exact input commitment сохраняется.
        var input = ReadSource(Path.Combine(Root, ExamRelative, "private", "TRAIN", CaseIds[0], "owner-input.txt"), inputHash); // Only previously authorized exact TRAIN owner input.
        var options = configured.Value; // Весь decoder/model/LoRA/tokenizer профиль остаётся неизменным.
        if (options.Tokenizer.Decode(options.Tokenizer.Encode(input)) != input || options.Tokenizer.ComputeStateSha256() != tokenizerHash) return Refuse("CONTROL_RETAINED_TOKENIZER_MISMATCH"); // Model input нельзя обрезать/переформатировать.
        var request = options.StartRequest with { ColdStartPolicy = RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize(reason) }; // Только offline stage использует прежнюю actual clean-start reason.
        var started = Stopwatch.StartNew(); // Наблюдаем полную стоимость нового diagnostic, не задаём deadline.
        WriteNew(preregistrationPath, JsonSerializer.SerializeToUtf8Bytes(new { userOverride = "кто установил этот лимит? Он не нужен, особенно в локальной модели", authorization = "Root relayed direct user authorization for exactly one retained diagnostic", protocolSha256 = ProtocolHash, priorFailedReceipt = "control-receipt.json", P = "NO_PRIOR_COMPARABLE_MODEL", currentRole = "C0 private untrained corrected no-update diagnostic; not accepted baseline", caseId = CaseIds[0], inputHash, configHash, tokenizerHash, initialHash, settings = options.Settings, settingsSha256 = CodeDocumentationStudentGeneration.SettingsSha256(options.Settings), options.StartRequest.BaseInitializationSeed, automaticStudentDeadlineSeconds = (int?)null, cancellation = "Ctrl+C/explicit stop/process shutdown only; actual runtime errors remain errors", hostMemoryBefore = ObserveMemory(), samplePeriodSeconds = 1, teacherCalls = 0, evaluatorCalls = 0, optimizerUpdates = 0, inferenceCallsAuthorized = 1, sourcePins = "repeat-source-pins.json", independentExam = "NOT_PERFORMED" }, Json), false); // Actual authorization/control freeze существует до owner START.
        var samples = new List<object>(); var sampleNumber = 0; // Cost history не теряется при manual cancellation/error.
        using var timer = new Timer(_ => // Observer не меняет модель, decoder, stop criterion либо concurrent apps.
        { // Записываем actual process и whole-PC physical memory раз в секунду.
            var process = Process.GetCurrentProcess(); process.Refresh(); // Не читаем и не завершаем чужие процессы.
            var sample = new { utc = DateTimeOffset.UtcNow, pid = process.Id, elapsedSeconds = started.Elapsed.TotalSeconds, workingSet = process.WorkingSet64, peakWorkingSet = process.PeakWorkingSet64, privateBytes = process.PrivateMemorySize64, cpuSeconds = process.TotalProcessorTime.TotalSeconds, wholePcMemory = ObserveMemory() }; // Actual observations, no estimate of model progress/quality.
            lock (samples) samples.Add(sample); // Receipt удерживает каждое наблюдение.
            if (Interlocked.Increment(ref sampleNumber) % 15 == 1) Console.WriteLine(JsonSerializer.Serialize(new { heartbeat = "retained Student diagnostic active", sample.pid, sample.elapsedSeconds, sample.workingSet, sample.cpuSeconds })); // Native heartbeat каждые 15 seconds не является execution timeout.
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1)); // Sampling продолжается до реального конца/ручной отмены.
        var service = new CodeDocumentationGenerationService(options with { StartRequest = request }); // Ровно existing production generation owner.
        var result = service.GenerateRetainedInitialControl(new("rp01-initial-train-diagnostic", request.DomainSlug, input), stop.Token); // Единственный user-authorized no-timeout repeat, никаких иных model calls.
        timer.Dispose(); // Observer команды прекращается после actual completion.
        object[] observed; lock (samples) observed = samples.ToArray(); // Stable observation history входит в durable receipt.
        var returned = result.Value; // Actual generation error/receipt не заменяется fabricated success.
        var retainedIdentical = returned?.Before?.FullCheckpointSha256 == initialHash && returned?.After?.FullCheckpointSha256 == initialHash; // До/после inference должны совпасть с immutable old snapshot.
        var record = new { status = "UNTRAINED_RETAINED_DIAGNOSTIC_NOT_LEARNING_NOT_ADOPTED", result.IsSuccess, result.ErrorCode, result = returned, sameImmutableInitialState = retainedIdentical, configSha256Before = configHash, configSha256After = Hash(File.ReadAllBytes(configPath)), tokenizerSha256Before = tokenizerHash, tokenizerSha256After = Hash(File.ReadAllBytes(tokenizerPath)), snapshotSha256Before = initialHash, snapshotSha256After = Hash(File.ReadAllBytes(snapshotPath)), inputSha256 = inputHash, outputRawUtf8Sha256 = returned?.Generation.Text is { } text ? Hash(Strict.GetBytes(text)) : null, elapsedSeconds = started.Elapsed.TotalSeconds, samples = observed, hostMemoryAfter = ObserveMemory(), automaticDeadlineSeconds = (int?)null, manualCancellationRequested = stop.Token.IsCancellationRequested, teacherCalls = 0, evaluatorCalls = 0, optimizerUpdates = 0, dpoPairsConsumed = 0, P = "NO_PRIOR_COMPARABLE_MODEL", independentExam = "NOT_PERFORMED", ordinaryEndpointAdoption = "NOT_PERFORMED", provider = "NONE" }; // No P score, accepted baseline, pair or quality invented.
        WriteNew(Path.Combine(evidence, "repeat-control-receipt.json"), JsonSerializer.SerializeToUtf8Bytes(record, Json), false); // Новый результат не перезаписывает historical timeout.
        Console.WriteLine(JsonSerializer.Serialize(new { result.IsSuccess, result.ErrorCode, generationError = returned?.Generation.ErrorCode, stopReason = returned?.Generation.StopReason, promptTokens = returned?.Generation.PromptTokenCount, generatedTokens = returned?.Generation.GeneratedTokenCount, retainedIdentical, outputRawUtf8Sha256 = record.outputRawUtf8Sha256, elapsedSeconds = started.Elapsed.TotalSeconds, samples = observed.Length }, Json)); // Actual completed/error outcome виден native caller.
        return result.IsSuccess && returned?.Generation.ErrorCode is null && retainedIdentical ? 0 : 1; // Ошибка либо изменение state остаются реальными failure.
    } // User authorization не позволяет следующий repeat после этого marker.

    private sealed class ManualStop : IDisposable // Local cancellation исходит только из explicit console/process lifecycle.
    { // Нет Timer/CancelAfter или resource threshold в этом owner.
        private readonly CancellationTokenSource _source = new(); // Token передаётся existing production generator.
        private readonly ConsoleCancelEventHandler _interrupt; private readonly EventHandler _shutdown; // Сохраняем handlers для корректного удаления.
        internal CancellationToken Token => _source.Token; // Worker не создаёт другой скрытый deadline token.
        internal ManualStop() // Привязываем фактическое ручное прерывание текущего процесса.
        { // Console Ctrl+C остаётся cooperative cancellation для генератора.
            _interrupt = (_, args) => { args.Cancel = true; _source.Cancel(); }; // Explicit Ctrl+C прекращает local model, давая сохранить readback.
            _shutdown = (_, _) => _source.Cancel(); // Process shutdown также передаёт cooperative stop.
            Console.CancelKeyPress += _interrupt; AppDomain.CurrentDomain.ProcessExit += _shutdown; // Только текущий process, no daemon/global hook.
        } // Периодический timer сюда не подключён.
        public void Dispose() { Console.CancelKeyPress -= _interrupt; AppDomain.CurrentDomain.ProcessExit -= _shutdown; _source.Dispose(); } // IDisposable очищает только собственные handlers/token.
    } // Local manual-stop scope не является trainer/runtime scheduler.

    private static string ReadSource(string path, string? expected) // Exact allowlisted teaching source читается один раз с finite byte budget.
    { // Symlink/reparse source не допускается.
        if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0 || new FileInfo(path).Length > 65536) throw new IOException("CONTROL_SOURCE_BOUNDARY_INVALID"); // Реальный error не теряется в summary.
        var bytes = File.ReadAllBytes(path); // Нет directory recursion/Gold read.
        if (expected is not null && Hash(bytes) != expected) throw new IOException("CONTROL_SOURCE_HASH_MISMATCH"); // Commitment проверяется до tokenizer training.
        return Strict.GetString(bytes); // Verbatim owner UTF-8 сохраняется полностью.
    } // Отказ не вызывает model/update.
    private static void VerifyFile(string path, string expected) { if (Hash(File.ReadAllBytes(path)) != expected) throw new IOException("CONTROL_PUBLIC_COMMITMENT_CHANGED"); } // Actual immutable public bytes защищают frozen protocol.
    private static object[] SourcePins(string[] paths) => paths.Select(path => (object)new { path, rawSha256 = Hash(File.ReadAllBytes(Path.Combine(Root, path))) }).ToArray(); // Hashes derived from current source closure, not pathname claims.
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(); // Канонический lowercase hash actual bytes.
    private static int Refuse(string code) { Console.Error.WriteLine(code); return 1; } // Отказ всегда имеет реальный CLI exit.
    private static void CreateCustodyDirectory(string path) // Только новые owned output directories получают custody.
    { // Чужой existing directory нельзя считать new owner allocation.
        var recoveryPath = Path.Combine(Root, EvidenceRelative, "private"); // Единственное допустимое recovery — новый пустой private каталог этой попытки.
        if (File.Exists(path) || Directory.Exists(path) && (Path.GetFullPath(path) != Path.GetFullPath(recoveryPath) || Directory.EnumerateFileSystemEntries(path).Any() || !File.Exists(Path.Combine(Root, EvidenceRelative, "runner-attempt-01.json")))) throw new IOException("CONTROL_DIRECTORY_EXISTS"); // Foreign/existing content не перезаписывается.
        Directory.CreateDirectory(path); // Новый target не является worktree/clone.
        var security = new DirectorySecurity(); // Новый DACL строится для этого directory only.
        security.SetAccessRuleProtection(true, false); // Наследуемые широкие права отключаются только у нового target.
        using var user = WindowsIdentity.GetCurrent(); // Actual running owner устанавливается из Windows identity.
        // Уже созданный Windows каталог принадлежит текущему owner; отдельная WRITE_OWNER операция не нужна.
        foreach (var sid in CustodySids(user.User!)) security.AddAccessRule(new FileSystemAccessRule(sid, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow)); // Same required SID set as existing SecureLocalFileBoundary.
        new DirectoryInfo(path).SetAccessControl(security); // Локальная ACL mutation не меняет global security policy.
    } // Actual production reader independently проверит this custody.
    private static SecurityIdentifier[] CustodySids(SecurityIdentifier owner) => new[] { owner, new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null) }.DistinctBy(sid => sid.Value).ToArray(); // Точный owner/SYSTEM/Admin set matches production boundary.
    private static void WriteNew(string path, byte[] bytes, bool custody) // Любой file write имеет CreateNew и durable flush.
    { // Existing bytes не заменяются, даже при interrupted prior attempt.
        using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None)) { stream.Write(bytes); stream.Flush(true); } // Полный write не маскирует I/O failure.
        if (!custody) return; // Public receipts не выдаются за secret custody.
        var security = new FileSecurity(); security.SetAccessRuleProtection(true, false); // Только новый canonical file получает protected ACL.
        using var user = WindowsIdentity.GetCurrent(); // Новый файл уже принадлежит фактическому текущему owner; меняется только DACL.
        foreach (var sid in CustodySids(user.User!)) security.AddAccessRule(new FileSystemAccessRule(sid, FileSystemRights.FullControl, AccessControlType.Allow)); // Same required non-inherited FullControl ACEs.
        new FileInfo(path).SetAccessControl(security); // Production reader проверяет собственную реальную handle/custody boundary.
    } // Никакие foreign/global ACL не меняются.
    private sealed record HostMemory(ulong Total, ulong Available); // Именованные properties действительно сериализуются в cost receipt.
    private static HostMemory? ObserveMemory() // Фактическая physical RAM через Windows API, no invented VRAM.
    { // API failure является unbound observation.
        var state = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() }; // Native structure size declared by actual runtime.
        return GlobalMemoryStatusEx(ref state) ? new(state.TotalPhysical, state.AvailablePhysical) : null; // Whole-PC sample содержит измеренную total/free physical memory.
    } // Чтение host state не вмешивается в другие приложения.
    [StructLayout(LayoutKind.Sequential)] private struct MemoryStatus { public uint Length, Load; public ulong TotalPhysical, AvailablePhysical, TotalPageFile, AvailablePageFile, TotalVirtual, AvailableVirtual, AvailableExtendedVirtual; } // Native Windows MEMORYSTATUSEX layout used observation-only.
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GlobalMemoryStatusEx(ref MemoryStatus state); // Read-only native host measurement.
} // Worker executor completes one bounded initialization/control effect.
