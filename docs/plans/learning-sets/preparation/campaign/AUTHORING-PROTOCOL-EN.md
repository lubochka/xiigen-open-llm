<!-- CURRENT-OWNER-20261026 -->
## Current parallel execution surface

The active registry has 24 domains, 217 allocations and 24,000 candidate TRAIN pairs. Use one unique agent ID and the assigned translations/orchestration/software pool on the shared data root. Read the returned coordination-prompt.md, coordination-material.required.json and coordination-commands.md paths, not historical executable commands embedded in the original request. Preserve that original request and existing rows.

Use returned progress after saving real full prompt/chosen/rejected records, finish for the assigned complete batch, then identity-bound next. Shared status: C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/status.json; authority: state.json. The quality_flags identify domain/topic/profile issues and evidence, not an independent-review gate. Never manually edit status/state, steal a claim by age, or share one agent ID across windows. The legacy batch-request runner is explicitly not coordinated and remains sequential.

See [parallel work and status](../28-PARALLEL-GENERATION-AND-QUALITY.md), [three starts](../00-START-HERE.md), [translation checks](TRANSLATION-SELF-CHECKS-2026-10-26.md) and [software checks](SOFTWARE-SELF-CHECKS-2026-10-26.md). Keep the subject examples below; current registry/owner commands determine quota and continuation.
<!-- /CURRENT-OWNER-20261026 -->

# DPO authoring protocol — CREATE_ONLY

Use English for internal reasoning. Do not output private reasoning, a chain-of-thought transcript, or a long planning monologue. Use the assigned languages for the student prompt and answers. Give the user short progress updates in Russian. This is an authoring instruction, not a new review or training phase.

## 1. Follow one explicit assignment

Read the current request and its assigned lesson. Copy its domain ID, objective, class counts, source IDs, output path, remaining count, and next ID. Do not derive a different assignment from an old log, a neighbouring folder, or examples. The target is 1000 pairs per assigned domain, distributed by the active registry; a stream, profile, or microtopic does not receive another 1000 automatically.

Keep one teaching objective in a pair and one assigned topic in a batch. Classes and negative controls may coexist only in the proportions explicitly assigned. Do not invent a 5+5 mixture. For example, VALIDWORD teaches preserving a correct word; it does not authorize adding typo-repair lessons. In a routing lesson, translation requests are inputs for choosing a route, not lessons that teach translation.

## 2. Construct one real preference pair

First identify the student's task: translate, choose the next action, explain code, or implement code. The student prompt must contain that task and every fact needed to answer it. “Generate a DPO dataset” is an instruction to you, not to the student.

Use this small construction sequence:

1. Choose a new concrete situation within the assigned source and objective.
2. Write a complete, unambiguous student prompt.
3. Write the complete correct answer as `chosen`.
4. Copy that answer and introduce exactly the assigned meaningful error to obtain `rejected`.
5. Compare both answers against the same visible facts. Keep their format, language, and unrelated content comparable.

Both answers must respond to the student task. A file inventory, generation plan, explanation of how to create DPO, or claim that work is complete is not a translation or code answer. Rejected should be a plausible wrong answer, not random text. Do not hide the deciding fact in metadata, a source document the student cannot see, or your private reasoning. A concise assigned error label or check result may be stored as metadata; it is not part of the student's answer.

## 3. Preserve families and shared scenarios

Use assigned canonical IDs. For translation, the canonical identifies the language, lemma, and intended sense. A typo, missing letter, repeated letter, or wrong-keyboard rendering of that lexeme remains in its family when the context establishes that relationship. Unrelated words such as “молоко” and “window” must not share a lexical family merely because they have the same row number. An ambiguous malformed token is not assigned a family by guessing.

The EN, RU, and HE versions of one underlying situation share an origin/scenario identity across directions and target-language domains. Their language-specific lexical canonicals remain explicit. Preserve this link instead of treating translations as independent exam material. For code and documentation, an implementation task and an explanation of the same program share the source-scenario identity, while their domain IDs and required answers differ. These links prevent relatives from being split across TRAIN and a closed EXAM. Do not change an existing split or invent a new family scheme during generation.

Public reference examples teach the construction method. Do not copy them, replace only a name or number, or present their descendants as new independent examples. Use the assigned fresh scenarios and IDs.

## 4. Perform small content checks while writing

For deletion, the damaged token has exactly one fewer character under the lesson's specified counting rule, and removing one position from the canonical reproduces it. `письмо → писмо` is deletion. `письмо → пильмо` keeps the length and changes a character: it is substitution. For insertion, deleting one added position recovers the canonical. For substitution, length is unchanged and exactly one position differs. For adjacent transposition, swap exactly two neighbouring, different characters; length and character counts remain unchanged. Do not invent a Unicode counting rule if the assignment already supplies one.

For keyboard-layout cases, apply the supplied key map character by character, including punctuation rules. Do not guess from visual resemblance. For example, with the stated RU→EN physical-key map, `окно` becomes `jryj`; a different sequence needs a different explanation.

Read the entire translation. Preserve negation, subject, number, tense, location, and possession. Hebrew `שולחנו` means “his table” in that context, not “our table”; changing the owner creates an additional error. Repair only the assigned defect. A syntactically valid JSON row does not establish these facts.

## 5. Save, continue, and handle tool states correctly

Use the exact current request path under the assigned learning-sets root. Keep existing runs and drafts. Perform the requested JSON/schema/count checks, save the draft state, and continue the next assigned batch until the domain quota and queue goal are reached. CREATE_ONLY does not add independent review, blind judging, export, training, checksums, or production CI.

Finish accepts only the saved draft and family-material paths. A completed draft is recorded with its size and mtime_ns, without checksums. Repeated finish/resume blocks if that draft is missing or its stamp changes; preserve the old state and use an explicitly assigned revision instead of resetting progress. This is a lightweight persistence guard, not semantic review or a content-integrity guarantee.

Inspect the tools actually exposed in the current Kiro session and their documentation. Use a documented synchronous completion option or the documented wait/resume operation for the returned asynchronous handle. Keep the same handle. A missing early result file does not prove failure; a shell return does not prove its background child finished. Do not relaunch, repeatedly poll an unrelated tool, or end the task with “waiting.” Wait within documented bounds, report useful progress, and read final output only after a terminal state. Never invent a wait API.

If output is unreadable, preserve the original files and read them through a documented UTF-8-capable path. Do not re-encode healthy source files. If generation becomes repetitive or incoherent, discard that response as a candidate and resume from the last saved request/state with a short clean context; do not carry the corrupted tail forward or assert an unproven cause. Direct STOP ends actions immediately. Direct NEXT changes the active assignment without continuing repairs to the previous one. A real tool or access block must be reported precisely, never disguised as completion.
