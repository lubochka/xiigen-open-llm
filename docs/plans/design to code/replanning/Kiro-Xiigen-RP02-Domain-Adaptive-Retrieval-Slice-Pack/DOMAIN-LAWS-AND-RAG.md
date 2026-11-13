# Seven law passports, example families, RAG and early pilot gating

**Subject is not developer intent.** CODE_Q/BUG/PLAN/ARCH/REFACTOR are established development-intent labels. EN↔RU, Orchestration, DTC, C#, Angular, SQL and DPO Builder are seven subject families, plus future original/extras; source law is a typed requirement, not a training label guessed from English terms.

## Seven passports — design examples, not dataset or teacher output

| Domain | Hard law / typed semantic features | Good source-independent example | Bad deceptive neighbor / law counterexample | Unknown/edge & consumer/metric |
|---|---|---|---|---|
| Translation EN↔RU | Negation scope, agreement, quantifiers, named entities, number/unit consistency, directional language | "Do not send before approval" keeps prohibition in Russian | Fluent rewrite removes "not" or inverts <= and >= | Polysemy, indirect negation, mixed source language; translation result: law-loss rate and contextual meaning |
| Orchestration | Typed inputs/outputs, dependency order, tool permissions, side effects, cancellation, full branch budget | Tool call only after prerequisite typed result and grant | Shorter path skips authorization or passes wrong output shape | Concurrent completion, retries, missing tools; valid path and all-branch cost |
| Figma/Canva→HTML/CSS DTC | Layout relationships, element ordering, responsive breakpoints, content, accessibility, RTL when relevant | Generated responsive section preserves text and hierarchy at two widths | Keyword-similar screenshot with swapped hierarchy or omitted required element | Unknown viewport/theme/font asset; rendered-user acceptance, semantic/visual fidelity |
| C# docs | Nullable typing, ?. vs ??, async/cancellation, exception contracts, C# version/runtime, exact code citations | s?.Length described as nullable int; s?.Length ?? 0 explicitly adds zero fallback | Same tokens used to claim identical result type or null law | Unknown project version, overloads and generics; grounded correctness on new files |
| Angular docs | Angular version and lifecycle, signals/RxJS/DI depending actual project, template vs TS relation | Explain lifecycle/version-sensitive behavior from actual source | Closest Angular text from incompatible version asserts obsolete behavior | Version mixed in monorepo, standalone vs module; grounded docs and wrong-version rate |
| SQL docs | Dialect, NULL/UNKNOWN 3-valued logic, JOIN multiplicity, COUNT(column) vs COUNT(*), transaction/order | SQL IS NULL and COUNT(column) interpreted per dialect | C# null semantics or SQL = NULL treated as correct predicate | Ambiguous dialect, stored procedure params; semantic query result/law violation |
| DPO Builder | Exact same x/context, positive truly preferred, rejected plausible material error, teacher & CODEX roles, one-use identity | Chosen respects critical source law, rejected changes a meaningful constraint | rejected is copied weak Student output, style-only difference, or altered x | TIE/BOTH_BAD/license/holdout leak; pair admission and later measured Student gain separate |

**Rights rule:** eligible_to_retrieve = lawful source, partition/split visibility, coordinate identity, permitted domain, memory-role usage. Admissible_to_execute = laws, types, grant, arbiter and side effects. Negative and warning memory MUST stay retrievable when licensed to warn, yet must never be silently promoted as correct demonstration. No magic 0.50 or 0.95 threshold without current owner-authorized calibration.

## Freeze a tiny E2 pilot (do not block on full seven)

- Choose C# nullable CodeDocumentation if ordinary consumer can be reached; if it cannot, justify a different lawful real caller. Freeze its selected actual source families, counterfactual law cases, predicted versus oracle routing and memorized training examples before evaluating new results.
- Source disjointness is by real origin family / repository / time before paraphrase, not text hash only. Training, development and sealed confirmatory groups must not overlap in derived aliases.
- Keep subject-specific hard-law cases (at least one good, one near-miss, one unknown and one wrong-domain distractor in design). Actual N, sample origin, authorization and statistical rule fixed before outcomes. Sample sizes in legacy proposals are examples, NOT a newly approved runtime budget.
- Extend to other six distinct domains when their actual consumer and corpus rights are found; no claim "seven domains learned" from seven synthetic paraphrases or a one-domain E2 success.
- Every law card must carry a primary source reference/source span, reviewer/arbiter type, warning feature, typed IR tokens and failing memory case. The above is a planning sketch, not actual assembled family panels.

## RAG routing protocol — mandatory targeted depth

1. Read D original task / goal / task prompt and specific D 09.09.26WorkLeft*; branch around explicit cross-domain requirement, not around S04.
2. D 09.09.26WorkLeft-RAG-MASTER-INDEX.md → D RAG-00-usage-guide → applicable RAG-01 DPO / RAG-02 DTC / RAG-04 IDE, including NDJSON where needed. Record visited ids and resolve root.
3. Follow actual link to G active-codex-execution-* DPO source, F DECISIONS-LOCKED / ARBITER-KNN / DYNAMIC-FLOW, V RAG-06/LeftToWork, and current K real code; direct PRIMARY_READ and CODE_TRACE must be distinguished from INDEX_ONLY/ATTRIBUTED_PRIOR.
4. Carry new source→law→features→neighbor→output→named owner→ordinary consumer→falsifier. On 404 or loop mark UNRESOLVED, do not invent source.
5. Preserve source/delivery for original Kiro S00–S10, DTC/IDE, RP03/04 DPO/Graph and future feedback as deferred named owners; no global 20-pass reread gate for the first pilot.

## Deliberately false shortcuts

Wrong: route SQL+BUG only to BUG and call the intent the SQL subject. Wrong: drop punctuation then detect C# vs TS. Wrong: use single cosine codebook and declare it lawful for translation/SQL/layout. Wrong: force a refused case to an allowed action because nearest positive had high score. Wrong: let oracle domain guide production and score it as predicted. Wrong: grade meaning using a single reference string only.
