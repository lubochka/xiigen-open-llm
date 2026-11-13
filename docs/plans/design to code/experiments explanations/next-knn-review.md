# Следующие примеры: RETENTION-M1 и KDP

Проверенный текущий HEAD: `7e051ca7246efde2864f2bee3a427157063d3cc6`. Исторический материал: `846144464a856cd3f118bcfc0165c8117b06c167`. Новых запусков моделей, сборок или тестов нет. Deliverables и репозиторий не изменялись.

## Решение

Для подробного объяснения рекомендованы два разных механизма. **RETENTION-M1** меняет способ накопления входных признаков обучаемой памяти. **KDP** добавляет ручную оценку уместности ответа после обычного поиска; её полезность измерена, а opt-in API уже перенесён в продукт. KDP нельзя выдавать за улучшение обучения весов. P125 ingress исключён: он исправляет потерю уже переданных данных и увеличивает ошибочные принятия.

## 1. RETENTION-M1-01

### Старый способ → обновление

На каждом уроке пара остаётся в KNN-памяти, но InputFeatureWeights маршрута целиком заменяются признаками последнего урока. Это исправно реализованная политика последнего представителя; старый маршрут не имеет накопленного представления всех формулировок.

Для того же FlowRef берётся максимум старого и свежего веса каждого входного токена. Другие поля берутся из нового урока; tokenizer, формула выбора, индекс и составитель плана остаются прежними.

Таблица входных токенов с положительными весами на FlowRef и память правильных пар запрос→план. Максимум весов задан кодом, сами токены и их принадлежность маршруту приходят из уроков. Нейронных весов, LoRA/DPO и optimizer updates нет.

В прочитанном коде нет сломанной реализации обещанной union: A явно присваивает BuildWeights последнего урока, B вводит иную политику агрегации. Это не неправильный локальный индекс HCL и не потеря уже переданного поля ingress. Более широкое продуктовое обещание удерживать каждую прежнюю формулировку здесь не устанавливается; при таком обещании тот же недостаток можно назвать дефектом retention-контракта. Вывод ограничен сменой политики памяти на согласованных уроках.

[RET_A_CODE](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/35.ExperimentsV2/Experiments/Rp01Micro/N61Mode1/RETENTION/A/PromptFlowGroundingModel.cs#L17-L24) · [RET_B_CODE](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/35.ExperimentsV2/Experiments/Rp01Micro/N61Mode1/RETENTION/B/PromptFlowGroundingModel.cs#L17-L37) · [RET_CARD](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/docs/plans/replanning-possible-diffs/experimets/cards/N61-mode1-retention-and-identity.md#L199-L279)

### Точный пример из сохранённых ответов

После уроков `validate payment receipt email → LAB-B` и `validate payment approval ledger → LAB-A` оба варианта правильно отвечают на `validate payment approval ledger audit`. Затем приходит новый урок того же LAB-A: `check invoice consent journal`; правильное тело плана не меняется.

| Вариант после третьего урока | Выбранный план | Фактическое тело |
|---|---|---|
| A | LAB-B | Validate receipt, then email the receipt. |
| B | LAB-A | Validate approval, then persist the ledger. |


[RET_A_EXAMPLE](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/A/raw-answers.json#L191-L251) · [RET_B_EXAMPLE](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/B/raw-answers.json#L191-L254)

В сохранённых весах у A для старого запроса LAB-A получает 0 совпадений, LAB-B —2. У B старые 4 признака LAB-A остаются, поэтому 4>2. Это наш арифметический пересчёт записанных весов и запроса, без вызова модели.

### Результат и цена

| Показатель | A | B |
|---|---:|---:|
| После первых двух уроков |3/4|3/4|
| После третьего урока |3/4|4/4|
| Старый запрос удержан после третьего урока |нет|да|
| Новый парафраз приобретён |да|да|
| Конкурент и отрицательный запрос сохранены |2/2|2/2|
| Пар / маршрутов после обучения |3/2|3/2|
| Входных признаков всего |8|12|
| Allocations третьего Train |10576 B|11752 B|
| Allocations четырёх чтений после него |127136 B|130280 B|

Измеренные добавки 1176B на третий Train и 3144B на четыре последующих чтения малы в абсолютном размере. Надёжная структурная цена — четыре дополнительных признака. Единичные wall/allocations не устанавливают устойчивую цену. Почти вся разница суммарного времени ответов уже существовала до третьего урока:48.7291vs74.2677 мс при одинаковом обученном состоянии. После вмешательства 1.1167vs1.2916 мс за 4 чтения; причинный speedup/slowdown не заявляется.

[RET_A_COST](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/A/costs.json#L1-L104) · [RET_B_COST](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/B/costs.json#L1-L104) · [RET_REPORT](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/report.md#L16-L69)

### Формула и книжная граница

`w_B(v)=max(w_old(v),w_lesson(v)); s(q,w)=Σ_v 1[v∈q]w(v)`. Неотрицательные признаки запроса и покомпонентный максимум дают `s(q,w_B)≥s(q,w_old)`. Это не cosine/Jaccard и не вероятность.

Книга, AppendixB.1,PDF797, использует фрагмент «скалярное произведение» и сумму `⟨x,y⟩=Σx_i y_i`. Книга даёт математическую запись; неубывание здесь выведено из кода. Она не доказывает сохранение любого полного ответа.

[BOOK_RET_INNER](https://github.com/lubochka/xiigen-planning/blob/ea9afbc13f5e6e8463fc74c89cfab3d2e154d62d/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0797-0800-appendix-b-notation-summary.md#L27-L35) · [RET_SCORE](https://github.com/lubochka/llm_mvp_core/blob/7e051ca7246efde2864f2bee3a427157063d3cc6/42.ProjectWikiDomain/PromptFlowGroundingModel.cs#L147-L205)

### Текущий статус и ограничения

Текущий 42.ProjectWikiDomain/PromptFlowGroundingModel.Train L17–24 всё ещё заменяет веса. Исправление FindLearnedPair L74–88 ограничивает извлечение выбранным flow; это отдельный IDENTITY-ремонт, не max-merge RETENTION.

[RET_CURRENT_TRAIN](https://github.com/lubochka/llm_mvp_core/blob/7e051ca7246efde2864f2bee3a427157063d3cc6/42.ProjectWikiDomain/PromptFlowGroundingModel.cs#L17-L24) · [RET_CURRENT_READER](https://github.com/lubochka/llm_mvp_core/blob/7e051ca7246efde2864f2bee3a427157063d3cc6/42.ProjectWikiDomain/PromptFlowGroundingModel.cs#L26-L88)

- Четыре узких запроса: три положительных и один контроль отказа; не независимое языковое обобщение.
- Два согласованных парафраза одного плана, а не противоречащие исправления; union может удерживать устаревшие признаки.
- Все 16 сохранённых ответов проверены по route,FlowRef и целому телу; один верный ID без правильного тела не засчитывался.
- Сравнение выполнено в общей экспериментальной проекции. GetOrchestratorAsync/plugin/полныйFlow wrapper не исполнялись; cold restore не проверялся.
- Порядок и состав токенов отныне не стираются только в InputFeatureWeights; другие поля остаются свежими, поэтому весь learner не становится коммутативным/порядконезависимым.

## 2. KDP01

### Старый способ → обновление

Тот же замороженный encoder создаёт embedding; HNSW возвращает кандидатов; исправный native reader складывает 0.45vector+0.35BM25+0.20pattern и выбирает максимум. Эта общая мера может почти не различать нужное время или текущий этап.

Вызывающая сторона явно подключает каталог фиксированных функций своего профиля. Каждая оценивает вход запроса и вход сохранённого примера; ограниченная добавка меняет порядок этих же кандидатов. Функция не получает Output и не сочиняет новый ответ.

Обучение памяти записывает примеры и их индекс, используя frozen Transformer. Функции времени/подэтапа написаны человеком и от новых уроков не учатся. Успех здесь — более полезное чтение имеющихся знаний, не лучшее обучение нейронной модели.

A корректно реализует зафиксированную fusion-формулу. Изменение добавляет другой критерий уместности, а не чинит ошибку арифметики, индексирования или потерянное поле. Копия native reader совпала с native Unit на 40S0 ответах по сохранённому отчёту.

### Точный пример: RU / S1 / E02

`Задание: Продолжи глаголом писать, без дополнений, с точкой. Префикс: Вчера Олег`

| Кандидат | Родная оценка | Ручная поправка B1 | Итог B1 |
|---|---:|---:|---:|
|S01: пишет.|0.6460025161504745|−0.2|0.44600251615047454|
|S02: писал.|0.643696017563343|+0.2|0.843696017563343|

A отвечает `пишет.`, B1 — `писал.`. Функция читает временной маркер «Вчера» и сравнивает его с временной рамкой входа сохранённого урока; правильное слово уже лежит в памяти. Она не учит прошедшее время с нуля.

[KDP_RU_S1_A](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/KDP01/results/attempt-01/raw.json#L6693-L6735) · [KDP_RU_S1_B1](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/KDP01/results/attempt-01/raw.json#L7334-L7385)

### Историческая A/B-панель после восьми уроков

| Профиль | A | Полезное B | Новых ошибок |
|---|---:|---:|---:|
|RU,время|5/8|8/8|0|
|OR,подэтап|5/8|8/8|0|
|HE,время|6/8|8/8|0|
|CS|6/8|6/8|0|
|EN|8/8|8/8|0|

Эти числа пересчитаны из 336 сохранённых строк, а не измерены новым запуском product Unit. Все эффекты полезных функций уже были на старой памяти S0; ускорения усвоения новых уроков нет. HE не воспроизводит один из четырёх исходных уроков. Сравнительные время, память и размер состояния не измерены; общий пик процесса 73 809 920B не заменяет A/B-цену функций.

[KDP_RAW](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/KDP01/results/attempt-01/raw.json#L1-L14802) · [KDP_REPORT](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/KDP01/results/attempt-01/report.md#L17-L220) · [KDP_QUALIFICATION](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/KDP01/results/attempt-01/qualification.json#L1-L692)

### Формула и её реальная граница

`s0=.45v+.35b+.20p`, где три родные компоненты лежат в[0,1]. `s1=s0+(.2/m)Σ_j w_j z_j`, где `m` — число функций, `0≤w_j≤1`, `z_j=clip(f_j,−1,1)` либо 0 для неприменимой/нечисловой оценки. Деления на сумму весов нет.

Отсюда `|δ|≤.2·mean(w)≤.2`, разность поправок двух кандидатов≤.4. Если старый отрыв строго>.4, переставить эту пару невозможно; при меньшем отрыве полезность зависит от самого признака. Пустой каталог оставляет s0 точно. Итоговыйscore может выйти из[0,1]; это не вероятность. Отдельная confidence=`round(clamp(.50+.49s1,0,.99),2)`, нового порога принятия ответа здесь нет.

Книга Def6.59/Eq6.44,PDF221, называет разрыв «внешний отступ». Определение дано для attention logits; здесь используется алгебра разности оценок. Граница.4 — наш вывод из конкретных проверок веса и clamp в продукте.

[BOOK_KDP_MARGIN](https://github.com/lubochka/xiigen-planning/blob/ea9afbc13f5e6e8463fc74c89cfab3d2e154d62d/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0221-0224-ch06-margin-flatness-and-relative-emergence.md#L44-L50) · [KDP_CURRENT_ADMISSION](https://github.com/lubochka/llm_mvp_core/blob/7e051ca7246efde2864f2bee3a427157063d3cc6/18.CapabilityTraining/KnnCandidatePrinciples.cs#L157-L245) · [KDP_CURRENT_BONUS](https://github.com/lubochka/llm_mvp_core/blob/7e051ca7246efde2864f2bee3a427157063d3cc6/18.CapabilityTraining/KnnCandidatePrinciples.cs#L248-L316) · [KDP_CURRENT_EXECUTE](https://github.com/lubochka/llm_mvp_core/blob/7e051ca7246efde2864f2bee3a427157063d3cc6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L225-L332)

### Что уже перенесено

Commit459958… добавил optional catalog в native Train и WithCandidatePrinciples, повторно использующий то же обученное состояние. ExecuteAsync вызывает ranker перед выдачей сохранённого Output. Встроенные фабрики — только RU-time-frame и OR-predecessor-stage. Пустой каталог сохраняет прежние output/confidence/evidence. Чужойprofile,Infinity/NaN и веса вне[0,1] отвергаются.

Автор commit сообщает 27/27 новых тестов и 222/223 проверок области; один старый census failure отдельно описан. Эти числа не являются новыми 8 строчнымиA/B5/8→8/8. Прочитанные тесты проверяют подключение,совместимость и численные/профильные границы; самостоятельного запуска здесь не было.

[KDP_TRANSFER_COMMIT](https://github.com/lubochka/llm_mvp_core/commit/45995820648eeb7db007e79cac0dc52f0a2f6691) · [KDP_CURRENT_FIT](https://github.com/lubochka/llm_mvp_core/blob/7e051ca7246efde2864f2bee3a427157063d3cc6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L102-L198) · [KDP_CURRENT_FACTORY](https://github.com/lubochka/llm_mvp_core/blob/7e051ca7246efde2864f2bee3a427157063d3cc6/18.CapabilityTraining/KnnCandidatePrinciples.cs#L553-L574) · [KDP_TEST_COMPAT](https://github.com/lubochka/llm_mvp_core/blob/7e051ca7246efde2864f2bee3a427157063d3cc6/Tests/KnnDomainPrincipleLearningTests.cs#L19-L174) · [KDP_TEST_BOUNDS](https://github.com/lubochka/llm_mvp_core/blob/7e051ca7246efde2864f2bee3a427157063d3cc6/Tests/KnnDomainPrincipleLearningTests.cs#L244-L463)

- Ноль новых ошибок на малой исторической панели не доказывает отсутствие ухудшений времени,памяти или на произвольных входах.
- Вычисления функций проходят по каждому уже найденному кандидату; сравнительная цена не измерена и не объявляется нулевой.
- RU/HE/EN тут задачи продолжения суффикса из известной памяти, не общий перевод или владение языком. Род и морфологические признаки не выучивались этим массивом.
- HE воспроизводит 3/4 собственных уроков; улучшение reader6/8→8/8 не доказывает приобретение новой дозы. В текущем factory нет отдельного встроенного HEпрофиля.
- Полезность уже присутствует наS0, поэтому не заявлять ускоренное обучение. Частотная функция не меняет ответы; C#/EN не улучшаются.
- Старый 40/40restore oracle восстанавливал Text/Output/Instruction/Prefix из fixture и функции из Arms. Это не полное доказательство восстановления saved memory; слабость подробно установлена планом 74.
- Opt-in native API уже реализован, но автоматическая маршрутизацияKDR,широкийProductHostE2E,полныйrestore иполная ресурсная квалификация из этого не следуют.

[KDP_RESTORE_LIMIT](https://github.com/lubochka/llm_mvp_core/blob/7e051ca7246efde2864f2bee3a427157063d3cc6/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/74-knn-domain-constructor-transfer.md#L127-L142)

## Почему P125 не становится следующей основной картой метода

Контраст 11/24→19/24 исправляет доставку уже заданных PhraseMappings в существующий Executor. Это ingress-ремонт, по смыслу той же нежелательной категории, что HCL. Отдельный native P1250/24→19/24 показывает действие обучения памяти, но A вообще не обучался: это не сравнение двух исправных способов с одинаковыми 8 уроками.

cc01 do not delete the file становится удали файл; cc04 did you delete the file превращается в команду. Обе строки раньше не принимались. Прежние правильные ответы не потеряны, но ошибочные принятия выросли 1→3.

При дополнительном чтении comparison.json GitHub вернул LFS pointer(size8632), не данные. Поэтому новые самостоятельные raw-счётчики P125 не заявлены; для исключения использован первичный отчёт и явно записанный тип изменения.

[P125_INGRESS_REPORT](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md#L1-L80) · [P125_NATIVE_REPORT](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md#L1-L92)

## Проверка и skills

Самостоятельно перечитаны 16 ответов RET, состояния памяти и записи затрат; 336 строк KDP; реализации A/B-Train; свежий код Unit, ranker и тестов; commit переноса. Обучение, инференс, тесты и сборки не выполнялись. Происхождение источников и номера строк сохранены в JSON. Ранее прочитанные архитектурный, планировочный, проверки кода, промпт-инжиниринга и коммуникации skills применены повторно. Большой plan-review ранее читался адресно, не полностью.
