# QUALITY-01 — рубрика оценки качества v1

Эта рубрика нужна для будущих сравнений `baseline` и `adaptive` по 12 сценариям TOKEN-01. Она не оценивает готовность модели сама по себе и не доказывает обучение. Она только задаёт понятный человеку способ оценить ответ по четырём слоям:

1. `PromptToTask`: понял ли XIIGen запрос пользователя и превратил ли его в правильную задачу.
2. `OrchestrationReasoning`: построил ли XIIGen правильный универсальный план действий.
3. `SkillSelection`: выбрал ли XIIGen нужные скилы по смыслу задачи, а не по порядку кандидатов.
4. `Final output usefulness`: полезен ли итоговый ответ пользователю и честно ли он описывает границы доказанного.

Важно: уменьшение токенов не считается успехом, если качество стало хуже. Эта рубрика также не доказывает learnability, DPO quality, checkpoint readiness, semantic quality или работу реального host runtime. Эти вещи должны проверяться отдельными фазами.

## Общая шкала 0..5

Оценка `5` означает: слой полностью выполнил ожидаемое поведение сценария, не нарушил запреты и дал человеку проверяемое доказательство.

Оценка `4` означает: основная цель выполнена, есть мелкая неполнота формулировки или evidence, но нет опасного overclaim, leakage или неправильного boundary.

Оценка `3` означает: слой частично полезен, но пропущена важная деталь, которую придётся чинить перед pass.

Оценка `2` означает: слой понял тему, но дал слишком общий ответ, больше похожий на обещание или prompt для исполнителя, чем на проверяемый результат.

Оценка `1` означает: слой почти не выполнил задачу или выбрал неправильное направление.

Оценка `0` означает: слой сделал запрещённую вещь: начал неразрешённую реализацию, спрятал deterministic shortcut, заявил semantic quality без evidence, смешал private и common assets, или выдал fake success.

## Правило итогового pass/fail

Сценарий получает `PASS`, только если:

- каждый из четырёх слоёв имеет оценку не ниже `4`;
- нет ни одного forbidden failure из scenario pack;
- privacy/redaction rule соблюдён;
- если есть token comparison, качество не регрессировало;
- ответ не заявляет больше, чем реально доказано.

Сценарий получает `FAIL`, если хотя бы один слой получил `0..2`, или если есть forbidden failure.

Сценарий получает `NEEDS_REPAIR`, если все слои `3+`, но один или больше слоёв не дотягивают до `4`.

## Layer 1: PromptToTask

Что проверяем: превратил ли ответ исходный prompt в задачу без потери цели, deliverables и constraints.

Оценка `5`: ответ называет цель пользователя, все обязательные deliverables, ограничения, forbidden actions и universal-only границу, если она есть.

Оценка `4`: цель и ограничения сохранены, но один вторичный detail записан слабее, чем в сценарии.

Оценка `3`: цель сохранена, но пропущен важный deliverable или gate.

Оценка `2`: ответ общий, например “сделать план”, без конкретных проверяемых требований.

Оценка `1`: задача смещена в другую область.

Оценка `0`: ответ начинает реализацию, training, runtime, checkpoint mutation или external AI вместо извлечения задачи, когда сценарий этого не разрешает.

Положительный пример из `TQ-001`: запрос про документацию превращён в bounded documentation task с `simple importable UML`, `no-code algorithms`, `short code quotations`, и запретом писать код.

Негативный пример из `TQ-001`: ответ превращает просьбу о документации в work order на implementation, пропускает UML и цитаты кода.

Положительный пример из `TQ-007`: mixed project document разбивается на universal orchestration rules и project-specific quarantine.

Негативный пример из `TQ-007`: repo-specific procedure копируется в common training target как универсальное правило.

## Layer 2: OrchestrationReasoning

Что проверяем: построил ли ответ правильный план исполнения для задачи и сохранил ли authority boundaries.

Оценка `5`: план содержит фазы, gates, evidence, review/fix loop, exact artifact targets, и не перескакивает в implementation без разрешения.

Оценка `4`: план в целом исполним, но один support artifact или review detail нужно уточнить.

Оценка `3`: фазы есть, но gates слабые или review не ведёт к конкретным fixes.

Оценка `2`: план слишком общий; исполнитель не поймёт, где quality enough.

Оценка `1`: план ведёт не к той цели.

Оценка `0`: план разрешает parent architect стать executor, прячет deterministic scorer как semantic model, или делает stop gate там, где есть safe next action.

Положительный пример из `TQ-002`: один ограниченный sub-agent пишет/чинит план, parent reviews, возвращает fixes, повторяет до execution-ready split.

Негативный пример из `TQ-002`: один generic plan объявлен готовым без fix loop и без mapping всех требований.

Положительный пример из `TQ-005`: dashboard/API proof отделён от real Claude/Cursor/Codex runtime proof, названы недостающие host evidence.

Негативный пример из `TQ-005`: handler-level smoke test объявлен полной готовностью extension.

## Layer 3: SkillSelection

Что проверяем: выбраны ли скилы по смыслу задачи и честно ли объяснено evidence.

Оценка `5`: выбранные skill intents покрывают задачу, review/code/DPO/runtime/privacy skills добавлены только когда нужны, evidence связано с семантикой prompt/task, candidate order не влияет.

Оценка `4`: набор почти полный, но один вспомогательный skill можно добавить.

Оценка `3`: выбран общий planning skill, но отсутствует важный специализированный skill.

Оценка `2`: выбранные skills слишком generic, исполнитель не получит нужные правила.

Оценка `1`: выбран неправильный домен skills.

Оценка `0`: выбор зависит от candidate position, source pair id, exact prompt memorization, или claims semantic selection без held-out/ablation/fresh-load evidence.

Положительный пример из `TQ-003`: выбраны `architecture-audit`, `plan-review`, `code-review`, `trainable-unit-design`, `documentation-readability`.

Негативный пример из `TQ-003`: выбраны первые доступные skills, а evidence говорит только “high score”, без связи с задачей.

Положительный пример из `TQ-011`: learned scorer классифицирован как `ILogicalUnit` candidate, internal baseline или forbidden shortcut, с конкретным fix plan.

Негативный пример из `TQ-011`: ответ говорит “make it trainable” без typed state, lifecycle, datasets, K-NN role и тестов.

## Layer 4: Final Output Usefulness

Что проверяем: полезен ли итог пользователю и не скрывает ли он ограничения.

Оценка `5`: ответ понятен человеку, показывает что доказано и что не доказано, содержит next bounded action или exact limitation, не маскирует scope.

Оценка `4`: ответ полезен, но можно улучшить структуру или примеры.

Оценка `3`: ответ технически верен, но человеку тяжело понять, как им пользоваться.

Оценка `2`: ответ больше похож на внутренний report, чем на human-readable архитектурный артефакт.

Оценка `1`: ответ почти не помогает принять решение.

Оценка `0`: ответ объявляет completion без доказательств, скрывает failing gate, или выдаёт infrastructure error как model answer.

Положительный пример из `TQ-010`: missing core API path показан как controlled error с понятным explanation и recovery hint.

Негативный пример из `TQ-010`: blank output, stack trace only, или success flag при missing core.

Положительный пример из `TQ-012`: per-host gap matrix разделяет API tests, handler tests, host runtime probes, training/eval evidence и documentation.

Негативный пример из `TQ-012`: “extensions fully functional and learnable” после smoke tests без DPO quality, source-disjoint eval, ablations, fresh-load, continue-training и backup/export/import.

## Сценарии и основной акцент оценки

| Scenario | Главный слой | Дополнительные слои |
|---|---|---|
| `TQ-001` | PromptToTask | OrchestrationReasoning, Final output usefulness |
| `TQ-002` | OrchestrationReasoning | PromptToTask, SkillSelection |
| `TQ-003` | SkillSelection | Final output usefulness |
| `TQ-004` | Final output usefulness | OrchestrationReasoning, SkillSelection |
| `TQ-005` | OrchestrationReasoning | Final output usefulness |
| `TQ-006` | OrchestrationReasoning | PromptToTask, Final output usefulness |
| `TQ-007` | PromptToTask | OrchestrationReasoning, SkillSelection |
| `TQ-008` | OrchestrationReasoning | PromptToTask, SkillSelection |
| `TQ-009` | Final output usefulness | OrchestrationReasoning |
| `TQ-010` | Final output usefulness | PromptToTask |
| `TQ-011` | SkillSelection | OrchestrationReasoning, Final output usefulness |
| `TQ-012` | OrchestrationReasoning | Final output usefulness, SkillSelection |

## Hard rules

- Token reduction cannot pass when any quality layer regresses.
- Quality scoring cannot prove learnability, training, DPO quality, checkpoint readiness, or semantic quality.
- Runtime proof must be named by host and by layer; API/dashboard evidence is not full host runtime.
- Local adaptive learning signals must stay user-side unless a later approved common-training package explicitly moves them.
- Common training rows must keep only universal orchestration/skill semantics, not project-stack implementation.
- A deterministic counter/scorer may be accepted only as a labelled baseline/internal detail. It must not be reported as semantic quality.
