# Recruitment → HeroManager / Deck

## What changed
Recruitment now resolves HeroSO assets through HeroManager.Database (HeroDB). The old HeroCatalog is no longer used by the chest UI; it is retained on disk for compatibility with other scenes.
A successful server snapshot sets recruitedCopies rather than incrementing it. Refresh/retry therefore cannot grant duplicates. The first copy unlocks a hero for Deck. Further copies remain copy counts, not fragments. Existing level, star and fragment values are preserved. SaveManager now includes HeroManager when saving/loading.
RecruitmentDeckSync on HeroManager loads the server collection without requiring the Shop to be opened. The Deck shows xN copy counts.

## Cloud deployment (required for new pools and debug grants)
1. Publish CloudCode/RecruitHero.js as the existing RecruitHero script in the same environment used by the game. Existing action, requestId and expectedSequence parameters are unchanged.
2. Publish rare_hero_chest and legendary_hero_chest from their JSON files. Both use the same seven heroes with weight 1 for now.
3. Old records and old pool IDs are still supported. No reset of recruitment_v1 is needed.

## Add test keys
Only for the intended test player's Cloud Save **Protected** data, add recruitment_debug_enabled with JSON boolean true (not the string "true"). Set false or delete it after testing.
The player must already have a recruitment_v1 record; for a new test player, create the existing schema with zero balances and empty heroes/relics first. Never overwrite an existing record.
In Unity Play Mode select the object with RecruitmentService (MainMenu's ShopLayout). Its Inspector has Debug +10 buttons for each chest. These call Cloud Code and show the real returned balance. They are not local fake keys. Retrying the same button after a lost response is safe. Production clients have no debug UI, and the server rejects callers without the protected flag.

## Existing ID compatibility
hero_fighter_001 → 01 (Lukas)
hero_ranger_001 → Ranger_Alex_01 (Alex)
hero_mage_001 → Ranger_Lisa_01 (Lisa)
hero_support_001 → Support_Ely_01 (Ely)
hero_tank_001 → Tank_Bubu_01 (Bubu)

Lukas's actual HeroSO ID is "01" and Lisa's is "Ranger_Lisa_01", even though her asset file is Mage_Lisa_01. These existing IDs were preserved to avoid breaking your coworker's references. New heroes Ora and Mona use Fighter_Ora_2 and Mage_Mona_2.

Hero progress/upgrade saves still follow the team's existing local/client Cloud Save design. Recruitment rolls, key spending and debug permission checks remain server-side. This change does not implement battle spawning or migrate the team's entire progression system to server authority.
