# Рекон: 06.VectorSearch — векторный поиск (IQueryable результатов, many→many)

corpus: core · slug: core-06-vsearch · relevance: low

## Что это и где (точные ключевые пути)

Маленький самодостаточный проект-библиотека `VectorSearch` (.NET 9, чистый C#, без ML-фреймворков). Весь исходный код — 6 файлов (остальное в `obj/`, `bin/` — артефакты сборки, пропущены):

- `C:/Projects/llm_mvp_core/06.VectorSearch/IVectorIndex.cs` — контракт индекса (DNA rule 4).
- `C:/Projects/llm_mvp_core/06.VectorSearch/IndexEntry.cs` — `record IndexEntry(string Id, float[] Vector, Dictionary<string,string> Metadata)`.
- `C:/Projects/llm_mvp_core/06.VectorSearch/Distances.cs` — статические метрики: `CosineSimilarity`, `EuclideanDistance`, `InnerProduct`.
- `C:/Projects/llm_mvp_core/06.VectorSearch/BruteForceIndex.cs` — точный O(n) перебор; Save/Load бросают `NotImplementedException` (in-memory only).
- `C:/Projects/llm_mvp_core/06.VectorSearch/HnswIndex.cs` — приближённый ANN (HNSW); есть атомарный Save/Load в JSON (DNA rule 7).
- `C:/Projects/llm_mvp_core/06.VectorSearch/VectorSearch.csproj` — голый SDK-проект, нет ссылок на другие сборки (в т.ч. нет `00.Core`, нет `OperationResult<T>`).

## Как устроено / работает (кратко)

Единый контракт `IVectorIndex` (`IVectorIndex.cs:4-9`):
```
int Count { get; }
void Add(float[] vector, string id, Dictionary<string,string>? metadata = null);
List<(string id, float score)> Search(float[] query, int topK);
```
Две реализации:
- `BruteForceIndex` — хранит `List<IndexEntry>`, на запрос делает `Select → CosineSimilarity → OrderByDescending → Take(topK) → ToList()` (`BruteForceIndex.cs:18-25`). Точный, медленный; для коллекций <10k и для проверки корректности HNSW.
- `HnswIndex` — иерархический граф «small world»: параллельные списки `_vectors/_ids/_meta` + `_graph` (`[node][level][neighbours]`). Insert: случайный уровень → жадный спуск от верха → двунаправленное соединение с ef-construction соседями. Search: жадный спуск до уровня 1, затем beam-search (`SearchLayer`, `SortedSet` по убыванию score) на уровне 0, `Take(topK)` (`HnswIndex.cs:94-106`). Метрика везде жёстко зашита — косинус (`GreedySearch`, `SearchLayer`, `SelectNeighbours` зовут только `Distances.CosineSimilarity`, хотя `Euclidean/InnerProduct` объявлены и не используются).

### Прямой ответ на вопрос: IQueryable результатов, many→many

- **IQueryable: НЕТ.** Контракт возвращает материализованный `List<(string id, float score)>` с немедленным `.ToList()`. Нет `IQueryable<T>`, нет `IAsyncEnumerable<T>`, нет отложенного/композируемого выражения, нет провайдера запросов, нет проекции в EF/LINQ-провайдер. Внутри LINQ применяется только к in-memory коллекциям (`IEnumerable`, не `IQueryable`). Результат — это плоский топ-K (id, score), без фильтров по метаданным, без пагинации, без серверной фильтрации.
- **many→many: НЕТ.** Поиск строго один→много: одна query-вектор-точка → один ранжированный список кандидатов. Нет batch-перегрузки (нет `Search(IEnumerable<float[]> queries, ...)`), нет фан-аута по нескольким запросам, нет матрицы запрос×кандидат, нет агрегации/слияния результатов нескольких запросов. `Add` тоже одиночный (по одному вектору), без bulk-вставки. `Dictionary<string,string> Metadata` хранится, но **нигде не используется** для группировки/связей many-to-many между сущностями.

Вывод по вопросу: ни «IQueryable результатов», ни «many→many» в `06.VectorSearch` сейчас не существует. Это узкий примитив top-K kNN-поиска по одному запросу с eager-материализацией.

## Релевантность к ILearningDynamicGraph (DPO / обучение / граф / оркестрация / измерение / teacher-provenance)

Низкая (relevance: low), но есть две точки соприкосновения как у **компонента-памяти/ретривера**, а не как у обучаемого узла:

- **KNN/retrieval как компонент модели, не как архитектура.** По `trainable-architecture-decision-SKILL.md` (раздел «KNN/retrieval note», строки ~112-117 и verdict `OK_MODEL_COMPONENT`) векторный kNN допустим как memory layer / nearest-neighbour retriever **внутри** разрешённого уровня. Для ILearningDynamicGraph это естественный кандидат на роль «KNN/nearest memory» в обучаемых юнитах (например, поддержка решения ближайшими примерами, поиск похожих DPO-пар, кандидат-ретривер), НО он не несёт ни обучаемого состояния, ни сигнала обучения сам по себе.
- **«Граф» здесь — это НЕ DynamicGraph.** `HnswIndex._graph` — это навигационный граф соседства HNSW (структура индекса для ускорения поиска), он не имеет отношения к графу узлов/рёбер ILearningUnit/Flow/Orchestrator. По Graph-Style/DynamicGraph Reconstruction Gate рёбра здесь не несут trainable-authority; путать их с рёбрами оркестрации нельзя.
- **DPO / обучение / измерение / teacher-provenance: отсутствуют полностью.** Нет тренировочных данных, нет learning signal, нет evaluate/checkpoint/manifest/locator, нет export/import обучаемого состояния (Save/Load у HNSW сериализует только индекс данных, не «веса модели»; у BruteForce Save/Load вообще `NotImplementedException`). Нет teacher/provenance-полей. Значит, по lifecycle-чеклисту это не обучаемый юнит.

## Классификация кода (по правилам скилов)

| Элемент | Уровень / классификация | Почему (ссылка на правило) |
|---|---|---|
| `IVectorIndex` + `BruteForceIndex` + `HnswIndex` (Add/Search) | **Внутренний детерминистический хелпер — retriever/nearest-neighbour store**, НЕ один из 5 разрешённых уровней | `trainable-architecture-decision-SKILL.md`: «retrievers ... KNN/Jaccard/BM25 stores ... are not architecture patterns»; допустимы только как internal detail внутри `ILogicalUnit/Flow/Orchestrator/ILearningMachine/Pair`. Здесь нет ни обёртки уровнем, ни lifecycle-evidence. |
| `Distances.*` | Внутренний детерминистический хелпер (чистые матем-функции) | То же; это support-вычисление, не authority и не trainable. |
| `HnswIndex._graph` (граф соседства) | Внутренняя структура индекса (детерминистическая), НЕ рёбра DynamicGraph | `generated-code-review-SKILL.md` Graph gate / Deterministic-Boundary gate: это не trainable-рёбра оркестрации. |
| Save/Load (`HnswIndex`) | Сериализация данных индекса, НЕ checkpoint обучаемой модели | Layer 5 (`generated-code-review-SKILL.md`): нет numeric metrics, нет learned state — это persist данных, а не модели. |

Итоговый verdict уровня скила: **OK_INTERNAL_DETAIL** (честный детерминистический ретривер) / при использовании в графе — **OK_MODEL_COMPONENT** с ролью «KNN/nearest memory, support evidence», но **никогда не runtime-authority обучаемого решения** без обёртки разрешённым уровнем и lifecycle-доказательств. Если план/отчёт назовёт этот поиск «обучаемым семантическим юнитом» или «final authority» — это `FORBIDDEN_RUNTIME_SHORTCUT` / `REPORT_CLAIM_TOO_STRONG`.

Заметки соответствия проекту:
- Контракт НЕ возвращает `OperationResult<T>` и публичные методы могут бросать (`NotImplementedException` в BruteForce Save/Load) — расходится с Rule 1 проекта; при втягивании в обучаемый pipeline это надо оборачивать.
- Метрики `EuclideanDistance`/`InnerProduct` объявлены, но мертвы (search хардкодит косинус) — потенциальный мини-долг.

## Пробелы / что доисследовать

- **Кто потребляет `IVectorIndex`** в репозитории (ретривер для какого юнита/флоу?). В csproj нет внешних ссылок, поэтому связи нужно искать по обратным зависимостям в `15.OrchestratorModel`, `42.ProjectWikiDomain`, `25.ModelServer` — не входило в эту узкую под-задачу.
- Нет тестов в самом проекте `06.VectorSearch` — где покрытие kNN/HNSW (recall vs BruteForce)? Не проверял другие тест-проекты.
- Для ILearningDynamicGraph открытый вопрос: нужен ли batch/many→many ретрив (фан-аут запросов, слияние) и фильтрация по `Metadata` — сейчас этого нет, это потребует расширения контракта (новая перегрузка `Search`), а не правки одной строки.
- Не исследовал, есть ли альтернативный векторный поиск/эмбеддинги в других подпроектах (этот рекон строго ограничен `06.VectorSearch`).
