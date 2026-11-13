# CORRECTIVE_BRANCH_WORKFLOW_PACKET - 2026-06-29

claim_scope:
- Один корректирующий doc-only packet о ветках, PR #7 и следующем безопасном workflow после уточнения Luba.

evidence_scope:
- Прочитан `codex-pr7-headref-authority-reconciliation-2026-06-29.md`.
- Прочитаны выборочные строки из `codex-designconversion-handoff-index-2026-06-28.md` и `codex-designconversion-completion-handoff-2026-06-28.md` через `rg`.
- Выполнены read-only `git status --short --branch` и `git branch --show-current`.
- Я не выполнял `git fetch`, `gh pr view`, тесты, build, browser/runtime checks, cleanup, stash, restore, commit, push, merge, close PR, retarget PR или external AI.

count_claimed:
- 1 corrective branch workflow packet written.

independent_evidence_count:
- 4 evidence groups: existing PR #7 reconciliation report, handoff index targeted read, completion handoff targeted read, local read-only git branch/status metadata.

subagent_id_or_NONE:
- bounded-subagent-current-session

downgrade_needed:
- Да. Этот packet не доказывает product completion, PR readiness, merge readiness, clean repo, runtime/browser readiness, visual fidelity, external AI judge proof или свежий GitHub PR state. Evidence supports only: текущий doc-only corrective workflow packet создан на основе прочитанных handoff/reconciliation reports и текущей read-only local branch/status context.

## Коротко по-русски: что пошло не так

Luba уточнила правило: самая свежая обновленная ветка ahead of `main` является источником authority и merge target, а не рабочей веткой для Codex.

Ошибка была в том, что source authority branch была фактически обработана как work branch. В результате PR #7 был создан из source-authority branch `codex/chat-gpt-5.5`. По уточненному правилу Luba это неправильно: source authority branch нельзя использовать как место, где Codex делает рабочие изменения, коммитит, пушит или открывает PR без отдельного точного разрешения Luba на конкретную ветку и действие.

Ключевая формула:

`SOURCE_BRANCH_IS_READONLY_AUTHORITY_NOT_WORK_BRANCH`

## Исправленный workflow

1. Самая свежая обновленная ветка, которая ahead of `main`, считается read-only source/merge target.
2. Для работы используется отдельная папка или отдельный git worktree.
3. Рабочая ветка называется `DesignToCode`.
4. Codex применяет свою работу только там: `Work happens on DesignToCode in a separate folder or worktree`.
5. После работы можно делать только read-only/local dry-run mergeability check в сторону source branch.
6. `Only check mergeability into the latest updated source branch; Luba performs the merge`.
7. Нельзя делать commit/push/PR из source authority branch, закрывать PR, retarget PR или менять branch refs, пока Luba явно не разрешит exact branch/action.

Практический смысл: source branch - это ориентир и цель сверки, а не стол для работы. Рабочий стол Codex - отдельная папка/worktree на `DesignToCode`.

## PR #7 status

По прочитанному reconciliation report:

- PR #7: `https://github.com/lubochka/llm_mvp_core/pull/7`
- status in report: open draft.
- head branch in report: `codex/chat-gpt-5.5`.
- report says PR #7 headRefOid and `origin/codex/chat-gpt-5.5` matched at `0c71179858368fcd3bbee122b4c881fcb1048f47`.

Corrective classification:

- PR #7 был создан как draft из source authority branch.
- По уточненному правилу Luba это wrong workflow.
- Но этот packet не закрывает, не retarget-ит и не мутирует PR #7.
- PR #7 нельзя закрывать или менять без отдельного явного решения Luba.

Decision options для Luba, человеческим языком:

1. Оставить PR #7 draft/open как historical governance artifact, ничего не менять сейчас.
2. Закрыть PR #7 как созданный из неправильной source-authority branch, но только после явного разрешения Luba: exact PR/action.
3. Создать новый корректный workflow из отдельного worktree на `DesignToCode`, а PR #7 оставить до отдельного решения.
4. Если Luba хочет сохранить часть governance changes, сначала подготовить перенос/сравнение в отдельном worktree, без push/PR из source authority branch.

## Readiness items are tasks, not stop blockers

`Proof readiness items are tasks, not stop blockers`

Runtime/browser/visual/clean/merge readiness нельзя больше подавать как туманный финальный стоп. Это high-priority task queues, которые требуют подготовки:

- runtime readiness: отдельный scenario packet с командами, ожидаемым output, evidence filenames и границами claims.
- browser readiness: сценарий с login/session preflight, no secrets captured, exact clicks/typing/expected visible state.
- visual readiness: policy prep для локальной проверки или external AI judge; если нужны secrets/provider/account, это отдельное решение Luba.
- clean repo readiness: отдельный residue/dirty-worktree packet с owner decisions; без cleanup/stash/restore по умолчанию.
- merge readiness: отдельная локальная dry-run mergeability task; не настоящий merge и не push.

Машинные статусы и labels не должны идти первыми. Сначала нужен human-language blocker report:

`Human-language blocker reports before machine labels`

То есть сначала простыми словами: что мешает, почему это важно, что можно сделать безопасно, что требует решения Luba. Только потом можно добавлять machine label.

## Next safe bounded work orders after governance repair

1. separate-worktree realignment plan

   Подготовить план отдельной папки/worktree: где находится read-only source branch, где будет `DesignToCode`, какие команды будут read-only, какие команды запрещены, какие evidence files появятся. Без создания веток, checkout, commit, push или PR в этом packet.

2. PR #7 decision-prep

   Подготовить отдельный human-language decision packet по PR #7: оставить draft, закрыть, заменить новым workflow, или перенести governance content. В packet должны быть последствия каждого варианта и минимальная exact approval phrase, если Luba выберет действие.

3. human-language blocker glossary

   Сделать словарь blocker/readiness terms простым русским языком: runtime proof, browser proof, visual proof, clean repo, mergeability, source branch, work branch, dry-run merge. Цель - чтобы каждый blocker стал понятной task queue, а не стоп-табличкой.

4. proof-task scenario queue

   Составить очередь proof tasks: runtime scenario, browser scenario, visual scenario, clean repo scenario, mergeability dry-run scenario. Для каждого: exact prep, allowed tools, forbidden actions, expected evidence, no secrets captured, downgrade rules.

## Boundary

Этот packet не является product completion. Он не доказывает merge readiness, PR readiness, clean repo, runtime/browser proof, visual fidelity или external AI proof.

Он только фиксирует corrected workflow после уточнения Luba:

- `CORRECTIVE_BRANCH_WORKFLOW_PACKET`
- `SOURCE_BRANCH_IS_READONLY_AUTHORITY_NOT_WORK_BRANCH`
- `Work happens on DesignToCode in a separate folder or worktree`
- `Only check mergeability into the latest updated source branch; Luba performs the merge`
- `Proof readiness items are tasks, not stop blockers`
- `Human-language blocker reports before machine labels`

## Report back fields

claim_scope:
- Created one doc-only corrective branch workflow packet.

evidence_scope:
- Existing PR #7 reconciliation report, targeted handoff/index reads, local read-only git branch/status context.

count_claimed:
- 1 file written.

independent_evidence_count:
- 4 evidence groups.

subagent_id_or_NONE:
- bounded-subagent-current-session.

downgrade_needed:
- Yes. No live PR mutation or fresh GitHub verification was performed by this packet.

files_changed:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-corrective-branch-workflow-packet-2026-06-29.md`

verification:
- File creation/readback should verify required literals and report fields only. No tests/builds/browser/runtime/git mutation should be inferred.

next_safe_followups:
- separate-worktree realignment plan.
- PR #7 decision-prep.
- human-language blocker glossary.
- proof-task scenario queue.
