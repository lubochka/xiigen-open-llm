// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// Доверенный договор оценки материала обучения способности.
//
// Зачем это нужно, по-человечески. Чтобы честно сказать «модель умеет обобщать», экзаменационные
// строки не должны встречаться в обучении — ни теми же байтами, ни под новым именем. Проверить
// это может только тот, кто видит ФАКТИЧЕСКИЙ материал и отдельный замороженный список
// источников. Этот файл задаёт сам список и роли строк; проверку выполняет сторож в соседнем
// файле.
//
// Чего здесь нет намеренно. Здесь нет разбивателя наборов, нет оценщика качества, нет модели и
// нет определителя смысловой семьи по тексту. Семья берётся из замороженного источника, а не
// выводится из содержания. Ожидаемый ответ принадлежит оценщику и в обучение не идёт.
//
// Откуда взялся этот договор. Опыт сто двенадцатый разбил материал по семьям и убрал пересечение
// обучающего и проверочного наборов с двух семей до нуля. Это выигрыш достоверности ОЦЕНКИ, а не
// рост качества: на внешнем экзамене оба плеча дали два верных ответа из четырёх — ровно столько
// же даёт постоянный ответ, и один класс строк при этом просел с двух из двух до нуля из двух.
// Поэтому переносится честность проверки материала, а не корпус и не заявление о точности.

// CapabilityMaterialRole называет роль строки материала.
public enum CapabilityMaterialRole
{
    // Train — строка идёт в обучение. Только эти строки получает обучаемая единица.
    Train,
    // Dev — строка идёт в промежуточную проверку. В обучение не идёт.
    Dev,
    // Test — строка идёт во внешний экзамен. В обучение не идёт ни под каким именем.
    Test
}

// CapabilitySourceRow — одна строка ЗАМОРОЖЕННОГО доверенного источника.
public sealed record CapabilitySourceRow(
    // SourceRowId — имя строки в источнике. Оно неизменно и не зависит от имени примера обучения.
    string SourceRowId,
    // SourceFamilyId — семья строки по источнику. Семья приходит отсюда и только отсюда: выводить
    // её из текста запрещено, потому что это была бы догадка о смысле, а не факт происхождения.
    string SourceFamilyId,
    // Role — роль строки: обучение, промежуточная проверка или внешний экзамен.
    CapabilityMaterialRole Role,
    // Content — точный текст строки источника. Сторож считает его отпечаток сам.
    string Content,
    // ExpectedOutput — верный ответ. Принадлежит оценщику; обучаемой единице он не передаётся.
    string? ExpectedOutput);

// CapabilityActualMaterialRow — одна ФАКТИЧЕСКАЯ строка материала, которую подготовили к
// обучению. Именно её содержание сторож проверяет заново, а не рассказ о ней.
public sealed record CapabilityActualMaterialRow(
    // ExampleId — имя примера обучения. Уникальность этого имени НЕ доказывает независимость
    // семей: две строки одной семьи легко получают разные имена.
    string ExampleId,
    // ClaimedSourceRowId — на какую строку источника ссылается этот пример.
    string ClaimedSourceRowId,
    // Role — заявленная роль этого примера.
    CapabilityMaterialRole Role,
    // Content — фактический текст, который пойдёт дальше. Единственный источник истины о байтах.
    string Content,
    // PresentedContentSha256 — ПРЕДЪЯВЛЕННЫЙ отпечаток точных байтов. Доверять ему нельзя: сторож
    // пересчитывает отпечаток из Content и сверяет. Опытный проверяющий искал совпадение именно
    // по предъявленному полю, не пересчитывая его, и поэтому согласованно подделанная расписка
    // прошла бы у него как честная.
    string PresentedContentSha256,
    // PresentedNormalizedSha256 — предъявленный отпечаток приведённого текста; проверяется так же.
    string PresentedNormalizedSha256);

// CapabilityMaterialNormalizationPolicy называет способ приведения текста перед сравнением.
public enum CapabilityMaterialNormalizationPolicy
{
    // ExactBytesOnly — приведения нет, сравниваются только точные байты. Это безопасный выбор по
    // умолчанию для любого материала, о котором не доказано обратное.
    ExactBytesOnly,
    // LowerCaseAndPunctuationStripped — нижний регистр и удаление знаков препинания.
    //
    // ГРАНИЦА ЭТОГО ПРИВЕДЕНИЯ, названная прямо. Это приведение тестового корпуса опыта сто
    // двенадцатого, где строки — короткие английские предложения. Оно НЕ является доказательством
    // смысловой эквивалентности и не годится для кода на C#, для запросов SQL и для перевода: там
    // регистр и знаки меняют смысл, а не оформление. Выбирать это приведение можно только там,
    // где владелец материала обосновал его для своего корпуса.
    LowerCaseAndPunctuationStripped
}

// CapabilityMaterialIntersectionPolicy — ЯВНО заданная политика запрещённых пересечений.
//
// Политика задаётся словами и значением, а не выводится из примера. Опыт сто двенадцатый считал
// пересечение обучающего и проверочного наборов ИЗМЕРЯЕМОЙ величиной: у построчного плеча оно
// законно равнялось двум, и это не было порчей. Для продуктовой оценки независимости это уже
// запрет, и запрет называется здесь, а не додумывается читателем.
public sealed record CapabilityMaterialIntersectionPolicy(
    // ForbidTrainTestContentOverlap — текст экзамена не имеет права встречаться в обучении.
    bool ForbidTrainTestContentOverlap,
    // ForbidTrainDevContentOverlap — то же про промежуточную проверку.
    bool ForbidTrainDevContentOverlap,
    // ForbidTrainTestFamilyOverlap — семья экзамена не имеет права встречаться в обучении, даже
    // если сами строки разные. Это и есть «независимость семей».
    bool ForbidTrainTestFamilyOverlap,
    // ForbidTrainDevFamilyOverlap — то же для промежуточной проверки.
    bool ForbidTrainDevFamilyOverlap,
    // CompareByNormalizedContent — сравнивать ли содержание ещё и по приведённому тексту. Имеет
    // смысл только вместе с приведением, обоснованным для этого корпуса.
    bool CompareByNormalizedContent)
{
    // IndependentFamilies — политика независимой оценки: запрещено всё перечисленное, сравнение
    // идёт и по точным байтам, и по приведённому тексту.
    public static CapabilityMaterialIntersectionPolicy IndependentFamilies { get; } =
        new(true, true, true, true, true);

    // ExactContentOnly — политика без приведения: запрещены только совпадения точных байтов и
    // пересечения семей. Пригодна там, где приведение текста не обосновано.
    public static CapabilityMaterialIntersectionPolicy ExactContentOnly { get; } =
        new(true, true, true, true, false);
}

// CapabilityEvaluationMaterial — замороженный доверенный договор оценки целиком.
public sealed record CapabilityEvaluationMaterial(
    // ProfileId — имя профиля оценки: по нему видно, какой материал и какая политика применялись.
    string ProfileId,
    // SourceManifestSha256 — отпечаток описи источника. Хранится как связь с неизменной описью.
    string SourceManifestSha256,
    // SourceRows — сами строки замороженного источника вместе с их семьями и ролями.
    IReadOnlyList<CapabilitySourceRow> SourceRows,
    // NormalizationPolicy — выбранное приведение текста с названной выше границей.
    CapabilityMaterialNormalizationPolicy NormalizationPolicy,
    // IntersectionPolicy — явно заданные запрещённые пересечения.
    CapabilityMaterialIntersectionPolicy IntersectionPolicy)
{
    // SchemaVersion — имя версии этого договора. Нужно, чтобы читатель не принял будущий договор
    // за нынешний и наоборот.
    public const string SchemaVersion = "capability-eval-material-v1";
}

// CapabilityMaterialAdmissionReceipt — расписка допуска материала к обучению.
//
// Расписка сама по себе ничего не доказывает: она описывает то, что сторож ПЕРЕСЧИТАЛ из
// фактических строк. Принять расписку без лежащих под ней фактических строк нельзя — именно это
// и было второй половиной дефекта опытного проверяющего.
public sealed record CapabilityMaterialAdmissionReceipt(
    // ProfileId — профиль оценки, по которому шла проверка.
    string ProfileId,
    // SchemaVersion — версия договора материала.
    string SchemaVersion,
    // CheckedRowCount — сколько фактических строк проверено.
    int CheckedRowCount,
    // TrainRowCount, DevRowCount, TestRowCount — сколько строк в каждой роли.
    int TrainRowCount,
    int DevRowCount,
    int TestRowCount,
    // TrainFamilies — семьи обучения, определённые по ПЕРЕСЧИТАННЫМ отпечаткам, а не по заявке.
    IReadOnlyList<string> TrainFamilies,
    // HeldOutFamilies — семьи промежуточной проверки и экзамена, определённые тем же способом.
    IReadOnlyList<string> HeldOutFamilies,
    // NormalizationPolicy и IntersectionPolicy — какие именно правила применялись.
    CapabilityMaterialNormalizationPolicy NormalizationPolicy,
    CapabilityMaterialIntersectionPolicy IntersectionPolicy,
    // ModelVisibleCollisionCheck — проверялись ли совпадения уже после превращения текста в числа
    // для модели. Здесь всегда «не измерено»: такая проверка требует собственного пути и своего
    // наряда. Источник-непересекающийся НЕ означает непересекающийся после кодирования.
    string ModelVisibleCollisionCheck,
    // ConsumedPreferenceRowCount — сколько фактически потребляемых обучением пар предпочтений было
    // связано с этим материалом. Ноль означает, что связи не было вовсе: сама проверка
    // происхождения смотрит только на строки подготовителя и о потребляемом не знает ничего.
    int ConsumedPreferenceRowCount,
    // ConsumedMaterialSha256 — отпечаток ФАКТИЧЕСКИ потребляемых обучением байтов. Отсутствие
    // значения означает отсутствие связи, а не пустой материал: это разные факты.
    string? ConsumedMaterialSha256,
    // ConsumedMaterialBinding — что именно доказано о связи проверенного материала с потребляемым.
    // Поле нужно затем, чтобы расписка не выглядела сильнее, чем есть: проверка происхождения сама
    // по себе связи не доказывает, и расписка это говорит прямо.
    string ConsumedMaterialBinding)
{
    // ConsumedIdentitySha256 — отпечаток ТОЖДЕСТВА всего, чем владеет допущенный носитель: общий
    // учебный текст, вопрос, обе стороны и учебный ВЕКТОР каждой пары, а также упорядоченный
    // корпус поиска. Поле добавлено отдельно от ConsumedMaterialSha256 намеренно: тот остался
    // текстовым и неизменным, потому что он опубликованный член этой расписки и его байты сверяют
    // прежние читатели. Отсутствие значения означает отсутствие сверки тождества, а не пустой
    // материал. Свойство объявлено со значением по умолчанию, поэтому прежние вызывающие
    // конструктора продолжают работать без правок.
    public string? ConsumedIdentitySha256 { get; init; }

    // ConsumedRetrievalCorpusRowCount — сколько строк упорядоченного корпуса поиска было связано
    // с этим материалом. Ноль означает, что корпус носителю не подавали вовсе.
    public int ConsumedRetrievalCorpusRowCount { get; init; }

    // NotMeasured — единственное допустимое сейчас значение проверки совпадений после кодирования.
    public const string NotMeasured = "NOT_MEASURED";

    // ConsumedMaterialNotBound — проверено только происхождение строк подготовителя. Что именно
    // получит обучатель, этой распиской не доказано.
    public const string ConsumedMaterialNotBound = "CONSUMED_MATERIAL_NOT_BOUND";

    // ConsumedHeldOutContentAbsent — проверено происхождение И доказано, что в фактически
    // потребляемом обучением материале нет содержания отложенных строк: ни по точным байтам, ни
    // по приведённому тексту, если политика велит сравнивать и так.
    //
    // ГРАНИЦА ЭТОГО ДОКАЗАТЕЛЬСТВА, названная прямо. Это ОТРИЦАТЕЛЬНАЯ связь: доказано
    // отсутствие отложенного содержания в том, что обучают. Положительное покрытие — что каждый
    // потребляемый байт происходит из допущенной обучающей строки — здесь НЕ доказано, потому что
    // договора составления обучающих строк из строк источника в продукте пока нет.
    public const string ConsumedHeldOutContentAbsent = "HELD_OUT_CONTENT_ABSENT_FROM_CONSUMED_MATERIAL";

    // ConsumedTrainCompleteAndHeldOutAbsent — доказаны ОБЕ половины связи: в том, что обучают,
    // нет содержания отложенных строк, и при этом содержание КАЖДОЙ допущенной учебной строки в
    // нём присутствует. Вторая половина закрывает потерю: прежде подготовитель мог честно
    // предъявить шесть разрешённых учебных строк, а до обучателя доходили три, и расписка этого
    // не замечала.
    //
    // ГРАНИЦА, названная прямо. Это по-прежнему не доказательство того, что каждый потребляемый
    // байт происходит из допущенной обучающей строки: в потребляемом материале законно есть и
    // своё оформление, и повторения, заданные явной проекцией. Доказано присутствие всего
    // разрешённого и отсутствие всего отложенного, не больше.
    public const string ConsumedTrainCompleteAndHeldOutAbsent =
        "TRAIN_COMPLETE_AND_HELD_OUT_ABSENT_IN_CONSUMED_MATERIAL";
}

// CapabilityEvaluationMaterialSubmission — то, что доверенный подготовитель оценки подаёт сторожу:
// замороженный договор И фактические строки, подготовленные к обучению. Одного договора мало, и
// одной расписки мало: проверять надо сами строки.
public sealed record CapabilityEvaluationMaterialSubmission(
    // Material — замороженный доверенный договор оценки.
    CapabilityEvaluationMaterial Material,
    // ActualOwnedRows — фактические строки материала, чьи байты и будут проверены заново.
    IReadOnlyList<CapabilityActualMaterialRow> ActualOwnedRows);

// ICapabilityEvaluationMaterialProvider — источник доверенного материала оценки.
//
// Почему это отдельный участник, а не поле запроса обучения. Материал оценки обязан приходить от
// доверенного подготовителя в составе обучателя, а НЕ из пользовательских полей запроса и не из
// доменных метк: иначе проверку независимости можно было бы пройти, просто объявив нужные
// значения. Прятать семью происхождения в доменные метки запрещено отдельно.
public interface ICapabilityEvaluationMaterialProvider
{
    // TryProvide возвращает материал для этого запуска и этой доменной метки, либо отсутствие
    // материала. Отсутствие — обычный, разрешённый случай: прежнее обучение без оценки
    // независимости продолжает работать, просто независимая оценка по нему не выдаётся.
    OperationResult<CapabilityEvaluationMaterialSubmission?> TryProvide(string runId, string domainTag);
}
