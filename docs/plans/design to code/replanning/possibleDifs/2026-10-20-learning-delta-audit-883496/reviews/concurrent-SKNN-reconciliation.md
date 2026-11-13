# Параллельное добавление AST: SKNN — альтернативный рецепт того же фактора

**20.10.2026. Итог: SAME_FACTOR / SKNN_REFERENCE_ONLY / NOT_RUN.** После исходного среза planning 6bc4 обнаружен опубликованный пакет63 на `9d311be43f4e108f6fa1cda1774549d54941c3c5`. Его [AST-SIB-CACHE-01](https://github.com/lubochka/xiigen-planning/blob/9d311be43f4e108f6fa1cda1774549d54941c3c5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/07-AST-SIB-CACHE-01.md#L3-L25) уже описывает ту же гипотезу, что наш SKNN-SIBLING-PREP-01: на один настоящий Train кэшировать sibling count/ordinal родителя, сохраняя structural vectors и результат обученной памяти. SKNN **не считается новой независимой гипотезой и не получает второго места запуска**. Старые документы и квитанции сохраняются.

## 1. Почему это дубликат фактора, несмотря на разные runner

Обе постановки адресуют один участок [StructuralVectorExtractor.Extract,54–59](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/12.SemanticCompiler/Prediction/StructuralKnn.cs#L54-L59): Parent.ChildNodes().ToList() и IndexOf на каждом ребёнке. У обеих B заменяет повторные обходы временным parent/child cache. Обе сохраняют capped count/ordinal, все остальные fields, target codec, insertion order, k=5, distance, voting и uncached query extractor. Цель — allocations/время полного native обучения; качество ответа/состояние измеряются рядом.

GitHub readback старого source pin `0b76ab950d82534f5904005dda096545505ae307` вернул **тот же Git blob `b3aa5d376be3f93423997674dc015cea7e9fbf58`**, который закреплён для файла на 883496. Каноническая AST-карточка имеет blob `94db0a52c548dd152e1b5eed6f48e917bb712468`. Локальные ранние копии содержат один лишний завершающий LF; после его учёта hash совпадает с указанными Git blobs. Это различие сохранено в `evidence/concurrent-sknn-reconciliation-receipt.json`, не выдано за исходные bytes.

Прежний `knn-sibling-novelty-receipt.json` относится к planning 6bc4 и частично материализованному protected-корпусу того момента. Он остаётся исторической квитанцией ограниченного поиска. Его отсутствие совпадения **не обосновывает новизну после чтения позднего 9d311**. Настоящее дополнение отменяет такой вывод, не удаляя прежнюю работу.

## 2. Что именно различается

| Контракт | Канонический AST-SIB-CACHE-01 | SKNN-SIBLING-PREP-01, только reference |
|---|---|---|
| Реальная граница Train | Полный [TrainFromSource](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/12.SemanticCompiler/Prediction/StructuralKnn.cs#L124-L141): один source/tree→pairs→Train | Полный [Facade.TrainStructuralKnn](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/12.SemanticCompiler/SemanticCompilerFacade.cs#L206-L225): несколько files→pairs→Train |
| Учебные входы | Narrow: 128 methods × 8 statements и wide: 1 method × 1024 statements; по 1024 operators в профиле; сравнение A/B внутри профиля. [данные](https://github.com/lubochka/xiigen-planning/blob/9d311be43f4e108f6fa1cda1774549d54941c3c5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/07-AST-SIB-CACHE-01.md#L83-L100) | Четыре initializer widths16 / 64 / 256 / 512 в одном batch; другой набор и состав AST |
| Бюджет | **4 TrainFromSource**, fresh nA→nB→wB→wA, по 1 Train; **128 native Predict**:16 до + 16 после каждого arm. [паспорт](https://github.com/lubochka/xiigen-planning/blob/9d311be43f4e108f6fa1cda1774549d54941c3c5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/07-AST-SIB-CACHE-01.md#L43-L47), [reader panel](https://github.com/lubochka/xiigen-planning/blob/9d311be43f4e108f6fa1cda1774549d54941c3c5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/07-AST-SIB-CACHE-01.md#L114-L118) | 2 facade Train, 32 facade queries; внутреннее число Predict зависит от nodes. Этот бюджет не прибавляется |
| BEFORE поведение reader | Пустой StructuralKnnPredictor возвращает 0; это настоящий output для direct Predict | Façade до достаточной memory возвращает []; выводы coverage отличаются |
| Cache equality | Default SyntaxNode equality, как у исходного List.IndexOf; TryAdd сохраняет первое вхождение. [B cache](https://github.com/lubochka/xiigen-planning/blob/9d311be43f4e108f6fa1cda1774549d54941c3c5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/07-AST-SIB-CACHE-01.md#L54-L81) | Explicit ReferenceEqualityComparer и родительская таблица другого оформления; перенос этой детали не назначен |
| Host | Source-only StructuralKnn.cs A/B + Program, net9 / Roslyn 4.12; workspace C:/x/ast01 | Facade/Options/Core dependencies и полный приводимый PowerShell/C# рецепт; workspace/build paths иные |
| Доказанный будущий вывод | Цена TrainFromSource и его learned-state/output при данном C# профиле | Дополнительные подробности facade интеграции; её ускорение каноническим AST-опытом не доказывается |

Изменение caller, fixtures или числа arms не делает sibling cache новым фактором. При этом **численные результаты двух рецептов нельзя считать одинаковыми измерениями**, а их бюджеты/paths/cache implementations нельзя смешивать.

## 3. Единственный выбранный будущий вариант и бюджет

Для объединённой программы зафиксирован **AST-SIB-CACHE-01 из пакета 63**. Выполняется ровно его одна назначенная попытка `20261020-01`: nA, nB, wB, wA; 4 настоящих TrainFromSource; 128 Predict; 0 прогревочных/дополнительных обучений, 0 LLM generations. Рецепт SKNN не является запасным automatic fallback при проблеме AST.

Сохраняются canonical source pin 0b76, k=5, data/profile generation, default-equality cache, net9 / Roslyn 4.12, workspace C:/x/ast01 и существующий planning output `possibleDifs/runs/AST-SIB-CACHE-01/20261020-01`. Команды и конфликты — [раздел7](https://github.com/lubochka/xiigen-planning/blob/9d311be43f4e108f6fa1cda1774549d54941c3c5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/07-AST-SIB-CACHE-01.md#L139-L188). Дополнение не создаёт второй output под SKNN, новый attempt02 или дополнительный facade run. Прежние файлы/reference card/prompt не удаляются и не запускаются.

`06-SKNN-SIBLING-PREP-01.md` и `07-SKNN-SIBLING-PREP-01-executor-prompt.md` нашего пакета сохраняются со статусом **REFERENCE_ONLY / SUPERSEDED_BY_AST-SIB-CACHE-01_FOR_EXECUTION**. Их прежние строки priority/NEXT и команды относятся к сохранённому альтернативному рецепту; они не действуют поверх единого registry. Доступность полного кода в reference не разрешает выполнить обе постановки «для подтверждения».

**Уточнение измерения без новых вызовов:** перед Train канонический Program уже разбирает EXAM и вызывает Predict. Поэтому выражение cold training time означает первый Train/его возможный JIT; полностью холодный Roslyn parser не гарантируется. Первый-Train JIT входит в замер как фактически произошло. Повтор/прогрев ради устранения этой оговорки не добавляется. Небольшое или шумное улучшение полезного показателя всё равно сохраняется с остальными изменениями; устойчивый speedup не обещается.

При existing/partial attempt, ошибке сборки, несовпадении source или исчерпании таймаута применяются канонические правила сохранения результатов и отсутствия retry. Не переключаться на SKNN, не увеличивать сумму бюджетов и не удалять уже созданные raw. Содержательный успех/регрессия ещё не измерены; оба дизайна остаются NOT_RUN в этом аудите.

## 4. Единая очередь и STOP/NEXT

Уточнённый общий маршрут: **05–15 → 17–31 → S01 ROPE-TRAIN-TABLE-01 → S02 AST-SIB-CACHE-01 → S03 NG01 → S04 BPE-REALIZED-MERGE-COUNT-01 → исходный 16 IDENTITY-M1-01 → исходный 32 KN-ROUTER-LEARNED-KEY-01 → END**. Исходные числовые ID не перенумеровываются. NEXT всей программы — №05 N62-R2. Здесь36 учётных строк:4 уже завершены,32 будущие; SKNN не добавляет37-ю строку.

AST.next=S03 NG01; NG01.next=S04 BPE; BPE.next=исходный 16. Если поручен только AST, его next=END. Это документный порядок на случай будущего поручения, не разрешение немедленно запускать программу. Поздние числовые32a / 32b / 33 из раннего текста нашего пакета заменяются этой единой ID-картой.

При STOP немедленно прекращаются действия; допустима только необходимая остановка своего уже работающего процесса. При NEXT прекращается прежняя задача и выполняется только явно назначенный следующий ID в разрешённой фазе: без дополнительного отчёта, hashes, review, cleanup, commit или повторного обучения прошлого опыта. Результат остаётся кандидатом для решения пользователя; автоматического production переноса нет.
