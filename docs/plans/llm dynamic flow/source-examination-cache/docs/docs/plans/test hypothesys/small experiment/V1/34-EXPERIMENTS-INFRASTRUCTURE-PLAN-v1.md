# 34.Experiments — Infrastructure Fix Plan
## Подпроект 34: три недостающих компонента для замыкания цикла измерений

**Проект:** llm-mvp-core — подпроект 34.Experiments  
**Язык:** C# / .NET 9  
**Внешние пакеты:** только Microsoft.CodeAnalysis.CSharp (Roslyn, уже используется в подпроекте 13)  
**Документ:** план исправления — не реализация  
**Этот документ не касается XIIGen.**

---

## Что построил Claude Code — и чего не хватает

Claude Code реализовал шесть алгоритмов (E01–E06) и они работают корректно: 544 теста зелёные. Проблема не в алгоритмах — проблема в том что цикл измерения не замкнут.

**Три отсутствующих компонента:**

```
Что есть:
  E01..E06 — алгоритмы, принимают List<int> tokens снаружи ✅
  ExperimentsDispatcher — маршрутизатор ✅
  BigramMarkov, TrigramMarkov — утилиты ✅
  ExperimentResult — тип результата ✅

Чего нет:
  ❌ DatasetLoader  — никто не читает .cs файлы из проекта
  ❌ ResultWriter   — результаты остаются в памяти, JSON/MD не пишутся
  ❌ ExperimentRunner — нет механизма нарастающей нагрузки (Scale Ladder)
  ❌ MemoryMb = 0  — замер памяти не реализован
```

Следствие: эксперимент запустить нельзя без написания кода снаружи.  
Ни одного файла `RESULT-*.json` или `RESULT-*.md` не создаётся.

---

## Папка Infrastructure — четыре новых файла

```
34.Experiments/
├── Infrastructure/                  ← новая папка
│   ├── DatasetLoader.cs             — читает .cs файлы по тирам нагрузки
│   ├── ExperimentRunner.cs          — связывает загрузку + алгоритм + запись
│   ├── ResourceMonitor.cs           — замер памяти (GC-based, без внешних инструментов)
│   └── ResultWriter.cs              — пишет JSON + MD после каждого прогона
└── (всё остальное без изменений)
```

Плюс: патч шести файлов `Experiments/E0*.cs` — заменить `MemoryMb = 0` на реальный замер.

---

## Фаза 1 — DatasetLoader (1 день)

**Что делает:** читает `.cs` файлы проекта из нужных подпапок и возвращает их содержимое как `DatasetContents`.

**Шесть тиров нагрузки:**

| Scale | Название | Что читает | ~Токенов |
|---|---|---|---|
| 0 | Markov-micro | BpeTokenizer.cs (один файл) | ~1 000 |
| 1 | Markov-small | 01.Tokenizer весь подпроект | ~9 100 |
| 2 | Markov-medium | 01 + 02 + 05 + 07 + 09 | ~30 000 |
| 3 | Probe-small | то же что Scale 1 | ~9 100 |
| 4 | Probe-medium | то же что Scale 2 | ~30 000 |
| 5 | Full | весь проект (00–14) | ~141 000 |

**Входной параметр:** `(int scale, string projectRoot)` — путь к корню `llm_mvp_core`.  
**Выходной тип:** `OperationResult<DatasetContents>` где `DatasetContents` содержит:
- список путей к файлам
- содержимое каждого файла
- весь текст склеенный в одну строку (для E01–E05)
- число символов и приблизительное число токенов

**Правило фильтрации:** пропускать файлы из `/bin/`, `/obj/`, `*.g.cs`, `*.generated.cs`.

**Gate A:** `dotnet build` без ошибок, тесты зелёные.  
**Gate B:** `Load(scale: 0, projectRoot)` возвращает один файл с текстом; `Load(scale: 1)` возвращает 9+ файлов.

---

## Фаза 2 — ResourceMonitor (0.5 дня)

**Что делает:** замеряет память выделенную алгоритмом (сверх базовой линии процесса) и время.

**Почему GC-based:** `GC.GetTotalMemory()` работает без внешних пакетов, без прав администратора, на любой ОС.

**Интерфейс:** `Start()` / `Stop()` → возвращает `ResourceSnapshot { ElapsedMs, PeakMemoryMb }`.  
Перед `Start()` вызывается `GC.Collect()` для очистки базовой линии.

**Патч шести файлов:** заменить `MemoryMb = 0` на `GC.GetTotalMemory(false) / 1_048_576.0`.  
Одна строка в каждом файле, два вхождения (baseline и enhanced).

**Gate A:** компилируется.  
**Gate B:** `monitor.Start(); AllocateSomeMb(10); var s = monitor.Stop();` → `s.PeakMemoryMb > 5`.

---

## Фаза 3 — ResultWriter (1 день)

**Что делает:** получает `ExperimentResult` + `DatasetContents`, пишет два файла:

```
experiments/results/RESULT-{EXX}-scale{N}.json   ← машиночитаемый
experiments/results/RESULT-{EXX}-scale{N}.md     ← человекочитаемый таблица
experiments/results/progress.md                  ← сводка всех прогонов
experiments/results/progress.json
```

**Формат JSON:** плоский объект с полями `experiment_id`, `scale`, `tier_name`, `dataset.*`, `baseline.*`, `enhanced.*`, `delta.*`, `passed`, `next_action`, `notes`. Соответствует `experiment-result.schema.json` из папки `docs/experiments/schema/`.

**Формат MD:** одна таблица сравнения:

```markdown
| Метрика | Baseline | Enhanced | Δ |
|---|---|---|---|
| Точность | 34.2% | 41.8% | +22.2% ✅ |
| Время | 45 мс | 48 мс | медленнее 6.7% ⚠️ |
| Память | 87 МБ | 12 МБ | меньше на 86.2% ✅ |
```

Знаки: `✅` = улучшение ≥ 5%, `⚠️` = улучшение 0–5% или нейтрально, `❌` = ухудшение.

**Директория создаётся автоматически** если не существует.

**Gate A:** компилируется, тесты зелёные.  
**Gate B:** после `Write(result, dataset)` файлы существуют на диске и корректно парсятся как JSON.

---

## Фаза 4 — ExperimentRunner (1 день)

**Что делает:** замыкает цепочку. Принимает имя эксперимента и Scale, сам делает всё остальное.

**Два метода:**

```
Run(experimentId, scale, projectRoot)
  → Load данные → Токенизировать → Запустить E0N → Исправить Scale в result → Write → Return

RunScaleLadder(experimentId, projectRoot, maxScale = 5)
  → Run(scale=0) → если Passed: Run(scale=1) → ... → остановиться при < 5%
  → после каждого Scale: обновить progress.md
```

**Токенизация:** символьный уровень (ASCII коды 0–127) — не требует BPE модели, достаточно для Марковских экспериментов.

**Исправление Scale:** эксперименты возвращают `Scale = 0` хардкодом. `ExperimentRunner` перезаписывает: `result with { Scale = scale }`.

**Вывод в консоль** после каждого Scale (для запуска вручную):
```
E06 Scale 0: Markov-micro (~1 000 токенов)
  Baseline accuracy:  42.1%
  Enhanced accuracy:  91.3%
  Delta:              +117.1%
  Result:             PROMOTE_TO_NEXT_SCALE
```

**Gate A:** компилируется, тесты зелёные.  
**Gate B:** `runner.Run("E06", scale: 0, projectRoot)` → файл `RESULT-E06-scale0.json` существует, `passed` корректен.  
**Gate C:** явное одобрение от Лубы перед Gate B.

---

## Проверка плана

### Все ли компоненты покрыты?

| Проблема | Решение | Фаза |
|---|---|---|
| Нет загрузки файлов | DatasetLoader | Фаза 1 |
| MemoryMb = 0 | ResourceMonitor + патч | Фаза 2 |
| Нет JSON/MD вывода | ResultWriter | Фаза 3 |
| Scale всегда = 0 | ExperimentRunner | Фаза 4 |
| Нет нарастающей нагрузки | RunScaleLadder() | Фаза 4 |

### DNA правила

| Правило | Проверка |
|---|---|
| Правило 1 (нет типизированных моделей) | DatasetContents, ResourceSnapshot — sealed record с примитивными полями ✓ |
| Правило 3 (OperationResult везде) | DatasetLoader.Load, ResultWriter.Write → OperationResult<T> ✓ |
| Правило 7 (идемпотентность) | Имя файла включает Scale → повторный прогон создаёт новый файл ✓ |
| Правило 8 (хранить перед событием) | ResultWriter.Write пишет на диск до обновления progress ✓ |

### Что не меняется

Шесть файлов `E01–E06`, `BigramMarkov`, `TrigramMarkov`, `ExperimentsDispatcher`, `ExperimentTypes` — без изменений в логике. Только патч `MemoryMb = 0` в шести местах.

---

## Оценка времени

| Фаза | Дней |
|---|---|
| Фаза 1 — DatasetLoader | 1 |
| Фаза 2 — ResourceMonitor + патч MemoryMb | 0.5 |
| Фаза 3 — ResultWriter | 1 |
| Фаза 4 — ExperimentRunner | 1 |
| **Итого** | **3.5 дня** |

---

⛔ STOP — жду одобрения плана перед реализацией.

*34.Experiments Infrastructure Fix Plan v1.0 — не касается XIIGen*
