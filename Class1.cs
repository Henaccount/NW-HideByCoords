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
            string logfile = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\HideByCoords.log";
            List<string> log = new List<string>();


            Document doc = Autodesk.Navisworks.Api.Application.ActiveDocument;
            ModelItemEnumerableCollection allmodelitems = doc.Models.CreateCollectionFromRootItems().DescendantsAndSelf;
            doc.Models.SetHidden(allmodelitems, false);


            ModelItemCollection itemsoutside = new ModelItemCollection();

            foreach (ModelItem modelItem in allmodelitems)
            {
                try
                {
                    if (modelItem.HasGeometry)
                    {
                        if (!BoxesIntersect(modelItem.BoundingBox().Min, modelItem.BoundingBox().Max, selbbmin, selbbmax))
                        {
                            itemsoutside.Add(modelItem);
                        }
                        else
                        {
                            foreach (ModelItem ancest in modelItem.AncestorsAndSelf)
                            {
                                if (ancest.DisplayName.StartsWith("/"))
                                {
                                    if (!log.Contains(ancest.DisplayName))
                                        log.Add(ancest.DisplayName + " -> " + Path.GetFileName(fileoutpath));
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
            doc.SaveFile(fileoutpath);
            File.AppendAllText(logfile, string.Join("\r\n", log));

            return 0;

        }


    }
}
#endregion