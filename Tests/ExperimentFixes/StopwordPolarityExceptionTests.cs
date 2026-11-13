// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using SemanticCompiler;

// Что здесь проверяется, по-человечески.
//
// Это перенос исправления опыта сто двадцать в форме исключений как довода (запись
// «Отрицание переживает сжатие первого уровня» в едином реестре переносов,
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md).
//
// Что было неправильно. Слова «not» и «no» стоят в общем наборе служебных слов, поэтому разбивка
// первого уровня их удаляет. Обучаемая единица, которая учится по словам имени, после этого
// физически не может отличить «IsNotEmpty» от «IsEmpty»: на входе у неё остаются одни и те же
// слова. Это логическая ошибка распознавания, измеренная опытом: сохранность отрицаний 0 из 2
// без правки и 2 из 2 с правкой.
//
// Почему нельзя было просто убрать два слова из набора. Эта правка тоже измерена, и она дорогая:
// запутанность продуктового оценщика 3,8458932055013695 → 4,779225984271398, то есть хуже на
// 24,27 процента, а словарь плеча 6 → 8 слов. По правилу владельца ухудшение больше пяти
// процентов перенос запрещает, и процессорное время тут ни при чём.
//
// Что стало. Общий набор служебных слов НЕ тронут ни на одно слово. Добавлен необязательный
// последний довод «слова-исключения» со значением «пусто», поэтому по умолчанию поведение
// побайтово прежнее. Исключения передаёт только учебный вход, чувствительный к полярности;
// нормализатор, собиратель семейств и путь сжатия довода не передают и остаются прежними —
// именно поэтому измеренные 24,27 процента здесь не возникают.
//
// Первый случай ниже — доказательство НУЛЕВОЙ ЦЕНЫ ПО ПОСТРОЕНИЮ: при пустом списке исключений
// выход совпадает с выходом прежней разбивки слово в слово.
//
// Прежнее правило воспроизведено функцией ОТ ВХОДА, и зависимость от входа показана вторым
// входом. Прогона не было: прямые команды сборки и прогона тестов запрещены политикой разрешений
// этой среды, я их не запускал и зелёными не называю.
public sealed class StopwordPolarityExceptionTests
{
    /// <summary>Исходник с двумя именами, которые различаются ровно отрицанием.</summary>
    private const string PolaritySource = """
        public static class Guards
        {
            public static bool IsNotEmpty(string value) => value.Length > 0;
            public static bool IsEmpty(string value) => value.Length == 0;
        }
        """;

    /// <summary>Исходник с обычными служебными словами и без отрицаний.</summary>
    private const string OrdinaryStopwordSource = """
        public static class Registry
        {
            public static string GetTheItemFromCache(string key) => key;
        }
        """;

    // ── Прежнее правило, воспроизведённое для доказательства красноты ───────────

    /// <summary>
    /// ПРЕЖНЕЕ правило: служебным считалось любое слово из общего набора, без исключений для
    /// вызывающего. Настоящая функция от входа: ответ считается из поданного слова.
    /// Воспроизведено здесь только для сравнения.
    /// </summary>
    private static bool PriorIsStopwordWithoutExceptions(string word)
        => SemanticTokenizerPipeline.IsStopword(word); // Довода исключений прежде не было вовсе.

    // ── Случай 1: нулевая цена по построению ───────────────────────────────────

    [Fact]
    public void WithoutExceptions_TheOutputIsWordForWordTheSameAsBefore()
    {
        // Разбивка без довода и разбивка с ПУСТЫМ списком исключений обязаны дать один результат:
        // именно это и означает «общий набор не тронут, по умолчанию ничего не изменилось».
        var withoutArgument = SemanticTokenizerPipeline.Level1Tokens(PolaritySource);
        var withEmptyExceptions = SemanticTokenizerPipeline.Level1Tokens(
            PolaritySource, new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        Assert.Equal(withoutArgument, withEmptyExceptions);

        // То же на втором уровне: довод проходит насквозь и по умолчанию ничего не меняет.
        Assert.Equal(
            SemanticTokenizerPipeline.Level2Tokens(PolaritySource),
            SemanticTokenizerPipeline.Level2Tokens(
                PolaritySource, new HashSet<string>(StringComparer.OrdinalIgnoreCase)));

        // И на обычном исходнике со служебными словами без отрицаний — тоже слово в слово.
        Assert.Equal(
            SemanticTokenizerPipeline.Level1Tokens(OrdinaryStopwordSource),
            SemanticTokenizerPipeline.Level1Tokens(
                OrdinaryStopwordSource, new HashSet<string>(StringComparer.OrdinalIgnoreCase)));
    }

    // ── Случай 2: учебный путь сохраняет отрицание ─────────────────────────────

    [Fact]
    public void ThePolaritySensitiveTrainingPath_KeepsNegation_WhileThePriorRuleDroppedIt()
    {
        // Прежнее правило на том же входе отвечает НЕВЕРНО для учебного смысла: оба слова
        // отрицания считаются служебными и удаляются, поэтому «IsNotEmpty» и «IsEmpty» дают на
        // входе обучения одни и те же слова.
        Assert.True(PriorIsStopwordWithoutExceptions("not"));
        Assert.True(PriorIsStopwordWithoutExceptions("no"));
        // Зависимость прежнего правила от входа: на содержательном слове его ответ другой.
        Assert.False(PriorIsStopwordWithoutExceptions("empty"));

        // Действующий учебный вход сохраняет отрицание.
        var trainingTokens = AstTokenizer.TokenizeForPolaritySensitiveTraining(PolaritySource);
        Assert.Contains("not", trainingTokens, StringComparer.OrdinalIgnoreCase);

        // А прежний вход разбивки первого уровня его по-прежнему удаляет: пути разные, и это
        // главное свойство этой формы переноса.
        var compressionTokens = AstTokenizer.TokenizeNormalized(PolaritySource);
        Assert.DoesNotContain("not", compressionTokens, StringComparer.OrdinalIgnoreCase);
    }

    // ── Случай 3: путь сжатия и нормализатор не изменились ─────────────────────

    [Fact]
    public void TheCompressionPathAndNormalizer_StillRemoveNegation_AndKeepTheirOwnOutput()
    {
        // Путь сжатия: слова отрицания удаляются, как и прежде.
        var level1 = AstTokenizer.TokenizeNormalized(PolaritySource);
        Assert.DoesNotContain("not", level1, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("no", level1, StringComparer.OrdinalIgnoreCase);

        // Нормализатор зовёт общую проверку без довода, поэтому для него отрицание служебное.
        Assert.True(SemanticTokenizerPipeline.IsStopword("not"));
        Assert.True(SemanticTokenizerPipeline.IsStopword("no"));

        // Прочие служебные слова ведут себя как прежде на ОБОИХ путях: исключения названы ровно
        // два слова, и больше ничего из набора не затронуто.
        foreach (var ordinary in new[] { "the", "is", "and", "from", "with" })
        {
            Assert.True(SemanticTokenizerPipeline.IsStopword(ordinary));
            Assert.True(SemanticTokenizerPipeline.IsStopword(ordinary, SemanticTokenizerPipeline.PolarityKeepWords));
        }

        // И на обычном исходнике учебный вход даёт то же, что прежний: отрицаний там нет, значит
        // и расхождения быть не должно.
        Assert.Equal(
            AstTokenizer.TokenizeNormalized(OrdinaryStopwordSource),
            AstTokenizer.TokenizeForPolaritySensitiveTraining(OrdinaryStopwordSource));
    }

    // ── Случай 4: набор исключений назван ровно двумя словами ──────────────────

    [Fact]
    public void ThePolarityKeepSet_IsExactlyTheTwoNegationWords()
    {
        // Набор полярности назван явно и не растёт молча: ровно два слова.
        Assert.Equal(2, SemanticTokenizerPipeline.PolarityKeepWords.Count);
        Assert.Contains("not", SemanticTokenizerPipeline.PolarityKeepWords);
        Assert.Contains("no", SemanticTokenizerPipeline.PolarityKeepWords);

        // Слово вне набора исключением не становится даже при переданном наборе.
        Assert.True(SemanticTokenizerPipeline.IsStopword("the", SemanticTokenizerPipeline.PolarityKeepWords));

        // А слово из набора служебным быть перестаёт — но только для того, кто набор передал.
        Assert.False(SemanticTokenizerPipeline.IsStopword("not", SemanticTokenizerPipeline.PolarityKeepWords));
    }

    // ── Случай 5: пустой исходник ведёт себя как прежде ───────────────────────

    [Fact]
    public void EmptySource_GivesAnEmptyResultOnBothPaths()
    {
        // Краевой случай: пустой исходник даёт пустой набор на обоих путях, как и прежние входы.
        Assert.Empty(AstTokenizer.TokenizeForPolaritySensitiveTraining("   "));
        Assert.Empty(AstTokenizer.TokenizeNormalized("   "));
    }
}
