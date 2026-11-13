# Остаточный конфликт четырёх neural-карточек с правилом 15

**Статус: DESIGN_INSUFFICIENCY / NOT_MEASURED.** Проверены E-SFT-RESPONSE-MASK, E-SFT-MOMENT-CONTINUITY, E-DPO-VALID-LENGTH, E-DPO-SFT-ORDER и их общий TRAIN readout. Это вывод о достаточности будущего рецепта, а не доказательство плохого обучения или отсутствия пользы ранее выполненных опытов. Исходный snapshot core: `dd150aa5216e42b8a8bba14955337398ec5f8397`. Runtime, обучение и product tests не запускались; protected документы не изменялись.

## Единственная существенная находка

Новый TRAIN readout устраняет недостаток наблюдаемости, но **не доказывает, что назначенные начальное состояние, данные и бюджет позволяют получить хотя бы один правильный ответ**. Во всех четырёх карточках по-прежнему фиксирован исходный **необученный B0** и очень короткое обучение. Доказательства готового почти обученного состояния либо уже диагностичного полного ответа на назначенной панели не приведены. Одних добавленных NLL/rank/token readouts недостаточно, чтобы объявить выполненным требование пользователя «0 правильных ответов до и после не приемлимо».

### Что за B0 в действительности назначено

- Во всех четырёх карточках строки **41–45** задают `rp01-csharp-small/v1`, vocab629, hidden512, 4 слоя, q/v LoRA8/16, dropout0, 39 base +16 adapter tensors; weights SHA256 `a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911`, manifest SHA256 `c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d`.
- Это не только название в карточке: [сохранённый manifest:1](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/training-artifacts/models/csharp-code-documentation/ilearning-unit/rp01-csharp-small/v1/checkpoints/000001/manifest.json#L1) указывает `parentCheckpoint:null`, `step:1` и причину сохранения первого **untrained Step1 no-update baseline**, без optimizer update или exam в B0. Физический путь — `training-artifacts/models/csharp-code-documentation/ilearning-unit/rp01-csharp-small/v1/checkpoints/000001/weights.bin`.
- [Действительный loader, Program.cs:315–332](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/tools/Rp01SimulationDpo/Program.cs#L315-L332) проверяет именно эти hash, Step1, отсутствие parent и `InitialBaselineNoUpdate`; другой checkpoint не является разрешённой заменой. Следовательно, восстановление более обученного descendant нельзя тихо вставить в прежнюю карточку под именем B0.
- Наличие saved weights подтверждено деревом GitHub и manifest; бинарные 47.9 MB здесь не загружались и модель не восстанавливалась. Это проверка указанной идентичности, не фактическая готовность Windows-хоста. Глобального утверждения «подходящих warm checkpoints нигде нет» данный аудит не делает.

### Текущий обучающий и диагностический бюджет

| Карточка | Реальные updates на одно плечо | Дополнительный TRAIN readout на всю A/B | Точное основание |
|---|---:|---:|---|
| E-SFT-RESPONSE-MASK | 2 SFT | 8 forwards | [карточка:81,87](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/experimets/cards/E-SFT-RESPONSE-MASK.md#L81-L93), :11 |
| E-SFT-MOMENT-CONTINUITY | 2 SFT, reset/carry перед вторым | 8 forwards | [карточка:81,87–93](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/experimets/cards/E-SFT-MOMENT-CONTINUITY.md#L81-L93), :11 |
| E-DPO-VALID-LENGTH | 2 DPO, batch1 | 8 forwards | [карточка:81–95](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/experimets/cards/E-DPO-VALID-LENGTH.md#L81-L95), :11 |
| E-DPO-SFT-ORDER | 1 DPO batch2 +2 SFT | 16 forwards | [карточка:81,87–100](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/experimets/cards/E-DPO-SFT-ORDER.md#L81-L100), :11 |

Во всех четырёх :11 прямо запрещает увеличивать steps, менять head/objective/LR или подбирать примеры до успеха. Это правильный запрет для исполнителя фиксированного A/B, но авторский рецепт ещё должен доказать достаточность выбранного бюджета. [Общий протокол:37,49–55](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/experimets/24-neural-learning-signal-protocol.md#L37-L55) сохраняет число updates и честно допускает `NO_POSITIVE_SIGNAL_AT_ASSIGNED_BUDGET`; улучшение NLL/rank может появиться без правильного полного ответа. Протокол не содержит заявленного успешного readout на именно этом B0 и D-T/PAD/S-панели.

## Как исправить документный контракт

В новом разрешённом planning-дополнении поставить для будущего запуска этих карточек отдельный статус **NEEDS_DIAGNOSTIC_TRAINING_RECIPE**, не переписывая protected source и не стирая прежние результаты. До будущего исполнения автор назначает один точный общий checkpoint/data recipe, для которого есть свидетельство как минимум одного правильного **ответа в задаче** на оговорённой панели до либо после минимального обучения. Correct token, файловый hash, изменившийся weight и teacher-forced whole response должны называться своими метриками; они не заменяют друг друга. При необходимости проверки условного правила отдельно сравнить простой constant baseline, чтобы один постоянный ответ не выдавался за освоение обеих противоположных задач.

Предпочтительный путь пользователя — восстановить уже почти обученное состояние: указать точные bytes/config/tokenizer/topology, optimizer/scheduler/RNG/counters и источник прежнего качества; обоим плечам дать один и тот же snapshot. Текущие Step1/no-parent/hash guards допускают только старый B0, поэтому новый warm snapshot требует явного отдельного экспериментального контракта загрузки. Если подходящего сохранённого состояния нет в прочитанных источниках, не обещать его наличие: описать общую подготовку с конечным заранее назначенным бюджетом, оплачиваемую отдельно от treatment A/B, и критерий диагностичности до выдачи двух плеч. Пока этого доказательства нет, нельзя самостоятельно объявлять достаточными 20/100/400 шагов или выбирать данные по успешному B-плечу.

Учебные updates, общая подготовка/warm state, diagnostic forwards и EXAM/generation должны иметь отдельные бюджеты. Отсутствие эффекта не разрешает исполнителю повторять исследование до PASS; STOP/NEXT остаётся немедленным. Добавленная панель — полезная часть измерения, её не надо удалять.

## Что сохраняется как полезное

Этот вывод не отменяет измеренные ресурсные выигрыши, улучшение NLL/rank или результаты №01–07 и не требует их повторения. Любое измеренное улучшение полезной оси остаётся успехом **этой оси и домена**, со всеми регрессиями рядом. Отдельно остаётся незакрытым требование диагностичности обучения в будущей карточке. Ни факт исходного необученного B0, ни отсутствие подтверждённого nonzero readout не доказывают, что две назначенные updates непременно дадут 0→0: результаты этих четырёх будущих A/B здесь не наблюдались.

## Позднее свидетельство, не меняющее этот адресный вывод

На core81141d57 проверен RESTORE-MINIMAL-SET: реальный H16 learned state и успешное продолжение NLL/rank/token accuracy существуют. Это важное дополнение к поиску состояний, но его topology отличается от H512 этих четырёх карточек; full-response exact0/2. Отсутствие подтверждённого H512donor не означает отсутствия всякого обученного состояния. [Полный readback](11-latest-core-delta-and-restore.md).
