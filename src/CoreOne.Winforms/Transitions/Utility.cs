using CoreOne.Reflection;
using System.ComponentModel;

namespace CoreOne.Winforms.Transitions;

internal class Utility
{
    public static double ConvertLinearToAcceleration(double elapsed) => elapsed * elapsed;

    public static double ConvertLinearToDeceleration(double elapsed) => elapsed * (2.0 - elapsed);

    public static double ConvertLinearToEaseInEaseOut(double elapsed)
    {
        double firstHalfTime = elapsed > 0.5 ? 0.5 : elapsed;
        double secondHalfTime = elapsed > 0.5 ? elapsed - 0.5 : 0.0;
        return (2 * firstHalfTime * firstHalfTime) + (2 * secondHalfTime * (1.0 - secondHalfTime));
    }

    public static object? GetValue(object model, string propertyName)
    {
        var type = model.GetType();
        var meta = MetaType.GetMetadata(type, propertyName);
        return meta.GetValue(model);
    }

    public static double Interpolate(double d1, double d2, double percentage)
    {
        var difference = d2 - d1;
        var distance = difference * percentage;
        return d1 + distance;
    }

    public static int Interpolate(int i1, int i2, double percentage) => (int)Interpolate(i1, (double)i2, percentage);

    public static float Interpolate(float f1, float f2, double percentage) => (float)Interpolate((double)f1, (double)f2, percentage);

    public static void RaiseEvent(EventHandler? handler, object sender)
    {
        if (handler == null)
            return;

        var args = EventArgs.Empty;
        foreach (var h in handler.GetInvocationList().Cast<EventHandler>())
        {
            try
            {
                if (h.Target is ISynchronizeInvoke target && target.InvokeRequired)
                    target.Invoke(h, [sender, args]);
                else
                    h(sender, args);
            }
            catch (Exception) { }
        }
    }

    public static void RaiseEvent<T>(EventHandler<T>? handler, object sender, T args) where T : EventArgs
    {
        if (handler == null)
            return;

        foreach (var h in handler.GetInvocationList().Cast<EventHandler<T>>())
        {
            try
            {
                if (h.Target is ISynchronizeInvoke target && target.InvokeRequired)
                    target.Invoke(h, [sender, args]);
                else
                    h(sender, args);
            }
            catch (Exception) { }
        }
    }

    public static void SetValue(object? target, string propertyName, object value)
    {
        MetaType.GetMetadata(target?.GetType(), propertyName)
            .SetValue(target, value);
    }
}