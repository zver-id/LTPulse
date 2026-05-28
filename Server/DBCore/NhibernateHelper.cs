using CommonModels.Models;
using DBCore;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Automapping;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace NHibernate.Infrastructure;

public class NhibernateHelper
{
  internal ISessionFactory SessionFactory { get; }
  
  private void Expose(Configuration configuration)
  {
    new SchemaUpdate(configuration).Execute(true, true);
  }
  
  private AutoPersistenceModel GetAutoPersistenceModel() =>
    AutoMap.AssemblyOf<Ticket>(new StoreConfiguration())
      //.Conventions.AddFromAssemblyOf<IdConvention>()
      //.Conventions.AddFromAssemblyOf<NHibernateInitializer>()
      .UseOverridesFromAssemblyOf<DbRepository>();

  private ISessionFactory CreateSessionFactory(string connectionString)
  {
    var cfg = new StoreConfiguration();
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

  public ISession OpenSession()
  {
    return this.SessionFactory.OpenSession();
  }

  public NhibernateHelper(string connectionString)
  {
    this.SessionFactory = this.CreateSessionFactory(connectionString);
  }
}