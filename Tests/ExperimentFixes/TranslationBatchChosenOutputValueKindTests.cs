// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Text.Json;
using System.Text.Json.Serialization;
using TranslationDomain;

// Что здесь проверяется, по-человечески.
//
// Проверка столкновения рабочих ключей читает образец предложения из текста правильного ответа
// учебной строки. Чтобы его прочитать, надо обратиться к свойству внутри значения.
//
// Что было неправильно. Проход сразу читал свойство у корневого элемента разобранного документа.
// Для КОРРЕКТНОГО, но не объектного значения — пустого массива, числа, строки — чтение свойства
// по договору библиотеки разбора выбрасывает исключение неверной операции, а перехвачено было
// только исключение разбора текста. Поэтому на таком входе падал весь проход, и собственный
// отказ при столкновении ключей до него просто не доходил: вместо управляемого отказа партии
// наружу уходило исключение.
//
// Что стало. Вид значения корня проверяется ДО любого чтения свойства. Пустое значение остаётся
// прежним тихим пропуском обучателя с обычным учётом принятой партии. Любой другой необъектный
// вид — управляемый отказ с кодом и названным фактическим видом значения.
//
// Почему проверка вида, а не расширение перехвата исключений. Перехват маскирует: по нему не
// видно, какой вид пришёл и почему он негоден. Проверка вида объясняет и позволяет назвать
// фактический вид в причине отказа.
//
// Чувствительность к мутации. Проверки «пустой массив» и «пустое значение» красные на прежней
// логике: там на этих входах вылетало исключение, а не отказ и не пропуск. Проверка «обычный
// объект» обязана быть зелёной и до, и после: она доказывает, что починка не отключила сам
// поиск столкновений.
//
// Чего эти проверки НЕ доказывают. Они не прогонялись: прямые команды сборки и прогона тестов
// запрещены политикой разрешений проекта, и обходить запрет оболочкой-посредником я не стал.
public sealed class TranslationBatchChosenOutputValueKindTests
{
    // Перечисления в учебных примерах пишутся словами, как их пишет продуктовый конструктор.
    private static readonly JsonSerializerOptions JsonOptions =
        new() { Converters = { new JsonStringEnumConverter() } };

    // ── Вход 1: пустой массив — управляемый отказ, а не исключение ────────────

    [Fact]
    public async Task EmptyArrayAsChosenOutput_IsARefusalWithNamedValueKindAndNoPublication()
    {
        var machine = new TranslationMachine();

        // Отпечаток состояния ДО попытки: с ним сравнивается состояние после отказа.
        var before = machine.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        var fingerprintBefore = Fingerprint(before.Value!);

        // Текст правильного ответа — корректный документ, но его корень не объект.
        var batch = BatchWithChosenOutput("[]");

        var train = await machine.TrainStructuredBatchAsync(batch);

        // Отказ, а не исключение: это обычный результат операции.
        Assert.False(train.IsSuccess);
        // Код отказа назван точно.
        Assert.Equal("BATCH_ROW_CHOSEN_OUTPUT_NOT_OBJECT", train.ErrorCode);
        // Причина называет ФАКТИЧЕСКИЙ вид значения и виновную строку.
        Assert.Contains("Array", train.ErrorMessage);
        Assert.Contains("row-not-object", train.ErrorMessage);

        // Публикации не было: состояние переводчика целиком то же, что до попытки.
        var after = machine.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        Assert.Equal(fingerprintBefore, Fingerprint(after.Value!));
    }

    // ── Вход 2: пустое значение — прежний пропуск обучателя ───────────────────

    [Fact]
    public async Task NullAsChosenOutput_KeepsTheExistingTrainerSkipAndCountsTheAcceptedBatch()
    {
        var machine = new TranslationMachine();

        // Пустое значение — разрешённый документ. Обучатель разборщика смысла на нём ничего не
        // прочитает и строку пропустит; партия при этом остаётся принятой.
        var batch = BatchWithChosenOutput("null");

        var train = await machine.TrainStructuredBatchAsync(batch);

        // Партия принята: исключения нет и отказа нет.
        Assert.True(train.IsSuccess, train.ErrorMessage);
        // Учёт партии прежний: строка посчитана как поданная в обучение.
        Assert.Equal(1, train.Value!.RowsTrained);
        // Имя партии в итоге то же, что подали: учёт не подменён.
        Assert.Equal(batch.BatchId, train.Value.BatchId);
    }

    // ── Вход 3: обычный объект — поиск столкновений по-прежнему работает ──────

    [Fact]
    public async Task OrdinaryObjectChosenOutput_StillDetectsTheKeyCollision()
    {
        var machine = new TranslationMachine();

        // Две строки отличаются только номером учебной пары, а образцы предложения у них разные.
        // После переноса номера пары из ключа оба ключа становятся одним, и молча затирать
        // первый урок последним нельзя.
        var collidingBatch = new TranslationLearningBatch(
            BatchId: "value-kind-collision-v1",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                TokenizerRow("c01", "SVO|VALIDATE_CHECK|PAIR:c01", SentencePattern.SVO),
                TokenizerRow("c02", "SVO|VALIDATE_CHECK|PAIR:c02", SentencePattern.SOV)
            ],
            SourceIds: ["simulation"]);

        var train = await machine.TrainStructuredBatchAsync(collidingBatch);

        // Отказ пришёл именно от столкновения ключей, а не от вида значения: значит обычный
        // объектный вход по-прежнему доходит до самой проверки.
        Assert.False(train.IsSuccess);
        Assert.Equal("TRANSLATION_SEMANTIC_KEY_COLLISION", train.ErrorCode);
    }

    // ── Оснастка ──────────────────────────────────────────────────────────────

    // BatchWithChosenOutput собирает партию из одной учебной строки разборщика смысла, у которой
    // текст правильного ответа задан дословно: именно его вид и проверяется.
    private static TranslationLearningBatch BatchWithChosenOutput(string chosenOutputJson)
        => new(
            BatchId: "value-kind-v1",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                new TranslationLearningExample(
                    ExampleId: "row-not-object:semantic",
                    UnitId: TranslationUnitIds.SemanticTokenizer,
                    FlowId: "ru-en-general",
                    InputJson: JsonSerializer.Serialize(
                        new TokenizerInput("Он проверил файл", "ru"), JsonOptions),
                    ChosenOutputJson: chosenOutputJson,
                    RejectedOutputJson: JsonSerializer.Serialize(
                        new TokenizerOutput(SentencePattern.UNKNOWN, "WRONG|SVO"), JsonOptions),
                    RejectedType: "wrong_sentence_pattern",
                    SemanticSignature: "SVO|VALIDATE_CHECK",
                    SourceId: "simulation",
                    HeldOut: false,
                    PairId: "row-not-object",
                    KeySchemaVersion: TranslationSemanticKey.SchemaVersion)
            ],
            SourceIds: ["simulation"]);

    // TokenizerRow собирает обычную учебную строку разборщика с ключом прежней формы.
    private static TranslationLearningExample TokenizerRow(
        string pairId,
        string legacyKey,
        SentencePattern chosenPattern)
        => new(
            ExampleId: pairId + ":semantic",
            UnitId: TranslationUnitIds.SemanticTokenizer,
            FlowId: "ru-en-general",
            InputJson: JsonSerializer.Serialize(
                new TokenizerInput("Он проверил " + pairId, "ru"), JsonOptions),
            ChosenOutputJson: JsonSerializer.Serialize(
                new TokenizerOutput(chosenPattern, legacyKey), JsonOptions),
            RejectedOutputJson: JsonSerializer.Serialize(
                new TokenizerOutput(SentencePattern.UNKNOWN, "WRONG|" + legacyKey), JsonOptions),
            RejectedType: "wrong_sentence_pattern",
            SemanticSignature: legacyKey,
            SourceId: "simulation",
            HeldOut: false,
            PairId: pairId,
            KeySchemaVersion: TranslationSemanticKey.LegacySchemaVersion);

    // Fingerprint снимает отпечаток всего сохранённого состояния. Время записи файла исключено
    // намеренно: это стенные часы, а не обученное состояние, и два экспорта одного и того же
    // состояния отличаются в нём всегда.
    private static string Fingerprint(TranslationMachineCheckpoint checkpoint)
        => JsonSerializer.Serialize(
            checkpoint with { ExportedAtUtc = DateTimeOffset.UnixEpoch },
            JsonOptions);
}
