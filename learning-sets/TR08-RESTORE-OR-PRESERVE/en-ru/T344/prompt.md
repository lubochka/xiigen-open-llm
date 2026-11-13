# Создание одной партии DPO в Kiro — CREATE_ONLY

<!-- CURRENT-LANGUAGE-AND-CAMPAIGN -->
Reason internally in English. Do not output private reasoning. Russian status; learner language follows the assigned profile. This prompt remains for an explicitly selected legacy request. New domain quotas and continuation use [campaign/registry.json](../campaign/registry.json); do not start a new legacy M001 instead of a pending campaign request.
<!-- /CURRENT-LANGUAGE-AND-CAMPAIGN -->


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
  "stream_id": "TR08-RESTORE-OR-PRESERVE/en-ru",
  "topic_id": "TR08",
  "objective_id": "TR08-RESTORE-OR-PRESERVE",
  "domain_id": "tr.ru",
  "profile_id": "en-ru",
  "run_id": "T344",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000000",
    "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000001",
    "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000002",
    "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000003",
    "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000004"
  ],
  "family_contracts": {
    "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000000": {
      "origin_family_id": "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000000",
      "leakage_group_ids": [
        "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000000"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000001": {
      "origin_family_id": "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000001",
      "leakage_group_ids": [
        "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000001"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000002": {
      "origin_family_id": "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000002",
      "leakage_group_ids": [
        "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000002"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000003": {
      "origin_family_id": "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000003",
      "leakage_group_ids": [
        "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000003"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000004": {
      "origin_family_id": "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000004",
      "leakage_group_ids": [
        "v3/tr/TR08-RESTORE-OR-PRESERVE/F0000004"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 2,
  "next_start": 5,
  "sources": [
    "RAG-024",
    "RAG-023"
  ],
  "card": "curriculum/cards/TR08-RESTORE-OR-PRESERVE.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR08-RESTORE-OR-PRESERVE\\en-ru\\T344",
  "mode": "CREATE_ONLY",
  "next_stream_id": "TR08-RESTORE-OR-PRESERVE/ru-he",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\TR08-RESTORE-OR-PRESERVE.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR08-RESTORE-OR-PRESERVE\\en-ru\\T344\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR08-RESTORE-OR-PRESERVE\\en-ru\\T344\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR08-RESTORE-OR-PRESERVE\\en-ru\\T344\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR08-RESTORE-OR-PRESERVE\\en-ru\\T344\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR08-RESTORE-OR-PRESERVE\\en-ru\\T344\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR08-RESTORE-OR-PRESERVE\\en-ru\\T344\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-024": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-024.md",
      "RAG-023": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-023.md"
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
  "training_status": "NOT_RUN",
  "family_class_field": "span_treatment",
  "family_class_values": [
    "restore",
    "preserve"
  ]
}
```

На каждую family_id создай по одной строке каждого класса в metadata.span_treatment: restore, preserve. Всего строк count.

## Предметное содержание карточки (без исторических команд)

{
  "objective_id": "TR08-RESTORE-OR-PRESERVE",
  "topic_id": "TR08",
  "title_ru": "Восстановить неверную раскладку или сохранить буквальный идентификатор по видимому контексту",
  "learning_goal_ru": "При одинаковом raw token различать исправление случайной раскладки обычного слова и точное сохранение идентификатора по роли фрагмента в видимом условии.",
  "generation_prompt_ru": "Подготовь контрфактические DPO-пары для одного направления перевода: одно и то же значение поля, набранное буквами другой раскладки, в одном видимом контексте является ошибочно набранным обычным словом, а в другом — точным буквальным идентификатором. В обеих строках оставь неизменными raw token, исходную фразу, карту клавиш, формат ответа и все факты кроме роли значения. Контекст роли обязателен прямо в learner prompt. Не раскрывай canonical_source, ответ, family ID или rationale в prompt. Для каждого назначенного family_id сделай две строки: metadata.span_treatment=restore и preserve. Вторая меняет предпочтение: chosen первой становится rejected второй и наоборот, если перевод окружающего текста неизменен. В каждом из шести новых сценарных семейств первой партии используй свою лексему и свой самодостаточный контекст; повторный пример с другим ID или именем не считается независимым сценарием. Используй только domains/translation/layout-fixture-v1.json, только поддержанные unshifted code points. В каждый learner prompt вставь полную таблицу соответствия выбранных двух раскладок из fixture, а не только её имя и не только символы текущего ответа. В restore/preserve контрфактуалах карта одинакова. Raw получи механически из canonical, затем проверь обратное преобразование и точные code-point границы raw внутри исходной строки. Для ru-source допустимо en-qwerty→ru-ycuken, для he-source en-qwerty→he-fixture, для en-source ru-ycuken→en-qwerty или he-fixture→en-qwerty. Сохрани ordinary canonical/typo/wrong-layout в одной лексической семье. Буквальный идентификатор не объединяй с лексемой по совпадению символов; контрфактическая пара всё равно имеет один scenario family и split. Первый batch12: шесть restore и шесть preserve, связанные попарно; не додумывай дополнительные темы и профили. Другой естественный перевод, синоним или допустимый порядок слов не является rejected. Неоднозначное намерение без заданного типа не размечай силой — отправь в пересмотр/назначенную TR10-тему. Не делай все restore-случаи латиницей, а preserve кириллицей: внутри каждой семьи raw совпадает. Разнообразь формулировки видимого контекста и окружающего текста между семьями; не обучай угадывать метку по одному шаблонному слову. Пиши draft.jsonl в назначенный output_directory; текущая стадия заканчивается структурным finish без независимого предметного review.\n\nОдин request задаёт одну микроцель, один профиль и один split. Используй текущую выбранную китайскую модель Kiro без override; помощники наследуют её. Имя модели записывай только из фактических метаданных. Отчёт и rationale — по-русски, ответы — на языке назначения с сохранением буквальных идентификаторов. Нет обязательных checksum, подписей, полного production CI, фиксированного имени модели, процента coverage или cross-model gate. Сохраняются необходимые проверки смысла пары, источника, состава и независимости учебных/экзаменационных семей. Прочитай 02-DATA-CONTRACT.md, 04-QUALITY-AND-SPLITS.md, 07-CONFLICTS-AND-REQUIREMENTS.md, эту карточку и RAG-024/RAG-023. Текущая фаза — подготовка DPO; не запускай обучение и не меняй production по собственной инициативе. При следующем ID немедленно переключись: запрещено продолжать предыдущую задачу, дописывать её отчёт, чинить её код или повторять запуск. При STOP прекрати действия немедленно. Старые эксперименты, строки и результаты сохраняй; новый run/revision идёт рядом. Если позднее отдельно выполняется разрешённое обучение, восстанови квалифицированный B1 по 05; один живой процесс продолжай через его handle и штатный bounded wait до terminal, не заканчивай ход вместо ожидания, не дублируй запуски и не делай частый polling. Любое измеренное улучшение полезной метрики при валидном обучении — успех по этой метрике/домену; другие изменения, включая ухудшения, показываются рядом. 0 правильных до и после не доказывает обучение; margin/loss и скорость подготовки это не заменяют.",
  "response_contract_ru": "Только перевод исходной строки на язык назначения; обычное слово восстановить и перевести, буквальный идентификатор сохранить точно. Не переводить контекст и не добавлять объяснение.",
  "scenario_axes": [
    {
      "axis": "span_treatment",
      "values": [
        "restore",
        "preserve"
      ]
    },
    {
      "axis": "carrier",
      "values": [
        "field_value",
        "short_sentence",
        "quoted_value"
      ]
    },
    {
      "axis": "restoration_map",
      "values": [
        "source_ru_en_to_ru",
        "source_he_en_to_he",
        "source_en_ru_to_en",
        "source_en_he_to_en"
      ]
    }
  ],
  "forbidden_mixing": [
    "Смешивание нескольких направлений перевода или objective в одном request.",
    "Объявление каждого смешанного алфавита ошибкой и автоматическое исправление всех строк.",
    "Объединение буквального ID с лексемой только по совпадающему raw.",
    "Скрытое намерение: роль значения есть только в metadata, но отсутствует в learner prompt.",
    "Одновременная опечатка, другая карта, изменение пола/роли/смысла окружающего предложения.",
    "Разделение restore/preserve, обратных переводов и повторных форм одной семьи между TRAIN/DEV/EXAM.",
    "Перенос публичных reference и их производных в запечатанный экзамен."
  ],
  "pairs_per_family": 2,
  "family_class_field": "span_treatment",
  "family_class_values": [
    "restore",
    "preserve"
  ],
  "examples": [
    {
      "example_id": "TR08-RESTORE-OR-PRESERVE.en-ru.REF01",
      "profile_id": "en-ru",
      "family_id": "v2-ref.translation.ticket-on-table",
      "prompt": "Переведи с языка «английский» на язык «русский» только исходную строку ниже. Сохрани структуру «название поля: значение»; не добавляй объяснение. Буквальные идентификаторы сохраняются точно; обычное слово со случайной ошибкой раскладки восстанавливается и переводится. Для этого примера доступна карта layout-fixture-v1: фактическая раскладка ru-ycuken, предполагаемая en-qwerty. Наличие карты само по себе не доказывает, что значение ошибочно. Полная карта для этого случая: каждый ключ JSON — символ фактической раскладки, значение — символ предполагаемой; применяется посимвольно, остальные символы остаются на месте. {\"й\": \"q\", \"ц\": \"w\", \"у\": \"e\", \"к\": \"r\", \"е\": \"t\", \"н\": \"y\", \"г\": \"u\", \"ш\": \"i\", \"щ\": \"o\", \"з\": \"p\", \"х\": \"[\", \"ъ\": \"]\", \"ф\": \"a\", \"ы\": \"s\", \"в\": \"d\", \"а\": \"f\", \"п\": \"g\", \"р\": \"h\", \"о\": \"j\", \"л\": \"k\", \"д\": \"l\", \"ж\": \";\", \"э\": \"'\", \"я\": \"z\", \"ч\": \"x\", \"с\": \"c\", \"м\": \"v\", \"и\": \"b\", \"т\": \"n\", \"ь\": \"m\", \"б\": \",\", \"ю\": \".\", \".\": \"/\"} Контекст значения: Значение поля — обычное слово о документе для проезда, который пассажир предъявляет контролёру. Пользователь сообщил, что случайно набрал именно значение в неверной раскладке; это не код и не идентификатор.\nИсходная строка: Value: ешслуе",
      "chosen": "Значение: билет",
      "rejected": "Значение: ешслуе",
      "error_tag": "missed_layout_restoration",
      "rationale": "Видимый контекст объявляет случайную ошибку раскладки в обычном слове. Восстановление по указанной карте даёт слово о проездном документе; его непереведённая последовательность клавиш не передаёт смысл.",
      "counterexample": "Та же исходная строка с противоположным явно заданным типом значения требует другого ответа. Если тип не указан и оба чтения правдоподобны, это TR10-CONDITIONAL, не угадывание здесь.",
      "source_refs": [
        "RAG-024",
        "RAG-023"
      ],
      "metadata": {
        "response_language": "ru",
        "language_pair": {
          "source": "en",
          "target": "ru"
        },
        "reference_only": true,
        "split": "reference",
        "simulation": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "measured_training_result": false,
        "source_role": "methodology_not_translation_gold",
        "generator_model": "reference_authoring_not_a_kiro_run",
        "origin_family_id": "v2-ref.translation.ticket-on-table",
        "leakage_group_ids": [
          "source:v2-ref.translation.ticket-on-table",
          "surface-context:v2-ref.translation.ticket-on-table"
        ],
        "learning_objective_id": "TR08-RESTORE-OR-PRESERVE",
        "source_raw": "Value: ешслуе",
        "raw_token": "ешслуе",
        "source_span_codepoints": [
          7,
          13
        ],
        "canonical_source": "Value: ticket",
        "layout_from": "ru-ycuken",
        "layout_to": "en-qwerty",
        "map_id": "layout-fixture-v1",
        "layout_applies_to": "raw_token_only",
        "layout_validation": "fixture_mapping_checked",
        "learner_map_visible": true,
        "decoded_if_transformed": "ticket",
        "span_treatment": "restore",
        "correction_required": true,
        "lexical_family_ids": [
          "lex:en:ticket"
        ],
        "exact_identifiers": [],
        "do_not_merge": [],
        "response_contract": "translate_label_then_contextual_span",
        "derived_from": [
          "TR08-LAYOUT.en-ru.REF01"
        ],
        "contrast_pair_id": "TR08-RESTORE-OR-PRESERVE.en-ru.REF02"
      }
    },
    {
      "example_id": "TR08-RESTORE-OR-PRESERVE.en-ru.REF02",
      "profile_id": "en-ru",
      "family_id": "v2-ref.translation.ticket-on-table",
      "prompt": "Переведи с языка «английский» на язык «русский» только исходную строку ниже. Сохрани структуру «название поля: значение»; не добавляй объяснение. Буквальные идентификаторы сохраняются точно; обычное слово со случайной ошибкой раскладки восстанавливается и переводится. Для этого примера доступна карта layout-fixture-v1: фактическая раскладка ru-ycuken, предполагаемая en-qwerty. Наличие карты само по себе не доказывает, что значение ошибочно. Полная карта для этого случая: каждый ключ JSON — символ фактической раскладки, значение — символ предполагаемой; применяется посимвольно, остальные символы остаются на месте. {\"й\": \"q\", \"ц\": \"w\", \"у\": \"e\", \"к\": \"r\", \"е\": \"t\", \"н\": \"y\", \"г\": \"u\", \"ш\": \"i\", \"щ\": \"o\", \"з\": \"p\", \"х\": \"[\", \"ъ\": \"]\", \"ф\": \"a\", \"ы\": \"s\", \"в\": \"d\", \"а\": \"f\", \"п\": \"g\", \"р\": \"h\", \"о\": \"j\", \"л\": \"k\", \"д\": \"l\", \"ж\": \";\", \"э\": \"'\", \"я\": \"z\", \"ч\": \"x\", \"с\": \"c\", \"м\": \"v\", \"и\": \"b\", \"т\": \"n\", \"ь\": \"m\", \"б\": \",\", \"ю\": \".\", \".\": \"/\"} Контекст значения: Значение поля — буквальный идентификатор записи. Опечатки в нём нет; все его символы значимы, алиасов нет. Совпадение с последовательностью клавиш обычного слова не меняет идентификатор.\nИсходная строка: Value: ешслуе",
      "chosen": "Значение: ешслуе",
      "rejected": "Значение: билет",
      "error_tag": "false_layout_restoration_of_identifier",
      "rationale": "Видимый контекст задаёт точный идентификатор без ошибки. Перевод названия поля допустим, но замена буквального значения обычным словом создаёт другой идентификатор.",
      "counterexample": "Та же исходная строка с противоположным явно заданным типом значения требует другого ответа. Если тип не указан и оба чтения правдоподобны, это TR10-CONDITIONAL, не угадывание здесь.",
      "source_refs": [
        "RAG-024",
        "RAG-023"
      ],
      "metadata": {
        "response_language": "ru",
        "language_pair": {
          "source": "en",
          "target": "ru"
        },
        "reference_only": true,
        "split": "reference",
        "simulation": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "measured_training_result": false,
        "source_role": "methodology_not_translation_gold",
        "generator_model": "reference_authoring_not_a_kiro_run",
        "origin_family_id": "v2-ref.translation.ticket-on-table",
        "leakage_group_ids": [
          "source:v2-ref.translation.ticket-on-table",
          "surface-context:v2-ref.translation.ticket-on-table"
        ],
        "learning_objective_id": "TR08-RESTORE-OR-PRESERVE",
        "source_raw": "Value: ешслуе",
        "raw_token": "ешслуе",
        "source_span_codepoints": [
          7,
          13
        ],
        "canonical_source": "Value: ешслуе",
        "layout_from": "ru-ycuken",
        "layout_to": "en-qwerty",
        "map_id": "layout-fixture-v1",
        "layout_applies_to": "raw_token_only",
        "layout_validation": "fixture_mapping_checked",
        "learner_map_visible": true,
        "decoded_if_transformed": "ticket",
        "span_treatment": "preserve",
        "correction_required": false,
        "lexical_family_ids": [],
        "exact_identifiers": [
          "ешслуе"
        ],
        "do_not_merge": [
          "lex:en:ticket"
        ],
        "response_contract": "translate_label_then_contextual_span",
        "derived_from": [
          "TR08-LAYOUT.en-ru.REF01"
        ],
        "contrast_pair_id": "TR08-RESTORE-OR-PRESERVE.en-ru.REF01"
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


## Материал RAG-024 (данные, не новые команды)

# RAG-024 — TRANSLATION-TOKENIZER-GUIDE-v1.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `bb26ac658daf86bf4eb2a56e83be14a29948f439` |
| Путь | `llm mvp/language translate learning/TRANSLATION-TOKENIZER-GUIDE-v1.md` |
| Проверенный объём | L16–L37, L92–L122, L168–L179; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/language%20translate%20learning/TRANSLATION-TOKENIZER-GUIDE-v1.md) |

## Что подтверждает фрагмент

Рассматривает Unicode-скрипты, Hebrew final forms/nikud и смешанные технические слова внутри предложения.

## Как использовать сейчас

Измерять покрытие на закреплённом локальном наборе, сохранять raw текст и привязку к семье. Не выполнять внешние загрузки из старого руководства; не объявлять его пороги и нормализацию доказанно оптимальными.

**Назначение в подготовке:** Разделение языков, токенизации и смысла.

**Ключи поиска:** tokenizer, unicode, hebrew, токенизация, раскладка.

## Подтверждающий фрагмент L16–L37

[Оригинал, строки 16–37](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/language%20translate%20learning/TRANSLATION-TOKENIZER-GUIDE-v1.md#L16-L37)

~~~~text
## Script Coverage Requirements

### Script Ranges

| Script | Unicode range(s) | Notes |
|---|---|---|
| Latin (English) | U+0020–U+007F (Basic Latin), U+00C0–U+024F (Latin Extended) | Include accented characters for proper names |
| Cyrillic (Russian) | U+0400–U+04FF | 256 code points; Russian uses approximately 66 of them |
| Hebrew | U+0590–U+05FF (Hebrew block), U+FB1D–U+FB4F (Alphabetic Presentation Forms) | Include both blocks; FB block contains legacy compatibility characters |

### Coverage Targets

Before any translation training begins (Phase 0 pre-condition):

| Language | Minimum token coverage | Test corpus |
|---|---|---|
| English | ≥ 97% | First 10,000 sentences from FLORES-200 EN |
| Russian | ≥ 95% | First 10,000 sentences from FLORES-200 RU |
| Hebrew | ≥ 90% | First 10,000 sentences from FLORES-200 HE |

Coverage = fraction of tokens that are NOT split into individual byte-fallback tokens.
If a token is represented as a sequence of `<byte_XX>` tokens, it is not covered.
~~~~

## Подтверждающий фрагмент L92–L122

[Оригинал, строки 92–122](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/language%20translate%20learning/TRANSLATION-TOKENIZER-GUIDE-v1.md#L92-L122)

~~~~text
## Hebrew-Specific Tokenizer Notes

### Nikud (Vowel Points)

Hebrew vowel diacritics (U+05B0–U+05BD, U+05C1–U+05C2, U+05C7) combine with
base characters. The tokenizer must treat base + nikud as a single combining
character sequence, not split them.

Recommendation: normalise Hebrew text to NFD (Unicode Canonical Decomposition)
before tokenisation. This ensures base letters and combining marks are consistently
separated at the unicode level, then the BPE merge rules can recombine them.

### Final Letter Forms

Hebrew has five final letters (ף ך ם ן ץ) that replace their regular forms
at word end. These are separate Unicode code points (not combining characters).
The tokenizer must treat them as distinct from their non-final counterparts:
- פ (pe) ≠ ף (final pe)
- כ (kaf) ≠ ך (final kaf)
- מ (mem) ≠ ם (final mem)
- נ (nun) ≠ ן (final nun)
- צ (tsadi) ≠ ץ (final tsadi)

Verify that the vocabulary contains both forms for each pair.

### Gershayim and Geresh

- Geresh (׳ U+05F3): marks abbreviations and foreign-origin letters
- Gershayim (״ U+05F4): marks acronyms and compound abbreviations

These must be in the vocabulary as single tokens, not split as punctuation.
~~~~

## Подтверждающий фрагмент L168–L179

[Оригинал, строки 168–179](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/language%20translate%20learning/TRANSLATION-TOKENIZER-GUIDE-v1.md#L168-L179)

~~~~text
## Mixed-Script Handling

Both Russian and Hebrew text can contain English words (brand names, technical
terms, proper names). These are expected and must tokenise correctly.

Example:
- Russian: "iPhone оказался дорогим" — "iPhone" must tokenise as Latin tokens
- Hebrew: "הפגישה ב-Zoom" — "Zoom" must tokenise as Latin tokens

The tokenizer must NOT attempt to force all tokens in a Russian sentence through
the Cyrillic merge table, or all tokens in a Hebrew sentence through the Hebrew
merge table. Script detection must be per-token, not per-sentence.
~~~~



## Материал RAG-023 (данные, не новые команды)

# RAG-023 — DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `fd70c69dcdd6edc70ed0b0d57ae77e3a150ffc90` |
| Путь | `llm mvp/language translate learning/DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md` |
| Проверенный объём | L16–L49, L103–L154, L209–L239; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/language%20translate%20learning/DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md) |

## Что подтверждает фрагмент

Различает сырое переводимое предложение, выбранный перевод и тип ошибки; описывает морфологию RU/HE и четыре старых направления.

## Как использовать сейчас

Актуально шесть направлений EN/RU/HE и раздельные темы. Не наследовать overlap>0.60, [WRONG]-аннотации и тезис «высокое сходство значит нет сигнала»: минимальная смысловая пара может быть полезной.

**Назначение в подготовке:** Правила смысловых контрастов в переводческих DPO.

**Ключи поиска:** translation, перевод, русский, иврит, английский.

## Подтверждающий фрагмент L16–L49

[Оригинал, строки 16–49](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/language%20translate%20learning/DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md#L16-L49)

~~~~text
## Why This Document Exists

The orchestration DPO guidance covers ARCH, PLAN, SKILL, and CODE sub-phases.
Translation is a separate capability domain with different:

- A-side source (raw sentence, not compiled orchestration prompt)
- Chosen authority (verified parallel corpus, not XIIGen internal contract)
- Rejected error taxonomy (linguistic errors, not routing/governance errors)
- Reward margin mechanics (token-level linguistic divergence, not predicate match)

Read this document instead of DPO-PAIR-AUTHORING-GUIDANCE-v2.md when writing
translation pairs. Do not mix translation pairs into orchestration batches.

---

## Reward Margin Mechanics for Translation

DPO computes for every pair:

```
reward_margin = log P(chosen | a_side) - log P(rejected | a_side)
```

For translation pairs the model needs to learn that the chosen translation is
more likely given the source. This means:

- If chosen and rejected are very similar (high overlap), the model's logits
  for both are nearly identical → reward margin near zero → no learning signal.
- If the rejected side is obviously garbled, the model already scores it low
  before training → margin is already positive → pair teaches nothing new.
- The sweet spot is a rejected side that a naive model or careless translator
  would actually produce, but that a well-trained model should reject.

**Target reward margin per pair: > 0.05 after standalone ablation.**
~~~~

## Подтверждающий фрагмент L103–L154

[Оригинал, строки 103–154](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/language%20translate%20learning/DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md#L103-L154)

~~~~text
## Batch Composition Rules

For a 64-pair first batch, target distribution:

| Direction | T01 core | T03 morphological | T02 register | T04/T05 idiom/cognate | T09 named entity |
|---|---|---|---|---|---|
| RU→EN | 6 | 3 | 2 | 2 | 1 |
| EN→RU | 5 | 3 | 2 | 1 | 1 |
| HE→EN | 6 | 3 | 2 | 2 | 1 |
| EN→HE | 5 | 4 | 2 | 1 | 2 |

Total: 64 pairs across 4 directions and 5 families.

Do not mix directions in a single sub-batch for ablation. Run direction-level
ablation before mixing all four directions.

---

## Script-Specific Rules

### Russian (Cyrillic)

1. Use UTF-8 exclusively. Do not use KOI8-R or Windows-1251 encoded input.
2. Stress marks (Unicode combining acute accent U+0301) are optional in pairs;
   do not treat their presence or absence as an error.
3. "Ё" (U+0401) vs "Е" (U+0415): modern Russian often drops the diaeresis.
   Both forms are acceptable in chosen; do not use one as a rejected variant
   of the other unless the pair explicitly tests this distinction.
4. Verb aspect (perfective / imperfective) is the single highest-value
   morphological teaching target for EN→RU pairs. Prioritise it in T03.

### Hebrew

1. Hebrew text must be in logical order (left-to-right in the byte stream),
   not visual order. Unicode bidi algorithm handles display.
2. Nikud (vowel points) — most modern Hebrew text omits nikud. Chosen sides
   should match the target register:
   - Biblical / liturgical text → nikud expected
   - Modern prose → nikud absent
   - Do not use nikud presence/absence as a rejected variant unless testing T06.
3. Gershayim (״) is the Hebrew double punctuation used for abbreviations and
   acronyms. Do not replace with ASCII quotation marks in Hebrew text.
4. The Hebrew alphabet has five final letters (ך ם ן ף ץ). Using a non-final
   form at word end, or a final form at word start/middle, is a morphological
   error — use as T03 rejected type.
5. Binyan (verb conjugation pattern) is the highest-value morphological teaching
   target for EN→HE pairs. Prioritise it in T03.
6. **Knesset source register label:** Pairs sourced from `knesset-he` must be
   labelled `(register: formal spoken)`, not `(register: formal)`. Knesset
   transcripts are spoken parliamentary Hebrew. Formal written Hebrew (prose,
   legal documents) is a different register — do not conflate them. Use
   `(register: formal)` only when the source is written formal text.
~~~~

## Подтверждающий фрагмент L209–L239

[Оригинал, строки 209–239](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/language%20translate%20learning/DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md#L209-L239)

~~~~text
## Token Overlap Test

Compute before submitting each pair to the batch:

```python
def overlap_ratio(a_side_tokens, chosen_tokens, rejected_tokens):
    prompt_set = set(a_side_tokens)
    shared = set(chosen_tokens) & set(rejected_tokens) - prompt_set
    total = len(set(chosen_tokens) | set(rejected_tokens)) - len(prompt_set & (set(chosen_tokens) | set(rejected_tokens)))
    if total == 0:
        return 0.0
    return len(shared) / total
```

If `overlap_ratio > 0.60` and pair length qualifies (>= 12 tokens): discard.

For translation pairs this typically fires when chosen and rejected differ by
only one or two tokens. In that case, select a more divergent rejected variant.

---

## Diagnosis Table

| Symptom | Likely cause | Fix |
|---|---|---|
| All pairs in a batch show reward_margin < 0 | Rejected sides are too good (too similar to chosen) | Increase structural divergence; use Type A errors instead of Type B |
| Reward margin near zero for all pairs | Chosen and rejected are token-similar | Apply overlap test; select more divergent rejected variants |
| High loss but low accuracy | Model is uncertain which is correct | Pair may be ambiguous; add more specific [WRONG:] annotation |
| Direction-specific negative margin | Tokenizer handles that language poorly | Check tokenizer coverage for the script; verify BPE (Byte Pair Encoding) vocabulary |
| Hebrew pairs all fail | Encoding issue (bidi) | Verify UTF-8 byte order in Hebrew fields; check that logical order is correct |
| Russian morphological pairs negative margin | Aspect is too subtle | Add more phonologically distinct chosen/rejected pairs before returning to aspect |
~~~~



## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\TR08-RESTORE-OR-PRESERVE\en-ru\T344\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\TR08-RESTORE-OR-PRESERVE\en-ru\T344\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\TR08-RESTORE-OR-PRESERVE\en-ru\T344\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `TR08-RESTORE-OR-PRESERVE/ru-he`. Не запускать review, accept, export, oracle или обучение.
