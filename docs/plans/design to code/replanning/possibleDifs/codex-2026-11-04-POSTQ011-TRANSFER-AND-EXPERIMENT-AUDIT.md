# После Q011: аудит переносов, достоверность опытов и адресный ремонт

4 ноября 2026. Проверенные ветки: core `codex/dpo-implementation` на `45bd4e371f511e29c053f8117d61f43365abb460`; planning `llm_plan3` на `9cd53517311be504a89f0e04cdcea0afb011bf8c`.

Основные документы в core: [158 — доказательства и выводы](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-158-postq011-transfer-and-experiment-audit-2026-11-04.md), [159 — адресный план](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-159-postq011-addressed-repair-plan-2026-11-04.md), [160 — покрытие карточек](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-160-postq011-card-coverage-2026-11-04.md), [51 — передача проверяемого задания](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/codex-51-postq011-evidence-boundaries-2026-11-04.md). Текущие входы — [реестр переносов](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#executor-next) и отдельный [индекс опытов](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#experiment-next).

## Что установлено

Восстановленные файлы и уточнённые сохранённые результаты продвинули проверку Q002–Q011, но не закрыли продуктовые остатки. Главный новый адресный факт — **Q007: настоящий optimizer importer принимает NaN и неполный для назначенного продолжения Step401, тогда как внешний reader guard их отвергает**. В сохранённом native receipt у этих двух случаев `NativeAccepted=true`, `FullStateAccepted=false`, optimizer digest изменён. Общие 9/9 означают успешную диагностику; исправление самого продукта ещё открыто. [Исходный receipt](https://github.com/lubochka/llm_mvp_core/blob/45bd4e371f511e29c053f8117d61f43365abb460/experiments-v2/RP01-MICRO/P0601/results/codex-review-2026-11-04/native-result-002.json).

Ремонт состоит из двух разных проверок. Численная допустимость moments требует finite m/v и неотрицательного v до атомарного применения. Полнота конкретного continuation определяется trusted expected initialized keyset, step и owner. Общий importer должен сохранить законные empty Step0 и sparse state; требовать moments для всех параметров модели нельзя. Реальный `DpoTrainer` consumer проверяется отдельно от внешнего reader. Детальная матрица positive/negative и before/after —159§2. Общий Q04/FIX-MC остаётся приостановленным.

## Существующие остатки переноса

| Владелец | Подтверждённый остаток | Что сохраняется |
|---|---|---|
| Q01/135-R01-CONCURRENCY | Observed version одного CAS теряется при позднем чтении CurrentVersion; Evaluate не проверяет отмену после publish barrier; нужна ordinary-store/new-process приёмка проигравшего кандидата | Написанные staging/CAS/store, исправленные BOM/hash и late-cancel Train; опубликованная приёмка R17 |
| R128-02/T121 | Два mutable recipient одинакового checkpoint не различаются content hash; отдельные ledgers, ownerless issuer и constructor branch требуют реальной привязки | Законные повторные шаги своего owner, разрешённый sharing и legacy по собственному договору |
| R128-03/P112 | Первый SourceRowId по content отвергает второй законный ID; Dev/Test flags смешаны | Независимая проверка content/family и точные typed refusals |
| R28/R117-04/P115 | Обычный producer сливает null/empty Output до правильного framed hash | Уже реализованные snapshot/cache/framing, положительный смысловой случай |
| P117/R117-06 | Actual export и restored consumer не покрываются expected journal | Закрытый EVID-P117-INDEX и честные частичные результаты |
| P127/R154-D/105§5 | Label-only checks не связывают ordered payload; external authority и выбор обычного consumer ещё нужны | TRAIN9/10→10/10, EXAM5/6→6/6 в обеих попытках |
| P131/R154-C | Checker не связывает raw с native-flow; historical producer и новый verifier нельзя отождествлять; child нужны абсолютные пути | N0/4→4/4, после reader4/4, P8/8 сохранены; H/X0/4 |

Сохранённые 128 raw keys P131 независимо связаны с native outputs128/128. В каждой сохранённой панели P12768 строк route/payload согласованы. Эти положительные факты поддерживают честность имеющихся данных; они не заменяют будущие native corruption controls. [Данные независимого пересчёта](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/experiments-v2/_audits/postq011-2026-11-04/transfer-saved-recount.json).

## Что изменилось в понимании ранних опытов

| Опыт | Подтверждённый результат | Ошибочный вывод, который предотвращает уточнение |
|---|---|---|
| P03/Q004 | B2/4→1/4, потерян E2; T1 NLL улучшилась. Предъявлялся T1, training loss по 21 позициям до update | Улучшение одной NLL не означает рост общей точности; две TRAIN-строки не означают два урока |
| P04/Q005 | A/B2/4→2/4,0 gains/losses;5 ухудшений gold NLL;8 checkpoint восстановлены | Разные rejected нельзя сравнивать как общую accuracy; last step-margin не обязательно final |
| P05/Q006 | TRAIN0/4,EXAM0/2,retention0/2; квалификация после 400SFT0/6 на 21 панели, A/B/EXAM не исполнялись | Число updates не доказывает умение; response mask уже присутствует |
| P06/Q007 | Два настоящих update, одинаковые конечные states, accuracy2/4; vector scalar зависит от числа занятых slots | Изменённые weights не доказывают перенос смысла координат TeachingVector |
| P07/Q008 | Learned BPE merge даёт 0 против 1 pure-response target; honest/mutant saved checker0/2 | Token boundary не является обучением ответу или широкой языковой приёмкой |
| P08/Q009 | SupportT1 2→1 при том же ответе; accuracy2/4; sampler дороже по четырём строкам | Distribution effect не равен gained answer или ускорению Generate |
| P09/Q010 |21 504 Float32-пары и 84 argmax совпадают; accuracy2/4 | Full vs empty-cache prefill не доказывает incremental decode или скорость |
| P10/Q011 |6 derivative comparisons проходят; max abs error0,000026932268, tolerance margin3,713018; accuracy2/4 | Second moment не независимая производная; узкая численная проекция не полный native backward |

Полный source-bound разбор с затратами Q002–Q011 и пределами отсутствующих state/logits/token IDs —158§5. Честные отрицательные и нулевые результаты сохраняются как научные данные. Нового обучения ради их оформления нет.

## Две новые проверки качества исполнения

**Связи objective в P10.** На честном сохранённом оригинале независимый пересчёт logP/reference/beta→margin→loss/derivative согласован 12/12. Копия с изменённым только chosen logP проходит нынешний узкий checker, но нарушает эту цепь в 1/12 строк. Это scope gap будущей полной приёмки, не опровержение исходного gradient result. Полный historical mask/state receipt каждого FD вызова не сохранён; его нельзя изготовить новым запуском. [Воспроизводимый результат](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/experiments-v2/_audits/postq011-2026-11-04/codex-saved-checks.json).

**Deadline запускателя.** P0701/P1001 синхронно ждут build/exe, затем только печатают 600/1200 секунд в log. Текущий source не ограничивает время child. Будущий технический repair должен проверяться искусственным child без модели: normal exit0, nonzero, timeout с остановкой своего process tree и partial logs. Историческое зависание этим аудитом не установлено; научный PRIMARY повторять не нужно.

**Ресурсные окна.** У Q009 отношение сохранённых sampler block medians B/A1,949–2,672, allocation10 120→25 872/25 936 байт. Это не время всего Generate и не процент среднего paired delta. У Q010 широкое wall100,1104→35,3144 мс расходится с медианами actual calls7,0148→7,2893 мс. Единственный прогон с выбросом не устанавливает устойчивый speedup. Разность 91 712 байт/4=22 928 — вычисление по широким окнам с неодинаковой обвязкой, а не invariant native-call cost. Эти границы внесены в соседние карточки.

## Файлы и две очереди

По прежнему фиксированному inventory3502 путей теперь **92 равны original Git identity,3358 сохраняют изменённую identity187,19 имеют иную identity,33 добавлены в исходной дельте**. Дополнительные 43 восстановленных identities после Q003:8P04,8P05,22P05Q01,2P06,по 1P08/P09/P10. Аудит не восстанавливал их повторно; это уже опубликованные repairs. Проверено 130 неизменившихся корневых trees и 7 изменившихся в universe137. Свежей материализации всех LFS payloads и native Load не было. Два shared S1 P11/P12 остаются адресным ремонтом 29-D3/29-W1 по 157.

Исходных результатов записано 131/131. Последовательная QA сохранённых опытов приняла Q001–Q011:11/131, остаётся 120; Q012 не назначен. Переносы идут Q01→Q02(PAIR)→Q03→Q06; Q04 приостановлен, Q05 зависит. Этот пакет не меняет порядок, статусы или научные дозы. Прямой ID пользователя имеет приоритет.

## Что уже исправлено в документах

В core усилены 22 экспериментальные карточки и contracts02/03/101; добавлены отчёт 158, план 159, покрытие 160, договор 51, проверяемые данные и скрипты. Актуализированы четыре навигационных входа. В planning усилены четыре предметных документа:

- [02 — DPO, objective и gradient](02-DPO-DATA-OBJECTIVE-GRADIENT.md).
- [03 — LoRA, tokenization и vector space](03-LORA-TOKENIZATION-VECTOR-SPACE.md).
- [06 — checkpoint restore и adoption](06-CHECKPOINT-RESTORE-ADOPTION.md).
- [26 — независимые stop/target/sampler](26-INDEPENDENT-STOP-TARGET-AND-SAMPLER-2026-10-16.md).

У каждой вставки указаны предметный факт, существующий владелец, сохраняемый положительный случай, отрицательный контроль и область доказательства. Нижние исторические результаты сохранены; новые тексты не маскируются под выполненный native перенос.

Применены проектные навыки архитектуры, планирования/review, анализа кода, постановки опытов, prompt authoring и человеческой коммуникации. Полное перечитывание всех больших skill/AGENTS файлов не заявляется; использованы применимые разделы, для редактируемых документов — проверенные текущие bytes. Независимый исследователь проверил продуктовые цепи и основной план, основной аудитор повторил сохранённые пересчёты и проверил конечный состав.

Fresh native build/test/CLI/Load — **UNRUN**: нужного runtime в этой среде нет. Новые Train/Forward/Generate/model calls —0. Изменены документы и audit evidence; продукт, существующие tests/runners, старые raw, gold и binaries остаются прежними. Публикация плана не закрывает описанные дефекты реализации.
