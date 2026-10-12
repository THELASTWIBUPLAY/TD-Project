// Deploy as a JavaScript Cloud Code script named RecruitHero.
// Parameters: action (String), requestId (String), expectedSequence (Numeric).
const { DataApi } = require('@unity-services/cloud-save-1.4');
const { SettingsApi } = require('@unity-services/remote-config-1.1');
const KEY = 'recruitment_v1';
const HERO_IDS = ['hero_fighter_001', 'hero_ranger_001', 'hero_mage_001', 'hero_support_001', 'hero_tank_001'];
const RELIC_IDS = ['relic_placeholder_001', 'relic_placeholder_002', 'relic_placeholder_003', 'relic_placeholder_004', 'relic_placeholder_005'];
const DEFAULT_RELIC_CONFIG = { enabled: true, pool: RELIC_IDS.map(relicId => ({ relicId, weight: 1 })) };

module.exports = async ({ params, context }) => {
    if (!context.playerId) throw new Error('Player authentication required.');
    const api = new DataApi(context);
    const read = async () => {
        const response = await api.getProtectedItems(context.projectId, context.playerId, [KEY]);
        return response.data.results.find(item => item.key === KEY);
    };
    const empty = { schemaVersion: 1, rareHeroKeys: 0, legendaryHeroKeys: 0, rareRelicKeys: 0, epicRelicKeys: 0,
        sequence: 0, heroes: [], relics: [], lastOpening: null };
    const reply = (state, error = '', reward = null) => ({
        ok: !error, error, environmentId: context.environmentId, state, reward
    });
    const validState = s => s && s.schemaVersion === 1 &&
        Number.isSafeInteger(s.rareHeroKeys) && s.rareHeroKeys >= 0 &&
        (s.legendaryHeroKeys === undefined || (Number.isSafeInteger(s.legendaryHeroKeys) && s.legendaryHeroKeys >= 0)) &&
        ['rareRelicKeys', 'epicRelicKeys'].every(k => s[k] === undefined || (Number.isSafeInteger(s[k]) && s[k] >= 0)) &&
        (s.relics === undefined || (Array.isArray(s.relics) &&
            s.relics.every(r => r && RELIC_IDS.includes(r.relicId) && Number.isSafeInteger(r.quantity) && r.quantity > 0) &&
            new Set(s.relics.map(r => r.relicId)).size === s.relics.length)) &&
        Number.isSafeInteger(s.sequence) && s.sequence >= 0 && Array.isArray(s.heroes) &&
        s.heroes.every(h => HERO_IDS.includes(h.heroId) && Number.isSafeInteger(h.quantity) && h.quantity > 0) &&
        new Set(s.heroes.map(h => h.heroId)).size === s.heroes.length;
    let item = await read();
    if (item && !validState(item.value)) throw new Error('Invalid recruitment record; contact the developer.');
    if (params.action === 'status') return reply(item ? item.value : empty);
    // Separate action prevents older deployed scripts from treating Legendary as Rare.
    const chests = {
        open: ['rare', 'rareHeroKeys', 'rare_hero_chest'],
        open_legendary: ['legendary', 'legendaryHeroKeys', 'legendary_hero_chest'],
        open_rare_relic: ['rare_relic', 'rareRelicKeys', 'rare_relic_chest'],
        open_epic_relic: ['epic_relic', 'epicRelicKeys', 'epic_relic_chest']
    };
    if (!Object.prototype.hasOwnProperty.call(chests, params.action)) return reply(item ? item.value : empty, 'INVALID_REQUEST');
    const [chestType, keyField, configKey] = chests[params.action];
    const isRelic = chestType.endsWith('_relic');
    const idField = isRelic ? 'relicId' : 'heroId';
    const collectionField = isRelic ? 'relics' : 'heroes';
    const allowedIds = isRelic ? RELIC_IDS : HERO_IDS;
    if (!/^[a-f0-9]{32}$/.test(params.requestId || '') ||
        !Number.isSafeInteger(params.expectedSequence) || params.expectedSequence < 0)
        return reply(item ? item.value : empty, 'INVALID_REQUEST');

    // Missing records cannot spend keys. Provision once through a trusted admin/reward flow.
    // Never create/reset a record here: concurrent first opens must not overwrite each other.
    if (!item) return reply(empty, 'NO_KEYS');
    let config;
    for (let attempt = 0; attempt < 3; attempt++) {
        const state = item.value;
        if (!validState(state) || !item.writeLock) throw new Error('Invalid recruitment state or missing write lock.');
        if (state.lastOpening && state.lastOpening.requestId === params.requestId) {
            if ((state.lastOpening.chestType || 'rare') !== chestType) return reply(state, 'INVALID_REQUEST');
            return reply(state, '', state.lastOpening);
        }
        if (state.sequence !== params.expectedSequence) return reply(state, 'STATE_CHANGED');
        if ((state[keyField] || 0) < 1) return reply(state, 'NO_KEYS');
        if (!config) {
            const rc = new SettingsApi(context);
            const fallbackKey = chestType === 'legendary' ? 'rare_hero_chest' : chestType === 'epic_relic' ? 'rare_relic_chest' : null;
            const requestedKeys = fallbackKey ? [configKey, fallbackKey] : [configKey];
            const response = await rc.assignSettingsGet(context.projectId, context.environmentId, 'settings', requestedKeys);
            const settings = response.data.configs?.settings;
            config = settings?.[configKey];
            // Legendary shares Rare's odds until its own config is published.
            // An explicitly disabled Legendary config still stays disabled.
            if (config === undefined && fallbackKey) config = settings?.[fallbackKey];
            // Five equal placeholders work without another dashboard key during prototyping.
            // Published configs take precedence, including an explicit enabled:false.
            if (config === undefined && isRelic) config = DEFAULT_RELIC_CONFIG;
            if (typeof config === 'string') config = JSON.parse(config);
            if (!config || config.enabled !== true) return reply(state, 'UNAVAILABLE');
            if (!Array.isArray(config.pool) || !config.pool.length ||
                !config.pool.every(h => h && allowedIds.includes(h[idField]) && Number.isSafeInteger(h.weight) && h.weight > 0) ||
                new Set(config.pool.map(h => h[idField])).size !== config.pool.length)
                throw new Error('Invalid ' + configKey + ' reward pool.');
        }
        const total = config.pool.reduce((n, h) => n + h.weight, 0);
        if (!Number.isSafeInteger(total)) throw new Error('Invalid total reward weight.');
        let roll = Math.random() * total;
        const selected = config.pool.find(h => (roll -= h.weight) < 0) || config.pool[config.pool.length - 1];
        const next = JSON.parse(JSON.stringify(state));
        if (!next[collectionField]) next[collectionField] = [];
        let owned = next[collectionField].find(h => h[idField] === selected[idField]);
        if (!owned) { owned = { [idField]: selected[idField], quantity: 0 }; next[collectionField].push(owned); }
        owned.quantity++;
        next[keyField]--;
        next.sequence++;
        if (!Number.isSafeInteger(owned.quantity) || !Number.isSafeInteger(next.sequence)) throw new Error('Collection limit reached.');
        next.lastOpening = { requestId: params.requestId, chestType, [idField]: selected[idField],
            quantity: owned.quantity, sequence: next.sequence };
        try {
            // One conditional write commits the key, quantity and receipt together.
            await api.setProtectedItem(context.projectId, context.playerId, {
                key: KEY, value: next, writeLock: item.writeLock
            });
            return reply(next, '', next.lastOpening);
        } catch (error) {
            if (error.response?.status !== 409) throw error;
            item = await read();
            if (!item) throw new Error('Recruitment record disappeared.');
        }
    }
    throw new Error('Collection is busy. Retry the same request.');
};
