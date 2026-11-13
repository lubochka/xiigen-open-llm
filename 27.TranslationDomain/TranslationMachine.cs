// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class TranslationMachine : ILearningMachine<TranslationDomain>
{
    public const string MachineId = "translation-machine";
    public const string DomainTag = "TranslationDomain";

    // StateVersionMetricKey — имя показания, в котором КАЖДЫЙ запрос сообщает
    // номер обслужившей его версии состояния. Это настоящая точка трассы
    // продукта, а не украшение отчёта: номер берётся из того же единственного
    // разрешения версии, по которому запрос и считал перевод. По нему обычный
    // потребитель и приёмка видят, что весь запрос прошёл по одной версии.
    public const string StateVersionMetricKey = "state_version";
    private const double LegacyUnverifiedRewardMargin = 0.03d;

    // Начальный номер версии состояния, с которого стартует необученная машина.
    private const string InitialSemanticVersion = "0.1.0";

    // _publishedVersion — ЕДИНСТВЕННАЯ изменяемая ссылка этой машины. Она
    // указывает на одну согласованную опубликованную версию состояния: поток
    // вывода, обучатели единиц, типизированные единицы, номер версии, точность,
    // историю обучения и опознания учебных строк. Раньше эти же данные лежали
    // отдельными полями, и обновление одного поля оставляло остальные части
    // графа на прежних объектах: замена памяти не доходила до уже выданного
    // дирижёра. Теперь публикация новой версии — это одна запись этой ссылки.
    private TranslationPublishedVersion _publishedVersion;

    // Default constructor creates a fresh machine with a full runtime; supports checkpoint export/import.
    public TranslationMachine() : this(CreateRuntime()) { }

    // Runtime constructor publishes the first version from a CreateRuntime() result.
    private TranslationMachine(TranslationRuntime runtime)
    {
        // Первая версия собирается целиком из одного связанного исполнения:
        // поток, обучатели и типизированные единицы приходят из него вместе.
        _publishedVersion = CreateInitialVersion(runtime);
    }

    public TranslationMachine(TranslationRoutingFlow flow)
    {
        // Путь внедрения потока используется проверками: типизированного
        // исполнения здесь нет, поэтому выгрузка состояния останется
        // недоступной, а обучателей единиц в этой версии ноль.
        var injectedFlow = flow ?? throw new ArgumentNullException(nameof(flow));
        // Первая версия этого пути несёт только поток вывода и пустой набор
        // обучателей: остальные части графа отсутствуют по построению.
        _publishedVersion = new TranslationPublishedVersion(
            VersionNumber: 1,
            Flow: injectedFlow,
            UnitTrainers: new Dictionary<string, ITranslationUnitTrainer>(StringComparer.Ordinal),
            Runtime: null,
            SemanticVersion: InitialSemanticVersion,
            AccuracyAfterLastBatch: 0.0,
            HasStructuredTraining: false,
            TrainingHistory: [],
            StructuredTrainingIdentities: []);
    }

    // CreateInitialVersion собирает первую опубликованную версию из связанного
    // исполнения. Отдельный метод нужен и конструктору, и подготовке копии.
    private static TranslationPublishedVersion CreateInitialVersion(TranslationRuntime runtime)
        // Все части версии берутся из одного исполнения, поэтому поток, обучатели
        // и типизированные единицы заведомо указывают на одни и те же объекты.
        => new(
            VersionNumber: 1,
            Flow: runtime.Flow,
            UnitTrainers: runtime.UnitTrainers,
            Runtime: runtime,
            SemanticVersion: InitialSemanticVersion,
            AccuracyAfterLastBatch: 0.0,
            HasStructuredTraining: false,
            TrainingHistory: [],
            StructuredTrainingIdentities: []);

    // CurrentVersion читает текущую опубликованную версию одним чтением ссылки.
    // Чтение через Volatile гарантирует, что вызывающий увидит именно последнюю
    // опубликованную версию, а не устаревшую копию ссылки из кеша процессора.
    private TranslationPublishedVersion CurrentVersion()
        // Одно чтение ссылки: вызывающий дальше работает с неизменной версией.
        => System.Threading.Volatile.Read(ref _publishedVersion);

    // CurrentStateVersion — номер опубликованной версии состояния, видимый снаружи.
    //
    // Зачем он открыт. Вызывающий, который готовит изменение от конкретной версии, обязан иметь
    // возможность назвать её при публикации. Без этого номера условную публикацию нельзя ни
    // попросить, ни проверить.
    // Тип ДЛИННОЕ целое, а не обычное, и это не вкусовая правка.
    //
    // Что было неправильно. Сам счётчик публикаций объявлен длинным целым (запись версии ниже),
    // а это свойство отдавало его как обычное целое. В C# длинное в обычное неявно не
    // преобразуется, поэтому область перевода просто не собиралась; а если бы преобразование
    // дописали приведением, оно молча усекало бы номер после предела обычного целого — то есть
    // две разные публикации получили бы один номер, и условная публикация по номеру версии
    // перестала бы различать их. Молчаливое усечение здесь хуже отказа сборки.
    //
    // Почему расширение читателя, а не проверяемое приведение. Номер публикации — монотонно
    // растущая величина, и длинное целое для неё канон, уже выбранный в записи версии. Свойство
    // не может честно отказать: отказ из свойства — это исключение, а правило проекта запрещает
    // исключения для обычной логики. Поэтому читатель расширен до типа самой величины, новых
    // путей отказа не появилось, и потери не существует по типу.
    public long CurrentStateVersion => CurrentVersion().VersionNumber;

    // HasStructuredTrainerFor отвечает на один вопрос: есть ли у ТЕКУЩЕЙ опубликованной
    // версии получатель обучения с таким опознавателем единицы.
    //
    // Зачем это нужно по-человечески. Приёмник партий обучения строку с неизвестным
    // получателем МОЛЧА пропускает: партия объявляется успешной, строка считается
    // обученной, а продвинутых единиц в ответе ноль, то есть урок не дошёл ни до кого.
    // Вызывающая сторона, которая хочет доставить урок названной единице, обязана иметь
    // возможность спросить про получателя ДО записи, а не узнавать о пустой доставке по
    // косвенным признакам после неё.
    //
    // Этот читатель НИЧЕГО не меняет: ни состояния, ни версии, ни памяти единиц. Поведение
    // самого приёмника партий он тоже не меняет — прежние вызывающие работают как прежде.
    // Версия читается один раз, поэтому ответ относится ровно к одной версии состояния.
    public bool HasStructuredTrainerFor(string unitId)
    {
        // Пустое имя единицы получателем быть не может, и спрашивать о нём нечего.
        if (string.IsNullOrWhiteSpace(unitId))
            return false;

        // Одно чтение версии: ответ относится ровно к той версии, которую видит вызывающий.
        return CurrentVersion().UnitTrainers.ContainsKey(unitId);
    }

    // StateVersionChangedErrorCode — код отказа публикации, когда версия состояния ушла вперёд
    // с того момента, как писатель прочитал свою основу.
    public const string StateVersionChangedErrorCode = "STATE_VERSION_CHANGED_DURING_TRAINING";

    // MeasuredVersionChangedErrorCode — код отказа записи измерения, когда измеряли одну версию,
    // а записать число пытаются к другой.
    public const string MeasuredVersionChangedErrorCode = "MEASURED_STATE_VERSION_CHANGED";

    // CandidatePersistenceFailedErrorCode — код отказа, когда проигравшего обученного кандидата
    // не удалось сохранить. Он отличается от отказа публикации: публикация отказана законно, а
    // вот оплаченная обученная работа при этом НЕ сохранена, и молчать об этом нельзя.
    public const string CandidatePersistenceFailedErrorCode = "CANDIDATE_PERSISTENCE_FAILED";

    // _lostCandidateStore — хранилище проигравших обученных кандидатов.
    //
    // Что было неправильно. Отказ публикации по несовпадению основы говорил «обученный кандидат
    // сохранён», а закрытая копия обученного состояния просто выходила из области видимости:
    // оплаченное обучение терялось, и проверить обещание было нечем — ни указателя, ни пути.
    //
    // Что стало. Когда хранилище назначено, кандидат собирается из СВОЕГО держателя версии и
    // СВОЕЙ закрытой копии исполнения и сохраняется записью, которую можно перечитать. Обычный
    // путь сохранения здесь непригоден: он читает текущую опубликованную версию, а после
    // проигрыша текущей является победитель. Пока хранилище не назначено, отказ прямо говорит,
    // что кандидат НЕ сохранён, вместо прежнего ложного обещания.
    private ITranslationLostCandidateStore? _lostCandidateStore;

    // BeforeConditionalPublishBarrier — минимальный барьер наблюдения ПЕРЕД условной публикацией
    // структурной партии.
    //
    // Зачем он нужен. Поздний конфликт двух писателей возникает только тогда, когда чужая
    // публикация проходит МЕЖДУ чтением основы и сравнением-с-заменой. Без управляемой точки
    // синхронизации этот отрезок пришлось бы ловить ожиданием по времени, то есть случайностью,
    // и проверка доказывала бы погоду, а не правило.
    //
    // Чем он не является. Это не подставной писатель и не замена рабочего метода: публикация,
    // сборка кандидата и запись кандидата идут настоящим продуктовым путём. Барьер только даёт
    // наблюдателю точку, в которой он может дать сработать другому писателю. В продукте он
    // остаётся пустым, поэтому поведение продукта им не меняется.
    internal Action? BeforeConditionalPublishBarrier;

    // BeforeMeasuredAccuracyPublishBarrier — такой же минимальный барьер наблюдения ПЕРЕД
    // условной записью измеренного числа.
    //
    // Зачем он нужен. Требование раздела ремонта: измерение держит ОДНУ версию, и если за время
    // измерения вышла новая, завершившееся измерение не имеет права менять число и личность
    // новой версии. Этот отрезок — между снятием числа и его записью — без управляемой точки
    // синхронизации тоже пришлось бы ловить ожиданием по времени.
    //
    // Чем он не является. Само измерение, условная запись и отказ идут настоящим продуктовым
    // путём; барьер только даёт наблюдателю точку, в которой может пройти чужая публикация. В
    // продукте он пуст.
    internal Action? BeforeMeasuredAccuracyPublishBarrier;

    // Наблюдение после настоящего неудачного CAS: последующая публикация не меняет observed.
    internal Action<long>? AfterConditionalPublishConflictBarrier;

    // UseLostCandidateStore назначает хранилище проигравших кандидатов.
    //
    // Отдельный метод, а не довод договора обучения: подпись члена интерфейса расширять нельзя,
    // лишний довод — даже необязательный — ломает реализацию члена. Поэтому назначение хранилища
    // живёт своей точкой входа, и все прежние вызывающие продолжают работать без правок.
    public OperationResult<bool> UseLostCandidateStore(ITranslationLostCandidateStore store)
    {
        if (store is null)
            return OperationResult<bool>.Fail(
                "Хранилище проигравших кандидатов обязательно.", "CANDIDATE_STORE_REQUIRED"); // Пустое назначение отвергается.

        _lostCandidateStore = store; // Назначенное хранилище применяется ко всем дальнейшим отказам публикации.
        return OperationResult<bool>.Ok(true); // Назначение состоялось.
    }

    // TryPublishVersion публикует новую версию ТОЛЬКО если текущая всё ещё та, от которой
    // писатель готовил изменение. Возвращает true при успехе и false, если версия ушла вперёд.
    //
    // Что было неправильно. Прежде публикация была слепой записью ссылки. Согласованность КАЖДОЙ
    // отдельной публикации при этом сохранялась — читатель никогда не видел полуготового графа, —
    // но потерю обновления это не предотвращало: два писателя, прочитавшие версию семь, каждый
    // готовил свою версию восемь и каждый её публиковал, и работа первого исчезала без следа и
    // без отказа. Атомарная замена ссылки сама по себе от потери обновления не спасает.
    //
    // Что стало. Замена ссылки выполняется сравнением с ожидаемой основой: если за время работы
    // писателя версию уже заменили, запись НЕ происходит, живое состояние остаётся прежним, а
    // вызывающий получает отказ и сам решает, что делать. Повторного обучения здесь нет и быть
    // не должно: это было бы самодействие, а не решение.
    private bool TryPublishVersion(
        TranslationPublishedVersion expectedBase,
        TranslationPublishedVersion next,
        out TranslationPublishedVersion observed)
    {
        // Номер публикации растёт здесь, в единственном месте публикации: любая
        // новая версия строится как «текущая с изменениями», поэтому прибавление
        // единицы к номеру основы даёт следующий номер без отдельного счётчика.
        var published = next with { VersionNumber = next.VersionNumber + 1 };

        // Сравнение и замена одной неделимой операцией: возвращается та ссылка, которая лежала
        // в поле ДО попытки. Сравнение идёт по самой ссылке, а не по содержимому: именно тот
        // экземпляр версии писатель и читал, когда начинал работу.
        observed = System.Threading.Interlocked.CompareExchange(
            ref _publishedVersion,
            published,
            expectedBase);

        // Совпадение ссылок означает, что за время работы писателя никто не публиковал, и его
        // версия встала. Несовпадение означает потерю обновления, которой мы не допустили.
        var succeeded = ReferenceEquals(observed, expectedBase);
        if (!succeeded)
            AfterConditionalPublishConflictBarrier?.Invoke(observed.VersionNumber);
        return succeeded;
    }

    // DescribeLostUpdate составляет человеческую причину отказа публикации с ОБЕИМИ версиями:
    // той, от которой готовилось изменение, и той, которая лежит сейчас.
    //
    // Что было неправильно. Эта же фраза говорила «обученный кандидат сохранён» на ЛЮБОМ пути
    // отказа, хотя сохранением никто не занимался: закрытая копия обученного состояния просто
    // выходила из области видимости. Сообщение обещало контрольную точку, которой не было.
    //
    // Что стало. Здесь остаётся только то, что действительно наблюдается в этой точке: обе
    // версии, отмена публикации, неизменность живого состояния и отсутствие самодействия.
    // Судьба обученного кандидата называется ОТДЕЛЬНОЙ фразой и только по факту записи.
    private static string DescribeLostUpdate(
        TranslationPublishedVersion expectedBase, TranslationPublishedVersion observed, string what)
        // Обе величины названы прямо, чтобы было видно, насколько состояние ушло вперёд.
        => $"{what} готовилось от версии состояния номер {expectedBase.VersionNumber}, а CAS наблюдал " +
           $"опубликованную версию номер {observed.VersionNumber}. Публикация отменена: " +
           "живое состояние не менялось, повторное обучение автоматически не запускается.";

    // PersistLostCandidate сохраняет проигравшего обученного кандидата и возвращает фразу о его
    // судьбе. Слово «сохранён» произносится ТОЛЬКО когда запись подтверждена перечитыванием.
    //
    // Кандидат собирается из СВОЕГО держателя версии и СВОЕЙ закрытой копии исполнения, а не
    // через обычную выгрузку: обычная выгрузка читает текущую опубликованную версию, а после
    // проигрыша сравнения-с-заменой текущей является победитель, и тогда под именем кандидата
    // записали бы чужое состояние.
    // Довод о победителе назван явно и может быть пустым: на пути отмены конкурента нет, и
    // запись обязана говорить «победителя не было», а не подставлять номер основы.
    private (string Sentence, string? Locator, bool PersistenceFailed, string? FailureReason) PersistLostCandidate(
        TranslationPublishedVersion expectedBase,
        TranslationPublishedVersion candidateVersion,
        TranslationRuntime? candidateRuntime,
        string candidateBatchId,
        LearningBatchResult candidateResult,
        string refusalErrorCode,
        string refusalReason,
        // Тип ДЛИННОЕ целое с возможным пустым значением: номер берётся из счётчика публикаций,
        // который объявлен длинным. Обычное целое здесь не собиралось бы, а приведение молча
        // усекало бы номер.
        long? winningStateVersionNumber)
    {
        var store = _lostCandidateStore; // Одно чтение назначенного хранилища на весь отказ.

        if (store is null)
            // Хранилище не назначено: прямо говорим, что кандидат НЕ сохранён, вместо обещания.
            return ("Обученный кандидат НЕ сохранён: хранилище записей проигравших кандидатов не " +
                    "назначено этой машине. Сохранить его нечем, поэтому оплаченная работа этой " +
                    "партии остаётся только в памяти вызывающего.", null, false, null);

        if (candidateRuntime is null)
            // Нет закрытой копии исполнения: сохранять фактическое состояние не из чего.
            return ("Обученный кандидат НЕ сохранён: у этой партии нет закрытой копии исполнения, " +
                    "поэтому фактического обученного состояния для записи не существует.", null, false, null);

        // Фактическое состояние кандидата: та же чистая сборка, что и у обычной выгрузки, но по
        // держателю версии КАНДИДАТА и его копии исполнения.
        var candidateCheckpoint = BuildCheckpoint(candidateVersion, candidateRuntime);

        // Опознание состояния без времени выгрузки: сверка восстановленного состояния не должна
        // падать из-за новых стенных часов.
        var stateIdentity = TranslationAtomicJsonWriter.CanonicalStateIdentity(candidateCheckpoint);

        // Происхождение: опознания учебных строк, добавленных именно этим кандидатом.
        var lineage = candidateVersion.StructuredTrainingIdentities
            .Where(identity => string.Equals(identity.BatchId, candidateBatchId, StringComparison.Ordinal))
            .Select(identity => identity.ExampleId)
            .OrderBy(exampleId => exampleId, StringComparer.Ordinal)
            .ToList();

        var artifact = new TranslationLostCandidateArtifact(
            SchemaVersion: 1,                                        // Версия формата записи.
            MachineId: MachineId,                                    // Машина, которой принадлежит кандидат.
            DomainTag: DomainTag,                                    // Предметная область машины.
            BaseStateVersionNumber: expectedBase.VersionNumber,      // Основа, от которой кандидат готовился.
            BaseSemanticVersion: expectedBase.SemanticVersion,       // Смысловой номер той же основы.
            WinningStateVersionNumber: winningStateVersionNumber,     // Победитель, либо пустое значение, если его не было.
            CandidateBatchId: candidateBatchId,                      // Партия обучения кандидата.
            CandidateSemanticVersion: candidateVersion.SemanticVersion, // Смысловой номер самого кандидата.
            CandidateStateIdentity: stateIdentity,                   // Опознание состояния кандидата.
            CandidateCheckpoint: candidateCheckpoint,                // ФАКТИЧЕСКОЕ состояние, не журнал уроков.
            LineageExampleIdentities: lineage,                       // Происхождение учебных строк кандидата.
            Cost: new TranslationLostCandidateCost(
                candidateResult.RowsTrained,                         // Сколько строк уже применено.
                candidateResult.CpuMs,                               // Потраченное процессорное время.
                candidateResult.ElapsedMs,                           // Прошедшее время работы.
                candidateResult.PeakRamMb),                          // Наибольшая занятая память.
            RefusalErrorCode: refusalErrorCode,                      // Код отказа публикации.
            RefusalReason: refusalReason,                            // Человеческая причина отказа.
            BuiltAtUtc: DateTimeOffset.UtcNow);                      // Время сборки записи.

        var saved = store.Save(artifact); // Запись с подтверждением перечитыванием внутри хранилища.

        if (!saved.IsSuccess)
            // Ошибка хранения названа отдельно, и слово «сохранён» здесь не произносится.
            return ("Обученный кандидат НЕ сохранён: " + saved.ErrorMessage +
                    " Живое состояние осталось целым, победившая версия не менялась.",
                    null, true, saved.ErrorMessage);

        var reference = saved.Value!; // Проверяемый указатель на подтверждённую запись.

        // Только теперь «сохранён» правдиво: байты записаны и перечитаны, указатель проверяем.
        return ("Обученный кандидат сохранён и доступен для чтения: " + reference.Locator +
                ". Свёртка записанных байтов " + reference.WrittenBytesHash +
                ", опознание состояния " + reference.CandidateStateIdentity +
                ". Повторное чтение этой записи обучения не запускает; применение её к другой " +
                "основе — отдельная операция с собственным допуском версии.",
                reference.Locator, false, null);
    }

    public Task<OperationResult<LearningBatchResult>> TrainBatchAsync(
        LearningBatch<TranslationDomain> batch,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail("Training cancelled.", "CANCELLED"));

        if (batch is null)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail("Training batch is required.", "BATCH_REQUIRED"));

        if (batch.TrainingItems.Count == 0)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail(
                "Training batch must contain at least one item.", "BATCH_EMPTY"));

        var startMs = Environment.TickCount64;
        // Текущая версия читается один раз: она же станет основой новой версии.
        var current = CurrentVersion();
        var accuracyBefore = current.AccuracyAfterLastBatch;

        // Legacy scaffold path: flat LearningTrainingItem rows are accepted for old tests,
        // but this path does not claim unit-level translation learning.
        var accuracyAfter = Math.Min(accuracyBefore + 0.01, 1.0);
        var result = new LearningBatchResult(
            BatchId: batch.BatchId,
            RowsTrained: batch.TrainingItems.Count,
            AccuracyBefore: accuracyBefore,
            AccuracyAfter: accuracyAfter,
            RewardMargin: 0.0,
            CpuMs: Environment.TickCount64 - startMs,
            PeakRamMb: GC.GetTotalMemory(forceFullCollection: false) / (1024 * 1024),
            ElapsedMs: Environment.TickCount64 - startMs,
            PromotedUnitIds: []);

        // Этот путь не меняет памяти единиц, поэтому поток и обучатели остаются
        // прежними объектами; новыми становятся только номер версии, точность и
        // история. Публикация всё равно идёт одной заменой ссылки.
        // Публикация условна по прочитанной основе и на этом пути тоже: номер версии, точность
        // и история — тоже состояние, и терять чужую запись в них так же нельзя.
        if (!TryPublishVersion(current, current with
        {
            SemanticVersion = NextPatchVersion(current.SemanticVersion),
            AccuracyAfterLastBatch = accuracyAfter,
            TrainingHistory = [.. current.TrainingHistory, result]
        }, out var observed))
        {
            // Живое состояние не менялось; повторное обучение автоматически не запускается.
            //
            // Записи проигравшего кандидата здесь НЕТ, и это названо прямо. Этот путь не меняет
            // памяти единиц: он несёт только номер версии, точность и историю, а исполнение
            // остаётся тем же живым объектом. Собрать «контрольную точку кандидата» из живого
            // исполнения значило бы записать под именем кандидата состояние ПОБЕДИТЕЛЯ — ту самую
            // ложь о данных, из-за которой обычный путь сохранения здесь непригоден.
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail(
                DescribeLostUpdate(current, observed, $"Партия обучения '{batch.BatchId}'") +
                " Обученного кандидата для записи здесь нет: этот путь не менял памяти единиц, " +
                "поэтому терять обученное состояние нечем, а записывать живое состояние под " +
                "именем кандидата запрещено.",
                StateVersionChangedErrorCode));
        }

        return Task.FromResult(OperationResult<LearningBatchResult>.Ok(result));
    }

    public Task<OperationResult<LearningBatchResult>> TrainStructuredBatchAsync(
        TranslationLearningBatch batch,
        CancellationToken ct = default,
        // expectedStateVersion — номер версии состояния, от которой вызывающий готовил эту
        // партию. Довод необязательный и стоит последним, поэтому все прежние вызывающие
        // работают без правок. Когда он назван, обучение не начнётся, если состояние уже ушло
        // вперёд: это оптимистичная блокировка для вызывающего, который сам прочитал версию,
        // собрал по ней материал и не хочет учить поверх чужой работы.
        //
        // Тип ДЛИННОЕ целое, потому что вызывающий читает номер открытым свойством версии, а оно
        // отдаёт длинное: обычное целое заставило бы его усекать номер на входе.
        long? expectedStateVersion = null)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail("Structured training cancelled.", "CANCELLED"));

        if (batch is null)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail("Structured training batch is required.", "BATCH_REQUIRED"));

        if (batch.Examples.Count == 0)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail(
                "Structured training batch must contain at least one example.", "BATCH_EMPTY"));

        // Проверка читаемости текстов JSON (JavaScript Object Notation, текстовый
        // формат записи данных) проходит ВСЕ потенциально обучаемые строки партии
        // и завершается ПОЛНОСТЬЮ до первого обновления памяти единиц. Без этого
        // прохода нечитаемая вторая строка оставляла урок первой строки уже
        // записанным в память переводчика, хотя партия считалась неприменённой.
        // Текущая опубликованная версия читается РОВНО ОДИН раз за партию. Всё
        // дальнейшее решение этой партии принимается относительно неё, поэтому
        // чужая публикация посреди работы не может смешать два состояния.
        var current = CurrentVersion();

        // Названное вызывающим условие по версии проверяется ДО обучения: если состояние уже
        // ушло вперёд, учить нечего — материал готовился от другой версии. Отказ называет обе
        // величины, живое состояние не меняется, повторное обучение не запускается само.
        if (expectedStateVersion is not null && expectedStateVersion.Value != current.VersionNumber)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail(
                $"Партия обучения '{batch.BatchId}' готовилась от версии состояния номер " +
                $"{expectedStateVersion.Value}, а сейчас опубликована версия номер " +
                $"{current.VersionNumber}. Обучение не начиналось: живое состояние не менялось, " +
                "повторное обучение автоматически не запускается.",
                StateVersionChangedErrorCode));

        var readabilityCheck = ValidateStructuredBatchRowsReadable(current, batch);
        // Нечитаемая строка даёт управляемый отказ партии: ни одной записи
        // состояния, версии, истории обучения или опознаний при этом не делается.
        if (!readabilityCheck.IsSuccess)
            return Task.FromResult(readabilityCheck.MapError<LearningBatchResult>());

        // Проверка столкновения рабочих ключей идёт ДО сборки копии и до единой записи в память.
        // Две строки партии могут отличаться только номером учебной пары: прежде это были два
        // разных ключа, и второй урок просто ложился рядом. Теперь номер пары в ключе не живёт,
        // оба ключа становятся одним, и если содержимое у строк несовместимое, последний урок
        // молча затёр бы первый. Такая партия отвергается целиком.
        var keyCollisionCheck = ValidateStructuredBatchKeysDoNotCollide(current, batch);
        // Отказ по столкновению — обычный результат с кодом, а не исключение: живая версия
        // остаётся ровно той, какой была, потому что ни одной записи ещё не делалось.
        if (!keyCollisionCheck.IsSuccess)
            return Task.FromResult(keyCollisionCheck.MapError<LearningBatchResult>());

        // Сверка ОБЪЯВЛЕННОЙ формы ключа с формой, выведенной из самих данных. Строка, которая
        // называет себя канонической, но несёт отрезок происхождения внутри ключа, — это ложь о
        // данных, и принимать её нельзя. Строка, которая форму не называет, разрешена: её форма
        // выводится из содержимого, и прежний договор писателей не ломается.
        var keySchemaCheck = ValidateStructuredBatchKeySchemaVersions(current, batch);
        // Отказ идёт до сборки закрытой копии, поэтому состояние переводчика не меняется.
        if (!keySchemaCheck.IsSuccess)
            return Task.FromResult(keySchemaCheck.MapError<LearningBatchResult>());

        var startMs = Environment.TickCount64;
        var accuracyBefore = current.AccuracyAfterLastBatch;

        // Закрытая копия всех изменяемых частей графа. Обучение этой партии
        // пойдёт ТОЛЬКО по ней, поэтому живая опубликованная версия не меняется
        // ни одной строкой партии до самой публикации. Именно это убирает
        // прежний разрыв: раньше ранние строки уже правили живую память, а
        // поздняя несовместимая строка оставляла половину партии принятой.
        // Копия исполнения новой версии. Остаётся null на пути внедрения потока,
        // где типизированных единиц нет и менять в памяти единиц нечего.
        TranslationRuntime? stagedRuntime = null;
        if (current.Runtime is not null)
        {
            var stagingResult = StageRuntimeCopy(current, current.Runtime);
            // Невозможность собрать копию — это отказ партии, а не тихий проход
            // по живому состоянию: причина отказа называется отдельным кодом.
            if (!stagingResult.IsSuccess)
                return Task.FromResult(OperationResult<LearningBatchResult>.Fail(
                    $"Structured training batch '{batch.BatchId}' could not prepare an isolated " +
                    $"state copy: {stagingResult.ErrorMessage}",
                    "BATCH_STAGING_FAILED"));

            // Копия готова: дальше обучение идёт только по ней.
            stagedRuntime = stagingResult.Value!;
        }
        // Обучатели копии: по ним и идёт обучение партии. На пути внедрения
        // потока берётся прежний пустой набор, и поведение остаётся тем же.
        var stagedTrainers = stagedRuntime?.UnitTrainers ?? current.UnitTrainers;
        var trainedUnits = new HashSet<string>(StringComparer.Ordinal);
        // Рабочий список опознаний: копия прежнего, чтобы живой список версии
        // не менялся до публикации даже порядком своих строк.
        var stagedIdentities = current.StructuredTrainingIdentities.ToList();
        // Рабочий НАБОР пар «опознание примера + опознание единицы» того же рабочего списка.
        // Он отвечает на единственный вопрос «такая пара уже записана?» и заменяет полный
        // перебор журнала, который прежде делался заново на КАЖДЫЙ урок партии.
        //
        // Почему набор ЛОКАЛЬНЫЙ, а не поле машины. Он живёт ровно столько же, сколько копия
        // списка выше: при отказе строки партии и при проигрыше сравнения-и-публикации оба
        // выбрасываются вместе с кадром вызова. Поле объекта в этих случаях осталось бы
        // изменённым при НЕопубликованном журнале, то есть прямо сломало бы неделимость
        // публикации, которую держит единственная замена ссылки версии.
        //
        // Почему ключ — кортеж ровно из двух строк. Сравнение кортежа по умолчанию идёт
        // сравнением по умолчанию каждой части, а для строки это сравнение ordinal и
        // регистрозависимое — ровно то же сравнение, которое делал прежний полный перебор.
        // Опознание источника и опознание партии в ключ не входят: они остаются метаданными
        // ПЕРВОЙ записанной пары.
        var stagedIdentityKeys = new HashSet<(string ExampleId, string UnitId)>(
            // Набор заполняется из УЖЕ ГОТОВОЙ копии списка, а не вторым перебором поля версии.
            // Иначе правка, вся суть которой — убрать лишний просмотр журнала, сама оставила бы
            // лишний просмотр: журнал читался бы дважды на партию, в том числе когда учить нечего.
            // Содержимое тождественно по построению, потому что копия сделана строкой выше из
            // того же поля, а источник истины остаётся один: если это поле однажды станет
            // ленивым или разделяемым изменяемым списком, список и набор разойтись уже не смогут.
            stagedIdentities.Select(
                identity => (identity.ExampleId, identity.UnitId)));

        foreach (var example in batch.Examples.Where(example => !example.HeldOut))
        {
            if (!stagedTrainers.TryGetValue(example.UnitId, out var trainer))
                continue;

            try
            {
                // Получатель обучения сам разбирает свой типовой договор: имена
                // перечислений, направление и форму полей знает только он.
                // Разбор повторно в машине не дублируется, поэтому один владелец
                // договора остаётся один, а сбой разбора ловится здесь.
                trainer.TrainOnExample(example);
            }
            catch (Exception trainingFailure)
            {
                // Сбой строки внутри обучения — управляемый отказ всей партии, а
                // не исключение наружу. Живая версия не публиковалась, поэтому
                // восстанавливать нечего: копия просто выбрасывается.
                return Task.FromResult(OperationResult<LearningBatchResult>.Fail(
                    DescribeRowTrainingFailure(batch, example, trainingFailure),
                    "BATCH_ROW_TRAINING_FAILED"));
            }

            trainedUnits.Add(example.UnitId);
            // Добавление в набор САМО отвечает, была ли такая пара уже записана, и делает это
            // за одно обращение вместо просмотра всего журнала. Истина означает «пары не было»,
            // и тогда запись дописывается в журнал точно там же, где и прежде. Ложь означает
            // «пара уже есть», и журнал остаётся с ПЕРВОЙ записью и её исходными метаданными.
            // Обучение выше при этом уже состоялось: нынешний договор знакомый урок повторно
            // обучает, но повторно не дописывает опознание, и этот порядок не меняется.
            if (stagedIdentityKeys.Add((example.ExampleId, example.UnitId)))
                stagedIdentities.Add(new TranslationStructuredTrainingIdentityCheckpoint(
                    example.ExampleId,
                    example.SourceId,
                    example.UnitId,
                    batch.BatchId));
        }

        var heldOut = batch.Examples.Where(example => example.HeldOut).ToArray();
        // Назначенное измерение на отложенных строках выполняется по КОПИИ:
        // измеряется именно та версия, которая будет опубликована, а не живая.
        var accuracyAfter = heldOut.Length > 0
            ? MeasureHeldOutAccuracy(stagedTrainers, heldOut)
            : MeasureUnitCoverage(trainedUnits);

        var result = new LearningBatchResult(
            BatchId: batch.BatchId,
            RowsTrained: batch.Examples.Count(example => !example.HeldOut),
            AccuracyBefore: accuracyBefore,
            AccuracyAfter: accuracyAfter,
            RewardMargin: 0.0,
            CpuMs: Environment.TickCount64 - startMs,
            PeakRamMb: GC.GetTotalMemory(forceFullCollection: false) / (1024 * 1024),
            ElapsedMs: Environment.TickCount64 - startMs,
            PromotedUnitIds: trainedUnits.Order(StringComparer.Ordinal).ToArray());

        // Публикация: ОДНА запись ссылки меняет сразу поток вывода, обучатели,
        // типизированные единицы, номер версии, точность, историю и опознания.
        // Поэтому и уже выданный ранее дирижёр, и новый получают эту же версию,
        // а частично обученного состояния не существует ни на один миг.
        // Публикация УСЛОВНА по прочитанной основе: она произойдёт только если версия всё ещё
        // та, от которой эта партия готовилась. Иначе другой писатель уже опубликовал свою
        // версию, и слепая запись здесь потеряла бы его работу молча.
        // Кандидат собирается ОТДЕЛЬНОЙ величиной до попытки публикации. Прежде он существовал
        // только как безымянный довод публикации и при отказе исчезал вместе с кадром вызова, а
        // сообщение об отказе всё равно называло его сохранённым. Названная величина позволяет
        // сохранить фактическое обученное состояние именно этого кандидата.
        var candidateVersion = current with
        {
            Flow = stagedRuntime?.Flow ?? current.Flow,
            UnitTrainers = stagedRuntime?.UnitTrainers ?? current.UnitTrainers,
            Runtime = stagedRuntime ?? current.Runtime,
            SemanticVersion = NextPatchVersion(current.SemanticVersion),
            AccuracyAfterLastBatch = accuracyAfter,
            HasStructuredTraining = trainedUnits.Count > 0,
            TrainingHistory = [.. current.TrainingHistory, result],
            StructuredTrainingIdentities = stagedIdentities
        };

        // Точка наблюдения перед условной публикацией. В продукте она пуста; наблюдатель может
        // дать здесь сработать другому писателю, чтобы поздний конфликт случился определённо, а
        // не по случайному стечению времени.
        BeforeConditionalPublishBarrier?.Invoke();

        // Поздняя отмена проверяется ПЕРЕД публикацией, и это последнее, что происходит до
        // сравнения-с-заменой.
        //
        // Что было неправильно. Отмена проверялась только на входе в метод. Отмена, пришедшая
        // после обучения закрытой копии, публикацию уже не останавливала: состояние вставало,
        // хотя вызывающий работу отменил. Частичного состояния при этом не возникало — копия
        // применяется целиком одной заменой ссылки, — но публикация отменённой работы сама по
        // себе неправильна.
        //
        // Что стало. При отменённой работе публикации НЕТ вовсе, живое состояние остаётся
        // прежним, а судьба уже оплаченного обученного кандидата называется той же отдельной
        // фразой и по тому же факту записи: отмена — не причина терять оплаченную работу молча.
        if (ct.IsCancellationRequested)
        {
            var cancelReason =
                $"Партия обучения '{batch.BatchId}' отменена ПОСЛЕ обучения закрытой копии и ДО " +
                "публикации. Публикации не было: живое состояние осталось прежним, частичного " +
                "состояния не возникает, повторное обучение автоматически не запускается.";

            var cancelledCandidate = PersistLostCandidate(
                current,            // Основа, от которой кандидат готовился.
                candidateVersion,   // Сам кандидат со своей обученной копией.
                stagedRuntime,      // Закрытая копия исполнения кандидата.
                batch.BatchId,      // Партия обучения кандидата.
                result,             // Уже оплаченный расход кандидата.
                "CANCELLED",        // Код отказа: отмена, а не конфликт версий.
                cancelReason,       // Человеческая причина отказа.
                // Победителя НЕТ: публикация не состоялась по отмене, а не проиграла чужой
                // работе. Пустое значение говорит это прямо; номер основы здесь был бы ложью.
                winningStateVersionNumber: null);

            // Отказ хранения и здесь называется своим кодом: отмена публикации законна, а
            // потеря уже оплаченной обученной работы — отдельная беда.
            if (cancelledCandidate.PersistenceFailed)
                return Task.FromResult(OperationResult<LearningBatchResult>.Fail(
                    cancelReason + " " + cancelledCandidate.Sentence,
                    CandidatePersistenceFailedErrorCode));

            return Task.FromResult(OperationResult<LearningBatchResult>.Fail(
                cancelReason + " " + cancelledCandidate.Sentence,
                "CANCELLED"));
        }

        if (!TryPublishVersion(current, candidateVersion, out var observed))
        {
            // Отказ публикации: живое состояние не менялось ни на одну запись, и повторное
            // обучение автоматически НЕ запускается — это отдельное решение вызывающего.
            // Оплаченное обучение кандидата больше не выбрасывается молча: оно записывается из
            // СВОЕЙ закрытой копии исполнения, и слово «сохранён» звучит только по факту записи.
            var refusalReason = DescribeLostUpdate(current, observed, $"Партия обучения '{batch.BatchId}'");

            var candidate = PersistLostCandidate(
                current,                                  // Основа, от которой кандидат готовился.
                candidateVersion,                         // Сам кандидат со своей обученной копией.
                stagedRuntime,                            // Закрытая копия исполнения кандидата.
                batch.BatchId,                            // Партия обучения кандидата.
                result,                                   // Уже оплаченный расход кандидата.
                StateVersionChangedErrorCode,             // Код отказа публикации.
                refusalReason,                            // Человеческая причина отказа.
                // Здесь победитель ЕСТЬ: это та версия, которая уже лежит вместо основы.
                winningStateVersionNumber: observed.VersionNumber);

            // Отказ хранения — отдельный названный отказ: публикация отменена законно, а вот
            // оплаченная обученная работа при этом НЕ сохранена, и код отказа говорит именно это.
            if (candidate.PersistenceFailed)
                return Task.FromResult(OperationResult<LearningBatchResult>.Fail(
                    refusalReason + " " + candidate.Sentence,
                    CandidatePersistenceFailedErrorCode));

            return Task.FromResult(OperationResult<LearningBatchResult>.Fail(
                refusalReason + " " + candidate.Sentence,
                StateVersionChangedErrorCode));
        }

        return Task.FromResult(OperationResult<LearningBatchResult>.Ok(result));
    }

    // DescribeRowTrainingFailure собирает человеческую причину отказа партии по
    // сбою ОДНОЙ строки. В сообщении названы: опознание виновной строки, её
    // получатель обучения, партия, род исходного сбоя и его текст, а также
    // ограниченная выдержка самих текстов строки — по ней видно, какое именно
    // значение получатель не смог прочитать как свой тип. Опознания законных
    // строк партии в сообщение не попадают: отказ указывает на виновную строку.
    private static string DescribeRowTrainingFailure(
        TranslationLearningBatch batch,
        TranslationLearningExample example,
        Exception trainingFailure)
        // Слово о нечитаемом документе здесь намеренно не используется: это
        // другая болезнь со своим прежним кодом отказа.
        => $"Structured training row '{example.ExampleId}' of unit '{example.UnitId}' in batch " +
           $"'{batch.BatchId}' failed while that unit applied its own typed contract: " +
           $"{trainingFailure.GetType().Name}: {trainingFailure.Message} " +
           $"Row input excerpt: {ExcerptRowText(example.InputJson)} " +
           $"Row chosen output excerpt: {ExcerptRowText(example.ChosenOutputJson)}";

    // ExcerptRowText ограничивает длину выдержки текста строки, чтобы сообщение
    // отказа оставалось читаемым человеком и не превращалось в дамп партии.
    private static string ExcerptRowText(string rowText)
    {
        // Отсутствующий текст называется прямо, а не пустым местом в сообщении.
        if (rowText is null)
            return "(none)";

        // Предел выдержки: его хватает, чтобы увидеть негодное значение поля.
        const int excerptLimit = 512;
        // Короткий текст выдаётся целиком, длинный — обрезается с пометкой.
        return rowText.Length <= excerptLimit
            ? rowText
            : rowText[..excerptLimit] + "…(truncated)";
    }

    // StageRuntimeCopy готовит закрытую копию всех изменяемых частей состояния.
    // Копия строится ЧЕРЕЗ собственную контрольную точку продукта: выгрузка
    // текущего состояния, создание свежего связанного исполнения и загрузка в
    // него. Поэтому копия заведомо согласована внутри себя — поток, адаптеры,
    // обучатели и типизированные единицы указывают на одни и те же объекты.
    private static OperationResult<TranslationRuntime> StageRuntimeCopy(
        TranslationPublishedVersion current,
        TranslationRuntime currentRuntime)
    {
        // Снимок текущего принятого состояния в виде контрольной точки.
        var snapshot = BuildCheckpoint(current, currentRuntime);
        // Свежее связанное исполнение: его единицы ещё пусты.
        var staged = CreateRuntime();
        // Полная проверка снимка относительно копии идёт до любой записи в неё.
        var structureValidation = ValidateCheckpointStructure(staged, snapshot);
        // Непригодный снимок означает, что копию собрать нельзя.
        if (!structureValidation.IsSuccess)
            return OperationResult<TranslationRuntime>.Fail(
                structureValidation.ErrorMessage, structureValidation.ErrorCode);

        // Загрузка снимка в копию: после неё копия повторяет принятое состояние.
        ApplyCheckpointToRuntime(staged, snapshot);
        // Копия готова принимать обучение партии.
        return OperationResult<TranslationRuntime>.Ok(staged);
    }

    // ValidateStructuredBatchRowsReadable проверяет синтаксис входного текста и
    // текста правильного ответа у тех строк партии, которые действительно пойдут
    // в обучение. Проход только читает партию и ничего не меняет, поэтому его
    // отказ оставляет переводчик ровно в том состоянии, в котором он был.
    private static OperationResult<bool> ValidateStructuredBatchRowsReadable(
        TranslationPublishedVersion current,
        TranslationLearningBatch batch)
    {
        // Строки перебираются в том же порядке, в котором их обучал бы цикл ниже.
        foreach (var example in batch.Examples)
        {
            // Отложенная строка в обучение не идёт: она служит только измерению
            // точности, и её синтаксис не становится ошибкой этой партии.
            if (example.HeldOut)
                continue;

            // Строка с неизвестным получателем обучения уже пропускалась циклом
            // обучения; существующий пропуск сохраняется и новой ошибкой не станет.
            // Набор получателей берётся из той же версии, по которой партия и
            // будет обучаться, поэтому проход и обучение видят один список.
            if (!current.UnitTrainers.ContainsKey(example.UnitId))
                continue;

            // Входной текст строки: именно его обучатель единицы читает первым.
            var inputFailure = DescribeUnreadableJson(example.InputJson);
            // Нечитаемый вход означает, что урок применить нельзя ни частично, ни целиком.
            if (inputFailure is not null)
                return OperationResult<bool>.Fail(
                    $"Structured training row '{example.ExampleId}' has unreadable InputJson: {inputFailure}",
                    "BATCH_ROW_UNREADABLE");

            // Текст правильного ответа строки: из него берутся перевод и отображения.
            var chosenFailure = DescribeUnreadableJson(example.ChosenOutputJson);
            // Сообщение отказа несёт настоящий идентификатор строки, а не номер по порядку.
            if (chosenFailure is not null)
                return OperationResult<bool>.Fail(
                    $"Structured training row '{example.ExampleId}' has unreadable ChosenOutputJson: {chosenFailure}",
                    "BATCH_ROW_UNREADABLE");
        }

        // Все потенциально обучаемые строки читаются: обучение может начинаться.
        return OperationResult<bool>.Ok(true);
    }

    // KeySchemaVersionMismatchErrorCode — код отказа, когда строка НАЗВАЛА форму рабочего ключа,
    // а сам ключ говорит об обратном: например объявлена каноническая форма, а внутри ключа
    // стоит отрезок происхождения.
    public const string KeySchemaVersionMismatchErrorCode = "BATCH_ROW_KEY_SCHEMA_VERSION_MISMATCH";

    // ValidateStructuredBatchKeySchemaVersions сверяет объявленную форму ключа с выведенной из
    // данных. Проход только читает партию и ничего не меняет.
    //
    // Почему это нужно. Объявить форму можно неверно, и прежде поле формы имело значение по
    // умолчанию «каноническая»: строка со старым ключом, несущим отрезок происхождения, молча
    // помечалась канонической, и загрузчик считал её уже перенесённой. Ни вспомогательный
    // вычислитель, ни новое поле записи сами по себе переносом не являются.
    private static OperationResult<bool> ValidateStructuredBatchKeySchemaVersions(
        TranslationPublishedVersion current,
        TranslationLearningBatch batch)
    {
        // Строки перебираются в том же порядке, в котором их обучал бы цикл обучения.
        foreach (var example in batch.Examples)
        {
            // Отложенная строка в обучение не идёт и форму ключа памяти не задаёт.
            if (example.HeldOut)
                continue;

            // Строка с неизвестным получателем обучения пропускается обучением и здесь тоже.
            if (!current.UnitTrainers.ContainsKey(example.UnitId))
                continue;

            // Форма не названа — разрешённый случай: она выводится из содержимого ключа, и
            // прежние писатели, которые про это поле не знают, работают как раньше.
            if (example.KeySchemaVersion is null)
                continue;

            // Форма, выведенная из самого ключа: это единственная надёжная величина.
            var derived = TranslationSemanticKey.DeriveSchemaVersion(example.SemanticSignature);
            // Объявленная форма совпала с выведенной — строка честна о своих данных.
            if (example.KeySchemaVersion.Value == derived)
                continue;

            // Расхождение: названы ОБЕ величины, чтобы было видно, что заявлено и что в данных.
            return OperationResult<bool>.Fail(
                $"Строка '{example.ExampleId}' партии '{batch.BatchId}' объявила форму рабочего ключа " +
                $"номер {example.KeySchemaVersion.Value}, а по самому ключу выведена форма номер " +
                $"{derived}: ключ " +
                (derived == TranslationSemanticKey.LegacySchemaVersion
                    ? "несёт отрезок происхождения внутри себя"
                    : "отрезка происхождения не несёт") +
                ". Партия не применена, состояние переводчика не менялось.",
                KeySchemaVersionMismatchErrorCode);
        }

        // Все обучаемые строки честны о форме своего ключа: обучение может начинаться.
        return OperationResult<bool>.Ok(true);
    }

    // ChosenOutputNotObjectErrorCode — код отказа, когда текст правильного ответа учебной строки
    // корректен как документ, но его корень не объект: пустой массив, число, строка или истина.
    // Прочитать образец предложения у такого значения нельзя, и это названный отказ партии, а не
    // исключение наружу.
    public const string ChosenOutputNotObjectErrorCode = "BATCH_ROW_CHOSEN_OUTPUT_NOT_OBJECT";

    // ValidateStructuredBatchKeysDoNotCollide проверяет, что две разные строки партии не заняли
    // один рабочий ключ несовместимым содержимым. Проход только читает партию и ничего не
    // меняет, поэтому его отказ оставляет переводчик ровно в том состоянии, в котором он был.
    //
    // Почему это нужно именно теперь. Пока номер учебной пары сидел внутри ключа, две строки из
    // разных пар всегда имели разные ключи, и столкнуться не могли. После переноса номера в своё
    // поле обе строки приходят по одному каноническому ключу. Для ячейки образца предложения это
    // означает, что вторая строка молча затрёт первую, и обученное знание потеряется без следа.
    private static OperationResult<bool> ValidateStructuredBatchKeysDoNotCollide(
        TranslationPublishedVersion current,
        TranslationLearningBatch batch)
    {
        // Занятые ключи: для каждого запоминается первая занявшая его строка и её образец.
        var occupiedKeys = new Dictionary<string, (string ExampleId, string? PairId, string Pattern)>(
            StringComparer.Ordinal);

        // Строки перебираются в том же порядке, в котором их обучал бы цикл обучения.
        foreach (var example in batch.Examples)
        {
            // Отложенная строка в обучение не идёт и ячейку не занимает.
            if (example.HeldOut)
                continue;

            // Строка с неизвестным получателем обучения пропускается обучением и здесь тоже.
            if (!current.UnitTrainers.ContainsKey(example.UnitId))
                continue;

            // Столкновение проверяется у разборщика смысла: только он кладёт образец предложения
            // в ячейку ПО КЛЮЧУ, то есть только там последний урок способен затереть предыдущий.
            // Память ближайших соседей и требования проверяющего складывают уроки списком и
            // поэтому друг друга не теряют.
            if (!string.Equals(example.UnitId, TranslationUnitIds.SemanticTokenizer, StringComparison.Ordinal))
                continue;

            // Образец предложения этой строки читается из текста правильного ответа.
            var patternRead = DescribeChosenPattern(example.ChosenOutputJson);
            // Негодный ВИД значения — управляемый отказ всей партии, а не исключение наружу.
            // Причина и код приходят от самого чтения, здесь они только дополняются строкой.
            if (!patternRead.IsSuccess)
                return OperationResult<bool>.Fail(
                    $"Строка '{example.ExampleId}' партии '{batch.BatchId}': {patternRead.ErrorMessage}",
                    patternRead.ErrorCode ?? ChosenOutputNotObjectErrorCode);

            // Пустое значение — разрешённый случай: обучатель разборщика такую строку пропустит,
            // и партия остаётся принятой. Ячейку по ключу такая строка не занимает.
            var pattern = patternRead.Value;
            // Строку без читаемого образца обучение разборщика всё равно пропустит.
            if (pattern is null)
                continue;

            // Рабочий ключ строки в той форме, в которой его увидит память.
            var key = TranslationSemanticKey.Canonicalize(example.SemanticSignature);
            // Пустой ключ ячейки не занимает.
            if (key.Length == 0)
                continue;

            // Ключ свободен: строка занимает его и запоминается как первая.
            if (!occupiedKeys.TryGetValue(key, out var first))
            {
                occupiedKeys[key] = (example.ExampleId, example.PairId, pattern);
                continue;
            }

            // Ключ занят той же величиной: это один и тот же урок, терять нечего.
            if (string.Equals(first.Pattern, pattern, StringComparison.Ordinal))
                continue;

            // Ключ занят ДРУГИМ образцом: названы оба номера строк, оба происхождения и оба
            // образца, чтобы человек видел, какие именно уроки столкнулись и чем.
            return OperationResult<bool>.Fail(
                $"Рабочий ключ '{key}' занят двумя строками партии '{batch.BatchId}' с несовместимым " +
                $"содержимым: строка '{first.ExampleId}' (происхождение '{first.PairId ?? "не названо"}') " +
                $"учит образцу предложения '{first.Pattern}', а строка '{example.ExampleId}' " +
                $"(происхождение '{example.PairId ?? "не названо"}') учит образцу '{pattern}'. " +
                $"Партия не применена, состояние переводчика не менялось.",
                "TRANSLATION_SEMANTIC_KEY_COLLISION");
        }

        // Ни один ключ не занят дважды несовместимо: партия может применяться.
        return OperationResult<bool>.Ok(true);
    }

    // DescribeChosenPattern возвращает образец предложения из текста правильного ответа строки
    // как ТЕКСТ и null, когда поля нет. Разбора в типовой договор здесь нет намеренно: этот
    // проход сравнивает две строки партии между собой, а владельцем типового договора остаётся
    // сам обучатель единицы.
    private static OperationResult<string?> DescribeChosenPattern(string chosenOutputJson)
    {
        try
        {
            // Текст разбирается как документ; синтаксис к этому моменту уже проверен.
            using var document = JsonDocument.Parse(chosenOutputJson);

            // ВИД значения корня проверяется ДО любого чтения свойства.
            //
            // Что было неправильно. Прежде этот проход сразу читал свойство у корневого
            // элемента. Для корректного, но НЕ объектного значения — пустого массива, числа,
            // строки — чтение свойства по договору библиотеки разбора выбрасывает исключение
            // неверной операции, а перехвачено здесь было только исключение разбора текста.
            // Поэтому на таком входе падал весь проход, и собственный отказ при столкновении
            // ключей до него просто не доходил.
            //
            // Почему именно проверка вида, а не расширение перехвата. Перехват маскирует:
            // по нему не видно, какой вид значения пришёл и почему он негоден. Проверка вида
            // объясняет и даёт назвать фактический вид в причине отказа.
            var rootKind = document.RootElement.ValueKind;

            // Разрешённое пустое значение — прежний тихий пропуск обучателя. Обучатель
            // разборщика смысла на таком входе ничего не прочитает и строку пропустит, а
            // партия при этом остаётся принятой. Отказом это не является.
            if (rootKind == JsonValueKind.Null)
                return OperationResult<string?>.Ok(null);

            // Любой другой НЕобъектный вид — управляемый отказ. Живое состояние остаётся
            // прежним, потому что этот проход идёт до сборки закрытой копии и до публикации.
            if (rootKind != JsonValueKind.Object)
                return OperationResult<string?>.Fail(
                    $"Текст правильного ответа строки обучения имеет вид '{rootKind}', а не объект, " +
                    "поэтому образец предложения прочитать нельзя. Партия не применена, состояние " +
                    "переводчика не менялось.",
                    ChosenOutputNotObjectErrorCode);

            // Вид корня — объект, значит чтение свойства законно. Имя поля в этом договоре
            // записано с заглавной буквы.
            if (document.RootElement.TryGetProperty("Pattern", out var pattern))
                return OperationResult<string?>.Ok(pattern.ToString());

            // Допускается и запись со строчной буквы: так текст остаётся читаемым для обоих
            // способов записи имён полей, и проход не зависит от настроек записи вызывающего.
            return OperationResult<string?>.Ok(
                document.RootElement.TryGetProperty("pattern", out var lowerCasePattern)
                    ? lowerCasePattern.ToString()
                    : null);
        }
        catch (JsonException)
        {
            // Нечитаемый текст — забота проверки читаемости, а не этого прохода: она идёт
            // раньше и даёт свой собственный код отказа.
            return OperationResult<string?>.Ok(null);
        }
    }

    // DescribeUnreadableJson возвращает человеческую причину, когда текст нельзя
    // прочитать как документ JSON, и null когда синтаксис верен. Проверяется
    // только синтаксис: соответствие текста типу и смыслу структуры данных эта
    // проверка не обещает и такой случай не закрывает.
    private static string? DescribeUnreadableJson(string json)
    {
        // Пустой или пробельный текст документом JSON не является: обучатель
        // единицы на нём ничего не прочитает и прерывал бы работу разборщиком.
        if (string.IsNullOrWhiteSpace(json))
            return "value is empty";

        try
        {
            // Разбор без построения объекта: проверяется только синтаксис документа.
            // Текст "null" синтаксически верен и остаётся прежним тихим пропуском
            // обучателя, а не превращается в новую ошибку партии.
            using var document = JsonDocument.Parse(json);
            // Документ разобран: причины отказа нет.
            return null;
        }
        catch (JsonException ex)
        {
            // Сообщение разборщика сохраняется как причина отказа именно этой строки.
            return ex.Message;
        }
    }

    // EvaluateAsync — член контракта обучаемой машины, и его подпись обязана совпадать с
    // объявленной в контракте ДОСЛОВНО: набор проверки и признак отмены, ровно два довода.
    //
    // Что было сломано и почему это важно. Прежняя правка добавила сюда третий довод — пусть и
    // необязательный. В C# метод с лишним доводом НЕ реализует член интерфейса: совпадать должна
    // сама подпись, а не только имя. Поэтому класс перестал удовлетворять контракту обучаемой
    // машины, и область перевода перестала собираться. Сборка этой области опытами не
    // проверяется, поэтому поломка дошла до ветки незамеченной.
    public Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(
        LearningEvaluationSet<TranslationDomain> evalSet,
        CancellationToken ct = default)
        // Делегирование без названного условия по версии: поведение прежних вызывающих не
        // меняется, потому что этот довод прежде не называл ни один вызывающий.
        => EvaluateAtVersionAsync(evalSet, expectedStateVersion: null, ct);

    // EvaluateAtVersionAsync — то же самое измерение, но вызывающий НАЗЫВАЕТ версию состояния,
    // качество которой собирался измерить.
    //
    // Почему отдельное имя, а не перегрузка по числу доводов: необязательные доводы вместе с
    // перегрузками дают неоднозначность вызова, и тогда выбор метода зависел бы от того, назвал
    // ли вызывающий необязательный довод. Отдельное имя такой двусмысленности не создаёт.
    //
    // Когда версия названа, измерение не начнётся на другой версии: число, снятое не с той
    // версии, которую хотели измерить, нельзя приписывать ни к одной из них.
    public async Task<OperationResult<LearningEvaluationResult>> EvaluateAtVersionAsync(
        LearningEvaluationSet<TranslationDomain> evalSet,
        // Тип ДЛИННОЕ целое по той же причине, что и у обучения партии: номер приходит из
        // открытого свойства версии, которое отдаёт длинное.
        long? expectedStateVersion,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<LearningEvaluationResult>.Fail("Evaluation cancelled.", "CANCELLED");

        if (evalSet is null)
            return OperationResult<LearningEvaluationResult>.Fail("Evaluation set is required.", "EVAL_SET_REQUIRED");

        if (evalSet.EvaluationItems.Count == 0)
            return OperationResult<LearningEvaluationResult>.Fail(
                "Evaluation set must contain at least one item.", "EVAL_SET_EMPTY");

        // Версия фиксируется один раз на всю оценку: все строки набора измеряются
        // ОДНИМ состоянием, даже если во время измерения опубликуется новая версия.
        var measuredVersion = CurrentVersion();

        // Названное вызывающим условие по версии проверяется ДО измерения: если состояние ушло
        // вперёд, измерять нечего — измерить хотели другую версию. Отказ называет обе величины.
        if (expectedStateVersion is not null && expectedStateVersion.Value != measuredVersion.VersionNumber)
            return OperationResult<LearningEvaluationResult>.Fail(
                $"Измерить собирались версию состояния номер {expectedStateVersion.Value}, а сейчас " +
                $"опубликована версия номер {measuredVersion.VersionNumber}. Измерение не " +
                "начиналось: число не той версии не приписывается ни к одной из них, повторное " +
                "измерение — отдельное решение вызывающего.",
                MeasuredVersionChangedErrorCode);

        var heldOutAccuracyMeasured = measuredVersion.HasStructuredTraining;
        var accuracy = 0.0;
        if (heldOutAccuracyMeasured)
        {
            var correct = 0;
            foreach (var item in evalSet.EvaluationItems)
            {
                var translated = await TranslateForEvaluationAsync(measuredVersion, item.Prompt, ct);
                // Отмена не является неверным переводом и не публикует измеренную точность.
                if (ct.IsCancellationRequested || translated.ErrorCode == "CANCELLED")
                    return OperationResult<LearningEvaluationResult>.Fail("Evaluation cancelled.", "CANCELLED");
                if (translated.IsSuccess &&
                    string.Equals(translated.Value, item.ExpectedResponse, StringComparison.Ordinal))
                {
                    correct++;
                }
            }

            accuracy = correct / (double)evalSet.EvaluationItems.Count;
            // Измерение привязано к ТОЙ ВЕРСИИ, которую измеряли.
            //
            // Что было неправильно. Прежде измеренная точность записывалась поверх САМОЙ СВЕЖЕЙ
            // версии. Доводом было «иначе публикация, прошедшая во время измерения, была бы
            // отброшена назад». Довод верен сам по себе, но следствие хуже болезни: число,
            // снятое со СТАРОГО состояния, приписывалось НОВОЙ версии. То есть качество
            // объявлялось про то, что никто не измерял.
            //
            // Что стало. Запись условна по измеренной версии. Если за время измерения вышла
            // новая версия, число к ней НЕ приписывается: оценка возвращает отказ с названием
            // обеих личностей, и вызывающий сам решает, измерять ли заново. Старая версия при
            // этом ничем не затирается, потому что записи вообще не происходит.
            // Точка наблюдения перед условной записью числа. В продукте она пуста; наблюдатель
            // может дать здесь пройти чужой публикации, чтобы случай «измеряли одну версию, а
            // опубликована другая» случился определённо, а не по стечению времени.
            BeforeMeasuredAccuracyPublishBarrier?.Invoke();

            // Последняя проверка отмены непосредственно перед публикацией метрики.
            if (ct.IsCancellationRequested)
                return OperationResult<LearningEvaluationResult>.Fail("Evaluation cancelled.", "CANCELLED");

            if (!TryPublishVersion(
                    measuredVersion,
                    measuredVersion with { AccuracyAfterLastBatch = accuracy }, out var observed))
            {
                // Названы обе личности: версия, которую измеряли, и версия, которая опубликована
                // сейчас. Приписать число не той версии — это ложь об измерении.
                return OperationResult<LearningEvaluationResult>.Fail(
                    $"Измерение качества снято на версии состояния номер " +
                    $"{measuredVersion.VersionNumber}, а CAS наблюдал версию номер " +
                    $"{observed.VersionNumber}. Число к другой версии не приписывается: " +
                    "записи не было, прежнее состояние не менялось, повторное измерение — " +
                    "отдельное решение вызывающего.",
                    MeasuredVersionChangedErrorCode);
            }
        }

        const double gateThreshold = 0.75;
        var meetsAccuracyTarget = heldOutAccuracyMeasured && accuracy >= gateThreshold;
        var gateNote = heldOutAccuracyMeasured
            ? $"REWARD_MARGIN_UNAVAILABLE: preference comparison was not evaluated; " +
              $"structured unit held_out_accuracy_measured=true; " +
              $"accuracy_target_met={meetsAccuracyTarget}; " +
              $"accuracy={accuracy:F4}; threshold={gateThreshold:F4}"
            : "REWARD_MARGIN_UNAVAILABLE: preference comparison was not evaluated; " +
              "legacy scaffold held_out_accuracy_measured=false; " +
              "accuracy_target_met=false; accuracy=0.0000; threshold=unavailable";

        var result = new LearningEvaluationResult(
            EvaluationId: evalSet.EvaluationId,
            RowsEvaluated: evalSet.EvaluationItems.Count,
            Accuracy: accuracy,
            RewardMargin: 0.0,
            PassesGate: meetsAccuracyTarget,
            GateNote: gateNote);

        return OperationResult<LearningEvaluationResult>.Ok(result);
    }

    // ExportAsync returns a metadata-only LearningMachineExport record.
    // It does NOT persist trained unit state. For trained-state persistence use SaveCheckpointAsync.
    // CheckpointRef is a placeholder string — it does not point to a file containing weights.
    public Task<OperationResult<LearningMachineExport>> ExportAsync(CancellationToken ct = default)
    {
        // Return early if cancellation was requested before doing any work.
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<LearningMachineExport>.Fail("Export cancelled.", "CANCELLED"));

        // Одно чтение версии: все поля записи метаданных берутся из неё, поэтому
        // номер версии и точность в одной записи заведомо относятся к одному состоянию.
        var current = CurrentVersion();

        // Build a metadata record. CheckpointRef here is not a file path — it is a placeholder.
        // For a real checkpoint that restores trained state, call SaveCheckpointAsync instead.
        var export = new LearningMachineExport(
            MachineId: MachineId,
            DomainTag: DomainTag,
            Version: current.SemanticVersion,
            CheckpointRef: $"{MachineId}/{current.SemanticVersion}/checkpoint",
            ManifestRef: $"{MachineId}/{current.SemanticVersion}/manifest.json",
            AccuracyAtExport: current.AccuracyAfterLastBatch,
            ExportedAtUtc: DateTimeOffset.UtcNow);

        return Task.FromResult(OperationResult<LearningMachineExport>.Ok(export));
    }

    // ImportAsync restores only scalar metadata (version, accuracy) from an export record.
    // It does NOT restore trained unit state. For full restore from disk use LoadCheckpointAsync.
    // This method is the metadata counterpart to ExportAsync — not a trained-model load path.
    public Task<OperationResult<bool>> ImportAsync(
        LearningMachineExport export,
        CancellationToken ct = default)
    {
        // Return early if cancellation was requested.
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<bool>.Fail("Import cancelled.", "CANCELLED"));

        // Validate the export record before reading any fields.
        if (export is null)
            return Task.FromResult(OperationResult<bool>.Fail("Export record is required.", "EXPORT_REQUIRED"));

        // Verify the machine identity before restoring any scalar fields.
        if (!string.Equals(export.MachineId, MachineId, StringComparison.Ordinal))
            return Task.FromResult(OperationResult<bool>.Fail(
                $"Export machine id '{export.MachineId}' does not match '{MachineId}'.", "MACHINE_ID_MISMATCH"));

        // Verify the domain tag before restoring any scalar fields.
        if (!string.Equals(export.DomainTag, DomainTag, StringComparison.Ordinal))
            return Task.FromResult(OperationResult<bool>.Fail(
                $"Export domain tag '{export.DomainTag}' does not match '{DomainTag}'.", "DOMAIN_TAG_MISMATCH"));

        // Restore only the scalar metadata — trained unit state is NOT restored here.
        // Обе скалярные величины публикуются одной заменой версии, поэтому
        // читатель не может увидеть новый номер версии при прежней точности.
        var current = CurrentVersion();
        // Публикация условна по прочитанной основе: загрузка не имеет права затирать версию,
        // которую опубликовал другой писатель, пока эта загрузка готовилась.
        if (!TryPublishVersion(current, current with
        {
            SemanticVersion = export.Version,
            AccuracyAfterLastBatch = export.AccuracyAtExport
        }, out var observed))
        {
            // Живое состояние не менялось; повторная попытка — отдельное решение вызывающего.
            return Task.FromResult(OperationResult<bool>.Fail(
                DescribeLostUpdate(current, observed, "Загрузка выгрузки модели"),
                StateVersionChangedErrorCode));
        }

        return Task.FromResult(OperationResult<bool>.Ok(true));
    }

    // ExportCheckpoint builds a TranslationMachineCheckpoint from all current trained unit state.
    // This method does NOT write to disk. Call SaveCheckpointAsync to persist.
    // Returns RUNTIME_STATE_UNAVAILABLE if the machine was constructed via the flow-injection path.
    public OperationResult<TranslationMachineCheckpoint> ExportCheckpoint()
    {
        // Одно чтение версии: выгрузка описывает ровно одно состояние целиком.
        var current = CurrentVersion();

        // Verify that the machine has typed unit references required for checkpoint export.
        if (current.Runtime is null)
            return OperationResult<TranslationMachineCheckpoint>.Fail(
                "Runtime state is not available for this machine.", "RUNTIME_STATE_UNAVAILABLE");

        // Сбор самой записи вынесен отдельно: тем же сбором пользуется подготовка
        // закрытой копии состояния перед обучением партии.
        return OperationResult<TranslationMachineCheckpoint>.Ok(
            BuildCheckpoint(current, current.Runtime));
    }

    // BuildCheckpoint собирает запись контрольной точки из ОДНОЙ версии состояния
    // и её исполнения. Чистая функция: ничего не меняет и ничего не публикует.
    private static TranslationMachineCheckpoint BuildCheckpoint(
        TranslationPublishedVersion version,
        TranslationRuntime runtime)
    {
        // Map the in-memory training history to checkpoint records.
        var historyEntries = version.TrainingHistory
            .Select(h => new TranslationTrainingHistoryEntryCheckpoint(
                BatchId: h.BatchId,
                RowsTrained: h.RowsTrained,
                AccuracyBefore: h.AccuracyBefore,
                AccuracyAfter: h.AccuracyAfter,
                RewardMargin: h.RewardMargin,
                CpuMs: h.CpuMs,
                PeakRamMb: h.PeakRamMb,
                ElapsedMs: h.ElapsedMs,
                PromotedUnitIds: h.PromotedUnitIds))
            .ToList();

        // Build the top-level checkpoint record from machine fields and all unit exports.
        return new TranslationMachineCheckpoint(
            SchemaVersion: 1,
            MachineId: MachineId,
            DomainTag: DomainTag,
            Version: version.SemanticVersion,
            AccuracyAfterLastBatch: version.AccuracyAfterLastBatch,
            HasStructuredTraining: version.HasStructuredTraining,
            ExportedAtUtc: DateTimeOffset.UtcNow,
            KnnStore: runtime.KnnStore.ExportCheckpoint(),
            SemanticTokenizer: runtime.SemanticTokenizer.ExportCheckpoint(),
            Executors: runtime.Executors.Values.Select(e => e.ExportCheckpoint()).ToList(),
            QualityVerifier: runtime.Verifier.ExportCheckpoint(),
            TrainingHistory: historyEntries,
            StructuredTrainingIdentities: version.StructuredTrainingIdentities
                .OrderBy(identity => identity.ExampleId, StringComparer.Ordinal)
                .ThenBy(identity => identity.UnitId, StringComparer.Ordinal)
                .ToList(),
            TatoebaLoop: null);  // Runner sets TatoebaLoop when it calls SaveCheckpointAsync.
    }

    // ImportCheckpoint restores all trained unit state from a checkpoint record into this machine.
    // The machine must have been constructed via the default constructor (not the flow-injection path).
    // Validates the complete checkpoint before mutation and returns a stable validation code on failure.
    public OperationResult<bool> ImportCheckpoint(TranslationMachineCheckpoint checkpoint)
    {
        // Validate the checkpoint record is not null.
        if (checkpoint is null)
            return OperationResult<bool>.Fail("Checkpoint is required.", "CHECKPOINT_REQUIRED");

        // Validate the schema version before reading any fields.
        if (checkpoint.SchemaVersion != 1)
            return OperationResult<bool>.Fail(
                $"Checkpoint schema version {checkpoint.SchemaVersion} is not supported.", "CHECKPOINT_SCHEMA_UNSUPPORTED");

        // Validate the machine identity to prevent loading a checkpoint from a different machine.
        if (!string.Equals(checkpoint.MachineId, MachineId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail(
                $"Checkpoint machine id '{checkpoint.MachineId}' does not match '{MachineId}'.", "MACHINE_ID_MISMATCH");

        // Validate the domain tag to prevent loading a checkpoint from a different domain.
        if (!string.Equals(checkpoint.DomainTag, DomainTag, StringComparison.Ordinal))
            return OperationResult<bool>.Fail(
                $"Checkpoint domain tag '{checkpoint.DomainTag}' does not match '{DomainTag}'.", "DOMAIN_TAG_MISMATCH");

        // Одно чтение текущей версии: она станет основой новой публикации.
        var current = CurrentVersion();

        // Verify typed unit references are present (requires default constructor path).
        if (current.Runtime is null)
            return OperationResult<bool>.Fail(
                "Runtime state is not available for this machine.", "RUNTIME_STATE_UNAVAILABLE");

        // Загрузка идёт в свежее связанное исполнение, а не в живое. Поэтому
        // неудача проверки или загрузки не оставляет живую версию полупринятой,
        // а успешная загрузка публикуется целиком одной заменой ссылки.
        var staged = CreateRuntime();

        // Complete every validation that can fail before mutating this machine or a nested unit.
        var structureValidation = ValidateCheckpointStructure(staged, checkpoint);
        if (!structureValidation.IsSuccess)
            return structureValidation;

        // Загрузка всех обученных частей в копию исполнения.
        ApplyCheckpointToRuntime(staged, checkpoint);

        // Rebuild the training history from checkpoint records.
        var restoredHistory = checkpoint.TrainingHistory
            .Select(entry => new LearningBatchResult(
                entry.BatchId,
                entry.RowsTrained,
                entry.AccuracyBefore,
                entry.AccuracyAfter,
                entry.RewardMargin == LegacyUnverifiedRewardMargin
                    ? 0.0
                    : entry.RewardMargin,
                entry.CpuMs,
                entry.PeakRamMb,
                entry.ElapsedMs,
                entry.PromotedUnitIds))
            .ToList();

        // Restore structured example identities for same-type no-replay checks.
        var restoredIdentities = (checkpoint.StructuredTrainingIdentities ?? []).ToList();

        // Публикация загруженного состояния: номер версии, точность, признак
        // обучения, память единиц, история и опознания становятся видны вместе.
        // Публикация условна по прочитанной основе: загруженное состояние не имеет права затереть
        // версию, опубликованную другим писателем за время проверки и переноса значений.
        if (!TryPublishVersion(current, current with
        {
            Flow = staged.Flow,
            UnitTrainers = staged.UnitTrainers,
            Runtime = staged,
            SemanticVersion = checkpoint.Version,
            AccuracyAfterLastBatch = checkpoint.AccuracyAfterLastBatch,
            HasStructuredTraining = checkpoint.HasStructuredTraining,
            TrainingHistory = restoredHistory,
            StructuredTrainingIdentities = restoredIdentities
        }, out var observed))
        {
            // Живое состояние не менялось; загруженный кандидат остаётся в закрытой копии.
            return OperationResult<bool>.Fail(
                DescribeLostUpdate(current, observed, "Загрузка контрольной точки"),
                StateVersionChangedErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    // ApplyCheckpointToRuntime переносит все обученные части записи контрольной
    // точки в ПЕРЕДАННОЕ исполнение. Вызывающий обязан сначала полностью
    // проверить запись: здесь уже только запись значений.
    private static void ApplyCheckpointToRuntime(
        TranslationRuntime runtime,
        TranslationMachineCheckpoint checkpoint)
    {
        // Restore the shared KNN store (all four executors share the same store reference).
        runtime.KnnStore.ImportCheckpoint(checkpoint.KnnStore);

        // Restore the semantic tokenizer corrections.
        runtime.SemanticTokenizer.ImportCheckpoint(checkpoint.SemanticTokenizer);

        // Restore each direction-specific executor by matching on Direction.
        foreach (var executorCheckpoint in checkpoint.Executors)
        {
            // Complete preflight proved that every direction exists exactly once.
            runtime.Executors[executorCheckpoint.Direction].ImportCheckpoint(executorCheckpoint);
        }

        // Restore the quality verifier rules.
        runtime.Verifier.ImportCheckpoint(checkpoint.QualityVerifier);
    }

    private static OperationResult<bool> ValidateCheckpointStructure(
        TranslationRuntime runtime,
        TranslationMachineCheckpoint checkpoint)
    {
        if (checkpoint.Version is null ||
            checkpoint.KnnStore is null ||
            checkpoint.SemanticTokenizer is null ||
            checkpoint.Executors is null ||
            checkpoint.QualityVerifier is null ||
            checkpoint.TrainingHistory is null ||
            !double.IsFinite(checkpoint.AccuracyAfterLastBatch))
        {
            return OperationResult<bool>.Fail(
                "Checkpoint contains a null or non-finite required top-level value.",
                "CHECKPOINT_STRUCTURE_INVALID");
        }

        if (checkpoint.Executors.Any(executor => executor is null))
            return OperationResult<bool>.Fail(
                "Checkpoint contains a null executor entry.", "CHECKPOINT_STRUCTURE_INVALID");

        foreach (var executorCheckpoint in checkpoint.Executors)
        {
            if (!runtime.Executors.ContainsKey(executorCheckpoint.Direction))
                return OperationResult<bool>.Fail(
                    $"Checkpoint contains unknown executor direction {executorCheckpoint.Direction}.",
                    "EXECUTOR_DIRECTION_UNKNOWN");
        }

        var duplicateDirection = checkpoint.Executors
            .GroupBy(executor => executor.Direction)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateDirection is not null)
            return OperationResult<bool>.Fail(
                $"Checkpoint contains duplicate executor direction {duplicateDirection.Key}.",
                "EXECUTOR_DIRECTION_DUPLICATE");

        var missingDirections = runtime.Executors.Keys
            .Except(checkpoint.Executors.Select(executor => executor.Direction))
            .Order()
            .ToArray();
        if (missingDirections.Length > 0)
            return OperationResult<bool>.Fail(
                $"Checkpoint missing executors for directions: {string.Join(", ", missingDirections)}",
                "EXECUTOR_DIRECTIONS_INCOMPLETE");

        if (checkpoint.KnnStore.Entries is null ||
            checkpoint.KnnStore.Entries.Any(entry =>
                entry is null ||
                entry.Signature is null ||
                entry.Source is null ||
                entry.Target is null))
        {
            return OperationResult<bool>.Fail(
                "Checkpoint KNN state is incomplete.", "CHECKPOINT_STRUCTURE_INVALID");
        }

        if (checkpoint.SemanticTokenizer.PatternCorrections is null ||
            checkpoint.SemanticTokenizer.SignatureCorrections is null ||
            checkpoint.SemanticTokenizer.PatternCorrections.Any(entry =>
                entry is null || entry.Signature is null) ||
            checkpoint.SemanticTokenizer.SignatureCorrections.Any(entry =>
                entry is null ||
                entry.NormalizedRawText is null ||
                entry.TokenizerSignature is null))
        {
            return OperationResult<bool>.Fail(
                "Checkpoint semantic-tokenizer state is incomplete.", "CHECKPOINT_STRUCTURE_INVALID");
        }

        // Форма записи учебной аннотации проверяется ДО любой записи в живое состояние.
        // Иначе сохранённое состояние с противоречивым набором пометок — например, назвавшим
        // сразу прошедшее и настоящее время одной записи — попало бы в обученную машину и
        // снова развело бы пометки с исправленным ответом.
        foreach (var entry in checkpoint.SemanticTokenizer.SignatureCorrections)
        {
            // Неизвестная форма записи не читается: что в ней значат поля, здесь не известно.
            if (entry.AnnotationVersion is not (TranslationTokenizerAnnotationVersions.Legacy
                or TranslationTokenizerAnnotationVersions.ConsistentTagSet))
                return OperationResult<bool>.Fail(
                    $"Сохранённая поправка разборщика смысла '{entry.NormalizedRawText}' объявляет " +
                    $"неизвестную форму записи учебной аннотации {entry.AnnotationVersion}. Загрузка " +
                    $"отменена, состояние переводчика не менялось.",
                    "CHECKPOINT_TOKENIZER_ANNOTATION_VERSION_UNSUPPORTED");

            // Прежняя форма записи не может нести набор пометок: такой записи его негде было
            // взять, и объявить её исправленной значит выдать старое за новое.
            if (entry.AnnotationVersion == TranslationTokenizerAnnotationVersions.Legacy &&
                entry.RuleTags is { Count: > 0 })
                return OperationResult<bool>.Fail(
                    $"Сохранённая поправка разборщика смысла '{entry.NormalizedRawText}' объявлена " +
                    $"прежней формой записи, но несёт набор учебных пометок. Загрузка отменена, " +
                    $"состояние переводчика не менялось.",
                    "CHECKPOINT_TOKENIZER_ANNOTATION_INVALID");

            // Новая форма записи обязана нести набор пометок: иначе согласовать с ним ключи
            // после загрузки нечем.
            if (entry.AnnotationVersion == TranslationTokenizerAnnotationVersions.ConsistentTagSet &&
                entry.RuleTags is null)
                return OperationResult<bool>.Fail(
                    $"Сохранённая поправка разборщика смысла '{entry.NormalizedRawText}' объявлена " +
                    $"согласованной записью, но набора учебных пометок не несёт. Загрузка отменена, " +
                    $"состояние переводчика не менялось.",
                    "CHECKPOINT_TOKENIZER_ANNOTATION_INVALID");

            // Сам набор проверяется той же меркой, что и при обучении: пустых пометок нет и
            // двух разных времён у одной записи нет.
            var annotation = TranslationVerifiedRuleTags.ValidateTrainedAnnotation(entry.RuleTags);
            if (!annotation.IsSuccess)
                return OperationResult<bool>.Fail(
                    $"Сохранённая поправка разборщика смысла '{entry.NormalizedRawText}' несёт " +
                    $"негодный набор учебных пометок: {annotation.ErrorMessage} Загрузка отменена, " +
                    $"состояние переводчика не менялось.",
                    "CHECKPOINT_TOKENIZER_ANNOTATION_INVALID");
        }

        // Перенос старой сохранённой памяти к канонической форме ключа проверяется ДО единой
        // записи в живое состояние. Две записи прежней формы, отличавшиеся только номером
        // учебной пары, сходятся в один канонический ключ; если образцы предложения у них
        // разные, перенос потерял бы один из уроков. Такая загрузка отвергается целиком.
        var migratedPatternKeys =
            new Dictionary<string, (string LegacySignature, SentencePattern Pattern)>(StringComparer.Ordinal);

        // Записи перебираются в том порядке, в котором их восстановила бы загрузка.
        foreach (var entry in checkpoint.SemanticTokenizer.PatternCorrections)
        {
            // Ключ приводится к той форме, в которой он встанет в память после загрузки.
            var migratedKey = TranslationSemanticKey.Canonicalize(entry.Signature);
            // Пустой ключ ячейки не занимает и столкнуться не может.
            if (migratedKey.Length == 0)
                continue;

            // Ключ свободен: запись занимает его и запоминается как первая.
            if (!migratedPatternKeys.TryGetValue(migratedKey, out var first))
            {
                migratedPatternKeys[migratedKey] = (entry.Signature, entry.Pattern);
                continue;
            }

            // Ключ занят тем же образцом: это одна и та же поправка, терять нечего.
            if (first.Pattern == entry.Pattern)
                continue;

            // Ключ занят ДРУГИМ образцом: названы оба прежних ключа и оба образца, чтобы было
            // видно, какие именно сохранённые уроки сошлись и чем они расходятся.
            return OperationResult<bool>.Fail(
                $"Сохранённая память разборщика смысла не переносится: ключи '{first.LegacySignature}' и " +
                $"'{entry.Signature}' сходятся в один рабочий ключ '{migratedKey}', но учат разным " +
                $"образцам предложения — '{first.Pattern}' и '{entry.Pattern}'. Загрузка отменена, " +
                $"состояние переводчика не менялось.",
                "TRANSLATION_SEMANTIC_KEY_COLLISION");
        }

        foreach (var executor in checkpoint.Executors)
        {
            if (executor.WordMap is null ||
                executor.SurfaceWordMap is null ||
                executor.PhraseMap is null ||
                executor.ExactSourceMap is null ||
                executor.WordMap.Any(entry =>
                    entry is null || entry.Lemma is null || entry.Target is null) ||
                HasInvalidStringMapEntry(executor.SurfaceWordMap) ||
                HasInvalidStringMapEntry(executor.PhraseMap) ||
                HasInvalidStringMapEntry(executor.ExactSourceMap))
            {
                return OperationResult<bool>.Fail(
                    $"Checkpoint executor state for {executor.Direction} is incomplete.",
                    "CHECKPOINT_STRUCTURE_INVALID");
            }

            // Check the versioned exact translation memory of this executor. This runs inside
            // structure validation, which the importer calls before it touches any live state,
            // so a broken or ambiguous exact memory can never half-replace the accepted model.
            var exactMemoryValidation = ValidateExecutorExactSourceMemory(executor);
            if (!exactMemoryValidation.IsSuccess)
                return exactMemoryValidation;
        }

        if (checkpoint.QualityVerifier.ConceptRules is null ||
            checkpoint.QualityVerifier.RequiredTermsBySignature is null ||
            checkpoint.QualityVerifier.RequiredTermsBySourceMarker is null ||
            checkpoint.QualityVerifier.ConceptRules.Any(rule =>
                rule is null || rule.Required is null) ||
            HasInvalidVerifierStringList(checkpoint.QualityVerifier.RequiredTermsBySignature) ||
            HasInvalidVerifierStringList(checkpoint.QualityVerifier.RequiredTermsBySourceMarker))
        {
            return OperationResult<bool>.Fail(
                "Checkpoint quality-verifier state is incomplete.", "CHECKPOINT_STRUCTURE_INVALID");
        }

        if (checkpoint.TrainingHistory.Any(entry =>
                entry is null ||
                entry.BatchId is null ||
                entry.PromotedUnitIds is null ||
                entry.PromotedUnitIds.Any(unitId => unitId is null) ||
                !double.IsFinite(entry.AccuracyBefore) ||
                !double.IsFinite(entry.AccuracyAfter) ||
                !double.IsFinite(entry.RewardMargin)))
        {
            return OperationResult<bool>.Fail(
                "Checkpoint training history is incomplete or non-finite.",
                "CHECKPOINT_STRUCTURE_INVALID");
        }

        if (checkpoint.StructuredTrainingIdentities is not null &&
            checkpoint.StructuredTrainingIdentities.Any(identity =>
                identity is null ||
                identity.ExampleId is null ||
                identity.SourceId is null ||
                identity.UnitId is null ||
                identity.BatchId is null))
        {
            return OperationResult<bool>.Fail(
                "Checkpoint structured-training identity state is incomplete.",
                "CHECKPOINT_STRUCTURE_INVALID");
        }

        if (checkpoint.TatoebaLoop is not null &&
            (checkpoint.TatoebaLoop.PrivatePairsPath is null ||
             checkpoint.TatoebaLoop.AcceptedTrainingRows is null ||
             checkpoint.TatoebaLoop.SkippedRows is null ||
             checkpoint.TatoebaLoop.AcceptedTrainingRows.Any(IsInvalidPilotPairReference) ||
             checkpoint.TatoebaLoop.SkippedRows.Any(IsInvalidPilotPairReference) ||
             checkpoint.TatoebaLoop.BestRuntimeProductAccuracy is double bestAccuracy &&
             !double.IsFinite(bestAccuracy)))
        {
            return OperationResult<bool>.Fail(
                "Checkpoint Tatoeba loop state is incomplete or non-finite.",
                "CHECKPOINT_STRUCTURE_INVALID");
        }

        return OperationResult<bool>.Ok(true);
    }

    // ValidateExecutorExactSourceMemory checks the exact translation memory of one executor
    // checkpoint and names the exact reason for a refusal.
    //
    // Why this is strict. The exact memory holds "this source text in this topic translates to
    // this", and the address of a cell is the pair (topic, source). An old saved file addressed
    // the same memory by source text alone, so its topic is simply absent. Reconstructing it is
    // impossible: from the saved pair "bank -> берег" nobody can tell whether the lesson was
    // taught in the river topic or in the finance topic. Therefore an old file whose exact
    // memory is non-empty is refused instead of being relabelled "general", guessed from the
    // translation, or quietly retrained. An old file whose exact memory is empty has no lost
    // topic and is accepted as an empty contextual memory.
    private static OperationResult<bool> ValidateExecutorExactSourceMemory(
        ExecutorUnitCheckpoint executor)
    {
        // The nested version of the exact memory is independent of the container's own version.
        var version = executor.ExactSourceKeyVersion;

        // A version this product cannot read is refused outright; guessing it would silently
        // reinterpret somebody else's saved memory as if it were ours.
        if (version != TranslationExactSourceKey.LegacySourceOnlyVersion &&
            version != TranslationExactSourceKey.ContextualTopicVersion)
        {
            return OperationResult<bool>.Fail(
                $"Checkpoint executor {executor.Direction} declares unsupported exact-source key " +
                $"version {version}; supported versions are " +
                $"{TranslationExactSourceKey.LegacySourceOnlyVersion} and " +
                $"{TranslationExactSourceKey.ContextualTopicVersion}.",
                "CHECKPOINT_EXACT_SOURCE_SCHEMA_UNSUPPORTED");
        }

        // The contextual entries of this executor, which legacy files do not have at all.
        var topicEntries = executor.TopicSourceMap;

        if (version == TranslationExactSourceKey.LegacySourceOnlyVersion)
        {
            // Legacy content carrying contextual entries contradicts its own declared version;
            // the two halves would disagree about which set is the authoritative memory.
            if (topicEntries is not null && topicEntries.Count > 0)
            {
                return OperationResult<bool>.Fail(
                    $"Checkpoint executor {executor.Direction} declares legacy exact-source key " +
                    "version but carries contextual topic entries.",
                    "CHECKPOINT_EXACT_SOURCE_STATE_INVALID");
            }

            // Legacy content with a non-empty source-only memory is refused on purpose: the lost
            // topic of each entry cannot be recovered. This is a deliberate compatibility
            // tightening named by the transfer plan, not a missing migration step.
            if (executor.ExactSourceMap.Count > 0)
            {
                return OperationResult<bool>.Fail(
                    $"Checkpoint executor {executor.Direction} carries a legacy source-only exact " +
                    "memory whose topic context cannot be recovered; migrating that content needs " +
                    "additional source data and is a separate assignment.",
                    "CHECKPOINT_EXACT_SOURCE_CONTEXT_REQUIRED");
            }

            // Legacy content with an empty exact memory loses nothing and is accepted.
            return OperationResult<bool>.Ok(true);
        }

        // From here the content declares the contextual version. A missing collection is not the
        // same thing as an empty collection: an untrained executor must still carry a real list.
        if (topicEntries is null)
        {
            return OperationResult<bool>.Fail(
                $"Checkpoint executor {executor.Direction} declares the contextual exact-source " +
                "key version but carries no topic-source collection.",
                "CHECKPOINT_EXACT_SOURCE_STATE_INVALID");
        }

        // Contextual content requires the legacy list to be empty. Two non-empty authoritative
        // sets are refused rather than silently merged or silently preferred one over the other.
        if (executor.ExactSourceMap.Count > 0)
        {
            return OperationResult<bool>.Fail(
                $"Checkpoint executor {executor.Direction} carries both a non-empty legacy " +
                "source-only exact memory and a contextual topic memory; exactly one " +
                "authoritative set is allowed.",
                "CHECKPOINT_EXACT_SOURCE_STATE_INVALID");
        }

        // Addresses already seen in this collection, used to catch two entries claiming one cell.
        var seenKeys = new HashSet<TranslationExactSourceKey>();
        foreach (var entry in topicEntries)
        {
            // A null row or an empty translation is not a usable cell of the memory.
            if (entry is null || string.IsNullOrWhiteSpace(entry.Target))
            {
                return OperationResult<bool>.Fail(
                    $"Checkpoint executor {executor.Direction} carries a contextual exact-memory " +
                    "entry with no translation.",
                    "CHECKPOINT_EXACT_SOURCE_STATE_INVALID");
            }

            // Both halves of the saved address must already be canonical. Re-normalizing them
            // here would move the lesson into a cell the file never named.
            if (!TranslationExactSourceKey.TryCreateFromStoredPair(
                    entry.TopicDomain, entry.Source, out var key))
            {
                return OperationResult<bool>.Fail(
                    $"Checkpoint executor {executor.Direction} carries a contextual exact-memory " +
                    "entry whose topic or source is empty or not in canonical form.",
                    "CHECKPOINT_EXACT_SOURCE_STATE_INVALID");
            }

            // Two entries that normalize to one address would make the restored memory depend on
            // collection order; that is refused instead of letting the last row win.
            if (!seenKeys.Add(key))
            {
                return OperationResult<bool>.Fail(
                    $"Checkpoint executor {executor.Direction} carries two contextual exact-memory " +
                    $"entries for one address (topic '{entry.TopicDomain}', source '{entry.Source}').",
                    "CHECKPOINT_EXACT_SOURCE_KEY_DUPLICATE");
            }
        }

        // Every entry carries a canonical unique address and a translation.
        return OperationResult<bool>.Ok(true);
    }

    private static bool HasInvalidStringMapEntry(
        IReadOnlyList<ExecutorStringMapEntryCheckpoint> entries)
        => entries.Any(entry =>
            entry is null || entry.Key is null || entry.Value is null);

    private static bool HasInvalidVerifierStringList(
        IReadOnlyList<QualityVerifierStringListCheckpoint> entries)
        => entries.Any(entry =>
            entry is null ||
            entry.Key is null ||
            entry.Values is null ||
            entry.Values.Any(value => value is null));

    private static bool IsInvalidPilotPairReference(TatoebaPilotPairRefCheckpoint entry)
        => entry is null ||
           entry.Direction is null ||
           entry.SourcePattern is null;

    // SaveCheckpointAsync serialises the machine to a JSON checkpoint file at checkpointPath.
    // Uses a tmp-then-move atomic write to avoid leaving a partially written file on crash.
    // Returns a LearningMachineExport whose CheckpointRef is the absolute checkpoint path.
    // loop is an optional TatoebaPilotLoopCheckpoint set by TatoebaPilotLoopRunner to embed cursor state.
    public async Task<OperationResult<LearningMachineExport>> SaveCheckpointAsync(
        string checkpointPath,
        TatoebaPilotLoopCheckpoint? loop = null,   // caller (TatoebaPilotLoopRunner) passes cursor/history
        CancellationToken ct = default)
    {
        // Validate the checkpoint path before any I/O.
        if (ct.IsCancellationRequested)
            return OperationResult<LearningMachineExport>.Fail("Save cancelled.", "CANCELLED");
        if (string.IsNullOrWhiteSpace(checkpointPath))
            return OperationResult<LearningMachineExport>.Fail(
                "Checkpoint path is required.", "CHECKPOINT_PATH_REQUIRED");

        // Build the in-memory checkpoint record from all trained unit state.
        var exportResult = ExportCheckpoint();
        if (!exportResult.IsSuccess)
            return OperationResult<LearningMachineExport>.Fail(exportResult.ErrorMessage, exportResult.ErrorCode);

        // Inject the optional loop cursor — create a with-copy if loop is provided, otherwise use as-is.
        var checkpointToWrite = loop is null
            ? exportResult.Value!
            : exportResult.Value! with { TatoebaLoop = loop };

        // Преобразование в текст и неделимая запись вынесены в ОБЩИЙ писатель: то же правило
        // имени временного файла и тот же порядок «сначала временный файл, потом перемещение»
        // применяет запись проигравшего обученного кандидата. Одно правило живёт в одном месте,
        // поэтому слабого двойника у него нет.
        var json = TranslationAtomicJsonWriter.Serialize(checkpointToWrite);
        var written = await TranslationAtomicJsonWriter.WriteAtomicallyAsync(checkpointPath, json, ct);

        // Прежний код выхода сохранён: вызывающие читают именно CHECKPOINT_WRITE_FAILED.
        if (!written.IsSuccess)
            return OperationResult<LearningMachineExport>.Fail(written.ErrorMessage, "CHECKPOINT_WRITE_FAILED");

        // Номер версии и точность берутся из записи, которая действительно
        // записана на диск: иначе в квитанции оказались бы числа другой версии.
        var writtenVersion = exportResult.Value!;

        // Return a LearningMachineExport whose CheckpointRef is the resolved absolute path.
        var export = new LearningMachineExport(
            MachineId: MachineId,
            DomainTag: DomainTag,
            Version: writtenVersion.Version,
            CheckpointRef: Path.GetFullPath(checkpointPath),
            ManifestRef: Path.GetFullPath(checkpointPath),
            AccuracyAtExport: writtenVersion.AccuracyAfterLastBatch,
            ExportedAtUtc: DateTimeOffset.UtcNow);

        return OperationResult<LearningMachineExport>.Ok(export);
    }

    // LoadCheckpointAsync reads a JSON checkpoint file and returns a fully restored TranslationMachineLoadResult.
    // This is the only valid post-restart load path. No replay or retraining is performed.
    // Returns CHECKPOINT_NOT_FOUND when the file is absent, CHECKPOINT_READ_FAILED on parse error.
    public static async Task<OperationResult<TranslationMachineLoadResult>> LoadCheckpointAsync(
        string checkpointPath,
        CancellationToken ct = default)
    {
        // Validate the checkpoint path and check file existence before reading.
        if (ct.IsCancellationRequested)
            return OperationResult<TranslationMachineLoadResult>.Fail("Load cancelled.", "CANCELLED");
        if (string.IsNullOrWhiteSpace(checkpointPath))
            return OperationResult<TranslationMachineLoadResult>.Fail(
                "Checkpoint path is required.", "CHECKPOINT_PATH_REQUIRED");
        if (!File.Exists(checkpointPath))
            return OperationResult<TranslationMachineLoadResult>.Fail(
                $"Checkpoint file not found: {checkpointPath}", "CHECKPOINT_NOT_FOUND");

        // Read and deserialise the checkpoint JSON.
        TranslationMachineCheckpoint checkpoint;
        try
        {
            var json = await File.ReadAllTextAsync(checkpointPath, System.Text.Encoding.UTF8, ct);
            var parsed = JsonSerializer.Deserialize<TranslationMachineCheckpoint>(
                json, TranslationCheckpointJsonOptions.Standard);
            if (parsed is null)
                return OperationResult<TranslationMachineLoadResult>.Fail(
                    "Checkpoint deserialisation returned null.", "CHECKPOINT_DESERIALIZE_FAILED");
            checkpoint = parsed;
        }
        catch (Exception ex)
        {
            return OperationResult<TranslationMachineLoadResult>.Fail(ex.Message, "CHECKPOINT_READ_FAILED");
        }

        // Create a fresh machine and restore all trained state from the checkpoint.
        var machine = new TranslationMachine();
        var importResult = machine.ImportCheckpoint(checkpoint);
        if (!importResult.IsSuccess)
            return OperationResult<TranslationMachineLoadResult>.Fail(importResult.ErrorMessage, importResult.ErrorCode);

        // Return the fully restored machine with the runner loop cursor — ready for inference without replay.
        return OperationResult<TranslationMachineLoadResult>.Ok(
            new TranslationMachineLoadResult(machine, checkpoint.TatoebaLoop));
    }

    public Task<OperationResult<ILogicalOrchestrator>> GetOrchestratorAsync(CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<ILogicalOrchestrator>.Fail("GetOrchestrator cancelled.", "CANCELLED"));

        // Дирижёр получает не застывший поток, а РЕШАТЕЛЬ текущей версии. Поэтому
        // дирижёр, выданный до публикации новой версии, после неё работает уже с
        // новой: прежний разрыв «меняем одно поле, а старый дирижёр смотрит в
        // прежний граф» закрыт. Версию он разрешает один раз на каждый запрос.
        ILogicalOrchestrator orchestrator = new InlineTranslationOrchestrator(CurrentVersion);
        return Task.FromResult(OperationResult<ILogicalOrchestrator>.Ok(orchestrator));
    }

    // MeasureHeldOutAccuracy измеряет точность на отложенных строках ПЕРЕДАННЫМИ
    // обучателями. Партия передаёт сюда обучателей своей закрытой копии, поэтому
    // измеряется именно та версия, которая будет опубликована.
    private static double MeasureHeldOutAccuracy(
        IReadOnlyDictionary<string, ITranslationUnitTrainer> trainers,
        IReadOnlyList<TranslationLearningExample> heldOut)
    {
        var scored = heldOut
            .GroupBy(example => example.UnitId)
            .Select(group => trainers.TryGetValue(group.Key, out var trainer)
                ? trainer.MeasureAccuracy(group.ToArray())
                : 0.0)
            .ToArray();

        return scored.Length == 0 ? 0.0 : scored.Average();
    }

    private static double MeasureUnitCoverage(IReadOnlySet<string> trainedUnits)
    {
        var required = new[]
        {
            TranslationUnitIds.SemanticTokenizer,
            TranslationUnitIds.ExecutorRuEn,
            TranslationUnitIds.QualityVerifier
        };

        return required.Count(trainedUnits.Contains) / (double)required.Length;
    }

    // TranslateForEvaluationAsync переводит одну строку набора оценки ЗАДАННОЙ
    // версией состояния. Версия передаётся снаружи и на время всего набора не
    // меняется, поэтому измерение не смешивает две версии между своими строками.
    private static async Task<OperationResult<string>> TranslateForEvaluationAsync(
        TranslationPublishedVersion measuredVersion,
        string prompt,
        CancellationToken ct)
    {
        // Зафиксированная версия: решатель всегда возвращает именно её, поэтому
        // все строки набора оценки измеряются одним и тем же состоянием.
        var orchestrator = new InlineTranslationOrchestrator(() => measuredVersion);
        var request = new LogicalOrchestratorRequest(
            RunId: $"eval-{Guid.NewGuid():N}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: prompt,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = "ru",
                ["target_language"] = "en",
                ["topic_domain"] = "general"
            });

        var result = await orchestrator.RunAsync(request, ct);
        return result.IsSuccess
            ? OperationResult<string>.Ok(result.Value!.FinalHumanResult)
            : OperationResult<string>.Fail(result.ErrorMessage, result.ErrorCode);
    }

    // NextPatchVersion возвращает следующий номер версии состояния. Чистая
    // функция: она ничего не меняет, а новый номер попадает в состояние только
    // вместе с публикацией всей новой версии, одной заменой ссылки.
    private static string NextPatchVersion(string currentVersion)
    {
        // Номер разбирается на три части по прежнему правилу проекта.
        var parts = currentVersion.Split('.');
        // Непонятный номер остаётся прежним: выдумывать его формат нельзя.
        return parts.Length == 3 && int.TryParse(parts[2], out var patch)
            ? $"{parts[0]}.{parts[1]}.{patch + 1}"
            : currentVersion;
    }

    private static TranslationRuntime CreateRuntime()
    {
        var knnStore = new TranslationKnnStore();
        var semanticTokenizer = new SemanticTokenizerUnit();
        var executorRuEn = new ExecutorUnit(TranslationDirection.RU_EN, knnStore);
        var executorEnRu = new ExecutorUnit(TranslationDirection.EN_RU, knnStore);
        var executorHeEn = new ExecutorUnit(TranslationDirection.HE_EN, knnStore);
        var executorEnHe = new ExecutorUnit(TranslationDirection.EN_HE, knnStore);
        var executorEsEn = new ExecutorUnit(TranslationDirection.ES_EN, knnStore);
        var executorEnEs = new ExecutorUnit(TranslationDirection.EN_ES, knnStore);
        var verifier = new QualityVerifierUnit();

        var adapters = new Dictionary<TranslationDirection, ITranslationDirectionAdapter>
        {
            [TranslationDirection.RU_EN] = new TrainedTranslationAdapter(
                TranslationDirection.RU_EN,
                semanticTokenizer,
                executorRuEn,
                verifier),
            [TranslationDirection.EN_RU] = new TrainedTranslationAdapter(
                TranslationDirection.EN_RU,
                semanticTokenizer,
                executorEnRu,
                verifier),
            [TranslationDirection.HE_EN] = new TrainedTranslationAdapter(
                TranslationDirection.HE_EN,
                semanticTokenizer,
                executorHeEn,
                verifier),
            [TranslationDirection.EN_HE] = new TrainedTranslationAdapter(
                TranslationDirection.EN_HE,
                semanticTokenizer,
                executorEnHe,
                verifier),
            [TranslationDirection.ES_EN] = new TrainedTranslationAdapter(
                TranslationDirection.ES_EN,
                semanticTokenizer,
                executorEsEn,
                verifier),
            [TranslationDirection.EN_ES] = new TrainedTranslationAdapter(
                TranslationDirection.EN_ES,
                semanticTokenizer,
                executorEnEs,
                verifier)
        };

        var trainers = new Dictionary<string, ITranslationUnitTrainer>(StringComparer.Ordinal)
        {
            [semanticTokenizer.UnitId] = semanticTokenizer,
            [executorRuEn.UnitId] = executorRuEn,
            [executorEnRu.UnitId] = executorEnRu,
            [executorHeEn.UnitId] = executorHeEn,
            [executorEnHe.UnitId] = executorEnHe,
            [executorEsEn.UnitId] = executorEsEn,
            [executorEnEs.UnitId] = executorEnEs,
            [verifier.UnitId] = verifier
        };

        // Collect direction → ExecutorUnit map so checkpoint export can access each executor.
        var executorsByDirection = new Dictionary<TranslationDirection, ExecutorUnit>
        {
            [TranslationDirection.RU_EN] = executorRuEn,
            [TranslationDirection.EN_RU] = executorEnRu,
            [TranslationDirection.HE_EN] = executorHeEn,
            [TranslationDirection.EN_HE] = executorEnHe,
            [TranslationDirection.ES_EN] = executorEsEn,
            [TranslationDirection.EN_ES] = executorEnEs
        };

        // Return the expanded runtime with all typed unit references for checkpoint access.
        return new TranslationRuntime(
            new TranslationRoutingFlow(adapters),
            trainers,
            knnStore,
            semanticTokenizer,
            executorsByDirection,
            verifier);
    }

    // TranslationRuntime holds both the inference graph (Flow) and typed concrete unit references.
    // The typed references are required for checkpoint export/import — the flow only exposes adapters.
    private sealed record TranslationRuntime(
        // Flow is the routing graph used for inference (TranslateAsync / GetOrchestratorAsync).
        TranslationRoutingFlow Flow,
        // UnitTrainers is the unit-id → trainer map used for training dispatch.
        IReadOnlyDictionary<string, ITranslationUnitTrainer> UnitTrainers,
        // KnnStore is the shared KNN bridge between training writes and inference reads.
        TranslationKnnStore KnnStore,
        // SemanticTokenizer is the tokenizer unit used during inference and exported in checkpoints.
        SemanticTokenizerUnit SemanticTokenizer,
        // Executors maps each TranslationDirection to its specific ExecutorUnit for checkpoint access.
        IReadOnlyDictionary<TranslationDirection, ExecutorUnit> Executors,
        // Verifier is the quality verifier unit used during inference and exported in checkpoints.
        QualityVerifierUnit Verifier);

    // TranslationPublishedVersion — одна ОПУБЛИКОВАННАЯ версия состояния машины
    // перевода целиком. Запись неизменна: новая версия не правит прежнюю, а
    // создаётся рядом и публикуется одной заменой ссылки машины.
    //
    // Зачем это нужно по-человечески. Машина перевода держит связанный граф: поток
    // вывода знает адаптеры, адаптеры знают те же единицы, по которым идёт
    // обучение, а дирижёр, однажды выданный наружу, держит поток. Пока эти части
    // лежали отдельными полями, замена памяти доходила не до всех: старый дирижёр
    // продолжал отвечать по прежнему графу, а частично обученная память уже была
    // видна. Версия собирает все части вместе, поэтому читатель видит либо целиком
    // прежнее состояние, либо целиком новое, и никогда — половину партии.
    private sealed record TranslationPublishedVersion(
        // VersionNumber — номер публикации, растущий на единицу с каждой новой
        // версией. Он выдаётся наружу в показании запроса, поэтому по ответу
        // видно, КАКАЯ версия его обслужила.
        long VersionNumber,
        // Flow — поток вывода этой версии: по нему отвечают дирижёры этой версии.
        TranslationRoutingFlow Flow,
        // UnitTrainers — обучатели единиц ЭТОЙ версии: обучение партии идёт по ним.
        IReadOnlyDictionary<string, ITranslationUnitTrainer> UnitTrainers,
        // Runtime — типизированные единицы этой версии для выгрузки и загрузки
        // состояния. Null только на пути внедрения потока, который применяют проверки.
        TranslationRuntime? Runtime,
        // SemanticVersion — человеческий номер версии состояния вида «0.1.7».
        string SemanticVersion,
        // AccuracyAfterLastBatch — точность, измеренная для этой версии.
        double AccuracyAfterLastBatch,
        // HasStructuredTraining — была ли в этой версии хоть одна обученная единица.
        bool HasStructuredTraining,
        // TrainingHistory — история принятых партий этой версии.
        IReadOnlyList<LearningBatchResult> TrainingHistory,
        // StructuredTrainingIdentities — опознания учебных строк этой версии.
        IReadOnlyList<TranslationStructuredTrainingIdentityCheckpoint> StructuredTrainingIdentities);

    private sealed class InlineTranslationOrchestrator : ILogicalOrchestrator
    {
        // _resolveVersion возвращает ТЕКУЩУЮ опубликованную версию. Хранится
        // решатель, а не сам поток: иначе дирижёр, выданный до публикации новой
        // версии, навсегда остался бы на прежнем графе.
        private readonly Func<TranslationPublishedVersion> _resolveVersion;

        public InlineTranslationOrchestrator(Func<TranslationPublishedVersion> resolveVersion)
        {
            // Решатель версии обязателен: без него дирижёр не знает, что читать.
            _resolveVersion = resolveVersion ?? throw new ArgumentNullException(nameof(resolveVersion));
        }

        public async Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
            LogicalOrchestratorRequest request,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<LogicalOrchestratorResult>.Fail("Translation orchestrator cancelled.", "CANCELLED");

            if (request is null)
                return OperationResult<LogicalOrchestratorResult>.Fail("Orchestrator request is required.", "REQUEST_REQUIRED");

            // Версия разрешается РОВНО ОДИН раз на запрос, и весь дальнейший
            // запрос идёт по этому потоку. Поэтому публикация новой версии
            // посреди запроса не может смешать два состояния в одном ответе:
            // начатый запрос доработает на прежней версии, следующий возьмёт новую.
            var version = _resolveVersion();
            // Поток берётся из той же разрешённой версии, второго чтения нет.
            var flow = version.Flow;

            var srcLang = ReadString(request.Metadata, "source_language", "en");
            var tgtLang = ReadString(request.Metadata, "target_language", "en");
            var topicDomain = ReadString(request.Metadata, "topic_domain", "general");

            var translationRequest = new TranslationRoutingRequest(
                SourceLanguage: srcLang ?? "en",
                TargetLanguage: tgtLang ?? "en",
                TopicDomain: topicDomain ?? "general",
                InputText: request.UserRequest);

            var flowRequest = new LogicalFlowRequest<TranslationRoutingRequest>(
                RunId: request.RunId,
                FlowId: TranslationRoutingFlow.FlowId,
                ScenarioId: TranslationRoutingFlow.ScenarioId,
                Input: translationRequest,
                Flags: request.Flags ?? new LogicalExecutionFlags(),
                Metadata: request.Metadata);

            var flowResult = await flow.ExecuteAsync(flowRequest, cancellationToken);
            if (!flowResult.IsSuccess)
                return OperationResult<LogicalOrchestratorResult>.Fail(flowResult.ErrorMessage, flowResult.ErrorCode);

            // Показание версии добавляется к показаниям потока: ответ сам
            // сообщает номер версии, которая его обслужила. Номер берётся из
            // ОДНОГО разрешения версии в начале запроса, второго чтения нет,
            // поэтому один ответ не может нести номера двух версий.
            var stampedMetrics = new Dictionary<string, string>(
                flowResult.Value!.Metrics, StringComparer.Ordinal)
            {
                [StateVersionMetricKey] =
                    version.VersionNumber.ToString(System.Globalization.CultureInfo.InvariantCulture)
            };
            // Результат потока копируется с добавленным показанием: сам поток
            // своих показаний не теряет и ничего не переписывает.
            var stampedFlowResult = flowResult.Value with { Metrics = stampedMetrics };

            var orchestratorResult = new LogicalOrchestratorResult(
                ScenarioId: request.ScenarioId,
                SelectedFlowId: TranslationRoutingFlow.FlowId,
                OverallVerdict: stampedFlowResult.OverallVerdict,
                FinalHumanResult: stampedFlowResult.FinalHumanResult,
                AppliedFlags: request.Flags ?? new LogicalExecutionFlags(),
                FlowResult: stampedFlowResult);

            return OperationResult<LogicalOrchestratorResult>.Ok(orchestratorResult);
        }

        private static string? ReadString(
            IReadOnlyDictionary<string, object> metadata,
            string key,
            string? defaultValue)
        {
            if (!metadata.TryGetValue(key, out var value))
                return defaultValue;

            return value?.ToString() ?? defaultValue;
        }
    }
}
