# 12 — Минимальный причинный эксперимент для каждого DIF-01…44
**16.10.2026 · SPECIFICATION, NOT EXECUTION.** Источник: [основной регистр](01-PRIORITIZED-DIFFERENCES.md) и [REC02/03/04/05](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md). Не запускать 44 исследования: **один самый информативный мини-опыт, затем обновить приоритеты**.

## Два разных вида доказательства
1. **CODE DIFFERENCE:** интерфейс или формула отличается; это *не* доказательство ухудшения обучаемости.
2. **CAUSAL LEARNING QUALITY:** до/после реального разрешённого обновления на тех же новых domain tasks, строгий A/B с единственным изменяемым механизмом, blind family-disjoint heldout, actual intended consumer и учёт времени. Для learned retrieval с Train/Index отдельно писать `MEMORY_ADOPTION`, не нейронный градиент.
3. **READOUT / EVALUATOR EFFECT:** в frozen Student поменялась только генерация/фильтр/маршрут; это изменение пользовательского качества, но **не** факта обучения весов.

Общий протокол: pin исходные source/weights/tokenizer SHA; проверить, что затронутый метод **на самом деле вызывается** в неудачном RP01 или относится к другому продукту; preregister A/B, оценку, split по semantic family, stop; держать train/dev/heldout изолированными; **никогда** не делать старый consumed dataset новым lesson. На стадии R=read-only 0 optimizer; T1/T2 — только новый разрешённый tiny train, не существующая команда `train 16` (её нет). Никаких GitHub Windows Actions для измерения качества.

## Картотека из 44 быстрых опытов
В каждом ряду A/B=ровно одно изменение; метрика относится к указанному named consumer, а не автоматически к Student. Классы: `R0` code/data trace ≤12 cases / 0 optimizer; `R1` read-only paired evaluation ≤24 cases / 0 optimizer; `T1` ≤1 new optimizer call/arm; `T2` ≤2 new calls/arm; `BLOCK` требует явной реализации/admission до training.

| ID | A/B: проверка ровно одной гипотезы | Проверяемый эффект и фальсификатор | Бюджет/владелец |
|---|---|---|---|
| DIF-01 | A: 2 DPO mini-steps; B: 1 supervised chosen + 1 DPO; один B0, корпус и heldout | Сравнить прирост correct_valid на новых C# families, critical mistakes и cost; [детальный протокол](11-MICRO-EXPERIMENT-COLD-START-VS-DPO.md) | T2/RP01 |
| DIF-02 | Переставить координаты TeachingVector при той же ненулевой sparsity | Если VectorSignal, gradient и logits неизменны — координатный смысл не используется; **качество не доказано** | R0/RP01 |
| DIF-03 | Local vocabulary→shared root vocabulary на тех же двух независимых prompts | Index↔concept и downstream law correctness; не путать координаты с LM LoRA | R0/RP02 |
| DIF-04 | Шаблонные vs law-diverse TRAIN при равных tokens/updates | New-family correct_valid; 500 строк ≠ 500 семейств | T1/RP01 |
| DIF-05 | Одна vs две **новых неповторяющихся** mini-итерации | Доза: odds и heldout correctness; не повторять consumed samples | T2/RP01 |
| DIF-06 | Для одного frozen checkpoint — response odds vs greedy first token vs full decode | Сдвиг logits без улучшения ответа = decoding/competence gap, не баг optimizer | R1/RP01 |
| DIF-07 | Один индекс/корпус: nearest[0] vs rerank same top-k | Law-correct answer и coverage; не выдавать за обучение LoRA | R1/RP02 |
| DIF-08 | Один Student SHA: обычный вызов без/с typed retrieval/arbiter | Трасса фактического consumer и quality; выигрыш может быть исключительно от retrieval | R1/RP02 |
| DIF-09 | После tiny-overfit: rank8 q/v vs rank16 q/v, same B0/data | Gain/compute на heldout при одном изменении; до готового режима — BLOCK | T2/RP01 |
| DIF-10 | Измерить C#/RU BPE tokens/word, p95 response length, roundtrip | Не менять vocabulary без совместимого нового checkpoint; fragmentation≠коррупция | R0/RP01 |
| DIF-11 | На тех же cases сравнить train preference margin vs свободную генерацию | Metric disagreement — разрыв оценки, не отдельный причинный механизм | R1/RP01 |
| DIF-12 | Отключить/включить чтение старой *retrieval* memory, не gradient replay | Old-skill retention и coverage; без положительного pre-skill baseline — N/A | R1/RP05 |
| DIF-13 | Одинаковые labeled examples: BPE hash vs order-sensitive AST features | После memory.Train сравнить unseen order/negation tasks, top1 и law accuracy | R1/RP02 |
| DIF-14 | Один scored pool: warning-only vs calibrated abstain | Correct@answered + coverage + critical accepted; precision@0 coverage — FAIL | R1/RP02 |
| DIF-15 | Фиксированный pool: top-k до vs после law-eligibility filter | Запрещённые ответы/правильные/abstain; unknown-law не hard veto | R1/RP02 |
| DIF-16 | Trace raw vs normalized для «not delete 10»/«delete 100», кириллица/операторы | Сравнить именно input конкретного потребителя, не весь проект | R0/RP02 |
| DIF-17 | Проверить живой порядок stack-detector и normalization | Если stack уже до normalizer — negative control, не ремонтировать | R0/RP02 |
| DIF-18 | 8 новых C# sides через compiler + semantic code oracle | Ошибочные chosen в карантин; качество новой исправленной пары отдельно | R0/RP01 |
| DIF-19 | Текущая simulation side vs новая законно созданная и независимо оценённая | Provenance не качество; после semantic vetting сравнить real learning только с допуском | R0/T1/RP01 |
| DIF-20 | Same law; superficial vs one-defect length-matched DPO contrast | На новом heldout проверить восстановление закона, не train margin | T1/RP01 |
| DIF-21 | Trace exact-v2 masks vs legacy Compact(480) на длинной паре | Если RP01 exact не обрезает, legacy deficit не причина текущего прогона | R0/RP01 |
| DIF-22 | Сравнить token-ID train prefix и inference framing/stop, без обновления весов | Read-only free valid и first-token logits при одном изменении framing | R1/RP01 |
| DIF-23 | Tiny finite difference dLoss/dLoRA-weight vs backward gradient | Знак/величина + one-hot masks и frozen base; потом tiny overfit | R0/RP01 |
| DIF-24 | Два 60D закона с одинаковой sparsity, переставить sign/slots | Если одинаковый scalar — нет vector→gradient projection; quality нужен отдельный T1 | R0/RP02 |
| DIF-25 | 2 inputs→3 outputs, поменять порядок доставки, проверить 1→0 | Правильные associations, types, payload, не count/hashes | R1/RP03 |
| DIF-26 | Один pair NodeOrEdgeId→named LoRA keys→cold ordinary Unit | Поменялся ли нужный получатель/heldout output, unrelated Units frozen | R0/T1/RP03 |
| DIF-27 | Frozen Units: fixed Flow vs изменённый *обученный* Flow | Правильное новое input→output association; если нет owner — BLOCK | R1/RP03 |
| DIF-28 | Frozen Units: first-fast vs multi-async branch+arbiter | Law-correctness, missing output, cancellations, **полная** latency; no learning claim без state | R1/RP04 |
| DIF-29 | Известные primitives, новая композиция; fixed graph vs trained edge policy | Новое сочетание корректно без illegal graph path; нет training hook — BLOCK | R1/RP04 |
| DIF-30 | Сначала old-domain PASS, затем новый ENRU lesson и проверка старой способности | Old retained + new gained; 0/3→0/3 не measurement retention | T2/RP05 |
| DIF-31 | Live trained vs cold-load private vs ordinary published consumer | Одинаковый SHA/logits и user-visible output, не private-only proof | R1/RP01 |
| DIF-32 | Read-only импорт C50 в policy+reference, freeze after | Starting both equal может быть корректно; проверить divergence only after update | R0/RP01 |
| DIF-33 | Dropout0 one seed: live weights/logits vs fresh restore | Изменился forbidden base / несовпадение logits? Только тогда repair | R0/RP01 |
| DIF-34 | На новых pairs: arbitrary ×10 weights vs balanced law-family weights | New-family gain и cost, не считать E44 успехом: исторически ухудшалось | T1/RP05 |
| DIF-35 | При одном candidate pool: first-hit vs BM25+semantic rerank | Holdout lawful correct@coverage, cost; не смешивать разные embedding spaces | R1/RP02 |
| DIF-36 | Одинаковые positive/negative/warning neighbors; фильтр off/on | Wrong accepted, correct retrieved, abstain; это retrieval-memory не DPO gradient | R1/RP02 |
| DIF-37 | Один SQL×BUG×RU query: collapsed domain vs typed subject/intent/lang | Actual correct solver/output, а не только новый enum | R1/RP02 |
| DIF-38 | Frozen candidates: замена только одного Unit/Flow route и per-stage credit | Вклад в final semantic correctness и owner обновления | R1/RP03 |
| DIF-39 | Train prompt exact bytes vs inference raw bytes, stop reason | Если равный frozen Student меняет качество лишь от framing — diagnosis decoder | R1/RP01 |
| DIF-40 | Fixed vs curriculum order для тех же новых допущенных семейств | Heldout correct и cost, при разрешённом независимом A/B; старый E45 не дал gain | T2/RP05 |
| DIF-41 | Те же веса на disclosed near-template vs sealed new-family panel | Изменение optimistic score не есть изменение learning; корректировать оценку | R1/RP01 |
| DIF-42 | Снять reference/policy/backward/AdamW/serialization/fresh-load по одному шагу | Quality per full wall/RAM; optimizer-only time не равно step time | R0/RP01 |
| DIF-43 | Один consented feedback→admitted new pair→state→cold consumer | Исправлено ли новое семейство без leaking/duplicate; no permit — BLOCK | T1/RP05 |
| DIF-44 | Один unknown/hard-law запрос: warning-only vs deny/clarify fallback | Critical wrong accepted, precision **и** coverage; abstain не learned answer | R1/RP02 |
## Самый первый запуск (важнее 43 оставшихся)

**Шаг 0:** быстро выполнить R0 DIF-02, 03, 21, 23, 31, 33 как source + tiny numeric proof. Если gradient математически ошибочен или checkpoint не совпадает после restore, **не** запускать замены корпуса и модели до исправления.

**Шаг 1:** при исправных gradient/save-state сделать один [контролируемый A/B «DPO-only vs SFT→DPO»](11-MICRO-EXPERIMENT-COLD-START-VS-DPO.md). Это лучше всего проверяет ведущую гипотезу RP01 и включает новый masked/semantic HELDOUT. Армы делят одни B0+test data, но controlled paired use одних training-pairs должен быть явно разрешён current one-pair/no-repeat contract; не обходить запрет разными runID.

**Шаг 2 (отдельно от нейронного обучения):** дешёвый retrieval R1 DIF-07/13/35 на одной памяти/запросах. Он проверяет утраченный механизм старых E09/E34, но **не** исправляет сам по себе качество DPO Student.

## Обязательный научный итог
Для каждого выполненного опыта: `DIF-ID; initial/head/weights hashes; actual consumer; dataset family split; A/B only changed field; full training calls/tokens/seconds/peak RAM; before/after ordinary generation; correct/critical/abstain/coverage; pair logits; independent blinded scorer; exact commands; linked receipt; verdict`. 

Verdict: `DIFF_CONFIRMED_EFFECT_NOT_MEASURED`, `OUTSIDE_FAILED_RP01_PATH`, `MECHANICAL_FAILURE`, `EXPLORATORY_CONSUMER_GAIN`, `EXPLORATORY_LEARNING_GAIN`, `NO_GAIN_AT_BUDGET`, `INCONCLUSIVE`, `BLOCKED_ADMISSION`. Даже удачный tiny A/B **не является статистически достаточным подтверждением общей эффективности**: повторить на другом semantic family/seed и независимом review прежде чем утверждать root cause.

**STOP:** один важный источник не подтверждён, unknown author permission, отсутствие маленького законного training API, impossible fair split, wrong masked target, original reference mutates, heldout leakage, invented evidence. Нельзя заменять эти STOP очередью Windows CI или повышением числа DPO строк до 500.
