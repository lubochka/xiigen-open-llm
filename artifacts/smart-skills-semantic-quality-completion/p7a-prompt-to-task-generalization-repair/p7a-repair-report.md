# P7A: ремонт после parent review

## Вердикт

`FAIL_NEEDS_TRAINING_COVERAGE`. Предыдущий P7A PASS-кандидат отклонен: он делал reserved/eval/holdout строки успешными через кодовый маппинг, подсмотренный из P7 сигналов. Этот маппинг удален.

## Что нашел parent review

Ремонт добавлял в production code точные сочетания сигналов, которые встречались в P7 reserved/eval/holdout строках. Это не было прямым обучением на eval rows, но было eval-informed code repair: deterministic fixture fitting вместо честного обобщения.

## Что исправлено

Производственный маппинг удален из `PromptToTaskTaskPatternUnit`. Runtime снова не использует P7 row id, source family, путь/имя файла, exact prompt, либо точные reserved signal combinations как authority. P7A больше не заявляет PASS.

## Текущий результат

- Зарезервированные строки с положительным runtime-поведением: 0/4
- Отрицательные контроли, прошедшие через короткий маркер: 0/24
- Новый checkpoint не создавался; P6 checkpoint только загружался.
- Eval/holdout строки не использовались для обучения.
- Semantic quality и P7 PASS не заявлены.

## Почему это честный FAIL

P6 candidate checkpoint содержит только две training-approved строки. Этого покрытия недостаточно, чтобы source-disjoint reserved строки с другими безопасными формулировками дали обученный результат без подсказок из P7. Без train-only расширения или заранее утвержденной общей таксономии runtime должен оставаться осторожным.

## Следующее действие

Нужна отдельная bounded phase: добавить новые PromptToTask training-only source-disjoint строки или подготовить общую таксономию, источник которой документирован до P7, затем создать новый checkpoint candidate и заново пройти P7 eval/ablation gate.
