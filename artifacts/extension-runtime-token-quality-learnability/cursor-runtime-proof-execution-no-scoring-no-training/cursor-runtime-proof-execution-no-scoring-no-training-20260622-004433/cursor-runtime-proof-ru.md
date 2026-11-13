# Cursor runtime proof - FAIL_NEEDS_REPAIR

Work order: DOCS-QA-CURSOR-RUNTIME-PROOF-EXECUTION-NO-SCORING-NO-TRAINING-01

## Что запускалось

- C:\Users\lubao\AppData\Local\Programs\cursor\resources\app\bin\cursor.cmd --help
- C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe --new-window --user-data-dir <runRoot>\cursor-user-data-extension-tests --extensions-dir <runRoot>\cursor-extensions-extension-tests --extensionDevelopmentPath "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills" --extensionTestsPath <runRoot>\extension-host-tests --disable-gpu --log trace <runRoot>
- C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe --new-window --user-data-dir Z:\u7 --extensions-dir Z:\x7 --extensionDevelopmentPath "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills" --inspect-extensions=9333 --disable-gpu --log trace Z:\w
- Invoke-RestMethod http://127.0.0.1:9333/json/list
- WScript/SendKeys and isolated keybinding attempts against xiigenSmartSkills.panel with PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR set to Z:\e5/Z:\e7/Z:\e9

Все запуски были локальными. Для обхода длинных путей Windows временно использовался subst-диск Z:, указывающий на C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-runtime-proof-execution-no-scoring-no-training\cursor-runtime-proof-execution-no-scoring-no-training-20260622-004433; после попыток mapping был удален.

## Почему это Cursor runtime path

Запуск шел через локальный Cursor executable/CLI, а не через dashboard/API-only simulation. В run cursor-runtime-proof-20260622-004433 Cursor был запущен с --extensionDevelopmentPath на реальную папку расширения C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills и с --inspect-extensions=9333. Локальный Extension Host inspector ответил на http://127.0.0.1:9333/json/list, а exthost log зарегистрировал xiigen.cursor-xiigen-smart-skills как isUnderDevelopment:true.

## Positive request

Позитивный redacted prompt был подготовлен как безопасный малый Cursor smart-skills routing request, но команда расширения не была надежно вызвана через UI/Extension Host. Fresh positive output от XIIGen/core не появился. Поэтому positive_request_executed=false и reached_llm_mvp_core=false.

## Negative request

Негативный redacted invalid/missing-field request был подготовлен, но команда расширения также не была надежно вызвана. Fresh warning/error output от Cursor extension path не появился. Поэтому negative_request_executed=false и negative_error_path_present=false.

## Correlation ids

Same-run correlation id: cursor-runtime-proof-20260622-004433. Он присутствует в evidence-файлах этого runtime attempt, но не доказан как сквозной request/response correlation id, потому что positive/negative extension traces не были созданы.

## Fresh artifacts

Fresh evidence artifacts записаны в C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-runtime-proof-execution-no-scoring-no-training\cursor-runtime-proof-execution-no-scoring-no-training-20260622-004433. Fresh extension/core request artifacts не появились, потому что command invocation не дошел до core path.

Fresh core trace status: fresh core traces ?? ?????????.

## RequestedSkills / AvailableSkills

RequestedSkills/AvailableSkills binding не доказан. Эквивалентная проверка загрузки расширения есть: Cursor Extension Host зарегистрировал extension id xiigen.cursor-xiigen-smart-skills и команды xiigen.smartSkills.select / xiigenSmartSkills.panel в dev extension metadata, но request-level binding до llm_mvp_core trace отсутствует.

## No side effects

No git, scoring, training, DPO, model mutation, checkpoint promotion, Claude runtime proof, or Codex runtime proof was performed. Cursor использовал isolated user-data/extensions folders внутри единственного разрешенного timestamped leaf. Dotnet build/test не запускался. External AI/web/model calls не выполнялись.

## Что не доказано

Этот runtime attempt не доказывает token savings; не доказывает quality improvement; не доказывает learnability; не доказывает semantic quality; не доказывает model readiness; не доказывает Claude/Codex runtime; не доказывает full extension readiness; и не доказывает готовность promoted checkpoint. Он также не доказывает, что Cursor-side positive/negative requests reached llm_mvp_core, потому что свежие request/response traces не были получены.

## Verdict

FAIL_NEEDS_REPAIR. Следующий bounded work order: DOCS-QA-CURSOR-RUNTIME-PROOF-REPAIR-DISCOVERY-NO-SCORING-NO-TRAINING-01.
