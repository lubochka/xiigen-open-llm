#!/usr/bin/env bash
# Обязательный прогон ПОСЛЕ ремонта улик (наряд NEEDS_FIX по слайсу «равные голоса
# соседей по методам разрешаются расстоянием», опубликованному коммитом 2f710d2b445).
# Пункты 6 и 7 ремонта изменили файл проверок
# Tests/ExperimentFixes/MethodKnnTieReachedThroughFacadeTests.cs (одна буква в
# комментарии и один абзац заголовочного комментария), поэтому тестовый проект
# собирается заново, обнаружение проверяется по именам, и обе семьи прогоняются
# до ожидаемых 8 из 8.
# Результат раннера пишется в СВЕЖИЙ каталог repair-0001/runs/after-repair/ под НОВЫМ
# именем файла: ни один прежний результат раннера не переиспользуется и не затирается.
# Каждая команда записывается ДОСЛОВНО в commands-verbatim.txt вместе со своим кодом
# возврата; цепочек с "&&", глотающих отказ, нет.
set -u

REPO="/c/Projects/llm_mvp_core"
RUN="$REPO/artifacts/fbe/FIX-R21/repair-0001/runs/after-repair"
CMD="$RUN/commands-verbatim.txt"
FILTER="FullyQualifiedName~MethodKnnTieIsResolvedByDistanceTests|FullyQualifiedName~MethodKnnTieReachedThroughFacadeTests"
TRX="repair-0001-after-repair-both-families.trx"
mkdir -p "$RUN"
cd "$REPO" || exit 90

# Файл дословных команд начинается заново: это первый и единственный прогон в этом каталоге.
: > "$CMD"
echo "cwd=$(pwd)" >> "$CMD"
echo "started_utc=$(date -u +%Y-%m-%dT%H:%M:%SZ)" >> "$CMD"
echo "started_local=$(date +%Y-%m-%dT%H:%M:%S%z)" >> "$CMD"
echo "script=artifacts/fbe/FIX-R21/repair-0001/run-after-repair.sh (этот файл и есть дословный текст команд; подстановки ниже раскрыты)" >> "$CMD"
echo "RUN=$RUN" >> "$CMD"
echo "FILTER=$FILTER" >> "$CMD"

# Шаг 0: свежесть каталога результатов — до прогона в нём не должно быть ни одного .trx.
PRE_TRX=$(find "$RUN" -maxdepth 1 -name '*.trx' | wc -l | tr -d ' ')
echo "step0_trx_files_present_before_run=$PRE_TRX" >> "$CMD"

# Шаг 1: сборка тестового проекта в Release (транзитивно собирает и продукт 12.SemanticCompiler).
echo "=== STEP1 ===" >> "$CMD"
echo "dotnet build Tests/LlmFromScratch.Tests.csproj -c Release > \"$RUN/step1-build-tests.log\" 2>&1" >> "$CMD"
dotnet build Tests/LlmFromScratch.Tests.csproj -c Release > "$RUN/step1-build-tests.log" 2>&1
E1=$?
echo "step1_exit=$E1" >> "$CMD"

# Шаг 2: обнаружение по именам — обе семьи одним фильтром, без сборки.
echo "=== STEP2 ===" >> "$CMD"
echo "dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter \"$FILTER\" --list-tests > \"$RUN/step2-list-tests.log\" 2>&1" >> "$CMD"
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build \
  --filter "$FILTER" \
  --list-tests > "$RUN/step2-list-tests.log" 2>&1
E2=$?
echo "step2_exit=$E2" >> "$CMD"

# Шаг 3: прогон обеих семей; результат раннера — в свежем каталоге под новым именем.
echo "=== STEP3 ===" >> "$CMD"
echo "dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter \"$FILTER\" --results-directory \"$RUN\" --logger \"trx;LogFileName=$TRX\" > \"$RUN/step3-both-families.log\" 2>&1" >> "$CMD"
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build \
  --filter "$FILTER" \
  --results-directory "$RUN" \
  --logger "trx;LogFileName=$TRX" > "$RUN/step3-both-families.log" 2>&1
E3=$?
echo "step3_exit=$E3" >> "$CMD"

# Шаг 4: раннер не должен был печатать предупреждение о перезаписи результата.
OVERWRITE=$(grep -c "Overwriting results file" "$RUN/step3-both-families.log")
echo "step4_overwrite_warning_lines=$OVERWRITE" >> "$CMD"

echo "finished_utc=$(date -u +%Y-%m-%dT%H:%M:%SZ)" >> "$CMD"
echo "finished_local=$(date +%Y-%m-%dT%H:%M:%S%z)" >> "$CMD"
echo "DONE E1=$E1 E2=$E2 E3=$E3 OVERWRITE_WARNINGS=$OVERWRITE" >> "$CMD"
