# Output Readability Gate

## Обязательный словарь человеческой коммуникации

При загрузке этого навыка читать [Словарь человеческой коммуникации](human-communication-GLOSSARY.md). Он задаёт значения терминов, в том числе: домен — предметная тема обучения единицы. В объяснениях и таблицах раздельно указывать тему, задачу/язык/направление, получателя обучения, механизм и измерения. Направления перевода, алгоритмы, классы и профили проверок не считать доменами автоматически. Определение не задаёт соответствие «один домен — одна единица» или отдельную модель для каждой темы.

## Luba Why / Loophole Repair-First Rule

When Luba asks "why", "почему", challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Fresh Execution / Method / Humility Readability Hook

Human-readable output must make these gates visible:
- `REPEATED_CURRENT_COMMAND_IS_FRESH_EXECUTION_GATE`: repeated current command is a fresh execution trigger; old artifacts are baseline evidence, not completion.
- `TOO_FAST_HIGH_PASS_SUSPICION_GATE`: too-fast 50-pass completion is suspicious until per-pass sub-agent evidence is accepted.
- `LARGE_VOLUME_MODEL_HUMILITY_AND_FOCUS_GATE`: large-volume model humility requires bounded slices and focus ledger before completion.
- `EXPLICIT_METHOD_OVERRIDE_GATE`: Luba's method/model/reasoning/count/folders/target/sub-agent route is binding; output must not hide a method gap behind content evidence.
- `FINAL_ANSWER_FRESHNESS_GATE`: final/status must plainly say freshly executed this turn vs only verified and answer the latest command.
- `CURRENT_REPEATED_PACKAGE_COMMAND_GOVERNANCE_ADDENDUM`: readable output must show repeated extension-development package work as a fresh execution chain, not old artifact verification; reject too-fast N-pass/status-package claims without ordered packets/records; show large-volume model humility through a focus ledger with exact current task, not-the-task, files, pass/review counts, active worker, remaining actions, and latest-command freshness; state one active worker at a time when Luba flags parallel/too-fast or exact-count sub-agent work unless her current unquoted message authorizes parallelism; flag interrupted or zero-work spawned-worker packets using `NONE` instead of `subagent_id: UNKNOWN_IN_SUBAGENT` when platform id is hidden; preserve the parent local sidecar work ban; put human-readable phase counts before execution continuation; and keep final/status freshness tied to latest command, fresh-vs-old execution, actual packets/records, no parent self-review counted, and no product-readiness overclaim.
- `COMMAND_SCOPE_EXACTNESS_GATE`: readable output must flag current-task command violations. Current `allowed_commands` and `forbidden_commands` are binding; read-only convenience commands, alternate git diff modes, external AI/web, `codex.exe`, nested sub-agents, branch/git mutations, or shell variants outside the current allowed list are violations unless Luba's current unquoted instruction explicitly authorizes them. Evidence from an out-of-scope command is quarantine evidence only and cannot support completion.

## Sub-Agent Work-Order Completeness Gate — central two-layer contract

This file is the concise owner for work-order readability and acceptance.
AGENTS.md, CLAUDE.md, SESSION-START-PROMPT.md, and specialist skills should
point here instead of carrying autonomous copies of the same contract.

Completeness is judged by meaning, not by the number of fields. A prompt that
contains `good_examples`, `bad_examples`, or `return_packet_schema` is still
invalid if the worker cannot see the behavior being checked, the current state,
the target state, one understandable success example, one understandable failure
example, and the exact evidence needed for acceptance.

Layer A — visible product result. The work order must include a ready example of
the expected human form in Russian or the operator's language. It states what
the product is trying to do, why the order matters, what should happen first,
what should happen after that, and what outcome would count as accepted.

Illustrative form for training-adjacent work, not a measured fact:

> Модель учится разбивать просьбу программиста на план действий. Мы загружаем
> её сохранённое состояние и проверяем ответы на отдельных заданиях. Если она
> забывает проверку результата, китайская модель получает отдельную учебную
> задачу и прежние удачные пары как примеры. Новую пару сохраняем, затем
> дообучаем модель. Проверяющий сравнивает ответы до и после. Улучшение
> сохраняем сразу; при ухудшении возвращаем состояние до этой пары.

Layer B — technical sources and internal controls. The same work order still
names exact scope, skills, source files, current-vs-target marking, allowed and
forbidden files, allowed and forbidden commands, verification, honesty fields,
role boundary, and git authority. Internal packet fields may use exact
identifiers, but they are evidence controls, not the style of the user-visible
document. Technical facts, code names, low-level component terms, paths, hashes,
and raw status values belong after the human explanation or in a separate
technical reference table.

Parent acceptance checks real output bytes, not only the filled field names:
before a large rewrite after a readability failure, the author writes one
compact section plus one diagram branch/example in the target live document.
The parent reads those bytes and checks them against the source facts: what is
being explained, why it matters, input, decision, outcome, current behavior
versus target behavior, and the honest boundary. Only after the parent explicitly
allows continuation do the remaining sections proceed. No extra marker, ledger,
or question to Luba is required. A new author using the same defective prompt is
not a repair.

## Strict Plan Readiness / Original Task Simulation / Future Gate / Historical Status Gate

PLAN_READY means zero unresolved design/dependency blockers and complete coverage of the original task. The plan must be mentally simulated as if implemented and must cover every original-task requirement. Future Gate B may contain execution actions such as tests/builds/screenshots/mergeability checks. Future Gate B may NOT hide unchosen legal provider, missing input/output contracts, missing dual-path design, unresolved repo ownership, missing external API seam design, missing provenance policy, missing thresholds, missing model/checkpoint lifecycle, or missing source-to-plan mapping. If a future gate hides a design/dependency decision, verdict is PLAN_NOT_READY.

30 sequential simulation/review passes are required for this readiness class: one pass at a time, no parallelism. Each pass records `requested_model: gpt-5.5`, `requested_reasoning_effort: xhigh`, skill focus, original-task requirement checked, plan section checked, mental simulation step, coverage verdict, finding, repair/no-fix reason, and recheck result. A single worker doing 30 internal passes must call them sequential simulation/review passes, not 30 sub-agents or 30 independent sub-agent passes.

Future gates are execution-continuation checkpoints, not stop permissions. After Luba approves a truly ready plan, gates are executed through bounded sub-agents until the product is complete. If a gate fails, the next bounded sub-agent fixes/mitigates or records a true external blocker. Do not stop/finalize while safe bounded work remains.

Any historical/pre-repair/evidence-only `NOT_READY`, `BLOCKED`, `FAILED`, `READY`, `DONE`, or similar status retained as evidence must be labelled historical/pre-repair/evidence-only, with the current verdict nearby. If a current artifact still makes Luba ask whether stale `NOT_READY` remains, the repair is incomplete.

Good future gate: `Run dotnet tests after implementation; if red, bounded fix worker repairs failing slice.` This is execution, not a design decision. Bad future gate: `Choose legal provider during implementation` or `figure out FLOW-38 input contract later`; these are design blockers, not execution gates. Good historical status: `Attachment 429f reported the pre-repair plan as NOT_READY; current verdict after repair is PLAN_NOT_READY because provider remains unchosen.` Bad historical status: leaving `NOT_READY` in a source map without saying historical/pre-repair and without current verdict next to it.

## Strict Plan Readiness / Future Gate / Historical Status Gate

PLAN_READY means zero unresolved design/dependency blockers. Design/dependency
decisions must be solved inside the plan before implementation. Future Gate B
may contain execution actions such as tests/builds/screenshots/mergeability
checks. Future Gate B may NOT hide unchosen legal provider, missing input/output
contracts, missing dual-path design, unresolved repo ownership, missing external
API seam design, missing provenance policy, missing thresholds, or missing
model/checkpoint lifecycle. If a future gate hides a design/dependency decision,
verdict is PLAN_NOT_READY, not PLAN_READY_FOR_LUBA_REVIEW.

Future gates are execution-continuation checkpoints, not stop permissions. After
Luba approves a truly ready plan, gates are executed through bounded sub-agents
until the product is complete. If a gate fails, the next bounded sub-agent
fixes/mitigates or records a true external blocker. Do not stop/finalize while
safe bounded work remains.

Any historical/pre-repair/evidence-only `NOT_READY`, `BLOCKED`, `FAILED`,
`READY`, `DONE`, or similar status retained as evidence must be labelled
historical/pre-repair/evidence-only, with the current verdict nearby. If a
current artifact still makes Luba ask whether stale `NOT_READY` remains, the
repair is incomplete.

Good future gate: `Run dotnet tests after implementation; if red, bounded fix
worker repairs failing slice.` This is execution, not a design decision. Bad
future gate: `Choose legal provider during implementation` or `figure out
FLOW-38 input contract later`; these are design blockers, not execution gates.
Good historical status: `Attachment 429f reported the pre-repair plan as
NOT_READY; current verdict after repair is PLAN_NOT_READY because provider
remains unchosen.` Bad historical status: leaving `NOT_READY` in a source map
without saying historical/pre-repair and without current verdict next to it.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.

## Core Problem

Generated output that contains shortcodes, status strings, or code-style
identifiers written as plain prose is unreadable to a human reviewer. The
same token that belongs in exact syntax — `CF-12`, `CONTEXT_INSUFFICIENT`,
`Rule 1` — becomes noise when it replaces a human explanation. This skill
defines the prohibited patterns, the syntax-only exception for code/table
snippets, and the gate check that must pass before any generated output is
accepted.

---

## When to Invoke

- Before accepting any output from a generation node (ai-generate, multi-generate)
- When reviewing context package prose and explanatory comments, diagrams, or
  tables; exact code syntax itself is not rewritten for readability
- When writing plan documents, skill files, or decision records
- When a human reviewer reports that output is "hard to follow" or "reads like code"
- When writing project documentation, dashboard/API docs, API catalogs, or user-facing
  examples.
- When a work order, diagram, table, or code-fence example mixes exact technical
  syntax with human explanations. Exact code/API/path identifiers may remain
  unchanged; comments, Mermaid/PlantUML labels, notes, legends, branch labels,
  expected-result cells, and quoted "good" examples still need human language.

---

## Section 1 — The Core Rule

No shortcode, status string, or code-style identifier may replace the human
meaning in prose. Exact syntax is allowed where it is truly syntax: code,
API names, paths, class/method names, status values, inline backticks, and
technical reference tables shown after the explanation.

The code-fence/table exception is only for syntax. It does not exempt human
explanations inside a fence or table. Comments, Mermaid/PlantUML labels,
`note for` text, legends, decision branches, expected-result cells, and quoted
"good examples" must still be readable in Russian or the operator's language.

This is not a ban on Latin identifiers in code. It is a ban on treating raw
identifiers as the explanation.

---

## Product-Level Human Readability Acceptance Gate

This skill is the concise owner for product-level readability acceptance.
Architecture, plan-review, generated-code-review, status-template, AGENTS.md,
and CLAUDE.md rules should reference this gate instead of duplicating a new
900-line reporting layer.

For any code slice, product-status packet, acceptance review, commit/push
report, or training-adjacent artifact, the visible message to Luba must first
explain what exists and what changed in product terms. A readable packet cannot
lead with what is absent, with counts only, or with a raw machine schema.

Required visible content for a product/code/training-adjacent slice, phrased
for Luba in Russian or the current operator language:

1. Что пользователь сможет сделать теперь: какое поведение изменилось и один
   конкретный пример входа/выхода или результата сценария.
2. Как работает алгоритм без кода: зачем эта логика нужна продукту, что
   происходит и в каком порядке.
3. Что не путается с обучением: отдельно назвать проверку, сохранение и
   повторное открытие данных; отдельно — изменение обученного состояния; отдельно
   — настоящий запуск обучения с метриками. Сохранение, паспорт, имя контрольной
   точки, поиск ближайшего примера и валидация не называются обучением без
   фактического обновления обученного состояния и измерений.
4. Что ожидалось и что фактически проверено: успешный случай, ошибочный или
   отрицательный случай, случай сохранения/повторного открытия, когда он
   относится к изменению, и честная граница того, что тесты не доказали.
5. Если менялись архитектура, поток данных или ветки решения, объяснение
   связывается с живой UML/диаграммой или краткой симуляцией веток с
   человеческими названиями. Это требование для принятия изменённой архитектуры
   или потока, а не для каждого короткого heartbeat-а.
6. Что осталось до первого запуска обучения: самый маленький упорядоченный
   список из текущего кода или текущего принятого плана. Отдельно указать, что
   осталось до заявлений о метриках, повторном использовании и готовности.
   Проценты, сроки и "весь проект закрыт" не выдумываются.
7. Что уже опубликовано и что ещё только локально или ждёт публикации. Если
   исторический commit body был непонятным, добавить живое объяснение в
   следующем отчёте или следующем разрешённом commit body; опубликованную историю
   не переписывать.
8. Идентификаторы evidence, hash, FQN, TRX, номера строк и raw packet fields —
   только вторичные ссылки после человеческого объяснения или во внутреннем
   пакете.

Ordinary heartbeat/progress commentary may use a shorter form, but it still
starts with product purpose, what changed/currently runs, and the next useful
action. Counts appear only when the meaningful product steps are known. Use one
compact honesty boundary; do not hide real blockers, but do not make the visible
message a denial-heavy wall about everything absent.

Требуемый образец смысла для будущего отчёта об обучении; это не доказательство,
что цикл уже реализован или пройден: продукт — разбор программистской задачи на
план. Сначала загружается существующая модель с весами. Затем отдельная
модель-проверяющий оценивает её ответы на закрытых заданиях и находит
конкретную ошибку. Открытая китайская модель получает не экзамен, а отдельную
учебную задачу, слабый ответ, внешний эталон только как ориентир и старые
сохранённые пары как хорошие/плохие примеры. Она создаёт новую или исправленную
пару; эту пару сохраняют с историей до обучения. Затем уже загруженную модель
дообучают только новой парой, старые пары в промпте не означают повторное
обновление весов на них. Проверяющий снова оценивает ответы. Если улучшение
ясное с учётом погрешности — новые веса сохраняются сразу. Если качество
ухудшилось — возвращается полное состояние до последней пары и запрашивается
исправленная пара. Если результат равный или неопределённый, улучшение не
объявляется. Unit-тесты могут доказывать хранение, загрузку и технический
откат, но не качество обучения.

Плохой отчёт: "19 fake-case checks green; sealed checkpoint publication; next
slice." Он проваливается, потому что читатель не видит поведение продукта,
порядок обучения, ожидание против фактической проверки, границу между
unit-тестами и оценкой качества, а также ближайший оставшийся шаг. Хорошая
условная формулировка после реального выполнения такого цикла: "Загружены
предыдущие веса модели разбора задач. Модель-проверяющий нашла ошибку на
отдельной задаче; открытая китайская модель получила старые DPO-пары только как
примеры хорошего/плохого ответа, без экзаменационных задач, и предложила
исправленную новую DPO-пару; новая пара сохранена с историей до обучения;
существующие веса были дообучены только этой новой парой; затем модель-
проверяющий повторно оценила качество. Ожидалось: ошибка уйдёт без ухудшения
на контрольных задачах. Фактически: <результат оценки>. Если качество хуже,
следующее действие — полный технический откат к состоянию до последней пары и новая
исправленная пара. Если такой оценки не было, отчёт обязан сказать: обучение и
качество ещё не проверены."

Смысловая проба перед большой переработкой: после замечания Лубы о
нечитабельности или перед переписыванием большого документа сначала пишется один
настоящий компактный фрагмент в целевом живом документе: короткое описание и
одна ветка/пример результата. Родитель читает этот фрагмент вместе с исходными
техническими фактами и проверяет, что человек понимает что, зачем, порядок,
ожидание и текущую границу. После такого родительского согласования продолжают
остальные файлы; одобрение Лубы на каждый фрагмент не требуется. Автор правила
не является независимым подтверждением эффекта, а поиск маркеров не равен
проверке смысла.

---

## Section 2 — Four Prohibited Patterns

### Pattern 1 — LETTER(S)-DASH-NUMBER(S) identifiers in prose

```
Prohibited: CF-12, DNA-5, OP-7, BUG-ENGINE-001, D-01, LEARNING-001
Reason: Shortcode syntax belongs in reference tables and code, not sentences.
Allowed form: "Capability Framework Rule 12 (CF-12)" on first use; "CF-12" after.
              Or: write out the rule in full and cite the identifier in parentheses.
```

### Pattern 2 — Bare letter+number tokens in prose

```
Prohibited: R1, G3, Q2, T4, E1 (when used as identifier shortcuts in prose)
Reason: Ambiguous without expansion — R1 could mean Rule 1, Requirement 1,
        or Round 1 depending on context.
Allowed form: write the full term first: "Round 1 (R1)" or "Rule 1" without abbreviation.
```

### Pattern 3 — ALL_CAPS_UNDERSCORE status strings in prose sentences

```
Prohibited: "The pipeline returned CONTEXT_INSUFFICIENT so we escalated."
            "When the result is DEFERRED_CONSTRAINT the session may close."
Reason: Status strings written as prose identifiers are not English sentences.
Allowed form: "The pipeline returned a context-insufficient result, so we escalated."
              Or: use a code fence to reference the status value.
              Or: write: the `CONTEXT_INSUFFICIENT` signal (inline backtick is allowed).
```

### Pattern 4 — Version suffix codes in prose

```
Prohibited: "The fix was applied in v23.0+hotfix-2 of the context package."
Reason: Version identifiers belong in changelogs and code metadata, not description prose.
Allowed form: "The fix was applied in version 23.0 (hotfix 2) of the context package."
```

### Pattern 5 — Bare internal identifier in owner-facing text (fifth, added 23.08.26; `NO_BARE_IDENTIFIER_RULE`)

(Русский канон — первым; EN rendering — ниже. Источник: поправка Лубы 23.08.26:
«Ссылки: W-203, W-169. Опять тут нет никаких деталей что именно такое 169,
203!!!! … Надо немедленно исправить скилы человеческого отчета!!!». Этот
паттерн СТРОЖЕ Pattern 1: для номеров строк работ, разрывов, решений и гейтов
(W-NNN, G-NN, D-NN, S-NN, №NN, P0..P6, QSG) в тексте, адресованном Лубе или
другому человеку, послабление «после первого употребления можно голый номер»
НЕ действует — номер каждый раз идёт в скобках после человеческого названия.)

**Внутренний номер никогда не стоит отдельно.** Любой внутренний идентификатор
(номер строки работ W-NNN, разрыва G-NN, решения D-NN, сценария S-NN, пункта
№NN, фазы P0..P6, гейта QSG, hash коммита, полное имя теста FQN, файл
результатов TRX, имя класса или гейта) в статусе, heartbeat-е, коммит-сообщении,
строке «следующее действие» или таблице для Лубы допустим ТОЛЬКО в скобках
сразу после полного человеческого названия и цели. Голый номер, список
номеров, строка «Ссылки: …», стрелочная цепочка номеров, заголовок/ячейка/
subject коммита из одного номера — нарушение `bare_identifier_to_owner`
(класс `machine_report_to_owner`), score_delta -500 для исполнителя, рецензента
и родителя-архитектора одинаково; повторное после замечания Лубы — -1000.
Строка шаблона «Ссылки» заменена на «Где это записано: <человеческое
название> (номер N в файле <название файла>, секция …)». В планирующих
файлах номер допустим в колонке «id» таблицы, если в той же строке есть
человеческое название; в прозе планов — то же правило.

```
Prohibited (Luba's quotes, 23.08.26):
  "Ссылки: W-203, W-169"
  "W-203 полный прогон на HEAD → ре-ревью 2 подозрительных коммитов (W-169)
   → W-197/W-200/W-201 гигиена → первый слайс W-198 (комментарии в 11.DPO)
   + W-199.1 (ARCHITECTURE.md)"
Reason: the reader cannot tell what 203 or 169 IS without opening the plan;
        a number after the human text is still a bare number.
Allowed form:
  "Дальше исполнитель: (1) прогоняет все тесты проекта на опубликованном
   коде, чтобы увидеть реальные поломки (строка работ 203); (2) заново
   проверяет два коммита, где ожидания тестов могли подогнать под код вместо
   починки кода (строка 169; коммиты 1b1b091907, aa0d151532); (3) убирает
   мусор из рабочего каталога: перестаёт отслеживать папку результатов
   тестов, чистит временную папку тестов, удаляет временную рабочую копию
   (строки 197, 200, 201); (4) начинает первый кусок работы по комментариям
   в коде — модуль обучения предпочтений (строка 198); (5) дополняет главный
   документ архитектуры (строка 199.1)"
```

**Проверка перед отправкой («тест незнакомого читателя»)**: прочитать свой
текст глазами человека, который не видел проекта и не знает ни одного номера;
каждое место, где нужно знать номер, чтобы понять смысл, — переписать.
Автопроверка поддерживает чтение, но не заменяет его: искать нужно в видимом
тексте отчёта и в пояснениях внутри fenced blocks, таблиц и диаграмм. Точные
идентификаторы, являющиеся синтаксисом, не переписываются; комментарии,
подписи, expected-result ячейки и quoted good examples проверяются на смысл.

```
\bW-[0-9]+\b|\bG-[0-9]+\b|\bD-[0-9]+\b|\bS-[0-9]+\b|\bQSG[0-9]*\b
```

Каждое совпадение должно стоять внутри скобок сразу после человеческого
названия; иначе отчёт переписывается до отправки. Ожидаемый результат для
отчёта Лубе: голых совпадений = 0.

(EN rendering:) An internal identifier never stands alone in owner-facing
text. Every internal number or code (W-NNN, G-NN, D-NN, S-NN, №NN, P0..P6,
QSG, TRX, FQN, hash, class or gate name) in a status, heartbeat, commit
message, "next action" line, or owner-facing table may appear ONLY in
parentheses immediately after the full human name and purpose — every time,
not only on first use. A bare id, an id list, a "References:" line, an arrow
chain of ids, or a heading/cell/commit subject made of an id is
`bare_identifier_to_owner` (-500 for executor, reviewer and parent alike;
-1000 on repeat after Luba's remark). The "References" template line is
replaced by "Where this is recorded: <human name> (number N in file <human
file name>, section …)". Before sending: stranger-reader test plus the grep
autocheck above; bare matches must be 0.

---

## Section 3 — .NET Project Equivalents

The same four patterns apply specifically to this project's identifiers:

```
Ban CF-N alone in prose sentences
  → Write "Capability Framework Rule N" or use inline backtick `CF-N`

Ban "Rule N" alone without a subject
  → Write "CLAUDE.md Rule N" or "DNA Rule N (no external ML frameworks)"

Ban CLAUDE.md rule shortcodes as prose adjectives
  → Not: "This is a Rule 1 violation in the return type."
  → Yes: "This violates Rule 1 (OperationResult<T> everywhere) — the method returns void."

Ban OperationResult status strings as prose
  → Not: "The method returned Fail with MISSING_CAPABILITY."
  → Yes: "The method returned a failure result with error code `MISSING_CAPABILITY`."

Ban pipeline signal type names as prose descriptions
  → Not: "The feedback node emits OUTCOME and DPO_TRIPLE."
  → Yes: "The feedback node emits an outcome signal and a DPO (Direct Preference Optimization) triple."
         (or use code fence / inline backtick for signal type names)
```

---

## Section 4 — Gate Check

Before accepting any prose output from a generation node:

```
□ No LETTER(S)-DASH-NUMBER(S) identifiers replace the explanation in prose;
  exact syntax may remain in code/backticks/tables only as a secondary reference
□ No bare letter+number tokens (R1, G3, Q2) used as identifier shortcuts in prose
□ No ALL_CAPS_UNDERSCORE status strings written as prose words
□ No version suffix codes in descriptive sentences
□ Bare internal identifiers = 0 in visible explanations (grep
  \bW-[0-9]+\b|\bG-[0-9]+\b|\bD-[0-9]+\b|\bS-[0-9]+\b|\bQSG[0-9]*\b; then read
  context, including comments/labels/cells inside code fences and tables: every
  explanatory match sits in parentheses after a human name)
  AND the stranger-reader test passed (Pattern 5, `NO_BARE_IDENTIFIER_RULE`)
□ Product/code/training-adjacent claims satisfy the Product-Level Human
  Readability Acceptance Gate: поведение + пример, алгоритм без кода, граница
  между проверкой/сохранением и обучением, ожидание против факта, симуляция
  изменённого потока, если поток менялся, оставшаяся работа до первого запуска
  обучения и отдельно до метрик/повторного использования, опубликовано против
  локально/ещё не опубликовано
□ Code-fence/table syntax exception was not used as a readability escape:
  comments, Mermaid/PlantUML notes, legends, branch labels, expected-result
  cells, and quoted "good examples" are human-readable in the operator's language
□ For a post-challenge or large rewrite, the first compact live fragment passed
  the semantic probe; marker search alone did not count as effect verification
```

If any checkbox fails: return the output to the generation node with the
specific pattern identified. Do not rewrite the prose manually — instruct
the generator to rewrite the failing sentence using the allowed form.

## Section 5 — Dashboard And API Documentation Readability

Dashboard/API documentation is accepted only when a non-implementer can answer:
"what API is this?", "what do I type?", "what will I see?", and "what does this
evidence prove?" without reading source code first.

Required for each exposed API method:

```
□ Human name, not only method id
□ One realistic example input
□ Main parameters in plain language
□ Expected output in plain language
□ Evidence boundary: what the result proves and what it does not prove
```

Required for dashboard/project docs:

```
□ UML or flow diagram is accompanied by examples/simulations for the edges
□ No-code algorithms name the actual class and method/function
□ The text explains parameters and outputs before showing code quotations
□ Code quotations are short and tied to a "why this matters" explanation
□ Test evidence is separated by scope: unit, integration, view-model, full UI,
  host runtime, semantic/model readiness
□ UX automation evidence is separated from click-path UI automation: labels,
  accessible names/help text, examples, error wording, raw/debug safety,
  history/copy/export clarity, layout/bounds, and no internal primary-path jargon
□ No mojibake or broken encoding marker names remain in human-facing prose
□ `human-readable Russian text must not contain mojibake`
□ `rendered/readback Russian must be human-readable, not only marker-clean`
□ `mojibake scan must include common UTF-8/Windows-1252 corruption markers and human readback of Russian sections`
□ No code-style status noise replaces a human explanation
```

For Russian sections, the gate fails if marker-name scans find Latin mojibake
bytes, CP1252/UTF-8 artifacts, or U+FFFD replacement characters in changed
active/governance files. Repair and read back the Russian section before
final/status.

For any newly edited human-readable section, a worker cannot claim
`no mojibake` from a blind command, summary, or marker-only scan. The worker
must read back the exact changed lines in terminal/tool output and verify no
U+00D0, U+00D1, U+00C2, U+00E2, U+FFFD, or visibly broken encoding remains in
those lines. If parent readback finds mojibake in the changed section, the
packet is downgraded and recovery is mandatory before accepting the pass.
For current overlay/status paragraphs or current visible table/readback
sections, inherited or adjacent historical text is part of the visible
clean-readback scope unless the packet explicitly excludes it with a tight line
range.

For the API dashboard specifically, this gate must reject any document that:

- lists only technical method ids without human names;
- shows examples without explaining the expected output;
- calls view-model automation "full UI" evidence;
- calls click-path UI automation "UX automation" evidence;
- calls dashboard-to-core invocation "Claude/Cursor/Codex runtime" evidence;
- hides external AI, host runtime, semantic-quality, or trained-model boundaries.

## Section 6 — Architecture Audit Readability

Architecture, trainable, code-review, documentation, and audit artifacts must
also pass the Human-Readable Architecture Artifact Gate:

```
□ Primary explanation is Russian unless Luba asked otherwise
□ Machine labels are secondary, not the explanation
□ UML is PlantUML or Mermaid classDiagram importable to draw.io
□ Data-flow simulations and expected branch outcomes are present
□ Algorithms are explained in no-code human language
□ Code quotes have exact file:line refs and short quotes
□ Every problem quote has an adjacent concrete solution
□ Concrete Repair Blueprint Gate is filled for dictionaries/counters/KNN/scorers/private selectors
□ No table-only, shortcode-heavy, English-only, schema-first, schema-only, or prompt/spec artifact
```

"Create schema", "chosen/rejected/quarantine", "wrap in ILogicalUnit", "add KNN",
or "make typed state" without concrete classes/records/methods/tests/data rows
не является решением. Return the artifact to governance/authoring repair before
accepting it.

When Luba asks for iterative audit readability improvement, the readability
ledger must show minimum 6 cycles, preferably 10, with the skill/gate applied,
failure, fix, and why readability improved in every cycle.

---

## Section 7 — Audit Sufficiency Wording

Readable architecture/trainable audit prose must not overclaim completion.

Gate check:

```
□ "v1 scope completed" names the exact bounded scope
□ "Architecture audit sufficient/complete" appears only after green breadth challenge
□ "достаточно", "готово", or "закрыто" does not describe a narrow audit as broad
□ Breadth challenge covers confidence claims, export accuracy claims,
  target-label leakage, runtime routing confidence floors,
  source/path/provenance leakage, and fallback-to-first-entry/default-candidate shortcuts
□ Parent review asks: "What would a broader audit likely find that this artifact missed?"
□ Later same-domain BLOCKING/HIGH findings create a Delta-Severity Feedback Gate record
```

If any row fails, the output is not readable enough for an architect because
the confidence boundary is false or missing. Return it to governance/review
repair before accepting the artifact.

---

## Section 8 — Sub-Agent Firewall Report Readability

Reports about sub-agent-only repair must make the role boundary visible in
plain language. A readable report cannot hide parent sidecar work behind
phrases such as "I verified locally", "while the agent worked", or "read-only
inspection" without saying when the inspection happened.

Gate check:

```
□ The report names the bounded sub-agent id that authored the changed files
□ The report lists exact changed files
□ The report says parent did not read/analyze governed files before packet return
□ Read-only verification is described as after-return verification
□ The report names the no local non-overlapping work escape
□ Exact files, not broad folder grep, prove the firewall repair
□ Sub-agent waiting is not described as a pause or completion boundary
□ Any final/stop before the sub-agent-required deliverable is complete is
  labelled a violation unless GOLD GATE asked for answer-only or stop
```

If any row fails, the report is not readable enough for Luba to verify whether
the parent stayed architect-only. Return it to review/packaging repair.

Parent Architect Subagent-Only Firewall compatibility: readable reports must
say that parent local sidecar work is forbidden, read-only verification is
allowed only after a sub-agent packet returns, the no local non-overlapping work
escape is disabled, and sub-agent waiting is not a stop.

## Section 9 — Human-Readable Plan/Execution Log Gate

Every phase packet, plan review packet, execution review packet, and completion
packet must be readable as a primary human status, not only as machine state.
Всякий W/G/D-номер в статусе — только после человеческого названия строки, в
скобках; голый номер или список номеров в статусе, heartbeat-е или «следующем
действии» = нарушение Pattern 5 (`NO_BARE_IDENTIFIER_RULE`).

Required meanings. Use Russian headings/sentences for Luba; use machine field
names only in internal packets or compact evidence after the human text:

```text
Этап и смысл:
Всего известных продуктовых шагов, если это уже известно:
Сделано сейчас:
Текущий шаг и владелец:
Тип текущего результата: код | артефакт | проверка | правило | только evidence | commit boundary
Что изменилось в поведении или правилах:
Почему это было нужно:
Что было неправильно:
Какие файлы/правила изменены:
Что возобновляется дальше:
Что осталось:
Какие утверждения открыты:
Какие утверждения закрыты и почему:
Какие утверждения остаются закрытыми и почему:
Почему этому статусу можно верить:
Следующее ограниченное действие:
```

Dual-Path Same-Scenario UI Proof status rule: when visual evidence or host UI
proof is in scope, the status must say that the same scenario twice is required,
which scenario is being repeated through UI, and how many paths are complete:
zero of two, automated only, UI only, or both joined. It must explain the
automated path and real host UI path comparison in human language.
JSON/log/marker screenshot is not UI proof, and a status that only points to
JSON, logs, markers, debug artifacts, or host-open screenshots is not readable
proof of UI execution.
Readable status must use `Dual-Path Same-Scenario Coverage`, not
`product-ready count = 0 because full host UI not proven` or equivalent old
full-live-host wording. If Path B cannot complete, the status must name the
exact local blocker, the next scenario/product fix, and the next bounded action.

Host Session Auth Preflight and Browser Login Recovery status rule: when a
Cursor, Claude, Codex, or other signed-in host UI scenario is in scope, the
status must say whether the host session is signed in, anonymous, wrong account,
or unknown before the UI proof can count. If Cursor opens anonymously, the
status must name browser login recovery as the next action before the same
scenario twice run continues. The readable packet must include the preflight
status `SIGNED_IN_READY`, `LOGIN_RECOVERY_REQUIRED`, `HUMAN_LOGIN_REQUIRED`, or
`AUTH_BLOCKED_WITH_EVIDENCE`, visible signed-in/not-signed-in proof, correlation
to the following UI scenario, and the phrase `no secrets captured`.

Multi-Monitor Target Window Control Protocol readability rule: when a Cursor,
Claude, Codex, or other desktop host UI scenario sends keyboard or mouse input,
the visible packet must say in Russian or the operator's language whether
target-window input focus is proven before input. Internal target-window control
anchors may follow the explanation: Multi-Monitor Target Window Control
Protocol; global foreground alone is not enough; Claude on another monitor is
not itself a blocker; do not send scenario keystrokes unless target-window input
focus is proven; target-window input focus is proven;
TARGET_WINDOW_CONTROL_READY; TARGET_WINDOW_CONTROL_PROTOCOL_MISSING;
TARGET_WINDOW_INPUT_NOT_VERIFIED; TARGET_WINDOW_CONTROL_BLOCKED_WITH_EVIDENCE;
HOST_FOREGROUND_NOT_VERIFIED; do not bypass by moving UI proof to the end.
The packet must include `monitor_count`, `target_window_title`, `target_pid`,
`target_window_handle_if_available`, `target_window_bounds`,
`target_monitor_id_or_bounds`, `activation_method_used`, `input_focus_probe`,
`cursor_ui_reaction_observed`, and `status`. The readable status must explain
that global foreground alone is not enough and that Claude on another monitor
is not itself a blocker. `HOST_FOREGROUND_NOT_VERIFIED` is readable only as a
post-protocol status after target-window activation/input verification fails
with evidence. The packet must not infer auth/session/panel state until
target-window input focus is proven, must not replace real UI control with
JSON/log screenshot, must not require human foregrounding before trying
target-window/multi-monitor control, and must not bypass by moving UI proof to
the end.

No-Blind-Spot UI Scenario Script readability rule: a UI scenario status must
say the scenario id, purpose, required qa/testing skills, exact launch and
preflight steps, Multi-Monitor Target Window Control Protocol before host input, exact
auth/session branches, exact clicks/typing/expected UI state, exact prompt/task,
screenshot/video capture moments, Path A comparison, status meanings, and next
smallest action for every common branch. agent judgment is not a scenario step.
Before execution, the visible packet must show 50-Pass Scenario Clarity Review
status with a human-readable 50-check ledger or the status remains
plan-not-ready.

Plan-Not-Ready Is Repair Loop, Not Stop readability rule: plan not ready is not a stop. The visible status must say do not execute an unready plan, name the
gap, return to architecture/planning repair loop with required skills/context
and subagents when required, and resume the nearest authorized execution only
after plan-ready criteria pass.

Executor-readiness visibility rule: do not stop is not executor authorization.
If the plan is not closed/readable/exact or the product-readiness plan gate has
not passed, the readable message must say plan-not-ready returns to planning
repair, not execution. If a sub-agent/executor was started too early, the
readable message must say the premature executor must be stopped and replaced
by bounded governance repair, then the plan-readiness workflow resumes. Before
any execution continuation is visible as allowed, the message must include
human-readable phase counts before execution continuation: total phases/steps,
done, in-progress, remaining, and plain-language phase/step meaning before any
QSG/row/state/machine label.

Dual-path readiness readability rule: for extension/runtime/host UI work, the
readable message must use these exact meanings before any acronym or packet id:
`Dual-path same-scenario coverage is product readiness evidence`;
`full live host UI path is not the readiness definition unless explicitly re-approved by Luba`;
`same scenario Path A plus Path B`;
`NOT READY is a bounded sub-agent work queue, not a stop`;
`human-readable phase counts before execution continuation`. If the status is
NOT READY, not proven, blocked, needs fix, or "full live host UI path not
proven", the message must name the exact next bounded sub-agent action or the
sub-agent-proven external blocker and mitigation attempt.

Scenario prep readability rule: Scenario prep NEEDS_FIX blocks executor work
order. A status is not readable if it says "run scenario", "test host UI", or
"continue execution" while the scenario prep review is `NEEDS_FIX`. The readable
status must say that plan not closed/readable/exact is repair loop, not
execution and not stop, then name the missing prep rows in human language:
human-readable Russian scenario prep, exact phase/step meaning, exact app/window
target, login/session preflight, which clicks and typing happen in the UI and
what visible state is expected, screenshot/video evidence moments,
monitor/window-overlap fallback, `no secrets captured`, automated Path A exact
commands or exact callable script entrypoints, expected output, and exact
evidence filenames. The next action must be prep
repair plus independent prep review PASS, not an executor run command.

Bad -> good example from the Row20 prep incident:

```text
Bad: "Run Row20 scenario / test host UI. Executor can continue; prep review is NEEDS_FIX."
Good: "Сценарий проверки через приложение пока не запускается: подготовка шага
не закрыта и не читаема для Luba. Нужно дописать по-русски, что именно делает
исполнитель, какие команды проверяют тот же сценарий автоматически, какие
кнопки нажимаются в интерфейсе, какой текст вводится, какое состояние должно
быть видно на экране и какие файлы доказывают результат. После этого отдельный
проверяющий подтверждает подготовку; до подтверждения это возврат на правку, а
не запуск исполнения и не остановка."
```
Internal scenario prep anchors: `Scenario prep NEEDS_FIX blocks executor work order`;
`human-readable Russian scenario prep`; `exact clicks/typing/expected visible state`;
`plan not closed/readable/exact is repair loop`. The visible status must carry
these meanings in Russian or the operator's language before the anchors.

Why Question Root-Cause Repair Trigger readability rule: for any "why/why did
you stop/why did this happen/pochemu/почему" failure-cause response, why is not
a stop. The visible status must answer in human language, name the exact
missing/weak/contradictory skill/guide/agent-file rule, say that the agent must
repair skills/guides/agent files before continuation through the active role
boundary, list concrete files/rules changed, and explain human language then
continue the nearest safe already-authorized work. Do not present protocol
repair complete as a final report, do not claim readiness until the exact
artifact/work chain resumes, and say when sub-agent waiting is the next
non-stop orchestration step.

Stop-loophole human-readable status repair (2026-06-28): every report that says
`NOT READY`, blocker, failed live host proof, impossible UI surface, missing
evidence, or old product-readiness wording must immediately translate that
state into plain Russian continuation wording. Required order before any
machine label: total phases/steps, done, in progress, remaining, what the
current phase solves, what the current step solves, exact evidence present,
exact evidence not proven, and next bounded sub-agent action. `NOT READY is a
bounded sub-agent work queue, not a stop`.

The old host-only readiness sentence `user in host UI -> extension -> core ->
answer in host UI` is readable only as optional bonus/live proof. The readable
primary gate is `Dual-path same-scenario coverage is product readiness
evidence`: same scenario Path A automated/API/core/adapter evidence plus Path B
real local host UI evidence, or exact Path B blocker with mitigation and next
bounded work order.
Internal control anchors: `why_is_not_a_stop` and
`explain_human_language_then_continue`. In the visible message, keep the same
meaning in Russian or the operator's language; these English anchors may appear
only as secondary internal references.

Machine-only JSON, ledger rows, hidden logs, or status codes may be attached as
evidence, but they cannot be the primary explanation for Luba. If these fields
are absent, the packet fails readability even when the machine evidence is
valid.

`ROUTINE_PROGRESS_COMPACT_EVIDENCE_GATE`: ordinary commentary heartbeat/progress
updates are not worker packets. They must open with a short human paragraph or
compact human board and may omit the full claim/evidence schema. If evidence
fields appear in routine commentary, keep them to one short `Evidence:` sentence
or at most two short support lines. Full claim/evidence schema belongs to worker
return packets, acceptance/rejection reviews, final summaries, and direct
audit/evidence answers. A report that formally starts with `Фаза...` but then
dumps a long raw evidence block is still `STATUS_FORMAT_HUMAN_GATE_FAILED` if
the visible message is not skimmable.
`HYBRID_STATUS_PACKET_DUMP_FORBIDDEN`: a formal human board plus a long raw
claim/evidence packet is unreadable hybrid status. Reject `Фаза/Шаг/Статус`
text or any formal human board / phase-step-status opening followed by
`claim_scope`, `evidence_scope`, `count_claimed`, `independent_evidence_count`,
`subagent_id_or_NONE`, `downgrade_needed`, raw worker packet, or machine
inventory as the main visible body. Reject it as
`STATUS_FORMAT_HUMAN_GATE_FAILED`. Parent output must summarize the worker
packet in human language first; routine progress may show only one short
`Evidence:` sentence or 1-2 compact support lines. Full schemas are reserved for
worker return packets, acceptance/rejection reviews, final summaries, or
explicit audit/evidence answers.

`PARENT_TRANSLATES_WORKER_MACHINE_FIELDS`: parent-visible status must translate
worker `claim_scope/evidence_scope` fields into human meaning, result, remaining
gap, and next bounded action before showing them to Luba. A spawned worker packet
with `subagent_id_or_NONE: NONE` is identity-defective; the worker uses
`UNKNOWN_IN_SUBAGENT` when the platform id is hidden, and the parent reconciles
the real platform id before accepting the packet.

`DIAGNOSTIC_STATUS_CLAIM_REQUIRES_VISIBLE_EVIDENCE`: when Luba asks why,
where the proof is, whether something was checked, or challenges a factual
status answer, readability fails if the answer gives only a conclusion. The
answer must include a compact proof in human Russian:

```text
Вывод: <plain conclusion>
Доказательство: <command/readback/path checked> -> <observed result>
Ограничение: <what this does not prove, if relevant>
```

`COMPACT_EVIDENCE_IS_REQUIRED_NOT_OPTIONAL`: ordinary heartbeat updates may use
short evidence, but direct diagnostic/status answers cannot omit evidence
entirely. If no command/readback was run, the answer must say "это не проверено
сейчас" before inference.

Human gate for chat logs: a log/status fails if Luba cannot tell from the
visible message what is done, what is running, what remains, whether the current
result is code, artifact, review, policy, or evidence-only, which claims are
still closed, and what bounded action comes next. The repair is governance/skill
repair before product continuation, not a final stop while the next action is
known.

### Human-Readable Execution Board Gate

Every long plan execution status must include a plain Russian progress board:

```text
Всего: <total major plan blocks and/or current-phase tasks>
Сделано: <completed count>/<total>; <completed names>
Сейчас выполняется: <current task and owner/sub-agent>
Осталось: <remaining count>; <remaining names>
Тип текущего результата: <код | артефакт | проверка | правило | только доказательство | граница коммита>
Принятые доказательства: <accepted evidence in human words>
Закрытые claims и почему: <claim names + human reason they remain closed>
Открытые разрывы: <open gaps>
Следующее действие: <следующее ограниченное действие человеческими словами>
Execution paused for plan-review visibility: yes/no
```

`STATUS_FORMAT_HUMAN_GATE_ALWAYS_REQUIRED`: every progress/status/review/final/
worker packet visible to Luba must pass the canonical Russian human gate before
any machine block, raw packet, JSON, ledger row, STATE path, validator label, or
opaque status. `STATUS_FORMAT_CHALLENGE_REPAIR` is a special case: when Luba
challenges the format of a work status, including `Формат статуса работы не
соответствует требованиям` or `Формат статуса работы не соответсвует
требованиям`, answer first, classify `status_format_governance_gap`, patch or
verify the governing rule, and only then continue execution. Acceptance/final/
worker/audit packets use the full canonical Russian block; ordinary commentary
progress may use the shorter human-first form from
`ROUTINE_PROGRESS_COMPACT_EVIDENCE_GATE`.

```text
Фаза X из Y: <человеческое название>. Смысл фазы: <что эта фаза решает для проекта/пользователя>.
Шаг X.Y из X.N: <человеческое название>. Смысл шага: <что этот шаг доказывает/чинит>.
Статус: <простое русское объяснение; внутренний код вроде DONE/NEEDS_FIX можно добавить после смысла>.
Всего: <общее количество фаз/шагов/работы, к которому относится статус>.
Сделано: <что реально завершено, с evidence boundary>.
В процессе: <что сейчас выполняется и кем/чем>.
Осталось:
1. <оставшийся шаг и смысл>.
2. <оставшийся шаг и смысл>.
Review нашёл: <что проверка нашла, или "пока не проверялось">.
Что исправлено: <что изменено, или "пока ничего не исправлялось">.
Следующее действие: <следующее ограниченное действие или "нет", если всё принято>.
Если работа ещё осталась: это промежуточный статус, а не остановка; следующий ограниченный шаг указан выше.
Evidence: <optional one short support sentence or 1-2 compact evidence lines for routine progress>.
```

If `remaining_count > 0`, the visible status must express this continuation
meaning in Russian or the operator's language. The English anchor
`final/status/report/commit-push/status-board is not a stop while authorized
work remains` may appear after the human sentence as an internal reference, but
it is not mandatory visible text and cannot replace the human sentence. Machine
logs, ledgers, JSON, file paths, claim fields, or short progress prose may
support the block, but cannot replace it. In routine commentary, evidence
support is capped at one `Evidence:` sentence or 1-2 short lines; the full
claim/evidence schema is reserved for worker return packets,
acceptance/rejection reviews, final summaries, and direct audit/evidence
answers. Reject as `STATUS_FORMAT_HUMAN_GATE_FAILED` any machine-only packet,
raw `claim_scope` block without the Russian phase/step/status board, bare label
such as `NEEDS_FIX`, `packet accepted`, `QSG`, `boundary pass`, STATE/JSON path,
informal partial board, or formal `Фаза...` opening followed by a long raw
evidence dump. A returned sub-agent packet is not user-visible status until the
parent translates it into this human-gate format.
If Luba says the status is `не читабельно`, `нечитабельно`, `format lost`, or
equivalent, treat it as a status-format challenge: answer first, classify the
old output as `STATUS_FORMAT_HUMAN_GATE_FAILED` or
`status_format_governance_gap`, patch or verify this rule, and then resume the
nearest authorized product work.

If the board is missing, the status is not readable enough even when STATE,
JSON, or validator output exists. The board must appear in the user-visible
message before or alongside machine evidence.

`status_board_counts_drive_continuation`: the visible board is execution
control, not a pause surface. If the board says `remaining_count > 0` and
`next_bounded_action_exists=true`, the readable output must say that this is
interim status and the parent must immediately continue by starting, waiting
  for, reviewing, or repairing the next bounded sub-agent work order. A
  final-style summary is readable only when Luba explicitly stops/pauses/asks for
  answer-only or all active requirements are complete. Required visible meaning,
not required English wording: if work remains, the report/status/commit-push/
status-board is interim and the next bounded action continues. The English
anchor `final/status/report/commit-push/status-board is not a stop while
authorized work remains` is an internal/reference anchor. Commit/push of an
evidence packet or status board is not a completion boundary when the status
board says remaining groups exist.
`accidental_final_recovery_requires_subagent_repair_and_resume`: after an
accidental final/status stop, the readable next-turn message must answer Luba
first, say what loophole was repaired through bounded sub-agents, and say which
previously known bounded action resumes without fresh approval unless Luba stops
or a real blocker/unsafe action exists.

### No Machine Log To User Rule

User-facing progress cannot be only `QSG-28 accepted`, `STATE goal_reached=true`,
`PASS_FOR_LUBA_REVIEW`, a JSON object, or an internal status label. Internal
labels may appear only with a human translation:

```text
Проверка: <что проверялось>
Результат: <что прошло или не прошло>
Что исправлено: <что исправлено, или "ничего" с причиной>
Что осталось: <какой разрыв остаётся, или "ничего" с причиной>
```

If a status cannot be understood without opening raw logs, return it to
readability repair.

## Section 10 — Execution Plan Visibility Gate

Before runtime/code/training/host execution continues after governance repair,
the parent must present the execution plan and 10 Review Cycle Gate outcome in
chat in human-readable Russian. The visible message must include the meanings,
not raw field names: current stage and why it matters; current plan step; what
the 10-review gate found; which plan sections changed; which claims opened or
closed and why; what gaps remain; and why execution is or is not allowed to
continue. Internal packets may map those meanings to `current_stage`,
`current_plan_step`, `10_review_cycle_outcome`, `plan_sections_changed`,
`claims_opened`, `claims_closed`, `remaining_gaps`, and
`execution_admission_reason` after the human explanation.

A plan hidden in files, a JSON matrix, or a machine-only log does not satisfy
this gate. The chat packet must be understandable without reading the raw log.

### Plan Presentation Gate

If Luba says she cannot see the plan, cannot see the 10 reviews, or cannot
understand the current stage, the next user-visible packet must present the plan
in chat before execution resumes. This is a visibility/protocol failure to
repair, not a final stop by itself. Artifact-only plans, STATE paths, JSON logs,
and hidden matrices do not satisfy the gate.

### Ten-Review Evidence Gate

When 10 reviews are required, the readable packet must show records 1..10. Each
record includes reviewer role/id, requested model/effort, verdict, findings,
repair action, files/sections affected, whether the plan/skills changed, and
recheck result. A summary such as "10 reviews completed" is not readable
evidence.

### Sub-Agent Visibility Ledger

Any sub-agent work used as evidence must be summarized in human language:
agent id or nickname, requested model, requested effort, task, status, key
result, exact changed files or reviewed sections, exact review scope, progress
counts, and what parent review accepted or rejected. Hidden sub-agent
transcripts are secondary evidence only.

---

## Anti-Patterns

1. **"Shortcodes are faster to write"** — shortcodes are faster for the
   generator; they are slower for the reader. The gate exists to protect
   review efficiency, not generation efficiency.

2. **"It's obvious what CF-12 means in context"** — shortcodes are only
   obvious after reading the reference table. Prose must be self-explanatory
   to a reader who has not read the table.

3. **"I'll expand it later"** — the gate runs before output is accepted.
   "Later" never comes in a generation pipeline. Expand at generation time.

4. **"The code fence/table exception covers this"** — the exception covers exact
   syntax, not explanation. A class name, API path, or status value may remain
   exact in code, inline backticks, or a technical reference table. Human-facing
   comments, Mermaid/PlantUML captions, notes, legends, branch labels, expected
   outcomes, and quoted "good" examples must still be readable human language.

---

## Integration

- ai-pipeline-topology-SKILL.md generation nodes: gate applies to all ai-generate output
- context-package-design-SKILL.md prose sections must pass this gate
- prompt-patch-authoring-SKILL.md WRONG/CORRECT pairs: exact identifiers in
  code examples may remain exact; explanatory comments or quoted "good" examples
  inside those fences still follow the human-language rule
- score-interpretation-SKILL.md bracket names (PASS, DETAIL_GAP) may appear as
  technical values in reference tables after the explanation; table cells that
  explain meaning, expected outcomes, or branch behavior are not exempt
- human-readable-architecture-audit-SKILL.md governs the architecture audit
  readability, sufficiency boundary, breadth challenge, and delta-severity
  feedback gates

PASS26_ACTIVE_PILOT_COUNTER_PROPAGATION_GATE: When the Claude board-owner resolves pilot activation or global execution-board counts, active-codex must propagate exact current status/counts/pilot identity to the master plan, execution board, requirements matrix, source map, plan-14, full-task gates, repair ledger, and relevant consumers, or label old text historical/pre-Pass26/local-only. A resolved pilot must not be downgraded to blocked reserve, baseline-59 outside scope, or an open Luba/Q41 question. Missing propagation is active-vs-Claude drift and blocks readiness, B-22 closure, and implementation authorization.
FULL_ACCESS_NO_PERMISSION_PROMPT_GATE: When Luba explicitly grants FULL ACCESS or the environment reports danger-full-access/unrestricted filesystem with approval policy never, the agent must not request permissions, must not set `sandbox_permissions`, and must not open approval prompts for filesystem work needed by the active plan/governance repair. Permission prompting under full access is a governance failure: answer Luba first, classify the old behavior as a permission-prompt loophole, patch this gate if missing, then continue the authorized repair loop. Full access does not authorize unrelated product implementation, destructive git operations, external provider calls, or publishing unless the active task explicitly includes those actions; it does authorize plan/governance/skill/guide/agent-file repairs in the named planning library and project guidance without asking again.
