# Row23 recovery scenario prep: approved input path gate

claim_scope: Row23 recovery scenario prep only; this prepares the next blocker-repair step and does not run product, runtime, UI, tests, eval, heldout, adapter/core, or host actions.
evidence_scope: Current Luba work order plus existing Row23 blocker/intake/board/action-plan evidence inspected in this turn. The evidence supports only: Row23 is blocked because no approved safe Row23 input/path exists.
count_claimed: prep_artifacts_created=2; product_runtime_ui_actions=0; semantic_quality_runs=0; heldout_passes=0; product_readiness_full_pass=0; formal_joined_pass=0; approved_input_paths_found_by_prior_intake=0.
independent_evidence_count: 0 new independent semantic/product evidence records created by this prep.
subagent_id_or_NONE: bounded Row23 recovery scenario prep sub-agent
downgrade_needed: yes for any semantic-quality, product-readiness, token-savings, checkpoint-promotion, locator-update, or Row24-readiness claim.

## Что Row23 пытается доказать

Row23 (`frdp-p01-codex-semantic-quality-heldout`) в полном будущем варианте должен доказать, что Codex semantic-quality heldout scenario проверен на безопасном, заранее одобренном входе: есть provenance, leakage controls, rubric/scoring или accepted comparison criteria, baseline/candidate or before/after comparison, and, if host/product readiness is claimed, host-visible Codex answer evidence for the same scenario.

Сейчас Row23 этого не доказал. Текущий узкий статус: `ROW23_SCOPED_BLOCKER_ACCEPTED / BLOCKED_NO_APPROVED_INPUT_PATH / blocked_not_proven`. До появления ровно одного approved safe Row23 input/path Row23 остается blocker/status artifact, не semantic-quality proof.

## Разрешенный input/source path

Разрешенный heldout/eval input path на момент этого prep: `NONE_APPROVED_YET`.

Для следующего узкого intake шага разрешенный источник проверки только такой:

- текущая нецитированная инструкция Luba или bounded work order, если она явно supply/approve ровно один safe Row23 input/path;
- уже существующие Row23 packets как evidence-only context, не как новое разрешение;
- board/action plan только для текущих счетчиков и claim locks.

Чтобы input/path стал допустимым для будущего Row23 evidence collection, он должен быть указан явно и содержать:

- provenance: откуда взят вход, почему он не сгенерирован скрытым eval/training путем;
- approval quote: свежая текущая Luba approval quote или bounded work-order authority;
- category: one of `preexisting_or_separately_approved_heldout_input`, `accepted_same_scenario_before_after_input`, `accepted_host_visible_evidence_path`, or `other_exact_safe_evidence_path`;
- evidence refs: ссылки на подтверждающие artifacts, если они уже существуют;
- forbidden actions: no training, no DPO generation, no checkpoint mutation/promotion, no locator mutation/update, no product/source edits, no hidden/external AI, no host UI automation unless separately authorized;
- claim locks: `semantic_quality_status=blocked_not_proven`, `product_readiness_claim=false`, `formal_joined_pass=false` until later accepted evidence proves a narrower claim.

## Что запрещено и почему

Запрещено брать input/path из старого quoted transcript, старого плана, старого assistant answer, generated fixture, runner-contract fixture, smoke result, board row, request packet, or blocker packet. Эти материалы могут объяснять статус, но не создают current approved heldout input/path.

Запрещено считать runner-contract/readback smoke semantic-quality proof. Этот smoke может подтверждать только форму локального no-send runner contract/readback artifact; он не доказывает качество ответа, heldout pass, host-visible answer, product readiness, token savings, checkpoint promotion, or locator update.

Запрещено начинать Row24 до Row23 approved input/path gate and any later authorized Row23 evidence step. Иначе Row24 стартовал бы поверх blocker, а не поверх accepted Row23 pass.

## Автоматический путь A: что будет проверять, если потом появится approved input

Этот prep ничего не запускает. Если позже будет отдельная authorization на Row23 evidence collection, автоматический путь A может проверять только pre-host/core boundary:

1. Read the one approved Row23 input/path from the current authority packet.
2. Verify provenance, approval quote, category, evidence refs, forbidden actions, and claim locks.
3. Build or invoke only the separately authorized no-send/controlled boundary runner for the same scenario.
4. Record request/response contract, prompt hash or input identity, rubric/comparison metadata, and non-claim locks.
5. Stop at host/core boundary unless the later work order separately authorizes UI/host actions.

Path A must not claim hidden deterministic algorithm quality, token savings, product readiness, checkpoint promotion, locator update, training, or learnability. Any deterministic fixture or runner-contract result is contract evidence only.

## Локальный UI/agent путь B: что проверял бы, если approved UI input exists

Этот prep не выполняет UI. Если позже Luba separately approves UI input and UI execution, Path B would test the same approved scenario through the real local Codex UI/agent path:

1. Auth/session preflight with no secrets captured.
2. Confirm the correct local Codex window/thread target and no overlapping-window blind spot.
3. Insert exactly the approved Row23 input, not a paraphrase.
4. Capture before-submit evidence.
5. Submit only if the future work order explicitly authorizes send/submit.
6. Capture visible response evidence and redacted screenshots/logs.
7. Compare Path B visible result to Path A same-scenario identity and rubric/comparison criteria.

Without separately approved UI input and exact clicks/typing/expected visible state, Path B status remains `BLOCKED_UI_INPUT_NOT_APPROVED`, not product readiness.

## PASS / BLOCKED / NEEDS_FIX

PASS for this prep means:

- prep is human-readable Russian and states the Row23 purpose, current missing input, forbidden sources, Path A/Path B boundaries, and claim locks;
- no product/runtime/UI action was performed;
- no hidden deterministic algorithm claim is made;
- no product readiness claim is made;
- the next allowed work order is narrow enough for 10 minutes and remains intake/status only.

BLOCKED for a future intake step means:

- no current unquoted Luba instruction or bounded work order supplies exactly one approved safe Row23 input/path;
- the supplied candidate is quoted/stale/evidence-only;
- more than one candidate is supplied without choosing exactly one;
- provenance, approval quote, category, evidence refs, forbidden actions, or claim locks are missing;
- the candidate requires product/runtime/UI/tests/eval/training/mutation/git actions outside the work order.

NEEDS_FIX means:

- this prep smuggles product/runtime/UI execution;
- this prep claims semantic-quality improvement, heldout pass, product readiness, token savings, checkpoint promotion, locator update, or Row24 readiness;
- this prep is machine-code-only rather than human-readable;
- this prep fails to name `NONE_APPROVED_YET` as the current allowed heldout/eval input path.

## Overclaim firewall

This prep avoids overclaiming by keeping these locks explicit:

- `semantic_quality_status=blocked_not_proven`
- `product_readiness_claim=false`
- `formal_joined_pass=false`
- `approved_input_paths_found_by_prior_intake=0`
- `boundary/API evidence != host readiness`
- `marker-only != full local desktop pass`
- `trace != savings`
- `schema != training`
- `checkpoint candidate != promotion`

No token savings are claimed. No semantic-quality improvement is claimed. No product readiness is claimed. No checkpoint promotion is claimed. No locator update is claimed.

## Exact next sub-agent execution work order if this prep passes

Bounded Row23 input-path intake sub-agent, maximum active time 10 minutes:

Read this prep plus the current unquoted Luba instruction/bounded work order. Do not run product/runtime/UI/tests/eval/heldout/adapter/core. Do not edit code, tools, AGENTS, CLAUDE, skills, or unrelated evidence. Create only a fresh Row23 subfolder under `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-semantic-quality-heldout/` with `row23-approved-input-path-intake.md` and `row23-approved-input-path-intake.json`. Decide whether the current authority supplies exactly one approved safe Row23 input/path with provenance, approval quote, category, evidence refs, forbidden actions, and claim locks. If yes, record `READY_FOR_SEPARATE_ROW23_EVIDENCE_WORK_ORDER` without running it. If no, record `BLOCKED_NO_APPROVED_INPUT_PATH`. Preserve `semantic_quality_status=blocked_not_proven`, `product_readiness_claim=false`, `formal_joined_pass=false`, and no Row24 advance. No commit/push.

## Prep verdict

PASS for prep artifact authoring only.
