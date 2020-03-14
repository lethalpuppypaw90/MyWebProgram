using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using prjShoppingCar.LinePay_Class;
using prjShoppingCar.Models;

namespace prjShoppingCar.Controllers
{
    public class HomeController : Controller
    {
        //建立可存取dbShoppingCar.mdf 資料庫的dbShoppingCarEntities 類別物件db
        dbShoppingCarEntities db = new dbShoppingCarEntities();

        // GET:首頁
        public ActionResult Index()
        {
            //取得所有產品放入 products
            var products = db.tProduct.ToList();
            //找到包含管理員帳號的資料表
            string[] admiIdSet = new string[] { "admi01", "admi02", "admi03" };
            var amember = db.tMember
               .Where(m => admiIdSet.Contains(m.fUserId))
               .FirstOrDefault();
            //若Session["Member"]為空,表示會員未登入
            if (Session["Member"]==null)
            {
                //指定Index.cshtml 套用_Layout.cshtml,View使用products
                return View("Index", "_Layout", products);
            }
            else if ((Session["Member"] as tMember).fUserId==amember.fUserId)
            {
                //管理者商品頁面
                return View("IndexAdministrator", "_LayoutAdministrator", products);
            }

            //會員登入狀態
            //指定Index.cshtml 套用_LayoutMember.cshtml,View 使用products
            return View("Index", "_LayoutMember", products);
        }
        // GET:管理者首頁
        public ActionResult ADMemberPageIndex()
        {

            if (Session["Member"] == null)
            {

                return RedirectToAction("Login");
            }
            Regex pattern = new Regex("^((?!admi).)*$", RegexOptions.IgnoreCase);
            var Members = db.tMember.ToList();
            var memberADPage = Members.Where(m => pattern.IsMatch(m.fUserId));
            return View("ADMemberPageIndex", "_LayoutAdministrator", memberADPage);
        }
        // GET:修改會員頁面
        public ActionResult ADMemberPage(int id)
        {
            var editlist = db.tMember.Where(m => m.fId == id).FirstOrDefault();
            if (Session["Member"] == null)
            {
                
                return RedirectToAction("Login");
            }
            return View("ADMemberPage", "_LayoutAdministrator", editlist);
        }
        // POST:修改會員功能
        [HttpPost]
        public ActionResult ADMemberPage(int fid,string fuserid,string fpwd,string fname,string femail,string fbanned)
        {       
            var adMember = db.tMember.Where(m => m.fId == fid).FirstOrDefault();
            adMember.fUserId = fuserid;
            adMember.fPwd = fpwd;
            adMember.fName = fname;
            adMember.fEmail = femail;
            adMember.fBanned = fbanned;
            db.SaveChanges();
            return RedirectToAction("ADMemberPageIndex");
        }
     
        //GET:登入首頁
        public ActionResult Login()
        {
        return View();
        }
        //POST:登入功能
        [HttpPost]
        public ActionResult Login(string fUserId,string fPwd)
        {
            //依帳密取得會員並指定給member
            var member = db.tMember
                .Where(m => m.fUserId == fUserId && m.fPwd == fPwd)
                .FirstOrDefault();
            //若member為null,表示會員未註冊
            if (member == null)
            {
                ViewBag.Message = "帳密有誤,登入失敗";
                return View();
            }
            else if (member.fBanned == "0")
            {
                ViewBag.Message = "帳密已被停權請聯絡客服";
                return View();
            }
            //使用Session變數紀錄迎詞
            Session["WelCome"] = member.fName + "歡迎光臨";
            //使用Session變數紀錄登入的會員物件
            Session["Member"] = member;
            //執行Home控制器的Index動作方法
            return RedirectToAction("Index");
        }

        //GET:註冊首頁
        public ActionResult Register()
        {
            return View();
        }
        //POST:註冊功能
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(tMember pMember)
        {
            //若模型沒有通過驗證則顯示目前的view
            if (ModelState.IsValid == false)
            {
                return View();
            }
            //依帳號取得會員並指定給member
            var member = db.tMember
                .Where(m => m.fUserId == pMember.fUserId)
                .FirstOrDefault();
            //若member 為 null ,表示會員未註冊
            if (member == null)
            {
                //將會員紀錄新增到tMember資料表
                db.tMember.Add(pMember);
                db.SaveChanges();
                //執行Home 控制器Login動作方法

                //導向Login 跳出註冊成功警示視窗
                TempData["message"] = "註冊成功";
                return RedirectToAction("Login");
            }
            ViewBag.Message = "此帳號已經有人使用,註冊失敗";
            return View();
        }

        //GET:登出
        public ActionResult Logout()
        {
            Session.Clear();    //清除Session變數
            return RedirectToAction("Index"); //執行Index 方法顯示產品列表
        }

        //GET:購物車首頁
        public ActionResult ShoppingCar()
        {
            //取得登入會員的帳號並指定給fUserId
            string fUserId = (Session["Member"] as tMember).fUserId;
            //找出未成為訂單明細的資料,即購物車內容
            var orderDetails = db.tOrderDetail
                .Where(m => m.fUserId == fUserId && m.fIsApproved == "否")
                .ToList();
            //指定 ShoppingCar.cshtml套用_LayoutMember.cshtml,view使用tOrderDetails模型
            return View("ShoppingCar", "_LayoutMember", orderDetails);
        }
               
        [HttpPost]
        public ActionResult ShoppingCar(string fReceiver, string fEmail, string fAddress)
        {

            string fUserId = (Session["Member"] as tMember).fUserId;
            var sumdb = db.tOrderDetail
                          .Where(o => o.fIsApproved == "否" && o.fUserId == fUserId)
                          .Sum(o => o.fQty * o.fPrice);
            LienPayRequest Payrequest = new LienPayRequest
            {
                amount = sumdb.Value,
                productImageUrl = "https://i.imgur.com/NLZWzqn.jpg",
                confirmUrl = "http://localhost:61971/Home/Confirm",
                productName = "購物車訂單總價錢",
                orderId = Guid.NewGuid().ToString(),
                currency = "TWD"
            };

            ShoppingCarDetail shopdetail = new ShoppingCarDetail
            {
                fReceiver = fReceiver,
                fEmail = fEmail,
                fAddress = fAddress,
                orderId = Payrequest.orderId
            };
            TempData["TheCustomData"] = shopdetail;
            using (var client = new HttpClient())
            {

                var requestjson = JsonConvert.SerializeObject(Payrequest);
                HttpContent httpContent = new StringContent(requestjson);
                httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                httpContent.Headers.Add("X-LINE-ChannelId", "1653922835");
                httpContent.Headers.Add("X-LINE-ChannelSecret", "7fba445413cb5ddd1494db8f4fe4fca9");

                var result = client.PostAsync("https://sandbox-api-pay.line.me/v2/payments/request/", httpContent)
                    .Result
                    .Content
                    .ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(result))
                {
                    var item = JsonConvert.DeserializeObject<RootObject>(result);

                    if (item.returnCode == "0000")
                    {
                        var RedirectpaymentUrl = item.info.paymentUrl.web;
                        return Redirect(RedirectpaymentUrl);
                    }
                }
                else
                {

                }
                return View();
            }
        }
        //接收RESPONSE
        //啟動核對是否付款成功
        //付款成功導向Index的View
        public ActionResult Confirm(string transactionId)
        {
            string fUserId = (Session["Member"] as tMember).fUserId;
            var sumdb = db.tOrderDetail
                          .Where(o => o.fIsApproved == "否" && o.fUserId == fUserId)
                          .Sum(o => o.fQty * o.fPrice);
            LienPayRequest confirmquest = new LienPayRequest
            {
                amount = sumdb.Value,
                currency = "TWD"
            };
            using (var client = new HttpClient())
            {

                var requestjson = JsonConvert.SerializeObject(confirmquest);
                HttpContent httpContent = new StringContent(requestjson);
                httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                httpContent.Headers.Add("X-LINE-ChannelId", "1653922835");
                httpContent.Headers.Add("X-LINE-ChannelSecret", "7fba445413cb5ddd1494db8f4fe4fca9");
                var url = string.Format("https://sandbox-api-pay.line.me/v2/payments/{0}/confirm", transactionId);

                var result = client.PostAsync(url, httpContent)
                    .Result
                    .Content
                    .ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(result))
                {
                    var item = JsonConvert.DeserializeObject<ConfirmObject>(result);

                    if (item.returnCode == "0000")
                    {
                        return RedirectToAction("ShoppingCarOrder", "Home");
                    }
                }
                else
                {
                    TempData["failConfirm"] = "付款核對失敗";
                    return RedirectToAction("ShoppingCar", "Home");
                }
                return View();
            }
        }
        //產生訂單
        public ActionResult ShoppingCarOrder()
        {
            ShoppingCarDetail cusdata = TempData["TheCustomData"] as ShoppingCarDetail;
            //找出會員帳號並指定給fUserId
            string fUserId = (Session["Member"] as tMember).fUserId;            
            //tOrder的fOrderGuid欄位會關連到tOrderDetail的fOrderGuid欄位
            //形成一對多的關係,即一筆訂單資料會對應到多筆訂單明細
            //建立訂單主檔資料
            tOrder order = new tOrder();
            order.fOrderGuid = cusdata.orderId;
            order.fUserId = fUserId;
            order.fReceiver = cusdata.fReceiver;
            order.fEmail = cusdata.fEmail;
            order.fAddress = cusdata.fAddress;
            order.fDate = DateTime.Now;
            db.tOrder.Add(order);
            //找出目前會員在訂單明細中是購物車狀態的產品
            var carList = db.tOrderDetail
                .Where(m => m.fIsApproved == "否" && m.fUserId == fUserId)
                .ToList();
            //將購物車狀態產品的fIsApproved設為"是",表示確認訂購產品
            foreach (var item in carList)
            {
                item.fOrderGuid = cusdata.orderId;
                item.fIsApproved = "是";
            }
            //更新資料庫,異動tOrder和tOrderDetail
            //完成訂單主檔和訂單明細更新
            db.SaveChanges();
            //執行Home控制器的OrderList動作方法
            return RedirectToAction("OrderList");
        }

        //GET:加入購物車
        public ActionResult AddCar(string fPId)
        {
            //取得會員帳號並指定給fUserId
            string fUserId = (Session["Member"] as tMember).fUserId;
            //找出會員放入訂單明細的產品,該產品的fIsApproved=="否"
            //表示該產品購物車狀態
            var currentCar = db.tOrderDetail
                .Where(m => m.fPId == fPId && m.fIsApproved == "否" && m.fUserId == fUserId)
                .FirstOrDefault();
            //若currentCar 等於null,表示會員選購的產品不是購物車狀態
            if (currentCar == null)
            {
                //找出目前選購的產品並指定給product
                var product = db.tProduct
                    .Where(m => m.fPId == fPId).FirstOrDefault();
                //將產品放入訂單明細,因為產品的fIsApproved為"否",表示為購物車狀態
                tOrderDetail orderDetail = new tOrderDetail();
                orderDetail.fUserId = fUserId;
                orderDetail.fPId = product.fPId;
                orderDetail.fName = product.fName;
                orderDetail.fPrice = product.fPrice;
                orderDetail.fQty = 1;
                orderDetail.fIsApproved = "否";
                db.tOrderDetail.Add(orderDetail);
            }
            else
            {
                //若產品為購物車狀態,即將該產品數量+1
                currentCar.fQty += 1;
            }
            db.SaveChanges();
            //執行Home控制器的ShoppingCar動作方法
            return RedirectToAction("ShoppingCar");
        }

        //GET:刪除購物車
        public ActionResult DeleteCar(int fId)
        {
            //依fId找出要刪除購物車狀態的產品
            var orderDetail = db.tOrderDetail.Where
                (m => m.fId == fId).FirstOrDefault();
            //刪除購物車狀態的產品
            db.tOrderDetail.Remove(orderDetail);
            db.SaveChanges();
            //執行Home 控制器的 ShoppingCar動作方法
            return RedirectToAction("ShoppingCar");
        }

        //GET:顯示訂單
        public ActionResult OrderList()
        {
            //找出會員帳號並指定給fUserId
            string fUserId= (Session["Member"] as tMember).fUserId;
            //找出目前會員的所有訂單主檔紀錄並依照fDate進行遞增排序
            //將查找結果指定給orders
            var orders = db.tOrder.Where(m => m.fUserId == fUserId)
                .OrderByDescending(m => m.fDate).ToList();
            //目前會員的訂單主檔
            //指定OrderList.cshtml套用_LayoutMember.cshtml,View使用orders模型
            return View("OrderList", "_LayoutMember", orders);
        }

        //GET:訂單明細
        public ActionResult OrderDetail(string fOrderGuid)
        {
            //根據fOrderGuid找出和訂單主檔關聯的訂單明細,並指定給orderDetails
            var orderDetails = db.tOrderDetail
                .Where(m => m.fOrderGuid == fOrderGuid).ToList();
            //目前訂單明細
            //指定OrderDetail.cshtml套用_LayoutMember.cshtml,view使用orderDetails模型
            return View("OrderDetail", "_LayoutMember", orderDetails);
        }
    }
}