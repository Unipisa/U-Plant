using System;
using UPlant.Models;

namespace UPlant.ViewComponents
{
    /// <summary>
    /// This is where you customize the navigation sidebar
    /// </summary>
    public static class ModuleHelper
    {
        public enum Module
        {
            Home,
            Tabellediamministrazione,
            Modulidiinserimento,
            Modulidiricerca,
            Strumentidisupporto

        }
        public static SidebarMenu AddHeader(string name)
        {
            return new SidebarMenu
            {
                Type = SidebarMenuType.Header,
                Name = name,
            };
        }

        public static SidebarMenu AddTree(string name, string iconClassName = "fa fa-link")
        {
            return new SidebarMenu
            {
                Type = SidebarMenuType.Tree,
                IsActive = false,
                Name = name,
                IconClassName = iconClassName,
                URLPath = "#",

            };
        }

        public static SidebarMenu AddModule(Module module
                                          , string Name
                                          , string Url
                                          , string IconClassName
                                          , Tuple<int, int, int> counter = null)
        {
            if (counter == null)
            {
                counter = Tuple.Create(0, 0, 0);
            }
            //if (System.Security.Claims.ClaimsPrincipal.Current.Identity.IsAuthenticated) { }
            switch (module)
            {
                case Module.Home:
                    return new SidebarMenu
                    {
                        Type = SidebarMenuType.Link,
                        Name = Name,
                        URLPath = Url,
                        IconClassName = IconClassName,
                        LinkCounter = counter,
                    };
                case Module.Tabellediamministrazione:
                    return new SidebarMenu
                    {
                        Type = SidebarMenuType.Link,
                        Name = Name,
                        URLPath = Url,
                        IconClassName = IconClassName,
                        LinkCounter = counter,

                    };
                case Module.Modulidiinserimento:
                    return new SidebarMenu
                    {
                        Type = SidebarMenuType.Link,
                        Name = Name,
                        URLPath = Url,
                        IconClassName = IconClassName,
                        LinkCounter = counter,

                    };
                case Module.Modulidiricerca:
                    return new SidebarMenu
                    {
                        Type = SidebarMenuType.Link,
                        Name = Name,
                        URLPath = Url,
                        IconClassName = IconClassName,
                        LinkCounter = counter,

                    };
                case Module.Strumentidisupporto:
                    return new SidebarMenu
                    {
                        Type = SidebarMenuType.Link,
                        Name = Name,
                        URLPath = Url,
                        IconClassName = IconClassName,
                        LinkCounter = counter,

                    };
                default:
                    break;
            }
            return null;
        }
    }
}