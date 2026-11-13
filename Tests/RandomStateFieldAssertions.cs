// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Общий договор сравнения состояния случайного потока модели.
//
// Зачем он нужен. Снимок состояния — это запись с заголовком и списком записей
// по каждому установленному адаптеру. Текстовая запись снимка (ToString) ПЕЧАТАЕТ
// у списка только имя его типа, а не содержимое: `Entries =
// System.Collections.Generic.List`1[TransformerModel.LlmModelRandomStateEntry]`.
// Поэтому сравнение двух снимков по тексту видит заголовок и не видит НИЧЕГО
// внутри записей — ни позиции потока, ни зёрен, ни самого набора ключей. Это и
// есть «сравнение имени коллекции вместо состояния», которое закрывает пятая
// работа плана поздних переносов (раздел «Проверять состояние RNG, а не имя его
// коллекции», файл 102 плана ремонта от 29 октября).
//
// Что он сравнивает. Заголовок снимка: версию записи, устройство потока, отпечаток
// топологии, отпечаток настройки адаптеров, шаг и число записей. Каждую запись:
// область потока, зерно начальной установки, зерно отсева и израсходованную
// позицию потока, а сверх того — всю запись целиком, чтобы поле, добавленное в
// запись позже, не прошло мимо договора молча.
//
// Политика тождества записи. Запись опознаётся по ключу адаптера
// (`AdapterKey`, сравнение StringComparer.Ordinal), а не по своему месту в
// массиве и не по ссылке на массив. Соответствие обязано быть взаимно
// однозначным: повтор ключа внутри любой из сторон — сам по себе расхождение,
// потому что тождество записи в этом случае неоднозначно; отсутствующий и лишний
// ключ названы отдельно. Порядок записей сравнением не управляет: два снимка с
// одним набором ключей в допустимом порядке считаются равными, если равны все
// поля каждой пары.
//
// Чего он НЕ доказывает. Он ничего не обучает, не читает с диска и не заявляет,
// что расходуемая случайная траектория двух прогонов совпала: равенство позиций
// потока при нулевом отсеве — это равенство состояния, а не доказательство
// одинакового расхода случайности.

namespace LlmFromScratch.Tests;

using TransformerModel;

/// <summary>Сравнение снимков состояния случайного потока по полям, а не по тексту.</summary>
internal static class RandomStateFieldAssertions
{
    /// <summary>
    /// Полный список расхождений между двумя снимками, по одной человеческой
    /// строке на каждое. Пустой список означает равенство по всему договору.
    /// Чистая функция без утверждений: ею пользуется и приёмка самого помощника.
    /// </summary>
    public static IReadOnlyList<string> Differences(
        LlmModelRandomStateSnapshot expected,
        LlmModelRandomStateSnapshot actual)
    {
        // Накопитель расхождений: собираются все сразу, а не только первое.
        var differences = new List<string>();

        // Заголовок снимка. Версия записи отвечает за то, что обе стороны вообще
        // описаны одним и тем же устройством полей.
        if (expected.FormatVersion != actual.FormatVersion)
            differences.Add(
                $"версия записи: ожидалась {expected.FormatVersion}, получена {actual.FormatVersion}");
        // Устройство потока — имя самого генератора случайных чисел.
        if (!string.Equals(expected.Algorithm, actual.Algorithm, StringComparison.Ordinal))
            differences.Add(
                $"устройство потока: ожидалось «{expected.Algorithm}», получено «{actual.Algorithm}»");
        // Отпечаток топологии привязывает снимок к той же форме модели.
        if (!string.Equals(expected.TopologySha256, actual.TopologySha256, StringComparison.Ordinal))
            differences.Add(
                $"отпечаток топологии: ожидался «{expected.TopologySha256}», получен «{actual.TopologySha256}»");
        // Отпечаток настройки адаптеров привязывает снимок к той же настройке дообучения.
        if (!string.Equals(expected.LoraConfigurationSha256, actual.LoraConfigurationSha256, StringComparison.Ordinal))
            differences.Add(
                $"отпечаток настройки адаптеров: ожидался «{expected.LoraConfigurationSha256}»,"
                + $" получен «{actual.LoraConfigurationSha256}»");
        // Шаг обучения, на котором снят снимок.
        if (expected.RandomStateStep != actual.RandomStateStep)
            differences.Add(
                $"шаг снимка: ожидался {expected.RandomStateStep}, получен {actual.RandomStateStep}");
        // Мощность набора записей: добавленная или убранная запись видна сразу здесь.
        if (expected.Entries.Count != actual.Entries.Count)
            differences.Add(
                $"число записей: ожидалось {expected.Entries.Count}, получено {actual.Entries.Count}");

        // Неоднозначное тождество проверяется до сопоставления: если ключ внутри
        // одной стороны повторяется, сопоставлять по ключу уже нельзя честно.
        differences.AddRange(DuplicateKeyDifferences("ожидаемом", expected.Entries));
        differences.AddRange(DuplicateKeyDifferences("полученном", actual.Entries));

        // Указатель от ключа адаптера к записи полученной стороны. Повтор ключа
        // уже назван расхождением выше, поэтому здесь берётся последняя запись.
        var actualByKey = new Dictionary<string, LlmModelRandomStateEntry>(StringComparer.Ordinal);
        foreach (var entry in actual.Entries)
            actualByKey[entry.AdapterKey] = entry;

        // Набор ключей ожидаемой стороны: по нему потом находятся лишние ключи.
        var expectedKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var expectedEntry in expected.Entries)
        {
            // Ключ запоминается независимо от того, нашлась ли пара.
            expectedKeys.Add(expectedEntry.AdapterKey);
            // Записи с таким ключом в полученном снимке может не быть вовсе.
            if (!actualByKey.TryGetValue(expectedEntry.AdapterKey, out var actualEntry))
            {
                differences.Add(
                    $"запись ключа адаптера «{expectedEntry.AdapterKey}» отсутствует в полученном состоянии");
                continue;
            }

            // Область потока: к какому именно потоку случайности привязан адаптер.
            if (!string.Equals(expectedEntry.StreamDomain, actualEntry.StreamDomain, StringComparison.Ordinal))
                differences.Add(
                    $"ключ адаптера «{expectedEntry.AdapterKey}», область потока:"
                    + $" ожидалась «{expectedEntry.StreamDomain}», получена «{actualEntry.StreamDomain}»");
            // Зерно начальной установки адаптера.
            if (expectedEntry.InitializationSeed != actualEntry.InitializationSeed)
                differences.Add(
                    $"ключ адаптера «{expectedEntry.AdapterKey}», зерно начальной установки:"
                    + $" ожидалось {expectedEntry.InitializationSeed}, получено {actualEntry.InitializationSeed}");
            // Зерно отсева: отдельный поток случайности того же адаптера.
            if (expectedEntry.DropoutSeed != actualEntry.DropoutSeed)
                differences.Add(
                    $"ключ адаптера «{expectedEntry.AdapterKey}», зерно отсева:"
                    + $" ожидалось {expectedEntry.DropoutSeed}, получено {actualEntry.DropoutSeed}");
            // Израсходованная позиция потока: сколько случайности уже взято.
            if (expectedEntry.ConsumedCursor != actualEntry.ConsumedCursor)
                differences.Add(
                    $"ключ адаптера «{expectedEntry.AdapterKey}», израсходованная позиция потока:"
                    + $" ожидалась {expectedEntry.ConsumedCursor}, получена {actualEntry.ConsumedCursor}");
            // Страховка договора: если поименованные выше поля совпали, а записи
            // целиком — нет, значит в записи появилось поле вне этого договора.
            if (!expectedEntry.Equals(actualEntry))
                differences.Add(
                    $"запись ключа адаптера «{expectedEntry.AdapterKey}» различается полем,"
                    + " не названным в договоре сравнения");
        }

        // Лишние ключи полученной стороны называются отдельно и поимённо.
        foreach (var actualEntry in actual.Entries)
            if (!expectedKeys.Contains(actualEntry.AdapterKey))
                differences.Add(
                    $"в полученном состоянии появилась лишняя запись ключа адаптера «{actualEntry.AdapterKey}»");

        return differences;
    }

    /// <summary>
    /// Утверждение равенства снимков по полям. Все расхождения показываются
    /// одним сообщением, чтобы красный прогон сразу называл причину целиком.
    /// </summary>
    public static void AssertEqual(
        LlmModelRandomStateSnapshot expected,
        LlmModelRandomStateSnapshot actual)
    {
        // Сначала считаются все расхождения, и только потом принимается решение.
        var differences = Differences(expected, actual);
        // Пустой список расхождений — это и есть равенство по договору.
        if (differences.Count == 0)
            return;
        // Непустой список целиком попадает в сообщение об отказе.
        Assert.Fail(
            "Состояние случайного потока различается по полям: "
            + string.Join("; ", differences));
    }

    /// <summary>Повторяющиеся ключи адаптера внутри одной стороны сравнения.</summary>
    private static IEnumerable<string> DuplicateKeyDifferences(
        string side,
        IReadOnlyList<LlmModelRandomStateEntry> entries)
        // Группировка по ключу адаптера: группа длиннее одной записи — повтор.
        => entries
            .GroupBy(entry => entry.AdapterKey, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group =>
                $"в {side} состоянии ключ адаптера «{group.Key}» встречается {group.Count()} раз:"
                + " тождество записи неоднозначно");
}
