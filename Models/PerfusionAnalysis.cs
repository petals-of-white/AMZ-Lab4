using System.Drawing;

namespace Models;

public static class PerfusionAnalysis
{
    public static float Baseline((float value, float time) [] input)
    {
        return input.Take(5).Select(point => point.value).Average();
    }

    public static float FWHM((float time, float value) [] input, (float time, float value) maxPE, float baseline)
    {
        var indexPE = Array.FindIndex(input, point => point.time == maxPE.time);
        var rozmakh = maxPE.value - baseline;
        var halflevel = baseline + rozmakh * 0.5f;

        float left50 = 0, right50 = 0;

        for (var i = indexPE; i >= 0; i--)
        {
            if (input [i].value <= halflevel)
            {
                left50 = StraightLineX(
                    new PointF(input [i].time, input [i].value),
                    new PointF(input [i + 1].time, input [i + 1].value),
                    halflevel);

                break;
            }
        }
        for (var i = indexPE; i < input.Length; i++)
        {
            if (input [i].value <= halflevel)
            {
                right50 = StraightLineX(
                    new PointF(input [i].time, input [i].value),
                    new PointF(input [i - 1].time, input [i - 1].value),
                    halflevel);

                break;
            }
        }

        return right50 - left50;
    }

    public static (float time, float value) PE((float time, float value) [] input) =>
                input.MaxBy(point => point.value);

    public static float RTTP((float time, float value) maxPE, float t0) => maxPE.time - t0;

    public static float StraightLineX(PointF p1, PointF p2, float y) =>
        (y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X;

    public static float T0((float time, float value) [] input, (float time, float value) maxPE, float baseline)
    {
        var rozmakh = maxPE.value - baseline;
        var level = baseline + rozmakh * 0.1f;
        var indexPE = Array.FindIndex(input, point => point.time == maxPE.time);
        for (var i = indexPE; i >= 0; i--)
        {
            if (input [i].value <= level)
            {
                (float x1, float y1) = input [i];
                (float x2, float y2) = input [i + 1];
                float y3 = level;
                float x3 = (y3 - y1) * (x2 - x1) / (y2 - y1) + x1;
                return x3;
            }
        }

        throw new ArgumentException("t0 not found");
        //input.Where(point => point.time == maxPE.time).Single()
    }

    public static float TRec((float time, float value) [] input, int indexPE)
    {
        //var indexPE = Array.FindIndex(input, point => point.time == maxPEtime);
        return input [indexPE + 3].time;
    }

    public static float TTP((float time, float value) maxPE) => maxPE.time;

    public static float WiR((float time, float value) [] input, int indexT0, int indexPE)
    {
        float maxAngle = 0;
        for (var i = indexT0; i < indexPE; i++)
        {
            var tanTheta = (input [i + 1].value - input [i].value) / (input [i + 1].time - input [i].time);
            float currentAngle = float.Atan(tanTheta);

            maxAngle = float.Max(Math.Abs(currentAngle), maxAngle);
        }
        return maxAngle;
    }

    public static float WoR((float time, float value) [] input, int indexTrec, int indexPE)
    {
        float maxAngle = 0;
        for (var i = indexPE; i < indexTrec; i++)
        {
            var tanTheta = (input [i + 1].value - input [i].value) / (input [i + 1].time - input [i].time);
            float currentAngle = float.Atan(tanTheta);

            maxAngle = float.Max(Math.Abs(currentAngle), maxAngle);
        }
        return maxAngle;
    }
}