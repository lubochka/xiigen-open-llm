# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
# See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
#
# Проверяемая программа для второй половины правки FIX-RF05: отказ при нулевом
# сходстве в обученной перегрузке классификатора маршрута проекта 29.
#
# Чем она отличается от первой. Первая половина правки продуктового ответа не
# меняла: пустой и отсутствующий профиль потребитель отсекает сам. Эта половина
# ответ МЕНЯЕТ, поэтому здесь дословно перенесена не только логика
# классификатора, но и вся детерминированная цепочка потребителя
# (`23.SemanticCompilerAdapter/DomainClassifierUnit.cs`), чтобы было измерено, во
# что именно превращается отказ: какая ветвь срабатывает, какая получается тема,
# какая уверенность и какие доводы.
#
# Три варианта логики классификатора: прежний (как опубликовано сейчас, с отказом
# только на пустом и отсутствующем входе), новый (плюс отказ при нулевом сходстве
# по ЛУЧШЕМУ расстоянию) и испорченный (сравнивает ПЕРВОЕ расстояние вместо
# лучшего — правдоподобная будущая поломка). Испорченный обязан уронить сверки.
#
# Чего эта программа не делает. Она не компилирует продуктовый проект и не
# запускает платформенные тесты. Контекстные пакеты потребителя (прежние темы и
# карта прежней уверенности) не моделируются: ни одна строка панелей их не
# передаёт, и это сказано прямо, а не умолчано.

$ErrorActionPreference = 'Stop'

# Вывод читает человек, и он по-русски: без явной кодировки оболочка заменяет
# кириллицу вопросительными знаками.
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

namespace OracleZeroSim
{
    public enum Domain { CODE_Q, BUG, PLAN, ARCH, REFACTOR, UNKNOWN }

    // Ответ классификатора в той части, которую читает потребитель.
    public sealed class Answer
    {
        public bool IsSuccess;
        public Domain Value;
        public string ErrorCode = "";
        public override string ToString() =>
            IsSuccess ? ("успех: " + Value) : ("отказ: " + ErrorCode);
    }

    // Ответ потребителя плюс название сработавшей ветви его цепочки.
    public sealed class ConsumerAnswer
    {
        public Domain Label;
        public double Confidence;
        public string Branch = "";
        public string Evidence = "";
        public override string ToString() =>
            string.Format("{0}, уверенность {1:F2}, ветвь «{2}», доводы [{3}]",
                Label, Confidence, Branch, Evidence);
    }

    public static class Oracle
    {
        public const int Legacy = 0;
        public const int Transferred = 1;
        public const int Mutated = 2;

        // ── Классификатор проекта 29, дословный перенос ──────────────────────

        // Мера несходства после первой половины правки: отсутствующий массив
        // читается как набор без признаков, пустое объединение даёт единицу.
        public static double JaccardDist(int[] a, int[] b)
        {
            var sa = a is null ? new HashSet<int>() : new HashSet<int>(a);
            var sb = b is null ? new HashSet<int>() : new HashSet<int>(b);
            int inter = sa.Count(x => sb.Contains(x));
            int union = sa.Count + sb.Count - inter;
            if (union == 0) return 1.0;
            return 1.0 - (double)inter / union;
        }

        public static Answer Classify(
            List<Tuple<int[], Domain>> train, int k, int[] predicates, int variant)
        {
            // Первая проверка: необученность. Во всех вариантах одинакова.
            if (train.Count == 0)
                return new Answer { IsSuccess = false, ErrorCode = "NOT_TRAINED" };

            // Вторая проверка: пустой и отсутствующий вход. Уже опубликована.
            if (predicates is null || predicates.Length == 0)
                return new Answer { IsSuccess = false, ErrorCode = "NO_EVIDENCE_FOR_DECISION" };

            var scored = train
                .Select(lesson => new { Lesson = lesson, Distance = JaccardDist(predicates, lesson.Item1) })
                .ToList();

            // Третья проверка: нулевое сходство.
            if (variant == Transferred && scored.Min(row => row.Distance) >= 1.0)
                return new Answer { IsSuccess = false, ErrorCode = "NO_EVIDENCE_FOR_DECISION" };

            // Испорченный вариант: сравнивает ПЕРВОЕ расстояние вместо лучшего.
            if (variant == Mutated && scored[0].Distance >= 1.0)
                return new Answer { IsSuccess = false, ErrorCode = "NO_EVIDENCE_FOR_DECISION" };

            var best = scored
                .OrderBy(row => row.Distance)
                .Take(k)
                .GroupBy(row => row.Lesson.Item2)
                .OrderByDescending(g => g.Count())
                .First();

            return new Answer { IsSuccess = true, Value = best.Key };
        }

        // Наборы ключевых слов проекта 29, дословно (строки 27-34 исходника).
        private static readonly Dictionary<Domain, string[]> KeywordMap = new Dictionary<Domain, string[]>
        {
            { Domain.BUG,      new[]{"fix","error","exception","null","fail","wrong","broken","why","debug","crash","bug"} },
            { Domain.REFACTOR, new[]{"refactor","clean","improve","simplify","restructur","rename","extract","move"} },
            { Domain.PLAN,     new[]{"flow","phase","plan","step","sequence","orchestrat","workflow","deploy","implement","build","create"} },
            { Domain.ARCH,     new[]{"architect","architecture","topology","design","pattern","structure","best","should","approach","layer","boundary","solid"} },
            { Domain.CODE_Q,   new[]{"what","how","explain","show","find","list","where","which","describe","understand","does","return","use","pattern"} }
        };

        public static string[] MatchedKeywords(string text, Domain domain)
        {
            string[] keywords;
            if (!KeywordMap.TryGetValue(domain, out keywords)) return new string[0];
            var lower = (text ?? "").ToLowerInvariant();
            return keywords
                .Where(keyword => lower.Contains(keyword, StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(keyword => keyword, StringComparer.Ordinal)
                .ToArray();
        }

        public static Domain ClassifyByKeywords(string text)
        {
            foreach (var domain in new[] { Domain.BUG, Domain.REFACTOR, Domain.PLAN, Domain.ARCH })
                if (MatchedKeywords(text, domain).Length > 0)
                    return domain;
            return Domain.CODE_Q;
        }

        // ── Детерминированная цепочка потребителя, дословный перенос ─────────
        // Источник: 23.SemanticCompilerAdapter/DomainClassifierUnit.cs, строки
        // 158-182 (цепочка), 184-222 (близость предикатов и разрыв ничьей),
        // 224-250 (признаки текста), 252-267 (уверенность).

        private static Domain DomainForPredicateSlot(int slot)
        {
            if (slot == 0 || slot == 2 || slot == 3 || slot == 4 || slot == 21) return Domain.ARCH;
            if (slot == 5 || slot == 8 || slot == 9 || slot == 10 || slot == 19) return Domain.PLAN;
            if (slot == 1 || slot == 6 || slot == 7) return Domain.BUG;
            if (slot == 12 || slot == 13 || slot == 14) return Domain.CODE_Q;
            return Domain.UNKNOWN;
        }

        private static int DomainTieBreakRank(Domain domain)
        {
            if (domain == Domain.BUG) return 0;
            if (domain == Domain.REFACTOR) return 1;
            if (domain == Domain.PLAN) return 2;
            if (domain == Domain.ARCH) return 3;
            if (domain == Domain.CODE_Q) return 4;
            return 5;
        }

        private static Domain ClassifyByPredicateAffinity(int[] predicates)
        {
            var votes = predicates
                .Select(DomainForPredicateSlot)
                .Where(domain => domain != Domain.UNKNOWN)
                .GroupBy(domain => domain)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => DomainTieBreakRank(group.Key))
                .FirstOrDefault();
            return votes == null ? Domain.UNKNOWN : votes.Key;
        }

        private static bool ContainsAny(string text, params string[] terms)
        {
            return terms.Any(term => text.Contains(term, StringComparison.Ordinal));
        }

        private static bool HasRuntimeFailureSignal(string prompt)
        {
            var lower = (prompt ?? "").ToLowerInvariant();
            return ContainsAny(lower, "fix", "error", "exception", "fail", "failed", "failure",
                "wrong", "broken", "debug", "crash", "bug", "runtime error", "null reference");
        }

        private static bool HasDirectQuestionSignal(string prompt)
        {
            var lower = (prompt ?? "").ToLowerInvariant();
            return ContainsAny(lower, "what does", "what happens", "what pattern", "how does",
                "show ", "list ", "find ", "where ", "which ", "does ", "return ");
        }

        private static double CalculateBaseConfidence(string prompt, Domain domain, int evidenceCount)
        {
            if (domain == Domain.UNKNOWN) return 0.20;
            if (string.IsNullOrWhiteSpace(prompt)) return 0.50;
            if (evidenceCount >= 3) return 0.95;
            if (evidenceCount == 2) return 0.90;
            if (evidenceCount == 1) return 0.82;
            return 0.20;
        }

        // Полный ответ потребителя на один запрос. Контекстные пакеты не
        // передаются ни одной строкой панелей, поэтому их ветвь не исполняется.
        public static ConsumerAnswer Consume(
            List<Tuple<int[], Domain>> train, int k, int[] predicates, string text, int variant)
        {
            Domain baseDomain;
            string branch;
            bool usedKnn = false;
            bool usedAffinity = false;

            // Ветвь 1: обученная перегрузка, если память есть и профиль непуст.
            var classified = Classify(train, k, predicates, variant);
            if (train.Count > 0 && predicates != null && predicates.Length > 0 && classified.IsSuccess)
            {
                baseDomain = classified.Value;
                branch = "обученная память";
                usedKnn = true;
            }
            // Ветвь 2: признак прямого вопроса в тексте.
            else if (HasDirectQuestionSignal(text))
            {
                baseDomain = Domain.CODE_Q;
                branch = "прямой вопрос";
            }
            // Ветвь 3: признак сбоя времени выполнения в тексте.
            else if (HasRuntimeFailureSignal(text))
            {
                baseDomain = Domain.BUG;
                branch = "признак сбоя";
            }
            // Ветвь 4: близость предикатов, если она что-то называет.
            else if (predicates != null && predicates.Length > 0 &&
                     ClassifyByPredicateAffinity(predicates) != Domain.UNKNOWN)
            {
                baseDomain = ClassifyByPredicateAffinity(predicates);
                branch = "близость предикатов";
                usedAffinity = true;
            }
            // Ветвь 5: сопоставление по ключевым словам.
            else
            {
                baseDomain = ClassifyByKeywords(text);
                branch = "ключевые слова";
            }

            var keywordEvidence = MatchedKeywords(text, baseDomain).ToList();
            var evidence = keywordEvidence.ToList();
            if (predicates != null && predicates.Length > 0) evidence.Add("compiled_predicate_profile");
            if (usedKnn) evidence.Add("trained_predicate_knn");
            if (usedAffinity) evidence.Add("predicate_affinity");

            var confidence = CalculateBaseConfidence(text, baseDomain, keywordEvidence.Count);
            if (usedKnn) confidence = Math.Max(confidence, 0.90);
            else if (usedAffinity && keywordEvidence.Count == 0) confidence = Math.Min(confidence, 0.25);

            return new ConsumerAnswer
            {
                Label = baseDomain,
                Confidence = Math.Round(Math.Min(Math.Max(confidence, 0.20), 0.99), 2),
                Branch = branch,
                Evidence = string.Join(", ", evidence.Distinct(StringComparer.OrdinalIgnoreCase))
            };
        }

        // Пустая память для панели необученности.
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

# Предупреждение о версии System.Runtime неизбежно и на поведение перенесённой
# логики не влияет; ошибки сборки при этом не глушатся.
Add-Type -TypeDefinition $code -ReferencedAssemblies $refs -IgnoreWarnings -WarningAction SilentlyContinue

$lessons = [OracleZeroSim.Oracle]::SeededLessons()
$k = 5
$names = @('прежний    ', 'новый      ', 'испорченный')

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

Write-Output "Панель 1. Признаки запросов без общих признаков: расстояние до каждого из семи уроков."
$zeroQueries = @(
    @{ Name = 'только слот 6, нейтральный текст'; P = @(6);     T = 'update the layout spacing' },
    @{ Name = 'слот 6, текст с прямым вопросом';  P = @(6);     T = 'show me the handler' },
    @{ Name = 'слот 6, текст о сбое';             P = @(6);     T = 'fix the broken handler' },
    @{ Name = 'слоты 11 и 20, нейтральный текст'; P = @(11,20); T = 'update the layout spacing' }
)
foreach ($q in $zeroQueries) {
    $line = ""
    foreach ($lesson in $lessons) {
        $line += ("{0:F3} " -f [OracleZeroSim.Oracle]::JaccardDist([int[]]$q.P, $lesson.Item1))
    }
    Write-Output ("  {0,-34} : {1}" -f $q.Name, $line.Trim())
}
Check "лучшее расстояние у запроса без общих признаков равно единице" "1.000" ("{0:F3}" -f ([OracleZeroSim.Oracle]::SeededLessons() | ForEach-Object { [OracleZeroSim.Oracle]::JaccardDist([int[]]@(6), $_.Item1) } | Measure-Object -Minimum).Minimum)
Write-Output ""

Write-Output "Панель 2. Ответ КЛАССИФИКАТОРА на запросах без общих признаков."
foreach ($q in $zeroQueries) {
    $l = [OracleZeroSim.Oracle]::Classify($lessons, $k, [int[]]$q.P, 0)
    $n = [OracleZeroSim.Oracle]::Classify($lessons, $k, [int[]]$q.P, 1)
    Write-Output ("  {0,-34} : прежний {1} | новый {2}" -f $q.Name, $l, $n)
}
Check "прежний: запрос без общих признаков получает тему первого урока как успех" "успех: BUG" ([OracleZeroSim.Oracle]::Classify($lessons, $k, [int[]]@(6), 0))
Check "новый:   тот же запрос получает названный отказ" "отказ: NO_EVIDENCE_FOR_DECISION" ([OracleZeroSim.Oracle]::Classify($lessons, $k, [int[]]@(6), 1))
Write-Output ""

Write-Output "Панель 3. ЦЕНА, измеренная: что возвращает ПОТРЕБИТЕЛЬ на тех же запросах."
foreach ($q in $zeroQueries) {
    $cl = [OracleZeroSim.Oracle]::Consume($lessons, $k, [int[]]$q.P, $q.T, 0)
    $cn = [OracleZeroSim.Oracle]::Consume($lessons, $k, [int[]]$q.P, $q.T, 1)
    Write-Output ("  {0}" -f $q.Name)
    Write-Output ("    прежний : {0}" -f $cl)
    Write-Output ("    новый   : {0}" -f $cn)
}
Check "ветвь на нейтральном тексте со слотом 6" "близость предикатов" ([OracleZeroSim.Oracle]::Consume($lessons, $k, [int[]]@(6), 'update the layout spacing', 1).Branch)
Check "ветвь на тексте с прямым вопросом" "прямой вопрос" ([OracleZeroSim.Oracle]::Consume($lessons, $k, [int[]]@(6), 'show me the handler', 1).Branch)
Check "ветвь на тексте о сбое" "признак сбоя" ([OracleZeroSim.Oracle]::Consume($lessons, $k, [int[]]@(6), 'fix the broken handler', 1).Branch)
Check "ветвь на неизвестных слотах и нейтральном тексте" "ключевые слова" ([OracleZeroSim.Oracle]::Consume($lessons, $k, [int[]]@(11,20), 'update the layout spacing', 1).Branch)
Check "прежняя ветвь на всех четырёх была обученной памятью" "обученная память" ([OracleZeroSim.Oracle]::Consume($lessons, $k, [int[]]@(11,20), 'update the layout spacing', 0).Branch)
Write-Output ""

Write-Output "Панель 4. КОНТРОЛЬ: запросы, у которых есть хотя бы один общий признак. Ни один ответ не должен измениться."
$overlapQueries = @(
    @{ Name = 'набор первого урока';   P = @(0,1,2) },
    @{ Name = 'набор второго урока';   P = @(1) },
    @{ Name = 'набор третьего урока';  P = @(5,8,9,10,19) },
    @{ Name = 'набор четвёртого урока';P = @(0,2) },
    @{ Name = 'набор пятого урока';    P = @(0,2,3,4,21) },
    @{ Name = 'набор шестого урока';   P = @(12,13,14) },
    @{ Name = 'набор седьмого урока';  P = @(5,7) },
    @{ Name = 'частичное пересечение по слоту 1';  P = @(1,11) },
    @{ Name = 'частичное пересечение по слоту 12'; P = @(12,11) }
)
$sameClassifier = 0
$sameConsumer = 0
foreach ($q in $overlapQueries) {
    $l = [OracleZeroSim.Oracle]::Classify($lessons, $k, [int[]]$q.P, 0)
    $n = [OracleZeroSim.Oracle]::Classify($lessons, $k, [int[]]$q.P, 1)
    $cl = [OracleZeroSim.Oracle]::Consume($lessons, $k, [int[]]$q.P, 'update the layout spacing', 0)
    $cn = [OracleZeroSim.Oracle]::Consume($lessons, $k, [int[]]$q.P, 'update the layout spacing', 1)
    if ("$l" -eq "$n") { $sameClassifier++ }
    if ("$cl" -eq "$cn") { $sameConsumer++ }
    Write-Output ("  {0,-30} : классификатор {1} | потребитель {2}" -f $q.Name, $n, $cn)
}
Check "ответов классификатора не изменилось, из девяти" 9 $sameClassifier
Check "ответов потребителя не изменилось, из девяти" 9 $sameConsumer
Write-Output ""

Write-Output "Панель 5. Контроль необученности: порядок трёх проверок не менялся."
$empty = [OracleZeroSim.Oracle]::NoLessons()
Check "необученный классификатор на пустом запросе отвечает прежним кодом" "отказ: NOT_TRAINED" ([OracleZeroSim.Oracle]::Classify($empty, $k, [int[]]@(), 1))
Check "необученный классификатор на запросе с признаками отвечает прежним кодом" "отказ: NOT_TRAINED" ([OracleZeroSim.Oracle]::Classify($empty, $k, [int[]]@(6), 1))
Check "пустой запрос на обученной памяти отвечает тем же новым кодом, что и прежде" "отказ: NO_EVIDENCE_FOR_DECISION" ([OracleZeroSim.Oracle]::Classify($lessons, $k, [int[]]@(), 1))
Write-Output ""

Write-Output "Панель 6. Чувствительность: испорченный вариант сравнивает ПЕРВОЕ расстояние вместо лучшего."
$sens = 0
$mutCases = @(
    @{ Name = 'набор шестого урока'; P = @(12,13,14) },
    @{ Name = 'набор третьего урока'; P = @(5,8,9,10,19) },
    @{ Name = 'набор седьмого урока'; P = @(5,7) }
)
foreach ($q in $mutCases) {
    $n = [OracleZeroSim.Oracle]::Classify($lessons, $k, [int[]]$q.P, 1)
    $m = [OracleZeroSim.Oracle]::Classify($lessons, $k, [int[]]$q.P, 2)
    if ("$n" -ne "$m") {
        $sens++
        Write-Output ("  упало  {0}: у нового {1}, у испорченного {2}" -f $q.Name, $n, $m)
    } else {
        Write-Output ("  совпало {0}: оба дают {1}" -f $q.Name, $n)
    }
}
Check "испорченная логика обязана уронить хотя бы одну сверку" $true ($sens -gt 0)
Write-Output ("  испорченная логика уронила сверок: {0} из 3" -f $sens)
Write-Output ""
Write-Output "Панель 6, честная граница. Испорченный вариант отличается только там, где"
Write-Output "ПЕРВЫЙ урок памяти не разделяет с запросом ни одного признака, а какой-то"
Write-Output "другой разделяет. На запросе со слотом 1 первый урок пересекается, поэтому"
Write-Output "испорченный вариант там неотличим от нового — и это измерено ниже."
$n1 = [OracleZeroSim.Oracle]::Classify($lessons, $k, [int[]]@(1), 1)
$m1 = [OracleZeroSim.Oracle]::Classify($lessons, $k, [int[]]@(1), 2)
Check "на запросе со слотом 1 испорченный вариант неотличим от нового" "$n1" "$m1"
Write-Output ""

if ($failures -eq 0) {
    Write-Output ("ИТОГ: все {0} сверок прошли. Правка воспроизводит назначенное поведение, испорченная логика отличается." -f $checks)
    exit 0
} else {
    Write-Output ("ИТОГ: сверок {0}, упало {1}." -f $checks, $failures)
    exit 1
}
