# Независимая проверка новых учебных карточек и договора исполнения

> **Исторический срез883/53cb.** На окончательном827 №01–06 завершены, следующий LF-03; P54 blanket запрет снят, NG01 имеет protected карточку и новый host, R1 report уже исправлен. Актуальный статус и оставшиеся gaps — [final827 update](../08-final-core827.md), действующие outputs — [root05](../05-executor-contract.md). Старый R2 wrapper/claims не запускать; этот receipt не назначает повтор.


> **Историческое свидетельство review черновика до concurrent53cb.** Ниже сохранены исходные35 строк/31 pending и закрытие трёх замечаний того draft; это не действующая очередь. После согласования planning `53cbcabd3926cda714bab428d294eb666301e9e4` active [root04](../04-execution-registry.md) содержит **38 различных ID, 4 завершённых, 34 pending**. [Машинное согласование](concurrent-merged-registry.json) сохраняет все36 унаследованных ID в прежнем относительном порядке и все32 прежних output assignments, включая28 исходных: `inherited_assignment_field_changes=[]`, то есть **0 изменений** полей result_root/report/candidate/entry/source_url. Добавлены только GRAD/FROZEN перед двумя dependent readers. Старый review и его машинный файл не переписаны под новую проверку; окончательный bounded review нового root04/05 — отдельное чтение.

Дата: 2026-10-20. Проверка документальная: исходники и рецепты прочитаны, новых сборок, обучения, тестов или изменений product/protected не выполнялось. Core: `883496bca18113bf37e0afe683c75b05a0075048`; planning: `6bc4b6fba8f14870f5dc4966cf5a344754962d66`. Reviewer — модель текущей сессии по умолчанию, без override; платформа не предоставляет отдельную проверяемую запись фактического runtime model ID.

## Вердикт по двум новым карточкам

**Оба предложения допустимы как небольшие будущие эксперименты настоящего обучения.** Их причинные цепочки подтверждаются исходниками; A/B-факторы различны и не объединены с прежними LF-03/RMS-COORD. Это проверка спецификаций, **не доказательство работоспособности ещё не созданного host и не измеренный выигрыш**. Две карточки, два встроенных промпта и общий договор не выдают будущие команды за уже существующий runner.

Проверены [GRAD-ACCUM-COPY-01](../learning/GRAD-ACCUM-COPY-01.md), [FROZEN-BROADCAST-MAP-01](../learning/FROZEN-BROADCAST-MAP-01.md), [общий договор](../learning/TENSOR-LEARNING-EXECUTION-CONTRACT.md), исходники Tensor/TensorStorage/TensorOps, DpoTrainer, simulation owner/loader, LlmModel/RmsNorm и ближайшие прежние LF-03/RMS-карточки. Применены ранее прочитанные навыки проверки плана, кода, промптов и человеческой коммуникации из pinned `docs/skills`; полный production pipeline к этому документальному обзору не применяется.

| Что проверено | GRAD-ACCUM-COPY-01 | FROZEN-BROADCAST-MAP-01 |
|---|---|---|
| Прямая связь с обучением | Policy backward → `Tensor.Backward` → `AccumulateGrad` → optimizer | Reference chosen/rejected forward → RmsNorm → `MultiplyBroadcast` → DPO objective → optimizer |
| Реальный фактор A/B | Только внешний `ToArray` первого gradient contribution | Только отсутствие `mappedBIndices` при обоих `RequiresGrad=false` |
| Сохранённая семантика | Defensive copy TensorStorage остаётся; ветвь `+=`, zeroing, граф и optimizer прежние | Shape guards, right alignment, `FlatToIndices`, float multiply, cancellation и вся gradient-ветвь прежние |
| Отличие от уже найденного | LF-03 меняет initial logits-gradient в `Tensor.Backward`, другой site | RMS-COORD меняет per-element координатный массив и сохраняет `mappedBIndices`; новая карточка — уточнение этой семьи |
| Назначенные физические пути | `C:/xg/AGC1/{src,host,results}` | `C:/xg/FBM1/{src,host,results}` |
| Учебный бюджет каждого опыта | Одна TRAIN-пара; один StepVector/arm; два optimizer updates всего | Тот же фиксированный бюджет, отдельный опыт |
| Дополнительный readout | Четыре policy DIAG_TRAIN forwards всего | Четыре policy DIAG_TRAIN forwards всего |
| Результат сейчас | Гипотеза; запуск не проводился | Гипотеза; запуск не проводился |

Доказательства первого фактора: [AccumulateGrad:984–997][accum], [Tensor constructor:39–46][ctor], [TensorStorage:21–24][storage], [DPO backward:1072–1121][backward]. Удаляется внешняя копия; внутренний `data.ToArray()` обеспечивает прежнее отдельное владение. Сумма theoretical payload не является замером GC/RSS и в карточке так не названа.

Доказательства второго фактора: [MultiplyBroadcast:148–213][broadcast], [freeze reference:110–112][freeze], [DPO reference/policy:875–904][step], [RmsNorm:29–40][rms]. В исходнике массив отображения выделяется до проверки RequiresGrad, а читается после неё только внутри backward closure. Предложенная no-grad ветвь повторяет существующее вычисление `bFlat` и не удаляет коэффициенты reference из DPO. Переносимость на другие домены и эффект в runtime не доказаны.

## Правильность before/after и наблюдаемых результатов

Общий договор согласуется с [protocol24][protocol]: одна и та же chosen TRAIN-панель, IDs/masks неизменны, A0/B0 перед обновлением, A1/B1 после committed update, один forward на состояние. Все NLL/rank/token/exact поля берутся из этих же logits; новый forward для каждого показателя не добавлен. Target сдвинут на один token; prompt/padding исключены; gold rank использует строгое `>` и не ошибочно приравнен к exact при tie. Diagnostic NLL считается отдельно от native float objective. Нулевые/невалидные mask/shape/IDs или nonfinite дают явную ошибку readout, а не фиктивные нули.

**Критичная прежняя ошибка не повторяется:** [StepVectorCore:891–935][pre-update] вычисляет loss/margin до ApplyOptimizer и возвращает эти старые scalars после update. Договор и оба промпта называют их `TRAIN_STEP_PRE_UPDATE`, не A1/B1. Дополнительный native post-update DPO loss/margin не назначен и указан как «не измерено». DIAG_TRAIN_PANEL действительно измеряет результат после update; она не объявлена heldout или свободной генерацией.

Стоимость считается на одинаково завершённых update-границах, отдельно от load/materialization/readout/report. Внутренняя publication не скрыто вычтена из StepVector. Allocated bytes, retained heap, working set, process peak, CPU, wall и GC не смешаны. Четыре диагностических forward имеют объявленную стоимость, не считаются бесплатным улучшением. Одно A→B не устанавливает статистическую устойчивость или независимость CPU/wall от порядка; полезный наблюдаемый знак дельты всё равно сохраняется по пользовательскому правилу.

## Пользовательский договор

| Требование | Результат проверки новых файлов |
|---|---|
| Любое улучшение полезной метрики — успех этой метрики и домена | Дословно есть в обеих карточках и обоих промптах; accuracy не обязательна, регрессии рядом |
| Никаких подмен обучения отдельным microbenchmark | Оба фактора оцениваются в настоящем StepVector с native LoRA recipient/admission; отдельный copy/inference benchmark не засчитывается |
| Старые результаты сохранить; prod/protected не менять | Результаты и candidate только во внешнем experiment workspace; перенос решает пользователь |
| Default model и русские отчёты | Модель текущей сессии, без override; точный report skeleton и навыки заданы общим договором |
| Exact host/data/model/run/report | Существующий B0 и fixture pinned; короткие пути; четыре source-copy библиотеки; bridge; единственная назначенная launcher-команда после подготовки |
| Ошибки/повторный запуск | Existing execution/training claim и result сохраняются; automatic retry, cleanup, самостоятельно выбранный новый RunId запрещены |
| Immediate NEXT/STOP | Полный запрет дописывать прежний report/hash/review/test имеется в обеих карточках, обоих промптах и общем договоре; оформление не выше прямого указания пользователя |
| Единый порядок | В новых файлах одинаково: исходные05–31 → GRAD → FROZEN → NG01 → исходный32 reader; текущий следующий N62-R2, завершённые01–04 не повторяются |

## Замечание к точности спецификации

**R-01, небольшое, исправляемое документом:** общий договор назначает fresh AdamW и native `CreateForProjectLora`, но в просмотренной версии не перечисляет числа optimizer/DPO. Они определены pinned source, поэтому A/B уже не позволяет менять их произвольно; однако исполнителю не нужно извлекать их заново. Рекомендуем явно записать DPO beta `0.1`, learning rate `1e-5`, AdamW beta1 `0.9`, beta2 `0.95`, weight decay `0.01`, epsilon `1e-8`; actual `trainer.OptimizerSettings` сохранить и сверить с назначением до update. Источники: [DpoTrainer:328–385][settings], [native creation/recording:160–164][owner-settings]. Замечание передано автору/интегратору. Оно не требует запуска или изменения production.

**Граница утверждения о запуске:** `pwsh`, SDK, B0 artifacts и ABI dependencies в целевой Windows-среде здесь не проверялись; `Run.ps1`, source-copy и runner пока описаны. Нельзя на основании данного review написать «READY_TO_RUN» или «успех подтверждён». При будущем порученном исполнении применяется уже назначенный preparation/status path, без поиска другой модели, silently invented old CLI, random fallback или построения production. Наличие явной спецификации будущей новой CLI отличается от утверждения, что эта CLI уже существует.

## Проверка интеграции root-пакета

Прочитаны новые [README](../README.md), [04-execution-registry](../04-execution-registry.md) и [05-executor-contract](../05-executor-contract.md). Проверка таблиц сохранена в [independent-root-mapping-review.json](independent-root-mapping-review.json): 35 строк реестра, 4 завершённых и **31 pending ID**; все 31 получили собственные claim/results-path, пропусков и лишних назначений нет, коллизий путей нет, protected не является output-root. Это точность документного mapping, не проверка существования Windows-каталогов/CLI.

Текущий следующий N62-R2, исходные №05–31 сохранены; четыре последних — GRAD-ACCUM-COPY-01, FROZEN-BROADCAST-MAP-01, NG01-null-guard-label и прежний №32 KN-ROUTER-LEARNED-KEY-01. Они совпадают с новыми карточками/промптами; конкурирующая очередь не найдена. Root05 разрешает конфликт старых protected-output указаний явными путями, не заставляя исполнителя выбирать их. Для AGC1/FBM1 claim/results совпадают с их собственным договором. Root05 отличает наблюдаемый полезный выигрыш от причинной уверенности; README не превращает 470 структурных ссылок в 470 исполненных экспериментов.

**R-02, ошибка навигации:** в первом прочитанном root04 шесть URL-фрагментов были закодированы как `%23queue-*`, то есть входили в имя файла вместо якоря. Для N62 R1/R2/E-OPT/LF03 и двух N61 Mode1 строк требуется `...md#queue-*`. Передано интегратору для исправления; это не требует обучения.

**R-03, точность описания:** заключение первого root05 «Этот блок дословно включён» сильнее проверенного факта: в learning-карточках включён тот же абсолютный смысл NEXT/STOP, но текст root05 отличается. Рекомендовано «Это правило включено». Сами карточки/промпты действительно запрещают продолжать старое оформление после команды перехода; смыслового исключения не найдено.

### Финальное закрытие замечаний

Один заключительный readback 2026-10-20 подтвердил все три исправления в публикационном пакете. **R-01 закрыто:** tensor-договор явно задаёт DPO beta0.1, LR1e-5, AdamW beta1=0.9, beta2=0.95, weightDecay=0.01, epsilon=1e-8, запись actual OptimizerSettings и pinned ссылки; отдельно объяснено переопределение default weight decay. **R-02 закрыто:** в `04-execution-registry.md` и `coverage/execution-projection.json` все шесть ссылок используют настоящий `#queue-*`, `%23` отсутствует. **R-03 закрыто:** root05 теперь говорит «Это правило включено». Открытых замечаний этого ограниченного review не осталось. Машинный `independent-root-mapping-review.json` сохраняет первый draft как историю обнаружения R-02; этот абзац фиксирует проверенное исправление. Новых runtime-проверок, обучения или production/protected изменений при readback не было.

[accum]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/00.TensorCore/Core/TensorOps.cs#L984-L997
[ctor]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/00.TensorCore/Core/Tensor.cs#L39-L46
[storage]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/00.TensorCore/Core/TensorStorage.cs#L21-L24
[backward]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/11.DPO/DpoTrainer.cs#L1072-L1121
[broadcast]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/00.TensorCore/Core/TensorOps.cs#L148-L213
[freeze]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L110-L112
[step]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/11.DPO/DpoTrainer.cs#L875-L904
[rms]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/03.TransformerModel/Layers/RmsNorm.cs#L29-L40
[protocol]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/24-neural-learning-signal-protocol.md
[pre-update]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/11.DPO/DpoTrainer.cs#L891-L935
[settings]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/11.DPO/DpoTrainer.cs#L328-L385
[owner-settings]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L160-L164
