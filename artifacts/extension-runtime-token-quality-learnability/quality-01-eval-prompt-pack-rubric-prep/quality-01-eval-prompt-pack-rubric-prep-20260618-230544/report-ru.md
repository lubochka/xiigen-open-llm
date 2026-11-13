# QUALITY-01 eval prompt pack / rubric prep

## Итог

Создан prep-only пакет для будущей человеческой оценки качества full extensions. Пакет описывает, как позже проверять PromptToTask, OrchestrationReasoning, SkillSelection и финальную полезность, но сам не запускает eval/scoring/runtime, не вызывает внешние AI/model, не создаёт DPO/training/checkpoint mutations и не открывает claims.

Текущий TOKEN-03 pair completion остаётся главным ограничением: из 9 host/method pairs завершено 0, неполных 9. Поэтому этот пакет не зависит от token savings и не может доказывать качество. Он только готовит рубрику, prompt pack, held-out/source-disjoint требования, no-memorization/ablation требования и границу universal-vs-stack-specific.

## Прочитанные локальные источники

- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`: quality/DPO/source/universal-vs-stack-specific/no-hidden-deterministic-shortcut/human-readable sections.
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`.
- `artifacts/extension-runtime-token-quality-learnability/token-03-pair-completion-capture-or-defer/token-03-pair-completion-capture-or-defer-20260618-225554/report-ru.md`.
- `artifacts/extension-runtime-token-quality-learnability/token-03-pair-completion-capture-or-defer/token-03-pair-completion-capture-or-defer-20260618-225554/pair-completion-matrix.json`.
- `artifacts/extension-runtime-token-quality-learnability/token-03-pair-completion-capture-or-defer/token-03-pair-completion-capture-or-defer-20260618-225554/deferrals.json`.

Приватный внешний корпус не читался и не нужен для этой prep-фазы.

## Что создано

- `eval-prompt-pack.json`: человекочитаемый русский prompt pack для будущей оценки четырёх слоёв.
- `quality-rubric.json`: scoring rubric с критериями, шкалой, failure labels и claim locks.
- `held-out-and-ablation-requirements.json`: требования к source-disjoint/held-out/no-memorization/ablation и universal-vs-stack-specific boundary.
- `checks-report.json`: результаты локальных статических проверок этого пакета.

## Prompt pack

Будущая оценка должна использовать одинаковые scenario ids и pair ids для baseline/adaptive outputs, но только после того, как pairing integrity станет готовой. До этого runner обязан вернуть `PAIRING_INTEGRITY_NOT_READY`, а не score.

PromptToTask проверяет, превращает ли extension запрос пользователя в явную задачу без подмены смысла, без скрытого path/title lookup и без exact prompt replay.

OrchestrationReasoning проверяет, строится ли план действий как универсальная логика llm_mvp_core, а не как host-specific fixture или hardcoded сценарий.

SkillSelection проверяет, выбираются ли навыки и инструменты по смыслу задачи, ограничениям и evidence scope, а не по ключевым словам, имени файла, названию host или порядку кандидатов.

Final usefulness проверяет, получает ли пользователь полезный, честно ограниченный ответ с видимыми границами доказательств, no-overclaim wording и понятным next step.

## Positive / negative example shapes

Положительные формы:

- Задача названа своими словами, входные ограничения сохранены, неизвестные поля отмечены как missing/deferred.
- Оркестрация объясняет порядок действий и зависимость от evidence, но не выдаёт dashboard/static evidence за live host proof.
- Skill selection выбирает релевантные skills/tools и явно отклоняет нерелевантные.
- Финальный ответ помогает пользователю и честно говорит, что именно доказано, а что нет.

Отрицательные формы:

- Output повторяет prompt почти дословно или угадывает fixture по заголовку.
- Output использует raw external AI answer как positive/negative example, DPO target или source material.
- Output переносит private/project/stack-specific detail в universal/common rule.
- Output говорит о token savings, quality improvement, learnability или readiness без scored evidence и claim locks.

## Source-disjoint / held-out / no-memorization

Будущий held-out split должен исключать пересечение по prompt hash, correction text, source ref, file path, title, vendor trace, nearest-neighbour source и приватному проектному контексту. Если пересечение найдено, scorer обязан вернуть failure label и не считать улучшение.

Ablation должна доказать, что возможный выигрыш не объясняется deterministic helper, словарём, regex, BM25/KNN, fixture replay, exact prompt replay, transcript replay, source-title lookup или candidate-order bias. Если ablation объясняет выигрыш, semantic quality claim остаётся закрытым.

## Universal vs stack-specific

Universal/common material может описывать только общие orchestration/skill semantics с проверенным permissive source или XIIGen/human-authored rule. Host-specific, stack-specific, private project, path/title/branch/locator details остаются user-local/adaptive-only/evidence-only/quarantine и не переходят в common training или common quality claim.

## Как это позже проверит качество full extensions

Когда runtime/token prerequisites будут готовы, будущий eval-only runner должен взять baseline/adaptive outputs для одного и того же pair_id, scenario_id, host, method, evidence class, tokenizer/accounting/redaction policy и target locator mode. Затем он применит этот prompt pack и rubric layer-by-layer. Улучшение full extensions можно будет обсуждать только если:

1. pairing integrity прошла;
2. source-disjoint held-out split прошёл;
3. baseline/adaptive outputs scored по одной рубрике;
4. regression guard не нашёл просадки;
5. ablation/no-leakage/no-memorization не объяснили выигрыш shortcut-ом;
6. report сохраняет token, quality, learnability и readiness claims отдельно.

Это не полагается на deterministic shortcuts или exact prompt replay, потому что будущая проверка должна намеренно включать held-out prompts, source-disjoint split, perturbation prompts, negative controls и ablation rows.

## Следующий bounded work order

`QUALITY-02-SCORER-EVAL-ONLY-RUNNER-PREP-01`

Scope: подготовить статический scorer/eval-only runner contract и schema для будущего scoring, без actual eval, без model calls, без runtime host execution, без DPO/training/checkpoint mutations и без quality/token/readiness claims. Runner должен уметь сказать `PAIRING_INTEGRITY_NOT_READY` для текущих 0/9 complete pairs.

Allowed write scope предлагается ограничить новой timestamped-папкой под `artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/` и state JSON. Parent commit/push после review.

## What this proves

- Есть человекочитаемый русский prep пакет для будущей оценки качества.
- Рубрика и prompt pack отделены от token savings, runtime, training, DPO и readiness.
- 0/9 complete token pairs явно учтены, поэтому scoring сейчас не разрешён.

## What this does not prove

- Не доказывает token savings.
- Не доказывает quality non-regression или quality improvement.
- Не доказывает semantic readiness, learnability, DPO, checkpoint mutation, target promotion, locator update, full host readiness или product readiness.
- Не выполняет actual eval/scoring/runtime/training.

## Claim locks

Все claim locks остаются false. Пакет является static prep evidence only.
