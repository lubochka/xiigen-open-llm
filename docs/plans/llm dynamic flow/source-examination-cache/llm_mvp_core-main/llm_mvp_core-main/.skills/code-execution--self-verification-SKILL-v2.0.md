# Self-Verification Skill v1.2

## When to Invoke

- Before declaring any fix "done"
- Before any phase-end gate
- After any AF station, fabric provider, DNA guard, or skill block change
- When a test passes but you are not confident the fix is at the right level
- **v1.1:** After any portability remediation (ClsService removal, @connectionType annotation, FREEDOM key rename)
- **NEW v1.2:** After adding `@UseGuards`, `@Roles`, or `@Public()` to any controller or route

---

## The 6 Rules

| # | Rule | What It Catches |
|---|------|-----------------|
| 1 | `fresh-fixtures` | Using hand-crafted StationInput instead of real EngineContract fixtures |
| 2 | `categorize` | Wrong change category — treating a MACHINE, PORTABILITY, or AUTH change as BACKWARD_COMPAT |
| 3 | `right-level` | Verifying at the wrong test level — L1 unit only when auth or portability requires more |
| 4 | `cascade` | Missing downstream effects of a change — factory → AF mapping → the business flow arbiter registration |
| 5 | `non-renderable` | Claiming DNA compliance without building and running the generated code |
| 6 | `predict` | Not predicting which DNA patterns, flows, and AF stations are affected before changing |

---

## Rule 2: Categorize (updated v1.2)

**Classify the change before testing.** Every change is one of:

| Category | Definition | Test Requirement |
|----------|------------|-----------------|
| `MACHINE_CHANGE` | Changes the engine's invariant behavior — DNA patterns, fabric contracts, station routing | L1 + L2 + L3 mandatory |
| `FREEDOM_CHANGE` | Changes configurable behavior — provider selection, quality thresholds, prompt templates | L1 + L2; L3 if cross-tenant |
| `BACKWARD_COMPAT` | Adds new behavior without breaking existing | L1; verify existing L2/L3 still pass |
| `NEW_ARTIFACT` | New factory, task type, flow — no existing behavior broken | L1 + L2 + the business flow arbiter validation |
| `PORTABILITY_REMEDIATION` | Removes ClsService, adds @connectionType, renames FREEDOM keys, adds requiredCoInstalls | L1 + portability check portability gate + unit test layer of FLOW-PORTABILITY-TEST-protocol version 2.0 |
| `authentication protection addition` | Adds @UseGuards, @Roles, @Public() to controller/routes; updates bypass-paths.registry.ts | L1 + authentication check auth gate + Rule 7 (401/403) + Rule 8 (the authentication and role isolation requirement cross-tenant JWT, if Tier C) — NEW v1.2 |

---

## Rule 2: Why authentication protection addition Is a Separate Category

Adding `@UseGuards(JwtAuthGuard, RolesGuard)` to a controller is NOT a `BACKWARD_COMPAT`
change. It changes the security surface in a way that breaks every existing caller
that does not supply a valid JWT with the correct role. Specifically:

**What BACKWARD_COMPAT testing does NOT verify:**
- Unauthenticated requests now return 401 (not 200)
- Wrong-role requests now return 403 (not 200 or the previous response)
- @Public() routes remain reachable without JWT
- Cross-tenant JWTs are rejected on routes they should not access (the authentication and role isolation requirement)
- The bypass-paths.registry.ts is correctly updated for all @Public() routes

A developer who classifies adding guards as BACKWARD_COMPAT will run L1 unit tests
and declare done — missing every auth-specific behavior change.

**authentication protection addition verification checklist:**
```bash
FLOW_DIR="server/src/engine/flows/{slug}"
CONTROLLER_FILES=$(find $FLOW_DIR -name "*.controller.ts" 2>/dev/null)

# 1. V10 auth gate (from flow-implementation-guide v1.3.0)
for FILE in $CONTROLLER_FILES; do
 GUARDS=$(grep -c "@UseGuards" $FILE 2>/dev/null || echo 0)
 CONTROLLERS=$(grep -c "@Controller(" $FILE 2>/dev/null || echo 0)
 echo "authentication check authentication check 1: $FILE — controllers=$CONTROLLERS guards=$GUARDS"
 # Expected: GUARDS >= CONTROLLERS

 ROUTES=$(grep -cE "@Get\(|@Post\(|@Put\(|@Delete\(|@Patch\(" $FILE 2>/dev/null || echo 0)
 AUTH_DECL=$(grep -cE "@Roles\(|@Public\(\)" $FILE 2>/dev/null || echo 0)
 echo "authentication check authentication check 2: $FILE — routes=$ROUTES auth_decl=$AUTH_DECL"
 # Expected: AUTH_DECL >= ROUTES
done

# 2. Rule 7: auth route test (from test-integrity v2.2.0)
CTRL_SPECS=$(find $FLOW_DIR -name "*.controller.spec.ts" 2>/dev/null)
for SPEC in $CTRL_SPECS; do
 AUTH_401=$(grep -cE "\.expect\(401\)|HttpStatus\.UNAUTHORIZED" $SPEC 2>/dev/null || echo 0)
 AUTH_403=$(grep -cE "\.expect\(403\)|HttpStatus\.FORBIDDEN" $SPEC 2>/dev/null || echo 0)
 echo "Rule 7: $SPEC — 401=$AUTH_401, 403=$AUTH_403"
 # Expected: both > 0
done

# 3. bypass-paths.registry.ts updated for all @Public() routes
PUBLIC_ROUTES=$(grep -rn "@Public()" $FLOW_DIR --include="*.controller.ts" 2>/dev/null | wc -l)
if [ "$PUBLIC_ROUTES" -gt 0 ]; then
 echo "Public routes found: $PUBLIC_ROUTES — verify each is in bypass-paths.registry.ts"
 grep -rn "@Public()" $FLOW_DIR --include="*.controller.ts" 2>/dev/null | \
 sed 's/^/ NEEDS BYPASS ENTRY: /'
 BYPASS_ENTRIES=$(grep -c "flow-{slug}\|{slug}" \
 server/src/auth/bypass-paths.registry.ts 2>/dev/null || echo 0)
 echo " Bypass entries for this flow: $BYPASS_ENTRIES"
 # Expected: BYPASS_ENTRIES >= PUBLIC_ROUTES
fi

# 4. Rule 8: cross-tenant JWT isolation (if Tier C target)
TIER_TARGET=$(jq -r '.tenant certification tier // "NONE"' \
 docs/sessions/{slug}/STATE.json 2>/dev/null || echo "NONE")
if [ "$TIER_TARGET" = "Tier C" ] || [ "$TIER_TARGET" = "Tier D" ]; then
 R6_TESTS=$(grep -rcE "cross.tenant|tenantB.*token|crossTenant" \
 $FLOW_DIR --include="*.spec.ts" 2>/dev/null \
 | awk -F: '{sum+=$2} END {print sum+0}')
 echo "Rule 8 the authentication and role isolation requirement cross-tenant JWT tests: $R6_TESTS"
 # Expected: >= 1 for Tier C target
fi

# 5. Update STATE.json authentication status
node -e "
 const fs = require('fs');
 const s = JSON.parse(fs.readFileSync('docs/sessions/{slug}/STATE.json'));
 s.authentication status = 'authentication in place';
 s.authGaps = [];
 fs.writeFileSync('docs/sessions/{slug}/STATE.json', JSON.stringify(s, null, 2));
" 2>/dev/null || echo "STATE.json update: set authentication status=authentication in place manually"
echo "STATE.json updated: authentication status = authentication in place"
```

**authentication deferred (not yet deployed) exception:**
If `authentication status = authentication deferred (not yet deployed)` (auth.module.ts absent), this category does not yet
apply. Controllers exist but guards are not active. When AUTH-ROLES-GROUPS-plan version 3.0
Phases 1-4 deploy, re-classify as authentication protection addition and run the full checklist.

---

## Rule 2: Why PORTABILITY_REMEDIATION Is a Separate Category (v1.1, unchanged)

Removing ClsService from a service is NOT a BACKWARD_COMPAT change. It changes how
tenantId flows through the service. BACKWARD_COMPAT testing does NOT verify:
- tenantId is correctly scoped in concurrent calls
- @connectionType annotation is present and correct (requires portability check portability gate)
- FLOW-PORTABILITY-TEST-PROTOCOL unit test layer passes

**PORTABILITY_REMEDIATION verification checklist:**
```bash
FLOW_DIR="server/src/engine/flows/{slug}"

# 1. Run portability gate (from dna-compliance-guard v1.2.0)
P1=$(grep -rc "import.*ClsService\|from 'nestjs-cls'" $FLOW_DIR --include="*.service.ts" \
 | awk -F: '{sum+=$2} END {print sum+0}')
echo "portability check 1 ClsService: $P1" # Expected: 0

ANNOTATED=$(grep -rl "@connectionType FLOW_SCOPED" $FLOW_DIR --include="*.service.ts" | wc -l)
TOTAL=$(ls $FLOW_DIR/*.service.ts 2>/dev/null | wc -l)
echo "portability check 2 Annotated: $ANNOTATED / $TOTAL" # Expected: equal

P3=$(grep -rE "freedom\.get\(|fromConfig\(" $FLOW_DIR --include="*.service.ts" \
 | grep -vc "flow[0-9][0-9]*_" || echo 0)
echo "portability check 3 Unscoped keys: $P3" # Expected: 0

# 2. Run concurrent tenant isolation test (mandatory for portability check 1 remediation)
npx jest --testPathPattern="$FLOW_DIR" \
 --testNamePattern="concurrent\|tenant.*isolat" --runInBand

# 3. Run FLOW-PORTABILITY-TEST-PROTOCOL-v2.0 unit test layer
# See the flow portability test protocol skill v1.1.0 unit test layer checklist

# 4. Update STATE.json portability status
node -e "
 const fs = require('fs');
 const s = JSON.parse(fs.readFileSync('docs/sessions/{slug}/STATE.json'));
 s.portability status = 'MOBILE';
 s.portabilityGaps = [];
 fs.writeFileSync('docs/sessions/{slug}/STATE.json', JSON.stringify(s, null, 2));
"
echo "STATE.json updated: portability status = MOBILE"
```

---

## Rule 1: Fresh Fixtures

**Real fixtures only.** Level 2 (simulation) tests must use real `EngineContract` objects
from `server/src/engine-contracts/sample-contracts.ts`. Hand-crafted `StationInput`
inline in tests is not a simulation — it is an extended unit test with fabricated inputs.

```typescript
// WRONG:
const input: StationInput = { taskType: 'T-001', fabricType: 'in-memory', ... };

// RIGHT:
import { sampleContracts } from '../engine-contracts/sample-contracts';
const contract = sampleContracts.find(c => c.taskType === 'T-001');
const input = StationInputFactory.fromContract(contract);
```

---

## Rule 3: Right Level

Match the test level to the change category:

| Change category | Minimum test level |
|----------------|-------------------|
| `MACHINE_CHANGE` | L1 + L2 + L3 |
| `FREEDOM_CHANGE` | L1 + L2 |
| `BACKWARD_COMPAT` | L1; verify L2/L3 still pass |
| `NEW_ARTIFACT` | L1 + L2 + the business flow arbiter validation |
| `PORTABILITY_REMEDIATION` | L1 + portability check + the flow portability test protocol skill v1.1.0 unit test layer + concurrent isolation test |
| `authentication protection addition` | L1 + authentication check + Rule 7 (401/403) + Rule 8 (the authentication and role isolation requirement if Tier C) + bypass-paths check |

---

## Anti-Patterns

1. **"All unit tests pass" declared as done.** Unit tests do not verify integration, concurrent behavior, or real fixture compatibility.
2. **"I reviewed the code and it looks correct."** Visual inspection is not verification.
3. **Skip cascade check because "it's a small change."** Small changes in fabric providers affect all stations using that fabric.
4. **Verify at L1 only because L2 is slow.** Slowness is not a justification for under-verification.
5. **v1.1: Classify ClsService removal as BACKWARD_COMPAT.** It is PORTABILITY_REMEDIATION. Requires portability check + concurrent isolation test.
6. **NEW v1.2: Classify adding @UseGuards as BACKWARD_COMPAT.** It is authentication protection addition. Breaks existing callers without JWT. Requires authentication check + Rule 7 + bypass-paths verification.

---

## Integration

```
self-verification-skill v1.2
 → Rule 2 categorize → dispatches to correct test suite
 → confirms fix verified before three-level-verification declares done
 → confirms cascade before declaring bug closed (bug-to-tests-skill)
 → PORTABILITY_REMEDIATION → portability check gate → dna-compliance-guard v1.2.0 portability check 1..portability check 5
 → PORTABILITY_REMEDIATION → the flow portability test protocol skill v1.1.0 unit test layer
 → authentication protection addition → authentication check gate → flow-implementation-guide v1.3.0
 → authentication protection addition → Rule 7 → test-integrity v2.2.0 (401/403)
 → authentication protection addition → Rule 8 → test-integrity v2.2.0 (the authentication and role isolation requirement, Tier C)
 → authentication protection addition → bypass-paths.registry.ts verification
```

## Changelog

- **v1.0.0** — initial skill. 6 rules: fresh-fixtures, categorize, right-level,
 cascade, non-renderable, predict. 4 change categories.
- **v1.1.0** — Rule 2: PORTABILITY_REMEDIATION added as 5th change category.
 Verification checklist: portability check + concurrent isolation test + the flow portability test protocol skill unit test layer.
 Anti-pattern 5: ClsService removal is not BACKWARD_COMPAT. Closes the translation plan series group.
- **v1.2.0** — Rule 2: authentication protection addition added as 6th change category.
 Verification checklist: authentication check + Rule 7 (401/403) + Rule 8 (the authentication and role isolation requirement if Tier C) +
 bypass-paths.registry.ts. authentication deferred (not yet deployed) exception documented. Anti-pattern 6:
 adding @UseGuards is not BACKWARD_COMPAT. Rule 3 table updated with AUTH row.
 Closes AUTH-ARBITER-SKILLS-REMEDIATION-plan version 3.0 Phase 7.
