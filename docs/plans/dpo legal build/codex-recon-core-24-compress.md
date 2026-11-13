# Рекон: 24.Compression — сжатие как часть жизненного цикла ассетов

## Что это и где (точные ключевые пути)

Чистая C#-библиотека алгоритмов сжатия данных (без внешних ML-фреймворков). Это
байтовые кодеки, не модель и не обучение.

- Контракт кодека: `C:\Projects\llm_mvp_core\24.Compression\Fabric\Interfaces.cs`
  — `ICompressor` (Compress/Decompress), `ICompressionStorage`, `ICompressionBenchmark`.
- Ядро/типы: `C:\Projects\llm_mvp_core\24.Compression\Core\Core.cs` (свой
  локальный `OperationResult<T>`, `BaseService`, `StandardEvent`),
  `C:\Projects\llm_mvp_core\24.Compression\Core\Types.cs` (`CompressionOptions`,
  `CompressionResult` с `CompressionRatio`, `BenchmarkResult`).
- Статус/карта: `C:\Projects\llm_mvp_core\24.Compression\state.json` — 4 семейства,
  все `status: complete`, покрытие "unit + e2e round-trip", last_updated 2026-04-30.
- Семейства алгоритмов: `Families\01-LzFamily` (Snappy/LZAV/LZ4/Zstandard + `LzPrimitives`),
  `Families\02-EntropyCoding` (Huffman/Deflate/rANS + `SymbolStatistics`),
  `Families\03-BwtFamily` (BWT/MTF/RLE/Bzip2Style),
  `Families\04-Specialized` (Brotli, LZMA, OpenZL).

## Как устроено/работает (кратко)

Каждый кодек реализует `ICompressor` и возвращает `OperationResult<CompressionResult>`.
Алгоритмы детерминированы и обратимы (round-trip). "Модельные" по названию части —
это классические компоненты кодеков, а не ML:
- `BrotliContextModel` (`Families\04-Specialized\BrotliContextModel.cs`): плоский
  массив частот `long[256*256*256]`, методы `Train(corpus)`/`RecordObservation` —
  это адаптивная статистика 2-го порядка для энтропийного кодирования; нет
  чекпойнта/экспорта/манифеста, состояние пересобирается по потоку.
- `LzmaMarkovModel` / `LzmaRangeCoder` (`Families\04-Specialized\Lzma*.cs`):
  11-битные адаптивные вероятности (Марков) для арифметического кодера LZMA.
- `BrotliStaticDictionary` (`Families\04-Specialized\BrotliStaticDictionary.cs`):
  жёстко зашитый массив строк (англ./HTML/JS), `FindMatch` по `StartsWith` —
  фиксированный словарь, не обучаемый.
- `OpenZlStructureAnalyzer` (`Families\04-Specialized\OpenZlStructureAnalyzer.cs`):
  детектор CSV/JSON + оценка энтропии колонок (`EstimateStringEntropy`) для
  колоночной перестановки; чистая эвристика на частотах символов.
- `CompressionOptions.DictionaryPath` (`Core\Types.cs`) — путь к
  предобученному словарю для Zstandard/OpenZL; это вход кодека, не learning-state.

## Релевантность к ILearningDynamicGraph

Прямой архитектурной роли (узел/ребро/обучение/DPO/оркестрация/измерение/
teacher-provenance) в графе у 24.Compression НЕТ. Это инфраструктурная утилита
жизненного цикла ассетов. Возможные точки переиспользования планом (как сервис,
не как trainable-уровень):
- Хранение/перенос ассетов: сжатие чекпойнтов модели, DPO-наборов (JSONL),
  манифестов, экспорт/импорт/share для `ILearningMachine` — через `ICompressor`
  + `ICompressionStorage`. Это закрывает "compression = часть lifecycle ассетов".
- Измерение: `CompressionResult.CompressionRatio` / `BenchmarkResult` дают
  численную метрику размера ассета (полезно для отчётов о весе чекпойнта), но это
  НЕ метрика качества обучения (accuracy/loss/reward-margin).
- Teacher-provenance: библиотека нейтральна — она не трогает источники/лицензии
  обучающих данных. Provenance-гейт надо решать на уровне набора, не здесь.
- Внимание: локальный `Compression.OperationResult<T>` отдельный от
  `AgentInfrastructure.OperationResult<T>` — на стыке сборок нужен alias/мост,
  иначе типы не совместимы.

## Классификация (по trainable-architecture-decision-SKILL + generated-code-review Layer 5A)

Уровень: НИ ОДИН из разрешённых trainable-уровней (не `ILearningMachine`,
`ILogicalOrchestrator`, `ILogicalFlow`, `ILogicalUnit`, `ILearningMachinePair`).
Это внутренние ДЕТЕРМИНИСТИЧЕСКИЕ хелперы / инфраструктура кодеков.

Почему (со ссылкой на правило скила):
- `BrotliContextModel.Train/RecordObservation` и Марков-таблицы LZMA — это
  "dictionary/counter state" из списка скила: счётчики частот, которые скил прямо
  относит к internal detail, а НЕ к архитектурному trainable-уровню. Нет
  жизненного цикла (training data как DPO, learning signal, eval, export/import,
  manifest, checkpoint, locator, fresh-load, continue-training, ablation) —
  обязательного для trainable-claim (generated-code-review Layer 5/5A).
- `BrotliStaticDictionary.FindMatch` и `OpenZlStructureAnalyzer.Analyze` —
  фиксированный словарь и regex/`StartsWith`/частотная эвристика → классические
  "regex/keyword/pattern matching", тоже internal detail.
- Семантического/обучаемого claim проект не делает (state.json: алгоритмы +
  round-trip), поэтому masquerade-нарушения НЕТ.
- Вердикт по скилу: `OK_INTERNAL_DETAIL` (детерминированные хелперы честно
  помечены как кодеки). При использовании в плане для ассет-lifecycle —
  это `helper/infrastructure`, не runtime-trainable-authority. Если план попытается
  выдать `BrotliContextModel`/Markov как "обучаемую модель графа" → это будет
  `REPORT_CLAIM_TOO_STRONG` / `NEEDS_ILOGICALUNIT_WRAPPER` с требованием полного
  lifecycle-доказательства.

## Пробелы / что доисследовать

- Тесты не прочитаны (в 24.Compression нет папки тестов; "unit + e2e" заявлены в
  state.json) — где физически лежат тест-проекты для этого модуля, не подтверждено.
- Не проверено, ссылается ли кто-то (15.OrchestratorModel / 25.ModelServer /
  `IModelRepository`) на `Compression.ICompressor` для сжатия чекпойнтов/ассетов —
  нужен grep по решению, чтобы понять, подключён ли модуль к lifecycle ассетов
  фактически или это изолированная библиотека.
- Не прочитаны 19 из 27 файлов (защита от перегруза): LZ-семейство (кроме Zstd
  head), вся EntropyCoding, весь BWT, `OpenZlCompressor`, `LzmaCompressor`,
  `BrotliCompressor` — детали реализации, но на классификацию (детерминированные
  кодеки) это не влияет.
- Формат/совместимость словаря `DictionaryPath` (кто его создаёт, формат файла) —
  не исследован; важно, если план захочет хранить DPO-словари этим путём.
