namespace Aimtec.SDK.Util
{
    using System;
    using System.Drawing;

    using Rectangle = Aimtec.Rectangle;

    public class MiscUtils
    {
        /// <summary>
        ///     The League of Legends font
        /// </summary>
        public static Font LeagueFont = new Font("Arial", 11);

        /// <summary>
        ///     Calculates the width of the text (not 100% accurate)
        /// </summary>
        public static int[] MeasureTextWidth(string text)
        {
            var textRect = Aimtec.Render.MeasureText(
                text,
                new Rectangle(0, 0, 0, 0),
              RenderTextFlags.None);

            var width = (textRect.Right - textRect.Left);

            var height = (textRect.Bottom - textRect.Top) + 10;

            return new int[] { width, height };
        }
    }
}
