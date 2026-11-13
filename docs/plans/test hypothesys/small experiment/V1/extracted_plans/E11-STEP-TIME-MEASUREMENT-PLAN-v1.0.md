# E11 — Измерение времени шага обучения (BPE vs AST vs Structural)
## Experiment Plan v1.0 · LlmFromScratch.sln

**Вопрос:** на сколько шагов обучения и на сколько секунд реально
быстрее обучение с AST-представлением против BPE?

**Этот документ не касается XIIGen.**

---

## Что сейчас не измеряется

Все предыдущие эксперименты измерили:
- Точность предсказания на N-граммном уровне ✅
- Время подготовки данных (Roslyn, K-Means) ✅
- Количество токенов для того же кода ✅

Не измерено ни разу:
- Время одного шага обучения нейросети с разными представлениями ❌
- Количество шагов до сходимости ❌
- Реальное итоговое время обучения ❌

---

## Зонд-нейросеть

Для измерения используем минимальную нейросеть:

```
Архитектура:
  Embedding:    vocab_size × 64
  Layer 1:      Linear(64 → 64) + ReLU
  Layer 2:      Linear(64 → vocab_size)
  Loss:         CrossEntropyLoss на следующий токен
  Optimizer:    SGD, lr=0.01

Параметры:
  BPE вариант:  vocab=512 → 512×64 + 64×64 + 64×512 = 98K параметров
  AST вариант:  vocab=145 → 145×64 + 64×64 + 64×145 = 22K параметров
  Очень маленькая — чтобы обучение шло секунды, не часы
```

---

## Что измеряем точно

```csharp
// StepTimer.cs — замеряет время одного шага обучения
public sealed class StepTimerResult
{
    // Время одного прямого прохода (forward pass), мс
    public double ForwardPassMs { get; init; }

    // Время одного обратного прохода (backward pass / градиенты), мс
    public double BackwardPassMs { get; init; }

    // Время шага оптимизатора (обновление весов), мс
    public double OptimizerStepMs { get; init; }

    // Итого один шаг обучения, мс
    public double TotalStepMs { get; init; }

    // Количество токенов в батче
    public int BatchSizeTokens { get; init; }

    // Длина последовательности (context window)
    public int SequenceLength { get; init; }
}
```

**Три варианта на одном батче:**

| Вариант | Vocab | Токенов на 1 KB кода | Seq length |
|---|---|---|---|
| BPE | 512 | ~218 | длинная |
| AST node types | 145 | ~34 | короткая (6.4× меньше) |
| AST + structural vector | 145 + доп. признаки | ~34 | короткая |

Важно: **один и тот же код** в разных представлениях.
Тогда разница во времени отражает только длину последовательности.

---

## Теоретическое ожидание

Attention стоит `O(n²)` где n — длина последовательности.
AST даёт в 6.4× меньше токенов:
```
BPE время attention:   n² = 218² = 47 524 операций
AST время attention:   n² = 34²  =  1 156 операций
Теоретическое ускорение: 47524 / 1156 = 41×
```

Реальное ускорение будет меньше из-за:
- Overhead памяти (allocation, cache)
- Матричное умножение параллелизуется
- Маленькая модель → overhead доминирует над вычислением

**Ожидаем:** 5-15× ускорение на шаг (не 41× теоретических).

---

## Сколько шагов до сходимости

Ключевой вопрос: если каждый шаг быстрее, нужно ли больше шагов?

Измеряем: loss на валидационном наборе каждые 10 шагов.
Записываем `steps_to_convergence` — когда loss перестаёт падать.

```
Критерий сходимости: loss не улучшается более чем на 0.01 за 50 шагов
```

**Возможные результаты:**

1. AST быстрее на шаг И нужно меньше шагов → однозначная победа
2. AST быстрее на шаг, нужно столько же шагов → ускорение пропорционально
3. AST быстрее на шаг, нужно больше шагов → зависит от соотношения

---

## Scaling signal

```
time_efficiency_ratio = (steps_bpe × time_per_step_bpe) /
                        (steps_ast × time_per_step_ast)

Ratio > 1.0 → AST быстрее в итоге
Ratio = 1.0 → одинаково
Ratio < 1.0 → AST медленнее (неожиданно — нужно разбираться)
```

---

## Полная таблица результатов (что заполнится после запуска)

| Метрика | BPE | AST | Structural |
|---|---|---|---|
| Токенов / KB кода | 218 | 34 | 34 |
| Время forward pass, мс | ? | ? | ? |
| Время backward pass, мс | ? | ? | ? |
| Время 1 шага итого, мс | ? | ? | ? |
| Шагов до сходимости | ? | ? | ? |
| Итоговое время, сек | ? | ? | ? |
| Финальный loss | ? | ? | ? |
| Ускорение × | 1.0× | ? | ? |

---

## Реализация

```csharp
// E11StepTimer.cs — запускает 100 шагов обучения зонд-нейросети
// для каждого из трёх представлений, замеряет точное время.

public sealed class E11StepTimer
{
    // Количество шагов для измерения
    // 100 достаточно для стабильного среднего (первые 10 — прогрев)
    private const int WarmupSteps = 10;
    private const int MeasureSteps = 100;

    public OperationResult<ExperimentV2Result> Run(
        string csharpSource,
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        // Вариант 1: BPE токены
        var bpeResult = MeasureTraining(TokenizeBpe(csharpSource), "BPE");

        // Вариант 2: AST токены
        var astResult = MeasureTraining(TokenizeAst(csharpSource), "AST");

        // Вариант 3: AST + structural vector (E09 стиль)
        var structResult = MeasureTraining(TokenizeStructural(files), "Structural");

        return BuildResult(bpeResult, astResult, structResult, tier);
    }

    private TrainingMetrics MeasureTraining(
        IReadOnlyList<int> tokens,
        string name)
    {
        // Создать зонд-нейросеть под vocab этих токенов
        // Запустить WarmupSteps (без замера)
        // Запустить MeasureSteps с замером каждого
        // Вернуть среднее время шага + steps_to_convergence
    }
}
```

---

## Порядок выполнения

```
1. Реализовать ProbeNeuralNetwork.cs (2 слоя, 64 dim, forward + backward + step)
2. Написать E11StepTimer.cs
3. runner.Run("E11", scale: 1)  ← Scale 1 достаточен, нужен стабильный baseline
4. Заполнить таблицу выше реальными числами
5. Вычислить time_efficiency_ratio
6. Если ratio > 2.0 → подтвердить в документации что AST ускоряет обучение
   и использовать это как аргумент для перехода к полной нейросети
```

**Этот эксперимент критичен для бизнес-кейса:**
он переводит теоретическое "N-грамм точнее" в практическое
"обучение займёт X часов вместо Y часов".

---

*E11 plan v1.0 — step time measurement experiment · не касается XIIGen*
