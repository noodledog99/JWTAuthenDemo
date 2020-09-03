using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWTAuthenDemo.Services
{
    public class DbConfig : IDbConfig
    {
        public string ConnectionString { get; set; }
        public string DbName { get; set; }
        public string Users { get; set; }
    }
}
