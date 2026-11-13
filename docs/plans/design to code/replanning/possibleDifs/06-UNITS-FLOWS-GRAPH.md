# DPO → обучаемый Unit / Flow / Orchestrator / Graph: что доказано и что не связано
## Исходная цель из первоисточника
[original task.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/design%20to%20code%20session/original%20task.md#L7-L22) задаёт:
- **ILearningUnit** — типизированный many→many domain/vector/input/output transformer; результат уровня входа должен соответствовать реальному запросу, а не только соседней записи.
- **ILearningFlow** — отношения и коррекция input→output между Unit, включая связь источника исправления.
- **ILearningOrchestrator** — асинхронно обрабатывает один input через несколько lawful комбинаций; считает стоимость всех веток.
- **ILearningDynamicGraph** — higher-order relations и композиции паттернов разных порядков; строит DPO по обнаруженным недостаткам.
- Судья внешней модели может служить ориентиром, но происхождение обучающих текстов должно оставаться разрешённым. Эти роли — не требование писать четыре независимых DPO trainers.

## Текущая установленная цепь RP01
`SimulationDpoInputRow` → `DpoPair` → `LogicalDpoPreferenceExample` → `LogicalDpoPreparedBatchMaterializer` → `DpoVectorBatch` → **один** `DpoTrainer.StepVector` → **q_proj/v_proj LoRA локального LlmModel** → private checkpoint → `CodeDocumentationStudentGeneration` / `TextGenerator`. [CodeDocumentationSimulationDpoExperiment.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L103-L139); [CodeDocumentationSimulationDpoExperiment.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L175-L193); [CodeDocumentationStudentGeneration.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs#L60-L85).

Данное наблюдение показывает **точный локальный Student-trainer**. Не доказано в этом flow: `GraphDpoPair.Level/UnitId` → точные обучаемые параметры каждого ILogicalUnit, update Flow's relation or edge weights, graph topology authority, learned orchestration, типизированные many→many outputs или kNN/arbiter correction. Всё это потенциально реализовано **где-либо в других владельцах**, но само существование класса там не означает RP01 learning benefit. Проверять по CODE path и receipt только нужных consumers.

## Дефекты интеграции, которые нужно фальсифицировать
| Граница | Возможный information/authority loss | Проба с frozen control |
| --- | --- | --- |
| DPO pair → unit identity | Domain/Level/UnitId отброшен, вся ошибка отправляется generic q/v Student | Сопоставить final pair manifest и installed trainable keys; per-unit recipient mapping |
| Unit input(s) → typed outputs | Copy `bestExample.Output` вместо запроса-обусловленного результата; ошибочно копируются source IDs/spans | два источника с общими tokens, разные OutputRefs; check typed associations |
| Unit → Flow | Неправильная коррекция следующего input, path order/causal relation сортировкой IDs | фиксировать Unit, менять только learned Flow, inspect next input/output |
| Flow → Orchestrator | Deterministic default route выигрывает без learned branch scoring; async fan-out не выполнен | parallel branch trace+selected lawful result, reward/cost attribution |
| Orchestrator → Graph | статическая topology/replayed route вместо newly learned higher-order motif | unseen composite family vs replayed ID; negative cyclical edges |
| Learned candidate → arbiter | KNN/cost/style score overridden hard law или bypass arbiter после fallback | rejected candidate никогда не проходит как accepted |
| Checkpoint → ordinary consumer | actual retained learner не chosen by serving configuration | pinned SHA and newly generated user answer, not only offline receipt |

**Сдерживание ложного вывода:** [FINDINGS.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/design%20to%20code%20session/replanning/results/R04/FINDINGS.md#L95-L108) подтверждает, что generic interfaces/OutputRefs и wrapped `LogicalUnitDynamicGraphNode` уже существуют: материализация `AsQueryable` и хранение refs не означают, что функциональность отсутствует всюду. В частности, static `DeterministicDynamicGraphNodeOutputProjector` и ID/hash — допустимая инженерная часть, но не доказательство обучения связей.

## Проверка детерминистических скрытых замен
Вывести trace attribution: `raw input → compiled source → eligible memory → candidate list → learned rank/weights/version → arbiter verdict → selected Unit/Flow/path → final output`. Для каждой решающей операции указать `deterministic guard`, `trained parameter`, `random sampler`, `fallback`; сравнить identical state no-update vs updated model с **одинаковыми guardrails**. Если улучшение существует только при inclusion deterministic law guard, писать `DETERMINISTIC_QUALITY_GAIN`, а не `LEARNED_GAIN`. RP01 узкий hybrid ENRU guard дал wrong accepted 4→0, correct2→3, abstain0→3: это полезно, но coverage упал6→3 и NN не училась; [RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md#L11-L16).

## Формула приёмки learned роли
У неё должен быть state owner, lawful teaching signal, baseline+changed state, durable saved/readback identity, **доказанный effect на ordinary consumer**, heldout novel semantic family и отрицательный контроль deterministic-only. Если нет хотя бы одного, писать `DESIGN/IMPLEMENTATION_GAP`, а не `LEARNED_GRAPH_PASS`.
