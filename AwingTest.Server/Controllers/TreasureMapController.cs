using AwingTest.Server.Entities;
using AwingTest.Server.Handler;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AwingTest.Server.Controllers
{
    [ApiController]
    [Route("TreasureMap")]
    public class TreasureMapController : ControllerBase
    {
        private readonly ILogger<TreasureMapHandler> _logger;
        private readonly AwingTestDbContext _dbContext;
        private readonly TreasureMapHandler _handler;

        public TreasureMapController(
            ILogger<TreasureMapHandler> logger,
            AwingTestDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
            _handler = new TreasureMapHandler(_logger, _dbContext);
        }

        [HttpPost("Add")]
        public async Task<TreasureMapRequest> Add(TreasureMapRequest request)
        {
            TreasureMapRequest response = new TreasureMapRequest();
            var result = await _handler.Add(request.map);
            response.id = result.id;
            response.ListError = result.ListError;
            return response;
        }

        [HttpPost("SetupDisplay")]
        public async Task<TreasureMapRequest> SetupDisplay(TreasureMapRequest request)
        {
            TreasureMapRequest response = new TreasureMapRequest();
            var result = await _handler.SetupDisplay(request.id.Value);
            response.ListError = result.ListError;
            response.map = result.map;
            return response;
        }

        [HttpPost("SetupEdit")]
        public async Task<TreasureMapRequest> SetupEdit(TreasureMapRequest request)
        {
            TreasureMapRequest response = new TreasureMapRequest();
            var result = await _handler.SetupEdit(request.id.Value);
            response.ListError = result.ListError;
            response.map = result.map;
            return response;
        }

        [HttpPost("Edit")]
        public async Task<TreasureMapRequest> Edit(TreasureMapRequest request)
        {
            TreasureMapRequest response = new TreasureMapRequest();
            var result = await _handler.Edit(request.map);
            response.ListError = result.ListError;
            return response;
        }


        [HttpPost("Test")]
        public async Task<TreasureMapRequest> Test(TreasureMapRequest request)
        {
            TreasureMapRequest response = new TreasureMapRequest();
            response.TestResult = await _handler.Test();
            return response;
        }

        public class TreasureMapRequest
        {
            public int? id { get; set; }
            public treasure_map? map { get; set; }
            public List<string>? ListError { get; set; }
            public Dictionary<string, bool>? TestResult { get; set; }
        }
    }
}