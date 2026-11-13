# Replay, перенос между доменами, feedback и retention

<!-- CODEX-POST128-PLANNING:START -->
## Действующее уточнение после P128 — Происхождение повторяемого материала и настоящая кратность

Проверенный core: `f8784df037b59cf878889eef4d0956f5d0c985e7`; planning: `5df24354aa803f7ed5dd3bac48146466e2feb189`. Это адресное усиление приёмки соответствующего класса, а не новая аттестация всех исторических опытов карточки.

Текстовый digest не заменяет SourceRowId. Две законные Train-строки одной семьи с одинаковым содержанием и независимым Test принимаются с обоими IDs; перестановка SourceRows не меняет их личность. Честный Train/Test overlap должен дойти до именованного отказа по пересечению, а не сломаться на rows[0]. Для selective policy Dev/Test учитываются раздельно; строгую защиту фактически потребляемого held-out не ослаблять. В E43 сохраняется уже исправленное извлечение пары внутри файла; project/file — отдельные элементы tuple, а не склейка для отображения. Исторические 3→0 межфайловых переходов сохраняются; качество и доза replay считаются отдельно.

**Передать исполнителю вместе с собственным паспортом:** проследить названный actual consumer, сначала принять полный honest пакет, затем отвергнуть одну адресную порчу тем же checker; результат включить в общий verdict и внешний exit. Указать источник/сборку, фактический расход каждой попытки и предел утверждения. Ремонт expected опирается на независимый договор, не на actual сломанного кода. Сохранённый PRIMARY не повторять; gold, исходные бюджеты и прошлые результаты не менять. Активная попытка сохраняет ранее выданный ей pin карточки. Native source/static counterexample и исполненный тест обозначать отдельно.

[Сводка нового аудита](codex-2026-11-03-POST128-TRANSFER-AND-EXPERIMENT-AUDIT.md) · [действующая core-карточка](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P112-family-disjoint-evaluation.md#current-executor-task). Историческое содержание ниже сохранено.
<!-- CODEX-POST128-PLANNING:END -->
## Что говорит эксперимент и что говорит нынешний RP01
Исторический E43: память с соотношением примерно **70% новых /30% replay старых** подняла удержание старого bigram-навыка c 0.3226 до 0.3978; [run-2026-05-01-001.json](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json), код [E43ReplayBuffer.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs#L8-L55). Это наблюдение **другой модельной системы**, не доказательство эффективности реплея LoRA-Transformer.

В actual RP01 [CodeDocumentationSimulationDpoExperiment.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L173-L190) один проход по новым 500 парам, батчи по10 и optimizer updates. ENRU продолжает C# C50 full checkpoint, не перезапускает base, но в показанном цикле отсутствует примешивание C# старых законных примеров к ENRU batches. Старое историческое решение D-06 рекомендует 70/30; **это архитектурное расхождение**, но нельзя объявлять его корнем провала ENRU, потому что C# уже давал 0/3 meaningful outputs до переноса.

## Чего особенно недостаёт доказательствам
- Отдельно измерять **initial old skill** before new domain; иначе `0→0 retention` не retention.
- New-domain gain, old-domain degradation, tokenizer compatibility, optimizer continuity, replay buffer legitimacy and provenance, old pair consumption one-time/resampling policy.
- Domain-specific adapters vs shared adapter; `old domain recall` в одинаковом consumer и не на подсмотренных generation prompts.
- Positive/negative/warning memory — не слепой повтор rejected data как chosen. Illegal training sources не должны попадать в replay даже при high score.
- Feedback pipeline: accepted/edited signals, consent, source, law verification, user corrections, quarantined poison, reference identity; replay `already-applied` journal and idempotency.
- Sleep/idle learning и full-PC budget — отдельная цель; нельзя смешивать с быстрым диагностическим trainer path.

Соседние источники: [DECISIONS-LOCKED.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/llm%20dynamic%20flow/DECISIONS-LOCKED.md#L9-L20); [FINDINGS.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/design%20to%20code%20session/replanning/results/R23/FINDINGS.md#L9-L43); [09.09.26WorkLeft-MECH-01-mechanism-validation.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-MECH-01-mechanism-validation.md#L13-L54).

## Минимальный корректный replay contrast
**После** положительного короткого C# capability preflight: A = C# then ENRU no replay; B = **те же** C# then ENRU + lawful 30% old families; C = C# frozen no ENRU as retention reference. Одинаковые tokenizer, optimizer rounds, input budget, family split, sampling schema, temperature; на всех трёх отдельно C# and ENRU heldout. Не подгонять fixed 30% под текущий free panel; 30% — лишь historic hypothesis. Обязательно сохранять actual old-pair provenance и не обучать дважды consumed irreversible DPO lesson без разрешения нового курса. При нулевой компетенции старого домена replay experiment `NOT_INFORMATIVE`.
