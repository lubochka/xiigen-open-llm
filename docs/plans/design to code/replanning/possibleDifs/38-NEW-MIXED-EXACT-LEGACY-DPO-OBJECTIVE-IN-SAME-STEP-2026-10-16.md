# N38-B — второй новый условный пробел: общий AdamW-шаг допускает пары с несопоставимыми objective

**16.10.2026 | SOURCE-CONFIRMED TRAINER-LEVEL CONDITIONAL ROUTE | NO NATIVE MIXED CALL | RP01 EXACT PROFILE PROTECTS CURRENT ROUTE.**

## Коротко для человека

У XIIGEN есть два разных представления DPO-урока: legacy считает **средний log-probability по последовательности**, exact-v2 — **сумму по ответным токенам**. В обычном generic StepMany код нормализует contribution по числу batch-ов, но **не запрещает** взять внутри одного AdamW-шага одну legacy-пару и одну exact-пару. Тогда изменение модели зависит от формата документа, а не только от содержания урока. Длинный exact ответ может оказаться неявно тяжелее короткого legacy даже при равных объявленных весах.

Это отдельная гипотеза о **смешанной кампании**, а не утверждение, что RP01 C#500 и ENRU500 были ошибочно смешаны. Подготовленный project-LoRA RP01 использует защищённый execution profile, который требует exact-v2.

## Первичная цепь и контрдоказательство current RP01

1. [DpoExactPairMaterial.Version/ObjectiveVersion](https://github.com/lubochka/llm_mvp_core/blob/aa284ba21790c5d7601328997cd6174c85cee878/11.DPO/Models/DpoExactPairMaterial.cs#L20-L28) — версия segmented-byte-BPE-v2, objective response-sum-softplus-signal-weighted-v3. [DpoBatch.MaterialVersion](https://github.com/lubochka/llm_mvp_core/blob/aa284ba21790c5d7601328997cd6174c85cee878/11.DPO/Models/DpoBatch.cs#L12-L29) по умолчанию legacy-full-sequence-mean-v1.
2. [DpoTrainer.TargetMask/BatchLogProbs](https://github.com/lubochka/llm_mvp_core/blob/aa284ba21790c5d7601328997cd6174c85cee878/11.DPO/DpoTrainer.cs#L1002-L1030) выбирают маски и sumTargets индивидуально для каждого batch.
3. [DpoTrainer.ComputeTargetLogProbs](https://github.com/lubochka/llm_mvp_core/blob/aa284ba21790c5d7601328997cd6174c85cee878/11.DPO/DpoTrainer.cs#L1033-L1069) возвращает sum для exact и mean для legacy.
4. [DpoTrainer.StepMany L701–765](https://github.com/lubochka/llm_mvp_core/blob/aa284ba21790c5d7601328997cd6174c85cee878/11.DPO/DpoTrainer.cs#L701-L765): предварительно CaptureBatch каждого элемента, RequirePairAdmissions, вычисление per-pair loss/gradient и единый ApplyOptimizer. В **самом generic StepMany** нет требования «MaterialVersion одинакова у всех». Функция HomogeneousObjectiveVersion только записывает null в отчёт, если mixed.
5. [DpoExactPairMaterial.CaptureAndValidate](https://github.com/lubochka/llm_mvp_core/blob/aa284ba21790c5d7601328997cd6174c85cee878/11.DPO/Models/DpoExactPairMaterial.cs#L92-L114): legacy и exact способны пройти локальную проверку по разным веткам, если их отдельные поля корректны.
6. **Важное ограничение:** [DpoExecutionProfile.Capture/RequireCurrent](https://github.com/lubochka/llm_mvp_core/blob/aa284ba21790c5d7601328997cd6174c85cee878/11.DPO/DpoTrainer.cs#L119-L151) требует exact, а [DpoTrainer.CaptureBatch](https://github.com/lubochka/llm_mvp_core/blob/aa284ba21790c5d7601328997cd6174c85cee878/11.DPO/DpoTrainer.cs#L991-L1000) сверяет этот профиль. Следовательно **current guarded project-LoRA candidate не допускает legacy**; generic тренер без профиля остаётся условной точкой входа, если внешняя mutation capability разрешит.

**Связь с прежним аудитом:** [N16-C](16-TRANSLATION-DPO-MODEL-VISIBLE-PAIR-COLLAPSE.md) уже описывает **разные математические цели в двух маршрутах**. **Новое здесь** — проверяемое допущение обеих целей **в одну StepMany transaction** и бессмысленность усреднённого cross-objective loss с null ObjectiveVersion, если generic consumer достигаем. Не переименовывать известную N16-C в новое открытие.

## Малый пример масштаба (аналитический, не native)

У двух мысленных одинаково корректных пар positive preference log-probability delta = 0.02 **на каждом из десяти ответных токенов**, reference fixed и одинаковая beta. Exact response-sum даёт общий ответный delta 0.20, legacy mean — delta 0.02, если 10 сравнимых позиций имеют то же delta. Соответственно same nominal per-batch gradientScale=1/N не делает эти пары сопоставимыми. В реальной модели общие параметры и logit coupling усложняют вектор градиентов — это пример **возможного масштаба**, а не измеренный X10 gain.

## Дифференциальный тест

- G0: две exact пары, один заранее допустимый recipient, одинаковое число токенов и одна и та же семантика; ожидаемый homogeneous objective.
- G1: одна exact и одна legacy пара с согласованным смысловым контрастом; фиксированные seed, reference, learningRate, total examples и одинаковая формальная train permit; **проверить, какой именно путь отказывает до мутации**. Для current RP01 ожидаемый вердикт PROFILE_REJECTS_MIX (успех защитного механизма), а не качество модели.
- G2: generic permitted trainer в test-only harness с профилем null (только если это разрешено внешним admission-contract): измерить batch loss per objective, norm per-pair gradient, relative share, checkpoint hash после одного шага. Если generic route недостижим обычным продуктом — LATENT_NOT_REACHED.
- G3: отдельно exact-only контроль для C#, ENRU и их смеси при frozen tokenizer compatibility; измерять semantic heldout по доменам, не loss общего масштаба.

**Falsifier:** если upstream всегда создаёт однотипные batches и generic training path не используется/не разрешён, этот механизм **не объясняет** ни текущую RP01 деградацию, ни ordinary learner. Не добавлять global normalization, пока не доказаны native mixed entry и реальный экономический или quality эффект.

**Что не менять:** ни один файл core, включая защищённый docs/plans/replanning-possible-diffs, ни обучение RP01, ни старые результаты. Реализация только после независимой проверяемой CODEX/Astra review; документ пока исследовательский.