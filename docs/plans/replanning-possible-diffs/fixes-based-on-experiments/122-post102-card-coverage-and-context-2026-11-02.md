# 122. Покрытие карточек и действующий контекст после P102

<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

Это **исторический срез после P102**. Последующее состояние описано в [аудите 123](codex-123-post107-transfer-and-experiment-audit-2026-11-02.md) и [плане 124](codex-124-post107-addressed-repair-plan-2026-11-02.md): R09 terminal metrics уже реализован; сборка Tests исправлена; targeted P85/facade остаётся открытым. Теперь 107 записанных/23 будущих, nextQ108. Остальные требования ниже сохраняются в незакрытой части; старые даты, бюджеты и результаты не переписаны.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->


Дата: 2 ноября 2026. Источник — [аудит120](120-post102-transfer-and-executor-audit-2026-11-02.md), конкретные будущие ремонты — [121](121-post102-addressed-repair-plan-2026-11-02.md). Этот документ фиксирует сделанные изменения документации, не новые исполнения.

## Граница покрытия

Усилены **44 экспериментальные карточки**:16 ранее записанных P01/P02/P03/P05/P83/P84/P85/P89/P93/P96/P97/P98/P99/P100/P101/P102 и все28 пунктов Q103–Q130, которые были будущими на исходном срезе. Перед публикацией concurrent P103 записан; теперь остаётся27 будущих Q104–Q130, все они покрыты. Отдельно усилены **12 карточек переноса**. Это56 адресных карточек core, не56 новых опытов. Семь исходных карточек планирования получают свои отдельные уточнения в planning; они не увеличивают число PRIMARY.

Remote каталог на `15745b357e055055ee9bc58b52cc374d90580961` содержит275 карточек, общий fixture и индекс. Объём каталога — проверка состава, не глубины научного аудита. Полнота методов/чисел всех275 не заявляется. Детальная новая проверка исполнения относится к P102 и переносу P85; ранее открытые продуктовые границы подтверждены чтением владельцев.

## Как предотвращается повтор

Каждая карточка получила конкретную ошибку WRONG, действующий договор CORRECT и адресную приёмку, связанную со своей механикой. Общие правила не размножаются десятками независимых версий: [40](../experimets/40-effective-executor-context-2026-11-02.md) задаёт роли навыков, единый callable scorer, фактического потребителя, стадии попытки, complete cost и реакцию на падение. Start prompt ссылается на него вместо старого неполного списка четырёх навыков.

Не менялись научные doses, назначенные factor/panels и порядок Q. Верхние старые summaries101/25/126 и nextQ102 переведены в исторический контекст. Текущий summary103/27/130 и nextQ104 соответствует таблице после сохранённого concurrent P103; Q102 не превращается в PASS. Исторические результаты не удаляются. У новых карточек исправлены3 отсутствующие ссылки и5 некорректных ссылок `file.cs:line`; публикация новых карточек подтверждена remote tree, поэтому старый вывод «не опубликованы» теперь явно ограничен прошлым срезом.

## Адресная матрица

|Тип|ID|Файл|Защищаемая граница|
|---|---|---|---|
|experiment|P01|[P01-trainable-recipient.md](../experimets/cards/P01-trainable-recipient.md)|Получатель и происхождение изменения|
|experiment|P02|[P02-warm-base.md](../experimets/cards/P02-warm-base.md)|Чтение обученной базы и вместимость ответа|
|experiment|P03|[P03-historical-score-lineage.md](../experimets/cards/P03-historical-score-lineage.md)|История числа не заменяет историю состояния|
|experiment|P05|[P05-response-length-objective.md](../experimets/cards/P05-response-length-objective.md)|Objective, градиент и update — отдельные уровни|
|experiment|P83|[P83-expression-body.md](../experimets/cards/P83-expression-body.md)|Обычное и scoped извлечение не взаимозаменяемы|
|experiment|P84|[P84-lambda-family.md](../experimets/cards/P84-lambda-family.md)|Вложенное тело и обязательства целого метода|
|experiment|P85|[P85-composite-obligations.md](../experimets/cards/P85-composite-obligations.md)|Уже внесённый tiebreak и незакрытая native приёмка|
|experiment|P89|[P89-density-stop.md](../experimets/cards/P89-density-stop.md)|Уже исправленный producer и правильный читатель|
|experiment|P93|[P93-confidence-abstention.md](../experimets/cards/P93-confidence-abstention.md)|Нужны примеры неверного ответа, а не одни предупреждения|
|experiment|P96|[P96-async-orchestration.md](../experimets/cards/P96-async-orchestration.md)|Подпись async и фактическая работа процесса|
|experiment|P97|[P97-learned-graph-order.md](../experimets/cards/P97-learned-graph-order.md)|Результат обучения должен дойти до записываемого графа|
|experiment|P98|[P98-tokenizer-capacity.md](../experimets/cards/P98-tokenizer-capacity.md)|Запрошенная и достигнутая ёмкость|
|experiment|P99|[P99-unknown-token-id.md](../experimets/cards/P99-unknown-token-id.md)|Потерянный ID и правильный текст совместимы|
|experiment|P100|[P100-topk-ties.md](../experimets/cards/P100-topk-ties.md)|Мощность множества не равна достижимости и частоте|
|experiment|P101|[P101-learning-budget.md](../experimets/cards/P101-learning-budget.md)|Изменение состава ответов и причины сдвига|
|experiment|P102|[P102-teacher-forced-decoding.md](../experimets/cards/P102-teacher-forced-decoding.md)|Исправление измерений сохранённого P102|
|experiment|P103|[P103-stack-before-normalization.md](../experimets/cards/P103-stack-before-normalization.md)|Фактический порядок до потери признака|
|experiment|P104|[P104-law-eligibility.md](../experimets/cards/P104-law-eligibility.md)|Допуск закона раньше ограничения кандидатов|
|experiment|P105|[P105-frozen-reference.md](../experimets/cards/P105-frozen-reference.md)|Reference state и влияние всего softmax|
|experiment|P106|[P106-corrected-semantic-target.md](../experimets/cards/P106-corrected-semantic-target.md)|Исправленный смысл должен стать фактическим уроком|
|experiment|P107|[P107-lawful-neighbor-aggregation.md](../experimets/cards/P107-lawful-neighbor-aggregation.md)|Настоящее множество соседей и логические уроки|
|experiment|P108|[P108-metadata-carrier.md](../experimets/cards/P108-metadata-carrier.md)|Три carrier должны донести те же данные|
|experiment|P109|[P109-gold-validity.md](../experimets/cards/P109-gold-validity.md)|Верный эталон и общий callable|
|experiment|P110|[P110-numeric-matrix-cost.md](../experimets/cards/P110-numeric-matrix-cost.md)|Выученная матрица и честная цена окна|
|experiment|P111|[P111-reward-credit.md](../experimets/cards/P111-reward-credit.md)|Награда изменяет выбранного владельца|
|experiment|P112|[P112-family-disjoint-evaluation.md](../experimets/cards/P112-family-disjoint-evaluation.md)|Разделение семей и состав выборки|
|experiment|P113|[P113-feature-energy.md](../experimets/cards/P113-feature-energy.md)|Энергия канала и полезность ответа|
|experiment|P114|[P114-zero-weight-pair.md](../experimets/cards/P114-zero-weight-pair.md)|Нулевой вес пары не означает отсутствие действий|
|experiment|P115|[P115-bm25-statistics-cache.md](../experimets/cards/P115-bm25-statistics-cache.md)|Кэш обновляется вместе с обученной памятью|
|experiment|P116|[P116-e43-file-boundary.md](../experimets/cards/P116-e43-file-boundary.md)|Границы файлов определяют допустимые пары|
|experiment|P117|[P117-translation-direction-memory.md](../experimets/cards/P117-translation-direction-memory.md)|Достижимость KNN не равна полному переводу|
|experiment|P118|[P118-projectwiki-content.md](../experimets/cards/P118-projectwiki-content.md)|Содержимое плана доходит до обычного потребителя|
|experiment|P119|[P119-learning-dynamics.md](../experimets/cards/P119-learning-dynamics.md)|Динамика LR не сводится к отношению weight delta|
|experiment|P120|[P120-semantic-compression-laws.md](../experimets/cards/P120-semantic-compression-laws.md)|Ключевые отрицания и полнота смыслового контроля|
|experiment|P121|[P121-project-adapter-selection.md](../experimets/cards/P121-project-adapter-selection.md)|Имя проекта и фактический adapter owner|
|experiment|P122|[P122-root-channel-unicode-digits-operators.md](../experimets/cards/P122-root-channel-unicode-digits-operators.md)|Нельзя терять знак/цифру при нормировании root channel|
|experiment|P123|[P123-design-conversion-critic-evidence.md](../experimets/cards/P123-design-conversion-critic-evidence.md)|Геометрический критерий и реальный critic|
|experiment|P124|[P124-p3-fixed-discriminator-contract.md](../experimets/cards/P124-p3-fixed-discriminator-contract.md)|Строгий контракт не закрывается частичным успехом|
|experiment|Q125|[P25-cold-restore.md](../experimets/cards/P25-cold-restore.md)|Холодный процесс читает тот же принятый S1|
|experiment|Q126|[SAVED-STATE-PANEL-0407-01.md](../experimets/cards/SAVED-STATE-PANEL-0407-01.md)|Четыре фактора и единый оплаченный parent|
|experiment|Q127|[KNN-ORDER-PERMUTATION-01.md](../experimets/cards/KNN-ORDER-PERMUTATION-01.md)|Позиция слова должна дойти до native KNN|
|experiment|Q128|[PHRASE-COVERAGE-CAP-01.md](../experimets/cards/PHRASE-COVERAGE-CAP-01.md)|Копия composer не аттестует продуктовый cap|
|experiment|Q129|[HELDOUT-ADMISSION-ATOMICITY-01.md](../experimets/cards/HELDOUT-ADMISSION-ATOMICITY-01.md)|Syntax preflight не доказывает полную транзакцию|
|experiment|Q130|[METHOD-KNN-SCOPED-EXPRESSION-TARGET-01.md](../experimets/cards/METHOD-KNN-SCOPED-EXPRESSION-TARGET-01.md)|Scoped target при общем новом tiebreak|
|transfer|FIX-N60|[11-translation-verifier-admission.md](11-translation-verifier-admission.md)|Рабочий проверяющий, а не калибровка его замены|
|transfer|TEST-TMEM|[19-translation-memory-coverage.md](19-translation-memory-coverage.md)|Восстановление согласованной памяти после партии|
|transfer|FIX-MC|[21-sft-optimizer-continuity.md](21-sft-optimizer-continuity.md)|Шаг AdamW не заменяет moments и реальный restore|
|transfer|FIX-KHE|[26-method-knn-training-admission.md](26-method-knn-training-admission.md)|Допуск обучения и tie selection — разные контракты|
|transfer|FIX-KS|[35-method-knn-target-scope.md](35-method-knn-target-scope.md)|Наружное тело expression target|
|transfer|FIX-NH|[36-dpo-scored-row-head.md](36-dpo-scored-row-head.md)|Сэкономленные строки не меняют смысл производной|
|transfer|FIX-BR|[40-bpe-realized-merge-objective.md](40-bpe-realized-merge-objective.md)|Результат Train и downstream capacity|
|transfer|FIX-FBC|[54-frozen-base-check-scope.md](54-frozen-base-check-scope.md)|Сохранность одной попытки не устанавливает виновника старого повреждения|
|transfer|FIX-OSR|[58-optimizer-step-metadata-read.md](58-optimizer-step-metadata-read.md)|Дешёвый step-read не является полным fingerprint|
|transfer|FIX-KNN-DOMAIN|[74-knn-domain-constructor-transfer.md](74-knn-domain-constructor-transfer.md)|Тема обучения и общий механизм не задают один глобальный tiebreak|
|transfer|FIX-PD/TS/SEP|[83-learned-bpe-boundary-transfer-addendum.md](83-learned-bpe-boundary-transfer-addendum.md)|Граница оцениваемого ответа и фактические tokens|
|transfer|STATE-BUNDLE|[101-persistent-state-bundle-contract-2026-10-30.md](101-persistent-state-bundle-contract-2026-10-30.md)|Полный bundle и recovery после уже выданного update|

## Проверка сохранности и источников

Машинная [матрица](../../../../experiments-v2/_audits/2026-11-02-post102-executor-audit/card-coverage.json) содержит WRONG/CORRECT/acceptance каждого файла. [Link repairs](../../../../experiments-v2/_audits/2026-11-02-post102-executor-audit/link-repairs.json) отделяет адресные исправления ссылок от добавлений. При снятии верхнего POST102 блока прежнее тело44+12 карточек должно совпадать с baseline, за исключением перечисленных link repairs и адресных исправлений подписей/происхождения ссылки в text-repairs.json. Эта проверка не считает старые нерелевантные TODO ошибками текущего исполнения.

Raw P102, production sources, Tests и наборы данных этим пакетом не меняются. Публикуемая Python проверка читает сохранённые JSON и моделирует строковые функции; она не является запуском .NET/PowerShell. Independent review и итог статической проверки находятся в evidence pack, границы метода указаны явно.
