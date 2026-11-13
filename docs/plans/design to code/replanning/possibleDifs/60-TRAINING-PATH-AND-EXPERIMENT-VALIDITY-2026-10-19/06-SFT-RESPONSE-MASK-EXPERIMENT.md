# Уточнение известной гипотезы: чему SFT учит на служебном prompt

**E-SFT-RESPONSE-MASK — NOT_RUN; известная проблема с недостающим точным опытом, не новая общая находка.**

## Факт и отличие от DPO-mask

IncrementalTrainer кодирует `Prompt + "\n" + Response` как одну последовательность ([исходник, 402–418](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/IncrementalTrainer.cs#L402-L418)). Trainer задаёт next-token targets для всей последовательности и выполняет CE/backward/optimizer ([102–147](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/04.TrainingLoop/Trainer.cs#L102-L147)). В прочитанном ProjectIndexer prompt содержит имя файла, response — его исходник ([68–76](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Indexer/ProjectIndexer.cs#L68-L76)).

Общий факт уже описан в planning `12-QUICK-CAUSAL-MICROEXPERIMENTS-2026-10-16.md` и MINI01. Наличие P05 о DPO response masks не означает наличие опыта SFT: это разные objective и backward callers. Текущий опыт проверяет, помогает ли исключение служебных prompt targets изменить полезное обучение response при сохранённом входном контексте.

## Маленький опыт

**Домен:** C# source completion; документационные ответы — дополнительная retention-панель того же Student, если до опыта там есть полезные ответы. Перенос на перевод требует отдельно установленного SFT consumer.

**Общий старт:** одинаковые копии пригодного B0 и LoRA, tokenizer, fixed LR (`MaxLr = MinLr > 0`, `WarmupSteps = 0`), optimizer m/v/step, dropout=0, clipping, decay и decoding. Два одинаковых коротких C# файла дают по одному настоящему supervised update; всего два updates на плечо. DPO отсутствует в обоих плечах. Исходные prompt, response и joint token IDs одинаковы.

| Параметр | A | B |
|---|---|---|
| Model input | Первые `T−1` IDs текущей последовательности, где `T = tokenIds.Count` | Те же первые `T−1` IDs |
| Target contribution | Все текущие next-token targets | Ноль для служебной prompt-части, прежний вклад response targets |
| Нормирование | Текущий знаменатель `T−1` | Тот же `T−1` |
| Backward | Реальный Trainer backward | Реальный backward с той же маской, что и loss |
| Optimizer и число updates | Без изменений | Без изменений |

**Единственный фактор — маска вклада prompt targets.** Знаменатель намеренно оставлен прежним, чтобы не подмешивать отдельную смену общего масштаба градиента. Это response-only numerator при фиксированном нормировании, а не одновременно маска и переход к response-token mean. Второе нормирование при необходимости проверяется другим опытом.

Sample weight одинаков. Для B: `loss = weight/(T−1) × Σ mask[i] × CE_i`; градиент logits каждой строки умножается на тот же `weight/(T−1) × mask[i]`. Фиксированный знаменатель сохраняет коэффициент response contribution до clipping; общая gradient norm, clipping factor и дальнейший AdamW update могут измениться из-за удаления prompt contributions. Это часть эффекта маски, а не обещание равных update norms. Norm и clipping factor сохраняются внутри тех же реальных updates.

Первый и последний scored targets определяются по действительному shifted index `i+1`; span маски относится к `tokenIds[i+1]`, с заранее закреплённой границей `Prompt + LF`. Маска должна опираться на границу response в **тех же joint token IDs**, которые получает модель. Нельзя молча использовать длину отдельно закодированного prompt, если BPE может слить токен через границу. До запуска нужен точный mapping исходных byte spans к этим IDs; если готового API нет, это небольшой явно отсутствующий helper, а не существующая функция по обещанию. Правило для пересекающего границу токена фиксируется до обучения; неоднозначный lesson заменяется до заморозки корпуса. Входные токены и attention context не удаляются, hidden activations не отсоединяются от autograd.

## Измерение во время приобретения навыка

Панель — восемь новых C# случаев из других исходных/алгоритмических семей плюс четыре ранее полезных контрольных ответа, если они имеются. Одинаковые исходные задания и gold используются в baseline, после первого и второго updates. Измерить response-region NLL одним общим scorer, валидность completion, ложные утверждения/полноту документационных ответов, gained/lost cases, время настоящих updates и peak memory. Два разных training-loss значения не сравниваются как один и тот же показатель качества: A и B по определению включают разные слагаемые. Записать отдельно фактический вклад prompt и response, state deltas и optimizer steps.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности для этого не обязателен. В отчёте рядом показываются изменения остальных показателей, включая ухудшения.** Если response NLL стал лучше, а полнота ответа хуже, обе оси видны. Если меняются только gradients/weights без полезного изменения на панели и без экономии обучения, влияние на полезные показатели пока не подтверждено.

## Что нужно реализовать для запуска

Исследовательский supervised execution вариант, передающий одинаковую target mask в loss и backward, и точная привязка маски к joint IDs. Production Trainer сейчас не объявляется уже имеющим этот интерфейс. Это небольшой опыт поверх настоящего Trainer/Model/AdamW; таблица с аналитическими CE без двух updates не удовлетворяет карточке. Защищённая папка и production defaults остаются вне изменений этого документарного аудита.

## Промпт исполнителю

> Выполни E-SFT-RESPONSE-MASK как два настоящих supervised updates на плечо. В A используй текущую full-sequence CE, в B обнули только вклад prompt targets и в loss, и в backward. Model input, joint IDs, исходные файлы, response targets, знаменатель T−1, LR, optimizer/RNG, LoRA и число updates одинаковы. Установи точную границу response в joint tokenization, не подменяй её неподтверждённой длиной отдельно Encode(prompt). Если требуется helper, реализуй только этот исследовательский шов вокруг native learning; не выдавай его за уже существующий production API. CODEX с astra 6 ultracode выбирает случаи и оценивает точные результаты; DPO в этот опыт не добавляй. Для отдельного будущего DPO обе стороны должна создавать внешняя открытая китайская модель. На общей family-disjoint панели покажи baseline, после первого и второго updates, реальные изменения состояния, полезные метрики и стоимость обучения. Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности для этого не обязателен. Рядом покажи все другие изменения, включая ухудшения. Не заменяй опыт mask-only численным тестом, frozen inference или пересчётом отчёта; не меняй protected docs/plans/replanning-possible-diffs.
