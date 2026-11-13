# Независимая проверка C# preprocessing / completion-memory пакета

Дата: 2026-10-19. Проверяющий: отдельный исследователь DPO/LoRA, не автор KNN findings. Статус: **SOURCE_REVIEW_ACCEPTED_WITH_LIMITS; native .NET NOT_RUN**.

## Вердикт

Три основные исходниковые цепочки подтверждены независимо: RX1 — comment delimiter внутри string стирает code tail; RX2 — удаление interpolation expression меняет AST-представление между raw training и Compile inference; OBS1 — direct conditional invocation `cache?.Add` отсутствует в observations completion memory. Это три механизма, но первые два разделяют один preprocessing boundary и не являются двумя независимыми историческими доказательствами обучения.

Тела соответствующего production-кода одинаковы в external `3036a01acec938f0b83fa4ffbda26202cc35d639` и current `44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76` после добавления copyright. Поэтому основной вывод — **унаследованные ограничения и проверяемое взаимодействие признаков/потребителя**, а не новая code regression между указанными SHA. Ухудшение из-за изменившегося корпуса/флагов/caller пока требует реальных before/after receipts.

## Независимо прочитанные связи

| Проверка | Источник и вывод |
|---|---|
| RX1 | [RegexPreprocessor L15–38][regex]: regex комментариев запускаются перед string regex. Для ASCII URL `https://...` существующий `//[^\n]*` удаляет дальнейший код этой строки; последующая замена строк не восстанавливает удалённое. |
| RX2 | [тот же preprocessing][regex]; [Facade L128–159 и L194–202][facade]; [FileActionKnn L21–38/L75–106][e14]. TrainFileActionKnn строит raw features/labels; Compile меняет source до prediction; координата SimpleMemberAccessExpression теряется при замене `$"{user.Length}"` на `$""`. |
| OBS1 | [MethodFrequencyTable L61–92][calls]. Учитываются только invocation.Expression типа MemberAccessExpressionSyntax. Direct `cache?.Add` использует MemberBinding, который не проходит этот фильтр. Таблица затем возвращает прежний candidate order для отсутствующего receiver [L45–58][calls]. |
| Флаги | [SemanticCompilerOptions L11–29][flags]: E10 default true, E14 default false. Для заявленного E14 downstream эффекта требуется включённый и обученный predictor. |
| Старый E10 | [E10RegexPrep L19–81][old-e10]: syntax-error guard и parse_time_ms, плюс подсчёт node totals; проверки semantic preservation или качества trained KNN нет. |
| Старый E02 | [E02FrequencyRanking L13–60/L79–85][old-e02]: список allCalls и обучение используют тот же extractor, поэтому пропущенный синтаксис исчезает также из знаменателя top1. |

Старый E10 receipt с меньшим числом AST nodes нельзя без исследования конкретного corpus приписать именно интерполяции. В авторском документе этот предел указан. E02 frequency memory — отдельное обучаемое состояние; её rank не является LoRA/DPO метрикой.

## Проверка provenance

Независимо сравнены все шесть file pairs из `source_parity.json`: `FileActionKnn.cs`, `MethodKnn.cs`, `MethodFrequencyTable.cs`, `RegexPreprocessor.cs`, `SemanticCompilerFacade.cs`, `SemanticCompilerOptions.cs`. Для всех шести тела равны после исключения пяти новых copyright-строк.

В local cache при записи добавлен один конечный LF. Git blob SHA рассчитан формулой SHA1(`blob <length>\0<content>`) после удаления ровно **одного** последнего LF. Так получены совпадения всех 12 файлов с API blob SHA из `source_parity.json`; CRLF/BOM-преобразования не требовались. Это проверка источников, не выполнение кода.

## Проверка diagnostic runner

Прочитаны `native/Program.cs`, `NativeProbe.csproj`, четыре fixtures `regex_proxy.py`, сохранённый proxy result, оба авторских документа. Runner вызывает actual repository project; mocks implementation нет. Конфигурация двух facade одинакова, кроме `UseRegexPreprocessor`; E14 включён, E09/E13 выключены. Шесть training sources с переименованными классами и query с другим именем параметра образуют **диагностический тест доступности признака**, а не независимую source-family-disjoint проверку смыслового качества.

Запрошенная review-поправка реализована: JSON теперь содержит actual `QueryStack`, `CSharpQueryDetected`, ожидаемые query/control actions и `QueryExpectationMatched`/`ControlExpectationMatched`. Так отсутствие C# route или выключенный predictor не может незаметно считаться положительным восстановлением. Эти expectation fields не заменяют CODEX quality judge. Нативная компиляция и исполнение не проводились: .NET SDK отсутствует.

Дополнительные рекомендации перед будущим запуском: сохранять actual checkout SHA отдельно от пользовательской подписи `KNN_AUDIT_SOURCE_REF`; выводить TotalPairs рядом с extracted calls; использовать отдельный disposable checkout. `ProjectReference` при build может создавать bin/obj у referenced projects: обещание отсутствия изменения source/model файлов не означает запрет всех временных filesystem writes.

OBS1 ремонта достаточно для проверки direct conditional member invocation. Вложенные `a?.B?.Run()` требуют отдельного receiver-resolution контроля. Case-fold `Read/read` и leftmost `db.Users`/`db.Log` не смешивать с OBS1 единственным изменяемым фактором; автор вынес их в supplemental signals.

## Дополнительная проверка собранных глав 00–05

При финальной независимой проверке исследователь DPO/LoRA сопоставил новые разделы с первоисточниками и передал четыре существенные поправки. Они приняты основным автором: builder Verifier описан как конструирование фиксированного target, а не фактическая оценка; G1 structural signal отделён от C1/C2 preprocessing; в G4-Units явно выбран потребляемый chosen target, изменение только executor rejected обозначено no-op контролем; сравнение trained E25 с helper 29 разделено на опыт внутри проекта 12 и отдельную интеграцию с явным encoder/label mapping. У helper 29 другой вход и набор меток, поэтому прямой A/B не объявляется однофакторным.

Главы 04/05 дополнительно прочитаны: новых существенных ошибок функций/цифр/границ не обнаружено. В поручении уточнены точные поля `OrdinaryTotalPairs` и `ConditionalTotalPairs`. Это проверка исследовательского текста и дизайна опытов; native code и model quality по-прежнему не исполнялись.

## Novelty и достаточность

Мой локальный rg обеих выгрузок по RegexPreprocessor, MemberBinding, interpolation, MethodFrequencyTable и близким терминам не обнаружил прежней точной цепочки. Ближайший prior — [LT-N03][prior-lt] о повторном Compile и переучёте одного документа: иной механизм изменения memory weights. Полный финальный счёт загруженных документов принадлежит audit_inventory; я не выдаю его за собственное построчное прочтение всего corpus.

Исходникового материала достаточно, чтобы включить три кандидата с малыми falsifiable experiments в research packet. Для причинной связи с ухудшением RP01 пока нет model-quality доказательства. Новых нативных тренировок/независимых оценок в этом review: **0**. Не следует раздувать эти результаты до решения для SQL, natural language или всех доменов; подтверждённый source scope — C# AST и completion observations.

[regex]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/Compression/RegexPreprocessor.cs#L15-L38
[facade]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/SemanticCompilerFacade.cs#L128-L202
[e14]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/Prediction/FileActionKnn.cs#L21-L106
[calls]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/Completion/MethodFrequencyTable.cs#L45-L92
[flags]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/SemanticCompilerOptions.cs#L11-L29
[old-e10]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/Experiments/E10RegexPrep.cs#L19-L81
[old-e02]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/Experiments/E02FrequencyRanking.cs#L13-L85
[prior-lt]: https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/47-LANGUAGE-TRANSLATE-INTERNALS-AND-NEW-HYPOTHESES-2026-10-19/03-NEW-KNN-HYPOTHESES.md
