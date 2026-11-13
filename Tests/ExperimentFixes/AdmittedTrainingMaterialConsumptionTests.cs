// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

// Что здесь проверяется, по-человечески.
//
// Что было неправильно. Проверка происхождения материала смотрела на строки доверенного
// подготовителя оценки, а обучение потребляло СВОИ наборы: пары предпочтений, общий учебный текст
// и корпус ближайших соседей. Эти две вещи не были связаны ничем. Поэтому можно было проверить
// одну копию, а обучать по другой, и проверка оставалась формальностью: расписка допуска нигде не
// читалась, а отбор допущенных обучающих строк не имел ни одного продуктового вызова.
//
// Что уже было закрыто ДО этой правки, и я этого не переписываю. Отказ самой проверки
// происхождения обучение останавливал: вызывающий смотрел признак успеха и возвращал отказ до
// разрешения на изменение весов. Открытой была именно СВЯЗЬ проверенного с потребляемым.
//
// Что стало. Материал приходит в обучение единственным носителем. Его конструктор закрыт, фабрика
// одна, и спросить подготовителя она решает сама. Сборка учебного стека и выполнение обучения
// предпочтениями принимают ТОЛЬКО этот носитель: довода с сырым текстом или сырым набором пар у
// них больше нет. Поэтому подменить содержание между проверкой и обучением нечем — это отнятие
// возможности, а не проверка вдогонку.
//
// Что доказано о связи, и что НЕ доказано. Доказано отрицательное: в том, что обучают, нет
// содержания отложенных строк замороженного источника — ни по точным байтам, ни по приведённому
// тексту, когда политика велит сравнивать и так. НЕ доказано положительное покрытие: что каждый
// потребляемый байт происходит из допущенной обучающей строки. Договора составления обучающих
// строк из строк источника в продукте пока нет, и выдумывать его здесь нельзя. Расписка называет
// это прямо своим полем доказанной связи.
//
// О «красном прогоне на прежней логике». Первая проверка красная по сути: прежний путь принимал
// честную расписку подготовителя и обучал на материале с экзаменационным содержанием. Третья
// проверка красная тем, что на прежнем коде она не компилируется вовсе: носителя допущенного
// материала не существовало, а входы обучения принимали сырой материал. Положительный контроль
// зелёный и до, и после.
//
// Прогона не было: прямые команды сборки и прогона тестов запрещены политикой разрешений проекта,
// и обходить запрет оболочкой-посредником я не стал.
public sealed class AdmittedTrainingMaterialConsumptionTests
{
    // Текст экзаменационной строки замороженного источника: именно он не имеет права попасть в
    // обучение ни теми же байтами, ни под новым именем.
    private const string HeldOutExamText = "Before implementing code, review it";

    // Обычный учебный текст одной стороны пары: он к отложенному материалу не относится.
    private const string TrainText = "Review the code before merging";

    // ── Проверка 1: отложенное содержание в том, что обучают, отвергнуто ──────

    [Fact]
    public void HeldOutContentInsideTheConsumedPreferences_IsRefusedBeforeLearning()
    {
        // Расписка подготовителя честная: его строки целиком повторяют замороженный источник.
        var provider = ProviderWithHonestLedger();

        // А вот то, что обучение фактически потребляет, несёт текст экзаменационной строки —
        // причём в НЕПРАВИЛЬНОЙ стороне пары. Место выбрано намеренно: общий учебный текст
        // собирается из вопросов, поэтому утечка здесь видна только проверкой самих сторон пары.
        var leakingPreferences = new[]
        {
            Pair(TrainText, "ok", "Так делать нельзя: " + HeldOutExamText)
        };

        var admitted = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            provider,
            "run-1",
            "documentation",
            leakingPreferences,
            CombinedTextOf(leakingPreferences));

        // Отказ приходит обычным конвертом с названным кодом, а не исключением.
        Assert.False(admitted.IsSuccess);
        Assert.Equal(
            CapabilityMaterialLineageGuard.ConsumedHeldOutContentErrorCode,
            admitted.ErrorCode);
        // Носителя наружу не выдано, значит обучению нечего получить.
        Assert.Null(admitted.Value);
        // Причина называет саму строку источника, её семью и место находки, а не только код.
        Assert.Contains("s5", admitted.ErrorMessage);
        Assert.Contains("g5", admitted.ErrorMessage);
        Assert.Contains("неправильный ответ пары 1", admitted.ErrorMessage);
        Assert.Contains("по точным байтам", admitted.ErrorMessage);
    }

    // ── Проверка 2: то же под новым именем и без знаков препинания ────────────

    [Fact]
    public void HeldOutContentUnderANewNameAndCase_IsRefusedByTheNormalizedComparison()
    {
        // Подготовитель честен, политика материала велит сравнивать и приведённый текст.
        var provider = ProviderWithHonestLedger();

        // Экзаменационная строка в нижнем регистре и без запятой — это та же строка.
        var aliasPreferences = new[]
        {
            Pair("before implementing code review it", "ok", "no")
        };

        var admitted = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            provider,
            "run-2",
            "documentation",
            aliasPreferences,
            CombinedTextOf(aliasPreferences));

        // Отказ тот же, и причина называет совпадение после приведения текста.
        Assert.False(admitted.IsSuccess);
        Assert.Equal(
            CapabilityMaterialLineageGuard.ConsumedHeldOutContentErrorCode,
            admitted.ErrorCode);
        Assert.Contains("после приведения текста", admitted.ErrorMessage);
    }

    // ── Проверка 3: материал, не прошедший проверку, до обучения не доходит ───

    [Fact]
    public void MaterialThatFailsLineage_YieldsNoCarrierAtAll()
    {
        // Подготовитель предъявляет честные отпечатки, но фактический текст строки изменён:
        // это тот самый вход, на котором опытный проверяющий выдавал приём.
        var material = Material();
        var honestRows = material.SourceRows.Select(row => ActualOf(row, row.Content)).ToArray();
        var tamperedRows = honestRows
            .Select((row, index) => index == 0 ? row with { Content = row.Content + " (изменено)" } : row)
            .ToArray();
        var provider = new FixedMaterialProvider(new CapabilityEvaluationMaterialSubmission(material, tamperedRows));

        // Потребляемый материал при этом безупречен: дело не в нём.
        var cleanPreferences = new[] { Pair(TrainText, "ok", "no") };

        var admitted = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            provider,
            "run-3",
            "documentation",
            cleanPreferences,
            CombinedTextOf(cleanPreferences));

        // Отказ по расхождению предъявленного отпечатка с посчитанным.
        Assert.False(admitted.IsSuccess);
        Assert.Equal(
            CapabilityMaterialLineageGuard.PresentedDigestMismatchErrorCode,
            admitted.ErrorCode);
        // И главное: носителя нет. Входы обучения принимают только носитель, поэтому материал без
        // пройденной проверки до обучателя не доходит вовсе.
        Assert.Null(admitted.Value);
    }

    // ── Проверка 4: положительный контроль, зелёный до и после ────────────────

    [Fact]
    public void HonestConsumedMaterial_IsAdmittedAndTheReceiptRecordsWhatWasBound()
    {
        // Честный подготовитель и честный потребляемый материал без отложенного содержания.
        var provider = ProviderWithHonestLedger();
        // Учебный набор ПОЛНЫЙ: все три разрешённые учебные строки источника доходят до обучателя.
        // Прежде здесь лежали две из трёх, и это принималось: тогда договор ещё не требовал
        // полноты разрешённого обучения. Требование добавлено этим срезом, поэтому оснастка
        // приведена к нему — ожидание не ослаблено, а дополнено третьей разрешённой строкой.
        var preferences = new[]
        {
            Pair(TrainText, "ok", "no"),
            Pair("Build the project before testing", "ok", "no"),
            Pair("Run the tests before pushing", "ok", "no")
        };
        var combinedText = CombinedTextOf(preferences);

        var admitted = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            provider,
            "run-4",
            "documentation",
            preferences,
            combinedText);

        // Материал допущен обычным успешным результатом.
        Assert.True(admitted.IsSuccess, admitted.ErrorMessage);
        var carrier = admitted.Value!;
        // Заметка названа словами: происхождение проверено и связано с потребляемым.
        Assert.Equal(
            CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.AdmittedWithLineageNote,
            carrier.EvaluationNote);
        // Носитель несёт ровно то, что было подано: обучение получит его, а не другой набор.
        Assert.Equal(combinedText, carrier.CombinedTrainingText);
        Assert.Equal(preferences.Length, carrier.PreferenceRowCount);
        // Выдача теперь сверяющая: она сначала проверяет тождество того, чем владеет носитель.
        var handedOutPairs = carrier.ReadPreferencesForLearner();
        Assert.True(handedOutPairs.IsSuccess, handedOutPairs.ErrorMessage);
        Assert.Equal(preferences.Select(pair => pair.Prompt), handedOutPairs.Value!.Select(pair => pair.Prompt));

        // Расписка читается и говорит, ЧТО именно было связано.
        Assert.NotNull(carrier.Receipt);
        Assert.Equal(preferences.Length, carrier.Receipt!.ConsumedPreferenceRowCount);
        // Доказаны обе половины связи: отложенного содержания в обучении нет И всё разрешённое
        // обучение до обучателя дошло. Прежнее название связи говорило только о первой половине.
        Assert.Equal(
            CapabilityMaterialAdmissionReceipt.ConsumedTrainCompleteAndHeldOutAbsent,
            carrier.Receipt.ConsumedMaterialBinding);
        // Отпечаток потребляемых байтов записан и совпадает с посчитанным по тем же байтам.
        Assert.Equal(ExpectedConsumedDigest(preferences, combinedText), carrier.Receipt.ConsumedMaterialSha256);
    }

    // ── Четвёртый раздел ремонта: перевод строки внутри поля больше не сдвигает границы ──
    //
    // Что было неправильно. Отпечаток потребляемого материала склеивал общий текст и три поля
    // каждой пары через перевод строки БЕЗ экранирования. Перевод строки внутри вопроса или
    // внутри предпочитаемой стороны — обычное содержание, поэтому два РАЗНЫХ набора пар давали
    // один и тот же текст и один отпечаток. Отпечаток, который не различает разный материал, не
    // доказывает, что обучали именно допущенное.
    //
    // Прежнее правило воспроизведено функцией ОТ ВХОДА (PriorNewlineConsumedDigest): она
    // принимает тот же материал и считает прежний ответ. Утверждение двойное: прежнее правило на
    // том же входе отвечает неверно, действующий продуктовый путь — верно.
    [Fact]
    public void NewlineInsideAFieldFooledThePriorDigest_AndIsSeparatedNow()
    {
        // Один и тот же честный подготовитель для обоих наборов.
        var provider = ProviderWithHonestLedger();
        // Общий учебный текст задан одинаковым нарочно: иначе наборы разошлись бы уже по нему. Он
        // несёт все три разрешённые учебные строки, потому что договор теперь требует полноты
        // разрешённого обучения; на сравнение отпечатков это не влияет — текст у обоих наборов
        // один и тот же.
        const string sharedCombinedText =
            "Review the code before merging Build the project before testing Run the tests before pushing";

        // Набор 1: перевод строки лежит внутри ВОПРОСА.
        var newlineInPrompt = new[] { Pair("Review the code\nbefore merging", "ok", "no") };
        // Набор 2: ровно те же куски разложены иначе — перевод строки внутри ПРЕДПОЧИТАЕМОЙ
        // стороны, а вопрос короче.
        var newlineInChosen = new[] { Pair("Review the code", "before merging\nok", "no") };

        // Прежнее правило на том же входе отвечает НЕВЕРНО: материал разный, отпечаток один.
        Assert.Equal(
            PriorNewlineConsumedDigest(newlineInPrompt, sharedCombinedText),
            PriorNewlineConsumedDigest(newlineInChosen, sharedCombinedText));

        // Действующий продуктовый путь отвечает верно: отпечатки разные.
        var firstAdmitted = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            provider, "run-newline-1", "documentation", newlineInPrompt, sharedCombinedText);
        Assert.True(firstAdmitted.IsSuccess, firstAdmitted.ErrorMessage);
        var secondAdmitted = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            provider, "run-newline-2", "documentation", newlineInChosen, sharedCombinedText);
        Assert.True(secondAdmitted.IsSuccess, secondAdmitted.ErrorMessage);

        var firstDigest = firstAdmitted.Value!.Receipt!.ConsumedMaterialSha256;
        var secondDigest = secondAdmitted.Value!.Receipt!.ConsumedMaterialSha256;
        Assert.NotEqual(firstDigest, secondDigest);

        // Отпечаток продуктового пути совпадает с посчитанным проверкой самостоятельно по тому
        // же однозначному правилу: связь нового правила с продуктом названа, а не предположена.
        Assert.Equal(ExpectedConsumedDigest(newlineInPrompt, sharedCombinedText), firstDigest);

        // Честный повтор того же материала даёт ТОТ ЖЕ отпечаток: правило остаётся
        // воспроизводимым, и разность берётся только из разного материала.
        var repeated = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            provider, "run-newline-3", "documentation", newlineInPrompt, sharedCombinedText);
        Assert.True(repeated.IsSuccess, repeated.ErrorMessage);
        Assert.Equal(firstDigest, repeated.Value!.Receipt!.ConsumedMaterialSha256);

        // И прежнее правило действительно зависит от входа: на материале без перевода строки его
        // ответ другой.
        Assert.NotEqual(
            PriorNewlineConsumedDigest(newlineInPrompt, sharedCombinedText),
            PriorNewlineConsumedDigest([Pair("Review the code", "ok", "no")], sharedCombinedText));
    }

    // ── Проверка 5: прежний путь без материала оценки сохранён и назван ───────

    [Fact]
    public void WithoutAnyEvaluationMaterial_TheLegacyPathIsKeptAndSaidOutLoud()
    {
        // Подготовителя нет вовсе — это обычный разрешённый прежний путь.
        var preferences = new[] { Pair(TrainText, "ok", "no") };
        var combinedText = CombinedTextOf(preferences);

        var admitted = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            provider: null,
            "run-5",
            "documentation",
            preferences,
            combinedText);

        // Обучение продолжается: прежний путь не запрещён.
        Assert.True(admitted.IsSuccess, admitted.ErrorMessage);
        // Расписки нет, и это сказано честно, а не выдано за пройденную оценку.
        Assert.Null(admitted.Value!.Receipt);
        Assert.Equal(
            CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.NoLineageEvaluationNote,
            admitted.Value.EvaluationNote);
        // Потребляемый материал при этом донесён без изменений.
        Assert.Equal(combinedText, admitted.Value.CombinedTrainingText);
    }

    // ── Проверка 6: допущенный набор нельзя подменить после допуска ───────────

    [Fact]
    public void TheAdmittedPreferenceSet_CannotBeSwappedAfterAdmission()
    {
        // Полный разрешённый учебный набор, который вызывающий оставляет себе на руках.
        var callerPreferences = new[]
        {
            Pair(TrainText, "ok", "no"),
            Pair("Build the project before testing", "ok", "no"),
            Pair("Run the tests before pushing", "ok", "no")
        };
        var admitted = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            ProviderWithHonestLedger(),
            "run-6",
            "documentation",
            callerPreferences,
            CombinedTextOf(callerPreferences));
        Assert.True(admitted.IsSuccess, admitted.ErrorMessage);
        var carrier = admitted.Value!;

        // Атака 1: вызывающий подменяет пару в СВОЁМ массиве после допуска.
        callerPreferences[0] = Pair("Задание: " + HeldOutExamText, "ok", "no");
        // Носитель отвечает прежней парой: он сделал свою копию при допуске.
        var afterFirstAttack = carrier.ReadPreferencesForLearner();
        Assert.True(afterFirstAttack.IsSuccess, afterFirstAttack.ErrorMessage);
        Assert.Equal(TrainText, afterFirstAttack.Value![0].Prompt);

        // Атака 2: получатель подменяет пару в выданной ему копии.
        var handedOut = carrier.ReadPreferencesForLearner();
        Assert.True(handedOut.IsSuccess, handedOut.ErrorMessage);
        handedOut.Value![0] = Pair("Задание: " + HeldOutExamText, "ok", "no");
        // Следующий получатель снова получает допущенную пару, а не испорченную.
        var afterSecondAttack = carrier.ReadPreferencesForLearner();
        Assert.True(afterSecondAttack.IsSuccess, afterSecondAttack.ErrorMessage);
        Assert.Equal(TrainText, afterSecondAttack.Value![0].Prompt);
        // Каждый получатель получает свой массив: общий объект снова открыл бы ту же дверь.
        Assert.NotSame(handedOut.Value, afterSecondAttack.Value);
    }

    // ── Оснастка ──────────────────────────────────────────────────────────────

    // Строки замороженного источника: три учебные, одна промежуточной проверки и одна
    // экзаменационная. Те же строки, что у проверок самого сторожа происхождения.
    private static IReadOnlyList<CapabilitySourceRow> HonestSourceRows() =>
    [
        new("s1", "g1", CapabilityMaterialRole.Train, TrainText, "ok"),
        new("s2", "g2", CapabilityMaterialRole.Train, "Build the project before testing", "ok"),
        new("s3", "g3", CapabilityMaterialRole.Train, "Run the tests before pushing", "ok"),
        new("s4", "g4", CapabilityMaterialRole.Dev, "Check the result before returning", "ok"),
        new("s5", "g5", CapabilityMaterialRole.Test, HeldOutExamText, "ok")
    ];

    // Договор оценки с выбранным приведением текста и политикой независимых семей.
    private static CapabilityEvaluationMaterial Material() =>
        new(
            ProfileId: "p112-family-disjoint",
            SourceManifestSha256: "0000000000000000000000000000000000000000000000000000000000000000",
            SourceRows: HonestSourceRows(),
            NormalizationPolicy: CapabilityMaterialNormalizationPolicy.LowerCaseAndPunctuationStripped,
            IntersectionPolicy: CapabilityMaterialIntersectionPolicy.IndependentFamilies);

    // Подготовитель с честной распиской: его фактические строки повторяют источник.
    private static FixedMaterialProvider ProviderWithHonestLedger()
    {
        var material = Material();
        // Отпечатки считаются из самого содержания, как и положено честной расписке.
        var actual = material.SourceRows.Select(row => ActualOf(row, row.Content)).ToArray();
        return new FixedMaterialProvider(new CapabilityEvaluationMaterialSubmission(material, actual));
    }

    // Одна пара предпочтений обычной формы: вопрос, верная сторона, неверная сторона и вектор.
    private static LogicalDpoPreferenceExample Pair(string prompt, string chosen, string rejected)
        => new(prompt, chosen, rejected, [0.1f, 0.2f]);

    // Общий учебный текст собирается из вопросов пар — так же, как его собирает обучатель.
    private static string CombinedTextOf(LogicalDpoPreferenceExample[] preferences)
        => string.Join("\n", preferences.Select(pair => pair.Prompt));

    // Ожидаемый отпечаток потребляемых байтов, посчитанный проверкой САМОСТОЯТЕЛЬНО, по тому же
    // однозначному правилу, что выбрано четвёртым разделом ремонта: версия правила кодирования и
    // назначение потока, затем общий учебный текст, затем число пар, затем вопрос и обе стороны
    // каждой пары. У каждого поля длина в БАЙТАХ объявляется до его байтов, у отсутствующего
    // значения — своя метка.
    //
    // Почему ожидание изменено, а не ослаблено. Прежнее ожидание повторяло прежнее правило —
    // склейку через перевод строки — и было верным для него. Правило сменил независимый
    // контракт: раздел плана ремонта о однозначном кодировании полей перед хешированием. Поэтому
    // здесь переписано именно ПРАВИЛО счёта, а не ослаблено утверждение: проверка по-прежнему
    // считает ожидаемый отпечаток сама и не вызывает продуктовый код, иначе сравнение стало бы
    // сравнением кода с самим собой.
    private static string ExpectedConsumedDigest(
        LogicalDpoPreferenceExample[] preferences,
        string combinedTrainingText)
    {
        // Накопитель байтов ожидаемого потока.
        var bytes = new List<byte>();
        // Версия правила кодирования идёт первым полем потока.
        AppendExpectedField(bytes, "length-prefixed-utf8-be32-sha256-v1");
        // Назначение потока идёт вторым полем.
        AppendExpectedField(bytes, "capability-consumed-training-text");
        // Общий учебный текст идёт третьим полем.
        AppendExpectedField(bytes, combinedTrainingText);
        // Число пар входит в поток отдельной счётной величиной.
        AppendExpectedCount(bytes, preferences.Length);
        foreach (var pair in preferences)
        {
            // Вопрос и обе стороны пары — три отдельных поля с объявленной длиной.
            AppendExpectedField(bytes, pair.Prompt);
            AppendExpectedField(bytes, pair.Chosen);
            AppendExpectedField(bytes, pair.Rejected);
        }

        // Отпечаток считается тем же названным примитивом по всему потоку.
        return Convert.ToHexString(SHA256.HashData(bytes.ToArray())).ToLowerInvariant();
    }

    // Одно поле ожидаемого потока: метка наличия, затем длина в байтах, затем сами байты.
    private static void AppendExpectedField(List<byte> bytes, string? value)
    {
        if (value is null)
        {
            bytes.Add(0x00); // Отсутствующее значение несёт только свою метку.
            return;          // Длины и байтов у него нет.
        }

        var utf8 = Encoding.UTF8.GetBytes(value); // Байты значения.
        bytes.Add(0x01);                          // Метка присутствующего значения.
        AppendExpectedBigEndian(bytes, utf8.Length); // Длина в байтах до самих байтов.
        bytes.AddRange(utf8);                     // Сами байты значения.
    }

    // Одна счётная величина ожидаемого потока: метка числа и четыре байта значения.
    private static void AppendExpectedCount(List<byte> bytes, int value)
    {
        bytes.Add(0x02);                       // Метка целого числа.
        AppendExpectedBigEndian(bytes, value); // Четыре байта от старшего к младшему.
    }

    // Четыре байта целого числа от старшего к младшему.
    private static void AppendExpectedBigEndian(List<byte> bytes, int value)
    {
        bytes.Add((byte)((value >> 24) & 0xFF)); // Старший байт.
        bytes.Add((byte)((value >> 16) & 0xFF)); // Второй байт.
        bytes.Add((byte)((value >> 8) & 0xFF));  // Третий байт.
        bytes.Add((byte)(value & 0xFF));         // Младший байт.
    }

    /// <summary>
    /// ПРЕЖНЕЕ правило отпечатка потребляемого материала: общий текст и три поля каждой пары
    /// склеивались через перевод строки, без экранирования, а отсутствующая сторона приводилась к
    /// пустой строке. Настоящая функция от входа: ответ считается из предъявленного материала.
    /// Воспроизведено здесь только для сравнения.
    /// </summary>
    private static string PriorNewlineConsumedDigest(
        LogicalDpoPreferenceExample[] preferences,
        string combinedTrainingText)
    {
        // Ровно прежняя склейка: перевод строки после каждого поля.
        var builder = new StringBuilder();
        builder.Append(combinedTrainingText).Append('\n');
        foreach (var pair in preferences)
        {
            builder.Append(pair.Prompt ?? string.Empty).Append('\n')
                   .Append(pair.Chosen ?? string.Empty).Append('\n')
                   .Append(pair.Rejected ?? string.Empty).Append('\n');
        }

        // Отпечаток считался по байтам собранной строки.
        return Sha256(builder.ToString());
    }

    // Фактическая строка материала с честно посчитанными отпечатками.
    private static CapabilityActualMaterialRow ActualOf(CapabilitySourceRow sourceRow, string content)
        => new(
            ExampleId: "example-" + sourceRow.SourceRowId,
            ClaimedSourceRowId: sourceRow.SourceRowId,
            Role: sourceRow.Role,
            Content: content,
            PresentedContentSha256: Sha256(content),
            PresentedNormalizedSha256: Sha256(NormalizeLikeGuard(content)));

    // Sha256 считает отпечаток точных байтов текста так же, как это делает сторож.
    private static string Sha256(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    // NormalizeLikeGuard повторяет выбранное приведение текста: нижний регистр, знаки препинания
    // убраны, пробелы сжаты. Повторено здесь намеренно, чтобы проверка готовила вход сама.
    private static string NormalizeLikeGuard(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            if (char.IsPunctuation(character) || char.IsSymbol(character)) continue;
            builder.Append(char.IsWhiteSpace(character) ? ' ' : char.ToLowerInvariant(character));
        }

        return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    // Подготовитель-заглушка обычной формы: отдаёт заданную подачу или её отсутствие.
    private sealed class FixedMaterialProvider : ICapabilityEvaluationMaterialProvider
    {
        private readonly CapabilityEvaluationMaterialSubmission? _submission;

        internal FixedMaterialProvider(CapabilityEvaluationMaterialSubmission? submission)
        {
            // Подача запоминается как есть: заглушка ничего о ней не решает.
            _submission = submission;
        }

        // TryProvide отвечает обычным результатом операции, как настоящий подготовитель.
        public OperationResult<CapabilityEvaluationMaterialSubmission?> TryProvide(string runId, string domainTag)
            => OperationResult<CapabilityEvaluationMaterialSubmission?>.Ok(_submission);
    }
}
