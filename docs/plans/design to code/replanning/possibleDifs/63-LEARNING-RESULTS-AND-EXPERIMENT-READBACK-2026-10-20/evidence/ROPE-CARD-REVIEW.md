# Независимый статический review ROPE-TRAIN-TABLE-01

Проверены авторская карточка `audit_notes/training_new_gaps/ROPE-TRAIN-TABLE-01-CARD.md`, её итоговая редакция `06-ROPE-TRAIN-TABLE-01.md` в пакете 63 и названные ниже исходники core `0b76ab950d82534f5904005dda096545505ae307`. Источники прочитаны; host не создан, сборка/обучение/новые probes не выполнялись. Это review проекта будущего эксперимента, не runtime certificate.

## Вердикт

**Открытых содержательных замечаний к итоговому проекту не осталось.** Он соответствует обучающей задаче и имеет статус документированной гипотезы; готовая CLI и измеренный выигрыш не заявлены. Две адресные поправки ниже внесены в итоговую карточку и проверены чтением. Дополнительных проверок или запусков ради этого review не требуется.

1. **Однозначный deadline — закрыто:** в draft build≤120с плюс A≤60с плюс B≤60с сосуществовали с «общий run deadline120с». Итоговая карточка назначает всей команде максимум **240с**, включая одну сборку и два процесса; stage caps 120/60/60 сохранены.
2. **Ложная receipt-метка — закрыто:** [borrowed Program L84](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/experiments-v2/RP01-MICRO/LFT01/experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01/Program.cs#L84) пишет `exactly native BPE with zero merges`, тогда как [Bytes L106](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/experiments-v2/RP01-MICRO/LFT01/experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01/Program.cs#L106) вызывает только `Encoding.UTF8.GetBytes`. Итоговая карточка явно назначает замену metadata на `UTF-8 bytes 0..255; no tokenizer Train/Encode invoked; no EOS`. Сравнение BPE ради исправления подписи не запускалось и не назначено.

## Что проверено по существу

| Критерий | Вывод и доказательство |
|---|---|
| Настоящий learner | [Trainer L102–145](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/04.TrainingLoop/Trainer.cs#L102-L145) формирует next-token inputs/targets, выполняет `Forward(training:true)`, backward и AdamW. Назначены 4 updates на плечо; это SFT/LoRA, DPO0 |
| Измеряемый участок участвует в обучении | [CausalSelfAttention L146–153](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/03.TransformerModel/Layers/CausalSelfAttention.cs#L146-L153) вызывает RoPE для Q/K; [TensorOps L1277–1363](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/00.TensorCore/Core/TensorOps.cs#L1277-L1363) читает cos/sin в forward и backward. Отдельный `GetFrequencies` вне урока явно исключён |
| Один фактор | Меняется только переиспользование таблиц внутри training при offset0. Исходные формулы, inference, `ForwardWithCache`, Linear, dtype, loss, schedule остаются заданными. Инструментация присутствует в обоих плечах, отражает реальные builds/reuses |
| Область кэша | [RotaryEmbedding L14–20](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/03.TransformerModel/Layers/RotaryEmbedding.cs#L14-L20) имеет readonly dim/theta. Для данного экземпляра и offset0 ключ seqLen достаточен в заданной узкой области. Проверка отмены/invalid length сохраняется. Новые tensors при замене длины не портят tensors, захваченные текущим backward |
| Честная стоимость | Cache первый раз заполняется внутри TRAIN; before EXAM не даёт бесплатного cache warmup. Allocations и elapsed/CPU относятся только к четырём Train calls; retained float payload и whole-process peak отдельно. Fixed-order timing не объявляется доказанным ускорением |
| Обучение и A/B не смешаны | Назначены A0/A1/B0/B1, NLL/exact и их учебные сдвиги; эффект фактора — ΔB−ΔA. Полные last256 logits, 4 losses, параметры и optimizer receipts обеспечивают проверку эквивалентности без новых измерений. Градиенты после ZeroGrad не выдаются за raw |
| Данные и конфигурация | Заданы точные 2 TRAIN+2 EXAM, UTF-8, model dimensions, seeds, новый LoRA stream, rank/alpha/dropout, optimizer и schedule. Целевая предметная тема C#, механизм RoPE указан отдельно. Четыре шага не объявляются освоением арифметики |
| Происхождение копии | `source_paths_36.json` содержит ровно36 путей, тот же path set, что исторический LFT01 source manifest. Для нового опыта требуется материализация из текущего0b76, а не перенос изменённой Linear-копии. Четыре недостающих локально зависимости core/dataset дополнительно получены с0b76 для read-only inspection. Совпадение имён не объявляется доказательством сборки свежих источников |
| Fresh metadata | Карточка явно требует заменить старые ID, CoreCommit, stream, Linear experiment references, `unit_count` на фактический native learner; не переносить исторические hashes в fresh manifest. Исправление byte-label включено; граница источников однозначна |
| Пути и исполнимость | Указаны короткий RPE01/a001 root, новый results, конкретные будущие dotnet/Python команды. Host пока отсутствует; это явно сказано до CLI. Отсутствующие Git object/SDK/runtime не разрешают искать другой проект или заявлять состоявшийся run |
| Правила пользователя | Default session model без override; русский report≤350 слов, candidate≤120; любая полезная ось успеха с ухудшениями рядом; старые/частичные результаты не удаляются; prod решает пользователь. STOP и NEXT имеют приоритет над отчётом/review/registry; возврат к завершённым опытам запрещён |

## Ограничения, которые надо сохранить в итоговом пакете

Это новый малый ресурсный A/B на одной предметной задаче, не доказательство, что отсутствие RoPE cache вызвало падение точности RP01. До запуска нет измеренного выигрыша. Список36 и статическое чтение не являются выполненной сборкой; future runner и изменённый Program ещё предстоит подготовить по карточке. Границы cache для concurrency, произвольных последовательностей, inference offsets и разных моделей здесь не измеряются и не требуют дополнительных запусков.

`next` итоговой карточки синхронизирован с добавочным registry, включая позднюю planning-дельту `6bc4b6fba8f14870f5dc4966cf5a344754962d66`: ROPE → AST → NG01 → IDENTITY-M1-01 → KN-ROUTER-LEARNED-KEY-01 → END. NG01 — сохранённый внешний проект S03, не новая собственная находка пакета. При отдельном поручении ROPE — END. Защищённый индекс не меняется этим review.

## Позднее уточнение 883496: TRAIN-fit и общий NEXT

Адресно прочитаны изменённые §2/§3/§5/§6 и prompt итоговой карточки 06. К существующим восьми EXAM forwards добавлены ровно **восемь TRAIN-fit forwards**: две TRAIN-строки × до/после × A/B. Всего 16 readout forwards, по-прежнему 4 native SFT updates на плечо, 8 training forwards/backwards суммарно. Новый `target_rank` вычисляется из тех же последних logits; дополнительных вызовов модели ради rank нет. Обобщение `Observe(model, rows)` задаёт точные панели. Для однотокенных ответов назначенный argmax exact соответствует full-response exact; TRAIN-fit отдельно от EXAM и не называется обобщением.

Все readouts используют `training:false` и находятся вне TRAIN-cost, поэтому не заполняют training-only RoPE cache. Модель, источник `0b76…`, один фактор, данные, 240с deadline, output и NOT_RUN остаются прежними. Это небольшое документное расширение наблюдения будущего опыта; old result не перезапускается ради новой панели. В изменённом registry №01–04 завершены, текущий NEXT — **№05 N62-R2**; собственный next ROPE → AST сохранён. Замечаний к этой дельте нет; новых сборок, forwards или updates reviewer не выполнял.
