# RP09 — seven law passports, splits and good/bad/tie/OOD

Each law is (domainId, version, source/rights, scope, hard constraint versus empirical regularity versus user preference, exceptions, independent expert). Verify the particular source version *before* choosing law; never elevate estimated patterns into inviolable laws. Do not use the same source family paraphrased in both train and heldout.

| Subject | Law-preserving positive | Hard negative / edge | Independent functional oracle |
|---|---|---|---|
| EN↔RU | Faithful negation, numeric units, names and permitted alternative phrasing | «не больше» vs «больше»; plural/quantity ambiguity; valid different translation may TIE | Human bilingual semantic adequacy + prohibited reversals by source family |
| Orchestration | Actual DAG order, permissions, idempotent/allowed effects and cancellation | Short illegal route, branch race and leaked canceled action; two lawful paths different cost | Feasible goal + best of **enumerated lawful** paths under equal resources |
| Figma→HTML+CSS | Preserve tree/order/geometry/assets/RTL and multi-viewport responsive constraints | Beautiful 1440-only layout fails mobile; unlicensed asset; RTL edge | Real renderer at frozen viewports + visual/functional independent rubric |
| C# documentation | Correct AST/operator, type/nullability, language/version and side effects | s?.Length vs s?.Length ?? 0; async deadlock claim; hallucinated method guarantee | Verified code snippets/compiler/independent source-backed factual rubric |
| Angular documentation | Correct component/template binding, change detection, lifecycle and supported version | Confuse event/source and update order; framework version drift | Version-pinned source and actual behavior where runnable |
| SQL documentation | Dialect semantics for NULL, LEFT JOIN, duplicates, cardinality and transactions | WHERE on right table after LEFT JOIN, NULL = NULL; dialect mismatch | Source-backed query plan or bounded DB engine with isolated legal data |
| DPO Builder | Same x/context, both final sides authored by admitted Chinese teacher, material better/worse reason | TIE, BOTH_BAD, unjudgable, cosmetic rejected, leaked CERT answer, changed bytes | Separate CODEX exact-pair judgement + later independent Student utility (not judge preference alone) |

**Domain identity is multi-axis:** subject × intent × language × stack/dialect × product version × source family × rights × uncertainty. UNKNOWN and MIXED are first-class. Oracle-domain mode is a diagnostic control only, never a production router cheat.

## Panel admission
1. Source/license/privacy/tenant rights verified for **specific use**, stable family key assigned, dedupe near-duplicates by project/document/version origin; closed exam contamination check and immutable embargo.
2. Distinct TRAIN, OP_DEV, CERT families; source-family grouping, not random record split. TRAIN may teach; OP_DEV may tune; sealed CERT cannot be seen by teacher, code author, pair selector, retrieval index, learned memory or metric tuning.
3. Each item has one original input, **set** of valid answers/feasible paths, critical law violations, explicitly allowable abstain, ambiguity/TIE and independent scoring rule; not exact-string-match-only.
4. Pre-register N and stopping rules as variables assigned by source owner **before outcomes**. Per-domain support, confidence/uncertainty and most harmed old-domain family reported. 95%+ is a later owner-stated aspiration, not an automatic current pass threshold.
5. For domains outside the seven, create versioned passport separately; no universal k, tokenizer or latent metric is presumed.

## Negative panel design
**GOOD**: positive meaning and law with alternate wording. **BAD**: same surface similarity yet law violation that must be caught, including missing/corrupted essential DPO answer tail. **TIE**: two legal equivalent solutions or translations; no forced preference. **OOD/MIXED**: missing or dual domain; honest abstain/handoff. **ADV**: poisoning, consent absent, wrong tenant or unsupported version; must fail closed. Cases are evaluation specifications only; never automatically add these to DPO optimizer training set.

### Frozen examiner record
datasetManifestId; source_family_ids by split; licence_consents; law_versions; rubric_version; panel/oracle authority; seeded deterministic pipeline parts; model/index/tokenizer/evaluator/host/hardware versions; declared primary metric, critical guardrails, plannedN, minUsefulDelta, retentionBound, qualityCostBound, uncertainty method, decision time. Unknown fields remain null/NOT_READY, not guessed from a historical report.