# E03 — Локальное окно внимания vs полный контекст
## CSR sparse attention (40 позиций) vs dense (полная длина)
**Проект:** llm-mvp-core · **Этот документ не касается XIIGen.**

---

## Гипотеза
Большинство важной информации для предсказания следующего токена
находится в последних 32 токенах + случайных 8 далёких.
Посмотреть на все 2048 предыдущих токенов — дорого и большей частью не нужно.

Марковский аналог: триграмма (контекст 2) лучше биграммы (контекст 1)
на 5–15%, но при этом матрица маленькая если словарь кластеров 50.
Ключевой вопрос: насколько сильно улучшение при увеличении контекста?

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

**Baseline (оригинал):** Биграмма: P(next | prev1). Полная матрица переходов V×V. При нейросети: полный dense attention.

**Enhanced (улучшение):** Триграмма: P(next | prev2, prev1). При нейросети: локальное окно 32 + 8 случайных (CSR маска из подпроекта 29М).

**Использует подпроекты:** 01.Tokenizer, 33.MatrixAlgorithms (CSR формат)

---

## Марковская реализация (Scale 0–2)

```csharp
// E03: Markov-level: bigram vs trigram
// baseline: bigram — P(next | prev)
// enhanced: trigram — P(next | prev2, prev1)

// Построить триграммную матрицу переходов
// Ключ = (prev2, prev1), значение = счётчики для каждого next
var trigramCounts = new Dictionary<(int,int), int[]>();
for (int i = 2; i < trainTokens.Count; i++)
{
    var key = (trainTokens[i-2], trainTokens[i-1]);
    if (!trigramCounts.ContainsKey(key))
        trigramCounts[key] = new int[vocabSize];
    trigramCounts[key][trainTokens[i]]++;
}

// Предсказание: найти argmax по строке (prev2, prev1)
int prev2 = valTokens[i-2], prev1 = valTokens[i-1];
var key = (prev2, prev1);
int predicted = trigramCounts.ContainsKey(key)
    ? Array.IndexOf(trigramCounts[key], trigramCounts[key].Max())
    : bigramFallback(prev1, vocabSize);  // fallback на биграмму если тригр. не найден
```

---

## Примечания по каждому уровню

**Scale 0:** BpeTokenizer.cs: bigram vs trigram. Матрица trigram на кластерах 50: 50²=2500 vs 50³=125000 — всё ещё мало.

**Scale 1:** 01.Tokenizer: тот же тест. Проверяем не исчезает ли выигрыш на большем датасете.

**Scale 2:** 5 подпроектов: тест стабильности. Trigram должен оставаться лучше bigram.

**Scale 3:** Зонд-сеть: window=32+8random vs full attention. Измеряем accuracy + память.

**Scale 4:** Зонд на 5 подпроектах: подтверждаем что sparse attention не теряет точность.

**Scale 5:** Полный трансформер со sparse маской: финальный тест E03.

---

## Ожидаемые результаты на Scale 0

| Метрика | Ожидаемое улучшение |
|---|---|
| accuracy | +5 до +15% |
| time | -30 до -50% |
| memory | -60 до -80% |
| cpu | -40 до -60% |
| disk | 0% |

---

## Альтернативный подход

PROGRESSIVE CURRICULUM (X04): вместо фиксированного окна — адаптивное. Начинаем с window=8, постепенно расширяем до 256 по мере обучения.

---

## Условие остановки (ABANDON)

Если trigram дал < 5% улучшения по accuracy — увеличение контекста не помогает. Попробовать Кнезер-Ней сглаживание (модификация счётчиков).

---

## Формат вывода: JSON + MD

Каждый прогон пишет два файла:

```
experiments/E03/
  RESULT-E03-scale{N}.json    ← машиночитаемый
  RESULT-E03-scale{N}.md      ← человекочитаемый
```

### JSON шаблон (заполняется автоматически)

```json
{
  "experiment_id": "E03",
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
# E03 scale {N} — [название]
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
- `E03-sparse-window/RESULT-E03-scale*.json` — результаты прогонов

*E03 v1.0 — микро-эксперименты · LlmFromScratch.sln*