# Проверка сохранённых результатов и измерений до/после

**Источник:** защищённые результаты на `0b76ab950d82534f5904005dda096545505ae307`; новый runtime не выполнялся. Цифры ниже взяты из report/raw, арифметика проверена чтением JSON. Статический разбор исходника и повторное чтение результата не являются новым экспериментом.

## 1. Три уже завершённых опыта

| Опыт / домен | Измеренная польза | Одновременное ограничение или ухудшение | Статус |
|---|---|---|---|
| BPE-NO-MATCH-COPY-01 / code-doc | TRAIN allocations3,904,664→1,126,744 bytes: −2,777,920, **−71.14%** | Это выделения в учебном потоке, не peak/RSS; стабильность wall/CPU по одному порядку не доказана | Успешный ресурсный кандидат |
| BPE-NO-MATCH-COPY-01 / en-ru |2,892,824→893,656 bytes: −1,999,168, **−69.11%** | Нельзя автоматически переносить величину на другой корпус/vocabulary | Успешный ресурсный кандидат в этом домене |
| NEURAL-FROZEN-TRANSPOSE-01 / C# sign |13,647,840→12,140,448 bytes: −1,507,392, **−11.04%**, одинаковые learning states/logits | Кэш удерживает+249,856bytes; exact0/2→0/2; wall+0.2567ms, CPU+15.625ms — наблюдения | Успешный ресурсный кандидат с платой retained memory |
| N-KU-01 / C# action labels | После обучения correct1/4→3/4; false ADD_LOOP3→0 | Требуемый ADD_LOOP1/1→0/1: один класс ухудшен | Локальный качественный кандидат; регрессию сохранять видимой |

Источники: [BPE-NO-MATCH-COPY-01 / report.md](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/report.md); [NEURAL-FROZEN-TRANSPOSE-01 / report.md](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/report.md); [N-KU-01 / report.md](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md). Детальный N-KU/BPE raw-разбор — [03](03-knn-units-and-domains.md).

## 2. Frozen-transpose: четыре состояния и реальный учебный переход

[Raw A](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/A/code-sign.json) и [raw B](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/B/code-sign.json) фиксируют native SFT next-token CE→backward→AdamW, по 4 optimizer updates, DPOsteps0, внешних teacher calls0. Seed базы 62001, LoRA62002;1 transformer block,2 adapter layers,4 trainable arrays,12 frozen arrays; vocab256/context64/hidden64. TRAIN — два знака, порядок 0,1,0,1; EXAM — два других близких задания. Это настоящий tiny learner, но узкая проверка, не экзамен свободного C#.

| Качество, одна и та же EXAM-панель | A0 | A1 | B0 | B1 |
|---|---:|---:|---:|---:|
| Средний target NLL |8.015334|7.974560|8.015334|7.974560|
| Exact правильных /2 |0|0|0|0|

ΔA=ΔB≈−.040774 по NLL; difference-in-differences0. До/после весов различаются, frozen state сохраняется, конечные trainable/optimizer SHA совпадают между arms. Совпадение результатов при меньших allocations и есть проверяемый эффект оптимизации. Нулевая exact не отменяет ресурсную пользу и сдвиг NLL, но не доказывает освоение знаков. Сохранённые loss относятся к соответствующим учебным шагам, не заменяют EXAM до/после.

Кэш:8misses/24hits, payload249,856bytes. Это удерживаемое содержимое; accumulated allocated bytes и peak working set измеряют другое. Нельзя вычитать их как взаимозаменяемые «байты RAM». Изменения wall/CPU/process peak опубликованы честно, но одиночное A→B не устанавливает причинный эффект скорости. Новые повторы ради улучшения отчёта не назначены.

### Отдельная 400-step проба

Текущий report ссылается на `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/probe-result.json` и сообщает TRAIN/EXAM1/2 против 0/2. Этот файл по указанному Windows-пути не доступен из данного snapshot как независимо прочитанный raw. Поэтому здесь это **свидетельство сохранённого отчёта**, а не заново верифицированные первичные данные. Оно не включается в бюджет четырёхшагового A/B, не становится 400-step B-cache proof и не доказывает освоение 2/2. Этот конкретный пробел происхождения не блокирует уже подтверждённые allocations четырёхшагового опыта и не назначает повторную 400-step серию.

## 3. BPE: обучение токенизатора — часть учебной цепочки

Два домена × два плеча =4 непустых native `BpeTokenizer.Train`. В каждом 64 merges, vocabulary256→320; tokenizer state и outputs совпали между A/B, roundtrip4/4. Это изменение стоимости **обучения токенизатора**, а не benchmark произвольного JSON-reader. Точный участок будущего изменения — [BpeTokenizer.MergeChunk](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/01.Tokenizer/BpeTokenizer.cs): при отсутствии данной пары не создавать новую копию chunk. Остальные merge-порядок, tie policy, chunks, byte fallback, corpus и vocabulary остаются теми же.

Первоначальный Windows path failure состоялся до Train; он не является последним результатом. Существующий report явно учитывает успешный короткий путь. Нельзя возвращать очередь к старому failed run, удалять его папку или повторять обучение только потому, что старый `status.json` сохранился.

## 4. N-KU: почему улучшение и регрессия должны жить рядом

Исправление построения учебной метки для `do` меняет реальные labels, передаваемые native learner. Совокупный correct вырос 1/4→3/4 и исчезли 3 ложных `ADD_LOOP`. Однако фактическое требование добавить loop стало хуже 1/1→0/1. Из этого следует локальный успешный кандидат, а не универсальное улучшение. Недостаточность признака DoStatement уже замечена раньше; не выдавать её за новую находку NG01.

Отчёт должен сохранять фактические дополнительные controls/вызовы прежнего run:6 обучающих вызовов, а не переименовывать историю в идеальный заранее заданный A/B. Эта документация дополнительных вызовов не делает и не назначает. Метод/данные/ограничение описаны в [исходной карточке](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md) и [03](03-knn-units-and-domains.md).

## 5. Обязательная схема нового измерения

Для изменения алгоритма update фиксировать один общий S0, A0/B0, равный budget, A1/B1, actual state delta, тот же экзамен и отдельные resource windows. Качество сравнивать ΔA, ΔB и ΔB−ΔA, дополнительно конечные A1/B1. Для стоимости Train сравнивать цену одинакового учебного перехода; allocations «до» процесса не подменяют allocations **во время** training. Для reader обученного состояния допустим один фактически обученный snapshot и A/B reader на нём, без фиктивного повторного Train: writer→snapshot→consumer должен быть предъявлен.

Успех — любая измеренная полезная дельта по метрике и домену; accuracy не обязательна. Единичный результат описывается как локальное наблюдение с точным размером панели, без обещания воспроизводимости. Нельзя автоматически требовать полный pipeline, исправление тестов продукта, три модели, дополнительные seeds или большие экзамены до решения Любы о внедрении.
