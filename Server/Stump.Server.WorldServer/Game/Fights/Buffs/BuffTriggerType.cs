using System;

namespace Stump.Server.WorldServer.Game.Fights.Buffs
{
    public enum BuffTriggerType //Revision Kenshin v.2.61
    {
        // I = Istante
        Instant,

        /* DAMAGE TRIGGER */    
        OnDamaged,            // D = Damaged     
        OnDamagedAir,         // DA = Damaged Air
        OnDamagedEarth,       // DE = Damaged Earth
        OnDamagedFire,        // DF = Damaged Fire
        OnDamagedWater,       // DW = Damaged Water
        OnDamagedNeutral,     // DN = Damaged Neutral
        OnDamagedByAlly,      // DBA = DamagedByAlly
        OnDamagedByEnemy,     // DBE = DamagedByEnemy
        OnDamagedBySummon,    // DI = DamagedBySummon
        OnDamagedByWeapon,    // DC = DamagedByWeapon
        OnDamagedBySpell,     // DS = DamagedBySpell
        OnDamagedByGlyph,     // DG = DamagedByGlyph
        OnDamagedByTrap,      // DP = DamagedByTrap
        OnDamagedInCloseRange,// DM = DamagedInCloseRange
        OnDamagedInLongRange, // DR = DamagedInLongRange
        OnDamagedByPush,      // MD = DamagedByPush
        OnDamagedByEnemyPush, // MDP = DamagedByEnemyPush
        OnDamageEnemyByPush,  // MDM = DamageEnemyByPush
        OnDamagedUnknown_2,   // Dr = Unknown
        OnDamagedUnknown_3,   // DTB = Unknown
        OnDamagedUnknown_4,   // DTE = Unknown
        OnPushDamaged,        // PD = PushDamaged
        OnMakeMeleeDamage,    // CDM = MakeMeleeDamage
        OnMakeDistanceDamage, // CDR = MakeDistanceDamage
        OnInderctlyPush,      // PPD = InderctlyPush
        OnPushDamagedInMelee, // PMD = PushDamagedInMelee

        /* TURN */
        OnTurnBegin,          // TB = TurnBegin
        OnTurnEnd,            // TE = TurnEnd

        /* AP, MP, PO */
        OnMPLost, // m
        OnAPLost, // A
        OnRangeLost, // R
        OnMPAttack, // MPA
        OnAPAttack, // APA

        /* HEAL */
        OnHealed, //H

        /* STATE */
        OnStateAdded, // EO
        OnSpecificStateAdded, //EO#
        OnStateRemoved, //Eo
        OnSpecificStateRemoved, //Eo#

        /* BUFF */
        OnDispelled, //d

        /* OTHERS */
        OnCriticalHit, //CC
        OnDeath, //X

        /* MOVEMENT */
        OnPushed, //MP
        OnMoved, //M
        OnTackled, //tF
        OnTackle, //tS
        HaveMoveDuringTurn, // CMP

        /* UNKNOWN */
        Unknown_3, //mA
        Unknown_4, //ML
        Unknown_6, //MS
        Unknown_7,
        UsedPortal, //PT

        /* CUSTOM */
        BeforeDamaged,
        AfterDamaged,
        BeforeAttack,
        AfterAttack,
        AfterHealed,
        OnHeal,
        AfterHeal,
        OnBuffEnded,
        OnBuffEndedTurnEnd,
        AfterRollCritical,
        AttackWithASpecificState, //EON

        Unknown,


        /*
        *A=lose AP (101)
        *CC=on critical hit
        *d=dispell
        *D=damage
        *DA=damage air
        *DBA=damage on ally
        *DBE=damage on enemy
        *DC=damaged by weapon
        *DE=damage earth
        *DF=damage fire
        *DG=damage from glyph
        *DI=
        *DM=distance between 0 and 1
        *DN=damage neutral
        *DP=damage from trap
        *Dr=
        *DR=distance > 1
        *DS=not weapon
        *DTB=
        *DTE=
        *DW=damage water
        *EO=on add state
        *EO#=on add state #
        *Eo=on state removed
        *Eo#=on state # removed
        *H=on heal
        *I=instant
        *m=lose mp (127)
        *M=OnMoved
        *mA=
        *MD=push damage
        *MDM=receive push damages from enemy push
        *MDP=inflict push damage to enemy
        *ML=
        *MP=Pushed
        *MS=
        *P=
        *R=Lost Range
        *TB=turn begin
        *TE=turn end
        *tF=Tackled
        *tS=Tackle
        *X= Death
        *PD=receive push damage
        */
    }
}