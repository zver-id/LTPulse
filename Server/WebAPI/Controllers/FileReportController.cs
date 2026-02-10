using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application;
using Application.ReportGeneration;
using AutoMapper;
using CommonModels.Interfaces;
using CommonModels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebAPI.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class FileReportController : ControllerBase
{
  private readonly IMapper mapper;
  public IRepository Repository { get; }

  [HttpGet]
  public async Task<ActionResult> GenerateReport(int teamId)
  {
    var reportWriter = new ReportGenerator(this.Repository);
    var team = this.Repository.Get<Team>(t => t.Id == teamId).First();
    try
    {
      var reportBytes = reportWriter.GenerateForTeam(team);
      //System.IO.File.WriteAllBytes(@"C:\Temp\Report.csv", reportBytes);
      return this.File(reportBytes, 
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
        "report.xlsx");
    }
    catch (Exception e)
    {
      return this.Problem();
    }
  }

  public FileReportController(IRepository repository, IMapper mapper)
  {
    this.mapper = mapper;
    this.Repository = repository;
  }
  
}