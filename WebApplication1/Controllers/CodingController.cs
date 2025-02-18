﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using WebApplication1.Models.Entities;
using WebApplication1.Models.VMs;
using System;
using System.IO;
using WebApplication1.Services;
using WebApplication1.Models.DTOs;

namespace WebApplication1.Controllers
{
    public class CodingController : Controller
    {
        PodmanService _podmanService;

        public CodingController(PodmanService podmanService)
        {
            _podmanService = podmanService;
        }

        public IActionResult Index()
        {
            CodingTask codingTask = new CodingTask()
            {
                Id = 1,
                Name = "Hello, world in python",
                Description = "Write \"Hello, world\" in python",
                Language = "Python"
            };

            string tempFolderName = Guid.NewGuid().ToString();
            // Get the system's temp directory
            string tempFolderPath = Path.Combine(Path.GetTempPath(), tempFolderName);

            Directory.CreateDirectory(tempFolderPath);
            string code = "print('Hello, world!')";
            string filePath = Path.Combine(tempFolderPath, "main.py");

            System.IO.File.WriteAllText(filePath, code);

            CodingIDEVM model = new CodingIDEVM
            {
                Task = codingTask,
                FolderDir = tempFolderPath,
                FilePaths = [filePath]
            };

            return View("IDE", model);
        }

        [HttpPost]
        public IActionResult SaveCode([FromBody] SaveCodeRequest request)
        {
            try
            {
                if (!string.IsNullOrEmpty(request.FilePath) && !string.IsNullOrEmpty(request.Content))
                {
                    System.IO.File.WriteAllText(request.FilePath, request.Content);
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Invalid file path or content." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RunCode(CodingIDEVM data)
        {
            (string output, string error) result = await _podmanService.RunFolderAsync(data.Task, data.FolderDir);

            data.Output = result.output;
            data.Error = result.error;

            return View("IDE", data);
        }
    }
}
