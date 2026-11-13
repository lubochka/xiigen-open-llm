# R17 — внешний workflow/branch rules migration proposal (NOT APPLIED)

**Scope:** read-only по отношению к lubochka/llm_mvp_core@codex/dpo-implementation, GitHub branch-protection/rulesets и продуктовым файлам. Этот текст не является действующей настройкой GitHub.

## Прочитанные прямые источники (13.10.2026)

- core/.github/workflows/s03-windows-verification.yml: on push для codex/dpo-implementation при **единственном** path .kiro/xiigen-slices/luna/s04/windows-verification-request.json; workflow_dispatch с candidate_sha/gate. Windows job windows-2022 timeout 90 minutes. Нужен для native/process/security/delivery properties, например PsecPhysical.
- core/.github/workflows/design-to-code-focused-verification.yml: on push DesignToCode-clean/codex/dpo-implementation/main, pull_request target main, workflow_dispatch. Ubuntu context/build/test work, focused job timeout 30 minutes. Широкий push на core branch создаёт расходы даже для изменений без learning hypothesis.
- Core and planning branch-protection GET: **403 Resource not accessible by integration**. Required statuses, exact branch ruleset, обязательный PR gate, org rules и exemptions — NOT_OBSERVED. Нельзя сделать вывод об отсутствии required checks.

## Действия конкретного владельца, до любых внешних изменений

1. Core/release owner, имеющий право читать GitHub rules, сохраняет branch/ruleset snapshots для main и codex/dpo-implementation: required status context name, source app, strict/required-up-to-date flag, merge policy. Определяет code-owner/reviewer для patch.
2. Классифицирует jobs по risk-matrix R17, отдельно learning experiment и native gate; точно устанавливает, какие действия требуют native Windows before effects/host delivery. PsecPhysical и права не отменять.
3. Выбирает изолированную стратегию для широкого workflow:
   - **A — немедленно (без config change):** оставить triggers, но НЕ трактовать unrelated Windows как критерий learning gain и не ожидать его в science milestone, если permitted policy/merge rules не требуют;
   - **B — после проверки rules:** перенести узко *нерелевантные* branch-push costly jobs к targeted dispatch/change-class selection, оставив обязательный постоянно сообщающий статус, если он необходим для branch protection. Не считать NOT_APPLICABLE фактическим product PASS и не скрывать failure. Сохранить PR/main protections и exact candidate binding.
4. Подготовить отдельный core change к **.github/workflows/design-to-code-focused-verification.yml**. Scope diff: изменить только доказанно избыточный trigger для codex/dpo-implementation либо partition expensive jobs; сохранить required-context compatibility и workflow_dispatch pre_merge/post_merge; не убирать main compatibility. **Пример кандидата к рассмотрению**: исключить branch push только тогда, когда проверены required-context consumers, их замена официально утверждена и есть alternative targeted run. Без этих подтверждений diff НЕ ПРИМЕНЯТЬ.
5. **.github/workflows/s03-windows-verification.yml**: сохранить существующий exact-marker admission и manual dispatch. Менять только если конкретная native capability и recorded evidence показывают недостаточный/избыточный gate. Не вызывать new runs этим предложением.
6. Валидировать migration на draft exact candidate: job statuses при docs-only/algorithm/state/native/release, missing/pending/skip behavior, required contexts, original rights/security gates, measured runner-time. Если required check остаётся Pending или исчез, rollback config до merge.
7. Зафиксировать owner approval, original→new status mapping, PR/SHA, before/after workflow definitions и честную причину, почему данная migration улучшает time-to-learning без понижения safety.

## Запрещённые shortcut

- Не добавлять [skip ci] или paths-ignore к **required** workflow без обновления branch-protection.
- Не менять статусы руками и не подменять skipped на PASS.
- Не коммитить этот proposed patch в core из R17; пользователь разрешил GitHub editing в рамках replanning, но активный R17 договор запрещает product/workflow mutations.
- Не объявлять native capability ready по Linux check и не включать весь Windows-gate в first lesson только из-за существования workflow.

**Owner:** core CI/release governance, routing R19/R20/S05/native; **trigger:** verified required-rule inventory and candidate delivery plan; **status:** UNAPPLIED_EXTERNAL_PROPOSAL / 403_RULES_BLOCKER.
