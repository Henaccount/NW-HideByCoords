//------------------------------------------------------------------
// NavisWorks Sample code
//------------------------------------------------------------------

// (C) Copyright 2009 by Autodesk Inc.

// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.

// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//------------------------------------------------------------------
//
// This is the 'HideByCoords' Sample.
//
//------------------------------------------------------------------
#region HideByCoords

//Add two new namespaces
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Plugins;
using Autodesk.Navisworks.Internal.ApiImplementation;
using System;
using System.Collections.Generic;
using System.IO;



namespace HideByCoords
{
    [PluginAttribute("HideByCoords.AHideByCoords",                   //Plugin name
                     "ADSK",                                       //4 character Developer ID or GUID
                     ToolTip = "HideByCoords.AHideByCoords tool tip",//The tooltip for the item in the ribbon
                     DisplayName = "HideByCoords")]          //Display name for the Plugin in the Ribbon

    public class AHideByCoords : AddInPlugin                 //Derives from AddInPlugin
    {
        public static bool BoxesIntersect(Point3D amin, Point3D amax, Point3D bmin, Point3D bmax)
        {
            if (amax.X < bmin.X) return false;
            if (amin.X > bmax.X) return false;
            if (amax.Y < bmin.Y) return false;
            if (amin.Y > bmax.Y) return false;
            if (amax.Z < bmin.Z) return false;
            if (amin.Z > bmax.Z) return false;
            return true;
        }

        public override int Execute(params string[] parameters)
        {
            string fileoutpath = @parameters[0];
            Point3D selbbmin = new Point3D(Convert.ToDouble(parameters[1].Replace("neg", "-")), Convert.ToDouble(parameters[2].Replace("neg", "-")), Convert.ToDouble(parameters[3].Replace("neg", "-")));
            Point3D selbbmax = new Point3D(Convert.ToDouble(parameters[4].Replace("neg", "-")), Convert.ToDouble(parameters[5].Replace("neg", "-")), Convert.ToDouble(parameters[6].Replace("neg", "-")));
            string logfile = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + Path.GetFileName(fileoutpath) + ".txt";
            List<string> log = new List<string>();


            Document doc = Autodesk.Navisworks.Api.Application.ActiveDocument;
            log.Add("units" + doc.Units);
            ModelItemEnumerableCollection allmodelitems = doc.Models.CreateCollectionFromRootItems().DescendantsAndSelf;
            doc.Models.SetHidden(allmodelitems, false);


            ModelItemCollection itemsoutside = new ModelItemCollection();

            foreach (ModelItem modelItem in allmodelitems)
            {
                try
                {
                    //if (modelItem.PropertyCategories.FindPropertyByDisplayName("Item", "Icon").Value.ToDisplayString() == "Geometry")
                    if (modelItem.HasGeometry)
                    {
                        //log.Add(modelItem.DisplayName + "###" + modelItem.BoundingBox().Min.ToString() + "###" + modelItem.BoundingBox().Max.ToString());
                        //break;

                        if (!BoxesIntersect(modelItem.BoundingBox().Min, modelItem.BoundingBox().Max, selbbmin, selbbmax))
                        {
                            //log.Add(modelItem.DisplayName + "###" + "no intersect");
                            itemsoutside.Add(modelItem);
                        }
                        else
                        {
                            foreach (ModelItem ancest in modelItem.AncestorsAndSelf)
                            {
                                if (ancest.DisplayName.StartsWith("/"))
                                {
                                    if (!log.Contains(ancest.DisplayName))
                                        log.Add(ancest.DisplayName);
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    File.AppendAllText(logfile, e.Message + "#error#" + e.StackTrace);
                }
            }

            doc.Models.SetHidden(itemsoutside, true);

            if (fileoutpath.ToLower().EndsWith(".nwd"))
            {
                doc.SaveFile(fileoutpath);
            }
            else if (fileoutpath.ToLower().EndsWith(".fbx"))
            {

                PluginRecord FBXPluginrecord = Autodesk.Navisworks.Api.Application.Plugins.FindPlugin("NativeExportPluginAdaptor_LcFbxExporterPlugin_Export.Navisworks");

                if (FBXPluginrecord != null)
                {
                    if (!FBXPluginrecord.IsLoaded)
                    {
                        FBXPluginrecord.LoadPlugin();
                    }

                    //save path of the FBX
                    string[] pa = { fileoutpath };

                    //way 1: by base class of plugin

                    //Plugin FBXplugin =
                    //           FBXPluginrecord.LoadedPlugin as Plugin;


                    //FBXplugin.GetType().InvokeMember("Execute",
                    //    System.Reflection.BindingFlags.InvokeMethod,
                    //    null, FBXplugin, pa);

                    //way 2: by specific class of export plugin

                    NativeExportPluginAdaptor FBXplugin = FBXPluginrecord.LoadedPlugin as NativeExportPluginAdaptor;

                    FBXplugin.Execute(pa);
                }
            }

            File.AppendAllText(logfile, string.Join("\r\n", log));

            return 0;

        }


    }
}
#endregion