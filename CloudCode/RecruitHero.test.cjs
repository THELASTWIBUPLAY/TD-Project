const { test } = require('node:test');
const assert = require('node:assert/strict');
const vm = require('node:vm');
const fs = require('node:fs');
const source = fs.readFileSync(__dirname + '/RecruitHero.js', 'utf8');
const clone = x => JSON.parse(JSON.stringify(x));
function fixture(keys = 5, options = {}) {
    let record = options.missing ? null : { key: 'recruitment_v1', writeLock: '0', value: {
        schemaVersion: 1, rareHeroKeys: keys, sequence: 0, heroes: [], lastOpening: null,
        ...(options.legendaryKeys === undefined ? {} : {legendaryHeroKeys: options.legendaryKeys}),
        ...(options.rareRelicKeys === undefined ? {} : {rareRelicKeys: options.rareRelicKeys}),
        ...(options.epicRelicKeys === undefined ? {} : {epicRelicKeys: options.epicRelicKeys})
    }};
    let version = 0, writes = 0, lostReply = options.lostReply;
    const config = { enabled: true, pool: [{heroId:'hero_mage_001',weight:1}], ...options.config };
    class DataApi {
        async getProtectedItems() { return { data: { results: record ? [clone(record)] : [] } }; }
        async setProtectedItem(project, player, item) {
            if (record.writeLock !== item.writeLock) throw { response: { status: 409 } };
            record = { key:item.key, value:clone(item.value), writeLock:String(++version) }; writes++;
            if (lostReply) { lostReply = false; throw new Error('Connection lost after commit'); }
        }
    }
    class SettingsApi { async assignSettingsGet(project, environment, type, keys) {
        const all = {rare_hero_chest:config, legendary_hero_chest:options.legendaryConfig,
            rare_relic_chest:options.rareRelicConfig, epic_relic_chest:options.epicRelicConfig};
        return {data:{configs:{settings:Object.fromEntries(keys.filter(k=>all[k]!==undefined).map(k=>[k,all[k]]))}}};
    } }
    const sandbox = { module:{exports:{}}, require:n => n.includes('cloud-save') ? {DataApi} : {SettingsApi},
        Math: {random: () => options.roll ?? 0.5}, Number, Set, JSON, Error };
    vm.runInNewContext(source, sandbox);
    const call = (sequence, requestId='a'.repeat(32), action='open') => sandbox.module.exports({
        context:{projectId:'test',playerId:'player',environmentId:'dev'}, params:{action,requestId,expectedSequence:sequence}
    });
    return {call, state:()=>clone(record?.value), writes:()=>writes};
}
test('duplicates accumulate and exactly one key is consumed per opening', async()=>{
    const f=fixture(6);
    for(let i=0;i<6;i++) assert.equal((await f.call(i,String(i).repeat(32))).ok,true);
    assert.equal(f.state().heroes.length,1); assert.equal(f.state().heroes[0].quantity,6);
    assert.equal(f.state().rareHeroKeys,0);
    assert.equal((await f.call(6)).error,'NO_KEYS'); assert.equal(f.writes(),6);
});
test('same request is replayed without reroll or charge', async()=>{
    const f=fixture(); const first=await f.call(0); const replay=await f.call(0);
    assert.equal(first.reward.heroId,replay.reward.heroId); assert.equal(f.writes(),1);
});
test('concurrent different requests cannot both spend the same sequence', async()=>{
    const f=fixture(); const replies=await Promise.all([f.call(0), f.call(0,'b'.repeat(32))]);
    assert.equal(replies.filter(r=>r.ok).length,1); assert.equal(f.writes(),1);
});
test('concurrent identical requests return the same committed award', async()=>{
    const f=fixture(); const replies=await Promise.all([f.call(0), f.call(0)]);
    assert.equal(replies.every(r=>r.ok),true); assert.equal(f.writes(),1);
});
test('network loss after save is recovered by the original request', async()=>{
    const f=fixture(5,{lostReply:true}); await assert.rejects(f.call(0));
    assert.equal((await f.call(0)).ok,true); assert.equal(f.writes(),1);
});
test('old request cannot charge again after newer openings', async()=>{
    const f=fixture(); await f.call(0); await f.call(1,'b'.repeat(32));
    assert.equal((await f.call(0)).error,'STATE_CHANGED'); assert.equal(f.writes(),2);
});
test('missing record does not mint keys or create unsafe initial writes', async()=>{
    const f=fixture(0,{missing:true}); assert.equal((await f.call(0)).error,'NO_KEYS'); assert.equal(f.writes(),0);
});
test('disabled chest and invalid pool fail without charging', async()=>{
    const disabled=fixture(5,{config:{enabled:false}}); assert.equal((await disabled.call(0)).error,'UNAVAILABLE');
    const invalid=fixture(5,{config:{pool:[{heroId:'forged',weight:1}]}}); await assert.rejects(invalid.call(0)); assert.equal(invalid.writes(),0);
});
test('invalid request inputs do not mutate collection', async()=>{
    const f=fixture(); assert.equal((await f.call(-1)).error,'INVALID_REQUEST');
    assert.equal((await f.call(0,'bad')).error,'INVALID_REQUEST'); assert.equal(f.writes(),0);
});

const legendary = {enabled:true,pool:[{heroId:'hero_ranger_001',weight:1}]};
test('Legendary uses its own pool and keys but shares the hero collection', async()=>{
    const f=fixture(5,{legendaryKeys:2,legendaryConfig:legendary});
    await f.call(0);
    const r=await f.call(1,'b'.repeat(32),'open_legendary');
    assert.equal(r.reward.heroId,'hero_ranger_001'); assert.equal(r.reward.chestType,'legendary');
    assert.equal(f.state().rareHeroKeys,4); assert.equal(f.state().legendaryHeroKeys,1);
    assert.equal(f.state().heroes.length,2);
    await f.call(2,'c'.repeat(32),'open_legendary');
    assert.equal(f.state().heroes.find(h=>h.heroId==='hero_ranger_001').quantity,2);
});
test('existing Rare-only records remain valid and have no Legendary keys', async()=>{
    const f=fixture();
    assert.equal((await f.call(0,'a'.repeat(32),'open_legendary')).error,'NO_KEYS');
    assert.equal((await f.call(0)).ok,true);
});
test('Legendary without a separate config uses Rare rewards and spends only a Legendary key', async()=>{
    const f=fixture(5,{legendaryKeys:2});
    const r=await f.call(0,'a'.repeat(32),'open_legendary');
    assert.equal(r.ok,true); assert.equal(r.reward.heroId,'hero_mage_001');
    assert.equal(r.reward.chestType,'legendary');
    assert.equal(f.state().rareHeroKeys,5); assert.equal(f.state().legendaryHeroKeys,1);
    assert.equal((await f.call(0,'a'.repeat(32),'open_legendary')).ok,true);
    assert.equal(f.writes(),1);
});
test('explicitly disabled Legendary config stays disabled despite enabled Rare config', async()=>{
    const f=fixture(5,{legendaryKeys:1,legendaryConfig:{...legendary,enabled:false}});
    assert.equal((await f.call(0,'a'.repeat(32),'open_legendary')).error,'UNAVAILABLE');
    assert.equal(f.writes(),0);
});
test('Legendary fallback respects disabled or invalid Rare configuration', async()=>{
    const f=fixture(5,{legendaryKeys:1,config:{enabled:false}});
    assert.equal((await f.call(0,'a'.repeat(32),'open_legendary')).error,'UNAVAILABLE');
    assert.equal(f.writes(),0);
    const bad=fixture(5,{legendaryKeys:1,config:{pool:[]}});
    await assert.rejects(bad.call(0,'a'.repeat(32),'open_legendary'));
    assert.equal(bad.writes(),0);
});
test('Legendary retry after lost response returns the same receipt without charging twice', async()=>{
    const f=fixture(5,{legendaryKeys:2,legendaryConfig:legendary,lostReply:true});
    await assert.rejects(f.call(0,'a'.repeat(32),'open_legendary'));
    assert.equal((await f.call(0,'a'.repeat(32),'open_legendary')).ok,true);
    assert.equal(f.state().legendaryHeroKeys,1); assert.equal(f.writes(),1);
    assert.equal((await f.call(0)).error,'INVALID_REQUEST'); assert.equal(f.writes(),1);
});
test('simultaneous Rare and Legendary openings cannot overwrite each other', async()=>{
    const f=fixture(5,{legendaryKeys:2,legendaryConfig:legendary});
    const r=await Promise.all([f.call(0),f.call(0,'b'.repeat(32),'open_legendary')]);
    assert.equal(r.filter(x=>x.ok).length,1); assert.equal(f.writes(),1);
    assert.equal(f.state().rareHeroKeys+f.state().legendaryHeroKeys,6);
});
test('configurable Legendary weights determine selection independently of Rare', async()=>{
    const pool=[{heroId:'hero_ranger_001',weight:1},{heroId:'hero_tank_001',weight:3}];
    for(const [roll,hero] of [[0,'hero_ranger_001'],[0.249,'hero_ranger_001'],[0.25,'hero_tank_001'],[0.999,'hero_tank_001']]) {
        const f=fixture(5,{legendaryKeys:1,legendaryConfig:{enabled:true,pool},roll});
        assert.equal((await f.call(0,'a'.repeat(32),'open_legendary')).reward.heroId,hero);
    }
});
test('shipped Rare and Legendary configurations start with the same five equal weights',()=>{
    const rare=JSON.parse(fs.readFileSync(__dirname+'/rare_hero_chest.json','utf8'));
    const legend=JSON.parse(fs.readFileSync(__dirname+'/legendary_hero_chest.json','utf8'));
    assert.deepEqual(legend,rare); assert.equal(legend.pool.length,5);
    assert.ok(legend.pool.every(x=>x.weight===1));
});

test('relic defaults cover five equal placeholders for both chest types',async()=>{
    for(const action of ['open_rare_relic','open_epic_relic']) for(let i=0;i<5;i++) {
        const f=fixture(5,{rareRelicKeys:1,epicRelicKeys:1,roll:(i+.5)/5});
        const r=await f.call(0,'a'.repeat(32),action);
        assert.equal(r.reward.relicId,'relic_placeholder_00'+(i+1));
        assert.equal(r.state.heroes.length,0); assert.equal(r.state.rareHeroKeys,5);
    }
});
test('relic duplicates share collection, retain heroes and spend only the selected keys',async()=>{
    const f=fixture(5,{rareRelicKeys:2,epicRelicKeys:1});
    await f.call(0);
    await f.call(1,'b'.repeat(32),'open_rare_relic');
    await f.call(2,'c'.repeat(32),'open_epic_relic');
    assert.equal(f.state().relics[0].quantity,2);
    assert.equal(f.state().heroes[0].quantity,1);
    assert.equal(f.state().rareRelicKeys,1); assert.equal(f.state().epicRelicKeys,0);
    assert.equal(f.state().rareHeroKeys,4);
});
test('older records have no relic keys and are not rewritten by status',async()=>{
    const f=fixture(); assert.equal((await f.call(0,'','status')).ok,true);
    assert.equal((await f.call(0,'a'.repeat(32),'open_rare_relic')).error,'NO_KEYS');
    assert.equal(f.writes(),0);
});
test('relic lost reply replay does not double spend and cannot be reused across chests',async()=>{
    const f=fixture(5,{rareRelicKeys:2,epicRelicKeys:2,lostReply:true});
    await assert.rejects(f.call(0,'a'.repeat(32),'open_rare_relic'));
    assert.equal((await f.call(0,'a'.repeat(32),'open_rare_relic')).ok,true);
    assert.equal((await f.call(0,'a'.repeat(32),'open_epic_relic')).error,'INVALID_REQUEST');
    assert.equal(f.writes(),1); assert.equal(f.state().rareRelicKeys,1);
});
test('hero and relic simultaneous requests preserve shared sequence',async()=>{
    const f=fixture(5,{rareRelicKeys:2});
    const replies=await Promise.all([f.call(0),f.call(0,'b'.repeat(32),'open_rare_relic')]);
    assert.equal(replies.filter(r=>r.ok).length,1); assert.equal(f.writes(),1);
});
test('relic config weights are independent and Epic falls back to Rare only when absent',async()=>{
    const rare={enabled:true,pool:[{relicId:'relic_placeholder_001',weight:1}]};
    const epic={enabled:true,pool:[{relicId:'relic_placeholder_004',weight:1},{relicId:'relic_placeholder_005',weight:3}]};
    for(const [roll,id] of [[0.249,'004'],[0.25,'005'],[0.999,'005']]) {
        const f=fixture(5,{epicRelicKeys:1,rareRelicConfig:rare,epicRelicConfig:epic,roll});
        assert.equal((await f.call(0,'a'.repeat(32),'open_epic_relic')).reward.relicId,'relic_placeholder_'+id);
    }
    const f=fixture(5,{epicRelicKeys:1,rareRelicConfig:rare});
    assert.equal((await f.call(0,'a'.repeat(32),'open_epic_relic')).reward.relicId,'relic_placeholder_001');
});
test('disabled and invalid relic configs cannot silently enable default rewards',async()=>{
    const f=fixture(5,{epicRelicKeys:1,epicRelicConfig:{enabled:false}});
    assert.equal((await f.call(0,'a'.repeat(32),'open_epic_relic')).error,'UNAVAILABLE'); assert.equal(f.writes(),0);
    const bad=fixture(5,{rareRelicKeys:1,rareRelicConfig:{enabled:true,pool:[{heroId:'hero_mage_001',weight:1}]}});
    await assert.rejects(bad.call(0,'a'.repeat(32),'open_rare_relic')); assert.equal(bad.writes(),0);
});
test('shipped relic pools match the five equal default placeholders',()=>{
    const rare=JSON.parse(fs.readFileSync(__dirname+'/rare_relic_chest.json','utf8'));
    const epic=JSON.parse(fs.readFileSync(__dirname+'/epic_relic_chest.json','utf8'));
    assert.deepEqual(rare,epic);
    assert.deepEqual(rare.pool,Array.from({length:5},(_,i)=>({relicId:'relic_placeholder_00'+(i+1),weight:1})));
});
