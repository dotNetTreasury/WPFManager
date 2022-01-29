﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Panuon.UI.Silver;
using Common;
using Common.Entities;
using System.IO;
using Common.Data.Local;

namespace DemoPlugin.Pages
{
    class MenuManager
    {
        public static Dictionary<string, Dictionary<BaseMenuInfo, List<MenuItemModel>>> PluginDic = new Dictionary<string, Dictionary<BaseMenuInfo, List<MenuItemModel>>>();

        public static string pageFolderName = "Pages";
        /// <summary>
        /// 导航的文件标志
        /// </summary>
        public static string MenuInfoName = "MenuInfo";
        /// <summary>
        /// 插件文件夹名称
        /// </summary>
        public static string PluginFolderName = "plugins";

        /// <summary>
        /// 初始化导航
        /// </summary>
        public static void InitMenus()
        {
            FindMenuDLLPages();
        }

        /// <summary>
        /// 查找dll中的Page所在的空间
        /// </summary>
        private static void FindMenuDLLPages()
        {
            // 当前导航信息
            Dictionary<BaseMenuInfo, List<MenuItemModel>> Dic = new Dictionary<BaseMenuInfo, List<MenuItemModel>>();

            Assembly currAssembly = Assembly.GetExecutingAssembly();
            //查找所有页面的命名空间
            var pageNsps = (from t in currAssembly.GetTypes()
                            where t.IsClass && t.Namespace != null
                            && t.Namespace != "DemoPlugin"
                            && t.Namespace.StartsWith("DemoPlugin")
                            && !t.Namespace.StartsWith("<")
                            select t.Namespace).GroupBy(c => c).ToList();

            foreach (var nsp in pageNsps)
            {
                var items = (from t in currAssembly.GetTypes()
                             where t.IsClass && t.Namespace != null
                             && t.Namespace == nsp.Key
                             select t.FullName).ToList();

                BaseMenuInfo menuInfo = null;
                if (!items.Any(c => c.EndsWith(MenuInfoName)))
                {
                    //没有导航说明文件的 不参与
                    continue;
                }

                //获取MenuIfo
                menuInfo = (BaseMenuInfo)Activator.CreateInstance(Type.GetType(items.Find(c => c.EndsWith(MenuInfoName))));

                Dic.Add(menuInfo, new List<MenuItemModel>());

                #region 填充数据

                List<BasePage> pages = new List<BasePage>();

                try
                {
                    foreach (var item in items)
                    {
                        if (item.EndsWith(MenuInfoName)) continue;

                        Type itemObj = Type.GetType(item);
                        BasePage itemPage = (BasePage)Activator.CreateInstance(itemObj);
                        if (!itemPage.IsMenu) continue;//不是导航 排除

                        itemPage.Code = $"{item.Substring(item.LastIndexOf('.') + 1)}";

                        pages.Add(itemPage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                if (pages.Count > 1)
                {
                    //导航排序
                    pages = pages.OrderBy(c => c.Order).ToList();
                }
                foreach (var itemPage in pages)
                {
                    Grid itemGrid = itemPage.Content as Grid;
                    //已经获取到页
                    MenuItemModel itemModel = new MenuItemModel();
                    itemModel.Buttons = GetButtons(itemGrid);
                    itemModel.ParentCode = menuInfo.Code;
                    itemModel.Code = itemPage.Code;
                    itemModel.PluginCode = "DemoPlugin";
                    itemModel.Name = itemPage.Title;
                    itemModel.Order = 0;
                    itemModel.Url = $"/{pageFolderName}/{menuInfo.Code}/{itemModel.Code}.xaml";

                    Dic[menuInfo].Add(itemModel);
                }

                #endregion 

            }

            if (Dic.Keys.Count > 0) PluginDic.Add("DemoPlugin", Dic);
        }

        /// <summary>
        /// 将Page中的Button元素作为权限对象
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        private static List<MenuItemButtonModel> GetButtons(Visual control)
        {
            if (control == null) return new List<MenuItemButtonModel>();

            List<MenuItemButtonModel> list = new List<MenuItemButtonModel>();

            int childCount = VisualTreeHelper.GetChildrenCount(control);

            for (int i = 0; i < childCount; i++)
            {
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(control, i);
                if (childVisual.GetType() == typeof(GroupBox))
                {
                    #region 特殊的GroupBox 

                    var groupBox = (childVisual as GroupBox);
                    //从依赖项中 读取gropubox右上角的按钮
                    var element = GroupBoxHelper.GetExtendControl(groupBox);
                    if (element != null && VisualTreeHelper.GetChildrenCount(element) > 0)
                    {
                        list.AddRange(GetButtons(element));
                    }
                    //读取groupbox的内容
                    var grid = (childVisual as GroupBox).Content as Grid;
                    list.AddRange(GetButtons(grid));

                    #endregion

                }
                else if (childVisual.GetType() == typeof(Button))
                {
                    Button button = childVisual as Button;
                    MenuItemButtonModel model = new MenuItemButtonModel();
                    model.Content = button.Content == null ? "未设置内容" : button.Content.ToString();
                    model.Name = button.Name;

                    list.Add(model);
                }
                else
                {
                    #region 常规容器

                    if (VisualTreeHelper.GetChildrenCount(childVisual) > 0)
                    {
                        list.AddRange(GetButtons(childVisual));
                    }

                    #endregion 
                }
            }

            return list;
        }

        private static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
        }
    }
}
