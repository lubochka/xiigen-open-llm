# Row1 split-only scenario repair review

claim_scope: independent review of Row1 split-only scenario repair artifact.
evidence_scope: read `row1-split-scenario-repair.md`; read `row1-split-scenario-repair.json`; parsed source JSON; wrote this review packet.
count_claimed: 10 requested checks reviewed.
independent_evidence_count: 4 evidence records before this file: target listing, source file list, full Markdown read, full JSON read; plus source JSON parse.
subagent_id_or_NONE: Carver the 2nd / 019f0616-ab7d-7a00-90f2-1c5b4463efa6.
downgrade_needed: yes. This review supports only artifact-quality findings, not product/runtime/UI readiness.

## Verdict

NEEDS_FIX

The split repair is structurally close: it defines 7 sub-scenarios, keeps each under 10 minutes, preserves product-readiness locks, includes counts `done 14/28 scoped`, `product-ready 0/28`, `remaining 14`, handles unknown Claude route as discovery, and names Dalton in metadata.

It does not pass the review because the requested scenario repair must be human-readable Russian, but the artifact is mostly English, and the exact next work order contains a corrupted/mojibake authority quote.

## Findings

1. BLOCKING: Artifact is not human-readable Russian.
   Evidence: `row1-split-scenario-repair.md` headings and scenario bodies are in English, including `Human status line`, `What Row1 tries to prove overall`, `Global Row1 rules`, and S1-S7 descriptions. `row1-split-scenario-repair.json` also stores the same English scenario names, purposes, proves/does-not-prove fields, criteria, and next review requirements.
   Affects checks: 1.
   Required fix: rewrite the scenario repair in clear human-readable Russian while preserving exact meaning, gates, counts, claim locks, and JSON parseability.

2. BLOCKING: Exact next work order has a corrupted current authority quote.
   Evidence: Markdown and JSON contain `Ð¢Ñ‹ bounded Row1 split-only scenario repair sub-agent Ð² C:\Projects\llm_mvp_core...` instead of readable Russian text.
   Affects checks: 1 and 8.
   Required fix: replace the mojibake quote with the readable original authority quote or a readable exact current work-order quote. Keep max active time <=10 minutes and the same no-product/runtime/UI/tests/eval/heldout/training/no-commit/no-push boundaries.

## Check Results

1. Human-readable Russian: NEEDS_FIX. The artifact is mostly English and includes mojibake in the next work order.
2. Splits workflow into 4-8 small <=10 min sub-scenarios/gates: PASS. It defines S1-S7, each timeboxed <=10 minutes.
3. Each sub-scenario has required fields: PASS. Each S1-S7 entry has purpose, proves, does-not-prove, inputs, allowed actions, forbidden actions, PASS/NEEDS_FIX/BLOCKED criteria, and next review requirement.
4. Dual-path same-scenario rule where possible: PASS. Global rules require automatic and local Claude paths to use the same scenario where possible.
5. No product readiness claim until all sub-scenarios and final review pass: PASS. The product-readiness lock and claim locks preserve this.
6. Does not invent unknown Claude host capability: PASS. Unknown route is explicitly treated as discovery/blocker for that sub-scenario, not a stop or readiness claim.
7. Human counts: PASS. The artifact states `Done 14/28 scoped; product-ready 0/28; remaining 14`.
8. Exact next work order <=10 minutes: NEEDS_FIX. The work order is timeboxed to 10 minutes and scoped, but the authority quote is mojibake and not human-readable.
9. JSON parses; `git diff --check -- <target folder>` passes; no false product-ready claim: PASS. Source JSON parsed before this review was written. Review JSON parsed after write. `git diff --check -- <target folder>` passed with exit 0 and no output. No false product-ready claim found in the artifact.
10. Subagent metadata names Dalton, not generic/NONE: PASS. Metadata names `Dalton the 2nd / 019f0610-7555-7ff3-8f59-3d40babb17ad`.

## Files Changed

- `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-claude-product-readiness-product-workflow/row1-split-scenario-repair-20260627-013349/review-20260627-014003/row1-split-review.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-claude-product-readiness-product-workflow/row1-split-scenario-repair-20260627-013349/review-20260627-014003/row1-split-review.json`

## Commands Run

- `Get-ChildItem -LiteralPath 'docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-claude-product-readiness-product-workflow/row1-split-scenario-repair-20260627-013349' -Force`
- `rg --files 'docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-claude-product-readiness-product-workflow/row1-split-scenario-repair-20260627-013349'`
- `Get-Date -Format 'yyyyMMdd-HHmmss'`
- `Get-Content -LiteralPath 'docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-claude-product-readiness-product-workflow/row1-split-scenario-repair-20260627-013349/row1-split-scenario-repair.md' -Raw`
- `Get-Content -LiteralPath 'docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-claude-product-readiness-product-workflow/row1-split-scenario-repair-20260627-013349/row1-split-scenario-repair.json' -Raw`
- `$p='docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-claude-product-readiness-product-workflow/row1-split-scenario-repair-20260627-013349/row1-split-scenario-repair.json'; Get-Content -LiteralPath $p -Raw | ConvertFrom-Json | Out-Null; 'source JSON parses'`
- `New-Item -ItemType Directory -Force -Path 'docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-claude-product-readiness-product-workflow/row1-split-scenario-repair-20260627-013349/review-20260627-014003' | Out-Null; 'review folder ready'`
- `$p='docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-claude-product-readiness-product-workflow/row1-split-scenario-repair-20260627-013349/review-20260627-014003/row1-split-review.json'; Get-Content -LiteralPath $p -Raw | ConvertFrom-Json | Out-Null; 'review JSON parses'`
- `git diff --check -- 'docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-claude-product-readiness-product-workflow/row1-split-scenario-repair-20260627-013349'`
- `git status --short -- 'docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-claude-product-readiness-product-workflow/row1-split-scenario-repair-20260627-013349'`

## Exact Next Work Order

Role: bounded Row1 split-only scenario repair sub-agent.

Max active time: 10 minutes.

Task: Fix only the Row1 split repair artifact so it is human-readable Russian and the exact next work order contains a readable authority quote, not mojibake. Preserve the 7 sub-scenarios, <=10 minute gates, purpose/proves/does-not-prove/inputs/allowed/forbidden/PASS/NEEDS_FIX/BLOCKED/next-review fields, dual-path same-scenario rule, no-product-readiness lock, unknown-route discovery rule, counts `done 14/28 scoped`, `product-ready 0/28`, `remaining 14`, and Dalton metadata.

Allowed writes: only the Row1 split repair Markdown and JSON files in the target repair folder, plus a fresh evidence/review packet if required by the supervising work order.

Forbidden: product/runtime/UI/tests/eval/heldout/training execution, code/tools/board/AGENTS/CLAUDE/skills edits, commit, push.

Return: PASS/NEEDS_FIX, findings fixed, files changed, commands run, JSON parse result, and `git diff --check -- <target folder>` result.
