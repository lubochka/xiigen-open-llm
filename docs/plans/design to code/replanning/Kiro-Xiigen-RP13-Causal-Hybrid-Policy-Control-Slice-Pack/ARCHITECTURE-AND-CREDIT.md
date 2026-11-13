# RP13 — архитектура причинной диагностики и выбора гибридной политики

## Четыре защищённые роли (не сливать)

1. **`ILearningUnit`:** упрощённая обучаемая domain-aware матрица над типизированными входами/выходами; many→many (0..N допустимых candidate для 1..N связных inputs) с ясными `InputRef`/`OutputRef`/shape/provenance; не трактовать один nearest hit как многие outputs.
2. **`ILearningFlow`:** типизированная связь Units, корректирующая вход следующего Unit; отделить изменение edge/association state от изменения замороженного Unit.
3. **`ILearningOrchestrator`:** более глубокая асинхронная стратегия преобразования input→outputs; ветви parallel/fan-in, deadline/cancellation/partial/priority, error budget и actual await; изменение исполнения не автоматически «обучение».
4. **`ILearningDynamicGraph`:** отношения паттернов различного порядка, constraint-preserving chain/path/motif, versioned learned authority; граф не обязательно единственный последовательно вызываемый runtime. Отличать downstream Student weight update от graph edge preference.

Иерархия функциональных возможностей, **не жёсткая цепь для каждого запроса**. Истинные интерфейсы искать в K и конвертировать под существующие ports с минимальным adapter, не придумывать generic types.

## Разрешённая гибридная политика

Для запроса `x` и контекста `z=(subject, intent, language/stack/version, law, representation, uncertainty, memory/checkpoint, PC budget)` детерминированный admission создаёт `A_lawful(z)`: доступные источники, типы, схемы, права, license, domain-law, cost cap, output constraints. Только на `A_lawful` допустима *learned* policy `πθ(a | z, features)`. Decision action может быть: eligible domain branch, retrieval configuration/neighbor fusion, Unit/Flow correction, async/graph route, escalate/defer/abstain. Нельзя «выучить» нарушение hard law как компромисс с точностью.

Оценка выбранного `a`: `utility = law-valid end-task quality − λ_latency * full end-to-end time − λ_memory * peak RAM/VRAM − λ_budget * total local/teacher cost` (лишь ПОСЛЕ жестких veto); коэффициенты только объявленная hypothesis и отдельно версионированная политика, не hard-coded «×10»/произвольные скоры. Если текущий owner не поддерживает objective/state update, предоставить `OWNER_GAP` и под-задачу существующему владельцу, не писать второй optimizer.

## Что измеряет RP13; не подмена DPO

- `trace: raw input→StackDetector before destructive normalization→domain+intent→symbol/token schema→embedding/projection/codebook(optional)→kNN several neighbors→typed output/Flow→async graph choice→Student/renderer→ordinary output`, со стадией veto и alternative IDs.
- `credit`: причина неверного результата устанавливается только через контролируемый контраст, когда один компонент/координатная версия изменяются при прочих фиксированных; «последний вызов» или косинус ближайшего соседа не равны причине.
- `recipient`: ошибка domain classifier идёт к RP02, неверная association при правильном domain — RP03, неверный async route/higher-order edge — RP04, плохая генерация при хорошем маршруте — RP01 Student, плохая teach-task selection — RP07 Builder; новое собственное RP13 learned target — **только доказанный policy selector**, и только при независимом owner/grant. Могут быть `MULTIPLE_CONTRIBUTORS`/`UNKNOWN`.
- `DPO`: Student standard DPO имеет обе финальные Chinese-authored стороны для одного x, separate CODEX exact judge, frozen reference, valid chosen/rejected likelihood, true mutable LoRA/Student optimizer. Для selector route pair wise rank/bandit update может иметь **другую** явно прописанную функцию; запрещено называть это стандартным DPO без logπ/reference/β/gradient.
- `LoRA≠tokenizer≠projection≠domain vector`. LoRA low-rank ΔW влияет на указанный Student/Matrix recipient; математически заданная projection переносит между координатами и версионируется; необходим доказанный consumer, прежде чем утверждать «LoRA превратила ответ в токен».

## Непустой алгоритмический контракт

`record input_family, legal_sources, law_versions, graph_state, state_owner, mutable_vs_frozen, z_features, schema/coordinate_hash, candidates_before_veto, veto_reason, alternatives_after_veto, predicted utility/budget, actual chosen action, true downstream consumer result, observed quality/cost, counterfactual arm, credit/confidence, proposed owner/recipient, update_grant, checkpoint_parent/trial/cold`.

Целевая неверная связь: domain=SQL but class=Angular due to stripping stack markers; kNN gets multi-hits but only hit[0] delivered; correct Unit output discarded at typed Flow boundary; async picked illegal path despite graph near miss; Student after good route writes wrong SQL. RP13 не чинит все четыре автоматически: один минимальный effect per arm, верный owner.

## Вердикты

SOURCE_TRACED / MECHANISM_USED / STATE_MUTATED / SCIENTIFIC_EFFECT / ORDINARY_ADOPTED / INDEPENDENT_REVIEWED — **разные оси**. Для эффекта: `NOT_MEASURED | INVALID_EVIDENCE | INCONCLUSIVE | REGRESS | IMPROVE`; для policy: `REUSE_NO_CHANGE | IMPLEMENT_MINIMAL | NEEDS_SOURCE | NEEDS_GRANT | DEFER_OWNER_TRIGGER | NO_UPDATE`. Основание решения — measured usefulness, не тесты, количество ветвей графа, checkpoint SHA или кодовые строки.
