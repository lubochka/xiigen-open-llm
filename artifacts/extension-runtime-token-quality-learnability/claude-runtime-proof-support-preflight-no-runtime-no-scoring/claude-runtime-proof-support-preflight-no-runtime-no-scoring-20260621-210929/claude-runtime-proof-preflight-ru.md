# Claude runtime proof preflight: support-only

Guard phrases for parent verification: этот support-only preflight не является доказательством Claude runtime proof; он создан без запуска Claude. Перед любым будущим runtime/scoring/training действием все еще требуется literal `fresh host-specific approval` и именно `fresh host-specific Luba approval`.

Этот пакет является только подготовительным support-only preflight для будущего Claude host runtime proof. Он не является Claude runtime proof, не запускает Claude, не проверяет Claude Desktop/chat UI, не выполняет scoring/eval/proof runner, не запускает training/DPO и не подтверждает готовность host/runtime/product.

## Что должен показать будущий Claude proof

Будущий Claude proof должен содержать видимые UI/chat-доказательства из самого Claude host: какой запрос был отправлен, какой ответ был показан пользователю, какой run id и timestamp видны или однозначно связаны с UI/chat evidence, и где видно, что это именно Claude surface, а не локальный adapter smoke или автономный скрипт. Скриншоты, transcript refs и redacted evidence должны принадлежать одному запуску.

## Same-run correlation refs

Один и тот же будущий запуск должен связать `run_id`, timestamp, host name `Claude`, visible UI/chat evidence, redacted input ref, redacted output ref, RequestedSkills/AvailableSkills binding, отрицательный/error path и no-side-effect statement. Нельзя смешивать артефакты из разных запусков или подменять UI/chat evidence более ранним support-only evidence.

## Redacted input/output refs

Будущие redacted input/output refs должны доказать, что вход был отправлен через Claude host, выход вернулся в видимый Claude chat/UI, private/raw content не раскрывался, а redaction сохраняет достаточные hashes/ids/ссылки для проверки связи с тем же запуском. Raw private reads остаются неразрешенными.

## RequestedSkills/AvailableSkills binding

Будущий proof должен показать, какие RequestedSkills были запрошены, какие AvailableSkills реально были доступны в Claude surface, как выбранная skill binding отразилась в выполнении, и как это связано с тем же run id. Если skill недоступна или binding отклонен, это должно быть видно как безопасный отрицательный путь, а не как молчаливый успех.

## Negative/error path

Будущий proof должен включить отрицательный или error path: например, недоступная skill, некорректная skill binding или безопасно отклоненный запрос. Evidence должно показать видимый ответ/ошибку в Claude UI/chat, тот же correlation id, отсутствие runtime/scoring/training побочных действий и отсутствие raw private reads.

## No-side-effect statement

Будущий no-side-effect statement должен доказать, что запуск не менял код, модели, checkpoints, locators, targets, training data, DPO data и project state сверх явно разрешенных proof artifacts. Для этого нужны refs на проверяемые артефакты и явное перечисление отсутствующих side effects.

## Fresh approval still required

Перед любым будущим Claude runtime launch, scoring, proof/eval runner, training/DPO, checkpoint/model/locator/target mutation, external AI/model/web call или raw private read требуется свежая host-specific approval quote от Luba. Этот preflight не предоставляет такое разрешение и не закрывает runtime proof queue.
