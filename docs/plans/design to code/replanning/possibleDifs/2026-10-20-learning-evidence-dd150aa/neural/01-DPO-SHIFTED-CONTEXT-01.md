# DPO-SHIFTED-CONTEXT-01: отдельная длина входа и последнего target

Статус на `dd150aa5216e42b8a8bba14955337398ec5f8397`: **SOURCE_CONFIRMED_BOUNDARY / EFFECT_NOT_MEASURED / PREPARATION_REQUIRED**. Нового запуска, нового результата обучения и исправления продукта этот документ не утверждает. Название обозначает проверяемое различие двух контрактов; текущий отказ сам по себе ещё не доказывает ошибку проекта или причину неудачи RP01.

## Что установлено

Exact DPO считает допустимость по полной длине `Prompt + Response`, а в loss использует следующий токен: последняя позиция carrier нужна как **target**, но её собственный forward logit не используется. SFT уже подаёт модели только `L−1` входных токенов и допускает исходную последовательность `ContextLen + 1`. Поэтому есть узкая гипотеза: оставить все токены, masks и provenance carrier, но разделить вход и targets, чтобы пограничный урок длины `ContextLen + 1` действительно получил DPO update.

Это не предложение отсечь последний target и не снятие ограничений на произвольную длину. Для примера `ContextLen=64`, `L=65` модель видит ровно 64 входные позиции, а loss оценивает target с индексом 64 по logit с индексом 63.

## Первичные доказательства

Все ссылки ниже фиксируют один SHA core, а не подвижную ветку.

| Наблюдение | Источник |
|---|---|
| Длины chosen/rejected — сумма длины prompt и соответствующего response; обе стороны padded до max | [DpoExactPairMaterial.cs, 30–47](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/11.DPO/Models/DpoExactPairMaterial.cs#L30-L47) |
| Проверка exact carrier отклоняет `SequenceLength > contextLength` | [DpoExactPairMaterial.cs, 121–137](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/11.DPO/Models/DpoExactPairMaterial.cs#L121-L137) |
| Trainer передаёт в эту проверку min контекстов policy/reference | [DpoTrainer.cs, 991–999](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/11.DPO/DpoTrainer.cs#L991-L999) |
| Reference и policy получают все IDs, включая последний target | [DpoTrainer.cs, 1007–1030](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/11.DPO/DpoTrainer.cs#L1007-L1030) |
| Scorer оценивает `ids[i+1]` только для `i < t−1`; последняя logit row не используется | [DpoTrainer.cs, 1036–1068](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/11.DPO/DpoTrainer.cs#L1036-L1068) |
| Backward использует тот же сдвиг и тот же предел `t−1` | [DpoTrainer.cs, 1072–1120](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/11.DPO/DpoTrainer.cs#L1072-L1120) |
| SFT допускает `tokenIds.Count <= ContextLen + 1`, создаёт inputs длины L−1 и targets ids[1..L−1] | [Trainer.cs, 77–117](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/04.TrainingLoop/Trainer.cs#L77-L117) |
| Реальный CodeDocumentation producer выбирает exact material | [CodeDocumentationServerOwnedDpoTrainingExecutor.cs, 379–400](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/14.ContinuousLearning/Training/CodeDocumentationServerOwnedDpoTrainingExecutor.cs#L379-L400) |
| Exact route специально ограничен доменом code-documentation | [DpoTrainingExecution.cs, 416–440](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/14.ContinuousLearning/Training/DpoTrainingExecution.cs#L416-L440) |

### Точная связь с обучением

`CodeDocumentationServerOwnedDpoTrainingExecutor.ExecuteCoreAsync` → `StableDpoTrainingExecutor` с `MaterialVersion=segmented-byte-BPE-v2` → native prepared material → `DpoTrainer.StepVectorCore` → `CaptureBatch` → `DpoExactPairMaterial.CaptureAndValidate` → capacity rejection **до** reference/policy forward и AdamW.

На допустимом примере та же цепочка продолжается через четыре forward, два backward и `ApplyOptimizer`: [StepVectorCore, 839–922](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/11.DPO/DpoTrainer.cs#L839-L922), [разрешённые параметры AdamW, 1219–1225](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/11.DPO/DpoTrainer.cs#L1219-L1225). Следовательно, это проверка доступности обучающего примера реальному learner, а не отдельная проверка metadata без участия в обучении.

Прямой `LlmModel.Forward` сам по себе в [219–241](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/03.TransformerModel/LlmModel.cs#L219-L241) не является доказательством разрешённой длины: нужный ограничитель находится в exact material. Нельзя объявить успехом случайный прямой вызов модели длиной 65, обойдя DPO validation.

## Что именно отличается от уже найденного другими агентами

Сверен весь материализованный `possibleDifs` на `0ea8f0b0094e234ffe7734adfa95e2a9e53203f5` и текущие protected cards. Это ограниченная проверка полноты данного среза, а не утверждение о всех будущих ветках.

| Уже описано | Почему этот фактор иной |
|---|---|
| PAD / E-DPO-VALID-LENGTH | PAD убирает справа **padding**, оставляя carrier и forward длины L; capacity не меняет. Здесь полные IDs/mask длины L остаются, а **вход** имеет L−1 и target mapping меняется согласованно. |
| N-HEAD | HEAD выбирает только response rows перед LM head, но transformer всё ещё видит исходный carrier. Здесь разделяются input extent и target extent, включая пограничную допустимость. |
| N63-INGRESS-LENGTH | N63 предварительно разделяет допустимые/недопустимые SFT rows при лимите ContextLen+1; не изменяет exact DPO scorer/backward и не переносит targets. |
| P07 materialized framing | P07 рассматривает сегментацию/границы/шаблоны. Здесь framing и IDs фиксированы, меняется исполнение того же next-token objective. |

В предыдущем PAD документе уже правильно предупреждали, что простое `Length−1` crop теряет target. Именно поэтому новая карточка требует **раздельных** input/target extents. Ссылки: [PAD blueprint, 29–36](https://github.com/lubochka/xiigen-planning/blob/0ea8f0b0094e234ffe7734adfa95e2a9e53203f5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/61-LEARNING-UNBLOCK-AND-EXPERIMENT-AUDIT-2026-10-20/03-DPO-PAD-IMPLEMENTATION-BLUEPRINT.md#L29-L36), [первый PAD вывод, 100–107](https://github.com/lubochka/xiigen-planning/blob/0ea8f0b0094e234ffe7734adfa95e2a9e53203f5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/60-TRAINING-PATH-AND-EXPERIMENT-VALIDITY-2026-10-19/01-DPO-LORA-NEW-TRAINING-DIFFERENCES.md#L100-L107), [HEAD mapping, 137–145](https://github.com/lubochka/xiigen-planning/blob/0ea8f0b0094e234ffe7734adfa95e2a9e53203f5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/61-LEARNING-UNBLOCK-AND-EXPERIMENT-AUDIT-2026-10-20/02-NEURAL-CURRENT-STATE-AND-NOVEL-HEAD.md#L137-L145).

Поиск `DPO SHIFT / shifted input / target-only context` в этом корпусе не дал карточки; поиск `ContextLen+1` привёл к SFT/N63; найденные `L−1` в DPO относятся к предупреждению PAD или HEAD, а не к самостоятельному shifted-input опыту.

## Предлагаемое экспериментальное вмешательство

Только локальные исходники нового эксперимента. Общие фрейминг, веса пары, prompt/response masks, LoRA targets, reference, beta, LR, AdamW, входные строки и tokenizer одинаковы.

1. Полный exact carrier, его digest/admission, IDs и masks сохраняются без изменений.
2. В B проверяется явный экспериментальный контракт: `T >= 2`, `forwardT = T−1`, `forwardT <= min(policy.ContextLen, reference.ContextLen)`. Нельзя поднять общий context limit и оставить старый full forward.
3. Все четыре forward получают `ids[0..T−2]`. Чтобы не смешать фактор с PAD, обе стороны получают именно общий `T−1`, а не собственную длину каждой стороны.
4. Scorer и backward B принимают полные target IDs/mask длины T и logits длины T−1; проверяют формы; обрабатывают `i=0..T−2` включительно. Поэтому target с индексом T−1 сохранён.
5. Legacy путь остаётся самостоятельным исходным режимом; новая гипотеза ограничена exact. В эксперименте хранится отдельная execution identity `SC01-shifted-input-v1`, и результаты не выдаются за производственный profile.
6. Shared experiment bridge тоже должен применять согласованный контракт: в N62 donor есть дополнительная ранняя validation до trainer. Поменять только `DpoTrainer` недостаточно. См. [N62ExperimentBridge.cs, 85–103](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/35.ExperimentsV2/Experiments/Rp01Micro/N62-R1/Common/ContinuousLearning/N62ExperimentBridge.cs#L85-L103).

При наличии полезного измеренного эффекта кандидат на изменение оригинала содержит именно `11.DPO/Models/DpoExactPairMaterial.cs` (разделение validity полного carrier и capacity исполнения), `11.DPO/DpoTrainer.cs` (input extent + target mapping в reference/policy/scorer/backward), и явную фиксацию новой execution/capacity семантики в profile/continuation. Простая замена `>` на `> ContextLen+1` не является исправлением.

## Ограничения вывода и домены

Первый опыт назначен только **code-documentation / C# byte-level fixture**, потому что это реальный текущий exact producer и есть сохранённое обученное состояние этого профиля. Применимость математического сдвига шире, но EN→RU, natural-language и другие tokenizer/framing режимы в этом опыте не проверяются.

Если все реальные RP01 пары короче границы, именно эта недостающая единица не объясняет их ухудшение. Отдельно измерить долю фактически отклонённых пар можно только в составе будущего опыта с обучением; отсутствие такой статистики сейчас не заменяется выдуманной частотой.

Ресурсная выгода удаления последней входной позиции возможна и при L≤ContextLen, но не измерена и не основная цель карточки. A при L=ContextLen+1 не выполняет updates; B выполняет. Это **разблокирование**, а не speedup двух одинаковых обучений. Более длительная B закономерна и должна быть показана.

Новый кандидат оформляется только после результата. Любое измеренное улучшение полезного показателя признаётся успехом этого показателя и домена; рост accuracy не требуется. Рядом фиксируются регрессии. Ни одно измерение этой гипотезы в текущем аудите не проводилось.

