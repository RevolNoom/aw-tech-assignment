namespace AwingTest.Server.Utility
{
    public static class Utility
    {
        const double Epsilon = 1e-6;
        public static bool ApproxEqual(double a, double b, double tolerance = Utility.Epsilon)
        {
            return Math.Abs(a - b) <= tolerance;
        }
    }
}