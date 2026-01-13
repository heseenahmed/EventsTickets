using AutoMapper;
using System.Reflection;
namespace Tickets.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile() => ApplyMappingFromAssembly(Assembly.GetExecutingAssembly());
        private void ApplyMappingFromAssembly(Assembly assembly)
        {
            var mapFromType = typeof(ImapFrom<>);
            var mappingMethodName = nameof(ImapFrom<object>.Mapping);
            bool HasInterface(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == mapFromType;
            var types = assembly.GetExportedTypes().Where(t => t.GetInterfaces().Any(HasInterface)).ToList();
            var argumentType = new Type[] { typeof(Profile) };
            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type);
                var methodInfo = type.GetMethod(mappingMethodName);
                if (methodInfo != null)
                {
                    methodInfo.Invoke(instance, [this]);
                }
                else
                {
                    var intrfaces = type.GetInterfaces().Where(HasInterface).ToList();
                    if (intrfaces.Count > 0)
                    {
                        foreach (var intrface in intrfaces)
                        {
                            var interfaceMethodInfo = intrface.GetMethod(mappingMethodName, argumentType);
                            interfaceMethodInfo?.Invoke(instance, [this]);
                        }
                    }
                }
            }
        }
    }
}
