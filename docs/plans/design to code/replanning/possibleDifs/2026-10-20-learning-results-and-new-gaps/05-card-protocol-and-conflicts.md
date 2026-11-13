# Проверка карточек, конфликты правил и обязательный протокол

## Что проверено и чего проверка не удостоверяет

В защищённом каталоге инвентаризированы и проверены текстовые правила 153 experiment cards и `00-execution-index.md`. Пофайловый [card-inventory.json](card-inventory.json) фиксирует имена и наличие текстовых правил. P01–P125 —125 семейств/470 подслучаев, а действующая очередь —32 позиции. Это разные счётчики. Текстовый поиск формулировок служит обнаружению пропуска; он не заменяет проверку смысла каждой A/B и не доказывает готовность 470 hosts.

Формулировки «Любой полезный измеренный…», «accuracy не обязана…» и «любое измеренное улучшение…» эквивалентны по действующему правилу. Проверка не объявляет карточку ошибочной только из-за отсутствия одного буквального шаблона. Для substantive review проверялись causal path, сравнимый learner/state, домен, границы ресурсов и budget; адресные выводы находятся в [02](02-learning-measurement-audit.md), [03](03-knn-units-and-domains.md), [04](04-neural-data-restore-audit.md).

## Обнаруженные конфликты и точное дополнение

| ID | Источник конфликта | Решение в этом пакете | Состояние |
|---|---|---|---|
| P54.1–P54.7 | [P54](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md): каждый reader объявлен OUT_OF_LEARNING_SCOPE, хотя получает learned maps | [06](06-P54-learned-reader-addendum.md): предъявлены writer, consumer и обратная связь внутри learning;0 новых Train допустимо на одном learned snapshot | Семь адресных дополнений; конкретные missing fixture/contract не замаскированы |
| KH-COST-01 | [строка 7 и тело карточки](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/KH-COST-01.md): MethodKnn в обязательном preamble, FileActionKnn в host/data/метриках | Во вводной учебный сигнал читать как **FileActionKnnPredictor.Train**, actual labels→30features→native index; MethodKnn не добавлять. Внутреннее №23→24 заменить при исполнении на **№22→23 KH-EXT-01** | Исправить только формулировку; исходные 2Train/16queries сохранить |
| KH-EXT-01 | Внутреннее №24→25 расходится с protected index | При исполнении **№23→24 GRADB-LOCALITY-01**; постоянный ID и учебный фактор не меняются | Документный override, без изменения protected |
| GRADB-LOCALITY-01 | Внутреннее №25→конец расходится с protected index | При исполнении **№24→25 RMS-COORD-ALLOC-01**, очередь не заканчивать | Документный override |
| Выбор опыта исполнителем | `experimets/03-metrics-and-reporting.md:3` предлагает «сам выбирает и готовит опыт», вопреки назначенной очереди | Брать назначенную строку реестра и полную карточку; недостающий рецепт закрывает автор карточки. [Точное основание и готовая оболочка](09-prompt-handoff-audit.md) | Адресное уточнение; новый host или другой опыт не выбирать |
| BPE status | Историческая ошибка 260-character path до обучения против свежего успешного результата | Использовать завершённый report20.10; не удалять старую failed папку и не повторять ID | Уже закрыт runtime; документация согласована |
| E22 verdict | Historical `passed=false` при .4892→.4944 | Локальный успех macro-F1; время classifier read не объявлять Train speed | Переклассифицирован смысл; raw сохранён |
|400-step diagnostic | Windows local path в report вместо доступного из repository первичного raw | Сохранить как report-level evidence, отдельно от 4-step A/B; не расширять claim/cache budget | Конкретный provenance gap; не основание повторять весь опыт |
| NG01 coverage | Broad N48/P26 есть, точного nullable/int-relational one-factor опыта не предъявлено | Новая [карточка](cards/NG01-null-guard-label.md) и [промпт](prompts/NG01-executor.md), честный refinement старого семейства | Рецепт; runtime не выполнялся |

## Все 14 уточнений пользователя: применение

| № | Правило | Как выполнено здесь и как применять исполнителю |
|---:|---|---|
|1|Небольшой быстро реализуемый опыт, лучше по доменам|NG01: один label factor, C#, фиксированные 12TRAIN/4EXAM и native learner; остальные различия ссылаются на существующие small cards|
|2|Любая полезная измеренная дельта — успех|Явно в каждой новой карточке/промпте; в отчётах BPE/transpose/N-KU и E22 польза отделена от регрессий|
|3|Измерение должно касаться процесса обучения|Нужна цепь training input/update/restore/learned consumer; E26 BuildDefault сам по себе не принят как новое обучение|
|4|Input/result dependencies допускаются с точным кодом и исправлением|P54 с writer→state→reader→heldout feedback; KH-COST с native Train; исходный метод и минимальная правка указываются|
|5|Сначала разблокировать обучение|Очередь начинает с next N62-R1, а не с docs hygiene или зависимых readers|
|6|Сначала экспериментальная копия, без полной product-цепочки|Эта сессия документная; future NG01 работает в назначенном source-copy проекте. Кандидат не означает немедленный prod patch|
|7|Не заставлять искать project/model/commands/conflict resolution|Полный short-path recipe в NG01; модель агента по умолчанию; существующий attempt сохраняется; повтор допускается только по прямо назначенному новому attempt, без самовольного retry|
|8|Старые эксперименты не удалять|Additive файлы и верхний navigation block; защищённый каталог не изменён|
|9|Learning unblock → конфликты → обновления → косвенные карточки|Этот протокол снимает ошибочные запреты; registry отделяет прямой learner от позднего reader; старые №01–03 не повторяет|
|10|Не переносить строгие product forbids на experiments|В экспериментальной копии разрешены диагностические DTO/assert/локальные исключения по рецепту; это не разрешение prod/submit simplification|
|11|Сейчас только документировать и записывать полезный результат кандидатом|[CANDIDATES.md](CANDIDATES.md):3 измеренных кандидата, без prod changes и новых запусков|
|12|Язык, report и skills заданы|Русский report≤350 слов пояснения +таблицы/raw links; candidate≤120 слов; текущая default model и перечисленные ниже skills|
|13|Запрет продолжать прошлую задачу после NEXT/STOP в каждой карточке|Полный обязательный блок в NG01 и его executor prompt; тот же смысл действует в registry и общих документах|
|14|Понятный порядок из registry|[08](08-execution-registry.md):next04, сохранены 31 первая позиция, добавлен NG01 перед final reader; P54 после прямого обучения с явными missing inputs|

## Навыки и разрешение старых инструкций

Применены доступные в указанной ветке навыки: [архитектура](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/skills/architect-behavior-classifier-SKILL.md), [планирование](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/skills/planning-session-startup-SKILL.md), [проверка плана](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/skills/plan-review-SKILL.md), [самопроверка плана](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/skills/plan-self-validation-SKILL.md), [проверка кода](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/skills/generated-code-review-SKILL.md), [prompt engineering](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/skills/prompt-patch-authoring-SKILL.md), [человеческое объяснение](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/skills/human-readable-architecture-audit-SKILL.md), [проверка читаемости](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/skills/output-readability-gate-SKILL.md), [дизайн эксперимента](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/skills/experiment-design-SKILL.md). Пользовательские архивы были прочитаны как исторические инструкции, а не как источник нового права менять core.

Позднее прямое поручение пользователя имеет приоритет: не применять порог+20%accuracy к любому успеху; отрицательная числовая дельта времени/loss/памяти может быть полезна; намеренно дефектный baseline допустим для проверки дефекта; новый run не заменяет старый; не требовать полную product-цепочку, другого LLM или отдельного разрешения на каждый ID/SHA. Здесь не переносится старое указание писать только в protected: текущий пользователь прямо запретил её менять.

## Завершение обычного порученного опыта

Сохранить raw, краткий русский report, фактическое число вызовов и таблицу всех метрик. Успешный полезный результат внести одной записью кандидата в разрешённый этим поручением файл. В данной сессии это `possibleDifs/.../CANDIDATES.md`; чужая ссылка на protected `required-changes.md` не даёт права её править. Один readback; никаких дополнительных runs/hash-аудитов ради красоты отчёта. Дальнейшее внедрение решает Люба.

**НЕМЕДЛЕННЫЙ NEXT/STOP:** если пользователь сказал «заканчивай и переходи», немедленно прекратить прежний опыт и перейти к следующему названному ID либо полю «Дальше». Запрещены дописывание прежнего отчёта, hashes, commit/push, review, уборка, перезапуск и последняя проверка. Если пользователь сказал остановить всё, прекратить любые действия, включая следующий опыт и сохранение служебных заметок. Состояние назвать кратко на русском; незавершённые и неудачные результаты не удалять. Этот запрет действует сильнее обычного checklist завершения.
