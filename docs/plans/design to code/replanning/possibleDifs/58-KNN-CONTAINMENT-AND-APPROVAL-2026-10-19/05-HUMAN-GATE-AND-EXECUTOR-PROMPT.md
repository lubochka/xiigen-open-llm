# N58-05 — точный стартовый пакет и stop-before-training для будущего исполнителя

**ЗАПРЕТ В ЭТОЙ СЕССИИ:** DO_NOT_EXECUTE; no code changes, no proxy/native tests, no model runs, no training, no new checkpoints. Следующий текст — вложенный проект промпта, а не автоматически выданное задание. Решение Любы по **этой версии**: PENDING.

## Стартовый промпт исполнителю — полный, ещё НЕ активированный

> Ты являешься CODEX research executor для XIIGEN. **Не начинай работу автоматически после чтения этого промпта.** Сначала получи от Любы явное APPROVE для точной карточки N58-EXP-K1, N58-EXP-K2 либо N58-EXP-K3 (в 06), SHA файла, frozen data и model/executor ID. Если APPROVE отсутствует: верни только «PENDING_LUBA_REVIEW; NO_RUN» и перечень отсутствующих сведений.
>
> Target (WRITE only after approval, if explicitly authorized): `lubochka/xiigen-planning` branch `llm_plan3` folder `.../replanning/possibleDifs/58-KNN-CONTAINMENT-AND-APPROVAL-2026-10-19` for reports. **READ ONLY ALWAYS:** `lubochka/llm_mvp_core/docs/plans/replanning-possible-diffs/**`. На стадии «только audit» core implementation тоже READ ONLY; будущий bounded harness/temporary worktree разрешается только отдельно.
>
> Frozen evidence: current core SHA eb05219c27663835ce927c7bf97cf1274c07cddb, source `27.TranslationDomain/TranslationTrainedUnits.cs` blob `1742abfc9fb617dfc66e4d618e0c169da01a6ccc`, `TranslationMemorySourceLaw.cs` and `TranslationDirectionAdapters.cs`, historical P125 report/reviewer, prior P16/P54/P55/P56/P57/P60/P93/P107/P117/P125. Before any execution re-fetch branch HEAD, verify source blobs, model asset/checkpoint initial identity and compare donor assumptions. New HEAD means recertify or pause for owner.
>
> Skills: read full latest `docs/skills/intent-to-plan-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `root-cause-tracing-SKILL.md`, `learning-path-audit-SKILL.md`, `experiment-design-SKILL.md`, `claim-verification-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md`, exact AGENTS/CLAUDE at captured SHA. In report state skills load result and SHA. Separation: learning topic=translation, profile=ENRU imperatives, learner=TranslationMachine.ExecutorUnit memory, mechanism=kNN, metric=user-level semantic correctness and accepted error; **not a neural DPO/LoRA experiment**.
>
> Proposed executor profile: requested `gpt-6-astra / ultra` (do not claim real model without attestation), independent reviewer separate session (reviewer identity PENDING); learner model `TranslationMachine` EN_RU, `executor-en_ru`, no neural weights, no LoRA. Initial fresh A/B states must show identical pre-lesson memory hashes. Dataset and exact text are inside approved N58-03: **K1** two full multi-action TRAIN and four single-action EXAM; **K2** two negative-imperative TRAIN and four polarity contrasting EXAM. Frozen expected semantic outputs and alternate valid Russian surface forms agreed before any learning.
>
> K1/K2 являются PRIOR ART N27/N37; это не новая находка. Для K3 используй СТРОГО отдельный 06 и closed-predicate comparator; не применяй вместе с K1/K2. For selected K1, make ONLY score criterion baseline query containment vs proposed Jaccard; for selected K2, make ONLY gate on sign of recognized imperative; don't combine. Both arms same two lessons, k5, signature, tokenizer, source, reference, seed, exact/phrase/word maps, same domain and grader. Four states A0/A1/B0/B1. First verify actual selected kNN route with all caller guards; if not reached return NOT_REACHED and do not claim an effect. Native consumer `GetOrchestratorAsync().RunAsync` produces candidate, scorer only after saved outputs, NO use of gold to build answer. Compare exact output, model state, loss/gain per case, accepted wrong count, coverage, correct/answered, wall time, real CPU/RAM and actual physical call counts. Save raw evidence and independent review; no retuning on EXAM.
>
> Stop immediately when source blob changed, protected file would be touched, source comparator cannot be reached, TRAIN/EXAM overlap occurs, exact requested executor model cannot be attested, truth label is questionable, or reviewer/owner approval not established; report exact reason and next decision. Report successful *guard* benefit separately from memory learning and separately from neural Student. Never claim restoration of general translator from this four-case panel.

## Разделение ролей и review

1. **Человеческое решение** (в этой задаче ещё нет): Люба читает 03/K1 или 03/K2, exact model/seed, skills, outputs, бюджет и expected tradeoffs; подтверждает **каждую карточку отдельно**.
2. **Executor / code examination**: после авторизации сначала подтвердить reachability без вмешательства; затем bounded isolated A/B (если разрешено). Указать реальный model ID, sub-agent packet if used, commit/worktree SHA.
3. **Independent reviewer**: проверить именно кандидата/commit, не чужой summary. Проверить TRAIN and EXAM disjoint, selected route and gold purity, before/after raw output ledger и semantic rubric на все 4 cases, no extra actions, negation; hashes, sampled resource units, cold equality. PASS привязан только к exact reviewed candidate SHA.
4. **Owner gate for further change**: независимо от красивого результата 2×4 вопросов, никто не меняет main product, P01–P125 protected или архитектуру без отдельного решения. Доменный перенос на C#/SQL/Hebrew/Spanish допускается только с собственными доступными consumer/feature/parser и отдельным опытом.

## Контрольные вопросы Любы перед одобрением

- Обязательно ли исправлять эту цель именно сейчас, если historical 19/24 achieved by phrase-memory и нейронная B0 всё ещё 0/4? (Нет по умолчанию; этот опыт изолирует kNN source-selector, не neural.)
- Доказано ли, что выбранный корпус попадает именно в ordinary ENRU kNN, а не exact/phrase/wordmap? (Пока НЕТ; обязательный preflight.)
- Действительно ли разница K1/Jaccard и K2/polarity управляется единственным фактором и не тестовым gold? (Да в **проекте**, но необходим code/reviewer readback до RUN.)
- Разрешены ли 2 урока/4 независимых события для вывода "улучшилось"? (Для различающего дешёвого пилота; устойчивое domain learning и новые независимые families требуют другого sealed опыта.)
- Все ли реальные models/prompts/skills/artifacts и ожидаемые результаты указаны прямо здесь? (Да: requested executor profile and learner class, full startup, TRAIN/EXAM, conditional expected outcome; реальные runtime attestations/initial hashes ещё PENDING, не сфабрикованы.)

**Duplicate-screen/human review update:** первоначальный K1/K2 перечень был обнаружен также в N27/N37. До запуска обязательно прочитать старые карточки, исключить тождественный уже разрешённый опыт и отдать приоритет отдельной K3 карточке [06](06-NEW-PREDICATE-COMPLEMENT-LAW-AND-SMALL-EXPERIMENT.md), если она вообще важна по product reachability. Не сообщать K1/K2 как новые находки.
