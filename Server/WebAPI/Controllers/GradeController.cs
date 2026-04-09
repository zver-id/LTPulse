using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CommonModels.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAPI.DTO;

[ApiController]
[Route("api/[controller]")]
public class GradeController : ControllerBase
{
  private readonly ILogger<GradeController> logger;
  private readonly IRepository repository;
  private readonly IMapper mapper;


  
  public GradeController(IRepository repository, ILogger<GradeController> logger, IMapper mapper)
  {
    this.repository = repository;
    this.logger = logger;
    this.mapper = mapper;
  }
}