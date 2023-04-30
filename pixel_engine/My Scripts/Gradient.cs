using Pixel;

public static class Gradient
{
    /// <summary>
    /// <code>
    ///     Position = the index of the sample to return (between 0 and subdivisions - 1)
    ///     Subdivisions = the amount of positions possible to sample on the gradient
    ///     Alpha = the transparency of the output colors (0 - 255)
    ///     GradientColors = the array to sample
    /// </code>
    /// </summary>
    /// <param name="position"></param>
    /// <param name="subdivisions"></param>
    /// <param name="alpha"></param>
    /// <param name="gradientColors"></param>
    /// <returns></returns>
    public static Color Sample(int position, int subdivisions = 360, byte alpha = 255, Color[]? gradientColors = null)
        {
            if (position >= subdivisions)
                position = subdivisions - 1;

            gradientColors ??= new Color[] { System.Drawing.Color.Red, System.Drawing.Color.Yellow, System.Drawing.Color.Green, System.Drawing.Color.Cyan, System.Drawing.Color.Blue, System.Drawing.Color.Magenta };
            float gradientPos = (float)position / subdivisions;
            int colorSegment = (int)(gradientPos * (gradientColors.Length - 1));
            float segmentPos = (gradientPos * (gradientColors.Length - 1)) - colorSegment;
            Color currentColor = Color.Blend(gradientColors[colorSegment], gradientColors[colorSegment + 1], segmentPos);
            currentColor.a = alpha;
            return currentColor;
        }

}