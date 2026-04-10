using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Application;
using AutoMapper;
using CommonModels.Interfaces;
using CommonModels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAPI.Controllers;
using WebAPI.DTO;

[ApiController]
[Route("api/[controller]")]
public class GradeController : ControllerBase
{
  private readonly ILogger<GradeController> logger;
  private readonly IRepository repository;
  private readonly IMapper mapper;

  [HttpGet]
  public async Task<ActionResult<Response>> GetGrades(int teamId, DateTime date, bool? onlyUnresearched, bool? onlyNegative)
  {
    var gradeService = new GradeService(this.repository);
    var team = this.repository.GetById<Team>(teamId);
    List<Grade> grades;
    if (onlyNegative == true)
    {
      grades = gradeService.GetNegativeGrades(team, onlyUnresearched == true);
    }
    else
    {
      grades = gradeService.GetAllGrades(team, date - TimeSpan.FromDays(1), date);
    }
    return this.BadRequest(new Response
    {
      Data = this.mapper.Map<List<GradeDTO>>(grades),
      Schema = typeof(Grade).GetProperties()
        .ToDictionary(p => p.Name,
          p => p.GetCustomAttribute<DisplayAttribute>()?.Name ?? p.Name)
    });
  }
  
  public GradeController(IRepository repository, ILogger<GradeController> logger, IMapper mapper)
  {
    this.repository = repository;
    this.logger = logger;
    this.mapper = mapper;
  }
}