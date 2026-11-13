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
  "stream_id": "TR02-COREFERENCE/ru-he",
  "topic_id": "TR02",
  "objective_id": "TR02-COREFERENCE",
  "domain_id": "tr.he",
  "profile_id": "ru-he",
  "run_id": "T393",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/tr/TR02-COREFERENCE/F0000013",
    "v3/tr/TR02-COREFERENCE/F0000015",
    "v3/tr/TR02-COREFERENCE/F0000017",
    "v3/tr/TR02-COREFERENCE/F0000018",
    "v3/tr/TR02-COREFERENCE/F0000019",
    "v3/tr/TR02-COREFERENCE/F0000020",
    "v3/tr/TR02-COREFERENCE/F0000022",
    "v3/tr/TR02-COREFERENCE/F0000023",
    "v3/tr/TR02-COREFERENCE/F0000024",
    "v3/tr/TR02-COREFERENCE/F0000026"
  ],
  "family_contracts": {
    "v3/tr/TR02-COREFERENCE/F0000013": {
      "origin_family_id": "v3/tr/TR02-COREFERENCE/F0000013",
      "leakage_group_ids": [
        "v3/tr/TR02-COREFERENCE/F0000013"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR02-COREFERENCE/F0000015": {
      "origin_family_id": "v3/tr/TR02-COREFERENCE/F0000015",
      "leakage_group_ids": [
        "v3/tr/TR02-COREFERENCE/F0000015"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR02-COREFERENCE/F0000017": {
      "origin_family_id": "v3/tr/TR02-COREFERENCE/F0000017",
      "leakage_group_ids": [
        "v3/tr/TR02-COREFERENCE/F0000017"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR02-COREFERENCE/F0000018": {
      "origin_family_id": "v3/tr/TR02-COREFERENCE/F0000018",
      "leakage_group_ids": [
        "v3/tr/TR02-COREFERENCE/F0000018"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR02-COREFERENCE/F0000019": {
      "origin_family_id": "v3/tr/TR02-COREFERENCE/F0000019",
      "leakage_group_ids": [
        "v3/tr/TR02-COREFERENCE/F0000019"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR02-COREFERENCE/F0000020": {
      "origin_family_id": "v3/tr/TR02-COREFERENCE/F0000020",
      "leakage_group_ids": [
        "v3/tr/TR02-COREFERENCE/F0000020"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR02-COREFERENCE/F0000022": {
      "origin_family_id": "v3/tr/TR02-COREFERENCE/F0000022",
      "leakage_group_ids": [
        "v3/tr/TR02-COREFERENCE/F0000022"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR02-COREFERENCE/F0000023": {
      "origin_family_id": "v3/tr/TR02-COREFERENCE/F0000023",
      "leakage_group_ids": [
        "v3/tr/TR02-COREFERENCE/F0000023"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR02-COREFERENCE/F0000024": {
      "origin_family_id": "v3/tr/TR02-COREFERENCE/F0000024",
      "leakage_group_ids": [
        "v3/tr/TR02-COREFERENCE/F0000024"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR02-COREFERENCE/F0000026": {
      "origin_family_id": "v3/tr/TR02-COREFERENCE/F0000026",
      "leakage_group_ids": [
        "v3/tr/TR02-COREFERENCE/F0000026"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 27,
  "sources": [
    "RAG-023",
    "RAG-043"
  ],
  "card": "curriculum/cards/TR02-COREFERENCE.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR02-COREFERENCE\\ru-he\\T393",
  "mode": "CREATE_ONLY",
  "next_stream_id": "TR02-COREFERENCE/he-ru",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\TR02-COREFERENCE.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR02-COREFERENCE\\ru-he\\T393\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR02-COREFERENCE\\ru-he\\T393\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR02-COREFERENCE\\ru-he\\T393\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR02-COREFERENCE\\ru-he\\T393\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR02-COREFERENCE\\ru-he\\T393\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR02-COREFERENCE\\ru-he\\T393\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-023": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-023.md",
      "RAG-043": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-043.md"
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
  "objective_id": "TR02-COREFERENCE",
  "topic_id": "TR02",
  "title_ru": "Референт местоимения в соседнем предложении",
  "learning_goal_ru": "Переводить короткий связный текст с верным референтом местоимения, используя только явно достаточный видимый контекст.",
  "generation_prompt_ru": "Это отдельный навык TR02: сохранение участника действия между предложениями. Не повторяй одиночные agent/patient-перестановки TR02-ROLE-ORDER и не делай целью угадывание пола по имени из TR04. В каждой новой сцене ровно два названных участника одного явно указанного рода; третьи неназванные лица исключены видимым условием. Дай две короткие фразы, во второй есть местоимение. Добавь естественный контекст, который однозначно определяет, кто совершил второе действие; нужное основание должно быть в learner prompt, а не только в metadata. Если два чтения всё ещё возможны, не выбирай удобное: верни случай на переработку или в TR10-CONDITIONAL с сохранением lineage.\n\nВидимый контракт требует перевести обе фразы и заменить местоимение во второй фразе именем соответствующего участника. Это диагностический перевод с разрешением референта; обычный перевод с тем же неоднозначным she/она/היא здесь не измеряет этот навык. Chosen и rejected имеют одинаковую естественную структуру; различаются только именем референта второго действия. Оба участника имеют одинаковый род, чтобы разницу нельзя было распознать только по he/she или форме глагола. Не меняй первое действие, время, предмет и отрицание. Не добавляй пояснение правильности в ответ.\n\nВ каждой партии из 12 строк зафиксируй четыре клетки grammatical_gender × coreference_class: по 3 feminine/first_mention, feminine/second_mention, masculine/first_mention и masculine/second_mention. Во всех клетках разные независимые сцены, разные имена, разные события и несколько форм достаточного контекста. Не назначай «женский род → первый участник» или «мужской → второй». Публичные контрфактические варианты ниже показывают переворот ответа при прежних участниках и исходных предложениях; они имеют прежний family/origin/split, а не объявляются независимыми. Грамматический род здесь проверяет состав корпуса и сам по себе не решает задачу. Это классы одного навыка, а не разные темы. Чередуй формы достаточного контекста: кто остался в нужном месте, кто продолжил ранее описанное действие, чьё явно заданное намерение реализовано; не используй одну фразу-подсказку во всех chosen. Каждая сгенерированная строка получает один самостоятельный сценарий (pairs_per_family=1). Если строятся контрфактуальные варианты одной сцены, они сохраняют family/origin/split и идут в отдельную явно назначенную revision, а не получают выдуманную независимость. Примеры ниже показывают оба класса для обоих полов и по шесть направлений каждого варианта; это две исходные семьи и 24 строки, а не 24 независимые семьи.\n\nВ metadata сохрани coreference_class, anaphor_surface, antecedent_name, competing_antecedent, context_evidence и sentence_index. context_evidence должен буквально встречаться в learner prompt. Для токенов допустимы lemma, grammatical_gender, syntactic_role, semantic_role и позиции raw; неизвестное null. Эти аннотации не считаются работающим механизмом kNN/LoRA до проверки конкретного consumer. При последующем обучении измеряй правильный перевод/референт на независимых сценах, а не количество заполненных аннотаций.\n\nРабота выполняется по одному request.json: один objective_id, topic_id, profile_id и split. Используй текущую китайскую модель, уже выбранную пользователем в Kiro; не меняй модель и не выдумывай её название. Помощники наследуют ту же модель. Фактически наблюдаемое имя/селектор сохраняется в provenance. Читай 02-DATA-CONTRACT.md, 04-QUALITY-AND-SPLITS.md, 06-SOURCES-AND-SKILLS.md, эту карточку и её RAG-источники. Скилы: intent-to-plan, plan-self-validation, human-readable-architecture-audit, code-examination, prompt-patch-authoring, human-communication; точные закреплённые локаторы находятся в 06-SOURCES-AND-SKILLS.md. Отчёт и объяснения — по-русски; learner-ответы — на целевом языке профиля.\n\nПиши draft.jsonl и отчёт генерации в папку, созданную prepare. Текущий исполнитель создаёт материал по назначенной теме; после записи и структурного finish следует следующий поток, без review. Результат этой задачи — подготовка кандидатов данных; она не запускает обучение и не меняет production-код. Рationale, аннотации ролей/рода, метки дефектов и ID не добавляются в learner prompt или ответы. Источники используются как методика; новые языковые примеры — авторские кандидатные симуляции; предметная проверка только отдельным будущим поручением, не цитаты из корпуса и не результаты обучения. Не скачивай внешние сайты из старых ссылок.\n\nОбщий исходный сценарий, его переводы, перефразирования, опечатки и неверные раскладки сохраняют один family/origin/split. Публичные reference-примеры и их производные не переносятся в закрытый DEV/EXAM. Прежние эксперименты, файлы и результаты не удалять; изменённый материал получает новую revision/run с derived_from. При прямом указании «следующий» немедленно перейти к указанному ID: запрещено продолжать предыдущую задачу, дописывать её отчёт, чинить её tests или повторять её запуск. При «стоп» немедленно прекратить действия.\n\nПри отдельном разрешении на учебный pilot сначала восстановить совместимое уже обученное состояние B1 по 05-LEARNING-AND-CHECKPOINTS.md. Если такого состояния нет, один раз получить непустое обученное состояние на выделенном TRAIN, сохранить его вместе с tokenizer/template/model/optimizer/RNG и lineage; не обучать заново для каждого сравнения. Запускать один процесс, сохранить его handle и использовать штатное ожидание до terminal state; не завершать ход вместо ожидания и не заменять ожидание повторными стартами/частым polling. Составить бюджет, достаточный для появления правильных ответов. 0→0 не является валидным учебным результатом; изменение margin само по себе этого не исправляет. Любое измеренное улучшение полезной метрики признаётся успехом по этой метрике и домену; показать рядом остальные метрики и ухудшения. Это не автоматическое разрешение интеграции.",
  "response_contract_ru": "Только перевод обеих исходных фраз. Местоимение во второй фразе заменить именем участника, однозначно заданного видимым контекстом; контекст и объяснение не переводить.",
  "scenario_axes": [
    {
      "axis": "coreference_class",
      "values": [
        "first_mention",
        "second_mention"
      ]
    },
    {
      "axis": "context_basis",
      "values": [
        "unique_remaining_participant",
        "explicit_prior_intention",
        "uniquely_continued_action"
      ]
    },
    {
      "axis": "same_gender_participants",
      "values": [
        "two_women",
        "two_men"
      ]
    }
  ],
  "forbidden_mixing": [
    "Неоднозначные местоимения, где нужный референт существует только в metadata.",
    "Подмена референтной связи угадыванием пола по имени или профессии.",
    "Искусственная перестановка сразу нескольких ролей, событий или времён.",
    "Вывод «модель выучила синтаксис» из наличия role-аннотаций без проверки обучаемого consumer.",
    "Другие objective_id, разные профили или разные split в одной партии.",
    "Публичные reference-примеры и их производные в скрытом DEV/EXAM."
  ],
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "TR02-COREFERENCE.ru-he.REF01",
      "profile_id": "ru-he",
      "family_id": "v4-ref.translation.two-women-door",
      "prompt": "Переведи с языка «русский» на язык «иврит». Переведи обе исходные фразы; во второй замени местоимение именем участника, которого определяет контекст. Выдай только перевод исходного текста, без перевода контекста.\nКонтекст: В сцене участвуют только две женщины, Дина и Вера. После разговора Вера уже ушла на улицу и больше не прикасалась к двери. Во втором предложении дверь закрыла единственная женщина, оставшаяся внутри.\nИсходный текст: Дина поговорила с Верой. Затем она закрыла дверь.",
      "chosen": "דינה שוחחה עם ורה. אחר כך דינה סגרה את הדלת.",
      "rejected": "דינה שוחחה עם ורה. אחר כך ורה סגרה את הדלת.",
      "error_tag": "wrong_coreference",
      "rationale": "Во втором действии Вера уже отсутствует и не касается двери; внутри остаётся Дина. Rejected приписывает закрывание Вере, сохраняя первое действие и грамматический род.",
      "counterexample": "Без явно достаточного контекста оба референта могут быть возможны; тогда нельзя объявлять один отрицательным. Само по себе имя первого или последнего участника не решает задачу.",
      "source_refs": [
        "RAG-023",
        "RAG-043"
      ],
      "metadata": {
        "response_language": "he",
        "language_pair": {
          "source": "ru",
          "target": "he"
        },
        "reference_only": true,
        "split": "reference",
        "simulation": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "measured_training_result": false,
        "source_role": "methodology_not_translation_gold",
        "generator_model": "reference_authoring_not_a_kiro_run",
        "origin_family_id": "v4-ref.translation.two-women-door",
        "source_raw": "Дина поговорила с Верой. Затем она закрыла дверь.",
        "response_contract": "translation_with_named_referent",
        "leakage_group_ids": [
          "source:v4-ref.translation.two-women-door"
        ],
        "learning_objective_id": "TR02-COREFERENCE",
        "context_evidence": "Вера уже ушла на улицу и больше не прикасалась к двери",
        "annotation_consumer_verified": false,
        "coreference_class": "first_mention",
        "anaphor_surface": "она",
        "antecedent_name": "Dina",
        "competing_antecedent": "Vera",
        "grammatical_gender": "feminine",
        "sentence_index": 2
      }
    },
    {
      "example_id": "TR02-COREFERENCE.ru-he.REF02",
      "profile_id": "ru-he",
      "family_id": "v4-ref.translation.two-men-map",
      "prompt": "Переведи с языка «русский» на язык «иврит». Переведи обе исходные фразы; во второй замени местоимение именем участника, которого определяет контекст. Выдай только перевод исходного текста, без перевода контекста.\nКонтекст: В сцене участвуют только двое мужчин, Павел и Олег. Сразу после разговора Павел ушёл за водой. Единственным оставшимся у стола был Олег. Второе предложение описывает действие за этим столом уже после ухода Павла.\nИсходный текст: Павел обсудил маршрут с Олегом. Затем он развернул карту.",
      "chosen": "פאבל דן במסלול עם אולג. אחר כך אולג פרש את המפה.",
      "rejected": "פאבל דן במסלול עם אולג. אחר כך פאבל פרש את המפה.",
      "error_tag": "wrong_coreference",
      "rationale": "Действие с картой происходит за столом после ухода Павла; там остался только Олег. Rejected ошибочно сохраняет первым названного человека как исполнителя второго действия.",
      "counterexample": "Без явно достаточного контекста оба референта могут быть возможны; тогда нельзя объявлять один отрицательным. Само по себе имя первого или последнего участника не решает задачу.",
      "source_refs": [
        "RAG-023",
        "RAG-043"
      ],
      "metadata": {
        "response_language": "he",
        "language_pair": {
          "source": "ru",
          "target": "he"
        },
        "reference_only": true,
        "split": "reference",
        "simulation": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "measured_training_result": false,
        "source_role": "methodology_not_translation_gold",
        "generator_model": "reference_authoring_not_a_kiro_run",
        "origin_family_id": "v4-ref.translation.two-men-map",
        "source_raw": "Павел обсудил маршрут с Олегом. Затем он развернул карту.",
        "response_contract": "translation_with_named_referent",
        "leakage_group_ids": [
          "source:v4-ref.translation.two-men-map"
        ],
        "learning_objective_id": "TR02-COREFERENCE",
        "context_evidence": "Единственным оставшимся у стола был Олег",
        "annotation_consumer_verified": false,
        "coreference_class": "second_mention",
        "anaphor_surface": "он",
        "antecedent_name": "Oleg",
        "competing_antecedent": "Pavel",
        "grammatical_gender": "masculine",
        "sentence_index": 2
      }
    },
    {
      "example_id": "TR02-COREFERENCE.ru-he.REF03",
      "profile_id": "ru-he",
      "family_id": "v4-ref.translation.two-women-door",
      "prompt": "Переведи с языка «русский» на язык «иврит». Переведи обе исходные фразы; во второй замени местоимение именем участника, которого определяет контекст. Выдай только перевод исходного текста, без перевода контекста.\nКонтекст: В сцене участвуют только две женщины, Дина и Вера. После разговора Дина уже ушла на улицу и больше не прикасалась к двери. Во втором предложении дверь закрыла единственная женщина, оставшаяся внутри.\nИсходный текст: Дина поговорила с Верой. Затем она закрыла дверь.",
      "chosen": "דינה שוחחה עם ורה. אחר כך ורה סגרה את הדלת.",
      "rejected": "דינה שוחחה עם ורה. אחר כך דינה סגרה את הדלת.",
      "error_tag": "wrong_coreference",
      "rationale": "В этой контрфактической версии ушла Дина; внутри осталась Вера. Грамматический род, порядок участников и исходные предложения прежние, но верный референт второй фразы меняется.",
      "counterexample": "Без явно достаточного контекста оба референта могут быть возможны; тогда нельзя объявлять один отрицательным. Само по себе имя первого или последнего участника не решает задачу.",
      "source_refs": [
        "RAG-023",
        "RAG-043"
      ],
      "metadata": {
        "response_language": "he",
        "language_pair": {
          "source": "ru",
          "target": "he"
        },
        "reference_only": true,
        "split": "reference",
        "simulation": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "measured_training_result": false,
        "source_role": "methodology_not_translation_gold",
        "generator_model": "reference_authoring_not_a_kiro_run",
        "origin_family_id": "v4-ref.translation.two-women-door",
        "source_raw": "Дина поговорила с Верой. Затем она закрыла дверь.",
        "response_contract": "translation_with_named_referent",
        "leakage_group_ids": [
          "source:v4-ref.translation.two-women-door"
        ],
        "learning_objective_id": "TR02-COREFERENCE",
        "context_evidence": "Дина уже ушла на улицу и больше не прикасалась к двери",
        "annotation_consumer_verified": false,
        "coreference_class": "second_mention",
        "anaphor_surface": "она",
        "antecedent_name": "Vera",
        "competing_antecedent": "Dina",
        "grammatical_gender": "feminine",
        "sentence_index": 2,
        "counterfactual_of": "TR02-COREFERENCE.ru-he.REF01",
        "derived_from": "TR02-COREFERENCE.ru-he.REF01"
      }
    },
    {
      "example_id": "TR02-COREFERENCE.ru-he.REF04",
      "profile_id": "ru-he",
      "family_id": "v4-ref.translation.two-men-map",
      "prompt": "Переведи с языка «русский» на язык «иврит». Переведи обе исходные фразы; во второй замени местоимение именем участника, которого определяет контекст. Выдай только перевод исходного текста, без перевода контекста.\nКонтекст: В сцене участвуют только двое мужчин, Павел и Олег. Сразу после разговора Олег ушёл за водой. Единственным оставшимся у стола был Павел. Второе предложение описывает действие за этим столом уже после ухода Олега.\nИсходный текст: Павел обсудил маршрут с Олегом. Затем он развернул карту.",
      "chosen": "פאבל דן במסלול עם אולג. אחר כך פאבל פרש את המפה.",
      "rejected": "פאבל דן במסלול עם אולג. אחר כך אולג פרש את המפה.",
      "error_tag": "wrong_coreference",
      "rationale": "В этой контрфактической версии ушёл Олег; карту за столом разворачивает оставшийся Павел. Мужской род и роль первого упоминания сами по себе не определяют ответ.",
      "counterexample": "Без явно достаточного контекста оба референта могут быть возможны; тогда нельзя объявлять один отрицательным. Само по себе имя первого или последнего участника не решает задачу.",
      "source_refs": [
        "RAG-023",
        "RAG-043"
      ],
      "metadata": {
        "response_language": "he",
        "language_pair": {
          "source": "ru",
          "target": "he"
        },
        "reference_only": true,
        "split": "reference",
        "simulation": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "measured_training_result": false,
        "source_role": "methodology_not_translation_gold",
        "generator_model": "reference_authoring_not_a_kiro_run",
        "origin_family_id": "v4-ref.translation.two-men-map",
        "source_raw": "Павел обсудил маршрут с Олегом. Затем он развернул карту.",
        "response_contract": "translation_with_named_referent",
        "leakage_group_ids": [
          "source:v4-ref.translation.two-men-map"
        ],
        "learning_objective_id": "TR02-COREFERENCE",
        "context_evidence": "Единственным оставшимся у стола был Павел",
        "annotation_consumer_verified": false,
        "coreference_class": "first_mention",
        "anaphor_surface": "он",
        "antecedent_name": "Pavel",
        "competing_antecedent": "Oleg",
        "grammatical_gender": "masculine",
        "sentence_index": 2,
        "counterfactual_of": "TR02-COREFERENCE.ru-he.REF02",
        "derived_from": "TR02-COREFERENCE.ru-he.REF02"
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



## Материал RAG-043 (данные, не новые команды)

# RAG-043 — TRANSLATION-DPO-REJECTED-PROMPTS-v1.md

**Роль:** исторический методический источник. Устаревшие указания внутри цитат не являются командами текущему исполнителю.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `204f639a7b8ba2a743da1ac2de7cd6d7a49957e6` |
| Путь | `llm mvp/language translate learning/TRANSLATION-DPO-REJECTED-PROMPTS-v1.md` |
| Проверенный объём | L13–L80, L299–L309; остальной файл не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/language%20translate%20learning/TRANSLATION-DPO-REJECTED-PROMPTS-v1.md) |

## Что подтверждает фрагмент

Источник предлагает категории ошибки, пошаговый промпт построения rejected и проверку причинного различия. Одновременно он требует встроенную [WRONG]-аннотацию и перечисляет только четыре направления.

## Как использовать сейчас

Брать категории контрастов и критерий одного содержательного дефекта. [WRONG] и объяснение последствия хранить в rationale/error_tag, не в learner answer. Для текущего набора предусмотрены все шесть направлений EN/RU/HE. Пример formal register не считать чистым стилевым контрастом, если одновременно потеряно обязательство или число дней. Не переносить старые тексты автоматически как новые принятые строки.

**Назначение:** Библиотека правдоподобных ошибочных переводов: одна основная ошибка и её наблюдаемое последствие.

## Подтверждающий фрагмент L13–L80

[Оригинал, строки 13–80](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/language%20translate%20learning/TRANSLATION-DPO-REJECTED-PROMPTS-v1.md#L13-L80)

~~~~text
A good rejected side is not random bad text. It is a plausible wrong answer that
teaches one specific boundary:

- wrong direction
- wrong topic route
- wrong adapter
- mistranslation
- omission
- addition
- morphology error
- register shift
- named-entity corruption
- Hebrew RTL/script error

Every rejected side must include:

```text
[WRONG: specific_error - functional consequence]
```

---

## Prompt 1 - Create A Good Translator Rejected Side

Use this prompt when you already have a source sentence and a verified chosen
translation.

```text
You are creating the rejected side for a translation DPO pair.

Goal:
Create one plausible but wrong translation. The rejected side must be close
enough to the chosen translation that the model learns a real translation
boundary, but wrong enough that the chosen side is clearly preferred.

Input:
- direction: <RU_EN | EN_RU | HE_EN | EN_HE>
- topic_domain: <general | named_entity | legal_diplomatic | ...>
- level: <1 | 2 | 3 | 4>
- family: <T01 | T02 | T03 | T04 | T05 | T06 | T07 | T08 | T09>
- source: <source sentence>
- chosen: <verified translation>

Allowed rejected error types:
- wrong_direction
- copied_source
- mistranslation
- omission
- addition
- negation_error
- morphology_error
- register_shift
- idiom_calque
- false_cognate
- named_entity_corruption
- rtl_or_script_error

Instructions:
1. Produce exactly one rejected translation.
2. Make it fluent enough to be plausible.
3. Introduce exactly one primary error type.
4. Do not make it random, broken, or obviously garbage.
5. Do not copy the chosen with only punctuation changes.
6. Do not include explanation outside the rejected text.
7. Append one [WRONG: ...] annotation that names the error and the functional consequence.

Output format:
rejected: "<wrong translation>\n[WRONG: <error_type> - <what the model cannot learn from this rejected side and what failure it would cause>]"
~~~~
## Подтверждающий фрагмент L299–L309

[Оригинал, строки 299–309](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/language%20translate%20learning/TRANSLATION-DPO-REJECTED-PROMPTS-v1.md#L299-L309)

~~~~text
## Minimal Quality Rule

A rejected side is good when a reviewer can complete this sentence:

```text
The chosen side is better because the rejected side makes exactly this mistake:
<specific mistake>, and if the model learned the rejected side it would fail by:
<functional consequence>.
```

If that sentence cannot be completed, repair or reject the pair.
~~~~



## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\TR02-COREFERENCE\ru-he\T393\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\TR02-COREFERENCE\ru-he\T393\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\TR02-COREFERENCE\ru-he\T393\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `TR02-COREFERENCE/he-ru`. Не запускать review, accept, export, oracle или обучение.
