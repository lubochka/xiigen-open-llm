# Независимая ревизия ND-DPO-SFT / ND-DPO-PAD и раннего E09

Дата: 2026-10-19. Автор: knn_units_history. Проверяемый документ: audit/agents/dpo_lora.md; номера строк относятся к прочитанной версии. Документ DPO-агента не изменён. Новых native training runs нет.

## Краткий вердикт

| Карточка | Обучение и правило успеха | Причинная изоляция | Требуемое уточнение |
|---|---|---|---|
| ND-DPO-SFT, L63–86 | PASS как дизайн:1 DPO+2 SFT updates, одинаковая панель, правило пользователя повторено в карточке/промпте | **HOST_MISSING для существующего owner wrapper.** Перестановка его вызова переснимет reference после SFT или будет отклонена guard | Изолированный исследовательский harness над actual DpoTrainer.StepVector и ProductionSupervisedTrainingStepExecutor; общий R0 подготовлен до обеих стадий |
| ND-DPO-PAD, L109–140 | PASS как дизайн:2 actual updates на плечо; стоимость реального обучения самостоятельный полезный показатель | PASS_BOUNDED: одна execution-length переменная, exact carrier и target set сохраняются | Назвать exact-v2-only guard, снять ambiguity EN→RU owner, описать одинаковые границы cost measurement и pre-zero gradient hook |

Обе карточки явно NOT_RUN. Их новые measurements не присваиваются историческим успехам. Требование обязательного изменения обучаемого состояния соблюдено: здесь не предлагается заменить обучение frozen forward или serving benchmark.

## R1 — существенная поправка ND-DPO-SFT: reference нельзя сохранить одной перестановкой wrapper-вызовов

В L69 документа правильно зафиксирован R0 до обеих стадий; в L84 минимальная реализация названа одним переключателем вокруг существующих owners. Последнее недостаточно.

StableDpoTrainingExecutor строит LogicalDpoTrainingRequest и вызывает _trainingUnit.ExecuteAsync. [Executor L139–206][executor] В non-seeded ветке LogicalDpoTrainingUnit:

- при отсутствии parent checkpoint вызывает CaptureFrozenReferenceState(copyPolicyState: true), L1280–1283;
- helper действительно копирует текущие policy weights в reference, L2235–2236;
- copyPolicyState:false требует FullStatesMatch до capture, L2193–2199;
- prepared seeded branch тоже проверяет полное равенство policy/reference, L1136–1139.

[Reference capture call][capture-call] [Копирование и проверка helper][capture-helper] [Seeded check][seeded]

Следовательно, после двух полезных SFT updates B-policy уже отличается от R0. Вызов прежнего wrapper **либо заменит R0 на post-SFT policy, либо не пройдёт guard**. Объявить это только эффектом порядка нельзя. Источник stage order сам подтверждён: IncrementalTrainer выполняет DPO L584–595 перед SFT L645–671. [Actual order][stage-order]

### Точная рекомендуемая замена L84

«Статус исполнения — HOST_MISSING до появления маленького исследовательского harness. Harness один раз подготавливает одинаковые admitted материалы, полную policy/LoRA и отдельный frozen R0, создаёт два отдельных native optimizer states и связывает их с теми же policy parameters. Он вызывает actual DpoTrainer.StepVector для DPO и ProductionSupervisedTrainingStepExecutor.Execute для SFT; A: DPO→SFT→SFT, B: SFT→SFT→DPO. Через LogicalDpoTrainingUnit после SFT не входить; его reference-provenance guards не изменять и не обходить. Оригинальные material admission, mutation permits, consumption/applied-once и publication boundaries сохраняются. До построения такого host не заявлять runnable готовность или результаты».

Это ограниченный новый research host, а не production patch. Native StepVector аккумулирует четыре пары и вызывает один ApplyOptimizer; supervised executor вызывает TrainSupervisedNextTokenStep с session.Optimizer. [DPO L866–923][native-dpo] [SFT executor L19–31][native-sft]

### Что добавить в execution checklist и промпт

- R0 hash сверяется до/после каждой стадии; между A/B reference одинаков. Полный policy state перед первым update тождествен.
- DPO и SFT имеют отдельные, заранее одинаковые внутри соответствующей стадии m/v/step/LR. Они привязаны к одним и тем же q/v parameters данной policy; SFT не переустанавливает LoRA.
- Градиенты каждой стадии действительно применяются и очищаются штатным native кодом. Переход между стадиями не переносит незапрошенный градиентный остаток.
- Current IncrementalTrainer создаёт DpoTrainingExecutionRequest без явного MaterialVersion, а default request — LegacyVersion. Указать выбранный материал явно. Если исследовательский host выбирает exact-v2 в обоих плечах, это контролируемый опыт порядка для exact-v2, а не буквальный rerun default incremental material. [Request L17–39][request-version]
- Минимальная поправка dataset split к L76: EXAM разделён по source/algorithm family относительно **и SFT-файлов, и DPO source**, а не только по именам. Retention cases отмечены как старые случаи из прежнего обучения, но исключены из текущих новых уроков.
- В промпте L86 заменить предположение о готовом owner-switch на указанное HOST_MISSING и harness; не требовать от исполнителя неявно ослабить existing guards.

**Правило успеха L80/L86 корректно.** Не добавлять условие «accuracy обязана вырасти» или «ни один показатель не ухудшился». Следует сохранить отдельные speed, accuracy, completeness, unsupported-claims и retention дельты. Один полезный выигрыш не доказывает overall promotion, но считается успехом по соответствующему показателю/домену.

## R2 — ND-DPO-PAD: причинная основа корректна, нужны три точных границы

Source подтверждает общий padded T и отдельные ChosenLength/RejectedLength; материал хранит original IDs/masks и проверяет их до вычисления. [Exact material L37–46][material-length] [Capture/validate L96–133][material-validate] StepVector использует четыре actual forward, два backward на пару и один ApplyOptimizer на набор. [DPO source][native-dpo]

Crop до **полной** собственной длины стороны L сохраняет target L−1, который читается из logit L−2. По текущим loops первое/последнее response target не теряются. Exact sumTargets не делится на padded T. [Target loops L1036–1068][target-loop] Causal attention masked −Infinity и RoPE offset0 поддерживают аргумент об отсутствии влияния правого padding на valid-prefix при dropout0. Это корректное предположение для native verification, не измеренный speedup.

### R2.1 — сохранить границу exact-v2 и доменного caller

Предлагаемый helper должен включаться **только для DpoExactPairMaterial.Version** после original CaptureAndValidate. Для LegacyVersion оставить исходный путь. Нельзя восстанавливать длины поиском ID0: ID0 может быть действительным токеном внутри sequence; authoritative length берётся из validated material.

В L123 EN→RU сейчас выглядит безусловным доменом. Однако прочитанный StableDpoTrainingExecutor прямо допускает exact-v2 только для code-documentation. [Domain guard L421–423][domain-guard] Нынешний TranslationDpoTrainerBridge строит отдельные side tensors и masks, не указывая ExactMaterial в этом initializer. [Translation builder L180–201][translation-builder]

**Исправление:** C# документация остаётся первым прямым профилем; EN→RU обозначить как условный exact-v2 research профиль после явного выбора native owner. Не утверждать, что нынешний Translation bridge уже использует общий max-length exact material. Не переименовывать translated domain в code-documentation для обхода guard. Если отдельный законный research caller отсутствует, EN→RU HOST_MISSING, C# опыт остаётся доступным к подготовке.

### R2.2 — actual gradients нужно снять до штатного ZeroGrad

В DpoTrainer ApplyOptimizer вызывается L921, затем _policy.ZeroGrad L923. Снимок gradient после StepVector вернёт уже очищенный градиент и не выполнит обещание L128. [Optimizer/zero L917–923][native-zero]

**Исправление:** существующее наблюдение actual step либо минимальный одинаковый для A/B observer снимает gradient norms/fingerprint перед ApplyOptimizer; post-weight и optimizer-state delta — после него. Никакого дополнительного no-update backward или замещающего forward. Если такого observer ещё нет, его введение входит в host-инструментацию обоих плеч.

### R2.3 — одинаковые границы измерения стоимости

Peak process memory имеет историю процесса; A→B в одной уже разогретой программе не даёт независимые пики. Два updates также чувствительны к JIT/caches и GC, поэтому «одинаковая запись checkpoint» в L128 — необходимое, но не полное условие.

**Исправление:** один свежий процесс на плечо с тем же environment/startup и одинаковой исходной оценкой; измерять один и тот же interval actual Train, включая B view-copy и те же admission/capture/publication. Если допускается общий процесс, использовать per-region allocation counter и сопоставимый memory baseline, не сравнивать cumulative process high-water marks как чистый treatment effect. Порядок плеч зафиксировать до запуска; если две точки не различают эффект от разрешения/шума, так и написать. Это не повод устраивать отдельный frozen benchmark или большой sweep.

**Правило успеха L132–134/L140 корректно.** Нарушение tolerance запрещает утверждать numerical equivalence, но не скрывает отдельно измеренные положительные/отрицательные quality/cost оси. Ускорение actual learning при равной accuracy полезно само по себе. Выбор useful metrics и направления фиксируется до просмотра результатов.

## R3 — E09 0.3368→0.3472: первичный архив подтверждён

Адресно получен primary artifact по уже имевшейся в корпусе ссылке; широкого поиска веток не проводилось:

- repo SHA: **8f20ceb8a9bfbadab2ad309226c47dd76e714548**, DesignToCode;
- experiments-v2/E09-STRUCTURAL-KNN/results/run-2026-04-30-001.md;
- L4: FAILED;
- L8: top1_accuracy **0.3368→0.3472**, относительный +3.1%, не достигнут >=5%;
- L9–10: время **1→74 ms**;
- L16: **tokens968, k5, trainPairs773**. [Receipt][e09-early]

На том же SHA получен driver 35.ExperimentsV2/Experiments/E09StructuralKnn.cs. L25–27 split80%; L46–53 baseline по val−1; L65–72 KNN по val.Take(500)−1. Для968 узлов это train774/val194 и **193 одинаковых перехода**. Лимит500 в этом раннем размере не различает плечи. [Driver][e09-early-driver]

**Разрешённая формулировка:** «Сохранённый ранний E09 показывает +0.0104 абсолютного top1 на общей небольшой панели по сохранённому driver; старый FAILED означал недостижение порога5%, а не отсутствие улучшения. Время выросло1→74ms. Это полезный результат по accuracy/домену согласно нынешнему правилу пользователя».

**Ограничения:** receipt не содержит per-case IDs, manifest train/test или hash фактически запущенного driver; использован source на том же просмотренном snapshot. Это не новый rerun. Вывод «+2 правильных перехода» совместим с округлёнными values и denominator, но не нужен как главное evidence. Не смешивать ранний matched-size run с поздним E09 .2413→.3507, где разные validation denominators, и не доказывать ими перенос E07→E09 без других изменений.

Ранний результат добавлен в audit/agents/knn_units.md, включая FAILED threshold, абсолютную дельту, latency и границы evidence.

[executor]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/DpoTrainingExecution.cs#L139-L206
[capture-call]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L1260-L1283
[capture-helper]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L2193-L2248
[seeded]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L1114-L1148
[stage-order]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/IncrementalTrainer.cs#L584-L671
[native-dpo]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/11.DPO/DpoTrainer.cs#L866-L923
[native-sft]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/SupervisedTrainingExecution.cs#L19-L31
[request-version]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/DpoTrainingExecution.cs#L17-L39
[material-length]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/11.DPO/Models/DpoExactPairMaterial.cs#L37-L46
[material-validate]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/11.DPO/Models/DpoExactPairMaterial.cs#L96-L133
[target-loop]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/11.DPO/DpoTrainer.cs#L1036-L1068
[domain-guard]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/DpoTrainingExecution.cs#L421-L423
[translation-builder]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/27.TranslationDomain/TranslationDpoTrainerBridge.cs#L180-L201
[native-zero]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/11.DPO/DpoTrainer.cs#L917-L923
[e09-early]: https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/experiments-v2/E09-STRUCTURAL-KNN/results/run-2026-04-30-001.md#L4-L19
[e09-early-driver]: https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L25-L72

