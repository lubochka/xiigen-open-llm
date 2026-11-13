# P50 approval-required boundary

P50 проверил оставшийся безопасный non-training зазор после принятого P49.
Вывод: безопасного non-training artifact work больше не осталось. Оставшиеся
пункты readiness contract требуют действий, которые текущий пакет прямо
запрещает: training, eval runner, checkpoint rewrite/mutation, fresh-load probe,
continue-training probe, host runtime или external AI/network.

Это не completion и не semantic-quality claim. Это граница, где дальнейшее
движение возможно только после свежего явного approval от Luba на конкретное
действие.

## Почему safe non-training gap отсутствует

P49 уже закрыл последний безопасный non-mutating шаг: связал target/rollback
semantics, source/license status, current checkpoint candidates и
OrchestrationReasoning resolver без мутации. По плану после этого остаются:

- source-disjoint eval и ablation report;
- checkpoint rewrite с manifest/index/locator;
- positive fresh-load reports;
- positive continue-training reports;
- API/firewall PASS или честный blocked reason после этих gates.

Каждый из этих шагов либо запускает eval/training/probe, либо меняет checkpoint
или locator/manifest, либо зависит от результатов таких запусков. Текущий P50
пакет запрещает все эти действия, поэтому P50 не фабрикует новый substitute
artifact.

## Что Luba должна явно одобрить до начала mutation/execution

Свежий approval должен быть текущей, не quoted, командой Luba. Он должен
назвать конкретное действие сейчас, а не ссылаться на старые планы, state или
transcript.

Минимальные поля перед execution:

1. `fresh_luba_approval_quote`: свежая цитата Luba, явно разрешающая named
   operation now.
2. `operation_type`: одно из `training`, `eval`, `checkpoint_mutation`,
   `checkpoint_rewrite`, `fresh_load_probe`, `continue_training_probe`,
   `rollback`.
3. `exact_machine_selection`: конкретный список из `PromptToTaskMachine`,
   `OrchestrationReasoningMachine`, `SkillSelectionMachine`.
4. `source_license_gate`: artifact path, row count, source family,
   license/approval source, common-training eligibility, user-local-only flag,
   quarantine reason, dataset checksum, heldout checksum.
5. `target_checkpoint_ref`: new staged checkpoint path, staged/new manifest
   path, locator ref to update only after future gates, checkpoint sha256,
   created-from dataset sha256. Silent overwrite of current repo checkpoint is
   not allowed.
6. `rollback_checkpoint_ref`: rollback checkpoint path, manifest path, locator
   ref, rollback checkpoint sha256, rollback activation steps or
   post-rollback verification.
7. `OrchestrationReasoning resolver choice`: choose locator-current rollback
   candidate, or explicitly justify alternate historical candidate.
8. `metrics_gate`: per-machine metrics, heldout pass rate by source family,
   regression check against previous checkpoint, explicit failure examples.
9. `ablation_gate`: exact prompt lookup, source-id/path/filename lookup,
   title/static route, nearest-only baseline, no-trained-state, and
   SkillSelection candidate-order permutation controls with approved pass
   thresholds.
10. `fresh_load_probe_result` and `continue_training_probe_result` are required
    before readiness claim/promotion; if the approval is for running those
    probes, it must name their scope and target checkpoint.

Until those fields exist and the future-approved gates pass, semantic quality
and model readiness remain unclaimed.

## Actions not run in P50

P50 did not run training, eval runner, checkpoint rewrite/mutation,
fresh-load/continue-training probes, host runtime, external AI/network,
dotnet tests/build, git stage/commit/push/switch, or production code/test edits.

