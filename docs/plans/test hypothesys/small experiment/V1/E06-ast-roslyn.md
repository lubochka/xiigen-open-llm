# E06 — AST узлы Roslyn vs плоский BPE текст
## Токенизация через синтаксическое дерево C# вместо BPE
**Проект:** llm-mvp-core · **Этот документ не касается XIIGen.**

---

## Гипотеза
BPE видит: ["public", " bool", " Validate", "(", "string", " token", ")"]
Roslyn видит: [MethodDeclaration, ReturnType, Identifier, ParameterList, Parameter]

AST токены:
  - Словарь ~40 типов (против ~800 BPE на один .cs файл)
  - Грамматика C# детерминирована: за MethodDeclaration ПОЧТИ ВСЕГДА следует ParameterList
  - Матрица переходов 40×40 = 1600 ячеек vs 800×800 = 640000

Это самый важный эксперимент. Если Марков на AST типах почти детерминированный
(accuracy 80–95%) — это доказывает что C# код имеет глубокую структуру
которую BPE полностью игнорирует.

Источник: Aider repo-map + Roslyn AST из подпроектов 13 и 23.

---

## Нарастающие уровни нагрузки (Scale Ladder)

Каждый эксперимент проходит три уровня. Переход разрешён только если предыдущий дал ≥5% улучшения.

| Уровень | Название | Датасет | Время прогона | Инструмент |
|---|---|---|---|---|
| Scale 0 | Markov-micro | 1 файл (~1 000 токенов) | < 10 мс | Марковская цепь |
| Scale 1 | Markov-small | 1 подпроект (~9 000 токенов) | < 100 мс | Марковская цепь |
| Scale 2 | Markov-medium | 5 подпроектов (~45 000 токенов) | < 500 мс | Марковская цепь |
| Scale 3 | Probe-net | 1 подпроект + зонд-сеть 2L×64d | < 3 мин | Зонд-нейросеть |
| Scale 4 | Probe-medium | 5 подпроектов + зонд-сеть | < 15 мин | Зонд-нейросеть |
| Scale 5 | Full | Весь проект + трансформер | часы | Подпроекты 03+04 |

**Правило остановки:** если Scale N дал < 5% по всем метрикам — эксперимент помечается ABANDON.
Не запускаем следующий уровень зря.

## Датасетные тиры

```
Scale 0 (Markov-micro):   BpeTokenizer.cs                   ~1 000 токенов
Scale 1 (Markov-small):   01.Tokenizer/                     ~9 100 токенов
Scale 2 (Markov-medium):  01 + 02 + 05 + 07 + 09            ~30 000 токенов
Scale 3 (Probe-net):      01.Tokenizer + зонд (2L, 64d)     ~9 100 токенов
Scale 4 (Probe-medium):   01+02+05+07+09 + зонд              ~30 000 токенов
Scale 5 (Full):           весь проект + трансформер          ~141 000 токенов
```

---

## Что меняется

**Baseline (оригинал):** BPE токенизация .cs файлов. Словарь ~800 уникальных токенов на файл. Матрица 640000 ячеек.

**Enhanced (улучшение):** CSharpSyntaxTree.ParseText → типы узлов как токены. Словарь ~40 SyntaxKind. Матрица 1600 ячеек.

**Использует подпроекты:** 01.Tokenizer, 13.CodeOptimizer (Roslyn), 23.OssEquivalents (RoslynCodeAnalyzer)

---

## Марковская реализация (Scale 0–2)

```csharp
// E06: самый важный — AST узлы vs BPE текст
// baseline:  стандартные BPE токены
var bpeTokens = BpeTokenizer.EncodeToInts(File.ReadAllText(filePath));

// enhanced:  типы AST узлов через Roslyn
// SyntaxKind.MethodDeclaration = 8651 → нормализуем в 0..39
var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath));
var astTokens = syntaxTree.GetRoot().DescendantNodes()
    .Select(n => (int)n.Kind())   // SyntaxKind enum value
    .ToArray();

// Нормализация: SyntaxKind values → 0..39
var uniqueKinds = astTokens.Distinct().OrderBy(x => x).ToList();
var kindToIndex = uniqueKinds.Select((k,i) => (k,i)).ToDictionary(x => x.k, x => x.i);
var normalizedAst = astTokens.Select(k => kindToIndex[k]).ToArray();

// Марков на normalizedAst → матрица ~40×40 = 1600 ячеек
// Марков на bpeTokens    → матрица ~800×800 = 640000 ячеек
```

---

## Примечания по каждому уровню

**Scale 0:** BpeTokenizer.cs один файл: AST ~200 узлов (~40 типов), BPE ~150 токенов (~800 уникальных). Матрица 1600 vs 640000.

**Scale 1:** 01.Tokenizer (9 файлов): AST все файлы. Типов узлов ~55 (больше типов на больше файлах). Матрица ~3000 vs 640000.

**Scale 2:** 5 подпроектов: AST типов ~70. Матрица ~5000 vs 640000. Сравниваем accuracy.

**Scale 3:** Зонд: embedding на SyntaxKind вместо BPE. Словарь 70 вместо 10000 → в 143 раза меньше embed таблица.

**Scale 4:** Зонд 5 подпроектов: подтверждаем что малый словарь AST не теряет точность.

**Scale 5:** Полный трансформер: AST трансформер для генерации кода. Финальный тест.

---

## Ожидаемые результаты на Scale 0

| Метрика | Ожидаемое улучшение |
|---|---|
| accuracy | +40 до +60% |
| time | +8 мс на файл (Roslyn разбор) |
| memory | -99.7% |
| cpu | -99% |
| disk | -99% |

---

## Альтернативный подход

Вместо SyntaxKind типов использовать роли в AST: «левая часть присваивания», «условие if», «возвращаемое значение». Смысловые роли vs структурные типы.

---

## Условие остановки (ABANDON)

Если accuracy AST < accuracy BPE — что-то неправильно с нормализацией узлов. Проверить что SyntaxKind.Token узлы фильтруются.

---

## Формат вывода: JSON + MD

Каждый прогон пишет два файла:

```
experiments/E06/
  RESULT-E06-scale{N}.json    ← машиночитаемый
  RESULT-E06-scale{N}.md      ← человекочитаемый
```

### JSON шаблон (заполняется автоматически)

```json
{
  "experiment_id": "E06",
  "scale": N,
  "dataset_tokens": 9100,
  "baseline": {
    "accuracy_pct": 34.2,
    "time_ms": 45,
    "memory_mb": 87,
    "cpu_ms": 120,
    "disk_kb": 0
  },
  "enhanced": {
    "accuracy_pct": 41.8,
    "time_ms": 48,
    "memory_mb": 12,
    "cpu_ms": 35,
    "disk_kb": 0
  },
  "delta": {
    "accuracy_pct": +22.2,
    "time_pct": -6.7,
    "memory_pct": +86.2,
    "cpu_pct": +70.8,
    "disk_pct": 0.0
  },
  "passed": true,
  "next_action": "PROMOTE_TO_SCALE_2",
  "notes": ""
}
```

### MD шаблон

```markdown
# E06 scale {N} — [название]
**Датасет:** ~9 100 токенов | **Уровень:** Markov-small

| Метрика | Baseline | Enhanced | Δ |
|---|---|---|---|
| Точность | 34.2% | 41.8% | **+22.2% ✅** |
| Время | 45 мс | 48 мс | -6.7% ✅ |
| Память | 87 МБ | 12 МБ | **+86.2% ✅** |
| CPU | 120 мс | 35 мс | **+70.8% ✅** |
| Диск | 0 КБ | 0 КБ | 0% ✅ |

**Итог: PASSED → PROMOTE_TO_SCALE_2**
```

---

## Связанные документы

- `EXPERIMENT-HARNESS-v2-MARKOV-PROXY.md` — Марковский фреймворк
- `EXPERIMENT-INDEX.md` — индекс всех шести экспериментов
- `E06-ast-roslyn/RESULT-E06-scale*.json` — результаты прогонов

*E06 v1.0 — микро-эксперименты · LlmFromScratch.sln*