# S27 — law-backed subjects, good/bad/edge and independent counterexamples

The labels CODE_Q, BUG, PLAN, ARCH, REFACTOR from F/DECISIONS-LOCKED D-04 are **intent**, not C#/SQL/EN-RU subject domain. Each panel must separately preserve task intent, subject, actual source-family law, version, language/dialect, tenant/project, OutputRefs, context provenance and rights. The following are illustrative, NOT authorized or sealed DPO data.

| Domain | Law to distinguish on real cases | Good source-based feedback | Bad shortcut / hard negative |
|---|---|---|---|
| C# docs | nullable s?.Length vs int fallback s?.Length ?? 0; exact member semantics by framework version | correct factual claim, source, language version, real output | mark nullable-vs-zero correction as style |
| EN↔RU | “do not send before approval” preserves NOT and temporal dependency | choose equivalent translation on unknown source families | train on literal word overlap or invent positive/negative from two equivalent styles |
| SQL docs | LEFT JOIN predicate on right table in WHERE filters unmatched vs predicate in ON; SQL NULL three-valued logic | classify predicate relocation as semantic fix after dialect check | “LEFT JOIN always preserves every left row regardless of WHERE” |
| Angular | child output event vs parent input, directive scopes and framework version | correct direction/typing of binding | transform syntax without confirming version or event owner |
| Figma→HTML/CSS | narrow viewport/RTL/layout constraints and source asset rights | visual/browser outcome across viewport families | count passing snapshot at one resolution as design correctness |
| Orchestration | A's typed OutputRef required by B; cancellation and non-idempotent side effect constraints | route violation before Graph learning; type/dependency repair first | parallel B before A because cosine/path score high |
| DPO Builder | same immutable x, chosen must be lawfully preferable to meaningful rejected | CODEX TIE/BOTH_BAD when contrast absent | credit a hallucinated negative as meaningful learning signal |

For each active domain include: accepted lawful correction, syntactically similar opposite-meaning counterexample, genuine stylistic preference, changed task/OOD, stale law version, cross-project/rights unsafe attempt, and cold first-use user question. Freeze TRAIN/OP_DEV/SEALED_CERT separated by SOURCE FAMILY before paraphrasing. Different editions of same law source may leak into evaluation if cross-assigned; record origin family.

**Concrete triage examples:** user says “Make it shorter” after correct C# explanation → STYLE_PREFERENCE; no factual-law DPO. User says “Null reference possible here, this returns int?” and source shows int? semantics → C# law anomaly, determine deterministic fix or true Student weakness. EN→RU user reverses a negation → law-semantics failure; not a harmless synonym preference. SQL user edits WHERE to ON but task asked to keep unmatched rows → actual law correction; if task asked to filter, edit may be a changed task. OOD source with unknown rights → quarantine. Any positive/negative DPO pair requires Chinese both final sides and CODEX exact inspection AFTER feedback triage.

Functional result for each: class accuracy macro/worst family, false teaching admission, false blocking of legitimate weakness, path and domain mapping, timestamped law authority, metrics P/C0/E/T only if real. Never “pass” from a made-up 100% fixture.
