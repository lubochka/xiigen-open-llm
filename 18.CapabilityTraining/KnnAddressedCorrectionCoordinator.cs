// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Globalization;
using System.Text;
using AgentInfrastructure;

// Адресная доставка исправляющего урока выбранному владельцу памяти.
//
// Зачем это нужно, по-человечески. Когда пользователь исправил ответ, исправляющий урок обязан
// дойти ровно до той обученной единицы, которая этот ответ дала. Раньше такого звена не было:
// исправление уходило «куда-нибудь», и при двух владельцах с одинаковым текстом запроса правильный
// адресат угадывался по порядку регистрации. В опыте это дало ровно половину правильных адресов
// (1 из 2) и одно изменение ЧУЖОЙ памяти. Это звено берёт опознание владельца из фактического
// ответа и доводит урок до него, не задев соседей.
//
// Чем это ЯВЛЯЕТСЯ по уровню устройства. Это ДЕТЕРМИНИРОВАННАЯ СВЯЗКА (deterministic plumbing)
// внутри уже разрешённого пути единиц: она сверяет опознания и передаёт материал. Обучением
// занимается по-прежнему родной обучающий блок за границей IAddressedCorrectionWriter.
//
// Чем это НЕ является, названо прямо:
//   * это НЕ обучаемое звено и НЕ обученный выбор получателя: связка ничего не выучивает, ни одного
//     веса, ни одного счётчика предпочтения; получателя она НЕ выбирает, а ПРОВЕРЯЕТ по опознанию
//     фактического ответа. Маршрут, который этого получателя выбрал, может быть обученным — но он
//     принадлежит другому владельцу и в этой связке отсутствует;
//   * это НЕ общий допуск доменных функций ранжирования: ни один каталог принципов здесь не
//     создаётся, не включается и не читается, и прежний путь без адресной доставки остаётся
//     прежним побайтово — ни одна строка обученной единицы этим файлом не изменена;
//   * это НЕ доказательство побайтовой сохранности состояния: опознание состояния связка только
//     ПЕРЕНОСИТ и СРАВНИВАЕТ, но не вычисляет. Нового общедоступного отпечатка состояния здесь
//     намеренно нет: полный носитель состояния принадлежит своему владельцу и остаётся открытым;
//   * это НЕ восстановление атомарности «публикация + отметка»: связка только гарантирует, что
//     отметка не появится раньше успешной публикации. Окно «публикация прошла, отметка не
//     записана» возвращается отказом и остаётся открытой частью владельца жизненного цикла.

// Опознание выбранного владельца: связка домена, модели и её версии.
//
// Каждое поле названо затем, чтобы урок нельзя было доставить «по имени вообще». Домен говорит, о
// какой предметной области урок; профиль — какой каталог правил ранжирования разрешён этому
// владельцу; опознание единицы и её роль — какая именно модель отвечала; версия и порядковый номер
// регистрации — какое именно состояние этой модели отвечало. Опознание состояния приходит снаружи
// как непрозрачная строка: связка её сверяет, но не считает.
public sealed record AddressedCorrectionOwnerIdentity(
    string DomainTag,
    string Profile,
    string UnitId,
    string UnitRole,
    string Version,
    long RegistrationOrder,
    string TrainedStateIdentity);

// Опознание фактического ответа, к которому относится исправление.
//
// Это и есть адрес урока. Он взят из уже выданного ответа, а не из текста запроса: одинаковый текст
// запроса у двух владельцев — обычное дело, и адресом он быть не может.
public sealed record AddressedCorrectionResponseEvidence(
    string RunId,
    string AnsweredUnitId,
    string AnsweredDomainTag,
    string AnsweredVersion,
    long AnsweredRegistrationOrder,
    string AnsweredStateIdentity);

// Запрос адресной доставки одного исправляющего урока.
//
// Запуск и обращение вместе с опознанием владельца образуют составной ключ доставки: по нему
// повторный тот же урок узнаётся как уже применённый.
public sealed record AddressedCorrection<TOutput>(
    string RunId,
    string FeedbackId,
    AddressedCorrectionOwnerIdentity Owner,
    AddressedCorrectionResponseEvidence ResponseEvidence,
    string CorrectedExampleId,
    TOutput CorrectedAnswer);

// Обученный, но ещё НЕ опубликованный исправленный владелец.
//
// Он существует отдельным звеном затем, чтобы дорогое обучение не повторялось при неудачной
// публикации: уже оплаченный кандидат остаётся на руках у вызывающего вместе с отказом.
public sealed record AddressedCorrectionCandidate<TOutput>(
    AddressedCorrectionOwnerIdentity Owner,
    ILogicalUnit<string, TOutput> CorrectedUnit,
    string NextVersion);

// Квитанция состоявшейся адресной доставки.
//
// Она долговечна: именно её наличие на диске означает «этот урок уже применён». Поля до и после
// публикации названы раздельно, поэтому по квитанции видно, какая версия отвечала и какая версия
// была опубликована.
public sealed record AddressedCorrectionReceipt(
    string DeliveryKey,
    string RunId,
    string FeedbackId,
    string DomainTag,
    string Profile,
    string UnitId,
    string UnitRole,
    string SelectedVersion,
    long SelectedRegistrationOrder,
    string PublishedVersion,
    long PublishedRegistrationOrder,
    string CorrectedExampleId)
{
    // Признак «эта квитанция прочитана с диска, а не создана сейчас». В файл он НЕ пишется: на
    // диске лежит факт применения, а не сведение о том, кто его перечитал.
    public bool ReusedCompletedDelivery { get; init; }
}

// Результат поиска долговечной отметки применения.
//
// Отсутствие отметки — это НЕ отказ: это обычный случай первого применения урока. Поэтому оно
// возвращается успехом с признаком Found=false, а не ошибкой. Квитанция заполнена тогда и только
// тогда, когда Found=true.
public sealed record AddressedCorrectionDeliveryLookup(
    bool Found,
    AddressedCorrectionReceipt? Receipt);

// Граница обучения и публикации. Правило 2 проекта: внешняя зависимость приходит через интерфейс.
//
// Связка не знает, какой именно родной обучающий блок стоит за этой границей, и не умеет обучать
// сама. Два раздельных вызова нужны ради слова «ПОСЛЕ»: между оплаченным обучением и публикацией
// есть точка, в которой публикация может не состояться, и тогда отметки применения быть не должно.
public interface IAddressedCorrectionWriter<TOutput>
{
    // Обучить исправленного кандидата ИМЕННО на той единице, которая отвечала.
    //
    // Выбранная единица приходит сюда объектом, а не именем: это и есть доказательство адресности —
    // фактически вызванный член получает фактический объект владельца, а не запись о нём.
    OperationResult<AddressedCorrectionCandidate<TOutput>> TrainCorrectedOwner(
        ILogicalUnit<string, TOutput> selectedUnit,
        AddressedCorrectionOwnerIdentity owner,
        string correctedExampleId,
        TOutput correctedAnswer,
        CancellationToken cancellationToken);

    // Опубликовать ТОЛЬКО выбранного владельца новой версией и вернуть её опознание.
    OperationResult<AddressedCorrectionOwnerIdentity> PublishCorrectedOwner(
        AddressedCorrectionCandidate<TOutput> candidate,
        CancellationToken cancellationToken);
}

// Справочник владельцев. Правило 2 проекта: связка берёт владельца через интерфейс.
public interface IAddressedCorrectionOwnerDirectory<TOutput>
{
    // Вернуть ЖИВОЕ опознание и исполняемый объект последней версии владельца по его опознанию.
    OperationResult<ResolvedLogicalUnitRegistryEntry<string, TOutput>> ResolveLatestOwnerById(string unitId);
}

// Переходник от продуктового реестра единиц к справочнику владельцев.
//
// Конкретный реестр назван ровно в одном месте — здесь, в точке сборки. Сама связка остаётся на
// интерфейсе, поэтому её можно проверить и на реестре продукта, и на подставном справочнике.
public sealed class LogicalUnitRegistryAddressedCorrectionDirectory<TOutput>
    : IAddressedCorrectionOwnerDirectory<TOutput>
{
    // Продуктовый реестр единиц. Он один и тот же для чтения владельца и для его публикации.
    private readonly InMemoryLogicalUnitRegistry _registry;

    // Переходник принимает реестр и больше ничего: своего состояния у него нет.
    public LogicalUnitRegistryAddressedCorrectionDirectory(InMemoryLogicalUnitRegistry registry)
    {
        // Реестр обязателен: без него справочник не смог бы назвать ни одного владельца.
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    // Чтение владельца идёт тем же продуктовым путём, что и обычное чтение реестра: последняя
    // версия по порядковому номеру регистрации, без собственной трактовки «новизны».
    public OperationResult<ResolvedLogicalUnitRegistryEntry<string, TOutput>> ResolveLatestOwnerById(string unitId)
        => _registry.ResolveLatestById<string, TOutput>(unitId);
}

// Долговечный журнал состоявшихся адресных доставок.
//
// Зачем он на диске. Признак «этот урок уже применён» обязан переживать перезапуск: отметка в
// памяти процесса исчезает вместе с процессом, и после восстановления состояния с диска тот же
// урок применился бы второй раз. Поэтому отметка — файл.
//
// Главное правило этого журнала: отметка появляется ТОЛЬКО ПОСЛЕ успешной публикации. Исходный
// опытный образец заполнял набор доставок ещё на решении, ДО обучения, и потому при неудаче считал
// урок доставленным. Такое поведение сюда намеренно НЕ перенесено.
internal static class AddressedCorrectionDeliveryJournal
{
    // Имя вида записи. Первой строкой файла: чужой вид дальше не разбирается.
    internal const string FormatVersion = "knn-addressed-correction-delivery-v1";

    // Коды отказов журнала. Каждый называет ровно одну причину.
    internal const string WriteFailedErrorCode = "ADDRESSED_CORRECTION_JOURNAL_WRITE_FAILED";
    internal const string ReadFailedErrorCode = "ADDRESSED_CORRECTION_JOURNAL_READ_FAILED";
    internal const string MalformedErrorCode = "ADDRESSED_CORRECTION_JOURNAL_MALFORMED";
    internal const string DigestMismatchErrorCode = "ADDRESSED_CORRECTION_JOURNAL_DIGEST_MISMATCH";

    // Метка отсутствующего значения в файле: «поля нет» никогда не совпадает с «поле пустое».
    private const string AbsentValueToken = "~absent";

    // Составной ключ доставки. Поля кодируются ОДНОЗНАЧНО: длина поля объявляется до его байтов.
    //
    // Почему не склейка через разделитель. При склейке «запуск|обращение|владелец» тот же самый
    // разделитель внутри любого поля сдвигает границы полей, и две РАЗНЫЕ доставки дают один ключ:
    // тогда второй, ещё не применённый урок молча считался бы уже применённым. Здесь границу искать
    // не нужно — читатель берёт ровно столько байтов, сколько объявлено.
    //
    // SHA-256 (Secure Hash Algorithm 256 — алгоритм свёртки на 256 бит) берётся из единственного
    // владельца этого правила в сборке, а не считается здесь заново.
    internal static string ComposeDeliveryKey(
        string runId,
        string feedbackId,
        AddressedCorrectionOwnerIdentity owner)
    {
        // Назначение потока называет именно адресную доставку: тот же набор значений в другом
        // по смыслу потоке даст другой ключ.
        var writer = new UnambiguousFieldEncoding.Writer("knn-addressed-correction-delivery-key");
        // Запуск и обращение пользователя образуют первую половину ключа.
        writer.WriteField(runId)
              .WriteField(feedbackId)
              // Вторая половина — опознание выбранного владельца.
              .WriteField(owner.DomainTag)
              .WriteField(owner.Profile)
              .WriteField(owner.UnitId)
              .WriteField(owner.UnitRole);
        // Версия и порядковый номер в ключ НЕ входят намеренно: успешная доставка сама делает
        // прежнюю версию старой, и повторное то же обращение обязано узнаваться после неё.
        return writer.Digest();
    }

    // ResolvePath называет точный файл отметки. Ключ уже шестнадцатеричный, поэтому он безопасен
    // как имя файла и не может создать подкаталог.
    internal static string ResolvePath(string journalRoot, string deliveryKey)
        => Path.Combine(journalRoot, $"{deliveryKey}.delivery");

    // MarkApplied записывает отметку применения и сразу читает её обратно.
    //
    // Вызывать его разрешено ТОЛЬКО после успешной публикации: это и есть слово «ПОСЛЕ».
    internal static OperationResult<string> MarkApplied(
        string journalRoot,
        AddressedCorrectionReceipt receipt)
    {
        // Без квитанции писать нечего: это отсутствие входа, а не отказ доставки.
        if (receipt is null)
            return OperationResult<string>.Fail(
                "Квитанция адресной доставки обязательна для записи.",
                WriteFailedErrorCode);

        // Путь считается до любой записи, чтобы отказ называл точный файл.
        var path = ResolvePath(journalRoot, receipt.DeliveryKey);
        try
        {
            // Каталог журнала создаётся при первой записи и далее переиспользуется.
            Directory.CreateDirectory(journalRoot);
            // Содержимое собирается целиком в памяти: частично записанного файла не остаётся.
            File.WriteAllText(path, Render(receipt), new UTF8Encoding(false));
        }
        catch (Exception exception) when (exception is IOException or
            UnauthorizedAccessException or NotSupportedException or ArgumentException)
        {
            // Причина отказа называет точный путь и слова самой ошибки файловой системы.
            return OperationResult<string>.Fail(
                $"Отметку адресной доставки не удалось записать в '{path}': {exception.Message}",
                WriteFailedErrorCode);
        }

        // Обратное чтение сразу после записи: записанное обязано читаться.
        var readBack = TryLoadApplied(journalRoot, receipt.DeliveryKey);
        // Отказ обратного чтения передаётся как есть, вместе с его кодом.
        if (!readBack.IsSuccess)
            return readBack.MapError<string>();

        // Записанная отметка обязана найтись: иначе запись не доказана.
        if (!readBack.Value!.Found)
            return OperationResult<string>.Fail(
                $"Отметка адресной доставки в '{path}' после записи не нашлась. Запись не доказана.",
                WriteFailedErrorCode);

        // И прочитанное обязано совпасть с тем, что записывали, по ВСЕМ полям.
        if (!ReceiptsMatch(receipt, readBack.Value.Receipt!))
            return OperationResult<string>.Fail(
                $"Отметка адресной доставки в '{path}' прочиталась не теми значениями, которые " +
                "записывались. Запись не доказана.",
                WriteFailedErrorCode);

        // Путь сохранённого файла возвращается вызывающему как свидетельство записи.
        return OperationResult<string>.Ok(path);
    }

    // TryLoadApplied читает отметку обратно и сверяет отпечаток её полей.
    //
    // Отсутствие файла — обычный случай первого применения урока, поэтому это успех с Found=false.
    internal static OperationResult<AddressedCorrectionDeliveryLookup> TryLoadApplied(
        string journalRoot,
        string deliveryKey)
    {
        // Путь называется до чтения, чтобы любой отказ называл точный файл.
        var path = ResolvePath(journalRoot, deliveryKey);
        string text;
        try
        {
            // Отметки нет — урок ещё применим. Это не ошибка.
            if (!File.Exists(path))
                return OperationResult<AddressedCorrectionDeliveryLookup>.Ok(
                    new AddressedCorrectionDeliveryLookup(false, null));

            // Файл читается целиком: отметка небольшая и читается одним куском.
            text = File.ReadAllText(path, Encoding.UTF8);
        }
        catch (Exception exception) when (exception is IOException or
            UnauthorizedAccessException or NotSupportedException or ArgumentException)
        {
            // Нечитаемая отметка — именно отказ: молча считать урок применимым здесь нельзя.
            return OperationResult<AddressedCorrectionDeliveryLookup>.Fail(
                $"Отметку адресной доставки из '{path}' не удалось прочитать: {exception.Message}",
                ReadFailedErrorCode);
        }

        // Строки разбираются по порядку записи: вид записи задаёт порядок однозначно.
        var lines = text.Split('\n').Select(line => line.TrimEnd('\r')).ToArray();
        var cursor = 0;
        try
        {
            // Первая строка называет вид записи: чужой вид дальше не разбирается.
            var format = NextValue(lines, ref cursor, "format");
            if (!string.Equals(format, FormatVersion, StringComparison.Ordinal))
                return OperationResult<AddressedCorrectionDeliveryLookup>.Fail(
                    $"Файл '{path}' записан видом '{format}', а не '{FormatVersion}'.",
                    MalformedErrorCode);

            // Поля читаются ровно в том порядке, в котором их записал Render.
            var storedKey = NextValue(lines, ref cursor, "deliveryKey") ?? string.Empty;
            var runId = NextValue(lines, ref cursor, "runId") ?? string.Empty;
            var feedbackId = NextValue(lines, ref cursor, "feedbackId") ?? string.Empty;
            var domainTag = NextValue(lines, ref cursor, "domainTag") ?? string.Empty;
            var profile = NextValue(lines, ref cursor, "profile") ?? string.Empty;
            var unitId = NextValue(lines, ref cursor, "unitId") ?? string.Empty;
            var unitRole = NextValue(lines, ref cursor, "unitRole") ?? string.Empty;
            var selectedVersion = NextValue(lines, ref cursor, "selectedVersion") ?? string.Empty;
            var selectedOrder = NextLong(lines, ref cursor, "selectedRegistrationOrder");
            var publishedVersion = NextValue(lines, ref cursor, "publishedVersion") ?? string.Empty;
            var publishedOrder = NextLong(lines, ref cursor, "publishedRegistrationOrder");
            var correctedExampleId = NextValue(lines, ref cursor, "correctedExampleId") ?? string.Empty;
            // Последним полем лежит отпечаток самих полей.
            var storedDigest = NextValue(lines, ref cursor, "fieldsDigest") ?? string.Empty;

            // Квитанция собирается из прочитанных значений.
            var restored = new AddressedCorrectionReceipt(
                storedKey,
                runId,
                feedbackId,
                domainTag,
                profile,
                unitId,
                unitRole,
                selectedVersion,
                selectedOrder,
                publishedVersion,
                publishedOrder,
                correctedExampleId);

            // Отпечаток пересчитывается из ПРОЧИТАННЫХ полей и сверяется с записанным.
            var recomputed = ComputeFieldsDigest(restored);
            if (!string.Equals(recomputed, storedDigest, StringComparison.Ordinal))
                return OperationResult<AddressedCorrectionDeliveryLookup>.Fail(
                    $"Отпечаток полей отметки в '{path}' не совпал: в файле '{storedDigest}', " +
                    $"посчитанный из прочитанных полей '{recomputed}'. Файл изменён или записан " +
                    "не полностью.",
                    DigestMismatchErrorCode);

            // Ключ внутри файла обязан совпасть с ключом, по которому файл искали: иначе отметка
            // чужой доставки выдала бы себя за эту.
            if (!string.Equals(storedKey, deliveryKey, StringComparison.Ordinal))
                return OperationResult<AddressedCorrectionDeliveryLookup>.Fail(
                    $"Отметка в '{path}' записана ключом '{storedKey}', а искалась по '{deliveryKey}'.",
                    MalformedErrorCode);

            // Отметка прочитана и сверена: урок уже применён.
            return OperationResult<AddressedCorrectionDeliveryLookup>.Ok(
                new AddressedCorrectionDeliveryLookup(true, restored));
        }
        catch (FormatException exception)
        {
            // Непригодная строка называется человеческой причиной, а не исключением наружу.
            return OperationResult<AddressedCorrectionDeliveryLookup>.Fail(
                $"Файл отметки адресной доставки '{path}' разобрать не удалось: {exception.Message}",
                MalformedErrorCode);
        }
    }

    // Render собирает содержимое файла. Порядок строк задан здесь и только здесь.
    private static string Render(AddressedCorrectionReceipt receipt)
    {
        // Накопитель строк файла.
        var builder = new StringBuilder();
        // Вид записи идёт первой строкой.
        AppendValue(builder, "format", FormatVersion);
        // Дальше поля квитанции в порядке её объявления.
        AppendValue(builder, "deliveryKey", receipt.DeliveryKey);
        AppendValue(builder, "runId", receipt.RunId);
        AppendValue(builder, "feedbackId", receipt.FeedbackId);
        AppendValue(builder, "domainTag", receipt.DomainTag);
        AppendValue(builder, "profile", receipt.Profile);
        AppendValue(builder, "unitId", receipt.UnitId);
        AppendValue(builder, "unitRole", receipt.UnitRole);
        AppendValue(builder, "selectedVersion", receipt.SelectedVersion);
        AppendLong(builder, "selectedRegistrationOrder", receipt.SelectedRegistrationOrder);
        AppendValue(builder, "publishedVersion", receipt.PublishedVersion);
        AppendLong(builder, "publishedRegistrationOrder", receipt.PublishedRegistrationOrder);
        AppendValue(builder, "correctedExampleId", receipt.CorrectedExampleId);
        // Отпечаток полей идёт последней строкой: он считается по уже известным значениям.
        AppendValue(builder, "fieldsDigest", ComputeFieldsDigest(receipt));
        // Готовое содержимое возвращается одной строкой файла.
        return builder.ToString();
    }

    // ComputeFieldsDigest считает отпечаток полей тем же единственным владельцем правила
    // однозначного кодирования: длина поля перед его байтами.
    private static string ComputeFieldsDigest(AddressedCorrectionReceipt receipt)
    {
        // Назначение потока называет именно отметку доставки, а не её ключ: совпадающий набор
        // значений в потоке ключа даст другой отпечаток.
        var writer = new UnambiguousFieldEncoding.Writer("knn-addressed-correction-delivery-receipt");
        // Поля идут в порядке объявления квитанции.
        writer.WriteField(receipt.DeliveryKey)
              .WriteField(receipt.RunId)
              .WriteField(receipt.FeedbackId)
              .WriteField(receipt.DomainTag)
              .WriteField(receipt.Profile)
              .WriteField(receipt.UnitId)
              .WriteField(receipt.UnitRole)
              .WriteField(receipt.SelectedVersion)
              // Порядковые номера входят своим текстовым неизменным видом: счётное поле потока
              // хранит четыре байта, а номер регистрации длиннее четырёх байтов.
              .WriteField(receipt.SelectedRegistrationOrder.ToString(CultureInfo.InvariantCulture))
              .WriteField(receipt.PublishedVersion)
              .WriteField(receipt.PublishedRegistrationOrder.ToString(CultureInfo.InvariantCulture))
              .WriteField(receipt.CorrectedExampleId);
        // Отпечаток считается один раз по всему потоку.
        return writer.Digest();
    }

    // ReceiptsMatch сверяет две квитанции по всем полям. Запись сравнивается с прочитанным именно
    // так, а не по отпечатку: отпечаток считается из тех же полей и сам по себе круговой довод.
    private static bool ReceiptsMatch(
        AddressedCorrectionReceipt written,
        AddressedCorrectionReceipt read)
        => string.Equals(written.DeliveryKey, read.DeliveryKey, StringComparison.Ordinal) &&
           string.Equals(written.RunId, read.RunId, StringComparison.Ordinal) &&
           string.Equals(written.FeedbackId, read.FeedbackId, StringComparison.Ordinal) &&
           string.Equals(written.DomainTag, read.DomainTag, StringComparison.Ordinal) &&
           string.Equals(written.Profile, read.Profile, StringComparison.Ordinal) &&
           string.Equals(written.UnitId, read.UnitId, StringComparison.Ordinal) &&
           string.Equals(written.UnitRole, read.UnitRole, StringComparison.Ordinal) &&
           string.Equals(written.SelectedVersion, read.SelectedVersion, StringComparison.Ordinal) &&
           written.SelectedRegistrationOrder == read.SelectedRegistrationOrder &&
           string.Equals(written.PublishedVersion, read.PublishedVersion, StringComparison.Ordinal) &&
           written.PublishedRegistrationOrder == read.PublishedRegistrationOrder &&
           string.Equals(written.CorrectedExampleId, read.CorrectedExampleId, StringComparison.Ordinal);

    // AppendValue пишет одну строку «имя, знак табуляции, закодированное значение».
    private static void AppendValue(StringBuilder builder, string name, string? value)
        // Значение кодируется так, чтобы перевод строки внутри него не создал новую строку файла.
        => builder.Append(name).Append('\t').Append(Encode(value)).Append('\n');

    // AppendLong пишет длинное целое неизменным видом, не зависящим от языка машины.
    private static void AppendLong(StringBuilder builder, string name, long value)
        => AppendValue(builder, name, value.ToString(CultureInfo.InvariantCulture));

    // NextValue забирает очередную строку, проверяет её имя и возвращает раскодированное значение.
    private static string? NextValue(string[] lines, ref int cursor, string expectedName)
    {
        // Кончившиеся строки означают неполный файл.
        if (cursor >= lines.Length)
            throw new FormatException($"поле '{expectedName}' отсутствует: файл кончился");

        // Строка делится ровно по первому знаку табуляции: в значении он уже закодирован.
        var line = lines[cursor++];
        var separator = line.IndexOf('\t');
        if (separator < 0)
            throw new FormatException($"строка поля '{expectedName}' не содержит разделителя");

        // Имя поля обязано совпасть с ожидаемым: иначе порядок полей не тот.
        var name = line[..separator];
        if (!string.Equals(name, expectedName, StringComparison.Ordinal))
            throw new FormatException($"ожидалось поле '{expectedName}', прочитано '{name}'");

        // Значение раскодируется обратным правилом.
        return Decode(line[(separator + 1)..]);
    }

    // NextLong читает длинное целое той же парой правил.
    private static long NextLong(string[] lines, ref int cursor, string expectedName)
    {
        // Значение читается как текст и разбирается неизменным видом.
        var value = NextValue(lines, ref cursor, expectedName);
        if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            throw new FormatException($"поле '{expectedName}' не является целым числом");

        // Разобранное число возвращается вызывающему.
        return parsed;
    }

    // Encode кодирует значение так, чтобы ни перевод строки, ни табуляция внутри него не
    // превратились в границу строки или поля файла.
    private static string Encode(string? value)
    {
        // Отсутствующее значение несёт свою метку и не совпадает с пустой строкой.
        if (value is null)
            return AbsentValueToken;

        // Сначала экранируется сама обратная косая черта, иначе её нельзя было бы отличить.
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal);
    }

    // Decode возвращает значение обратным правилом, знак за знаком.
    private static string? Decode(string encoded)
    {
        // Метка отсутствия читается обратно именно как отсутствие значения.
        if (string.Equals(encoded, AbsentValueToken, StringComparison.Ordinal))
            return null;

        // Накопитель раскодированного значения.
        var builder = new StringBuilder(encoded.Length);
        for (var index = 0; index < encoded.Length; index++)
        {
            // Обычный знак переносится как есть.
            if (encoded[index] != '\\')
            {
                builder.Append(encoded[index]);
                continue;
            }

            // Косая черта в самом конце означает непригодную запись.
            if (index + 1 >= encoded.Length)
                throw new FormatException("незавершённое экранирование в значении поля");

            // Следующий знак называет, что именно было закодировано.
            index++;
            builder.Append(encoded[index] switch
            {
                '\\' => '\\',
                'n' => '\n',
                'r' => '\r',
                't' => '\t',
                _ => throw new FormatException("неизвестное экранирование в значении поля")
            });
        }

        // Готовое значение возвращается вызывающему.
        return builder.ToString();
    }
}

// Связка адресной доставки исправляющего урока.
//
// Порядок проверок задан здесь и только здесь, потому что порядок — часть договора. Два места, где
// порядок несёт смысл, названы прямо в коде ниже: долговечная отметка читается ДО суждения о
// устаревшей версии, а записывается ПОСЛЕ успешной публикации.
public sealed class KnnAddressedCorrectionCoordinator<TOutput>
{
    // Коды отказов связки. Каждый называет ровно одну причину, по которой урок не доставлен.
    public const string RequestRequiredErrorCode = "ADDRESSED_CORRECTION_REQUEST_REQUIRED";
    public const string RunRequiredErrorCode = "ADDRESSED_CORRECTION_RUN_REQUIRED";
    public const string FeedbackRequiredErrorCode = "ADDRESSED_CORRECTION_FEEDBACK_REQUIRED";
    public const string OwnerRequiredErrorCode = "ADDRESSED_CORRECTION_OWNER_REQUIRED";
    public const string MaterialRequiredErrorCode = "ADDRESSED_CORRECTION_MATERIAL_REQUIRED";
    public const string AnswerFromAnotherRunErrorCode = "ADDRESSED_CORRECTION_ANSWER_FROM_ANOTHER_RUN";
    public const string ForeignResponseEvidenceErrorCode = "ADDRESSED_CORRECTION_FOREIGN_RESPONSE_EVIDENCE";
    public const string UnknownSelectedOwnerErrorCode = "ADDRESSED_CORRECTION_UNKNOWN_SELECTED_OWNER";
    public const string ForeignOwnerDomainErrorCode = "ADDRESSED_CORRECTION_FOREIGN_OWNER_DOMAIN";
    public const string ForeignOwnerProfileErrorCode = "ADDRESSED_CORRECTION_FOREIGN_OWNER_PROFILE";
    public const string ForeignOwnerRoleErrorCode = "ADDRESSED_CORRECTION_FOREIGN_OWNER_ROLE";
    public const string StaleSelectedVersionErrorCode = "ADDRESSED_CORRECTION_STALE_SELECTED_VERSION";
    public const string OwnerMovedBeforePublicationErrorCode = "ADDRESSED_CORRECTION_OWNER_MOVED_BEFORE_PUBLICATION";
    public const string PublishedOtherOwnerErrorCode = "ADDRESSED_CORRECTION_PUBLISHED_OTHER_OWNER";
    public const string CancelledErrorCode = "ADDRESSED_CORRECTION_CANCELLED";

    // Справочник владельцев: откуда связка берёт ЖИВОЕ опознание выбранной модели.
    private readonly IAddressedCorrectionOwnerDirectory<TOutput> _directory;
    // Граница обучения и публикации: кто именно учит выбранного владельца.
    private readonly IAddressedCorrectionWriter<TOutput> _writer;
    // Каталог долговечных отметок применения. Именно он делает дедупликацию устойчивой.
    private readonly string _journalRoot;

    // Связка собирается из двух интерфейсов и одного каталога: своего обученного состояния у неё
    // нет и быть не должно.
    public KnnAddressedCorrectionCoordinator(
        IAddressedCorrectionOwnerDirectory<TOutput> directory,
        IAddressedCorrectionWriter<TOutput> writer,
        string journalRoot)
    {
        // Справочник владельцев обязателен: без него нельзя узнать живую версию адресата.
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        // Обучающая граница обязательна: сама связка не обучает.
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        // Каталог отметок обязателен: без него дедупликация жила бы в памяти процесса.
        _journalRoot = string.IsNullOrWhiteSpace(journalRoot)
            ? throw new ArgumentException("Каталог журнала адресных доставок обязателен.", nameof(journalRoot))
            : journalRoot;
    }

    // Применить один исправляющий урок к ВЫБРАННОМУ владельцу.
    public async Task<OperationResult<AddressedCorrectionReceipt>> ApplyAsync(
        AddressedCorrection<TOutput> request,
        CancellationToken cancellationToken = default)
    {
        // Отмена возвращается общим конвертом до любой работы.
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<AddressedCorrectionReceipt>.Fail("Отменено.", CancelledErrorCode);

        // Шаг 0. Состав запроса. Пустые обязательные поля — это отсутствие входа, а не отказ урока.
        var shape = ValidateShape(request);
        // Отказ состава возвращается как есть: ни обучения, ни отметки не было.
        if (!shape.IsSuccess)
            return shape.MapError<AddressedCorrectionReceipt>();

        // Опознание выбранного владельца и опознание фактического ответа читаются один раз.
        var owner = request.Owner;
        var evidence = request.ResponseEvidence;

        // Шаг 1. Адрес урока берётся из ФАКТИЧЕСКОГО ответа, а не из текста запроса.
        // Ответ другого запуска исправлять этим обращением нельзя: он относится к другому следу.
        if (!string.Equals(evidence.RunId, request.RunId, StringComparison.Ordinal))
            return OperationResult<AddressedCorrectionReceipt>.Fail(
                $"Ответ принадлежит запуску '{evidence.RunId}', а исправление пришло с запуском " +
                $"'{request.RunId}'.",
                AnswerFromAnotherRunErrorCode);

        // Опознание ответа обязано называть ровно того владельца, которому адресован урок: модель,
        // домен, версию и порядковый номер регистрации.
        if (!string.Equals(evidence.AnsweredUnitId, owner.UnitId, StringComparison.Ordinal) ||
            !string.Equals(evidence.AnsweredDomainTag, owner.DomainTag, StringComparison.Ordinal) ||
            !string.Equals(evidence.AnsweredVersion, owner.Version, StringComparison.Ordinal) ||
            evidence.AnsweredRegistrationOrder != owner.RegistrationOrder ||
            // Опознание состояния переносится вместе с ответом. Связка его СРАВНИВАЕТ, но не
            // вычисляет: побайтовая сохранность состояния этим НЕ доказывается.
            !string.Equals(evidence.AnsweredStateIdentity, owner.TrainedStateIdentity, StringComparison.Ordinal))
            return OperationResult<AddressedCorrectionReceipt>.Fail(
                $"Опознание ответа ('{evidence.AnsweredUnitId}'/'{evidence.AnsweredDomainTag}'/" +
                $"'{evidence.AnsweredVersion}'/{evidence.AnsweredRegistrationOrder}) не совпадает с " +
                $"выбранным владельцем ('{owner.UnitId}'/'{owner.DomainTag}'/'{owner.Version}'/" +
                $"{owner.RegistrationOrder}).",
                ForeignResponseEvidenceErrorCode);

        // Шаг 2. Живое опознание владельца читается из справочника по его опознанию модели.
        var resolved = _directory.ResolveLatestOwnerById(owner.UnitId);
        // Неизвестный владелец — отказ ДО обучения: обе памяти остаются прежними.
        if (!resolved.IsSuccess)
            return OperationResult<AddressedCorrectionReceipt>.Fail(
                $"Выбранного владельца '{owner.UnitId}' в справочнике нет: {resolved.ErrorMessage}",
                UnknownSelectedOwnerErrorCode);

        // Живое описание владельца и его исполняемый объект берутся из ОДНОЙ записи справочника.
        var live = resolved.Value!.Descriptor;

        // Чужой домен не исправляется этим обращением: домен — часть связки, а не подсказка.
        if (!string.Equals(live.DomainTag, owner.DomainTag, StringComparison.Ordinal))
            return OperationResult<AddressedCorrectionReceipt>.Fail(
                $"Владелец '{owner.UnitId}' живёт в домене '{live.DomainTag}', а урок адресован " +
                $"домену '{owner.DomainTag}'.",
                ForeignOwnerDomainErrorCode);

        // Профиль каталога правил ранжирования обязан совпадать с доменом владельца. Это тот же
        // продуктовый инвариант, по которому обученная единица отвергает каталог чужого профиля.
        if (!string.Equals(owner.Profile, live.DomainTag, StringComparison.Ordinal))
            return OperationResult<AddressedCorrectionReceipt>.Fail(
                $"Профиль '{owner.Profile}' не совпадает с доменом владельца '{live.DomainTag}'.",
                ForeignOwnerProfileErrorCode);

        // Роль — это место, которое владелец занимает. Чужая роль означает другого адресата.
        if (!string.Equals(live.UnitRole, owner.UnitRole, StringComparison.Ordinal))
            return OperationResult<AddressedCorrectionReceipt>.Fail(
                $"Владелец '{owner.UnitId}' занимает роль '{live.UnitRole}', а урок адресован роли " +
                $"'{owner.UnitRole}'.",
                ForeignOwnerRoleErrorCode);

        // Составной ключ доставки считается однозначным кодированием, а не склейкой строк.
        var deliveryKey = AddressedCorrectionDeliveryJournal.ComposeDeliveryKey(
            request.RunId,
            request.FeedbackId,
            owner);

        // Шаг 3. Долговечная отметка применения читается ЗДЕСЬ, до суждения об устаревшей версии.
        //
        // Почему именно в этом порядке. Успешная доставка сама делает прежнюю версию старой. Если
        // проверять устаревание раньше, повторное то же обращение получило бы отказ «версия
        // устарела» вместо прежней квитанции — и вызывающий, не найдя квитанции, имел бы повод
        // учить второй раз. Поэтому уже применённый урок узнаётся первым.
        var applied = AddressedCorrectionDeliveryJournal.TryLoadApplied(_journalRoot, deliveryKey);
        // Нечитаемый журнал — отказ: молча считать урок применимым нельзя.
        if (!applied.IsSuccess)
            return applied.MapError<AddressedCorrectionReceipt>();

        // Урок уже применён: возвращается ПРЕЖНЯЯ квитанция, без обучения и без публикации.
        if (applied.Value!.Found)
            return OperationResult<AddressedCorrectionReceipt>.Ok(
                applied.Value.Receipt! with { ReusedCompletedDelivery = true });

        // Шаг 4. Устаревшая версия отвергается ДО обучения: урок относился к другому состоянию.
        if (!string.Equals(live.Version, owner.Version, StringComparison.Ordinal) ||
            live.RegistrationOrder != owner.RegistrationOrder)
            return OperationResult<AddressedCorrectionReceipt>.Fail(
                $"Выбранная версия владельца '{owner.Version}' (порядок {owner.RegistrationOrder}) " +
                $"уже не живая: в справочнике '{live.Version}' (порядок {live.RegistrationOrder}).",
                StaleSelectedVersionErrorCode);

        // Шаг 5. Обучение исправленного кандидата. Выбранная единица передаётся ОБЪЕКТОМ из той же
        // записи справочника, откуда прочитано её описание: адресность доказывается фактически
        // вызванным членом, а не записью в полях.
        var candidate = _writer.TrainCorrectedOwner(
            resolved.Value.Unit,
            owner,
            request.CorrectedExampleId,
            request.CorrectedAnswer,
            cancellationToken);

        // Отказ обучения возвращается как есть. Отметки применения при этом НЕ появляется, поэтому
        // урок остаётся применимым: это и есть слово «ПОСЛЕ».
        if (!candidate.IsSuccess)
            return candidate.MapError<AddressedCorrectionReceipt>();

        // Шаг 6. Перед публикацией живое опознание владельца читается ЗАНОВО: за время обучения его
        // мог сменить другой писатель.
        var recheck = _directory.ResolveLatestOwnerById(owner.UnitId);
        // Пропавший за время обучения владелец — отказ публикации, а не повторное обучение.
        if (!recheck.IsSuccess)
            return OperationResult<AddressedCorrectionReceipt>.Fail(
                $"Выбранного владельца '{owner.UnitId}' перед публикацией в справочнике нет: " +
                $"{recheck.ErrorMessage}",
                OwnerMovedBeforePublicationErrorCode);

        // Сдвинувшийся владелец означает чужую публикацию между обучением и нашей: публикация
        // отменяется, оплаченный кандидат остаётся у вызывающего, повторное обучение запрещено.
        // Отметки применения не появляется, урок остаётся применимым.
        if (!string.Equals(recheck.Value!.Descriptor.Version, owner.Version, StringComparison.Ordinal) ||
            recheck.Value.Descriptor.RegistrationOrder != owner.RegistrationOrder)
            return OperationResult<AddressedCorrectionReceipt>.Fail(
                $"Владельца '{owner.UnitId}' между обучением и публикацией сменил другой писатель: " +
                $"было '{owner.Version}' (порядок {owner.RegistrationOrder}), стало " +
                $"'{recheck.Value.Descriptor.Version}' (порядок " +
                $"{recheck.Value.Descriptor.RegistrationOrder}).",
                OwnerMovedBeforePublicationErrorCode);

        // Шаг 7. Публикуется ТОЛЬКО выбранный владелец новой версией.
        var published = _writer.PublishCorrectedOwner(candidate.Value!, cancellationToken);
        // Отказ публикации возвращается как есть. Отметки применения НЕ появляется.
        if (!published.IsSuccess)
            return published.MapError<AddressedCorrectionReceipt>();

        // Опубликованное обязано быть тем же владельцем: иначе урок ушёл в соседнюю память.
        if (!string.Equals(published.Value!.UnitId, owner.UnitId, StringComparison.Ordinal) ||
            !string.Equals(published.Value.DomainTag, owner.DomainTag, StringComparison.Ordinal) ||
            !string.Equals(published.Value.UnitRole, owner.UnitRole, StringComparison.Ordinal))
            return OperationResult<AddressedCorrectionReceipt>.Fail(
                $"Опубликован владелец '{published.Value.UnitId}'/'{published.Value.DomainTag}'/" +
                $"'{published.Value.UnitRole}', а урок адресован '{owner.UnitId}'/'{owner.DomainTag}'/" +
                $"'{owner.UnitRole}'.",
                PublishedOtherOwnerErrorCode);

        // Квитанция собирается из опознаний ДО и ПОСЛЕ публикации.
        var receipt = new AddressedCorrectionReceipt(
            deliveryKey,
            request.RunId,
            request.FeedbackId,
            owner.DomainTag,
            owner.Profile,
            owner.UnitId,
            owner.UnitRole,
            owner.Version,
            owner.RegistrationOrder,
            published.Value.Version,
            published.Value.RegistrationOrder,
            request.CorrectedExampleId);

        // Шаг 8. ТОЛЬКО ТЕПЕРЬ, после состоявшейся публикации, на диск кладётся отметка применения.
        // Отметка до обучения — именно тот опытный образец, который сюда НЕ перенесён: он считал
        // урок доставленным даже тогда, когда доставка не состоялась.
        var marked = AddressedCorrectionDeliveryJournal.MarkApplied(_journalRoot, receipt);
        // Неудача записи отметки не замалчивается: публикация состоялась, и вызывающий обязан
        // узнать, что долговечного признака применения нет.
        if (!marked.IsSuccess)
            return marked.MapError<AddressedCorrectionReceipt>();

        // Шаг 9. Доставка состоялась: квитанция уходит вызывающему, и она же лежит на диске.
        return await Task.FromResult(OperationResult<AddressedCorrectionReceipt>.Ok(receipt));
    }

    // ValidateShape проверяет только состав запроса: отсутствие обязательного поля — это не отказ
    // урока, а отсутствие входа, и называется оно отдельным кодом.
    private static OperationResult<bool> ValidateShape(AddressedCorrection<TOutput> request)
    {
        // Без самого запроса проверять нечего.
        if (request is null)
            return OperationResult<bool>.Fail("Запрос адресной доставки обязателен.", RequestRequiredErrorCode);
        // Запуск опознаёт след исполнения, в котором был получен исправляемый ответ.
        if (string.IsNullOrWhiteSpace(request.RunId))
            return OperationResult<bool>.Fail("Опознание запуска обязательно.", RunRequiredErrorCode);
        // Обращение пользователя опознаёт само исправление и входит в ключ доставки.
        if (string.IsNullOrWhiteSpace(request.FeedbackId))
            return OperationResult<bool>.Fail("Опознание обращения обязательно.", FeedbackRequiredErrorCode);
        // Без опознания владельца урок некому адресовать.
        if (request.Owner is null)
            return OperationResult<bool>.Fail("Опознание выбранного владельца обязательно.", OwnerRequiredErrorCode);
        // Без опознания фактического ответа адрес урока был бы догадкой.
        if (request.ResponseEvidence is null)
            return OperationResult<bool>.Fail("Опознание фактического ответа обязательно.", OwnerRequiredErrorCode);
        // Пустые поля опознания владельца сделали бы связку домена, модели и версии неполной.
        if (string.IsNullOrWhiteSpace(request.Owner.DomainTag) ||
            string.IsNullOrWhiteSpace(request.Owner.Profile) ||
            string.IsNullOrWhiteSpace(request.Owner.UnitId) ||
            string.IsNullOrWhiteSpace(request.Owner.UnitRole) ||
            string.IsNullOrWhiteSpace(request.Owner.Version) ||
            string.IsNullOrWhiteSpace(request.Owner.TrainedStateIdentity))
            return OperationResult<bool>.Fail(
                "Опознание владельца обязано называть домен, профиль, модель, роль, версию и состояние.",
                OwnerRequiredErrorCode);
        // Без опознания исправляемой строки урока неизвестно, что именно исправляется.
        if (string.IsNullOrWhiteSpace(request.CorrectedExampleId))
            return OperationResult<bool>.Fail(
                "Опознание исправляемой строки урока обязательно.",
                MaterialRequiredErrorCode);
        // Пустой исправленный ответ ничему не учит.
        if (request.CorrectedAnswer is null)
            return OperationResult<bool>.Fail("Исправленный ответ обязателен.", MaterialRequiredErrorCode);

        // Состав запроса пригоден: дальше судят уже о самой доставке.
        return OperationResult<bool>.Ok(true);
    }
}
