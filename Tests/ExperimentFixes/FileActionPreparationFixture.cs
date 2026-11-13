// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Оснастка набора данных для переноса «признак и метка урока используют один
// разбор» (строка очереди переносов 40, идентификатор работы FIX-KHC, карточка
// о едином разборе для признака и действия над файлом, раздел о фиксированных
// данных).
//
// Что она делает человеческими словами. Читает с диска шесть учебных строк,
// четыре экзаменационных строки с независимым правильным ответом и паспорт
// набора; сверяет отпечатки прочитанного с паспортом, чтобы проверка никогда не
// училась на подменённых данных. Ничего не вычисляет и ничего не обучает.
//
// Откуда взялись данные. Из сохранённого исходного наблюдения опыта
// `experiments-v2/RP01-MICRO/KHC/results/arm-A.json`, поля `inputs.train`,
// `inputs.exam` и `prepared`. Эти байты опубликованы и только читаются; набор
// данных — их выписка по адресам, назначенным карточкой.
//
// Чего она НЕ делает. Не вычисляет правильный ответ разметчиком продукта: он
// лежит в наборе рядом с экзаменационной строкой и пришёл из исходного
// наблюдения. Иначе проверка спрашивала бы у подсудимого его же приговор.
public static class FileActionPreparationFixture
{
    // Одна строка учебного или экзаменационного набора.
    public sealed record Row(string Id, string Source, string? Gold);

    // Одна историческая подготовленная строка: метка и вектор признаков.
    public sealed record HistoricalRow(string Id, int Label, string LabelName, float[] Features);

    // Прочитанный набор целиком плюс числа договора из паспорта.
    public sealed record Loaded(
        IReadOnlyList<Row> Train,
        IReadOnlyList<Row> Exam,
        IReadOnlyList<HistoricalRow> Historical,
        int ExpectedTrainingSizeAfterFit,
        int ExpectedCorrectAfterTraining,
        int ExpectedLoopFamilyCorrect,
        int ExpectedNoLoopFamilyCorrect,
        int ConstantBaseline,
        int FeatureVectorLength,
        int ParseSitesBeforeTransfer,
        string ManifestSha256);

    // Корень хранилища ищется по файлу правил проекта, вверх от каталога сборки.
    public static string RepositoryRoot()
    {
        // Поиск начинается от каталога, откуда запущена сборка проверок.
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        // Подъём вверх до каталога, где лежит файл правил агента.
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            directory = directory.Parent;

        // Без корня хранилища набор данных не найти.
        Assert.NotNull(directory);

        // Полный путь корня возвращается вызывающей стороне.
        return directory!.FullName;
    }

    // Каталог набора данных этой работы — ровно тот, который назначает карточка.
    public static string FixtureDirectory() => Path.Combine(
        RepositoryRoot(), "Tests", "Fixtures", "ExperimentFixes", "v1", "Khc01");

    // Отпечаток текста файла: тот же прибор, которым посчитан паспорт.
    private static string Sha256OfText(string text)
    {
        // Байты берутся в той же кодировке, в которой файл записан.
        var bytes = Encoding.UTF8.GetBytes(text);

        // Шестнадцатеричная запись без разделителей, в нижнем регистре.
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    // Читает файл набора и сверяет его отпечаток с тем, что обещает паспорт.
    private static JsonDocument ReadVerified(string directory, string name, string expectedSha256)
    {
        // Полный путь файла внутри каталога набора.
        var path = Path.Combine(directory, name);

        // Отсутствие файла — это конкретный несовместимый набор, а не повод
        // придумать другие данные.
        Assert.True(File.Exists(path), $"файл набора данных не найден: {path}");

        // Текст читается целиком: файлы набора маленькие.
        var text = File.ReadAllText(path);

        // Отпечаток прочитанного обязан совпасть с паспортом.
        Assert.Equal(expectedSha256, Sha256OfText(text));

        // Разобранный документ отдаётся вызывающей стороне.
        return JsonDocument.Parse(text);
    }

    // Загружает набор целиком: паспорт, учебные строки, экзаменационные строки.
    public static Loaded Load()
    {
        // Каталог набора данных этой работы.
        var directory = FixtureDirectory();

        // Паспорт читается первым: в нём лежат отпечатки остальных файлов.
        var manifestPath = Path.Combine(directory, "manifest.json");

        // Без паспорта сверять отпечатки нечем.
        Assert.True(File.Exists(manifestPath), $"паспорт набора не найден: {manifestPath}");

        // Текст паспорта нужен и для разбора, и для собственного отпечатка.
        var manifestText = File.ReadAllText(manifestPath);

        // Разобранный паспорт.
        using var manifest = JsonDocument.Parse(manifestText);

        // Узел с отпечатками файлов набора.
        var digests = manifest.RootElement.GetProperty("sha256");

        // Узел с числами ожидаемого договора.
        var contract = manifest.RootElement.GetProperty("expectedContract");

        // Учебные строки: только опознаватель и исходный текст, без меток.
        using var trainDoc = ReadVerified(directory, "train.json", digests.GetProperty("train.json").GetString()!);

        // Экзаменационные строки: исходный текст и независимый правильный ответ.
        using var examDoc = ReadVerified(directory, "exam.json", digests.GetProperty("exam.json").GetString()!);

        // Накопитель учебных строк в порядке файла.
        var train = new List<Row>();

        // Каждая учебная строка берётся как есть, порядок сохраняется.
        foreach (var row in trainDoc.RootElement.GetProperty("rows").EnumerateArray())
            train.Add(new Row(row.GetProperty("id").GetString()!, row.GetProperty("source").GetString()!, null));

        // Историческая подготовленная таблица: метки и векторы из наблюдения.
        using var histDoc = ReadVerified(
            directory, "historical-prepared.json", digests.GetProperty("historical-prepared.json").GetString()!);

        // Накопитель исторических строк в порядке файла.
        var historical = new List<HistoricalRow>();

        // Каждая историческая строка несёт свою метку и свой вектор признаков.
        foreach (var row in histDoc.RootElement.GetProperty("rows").EnumerateArray())
        {
            // Признаки читаются по одному и складываются в массив той же длины.
            var features = row.GetProperty("features").EnumerateArray()
                .Select(value => (float)value.GetDouble()).ToArray();

            // Строка складывается целиком, порядок файла сохраняется.
            historical.Add(new HistoricalRow(
                row.GetProperty("id").GetString()!,
                row.GetProperty("label").GetInt32(),
                row.GetProperty("labelName").GetString()!,
                features));
        }

        // Накопитель экзаменационных строк в порядке файла.
        var exam = new List<Row>();

        // Каждая экзаменационная строка несёт свой независимый правильный ответ.
        foreach (var row in examDoc.RootElement.GetProperty("rows").EnumerateArray())
            exam.Add(new Row(
                row.GetProperty("id").GetString()!,
                row.GetProperty("source").GetString()!,
                row.GetProperty("gold").GetString()!));

        // Числа договора читаются из паспорта, а не задаются в коде проверки.
        return new Loaded(
            train,
            exam,
            historical,
            contract.GetProperty("trainingSizeAfterFit").GetInt32(),
            contract.GetProperty("correctAfterTraining").GetInt32(),
            contract.GetProperty("loopFamilyCorrect").GetInt32(),
            contract.GetProperty("noLoopFamilyCorrect").GetInt32(),
            contract.GetProperty("constantAddLoopBaseline").GetInt32(),
            contract.GetProperty("featureVectorLength").GetInt32(),
            contract.GetProperty("parseSitesExpectedFromSourceBeforeTransfer").GetInt32(),
            Sha256OfText(manifestText));
    }

    // Читает обученную таблицу целиком: пары «признаки и метка» в порядке обучения.
    // Это чтение, а не подмена: ни одна строка здесь ничего не записывает. Нужно
    // оно потому, что сохранность пары «признаки и метка одного урока» — и есть
    // то свойство, которое перенос обязан не нарушить.
    public static IReadOnlyList<(float[] Features, int Label)> TrainedRows(object facade)
    {
        // Частное поле предсказателя действия над файлом внутри фасада.
        var field = facade.GetType().GetField("_fileActionKnn", BindingFlags.Instance | BindingFlags.NonPublic);

        // Отсутствие поля означает смену договора фасада.
        Assert.NotNull(field);

        // Сам предсказатель.
        var predictor = field!.GetValue(facade);

        // Без предсказателя читать нечего.
        Assert.NotNull(predictor);

        // Частное поле обученной таблицы внутри предсказателя.
        var trainField = predictor!.GetType().GetField("_train", BindingFlags.Instance | BindingFlags.NonPublic);

        // Отсутствие поля означает смену договора предсказателя.
        Assert.NotNull(trainField);

        // Таблица читается как перечисление, без приведения к точному типу кортежа.
        var table = (System.Collections.IEnumerable)trainField!.GetValue(predictor)!;

        // Накопитель прочитанных пар в порядке обучения.
        var rows = new List<(float[] Features, int Label)>();

        // Каждая пара разбирается по открытым полям кортежа.
        foreach (var item in table)
        {
            // Тип элемента таблицы: кортеж из массива признаков и метки.
            var itemType = item!.GetType();

            // Первое поле кортежа — массив признаков.
            var features = (float[])itemType.GetField("Item1")!.GetValue(item)!;

            // Второе поле кортежа — метка.
            var label = (int)itemType.GetField("Item2")!.GetValue(item)!;

            // Пара складывается без изменения.
            rows.Add((features, label));
        }

        // Прочитанная таблица отдаётся вызывающей стороне.
        return rows;
    }

    // Читает ТОЛЬКО размер обученной таблицы у частного предсказателя фасада.
    // Карточка разрешает такой доступ ровно для размера и запрещает подменять им
    // состояние: ни одна строка здесь ничего не записывает.
    public static int TrainedTableSize(object facade)
    {
        // Частное поле предсказателя действия над файлом внутри фасада.
        var field = facade.GetType().GetField("_fileActionKnn", BindingFlags.Instance | BindingFlags.NonPublic);

        // Отсутствие поля означает смену договора фасада, а не повод обойти его.
        Assert.NotNull(field);

        // Сам предсказатель.
        var predictor = field!.GetValue(facade);

        // Без предсказателя размер читать нечем.
        Assert.NotNull(predictor);

        // Публичное свойство размера обученной таблицы.
        var size = predictor!.GetType().GetProperty("TrainingSize");

        // Отсутствие свойства — тоже смена договора.
        Assert.NotNull(size);

        // Возвращается прочитанное число без изменения состояния.
        return (int)size!.GetValue(predictor)!;
    }
}
