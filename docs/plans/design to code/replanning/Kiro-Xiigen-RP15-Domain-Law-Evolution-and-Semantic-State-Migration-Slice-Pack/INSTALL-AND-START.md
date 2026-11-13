# RP15 — install, dispatch and starting conditions

1. Checkout planning branch **llm_plan3**, confirm HEAD and this directory's README/manifest; core **codex/dpo-implementation** separately and confirm its HEAD. The earlier observed core SHA 3c7f5f8c7809936e25579603c96ea6df8598fc40 is only a planning observation, NOT a future execution pin.
2. Read current P/D/AGENTS.md, P/D/replanning/AGENTS.md, K/AGENTS.md, K/CLAUDE.md and exactly applicable skills under [core docs/skills](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/skills). Re-read before each changed task.
3. Open [SOURCE-AND-OWNERSHIP](SOURCE-AND-OWNERSHIP.md). Search WorkLeft* across D rather than stopping at S04; read relevant RAG .md AND .ndjson row, then real G/F/V target; current Rxx results and other RP packs are evidence, not replacement for live code.
4. Choose ONE [task](EXECUTION-BOARD.md) with prerequisites met; paste [START-PROMPT](START-PROMPT.md) + that entire detailed task text. One task = one bounded executor. Larger 02-domain work splits into independent 02A/B/C/D; no worker is ordered to implement the entire slice in one attempt.
5. For read-only science work, do not call models, train, edit K or start Windows CI. For code tasks 03A/B, obtain specific K write/branch/runtime authority; obey core governance and target only actual observed callsites. For Chinese Teacher or CODEX judging, require distinct real provider/grant and actual invocation receipt.
6. Close each task's own deliverables, primary evidence, negative/edge examination, exact commit SHA/remote readback if published and an honest NOT_MEASURED/NO_CHANGE/REGRESS verdict. Do not self-sign external review.
7. The fastest useful publication is *one live compatibility gate*, not a new 25-round rerun. If equivalent mechanism exists, integrate/run its ordinary consumer and return REUSE_NO_CHANGE.

**No hidden prerequisites:** RP13/RP14 folders not seen during initial RP15 directory inventory, their absence does not block RP15; R01–R25 documentary statuses are not mandatory DONE gates. RP01 E1 Student lesson and RP15 domain compatibility study can proceed independently. Legacy Windows/native/release gates remain only where relevant to a concrete release/host property.

**After each task:** validate diff against declared ownership and meaning, independent review if actually available, commit/push, verify remote SHA; do not start the next task before the previous authorized publication cycle closes. A documentary draft may be published without falsely claiming product PASS.
