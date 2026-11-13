# 35.ExperimentsV2 — Диагноз и план исправления
## Почему результат непонятен — и что нужно добавить

**Проект:** llm-mvp-core — подпроект 35.ExperimentsV2  
**Этот документ не касается XIIGen.**

---

## Что хорошо в V2

Алгоритмы реализованы правильно. В сравнении с V1 (34.Experiments):
- Метрики конкретнее: перплексия, MRR, Recall@3 — не просто «точность»
- У каждого эксперимента своя ось масштабирования (насыщение словаря, confidence ratio, Gini и т.д.)
- Планы написаны хорошо: README, отдельный файл на каждый эксперимент

---

## Диагноз: четыре отсутствующих компонента

### Проблема 1 — Нет загрузки файлов (та же что в V1)

Каждый эксперимент принимает готовые данные от вызывающего:

```
E01: string csharpSource       ← кто читает файл?
E02: IReadOnlyList<string>      ← кто собирает список текстов?
E03: IReadOnlyList<int>         ← кто токенизирует?
E04: IReadOnlyList<(Id, Source)> + IReadOnlyList<(Query, ExpectedId)>  ← кто строит пары?
E05: string csharpSource       ← кто читает файл?
E06: IReadOnlyList<(Caller, Callee)>  ← кто извлекает граф вызовов?
```

Никто. Эксперименты — это «двигатели» без «коробки передач».

### Проблема 2 — Разные форматы входа требуют разных адаптеров

V2 сложнее V1 в этом месте: каждый эксперимент ждёт данные в особом формате.
E04 ждёт готовые пары «запрос → ожидаемый файл». E06 ждёт рёбра графа вызовов.
Нельзя просто «передать текст» — нужен отдельный адаптер для каждого эксперимента.

### Проблема 3 — Нет JSON/MD вывода (та же что в V1)

`ExperimentV2Result` возвращается из метода в память. Ни одного файла не пишется.

### Проблема 4 — Tier всегда = 1

Все шесть файлов содержат `Tier = 1` хардкодом. Нет механизма нарастающей нагрузки.  
Дополнительно: README говорит что E04 «начинает с Tier 2» — но в коде это не проверяется.

---

## Что добавить: три файла + патч

```
35.ExperimentsV2/
├── Infrastructure/                     ← новая папка (три файла)
│   ├── DatasetLoader.cs                — читает .cs файлы по тирам
│   ├── InputAdapters.cs                — готовит нужный формат для каждого эксперимента
│   └── ResultWriter.cs                 — пишет JSON + MD после каждого прогона
└── ExperimentRunner.cs                 ← новый файл (точка входа)
```

Плюс: патч `Tier = 1` во всех шести `E0*.cs` → `Tier = tier` (передаётся снаружи).

---

## Фаза 1 — DatasetLoader (1 день)

**Одинаковый с V1 по смыслу.** Читает `.cs` файлы по тирам.

| Тир | Что читает | Файлов | ~Токенов |
|---|---|---|---|
| 1 | `01.Tokenizer` | ~3 файла | ~2 000 |
| 2 | 6 малых подпроектов | ~47 файлов | ~9 100 |
| 3 | Все 24 подпроекта | ~195 файлов | ~141 000 |

Выходной тип: `DatasetContents` — список пар `(путь, текст)` + склеенный текст.

**Gate A:** компилируется, тесты зелёные.  
**Gate B:** `Load(tier: 1)` возвращает 3+ файла; `Load(tier: 2)` возвращает 47+.

---

## Фаза 2 — InputAdapters (1.5 дня)

**Это новое по сравнению с V1** — потому что у V2 разные форматы входа.

Шесть статических методов, каждый принимает `DatasetContents` и возвращает готовый вход для своего эксперимента:

```
AdaptForE01(DatasetContents) → string csharpSource
  Берёт CombinedText — весь код проекта как одна строка.

AdaptForE02(DatasetContents) → IReadOnlyList<string>
  Возвращает список текстов файлов — каждый файл отдельно (E02 ищет вызовы методов).

AdaptForE03(DatasetContents) → IReadOnlyList<int>
  BPE-токенизирует CombinedText через BpeTokenizer из подпроекта 01.

AdaptForE04(DatasetContents) → (documents, queries)
  documents = список (имя_файла, текст_файла)
  queries   = строится автоматически: для каждого вызова метода из другого файла
              → запрос = имя метода, ожидаемый документ = файл где он определён
  Примечание: при Tier 1 (<20 файлов) возвращает предупреждение что E04 нужен Tier 2+.

AdaptForE05(DatasetContents) → string csharpSource
  Берёт CombinedText (тот же что E01).

AdaptForE06(DatasetContents) → IReadOnlyList<(Caller, Callee)>
  Строит граф вызовов через Roslyn: для каждого файла находит
  InvocationExpressionSyntax → пара (имя_вызывающего_метода, имя_вызываемого).
```

**Почему адаптеры важны:** без них каждый пользователь должен сам написать 6 разных кусков кода подготовки данных. С адаптерами — одна строка на эксперимент.

**Gate A:** компилируется.  
**Gate B:** `AdaptForE04(tier1Data)` возвращает предупреждение; `AdaptForE04(tier2Data)` возвращает ≥20 документов и ≥10 запросов.

---

## Фаза 3 — ResultWriter (1 день)

**Аналогичен V1 ResultWriter, но под новый тип `ExperimentV2Result`.**

После каждого прогона записывает два файла:
```
experiments/E01-AST-TOKENIZER/results/run-YYYY-MM-DD-001.json
experiments/E01-AST-TOKENIZER/results/run-YYYY-MM-DD-001.md
```

**JSON** — плоский объект по схеме из `experiment-result-v1.json`:
```json
{
  "experiment_id": "E01-AST-TOKENIZER",
  "tier": 1,
  "run_at": "2026-04-30T12:00:00Z",
  "primary_metric": "prediction_error",
  "baseline_value": 0.312,
  "enhanced_value": 0.255,
  "improvement_pct": 18.3,
  "passed": true,
  "next_action": "PROMOTE_TO_NEXT_TIER",
  "scaling_signal": 0.04,
  "scaling_signal_note": "vocab_saturation_ratio — AST близко к насыщению",
  "baseline_time_ms": 45,
  "enhanced_time_ms": 51,
  "notes": "..."
}
```

**MD** — таблица сравнения + строка итога + строка что делать дальше.

Дополнительно: обновляет `experiments/progress/progress.json` и `progress.md`.

**Gate B:** после `Write(result)` файлы существуют и корректно парсятся через `JsonDocument`.

---

## Фаза 4 — ExperimentRunner + патч Tier (1 день)

**ExperimentRunner** — точка входа, связывает все компоненты:

```
Run(experimentId, tier, projectRoot):
  1. DatasetLoader.Load(tier, projectRoot)
  2. InputAdapters.AdaptFor{EXX}(dataset)
  3. Dispatcher.RunE{XX}(adaptedInput)
  4. result with { Tier = tier }   ← патч хардкодного Tier = 1
  5. ResultWriter.Write(result)
  6. Return result

RunTierLadder(experimentId, projectRoot):
  → Run(tier=1) → если Passed → Run(tier=2) → ...
  → остановка при первом TUNE_OR_ABANDON
```

**Особая логика для E04:** если tier < 2 → вернуть `OperationResult.Fail("E04 требует Tier 2+, сейчас <20 файлов")` без запуска.

**Патч `Tier = 1`** во всех шести `Experiments/E0*.cs`: это один коммит, шесть строк.

**Gate B:** `runner.Run("E01", tier: 1, projectRoot)` → файл `run-YYYY-MM-DD-001.json` существует, `passed` и `improvement_pct` корректны.

---

## Порядок фаз и зависимости

```
Фаза 1 (DatasetLoader)
    ↓
Фаза 2 (InputAdapters)   ← зависит от DatasetLoader
    ↓
Фаза 3 (ResultWriter)    ← можно параллельно с Фазой 2
    ↓
Фаза 4 (ExperimentRunner) ← зависит от 1 + 2 + 3
```

---

## Проверка плана

### Покрыты ли все четыре проблемы?

| Проблема | Решение | Фаза |
|---|---|---|
| Нет загрузки файлов | DatasetLoader | 1 |
| Разные форматы входа | InputAdapters | 2 |
| Нет JSON/MD вывода | ResultWriter | 3 |
| Tier всегда = 1 | патч + ExperimentRunner | 4 |

### DNA правила

| Правило | Проверка |
|---|---|
| Правило 1 | DatasetContents — sealed record с примитивами ✓ |
| Правило 3 | DatasetLoader.Load, ResultWriter.Write → OperationResult<T> ✓ |
| Правило 7 | Имя файла включает дату и порядковый номер → идемпотентно ✓ |
| Правило 8 | ResultWriter пишет JSON на диск до обновления progress ✓ |

### Что не меняется

Шесть файлов `E01–E06`, `NgramModel`, `ResultTypes`, `ExperimentsV2Dispatcher` — без изменений в логике. Только патч `Tier = 1` в шести местах.

---

## Оценка времени

| Фаза | Дней |
|---|---|
| 1 — DatasetLoader | 1 |
| 2 — InputAdapters | 1.5 |
| 3 — ResultWriter | 1 |
| 4 — ExperimentRunner + патч | 1 |
| **Итого** | **4.5 дня** |

---

⛔ STOP — жду одобрения плана.

*35.ExperimentsV2 Infrastructure Plan v1.0 — не касается XIIGen*
