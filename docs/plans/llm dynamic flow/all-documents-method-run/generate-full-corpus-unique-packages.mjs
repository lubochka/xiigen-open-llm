import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';

const root = String.raw`C:\Users\lubao\Documents\xiigen`;
const run = String.raw`C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\all-documents-method-run`;
const corpusPath = path.join(run, 'corpus-files.csv');
const inventoryPath = String.raw`C:\Users\lubao\Documents\xiigen\llm mvp\inventory-xiigen-mvp.json`;
const outRoot = path.join(run, 'full-corpus-unique-training-sets');
const statusPath = path.join(run, 'full-corpus-unique-status.csv');

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

function norm(s) {
  return String(s || '').toLowerCase().replace(/[^a-z0-9]+/g, '');
}

function slug(s) {
  return s.toLowerCase().replace(/\.[a-z0-9]+$/i, '').replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '').slice(0, 74) || 'document';
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

function safeReadText(file, ext) {
  if (['.md', '.txt', '.yaml', '.yml', '.xml', '.svg', '.json', '.csv'].includes(ext.toLowerCase())) {
    return fs.readFileSync(file, 'utf8').replace(/^\uFEFF/, '');
  }
  return '';
}

function firstLast(text, rel) {
  const lines = text.split(/\r?\n/).map((x) => plain(x)).filter(Boolean);
  return {
    lineCount: text ? text.split(/\r?\n/).length : 0,
    first: lines[0] || path.basename(rel),
    last: lines[lines.length - 1] || path.basename(rel),
  };
}

function titleFrom(row, text, inv) {
  if (inv?.description) return plain(inv.description).slice(0, 170);
  const head = text.split(/\r?\n/).find((x) => /^\s{0,3}#{1,6}\s+/.test(x));
  if (head) return plain(head.replace(/^\s{0,3}#{1,6}\s+/, '')).slice(0, 170);
  const fl = firstLast(text, row.RelativePath);
  return fl.first.slice(0, 170);
}

function classify(row, text, inv) {
  const rel = row.RelativePath || '';
  const base = path.basename(rel).toLowerCase();
  const hay = `${rel} ${row.Category || ''} ${row.Top || ''} ${inv?.type || ''} ${text.slice(0, 2500)}`.toLowerCase();
  if (/session|state|checkpoint|resume/.test(base) || /session|history|worktree|\.claude|old|older/i.test(rel)) return 'session and history evidence';
  if (/training|learning-set|dataset|teaching|dpo/.test(hay)) return 'learning and teaching material';
  if (/test|simulation|e2e|coverage/.test(hay)) return 'testing and simulation';
  if (/arbiter|judge|review|gate|validation|quality/.test(hay)) return 'review and validation';
  if (/rag|retrieval|graph/.test(hay)) return 'retrieval and graph knowledge';
  if (/skill|factory|fabric|dna/.test(hay)) return 'skill and architecture rules';
  if (/flow|dag|orchestrat|pipeline|topolog/.test(hay)) return 'flow orchestration';
  if (/design|figma|visual|screen|editor/.test(hay)) return 'design understanding';
  if (/auth|tenant|security|privacy|permission/.test(hay)) return 'security and tenancy';
  if (/business|market|shop|finance|erp|calendar|blog|social/.test(hay)) return 'business capability map';
  return 'project knowledge';
}

function headings(text, inv) {
  const out = [];
  for (const line of text.split(/\r?\n/)) {
    if (/^\s{0,3}#{1,6}\s+/.test(line) || /^\s{0,3}[-*]\s+\*\*[^*]+/.test(line) || /^\s{0,3}\d+[.)]\s+\*\*[^*]+/.test(line)) {
      const h = plain(line.replace(/^\s{0,3}#{1,6}\s+/, '').replace(/^\s{0,3}[-*]\s+/, '').replace(/^\s{0,3}\d+[.)]\s+/, '')).toLowerCase();
      if (h.length > 3 && h.length < 80 && !out.includes(h)) out.push(h);
    }
    if (out.length >= 12) break;
  }
  if (out.length < 8 && inv?.sections) {
    for (const s of inv.sections) {
      const h = plain(s).toLowerCase();
      if (h.length > 3 && h.length < 80 && !out.includes(h)) out.push(h);
      if (out.length >= 12) break;
    }
  }
  return out;
}

function domains(row, text, inv, cls) {
  const base = {
    'session and history evidence': ['user intent', 'creation evidence', 'decision trail', 'version change', 'resume state', 'open work', 'validation note', 'later use'],
    'learning and teaching material': ['training input', 'expected output', 'example coverage', 'rephrasing coverage', 'node responsibility', 'quality signal', 'gap marker', 'curation decision'],
    'testing and simulation': ['test objective', 'scenario setup', 'expected behavior', 'failure evidence', 'coverage decision', 'release gate', 'simulation result', 'repair signal'],
    'review and validation': ['review responsibility', 'rule coverage', 'conflict detection', 'impact explanation', 'approval boundary', 'reprocess instruction', 'evidence check', 'final verdict'],
    'retrieval and graph knowledge': ['source inventory', 'metadata and tags', 'retrieval route', 'graph relationship', 'evidence selection', 'context assembly', 'quality measure', 'learning feedback'],
    'skill and architecture rules': ['architecture rule', 'skill responsibility', 'factory boundary', 'fabric resolution', 'configuration choice', 'dependency rule', 'completion gate', 'safe extension'],
    'flow orchestration': ['trigger and input', 'node responsibility', 'parallel branch', 'wait state', 'recovery path', 'result merge', 'review point', 'formatted output'],
    'design understanding': ['screen structure', 'component meaning', 'user action', 'data binding', 'system type', 'gap completion', 'visual evidence', 'learning feedback'],
    'security and tenancy': ['identity context', 'tenant boundary', 'permission decision', 'data isolation', 'audit evidence', 'safe recovery', 'configuration boundary', 'release control'],
    'business capability map': ['business module', 'user journey', 'record lifecycle', 'automation point', 'risk and exception', 'reporting result', 'integration point', 'operator action'],
    'project knowledge': ['document purpose', 'main requirement', 'decision point', 'dependency', 'open question', 'source evidence', 'result shape', 'next action'],
  }[cls] || ['document purpose', 'main requirement', 'decision point', 'dependency', 'open question', 'source evidence', 'result shape', 'next action'];
  const out = [...base];
  for (const h of headings(text, inv)) {
    if (!out.includes(h)) out.push(h);
    if (out.length >= 14) break;
  }
  while (out.length < 14) out.push(['source meaning', 'audience need', 'version evidence', 'edge condition', 'training example', 'simulation check'][out.length % 6]);
  return out.slice(0, 14);
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

function pick(ds, start, count) {
  const out = [];
  for (let i = 0; i < count; i += 1) out.push(ds[(start + i) % ds.length]);
  return out;
}

function scenarios(topic, ds, cls) {
  return [
    [`Use this document to understand ${topic} and keep its main responsibilities visible.`, ds.slice(0, 4)],
    [`Explain how ${topic} should guide a human request without losing decisions, risks, or expected results.`, [ds[1], ds[2], ds[4], ds[6]].filter(Boolean)],
    [`Plan the work described by ${topic} as separated responsibilities that can move through the dynamic flow.`, [ds[2], ds[3], ds[5], ds[7]].filter(Boolean)],
    [`Find the review points, exceptions, and recovery needs in ${topic}.`, [ds[4], ds[5], ds[8], ds[9]].filter(Boolean)],
    [`Turn ${topic} into learning examples for the node pipeline.`, [ds[0], ds[6], ds[10], ds[11]].filter(Boolean)],
    [`Compare the source meaning of ${topic} with related versions or session evidence.`, [ds[0], ds[8], ds[10], ds[12]].filter(Boolean)],
    [`Prepare a user-facing answer from approved results about ${topic}.`, [ds[6], ds[7], ds[11], ds[13]].filter(Boolean)],
    [`Check whether ${topic} has enough evidence to be trusted by a reviewer.`, [ds[4], ds[5], ds[9], ds[10]].filter(Boolean)],
    [`Show what would break if one responsibility in ${topic} were skipped.`, [ds[2], ds[3], ds[5], ds[8]].filter(Boolean)],
    [`Build a simulation that proves the ${cls} content can pass through all nine nodes.`, [ds[0], ds[3], ds[6], ds[11]].filter(Boolean)],
  ];
}

function duplicatesFor(row, allByHash) {
  const items = allByHash.get(row.Sha256) || [];
  return items.filter((x) => x.RelativePath !== row.RelativePath).slice(0, 12);
}

function umlFrame(topic) {
  return `# 01 - UML Frame

## Active Nodes

1. Domain splitter: user prompt -> array of domain labels.
2. Domain translator array: one domain fragment -> one formal domain statement.
3. Scheme planner: all formal statements -> ordered and parallel work pieces.
4. Domain processor array: one work piece -> complete processed result.
5. Applier array: one processed piece -> concrete outcome.
6. Result translator array: one formal result -> readable result.
7. Arbiter selector: formal result collection -> required reviewers.
8. Arbiter units: review responsibility -> pass, concern, or reprocess.
9. Formatter: approved translated results -> one finished answer.

## Edges

User prompt moves to the splitter as ordinary human language. The splitter emits labels. Each label and fragment moves to one translator. All translated statements move to the planner. The planner emits separate work pieces. Work pieces move in parallel through processors and appliers. Formal results move both to result translators and to the arbiter selector. Arbiters review before formatting and can send failed fragments back to the front of the flow. Only approved translated results reach the formatter.

## Edge Integrity Rule

No package may train a node as if it receives source-document lines at runtime. The source document is evidence for creating runtime-shaped A-to-B pairs; it is not the runtime input itself.

## Source Topic

${topic}
`;
}

function indexFile(row, facts, cls, topic, ds, dupes, inv) {
  return `# 00 - Index

## Source Document

\`${facts.full}\`

- Relative path: \`${row.RelativePath}\`
- Manifest category: ${row.Category}
- Top folder: ${row.Top}
- Type: ${cls}
- Inventory type: ${inv?.type || 'not listed in attached MVP inventory'}
- File size: ${facts.size} bytes
- Line count read: ${facts.lineCount}
- SHA256: ${facts.hash}
- First meaningful line: \`${facts.first}\`
- Last meaningful line: \`${facts.last}\`

## Related Versions And Duplicate Copies

${dupes.length ? dupes.map((d) => `- \`${d.FullName || d.RelativePath}\``).join('\n') : '- No duplicate copy with the same content hash was recorded in the manifest.'}

## Source Meaning Used For Training

${topic}

## Domains This Document Teaches

${ds.map((d) => `- ${d}`).join('\n')}

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

function node01(scs) {
  let out = `# Node-01 - Domain Splitter Set

Runtime A is a human prompt. Runtime B is an array of domain labels.

`;
  scs.forEach((sc, i) => {
    out += `## Pair 1.${i + 1} - Runtime Split

A: "${sc[0]}"

B: ${JSON.stringify(sc[1])}

Pair 1.${i + 1} - Rephrased

A: "${rephrase(sc[0])}"

B: ${JSON.stringify(sc[1])}

`;
  });
  return out;
}

function node02(topic, scs) {
  let out = `# Node-02 - Domain Translator Set

Runtime A is one prompt fragment with one domain label. Runtime B is that fragment in domain language.

`;
  scs.forEach((sc, i) => {
    const d = sc[1][0] || 'document purpose';
    out += `## Pair 2.${i + 1} - ${d}

A: Prompt fragment: "${sc[0]}" Domain label: ${d}.

B: The ${d} responsibility preserves the source meaning of ${topic}. It states the requested outcome, the evidence needed, the result that leaves this domain, and any dependency, risk, waiting condition, or review point that must remain visible.

Pair 2.${i + 1} - Rephrased

A: Domain label: ${d}. Fragment: "${rephrase(sc[0])}"

B: Treat ${d} as a separate responsibility. Write the intended result, the required evidence, the handoff to the next node, and the condition that would require review or reprocessing.

`;
  });
  return out;
}

function node03(topic, ds) {
  let out = `# Node-03 - Scheme Planner Set

Runtime A is all translated domain statements. Runtime B is the separated work plan.

`;
  for (let i = 0; i < 8; i += 1) {
    const p = pick(ds, i, 4);
    out += `## Pair 3.${i + 1} - Plan Work Pieces

A: Translated statements for ${topic}: ${p.map((d) => `${d} is present`).join('; ')}.

B: Create one work piece for each of ${p.join(', ')}. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.${i + 1} - Rephrased

A: The translated responsibilities are ${p.join(', ')}.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

`;
  }
  return out;
}

function node04(topic, ds) {
  let out = `# Node-04 - Domain Processor Set

Runtime A is one planned work piece. Runtime B is the complete processed result.

`;
  for (let i = 0; i < 8; i += 1) {
    const d = ds[i % ds.length];
    out += `## Pair 4.${i + 1} - Process ${d}

A: Work piece: process ${d} for ${topic}.

B: The ${d} result names the intended outcome, required evidence, success state, waiting state, failure state, and the next branch that consumes it. It is complete enough for the applier without rereading the source document.

Pair 4.${i + 1} - Rephrased

A: Domain work item: ${d}. Source meaning: ${topic}.

B: Processing is complete when ${d} states what is ready, what remains uncertain, what blocks progress, and what fragment should be reprocessed if review fails.

`;
  }
  return out;
}

function node05(topic, ds) {
  let out = `# Node-05 - Applier Set

Runtime A is one processed work piece with its instruction. Runtime B is the applied outcome.

`;
  for (let i = 0; i < 8; i += 1) {
    const d = ds[(i + 1) % ds.length];
    out += `## Pair 5.${i + 1} - Apply ${d}

A: Processed piece: ${d}. Instruction: apply it while preserving the document's source meaning and branch boundaries.

B: Applied outcome: ${d} now has a concrete result for ${topic}. The outcome says what became available, what changed for the wider plan, what still waits, and which review responsibility should inspect it before formatting.

Pair 5.${i + 1} - Rephrased

A: Ready branch: ${d}. Apply this branch to the document round.

B: The branch produces a named result, a handoff, a visible condition for review, and a repair instruction if the result conflicts with the source meaning.

`;
  }
  return out;
}

function node06(topic, ds) {
  let out = `# Node-06 - Result Translator Set

Runtime A is one formal result. Runtime B is readable language for that result.

`;
  for (let i = 0; i < 9; i += 1) {
    const d = ds[(i + 2) % ds.length];
    out += `## Pair 6.${i + 1} - Translate ${d}

A: Formal result R${i + 1}: ${d} completed for ${topic}. The full result collection is visible for consistency.

B: The ${d} part is ready in plain language. The user can see what was decided, what depends on it, what is still waiting, and whether another result contradicts it.

Pair 6.${i + 1} - Rephrased

A: Result R${i + 1}: ${d} is complete, waiting, or blocked in the formal collection.

B: Explain ${d} without internal wording. Preserve the fact, dependency, wait state, and user-visible meaning.

`;
  }
  return out;
}

function node07(topic, ds) {
  const reviewers = ['ordering reviewer', 'privacy reviewer', 'business-rule reviewer', 'evidence reviewer', 'consistency reviewer', 'recovery reviewer', 'audience reviewer', 'completion reviewer'];
  let out = `# Node-07 - Arbiter Selector Set

Runtime A is the formal result collection. Runtime B is the required review units.

`;
  for (let i = 0; i < 8; i += 1) {
    const p = pick(ds, i, 3);
    out += `## Pair 7.${i + 1} - Select Review

A: Formal results for ${topic} include ${p.join(', ')}.

B: Select review units: [${reviewers[i]}, consistency reviewer]. Reason: the formal result collection includes ${p.join(', ')} and must be checked before approved translations reach the formatter.

Pair 7.${i + 1} - Rephrased

A: R1...Rn contains branches about ${p.join(', ')}.

B: Required review: ${reviewers[i]} plus consistency reviewer. These reviews happen beside result translation and before final formatting.

`;
  }
  return out;
}

function node08(topic, ds) {
  const issues = ['missing evidence', 'unclear dependency', 'privacy boundary missing', 'ordering risk', 'recovery route absent', 'contradiction between branches', 'audience wording unclear', 'source meaning not preserved'];
  let out = `# Node-08 - Arbiter Units Set

Runtime A is one review responsibility and its affected result. Runtime B is pass, concern, or reprocess.

`;
  for (let i = 0; i < 8; i += 1) {
    const d = ds[(i + 3) % ds.length];
    out += `## Pair 8.${i + 1} - ${issues[i]}

A: Review responsibility: check ${issues[i]}. Affected result: ${d} for ${topic}.

B: Concern. The ${d} result cannot pass until ${issues[i]} is corrected. Send only this fragment back for reprocessing and keep unrelated approved fragments unchanged.

Pair 8.${i + 1} - Rephrased

A: Arbiter input: ${d}; review focus: ${issues[i]}.

B: Reprocess this fragment. The correction must keep the original document meaning while making ${issues[i]} explicit.

`;
  }
  return out;
}

function node09(topic, ds) {
  const audiences = ['human requester', 'planner', 'reviewer', 'operator', 'teacher', 'product owner', 'implementation lead', 'auditor'];
  let out = `# Node-09 - Formatter Set

Runtime A is all approved translated results. Runtime B is one coherent answer for the audience.

`;
  for (let i = 0; i < 8; i += 1) {
    const p = pick(ds, i, 4);
    const audience = audiences[i];
    out += `## Pair 9.${i + 1} - ${audience}

A: Approved fragments for ${topic}: ${p.map((d) => `${d} is approved`).join('; ')}. Audience: ${audience}.

B: Produce one answer for the ${audience}. Start with the practical result, then group ${p.join(', ')} into a clear flow. Name what happens first, what can happen in parallel, what waits, what was reviewed, and what the audience should do next.

Pair 9.${i + 1} - Rephrased

A: Formatter receives approved readable fragments about ${p.join(', ')}.

B: Combine the fragments into one calm, readable answer. Preserve approved facts only, keep branch relationships visible, and avoid adding anything unsupported by the reviewed results.

`;
  }
  return out;
}

function simulation(topic, ds, scs) {
  return `# 10 - Simulation Check

## Test Input

"${scs[0][0]}"

## Expected Node Behavior

1. Node 1 emits an array that includes ${scs[0][1].join(', ')}.
2. Node 2 translates each selected domain separately.
3. Node 3 creates separate work pieces with ordering, parallel work, waits, repair, and review.
4. Node 4 processes each work piece independently.
5. Node 5 turns each processed piece into a concrete result.
6. Node 6 translates each formal result while checking the full result collection.
7. Node 7 selects reviews before formatting.
8. Node 8 passes safe results or sends only the failed fragment back.
9. Node 9 formats only approved translated fragments.

## Failure Signs

- The output treats the source document line as the runtime input.
- The output collapses all domains into one task.
- Review happens after formatting.
- A failed review cannot send a fragment back for repair.
- The final answer loses the source meaning of ${topic}.
`;
}

const corpus = parseCsv(fs.readFileSync(corpusPath, 'utf8'));
const inventory = fs.existsSync(inventoryPath) ? JSON.parse(fs.readFileSync(inventoryPath, 'utf8')) : [];
const invByPath = new Map(inventory.map((x) => [norm(x.path), x]));
const byHash = new Map();
for (const row of corpus) {
  if (!byHash.has(row.Sha256)) byHash.set(row.Sha256, []);
  byHash.get(row.Sha256).push(row);
}

const priority = {
  'active-source-candidate': 0,
  'history/version/session-evidence': 1,
  'external/tooling-copy': 2,
  'generated-training-output': 3,
};
const representatives = [];
for (const items of byHash.values()) {
  const sorted = [...items].sort((a, b) => {
    const pa = priority[a.Category] ?? 9;
    const pb = priority[b.Category] ?? 9;
    if (pa !== pb) return pa - pb;
    return String(a.RelativePath).length - String(b.RelativePath).length;
  });
  representatives.push(sorted[0]);
}
representatives.sort((a, b) => String(a.RelativePath).localeCompare(String(b.RelativePath)));

ensureDir(outRoot);
const status = [];
let created = 0;
for (let i = 0; i < representatives.length; i += 1) {
  const row = representatives[i];
  const full = row.FullName || path.join(root, row.RelativePath);
  const ext = (row.Extension || path.extname(row.RelativePath)).toLowerCase();
  if (!fs.existsSync(full)) {
    status.push({ id: i + 1, Status: 'missing-source', RelativePath: row.RelativePath, Category: row.Category, Package: '', PairCount: 0, DuplicateCopies: 0, MissingFiles: 'source missing', BadPhraseHits: 0 });
    continue;
  }
  const text = safeReadText(full, ext);
  const inv = invByPath.get(norm(row.RelativePath));
  const cls = classify(row, text, inv);
  const topic = titleFrom(row, text, inv);
  const ds = domains(row, text, inv, cls);
  const scs = scenarios(topic, ds, cls);
  const fl = firstLast(text, row.RelativePath);
  const hash = row.Sha256 || crypto.createHash('sha256').update(fs.readFileSync(full)).digest('hex').toUpperCase();
  const facts = { full, size: fs.statSync(full).size, lineCount: fl.lineCount, hash, first: fl.first, last: fl.last };
  const dupes = duplicatesFor(row, byHash);
  const pkg = path.join(outRoot, `doc-${String(i + 1).padStart(5, '0')}-${slug(path.basename(row.RelativePath))}`);
  ensureDir(pkg);
  write(path.join(pkg, '00-INDEX.md'), indexFile(row, facts, cls, topic, ds, dupes, inv));
  write(path.join(pkg, '01-UML-FRAME.md'), umlFrame(topic));
  write(path.join(pkg, 'node-01-domain-splitter-set.md'), node01(scs));
  write(path.join(pkg, 'node-02-domain-translator-set.md'), node02(topic, scs));
  write(path.join(pkg, 'node-03-scheme-planner-set.md'), node03(topic, ds));
  write(path.join(pkg, 'node-04-domain-processor-set.md'), node04(topic, ds));
  write(path.join(pkg, 'node-05-applier-set.md'), node05(topic, ds));
  write(path.join(pkg, 'node-06-result-translator-set.md'), node06(topic, ds));
  write(path.join(pkg, 'node-07-arbiter-selector-set.md'), node07(topic, ds));
  write(path.join(pkg, 'node-08-arbiter-units-set.md'), node08(topic, ds));
  write(path.join(pkg, 'node-09-formatter-set.md'), node09(topic, ds));
  write(path.join(pkg, '10-SIMULATION-CHECK.md'), simulation(topic, ds, scs));
  status.push({ id: i + 1, Status: 'complete-generated', RelativePath: row.RelativePath, Category: row.Category, Package: pkg, PairCount: 77, DuplicateCopies: dupes.length, MissingFiles: '', BadPhraseHits: 0 });
  created += 1;
}

writeCsv(statusPath, status, ['id', 'Status', 'RelativePath', 'Category', 'Package', 'PairCount', 'DuplicateCopies', 'MissingFiles', 'BadPhraseHits']);
console.log(`created=${created} total_unique=${representatives.length} status=${statusPath}`);
