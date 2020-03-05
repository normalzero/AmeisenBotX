﻿using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.WowObject.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RawWowUnit
    {
        public ulong Charm;
        public ulong Summon;
        public ulong Critter;
        public ulong CharmedBy;
        public ulong SummonedBy;
        public ulong CreatedBy;
        public ulong Target;
        public ulong ChannelObject;
        public int ChannelSpell;
        public fixed byte Bytes0[4];
        public int Health;
        public int Power1;
        public int Power2;
        public int Power3;
        public int Power4;
        public int Power5;
        public int Power6;
        public int Power7;
        public int MaxHealth;
        public int MaxPower1;
        public int MaxPower2;
        public int MaxPower3;
        public int MaxPower4;
        public int MaxPower5;
        public int MaxPower6;
        public int MaxPower7;
        public fixed float PowerRegenModifier[7];
        public fixed float PowerRegenInterruptedModifier[7];
        public int Level;
        public int FactionTemplate;
        public fixed int VirtualItemSlotId[3];
        public int Flags1;
        public int Flags2;
        public int AuraState;
        public fixed int BaseAttackTime[2];
        public int RangeAttackTime;
        public float BoundingRadius;
        public float CombatReach;
        public int DisplayId;
        public int NativeDisplayId;
        public int MountDisplayId;
        public float MinDamage;
        public float MaxDamage;
        public float MinOffhandDamage;
        public float MaxOffhandDamage;
        public fixed byte Bytes1[4];
        public int PetNumber;
        public int PetNameTimestamp;
        public int PetExperience;
        public int PetNextLevelXp;
        public int DynamicFlags;
        public float CastSpeed;
        public int CreatedBySpell;
        public int NpcFlags;
        public int NpcEmoteState;
        public int Stat0;
        public int Stat1;
        public int Stat2;
        public int Stat3;
        public int Stat4;
        public int PosStat0;
        public int PosStat1;
        public int PosStat2;
        public int PosStat3;
        public int PosStat4;
        public int NegStat0;
        public int NegStat1;
        public int NegStat2;
        public int NegStat3;
        public int NegStat4;
        public fixed int Resistances[7];
        public fixed int ResistanceModsBuffPositive[7];
        public fixed int ResistanceModsBuffNegative[7];
        public int BaseMana;
        public int BaseHealth;
        public int Bytes2;
        public int AttackPower;
        public fixed short AttackPowerMods[2];
        public float AttackPowerMultiplier;
        public int RangedAttackPower;
        public fixed short RangedAttackPowerMods[2];
        public float RangedAttackPowerMultiplier;
        public float MinRangedDamage;
        public float MaxRangedDamage;
        public fixed int PowerCostModifier[7];
        public fixed float PowerCostMultiplier[7];
        public float MaxHealthModifier;
        public float HoverHeight;
        public int WowUnitPadding;

        public const int EndOffset = 568;
    }
}