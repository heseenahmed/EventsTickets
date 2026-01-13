using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetLinker.Application.Common.Caching
{
    public static class CacheKeys
    {
        public const string UsersAll = "users:all";
        public const string RolesAll = "roles:all";

        public static string UserById(string id)
            => $"users:{id}";
    }
}
