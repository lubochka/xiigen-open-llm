# Review/Fix Pass 08 - No-overclaim firewall and evidence separation

Verdict after fix: `PASS_WITH_FIX`.

This is a real review/fix pass, not a ledger row. The ledger row points here only as an index. `ledger_rows_are_evidence=false`.

## Before finding

The plan already had a Section 11 list of forbidden claims. That was not enough, because a future report could still say "dashboard passed" or "token count improved" in a way that sounded like full extension readiness.

The missing piece was a human-readable evidence separation rule:

- what each evidence type may prove;
- what it cannot prove;
- the exact limitation wording the report must show;
- what safe independent track continues if one evidence type is missing.

Without that, dashboard/API explorer, direct handler calls, test-only adapters, static JSON, smoke tests, positive-only traces, token deltas, quality eval, DPO rows, local training, fresh-load, held-out/ablation, or rollback reports could be overclaimed.

## Concrete fix

Patched `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`:

- Section 8 row 8 now points to this packet and is no longer ledger-only.
- Section 11.1 now contains an evidence separation matrix.
- Section 11.2 now contains a mandatory user-visible report wording gate.

## Exact changed sections

- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` Section 8
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` Section 11.1
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` Section 11.2
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1-REVIEW.jsonl` row 8

## Recheck evidence

After the fix, the plan requires every report to include:

```text
What this proves:
- <narrow evidence claim tied to the exact packet, host, scenario, machine, or checkpoint>

What this does not prove:
- <runtime, quality, learnability, semantic readiness, common readiness, target promotion, or rollback scope that remains unproven>

Next independent track:
- <the next safe track that can continue without turning this limitation into a stop gate>
```

The plan now explicitly separates these evidence types:

- dashboard/API explorer QA;
- direct handler/core call;
- test-only adapter;
- static check / JSON report;
- smoke test;
- positive-only trace;
- host runtime proof packet;
- token delta report;
- quality eval report;
- local DPO candidate artifact;
- user-local continue-training report;
- fresh-load report;
- held-out/ablation report;
- rollback report;
- final full-extension report.

The plan now says dashboard/API evidence cannot claim Claude/Cursor/Codex runtime, token savings cannot claim quality, quality eval cannot claim learnability, local learnability cannot claim common readiness, and full extension functionality requires packet-backed evidence across all required tracks.

## Scope

No product code, runtime, training, build, tests, git, commit, or push were run in this pass.

`overall_complete_claimed=false`.
