# TOKEN-02 token trace schema

This slice defines a stable trace shape and a minimal accounting helper for later baseline/adaptive comparisons.

## What this proves

- A token trace has stable fields for scenario id, pair id, host, method, debug state, prompt hash, visible answer hash, tokenizer/accounting identity, token counts, timestamps, artifact refs, and no-overclaim flags.
- Baseline/adaptive comparisons must use the same prompt hash, host, method, tokenizer name/version/accounting type, and debug setting.
- The current accounting helper is explicitly labelled `heuristic`; it is not an exact model tokenizer.
- Token delta alone cannot pass quality.

## What this does not prove

- It does not prove semantic quality.
- It does not prove token savings.
- It does not prove local model learnability.
- It does not train, mutate, export, import, or checkpoint any model.
- It does not launch Claude, Cursor, or Codex host runtime.

## Human-readable algorithm

1. The caller provides a scenario id, pair id, mode (`baseline` or `adaptive`), host name, method, debug setting, prompt text, visible answer text, isolation mode, artifact refs, and timestamps.
2. The helper hashes the prompt and visible answer with SHA-256 so the trace can be compared without storing raw private text.
3. The helper counts input and output tokens with the named heuristic accounting method.
4. The helper writes `quality_score = null` and `quality_regression = null`, because TOKEN-02 is not a quality-rubric phase.
5. The helper writes `semantic_quality_claimed = false`, `token_savings_claimed = false`, and `learnability_claimed = false`.
6. Pair validation rejects a baseline/adaptive pair if host, method, prompt hash, tokenizer identity, accounting type, or debug setting differ.
7. Quality-gate validation is separate from pair compatibility: a pair may be compatible for trace collection while still unable to pass quality when there is only a token delta and no approved quality score.

## Code references

- `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\TokenTraceAccounting.cs`
- `C:\Projects\llm_mvp_core\Tests\ExtensionTokenTraceAccountingTests.cs`

## Next independent track

`TOKEN-03` can use this schema with the fixed scenario pack to collect baseline traces. It must still avoid semantic-quality, token-savings, learnability, DPO, training, or checkpoint claims until the later approved gates produce that evidence.
