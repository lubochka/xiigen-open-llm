# CP4.03 — опубликованные изменения и граница результата

**2026-10-13. AUTHORING_DELIVERED / DOCUMENTARY_READBACK.** Это завершение текущего авторского уточнения плана после промежуточных публикаций, не выполнение25исследовательских/аналитических сессий, не PLAN_READY после независимой экспертизы и не готовность обученной системы.

## Действующие артефакты

Все пути ниже относительно `llm mvp/design to code session/replanning`. Ветка `lubochka/xiigen-planning:llm_plan3`.

| Файл | Что изменено | Публикация / проверенная опора |
|---|---|---|
| [RECOVERED-DESIGN-PROMPTS.md](../RECOVERED-DESIGN-PROMPTS.md) | Обновлены25предметных карточек: concrete prompt/context/skills/primary routes, good/bad/edge, outputs, verification/future examination; семь domain cases; recipient/order/reward/lineage; algorithm comparisons и PC feedback. | Commit `a67f8dc4af080a9123bbe0f3bea26c481790da8b`; blob `4428d29cf47cd0520773e1071bfc1ca6203c0a42`. |
| [START-NEXT-RUN.md](../START-NEXT-RUN.md) | Effective packet, current R02 locators, separate source/section namespaces, parallel continuation; поздний R03 CP07 F20/F21 направлен конкретным владельцам без restart. | Первичная CP4 правка `a2201b78e48b44021f197522eb5c376f2729f9d2`; итоговая `23f4ea5be3bf4f79623ef1d8bb7bd05fd7eaf9b7`; blob `7b7498df1b2a28c331733d9b85b8e8c47e85726f`. |
| [PLAN-25-RUNS.md](../PLAN-25-RUNS.md) | Единая очередь10+5+5+5, source→owner→application map и first-learning critical path; пример domain-conditioned metric как гипотеза, не назначенный optimum. | Commit `00849cf10189417d6cb4017c9caff31c7b1b590c`; blob `164118073ddf7aa019f9b507fb01d981bf9e93f4`. |
| [CP4.01](CP4-01-ALGORITHM-PLAN-AUDIT.md) | Первый checkpoint опубликован до завершения уточнения; scope/read boundaries/сохранение parallel workers. | Commit `277959a20f2e6742d8caa51d08106e061e16a086`; blob `9889b41e59c21b180766796d9cce5ca18d8428ec`. |
| [CP4.02](CP4-02-NEW-SOURCE-DELTAS.md) | Проверенные первичные противоречия, field/order/reward/fan-out; поздние атрибутированные R03-F20/F21 и заполненный адресный handoff. | Первая публикация `6994a7f5d01d1467c293e6aac45a25bd30e65d1c`; дополнение `9ae7dfa14218d722e679c45d27a4fdd38acaa937`; returned blob `c391aed3f96154fc3f40e72b0ea09de066e8967d`. |

Immutable candidate для сверки трёх действующих файлов: `23f4ea5be3bf4f79623ef1d8bb7bd05fd7eaf9b7`. Параллельные workers могут продолжать ветку; это не утверждение, что её HEAD навсегда останется равен candidate. История прежних версий сохранена; force-push и whole-tree replacement не применялись.

## Что действительно проверено

До update_file прочитаны соответствующие прежние тексты и current blob SHA. Исходный RECOVERED-DESIGN-PROMPTS CP3 прочитан диапазонами1–350, включая все25карточек; START CP3 —1–110; PLAN — его основная очередь и заключительные sections. У длинных отдельных source reads границы/усечения не скрывались.

После публикации прямым fetch_file проверены: CP4.01 на своём exact commit; RECOVERED карточки/доменные случаи115–153 на `a67f8dc...`; на exact candidate `23f4ea...` — RECOVERED77–106 (representation, recipient/DPO и kNN),328–379 (PC/feedback/ordinary consumers/R25), START35–79 (locators/namespaces/адресный CP07 handoff) и PLAN105–139 (first lesson/algorithms/measurement/scope). Возвращённые blob SHA совпали с receipt опубликованных файлов. Это предметный selective readback и авторская смысловая сверка, не полный автоматический markdown/link audit или independent reviewer.

Прямые primary reads автора: DPO builder plan P14 §§3.1–3.2a/195–438; negative strategy N01/1–90; core LearningMachineContracts/1–105. Результаты R01/R02/R03 прочитаны по указанным в CP4.02 разделам; поздний R03 CP071–67/136–207 — атрибутированный input. Его новые primary/code reads не выдаются за свои. Исторический `previous rounds thinking results.md` использован в прочитанной начальной части и адресных атрибутированных находках; весь большой архив не объявлен прочитанным.

Skills применялись по реально прочитанным sections: planning-session-startup1–70; phase-planning1–80; plan-review80–155; trainable-architecture-decision125–225; generated-code-review1–85; architect-behavior-classifier1–65; prompt-patch-authoring1–85; output-readability-gate1–65; reference-inventory-and-source-map65–120. Это не заявление полного чтения всей skill-библиотеки; дополнительные specialist skills остаются заданиями соответствующего исполнителя. Исторические30/50-pass правила не импортированы как дополнительные sessions.

## Существенные исправления вместо ещё одного общего трактата

1. **Pair recipient:** NodeOrEdgeId, winner solver id и target selector не сливаются; lower four-field item проверяется вместе с actual external transport.
2. **Semantic order:** OrderedPatternIds сохраняет порядок зависимых шагов; candidate permutation не label. Judge score не подменяет DPO policy/reference reward.
3. **Pair quality:** N01 one-error/two-error contradiction разделён на контролируемые контрасты; annotations проверяются как потенциальные cues. N−1 attempts остаются одной source lineage и не разрешают repeated updates.
4. **Hybrid mechanisms:** fixed candidates/constraints отделены от learned selection. Поздний старый blanket helper ban из R03-F20 получает primary examination/решение R12, а не автоматически запрещает требуемую пользователем гибридную систему.
5. **Representation/LoRA:** raw-word/TaskSignal coordinate meaning, actual projection/optimizer/consumer, index compatibility и total footprint вместо proof по shape/loss/adapter size.
6. **Examination:** per-domain law/feature/contrast, same-panel snapshots/no-update/retention и full costs; разные bulk difficulties, family leakage и extra teacher/compute не выдают ложный gain.
7. **First lesson:** один minimal existing-path seam и R13 recipe передаются R18 сначала; остальные B/C/curriculum/Windows delivery не искусственные prerequisites.

## Как продолжать без новой очереди

R01–R03 продолжают свои собственные snapshots/остатки, не ждут друг друга и не перезапускаются из-за этих commits. Автор CP4 не менял ни один файл `results/R01/`, `results/R02/`, `results/R03/`. Чужие статусы не повышались по наличию файлов. Их настоящие последующие commits остаются их работой.

R04/R05/R06 берут достаточные тематические evidence subsets по START. R05 особенно получает P14/C01 и адресный LoRA seam R03-F21; R06 — domain/graph и helper conflict F20. R07 анализирует только стыки. R12 решает конкретные ownership/objective/policy gaps; R13 подготавливает первый различающий recipe; R18 — exact bounded implementation prompt на существующем механизме. Это будущие назначенные Rxx, не якобы исполненная работа этого автора.

R15/R25 остаются отдельными реальными экспертизами. Состояние сейчас: **INDEPENDENT_ASTRA_REVIEW=NOT_PERFORMED; LEARNING_GAIN=NOT_MEASURED; PC_FIT=NOT_MEASURED; FULL_SOURCE_TRAVERSAL=NOT_CLAIMED; PRODUCT_IMPLEMENTATION=NOT_EXECUTED_THIS_AUTHORING.**

Новые product tests/coverage/build/training/Windows job invocations не выполнялись; code/tests/workflows/weights/datasets не менялись. Документные commits используют `[skip ci]`; это не аудит всех автоматических workflow-событий репозитория. GitHub записи реально выполнены, старое «read-only» не использовалось как ограничение.

Итог текущего поручения — опубликованные уточнённые карточки, действующий dispatcher и согласованный roadmap с проверенным readback, а не обещание продолжать невидимую фоновую работу и не утверждение, что система уже научилась лучше.
