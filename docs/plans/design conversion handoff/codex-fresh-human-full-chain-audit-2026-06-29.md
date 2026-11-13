# Fresh Human Full-Chain Audit - 2026-06-29

## Простое русское резюме

Это свежий человеческий аудит после исправленного выравнивания worktree `DesignToCode`.

Главный вывод: правильная рабочая папка сейчас `C:\Projects\llm_mvp_core_DesignToCode_worktree`, рабочая ветка там `DesignToCode`, и текущий локальный `HEAD` совпадает с `origin/DesignToCode` на `8f20ceb8a9bfbadab2ad309226c47dd76e714548`.

Source branch не должна быть рабочей веткой Codex. По последнему отчету о применении governance к `DesignToCode`, source authority остается `origin/codex/chat-gpt-5.5` на `c45d59e78da51b04eab370f720b9d0ec99ad9ccc`. Текущая локальная сортировка ref по дате показывает, что после push рабочая ветка `origin/DesignToCode` стала свежее, чем `origin/codex/chat-gpt-5.5`. Поэтому безопасная трактовка такая: если выбирать latest updated source branch, нужно исключить рабочую ветку `DesignToCode`, иначе source и work branch схлопнутся в одну ветку и сломают правило Luba.

Dry mergeability был обновлен только как dry check: `git merge-tree --write-tree --messages --name-only refs/heads/DesignToCode refs/remotes/origin/codex/chat-gpt-5.5` завершился с code 0, вернул synthetic tree `7b9ff7367e13407c2e9d435e4b1f74833d362208` и сообщил только `Auto-merging AGENTS.md`; конфликтов в выводе не было. Настоящий merge не выполнялся.

`origin/DesignToCode` существует и указывает на `8f20ceb8a9bfbadab2ad309226c47dd76e714548`. Это доказывает только pushed ref, а не PR, merge readiness, runtime proof или product completion.

PR ситуация:

- PR #6 открыт draft, head `DesignToCode-clean`, head SHA `642b67c2bb2330b35a2b5a4e3972f7c6657abff3`. Это старый branch workflow, не исправленный `DesignToCode`.
- PR #7 открыт draft, head `codex/chat-gpt-5.5`, head SHA `c45d59e78da51b04eab370f720b9d0ec99ad9ccc`. Это source-authority branch mistake: PR создан из ветки, которая должна быть read-only authority, а не рабочей веткой.
- По `gh pr list --repo lubochka/llm_mvp_core --head DesignToCode --state all --json ...` PR с head `DesignToCode` не найден: `[]`.

В оригинальной папке `C:\Projects\llm_mvp_core` локально изменен `AGENTS.md` (`M AGENTS.md`). Это local residue / owner decision item. Это не product blocker и не причина утверждать, что `DesignToCode` worktree грязный.

Proof-task очередь не завершена продуктово. Это очередь задач: mergeability, clean repo/evidence, runtime/browser, visual fidelity, package/trace/corpora/fixture. Эти пункты надо вести как отдельные bounded work orders, а не как финальный стоп.

## Что уже сделано

A) Уже обработано:

- Правильная рабочая папка проверена: `C:\Projects\llm_mvp_core_DesignToCode_worktree`.
- Рабочая ветка проверена: `DesignToCode`.
- `origin/DesignToCode` проверен: `8f20ceb8a9bfbadab2ad309226c47dd76e714548`.
- Dry mergeability в сторону read-only source authority обновлен и не показал конфликтов.
- PR #6 и PR #7 проверены read-only через GitHub connector.
- Отдельный PR с head `DesignToCode` проверен через `gh pr list`; сейчас не найден.
- Существующие same-day codex reports до этого аудита уже были включены в index и completion handoff.

B) Нужно решение Luba, secret, policy или сценарий:

- Что делать с PR #7: оставить историческим draft, закрыть, заменить корректным PR, или переносить governance content.
- Создавать ли новый PR из `DesignToCode`; это PR mutation и требует отдельного явного разрешения.
- Как формально выбирать latest source branch после того, как рабочая ветка `DesignToCode` сама стала самой свежей updated branch ahead of main.
- Любой настоящий merge, push, branch/ref mutation или PR mutation.
- External visual judge: provider/account/model/endpoint/secret env var name/data-sharing/rubric/threshold.
- Runtime/browser execution: точный сценарий, команды, login/session preflight, clicks/typing/expected visible state, evidence paths.
- Cleanup/stash/restore/archive/delete/commit для local residue, traces, browser data, generated evidence, packages, corpora, fixtures.

C) Безопасные следующие задачи через bounded sub-agent:

1. `bounded-pr-designtocode-decision-prep-worker`
   - Вход: текущие PR #6/#7 facts, `origin/DesignToCode` SHA, запрет PR mutation.
   - Действие: подготовить варианты для Luba: оставить PR #6/#7, закрыть PR #7, открыть новый PR из `DesignToCode`, перенести/не переносить governance content.
   - Выход: human-language decision packet, no PR mutation.

2. `bounded-source-selection-policy-worker`
   - Вход: правило Luba, текущий ref sort, `origin/DesignToCode` и `origin/codex/chat-gpt-5.5`.
   - Действие: описать selector rule, который исключает work branch из source-authority selection.
   - Выход: policy packet; no branch/ref mutation.

3. `bounded-clean-repo-evidence-residue-classifier`
   - Вход: repo roots, read-only status boundary, category vocabulary.
   - Действие: классифицировать dirty/untracked/local residue as evidence/residue/quarantine/defer/decision item.
   - Выход: classification manifest; no cleanup/delete/move/stash/restore.

4. `bounded-runtime-browser-scenario-prep-worker`
   - Вход: exact runtime/browser goal, commands, expected visible states, evidence paths.
   - Действие: написать scenario prep с exact clicks/typing/expected visible state.
   - Выход: scenario prep packet; no runtime/browser execution.

5. `bounded-external-visual-judge-owner-decision-collector`
   - Вход: artifact set and visual-proof need.
   - Действие: собрать missing non-secret provider/rubric/threshold/data-sharing decisions.
   - Выход: decision packet; no external AI call and no secret capture.

6. `bounded-package-trace-corpora-fixture-selector-worker`
   - Вход: package/trace/corpora/fixture families and allowed inspection boundary.
   - Действие: разделить package, trace JSON, wiki corpora, Figma fixtures на defer/archive/waive/provenance/sensitivity/preflight queues.
   - Выход: selector packet; no build, publish, replay, training, fixture mutation, cleanup, or secret access.

## Evidence

- `git -C C:\Projects\llm_mvp_core_DesignToCode_worktree status --short --branch --untracked-files=no` -> `## DesignToCode`.
- `git -C C:\Projects\llm_mvp_core_DesignToCode_worktree rev-parse HEAD` -> `8f20ceb8a9bfbadab2ad309226c47dd76e714548`.
- `git -C C:\Projects\llm_mvp_core_DesignToCode_worktree rev-parse --verify origin/DesignToCode` -> `8f20ceb8a9bfbadab2ad309226c47dd76e714548`.
- `git for-each-ref --sort=-committerdate ...` showed `DesignToCode` and `origin/DesignToCode` at `2026-06-29T07:51:23+03:00`, then `origin/codex/chat-gpt-5.5` at `2026-06-29T07:47:15+03:00`.
- `git rev-list --left-right --count origin/main...origin/codex/chat-gpt-5.5` -> `4 326`.
- `git rev-list --left-right --count origin/main...origin/DesignToCode` -> `4 299`.
- `git rev-list --left-right --count origin/codex/chat-gpt-5.5...DesignToCode` -> `30 3`.
- `git merge-base origin/codex/chat-gpt-5.5 DesignToCode` -> `24ae0f9f93203f1e02a37a664b7f3b5cd6c710f2`.
- Dry mergeability probe -> exit code 0, synthetic tree `7b9ff7367e13407c2e9d435e4b1f74833d362208`, output `Auto-merging AGENTS.md`.
- GitHub connector PR #6 -> open draft, head `DesignToCode-clean`, head SHA `642b67c2bb2330b35a2b5a4e3972f7c6657abff3`, mergeable true.
- GitHub connector PR #7 -> open draft, head `codex/chat-gpt-5.5`, head SHA `c45d59e78da51b04eab370f720b9d0ec99ad9ccc`, mergeable false.
- `gh pr list --repo lubochka/llm_mvp_core --head DesignToCode --state all --json ...` -> `[]`.
- `git -C C:\Projects\llm_mvp_core status --short -- AGENTS.md` -> `M AGENTS.md`.
- Inclusion check before this audit: all existing `codex-*-2026-06-29.md` reports were already present in both handoff files.

## Machine labels

- `FRESH_HUMAN_FULL_CHAIN_AUDIT`
- `Human-language blocker reports before machine labels`
- `SOURCE_BRANCH_IS_READONLY_AUTHORITY_NOT_WORK_BRANCH`
- `Work happens on DesignToCode in a separate folder or worktree`
- `Only check mergeability into the latest updated source branch; Luba performs the merge`
- `Proof readiness items are tasks, not stop blockers`
- `final audit is not product completion`

claim_scope:
- Fresh human-language full-chain audit after corrected `DesignToCode` worktree realignment.
- Scoped handoff/index inclusion for this new audit only.
- No product completion, PR creation, PR mutation, branch mutation, real merge, cleanup, tests, builds, runtime/browser proof, external AI, secret check, staging, commit, or push is claimed.

evidence_scope:
- Read relevant same-day codex reports in the handoff folder.
- Read-only local git metadata from `C:\Projects\llm_mvp_core` and `C:\Projects\llm_mvp_core_DesignToCode_worktree`.
- GitHub connector metadata for PR #6 and PR #7.
- `gh pr list` read-only metadata for head `DesignToCode`.
- Dry local mergeability probe with no branch/ref/worktree update.

count_claimed:
- 1 fresh audit report created.
- 2 handoff files updated for this audit inclusion.
- 1 refreshed dry mergeability probe.
- 2 PR metadata reads through GitHub connector.
- 1 read-only `gh pr list` search for `DesignToCode`.

independent_evidence_count:
- 19 evidence groups before file writes: same-day report listing/read groups, inclusion check, branch/status/ref commands, PR connector calls, `gh pr list`, original AGENTS status, and dry mergeability probe.

subagent_id_or_NONE:
- delegated-subagent-current-session

downgrade_needed:
- Yes. The evidence supports only this audit, current metadata, dry mergeability at recorded SHAs, and document inclusion. It does not support product completion, PR readiness, merge readiness, clean repo, runtime/browser readiness, visual fidelity, external AI judge proof, package provenance, trace safety, corpora disposition, fixture correctness, or final full-chain completion.

files_changed:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-fresh-human-full-chain-audit-2026-06-29.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-designconversion-handoff-index-2026-06-28.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-designconversion-completion-handoff-2026-06-28.md`

verification:
- Required literal readback must verify the fresh audit filename and required literals in all three changed files.
- No tests/builds/browser/runtime/external-AI/git mutation verification is implied.
