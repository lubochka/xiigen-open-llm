# Сверка исполнения переносов и точности экспериментов — 30 октября 2026

## Дополнение при публикации

Текущие parents: core`aab5e5143f2516b92b42ce82606759bb6b4ef0b2`, planning`9b58737da5183da37af5685e86f9e89d3eabe895`. Учитываются R7 evaluators, R8 resolver и P53, появившиеся после основного аудита. R8 закрыт в назначенном объёме по source/5PASS controls; R7 частичен: constant tie ошибочно считается real crossover в новом P30 evaluator, что воспроизведено без Train. Текущая очередь102 —**5DONE/3PARTIAL/1OPEN/1CONDITIONAL**. Ниже4/2/3/1 относится к историческомуcbf3b1d.

Planning README сохранён вместе с добавленными remote bridge29Oct. Веткаllm_plan3 имеет существующую историю без общего предка сmain; этот документный commit сохраняет указанную пользователем ветку и не объединяет две несвязанные истории. Core main является предком целевой ветки. Новая Git-публикация и исходный научный scope разделены.

## Какие версии связаны

Исходные гипотезы этого репозитория проверены на `xiigen-planning@3b7537c267e45002ebab4f44845fe4ce92f204f9`. Код, карточки и результаты — на `llm_mvp_core@cbf3b1d834cbf28baa2bb13baeb1bddc17e21019`, ветка `codex/dpo-implementation`. Последний срез уже включает P52 о rejected annotation, появившийся после присланного списка коммитов.

Это адресный аудит последних переносов и связанных экспериментов. Он не означает независимое воспроизведение всех опытов possibleDifs или семантический аудит всех558 элементов каталога.

В парном пакете изменений core подготовлены:

- `docs/plans/replanning-possible-diffs/fixes-based-on-experiments/104-transfer-and-experiment-audit-2026-10-30.md` — источники, наблюдения, границы и текущие статусы.
- `105-addressed-repair-plan-2026-10-30.md` в том же каталоге — владельцы, зависимости и positive/negative gates будущего кода.
- `106-card-hardening-and-coverage-2026-10-30.md` — покрытие обновлённых P01–P64 и непроверенная область.
- `docs/plans/replanning-possible-diffs/experimets/39-transfer-evidence-contract-2026-10-30.md` — исполнимые требования к переносу, состоянию, запуску и выводу.

Эти новые номера отсутствовали на исходном проверенном SHA и публикуются парными документными commit. Исторические ссылки ниже указывают на фактически прочитанные immutable sources.

## Текущее состояние переноса

Карта103 находит коммиты для14 строк планов88/95. Это полезная проверка исполнения, но не14 независимых доказанных улучшений: warmup повторяется между очередями, есть HOLD и честный отрицательный reachability результат. Сам документ103 ограничивает свой вывод. Остаток очереди102 после последних изменений:

| Состояние | Пункты |
|---|---|
| DONE | R1 normalized ID/counts; R3 own dataset-router arrays; R4 atomic rejected batch; R5 RNG fields |
| PARTIAL | R2 default mutable answer; R6 caller paths/inputs/one-use launch/dependencies при закрытом native exit priority |
| OPEN | R7 effective P30/P40 evaluators; R8 P31 correction resolver; R9 обычный consumer learned route |
| CONDITIONAL | R10 новый опыт только под оставшийся вопрос |

[Исходная очередь102](https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/102-late-transfer-repair-plan-2026-10-29.md), [историческая карта103](https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/103-transfer-queue-commit-mapping-2026-10-30.md).

## Найденные повторения ошибок

| Класс | Подтверждённое наблюдение | Куда перенесён критерий исправления |
|---|---|---|
| Обещание ownership не охватывает default overload | Plain KNN default возвращает mutable answer общей ссылкой; explicit FromCopy работает | P19/P39; plan105/C02 |
| Исправлен один owner, соседний остался прежним |29.RouteClassifier хранит caller arrays и стирает старую память до отказа новой партии | P33/P34/P36/P53; C03 |
| Внутренний learned метод принимают за обычный путь |29.Compile вызывает string overload; controlled labels меняют int ответ, но не Compile | P01/P33/P45 и reached-consumer checks; C07 |
| Исправляется только первый похожий дефект | ProbeNN обновляетW2 передdH иW1 передdEmbedding | P10/P11/P46/P48/P49; C04 |
| Общий runner считается защитой всех entry points | P50/P52 удаляют claim; P50 повторил main под тем жеattempt; P20/P21 output path расходится | P20/P21/P50/P52 и39; C01 |
| Hash/lessons считаются сохранённым learner | P50 нетactualKNN export; P52 полный checkpoint свёрнут вhash | P17/P18/P24/P25/P42/P50/P52; C05 |
| Цена только выбранного quality run | P50main8/8/16 не включает первый такой же проход | P50 и39; C06 |
| Независимость задаётся невидимыми модели ID | P49EXAM4/4Input/Gold pairs уже вTRAIN | P46/P48/P49,fixture35; C10 |
| Адресный эффект называют quality/product эффектом | P47/P51 исправлены вкопии; P52text неразличим | P47/P51/P52; C08 |
| Проверкаshape принимается заполную load validation | HNSWсEntryPoint99 при одном узле загружается, Search падает | P18/P20/P21; C09 |

В аудите выполнены16 source-bound C# наблюдений:10 подтвердили работающие инварианты и6 воспроизвели дефекты. Полный product suite и большие программы не запускались. В ProbeNN проведён один шаг на каждый из трёх диагностических запусков воспроизводителя, суммарно3 updates; научный P48 не повторялся. Пересчёт сохранённых P25/P49/P50/P52 отделён от повторного обучения.

Нельзя расширять learned-bypass вывод на все consumers: `23.DomainClassifierUnit` имеет настоящий learned branch через wrapper12. Задача — проверить нужный публичный caller и его downstream решение;29.Compile в текущем виде остаётся baseline.

## Что уточнено в научных выводах

P25: обученная память19/24 сохранилась в новом процессе; equality24/24 включает пять одинаковых неправильных ответов. Проверен checkpoint API, не универсальная пригодность всехexport/import.

P28:4/4 совпадает сgiven-family baseline, поэтому речь о чтении заданной семьи. P41 улучшил конечную панель2/4→4/4, но не различил выбранный threshold.5 от.7. P43 rule3/4 превосходит learned2/4 на назначенных четырёх примерах. P44 projection не исправила потерю порядка одинаковых шагов; P45 ordinary route вызвал reorder0 раз. P46 убрал ties8→0 при одинаковых12/16ответах — полезная отдельная ось, не accuracy gain.

P48: обе сети получилиNaN после2400updates; причина weighting не установлена. Финальные лишние3NaN не определяют время разрушения. P49:6/16→11/16 при600warm+60new против60new и пересекающихся model-visible pairs; это не доказательство скорости или обобщения на незнакомый input.

P50: learned dependencies дали2/4→4/4 сmutation0/4, но state/runner/расход неполны. Main total16lessons/16graphs/32node calls;source-derived полный счёт52native lesson presentations/60native queries, отдельно8cold observations и2BPE witness fits.

P47: основнойEXAM4/4→4/4,контекстные переходы8/12→12/12,collisions4→0. P51: topic-aware address вкопии2/4→4/4. P52:address labels0/2→2/2 приtext4/4→4/4 во всехtrainedarms — KEY_SHAPE_ONLY. Прямые production consumers и schema ещё требуют адресного переноса.

## Связь с исходными N29 и N33

[N29](29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md) уже предупреждал о маскировке адресной ошибки exact/prefix/structure путями. Новое P52 подтверждает необходимость этого ограничения. Заменить rejected на chosen недостаточно, если annotation недоступна при runtime; teacher provenance и input features должны быть разделены.

[N33](33-NEW-E44-E45-PAIR-TARGET-CONFOUND-2026-10-16.md) отделял исторический отрицательный weighted outcome от target-topology confound. Ссылка на этот исход не должна превращаться в доказанную причину catastrophic forgetting. Действующее правилоD09 сохраняется; отсутствие доказательства причины не является разрешением отменить политику.

## Как читаются обновлённые карточки

В P01–P64 сохранён прежний дизайн и raw history; после заголовка добавлены current note, named check, конкретные WRONG/CORRECT и positive/negative условия. Для прочитанных результатов note устраняет конфликт с устаревшимDESIGNED/SHALL_IMPLEMENT. Для непроверенных результатов новый статус не придуман. P65–P125 и прочие старые гипотезы этой редакцией не объявляются проверенными.

Научный NEXT остаётся в едином execution index core. C01–C10 — очередь ремонта механизмов и приёмки; новый опыт назначается только под конкретный неотвеченный вопрос. Исправление отчёта, устранение ссылки или пересчёт receipts выполняются сTrain0.
