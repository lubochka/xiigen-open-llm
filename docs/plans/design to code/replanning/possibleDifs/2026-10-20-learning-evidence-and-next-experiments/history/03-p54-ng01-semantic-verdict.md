# Точечная проверка позднего покрытия: P54 и NG01

> **Исторический срез883/53cb.** На окончательном827 №01–06 завершены, следующий LF-03; P54 blanket запрет снят, NG01 имеет protected карточку и новый host, R1 report уже исправлен. Актуальный статус и оставшиеся gaps — [final827 update](../08-final-core827.md), действующие outputs — [root05](../05-executor-contract.md). Старый R2 wrapper/claims не запускать; этот receipt не назначает повтор.


Дата: 20.10.2026. Проверка документная, без сборки, Train/Predict, новых метрик и изменений защищённых файлов. Core закреплён на `883496bca18113bf37e0afe683c75b05a0075048`, planning — на `6bc4b6fba8f14870f5dc4966cf5a344754962d66`. Отсутствие имени документа в автоматическом name-crosswalk само по себе не считается отсутствием содержательного покрытия.

## Вывод для общего реестра

| Поздний материал | Содержательное покрытие protected | Что действительно требуется |
| --- | --- | --- |
| `2026-10-20-learning-results-and-new-gaps/06-P54-learned-reader-addendum.md` | Все семь механизмов уже описаны как P54.1–P54.7. Учебная связь reader подтверждается актуальным кодом. Внутренний запрет запуска противоречит общему правилу той же P54 | Сохранить additive поправку применимости и указать конфликт. Не добавлять семь новых семейств гипотез. До назначения runtime автору закрепить конкретный learned checkpoint, входы и existing host/команду; сейчас это не READY |
| `.../cards/NG01-null-guard-label.md` и `.../prompts/NG01-executor.md` | N48-01/C2 покрывает общую ошибку null-evidence, но не exact int-relational-pattern factor. Документ65 содержит общий prior `IsPatternExpression`, а не этот micro | Сослаться на уже существующий полный NG01-паспорт и prompt как на дополнительную конкретизацию. Не создавать второй NG01. Protected exact recipe для этого подслучая не найден; это семантическое частичное покрытие, а не новая root-cause family |

## P54: семь прежних гипотез, один действующий конфликт

В [нынешней P54:127][p54] прямо разрешены подготовка, восстановление и выдача результата выученного состояния без обязательного нового update. Но [строки144,151,158,165,172,179,186][p54-subcases] по-прежнему маркируют каждый из семи подслучаев как `OUT_OF_LEARNING_SCOPE`, разрешённых вызовов 0, и требуют нового writer/update/feedback. Это конкретный конфликт внутри защищённой карточки; поздний planning-addendum не устранён последующим изменением её вступления.

Текущая причинная цепь проходит через реальные записи и чтение:

1. [TranslationMachine:107–127][tm-train] вызывает `TrainOnExample`, затем `MeasureHeldOutAccuracy`.
2. [ExecutorUnit:404–455][unit-train] сохраняет chosen-результаты в exact, kNN, word и phrase maps.
3. [Execute:264–294][unit-read] читает эти maps в порядке exact → phrase → word → kNN.
4. [MeasureAccuracy:460–481][unit-score] вызывает тот же `Execute`; [TranslationMachine:662–671][tm-score] включает его score в результат batch.

Следовательно, исправление такого reader может менять использование и оценку реального результата обучения. Оно не становится посторонним только потому, что не меняет writer или optimizer. При этом сравнение одного frozen learned checkpoint A/B измеряет **пользу reader для результата обучения**, а не новый приобретённый навык от дополнительного урока. Для такого исследования не надо искусственно добавлять Train ради допуска.

Проверенные текущие Git blobs: `TranslationMachine.cs = fd0e736de3739236b4dde2d6b9d000f5ebae8416`, `TranslationTrainedUnits.cs = 1742abfc9fb617dfc66e4d618e0c169da01a6ccc`. Они совпадают с двумя blobs, названными в [позднем addendum:112][p54-addendum]; актуальная проверка не опирается только на его старый commit.

| P54 подслучай | Новая причина в позднем addendum? | Оставшаяся конкретная зависимость |
| --- | --- | --- |
| .1 greedy segmentation | Нет; тот же bounded coverage selector | Выученные пересекающиеся фразы, фиксированная панель без exact-перехвата |
| .2 filtered ordinal/original index | Нет; тот же reader индекса | Настоящие tokens, где фильтрация создаёт сдвиг, плюс learned fallback entries |
| .3 mandatory phrase coverage | Нет | Learned partial phrase и разрешённый источник mandatory mask |
| .4 исчезнувший разделитель | Нет | Реальная токенизация separator и его доменный boundary contract |
| .5 word-map omission | Нет | Выученные word entries, отсутствие exact/phrase-перехвата и final fallback outcome |
| .6 semantic rerank | Нет | Общий frozen bounded composer, learned candidates, независимый runtime relation supplier |
| .7 early word-map return | Нет | Реально достижимые word/kNN кандидаты и разрешённый contradiction checker |

Эти зависимости перечислены отдельно в [addendum:31–99][p54-addendum]. Необходимый для каждого случая checkpoint с доказанным происхождением там не закреплён. Отсутствие конкретного артефакта в этом рецепте нельзя переименовывать в отсутствие успешной исторической памяти во всех ветках. Правильная запись очереди: «учебная связь подтверждена; конкретные входы/host ещё не закреплены», позже активных блокеров обучения. Новые карточки целесообразны только как точные executable subcases после назначения входов, без повторного открытия тех же семи причин.

## NG01: точный подслучай не совпадает с уже имеющимся C2

[Protected C2:116–126][c2] изменяет TRAIN labels для невызванного/вызванного nested guard и связывания receiver. B получает заранее проверенные метки; там прямо сказано, что это небольшой supervised набор, не готовая правка анализатора. В текущем [FileActionKnn:32–38][file-label] любой `IsPatternExpression` внутри if считается null-condition. Общий тезис о несовершенстве такой эвристики уже указан в [protected65:45][prior65].

[NG01:25–47][ng01] конкретизирует другой однофакторный контраст: direct relational pattern на параметре явно типа `int`, например `if(n is >0)`, не должен считаться защитой nullable `x` перед `x.Length`. B меняет native generator, сохраняя type-pattern, null-literal и nullable-int relational guards. Файл источника имеет тот же актуальный blob `ef7a88a3c4c2540569b9ce6f204d8e4750875501`, что закреплён в карточке. Общий receiver/scope/dominance анализ не добавляется скрыто.

Учебная применимость здесь прямая: изменённые Label передаются настоящему `FileActionKnnPredictor.Train`; сохранённые labels читаются `Predict`. В карточке уже есть literal 12 TRAIN/4 EXAM, точная source-copy правка, короткий host `C:/xg/NG01/Runner.csproj`, `Prepare.ps1`/`Run.ps1`, модель сессии по умолчанию, русский report, счётчики до/после, правила всех полезных метрик, регрессии и немедленный STOP/NEXT. [Код карточки:260–285][ng01] показывает actual Label → rows → Train → Predict, а не отдельный тест эвристики.

Метрика исправленной supervision относится к реально использованным учебным labels и может быть полезным успехом даже без изменения четырёх ответов. Эти два результата необходимо показывать раздельно. Повторяемые AST histograms позволяют говорить о доставке учебного сигнала/family recall; независимое структурное обобщение не доказано. Карточка сама отмечает эту границу. Native запуск и компиляция NG01 в настоящем аудите не выполнялись; наличие полного кода в Markdown не доказательство build success.

**Решение:** оставить NG01 дополнительным конкретным опытом внутри N48-01, зарегистрировать ссылку на существующие card/prompt. Это один точный пробел покрытия protected micro, уже заполненный planning-паспортом. Не выдавать generic C2 за exact duplicate и не писать ещё одну конкурирующую карточку. Порядок исполнения определяется актуальным общим реестром, а не историческими номерами, записанными в позднем пакете.

[p54]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md#L125-L137
[p54-subcases]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md#L139-L188
[tm-train]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/27.TranslationDomain/TranslationMachine.cs#L107-L127
[unit-train]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/27.TranslationDomain/TranslationTrainedUnits.cs#L404-L455
[unit-read]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L294
[unit-score]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/27.TranslationDomain/TranslationTrainedUnits.cs#L460-L481
[tm-score]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/27.TranslationDomain/TranslationMachine.cs#L662-L671
[p54-addendum]: https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-results-and-new-gaps/06-P54-learned-reader-addendum.md
[c2]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/09-additions-44-48.md#L116-L126
[file-label]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/12.SemanticCompiler/Prediction/FileActionKnn.cs#L32-L38
[prior65]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/65-e22-positive-late-run-and-label-transfer.md#L45
[ng01]: https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-results-and-new-gaps/cards/NG01-null-guard-label.md
