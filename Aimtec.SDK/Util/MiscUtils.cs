namespace Aimtec.SDK.Util
{
    using System;
    using System.Drawing;

    using Aimtec.SDK.Menu;

    using Rectangle = Aimtec.Rectangle;

    public class MiscUtils
    {
        #region Static Fields

        /// <summary>
        ///     The League of Legends font
        /// </summary>
        public static Font LeagueFont = new Font("Arial", 11);

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Calculates the dimensions of the text and returns a array containing the width and height
        /// </summary>
        public static int[] MeasureText(string text)
        {
            var textRect = Render.MeasureText(text, new Rectangle(0, 0, 0, 0), RenderTextFlags.None);

            var width = (textRect.Right - textRect.Left);

            var height = (textRect.Bottom - textRect.Top) + 10;

            return new int[] { width, height };
        }

        /// <summary>
        ///     Calculates the width of the text (not 100% accurate)
        /// </summary>
        [Obsolete(
            "This method is deprecated. Use the MeasureText from this class or directly from Aimtec.Render class.")]
        public static float MeasureTextWidth(string text)
        {
            float textWidth = 0;

            using (var bmp = new Bitmap(1, 1))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    textWidth = g.MeasureString(text, LeagueFont).Width;
                }
            }

            return textWidth;
        }

        #endregion
    }
}