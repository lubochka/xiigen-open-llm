# Успешное обучение в соседних ветках и текущей памяти

> **Окончательный срез аудита:** native code совпадает с прочитанным `62bcc8f`; адресная документная сверка доведена до core `ce890a48efda7f00d0ee040f2383cc2d06428ea6` и planning `3b625a83ee878906d42544516741849a16c04689`. В protected **291 Markdown / 128 файлов карточек**. N61 уже покрывают Mode1 и reference reuse; N-KU имеет опубликованную узкую таблицу ответов; N60-E2 конкретизировал writer/state/reader, но host не завершён. Числа 286/126/577 ниже — исходный воспроизводимый срез. [Поздние изменения и приоритет версий](06-CONCURRENT-UPDATES.md), [последняя дельта core](coverage/06-FINAL-CORE-DELTA-ce890a.md).

## Положительные результаты сохраняют смысл в своём механизме

Ниже приведены прочитанные первичные отчёты, закреплённые commit. В этом аудите они **не запускались повторно**. Число из отчёта — историческое наблюдение; source inspection объясняет, что именно измерено. Отсутствие нового прогона не делает прошлый результат отрицательным, а наличие прошлого успеха не гарантирует такой же эффект у другой реализации.

| Ветка / опыт | Записано в первичном результате | Что реально учится или сравнивается | Граница вывода |
|---|---|---|---|
| `language_translate`, [E25][e25] | top-1 **0.3438 → 0.875**, train 128 / validation 32; hard 24/28; время 0.4 → 15.4 ms | POS-pattern память, Jaccard k=3; baseline — статическое правило, а не та же нейронная матрица | Полезный результат классификации шаблонов на синтетических семействах. Время относится к inference; это не измерение стоимости Train и не полный перевод |
| `language_translate`, [E26][e26] | disambiguation **0.4 → 1.0** на 15 примерах; время 0.8 → 2.2 ms | Выбор перевода многозначного слова с учётом контекста | Успех контекстной словарной функции. Не доказательство правильной морфологии и порядка целого предложения |
| `language_translate`, [E43][e43] | retention **0.3226 → 0.3978**; old 465, new 414, replay 177; время 0.1 → 5.8 ms | Новые счётчики биграмм против счётчиков new+replay | Полезное удержание старых переходов. Код не продолжает прежний neural optimizer; слово replay не делает его LoRA-resume |
| `orchestrator-extensions`, [E09][e09] | top-1 **0.2413 → 0.3507**; 5617 tokens, 4492 train pairs, k=5; время 14.5 → 763.5 ms | Соседский прогноз SyntaxKind со структурными признаками | В [коде][e09-code] baseline оценивает весь validation, а kNN — `Take(500)` с 499 переходами. Это не одна и та же панель/знаменатель; численную разницу нельзя приписать только расстоянию |

В E25 [native experiment][e25-code] обучает память структур POS на восьми синтетических паттернах; это полезная функция, которая могла быть неправильно отвергнута при ожидании генеративной матрицы. Но эти отчёты сами по себе не образуют строгое A/B «одна и та же функция в матрице → в kNN»: начальные модели, representation и задачи различаются. Такой перенос заслуживает отдельного маленького предметного опыта, а не нового ярлыка failed.

## Когда функция kNN применима к домену

| Признак | Полезный учебный вопрос | Что нельзя переносить автоматически |
|---|---|---|
| C# AST: kind, nesting, роли, scope | Выбирается ли допустимое действие/продолжение при новой структуре программы? | SQL, обычная русская фраза и Node/TypeScript не получают правильный C# SyntaxKind codec автоматически |
| POS, лексический контекст | Различается ли смысл слова или тип фразы после обучающих примеров? | Хороший POS pattern не гарантирует сохранность отрицания, объекта, вопроса и морфологии |
| Порядок и синтаксические зависимости фразы | Различаются ли «не удалять» и «удалять», кто что делает, модификаторы и их область? | Множество токенов без позиций может считать разные отношения одинаковыми; один fixed reorder не покрывает язык |
| Графовые признаки/flow identity | Выбирается ли обученный маршрут и тот ли output становится следующим input? | Topological sort, PageRank или число рёбер не являются обучением отношений без изменяемого state и reader |
| Фразы и слова в learned memory | Переносится ли конкретное усвоенное сочетание на новую композицию? | Частичный overlap не разрешает удалять отрицание и принимать сомнительный ответ за точный |

Существующая адресация: [P26][p26], [P27][p27], [P43][p43], [P44][p44], [P45][p45], [P55][p55], [P95][p95]–[P97][p97]. Это разные предметные опыты, не универсальная функция «включить AST всем».

## Три подтверждённых результата EN→RU, которые важно не смешивать

### A. Память приобретает умение после уроков

[P125 report][memory-result] сравнивает fresh A без уроков, B с восемью корректными уроками и C с подменёнными targets. Обычный Translation Flow используется как consumer. B получает 19/24 против исходных 0/24: 8 exact recall, 3 near и 8 новых комбинаций. Всего в протоколе две учебные batch-последовательности (16 уроков для B/C) и 144 Flow calls по трём состояниям/панелям; это не 144 урока и не neural training.

Смена target в C меняет ответы (13/24), что поддерживает связь результата с материалом. Внутренняя `accuracyBefore/After` равна 0, тогда как отдельный предметный scorer видит 19/24. Это конкретная причина не подменять качество перевода общей технической метрикой. У B 22 принятых ответа, из них 19 правильных; оставшиеся три требуют отдельного анализа, а не исчезают при отчёте accuracy.

### B. Загрузчик передаёт полезную часть того же урока

[Ingress report][ingress-result] держит одинаковыми восемь уроков, 24 EXAM и независимые экземпляры. В A поле phrase_mappings не доходит до learner; в B доходит. Correct 11/24 → 19/24, novel composition 0/8 → 8/8, accepted coverage 12/24 → 22/24. Ошибочно принятые ответы 1 → 3, потерянных прежних правильных — 0. Это успех по полезному результату с ухудшением другой оси; правило пользователя не требует отменить первый вывод.

В [текущем loader][translation-loader] поле разбирается, в [BuildTranslationContinueExample][translation-continue] оно передаётся в `ExecutorOutput`, а [Executor TrainOnExample][translation-units] сохраняет mappings. Поэтому этот fix уже исходное состояние новых опытов. Одно наблюдение времени с JIT в отчёте нельзя рекламировать как доказанное ускорение.

### C. Восстановление сохраняет именно выученный результат

[P25 report][restore-result] использует один сохранённый обученный state и свежий процесс: 24 Flow calls, ноль новых уроков. Все ответы/маршруты/confidence воспроизведены, 19/24 correct сохраняются. Это валидная зависимость результата обучения. Придумывать повторный Train только для разрешения restore не требуется.

Границы: не доказаны совместимость любых версий, сохранность optimizer/RNG при следующем шаге и общий перенос всех Unit схем. Для нового Wiki taxonomy fresh Train отдельно проверяет расширенную схему, но не миграцию legacy snapshots.

## Что остаётся предметом малых опытов

У памяти остаются ошибки: `do not delete the file` превращается в разрешающее удаление, вопрос может превращаться в команду, а сложное перечисление объектов не всегда удовлетворяет строгому scorer. Это доказательства конкретных ответов из P125, а не доказательство отсутствия всей полезной композиции. Нужны разделённые метрики: правильное действие, объект, оператор/модальность, приемлемый отказ и ошибочное принятие.

У RP01 neural изменения LoRA/loss/margin должны проверяться собственным ответом Student на отдельном материале. Прирост памяти не закрывает этот вопрос. Обратное тоже верно: низкое качество свободной генерации не стирает приобретение полезной памяти или экономию реального Train.

У ProjectWiki текущий расширенный enum и `Labels` больше не согласованы, в отличие от [донора local_wiki_planning][wiki-donor]. Отдельный отказ при нулевом trained signal создаёт проверяемое взаимодействие с прежним skip-on-correct правилом. Эти две новые source findings оформлены самостоятельными micro-experiments, а не выводятся из чужих aggregate процентов.

[e09]: https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json
[e09-code]: https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/35.ExperimentsV2/Experiments/E09StructuralKnn.cs
[e25]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json
[e25-code]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E25PatternClassifier.cs
[e26]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-001.json
[e43]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json
[ingress-result]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md
[memory-result]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md
[p26]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/P26-structural-features.md
[p27]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/P27-structural-codec.md
[p43]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/P43-contextual-pos.md
[p44]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/P44-template-projection.md
[p45]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/P45-learned-reordering.md
[p55]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/P55-translation-laws.md
[p95]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/P95-learned-flow-edge.md
[p97]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/P97-learned-graph-order.md
[restore-result]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md
[translation-continue]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/27.TranslationDomain.Tools/Program.cs#L4682-L4703
[translation-loader]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/27.TranslationDomain.Tools/Program.cs#L4442-L4470
[translation-units]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/27.TranslationDomain/TranslationTrainedUnits.cs
[wiki-donor]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs
