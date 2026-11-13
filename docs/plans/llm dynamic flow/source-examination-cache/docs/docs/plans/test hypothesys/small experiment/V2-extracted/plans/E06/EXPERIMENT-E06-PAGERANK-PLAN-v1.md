# E06 — PageRank ранжирование vs частота упоминания
## Отдельный план эксперимента · LlmFromScratch.sln

**ID:** E06-PAGERANK-RANKING  
**Подпроект-источник:** 38.CodeCompletion (Уровень 5 — PageRank граф зависимостей)  
**Использует готовые:** `07.GraphEngine.PageRankCalculator`, `07.GraphEngine.KnowledgeGraph`  
**Альтернативный подход учтён:** RESEARCH-03-T9 (PageRank = T9 с пониманием важности)

---

## Гипотеза

Частота упоминания метода в тексте (простой счётчик) ≠ его архитектурная важность. Метод `GetId()` может упоминаться 200 раз, а `ValidateAndAuthenticate()` — 5 раз. Но `ValidateAndAuthenticate` вызывается из 20 разных мест, из которых каждое само вызывается из многих мест — PageRank должен ранжировать его выше.

**Из RESEARCH-03-T9:** PageRank — это T9 с "социальной" оценкой важности. T9 предлагает то что часто нажимают. PageRank предлагает то что важно для системы в целом.

**Уникальный угол масштабирования:** PageRank нуждается в связном графе. При 3 файлах граф вызовов почти пустой, PageRank все методы оценит одинаково. При 50+ файлах появляются "хабы" — методы со многими входящими вызовами. Эксперимент масштабируется по **плотности графа**, а не просто по числу токенов.

---

## Что меняется

| Параметр | Baseline | Enhanced |
|---|---|---|
| Ранжирование | По частоте упоминания в тексте (count) | По PageRank в графе вызовов |
| Граф | Не строится | `KnowledgeGraph` с рёбрами типа "вызывает" |
| PageRank | — | `PageRankCalculator.Compute()` (уже реализован!) |
| Метрика | MRR (Mean Reciprocal Rank) | MRR |

MRR = среднее(1 / позиция_правильного_ответа):
```
Если правильный метод был предложен первым  → вклад = 1.0
Если вторым                                 → вклад = 0.5
Если третьим                                → вклад = 0.33
Если не попал в топ-10                      → вклад = 0

MRR = 1.0 → идеально, всегда правильный первым
MRR = 0.5 → в среднем правильный на второй позиции
```

---

## Уникальная ось масштабирования: плотность графа вызовов

```
Плотность графа = рёбра / (узлы × (узлы-1))

Tier 1 (3 файла, ~30 методов): граф разреженный, PageRank = случайно
Tier 2 (47 файлов, ~300 методов): граф начинает быть связным, PageRank начинает различать
Tier 3 (195 файлов, ~1500 методов): плотный граф, PageRank надёжен
```

**Метрика Gini PageRank = неравенство в распределении PageRank оценок:**
```
Gini = 0.0 → все методы одинаково важны (PageRank не помогает)
Gini = 1.0 → один метод бесконечно важнее других (PageRank очень полезен)

При маленьком графе: ожидаем Gini ~0.2–0.3
При большом графе:   ожидаем Gini ~0.5–0.7
```

---

## Лестница тиров

### Tier 1 — micro (< 5 секунд, < 50 МБ RAM)

**Датасет:** `01.Tokenizer` — 3 файла, ~30 методов  
**Ожидание:** PageRank не даёт преимущества (граф почти пустой)  
**Специфичный тест:** Gini PageRank — ожидаем ~0.15–0.25  
**Полезный результат даже при нулевом улучшении:** подтверждаем что механизм работает

---

### Tier 2 — small (< 30 секунд, < 120 МБ RAM)

**Датасет:** 6 малых подпроектов — ~47 файлов, ~300 методов  
**Ожидание:** PageRank начинает давать улучшение (+5–15% MRR)  
**Специфичный тест:** Gini ожидаем ~0.35–0.50  
**Ключевой вопрос:** какие методы получили высокий PageRank? Они действительно "важнее"?

---

### Tier 3 — medium (< 2 минуты, < 250 МБ RAM)

**Датасет:** все 24 подпроекта — ~195 файлов, ~1500 методов  
**Ожидание:** стабильное улучшение (+15–30% MRR)  
**Специфичный тест:** построить граф связности — есть ли явные хабы?  
**Визуализация:** топ-10 методов по PageRank vs топ-10 по frequency → насколько они совпадают?

---

### Tier 4 — external (< 10 минут)

**Датасет:** внешний C# проект  
**Ожидание:** PageRank переносится лучше frequency (архитектурные паттерны похожи между проектами)  
**Специфичный тест:** обучить PageRank на нашем проекте, применить к внешнему → насколько MRR хуже?

---

### Tier 5 — large connected graph (одноразово)

**Датасет:** несколько C# проектов вместе  
**Цель:** найти точку насыщения PageRank — при каком размере графа улучшение MRR перестаёт расти?  
**Гипотеза:** PageRank насыщается при ~500–1000 узлах (методах)

---

## Ресурсный бюджет

| Tier | Методов в графе | Время | RAM | Диск |
|---|---|---|---|---|
| 1 | ~30 | < 5 сек | < 50 МБ | < 3 КБ |
| 2 | ~300 | < 30 сек | < 120 МБ | < 8 КБ |
| 3 | ~1500 | < 2 мин | < 250 МБ | < 20 КБ |
| 4 | ~3000 | < 10 мин | < 500 МБ | < 50 КБ |
| 5 | ~10000 | часы | < 2 ГБ | < 200 КБ |

**PageRank сложность:** O(iterations × edges). 20 итераций × 1500 рёбер = 30 000 операций → миллисекунды.  
**Использует готовые:** `07.GraphEngine.PageRankCalculator.Compute()` — уже реализован!

---

## JSON результат (шаблон)

```json
{
  "meta": {
    "experiment_id": "E06-PAGERANK-RANKING-v1",
    "hypothesis": "PageRank on call graph ranks methods better than raw frequency for code completion",
    "run_timestamp_utc": null,
    "tier": 1
  },
  "dataset": {
    "tier": 1,
    "files_count": null,
    "methods_in_graph": null,
    "call_edges_in_graph": null,
    "source_project": "01.Tokenizer"
  },
  "baseline": {
    "name": "frequency-count-ranking",
    "mrr": null,
    "top1_accuracy": null,
    "top3_accuracy": null,
    "time_ms": null,
    "memory_peak_mb": null,
    "cpu_avg_pct": null,
    "disk_write_bytes": null
  },
  "enhanced": {
    "name": "pagerank-call-graph",
    "mrr": null,
    "top1_accuracy": null,
    "top3_accuracy": null,
    "pagerank_gini_coefficient": null,
    "pagerank_iterations": 20,
    "pagerank_computation_ms": null,
    "time_ms": null,
    "memory_peak_mb": null,
    "cpu_avg_pct": null,
    "disk_write_bytes": null
  },
  "delta": {
    "mrr_pct": null,
    "top1_accuracy_pct": null,
    "time_ms_pct": null,
    "memory_peak_mb_pct": null,
    "cpu_avg_pct_pct": null,
    "disk_write_bytes_pct": null
  },
  "scaling_signal": {
    "graph_density": null,
    "pagerank_gini": null,
    "note": "gini > 0.4 means PageRank has meaningful variance → improvement expected"
  },
  "verdict": {
    "passed": null,
    "next_action": null,
    "notes": ""
  }
}
```

---

## MD результат (шаблон)

```markdown
# E06 — PageRank vs частота · Tier [N]
Запущено: [дата] · [N] файлов · [N] методов в графе · [N] рёбер вызовов

## Результат: ✅ / ⚠️ / ❌

| Метрика | Baseline (frequency) | Enhanced (PageRank) | Δ | ≥5%? |
|---|---|---|---|---|
| MRR | — | — | — | — |
| Top-1 accuracy | —% | —% | — | — |
| Top-3 accuracy | —% | —% | — | — |
| Время | — мс | — мс | — | — |
| Память | — МБ | — МБ | — | — |

## Сигнал графа
Узлов: [N] · Рёбер: [N] · Плотность: [ratio]  
Gini PageRank: [value] → граф [связный / разреженный]  
PageRank [помогает / ещё слишком разреженный граф]

## Топ-3 метода по PageRank vs по frequency
PageRank: [m1, m2, m3] · Frequency: [m1, m2, m3]
Совпадение: [N из 3]

## Следующий шаг: [PROMOTE к большему графу / TUNE]
```

---

*E06 plan v1.0 — PageRank ranking experiment · не касается XIIGen*
