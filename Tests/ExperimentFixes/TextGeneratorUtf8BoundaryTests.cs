// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using Inference;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

// Что здесь проверяется, по-человечески.
//
// Что было неправильно. Обычная выдача ответа останавливалась по ЧИСЛУ ТОКЕНОВ: либо токен
// признака конца, либо достижение предела длины. Словарь этого токенизатора байтовый — один
// токен равен одному байту, — поэтому одна русская буква занимает два токена, а составной
// символ четыре. Предел длины мог прийтись на середину символа. Дальше байты превращались в
// строку одним вызовом, и незаконченный символ становился символом замены: ответ рвался посреди
// символа ТИХО, без отказа и без следа.
//
// Доказательство дефекта по коду: `09.Inference/TextGenerator.cs`, условие остановки
// `if (nextToken == eosToken || generated.Count == maxNewTokens) break;` считает ТОКЕНЫ, а
// `01.Tokenizer/BpeTokenizer.cs:249` превращает накопленные байты в строку целиком. Байтовость
// словаря видна там же: `.ToDictionary(i => i, i => new byte[] { (byte)i })` — 256 однобайтовых
// записей.
//
// Что стало. В текст идёт только тот начальный кусок байтов, который целиком состоит из
// законченных символов. Незаконченный хвост в текст не попадает, и его длина названа в записи
// результата отдельным числом, поэтому укорачивание не тихое.
//
// Чего это НЕ лечит, названо прямо. Граница держит ОДИН символ кодировки. Составной знак из
// нескольких символов — буква со отдельным знаком ударения, флаг из двух указателей страны,
// цепочка с нулевым соединителем, вариантная пометка — всё ещё может разделиться, потому что
// каждый его символ сам законен и закончен. Это отдельный класс случаев и отдельная работа:
// нужны правила разбиения на видимые знаки, а не проверка байтов.
//
// Проверки идут настоящим путём чтения ответа: подставляется только источник номеров, а разбор
// ответа и сборка текста остаются продуктовыми.
//
// Прогона не было: прямые команды сборки и прогона тестов запрещены политикой разрешений
// проекта, и обходить запрет оболочкой-посредником я не стал.
public sealed class TextGeneratorUtf8BoundaryTests
{
    // Байтовый словарь: 256 однобайтовых записей, то есть номер токена равен номеру байта.
    private const int ByteCodecVocabularySize = 256;

    // Русская буква «Я» в кодировке UTF-8 — два байта: 208 и 175.
    private const int CyrillicLeadByte = 208;
    private const int CyrillicContinuationByte = 175;
    private const string CyrillicLetter = "Я";

    // Символ замены — то, во что превращался разорванный символ в прежнем поведении. Он собран
    // из номера, а не записан буквой в исходнике: настоящий символ замены внутри кода — это ровно
    // та примета испорченной кодировки, которую ищут проверки формата, и держать его в файле
    // нельзя даже внутри строки сравнения.
    private static readonly string ReplacementCharacter = ((char)0xFFFD).ToString();

    // ── Проверка 1: граница попала внутрь символа ─────────────────────────────

    [Fact]
    public void WhenTheLimitFallsInsideACharacter_TheAnswerIsWholeAndTheTornTailIsNamed()
    {
        // Поток номеров: целая буква, а затем первый байт следующей такой же буквы. Предел
        // длины три токена, поэтому второй символ остаётся незаконченным.
        var receipt = ReadReceipt([CyrillicLeadByte, CyrillicContinuationByte, CyrillicLeadByte]);

        // В тексте только законченный символ: разорванного символа и символа замены в нём нет.
        Assert.Equal(CyrillicLetter, receipt.Text);
        Assert.DoesNotContain(ReplacementCharacter, receipt.Text);
        // Отброшенный хвост назван числом, а не потерян молча.
        Assert.Equal(1, receipt.HeldBackByteCount);
        // Число выданных токенов прежнее: укорачивание касается текста, а не учёта выдачи.
        Assert.Equal(3, receipt.GeneratedTokenCount);
    }

    // ── Проверка 2: положительный контроль, ответ целиком в пределах ──────────

    [Fact]
    public void WhenTheAnswerFitsWholeCharacters_NothingChanges()
    {
        // Поток номеров: ровно одна целая буква, предел длины два токена.
        var receipt = ReadReceipt([CyrillicLeadByte, CyrillicContinuationByte]);

        // Текст прежний, ни один байт не отброшен.
        Assert.Equal(CyrillicLetter, receipt.Text);
        Assert.Equal(0, receipt.HeldBackByteCount);
        Assert.Equal(2, receipt.GeneratedTokenCount);
    }

    // ── Проверка 3: предел длины меньше одного символа ────────────────────────

    [Fact]
    public void WhenTheLimitIsSmallerThanOneCharacter_TheResultIsUnderstandableNotTornAndNotAnException()
    {
        // Поток номеров: только первый байт двухбайтовой буквы, предел длины один токен.
        // Законченных символов в ответе нет вовсе.
        var receipt = ReadReceipt([CyrillicLeadByte]);

        // Текст пустой — честно короче, а не мусор из символа замены.
        Assert.Equal(string.Empty, receipt.Text);
        Assert.DoesNotContain(ReplacementCharacter, receipt.Text);
        // Отброшен ровно один байт, и это сказано числом.
        Assert.Equal(1, receipt.HeldBackByteCount);
        // Причина остановки прежняя и понятная, исключения нет.
        Assert.Equal("LENGTH_LIMIT", receipt.StopReason);
        Assert.Equal(1, receipt.GeneratedTokenCount);
    }

    // ── Оснастка ──────────────────────────────────────────────────────────────

    // ReadReceipt читает ответ обычным продуктовым путём на заданном потоке номеров и
    // возвращает полную запись результата. Длина ответа равна длине потока, температура ноль —
    // жадная ветвь.
    private static TextGenerationReceipt ReadReceipt(int[] stream)
        => GeneratorReadingExactly(ByteCodecVocabulary(), stream)
            .GenerateDetailed("abc", stream.Length, 0f, 0, 1f, 7, null, CancellationToken.None);

    // ByteCodecVocabulary строит байтовый словарь без обучения склейкам: номер токена равен
    // номеру байта, поэтому поток номеров — это прямо байты ответа.
    private static BpeTokenizer ByteCodecVocabulary()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("abc", ByteCodecVocabularySize);
        return tokenizer;
    }

    // TinyConfig — крошечная настройка модели: здесь она только носитель формы, потому что
    // номера подставляются потоком, а не берутся у модели.
    private static ModelConfig TinyConfig(int vocabSize) => new()
    {
        VocabSize = vocabSize,
        ContextLen = 128,
        HiddenDim = 32,
        NumLayers = 2,
        NumHeads = 4,
        NumKvHeads = 2,
        FfnHiddenDim = 64
    };

    // GeneratorReadingExactly — генератор, читающий ровно заданный поток номеров. Подставляется
    // только источник числа; сам разбор ответа и сборка текста остаются продуктовыми.
    private static TextGenerator GeneratorReadingExactly(BpeTokenizer tokenizer, int[] stream)
    {
        var index = 0;
        return new TextGenerator(
            new LlmModel(TinyConfig(tokenizer.VocabSize)),
            tokenizer,
            layers => new KvCache(layers),
            (_, _, _, _, _) => stream[Math.Min(index++, stream.Length - 1)]);
    }
}
