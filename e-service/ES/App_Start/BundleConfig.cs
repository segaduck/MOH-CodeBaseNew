using System.Web;
using System.Web.Optimization;

namespace ES
{
    public class BundleConfig
    {
        // 如需 Bundling 的詳細資訊，請造訪 http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                            "~/Scripts/jquery-{version}.js",
                            "~/Scripts/jquery.blockUI.js",
                            "~/Scripts/jquery.hover.js",
                            "~/Scripts/global.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.custom.js",
                        "~/Scripts/jquery-ui.unobtrusive-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/timepicker").Include(
                        "~/Scripts/jquery-ui-timepicker-addon.js",
                        "~/Scripts/jquery-ui-timepicker-zh-TW.js"));

            bundles.Add(new ScriptBundle("~/bundles/ztreecore").Include(
                        "~/Scripts/jquery.ztree.core-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/ztreecheck").Include(
                        "~/Scripts/jquery.ztree.excheck-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                   "~/Scripts/bootstrap.js",
                   "~/Scripts/bootstrap-treeview.js",
                   "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/HiPKICerts").Include(
                   "~/Scripts/HiPKIErrorcode.js",
                   "~/Scripts/HiPKICerts.js"));

            bundles.Add(new ScriptBundle("~/bundles/HCAAPISVI").Include(
                         "~/HCA/HCAAPISVIAdapter.js",
                         "~/HCA/CheckAndLoad.js",
                         "~/HCA/AllComponentErrCode.js",
                         "~/HCA/HCAServiSign.js",
                          "~/HCA/HCAEvn.js",
                         "~/HCA/HCALogin.js"));


            // 使用開發版本的 Modernizr 進行開發並學習。然後，當您
            // 準備好實際執行時，請使用 http://modernizr.com 上的建置工具，只選擇您需要的測試。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/base/css").Include(
                "~/style/base.css",
                "~/style/bootstrap.css"));

            bundles.Add(new StyleBundle("~/Content/base2/css").Include(
                "~/style/base2.css",
                "~/style/bootstrap.css"));

            bundles.Add(new StyleBundle("~/Content/themes/redmond/css").Include(
                        "~/Content/themes/redmond/jquery-ui-{version}.custom.css"));

            bundles.Add(new StyleBundle("~/Content/themes/timepicker").Include(
                        "~/Content/themes/jquery-ui-timepicker-addon.css"));

            bundles.Add(new StyleBundle("~/Content/zTreeStyle/css").Include(
                        "~/Content/zTreeStyle/zTreeStyle.css"));

            bundles.Add(new StyleBundle("~/Content/font-awesome").Include(
                "~/Content/font-awesome-4.7.0.min.css"));

            bundles.Add(new StyleBundle("~/Content/fontawesome/css").Include(
                "~/style/fontawesome-free-5.15.1-web/css/all.css"));

            bundles.Add(new ScriptBundle("~/bundle/polyfills").Include(
                "~/js/polyfills/turboframe_polyfills_bundle.min.js"
                ));
            bundles.Add(new ScriptBundle("~/bundle/vender").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery-3.7.1.js",
                "~/Scripts/jquery.min.js",
                "~/Scripts/jquery.blockUI.js",
                "~/Scripts/select2.js",
                "~/Scripts/jquery-scrolltofixed.js",
                "~/Scripts/jquery-ui/ui/widgets/datepicker.js",
                "~/Scripts/jquery.maskedinput/src/jquery.maskedinput.js"
                ));
            bundles.Add(new ScriptBundle("~/bundle/components").Include(
                "~/js/components/TU.datepicker.js",
                "~/js/components/TU.select2.js"
                ));
            bundles.Add(new ScriptBundle("~/bundle/javascripts").Include(
                "~/js/javascript/Turboframe_ScrollTop.js",
                "~/js/javascript/Turboframe_ObserveEachClass.js",
                "~/js/javascript/Turboframe_ObserveSingleScroll.js",
                "~/js/javascript/Turboframe_Utils.js"
                ));
            bundles.Add(new StyleBundle("~/bundles/select2").Include(
                "~/Content/css/select2.css",
                "~/Content/themes/base/jquery-ui.css",
                "~/Content/dist/css/icon-group.css",
                "~/Content/dist/css/animate.css",
                "~/Content/dist/css/main.css"
                ));
        }
    }
}