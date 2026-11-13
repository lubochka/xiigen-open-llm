# Experiments — План конкретных действий v1.0
## Что именно делать с каждым экспериментом

**Проект:** llm-mvp-core — подпроект 34.Experiments  
**Этот документ не касается XIIGen.**

---

## Корневая проблема: токенизатор неправильный для трёх экспериментов

`ExperimentRunner` использует `CharLevelTokenize()` — каждый символ → ASCII код 0–127.  
Это даёт `vocabSize = 128` и `seenTokens ≈ 76` на любом файле C#.

Для **E03** (триграмма) и **E06** (AST) это нормально — им нужны последовательности, не семантика слов.  
Для **E01** (K-Means кластеры), **E04** (PMI) и **E05** (иерархия) — это неправильно.  
Их теория работает на словах/подсловах, не на символах.

**Правило:** если эксперимент тестирует идею «сгруппировать похожие токены» —  
токены должны быть словами или BPE подсловами, а не буквами.

---

## Проблема 2: баг в E05 — guard никогда не срабатывает

```
Код: if (vocabSize < 100)  // vocabSize = max_token + 1 = 128 всегда для ASCII
128 < 100 = FALSE → guard не срабатывает → эксперимент запускается на 76 символах
```

Нужно проверять `seenTokens.Count < 100`, а не `vocabSize < 100`.

---

## Семь конкретных задач

---

### Задача 1 — ExperimentRunner: переключатель токенизатора

**Файл:** `Infrastructure/ExperimentRunner.cs`

**Что добавить:** вместо одного `CharLevelTokenize()` — два метода и выбор по эксперименту:

```csharp
// Char-level: для E03 (n-граммы), E06 (AST сравнение) — символы несут достаточно структуры
// BPE: для E01 (K-Means), E04 (PMI), E05 (иерархия) — нужны слова/подслова

OperationResult<ExperimentResult> runResult = experimentId switch
{
    "E01" => _dispatcher.RunE01(BpeTokenize(dataset.CombinedText, vocabSize: 500)),
    "E02" => _dispatcher.RunE02(CharLevelTokenize(dataset.CombinedText)),
    "E03" => _dispatcher.RunE03(CharLevelTokenize(dataset.CombinedText)),
    "E04" => _dispatcher.RunE04(BpeTokenize(dataset.CombinedText, vocabSize: 500)),
    "E05" => _dispatcher.RunE05(BpeTokenize(dataset.CombinedText, vocabSize: 500)),
    "E06" => _dispatcher.RunE06(CharLevelTokenize(dataset.CombinedText), dataset.CombinedText),
    _     => OperationResult<ExperimentResult>.Fail($"Unknown: {experimentId}"),
};
```

**BpeTokenize — новый приватный метод:**

```csharp
// Токенизировать текст через BPE из подпроекта 01 (уже есть в Solution).
// vocabSize — размер словаря: 500 для Scale 0-1, 1000 для Scale 2+.
// Результат: подслова типа "pub", "lic", "bool", "Validate" → целые числа.
private static IReadOnlyList<int> BpeTokenize(string text, int vocabSize)
{
    var bpe = new Tokenizer.BpeTokenizer();
    bpe.Train(text, Math.Max(256, vocabSize));
    return bpe.Encode(text);
}
```

**Ожидаемый эффект:**
- E01: vocabSize ~300–500 BPE подслов, seenTokens ~300–400 → K-Means на словах
- E04: матрица PMI 300×300 вместо 128×128, заполненная гораздо лучше
- E05: реальная иерархия «слово → кластер слов → предсказание следующего»

**Gate B:** `BpeTokenize(csharpCode, 500)` возвращает токены с `Max() > 255` (есть мерджи).

---

### Задача 2 — E05: исправить guard

**Файл:** `Experiments/E05HierarchicalPrediction.cs`

**Было:**
```csharp
int vocabSize = train.Max() + 1;
if (vocabSize < 100)  // НЕПРАВИЛЬНО: vocabSize = 128 всегда для ASCII
```

**Нужно:**
```csharp
int vocabSize = train.Max() + 1;
var seenTokens = Enumerable.Range(0, vocabSize).Where(t => tokenCounts[t] > 0).ToList();
if (seenTokens.Count < 50)  // ПРАВИЛЬНО: сколько токенов реально встречается
    return OperationResult<ExperimentResult>.Fail(
        $"E05 нужно ≥50 уникальных токенов, нашли {seenTokens.Count}. " +
        "С BPE (Задача 1) этот guard не будет срабатывать.");
```

После Задачи 1 этот guard станет страховкой — с BPE будет 300+ уникальных токенов.

---

### Задача 3 — E04: убрать старый файл результата

**Что сделать:**  
Удалить `RESULT-E04-scale0.json` и `RESULT-E04-scale0.md` из папки результатов.  
E04 не запускался с новым кодом — файл от старого прогона с отметкой `14:08`.  
Когда E04 правильно пропустится (Задача 1 добавит проверку токенов), RunScaleLadder  
запишет новый файл с пометкой `SKIPPED` или `FAIL("нужен Scale 2")`.

**Дополнительно:** добавить в `ResultWriter.WriteSkipped()` метод для явной записи пропусков:

```json
{
  "experiment_id": "E04",
  "scale": 0,
  "status": "SKIPPED",
  "reason": "E04 требует Scale 2+ (~30K токенов). Текущий датасет: 1 433 токена.",
  "next_action": "RUN_AT_SCALE_2"
}
```

---

### Задача 4 — E01, E05: пересчитать ожидаемые результаты после BPE

После Задачи 1 E01 и E05 получат правильные входные данные. Новые ожидания:

**E01 при Scale 0 с BPE (vocabSize ~350):**
- K-Means K=50 на 350 уникальных BPE подсловах → осмысленные кластеры
- `adaptiveK = min(50, 350/5) = 50` — теперь K не урезается
- Ожидаемый результат: accuracy +10–20% (кластеры объединят варианты одного слова)
- Если не прошёл: Scale 1 (9 100 токенов) — больше данных для K-Means

**E05 при Scale 0 с BPE (vocabSize ~350):**
- seenTokens ~300 → guard `< 50` не сработает
- K=50 кластеров на 300 токенах → 6 токенов на кластер в среднем
- Ожидаемый результат: accuracy -5 до +5% (иерархия ещё мала, нужен Scale 1)
- Если не прошёл: Scale 1 — при 9 100 токенах иерархия становится значимой

---

### Задача 5 — Определить что значит «Scale 1» для каждого эксперимента

**Конкретные инструкции для Claude Code:**

```
Scale 1 = DatasetLoader.Load(scale: 1, projectRoot)
Читает: папку 01.Tokenizer целиком (все .cs файлы)
Файлов: ~9 штук
Символов: ~6 267
Токенов BPE (vocabSize=500): ~1 500–2 000
Токенов ASCII: ~6 267

Для каждого эксперимента на Scale 1:
  E01: BPE vocabSize=500, K=50 — должен пройти если теория верна
  E02: CharLevel, K=50 — больше данных для кластеров
  E03: CharLevel — больший корпус, триграмма надёжнее
  E04: НЕ ЗАПУСКАТЬ — нужен Scale 2 (~30K токенов из 5 подпроектов)
  E05: BPE vocabSize=500 — должен показать осмысленную иерархию
  E06: CharLevel + Roslyn — больше AST узлов, надёжнее статистика
```

**Команда запуска (добавить в README):**
```csharp
var runner = new ExperimentRunner(
    projectRoot: @"путь\к\llm_mvp_core",
    outputDir:   @"experiments\results");

// Запустить конкретный эксперимент на Scale 1:
runner.Run("E01", scale: 1);

// Или запустить все на Scale 1:
foreach (var id in new[] { "E01", "E02", "E03", "E05", "E06" })
    runner.Run(id, scale: 1);

// Или автоматическая лестница от Scale 0 до первого провала:
runner.RunScaleLadder("E01");
```

---

### Задача 6 — E04: конкретный план Scale 2

E04 (PMI предсказатель) проверяет теорию: PMI лучше count-based Маркова.  
Это правдоподобная идея — нужно просто достаточно данных.

**Scale 2 для E04:**
```
DatasetLoader.Load(scale: 2, projectRoot)
Читает: 01.Tokenizer + 02.DatasetPipeline + 05.Embeddings + 07.GraphEngine + 09.Inference
Файлов: ~47
Токенов BPE (vocabSize=1000): ~7 000–10 000
PMI матрица 1000×1000 — заполненная (не нулевая)
```

**Запуск:**
```csharp
runner.Run("E04", scale: 2);
// ExperimentRunner убедится что ApproxTokens >= 30_000 перед запуском
```

---

### Задача 7 — Обновить критерий pass/fail в notes

Сейчас в notes пишется только технические детали (K=50, vocabSize=128).  
Добавить в notes **интерпретацию** — что результат означает для теории:

```json
"notes": "K=50 кластеров на 350 BPE токенах. Кластеры объединили варианты одного слова. accuracy_delta=+12.3% подтверждает гипотезу: семантические семьи помогают предсказанию."
```

или

```json
"notes": "PMI матрица разрежена: только 34% строк имеют ненулевые значения. Для надёжного PMI нужен более крупный корпус. Запустить Scale 2."
```

---

## Сводная таблица: файл → задача → строк кода

| Файл | Задача | Строк |
|---|---|---|
| `Infrastructure/ExperimentRunner.cs` | Задача 1: BpeTokenize + переключатель | 20 |
| `Experiments/E05HierarchicalPrediction.cs` | Задача 2: исправить guard | 3 |
| `Infrastructure/ResultWriter.cs` | Задача 3: метод WriteSkipped | 15 |
| Результаты: удалить `RESULT-E04-scale0.*` | Задача 3: удалить старые файлы | — |
| `Infrastructure/ExperimentRunner.cs` | Задача 7: расширить notes | 10 |

Итого: ~4 файла, ~50 строк.

---

## Ожидаемые результаты после всех исправлений

| Эксп. | Scale 0 (сейчас) | Scale 0 после Задачи 1 | Scale 1 (если Scale 0 не прошёл) |
|---|---|---|---|
| E01 | ❌ +1.5% (ASCII) | ✅ ожидаем +10–20% (BPE) | Если не прошёл: запустить |
| E02 | ✅ +11% | Без изменений | Запустить для подтверждения |
| E03 | ✅ +14% | Без изменений | Запустить для подтверждения |
| E04 | ⛔ Пропустить | Пропустить | ⛔ Пропустить → Scale 2 |
| E05 | ❌ -14% (ASCII + баг guard) | Нейтрально ожидаем | Если не прошёл: запустить |
| E06 | ✅ +30% | Без изменений | Запустить для подтверждения |

---

## Оценка времени

| Задача | Дней |
|---|---|
| 1 — BpeTokenize в ExperimentRunner | 0.5 |
| 2 — E05 guard | 0.5 |
| 3 — ResultWriter.WriteSkipped + удаление файла | 0.5 |
| 7 — Расширенные notes | 0.5 |
| **Итого** | **2 дня** |

---

⛔ STOP — жду одобрения.

*Experiments Fix Plan v1.0 — не касается XIIGen*
