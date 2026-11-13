# Scenario execution report - Cursor token savings baseline

CURRENT TASK: advance the next concrete product-readiness scenario with local evidence, not a status-only artifact.
NOT THE TASK: product readiness claim, full Cursor readiness claim, real Cursor user interface (UI) claim, token savings claim, semantic quality improvement claim, training, checkpoint mutation, locator update, commit, push, or stage.
EVIDENCE-ONLY: product readiness scenario board, Phase 03 token/accounting guard evidence, and this scenario-specific adapter/core token trace packet.

## Scenario

| Field | Value |
|---|---|
| Scenario number | 12 of 28 |
| Scenario identifier | `frdp-p01-cursor-token-savings-baseline` |
| Host | Cursor |
| Claim family | token savings baseline for Cursor |
| Marker | `FRDP-P01-frdp-p01-cursor-token-savings-baseline` |
| Selected because | Row 12 was not moved and had a feasible local automated/core/Application Programming Interface (API) path without external AI or unsafe prompt sending. |

## Commands actually run

| Mode | Exit code | Command result | Output log | Error log |
|---|---:|---|---|---|
| baseline | 0 | [baseline-command-result.json](baseline-command-result.json) | [baseline-command-output.txt](baseline-command-output.txt) | [baseline-command-error.txt](baseline-command-error.txt) |
| adaptive | 0 | [adaptive-command-result.json](adaptive-command-result.json) | [adaptive-command-output.txt](adaptive-command-output.txt) | [adaptive-command-error.txt](adaptive-command-error.txt) |

## Concrete result numbers

| Check | Result |
|---|---:|
| Baseline response success | True |
| Adaptive response success | True |
| Pair valid | True |
| Baseline input tokens | 52 |
| Baseline output tokens | 29 |
| Baseline total tokens | 81 |
| Adaptive input tokens | 52 |
| Adaptive output tokens | 29 |
| Adaptive total tokens | 81 |
| Token delta | 0 |
| Token reduction observed | False |
| Quality gate can pass | False |
| Token savings claim allowed | False |
| Token savings claimed | False |
| Semantic quality claimed | False |
| Host runtime claimed | False |
| External AI used | False |
| Training started | False |
| Checkpoint mutated | False |
| Locator updated | False |

## What moved

This scenario moved from no scenario-specific execution packet to a partial local evidence packet. The fresh run proves that a Cursor-shaped baseline/adaptive pair can reach SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest, emit actual core responses, and produce paired heuristic token traces with the same prompt hash, host, method, debug flag, tokenizer name, tokenizer version, and tokenizer accounting type.

The evidence does not prove token savings. The quality gate is closed because token delta alone is not quality evidence, no approved quality regression rubric is attached, and live Cursor candidate behavior is still not proven.

## Evidence files

- [baseline-request.json](baseline-request.json)
- [baseline-response.json](baseline-response.json)
- [baseline-core-trace.json](baseline-core-trace.json)
- [adaptive-request.json](adaptive-request.json)
- [adaptive-response.json](adaptive-response.json)
- [adaptive-core-trace.json](adaptive-core-trace.json)
- [token-traces.json](token-traces.json)
- [token-traces.jsonl](token-traces.jsonl)
- [pair-validation.json](pair-validation.json)
- [execution-report.json](execution-report.json)

## Next concrete action

Run a safe Cursor extension-host or no-send local route for the same token scenario, or attach approved quality non-regression evidence to the same baseline/adaptive token pair before any token-savings claim is reconsidered.
