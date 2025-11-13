using Common.Models;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace DBCore.Overrides;

/// <summary>
/// Переопределение полей команды.
/// </summary>
public class UniqueTeamNameOverride : IAutoMappingOverride<Team>
{
    public void Override(AutoMapping<Team> mapping)
    {
        mapping.Map(x => x.Name)
            .Unique();;
    }
}
