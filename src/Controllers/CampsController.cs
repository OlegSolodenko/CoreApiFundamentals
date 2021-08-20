using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public CampsController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false)
        {
            try
            {
                var result = await _repository.GetAllCampsAsync(includeTalks);

                CampModel[] models = _mapper.Map<CampModel[]>(result);

                return models;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var res = await _repository.GetCampAsync(moniker);
                if (res == null)
                    return NotFound();
                return _mapper.Map<CampModel>(res);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> Search(DateTime date, bool includeTalks = false)
        {
            try
            {
                var result = await _repository.GetAllCampsByEventDate(date, includeTalks);

                if (!result.Any())
                    return NotFound();

                return _mapper.Map<CampModel[]>(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }


        [HttpPost]
        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                var existsCamp = await _repository.GetCampAsync(model.Moniker);
                if (existsCamp != null)
                {
                    return BadRequest("Moniker in use!");
                }

                _linkGenerator.GetPathByAction("Get", "Camp", new { moniker = model.Moniker });

                var camp = _mapper.Map<Camp>(model);

                _repository.Add(camp);
                if (await _repository.SaveChangesAsync())
                {
                    return Created(_linkGenerator.ToString(), _mapper.Map<CampModel>(camp));
                }

                return BadRequest("Something went wrong!");

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put(string moniker, CampModel model)
        {
            try
            {
                var oldCamp = await _repository.GetCampAsync(moniker);
                if (oldCamp == null)
                    return NotFound($"Could not find camp with moniker of {moniker}");

                _mapper.Map(model, oldCamp);

                if (await _repository.SaveChangesAsync())
                {
                    return _mapper.Map<CampModel>(oldCamp);
                }

                return BadRequest("Something went wrong!");

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }
        
        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker, CampModel model)
        {
            try
            {
                var oldCamp = await _repository.GetCampAsync(moniker);
                if (oldCamp == null)
                    return NotFound($"Could not find camp with moniker of {moniker}");

                _repository.Delete(oldCamp);

                if (await _repository.SaveChangesAsync())
                    return Ok();

                return BadRequest("Something went wrong!");

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }
    }
}
