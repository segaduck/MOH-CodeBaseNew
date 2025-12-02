using System.Web;
using System.Web.Optimization;

namespace EECOnline
{
    public class BundleConfig
    {
        // 如需「搭配」的詳細資訊，請瀏覽 http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // 將 EnableOptimizations 設為 false 以進行偵錯。如需詳細資訊，
            // 請造訪 http://go.microsoft.com/fwlink/?LinkId=301862
            // 使用開發版本的 Modernizr 進行開發並學習。然後，當您
            // 準備好實際執行時，請使用 http://modernizr.com 上的建置工具，只選擇您需要的測試。


            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            //bundles.Add(new ScriptBundle("~/bundles/select2").Include(
            //         "~/Scripts/select2.min.js"));

            //bundles.Add(new StyleBundle("~/Content/select2").Include(
            //       "~/Content/select2.min.css"));

            #region 前台

            bundles.Add(new StyleBundle("~/Content/owlcarousel").Include(
                   // owl carousel
                   "~/vendor/owl.carousel/dist/assets/owl.carousel.min.css",
                   "~/vendor/owl.carousel/dist/assets/owl.theme.default.min.css"));

            bundles.Add(new StyleBundle("~/Content/FontBootstrap").Include(
                   // bootstrap
                   "~/Content/front/bootstrap.css",
                   // font-awesome
                   "~/Content/fontawesome-5.6.3.css",
                   // jquery blockUI
                   "~/Content/jquery-confirm.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/FrontBootstrapScript").Include(
                   // jquery
                   "~/vendor/jquery/dist/jquery.js",
                   // bootstrap
                   "~/Scripts/front/bootstrap.js",
                   // popper
                   "~/Scripts/popper.js",
                   // jquery blockUI
                   "~/Scripts/jquery-confirm.min.js",
                   "~/Scripts/jquery.blockUI.js"));

            bundles.Add(new StyleBundle("~/Content/FontBootstrapTable").Include(
                   // bootstrap-table
                   "~/Content/bootstrap-table.css"));

            bundles.Add(new ScriptBundle("~/bundles/FrontOwlScript").Include(
                   // bootstrap-table
                   "~/Scripts/bootstrap-table.js",
                   // owl carousel
                   "~/vendor/owl.carousel/dist/owl.carousel.js",
                   // GoTop
                   "~/Scripts/front/gotop.js",
                   // main
                   "~/Scripts/front/main.js"));

            bundles.Add(new ScriptBundle("~/bundles/APISample").Include(
                 "~/Scripts/APISample/AllComponentErrCode.js",
                 "~/Scripts/APISample/CheckAndLoad.js",
                 "~/Scripts/APISample/HCAAPISVIAdapter.js",
                 "~/Scripts/APISample/env.js"));

            bundles.Add(new StyleBundle("~/Content/base")
                .Include("~/Content/front/base.css", new CssRewriteUrlTransform()));

            bundles.Add(new StyleBundle("~/Content/main")
              .Include("~/Content/front/main.css", new CssRewriteUrlTransform()));

            #endregion

            #region 後台

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                      "~/Scripts/jquery-{version}.js",
                      "~/Scripts/jquery-confirm.min.js",
                      "~/Scripts/jquery.blockUI.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/bootstrap-treeview.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/globaljs").Include(
                      "~/Scripts/global.js",
                      "~/Scripts/print.js"));

            bundles.Add(new StyleBundle("~/Content/jquery").Include(
                    "~/Content/jquery-confirm.min.css"));

            bundles.Add(new StyleBundle("~/Content/bootstrap").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-treeview.css",
                      "~/Content/font-awesome-4.7.0.min.css",
                      "~/Content/fontawesome-5.6.3.css"));

            //toastr
            bundles.Add(new ScriptBundle("~/Script/toastr").Include(
           "~/Scripts/toastr.min.js"));

            //toastr
            bundles.Add(new ScriptBundle("~/Script/knockout").Include(
           "~/Scripts/knockout-3.4.2.js",
           "~/Scripts/knockout.mapping.min.js"));

            //toastr
            bundles.Add(new StyleBundle("~/CSS/toastr").Include(
                 "~/Content/toastr.min.css")
             );

            bundles.Add(new StyleBundle("~/Content/base_back")
                .Include("~/Content/base_back.css", new CssRewriteUrlTransform())
            );

            #endregion

            #region 醫事人員憑證
            bundles.Add(new ScriptBundle("~/bundles/HCAAPISVI").Include(
                        "~/HCA/HCAAPISVIAdapter.js",
                        "~/HCA/CheckAndLoad.js",
                        "~/HCA/AllComponentErrCode.js",
                        "~/HCA/HCAServiSign.js",
                         "~/HCA/HCAEvn.js",
                        "~/HCA/HCALogin.js"));
            #endregion

        }
    }
}
