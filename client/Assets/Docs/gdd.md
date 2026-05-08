# Game Design Document
## Idle RPG (working title)

---

## 1. Wizja gry

Mobilny Idle RPG z turową walką w stylu klasycznych MMO, inspirowany feelingiem Ragnarok Online ale z własnym uniwersum, klasami i mechanikami. Gracz prowadzi pojedynczą postać przez świat podzielony na regiony, walcząc z falami potworów i bossami. Gra w trybie idle/auto z możliwością aktywnego zaangażowania w kluczowych momentach.

**Target:** casual-to-midcore audience, gracze 25-45 lat z nostalgią do klasycznych MMORPG.

**Wyróżnik wizualny:** Wysokiej jakości grafika 3D (Synty assets) w gatunku zdominowanym przez 2D i generyczne assety.

---

## 2. Core Gameplay Loop

### Pętla minutowa (sesja 2-5 min)
Gracz wybiera lokację → walka się odbywa automatycznie → loot leci → equip/upgrade → następna lokacja lub powrót do hub'u.

### Pętla dzienna
Daily quests, battle pass progress, dungeon entries (energy-gated), PvP arena, gildie.

### Pętla tygodniowa/długoterminowa
Progression przez regiony → odblokowanie nowych klas (job advancement) → Reincarnation (rebirth) → endgame content.

---

## 3. System walki

### Setup wizualny
- Kamera z perspektywą 3/4 lub side-view
- Drużyna gracza po jednej stronie ekranu (na górze), wrogowie po drugiej (na dole)
- Postacie statyczne, animacje ataków lecą w przeciwnika a postacie wykonują lekkie ruchy w kierunku ataku
- Tło to lokacja w której gracz się znajduje (Synty environments)

### Mechanika walki
- **System ATB (Active Time Battle):** każda postać ma pasek prędkości ataku (ASPD) który ładuje się w czasie rzeczywistym
- **Po naładowaniu paska:** postać wykonuje **basic attack** automatycznie
- **Skille i itemy:** używane przez gracza ze **paska umiejętności** w dowolnym momencie
    - Skille kosztują **manę (MP)**
    - Itemy (potiony, jedzenie) mają cooldowny i ograniczoną ilość
- **Auto mode:** opcjonalny, gra sama używa skilli i itemów według priorytetów
- **Speed stat** decyduje jak szybko ładuje się ASPD (kluczowy w PvP i raidach)

### Struktura starcia
Każdy stage składa się z fal przeciwników np:
- Fala 1: 2-3 słabszych potworów
- Fala 2: 2-3 średnich potworów (lub 1 elite)
- Fala 3: 2-3 mocniejszych potworów lub mini-boss
- **Boss stage** (specjalny): 1 boss + opcjonalni minioni, dłuższa walka
- Lub 1 fala jeśli walka z np tylko dwoma przeciwnikami
- Max 3 potwory na fale

Pomiędzy falami:
- HP/MP **nie regeneruje się** (decyzje taktyczne kiedy lekować)
- Skill cooldowny **nie resetują się**
- Krótka animacja przejścia między falami

---

## 4. Klasy i progresja

### System klas (Tier system)

Gracz rozwija dwa levele, level główny oraz job level.
Level główny powoduje rozwój statystyk.
Level job dostarcza skill pointy do odblkowywania skilli w drzewku umiejętności.

**Tier 1 - Klasy podstawowe:**
- **Vanguard** - tank/melee, aggro, defense
- **Berserker** - melee DPS, lifesteal, rage
- **Arcanist** - magic AOE/DOT
- **Hunter** - ranged DPS, crit focus
- **Cleric** - healer/support, buffs
- **Rogue** - assassin, evasion, single-target burst

**Tier 2 - Job advancement (po osiągnięciu job level 40). Odblokowywuje następne drzewko umiejętności oraz reset job level do 1:**
Każda klasa T1 ma 2 ścieżki rozwoju (np. Vanguard → Guardian / Crusader). Łącznie 12 klas T2.

**Tier 3 - Transcendent (post-Reincarnation, po osiągnięciu job level 50 oraz level 99):**
Każda klasa T2 ma własną ścieżkę endgame. Łącznie ~12-16 klas T3.

### Reincarnation (Rebirth)
- Dostępne po osiągnięciu lvl 99
- Resetuje level i job level do 1, zachowuje equipment
- **+10% wszystkich statystyk permanentnie** (skaluje z każdym Reincarnation)
- Odblokowuje Tier 3 klasy
- Daje dostęp do "Reincarnated zones" z lepszym lootem
- Wielokrotne reincarnations możliwe, malejące bonusy

### System statystyk
6 statystyk, punkty przydzielane co level:
- **Power** - melee damage, fizyczna siła
- **Agility** - speed/ASPD, evasion, attack speed
- **Vitality** - HP, defense, status resistance
- **Intellect** - magic damage, MP pool, magic defense
- **Precision** - accuracy, crit chance, ranged damage
- **Fortune** - crit damage, drop rate, lucky procs

Respec dostępny za premium currency.

---

## 5. Świat gry

### Struktura
**6-8 regionów** odblokowywanych progresywnie przez story i level.

Każdy region:
- Unikalny biom wizualny (las, ruiny, pustynia, bagna, góry, wulkan, etc.)
- 8-10 lokacji
- unikalne potwory per lokacja (ale mogą powtarzać się też z innych zachowując klimat)
- przynajmniej 1 boss stage per region
- Unikalne materiały craftingowe i dropy

### Przykładowa progresja regionów
- Region 1 (lvl 1-15): Las/łąki - humanoidy, podstawy
- Region 2 (lvl 15-30): Ruiny/podziemia - undead
- Region 3 (lvl 30-50): Pustynia - golemy, skorpiony
- Region 4 (lvl 50-70): Bagna - trujące mob'y, alchemia
- Region 5 (lvl 70-90): Góry lodowe - elementarne
- Region 6 (lvl 90-100): Wulkan/piekło - demony
- Region 7+: Endgame zones (post-Reincarnation)

### Farm mode
Każda lokacja ma tryb **farm/grind** - gracz powtarza stage'e dla materiałów i expa. To kluczowy element idle gameplay.

---

## 6. Postacie i ekwipunek

### Postać gracza
- **Jedna postać główna** dedykowana graczowi przez całą rozgrywkę
- Customizacja wyglądu (kolor włosów, twarz, etc.)
- Wybór klasy startowej, później job advancement

### Equipment slots
8 slotów ekwipunku:
- Weapon (broń)
- Shield (tarcza)
- Helmet (hełm)
- Armor (zbroja)
- Shoes (buty)
- Garment (płaszcz/dodatek)
- Accessory x2 (pierścienie/amulety)

### Rzadkości przedmiotów
- Common (biały)
- Uncommon (zielony)
- Rare (niebieski)
- Epic (fioletowy)
- Legendary (pomarańczowy)
- Mythic (czerwony)

### System Card
Przedmioty (nie wszystkie) posiadają card slots (1-3).
Specjalne dropy z potworów dające unikalne pasywne efekty po włożeniu w slot. Bardzo rzadkie, kolekcjonerski element, kluczowy w endgame buildach.

### Refining/Enchanting
Upgrade ekwipunku za materiały. **Ryzyko zniszczenia** przy wyższych poziomach - emocjonujący gameplay moment.

---

## 7. Tryby gry

### Singleplayer (core)
Główna pętla - eksploracja regionów, farm, progresja klasy, boss fights solo.

### Party Mode (MMO mode - **post-launch feature**)
- **Maksymalnie 3 graczy aktywnie w walce**
- Pozostali gracze (4-6) **czekają w kolejce**
- Gdy któryś z aktywnych graczy padnie - **automatycznie wskakuje kolejny z kolejki**
- Pozwala na duże party (do 9 graczy zaplanowanych) z dynamiczną rotacją
- Dedykowany content: party dungeons, raid bosses, world events
- **Implementacja w późniejszym etapie** jako rozszerzenie MMO

### PvP Arena
Async PvP - gracze walczą z buildami innych graczy sterowanymi przez AI. Sezony 2-tygodniowe, leaderboardy, nagrody.

### Endgame modes
- Endless tower (leaderboard floors)
- MVP bosses (świat-spawn, konkurencja o kill)
- Guild raids (cooperative damage)
- Weekly events

---

## 8. Systemy progresji i retencji

### Daily content
- Daily quests (3-5 zadań)
- Dungeon entries (energy-gated, 3-5 dziennie)
- PvP arena entries (5 walk)
- Daily boss attempts

### Weekly content
- Guild raid boss
- Weekly tower challenge
- Rotujący event dungeon

### Battle Pass
Sezony 4-6 tygodniowe, free + premium track. Główny driver codziennego logowania.

### Eventy
Cotygodniowe lub dwutygodniowe wydarzenia 3-7 dniowe z exclusive lootem i bohaterami.

---

## 9. Monetyzacja

### IAP
- **Battle Pass** ($5-10/sezon) - core monetization
- **Gem packs** - premium currency
- **Starter packs** - one-time offers dla nowych graczy
- **Class unlock packs** - przyspieszone odblokowanie advance klas
- **Special event bundles** - limited time offers

### Filozofia
**Nie pay-to-win** w PvP (skill/build matters), ale pay-to-progress-faster. Whales mają więcej cosmetics, prestige skinów, i wcześniejszy dostęp do nowych klas.

---

## 10. Scope (launch)

### MVP
- 1-2 dopracowana klas Tier 1 end-two-end
- 1 region wraz z lokacjami
- Turowa walka z auto-mode (ASPD + skill bar) - dopracowana produkcyjnie end-to-end
- 5 typów potworów
- 1 boss fights
- Equipment system
- Daily quests + Battle Pass
- Async PvP arena
- Customizacja postaci

### WYJŚCIE Z MVP NR 1
- Tier 2 klasy (job advancement)
- Reincarnation system
- 2 dodatkowe regiony
- Gildie
- MVP world bosses

### WYJŚCIE Z MVP NR 2
- Tier 3 klasy
- Card system
- Refining/Enchanting
- Endless tower
- **Party Mode (MMO)** - 3 active + queue rotation

### WYJŚCIE Z MVP NR 3
- Dodatkowe regiony
- Nowe job paths
- Seasonal events
- Cross-server features

---

## 11. Estetyka i klimat

- **Visual style:** Stylized 3D (Synty assets), kolorowa paleta, anime-inspired
- **Vibe:** Klasyczne fantasy MMO, nostalgiczne ale świeże
- **Audio:** Orkiestrowy soundtrack, sygnaturowe SFX dla skilli, voiceover dla bossów (opcjonalny)
- **UI:** Czytelny, mobile-first, inspirowany klasycznymi RPG ale dostosowany do dotyku

---

## 12. Target Platform

- **Primary:** Android (Phone + Tablet)
- **Orientation:** Portrait

---

*Dokument żywy - do iteracji w trakcie produkcji.*