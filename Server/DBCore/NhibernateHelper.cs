using CommonModels;
using CommonModels.Models;
using DBCore;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Automapping;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace NHibernate.Infrastructure;

public static class NhibernateHelper
{
  private static ISessionFactory _sessionFactory;

  public static ISessionFactory SessionFactory
  {
    get
    {
      if (_sessionFactory == null)
      {
        _sessionFactory = CreateSessionFactory();
      }
      return _sessionFactory;
    }
  }
  
  private static void Expose(Configuration configuration)
  {
    new SchemaUpdate(configuration).Execute(true, true);
  }
  
  private static AutoPersistenceModel GetAutoPersistenceModel() =>
    AutoMap.AssemblyOf<Ticket>(new StoreConfiguration())
      //.Conventions.AddFromAssemblyOf<IdConvention>()
      //.Conventions.AddFromAssemblyOf<NHibernateInitializer>()
      .UseOverridesFromAssemblyOf<DBRepository>();

  private static ISessionFactory CreateSessionFactory()
  {
    var cfg = new StoreConfiguration();
    string connectionString = Settings.DatabaseConnectionString;
    return Fluently.Configure()
      .Database(PostgreSQLConfiguration.Standard
        .ConnectionString(connectionString)
        .ShowSql())
      .Mappings(x => x.AutoMappings.Add(GetAutoPersistenceModel()))
      //.Mappings(m => m.AutoMappings
      //  .Add(AutoMap.AssemblyOf<CollectionItemType>(cfg)))
      .ExposeConfiguration(Expose)
      .BuildSessionFactory();
  }

  public static ISession OpenSession()
  {
    return SessionFactory.OpenSession();
  }
}