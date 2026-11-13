# S32 — actual laws of seven task families and the examiner's counterexamples
The following are **scenario seeds only**; specific laws, editions, licenses, examples and oracle labels MUST be derived from task-matched primary sources. Never turn these examples into sealed gold labels after seeing model output.

| Family | Subject law to source | Good law-aware route | Bad/confusing near-miss | Edge and confidence failure |
|---|---|---|---|---|
| D1 C# API documentation | actual target .NET/C# API revision: async return, nullability, ownership | documents `Task<T?>` and cancellation semantics supported by source | smooth prose that asserts non-null or synchronous outcome | overloads/nullable annotations shift; right-looking retrieval wrong project |
| D2 SQL Server | schema, join and transaction/isolation semantics | preserves JOIN multiplicity and transaction meaning | top neighbor uses wrong dialect/version but high cosine | missing schema/late transaction isolation change |
| D3 Angular UI | target Angular version, template accessibility/state and change path | preserves binding and role/aria semantics | older Angular example ranked high on lexical similarity | changing major version/async state |
| D4 EN→RU translation | negation, numbers, named entities, register, source context | retains `not`, tense and number with natural RU | fluent RU with polarity inversion | idiom or ambiguous referent, low independent judge confidence |
| D5 Figma→HTML/CSS | design assets/rights, viewport, responsive/hierarchy | lawful mobile+desktop layout within source constraints | pixel-close one viewport with broken accessibility | missing asset license, scaled screenshot only |
| D6 installed IDE/code edit | code workspace rights, tool effect, security/rollback | suggests safe edit with explicit user approval and proof | confidence used to authorize file write | no permission, stale index, wrong workspace, rollback |
| D7 mixed/novel/law drift | composite passport per SUBJECT, real version and knowledge bounds | joins C#+SQL+RU with typed OutputRefs, abstains on unverifiable novel law | collapses three laws into one D-04 intent or silently borrows kNN score from another domain | conflicting laws, withdrawn source/unknown domain and insufficient PC |

## Blind scientific panel
For each family freeze before training: (1) SOURCE FAMILY registry and rights, (2) TRAIN vs OP_DEV vs SEALED_CERT disjoint at *source/author/project/time family* not only row, (3) law+version passport, (4) user task and independent functional judge rubric, (5) negative/positive or adversarial queries, (6) baseline/oracle budget and stop policy, (7) DPO pair admission separate from exam samples. Nonzero independent N and uncertainty are required to claim a metric, not an arbitrary common minimum count. Sparse domain → `INSUFFICIENT_EVIDENCE`; no leakage.

Judge outputs: `LAW_VALID`, `LAWFUL_BUT_WRONG`, `LAW_VIOLATION`, `CANNOT_JUDGE`, `TIE`, plus task quality/risk, output type, full cost and correct abstention where applicable. Laws override preferences: a bad-law high-ranked output is veto, not trainable negative automatically.

Required contrasts: known law / wrong law / old edition / mixed subject / low data / conflicting source / poor calibration / adversarial high-confident error / valid but expensive multi-branch / near-tied feasible paths / trained Student changed but stale vector coordinate. A T1 success on DEV never upgrades SEALED_CERT. Real input permissions must not be fictionalized.
