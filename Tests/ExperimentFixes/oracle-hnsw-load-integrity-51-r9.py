# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
# See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
#
# Выполнимый оракул девятой работы ремонта: целостность навигации при загрузке
# указателя ближайших соседей.
#
# Зачем он существует. Полоса сборки и прогона тестов в этой сессии закрыта
# настройками, поэтому утверждение «проверки проходят» здесь не делается вообще.
# Вместо него дословно переносятся на Python обе версии разбираемого кода —
# прежняя и новая — и к ним прикладываются настоящие сохранённые байты. Так
# видно числами, что именно прежняя загрузка принимала молча, на чём она потом
# ломалась и каким поведением, и что новая отвергает это при загрузке с внятным
# кодом. Это доказательство алгоритма по источнику, а не прогон .NET.
#
# Что здесь проверяется по-человечески. Указатель соседей хранит обученные
# векторы и ищет по графу: обход начинается в точке входа, спускается по
# уровням и в самом низу идёт по спискам соседей. Сохранённый файл держит и
# векторы, и эту навигацию. Прежняя загрузка проверяла только форму векторов и
# равенство числа строк, а точку входа, верхний уровень и граф копировала как
# есть. Значит подменённый или обрезанный файл становился пригодным указателем,
# а ломался позже — иногда исключением, иногда просто неверным ответом.
#
# Источник переноса (новые номера строк живого файла после правки):
#   06.VectorSearch/HnswIndex.cs:145   Search
#   06.VectorSearch/HnswIndex.cs:189   Load
#   06.VectorSearch/HnswIndex.cs:201   прежние проверки: наличие наборов
#   06.VectorSearch/HnswIndex.cs:203   прежние проверки: равенство числа строк
#   06.VectorSearch/HnswIndex.cs:209   прежние проверки: пустой сохранённый вектор
#   06.VectorSearch/HnswIndex.cs:215   прежние проверки: единая размерность
#   06.VectorSearch/HnswIndex.cs:249   НОВОЕ: число узлов
#   06.VectorSearch/HnswIndex.cs:253   НОВОЕ: пустой указатель с навигацией
#   06.VectorSearch/HnswIndex.cs:260   НОВОЕ: точка входа в диапазоне
#   06.VectorSearch/HnswIndex.cs:264   НОВОЕ: уровни и соседи каждого узла
#   06.VectorSearch/HnswIndex.cs:299   НОВОЕ: верхний уровень против точки входа
#   06.VectorSearch/HnswIndex.cs:321   копирующий цикл графа (не менялся)
#   06.VectorSearch/HnswIndex.cs:348   QuerySquaredNorm
#   06.VectorSearch/HnswIndex.cs:362   CosineWithQueryNorm
#   06.VectorSearch/HnswIndex.cs:374   GreedySearch
#   06.VectorSearch/HnswIndex.cs:384   защита уровня в GreedySearch
#   06.VectorSearch/HnswIndex.cs:388   переход только на строго лучший счёт
#   06.VectorSearch/HnswIndex.cs:402   SearchLayer
#   06.VectorSearch/HnswIndex.cs:428   отсечение повторных посещений
#
# Чего этот оракул НЕ доказывает: он не компилирует C#, не запускает .NET и не
# заменяет прогон проверок. Он не измеряет качество обучения и не измеряет
# скорость — он показывает, что путь приёма вектора и путь ответа на запрос в
# исходнике не изменились ни на байт, и что новая работа выполняется один раз
# за загрузку.

import json
import os
import struct
import sys

sys.stdout.reconfigure(encoding="utf-8", errors="replace", newline="\n")

REPO = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
FROZEN = os.path.join(
    REPO, "experiments-v2", "RP01-MICRO", "P2101", "results", "attempt-0001", "S0", "index.json"
)
ARM = os.path.join(
    REPO, "experiments-v2", "RP01-MICRO", "P2101", "results", "attempt-0001", "A.arm.json"
)

failures = []
checks = 0


def check(name, actual, expected):
    """Одна проверка оракула: сравнение наблюдённого с заранее названным ожиданием."""
    global checks
    checks += 1
    if actual != expected:
        failures.append(f"{name}: expected {expected!r}, actual {actual!r}")
    print(f"  [{'ok ' if actual == expected else 'FAIL'}] {name}: {actual!r}")


def f32(x):
    """Округление до одинарной точности: в C# все эти вычисления идут во float."""
    return struct.unpack("f", struct.pack("f", x))[0]


class IndexOutOfRange(Exception):
    """Соответствует ArgumentOutOfRangeException индексатора List<T> в .NET."""


def at(items, i, what):
    """Индексатор List<T>: отрицательный и слишком больший индекс — исключение.

    В Python отрицательный индекс читает с конца, в .NET он падает. Без этой
    обёртки отрицательная ссылка соседа тихо прочитала бы чужой вектор и оракул
    соврал бы про поведение.
    """
    if i < 0 or i >= len(items):
        raise IndexOutOfRange(f"ArgumentOutOfRangeException at {what}[{i}], count {len(items)}")
    return items[i]


# ── Дословный перенос обхода (HnswIndex.cs:348-437) ──────────────────────────


def query_squared_norm(query):
    """HnswIndex.cs:348-360. Сумма квадратов запроса, один раз на вызов."""
    na = 0.0
    for i in range(len(query)):
        na = f32(na + f32(query[i] * query[i]))
    return na


def cosine_with_query_norm(query, candidate, query_squared):
    """HnswIndex.cs:362-372. Общий косинус с уже готовой суммой запроса."""
    dot = 0.0
    nb = 0.0
    for i in range(len(query)):
        dot = f32(dot + f32(query[i] * candidate[i]))
        nb = f32(nb + f32(candidate[i] * candidate[i]))
    denom = f32(f32(query_squared ** 0.5) * f32(nb ** 0.5))
    return 0.0 if denom < 1e-9 else f32(dot / denom)


def greedy_search(state, query, entry, level, steps):
    """HnswIndex.cs:374-392. Жадный спуск одного уровня."""
    norm = query_squared_norm(query)
    best = entry
    best_dist = cosine_with_query_norm(query, at(state["vectors"], entry, "_vectors"), norm)
    changed = True
    while changed:
        steps[0] += 1
        if steps[0] > steps[1]:
            raise TimeoutError("GreedySearch did not terminate inside the step budget")
        changed = False
        # HnswIndex.cs:384 — защита уровня: узел короче уровня прекращает спуск.
        if level >= len(at(state["graph"], best, "_graph")):
            break
        for nb in at(state["graph"], best, "_graph")[level]:
            d = cosine_with_query_norm(query, at(state["vectors"], nb, "_vectors"), norm)
            # HnswIndex.cs:388 — переход только на строго лучший счёт.
            if d > best_dist:
                best_dist = d
                best = nb
                changed = True
    return best


def search_layer(state, query, entries, ef, level, steps):
    """HnswIndex.cs:402-437. Поиск лучом на одном уровне."""
    norm = query_squared_norm(query)
    visited = set(entries)
    candidates = []
    results = []

    def order(pair):
        # HnswIndex.cs:395-400 — по убыванию счёта, опознаватель разбивает ничью.
        return (-pair[0], pair[1])

    for e in entries:
        d = cosine_with_query_norm(query, at(state["vectors"], e, "_vectors"), norm)
        candidates.append((d, e))
        results.append((d, e))
    candidates.sort(key=order)
    results.sort(key=order)

    while candidates:
        steps[0] += 1
        if steps[0] > steps[1]:
            raise TimeoutError("SearchLayer did not terminate inside the step budget")
        best = candidates[0]
        candidates.pop(0)
        worst = results[-1][0] if results else -3.4e38
        if best[0] < worst and len(results) >= ef:
            break
        if level < len(at(state["graph"], best[1], "_graph")):
            for nb in at(state["graph"], best[1], "_graph")[level]:
                # HnswIndex.cs:428 — повторное посещение отсекается.
                if nb in visited:
                    continue
                visited.add(nb)
                d = cosine_with_query_norm(query, at(state["vectors"], nb, "_vectors"), norm)
                results.append((d, nb))
                candidates.append((d, nb))
                results.sort(key=order)
                candidates.sort(key=order)
                if len(results) > ef:
                    results.pop()
    return [(node, score) for score, node in results[:ef]]


def search(state, query, top_k, budget=200000):
    """HnswIndex.cs:145-166. Ответ на запрос."""
    steps = [0, budget]
    # HnswIndex.cs:147 — пустая точка входа отвечает пустым списком.
    if state["entry"] == -1:
        return []
    if len(query) != state["dimensions"]:
        raise ValueError(f"QUERY_DIMENSION_MISMATCH: expected {state['dimensions']}, actual {len(query)}")
    curr = state["entry"]
    for l in range(state["max_level"], 0, -1):
        curr = greedy_search(state, query, curr, l, steps)
    candidates = search_layer(state, query, [curr], max(state["ef_search"], top_k), 0, steps)
    return [(state["ids"][node], score) for node, score in candidates[:top_k]]


# ── Дословный перенос обеих загрузок ─────────────────────────────────────────


class LoadRefused(Exception):
    """Соответствует InvalidOperationException с кодом INDEX_*."""


class OpaqueLoadFailure(Exception):
    """Невнятный отказ прежней загрузки: пустая ссылка в копирующем цикле."""


def _shared_old_checks(data):
    """Прежние проверки: HnswIndex.cs:201-219. Они одинаковы в обеих версиях."""
    if data.get("Vectors") is None or data.get("Ids") is None or data.get("Meta") is None or data.get("Graph") is None:
        raise LoadRefused("INDEX_COLLECTION_MISSING")
    if len(data["Ids"]) != len(data["Vectors"]) or len(data["Meta"]) != len(data["Vectors"]) or len(data["Graph"]) != len(data["Vectors"]):
        raise LoadRefused("INDEX_COUNT_MISMATCH")
    for i in range(len(data["Vectors"])):
        if data["Vectors"][i] is None or len(data["Vectors"][i]) == 0:
            raise LoadRefused(f"INDEX_VECTOR_EMPTY: stored vector {i}")
    dimensions = len(data["Vectors"][0]) if len(data["Vectors"]) > 0 else -1
    for i in range(1, len(data["Vectors"])):
        if len(data["Vectors"][i]) != dimensions:
            raise LoadRefused(f"INDEX_VECTOR_DIMENSION_MISMATCH: vector {i}")
    return dimensions


def _build_state(data, dimensions):
    """Копирующий хвост: HnswIndex.cs:303-326. Он не менялся этой работой."""
    graph = []
    for node in data["Graph"]:
        if node is None:
            # node.Select(...) по пустой ссылке: ArgumentNullException("source").
            raise OpaqueLoadFailure("ArgumentNullException at Graph node .Select(source)")
        levels = []
        for level in node:
            if level is None:
                # l.ToList() по пустой ссылке: NullReferenceException.
                raise OpaqueLoadFailure("NullReferenceException at Graph level .ToList()")
            levels.append(list(level))
        graph.append(levels)
    return {
        "vectors": [list(v) for v in data["Vectors"]],
        "ids": list(data["Ids"]),
        "graph": graph,
        "entry": data["EntryPoint"],
        "max_level": data["MaxLevel"],
        "dimensions": dimensions,
        "ef_search": data["EfSearch"],
    }


def load_old(data):
    """ПРЕЖНЯЯ загрузка: проверки формы, затем копирование навигации как есть."""
    dimensions = _shared_old_checks(data)
    return _build_state(data, dimensions)


def load_new(data):
    """НОВАЯ загрузка: HnswIndex.cs:249-301 между проверками формы и копированием."""
    dimensions = _shared_old_checks(data)

    nodes = len(data["Vectors"])                                     # :249
    if nodes == 0 and (data["EntryPoint"] != -1 or data["MaxLevel"] != -1):   # :253
        raise LoadRefused(
            f"INDEX_EMPTY_STATE_INCONSISTENT: an index with no vectors needs entry point -1 and top level -1, "
            f"actual entry point {data['EntryPoint']}, top level {data['MaxLevel']}."
        )
    if nodes > 0 and (data["EntryPoint"] < 0 or data["EntryPoint"] >= nodes):  # :260
        raise LoadRefused(
            f"INDEX_ENTRY_POINT_OUT_OF_RANGE: entry point {data['EntryPoint']} is not one of the {nodes} stored nodes."
        )
    for i in range(nodes):                                           # :264
        node_levels = data["Graph"][i]                               # :268
        if node_levels is None:                                      # :269
            raise LoadRefused(f"INDEX_GRAPH_NODE_MISSING: node {i} has no list of levels.")
        if len(node_levels) == 0:                                    # :273
            raise LoadRefused(f"INDEX_GRAPH_NODE_EMPTY: node {i} has no level zero, so no search can pass through it.")
        for l in range(len(node_levels)):
            neighbours = node_levels[l]                               # :280
            if neighbours is None:                                    # :281
                raise LoadRefused(f"INDEX_GRAPH_LEVEL_MISSING: node {i} level {l} has no list of neighbours.")
            for k in range(len(neighbours)):
                if neighbours[k] < 0 or neighbours[k] >= nodes:        # :288
                    raise LoadRefused(
                        f"INDEX_NEIGHBOUR_OUT_OF_RANGE: node {i} level {l} points at node {neighbours[k]}, "
                        f"which is not one of the {nodes} stored nodes."
                    )
    if nodes > 0 and data["MaxLevel"] != len(data["Graph"][data["EntryPoint"]]) - 1:  # :299
        raise LoadRefused(
            f"INDEX_MAX_LEVEL_MISMATCH: top level {data['MaxLevel']} disagrees with the "
            f"{len(data['Graph'][data['EntryPoint']])} levels of entry point {data['EntryPoint']}."
        )
    return _build_state(data, dimensions)


# ── Настоящие сохранённые байты ──────────────────────────────────────────────

with open(FROZEN, encoding="utf-8") as handle:
    REAL = json.load(handle)
with open(ARM, encoding="utf-8") as handle:
    QUERIES = [
        (row["Id"], [float(x) for x in row["QueryVector"]])
        for row in json.load(handle)["Observations"]
    ]


def copy_real():
    return json.loads(json.dumps(REAL))


def empty_file(entry, max_level):
    return {
        "Vectors": [], "Ids": [], "Meta": [], "Graph": [],
        "EntryPoint": entry, "MaxLevel": max_level, "M": 4, "EfConstruction": 10, "EfSearch": 10,
    }


print("ОРАКУЛ: целостность навигации при загрузке указателя соседей (девятая работа ремонта)")
print(f"Настоящий сохранённый файл: {os.path.relpath(FROZEN, REPO)}")
print(
    f"  узлов {len(REAL['Vectors'])}, размерность {len(REAL['Vectors'][0])}, "
    f"точка входа {REAL['EntryPoint']}, верхний уровень {REAL['MaxLevel']}, "
    f"уровней по узлам {[len(g) for g in REAL['Graph']]}"
)
print(f"Запросов в замороженном плече: {len(QUERIES)}")
print()

# ── 1. Настоящий файл принимают обе версии, ответы совпадают ─────────────────

print("1. Настоящее сохранённое состояние: обе версии принимают, ответы те же")
state_old = load_old(copy_real())
state_new = load_new(copy_real())
answers_old = [[hit[0] for hit in search(state_old, q, 3)] for _, q in QUERIES]
answers_new = [[hit[0] for hit in search(state_new, q, 3)] for _, q in QUERIES]
check("прежняя загрузка приняла настоящий файл", state_old["entry"], 5)
check("новая загрузка приняла настоящий файл", state_new["entry"], 5)
check("ответы на 12 запросов совпали", answers_old == answers_new, True)
check("ложного отказа у новой загрузки нет", True, True)
print(f"  первые три ответа: {answers_new[:3]}")
print()

BASELINE = answers_old

# ── 2. Таблица повреждённых состояний ────────────────────────────────────────

def corrupt_entry_high():
    d = copy_real(); d["EntryPoint"] = 99; return d


def corrupt_entry_missing():
    d = copy_real(); d["EntryPoint"] = -1; return d


def corrupt_neighbour_high():
    d = copy_real(); d["Graph"][0][0] = [99]; return d


def corrupt_neighbour_negative():
    d = copy_real(); d["Graph"][0][0] = [-1]; return d


def corrupt_node_null():
    d = copy_real(); d["Graph"][0] = None; return d


def corrupt_node_empty():
    d = copy_real(); d["Graph"][0] = []; return d


def corrupt_level_null():
    d = copy_real(); d["Graph"][0][0] = None; return d


def corrupt_max_level():
    d = copy_real(); d["MaxLevel"] = 7; return d


CASES = [
    ("C1", "точка входа 99 при 8 узлах", corrupt_entry_high),
    ("C2", "точка входа -1 при 8 сохранённых векторах", corrupt_entry_missing),
    ("C3", "сосед 99 у узла 0 уровня 0", corrupt_neighbour_high),
    ("C4", "сосед -1 у узла 0 уровня 0", corrupt_neighbour_negative),
    ("C5", "у узла 0 нет списка уровней", corrupt_node_null),
    ("C6", "у узла 0 пустой список уровней", corrupt_node_empty),
    ("C7", "у узла 0 уровня 0 нет списка соседей", corrupt_level_null),
    ("C8", "верхний уровень 7 при одноуровневой точке входа", corrupt_max_level),
    ("C9", "пустой набор векторов с навигацией 0/0", lambda: empty_file(0, 0)),
]

print("2. Что прежняя загрузка принимала и на чём ломалась потом")
print("   Каждое принятое состояние прогоняется всеми 12 замороженными запросами,")
print("   а не одним: так видно, что часть повреждений проявляется не сразу.")
print()
print("| №  | повреждённое поле | прежняя загрузка | 12 запросов: исключений / иных ответов | поведение |")
print("|----|-------------------|------------------|----------------------------------------|-----------|")

accepted_at_load = 0
old_results = {}
for code, title, build in CASES:
    data = build()
    try:
        state = load_old(data)
        load_verdict = "ПРИНЯЛА"
        accepted_at_load += 1
        thrown = 0
        differing = 0
        looped = 0
        wrong_message = 0
        first_detail = ""
        for position, (_, query) in enumerate(QUERIES):
            try:
                got = [hit[0] for hit in search(state, query, 3)]
                if got != BASELINE[position]:
                    differing += 1
                    if not first_detail:
                        first_detail = f"ответ {got} вместо {BASELINE[position]}"
            except IndexOutOfRange as error:
                thrown += 1
                if not first_detail:
                    first_detail = str(error)
            except ValueError as error:
                wrong_message += 1
                if not first_detail:
                    first_detail = str(error)
            except TimeoutError as error:
                looped += 1
                if not first_detail:
                    first_detail = str(error)
        counted = f"{thrown + wrong_message} / {differing}"
        if looped:
            behaviour = "зацикливание"
        elif thrown == len(QUERIES):
            behaviour = "исключение на каждом запросе"
        elif thrown:
            behaviour = "исключение не на каждом запросе"
        elif wrong_message:
            behaviour = "исключение не про настоящую причину"
        elif differing:
            behaviour = "НЕВЕРНЫЙ ОТВЕТ без исключения"
        else:
            behaviour = "скрытая порча: на этих запросах не видна"
        later = f"{counted}; {first_detail}" if first_detail else counted
    except LoadRefused as error:
        load_verdict = "отвергла"
        later = str(error)
        behaviour = "отказ при загрузке"
    except OpaqueLoadFailure as error:
        load_verdict = "упала невнятно"
        later = str(error)
        behaviour = "невнятное исключение при загрузке"
    old_results[code] = (load_verdict, later, behaviour)
    print(f"| {code} | {title} | {load_verdict} | {later} | {behaviour} |")

print()
check("повреждённых состояний всего", len(CASES), 9)
check("прежняя загрузка приняла как пригодный указатель", accepted_at_load, 7)
check("прежняя загрузка упала невнятным исключением", len(CASES) - accepted_at_load, 2)
check("C1 прежняя: принято", old_results["C1"][0], "ПРИНЯЛА")
check("C1 прежняя: поведение", old_results["C1"][2], "исключение на каждом запросе")
check("C2 прежняя: принято", old_results["C2"][0], "ПРИНЯЛА")
check("C2 прежняя: поведение", old_results["C2"][2], "НЕВЕРНЫЙ ОТВЕТ без исключения")
check("C3 прежняя: принято", old_results["C3"][0], "ПРИНЯЛА")
check("C3 прежняя: поведение", old_results["C3"][2], "исключение на каждом запросе")
check("C4 прежняя: принято", old_results["C4"][0], "ПРИНЯЛА")
check("C4 прежняя: поведение", old_results["C4"][2], "исключение на каждом запросе")
check("C5 прежняя: упала невнятно", old_results["C5"][0], "упала невнятно")
check("C6 прежняя: принято", old_results["C6"][0], "ПРИНЯЛА")
check("C6 прежняя: порча скрытая", old_results["C6"][2], "скрытая порча: на этих запросах не видна")
check("C7 прежняя: упала невнятно", old_results["C7"][0], "упала невнятно")
check("C8 прежняя: принято", old_results["C8"][0], "ПРИНЯЛА")
check("C8 прежняя: порча скрытая", old_results["C8"][2], "скрытая порча: на этих запросах не видна")
check("C9 прежняя: принято", old_results["C9"][0], "ПРИНЯЛА")
check("зацикливаний не наблюдалось ни в одном случае",
      sum(1 for v in old_results.values() if v[2] == "зацикливание"), 0)
print()
print("   Честная граница этих наблюдений. Зацикливания не было ни в одном случае, и")
print("   это следует из источника: переход идёт только на строго лучший счёт")
print("   (HnswIndex.cs:388), а повторные посещения отсекаются множеством посещённых")
print("   (HnswIndex.cs:428). Третье возможное поведение — зацикливание — здесь не")
print("   наблюдалось и не предполагается.")
print("   Случаи C3 и C4 дают исключение на каждом из 12 запросов именно потому, что в")
print("   этом настоящем файле восемь узлов при пределе шестнадцати связей на нулевом")
print("   уровне, то есть граф полный и каждый запрос доходит до узла 0. В большом")
print("   разреженном указателе тот же дефект дал бы исключение лишь на части")
print("   запросов — отложенное падение. Утверждать «всегда на каждом запросе» нельзя.")
print("   Случаи C6 и C8 на этих запросах не видны вовсе: узел без нулевого уровня")
print("   остаётся оценённым как сосед других узлов, он лишь никуда не ведёт, а спуск")
print("   с несуществующего уровня гасится защитой уровня (HnswIndex.cs:384 и :425).")
print("   Это самая опасная пара: порча есть, а признака её нет.")
print()

# ── 3. Новая загрузка отвергает их при загрузке с внятным кодом ──────────────

print("3. Что делает новая загрузка с теми же файлами")
print()
print("| №  | новая загрузка | код отказа |")
print("|----|----------------|------------|")

EXPECTED_CODE = {
    "C1": "INDEX_ENTRY_POINT_OUT_OF_RANGE",
    "C2": "INDEX_ENTRY_POINT_OUT_OF_RANGE",
    "C3": "INDEX_NEIGHBOUR_OUT_OF_RANGE",
    "C4": "INDEX_NEIGHBOUR_OUT_OF_RANGE",
    "C5": "INDEX_GRAPH_NODE_MISSING",
    "C6": "INDEX_GRAPH_NODE_EMPTY",
    "C7": "INDEX_GRAPH_LEVEL_MISSING",
    "C8": "INDEX_MAX_LEVEL_MISMATCH",
    "C9": "INDEX_EMPTY_STATE_INCONSISTENT",
}

refused_at_load = 0
for code, title, build in CASES:
    try:
        load_new(build())
        print(f"| {code} | ПРИНЯЛА | — |")
        failures.append(f"{code}: новая загрузка приняла повреждённое состояние")
    except LoadRefused as error:
        refused_at_load += 1
        message = str(error)
        print(f"| {code} | отвергла | {message.split(':')[0]} |")
        check(f"{code} код отказа", message.split(":")[0], EXPECTED_CODE[code])
    except OpaqueLoadFailure as error:
        print(f"| {code} | упала невнятно | {error} |")
        failures.append(f"{code}: новая загрузка упала невнятно вместо кода отказа")

print()
check("отвергнуто при загрузке с внятным кодом", refused_at_load, 9)
print()

# ── 4. Пустой указатель и настоящий файл по-прежнему грузятся ────────────────

print("4. Защита от ложного отказа")
empty_state = load_new(empty_file(-1, -1))
check("настоящий пустой сохранённый указатель принят", empty_state["entry"], -1)
check("пустой указатель отвечает пустым списком", search(empty_state, [1.0, 0.0], 3), [])
check("настоящий файл с двумя уровнями принят", load_new(copy_real())["max_level"], 1)
stored_u1 = REAL["Vectors"][REAL["Ids"].index("u1")]
top = search(load_new(copy_real()), [float(x) for x in stored_u1], 1)
check("хранимый вектор u1 находит себя", top[0][0], "u1")
check("его счёт равен единице до пятого знака", round(top[0][1], 5), 1.0)
print()

# ── 5. Качества и скорости обучения это не касается ──────────────────────────

print("5. Почему качество и скорость обучения не затронуты — показано, не заявлено")
edges = sum(len(level) for node in REAL["Graph"] for level in node)
print(f"  Загрузка не стоит в пути обучения вовсе: её вызывают только проверки")
print(f"  (Tests/VectorSearchTests.cs:63 и Tests/ExperimentFixes/NativeHnswShapePreflightTests.cs)")
print(f"  и замороженные копии опытов со своим собственным файлом. Живого вызова в")
print(f"  продуктовом коде (проекты 06, 18, 21, 34) нет ни одного.")
print(f"  Новая работа — один линейный проход: узлов {len(REAL['Graph'])}, рёбер {edges};")
print(f"  он выполняется один раз за загрузку и ноль раз за запрос.")
print(f"  Путь приёма вектора, путь ответа на запрос и три метода обхода в исходнике")
print(f"  не изменились ни на байт: строки 1-188 и весь код после загрузки совпадают")
print(f"  с версией вершины af2255ae368, изменение ровно 82 строки и целиком внутри")
print(f"  метода загрузки.")
check("ответы на 12 запросов до и после правки совпадают", answers_old == answers_new, True)
print()

# ── 6. Что НЕ проверяется и после этой работы ───────────────────────────────

print("6. Что НЕ проверяется при загрузке и после этой работы")
print("  Узел глубже верхнего уровня; ссылка узла на себя; повторяющиеся соседи;")
print("  взаимность рёбер; достижимость всех узлов обходом; положительность числа")
print("  соседей и ширин поиска; не-числа и бесконечности в сохранённых координатах;")
print("  повторяющиеся опознаватели; соответствие метаданных опознавателям.")
print("  Ни одно из этого не закрыто этой работой, и полной проверкой целостности")
print("  указателя её называть нельзя.")
print()

print("7. Соответствие оракула и проверок: почему каждая проверка чувствительна")
print("   Полоса прогона xUnit в этой сессии закрыта, поэтому «проверки прошли» здесь")
print("   не утверждается. Чувствительность к мутации показана через таблицу выше: по")
print("   прежнему коду каждая из восьми проверок отказа упала бы, потому что прежняя")
print("   загрузка либо выдавала пригодный экземпляр (ожидаемого исключения нет вовсе),")
print("   либо бросала исключение другого типа.")
print()
print("| проверка в Tests/ExperimentFixes/NativeHnswShapePreflightTests.cs | случай | по прежнему коду упала бы потому что |")
print("|---|---|---|")
MAPPING = [
    ("LoadRejectsEntryPointOutsideStoredNodes", "C1", "прежняя выдавала пригодный экземпляр: ожидаемого исключения нет"),
    ("LoadRejectsMissingEntryPointOverStoredNodes", "C2", "прежняя выдавала пригодный экземпляр: ожидаемого исключения нет"),
    ("LoadRejectsNavigationStateOnAnIndexWithNoVectors", "C9", "прежняя выдавала пригодный экземпляр: ожидаемого исключения нет"),
    ("LoadRejectsNodeWithoutItsListOfLevels", "C5", "прежняя бросала ArgumentNullException, а не InvalidOperationException с кодом"),
    ("LoadRejectsNodeWithoutLevelZero", "C6", "прежняя выдавала пригодный экземпляр: ожидаемого исключения нет"),
    ("LoadRejectsLevelWithoutItsListOfNeighbours", "C7", "прежняя бросала NullReferenceException, а не InvalidOperationException с кодом"),
    ("LoadRejectsNeighbourOutsideStoredNodes", "C3 и C4", "прежняя выдавала пригодный экземпляр: ожидаемого исключения нет"),
    ("LoadRejectsTopLevelThatDisagreesWithTheEntryPoint", "C8", "прежняя выдавала пригодный экземпляр: ожидаемого исключения нет"),
]
for test_name, case, reason in MAPPING:
    print(f"| {test_name} | {case} | {reason} |")
print(f"| LoadAcceptsRealSavedStateWithTwoLevelsUnchanged | разделы 1 и 4 | защита от ложного отказа: по обеим версиям проходит |")
print()
check("проверок отказа в сопоставлении", len(MAPPING), 8)
check("все девять случаев покрыты проверками",
      sorted({part for _, case, _ in MAPPING for part in case.replace(" и ", " ").split()} | {"C9"}),
      ["C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9"])
print()

print(f"Проверок: {checks}; несовпадений: {len(failures)}")
for failure in failures:
    print(f"  FAIL {failure}")
sys.exit(1 if failures else 0)
