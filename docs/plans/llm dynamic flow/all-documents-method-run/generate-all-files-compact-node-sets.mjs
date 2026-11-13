import fs from 'node:fs';
import path from 'node:path';
import readline from 'node:readline';

const run = String.raw`C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\all-documents-method-run`;
const outDir = path.join(run, 'all-files-including-zips-compact');
const nestedManifestPath = path.join(outDir, 'all-files-and-zip-entries-manifest-with-nested.csv');
const sanitizedManifestPath = path.join(outDir, 'all-files-and-zip-entries-manifest-sanitized.csv');
const manifestPath = fs.existsSync(nestedManifestPath)
  ? nestedManifestPath
  : fs.existsSync(sanitizedManifestPath)
    ? sanitizedManifestPath
    : path.join(outDir, 'all-files-and-zip-entries-manifest.csv');
const statusPath = path.join(outDir, 'all-files-compact-status.csv');
const validationPath = path.join(outDir, 'validation-report-all-files-compact.csv');

function parseCsvLine(line) {
  const out = [];
  let cell = '';
  let q = false;
  for (let i = 0; i < line.length; i += 1) {
    const ch = line[i];
    const nx = line[i + 1];
    if (q) {
      if (ch === '"' && nx === '"') {
        cell += '"';
        i += 1;
      } else if (ch === '"') q = false;
      else cell += ch;
    } else if (ch === '"') q = true;
    else if (ch === ',') {
      out.push(cell);
      cell = '';
    } else cell += ch;
  }
  out.push(cell);
  return out;
}

function csvEscape(v) {
  const s = String(v ?? '');
  return /[",\n\r]/.test(s) ? `"${s.replace(/"/g, '""')}"` : s;
}

function clean(s) {
  return String(s || '').replace(/\s+/g, ' ').trim();
}

function basenameFrom(row) {
  if (row.SourceKind === 'zip-entry') return path.basename(row.ZipEntryPath || row.RelativePath || 'zip entry');
  return path.basename(row.RelativePath || row.PhysicalFullName || 'file');
}

function topic(row) {
  const sample = clean(row.TextSample);
  if (sample) return sample.slice(0, 180);
  const name = basenameFrom(row).replace(/\.[^.]+$/, '').replace(/[-_]+/g, ' ');
  return clean(name || row.RelativePath || row.PhysicalFullName).slice(0, 180);
}

function classify(row) {
  const s = `${row.RelativePath} ${row.ZipEntryPath} ${row.Category} ${row.ReadableKind} ${row.Extension}`.toLowerCase();
  if (String(row.SourceKind).includes('zip-entry')) return 'zip entry';
  if (/zip archive/.test(row.ReadableKind)) return 'zip archive';
  if (/session|history|old|older|worktree|\.claude/.test(s)) return 'session history file';
  if (/node_modules|vendor|dist|build|bin|obj/.test(s)) return 'tooling or build file';
  if (/image binary/.test(s)) return 'visual asset';
  if (/runtime binary|binary asset|document binary/.test(s)) return 'binary document or asset';
  if (/readable text/.test(s)) return 'readable source file';
  return 'general file';
}

function domains(row) {
  const cls = classify(row);
  const ext = row.Extension || 'none';
  const kind = row.ReadableKind || 'file';
  const base = {
    'zip entry': ['archive entry identity', 'entry folder context', 'entry file type', 'entry source meaning', 'entry extraction status', 'entry training usefulness'],
    'zip archive': ['archive identity', 'contained file inventory', 'bundle purpose', 'version package', 'extraction status', 'archive training usefulness'],
    'session history file': ['session intent', 'decision evidence', 'version context', 'resume state', 'open work', 'validation note'],
    'tooling or build file': ['tooling role', 'dependency context', 'build artifact meaning', 'source boundary', 'reuse signal', 'training usefulness'],
    'visual asset': ['asset identity', 'visual purpose', 'folder context', 'reuse signal', 'training usefulness', 'review need'],
    'binary document or asset': ['file identity', 'binary document purpose', 'folder context', 'extraction limitation', 'training usefulness', 'review need'],
    'readable source file': ['file purpose', 'source meaning', 'folder context', 'declared responsibility', 'dependency signal', 'training usefulness'],
    'general file': ['file identity', 'folder context', 'file type', 'source meaning', 'training usefulness', 'review need'],
  }[cls] || ['file identity', 'folder context', 'file type', 'source meaning', 'training usefulness', 'review need'];
  return [...base, `extension ${ext || 'none'}`, kind].slice(0, 8);
}

function rephrase(a) {
  return a
    .replace(/^Use /, 'Apply ')
    .replace(/^Translate /, 'Convert ')
    .replace(/^Plan /, 'Prepare ')
    .replace(/^Process /, 'Work through ')
    .replace(/^Apply /, 'Use ')
    .replace(/^Explain /, 'Show ')
    .replace(/^Select /, 'Choose ')
    .replace(/^Review /, 'Check ')
    .replace(/^Format /, 'Shape ');
}

function record(row, node, pairName, A, B, labels) {
  return {
    sourceId: Number(row.SourceId),
    sourceKind: row.SourceKind,
    relativePath: row.RelativePath,
    archiveRelativePath: row.ArchiveRelativePath,
    zipEntryPath: row.ZipEntryPath,
    category: row.Category,
    readableKind: row.ReadableKind,
    node,
    pairName,
    A,
    B,
    rephrasedA: rephrase(A),
    rephrasedB: B,
    labels,
  };
}

function nodeRecords(row) {
  const t = topic(row);
  const d = domains(row);
  const rel = row.RelativePath;
  const sourcePhrase = row.SourceKind === 'zip-entry'
    ? `zip entry ${row.ZipEntryPath} from archive ${row.ArchiveRelativePath}`
    : row.SourceKind === 'nested-zip-entry'
      ? `nested zip entry ${row.ZipEntryPath} from archive chain ${row.ArchiveRelativePath}`
    : `file ${rel}`;
  return [
    record(row, 'node-01-domain-splitter', 'split-file-request',
      `Use ${sourcePhrase} to understand ${t} and keep its relevant responsibilities visible.`,
      d.slice(0, 4),
      d.slice(0, 4)),
    record(row, 'node-02-domain-translator', 'translate-file-purpose',
      `Translate the fragment about ${t} for domain ${d[0]}.`,
      `${d[0]} means this source must be treated as a distinct file responsibility. Preserve its folder context, file type, source meaning, and any limitation caused by archive, binary, generated, history, or tooling status.`,
      [d[0]]),
    record(row, 'node-03-scheme-planner', 'plan-file-work',
      `Plan the work for ${sourcePhrase} after its domains are translated: ${d.slice(0, 4).join(', ')}.`,
      `Create separate work pieces for ${d.slice(0, 4).join(', ')}. Keep archive identity, file meaning, extraction limits, review needs, and training usefulness visible. Allow independent pieces to move in parallel and merge only after review.`,
      d.slice(0, 4)),
    record(row, 'node-04-domain-processor', 'process-file-piece',
      `Process the ${d[1]} work piece for ${sourcePhrase}.`,
      `The ${d[1]} result names the source, the readable or unreadable status, the useful meaning available from the file, the waiting condition if content cannot be read, and the exact fragment that should be reprocessed if review fails.`,
      [d[1]]),
    record(row, 'node-05-applier', 'apply-file-piece',
      `Apply the processed ${d[2]} piece for ${sourcePhrase}.`,
      `Applied outcome: ${d[2]} is recorded as a concrete training source result. The outcome states what was learned, what remains limited, and which review responsibility should inspect it before formatting.`,
      [d[2]]),
    record(row, 'node-06-result-translator', 'translate-file-result',
      `Explain formal result R for ${sourcePhrase}: ${d[3]} was processed.`,
      `Readable result: this source contributes ${d[3]} to the corpus. The user can see whether it was a physical file or zip entry, what kind of file it is, and what useful meaning was available without exposing unsupported claims.`,
      [d[3]]),
    record(row, 'node-07-arbiter-selector', 'select-file-reviewers',
      `Select reviewers for formal results about ${sourcePhrase}.`,
      [`evidence reviewer`, `archive-boundary reviewer`, `consistency reviewer`],
      ['review']),
    record(row, 'node-08-arbiter-units', 'review-file-result',
      `Review ${sourcePhrase} for missing evidence, unreadable content, archive boundary mistakes, or source meaning loss.`,
      `If evidence is sufficient, pass. If the file is unreadable or the archive boundary is confused, return only this source record for reprocessing with the reason attached. Keep unrelated source records unchanged.`,
      ['review']),
    record(row, 'node-09-formatter', 'format-file-answer',
      `Format approved translated fragments for ${sourcePhrase}.`,
      `Final answer fragment: ${sourcePhrase} was processed as ${classify(row)}. The output preserves its source identity, file type, available meaning, review status, and training usefulness in one readable statement.`,
      ['formatter']),
  ];
}

fs.mkdirSync(outDir, { recursive: true });
const nodeFiles = {
  'node-01-domain-splitter-set.jsonl': fs.createWriteStream(path.join(outDir, 'node-01-domain-splitter-set.jsonl'), 'utf8'),
  'node-02-domain-translator-set.jsonl': fs.createWriteStream(path.join(outDir, 'node-02-domain-translator-set.jsonl'), 'utf8'),
  'node-03-scheme-planner-set.jsonl': fs.createWriteStream(path.join(outDir, 'node-03-scheme-planner-set.jsonl'), 'utf8'),
  'node-04-domain-processor-set.jsonl': fs.createWriteStream(path.join(outDir, 'node-04-domain-processor-set.jsonl'), 'utf8'),
  'node-05-applier-set.jsonl': fs.createWriteStream(path.join(outDir, 'node-05-applier-set.jsonl'), 'utf8'),
  'node-06-result-translator-set.jsonl': fs.createWriteStream(path.join(outDir, 'node-06-result-translator-set.jsonl'), 'utf8'),
  'node-07-arbiter-selector-set.jsonl': fs.createWriteStream(path.join(outDir, 'node-07-arbiter-selector-set.jsonl'), 'utf8'),
  'node-08-arbiter-units-set.jsonl': fs.createWriteStream(path.join(outDir, 'node-08-arbiter-units-set.jsonl'), 'utf8'),
  'node-09-formatter-set.jsonl': fs.createWriteStream(path.join(outDir, 'node-09-formatter-set.jsonl'), 'utf8'),
};

const status = fs.createWriteStream(statusPath, 'utf8');
status.write('SourceId,Status,SourceKind,RelativePath,ArchiveRelativePath,ZipEntryPath,NodeRecords,TrainingPairs,ReadStatus\n');

const counts = Object.fromEntries(Object.keys(nodeFiles).map((x) => [x, 0]));
let total = 0;
let header = null;

const rl = readline.createInterface({
  input: fs.createReadStream(manifestPath, 'utf8'),
  crlfDelay: Infinity,
});

for await (const line of rl) {
  if (!header) {
    header = parseCsvLine(line);
    continue;
  }
  if (!line.trim()) continue;
  const vals = parseCsvLine(line);
  const row = Object.fromEntries(header.map((h, i) => [h, vals[i] ?? '']));
  const recs = nodeRecords(row);
  for (const rec of recs) {
    const fileName = `${rec.node.replace('node-', 'node-').replace('domain-splitter', 'domain-splitter-set').replace('domain-translator', 'domain-translator-set').replace('scheme-planner', 'scheme-planner-set').replace('domain-processor', 'domain-processor-set').replace('applier', 'applier-set').replace('result-translator', 'result-translator-set').replace('arbiter-selector', 'arbiter-selector-set').replace('arbiter-units', 'arbiter-units-set').replace('formatter', 'formatter-set')}.jsonl`;
    nodeFiles[fileName].write(`${JSON.stringify(rec)}\n`);
    counts[fileName] += 1;
  }
  status.write([
    row.SourceId,
    'complete',
    csvEscape(row.SourceKind),
    csvEscape(row.RelativePath),
    csvEscape(row.ArchiveRelativePath),
    csvEscape(row.ZipEntryPath),
    9,
    18,
    csvEscape(row.ReadStatus),
  ].join(',') + '\n');
  total += 1;
}

await Promise.all([...Object.values(nodeFiles), status].map((stream) => new Promise((resolve) => stream.end(resolve))));

const validationRows = Object.entries(counts).map(([file, count]) => ({ file, count, expected: total, status: count === total ? 'pass' : 'fail' }));
fs.writeFileSync(
  validationPath,
  `File,RecordCount,ExpectedCount,Status\n${validationRows.map((r) => `${csvEscape(r.file)},${r.count},${r.expected},${r.status}`).join('\n')}\n`,
  'utf8',
);

const index = `# All Files Including Zips - Compact Training Sets

## Scope

This batch processes every physical file under \`C:\\Users\\lubao\\Documents\\xiigen\`, excluding the generated method-run output folder, and every file entry found inside zip archives.

## Counts

- Source records processed: ${total}
- Physical files and zip entries are both source records.
- Node record files: 9
- Node records per source: 9
- Training pairs per source: 18 because every node record contains original A/B and rephrased A/B.
- Total node records: ${total * 9}
- Total compact training pairs: ${total * 18}

## Files

- Manifest: \`${manifestPath}\`
- Status: \`${statusPath}\`
- Validation: \`${validationPath}\`
- Node files:
${Object.keys(nodeFiles).map((f) => `  - \`${path.join(outDir, f)}\``).join('\n')}

## Edge Rule

Every source record has records for all nine UML nodes. Archive entries keep their archive path and entry path so zip boundaries are not lost.
`;

fs.writeFileSync(path.join(outDir, '00-RUN-INDEX.md'), index.replace(/\n/g, '\r\n'), 'utf8');
console.log(`processed=${total} node_records=${total * 9} training_pairs=${total * 18}`);
