# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
# See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
#
# Проверяемая программа девятой работы ремонта: две оставшиеся межузловые
# проверки загрузки сохранённого указателя соседей (уточнённый договор ремонта
# 114-post88-addressed-repair-plan-2026-11-01, раздел согласованного графа;
# опубликованный аудит границ исполнения 116-post89-execution-boundary-audit-
# 2026-11-01, раздел про два отдельных договора загрузки).
#
# Что проверяется по-человечески. Сохранённый указатель соседей хранит граф по
# уровням: нижний уровень связывает все узлы, верхние — редкие, для быстрого
# спуска. Два межузловых согласования прежде не проверялись: что сосед,
# названный на уровне l, сам доживает до уровня l, и что объявленный верхний
# уровень равен глубине самого глубокого узла, а не только точки входа.
#
# Почему оба дефекта тихие, и это измерено, а не предположено. Обе ветви
# навигации перед обходом соседей спрашивают, есть ли у текущего узла нужный
# уровень: спуск обрывается, луч поиска молча не разворачивает узел. Поэтому
# повреждение не падает ошибкой, а укорачивает навигацию. Опубликованный аудит
# показал последствие прямо: обе испорченные копии загрузкой принимались, и при
# этом все двенадцать проверочных запросов возвращали ожидаемые опознаватели.
# Панель точности к структурному дефекту нечувствительна — это сверяется ниже
# по его опубликованной улике, а не пересказывается.
#
# Способ доказательства. Вся цепочка проверок загрузки перенесена сюда
# ДОСЛОВНО и прогоняется на настоящем замороженном состоянии двадцать первого
# опыта, на двух его испорченных копиях и на одиннадцати прежних неверных
# входах. Это модель одного метода, а не весь механизм: навигация и поиск сюда
# не переносились.
#
# Бюджет: сборок проекта ноль, прогонов площадки ноль, обучения ноль, добавлений
# ноль. Одна внутрипроцессная компиляция оболочкой для переноса логики — тот же
# способ, что в прежних принятых проверяемых программах этой папки. Файлы опыта
# не редактируются: испорченные копии строятся в памяти.

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
if ($null -ne $PSStyle) { $PSStyle.OutputRendering = 'PlainText' }

$checks = 0
$failures = 0
# Признак того, что шестая панель не исполнилась из-за недоступности чужой
# опубликованной улики. Он печатается в итоге, чтобы недоступность была видна,
# а не растворилась в числе пройденных сверок.
$panel6NotExecuted = $false

function Check([string]$what, $expected, $actual) {
    $script:checks++
    $shownExpected = if ("$expected" -eq '') { 'принято без отказа' } else { "$expected" }
    $shownActual = if ("$actual" -eq '') { 'принято без отказа' } else { "$actual" }
    if ("$expected" -eq "$actual") {
        Write-Output ("  ок     {0} — ожидалось {1}, получено {2}" -f $what, $shownExpected, $shownActual)
    } else {
        $script:failures++
        Write-Output ("  УПАЛО  {0} — ожидалось {1}, получено {2}" -f $what, $shownExpected, $shownActual)
    }
}

# ── Дословный перенос цепочки проверок загрузки ─────────────────────────────
$refRoot = Get-ChildItem -Path 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref' -Directory |
    Where-Object { $_.Name -like '9.*' } | Sort-Object Name -Descending | Select-Object -First 1
if ($null -eq $refRoot) { throw 'Эталонные сборки платформы 9 не найдены' }
$refDir = Join-Path $refRoot.FullName 'ref\net9.0'
$refs = @('System.Runtime.dll', 'System.Text.Json.dll', 'System.Collections.dll', 'System.Linq.dll', 'netstandard.dll') |
    ForEach-Object { Join-Path $refDir $_ }

$transfer = @'
using System;
using System.Collections.Generic;
using System.Text.Json;

public static class LoadTransfer
{
    // Тот же набор полей, что у приватного сохранённого вида указателя.
    public sealed class Saved
    {
        public List<List<float>> Vectors { get; set; }
        public List<string> Ids { get; set; }
        public List<Dictionary<string, string>> Meta { get; set; }
        public List<List<List<int>>> Graph { get; set; }
        public int EntryPoint { get; set; }
        public int MaxLevel { get; set; }
        public int M { get; set; }
        public int EfConstruction { get; set; }
        public int EfSearch { get; set; }
    }

    // mode 0 — дословный перенос;
    // mode 1 — испорчен порог проверки уровня соседа на единицу (строгое меньше);
    // mode 2 — у глобальной проверки оставлена только половина «верх завышен».
    public static string Check(string json, int mode)
    {
        Saved data;
        try { data = JsonSerializer.Deserialize<Saved>(json); }
        catch (JsonException) { return "PARSE_FAILED"; }
        if (data == null) return "Failed to deserialize HNSW index";

        if (data.Vectors == null || data.Ids == null || data.Meta == null || data.Graph == null)
            return "INDEX_COLLECTION_MISSING";
        if (data.Ids.Count != data.Vectors.Count || data.Meta.Count != data.Vectors.Count || data.Graph.Count != data.Vectors.Count)
            return "INDEX_COUNT_MISMATCH";

        for (int i = 0; i < data.Vectors.Count; i++)
            if (data.Vectors[i] == null || data.Vectors[i].Count == 0)
                return "INDEX_VECTOR_EMPTY: stored vector " + i;

        int dimensions = data.Vectors.Count > 0 ? data.Vectors[0].Count : -1;
        for (int i = 1; i < data.Vectors.Count; i++)
            if (data.Vectors[i].Count != dimensions)
                return "INDEX_VECTOR_DIMENSION_MISMATCH";

        int nodes = data.Vectors.Count;

        if (nodes == 0 && (data.EntryPoint != -1 || data.MaxLevel != -1))
            return "INDEX_EMPTY_STATE_INCONSISTENT";

        if (nodes > 0 && (data.EntryPoint < 0 || data.EntryPoint >= nodes))
            return "INDEX_ENTRY_POINT_OUT_OF_RANGE: entry point " + data.EntryPoint;

        for (int i = 0; i < nodes; i++)
        {
            var nodeLevels = data.Graph[i];
            if (nodeLevels == null) return "INDEX_GRAPH_NODE_MISSING: node " + i;
            if (nodeLevels.Count == 0) return "INDEX_GRAPH_NODE_EMPTY: node " + i;

            for (int l = 0; l < nodeLevels.Count; l++)
            {
                var neighbours = nodeLevels[l];
                if (neighbours == null) return "INDEX_GRAPH_LEVEL_MISSING: node " + i + " level " + l;

                for (int k = 0; k < neighbours.Count; k++)
                    if (neighbours[k] < 0 || neighbours[k] >= nodes)
                        return "INDEX_NEIGHBOUR_OUT_OF_RANGE: node " + i + " level " + l + " points at node " + neighbours[k];
            }
        }

        if (nodes > 0 && data.MaxLevel != data.Graph[data.EntryPoint].Count - 1)
            return "INDEX_MAX_LEVEL_MISMATCH: top level " + data.MaxLevel;

        // Две новые межузловые проверки, перенесённые дословно.
        if (nodes > 0)
        {
            int deepestNode = 0;
            int deepestLevel = data.Graph[0].Count - 1;

            for (int i = 0; i < nodes; i++)
            {
                int nodeTop = data.Graph[i].Count - 1;
                if (nodeTop > deepestLevel) { deepestLevel = nodeTop; deepestNode = i; }

                for (int l = 0; l < data.Graph[i].Count; l++)
                    foreach (int nb in data.Graph[i][l])
                    {
                        bool violates = mode == 1
                            ? data.Graph[nb].Count < l      // испорчено: порог сдвинут на единицу
                            : data.Graph[nb].Count <= l;    // дословно
                        if (violates)
                            return "INDEX_NEIGHBOUR_LEVEL_MISSING: node " + i + " level " + l + " points at node " + nb
                                 + ", which has only " + data.Graph[nb].Count + " levels and therefore no level " + l;
                    }
            }

            bool globalBad = mode == 2
                ? data.MaxLevel > deepestLevel      // испорчено: только завышенный верх
                : data.MaxLevel != deepestLevel;    // дословно: равенство
            if (globalBad)
                return "INDEX_MAX_LEVEL_NOT_GLOBAL: top level " + data.MaxLevel
                     + " disagrees with the deepest stored node " + deepestNode
                     + ", whose top level is " + deepestLevel;
        }

        if (data.EfConstruction <= 0)
            return "INDEX_CONSTRUCTION_BEAM_INVALID: construction beam " + data.EfConstruction;

        return string.Empty;
    }
}
'@

Add-Type -TypeDefinition $transfer -ReferencedAssemblies $refs -IgnoreWarnings -WarningAction SilentlyContinue

function Verdict([string]$json, [int]$mode = 0) { return [LoadTransfer]::Check($json, $mode) }

# ── Исходные данные: настоящее замороженное состояние ───────────────────────
$repoRoot = 'C:\Projects\llm_mvp_core'
$frozenPath = Join-Path $repoRoot 'experiments-v2\RP01-MICRO\P2101\results\attempt-0001\S0\index.json'
$frozenBytes = [System.IO.File]::ReadAllBytes($frozenPath)
$frozenSha = ([System.Security.Cryptography.SHA256]::HashData($frozenBytes) |
    ForEach-Object { $_.ToString('x2') }) -join ''
$frozenJson = [System.Text.Encoding]::UTF8.GetString($frozenBytes)

Write-Output "Проверяемая программа: две межузловые проверки загрузки указателя соседей."
Write-Output ("Замороженное состояние: " + $frozenPath)
Write-Output ("Отпечаток прочитанных байт: " + $frozenSha)
Check "отпечаток замороженного состояния тот самый" "f9fc76519f366d13e81925e77c9e47966beb934bf948feea9d67726a29544c4f" $frozenSha
Write-Output ""

# ── Панель 1. Измеренная форма замороженного состояния ─────────────────────
Write-Output "Панель 1. Что на самом деле лежит в замороженном состоянии."
$frozen = $frozenJson | ConvertFrom-Json
$levelTops = @($frozen.Graph | ForEach-Object { $_.Count - 1 })
$globalMax = ($levelTops | Measure-Object -Maximum).Maximum
Write-Output ("  узлов: " + $levelTops.Count + "; точка входа: " + $frozen.EntryPoint + "; объявленный верх: " + $frozen.MaxLevel)
Write-Output ("  глубина каждого узла: " + ($levelTops -join ','))
Write-Output ("  глобальный максимум глубин: " + $globalMax + "; глубина точки входа: " + $levelTops[$frozen.EntryPoint])
$violating = 0
for ($i = 0; $i -lt $frozen.Graph.Count; $i++) {
    for ($l = 0; $l -lt $frozen.Graph[$i].Count; $l++) {
        foreach ($nb in @($frozen.Graph[$i][$l])) {
            if ($frozen.Graph[$nb].Count -le $l) { $violating++ }
        }
    }
}
Write-Output ("  рёбер, чей сосед не доживает до уровня ссылки: " + $violating)
Write-Output ("  нулевой уровень связывает узлов с каждым: " + @($frozen.Graph[0][0]).Count + " из " + ($levelTops.Count - 1))
Check "глобальный максимум равен объявленному верху" $frozen.MaxLevel $globalMax
Check "нарушающих рёбер в исправном состоянии нет" 0 $violating
Check "исправное замороженное состояние принимается" "" (Verdict $frozenJson)
Write-Output ""
Write-Output "  Пояснение к слепоте панели точности: нулевой уровень здесь полный —"
Write-Output "  каждый узел связан со всеми остальными. Поэтому поиск находит верный"
Write-Output "  ответ перебором нижнего уровня независимо от повреждений наверху."
Write-Output ""

# ── Панель 2. Две испорченные копии ────────────────────────────────────────
Write-Output "Панель 2. Две испорченные копии: ровно одно изменение в каждой."

function Edit-FrozenCopy([scriptblock]$mutate) {
    # Копия строится разбором и обратной записью в памяти: файл опыта не трогается.
    $copy = $frozenJson | ConvertFrom-Json
    & $mutate $copy
    return ($copy | ConvertTo-Json -Depth 12 -Compress)
}

# Копия A: ребро верхнего уровня ведёт к узлу, у которого этого уровня нет.
$copyA = Edit-FrozenCopy { param($c) $c.Graph[5][1] = @(0) }
$verdictA = Verdict $copyA
Write-Output ("  копия A, ребро верхнего уровня на узел без этого уровня -> " + $(if ($verdictA -eq '') { 'ПРИНЯТО' } else { $verdictA }))
Check "копия A отвергается своим кодом" "INDEX_NEIGHBOUR_LEVEL_MISSING: node 5 level 1 points at node 0, which has only 1 levels and therefore no level 1" $verdictA

# Копия B: не точка входа оказывается глубже объявленного верха.
$copyB = Edit-FrozenCopy { param($c) $c.Graph[0] = @($c.Graph[0][0], @(), @()) }
$verdictB = Verdict $copyB
Write-Output ("  копия B, узел глубже объявленного верха -> " + $(if ($verdictB -eq '') { 'ПРИНЯТО' } else { $verdictB }))
Check "копия B отвергается своим кодом" "INDEX_MAX_LEVEL_NOT_GLOBAL: top level 1 disagrees with the deepest stored node 0, whose top level is 2" $verdictB
Write-Output ""

# ── Панель 3. Прежние одиннадцать отказов не ослаблены и не переименованы ──
Write-Output "Панель 3. Одиннадцать прежних отрицательных случаев: тот же код, что был."

function Saved([string]$vectors, [string]$ids, [string]$meta, [string]$graph, [int]$entryPoint = 0, [int]$maxLevel = 0) {
    return '{"Vectors":' + $vectors + ',"Ids":' + $ids + ',"Meta":' + $meta + ',"Graph":' + $graph +
        ',"EntryPoint":' + $entryPoint + ',"MaxLevel":' + $maxLevel + ',"M":4,"EfConstruction":10,"EfSearch":10}'
}

$twoNodes = '[[1.0,0.0],[0.0,1.0]]'
$twoIds = '["a","b"]'
$twoMeta = '[{},{}]'
$twoGraph = '[[[1]],[[0]]]'

$previous = @(
    @{ n = 'набор векторов отсутствует'; j = (Saved 'null' '[]' '[]' '[]'); c = 'INDEX_COLLECTION_MISSING' },
    @{ n = 'разошлись счётчики наборов'; j = (Saved $twoNodes '["a"]' $twoMeta $twoGraph); c = 'INDEX_COUNT_MISMATCH' },
    @{ n = 'сохранённый вектор без составляющих'; j = (Saved '[[]]' '["a"]' '[{}]' '[[[]]]'); c = 'INDEX_VECTOR_EMPTY: stored vector 0' },
    @{ n = 'сохранённый вектор отсутствует'; j = (Saved '[[1.0,0.0],null]' $twoIds $twoMeta $twoGraph); c = 'INDEX_VECTOR_EMPTY: stored vector 1' },
    @{ n = 'разные длины сохранённых векторов'; j = (Saved '[[1.0,0.0],[0.0,1.0,0.0]]' $twoIds $twoMeta $twoGraph); c = 'INDEX_VECTOR_DIMENSION_MISMATCH' },
    @{ n = 'навигация при пустом наборе векторов'; j = (Saved '[]' '[]' '[]' '[]' 0 0); c = 'INDEX_EMPTY_STATE_INCONSISTENT' },
    @{ n = 'точка входа вне числа узлов'; j = (Saved $twoNodes $twoIds $twoMeta $twoGraph 7 0); c = 'INDEX_ENTRY_POINT_OUT_OF_RANGE: entry point 7' },
    @{ n = 'точки входа нет при сохранённых векторах'; j = (Saved $twoNodes $twoIds $twoMeta $twoGraph -1 0); c = 'INDEX_ENTRY_POINT_OUT_OF_RANGE: entry point -1' },
    @{ n = 'узел без списка уровней'; j = (Saved $twoNodes $twoIds $twoMeta '[null,[[0]]]'); c = 'INDEX_GRAPH_NODE_MISSING: node 0' },
    @{ n = 'узел без нулевого уровня'; j = (Saved $twoNodes $twoIds $twoMeta '[[],[[0]]]'); c = 'INDEX_GRAPH_NODE_EMPTY: node 0' },
    @{ n = 'уровень без списка соседей'; j = (Saved $twoNodes $twoIds $twoMeta '[[null],[[0]]]'); c = 'INDEX_GRAPH_LEVEL_MISSING: node 0 level 0' },
    @{ n = 'сосед вне числа узлов'; j = (Saved $twoNodes $twoIds $twoMeta '[[[5]],[[0]]]'); c = 'INDEX_NEIGHBOUR_OUT_OF_RANGE: node 0 level 0 points at node 5' },
    @{ n = 'отрицательная ссылка на соседа'; j = (Saved $twoNodes $twoIds $twoMeta '[[[-1]],[[0]]]'); c = 'INDEX_NEIGHBOUR_OUT_OF_RANGE: node 0 level 0 points at node -1' },
    @{ n = 'верх расходится с точкой входа'; j = (Saved $twoNodes $twoIds $twoMeta $twoGraph 0 3); c = 'INDEX_MAX_LEVEL_MISMATCH: top level 3' }
)

$preservedCodes = @()
foreach ($row in $previous) {
    $verdict = Verdict $row.j
    $preservedCodes += ($verdict -split ':')[0]
    Check ("прежний случай: " + $row.n) $row.c $verdict
}
$distinctPrevious = @($preservedCodes | Sort-Object -Unique)
Write-Output ("  различных прежних кодов отказа сработало: " + $distinctPrevious.Count + " — " + ($distinctPrevious -join ', '))
Check "прежних различных кодов ровно одиннадцать" 11 $distinctPrevious.Count
Write-Output ""

# ── Панель 4. Положительные контроли пустого и одноузлового указателя ──────
Write-Output "Панель 4. Положительные контроли: пустой и одноузловой указатели."
$emptyIndex = Saved '[]' '[]' '[]' '[]' -1 -1
Check "настоящий пустой указатель принимается" "" (Verdict $emptyIndex)
$oneNodeFlat = Saved '[[1.0,0.0]]' '["a"]' '[{}]' '[[[]]]' 0 0
Check "одноузловой указатель с одним уровнем принимается" "" (Verdict $oneNodeFlat)
$oneNodeTall = Saved '[[1.0,0.0]]' '["a"]' '[{}]' '[[[],[]]]' 0 1
Check "одноузловой указатель с двумя уровнями принимается" "" (Verdict $oneNodeTall)
$oneNodeSelf = Saved '[[1.0,0.0]]' '["a"]' '[{}]' '[[[],[0]]]' 0 1
Check "одноузловой указатель со ссылкой на себя наверху принимается" "" (Verdict $oneNodeSelf)
$oneNodeWrongTop = Saved '[[1.0,0.0]]' '["a"]' '[{}]' '[[[],[]]]' 0 0
Check "одноузловой указатель с занижённым верхом отвергается прежним кодом" "INDEX_MAX_LEVEL_MISMATCH: top level 0" (Verdict $oneNodeWrongTop)
Write-Output ""

# ── Панель 5. Чувствительность с названной границей отличимости ────────────
Write-Output "Панель 5. Чувствительность: каждая новая проверка испорчена правдоподобно."
Write-Output ""
Write-Output "  Порча 1: порог проверки уровня соседа сдвинут на единицу (строгое меньше)."
$mutA = Verdict $copyA 1
Write-Output ("    копия A под испорченной проверкой -> " + $(if ($mutA -eq '') { 'ПРИНЯТО (порок виден)' } else { $mutA }))
Check "испорченный порог пропускает копию A" "" $mutA
Check "исправное состояние испорченный порог тоже принимает" "" (Verdict $frozenJson 1)

Write-Output ""
Write-Output "  Порча 2: у глобальной проверки оставлена только половина «верх завышен»."
$mutB = Verdict $copyB 2
Write-Output ("    копия B под испорченной проверкой -> " + $(if ($mutB -eq '') { 'ПРИНЯТО (порок виден)' } else { $mutB }))
Check "испорченная половина пропускает копию B" "" $mutB

# Завышенный верх: его ловит ПРЕЖНЯЯ проверка, и это граница отличимости.
$tooHigh = Edit-FrozenCopy { param($c) $c.MaxLevel = 4 }
$tooHighVerdict = Verdict $tooHigh
Write-Output ("    завышенный верх -> " + $tooHighVerdict)
Check "завышенный верх ловит прежняя проверка точки входа, а не новая" "INDEX_MAX_LEVEL_MISMATCH: top level 4" $tooHighVerdict
Write-Output ""
Write-Output "  Граница отличимости, названная прямо и измеренная."
Write-Output "  У проверки уровня соседа несущим является именно нестрогое сравнение:"
Write-Output "  сосед с ровно l уровнями не имеет уровня l, потому что уровни считаются"
Write-Output "  с нуля. Сдвиг порога на единицу пропускает ВСЕ такие рёбра, то есть весь"
Write-Output "  дефект целиком, и при этом не даёт ни одного ложного отказа — поэтому"
Write-Output "  отличить его можно только случаем с повреждённым ребром, а не контролем."
Write-Output ""
Write-Output "  У глобальной проверки сегодня достижима ровно ОДНА половина равенства:"
Write-Output "  «ни один узел не глубже объявленного верха». Вторая половина, завышенный"
Write-Output "  верх, перехватывается ПРЕЖНЕЙ проверкой совпадения с глубиной точки входа"
Write-Output "  и до новой проверки не доходит — это измерено выше. Значит равенство"
Write-Output "  записано ради устойчивости к будущему ослаблению прежней проверки, а его"
Write-Output "  несущая часть сегодня — только половина про глубокий узел."
Write-Output ""

# ── Панель 6. Сверка опубликованной улики аудита о слепоте панели ──────────
Write-Output "Панель 6. Слепота панели точности: сверка опубликованной улики аудита."
# Пакет свидетельств аудита опубликован в вершине канонной ветки, но на диске
# этой рабочей копии его нет — ровно так же, как самого документа аудита.
# Поэтому улика читается из опубликованного коммита, закреплённого здесь
# буквально; это чтение чужого опубликованного свидетельства, а не запись.
# Если улика недоступна ни на диске, ни в вершине, панель помечается
# NOT_EXECUTED и НЕ считается упавшей сверкой: недоступность чужого
# свидетельства не является дефектом этого ремонта.
$auditCommit = 'aef5fbb6878e0225eed99211339d37a7b844534f'
$auditRelative = 'experiments-v2/_audits/2026-11-01-post89-execution-boundary-audit/components/hnsw-run-04/results.json'
$auditResults = Join-Path $repoRoot ($auditRelative -replace '/', '\')
$auditText = $null
if (Test-Path -LiteralPath $auditResults) {
    $auditText = Get-Content -Raw -LiteralPath $auditResults
    Write-Output "  источник улики: рабочая копия на диске"
} else {
    $probe = & git -C $repoRoot cat-file -e "${auditCommit}:${auditRelative}" 2>&1
    if ($LASTEXITCODE -eq 0) {
        $auditText = (& git -C $repoRoot show "${auditCommit}:${auditRelative}") -join "`n"
        Write-Output ("  источник улики: опубликованный коммит " + $auditCommit.Substring(0, 10))
    }
}
if ($null -ne $auditText) {
    $audit = $auditText | ConvertFrom-Json
    Write-Output ("  загрузок в пробе аудита: " + $audit.loads + "; поисков: " + $audit.searches + "; обучений: " + $audit.train + "; добавлений: " + $audit.add)
    foreach ($row in $audit.rows) {
        $same = @($row.queries | Where-Object { $_.same_id }).Count
        $total = @($row.queries).Count
        Write-Output ("  " + $row.name + ": загрузка принята=" + $row.load_accepted + "; совпавших ответов " + $same + " из " + $total)
    }
    $malformed = @($audit.rows | Where-Object { $_.invalid_topology })
    Check "испорченных копий в улике аудита ровно две" 2 $malformed.Count
    $allTwelve = @($malformed | Where-Object { (@($_.queries | Where-Object { $_.same_id }).Count) -eq 12 }).Count
    Check "на обеих испорченных копиях панель давала 12 из 12" 2 $allTwelve
    $acceptedMalformed = @($malformed | Where-Object { $_.load_accepted }).Count
    Check "обе испорченные копии прежняя загрузка принимала" 2 $acceptedMalformed
} else {
    Write-Output "  улика аудита недоступна ни на диске, ни в закреплённом коммите."
    Write-Output "  Эта панель помечается NOT_EXECUTED и не считается упавшей сверкой:"
    Write-Output "  недоступность чужого свидетельства не является дефектом ремонта."
    $script:panel6NotExecuted = $true
}
Write-Output ""

# ── Итог ───────────────────────────────────────────────────────────────────
Write-Output "Бюджет этого прогона."
Write-Output "  сборок проекта: 0; прогонов площадки: 0; обучения: 0; добавлений: 0"
Write-Output "  одна внутрипроцессная компиляция оболочкой для переноса логики"
Write-Output "  файлы опыта не редактировались: испорченные копии построены в памяти"
Write-Output ""
Write-Output "Чем это НЕ доказано."
Write-Output "  Родная приёмка платформы здесь не исполнялась: лане сборки и тестов"
Write-Output "  закрыта правами сессии, поэтому этот уровень помечается NOT_EXECUTED,"
Write-Output "  а не PASS. Перенесена только цепочка проверок одного метода; навигация,"
Write-Output "  поиск и добавление сюда не переносились, поэтому слепота панели точности"
Write-Output "  сверена по опубликованной улике аудита, а не измерена заново. Частота"
Write-Output "  таких повреждений в живых данных не измерялась. Взаимность рёбер,"
Write-Output "  повторяющиеся соседи, достижимость всех узлов обходом и не-числа в"
Write-Output "  координатах остаются отдельными договорами."
Write-Output ""

if ($panel6NotExecuted) {
    Write-Output "  Шестая панель: NOT_EXECUTED — чужая опубликованная улика недоступна."
    Write-Output ""
}

if ($failures -eq 0) {
    Write-Output ("ИТОГ: все {0} сверок прошли. Две межузловые проверки отвергают обе испорченные копии до выдачи экземпляра, одиннадцать прежних кодов сохранены, положительные контроли приняты." -f $checks)
    exit 0
} else {
    Write-Output ("ИТОГ: сверок {0}, упало {1}." -f $checks, $failures)
    exit 1
}
