﻿using DevSpot.Constants;
using DevSpot.Models;
using DevSpot.Repositories;
using DevSpot.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DevSpot.Controllers
{
    public class JobPostingController : Controller
    {

        private readonly IRepository<JobPosting> _repository;
        private readonly UserManager<IdentityUser> _userManager;

        public JobPostingController(IRepository<JobPosting> repository, 
            UserManager<IdentityUser> userManager)
        {
            _repository = repository; 
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if(User.IsInRole(Roles.Employer))
            {
                var allJobPostings = await _repository.GetAllAsync();
                var userId = _userManager.GetUserId(User);
                var filteredJobPosting = allJobPostings.Where(jp => jp.UserId == userId);
                return View(filteredJobPosting);
            }
            var JobPostings = await _repository.GetAllAsync();

            return View(JobPostings);

        }

        [Authorize(Roles = "Admin, Employer")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(JobPostingViewModel jobPostingVm)
        {


            if (ModelState.IsValid)
            {

                var jobPosting = new JobPosting
                {
                    Title = jobPostingVm.Title,
                    Description = jobPostingVm.Description,
                    Company = jobPostingVm.Company,
                    Location = jobPostingVm.Location,
                    UserId = _userManager.GetUserId(User),
                };
                
                await _repository.AddAsync(jobPosting);
            }

            return RedirectToAction(nameof(Index));
        }


        // JobPosting/Delete/5
        [HttpDelete]
        [Authorize(Roles = "Admin, Employer")]
        public async Task<IActionResult> Delete(int id)
        {

            var jobPosting = await _repository.GetByIdAsync(id);

            if(jobPosting == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            if(User.IsInRole(Roles.Admin) == false && jobPosting.UserId != userId)
            {
                return Forbid();
            }

            await _repository.DeleteAsync(id);

            return Ok();
        }

        /*
        [Authorize(Roles = "Admin, Employer")]
        public async Task<IActionResult> DeleteEasy(int id)
        {

            var jobPosting = await _repository.GetByIdAsync(id);

            if (jobPosting == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            if (User.IsInRole(Roles.Admin) == false && jobPosting.UserId != userId)
            {
                return Forbid();
            }

            await _repository.DeleteAsync(id);

            return RedirectToAction("Index");
        }
        */
    }
}