﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.Character;

namespace WizardsCode.Stats
{
    /// <summary>
    /// A StatSO tracks the base value and current value of a Stat. This can be used by an AI
    /// system to decide on actions to take.
    /// </summary>
    [CreateAssetMenu(fileName = "New Stat", menuName = "Wizards Code/Stats/New Stat")]
    public class StatSO : ScriptableObject
    {
        [Header("Details")]
        [SerializeField, Tooltip("The human readable name for this stat.")]
        string displayName = "No Name Stat";
        [SerializeField, Tooltip("The base value for this stat. This is the value that the character will always trend towards with no external factors influencing the current value."), Range(-100, 100)]
        float m_BaseValue = 0;

        [HideInInspector, SerializeField]
        float m_CurrentValue;

        /// <summary>
        /// Get a human readable description of the current status of this stat.
        /// That is, it's value, whether it is wihtin the desired range etc.
        /// </summary>
        public string statusDescription
        {
            get {
                string msg = name + " is " + value;
                return msg; 
            }
        }

        /// <summary>
        /// Called every tick to allow for the state to be updated over time.
        /// </summary>
        internal virtual void OnUpdate()
        {
            // Do nothing by default
        }

        /// <summary>
        /// Set the current value of this stat. If an attempt is made to set the value 
        /// outside the allowable range (-100 to 100) then the value will
        /// be adjusted to fit this range.
        /// </summary>
        public float value {
            get { return m_CurrentValue; }
            internal set
            {
                m_CurrentValue = Mathf.Clamp(value, -100, 100);
            }
        }

        private void Awake()
        {
            m_CurrentValue = m_BaseValue;
        }
    }
}
