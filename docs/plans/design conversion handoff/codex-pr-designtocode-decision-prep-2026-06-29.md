# PR_DESIGNTOCODE_DECISION_PREP - 2026-06-29

## Коротко по-русски

Это только decision-prep по PR для правильной ветки `DesignToCode`. Это не product completion, не PR readiness, не merge readiness и не разрешение на merge. `Luba performs the merge`.

Текущая безопасная картина такая:

- PR #6 открыт как draft из старой ветки `DesignToCode-clean`. Это старый branch model, а не исправленная рабочая ветка `DesignToCode`.
- `draft PR #7` открыт как draft из `codex/chat-gpt-5.5`. Это source-authority branch, которая должна быть read-only authority, а не рабочая ветка для Codex PR. Поэтому PR #7 был создан под неправильной branch model.
- Правильная рабочая ветка для этого chain: `DesignToCode`.
- Свежая read-only проверка `gh pr list --repo lubochka/llm_mvp_core --head DesignToCode --state all --json ...` вернула `[]`. Значит на момент этой проверки отдельный PR с head `DesignToCode` не найден.
- В этой задаче выполнено `no PR mutation`: PR не создавался, не закрывался, не переименовывался, не переводился из draft, reviewers не запрашивались, merge/push не выполнялись.

## Свежие read-only факты

Источники:

- `codex-fresh-human-full-chain-audit-2026-06-29.md`
- `codex-pr7-headref-authority-reconciliation-2026-06-29.md`
- `codex-pr-state-action-resolver-2026-06-28.md`
- fresh read-only `gh pr view` / `gh pr list`
- fresh read-only `git ls-remote`

Проверенные факты:

| Item | State |
| --- | --- |
| PR #6 | `OPEN`, draft `true`, base `main`, head `DesignToCode-clean`, head SHA `642b67c2bb2330b35a2b5a4e3972f7c6657abff3`, title `DesignConversion learning domain and governance continuation rules` |
| `draft PR #7` | `OPEN`, draft `true`, base `main`, head `codex/chat-gpt-5.5`, head SHA `c45d59e78da51b04eab370f720b9d0ec99ad9ccc`, title `Govern Class B continuation gate` |
| PR search for `DesignToCode` | `[]`; no PR with head `DesignToCode` found by fresh read-only check |
| remote `DesignToCode` | `8f20ceb8a9bfbadab2ad309226c47dd76e714548` |
| remote `DesignToCode-clean` | `642b67c2bb2330b35a2b5a4e3972f7c6657abff3` |
| remote `codex/chat-gpt-5.5` | `c45d59e78da51b04eab370f720b9d0ec99ad9ccc` |

Note: older report text recorded PR #7 mergeability differently at one point. The fresh `gh pr view 7` metadata now says `MERGEABLE`, but this is only GitHub metadata and does not prove merge readiness.

## Решение для Luba

### A) Оставить PR #7 как draft record и пока не создавать новый PR

Это safe default.

Что это значит:

- Ничего не меняем в GitHub.
- `draft PR #7` остается историческим draft record неправильной branch model.
- Новый PR из `DesignToCode` пока не создается.
- Риск: в GitHub будет продолжать висеть draft PR #7 из `codex/chat-gpt-5.5`, и без заметки/решения он может путать будущий handoff.

### B) Позже создать draft PR из `DesignToCode`, только после явного approval Luba

Это правильный путь, если нужен новый PR именно из рабочей ветки.

Предлагаемый exact title:

```text
DesignToCode handoff from corrected working branch
```

Предлагаемый exact body:

```markdown
## Scope

Draft PR from corrected work branch `DesignToCode`.

## Branch model

- `DesignToCode` is the working branch for this handoff.
- `DesignToCode-clean` is the old branch used by PR #6.
- `codex/chat-gpt-5.5` is source-authority/read-only context and was incorrectly used for draft PR #7.

## Boundaries

This PR is not product completion, not PR readiness, not merge readiness, and not a merge request. Luba performs the merge.

## Evidence boundary

Creation of this draft PR proves only PR existence/head branch metadata. It does not prove tests, builds, runtime/browser readiness, visual fidelity, clean repo state, or merge readiness.
```

Что это значит:

- Будет новый draft PR с head `DesignToCode`, если Luba отдельно разрешит PR creation.
- Перед созданием нужен fresh read-only check, что `DesignToCode` head SHA все еще ожидаемый или Luba принимает новый SHA.
- Это все равно не означает product completion, PR readiness или merge readiness.

### C) Закрыть или переименовать PR #7 только если Luba явно разрешит

Это PR mutation, поэтому сейчас не делается.

Возможные exact variants после approval:

- Close PR #7 with comment/body reason: `Closed because draft PR #7 was created from source-authority branch codex/chat-gpt-5.5 under the wrong branch model. Correct working branch is DesignToCode.`
- Retitle PR #7 to: `Historical draft: source-authority branch, not DesignToCode handoff`
- Leave PR #7 open but add/update body note only if Luba explicitly approves exact text.

Что это значит:

- Закрытие уменьшит путаницу, но меняет GitHub state.
- Retitle/body note сохранит record, но тоже меняет PR metadata.
- Без явного approval Luba безопасное действие остается `no PR mutation`.

## Последствия, если решения нет

Если Luba пока не выбирает вариант:

- safe default = A;
- PR #6 остается старым draft из `DesignToCode-clean`;
- `draft PR #7` остается draft из `codex/chat-gpt-5.5`;
- PR с head `DesignToCode` остается отсутствующим по свежей проверке;
- текущий packet не доказывает product completion, PR readiness или merge readiness;
- следующий безопасный шаг остается bounded decision follow-up, а не автоматическая PR mutation.

## Future bounded work order after Luba decision

```text
WORK ORDER: bounded-pr-designtocode-action-after-luba-decision

MODE:
- Execute only the exact Luba-selected option: A, B, or C.
- No tests/builds/browser/runtime/external AI/secrets/cleanup.
- Do not touch claude-* files.
- Do not merge. Luba performs the merge.

PRECHECK:
- Read this packet: codex-pr-designtocode-decision-prep-2026-06-29.md.
- Fresh read-only check:
  gh pr view 6 --repo lubochka/llm_mvp_core --json number,state,isDraft,headRefName,headRefOid,baseRefName,title,url,mergeable
  gh pr view 7 --repo lubochka/llm_mvp_core --json number,state,isDraft,headRefName,headRefOid,baseRefName,title,url,mergeable
  gh pr list --repo lubochka/llm_mvp_core --head DesignToCode --state all --json number,state,isDraft,headRefName,headRefOid,baseRefName,title,url
  git ls-remote origin refs/heads/DesignToCode refs/heads/DesignToCode-clean refs/heads/codex/chat-gpt-5.5

IF LUBA SELECTS A:
- Perform no PR mutation.
- Report current facts and explicitly say no PR was created or changed.

IF LUBA SELECTS B:
- Only if fresh check still shows no PR with head DesignToCode, create draft PR from head DesignToCode to base main with the exact approved title/body.
- If any PR with head DesignToCode exists, stop and report it instead of creating a duplicate.
- After creation, report URL, number, draft state, headRefName, headRefOid, baseRefName, and no merge performed.

IF LUBA SELECTS C:
- Mutate only PR #7 and only with the exact Luba-approved action/text: close, retitle, or body update.
- Before mutation, verify PR #7 is still OPEN draft with head codex/chat-gpt-5.5.
- If head/state changed, stop and ask for renewed owner confirmation.

BOUNDARIES:
- no product completion claim
- no PR readiness claim
- no merge readiness claim
- no merge
- no push unless Luba separately authorizes it
```

## Evidence and claim ledger

claim_scope:
- Created one doc-only PR decision-prep packet for the correct `DesignToCode` branch and mistaken `draft PR #7`.
- Explained PR #6, PR #7, absence of a `DesignToCode` head PR in fresh read-only check, Luba options, safe default, no-decision consequences, and future bounded work order.

evidence_scope:
- Read `codex-fresh-human-full-chain-audit-2026-06-29.md`.
- Read relevant PR report excerpts from `codex-pr7-headref-authority-reconciliation-2026-06-29.md`.
- Read relevant PR report excerpts from `codex-pr-state-action-resolver-2026-06-28.md`.
- Ran fresh read-only `gh pr view 6`.
- Ran fresh read-only `gh pr view 7`.
- Ran fresh read-only `gh pr list --head DesignToCode`.
- Ran fresh read-only `git ls-remote` for `DesignToCode`, `DesignToCode-clean`, and `codex/chat-gpt-5.5`.

count_claimed:
- 1 decision-prep document created.
- 3 local report inputs read by relevant excerpts.
- 4 fresh read-only metadata commands run.
- 0 PR mutations.
- 0 pushes.
- 0 merges.
- 0 tests/builds/browser/runtime/external AI/secrets/cleanup actions.

independent_evidence_count:
- 7 evidence groups: 3 report reads plus 4 fresh read-only metadata commands.

subagent_id_or_NONE:
- NONE. I did not spawn a nested agent. This document is the bounded sub-agent executor output for the current work order.

downgrade_needed:
- Yes for any claim beyond decision-prep and read-only PR/branch metadata.
- The evidence supports only: PR decision-prep was written, current read-only metadata shows PR #6 head `DesignToCode-clean`, `draft PR #7` head `codex/chat-gpt-5.5`, remote `DesignToCode` exists, and no PR with head `DesignToCode` was found by fresh `gh pr list`.

files_changed:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-pr-designtocode-decision-prep-2026-06-29.md`

verification:
- Read-only metadata commands succeeded.
- No tests/builds/browser/runtime checks were run.
- No GitHub mutation was performed.
- No file outside the write scope was intentionally written.

Class C yes-no:
- Yes: this packet handles the Class C doc-only PR decision-prep follow-up.
- Class C remaining from this exact work order: no.
