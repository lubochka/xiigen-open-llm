# E04 — PMI инициализация весов vs случайная
## PMI-SVD embedding init vs Xavier random
**Проект:** llm-mvp-core · **Этот документ не касается XIIGen.**

---

## Гипотеза
PMI (Pointwise Mutual Information — взаимная точечная информация):
PMI[a][b] = log(P(a,b) / P(a)×P(b))

PMI > 0: b чаще следует за a чем можно ожидать случайно.
PMI < 0: редкое сочетание.

Инициализировать embedding матрицу из PMI (через SVD) вместо случайной Xavier даёт
модели «предзнание» о совместных вхождениях. Первые 10 000 шагов обучения
уже «пропущены» — модель не тратит их на открытие очевидного.

Марковский уровень: PMI как прямой предсказатель (argmax PMI[current_token])
должен быть точнее count-based Маркова.

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

**Baseline (оригинал):** Count-based Марков: P(next|prev) = count(prev,next)/count(prev). Нейросеть: Xavier random init.

**Enhanced (улучшение):** PMI-weighted: argmax(PMI[prev, :]). Нейросеть: init из top-64 сингулярных векторов PMI матрицы.

**Использует подпроекты:** 01.Tokenizer, 33.MatrixAlgorithms (SVD из подпроекта 28М)

---

## Марковская реализация (Scale 0–2)

```csharp
// E04: PMI как прямой предсказатель вместо count-based
static double[,] ComputePmi(long[,] cooc, long[] marginals, int V)
{
    long total = marginals.Sum();
    var pmi = new double[V, V];
    for (int a = 0; a < V; a++)
    for (int b = 0; b < V; b++)
    {
        if (cooc[a,b] == 0) continue;
        double pAB = cooc[a,b] / (double)total;
        double pA  = marginals[a] / (double)total;
        double pB  = marginals[b] / (double)total;
        pmi[a,b] = Math.Log2(pAB / (pA * pB));
    }
    return pmi;
}

// baseline:  предсказываем argmax(counts[prev, :])
// enhanced:  предсказываем argmax(pmi[prev, :])
// (PMI отфильтровывает частые но незначимые переходы типа "the → the")
```

---

## Примечания по каждому уровню

**Scale 0:** BpeTokenizer.cs: посчитать PMI матрицу на 1000 токенах. Сравнить точность предсказания.

**Scale 1:** 01.Tokenizer: PMI на 9100 токенах. Матрица плотнее → PMI точнее.

**Scale 2:** 5 подпроектов: PMI матрица большего словаря. Разреженность растёт → нужно PPMI (positive PMI only).

**Scale 3:** Зонд: инициализировать embedding из SVD(PMI), обучить 100 шагов. Сравнить с Xavier init.

**Scale 4:** Зонд на 5 подпроектах: проверяем что PMI init сходится быстрее.

**Scale 5:** Полный трансформер: PMI init должен сократить шаги до цели на 20–40%.

---

## Ожидаемые результаты на Scale 0

| Метрика | Ожидаемое улучшение |
|---|---|
| accuracy | +5 до +15% |
| time | +3% (вычисление PMI) |
| memory | ±0% |
| cpu | +5% |
| disk | +10% (сохранить PMI матрицу) |

---

## Альтернативный подход

THREE TRACKS (X02): вместо PMI на всём словаре — отдельные PMI матрицы для каждого трека (код / документация / тех.язык). Меньше шума от смешения типов.

---

## Условие остановки (ABANDON)

Если PMI предсказатель хуже или равен count — PMI слишком разрежен на маленьком датасете. Попробовать PPMI (обнулить отрицательные) или сначала кластеризовать.

---

## Формат вывода: JSON + MD

Каждый прогон пишет два файла:

```
experiments/E04/
  RESULT-E04-scale{N}.json    ← машиночитаемый
  RESULT-E04-scale{N}.md      ← человекочитаемый
```

### JSON шаблон (заполняется автоматически)

```json
{
  "experiment_id": "E04",
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
# E04 scale {N} — [название]
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
- `E04-pmi-init/RESULT-E04-scale*.json` — результаты прогонов

*E04 v1.0 — микро-эксперименты · LlmFromScratch.sln*