# RUNTIME-04 Codex live runtime QA

Run: runtime-04-20260618-020543

## Что доказано

- Найден установленный Codex app package: OpenAI.Codex_26.609.4994.0_x64__2p2nqsd0c76g0; shell-visible codex.exe path: C:\Program Files\WindowsApps\OpenAI.Codex_26.609.4994.0_x64__2p2nqsd0c76g0\app\resources\codex.exe.
- Прямой запуск codex.exe из PowerShell в этой среде вернул Access denied, поэтому full Codex CLI/app-server/chat send-message путь не использовался.
- Использован максимально безопасный доступный путь: installed Codex plugin cache entrypoint: C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\src\codex-entrypoint.js.
- Cache entrypoint SHA-256 совпадает с repo entrypoint SHA-256: 369a6c50260ad609f6c75d75f9e20585e9d534d53822d1851a78c25f79da24b6.
- Positive case создал XiigenExtensionRequest, передал TargetLocator, вызвал C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1, а тот вызвал XiigenExtensionApiService.ProcessHostExtensionRequest.
- Core response показал target_locator_bound=true, target_locator_mode=target, debug shape xiigen prompt-to-task / input / output присутствует.
- Negative case с missing TargetLocator under target-locator-required вернул controlled error target_locator_required без AI/model/training.
- Во всех runtime artifacts записано external_ai=false; training/model/checkpoint/production locator mutation не запускались.

## Что НЕ доказано

- Не доказан полный Codex chat UI / app send-message visible conversation path.
- Не доказан Codex model-backed task solving path; он сознательно не запускался, чтобы не вызвать внешний AI.
- Не доказаны token savings, quality improvement, semantic quality, learnability, DPO/training lifecycle, target promotion, locator update, rollback или final readiness.
- Этот packet не заменяет TOKEN-01 и последующие token/quality/learnability фазы.

## Evidence paths

- Evidence JSON: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\runtime-04-codex-live-runtime\runtime-04-20260618-020543\runtime-04-codex-live-runtime-evidence.json
- Validation JSON: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\runtime-04-codex-live-runtime\runtime-04-20260618-020543\runtime-04-codex-live-runtime-validation.json
- Self-review JSON: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\runtime-04-codex-live-runtime\runtime-04-20260618-020543\runtime-04-codex-live-runtime-self-review.json
- Evidence ledger JSONL: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\runtime-04-codex-live-runtime\runtime-04-20260618-020543\runtime-04-codex-live-runtime-evidence-ledger.jsonl
- Positive raw request/response/trace: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\runtime-04-codex-live-runtime\runtime-04-20260618-020543\positive-installed-cache-entrypoint
- Negative raw request/response/trace: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\runtime-04-codex-live-runtime\runtime-04-20260618-020543\negative-missing-target-locator
- Codex discovery logs: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\runtime-04-codex-live-runtime\runtime-04-20260618-020543\codex-discovery
- Plugin mirror report: C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex-live-runtime\runtime-04-20260618-020543\runtime-04-codex-live-runtime-mirror-ru.md

## Scope wording

This proves the named Codex installed-cache/plugin-entrypoint path for this scenario only. It does not prove model quality or learning.

This is smoke/plugin-entrypoint evidence only. Full extension readiness remains unproven.

## Следующий независимый трек

Если parent review принимает RUNTIME-04, следующий bounded phase: TOKEN-01 token before/after pairing baseline/adaptive setup. Он должен начинаться без claims о savings до valid pairing и quality non-regression gate.
