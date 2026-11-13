// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler29;

// RouteClassifier: classify input into one of five canonical domains.
// E18: +15.2% — 95% domain accuracy on C# corpus.
// Uses predicate profile (E17 output) as feature vector, plus keyword fallback.
// Canonical domain names: CODE_Q / BUG / PLAN / ARCH / REFACTOR (DECISIONS-LOCKED D-04).
public sealed class RouteClassifier : BaseService, IRouteClassifier29
{
    // Память маршрутизатора: таблица принятых уроков. Поле намеренно не readonly —
    // приём партии публикует собственную новую таблицу вместо правки прежней.
    private List<(int[] Predicates, Domain Domain)> _train = [];
    private readonly int _k;

    // Standard constructor: inject storage and logger without constructing them here.
    public RouteClassifier(ICompilerStorage storage, ICompilerEventLogger logger, int k = 5)
        : base(storage, logger)
    {
        _k = Math.Max(1, k);
    }

    // Keyword sets for each domain — priority order: BUG first, then others.
    private static readonly Dictionary<Domain, string[]> KeywordMap = new()
    {
        [Domain.BUG]     = ["fix","error","exception","null","fail","wrong","broken","why","debug","crash","bug"],
        [Domain.REFACTOR]= ["refactor","clean","improve","simplify","restructur","rename","extract","move"],
        [Domain.PLAN]    = ["flow","phase","plan","step","sequence","orchestrat","workflow","deploy","implement","build","create"],
        [Domain.ARCH]    = ["architect","architecture","topology","design","pattern","structure","best","should","approach","layer","boundary","solid"],
        [Domain.CODE_Q]  = ["what","how","explain","show","find","list","where","which","describe","understand","does","return","use","pattern"],
    };

    // Predicate → domain affinity for E17 predicate-profile routing.
    private static readonly Dictionary<string, Domain> PredicateDomainMap = new()
    {
        ["NULL_CHECK"]  = Domain.BUG,
        ["CATCH_OP"]    = Domain.BUG,
        ["THROW_OP"]    = Domain.BUG,
        ["VALIDATE_OP"] = Domain.CODE_Q,
        ["GET_OP"]      = Domain.CODE_Q,
        ["CREATE_OP"]   = Domain.CODE_Q,
        ["LOOP_OP"]     = Domain.CODE_Q,
        ["FILTER_OP"]   = Domain.REFACTOR,
        ["CONVERT_OP"]  = Domain.REFACTOR,
        ["AGGREGATE_OP"]= Domain.PLAN,
        ["SEND_OP"]     = Domain.PLAN,
        ["AUTH_OP"]     = Domain.ARCH,
    };

    // Classify a token array via predicate profile vote, then keyword fallback.
    public Domain Classify(string[] tokens)
    {
        if (tokens.Length == 0) return Domain.UNKNOWN;

        // Predicate-profile voting: count domain votes from each token.
        var votes = tokens
            .Where(t => PredicateDomainMap.ContainsKey(t))
            .GroupBy(t => PredicateDomainMap[t])
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        if (votes != null) return votes.Key;

        // Fallback: keyword matching on the joined token string.
        return ClassifyByKeywords(string.Join(" ", tokens));
    }

    // Train the canonical E18 KNN route classifier on predicate profiles.
    // Приём партии уроков устроен так: сначала вся партия целиком проверяется и
    // переносится в приватную таблицу собственными копиями, и только после
    // полного успеха принятая память заменяется одной операцией. Обучение
    // остаётся полной ЗАМЕНОЙ набора уроков, а не дописыванием к нему.
    // Форма отказа прежняя — исключение: подпись метода не меняется.
    public void Train(IReadOnlyList<(int[] Predicates, Domain Domain)> examples)
    {
        // Отсутствующая партия обучением не является: отказ происходит до любой правки памяти.
        ArgumentNullException.ThrowIfNull(examples);

        // Новая приватная таблица собирается в стороне, чтобы принятая память пережила любой отказ.
        var ownedRows = new List<(int[] Predicates, Domain Domain)>(examples.Count);

        // Каждый урок партии переносится в таблицу поэлементной копией, а не ссылкой вызывающей стороны.
        foreach (var example in examples)
        {
            // Урок без массива признаков ничему не учит и отвергается до замены памяти.
            if (example.Predicates is null)
                throw new ArgumentException(
                    "Урок партии пришёл без массива признаков: обучение отклонено, принятая память сохранена.",
                    nameof(examples));

            // Под признаки урока выделяется собственный массив ровно той длины, какую дал вызывающий.
            var ownedPredicates = new int[example.Predicates.Length];

            // Значения признаков переписываются в собственный массив: ссылка вызывающей стороны в память не попадает.
            Array.Copy(example.Predicates, ownedPredicates, ownedPredicates.Length);

            // В таблицу памяти кладётся урок, массив признаков которого принадлежит маршрутизатору.
            ownedRows.Add((ownedPredicates, example.Domain));
        }

        // Партия без единого прочитанного урока оставила бы маршрутизатор необученным,
        // то есть молчаливо стёрла бы всё выученное: такой приём отвергается.
        if (ownedRows.Count == 0)
            throw new ArgumentException(
                "Партия уроков пуста: обучение отклонено, принятая память сохранена.",
                nameof(examples));

        // Собранная таблица публикуется целиком и одной операцией становится памятью маршрутизатора.
        _train = ownedRows;
    }

    // Classify a predicate profile through the trained KNN model.
    //
    // FIX-RF05, первая малая правка пятой строки очереди адресных ремонтов
    // (файл плана «108. Адресный план ремонта после проверки переносов»,
    // 1 ноября 2026). Прежде здесь стояла ровно одна проверка — необученность, —
    // и запрос без признаков уходил прямо в поиск соседей. Там мера расстояния
    // даёт пустому запросу РОВНО ЕДИНИЦУ до каждого урока: пересечение пусто,
    // объединение равно размеру урока. Все расстояния равны, упорядочивание
    // устойчиво, поэтому отбор брал первые уложенные строки памяти, а голосование
    // возвращало метку ПЕРВОГО уложенного урока — и возвращало её как УСПЕХ, без
    // единого признака того, что решение ни на чём не основано. Отсутствующий
    // массив вместо этого бросал исключение из построения набора, что нарушает
    // первое правило проекта: для бизнес-логики не бросать, а возвращать отказ.
    //
    // FIX-RF05, продолжение от 1 ноября 2026: тот же отказ выдаётся и при нулевом
    // сходстве — когда ни один выученный урок не разделяет с запросом ни одного
    // признака. Это ВТОРАЯ, продуктово достижимая половина того же дефекта:
    // пустой и отсутствующий профиль потребитель отсекает сам своей охраной
    // непустого профиля, а запрос с неизвестными признаками эту охрану проходит и
    // прежде получал тему первого уложенного урока как уверенный успех. В отличие
    // от первой половины эта правка МЕНЯЕТ продуктовый ответ: при отказе
    // потребитель проваливается в объявленную детерминированную цепочку.
    //
    // Порядок трёх проверок выбран намеренно и его нельзя менять ради красоты:
    // необученность проверяется ПЕРВОЙ, как и раньше, затем пустой и
    // отсутствующий вход, и только затем нулевое сходство. Если поменять порядок,
    // необученный маршрутизатор с пустым запросом начнёт отвечать новым кодом
    // вместо прежнего «не обучен», то есть правка молча изменит уже принятый
    // ответ там, где её об этом не просили. Нулевое сходство стоит последним ещё
    // и потому, что только оно требует посчитать расстояния, то есть цену.
    public OperationResult<Domain> Classify(int[] predicates)
    {
        // Без памяти решать нечем; прежний код отказа сохранён дословно.
        if (_train.Count == 0)
            return OperationResult<Domain>.Fail("Classifier not trained", "NOT_TRAINED");

        // Запрос без признаков — отсутствующий массив или массив нулевой длины —
        // не несёт свидетельства о теме, поэтому отказ выдаётся ДО поиска
        // соседей: до единственного места, где равные расстояния превращались в
        // метку первого урока.
        if (predicates is null || predicates.Length == 0)
            return OperationResult<Domain>.Fail(
                "Профиль признаков пуст: свидетельства для решения о теме нет.",
                "NO_EVIDENCE_FOR_DECISION");

        // Расстояния до всех уроков считаются ОДИН раз и дальше используются и
        // для проверки нулевого сходства, и для выбора соседей. Порядок строк
        // здесь тот же, что в памяти, поэтому последующее упорядочивание по
        // расстоянию остаётся устойчивым и выбор соседей не меняется ни на один
        // урок по сравнению с прежним кодом.
        var scored = _train
            .Select(lesson => (Lesson: lesson, Distance: JaccardDist(predicates, lesson.Predicates)))
            .ToList();

        // Нулевое сходство: ЛУЧШЕЕ, то есть наименьшее, расстояние равно единице —
        // значит ни один выученный урок не разделяет с запросом ни одного
        // признака. Прежде такой запрос получал тему первого уложенного урока как
        // успех, потому что все расстояния равнялись единице и выбор решал
        // порядок памяти, а не близость. Сравнивать нужно именно наименьшее
        // расстояние, а не первое: при первом совпавшем уроке где-нибудь в
        // середине памяти отказ сработал бы ошибочно.
        //
        // Единица здесь не подобранный порог, а определение: мера несходства
        // равна единице ровно тогда, когда пересечение признаков пусто, и выше
        // единицы она не бывает. Поэтому сравнение «не меньше единицы» означает
        // «общего признака нет вовсе» и никакого порога близости не вводит;
        // общий порог близости — отдельная неизмеренная работа.
        if (scored.Min(row => row.Distance) >= 1.0)
            return OperationResult<Domain>.Fail(
                "Ни один выученный урок не разделяет с запросом ни одного признака: свидетельства для решения о теме нет.",
                "NO_EVIDENCE_FOR_DECISION");

        // Выбор соседей и голосование повторяют прежний код слово в слово;
        // изменился только источник уже посчитанного расстояния.
        var best = scored
            .OrderBy(row => row.Distance)
            .Take(_k)
            .GroupBy(row => row.Lesson.Domain)
            .OrderByDescending(g => g.Count())
            .First();

        return OperationResult<Domain>.Ok(best.Key);
    }

    // Keyword-matching fallback — also used directly from SemanticCompilerPipeline.
    public static Domain ClassifyByKeywords(string text)
    {
        // Priority order: BUG before REFACTOR before PLAN before ARCH before CODE_Q.
        foreach (var domain in new[] { Domain.BUG, Domain.REFACTOR, Domain.PLAN, Domain.ARCH })
            if (MatchedKeywords(text, domain).Length > 0)
                return domain;
        return Domain.CODE_Q;
    }

    public static string[] MatchedKeywords(string text, Domain domain)
    {
        if (!KeywordMap.TryGetValue(domain, out var keywords))
            return [];

        var lower = text.ToLowerInvariant();
        return keywords
            .Where(keyword => lower.Contains(keyword, StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(keyword => keyword, StringComparer.Ordinal)
            .ToArray();
    }

    public static string[] KeywordsFor(Domain domain)
        => KeywordMap.TryGetValue(domain, out var keywords) ? keywords.ToArray() : [];

    // Мера несходства двух наборов признаков: ноль — полное совпадение,
    // единица — ни одного общего признака.
    //
    // FIX-RF05, два соседних случая той же правки. Первый: отсутствующий массив
    // прежде бросал исключение прямо здесь, в построении набора, хотя вызов
    // пришёл из обычного пути чтения, а не из программной ошибки. Теперь
    // отсутствующий массив читается как набор без признаков, и отказ принимает
    // вызывающая сторона, а не исключение. Второй: два набора без признаков
    // прежде объявлялись расстоянием НОЛЬ, то есть идеальным совпадением — это
    // прямо противоположно смыслу, потому что общего признака у них не один, а
    // ни одного. Теперь такая пара получает единицу, как и любая пара без
    // пересечения.
    private static double JaccardDist(int[] a, int[] b)
    {
        // Отсутствующий массив признаков — это набор без признаков, а не ошибка вызова.
        var sa = a is null ? new HashSet<int>() : new HashSet<int>(a);

        // То же правило для второй стороны: урок без массива признаков не бросает исключение.
        var sb = b is null ? new HashSet<int>() : new HashSet<int>(b);

        // Сколько признаков запроса действительно встретилось в уроке.
        int inter = sa.Count(x => sb.Contains(x));

        // Сколько различных признаков есть у двух наборов вместе.
        int union = sa.Count + sb.Count - inter;

        // Пустое объединение означает, что общих признаков нет ни одного, а не
        // что наборы совпали: отсюда единица, прежде здесь стоял ноль. После
        // раннего отказа выше этот случай из обученной перегрузки недостижим,
        // поэтому правка снимает ловушку, а не меняет достижимый ответ.
        return union == 0 ? 1.0 : 1.0 - (double)inter / union;
    }
}
