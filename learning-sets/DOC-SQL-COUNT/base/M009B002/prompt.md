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
  "stream_id": "DOC-SQL-COUNT/base",
  "topic_id": "DOC-SQL-01",
  "objective_id": "DOC-SQL-COUNT",
  "domain_id": "docs.sql",
  "profile_id": "base",
  "run_id": "M009B002",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/DOC-SQL-COUNT/base/F0000017",
    "v3/DOC-SQL-COUNT/base/F0000019",
    "v3/DOC-SQL-COUNT/base/F0000023",
    "v3/DOC-SQL-COUNT/base/F0000024",
    "v3/DOC-SQL-COUNT/base/F0000025",
    "v3/DOC-SQL-COUNT/base/F0000026",
    "v3/DOC-SQL-COUNT/base/F0000027",
    "v3/DOC-SQL-COUNT/base/F0000028",
    "v3/DOC-SQL-COUNT/base/F0000029",
    "v3/DOC-SQL-COUNT/base/F0000032"
  ],
  "family_contracts": {
    "v3/DOC-SQL-COUNT/base/F0000017": {
      "origin_family_id": "v3/DOC-SQL-COUNT/base/F0000017",
      "leakage_group_ids": [
        "v3/DOC-SQL-COUNT/base/F0000017"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-SQL-COUNT/base/F0000019": {
      "origin_family_id": "v3/DOC-SQL-COUNT/base/F0000019",
      "leakage_group_ids": [
        "v3/DOC-SQL-COUNT/base/F0000019"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-SQL-COUNT/base/F0000023": {
      "origin_family_id": "v3/DOC-SQL-COUNT/base/F0000023",
      "leakage_group_ids": [
        "v3/DOC-SQL-COUNT/base/F0000023"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-SQL-COUNT/base/F0000024": {
      "origin_family_id": "v3/DOC-SQL-COUNT/base/F0000024",
      "leakage_group_ids": [
        "v3/DOC-SQL-COUNT/base/F0000024"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-SQL-COUNT/base/F0000025": {
      "origin_family_id": "v3/DOC-SQL-COUNT/base/F0000025",
      "leakage_group_ids": [
        "v3/DOC-SQL-COUNT/base/F0000025"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-SQL-COUNT/base/F0000026": {
      "origin_family_id": "v3/DOC-SQL-COUNT/base/F0000026",
      "leakage_group_ids": [
        "v3/DOC-SQL-COUNT/base/F0000026"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-SQL-COUNT/base/F0000027": {
      "origin_family_id": "v3/DOC-SQL-COUNT/base/F0000027",
      "leakage_group_ids": [
        "v3/DOC-SQL-COUNT/base/F0000027"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-SQL-COUNT/base/F0000028": {
      "origin_family_id": "v3/DOC-SQL-COUNT/base/F0000028",
      "leakage_group_ids": [
        "v3/DOC-SQL-COUNT/base/F0000028"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-SQL-COUNT/base/F0000029": {
      "origin_family_id": "v3/DOC-SQL-COUNT/base/F0000029",
      "leakage_group_ids": [
        "v3/DOC-SQL-COUNT/base/F0000029"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-SQL-COUNT/base/F0000032": {
      "origin_family_id": "v3/DOC-SQL-COUNT/base/F0000032",
      "leakage_group_ids": [
        "v3/DOC-SQL-COUNT/base/F0000032"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 33,
  "sources": [
    "RAG-040",
    "RAG-041"
  ],
  "card": "curriculum/cards/DOC-SQL-COUNT.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-SQL-COUNT\\base\\M009B002",
  "mode": "CREATE_ONLY",
  "next_stream_id": "CODE-SQL-COUNT/base",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\DOC-SQL-COUNT.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-SQL-COUNT\\base\\M009B002\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-SQL-COUNT\\base\\M009B002\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-SQL-COUNT\\base\\M009B002\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-SQL-COUNT\\base\\M009B002\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-SQL-COUNT\\base\\M009B002\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-SQL-COUNT\\base\\M009B002\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-040": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-040.md",
      "RAG-041": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-041.md"
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
  "objective_id": "DOC-SQL-COUNT",
  "topic_id": "DOC-SQL-01",
  "title_ru": "Документировать COUNT по nullable столбцу",
  "learning_goal_ru": "Точно объяснять COUNT(nullable_column): исключение NULL, сохранение дубликатов и одна итоговая строка без GROUP BY.",
  "generation_prompt_ru": "Пиши только документацию одного SELECT COUNT(v) без JOIN, GROUP BY, DISTINCT, UPDATE или window functions. Полностью задай DECLARE таблицы, INSERT и запрос; пустая таблица задаётся без INSERT. В данных допускай NULL, дубликаты, 0 и отрицательные числа. Chosen называет число строк результата, значение n и правило подсчёта. Rejected меняет одну ошибочную интерпретацию COUNT: учитывает NULL, удаляет дубликаты, считает 0 отсутствующим или возвращает ноль строк при пустом входе. Не делай выводов о производительности и порядке исходных строк.\n\nРаботай только с данным objective_id и назначенными family_id/split/count из request.json. Пиши одну смысловую пару на назначенное семейство; chosen и rejected должны иметь одинаковый формат и отличаться одной проверяемой ошибкой. Все сведения, версии, исходные данные и договор ответа помещай в prompt: metadata learner не получает. Примеры ниже — открытые авторские reference seeds, не закрытый EXAM, не сгенерированный китайской моделью корпус и не измеренные результаты. Не копируй reference-строки в TRAIN под новым ID. Переименование переменных/таблиц и замена чисел не делают шаблон новым независимым семейством. Для документации объясняй именно показанный код, для генерации возвращай только запрошенный код. Не вставляй в ответы labels, rationale, source ID и предпочтительную позицию. Oracle expected — заранее назначенное ожидание; статус остаётся NOT_RUN, пока соответствующий runtime действительно не выполнен. Runtime oracle сейчас не запускай; сохрани его ожидания как NOT_RUN, не выдавай их за наблюдённый результат. Не переходи к другой теме внутри пакета. При NEXT немедленно используй следующий ID из реестра, при STOP прекрати новые действия. Production и старые эксперименты не менять; отчёт на русском по 08-REPORT-TEMPLATE.md, фактическая текущая Kiro-модель без override.",
  "response_contract_ru": "Одна русская фраза о числе строк и n; второе предложение объясняет подсчёт по видимым данным.",
  "scenario_axes": [
    {
      "axis": "вход",
      "values": [
        "NULL с дубликатами",
        "только NULL",
        "0 и отрицательное число",
        "пустой набор"
      ]
    },
    {
      "axis": "ошибка",
      "values": [
        "COUNT как COUNT(*)",
        "COUNT как DISTINCT",
        "ноль как отсутствие",
        "пустой результат вместо scalar aggregate"
      ]
    }
  ],
  "forbidden_mixing": [
    "JOIN и ON/WHERE",
    "GROUP BY/HAVING",
    "изменение таблиц",
    "транзакции/блокировки"
  ],
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "DOC-SQL-COUNT.REF.01",
      "profile_id": "base",
      "family_id": "ref.sql.count.duplicate-null.01",
      "prompt": "Авторская симуляция. T-SQL / SQL Server 2019, ANSI_NULLS ON. Выполняется только этот batch:\nDECLARE @T TABLE (v int NULL); INSERT INTO @T(v) VALUES (7),(NULL),(7); SELECT COUNT(v) AS n FROM @T;\nОпиши число строк результата, значение n и какие строки учитываются. Не выполняй изменения внешней БД.",
      "chosen": "Одна строка: n=2. Оба значения 7 учитываются, NULL исключается.",
      "rejected": "Одна строка: n=1. Повторяющееся значение 7 учитывается один раз, NULL исключается.",
      "error_tag": "invented_distinct",
      "rationale": "COUNT(v) считает ненулевые строки, а DISTINCT в запросе нет.",
      "counterexample": "Две строки со значением 7 дают 2, не 1.",
      "source_refs": [
        "RAG-040",
        "RAG-041"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "T-SQL / SQL Server 2019; ANSI_NULLS ON; table variables local to the batch",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "origin_family_id": "ref.sql.count.duplicate-null.01",
        "leakage_group_ids": [
          "ref.sql.count.duplicate-null.01"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "sqlserver2019",
          "setup_and_query": "DECLARE @T TABLE (v int NULL); INSERT INTO @T(v) VALUES (7),(NULL),(7); SELECT COUNT(v) AS n FROM @T;",
          "expected_result": {
            "columns": [
              "n"
            ],
            "rows": [
              [
                2
              ]
            ]
          },
          "assertion": "Exactly one row and exact integer n; table variable is isolated per batch."
        }
      }
    },
    {
      "example_id": "DOC-SQL-COUNT.REF.02",
      "profile_id": "base",
      "family_id": "ref.sql.count.only-null.01",
      "prompt": "Авторская симуляция. T-SQL / SQL Server 2019, ANSI_NULLS ON. Выполняется только этот batch:\nDECLARE @T TABLE (v int NULL); INSERT INTO @T(v) VALUES (NULL),(NULL); SELECT COUNT(v) AS n FROM @T;\nОпиши число строк результата, значение n и какие строки учитываются. Не выполняй изменения внешней БД.",
      "chosen": "Одна строка: n=0. Обе строки содержат NULL в v и не увеличивают COUNT(v).",
      "rejected": "Одна строка: n=2. Обе исходные строки увеличивают COUNT(v), включая NULL.",
      "error_tag": "null_counted",
      "rationale": "COUNT(v) исключает NULL; количество исходных строк не равно его значению.",
      "counterexample": "Два NULL дают n=0, не n=2.",
      "source_refs": [
        "RAG-040",
        "RAG-041"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "T-SQL / SQL Server 2019; ANSI_NULLS ON; table variables local to the batch",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "origin_family_id": "ref.sql.count.only-null.01",
        "leakage_group_ids": [
          "ref.sql.count.only-null.01"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "sqlserver2019",
          "setup_and_query": "DECLARE @T TABLE (v int NULL); INSERT INTO @T(v) VALUES (NULL),(NULL); SELECT COUNT(v) AS n FROM @T;",
          "expected_result": {
            "columns": [
              "n"
            ],
            "rows": [
              [
                0
              ]
            ]
          },
          "assertion": "Exactly one row and exact integer n; table variable is isolated per batch."
        }
      }
    },
    {
      "example_id": "DOC-SQL-COUNT.REF.03",
      "profile_id": "base",
      "family_id": "ref.sql.count.zero-negative.01",
      "prompt": "Авторская симуляция. T-SQL / SQL Server 2019, ANSI_NULLS ON. Выполняется только этот batch:\nDECLARE @T TABLE (v int NULL); INSERT INTO @T(v) VALUES (0),(-3),(NULL); SELECT COUNT(v) AS n FROM @T;\nОпиши число строк результата, значение n и какие строки учитываются. Не выполняй изменения внешней БД.",
      "chosen": "Одна строка: n=2. Ноль и -3 являются не-NULL значениями и учитываются.",
      "rejected": "Одна строка: n=1. Учитывается -3, а ноль и NULL считаются отсутствующими.",
      "error_tag": "zero_treated_as_null",
      "rationale": "Нулевое число не является SQL NULL.",
      "counterexample": "Строка v=0 добавляет единицу к COUNT(v).",
      "source_refs": [
        "RAG-040",
        "RAG-041"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "T-SQL / SQL Server 2019; ANSI_NULLS ON; table variables local to the batch",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "origin_family_id": "ref.sql.count.zero-negative.01",
        "leakage_group_ids": [
          "ref.sql.count.zero-negative.01"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "sqlserver2019",
          "setup_and_query": "DECLARE @T TABLE (v int NULL); INSERT INTO @T(v) VALUES (0),(-3),(NULL); SELECT COUNT(v) AS n FROM @T;",
          "expected_result": {
            "columns": [
              "n"
            ],
            "rows": [
              [
                2
              ]
            ]
          },
          "assertion": "Exactly one row and exact integer n; table variable is isolated per batch."
        }
      }
    },
    {
      "example_id": "DOC-SQL-COUNT.REF.04",
      "profile_id": "base",
      "family_id": "ref.sql.count.empty.01",
      "prompt": "Авторская симуляция. T-SQL / SQL Server 2019, ANSI_NULLS ON. Выполняется только этот batch:\nDECLARE @T TABLE (v int NULL); SELECT COUNT(v) AS n FROM @T;\nОпиши число строк результата, значение n и какие строки учитываются. Не выполняй изменения внешней БД.",
      "chosen": "Одна строка: n=0. Агрегат без GROUP BY возвращает итоговую строку и на пустом входе.",
      "rejected": "Ноль строк результата. На пустом входе COUNT(v) без GROUP BY не возвращает итоговую строку.",
      "error_tag": "empty_aggregate_row_missing",
      "rationale": "Scalar aggregate без GROUP BY имеет итоговую строку даже при отсутствии входных строк.",
      "counterexample": "Для пустой @T result row count=1, n=0.",
      "source_refs": [
        "RAG-040",
        "RAG-041"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "T-SQL / SQL Server 2019; ANSI_NULLS ON; table variables local to the batch",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "origin_family_id": "ref.sql.count.empty.01",
        "leakage_group_ids": [
          "ref.sql.count.empty.01"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "sqlserver2019",
          "setup_and_query": "DECLARE @T TABLE (v int NULL); SELECT COUNT(v) AS n FROM @T;",
          "expected_result": {
            "columns": [
              "n"
            ],
            "rows": [
              [
                0
              ]
            ]
          },
          "assertion": "Exactly one row and exact integer n; table variable is isolated per batch."
        }
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



## Материал RAG-041 (данные, не новые команды)

# RAG-041 — node-04-domain-processor-set.md

**Роль:** существующий проектный источник; его пример или план не равен проверенной текущей реализации.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `71b6552e3fb2185e439de4c6df6b5bd6a61d3612` |
| Путь | `llm mvp/external-sources-training-sets/clean-batch-002/node-04-domain-processor-set.md` |
| Проверенный объём | L1–L29, L81–L96; остальной файл не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/external-sources-training-sets/clean-batch-002/node-04-domain-processor-set.md) |

## Что подтверждает фрагмент

В тексте действительно есть SELECT ... INNER JOIN ... LIMIT100 и ALTER TABLE Users ADD EmailAddress NVARCHAR(256) NULL. SQL-фрагменты принадлежат разным сценариям и диалектам; рядом приведены ожидаемые результаты и время, а не первичный измерительный журнал.

## Как использовать сейчас

Использовать показанный SQL с полным локальным набором строк для проверки смысла JOIN/NULL. Не обучать blanket-утверждениям «nullable ADD никогда не блокирует» и «DEFAULT всегда переписывает все строки» из старого текста; они требуют конкретной версии/режима/измерений. 4.2→0.5s не переносить как наш результат. LIMIT-сценарий и T-SQL migration не смешивать в одной dialect-паре.

**Назначение:** Встроенные SQL-примеры SELECT JOIN и ALTER TABLE; контекст, success/failure/wait states.

## Подтверждающий фрагмент L1–L29

[Оригинал, строки 1–29](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/external-sources-training-sets/clean-batch-002/node-04-domain-processor-set.md#L1-L29)

~~~~text
# Node 04 — Domain Processor — Clean Batch 002

Node 4 receives exactly one sub-task plus its domain context.
It produces all success states, all failure states, and all wait-state outcomes for that one sub-task.
The output must contain enough information for the applier and result translator to act without re-reading the original input.

A is the exact sub-task plus the domain context as it would arrive at runtime.
B is the complete enumeration of success, failure, and wait-state outcomes — not a description of what those outcomes would address.
A B that says "the outcomes include success and failure cases" is wrong — each outcome must be named with its specific condition and specific result.

One pair covers one sub-task. Array nodes receive one element at a time.

---

Pair CB2-04-001 — Add select_related('author') to Post queryset
Source: https://github.com/princeton-nlp/SWE-bench — SWE-bench task instances
License: MIT
A: Sub-task: "Add select_related('author') to the Post queryset"
Domain context: [D-database-query-efficiency] The current queryset is Post.objects.all()[:100]. It generates 101 database queries for 100 posts: 1 for posts, then 1 per post for the author. The template accesses post.author.name in a loop. The fix is to add select_related('author') so Django generates a single JOIN query instead of N+1 individual queries.
B:
Success state: Post.objects.select_related('author').all()[:100] executes one SQL query: SELECT blog_post.id, blog_post.title, blog_post.body, blog_post.author_id, auth_user.id, auth_user.username, auth_user.first_name, auth_user.last_name FROM blog_post INNER JOIN auth_user ON blog_post.author_id = auth_user.id LIMIT 100. The ORM caches the author object on each Post instance from the JOIN result. Template access to post.author.name does not trigger a database query. Total queries for the request: 1. Expected render time: under 0.5 seconds, down from 4.2 seconds.

Failure state 1 — FieldError: If Post.author is not a direct ForeignKey field (e.g., it is a reverse ForeignKey or a ManyToManyField), select_related raises FieldError: "Invalid field name(s) for model Post: 'author'". Verify that Post.author is defined as author = models.ForeignKey(User, on_delete=models.CASCADE) or equivalent. If Post.author is a ManyToManyField, use prefetch_related('authors') instead.

Failure state 2 — Over-fetching after the fix: select_related('author') joins the full auth_user row. If auth_user has 30 columns and the template uses only username, 29 columns are transferred and discarded per row. This is acceptable at 100 posts but becomes significant at higher limits. Mitigate with .only('id', 'title', 'body', 'author__username') if the query is performance-critical at scale.

Wait state: No migration required. select_related is a Python-layer query modifier. No database schema changes. No coordination with other services. The queryset change is safe to deploy as a single commit to the view function.
Reason: The success state names the specific SQL generated, the specific template access pattern that no longer triggers queries, and the target query count and render time. The failure states name the specific exceptions and their conditions. The wait state confirms no migration or service coordination is needed — the information the applier requires to proceed without further input.
Source: https://github.com/princeton-nlp/SWE-bench — SWE-bench task instances
~~~~
## Подтверждающий фрагмент L81–L96

[Оригинал, строки 81–96](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/external-sources-training-sets/clean-batch-002/node-04-domain-processor-set.md#L81-L96)

~~~~text
Pair CB2-04-003 — Add EmailAddress nullable column migration
Source: https://github.com/dotnet/efcore — EF Core migration documentation
License: MIT
A: Sub-task: "Create an EF Core migration that adds a nullable EmailAddress column to the Users table"
Domain context: [Database schema evolution] SQL Server, Users table, 5 million rows. Active read and write traffic during business hours. Three Dapper services read the Email column by name. This is Phase 1 of a 5-phase expand-contract column rename. The goal is to add the new column without acquiring a table lock and without modifying any application service.
B:
Success state: EF Core generates the migration with the SQL statement: ALTER TABLE Users ADD EmailAddress NVARCHAR(256) NULL. SQL Server executes this DDL without a full table lock — adding a nullable column with no DEFAULT value is a metadata-only operation on SQL Server 2012 and later. After the migration: all existing rows have EmailAddress = NULL; the Email column is unchanged; all three services continue querying the Email column without error; EF Core __EFMigrationsHistory contains the new migration ID.

Failure state 1 — Table lock if column has a default or NOT NULL constraint: If the migration specifies NVARCHAR(256) NOT NULL DEFAULT '' instead of NULL, SQL Server must update every existing row to set the default value, acquiring a lock for the duration. On 5 million rows this causes a blocking period that violates the zero-downtime requirement. The migration must specify NULL with no DEFAULT to remain non-blocking.

Failure state 2 — Column add blocked by concurrent schema lock: If another DDL operation (index rebuild, statistics update job) holds a schema lock at migration time, ALTER TABLE blocks. Confirm no concurrent DDL is running before executing, or schedule the migration outside peak DDL activity.

Failure state 3 — NOT NULL constraint added prematurely: If a subsequent migration immediately adds a NOT NULL constraint before the backfill runs in Phase 3, existing rows with NULL EmailAddress violate the constraint and the migration fails. The NOT NULL constraint must wait until Phase 5, after backfill and read cutover are complete.

Wait state: Do not deploy Phase 2 (dual-write application code) until __EFMigrationsHistory confirms the migration ID is present in the production database. If the migration has not run, the EmailAddress column does not exist and any dual-write code that writes to it will throw a SqlException on every INSERT and UPDATE.
Reason: The success state names the exact SQL generated, the SQL Server version requirement for non-blocking behavior, and the observable confirmation (EFMigrationsHistory). The failure states name the specific conditions that make the operation blocking or failing. The wait state names the exact blocking condition for Phase 2 — the column must exist before dual-write code is deployed. Note: Failure state 1 (NOT NULL with DEFAULT at column creation) and Failure state 3 (NOT NULL constraint added before backfill completes) share the same root cause — a NOT NULL constraint present before all rows have values — but are kept as separate entries because they occur at different phases, produce different observable signals (a table lock during the initial migration versus a constraint violation during a subsequent migration), and require different remediations (fix the column definition in the current migration versus defer the NOT NULL constraint to Phase 5).
~~~~



## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\DOC-SQL-COUNT\base\M009B002\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\DOC-SQL-COUNT\base\M009B002\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\DOC-SQL-COUNT\base\M009B002\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `CODE-SQL-COUNT/base`. Не запускать review, accept, export, oracle или обучение.
