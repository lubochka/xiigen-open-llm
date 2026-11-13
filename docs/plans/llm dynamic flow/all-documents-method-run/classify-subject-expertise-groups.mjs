import fs from 'node:fs';
import path from 'node:path';
import readline from 'node:readline';

const run = String.raw`C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\all-documents-method-run`;
const compact = path.join(run, 'all-files-including-zips-compact');
const manifest = path.join(compact, 'all-files-and-zip-entries-manifest-with-nested.csv');
const csvOut = path.join(compact, 'subject-expertise-groups.csv');
const jsonOut = path.join(compact, 'subject-expertise-groups-summary.json');

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

const groups = [
  {
    id: 'G01',
    name: 'Session, History, And Version Evidence',
    test: /session|history|old|older|worktree|\.claude|checkpoint|resume|state/i,
  },
  {
    id: 'G02',
    name: 'Flow Planning And Orchestration',
    test: /flow|dag|orchestrat|pipeline|topolog|node|branch|arbiter/i,
  },
  {
    id: 'G03',
    name: 'Architecture, Fabric, Factory, And Skill Rules',
    test: /architecture|fabric|factory|skill|dna|capability|microservice|interface|contract/i,
  },
  {
    id: 'G04',
    name: 'Learning, Teaching, Prompt, And Model Improvement',
    test: /learning|teaching|training|dataset|prompt|dpo|model|llm|ai-provider|judge/i,
  },
  {
    id: 'G05',
    name: 'Retrieval, Graph Knowledge, And Context Assembly',
    test: /rag|retrieval|graph|vector|embedding|context|source-index|knowledge/i,
  },
  {
    id: 'G06',
    name: 'Business Domains And Product Modules',
    test: /business|market|shop|finance|erp|calendar|blog|social|freelance|event|post|registration|onboarding|commerce|ads|sponsored|forms/i,
  },
  {
    id: 'G07',
    name: 'Design, Figma, Visual Editing, And UI Understanding',
    test: /design|figma|visual|screen|editor|canvas|component|layout|theme|ux|ui/i,
  },
  {
    id: 'G08',
    name: 'Frontend, Mobile, Web, And Client Rendering',
    test: /react|react-native|vue|angular|frontend|client|mobile|expo|tsx|jsx|css|html|web/i,
  },
  {
    id: 'G09',
    name: 'Backend, APIs, Services, And Providers',
    test: /backend|server|api|controller|service|provider|nestjs|nodejs|dotnet|\.cs|\.ts|\.js|java|go|rust|php/i,
  },
  {
    id: 'G10',
    name: 'Security, Authorization, Identity, And Tenancy',
    test: /auth|authorization|security|identity|tenant|permission|privacy|oauth|session|jwt|token|isolation/i,
  },
  {
    id: 'G11',
    name: 'Data, Database, Search, Cache, And Storage',
    test: /database|db|elastic|redis|cache|storage|sql|index|warehouse|schema|migration|entity|document/i,
  },
  {
    id: 'G12',
    name: 'Testing, Validation, Simulation, And Quality Gates',
    test: /test|e2e|simulation|validation|quality|coverage|gate|fixture|assert|spec/i,
  },
  {
    id: 'G13',
    name: 'DevOps, CI-CD, Environment, And Deployment',
    test: /devops|ci|cd|docker|kubernetes|deployment|environment|pipeline|backup|restore|provision|jenkins|azure/i,
  },
  {
    id: 'G14',
    name: 'WordPress, Plugin, Theme, And CMS Integration',
    test: /wordpress|plugin|theme|cms|drupal|php|shortcode|block/i,
  },
  {
    id: 'G15',
    name: 'Migration, Translation, And Multi-Stack Alternatives',
    test: /translate|translation|migration|alternative|nest|python|java|rust|go|vue|angular|reactjs|target/i,
  },
  {
    id: 'G16',
    name: 'External Tooling, Build Output, And Dependencies',
    test: /node_modules|vendor|dist|build|bin|obj|package-lock|pnpm-lock|yarn.lock|composer|target/i,
  },
  {
    id: 'G17',
    name: 'Binary Assets, Images, Documents, And Theme Media',
    test: /image binary|runtime binary|document binary|\.png|\.jpg|\.jpeg|\.gif|\.webp|\.ico|\.ttf|\.woff|\.pdf|\.docx|themeforest/i,
  },
  {
    id: 'G18',
    name: 'Archive Containers And Zip Boundary Handling',
    test: /zip archive|zip-entry|nested-zip-entry|\.zip/i,
  },
];

function classify(row) {
  const hay = `${row.SourceKind} ${row.RelativePath} ${row.ArchiveRelativePath} ${row.ZipEntryPath} ${row.Extension} ${row.Category} ${row.ReadableKind} ${row.TextSample}`;
  for (const g of groups) {
    if (g.test.test(hay)) return g;
  }
  return { id: 'G19', name: 'General Project Knowledge And Uncategorized Files' };
}

const counts = new Map();
const sourceKindCounts = new Map();
const examples = new Map();
let header = null;
let total = 0;

const rl = readline.createInterface({
  input: fs.createReadStream(manifest, 'utf8'),
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
  const g = classify(row);
  counts.set(g.id, { id: g.id, name: g.name, count: (counts.get(g.id)?.count || 0) + 1 });
  const skey = `${g.id}|${row.SourceKind}`;
  sourceKindCounts.set(skey, (sourceKindCounts.get(skey) || 0) + 1);
  if (!examples.has(g.id)) examples.set(g.id, []);
  const ex = examples.get(g.id);
  if (ex.length < 6) ex.push(row.RelativePath);
  total += 1;
}

const rows = [...counts.values()].sort((a, b) => a.id.localeCompare(b.id));
const csvLines = ['GroupId,GroupName,SourceRecords,PhysicalFiles,ZipEntries,NestedZipEntries,ExamplePaths'];
for (const r of rows) {
  const phys = sourceKindCounts.get(`${r.id}|physical-file`) || 0;
  const zip = sourceKindCounts.get(`${r.id}|zip-entry`) || 0;
  const nested = sourceKindCounts.get(`${r.id}|nested-zip-entry`) || 0;
  csvLines.push([
    r.id,
    csvEscape(r.name),
    r.count,
    phys,
    zip,
    nested,
    csvEscape((examples.get(r.id) || []).join(' | ')),
  ].join(','));
}
fs.writeFileSync(csvOut, `${csvLines.join('\n')}\n`, 'utf8');
fs.writeFileSync(jsonOut, JSON.stringify({ total, groups: rows, examples: Object.fromEntries(examples) }, null, 2), 'utf8');
console.log(`classified=${total} groups=${rows.length} csv=${csvOut}`);
