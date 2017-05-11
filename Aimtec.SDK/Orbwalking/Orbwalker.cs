using System;

namespace Aimtec.SDK.Orbwalking
{
    using Aimtec.SDK.Damage;
    using Aimtec.SDK.Menu;

    public class Orbwalker : IOrbwalker
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Orbwalker" /> class.
        /// </summary>
        /// <param name="impl">The implementation.</param>
        public Orbwalker(IOrbwalker impl)
        {
            Implementation = impl;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Orbwalker" /> class.
        /// </summary>
        public Orbwalker()
        {
        }

        /// <summary>
        ///     Gets or sets the implementation of the orbwalker.
        /// </summary>
        /// <value>
        ///     The implementation of the orbwalker.
        /// </value>
        public static IOrbwalker Implementation
        {
            get
            {
                return impl;
            }
            set
            {
                impl?.Dispose();
                impl = value;
            }
        }

        private static IOrbwalker impl;

        /// <inheritdoc cref="IOrbwalker" />
        public float AnimationTime => Implementation.AnimationTime;

        /// <inheritdoc cref="IOrbwalker" />
        public float AutoAttackRange => Implementation.AutoAttackRange;

        /// <inheritdoc cref="IOrbwalker" />
        public bool CanMove => Implementation.CanMove;

        /// <inheritdoc cref="IOrbwalker" />
        public bool CanAttack => Implementation.CanAttack;

        /// <inheritdoc cref="IOrbwalker" />
        public bool DisableAttacks
        {
            get { return Implementation.DisableAttacks; }
            set { Implementation.DisableAttacks = value; }
        }

        /// <inheritdoc cref="IOrbwalker" />
        public bool DisableMove
        {
            get { return Implementation.DisableMove; }
            set { Implementation.DisableMove = value; }
        }

        /// <inheritdoc cref="IOrbwalker" />
        public float WindUpTime => Implementation.WindUpTime;

        /// <inheritdoc cref="IOrbwalker" />
        public OrbwalkingMode Mode
        {
            get { return Implementation.Mode; }
            set { Implementation.Mode = value; }
        }

        /// <inheritdoc cref="IOrbwalker" />
        public Obj_AI_Base GetTarget()
        {
            return Implementation.GetTarget();
        }

        /// <inheritdoc cref="IOrbwalker" />
        public void ResetAutoAttackTimer()
        {
            Implementation.ResetAutoAttackTimer();
        }

        /// <inheritdoc cref="IOrbwalker" />
        public void AddToMenu(IMenu menu)
        {
            Implementation.AddToMenu(menu);
        }

        /// <inheritdoc cref="IOrbwalker" />
        public void AutoAttack(AttackableUnit unit)
        {
            Implementation.AutoAttack(unit);
        }

        /// <inheritdoc cref="IOrbwalker" />
        public void ForceTarget(AttackableUnit unit)
        {
            Implementation.ForceTarget(unit);
        }

        /// <inheritdoc cref="IOrbwalker" />
        public bool IsAutoAttack(string missileName)
        {
            return Implementation.IsAutoAttack(missileName);
        }

        /// <inheritdoc cref="IOrbwalker" />
        public event EventHandler<BeforeAttackEventArgs> BeforeAttack
        {
            add { Implementation.BeforeAttack += value; }
            remove { Implementation.BeforeAttack -= value; }
        }

        /// <inheritdoc cref="IOrbwalker" />
        public event EventHandler<AttackEventArgs> Attack
        {
            add { Implementation.Attack += value; }
            remove { Implementation.Attack -= value; }
        }

        /// <inheritdoc cref="IOrbwalker" />
        public event EventHandler<AfterAttackEventArgs> AfterAttack
        {
            add { Implementation.AfterAttack += value; }
            remove { Implementation.AfterAttack -= value; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Implementation.Dispose();
        }
    }
}