﻿using Microsoft.AspNetCore.Mvc;
using SeminarManager.API.Misc;
using SeminarManager.Model;
using System;

namespace SeminarManager.API.Controllers
{
    public class PersonController : Controller
    {
        private IRepository repository;

        public PersonController(IRepository repository)
        {
            this.repository = repository;
        }

        [Route("/Person/")]
        [HttpPost]
        public IActionResult Create([FromBody] Person obj)
        {
            repository.Persons.Save(obj);
            return Json(new OperationResult());
        }

        [Route("/Person/")]
        [HttpGet]
        public IActionResult Read()
        {
            var objects = repository.Persons.All();
            return Json(objects);
        }

        [Route("/Person/{id}")]
        [HttpGet]
        public IActionResult Read(int id)
        {
            var obj = repository.Persons.ById(id);

            if (obj == null)
                return Json(new OperationResult("Object not found!"));

            return Json(obj);
        }

        [Route("/Person/{id}")]
        [HttpPut]
        public IActionResult Update([FromRoute] int id, [FromForm] Person obj)
        {
            if (repository.Persons.ById(id) == null)
                return Json(new OperationResult("Object not found!"));

            obj.ID = id;
            repository.Persons.Save(obj);
            return Json(new OperationResult());
        }

        [Route("/Person/{id}")]
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var user = (Person)HttpContext.Items["user"];
            if (id == user.ID)
                return Json(new OperationResult("Cannot delete current user!"));

            repository.Persons.Delete(id);
            return Json(new OperationResult());
        }
    }
}