using System;

namespace Aimtec.SDK.Orbwalking
{
    using Aimtec.SDK.Menu;

    /// <summary>
    /// </summary>
    public interface IOrbwalker : IDisposable
    {
        /// <summary>
        ///     Gets the animation time.
        /// </summary>
        /// <value>
        ///     The animation time.
        /// </value>
        float AnimationTime { get; }
       
        /// <summary>
        ///     Gets the wind up time.
        /// </summary>
        /// <value>
        ///     The wind up time.
        /// </value>
        float WindUpTime { get; }


        /// <summary>
        ///     Gets the if the player can move.
        /// </summary>
        /// <value>
        ///     If the player can move.
        /// </value>
        bool CanMove { get; }

        /// <summary>
        ///     Gets the if the player can attack.
        /// </summary>
        /// <value>
        ///     If the player can attack.
        /// </value>
        bool CanAttack { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether the orbwalker should disable attacking.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the orbwalker should disable attacking; otherwise, <c>false</c>.
        /// </value>
        bool DisableAttacks { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the orbwalker should disable moving.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the orbwalker should disable moving; otherwise, <c>false</c>.
        /// </value>
        bool DisableMove { get; set; }

        /// <summary>
        ///     Gets or sets the mode.
        /// </summary>
        /// <value>
        ///     The mode.
        /// </value>
        OrbwalkingMode Mode { get; set; }

        /// <summary>
        ///     Gets the target.
        /// </summary>
        /// <returns></returns>
        AttackableUnit GetTarget();

        /// <summary>
        ///     Resets the automatic attack timer.
        /// </summary>
        void ResetAutoAttackTimer();

        /// <summary>
        ///     Adds to menu.
        /// </summary>
        /// <param name="menu">The menu.</param>
        void AddToMenu(IMenu menu);

        /// <summary>
        ///     Forces the target.
        /// </summary>
        /// <param name="unit">The unit.</param>
        void ForceTarget(AttackableUnit unit);

        /// <summary>
        ///     Occurs when the orbwalking is about to launch an attack.
        /// </summary>
        event EventHandler<PreAttackEventArgs> PreAttack;

        /// <summary>
        ///     Occurs when after an attack has been launched and acknowledged by the server.
        /// </summary>
        event EventHandler<PostAttackEventArgs> PostAttack;

        /// <summary>
        ///     Occurs before a movement order is issued.
        /// </summary>
        event EventHandler<PreMoveEventArgs> PreMove;

    }

    /// <summary>
    ///     A base class used by the Orbwalker for events.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class OrbwalkingEventArgs : EventArgs
    {
        /// <summary>
        ///     Gets or sets the target.
        /// </summary>
        /// <value>
        ///     The target.
        /// </value>
        public AttackableUnit Target { get; set; }
    }

    /// <summary>
    ///     The event arguements for the <see cref="IOrbwalker.PreAttack" /> event.
    /// </summary>
    /// <seealso cref="Aimtec.SDK.Orbwalking.OrbwalkingEventArgs" />
    public class PreAttackEventArgs : OrbwalkingEventArgs
    {
        public bool Cancel { get; set; } = false;
    }

    /// <summary>
    ///     The event arguements for the <see cref="IOrbwalker.PostAttack" /> event.
    /// </summary>
    /// <seealso cref="Aimtec.SDK.Orbwalking.OrbwalkingEventArgs" />
    public class PostAttackEventArgs : OrbwalkingEventArgs
    {
    }

    /// <summary>
    ///     The event arguements for the <see cref="IOrbwalker.PreMove" /> event.
    /// </summary>
    /// <seealso cref="Aimtec.SDK.Orbwalking.OrbwalkingEventArgs" />
    public class PreMoveEventArgs : EventArgs
    {
        public Vector3 MovePosition { get; set; }
        public bool Cancel { get; set; }
    }
}