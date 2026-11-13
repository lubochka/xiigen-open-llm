// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using ExperimentsV2;

// Что здесь проверяется, по-человечески.
//
// Это раздел ремонта про файловую границу и происхождение концов пары (пятый по порядку
// разделов в файле codex-139-post117-addressed-repair-plan-2026-11-03.md).
//
// Что было неправильно. Нарезка учебных пар складывала предикаты ВСЕХ файлов в один список и
// брала соседние элементы. На каждой границе файлов это давало пару-шов: последний предикат
// одного файла становился «предыдущим» для первого предиката следующего файла. Такой пары в
// материале нет вовсе — её изобрела склейка, а обучение считало её законным переходом. У пары
// при этом не было ни проекта, ни файла, ни позиции, поэтому отличить законную пару от шва или
// от пары чужого проекта было нечем: совпадения числа пар и значений считалось достаточно.
//
// Что стало. Каждый файл объявляется доверенным потоком со своим ключом проекта и файла и
// отпечатком, пересчитанным из фактических опознаний; пары берутся только внутри потока, и у
// каждой есть проверяемое происхождение: ключ, позиция и оба значения. Все отказы названы
// кодами и возвращаются результатом, а не исключением.
//
// Прежнее правило воспроизведено ниже функцией ОТ ВХОДА (PriorFlattenedPairs): она принимает те
// же потоки файлов и считает прежний ответ. Каждое красное утверждение двойное: прежнее правило
// на том же входе даёт неверный ответ, действующее — верный. Зависимость от входа показана
// вторым входом.
//
// Прогона не было: прямые команды сборки и прогона тестов запрещены политикой разрешений этой
// среды, я их не запускал и зелёными не называю.
public sealed class E43PairProvenanceTests
{
    /// <summary>Проект, которому принадлежат оба файла законного случая.</summary>
    private const string ProjectA = "projectAlpha";

    /// <summary>Соседний проект с ТАКОЙ ЖЕ меткой файла.</summary>
    private const string ProjectB = "projectBeta";

    /// <summary>Метка первого файла.</summary>
    private const string FileOne = "src/One.cs";

    /// <summary>Метка второго файла.</summary>
    private const string FileTwo = "src/Two.cs";

    // ── Прежнее правило, воспроизведённое для доказательства красноты ───────────

    /// <summary>
    /// ПРЕЖНЕЕ правило нарезки: предикаты всех файлов складывались в один список, и пары
    /// брались по соседним элементам этого списка. Настоящая функция от входа: ответ считается
    /// из поданных потоков файлов. Воспроизведено здесь только для сравнения.
    /// </summary>
    private static List<(int Current, int Next)> PriorFlattenedPairs(
        IReadOnlyList<IReadOnlyList<int>> perFileTokens)
    {
        var flattened = new List<int>();                 // Один список на все файлы — тот самый дефект.
        foreach (var file in perFileTokens) flattened.AddRange(file); // Границы файлов стираются здесь.
        return flattened.Zip(flattened.Skip(1))          // Соседние элементы склеенного списка.
            .Select(pair => (pair.First, pair.Second))    // Пара из двух соседей.
            .ToList();
    }

    /// <summary>
    /// ВЫРОЖДЕННЫЙ проверяющий «только по числу»: он сверяет лишь количество пар. Нужен затем,
    /// чтобы показать, что такой проверяющий полный набор случаев не проходит.
    /// </summary>
    private static bool CountOnlyVerifierAccepts(int expectedCount, int actualCount)
        => expectedCount == actualCount; // Число совпало — и этого ему достаточно.

    /// <summary>
    /// ВЫРОЖДЕННЫЙ проверяющий «только по значениям»: он сверяет лишь множества значений концов,
    /// не глядя ни на проект, ни на файл, ни на позицию.
    /// </summary>
    private static bool ValueOnlyVerifierAccepts(
        IReadOnlyList<(int Current, int Next)> expected,
        IReadOnlyList<(int Current, int Next)> actual)
        => expected.Count == actual.Count &&
           expected.OrderBy(p => p.Current).ThenBy(p => p.Next)
               .SequenceEqual(actual.OrderBy(p => p.Current).ThenBy(p => p.Next));

    // ── Случай 1: законная пара внутри файла, и шва на границе файлов больше нет ──

    [Fact]
    public void WithinFilePairsAreLawful_AndThePriorRuleInventedACrossFileSeam()
    {
        // Два файла одного проекта. Шов прежнего правила был бы парой (3, 10): тройка — последний
        // предикат первого файла, десятка — первый предикат второго.
        var fileOne = new[] { 1, 2, 3 };
        var fileTwo = new[] { 10, 11 };

        // Прежнее правило на том же входе отвечает НЕВЕРНО: среди его пар есть (3, 10), которой
        // нет ни в одном файле.
        var prior = PriorFlattenedPairs([fileOne, fileTwo]);
        Assert.Contains((3, 10), prior);
        Assert.Equal(4, prior.Count); // Три законные пары плюс один шов.

        // Действующее правило отвечает верно: шва нет вовсе, и пар ровно столько, сколько их
        // внутри файлов.
        var streams = BuiltStreams((ProjectA, FileOne, fileOne), (ProjectA, FileTwo, fileTwo));
        var extracted = E43PairProvenance.ExtractPairsWithinStreams(streams);
        Assert.True(extracted.IsSuccess, extracted.ErrorMessage);
        var pairs = extracted.Value!;
        Assert.Equal(3, pairs.Count);
        Assert.DoesNotContain(pairs, pair => pair.Current == 3 && pair.Next == 10);

        // Каждая пара несёт своё происхождение: проект, файл, позицию и оба значения, и каждая
        // проходит проверку против фактического потока.
        foreach (var pair in pairs)
        {
            var belongs = E43PairProvenance.RequirePairBelongsToStream(streams, pair);
            Assert.True(belongs.IsSuccess, belongs.ErrorMessage);
        }

        // И прежнее правило действительно зависит от входа: на одном файле шва нет и у него,
        // поэтому его неверный ответ берётся именно из границы файлов, а не из самой склейки.
        var priorSingleFile = PriorFlattenedPairs([fileOne]);
        Assert.DoesNotContain((3, 10), priorSingleFile);
        Assert.NotEqual(prior.Count, priorSingleFile.Count);
    }

    // ── Случай 2: пара через границу файлов отвергается названным кодом ──────────

    [Fact]
    public void CrossFilePairIsRefusedByPosition_WhileThePriorRuleAcceptedItSilently()
    {
        var fileOne = new[] { 1, 2, 3 };
        var fileTwo = new[] { 10, 11 };
        var streams = BuiltStreams((ProjectA, FileOne, fileOne), (ProjectA, FileTwo, fileTwo));

        // Прежнее правило такую пару молча считало законной: она есть в его списке.
        Assert.Contains((3, 10), PriorFlattenedPairs([fileOne, fileTwo]));

        // Действующая проверка отвергает её своим кодом: второй конец не следующий элемент
        // того же потока. Позиция 2 — последняя в первом файле, следующего элемента там нет.
        var seam = new E43TrustedPair(ProjectA, FileOne, 2, 3, 10);
        var refused = E43PairProvenance.RequirePairBelongsToStream(streams, seam);
        Assert.False(refused.IsSuccess);
        Assert.Equal(E43PairProvenance.WrongPositionCode, refused.ErrorCode);
    }

    // ── Случай 3: та же метка файла в другом проекте — другой материал ───────────

    [Fact]
    public void SameFileLabelInAnotherProject_IsADifferentStream()
    {
        var tokens = new[] { 1, 2, 3 };

        // Одна и та же метка файла в двух проектах даёт РАЗНЫЕ отпечатки: проект входит в ключ.
        var inA = E43PairProvenance.ComputeStreamHash(ProjectA, FileOne, tokens);
        var inB = E43PairProvenance.ComputeStreamHash(ProjectB, FileOne, tokens);
        Assert.NotEqual(inA, inB);

        // Пара, названная от чужого проекта, отвергается как неизвестный поток.
        var streams = BuiltStreams((ProjectA, FileOne, tokens));
        var foreign = new E43TrustedPair(ProjectB, FileOne, 0, 1, 2);
        var refused = E43PairProvenance.RequirePairBelongsToStream(streams, foreign);
        Assert.False(refused.IsSuccess);
        Assert.Equal(E43PairProvenance.UnknownStreamCode, refused.ErrorCode);

        // Вырожденный проверяющий «только по значениям» этот случай ПРОПУСКАЕТ: значения концов
        // в обоих проектах одинаковы. Это и есть причина, по которой он не годится.
        var sameValues = new[] { (1, 2), (2, 3) };
        Assert.True(ValueOnlyVerifierAccepts(sameValues, sameValues));
    }

    // ── Случай 4: сменился только проект, токены прежние ────────────────────────

    [Fact]
    public void ProjectSwapAloneChangesIdentity_EvenWithUnchangedTokens()
    {
        var tokens = new[] { 7, 8, 9 };

        // Токены те же, сменился только проект — опознание потока обязано отличаться.
        var before = E43PairProvenance.BuildStream(ProjectA, FileOne, tokens);
        var after = E43PairProvenance.BuildStream(ProjectB, FileOne, tokens);
        Assert.True(before.IsSuccess, before.ErrorMessage);
        Assert.True(after.IsSuccess, after.ErrorMessage);
        Assert.NotEqual(before.Value!.ContentHash, after.Value!.ContentHash);

        // Число пар при этом одинаково, поэтому вырожденный проверяющий «только по числу» такую
        // подмену не замечает вовсе.
        Assert.True(CountOnlyVerifierAccepts(2, 2));
    }

    // ── Случай 5: содержимое заменено при том же числе токенов ──────────────────

    [Fact]
    public void SameCountContentReplacement_IsDetectedByTheRecomputedHash()
    {
        // Три токена до и три после — число совпадает, значения другие.
        var original = E43PairProvenance.BuildStream(ProjectA, FileOne, [1, 2, 3]);
        var replaced = E43PairProvenance.BuildStream(ProjectA, FileOne, [1, 2, 4]);
        Assert.True(original.IsSuccess, original.ErrorMessage);
        Assert.True(replaced.IsSuccess, replaced.ErrorMessage);
        Assert.NotEqual(original.Value!.ContentHash, replaced.Value!.ContentHash);

        // Вырожденный проверяющий «только по числу» подмену пропускает: число пар то же.
        Assert.True(CountOnlyVerifierAccepts(2, 2));

        // А пара, названная старым значением, отвергается по значению на этой позиции.
        var streams = new[] { replaced.Value! };
        var stalePair = new E43TrustedPair(ProjectA, FileOne, 1, 2, 3);
        var refused = E43PairProvenance.RequirePairBelongsToStream(streams, stalePair);
        Assert.False(refused.IsSuccess);
        Assert.Equal(E43PairProvenance.WrongPositionCode, refused.ErrorCode);
    }

    // ── Случай 6: двойной ключ и испорченный отпечаток — названные отказы ───────

    [Fact]
    public void DuplicateKeyMalformedAndShortHash_AreTypedRefusalsNotExceptions()
    {
        var first = E43PairProvenance.BuildStream(ProjectA, FileOne, [1, 2, 3]);
        Assert.True(first.IsSuccess, first.ErrorMessage);

        // Два потока с одним ключом: происхождение стало бы двусмысленным.
        var duplicate = E43PairProvenance.RequireTrustedStreams([first.Value!, first.Value!]);
        Assert.False(duplicate.IsSuccess);
        Assert.Equal(E43PairProvenance.DuplicateStreamKeyCode, duplicate.ErrorCode);

        // Короткий отпечаток: обрезок сравнивать нельзя.
        var shortHash = new E43TrustedStream(ProjectA, FileOne, [1, 2, 3], "abc123");
        var shortRefused = E43PairProvenance.RequireTrustedStreams([shortHash]);
        Assert.False(shortRefused.IsSuccess);
        Assert.Equal(E43PairProvenance.ShortHashCode, shortRefused.ErrorCode);

        // Испорченная форма отпечатка при правильной длине: не шестнадцатеричная запись.
        var malformed = new E43TrustedStream(ProjectA, FileOne, [1, 2, 3], new string('z', 64));
        var malformedRefused = E43PairProvenance.RequireTrustedStreams([malformed]);
        Assert.False(malformedRefused.IsSuccess);
        Assert.Equal(E43PairProvenance.MalformedHashCode, malformedRefused.ErrorCode);

        // Правильная форма и длина, но отпечаток не от этого материала: судьёй является
        // пересчитанный отпечаток, а не объявленный.
        var wrongHash = new E43TrustedStream(ProjectA, FileOne, [1, 2, 3], new string('a', 64));
        var wrongRefused = E43PairProvenance.RequireTrustedStreams([wrongHash]);
        Assert.False(wrongRefused.IsSuccess);
        Assert.Equal(E43PairProvenance.HashMismatchCode, wrongRefused.ErrorCode);

        // Пустой ключ тоже отказ, а не исключение.
        var emptyKey = E43PairProvenance.BuildStream(string.Empty, FileOne, [1, 2]);
        Assert.False(emptyKey.IsSuccess);
        Assert.Equal(E43PairProvenance.EmptyStreamKeyCode, emptyKey.ErrorCode);
    }

    // ── Случай 7: вырожденные проверяющие полный набор не проходят ──────────────

    [Fact]
    public void DegenerateVerifiersFailTheFullSet_WhileTheProvenanceVerifierPassesIt()
    {
        var tokens = new[] { 1, 2, 3 };
        var streams = BuiltStreams((ProjectA, FileOne, tokens));
        var samePairs = new[] { (1, 2), (2, 3) };

        // Подмена проекта при тех же токенах: оба вырожденных проверяющих её ПРИНИМАЮТ.
        Assert.True(CountOnlyVerifierAccepts(samePairs.Length, samePairs.Length));
        Assert.True(ValueOnlyVerifierAccepts(samePairs, samePairs));

        // Проверяющий по происхождению её отвергает названным кодом.
        var foreign = new E43TrustedPair(ProjectB, FileOne, 0, 1, 2);
        var refused = E43PairProvenance.RequirePairBelongsToStream(streams, foreign);
        Assert.False(refused.IsSuccess);
        Assert.Equal(E43PairProvenance.UnknownStreamCode, refused.ErrorCode);

        // Он же принимает законную пару того же материала: отказ не огульный.
        var lawful = new E43TrustedPair(ProjectA, FileOne, 0, 1, 2);
        var accepted = E43PairProvenance.RequirePairBelongsToStream(streams, lawful);
        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
    }

    // ── Случай 8: правило отпечатка одно на два места ───────────────────────────

    [Fact]
    public void LocalStreamDigestAgreesWithTheSingleOwnerOfTheEncodingRule()
    {
        // Правило кодирования полей объявлено владельцем в проекте обучения способностей, но
        // объявлено внутренним, а ссылки на ту сборку у сборки опытов нет, поэтому в опытах
        // живёт вторая реализация того же правила. Эта проверка закрепляет их согласие: один и
        // тот же вход, посчитанный обоими способами, обязан дать один отпечаток. Если
        // реализации разойдутся, упадёт именно она.
        var tokens = new[] { 5, 6, 7 };

        var local = E43PairProvenance.ComputeStreamHash(ProjectA, FileOne, tokens);

        var owner = new AgentInfrastructure.Logical.UnambiguousFieldEncoding.Writer(
            E43PairProvenance.StreamDigestPurpose);
        owner.WriteField(ProjectA);              // Проект потока — третьим полем.
        owner.WriteField(FileOne);               // Файл потока — четвёртым полем.
        owner.WriteCount(tokens.Length);         // Число опознаний до самих опознаний.
        foreach (var token in tokens) owner.WriteCount(token); // Каждое опознание своей величиной.

        Assert.Equal(owner.Digest(), local);

        // И версия правила у обоих одна: расхождение версий тоже было бы расхождением правил.
        Assert.Equal(
            AgentInfrastructure.Logical.UnambiguousFieldEncoding.EncodingVersion,
            E43PairProvenance.StreamEncodingVersion);
    }

    // ── Оснастка ──────────────────────────────────────────────────────────────

    /// <summary>Собирает доверенные потоки и проверяет, что сборка каждого удалась.</summary>
    private static E43TrustedStream[] BuiltStreams(
        params (string ProjectId, string FilePath, int[] Tokens)[] declared)
    {
        var streams = new List<E43TrustedStream>();   // Готовые потоки для проверки.
        foreach (var (projectId, filePath, tokens) in declared)
        {
            var built = E43PairProvenance.BuildStream(projectId, filePath, tokens);
            Assert.True(built.IsSuccess, built.ErrorMessage); // Сборка обязана удаться.
            streams.Add(built.Value!);                        // Поток принят.
        }

        return [.. streams];
    }
}
