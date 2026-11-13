const fs=require('fs'),cp=require('child_process'),crypto=require('crypto'),path=require('path');
const root='C:/Projects/llm_mvp_core',e=root+'/artifacts/fbe/CODEX-NOV12-RECHECK/attempt-0001';
const stage=process.argv[2];if(!stage)throw Error('stage required');
const args=['--files','--hidden','--no-ignore','-g','*.cs','-g','*.csproj','-g','*.props','-g','*.targets','-g','*.sln','-g','*.slnx','-g','*.resx','-g','global.json','-g','NuGet.Config','-g','nuget.config','-g','packages.lock.json','-g','!**/bin/**','-g','!**/obj/**','-g','!**/TestResults/**','-g','!**/.git/**','-g','!**/node_modules/**','-g','!**/.venv/**','-g','!artifacts/fbe/CODEX-NOV12-RECHECK/**'];
const inventory=cp.spawnSync('git',['ls-files','--cached','--others','--exclude-standard','-z'],{cwd:root,encoding:'utf8',maxBuffer:32*1024*1024,timeout:25000});
if(inventory.status!==0)throw Error('inventory failed '+inventory.status+' '+inventory.stderr);
const sha=x=>crypto.createHash('sha256').update(x).digest('hex');
const closure=[],visited=new Set();
function visit(file){file=path.resolve(root,file).replace(/\\/g,'/');if(visited.has(file))return;visited.add(file);if(!fs.existsSync(file))throw Error('missing project '+file);const text=fs.readFileSync(file,'utf8');const refs=[...text.matchAll(/<ProjectReference\s+[^>]*Include="([^"]+)"/g)].map(m=>path.resolve(path.dirname(file),m[1].replace(/\\/g,'/')).replace(/\\/g,'/'));closure.push({project:path.relative(root,file).replace(/\\/g,'/'),references:refs.map(x=>path.relative(root,x).replace(/\\/g,'/'))});for(const p of refs)visit(p);}
visit('Tests/LlmFromScratch.Tests.csproj');
const candidates=inventory.stdout.split('\0').filter(x=>x&&(/\.(cs|csproj|props|targets|sln|slnx|resx)$/.test(x)||/(global\.json|NuGet\.Config|nuget\.config|packages\.lock\.json)$/.test(x))&&!/(^|\/)(bin|obj|TestResults)\//.test(x)&&!x.startsWith('artifacts/fbe/CODEX-NOV12-RECHECK/')).map(x=>x.replace(/\\/g,'/')).sort();

const projectDirs=closure.map(x=>path.posix.dirname(x.project)+'/');
const files=candidates.filter(x=>projectDirs.some(d=>x.startsWith(d))||!x.includes('/'));
const inputs=files.map(file=>({file,sha256:sha(fs.readFileSync(root+'/'+file))}));
const git=(args)=>{const r=cp.spawnSync('git',args,{cwd:root,encoding:'utf8',maxBuffer:32*1024*1024,timeout:20000});if(r.status!==0)throw Error('git '+args+' failed '+r.status+' '+r.stderr);return r.stdout;};
const snapshot={utc:new Date().toISOString(),stage,head:git(['rev-parse','HEAD']).trim(),status:git(['status','--porcelain=v1','--untracked-files=normal']),diffSha256:sha(git(['diff','--binary','HEAD'])),inventoryDescription:'Git tracked plus untracked nonignored source/config files under transitive ProjectReference project directories and root configuration; excludes TestResults/bin/obj and evidence root. Includes new untracked compile files. Linked Compile inputs also preserved by initial tracked source inventory and explicit file hashes; generated ignored outputs bound by DLL hashes.',inventoryCommand:['git','ls-files','--cached','--others','--exclude-standard','-z'],count:inputs.length,fingerprint:sha(JSON.stringify(inputs)),projectClosure:closure,inputs};
fs.writeFileSync(e+'/codex-source-'+stage+'.json',JSON.stringify(snapshot,null,2)+'\n',{flag:'wx'});
console.log(JSON.stringify({stage,head:snapshot.head,count:snapshot.count,fingerprint:snapshot.fingerprint,projectClosureCount:closure.length,status:snapshot.status}));
