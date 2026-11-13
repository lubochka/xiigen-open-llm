# SOURCE_SELECTION_POLICY - 2026-06-29

## Человеческое резюме

Политика простая: source branch выбирается как latest updated remote branch ahead of main на момент проверки. Эта ветка является только источником правды для сравнения и mergeability-проверки. Она не является рабочей веткой Codex.

Рабочая ветка остается `DesignToCode`, и работа идет в отдельной папке или worktree. Source branch нельзя checkout-ить как рабочую ветку, нельзя туда коммитить, нельзя пушить от имени Codex, нельзя открывать или менять PR из нее, и нельзя делать no source branch mutation.

Если пока работа продолжается появилась более свежая remote branch ahead of main, выбор source branch надо повторить. После повторного выбора надо заново проверить mergeability между `DesignToCode` и новой latest updated source branch. Это только проверка совместимости. Источник не мутируется.

Финальное слияние выполняет Luba. Codex может только подготовить read-only evidence и bounded decision packet, если перед push, PR или mergeability proof выяснится, что source branch изменилась.

## Правило выбора source branch

1. Перед доказательством mergeability, перед push/PR-подготовкой или перед финальным handoff надо обновить read-only представление remote refs и определить latest updated remote branch ahead of main на момент проверки.
2. Выбранная ветка получает статус read-only authority branch.
3. `DesignToCode` исключается из роли source authority, потому что это рабочая ветка. Если она стала самой свежей updated branch из-за рабочего push, это не превращает ее в source branch.
4. Если latest updated source branch отличается от ранее записанной source branch, надо зафиксировать это как изменение authority и повторить mergeability check.
5. Повторная проверка не дает права мутировать source branch.

## Что запрещено

- Коммитить в source branch.
- Пушить source branch.
- Открывать или обновлять PR из source branch без отдельного явного решения Luba.
- Делать reset, rebase, merge, checkout-as-work-branch или cleanup source branch.
- Называть dry mergeability proof финальным merge readiness без актуального source recheck.

## Будущий bounded work order

Название: `bounded-source-branch-recheck-before-push-pr-mergeability-proof`

Режим:
- read-only git metadata only;
- no branch/ref mutation;
- no PR mutation;
- no tests/builds/browser/runtime/external AI/secrets/cleanup;
- no source branch mutation.

Вход:
- рабочая папка/worktree `DesignToCode`;
- текущий записанный source authority branch, если он есть;
- remote refs на момент проверки;
- `origin/main` как база для ahead-of-main selection.

Действия:
1. Выполнить read-only source selection: найти latest updated remote branch ahead of main на момент проверки.
2. Подтвердить, что выбранная source branch не является рабочей веткой `DesignToCode`.
3. Если source branch изменилась, записать изменение authority и заново выполнить только dry mergeability check против `DesignToCode`.
4. Если source branch не изменилась, записать narrow evidence: selection unchanged at check time.
5. Вернуть decision packet, если нужна owner decision перед PR, push или финальным mergeability handoff.

Выход:
- source branch name and SHA at check time;
- whether it changed from prior authority;
- dry mergeability result or explicit reason why it was not run;
- exact statement that Luba performs final merge;
- exact statement that no source branch mutation occurred.

## Machine literals

- SOURCE_SELECTION_POLICY
- SOURCE_BRANCH_IS_READONLY_AUTHORITY_NOT_WORK_BRANCH
- latest updated source branch
- Work happens on DesignToCode in a separate folder or worktree
- Only check mergeability into the latest updated source branch; Luba performs the merge
- no source branch mutation

## Claim ledger

claim_scope: bounded source-selection policy packet only.
evidence_scope: read three named handoff/governance reports and wrote this single markdown policy file.
count_claimed: 1 policy packet created.
independent_evidence_count: 4 evidence records: 3 required source documents read, 1 allowed policy file written.
subagent_id_or_NONE: delegated-bounded-subagent-current-session
downgrade_needed: yes; this packet defines policy only and does not prove current source selection, current mergeability, PR readiness, push readiness, final merge readiness, runtime/browser readiness, or product completion.
files_changed: `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-source-selection-policy-2026-06-29.md`
verification: literal readback required after write; no tests/builds/browser/runtime/git mutation verification claimed.
Class C yes-no: no.
