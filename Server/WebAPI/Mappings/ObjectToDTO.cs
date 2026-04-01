using System.Runtime.CompilerServices;
using AutoMapper;
using CommonModels.Models;
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
    this.CreateMap<Ticket, TicketDTO>()
      .ForMember(dest => dest.Key,
        opt => opt.MapFrom(src => src.Id))
      .ForMember(dest => dest.Priority, 
        opt => opt.MapFrom(src => src.Priority.Name))
      .ForMember(dest => dest.State, 
        opt => opt.MapFrom(src => src.State.State));
  }
}