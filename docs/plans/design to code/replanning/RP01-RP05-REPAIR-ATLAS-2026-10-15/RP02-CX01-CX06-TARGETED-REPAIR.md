# RP02 CX01–CX06 · Targeted algorithm and examination repair

**15 October 2026 · INTERMEDIATE SOURCE/DESIGN PUBLICATION · AUTHOR REVIEW ONLY.** This extends the existing atlas, not its product dispatcher. Namespace `RP02-CX` is distinct from RP02-F17–F32, general items 17–46 and other slices' F-numbers. No new mandatory rounds, experiment IDs, Windows runs or coverage targets are introduced.

Planning branch observed at `30bee7fdbdf32a491319d084a243669115b36c3f`; core branch observed at `025c42c74b1d30ffa4660f9824a52cf8fcaf7c88`. Source claims below are additionally bound to actual file blobs. Existing RP02 revision 7.3 is the baseline: its 18 tasks, CODE/UPDATE split, first ready-domain lesson, independent C0/T and zero-new-tests default are retained. This publication does not claim implementation or training.

## Findings and narrowly scoped owners

| ID | Classification | Concrete issue | Owner / when it matters |
|---|---|---|---|
| CX01 | Verified prompt under-specification; mathematical counterexample | Concatenating context and action in a linear action scorer makes action ordering independent of context | 02B2/LEARN; only the learned search-policy claim |
| CX02 | Current source counterexample; ordinary exposure unverified | TrainingVectorBuilder.Build(prompt) fits a new local root vocabulary per request, so identical coordinates can mean different words | 01B, with actual producer/caller trace from 00A |
| CX03 | Current source counterexample; ordinary exposure unverified | NormalizeRoots removes before/after, erasing a domain-relevant temporal relation even with a shared vocabulary | 01B/01D for the affected normalization path |
| CX04 | Source-informed examination detail | BPE pieces can be partial UTF-8 or grammatical prefixes; a whole-word veto applied per piece can exclude valid continuations | 02D and 01B, only an actual incremental token consumer |
| CX05 | Proposed first-lesson discriminator | Query-only or constant post-filter law features cannot teach candidate preference; valid but trivial DPO contrasts can teach nuisance cues | 01D, 02B1/S/UPDATE; before its actual chosen lesson |
| CX06 | Proposed cost-examination refinement | Fixed arm order, timeouts dropped from denominators or serial loading billed to one arm can manufacture apparent PC efficiency | 03A, 02B2, 02C within existing C03–C05 |

These are **not six observed production incidents**. CX01 is a permitted bad implementation of an underspecified prompt, not proof that it was implemented. CX02/CX03 are deductions from current source, not .NET runtime reproductions. CX04–CX06 add concrete discriminators to existing obligations. Existing top-1, duplicate-ID, holdout leakage, paid-probe and similarity-calibration findings are reused, not counted again.

## CX01 · A contextual policy needs context × action interaction

**Source:** current `tasks-v7/children/RP02.02B2-LEARN.md`, blob `a387b4529bc7eec231538aaccce0acd80ef473ec`, S prompt. It permits `Q_w(h,a)=w·χ(h,a)` with predecision context plus action description but does not require an interaction.

**WRONG permitted interpretation:** `χ(h,a)=[h;one_hot(a)]`. Then `Q(h,a)-Q(h,b)=w_a-w_b`: the context term cancels. Weights may change, yet every context receives the same action ordering. This does not meet the requested domain-dependent dynamic k.

**CORRECT minimal option:** action-specific contextual weights `Q(h,a)=w_a·[1;h]`, equivalently a context/action tensor product. For a two-action public diagnostic, let ambiguity h be 0 or 1 and let `Q(expand)-Q(probe)=-1+2h`; probe wins at h=0, expand at h=1. These are illustrative values, not required learned weights, training data or an observed result.

**Required output:** explicit χ layout/dimensions, action mask, feature-availability timestamps, two contexts with a justified ranking reversal, named mutable weights and cold reader. A paid probe's features are usable only after that probe. For different legal action sets, compare only jointly feasible alternatives and account for the real rollout cost.

**BAD:** claim adaptive learning from a changed checkpoint, only action biases, or future exhaustive-search margins. **EDGE:** equal utilities, no feasible action, no supported counterfactual, zero feature difference. **Recovery:** fix χ or collect a new authorized TRAIN observation; no hidden exam tuning, no replay of an already applied lesson. Use existing C04, not a new gate before the θ-ranker pilot.

## CX02 · A shared width is not a shared local root dictionary

**Source:** K `23.SemanticCompilerAdapter/TrainingVectorBuilder.cs`, blob `d1fc22d7823f377da497881c0e540218dd22c6a2`, `Build(string)`, `Build(DevelopmentPromptCompilation)`, `BuildRootVocabulary`, `BuildVector`.

`Build(string)` calls `BuildRootVocabulary([prompt])`. The root vocabulary enumerates that request's first distinct roots, assigning local slots 0..31. In the source-defined public counterexample, `Build("alpha beta")` and `Build("gamma delta")` give the same root occupancy and the same predicate/domain/final slots, although the root meanings differ. This is not proof that a production caller compares these overload outputs; that caller remains to be traced.

**CORRECT:** before using vectors in a shared index or learner, identify the actual overload and its callers. Reuse a frozen TRAIN-fitted shared dictionary passed to `Build(prompt, rootVocab)`, or an explicitly versioned stable transform already owned by that path. Persist the dictionary/normalization digest and share it between training, query and cold loading. A dictionary-free relational representation is also possible when its semantics are explicit; do not replace the entire tokenizer merely because this particular helper is unsuitable.

**GOOD:** different words occupy their declared shared coordinates and unchanged inputs keep coordinates after unrelated requests. **BAD:** fit one dictionary per query, pad all vectors to 60 and call them compatible. **EDGE:** unseen roots, vocabulary order changes, 32-root truncation, cold-load mismatch. **Recovery:** explicit unknown representation or scoped compatible reindex; never train θ to compensate for unidentified coordinates. Check one relevant current-path discriminator, not every word by snapshot.

## CX03 · Preprocessing must not erase the relation being learned

**Same K source**, `NormalizeRoots` explicitly drops `before` and `after`. Under this helper, `send before approval` and `send after approval` have the same remaining roots, predicates, domain slot and final slot; using a shared root vocabulary alone does not repair this distinction.

**CORRECT:** trace stack/domain detection → normalization → relation extraction → candidate features. Preserve raw spans and a typed temporal relation (including UNKNOWN) before destructive normalization, or minimally retain the needed relation in the selected encoder. It must reach the real scorer. Do not claim every BPE tokenizer or every domain path has this defect.

**GOOD:** before/after/order is represented as a relation between the relevant events. **BAD:** a byte-perfect stored pair becomes identical ψ and is counted as a useful optimizer lesson. **EDGE:** negated condition, quotation, unrelated events, missing referent. Do not hard-code every word `before` to one global instruction.

**Recovery:** repair this transform and obtain new valid derived features; if final authored DPO text changes, require new Chinese authorship and exact-pair judgment. Use existing C02/C03 law families. A representation repair is engineering until an independent learned-state effect is measured.

## CX04 · Byte tokens, word roles and complete syntax are different units

**Source:** K `01.Tokenizer/BpeTokenizer.cs`, blob `d7c5e69308a32bf832ec4b087097a35b26f51d64`, Train/Encode/Decode; current 02D blob `4d8dba22a23d0cdca9daddbe554f4632c51db779`. BPE begins with all 256 byte tokens; Decode uses UTF-8 reconstruction. One byte can be an incomplete prefix of a valid Cyrillic character. Current 02D discusses partial ASTs but leaves the byte/lexeme/grammar boundary implicit.

**CORRECT:** record whether the actual candidate is a byte token, merged BPE piece, Unicode scalar, lexeme, AST action or complete answer. Incremental validation retains the decoded prefix and pending byte/parser state. Distinguish `prefix viable`, `proven contradiction` and `unknown`; these are proposed diagnostic labels, not new public API enums. A final grammatical rule is enforced at its proper boundary, not on every fragment.

**GOOD:** a first byte of `к` is pending, and the complete character can continue normally. **BAD:** a replacement character obtained by decoding one partial token becomes evidence that the proposed Russian word is illegal; a legal incomplete C# expression is rejected as a finished program. **EDGE:** actually invalid UTF-8, EOS, tokenizer merge boundaries, combining characters and rollback of an abandoned candidate. Inspect the actual EOS consumer and reserved-token contract before making claims from BpeTokenizer.EosToken; no global EOS repair is authorized by this document.

**Recovery:** preserve incremental state or explicitly use whole-action/answer scoring. Do not claim a next-token implementation without a real reader. Extend existing C07, not the first answer-ranker prerequisites.

## CX05 · The first lawful contrast must be informative at the decision stage

**Sources:** 01D blob `3eb39b3aa167a29546cc4b8b5e23981aef04d373`, 02B1/CODE blob `61054c06d6ad66767b47bb037f2fe699cddfbf9d`, runbook blob `6d177e82549cf8cd9657681e6e9060517f524df5`. Existing d=0 protection is retained; the added detail is how to choose a contrast that survives the real pipeline.

For each ψ field specify whether it depends on q alone, candidate z, or their relationship. Query-only fields cancel in pairwise ranking; hard-law violation flags may be identically zero after valid hard filtering. Neither should be presented as the feature teaching a difference between surviving candidates.

**CORRECT first lesson:** choose a permitted pair that differs on a candidate-conditioned, inference-computable domain relation or helpfulness distinction. Show pre-filter eligibility, serving admissibility, candidate support, ψ+, ψ− and the data-gradient separately. A lawful negative-memory record may inform avoidance without becoming an accepted output; metadata rights are never relaxed to create a loser. If only one candidate can ever survive, label its advantage deterministic admission, not a ranker-learning opportunity.

**GOOD:** two permitted explanations differ in preservation of a required condition, and the scorer can compute that difference without the hidden answer. **BAD:** query operator flags are identical for both candidates; only ID or length distinguishes them; correctness is copied from the later judge into runtime ψ. **EDGE:** both good, both bad, legitimate ambiguity, all rivals excluded, nonzero numerical delta carrying no semantic distinction. Return NO_INFORMATIVE_PAIR or an exact material/feature owner; no fake update.

Use matched public examples to expose length/style/ID shortcuts, then new independent families for quality. Public illustrations are not final DPO. CODEX `astra 6 ultracode` selects the case; an admitted external open Chinese model authors BOTH final sides; a separate CODEX `astra 6 ultracode` judges the exact unchanged pair. Distinct actual invocation receipts are mandatory; no such calls occurred here.

## CX06 · Measure a fair bounded cost, including incomplete work

**Source:** current scientific contract blob `9370d0028f948c5e948d677c4ef51f9a13936fdf`. It already specifies whole-PC cost, family pairing, cold/warm and uncertainty. This is a concrete protocol refinement, not a claim that existing results are biased.

Within the existing chosen C03/C04/C05 experiment, preregister the measurement boundary, warm-up/cache policy, arm order, concurrency and deadline. Balance/randomize C0/T order by independent family when safe; do not always run C0 cold and T warm. Record actual hardware/load and raw per-case latency. Cold-start memory includes simultaneous parent/trial/index residency required by the chosen execution, not just adapter file size.

Keep completed-correct, completed-wrong, justified abstention, timeout/cancelled, unsupported action and infrastructure failure distinct. A timeout has observed elapsed work and a censored completion time, not a fabricated full runtime or automatic semantic-error label. It remains in the appropriate attempted/eligible and budget-success denominator under the preregistered rule. An unrun action cannot become the negative label for learned k. An infrastructure-invalid paired comparison is repaired or marked INVALID_EVIDENCE, not selectively removed from the slower arm.

**GOOD:** both arms receive the declared cold/warm regime and failed work is retained; fixed-budget lawful success improves at comparable coverage. **BAD:** drop every timed-out T request, report only surviving fast answers, or give one arm the other arm's populated cache. **EDGE:** tiny n, thermal/load drift, shared caches, genuine early cancellation, no feasible action. Report raw counts and INCONCLUSIVE where necessary; do not launch a benchmarking framework or mandatory repeated Windows matrix.

## Integration status at this checkpoint

Findings published; in-place task integration is the next action of this same repair. Existing product code, data, weights, sibling results, global skills and workflows are unchanged. No runtime reproduction, teacher call, Student/ranker update, independent domain exam or independent Astra review has been performed.

`PLANNING_DRAFT; PRODUCT_TESTS_ADDED=0; CORE_UNCHANGED; TRAINING_NOT_RUN; DOMAIN_GAIN_NOT_MEASURED; PC_NOT_MEASURED; INDEPENDENT_ASTRA_NOT_PERFORMED`.
