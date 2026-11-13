// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное регрессионное покрытие исправного родного слоя вложений
// (пятая, последняя строка плана ремонта 88 из папки docs/plans/
// replanning-possible-diffs/fixes-based-on-experiments, раздел «Учебная
// регрессия исправного native Embedding»).
//
// Что здесь проверяется по-человечески. Модель читает текст побайтово: каждый
// байт сначала превращается в вектор из таблицы вложений. Вопрос, на который
// отвечает это покрытие, простой: когда модель учат, доходит ли сигнал
// обучения назад до самой этой таблицы и меняет ли её числа. Раньше ответ
// «доходит» подтверждал только разовый опыт, записанный в файл; ни одна
// строка кода проекта это больше не перепроверяла. Теперь перепроверяет.
//
// Как это доказывается. Одна общая подготовка повторяет исходную программу
// одиннадцатого опыта из обученного старта: два равнодозовых плеча получают
// одни и те же четыре урока в порядке «первый, второй, первый, второй», и
// обучаемый получатель в обоих плечах ровно один — таблица вложений.
//   Плечо A — объявленный необучаемый вид того же самого хранилища чисел:
//             слой вложений видит таблицу как необучаемую, поэтому его выход
//             создаётся без связи с родительской таблицей, и дорога градиента
//             к ней разорвана. Сама таблица при этом остаётся получателем
//             оптимизатора.
//   Плечо B — нынешний родной обратный проход, где связь есть и вклады
//             повторяющихся байтов складываются в одну строку таблицы.
// Преднамеренно нулевое изменение в плече A при реально обучающемся плече B —
// это валидный контроль, а не провал: он показывает, что ненулевой результат
// плеча B пришёл именно из проверяемой связи, а не из чего-то ещё.
//
// Это покрытие уже работающего кода, а НЕ перенос нового алгоритма в продукт.
// Родной `00.TensorCore/Layers/Embedding.cs` здесь не меняется и не копируется:
// прямой и обратный проход слоя вызываются настоящие, продуктовые. Абляция
// плеча A пользуется уже существующей развилкой самого продукта и живёт в
// файлах хозяина опыта, которые подключены сюда ссылкой, а не переписаны.
//
// Откуда берутся ожидаемые числа. Из уже сохранённых доказательств
// одиннадцатого опыта (experiments-v2/RP01-MICRO/P1101/results/attempt-0001:
// status.json, arms.json, metrics.json) и из независимых эталонов: верный знак
// считается из самого запроса обычной целочисленной разностью, ожидаемый
// градиент таблицы складывает независимый оракул по собственному правилу, а
// конечная разность считается «в лоб» двумя лишними прямыми проходами. Числа
// нового кода ожиданиями не становятся.
//
// Чего это покрытие НЕ доказывает. Проверены один профиль модели, один набор
// из двух уроков и двух экзаменов, один обученный старт и девять заранее
// закреплённых координат конечной разности. Это не закон о всех координатах,
// не проверка многознаковой маски ответа, не проверка других предметных
// областей и не заявление о приросте точности: два верных ответа из четырёх
// даёт и тривиальный постоянный ответ знаком «плюс». Холодное восстановление
// сохранённого состояния — отдельная будущая проверка со своим бюджетом; в эти
// пятьдесят девять вызовов модели она не спрятана.

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Inference;
using LoRA;
using P1101Experiment;
using TensorCore.Core;
using TensorCore.Init;
using TensorCore.Layers;
using Tokenizer;
using TrainingLoop;
using TrainingLoop.Optimizers;
using TrainingLoop.Schedulers;
using TransformerModel;
using TransformerModel.Config;

/// <summary>Одно чтение панели: что спросили, что ответили и насколько уверенно.</summary>
/// <param name="Arm">Имя плеча: назначенная абляция или родной обратный проход.</param>
/// <param name="Phase">Когда прочитано: до обучения или после него.</param>
/// <param name="Id">Короткое имя строки панели.</param>
/// <param name="Kind">Вид строки: учебная с удержанием или экзаменационная.</param>
/// <param name="Gold">Верный знак, посчитанный из самого запроса.</param>
/// <param name="Answer">Жадный ответ модели одним знаком.</param>
/// <param name="Correct">Совпал ли ответ с верным.</param>
/// <param name="LogPlus">Логарифмическая вероятность знака «плюс».</param>
/// <param name="LogMinus">Логарифмическая вероятность знака «минус».</param>
/// <param name="PrefillForwardCalls">Сколько прямых проходов стоило чтение запроса.</param>
/// <param name="DecodeForwardCalls">Сколько прямых проходов стоило продолжение.</param>
public sealed record EmbeddingPanelRow(
    string Arm,
    string Phase,
    string Id,
    string Kind,
    string Gold,
    string Answer,
    bool Correct,
    double LogPlus,
    double LogMinus,
    int PrefillForwardCalls,
    int DecodeForwardCalls);

/// <summary>Один урок одного плеча и насколько он сдвинул таблицу вложений.</summary>
/// <param name="Arm">Имя плеча этого урока.</param>
/// <param name="Index">Порядковый номер урока внутри плеча.</param>
/// <param name="LessonId">Какая учебная строка предъявлена.</param>
/// <param name="Loss">Величина потерь, вернувшаяся из настоящего шага обучения.</param>
/// <param name="ChangedCoordinates">Сколько чисел таблицы изменил этот урок.</param>
/// <param name="ChangedRows">Сколько строк таблицы изменил этот урок.</param>
/// <param name="LearningRate">Скорость обучения, назначенная расписанием этому шагу.</param>
public sealed record EmbeddingLessonRow(
    string Arm,
    int Index,
    string LessonId,
    float Loss,
    int ChangedCoordinates,
    int ChangedRows,
    double LearningRate);

/// <summary>Итоговое изменение таблицы вложений за все уроки одного плеча.</summary>
/// <param name="TotalCoordinates">Всего чисел в таблице.</param>
/// <param name="ChangedCoordinates">Сколько чисел изменилось.</param>
/// <param name="ChangedRowCount">Сколько строк изменилось.</param>
/// <param name="DeltaL2">Длина вектора изменения таблицы.</param>
/// <param name="MaxAbsoluteDelta">Наибольшее по модулю изменение одного числа.</param>
/// <param name="ChangedRows">Номера изменившихся строк по возрастанию.</param>
public sealed record EmbeddingTableDelta(
    int TotalCoordinates,
    int ChangedCoordinates,
    int ChangedRowCount,
    double DeltaL2,
    double MaxAbsoluteDelta,
    int[] ChangedRows);

/// <summary>Одна заранее закреплённая координата независимого контроля «в лоб».</summary>
/// <param name="Row">Номер строки таблицы, то есть номер знака.</param>
/// <param name="Column">Номер столбца внутри строки.</param>
/// <param name="Why">Человеческая причина, по которой координата закреплена.</param>
/// <param name="Analytic">Производная, снятая настоящим обратным проходом.</param>
/// <param name="FiniteDifference">Независимая центральная конечная разность.</param>
/// <param name="AbsoluteError">Абсолютная погрешность согласия двух способов.</param>
/// <param name="RelativeError">Относительная погрешность согласия двух способов.</param>
/// <param name="Agrees">Уложилось ли согласие в заранее закреплённый порог.</param>
public sealed record EmbeddingDerivativeRow(
    int Row,
    int Column,
    string Why,
    double Analytic,
    double FiniteDifference,
    double AbsoluteError,
    double RelativeError,
    bool Agrees);

/// <summary>Один файл сохранённого состояния и его отпечаток.</summary>
/// <param name="Name">Имя файла внутри назначенного каталога.</param>
/// <param name="Sha256">Отпечаток фактических байтов файла.</param>
/// <param name="Bytes">Размер файла в байтах.</param>
public sealed record SavedStartFile(string Name, string Sha256, long Bytes);

/// <summary>
/// Общая подготовка: исходная программа одиннадцатого опыта выполняется ровно
/// один раз на весь класс проверок, и все проверки читают одну эту квитанцию.
/// Без общей подготовки каждая проверка создавала бы новый экземпляр класса и
/// назначенная доза вызовов модели умножилась бы на число проверок.
/// </summary>
public sealed class EmbeddingLearningReachabilityFixture : IDisposable
{
    // ── Паспортные опознаватели проверенного обученного старта ───────────────────
    /// <summary>Отпечаток весов старта, названный паспортом опыта и описью набора.</summary>
    public const string StartWeightsSha256 =
        "8721dd57b11fa61b11acf37c337611129891a1d3096ae0392a89455d6d58fdec";

    /// <summary>Счётчик шага донорского оптимизатора: происхождение старта, а не доза.</summary>
    public const int DonorOptimizerStep = 400;

    /// <summary>Зерно начальных значений замороженной основы набора.</summary>
    private const ulong BaseSeed = 62001UL;

    /// <summary>Зерно начальных значений адаптеров набора.</summary>
    private const ulong AdapterSeed = 62002UL;

    /// <summary>Поток случайных чисел адаптеров набора.</summary>
    private const string AdapterStream = "rms-row-reduce-probe-v1";

    /// <summary>Текст топологии набора: им подписан снимок генератора случайных чисел.</summary>
    private const string TopologyLiteral = "RD01|V256|H64|L1|Q4|KV2|FF176|C64|R4|q_proj,v_proj";

    /// <summary>Текст настройки адаптеров набора: им подписан тот же снимок.</summary>
    private const string LoraLiteral = "RD01|alpha8|dropout0|seed62002|rms-row-reduce-probe-v1";

    // ── Настройки чтения панели, закреплённые первичным одиннадцатым опытом ──────
    /// <summary>Зерно выдачи; при нулевой температуре на выбор знака не влияет.</summary>
    private const int ReadoutSeed = 20261028;

    /// <summary>Ровно один знак ответа, как требует паспорт опыта.</summary>
    private const int ReadoutMaxNewTokens = 1;

    /// <summary>Жадная выдача: случайность в выборе знака не участвует.</summary>
    private const float ReadoutTemperature = 0f;

    /// <summary>Отсечение по числу кандидатов выключено.</summary>
    private const int ReadoutTopK = 0;

    /// <summary>Отсечение по суммарной вероятности выключено.</summary>
    private const float ReadoutTopP = 1f;

    // ── Пороги согласия: новая явная регистрация ДО запуска ──────────────────────
    // В первичном опыте эти три числа жили в коде запускателя, но не были
    // численно закреплены в карточке. План ремонта требует закрепить их
    // константами заранее. Это регистрация порогов, а не изменение статуса
    // исторического результата.

    /// <summary>Шаг возмущения независимого контроля «в лоб».</summary>
    public const double FiniteDifferenceEpsilon = 0.01d;

    /// <summary>Относительный порог согласия двух способов получения производной.</summary>
    public const double FiniteDifferenceRelativeTolerance = 0.15d;

    /// <summary>Абсолютный порог согласия двух способов получения производной.</summary>
    public const double FiniteDifferenceAbsoluteTolerance = 2e-4;

    /// <summary>Порог, ниже которого величина считается околонулевой.</summary>
    public const double NearZeroThreshold = 1e-6;

    /// <summary>Предельная абсолютная погрешность сверки с независимым оракулом сложения.</summary>
    public const double OracleAbsoluteTolerance = 1e-5;

    /// <summary>Предельная относительная погрешность сверки с тем же оракулом.</summary>
    public const double OracleRelativeTolerance = 1e-4;

    // ── Назначенная планом доза: исполняется ровно, без скрытых лишних вызовов ───
    /// <summary>Назначенное число прямых проходов модели за всю подготовку.</summary>
    public const int PlannedModelForwards = 59;

    /// <summary>Назначенное число обратных проходов модели за всю подготовку.</summary>
    public const int PlannedModelBackwards = 9;

    /// <summary>Назначенное число прямых проходов одного только слоя вложений.</summary>
    public const int PlannedLayerForwards = 3;

    /// <summary>Назначенное число обратных проходов одного только слоя вложений.</summary>
    public const int PlannedLayerBackwards = 1;

    /// <summary>Назначенное число обращений к оптимизатору: по четыре на каждое плечо.</summary>
    public const int PlannedOptimizerCalls = 8;

    /// <summary>Назначенное число действующих изменений в плече назначенной абляции.</summary>
    public const int PlannedEffectiveUpdatesAblationArm = 0;

    /// <summary>Назначенное число действующих изменений в плече родного обратного прохода.</summary>
    public const int PlannedEffectiveUpdatesNativeArm = 4;

    /// <summary>Назначенное число жадных выдач панели.</summary>
    public const int PlannedGenerations = 16;

    /// <summary>Назначенное число прогревочных прогонов: ноль, старт берётся обученным.</summary>
    public const int PlannedBootstrapRuns = 0;

    /// <summary>Число уроков в каждом плече: порядок «первый, второй, первый, второй».</summary>
    public const int LessonsPerArm = 4;

    // ── Имена плеч и фаз, чтобы строки квитанции читались человеком ──────────────
    /// <summary>Имя плеча назначенной абляции.</summary>
    public const string AblationArm = "A";

    /// <summary>Имя плеча нынешнего родного обратного прохода.</summary>
    public const string NativeArm = "B";

    /// <summary>Имя фазы чтения до обучения.</summary>
    public const string PhaseBefore = "до";

    /// <summary>Имя фазы чтения после обучения.</summary>
    public const string PhaseAfter = "после";

    // ── Счётчики фактической работы: берутся на вызове, а не после успеха ────────
    /// <summary>Учебные прямые проходы внутри настоящего шага обучения.</summary>
    public int TrainForwards { get; private set; }

    /// <summary>Учебные обратные проходы внутри того же шага.</summary>
    public int TrainBackwards { get; private set; }

    /// <summary>Жадные выдачи панели: по одной на каждое чтение.</summary>
    public int Generations { get; private set; }

    /// <summary>Прямые проходы модели, потраченные самими жадными выдачами.</summary>
    public int GenerationForwards { get; private set; }

    /// <summary>Отдельные оценивающие прямые проходы, дающие числа уверенности.</summary>
    public int ScoringForwards { get; private set; }

    /// <summary>Прямые проходы модели в контрольных фазах вне обучения и панели.</summary>
    public int ControlModelForwards { get; private set; }

    /// <summary>Обратные проходы модели в тех же контрольных фазах.</summary>
    public int ControlModelBackwards { get; private set; }

    /// <summary>Прямые проходы одного только слоя вложений.</summary>
    public int LayerForwards { get; private set; }

    /// <summary>Обратные проходы одного только слоя вложений.</summary>
    public int LayerBackwards { get; private set; }

    /// <summary>Фактические обращения к оптимизатору по обоим плечам.</summary>
    public int OptimizerCalls { get; private set; }

    /// <summary>Прогревочные прогоны с нуля: обучение с нуля здесь не выполняется.</summary>
    public int BootstrapRuns => 0;

    /// <summary>Все прямые проходы модели вместе: обучение, панель и контроли.</summary>
    public int ModelForwards => TrainForwards + ScoringForwards + GenerationForwards + ControlModelForwards;

    /// <summary>Все обратные проходы модели вместе: обучение и контроли.</summary>
    public int ModelBackwards => TrainBackwards + ControlModelBackwards;

    /// <summary>Счёт прямых проходов модели на момент перед сохранением состояния.</summary>
    public int ModelForwardsBeforeSave { get; private set; }

    // ── Личность входов сохранённого старта ──────────────────────────────────────
    /// <summary>Папка проверенного обученного старта, из которого начата подготовка.</summary>
    public string StartBundleDirectory { get; }

    /// <summary>Имена и отпечатки входов старта, сверенные с описью набора.</summary>
    public IReadOnlyList<SavedStartFile> StartInputs { get; }

    /// <summary>Все ли входы совпали с отпечатками, объявленными описью набора.</summary>
    public bool StartInputsMatchManifest { get; }

    // ── Установленный единственный фактор ────────────────────────────────────────
    /// <summary>Связан ли выход слоя вложений с таблицей в плече назначенной абляции.</summary>
    public bool AblationArmOutputLinkedToTable { get; }

    /// <summary>Связан ли тот же выход с таблицей в плече родного обратного прохода.</summary>
    public bool NativeArmOutputLinkedToTable { get; }

    /// <summary>Делит ли вид слоя в плече абляции то же хранилище чисел, что и таблица.</summary>
    public bool AblationArmViewSharesStorage { get; }

    /// <summary>Остаётся ли таблица получателем оптимизатора в плече абляции.</summary>
    public bool AblationArmTableIsOptimizerRecipient { get; }

    /// <summary>Сколько обучаемых массивов в плече абляции: обязан быть один.</summary>
    public int AblationArmTrainableParameters { get; }

    /// <summary>Сколько обучаемых массивов в плече родного прохода: обязан быть один.</summary>
    public int NativeArmTrainableParameters { get; }

    /// <summary>Имя привязки таблицы вложений в контрольной точке модели.</summary>
    public string TableBindingKey { get; }

    /// <summary>Имя самой первой записи контрольной точки модели.</summary>
    public string FirstCheckpointEntryKey { get; }

    // ── Материал уроков и достижимые строки таблицы ──────────────────────────────
    /// <summary>Номера знаков, которые уроки действительно предъявляют слою вложений.</summary>
    public IReadOnlyList<int> ReachableRows { get; }

    /// <summary>
    /// Сколько строк таблицы получают больше одного вклада внутри первого урока.
    /// Считается именно по первому уроку: ровно эту последовательность получает
    /// независимый оракул сложения, поэтому числа сравнимы между собой. По
    /// объединению двух уроков повторяется каждая строка, потому что оба запроса
    /// собраны из одного и того же набора байтов, и такое число ничего о сложении
    /// вкладов не говорит.
    /// </summary>
    public int FirstLessonRepeatedRowCount { get; }

    /// <summary>Номер знака «плюс»: его строка во входах уроков отсутствует.</summary>
    public int PlusTokenId { get; }

    /// <summary>Номер знака «минус»: он встречается внутри самого запроса.</summary>
    public int MinusTokenId { get; }

    /// <summary>Всего чисел в таблице вложений.</summary>
    public int TableCoordinates { get; }

    // ── Измеренный результат общей подготовки ────────────────────────────────────
    /// <summary>Восемь чтений панели до обучения: по четыре на каждое плечо.</summary>
    public IReadOnlyList<EmbeddingPanelRow> PanelBefore { get; }

    /// <summary>Восемь чтений панели после обучения: по четыре на каждое плечо.</summary>
    public IReadOnlyList<EmbeddingPanelRow> PanelAfter { get; }

    /// <summary>Восемь уроков обоих плеч со своими числами.</summary>
    public IReadOnlyList<EmbeddingLessonRow> Lessons { get; }

    /// <summary>Итоговое изменение таблицы в плече назначенной абляции.</summary>
    public EmbeddingTableDelta AblationArmDelta { get; }

    /// <summary>Итоговое изменение таблицы в плече родного обратного прохода.</summary>
    public EmbeddingTableDelta NativeArmDelta { get; }

    /// <summary>Счётчик шага оптимизатора плеча абляции после всех уроков.</summary>
    public int AblationArmOptimizerStep { get; }

    /// <summary>Сколько записей моментов завёл оптимизатор плеча абляции.</summary>
    public int AblationArmMomentEntries { get; }

    /// <summary>Счётчик шага оптимизатора плеча родного прохода после всех уроков.</summary>
    public int NativeArmOptimizerStep { get; }

    /// <summary>Сколько записей моментов завёл оптимизатор плеча родного прохода.</summary>
    public int NativeArmMomentEntries { get; }

    // ── Независимый оракул сложения по строкам таблицы ───────────────────────────
    /// <summary>Сколько чисел таблицы сравнил независимый оракул сложения.</summary>
    public int OracleComparedCoordinates { get; }

    /// <summary>Сколько чисел вышло за объявленный допуск оракула.</summary>
    public int OracleMismatchedCoordinates { get; }

    /// <summary>Наибольшая абсолютная разница между ожиданием оракула и продуктом.</summary>
    public double OracleMaxAbsoluteDifference { get; }

    /// <summary>Сколько строк оракул ожидал ненулевыми.</summary>
    public int OracleExpectedNonZeroRows { get; }

    /// <summary>Сколько строк оказалось ненулевыми на самом деле.</summary>
    public int OracleActualNonZeroRows { get; }

    /// <summary>Сколько строк получили вклад более чем одной позиции урока.</summary>
    public int OracleRepeatedIdRows { get; }

    /// <summary>Остались ли строки отсутствующих знаков ровно нулевыми.</summary>
    public bool OracleAbsentRowsAreExactlyZero { get; }

    /// <summary>Сошлось ли всё в пределах объявленного допуска оракула.</summary>
    public bool OracleAgrees { get; }

    // ── Независимый контроль «в лоб» ─────────────────────────────────────────────
    /// <summary>Девять заранее закреплённых координат и оба способа их производной.</summary>
    public IReadOnlyList<EmbeddingDerivativeRow> FiniteDifferenceRows { get; }

    // ── Сторожа неизменности ─────────────────────────────────────────────────────
    /// <summary>Отпечаток замороженной основы плеча абляции до уроков.</summary>
    public string AblationArmFrozenBeforeSha256 { get; }

    /// <summary>Тот же отпечаток после всей подготовки.</summary>
    public string AblationArmFrozenAfterSha256 { get; }

    /// <summary>Отпечаток замороженной основы плеча родного прохода до уроков.</summary>
    public string NativeArmFrozenBeforeSha256 { get; }

    /// <summary>Тот же отпечаток после всей подготовки.</summary>
    public string NativeArmFrozenAfterSha256 { get; }

    /// <summary>Отпечаток неизменяемой эталонной модели до уроков.</summary>
    public string ReferenceBeforeSha256 { get; }

    /// <summary>Тот же отпечаток после всей подготовки.</summary>
    public string ReferenceAfterSha256 { get; }

    // ── Сохранённое заново полученное состояние ──────────────────────────────────
    /// <summary>Назначенный планом каталог сохранения нового обученного состояния.</summary>
    public string SavedStartDirectory { get; }

    /// <summary>Состав сохранённого каталога с отпечатками фактических байтов.</summary>
    public IReadOnlyList<SavedStartFile> SavedStartFiles { get; }

    /// <summary>Совпали ли отпечатки описи с фактическими байтами сохранённых файлов.</summary>
    public bool SavedStartManifestMatchesBytes { get; }

    /// <summary>Записан ли сохранённый каталог именно этим прогоном, а не прежним.</summary>
    public bool SavedStartWrittenByThisRun { get; }

    /// <summary>Сколько шагов обучения с учителем выполнило сохранённое плечо.</summary>
    public int SavedSupervisedStepsPerformed { get; }

    /// <summary>Выполнить общую подготовку: исходная программа одиннадцатого опыта один раз.</summary>
    public EmbeddingLearningReachabilityFixture()
    {
        // ── 1. Личность входов: ни один байт сохранённого старта не берётся на веру ──
        StartBundleDirectory = SharedLearnedStartDirectory(); // Папка проверенного обученного старта.
        var weightsPath = Path.Combine(StartBundleDirectory, "S0.weights.bin"); // Файл сохранённых весов старта.
        var actualWeightsSha = Sha256(File.ReadAllBytes(weightsPath)); // Отпечаток фактических байтов весов.
        if (actualWeightsSha != StartWeightsSha256) // Другой отпечаток означает другой старт, а не ту же проверку.
            throw new InvalidOperationException("EMBEDDING_REACHABILITY_START_WEIGHTS_SHA:" + actualWeightsSha);

        using var bundleDocument = JsonDocument.Parse(
            File.ReadAllBytes(Path.Combine(StartBundleDirectory, "bundle.json"))); // Опись набора.
        var declaredShaByName = bundleDocument.RootElement.GetProperty("files").EnumerateArray()
            .ToDictionary(
                file => file.GetProperty("name").GetString()!,
                file => file.GetProperty("sha256").GetString()!); // Отпечатки, объявленные самой описью.
        var inputNames = new[]
        {
            "S0.weights.bin", "S0.optimizer.bin", "S0.rng.json",
            "tokenizer.json", "data.material.json", "data.source.jsonl", "generation.json"
        }; // Входы старта, названные паспортом первичного опыта.
        var inputs = new List<SavedStartFile>(inputNames.Length); // Опись фактически прочитанных входов.
        var inputsMatch = true; // Пока ни одного расхождения не найдено.
        foreach (var name in inputNames) // Каждый вход читается и сверяется отдельно.
        {
            var path = Path.Combine(StartBundleDirectory, name); // Полный путь входа.
            if (!File.Exists(path)) // Отсутствующий вход — это другой набор, а не та же проверка.
                throw new InvalidOperationException("EMBEDDING_REACHABILITY_INPUT_MISSING:" + name);
            var bytes = File.ReadAllBytes(path); // Фактические байты входа.
            var actual = Sha256(bytes); // Настоящий отпечаток прочитанных байтов.
            inputs.Add(new SavedStartFile(name, actual, bytes.LongLength)); // Строка описи этого входа.
            if (declaredShaByName.TryGetValue(name, out var declared) && declared != actual)
                inputsMatch = false; // Опись и байты разошлись: это зафиксируется, а не спрячется.
        }

        StartInputs = inputs; // Опись входов доступна проверкам.
        StartInputsMatchManifest = inputsMatch; // Общий вердикт сверки входов с описью.
        if (declaredShaByName["S0.weights.bin"] != StartWeightsSha256) // Опись обязана называть тот же отпечаток весов.
            throw new InvalidOperationException("EMBEDDING_REACHABILITY_BUNDLE_SHA:" + declaredShaByName["S0.weights.bin"]);

        // ── 2. Численная настройка модели и обучения читается из описания набора ────
        using var generationDocument = JsonDocument.Parse(
            File.ReadAllBytes(Path.Combine(StartBundleDirectory, "generation.json"))); // Описание порождения набора.
        var modelBlock = generationDocument.RootElement.GetProperty("model"); // Раздел численной настройки модели.
        var config = new ModelConfig
        {
            VocabSize = modelBlock.GetProperty("VocabSize").GetInt32(), // Размер словаря.
            ContextLen = modelBlock.GetProperty("ContextLen").GetInt32(), // Длина окна внимания.
            HiddenDim = modelBlock.GetProperty("HiddenDim").GetInt32(), // Ширина скрытого слоя.
            NumLayers = modelBlock.GetProperty("NumLayers").GetInt32(), // Число слоёв.
            NumHeads = modelBlock.GetProperty("NumHeads").GetInt32(), // Число голов запроса.
            NumKvHeads = modelBlock.GetProperty("NumKvHeads").GetInt32(), // Число голов ключей и значений.
            FfnHiddenDim = modelBlock.GetProperty("FfnHiddenDim").GetInt32(), // Ширина полносвязного слоя.
            RopeTheta = modelBlock.GetProperty("RopeTheta").GetSingle(), // Основание поворотных позиций.
            RmsNormEps = modelBlock.GetProperty("RmsNormEps").GetSingle() // Сглаживание нормировки.
        };
        var trainingBlock = generationDocument.RootElement.GetProperty("training"); // Раздел настроек обучения набора.
        var optimizerBlock = generationDocument.RootElement.GetProperty("optimizer"); // Раздел настроек оптимизатора.
        var trainingConfig = new TrainingConfig
        {
            BatchSize = trainingBlock.GetProperty("batchSize").GetInt32(), // Размер партии донора.
            SeqLen = trainingBlock.GetProperty("seqLen").GetInt32(), // Длина последовательности донора.
            MaxLr = trainingBlock.GetProperty("maxLr").GetSingle(), // Верхняя скорость обучения донора.
            MinLr = trainingBlock.GetProperty("minLr").GetSingle(), // Нижняя скорость обучения донора.
            WarmupSteps = trainingBlock.GetProperty("warmup").GetInt32(), // Разогрев расписания донора.
            TotalSteps = trainingBlock.GetProperty("totalSteps").GetInt32(), // Длина расписания донора.
            GradAccumSteps = trainingBlock.GetProperty("gradAccumSteps").GetInt32(), // Накопление градиента донора.
            GradClipNorm = trainingBlock.GetProperty("gradClipNorm").GetSingle(), // Обрезка нормы градиента донора.
            WeightDecay = trainingBlock.GetProperty("weightDecay").GetSingle(), // Затухание весов расписания донора.
            CheckpointDir = Path.GetTempPath(), // Шаг обучения с учителем эту папку не использует.
            CheckpointEvery = int.MaxValue, // Контрольные точки здесь не сохраняются.
            LogEvery = int.MaxValue // Журнал продуктового цикла здесь не используется.
        };
        if (trainingConfig.WarmupSteps != 0) // Конфигурация первичного опыта: разогрева нет, скорость постоянна.
            throw new InvalidOperationException("EMBEDDING_REACHABILITY_WARMUP_NOT_ZERO:" + trainingConfig.WarmupSteps);
        var scheduler = new CosineWithWarmup(
            trainingConfig.WarmupSteps, trainingConfig.TotalSteps, trainingConfig.MaxLr, trainingConfig.MinLr);
        for (var step = 0; step < LessonsPerArm; step++) // Все четыре шага обязаны идти на одной скорости.
        {
            if (scheduler.GetLr(step) != trainingConfig.MaxLr) // Любой другой профиль — это другой опыт.
                throw new InvalidOperationException("EMBEDDING_REACHABILITY_LEARNING_RATE_NOT_CONSTANT:" + step);
        }

        // ── 3. Словарь: тот самый побайтовый переходник, которым читал опыт ─────────
        var tokenizer = BpeTokenizer.Load(Path.Combine(
            RepositoryRoot(), "experiments-v2", "RP01-MICRO", "P1101", "src", "byte-identity-tokenizer.json"));
        if (tokenizer.VocabSize != config.VocabSize) // Ширина словаря обязана совпасть с моделью.
            throw new InvalidOperationException("EMBEDDING_REACHABILITY_TOKENIZER_VOCAB:" + tokenizer.VocabSize);
        int[] Ids(string text) => [.. tokenizer.Encode(text)]; // Номера знаков строки.
        PlusTokenId = Ids("+").Single(); // Номер знака «плюс»: ответ ровно из одного знака.
        MinusTokenId = Ids("-").Single(); // Номер знака «минус»: ответ ровно из одного знака.

        // ── 4. Уроки и экзамен — донорские байты набора, а не переписанный текст ───
        var donorRows = File.ReadAllLines(Path.Combine(StartBundleDirectory, "data.source.jsonl"))
            .Where(line => line.Trim().Length > 0)
            .Select(line =>
            {
                using var row = JsonDocument.Parse(line); // Каждая донорская строка разбирается отдельно.
                return (
                    Id: row.RootElement.GetProperty("id").GetString()!, // Короткое имя строки.
                    Prompt: row.RootElement.GetProperty("prompt").GetString()!, // Запрос строки.
                    Answer: row.RootElement.GetProperty("answer").GetString()!); // Верный ответ строки.
            })
            .ToArray(); // Донорские байты двух учебных строк.
        if (donorRows.Length != 2 || donorRows[0].Id != "T1" || donorRows[1].Id != "T2")
            throw new InvalidOperationException("EMBEDDING_REACHABILITY_DONOR_ROWS"); // Донор обязан дать ровно две строки.

        // Панель чтения: две учебные строки и две экзаменационные; верный знак считается из запроса.
        var panel = new[]
        {
            (Id: "T1", Prompt: donorRows[0].Prompt, Kind: "обучение и удержание"), // Первая учебная строка.
            (Id: "T2", Prompt: donorRows[1].Prompt, Kind: "обучение и удержание"), // Вторая учебная строка.
            (Id: "E1", Prompt: "C# result sign: 8-3 =", Kind: "экзамен"), // Первая экзаменационная строка.
            (Id: "E2", Prompt: "C# result sign: 3-8 =", Kind: "экзамен") // Вторая экзаменационная строка.
        };
        var goldById = panel.ToDictionary(row => row.Id, row => GoldFromPrompt(row.Prompt)); // Независимый эталон.
        if (goldById["T1"] != donorRows[0].Answer || goldById["T2"] != donorRows[1].Answer)
            throw new InvalidOperationException("EMBEDDING_REACHABILITY_GOLD_DIFFERS_FROM_DONOR"); // Эталон обязан совпасть.

        // ── 5. Восстановление проверенного обученного старта пятью копиями ─────────
        var lora = new LoraConfig { Rank = 4, Alpha = 8f, Dropout = 0f, TargetModules = ["q_proj", "v_proj"] };
        var topologySha = Sha256Text(TopologyLiteral); // Опознаватель топологии для снимка генератора.
        var loraSha = Sha256Text(LoraLiteral); // Опознаватель настройки адаптеров для того же снимка.
        var randomSnapshot = JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>(
            File.ReadAllBytes(Path.Combine(StartBundleDirectory, "S0.rng.json")))
            ?? throw new InvalidOperationException("EMBEDDING_REACHABILITY_RANDOM_STATE_UNREADABLE");
        var randomIdentity = new LlmModelRandomStateIdentity(topologySha, loraSha); // Пара опознавателей снимка.

        LlmModel BuildModel() // Одинаковая сборка для обоих плеч, эталона, оракула и контроля.
        {
            LlmModel built; // Модель объявлена снаружи блока зерна, чтобы жить дальше.
            using (Initializers.UseDeterministicSeed(BaseSeed)) built = new LlmModel(config); // Основа тем же зерном.
            LoraApplicator.ApplySeeded(built, lora, AdapterSeed, AdapterStream); // Адаптеры тем же зерном и потоком.
            built.LoadCheckpoint(Path.Combine(StartBundleDirectory, "S0.weights.bin")); // Поверх кладутся веса старта.
            built.RestoreRandomState(randomSnapshot, randomIdentity); // Состояние генератора восстановлено с проверкой.
            return built; // Готовая копия проверенного обученного старта.
        }

        Parameter TableOf(LlmModel model) // Таблица вложений как зарегистрированный обучаемый массив модели.
        {
            var table = NativeAccess.TokenEmbedding(model).Weight; // Та самая таблица, которой пользуется слой.
            if (!model.Parameters().Any(parameter => ReferenceEquals(parameter, table)))
                throw new InvalidOperationException("EMBEDDING_REACHABILITY_TABLE_NOT_REGISTERED"); // Должна быть в модели.
            if (table.Value.Shape.Dimensions[0] != config.VocabSize
                || table.Value.Shape.Dimensions[1] != config.HiddenDim)
                throw new InvalidOperationException("EMBEDDING_REACHABILITY_TABLE_SHAPE"); // Форма обязана совпасть.
            return table; // Назначенный получатель урока.
        }

        void TrainOnlyTable(LlmModel model) // Обучаемый получатель ровно один: таблица вложений.
        {
            var table = TableOf(model); // Назначенный получатель урока.
            foreach (var parameter in model.Parameters())
                parameter.Value.RequiresGrad = ReferenceEquals(parameter, table); // Всё остальное заморожено.
        }

        var ablationModel = BuildModel(); // Модель плеча назначенной абляции.
        var nativeModel = BuildModel(); // Модель плеча родного обратного прохода.
        var referenceModel = BuildModel(); // Неизменяемая эталонная копия исходного старта.
        var oracleModel = BuildModel(); // Отдельная копия для независимого оракула сложения.
        var differenceModel = BuildModel(); // Отдельная копия для контроля конечной разностью.
        foreach (var parameter in referenceModel.Parameters()) parameter.Value.RequiresGrad = false; // Эталон не учится.
        TrainOnlyTable(ablationModel); // В плече абляции обучаема только таблица вложений.
        TrainOnlyTable(nativeModel); // В плече родного прохода обучаема только таблица вложений.
        TrainOnlyTable(oracleModel); // Оракулу нужна обучаемая таблица, чтобы увидеть её градиент.
        TrainOnlyTable(differenceModel); // Контролю нужна та же обучаемая таблица.

        var ablationTable = TableOf(ablationModel); // Таблица плеча абляции.
        var nativeTable = TableOf(nativeModel); // Таблица плеча родного прохода.
        var startTable = ablationTable.Value.Data.ToArray(); // Снимок чисел таблицы на старте.
        TableCoordinates = startTable.Length; // Всего чисел в таблице вложений.
        if (!startTable.SequenceEqual(nativeTable.Value.Data)) // Оба плеча обязаны стартовать с одних чисел.
            throw new InvalidOperationException("EMBEDDING_REACHABILITY_ARMS_START_DIFFER");
        AblationArmTrainableParameters = ablationModel.Parameters().Count(p => p.Value.RequiresGrad); // Обучаемых в A.
        NativeArmTrainableParameters = nativeModel.Parameters().Count(p => p.Value.RequiresGrad); // Обучаемых в B.
        // Имя привязки берётся у самой модели, а не придумывается: продуктовый
        // список записей контрольной точки именует базовые массивы позиционно,
        // и таблица вложений регистрируется моделью первой.
        var checkpointEntries = nativeModel.GetCheckpointParameterEntries(); // Продуктовый список записей.
        TableBindingKey = checkpointEntries
            .Single(entry => ReferenceEquals(entry.Parameter, nativeTable)).Key; // Имя привязки самой таблицы.
        FirstCheckpointEntryKey = checkpointEntries[0].Key; // Имя самой первой записи того же списка.

        // Донорский оптимизатор обучения с учителем остаётся происхождением и НЕ импортируется:
        // паспорт первичного опыта требует новой эпохи оптимизатора с нулевыми моментами.
        var donorOptimizer = AdamWPortableState.FromBytes(
            File.ReadAllBytes(Path.Combine(StartBundleDirectory, "S0.optimizer.bin")))
            ?? throw new InvalidOperationException("EMBEDDING_REACHABILITY_DONOR_OPTIMIZER_UNREADABLE");
        if (donorOptimizer.Step != DonorOptimizerStep) // Паспорт назвал донорский счётчик шага 400.
            throw new InvalidOperationException("EMBEDDING_REACHABILITY_DONOR_STEP:" + donorOptimizer.Step);

        // ── 6. Единственный фактор и прямое доказательство того, что он установлен ──
        var nativeAblation = EmbeddingReachabilityAblation.KeepNativeLink(); // Плечо B остаётся продуктовым.
        var ablationRecord = EmbeddingReachabilityAblation.BreakOutputToTableLink(ablationModel, ablationTable);
        AblationArmViewSharesStorage = ablationRecord.LayerViewSharesStorage; // Вид делит то же хранилище чисел.
        AblationArmTableIsOptimizerRecipient = ablationRecord.TableIsOptimizerRecipient; // Таблица осталась получателем.
        if (!nativeAblation.OutputLinkedToTable) // Плечо B обязано сохранить родную связь.
            throw new InvalidOperationException("EMBEDDING_REACHABILITY_NATIVE_ARM_DECLARED_BROKEN");

        var lessonTokens = new[]
        {
            Ids(donorRows[0].Prompt + donorRows[0].Answer), // Первый урок целиком: запрос и ответ подряд.
            Ids(donorRows[1].Prompt + donorRows[1].Answer) // Второй урок целиком.
        };
        var firstLessonInputs = lessonTokens[0][..^1]; // Входы первого урока: всё, кроме последнего знака.
        var firstLessonTargets = lessonTokens[0][1..]; // Цели первого урока: всё, кроме первого знака.
        var secondLessonInputs = lessonTokens[1][..^1]; // Входы второго урока.
        AblationArmOutputLinkedToTable =
            EmbeddingReachabilityAblation.OutputIsLinkedToTable(ablationModel, firstLessonInputs); // Проверка плеча A.
        LayerForwards++; // Это один прямой проход только слоя вложений.
        NativeArmOutputLinkedToTable =
            EmbeddingReachabilityAblation.OutputIsLinkedToTable(nativeModel, firstLessonInputs); // Проверка плеча B.
        LayerForwards++; // И ещё один такой же прямой проход.

        ReachableRows = firstLessonInputs.Concat(secondLessonInputs).Distinct().OrderBy(id => id).ToArray();
        FirstLessonRepeatedRowCount = firstLessonInputs
            .GroupBy(id => id).Count(group => group.Count() > 1); // Строк первого урока с повторным вкладом.

        // ── 7. Панель ДО обучения: по четыре чтения на каждое плечо ────────────────
        EmbeddingPanelRow ReadRow(LlmModel model, string arm, string phase, string id, string prompt, string kind)
        {
            var generator = new TextGenerator(model, tokenizer); // Продуктовый порождатель на текущем состоянии.
            var produced = generator.GenerateWithFirstDecision(
                prompt, ReadoutMaxNewTokens, ReadoutTemperature, ReadoutTopK, ReadoutTopP,
                ReadoutSeed, null, CancellationToken.None); // Тот же продуктовый путь выдачи плюс счётчики проходов.
            Generations++; // Одна жадная выдача.
            GenerationForwards += produced.PrefillForwardCalls + produced.DecodeForwardCalls; // Её фактическая цена.

            var promptIds = Ids(prompt); // Номера знаков запроса.
            var scored = model.Forward(
                new Tensor(promptIds.Select(value => (float)value).ToArray(), new TensorShape(1, promptIds.Length)),
                training: false); // Отдельный оценивающий прямой проход на том же запросе.
            ScoringForwards++; // Один оценивающий проход.
            var vocabulary = scored.Shape.Dimensions[2]; // Ширина словаря в выходных числах.
            var lastPosition = promptIds.Length - 1; // Последняя позиция запроса решает следующий знак.
            var lastLogits = new float[vocabulary]; // Числа последней позиции.
            Array.Copy(scored.Data, lastPosition * vocabulary, lastLogits, 0, vocabulary); // Копия этих чисел.
            var logSoftmax = LogSoftmax(lastLogits); // Логарифмические вероятности всех знаков словаря.
            var gold = goldById[id]; // Верный знак этой строки по независимому эталону.
            return new EmbeddingPanelRow(
                arm, // Какое плечо читается.
                phase, // До обучения или после него.
                id, // Короткое имя строки панели.
                kind, // Учебная строка или экзаменационная.
                gold, // Верный знак, посчитанный из самого запроса.
                produced.Receipt.Text, // Жадный ответ, расшифрованный настоящим разбивателем текста.
                string.Equals(produced.Receipt.Text, gold, StringComparison.Ordinal), // Совпал ли ответ с верным.
                logSoftmax[PlusTokenId], // Уверенность в знаке «плюс».
                logSoftmax[MinusTokenId], // Уверенность в знаке «минус».
                produced.PrefillForwardCalls, // Сколько прямых проходов ушло на чтение запроса.
                produced.DecodeForwardCalls); // Сколько прямых проходов ушло на продолжение.
        }

        var beforeRows = new List<EmbeddingPanelRow>(8); // Восемь чтений панели до обучения.
        foreach (var row in panel) beforeRows.Add(ReadRow(ablationModel, AblationArm, PhaseBefore, row.Id, row.Prompt, row.Kind));
        foreach (var row in panel) beforeRows.Add(ReadRow(nativeModel, NativeArm, PhaseBefore, row.Id, row.Prompt, row.Kind));
        PanelBefore = beforeRows; // Панель до обучения доступна проверкам.

        AblationArmFrozenBeforeSha256 = ParametersSha256(
            ablationModel.Parameters().Where(p => !p.Value.RequiresGrad)); // Замороженная основа плеча A до уроков.
        NativeArmFrozenBeforeSha256 = ParametersSha256(
            nativeModel.Parameters().Where(p => !p.Value.RequiresGrad)); // Замороженная основа плеча B до уроков.
        ReferenceBeforeSha256 = ParametersSha256(referenceModel.Parameters()); // Эталонная модель до уроков.

        // ── 8. Обучение: по четыре урока на плечо в порядке «первый, второй, …» ────
        var lessonOrder = new[] { 0, 1, 0, 1 }; // Порядок уроков: первый, второй, первый, второй.
        var lessonIds = new[] { donorRows[0].Id, donorRows[1].Id }; // Имена учебных строк.
        var lessons = new List<EmbeddingLessonRow>(PlannedOptimizerCalls); // Записи всех восьми уроков.

        EmbeddingLessonRow RunLesson(
            string arm, int index, LlmModel model, Parameter table, Trainer trainer, AdamW optimizer, int lesson)
        {
            var before = table.Value.Data.ToArray(); // Числа таблицы до урока.
            TrainForwards++; // Счётчик берётся на входе: отказ шага обязан остаться видимым в дозе.
            TrainBackwards++; // Тот же порядок для обратного прохода.
            OptimizerCalls++; // Обращение к оптимизатору считается по вызову, а не по успешному возврату.
            var loss = trainer.TrainSupervisedNextTokenStep(model, optimizer, lessonTokens[lesson], 1f); // Настоящий шаг.
            var delta = SummarizeDelta(before, table.Value.Data, config.HiddenDim); // Что урок сделал с таблицей.
            return new EmbeddingLessonRow(
                arm, index, lessonIds[lesson], loss, delta.ChangedCoordinates, delta.ChangedRowCount,
                scheduler.GetLr(index - 1)); // Скорость, назначенная расписанием этому шагу.
        }

        var ablationOptimizer = new AdamW(
            optimizerBlock.GetProperty("beta1").GetSingle(),
            optimizerBlock.GetProperty("beta2").GetSingle(),
            optimizerBlock.GetProperty("weightDecay").GetSingle(),
            optimizerBlock.GetProperty("eps").GetSingle()); // Новая эпоха оптимизатора плеча A.
        var nativeOptimizer = new AdamW(
            optimizerBlock.GetProperty("beta1").GetSingle(),
            optimizerBlock.GetProperty("beta2").GetSingle(),
            optimizerBlock.GetProperty("weightDecay").GetSingle(),
            optimizerBlock.GetProperty("eps").GetSingle()); // Новая эпоха оптимизатора плеча B.
        var ablationBindings = new[] { new AdamWParameterBinding(TableBindingKey, ablationTable) }; // Привязка плеча A.
        var nativeBindings = new[] { new AdamWParameterBinding(TableBindingKey, nativeTable) }; // Привязка плеча B.
        var freshAblation = ablationOptimizer.ExportPortableState(ablationBindings); // Состояние A до первого урока.
        var freshNative = nativeOptimizer.ExportPortableState(nativeBindings); // Состояние B до первого урока.
        if (freshAblation.Step != 0 || freshAblation.Entries.Count != 0
            || freshNative.Step != 0 || freshNative.Entries.Count != 0)
            throw new InvalidOperationException("EMBEDDING_REACHABILITY_OPTIMIZER_NOT_FRESH"); // Моменты обязаны быть нулевыми.

        var ablationTrainer = new Trainer(trainingConfig); // Свой счётчик шага расписания у плеча A.
        var nativeTrainer = new Trainer(trainingConfig); // Свой счётчик шага расписания у плеча B.
        for (var index = 0; index < lessonOrder.Length; index++) // Четыре урока плеча назначенной абляции.
            lessons.Add(RunLesson(AblationArm, index + 1, ablationModel, ablationTable, ablationTrainer, ablationOptimizer, lessonOrder[index]));
        for (var index = 0; index < lessonOrder.Length; index++) // Четыре урока плеча родного обратного прохода.
            lessons.Add(RunLesson(NativeArm, index + 1, nativeModel, nativeTable, nativeTrainer, nativeOptimizer, lessonOrder[index]));
        Lessons = lessons; // Записи уроков доступны проверкам.

        AblationArmDelta = SummarizeDelta(startTable, ablationTable.Value.Data, config.HiddenDim); // Итог плеча A.
        NativeArmDelta = SummarizeDelta(startTable, nativeTable.Value.Data, config.HiddenDim); // Итог плеча B.
        var ablationState = ablationOptimizer.ExportPortableState(ablationBindings); // Состояние A после уроков.
        var nativeState = nativeOptimizer.ExportPortableState(nativeBindings); // Состояние B после уроков.
        AblationArmOptimizerStep = ablationState.Step; // Счётчик шага оптимизатора плеча A.
        AblationArmMomentEntries = ablationState.Entries.Count; // Сколько записей моментов завёл оптимизатор A.
        NativeArmOptimizerStep = nativeState.Step; // Счётчик шага оптимизатора плеча B.
        NativeArmMomentEntries = nativeState.Entries.Count; // Сколько записей моментов завёл оптимизатор B.

        // ── 9. Панель ПОСЛЕ обучения: те же четыре чтения на каждое плечо ──────────
        var afterRows = new List<EmbeddingPanelRow>(8); // Восемь чтений панели после обучения.
        foreach (var row in panel) afterRows.Add(ReadRow(ablationModel, AblationArm, PhaseAfter, row.Id, row.Prompt, row.Kind));
        foreach (var row in panel) afterRows.Add(ReadRow(nativeModel, NativeArm, PhaseAfter, row.Id, row.Prompt, row.Kind));
        PanelAfter = afterRows; // Панель после обучения доступна проверкам.

        // ── 10. Независимый оракул сложения по строкам таблицы ─────────────────────
        // Оракул придумывает верхний градиент сам по закреплённому правилу и сам
        // складывает ожидаемый результат прямым перебором позиций. Ни одно число
        // не берётся у проверяемого кода.
        var oracleTable = TableOf(oracleModel); // Таблица отдельной копии для оракула.
        oracleTable.ZeroGrad(); // Прежних накоплений быть не должно.
        var oracleEmbedding = NativeAccess.TokenEmbedding(oracleModel); // Настоящий продуктовый слой вложений.
        var oracleOutput = oracleEmbedding.Forward(new Tensor(
            firstLessonInputs.Select(id => (float)id).ToArray(),
            new TensorShape(1, firstLessonInputs.Length))); // Настоящий прямой проход слоя вложений.
        LayerForwards++; // Ещё один прямой проход только слоя вложений.
        var upstream = ScatterAddOracle.Upstream(firstLessonInputs.Length, config.HiddenDim); // Верхний градиент оракула.
        oracleOutput.Backward(new Tensor(upstream, oracleOutput.Shape)); // Настоящий обратный проход слоя вложений.
        LayerBackwards++; // Один обратный проход только слоя вложений.
        var oracleActual = oracleTable.Grad is null
            ? new float[startTable.Length]
            : oracleTable.Grad.Data.ToArray(); // Фактический градиент таблицы от продукта.
        var oracleExpected = ScatterAddOracle.Expected(
            firstLessonInputs, upstream, config.VocabSize, config.HiddenDim); // Независимое ожидание оракула.
        var oracle = ScatterAddOracle.Compare(
            oracleExpected, oracleActual, firstLessonInputs, config.VocabSize, config.HiddenDim,
            OracleAbsoluteTolerance, OracleRelativeTolerance, NearZeroThreshold); // Сверка с объявленным допуском.
        OracleComparedCoordinates = oracle.ComparedCoordinates; // Сколько чисел сравнено.
        OracleMismatchedCoordinates = oracle.MismatchedCoordinates; // Сколько чисел вышло за допуск.
        OracleMaxAbsoluteDifference = oracle.MaxAbsoluteDifference; // Наибольшая абсолютная разница.
        OracleExpectedNonZeroRows = oracle.ExpectedNonZeroRows; // Сколько строк ожидалось ненулевыми.
        OracleActualNonZeroRows = oracle.ActualNonZeroRows; // Сколько строк вышло ненулевыми.
        OracleRepeatedIdRows = oracle.RepeatedIdRows; // Сколько строк получили повторный вклад.
        OracleAbsentRowsAreExactlyZero = oracle.AbsentRowsAreExactlyZero; // Отсутствующие строки ровно нулевые.
        OracleAgrees = oracle.Agrees; // Общий вердикт оракула.

        // ── 11. Независимый контроль «в лоб»: центральная конечная разность ────────
        var differenceTable = TableOf(differenceModel); // Таблица отдельной копии для контроля.
        var analytic = ProductLossSeam.AnalyticTableGradient(
            differenceModel, differenceTable, firstLessonInputs, firstLessonTargets); // Аналитический градиент.
        ControlModelForwards++; // Один прямой проход модели.
        ControlModelBackwards++; // Один обратный проход модели.
        var busiestRow = firstLessonInputs.GroupBy(id => id)
            .OrderByDescending(group => group.Count()).ThenBy(group => group.Key).First().Key; // Самый частый знак урока.
        var firstRow = firstLessonInputs[0]; // Первый знак запроса: встречается один раз.
        var digitRow = Ids("9").Single(); // Цифра «9» из тела запроса.
        var probes = new (int Row, int Column, string Why)[]
        {
            (busiestRow, 0, "самый частый знак урока, столбец 0"),
            (busiestRow, 31, "самый частый знак урока, столбец 31"),
            (firstRow, 0, "первый знак запроса, столбец 0"),
            (firstRow, 31, "первый знак запроса, столбец 31"),
            (MinusTokenId, 0, "знак «-» внутри запроса, столбец 0"),
            (MinusTokenId, 31, "знак «-» внутри запроса, столбец 31"),
            (digitRow, 0, "цифра «9» внутри запроса, столбец 0"),
            (digitRow, 31, "цифра «9» внутри запроса, столбец 31"),
            (PlusTokenId, 0, "знак «+» во входах отсутствует: ожидается ровно ноль")
        }; // Девять заранее закреплённых координат; по результату они не подбираются.
        if (probes.Select(probe => probe.Row * config.HiddenDim + probe.Column).Distinct().Count() != probes.Length)
            throw new InvalidOperationException("EMBEDDING_REACHABILITY_PROBE_COORDINATES_NOT_DISTINCT");

        var derivativeRows = new List<EmbeddingDerivativeRow>(probes.Length); // Записи контроля «в лоб».
        foreach (var probe in probes) // Каждая координата возмущается отдельно в плюс и в минус.
        {
            var index = probe.Row * config.HiddenDim + probe.Column; // Место числа в таблице.
            var original = differenceTable.Value.Data[index]; // Исходное значение числа.
            differenceTable.Value.Data[index] = (float)(original + FiniteDifferenceEpsilon); // Возмущение в плюс.
            var lossPlus = ProductLossSeam.Loss(differenceModel, firstLessonInputs, firstLessonTargets); // Потери «плюс».
            ControlModelForwards++; // Один прямой проход модели.
            differenceTable.Value.Data[index] = (float)(original - FiniteDifferenceEpsilon); // Возмущение в минус.
            var lossMinus = ProductLossSeam.Loss(differenceModel, firstLessonInputs, firstLessonTargets); // Потери «минус».
            ControlModelForwards++; // Ещё один прямой проход модели.
            differenceTable.Value.Data[index] = original; // Число возвращается ровно на место.

            var numeric = (lossPlus - lossMinus) / (2d * FiniteDifferenceEpsilon); // Производная «в лоб».
            var analyticValue = (double)analytic[index]; // Производная, снятая обратным проходом.
            var absolute = Math.Abs(numeric - analyticValue); // Абсолютное расхождение двух способов.
            var scale = Math.Abs(analyticValue); // Масштаб аналитического значения.
            var relative = scale >= NearZeroThreshold ? absolute / scale : 0d; // Относительное расхождение.
            var agrees = absolute <= FiniteDifferenceAbsoluteTolerance
                || (scale >= NearZeroThreshold && absolute <= FiniteDifferenceRelativeTolerance * scale);
            derivativeRows.Add(new EmbeddingDerivativeRow(
                probe.Row, probe.Column, probe.Why, analyticValue, numeric, absolute, relative, agrees));
        }

        FiniteDifferenceRows = derivativeRows; // Девять координат контроля доступны проверкам.

        // ── 12. Сторожа неизменности после всей подготовки ─────────────────────────
        AblationArmFrozenAfterSha256 = ParametersSha256(
            ablationModel.Parameters().Where(p => !p.Value.RequiresGrad)); // Замороженная основа A после всего.
        NativeArmFrozenAfterSha256 = ParametersSha256(
            nativeModel.Parameters().Where(p => !p.Value.RequiresGrad)); // Замороженная основа B после всего.
        ReferenceAfterSha256 = ParametersSha256(referenceModel.Parameters()); // Эталонная модель после всего.

        // ── 13. Сохранение вновь полученного состояния ровно один раз ──────────────
        // Сохраняется состояние плеча родного обратного прохода: именно оно
        // действительно обучилось. Плечо назначенной абляции — контроль с нулевым
        // изменением, сохранять его как обученный старт было бы неправдой.
        ModelForwardsBeforeSave = ModelForwards; // Счёт прямых проходов на момент перед сохранением.
        SavedStartDirectory = Path.Combine(
            RepositoryRoot(), "experiments-v2", "RP01-MICRO", "_shared-learned-starts", "p11-embedding-r29", "v1");
        SavedSupervisedStepsPerformed = LessonsPerArm; // Столько шагов обучения с учителем выполнило это плечо.
        var saved = SaveTrainedStartOnce(
            nativeModel, nativeOptimizer, nativeBindings, config, trainingConfig, topologySha, loraSha,
            lessonOrder.Select(lesson => lessonIds[lesson]).ToArray(), nativeState.Step,
            PanelAfter.Where(row => row.Arm == NativeArm).ToArray(), tokenizer.VocabSize);
        SavedStartFiles = saved.Files; // Состав сохранённого каталога.
        SavedStartManifestMatchesBytes = saved.ManifestMatchesBytes; // Опись совпала с фактическими байтами.
        SavedStartWrittenByThisRun = saved.WrittenByThisRun; // Записан ли каталог именно этим прогоном.
    }

    /// <summary>Итог однократного сохранения: состав, сверка описи и кто записал каталог.</summary>
    /// <param name="Files">Файлы сохранённого каталога с отпечатками.</param>
    /// <param name="ManifestMatchesBytes">Совпали ли отпечатки описи с фактическими байтами.</param>
    /// <param name="WrittenByThisRun">Записан ли каталог именно этим прогоном.</param>
    private sealed record SaveOutcome(
        IReadOnlyList<SavedStartFile> Files, bool ManifestMatchesBytes, bool WrittenByThisRun);

    /// <summary>
    /// Сохранить вновь полученное состояние один раз по назначенному планом пути.
    /// Повторный прогон байты не переписывает: он пересчитывает их в черновике и
    /// сверяет отпечатки, поэтому сохранение остаётся единственным. Обращений к
    /// модели здесь нет, поэтому доза прямых проходов не растёт.
    /// </summary>
    private SaveOutcome SaveTrainedStartOnce(
        LlmModel model,
        AdamW optimizer,
        IReadOnlyList<AdamWParameterBinding> bindings,
        ModelConfig config,
        TrainingConfig trainingConfig,
        string topologySha,
        string loraSha,
        IReadOnlyList<string> lessonOrder,
        int optimizerStepAfter,
        IReadOnlyList<EmbeddingPanelRow> panelAfter,
        int tokenizerVocabSize)
    {
        var tokenizerPath = Path.Combine(
            "experiments-v2", "RP01-MICRO", "P1101", "src", "byte-identity-tokenizer.json").Replace('\\', '/');
        var tokenizerSha = Sha256(File.ReadAllBytes(Path.Combine(RepositoryRoot(), tokenizerPath))); // Отпечаток словаря.
        var scratch = Path.Combine(Path.GetTempPath(), $"p11-embedding-r29-{Guid.NewGuid():N}"); // Черновая папка.
        Directory.CreateDirectory(scratch); // Байты сначала считаются в черновике, затем сверяются.
        try
        {
            // Веса полученного состояния в продуктовом формате контрольной точки.
            model.SaveCheckpoint(Path.Combine(scratch, "S1.weights.bin"));
            // Моменты оптимизатора и его счётчик шага в переносимом виде.
            File.WriteAllBytes(
                Path.Combine(scratch, "S1.optimizer.bin"), optimizer.ExportPortableState(bindings).ToBytes());
            // Снимок генератора случайных чисел с теми же опознавателями набора.
            WriteText(
                Path.Combine(scratch, "S1.rng.json"),
                JsonSerializer.Serialize(
                    model.CaptureRandomState(topologySha, loraSha, 0),
                    new JsonSerializerOptions { WriteIndented = true }));
            // Топология, словарь и численная настройка — чтобы состояние описывало само себя.
            WriteText(Path.Combine(scratch, "topology.json"), JsonSerializer.Serialize(new
            {
                topology_literal = TopologyLiteral, // Текст топологии, которым подписан снимок генератора.
                lora_literal = LoraLiteral, // Текст настройки адаптеров того же снимка.
                topology_sha256 = topologySha, // Отпечаток текста топологии.
                lora_sha256 = loraSha, // Отпечаток текста настройки адаптеров.
                model = new
                {
                    config.VocabSize, config.ContextLen, config.HiddenDim, config.NumLayers,
                    config.NumHeads, config.NumKvHeads, config.FfnHiddenDim, config.RopeTheta, config.RmsNormEps
                }, // Численная настройка модели этого состояния.
                lora = new { rank = 4, alpha = 8f, dropout = 0f, targets = new[] { "q_proj", "v_proj" } },
                base_seed = BaseSeed, // Зерно замороженной основы.
                adapter_seed = AdapterSeed, // Зерно адаптеров.
                adapter_stream = AdapterStream, // Поток случайных чисел адаптеров.
                tokenizer = new { path = tokenizerPath, sha256 = tokenizerSha, vocab_size = tokenizerVocabSize },
                training = new
                {
                    max_lr = trainingConfig.MaxLr, min_lr = trainingConfig.MinLr,
                    warmup_steps = trainingConfig.WarmupSteps, total_steps = trainingConfig.TotalSteps,
                    grad_clip_norm = trainingConfig.GradClipNorm, weight_decay = trainingConfig.WeightDecay
                } // Настройки обучения, при которых состояние получено.
            }, new JsonSerializerOptions { WriteIndented = true }));
            // Счётчик обучения с учителем записывается ОТДЕЛЬНЫМ файлом, как требует план.
            WriteText(Path.Combine(scratch, "supervised-step-counter.json"), JsonSerializer.Serialize(new
            {
                supervised_steps_performed = SavedSupervisedStepsPerformed, // Сколько уроков прошло это плечо.
                lesson_order = lessonOrder, // В каком порядке уроки предъявлялись.
                trainable_recipient = "таблица вложений модели; остальные массивы заморожены",
                optimizer_step_before = 0, // Новая эпоха оптимизатора начинается с нуля.
                optimizer_step_after = optimizerStepAfter, // Счётчик шага оптимизатора после уроков.
                counter_note =
                    "число выполненных шагов дообучения записано как метаданные; продуктового способа "
                    + "восстановить приватный счётчик расписания обучателя сейчас нет, поэтому называть "
                    + "этот снимок полным продолжением обучателя нельзя"
            }, new JsonSerializerOptions { WriteIndented = true }));

            var payload = new[] { "S1.weights.bin", "S1.optimizer.bin", "S1.rng.json", "topology.json", "supervised-step-counter.json" };
            // Проверяемая опись: имя, отпечаток и размер каждого файла состояния.
            WriteText(Path.Combine(scratch, "manifest.json"), JsonSerializer.Serialize(new
            {
                origin_bundle = "experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1",
                origin_weights_sha256 = StartWeightsSha256, // Отпечаток весов исходного старта.
                origin_optimizer_step = DonorOptimizerStep, // Донорский счётчик шага: происхождение, не доза.
                produced_by = "Tests/ExperimentFixes/EmbeddingLearningReachabilityTests.cs",
                produced_arm = "родной обратный проход слоя вложений",
                control_arm_note =
                    "второе плечо — объявленный необучаемый вид того же хранилища; оно преднамеренно "
                    + "не изменило ни одного числа таблицы и как обученный старт не сохраняется",
                lesson_order = lessonOrder, // Порядок уроков, давших это состояние.
                supervised_steps_performed = SavedSupervisedStepsPerformed, // Столько шагов обучения с учителем.
                evidence_note =
                    "два верных ответа из четырёх — сохранение, а не прирост точности: ровно столько же "
                    + "даёт тривиальный постоянный ответ знаком «плюс»",
                panel_after = panelAfter.Select(row => new
                {
                    id = row.Id, gold = row.Gold, answer = row.Answer, correct = row.Correct
                }).ToArray(), // Ответы панели этого плеча после уроков.
                files = payload.Select(name => new
                {
                    name, // Имя файла состояния.
                    sha256 = Sha256(File.ReadAllBytes(Path.Combine(scratch, name))), // Его отпечаток.
                    bytes = new FileInfo(Path.Combine(scratch, name)).Length // Его размер в байтах.
                }).ToArray()
            }, new JsonSerializerOptions { WriteIndented = true }));

            var names = payload.Append("manifest.json").ToArray(); // Полный состав сохраняемого каталога.
            var written = false; // Записан ли каталог именно этим прогоном.

            // Вердикт опубликованности обученного старта берётся у хранилища.
            //
            // Почему прежней проверки «каталога ещё нет» недостаточно. Она держится только
            // пока каталог лежит на диске: отслеживаемый хранилищем каталог можно удалить
            // локально, и тогда прогон запишет в ОПУБЛИКОВАННЫЙ путь другие байты.
            // Опубликованный обученный старт не перезаписывается и не воссоздаётся.
            var publishedStart = ExperimentFixes.ExperimentEvidenceSink.IsPublishedEvidenceDirectory(
                RepositoryRoot(), SavedStartDirectory, out _);

            // Первое и единственное сохранение: только когда каталога нет И он не опубликован.
            if (!Directory.Exists(SavedStartDirectory) && !publishedStart)
            {
                Directory.CreateDirectory(SavedStartDirectory); // Назначенный планом каталог создаётся один раз.
                foreach (var name in names)
                    File.Copy(Path.Combine(scratch, name), Path.Combine(SavedStartDirectory, name));
                written = true; // Байты записаны этим прогоном.
            }

            // Независимо от того, писали ли сейчас, сохранённые байты обязаны совпасть с пересчитанными.
            var files = new List<SavedStartFile>(names.Length); // Состав сохранённого каталога.
            var matches = true; // Пока ни одного расхождения не найдено.
            foreach (var name in names)
            {
                var savedPath = Path.Combine(SavedStartDirectory, name); // Путь сохранённого файла.
                if (!File.Exists(savedPath)) // Недостающий файл состояния — это неполный payload.
                {
                    matches = false; // Расхождение зафиксировано.
                    continue; // Остальные файлы всё равно описываются.
                }

                var savedBytes = File.ReadAllBytes(savedPath); // Фактические байты сохранённого файла.
                files.Add(new SavedStartFile(name, Sha256(savedBytes), savedBytes.LongLength)); // Строка описи.
                if (Sha256(savedBytes) != Sha256(File.ReadAllBytes(Path.Combine(scratch, name))))
                    matches = false; // Сохранённые байты разошлись с пересчитанными.
            }

            // Опись внутри каталога обязана называть те же отпечатки, что у фактических байтов.
            using var manifest = JsonDocument.Parse(
                File.ReadAllBytes(Path.Combine(SavedStartDirectory, "manifest.json")));
            foreach (var entry in manifest.RootElement.GetProperty("files").EnumerateArray())
            {
                var name = entry.GetProperty("name").GetString()!; // Имя файла по описи.
                var declared = entry.GetProperty("sha256").GetString()!; // Отпечаток по описи.
                var actual = files.FirstOrDefault(file => file.Name == name)?.Sha256; // Фактический отпечаток.
                if (actual is null || actual != declared) matches = false; // Опись и байты обязаны совпасть.
            }

            return new SaveOutcome(files, matches, written); // Итог однократного сохранения.
        }
        finally
        {
            if (Directory.Exists(scratch)) Directory.Delete(scratch, recursive: true); // Черновик не остаётся.
        }
    }

    /// <summary>Итог изменения таблицы: сколько чисел и строк сдвинулось и насколько.</summary>
    /// <param name="before">Числа таблицы до изменения.</param>
    /// <param name="after">Числа той же таблицы после изменения.</param>
    /// <param name="hiddenDim">Ширина одной строки таблицы.</param>
    private static EmbeddingTableDelta SummarizeDelta(float[] before, IReadOnlyList<float> after, int hiddenDim)
    {
        var changedCoordinates = 0; // Сколько чисел изменилось.
        var sumOfSquares = 0d; // Сумма квадратов изменений.
        var maxAbsolute = 0d; // Наибольшее по модулю изменение.
        var changedRows = new SortedSet<int>(); // Какие строки изменились.
        for (var position = 0; position < before.Length; position++)
        {
            var delta = (double)after[position] - before[position]; // Изменение одного числа.
            if (delta != 0d)
            {
                changedCoordinates++; // Это число изменилось.
                changedRows.Add(position / hiddenDim); // Строка этого числа изменилась.
            }

            sumOfSquares += delta * delta; // Накопление длины вектора изменения.
            maxAbsolute = Math.Max(maxAbsolute, Math.Abs(delta)); // Худшее изменение одного числа.
        }

        return new EmbeddingTableDelta(
            before.Length, changedCoordinates, changedRows.Count, Math.Sqrt(sumOfSquares), maxAbsolute,
            changedRows.ToArray());
    }

    /// <summary>Верный знак считается из самого запроса обычной целочисленной разностью.</summary>
    /// <param name="prompt">Запрос вида «C# result sign: 9-4 =».</param>
    private static string GoldFromPrompt(string prompt)
    {
        const string head = "C# result sign: "; // Неизменная голова запроса этого профиля.
        const string tail = " ="; // Неизменный хвост запроса этого профиля.
        if (!prompt.StartsWith(head, StringComparison.Ordinal) || !prompt.EndsWith(tail, StringComparison.Ordinal))
            throw new InvalidOperationException("EMBEDDING_REACHABILITY_PROMPT_SHAPE:" + prompt); // Чужая форма запроса.
        var body = prompt[head.Length..^tail.Length]; // Тело запроса вида «9-4».
        var separator = body.IndexOf('-'); // Знак вычитания отделяет уменьшаемое от вычитаемого.
        if (separator <= 0)
            throw new InvalidOperationException("EMBEDDING_REACHABILITY_PROMPT_NO_MINUS:" + body); // Без вычитания эталона нет.
        var left = int.Parse(body[..separator], CultureInfo.InvariantCulture); // Уменьшаемое.
        var right = int.Parse(body[(separator + 1)..], CultureInfo.InvariantCulture); // Вычитаемое.
        return left - right > 0 ? "+" : left - right < 0 ? "-" : "0"; // Донор записывал знак одним символом.
    }

    /// <summary>Логарифмические вероятности всех знаков словаря из уже полученных чисел.</summary>
    /// <param name="logits">Числа последней позиции запроса.</param>
    private static double[] LogSoftmax(IReadOnlyList<float> logits)
    {
        var maximum = logits.Select(value => (double)value).Max(); // Наибольшее число для устойчивости вычисления.
        var sum = logits.Sum(value => Math.Exp(value - maximum)); // Сумма сдвинутых показателей.
        var logSum = maximum + Math.Log(sum); // Логарифм нормирующей суммы.
        return logits.Select(value => value - logSum).ToArray(); // Логарифмическая вероятность каждого знака.
    }

    /// <summary>Отпечаток набора параметров: форма и все числа в объявленном порядке.</summary>
    /// <param name="parameters">Набор обучаемых или замороженных массивов.</param>
    private static string ParametersSha256(IEnumerable<Parameter> parameters)
    {
        using var stream = new MemoryStream(); // Буфер для форм и чисел.
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true); // Двоичная запись без преобразований.
        foreach (var parameter in parameters)
        {
            writer.Write(parameter.Value.Shape.Rank); // Число осей массива.
            foreach (var dimension in parameter.Value.Shape.Dimensions) writer.Write(dimension); // Длина каждой оси.
            foreach (var value in parameter.Value.Data) writer.Write(value); // Все числа массива.
        }

        writer.Flush(); // Буфер дописывается до конца перед отпечатком.
        return Sha256(stream.ToArray()); // Отпечаток собранных байтов.
    }

    /// <summary>Отпечаток байтов в человеко-сравнимом виде.</summary>
    /// <param name="bytes">Байты, у которых берётся отпечаток.</param>
    private static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    /// <summary>Отпечаток строки: им подписан снимок генератора случайных чисел набора.</summary>
    /// <param name="text">Строка, у которой берётся отпечаток.</param>
    private static string Sha256Text(string text) => Sha256(new UTF8Encoding(false).GetBytes(text));

    /// <summary>Запись текста по правилам хранилища: без метки порядка байтов, LF, без хвостовых пробелов.</summary>
    /// <param name="path">Куда писать.</param>
    /// <param name="text">Что писать.</param>
    private static void WriteText(string path, string text)
    {
        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n'); // Переводы строк приводятся к LF.
        var lines = normalized.Split('\n').Select(line => line.TrimEnd(' ', '\t')); // Хвостовые пробелы срезаются.
        var payload = string.Join("\n", lines).TrimEnd('\n') + "\n"; // Ровно один перевод строки в конце.
        File.WriteAllText(path, payload, new UTF8Encoding(false)); // Метка порядка байтов не пишется.
    }

    /// <summary>Путь проверенного обученного старта, из которого начинался опыт.</summary>
    public static string SharedLearnedStartDirectory() => Path.Combine(
        RepositoryRoot(), "experiments-v2", "RP01-MICRO", "_shared-learned-starts", "h64-rms-row", "v1");

    /// <summary>Папка сохранённых доказательств первичного одиннадцатого опыта.</summary>
    public static string SavedAttemptDirectory() => Path.Combine(
        RepositoryRoot(), "experiments-v2", "RP01-MICRO", "P1101", "results", "attempt-0001");

    /// <summary>Подъём от папки сборки до корня хранилища: пути не зависят от места запуска.</summary>
    private static string RepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory); // Папка собранных проверок.
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln"))) return current.FullName; // Корень найден.
            current = current.Parent; // Иначе поднимаемся ещё на уровень вверх.
        }

        throw new DirectoryNotFoundException("EMBEDDING_REACHABILITY_REPOSITORY_ROOT_NOT_FOUND"); // Корень не найден.
    }

    /// <summary>Общая подготовка временных каталогов за собой не оставляет.</summary>
    public void Dispose()
    {
        // Черновая папка сохранения удаляется сразу после записи, а назначенный планом
        // каталог состояния остаётся продуктовым результатом и не удаляется.
    }
}

/// <summary>
/// Проверки исправного родного слоя вложений: установленный фактор, фактическая
/// достижимость обучения, независимый оракул сложения, независимая конечная
/// разность, удержание по каждой строке панели, целостность замороженного и
/// эталонного состояния, исполненная доза и сохранённое состояние.
/// </summary>
public sealed class EmbeddingLearningReachabilityTests
    : IClassFixture<EmbeddingLearningReachabilityFixture>
{
    /// <summary>Общая подготовка: исходная программа опыта, выполненная ровно один раз.</summary>
    private readonly EmbeddingLearningReachabilityFixture _measured;

    /// <summary>Канал человеческих показаний проверки.</summary>
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    /// <summary>Принять общую подготовку класса и канал показаний.</summary>
    /// <param name="measured">Общая подготовка, выполненная ровно один раз.</param>
    /// <param name="output">Канал человеческих показаний проверки.</param>
    public EmbeddingLearningReachabilityTests(
        EmbeddingLearningReachabilityFixture measured, Xunit.Abstractions.ITestOutputHelper output)
    {
        _measured = measured; // Готовые измерения общей подготовки.
        _output = output; // Канал, в который пишутся человеческие показания.
    }

    // ── Личность входов ──────────────────────────────────────────────────────────

    /// <summary>
    /// Подготовка началась именно с проверенного обученного старта: все входы
    /// совпали с отпечатками, записанными описью набора, а веса совпали ещё и с
    /// отпечатком, закреплённым паспортом опыта.
    /// </summary>
    [Fact]
    public void StartBundleIdentityMatchesTheSealedManifest()
    {
        Assert.True(_measured.StartInputsMatchManifest, "EMBEDDING_REACHABILITY_START_INPUTS_DIFFER_FROM_MANIFEST");
        Assert.Equal(7, _measured.StartInputs.Count); // Семь входов старта, названных паспортом.
        Assert.Equal(
            EmbeddingLearningReachabilityFixture.StartWeightsSha256,
            _measured.StartInputs.Single(file => file.Name == "S0.weights.bin").Sha256); // Тот самый обученный старт.
        Assert.Equal(320620L, _measured.StartInputs.Single(file => file.Name == "S0.weights.bin").Bytes);

        _output.WriteLine("Старт: " + string.Join("; ", _measured.StartInputs.Select(file =>
            $"{file.Name} {file.Sha256[..12]}… ({file.Bytes} байт)")));
    }

    // ── Единственный фактор действительно установлен ─────────────────────────────

    /// <summary>
    /// Между плечами отличается ровно одно: в плече абляции слой вложений видит
    /// объявленный необучаемый вид того же самого хранилища чисел, поэтому его
    /// выход создаётся без связи с родительской таблицей. Сама таблица при этом
    /// остаётся обучаемым получателем оптимизатора, а обучаемый массив в обоих
    /// плечах ровно один.
    /// </summary>
    [Fact]
    public void SingleFactorIsInstalled_AblationArmSeesNonTrainableViewOfTheSameStorage()
    {
        Assert.False(_measured.AblationArmOutputLinkedToTable); // В плече абляции связи с таблицей нет.
        Assert.True(_measured.NativeArmOutputLinkedToTable); // В плече родного прохода связь есть.
        Assert.True(_measured.AblationArmViewSharesStorage); // Вид делит ТО ЖЕ хранилище чисел, а не копию.
        Assert.True(_measured.AblationArmTableIsOptimizerRecipient); // Таблица осталась получателем оптимизатора.

        Assert.Equal(1, _measured.AblationArmTrainableParameters); // Обучаемый массив ровно один.
        Assert.Equal(1, _measured.NativeArmTrainableParameters); // И во втором плече тоже один.

        // Получатель урока — именно таблица вложений, и это проверяется структурно:
        // модель регистрирует её самым первым базовым массивом, а продуктовый
        // список записей контрольной точки именует базовые массивы позиционно,
        // то есть «base.» плюс шестизначный номер. Рукописная метка хозяина
        // опыта `base.token_embedding.weight` именем продукта никогда не была.
        Assert.Equal(_measured.FirstCheckpointEntryKey, _measured.TableBindingKey); // Таблица — первая запись.
        Assert.Equal("base.000000", _measured.TableBindingKey); // Позиционное имя нулевой базовой записи.
        Assert.Equal(16384, _measured.TableCoordinates); // 256 строк словаря на 64 числа строки.

        // Уроки достигают не всей таблицы: строка знака «плюс» во входах отсутствует.
        // Оба запроса собраны из одного набора байтов «C# result sign: », цифр и
        // знаков, поэтому достижимых строк семнадцать из двухсот пятидесяти шести.
        Assert.Equal(17, _measured.ReachableRows.Count); // Семнадцать достижимых строк.
        Assert.DoesNotContain(_measured.PlusTokenId, _measured.ReachableRows); // Строка знака «плюс» недостижима.
        Assert.Contains(_measured.MinusTokenId, _measured.ReachableRows); // Строка знака «минус» достижима.

        // Первый урок предъявляет двадцать один байт на семнадцати разных строках,
        // поэтому ровно две строки получают больше одного вклада: пробел встречается
        // четыре раза, буква «s» — дважды. Без таких строк сторож сложения вкладов
        // не проверял бы ничего.
        Assert.Equal(2, _measured.FirstLessonRepeatedRowCount); // Две строки с повторным вкладом.

        _output.WriteLine(
            $"Фактор установлен: связь с таблицей в плече абляции {_measured.AblationArmOutputLinkedToTable}, "
            + $"в плече родного прохода {_measured.NativeArmOutputLinkedToTable}; "
            + $"достижимых строк {_measured.ReachableRows.Count}, с повторным вкладом в первом уроке "
            + $"{_measured.FirstLessonRepeatedRowCount}.");
    }

    // ── Главное: обучение действительно доходит до таблицы вложений ──────────────

    /// <summary>
    /// Родной обратный проход доводит обучение до таблицы вложений: все четыре
    /// урока плеча меняют её числа, и меняются только достижимые уроками строки.
    /// Плечо назначенной абляции при той же дозе не меняет ни одного числа — это
    /// преднамеренный контроль, доказывающий, что результат второго плеча пришёл
    /// именно из проверяемой связи, а не из чего-то ещё.
    /// </summary>
    [Fact]
    public void NativeBackwardReachesTheEmbeddingTable_AndTheAblationArmStaysExactlyZero()
    {
        var ablationLessons = _measured.Lessons
            .Where(lesson => lesson.Arm == EmbeddingLearningReachabilityFixture.AblationArm).ToArray();
        var nativeLessons = _measured.Lessons
            .Where(lesson => lesson.Arm == EmbeddingLearningReachabilityFixture.NativeArm).ToArray();
        Assert.Equal(EmbeddingLearningReachabilityFixture.LessonsPerArm, ablationLessons.Length); // Четыре урока.
        Assert.Equal(EmbeddingLearningReachabilityFixture.LessonsPerArm, nativeLessons.Length); // И столько же.
        Assert.Equal(new[] { "T1", "T2", "T1", "T2" }, ablationLessons.Select(lesson => lesson.LessonId).ToArray());
        Assert.Equal(new[] { "T1", "T2", "T1", "T2" }, nativeLessons.Select(lesson => lesson.LessonId).ToArray());

        // Доза равна, а результат разный: ровно это и означает достижимость.
        Assert.All(ablationLessons, lesson => Assert.Equal(0, lesson.ChangedCoordinates)); // Абляция не сдвинула ничего.
        Assert.All(nativeLessons, lesson => Assert.True(lesson.ChangedCoordinates > 0,
            $"EMBEDDING_REACHABILITY_NATIVE_LESSON_DID_NOT_REACH_TABLE:{lesson.Index}"));
        Assert.Equal(
            EmbeddingLearningReachabilityFixture.PlannedEffectiveUpdatesAblationArm,
            ablationLessons.Count(lesson => lesson.ChangedCoordinates > 0)); // Действующих изменений в абляции ноль.
        Assert.Equal(
            EmbeddingLearningReachabilityFixture.PlannedEffectiveUpdatesNativeArm,
            nativeLessons.Count(lesson => lesson.ChangedCoordinates > 0)); // А в родном проходе их четыре.

        // Итог по всей таблице: ровно ноль против 1088 чисел в 17 строках.
        Assert.Equal(0, _measured.AblationArmDelta.ChangedCoordinates); // Ни одно число не сдвинулось.
        Assert.Equal(0, _measured.AblationArmDelta.ChangedRowCount); // И ни одна строка.
        Assert.Equal(0d, _measured.AblationArmDelta.MaxAbsoluteDelta); // Даже на последнем знакоместе.
        Assert.Equal(1088, _measured.NativeArmDelta.ChangedCoordinates); // Семнадцать строк по шестьдесят четыре числа.
        Assert.Equal(17, _measured.NativeArmDelta.ChangedRowCount); // Столько строк и было достижимо.
        Assert.Equal(_measured.ReachableRows, _measured.NativeArmDelta.ChangedRows); // Ровно достижимые строки.
        Assert.True(_measured.NativeArmDelta.MaxAbsoluteDelta > 0d); // Сдвиг действительно ненулевой.

        // Оптимизатор вызван одинаково, но моменты таблицы появились только там,
        // где до неё дошёл градиент.
        Assert.Equal(EmbeddingLearningReachabilityFixture.LessonsPerArm, _measured.AblationArmOptimizerStep);
        Assert.Equal(EmbeddingLearningReachabilityFixture.LessonsPerArm, _measured.NativeArmOptimizerStep);
        Assert.Equal(0, _measured.AblationArmMomentEntries); // Градиента не было — моментов нет.
        Assert.Equal(1, _measured.NativeArmMomentEntries); // Градиент был — момент таблицы появился.

        _output.WriteLine(
            $"Достижимость: плечо абляции изменило {_measured.AblationArmDelta.ChangedCoordinates} чисел, "
            + $"плечо родного прохода — {_measured.NativeArmDelta.ChangedCoordinates} в "
            + $"{_measured.NativeArmDelta.ChangedRowCount} строках; нулевая абляция — валидный контроль.");
    }

    // ── Независимый оракул сложения по строкам таблицы ───────────────────────────

    /// <summary>
    /// Вклады повторяющихся знаков складываются в одну строку таблицы, строки
    /// знаков, которых в уроке нет, остаются ровно нулевыми, и все числа совпали
    /// с независимым ожиданием, которое оракул посчитал сам.
    /// </summary>
    [Fact]
    public void ScatterAddOracle_SumsRepeatedTokensIntoOneRow_AndKeepsAbsentRowsExactlyZero()
    {
        Assert.Equal(_measured.TableCoordinates, _measured.OracleComparedCoordinates); // Сравнена вся таблица.
        Assert.Equal(0, _measured.OracleMismatchedCoordinates); // Ни одно число не вышло за допуск.
        Assert.Equal(0d, _measured.OracleMaxAbsoluteDifference); // Совпадение точное, а не «в пределах».
        Assert.True(_measured.OracleRepeatedIdRows > 0,
            "EMBEDDING_REACHABILITY_ORACLE_HAS_NO_REPEATED_ROWS"); // Без повторов сторож сложения ничего не проверяет.
        Assert.Equal(_measured.FirstLessonRepeatedRowCount, _measured.OracleRepeatedIdRows); // Та же совокупность.
        Assert.Equal(2, _measured.OracleRepeatedIdRows); // Пробел и буква «s» первого урока.
        Assert.Equal(_measured.OracleExpectedNonZeroRows, _measured.OracleActualNonZeroRows); // Состав строк совпал.
        Assert.Equal(_measured.ReachableRows.Count, _measured.OracleActualNonZeroRows); // И равен числу достижимых строк.
        Assert.True(_measured.OracleAbsentRowsAreExactlyZero); // Отсутствующие знаки не получили ни одного вклада.
        Assert.True(_measured.OracleAgrees); // Общий вердикт оракула.

        _output.WriteLine(
            $"Оракул сложения: расхождений {_measured.OracleMismatchedCoordinates} из "
            + $"{_measured.OracleComparedCoordinates}; строк с повторами {_measured.OracleRepeatedIdRows}; "
            + $"ненулевых строк {_measured.OracleActualNonZeroRows}.");
    }

    // ── Независимая конечная разность против аналитической производной ───────────

    /// <summary>
    /// Производная, снятая настоящим обратным проходом, совпадает с независимой
    /// центральной конечной разностью той же величины потерь на всех девяти
    /// заранее закреплённых координатах. Пороги согласия закреплены константами
    /// до запуска: шаг возмущения 0,01, относительный порог 0,15 либо абсолютный
    /// 2e-4. Координата знака «плюс» отсутствует во входах урока и обязана дать
    /// ровно ноль обоими способами.
    /// </summary>
    [Fact]
    public void FiniteDifferenceAgreesWithTheAnalyticGradient_OnNinePinnedCoordinates()
    {
        Assert.Equal(0.01d, EmbeddingLearningReachabilityFixture.FiniteDifferenceEpsilon); // Шаг возмущения закреплён.
        Assert.Equal(0.15d, EmbeddingLearningReachabilityFixture.FiniteDifferenceRelativeTolerance); // Порог закреплён.
        Assert.Equal(2e-4, EmbeddingLearningReachabilityFixture.FiniteDifferenceAbsoluteTolerance); // И второй порог.
        Assert.Equal(9, _measured.FiniteDifferenceRows.Count); // Девять координат, а не сколько получилось.

        foreach (var row in _measured.FiniteDifferenceRows)
        {
            // Погрешности пересчитываются независимо из исходных чисел строки,
            // а не принимаются из её же сохранённых полей.
            var recomputedAbsolute = Math.Abs(row.Analytic - row.FiniteDifference); // Абсолютная погрешность заново.
            Assert.Equal(recomputedAbsolute, row.AbsoluteError, 15); // Сохранённое поле сходится с пересчётом.
            var scale = Math.Abs(row.Analytic); // Масштаб аналитического значения.
            var recomputedRelative = scale >= EmbeddingLearningReachabilityFixture.NearZeroThreshold
                ? recomputedAbsolute / scale
                : 0d; // Относительная погрешность заново.
            Assert.Equal(recomputedRelative, row.RelativeError, 15); // И она тоже сходится с пересчётом.

            // Согласие проверяется по каждой координате отдельно, а не «хотя бы где-то».
            var agrees = recomputedAbsolute <= EmbeddingLearningReachabilityFixture.FiniteDifferenceAbsoluteTolerance
                || (scale >= EmbeddingLearningReachabilityFixture.NearZeroThreshold
                    && recomputedAbsolute <= EmbeddingLearningReachabilityFixture.FiniteDifferenceRelativeTolerance * scale);
            Assert.Equal(agrees, row.Agrees); // Правило согласия пересчитано из порогов, а не принято на веру.
            Assert.True(row.Agrees,
                $"EMBEDDING_REACHABILITY_DERIVATIVE_DISAGREES:строка {row.Row} столбец {row.Column}: "
                + $"аналитическая {row.Analytic:R}, конечная разность {row.FiniteDifference:R}, "
                + $"абсолютная погрешность {row.AbsoluteError:R}, относительная {row.RelativeError:R}");
        }

        // Координата знака «плюс» во входах урока отсутствует: ровно ноль обоими способами.
        var absentRow = _measured.FiniteDifferenceRows.Single(row => row.Row == _measured.PlusTokenId);
        Assert.Equal(0d, absentRow.Analytic); // Аналитическая производная ровно нулевая.
        Assert.Equal(0d, absentRow.FiniteDifference); // И конечная разность тоже.

        // На остальных восьми координатах совпали не только величины, но и направление.
        foreach (var row in _measured.FiniteDifferenceRows.Where(row => row.Row != _measured.PlusTokenId))
        {
            Assert.True(Math.Abs(row.Analytic) > EmbeddingLearningReachabilityFixture.NearZeroThreshold,
                $"EMBEDDING_REACHABILITY_DERIVATIVE_UNEXPECTEDLY_NEAR_ZERO:{row.Row}/{row.Column}");
            Assert.True(Math.Sign(row.Analytic) == Math.Sign(row.FiniteDifference),
                $"EMBEDDING_REACHABILITY_DERIVATIVE_SIGN_DIFFERS:{row.Row}/{row.Column}");
        }

        var worst = _measured.FiniteDifferenceRows.Max(row => row.RelativeError); // Худшее относительное расхождение.
        Assert.True(worst < EmbeddingLearningReachabilityFixture.FiniteDifferenceRelativeTolerance,
            "EMBEDDING_REACHABILITY_DERIVATIVE_WORST_RELATIVE_AT_THRESHOLD:" + worst.ToString("R", CultureInfo.InvariantCulture));

        _output.WriteLine("Конечная разность: " + string.Join("; ", _measured.FiniteDifferenceRows.Select(row =>
            $"[{row.Row}/{row.Column}] аналитическая {row.Analytic.ToString("E3", CultureInfo.InvariantCulture)}, "
            + $"в лоб {row.FiniteDifference.ToString("E3", CultureInfo.InvariantCulture)}, "
            + $"относительно {row.RelativeError.ToString("F4", CultureInfo.InvariantCulture)}")));
    }

    // ── Удержание по каждой строке панели ────────────────────────────────────────

    /// <summary>
    /// Панель читается построчно, а не одним числом «сколько верных». До обучения
    /// оба плеча отвечают одинаково: учебная строка T1 верна, T2 неверна, экзамен
    /// E1 неверен, экзамен E2 верен. После обучения плечо абляции повторяет эти
    /// ответы знак в знак, потому что таблица не менялась. Плечо родного прохода
    /// удерживает T1, не выучивает T2, исправляет E1 с «минуса» на «плюс» и при
    /// этом портит E2 с «минуса» на «плюс». Итог остаётся два верных из четырёх —
    /// ровно столько же даёт тривиальный постоянный ответ знаком «плюс», поэтому
    /// прироста качества здесь не заявляется.
    /// </summary>
    [Fact]
    public void PerRowRetention_HoldsT1_DoesNotLearnT2_FixesE1_BreaksE2_AtTheConstantPlusBaseline()
    {
        Assert.Equal(8, _measured.PanelBefore.Count); // Четыре строки на каждое плечо.
        Assert.Equal(8, _measured.PanelAfter.Count); // И столько же после обучения.

        EmbeddingPanelRow Row(IReadOnlyList<EmbeddingPanelRow> rows, string arm, string id)
            => rows.Single(row => row.Arm == arm && row.Id == id); // Одна строка панели по плечу и имени.

        var ablation = EmbeddingLearningReachabilityFixture.AblationArm; // Плечо назначенной абляции.
        var native = EmbeddingLearningReachabilityFixture.NativeArm; // Плечо родного обратного прохода.

        // До обучения оба плеча читаются одинаково: состояние у них одно и то же.
        foreach (var id in new[] { "T1", "T2", "E1", "E2" })
        {
            Assert.Equal(Row(_measured.PanelBefore, ablation, id).Answer, Row(_measured.PanelBefore, native, id).Answer);
            Assert.Equal(Row(_measured.PanelBefore, ablation, id).LogPlus, Row(_measured.PanelBefore, native, id).LogPlus);
            Assert.Equal(Row(_measured.PanelBefore, ablation, id).LogMinus, Row(_measured.PanelBefore, native, id).LogMinus);
        }

        // Исходное состояние панели: два верных ответа из четырёх.
        Assert.Equal("+", Row(_measured.PanelBefore, native, "T1").Answer); // Учебная строка уже верна.
        Assert.Equal("+", Row(_measured.PanelBefore, native, "T2").Answer); // Вторая учебная строка неверна.
        Assert.Equal("-", Row(_measured.PanelBefore, native, "E1").Answer); // Первый экзамен неверен.
        Assert.Equal("-", Row(_measured.PanelBefore, native, "E2").Answer); // Второй экзамен верен.
        Assert.Equal(2, _measured.PanelBefore.Count(row => row.Arm == native && row.Correct)); // Два из четырёх.

        // Плечо абляции после уроков отвечает ровно так же: нулевое изменение таблицы
        // не может изменить ни ответ, ни число уверенности.
        foreach (var id in new[] { "T1", "T2", "E1", "E2" })
        {
            Assert.Equal(Row(_measured.PanelBefore, ablation, id).Answer, Row(_measured.PanelAfter, ablation, id).Answer);
            Assert.Equal(Row(_measured.PanelBefore, ablation, id).LogPlus, Row(_measured.PanelAfter, ablation, id).LogPlus);
            Assert.Equal(Row(_measured.PanelBefore, ablation, id).LogMinus, Row(_measured.PanelAfter, ablation, id).LogMinus);
        }

        // Плечо родного прохода: удержание, невыученная строка, исправление и порча.
        Assert.Equal("+", Row(_measured.PanelAfter, native, "T1").Answer); // T1 удержан.
        Assert.True(Row(_measured.PanelAfter, native, "T1").Correct); // И остался верным.
        Assert.Equal("+", Row(_measured.PanelAfter, native, "T2").Answer); // T2 так и не выучен.
        Assert.False(Row(_measured.PanelAfter, native, "T2").Correct); // Его верный знак — «минус».
        Assert.Equal("+", Row(_measured.PanelAfter, native, "E1").Answer); // E1 исправлен с «минуса» на «плюс».
        Assert.True(Row(_measured.PanelAfter, native, "E1").Correct); // Теперь он верен.
        Assert.Equal("+", Row(_measured.PanelAfter, native, "E2").Answer); // E2 испорчен с «минуса» на «плюс».
        Assert.False(Row(_measured.PanelAfter, native, "E2").Correct); // И стал неверным.

        // Состояние действительно сдвинулось: числа уверенности изменились.
        Assert.NotEqual(Row(_measured.PanelBefore, native, "T1").LogPlus, Row(_measured.PanelAfter, native, "T1").LogPlus);

        // Итог тот же, что у тривиального постоянного ответа знаком «плюс».
        Assert.Equal(2, _measured.PanelAfter.Count(row => row.Arm == native && row.Correct)); // Два из четырёх.
        Assert.Equal(2, _measured.PanelAfter.Count(row => row.Arm == native && row.Gold == "+")); // Столько же «плюсов».
        Assert.Equal(4, _measured.PanelAfter.Count(row => row.Arm == native && row.Answer == "+")); // Отвечает всегда «плюс».

        _output.WriteLine("Панель после уроков (родной проход): " + string.Join("; ",
            _measured.PanelAfter.Where(row => row.Arm == native).Select(row =>
                $"{row.Id} ответ «{row.Answer}», верный «{row.Gold}», совпало {(row.Correct ? "да" : "нет")}")));
    }

    // ── Целостность замороженного и эталонного состояния ─────────────────────────

    /// <summary>
    /// Уроки изменили только разрешённый получатель: замороженная основа обоих
    /// плеч и неизменяемая эталонная модель остались байт в байт теми же после
    /// всей подготовки, включая обе контрольные фазы.
    /// </summary>
    [Fact]
    public void FrozenBaseAndReferenceModel_StayByteIdenticalThroughTheWholeFixture()
    {
        Assert.Equal(_measured.AblationArmFrozenBeforeSha256, _measured.AblationArmFrozenAfterSha256);
        Assert.Equal(_measured.NativeArmFrozenBeforeSha256, _measured.NativeArmFrozenAfterSha256);
        Assert.Equal(_measured.ReferenceBeforeSha256, _measured.ReferenceAfterSha256);
        Assert.Equal(_measured.AblationArmFrozenBeforeSha256, _measured.NativeArmFrozenBeforeSha256);
    }

    // ── Назначенная доза исполнена ровно ─────────────────────────────────────────

    /// <summary>
    /// Назначенная планом доза потрачена ровно: пятьдесят девять прямых проходов
    /// модели, девять обратных, три прямых прохода одного только слоя вложений,
    /// один его обратный проход, восемь обращений к оптимизатору при нуле
    /// действующих изменений в плече абляции и четырёх в плече родного прохода,
    /// шестнадцать жадных выдач и ни одного прогрева с нуля. Сохранение нового
    /// состояния не добавило ни одного прямого прохода.
    /// </summary>
    [Fact]
    public void AssignedDoseIsSpentExactly_AndSavingTheNewStartAddsNoForward()
    {
        Assert.Equal(EmbeddingLearningReachabilityFixture.PlannedModelForwards, _measured.ModelForwards);
        Assert.Equal(EmbeddingLearningReachabilityFixture.PlannedModelBackwards, _measured.ModelBackwards);
        Assert.Equal(EmbeddingLearningReachabilityFixture.PlannedLayerForwards, _measured.LayerForwards);
        Assert.Equal(EmbeddingLearningReachabilityFixture.PlannedLayerBackwards, _measured.LayerBackwards);
        Assert.Equal(EmbeddingLearningReachabilityFixture.PlannedOptimizerCalls, _measured.OptimizerCalls);
        Assert.Equal(EmbeddingLearningReachabilityFixture.PlannedGenerations, _measured.Generations);
        Assert.Equal(EmbeddingLearningReachabilityFixture.PlannedBootstrapRuns, _measured.BootstrapRuns);

        // Разложение дозы: восемь учебных, шестнадцать оценивающих, шестнадцать
        // от жадных выдач и девятнадцать контрольных прямых проходов дают ровно
        // пятьдесят девять; восемь учебных и один контрольный обратный дают девять.
        Assert.Equal(8, _measured.TrainForwards); // Учебные прямые проходы.
        Assert.Equal(8, _measured.TrainBackwards); // Учебные обратные проходы.
        Assert.Equal(16, _measured.ScoringForwards); // Отдельные оценивающие проходы панели.
        Assert.Equal(16, _measured.GenerationForwards); // Жадная выдача стоила ровно один проход каждая.
        Assert.Equal(19, _measured.ControlModelForwards); // Один для аналитического градиента и восемнадцать «в лоб».
        Assert.Equal(1, _measured.ControlModelBackwards); // Один обратный проход для аналитического градиента.
        Assert.Equal(
            _measured.TrainForwards + _measured.ScoringForwards + _measured.GenerationForwards + _measured.ControlModelForwards,
            _measured.ModelForwards); // Слагаемые складываются именно в объявленную сумму.

        // Цена каждой выдачи измерена продуктовыми счётчиками, а не принята на веру.
        Assert.All(_measured.PanelBefore, row => Assert.Equal(1, row.PrefillForwardCalls));
        Assert.All(_measured.PanelBefore, row => Assert.Equal(0, row.DecodeForwardCalls));
        Assert.All(_measured.PanelAfter, row => Assert.Equal(1, row.PrefillForwardCalls));
        Assert.All(_measured.PanelAfter, row => Assert.Equal(0, row.DecodeForwardCalls));

        // Сохранение состояния — запись файлов, а не новый прогон модели.
        Assert.Equal(_measured.ModelForwardsBeforeSave, _measured.ModelForwards);

        _output.WriteLine(
            $"Доза: прямых проходов модели {_measured.ModelForwards}, обратных {_measured.ModelBackwards}, "
            + $"прямых проходов слоя {_measured.LayerForwards}, обратных слоя {_measured.LayerBackwards}, "
            + $"обращений к оптимизатору {_measured.OptimizerCalls}, выдач {_measured.Generations}, "
            + $"прогревов с нуля {_measured.BootstrapRuns}.");
    }

    // ── Сохранённое состояние записано один раз и проверяемо ─────────────────────

    /// <summary>
    /// Вновь полученное состояние сохранено один раз по назначенному планом пути
    /// полным составом: веса, моменты оптимизатора, состояние генератора
    /// случайных чисел, топология со словарём и настройкой, отдельный счётчик
    /// обучения с учителем и проверяемая опись. Отпечатки описи совпали с
    /// фактическими байтами.
    /// </summary>
    [Fact]
    public void SavedTrainedStart_IsWrittenOnce_WithVerifiableManifest()
    {
        Assert.True(Directory.Exists(_measured.SavedStartDirectory),
            "EMBEDDING_REACHABILITY_SAVED_START_DIRECTORY_MISSING:" + _measured.SavedStartDirectory);
        Assert.EndsWith(Path.Combine("p11-embedding-r29", "v1"), _measured.SavedStartDirectory); // Путь назначен планом.

        var expected = new[]
        {
            "S1.weights.bin", "S1.optimizer.bin", "S1.rng.json",
            "topology.json", "supervised-step-counter.json", "manifest.json"
        }; // Полный состав сохранённого состояния.
        Assert.Equal(expected.OrderBy(name => name, StringComparer.Ordinal).ToArray(),
            _measured.SavedStartFiles.Select(file => file.Name).OrderBy(name => name, StringComparer.Ordinal).ToArray());
        Assert.All(_measured.SavedStartFiles, file => Assert.True(file.Bytes > 0,
            "EMBEDDING_REACHABILITY_SAVED_START_FILE_EMPTY:" + file.Name));
        Assert.True(_measured.SavedStartManifestMatchesBytes,
            "EMBEDDING_REACHABILITY_SAVED_START_MANIFEST_DIFFERS_FROM_BYTES");

        // Счётчик обучения с учителем записан отдельно и честно ограничен.
        using var counter = JsonDocument.Parse(
            File.ReadAllBytes(Path.Combine(_measured.SavedStartDirectory, "supervised-step-counter.json")));
        Assert.Equal(
            _measured.SavedSupervisedStepsPerformed,
            counter.RootElement.GetProperty("supervised_steps_performed").GetInt32());
        Assert.Equal(
            new[] { "T1", "T2", "T1", "T2" },
            counter.RootElement.GetProperty("lesson_order").EnumerateArray()
                .Select(value => value.GetString()).ToArray());

        // Опись называет исходный старт, из которого это состояние получено.
        using var manifest = JsonDocument.Parse(
            File.ReadAllBytes(Path.Combine(_measured.SavedStartDirectory, "manifest.json")));
        Assert.Equal(
            EmbeddingLearningReachabilityFixture.StartWeightsSha256,
            manifest.RootElement.GetProperty("origin_weights_sha256").GetString());
        Assert.Equal(
            EmbeddingLearningReachabilityFixture.DonorOptimizerStep,
            manifest.RootElement.GetProperty("origin_optimizer_step").GetInt32());

        _output.WriteLine($"Сохранено в {_measured.SavedStartDirectory} "
            + $"(записано этим прогоном: {(_measured.SavedStartWrittenByThisRun ? "да" : "нет, байты сверены")}): "
            + string.Join("; ", _measured.SavedStartFiles.Select(file => $"{file.Name} {file.Bytes} байт")));
    }

    // ── Связь с уже сохранёнными доказательствами опыта ──────────────────────────

    /// <summary>
    /// Живые числа этой подготовки воспроизводят уже сохранённые доказательства
    /// одиннадцатого опыта: величины потерь каждого урока, итоговое изменение
    /// таблицы, состав строк, результат оракула, обе производные на всех девяти
    /// координатах и ответы панели. Ожидания берутся из сохранённого файла, а не
    /// из того, что выдал новый код.
    /// </summary>
    [Fact]
    public void LiveRunReproducesSavedExperimentEvidence()
    {
        var attempt = EmbeddingLearningReachabilityFixture.SavedAttemptDirectory(); // Папка сохранённой попытки.
        using var arms = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(attempt, "arms.json"))); // Плечи и контроли.
        using var metrics = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(attempt, "metrics.json"))); // Показатели.

        // Величины потерь всех восьми уроков совпали с сохранёнными.
        var savedSteps = arms.RootElement.GetProperty("steps").EnumerateArray().Select(step => new
        {
            Arm = step.GetProperty("Arm").GetString()!, // Какое плечо.
            Index = step.GetProperty("Index").GetInt32(), // Какой по счёту урок.
            Lesson = step.GetProperty("RowId").GetString()!, // Какая учебная строка.
            Loss = step.GetProperty("Loss").GetDouble(), // Величина потерь урока.
            Changed = step.GetProperty("ChangedCoordinates").GetInt32() // Сколько чисел сдвинул урок.
        }).ToArray();
        Assert.Equal(_measured.Lessons.Count, savedSteps.Length); // Состав уроков обязан совпасть.
        foreach (var savedStep in savedSteps)
        {
            var live = _measured.Lessons.Single(lesson => lesson.Arm == savedStep.Arm && lesson.Index == savedStep.Index);
            Assert.Equal(savedStep.Lesson, live.LessonId); // Та же учебная строка на том же месте.
            Assert.Equal(savedStep.Loss, live.Loss, 6); // Та же величина потерь.
            Assert.Equal(savedStep.Changed, live.ChangedCoordinates); // То же число сдвинутых чисел.
        }

        // Итоговое изменение таблицы и состав изменившихся строк совпали.
        var savedTotals = arms.RootElement.GetProperty("totals"); // Итоги обоих плеч.
        Assert.Equal(
            savedTotals.GetProperty("arm_a").GetProperty("ChangedCoordinates").GetInt32(),
            _measured.AblationArmDelta.ChangedCoordinates);
        Assert.Equal(
            savedTotals.GetProperty("arm_b").GetProperty("ChangedCoordinates").GetInt32(),
            _measured.NativeArmDelta.ChangedCoordinates);
        Assert.Equal(
            savedTotals.GetProperty("arm_b").GetProperty("ChangedRows").EnumerateArray()
                .Select(value => value.GetInt32()).ToArray(),
            _measured.NativeArmDelta.ChangedRows);

        // Независимый оракул сложения дал те же числа.
        var savedOracle = arms.RootElement.GetProperty("oracle").GetProperty("result"); // Итог оракула.
        Assert.Equal(savedOracle.GetProperty("MismatchedCoordinates").GetInt32(), _measured.OracleMismatchedCoordinates);
        Assert.Equal(savedOracle.GetProperty("RepeatedIdRows").GetInt32(), _measured.OracleRepeatedIdRows);
        Assert.Equal(savedOracle.GetProperty("ActualNonZeroRows").GetInt32(), _measured.OracleActualNonZeroRows);

        // Пороги контроля «в лоб» совпали с теми, что стояли в прогоне опыта.
        var savedDifference = arms.RootElement.GetProperty("finite_difference"); // Раздел контроля «в лоб».
        Assert.Equal(
            EmbeddingLearningReachabilityFixture.FiniteDifferenceEpsilon,
            savedDifference.GetProperty("epsilon").GetDouble());
        Assert.Equal(
            EmbeddingLearningReachabilityFixture.FiniteDifferenceRelativeTolerance,
            savedDifference.GetProperty("own_tolerance").GetProperty("relative").GetDouble());
        Assert.Equal(
            EmbeddingLearningReachabilityFixture.FiniteDifferenceAbsoluteTolerance,
            savedDifference.GetProperty("own_tolerance").GetProperty("absolute").GetDouble());

        // Обе производные на всех девяти координатах воспроизведены.
        var savedCoordinates = savedDifference.GetProperty("coordinates").EnumerateArray().Select(row => new
        {
            Row = row.GetProperty("Row").GetInt32(), // Строка таблицы.
            Column = row.GetProperty("Column").GetInt32(), // Столбец строки.
            Analytic = row.GetProperty("Analytic").GetDouble(), // Аналитическая производная.
            Numeric = row.GetProperty("Numeric").GetDouble() // Конечная разность.
        }).ToArray();
        Assert.Equal(_measured.FiniteDifferenceRows.Count, savedCoordinates.Length); // Состав координат совпал.
        foreach (var savedRow in savedCoordinates)
        {
            var live = _measured.FiniteDifferenceRows.Single(row => row.Row == savedRow.Row && row.Column == savedRow.Column);
            Assert.Equal(savedRow.Analytic, live.Analytic, 6); // Аналитическая производная воспроизведена.
            Assert.Equal(savedRow.Numeric, live.FiniteDifference, 6); // И конечная разность тоже.
        }

        // Ответы панели после обучения совпали построчно по обоим плечам.
        var savedPanelAfter = metrics.RootElement.GetProperty("panel").GetProperty("after").EnumerateArray().Select(row => new
        {
            State = row.GetProperty("State").GetString()!, // Строка вида «B:после».
            Id = row.GetProperty("Id").GetString()!, // Имя строки панели.
            Answer = row.GetProperty("GreedyText").GetString()! // Жадный ответ модели.
        }).ToArray();
        Assert.Equal(_measured.PanelAfter.Count, savedPanelAfter.Length); // Состав панели совпал.
        foreach (var savedRow in savedPanelAfter)
        {
            var arm = savedRow.State.Split(':')[0]; // Имя плеча из сохранённой строки состояния.
            var live = _measured.PanelAfter.Single(row => row.Arm == arm && row.Id == savedRow.Id);
            Assert.Equal(savedRow.Answer, live.Answer); // Тот же ответ на той же строке того же плеча.
        }

        _output.WriteLine("Живой прогон воспроизвёл сохранённые доказательства опыта: "
            + $"{savedSteps.Length} уроков, {savedCoordinates.Length} координат производной, "
            + $"{savedPanelAfter.Length} ответов панели.");
    }

    // ── Граница вывода записывается кодом, а не только текстом отчёта ────────────

    /// <summary>
    /// Это покрытие проверяет один профиль модели, один набор уроков и экзаменов,
    /// один обученный старт и девять заранее закреплённых координат. Оно не
    /// доказывает все координаты таблицы, многознаковую маску ответа, другие
    /// предметные области и не заявляет прироста качества. Холодное
    /// восстановление сохранённого состояния — отдельная будущая проверка со
    /// своим бюджетом, и в эти пятьдесят девять вызовов модели она не спрятана.
    /// </summary>
    [Fact]
    public void EvidenceBoundary_IsOneProfileOneLessonSetOneStartAndNinePinnedCoordinates()
    {
        Assert.Equal(9, _measured.FiniteDifferenceRows.Count); // Проверено девять координат из 16384.
        Assert.Equal(16384, _measured.TableCoordinates); // Всего чисел в таблице вложений.
        Assert.Equal(17, _measured.ReachableRows.Count); // Уроки достигают только семнадцати строк.
        Assert.All(_measured.PanelAfter, row => Assert.Equal(1, row.Answer.Length)); // Ответ ровно из одного знака.
        Assert.Equal(
            EmbeddingLearningReachabilityFixture.LessonsPerArm * 2,
            _measured.Lessons.Count); // Восемь уроков всего: по четыре на плечо, один старт.

        // Старт тот самый проверенный обученный, а не новая база.
        Assert.Equal(
            EmbeddingLearningReachabilityFixture.StartWeightsSha256,
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(
                EmbeddingLearningReachabilityFixture.SharedLearnedStartDirectory(), "S0.weights.bin"))))
                .ToLowerInvariant());

        // Холодного восстановления здесь нет: сохранённые веса заново не загружались.
        Assert.Equal(EmbeddingLearningReachabilityFixture.PlannedModelForwards, _measured.ModelForwards);

        _output.WriteLine(
            "Граница вывода: один профиль, один набор уроков, один обученный старт, девять закреплённых "
            + "координат из 16384. Не доказаны все координаты, многознаковая маска ответа и другие "
            + "предметные области. Два верных ответа из четырёх объясняются постоянным ответом знаком "
            + "«плюс», поэтому прирост качества не заявляется. Холодное восстановление сохранённого "
            + "состояния — отдельная проверка со своим бюджетом.");
    }
}
