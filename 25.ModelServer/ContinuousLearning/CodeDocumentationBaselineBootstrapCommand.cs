// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
using AgentInfrastructure.Logical; // Используем прежний классификатор некритических исключений для безопасного отказа.
using ContinuousLearning; // Читаем существующие repository-owned контракты manifest и допуска DPO.
using TrainableRouter.Training; // Переиспользуем владельцев START и checkpoint publication.

namespace ModelServer; // Команда остаётся внутренней частью executable ModelServer.

internal sealed record CodeDocumentationBaselineBootstrapResult( // Возвращаем узкий результат отдельной offline-стадии bootstrap.
    string Status, string Message, string? ErrorCode = null, // Передаём оператору статус и объяснение точной причины отказа.
    CodeDocumentationRepositoryOwnedModelCheckpointPublicationEvidence? Publication = null) // Сохраняем publication evidence только когда владелец вернул её после записи.
{ // Коды завершения относятся к отдельной bootstrap-команде.
    internal int ExitCode => Status is "INITIAL_BASELINE_CREATED" or "ALREADY_PRESENT" ? 0 : // Успешный initial baseline или уже существующая модель завершают процесс без ошибки.
        Status == "RECONCILIATION_REQUIRED" ? 2 : 1; // Неопределённый итог публикации требует reconciliation, остальные отказы дают код 1.
} // Завершаем envelope результата, не создавая дополнительный runner.

// Отдельная offline-композиция сохраняет прежний запрет скрытого cold start у generation host и DPO coordinator.
internal static class CodeDocumentationBaselineBootstrapCommand // Внутренний entry не становится HTTP-властью на модель или обучение.
{ // Все bootstrap-проверки живут в одной offline-композиции.
    internal const string CommandArgument = "--code-documentation-bootstrap-parent"; // Распознаём только отдельную явную команду начальной публикации.
    internal const string EnabledEnvironmentName = "XIIGEN_CODE_DOCUMENTATION_BASELINE_BOOTSTRAP_ENABLED"; // Новое startup opt-in независимо разрешает именно эту стадию.
    internal const string DatasetVersionArgument = "--dataset-version"; // Оператор выбирает сохранённую dataset version без произвольного пути.
    internal const string DatasetHashArgument = "--dataset-manifest-sha256"; // Оператор закрепляет точную текущую идентичность DPO manifest.

    internal static bool IsRequested(string[] args) // Определяем bootstrap-намерение до обычного серверного запуска.
        => args.Any(argument => argument == CommandArgument || // Явный ключ направляет управление в bootstrap parser.
            argument.StartsWith(CommandArgument + "=", StringComparison.Ordinal)); // Неподдерживаемый присоединённый аргумент тоже попадает в parser и получает отказ.

    // Production entry закрепляет canonical root и process-owned startup inputs.
    internal static CodeDocumentationBaselineBootstrapResult Run(CancellationToken token, string[] args) // Единственный production entry берёт root и environment из процесса ModelServer.
        => RunCore(args, Environment.GetEnvironmentVariable, // Захватываем startup только в существующем offline-пути.
            Program.RepositoryOwnedTrainingTrustedRepositoryRoot, token); // Canonical root берётся из доверенной серверной конфигурации.

    // Отдельный внутренний synthetic fixture не добавляет публичного CLI root override.
    internal static CodeDocumentationBaselineBootstrapResult RunSyntheticFixtureForTesting( // Изолированный fixture получает свой synthetic root без production CLI override.
        string[] args, Func<string, string?> readEnvironment, string syntheticRepositoryRoot, // Synthetic entry сохраняет те же аргументы и проверки startup.
        CancellationToken token = default) // Fixture может передать отмену до записи checkpoint.
        => RunCore(args, readEnvironment, syntheticRepositoryRoot, token); // Fixture использует точно тот же алгоритм bootstrap.

    private static CodeDocumentationBaselineBootstrapResult RunCore(string[] args, // Объединяем разрешение, чтение и публикацию одной ограниченной команды.
        Func<string, string?> readEnvironment, string root, CancellationToken token) // Startup reader и root передаются только существующими внутренними entry.
    { // Неопределённый результат записи отслеживается отдельно от отказа до публикации.
        var publicationAttempted = false; // До вызова publication отказ ещё не требует сверки записанных bytes.
        CodeDocumentationRepositoryOwnedModelCheckpointPublicationEvidence? publication = null; // Сохраняем фактическое evidence последней попытки публикации.
        try // Некритические ошибки и отмена имеют отдельные безопасные ответы.
        { // Выполняем bootstrap только внутри этого защищённого envelope.
            // Разбираем всё операторское намерение до artifact/custody/model owners.
            if (!TryParse(args, out var version, out var expectedManifestHash)) // Проверяем полную уникальную команду до чтения config и модели.
                return Refused("BOOTSTRAP_ARGUMENTS_INVALID", "Первичная публикация отклонена: нужны точные уникальные параметры команды, версии DPO и SHA manifest."); // Отказываем неполному или неизвестному операторскому намерению без artifact effects.
            var names = new[] { EnabledEnvironmentName, Program.RepositoryOwnedTrainingEnabledEnvironmentName, // Список startup inputs включает новый stage switch и прежнее разрешение training route.
                Program.RepositoryOwnedTrainingAllowColdStartEnvironmentName, Program.RepositoryOwnedTrainingColdStartReasonEnvironmentName, // Отдельно захватываем cold-start switch и его человеческую причину.
                Program.RepositoryOwnedTrainingProvenanceRefsEnvironmentName, Program.RepositoryOwnedTrainingRepositoryRootEnvironmentName, // Захватываем происхождение и optional asserted root до любых владельцев модели.
                CodeDocumentationGenerationConfiguration.EnabledEnvironmentName, CodeDocumentationGenerationConfiguration.HashEnvironmentName }; // Захватываем прежние generation opt-in и точный config hash.
            var startup = names.ToDictionary(name => name, readEnvironment, StringComparer.Ordinal); // Читаем каждое startup значение один раз, чтобы authority не менялась между gates.
            if (!IsTrue(startup[EnabledEnvironmentName]) || !IsTrue(startup[Program.RepositoryOwnedTrainingEnabledEnvironmentName]) || // Все разрешения должны быть явным true, а не просто присутствующими строками.
                !IsTrue(startup[Program.RepositoryOwnedTrainingAllowColdStartEnvironmentName])) // Без отдельного cold-start разрешения fresh baseline не создаётся.
                return Refused("BOOTSTRAP_STAGE_NOT_AUTHORIZED", "Первичная публикация не разрешена оператором для этой отдельной стадии."); // Неавторизованная стадия возвращает отказ до config или custody чтения.
            var reason = startup[Program.RepositoryOwnedTrainingColdStartReasonEnvironmentName]; // Берём точную человеческую причину из уже захваченного startup.
            var rawRefs = startup[Program.RepositoryOwnedTrainingProvenanceRefsEnvironmentName]; // Берём исходные provenance refs без скрытого исправления текста.
            var refs = rawRefs?.Split(';') ?? []; // Разделяем refs только по установленному серверному разделителю.
            if (!BoundedText(reason) || refs.Length is < 1 or > 32 || refs.Any(reference => !BoundedText(reference)) || // Проверяем длину, содержание и ограниченное количество evidence refs.
                refs.Distinct(StringComparer.Ordinal).Count() != refs.Length) // Повторный ref не выдаётся за отдельное происхождение.
                return Refused("BOOTSTRAP_STARTUP_INVALID", "Первичная публикация отклонена: отсутствуют точная причина холодного старта или допустимые provenance refs."); // Ошибочная причина или refs закрывают стадию до модели.
            var assertedRoot = startup[Program.RepositoryOwnedTrainingRepositoryRootEnvironmentName]; // Optional root утверждает только уже закреплённый repository identity.
            if (assertedRoot is not null && !SamePath(assertedRoot, root)) // Чужой asserted root не перенаправляет bootstrap к другим файлам.
                return Refused("BOOTSTRAP_ROOT_INVALID", "Первичная публикация отклонена: startup root не совпадает с закреплённым репозиторием."); // Отказ сообщает точное несовпадение startup root.
            token.ThrowIfCancellationRequested(); // Отмена до configuration не допускает дальнейших effects.

            // Config остаётся strict и denied; только отдельно разрешённая offline-стадия копирует cold-start policy.
            var configured = CodeDocumentationGenerationConfiguration.Resolve(name => startup.GetValueOrDefault(name), root, token); // Переиспользуем strict generation config owner с прежним denied cold-start запросом.
            if (configured.Options is null) // Непрошедший профиль или tokenizer не заменяется default-моделью.
                return Refused(configured.ErrorCode!, "Первичная публикация отклонена: generation profile или tokenizer не прошли существующую проверку."); // Возвращаем точную ошибку существующего configuration owner.
            var deniedRequest = configured.Options.StartRequest; // Сохраняем denied запрос для независимого свежего readback после публикации.
            var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(root, // Canonical DPO address строится владельцем из проверенной модели и выбранной версии.
                deniedRequest.DomainSlug, deniedRequest.DesignPatternSlug, version!); // Domain и design pattern никогда не берутся из свободного CLI пути.
            if (!address.IsSuccess || address.Value is null) // Ошибочный canonical address прекращает bootstrap до чтения DPO.
                return Refused(address.ErrorCode!, "Первичная публикация отклонена: адрес сохранённого DPO недопустим."); // Оператор получает причину отказа существующего layout owner.
            var identity = RepositoryOwnedDpoDatasetManifestSealer.OpenManifestIdentityOnly(address.Value); // Открываем текущую saved manifest identity через существующий DPO owner.
            if (!identity.IsSuccess || identity.Value is null || identity.Value.Manifest.ManifestSha256 != expectedManifestHash) // Не принимаем устаревший или отличный от выбранного оператором manifest hash.
                return Refused(ErrorOr(identity.ErrorCode, "BOOTSTRAP_DPO_IDENTITY_MISMATCH"), "Первичная публикация отклонена: текущий DPO manifest не совпадает с выбранным оператором."); // Возвращаем точную ошибку чтения либо identity mismatch, даже при пустом owner code.
            var dataset = RepositoryOwnedDpoDatasetManifestSealer.OpenSealedTrainingEligible(address.Value, identity.Value.RequestIdentity); // Полный saved DPO open повторно проверяет bytes и persisted governance admission.
            if (!dataset.IsSuccess || dataset.Value is null || dataset.Value.Manifest.ManifestSha256 != expectedManifestHash) // Sealed identity не заменяет проверку actual pair files и допуска материала.
                return Refused(ErrorOr(dataset.ErrorCode, "BOOTSTRAP_DPO_IDENTITY_MISMATCH"), "Первичная публикация отклонена: полные DPO bytes или сохранённое разрешение на материал не прошли проверку."); // Повреждённые или недопущенные DPO bytes прекращают стадию без checkpoint.
            token.ThrowIfCancellationRequested(); // Отмена после DPO проверки сохраняет границу до модели.
            var request = deniedRequest with { ColdStartPolicy = RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize(reason!) }; // Только авторизованная offline-стадия копирует strict запрос с точной cold-start причиной.
            token.ThrowIfCancellationRequested(); // Последняя проверка отмены предшествует вызову публикации.
            var dpoIdentity = new RepositoryOwnedDpoDatasetManifestIdentity(address.Value.ManifestRelativePath, dataset.Value.Manifest.ManifestSha256); // Исходный baseline сохраняет подлинную DPO identity как provenance без optimizer update.
            var published = CodeDocumentationRepositoryOwnedModelCheckpointPublicationService.PublishInitialBaseline(request, // Initial publication owner сам создаёт fresh модель и не получает caller Model.
                dpoIdentity, DateTimeOffset.UtcNow, Array.AsReadOnly(refs), token); // Передаём точную saved DPO identity, UTC время и захваченные refs в прежнюю transaction.
            if (!published.IsSuccess || published.Value is null) // Владелец подтверждает, что отказ произошёл до входа в checkpoint sealer.
                return Refused(ErrorOr(published.ErrorCode, "BOOTSTRAP_UNAVAILABLE"), "Первичная публикация отклонена владельцем START/publication до записи checkpoint."); // Ранний отказ не называется неизвестной записью и не запускает fallback.
            if (published.Value is CodeDocumentationInitialBaselinePublicationOutcome.AlreadyPresent) // Один owner START уже нашёл совместимую сохранённую модель.
                return new("ALREADY_PRESENT", "Сохранённая совместимая модель уже существует. Первичная публикация не выполнена; новый шаг не создан."); // Команда не создаёт precheck Model и не вызывает второй initial START.
            if (published.Value is CodeDocumentationInitialBaselinePublicationOutcome.ReconciliationRequired uncertain) // Owner сообщил ошибку после возможной записи checkpoint.
                return Reconcile(uncertain.ErrorCode, null); // Не повторяем публикацию при неизвестном disk outcome.
            if (published.Value is not CodeDocumentationInitialBaselinePublicationOutcome.Created created) // Неизвестный тип результата не становится successful baseline.
                return Refused("BOOTSTRAP_PUBLICATION_OUTCOME_INVALID", "Первичная публикация отклонена: неизвестный результат владельца."); // Fail closed без Model либо строкового caller authority.
            publicationAttempted = true; // Written/reopened evidence требует reconciliation при любом последующем сбое readback.
            publication = created.Publication; // Сохраняем только фактическое evidence уже созданного checkpoint.
            var fresh = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(deniedRequest, token); // Свежий START с запретом cold start должен открыть только сохранённое состояние.
            var manifest = fresh.Value?.ParentManifest; // Проверяем manifest реального disk-reopen, а не только returned publication label.
            if (!fresh.IsSuccess || manifest is null || manifest.Step != 1 || manifest.ParentCheckpoint is not null || // Baseline обязан быть ровно parentless Step 1 и успешно загружаться через denied START.
                manifest.Schema != RepositoryOwnedModelCheckpointManifestSealer.InitialBaselineSchema || // Новая sealed initial схема отличает происхождение от applied history.
                manifest.DpoApplication != RepositoryOwnedModelCheckpointDpoApplication.InitialBaselineNoUpdate || // Типизированная роль должна подтвердиться после parser/hash/weights validation.
                manifest.ManifestSha256 != publication.ManifestSha256 || manifest.WeightsSha256 != publication.WeightsSha256 || // Свежий manifest и веса должны иметь ровно identity опубликованного состояния.
                manifest.WeightsByteLength != publication.WeightsByteLength || manifest.DpoDatasetManifest != dpoIdentity || // Проверяем фактическую длину весов и происхождение выбранного DPO.
                manifest.RuntimeCompatibility != deniedRequest.RuntimeCompatibility || manifest.Domain != deniedRequest.DomainSlug || // Runtime fingerprint и домен должны совпасть с strict generation request.
                manifest.DesignPattern != deniedRequest.DesignPatternSlug || manifest.ModelId != deniedRequest.ModelIdSlug || // Design pattern и модель должны принадлежать исходной canonical ветке.
                manifest.Version != deniedRequest.ModelVersion || manifest.ColdStartReason != reason || // Версия и точная причина должны остаться теми же после чтения с диска.
                fresh.Value!.ColdStartReason is not null) // Fresh denied load не должен сообщать новый cold-start переход.
                return Reconcile(ErrorOr(fresh.ErrorCode, "BOOTSTRAP_DENIED_RELOAD_MISMATCH"), publication); // Любое расхождение readback требует сверки диска без следующего checkpoint.
            return new("INITIAL_BASELINE_CREATED", "Сохранено необученное начальное состояние, шаг 1. Свежий START с запретом cold start загрузил те же repository-owned bytes. DPO-пары не использованы для обновления весов; качество обучения не измерялось.", Publication: publication); // Успех описывает только сохранение необученного состояния и его повторную загрузку.
        } // Завершаем безопасную успешную bootstrap-композицию.
        catch (OperationCanceledException) when (token.IsCancellationRequested) // Обрабатываем только отмену текущего переданного token.
        { // Отмена не должна запускать повторную публикацию.
            return publicationAttempted ? Reconcile("BOOTSTRAP_CANCELLED", publication) : // После попытки публикации отмена остаётся неопределённым disk outcome.
                Refused("BOOTSTRAP_CANCELLED", "Первичная публикация отменена до записи checkpoint."); // До publication отмена означает отказ до записи checkpoint.
        } // Завершаем cancellation envelope отдельной стадии.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Некритические ошибки config, custody и model owners возвращают безопасный статус.
        { // Не позволяем ошибке перейти в fallback или продолжение обучения.
            return publicationAttempted ? Reconcile("BOOTSTRAP_UNAVAILABLE", publication) : // После publication exception требует reconciliation текущего состояния.
                Refused("BOOTSTRAP_UNAVAILABLE", "Первичная публикация отклонена: конфигурация или repository-owned материалы недоступны."); // До publication недоступность остаётся обычным отказом.
        } // Завершаем обработку некритических исключений.
    } // Закрываем алгоритм отдельного bootstrap entry.

    private static bool TryParse(string[] args, out string? version, out string? hash) // Parser принимает ровно ключ команды, dataset version и SHA256.
    { // Неподдержанные параметры не проходят к filesystem owner.
        version = hash = null; // Не используем остаточные значения неполной команды.
        var seen = new HashSet<string>(StringComparer.Ordinal); // Каждый операторский ключ должен встретиться ровно один раз.
        for (var index = 0; index < args.Length; index++) // Разбираем аргументы последовательно, сохраняя границы значений.
        { // Значение привязано только к непосредственно предшествующему разрешённому ключу.
            var argument = args[index]; // Берём очередной ключ команды без trim или исправления.
            if (!seen.Add(argument)) return false; // Повтор аргумента закрывает bootstrap до любого artifact чтения.
            if (argument == CommandArgument) continue; // Флаг команды не имеет собственного значения.
            if (argument is not (DatasetVersionArgument or DatasetHashArgument) || ++index == args.Length) return false; // Неизвестный ключ или отсутствие значения даёт отказ parser.
            if (argument == DatasetVersionArgument) version = args[index]; else hash = args[index]; // Связываем выбранную версию или SHA с их точным ключом.
        } // Завершаем разбор очередной пары ключ и значение.
        return seen.Contains(CommandArgument) && !string.IsNullOrEmpty(version) && // Команда и непустая dataset version обязательны для допуска.
            version.Length <= 128 && version.All(c => c is >= 'a' and <= 'z' or >= '0' and <= '9' or '-' or '_' or '.') && // Ограничиваем version безопасным алфавитом и длиной перед canonical layout owner.
            hash is { Length: 64 } && hash.All(c => c is >= 'a' and <= 'f' or >= '0' and <= '9'); // SHA должен быть ровно lowercase hex длиной 64, без скрытой нормализации.
    } // Завершаем строгий parser операторского намерения.
    private static bool IsTrue(string? value) => string.Equals(value, "true", StringComparison.OrdinalIgnoreCase); // Startup opt-in допускается только как точное true без whitespace fallback.
    private static string ErrorOr(string? code, string fallback) => string.IsNullOrEmpty(code) ? fallback : code; // Пустой owner error code заменяется стабильной причиной отказа.
    private static bool BoundedText(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 512 && // Человеческая причина или ref обязаны быть непустыми и ограниченными по длине.
        value == value.Trim() && !value.Any(char.IsControl); // Control chars и краевой whitespace не исправляются молча.
    private static bool SamePath(string left, string right) => string.Equals(Path.TrimEndingDirectorySeparator(Path.GetFullPath(left)), // Сравниваем нормализованные абсолютные пути, не перенаправляя canonical root.
        Path.TrimEndingDirectorySeparator(Path.GetFullPath(right)), OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal); // Windows path identity использует прежнее регистронезависимое сравнение.
    private static CodeDocumentationBaselineBootstrapResult Refused(string code, string message) => new("REFUSED", message, code); // Отказы до publication дают оператору стабильный тип результата.
    private static CodeDocumentationBaselineBootstrapResult Reconcile(string code, CodeDocumentationRepositoryOwnedModelCheckpointPublicationEvidence? publication) // Отдельный reconciliation envelope несёт только реально полученное evidence.
        => new("RECONCILIATION_REQUIRED", "Публикация была вызвана; итоговый checkpoint требует сверки с диском. Не повторяйте запись и не создавайте следующий шаг.", code, publication); // Оператор должен сверить неизвестный disk outcome, а не создать новый шаг.
} // Закрываем offline bootstrap, не запускающий host или trainer.
