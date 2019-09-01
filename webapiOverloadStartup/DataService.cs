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

    public interface IExternalService
    {
        List<string> GetValues();
    }

    public class ExternalService : IExternalService
    {
        IDependencyForExternalService _dependencyForExternalService;
        public ExternalService(IDependencyForExternalService dependencyForExternalService)
        {
            _dependencyForExternalService = dependencyForExternalService;
        }

        public virtual List<string> GetValues()
        {
            return new List<string> { "value1", "value2" };
        }
    }

    public interface IDependencyForExternalService { }

    public class DependencyForExternalService : IDependencyForExternalService
    {
        public DependencyForExternalService()
        {
        }
    }
}
