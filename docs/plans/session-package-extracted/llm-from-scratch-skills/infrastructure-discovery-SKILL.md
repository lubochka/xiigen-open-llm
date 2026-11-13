# Infrastructure Discovery — LlmFromScratch
## For: Session start, before writing any plan

Run this before writing plan content.
Plans written from memory produce wrong paths, wrong baseline numbers,
and references to subprojects that don't exist yet.

## The Rule

Every fact a plan states about the codebase must have a live file:line reference.

---

## When to Invoke

- Before writing any plan, spec, or implementation decision
- At the start of every new phase
- Whenever a plan references a subproject number, namespace, or test filter

---

## The 8-Step Protocol

### Step 0 — Pre-existing Failure Audit

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Record the current pass/fail counts. Store as `baselineCount` before writing
any plan element.

**Why this is step 0:** Plans must not treat pre-existing failures as goals.
The zero-regression gate measures new failures introduced by this session —
not the total count.

---

### Step 1 — Read Subproject Map

```bash
cat docs/SUBPROJECT-MAP.md
# Or: ls -d */  (to see which subproject directories exist)
```

Record which subprojects exist and their namespace names.
**Red flag:** Plan references a subproject number that does not exist in the
solution.

---

### Step 2 — Code Structure

```bash
# What namespaces exist?
grep -rn "^namespace " . --include="*.cs" | grep -v "/bin/\|/obj/" | \
  sed 's/.*namespace //' | sort -u

# What interfaces exist in Fabric/?
find . -path "*/Fabric/Interfaces.cs" | xargs grep -n "public interface"

# What public classes exist in each subproject?
find . -name "*.cs" | grep -v "/bin/\|/obj/\|Tests" | \
  xargs grep -ln "public class\|public abstract class" | sort
```

---

### Step 3 — Read DECISIONS-LOCKED.md

```bash
cat docs/DECISIONS-LOCKED.md
```

Read every locked decision. Plans that contradict a locked decision are wrong
regardless of how well-reasoned they look.

---

### Step 4 — Read EXPERIMENT-RESULTS-MAP.md

```bash
cat docs/EXPERIMENT-RESULTS-MAP.md
```

Know which experiments are PASSED, FAILED, SCALE_UP, or ARCH before proposing
new algorithm work. Plans that repeat failed experiments waste sessions.

---

### Step 5 — Read Key Files in Full

For any file the plan will modify or reference, read it completely:

```bash
# Read the target file
cat NN.SubprojectName/ComponentName.cs

# Check line count (record for plan precision)
wc -l NN.SubprojectName/ComponentName.cs

# Verify the interface it implements
grep -n "interface\|: Base" NN.SubprojectName/ComponentName.cs
```

Record: actual line count, all public methods, return types.

**Red flag:** Plan says "add method X" without confirming the file's current
content and which interface it must satisfy.

---

### Step 6 — Test Filter Inventory

Know what test filters work before writing plan gates:

```bash
# Find test class names for a subproject
grep -rn "public class.*Tests" Tests/ --include="*.cs" | grep "SubprojectName"

# Verify a filter works
dotnet test --filter "ClassName" Tests/LlmFromScratch.Tests.csproj --list-tests
```

**Red flag:** Plan gate uses a filter that matches zero tests.

---

### Step 7 — Namespace Boundary Check

Before proposing any new class:

```bash
# Does a class with this name already exist?
grep -rn "class ProposedClassName" . --include="*.cs" | grep -v "/bin/\|/obj/"

# Does the namespace conflict with an existing one?
grep -rn "namespace ProposedNamespace" . --include="*.cs" | grep -v "/bin/\|/obj/"
```

---

### Step 8 — Rewrite Plan Elements with Real References

Every plan element must become:
```
BEFORE: "Add FamilyCollapser to SemanticCompiler"
AFTER:  "Add FamilyCollapser.cs to 29.SemanticCompiler/Algorithms/
         (directory currently has 5 files, 0 with 'Collapse' in name).
         Implements IFamilyCollapser29 (in 29.SemanticCompiler/Fabric/Interfaces.cs:36).
         Returns OperationResult<IReadOnlyList<string>>.
         Gate B: SC29_FamilyCollapser_Validates_CollapsesToValidate passes."
```

---

## Red Flags — Stop and Ask

| Red Flag | Problem | Action |
|----------|---------|--------|
| Plan references subproject NN that has no directory | Phantom subproject | Verify with `ls -d NN.*` |
| Plan proposes class returning raw `T` | Rule 1 violation | Change to `OperationResult<T>` |
| Plan proposes `new ConcreteStorage()` in production code | Rule 2 violation | Use constructor injection |
| Plan gate filter matches zero tests | Wrong filter | Run `--list-tests` to verify |
| Plan proposes experiment already marked FAILED | Wasted session | Check EXPERIMENT-RESULTS-MAP.md |
| Plan references a line number without reading the file | Phantom line | Read the file first |
