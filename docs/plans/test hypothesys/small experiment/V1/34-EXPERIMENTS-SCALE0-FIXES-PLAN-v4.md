# Experiments — План Scale 2
## Что именно делать дальше

**Проект:** llm-mvp-core — подпроект 34.Experiments  
**Этот документ не касается XIIGen.**

---

## Состояние после Scale 0

| Эксп. | Scale 0 | Что делать дальше |
|---|---|---|
| E01 | ✅ +33.0% | Запустить Scale 1 |
| E02 | ✅ +11.0% | Запустить Scale 1 |
| E03 | ✅ +14.4% | Запустить Scale 1 |
| E04 | ⏭ SKIPPED | Запустить Scale 3 (нужно ~100K токенов) |
| E05 | ❌ -12.5% | Запустить Scale 2 (нужно ~30K токенов) |
| E06 | ✅ +30.4% | Запустить Scale 1 |

**progress.json устарел** — показывает старые результаты E01 (passed: false). Обновить.

---

## Задача 1 — Исправить DatasetLoader Scale 2

**Файл:** `Infrastructure/DatasetLoader.cs`

**Проблема:** текущий Scale 2 читает `01+02+05+07+09` = **~8 000 BPE токенов**.  
Это не достаточно для E05 (нужно 10+ наблюдений на ячейку матрицы кластеров).

**Вычисление:**

```
Текущий Scale 2:
  01.Tokenizer      ~1 566 BPE
  02.DatasetPipeline ~1 535 BPE
  05.Embeddings      ~1 015 BPE
  07.GraphEngine     ~1 937 BPE
  09.Inference       ~1 898 BPE
  ИТОГО:             ~7 953 BPE токена ← слишком мало

Нужный Scale 2 (≥30K токенов):
  00.TensorCore     ~12 968 BPE
  01.Tokenizer       ~1 566 BPE
  02.DatasetPipeline ~1 535 BPE
  03.TransformerModel ~4 346 BPE
  04.TrainingLoop    ~2 370 BPE
  05.Embeddings      ~1 015 BPE
  06.VectorSearch    ~2 699 BPE
  07.GraphEngine     ~1 937 BPE
  08.GraphRag        ~2 107 BPE
  ИТОГО:            ~30 543 BPE токена ✅
```

**Исправление в DatasetLoader.cs:**

```csharp
// Было:
[2] = new[] { "01.Tokenizer", "02.DatasetPipeline", "05.Embeddings",
              "07.GraphEngine", "09.Inference" },

// Нужно:
[2] = new[] { "00.TensorCore", "01.Tokenizer", "02.DatasetPipeline",
              "03.TransformerModel", "04.TrainingLoop", "05.Embeddings",
              "06.VectorSearch", "07.GraphEngine", "08.GraphRag" },
```

```csharp
// Обновить TierNames:
[2] = "Markov-medium (~30 000 токенов, 9 подпроектов 00–08)",
```

**Gate B:** `DatasetLoader.Load(2, projectRoot).Value.ApproxTokens > 25_000`.

---

## Задача 2 — Исправить DatasetLoader Scale 3 (для E04)

Добавить Scale 3 = весь проект (~129K BPE токенов).

**Почему E04 нужен Scale 3:**

PMI матрица с vocab=500 имеет 250 000 ячеек.  
Scale 2 даёт 24 000 обучающих пар = **0.1 наблюдения на ячейку** — вырожденная матрица.  
Scale 3 (полный проект) даёт 103 000 пар = **10 наблюдений на ячейку** — надёжно.

**E04 в ExperimentRunner:** изменить блокировку с 30K на 100K:

```csharp
// Было:
if (experimentId == "E04" && dataset.ApproxTokens < 30_000)

// Нужно:
if (experimentId == "E04" && dataset.ApproxTokens < 100_000)
    return OperationResult<ExperimentResult>.Fail(
        $"E04 требует Scale 3+ (~100K токенов для надёжной PMI матрицы). " +
        $"Текущий датасет: {dataset.ApproxTokens:N0} токенов. Запустить с scale: 3.");
```

**Обновить Scale 3 в DatasetLoader** (сейчас Scale 3 = Probe-small = 01.Tokenizer):

```csharp
// Было:
[3] = new[] { "01.Tokenizer" },   // Probe-small

// Нужно — Scale 3 теперь полный проект:
[3] = new[] { "00.TensorCore", "01.Tokenizer", "02.DatasetPipeline",
              "03.TransformerModel", "04.TrainingLoop", "05.Embeddings",
              "06.VectorSearch", "07.GraphEngine", "08.GraphRag",
              "09.Inference", "10.LoRA", "11.DPO", "12.SearchEngine",
              "13.CodeOptimizer", "14.MachineLearning" },
```

```csharp
[3] = "Full (~129 000 токенов, все подпроекты 00–14)",
```

**Gate B:** `DatasetLoader.Load(3, projectRoot).Value.ApproxTokens > 100_000`.

---

## Задача 3 — Обновить progress.json

**Файл:** `Infrastructure/ResultWriter.cs`, метод `UpdateProgress()`

**Проблема:** после нового прогона progress.json не обновился — показывает старые данные (E01 passed: false, timestamp 14:54).

**Исправление:** вызывать `_writer.UpdateProgress(results)` в конце каждого `Run()`, не только в `RunScaleLadder()`.

```csharp
// В ExperimentRunner.Run() после Step 6:
// Step 7: update progress summary after every run
_writer.UpdateProgress(new[] { result });  // append, не перезаписывать
```

Или проще: в `Program.cs` (точке запуска) явно вызывать после всех прогонов:
```csharp
writer.UpdateProgress(allResults);
```

**Gate B:** после запуска E01 Scale 1 — в progress.json появляется запись с `scale: 1` и правильным `accuracy_delta_pct`.

---

## Задача 4 — Запустить Scale 1 для E01, E02, E03, E06

Эти четыре прошли Scale 0. Нужно проверить что преимущество сохраняется при большем датасете.

**Scale 1 = `01.Tokenizer` целиком:**
- Файлов: 3 (BpeTokenizer.cs + RegexSplitter.cs + Tokenizer.cs)
- Символов: ~6 267
- BPE токенов (~vocabSize=500): ~1 500–2 000

**Точная команда для Claude Code:**
```csharp
var runner = new ExperimentRunner(
    projectRoot: @"путь\к\llm_mvp_core",
    outputDir:   @"experiments\results");

runner.Run("E01", scale: 1);
runner.Run("E02", scale: 1);
runner.Run("E03", scale: 1);
runner.Run("E06", scale: 1);
```

**Критерий перехода:** accuracy_delta ≥ 5% → переходим на Scale 2.

**Ожидаемые результаты Scale 1:**

| Эксп. | Scale 0 | Ожидание Scale 1 | Обоснование |
|---|---|---|---|
| E01 | +33.0% | +25–35% | K-Means на ~400 BPE токенах — больше данных, кластеры плотнее |
| E02 | +11.0% | +12–18% | Энтропия улучшается с объёмом |
| E03 | +14.4% | +12–18% | Тригамма надёжнее при большем корпусе |
| E06 | +30.4% | +30–40% | Больше AST узлов → плотнее матрица переходов |

---

## Задача 5 — Запустить Scale 2 для E05

**E05 (иерархическое предсказание)** провалился на Scale 0 потому что на 1 файле было только 0.5 наблюдения на ячейку матрицы кластеров.

**Scale 2 = 9 подпроектов (~30K BPE токенов):**

```
E05 при Scale 2:
  Обучающих пар: ~24 000
  Матрица кластеров: 50×50 = 2 500 ячеек
  Наблюдений на ячейку: ~10 ✅ (нужно ≥5)
```

**Точная команда для Claude Code:**
```csharp
runner.Run("E05", scale: 2);
```

**Ожидаемый результат Scale 2:**

| Метрика | Scale 0 | Ожидание Scale 2 |
|---|---|---|
| accuracy_delta | -12.5% ❌ | +3–8% |
| Обоснование | Матрица разрежена | 10 набл./ячейку — иерархия стабильна |

Если Scale 2 даёт менее +5% — попробовать K=20 вместо K=50 (меньше кластеров = плотнее матрица).

---

## Задача 6 — Запустить Scale 3 для E04

**E04 (PMI предсказатель)** нужен полный проект (~129K токенов).

**Точная команда для Claude Code:**
```csharp
runner.Run("E04", scale: 3);
```

**Ожидаемый результат Scale 3:**

| Метрика | Scale 0 | Scale 3 |
|---|---|---|
| Статус | SKIPPED | Запустится |
| Матрица PMI vocab=500 | 0.1 набл./ячейку | 10 набл./ячейку |
| accuracy_delta | — | +5–15% ожидаем |
| Обоснование | Вырожденная | PMI отфильтрует шумные переходы |

---

## Итоговый порядок выполнения

```
Шаг 1: Исправить DatasetLoader
        Scale 2 = 00-08 подпроекты (~30K токенов)
        Scale 3 = 00-14 подпроекты (~129K токенов)
        → Gate B для обоих

Шаг 2: Исправить блокировку E04 в ExperimentRunner
        ApproxTokens < 100_000 вместо < 30_000
        → Gate B: E04 запускается на Scale 3, пропускается на Scale 0-2

Шаг 3: Запустить E01 E02 E03 E06 на Scale 1
        → Все должны пройти ≥5%

Шаг 4: Запустить E05 на Scale 2
        → Должен пройти ≥5% при 10 набл./ячейку

Шаг 5: Запустить E04 на Scale 3
        → Должен пройти ≥5%

Шаг 6: Обновить progress.json
        → Отражает все актуальные результаты
```

---

## Сводная таблица изменений кода

| Файл | Что меняем | Строк |
|---|---|---|
| `Infrastructure/DatasetLoader.cs` | Scale 2 = 00–08, Scale 3 = 00–14, обновить TierNames | 10 |
| `Infrastructure/ExperimentRunner.cs` | E04 блокировка: порог 30K → 100K | 2 |
| `Infrastructure/ResultWriter.cs` или Program.cs | Обновление progress.json после каждого Run | 5 |

Итого: 3 файла, ~17 строк.

---

## Оценка времени

| Задача | Дней |
|---|---|
| 1–3: Код (DatasetLoader + ExperimentRunner + progress) | 1 |
| 4: Запуск Scale 1 (E01 E02 E03 E06) | 0.5 |
| 5: Запуск Scale 2 (E05) | 0.5 |
| 6: Запуск Scale 3 (E04) | 0.5 |
| **Итого** | **2.5 дня** |

---

⛔ STOP — жду одобрения.

*Experiments Scale 2 Plan v1.0 — не касается XIIGen*
