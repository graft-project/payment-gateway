using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace Views.Shared
{
    public static class LeftMenuNavPages
    {
        public static string Index => "Index";
        public static string Dashboard => "Dashboard";
        public static string ServiceProviders => "ServiceProviders";
        public static string Merchants => "Merchants";
        public static string Stores => "Stores";
        public static string Terminals => "Terminals";
        public static string Payments => "Payments";
        public static string Currencies => "Currencies";
        public static string Cryptocurrencies => "Cryptocurrencies";
        public static string Profile => "Profile";
        public static string Statistics => "Statistics";
        public static string AdminDashboard => "AdminDashboard";
        public static string ApiKeys => "ApiKeys";
        public static string TagTerminals => "TagTerminals";
        public static string TagMerchants => "TagMerchants";
        //public static string _ => "_";


        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);
        public static string DashboardNavClass(ViewContext viewContext) => PageNavClass(viewContext, Dashboard);
        public static string ServiceProvidersNavClass(ViewContext viewContext) => PageNavClass(viewContext, ServiceProviders);
        public static string MerchantsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Merchants);
        public static string StoresNavClass(ViewContext viewContext) => PageNavClass(viewContext, Stores);
        public static string TerminalsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Terminals);
        public static string PaymentsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Payments);
        public static string CurrenciesNavClass(ViewContext viewContext) => PageNavClass(viewContext, Currencies);
        public static string CryptocurrenciesNavClass(ViewContext viewContext) => PageNavClass(viewContext, Cryptocurrencies);
        public static string ProfileNavClass(ViewContext viewContext) => PageNavClass(viewContext, Profile);
        public static string StatisticsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Statistics);
        public static string AdminDashboardNavClass(ViewContext viewContext) => PageNavClass(viewContext, AdminDashboard);
        public static string ApiKeysNavClass(ViewContext viewContext) => PageNavClass(viewContext, ApiKeys);
        public static string TagTerminalsNavClass(ViewContext viewContext) => PageNavClass(viewContext, TagTerminals);
        public static string TagMerchantsNavClass(ViewContext viewContext) => PageNavClass(viewContext, TagMerchants);
        //public static string _NavClass(ViewContext viewContext) => PageNavClass(viewContext, _);


        public static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
