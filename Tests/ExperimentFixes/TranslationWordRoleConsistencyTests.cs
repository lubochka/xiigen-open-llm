// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие остатка «роли слов согласованы с исправленным смыслом»
// (назначение FIX-124-S05, раздел S05 плана
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments/
// codex-124-post107-addressed-repair-plan-2026-11-02.md; основание — первичный
// разбор плеча B опыта о чтениях и вердиктах P10601, где исправленные образец и
// подпись сосуществовали с «файл»=VERB/IMPERATIVE и лексемой «фай»).
//
// Что здесь проверяется по-человечески. Разборщик определяет роль слова по его
// окончанию. Из-за этого существительное могло быть названо действием: слово
// «файл» суффиксное правило относило к глаголам только потому, что оно
// кончается на «л». Дальше эта неверная роль расходилась по всему результату
// разбора:
//
//   * лексема обрезалась до «фай», и объявленное словарём понятие «файл» —
//     DATA_ENTITY — исчезало из полного ключа вообще;
//   * роль слова становилась «повеление», то есть прямой объект выдавал себя
//     за саму команду;
//   * образец предложения переворачивался: «Он проверил файл» выглядел
//     предложением с глаголом в конце, а страдательное «файл проверен» —
//     повелительным;
//   * пометки предложения получали прошедшее время, пол глагола прошедшей
//     формы и согласование подлежащего с ним — всё это выдумано поверх
//     существительного;
//   * все три ключа записи собирались из этих токенов, поэтому обученная память
//     адресовалась ложной ролью слова.
//
// Граница этого остатка названа прямо, и отчёт её не расширяет:
//
//   * догадка сырого разбора о времени обычного глагола НЕ исправлена: для
//     «Он проверяет файл» разборщик по-прежнему сообщает прошедшее. Это
//     закреплено отдельной проверкой соседнего набора
//     `TranslationVerifiedRuleTagTests`, и здесь она не ослабляется;
//   * защита распространяется только на ОБЪЯВЛЕННЫЕ словарём сущности.
//     Незнакомое слово на «л» остаётся прежней догадкой суффикса, и это
//     закреплено отдельной проверкой ниже как честный предел;
//   * поиск подлежащего по словам предложения не менялся: у повелительного
//     предложения он по-прежнему может указать на прямой объект. Это отдельный
//     механизм определения рода, а не базовая классификация роли слова;
//   * улучшения перевода этот остаток не заявляет: на обученных строках текст
//     верен и до, и после правки. Основание допуска — логическая целость
//     разбора и удержание правильных ответов.
//
// Чувствительность к мутации. Каждая решающая проверка ниже падает на
// НЕИЗМЕНЁННОМ продуктовом коде по существу: слово «файл» приходит глаголом с
// лексемой «фай» и ролью повеления, образец предложения перевёрнут,
// повелительное и страдательное предложения поменялись местами, пометки несут
// выдуманное прошедшее время с полом глагола, а ключи обученной записи
// называют два глагола там, где глагол один. Падения из-за несобравшейся
// сборки или отсутствующего набора красным не считаются.

namespace LlmFromScratch.Tests;

using System.Text;
using System.Text.Json;
using AgentInfrastructure.Logical;
using TranslationDomain;
using Xunit;

// Набор проверок согласованности роли слова с целым результатом разбора.
public sealed class TranslationWordRoleConsistencyTests
{
    // Назначение и попытка — ими же назван каталог улик этой работы.
    private const string FixId = "FIX-124-S05";
    private const string AttemptId = "attempt-0001";

    // Направление разбора: русский источник, английская цель.
    private const string SourceLanguage = "ru";
    private const string TargetLanguage = "en";

    // Повествовательная строка с прямым объектом «файл».
    private const string StatementSource = "Он проверил файл";

    // Повелительная строка: команда стоит первой, «файл» — прямой объект.
    private const string CommandSource = "проверь файл";

    // Страдательная строка: «файл» стоит первым, но это не команда.
    private const string PassiveSource = "файл проверен";

    // Слово, которое суффиксное правило относило не к той роли.
    private const string EntityWord = "файл";

    // Пометка прошедшего времени в написании сырого разбора.
    private const string RawPastTense = "RU:TENSE:PAST";

    // Пометка повелительного наклонения согласованного разбора.
    private const string RawImperativeMood = "RU:MOOD:IMPERATIVE";

    // Начало семейства времени и семейства наклонения.
    private const string RawTenseFamily = "RU:TENSE:";
    private const string RawMoodFamily = "RU:MOOD:";

    // Настройки файлов улик: читаемый отступ и русский текст как есть.
    private static readonly JsonSerializerOptions ReceiptJson = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    // ── Решающая проверка 1: объявленная сущность остаётся предметом ──────────
    [Fact]
    public void DeclaredEntity_IsNotTurnedIntoAVerbByItsLastLetter()
    {
        var tokenizer = new TranslationSemanticTokenizer();

        // Одно и то же слово проверяется в трёх разных предложениях: его роль не
        // может зависеть от того, что стоит рядом.
        var observed = new List<object>();
        foreach (var source in new[] { StatementSource, CommandSource, PassiveSource })
        {
            var sequence = tokenizer.Tokenize(source, SourceLanguage);
            var entity = sequence.Tokens.Single(token =>
                string.Equals(token.Word, EntityWord, StringComparison.OrdinalIgnoreCase));

            observed.Add(new
            {
                строка = source,
                часть_речи = entity.Pos.ToString(),
                лексема = entity.Lemma,
                понятие = entity.Concept.ToString(),
                роль = entity.Role
            });
        }

        // Улики пишутся ДО решающих утверждений, чтобы журнал обнаружения
        // остался и у красного прогона.
        WriteReceipt("word-role-of-the-declared-entity.json", new
        {
            назначение = FixId,
            попытка = AttemptId,
            объявленное_понятие_слова = SemanticConcept.DATA_ENTITY.ToString(),
            наблюдения = observed
        });

        foreach (var source in new[] { StatementSource, CommandSource, PassiveSource })
        {
            var sequence = tokenizer.Tokenize(source, SourceLanguage);
            var entity = sequence.Tokens.Single(token =>
                string.Equals(token.Word, EntityWord, StringComparison.OrdinalIgnoreCase));

            // Предмет — не действие. Эталон взят из русского языка и из
            // объявленного словаря понятий, а не из поведения кода.
            Assert.Equal(TranslationPos.NOUN, entity.Pos);

            // Лексема обязана остаться разрешимой до объявленного понятия:
            // обрезка до «фай» уничтожала его молча.
            Assert.Equal(EntityWord, entity.Lemma);
            Assert.Equal(SemanticConcept.DATA_ENTITY, entity.Concept);

            // Прямой объект не может быть самой командой.
            Assert.NotEqual("IMPERATIVE", entity.Role);
        }
    }

    // ── Решающая проверка 2: роль одного слова больше не переписывает образец ─
    [Fact]
    public void TheRoleOfOneWord_NoLongerRewritesTheSentencePattern()
    {
        var tokenizer = new TranslationSemanticTokenizer();

        var statement = tokenizer.Tokenize(StatementSource, SourceLanguage);
        var command = tokenizer.Tokenize(CommandSource, SourceLanguage);
        var passive = tokenizer.Tokenize(PassiveSource, SourceLanguage);

        WriteReceipt("sentence-pattern-of-three-readings.json", new
        {
            назначение = FixId,
            попытка = AttemptId,
            образцы = new
            {
                повествование = new { строка = StatementSource, образец = statement.Pattern.ToString() },
                повеление = new { строка = CommandSource, образец = command.Pattern.ToString() },
                страдательное = new { строка = PassiveSource, образец = passive.Pattern.ToString() }
            },
            части_речи = new
            {
                повествование = statement.Tokens.Select(token => token.Pos.ToString()),
                повеление = command.Tokens.Select(token => token.Pos.ToString()),
                страдательное = passive.Tokens.Select(token => token.Pos.ToString())
            }
        });

        // «Он проверил файл» — подлежащее, действие, прямой объект. Прямой объект
        // в конце не делает предложение предложением с глаголом в конце.
        Assert.Equal(SentencePattern.SVO, statement.Pattern);

        // «проверь файл» — настоящее повеление: команда стоит первой.
        Assert.Equal(SentencePattern.IMPERATIVE_V, command.Pattern);
        Assert.Equal(TranslationPos.VERB, command.Tokens[0].Pos);
        Assert.Equal("IMPERATIVE", command.Tokens[0].Role);

        // «файл проверен» повелением не является, и ни одно его слово не
        // получает роль команды: повеление и страдательное больше не
        // поменяны местами.
        Assert.NotEqual(SentencePattern.IMPERATIVE_V, passive.Pattern);
        Assert.DoesNotContain(passive.Tokens, token => token.Role == "IMPERATIVE");
    }

    // ── Решающая проверка 3: выдуманного прошедшего времени больше нет ────────
    [Fact]
    public void NoFabricatedPastTense_IsStampedOnACommandOrOnAThing()
    {
        var tokenizer = new TranslationSemanticTokenizer();

        var command = tokenizer.Tokenize(CommandSource, SourceLanguage);
        var passive = tokenizer.Tokenize(PassiveSource, SourceLanguage);

        WriteReceipt("rule-tags-of-the-command-and-the-passive.json", new
        {
            назначение = FixId,
            попытка = AttemptId,
            повеление = new { строка = CommandSource, пометки = command.RuleTags },
            страдательное = new { строка = PassiveSource, пометки = passive.RuleTags }
        });

        // У повеления времени нет вовсе, поэтому прошедшее ему выдумать нельзя.
        Assert.DoesNotContain(RawPastTense, command.RuleTags);

        // Пол глагола прошедшей формы и согласование с ним имеют смысл только
        // при прошедшем времени. Поверх существительного «файл» они были
        // выдуманы целиком.
        Assert.DoesNotContain(
            command.RuleTags,
            tag => tag.Contains("VERB_GENDER", StringComparison.Ordinal) ||
                   tag.Contains("_PAST_", StringComparison.Ordinal));

        // Согласованный разбор говорит о повелении прямо, а не молчит.
        Assert.Contains(RawImperativeMood, command.RuleTags);

        // У страдательной строки глагола объявленный разбор не находит, поэтому
        // ни времени, ни наклонения он ей не приписывает.
        Assert.DoesNotContain(
            passive.RuleTags,
            tag => tag.StartsWith(RawTenseFamily, StringComparison.Ordinal) ||
                   tag.StartsWith(RawMoodFamily, StringComparison.Ordinal));
    }

    // ── Решающая проверка 4: ключи несут то же чтение, что и токены ───────────
    [Fact]
    public void TheThreeKeys_CarryTheSameReadingAsTheTokens()
    {
        var tokenizer = new TranslationSemanticTokenizer();
        var statement = tokenizer.Tokenize(StatementSource, SourceLanguage);

        // Сколько действий названо в перечне частей речи структурного ключа и сколько
        // глаголов в самих токенах — это должно быть одно и то же число.
        var verbsInKey = VerbsNamedInStructureKey(statement);
        var verbsInTokens = statement.Tokens.Count(token => token.Pos == TranslationPos.VERB);

        WriteReceipt("keys-and-tokens-of-the-statement.json", new
        {
            назначение = FixId,
            попытка = AttemptId,
            строка = StatementSource,
            полный_ключ = statement.Signature,
            префиксный_ключ = statement.PrefixSignature,
            структурный_ключ = statement.StructureSignature,
            части_речи_в_ключе = PartsOfSpeechNamedInStructureKey(statement),
            части_речи_в_токенах = statement.Tokens.Select(token => token.Pos.ToString()),
            глаголов_в_ключе = verbsInKey,
            глаголов_в_токенах = verbsInTokens
        });

        // В «Он проверил файл» глагол ровно один. Два — это и была разошедшаяся
        // роль слова, записанная прямо в адрес обученной памяти.
        Assert.Equal(1, verbsInTokens);
        Assert.Equal(verbsInTokens, verbsInKey);

        // Перечень частей речи в ключе обязан слово в слово повторять роли токенов.
        Assert.Equal(
            statement.Tokens.Select(token => token.Pos.ToString()).ToList(),
            PartsOfSpeechNamedInStructureKey(statement));

        // Объявленное понятие прямого объекта обязано дойти до ключей: именно
        // его уничтожала обрезка лексемы до «фай».
        Assert.Contains(SemanticConcept.DATA_ENTITY.ToString(), statement.Signature);
        Assert.Contains(SemanticConcept.DATA_ENTITY.ToString(), statement.PrefixSignature);

        // Каждая возвращённая пометка обязана стоять и в полном, и в структурном
        // ключе: иначе ключ описывает не тот ответ, что пометки.
        foreach (var tag in statement.RuleTags)
        {
            Assert.Contains(tag, statement.Signature);
            Assert.Contains(tag, statement.StructureSignature);
        }

        // Образец предложения — начало каждого из трёх ключей.
        var pattern = statement.Pattern.ToString();
        Assert.StartsWith(pattern, statement.Signature, StringComparison.Ordinal);
        Assert.StartsWith(pattern, statement.PrefixSignature, StringComparison.Ordinal);
        Assert.StartsWith(pattern, statement.StructureSignature, StringComparison.Ordinal);
    }

    // ── Решающая проверка 5: знаки препинания и перестановка слов ─────────────
    [Fact]
    public void PunctuationVariantsAndWordOrder_ReadTheSameWordRoles()
    {
        var tokenizer = new TranslationSemanticTokenizer();

        // Три варианта той же команды: с точкой, с запятой и с переставленными
        // словами. Роль слова не может зависеть от знака препинания.
        var variants = new[] { "Проверь файл.", "проверь, файл", "файл проверь" };

        var observed = new List<object>();
        foreach (var source in variants)
        {
            var sequence = tokenizer.Tokenize(source, SourceLanguage);
            observed.Add(new
            {
                строка = source,
                токены = sequence.Tokens.Select(token => new
                {
                    слово = token.Word,
                    часть_речи = token.Pos.ToString(),
                    лексема = token.Lemma,
                    понятие = token.Concept.ToString(),
                    роль = token.Role
                })
            });
        }

        WriteReceipt("punctuation-and-word-order-variants.json", new
        {
            назначение = FixId,
            попытка = AttemptId,
            наблюдения = observed
        });

        foreach (var source in variants)
        {
            var sequence = tokenizer.Tokenize(source, SourceLanguage);

            // Команда остаётся действием с ролью повеления при любом порядке
            // слов и любом знаке препинания.
            var verb = sequence.Tokens.Single(token =>
                token.Word.Contains("роверь", StringComparison.Ordinal));
            Assert.Equal(TranslationPos.VERB, verb.Pos);
            Assert.Equal("IMPERATIVE", verb.Role);

            // Прямой объект остаётся объявленным предметом.
            var entity = sequence.Tokens.Single(token =>
                token.Word.Contains(EntityWord, StringComparison.OrdinalIgnoreCase));
            Assert.Equal(TranslationPos.NOUN, entity.Pos);
            Assert.Equal(SemanticConcept.DATA_ENTITY, entity.Concept);
        }
    }

    // ── Решающая проверка 6: неподдержанная морфология остаётся неразобранной ─
    [Fact]
    public void UnsupportedMorphology_IsLeftUnreadWithoutInventingATenseOrAMood()
    {
        var tokenizer = new TranslationSemanticTokenizer();

        // Отрицание с повелительной формой несовершенного вида: объявленный
        // разбор такую форму не поддерживает и обязан промолчать, а не выдумать.
        var sequence = tokenizer.Tokenize("не проверяй файл", SourceLanguage);

        WriteReceipt("unsupported-morphology-stays-unread.json", new
        {
            назначение = FixId,
            попытка = AttemptId,
            строка = "не проверяй файл",
            пометки = sequence.RuleTags,
            токены = sequence.Tokens.Select(token => new
            {
                слово = token.Word,
                часть_речи = token.Pos.ToString(),
                понятие = token.Concept.ToString(),
                роль = token.Role
            })
        });

        // Ни времени, ни наклонения неразобранной строке не приписывается.
        Assert.DoesNotContain(
            sequence.RuleTags,
            tag => tag.StartsWith(RawTenseFamily, StringComparison.Ordinal) ||
                   tag.StartsWith(RawMoodFamily, StringComparison.Ordinal));

        // Объявленный предмет при этом остаётся предметом: отрицание и
        // неразобранное слово рядом его роль не меняют.
        var entity = sequence.Tokens.Single(token =>
            string.Equals(token.Word, EntityWord, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(TranslationPos.NOUN, entity.Pos);
        Assert.Equal(SemanticConcept.DATA_ENTITY, entity.Concept);
    }

    // ── Честный предел: незнакомое слово остаётся прежней догадкой суффикса ───
    [Fact]
    public void AnUndeclaredWordOnTheSameLetter_KeepsTheOldSuffixGuess()
    {
        // Этот предел назван прямо, чтобы отчёт не выглядел шире сделанной
        // работы: защита распространяется только на ОБЪЯВЛЕННЫЕ словарём
        // сущности. Слово «журнал» в словаре понятий не объявлено, поэтому
        // суффиксное правило по-прежнему считает его действием. Эту проверку
        // нельзя убирать: она закрепляет границу, а не достижение.
        var tokenizer = new TranslationSemanticTokenizer();

        var sequence = tokenizer.Tokenize("Он прочитал журнал", SourceLanguage);
        var undeclared = sequence.Tokens.Single(token =>
            token.Word.Contains("журнал", StringComparison.OrdinalIgnoreCase));

        WriteReceipt("named-limit-undeclared-word.json", new
        {
            назначение = FixId,
            попытка = AttemptId,
            строка = "Он прочитал журнал",
            слово = undeclared.Word,
            часть_речи = undeclared.Pos.ToString(),
            понятие = undeclared.Concept.ToString(),
            предел = "защита охватывает только объявленные словарём сущности"
        });

        Assert.Equal(TranslationPos.VERB, undeclared.Pos);
        Assert.Equal(SemanticConcept.UNKNOWN, undeclared.Concept);
    }

    // ── Путь 1 из 3: обычное чтение обученной единицей без обучения ───────────
    [Fact]
    public void OrdinaryReadPath_OfTheUntrainedUnit_GivesTheSameWordRoles()
    {
        // Обученная единица без единого урока обязана отдавать ровно то же, что
        // отдаёт разбор правилами: обходного пути для роли слова здесь нет.
        var raw = new TranslationSemanticTokenizer().Tokenize(StatementSource, SourceLanguage);
        var unit = new SemanticTokenizerUnit().Tokenize(StatementSource, SourceLanguage);

        WriteReceipt("path-1-ordinary-read.json", new
        {
            назначение = FixId,
            попытка = AttemptId,
            строка = StatementSource,
            разбор_правилами = new
            {
                образец = raw.Pattern.ToString(),
                части_речи = raw.Tokens.Select(token => token.Pos.ToString()),
                структурный_ключ = raw.StructureSignature
            },
            необученная_единица = new
            {
                образец = unit.Pattern.ToString(),
                части_речи = unit.Tokens.Select(token => token.Pos.ToString()),
                структурный_ключ = unit.StructureSignature
            }
        });

        Assert.Equal(raw.Pattern, unit.Pattern);
        Assert.Equal(raw.StructureSignature, unit.StructureSignature);
        Assert.Equal(raw.Signature, unit.Signature);

        var entity = unit.Tokens.Single(token =>
            string.Equals(token.Word, EntityWord, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(TranslationPos.NOUN, entity.Pos);
        Assert.Equal(SemanticConcept.DATA_ENTITY, entity.Concept);
    }

    // ── Путь 2 из 3: обучение ────────────────────────────────────────────────
    [Fact]
    public void TrainingPath_OfTheCorrectedRecord_AddressesTheMemoryByTheAgreedRoles()
    {
        // Поправка меняет образец предложения и подпись, но токены берёт у
        // разбора правилами. Поэтому неверная роль слова попадала прямо в адрес,
        // под которым обученная память хранит перевод.
        var unit = TrainSemanticUnit(BuildBatch(StatementRow()));
        var trained = unit.Tokenize(StatementSource, SourceLanguage);

        var verbsInKey = VerbsNamedInStructureKey(trained);
        var verbsInTokens = trained.Tokens.Count(token => token.Pos == TranslationPos.VERB);

        WriteReceipt("path-2-training.json", new
        {
            назначение = FixId,
            попытка = AttemptId,
            строка = StatementSource,
            образец_исправленной_записи = trained.Pattern.ToString(),
            полный_ключ = trained.Signature,
            структурный_ключ = trained.StructureSignature,
            пометки = trained.RuleTags,
            глаголов_в_ключе = verbsInKey,
            глаголов_в_токенах = verbsInTokens,
            токены = trained.Tokens.Select(token => new
            {
                слово = token.Word,
                часть_речи = token.Pos.ToString(),
                лексема = token.Lemma,
                понятие = token.Concept.ToString(),
                роль = token.Role
            })
        });

        // Адрес обученной записи называет один глагол, потому что в строке он
        // один.
        Assert.Equal(1, verbsInTokens);
        Assert.Equal(verbsInTokens, verbsInKey);

        // Объявленный предмет остаётся предметом и в исправленной записи.
        var entity = trained.Tokens.Single(token =>
            string.Equals(token.Word, EntityWord, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(TranslationPos.NOUN, entity.Pos);
        Assert.Equal(SemanticConcept.DATA_ENTITY, entity.Concept);
        Assert.NotEqual("IMPERATIVE", entity.Role);
    }

    // ── Путь 3 из 3: загрузка состояния с диска свежим объектом без обучения ──
    [Fact]
    public async Task FreshLoadPath_FromDisk_GivesTheSameWordRolesAsTheLiveObject()
    {
        // Обычная продуктовая машина, а не конструктор с подставленным потоком.
        var machine = new TranslationMachine();

        var batch = BuildBatch(StatementRow());
        var train = await machine.TrainStructuredBatchAsync(batch);
        Assert.True(train.IsSuccess, train.ErrorMessage);
        Assert.Equal(batch.Examples.Count, train.Value!.RowsTrained);

        // Один обычный ответ живого объекта: удержание текста наблюдается до
        // сохранения.
        var liveAnswer = await ReadThroughOrdinaryFlowAsync(machine, StatementSource);

        var checkpointPath = Path.Combine(
            Path.GetTempPath(), $"fix-124-s05-{Guid.NewGuid():N}.json");
        try
        {
            var save = await machine.SaveCheckpointAsync(checkpointPath);
            Assert.True(save.IsSuccess, save.ErrorMessage);

            var load = await TranslationMachine.LoadCheckpointAsync(checkpointPath);
            Assert.True(load.IsSuccess, load.ErrorMessage);
            var restoredMachine = load.Value!.Machine;

            // Свежий объект: восстановленная машина — не та же ссылка.
            Assert.NotSame(machine, restoredMachine);

            // Свежая единица разборщика, собранная из состояния, прочитанного с
            // диска. Обучения в этом восстановлении нет.
            var restoredUnit = new SemanticTokenizerUnit();
            var restoredExport = restoredMachine.ExportCheckpoint();
            Assert.True(restoredExport.IsSuccess, restoredExport.ErrorMessage);
            restoredUnit.ImportCheckpoint(restoredExport.Value!.SemanticTokenizer);

            var live = new SemanticTokenizerUnit();
            var liveExport = machine.ExportCheckpoint();
            Assert.True(liveExport.IsSuccess, liveExport.ErrorMessage);
            live.ImportCheckpoint(liveExport.Value!.SemanticTokenizer);

            var liveRead = live.Tokenize(StatementSource, SourceLanguage);
            var restoredRead = restoredUnit.Tokenize(StatementSource, SourceLanguage);
            var restoredAnswer = await ReadThroughOrdinaryFlowAsync(restoredMachine, StatementSource);

            var verbsInKey = VerbsNamedInStructureKey(restoredRead);
            var verbsInTokens = restoredRead.Tokens.Count(token => token.Pos == TranslationPos.VERB);

            WriteReceipt("path-3-fresh-load.json", new
            {
                назначение = FixId,
                попытка = AttemptId,
                обучено_уроков_единиц = train.Value!.RowsTrained,
                сохранений = 1,
                свежих_загрузок = 1,
                ответ_живого_объекта = liveAnswer,
                ответ_после_свежей_загрузки = restoredAnswer,
                живой_структурный_ключ = liveRead.StructureSignature,
                восстановленный_структурный_ключ = restoredRead.StructureSignature,
                глаголов_в_ключе = verbsInKey,
                глаголов_в_токенах = verbsInTokens,
                восстановленные_токены = restoredRead.Tokens.Select(token => new
                {
                    слово = token.Word,
                    часть_речи = token.Pos.ToString(),
                    лексема = token.Lemma,
                    понятие = token.Concept.ToString(),
                    роль = token.Role
                })
            });

            // Свежая загрузка обязана дать ровно то же чтение, что живой объект.
            Assert.Equal(liveRead.Pattern, restoredRead.Pattern);
            Assert.Equal(liveRead.Signature, restoredRead.Signature);
            Assert.Equal(liveRead.StructureSignature, restoredRead.StructureSignature);
            Assert.Equal(liveRead.PrefixSignature, restoredRead.PrefixSignature);

            // И это чтение согласовано: один глагол в токенах и один в адресе.
            Assert.Equal(1, verbsInTokens);
            Assert.Equal(verbsInTokens, verbsInKey);

            var entity = restoredRead.Tokens.Single(token =>
                string.Equals(token.Word, EntityWord, StringComparison.OrdinalIgnoreCase));
            Assert.Equal(TranslationPos.NOUN, entity.Pos);
            Assert.Equal(SemanticConcept.DATA_ENTITY, entity.Concept);

            // Правильный ответ удержан: эталон читается только здесь, после
            // получения фактических ответов.
            Assert.Equal("He checked the file", liveAnswer);
            Assert.Equal("He checked the file", restoredAnswer);
        }
        finally
        {
            if (File.Exists(checkpointPath)) File.Delete(checkpointPath);
        }
    }

    // ── Вспомогательное ──────────────────────────────────────────────────────

    // StatementRow — учебная строка повествовательного предложения с прямым
    // объектом «файл». Отвергнутый вариант намеренно заявляет другое время.
    private static TranslationTrainReadyPair StatementRow()
    {
        return new TranslationTrainReadyPair(
            Id: "124-S05-r1",
            Source: "simulation",
            Direction: "RU_EN",
            PairFamily: "word-role",
            Prompt: "Переведи",
            ASide: StatementSource,
            Chosen: "He checked the file",
            Rejected: "He checks the file [WRONG: tense] [RULE: TENSE_PRESENT]",
            ValidationOutcome: "train_ready",
            ValidationErrors: [],
            VerifiedRuleTags: ["TENSE_PAST"]);
    }

    // BuildBatch строит учебную партию обычным продуктовым конструктором.
    private static TranslationLearningBatch BuildBatch(params TranslationTrainReadyPair[] pairs)
    {
        return TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
            "124-S05-main-v1",
            pairs,
            signatureMode: TranslationSignatureMode.Structural);
    }

    // TrainSemanticUnit скармливает уроки разборщика той же обученной единице,
    // которую использует продуктовая машина.
    private static SemanticTokenizerUnit TrainSemanticUnit(TranslationLearningBatch batch)
    {
        var unit = new SemanticTokenizerUnit();
        foreach (var example in batch.Examples
            .Where(example => example.UnitId == TranslationUnitIds.SemanticTokenizer))
            unit.TrainOnExample(example);

        return unit;
    }

    // ReadThroughOrdinaryFlowAsync делает одно обычное чтение перевода: тот же
    // распорядитель и тот же поток, которым пользуется пользователь.
    private static async Task<string> ReadThroughOrdinaryFlowAsync(
        TranslationMachine machine,
        string source)
    {
        var orchestrator = await machine.GetOrchestratorAsync();
        Assert.True(orchestrator.IsSuccess, orchestrator.ErrorMessage);

        var result = await orchestrator.Value!.RunAsync(new LogicalOrchestratorRequest(
            RunId: $"fix-124-s05-{Guid.NewGuid():N}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: source,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = SourceLanguage,
                ["target_language"] = TargetLanguage,
                ["topic_domain"] = "general"
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!.FinalHumanResult;
    }

    // VerbsNamedInStructureKey считает, сколько раз структурный ключ называет действие в своём
    // перечне частей речи.
    //
    // Считается именно перечень частей речи, а не весь ключ целиком: ключ несёт после него ещё и
    // пометки, и среди них есть «RU:VERB_GENDER:MASC», в котором слово «VERB» встречается как
    // часть названия пометки. Счёт по всему ключу давал бы лишнюю единицу и прятал настоящее
    // число. Структурный ключ устроен так: сначала образец предложения, затем по одной части речи
    // на каждое слово, затем пометки.
    private static int VerbsNamedInStructureKey(SemanticTokenSequence sequence)
    {
        var segments = sequence.StructureSignature.Split('|');

        return segments
            .Skip(1)
            .Take(sequence.Tokens.Count)
            .Count(segment => string.Equals(segment, nameof(TranslationPos.VERB), StringComparison.Ordinal));
    }

    // PartsOfSpeechNamedInStructureKey отдаёт тот же перечень частей речи ключа как список.
    private static IReadOnlyList<string> PartsOfSpeechNamedInStructureKey(SemanticTokenSequence sequence)
    {
        return sequence.StructureSignature
            .Split('|')
            .Skip(1)
            .Take(sequence.Tokens.Count)
            .ToList();
    }

    // EvidenceDirectory — каталог улик этой попытки, разрешённый приёмником доказательств.
    private static string EvidenceDirectory() => AllowedEvidenceDirectory.Value;

    // Корень рабочего дерева: подъём от каталога сборки проверок до каталога улик.
    private static string? WorkingTreeRootOrNull()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "artifacts", "fbe")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return null;
    }

    // Куда этому прогону разрешено писать улики. Решение принимает общий приёмник
    // доказательств ОДИН раз за прогон, по одному факту: отслеживает ли хранилище файлы
    // внутри назначенного каталога улик.
    //
    // Почему это нужно здесь. Назначенный каталог улик этой попытки уже опубликован: его
    // файлы отслеживаются хранилищем. Значит каждый следующий прогон перезаписывал
    // опубликованные улики. Пока каталог не опубликован, прогон пишет в него и остаётся в
    // хранилище — это запись наблюдения. После публикации тот же прогон становится
    // проверочным и заметно уходит в игнорируемый каталог результатов испытаний.
    private static readonly Lazy<string> AllowedEvidenceDirectory = new(() =>
    {
        var root = WorkingTreeRootOrNull();

        // Корень не найден: улики пишутся рядом со сборкой, и это видно по пути.
        if (root is null)
        {
            var fallback = Path.Combine(AppContext.BaseDirectory, FixId, AttemptId);
            Directory.CreateDirectory(fallback);
            return fallback;
        }

        // Назначенный каталог улик этой попытки.
        var declared = Path.Combine(root, "artifacts", "fbe", FixId, AttemptId);

        // Разрешённый каталог выдаёт приёмник, он же сообщает род прогона и точный путь.
        return ExperimentFixes.ExperimentEvidenceSink
            .ForDeclaredDirectory(root, declared)
            .EnsureWriteRootDirectory();
    });

    // WriteReceipt пишет один файл улик в кодировке UTF-8 без метки порядка
    // байтов, только с переводом строки Unix и ровно одним в конце.
    private static void WriteReceipt(string fileName, object payload)
    {
        var path = Path.Combine(EvidenceDirectory(), fileName);

        var json = JsonSerializer.Serialize(payload, ReceiptJson)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);
        if (!json.EndsWith('\n')) json += "\n";
        File.WriteAllText(path, json, new UTF8Encoding(false));
    }
}
