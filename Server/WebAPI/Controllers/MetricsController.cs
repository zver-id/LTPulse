using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application;
using AutoMapper;
using CommonModels.Models;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTO;

namespace WebAPI.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class MetricsController : ControllerBase
{
  /// <summary>
  /// Маппер.
  /// </summary>
  private readonly IMapper mapper;
  
  /// <summary>
  /// Сервис команд.
  /// </summary>
  private readonly TeamService teamService;
  
  /// <summary>
  /// Сервис метрик.
  /// </summary>
  private readonly MetricsService metricsService;

  [HttpGet]
  public async Task<ActionResult<List<Dictionary<string, object>>>> Get(int teamId, int dayCount)
  {
    var team = await this.teamService.GetTeamById(teamId);
    if (team == null)
      return this.BadRequest("Team not found");
    DateTime beginDate = DateTime.Now - TimeSpan.FromDays(dayCount);
    List<Dictionary<string, object>> response = await this.metricsService.GetMetrics(
      x => (
      (x.Date > beginDate) &&
      (x.Team.Equals(team))));
    return this.Ok(response);
  }

  [HttpGet("filtered")]
  public async Task<ActionResult<List<Dictionary<string, object>>>> GetFiltered(int teamId, int dayCount,
    string filterSing)
  {
    var team = await this.teamService.GetTeamById(teamId);
    if (team == null)
      return this.BadRequest("Team not found");
    DateTime beginDate = DateTime.Now - TimeSpan.FromDays(dayCount);
    List<Dictionary<string, object>> response = await this.metricsService.GetMetrics(
      x => (
        (x.Date > beginDate) &&
        (x.Team.Equals(team)) &&
        (x.MetricType.MetricGroup.Name.ToLower().Equals(filterSing.ToLower()))));
    return this.Ok(response);
  }

  public MetricsController(IMapper mapper)
  {
    this.mapper = mapper;
    //TODO зарегистрироать все нормально
    this.metricsService = new MetricsService();
    this.teamService = new TeamService();
  }
}