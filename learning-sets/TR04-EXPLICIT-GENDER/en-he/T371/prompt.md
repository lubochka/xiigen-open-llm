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
  "stream_id": "TR04-EXPLICIT-GENDER/en-he",
  "topic_id": "TR04",
  "objective_id": "TR04-EXPLICIT-GENDER",
  "domain_id": "tr.he",
  "profile_id": "en-he",
  "run_id": "T371",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/tr/TR04-EXPLICIT-GENDER/F0000013",
    "v3/tr/TR04-EXPLICIT-GENDER/F0000014",
    "v3/tr/TR04-EXPLICIT-GENDER/F0000016",
    "v3/tr/TR04-EXPLICIT-GENDER/F0000017",
    "v3/tr/TR04-EXPLICIT-GENDER/F0000018",
    "v3/tr/TR04-EXPLICIT-GENDER/F0000019",
    "v3/tr/TR04-EXPLICIT-GENDER/F0000020",
    "v3/tr/TR04-EXPLICIT-GENDER/F0000021",
    "v3/tr/TR04-EXPLICIT-GENDER/F0000022",
    "v3/tr/TR04-EXPLICIT-GENDER/F0000023"
  ],
  "family_contracts": {
    "v3/tr/TR04-EXPLICIT-GENDER/F0000013": {
      "origin_family_id": "v3/tr/TR04-EXPLICIT-GENDER/F0000013",
      "leakage_group_ids": [
        "v3/tr/TR04-EXPLICIT-GENDER/F0000013"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR04-EXPLICIT-GENDER/F0000014": {
      "origin_family_id": "v3/tr/TR04-EXPLICIT-GENDER/F0000014",
      "leakage_group_ids": [
        "v3/tr/TR04-EXPLICIT-GENDER/F0000014"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR04-EXPLICIT-GENDER/F0000016": {
      "origin_family_id": "v3/tr/TR04-EXPLICIT-GENDER/F0000016",
      "leakage_group_ids": [
        "v3/tr/TR04-EXPLICIT-GENDER/F0000016"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR04-EXPLICIT-GENDER/F0000017": {
      "origin_family_id": "v3/tr/TR04-EXPLICIT-GENDER/F0000017",
      "leakage_group_ids": [
        "v3/tr/TR04-EXPLICIT-GENDER/F0000017"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR04-EXPLICIT-GENDER/F0000018": {
      "origin_family_id": "v3/tr/TR04-EXPLICIT-GENDER/F0000018",
      "leakage_group_ids": [
        "v3/tr/TR04-EXPLICIT-GENDER/F0000018"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR04-EXPLICIT-GENDER/F0000019": {
      "origin_family_id": "v3/tr/TR04-EXPLICIT-GENDER/F0000019",
      "leakage_group_ids": [
        "v3/tr/TR04-EXPLICIT-GENDER/F0000019"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR04-EXPLICIT-GENDER/F0000020": {
      "origin_family_id": "v3/tr/TR04-EXPLICIT-GENDER/F0000020",
      "leakage_group_ids": [
        "v3/tr/TR04-EXPLICIT-GENDER/F0000020"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR04-EXPLICIT-GENDER/F0000021": {
      "origin_family_id": "v3/tr/TR04-EXPLICIT-GENDER/F0000021",
      "leakage_group_ids": [
        "v3/tr/TR04-EXPLICIT-GENDER/F0000021"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR04-EXPLICIT-GENDER/F0000022": {
      "origin_family_id": "v3/tr/TR04-EXPLICIT-GENDER/F0000022",
      "leakage_group_ids": [
        "v3/tr/TR04-EXPLICIT-GENDER/F0000022"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR04-EXPLICIT-GENDER/F0000023": {
      "origin_family_id": "v3/tr/TR04-EXPLICIT-GENDER/F0000023",
      "leakage_group_ids": [
        "v3/tr/TR04-EXPLICIT-GENDER/F0000023"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 24,
  "sources": [
    "RAG-023",
    "RAG-024"
  ],
  "card": "curriculum/cards/TR04-EXPLICIT-GENDER.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR04-EXPLICIT-GENDER\\en-he\\T371",
  "mode": "CREATE_ONLY",
  "next_stream_id": "TR04-EXPLICIT-GENDER/he-en",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\TR04-EXPLICIT-GENDER.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR04-EXPLICIT-GENDER\\en-he\\T371\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR04-EXPLICIT-GENDER\\en-he\\T371\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR04-EXPLICIT-GENDER\\en-he\\T371\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR04-EXPLICIT-GENDER\\en-he\\T371\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR04-EXPLICIT-GENDER\\en-he\\T371\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR04-EXPLICIT-GENDER\\en-he\\T371\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-023": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-023.md",
      "RAG-024": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-024.md"
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
  "objective_id": "TR04-EXPLICIT-GENDER",
  "topic_id": "TR04",
  "title_ru": "Род участника, явно указанный в тексте",
  "learning_goal_ru": "Сохранять явно выраженный род участника и согласование, не угадывая его по профессии или имени.",
  "generation_prompt_ru": "Создай 10 новых коротких ситуаций с одним участником: 5 с явно женским и 5 с явно мужским референтом. Источник должен содержать местоимение либо однозначное согласование, а при необходимости — видимый контекст о том, к кому относится местоимение. Chosen сохраняет эту информацию; rejected заменяет её на другой род, оставляя число, время, событие и профессию прежними. Различай grammatical_gender и явно сообщённую информацию о референте: грамматический род неодушевлённого существительного не означает пол человека. Не делай вывод «программист — мужчина» или «медсестра — женщина» из стереотипа. Для английского source без заданного рода не изобретай женщину/мужчину: такой случай относится к карточке условного уточнения либо требует нейтрального перевода, если он естественно возможен. Не включай одновременное изменение singular/plural: число — отдельная микроцель. Сохраняй лемму, словоформу, род, число и роль в companion metadata; не сливай разные словоформы в один текст только потому, что у них одна лексическая семья.\n\nРабота выполняется по одному request.json: один objective_id, topic_id, profile_id и split. Используй текущую китайскую модель, уже выбранную пользователем в Kiro; не меняй модель и не выдумывай её название. Помощники наследуют ту же модель. Фактически наблюдаемое имя/селектор сохраняется в provenance. Читай 02-DATA-CONTRACT.md, 04-QUALITY-AND-SPLITS.md, 06-SOURCES-AND-SKILLS.md, эту карточку и её RAG-источники. Скилы: intent-to-plan, plan-self-validation, human-readable-architecture-audit, code-examination, prompt-patch-authoring, human-communication; точные закреплённые локаторы находятся в 06-SOURCES-AND-SKILLS.md. Отчёт и объяснения — по-русски; learner-ответы — на целевом языке профиля.\n\nПиши draft.jsonl и отчёт генерации в папку, созданную prepare. Текущий исполнитель создаёт материал по назначенной теме; после записи и структурного finish следует следующий поток, без review. Результат этой задачи — подготовка кандидатов данных; она не запускает обучение и не меняет production-код. Рationale, аннотации ролей/рода, метки дефектов и ID не добавляются в learner prompt или ответы. Источники используются как методика; новые языковые примеры — авторские кандидатные симуляции; предметная проверка только отдельным будущим поручением, не цитаты из корпуса и не результаты обучения. Не скачивай внешние сайты из старых ссылок.\n\nОбщий исходный сценарий, его переводы, перефразирования, опечатки и неверные раскладки сохраняют один family/origin/split. Публичные reference-примеры и их производные не переносятся в закрытый DEV/EXAM. Прежние эксперименты, файлы и результаты не удалять; изменённый материал получает новую revision/run с derived_from. При прямом указании «следующий» немедленно перейти к указанному ID: запрещено продолжать предыдущую задачу, дописывать её отчёт, чинить её tests или повторять её запуск. При «стоп» немедленно прекратить действия.\n\nПри отдельном разрешении на учебный pilot сначала восстановить совместимое уже обученное состояние B1 по 05-LEARNING-AND-CHECKPOINTS.md. Если такого состояния нет, один раз получить непустое обученное состояние на выделенном TRAIN, сохранить его вместе с tokenizer/template/model/optimizer/RNG и lineage; не обучать заново для каждого сравнения. Запускать один процесс, сохранить его handle и использовать штатное ожидание до terminal state; не завершать ход вместо ожидания и не заменять ожидание повторными стартами/частым polling. Составить бюджет, достаточный для появления правильных ответов. 0→0 не является валидным учебным результатом; изменение margin само по себе этого не исправляет. Любое измеренное улучшение полезной метрики признаётся успехом по этой метрике и домену; показать рядом остальные метрики и ухудшения. Это не автоматическое разрешение интеграции.",
  "response_contract_ru": "Только перевод исходного текста на целевой язык; метаданные и объяснение в ответ не входят.",
  "scenario_axes": [
    {
      "axis": "explicit_reference",
      "values": [
        "female",
        "male"
      ]
    },
    {
      "axis": "role",
      "values": [
        "engineer",
        "client",
        "speaker",
        "student"
      ]
    },
    {
      "axis": "evidence",
      "values": [
        "pronoun",
        "overt_agreement",
        "visible_context"
      ]
    }
  ],
  "forbidden_mixing": [
    "Определение пола по профессии, имени, статистике корпуса или гендерному стереотипу.",
    "Изменение числа, отрицания, действия или отношения времени вместе с родом.",
    "Скрытая gender-аннотация как единственное доказательство ответа.",
    "Другие objective_id, разные профили или разные split в одной партии.",
    "Публичные reference-примеры и их производные в скрытом DEV/EXAM."
  ],
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "TR04-EXPLICIT-GENDER.en-he.REF01",
      "profile_id": "en-he",
      "family_id": "v3-ref.translation.gender-engineer",
      "prompt": "Переведи с языка «английский» на язык «иврит». Выдай только перевод. Контекст: в сообщении описывается состояние самого говорящего; местоимение в придаточной части относится к этому же человеку.\nИсходный текст: The engineer said that she was ready.",
      "chosen": "המהנדסת אמרה שהיא מוכנה.",
      "rejected": "המהנדס אמר שהוא מוכן.",
      "error_tag": "tr04_explicit_gender",
      "rationale": "Женский референт задан местоимением и/или согласованием в самом тексте. Rejected меняет его, а не просто выбирает другой стиль.",
      "counterexample": "Исходник «The engineer is ready» не сообщает пол. Нельзя автоматически переносить на него женский chosen этого примера.",
      "source_refs": [
        "RAG-023",
        "RAG-024"
      ],
      "metadata": {
        "response_language": "he",
        "language_pair": {
          "source": "en",
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
        "origin_family_id": "v3-ref.translation.gender-engineer",
        "source_raw": "The engineer said that she was ready.",
        "response_contract": "translation_only",
        "leakage_group_ids": [
          "source:v3-ref.translation.gender-engineer"
        ],
        "learning_objective_id": "TR04-EXPLICIT-GENDER",
        "explicit_reference": "female",
        "number": "singular",
        "semantic_roles": {
          "speaker": "engineer",
          "experiencer": "same_engineer"
        },
        "gender_evidence": [
          "she"
        ]
      }
    },
    {
      "example_id": "TR04-EXPLICIT-GENDER.en-he.REF02",
      "profile_id": "en-he",
      "family_id": "v3-ref.translation.gender-client",
      "prompt": "Переведи с языка «английский» на язык «иврит». Выдай только перевод. Контекст: в сообщении описывается состояние самого говорящего; местоимение в придаточной части относится к этому же человеку.\nИсходный текст: The client said that he was tired.",
      "chosen": "הלקוח אמר שהוא עייף.",
      "rejected": "הלקוחה אמרה שהיא עייפה.",
      "error_tag": "tr04_explicit_gender",
      "rationale": "Мужской референт указан в исходнике; rejected меняет его при сохранении состояния усталости.",
      "counterexample": "Грамматически мужское название профессии без дополнительных свидетельств не всегда задаёт пол референта. Здесь доказательство — полный текст, включая местоимение.",
      "source_refs": [
        "RAG-023",
        "RAG-024"
      ],
      "metadata": {
        "response_language": "he",
        "language_pair": {
          "source": "en",
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
        "origin_family_id": "v3-ref.translation.gender-client",
        "source_raw": "The client said that he was tired.",
        "response_contract": "translation_only",
        "leakage_group_ids": [
          "source:v3-ref.translation.gender-client"
        ],
        "learning_objective_id": "TR04-EXPLICIT-GENDER",
        "explicit_reference": "male",
        "number": "singular",
        "semantic_roles": {
          "speaker": "client",
          "experiencer": "same_client"
        },
        "gender_evidence": [
          "he"
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



## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\TR04-EXPLICIT-GENDER\en-he\T371\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\TR04-EXPLICIT-GENDER\en-he\T371\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\TR04-EXPLICIT-GENDER\en-he\T371\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `TR04-EXPLICIT-GENDER/he-en`. Не запускать review, accept, export, oracle или обучение.
