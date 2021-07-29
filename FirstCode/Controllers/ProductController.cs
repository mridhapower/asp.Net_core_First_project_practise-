using FirstCode_DataAccess;
using FirstCode_Models;
using FirstCode_Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using FirstCode_Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FirstCode_DataAccess.Repository.IRepository;

namespace FirstCode.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class ProductController : Controller
    {

         private readonly IProductRepository _prodRepo;
        private readonly IWebHostEnvironment _webHostEnviroment;
        public ProductController(IProductRepository prodRepo,IWebHostEnvironment webHostEnvironment)
        {
            _prodRepo = prodRepo;
            _webHostEnviroment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            IEnumerable <Product> objList = _prodRepo.GetAll(includeProperties:"Category,ApplicationType");
            //foreach (var obj in objList)
            //{
            //    obj.Category = _db.Category.FirstOrDefault(u => u.Id == obj.CategoryId);
            //    obj.ApplicationType = _db.ApplicationType.FirstOrDefault(u => u.Id == obj.ApplicationTypeId);
            //};
            return View(objList);
        }

        //GET - UPSERT
        public IActionResult Upsert(int? id)
        {
            //IEnumerable<SelectListItem> CategoryDropDown = _db.Category.Select(i => new SelectListItem
            //{
            //    Text = i.Name,
            //    Value = i.Id.ToString()
            //});
            //ViewData["CategoryDropDown"] = CategoryDropDown;
            //Product product = new Product();
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList= _prodRepo.GetAllDropdownList(WC.CategoryName),
                ApplicationTypeSelectList = _prodRepo.GetAllDropdownList(WC.ApplicatioTypeName),
            };
            if (id == null)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _prodRepo.Find(id.GetValueOrDefault());
                if (productVM.Product == null)
                {
                    return NotFound();
                }
                else
                {
                    return View(productVM);
                }
            }
        }
        //POST - UPSERT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnviroment.WebRootPath;

                if (productVM.Product.Id == 0)
                {
                    //creating
                    string upload = webRootPath + WC.ImagePath;
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);

                    using(var fileStream=new FileStream(Path.Combine(upload, fileName+ extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }
                    productVM.Product.Image = fileName + extension;
                    _prodRepo.Add(productVM.Product);
                    
                }
                else
                {
                    //updating
                    var objFromDb = _prodRepo.FirstOrDefault(u => u.Id == productVM.Product.Id, isTracking: false);

                    if (files.Count > 0)
                    {
                        string upload = webRootPath + WC.ImagePath;
                        string fileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);
                        var oldFile = Path.Combine(upload, objFromDb.Image);

                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }
                        productVM.Product.Image = fileName + extension;

                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }
                    }
                    else
                    {
                        productVM.Product.Image = objFromDb.Image;
                    }
                    _prodRepo.Update(productVM.Product);

                }
                _prodRepo.Save();
                return RedirectToAction("Index");
            }
            productVM.CategorySelectList = _prodRepo.GetAllDropdownList(WC.CategoryName);
                productVM.ApplicationTypeSelectList = _prodRepo.GetAllDropdownList(WC.ApplicatioTypeName);

            return View(productVM);
            
        }


        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product product = _prodRepo.FirstOrDefault(u=>u.Id==id,includeProperties:"Category,ApplicationType");
            //product.Category = _db.Category.Find(product.CategoryId);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        //POST - DELETE
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _prodRepo.Find(id.GetValueOrDefault());
            if (obj ==null)
            {
                //_db.Product.Remove(obj);
                //_db.SaveChanges();
                //return RedirectToAction("Index");
                return NotFound();
            }
            string upload = _webHostEnviroment.WebRootPath + WC.ImagePath;
            var oldFile = Path.Combine(upload, obj.Image);

            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }
            _prodRepo.Remove(obj);
            _prodRepo.Save();
            return RedirectToAction("Index");
           

        }
    }
}
