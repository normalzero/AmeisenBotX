﻿using AmeisenBotX.Core.Data.Cache.Enums;
using AmeisenBotX.Core.Data.CombatLog.Enums;
using AmeisenBotX.Core.Data.CombatLog.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Data.Cache
{
    public interface IAmeisenBotCache
    {
        Dictionary<int, List<Vector3>> BlacklistNodes { get; }

        Dictionary<(CombatLogEntryType, CombatLogEntrySubtype), List<BasicCombatLogEntry>> CombatLogEntries { get; }

        Dictionary<(MapId, HerbNode), List<Vector3>> HerbNodes { get; }

        Dictionary<ulong, string> NameCache { get; }

        Dictionary<(MapId, OreNode), List<Vector3>> OreNodes { get; }

        Dictionary<(MapId, PoiType), List<Vector3>> PointsOfInterest { get; }

        Dictionary<(int, int), WowUnitReaction> ReactionCache { get; }

        Dictionary<int, string> SpellNameCache { get; }

        void CacheBlacklistPosition(int mapId, Vector3 node);

        void CacheCombatLogEntry((CombatLogEntryType, CombatLogEntrySubtype) key, BasicCombatLogEntry entry);

        void CacheHerb(MapId mapId, HerbNode displayId, Vector3 position);

        void CacheName(ulong guid, string name);

        void CacheOre(MapId mapId, OreNode displayId, Vector3 position);

        void CachePoi(MapId mapId, PoiType poiType, Vector3 position);

        void CacheReaction(int a, int b, WowUnitReaction reaction);

        void CacheSpellName(int spellId, string name);

        void Clear();

        void Load();

        void Save();

        bool TryGetBlacklistPosition(int mapId, Vector3 position, double maxRadius, out List<Vector3> nodes);

        bool TryGetPointsOfInterest(MapId mapId, PoiType poiType, Vector3 position, double maxRadius, out List<Vector3> nodes);

        bool TryGetReaction(int a, int b, out WowUnitReaction reaction);

        bool TryGetSpellName(int spellId, out string name);

        bool TryGetUnitName(ulong guid, out string name);
    }
}