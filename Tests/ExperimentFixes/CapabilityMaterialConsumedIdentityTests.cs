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
// Назначенный остаток переноса «обучатель потребляет именно проверенный материал» состоит из шести
// пунктов. Каждый пункт получает здесь РОВНО ОДНУ решающую проверку, и каждая из них падает, если
// сломать именно ту логику, которую она охраняет.
//
// 1. Сквозная идентичность корпуса ближайших соседей. Прежде корпус поиска собирался много позже
//    допуска, прямо из слепков повтора и планов ветвей, которых допуск вообще не видел. Поэтому
//    экзаменационная строка в корпусе не ловилась ничем: отказ смотрел только общий учебный текст
//    и стороны пар. Теперь корпус входит в допуск и читается обучателем поиска только из носителя.
// 2. Идентичность прочитанных байтов. Между допуском и чтением содержание подменить нечем, и
//    выдача сверяет отпечаток тождества. Прежний текстовый отпечаток изменение числа в учебном
//    векторе не мог даже представить — он считался только по тексту.
// 3. Полнота разрешённого учебного набора. Прежде подготовитель мог честно предъявить три
//    разрешённых учебных строки, а до обучателя доходили две, и расписка этого не замечала.
// 4. Сохраняемая квитанция. Прежде расписка жила ровно до выхода из метода обучения, то есть
//    связкой материала с обучением не была: доказательство обязано переживать перезапуск.
// 5. Однозначное кодирование перевода строки в отпечатке. Эта часть уже была закрыта отдельным
//    опубликованным коммитом для ТЕКСТОВОГО отпечатка; здесь то же свойство проверяется у нового
//    отпечатка тождества, чтобы новый отпечаток не внёс старую дыру заново.
// 6. Владение изменяемыми векторами. Учебный вектор лежит в записи обычным изменяемым массивом, и
//    копия массива записей отдавала тот же самый объект вектора. Вызывающий менял число в векторе
//    ПОСЛЕ допуска, и обучение получало другие числа.
//
// Чего эти проверки НЕ доказывают, названо прямо. Здесь нет ни одного обучения, ни одного прямого
// прохода и ни одного создания модели: проверяется допуск материала и выдача его обучению, то есть
// граница до обучения. Рост точности, полезность ответа и качество обученной памяти этим не
// доказываются и не заявляются.
//
// Ожидания заданы проверкой самостоятельно: ожидаемые отпечатки считаются здесь по тому же
// открытому правилу однозначного кодирования, а не берутся вызовом продуктового кода — иначе
// сравнение стало бы сравнением кода с самим собой. Сравнений с числовым допуском здесь нет вовсе:
// все сравнения точные — равенство отпечатков, равенство текста, равенство чисел вектора.
public sealed class CapabilityMaterialConsumedIdentityTests
{
    // Текст экзаменационной строки замороженного источника: он не имеет права попасть в обучение
    // ни теми же байтами, ни под новым именем, ни через корпус поиска.
    private const string HeldOutExamText = "Before implementing code, review it";

    // Три разрешённые учебные строки источника.
    private const string FirstTrainText = "Review the code before merging";
    private const string SecondTrainText = "Build the project before testing";
    private const string ThirdTrainText = "Run the tests before pushing";

    // ── Пункт 1: экзаменационная строка в корпусе поиска отвергнута ───────────

    [Fact]
    public void HeldOutContentInsideTheRetrievalCorpus_IsRefusedBeforeLearning()
    {
        // Подготовитель честен, и то, что обучают парами, безупречно и полно.
        var provider = ProviderWithHonestLedger();
        var preferences = CompleteTrainPreferences();

        // А вот корпус поиска несёт текст экзаменационной строки. Место выбрано намеренно: ни
        // общий учебный текст, ни стороны пар утечки не содержат, поэтому поймать её может только
        // проверка самого корпуса.
        var leakingCorpus = new[]
        {
            CorpusRow("e1", FirstTrainText, "ok"),
            CorpusRow("e2", "Смотри образец: " + HeldOutExamText, "ok")
        };

        var admitted = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            provider,
            "run-corpus-leak",
            "documentation",
            preferences,
            CombinedTextOf(preferences),
            leakingCorpus);

        // Отказ приходит обычным конвертом с названным кодом, а не исключением.
        Assert.False(admitted.IsSuccess);
        Assert.Equal(
            CapabilityMaterialLineageGuard.ConsumedHeldOutContentErrorCode,
            admitted.ErrorCode);
        // Носителя наружу не выдано, значит обучателю поиска нечего получить.
        Assert.Null(admitted.Value);
        // Причина называет саму строку источника, её семью и ИМЕННО корпус как место находки.
        Assert.Contains("s5", admitted.ErrorMessage);
        Assert.Contains("g5", admitted.ErrorMessage);
        Assert.Contains("корпуса поиска", admitted.ErrorMessage);

        // Контроль того же входа: без утечки в корпусе тот же материал допускается, и носитель
        // отдаёт РОВНО тот упорядоченный корпус, который прошёл допуск. Это и есть сквозная
        // идентичность: обучатель поиска читает допущенные строки, а не собирает свои.
        var cleanCorpus = new[]
        {
            CorpusRow("e1", FirstTrainText, "ok"),
            CorpusRow("e2", SecondTrainText, "ok")
        };
        var admittedClean = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            provider,
            "run-corpus-clean",
            "documentation",
            preferences,
            CombinedTextOf(preferences),
            cleanCorpus);
        Assert.True(admittedClean.IsSuccess, admittedClean.ErrorMessage);
        var handedOutCorpus = admittedClean.Value!.ReadRetrievalCorpusForLearner();
        Assert.True(handedOutCorpus.IsSuccess, handedOutCorpus.ErrorMessage);
        // Порядок и состав строк совпадают с допущенными: порядок у корпуса значим.
        Assert.Equal(
            cleanCorpus.Select(row => row.ExampleId + "|" + row.TrainingText + "|" + row.OutputText),
            handedOutCorpus.Value!.Select(row => row.ExampleId + "|" + row.TrainingText + "|" + row.OutputText));
        // И расписка называет число связанных строк корпуса, а не молчит о нём.
        Assert.Equal(cleanCorpus.Length, admittedClean.Value.Receipt!.ConsumedRetrievalCorpusRowCount);
    }

    // ── Пункт 2: отпечаток тождества различает то, что текстовый не видел ─────

    [Fact]
    public void TheIdentityDigest_SeesAVectorChangeThatTheTextDigestCannotRepresent()
    {
        // Один и тот же честный подготовитель и один и тот же полный учебный материал.
        var provider = ProviderWithHonestLedger();
        var first = CompleteTrainPreferences();
        // Второй набор отличается от первого РОВНО одним числом учебного вектора первой пары.
        var second = CompleteTrainPreferences();
        second[0] = second[0] with { TeachingVector = [0.1f, 0.3f] };

        var firstAdmitted = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            provider, "run-identity-1", "documentation", first, CombinedTextOf(first));
        var secondAdmitted = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            provider, "run-identity-2", "documentation", second, CombinedTextOf(second));
        Assert.True(firstAdmitted.IsSuccess, firstAdmitted.ErrorMessage);
        Assert.True(secondAdmitted.IsSuccess, secondAdmitted.ErrorMessage);

        // ТЕКСТОВЫЙ отпечаток у двух разных материалов ОДИН И ТОТ ЖЕ: он считается только по
        // тексту, поэтому изменение числа в векторе он не может даже представить. Это и есть
        // причина, по которой сверка тождества понадобилась отдельным отпечатком.
        Assert.Equal(
            firstAdmitted.Value!.Receipt!.ConsumedMaterialSha256,
            secondAdmitted.Value!.Receipt!.ConsumedMaterialSha256);

        // А отпечаток ТОЖДЕСТВА их различает.
        Assert.NotEqual(
            firstAdmitted.Value.Receipt.ConsumedIdentitySha256,
            secondAdmitted.Value.Receipt.ConsumedIdentitySha256);

        // Отпечаток тождества совпадает с посчитанным проверкой самостоятельно по тому же
        // открытому правилу: связь нового отпечатка с продуктом названа, а не предположена.
        Assert.Equal(
            ExpectedIdentityDigest(first, CombinedTextOf(first), []),
            firstAdmitted.Value.Receipt.ConsumedIdentitySha256);

        // Повтор того же материала даёт тот же отпечаток: правило воспроизводимо, и разность
        // берётся только из разного материала.
        var repeated = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            provider, "run-identity-3", "documentation", CompleteTrainPreferences(),
            CombinedTextOf(first));
        Assert.True(repeated.IsSuccess, repeated.ErrorMessage);
        Assert.Equal(
            firstAdmitted.Value.Receipt.ConsumedIdentitySha256,
            repeated.Value!.Receipt!.ConsumedIdentitySha256);

        // И сверяющее чтение действительно сверяет: оно выдаёт материал, чьё тождество совпало с
        // записанным при допуске.
        var read = firstAdmitted.Value.ReadPreferencesForLearner();
        Assert.True(read.IsSuccess, read.ErrorMessage);
        Assert.Equal(first.Length, read.Value!.Length);
    }

    // ── Пункт 3: потерянная учебная строка отвергнута ─────────────────────────

    [Fact]
    public void LosingOneAdmittedTrainRow_IsRefusedBeforeLearning()
    {
        // Подготовитель честен: он предъявляет все три разрешённые учебные строки.
        var provider = ProviderWithHonestLedger();

        // А до обучателя доходят только две из трёх: третья потеряна.
        var incomplete = new[]
        {
            Pair(FirstTrainText, "ok", "no"),
            Pair(SecondTrainText, "ok", "no")
        };

        var admitted = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            provider,
            "run-incomplete",
            "documentation",
            incomplete,
            CombinedTextOf(incomplete));

        // Отказ назван своим кодом: это потеря разрешённого материала, а не утечка отложенного.
        Assert.False(admitted.IsSuccess);
        Assert.Equal(
            CapabilityMaterialLineageGuard.ConsumedTrainIncompleteErrorCode,
            admitted.ErrorCode);
        Assert.Null(admitted.Value);
        // Причина называет точную потерянную строку, а не только код.
        Assert.Contains("example-s3", admitted.ErrorMessage);
        Assert.Contains("s3", admitted.ErrorMessage);

        // Контроль того же входа: с полным учебным набором тот же материал допускается, и расписка
        // называет обе доказанные половины связи.
        var complete = CompleteTrainPreferences();
        var admittedComplete = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            provider, "run-complete", "documentation", complete, CombinedTextOf(complete));
        Assert.True(admittedComplete.IsSuccess, admittedComplete.ErrorMessage);
        Assert.Equal(
            CapabilityMaterialAdmissionReceipt.ConsumedTrainCompleteAndHeldOutAbsent,
            admittedComplete.Value!.Receipt!.ConsumedMaterialBinding);
    }

    // ── Пункт 4: расписка сохраняется и читается обратно ──────────────────────

    [Fact]
    public void TheAdmissionReceipt_IsSavedToDiskAndReadBackIdentically()
    {
        // Честный допуск даёт расписку, которую и будем сохранять.
        var provider = ProviderWithHonestLedger();
        var preferences = CompleteTrainPreferences();
        var admitted = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            provider, "run-receipt", "documentation", preferences, CombinedTextOf(preferences),
            [CorpusRow("e1", FirstTrainText, "ok")]);
        Assert.True(admitted.IsSuccess, admitted.ErrorMessage);
        var receipt = admitted.Value!.Receipt!;

        // Каталог берётся временный и свой: ни одного файла проекта эта проверка не касается.
        var root = Path.Combine(
            Path.GetTempPath(),
            "llm-mvp-t112-receipt-" + Guid.NewGuid().ToString("N"));
        try
        {
            // Запись сама читает записанное обратно и сверяет все поля.
            var saved = CapabilityMaterialAdmissionReceiptStore.Save(root, "run-receipt", receipt);
            Assert.True(saved.IsSuccess, saved.ErrorMessage);
            Assert.True(File.Exists(saved.Value!));

            // Отдельное чтение — как после перезапуска: о памяти прежнего запуска оно не знает.
            var loaded = CapabilityMaterialAdmissionReceiptStore.Load(saved.Value!);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            var restored = loaded.Value!;

            // Прочитанная расписка совпадает по всем значащим полям, включая оба отпечатка.
            Assert.Equal(receipt.ProfileId, restored.ProfileId);
            Assert.Equal(receipt.SchemaVersion, restored.SchemaVersion);
            Assert.Equal(receipt.CheckedRowCount, restored.CheckedRowCount);
            Assert.Equal(receipt.TrainRowCount, restored.TrainRowCount);
            Assert.Equal(receipt.DevRowCount, restored.DevRowCount);
            Assert.Equal(receipt.TestRowCount, restored.TestRowCount);
            Assert.Equal(receipt.TrainFamilies, restored.TrainFamilies);
            Assert.Equal(receipt.HeldOutFamilies, restored.HeldOutFamilies);
            Assert.Equal(receipt.NormalizationPolicy, restored.NormalizationPolicy);
            Assert.Equal(receipt.IntersectionPolicy, restored.IntersectionPolicy);
            Assert.Equal(receipt.ModelVisibleCollisionCheck, restored.ModelVisibleCollisionCheck);
            Assert.Equal(receipt.ConsumedPreferenceRowCount, restored.ConsumedPreferenceRowCount);
            Assert.Equal(receipt.ConsumedMaterialSha256, restored.ConsumedMaterialSha256);
            Assert.Equal(receipt.ConsumedMaterialBinding, restored.ConsumedMaterialBinding);
            Assert.Equal(receipt.ConsumedIdentitySha256, restored.ConsumedIdentitySha256);
            Assert.Equal(
                receipt.ConsumedRetrievalCorpusRowCount,
                restored.ConsumedRetrievalCorpusRowCount);

            // И чтение не молчаливое: изменённый на диске файл отвергается названным отказом.
            var text = File.ReadAllText(saved.Value!);
            File.WriteAllText(
                saved.Value!,
                text.Replace("\ttrain-", "\tподменено-", StringComparison.Ordinal)
                    .Replace(receipt.ProfileId, "подменённый-профиль", StringComparison.Ordinal));
            var tampered = CapabilityMaterialAdmissionReceiptStore.Load(saved.Value!);
            Assert.False(tampered.IsSuccess);
            Assert.Equal(
                CapabilityMaterialAdmissionReceiptStore.DigestMismatchErrorCode,
                tampered.ErrorCode);
        }
        finally
        {
            // Свой временный каталог убирается тем же ходом, в котором был создан.
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    // ── Пункт 5: перевод строки внутри поля не сдвигает границы и у тождества ──

    [Fact]
    public void NewlineInsideAFieldCannotMoveTheIdentityDigestBoundaries()
    {
        // Один и тот же честный подготовитель для обоих наборов.
        var provider = ProviderWithHonestLedger();
        // Общий учебный текст одинаков нарочно и несёт все три разрешённые учебные строки: иначе
        // наборы разошлись бы уже по нему, а материал был бы неполным.
        var sharedCombinedText = string.Join(" ", FirstTrainText, SecondTrainText, ThirdTrainText);

        // Набор 1: перевод строки лежит внутри ВОПРОСА первой пары.
        var newlineInPrompt = CompleteTrainPreferences();
        newlineInPrompt[0] = Pair(FirstTrainText + "\nпояснение", "ok", "no");
        // Набор 2: те же куски разложены иначе — перевод строки ушёл в ПРЕДПОЧИТАЕМУЮ сторону.
        var newlineInChosen = CompleteTrainPreferences();
        newlineInChosen[0] = Pair(FirstTrainText, "пояснение\nok", "no");

        // Прежнее правило склейки через перевод строки на этом входе отвечает НЕВЕРНО: материал
        // разный, а поток байтов и отпечаток у него один.
        Assert.Equal(
            PriorNewlineJoinedDigest(newlineInPrompt, sharedCombinedText),
            PriorNewlineJoinedDigest(newlineInChosen, sharedCombinedText));

        // Действующее правило тождества отвечает верно: отпечатки разные.
        var firstAdmitted = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            provider, "run-newline-identity-1", "documentation", newlineInPrompt, sharedCombinedText);
        var secondAdmitted = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            provider, "run-newline-identity-2", "documentation", newlineInChosen, sharedCombinedText);
        Assert.True(firstAdmitted.IsSuccess, firstAdmitted.ErrorMessage);
        Assert.True(secondAdmitted.IsSuccess, secondAdmitted.ErrorMessage);
        Assert.NotEqual(
            firstAdmitted.Value!.Receipt!.ConsumedIdentitySha256,
            secondAdmitted.Value!.Receipt!.ConsumedIdentitySha256);

        // И прежнее правило действительно зависит от входа: на материале без перевода строки его
        // ответ другой, то есть сравнение выше не вырожденное.
        Assert.NotEqual(
            PriorNewlineJoinedDigest(newlineInPrompt, sharedCombinedText),
            PriorNewlineJoinedDigest(CompleteTrainPreferences(), sharedCombinedText));
    }

    // ── Пункт 6: вектор нельзя изменить после допуска ─────────────────────────

    [Fact]
    public void MutatingTheTeachingVectorAfterAdmission_CannotReachTheLearner()
    {
        // Вектор, который вызывающий оставляет себе на руках.
        var callerVector = new[] { 0.1f, 0.2f };
        var callerPreferences = CompleteTrainPreferences();
        callerPreferences[0] = callerPreferences[0] with { TeachingVector = callerVector };

        var admitted = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
            ProviderWithHonestLedger(),
            "run-vector",
            "documentation",
            callerPreferences,
            CombinedTextOf(callerPreferences));
        Assert.True(admitted.IsSuccess, admitted.ErrorMessage);
        var carrier = admitted.Value!;

        // Атака 1: вызывающий меняет число в СВОЁМ векторе уже после допуска. Текст пары он не
        // трогает вовсе, поэтому ни один текстовый отпечаток этого бы не заметил.
        callerVector[1] = 999f;

        // Носитель отдаёт обучению допущенные числа, а не подменённые.
        var handedOut = carrier.ReadPreferencesForLearner();
        Assert.True(handedOut.IsSuccess, handedOut.ErrorMessage);
        Assert.Equal(new[] { 0.1f, 0.2f }, handedOut.Value![0].TeachingVector);

        // Атака 2: получатель меняет число в выданной ЕМУ копии.
        handedOut.Value[0].TeachingVector![1] = 777f;
        // Следующий получатель снова получает допущенные числа: порча не переходит к нему.
        var secondHandOut = carrier.ReadPreferencesForLearner();
        Assert.True(secondHandOut.IsSuccess, secondHandOut.ErrorMessage);
        Assert.Equal(new[] { 0.1f, 0.2f }, secondHandOut.Value![0].TeachingVector);
        // И вектор у каждого получателя свой: общий объект снова открыл бы ту же дверь.
        Assert.NotSame(handedOut.Value[0].TeachingVector, secondHandOut.Value[0].TeachingVector);
    }

    // ── Оснастка ──────────────────────────────────────────────────────────────

    // Строки замороженного источника: три учебные, одна промежуточной проверки и одна
    // экзаменационная. Те же строки, что у соседних проверок этого же сторожа.
    private static IReadOnlyList<CapabilitySourceRow> HonestSourceRows() =>
    [
        new("s1", "g1", CapabilityMaterialRole.Train, FirstTrainText, "ok"),
        new("s2", "g2", CapabilityMaterialRole.Train, SecondTrainText, "ok"),
        new("s3", "g3", CapabilityMaterialRole.Train, ThirdTrainText, "ok"),
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

    // Полный разрешённый учебный набор: все три учебные строки источника доходят до обучателя.
    private static LogicalDpoPreferenceExample[] CompleteTrainPreferences() =>
    [
        Pair(FirstTrainText, "ok", "no"),
        Pair(SecondTrainText, "ok", "no"),
        Pair(ThirdTrainText, "ok", "no")
    ];

    // Одна пара предпочтений обычной формы: вопрос, верная сторона, неверная сторона и вектор.
    private static LogicalDpoPreferenceExample Pair(string prompt, string chosen, string rejected)
        => new(prompt, chosen, rejected, [0.1f, 0.2f]);

    // Одна строка упорядоченного корпуса поиска обычной формы.
    private static GeneratedCapabilityKnnExample CorpusRow(
        string exampleId,
        string trainingText,
        string outputText)
        => new(exampleId, trainingText, outputText, ["p1"], ["documentation"]);

    // Общий учебный текст собирается из вопросов пар — так же, как его собирает обучатель.
    private static string CombinedTextOf(LogicalDpoPreferenceExample[] preferences)
        => string.Join(" ", preferences.Select(pair => pair.Prompt));

    // Ожидаемый отпечаток ТОЖДЕСТВА, посчитанный проверкой самостоятельно по тому же открытому
    // правилу: версия правила кодирования и назначение потока, затем общий учебный текст, число
    // пар, затем у каждой пары вопрос, обе стороны и учебный вектор точными битами каждого числа,
    // затем число строк корпуса и сами строки. У каждого поля длина в БАЙТАХ объявляется до его
    // байтов, у отсутствующего значения — своя метка.
    private static string ExpectedIdentityDigest(
        LogicalDpoPreferenceExample[] preferences,
        string combinedTrainingText,
        GeneratedCapabilityKnnExample[] retrievalCorpus)
    {
        // Накопитель байтов ожидаемого потока.
        var bytes = new List<byte>();
        // Версия правила кодирования идёт первым полем потока.
        AppendField(bytes, "length-prefixed-utf8-be32-sha256-v1");
        // Назначение потока идёт вторым полем и у тождества своё.
        AppendField(bytes, "capability-consumed-training-identity-v1");
        // Общий учебный текст идёт третьим полем.
        AppendField(bytes, combinedTrainingText);
        // Число пар входит в поток отдельной счётной величиной.
        AppendCount(bytes, preferences.Length);
        foreach (var pair in preferences)
        {
            // Три текстовых поля пары.
            AppendField(bytes, pair.Prompt);
            AppendField(bytes, pair.Chosen);
            AppendField(bytes, pair.Rejected);
            if (pair.TeachingVector is null)
            {
                // Отсутствующий вектор несёт только метку отсутствия.
                AppendField(bytes, null);
                continue;
            }

            // Длина вектора, затем точные биты каждого числа.
            AppendCount(bytes, pair.TeachingVector.Length);
            foreach (var value in pair.TeachingVector)
                AppendCount(bytes, BitConverter.SingleToInt32Bits(value));
        }

        // Число строк корпуса и сами строки с их наборами признаков и метк.
        AppendCount(bytes, retrievalCorpus.Length);
        foreach (var example in retrievalCorpus)
        {
            AppendField(bytes, example.ExampleId);
            AppendField(bytes, example.TrainingText);
            AppendField(bytes, example.OutputText);
            AppendCount(bytes, example.PredicateIds.Count);
            foreach (var predicateId in example.PredicateIds)
                AppendField(bytes, predicateId);
            AppendCount(bytes, example.DomainTags.Count);
            foreach (var domainTag in example.DomainTags)
                AppendField(bytes, domainTag);
        }

        // Отпечаток считается тем же названным примитивом по всему потоку.
        return Convert.ToHexString(SHA256.HashData(bytes.ToArray())).ToLowerInvariant();
    }

    // Одно поле ожидаемого потока: метка наличия, затем длина в байтах, затем сами байты.
    private static void AppendField(List<byte> bytes, string? value)
    {
        if (value is null)
        {
            bytes.Add(0x00); // Отсутствующее значение несёт только свою метку.
            return;          // Длины и байтов у него нет.
        }

        var utf8 = Encoding.UTF8.GetBytes(value); // Байты значения.
        bytes.Add(0x01);                          // Метка присутствующего значения.
        AppendBigEndian(bytes, utf8.Length);      // Длина в байтах до самих байтов.
        bytes.AddRange(utf8);                     // Сами байты значения.
    }

    // Одна счётная величина ожидаемого потока: метка числа и четыре байта значения.
    private static void AppendCount(List<byte> bytes, int value)
    {
        bytes.Add(0x02);              // Метка целого числа.
        AppendBigEndian(bytes, value); // Четыре байта от старшего к младшему.
    }

    // Четыре байта целого числа от старшего к младшему.
    private static void AppendBigEndian(List<byte> bytes, int value)
    {
        bytes.Add((byte)((value >> 24) & 0xFF)); // Старший байт.
        bytes.Add((byte)((value >> 16) & 0xFF)); // Второй байт.
        bytes.Add((byte)((value >> 8) & 0xFF));  // Третий байт.
        bytes.Add((byte)(value & 0xFF));         // Младший байт.
    }

    /// <summary>
    /// ПРЕЖНЕЕ правило отпечатка: поля склеивались через перевод строки без экранирования, а
    /// отсутствующая сторона приводилась к пустой строке. Настоящая функция от входа: ответ
    /// считается из предъявленного материала. Воспроизведено здесь только для сравнения.
    /// </summary>
    private static string PriorNewlineJoinedDigest(
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
