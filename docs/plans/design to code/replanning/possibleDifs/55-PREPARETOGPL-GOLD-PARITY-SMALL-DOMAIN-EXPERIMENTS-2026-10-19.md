# N55 / G55 — короткие причинные эксперименты по доменам, GOLD-first

**19.10.2026 — DESIGN_ONLY / NATIVE_RUN_NOT_EXECUTED.** Документ является готовым протоколом, не отчётом о запуске. Исходная золотая ветка: [PrepareToGPL `31346d8`](https://github.com/lubochka/llm_mvp_core/tree/31346d8928be4469b01aaf313780e8f950ff91e2), текущая реализация: [`6d36492`](https://github.com/lubochka/llm_mvp_core/tree/6d36492714ebc1b9ae5f31a5a1da00673f524d83). Исторические receipts берутся по ссылкам, **не создаются вновь**. Код и тесты `llm_mvp_core` этим пакетом не меняются.

## 0. Обязательная общая методика

Каждая проверка явно различает **тему обучения**, профиль задачи/язык, конкретный **получатель обучения** (Unit/Flow/Student), **механизм**, **судью** и **измерение**. Точность kNN, качество ответа продукта, preference margin DPO и число changed LoRA tensors — разные оси. Поля опыта: `ExperimentId, Baseline, Enhanced, Metric, Tier` по [experiment-design skill](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/docs/skills/experiment-design-SKILL.md). Для каждого опыта:

1. Закрепить `branch_sha, code_sha, checkpoint_sha256, data/corpus_sha256, tokenizer_id, unit_id, policy_version, adapter_id, seed` и source/legal admissibility; не подменять недоступное значение вымышленным. Сохранить входы и полные выходы *до* обучения.
2. Один фактор A/B, одинаковый train/heldout и последовательность задач; 2–4 настоящих edge+negative controls. При обновлении памяти `old cases` и `new cases` проверять отдельно.
3. Малая обучающая порция → **самостоятельно перезапущенный процесс**, загрузка **с диска**, **обычный потребитель** ответа. Прямой Unit-only — только диагностика; показатель Student нельзя переносить с Unit.
4. Заранее закреплённая семантическая разметка правильности, включая «не знаю» / bootstrap / неполный ответ. Отдельно `incorrect`, `abstain`, `accepted-but-wrong`, `rejected-correct`. Судья не меняется по ходу эксперимента.
5. Минимальные safety/contract проверки допуска и сохранения состояния остаются; **не запускать full Windows/GitHub matrix вместо измерения обучения**. Качество не объявлять доказанным по одному deterministic proxy.

### G55-00 / P0 — загрузить GOLD-состояние и получить реальный ответ (перевод)

**Эксперимент:** `G55-00-ENRU-MEMORY-GOLD-PARITY`; **Tier 1**. Тема — перевод; профиль EN→RU. **Получатель:** `TranslationMachine → ExecutorUnit / TranslationKnnStore → TranslationRoutingFlow`, а не random B0-DPO Student. **Механизм:** ранее сохранённые word/phrase/KNN patterns + cold restore.

- **Baseline A:** GOLD frozen checkpoint через GOLD `LoadCheckpointAsync` в fresh process; один набор 8 разрешённых структурированных уроков (если применяется повторная подготовка, строго отделить checkpoint-only от retrain).
- **Enhanced B:** тот же исходный checkpoint, те же разрешённые уроки, current из чистого процесса; **без изменения DPO/LoRA**, с явной версией inference policy и только после подтверждённой совместимости loader.
- **Перед смысловым сравнением:** проверить миграцию 4→6 executor directions **как отдельную инженерную предпосылку**. Если current возвращает `EXECUTOR_DIRECTIONS_INCOMPLETE`, результат `BLOCKED_LOADER` и **никаких 0/24 вместо «качества»**. Миграция сохраняет четыре старые обученные направления, новые направленности создаёт изолированно; не ослаблять legal/manifest gates.
- **Панель:** сохранённая 24-case P125 (8 teaching examples + paraphrase + новые составления), дополнительно 4 отрицательные/полярные контрольные пары; последние считают отдельно, чтобы не переписать знаменатель P125.
- **Метрики:** правильность при ordinary Flow /24 до/после; old retention /24; accepted-but-wrong; cold-vs-warm same-input parity; `adapter_id`; числа Knn/phrase/exact hits, время/запрос, RAM.
- **Фальсификация:** одинаковые ответы GOLD/B/current при неизменном checkpoint и одних inputs отвергают именно регрессию на этой панели. Рост Unit hit без улучшения ответа не считается восстановлением.
- **Предыдущая положительная опора:** [P125 отчёт](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md) 0/24→19/24, [N41 loader](41-FOUR-VS-SIX-TRANSLATION-CHECKPOINT-MIGRATION-2026-10-18.md).

### G55-01 / P0 — новый source-law действительно улучшает ответ?

**Эксперимент:** `G55-01-ENRU-POLARITY-SELECTOR`; **Tier 1**, только после G55-00 when compatible; **N55-P1**. Тема — перевод EN→RU; получатель — `TranslationKnnStore` и отдельно ordinary `ExecutorUnit/Flow`.

- **Вход фиксируется:** signature с явно не-low-information tier `"SVO|TEST|COPULAR|POLARITY"`; запись `("the box is open", "коробка открыта")`, query `"the box is not open"`. В первом механическом проходе A = GOLD `FindNearestCascade(...k=1, querySourceText=query)`, B = CURRENT на той же записи. Посмотреть actual candidates, а не количество tests.
- **Контроли:** query `"the box is open"` должен сохранить известный positive; `"the box is closed"` — UNKNOWN (не считать правильным open); `"the box isn't open"` и `"the door is not open"` раскрывают границы parser/scope. Не выдавать невозврат candidate за правильный перевод: допускается честный abstain.
- **Настоящий learning-pass:** в одинаковых fresh Unit экземплярах разрешённо обучить 1–2 source/target pairs; A GOLD default, B CURRENT default **или** текущая сборка с отключённым только source-law через управляемый test seam. Ни regex, ни candidate list, ни Unit routing больше не менять.
- **Результат:** на 8–12 заранее размеченных polarity cases считать `wrong-positive/total`, `correct/total`, `abstain/total`, cold-load parity и selected AdapterId. Если law меняет kNN result, но ordinary answer не меняется — **P2 path shielding**, а не повышение качества.
- **Доказательство:** [GOLD/current diff и границы новой гипотезы](55-PREPARETOGPL-POLICY-IDENTITY-AND-CONSUMER-BOUNDARY-2026-10-19.md).

### G55-02 / P0 — проверка precedence и полноты learned-пути (перевод, несколько режимов)

**Эксперимент:** `G55-02-ENRU-KNN-VS-PHRASE-REACHABILITY`; **Tier 1**, N55-P2. Обучаемый получатель — `ExecutorUnit`; одна фиксированная training pair/heldout полярности. **Не сравнивать другую LoRA.**

- **A:** изолированно тренировать только kNN signature/exact candidates, наблюдать каскад с source-law.
- **B:** на той же KNN памяти включить разрешённые *проверенные* phrase/word mappings (один фактор: ранняя candidate availability), ordinary `Execute`.
- **Фиксировать:** первый сработавший путь exact-source / phrase / word / KNN, возвращённый текст, verifier pass, confidence, итоговый fallback и **семантический verdict** для каждого из 8 положительных/отрицательных случаев.
- **Контроль:** phrase mapping, где перевод должен быть верным, не должна теряться при новом law; negation-opposite pair не должна возвращать старую положительную фразу только потому, что source-law живёт ниже первого return.
- **Успех:** меньше ошибочных обычных ответов, старые правильные не потеряны; дополнительные расходы/latency записаны. При недостижимости ранней ветки на этих реальных данных N55-P2 остаётся только source-level риском.
- **Ограничение новизны:** общий риск wrong word-map уже известен [N35](35-NEW-NEGATION-OMISSION-WORDMAP-TO-099-CONFIDENCE-2026-10-16.md).

### G55-03 / P0 — структурный kNN GOLD E09 в C#-потребителе

**Эксперимент:** `G55-03-CSHARP-E09-RESTORE`; **Tier 1**. Тема — программирование/документация, профиль C# AST; получатель — исторический learned E09 и реальный code/documentation consumer, не переводчик.

- **A:** нынешний lexical/vector-only или другой фактически вызываемый KNN на **том же** наборе исходников.
- **B:** тот же source/label codec, но learned structural AST features E09 включены в реальный ranker. Не копировать отрицательные выводы E07 matrix на E09 learner.
- **Панель:** 12–20 source-family-disjoint методов, включая `x++` vs `++x`, `if`/nested-`if`, инверсию guard, `validate → save` vs `save → validate`. Для каждого заранее прописать верную семантическую характеристику и, если применимо, документацию действия.
- **Метрики:** `top1@same-denominator`, `downstream correct descriptions / N`, confusion по редким семействам, median latency. Отдельный контроль с переменными-синонимами не должен менять смысл. Не переносить арифметику E09 0.2413→0.3507 как matched A/B: [N44](44-RECOVERY-FIRST-EVIDENCE-AND-PRIORITY-2026-10-19.md) предупреждает о разных denominators.

### G55-04 / P0 — реально ли малый DPO/LoRA Student учится, а не меняет веса?

**Эксперимент:** `G55-04-STUDENT-SIGNAL-VS-FREE-OUTPUT`; **Tier 1**. **Тема/профили раздельно:** C# semantics, EN→RU. **Получатель:** native RP01 `SimulationDpo... Student`. Историческая память `ExecutorUnit` служит **положительным контролем**, но не «baseline same model».

- **A:** нынешний random/unpretrained B0 + admitted DPO only; **B:** тот же допустимый DPO, но начальный state с доказанной tiny SFT/warm capability. Один фактор — **начальная компетенция**; не менять одновременно tokenizer, LoRA rank, reward, decoder, chosen/rejected.
- **Данные:** 8–16 настоящих качественных пар (не 16 перефразировок 1 шаблона), 4–8 family-disjoint heldout, forbidden teacher labels в heldout не используются. После tiny update выполнить 1 train overfit diagnostic и отдельную blind free generation, затем cold restore.
- **Метрики:** correct free answers /N, directional counterfactual sign, `chosen/rejected` absolute logP, margin, nonfinite/grad counts, token coverage, cold-replay exactness, CPU/RAM/GPU. Нужен рост правильного ответа, не только `weights changed`. Если есть только 0/N у A и B, не объявлять improvement.
- **План/архивный негативный baseline:** [RP01 learning recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md). Материалы допуска DPO — [родительский RAG](../../09.09.26WorkLeft-RAG-01-dpo-legal-build.md); не возвращать raw/unapproved source.

### G55-05 / P1 — обучаемые категории Design-to-Code и реальный браузерный ответ

**Эксперимент:** `G55-05-DTC-FEEDBACK-TO-HTML`; **Tier 1**. Тема — дизайн→HTML/CSS; получатель — `DesignConversionMachine` learned correction weights; отдельный neural Critic/Improver **не смешивать**. Использовать существующий [GDC-A0/A1](54-DESIGNTOCODE-CLEAN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md), не повторно заявлять novelty.

- **A:** current admitted canonical HTML/CSS pair без разрешённого typed correction-category; **B:** та же пара + разрешённая authority-bound category, меняется только delivery учебного *признака*, а не визуальный ground truth.
- **Панель:** 4 frozen макета: wrong spacing, missing element, wrong contrast, two concurrent defects. Независимый Render-QA судья: доля реально исправленных известных дефектов /4, сохранение прошлого правильного, CSS validity, cold restore.
- **Предпосылка:** для реально показанных улучшений должен вызываться ordinary `DesignConversionMachine`, не иной mocked wrapper; пока не прочитан actual trace — не делать product claim. Old trusted score сам по себе мог не менять веса, как отмечено [в master](00-MASTER-SYNTHESIS-AND-CONFLICTS.md).

### G55-06 / P1 — learned orchestration, а не сохранённый JSON

**Эксперимент:** `G55-06-ORCH-RECEIPT-TO-CONSUMER`; **Tier 1**. Тема — оркестрация. Получатель — `OrchestrationPlanMachine` learned modes. Следовать имеющемуся [OE-X01/OE-X02](00-ORCHESTRATOR-EXTENSIONS-GOLDEN-FIRST-2026-10-19.md), не повторять уже историческую оценку.

- **A/B:** exact donor checkpoint directly loaded → ordinary current consumer на 6 новых стилях. Один фактор внутри B — binding learned Unit вместо generic fallback; не обучать новый LLM параллельно.
- **Метрики:** correctness order/dependency /6, learned route choice /6, output state refs, cold parity, answer evidence, overhead; mock-only успех не засчитывается.

## 1. Как оформлять доказательство

Один артефакт результата на запуск:
```json
{
  "experiment_id": "G55-01-ENRU-POLARITY-SELECTOR",
  "status": "NOT_RUN",
  "gold_sha": "31346d8928be4469b01aaf313780e8f950ff91e2",
  "current_sha": "6d36492714ebc1b9ae5f31a5a1da00673f524d83",
  "recipient": "TranslationKnnStore / ExecutorUnit / TranslationRoutingFlow",
  "subject_domain": "translation",
  "profile": "EN->RU, polarity contrast",
  "teacher_admission": "not_applicable_or_verified",
  "baseline_state_sha256": null,
  "enhanced_state_sha256": null,
  "policy_version": null,
  "heldout_frozen_hash": null,
  "ordinary_correct_A": null,
  "ordinary_correct_B": null,
  "accepted_but_wrong_A": null,
  "accepted_but_wrong_B": null,
  "old_retention_A": null,
  "old_retention_B": null,
  "time_ms_A": null,
  "time_ms_B": null,
  "limitations": ["No native experiment has yet been run"]
}
```
Оставлять `null` для неизвестных измерений. **NO PASS без точного native output и независимой проверки**.

## 2. Короткий executor prompt и проверка человеком

**Для исполнителя (после отдельного разрешения запусков):** «Прочитай original task + WorkLeft RAG (primary refs), `architect-behavior-classifier`, `reference-inventory-and-source-map`, `plan-review`, `code-examination`, `experiment-design`, `human-communication-GLOSSARY`; проверь текущий HEAD. Выполни **только один G55** по фиксированному коду и набору. Назови получателя, модель, domain/profile, законные источники, источник решения chosen/rejected. Сохрани raw answers, checkpoint hashes, selected candidate path, отрицательные контроли, semantic verdict. Не меняй frozen labels после результата, не запускай full Windows matrix для доказательства learning gain. Изменения core/docs/plans/replanning-possible-diffs/** запрещены». 

**GOOD:** «GOLD 5/8, CURRENT 7/8 на одном семантическом heldout; raw outputs и обычный Flow подтверждены, исправлен конкретный механизм. Это иллюстрация желаемого формата, **не результат**».  
**BAD:** «kNN returned 8 hits / LoRA weights changed / CI green → обучение улучшилось».

**План-review:** FAIL, если A/B поменяли два независимых фактора, denom/teacher/labels различаются, профиль перевода объявлен новым доменом, `PassesGate` превратили в semantic score или успех unit-only объявили общим продуктовым успехом. При таком FAIL ремонтировать эксперимент, а не подгонять границы приемки.
