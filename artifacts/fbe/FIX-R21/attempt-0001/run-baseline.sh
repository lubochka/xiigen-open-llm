#!/usr/bin/env bash
# Опорный прогон уже существующей приёмки предсказателя (в плане помечена UNRUN).
# Пометка 10.11.2026 (ремонт улик): UNRUN — «не запускалась», от английского un-run,
# прогон не выполнялся; расшифровка дописана комментарием при первом употреблении по
# правилу 9 проекта, исполняемые строки скрипта не менялись.
# Каждый шаг проверяется отдельно по своему коду возврата; цепочки с "&&" нет,
# чтобы код возврата каждого шага был записан честно, а не поглощён следующим.
set -u

REPO="/c/Projects/llm_mvp_core"
EV="$REPO/artifacts/fbe/FIX-R21/attempt-0001"
RUN="$EV/runs/baseline"
mkdir -p "$RUN"
cd "$REPO" || exit 90

echo "cwd=$(pwd)" > "$RUN/commands.txt"
echo "started_utc=$(date -u +%Y-%m-%dT%H:%M:%SZ)" >> "$RUN/commands.txt"

# Шаг 1: сборка продуктового проекта семантического компилятора в конфигурации Release.
echo "=== STEP1 build 12.SemanticCompiler ===" >> "$RUN/commands.txt"
dotnet build 12.SemanticCompiler/12.SemanticCompiler.csproj -c Release > "$RUN/step1-build-product.log" 2>&1
E1=$?
echo "step1_exit=$E1" >> "$RUN/commands.txt"

# Шаг 2: сборка тестового проекта в той же конфигурации Release.
echo "=== STEP2 build Tests ===" >> "$RUN/commands.txt"
dotnet build Tests/LlmFromScratch.Tests.csproj -c Release > "$RUN/step2-build-tests.log" 2>&1
E2=$?
echo "step2_exit=$E2" >> "$RUN/commands.txt"

# Шаг 3: перечисление обнаруженных случаев — доказательство дискавери по именам.
echo "=== STEP3 list-tests filtered ===" >> "$RUN/commands.txt"
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build \
  --filter "FullyQualifiedName~MethodKnnTieIsResolvedByDistanceTests" \
  --list-tests > "$RUN/step3-list-tests.log" 2>&1
E3=$?
echo "step3_exit=$E3" >> "$RUN/commands.txt"

# Шаг 4: фактическое выполнение пяти случаев уже принятой приёмки предсказателя.
echo "=== STEP4 run baseline tests ===" >> "$RUN/commands.txt"
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build \
  --filter "FullyQualifiedName~MethodKnnTieIsResolvedByDistanceTests" \
  --results-directory "$RUN" \
  --logger "trx;LogFileName=baseline.trx" > "$RUN/step4-test-run.log" 2>&1
E4=$?
echo "step4_exit=$E4" >> "$RUN/commands.txt"

echo "finished_utc=$(date -u +%Y-%m-%dT%H:%M:%SZ)" >> "$RUN/commands.txt"
echo "DONE E1=$E1 E2=$E2 E3=$E3 E4=$E4" >> "$RUN/commands.txt"
