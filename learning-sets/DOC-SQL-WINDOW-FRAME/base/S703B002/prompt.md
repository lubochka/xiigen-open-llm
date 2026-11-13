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
  "stream_id": "DOC-SQL-WINDOW-FRAME/base",
  "topic_id": "DOC-SQL-04",
  "objective_id": "DOC-SQL-WINDOW-FRAME",
  "domain_id": "docs.sql",
  "profile_id": "base",
  "run_id": "S703B002",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000014",
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000015",
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000016",
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000017",
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000018",
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000019",
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000021",
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000022",
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000023",
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000024"
  ],
  "family_contracts": {
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000014": {
      "origin_family_id": "v3/DOC-SQL-WINDOW-FRAME/base/F0000014",
      "leakage_group_ids": [
        "v3/DOC-SQL-WINDOW-FRAME/base/F0000014"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000015": {
      "origin_family_id": "v3/DOC-SQL-WINDOW-FRAME/base/F0000015",
      "leakage_group_ids": [
        "v3/DOC-SQL-WINDOW-FRAME/base/F0000015"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000016": {
      "origin_family_id": "v3/DOC-SQL-WINDOW-FRAME/base/F0000016",
      "leakage_group_ids": [
        "v3/DOC-SQL-WINDOW-FRAME/base/F0000016"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000017": {
      "origin_family_id": "v3/DOC-SQL-WINDOW-FRAME/base/F0000017",
      "leakage_group_ids": [
        "v3/DOC-SQL-WINDOW-FRAME/base/F0000017"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000018": {
      "origin_family_id": "v3/DOC-SQL-WINDOW-FRAME/base/F0000018",
      "leakage_group_ids": [
        "v3/DOC-SQL-WINDOW-FRAME/base/F0000018"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000019": {
      "origin_family_id": "v3/DOC-SQL-WINDOW-FRAME/base/F0000019",
      "leakage_group_ids": [
        "v3/DOC-SQL-WINDOW-FRAME/base/F0000019"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000021": {
      "origin_family_id": "v3/DOC-SQL-WINDOW-FRAME/base/F0000021",
      "leakage_group_ids": [
        "v3/DOC-SQL-WINDOW-FRAME/base/F0000021"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000022": {
      "origin_family_id": "v3/DOC-SQL-WINDOW-FRAME/base/F0000022",
      "leakage_group_ids": [
        "v3/DOC-SQL-WINDOW-FRAME/base/F0000022"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000023": {
      "origin_family_id": "v3/DOC-SQL-WINDOW-FRAME/base/F0000023",
      "leakage_group_ids": [
        "v3/DOC-SQL-WINDOW-FRAME/base/F0000023"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-SQL-WINDOW-FRAME/base/F0000024": {
      "origin_family_id": "v3/DOC-SQL-WINDOW-FRAME/base/F0000024",
      "leakage_group_ids": [
        "v3/DOC-SQL-WINDOW-FRAME/base/F0000024"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 25,
  "sources": [
    "RAG-027",
    "RAG-040"
  ],
  "card": "curriculum/cards/DOC-SQL-WINDOW-FRAME.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-SQL-WINDOW-FRAME\\base\\S703B002",
  "mode": "CREATE_ONLY",
  "next_stream_id": "CODE-SQL-RANK-TIE/base",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\DOC-SQL-WINDOW-FRAME.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-SQL-WINDOW-FRAME\\base\\S703B002\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-SQL-WINDOW-FRAME\\base\\S703B002\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-SQL-WINDOW-FRAME\\base\\S703B002\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-SQL-WINDOW-FRAME\\base\\S703B002\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-SQL-WINDOW-FRAME\\base\\S703B002\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-SQL-WINDOW-FRAME\\base\\S703B002\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-027": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-027.md",
      "RAG-040": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-040.md"
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
  "objective_id": "DOC-SQL-WINDOW-FRAME",
  "topic_id": "DOC-SQL-04",
  "title_ru": "Описать строки, partition и границы оконного агрегата",
  "learning_goal_ru": "Вычислить результат конкретного оконного запроса и отличить исходные строки, вход окна после WHERE/GROUP BY и его рамку.",
  "generation_prompt_ru": "Создавай таблицу из 3–6 строк и один T-SQL SELECT. Все типы, NULL, ключи и данные видимы. Для окна явно задай PARTITION BY, ORDER BY и ROWS/RANGE либо явно отсутствие каждого из них. Финальный ORDER BY задаёт печатный порядок отдельно от порядка внутри окна. Сначала определяй строки после WHERE/GROUP BY, затем окно; не учи правилу «окно всегда сохраняет число строк исходной таблицы», если до него была группировка. Чередуй cumulative, bounded frame, peers RANGE, NULL и window over grouped rows. Одна пара меняет один смысловой вывод, а не диалект.\n\nПРОСТАЯ САМОПРОВЕРКА:\n1. Выпиши входные строки окна после WHERE и GROUP BY. Сосчитай их до вычисления сумм.\n2. Для каждой выходной строки перечисли ID, попавшие в её partition/frame.\n3. ROWS считает позиции; RANGE CURRENT ROW включает peers с одинаковым полным ключом оконного ORDER BY.\n4. Для пустой рамки SUM даёт NULL; COUNT(Amount) не считает NULL.\n5. Выведи столько строк, сколько осталось на входе окна, затем применяй только явный финальный ORDER BY.\n\nРаботай по одному готовому request.json: один topic_id, objective_id, domain_id, profile_id и split. В каждом prompt повтори среду, полный видимый код/данные, точку наблюдения и форму ответа: metadata не попадает в строгую проекцию prompt/chosen/rejected. Создай полный chosen и близкий rejected с одним основным содержательным дефектом, одинакового формата. Не заменяй rejected синтаксической ошибкой, шуткой, заведомо коротким ответом или другой правильной реализацией. Объяснение проверки, source_refs и error_tag записывай вне ответов.\nСначала выпиши маленькую таблицу состояния либо результата по условию, затем сравни с ней обе альтернативы. Это самопроверка автора кандидатов, не независимая оценка качества и не измерение обучения. Если по указанному входу ответы одинаковы, измени различающий вход в пределах задания; не придумывай победу. Смена имени или чисел сама по себе не создаёт независимую семью. Производные, обратные и DOC/CODE-варианты конкретного случая наследуют общий origin/leakage group и split. Публичные reference не являются закрытым EXAM.\nИсточники — только назначенные файлы xiigen-planning main/llm_plan3. Они дают методику и исходную тематику. Новые фрагменты являются авторскими симуляциями; runtime/oracle сейчас NOT_RUN. Не объявляй новую симуляцию извлечённым production-кодом, измеренным результатом или китайской генерацией.\nРоль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. Сейчас CREATE_ONLY: назначение уже дано карточкой/request. Не запускай новые selection/review/oracle/accept/export/Student/production CI и не задерживай следующую партию ради них. Модель — фактически текущая выбранная в Kiro, без override; неизвестное имя/session запиши unknown. Checksum, подписи, profile receipts и независимый review не нужны для создания.\nКорень наборов C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets; подготовка находится в preparation внутри него. Возьми абсолютные входные и выходные пути из request.paths, не ищи runner в llm_mvp_core. После записи реальных JSONL-строк выполни одну готовую команду finish, получи структурный DRAFT_CREATED и перейди к next_stream_id полной execution-queue.json. PREPARED без draft не считается созданием. Старые данные/попытки не удалять; существующий run продолжать через resume.\nПри прямом NEXT/новом ID немедленно перейти к нему: запрещено продолжать предыдущую задачу, её отчёт или исправления. STOP прекращает действия сразу. Живой процесс ждать штатным wait/resume по единственному handle до terminal в том же ходе; не запускать дубликаты, не делать частый polling и не завершать ход словом «жду». Отчёт по-русски: тема, созданные строки/семьи, точный файл, фактическая модель, структурный статус, следующий ID. Нельзя приписывать вызов Astra текущей модели.",
  "response_contract_ru": "Русский ответ с точными строками в заданном порядке и коротким объяснением рамки; не заменять результат советом исправить SQL.",
  "scenario_axes": [
    {
      "axis": "окно",
      "values": [
        "полный partition",
        "накопление ROWS",
        "скользящая рамка",
        "RANGE peers",
        "пустая рамка"
      ]
    },
    {
      "axis": "вход окна",
      "values": [
        "исходные строки",
        "после WHERE",
        "после GROUP BY",
        "nullable values"
      ]
    }
  ],
  "forbidden_mixing": [
    "Другой предмет обучения в той же партии.",
    "Смешение языковых версий, SQL-диалектов либо AngularJS/Angular.",
    "Изменение production и запуск приёмки/обучения вместо создания.",
    "Объявление ожидаемого результата наблюдавшимся."
  ],
  "generation_constraints": {
    "input_context": "Complete in prompt after native projection",
    "source_role": "Methodology/topic evidence; source-bounded authored simulations",
    "version_profile": "T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON",
    "balance": "Чередовать положительные, граничные и противоположные условия; не учить выбору по имени API.",
    "self_check_ru": [
      "Выпиши входные строки окна после WHERE и GROUP BY. Сосчитай их до вычисления сумм.",
      "Для каждой выходной строки перечисли ID, попавшие в её partition/frame.",
      "ROWS считает позиции; RANGE CURRENT ROW включает peers с одинаковым полным ключом оконного ORDER BY.",
      "Для пустой рамки SUM даёт NULL; COUNT(Amount) не считает NULL.",
      "Выведи столько строк, сколько осталось на входе окна, затем применяй только явный финальный ORDER BY."
    ]
  },
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "DOC-SQL-WINDOW-FRAME.REF.01",
      "profile_id": "base",
      "family_id": "sim2.doc-sql-04.f0006",
      "source_refs": [
        "RAG-027",
        "RAG-040"
      ],
      "prompt": "Авторская симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @T TABLE(Id int PRIMARY KEY,Dept char(1),Amount int); INSERT @T VALUES(1,'A',10),(2,'A',5),(3,'B',7); SELECT Id,SUM(Amount) OVER(PARTITION BY Dept) AS Total FROM @T ORDER BY Id; Укажи все пары (Id,Total).",
      "chosen": "(1,15),(2,15),(3,7). Сумма повторяется на каждой строке соответствующего Dept.",
      "rejected": "(1,15),(3,7). Оконная сумма сворачивает каждое Dept до одной строки.",
      "error_tag": "window_collapsed_rows",
      "rationale": "Окно не заменено GROUP BY; потеряна строка Id=2.",
      "counterexample": "На входе окна 3 строки, выходных тоже 3; в Dept=A сумма 10+5.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON",
        "origin_family_id": "sim2.doc-sql-04.f0006",
        "leakage_group_ids": [
          "sim2.doc-sql-04.f0006"
        ],
        "source_role": "methodology_and_topic_seed",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON",
          "expected": {
            "rows": [
              [
                1,
                15
              ],
              [
                2,
                15
              ],
              [
                3,
                7
              ]
            ]
          },
          "assertion": "На входе окна 3 строки, выходных тоже 3; в Dept=A сумма 10+5.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-SQL-04",
        "derived_from_reference_family_ids": [
          "sim2.doc-sql-04.f0006"
        ]
      }
    },
    {
      "example_id": "DOC-SQL-WINDOW-FRAME.REF.02",
      "profile_id": "base",
      "family_id": "sim2.doc-sql-04.f0001",
      "source_refs": [
        "RAG-027",
        "RAG-040"
      ],
      "prompt": "Авторская симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @T TABLE(Id int PRIMARY KEY,Amount int); INSERT @T VALUES(1,10),(2,5),(3,7); SELECT Id,SUM(Amount) OVER(ORDER BY Id ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Total FROM @T ORDER BY Id; Укажи (Id,Total).",
      "chosen": "(1,10),(2,15),(3,22). Рамка заканчивается на текущей строке.",
      "rejected": "(1,22),(2,22),(3,22). Каждая строка получает сумму всей таблицы.",
      "error_tag": "current_row_boundary_ignored",
      "rationale": "В rejected исчезла правая граница CURRENT ROW.",
      "counterexample": "Рамки по Id: [1], [1,2], [1,2,3].",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON",
        "origin_family_id": "sim2.doc-sql-04.f0001",
        "leakage_group_ids": [
          "sim2.doc-sql-04.f0001"
        ],
        "source_role": "methodology_and_topic_seed",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON",
          "expected": {
            "rows": [
              [
                1,
                10
              ],
              [
                2,
                15
              ],
              [
                3,
                22
              ]
            ]
          },
          "assertion": "Рамки по Id: [1], [1,2], [1,2,3].",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-SQL-04",
        "derived_from_reference_family_ids": [
          "sim2.doc-sql-04.f0001"
        ]
      }
    },
    {
      "example_id": "DOC-SQL-WINDOW-FRAME.REF.03",
      "profile_id": "base",
      "family_id": "v7.ref.sql.range-peers",
      "source_refs": [
        "RAG-027",
        "RAG-040"
      ],
      "prompt": "Авторская симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @T TABLE(Id int PRIMARY KEY,Score int,Amount int); INSERT @T VALUES(1,1,10),(2,1,5),(3,2,7); SELECT Id,SUM(Amount) OVER(ORDER BY Score RANGE BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Total FROM @T ORDER BY Id; Укажи (Id,Total).",
      "chosen": "(1,15),(2,15),(3,22). Оба Score=1 — peers, поэтому первая рамка уже включает обе строки.",
      "rejected": "(1,10),(2,15),(3,22). Первая рамка включает только физически первую строку Score=1.",
      "error_tag": "range_treated_as_rows",
      "rationale": "RANGE CURRENT ROW ошибочно трактуется как одна позиция ROWS.",
      "counterexample": "У строк 1 и 2 одинаковый оконный ключ Score, поэтому одинаковые рамка и Total.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON",
        "origin_family_id": "v7.ref.sql.range-peers",
        "leakage_group_ids": [
          "v7.ref.sql.range-peers"
        ],
        "source_role": "methodology_and_topic_seed",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON",
          "expected": {
            "rows": [
              [
                1,
                15
              ],
              [
                2,
                15
              ],
              [
                3,
                22
              ]
            ]
          },
          "assertion": "У строк 1 и 2 одинаковый оконный ключ Score, поэтому одинаковые рамка и Total.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-SQL-04"
      }
    },
    {
      "example_id": "DOC-SQL-WINDOW-FRAME.REF.04",
      "profile_id": "base",
      "family_id": "sim2.doc-sql-04.f0003",
      "source_refs": [
        "RAG-027",
        "RAG-040"
      ],
      "prompt": "Авторская симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @T TABLE(Id int PRIMARY KEY,Amount int); INSERT @T VALUES(1,2),(2,4),(3,8); SELECT Id,SUM(Amount) OVER(ORDER BY Id ROWS BETWEEN 1 PRECEDING AND CURRENT ROW) AS Total FROM @T ORDER BY Id; Укажи (Id,Total).",
      "chosen": "(1,2),(2,6),(3,12). Последняя рамка содержит только строки 2 и 3.",
      "rejected": "(1,2),(2,6),(3,14). Последняя рамка содержит все предшествующие строки.",
      "error_tag": "bounded_frame_made_unbounded",
      "rationale": "1 PRECEDING ошибочно расширено до UNBOUNDED PRECEDING.",
      "counterexample": "Рамки: [1], [1,2], [2,3]; для третьей строки сумма 4+8=12, а не 2+4+8.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON",
        "origin_family_id": "sim2.doc-sql-04.f0003",
        "leakage_group_ids": [
          "sim2.doc-sql-04.f0003"
        ],
        "source_role": "methodology_and_topic_seed",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON",
          "expected": {
            "rows": [
              [
                1,
                2
              ],
              [
                2,
                6
              ],
              [
                3,
                12
              ]
            ]
          },
          "assertion": "Рамки: [1], [1,2], [2,3]; для третьей строки сумма 4+8=12, а не 2+4+8.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-SQL-04",
        "derived_from_reference_family_ids": [
          "sim2.doc-sql-04.f0003"
        ]
      }
    },
    {
      "example_id": "DOC-SQL-WINDOW-FRAME.REF.05",
      "profile_id": "base",
      "family_id": "v7.ref.sql.empty-previous-frame",
      "source_refs": [
        "RAG-027",
        "RAG-040"
      ],
      "prompt": "Авторская симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @T TABLE(Id int PRIMARY KEY,Amount int); INSERT @T VALUES(1,10),(2,20),(3,30); SELECT Id,SUM(Amount) OVER(ORDER BY Id ROWS BETWEEN 1 PRECEDING AND 1 PRECEDING) AS PreviousAmount FROM @T ORDER BY Id; Укажи (Id,PreviousAmount).",
      "chosen": "(1,NULL),(2,10),(3,20). У первой строки рамка пуста; SUM пустой рамки равен NULL.",
      "rejected": "(1,0),(2,10),(3,20). У первой строки SUM пустой рамки автоматически заменяется нулём.",
      "error_tag": "empty_sum_coalesced_without_code",
      "rationale": "В запросе нет COALESCE; NULL подменён нулём.",
      "counterexample": "Первая строка не имеет предшественника; проверь отсутствие COALESCE.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON",
        "origin_family_id": "v7.ref.sql.empty-previous-frame",
        "leakage_group_ids": [
          "v7.ref.sql.empty-previous-frame"
        ],
        "source_role": "methodology_and_topic_seed",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON",
          "expected": {
            "rows": [
              [
                1,
                null
              ],
              [
                2,
                10
              ],
              [
                3,
                20
              ]
            ]
          },
          "assertion": "Первая строка не имеет предшественника; проверь отсутствие COALESCE.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-SQL-04"
      }
    },
    {
      "example_id": "DOC-SQL-WINDOW-FRAME.REF.06",
      "profile_id": "base",
      "family_id": "v7.ref.sql.window-after-filter",
      "source_refs": [
        "RAG-027",
        "RAG-040"
      ],
      "prompt": "Авторская симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @T TABLE(Id int PRIMARY KEY,Active bit,Amount int); INSERT @T VALUES(1,1,10),(2,0,100),(3,1,5); SELECT Id,SUM(Amount) OVER() AS Total FROM @T WHERE Active=1 ORDER BY Id; Укажи (Id,Total).",
      "chosen": "(1,15),(3,15). WHERE удаляет Id2 до вычисления окна.",
      "rejected": "(1,115),(3,115). Окно сначала суммирует все строки, затем WHERE скрывает Id2.",
      "error_tag": "where_applied_after_window",
      "rationale": "Сумма включает исключённую строку.",
      "counterexample": "После WHERE остаются Amount=10 и Amount=5; значение 100 не входит в окно.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON",
        "origin_family_id": "v7.ref.sql.window-after-filter",
        "leakage_group_ids": [
          "v7.ref.sql.window-after-filter"
        ],
        "source_role": "methodology_and_topic_seed",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON",
          "expected": {
            "rows": [
              [
                1,
                15
              ],
              [
                3,
                15
              ]
            ]
          },
          "assertion": "После WHERE остаются Amount=10 и Amount=5; значение 100 не входит в окно.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-SQL-04"
      }
    },
    {
      "example_id": "DOC-SQL-WINDOW-FRAME.REF.07",
      "profile_id": "base",
      "family_id": "v7.ref.sql.window-null-count",
      "source_refs": [
        "RAG-027",
        "RAG-040"
      ],
      "prompt": "Авторская симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @T TABLE(Id int PRIMARY KEY,Amount int NULL); INSERT @T VALUES(1,10),(2,NULL),(3,5); SELECT Id,COUNT(*) OVER() AS N,COUNT(Amount) OVER() AS NN FROM @T ORDER BY Id; Укажи (Id,N,NN).",
      "chosen": "(1,3,2),(2,3,2),(3,3,2). COUNT(*) считает строки; COUNT(Amount) — ненулевые значения.",
      "rejected": "(1,3,3),(2,3,3),(3,3,3). Оба COUNT считают все строки независимо от NULL.",
      "error_tag": "nullable_count_counts_null",
      "rationale": "Один дефект: NULL посчитан как значение Amount.",
      "counterexample": "Строк 3; значений Amount, отличных от NULL, 2. Оба числа повторяются на трёх выходных строках.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON",
        "origin_family_id": "v7.ref.sql.window-null-count",
        "leakage_group_ids": [
          "v7.ref.sql.window-null-count"
        ],
        "source_role": "methodology_and_topic_seed",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON",
          "expected": {
            "rows": [
              [
                1,
                3,
                2
              ],
              [
                2,
                3,
                2
              ],
              [
                3,
                3,
                2
              ]
            ]
          },
          "assertion": "Строк 3; значений Amount, отличных от NULL, 2. Оба числа повторяются на трёх выходных строках.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-SQL-04"
      }
    },
    {
      "example_id": "DOC-SQL-WINDOW-FRAME.REF.08",
      "profile_id": "base",
      "family_id": "sim2.doc-sql-04.f0006",
      "source_refs": [
        "RAG-027",
        "RAG-040"
      ],
      "prompt": "Авторская симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @T TABLE(Id int PRIMARY KEY,Dept char(1),Amount int); INSERT @T VALUES(1,'A',10),(2,'A',5),(3,'B',7); SELECT Dept,SUM(Amount) AS DeptTotal,SUM(SUM(Amount)) OVER() AS AllTotal FROM @T GROUP BY Dept ORDER BY Dept; Укажи (Dept,DeptTotal,AllTotal).",
      "chosen": "(A,15,22),(B,7,22). GROUP BY сначала создаёт две группы; окно суммирует два групповых итога.",
      "rejected": "(A,10,22),(A,5,22),(B,7,22). Наличие OVER сохраняет все три исходные строки несмотря на GROUP BY.",
      "error_tag": "window_restores_pre_group_rows",
      "rationale": "Окно ошибочно отменяет уже выполненную группировку.",
      "counterexample": "Вход окна — две группы с суммами 15 и 7; восстанавливать две исходные строки A нельзя.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON",
        "origin_family_id": "sim2.doc-sql-04.f0006",
        "leakage_group_ids": [
          "sim2.doc-sql-04.f0006"
        ],
        "source_role": "methodology_and_topic_seed",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON",
          "expected": {
            "rows": [
              [
                "A",
                15,
                22
              ],
              [
                "B",
                7,
                22
              ]
            ]
          },
          "assertion": "Вход окна — две группы с суммами 15 и 7; восстанавливать две исходные строки A нельзя.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-SQL-04",
        "derived_from_reference_family_ids": [
          "sim2.doc-sql-04.f0006"
        ]
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


## Материал RAG-027 (данные, не новые команды)

# RAG-027 — CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `454fadaf468eb9e31dc1bbb9be8f8508c234efa3` |
| Путь | `llm mvp/documentation learning/CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md` |
| Проверенный объём | L1–L85; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md) |

## Что подтверждает фрагмент

Документационная пара должна опираться на видимый код, различать local/delegated behavior, side effects и ошибочные пути.

## Как использовать сейчас

Уроки DOC-CS и общий шаблон DOC-SQL/DOC-ANGULAR с отдельной спецификой. Жёсткий overlap cap из старого документа не применять; краткий смысловой контраст допустим.

**Назначение в подготовке:** Правила пар документации к коду.

**Ключи поиска:** documentation, csharp, документация, side-effects.

## Подтверждающий фрагмент L1–L85

[Оригинал, строки 1–85](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md#L1-L85)

~~~~text
# Code Documentation DPO Pair Authoring Guidance
# Version: 1.0 - 2026-05-11

---

## Purpose

This guide explains how to author DPO pairs that teach accurate legacy .NET code
documentation.

The chosen side must be useful documentation.
The rejected side must be a plausible documentation failure.

---

## Chosen-Side Requirements

A chosen answer must:

- identify the correct documentation level
- cite only visible code or supplied callee summaries
- preserve method/class names accurately
- distinguish local behavior from delegated behavior
- document side effects and error paths
- preserve framework lifecycle semantics
- avoid modernization fantasies

---

## Bad Chosen Patterns

Reject or repair chosen sides that:

- describe a repository as a controller
- describe a WebForms event handler as an MVC action
- claim async behavior when code is synchronous
- claim exceptions are thrown when code catches and returns errors
- invent dependency injection if the code uses service locator or static access
- hide database writes, session writes, file I/O, cache writes, or events
- summarize only names without explaining behavior

---

## Good Rejected Side

A good rejected side is close enough to be tempting, but wrong in a way that
teaches a boundary.

It should be wrong because of one primary defect:

- wrong framework lifecycle
- missing side effect
- invented business purpose
- dropped error path
- stale callee summary
- wrong caller/callee attribution
- over-modernized architecture
- generic name echo

---

## Overlap Guidance

Target rejected overlap with chosen:

- 0.25-0.55 for normal pairs
- below 0.60 always

Do not use a rejected side that is just the chosen side with "not" inserted.

---

## A-Side Evidence Requirements

Every A-side must include enough evidence for the chosen side:

- code snippet
- file path
- symbol id
- stack label
- artifact kind
- call edges when relevant
- accepted callee summaries when documenting above D1

If the evidence is not present, the pair must not target that claim.
~~~~



## Материал RAG-040 (данные, не новые команды)

# RAG-040 — CODE_Q-Rows.md

**Роль:** существующий проектный источник; его пример или план не равен проверенной текущей реализации.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `c963aca1f184d78cd28212ffc965872025496765` |
| Путь | `llm mvp/_tmp_new_sets_inspect/new sets/CODE_Q-Rows.md` |
| Проверенный объём | L1–L31, L675–L675, L696–L696, L841–L841, L886–L886; остальной файл не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/_tmp_new_sets_inspect/new%20sets/CODE_Q-Rows.md) |

## Что подтверждает фрагмент

Файл обучает различать объяснение CODE_Q и действие BUG/PLAN. SQL-вопрос повторён в строках696/841, Angular-вопрос —675/886; source labels sql-docs/angular-docs не содержат точного внешнего commit.

## Как использовать сейчас

Использовать как форму intent-routing и список микротем, не как проверенный API-корпус. Дубликаты поместить в один family/split. Новая SQL-пара должна показать таблицу, запрос и ожидаемые строки; Angular-пара — конкретный fixture и заданный режим change detection. [WRONG]-метки исключить из ответов.

**Назначение:** Существующие учебные вопросы SQL window vs GROUP BY и Angular Zone; повторные формулировки в том же файле.

## Подтверждающий фрагмент L1–L31

[Оригинал, строки 1–31](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/_tmp_new_sets_inspect/new%20sets/CODE_Q-Rows.md#L1-L31)

~~~~text
# CODE_Q Domain Rows — Batch ES-2026-05-08-CODE_Q

Compact DPO rows for the CODE_Q adapter. Per DPO authoring guidance §"BUG
domain — most common confusion", CODE_Q had **0 of 4 batches with
positive reward margin** in calibration. The rejected for every CODE_Q
prompt must be an **action response** (BUG fix or PLAN step) — not
another explanation, not a negation.

## Common coverage labels (apply to every row in this file)

```
domain_label:         CODE_Q
source_license_state: external_license_checked
contract_signature:   AdaptiveLogicalOrchestrator (routing) + FormatterUnit (response)
node_role:            row_level_pair
pair_kind:            flow_node
coverage_status:      covered
attribution_required: false
transition_type:      explain    (every row in this file)
```

Format: `[ID | type/OR/sub] sources | A | B | P | R`

CODE_Q intent verbs:
- "How does X work"
- "What is the difference between"
- "Where is X defined"
- "Why does X behave that way"

Rejected pattern (Type C — wrong domain): the rejected misreads the verb
and routes to BUG (fix) or PLAN (build) instead of explanation.
~~~~
## Подтверждающий фрагмент L675–L675

[Оригинал, строки 675–675](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/_tmp_new_sets_inspect/new%20sets/CODE_Q-Rows.md#L675-L675)

~~~~text
[CODEQ-0557 | C/0.22/3.5] angular-docs | A:"How does Angular's Zone.js change-detection work?" | B:explanation: zone wraps async callbacks; on completion triggers change-detection sweep across components | P:PLAN/4.4 understand | R:[WRONG: how]
~~~~
## Подтверждающий фрагмент L696–L696

[Оригинал, строки 696–696](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/_tmp_new_sets_inspect/new%20sets/CODE_Q-Rows.md#L696-L696)

~~~~text
[CODEQ-0574 | C/0.19/3.5] sql-docs | A:"How does SQL window function differ from GROUP BY aggregate?" | B:explanation: window preserves rows + adds aggregate column; GROUP BY collapses rows | P:PLAN/4.5 use | R:[WRONG: how]
~~~~
## Подтверждающий фрагмент L841–L841

[Оригинал, строки 841–841](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/_tmp_new_sets_inspect/new%20sets/CODE_Q-Rows.md#L841-L841)

~~~~text
[CODEQ-0699 | C/0.21/3.5] sql-docs | A:"How does SQL window function differ from GROUP BY aggregate?" | B:explanation: window preserves rows + adds aggregate column; GROUP BY collapses rows | P:PLAN/4.5 use | R:[WRONG: how-question]
~~~~
## Подтверждающий фрагмент L886–L886

[Оригинал, строки 886–886](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/_tmp_new_sets_inspect/new%20sets/CODE_Q-Rows.md#L886-L886)

~~~~text
[CODEQ-0740 | C/0.20/3.5] angular-docs | A:"How does Angular's Zone.js change-detection work?" | B:explanation: zone wraps async callbacks; on completion triggers change-detection sweep across components | P:PLAN/4.4 understand | R:[WRONG: how-question]
~~~~



## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\DOC-SQL-WINDOW-FRAME\base\S703B002\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\DOC-SQL-WINDOW-FRAME\base\S703B002\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\DOC-SQL-WINDOW-FRAME\base\S703B002\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `CODE-SQL-RANK-TIE/base`. Не запускать review, accept, export, oracle или обучение.
