// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Регрессионное покрытие производной родного обучения предпочтению
// (вторая половина строки работ «точность производных чисел», номер 29-D1
// в плане ремонта 88 из папки docs/plans/replanning-possible-diffs/
// fixes-based-on-experiments).
//
// Что здесь проверяется по-человечески. Модель учат предпочитать правильный
// ответ неправильному: ей показывают пару «хороший ответ / плохой ответ» и
// одним шагом двигают веса в сторону хорошего. Чтобы такой шаг вообще имел
// смысл, величина и направление сдвига, которые обучающий код выводит
// аналитически, обязаны совпасть с той же величиной, посчитанной «в лоб»:
// меняем один вес на крошечную величину в плюс и в минус, дважды считаем
// потери и делим разность пополам. Опыт десятый это однажды измерил; здесь
// то же самое становится постоянной проверкой уже работающего кода.
//
// Чем это отличается от простого повторения опыта. Проверка вызывает
// НАСТОЯЩИЙ родной путь обучения предпочтению — продуктового получателя
// адаптеров, продуктовый профиль исполнения, продуктового обучателя и его
// единственный StepVector. Величину потерь для конечной разности считают те
// же самые продуктовые формулы, взятые отражением прямо из обучателя, а не
// их копия: если формулу изменят, сдвинется и эталон, и проверяемое число,
// и расхождение станет видно.
//
// Откуда берутся ожидаемые числа. Из уже сохранённого файла сравнений
// experiments-v2/RP01-MICRO/P1001/results/attempt-0001/derivative-comparison.json
// и из принятой поправки audit-correction-r29. Числа нового кода ожиданиями
// не становятся: сохранённые поля ещё и независимо пересчитываются здесь из
// исходных величин той же строки.
//
// Чего это покрытие НЕ доказывает. Проверены две заранее закреплённые
// координаты на одной учебной строке одного профиля при трёх значениях шага
// возмущения. Это не закон о всех координатах, не проверка многознаковой
// маски ответа, не проверка других предметных областей и не новый
// продуктовый алгоритм. Два верных ответа из четырёх — сохранение, а не
// прирост качества: ровно столько же даёт тривиальный ответ «плюс» всегда.

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure.Logical;
using DPO;
using DPO.Models;
using Inference;
using LoRA;
using P1001Experiment;
using TensorCore.Core;
using TensorCore.Init;
using TensorCore.Layers;
using Tokenizer;
using TrainingLoop.Optimizers;
using TransformerModel;
using TransformerModel.Config;

/// <summary>Одно сравнение аналитической производной с независимой конечной разностью.</summary>
/// <param name="Coordinate">Короткое имя координаты: проекция запроса или проекция значения.</param>
/// <param name="BindingKey">Имя привязки этой координаты в контрольной точке.</param>
/// <param name="Epsilon">Шаг возмущения, на котором снята конечная разность.</param>
/// <param name="AnalyticDerivative">Производная величины потерь, снятая с настоящего шага.</param>
/// <param name="FiniteDifference">Независимая центральная конечная разность той же величины.</param>
/// <param name="AbsoluteError">Абсолютная погрешность согласия двух способов.</param>
/// <param name="RelativeError">Относительная погрешность согласия двух способов.</param>
/// <param name="NearZeroFiniteDifference">Признак околонулевой конечной разности.</param>
/// <param name="MarginSymmetry">Полусумма запасов предпочтения при симметричном возмущении.</param>
public sealed record ComposedGradientRow(
    string Coordinate,
    string BindingKey,
    double Epsilon,
    double AnalyticDerivative,
    double FiniteDifference,
    double AbsoluteError,
    double RelativeError,
    bool NearZeroFiniteDifference,
    double MarginSymmetry);

/// <summary>Одна заранее закреплённая координата и оба восстановления её производной.</summary>
/// <param name="Coordinate">Короткое имя координаты.</param>
/// <param name="BindingKey">Имя привязки этой координаты в контрольной точке.</param>
/// <param name="FirstMoment">Первый момент оптимизатора после единственного обновления.</param>
/// <param name="SecondMoment">Второй момент оптимизатора после единственного обновления.</param>
/// <param name="DerivativeAsAppliedByOptimizer">Производная вместе с множителем вклада.</param>
/// <param name="AnalyticDerivative">Та же производная после однократного снятия множителя.</param>
/// <param name="MagnitudeFromSecondMoment">Величина производной, восстановленная вторым моментом.</param>
public sealed record ComposedGradientCoordinate(
    string Coordinate,
    string BindingKey,
    float FirstMoment,
    float SecondMoment,
    double DerivativeAsAppliedByOptimizer,
    double AnalyticDerivative,
    double MagnitudeFromSecondMoment);

/// <summary>Одно чтение панели: что спросили, что ответили и насколько уверенно.</summary>
/// <param name="Id">Короткое имя строки панели.</param>
/// <param name="Gold">Верный ответ, посчитанный из самого запроса.</param>
/// <param name="Answer">Жадный ответ модели одним знаком.</param>
/// <param name="Correct">Совпал ли ответ с верным.</param>
/// <param name="LogPlus">Логарифмическая вероятность знака «плюс».</param>
/// <param name="LogMinus">Логарифмическая вероятность знака «минус».</param>
public sealed record ComposedPanelRow(
    string Id, string Gold, string Answer, bool Correct, double LogPlus, double LogMinus);

/// <summary>
/// Общая подготовка: один настоящий шаг обучения предпочтению из проверенного
/// обученного старта и независимая конечная разность той же величины потерь.
/// Подготовка выполняется ровно один раз на весь класс проверок, поэтому
/// назначенная доза вызовов модели тратится один раз, а не на каждый факт.
/// </summary>
public sealed class ComposedDpoGradientFixture : IDisposable
{
    // ── Паспортные опознаватели проверенного обученного старта ───────────────────
    /// <summary>Отпечаток весов старта, названный паспортом опыта и описью набора.</summary>
    public const string StartWeightsSha256 =
        "8721dd57b11fa61b11acf37c337611129891a1d3096ae0392a89455d6d58fdec";

    /// <summary>Счётчик шага донорского оптимизатора обучения с учителем: происхождение, не доза.</summary>
    private const int DonorOptimizerStep = 400;

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

    // ── Настройки чтения панели, закреплённые опытом ─────────────────────────────
    /// <summary>Зерно выдачи; при нулевой температуре на выбор знака не влияет.</summary>
    private const int ReadoutSeed = 20261027;

    /// <summary>Ровно один знак ответа, как требует паспорт опыта.</summary>
    private const int ReadoutMaxNewTokens = 1;

    /// <summary>Жадная выдача: случайность в выборе знака не участвует.</summary>
    private const float ReadoutTemperature = 0f;

    /// <summary>Отсечение по числу кандидатов выключено.</summary>
    private const int ReadoutTopK = 0;

    /// <summary>Отсечение по суммарной вероятности выключено.</summary>
    private const float ReadoutTopP = 1f;

    // ── Допуски согласия, закреплённые паспортом опыта ───────────────────────────
    /// <summary>Предельная абсолютная погрешность согласия двух способов.</summary>
    public const double MaxAbsoluteTolerance = 1e-4;

    /// <summary>Предельная относительная погрешность согласия двух способов.</summary>
    public const double MaxRelativeTolerance = 1e-2;

    /// <summary>Порог, ниже которого конечная разность считается околонулевой.</summary>
    public const double NearZeroThreshold = 1e-6;

    /// <summary>Три значения шага возмущения: исходный масштаб опыта.</summary>
    public static readonly double[] Epsilons = [1e-3, 3e-3, 1e-2];

    // ── Назначенный бюджет: исполняется ровно, без скрытых лишних вызовов ────────
    /// <summary>Назначенное число прямых проходов модели.</summary>
    public const int PlannedModelForwards = 36;

    /// <summary>Назначенное число обратных проходов.</summary>
    public const int PlannedBackwards = 4;

    /// <summary>Назначенное число обновлений оптимизатора.</summary>
    public const int PlannedUpdates = 1;

    /// <summary>Назначенное число жадных выдач панели.</summary>
    public const int PlannedGenerations = 8;

    /// <summary>Назначенное число вычислений величины потерь конечной разностью.</summary>
    public const int PlannedFiniteDifferenceEvaluations = 12;

    /// <summary>Назначенное число прогревочных прогонов: ноль, старт берётся обученным.</summary>
    public const int PlannedBootstrapRuns = 0;

    // ── Узкий отражающий доступ к настоящим продуктовым формулам ─────────────────
    // Копия формул в проверку не переносится: берутся сами продуктовые члены,
    // поэтому изменение формулы сдвигает и эталон, и проверяемое число.

    /// <summary>Настоящая продуктовая сумма логарифмических вероятностей ответа.</summary>
    private static readonly MethodInfo ProductComputeTargetLogProbs =
        typeof(DpoTrainer).GetMethod("ComputeTargetLogProbs", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new MissingMethodException("COMPOSED_DPO_GRADIENT_PRODUCT_LOGPROBS_MISSING");

    /// <summary>Настоящая продуктовая точная величина потерь и её производная по запасу.</summary>
    private static readonly MethodInfo ProductExactObjective =
        typeof(DpoTrainer).GetMethod("ExactObjective", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new MissingMethodException("COMPOSED_DPO_GRADIENT_PRODUCT_OBJECTIVE_MISSING");

    /// <summary>Настоящая продуктовая сила вклада обучающего вектора.</summary>
    private static readonly MethodInfo ProductVectorSignal =
        typeof(DpoTrainer).GetMethod("VectorSignal", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new MissingMethodException("COMPOSED_DPO_GRADIENT_PRODUCT_VECTOR_SIGNAL_MISSING");

    /// <summary>Настоящий продуктовый коэффициент силы расхождения с эталоном.</summary>
    private static readonly float ProductBeta =
        (float)(typeof(DpoTrainer).GetField("Beta", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingFieldException("COMPOSED_DPO_GRADIENT_PRODUCT_BETA_MISSING")).GetRawConstantValue()!;

    /// <summary>Временный каталог обучателя: контрольные точки шага сюда и ложатся.</summary>
    private readonly string _checkpointRoot =
        Path.Combine(Path.GetTempPath(), $"composed-dpo-gradient-{Guid.NewGuid():N}");

    // ── Счётчики фактической работы ──────────────────────────────────────────────
    /// <summary>Фактические прямые проходы модели всех видов.</summary>
    public int ModelForwards { get; private set; }

    /// <summary>Фактические обратные проходы: учебные внутри шага и контрольные.</summary>
    public int Backwards { get; private set; }

    /// <summary>Фактически применённые обновления оптимизатора.</summary>
    public int Updates { get; private set; }

    /// <summary>Фактические жадные выдачи панели: они же дают прямые проходы панели.</summary>
    public int Generations { get; private set; }

    /// <summary>Фактические вычисления величины потерь конечной разностью.</summary>
    public int FiniteDifferenceEvaluations { get; private set; }

    /// <summary>Фактические прогревочные прогоны: обучение с нуля здесь не выполняется.</summary>
    public int BootstrapRuns => 0;

    // ── Измеренный результат общей подготовки ────────────────────────────────────
    /// <summary>Шесть сравнений: две координаты на три значения шага возмущения.</summary>
    public IReadOnlyList<ComposedGradientRow> Rows { get; }

    /// <summary>Две координаты и оба восстановления их производной.</summary>
    public IReadOnlyList<ComposedGradientCoordinate> Coordinates { get; }

    /// <summary>Панель из четырёх чтений до настоящего шага.</summary>
    public IReadOnlyList<ComposedPanelRow> PanelBefore { get; }

    /// <summary>Панель из четырёх чтений после настоящего шага.</summary>
    public IReadOnlyList<ComposedPanelRow> PanelAfter { get; }

    /// <summary>Сила вклада обучающего вектора, посчитанная продуктовой формулой.</summary>
    public float VectorSignal { get; }

    /// <summary>Нормализованный вес единственной пары в наборе этого шага.</summary>
    public float VectorWeight { get; }

    /// <summary>Общий множитель производной: вес пары, умноженный на силу вклада.</summary>
    public float ScaledWeight { get; }

    /// <summary>Первый коэффициент моментов настоящего оптимизатора предпочтений.</summary>
    public float Beta1 { get; }

    /// <summary>Второй коэффициент моментов настоящего оптимизатора предпочтений.</summary>
    public float Beta2 { get; }

    /// <summary>Счётчик шага оптимизатора до настоящего шага: обязан быть нулевым.</summary>
    public int OptimizerStepBefore { get; }

    /// <summary>Счётчик шага оптимизатора после настоящего шага: обязан стать единицей.</summary>
    public int OptimizerStepAfter { get; }

    /// <summary>Опознаватель точной функции потерь, вернувшийся из настоящего шага.</summary>
    public string? ObjectiveVersion { get; }

    /// <summary>Отпечаток замороженной основы обучаемой модели до шага.</summary>
    public string FrozenBeforeSha256 { get; }

    /// <summary>Отпечаток той же замороженной основы после всего прогона.</summary>
    public string FrozenAfterSha256 { get; }

    /// <summary>Отпечаток эталонной модели до шага.</summary>
    public string ReferenceBeforeSha256 { get; }

    /// <summary>Отпечаток эталонной модели после всего прогона.</summary>
    public string ReferenceAfterSha256 { get; }

    /// <summary>Отпечаток обучаемых адаптеров до шага.</summary>
    public string AdaptersBeforeSha256 { get; }

    /// <summary>Отпечаток обучаемых адаптеров после шага.</summary>
    public string AdaptersAfterSha256 { get; }

    /// <summary>Состояние обучаемой модели не менялось во время чтения панели до шага.</summary>
    public bool BeforePanelStateStable { get; }

    /// <summary>Состояние обучаемой модели не менялось во время чтения панели после шага.</summary>
    public bool AfterPanelStateStable { get; }

    /// <summary>Сколько обучаемых массивов изменил контрольный нулевой верхний градиент.</summary>
    public int ZeroUpstreamChangedArrays { get; }

    /// <summary>Наибольшее изменение одного числа градиента при нулевом верхнем градиенте.</summary>
    public double ZeroUpstreamMaxAbsoluteChange { get; }

    /// <summary>Сколько раз нетронутые координаты совпали после восстановления точно.</summary>
    public int RestoredExactlyCount { get; }

    /// <summary>Путь проверенного обученного старта, из которого начата эта проверка.</summary>
    public string StartBundleDirectory { get; }

    /// <summary>Выполнить общую подготовку: один настоящий шаг и независимая конечная разность.</summary>
    public ComposedDpoGradientFixture()
    {
        // ── 1. Проверенный обученный старт опознаётся отпечатком, а не именем файла ──
        StartBundleDirectory = SharedLearnedStartDirectory(); // Папка проверенного обученного старта.
        var weightsPath = Path.Combine(StartBundleDirectory, "S0.weights.bin"); // Файл сохранённых весов старта.
        var actualWeightsSha = Sha256(File.ReadAllBytes(weightsPath)); // Отпечаток фактических байтов весов.
        if (actualWeightsSha != StartWeightsSha256) // Другой отпечаток означает другой старт, а не ту же проверку.
            throw new InvalidOperationException("COMPOSED_DPO_GRADIENT_START_WEIGHTS_SHA:" + actualWeightsSha);

        // Опись набора обязана называть тот же отпечаток: иначе сам набор несогласован.
        using (var bundleDocument = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(StartBundleDirectory, "bundle.json"))))
        {
            var declared = bundleDocument.RootElement.GetProperty("files").EnumerateArray()
                .Single(file => file.GetProperty("name").GetString() == "S0.weights.bin")
                .GetProperty("sha256").GetString(); // Отпечаток весов, записанный описью набора.
            if (declared != StartWeightsSha256) // Опись и паспорт обязаны называть одно и то же.
                throw new InvalidOperationException("COMPOSED_DPO_GRADIENT_BUNDLE_SHA:" + declared);
        }

        // ── 2. Численная настройка модели читается из описания порождения набора ────
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

        // ── 3. Словарь: тот самый побайтовый переходник, которым читал опыт ─────────
        var tokenizer = BpeTokenizer.Load(Path.Combine(
            RepositoryRoot(), "experiments-v2", "RP01-MICRO", "P1001", "src", "byte-identity-tokenizer.json"));
        if (tokenizer.VocabSize != config.VocabSize) // Ширина словаря обязана совпасть с моделью.
            throw new InvalidOperationException("COMPOSED_DPO_GRADIENT_TOKENIZER_VOCAB:" + tokenizer.VocabSize);
        var plusTokenId = tokenizer.Encode("+").Single(); // Номер знака «плюс»: ответ ровно из одного знака.
        var minusTokenId = tokenizer.Encode("-").Single(); // Номер знака «минус»: ответ ровно из одного знака.

        // ── 4. Уроки и экзамен — донорские байты набора, а не переписанный текст ────
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
            throw new InvalidOperationException("COMPOSED_DPO_GRADIENT_DONOR_ROWS"); // Донор обязан дать ровно две строки.

        // Панель чтения: две учебные строки и две экзаменационные; верный ответ считается из запроса.
        var panel = new[]
        {
            (Id: "T1", Prompt: donorRows[0].Prompt, Gold: GoldFromPrompt(donorRows[0].Prompt)), // Учебная строка шага.
            (Id: "T2", Prompt: donorRows[1].Prompt, Gold: GoldFromPrompt(donorRows[1].Prompt)), // Вторая учебная строка.
            (Id: "E1", Prompt: "C# result sign: 8-3 =", Gold: GoldFromPrompt("C# result sign: 8-3 =")), // Первый экзамен.
            (Id: "E2", Prompt: "C# result sign: 3-8 =", Gold: GoldFromPrompt("C# result sign: 3-8 =")) // Второй экзамен.
        };
        if (donorRows[0].Answer != panel[0].Gold || donorRows[1].Answer != panel[1].Gold)
            throw new InvalidOperationException("COMPOSED_DPO_GRADIENT_GOLD_DIFFERS_FROM_DONOR"); // Эталон обязан совпасть с донором.

        // ── 5. Восстановление проверенного обученного старта: политика и эталон ─────
        var lora = new LoraConfig { Rank = 4, Alpha = 8f, Dropout = 0f, TargetModules = ["q_proj", "v_proj"] }; // Настройка адаптеров паспорта.
        var topologySha = Sha256Text(TopologyLiteral); // Опознаватель топологии для снимка генератора.
        var loraSha = Sha256Text(LoraLiteral); // Опознаватель настройки адаптеров для того же снимка.
        var randomSnapshot = JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>(
            File.ReadAllBytes(Path.Combine(StartBundleDirectory, "S0.rng.json")))
            ?? throw new InvalidOperationException("COMPOSED_DPO_GRADIENT_RANDOM_STATE_UNREADABLE"); // Снимок генератора набора.
        var randomIdentity = new LlmModelRandomStateIdentity(topologySha, loraSha); // Пара опознавателей снимка.

        LlmModel BuildModel() // Одинаковая сборка для политики, эталона и всех копий конечной разности.
        {
            LlmModel built; // Модель объявлена снаружи блока зерна, чтобы жить дальше.
            using (Initializers.UseDeterministicSeed(BaseSeed)) built = new LlmModel(config); // Основа тем же зерном набора.
            LoraApplicator.ApplySeeded(built, lora, AdapterSeed, AdapterStream); // Адаптеры тем же зерном и потоком.
            built.LoadCheckpoint(Path.Combine(StartBundleDirectory, "S0.weights.bin")); // Поверх кладутся обученные веса старта.
            built.RestoreRandomState(randomSnapshot, randomIdentity); // Состояние генератора восстанавливается с проверкой.
            return built; // Готовая копия проверенного обученного старта.
        }

        var policy = BuildModel(); // Обучаемая модель этой проверки.
        var reference = BuildModel(); // Неизменяемая эталонная модель с теми же весами старта.
        foreach (var parameter in reference.Parameters()) parameter.Value.RequiresGrad = false; // Эталон не обучается никогда.

        var adapters = Enumerable.Range(0, policy.Blocks.Count)
            .SelectMany(block => lora.TargetModules.Select(target => (LoraLinear)policy.GetAttachedProjectionAdapter(block, target)!))
            .ToArray(); // Все присоединённые адаптеры запроса и значения обучаемой модели.
        var bindings = AdapterBindings(policy, adapters); // Именованные привязки адаптеров в закреплённом порядке.
        var trainableParameters = policy.Parameters().Where(parameter => parameter.Value.RequiresGrad).ToArray(); // Обучаемые массивы.
        var frozenParameters = policy.Parameters().Where(parameter => !parameter.Value.RequiresGrad).ToArray(); // Замороженная основа.
        if (trainableParameters.Length == 0 || frozenParameters.Length == 0)
            throw new InvalidOperationException("COMPOSED_DPO_GRADIENT_NO_TRAINABLE_SPLIT"); // Без обоих наборов проверка бессмысленна.

        // Донорский оптимизатор обучения с учителем остаётся происхождением и НЕ импортируется.
        var donorOptimizer = AdamWPortableState.FromBytes(
            File.ReadAllBytes(Path.Combine(StartBundleDirectory, "S0.optimizer.bin")))
            ?? throw new InvalidOperationException("COMPOSED_DPO_GRADIENT_DONOR_OPTIMIZER_UNREADABLE"); // Донорские моменты.
        if (donorOptimizer.Step != DonorOptimizerStep) // Паспорт назвал донорский счётчик шага 400.
            throw new InvalidOperationException("COMPOSED_DPO_GRADIENT_DONOR_STEP:" + donorOptimizer.Step);

        FrozenBeforeSha256 = ParametersSha256(frozenParameters); // Отпечаток замороженной основы до шага.
        ReferenceBeforeSha256 = ParametersSha256(reference.Parameters()); // Отпечаток эталонной модели до шага.
        AdaptersBeforeSha256 = ParametersSha256(bindings.Select(binding => binding.Parameter)); // Отпечаток адаптеров до шага.
        var trainableBeforeSha = ParametersSha256(trainableParameters); // Отпечаток обучаемых массивов до шага.
        var rngBeforeSha = Sha256Text(JsonSerializer.Serialize(policy.CaptureRandomState(topologySha, loraSha, 0))); // Генератор до шага.

        // ── 6. Панель ДО шага: четыре жадные выдачи, по одному прямому проходу ──────
        ComposedPanelRow ReadRow(string id, string prompt, string gold)
        {
            var generator = new TextGenerator(policy, tokenizer); // Продуктовый порождатель на текущем состоянии.
            var produced = generator.GenerateWithFirstDecision(
                prompt, ReadoutMaxNewTokens, ReadoutTemperature, ReadoutTopK, ReadoutTopP,
                ReadoutSeed, null, CancellationToken.None); // Тот же продуктовый путь выдачи плюс захват чисел решения.
            var forwards = produced.PrefillForwardCalls + produced.DecodeForwardCalls; // Сколько прямых проходов стоило чтение.
            ModelForwards += forwards; // Прямые проходы панели входят в общий счёт.
            Generations++; // Одна жадная выдача: повторно к прямым проходам она не прибавляется.
            var logSoftmax = LogSoftmax(produced.Decision.Logits); // Логарифмические вероятности всех знаков словаря.
            return new ComposedPanelRow(
                id, // Короткое имя строки панели.
                gold, // Верный ответ, посчитанный из самого запроса.
                produced.Receipt.Text, // Жадный ответ, расшифрованный настоящим разбивателем текста.
                string.Equals(produced.Receipt.Text, gold, StringComparison.Ordinal), // Совпал ли ответ с верным.
                logSoftmax[plusTokenId], // Уверенность в знаке «плюс».
                logSoftmax[minusTokenId]); // Уверенность в знаке «минус».
        }

        PanelBefore = panel.Select(row => ReadRow(row.Id, row.Prompt, row.Gold)).ToArray(); // Панель до шага.
        BeforePanelStateStable = trainableBeforeSha == ParametersSha256(trainableParameters)
            && FrozenBeforeSha256 == ParametersSha256(frozenParameters)
            && AdaptersBeforeSha256 == ParametersSha256(bindings.Select(binding => binding.Parameter))
            && rngBeforeSha == Sha256Text(JsonSerializer.Serialize(policy.CaptureRandomState(topologySha, loraSha, 0)));

        // Оценки замороженного эталона для учебной строки берутся из этого же чтения
        // один раз: эталон получил те же веса старта, что и политика, а ответ занимает
        // ровно один знак, поэтому сумма логарифмических вероятностей ответа равна
        // логарифмической вероятности этого знака на том же запросе. Дополнительных
        // прямых проходов эталона в конечной разности поэтому не выполняется.
        var t1Before = PanelBefore.Single(row => row.Id == "T1"); // Чтение учебной строки до шага.
        var referenceChosenLogP = (float)t1Before.LogPlus; // Оценка эталона для предпочтительного ответа.
        var referenceRejectedLogP = (float)t1Before.LogMinus; // Оценка эталона для отклонённого ответа.

        // ── 7. Настоящая продуктовая подготовка точной пары предпочтения ───────────
        var preferences = new[]
        {
            new LogicalDpoPreferenceExample(donorRows[0].Prompt, donorRows[0].Answer, donorRows[1].Answer)
            {
                ChosenAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator, // Предпочтительная сторона — строка донора.
                RejectedAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator // Отклонённая сторона — тоже строка донора.
            }
        };
        var prepared = LogicalDpoPreparedBatchMaterializer.Prepare(preferences, tokenizer); // Штатная подготовка точной партии.
        if (!prepared.IsSuccess || prepared.Value is null || prepared.Value.Count != 1)
            throw new InvalidOperationException("COMPOSED_DPO_GRADIENT_PREPARE_REFUSED:" + prepared.ErrorCode); // Пара ровно одна.
        var vectorBatch = prepared.Value[0]; // Единственная партия обучающего вектора этого шага.
        var batch = vectorBatch.Batch; // Сама партия пары предпочтения.
        if (batch.ExactMaterial is null || batch.MaterialVersion != DpoExactPairMaterial.Version)
            throw new InvalidOperationException("COMPOSED_DPO_GRADIENT_MATERIAL_NOT_EXACT:" + batch.MaterialVersion);
        var chosenResponseMask = batch.ChosenResponseMask
            ?? throw new InvalidOperationException("COMPOSED_DPO_GRADIENT_CHOSEN_RESPONSE_MASK_MISSING"); // Маска ответа.
        var rejectedResponseMask = batch.RejectedResponseMask
            ?? throw new InvalidOperationException("COMPOSED_DPO_GRADIENT_REJECTED_RESPONSE_MASK_MISSING"); // Маска ответа.
        if (chosenResponseMask.Data.Count(value => value != 0f) != 1
            || rejectedResponseMask.Data.Count(value => value != 0f) != 1)
            throw new InvalidOperationException("COMPOSED_DPO_GRADIENT_RESPONSE_MASK_UNEXPECTED"); // Ответ ровно из одного знака.

        // ── 8. Настоящий продуктовый получатель, профиль и обучатель предпочтений ──
        var recipient = DpoProjectLoraRecipient.Capture(policy, reference, adapters, lora); // Настоящий получатель изменений.
        var profile = DpoExecutionProfile.Capture(batch, recipient.Bindings); // Настоящий профиль исполнения.
        var trainer = DpoTrainer.CreateForProjectLora(
            policy, reference, _checkpointRoot, DpoMutationAdmissionCapability.Issue(new TestLease()),
            recipient, profile, null, null,
            step => policy.CaptureRandomState(topologySha, loraSha, step), // Снимок генератора продуктовым способом.
            state => policy.RestoreRandomState(state, randomIdentity), // Восстановление генератора продуктовым способом.
            2); // Ёмкость памяти ответов эталона, закреплённая этим опытом.

        var optimizerSettings = trainer.OptimizerSettings; // Фактические настройки оптимизатора предпочтений.
        Beta1 = optimizerSettings.Beta1; // Первый коэффициент моментов берётся у самого обучателя.
        Beta2 = optimizerSettings.Beta2; // Второй коэффициент моментов берётся у самого обучателя.
        OptimizerStepBefore = trainer.ExportOptimizerState().Step; // Счётчик шага до настоящего шага.

        VectorSignal = (float)ProductVectorSignal.Invoke(null, [vectorBatch.TeachingVector])!; // Сила вклада продуктовой формулой.
        VectorWeight = Math.Max(0.01f, vectorBatch.Weight) / Math.Max(1e-6f, Math.Max(0.01f, vectorBatch.Weight)); // Вес пары.
        ScaledWeight = VectorWeight * VectorSignal; // Именно на это число обучатель умножает производную.

        // ── 9. Единственный настоящий шаг обучения предпочтению ────────────────────
        var timing = new DpoStepVectorTiming(); // Наблюдение фаз шага: фактические вызовы, а не предположения.
        var metrics = trainer.StepVector([vectorBatch], 1, timing); // НАСТОЯЩИЙ шаг: два плеча потерь, два обратных прохода, одно обновление.
        var timingSnapshot = timing.Snapshot(); // Фактические счётчики фаз шага от продуктового наблюдателя.
        ModelForwards += (int)(timingSnapshot.ReferenceForward.CompletedCalls + timingSnapshot.PolicyForward.CompletedCalls);
        Backwards += (int)timingSnapshot.Backward.CompletedCalls; // Учебные обратные проходы самого шага.
        Updates++; // Применено единственное обновление оптимизатора.
        ObjectiveVersion = metrics.ObjectiveVersion; // Опознаватель точной функции потерь из самого шага.

        var optimizerAfterStep = trainer.ExportOptimizerState(); // Моменты оптимизатора после шага.
        OptimizerStepAfter = optimizerAfterStep.Step; // Счётчик шага после настоящего шага.
        AdaptersAfterSha256 = ParametersSha256(bindings.Select(binding => binding.Parameter)); // Отпечаток адаптеров после шага.

        // ── 10. Восстановление производной из моментов свежего оптимизатора ────────
        // Свежий оптимизатор предпочтений начинает с нулевых моментов, поэтому после
        // ровно одного обновления первый момент равен (1 - beta1) * производная, а
        // второй равен (1 - beta2) * производная в квадрате. Это даёт точное
        // восстановление производной и независимое восстановление её величины.
        Coordinates = new[] { "q_proj", "v_proj" }
            .Select(target =>
            {
                var adapter = (LoraLinear)policy.GetAttachedProjectionAdapter(0, target)!; // Адаптер первого слоя.
                var parameter = adapter.AdapterParameters.Single(entry => entry.Suffix == "lora_b").Parameter; // Матрица B.
                var binding = recipient.Bindings.Single(item => ReferenceEquals(item.Parameter, parameter)); // Её привязка.
                var entryState = optimizerAfterStep.Entries.Single(item => item.Key == binding.Key); // Её моменты после шага.
                var firstMoment = entryState.FirstMoment[0]; // Первый момент нулевой координаты.
                var secondMoment = entryState.SecondMoment[0]; // Второй момент нулевой координаты.
                var applied = RecoverAppliedDerivative(firstMoment, Beta1); // Производная вместе с множителем вклада.
                var magnitude = Math.Sqrt(Math.Max(0d, secondMoment / (1f - Beta2))) / ScaledWeight; // Величина по второму моменту.
                return new ComposedGradientCoordinate(
                    target, binding.Key, firstMoment, secondMoment, applied, applied / ScaledWeight, magnitude);
            })
            .ToArray();

        // ── 11. Панель ПОСЛЕ шага: четыре чтения того же вида на изменённом состоянии ─
        var trainableAfterStartSha = ParametersSha256(trainableParameters); // Обучаемые массивы перед чтениями.
        var frozenAfterStartSha = ParametersSha256(frozenParameters); // Замороженная основа перед чтениями.
        var adaptersAfterStartSha = ParametersSha256(bindings.Select(binding => binding.Parameter)); // Адаптеры перед чтениями.
        PanelAfter = panel.Select(row => ReadRow(row.Id, row.Prompt, row.Gold)).ToArray(); // Панель после шага.
        AfterPanelStateStable = trainableAfterStartSha == ParametersSha256(trainableParameters)
            && frozenAfterStartSha == ParametersSha256(frozenParameters)
            && adaptersAfterStartSha == ParametersSha256(bindings.Select(binding => binding.Parameter));

        // ── 12. Независимая центральная конечная разность на свежих копиях старта ──
        // Каждое вычисление берёт собственную копию проверенного обученного старта,
        // двигает ровно одну координату и считает ту же величину потерь теми же
        // продуктовыми формулами. Оптимизатор здесь не вызывается, исполняемый граф
        // настоящего шага не затрагивается.
        var chosenIds = batch.ChosenIds; // Номера знаков предпочтительной стороны.
        var rejectedIds = batch.RejectedIds; // Номера знаков отклонённой стороны.
        var referenceCloneSha = ParametersSha256(BuildModel().Parameters()); // Отпечаток нетронутой копии старта.
        var losses = new Dictionary<(string Target, double Epsilon, int Sign), (float Loss, float Margin)>(); // Величины потерь.
        var zeroUpstreamDone = false; // Контроль нулевого верхнего градиента выполняется один раз.
        var zeroUpstreamChanged = -1; // Сколько массивов изменил нулевой верхний градиент.
        var zeroUpstreamMaxChange = -1d; // Наибольшее изменение одного числа градиента.
        var restoredExactly = 0; // Сколько раз нетронутые координаты совпали после восстановления точно.

        foreach (var coordinate in Coordinates) // Обе заранее закреплённые координаты.
        {
            foreach (var epsilon in Epsilons) // Три значения шага возмущения.
            {
                foreach (var sign in new[] { +1, -1 }) // Возмущение в плюс и в минус от исходного значения.
                {
                    var clone = BuildModel(); // Независимая копия старта на каждое вычисление.
                    var cloneAdapter = (LoraLinear)clone.GetAttachedProjectionAdapter(0, coordinate.Coordinate)!; // Тот же адаптер.
                    var cloneParameter = cloneAdapter.AdapterParameters.Single(entry => entry.Suffix == "lora_b").Parameter; // Матрица B.
                    var original = cloneParameter.Value.Data[0]; // Исходное значение возмущаемой координаты.
                    cloneParameter.Value.Data[0] = (float)(original + sign * epsilon); // Возмущение ровно одной координаты.

                    var chosenInput = new Tensor((float[])chosenIds.Data.Clone(), chosenIds.Shape); // Свежий вход предпочтительной стороны.
                    var chosenLogits = clone.Forward(chosenInput, training: true); // Прямой проход политики на возмущённой копии.
                    ModelForwards++; // Учёт фактического прямого прохода конечной разности.
                    var chosenLogP = TargetLogProbs(chosenLogits, chosenInput, chosenResponseMask); // Сумма по размеченному ответу.

                    var rejectedInput = new Tensor((float[])rejectedIds.Data.Clone(), rejectedIds.Shape); // Свежий вход отклонённой стороны.
                    var rejectedLogits = clone.Forward(rejectedInput, training: true); // Прямой проход политики на возмущённой копии.
                    ModelForwards++; // Учёт фактического прямого прохода конечной разности.
                    var rejectedLogP = TargetLogProbs(rejectedLogits, rejectedInput, rejectedResponseMask); // Сумма по размеченному ответу.

                    var chosenReward = ProductBeta * (chosenLogP - referenceChosenLogP); // Та же формула, что в обучателе.
                    var rejectedReward = ProductBeta * (rejectedLogP - referenceRejectedLogP); // Та же формула, что в обучателе.
                    var margin = chosenReward - rejectedReward; // Запас предпочтения на возмущённых весах.
                    var objective = ExactObjective(margin); // Та же точная величина потерь продуктовой формулой.
                    FiniteDifferenceEvaluations++; // Выполнено ещё одно вычисление величины потерь.
                    losses[(coordinate.Coordinate, epsilon, sign)] = (objective.Loss, margin); // Величина потерь и запас этой точки.

                    if (!zeroUpstreamDone) // Сторож нулевого верхнего градиента на уже построенном графе первой копии.
                    {
                        var control = RunZeroUpstreamControl(clone, chosenLogits, rejectedLogits); // Два контрольных обратных прохода.
                        Backwards += 2; // Учёт фактических контрольных обратных проходов.
                        zeroUpstreamChanged = control.ChangedArrays; // Сколько массивов изменилось вопреки нулю.
                        zeroUpstreamMaxChange = control.MaxAbsoluteChange; // Наибольшее изменение одного числа.
                        zeroUpstreamDone = true; // Контроль выполняется ровно один раз.
                    }

                    cloneParameter.Value.Data[0] = original; // Восстановление возмущённой координаты.
                    if (ParametersSha256(clone.Parameters()) == referenceCloneSha) restoredExactly++; // Нетронутые числа совпали точно.
                }
            }
        }

        ZeroUpstreamChangedArrays = zeroUpstreamChanged; // Итог сторожа нулевого верхнего градиента.
        ZeroUpstreamMaxAbsoluteChange = zeroUpstreamMaxChange; // Наибольшее изменение одного числа градиента.
        RestoredExactlyCount = restoredExactly; // Сколько восстановлений оказались точными.

        // ── 13. Шесть сравнений: аналитическая производная против конечной разности ─
        Rows = Coordinates
            .SelectMany(coordinate => Epsilons.Select(epsilon =>
            {
                var plus = losses[(coordinate.Coordinate, epsilon, +1)]; // Величина потерь при возмущении в плюс.
                var minus = losses[(coordinate.Coordinate, epsilon, -1)]; // Величина потерь при возмущении в минус.
                var finiteDifference = (plus.Loss - minus.Loss) / (2d * epsilon); // Центральная конечная разность.
                var absoluteError = Math.Abs(coordinate.AnalyticDerivative - finiteDifference); // Абсолютная погрешность согласия.
                var scale = Math.Max(Math.Abs(coordinate.AnalyticDerivative), Math.Abs(finiteDifference)); // Масштаб сравнения.
                var relativeError = scale > 0d ? absoluteError / scale : 0d; // Относительная погрешность согласия.
                return new ComposedGradientRow(
                    coordinate.Coordinate, // Какая координата сравнивается.
                    coordinate.BindingKey, // Имя её привязки в контрольной точке.
                    epsilon, // Значение шага возмущения.
                    coordinate.AnalyticDerivative, // Аналитическая производная величины потерь.
                    finiteDifference, // Независимая центральная конечная разность.
                    absoluteError, // Абсолютная погрешность согласия.
                    relativeError, // Относительная погрешность согласия.
                    Math.Abs(finiteDifference) < NearZeroThreshold, // Околонулевая ли конечная разность.
                    (plus.Margin + minus.Margin) / 2d); // Полусумма запасов при симметричном возмущении.
            }))
            .ToArray();

        // ── 14. Неизменность эталона и замороженной основы после всего прогона ─────
        FrozenAfterSha256 = ParametersSha256(frozenParameters); // Замороженная основа после всего прогона.
        ReferenceAfterSha256 = ParametersSha256(reference.Parameters()); // Эталонная модель после всего прогона.
    }

    /// <summary>Шесть сравнений в виде, который принимает действующий построитель производных чисел.</summary>
    internal IReadOnlyList<P1001DerivativeComparison> AsSummaryComparisons()
        => Rows.Select(row => new P1001DerivativeComparison
        {
            Coordinate = row.Coordinate, // Какая координата сравнивается.
            BindingKey = row.BindingKey, // Имя привязки этой координаты.
            Index = 0, // Нулевой индекс матрицы B: координата закреплена заранее.
            Epsilon = row.Epsilon, // Значение шага возмущения.
            AnalyticDerivative = row.AnalyticDerivative, // Аналитическая производная величины потерь.
            FiniteDifference = row.FiniteDifference, // Независимая центральная конечная разность.
            AbsoluteError = row.AbsoluteError, // Абсолютная погрешность согласия.
            RelativeError = row.RelativeError, // Относительная погрешность согласия.
            NearZeroFiniteDifference = row.NearZeroFiniteDifference, // Околонулевая ли конечная разность.
            MarginSymmetry = row.MarginSymmetry // Полусумма запасов при симметричном возмущении.
        }).ToArray();

    /// <summary>
    /// Восстановление применённой оптимизатором производной из первого момента
    /// свежего оптимизатора: после единственного обновления с нулевых моментов
    /// первый момент равен (1 - beta1) * производная, поэтому деление на эту долю
    /// возвращает саму производную. Пропуск деления даёт число в десять раз
    /// меньше и обязан ломать проверку согласия.
    ///
    /// Деление выполняется в одинарной точности, как и в хозяине опыта: сам
    /// момент хранится оптимизатором в одинарной точности, поэтому расширение
    /// до двойной точности ДО деления дало бы другое последнее знакоместо и
    /// разошлось бы с уже сохранёнными числами опыта примерно на одну
    /// стомиллионную долю.
    /// </summary>
    /// <param name="firstMoment">Первый момент оптимизатора после единственного обновления.</param>
    /// <param name="beta1">Первый коэффициент моментов, взятый у самого обучателя.</param>
    private static double RecoverAppliedDerivative(float firstMoment, float beta1)
        => firstMoment / (1f - beta1); // Доля градиента в первом моменте снимается ровно один раз.

    /// <summary>Та же продуктовая сумма логарифмических вероятностей размеченного ответа.</summary>
    /// <param name="logits">Числа, полученные прямым проходом модели.</param>
    /// <param name="ids">Номера знаков того же входа.</param>
    /// <param name="mask">Маска позиций ответа этой стороны пары.</param>
    private static float TargetLogProbs(Tensor logits, Tensor ids, Tensor mask)
        => (float)ProductComputeTargetLogProbs.Invoke(null, [logits, ids, mask, true])!; // Точный материал суммирует ответ.

    /// <summary>Та же продуктовая точная величина потерь и её производная по запасу предпочтения.</summary>
    /// <param name="margin">Запас предпочтения между сторонами пары.</param>
    private static (float Loss, float Derivative) ExactObjective(float margin)
        => ((float, float))ProductExactObjective.Invoke(null, [margin])!; // Берётся сам продуктовый член, не его копия.

    /// <summary>
    /// Сторож нулевого внешнего верхнего градиента на уже построенном графе копии:
    /// настоящие числа градиента помечаются узнаваемым образцом, затем обратный
    /// проход с нулём обязан не изменить ни одного числа.
    /// </summary>
    /// <param name="model">Копия старта, граф которой уже построен.</param>
    /// <param name="chosenLogits">Числа предпочтительной стороны этого графа.</param>
    /// <param name="rejectedLogits">Числа отклонённой стороны этого графа.</param>
    private static (int ChangedArrays, double MaxAbsoluteChange) RunZeroUpstreamControl(
        LlmModel model, Tensor chosenLogits, Tensor rejectedLogits)
    {
        var parameters = model.Parameters().Where(parameter => parameter.Value.RequiresGrad).ToArray(); // Обучаемые массивы копии.
        var markers = new List<(Parameter Parameter, float[] Marker)>(parameters.Length); // Узнаваемые образцы по каждому массиву.
        foreach (var parameter in parameters)
        {
            var marker = new float[parameter.Value.Data.Length]; // Образец вместо настоящих чисел градиента.
            for (var index = 0; index < marker.Length; index++) marker[index] = 0.25f + index * 0.03125f; // Точно представимые числа.
            parameter.Value.Grad = new Tensor((float[])marker.Clone(), parameter.Value.Shape); // Образец кладётся на место градиента.
            markers.Add((parameter, marker)); // Образец запоминается для сверки.
        }

        chosenLogits.Backward(new Tensor(new float[chosenLogits.Data.Length], chosenLogits.Shape)); // Первый контрольный проход.
        rejectedLogits.Backward(new Tensor(new float[rejectedLogits.Data.Length], rejectedLogits.Shape)); // Второй контрольный проход.

        var changedArrays = 0; // Сколько массивов изменилось вопреки нулю.
        var maxAbsoluteChange = 0d; // Наибольшее изменение одного числа.
        foreach (var (parameter, marker) in markers)
        {
            var current = parameter.Value.Grad!.Data; // Числа градиента после контрольного прохода.
            var changed = false; // Изменился ли этот массив.
            for (var index = 0; index < marker.Length; index++)
            {
                maxAbsoluteChange = Math.Max(maxAbsoluteChange, Math.Abs((double)current[index] - marker[index])); // Изменение числа.
                if (BitConverter.SingleToInt32Bits(current[index]) != BitConverter.SingleToInt32Bits(marker[index])) changed = true;
            }

            if (changed) changedArrays++; // Этот массив изменился вопреки нулю.
        }

        foreach (var parameter in parameters) parameter.Value.Grad = null; // Копия после контроля остаётся чистой.
        return (changedArrays, maxAbsoluteChange); // Итог сторожа для видимой проверки.
    }

    /// <summary>Именованные привязки только фактически присоединённых адаптеров запроса и значения.</summary>
    /// <param name="model">Обучаемая модель, которой принадлежат адаптеры.</param>
    /// <param name="adapters">Присоединённые адаптеры этой модели.</param>
    private static IReadOnlyList<AdamWParameterBinding> AdapterBindings(LlmModel model, IReadOnlyList<LoraLinear> adapters)
    {
        var allowed = adapters.SelectMany(adapter => adapter.AdapterParameters).Select(entry => entry.Parameter)
            .ToHashSet(ReferenceEqualityComparer.Instance); // Параметры адаптеров по ссылочной тождественности.
        return model.GetCheckpointParameterEntries().Where(entry => allowed.Contains(entry.Parameter))
            .Select(entry => new AdamWParameterBinding(entry.Key, entry.Parameter))
            .OrderBy(binding => binding.Key, StringComparer.Ordinal).ToArray(); // Привязки в закреплённом порядке.
    }

    /// <summary>Верный ответ считается из самого запроса обычной целочисленной разностью.</summary>
    /// <param name="prompt">Запрос вида «C# result sign: 9-4 =».</param>
    private static string GoldFromPrompt(string prompt)
    {
        const string head = "C# result sign: "; // Неизменная голова запроса этого профиля.
        const string tail = " ="; // Неизменный хвост запроса этого профиля.
        if (!prompt.StartsWith(head, StringComparison.Ordinal) || !prompt.EndsWith(tail, StringComparison.Ordinal))
            throw new InvalidOperationException("COMPOSED_DPO_GRADIENT_PROMPT_SHAPE:" + prompt); // Чужая форма запроса — отказ.
        var body = prompt[head.Length..^tail.Length]; // Тело запроса вида «9-4».
        var separator = body.IndexOf('-'); // Знак вычитания отделяет уменьшаемое от вычитаемого.
        if (separator <= 0) throw new InvalidOperationException("COMPOSED_DPO_GRADIENT_PROMPT_NO_MINUS:" + body);
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

    /// <summary>Путь проверенного обученного старта, из которого начинался этот опыт.</summary>
    public static string SharedLearnedStartDirectory() => Path.Combine(
        RepositoryRoot(), "experiments-v2", "RP01-MICRO", "_shared-learned-starts", "h64-rms-row", "v1");

    /// <summary>Путь сохранённых сравнений производных этого опыта: источник ожидаемых чисел.</summary>
    public static string SavedComparisonPath() => Path.Combine(
        RepositoryRoot(), "experiments-v2", "RP01-MICRO", "P1001", "results", "attempt-0001", "derivative-comparison.json");

    /// <summary>Подъём от папки сборки до корня хранилища: пути не зависят от места запуска.</summary>
    private static string RepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory); // Папка собранных проверок.
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln"))) return current.FullName; // Корень найден.
            current = current.Parent; // Иначе поднимаемся ещё на уровень вверх.
        }

        throw new DirectoryNotFoundException("COMPOSED_DPO_GRADIENT_REPOSITORY_ROOT_NOT_FOUND"); // Корень не найден.
    }

    /// <summary>Мандат на изменение обучателя для проверки: та же продуктовая дверь, что у хозяина опыта.</summary>
    private sealed class TestLease : IDpoMutationAdmissionIssuer
    {
        /// <summary>Выдать живое разрешение на изменение обучателя.</summary>
        public IDisposable AcquireDpoTrainerMutationPermit() => new CancellationTokenSource();

        /// <summary>Освободить выданное разрешение.</summary>
        public void Dispose() { }
    }

    /// <summary>Удалить временный каталог контрольных точек после всех проверок класса.</summary>
    public void Dispose()
    {
        if (Directory.Exists(_checkpointRoot)) Directory.Delete(_checkpointRoot, recursive: true); // Временные байты не остаются.
    }
}

/// <summary>
/// Проверки производной родного обучения предпочтению: согласие с независимой
/// конечной разностью, правила восстановления, целостность замороженного и
/// эталонного состояния, состав панели и исполнение назначенной дозы.
/// </summary>
public sealed class ComposedDpoGradientTests : IClassFixture<ComposedDpoGradientFixture>
{
    /// <summary>Общая подготовка: один настоящий шаг и независимая конечная разность.</summary>
    private readonly ComposedDpoGradientFixture _measured;

    /// <summary>Канал человеческих показаний проверки.</summary>
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    /// <summary>Принять общую подготовку класса и канал показаний.</summary>
    /// <param name="measured">Общая подготовка, выполненная ровно один раз.</param>
    /// <param name="output">Канал человеческих показаний проверки.</param>
    public ComposedDpoGradientTests(ComposedDpoGradientFixture measured, Xunit.Abstractions.ITestOutputHelper output)
    {
        _measured = measured; // Готовые измерения общей подготовки.
        _output = output; // Канал, в который пишутся человеческие показания.
    }

    // ── Главное: аналитическая производная согласуется с конечной разностью ──────

    /// <summary>
    /// Производная, снятая с настоящего шага обучения предпочтению, совпадает с
    /// независимой центральной конечной разностью той же величины потерь на обеих
    /// закреплённых координатах и всех трёх значениях шага возмущения.
    /// </summary>
    [Fact]
    public void AnalyticDerivativeOfRealPreferenceStep_AgreesWithIndependentCentralDifference()
    {
        Assert.Equal(6, _measured.Rows.Count); // Две координаты на три значения шага возмущения.
        Assert.Equal(
            new[] { "q_proj", "q_proj", "q_proj", "v_proj", "v_proj", "v_proj" },
            _measured.Rows.Select(row => row.Coordinate).ToArray()); // Координаты закреплены заранее, а не подобраны.
        Assert.Equal(
            ComposedDpoGradientFixture.Epsilons.Concat(ComposedDpoGradientFixture.Epsilons).ToArray(),
            _measured.Rows.Select(row => row.Epsilon).ToArray()); // Исходный масштаб шага возмущения сохранён.

        foreach (var row in _measured.Rows)
        {
            // Погрешности пересчитываются независимо из исходных чисел строки, а не
            // принимаются из её же сохранённых полей.
            var recomputedAbsolute = Math.Abs(row.AnalyticDerivative - row.FiniteDifference); // Абсолютная погрешность заново.
            var scale = Math.Max(Math.Abs(row.AnalyticDerivative), Math.Abs(row.FiniteDifference)); // Масштаб сравнения.
            Assert.Equal(recomputedAbsolute, row.AbsoluteError, 15); // Сохранённое поле сходится с пересчётом.
            Assert.Equal(recomputedAbsolute / scale, row.RelativeError, 15); // И относительная погрешность тоже.

            // Согласие по каждому сравнению отдельно, а не «координата сошлась хотя бы раз».
            var agreed = row.NearZeroFiniteDifference
                ? row.AbsoluteError <= ComposedDpoGradientFixture.MaxAbsoluteTolerance // Околонулевая величина: только абсолютный допуск.
                : row.AbsoluteError <= ComposedDpoGradientFixture.MaxAbsoluteTolerance
                    || row.RelativeError <= ComposedDpoGradientFixture.MaxRelativeTolerance; // Иначе достаточно одного из двух.
            Assert.True(agreed, $"COMPOSED_DPO_GRADIENT_DISAGREES:{row.Coordinate} шаг {row.Epsilon}: "
                + $"аналитическая {row.AnalyticDerivative:R}, конечная разность {row.FiniteDifference:R}, "
                + $"абсолютная погрешность {row.AbsoluteError:R}, относительная {row.RelativeError:R}");

            // Направление сдвига обязано совпасть: согласие по величине без согласия
            // по знаку означало бы обучение в противоположную сторону.
            Assert.True(Math.Sign(row.AnalyticDerivative) == Math.Sign(row.FiniteDifference),
                $"COMPOSED_DPO_GRADIENT_SIGN_DIFFERS:{row.Coordinate} шаг {row.Epsilon}");
        }

        _output.WriteLine("Согласие производных: " + string.Join("; ", _measured.Rows.Select(row =>
            $"{row.Coordinate} шаг {row.Epsilon.ToString("E1", CultureInfo.InvariantCulture)} "
            + $"погрешность {row.AbsoluteError.ToString("E3", CultureInfo.InvariantCulture)}")));
    }

    // ── Правило снятия производной из первого момента ────────────────────────────

    /// <summary>
    /// Производная снимается из первого момента только при доказанном свежем
    /// оптимизаторе и известном первом коэффициенте моментов, а множитель вклада
    /// обучающего вектора снимается ровно один раз.
    /// </summary>
    [Fact]
    public void FirstMomentRecovery_RequiresFreshOptimizer_AndRemovesVectorSignalExactlyOnce()
    {
        Assert.Equal(0, _measured.OptimizerStepBefore); // Свежий оптимизатор предпочтений начинает с нуля.
        Assert.Equal(1, _measured.OptimizerStepAfter); // Единственное обновление переводит счётчик 0 -> 1.
        Assert.Equal(0.9f, _measured.Beta1); // Первый коэффициент моментов взят у самого обучателя.
        Assert.Equal(0.95f, _measured.Beta2); // Второй коэффициент моментов взят у самого обучателя.
        Assert.Equal(DpoExactPairMaterial.ObjectiveVersion, _measured.ObjectiveVersion); // Шаг использовал точную функцию потерь.

        Assert.Equal(1f, _measured.VectorWeight); // Единственная пара набора несёт полный вес.
        Assert.Equal(1.0833334f, _measured.VectorSignal); // Сила вклада обучающего вектора этой пары.
        Assert.Equal(_measured.VectorWeight * _measured.VectorSignal, _measured.ScaledWeight); // Общий множитель производной.

        foreach (var coordinate in _measured.Coordinates)
        {
            // Правило восстановления: первый момент свежего оптимизатора равен доле
            // (1 - beta1) от производной, поэтому деление на эту долю её возвращает.
            // Деление идёт в одинарной точности, в которой момент и хранится.
            Assert.Equal(
                coordinate.FirstMoment / (1f - _measured.Beta1),
                (float)coordinate.DerivativeAsAppliedByOptimizer);

            // Независимая обратная сверка умножением, а не повтор того же деления:
            // доля (1 - beta1) от восстановленной производной обязана вернуть
            // сохранённый оптимизатором первый момент.
            Assert.Equal(
                (double)coordinate.FirstMoment,
                (1d - _measured.Beta1) * coordinate.DerivativeAsAppliedByOptimizer,
                8);

            // Множитель вклада снимается ровно один раз: отношение применённой
            // производной к производной самой величины потерь равно этому множителю.
            Assert.Equal(
                (double)_measured.ScaledWeight,
                coordinate.DerivativeAsAppliedByOptimizer / coordinate.AnalyticDerivative,
                6);

            // Снять множитель дважды — другое число: двойное снятие обязано ломать согласие.
            var removedTwice = coordinate.AnalyticDerivative / _measured.ScaledWeight; // Что получилось бы при двойном снятии.
            var worstAbsoluteError = _measured.Rows
                .Where(row => row.Coordinate == coordinate.Coordinate)
                .Max(row => row.AbsoluteError); // Худшая погрешность этой координаты при верном снятии.
            Assert.True(
                Math.Abs(removedTwice - coordinate.AnalyticDerivative) > worstAbsoluteError,
                $"COMPOSED_DPO_GRADIENT_DOUBLE_REMOVAL_INDISTINGUISHABLE:{coordinate.Coordinate}");

            // Пропуск деления на (1 - beta1) — тоже другое число, и тоже заметно.
            Assert.True(
                Math.Abs((double)coordinate.FirstMoment - coordinate.DerivativeAsAppliedByOptimizer) > worstAbsoluteError,
                $"COMPOSED_DPO_GRADIENT_MISSING_BETA1_DIVISION_INDISTINGUISHABLE:{coordinate.Coordinate}");
        }

        _output.WriteLine("Снятие производной: " + string.Join("; ", _measured.Coordinates.Select(coordinate =>
            $"{coordinate.Coordinate} первый момент {coordinate.FirstMoment.ToString("E9", CultureInfo.InvariantCulture)}, "
            + $"применённая {coordinate.DerivativeAsAppliedByOptimizer.ToString("E9", CultureInfo.InvariantCulture)}, "
            + $"своя {coordinate.AnalyticDerivative.ToString("E9", CultureInfo.InvariantCulture)}")));
    }

    // ── Второй момент: проверка согласованности, а не вторая конечная разность ───

    /// <summary>
    /// Второй момент подтверждает ВЕЛИЧИНУ той же производной. Это проверка
    /// согласованности двух восстановлений из одного и того же обратного прохода,
    /// а не независимая вторая проверка конечной разностью: знак второй момент не
    /// несёт и новой информации о правильности производной не даёт.
    /// </summary>
    [Fact]
    public void SecondMoment_ConfirmsMagnitudeAsConsistencyCheck_NotAnIndependentFiniteDifference()
    {
        foreach (var coordinate in _measured.Coordinates)
        {
            // Величина, восстановленная вторым моментом, совпадает с величиной
            // производной, восстановленной первым моментом.
            Assert.Equal(
                Math.Abs(coordinate.AnalyticDerivative),
                coordinate.MagnitudeFromSecondMoment,
                7);

            // Второй момент неотрицателен и знака не несёт: по нему нельзя узнать
            // направление сдвига, поэтому независимой проверкой он быть не может.
            Assert.True(coordinate.SecondMoment >= 0f, $"COMPOSED_DPO_GRADIENT_SECOND_MOMENT_NEGATIVE:{coordinate.Coordinate}");
            Assert.True(coordinate.MagnitudeFromSecondMoment > 0d, $"COMPOSED_DPO_GRADIENT_SECOND_MOMENT_ZERO:{coordinate.Coordinate}");

            // Оба восстановления приходят из одного обратного прохода, поэтому их
            // согласие не заменяет конечную разность: та считается отдельно и на
            // отдельных копиях старта.
            Assert.Contains(_measured.Rows, row => row.Coordinate == coordinate.Coordinate && row.FiniteDifference != 0d);
        }
    }

    // ── Околонулевое сравнение опирается на абсолютный допуск ────────────────────

    /// <summary>
    /// Признак околонулевой конечной разности выводится из самого числа и порога,
    /// а не выставляется вручную; при околонулевой величине сравнение опирается
    /// только на абсолютный допуск, и все шесть сравнений этот строгий допуск
    /// выдерживают сами по себе.
    /// </summary>
    [Fact]
    public void NearZeroComparison_IsDerivedFromThreshold_AndLeansOnAbsoluteToleranceOnly()
    {
        foreach (var row in _measured.Rows)
        {
            // Признак пересчитывается из величины конечной разности и порога.
            Assert.Equal(
                Math.Abs(row.FiniteDifference) < ComposedDpoGradientFixture.NearZeroThreshold,
                row.NearZeroFiniteDifference);

            // Даже под самым строгим правилом — только абсолютный допуск — сравнение проходит.
            Assert.True(
                row.AbsoluteError <= ComposedDpoGradientFixture.MaxAbsoluteTolerance,
                $"COMPOSED_DPO_GRADIENT_ABSOLUTE_TOLERANCE_EXCEEDED:{row.Coordinate} шаг {row.Epsilon}: {row.AbsoluteError:R}");
        }

        // В этом масштабе ни одна конечная разность околонулевой не оказалась:
        // особое правило существует, но текущие числа его не используют.
        Assert.Equal(0, _measured.Rows.Count(row => row.NearZeroFiniteDifference));
        Assert.All(_measured.Rows, row => Assert.True(
            Math.Abs(row.FiniteDifference) > ComposedDpoGradientFixture.NearZeroThreshold));
    }

    // ── Целостность замороженного, эталонного и считывающего пути ────────────────

    /// <summary>
    /// Настоящий шаг изменил только разрешённые адаптеры: замороженная основа и
    /// эталонная модель остались байт в байт теми же, чтение панели состояние не
    /// меняло, а нулевой внешний верхний градиент не изменил ни одного числа.
    /// </summary>
    [Fact]
    public void FrozenBaseReferenceAndReadout_StayIntactWhileOnlyAdaptersMove()
    {
        Assert.Equal(_measured.FrozenBeforeSha256, _measured.FrozenAfterSha256); // Замороженная основа не изменилась.
        Assert.Equal(_measured.ReferenceBeforeSha256, _measured.ReferenceAfterSha256); // Эталонная модель не изменилась.
        Assert.NotEqual(_measured.AdaptersBeforeSha256, _measured.AdaptersAfterSha256); // Адаптеры настоящим шагом изменились.
        Assert.True(_measured.BeforePanelStateStable, "COMPOSED_DPO_GRADIENT_STATE_CHANGED_DURING_BEFORE_PANEL");
        Assert.True(_measured.AfterPanelStateStable, "COMPOSED_DPO_GRADIENT_STATE_CHANGED_DURING_AFTER_PANEL");

        Assert.Equal(0, _measured.ZeroUpstreamChangedArrays); // Нулевой верхний градиент не изменил ни одного массива.
        Assert.Equal(0d, _measured.ZeroUpstreamMaxAbsoluteChange); // И ни одного числа внутри них.
        Assert.Equal(
            ComposedDpoGradientFixture.PlannedFiniteDifferenceEvaluations,
            _measured.RestoredExactlyCount); // Каждая копия после восстановления совпала с нетронутой точно.
    }

    // ── Состав панели: два верных ответа из четырёх, как и у тривиального ответа ─

    /// <summary>
    /// Панель из четырёх строк даёт два верных ответа и до шага, и после него.
    /// Ровно столько же даёт тривиальный постоянный ответ «плюс», поэтому этот
    /// состав не является доказательством прироста качества.
    /// </summary>
    [Fact]
    public void ReadoutPanel_KeepsTwoOfFourAndEqualsTheTrivialConstantBaseline()
    {
        Assert.Equal(4, _measured.PanelBefore.Count); // Панель ровно из четырёх строк.
        Assert.Equal(4, _measured.PanelAfter.Count); // И после шага столько же.
        Assert.Equal(2, _measured.PanelBefore.Count(row => row.Correct)); // Два верных ответа до шага.
        Assert.Equal(2, _measured.PanelAfter.Count(row => row.Correct)); // Два верных ответа после шага.

        // Тривиальный постоянный ответ «плюс» объясняет тот же состав, поэтому
        // сам по себе он ничего не говорит о качестве обученной модели.
        Assert.Equal(2, _measured.PanelAfter.Count(row => row.Gold == "+"));

        // Ответы до и после совпали построчно: сохранение, а не прирост.
        Assert.Equal(
            _measured.PanelBefore.Select(row => row.Answer).ToArray(),
            _measured.PanelAfter.Select(row => row.Answer).ToArray());

        // Учебная строка единственного шага осталась верной и до, и после.
        Assert.True(_measured.PanelBefore.Single(row => row.Id == "T1").Correct);
        Assert.True(_measured.PanelAfter.Single(row => row.Id == "T1").Correct);

        _output.WriteLine("Панель после шага: " + string.Join("; ", _measured.PanelAfter.Select(row =>
            $"{row.Id} ответ «{row.Answer}», верный «{row.Gold}», совпало {(row.Correct ? "да" : "нет")}")));
    }

    // ── Назначенная доза исполнена ровно ─────────────────────────────────────────

    /// <summary>
    /// Назначенный бюджет исполнен ровно: тридцать шесть прямых проходов, четыре
    /// обратных, одно обновление, восемь жадных выдач и двенадцать вычислений
    /// величины потерь конечной разностью, без прогрева с нуля.
    /// </summary>
    [Fact]
    public void AssignedBudget_IsSpentExactly()
    {
        Assert.Equal(ComposedDpoGradientFixture.PlannedModelForwards, _measured.ModelForwards); // Тридцать шесть прямых проходов.
        Assert.Equal(ComposedDpoGradientFixture.PlannedBackwards, _measured.Backwards); // Четыре обратных прохода.
        Assert.Equal(ComposedDpoGradientFixture.PlannedUpdates, _measured.Updates); // Одно обновление оптимизатора.
        Assert.Equal(ComposedDpoGradientFixture.PlannedGenerations, _measured.Generations); // Восемь жадных выдач.
        Assert.Equal(
            ComposedDpoGradientFixture.PlannedFiniteDifferenceEvaluations,
            _measured.FiniteDifferenceEvaluations); // Двенадцать вычислений величины потерь.
        Assert.Equal(ComposedDpoGradientFixture.PlannedBootstrapRuns, _measured.BootstrapRuns); // Прогрева с нуля не было.

        // Жадные выдачи дали прямые проходы панели и повторно к ним не прибавляются:
        // восемь выдач плюс четыре прохода шага плюс двадцать четыре прохода конечной
        // разности дают ровно тридцать шесть.
        Assert.Equal(
            _measured.Generations + 4 + (ComposedDpoGradientFixture.PlannedFiniteDifferenceEvaluations * 2),
            _measured.ModelForwards);
    }

    // ── Связь с уже сохранёнными доказательствами опыта ──────────────────────────

    /// <summary>
    /// Живые числа этой проверки воспроизводят уже сохранённые сравнения опыта:
    /// ожидаемые числа берутся из сохранённого файла, а не из вывода нового кода,
    /// и там же независимо пересчитываются из исходных величин каждой строки.
    /// </summary>
    [Fact]
    public void LiveComparisons_ReproduceSavedExperimentEvidence()
    {
        using var saved = JsonDocument.Parse(File.ReadAllBytes(ComposedDpoGradientFixture.SavedComparisonPath()));
        var savedRows = saved.RootElement.GetProperty("rows").EnumerateArray().Select(row => new
        {
            Coordinate = row.GetProperty("coordinate").GetString()!, // Какая координата сравнивалась.
            Epsilon = row.GetProperty("epsilon").GetDouble(), // На каком шаге возмущения.
            Analytic = row.GetProperty("analytic_derivative").GetDouble(), // Аналитическая производная.
            FiniteDifference = row.GetProperty("finite_difference").GetDouble(), // Конечная разность.
            AbsoluteError = row.GetProperty("absolute_error").GetDouble(), // Сохранённая абсолютная погрешность.
            RelativeError = row.GetProperty("relative_error").GetDouble() // Сохранённая относительная погрешность.
        }).ToArray();
        Assert.Equal(_measured.Rows.Count, savedRows.Length); // Состав сравнений обязан совпасть.

        // Допуски сравнения берутся из самого сохранённого файла, а не выдумываются здесь.
        var savedTolerances = saved.RootElement.GetProperty("tolerances"); // Раздел зарегистрированных допусков.
        Assert.Equal(ComposedDpoGradientFixture.MaxAbsoluteTolerance, savedTolerances.GetProperty("max_absolute").GetDouble());
        Assert.Equal(ComposedDpoGradientFixture.MaxRelativeTolerance, savedTolerances.GetProperty("max_relative").GetDouble());
        Assert.Equal(ComposedDpoGradientFixture.NearZeroThreshold, savedTolerances.GetProperty("near_zero_threshold").GetDouble());

        // Множитель вклада обучающего вектора записан в том же файле и обязан совпасть.
        // Сравнение идёт в одинарной точности: именно в ней множитель вычислен
        // продуктовой формулой и записан в сохранённый файл, поэтому расширение до
        // двойной точности сравнивало бы разные записи одного и того же числа.
        Assert.Equal(
            _measured.VectorSignal,
            (float)saved.RootElement.GetProperty("derivative_scaling").GetProperty("vector_signal").GetDouble());

        foreach (var savedRow in savedRows)
        {
            // Сохранённые поля независимо пересчитываются из исходных чисел той же строки.
            var recomputedAbsolute = Math.Abs(savedRow.Analytic - savedRow.FiniteDifference); // Абсолютная погрешность заново.
            Assert.Equal(recomputedAbsolute, savedRow.AbsoluteError, 15); // Сохранённое поле сходится с пересчётом.
            Assert.Equal(
                recomputedAbsolute / Math.Max(Math.Abs(savedRow.Analytic), Math.Abs(savedRow.FiniteDifference)),
                savedRow.RelativeError,
                15); // И относительная погрешность тоже.

            // Живая строка того же сравнения обязана воспроизвести сохранённые числа.
            var live = _measured.Rows.Single(row => row.Coordinate == savedRow.Coordinate && row.Epsilon == savedRow.Epsilon);
            Assert.Equal(savedRow.Analytic, live.AnalyticDerivative, 12); // Аналитическая производная воспроизведена.
            Assert.Equal(savedRow.FiniteDifference, live.FiniteDifference, 12); // Конечная разность воспроизведена.
        }

        // Производные числа точности считает ДЕЙСТВУЮЩИЙ общий построитель, который
        // компилируется и в хозяина опыта, и сюда: правило сверки не повторяется здесь.
        var figures = P1001DerivativeAccuracySummary.Build(
            _measured.AsSummaryComparisons(),
            ComposedDpoGradientFixture.MaxAbsoluteTolerance,
            ComposedDpoGradientFixture.MaxRelativeTolerance,
            ComposedDpoGradientFixture.NearZeroThreshold);
        Assert.Equal(6, figures.ComparisonCount); // Шесть сравнений вошли в расчёт.
        Assert.Equal(6, figures.AgreedComparisonCount); // Все шесть уложились в допуск по отдельности.
        Assert.True(figures.AllComparisonsAgreed); // Согласие считается по каждому сравнению.
        Assert.True(figures.EveryCoordinateAgreedOnSomeEpsilon); // Прежний более слабый вердикт тоже держится.
        Assert.Equal(0, figures.NearZeroComparisonCount); // Околонулевых конечных разностей нет.
        Assert.False(figures.NearZeroRuleApplied); // Особое правило околонулевой величины не применялось.

        // Худшее сравнение и запас по абсолютному допуску совпали с принятой поправкой:
        // запас равен 3,713, а не заявленным когда-то «не менее четырёх раз».
        Assert.Equal("v_proj[0] шаг 1.0E-003", figures.MaxAbsoluteErrorRow); // Худшим осталось то же сравнение.
        Assert.Equal(2.6932268261364856e-05, figures.MaxAbsoluteError, 12); // И та же наибольшая погрешность.
        Assert.Equal(3.7130181175067603, figures.AbsoluteToleranceMarginFactor, 6); // И тот же запас по допуску.
        Assert.True(figures.AbsoluteToleranceMarginFactor < 4d, "COMPOSED_DPO_GRADIENT_MARGIN_OVERSTATED");

        _output.WriteLine(
            $"Наибольшая абсолютная погрешность {figures.MaxAbsoluteError.ToString("E6", CultureInfo.InvariantCulture)} "
            + $"на сравнении {figures.MaxAbsoluteErrorRow}; запас по абсолютному допуску "
            + $"{figures.AbsoluteToleranceMarginFactor.ToString("F6", CultureInfo.InvariantCulture)} раза.");
    }

    // ── Граница вывода записывается кодом, а не только текстом отчёта ────────────

    /// <summary>
    /// Эта проверка покрывает две заранее закреплённые координаты на одной учебной
    /// строке одного профиля. Она не доказывает все координаты, многознаковую
    /// маску ответа, другие предметные области и не вводит нового продуктового
    /// алгоритма: это регрессия уже работающего кода.
    /// </summary>
    [Fact]
    public void EvidenceBoundary_IsTwoPinnedCoordinatesOfOneLessonNotAGeneralLaw()
    {
        Assert.Equal(2, _measured.Coordinates.Count); // Проверены ровно две координаты.
        Assert.Equal(
            new[] { "lora.block_0000.q_proj.lora_b", "lora.block_0000.v_proj.lora_b" },
            _measured.Coordinates.Select(coordinate => coordinate.BindingKey).ToArray()); // Именно эти привязки.

        // Ответ занимает ровно один знак: многознаковая маска ответа здесь не проверена.
        Assert.All(_measured.PanelAfter, row => Assert.Equal(1, row.Answer.Length));

        // Старт один, учебная строка одна, обновление одно: разброс не оценивался.
        Assert.Equal(1, _measured.Updates);
        Assert.Equal(ComposedDpoGradientFixture.StartWeightsSha256,
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(
                Path.Combine(ComposedDpoGradientFixture.SharedLearnedStartDirectory(), "S0.weights.bin"))))
                .ToLowerInvariant()); // Старт тот самый проверенный обученный, а не новая база.

        _output.WriteLine(
            "Граница вывода: две закреплённые координаты, одна учебная строка, один профиль, один старт. "
            + "Не доказаны все координаты, многознаковая маска ответа и другие предметные области. "
            + "Это регрессионное покрытие уже работающего кода, а не новый продуктовый алгоритм.");
    }
}
