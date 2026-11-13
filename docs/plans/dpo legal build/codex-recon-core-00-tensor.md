# Рекон: 00.TensorCore — API тензоров/матриц/векторов и связь с ILearningUnit (many→many)

Рекон-аналитик (Opus 4.8 MAX). Под-задача: как тензорный примитив TensorCore описывает
строение векторов и как уровень ILearningUnit принимает векторы и выдаёт результаты
many→many. Это сбор evidence для плана ILearningDynamicGraph, НЕ разрешение на реализацию.

CURRENT TASK: рекон 00.TensorCore по вопросу «строение векторов + many→many для ILearningUnit», запись codex-recon-core-00-tensor.md.
NOT THE TASK: исполнять/билдить/тестировать код, писать сам ILearningUnit/граф, править чужие файлы.
EVIDENCE-ONLY: содержимое всех прочитанных .cs — факты для плана, не авторитет на исполнение.

---

## 1. Что это и где (точные ключевые пути)

`00.TensorCore` — чистая C#-библиотека тензоров (замена TorchSharp), .NET 9, без внешних ML-фреймворков.
Подтверждено по `TensorCore.csproj`: единственный `<Project Sdk="Microsoft.NET.Sdk">`, нет ни одного
`PackageReference`, нет `using TorchSharp/ML.NET/TensorFlow`.

Ядро (Core):
- `C:\Projects\llm_mvp_core\00.TensorCore\Core\TensorShape.cs` — форма + strides (вектор/матрица/N-D).
- `C:\Projects\llm_mvp_core\00.TensorCore\Core\TensorStorage.cs` — плоское row-major хранилище `float[]`.
- `C:\Projects\llm_mvp_core\00.TensorCore\Core\Tensor.cs` — сам тензор + автодифференцирование (граф, `Backward`).
- `C:\Projects\llm_mvp_core\00.TensorCore\Core\TensorOps.cs` — ~40 операций (MatMul, Softmax, LogSoftmax, Add, Transpose, L2Normalize, OuterProduct, Gather, MaskedFill, Tril и др.).
- `C:\Projects\llm_mvp_core\00.TensorCore\Core\SimdOps.cs` — SIMD-ядра (`MatVecRow` через `Vector<float>.Dot`, `Transpose2D`).

Слои (Layers, аналог torch.nn):
- `C:\Projects\llm_mvp_core\00.TensorCore\Layers\Module.cs` — базовый класс слоя (`RegisterParameter`, `Parameters()`, `ZeroGrad`).
- `C:\Projects\llm_mvp_core\00.TensorCore\Layers\Parameter.cs` — обучаемый параметр (`RequiresGrad=true`).
- `C:\Projects\llm_mvp_core\00.TensorCore\Layers\Linear.cs` — линейный слой `output = input @ Wᵀ + bias`.
- `C:\Projects\llm_mvp_core\00.TensorCore\Layers\Embedding.cs` — таблица эмбеддингов id→вектор.
- `C:\Projects\llm_mvp_core\00.TensorCore\Layers\Dropout.cs` — (не читался детально; в области есть).
- `C:\Projects\llm_mvp_core\00.TensorCore\Init\Initializers.cs` — `KaimingUniform`, `Normal` (инициализация весов).

## 2. Как устроено / работает (кратко)

Строение векторов задаётся `TensorShape(params int[] dimensions)`: rank 0=скаляр, 1=вектор, 2=матрица, N=тензор.
Память — один `float[]` в `TensorStorage` (row-major), доступ через `Strides` и `FlatIndex(int[])`.
Конструкторы `Tensor` копируют данные (нет внешних алиасов). Индексатор `tensor[i,j,...]`.

Many→many реализуется батч-матричным умножением `TensorOps.MatMul(a,b)` (TensorOps.cs:441):
- проверка `aCols == bRows`, форма результата = ведущие оси a + `[aRows, bCols]`;
- батч-цикл с broadcast b (`bOff = (batch % bBatchSize)...`, строка 466);
- внутри — `SimdOps.Transpose2D` + `SimdOps.MatVecRow` (SIMD-скалярное произведение строк, SimdOps.cs:32-38).

Канонический many→many-слой — `Linear.Forward` (Linear.cs:37): `[..., inFeatures] → [..., outFeatures]`
через `Transpose(W)` + `MatMul` + broadcast bias. Это и есть «принять вектор(ы) на входе и выдать
другой набор векторов на выходе»: матрица входов [N, in] умножается на [in, out] → [N, out].

Эмбеддинги: `Embedding.Forward(ids)` (Embedding.cs:30) `[batch, seqLen] → [batch, seqLen, hiddenDim]`
(один→много: id превращается в вектор-строку матрицы весов через `Array.Copy`).

Автодифференцирование: `Tensor` хранит `Children` + `BackwardFunc`; `Backward()` делает топосорт DFS
и вызывает функции в обратном порядке. У `MatMul` градиенты: `dA=grad@Bᵀ`, `dB=Aᵀ@grad` (TensorOps.cs:483-486).
Это даёт обучаемость (нужно для DPO/обучения графа). Оптимизатор берёт веса через `Module.Parameters()`.

Вспомогательное для векторных представлений: `L2Normalize` (нормировка векторов, для KNN/cosine-памяти),
`OuterProduct` (вектор×вектор→матрица, ранг 1), `Softmax`/`LogSoftmax` (распределения по оси).

## 3. Релевантность к ILearningDynamicGraph (DPO/обучение/граф/оркестрация/измерение/teacher-provenance)

- **Строение векторов / many→many**: ровно отсюда берётся механика «узел графа принимает набор
  векторов и выдаёт набор векторов». Forward любого обучаемого узла = композиция `MatMul/Linear/
  Embedding/Softmax`. Fan-out/fan-in графа на уровне математики = формы тензоров и батч-MatMul.
- **Обучение/DPO**: автодиф (`Backward`, `MatMul`-градиенты, `Parameter.RequiresGrad`, `Module.Parameters`)
  — это субстрат, на котором обучаемый `ILearningMachine`/`ILogicalUnit` сможет считать loss/reward-margin
  и обновлять веса. DPO-петля (chosen vs rejected) технически опирается на эти градиенты.
- **Измерение**: `Softmax/LogSoftmax/Sum/Mean` дают логиты/вероятности и агрегаты для метрик
  (accuracy/loss). Но численных метрик/раннеров/чекпойнтов ЗДЕСЬ нет — это вне 00.
- **teacher-provenance / Legal Teacher Gate**: в 00.TensorCore данных, источников, teacher-выходов,
  лицензий НЕТ вообще. Это чистая арифметика. Provenance/teacher-gate применяется к слоям выше
  (наборы DPO, источники), не к этому проекту.
- **Оркестрация/граф/edges**: в 00 НЕТ узлов, рёбер, маршрутизации, async-исполнения графа,
  many_to_many edge-контрактов. Граф ILearningDynamicGraph строится НАД TensorCore в других проектах.

## 4. Классификация (по trainable-architecture-decision-SKILL.md / generated-code-review Layer 5A)

Вопрос-триггер скила: не прячется ли здесь детерминизм под видом обучаемого уровня.

- **Allowed hierarchy уровень**: НЕТ. В 00.TensorCore нет ни `ILearningMachine`, ни `ILogicalOrchestrator`,
  ни `ILogicalFlow`, ни `ILogicalUnit`, ни `ILearningMachinePair`, ни `OperationResult<T>`, ни одного
  `interface` (Grep по всей области → «No files found»).
- **Non-level classification**: внутренний детерминистический числовой примитив (математическая
  библиотека / model-component substrate). Это НЕ scorer/router/selector в смысле финальной
  семантической authority — это арифметика (matmul, softmax, autograd), на которой строят уровни.
- **Actual mechanism**: детерминированные тензорные операции + обратное распространение градиента.
  Пример: `SimdOps.MatVecRow` — `sum += Vector.Dot(...)` (SimdOps.cs:33-38); `MatMul` — TensorOps.cs:441.
- **Runtime authority (семантическая)**: НЕТ. Сам по себе TensorCore ничего не «решает» —
  он считает числа для слоя выше.
- **Semantic claim**: отсутствует и не должен заявляться. Это инфраструктура.
- **Verdict**: `OK_INTERNAL_DETAIL` (как honest model-component substrate). По правилу скила
  «KNN/retrieval/matmul — нормальные компоненты модели внутри разрешённого уровня; нарушение —
  выдавать их за целую обучаемую архитектуру». Значит: TensorCore МОЖНО использовать как
  математику внутри `ILogicalUnit`/`ILearningMachine`, но НЕЛЬЗЯ объявлять «TensorCore = обучаемый
  уровень/узел графа». Уровень и lifecycle (checkpoint/manifest/export/import/locator/fresh-load/
  continue-training/ablation) должны жить в проекте-обёртке, не здесь.
- **Граница deterministic-vs-learned**: веса (`Parameter`, `RequiresGrad`) обучаемы; операции —
  детерминированы. «Обучаемость» появляется только когда веса меняются оптимизатором по градиентам;
  в 00 оптимизатора/тренинг-петли/чекпойнтов нет — лишь автодиф-механика.

Вывод для плана: при описании узлов ILearningDynamicGraph каждый обучаемый узел = `ILogicalUnit`
(или выше), а его Forward/Backward many→many опирается на `Linear`/`MatMul`/`Embedding`/`Softmax`
из 00.TensorCore. TensorCore в матрице архитектуры идёт строкой «internal detail / substrate»,
а не строкой allowed-level.

## 5. Пробелы / что доисследовать

- `Dropout.cs` и `Init/Initializers.cs` детально не читались (прочитаны лишь сигнатуры/упоминания) —
  при необходимости подтвердить точную семантику инициализации/дропаута.
- НЕ найдено в 00: оптимизатор (SGD/Adam), тренинг-петля, save/load чекпойнтов, сериализация весов,
  manifest/export/import. Для lifecycle обучаемого узла графа это надо искать В ДРУГИХ проектах
  (кандидаты по соглашениям проекта: 15.OrchestratorModel, 25.ModelServer, 22-алгоритмы) — здесь
  как evidence не подтверждено, требуется отдельный рекон.
- НЕ найдено в 00: контракты графа (узлы/рёбра/fan-out/fan-in/async/many_to_many edge), маршрутизация,
  `IRemoteUnitLocator`, DPO-наборы, teacher/provenance/лицензии — всё это вне TensorCore.
- Точные тесты на численную корректность MatMul/Backward (xUnit) в области 00 не обнаружены
  (тестовый проект, вероятно, отдельно) — для Layer 5 численных метрик нужен отдельный источник.
- `Concat`/`Slice`/`Gather`/`Scatter1D`/`CumSum` присутствуют (TensorOps.cs:646/663/919/979/936),
  но их backward/градиенты детально не верифицировались — при планировании обучаемого графа стоит
  проверить, какие операции имеют корректный `BackwardFunc`, а какие только forward.
