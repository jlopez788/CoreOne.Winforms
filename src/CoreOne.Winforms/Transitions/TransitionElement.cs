namespace CoreOne.Winforms.Transitions;

public class TransitionElement(double endTime, double endValue, InterpolationMethod interpolationMethod)
{
    public double EndTime { get; set; } = endTime;
    public double EndValue { get; set; } = endValue;
    public InterpolationMethod InterpolationMethod { get; set; } = interpolationMethod;
}