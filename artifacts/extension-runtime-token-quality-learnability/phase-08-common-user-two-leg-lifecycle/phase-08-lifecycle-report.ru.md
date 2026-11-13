# Фаза 08: common/user two-leg learning lifecycle

## Что решает фаза
Фаза доказывает executable common/user lifecycle только для SkillSelection. PromptToTask, OrchestrationReasoning и ResultGap/correction остаются declared-not-yet-executed: их common/user legs требуются и описаны, но full lifecycle probe для них не заявлен.

## Покрытые подшаги
- 08.02
- 08.03
- 08.04
- 08.05
- 08.06
- 08.07
- 08.08

## Что доказано
- SkillSelection имеет executable lifecycle proof: user-local manifest, portable backup/export, fresh-profile import, approved anonymized share, continue-training and rollback/hash isolation.
- PromptToTask, OrchestrationReasoning и ResultGap/correction остаются required-but-not-yet-executed, поэтому matrix.passed=false до отдельных executable probes.
- Backup/export/import artifacts используют portable refs и remap table вместо абсолютных user/common paths.
- User-local manifest хранит personal state отдельно от common repo state и отключает silent sync.
- Share/copy требует approval, anonymization и не содержит silent sync.
- Common upgrade меняет common manifest, сохраняет personal checkpoint hash и создает rollback record для проверенной SkillSelection path.

## Проверки
- 08.02: PASS - Матрица содержит PromptToTask, OrchestrationReasoning, SkillSelection и ResultGap/correction, но executable lifecycle proof выполнен только для SkillSelection; остальные строки явно оставлены declared-required-but-not-yet-executed.
  Positive: SkillSelection row has covered_by_executable_probe after user-local manifest, portable export/import, approved share, continue-training and rollback/hash isolation.
  Negative: A matrix that marks PromptToTask, OrchestrationReasoning or ResultGap/correction as fully portable without an executable probe would fail this check.
- 08.03: PASS - Store manifest keeps personal checkpoint refs under the user root and outside the common repo root.
  Positive: User profile A has user-local checkpoint entries with Shareable=false.
  Negative: A user root inside common repo is rejected with PHASE08_USER_ROOT_INSIDE_COMMON_REPO.
- 08.04: PASS - Backup/export package copies the personal checkpoint and store manifest with hashes.
  Positive: Export package contains personal-adaptive-checkpoint.json and user-local-store-manifest.json.
  Negative: A missing checkpoint is rejected with PHASE08_EXPORT_CHECKPOINT_REQUIRED.
- 08.05: PASS - Fresh profile import copies the exported checkpoint and verifies the same hash without retraining.
  Positive: User profile B imports the checkpoint and keeps the exported hash.
  Negative: A package without personal checkpoint would fail PHASE08_IMPORT_CHECKPOINT_MISSING.
- 08.06: PASS - Share/copy creates only an anonymized manifest after explicit approval and disables silent sync.
  Positive: Approved share manifest contains role, file name, hash and size only.
  Negative: Unapproved share and silent-sync requests are rejected before any share manifest is accepted.
- 08.07: PASS - Continue-training loads the imported checkpoint, appends a new signal id, and does not mutate common state.
  Positive: MetricBefore=2 and MetricAfter=3 after adding phase08-signal-3.
  Negative: Missing checkpoint and attempted common mutation are rejected.
- 08.08: PASS - Common upgrade changes only the common manifest, preserves personal checkpoint hash, and rollback restores the previous common manifest.
  Positive: Personal checkpoint hash before and after common upgrade is identical.
  Negative: A common upgrade that attempts personal overwrite is rejected with PHASE08_PERSONAL_OVERWRITE_REJECTED.

## Dashboard/API QA boundary
В Phase 08 не менялся dashboard/API проект, потому что allowed write scope не включает 43.SmartSkillsDomain.Dashboard. Доказательство остается domain-service + xUnit + evidence leaf; отдельный dashboard scenario должен позже показать эти же lifecycle rows через UI/API без объявления host runtime proof.

## Что не заявлено
- Не заявлена full semantic quality.
- Не заявлена full extension readiness для Claude, Cursor или Codex.
- Не заявлены token savings.
- Не заявлена финальная готовность проекта.
- Не заявлен executable lifecycle proof для PromptToTask, OrchestrationReasoning и ResultGap/correction.
- Не запускался host live proof и не начиналась фаза 09.
