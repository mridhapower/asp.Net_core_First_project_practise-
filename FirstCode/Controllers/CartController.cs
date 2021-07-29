using FirstCode_DataAccess;
using FirstCode_Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirstCode_Utility;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FirstCode_Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using FirstCode_DataAccess.Repository.IRepository;

namespace FirstCode.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IProductRepository _prodRepo;
        private readonly IApplicationUserRepository _userRepo;
        private readonly IInquiryDetailRepository _inqDRepo;
        private readonly IInquiryHeaderRepository _inqHRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; } 

        public CartController(IWebHostEnvironment webHostEnvironment,IEmailSender emailSender
            , IInquiryHeaderRepository inqHRepo, IInquiryDetailRepository inqDRepo,
            IApplicationUserRepository userRepo, IProductRepository prodRepo)
        {
            _inqDRepo = inqDRepo;
            _inqHRepo = inqHRepo;
            _prodRepo = prodRepo;
            _userRepo = userRepo;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count()>0)
            {
                //session exists
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);

            }
            List<int> prodInCart = shoppingCartList.Select(u => u.ProductId).ToList();
            IEnumerable<Product> prodList = _prodRepo.GetAll(u => prodInCart.Contains(u.Id));
            return View(prodList);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPsot()
        {
            
            return RedirectToAction(nameof(Summary));
        }

        
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                return null;
            }

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                //session exists
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);

            }
            List<int> prodInCart = shoppingCartList.Select(u => u.ProductId).ToList();
            IEnumerable<Product> prodList = _prodRepo.GetAll(u => prodInCart.Contains(u.Id));

            ProductUserVM = new ProductUserVM()
            {
                ApplicationUser = _userRepo.FirstOrDefault(u => u.Id == claim.Value),
                ProductList = prodList.ToList()
            };

            return View(ProductUserVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryPost(ProductUserVM ProductUserVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            Claim claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                return null;
            }

            var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                + "templates" + Path.DirectorySeparatorChar.ToString() + "Inquiry.html";

            var subject = "New Inquiry";
            string HtmlBody = "";

            using (StreamReader sr = System.IO.File.OpenText(PathToTemplate))
            {
                HtmlBody = sr.ReadToEnd();
            }

            var productListSB = new StringBuilder();
            foreach(var prod in ProductUserVM.ProductList)
            {
                productListSB.Append($"-Name: {prod.Name}<span style='font-size:14px;'> (ID: {prod.Id})</span?<br/>");
            }

            string messageBody = string.Format(HtmlBody,
                ProductUserVM.ApplicationUser.FullName,
                ProductUserVM.ApplicationUser.Email,
                ProductUserVM.ApplicationUser.PhoneNumber,
                productListSB.ToString()
                );

            await _emailSender.SendEmailAsync(WC.EmailAdmin, subject, messageBody);
            InquiryHeader inquiryHeader = new InquiryHeader()
            {
                ApplicationUserId = claim.Value,
                FullName = ProductUserVM.ApplicationUser.FullName,
                Email = ProductUserVM.ApplicationUser.Email,
                PhoneNumber=ProductUserVM.ApplicationUser.PhoneNumber,
                InquiryDate = DateTime.Now
            };
            _inqHRepo.Add(inquiryHeader);
            _inqHRepo.Save();

            foreach(var prod in ProductUserVM.ProductList)
            {
                InquiryDetail inquiryDetail = new InquiryDetail()
                {
                    InquiryHeaderId=inquiryHeader.Id,
                    ProductId=prod.Id
                };
                _inqDRepo.Add(inquiryDetail);
                
            }
            _inqDRepo.Save();




            return RedirectToAction(nameof(InquiryConfirmation));
        }

        public IActionResult InquiryConfirmation()
        {

            HttpContext.Session.Clear();
            return View();
        }

        public IActionResult Remove(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                //session exists
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);

            }
            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(u => u.ProductId == id));
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);


            return RedirectToAction(nameof(Index));
        }
    }
}
