# MyWebProgram
**購物車系統**

**作品功能有:**

1.會員註冊、登入、登出

2.非會員瀏覽商品

3.會員瀏覽商品及加入購物車、查看購物車及刪除購物車商品

4.確認訂單、訂單付款  (使用LINEPAY第三方支付API)、查看訂單及訂單明細

5.管理者產品上下架及內容修改
(後端使用WEB  API設計搭配前端AJAX做資料操作)

6.管理會員資料修改、停權


**主要的CODE區:**

Controllers/ProductController.cs 這隻檔案是WEBAPI專門做產品上下架

Controllers/HomeController.cs 這裡有其他功能及LINEPAY API付款功能

Views/Shared/ 這裡是三個LAYOUT版面供非會員、會員、管理者使用

Views/Home/ 這裡是搭配LAYOUT的每個功能前端頁面
