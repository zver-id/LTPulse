using Common.Models;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace DBCore.Overrides;

/// <summary>
/// Переопределение полей пользователя.
/// </summary>
public class EmployeeOverride : IAutoMappingOverride<Employee>
{
  public void Override(AutoMapping<Employee> mapping)
  {
    mapping.Map(x => x.PersonnelNumber)
      .Unique();
    
    mapping.Map(x => x.TechKASNumber)
      .Unique();
  }
}