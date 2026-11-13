# Эксперименты — Мастер README
## 6 независимых планов · LlmFromScratch.sln

Каждый эксперимент — отдельный файл, отдельная ось масштабирования.  
Источник: подпроекты 33–38 (CODE-COMPLETION-ALGORITHMS) +  
альтернативные подходы из `MICRO-EXPERIMENT-FRAMEWORK-v1` +  
рамка масштабирования из `RESEARCH-04-UNIFIED-PREDICTION-FRAMEWORK-v1`.

---

## Карта экспериментов

| ID | Файл плана | Подпроект | Что тестируем | Первичная метрика | Уникальная ось масштабирования |
|---|---|---|---|---|---|
| E01 | E01/EXPERIMENT-E01-AST-TOKENIZER-PLAN-v1.md | 33 | AST узлы vs BPE токены | prediction_error | Насыщение словаря: AST стабилизируется раньше BPE |
| E02 | E02/EXPERIMENT-E02-FREQUENCY-RANKING-PLAN-v1.md | 34 | Частота вызовов vs алфавит | top1_accuracy | Статистическая уверенность: улучшение растёт с объёмом |
| E03 | E03/EXPERIMENT-E03-NGRAM-MODEL-PLAN-v1.md | 35 | Оптимальный N для N-грамм | perplexity | Контекстное окно: оптимальный N растёт с данными |
| E04 | E04/EXPERIMENT-E04-BM25-CONTEXT-PLAN-v1.md | 36 | BM25 поиск по проекту | recall_at_3 | Порог корпуса: BM25 начинает работать при N≥20 файлах |
| E05 | E05/EXPERIMENT-E05-SYNTAX-FILTER-PLAN-v1.md | 37 | Roslyn маска допустимых | syntactically_correct_pct | Сложность кода: чем сложнее, тем больше выигрыш |
| E06 | E06/EXPERIMENT-E06-PAGERANK-PLAN-v1.md | 38 | PageRank vs частота | mrr | Плотность графа: PageRank работает при связном графе |

---

## Общие тиры датасета

Все эксперименты используют одни и те же тиры, но каждый имеет свой критерий перехода:

| Тир | Датасет | Токенов | Файлов | Лимит времени | Лимит RAM |
|---|---|---|---|---|---|
| 1 | `01.Tokenizer` | ~2 000 | 3 | 30 сек | 80 МБ |
| 2 | 6 малых подпроектов (01,02,05,07,09,10) | ~9 100 | 47 | 5 мин | 200 МБ |
| 3 | Все 24 подпроекта | ~141 000 | 195 | 10 мин | 500 МБ |
| 4 | Внешний C# репозиторий | ~200 000+ | 500+ | 20 мин | 1 ГБ |
| 5 | CodeSearchNet C# или WikiText-103 CS | 500 000+ | — | часы | 4 ГБ |

---

## Правила эксперимента (общие для всех)

```
НЕЛЬЗЯ: обучать нейросеть (даже 1 шаг градиентного спуска)
НЕЛЬЗЯ: скачивать внешние данные во время прогона
НЕЛЬЗЯ: держать файлы открытыми дольше 1 секунды
НЕЛЬЗЯ: смешивать несколько экспериментов в одном прогоне

НУЖНО: каждый прогон → один .json + один .md файл
НУЖНО: имя файла = run-YYYY-MM-DD-NNN.json (NNN = порядковый номер прогона)
НУЖНО: обновить progress.json после каждого прогона

МОЖНО: кешировать Roslyn AST между прогонами (разобрать файл один раз)
МОЖНО: использовать любой уже реализованный подпроект (01–38)
```

---

## Критерии перехода на следующий тир (уникальны для каждого эксперимента)

| Эксперимент | Критерий перехода | Если не прошёл |
|---|---|---|
| E01 | prediction_error улучшение ≥5% AND ast_saturation_ratio < 0.15 | Оптимизировать кеш Roslyn |
| E02 | top1_accuracy улучшение ≥5% AND confidence_ratio > 0.5 | Собрать больше call sites из файлов |
| E03 | bigram perplexity < 0.6 × unigram perplexity | Проверить размер словаря (слишком большой?) |
| E04 | recall_at_3 улучшение ≥5% AND files_count ≥ 20 | E04 пропускает Tier 1, начинает с Tier 2 |
| E05 | syntactically_correct_pct ≥ 95% AND top1_accuracy улучшение ≥5% | Проверить кеш SemanticModel |
| E06 | mrr улучшение ≥5% AND gini > 0.35 | E06 может не работать на Tier 1 — это нормально |

---

## Структура файлов результатов

```
experiments/
├── README.md                              ← этот файл
├── schema/
│   └── experiment-result-v1.json         ← JSON Schema (уже создана)
├── E01-AST-TOKENIZER/
│   ├── EXPERIMENT-E01-AST-TOKENIZER-PLAN-v1.md    ← план
│   └── results/
│       ├── run-YYYY-MM-DD-001.json        ← результаты прогона
│       └── run-YYYY-MM-DD-001.md
├── E02-FREQUENCY-RANKING/
│   ├── EXPERIMENT-E02-FREQUENCY-RANKING-PLAN-v1.md
│   └── results/
├── E03-NGRAM-MODEL/
│   ├── EXPERIMENT-E03-NGRAM-MODEL-PLAN-v1.md
│   └── results/
├── E04-BM25-CONTEXT/
│   ├── EXPERIMENT-E04-BM25-CONTEXT-PLAN-v1.md
│   └── results/
├── E05-SYNTAX-FILTER/
│   ├── EXPERIMENT-E05-SYNTAX-FILTER-PLAN-v1.md
│   └── results/
├── E06-PAGERANK-RANKING/
│   ├── EXPERIMENT-E06-PAGERANK-PLAN-v1.md
│   └── results/
└── progress/
    ├── progress.json                      ← сводка по всем экспериментам
    └── progress.md                        ← для человека
```

---

## Связь с теорией (RESEARCH-04)

Все 6 экспериментов проверяют одну и ту же идею:

> N-граммная/статистическая модель и обученная матрица весов нейросети  
> отвечают на один вопрос: P(следующий токен | контекст)  
> Если подход X улучшает N-граммную точность → он улучшит и нейросетевую.

Поэтому эти эксперименты — не просто полезные сами по себе. Они **валидируют гипотезы до запуска дорогого обучения** (EXPERIMENT-HARNESS-v2-MARKOV-PROXY).

Порядок работы:
```
1. Запустить E01–E06 на Tier 1 (минуты, без GPU)
2. Те эксперименты что прошли → продвигать на Tier 2 (часы, без GPU)
3. Tier 3–5 → с этими экспериментами уже можно идти в нейросеть (EXPERIMENT-HARNESS-v1)
```

---

## Быстрый старт — с чего начать

**Рекомендуемый порядок первого запуска:**

1. **E01** (AST tokenizer) — самый быстрый, сразу виден результат (< 5 сек)
2. **E03** (N-граммы) — строит базовую модель, нужна для остальных как baseline
3. **E02** (Frequency) — использует те же данные что E03
4. **E05** (Syntax filter) — тоже использует Roslyn, кешировать с E01
5. **E06** (PageRank) — нужен граф из `07.GraphEngine`, строится один раз
6. **E04** (BM25) — начинать с Tier 2, не Tier 1 (нужен corpus ≥20 файлов)

---

## Прогресс (обновляется вручную)

```json
{
  "last_updated": null,
  "experiments": {
    "E01": { "status": "NOT_STARTED", "best_tier_passed": null },
    "E02": { "status": "NOT_STARTED", "best_tier_passed": null },
    "E03": { "status": "NOT_STARTED", "best_tier_passed": null },
    "E04": { "status": "NOT_STARTED", "best_tier_passed": null },
    "E05": { "status": "NOT_STARTED", "best_tier_passed": null },
    "E06": { "status": "NOT_STARTED", "best_tier_passed": null }
  }
}
```

---

*README v1.0 — master experiment index · не касается XIIGen*
