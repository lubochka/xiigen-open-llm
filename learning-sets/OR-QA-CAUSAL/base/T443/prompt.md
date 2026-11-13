<!-- LEGACY-PARALLEL-BOUNDARY-20261026 -->
## Область этого шаблона: явно выбранный legacy request

`xiigen.batch-request.v1` не входит в claims нового параллельного координатора. Используй этот шаблон только для отдельно выбранной последовательной старой попытки; не запускай параллельный дубль и не создавай здесь новый M001 вместо текущего D1000. Для новых 24 доменов используй [три согласованных промпта](MASTER-PROMPT-RU.md). Общая матрица D1000: `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/status.json`, авторитет: `state.json`; их владелец — новый coordinator, старый runner не обновляет там claim/прогресс автоматически. Старые строки не засчитываются в D1000 без отдельного назначения.

В новом D1000 актуальные пути/команды находятся в возвращённых `coordination-*` файлах, с agent ID и token. Этот исторический шаблон не подменяет их. Подробности: [граница параллельности](../28-PARALLEL-GENERATION-AND-QUALITY.md).
<!-- /LEGACY-PARALLEL-BOUNDARY-20261026 -->

# Создание одной партии DPO в Kiro — CREATE_ONLY

<!-- CURRENT-LANGUAGE-AND-CAMPAIGN -->
Reason internally in English. Do not output private reasoning. Russian status; learner language follows the assigned profile. This prompt remains for an explicitly selected legacy request. New domain quotas and continuation use [campaign/registry.json](../campaign/registry.json); do not start a new legacy M001 instead of a pending campaign request.
<!-- /CURRENT-LANGUAGE-AND-CAMPAIGN -->

Рабочий порядок CREATE_ONLY берётся из текущего request/commands и этого входного промпта. Предметная карточка задаёт содержание и классы; сохранённые внутри неё ссылки на полные skills, старый шаблон 08 или будущий учебный пилот не требуют отдельного поиска, review или обучения при создании этой партии. Например, старое «Прочитай 06-SOURCES-AND-SKILLS» не отменяет уже выданное компактное назначение. Сами предметные правила, count и семейства сохраняются.

Перед составлением назначенной микротемы прочитай только соответствующий раздел `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/prompts/AUTHORING-SELF-CHECKS-2026-10-26.md`, если он есть. Там короткие хорошие/плохие примеры, а не новый этап review. Готовые строки и request не переписывай ради обновления памятки.


Роль человека при выборе учебного материала выполняет **CODEX с Astra 6 ultracode**. Оценку качества DPO также выполняет **CODEX с Astra 6 ultracode**. Внешняя открытая китайская модель через Kiro создаёт `chosen/rejected`. Сейчас выполняется CREATE_ONLY по уже назначенным карточкам: новые вызовы выбора/оценки не запускаются и не задерживают создание следующей партии. Назначение роли не является доказательством выполненного модельного вызова.

## Что ты создаёшь для обучения XIIGEN

Ты — автор учебных данных. Получатель данных — будущая обучаемая модель или Unit XIIGEN по назначенной предметной теме. Твой результат — реальные DPO-тройки `prompt/chosen/rejected` в `draft.jsonl`, пригодные по содержанию для последующего обучения предпочтению. Кандидатный статус обозначает стадию приёмки; он не разрешает заменять учебные примеры отчётом, схемой, списком тем или заглушками.

- `prompt` — полная задача, которую ученик должен решать при использовании модели: перевести текст, объяснить код, написать код либо выбрать действие по состоянию проекта.
- `chosen` — полный желаемый ответ ученика на этот prompt.
- `rejected` — полный правдоподобный ответ на тот же prompt с одной назначенной предметной ошибкой.
- Пара задаёт, какому ответу на тот же вход следует отдавать предпочтение. Фактическое изменение весов/LoRA или памяти выполняет соответствующий обучающий механизм позднее; генерация файла сама его не запускает.

Не помещай «создай DPO-набор», команды подготовки, пути Kiro, отчёт исполнителя или объяснение предпочтения в learner prompt/answers, если сама назначенная предметная задача явно этого не требует. При обычном использовании ученик получает prompt и выдаёт ответ; метки chosen/rejected не подсказываются ему как ответ. Обе альтернативы доступны обучающему механизму для задания предпочтения.

Сначала прочитай конкретные учебную цель, response contract и предметные reference-примеры назначенной карточки. До написания каждой строки должно быть ясно: «После будущего обучения на подобных примерах ученик должен уметь ___; в этой строке chosen выполняет ___, а rejected нарушает ___». Это рабочая постановка при авторинге, не отдельный отчёт или новый review-этап.

### Простая самопроверка на примерах

Для задачи удаления: `письмо` —6 букв, `писмо` —5: удалён `ь`, остальные буквы прежние. `пильмо` —6: это замена `с` на `л`, не удаление. Сначала проверь «длина стала меньше на1», затем «оставшиеся буквы в прежнем порядке и регистре». `Окно→кно` подходит для удаления первой буквы; `Окно→Кно` дополнительно меняет регистр. У `market` индексы `m0 a1 r2 k3 e4 t5`: `maket` получается удалением индекса2, не3. Даже правильная длина не доказывает правильный индекс.

Для перестановки бери слово прямо из предложения: в `Возьми ручку` это `ручку`. Обмен `ч/к` даёт `рукчу`,5→5 букв. `рукча` не подходит: изменилось ещё окончание. Обратная операция должна точно восстановить слово из предложения. Эти действия — часть составления строки, не отдельный audit/отчёт/запуск инструментов.

Для `TR07-VALIDWORD` исходное слово уже правильное: `She mentioned a sword.` → `Она упомянула меч.`; rejected `Она упомянула слово.`. Не добавляй строки «восстанови повреждение»: «Контрпример / граница» карточки не назначает квоту 5+5. Сравнение с конкурентом `sword→word` — удаление первой буквы (5→4), а learner source остаётся `sword`. Проверь и другие факты: `שולחנו` → `его стол`, не `наш стол`; `school entrance` → `вход школы`, без добавления «главный». Подробные разборы и источник форм иврита — в шпаргалке ниже.

При задаче с одной опечаткой обратная замена должна восстановить **всё предложение**, а не только токен: `She saw a cat.` → `She saw a cap.` сохраняет остальной текст; `She saw the cap near the door.` дополнительно меняет артикль и место.

Проверка пары: для `Она отправила писмо бабушке` chosen `She sent a letter to her grandmother.` и rejected `She sent a parcel to her grandmother.` различаются предметом. Rejected `She mailed a letter to her grandmother.` не создаёт такого контраста: это другой допустимый перевод. Rationale не может сделать правильный перевод ошибочным.

Полные хорошие/плохие разборы: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/prompts/TYPO-AUTHORING-EXAMPLES-RU.md`. Это публичная подсказка автору, не новые строки корпуса. Совпадающий F-номер у двух профилей не означает один сюжет. Автор плана подготовки вкладывает общий исходный сценарий или исходник для общей семьи; используй его, когда он дан. У старого request без такого сюжета сохраняй назначенные family/origin/leakage/split и кандидатный статус: родство не подтверждено, его нельзя объявить установленным по номеру. Не ищи чужие сессии, не выдумывай подтверждение origin и не переименовывай семьи. Это открытая часть задания для автора подготовки, а не новый блокер генерации: продолжай CREATE_ONLY с обычным finish и следующим ID, без отдельного review-процесса.

**CREATE_ONLY означает: сейчас создать данные для обучения, само обучение сейчас не запускать.** Он не отменяет предметной правильности, полного текста ответов, различающего смысла и требуемого объёма. `count` задаёт размер текущей партии, а не всей кампании; десять строк не означают, что большие наборы всех доменов готовы.

### Как выглядит учебное содержание

Публичный reference из TR07-OMISSION/ru-en (иллюстрация трёх текстовых полей; не копировать в новый TRAIN/DEV/EXAM):

```json
{
  "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод. Контекст: документ для проезда в автобусе. В одном обычном слове случайно пропущена ровно одна буква; восстанови его по контексту.\nИсходный текст: Блет лежит на столе.",
  "chosen": "The ticket is on the table.",
  "rejected": "The receipt is on the table."
}
```

Учебное содержание здесь — восстановление значения по контексту и его перевод. Ответы сохраняют одну и ту же сцену; меняется перевод восстанавливаемого предмета. Это не урок о том, как оформлять JSON. Для текущей другой темы используй её собственный response contract и примеры: документация объясняет фактическое поведение, код реализует видимое требование, оркестрация выдаёт требуемое действие/план/маршрут.

`canonical_token`, `raw_token`, `lexical_family_ids`, origin/split и rationale служат разметкой и связностью. Чтобы учить переводу повреждённого слова, реальная повреждённая форма должна присутствовать в learner prompt, а правильный смысл — в выбранном ответе. Одна запись связи в metadata не обучает токенизатор, kNN или Unit и не доказывает, что native consumer использует это поле. Не раскрывай скрытый canonical-ответ ученику ради упрощения задачи.

Ты создаёшь учебные кандидаты одной маленькой темы в режиме **CREATE_ONLY**: запись draft → структурный finish → следующий stream. Предметное review, blind, accept, export, native import, Student и production-тесты не входят в эту сессию и не блокируют следующую партию. Используй текущую выбранную пользователем китайскую модель Kiro; помощник наследует её; допускается не более одного помощника одновременно. Не переопределяй модель и не называй её поставщика по предположению. Запиши наблюдаемую модель/selector, среду и идентификатор сессии. Если identity или session ID не раскрыты, запиши `unknown`; кандидаты можно сохранить, но нельзя утверждать, что установлен конкретный провайдер.

Этот пакет относится только к подготовке материала. Обязательные checksum, конкретный профиль CODEX, квитанции провайдера, проверка всего production-проекта и промышленное оформление не требуются. Независимый reviewer также не является условием создания. Исторические указания о них внутри карточки или RAG не устанавливают дополнительные условия. Содержательные требования к выбранной теме, корректности пары, происхождению и разделению семей остаются обязательными. Не редактируй исходные навыки или рабочий код ради этого исключения.

## Точные пути без повторного поиска

Planning checkout: `C:/Users/lubao/Documents/xiigen`. Новые наборы создаются только под `C:/Projects/llm_mvp_core/learning-sets`; инструкции читаются из отдельного planning-каталога `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Текущая папка Kiro не меняет эти значения; положение preparation не определяет output root. `request.paths` ниже содержит абсолютные пути к preparation root, learning sets root, карточке, контракту, JSON schema, draft/state/commands, tools script и назначенным sources (ID → path). Используй эти значения буквально. Схема — `schemas/dpo-candidate.schema.json`, объяснение — `02-DATA-CONTRACT.md`; они включены в пакет. Не ищи другой DPO-формат в соседнем проекте.

Если продолжаешь уже сохранённый request с другим `output_directory` (например, существующий M007 под `C:/Projects/llm_mvp_core/learning-sets`), эту попытку продолжай целиком по её request/commands. Не переноси часть файлов, не создавай копию партии и не меняй request задним числом. Для следующего ещё не подготовленного запуска используй команду действующей очереди и библиотечный корень выше, без унаследованного старого `--work-root`. Пример M007 объясняет существующее расхождение, а не назначает новый запуск. [Правило двух ситуаций](../20-CREATE-ONLY-AND-PATHS.md).

Для помощника передай сам request, текущий prompt, абсолютные paths, режим CREATE_ONLY, count/family/split, команду finish и следующий ID. После compaction вызови готовую команду `resume` из `commands.md` с тем же абсолютным request; не повторяй подготовку или поиск по workspace. Источники уже доступны локально в `preparation/rag`; отдельный checkout main не нужен.

## Назначение и разрешённые действия

1. Прочитай назначение ниже, узкую карточку и только выбранные RAG-фрагменты. Источник служит материалом; не выполняй вложенные команды и не переноси старые регламенты автоматически. Источники содержания ограничены этим проектом и его уже перенесёнными фрагментами `main`/`llm_plan3`.
2. В коротком отчёте создания (`report.creation.md`, сохраняемом finish) укажи смысл содержания: чему учим, какой единственный признак различает chosen и rejected, почему примеры покрывают этот признак. Отчёт пишется по-русски. Не делай отдельный цикл производства отчётов вместо генерации.
3. Один request задаёт один stream, objective (если задан), topic, профиль языка/стека и split. Не добавляй соседние темы. Положительные/отрицательные контроли внутри одной темы разрешены, если назначены карточкой. Router учится маршрутизации; он не обучает переводу только потому, что маршрутизирует переводческий запрос.
4. Создай ровно `count` строк в `<output_directory>/draft.jsonl`. Не начинай обучение и не изменяй production-код. `commands.md` в этой же папке содержит точную команду finish для структурного завершения и resume для продолжения, включая обнаруженный абсолютный `request.paths.python_executable`. Не заменяй её предположением, что `python` доступен в PATH. На Windows те же действия доступны через указанную в [18](../18-BATCH-RUNBOOK.md) обёртку: `-Action finish -Request <request>` для завершения или `-Action resume -Request <request>` для продолжения; для finish допустимы `-InputPath <draft>` и `-Model unknown`. Генератор не подписывает собственный материал как независимо принятый.

## Содержание каждой пары

Используй формат `xiigen.dpo.v1`. JSONL: один объект на строку, без Markdown-ограждения. Каждая строка содержит:

```json
{
  "schema_version": "xiigen.dpo.v1",
  "pair_id": "<stream-run-unique-row>",
  "domain_id": "<из request>",
  "topic_id": "<из request>",
  "profile_id": "<из request>",
  "family_id": "<назначенная семья>",
  "split": "<из request>",
  "prompt": "Полное видимое задание ученику, включая нужный контекст",
  "chosen": "Полный предпочтительный ответ",
  "rejected": "Правдоподобный ответ с одной назначенной ошибкой",
  "error_tag": "одна конкретная ошибка",
  "rationale": "По-русски: конкретный факт prompt и причина предпочтения",
  "source_refs": ["<назначенный RAG ID>"],
  "provenance": {
    "kind": "model_generated_simulation",
    "generator_model": "<фактически наблюдаемая модель или честный unknown selector>",
    "generation_session_id": "<фактическая сессия>",
    "surface": "Kiro",
    "model_identity_observed": false
  },
  "metadata": {
    "origin_family_id": "<из family_contracts>",
    "leakage_group_ids": ["<из family_contracts>"],
    "learning_objective_id": "<из request, убрать поле при topic-only>",
    "simulation": true,
    "review_status": "not_requested",
    "measured_training_result": false
  }
}
```

Это шаблон полей с заполнителями, не готовая строка. Машинная схема и конкретный request включены ниже; все topic/profile/family значения берутся из request, а не из примера. `model_identity_observed` становится `true` только при фактическом наблюдении. Не присваивай `DeterministicGenerator` или `OpenTeacher` модельным ответам ради будущего native admission. Отмена подготовки checksum не меняет контракт нативного trainer.

Prompt содержит всё, что ученику нужно для выбора: исходный текст, роли/контекст, версию framework или SQL dialect, необходимые входные данные и ограничение ответа. Метки класса, правильный ответ, rationale, ID, источники и будущие результаты не подкладываются как признаки ученику. Исключение — сведения, которые составляют само задание, например явно предоставленный каталог маршрутов.

Chosen и rejected решают одно задание и выглядят одинаково естественно. Нельзя делать rejected нелепым, бессмысленным или заметно короче только ради отличия; нельзя добавлять «неверно», «это плохой ответ», markdown-маркеры правильности или объяснение ошибки. Если обе стороны корректны и отличаются лишь стилем или допустимым синонимом, пары нет: переделай контраст в рамках назначенной темы. Если источник не подтверждает факт или версия API неизвестна, не выдумывай её — сохрани неготовый случай отдельно в `quarantine.jsonl` этой попытки с причиной и укажи недостающий материал. Quarantine не считается созданной парой и не входит в count. При отсутствии source-файла назови конкретный ID/path и используй назначенную карту происхождения; не создавай пустой manifest, фальшивую RAG-выдержку или текст под ожидаемый hash.

## Семьи и независимость наборов

Копируй `origin_family_id` и `leakage_group_ids` из `family_contracts`. Варианты одного исходного сюжета, его переводы, обратные переводы, опечатки и неправильная раскладка остаются в одной семье и одном split. Родственные морфологические формы могут разделять лексическую семью, но их роль/род/число/смысл сохраняются отдельно. Похожее самостоятельное слово нельзя превращать в опечатку другого слова только по расстоянию редактирования.

Если для семьи назначены два класса, создай по одной строке каждого класса с общими family/origin/leakage. Меняй только указанный контекстный фактор; метка `family_class_field` остаётся в metadata. Не заменяй требуемый контроль второй положительной строкой. Общие квоты routing-классов из request обязательны.

Reference-примеры карточек показывают качество постановки. Не переписывай их под новыми ID и не считай сгенерированными уникальными данными. Их производные не становятся скрытыми DEV/EXAM. Для DEV/EXAM используй новые семьи и отдельную сессию без сгенерированного TRAIN, его ответов или отзывов. Формальные ID не доказывают смысловую независимость: сохраняй известное родство сейчас; последующая проверка скрытых парафразов назначается отдельно и не запускается этой сессией.

## Качество по предмету

- Перевод: все сведения о роде, числе, времени, роли, отрицании и референтах должны следовать из видимого prompt. Нельзя приписывать пол по стереотипу профессии. Проверить направление языка отдельно. Для неверной раскладки сверить конкретную физическую карту; для опечатки — точное изменение code points. Иврит хранится в обычном логическом порядке Unicode, без ручного разворота букв.
- Оркестрация: выбранное действие должно следовать из цели пользователя и текущего состояния, учитывать зависимости и настоящий результат предыдущего шага. Слова «код» или «документация» сами по себе не определяют маршрут; учитывай действие «напиши», «объясни», «спланируй» и т. п. Следующее действие и ожидание не заменяются фиктивным успехом.
- Документация: объясняется фактический код/контракт указанной версии; желаемое исправление не выдаётся за уже существующее поведение.
- Написание кода: верный ответ удовлетворяет видимому контракту. Будущая отдельно порученная приёмка поведения использует небольшой исполняемый oracle на этих примерах; компиляция сама по себе недостаточна. Сейчас ни oracle, ни глобальные production-тесты не запускаются: создаётся кандидат по полному видимому требованию.

## Завершение создания и переход

После сохранения ровно `count` строк выполни готовую команду `finish` из `commands.md`, сохранив абсолютные пути интерпретатора/request/draft и заменяя `unknown` только наблюдаемым именем модели. Отдельный `check` не нужен: finish проверяет JSON/контракт/количество и структурные связи; он не оценивает смысл независимо, не вызывает provider и не обучает. При ошибке исправляй только создаваемый файл с сохранением прежней версии. Не создавай новый run ради обхода структурной ошибки и не выдавай неполный результат за завершённый.

Успешный finish сохраняет `state.json` и `report.creation.md` (новая версия вместо затирания имеющегося отчёта). `DRAFT_CREATED` означает созданных кандидатов. Сразу переходи к следующему stream из registry; `review_status=not_requested`, независимая смысловая приёмка не выполнена, Student `NOT_RUN`. **Запрещено автоматически создавать blind/review, вызывать отдельного reviewer, делать accept/export или ждать их перед следующим набором.** Сохранённые описания этих этапов используются только по отдельному последующему заданию.

Короткое сообщение по-русски: «Создано N из N кандидатных пар по теме …; файлы …; структурная проверка завершена; следующий …». Если число неполное, назови фактическое число и точную причину. Не добавляй вымышленные оценки смысла, скорость Student, стоимость или обучение. Имя модели/session остаётся unknown, если оно не наблюдалось.

## Контекст и ожидание

Не пытайся вместить большой корпус в один ответ. Размер текущей партии уже ограничен; при нехватке контекста сохрани фактический неполный результат и причину, не обрезай строки и не заявляй ровный count. Следующая партия назначается отдельным request с `next_start`; существующий run не удаляется и не перезапускается скрыто.

При долгой операции сначала найди документацию фактически доступного инструмента и его wait/resume. Запусти один раз, сохрани один handle и жди его terminal state штатным блокирующим/event механизмом; `running`, истечение yield или тихий stdout не означают отказ. Не заканчивай ход словами «жду», не запускай второй процесс или фоновый цикл частого опроса. При явном «стоп» прекрати работу. При «следующий» немедленно перейди к названному ID и не дописывай прошлую задачу. Если ID не назван, очередь берётся из `execution-queue.json`; точный `next_stream_id` также возвращают finish/resume. Дополнительный поиск runner или модели для перехода не нужен.

Подготовка/JSON/oracle не являются измерением обучения. Если позже пользователь разрешит учебный опыт, требуется настоящий обучающий механизм, совместимое сохранённое ненулевое состояние, независимое до/после и достаточный бюджет. `0→0` не подтверждает обучение. Любое измеренное улучшение полезного показателя при валидном опыте — успех по нему и домену; остальные показатели, включая ухудшения, показываются рядом. Эти требования не дают разрешения запускать обучение сейчас.


## Назначение

```json
{
  "schema_version": "xiigen.batch-request.v1",
  "stream_id": "OR-QA-CAUSAL/base",
  "topic_id": "OR-QA-01",
  "objective_id": "OR-QA-CAUSAL",
  "domain_id": "or.qa",
  "profile_id": "base",
  "run_id": "T443",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/OR-QA-CAUSAL/base/F0000013",
    "v3/OR-QA-CAUSAL/base/F0000014",
    "v3/OR-QA-CAUSAL/base/F0000015",
    "v3/OR-QA-CAUSAL/base/F0000019",
    "v3/OR-QA-CAUSAL/base/F0000020",
    "v3/OR-QA-CAUSAL/base/F0000021",
    "v3/OR-QA-CAUSAL/base/F0000022",
    "v3/OR-QA-CAUSAL/base/F0000024",
    "v3/OR-QA-CAUSAL/base/F0000026",
    "v3/OR-QA-CAUSAL/base/F0000027"
  ],
  "family_contracts": {
    "v3/OR-QA-CAUSAL/base/F0000013": {
      "origin_family_id": "v3/OR-QA-CAUSAL/base/F0000013",
      "leakage_group_ids": [
        "v3/OR-QA-CAUSAL/base/F0000013"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-CAUSAL/base/F0000014": {
      "origin_family_id": "v3/OR-QA-CAUSAL/base/F0000014",
      "leakage_group_ids": [
        "v3/OR-QA-CAUSAL/base/F0000014"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-CAUSAL/base/F0000015": {
      "origin_family_id": "v3/OR-QA-CAUSAL/base/F0000015",
      "leakage_group_ids": [
        "v3/OR-QA-CAUSAL/base/F0000015"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-CAUSAL/base/F0000019": {
      "origin_family_id": "v3/OR-QA-CAUSAL/base/F0000019",
      "leakage_group_ids": [
        "v3/OR-QA-CAUSAL/base/F0000019"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-CAUSAL/base/F0000020": {
      "origin_family_id": "v3/OR-QA-CAUSAL/base/F0000020",
      "leakage_group_ids": [
        "v3/OR-QA-CAUSAL/base/F0000020"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-CAUSAL/base/F0000021": {
      "origin_family_id": "v3/OR-QA-CAUSAL/base/F0000021",
      "leakage_group_ids": [
        "v3/OR-QA-CAUSAL/base/F0000021"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-CAUSAL/base/F0000022": {
      "origin_family_id": "v3/OR-QA-CAUSAL/base/F0000022",
      "leakage_group_ids": [
        "v3/OR-QA-CAUSAL/base/F0000022"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-CAUSAL/base/F0000024": {
      "origin_family_id": "v3/OR-QA-CAUSAL/base/F0000024",
      "leakage_group_ids": [
        "v3/OR-QA-CAUSAL/base/F0000024"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-CAUSAL/base/F0000026": {
      "origin_family_id": "v3/OR-QA-CAUSAL/base/F0000026",
      "leakage_group_ids": [
        "v3/OR-QA-CAUSAL/base/F0000026"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-CAUSAL/base/F0000027": {
      "origin_family_id": "v3/OR-QA-CAUSAL/base/F0000027",
      "leakage_group_ids": [
        "v3/OR-QA-CAUSAL/base/F0000027"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 28,
  "sources": [
    "RAG-002",
    "RAG-032",
    "RAG-046"
  ],
  "card": "curriculum/cards/OR-QA-CAUSAL.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-QA-CAUSAL\\base\\T443",
  "mode": "CREATE_ONLY",
  "next_stream_id": "TR01-NEGATION-SCOPE/ru-en",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\OR-QA-CAUSAL.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-QA-CAUSAL\\base\\T443\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-QA-CAUSAL\\base\\T443\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-QA-CAUSAL\\base\\T443\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-QA-CAUSAL\\base\\T443\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-QA-CAUSAL\\base\\T443\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-QA-CAUSAL\\base\\T443\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-002": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-002.md",
      "RAG-032": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-032.md",
      "RAG-046": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-046.md"
    }
  },
  "model_policy": "Current user-selected Chinese model in Kiro; no override; record observed identity honestly.",
  "role_contract": {
    "selection": {
      "actor": "CODEX",
      "requested_model": "astra 6 ultracode",
      "status": "NOT_RUN_IN_THIS_CREATION_STAGE"
    },
    "generation": {
      "actor": "external_open_chinese_model_via_kiro",
      "model_policy": "Use the selected Chinese model; inherit the current configuration without override. Record observed identity or unknown."
    },
    "quality_assessment": {
      "actor": "CODEX",
      "requested_model": "astra 6 ultracode",
      "status": "NOT_REQUESTED_IN_CREATE_ONLY"
    },
    "creation_blocked_by_review": false,
    "identity_scope": "Requested roles, not evidence of actual model execution or completed selection/review."
  },
  "scope": "DATA_PREPARATION_ONLY",
  "training_status": "NOT_RUN"
}
```

На каждую family_id создай ровно одну строку.

## Предметное содержание карточки (без исторических команд)

{
  "objective_id": "OR-QA-CAUSAL",
  "topic_id": "OR-QA-01",
  "title_ru": "Один совместимый parent для сравнения учебного изменения",
  "learning_goal_ru": "Выбирать протокол A/B, который восстанавливает одинаковое полное обученное состояние до изменения единственного учебного фактора.",
  "generation_prompt_ru": "Сгенерируй только случаи выбора общего совместимого parent перед A/B учебного изменения. В prompt перечисли компоненты используемого состояния: weights, tokenizer, adapter, optimizer, RNG, память/индекс, если их потребляет learner. Назови исследуемый фактор. Два протокола различаются одним дефектом восстановления; не смешивай это со split или критерием успеха. Случайно переставляй метки A/B: правильный протокол не всегда A. Все числа условные. Работай только с этим objective_id и его темой. Ситуации синтетические; полные условия помещай в prompt. Изменяй причинные условия, а не только названия объектов. Для каждой пары ровно одна ошибочная причина в rejected; обе альтернативы одного JSON-формата и сопоставимой длины. Не добавляй пояснения или метки правильности в chosen/rejected. Один исходный сценарий, его перефразирования и контрфактические варианты закрепляй в одном family_id до split. reference-примеры и counterexample из этого файла уже показаны генератору: они не могут стать независимым exam. Следуй полям request.json, ID семей и лимиту count; не копируй эталоны в новый train. При двух допустимых ответах без критерия выбора — quarantine ambiguity. Отчёт по-русски; фактическую модель Kiro запиши без догадок. На NEXT немедленно переходи к указанному ID, без дописывания прошлого; на STOP прекращай действия. Не удаляй прошлые опыты и не меняй production. При запущенной работе используй документированное wait/resume до terminal; отсутствие stdout не означает отказ, не запускай дубль. Не выдавай структурную проверку данных за измерение обучения.",
  "response_contract_ru": "JSON {\"protocol\": имя одного протокола из prompt}; не утверждать результаты будущего запуска.",
  "scenario_axes": [
    {
      "axis": "parent_restore",
      "values": [
        "полный совместимый снимок",
        "только веса",
        "новая инициализация",
        "снимок с другим tokenizer"
      ]
    },
    {
      "axis": "changed_factor",
      "values": [
        "способ выбора negatives",
        "один update DPO",
        "одна kNN-функция"
      ]
    },
    {
      "axis": "stochastic_state",
      "values": [
        "ГСЧ сохранён и восстановлен",
        "другая случайная последовательность"
      ]
    }
  ],
  "forbidden_mixing": [
    "В этой партии только выбор общего совместимого parent для A/B; split leakage, скорость и критерии успеха вынести в свои objective.",
    "Не включать ответы предметного переводчика или кода; learner учится выбирать протокол восстановления.",
    "Не использовать 0→0 или выдуманный runtime как свидетельство результатов."
  ],
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "OR-QA-CAUSAL-REF-01",
      "profile_id": "base",
      "family_id": "OR-QA-CAUSAL-RF01",
      "prompt": "Сравниваются два способа выбора negatives. Совместимый обученный snapshot S даёт 6/10 правильных ответов на калибровке. Требуется изолировать только выбор negatives. A: оба плеча клонируют полный S. B: первое плечо клонирует S, второе заново инициализируется. Данные, EXAM и бюджет одинаковы. Выбери protocol A или B.",
      "chosen": "{\"protocol\":\"A\"}",
      "rejected": "{\"protocol\":\"B\"}",
      "error_tag": "different_parent_confound",
      "rationale": "B одновременно меняет negatives и начальное состояние; один фактор нельзя отделить от другого. Числа условные.",
      "counterexample": "Если изучается именно сравнение способов инициализации, различные начальные состояния допустимы, но это другой заранее заданный objective.",
      "source_refs": [
        "RAG-002",
        "RAG-032",
        "RAG-046"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false
      }
    },
    {
      "example_id": "OR-QA-CAUSAL-REF-02",
      "profile_id": "base",
      "family_id": "OR-QA-CAUSAL-RF02",
      "prompt": "Обучающий consumer использует weights и optimizer moments; momentum ненулевой и влияет на следующий update. Snapshot S сохраняет оба. A: второе плечо восстанавливает только weights и обнуляет moments. B: оба плеча восстанавливают weights+moments из S. Исследуемый фактор — состав одной DPO-пары; данные/EXAM/бюджет совпадают. Выбери protocol.",
      "chosen": "{\"protocol\":\"B\"}",
      "rejected": "{\"protocol\":\"A\"}",
      "error_tag": "optimizer_reset_changes_parent",
      "rationale": "Обнуление реально используемых moments меняет следующий учебный шаг независимо от пары; B сохраняет общий parent.",
      "counterexample": "Если optimizer прямо задан stateless и moments не читаются, их отсутствие не является дефектом и требовать фиктивные moments нельзя.",
      "source_refs": [
        "RAG-002",
        "RAG-032",
        "RAG-046"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false
      }
    },
    {
      "example_id": "OR-QA-CAUSAL-REF-03",
      "profile_id": "base",
      "family_id": "OR-QA-CAUSAL-RF03",
      "prompt": "Исследуется один новый вариант kNN update. Consumer читает обученную память M и случайно выбирает tie-break через RNG. S сохраняет M и RNG; усвоение уже ненулевое. A: оба плеча восстанавливают M+RNG из S. B: память совпадает, но второе плечо берёт новый RNG seed. Прочие условия равны. Выбери protocol.",
      "chosen": "{\"protocol\":\"A\"}",
      "rejected": "{\"protocol\":\"B\"}",
      "error_tag": "rng_state_not_restored",
      "rationale": "Другой RNG может изменить выбор при равенстве независимо от исследуемого kNN update; A изолирует фактор.",
      "counterexample": "Если consumer детерминирован и RNG вообще не используется, отличие нечитанного RNG не влияет на parent и не должно блокировать протокол.",
      "source_refs": [
        "RAG-002",
        "RAG-032",
        "RAG-046"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false
      }
    },
    {
      "example_id": "OR-QA-CAUSAL-REF-04",
      "profile_id": "base",
      "family_id": "OR-QA-CAUSAL-RF04",
      "prompt": "Snapshot S обучен с tokenizer T1, vocab_sha=v1 и adapter A1; он имеет ненулевые ответы. Consumer интерпретирует weights через token IDs. A: оба плеча загружают weights S, но второе использует T2 с иными token IDs. B: оба восстанавливают S+T1+A1 и проверяют v1. Исследуемый фактор — новый DPO update, не tokenizer. Выбери protocol.",
      "chosen": "{\"protocol\":\"B\"}",
      "rejected": "{\"protocol\":\"A\"}",
      "error_tag": "tokenizer_parent_incompatible",
      "rationale": "Одинаковые числовые weights с другой интерпретацией token IDs не являются тем же обученным parent.",
      "counterexample": "Если T2 доказанно побитово эквивалентен T1 по mapping и конфигурации, смена имени файла не меняет parent; сравнивать нужно содержимое.",
      "source_refs": [
        "RAG-002",
        "RAG-032",
        "RAG-046"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false
      }
    }
  ]
}

## Полный контракт строки: не искать другую схему

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "title": "XIIGEN candidate DPO row, creation stage only",
  "description": "One JSON object per JSONL line. Structural conformance does not establish semantic quality, native admission or learning. Request-specific family/topic constraints are checked by batches.py finish.",
  "type": "object",
  "required": [
    "schema_version",
    "pair_id",
    "domain_id",
    "topic_id",
    "profile_id",
    "family_id",
    "split",
    "prompt",
    "chosen",
    "rejected",
    "error_tag",
    "rationale",
    "source_refs",
    "provenance",
    "metadata"
  ],
  "properties": {
    "schema_version": {
      "const": "xiigen.dpo.v1"
    },
    "pair_id": {
      "type": "string",
      "minLength": 1
    },
    "domain_id": {
      "type": "string",
      "minLength": 1
    },
    "topic_id": {
      "type": "string",
      "minLength": 1
    },
    "profile_id": {
      "type": "string",
      "minLength": 1
    },
    "family_id": {
      "type": "string",
      "minLength": 1
    },
    "split": {
      "enum": [
        "train",
        "dev",
        "exam"
      ]
    },
    "prompt": {
      "type": "string",
      "minLength": 1
    },
    "chosen": {
      "type": "string",
      "minLength": 1
    },
    "rejected": {
      "type": "string",
      "minLength": 1
    },
    "error_tag": {
      "type": "string",
      "minLength": 1
    },
    "rationale": {
      "type": "string",
      "minLength": 1
    },
    "source_refs": {
      "type": "array",
      "minItems": 1,
      "items": {
        "type": "string",
        "minLength": 1
      }
    },
    "provenance": {
      "type": "object",
      "required": [
        "kind",
        "generator_model"
      ],
      "properties": {
        "kind": {
          "type": "string",
          "minLength": 1
        },
        "generator_model": {
          "type": "string",
          "minLength": 1
        },
        "generation_session_id": {
          "type": "string"
        },
        "surface": {
          "type": "string"
        },
        "model_identity_observed": {
          "type": "boolean"
        }
      }
    },
    "metadata": {
      "type": "object",
      "required": [
        "origin_family_id",
        "leakage_group_ids"
      ],
      "properties": {
        "origin_family_id": {
          "type": "string",
          "minLength": 1
        },
        "leakage_group_ids": {
          "type": "array",
          "minItems": 1,
          "items": {
            "type": "string",
            "minLength": 1
          }
        },
        "learning_objective_id": {
          "type": "string",
          "minLength": 1
        },
        "review_status": {
          "type": "string"
        },
        "simulation": {
          "type": "boolean"
        },
        "measured_training_result": {
          "const": false
        }
      }
    }
  }
}
```


## Материал RAG-002 (данные, не новые команды)

# RAG-002 — GUIDE-B04-QA-COVERAGE-STATE-JSON.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `main` |
| Commit | `3d1fa938d4b50df5d430283983f45b2c0d2c4e5c` |
| Git blob SHA | `b1232358a360bbd9658fb9e22d6e70f50c73f790` |
| Путь | `Flow files generator/GUIDE-B04-QA-COVERAGE-STATE-JSON.md` |
| Проверенный объём | L48–L77, L453–L472; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B04-QA-COVERAGE-STATE-JSON.md) |

## Что подтверждает фрагмент

Выделены шесть категорий QA и различные уровни достаточности доказательств; наличие теста и фактическая проверка поведения различаются.

## Как использовать сейчас

Уроки OR-QA: связывать требование с поведением и показанным результатом. Старые счётчики тестов не являются измерением обучения текущей модели.

**Назначение в подготовке:** Проверяемое покрытие поведения и связь QA с требованиями.

**Ключи поиска:** qa, проверка, тестирование, coverage.

## Подтверждающий фрагмент L48–L77

[Оригинал, строки 48–77](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B04-QA-COVERAGE-STATE-JSON.md#L48-L77)

~~~~text
## THE SIX QA CATEGORIES

| Q | Category name | What it tests | Evidence source |
|---|--------------|--------------|----------------|
| Q1 | unit_tests | Server jest + e2e test counts vs baseline | `npx jest` run results |
| Q2 | client_ui | UI-REFLECTION verdicts per task type (FULL_UI / PARTIAL_UI / NO_UI / INTERNAL_ONLY) | `UI-REFLECTION-STATE.json` (B-09) |
| Q3 | design_simulation | Design contract spec presence (design contract item 01..design contract item 10); the preference learning signal triple check | `ls` of topology + design contract files |
| Q4 | marketplace_ui | Marketplace package applicability; ironRules + arbiterConfigIds population | the search index query or fixture file inspection |
| Q5 | cross_tenant_install | Cross-tenant install path validation; scope-portability tests | Test file existence + provisioning test run |
| Q6 | bfa_validation | CF rule presence, cross-flow validator suite result | `grep` for CF rules + the business flow arbiter suite run |

**Verdict values per Q-category:**
| Verdict | Meaning |
|---------|---------|
| `"PASS"` | All criteria met, evidence is positive |
| `"PASS_WITH_CAVEATS"` | Criteria met but with acknowledged limitations (e.g., weakened assertions) |
| `"PARTIAL_GAP"` | Some criteria met, specific gaps identified |
| `"TBD"` | Not yet evaluated — insufficient evidence to set a verdict |
| `"FAIL"` | Critical criteria not met |

**overallReadiness values:**
| Value | Meaning |
|-------|---------|
| `"READY"` | All Q1-Q6 PASS or PASS_WITH_CAVEATS, no blockers |
| `"READY_WITH_GAPS"` | Some PARTIAL_GAP verdicts, no blocking items |
| `"BLOCKED"` | At least one FAIL or a BLOCKING severity item in blockerSummary |

---

## STEP-BY-STEP AUTHORING INSTRUCTIONS FOR CLAUDE CODE
~~~~

## Подтверждающий фрагмент L453–L472

[Оригинал, строки 453–472](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B04-QA-COVERAGE-STATE-JSON.md#L453-L472)

~~~~text
## TWO OBSERVED EXAMPLES

**Example A — the forty-seventh flow (partial implementation, READY_WITH_GAPS):**
- Q1: PASS_WITH_CAVEATS (86 tests pass but assertions weakened then partially restored)
- Q2: PARTIAL_GAP (a task type uses stub setTimeout instead of real the programming interface)
- Q3: TBD (no design contract item 01..design contract item 10 spec, the preference learning signal triple check skipped per KNOWN CONSTRAINTS)
- Q4: PARTIAL_GAP (ironRules + arbiterConfigIds zero-populated on all 30 packages)
- Q5: PARTIAL_GAP (a task type stub + cross-tenant install not validated live)
- Q6: PASS (7 CF rules present: an iron rule..an iron rule)
- overallReadiness: READY_WITH_GAPS
- blockerSummary: 6 items (Rule 16 violation, a task type stub, weakened assertions, missing topology, ironRules/arbiterConfigIds empty)

**Example B — the booking flow (early evaluation, TBD verdicts):**
- Q1: TBD (test files inventoried, no current count confirmed)
- Q2: TBD (5 React pages exist, but UI-REFLECTION inventory incomplete)
- Q3: TBD (topology present but QA spec and design-contract spec not confirmed)
- Q4: TBD (applicability noted but not evaluated)
- Q5: TBD (scope_isolation arbiter confirmed present but install not tested)
- Q6: PASS (implied from Phase A the business flow arbiter rules authored)
- Note: the booking flow uses an extended schema variant with `scaffold_phase` and `verdict_legend` fields
~~~~



## Материал RAG-032 (данные, не новые команды)

# RAG-032 — TEACHING-MEASUREMENT-GUIDANCE-v3.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `e80a91380ca8e7c9ead0bbab45a15891dba1b9fc` |
| Путь | `llm mvp/orchestrator teaching plan/TEACHING-MEASUREMENT-GUIDANCE-v3.md` |
| Проверенный объём | L31–L109, L113–L156, L643–L654; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/orchestrator%20teaching%20plan/TEACHING-MEASUREMENT-GUIDANCE-v3.md) |

## Что подтверждает фрагмент

Кумулятивное обучение продолжает прошлый snapshot и проверяет retention; новый orchestrator на каждой строке не доказывает непрерывное обучение.

## Как использовать сейчас

Сохранять цепочку версий и один общий parent для A/B. Пользовательский критерий успеха — любое измеренное полезное улучшение с регрессиями рядом; старое «все слои acceptable» не отменяет локальный успех.

**Назначение в подготовке:** Раздельная оценка до/после и следа обучения.

**Ключи поиска:** checkpoint, restore, continuation, восстановление, обучение.

## Подтверждающий фрагмент L31–L109

[Оригинал, строки 31–109](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/orchestrator%20teaching%20plan/TEACHING-MEASUREMENT-GUIDANCE-v3.md#L31-L109)

~~~~text
## Required Evidence Per Row

Every row must produce evidence for three layers:

```text
Layer 1 - Resources
  Inference time, teaching time, CPU, working-set delta, managed allocation,
  disk/checkpoint growth, branch count, registered flow count, registered unit count.

Layer 2 - Routing
  Compiler predicates, routed sub-phases, confidence values, branch count,
  and whether each branch is justified by the real human prompt.

Layer 3 - Verdict
  Pre-teaching verdict, teaching result, post-teaching verification verdict,
  knowledge-edge count before and after, and whether cumulative registry state improved.
```

A row succeeds only if all three layers are acceptable.

---

## Mandatory Cumulative Teaching Rule

The teaching runner must not create a fresh orchestrator per row.

Correct cycle:

```text
Start bundle
  create topology registry once
  create flow registry once
  create unit registry once
  create capability trainer once
  create adaptive orchestrator once
  save active teaching snapshot

For row 1
  load active teaching snapshot
  compile prompt
  route to sub-phases
  teach every routed branch
  register learned unit/flow into the same registries
  verify row 1 against the same registries
  save active teaching snapshot
  write row evidence

For row 2
  load active teaching snapshot from row 1
  compile prompt
  route to sub-phases
  teach every routed branch, including sub-phases already seen before
  register newer learned unit versions
  verify row 2 against cumulative registry state
  re-test row 1 against the grown registry
  save active teaching snapshot
  write row evidence

For row N
  load active teaching snapshot from row N-1
  teach only row N
  verify row N
  re-test the newest 2-3 previously accepted rows
  accept row N only if the new row passes and the regression window still passes
  save active teaching snapshot
```

This deliberately avoids an "N rows in one long push" teaching run. The machine must show progress after one row at a time, because the value being measured is whether the logical unit grows without forgetting recent accepted behavior.

Wrong cycle:

```text
For each row
  create a fresh orchestrator
  train missing flows
  retry same row
```

The wrong cycle proves only local self-repair. It does not prove continuous learning, because row 2 cannot benefit from row 1 and row 3 cannot benefit from rows 1 and 2.
~~~~

## Подтверждающий фрагмент L113–L156

[Оригинал, строки 113–156](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/orchestrator%20teaching%20plan/TEACHING-MEASUREMENT-GUIDANCE-v3.md#L113-L156)

~~~~text
## Already-Known Sub-Phase Rule

If row N routes to a sub-phase that already has a learned flow from an earlier row, the row must still create a new teaching event for that sub-phase.

Reason:

```text
Existing capability means the machine has a prior belief.
The new row is new evidence.
Teaching must update that belief, not skip it.
```

Correct behavior:

```text
row 1 routes to 4.1 -> create 4.1-runtime-unit v1
row 2 routes to 4.1 -> load v1 checkpoint, continue training, save v2
ResolveLatest(4.1) now selects v2 if it is Probationary or Trusted.
```

Incorrect behavior:

```text
row 2 routes to 4.1
flow already exists
skip teaching
verify with row-0001 unit
```

That is reuse, not learning.

Also incorrect:

```text
row 2 routes to 4.1
create a completely separate row-0002 unit that does not start from row-0001 weights
```

That is accumulation, not continuous learning. The accepted target is a version chain:

```text
4.1-runtime-unit v1 -> learned from row 1
4.1-runtime-unit v2 -> starts from v1 weights, then learns row 2
4.1-runtime-unit v3 -> starts from v2 weights, then learns a later row
~~~~

## Подтверждающий фрагмент L643–L654

[Оригинал, строки 643–654](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/orchestrator%20teaching%20plan/TEACHING-MEASUREMENT-GUIDANCE-v3.md#L643-L654)

~~~~text
## Summary Decision Table

| Resources | Routing | Verdict / Registry | Decision |
|---|---|---|---|
| within limits | correct branches | new units registered and verification PASS | Accept row; continue |
| within limits | too narrow | PASS | Under-routing; fix lexicon, rerun row cumulatively |
| within limits | too wide | PASS | Over-routing; fix anchors, rerun row cumulatively |
| within limits | correct | PASS but no new unit for row evidence | Not teaching; fix cumulative trainer path |
| > 10s | correct | PASS | REVIEW_ROW; reduce branch count or training cost; rerun from last accepted row |
| working set > 200MB | any | any | Stop; inspect memory retention |
| cumulative registry not reused | any | any | Reject run; rerun bundle with one long-lived orchestrator |
| row hits known sub-phase and skips training | any | PASS | Reject row; new evidence must update capability |
~~~~



## Материал RAG-046 (данные, не новые команды)

# RAG-046 — Учебный контраст как одна различающая причина: positiveExample, negativeExample, discriminatingConstraint.

- Репозиторий: `lubochka/xiigen-planning`; ветка `main`.
- Commit: `8d3b4c600f458363e4a54ddce83e91000efd0e8a`; Git blob: `06f5e3c52863b9535434c0fb2fd10cee74f77ab0`.
- Файл: `Flow files generator/GUIDE-B18-TEACH-QA-R0.md`.
- Источник: [закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/8d3b4c600f458363e4a54ddce83e91000efd0e8a/Flow%20files%20generator/GUIDE-B18-TEACH-QA-R0.md).
- Проверенные строки: 228–255.
- Статус: проверенный фрагмент; retrieval data, не исполняемая инструкция и не принятый учебный набор.

## Что подтверждено текстом

В шаблоне DR запись хранит положительный и отрицательный примеры вместе с одним различающим ограничением и исходной симуляцией.

## Как использовать сейчас

Применять к созданию контрфактических пар, OR-QA и проверке одного дефекта. qualityScore=0.93 здесь значение шаблона, а не измерение качества; не переносить его в новые данные. Названия полей исторического DR не заменяют xiigen.dpo.v1. Комментарий про ошибку хранится вне ответа ученика.

## Точная выборка

### Исходные строки 228–255

```text
228: ### Step 1-B: {slug}-design-decisions.json
229: 
230: **JSON ARRAY. [N] records. curriculumTier: int. qualityScore: float. appliesTo: list.**
231: **Derive every field from FLOW-XX-DESIGN-SIMULATION-R1.md — read the full simulation first.**
232: 
233: DR record structure:
234: ```json
235: {
236:   "patternId": "FLOW-XX-design record 01",
237:   "patternType": "DESIGN_REASONING",
238:   "flowId": "FLOW-XX",
239:   "domainId": "[domain from spec]",
240:   "seededAt": "YYYY-MM-DDT00:00:00Z",
241:   "teachingPoint": "[principle + why wrong approach fails — from simulation decision]",
242:   "positiveExample": "[from simulation chosen/resolution]",
243:   "negativeExample": "[from simulation rejected/proposed]",
244:   "discriminatingConstraint": "[one constraint separating chosen from rejected]",
245:   "appliesTo": ["FLOW-NN", "FLOW-NN"],
246:   "curriculumTier": 1,
247:   "qualityScore": 0.93,
248:   "sourceDocument": "FLOW-XX-DESIGN-SIMULATION-R1.md (DR-XX-A)",
249:   "tags": ["[tag1]", "[tag2]"],
250:   "keywords": "[space-separated retrieval terms]"
251: }
252: ```
253: 
254: DR count: Minimum 4 records. Capture every decision that would cause SILENT_FAILURE
255: if the AI model applies the wrong approach.
```


## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\OR-QA-CAUSAL\base\T443\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\OR-QA-CAUSAL\base\T443\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\OR-QA-CAUSAL\base\T443\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `TR01-NEGATION-SCOPE/ru-en`. Не запускать review, accept, export, oracle или обучение.
