# Ограниченный независимый review процесса подготовки DPO

## Итог

В прочитанном объёме после двух точечных исправлений блокирующих замечаний не осталось. Это инженерное review процесса подготовки данных; оно не является формальной приёмкой пар от имени CODEX с astra 6 ultracode. Фактическая модель этого worker не подтверждена как назначенный профиль.

Прочитаны целиком tools/sets.py, 10-CODEX-AND-CHINESE-ROLES.md, MASTER-PROMPT-RU.md, CODEX-SELECT-PROMPT-RU.md, REVIEW-PROMPT-RU.md и 03-GENERATION-AND-KIRO.md. SHA-256 каждого проверенного файла — в workflow-review.json. Применены полностью прочитанные generated-code-review, plan-review и training-data-gap-audit skills; полный source ledger — source-notes.json.

## Исправленные замечания

1. **Явный запрет строки не участвовал в admission.** Значение metadata.training_allowed=false у non-reference записи раньше проходило validate и могло попасть в export. В validate_rows добавлена одна проверка TRAINING_EXPLICITLY_DISALLOWED. Reference по-прежнему допускается только специальным структурным allow-reference и не становится обучающим материалом. Это соблюдение явного статуса строки; общих новых approval-процедур не добавлено.
2. **Правила oracle расходились между документами.** В 03 исполняемая проверка кода обязательна, а REVIEW и роли объявляли её условной. Теперь для code.* принятие требует изолированной проверки поведения обоих A/B на одинаковых входах в указанной версии/диалекте. Для docs.* oracle нужен для неразрешённого факта. Если необходимый запуск отсутствует, needs_oracle=true и valid=false; текущий validate уже отказывает в такой приёмке. Подготовка и reference не блокируются; production-тесты не запускаются.

## Что проверено в действующем пути

- prepare создаёт незаполненные PENDING receipts, явно отделяет CODEX selection/review от китайского генератора и не заявляет выполненный вызов.
- У selection/review проверяются actor, required/observed profile, указанные evidence и хеши связанных байтов.
- blind сохраняет отдельный private key, канонический пакет восстанавливается из input и позиций A/B; подмена пакета/ключа обнаруживается, ключ проверяющему не требуется.
- validate требует заполненные смысловые проверки, победу действительно выбранного кандидата и отсутствие pending oracle/language review; merge повторяет структурные и byte bindings.
- merge/compose/export требуют текущий acceptance contract; статусы остаются CANDIDATE_VALIDATED_NOT_NATIVE_TRAIN_READY и INTERCHANGE_FORMAT_ONLY.
- Инструмент и текст явно ограничивают доказательство declared_evidence_only: provider_identity_authenticated=false. Они не выдают самостоятельно заполненные поля за аутентификацию или модельное качество.

## Наблюдаемая проверка

Проверен актуальный отчёт work/translation/authority-probe-results.json. Translation worker выполнил один положительный MOCK workflow prepare→blind→validate→audit→merge→compose→export, семь отрицательных случаев и регрессию reference/allow-reference. Все девять проверок прошли. Положительный fixture имеет training_allowed=true и fixture_only=true; отрицательный false-случай имеет согласованные хеши и отклоняется именно TRAINING_EXPLICITLY_DISALLOWED.

Реальных вызовов CODEX: 0. Китайской модели: 0. Обучающих прогонов: 0. Нативное исполнение C#/SQL Server/Angular: не запускалось. Эти checks подтверждают только поведение утилиты на искусственных данных и не представляют успешный опыт обучения.

## Границы вывода

Смысловая независимость семейств и достоверность evidence требуют реальной проверки содержания. Hash bindings не доказывают честность автора квитанции. Новые 72 DOC/CODE-пары остаются reference; oracle и назначенная модельная приёмка впереди. Ни production, ни его тестовая цепочка не изменены.
