# Рекон: 33.MatrixAlgorithms — матричные алгоритмы для «упрощённой матрицы» ILearningUnit

## Что это и где (точные ключевые пути)

Чистый C# (.NET 9) пакет линейной алгебры «с нуля», без внешних ML-фреймворков. Один проект-библиотека, единственный потребитель — тестовый проект `Tests/LlmFromScratch.Tests.csproj` (проверено grep по `*.csproj`; других ссылок в репозитории нет).

Ключевые пути:
- `C:/Projects/llm_mvp_core/33.MatrixAlgorithms/Core/Core.cs` — `OperationResult<T>`, `StandardEvent`, `IdempotencyStore`, фабричные интерфейсы `IMatrixStorage`/`IMatrixEventLogger`, абстрактный `BaseService(storage, logger)`.
- `C:/Projects/llm_mvp_core/33.MatrixAlgorithms/Core/Types.cs` — записи результатов: `DecompositionResult`, `SparseMatrixData`, `CompressedWeightMatrix`, `QuantizedWeightMatrix`, `SpectralDecayReport`.
- `C:/Projects/llm_mvp_core/33.MatrixAlgorithms/Fabric/Interfaces.cs` — доменные контракты `IMatrixDecomposer`, `ISparseMatrixStorage`, `IWeightCompressor` + in-memory реализации для тестов.
- `C:/Projects/llm_mvp_core/33.MatrixAlgorithms/Dispatcher/MatrixAlgorithmsDispatcher.cs` — единая точка входа: строковое имя алгоритма + словарь → результат-словарь.
- `C:/Projects/llm_mvp_core/33.MatrixAlgorithms/Families/01-Decomposition/` — LU (`LuDecompositionService`, `LuLinearSolver`), QR (`HouseholderReflection`, `QrDecompositionService`, `QrLeastSquaresSolver`), SVD (`SingularValueDecompositionService`, `TruncatedSvdService`, `RandomizedSvdService`).
- `C:/Projects/llm_mvp_core/33.MatrixAlgorithms/Families/02-Sparse/` — форматы COO/CSR/CSC, `SparseMatrixConverter`, `SparseMatrixMultiplier`, `SparseLinearSolver`.
- `C:/Projects/llm_mvp_core/33.MatrixAlgorithms/Families/03-LowRankCompression/` — `WeightMatrixAnalyzer`, `SvdWeightCompressor`, `Int8Quantizer`, `QuantizationPlusLowRankCompressor`.
- `C:/Projects/llm_mvp_core/33.MatrixAlgorithms/state.json` — `status:"complete"` (2026-04-30), `testCount:43`, перечень семейств и 12 алгоритмов диспетчера.

## Как устроено / работает (кратко)

Паттерн Genie DNA: каждый публичный метод возвращает `OperationResult<T>` (бизнес-ошибки через `Fail`, не исключения); сервисы наследуют `BaseService` и получают `IMatrixStorage` + `IMatrixEventLogger` через конструктор (fabric-first); у каждого — `IdempotencyStore`; перед событием результат сохраняется в storage; на каждую операцию пишется `StandardEvent`.

`MatrixAlgorithmsDispatcher` — `switch` по 12 именам (`lu`, `qr`, `svd`, `truncated-svd`, `randomized-svd`, `lu-solve`, `qr-least-squares`, `sparse-solve`, `compress-weight`, `decompress-weight`, `analyse-weight`, `quantize`); создаёт нужный сервис и читает входы из словаря через `TryGet<T>`.

Все вычисления — детерминированная численная линейная алгебра: разложения (LU с частичным выбором ведущего, QR через отражения Хаусхолдера, полное/усечённое/рандомизированное SVD), решение систем/наименьшие квадраты, операции над разреженными матрицами и сжатие весов (низкоранговое SVD + 8-битное квантование). «Importance»/«rank» — это сингулярные значения, а не обучаемое состояние: например `WeightMatrixAnalyzer.Analyse` (Families/03/WeightMatrixAnalyzer.cs:24) считает энергию `Sum(s*s)`, порог `singularValues[0]*tolerance`, `EstimatedRank` = число значимых сингулярных значений, число обусловленности = отношение макс/мин. `SvdWeightCompressor` (Families/03/SvdWeightCompressor.cs) хранит U, S, V и реконструирует `U·diag(S)·Vᵀ`.

## Релевантность к ILearningDynamicGraph (DPO/обучение/граф/оркестрация/измерение/teacher-provenance)

Прямой архитектурной связи с ILearningDynamicGraph нет: grep по `ILogicalUnit|ILearningMachine|ILogicalFlow|ILogicalOrchestrator|ILearningMachinePair|TrainBatch|DPO|checkpoint|Train(` внутри проекта = 0 совпадений. Нет обучаемого состояния, нет тренировки, нет экспорта/импорта чекпойнтов, нет провенанса teacher/DPO. Это вычислительный примитив, а не уровень иерархии.

Релевантность только как переиспользуемый математический бэкенд для будущей «упрощённой матрицы» ILearningUnit:
- Хранение/обновление весов обучаемого юнита как матрицы; сжатие чекпойнта (`SvdWeightCompressor`, `Int8Quantizer`) и оценка сжимаемости/ранга (`WeightMatrixAnalyzer`) — для меньших экспортируемых чекпойнтов.
- Разреженные форматы (CSR/CSC/COO) для разреженной матрицы признак×решение или матрицы смежности графа узлов/рёбер ILearningDynamicGraph.
- LU/QR-решатели и наименьшие квадраты — для линейных шагов внутри обучающего апдейта.
- Измерение: `ReconstructionError`, `MaxAbsoluteError`, `EnergyFractionCaptured` — численные метрики потерь сжатия (диагностика, не held-out accuracy/DPO-метрики обучаемого юнита).

Важно для плана: эти алгоритмы могут быть только внутренней деталью внутри разрешённого уровня (`ILogicalUnit`/`ILearningMachine`), который и владеет обучаемым состоянием, lifecycle (load user/cache → repo/common fallback → export/import/share → continue-training), DPO-парами и провенансом. Сам пакет 33 ничего из lifecycle/DPO не предоставляет.

## Классификация (правила скилов)

`trainable-architecture-decision-SKILL.md`: разрешённые обучаемые уровни — только ILearningMachine / ILogicalOrchestrator / ILogicalFlow / ILogicalUnit / ILearningMachinePair; «всё остальное — деталь реализации, если не обёрнуто одним из уровней и не имеет lifecycle-доказательств». `generated-code-review-SKILL.md` Layer 5A: scorer/selector/retriever и т.п. — не архитектурные паттерны.

- Уровень: НЕТ (ни один из пяти уровней не реализован и не упомянут).
- Классификация: внутренний детерминистический хелпер / вычислительный примитив (numerical linear algebra). Почему: чистые детерминированные численные функции, без обучаемого состояния, тренировки, инференса-как-обобщения, чекпойнтов и провенанса (Core.cs, Types.cs, все Families/*; нулевой grep по иерархии/DPO/Train).
- Runtime-authority как обучаемого решателя: нет. Скрытого детерминизма «под видом trainable» (FORBIDDEN_RUNTIME_SHORTCUT) нет, потому что проект и не претендует на semantic/trainable — он честно вычислительный.
- Вердикт по скилу: `OK_INTERNAL_DETAIL` — допустимо как внутренняя деталь/`OK_MODEL_COMPONENT` (компонент модели: сжатие/квантование/разреженное хранение/решатели) внутри разрешённого уровня при честной маркировке роли. KNN-нота скила здесь не применяется (KNN/retrieval в проекте отсутствуют).

## Пробелы / что доисследовать

- Не прочитаны (намеренно, защита от перегрузки): реализации LU/QR/SVD-решателей и разреженного семейства целиком (`LuDecompositionService.cs`, `QrDecompositionService.cs`, `SingularValueDecompositionService.cs`, `TruncatedSvdService.cs`, `RandomizedSvdService.cs`, `HouseholderReflection.cs`, `NumericalUtilities.cs`, `DenseMatrix.cs`, `SparseLinearSolver.cs` и др.) — алгоритмическая корректность/численная устойчивость не верифицирована.
- `Int8Quantizer.cs` и `QuantizationPlusLowRankCompressor.cs` прочитаны только по сигнатурам/записям, не по телу — точная схема квантования (scale/zero-point) не подтверждена построчно.
- Нет тестов в самом проекте; 43 теста живут в `Tests/LlmFromScratch.Tests.csproj` — фактические ассерты не просмотрены (тесты не запускались — это рекон).
- Не определено, какую именно «упрощённую матрицу» ILearningUnit план имеет в виду (веса юнита? матрица смежности графа? матрица признак×решение для DPO?) — нужен явный контракт от плана, чтобы выбрать конкретные алгоритмы 33 как внутренние детали.
- Не исследовано, как чекпойнт будущего ILearningUnit сериализуется/публикуется (25.ModelServer/IModelRepository/манифест) и где сжатие из 33 встроится в этот lifecycle.
