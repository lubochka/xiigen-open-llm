# Адресная сверка двух Tensor-факторов после core669

20 октября 2026. Только чтение исходников и документирование. Новых build/train/inference calls, изменений production или protected нет. Четыре публикационных learning Markdown и исходный `learning-novelty.json` этим дополнением не изменялись.

**Вердикт:** GRAD-ACCUM-COPY-01 и FROZEN-BROADCAST-MAP-01 сохраняют свои точные исходные участки и отличие от N62-R2. Source pin `883496bca18113bf37e0afe683c75b05a0075048` не нужно незаметно заменять на поздний HEAD.

## 1. Что проверено на двух точных commit

Через GitHub `fetch_file` заново получены оба файла на `883496bca18113bf37e0afe683c75b05a0075048` и `66947785d1318f7cbbc88118f3156cdc75001e16`. Совпали Git blob SHA и полные возвращённые тексты, не только отдельные строки.

| Файл | Git blob на обоих commit | Вывод |
|---|---|---|
| `00.TensorCore/Core/TensorOps.cs` | `e1a20dca84d76ee85d0c559bd496c1fa3c96ac8b` | Полностью неизменён: GRAD AccumulateGrad и FROZEN MultiplyBroadcast остаются прежними |
| `00.TensorCore/Core/TensorStorage.cs` | `22aec3ba03b756530001ae786acbdce28af90104` | Полностью неизменён: defensive copy сохраняется |

На core669 [AccumulateGrad:984–997][accum] по-прежнему передаёт `gradToAdd.Data.ToArray()` в Tensor constructor. [TensorStorage:21–24][storage] по-прежнему копирует входной массив. GRAD убирает только внешнюю копию первого вклада.

На том же core669 [MultiplyBroadcast:168–210][broadcast] по-прежнему создаёт и заполняет `mappedBIndices` до условного создания backward closure. FROZEN убирает этот массив только при обоих `RequiresGrad=false`, сохраняя `FlatToIndices`. Статус остаётся «отдельное уточнение известной RMS-семьи», не повторное открытие всей семьи.

## 2. Актуальный N62-R2 — другой фактор

Получена карточка `docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md` на core669, Git blob `ddf20c0c4905ab6f692943634fe258b543be2b6e`.

[Её строки 289–301][r2] сохраняют точное вмешательство: выделение `new float[v]` в `DpoTrainer.BackwardLogProbs` переносится из token-loop на один вызов backward, отдельно для chosen/rejected. Полная перезапись scratch, порядок арифметики, masks, `logits.Backward` и прежнее выделение `logits.Grad` сохраняются. Это не удаление копии в `AccumulateGrad` и не отказ от no-grad broadcast-map. Успех или завершение R2 не является результатом наших двух кандидатов.

[Строки 3–9][latest-status] отмечают сохранённые N62-R1 и N62-R2, запрещают их повтор и называют следующим ещё не выполненным №06 E-OPT-STEP. Новая learning_signal/TRAINv2 панель отсутствует в их старых raw и не должна приписываться им. Здесь прочитана карточка статуса; повторного независимого разбора result payload не было. Общую очередь 38 строк / 5 завершены / 33 ожидают root актуализирует отдельно; в этом дополнении NEXT публикационных карточек не переписывался.

## 3. Воспроизводимость назначенного source-copy pin

Для двух проверенных факторов pin883 остаётся точным воспроизводимым источником: это неизменяемый commit, и оба задействованных Tensor-файла идентичны core669. Будущий host должен брать назначенные source bytes из pin883, а не случайный рабочий HEAD и не уже изменённую R2-копию. Исходное A и единственное B-вмешательство сохраняются.

Эта адресная сверка не является повторным аудитом всех зависимостей четырёх библиотек и не проверяет наличие B0, binary pack, ABI, SDK или работоспособность ещё не созданного runner. Новое уточнение N62 о provenance требует фиксировать identities после завершения назначенного B-factor и до build; оно не разрешает переписывать исторические receipts либо повторять сохранённый опыт.

[accum]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/00.TensorCore/Core/TensorOps.cs#L984-L997
[storage]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/00.TensorCore/Core/TensorStorage.cs#L21-L24
[broadcast]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/00.TensorCore/Core/TensorOps.cs#L168-L210
[r2]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L289-L301
[latest-status]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L3-L9
