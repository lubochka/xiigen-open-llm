// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Договор владения ОТВЕТОМ обучаемого блока ближайших соседей
// (работа R2 плана ремонта 102, поздний аудит 29 октября).
//
// Зачем это нужно по-человечески. Блок ближайших соседей запоминает уроки и
// потом отвечает сохранённым ответом ближайшего урока. Описания урока (условия,
// предметные метки, ссылки на доказательства) блок уже копирует себе, а сам
// ОТВЕТ до этой правки хранился и выдавался той же ссылкой, которую дала
// вызывающая сторона. Если ответ изменяемый — например ответ-маршрут со списком
// шагов, — то получатель ответа мог изменить этот объект и тем самым молча
// переписать обученную память. Второй раз тот же вопрос возвращал уже
// испорченный ответ, хотя никакого обучения между этими чтениями не было.
//
// Что делает этот договор. Он отвечает ровно на один вопрос: кому принадлежит
// значение ответа. Политика применяется дважды: при приёме урока в память
// (ForStorage) и при каждой выдаче ответа наружу (ForRead). Между вызывающей
// стороной и памятью после этого нет общего изменяемого объекта.
//
// Это механизм владения, а не отдельная обучаемая единица: он ничему не учится,
// ничего не выбирает и не участвует в поиске соседа.
//
// Почему интерфейс открытый (public), а не internal. Он входит в открытый путь
// обучения: его принимает перегрузка метода обучения, и его реализуют внешние
// доменные проекты для своих изменяемых типов ответа. Слово «служебный» здесь
// означает «вспомогательный по смыслу», а не уровень доступа C#.
//
// Правила выбора политики.
// 1. Для заведомо неизменяемых типов (строка, перечисление, простые числовые
//    типы и несколько известных типов значений) достаточно политики тождества:
//    копия не нужна, потому что испортить такое значение снаружи нельзя.
// 2. Для изменяемых типов нужна явная типизированная реализация этого договора,
//    которая создаёт новый верхний объект И копирует вложенные коллекции.
// 3. Произвольный ссылочный тип нельзя молча считать неизменяемым. Встроенная
//    политика для заведомо неизменяемых типов на таком типе даёт внятный отказ
//    с именованным кодом, а не тихо принимает чужую ссылку.
// 4. Универсальное клонирование через сериализацию в JSON запрещено как
//    неявное умолчание: оно теряет типы, ссылки и поведение, а выглядит как
//    бесплатная безопасность.
// 5. Политика по умолчанию выбирается ТОЛЬКО для явно поддержанных
//    неизменяемых типов. Любой другой тип ответа получает именованный отказ,
//    и этот отказ обязан случиться до построения памяти и до выдачи
//    обученного блока наружу. Режим общего доступа остаётся доступным, но его
//    называет сам вызывающий: молча он не выбирается никогда, потому что
//    молчаливый общий доступ снаружи неотличим от настоящего владения.
// 6. Для точки сохранения модели сериализуются ДАННЫЕ и СХЕМА, а политику
//    владения выбирает владелец типа при загрузке. Политика поверх функции
//    копирования хранит исполняемую функцию и переносимым состоянием не
//    является; в описи модели она называется «custom» именно для того, чтобы
//    её нельзя было принять за сохранённое состояние. У обычного блока
//    ближайших соседей точки сохранения сегодня нет, и это правило записано
//    заранее, чтобы будущая точка сохранения не попыталась сериализовать
//    функцию копирования вместо данных и схемы.

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// Договор владения значением ответа обучаемого блока ближайших соседей.
// Два действия: взять значение во владение памяти и выдать значение наружу.
public interface IKnnAnswerSnapshot<TOutput>
{
    // Вернуть значение, которым память владеет единолично: вызывающая сторона
    // после обучения не должна иметь возможности изменить сохранённый ответ.
    OperationResult<TOutput> ForStorage(TOutput value);

    // Вернуть значение для выдачи наружу: получатель ответа не должен иметь
    // возможности изменить то, что осталось лежать в памяти.
    OperationResult<TOutput> ForRead(TOutput ownedValue);
}

// Политика тождества для заведомо неизменяемых ответов: лишних копий нет.
// На любом другом типе она отказывает, а не делает вид, что владеет значением.
public sealed class KnownImmutableKnnAnswerSnapshot<TOutput> : IKnnAnswerSnapshot<TOutput>
{
    // Отказ формулируется один раз и одинаково для приёма и для выдачи.
    private static OperationResult<TOutput> Refuse()
    {
        // Имя типа названо прямо в сообщении, чтобы вызывающая сторона сразу
        // видела, для какого ответа нужна своя типизированная политика.
        return OperationResult<TOutput>.Fail(
            $"Answer type '{typeof(TOutput).FullName}' is not a known-immutable type, so this unit cannot own its answers. Supply an explicit IKnnAnswerSnapshot<{typeof(TOutput).Name}> implementation that copies the top-level object and its nested collections.",
            "KNN_ANSWER_SNAPSHOT_POLICY_REQUIRED");
    }

    // Приём урока в память: неизменяемое значение сохраняется как есть.
    public OperationResult<TOutput> ForStorage(TOutput value)
    {
        // Неизвестный изменяемый тип останавливает обучение внятным отказом.
        if (!KnnAnswerSnapshot.IsKnownImmutable(typeof(TOutput)))
            return Refuse();

        // Заведомо неизменяемое значение отдаётся тем же объектом: копия не нужна.
        return OperationResult<TOutput>.Ok(value);
    }

    // Выдача ответа наружу: неизменяемое значение отдаётся как есть.
    public OperationResult<TOutput> ForRead(TOutput ownedValue)
    {
        // Тот же отказ действует и на выдаче, чтобы путь чтения не был слабее.
        if (!KnnAnswerSnapshot.IsKnownImmutable(typeof(TOutput)))
            return Refuse();

        // Испортить строку или число снаружи нельзя, поэтому копия не создаётся.
        return OperationResult<TOutput>.Ok(ownedValue);
    }
}

// Политика «владение объявлено общим»: память и вызывающая сторона делят одну
// ссылку, и это записано явно, а не подразумевается. Она применяется только
// тогда, когда вызывающая сторона не передала свою политику, а тип ответа не
// является заведомо неизменяемым. Такой блок НЕ заявляет владения ответом.
public sealed class DeclaredSharedKnnAnswerSnapshot<TOutput> : IKnnAnswerSnapshot<TOutput>
{
    // Приём урока в память: ссылка сохраняется как есть, владение не заявлено.
    public OperationResult<TOutput> ForStorage(TOutput value)
    {
        // Копии нет и обещания владения нет: состояние названо в описи модели.
        return OperationResult<TOutput>.Ok(value);
    }

    // Выдача ответа наружу: та же ссылка, владение по-прежнему не заявлено.
    public OperationResult<TOutput> ForRead(TOutput ownedValue)
    {
        // Копии нет и обещания владения нет: состояние названо в описи модели.
        return OperationResult<TOutput>.Ok(ownedValue);
    }
}

// Политика поверх заданной вызывающей стороной функции копирования. Она нужна
// доменным проектам, у которых изменяемый тип ответа уже умеет копировать себя:
// функция копирования остаётся типизированной и явной, универсального
// клонирования здесь нет.
public sealed class DelegatingKnnAnswerSnapshot<TOutput> : IKnnAnswerSnapshot<TOutput>
{
    // Функция, создающая независимую копию значения ответа.
    private readonly Func<TOutput, TOutput> _copy;

    // Политика собирается вокруг одной функции копирования.
    public DelegatingKnnAnswerSnapshot(Func<TOutput, TOutput> copy)
    {
        // Отсутствие функции копирования — ошибка сборки политики, а не ответа.
        _copy = copy ?? throw new ArgumentNullException(nameof(copy));
    }

    // Приём урока в память: память получает собственную копию значения.
    public OperationResult<TOutput> ForStorage(TOutput value)
    {
        // Копия создаётся заданной функцией и возвращается общим конвертом.
        return Copy(value);
    }

    // Выдача ответа наружу: наружу уходит отдельная копия, а не память.
    public OperationResult<TOutput> ForRead(TOutput ownedValue)
    {
        // Копия создаётся той же функцией, поэтому оба пути одинаково надёжны.
        return Copy(ownedValue);
    }

    // Одно место, где заданная функция копирования вызывается и проверяется.
    private OperationResult<TOutput> Copy(TOutput value)
    {
        // Сбой пользовательской функции копирования остаётся внутри конверта.
        try
        {
            // Создать независимую копию значения ответа.
            return OperationResult<TOutput>.Ok(_copy(value));
        }
        catch (Exception ex)
        {
            // Понятный именованный отказ вместо выброса исключения наружу.
            return OperationResult<TOutput>.Fail(ex.Message, "KNN_ANSWER_SNAPSHOT_COPY_FAILED");
        }
    }
}

// Сборка политик владения ответом и единственное место, где решается, какой
// тип ответа считается заведомо неизменяемым.
public static class KnnAnswerSnapshot
{
    // Имя состояния «память владеет ответом, потому что он неизменяем».
    public const string KnownImmutableKind = "known-immutable";

    // Имя состояния «владение ответом объявлено общим и памятью не заявлено».
    public const string DeclaredSharedKind = "declared-shared";

    // Имя состояния «вызывающая сторона передала свою типизированную политику».
    public const string CustomKind = "custom";

    // Закрытый список типов значений, которые нельзя испортить снаружи.
    // Список именно закрытый: догадки о произвольном типе здесь запрещены.
    private static readonly HashSet<Type> KnownImmutableValueTypes =
    [
        typeof(decimal),
        typeof(Guid),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(DateOnly),
        typeof(TimeOnly),
        typeof(TimeSpan)
    ];

    // Политика тождества для заведомо неизменяемого ответа.
    public static IKnnAnswerSnapshot<TOutput> KnownImmutable<TOutput>()
    {
        // Отдельного состояния у политики нет, поэтому создаётся новый экземпляр.
        return new KnownImmutableKnnAnswerSnapshot<TOutput>();
    }

    // Политика с явно объявленным общим владением ответом.
    public static IKnnAnswerSnapshot<TOutput> DeclaredShared<TOutput>()
    {
        // Отдельного состояния у политики нет, поэтому создаётся новый экземпляр.
        return new DeclaredSharedKnnAnswerSnapshot<TOutput>();
    }

    // Политика поверх заданной вызывающей стороной функции копирования.
    public static IKnnAnswerSnapshot<TOutput> FromCopy<TOutput>(Func<TOutput, TOutput> copy)
    {
        // Проверка самой функции живёт внутри политики, здесь только сборка.
        return new DelegatingKnnAnswerSnapshot<TOutput>(copy);
    }

    // Политика по умолчанию, когда вызывающая сторона не передала свою.
    // Решение возвращается общим конвертом, а не готовой политикой: для
    // неподдержанного типа ответа политики по умолчанию НЕ СУЩЕСТВУЕТ, и это
    // обязано быть видно вызывающей стороне как именованный отказ, а не как
    // тихо подставленный общий доступ.
    public static OperationResult<IKnnAnswerSnapshot<TOutput>> ResolveDefault<TOutput>()
    {
        // Заведомо неизменяемый ответ: память владеет им без единой копии.
        if (IsKnownImmutable(typeof(TOutput)))
            return OperationResult<IKnnAnswerSnapshot<TOutput>>.Ok(KnownImmutable<TOutput>());

        // Любой другой тип ответа остановлен здесь. Прежнее поведение молча
        // выбирало общий доступ: память и вызывающая сторона оставались на
        // одной ссылке, и получатель ответа мог переписать обученную память,
        // ничему её не обучая. Сообщение называет тип ответа и обе законные
        // двери, чтобы вызывающая сторона не осталась в тупике: типизированный
        // владелец копии либо явно названный общий доступ, который владением
        // не считается.
        return OperationResult<IKnnAnswerSnapshot<TOutput>>.Fail(
            $"Answer type '{typeof(TOutput).FullName}' is not a supported immutable answer type, so the default overload cannot own its answers. Call the overload that takes an explicit IKnnAnswerSnapshot<{typeof(TOutput).Name}>: use KnnAnswerSnapshot.FromCopy for a typed copy owner that also copies nested collections, or KnnAnswerSnapshot.DeclaredShared to state shared access explicitly (declared shared access is not ownership).",
            "KNN_ANSWER_SNAPSHOT_DEFAULT_POLICY_REQUIRED");
    }

    // Имя состояния политики для описи обученной модели.
    public static string DescribeKind<TOutput>(IKnnAnswerSnapshot<TOutput> snapshot)
    {
        // Встроенная политика тождества для неизменяемых ответов.
        if (snapshot is KnownImmutableKnnAnswerSnapshot<TOutput>)
            return KnownImmutableKind;

        // Встроенная политика с явно объявленным общим владением.
        if (snapshot is DeclaredSharedKnnAnswerSnapshot<TOutput>)
            return DeclaredSharedKind;

        // Любая переданная вызывающей стороной типизированная политика.
        return CustomKind;
    }

    // Единственное решение о том, можно ли отдавать значение этого типа наружу
    // без копии. Ответ «да» даётся только по закрытому списку, а не по догадке.
    public static bool IsKnownImmutable(Type type)
    {
        // Без типа решения нет, поэтому ответ консервативный.
        if (type is null)
            return false;

        // Строка неизменяема по устройству среды выполнения.
        if (type == typeof(string))
            return true;

        // Перечисление — это значение, его нельзя изменить через полученную ссылку.
        if (type.IsEnum)
            return true;

        // Простые типы среды выполнения: целые, вещественные, логический, знак.
        if (type.IsPrimitive)
            return true;

        // Несколько известных типов значений из закрытого списка выше.
        if (KnownImmutableValueTypes.Contains(type))
            return true;

        // Обнуляемая обёртка неизменяема ровно настолько, насколько её содержимое.
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying is not null)
            return IsKnownImmutable(underlying);

        // Всё остальное — произвольный тип, и молча считать его неизменяемым нельзя.
        return false;
    }
}
