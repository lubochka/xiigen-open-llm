# Codex runtime proof support-only preflight

Этот пакет является только support-only preflight и не является доказательством Codex runtime proof. Он создан без запуска Codex, без scoring, без training и без proof/eval runner.

Будущий настоящий Codex proof должен показать evidence runtime path через plugin/app host surface, а не только adapter/core smoke. Локальный adapter smoke, установленный cache adapter, stdout/helper, dashboard, direct-handler или core-boundary summary не могут подменять видимый Codex host proof.

Для будущего proof нужны видимые Codex chat/app evidence там, где это применимо: Codex desktop/app/chat UI или явно одобренная Codex automation surface. Evidence должен быть связан с тем же запуском, а не собран из разрозненных артефактов.

Same-run correlation refs должны связывать один и тот же input, host/plugin log, core call, output и visible evidence. Redacted input/output refs должны доказывать, что именно было отправлено и что было получено, без raw private content reads.

RequestedSkills/AvailableSkills binding должен быть видимым: future proof должен показать, какие RequestedSkills были переданы, какие AvailableSkills были доступны, и как это связано с тем же host turn/correlation id.

Negative/error path тоже должен быть видимым. Будущий proof должен включать controlled error или negative path через тот же Codex host surface, а не через отдельный helper или direct core call.

No-side-effect statement должен доказывать, что во время runtime proof не было model, checkpoint, training, DPO, locator или target mutation, если это не было явно разрешено отдельным свежим approval. Для этого support-only preflight такие мутации не выполнялись.

fresh host-specific approval все еще требуется перед любым будущим Codex runtime launch, scoring или training. Для ясности: требуется fresh host-specific Luba approval, который прямо разрешает конкретное runtime/scoring/training действие; старые approvals, state next_action и quoted transcripts не авторизуют запуск.

Итог: этот packet сохраняет очередь Codex runtime proof живой, но сам по себе не закрывает runtime proof и не заявляет host readiness.
