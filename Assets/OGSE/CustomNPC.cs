using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ogse
{



    // Новые группы добавлять только в конец
    public enum NpcGroup
    {
        SIDOROVICH,
        BARMAN,
        BARMEN,
        STALKER,
        NOVICE,
        SCIENCE,
        ECOLOG,
        BANDIT,
        DOLG,
        SVOBODA,
        MILITARY,
        MONOLIT,
        CLEARSKY,
        RENEGAT,
        ZOMBIED,
        KILLER
    }

    public class CustomNPC : MonoBehaviour
    {
        bool _isQuestPerson;
        bool _isZombied;
        bool _isFriendly;
        bool _isTrader;

        bool _isAlive;


        Animator m_animator;

        /// <summary>
        /// На правой руке NPC в Prefab к bip01_r_hand добавлен WeaponPoint 
        /// </summary>
        [Header("Prefab with all weapons")]
        [SerializeField]
        GameObject WeaponAttachPoint;
        [SerializeField]
        GameObject[] WeaponList;


        void Awake()
        {
            m_animator = GetComponent<Animator>();
        }


        void Start()
        {

        }


        void Update()
        {

        }
    }
}
