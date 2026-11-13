# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
# See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
#
# Проверяемая программа для правки FIX-RF05: ранний типизированный отказ в
# обученной перегрузке классификатора маршрута проекта 29.
#
# Зачем она нужна. Сборка кода на C# и прогон тестов площадки закрыты правами
# этой сессии, поэтому поведение доказывается иначе: логика трёх вариантов
# перенесена сюда ДОСЛОВНО из продуктового файла и исполняется на фактических
# продуктовых уроках. Три варианта: прежний (как было до правки), новый (как
# стало) и испорченный (правдоподобная будущая поломка — проверка пустого входа
# переставлена ПОСЛЕ поиска соседей, а пустое объединение снова объявлено
# идеальным совпадением). Испорченный вариант обязан уронить сверки: это и есть
# проверка на чувствительность, без которой «всё сошлось» ничего не значит.
#
# Чего эта программа не делает. Она не компилирует продуктовый проект и не
# запускает платформенные тесты. Она проверяет алгоритм, а не сборку.

$ErrorActionPreference = 'Stop'

# Вывод этой программы читает человек, и он по-русски. Без явной кодировки
# оболочка заменяет кириллицу вопросительными знаками, и сохранённые показания
# становятся нечитаемыми.
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

# Цветовые управляющие последовательности в сохранённом файле показаний — мусор,
# поэтому вывод делается простым текстом.
if ($null -ne $PSStyle) { $PSStyle.OutputRendering = 'PlainText' }

# Ссылочные сборки платформы: Add-Type заменяет набор ссылок целиком, поэтому
# их надо перечислить явно, иначе не найдётся даже набор и перечисление.
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

namespace OracleRf05
{
    // Пять канонических тем плюс «неизвестно», как в продуктовом перечислении.
    public enum Domain { CODE_Q, BUG, PLAN, ARCH, REFACTOR, UNKNOWN }

    // Результат чтения: успех с темой либо отказ с кодом. Повторяет форму
    // продуктового OperationResult ровно в той части, которую читает панель.
    public sealed class Answer
    {
        public bool IsSuccess;
        public Domain Value;
        public string ErrorCode = "";
        public bool Threw;

        public override string ToString()
        {
            if (Threw) return "ИСКЛЮЧЕНИЕ";
            return IsSuccess ? ("успех: " + Value) : ("отказ: " + ErrorCode);
        }
    }

    public static class Oracle
    {
        // Вариант логики: 0 — прежний, 1 — новый, 2 — испорченный.
        public const int Legacy = 0;
        public const int Transferred = 1;
        public const int Mutated = 2;

        // Мера несходства, дословный перенос. Вариант управляет двумя местами:
        // терпимостью к отсутствующему массиву и значением пустого объединения.
        public static double JaccardDist(int[] a, int[] b, int variant)
        {
            HashSet<int> sa;
            HashSet<int> sb;
            if (variant == Transferred)
            {
                // Новый вариант: отсутствующий массив читается как набор без признаков.
                sa = a is null ? new HashSet<int>() : new HashSet<int>(a);
                sb = b is null ? new HashSet<int>() : new HashSet<int>(b);
            }
            else
            {
                // Прежний и испорченный: построение набора из отсутствующего массива бросает.
                sa = new HashSet<int>(a);
                sb = new HashSet<int>(b);
            }

            int inter = sa.Count(x => sb.Contains(x));
            int union = sa.Count + sb.Count - inter;

            // Новый вариант: пустое объединение означает отсутствие общих признаков.
            if (union == 0)
                return variant == Transferred ? 1.0 : 0.0;

            return 1.0 - (double)inter / union;
        }

        // Обученная перегрузка, дословный перенос трёх вариантов.
        public static Answer Classify(
            List<Tuple<int[], Domain>> train, int k, int[] predicates, int variant)
        {
            try
            {
                // Прежний порядок: сначала необученность. Сохранён во всех вариантах.
                if (train.Count == 0)
                    return new Answer { IsSuccess = false, ErrorCode = "NOT_TRAINED" };

                // Новый вариант: отказ до поиска соседей.
                if (variant == Transferred && (predicates is null || predicates.Length == 0))
                    return new Answer { IsSuccess = false, ErrorCode = "NO_EVIDENCE_FOR_DECISION" };

                var best = train
                    .OrderBy(t => JaccardDist(predicates, t.Item1, variant))
                    .Take(k)
                    .GroupBy(t => t.Item2)
                    .OrderByDescending(g => g.Count())
                    .First();

                // Испорченный вариант: та же проверка, но ПОСЛЕ поиска соседей —
                // ровно та правдоподобная поломка, от которой защищает порядок.
                if (variant == Mutated && (predicates is null || predicates.Length == 0))
                    return new Answer { IsSuccess = false, ErrorCode = "NO_EVIDENCE_FOR_DECISION" };

                return new Answer { IsSuccess = true, Value = best.Key };
            }
            catch (ArgumentNullException)
            {
                return new Answer { Threw = true };
            }
        }

        // Пустая память: нужна панели необученности, которая проверяет, что
        // прежний код отказа сохранён и порядок двух проверок не менялся.
        public static List<Tuple<int[], Domain>> NoLessons()
        {
            return new List<Tuple<int[], Domain>>();
        }

        // Семь уроков, которыми продукт засевает классификатор. Источник —
        // 21.AgentInfrastructure/Logical/PromptDomainClassifier.cs, строки 79-87.
        public static List<Tuple<int[], Domain>> SeededLessons()
        {
            return new List<Tuple<int[], Domain>>
            {
                Tuple.Create(new[] { 0, 1, 2 }, Domain.BUG),
                Tuple.Create(new[] { 1 }, Domain.BUG),
                Tuple.Create(new[] { 5, 8, 9, 10, 19 }, Domain.PLAN),
                Tuple.Create(new[] { 0, 2 }, Domain.ARCH),
                Tuple.Create(new[] { 0, 2, 3, 4, 21 }, Domain.ARCH),
                Tuple.Create(new[] { 12, 13, 14 }, Domain.CODE_Q),
                Tuple.Create(new[] { 5, 7 }, Domain.REFACTOR)
            };
        }
    }
}
'@

# Предупреждение CS1701 о несовпадении версии System.Runtime здесь неизбежно:
# ссылочные сборки платформы девятой версии, а оболочка исполняется на десятой.
# На поведение перенесённой логики это не влияет — она пользуется только набором,
# перечислением и упорядочиванием, — поэтому предупреждения глушатся, чтобы не
# залить показания сотней одинаковых строк. Ошибки сборки при этом НЕ глушатся:
# при ошибке Add-Type остановит программу, потому что режим остановки включён.
Add-Type -TypeDefinition $code -ReferencedAssemblies $refs -IgnoreWarnings -WarningAction SilentlyContinue

$lessons = [OracleRf05.Oracle]::SeededLessons()
$k = 5
$names = @('прежний    ', 'новый      ', 'испорченный')
$variants = @(0, 1, 2)

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

Write-Output "Панель 1. Расстояние от пустого запроса до каждого продуктового урока."
foreach ($v in $variants) {
    $line = ""
    foreach ($lesson in $lessons) {
        $d = [OracleRf05.Oracle]::JaccardDist(@(), $lesson.Item1, $v)
        $line += ("{0:F3} " -f $d)
    }
    Write-Output ("  {0} : {1}" -f $names[$v], $line.Trim())
}
Check "прежний: расстояние от пустого запроса до первого урока" "1.000" ("{0:F3}" -f [OracleRf05.Oracle]::JaccardDist(@(), $lessons[0].Item1, 0))
Check "новый:   расстояние от пустого запроса до первого урока" "1.000" ("{0:F3}" -f [OracleRf05.Oracle]::JaccardDist(@(), $lessons[0].Item1, 1))
Write-Output ""

Write-Output "Панель 2. Два набора без признаков: расстояние между ними."
foreach ($v in $variants) {
    $d = [OracleRf05.Oracle]::JaccardDist(@(), @(), $v)
    Write-Output ("  {0} : {1:F3}" -f $names[$v], $d)
}
Check "прежний: два пустых набора объявлены идеальным совпадением" "0.000" ("{0:F3}" -f [OracleRf05.Oracle]::JaccardDist(@(), @(), 0))
Check "новый:   два пустых набора — ни одного общего признака" "1.000" ("{0:F3}" -f [OracleRf05.Oracle]::JaccardDist(@(), @(), 1))
Write-Output ""

Write-Output "Панель 3. Ответ на запрос БЕЗ признаков при семи выученных уроках и пяти соседях."
$a3 = @{}
foreach ($v in $variants) {
    $a3[$v] = [OracleRf05.Oracle]::Classify($lessons, $k, @(), $v)
    Write-Output ("  {0} : {1}" -f $names[$v], $a3[$v])
}
Check "прежний: пустой запрос получает метку первого уложенного урока как успех" "успех: BUG" $a3[0]
Check "новый:   пустой запрос получает названный отказ" "отказ: NO_EVIDENCE_FOR_DECISION" $a3[1]
Write-Output ""

Write-Output "Панель 4. Ответ на ОТСУТСТВУЮЩИЙ массив признаков."
$a4 = @{}
foreach ($v in $variants) {
    $a4[$v] = [OracleRf05.Oracle]::Classify($lessons, $k, $null, $v)
    Write-Output ("  {0} : {1}" -f $names[$v], $a4[$v])
}
Check "прежний: отсутствующий массив бросает вместо отказа" "ИСКЛЮЧЕНИЕ" $a4[0]
Check "новый:   отсутствующий массив получает названный отказ" "отказ: NO_EVIDENCE_FOR_DECISION" $a4[1]
Write-Output ""

Write-Output "Панель 5. Контроль: непустой запрос с пересечением. Ответы обязаны совпасть у всех трёх вариантов."
$query5 = @(1)
$a5 = @{}
foreach ($v in $variants) {
    $a5[$v] = [OracleRf05.Oracle]::Classify($lessons, $k, $query5, $v)
    Write-Output ("  {0} : {1}" -f $names[$v], $a5[$v])
}
Check "прежний и новый отвечают на запрос с пересечением одинаково" $a5[0] $a5[1]
Check "испорченный на этом запросе тоже не отличается" $a5[0] $a5[2]
Write-Output ""

Write-Output "Панель 6. Контроль необученности: пустой запрос на пустой памяти."
$empty = [OracleRf05.Oracle]::NoLessons()
$a6 = @{}
foreach ($v in $variants) {
    $a6[$v] = [OracleRf05.Oracle]::Classify($empty, $k, @(), $v)
    Write-Output ("  {0} : {1}" -f $names[$v], $a6[$v])
}
Check "прежний код отказа сохранён: порядок проверок не менялся" "отказ: NOT_TRAINED" $a6[1]
Check "прежний вариант отвечает тем же кодом" "отказ: NOT_TRAINED" $a6[0]
Write-Output ""

Write-Output "Панель 7. ОСТАТОК, который эта правка НЕ чинит: непустой запрос без единого общего признака."
$query7 = @(99, 100)
$a7 = @{}
foreach ($v in $variants) {
    $a7[$v] = [OracleRf05.Oracle]::Classify($lessons, $k, $query7, $v)
    Write-Output ("  {0} : {1}" -f $names[$v], $a7[$v])
}
Check "новый вариант на запросе без пересечения по-прежнему отдаёт метку первого урока" "успех: BUG" $a7[1]
Check "и прежний вариант отвечает тем же" "успех: BUG" $a7[0]
Write-Output "  Это измеренный факт, а не предположение: отказ добавлен только для"
Write-Output "  пустого и отсутствующего входа, как и назначено нарядом. Запрос с"
Write-Output "  признаками, ни один из которых не встречается ни в одном уроке, даёт"
Write-Output "  те же равные расстояния и ту же метку первого урока. Это отдельная"
Write-Output "  строка работ, а не часть этой правки."
Write-Output ""

Write-Output "Панель 8. Наблюдение того же семейства, правкой не затронутое: точное совпадение может проиграть голос."
$query8 = @(12, 13, 14)
$a8 = [OracleRf05.Oracle]::Classify($lessons, $k, $query8, 1)
$d8 = [OracleRf05.Oracle]::JaccardDist($query8, $lessons[5].Item1, 1)
Write-Output ("  запрос совпадает с уроком темы CODE_Q дословно, расстояние {0:F3}" -f $d8)
Write-Output ("  ответ нового варианта: {0}" -f $a8)
Check "точное совпадение есть, но голос пяти соседей отдаёт ответ другой теме" "успех: BUG" $a8
Write-Output "  Голос не взвешен по расстоянию: один сосед на нуле проигрывает двум"
Write-Output "  соседям на единице. Правка этого не касается и не заявляет."
Write-Output ""

Write-Output "Панель 9. Чувствительность: что именно роняет испорченный вариант."
$sens = 0
if ("$($a3[2])" -ne "$($a3[1])") { $sens++; Write-Output ("  упало  ответ на пустой запрос: у нового {0}, у испорченного {1}" -f $a3[1], $a3[2]) }
if ("$($a4[2])" -ne "$($a4[1])") { $sens++; Write-Output ("  упало  ответ на отсутствующий массив: у нового {0}, у испорченного {1}" -f $a4[1], $a4[2]) }
$dm = [OracleRf05.Oracle]::JaccardDist(@(), @(), 2)
$dt = [OracleRf05.Oracle]::JaccardDist(@(), @(), 1)
if ("$dm" -ne "$dt") { $sens++; Write-Output ("  упало  расстояние двух пустых наборов: у нового {0:F3}, у испорченного {1:F3}" -f $dt, $dm) }
Check "испорченная логика обязана уронить хотя бы одну сверку" $true ($sens -gt 0)
Write-Output ("  испорченная логика уронила сверок: {0} из 3" -f $sens)
Write-Output ""
Write-Output "Панель 9, честная граница. Испорченный вариант всё же отвечает отказом"
Write-Output "на пустой запрос — только ПОСЛЕ поиска соседей. На этих панелях разница"
Write-Output "видна по ответу на отсутствующий массив и по расстоянию двух пустых"
Write-Output "наборов, а не по коду отказа на пустом массиве. Значит ловить перенос"
Write-Output "проверки обязаны именно эти две сверки."
Write-Output ""

if ($failures -eq 0) {
    Write-Output ("ИТОГ: все {0} сверок прошли. Правка воспроизводит назначенное поведение, испорченная логика отличается." -f $checks)
    exit 0
} else {
    Write-Output ("ИТОГ: сверок {0}, упало {1}." -f $checks, $failures)
    exit 1
}
