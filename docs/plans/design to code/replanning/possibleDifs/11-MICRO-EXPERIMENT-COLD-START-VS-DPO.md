# 11 — Мини-эксперимент RP01: DPO-only против короткого SFT→DPO
**Дата:** 2026-10-16 · **Статус:** SPEC_READY_FOR_IMPLEMENTATION_REVIEW / NOT_RUN / NOT_INDEPENDENTLY_APPROVED · **Связанные DIF:** 01, 04, 05, 06, 09, 10, 11, 20, 22, 23, 39, 41 · **владелец:** RP01.REC02 + RP01.REC03.

## 1. Одна проверяемая гипотеза, а не пять исправлений разом

**H-COLD:** Нынешний RP01 не научился свободно отвечать главным образом потому, что DPO и q/v LoRA применили к случайной B0-модели без сформированной способности продолжать осмысленный текст. Если перед DPO дать **короткое, контролируемое supervised обучение на правильных ответах**, то при равных исходных данных, параметрах и примерно равном compute появится измеримо более качественный **непоказанный раньше** ответ.

**H0:** такая замена предобучающей стадии не даёт полезного прироста на заранее закреплённом наборе; первопричину надо искать в DPO-gradient, данных, tokenizer, decoder, capacity или обычном consumer.

**Что реально установлено до эксперимента:**
- [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) сообщает: B0 случайная, C#500 `0/3→0/3`, ENRU500 `0/4→0/4`; change weights и small margin не привели к полезному free-answer gain.
- [generation-v1.json](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/config/code-documentation/generation-v1.json#L7-L49): vocab629, hidden512, 4 layers, rank8, alpha16, dropout0, q/v. **Не** менять эти параметры в данном опыте.
- [CodeDocumentationSimulationDpoExperiment.ValidOptions](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L224-L230): current `calibration` ровно **1** pair/step, `main` ровно **500**; CLI [Program.cs](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/tools/Rp01SimulationDpo/Program.cs#L15-L23) имеет pinned hashes. **Команды для 8/12/16 мини-пар нет.** Ниже — поручение *сначала* создать легальный диагностический вход, не пример уже работающей команды.
- [Trainer.TrainSupervisedNextTokenStep](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/04.TrainingLoop/Trainer.cs#L61-L147) — реальная существующая SFT-подобная next-token CE операция. Обучает **полную последовательность prompt+response** и не объявляется response-only SFT. [ProductionSupervisedTrainingStepExecutor](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/SupervisedTrainingExecution.cs#L19-L31) делегирует в этот метод. Переиспользовать только через подходящий trusted session/permit, не вызывать низкоуровневую операцию в обход lifecycle.
- [DpoTrainer.StepVector](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L832-L925) — существующий точный DPO-gradient/AdamW; [RP01 recipient+checkpoint](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L127-L208) уже фиксирует freeze/reference/save-readback. **Новый DPO trainer не создавать.**

**Граница:** эксперимент проверяет **эффект стратегии warm-start**, включая её последовательность и версию reference. Он не изолирует отдельно «веса base pretrained», «насколько хорош LoRA rank» или «смысл vector features». Для них — [12, микротесты](12-DIF-01-44-CHEAP-CAUSAL-PROBES.md).

## 2. Самый короткий дизайн: 2 training arms + бесплатная промежуточная SFT-only точка

| | **A — CONTROL** | **B — TREATMENT** |
|---|---|---|
| Исходное состояние | независимая копия **того же** B0, tokenizer, LoRA init/seed | независимая копия **того же** B0, tokenizer, LoRA init/seed |
| Первая мини-стадия | 1 bounded optimizer DPO-step на первом неповторяющемся блоке T0 | 1 bounded supervised next-token CE step на **chosen** из T0; тот же prompt+response |
| Промежуточная точка | A1 policy, score на DEV; base frozen | **B1 = SFT-only** policy, score на DEV; base frozen |
| Опора для заключительного DPO | frozen reference = собственный A1 | frozen reference = собственный B1 |
| Вторая стадия | 1 одинаково ограниченный DPO-step на новом блоке T1 | 1 одинаково ограниченный DPO-step на **том же** T1, если controlled-arm dataset policy разрешает это |
| После | A2 fresh checkpoint + full read-only eval | B2 fresh checkpoint + full read-only eval |
| Что сравнивается | A2 против B2; дополнительно A1/B1 и B1/B2 | одинаковые requests, decoding, semantic rubric, budget |

**T0/T1:** два **непересекающихся** блока по 4–8 *новых*, содержательно различных, допущенных пар; seed, длины, split и порядок заранее зафиксированы. Одна пара **никогда** не применяется дважды в пределах одной training lineage; старые C#500/ENRU500 вообще не используются для optimizer. Сравнение на *одних T1 в независимых research arms* требует **явного controlled-arm разрешения** от существующих source/admission/no-repeat contracts. Если оно отсутствует, **не запускать под разными ID обойдённые пары**: зафиксировать `BLOCKED_PAIRED_ARM_ADMISSION` и подготовить однократное изменение допустимого исследовательского контракта, а не подменять его разными training-данными и называть результат чистым A/B.

**Справедливость:** A и B имеют по **2 optimizer calls**, но алгоритмы CE и DPO имеют разную вычислительную стоимость/количество обработанных target-токенов. Нельзя писать «равный compute» без фактических CPU/wall/token measurements. Для точной экономической оценки: `compute_per_new_correct_law`; если бюджет отличается, явно записать mismatch, а затем, при необходимости, провести *одну* ограниченную сопоставимую по бюджету корректировку. Не менять B0, vocab, LoRA rank, dataset content, seed, decoder или evaluator одновременно с objective первой стадии.

## 3. Материал и независимый экзамен

**Pilot-domain:** сначала **один** (C# code documentation). Выбрать 8–16 коротких уникальных разрешённых *source problems*; смысловые семейства разбить до создания/чтения answers:
- **TRAIN:** 8–16 пар из 4–8 семейств; T0/T1 разделены по sample ID и source family, отсутствие дублей/near-duplicates.
- **DEV (diagnostic):** 4–6 новых семейств, короткие code snippets, с заранее известным статическим/компиляторным смыслом. DEV служит для раннего STOP, никогда не превращается в TRAIN.
- **HELD-OUT (report-only):** 8–12 новых запросов минимум из 4 иных семейств. Содержание закрыть до freeze рук A2/B2; этот набор **не** используется для выбора tokenizer, hyperparameters, warmup steps и prompt.
- **RETAIN:** 2–4 чужих контраста/старого навыка, только если до старта он действительно выполнялся; иначе `RETENTION_NOT_MEASURABLE`, а не сохранённые 0/3.

**Пример domain-law без записи в training dataset**: исходное `int x=1; int y=x++;` — верный ответ различает `y=1` и `x=2`; плохой объясняет `y=2`. В другом semantic family — nullable/flow, в третьем — short-circuit, в четвёртом — collection mutation. Это **иллюстрации** для reviewer, не тайный наполнитель набора и не подтверждённые teacher outputs.

**Роли и авторство:** в официальной программе CODEX/Astra выбирает урок, разрешённая открытая китайская модель пишет *обе финальные стороны*, CODEX/Astra отдельно оценивает exact pair. Если для изоляции механики используется явно разрешённый **synthetic research** режим, обозначить `DEVELOPMENT_SYNTHETIC_NOT_PROMOTABLE` и не выдавать его за внешнюю модель. Проверить текущие юридические/контентные допуски до любой тренировки. Не использовать раскрытые C#500/ENRU500 frozen exam panels как новые held-out.

## 4. Что измерять и какой вывод делать

**Первичная метрика:** число **правильных и формально допустимых ответов** на одинаковых HELD-OUT source problems: `correct_valid / all_cases`. Отдельно считать `critical_wrong`, `abstain`, `coverage`. Оценка по реальному `TextGenerator.GenerateDetailed` после **fresh-load** каждого A2/B2, c одинаковыми точными prompt bytes и `temperature=0`, `topK=0`, `topP=1`, `maxNewTokens` pinned.

**Вторичные:** chosen/rejected response-only log-odds, masked response CE, loss/gradient-norm (TRAIN), first meaningful token distribution, correct C# syntax/semantic law, reason for invalid reply, `StopReason`, body length, tokenizer IDs/prompt length, reference unchanged, base unchanged, adapter delta, wall/CPU and peak process RAM. Никакого `margin↑ ⇒ learned` и `changed weights ⇒ passed`.

**Blind grading:** reviewer видит для каждого case два ответа в случайном A/B порядке и закон задачи, но не arm, chosen/rejected labels, training loss или гипотезу. Фиксирует: `CORRECT`, `WRONG`, `ABSTAIN`, `INVALID`, `UNJUDGEABLE`, critical failure, supporting language/AST result. Для нового подтверждения нужен независимый CODEX/Astra review; если он не выполнялся, verdict только `EXPLORATORY_AUTHOR_SIDE`.

**Предварительный сигнал, НЕ доказательство:** B2 исправляет ≥2 из 8–12 заранее выбранных HELD-OUT cases, не увеличивает critical failures, и B1 vs B2 показывает, что DPO не уничтожил SFT-пользу. Это trigger следующего независимого replicate на **новых** families / second seed; не авто-promotion. **Отсутствие такого эффекта после 2 шагов ≠ строгая фальсификация всей идеи pretraining**: возможен недостаточный бюджет, неправильный SFT recipient или плохо выбранный law. Правильный статус `NO_EARLY_SIGNAL_AT_DEFINED_BUDGET` либо `INCONCLUSIVE`, не `SFT_DOES_NOT_WORK`.

**Возможные исходы (предопределены):**
1. **A не overfits TRAIN, B не overfits TRAIN:** STOP; сперва gradient/mask/optimizer/recipient (REC02), не обещать, что SFT решит проблему.
2. **B1 научился TRAIN CE, A1 нет; B2 больше HELD-OUT correct:** поддержка H-COLD как *раннего сигнала*, обязательно повторить на новых families/seed. Не claim production readiness.
3. **A2/B2 научились TRAIN odds, free HELD-OUT оба плохи:** данные/структурная обобщаемость/decoder/capacity; не лечить большим 500-row DPO.
4. **B1 хорошо, B2 хуже:** DPO разрушает supervised gain — проверить beta/reference, T1 pair quality, length confound, forgetting.
5. **Student B2 хорошо в этом пробнике, ordinary Unit/Flow output не меняется:** bottleneck serving/adoption, следующий шаг RP03, не DPO.
6. **Невозможен разрешённый tiny run:** `BLOCKED_BY_ADMISSION_OR_MODE` с точным вызовом/строкой; не подменять результат экспертным прогнозом.

## 5. Минимальная реализация (без фиктивных команд)

**0 — READ-ONLY (ни одного optimizer call):** проверить source hash, model/tokenizer SHA, наличие/отсутствие `SFT` training owner, exact prompt segmentation, response mask, параметры AdamW, допустимый минимальный CLI. Использовать существующие `inspect` и `preflight` только согласно реальному signature в [Program.cs](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/tools/Rp01SimulationDpo/Program.cs#L36-L80). Прогнозируемый wall записать после одной калибровки, не придумывать минуты.

**1 — ONE SCOPED EXPERIMENT ADAPTER, только если необходим:** добавить в offline research tool *новый* явно названный режим (например `diagnose-small`, **ЭТО ПРЕДЛОЖЕННОЕ ИМЯ, НЕ ТЕКУЩАЯ КОМАНДА**), с `1…16` read-only hash-pinned records, `≤2` optimizer calls per arm, `≤2` arms, explicit source permit; B0 read-only input; unique runID per branch; `DpoTrainer` прежний; `Trainer.TrainSupervisedNextTokenStep` только через существующий authorized session или новую scoped lease. Публикация — **private experiment candidate**, не production accepted pointer; никаких автоматических повторов. Если существующий trainer SFT full-sequence, **не называть это conditional response-only warmup**. Либо согласовать SFT objective и маску как отдельное именованное изменение — и не менять ещё что-либо.

**2 — SAVE/RELOAD:** для каждой ветки проверить fully keyed LoRA/base, optimizer/RNG/ref identity, exact output logits match `in-memory == cold-loaded`, исключить stale serving pointer. Если штатный research writer не поддерживает SFT→DPO optimizer handoff, запланировать точный метод/codec/lease, а не придумывать наличие поддержки.

**3 — EVALUATE:** сохранить raw decoded outputs и blind per-case judgement на новых heldout; primary paired count и отдельно wall/resource. Не проводить Windows Actions, полный repository test suite, 500-pair rerun. Допускается **один** локальный focused build и 1–2 smoke checks, если изменён экспериментальный адаптер.

**Бюджет и STOP:** максимум 4 совокупных training optimizer calls (2×2), плюс ≤2 bounded read-only generation panels и свежая загрузка. При первой ненулевой train-CE неправильной производной/NaN, изменении base/reference, неподтверждённом source permit или масках STOP до второй стадии. Если модели не хватает памяти/времени — сохранить статус + measurable cost, не увеличивать лимит втихую. **Отдельное согласованное увеличение — отдельное решение.**

## 6. Минимальный выходной артефакт (шаблон, НЕ фактический результат)

```json
{
  "experiment_id": "RP01-COLD-A-B-v1",
  "status": "NOT_RUN",
  "source_head_sha": "TO_CAPTURE",
  "reference_b0_sha": "TO_VERIFY",
  "dataset_sha": "TO_CREATE_WITH_ADMISSION",
  "arms": ["A:DPO_FIRST_then_DPO", "B:SFT_FIRST_then_DPO"],
  "train_family_ids": [],
  "dev_family_ids": [],
  "heldout_family_ids": [],
  "paired_arm_permission": "NOT_VERIFIED",
  "budget_max_optimizer_calls_total": 4,
  "a_correct_valid": null,
  "b_correct_valid": null,
  "n_heldout": null,
  "critical_a": null,
  "critical_b": null,
  "training_cost_seconds_a": null,
  "training_cost_seconds_b": null,
  "gradient_mask_reference_valid": null,
  "fresh_reload_logits_equal": null,
  "independent_review": "NOT_PERFORMED",
  "verdict": "NOT_MEASURED"
}
```

## 7. Текст готового поручения исполнителю

> **CURRENT TASK:** реализовать минимальную проверку **DIF-01 / H-COLD** как ограниченное дополнение к `RP01.REC02` и `RP01.REC03`, не менять исходные frozen RP01 findings и другие roadmap packs. **SOURCE FIRST:** original task + WorkLeft-RAG→REC02/REC03, `Program.cs`, `CodeDocumentationSimulationDpoExperiment`, `DpoTrainer.StepVector`, `Trainer.TrainSupervisedNextTokenStep`, `CodeDocumentationStudentGeneration`, checkpoint owners, `generation-v1.json`. **SKILLS:** `architect-behavior-classifier`, `experiment-design`, `code-examination`, `root-cause-tracing`, `phase-planning`, `plan-review`, `prompt-patch-authoring`, `human-readable-architecture-audit`, `verification-before-completion` from `docs/skills`; record loaded paths and exact role. **DO:** first report real implemented CLI vs proposed new entry; protect admission and no-repeat; freeze 2 independent arm states from exact B0; T0/T1 new authorized source, two mini updates per arm, actual fresh-load logits, frozen blind heldout and full wall. **GOOD:** changed valid-law answer on new input with same examiner and no critical regression; **BAD:** `margin↑`, `weights changed` or `SFT loss↓` called useful learning. **EDGE:** no legal paired-arm use; identical tokenized sides; no EOS; no SFT continuation codec; truncated response; remote provider unauthorized; stale checkpoint. **OUTPUT:** one evidence packet with links `file:line:sha`, exact commands actually run, statuses and experiment receipt. **STOP:** no allowed tiny mode or no admission → print precise blocker; never fake a successful run. Independent Astra review remains a separate future action, not a self-awarded PASS.

## 8. Важно не спутать с соседней гипотезой

**DIF-02 (TeachingVector activeSlots)** можно вначале проверить за **0 training steps**: координатная перестановка при том же количестве ненулевых слотов обязана оставить `VectorSignal` прежним. Но это показывает **неиспользование семантики координат**, а НЕ причинное влияние на способность генерации. В этом мини-эксперименте не менять `VectorSignal` вместе с SFT: иначе невозможно понять, что дало прирост. Отдельный cheap check для DIF-02 и остальных описан в [12](12-DIF-01-44-CHEAP-CAUSAL-PROBES.md).

**Публикационный verdict этого файла:** `PROTOCOL_DESIGNED; CODE_ADAPTER_NOT_IMPLEMENTED; NO_NEW_TRAINING; NO_NEW_QUALITY_CLAIM`.
