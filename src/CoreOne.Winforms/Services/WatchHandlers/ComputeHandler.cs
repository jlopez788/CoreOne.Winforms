using CoreOne.Threading.Tasks;
using CoreOne.Winforms.Attributes;
using System.Reflection;

namespace CoreOne.Winforms.Services.WatchHandlers;

/// <summary>
/// Handler for Compute attributes that executes a method to compute property values
/// </summary>
public class ComputeHandler : WatchFactoryFromAttribute<ComputeAttribute>, IWatchFactory
{
    private class ComputeHandlerInstance(PropertyGridItem gridItem, ComputeAttribute attribute) : WatchHandler(gridItem.Property)
    {
        private readonly List<Func<object?, object?>> _parameterGetters = [];
        private bool _hasResult;
        private InvokeCallback _invokeCallback = InvokeCallback.Empty;

        protected override void OnInitialize(object model)
        {
            var modelType = model.GetType();
            var method = modelType.GetMethod(attribute.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (method is null)
                return;

            var parameters = method.GetParameters();

            var properties = MetaType.GetMetadatas(modelType).ToDictionary();

            _invokeCallback = MetaType.GetInvokeMethod(method);
            _hasResult = method.ReturnType != Types.Void;

            gridItem.InputControl?.Enabled = false;
            if (parameters?.Length > 0)
            {
                foreach (var parameter in parameters)
                {
                    if (parameter.ParameterType == modelType)
                    {
                        _parameterGetters.Add(m => m);
                    }
                    else if (!string.IsNullOrEmpty(parameter.Name) && properties.TryGetValue(parameter.Name, out var meta))
                    {
                        _parameterGetters.Add(m => {
                            var value = m is null ? parameter.ParameterType.GetDefault() : meta.GetValue(m);
                            return Convert.ChangeType(value, parameter.ParameterType);
                        });
                    }
                }
            }
        }

        protected override void OnRefresh(object model)
        {
            if (_invokeCallback.Equals(InvokeCallback.Empty))
                return;

            var args = _parameterGetters.Select(getter => getter(model)).ToArray();
            var result = _invokeCallback.Invoke(model, args);

            if (_hasResult)
            {
                result = Convert.ChangeType(result, Property.FPType);
                var flag = Property.SetValue(model, result);
                Property.Setter?.Invoke(model, result);
                gridItem.SetValue?.Invoke(result);
            }
        }
    }

    protected override IWatchHandler? OnCreateInstance(PropertyGridItem gridItem, ComputeAttribute[] attributes)
    {
        return new ComputeHandlerInstance(gridItem, attributes[0]);
    }
}