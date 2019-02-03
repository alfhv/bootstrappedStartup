using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webapiOverloadStartup
{
    public interface IDataService
    {
        List<string> GetValues();
    }

    public class DataService : IDataService
    {
        public List<string> GetValues()
        {
            return new List<string> { "value1", "value2" };
        }
    }
}
