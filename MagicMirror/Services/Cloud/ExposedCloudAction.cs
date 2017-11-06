using System;
using System.Reflection;
using System.Threading.Tasks;

namespace MagicMirror.Services.Cloud
{
    internal class ExposedCloudAction
    {
        public Type InputType { get; set; }
        public Type OutputType { get; set; }
        public string Route { get; set; }
        public string[] Methods { get; set; }
        public ExposedCloudService CloudService { get { return _cloudService; } }

        private readonly ExposedCloudService _cloudService;
        private readonly MethodInfo _methodInfo;

        public ExposedCloudAction(ExposedCloudService cloudService, MethodInfo methodInfo)
        {
            _cloudService = cloudService;
            _methodInfo = methodInfo;
        }

        public async Task<object> Execute(CloudHttpContext context, object param)
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

        private object ExecuteInternal(CloudHttpContext context, object param)
        {
            return _methodInfo.Invoke(_cloudService.GetInstance(context), new object[] { param });
        }
        private object ExecuteInternal(CloudHttpContext context)
        {
            return _methodInfo.Invoke(_cloudService.GetInstance(context), new object[0]);
        }
        private async Task<object> ExecuteAsync(CloudHttpContext context, object param)
        {
            return await (dynamic)_methodInfo.Invoke(_cloudService.GetInstance(context), new object[] { param });
        }
        private async Task<object> ExecuteAsync(CloudHttpContext context)
        {
            return await (dynamic)_methodInfo.Invoke(_cloudService.GetInstance(context), new object[0]);
        }
        private async Task ExecuteVoidAsync(CloudHttpContext context, object param)
        {
            await (Task)_methodInfo.Invoke(_cloudService.GetInstance(context), new object[] { param });
        }
        private async Task ExecuteVoidAsync(CloudHttpContext context)
        {
            await (Task)_methodInfo.Invoke(_cloudService.GetInstance(context), new object[0]);
        }
        private void ExecuteVoid(CloudHttpContext context, object param)
        {
            _methodInfo.Invoke(_cloudService.GetInstance(context), new object[] { param });
        }
        private void ExecuteVoid(CloudHttpContext context)
        {
            _methodInfo.Invoke(_cloudService.GetInstance(context), new object[0]);
        }

        private static bool IsGenericTaskType(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Task<>);
        }
    }
}