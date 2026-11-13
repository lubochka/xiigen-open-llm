# P7G: формальное подтверждение PromptToTask eval/ablation

## Вердикт
PASS_CANDIDATE_READY_FOR_PARENT_REVIEW: P7F candidate checkpoint свежезагружен через проектный код и прошел компонентный PromptToTask eval/ablation контур. Это означает только, что PromptToTask component evidence готово к родительскому review. Это не full API semantic quality, не readiness расширения и не final model readiness.

## Что проверялось
- Fresh-load через PromptToTaskMachine.LoadCheckpointAsync по P7F checkpoint/manifest.
- 4 source-disjoint reserved positive rows: 2 eval candidate и 2 holdout candidate.
- 32 negative controls: exact prompt lookup, filename/path/title authority, source-family id authority, nearest-only/retrieval-only, row-id marker, source/provenance signal, learned pattern id authority и keyword-only.
- Отсутствие training contamination: P7 reserved/eval/holdout rows не находятся в accepted training refs.
- Отсутствие точной копии failed P7E signal combinations как learned authority.

## Результаты
- Reserved positives: 4 из 4 вернули learned PromptToTask contract.
- Shortcut leaks: 0 из 32 controls.
- Training run: не запускался.
- Checkpoint rewrite: не выполнялся.
- Final/common/P6/P7D/P7F checkpoint overwrite: не выполнялся.

## Примеры положительных строк
- P5H-DPO-PTT-002 нашел p7f-pattern-task-contract-planning по содержательному сигналу intent, а не по row id или source ref.
- P5H-DPO-PTT-005 нашел p7f-pattern-missing-context-template по содержательному сигналу missing_inputs, а не по template filename или source family.

## Примеры отрицательных контролей
- ilename_path_title: передаются только маркеры имени/пути/title; learned output не должен появиться.
- source_provenance_signal: передаются только названия provenance полей; learned output не должен появиться.
- pattern_id_authority: передаются только id learned patterns; learned output не должен появиться.

## Чего это не доказывает
P7G не доказывает полную semantic quality всего API или расширения. Он не проверяет host runtime, SkillSelection, dashboard/UI, continue-training и не продвигает final/common model asset. Это компонентное evidence для PromptToTask и оно требует parent review.