# Рекон: 05.Embeddings — построение векторов для входов юнитов

Контекст: РЕКОН для плана ILearningDynamicGraph (авто-построение DPO для
ILearningUnit/Flow/Orchestrator/DynamicGraph). Это сбор evidence, не разрешение
на реализацию. Все пути — реально найденные.

## Что это и где (точные ключевые пути)

Проект `05.Embeddings` — крошечный, ровно 2 production-файла:

- `C:/Projects/llm_mvp_core/05.Embeddings/EmbeddingModel.cs` — класс
  `EmbeddingModel` (namespace `Embeddings`).
- `C:/Projects/llm_mvp_core/05.Embeddings/MeanPooling.cs` — статический класс
  `MeanPooling` (namespace `Embeddings`).
- `C:/Projects/llm_mvp_core/05.Embeddings/Embeddings.csproj` — ссылки на
  `00.TensorCore`, `01.Tokenizer`, `03.TransformerModel`. ML-фреймворков нет
  (Rule 10 соблюдён).

Зависимости вверх по стеку:
- `C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:52` —
  `LlmModel.ForwardHidden(Tensor tokenIds)` отдаёт скрытые состояния `[B,T,D]`.
- `C:/Projects/llm_mvp_core/00.TensorCore` — `TensorOps.L2Normalize`.

Кто потребляет (главное под вопрос «векторы для входов юнитов»):
- `C:/Projects/llm_mvp_core/18.CapabilityTraining/LlmIngredientLogicalUnits.cs:31`
  — `TransformerEmbeddingKnnLogicalUnit<TOutput> : ILogicalUnit<string,TOutput>`.
- `C:/Projects/llm_mvp_core/08.GraphRag/Indexer.cs:14,30` — индексация узлов
  графа через `_embedder.Embed(text)`.
- Тесты: `C:/Projects/llm_mvp_core/Tests/MeanPoolingTests.cs`,
  `Tests/LlmIngredientLogicalUnitTests.cs`, `Tests/IntegrationTests2.cs`.
- Док: `C:/Projects/llm_mvp_core/docs/projects/05-embeddings.md`.

## Как устроено / работает (кратко)

`EmbeddingModel` превращает текст в фиксированный вещественный вектор-эмбеддинг
через transformer-энкодер (не lm_head, а скрытые состояния):

1. `Embed(text)` → обёртка над `EmbedBatch([text])[0]`.
2. `EmbedBatch(texts)` (EmbeddingModel.cs:26): токенизация + padding
   (`Tokenize`, обрезка до `maxLen`), затем `_encoder.ForwardHidden(ids)` →
   `[B,T,D]`; `MeanPooling.Pool(hidden, mask)` → `[B,D]`;
   `MeanPooling.Normalize` → L2-нормализация → `[B,D]`; разрезание на строки.
3. `EnsureNonZeroUnitVector` (EmbeddingModel.cs:44) — страховка: если вектор
   почти нулевой, заполняет его по token-id (`tokenId % len += 1`), при пустой
   токенизации — по `GetHashCode` строки, и нормализует. Детерминистический
   fallback, не семантика.
4. `Dimensions => _encoder.Config.HiddenDim`.

`MeanPooling.Pool` (MeanPooling.cs:14) — взвешенное по attention-mask среднее
токен-векторов (padding с маской 0 не искажает), деление на `max(maskSum,1e-9)`.
`Normalize` (MeanPooling.cs:48) — `TensorOps.L2Normalize(eps:1e-9)`; после неё
cosine = dot product.

Возвраты: пакет `OperationResult<T>` НЕ использует (это чистый числовой
примитив, не сервис; Rule 1 здесь неприменим — нет бизнес-логики/контракта
сервиса). Комментарии построчные есть (Rule 3 ок).

## Релевантность к ILearningDynamicGraph

Прямая и существенная, но как ОБЩИЙ ПРИМИТИВ, а не как обучаемый узел графа:

- «Векторы для входов юнитов» реализуются именно тут. Канонический путь:
  `LlmIngredientLogicalUnits.cs:142` — `_embeddingModel.Embed(request.Input)`
  строит query-вектор, который дальше идёт в HNSW-поиск
  (`06.VectorSearch:HnswIndex`) и решение fusion. То есть `05.Embeddings` —
  входной feature-extractor для `ILogicalUnit`/будущих узлов графа.
- При обучении (`Train`, :62-126) каждый DPO/пример эмбеддится
  (`embeddingModel.Embed(example.Text)`, :91) и кладётся в индекс. Эмбеддинги —
  поставщик представлений для построения обучающих/DPO наборов.
- Provenance: путь явно декларируется в `LlmIngredientManifest` как
  `"05.Embeddings:EmbeddingModel"` (:105) — годится как teacher/ingredient
  provenance-маркер для графа.
- Граф/оркестрация/измерение: сам по себе проект НЕ содержит узлов, рёбер,
  fan-out/fan-in, оркестрации или метрик обучения — он лишь даёт вектор. Любая
  «authority на ребре» лежит у вызывающего юнита/флоу, не здесь.
- Teacher-provenance (Legal Teacher Gate скила): эмбеддинги считаются локальной
  собственной моделью (`03.TransformerModel`), внешних закрытых teacher здесь
  нет — gate тут не нарушается; провенанс к проверке относится к данным/учителям
  выше по стеку.

## Классификация (по trainable-architecture-decision-SKILL.md)

`EmbeddingModel` и `MeanPooling`:
- Уровень иерархии ILearningMachine/Orchestrator/Flow/Unit/Pair: НЕ являются ни
  одним из разрешённых уровней. Это feature-extractor / числовой компонент
  модели.
- Классификация: `OK_MODEL_COMPONENT` (см. skill §Verdicts и KNN/retrieval note:
  «It may be a normal model architecture component, ... feature source inside an
  allowed level»). Эмбеддинги — честный компонент-источник признаков внутри
  разрешённого уровня (его оборачивает `TransformerEmbeddingKnnLogicalUnit :
  ILogicalUnit`).
- НЕ является скрытым детерминистическим шорткатом, маскирующимся под обучаемый
  юнит: класс ничего не «решает» и не выдаёт финальную authority; он возвращает
  вектор. Семантических claim'ов сам по себе не делает.
- Детерминистический под-фрагмент: `EnsureNonZeroUnitVector` (token-id/hash
  fallback) и `MeanPooling`/L2 — внутренние детерминистические хелперы
  (`OK_INTERNAL_DETAIL`), честно помечены назначением (страховка от нулевого
  вектора, агрегация). Они не должны называться обученной семантикой.
- Граница deterministic-vs-learned: «обученность» приходит из весов
  `03.TransformerModel` (ForwardHidden); pooling+нормализация+fallback —
  детерминированы. Для графа это значит: эмбеддинг — детерминированная функция
  от (веса энкодера, текст), а не обучаемое ребро.

Итог классификации для плана: использовать `05.Embeddings` как
input-vector-builder/feature source внутри узлов ILearningUnit/Flow; не выдавать
его за обучаемый узел или за финальную authority на ребре графа.

## Пробелы / что доисследовать

- Нет собственного lifecycle (export/import/manifest/checkpoint) у самого
  `EmbeddingModel`: веса живут в `03.TransformerModel`. Для DPO-графа уточнить,
  как версионируется/локается энкодер, дающий эмбеддинги (где checkpoint, как
  fresh-load) — это вне `05.Embeddings`.
- `EnsureNonZeroUnitVector` через `GetHashCode`/`tokenId % len`: проверить, не
  создаёт ли это коллизий/ложного сходства на коротких/пустых входах, способных
  протечь как ложный сигнал в KNN-набор (риск leakage в DPO-строках).
- Детерминизм эмбеддингов между запусками/потоками (порядок суммирования
  float, многопоточность в `Indexer` через `Task.Run`) — для воспроизводимых
  DPO-меток стоит проверить стабильность.
- Не прочитаны (вне бюджета 3-8 файлов, при необходимости): тела
  `Tests/MeanPoolingTests.cs`, `Tests/LlmIngredientLogicalUnitTests.cs`,
  `docs/projects/05-embeddings.md`, `06.VectorSearch` (HnswIndex), полный
  `ExecuteAsync`/fusion в `LlmIngredientLogicalUnits.cs` (:150+),
  `08.GraphRag/{LocalSearch,GlobalSearch}.cs` — как эмбеддинги участвуют в графе
  поиска.
- maxLen-усечение (512 в EmbeddingModel, но 64 в CapabilityTraining-пути):
  уточнить влияние короткого окна на качество векторов длинных входов юнитов.
