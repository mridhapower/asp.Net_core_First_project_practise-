using FirstCode_DataAccess;
using FirstCode_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using FirstCode_Utility;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirstCode_DataAccess.Repository.IRepository;

namespace FirstCode.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class CategoryController : Controller
    {

        private readonly ICategoryRepository _catRepo;
        public CategoryController(ICategoryRepository catRepo)
        {
            _catRepo = catRepo;
        }
        public IActionResult Index()
        {
            IEnumerable<Category> objList = _catRepo.GetAll();
            return View(objList);
        }

        //GET - CREATE
        public IActionResult Create()
        {
            
            return View();
        }
        //POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                _catRepo.Add(obj);
                _catRepo.Save();
                return RedirectToAction("Index");
            }
            return View(obj);
            
        }
        //GET - EDIT
        public IActionResult Edit(int? id)
        {
            if(id==null || id == 0)
            {
                return NotFound();
            }
            var obj = _catRepo.Find(id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }
            return View(obj);
        }
        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _catRepo.Update(obj);
                _catRepo.Save();
                return RedirectToAction("Index");
            }
            return View(obj);

        }

        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _catRepo.Find(id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }
            return View(obj);
        }
        //POST - DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _catRepo.Find(id.GetValueOrDefault());
            if (obj !=null)
            {
                _catRepo.Remove(obj);
                _catRepo.Save();
                return RedirectToAction("Index");
            }
            else
            {
                return NotFound();
            }

        }
    }
}
