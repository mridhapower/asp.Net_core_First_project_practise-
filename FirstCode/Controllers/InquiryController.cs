
using FirstCode_DataAccess.Repository.IRepository;
using FirstCode_Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FirstCode.Controllers
{
    public class InquiryController : Controller
    {
        private readonly IInquiryHeaderRepository _inqHRepo;
        private readonly IInquiryDetailRepository _inqDRepo;
        [BindProperty]
        public InquiryVM InquiryVM { get; set; }

        public InquiryController(IInquiryHeaderRepository inqHRepo, IInquiryDetailRepository inDRepo)
        {
            _inqDRepo = inDRepo;
            _inqHRepo = inqHRepo;
        }
        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Details(int id)
        {
            InquiryVM = new InquiryVM()
            {
                InquiryHeader = _inqHRepo.FirstOrDefault(u => u.Id == id),
                InquiryDetail = _inqDRepo.GetAll(u => u.InquiryHeaderId == id, includeProperties: "Product")
            };
            return View(InquiryVM);
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetInquiryList()
        {
            return Json(new { data = _inqHRepo.GetAll() });
        }
        #endregion 

    }
}
