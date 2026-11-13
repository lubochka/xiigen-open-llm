# 63. Случайное состояние HNSW и реальные пути восстановления

16 октября 2026. **READ_ONLY_RESEARCH**. `/root/rp01_code_trace_astra`, единственный Astra ultra worker, без вложенных агентов. Создан только этот Markdown. Ни proxy/эксперименты, ни tests/build/inference/training не запускались; продукт, индексы, данные и веса не менялись.

**Общий RNG и отсутствие его состояния в HNSW snapshot подтверждаются по source. Но production-цепочка «HnswIndex.Load → новые Add» не найдена в проверенном tracked C#.** Реальный capability restore загружает corpus и заново строит индекс через Train. Для него возможна иная случайная топология при rebuild. Это условный механизм воспроизводимости learned memory, не наблюдённая потеря качества и не установленная причина neural free answers RP01.

Предложенный helper A/B требует исправления: два независимых холодных процесса имеют **не зафиксированные** начальные RNG. Даже без посторонних Add они могут построить разные продолжения. Одинаковые байты base.json фиксируют граф, но не генератор. Такой A/B смешивает cold-start variation с межиндексным влиянием.

Core: `216a1f3d6af3f40ceef081dc168b07e4ce3934f9`; planning: `1ebc4bac58cf305c0ec1f60c18aa4d19eebf5f99`. Scoped diff от helper core `32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7` пуст для восьми проверенных файлов: HnswIndex, Distances, IVectorIndex, двух Units, TeachingRegistryPersistence, DefaultLogicalCapabilityTrainer, VectorSearchTests. Их scoped worktree status также пуст. Это source provenance, не аттестация runtime binary.

## Что сохраняется, а что продолжает меняться

[HnswIndex:36](C:/Projects/llm_mvp_core/06.VectorSearch/HnswIndex.cs:36) содержит `private static readonly Random _rng = new()`. Конструктор принимает только m/efConstruction/efSearch; публичного seed/state параметра нет. Экземпляры одной загруженной реализации HnswIndex используют один объект RNG; отдельные процессы не разделяют этот объект.

Каждый [Add:49](C:/Projects/llm_mvp_core/06.VectorSearch/HnswIndex.cs:49) вызывает RandomLevel и создаёт соответствующее число уровней; затем уровень влияет на обход, связи и смену entry point. [RandomLevel:156](C:/Projects/llm_mvp_core/06.VectorSearch/HnswIndex.cs:156) вызывает NextDouble в цикле. Для штатного m>1 порог равен 1/m; число draws зависит от результата, а не равно фиксированному одному draw на вставку. Поэтому N чужих Add нельзя автоматически считать сдвигом ровно на N draws.

[Save:115](C:/Projects/llm_mvp_core/06.VectorSearch/HnswIndex.cs:115) сохраняет vectors, IDs, metadata, graph, entry point, max level и m/ef. [SerializedIndex:233](C:/Projects/llm_mvp_core/06.VectorSearch/HnswIndex.cs:233) не имеет RNG seed/state/cursor. [Load:134](C:/Projects/llm_mvp_core/06.VectorSearch/HnswIndex.cs:134) копирует эти данные в новый объект; он **не вызывает Add/RandomLevel**, не перестраивает adjacency и не восстанавливает RNG. В том же процессе Load не возвращает общий RNG в состояние времени Save; в новом процессе snapshot графа не задаёт начальную случайную последовательность.

Отсюда следует последовательная связь, не требующая потоковой гонки: посторонний индекс вызывает Add → потребляет общий RNG → следующий Add целевого индекса читает другой участок последовательности. **Уровень может измениться**, но не обязан: разные draws способны дать один уровень. Даже изменившиеся уровни/граф не гарантируют иной nearest ID или худшее качество. Ни направление качества-эффекта, ни его частота не выведены из наличия static.

## Сильный отрицательный контроль: Search без Add

[Search:99](C:/Projects/llm_mvp_core/06.VectorSearch/HnswIndex.cs:99), [GreedySearch:164](C:/Projects/llm_mvp_core/06.VectorSearch/HnswIndex.cs:164), [SearchLayer:190](C:/Projects/llm_mvp_core/06.VectorSearch/HnswIndex.cs:190) не обращаются к RNG. Они читают vectors/graph/entry/config, создают локальные visited/candidates/results. [ScoreDesc:182](C:/Projects/llm_mvp_core/06.VectorSearch/HnswIndex.cs:182) разбирает равные score по внутреннему ID; [Distances.CosineSimilarity:10](C:/Projects/llm_mvp_core/06.VectorSearch/Distances.cs:10) также не использует RNG.

При одном валидном сохранённом графе, тех же конечных векторах, query, topK, efSearch и реализации вычислений:

- Save→Load→Search без новых целевых Add не меняет ответ **из-за пропущенного RNG state**.
- Посторонние Search не потребляют общий RNG и не меняют поток будущих уровней.
- Даже посторонние Add не меняют уже загруженный целевой graph. Пока целевой индекс только ищет, сдвиг общего RNG ему не нужен.

Эти утверждения предполагают отсутствие параллельной мутации целевого graph, SetEfSearch и записи в retained vector arrays. Последний механизм уже рассмотрен в [51](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/51-vector-index-admission-and-partial-state.md:59), а не является RNG-эффектом. Здесь нет обещания общей неизменности любого индекса при произвольных внешних записях.

В [VectorSearchTests:55](C:/Projects/llm_mvp_core/Tests/VectorSearchTests.cs:55) есть source-fixture из двух finite vectors: Save, Load, один Search, ожидаемый ID `a`. Он **не добавляет узлы после Load**, не сравнивает continuation histories и не исполнен в этом исследовании. Его название PreservesResults нельзя выдавать за уже проведённую проверку RNG continuation.

## Найденные callers: rebuild отдельно от Load→Add

Tracked поиск `HnswIndex` во всех C# и точный поиск `HnswIndex.Load` дали один внешний прямой Load — названный тест на строке 63. Production Load→Add не найден. Это ограниченный текстовый результат: внешние сборки, динамические вызовы и reflection не объявлены невозможными. [IVectorIndex:9](C:/Projects/llm_mvp_core/06.VectorSearch/IVectorIndex.cs:9) содержит только Count/Add/Search; скрытого стандартного Load в этом интерфейсе нет.

| Реальный source-путь | Где возникает случайность и какой результат потребляется |
|---|---|
| Generated capability: создание | [DefaultLogicalCapabilityTrainer:949](C:/Projects/llm_mvp_core/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs:949) фиксирует ordered corpus и вызывает TransformerEmbeddingKnnLogicalUnit.Train. [Train:114](C:/Projects/llm_mvp_core/18.CapabilityTraining/LlmIngredientLogicalUnits.cs:114) создаёт новый HNSW и последовательно Add для embeddings примеров. |
| Generated capability: cold restore | [TeachingRegistryPersistence:1693](C:/Projects/llm_mvp_core/18.CapabilityTraining/TeachingRegistryPersistence.cs:1693) загружает neural checkpoint, читает corpus examples и вызывает Train заново. Это **rebuild всех узлов**, а не загрузка сохранённой HNSW adjacency. Поэтому граф может зависеть от RNG и порядка других построений уже во время restore, даже если после завершения restore вызываются только запросы. |
| Generated capability: обычный ответ | [GeneratedCapabilityFlow:7383](C:/Projects/llm_mvp_core/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs:7383) вызывает Unit.ExecuteAsync. [Unit:181](C:/Projects/llm_mvp_core/18.CapabilityTraining/LlmIngredientLogicalUnits.cs:181) получает кандидатов, применяет fusion и возвращает сохранённый `example.Output`. Это полезный потребитель обученной памяти, не нейронная свободная генерация. |
| Generic KnnLogicalUnit | [Train:136](C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:136) обучает tokenizer, создаёт новый индекс и добавляет все examples. [ExecuteAsync:221](C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:221) ищет `_k` соседей и возвращает `bestExample.Output`. В этом классе не найден incremental Add после публикации Unit. |

Helper предлагает после m=2/4 подтвердить эффект на «production default m=16». Это **дефолт конструктора**, но не конфигурация этих callers: Transformer Unit использует **m=8, efConstruction=32, efSearch=16**; generic Unit — **m=8, efConstruction=64, efSearch=32**. Доменный опыт должен повторить именно установленный consumer/config. Уменьшение m/ef в механическом stress fixture не оценивает частоту дефекта на реальном пути.

У Transformer Unit есть особенно сильное ограничение влияния topology: Search запрашивает `topK=Math.Max(1,_examples.Count)`, а HNSW использует `ef=Math.Max(efSearch,topK)`. Если из стартового узла на уровне 0 достижимы все N узлов, этот обход соберёт весь corpus, а не малый ANN shortlist. При тех же vectors/queries сохранится тот же набор cosine score; [ScoreCandidates:217](C:/Projects/llm_mvp_core/18.CapabilityTraining/LlmIngredientLogicalUnits.cs:217) детерминированно сортирует fusion score и затем ExampleId. **Другой graph при полной достижимости тогда не меняет итоговый selected output.**

Для этого consumer нужны свидетельства изменившейся достижимости/набора кандидатов, а не только graph hash. Уже разобранный reverse-prune в [45](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/45-memory-reachability-and-context-consumption.md:26) может нарушать достижимость, но этот пакет не переаудирует его и не утверждает, что каждый RNG сдвиг обязательно делает это. У generic `_k` может быть меньше corpus; там широкий-полный контроль автоматически не действует.

Для прямого RP01 neural free generation вызванная цепочка не включает этот индекс; разграничение generation/retrieval сохранено в [51:71](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/51-vector-index-admission-and-partial-state.md:71) и прямом adapter-разборе [53](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/53-translation-composition-and-direction-consumers.md:11). Данных, что HNSW RNG объясняет RP01 0/3 или 0/4, этот пакет не добавляет.

## Почему helper A/B не выделяет причину

В [finding:27](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-NEW-HNSW-RNG-CONTINUATION-AND-COUPLING-2026-10-16.md:27>) и [MICRO:19](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-MICRO-CAUSAL-EXECUTOR-AND-EXPERIMENT-2026-10-16.md:19>) A/B стартуют в разных свежих процессах, читают одинаковый base JSON, затем только B делает 64 чужих Add. Однако private Random создан без фиксированного seed; исходное случайное состояние у A/B не уравнено. Причины различий: **начальная RNG history плюс вмешательство B**, а не только B. Один дополнительный холодный A/A обнаруживает baseline variation, но сам не исправляет причинную идентификацию пары A/B. Три повтора тоже не превращают независимые состояния в парный контроль.

Точно так же последовательное «сначала A, потом B» в одном процессе не помогает: A уже сдвигает static RNG. Нельзя reset только graph и считать reset генератора состоявшимся. Exact BruteForce с теми же векторами полезен как контроль retrieval/labels, но не фиксирует RNG HNSW и не очищает эту смешанную причину.

В source нет синхронизации самого Add/graph и чтения `_rng` на уровне HnswIndex. Для установленного sequential coupling она не нужна. Реальный concurrent caller и фактическая гонка здесь **не доказаны**; внутреннюю thread-safety конкретной реализации System.Random отдельно не исследовали. Будущий причинный опыт должен быть последовательным, чтобы не добавлять планирование потоков как ещё одну переменную.

## Исправленный малый будущий протокол — только проект

Сейчас ни harness, ни seed seam, ни индекс/fixture не созданы. Прямое разрешение на поиск не разрешает исполнение или изменение HnswIndex. Публичный current API не даёт зафиксировать RNG; будущий управляемый native опыт требует отдельно разрешённого test-only seam для одинакового начального Random stream/state. Подмена private readonly поля reflection или скрытая production-правка не предлагаются.

1. **Нулевой контроль, без целевых Add.** Одни base bytes и запросы; сравнить original/load Search и Search после постороннего Add. Целевые vectors/graph/config не менять. Предсказание по source: RNG-эффекта нет. Изменение результата здесь требует искать другую переменную, а не объяснять его невосстановленным RNG.
2. **Изоляция межиндексного влияния.** В будущем одинаково подготовить A0/A1/B: один base graph, одинаковый начальный shared RNG state, фиксированные vectors/IDs/order и single-thread execution. A0 и A1 — одинаковое целевое продолжение без чужих Add; B — ровно те же шаги плюс заранее заданные чужие Add до продолжения. Отдельный контроль чужих Search должен читать готовый чужой graph без его построения через Add. Зафиксировать фактическую последовательность draws/levels, затем topology и hits. A0=A1 обязателен прежде интерпретации B; разные levels подтверждают эффект сдвига, но не потерю качества.
3. **Изоляция непродолженного RNG snapshot.** Отдельная пара: одинаково воспроизвести prefix до save с одним seed/state. A продолжает без перезапуска; B восстанавливает graph, но также получает **точное RNG state на границе Save** через будущий контрольный seam. A/B должны совпасть. Затем отдельный B0 с тем же graph и намеренно начальным вместо продолженного state выделяет missing-state фактор. Здесь не добавлять посторонние индексы. Seed без cursor/state недостаточен, если генератор после Save возвращён к началу.

Только после локального механизма нужен **один установленный consumer**, а не обещание сразу двух доменов: его фактические m/ef/topK, тот же корпус и byte-identical vectors/query vectors, tokenizer/model identity, тот же порядок примеров. Для capability restore сравнивать фактический rebuild, а не подставлять неиспользуемый Load→Add путь. Записать selected lesson ID, обычный returned output и заранее проверенный gold; для Transformer consumer отдельно размер/состав достижимого candidate set. Разные графы при одинаковом правильном ответе означают variation без измеренного quality delta; изменение ответа может как улучшить, так и ухудшить его. Exact-neighbour recall и смысловая правильность stored lesson — разные метрики.

Успех будущей per-index RNG/state реализации докажет воспроизводимость заданного continuation. Он сам по себе не докажет лучшую ANN-топологию, не исправит другие HNSW ошибки и не покажет обучение neural Student.

## Покрытие и следующая первичная граница

Полностью прочитан 38-строчный [N33 RNG finding](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-NEW-HNSW-RNG-CONTINUATION-AND-COUPLING-2026-10-16.md:1>); у MICRO — строки 1–22 с RNG-разделом. HnswIndex прочитан полностью (246 строк); IVectorIndex и Distances — полностью. У Units, capability trainer/persistence и существующего test — только процитированные участки. У 51 адресно перепроверены строки 55–71; первоначальный объединённый вывод был усечён в середине 51, полного свежего перечитывания 51 не заявляется. Tracked C# constructor/Load search проведён; история HnswIndex ограничена тремя последними log entries, без утверждения о дате появления RNG-механизма. E09, UTF8 и остальные новые темы не переисследовались.

Governance/skills найдены и прочитаны адресно: local AGENTS 1–16, canonical AGENTS 1590–1610, CLAUDE 1029–1042; architect 340–346, planning startup 305–313, plan-review 674–684, generated-code-review 75–84, prompt-patch 26–34, readability 26–35, human-readable architecture 100–113. Это targeted load, не полный reload, не независимые review-проходы. Никакие helper-пометки READY/ALLOWED не использованы как разрешение запуска.

Точный открытый lead — signed corpus/checkpoint и сохранённый ordinary вызов **TeachingRegistryPersistence.cs:1693 → TransformerEmbeddingKnnLogicalUnit.Train:98 → ExecuteAsync:181**, с graph/candidate-set evidence при rebuild. Для самостоятельного API-сценария Load→Add прежде нужен реальный caller, которого этот tracked search не нашёл. Ни инцидентный corpus, ни native continuation receipt здесь не установлены. Общие индексы/лог не изменены; пакет не завершает общее исследование.
