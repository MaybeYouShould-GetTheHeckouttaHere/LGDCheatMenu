using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LongGoneDaysCheats
{
    // Standalone Unity overlay. MelonLoader hosts it; Harmony supplies runtime hooks.
    public sealed class CheatMenu : MonoBehaviour
    {
        public static bool IsOpen;
        public static bool Noclip;
        static CheatMenu instance;
        static readonly FieldInfo ValuesField = typeof(Stats).GetField("m_statValues", BindingFlags.NonPublic | BindingFlags.Instance);
        const string Prefix = "__lgd_base_";
        readonly Dictionary<string, string> edits = new Dictionary<string, string>();
        readonly List<Texture2D> textures = new List<Texture2D>();
        Rect window = new Rect(0, 0, 940, 640);
        Vector2 scroll, actorScroll;
        int tab, category;
        string search = "", selectedId = "", notice = "Changes apply immediately. Save normally to keep inventory and character edits.";
        GUIStyle title, subtitle, label, muted, button, active, field, card, panel, heading;
        float oldScale;
        bool oldCursor;
        CursorLockMode oldLock;
        SaveData lastSave;
        string lastLocation;
        Tile lastSafeTile;
        float uiScale = 1;
        bool rebinding;
        bool onlyOwned;

        public static void Shutdown() { if (instance != null) Destroy(instance.gameObject); }
        public static void ResetExploration()
        { Noclip = false; if (instance != null) { instance.lastSafeTile = null; instance.edits.Clear(); } }

        public static void Initialize()
        {
            if (instance != null) return;
            var go = new GameObject("Long Gone Days • Cheat Menu");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<CheatMenu>();
            Debug.Log("[LGD Cheat Menu] Loaded. Press " + CheatMod.ShortcutLabel + ".");
        }

        static Dictionary<string, int> Values(Stats stats) { return (Dictionary<string, int>)ValuesField.GetValue(stats); }
        public static int MaxStat(int original, Stats stats, string id)
        {
            int value;
            return Values(stats).TryGetValue(Prefix + id, out value) ? value : original;
        }
        public static void CopyBaseStats(Stats target, Actor actor)
        {
            if (actor == null || actor.PersistentStats == null || target == actor.PersistentStats) return;
            var source = Values(actor.PersistentStats);
            var dest = Values(target);
            foreach (var pair in source)
                if (pair.Key.StartsWith(Prefix) || (pair.Key != "hp" && pair.Key != "morale")) dest[pair.Key] = pair.Value;
        }
        public static void PrepareBattle(BattleActorAlly ally, Actor actor)
        {
            // Constructor caches totals before it creates Stats. Refresh those caches too.
            CopyBaseStats(ally.Stats, actor);
            SetPrivate(ally, "m_totalHP", ally.Stats.GetMaxHP());
            SetPrivate(ally, "m_totalMorale", ally.Stats.GetMaxMorale());
        }
        public static bool MovementCheck(bool original, EntityAgent agent)
        {
            var manager = MonoSingleton<EntityManager>.Instance;
            return original || (Noclip && manager != null && manager.PlayerExist && manager.PlayerEntity != null && manager.PlayerEntity.Agent == agent);
        }
        public static bool Walkable(Tile tile, EntityAgent agent)
        {
            // Occupied cells remain blocked: crossing one would remove the NPC's tile residency.
            return tile.Walkable || (!tile.HasResident && MovementCheck(false, agent));
        }
        static void SetPrivate(object obj, string name, object value)
        { obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, value); }

        void Update()
        {
            if (!rebinding && CheatMod.ShortcutPressed()) Toggle();
            else if (!rebinding && IsOpen && Input.GetKeyDown(KeyCode.Escape)) Toggle();
            if (IsOpen) Time.timeScale = 0;
            if (lastSave != GameUtils.SaveData || (GameUtils.SaveData != null && lastLocation != GameUtils.SaveData.LocationID))
            {
                lastSave = GameUtils.SaveData;
                lastLocation = lastSave == null ? null : lastSave.LocationID;
                edits.Clear(); selectedId = ""; lastSafeTile = null; Noclip = false;
            }
            var manager = MonoSingleton<EntityManager>.Instance;
            if (!Noclip && manager != null && manager.PlayerExist && manager.PlayerEntity != null)
            {
                var agent = manager.PlayerEntity.Agent;
                if (!agent.IsMoving && agent.CurrentTile != null && agent.CurrentTile.m_walkable) lastSafeTile = agent.CurrentTile;
            }
        }
        void Toggle()
        {
            IsOpen = !IsOpen;
            if (IsOpen)
            {
                oldScale = Time.timeScale; Time.timeScale = 0;
                oldCursor = Cursor.visible; oldLock = Cursor.lockState;
                Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
                edits.Clear();
            }
            else
            {
                Time.timeScale = oldScale; Cursor.lockState = oldLock; Cursor.visible = oldCursor;
            }
        }
        void OnDestroy()
        {
            if (IsOpen) Toggle();
            Noclip = false; instance = null;
            foreach (var texture in textures) Destroy(texture);
        }

        Texture2D Rounded(Color color, int radius)
        {
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(radius - x, x - (size - 1 - radius), 0);
                float dy = Mathf.Max(radius - y, y - (size - 1 - radius), 0);
                float alpha = Mathf.Clamp01(radius + 0.5f - Mathf.Sqrt(dx * dx + dy * dy));
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * alpha));
            }
            texture.Apply(); textures.Add(texture); return texture;
        }
        GUIStyle Text(int size, Color color, FontStyle weight)
        {
            var style = new GUIStyle(GUI.skin.label); style.fontSize = size; style.fontStyle = weight;
            style.normal.textColor = color; style.richText = false; style.padding = new RectOffset(0, 0, 3, 3);
            return style;
        }
        void Styles()
        {
            if (title != null) return;
            var white = new Color(.91f, .94f, .98f);
            title = Text(26, white, FontStyle.Bold);
            heading = Text(19, white, FontStyle.Bold);
            label = Text(14, white, FontStyle.Normal);
            muted = Text(12, new Color(.57f, .64f, .73f), FontStyle.Normal); muted.wordWrap = true;
            subtitle = Text(12, new Color(.35f, .85f, .76f), FontStyle.Bold);
            button = new GUIStyle(GUI.skin.button);
            button.fontSize = 14; button.fontStyle = FontStyle.Bold;
            button.normal.textColor = white; button.hover.textColor = Color.white; button.active.textColor = Color.white;
            button.normal.background = Rounded(new Color(.16f, .21f, .29f), 8);
            button.hover.background = Rounded(new Color(.22f, .31f, .39f), 8);
            button.active.background = Rounded(new Color(.19f, .43f, .43f), 8);
            button.border = new RectOffset(10, 10, 10, 10); button.padding = new RectOffset(14, 14, 8, 8);
            active = new GUIStyle(button); active.normal.background = Rounded(new Color(.13f, .37f, .35f), 8);
            active.normal.textColor = new Color(.55f, 1f, .86f);
            field = new GUIStyle(GUI.skin.textField); field.fontSize = 15; field.normal.textColor = white;
            field.focused.textColor = Color.white; field.normal.background = Rounded(new Color(.07f, .10f, .16f), 6);
            field.focused.background = Rounded(new Color(.12f, .22f, .29f), 6);
            field.border = new RectOffset(8, 8, 8, 8); field.padding = new RectOffset(10, 10, 7, 7);
            panel = new GUIStyle(); panel.normal.background = Rounded(new Color(.065f, .085f, .13f), 12);
            panel.border = new RectOffset(14, 14, 14, 14); panel.padding = new RectOffset(24, 24, 20, 20);
            card = new GUIStyle(); card.normal.background = Rounded(new Color(.10f, .135f, .19f), 8);
            card.border = new RectOffset(10, 10, 10, 10); card.padding = new RectOffset(14, 14, 12, 12);
        }
        void OnGUI()
        {
            if (!IsOpen) return;
            if (rebinding && Event.current.type == EventType.KeyDown)
            {
                var e = Event.current;
                if (e.keyCode == KeyCode.Escape) { rebinding = false; notice = "Shortcut unchanged."; e.Use(); }
                else if (e.keyCode != KeyCode.None && e.keyCode != KeyCode.LeftControl && e.keyCode != KeyCode.RightControl && e.keyCode != KeyCode.LeftShift && e.keyCode != KeyCode.RightShift && e.keyCode != KeyCode.LeftAlt && e.keyCode != KeyCode.RightAlt && e.keyCode != KeyCode.LeftCommand && e.keyCode != KeyCode.RightCommand)
                { CheatMod.Bind(e.keyCode, e.control, e.alt, e.shift); rebinding = false; notice = "Shortcut saved: " + CheatMod.ShortcutLabel; e.Use(); }
            }
            Color previousColor = GUI.color, previousBackground = GUI.backgroundColor, previousContent = GUI.contentColor;
            bool previousEnabled = GUI.enabled;
            int previousDepth = GUI.depth;
            Matrix4x4 old = GUI.matrix;
            try
            {
            GUI.color = GUI.backgroundColor = GUI.contentColor = Color.white;
            GUI.enabled = true;
            Styles(); GUI.depth = -10000;
            float scale = Mathf.Min(Screen.width / 1000f, Screen.height / 700f) * uiScale;
            scale = Mathf.Max(.35f, scale);
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1));
            var area = new Rect(0, 0, Screen.width / scale, Screen.height / scale);
            window.x = Mathf.Clamp(window.x == 0 ? (area.width - window.width) / 2 : window.x, 0, Mathf.Max(0, area.width - window.width));
            window.y = Mathf.Clamp(window.y == 0 ? (area.height - window.height) / 2 : window.y, 0, Mathf.Max(0, area.height - window.height));
            window = GUI.Window(184971, window, DrawWindow, "", panel);
            }
            finally
            {
                GUI.matrix = old;
                GUI.color = previousColor; GUI.backgroundColor = previousBackground; GUI.contentColor = previousContent;
                GUI.enabled = previousEnabled; GUI.depth = previousDepth;
            }
        }
        void DrawWindow(int id)
        {
            GUI.color = GUI.backgroundColor = GUI.contentColor = Color.white;
            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(); GUILayout.Label("LONG GONE DAYS", subtitle); GUILayout.Label("Field companion", title); GUILayout.EndVertical();
            GUILayout.FlexibleSpace(); GUILayout.Label("CHEAT MENU  /  1.1.2", muted, GUILayout.Width(145));
            if (GUILayout.Button("Close  ×", button, GUILayout.Width(110))) Toggle();
            GUILayout.EndHorizontal(); GUILayout.Space(20);
            GUILayout.BeginHorizontal(); GUILayout.BeginVertical(GUILayout.Width(156));
            string[] tabs = { "Characters", "Inventory", "Exploration", "Settings" };
            for (int i = 0; i < tabs.Length; i++)
            {
                if (GUILayout.Button(tabs[i], tab == i ? active : button, GUILayout.Height(42))) { tab = i; scroll = Vector2.zero; edits.Clear(); GUI.FocusControl(null); }
                GUILayout.Space(7);
            }
            GUILayout.Space(24); GUILayout.Label(CheatMod.ShortcutLabel, subtitle); GUILayout.Label("Toggle menu\nEsc to close\n\nGame paused while open.\n\nDrag the header to move.", muted);
            GUILayout.EndVertical(); GUILayout.Space(22); GUILayout.BeginVertical(GUILayout.Height(435));
            try
            {
                if (tab == 3) { scroll = GUILayout.BeginScrollView(scroll); Settings(); GUILayout.EndScrollView(); }
                else if (GameUtils.SaveData == null) { GUILayout.Label("Ready when you are", heading); GUILayout.Label("Start or load a game to edit your party and inventory.", muted); }
                else if (tab == 0) Characters(); else if (tab == 1) InventoryView(); else Exploration();
            }
            catch (Exception ex) { notice = "Unable to complete action: " + ex.Message; Debug.LogError("[LGD Cheat Menu] " + ex); }
            GUILayout.EndVertical(); GUILayout.EndHorizontal(); GUILayout.FlexibleSpace();
            GUILayout.Label(notice, muted, GUILayout.Height(44));
            GUI.DragWindow(new Rect(0, 0, window.width - 135, 78));
        }
        string NumericField(string key, string value, int max)
        {
            var e = Event.current;
            if (GUI.GetNameOfFocusedControl() == key && e.type == EventType.KeyDown && !e.control && !e.command && e.character >= ' ' && (e.character < '0' || e.character > '9')) e.Use();
            GUI.SetNextControlName(key);
            string next = GUILayout.TextField(value, field, GUILayout.Width(95));
            // Paste and IME input pass through the same digit-only filter.
            next = new string(next.Where(c => c >= '0' && c <= '9').ToArray());
            int parsed;
            if (next.Length > 0 && (!int.TryParse(next, out parsed) || parsed > max)) next = max.ToString();
            return next;
        }
        void Number(string key, string caption, int current, int min, int max, Action<int> apply, bool immediate = false)
        {
            GUILayout.BeginHorizontal(card);
            GUILayout.Label(caption, label, GUILayout.Width(174));
            GUILayout.Label(current.ToString(), muted, GUILayout.Width(66)); GUILayout.FlexibleSpace();
            string value; if (!edits.TryGetValue(key, out value)) value = current.ToString();
            string next = NumericField(key, value, max); edits[key] = next;
            if (immediate)
            {
                int parsed;
                if (next != value && int.TryParse(next, out parsed))
                {
                    parsed = Mathf.Clamp(parsed, min, max);
                    if (parsed != current) { apply(parsed); notice = caption + " updated to " + parsed + "."; }
                    edits[key] = parsed.ToString();
                }
                // A blank field is allowed while replacing a count; it does not remove items.
                if (GUI.GetNameOfFocusedControl() != key) edits.Remove(key);
            }
            else
            {
            bool submit = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == key;
            if (GUILayout.Button("Set", button, GUILayout.Width(64)) || submit)
            {
                int parsed;
                if (!int.TryParse(next, out parsed) || parsed < min || parsed > max) notice = "Enter a whole number from " + min + " to " + max + ".";
                else { apply(parsed); edits.Remove(key); notice = caption + " updated to " + parsed + "."; GUI.FocusControl(null); }
                if (submit) Event.current.Use();
            }
            }
            GUILayout.EndHorizontal(); GUILayout.Space(6);
        }
        BattleActorAlly BattleActorFor(Actor actor)
        {
            var state = MonoSingleton<GameStateManager>.Instance;
            if (state == null || state.CurrentState == null || state.CurrentStateType != GameStateManager.EGameState.BattleState) return null;
            var battle = FindObjectOfType<BattleField>();
            return battle == null || battle.BattleActorsList == null ? null : battle.BattleActorsList.OfType<BattleActorAlly>().FirstOrDefault(a => a.ID == actor.ID);
        }
        void Characters()
        {
            var actors = GameUtils.PartyMembers.GetAllPartyActors();
            GUILayout.Label("Your characters", heading);
            if (actors.Count == 0) { GUILayout.Label("No party members yet.", muted); return; }
            if (!actors.Any(a => a.ID == selectedId)) selectedId = actors[0].ID;
            actorScroll = GUILayout.BeginScrollView(actorScroll, GUILayout.Height(51)); GUILayout.BeginHorizontal();
            foreach (var entry in actors) if (GUILayout.Button(entry.ConcreteData.Name, entry.ID == selectedId ? active : button)) { selectedId = entry.ID; edits.Clear(); scroll = Vector2.zero; }
            GUILayout.EndHorizontal(); GUILayout.EndScrollView(); GUILayout.Space(8);
            Actor actor = actors.First(a => a.ID == selectedId);
            var battleActor = BattleActorFor(actor);
            Stats stats = battleActor == null ? actor.PersistentStats : battleActor.Stats;
            GUILayout.Label(battleActor == null ? "Persistent character data" : "In battle • current resources update in this battle too", muted);
            scroll = GUILayout.BeginScrollView(scroll);
            // Levels change unlocked skills. Apply between battles to avoid a stale turn/action queue.
            GUI.enabled = battleActor == null;
            Number("level", "Level", actor.CurrentLevel, 1, 999, delegate(int v) { actor.SetLevel(v); actor.PersistentStats.UpdateStats(actor); actor.ActorDataUpdated(); });
            GUI.enabled = true;
            if (battleActor != null) GUILayout.Label("Change levels between battles to refresh unlocked skills.", muted);
            Number("hp", "Current HP", stats.CurrentHP, battleActor == null ? 0 : 1, stats.GetMaxHP(), v => ChangeStat(actor, battleActor, "hp", v));
            Number("morale", "Current SP / morale", stats.CurrentMorale, 0, stats.GetMaxMorale(), v => ChangeStat(actor, battleActor, "morale", v));
            GUILayout.Space(10); GUILayout.Label("Base stats", heading); GUILayout.Label("Equipment and temporary bonuses are added separately. Maximum overrides stay fixed when leveling.", muted);
            foreach (string stat in new[] { "hp", "morale", "attack", "defense", "agility", "luck" })
            {
                string key = stat;
                bool pool = key == "hp" || key == "morale";
                int current = pool ? stats.GetMaxStatAtLevel(key, actor.CurrentLevel) : stats.GetStatValue(key, true);
                if (current < 0) continue;
                string caption = key == "hp" ? "Maximum HP (base)" : key == "morale" ? "Maximum SP (base)" : char.ToUpper(key[0]) + key.Substring(1);
                Number("base_" + key, caption, current, key == "hp" || key == "attack" ? 1 : 0, pool ? 99999 : 2000, delegate(int value)
                {
                    if (pool)
                    {
                        Values(actor.PersistentStats)[Prefix + key] = value;
                        if (battleActor != null) Values(stats)[Prefix + key] = value;
                        ChangeStat(actor, battleActor, key, Mathf.Min(stats.GetStatValue(key), key == "hp" ? stats.GetMaxHP() : stats.GetMaxMorale()));
                        RefreshBattleTotals(battleActor);
                    }
                    else ChangeStat(actor, battleActor, key, value);
                    actor.ActorDataUpdated();
                });
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Restore HP + SP", active)) { ChangeStat(actor, battleActor, "hp", stats.GetMaxHP()); ChangeStat(actor, battleActor, "morale", stats.GetMaxMorale()); notice = "Resources restored."; edits.Clear(); }
            GUILayout.Space(6);
            if (GUILayout.Button("Use normal maximums for this level", button))
            {
                Values(actor.PersistentStats).Remove(Prefix + "hp"); Values(actor.PersistentStats).Remove(Prefix + "morale");
                Values(stats).Remove(Prefix + "hp"); Values(stats).Remove(Prefix + "morale");
                RefreshBattleTotals(battleActor);
                ChangeStat(actor, battleActor, "hp", Mathf.Min(stats.CurrentHP, stats.GetMaxHP()));
                ChangeStat(actor, battleActor, "morale", Mathf.Min(stats.CurrentMorale, stats.GetMaxMorale()));
                edits.Clear(); notice = "Maximum HP and SP now follow the game's level growth.";
            }
            GUILayout.Space(10); GUILayout.Label("Status: " + (battleActor == null ? actor.StatusEffects : battleActor.StatusEffects).CurrentStatus, label);
            if (GUILayout.Button("Clear status effects and buffs", button))
            {
                actor.StatusEffects.ForceRemoveStatus(); actor.PersistentStats.RemoveAllStatBuffs();
                if (battleActor != null) { var old = battleActor.StatusEffects.CurrentStatus; battleActor.StatusEffects.ForceRemoveStatus(); stats.RemoveAllStatBuffs(); battleActor.HandleStatusEffectRemoved(old); }
                actor.ActorDataUpdated(); notice = "Status effects and buffs cleared.";
            }
            GUILayout.EndScrollView();
        }
        static void RefreshBattleTotals(BattleActorAlly battle)
        {
            if (battle == null) return;
            SetPrivate(battle, "m_totalHP", battle.Stats.GetMaxHP()); SetPrivate(battle, "m_totalMorale", battle.Stats.GetMaxMorale());
        }
        static void ChangeStat(Actor actor, BattleActorAlly battle, string key, int value)
        {
            actor.PersistentStats.AddToStat(key, value, true);
            if (battle != null) battle.Stats.AddToStat(key, value, true);
            actor.ActorDataUpdated();
        }
        void InventoryView()
        {
            GUILayout.Label("Inventory", heading);
            GUILayout.Label("Counts update as you type. Equipped copies are preserved. Clear a field to replace it; type 0 to remove an item.", muted);
            search = GUILayout.TextField(search, field); GUILayout.Space(6);
            bool owned = GUILayout.Toggle(onlyOwned, "Show Only Owned Items");
            if (owned != onlyOwned) { onlyOwned = owned; scroll = Vector2.zero; }
            string[] categories = new[] { "All" }.Concat(Enum.GetNames(typeof(EItemType))).ToArray();
            int next = GUILayout.SelectionGrid(category, categories, 4, button);
            if (next != category) { category = next; scroll = Vector2.zero; }
            GUILayout.Space(8);
            var manager = MonoSingleton<GameDataManager>.Instance;
            if (manager == null) return;
            var items = manager.GetHolderByType<ItemData>().GetAll<ItemData>();
            var filtered = items.Where(i => i != null && (!onlyOwned || GameUtils.Inventory.GetAmount(i) > 0) && (category == 0 || i.Type.ToString() == categories[category]) && (string.IsNullOrEmpty(search) || (i.Name + " " + i.ID).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)).OrderBy(i => i.Name).ToList();
            GUILayout.Label(filtered.Count + " items  •  Search by name or ID", muted);
            scroll = GUILayout.BeginScrollView(scroll);
            foreach (var item in filtered)
            {
                var selected = item;
                int equipped = GameUtils.Inventory.GetItems(item).Count(i => GameUtils.Inventory.IsItemInUse(i.UniqueID));
                Number("item_" + item.ID, item.Name, GameUtils.Inventory.GetAmount(item), equipped, 999, v => SetItemCount(selected, v), true);
                GUILayout.Label(item.Type + "  /  " + item.ID + (equipped > 0 ? "  /  " + equipped + " equipped" : ""), muted);
            }
            GUILayout.EndScrollView();
        }
        public static void SetItemCount(ItemData item, int target)
        {
            var inv = GameUtils.Inventory;
            var copies = inv.GetItems(item);
            int equipped = copies.Count(i => inv.IsItemInUse(i.UniqueID));
            if (target < equipped || target > 999) throw new ArgumentOutOfRangeException("target");
            if (target > copies.Count)
                for (int i = copies.Count; i < target; i++) { if (inv.Add(item) == null) throw new InvalidOperationException("The game could not create this item."); }
            else
            {
                int remove = copies.Count - target;
                foreach (var copy in copies.Where(i => !inv.IsItemInUse(i.UniqueID)).Take(remove))
                    if (!inv.Consume(copy)) throw new InvalidOperationException("The game refused to remove this item.");
            }
        }
        void Exploration()
        {
            GUILayout.Label("Explore freely", heading); GUILayout.Space(12);
            GUILayout.BeginVertical(card);
            GUILayout.Label("Noclip", heading);
            GUILayout.Label("Walk through walls and directional barriers using normal movement. NPC-occupied cells and map edges remain blocked. Tile events still run.", muted);
            GUILayout.Space(10);
            var state = MonoSingleton<GameStateManager>.Instance;
            bool world = state != null && state.CurrentState != null && state.CurrentStateType == GameStateManager.EGameState.WorldState;
            var manager = MonoSingleton<EntityManager>.Instance;
            GUI.enabled = Noclip || (world && manager != null && manager.PlayerExist && manager.PlayerEntity != null);
            if (GUILayout.Button(Noclip ? "Noclip enabled  •  Turn off" : "Enable noclip", Noclip ? active : button))
            {
                Noclip = !Noclip;
                notice = Noclip ? "Noclip enabled. Close the menu to move." : "Noclip disabled. Any step already in progress will finish when you close the menu.";
            }
            GUI.enabled = world && lastSafeTile != null && manager != null && manager.PlayerExist && manager.PlayerEntity != null && !manager.PlayerEntity.Agent.IsMoving;
            if (GUILayout.Button("Return to last safe tile", button))
            { manager.PlayerEntity.Agent.TeleportAgent(lastSafeTile, EDirection.Down); Noclip = false; notice = "Returned to the last safe tile."; }
            GUI.enabled = true;
            GUILayout.Label("Noclip resets on load and location changes. You can turn it off at any time. If inside a wall, re-enable it to reach normal ground. Return to safe tile is available when standing still.", muted);
            GUILayout.EndVertical();
        }
        void Settings()
        {
            GUILayout.Label("Make yourself comfortable", heading); GUILayout.Space(12);
            GUILayout.BeginVertical(card); GUILayout.Label("Menu shortcut: " + CheatMod.ShortcutLabel, label);
            if (GUILayout.Button(rebinding ? "Press a shortcut… Esc cancels" : "Change shortcut", rebinding ? active : button)) { rebinding = true; GUI.FocusControl(null); }
            if (GUILayout.Button("Reset to Ctrl+C", button)) { CheatMod.Bind(KeyCode.C, true, false, false); rebinding = false; notice = "Shortcut reset to Ctrl+C."; }
            GUILayout.Label("Saved in UserData/MelonPreferences.cfg. No other UI mod is required.", muted);
            GUILayout.EndVertical(); GUILayout.Space(10);
            GUILayout.BeginVertical(card); GUILayout.Label("Interface size", label);
            uiScale = GUILayout.HorizontalSlider(uiScale, .75f, 1f); GUILayout.Label(Mathf.RoundToInt(uiScale * 100) + "% • fitted to your screen", muted);
            GUILayout.EndVertical(); GUILayout.Space(12);
            GUILayout.Label("Inventory, levels, base stats and resources are stored with normal game saves. This menu never saves progress automatically. Save to a separate slot before experimenting.", muted);
            GUILayout.Space(12); GUILayout.Label("HP + SP", heading);
            GUILayout.Label("This game calls its skill resource morale / SP. It has no separate MP stat. Base combat stats exclude equipment bonuses. Maximum HP / SP overrides are stored with each character and copied into battles.", muted);
            GUILayout.Space(12); GUILayout.Label(CheatMod.ShortcutLabel + " opens the menu. Esc closes it. Numeric fields apply with Set or Enter. Level changes are available outside battle; other edits update the current battle as well. Current battle HP cannot be set below 1 to avoid interrupting death sequences.", muted);
        }
    }
}
