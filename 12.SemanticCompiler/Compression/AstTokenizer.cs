// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler;

// AstTokenizer extracts identifier words from C# source using Roslyn's AST.
// E01: +93% perplexity reduction vs BPE (Byte Pair Encoding) tokenization.
// Three levels of compression available: Level0 (raw), Level1 (E15), Level2 (E16).
public static class AstTokenizer
{
    // Level 0: raw identifier words — baseline used to measure E15 improvement
    public static IReadOnlyList<string> Tokenize(string csharpSource)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return Array.Empty<string>();
        return SemanticTokenizerPipeline.Level0Tokens(csharpSource).AsReadOnly();
    }

    // Level 1: stopword removal + suffix stemming (E15)
    public static IReadOnlyList<string> TokenizeNormalized(string csharpSource)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return Array.Empty<string>();
        return SemanticTokenizerPipeline.Level1Tokens(csharpSource).AsReadOnly();
    }

    // Level 2: semantic op families (E16)
    public static IReadOnlyList<string> TokenizeSemanticOps(string csharpSource)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return Array.Empty<string>();
        return SemanticTokenizerPipeline.Level2Tokens(csharpSource).AsReadOnly();
    }

    // Учебный путь, чувствительный к полярности имени: разбивка первого уровня, в которой слова
    // отрицания НЕ считаются служебными.
    //
    // Зачем отдельный вход, а не правка прежних. Обучаемая единица, которая учится по словам
    // имени, после удаления «not» и «no» не может отличить «IsNotEmpty» от «IsEmpty»: на входе
    // остаются одни и те же слова. Это измеренная логическая ошибка распознавания. Но общее
    // исключение этих двух слов из набора служебных стоит дорого и тоже измерено: запутанность
    // продуктового оценщика хуже на 24,27 процента. Поэтому исключения передаёт ТОЛЬКО этот
    // учебный вход, а прежние три входа и путь сжатия остаются побайтово прежними и дорогой цены
    // не платят.
    //
    // Чего этот вход НЕ делает: он не объявляет никакую единицу обученной и не переключает ни
    // одного действующего вызывающего. Подключение конкретной обучаемой единицы к этому входу —
    // отдельный кусок со своим измерением до и после, потому что переключение общего пути
    // изменило бы уже записанные потоки других опытов.
    public static IReadOnlyList<string> TokenizeForPolaritySensitiveTraining(string csharpSource)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return Array.Empty<string>(); // Пустой источник даёт пустой набор, как и прежние входы.

        // Набор полярности передаётся доводом; общий набор служебных слов не тронут.
        return SemanticTokenizerPipeline
            .Level1Tokens(csharpSource, SemanticTokenizerPipeline.PolarityKeepWords)
            .AsReadOnly();
    }

    // Level 3: logical predicate string tokens (E17) — use LogicExtractor for integers
    public static IReadOnlyList<string> TokenizePredicates(string csharpSource)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return Array.Empty<string>();
        return SemanticTokenizerPipeline.Level3Tokens(csharpSource).AsReadOnly();
    }
}
