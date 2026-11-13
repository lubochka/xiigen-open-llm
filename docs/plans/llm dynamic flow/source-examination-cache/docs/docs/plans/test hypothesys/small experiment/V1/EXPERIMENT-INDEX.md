# EXPERIMENT-INDEX.md
## Шесть микро-экспериментов — индекс

| ID | Название | Метод | Ключевая метрика | Файл плана |
|---|---|---|---|---|
| E01 | Семантические семьи токенов vs плоский BPE | K-Means кластеризация токенов вместо инд... | accuracy | [E01-semantic-families.md](E01-semantic-families.md) |
| E02 | Энтропия Шеннона и сжатие токенного потока | Zstandard-сжатие vs plain uint16 shards... | accuracy | [E02-shannon-compression.md](E02-shannon-compression.md) |
| E03 | Локальное окно внимания vs полный контекст | CSR sparse attention (40 позиций) vs den... | accuracy | [E03-sparse-window.md](E03-sparse-window.md) |
| E04 | PMI инициализация весов vs случайная | PMI-SVD embedding init vs Xavier random... | accuracy | [E04-pmi-init.md](E04-pmi-init.md) |
| E05 | Иерархическое предсказание vs плоский softmax | Двухуровневый Марков: сначала кластер, п... | accuracy | [E05-hierarchical-prediction.md](E05-hierarchical-prediction.md) |
| E06 | AST узлы Roslyn vs плоский BPE текст | Токенизация через синтаксическое дерево ... | accuracy | [E06-ast-roslyn.md](E06-ast-roslyn.md) |

## Порядок запуска

Рекомендуемый порядок: E06 (AST, быстрый и показательный) → E05 (иерархия) → E01 (семьи) → E04 (PMI) → E03 (окно) → E02 (сжатие)

## Правило промоции

Scale N → Scale N+1 только если: accuracy ≥ +5% ИЛИ (любые 3 метрики ≥ +5% И accuracy ≥ 0%).

## Связанные документы

- `EXPERIMENT-HARNESS-v2-MARKOV-PROXY.md` — технические детали Марковского уровня
- `RESEARCH-04-UNIFIED-PREDICTION-FRAMEWORK-v1.md` — теоретическое обоснование
- `MICRO-EXPERIMENT-FRAMEWORK-v1.md` — общий фреймворк измерений
- `schema/experiment-result.schema.json` — JSON Schema для результатов