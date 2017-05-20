using System;


namespace Aimtec.SDK.Menu
{
    /// <summary>
    /// The Arguments for ValueChanged Event
    /// </summary>
    public class ValueChangedArgs : EventArgs
    {
        private MenuComponent previousValue;

        private MenuComponent newValue;

        /// <summary>
        /// The internal name of the Menu Component that fired this event
        /// </summary>
        public string InternalName { get; }

        /// <summary>
        ///     Creates a new instance of the ValueChangedArgs class
        /// </summary>
        /// <param name="oldVal"></param>
        /// <param name="newVal"></param>
        public ValueChangedArgs(MenuComponent oldVal, MenuComponent newVal)
        {
            this.previousValue = oldVal;
            this.newValue = newVal;
            this.InternalName = newVal.InternalName;
        }


        /// <summary>
        /// Gets the previous instance before the Value Changed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetPreviousValue<T>() where T : MenuComponent
        {
            return (T) this.previousValue;
        }

        /// <summary>
        /// Gets the new or current instance after the Value Changed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetNewValue<T>() where T : MenuComponent
        {
            return (T)this.newValue;
        }
    }

}
