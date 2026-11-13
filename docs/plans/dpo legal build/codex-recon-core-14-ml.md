# Codex Recon — 14.MachineLearning (ML-примитивы, тренировочные циклы, метрики)

Корпус: core · slug: core-14-ml · Цель: `C:/Projects/llm_mvp_core/14.MachineLearning`
Вопрос рекона: ML-примитивы, тренировочные циклы, метрики точности/лосса.
Скилы для классификации просмотрены (skim):
`docs/skills/trainable-architecture-decision-SKILL.md`,
`docs/skills/generated-code-review-SKILL.md` (оба существуют).

---

## 1. Что это и где (точные ключевые пути)

`14.MachineLearning` — автономная библиотека классических ML-алгоритмов «с нуля»
на чистом C#. Своя локальная инфраструктура (НЕ `AgentInfrastructure`).

Ключевые пути:
- `14.MachineLearning/Core/Core.cs` — локальный `OperationResult<T>` (namespace
  `MachineLearning`, отдельный от `AgentInfrastructure.OperationResult<T>`),
  `StandardEvent`, `IdempotencyStore`, абстрактный `BaseService(IModelStateStorage, IPredictionLogger)`.
- `14.MachineLearning/Fabric/Interfaces.cs` — `IModelStateStorage`
  (`Save/Load/List` через `Dictionary<string,object>`), `ITrainingDataProvider`,
  `IPredictionLogger`.
- `14.MachineLearning/Fabric/InMemoryStorage.cs` — in-memory реализации фабрики.
- `14.MachineLearning/Dispatcher/MachineLearningDispatcher.cs` — роутер по имени
  алгоритма (`linear_regression`, `naive_bayes`, `multilayer_perceptron`, `kmeans`).
- `14.MachineLearning/Algorithms/*.cs` — 7 алгоритмов:
  `LinearRegression.cs`, `NaiveBayes.cs`, `MultilayerPerceptron.cs` (MLP),
  `KMeans.cs`, `Apriori.cs`, `PCA.cs`, `QLearning.cs`.

Прочитано полностью (в рамках бюджета 3–8): Core.cs, Interfaces.cs,
MachineLearningDispatcher.cs, MultilayerPerceptron.cs, LinearRegression.cs,
QLearning.cs (6 файлов) + 2 таргетных grep-прохода по всей области.

---

## 2. Как устроено / работает (кратко)

ML-примитивы и их тренировочные циклы:
- **LinearRegression** (`LinearRegressionAlgorithm.Fit`): OLS закрытой формулой
  (slope/intercept по ковариации). НЕ итеративный, без эпох/лосса. Есть
  `LoadParameters(slope, intercept)` — примитивный «импорт» состояния.
- **MultilayerPerceptron** (`MultilayerPerceptronAlgorithm.Fit`, строки 14–70):
  настоящий тренировочный цикл — 1 скрытый слой, sigmoid, SGD/backprop,
  параметры `hiddenSize/learningRate/epochs` (по умолч. 500 эпох), `Random(42)`.
  Forward → outErr/hidErr → обновление весов `_w1/_b1/_w2/_b2`. ЛОСС НЕ считается
  и НЕ возвращается; цикл крутит фиксированное число эпох без early-stop/метрики.
- **QLearning** (`QLearningAlgorithm.Train`, строки 11–57): табличный RL,
  правило Q(s,a) ← Q + lr·[reward + γ·maxQ' − Q], epsilon-greedy. `reward` —
  это ВХОД из функции среды `environment`, а не вычисленная метрика модели.
- **NaiveBayes** (`NaiveBayes.cs`): `score`/`bestScore` — это лог-вероятности при
  инференсе (выбор класса argmax), НЕ метрика качества обучения.
- **KMeans / Apriori / PCA**: классические unsupervised-примитивы (не читались
  построчно; по Glob/grep — без метрик точности/лосса и без обучаемой иерархии).

Сервисный слой (`*Service : BaseService`): оборачивает алгоритм, добавляет
`IdempotencyStore` и `Storage.Save(...)` со словарём. `MultilayerPerceptronService.Train`
сохраняет лишь флаг `["fitted"]=true` — НАСТОЯЩИЕ веса в storage не
сериализуются. Dispatcher возвращает сам сервис (`Ok(service)`), а не результат.

---

## 3. Метрики точности / лосса — статус

ОТСУТСТВУЮТ как вычисляемые/возвращаемые величины. Grep по
`accuracy|loss|reward|metric|Evaluate|score|Accuracy|Loss|Reward|Metric` по всей
области (без obj/) дал только: формулу/`reward` как ВХОД в QLearning и
`score/bestScore` (лог-prob инференса) в NaiveBayes. Нет `Evaluate*`, нет
accuracy/precision/recall/F1, нет вычисления и логирования training/val loss,
нет reward-margin, нет held-out оценки, нет ablation, нет numeric-metric отчёта.
То есть Layer 5 (`generated-code-review-SKILL.md`, «numeric metrics present AND
reproducible») этой областью НЕ закрывается — метрик нет в принципе.

---

## 4. Релевантность к ILearningDynamicGraph (DPO/обучение/граф/оркестрация/измерение/teacher-provenance)

- **Обучение (источник примитивов):** ВЫСОКАЯ как референс/строительный
  материал. MLP backprop+SGD и QLearning (TD-update) — единственные в проекте
  готовые итеративные тренировочные циклы «с нуля», на которые может опереться
  обучаемый узел графа. Но их нельзя взять «как есть» как обучаемый узел.
- **DPO:** релевантность НИЗКАЯ напрямую. Здесь нет ни пар chosen/rejected, ни
  reward-margin, ни DPO-петли. Это supervised/RL/unsupervised примитивы.
- **Граф / оркестрация:** релевантность НИЗКАЯ. `MachineLearningDispatcher` —
  плоский словарный роутер по строке-имени; это НЕ граф, НЕ flow, НЕ оркестратор
  в смысле иерархии (нет узлов/рёбер, fan-in/fan-out, async-исполнения).
- **Измерение:** ПРОБЕЛ (см. §3) — для ILearningDynamicGraph метрики/измерение
  придётся строить отдельно; переиспользовать здесь нечего.
- **Teacher-provenance / Legal Teacher Gate:** НЕ ПРИМЕНИМО к существующему коду
  (обучение на переданных массивах X/Y и среде, без внешнего teacher/дистилляции,
  без `teacher_output_used_for_training`). Provenance-поля тут отсутствуют —
  это чистая локальная вычислительная база, без юридического риска источника.

Главный вывод для плана: `14.MachineLearning` — это «движок арифметики»
(референс по backprop/SGD/TD), но он ИЗОЛИРОВАН от целевой архитектуры:
ноль ссылок на `ILearningMachine/ILogicalUnit/ILogicalFlow/ILogicalOrchestrator/
ILearningMachinePair`, ноль `AgentInfrastructure`, ноль checkpoint/manifest/
export-import (кроме тривиального `Save/Load` словаря), ноль метрик, ноль DPO.

---

## 5. Классификация по уровням (правило скила)

Применён `trainable-architecture-decision-SKILL.md` (раздел «Decision Procedure»,
«Verdicts») + Layer 5/5A `generated-code-review-SKILL.md`.

| Capability (класс/метод) | Заявленный паттерн | Фактический механизм | Уровень иерархии | Внеуровневая классификация | Verdict (по скилу) |
|---|---|---|---|---|---|
| `MultilayerPerceptronAlgorithm.Fit/Predict` | (не заявлен как иерархия) | SGD+backprop, веса `_w1.._b2` | НЕТ (не обёрнут) | внутренний обучаемый примитив / model component | `OK_INTERNAL_DETAIL` (как примитив). Для узла графа → `NEEDS_ILOGICALUNIT_WRAPPER` + lifecycle |
| `QLearningAlgorithm.Train/BestAction/QValue` | (не заявлен) | табличный RL, Q-table dict `(state,action)->double` | НЕТ | внутренний RL-примитив | `OK_INTERNAL_DETAIL`; как обучаемый узел → `NEEDS_ILOGICALUNIT_WRAPPER` |
| `LinearRegressionAlgorithm` | (не заявлен) | OLS closed-form | НЕТ | детерминистический matematический хелпер | `OK_INTERNAL_DETAIL` |
| `NaiveBayesAlgorithm` (`score/bestScore`) | (не заявлен) | argmax лог-prob классификатор | НЕТ | классификатор-хелпер (НЕ паттерн) | `OK_INTERNAL_DETAIL` (baseline; не семантический unit) |
| `KMeans / Apriori / PCA` | (не заявлен) | unsupervised примитивы | НЕТ | внутренние ML-хелперы | `OK_INTERNAL_DETAIL` |
| `MachineLearningDispatcher.Dispatch` | роутер | словарный lookup по строке + возврат сервиса | НЕТ (не `ILogicalOrchestrator/Flow`) | детерминистический lookup-роутер | `OK_INTERNAL_DETAIL`; если выдать за оркестрацию → `NEEDS_ILOGICALFLOW_OR_ORCHESTRATOR_WRAPPER` |
| `IModelStateStorage.Save/Load` | (не заявлен) | dict в память/строку | НЕТ | хелпер хранения (не checkpoint/manifest) | `OK_INTERNAL_DETAIL`; НЕ покрывает export/import/locator/fresh-load |

Обоснование (правило скила): единственные разрешённые обучаемые уровни —
`ILearningMachine / ILogicalOrchestrator / ILogicalFlow / ILogicalUnit /
ILearningMachinePair` (skill, строки 91–106). Всё остальное — «implementation
detail unless wrapped by one of those levels and has the required lifecycle
evidence». Ни один класс в `14.MachineLearning` не обёрнут в эти уровни и не
несёт lifecycle-доказательств (training data asset, learning signal, evaluation,
export/import, manifest, checkpoint, locator, fresh-load, continue-training,
ablation). Поэтому корректная классификация — внутренние детерминистические/
обучаемые ПРИМИТИВЫ, а не архитектурные паттерны. KNN-нота скила тут не
триггерится (KNN-ретривала в области нет).

Опасность для плана (Layer 5/5A): нельзя докладывать любой из этих классов как
готовый «trainable unit». MLP/QLearning без checkpoint-сериализации, без
export/import, без метрик и без обёртки = `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`
в лучшем случае, но НЕ `PASS_AS_TRAINED_FEATURE`. Для ILearningDynamicGraph
любой обучаемый узел на базе этих примитивов требует: класс-обёртку нужного
уровня, типизированный trained-state record, реальную сериализацию весов
(сейчас сохраняется только флаг `fitted`), численные метрики, и полный lifecycle.

---

## 6. Пробелы / что доисследовать

- **Не читались построчно** `KMeans.cs`, `Apriori.cs`, `PCA.cs`, `NaiveBayes.cs`
  (полностью), `InMemoryStorage.cs` — классификация по ним сделана по Glob+grep;
  при необходимости подтвердить отсутствие метрик/иерархии прямым чтением.
- **Сериализация весов:** подтвердить, что нигде (включая `InMemoryStorage`) MLP
  не сохраняет реальные `_w1.._b2` — сейчас в `Train` пишется только `["fitted"]`.
  Это критично для пункта «модель можно сохранить/импортировать/продолжить».
- **Потребители области:** не проверялось, КТО вызывает `14.MachineLearning`
  снаружи (Glob/Grep по решению на `MachineLearningDispatcher`/`MachineLearning.`
  в других проектах) — нужно для оценки, встроена ли область в маршрут
  ILearningDynamicGraph или полностью изолирована.
- **`ITrainingDataProvider`:** интерфейс объявлен, но его использование/реализации
  и формат датасетов не прослежены — доисследовать для DPO/обучения графа.
- **Связь с TensorCore (00):** правило проекта требует TensorCore как единственный
  тензорный примитив; MLP здесь использует «сырые» `double[,]` вручную — стоит
  отметить как расхождение/кандидат на переиспользование при проектировании узла.
