using System;

namespace Aimtec.SDK.Orbwalking
{
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
            : this(new OrbwalkingImpl())
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
                if (impl == null)
                {
                    impl = new OrbwalkingImpl();
                }

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
        public float WindUpTime => Implementation.WindUpTime;

        /// <inheritdoc cref="IOrbwalker" />
        public bool CanMove()
        {
            return Implementation.CanMove();
        }


        /// <inheritdoc cref="IOrbwalker" />
        public bool CanAttack => Implementation.CanAttack;

        /// <inheritdoc cref="IOrbwalker" />
        public bool DisableAttacks
        {
            get => Implementation.DisableAttacks;
            set => Implementation.DisableAttacks = value;
        }

        /// <inheritdoc cref="IOrbwalker" />
        public bool DisableMove
        {
            get => Implementation.DisableMove;
            set => Implementation.DisableMove = value;
        }


        /// <inheritdoc cref="IOrbwalker" />
        public OrbwalkingMode Mode
        {
            get => Implementation.Mode;
            set => Implementation.Mode = value;
        }

        /// <inheritdoc cref="IOrbwalker" />
        public AttackableUnit GetTarget()
        {
            return Implementation.GetTarget();
        }

        /// <inheritdoc cref="IOrbwalker" />
        public void ResetAutoAttackTimer()
        {
            Implementation.ResetAutoAttackTimer();
        }

        /// <inheritdoc cref="IOrbwalker" />
        public void Attach(IMenu menu)
        {
            Implementation.Attach(menu);
        }

        /// <inheritdoc cref="IOrbwalker" />
        public void ForceTarget(AttackableUnit unit)
        {
            Implementation.ForceTarget(unit);
        }


        /// <inheritdoc cref="IOrbwalker" />
        public event EventHandler<PreAttackEventArgs> PreAttack
        {
            add => Implementation.PreAttack += value;
            remove => Implementation.PreAttack -= value;
        }

        /// <inheritdoc cref="IOrbwalker" />
        public event EventHandler<PostAttackEventArgs> PostAttack
        {
            add => Implementation.PostAttack += value;
            remove => Implementation.PostAttack -= value;
        }

        /// <inheritdoc cref="IOrbwalker" />
        public event EventHandler<PreMoveEventArgs> PreMove
        {
            add => Implementation.PreMove += value;
            remove => Implementation.PreMove -= value;

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