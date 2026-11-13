// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;

namespace VectorSearch;

/// <summary>
/// Hierarchical Navigable Small World (HNSW) approximate nearest-neighbour index.
///
/// Structure: each inserted vector gets a random max-level.
/// Upper levels are sparse (fast coarse navigation).
/// Level 0 contains all vectors (precise local search).
///
/// Insert: find entry point at top level, greedily descend to target level,
///         then connect the new node to its ef-construction nearest neighbours.
/// Search: greedy descent from top to level 1, then beam-search at level 0.
///
/// DNA rule 4: implements IVectorIndex.
/// </summary>
public class HnswIndex : IVectorIndex
{
    private readonly int _m;              // max neighbours per node per level
    private readonly int _efConstruction; // search width during build
    private int _efSearch;                // search width during query

    private readonly List<float[]> _vectors = [];
    private readonly List<string> _ids = [];
    private readonly List<Dictionary<string, string>> _meta = [];
    private readonly List<List<List<int>>> _graph = [];  // [node][level][neighbours]
    private int _entryPoint = -1;
    private int _maxLevel = -1;

    // The dimension this index has accepted. -1 means no vector has been
    // accepted yet, so the next accepted one decides it. It is derived, never
    // stored: a saved index keeps the same file shape and old files still load.
    private int _dimensions = -1;

    private static readonly Random _rng = new();

    public int Count => _vectors.Count;

    // FIX-RFD-01, девятая работа ремонта, второй пробел (план 116 из папки
    // docs/plans/replanning-possible-diffs/fixes-based-on-experiments, разбор
    // RF02/RF09 от 1 ноября 2026). Прежняя девятая работа закрыла СОСТОЯНИЕ
    // навигации при загрузке — точку входа, уровни, диапазоны соседей — и не
    // закрыла её ПАРАМЕТРЫ. Ширина построения входит в экземпляр здесь, и
    // непригодное значение отвергается до того, как экземпляр появится.
    //
    // Почему непригодная ширина опасна именно тихо: при нуле луч поиска по слою
    // в добавлении возвращает пустой список кандидатов, отбор соседей тоже пуст,
    // поэтому цикл двусторонних связей не выполняется ни разу. Новый узел
    // остаётся без соседей на всех своих уровнях, а старые узлы не получают
    // ссылки на него — новое знание оказывается отсоединённым от старого, при
    // этом указатель считается годным и продолжает отвечать.
    //
    // Область намеренно узкая: число соседей и ширина поиска здесь НЕ
    // проверяются. У ширины поиска дефекта нет — публичный поиск берёт
    // наибольшее из неё и запрошенного числа ответов; числу соседей нужна своя
    // политика допустимых значений, и она в этот ремонт не входит.
    public HnswIndex(int m = 16, int efConstruction = 200, int efSearch = 50)
    {
        // Ширина построения меньше единицы делает добавление бессвязным, поэтому
        // она отвергается нарушением договора вызова с названным полем и его
        // фактическим значением — по образцу соседних кодов отказа загрузки.
        if (efConstruction <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(efConstruction),
                efConstruction,
                "INDEX_CONSTRUCTION_BEAM_INVALID: the construction beam must be positive, otherwise an add finds no neighbours at all.");

        _m = m;
        _efConstruction = efConstruction;
        _efSearch = efSearch;
    }

    public void SetEfSearch(int ef) => _efSearch = ef;

    // FIX-R11 transfer, plan 98-current-transfer-repair-plan-2026-10-29.md, from P21.
    //
    // The shape is checked here, before the first line that changes anything.
    // Until this check existed, a vector of a foreign length was already in the
    // three lists, the counter had already grown and the level draw had already
    // been taken from the shared generator by the time the traversal walked off
    // the end of an array. A rejected call must leave the index exactly as it
    // was and must not spend a draw, so nothing below this block may run first.
    public void Add(float[] vector, string id, Dictionary<string, string>? metadata = null)
    {
        // A missing array is a caller-contract violation, not a business outcome.
        if (vector is null)
            throw new ArgumentNullException(nameof(vector), "VECTOR_REQUIRED");
        // An empty vector carries no position, and as the very first one it would
        // fix the accepted dimension at zero and lock the whole index out.
        if (vector.Length == 0)
            throw new ArgumentException("VECTOR_EMPTY: a vector must have at least one component.", nameof(vector));
        // Once a dimension has been accepted, every later vector must match it.
        if (_dimensions >= 0 && vector.Length != _dimensions)
            throw new ArgumentException(
                $"VECTOR_DIMENSION_MISMATCH: expected {_dimensions}, actual {vector.Length}", nameof(vector));

        // From here the call is admitted; the first admitted vector fixes the dimension.
        _dimensions = vector.Length;

        // FIX-R29 ownership transfer, same repair as the plain nearest-neighbour unit.
        //
        // Until this copy existed the index stored the caller's own array. The caller
        // could change its cells afterwards and the stored memory changed with them:
        // the same query answered differently with no Add, no re-training and no
        // reload in between, and no event marked the moment. From this line on the
        // insert works only on the index's own array, so placement and storage can
        // never disagree; the caller keeps its array and may change it freely.
        float[] owned = [.. vector];

        int newId = _vectors.Count;
        // The index stores its own array, never the caller's.
        _vectors.Add(owned);
        _ids.Add(id);
        // The metadata dictionary is taken by copy for the same reason. The caller's
        // own key comparer travels with the copy, so lookup semantics stay exactly
        // what the caller chose and a case-colliding pair cannot fail this copy.
        _meta.Add(metadata is null ? [] : new Dictionary<string, string>(metadata, metadata.Comparer));

        int level = RandomLevel();
        var nodeLevels = new List<List<int>>(level + 1);
        for (int l = 0; l <= level; l++) nodeLevels.Add([]);
        _graph.Add(nodeLevels);

        if (_entryPoint == -1)
        {
            _entryPoint = newId;
            _maxLevel = level;
            return;
        }

        int currEntry = _entryPoint;

        // Greedy descent from top to level+1
        for (int l = _maxLevel; l > level; l--)
            // The descent reads the stored array, so the node is placed by the same
            // values the index now holds and not by memory the caller still owns.
            currEntry = GreedySearch(owned, currEntry, l);

        // From level down to 0: find neighbours and bidirectionally connect
        for (int l = Math.Min(level, _maxLevel); l >= 0; l--)
        {
            int maxM = l == 0 ? _m * 2 : _m;
            // The beam search at this level reads the stored array for the same reason.
            var candidates = SearchLayer(owned, [currEntry], _efConstruction, l);
            // The neighbour ranking reads the stored array for the same reason.
            var neighbours = SelectNeighbours(owned, candidates.Select(c => c.id).ToList(), maxM);

            foreach (int nb in neighbours)
            {
                _graph[newId][l].Add(nb);
                _graph[nb][l].Add(newId);
                if (_graph[nb][l].Count > maxM)
                    _graph[nb][l] = SelectNeighbours(_vectors[nb], _graph[nb][l], maxM);
            }

            if (candidates.Count > 0) currEntry = candidates[0].id;
        }

        if (level > _maxLevel)
        {
            _maxLevel = level;
            _entryPoint = newId;
        }
    }

    public List<(string id, float score)> Search(float[] query, int topK)
    {
        if (_entryPoint == -1) return [];

        // FIX-R11: an index that holds vectors has an accepted dimension, so a
        // query of another length can only walk off the end of a stored vector
        // mid-traversal. It is refused here, before the first comparison.
        if (query is null)
            throw new ArgumentNullException(nameof(query), "QUERY_REQUIRED");
        if (query.Length != _dimensions)
            throw new ArgumentException(
                $"QUERY_DIMENSION_MISMATCH: expected {_dimensions}, actual {query.Length}", nameof(query));

        int currEntry = _entryPoint;
        for (int l = _maxLevel; l > 0; l--)
            currEntry = GreedySearch(query, currEntry, l);

        var candidates = SearchLayer(query, [currEntry], Math.Max(_efSearch, topK), 0);
        return candidates.Take(topK)
            .Select(c => (_ids[c.id], c.score))
            .ToList();
    }

    // ── Persistence (DNA rule 7: atomic write) ───────────────────────────────

    public void Save(string path)
    {
        var data = new SerializedIndex
        {
            Vectors = _vectors.Select(v => v.ToList()).ToList(),
            Ids = _ids,
            Meta = _meta,
            Graph = _graph.Select(n => n.Select(l => l.ToList()).ToList()).ToList(),
            EntryPoint = _entryPoint,
            MaxLevel = _maxLevel,
            M = _m,
            EfConstruction = _efConstruction,
            EfSearch = _efSearch
        };
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(data));
        File.Move(tmp, path, overwrite: true);
    }

    public static HnswIndex Load(string path)
    {
        var data = JsonSerializer.Deserialize<SerializedIndex>(File.ReadAllText(path))
            ?? throw new InvalidOperationException("Failed to deserialize HNSW index");

        // FIX-R11: a saved index is checked before a usable instance is handed
        // back. A file whose vectors disagree on length, or whose four main
        // collections hold different numbers of rows, is already broken; letting
        // it become a live index only moves the failure to the first query.
        // The accepted dimension of an older file is read off its first vector
        // and then every other vector is held to it; the file shape itself is
        // unchanged, so this is no schema migration.
        if (data.Vectors is null || data.Ids is null || data.Meta is null || data.Graph is null)
            throw new InvalidOperationException("INDEX_COLLECTION_MISSING: a saved index needs vectors, ids, meta and graph.");
        if (data.Ids.Count != data.Vectors.Count || data.Meta.Count != data.Vectors.Count || data.Graph.Count != data.Vectors.Count)
            throw new InvalidOperationException(
                $"INDEX_COUNT_MISMATCH: vectors {data.Vectors.Count}, ids {data.Ids.Count}, meta {data.Meta.Count}, graph {data.Graph.Count}");

        // A stored row that is absent or carries no components has no position at
        // all, so it is refused before it can state or satisfy a dimension.
        for (int i = 0; i < data.Vectors.Count; i++)
            if (data.Vectors[i] is null || data.Vectors[i].Count == 0)
                throw new InvalidOperationException($"INDEX_VECTOR_EMPTY: stored vector {i} has no components.");

        // The first stored vector states the dimension; -1 keeps an empty file empty.
        int dimensions = data.Vectors.Count > 0 ? data.Vectors[0].Count : -1;
        for (int i = 1; i < data.Vectors.Count; i++)
            // Every remaining vector is held to the dimension the first one stated.
            if (data.Vectors[i].Count != dimensions)
                throw new InvalidOperationException(
                    $"INDEX_VECTOR_DIMENSION_MISMATCH: vector {i} has {data.Vectors[i].Count} components, expected {dimensions}");

        // Ninth repair work: the navigation state of a saved index is checked here
        // too, not only the shape of its stored vectors.
        //
        // The audit of the vector index (plan "51. Допуск векторного указателя и
        // частичное состояние", the paragraph on Save/Load) recorded that a load
        // restores the file without a general check of connectivity and field
        // agreement; the preceding repair closed the vector lengths and the row
        // counts and wrote that exact remainder down as its own boundary. Until
        // this block existed the entry point, the top level and the whole
        // neighbour graph were copied into a live instance unchecked, so a
        // substituted or truncated file became a usable index and failed only
        // later: an entry point outside the stored nodes threw an index-out-of-
        // range from the first traversal, a neighbour outside them threw the same
        // thing but only once a query happened to walk that far, a missing level
        // list threw an opaque null-reference from the copy loop below, and the
        // two quietest states - an entry point of -1 over stored vectors and a
        // node with no level zero - threw nothing at all and simply answered
        // wrongly. A refusal at load names the broken field instead.
        //
        // The admission ground is the third one: removal of a named defect. There
        // is no measured experiment behind it - the failure is the one the plan
        // predicts from the source, and that is stated plainly rather than dressed
        // up as a measurement.
        //
        // Nothing here touches learning quality or speed. The checks live only on
        // this restore path; accepting a vector, training and answering a query
        // are untouched, and the work is one linear pass over nodes and edges once
        // per load, never per query.
        int nodes = data.Vectors.Count;

        // A file with no vectors must say so in both navigation fields; any other
        // pair means the header and the body disagree about an empty index.
        if (nodes == 0 && (data.EntryPoint != -1 || data.MaxLevel != -1))
            throw new InvalidOperationException(
                $"INDEX_EMPTY_STATE_INCONSISTENT: an index with no vectors needs entry point -1 and top level -1, actual entry point {data.EntryPoint}, top level {data.MaxLevel}.");

        // With vectors present the entry point must name one of them. -1 here is
        // the quiet case: every query would return nothing while the index still
        // reports its rows, so it is refused rather than left to answer wrongly.
        if (nodes > 0 && (data.EntryPoint < 0 || data.EntryPoint >= nodes))
            throw new InvalidOperationException(
                $"INDEX_ENTRY_POINT_OUT_OF_RANGE: entry point {data.EntryPoint} is not one of the {nodes} stored nodes.");

        for (int i = 0; i < nodes; i++)
        {
            // A node without its list of levels cannot be copied at all; without
            // this refusal the copy loop below fails on an empty reference instead.
            var nodeLevels = data.Graph[i];
            if (nodeLevels is null)
                throw new InvalidOperationException($"INDEX_GRAPH_NODE_MISSING: node {i} has no list of levels.");
            // Every node of a built index owns level zero. A node without it is
            // sterile: the search reaches nothing through it and it answers nothing.
            if (nodeLevels.Count == 0)
                throw new InvalidOperationException($"INDEX_GRAPH_NODE_EMPTY: node {i} has no level zero, so no search can pass through it.");

            for (int l = 0; l < nodeLevels.Count; l++)
            {
                // A level without its list of neighbours is the same empty
                // reference in the copy loop below, so it is named here as well.
                var neighbours = nodeLevels[l];
                if (neighbours is null)
                    throw new InvalidOperationException($"INDEX_GRAPH_LEVEL_MISSING: node {i} level {l} has no list of neighbours.");

                for (int k = 0; k < neighbours.Count; k++)
                    // A neighbour outside the stored nodes is read as a stored
                    // vector during the traversal, which is where the old failure
                    // came from - and only when a query happened to walk there.
                    if (neighbours[k] < 0 || neighbours[k] >= nodes)
                        throw new InvalidOperationException(
                            $"INDEX_NEIGHBOUR_OUT_OF_RANGE: node {i} level {l} points at node {neighbours[k]}, which is not one of the {nodes} stored nodes.");
            }
        }

        // The descent starts at the top level of the entry point, and an index
        // built by accepting vectors always keeps those two in step: the entry
        // point is the deepest node and the top level is its own depth. A file
        // where they disagree navigates from a level the entry point does not
        // have, so the descent silently does nothing.
        if (nodes > 0 && data.MaxLevel != data.Graph[data.EntryPoint].Count - 1)
            throw new InvalidOperationException(
                $"INDEX_MAX_LEVEL_MISMATCH: top level {data.MaxLevel} disagrees with the {data.Graph[data.EntryPoint].Count} levels of entry point {data.EntryPoint}.");

        // FIX-RFD-09, девятая работа ремонта, две оставшиеся межузловые проверки
        // (уточнённый договор ремонта 114-post88-addressed-repair-plan-2026-11-01,
        // раздел согласованного графа; опубликованный аудит границ исполнения
        // 116-post89-execution-boundary-audit-2026-11-01, раздел про два
        // отдельных договора загрузки).
        //
        // Что прежние проверки НЕ видели. Выше проверено, что каждый сосед —
        // один из сохранённых узлов, и что объявленный верхний уровень совпадает
        // с глубиной ТОЧКИ ВХОДА. Не проверено двух межузловых согласований:
        // что сосед, названный на уровне l, сам доживает до уровня l, и что
        // объявленный верх равен глубине САМОГО ГЛУБОКОГО узла, а не только
        // точки входа.
        //
        // Почему оба дефекта тихие. Обе ветви навигации перед обходом соседей
        // спрашивают, есть ли у текущего узла нужный уровень: жадный спуск
        // обрывается, а луч поиска по слою молча не разворачивает узел. Значит
        // ребро на отсутствующий уровень не падает ошибкой — оно укорачивает
        // навигацию. Узел глубже объявленного верха вообще не посещается: спуск
        // начинается с объявленного уровня, поэтому всё верхнее знание такого
        // узла мертво, а добавление поднимает верх только собственной выборкой
        // уровня и само это не исправляет. Опубликованный аудит показал это
        // прямо: обе испорченные копии сохранённого графа загрузкой принимались,
        // и при этом все двенадцать проверочных запросов возвращали ожидаемые
        // опознаватели — панель точности к структурному дефекту нечувствительна.
        //
        // Третья проверка договора — что точка входа принадлежит ГЛОБАЛЬНОМУ
        // верхнему уровню — отдельной строкой НЕ добавляется, и это измеренный
        // вывод, а не упущение. Прежняя проверка выше требует равенства глубины
        // точки входа объявленному верху, а новая проверка ниже требует
        // равенства ему же глобального максимума; из двух равенств третье
        // следует. Отдельный код отказа для него был бы недостижим.
        //
        // Проход отдельный и идёт ПОСЛЕ прежнего намеренно: внутри прежнего
        // прохода ещё не известно, что у всех узлов список уровней есть и не
        // пуст, поэтому чтение глубины соседа там могло бы упасть непрозрачным
        // обращением к пустой ссылке на узле, до которого проверка ещё не
        // дошла. Порядок и коды прежних отказов не изменены ни одной строкой:
        // новые проверки стоят после них.
        //
        // Взаимность направленных рёбер НЕ требуется: родная обрезка соседей её
        // не обещает, и такое требование отвергало бы законные графы.
        if (nodes > 0)
        {
            // Глубина самого глубокого узла и его номер: номер нужен, чтобы
            // отказ называл, кто именно оказался глубже объявленного верха.
            int deepestNode = 0;
            int deepestLevel = data.Graph[0].Count - 1;

            for (int i = 0; i < nodes; i++)
            {
                // Список уровней этого узла уже проверен выше: он есть и не пуст.
                int nodeTop = data.Graph[i].Count - 1;
                if (nodeTop > deepestLevel)
                {
                    deepestLevel = nodeTop;
                    deepestNode = i;
                }

                for (int l = 0; l < data.Graph[i].Count; l++)
                    foreach (int nb in data.Graph[i][l])
                        // Сосед уже проверен на попадание в число узлов, а здесь
                        // — на то, что у него есть сам уровень ссылки. Без этой
                        // проверки спуск по слою на таком ребре обрывается, и
                        // навигация укорачивается молча.
                        if (data.Graph[nb].Count <= l)
                            throw new InvalidOperationException(
                                $"INDEX_NEIGHBOUR_LEVEL_MISSING: node {i} level {l} points at node {nb}, which has only {data.Graph[nb].Count} levels and therefore no level {l}.");
            }

            // Объявленный верх обязан РАВНЯТЬСЯ глобальному максимуму глубин.
            // Именно равняться: заниженный верх оставляет верхнее знание
            // глубокого узла непосещаемым, а завышенный означает уровень, до
            // которого не доживает ни один узел.
            if (data.MaxLevel != deepestLevel)
                throw new InvalidOperationException(
                    $"INDEX_MAX_LEVEL_NOT_GLOBAL: top level {data.MaxLevel} disagrees with the deepest stored node {deepestNode}, whose top level is {deepestLevel}.");
        }

        // FIX-RFD-01: последняя проверка перед созданием экземпляра — параметр
        // продолжения, а не состояние. Поле ширины построения хранится обычным
        // целым, поэтому отсутствующее поле даёт ноль при разборе, и прежняя
        // загрузка передавала этот ноль конструктору как есть. Проверка стоит
        // ЗДЕСЬ, после всех прежних проверок состояния, после двух новых
        // межузловых проверок и до создания экземпляра: порядок прежних
        // проверок не меняется, а непригодный указатель наружу не выдаётся.
        // Отказ называет поле и его фактическое значение, как тринадцать
        // соседних кодов этого же блока.
        if (data.EfConstruction <= 0)
            throw new InvalidOperationException(
                $"INDEX_CONSTRUCTION_BEAM_INVALID: construction beam {data.EfConstruction} is not positive, so the next add would find no neighbours and leave its node disconnected.");

        var idx = new HnswIndex(data.M, data.EfConstruction, data.EfSearch)
        {
            _entryPoint = data.EntryPoint,
            _maxLevel = data.MaxLevel,
            // The checked dimension travels with the instance, so the first query
            // after a load is held to the same shape as a freshly built index.
            _dimensions = dimensions
        };
        idx._vectors.AddRange(data.Vectors.Select(v => v.ToArray()));
        idx._ids.AddRange(data.Ids);
        // No defensive copy here, and that is deliberate. These dictionaries were
        // just built by the parser from the file's bytes, and the only reference to
        // them is the local above, which dies with this method. There is no caller
        // holding them and therefore nothing to be isolated from; a copy would cost
        // one allocation per row and protect against nobody. The accepting path is
        // the opposite case: there the array and the dictionary belong to a caller
        // that stays alive, so there the copy is mandatory.
        idx._meta.AddRange(data.Meta);
        foreach (var node in data.Graph)
        {
            var levels = node.Select(l => l.ToList()).ToList();
            idx._graph.Add(levels);
        }
        return idx;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private int RandomLevel()
    {
        double ml = 1.0 / Math.Log(_m);
        int l = 0;
        while (_rng.NextDouble() < Math.Exp(-1.0 / ml) && l < 32) l++;
        return l;
    }

    // FIX-HNQ transfer, plan 50-hnsw-training-query-norm.md, from HNQ01.
    //
    // The sum of the query's squared components does not depend on which
    // candidate it is being compared with, yet the shared cosine recomputed it
    // on every comparison. The three traversal methods below hold one query for
    // the whole call, so each of them now computes that sum once and hands it
    // to every comparison it makes. Nothing is remembered past the call: the
    // caller is free to change the array afterwards, and the next call starts
    // from a fresh sum.
    private static float QuerySquaredNorm(float[] query)
    {
        // Same accumulator type and same ascending order as the shared cosine,
        // so the sum is the identical float value down to its bits.
        float na = 0f;
        for (int i = 0; i < query.Length; i++)
            na += query[i] * query[i];
        return na;
    }

    // The shared cosine with the query's own sum supplied instead of recomputed.
    // Everything else — the dot product, the candidate's sum, the two square
    // roots, the guard against a vanishing denominator — stays exactly as it is
    // in Distances.CosineSimilarity, which remains the independent reference.
    private static float CosineWithQueryNorm(float[] query, float[] candidate, float querySquaredNorm)
    {
        float dot = 0f, nb = 0f;
        for (int i = 0; i < query.Length; i++)
        {
            dot += query[i] * candidate[i];
            nb  += candidate[i] * candidate[i];
        }
        float denom = MathF.Sqrt(querySquaredNorm) * MathF.Sqrt(nb);
        return denom < 1e-9f ? 0f : dot / denom;
    }

    private int GreedySearch(float[] query, int entry, int level)
    {
        // One sum for this descent; the query does not change inside it.
        float queryNorm = QuerySquaredNorm(query);
        int best = entry;
        float bestDist = CosineWithQueryNorm(query, _vectors[entry], queryNorm);
        bool changed = true;
        while (changed)
        {
            changed = false;
            if (level >= _graph[best].Count) break;
            foreach (int nb in _graph[best][level])
            {
                float d = CosineWithQueryNorm(query, _vectors[nb], queryNorm);
                if (d > bestDist) { bestDist = d; best = nb; changed = true; }
            }
        }
        return best;
    }

    // Descending by score; id breaks ties to ensure uniqueness in the set.
    private static readonly IComparer<(float score, int id)> ScoreDesc =
        Comparer<(float score, int id)>.Create((a, b) =>
        {
            int c = b.score.CompareTo(a.score);
            return c != 0 ? c : a.id.CompareTo(b.id);
        });

    private List<(int id, float score)> SearchLayer(float[] query, List<int> entries, int ef, int level)
    {
        // One sum for this whole beam search; the query is fixed for the call.
        float queryNorm = QuerySquaredNorm(query);
        var visited    = new HashSet<int>(entries);
        var candidates = new SortedSet<(float score, int id)>(ScoreDesc);
        var results    = new SortedSet<(float score, int id)>(ScoreDesc);

        foreach (int e in entries)
        {
            float d = CosineWithQueryNorm(query, _vectors[e], queryNorm);
            candidates.Add((d, e));
            results.Add((d, e));
        }

        while (candidates.Count > 0)
        {
            var best = candidates.Min;
            candidates.Remove(best);

            float worstResult = results.Count > 0 ? results.Max.score : float.MinValue;
            if (best.score < worstResult && results.Count >= ef) break;

            if (level < _graph[best.id].Count)
                foreach (int nb in _graph[best.id][level])
                {
                    if (!visited.Add(nb)) continue;
                    float d = CosineWithQueryNorm(query, _vectors[nb], queryNorm);
                    results.Add((d, nb));
                    candidates.Add((d, nb));
                    if (results.Count > ef) results.Remove(results.Max);
                }
        }

        return results.Take(ef).Select(t => (t.id, t.score)).ToList();
    }

    private List<int> SelectNeighbours(float[] query, List<int> candidates, int maxM)
    {
        // One sum for this ranking. The query here can be a stored vector from
        // the back-pruning path; no assumption is made about its length.
        float queryNorm = QuerySquaredNorm(query);
        return candidates
            .OrderByDescending(c => CosineWithQueryNorm(query, _vectors[c], queryNorm))
            .Take(maxM)
            .ToList();
    }

    private class SerializedIndex
    {
        public List<List<float>> Vectors { get; set; } = [];
        public List<string> Ids { get; set; } = [];
        public List<Dictionary<string, string>> Meta { get; set; } = [];
        public List<List<List<int>>> Graph { get; set; } = [];
        public int EntryPoint { get; set; }
        public int MaxLevel { get; set; }
        public int M { get; set; }
        public int EfConstruction { get; set; }
        public int EfSearch { get; set; }
    }
}
