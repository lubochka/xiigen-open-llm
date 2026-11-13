# S19 — architect decision: distinguish laws, learned owners and interference
## Problem and novel seam
Same-system two-domain learning can falsely improve overall average while worse answers for an older domain appear in the ordinary PC consumer. S19 proposes **an interference-aware eligibility and selection seam**, not a second engine. It must remain optional and surgically integrated after live method tracing. If current project LoRA/replay/router already solves the issue, measure and reuse.

## Separate axes and authority
- taskIntent D-04 (CODE_Q, BUG, PLAN, ARCH, REFACTOR)
- subjectDomain (e.g. C# documentation, SQL documentation, EN↔RU translation)
- project/workspace, stack/runtime, authoritative law/dialect/version, requested typed output, source rights/privacy, representation/codebook, selected Student/LoRA adapter, permitted memory pool, policy/Graph state.
The **same** intent is allowed in multiple subjects; a common embedding neighborhood does NOT permit cross-subject reuse. Unknown/conflict/low confidence must not silently choose a confident incompatible adapter. Hard constraints and arbiter precede learned preference (D-15). D-05 project adapters are retained: do not invent per-domain LoRA weights as an assumed approved design.

## Actual hybrid pipeline to trace, not reimplement
raw input → stack/law/intent/subject classification (D-07 ordering) → typed law-bearing tokens/IR with version and direction/units/negation/predicate → project-compatible adapter + representation/memory eligibility → RP02 lawful **multi-neighbor** adaptive kNN → RP03 ILearningUnit many→many typed candidate associations → RP03 ILearningFlow learned correction of next Unit input → RP04 ILearningOrchestrator bounded asynchronous route candidates → RP04 ILearningDynamicGraph higher-order pattern relation → deterministic arbiter → existing Student generation and response → RP05 admitted feedback and, optionally, RP01 exact judged DPO/LoRA update → RP09 exam / RP10 controlled state adoption.

S19 owns **only one proven-missing cross-domain compatibility/selection edge** between these existing mechanisms, plus causal collision observations; it delegates algorithms to their original owners. Every interface name in this paragraph is a conceptual owner role until K implementations, generics, callers and consumed outputs are traced. ILearningUnit may return IEnumerable/results many→many; never reduce to positional ZIP or assume one output.

## Example of a real interference candidate
C# task **CODE_Q** about a nullable annotation and SQL task **CODE_Q** about a query can share D-04 intent while having different syntax/laws and retrieval neighborhoods. If a shared intent router selects the same semantic token policy, kNN memory or wrong project's LoRA, it is a collision candidate. **Do not claim collision exists** without a trace from actual ordinary query to used evidence to actual output. Similarly SQL Server TOP vs PostgreSQL LIMIT is a dialect-law difference; S17/RP14 handle in-domain law drift, S19 examines wrong cross-subject/domain borrowing.

## Four distinct trainable roles and causality
1. ILearningUnit: lightweight domain-specified vector/output-shaped many→many candidate machine, preserves linked InputRef/OutputRef; state change is distinct from retrieval hit count.
2. ILearningFlow: learns/corrects relations between typed Units; never changes law or silently edits Unit state.
3. ILearningOrchestrator: deeper asynchronous input→output strategy, explicit real parallel/fan-in, cancellation/deadline/full-branch costs; running async is not evidence of trained policy.
4. ILearningDynamicGraph: higher-order pattern combinations and learned edge/path preferences with provenance; never overrules arbiter.
Student's DPO/LoRA and Builder's teaching meta-policy are **additional distinct states**, not synonyms of these four roles.

## Proposed falsifiable hypotheses (all unmeasured)
H19.1 Subject/law/project-scoped **selection eligibility** lowers wrong-domain memory/adapter use compared with intent-only selection at the same budget.
H19.2 A domain-aware negative-transfer monitor detects domain B regressions hidden by aggregate mean after a domain A lesson.
H19.3 A minimal compatibility/selection intervention improves worst-domain lawful utility, if a traced collision actually exists; otherwise REUSE_NO_CHANGE or HYPOTHESIS_REJECTED.
H19.4 Existing D-06 70/30 replay under permission is not automatically sufficient in every domain; one-factor **observational** study may find better or worse retention; changing the locked ratio is NOT authorized by this slice plan.
H19.5 Training one new lawful chosen/rejected pair on A and optionally a different one on B improves only when independent T1/T2 effects beat C0 on suitable domain-law and whole-PC measures; negative and null results are valid.
H19.6 Ordinary cold-selected state loads after crash/rollback without double-step or wrong domain association.

## Owner-routing and no-duplicate decision
- Mapper/subject/vector/project metadata or kNN scoring bug → RP02.
- Exact Chinese pair material/objective/LoRA optimizer bug → RP01/R18.
- Unit/Flow edge state bug → RP03; async/Graph bug → RP04; journal/feedback/replay → RP05; live host → RP06; Builder decision quality → RP07; general cross-domain transfer → RP08; independent lab/score → RP09; promotion/PC → RP10; causal credit/controller → RP11–RP13; law-version drift → RP14/S17.
- **Only** a missing eligibility bridge that misconnects distinct existing correct owners is S19's own narrow code seam, subject to architectural review and one-writer grants. Changed-law transitions remain S17/RP14.
