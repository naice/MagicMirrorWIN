using System;
using System.Reflection;
using System.Threading.Tasks;

namespace NETStandard.RestServer
{
    internal class ExposedRestServerAction
    {
        public Type InputType { get; set; }
        public Type OutputType { get; set; }
        public string Route { get; set; }
        public string[] Methods { get; set; }
        public ExposedRestServerService RestServerService { get { return _RestServerService; } }

        private readonly ExposedRestServerService _RestServerService;
        private readonly MethodInfo _methodInfo;

        public ExposedRestServerAction(ExposedRestServerService RestServerService, MethodInfo methodInfo)
        {
            _RestServerService = RestServerService;
            _methodInfo = methodInfo;
        }

        public async Task<object> Execute(RestServerHttpContext context, object param)
        {
            // VOID
            if (OutputType == null && InputType == null)
            {
                ExecuteVoid(context);
                return null;
            }
            if (OutputType == null)
            {
                ExecuteVoid(context, param);
                return null;
            }
            if (OutputType == typeof(Task) && InputType == null)
            {
                await ExecuteVoidAsync(context);
                return null;
            }
            if (OutputType == typeof(Task))
            {
                await ExecuteVoidAsync(context, param);
                return null;
            }

            // RESULT
            if (IsGenericTaskType(OutputType) && InputType == null)
            {
                return await ExecuteAsync(context);
            }
            if (IsGenericTaskType(OutputType))
            {
                return await ExecuteAsync(context, param);
            }
            if (InputType == null)
            {
                return ExecuteInternal(context);
            }

            return ExecuteInternal(context, param);
        }

        private object ExecuteInternal(RestServerHttpContext context, object param)
        {
            return _methodInfo.Invoke(_RestServerService.GetInstance(context), new object[] { param });
        }
        private object ExecuteInternal(RestServerHttpContext context)
        {
            return _methodInfo.Invoke(_RestServerService.GetInstance(context), new object[0]);
        }
        private async Task<object> ExecuteAsync(RestServerHttpContext context, object param)
        {
            return await (dynamic)_methodInfo.Invoke(_RestServerService.GetInstance(context), new object[] { param });
        }
        private async Task<object> ExecuteAsync(RestServerHttpContext context)
        {
            return await (dynamic)_methodInfo.Invoke(_RestServerService.GetInstance(context), new object[0]);
        }
        private async Task ExecuteVoidAsync(RestServerHttpContext context, object param)
        {
            await (Task)_methodInfo.Invoke(_RestServerService.GetInstance(context), new object[] { param });
        }
        private async Task ExecuteVoidAsync(RestServerHttpContext context)
        {
            await (Task)_methodInfo.Invoke(_RestServerService.GetInstance(context), new object[0]);
        }
        private void ExecuteVoid(RestServerHttpContext context, object param)
        {
            _methodInfo.Invoke(_RestServerService.GetInstance(context), new object[] { param });
        }
        private void ExecuteVoid(RestServerHttpContext context)
        {
            _methodInfo.Invoke(_RestServerService.GetInstance(context), new object[0]);
        }

        private static bool IsGenericTaskType(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Task<>);
        }
    }
}