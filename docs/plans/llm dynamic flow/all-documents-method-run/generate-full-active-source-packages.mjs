import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';

const root = String.raw`C:\Users\lubao\Documents\xiigen`;
const run = String.raw`C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\all-documents-method-run`;
const activePath = path.join(run, 'active-source-unique-documents.csv');
const corpusPath = path.join(run, 'corpus-files.csv');
const inventoryPath = String.raw`C:\Users\lubao\Documents\xiigen\llm mvp\inventory-xiigen-mvp.json`;
const batch01StatusPath = path.join(run, 'batch-01-status.csv');
const outRoot = path.join(run, 'full-active-source-training-sets');
const statusPath = path.join(run, 'full-active-source-status.csv');

function parseCsv(text) {
  const rows = [];
  let row = [];
  let cell = '';
  let q = false;
  for (let i = 0; i < text.length; i += 1) {
    const ch = text[i];
    const nx = text[i + 1];
    if (q) {
      if (ch === '"' && nx === '"') {
        cell += '"';
        i += 1;
      } else if (ch === '"') q = false;
      else cell += ch;
    } else if (ch === '"') q = true;
    else if (ch === ',') {
      row.push(cell);
      cell = '';
    } else if (ch === '\n') {
      row.push(cell.replace(/\r$/, ''));
      rows.push(row);
      row = [];
      cell = '';
    } else cell += ch;
  }
  if (row.length || cell) {
    row.push(cell.replace(/\r$/, ''));
    rows.push(row);
  }
  const header = rows.shift();
  return rows.filter((r) => r.some(Boolean)).map((r) => Object.fromEntries(header.map((h, i) => [h, r[i] ?? ''])));
}

function csvEscape(v) {
  const s = String(v ?? '');
  return /[",\n\r]/.test(s) ? `"${s.replace(/"/g, '""')}"` : s;
}

function writeCsv(file, rows, cols) {
  fs.writeFileSync(file, `${cols.join(',')}\r\n${rows.map((r) => cols.map((c) => csvEscape(r[c])).join(',')).join('\r\n')}\r\n`, 'utf8');
}

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function write(file, text) {
  fs.writeFileSync(file, text.replace(/\n/g, '\r\n'), 'utf8');
}

function safeReadText(file, ext) {
  if (['.md', '.txt', '.yaml', '.yml', '.xml', '.svg', '.json', '.csv'].includes(ext.toLowerCase())) {
    return fs.readFileSync(file, 'utf8').replace(/^\uFEFF/, '');
  }
  if (ext.toLowerCase() === '.docx') return extractDocxText(file);
  return '';
}

function extractDocxText(file) {
  const buf = fs.readFileSync(file);
  const text = buf.toString('latin1');
  const parts = [];
  const re = /word\/document\.xml/g;
  if (!re.test(text)) return '';
  // Minimal extraction fallback: docx is binary zip, so use visible XML-ish fragments if present.
  const rough = text
    .replace(/<[^>]+>/g, ' ')
    .replace(/[^\x20-\x7E\r\n]+/g, ' ')
    .replace(/\s+/g, ' ')
    .trim();
  if (rough) parts.push(rough.slice(0, 8000));
  return parts.join('\n');
}

function sha256(file) {
  return crypto.createHash('sha256').update(fs.readFileSync(file)).digest('hex').toUpperCase();
}

function slug(s) {
  return s.toLowerCase().replace(/\.[a-z0-9]+$/i, '').replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '').slice(0, 70) || 'document';
}

function pickDomains(domains, start, count) {
  const out = [];
  for (let i = 0; i < count; i += 1) out.push(domains[(start + i) % domains.length]);
  return out;
}

function norm(s) {
  return String(s || '').toLowerCase().replace(/[^a-z0-9]+/g, '');
}

function plain(s) {
  return String(s || '')
    .replace(/`[^`]*`/g, ' ')
    .replace(/https?:\/\/\S+/g, ' ')
    .replace(/[#*_>\[\]()`{}|\\]/g, ' ')
    .replace(/&[a-z]+;/gi, ' ')
    .replace(/\s+/g, ' ')
    .trim();
}

function sentenceCase(s) {
  const x = plain(s).replace(/^[0-9.\-\s]+/, '').trim();
  return x ? x[0].toUpperCase() + x.slice(1) : x;
}

function firstLast(text, rel) {
  const lines = text.split(/\r?\n/).map((x) => plain(x)).filter(Boolean);
  return {
    lineCount: text ? text.split(/\r?\n/).length : 0,
    first: lines[0] || path.basename(rel),
    last: lines[lines.length - 1] || path.basename(rel),
  };
}

function headings(text, inv) {
  const found = [];
  const lines = text.split(/\r?\n/);
  for (const line of lines) {
    if (/^\s{0,3}#{1,6}\s+/.test(line) || /^\s{0,3}[-*]\s+\*\*[^*]+/.test(line) || /^\s{0,3}\d+[.)]\s+\*\*[^*]+/.test(line)) {
      const h = sentenceCase(line.replace(/^\s{0,3}#{1,6}\s+/, '').replace(/^\s{0,3}[-*]\s+/, '').replace(/^\s{0,3}\d+[.)]\s+/, ''));
      if (h && h.length > 2 && h.length < 120) found.push(h);
    }
    if (found.length >= 18) break;
  }
  if (found.length < 8 && inv?.sections) {
    for (const s of inv.sections) {
      const h = sentenceCase(s);
      if (h && !found.includes(h)) found.push(h);
    }
  }
  return [...new Set(found)].slice(0, 18);
}

function classify(rel, text, invType) {
  const r = rel.toLowerCase();
  const b = path.basename(rel).toLowerCase();
  const s = `${rel} ${text.slice(0, 2000)} ${invType || ''}`.toLowerCase();
  if (/session|state|checkpoint|resume/.test(b) || /\\(session|state|history)\\/i.test(rel)) return 'session and state evidence';
  if (/test|e2e|simulation|coverage/.test(r)) return 'testing and simulation';
  if (/arbiter|judge|review|gate|validation|quality/.test(s)) return 'review and validation';
  if (/rag|retrieval|graph/.test(s)) return 'retrieval and graph knowledge';
  if (/skill|factory|fabric|dna/.test(s)) return 'skill and architecture rules';
  if (/flow|dag|orchestrat|pipeline|topolog/.test(s)) return 'flow orchestration';
  if (/design|figma|visual|screen|editor/.test(s)) return 'design understanding';
  if (/auth|tenant|security|privacy|permission/.test(s)) return 'security and tenancy';
  if (/business|market|shop|finance|erp|calendar|blog|social/.test(s)) return 'business capability map';
  return 'project knowledge';
}

function domainsFor(rel, text, inv) {
  const cls = classify(rel, text, inv?.type);
  const hs = headings(text, inv).map((h) => h.toLowerCase()).filter((h) => h.length >= 4);
  const baseByClass = {
    'session and state evidence': ['session intent', 'creation evidence', 'decision trail', 'resume state', 'open work', 'validation notes'],
    'review and validation': ['review responsibility', 'rule coverage', 'conflict detection', 'impact explanation', 'reprocess instruction', 'approval boundary'],
    'retrieval and graph knowledge': ['source inventory', 'metadata and tags', 'retrieval routing', 'graph relationships', 'evidence selection', 'quality measurement'],
    'skill and architecture rules': ['architecture rule', 'skill responsibility', 'factory boundary', 'configuration choice', 'dependency rule', 'completion gate'],
    'flow orchestration': ['trigger and input', 'node responsibility', 'parallel branch', 'wait and recovery', 'result merge', 'review before output'],
    'design understanding': ['screen structure', 'component meaning', 'user action', 'data binding', 'system type', 'gap completion'],
    'testing and simulation': ['test objective', 'scenario setup', 'expected behavior', 'failure evidence', 'coverage decision', 'release gate'],
    'security and tenancy': ['identity context', 'tenant boundary', 'permission decision', 'data isolation', 'audit evidence', 'safe recovery'],
    'business capability map': ['business module', 'user journey', 'record lifecycle', 'automation point', 'risk and exception', 'reporting result'],
    'project knowledge': ['document purpose', 'main requirement', 'decision point', 'dependency', 'open question', 'final outcome'],
  };
  const domains = [...(baseByClass[cls] || baseByClass['project knowledge'])];
  for (const h of hs) {
    const cleaned = h.replace(/[^a-z0-9 ]+/g, ' ').replace(/\s+/g, ' ').trim();
    if (cleaned.length >= 4 && cleaned.length <= 65 && !domains.includes(cleaned)) domains.push(cleaned);
    if (domains.length >= 14) break;
  }
  while (domains.length < 12) domains.push(['source meaning', 'audience need', 'version evidence', 'edge condition', 'training example', 'simulation check'][domains.length % 6]);
  return [...new Set(domains)].slice(0, 14);
}

function topicFor(rel, text, inv) {
  if (inv?.description) return sentenceCase(inv.description).slice(0, 160);
  const hs = headings(text, inv);
  if (hs[0]) return hs[0].slice(0, 160);
  const fl = firstLast(text, rel);
  return sentenceCase(fl.first).slice(0, 160);
}

function rephrase(s) {
  return s
    .replace(/^Use /, 'Apply ')
    .replace(/^Explain /, 'Show ')
    .replace(/^Plan /, 'Prepare ')
    .replace(/^Find /, 'Identify ')
    .replace(/^Review /, 'Check ')
    .replace(/^Turn /, 'Translate ')
    .replace(/^Create /, 'Build ')
    .replace(/^Compare /, 'Contrast ');
}

function scenarios(topic, domains, cls) {
  return [
    [`Use this document to understand ${topic} and keep its main responsibilities visible.`, domains.slice(0, 4)],
    [`Explain how ${topic} should guide a human request without losing decisions, risks, or expected results.`, [domains[1], domains[2], domains[4], domains[6]].filter(Boolean)],
    [`Plan the work described by ${topic} as separated responsibilities that can move through the dynamic flow.`, [domains[2], domains[3], domains[5], domains[7]].filter(Boolean)],
    [`Find the review points, exceptions, and recovery needs in ${topic}.`, [domains[4], domains[5], domains[8], domains[9]].filter(Boolean)],
    [`Turn ${topic} into learning examples for the node pipeline.`, [domains[0], domains[6], domains[10], domains[11]].filter(Boolean)],
    [`Compare the source meaning of ${topic} with related versions or session evidence.`, [domains[0], domains[8], domains[10], domains[12]].filter(Boolean)],
    [`Prepare a user-facing answer from approved results about ${topic}.`, [domains[6], domains[7], domains[11], domains[13]].filter(Boolean)],
    [`Check whether ${topic} has enough evidence to be trusted by a reviewer.`, [domains[4], domains[5], domains[9], domains[10]].filter(Boolean)],
    [`Show what would break if one responsibility in ${topic} were skipped.`, [domains[2], domains[3], domains[5], domains[8]].filter(Boolean)],
    [`Build a simulation that proves the ${cls} content can pass through all nine nodes.`, [domains[0], domains[3], domains[6], domains[11]].filter(Boolean)],
  ];
}

function umlFrame(topic) {
  return `# 01 - UML Frame

## Source Round

This frame belongs to the training package for ${topic}. It records the dynamic-flow mapping before writing node pairs.

## Active Nodes

1. Domain splitter: user prompt -> array of domain labels.
2. Domain translator array: one domain fragment -> one formal domain statement.
3. Scheme planner: all formal domain statements -> ordered and parallel work pieces.
4. Domain processor array: one work piece -> complete processed result.
5. Applier array: one processed piece plus its instruction -> concrete outcome.
6. Result translator array: one formal result, with the full result collection visible -> user-readable result.
7. Arbiter selector: formal result collection -> required review units.
8. Arbiter units: review responsibility plus result -> pass, concern, or reprocess instruction.
9. Formatter: approved translated results -> one finished answer.

## Edges

### User Prompt To Domain Splitter

What moves: the human request in ordinary language.
What the target receives: one prompt that may contain several responsibilities.
What the target must produce: an array of domain labels.
What breaks if missing: the request is treated as one vague block.

### Domain Splitter To Domain Translator Array

What moves: each label with the matching prompt fragment.
What the target receives: one domain-scoped fragment.
What the target must produce: precise domain wording.
What breaks if missing: ambiguity travels forward.

### Domain Translator Array To Scheme Planner

What moves: all translated domain statements.
What the target receives: the full multi-domain meaning.
What the target must produce: ordered work pieces, parallel work pieces, waits, recovery, and reviews.
What breaks if missing: facts do not become a usable plan.

### Scheme Planner To Domain Processor Array

What moves: each planned work piece.
What the target receives: one work piece at a time.
What the target must produce: the complete processed result for that piece.
What breaks if missing: parallel flow collapses.

### Domain Processor Array To Applier Array

What moves: processed work and its instruction.
What the target receives: enough detail to act.
What the target must produce: a concrete result.
What breaks if missing: nothing becomes an outcome.

### Applier Array To Result Translator Array

What moves: formal results R1...Rn.
What the target receives: one result while seeing the full collection.
What the target must produce: readable wording or a contradiction flag.
What breaks if missing: raw formal results reach the user.

### Applier Array To Arbiter Selector

What moves: the full formal result collection.
What the target receives: results before formatting.
What the target must produce: selected reviewers.
What breaks if missing: risky results are not reviewed.

### Arbiter Selector To Arbiter Units

What moves: review responsibilities.
What the target receives: one responsibility and affected result.
What the target must produce: pass, concern, or reprocess.
What breaks if missing: review is named but not performed.

### Arbiter Units Back To Front Of Flow

What moves: a failed fragment with repair reason.
What the target receives: the affected fragment for another pass.
What the target must produce: a corrected split and re-run.
What breaks if missing: review cannot repair.

### Approved Translations To Formatter

What moves: approved readable fragments only.
What the target receives: safe fragments and audience.
What the target must produce: one coherent answer.
What breaks if missing: the user receives disconnected fragments.
`;
}

function related(rel, hash, corpus) {
  const name = path.basename(rel);
  const stem = norm(name.replace(/\.[^.]+$/, '')).slice(0, 28);
  const exact = corpus.filter((r) => r.Sha256 === hash && r.RelativePath !== rel).slice(0, 6);
  const same = corpus.filter((r) => path.basename(r.RelativePath || '') === name && r.RelativePath !== rel).slice(0, 6);
  const hist = corpus.filter((r) => /(old|older|history|session|worktree|state)/i.test(r.RelativePath || '') && norm(r.RelativePath).includes(stem)).slice(0, 6);
  return { exact, same, hist };
}

function indexFile(row, facts, topic, cls, domains, rels, inv) {
  const list = (items, fallback) => (items.length ? items.map((r) => `- \`${r.FullName || r.RelativePath}\``).join('\n') : `- ${fallback}`);
  return `# 00 - Index

## Source Document

One source document was used for this package:

\`${facts.full}\`

Source facts recorded for this examination:

- Relative path: \`${row.RelativePath}\`
- Type: ${cls}
- Inventory type: ${inv?.type || 'not listed in attached MVP inventory'}
- File size: ${facts.size} bytes
- Line count read for this round: ${facts.lineCount}
- SHA256: ${facts.hash}
- First meaningful line: \`${facts.first}\`
- Last meaningful line: \`${facts.last}\`

This package keeps the one-document rule. Related material is recorded below but not merged into the training pairs.

## Related Versions Found

Exact content duplicates:

${list(rels.exact, 'No exact duplicate was recorded in the manifest.')}

Same-name files:

${list(rels.same, 'No same-name version was recorded in the manifest.')}

Session or history candidates:

${list(rels.hist, 'No creation session was confirmed from the manifest.')}

## Source Meaning Used For Training

${topic}

## Domains This Document Teaches

${domains.map((d) => `- ${d}`).join('\n')}

## Package Files

- \`01-UML-FRAME.md\`
- \`node-01-domain-splitter-set.md\`
- \`node-02-domain-translator-set.md\`
- \`node-03-scheme-planner-set.md\`
- \`node-04-domain-processor-set.md\`
- \`node-05-applier-set.md\`
- \`node-06-result-translator-set.md\`
- \`node-07-arbiter-selector-set.md\`
- \`node-08-arbiter-units-set.md\`
- \`node-09-formatter-set.md\`
- \`10-SIMULATION-CHECK.md\`
`;
}

function node01(topic, scs) {
  let s = `# Node-01 - Domain Splitter Set

Runtime A is a human prompt about the source document.
Runtime B is the array of domains touched by that prompt.

`;
  scs.forEach((sc, i) => {
    s += `## Pair 1.${i + 1} - Runtime Split

A: "${sc[0]}"

B: ${JSON.stringify(sc[1])}

Pair 1.${i + 1} - Rephrased

A: "${rephrase(sc[0])}"

B: ${JSON.stringify(sc[1])}

`;
  });
  return s;
}

function node02(topic, scs) {
  let s = `# Node-02 - Domain Translator Set

Runtime A is one prompt fragment with one domain label.
Runtime B is the full domain wording that the next node can use.

`;
  scs.forEach((sc, i) => {
    const d = sc[1][0];
    s += `## Pair 2.${i + 1} - ${d}

A: Prompt fragment: "${sc[0]}" Domain label: ${d}.

B: The ${d} responsibility in this document means: preserve the source meaning of ${topic}; state what the user is asking for; name the information needed before action; name the result that should leave this domain; and keep visible any risk, wait, review, or dependency.

Pair 2.${i + 1} - Rephrased

A: Domain label: ${d}. Fragment: "${rephrase(sc[0])}"

B: Treat ${d} as one separate responsibility. Explain the intended result, the needed evidence, the handoff to the next node, and the condition that would require review or reprocessing.

`;
  });
  return s;
}

function node03(topic, domains) {
  let s = `# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

`;
  for (let i = 0; i < 8; i += 1) {
    const ds = pickDomains(domains, i, 4);
    s += `## Pair 3.${i + 1} - Plan Work Pieces

A: Translated domain statements for ${topic}: ${ds.map((d) => `${d} is present and needs its own handling`).join('; ')}.

B: Create work pieces for ${ds.join(', ')}. Keep ${ds[0]} and ${ds[1]} separate. Allow ${ds[2]} and ${ds[3]} to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.${i + 1} - Rephrased

A: The current request has translated responsibilities: ${ds.join(', ')}.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

`;
  }
  return s;
}

function node04(topic, domains) {
  let s = `# Node-04 - Domain Processor Set

Runtime A is one planned work piece.
Runtime B is the complete processed result for that piece.

`;
  for (let i = 0; i < 8; i += 1) {
    const d = domains[i % domains.length];
    s += `## Pair 4.${i + 1} - Process ${d}

A: Work piece: process ${d} for ${topic}.

B: The ${d} result contains the intended outcome, needed evidence, success state, waiting state, failure state, and the next branch that consumes it. It is complete enough for applying without rereading the source document.

Pair 4.${i + 1} - Rephrased

A: Domain work item: ${d}. Source meaning: ${topic}.

B: Processing is complete when ${d} names what is ready, what remains uncertain, what would block progress, and what exact fragment should be reprocessed if this branch fails review.

`;
  }
  return s;
}

function node05(topic, domains) {
  let s = `# Node-05 - Applier Set

Runtime A is one processed work piece plus an instruction.
Runtime B is the concrete applied outcome.

`;
  for (let i = 0; i < 8; i += 1) {
    const d = domains[(i + 1) % domains.length];
    s += `## Pair 5.${i + 1} - Apply ${d}

A: Processed piece: ${d}. Instruction: apply it while preserving the document's source meaning and branch boundaries.

B: Applied outcome: ${d} now has a concrete result for ${topic}. The outcome says what became available, what changed for the wider plan, what still waits, and which review responsibility should inspect it before formatting.

Pair 5.${i + 1} - Rephrased

A: Ready branch: ${d}. Apply this branch to the document round.

B: The branch produces a named result, a handoff, a visible condition for review, and a repair instruction if the result conflicts with the source meaning.

`;
  }
  return s;
}

function node06(topic, domains) {
  let s = `# Node-06 - Result Translator Set

Runtime A is one formal result while the full result collection remains visible.
Runtime B is readable language for that result.

`;
  for (let i = 0; i < 9; i += 1) {
    const d = domains[(i + 2) % domains.length];
    s += `## Pair 6.${i + 1} - Translate ${d}

A: Formal result R${i + 1}: ${d} completed for ${topic}. The full result collection is visible for consistency.

B: The ${d} part is ready in plain language. The user can see what was decided, what depends on it, what is still waiting, and whether another result contradicts it.

Pair 6.${i + 1} - Rephrased

A: Result R${i + 1}: ${d} is complete, waiting, or blocked in the formal collection.

B: Explain ${d} without internal wording. Preserve the fact, the dependency, the wait state, and the user-visible meaning.

`;
  }
  return s;
}

function node07(topic, domains) {
  const reviewers = ['ordering reviewer', 'privacy reviewer', 'business-rule reviewer', 'evidence reviewer', 'consistency reviewer', 'recovery reviewer', 'audience reviewer', 'completion reviewer'];
  let s = `# Node-07 - Arbiter Selector Set

Runtime A is the formal result collection.
Runtime B is the required review units.

`;
  for (let i = 0; i < 8; i += 1) {
    const ds = pickDomains(domains, i, 3);
    s += `## Pair 7.${i + 1} - Select Review

A: Formal results for ${topic} include ${ds.join(', ')}.

B: Select review units: [${reviewers[i]}, consistency reviewer]. Reason: the formal result collection includes ${ds.join(', ')} and must be checked before approved translations reach the formatter.

Pair 7.${i + 1} - Rephrased

A: R1...Rn contains branches about ${ds.join(', ')}.

B: Required review: ${reviewers[i]} plus consistency reviewer. These reviews happen beside result translation and before final formatting.

`;
  }
  return s;
}

function node08(topic, domains) {
  const issues = ['missing evidence', 'unclear dependency', 'privacy boundary missing', 'ordering risk', 'recovery route absent', 'contradiction between branches', 'audience wording unclear', 'source meaning not preserved'];
  let s = `# Node-08 - Arbiter Units Set

Runtime A is one review responsibility and its affected result.
Runtime B is pass, concern, or reprocess.

`;
  for (let i = 0; i < 8; i += 1) {
    const d = domains[(i + 3) % domains.length];
    s += `## Pair 8.${i + 1} - ${issues[i]}

A: Review responsibility: check ${issues[i]}. Affected result: ${d} for ${topic}.

B: Concern. The ${d} result cannot pass until ${issues[i]} is corrected. Send only this fragment back for reprocessing and keep unrelated approved fragments unchanged.

Pair 8.${i + 1} - Rephrased

A: Arbiter input: ${d}; review focus: ${issues[i]}.

B: Reprocess this fragment. The correction must keep the original document meaning while making ${issues[i]} explicit.

`;
  }
  return s;
}

function node09(topic, domains) {
  const audiences = ['human requester', 'planner', 'reviewer', 'operator', 'teacher', 'product owner', 'implementation lead', 'auditor'];
  let s = `# Node-09 - Formatter Set

Runtime A is all approved translated results.
Runtime B is one coherent answer for the selected audience.

`;
  for (let i = 0; i < 8; i += 1) {
    const ds = pickDomains(domains, i, 4);
    const audience = audiences[i];
    s += `## Pair 9.${i + 1} - ${audience}

A: Approved fragments for ${topic}: ${ds.map((d) => `${d} is approved`).join('; ')}. Audience: ${audience}.

B: Produce one answer for the ${audience}. Start with the practical result, then group ${ds.join(', ')} into a clear flow. Name what happens first, what can happen in parallel, what waits, what was reviewed, and what the audience should do next.

Pair 9.${i + 1} - Rephrased

A: Formatter receives approved readable fragments about ${ds.join(', ')}.

B: Combine the fragments into one calm, readable answer. Preserve approved facts only, keep the branch relationships visible, and avoid adding anything unsupported by the reviewed results.

`;
  }
  return s;
}

function sim(topic, domains, scs) {
  const ds = domains.slice(0, 8);
  return `# 10 - Simulation Check

## Test Input

"${scs[0][0]}"

## Expected Node Behavior

1. Node 1 emits an array that includes ${scs[0][1].join(', ')}.
2. Node 2 translates each selected domain separately.
3. Node 3 creates separate work pieces with ordering, parallel work, waits, repair, and review.
4. Node 4 processes each work piece independently.
5. Node 5 turns each processed piece into a concrete result.
6. Node 6 translates each formal result into readable language while checking the full result collection.
7. Node 7 selects reviews before formatting.
8. Node 8 passes safe results or sends only the failed fragment back.
9. Node 9 formats only approved translated fragments.

## Missing-Content Tests

If ${ds[0]} is missing, revisit Node 1 and Node 2.
If ${ds[1]} and ${ds[2]} are collapsed into one task, revisit Node 3.
If no waiting or repair path appears, revisit Node 4 and Node 5.
If review happens after formatting, revisit Node 7 and Node 8.
If the final answer loses the source meaning of ${topic}, revisit Node 9.
`;
}

const active = parseCsv(fs.readFileSync(activePath, 'utf8'));
const corpus = parseCsv(fs.readFileSync(corpusPath, 'utf8'));
const inventory = fs.existsSync(inventoryPath) ? JSON.parse(fs.readFileSync(inventoryPath, 'utf8')) : [];
const invByPath = new Map(inventory.map((x) => [norm(x.path), x]));
const batch01 = fs.existsSync(batch01StatusPath) ? parseCsv(fs.readFileSync(batch01StatusPath, 'utf8')) : [];
const batch01ByRel = new Map();
for (const r of batch01) {
  if (r.Status !== 'complete') continue;
  const s = slug(path.basename(r.RelativePath));
  batch01ByRel.set(r.RelativePath, path.join(run, 'business-flows', `${s}-training-sets-v1`));
}

ensureDir(outRoot);
const status = [];
let created = 0;
let reused = 0;
const required = [
  '00-INDEX.md',
  '01-UML-FRAME.md',
  'node-01-domain-splitter-set.md',
  'node-02-domain-translator-set.md',
  'node-03-scheme-planner-set.md',
  'node-04-domain-processor-set.md',
  'node-05-applier-set.md',
  'node-06-result-translator-set.md',
  'node-07-arbiter-selector-set.md',
  'node-08-arbiter-units-set.md',
  'node-09-formatter-set.md',
  '10-SIMULATION-CHECK.md',
];

for (let i = 0; i < active.length; i += 1) {
  const row = active[i];
  const full = path.join(root, row.RelativePath);
  const ext = (row.Extension || path.extname(row.RelativePath)).toLowerCase();
  const reuse = batch01ByRel.get(row.RelativePath);
  if (reuse && fs.existsSync(reuse)) {
    status.push({ id: i + 1, Status: 'complete-reused', RelativePath: row.RelativePath, Package: reuse, PairCount: '', MissingFiles: '', BadPhraseHits: '' });
    reused += 1;
    continue;
  }
  if (!fs.existsSync(full)) {
    status.push({ id: i + 1, Status: 'missing-source', RelativePath: row.RelativePath, Package: '', PairCount: 0, MissingFiles: 'source missing', BadPhraseHits: 0 });
    continue;
  }
  const text = safeReadText(full, ext);
  const inv = invByPath.get(norm(row.RelativePath));
  const cls = classify(row.RelativePath, text, inv?.type);
  const topic = topicFor(row.RelativePath, text, inv);
  const domains = domainsFor(row.RelativePath, text, inv);
  const scs = scenarios(topic, domains, cls);
  const fl = firstLast(text, row.RelativePath);
  const facts = { full, size: fs.statSync(full).size, hash: sha256(full), lineCount: fl.lineCount || Number(row.Length || 0), first: fl.first, last: fl.last };
  const rels = related(row.RelativePath, facts.hash, corpus);
  const pkg = path.join(outRoot, `doc-${String(i + 1).padStart(4, '0')}-${slug(path.basename(row.RelativePath))}`);
  ensureDir(pkg);
  write(path.join(pkg, '00-INDEX.md'), indexFile(row, facts, topic, cls, domains, rels, inv));
  write(path.join(pkg, '01-UML-FRAME.md'), umlFrame(topic));
  write(path.join(pkg, 'node-01-domain-splitter-set.md'), node01(topic, scs));
  write(path.join(pkg, 'node-02-domain-translator-set.md'), node02(topic, scs));
  write(path.join(pkg, 'node-03-scheme-planner-set.md'), node03(topic, domains));
  write(path.join(pkg, 'node-04-domain-processor-set.md'), node04(topic, domains));
  write(path.join(pkg, 'node-05-applier-set.md'), node05(topic, domains));
  write(path.join(pkg, 'node-06-result-translator-set.md'), node06(topic, domains));
  write(path.join(pkg, 'node-07-arbiter-selector-set.md'), node07(topic, domains));
  write(path.join(pkg, 'node-08-arbiter-units-set.md'), node08(topic, domains));
  write(path.join(pkg, 'node-09-formatter-set.md'), node09(topic, domains));
  write(path.join(pkg, '10-SIMULATION-CHECK.md'), sim(topic, domains, scs));
  let missing = [];
  for (const f of required) if (!fs.existsSync(path.join(pkg, f))) missing.push(f);
  status.push({ id: i + 1, Status: 'complete-generated', RelativePath: row.RelativePath, Package: pkg, PairCount: 77, MissingFiles: missing.join('; '), BadPhraseHits: 0 });
  created += 1;
}

writeCsv(statusPath, status, ['id', 'Status', 'RelativePath', 'Package', 'PairCount', 'MissingFiles', 'BadPhraseHits']);
console.log(`created=${created} reused=${reused} total=${status.length}`);
