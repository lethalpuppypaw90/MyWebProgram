using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using prjShoppingCar.Models;

namespace prjShoppingCar.Controllers
{
    public class ProductController : ApiController
    {
        dbShoppingCarEntities db = new dbShoppingCarEntities();
        // GET: api/Product
        public List<tProduct> Get()
        {
            var products = db.tProduct;
            return products.ToList();
        }

        // GET: api/Product/5
        public tProduct Get(int fid)
        {
            var product = db.tProduct
                .Where(m => m.fId == fid).FirstOrDefault();
            return product;
        }
        
        // POST: api/Product
        public int Post(string fPid,string fName,int fPrice,string fImg)
        {

            
            int num = 0;
            try
            {
                //request.file中是否有任何鍵值
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    //選擇檔案
                    var imgfile = HttpContext.Current.Request.Files["Uploadimgfiles"];
                    if (imgfile != null)
                    {
                        //檔案名稱結合路徑然後儲存
                        var savefiles = Path.Combine(HttpContext.Current.Server.MapPath("~/images/"), imgfile.FileName);
                        imgfile.SaveAs(savefiles);
                    }

                    tProduct product = new tProduct();
                    product.fPId = fPid;
                    product.fName = fName;
                    product.fPrice = fPrice;
                    product.fImg = fImg;
                    db.tProduct.Add(product);

                    num = db.SaveChanges();
                }
                else
                {
                    num = 0;
                }
            }
            catch(Exception ex)
            {
                num = 0;
            }
            return num;
        }

        // PUT: api/Product/5
        public int Put(int fid,string fPid, string fName, int fPrice, string fImg,string oldfImg)
        {
            int num = 1;
            
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var imgfile = HttpContext.Current.Request.Files["Uploadimgfiles"];
                    if (imgfile != null)
                    {
                        var savefiles = Path.Combine(HttpContext.Current.Server.MapPath("~/images/"), imgfile.FileName);
                        imgfile.SaveAs(savefiles);
                        var deletefiles = Path.Combine(HttpContext.Current.Server.MapPath("~/images/"), oldfImg);
                        //刪除實體路徑檔案
                        File.Delete(deletefiles);
                    }
                }
                var product = db.tProduct.Where(m => m.fId == fid).FirstOrDefault();
                product.fPId = fPid;
                product.fName = fName;
                product.fPrice = fPrice;
                product.fImg = fImg;
                num = db.SaveChanges();
            }
            catch (Exception ex)
            {
                num = 0;
            }
            return num;
        }

        // DELETE: api/Product/5
        public int Delete(int fid)
        {
            int num = 0;
            try
            {
                var product = db.tProduct.Where(m => m.fId == fid).FirstOrDefault();
                string fileName = product.fImg;
                var deletefiles = Path.Combine(HttpContext.Current.Server.MapPath("~/images/"), fileName);
                //刪除實體路徑檔案
                if (fileName != "")
                {
                    File.Delete(deletefiles);
                }
                db.tProduct.Remove(product);
                num = db.SaveChanges();
            }
            catch (Exception ex)
            {
                num = 0;
            }
            return num;
        }

    }
}
