# N60-04 — объединение результатов параллельных агентов, проверка публикации и граница полноты

**19 октября 2026; RECONCILED DOCUMENTS / SOURCE_READBACK_OK / ZERO NEW TRAINING.** Защищённый core/docs/plans/replanning-possible-diffs/ НЕ МЕНЯЛСЯ этой веткой работ. Этот файл нужен не для ещё одной «новой» гипотезы, а чтобы сохранить ссылку на независимые findings, **не переименовать чужие результаты в свои** и не спутать coverage с результатами обучения.

## 1. Readback новых N60 документов из GitHub на ветке llm_plan3

Четыре новых файла были созданы через GitHub и затем отдельно прочитаны из той же ветки; все 4 вернули полноценный UTF-8 Markdown, не только commit receipt.

| Файл | Что содержит | git blob SHA проверки |
|---|---|---|
| [60-00 gold→RP01](60-00-RP01-SUCCESS-LINEAGE-AND-GAP-ATLAS-2026-10-19.md) | Evidence map: successful memory, E09 Tier1+Tier2 denominator flaw, E14, model/Unit separation | 241fd4dad0d09b7214fd869b90b3dd05f0ef40c4 |
| [60-01 hypotheses](60-01-NEW-CURRICULUM-FEEDBACK-AND-UNIT-MEASUREMENT-SEAMS-2026-10-19.md) | **H60-A** historical-best hysteresis и **H60-B** admission timing: proved source path to actual TrainRowsAsync, NOT measured improvement; side note Unit metric bypass | ee3227f771ea9f5bd31f47527c1733c6f9983715 |
| [60-02 experiments](60-02-REAL-LEARNING-MICROEXPERIMENTS-2026-10-19.md) | Mini A/B before-after, EN→RU and RU→EN, explicit teaching and ordinary Flow, conditional AST/neural comparisons, complete executor prompt and human approval | 3f17cfa50ae864a805f8566e8f813b178861aaae |
| [60-03 protected audit](60-03-PROTECTED-LIBRARY-READONLY-COVERAGE-2026-10-19.md) | Sample semantic audit and exclusion zero-train, coverage gap including newest shadows, not claiming 470/470 | 04a9eb0ede7937859852659d1e632fd18ac98e0c |

No new native test or training run, new QA metric or new model checkpoint has been created by those four writes. GitHub commit receipts: a1b480a, 398c922, 8964fe5, 92fe518, respectively; they are planning documentation, not code modifications.

## 2. Не дублируемые результаты других агентов, обнаруженные при readback того же possibleDifs

| Независимый опубликованный пакет | По факту добавлено / что уже покрывает | Статус |
|---|---|---|
| [N60-A: independent 125-card compliance](60-INDEPENDENT-125-CARD-COMPLIANCE-AND-VALIDITY-2026-10-19.md) | Полный source-text обход 125/125 P-карточек; в 125/125 есть правило любого measured gain (в т.ч. без accuracy), требование learner/state и исполнительские формулировки. **Не утверждает** семантическую валидность всех 470 cases | Independent STRUCTURAL AUDIT DONE, native 0 |
| [N60 protected readability and validity](60-PROTECTED-EXPERIMENT-READONLY-QUALITY-AND-COVERAGE-2026-10-19.md) | То же 125/125 structural keyword lint; маркирует subcases, которые остаются serving-only diagnostics; источник для честного X=OUT_OF_LEARNING_SCOPE | Structural, selected semantic, NOT FULL scientific review |
| [N60 full training-path package](60-TRAINING-PATH-AND-EXPERIMENT-VALIDITY-2026-10-19/README.md) | **ND-DPO-SFT**: DPO потом C# SFT на общих адаптерах; **ND-DPO-PAD**: padded forward/backward на нерелевантных позициях, потенциальная цена; **N-KU-01**: E14 ADD_LOOP labels ошибаются на Roslyn ForEachVariableStatement/DoStatement. В пакете есть настоящие before/after training A/B protocols и истоки source | Source-confirmed mechanisms, quality impact NOT_RUN. Это **другой кодовый путь**, не доказанный корень RP01 |
| [N60 interaction experiments](60-NEW-INTERACTION-HYPOTHESES-AND-DOMAIN-EXPERIMENTS-2026-10-19.md) | Cross-language homograph correction, два lesson/Executor learning-state continuity, feedback-poisoning; явно указал старые prior art и conditional/blocked подслучаи | Не повторять в N60-01 как новые |
| [N60 verifier/phrase composite](60-COMPOSITE-CAUSAL-HYPOTHESES-VERIFIER-PHRASE-SKILL-DRIFT-2026-10-19.md) | Неатомарность отрицательной пары и verifier rejection при ordinary receipt; связывает P52/P54/P55/P56/P125 | HYPOTHESES, NOT RUN |
| [N59 cross-component continuation](59-03-NEW-INTERACTION-HYPOTHESES-AND-FAST-LEARNING-CARDS-2026-10-19.md) | Второй урок на положительном P125, безопасность отрицания, AST KNN control и scheduler→trainer honesty | Prior existing, нельзя считать заново обнаруженным |

**Находки этого пакета, отличимые от перечисленных:** N60-01 H60-A и H60-B формулируют ТОЛЬКО (a) эффект исторического максимума и (b) order of feedback→admission на реальном Tatoeba learning cycle. Это не новое обнаружение ProductMatch scorer (P58.4), не базовые P51/P106 language keys и не P125 proof. Новизна глобально по всем 836/904+ историческим артефактам НЕ установлена; сформулирована лишь уникальная, проверяемая причинная связка относительно известных соседних source findings.

## 3. Особо важные доказательные ограничения

**RP01:** изменение LoRA параметров и margin — измеренные показатели optimiser. Одновременно свободные ответы C# 0/3, EN→RU 0/4 без улучшений в отмеченной панели. Не называть «система вообще не учится»; не приписывать P125 memory success RP01 neural.
**E09:** 0.3368→0.3472 Tier1 — локальный успех accuracy с очень большим ухудшением time. 0.2413→0.3507 Tier2 нельзя считать causal +45% пока baseline и enhanced используют разные размеры validation.
**P125:** 0/24→19/24 correct/all после 8 уроков на настоящей memory Unit, 3 wrong accepted. Это сильная индикация обучения на конкретном EN→RU наборе, но не перенос на весь model stack.
**Protected library:** 125/125 формальных карточек и prompts имеют user success rule; нельзя автоматически сказать, что каждый Pxx.* включает real training. Примеры zero-update в исторических дополнениях являются диагностиками и не подлежат самостоятельным учебным запускам. Поздние revisions core (f50) уже маркируют/исключают часть таких runs, поэтому не представлять закрытый вопрос как ещё действующий production дефект.
**Метрики:** любое фактически измеренное полезное улучшение в подходящей теме/профиле = локальный успех; accuracy не требуется. Другие оси и ущерб обязательно рядом. Zero update не выдавать за learning speed gain, не делать искусственный no-op update для «доказательства».

## 4. Научный статус и следующий порядок без лишних токенов

- Proof/current callers + P125 positive evidence: VERIFIED from source/reports; cause of RP01 reduction: UNKNOWN.
- 125 cards structurally checked by **независимыми отчётами** (links above), но complete 470-case semantic audit and complete historical lineage: OPEN. Не заявлять иначе.
- **Приоритет 1:** N60-02 EXP-N60-A только если есть настоящая историческая quality decline относительно best, получены разные admitted lesson IDs и конкретный scorer входит в TrainRowsAsync. В противном случае сразу NOT_APPLICABLE.
- **Приоритет 2:** N60-02 EXP-N60-B только при lawful DEV feedback; sealed test leakage = NOT_ELIGIBLE.
- **Приоритет 3:** independent [ND-DPO-PAD](60-TRAINING-PATH-AND-EXPERIMENT-VALIDITY-2026-10-19/01-DPO-LORA-NEW-TRAINING-DIFFERENCES.md) — полезная учебная cost-axis optimization даже без роста accuracy, но только в настоящем DPO StepVector; historical P125 continuation — для обучения переводу.
- Не делать repo-wide Windows CI, unrelated report-only probes или повторение 125 уже оформленных схем ради закрытия чекбокса.
- Ни один experiment из этого документа не разрешён к исполнению; требуется отдельное одобрение полного versioned паспорта пользователем. Доказательство последующего native результата должно включать accepted lessons, Unit/state hashes, own outputs before/after, metric vector и reviewer SHA. 
