﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Character.Stats {
    /// <summary>
    /// The StatsController is responsible for tracking and reporting on the stats of the character.
    /// Stats are made up of a number of `StatsSO` objects.
    /// </summary>
    public class StatsController : MonoBehaviour
    {
        [Header("Optimization")]
        [SerializeField, Tooltip("How often stats should be processed for changes.")]
        float m_TimeBetweenUpdates = 0.5f;

        [HideInInspector, SerializeField]
        List<StatSO> m_Stats = new List<StatSO>();
        [HideInInspector, SerializeField]
        List<StatsInfluencerSO> m_StatsInfluencers = new List<StatsInfluencerSO>();

        float m_TimeOfLastUpdate = 0;
        float m_TimeOfNextUpdate = 0;
        MemoryController m_Memory;

        private void Awake()
        {
            m_Memory = GetComponent<MemoryController>();
        }

        private void Update()
        {
            if (Time.timeSinceLevelLoad >= m_TimeOfNextUpdate)
            {
                for (int i = 0; i < m_Stats.Count; i++)
                {
                    m_Stats[i].OnUpdate();
                }

                for (int i = 0; i < m_StatsInfluencers.Count; i++)
                {
                    if (m_StatsInfluencers[i] != null)
                    {
                        if (m_StatsInfluencers[i].duration > 0)
                        {
                            float influence = m_StatsInfluencers[i].changePerSecond * (Time.timeSinceLevelLoad - m_TimeOfLastUpdate);
                            ChangeStat(m_StatsInfluencers[i], influence);
                        } else
                        {
                            ChangeStat(m_StatsInfluencers[i], m_StatsInfluencers[i].maxChange);
                        }
                    } else
                    {
                        m_StatsInfluencers.RemoveAt(i);
                    }
                }

                m_TimeOfLastUpdate = Time.timeSinceLevelLoad;
                m_TimeOfNextUpdate = Time.timeSinceLevelLoad + m_TimeBetweenUpdates;
            }
        }

        /// <summary>
        /// Apply an immediate change to a given Stats, if this controller is tracking that stat.
        /// </summary>
        /// 
        /// <param name="influencer">The influencer imparting the change.</param>
        /// <param name="change">The change to make. The result is kept within the -100 to 100 range.</param>
        internal void ChangeStat(StatsInfluencerSO influencer, float change)
        {
            StatSO stat = GetStat(influencer.statName);
            stat.value += change;
            influencer.influenceApplied += change;
            Debug.Log(gameObject.name + " changed stat " + influencer.statName + " by " + change);
        }

        /// <summary>
        /// Get the stat object representing a named stat.
        /// </summary>
        /// <param name="statName"></param>
        /// <returns></returns>
        private StatSO GetStat(string statName)
        {
            StatSO stat;
            // TODO cache results in a dictionary
            for (int i = 0; i < m_Stats.Count; i++)
            {
                if (m_Stats[i].name == statName)
                {
                    return m_Stats[i];
                }
            }

            stat = ScriptableObject.CreateInstance<StatSO>();
            stat.name = statName;
            return stat;
        }

        /// <summary>
        /// Add an influencer to this controller. If this controller is not managing the required stat then 
        /// do nothing. If this character has any memory of being influenced by the object within short term 
        /// memory this new influence will be rejected.
        /// </summary>
        /// <param name="influencer">The influencer to add.</param>
        internal void TryAddInfluencer(StatsInfluencerSO influencer)
        {
            if (m_Memory != null) {
                MemorySO[] memories = m_Memory.RetrieveShortTermMemoriesAbout(influencer.generator);
                if (memories.Length > 0)
                {
                    for (int i = 0; i < memories.Length; i++)
                    {
                        if (Time.timeSinceLevelLoad < memories[i].m_Time + memories[i].cooldown)
                        {
                            Debug.Log("Not adding influence because there is already a recent short term memory present " + influencer);
                            return;
                        }
                    }
                } else
                {
                    m_Memory.AddMemory(influencer);
                }
            }

            StatSO stat = GetStat(influencer.statName);
            m_StatsInfluencers.Add(influencer);
        }
    }
}