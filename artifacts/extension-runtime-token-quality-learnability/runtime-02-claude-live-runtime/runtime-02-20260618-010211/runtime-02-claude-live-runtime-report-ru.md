# RUNTIME-02 Claude live runtime QA

Generated: 2026-06-17T22:02:46.2895142Z
Run id: runtime-02-20260618-010211

## Статус человеческим языком

Сделано: свежая проверка Claude Code 2.1.177, --plugin-dir plugin validate/list/details, positive SessionStart hook command path into llm_mvp_core, and controlled missing TargetLocator error path.
Сейчас выполняется: RUNTIME-02 evidence packet готов для parent review.
Осталось: полный Claude chat UI / Claude Desktop Appx visible conversation proof не доказан, потому что безопасный non-AI путь не запускает внешний Claude model prompt.
Следующее действие: parent review; если принято, следующий плановый phase - RUNTIME-03 Cursor live runtime QA.

## Что доказано

- Использованный Claude Code executable: C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.177\claude.exe.
- Версия: 2.1.177 (Claude Code).
- Claude Desktop/Appx найден: Claude_1.13576.0.0_x64__pzs8sxrjxfjjc; executable exists: True.
- plugin validate прошёл: True.
- --plugin-dir ... plugin list --json прошёл и JSON распарсен: True.
- --plugin-dir ... plugin details claude-xiigen-smart-skills увидел inline source и SessionStart hook: True.
- Positive hook path вызвал src/claude-entrypoint.js, записал XiigenExtensionRequest, дошёл до SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest, вернул visible redacted answer/debug shape и bound TargetLocator.
- Negative hook path без TargetLocator вернул expected target_locator_required через тот же boundary.
- В evidence flags: external_ai=false, host_training=false, runtime_extension_claimed=false, semantic_quality_claimed=false; production model/checkpoint/locator не мутировались.

## Что не доказано

- Полный Claude chat UI или Claude Desktop Appx visible conversation path.
- Любой внешний Claude model solving path: намеренно не запускался.
- Token savings, quality improvement, broad semantic quality, learnability/DPO/training, promotion/locator update, final readiness.

## Positive visible output

```text
Adaptive task+plan+skills ready. Skills: claim-verification. Semantic quality is not claimed.
xiigen prompt-to-task :
input: <redacted>
output: <redacted>
```

## Negative visible output

```text
Xiigen request validation failed.
xiigen prompt-to-task :
input: <redacted>
output: <redacted>
```

## Evidence refs

- Core evidence: artifacts/extension-runtime-token-quality-learnability/runtime-02-claude-live-runtime/runtime-02-20260618-010211/runtime-02-claude-live-runtime-evidence.json
- Validation: artifacts/extension-runtime-token-quality-learnability/runtime-02-claude-live-runtime/runtime-02-20260618-010211/runtime-02-claude-live-runtime-validation.json
- Evidence ledger: artifacts/extension-runtime-token-quality-learnability/runtime-02-claude-live-runtime/runtime-02-20260618-010211/runtime-02-claude-live-runtime-evidence-ledger.jsonl
- Positive request/response/trace folder: artifacts/extension-runtime-token-quality-learnability/runtime-02-claude-live-runtime/runtime-02-20260618-010211/hook-positive-target-locator
- Negative request/response/trace folder: artifacts/extension-runtime-token-quality-learnability/runtime-02-claude-live-runtime/runtime-02-20260618-010211/hook-negative-missing-target-locator
- Claude command outputs folder: artifacts/extension-runtime-token-quality-learnability/runtime-02-claude-live-runtime/runtime-02-20260618-010211/claude-runtime-commands

## Self-review

- Architecture boundary: PASS narrow - Claude repo stayed thin; core logic remained in llm_mvp_core.
- Hidden deterministic shortcut: PASS narrow - no host-side selector/scorer/training/model path was added or invoked.
- No-overclaim: PASS - report labels this as plugin load plus hook/core-boundary evidence, not full chat UI readiness.
- Evidence freshness: PASS - all commands in this packet were generated under runtime-02-20260618-010211.
- Human-readable status: PASS - this report says what is done, open, and next.
