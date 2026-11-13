# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
# See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
#
# Проверяемая программа для девятой работы ремонта, её двух пробелов: отказ по
# непригодной ширине построения указателя ближайших соседей и измерение самого
# дефекта — что происходит с соседями нового узла при нулевой ширине.
#
# Что здесь перенесено ДОСЛОВНО из `06.VectorSearch/HnswIndex.cs`:
#   * ограничение луча `results.Take(ef)` из поиска по слою (строка 436 до правки);
#   * цикл двусторонних связей добавления (строки 123-133): кандидаты берутся
#     лучом ширины построения, из них отбираются соседи, и только затем в графе
#     появляются обе ссылки — новый узел на соседа и сосед на новый узел;
#   * проверка параметра при загрузке и её отсутствие в прежнем варианте.
#
# Что здесь СОКРАЩЕНО, и это сказано прямо: спуск по уровням и настоящий обход
# графа заменены одним уровнем и полным перебором узлов, упорядоченных по
# расстоянию. На маленьком полносвязном графе это поведенчески то же самое —
# луч всё равно ограничивает число кандидатов, — но это модель, а не весь
# механизм. Многоуровневая навигация, выбор случайного уровня и обрезка соседей
# по верхнему пределу здесь не воспроизводятся и не проверяются.
#
# Чего эта программа не делает. Она не компилирует продуктовый проект и не
# запускает платформенные тесты: сборка и прогон закрыты правами сессии.
# Родная квота приёмки плана не выполняется здесь ни в какой части.

$ErrorActionPreference = 'Stop'

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
if ($null -ne $PSStyle) { $PSStyle.OutputRendering = 'PlainText' }

$refRoot = Get-ChildItem -LiteralPath (Join-Path $env:ProgramFiles 'dotnet\packs\Microsoft.NETCore.App.Ref') -Directory |
    Sort-Object Name -Descending | Select-Object -First 1
$refDir = Join-Path $refRoot.FullName 'ref\net9.0'
Write-Output "Ссылочные сборки платформы: $refDir"
Write-Output ""

$refs = Get-ChildItem -LiteralPath $refDir -Filter '*.dll' |
    Where-Object { $_.Name -notlike '*Native*' } |
    ForEach-Object { $_.FullName }

$code = @'
using System;
using System.Collections.Generic;
using System.Linq;

namespace OracleBeam
{
    // Итог загрузки: либо отказ с кодом, либо принятый экземпляр с его шириной.
    public sealed class LoadResult
    {
        public bool Accepted;
        public string RefusalMessage = "";
        public int EfConstruction;
        public override string ToString() =>
            Accepted ? ("принят, ширина построения " + EfConstruction) : ("отказ: " + RefusalMessage);
    }

    public static class Oracle
    {
        public const int Legacy = 0;       // как было до правки: параметр не проверяется
        public const int Transferred = 1;  // как стало: ширина построения <= 0 отвергается
        public const int Mutated = 2;      // правдоподобная поломка: проверяется только < 0

        // Проверка параметра при загрузке. Отсутствующее поле приходит сюда
        // нулём, потому что в разборе данных это обычное целое.
        public static LoadResult Load(int savedBeam, int variant)
        {
            if (variant == Transferred && savedBeam <= 0)
                return new LoadResult
                {
                    Accepted = false,
                    RefusalMessage = "INDEX_CONSTRUCTION_BEAM_INVALID: construction beam " + savedBeam + " is not positive"
                };

            // Испорченный вариант забывает, что отсутствующее поле даёт ноль.
            if (variant == Mutated && savedBeam < 0)
                return new LoadResult
                {
                    Accepted = false,
                    RefusalMessage = "INDEX_CONSTRUCTION_BEAM_INVALID: construction beam " + savedBeam + " is not positive"
                };

            return new LoadResult { Accepted = true, EfConstruction = savedBeam };
        }

        // Дословный перенос ограничения луча: при нуле список кандидатов пуст
        // независимо от того, сколько узлов нашлось.
        private static List<int> SearchLayer(List<float[]> vectors, float[] query, int ef)
        {
            var results = Enumerable.Range(0, vectors.Count)
                .Select(i => new { Id = i, D = Distance(query, vectors[i]) })
                .OrderBy(r => r.D)
                .ToList();
            return results.Take(ef).Select(r => r.Id).ToList();
        }

        // Дословный перенос отбора соседей по верхнему пределу их числа.
        private static List<int> SelectNeighbours(List<int> candidates, int maxM)
        {
            return candidates.Take(maxM).ToList();
        }

        // Дословный перенос цикла двусторонних связей добавления.
        // Возвращает: сколько соседей получил новый узел и сколько старых узлов
        // получили ссылку на него.
        public static (int NewNodeNeighbours, int OldNodesLinking) Add(
            List<float[]> vectors, List<List<int>> graph, float[] newVector, int efConstruction, int m)
        {
            var newId = vectors.Count;
            vectors.Add(newVector);
            graph.Add(new List<int>());

            var candidates = SearchLayer(vectors.Take(newId).ToList(), newVector, efConstruction);
            var neighbours = SelectNeighbours(candidates, m * 2);

            foreach (int nb in neighbours)
            {
                graph[newId].Add(nb);
                graph[nb].Add(newId);
            }

            var linking = Enumerable.Range(0, newId).Count(i => graph[i].Contains(newId));
            return (graph[newId].Count, linking);
        }

        private static float Distance(float[] a, float[] b)
        {
            float sum = 0f;
            for (int i = 0; i < a.Length; i++) sum += (a[i] - b[i]) * (a[i] - b[i]);
            return sum;
        }

        // Небольшое начальное состояние: три узла, все связаны между собой.
        public static (List<float[]> Vectors, List<List<int>> Graph) StartingState()
        {
            var vectors = new List<float[]>
            {
                new[] { 1f, 0f, 0f },
                new[] { 0f, 1f, 0f },
                new[] { 0f, 0f, 1f }
            };
            var graph = new List<List<int>>
            {
                new List<int> { 1, 2 },
                new List<int> { 0, 2 },
                new List<int> { 0, 1 }
            };
            return (vectors, graph);
        }

        // Публичный поиск берёт наибольшее из своей ширины и запрошенного числа
        // ответов — дословный перенос строки 162. Нужен панели о ширине поиска.
        public static int EffectiveSearchBeam(int efSearch, int topK) => Math.Max(efSearch, topK);
    }
}
'@

Add-Type -TypeDefinition $code -ReferencedAssemblies $refs -IgnoreWarnings -WarningAction SilentlyContinue

$failures = 0
$checks = 0

function Check([string]$what, $expected, $actual) {
    $script:checks++
    if ("$expected" -eq "$actual") {
        Write-Output ("  ок     {0} — ожидалось {1}, получено {2}" -f $what, $expected, $actual)
    } else {
        $script:failures++
        Write-Output ("  УПАЛО  {0} — ожидалось {1}, получено {2}" -f $what, $expected, $actual)
    }
}

Write-Output "Панель 1. ИЗМЕРЕНИЕ ДЕФЕКТА: продолженное добавление при разной ширине построения."
Write-Output "          Начальное состояние: три узла, все связаны. Добавляется четвёртый."
foreach ($beam in 0, 1, 20, 64) {
    $state = [OracleBeam.Oracle]::StartingState()
    $r = [OracleBeam.Oracle]::Add($state.Item1, $state.Item2, [float[]]@(0.5, 0.5, 0), $beam, 4)
    Write-Output ("  ширина {0,2} : соседей у нового узла {1}, старых узлов со ссылкой на него {2}" -f $beam, $r.Item1, $r.Item2)
}
$zero = [OracleBeam.Oracle]::StartingState()
$rz = [OracleBeam.Oracle]::Add($zero.Item1, $zero.Item2, [float[]]@(0.5, 0.5, 0), 0, 4)
Check "при нулевой ширине новый узел не получает ни одного соседа" 0 $rz.Item1
Check "при нулевой ширине ни один старый узел не ссылается на новый" 0 $rz.Item2
$pos = [OracleBeam.Oracle]::StartingState()
$rp = [OracleBeam.Oracle]::Add($pos.Item1, $pos.Item2, [float[]]@(0.5, 0.5, 0), 20, 4)
Check "при ширине 20 новый узел получает всех трёх соседей" 3 $rp.Item1
Check "при ширине 20 все три старых узла ссылаются на новый" 3 $rp.Item2
Write-Output "  Это и есть «новое знание отсоединено от старого», измеренное числом:"
Write-Output "  при нулевой ширине обе величины ноль, при положительной — три и три."
Write-Output ""

Write-Output "Панель 2. Отказ при загрузке: поле убрано (ноль при разборе), ноль, минус один."
foreach ($saved in 0, -1) {
    $l = [OracleBeam.Oracle]::Load($saved, 0)
    $n = [OracleBeam.Oracle]::Load($saved, 1)
    Write-Output ("  сохранено {0,2} : прежний {1} | новый {2}" -f $saved, $l, $n)
}
Check "прежний вариант принимает нулевую ширину как годную" "принят, ширина построения 0" ([OracleBeam.Oracle]::Load(0, 0))
Check "новый вариант отвергает нулевую ширину с названным значением" "отказ: INDEX_CONSTRUCTION_BEAM_INVALID: construction beam 0 is not positive" ([OracleBeam.Oracle]::Load(0, 1))
Check "новый вариант отвергает отрицательную ширину с её значением" "отказ: INDEX_CONSTRUCTION_BEAM_INVALID: construction beam -1 is not positive" ([OracleBeam.Oracle]::Load(-1, 1))
Write-Output ""

Write-Output "Панель 3. Контроль ложного отказа: проверенное начальное состояние с явной шириной 64."
$ok = [OracleBeam.Oracle]::Load(64, 1)
Write-Output ("  новый вариант : {0}" -f $ok)
Check "проверенное состояние по-прежнему принимается" "принят, ширина построения 64" $ok
Check "и прежний вариант принимал его так же" "принят, ширина построения 64" ([OracleBeam.Oracle]::Load(64, 0))
Write-Output ""

Write-Output "Панель 4. Честная граница: ширина ПОИСКА нулём дефекта не даёт."
foreach ($k in 1, 3, 10) {
    Write-Output ("  запрошено ответов {0,2} : действующая ширина поиска при нулевой настройке {1}" -f $k, [OracleBeam.Oracle]::EffectiveSearchBeam(0, $k))
}
Check "при нулевой настройке действующая ширина поиска равна запрошенному числу" 3 ([OracleBeam.Oracle]::EffectiveSearchBeam(0, 3))
Write-Output "  Поэтому подавать нулевую ширину поиска тем же контрпримером нельзя:"
Write-Output "  публичный поиск берёт наибольшее из настройки и запрошенного числа."
Write-Output ""

Write-Output "Панель 5. Чувствительность: испорченный вариант проверяет только строго отрицательное."
$sens = 0
foreach ($saved in 0, -1) {
    $n = [OracleBeam.Oracle]::Load($saved, 1)
    $m = [OracleBeam.Oracle]::Load($saved, 2)
    if ("$n" -ne "$m") {
        $sens++
        Write-Output ("  упало  сохранено {0}: у нового {1}, у испорченного {2}" -f $saved, $n, $m)
    } else {
        Write-Output ("  совпало сохранено {0}: оба дают {1}" -f $saved, $n)
    }
}
Check "испорченная логика обязана уронить хотя бы одну сверку" $true ($sens -gt 0)
Write-Output ("  испорченная логика уронила сверок: {0} из 2" -f $sens)
Write-Output ""
Write-Output "Панель 5, честная граница. Испорченный вариант отличается ровно на нуле —"
Write-Output "то есть на самом опасном случае, потому что отсутствующее поле даёт именно"
Write-Output "ноль. На минус одном он неотличим от нового. Значит ловить подмену «не"
Write-Output "больше нуля» на «меньше нуля» обязан случай с убранным полем, а не случай"
Write-Output "с отрицательным значением."
Write-Output ""

if ($failures -eq 0) {
    Write-Output ("ИТОГ: все {0} сверок прошли. Правка воспроизводит назначенное поведение, испорченная логика отличается." -f $checks)
    exit 0
} else {
    Write-Output ("ИТОГ: сверок {0}, упало {1}." -f $checks, $failures)
    exit 1
}
