// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

// Сверка НАСТОЯЩЕГО состояния памяти ближайших соседей с НЕЗАВИСИМО объявленным журналом
// ожидаемых записей.
//
// Назначение: план сто тридцать девять, раздел R117-06; аудит сто тридцать восемь, находки
// F09/F10 (ожидаемый журнал подменял наблюдение фактического состояния).
//
// ДЕФЕКТ, КОТОРЫЙ ЭТО ЧИНИТ. Прежняя приёмка довольствовалась двумя вещами: числом записей
// («девять») и свёрткой ожидаемого журнала. Оба показателя не доказывают ничего о памяти.
// Число девять совпадает у множества совершенно разных состояний: достаточно испортить один
// перевод, переставить две записи в одной ячейке, переименовать ключ или перенести запись из
// одной ячейки в другую — число останется девять. А свёртка ожидаемого журнала, сравниваемая
// сама с собой, равна себе ВСЕГДА: такая проверка не может упасть, и честной проверкой она не
// является. Если журнал ожидаемого строится тем же кодом, что и проверяемое состояние, их
// согласие — это согласие кода с самим собой.
//
// ЧТО СТАЛО. Журнал ожидаемых записей приходит сюда как ДАННЫЕ: тройки «ключ, источник,
// перевод», объявленные тем, кто учил. Сверка не вызывает ни хранилище памяти, ни разборщик
// текста, ни построитель ключей — иначе она проверяла бы код его же кодом. Сравнивается всё,
// что названо разделом плана: состав ключей, источник и перевод каждой записи, кратности и
// порядок внутри ячейки. Каждое расхождение имеет свой названный код отказа.
//
// НАЗВАННЫЕ ГРАНИЦЫ, которые эта сверка НЕ доказывает, и я не выдаю их за доказанные.
//
// Первое. Фактическое состояние памяти наблюдается через ПРОДУКТОВУЮ выгрузку состояния, а не
// чтением живых структур в памяти процесса. Поэтому сверка видит ту картину, которую отдаёт
// выгрузка. Если бы сама выгрузка теряла или переписывала записи, эта сверка такого не
// заметила бы: она сравнивает выгрузку с журналом, а не выгрузку с памятью. Это ненаблюдённая
// граница, а не закрытый вопрос.
//
// Второе. Сверка доказывает, что обучение ЗАПИСАЛО ожидаемое, и не доказывает, что чтение этим
// пользуется. Какой читатель ответил на запрос — ранняя точная карта или память ближайших
// соседей — проверяется отдельно, своим следом исполнения.
//
// Третье. Ключи в журнал приходят как объявленные данные. Правило, по которому разборщик текста
// строит ключ, этой сверкой не перепроверяется: сменись оно молча, журнал с прежними ключами
// покажет расхождение состава, но причину — смену правила — назовёт человек, а не сверка.
//
// Четвёртое. У контрольной точки памяти нет пространства имён направления перевода. Это
// известный отдельный пункт плана, и здесь он намеренно НЕ расширяется: сверка работает с тем
// составом записи, который есть сегодня, и не изобретает поля, которых в состоянии нет.
namespace TranslationDomain;

using AgentInfrastructure;          // Конверт результата операции этой сборки.

// Одна ожидаемая запись памяти: ключ ячейки, источник и перевод.
//
// Это объявленные ДАННЫЕ того, кто учил, а не вычисленное состояние. Отдельный тип нужен затем,
// чтобы журнал нельзя было случайно собрать из проверяемой контрольной точки: у неё свой тип
// записи, и подстановка одного вместо другого не скомпилируется.
public sealed record TranslationKnnExpectedWrite(
    string Signature,   // Ключ ячейки памяти, под которым ожидается запись.
    string Source,      // Текст источника этой записи.
    string Target);     // Ожидаемый перевод этой записи.

// Сверка состояния памяти с журналом ожидаемых записей.
public static class TranslationKnnStateAudit
{
    // Число записей разошлось: это первый, но НЕ единственный и не достаточный признак.
    public const string EntryCountMismatchCode = "KNN_AUDIT_ENTRY_COUNT_MISMATCH";

    // В памяти нет ячейки, которую объявил журнал.
    public const string MissingSignatureCode = "KNN_AUDIT_MISSING_SIGNATURE";

    // В памяти есть ячейка, которой журнал не объявлял.
    public const string UnexpectedSignatureCode = "KNN_AUDIT_UNEXPECTED_SIGNATURE";

    // Кратность записей внутри ячейки не та, что объявлена журналом.
    public const string MultiplicityMismatchCode = "KNN_AUDIT_MULTIPLICITY_MISMATCH";

    // Источник записи не тот, что объявлен журналом на этом месте ячейки.
    public const string SourceMismatchCode = "KNN_AUDIT_SOURCE_MISMATCH";

    // Перевод записи не тот, что объявлен журналом на этом месте ячейки.
    public const string TargetMismatchCode = "KNN_AUDIT_TARGET_MISMATCH";

    // Состав ячейки тот же, но порядок записей другой. Порядок памяти значим: запись, легшая
    // позже, несёт более сильный учебный сигнал, поэтому перестановка меняет смысл состояния.
    public const string OrderMismatchCode = "KNN_AUDIT_ORDER_MISMATCH";

    // Журнал пуст: сверять нечего, и молча называть это совпадением нельзя.
    public const string EmptyJournalCode = "KNN_AUDIT_EMPTY_JOURNAL";

    // RequireMemoryMatchesJournal сверяет фактическую выгрузку памяти с журналом ожидаемого.
    //
    // Порядок проверок выбран так, чтобы отказ называл САМУЮ ТОЧНУЮ причину, а не первую
    // попавшуюся: сначала состав ячеек, затем кратности внутри ячейки, затем сами записи по
    // местам. Число записей проверяется тоже, но как отдельный признак, а не как приёмка:
    // равенство числа ничего не доказывает, и именно на нём держался прежний дефект.
    public static OperationResult<bool> RequireMemoryMatchesJournal(
        TranslationKnnStoreCheckpoint actual,
        IReadOnlyList<TranslationKnnExpectedWrite> expectedJournal)
    {
        if (actual is null)
            // Отсутствующая выгрузка — отказ, а не исключение.
            return OperationResult<bool>.Fail(
                "Выгрузка памяти обязательна для сверки.", EntryCountMismatchCode);

        if (expectedJournal is null || expectedJournal.Count == 0)
            // Пустой журнал не может быть доказательством совпадения.
            return OperationResult<bool>.Fail(
                "Журнал ожидаемых записей пуст: сверять нечего.", EmptyJournalCode);

        var actualEntries = actual.Entries ?? [];        // Фактические записи выгрузки.

        // Фактические записи группируются по ключу с СОХРАНЕНИЕМ порядка внутри ячейки.
        var actualByKey = actualEntries
            .GroupBy(entry => entry.Signature, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(entry => (entry.Source, entry.Target)).ToList(),
                StringComparer.Ordinal);

        // Ожидаемые записи группируются тем же способом — по объявленному ключу и в объявленном
        // порядке. Никакого обращения к хранилищу памяти или к построителю ключей здесь нет.
        var expectedByKey = expectedJournal
            .GroupBy(write => write.Signature, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(write => (write.Source, write.Target)).ToList(),
                StringComparer.Ordinal);

        // Ячейка, объявленная журналом, но отсутствующая в памяти: урок не дошёл.
        foreach (var expectedKey in expectedByKey.Keys.OrderBy(key => key, StringComparer.Ordinal))
        {
            if (!actualByKey.ContainsKey(expectedKey))
                return OperationResult<bool>.Fail(
                    "В памяти нет объявленной журналом ячейки: " + expectedKey,
                    MissingSignatureCode);
        }

        // Ячейка, которой журнал не объявлял: в памяти лежит чужая или изобретённая запись.
        foreach (var actualKey in actualByKey.Keys.OrderBy(key => key, StringComparer.Ordinal))
        {
            if (!expectedByKey.ContainsKey(actualKey))
                return OperationResult<bool>.Fail(
                    "В памяти есть ячейка, которой журнал не объявлял: " + actualKey,
                    UnexpectedSignatureCode);
        }

        // Состав ячеек совпал: дальше сверяются кратности и сами записи по местам.
        foreach (var expectedKey in expectedByKey.Keys.OrderBy(key => key, StringComparer.Ordinal))
        {
            var expectedCell = expectedByKey[expectedKey]; // Объявленные записи этой ячейки.
            var actualCell = actualByKey[expectedKey];     // Фактические записи этой ячейки.

            if (expectedCell.Count != actualCell.Count)
                // Перенос записи между ячейками оставляет общее число прежним, а кратность — нет.
                return OperationResult<bool>.Fail(
                    "Кратность записей ячейки " + expectedKey + " не та: ожидалось " +
                    expectedCell.Count + ", в памяти " + actualCell.Count,
                    MultiplicityMismatchCode);

            for (var position = 0; position < expectedCell.Count; position++)
            {
                var expectedWrite = expectedCell[position]; // Ожидаемая запись на этом месте.
                var actualWrite = actualCell[position];     // Фактическая запись на этом месте.

                if (!string.Equals(expectedWrite.Source, actualWrite.Source, StringComparison.Ordinal))
                {
                    // Источник не тот: либо запись не от того урока, либо места перепутаны.
                    // Перестановка внутри ячейки называется своим кодом, а не общим.
                    var code = actualCell.Any(candidate =>
                        string.Equals(candidate.Source, expectedWrite.Source, StringComparison.Ordinal) &&
                        string.Equals(candidate.Target, expectedWrite.Target, StringComparison.Ordinal))
                        ? OrderMismatchCode   // Та же запись есть, но на другом месте — это порядок.
                        : SourceMismatchCode; // Такой записи в ячейке нет вовсе — это источник.

                    return OperationResult<bool>.Fail(
                        "Запись " + position + " ячейки " + expectedKey +
                        " имеет источник «" + actualWrite.Source + "» вместо «" +
                        expectedWrite.Source + "»",
                        code);
                }

                if (!string.Equals(expectedWrite.Target, actualWrite.Target, StringComparison.Ordinal))
                {
                    // Перевод не тот: ровно тот случай, который равенство числа записей пропускает.
                    var code = actualCell.Any(candidate =>
                        string.Equals(candidate.Source, expectedWrite.Source, StringComparison.Ordinal) &&
                        string.Equals(candidate.Target, expectedWrite.Target, StringComparison.Ordinal))
                        ? OrderMismatchCode   // Та же пара есть, но на другом месте — это порядок.
                        : TargetMismatchCode; // Такой пары в ячейке нет — испорчен перевод.

                    return OperationResult<bool>.Fail(
                        "Запись " + position + " ячейки " + expectedKey +
                        " имеет перевод «" + actualWrite.Target + "» вместо «" +
                        expectedWrite.Target + "»",
                        code);
                }
            }
        }

        // Число записей сверяется последним и только как дополнительный признак: к этому месту
        // уже доказано совпадение состава, кратностей и самих записей по местам, поэтому
        // расхождение здесь означало бы ошибку самой сверки, а не состояния.
        if (actualEntries.Count != expectedJournal.Count)
            return OperationResult<bool>.Fail(
                "Число записей памяти " + actualEntries.Count +
                " не совпало с числом записей журнала " + expectedJournal.Count,
                EntryCountMismatchCode);

        // Память содержит ровно объявленные записи, в объявленных ячейках, кратностях и порядке.
        return OperationResult<bool>.Ok(true);
    }
}
