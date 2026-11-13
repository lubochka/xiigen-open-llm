# Повторно найденные AUT-WEIGHT/AUT-REPLAY: покрытие и поправки к исполнению

20.10.2026. **Это не новые гипотезы.** Источники прочитаны на planning `de270758087502a350c4fcf460acdf1638bdd318` и core `ce890a48efda7f00d0ee040f2383cc2d06428ea6`. Защищённая папка не изменена. Ни экспериментальное обучение, ни build не запускались.

## Где детали уже описаны

| Механизм | Исследовательский prior | Защищённая программа | Оценка покрытия |
|---|---|---|---|
| Один Domain ⇒ numClasses1, rareCountN ⇒ maxWeight1 ⇒ cap Weight>1 | [53/04, AUT-WEIGHT-01][prior] | [15-additions,158–164][weight-card] | Конкретная причинная цепь и native follow-up уже есть. Не выдавать за новую деталь |
| Add новых уроков в replay до candidate evaluation | [53/04, AUT-REPLAY-02][prior]; [54/04 уточнение][prior54] | [15-additions,166–172][replay-card] | Уже разделены candidate rejection, качество уроков и следующий учебный цикл. Не называть любой непринятый cohort отравлением |
| Междоменное вытеснение при общей capacity и domain-filtered Sample | Отдельного этого owner/interaction в полном поиске не найдено | P15/E43 обсуждают replay вообще; AUT-REPLAY02 — другой момент принятия решения | Новый узкий условный вопрос; полная карточка в [N63-AR-CAP](02-N63-AR-CAP-EXPERIMENT.md) |

Защищённые карточки уже содержат правило «любое фактически измеренное полезное улучшение — успех по этому показателю/домену; accuracy не обязательна», minExamples5, различие supervised и DPO, actual update counts, запрет подменять membership качеством. Это полезные исправления, их не надо переписывать или объявлять отсутствующими.

## AUT-WEIGHT-01: что требуется уточнить в новом append-only дополнении

[Текущий caller,116–147][weight-caller] допускает только один Domain и вызывает weighting до PrepareBatch. [DatasetWeighter,26–64][weight-code] реализует `N/(numClasses*rareExamples)` и `Min(e.Weight*maxWeight,maxWeight)`. [IncrementalTrainer,645–662][consumer] передаёт вес в настоящий supervised update. Потеря относительного веса подтверждается source. Польза сохранения веса не измерена; provider с Weight1 делает изменение no-op.

Карточка 158–164 предусматривает 8 преобразований с 0 updates и затем условное настоящее обучение. **Восемь преобразований не должны исполняться как самостоятельный опыт качества.** Значения weights достаточно записать на входе/выходе экспериментальной копии `Weight` внутри того же запуска настоящего learner. Изменение только в fixture после `Weight` не проверит исходное место потери приоритета.

Чтобы отделить восстановление приоритета от простого увеличения общей массы loss, полезен заранее заданный третий контроль: пять исходных весов `[1,4,1,1,1]`; A получает текущие `[1,1,1,1,1]`; B — сохранённые относительные веса, нормированные к той же общей сумме 5: `[.625,2.5,.625,.625,.625]`. Разница между A и B — распределение фиксированной массы по тем же пяти примерам; дополнительное плечо с необработанными `[1,4,1,1,1]` допустимо только отдельной заранее назначенной версией, а не после результата. Такой B изменяется **в экспериментальной копии DatasetWeighter.Weight**, не прямо в production и не посредством подмены Domain. Это предложенное уточнение ещё не выполнено и не заменяет исходную карточку молча.

Условия: общий checkpoint/tokenizer/LoRA/optimizer, одни тексты и порядок, пять supervised calls/arm, одинаковый суммарный вес, фиксированная clip norm и LR. До и после — response NLL на редких исключениях, обычных вычислениях и прежних сохранённых правилах отдельно. Native returned train loss вычислен до update и не служит after score. Пер-сэмпловое масштабирование при AdamW может почти компенсироваться, поэтому измерять реальные native updates и EXAM; отношение 4:1 само по себе не доказывает приобретение навыка. Любой выигрыш полезной метрики — успех на этой оси, регрессии показываются рядом.

Открытый остаток protected-card исполнения — точный host/CLI/cold-state source для этого механизма: строка 164 говорит «после установки настоящего caller и decoder», но не задаёт их как существующие. Нельзя объявить это уже воспроизводимым native run. Весь product pipeline ради такого изучения строить не нужно: исследовательский thin host может вызывать тот же `Trainer.TrainSupervisedNextTokenStep`, как полностью описано в N63-AR-CAP, но перенос такой orchestration в **отдельный опыт AUT-WEIGHT** требует собственных новых named files и freeze. Настоящая текущая документная работа такой runner не создала и не выполнила.

## AUT-REPLAY-02: что считать отказом и что измерять

[AutonomousTrainer,190–199][commit] добавляет cohort после technical success/provenance; [Pipeline,268–315][evaluate] только потом оценивает candidate и сравнивает accuracy. Это унаследованный порядок, уже установленный 53. Из него нельзя вывести, что уроки неверны или что буфер следует очищать по `accuracy_not_improved`.

Принципиально разделить три свойства: `training_completed`, `lesson_validity`, `candidate_quality_by_metric`. Хороший урок, который улучшает loss/retention/cost при той же accuracy, удовлетворяет нынешнему пользовательскому правилу полезного улучшения. Технический сбой evaluator не устанавливает ни хорошесть, ни плохость его содержания.

В новом дополнении к карточке решение о положительном replay должно иметь заранее заданный источник. Основной различающий пример — **явно invalid lesson / accepted=false на уровне урока**, например сохранённый в protected170 неправильный ответ `Always returns x plus one.` для функции, бросающей исключение при x<0. Он остаётся в архиве как negative/warning; он не удаляется и не предъявляется следующим положительным supervised target. Отдельный контроль — корректный, но пока не давший прироста accuracy cohort; его нельзя автоматически считать rejected lesson. Fault evaluator — самостоятельное состояние pending, не метка «ошибка знаний».

В learning A/B оба плеча должны начать **следующий цикл с одного accepted model checkpoint и одинаковых optimizer moments**, иначе сравниваются два эффекта: различный исходный learner и различная память. Размеры buffers и следующего batch одинаковы (5 новых+2 replay), а отличаться может только допуск действительно отвергнутого положительного target. Старые положительные строки в B дают замену тем же двум слотам; не добавлять больше replay или свежих уроков. Нужен второй настоящий native update и before/after old/new response NLL/полезные ответы; одна membership таблица не проверяет влияние на обучение.

Protected172 уже честно требует saved lifecycle state и учитывает дополнительный первый цикл, если его надо создать. Поэтому незавершённость этого input нельзя скрывать успешным source probe. Одновременно синтетический research fixture с честным статусом и настоящим native learner допустим по нынешнему пользовательскому исключению для экспериментов: его нельзя назвать найденным историческим жизненным циклом или использовать как разрешение product mutation.

## Десять правил для любого будущего дополнения этих двух карточек

1. Малый доменный эксперимент проверяет гипотезу на реальном learner; не запускается вся система ради предварительной проверки.
2. Любое измеренное полезное улучшение — успех по показателю и домену; рост accuracy не требуется, остальные изменения и потери рядом.
3. Weight arithmetic и replay membership не образуют отдельное доказательство качества; они включаются в трассу того же native learning A/B.
4. Подготовка входов/restore допустима только с указанным consumer: Weight/Add/Sample → actual supervised update → post measurement; точный будущий ремонт в этих исходных методах.
5. Эти условные механизмы не вытесняют разблокирование уже действующего RP01 обучения.
6. Копии алгоритмов и новые experiment files до доказательства пользы; production и полная test chain — только после решения пользователя.
7. До запуска новой версии должны быть зафиксированы конкретные файлы, данные, CLI, бюджет и поведение при конфликтах; нынешнее OPEN_HOST не превращать в выдуманную runnable команду.
8. Старые карточки/результаты/эксперименты не удалять и не перезаписывать; здесь разрешён только append-only companion.
9. Приоритет: learning unblock → конфликты → обновления пользователя → косвенные проверки.
10. Research exceptions разрешают изолированный native host и честные synthetic fixture; они не отменяют product admission/submission и не оправдывают фиктивный teacher receipt.

### Промпт на уточнение AUT-WEIGHT

> Подготовь отдельное append-only дополнение AUT-WEIGHT-01. Гипотеза уже известна; не переименовывай её в новую. Реализуемое вмешательство должно находиться внутри экспериментальной копии DatasetWeighter.Weight, одинаковые пять уроков/пять updates, общая инициализация и optimizer; сравни cap против относительных весов с общей суммой 5. До/после измеряй priority-slice response NLL, обычный slice и retention; не ограничивайся массивами весов. Назови конкретный новый host/CLI и данные до запуска, не объявляй условный protected follow-up выполненным. Выполни десять правил выше: малый доменный native опыт; любой полезный gain без требования accuracy и с регрессиями; никаких standalone proxies; точная input/restore→update→EXAM связь; сначала learning unblock; только экспериментальные копии до решения пользователя; фиксированные files/CLI/conflicts; только добавления; указанная очередность; research exceptions только вне prod/submission. Источник, бюджет и gold после наблюдения результата не менять.

### Промпт на уточнение AUT-REPLAY

> Подготовь отдельное append-only дополнение AUT-REPLAY-02. Не называй любой rejected candidate плохими уроками: отдельно зафиксируй technical completion, lesson accepted=false/invalid и useful quality deltas. Сохраняй полный архив, меняй только допуск неверной положительной строки в следующий replay. A/B начинают следующий цикл из одного accepted checkpoint/optimizer, получают 5 новых+2 replay, затем настоящий native update и before/after loss/retention. Корректный урок без accuracy gain — контроль, не повод удаления; evaluator fault — pending. Не подставляй invented lifecycle вместо исторического evidence. Соблюди десять правил: малый native domain опыт; успех по любой полезной оси с потерями рядом; не membership-only; точный consumer и адрес ремонта; learning first; только новые experiment files; заранее paths/CLI/conflicts; не удалять старое; learning→конфликты→updates→indirect; исключения эксперимента не разрешают prod/submission/fake receipts.

[prior]: https://github.com/lubochka/xiigen-planning/blob/de270758087502a350c4fcf460acdf1638bdd318/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/53-EXTERNAL-DOMAINS-LEARNING-SEMANTICS-AUDIT-2026-10-19/04-AUTONOMOUS-LEARNING-CONDITIONAL-GAPS.md
[prior54]: https://github.com/lubochka/xiigen-planning/blob/de270758087502a350c4fcf460acdf1638bdd318/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/54-DESIGNTOCODE-CLEAN-RECOVERY-2026-10-19/04-DOMAIN-MICRO-EXPERIMENTS.md#L95-L110
[weight-card]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/15-additions-designtocode-learning-semantics.md#L158-L164
[replay-card]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/15-additions-designtocode-learning-semantics.md#L166-L172
[weight-caller]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/16.AutonomousLearning/Pipeline/AutonomousLearningPipeline.cs#L116-L147
[weight-code]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/16.AutonomousLearning/Preprocessing/DatasetWeighter.cs#L26-L64
[consumer]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Training/IncrementalTrainer.cs#L645-L662
[commit]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/16.AutonomousLearning/Training/AutonomousTrainer.cs#L190-L199
[evaluate]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/16.AutonomousLearning/Pipeline/AutonomousLearningPipeline.cs#L268-L315
