# Next bounded work order

Work order: `RUNTIME-CORE-BOUNDARY-NO-EVAL-REVIEW-SLICE`

## Зачем

Exact local-private approval для QUALITY-02 отсутствует, поэтому actual eval остаётся закрытым. Чтобы не останавливаться и не притворяться, что fixture rehearsal равен real eval, следующий безопасный шаг двигает другой независимый трек: runtime/core boundary review для будущих Claude/Cursor/Codex thin extensions.

## Что делать

Суб-агент должен read-only проверить текущие source/docs и собрать review packet:

- где проходит `XiigenExtensionApiService` / request-response boundary;
- какие Claude/Cursor/Codex adapter entrypoints уже есть или отсутствуют;
- где риск переноса логики из `llm_mvp_core` в extensions;
- какие evidence classes являются static/review/supporting only;
- какие exact follow-up bounded work orders нужны для implementation/runtime proof.

## Write scope

- Новый timestamped folder under `artifacts/extension-runtime-token-quality-learnability/runtime-core-boundary-no-eval-review/`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`

## Запрещено

- actual eval execution или `dotnet run` eval runner;
- external AI/model calls;
- training, DPO creation, checkpoint/locator/target/manifest mutation;
- source/test/skill/governance/plan edits outside state JSON;
- git add/commit/push or any git write.

## Проверка

- JSON parse for created artifacts;
- `git diff --check` on allowed paths;
- scope guard: no changes outside allowed write scope;
- no source code changes;
- claim locks remain false for token savings, semantic quality, learnability, readiness, final readiness, and product readiness.

Parent performs commit/push only after review. This work order has `git_authority=forbidden`.
