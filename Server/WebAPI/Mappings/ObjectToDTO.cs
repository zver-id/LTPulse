using System.Runtime.CompilerServices;
using AutoMapper;
using Common.Models;
using WebAPI.DTO;

namespace WebAPI.Mappings;

/// <summary>
/// Конфигурация маппинга.
/// </summary>
public class ObjectToDTO : Profile
{
  public ObjectToDTO()
  {
    this.CreateMap<Team, TeamDTO>().ReverseMap();
    this.CreateMap<Metric, MetricDTO>().ReverseMap();
  }
}