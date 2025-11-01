using System.Linq;
using System.Reflection;
using CollectionLibrary.Nhibernate.Infrastructure;
using FluentNHibernate.Automapping;
using FluentNHibernate.Mapping;

namespace NHibernate.Infrastructure;

public class StoreConfiguration : DefaultAutomappingConfiguration
{
    public override bool ShouldMap(System.Type type)
    {
        return type.Namespace == "CommonModels.Models";
    }
}